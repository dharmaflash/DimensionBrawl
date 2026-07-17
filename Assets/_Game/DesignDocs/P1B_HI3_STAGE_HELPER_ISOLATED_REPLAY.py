from __future__ import annotations

import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
from typing import Any


PRODUCER_RELATIVE_PATH = Path("_tools/analyze_honkai_impact_3rd_full_repos.py")
PRODUCER_SHA256 = "39daaf45913281619c054eabf71de2fde00e435f7efd0d5c3823f23a816953ea"
SNAPSHOT_DATE = "2026-06-15"
EXPECTED_INPUT_COUNT = 1509
EXPECTED_INPUT_BYTES = 456_457_979
EXPECTED_INPUT_INVENTORY_SHA256 = "3b00de9a3cc41d63c7576a1958c0d01fe098e412a2c98e43aba0b1e6d544e662"
EXPECTED_OUTPUTS = {
    "hi3-stage-table-summary.csv": {
        "sizeBytes": 295_098,
        "sha256": "d8292d42ef71a5d63b1288820475c20061526abf6f894fbf2fd0e73aba96f5e7",
        "dataRows": 1509,
    },
    "hi3-stage-row-samples.csv": {
        "sizeBytes": 4_459_588,
        "sha256": "5067a78931a114658a4026889fcb9bff91c327fa7356bb5f75f8927123e95d92",
        "dataRows": 14_855,
    },
}


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Replay only the two HI3 stage helper outputs outside the Ark tree."
    )
    parser.add_argument(
        "--ark-root",
        type=Path,
        default=Path(r"C:\Ark\SubcultureGameData"),
    )
    parser.add_argument("--output-root", type=Path, required=True)
    parser.add_argument("--result-path", type=Path)
    args = parser.parse_args()

    ark_root = args.ark_root.resolve()
    output_root = args.output_root.resolve()
    if output_root == ark_root or ark_root in output_root.parents:
        raise RuntimeError("output-root must be outside the Ark source tree")

    producer_path = ark_root / PRODUCER_RELATIVE_PATH
    actual_producer_sha256 = sha256_file(producer_path)
    if actual_producer_sha256 != PRODUCER_SHA256:
        raise RuntimeError(
            f"producer SHA-256 changed: {actual_producer_sha256}"
        )

    spec = importlib.util.spec_from_file_location("hi3_stage_helper_replay", producer_path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"cannot load producer: {producer_path}")
    producer = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(producer)

    output_game = output_root / "games" / "honkai-impact-3rd"
    producer.GAME = output_game

    def source_paths(source: dict[str, str]) -> dict[str, Path]:
        raw = (
            ark_root
            / "games"
            / "honkai-impact-3rd"
            / "raw"
            / source["slug"]
            / SNAPSHOT_DATE
        )
        files = raw / "files"
        extracted = files / "extracted_repo" / source["root_name"]
        return {
            "raw": raw,
            "files": files,
            "extracted": extracted,
            "zip": files / source["zip_name"],
        }

    producer.source_paths = source_paths
    original_find_tables = producer.find_tables
    captured_inputs: list[tuple[str, str, int, str]] = []

    def captured_find_tables(
        short: str, patterns: list[str]
    ) -> list[tuple[dict[str, str], Path, Any]]:
        rows = original_find_tables(short, patterns)
        if short == "" and patterns == ["Stage", "Monster", "Level", "MapSite"]:
            captured_inputs.clear()
            for source, path, _ in rows:
                root = source_paths(source)["extracted"]
                captured_inputs.append(
                    (
                        source["short"],
                        path.relative_to(root).as_posix(),
                        path.stat().st_size,
                        sha256_file(path).upper(),
                    )
                )
        return rows

    producer.find_tables = captured_find_tables
    counts = producer.make_stage_helpers()

    inventory_rows = sorted(captured_inputs, key=lambda row: (row[0], row[1]))
    inventory_payload = "".join(
        f"{source}\t{path}\t{size}\t{digest}\n"
        for source, path, size, digest in inventory_rows
    ).encode("utf-8")
    inventory_digest = hashlib.sha256(inventory_payload).hexdigest()
    inventory_bytes = sum(row[2] for row in inventory_rows)
    if len(inventory_rows) != EXPECTED_INPUT_COUNT:
        raise RuntimeError(f"input count changed: {len(inventory_rows)}")
    if inventory_bytes != EXPECTED_INPUT_BYTES:
        raise RuntimeError(f"input bytes changed: {inventory_bytes}")
    if inventory_digest != EXPECTED_INPUT_INVENTORY_SHA256:
        raise RuntimeError(f"input inventory digest changed: {inventory_digest}")

    output_rows = []
    for name, expected in EXPECTED_OUTPUTS.items():
        path = output_game / "enemies-stages" / name
        size_bytes = path.stat().st_size
        digest = sha256_file(path)
        data_rows = sum(1 for _ in path.open("r", encoding="utf-8")) - 1
        if size_bytes != expected["sizeBytes"]:
            raise RuntimeError(f"{name} size changed: {size_bytes}")
        if digest != expected["sha256"]:
            raise RuntimeError(f"{name} SHA-256 changed: {digest}")
        if data_rows != expected["dataRows"]:
            raise RuntimeError(f"{name} data-row count changed: {data_rows}")
        output_rows.append(
            {
                "name": name,
                "path": path.as_posix(),
                "sizeBytes": size_bytes,
                "sha256": digest,
                "dataRows": data_rows,
            }
        )

    result = {
        "status": "PASS",
        "producerPath": producer_path.as_posix(),
        "producerSha256": actual_producer_sha256,
        "inputCount": len(inventory_rows),
        "inputBytes": inventory_bytes,
        "inputInventorySha256": inventory_digest,
        "producerCounts": counts,
        "outputs": output_rows,
    }
    encoded = json.dumps(
        result,
        ensure_ascii=False,
        indent=2,
        sort_keys=True,
    ) + "\n"
    if args.result_path is not None:
        result_path = args.result_path.resolve()
        if result_path == ark_root or ark_root in result_path.parents:
            raise RuntimeError("result-path must be outside the Ark source tree")
        result_path.parent.mkdir(parents=True, exist_ok=True)
        result_path.write_text(encoded, encoding="utf-8", newline="\n")
    print(encoded, end="")


if __name__ == "__main__":
    main()
