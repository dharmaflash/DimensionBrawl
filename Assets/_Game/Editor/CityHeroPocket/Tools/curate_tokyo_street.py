#!/usr/bin/env python3
"""Build a bounded, provenance-tracked Tokyo Street asset subset.

The source package is never copied wholesale into the product project.  This
tool consumes a dependency-closure manifest produced in the isolated staging
project, copies only the listed assets, converts TGA textures to PNG while
preserving their Unity GUID/import settings, and writes a complete hash ledger.

Normal maps are downsampled in vector space and renormalized.  Other packed
maps use an exact 2x2 channel-preserving box filter.  Albedo maps selected by
the manifest remain at their authored resolution.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import shutil
import sys
import uuid
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path, PurePosixPath
from typing import Any, Iterable

import numpy as np
from PIL import Image


SCHEMA = "dimensionbrawl.city_hero_pocket.curation.v1"
SOURCE_PREFIX = PurePosixPath("Assets/Tokyo_Street")
DEFAULT_TARGET_PREFIX = PurePosixPath(
    "Assets/_Game/Art/Environment/CityHeroPocket/TokyoStreet"
)
GUID_PATTERN = re.compile(r"^guid:\s*([0-9a-fA-F]{32})\s*$", re.MULTILINE)
FOLDER_GUID_NAMESPACE = uuid.UUID("52d7d157-a91a-43ee-a79e-b68112c121ed")
FORBIDDEN_PATH_PARTS = (
    "/Other/Door.cs",
    "/Other/SimpleCameraController.cs",
    "/Other/Leaves.shader",
    "/Other/Decals.shader",
    "/Roof_Wall_04",
    "/Flowers",
    "/Scenes/",
    "/Other/Terrain.asset",
)


@dataclass(frozen=True)
class CuratedRecord:
    source_path: str
    target_path: str
    guid: str
    role: str
    transform: str
    source_sha256: str
    source_meta_sha256: str
    target_sha256: str
    target_meta_sha256: str
    source_bytes: int
    source_meta_bytes: int
    target_bytes: int
    target_meta_bytes: int
    width: int | None = None
    height: int | None = None
    target_width: int | None = None
    target_height: int | None = None
    decoded_pixel_sha256: str | None = None


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest().upper()


def sha256_bytes(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest().upper()


def normalized_asset_path(value: str) -> PurePosixPath:
    path = PurePosixPath(value.replace("\\", "/"))
    if path.is_absolute() or ".." in path.parts:
        raise ValueError(f"Unsafe asset path: {value}")
    return path


def source_to_target(
    source_path: PurePosixPath, target_prefix: PurePosixPath, is_texture: bool
) -> PurePosixPath:
    try:
        relative = source_path.relative_to(SOURCE_PREFIX)
    except ValueError as exc:
        raise ValueError(f"Asset is outside {SOURCE_PREFIX}: {source_path}") from exc
    target = target_prefix / relative
    return target.with_suffix(".png") if is_texture else target


def read_meta_guid(meta_path: Path) -> str:
    text = meta_path.read_text(encoding="utf-8-sig")
    match = GUID_PATTERN.search(text)
    if not match:
        raise ValueError(f"Unity GUID missing from {meta_path}")
    return match.group(1).lower()


def image_pixel_hash(image: Image.Image) -> str:
    return sha256_bytes(np.asarray(image).tobytes(order="C"))


def downsample_box(array: np.ndarray) -> np.ndarray:
    height, width, channels = array.shape
    if width % 2 or height % 2:
        raise ValueError(f"Expected even texture dimensions, got {width}x{height}")
    widened = array.astype(np.uint32)
    total = (
        widened[0::2, 0::2]
        + widened[0::2, 1::2]
        + widened[1::2, 0::2]
        + widened[1::2, 1::2]
    )
    return ((total + 2) // 4).astype(np.uint8).reshape(
        height // 2, width // 2, channels
    )


def downsample_normal(array: np.ndarray) -> np.ndarray:
    height, width, channels = array.shape
    if channels not in (3, 4):
        raise ValueError(f"Expected RGB/RGBA normal texture, got {channels} channels")
    if width % 2 or height % 2:
        raise ValueError(f"Expected even texture dimensions, got {width}x{height}")

    rgb = array[:, :, :3].astype(np.float64) / 127.5 - 1.0
    vector_sum = (
        rgb[0::2, 0::2]
        + rgb[0::2, 1::2]
        + rgb[1::2, 0::2]
        + rgb[1::2, 1::2]
    )
    lengths = np.linalg.norm(vector_sum, axis=2, keepdims=True)
    zero = lengths < 1.0e-12
    safe_lengths = np.where(zero, 1.0, lengths)
    normalized = vector_sum / safe_lengths
    normalized = np.where(zero, np.array([0.0, 0.0, 1.0]), normalized)
    encoded = np.floor(np.clip((normalized * 0.5 + 0.5) * 255.0, 0, 255) + 0.5)
    result = encoded.astype(np.uint8)

    if channels == 4:
        alpha = downsample_box(array[:, :, 3:4])
        result = np.concatenate((result, alpha), axis=2)
    return result


def is_normal_map(path: PurePosixPath, asset: dict[str, Any]) -> bool:
    if bool(asset.get("is_normal_map", False)):
        return True
    return path.stem.lower().endswith("_n")


def should_keep_authored_size(path: PurePosixPath, asset: dict[str, Any]) -> bool:
    policy = str(asset.get("target_policy", "")).lower()
    if policy in {
        "keep_2048",
        "keep_authored",
        "pixel_exact_2k",
        "png_2048_lossless_pixels",
    }:
        return True
    if policy in {
        "resize_1024",
        "downsample_1k",
        "png_1024_box",
        "png_1024_box_normal_vector_renormalized",
    }:
        return False
    # Rich-facade manifests may omit the policy.  Keep authored albedo/detail
    # color maps and downsample all normal/packed/support maps.
    return path.stem.lower().endswith("_a")


def convert_texture(
    source_path: Path,
    target_path: Path,
    source_asset_path: PurePosixPath,
    asset: dict[str, Any],
) -> tuple[str, int, int, int, int, str]:
    with Image.open(source_path) as opened:
        mode = "RGBA" if "A" in opened.getbands() else "RGB"
        authored = opened.convert(mode)
        source_width, source_height = authored.size
        authored_pixels = np.asarray(authored)

    if should_keep_authored_size(source_asset_path, asset):
        transformed = authored_pixels.copy()
        transform = "tga_to_png_pixel_exact"
    elif is_normal_map(source_asset_path, asset):
        transformed = downsample_normal(authored_pixels)
        transform = "tga_to_png_2x2_normal_renormalized"
    else:
        transformed = downsample_box(authored_pixels)
        transform = "tga_to_png_2x2_channel_preserving"

    output_mode = "RGBA" if transformed.shape[2] == 4 else "RGB"
    target_image = Image.fromarray(transformed, mode=output_mode)
    target_path.parent.mkdir(parents=True, exist_ok=True)
    target_image.save(target_path, format="PNG", compress_level=9, optimize=False)

    expected_hash = image_pixel_hash(target_image)
    with Image.open(target_path) as decoded:
        decoded_hash = image_pixel_hash(decoded.convert(output_mode))
    if decoded_hash != expected_hash:
        raise ValueError(f"PNG decoded pixels differ from authored target: {target_path}")

    return (
        transform,
        source_width,
        source_height,
        target_image.width,
        target_image.height,
        decoded_hash,
    )


def iter_existing_guids(root: Path) -> Iterable[tuple[str, Path]]:
    if not root.exists():
        return
    for meta_path in root.rglob("*.meta"):
        try:
            yield read_meta_guid(meta_path), meta_path
        except (OSError, UnicodeError, ValueError):
            continue


def deterministic_folder_guid(asset_path: PurePosixPath, occupied: set[str]) -> str:
    attempt = 0
    while True:
        key = asset_path.as_posix() if attempt == 0 else f"{asset_path.as_posix()}#{attempt}"
        candidate = uuid.uuid5(FOLDER_GUID_NAMESPACE, key).hex
        if candidate not in occupied:
            occupied.add(candidate)
            return candidate
        attempt += 1


def write_folder_meta(path: Path, guid: str) -> None:
    path.write_text(
        "\n".join(
            (
                "fileFormatVersion: 2",
                f"guid: {guid}",
                "folderAsset: yes",
                "DefaultImporter:",
                "  externalObjects: {}",
                "  userData: ",
                "  assetBundleName: ",
                "  assetBundleVariant: ",
                "",
            )
        ),
        encoding="utf-8",
        newline="\n",
    )


def create_folder_metas(
    output_root: Path,
    target_prefix: PurePosixPath,
    occupied: set[str],
) -> list[dict[str, str]]:
    target_root = output_root.joinpath(*target_prefix.parts)
    boundary_name = "CityHeroPocket"
    boundary_index = target_prefix.parts.index(boundary_name)
    boundary = output_root.joinpath(*target_prefix.parts[: boundary_index + 1])
    folders = sorted(
        (path for path in boundary.rglob("*") if path.is_dir()),
        key=lambda value: (len(value.parts), value.as_posix().lower()),
    )
    folders.insert(0, boundary)
    records: list[dict[str, str]] = []
    for folder in folders:
        meta_path = folder.with_name(folder.name + ".meta")
        if meta_path.exists():
            guid = read_meta_guid(meta_path)
        else:
            asset_path = PurePosixPath(folder.relative_to(output_root).as_posix())
            guid = deterministic_folder_guid(asset_path, occupied)
            write_folder_meta(meta_path, guid)
        records.append(
            {
                "path": folder.relative_to(output_root).as_posix(),
                "meta_path": meta_path.relative_to(output_root).as_posix(),
                "guid": guid,
                "meta_sha256": sha256_file(meta_path),
            }
        )
    if not target_root.exists():
        raise ValueError(f"Curated target root was not created: {target_root}")
    return records


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", required=True, type=Path)
    parser.add_argument("--source-project-root", required=True, type=Path)
    parser.add_argument("--output-root", required=True, type=Path)
    parser.add_argument(
        "--target-prefix",
        default=DEFAULT_TARGET_PREFIX.as_posix(),
        help="Asset path below output-root; must include CityHeroPocket",
    )
    parser.add_argument(
        "--guid-scan-root",
        type=Path,
        help="Existing Unity Assets tree whose GUIDs must not collide",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    manifest_path = args.manifest.resolve()
    source_root = args.source_project_root.resolve()
    output_root = args.output_root.resolve()
    target_prefix = normalized_asset_path(args.target_prefix)

    if "CityHeroPocket" not in target_prefix.parts:
        raise ValueError("target-prefix must include a CityHeroPocket boundary")
    if output_root.exists() and any(output_root.iterdir()):
        raise FileExistsError(f"Refusing to overwrite non-empty output: {output_root}")
    output_root.mkdir(parents=True, exist_ok=True)

    manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    assets = manifest.get("assets")
    if not isinstance(assets, list) or not assets:
        raise ValueError("Manifest must contain a non-empty assets array")

    manifest_guids: dict[str, str] = {}
    normalized_assets: list[tuple[dict[str, Any], PurePosixPath, str]] = []
    for asset in assets:
        source_asset = normalized_asset_path(str(asset["path"]))
        normalized = "/" + source_asset.as_posix()
        forbidden = next((item for item in FORBIDDEN_PATH_PARTS if item.lower() in normalized.lower()), None)
        if forbidden:
            raise ValueError(f"Forbidden asset in closure ({forbidden}): {source_asset}")
        guid = str(asset.get("guid", "")).lower()
        if not re.fullmatch(r"[0-9a-f]{32}", guid):
            raise ValueError(f"Invalid manifest GUID for {source_asset}: {guid}")
        prior = manifest_guids.get(guid)
        if prior and prior != source_asset.as_posix():
            raise ValueError(f"Duplicate manifest GUID {guid}: {prior}, {source_asset}")
        manifest_guids[guid] = source_asset.as_posix()
        role = str(asset.get("role") or "").lower()
        normalized_assets.append((asset, source_asset, role))

    occupied = set(manifest_guids)
    collisions: list[dict[str, str]] = []
    if args.guid_scan_root:
        for guid, meta_path in iter_existing_guids(args.guid_scan_root.resolve()):
            occupied.add(guid)
            if guid in manifest_guids:
                collisions.append(
                    {
                        "guid": guid,
                        "source": manifest_guids[guid],
                        "existing_meta": str(meta_path),
                    }
                )
    if collisions:
        raise ValueError(
            "Curated source GUIDs already exist in the target project:\n"
            + json.dumps(collisions, indent=2, ensure_ascii=False)
        )

    records: list[CuratedRecord] = []
    target_guids: set[str] = set()
    for asset, source_asset, role in sorted(
        normalized_assets, key=lambda item: item[1].as_posix().lower()
    ):
        source_path = source_root.joinpath(*source_asset.parts)
        source_meta = source_path.with_name(source_path.name + ".meta")
        if not source_path.is_file() or not source_meta.is_file():
            raise FileNotFoundError(f"Missing source asset/meta pair: {source_asset}")
        actual_guid = read_meta_guid(source_meta)
        manifest_guid = str(asset["guid"]).lower()
        if actual_guid != manifest_guid:
            raise ValueError(
                f"Manifest/meta GUID mismatch for {source_asset}: "
                f"{manifest_guid} != {actual_guid}"
            )
        if actual_guid in target_guids:
            raise ValueError(f"Duplicate output GUID: {actual_guid}")
        target_guids.add(actual_guid)

        declared_source_hash = str(asset.get("source_sha256") or "").upper()
        declared_meta_hash = str(
            asset.get("meta_sha256") or asset.get("source_meta_sha256") or ""
        ).upper()
        actual_source_hash = sha256_file(source_path)
        actual_meta_hash = sha256_file(source_meta)
        if declared_source_hash and declared_source_hash != actual_source_hash:
            raise ValueError(f"Manifest source hash mismatch: {source_asset}")
        if declared_meta_hash and declared_meta_hash != actual_meta_hash:
            raise ValueError(f"Manifest meta hash mismatch: {source_asset}")

        is_texture = role == "texture" or source_asset.suffix.lower() == ".tga"
        target_asset = source_to_target(source_asset, target_prefix, is_texture)
        target_path = output_root.joinpath(*target_asset.parts)
        target_meta = target_path.with_name(target_path.name + ".meta")
        target_path.parent.mkdir(parents=True, exist_ok=True)

        width = height = target_width = target_height = None
        decoded_hash = None
        if is_texture:
            (
                transform,
                width,
                height,
                target_width,
                target_height,
                decoded_hash,
            ) = convert_texture(source_path, target_path, source_asset, asset)
            declared_width = asset.get("target_width")
            declared_height = asset.get("target_height")
            if declared_width not in (None, "") and int(declared_width) != target_width:
                raise ValueError(
                    f"Manifest target width mismatch for {source_asset}: "
                    f"{declared_width} != {target_width}"
                )
            if declared_height not in (None, "") and int(declared_height) != target_height:
                raise ValueError(
                    f"Manifest target height mismatch for {source_asset}: "
                    f"{declared_height} != {target_height}"
                )
        else:
            shutil.copy2(source_path, target_path)
            transform = "copy_exact"
            if sha256_file(source_path) != sha256_file(target_path):
                raise ValueError(f"Copied asset hash mismatch: {source_asset}")

        shutil.copy2(source_meta, target_meta)
        if read_meta_guid(target_meta) != actual_guid:
            raise ValueError(f"Copied meta GUID mismatch: {target_meta}")

        records.append(
            CuratedRecord(
                source_path=source_asset.as_posix(),
                target_path=target_asset.as_posix(),
                guid=actual_guid,
                role="texture" if is_texture else role,
                transform=transform,
                source_sha256=actual_source_hash,
                source_meta_sha256=actual_meta_hash,
                target_sha256=sha256_file(target_path),
                target_meta_sha256=sha256_file(target_meta),
                source_bytes=source_path.stat().st_size,
                source_meta_bytes=source_meta.stat().st_size,
                target_bytes=target_path.stat().st_size,
                target_meta_bytes=target_meta.stat().st_size,
                width=width,
                height=height,
                target_width=target_width,
                target_height=target_height,
                decoded_pixel_sha256=decoded_hash,
            )
        )

    folder_records = create_folder_metas(output_root, target_prefix, occupied)
    total_source_bytes = sum(record.source_bytes for record in records)
    total_source_meta_bytes = sum(record.source_meta_bytes for record in records)
    total_target_bytes = sum(record.target_bytes for record in records)
    total_target_meta_bytes = sum(record.target_meta_bytes for record in records)
    texture_records = [record for record in records if record.role == "texture"]
    report = {
        "schema": SCHEMA,
        "generated_at_utc": datetime.now(timezone.utc).isoformat(),
        "source_manifest": {
            "path": str(manifest_path),
            "sha256": sha256_file(manifest_path),
        },
        "source_manifest_metadata": {
            key: value for key, value in manifest.items() if key != "assets"
        },
        "target_prefix": target_prefix.as_posix(),
        "summary": {
            "asset_count": len(records),
            "texture_count": len(texture_records),
            "folder_meta_count": len(folder_records),
            "source_asset_bytes": total_source_bytes,
            "source_meta_bytes": total_source_meta_bytes,
            "source_asset_plus_meta_bytes": total_source_bytes + total_source_meta_bytes,
            "target_asset_bytes": total_target_bytes,
            "target_meta_bytes": total_target_meta_bytes,
            "target_asset_plus_meta_bytes": total_target_bytes + total_target_meta_bytes,
            "asset_byte_reduction_percent": round(
                (1.0 - total_target_bytes / float(total_source_bytes)) * 100.0, 4
            ),
            "guid_collision_count": 0,
            "forbidden_asset_count": 0,
            "png_roundtrip_failure_count": 0,
        },
        "assets": [record.__dict__ for record in records],
        "folders": folder_records,
    }

    report_path = output_root / "CITY_HERO_POCKET_TOKYO_STREET_CURATION.json"
    report_path.write_text(
        json.dumps(report, indent=2, ensure_ascii=False) + "\n",
        encoding="utf-8",
        newline="\n",
    )

    csv_path = output_root / "CITY_HERO_POCKET_TOKYO_STREET_CURATION.csv"
    with csv_path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(CuratedRecord.__dataclass_fields__))
        writer.writeheader()
        for record in records:
            writer.writerow(record.__dict__)

    hash_paths = sorted(
        path
        for path in output_root.rglob("*")
        if path.is_file() and path.name != "SHA256SUMS"
    )
    sums_path = output_root / "SHA256SUMS"
    sums_path.write_text(
        "".join(
            f"{sha256_file(path)}  {path.relative_to(output_root).as_posix()}\n"
            for path in hash_paths
        ),
        encoding="utf-8",
        newline="\n",
    )

    print(json.dumps(report["summary"], indent=2))
    print(f"Curated output: {output_root}")
    print(f"Report SHA256: {sha256_file(report_path)}")
    print(f"SHA256SUMS SHA256: {sha256_file(sums_path)}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as error:  # fail closed with a useful CI/batch diagnostic
        print(f"ERROR: {error}", file=sys.stderr)
        raise
