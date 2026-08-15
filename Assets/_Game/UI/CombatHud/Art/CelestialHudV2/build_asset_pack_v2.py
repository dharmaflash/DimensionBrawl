"""Build the Celestial HUD V2 component pack deterministically.

The production outputs are deliberately split at runtime-state boundaries:
plates, glyphs, portraits, masks, rails, fills, and reticle parts are separate.
No labels, numerals, cooldown values, or other runtime text are baked.

ImageGen is used only as a source for the action plate micro-surface and action
glyph silhouettes.  Those chroma sources are keyed, component-cleaned,
recoloured to the V22 palette, fitted to exact occupancy targets, and
downsampled.  Every other UI geometry is authored here at 4x supersampling.

Run from the repository Assets directory with the bundled Python environment:

    uv run --with pillow --with numpy python \
      _Game/UI/CombatHud/Art/CelestialHudV2/build_asset_pack_v2.py --force
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import shutil
from collections import deque
from pathlib import Path
from typing import Callable, Iterable

import numpy as np
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parent
RUNTIME = ROOT / "Runtime"
MOTION = ROOT / "Motion"
QA = ROOT / "QA"
IMAGEGEN = ROOT / "Source" / "ImageGen"
ASSETS = ROOT.parents[4]
LEGACY = ROOT.parent / "CelestialHud"

SS = 4

CLEAR = (0, 0, 0, 0)
PANEL_BASE = (11, 17, 24, 189)
PANEL_INNER = (23, 29, 37, 219)
PANEL_RAISED = (31, 38, 48, 230)
OUTER_KEY = (5, 8, 13, 245)
GUNMETAL = (74, 82, 92, 242)
GUNMETAL_DARK = (41, 47, 56, 235)
PEARL_RIM = (216, 217, 211, 235)
ICON_PEARL = (241, 239, 232, 247)
READY_CYAN = (57, 214, 232, 247)
READY_CYAN_DIM = (57, 214, 232, 115)
ENEMY_RED = (226, 76, 91, 255)
CHAMPAGNE = (209, 174, 103, 255)
WARNING_AMBER = (240, 138, 60, 255)
COOLDOWN_MASK = (3, 6, 9, 158)

RUNTIME_SPECS: dict[str, tuple[tuple[int, int], str, str]] = {
    "objective_frame.png": ((960, 199), "Core", "Open-left mission ribbon; runtime text only."),
    "boss_name_tab.png": ((512, 72), "Core", "Boss name tab; runtime name only."),
    "boss_hp_track.png": ((1024, 80), "Core", "Boss HP rail with transparent aperture."),
    "boss_hp_fill.png": ((1024, 24), "Core", "Boss HP horizontal fill."),
    "boss_cost_track.png": ((1024, 48), "Core", "Boss cost rail with transparent aperture."),
    "boss_cost_fill.png": ((1024, 18), "Core", "Boss cost horizontal fill."),
    "pause_plate.png": ((160, 160), "Core", "System button plate without glyph."),
    "glyph_pause.png": ((160, 160), "Glyph", "Pause glyph only."),
    "action_plate.png": ((256, 256), "Core", "Shared secondary action plate."),
    "action_ready_arc.png": ((256, 256), "Core", "Shared 72 degree ready arc."),
    "action_cooldown_disc.png": ((256, 256), "Core", "Shared radial cooldown mask source."),
    "glyph_weapon_swap.png": ((256, 256), "Glyph", "Weapon swap glyph only."),
    "glyph_ultimate.png": ((256, 256), "Glyph", "Ultimate glyph only."),
    "glyph_dash.png": ((256, 256), "Glyph", "Dash glyph only."),
    "glyph_attack_ranged.png": ((320, 320), "Glyph", "Ranged attack glyph only."),
    "summon_mask.png": ((320, 344), "Core", "Shared summon portrait aperture mask."),
    "summon_frame_s1.png": ((320, 344), "Core", "Primary summon frame."),
    "summon_frame_s2.png": ((288, 316), "Core", "Secondary summon frame, slot 2."),
    "summon_frame_s3.png": ((288, 316), "Core", "Secondary summon frame, slot 3."),
    "summon_state_arc.png": ((320, 344), "Core", "Shared summon 72 degree state arc."),
    "summon_cost_tab.png": ((92, 54), "Core", "Summon cost tab; runtime number only."),
    "summon_portrait_s1.png": ((600, 600), "Portrait", "Identity-preserved project summon portrait 1."),
    "summon_portrait_s2.png": ((600, 600), "Portrait", "Identity-preserved project summon portrait 2."),
    "summon_portrait_s3.png": ((600, 600), "Portrait", "Identity-preserved project summon portrait 3."),
    "player_portrait_frame.png": ((200, 200), "Core", "Player portrait socket frame."),
    "player_portrait_mask.png": ((200, 200), "Core", "Player portrait aperture mask."),
    "player_portrait.png": ((512, 512), "Portrait", "Identity-preserved project player portrait."),
    "player_hp_track.png": ((1024, 56), "Core", "Player HP rail."),
    "player_hp_fill.png": ((1024, 24), "Core", "Player HP horizontal fill."),
    "player_en_track.png": ((1024, 44), "Core", "Player EN rail."),
    "player_en_fill.png": ((1024, 20), "Core", "Player EN horizontal fill."),
    "player_ammo_plate.png": ((256, 144), "Core", "Ammo/status plate; runtime content only."),
    "glyph_bullet.png": ((128, 128), "Glyph", "Ammo glyph only."),
    "glyph_mode_ranged.png": ((128, 128), "Glyph", "Ranged mode glyph only."),
    "joystick_base.png": ((320, 320), "Core", "Virtual joystick base."),
    "joystick_knob.png": ((128, 128), "Core", "Virtual joystick knob."),
    "reticle_needle.png": ((192, 192), "Core", "One left precision needle on a centered canvas."),
    "reticle_dot.png": ((192, 192), "Core", "Precision reticle center point."),
}

ACTION_SOURCE_NAMES = {
    "glyph_weapon_swap.png": "glyph_weapon_swap_chroma.png",
    "glyph_ultimate.png": "glyph_ultimate_chroma.png",
    "glyph_dash.png": "glyph_dash_chroma.png",
    "glyph_attack_ranged.png": "glyph_attack_ranged_chroma.png",
}

ACTION_LEGACY_NAMES = {
    "glyph_weapon_swap.png": "action_weapon_swap.png",
    "glyph_ultimate.png": "action_ultimate.png",
    "glyph_dash.png": "action_dash.png",
    "glyph_attack_ranged.png": "action_attack_ranged.png",
}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--force",
        action="store_true",
        help="Overwrite an existing generated V2 pack.",
    )
    return parser.parse_args()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def load_rgba(path: Path) -> Image.Image:
    if not path.exists():
        raise FileNotFoundError(path)
    with Image.open(path) as image:
        return image.convert("RGBA")


def zero_transparent_rgb(image: Image.Image, alpha_threshold: int = 0) -> Image.Image:
    data = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    if alpha_threshold > 0:
        data[data[..., 3] <= alpha_threshold] = 0
    else:
        data[data[..., 3] == 0, :3] = 0
    return Image.fromarray(data, "RGBA")


def alpha_bbox(image: Image.Image, threshold: int = 1) -> tuple[int, int, int, int] | None:
    alpha = image.convert("RGBA").getchannel("A")
    if threshold > 1:
        alpha = alpha.point(lambda value: 255 if value >= threshold else 0)
    return alpha.getbbox()


def scale_points(points: Iterable[tuple[float, float]], factor: int = SS) -> list[tuple[int, int]]:
    return [(round(x * factor), round(y * factor)) for x, y in points]


def supersample(
    size: tuple[int, int],
    painter: Callable[[Image.Image, ImageDraw.ImageDraw, int], None],
) -> Image.Image:
    large = Image.new("RGBA", (size[0] * SS, size[1] * SS), CLEAR)
    painter(large, ImageDraw.Draw(large, "RGBA"), SS)
    result = large.resize(size, Image.Resampling.LANCZOS)
    return zero_transparent_rgb(result, alpha_threshold=3)


def inset_points(
    points: Iterable[tuple[float, float]],
    amount_x: float,
    amount_y: float | None = None,
) -> list[tuple[float, float]]:
    points = list(points)
    amount_y = amount_x if amount_y is None else amount_y
    xs = [point[0] for point in points]
    ys = [point[1] for point in points]
    left, right = min(xs), max(xs)
    top, bottom = min(ys), max(ys)
    center_x = (left + right) * 0.5
    center_y = (top + bottom) * 0.5
    scale_x = max(0.01, (right - left - amount_x * 2.0) / max(1.0, right - left))
    scale_y = max(0.01, (bottom - top - amount_y * 2.0) / max(1.0, bottom - top))
    return [
        (center_x + (x - center_x) * scale_x, center_y + (y - center_y) * scale_y)
        for x, y in points
    ]


def smoothstep01(value: np.ndarray) -> np.ndarray:
    value = np.clip(value, 0.0, 1.0)
    return value * value * (3.0 - 2.0 * value)


def remove_green_chroma(image: Image.Image) -> Image.Image:
    """Remove the flat green source while suppressing green antialias fringe."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.float32)
    rgb = rgba[..., :3]
    height, width = rgb.shape[:2]
    border = max(4, min(height, width) // 80)
    border_pixels = np.concatenate(
        (
            rgb[:border].reshape(-1, 3),
            rgb[-border:].reshape(-1, 3),
            rgb[:, :border].reshape(-1, 3),
            rgb[:, -border:].reshape(-1, 3),
        ),
        axis=0,
    )
    key = np.median(border_pixels, axis=0)
    distance = np.linalg.norm(rgb - key[None, None, :], axis=2)
    green_dominance = rgb[..., 1] - np.maximum(rgb[..., 0], rgb[..., 2])
    distance_alpha = smoothstep01((distance - 18.0) / 92.0)
    dominance_alpha = smoothstep01((145.0 - green_dominance) / 92.0)
    alpha = distance_alpha * dominance_alpha
    alpha[alpha < 0.035] = 0.0
    alpha[alpha > 0.97] = 1.0

    # Despill only retained pixels. Final glyph recolouring removes the last
    # source hue; this keeps the plate source clean before microdetail sampling.
    retained = alpha > 0.0
    neutral_ceiling = np.maximum(rgb[..., 0], rgb[..., 2]) + 10.0
    rgb[..., 1] = np.where(retained, np.minimum(rgb[..., 1], neutral_ceiling), rgb[..., 1])

    output = np.zeros((height, width, 4), dtype=np.uint8)
    output[..., :3] = np.clip(np.rint(rgb), 0, 255).astype(np.uint8)
    output[..., 3] = np.clip(np.rint(alpha * 255.0), 0, 255).astype(np.uint8)
    return zero_transparent_rgb(Image.fromarray(output, "RGBA"), alpha_threshold=3)


def trim_and_fit(
    image: Image.Image,
    size: tuple[int, int],
    target_bbox: tuple[int, int],
    threshold: int = 12,
) -> Image.Image:
    bbox = alpha_bbox(image, threshold)
    if bbox is None:
        raise RuntimeError("Refusing to fit an empty image")
    cropped = image.crop(bbox)
    scale = min(target_bbox[0] / cropped.width, target_bbox[1] / cropped.height)
    resized = cropped.resize(
        (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale))),
        Image.Resampling.LANCZOS,
    )
    canvas = Image.new("RGBA", size, CLEAR)
    canvas.alpha_composite(
        resized,
        ((size[0] - resized.width) // 2, (size[1] - resized.height) // 2),
    )
    return zero_transparent_rgb(canvas, alpha_threshold=3)


def keep_largest_components(image: Image.Image, count: int) -> Image.Image:
    """Keep semantic components and discard detached ImageGen particles."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8)
    binary = rgba[..., 3] >= 48
    height, width = binary.shape
    visited = np.zeros_like(binary, dtype=bool)
    components: list[list[tuple[int, int]]] = []
    for y in range(height):
        for x in range(width):
            if not binary[y, x] or visited[y, x]:
                continue
            queue: deque[tuple[int, int]] = deque([(x, y)])
            visited[y, x] = True
            component: list[tuple[int, int]] = []
            while queue:
                px, py = queue.popleft()
                component.append((px, py))
                for nx, ny in ((px - 1, py), (px + 1, py), (px, py - 1), (px, py + 1)):
                    if 0 <= nx < width and 0 <= ny < height and binary[ny, nx] and not visited[ny, nx]:
                        visited[ny, nx] = True
                        queue.append((nx, ny))
            components.append(component)

    components.sort(key=len, reverse=True)
    keep = np.zeros_like(binary, dtype=np.uint8)
    for component in components[:count]:
        for x, y in component:
            keep[y, x] = 255
    keep_image = Image.fromarray(keep, "L").filter(ImageFilter.MaxFilter(3))
    keep_array = np.asarray(keep_image, dtype=np.uint8)
    output = rgba.copy()
    output[keep_array == 0] = 0
    return zero_transparent_rgb(Image.fromarray(output, "RGBA"), alpha_threshold=3)


def recolour_action_glyph(image: Image.Image) -> Image.Image:
    source = np.asarray(image.convert("RGBA"), dtype=np.uint8)
    rgb = source[..., :3].astype(np.int16)
    alpha = source[..., 3]
    output = np.zeros_like(source)
    output[..., 3] = alpha

    # Pearl is the base. Preserve at most the meaningful ImageGen inset cuts:
    # desaturated blue-grey becomes gunmetal and the single warm ultimate core
    # becomes champagne. Chroma-key green never survives this mapping.
    output[..., :3] = np.array(ICON_PEARL[:3], dtype=np.uint8)
    red, green, blue = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    warm = (red - blue > 28) & (green - blue > 12) & (alpha > 12)
    cool_inset = (blue - red > 4) & (green - red > 4) & (alpha > 12)
    output[warm, :3] = np.array(CHAMPAGNE[:3], dtype=np.uint8)
    output[cool_inset, :3] = np.array(GUNMETAL[:3], dtype=np.uint8)
    output[alpha == 0, :3] = 0
    return zero_transparent_rgb(Image.fromarray(output, "RGBA"), alpha_threshold=3)


def extract_legacy_glyph(path: Path) -> Image.Image:
    """Deterministic fallback: separate the bright glyph from legacy button art."""
    image = load_rgba(path)
    data = np.asarray(image, dtype=np.uint8)
    rgb = data[..., :3].astype(np.float32)
    alpha = data[..., 3]
    luminance = rgb[..., 0] * 0.299 + rgb[..., 1] * 0.587 + rgb[..., 2] * 0.114
    yy, xx = np.mgrid[0 : image.height, 0 : image.width]
    cx, cy = image.width * 0.5, image.height * 0.5
    radial = np.sqrt(((xx - cx) / image.width) ** 2 + ((yy - cy) / image.height) ** 2)
    mask = (alpha > 16) & (luminance > 164) & (radial < 0.39)
    output = np.zeros_like(data)
    output[..., :3] = np.array(ICON_PEARL[:3], dtype=np.uint8)
    output[..., 3] = np.where(mask, alpha, 0).astype(np.uint8)
    output[output[..., 3] == 0, :3] = 0
    return zero_transparent_rgb(Image.fromarray(output, "RGBA"), alpha_threshold=3)


def add_micrograin(
    image: Image.Image,
    source: Image.Image | None,
    strength: float = 3.0,
    interior_radius: float | None = None,
) -> Image.Image:
    if source is None:
        return image
    source_gray = ImageOps.grayscale(source.convert("RGB")).resize(image.size, Image.Resampling.LANCZOS)
    blurred = source_gray.filter(ImageFilter.GaussianBlur(radius=max(2.0, min(image.size) / 42.0)))
    detail = np.asarray(source_gray, dtype=np.float32) - np.asarray(blurred, dtype=np.float32)
    deviation = float(np.percentile(np.abs(detail), 92.0))
    if deviation <= 0.001:
        return image
    detail = np.clip(detail / deviation, -1.0, 1.0) * strength

    data = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    mask = data[..., 3] > 0
    if interior_radius is not None:
        yy, xx = np.mgrid[0 : image.height, 0 : image.width]
        cx, cy = image.width * 0.5, image.height * 0.5
        mask &= (xx - cx) ** 2 + (yy - cy) ** 2 <= interior_radius**2
    adjusted = data[..., :3].astype(np.float32)
    adjusted[mask] += detail[mask, None]
    data[..., :3] = np.clip(np.rint(adjusted), 0, 255).astype(np.uint8)
    return zero_transparent_rgb(Image.fromarray(data, "RGBA"))


def draw_objective_frame() -> Image.Image:
    size = RUNTIME_SPECS["objective_frame.png"][0]

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        # PGR-like information strip: one translucent core, a screen-left bleed,
        # restrained broad facets, and one clipped right edge. There is no
        # complete perimeter/pearl border, so it cannot read as an empty card.
        core = [(-12, 51), (907, 51), (958, 80), (925, 148), (-12, 148)]
        draw.polygon(scale_points(core, s), fill=(11, 17, 24, 178))
        draw.polygon(
            scale_points([(-12, 51), (907, 51), (936, 68), (612, 64), (422, 72), (205, 62), (-12, 70)], s),
            fill=(74, 82, 92, 44),
        )
        draw.polygon(
            scale_points([(-12, 132), (188, 142), (392, 132), (610, 144), (925, 132), (925, 148), (-12, 148)], s),
            fill=(5, 8, 13, 62),
        )
        # Four low-alpha top/bottom planes establish direction without a
        # repeated sawtooth silhouette.
        facet_width = 192
        for index in range(4):
            left = -24 + index * facet_width
            draw.polygon(
                scale_points([(left, 51), (left + 96, 20), (left + 192, 51)], s),
                fill=(170, 178, 188, 28 if index % 2 == 0 else 18),
            )
            draw.polygon(
                scale_points([(left + 28, 148), (left + 124, 179), (left + 220, 148)], s),
                fill=(5, 8, 13, 48 if index % 2 == 0 else 34),
            )

    result = supersample(size, paint)
    micro_path = IMAGEGEN / "panel_microtexture.png"
    return add_micrograin(result, load_rgba(micro_path) if micro_path.exists() else None, 2.0)


def draw_boss_name_tab() -> Image.Image:
    size = RUNTIME_SPECS["boss_name_tab.png"][0]

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        outer = [(10, 5), (455, 5), (503, 26), (478, 67), (24, 67), (2, 45)]
        metal = inset_points(outer, 5, 5)
        pearl = inset_points(outer, 11, 10)
        body = inset_points(outer, 13, 12)
        draw.polygon(scale_points(outer, s), fill=OUTER_KEY)
        draw.polygon(scale_points(metal, s), fill=GUNMETAL)
        draw.polygon(scale_points(pearl, s), fill=PEARL_RIM)
        draw.polygon(scale_points(body, s), fill=PANEL_INNER)
        draw.polygon(scale_points([(26, 12), (441, 12), (458, 18), (24, 18)], s), fill=(216, 217, 211, 56))

    return supersample(size, paint)


def meter_polygon(width: int, height: int, chamfer: int) -> list[tuple[float, float]]:
    return [
        (chamfer, 1),
        (width - chamfer - 1, 1),
        (width - 1, height * 0.5),
        (width - chamfer - 1, height - 1),
        (chamfer, height - 1),
        (1, height * 0.5),
    ]


def draw_meter_track(
    size: tuple[int, int],
    opening: tuple[int, int],
    chamfer: int,
    outer_width: int,
    metal_width: int,
    pearl_width: int,
) -> Image.Image:
    width, height = size

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        outer = meter_polygon(width, height, chamfer)
        metal = inset_points(outer, outer_width, outer_width)
        pearl = inset_points(metal, metal_width, metal_width)
        inner = inset_points(pearl, pearl_width, pearl_width)
        draw.polygon(scale_points(outer, s), fill=OUTER_KEY)
        draw.polygon(scale_points(metal, s), fill=GUNMETAL_DARK)
        draw.polygon(scale_points(pearl, s), fill=PEARL_RIM)
        draw.polygon(scale_points(inner, s), fill=PANEL_BASE)
        top, bottom = opening
        opening_poly = [
            (chamfer + 8, top),
            (width - chamfer - 8, top),
            (width - 4, (top + bottom) * 0.5),
            (width - chamfer - 8, bottom),
            (chamfer + 8, bottom),
            (4, (top + bottom) * 0.5),
        ]
        draw.polygon(scale_points(opening_poly, s), fill=CLEAR)

    return supersample(size, paint)


def gradient_fill(
    size: tuple[int, int],
    points: list[tuple[float, float]],
    base_color: tuple[int, int, int, int],
) -> Image.Image:
    width, height = size
    mask_large = Image.new("L", (width * SS, height * SS), 0)
    ImageDraw.Draw(mask_large).polygon(scale_points(points), fill=255)
    mask = mask_large.resize(size, Image.Resampling.LANCZOS)
    x = np.linspace(-1.0, 1.0, width, dtype=np.float32)[None, :, None]
    rgb = np.array(base_color[:3], dtype=np.float32)[None, None, :]
    modulation = 1.0 + 0.035 * (1.0 - x) - 0.025 * x
    color = np.broadcast_to(np.clip(rgb * modulation, 0, 255), (height, width, 3)).copy()
    output = np.zeros((height, width, 4), dtype=np.uint8)
    output[..., :3] = np.rint(color).astype(np.uint8)
    output[..., 3] = np.asarray(mask, dtype=np.uint8)
    return zero_transparent_rgb(Image.fromarray(output, "RGBA"), alpha_threshold=3)


def draw_pause_plate() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        boxes = [
            ((10, 10, 150, 150), 18, OUTER_KEY),
            ((16, 16, 144, 144), 13, GUNMETAL),
            ((21, 21, 139, 139), 10, PEARL_RIM),
            ((23, 23, 137, 137), 9, PANEL_INNER),
        ]
        for box, radius, color in boxes:
            draw.rounded_rectangle(tuple(round(value * s) for value in box), radius=round(radius * s), fill=color)

    result = supersample((160, 160), paint)
    micro_path = IMAGEGEN / "panel_microtexture.png"
    return add_micrograin(result, load_rgba(micro_path) if micro_path.exists() else None, 2.0)


def draw_pause_glyph() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        for left in (55, 91):
            draw.rounded_rectangle(
                (left * s, 51 * s, (left + 14) * s, 109 * s),
                radius=5 * s,
                fill=ICON_PEARL,
            )

    return supersample((160, 160), paint)


def draw_procedural_action_plate() -> Image.Image:
    size = (256, 256)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        cx = cy = 128
        for radius, color in (
            (122, OUTER_KEY),
            (118, GUNMETAL),
            (114, PANEL_INNER),
        ):
            draw.ellipse(
                ((cx - radius) * s, (cy - radius) * s, (cx + radius) * s, (cy + radius) * s),
                fill=color,
            )
        draw.ellipse(
            ((cx - 114) * s, (cy - 114) * s, (cx + 114) * s, (cy + 114) * s),
            outline=PEARL_RIM,
            width=2 * s,
        )
        draw.arc(
            ((cx - 118) * s, (cy - 118) * s, (cx + 118) * s, (cy + 118) * s),
            204,
            246,
            fill=CHAMPAGNE,
            width=2 * s,
        )

    plate = supersample(size, paint)
    source_path = IMAGEGEN / "action_plate_chroma.png"
    source = remove_green_chroma(load_rgba(source_path)) if source_path.exists() else None
    return add_micrograin(plate, source, strength=3.0, interior_radius=112.0)


def cubic_curve(
    start: tuple[float, float],
    control_a: tuple[float, float],
    control_b: tuple[float, float],
    end: tuple[float, float],
    steps: int = 28,
) -> list[tuple[float, float]]:
    points: list[tuple[float, float]] = []
    for index in range(steps + 1):
        t = index / steps
        inv = 1.0 - t
        x = (
            inv**3 * start[0]
            + 3.0 * inv**2 * t * control_a[0]
            + 3.0 * inv * t**2 * control_b[0]
            + t**3 * end[0]
        )
        y = (
            inv**3 * start[1]
            + 3.0 * inv**2 * t * control_a[1]
            + 3.0 * inv * t**2 * control_b[1]
            + t**3 * end[1]
        )
        points.append((x, y))
    return points


def draw_ultimate_glyph() -> Image.Image:
    """Two attached orbiting blades; no star, lightning, or loose shard."""
    size = (256, 256)

    def rotate_180(points: list[tuple[float, float]]) -> list[tuple[float, float]]:
        return [(256.0 - x, 256.0 - y) for x, y in points]

    outer_curve = cubic_curve((125, 42), (172, 45), (204, 77), (202, 119))
    inner_curve = cubic_curve((202, 119), (178, 102), (151, 95), (134, 72))
    blade = outer_curve + inner_curve + [(125, 42)]

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.polygon(scale_points(blade, s), fill=ICON_PEARL)
        draw.polygon(scale_points(rotate_180(blade), s), fill=ICON_PEARL)
        draw.ellipse((116 * s, 116 * s, 140 * s, 140 * s), fill=GUNMETAL_DARK)
        draw.ellipse((121 * s, 121 * s, 135 * s, 135 * s), fill=CHAMPAGNE)

    return supersample(size, paint)


def draw_action_ready_arc() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.arc((12 * s, 12 * s, 244 * s, 244 * s), -88, -16, fill=READY_CYAN, width=8 * s)

    return supersample((256, 256), paint)


def draw_action_cooldown_disc() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.ellipse((20 * s, 20 * s, 236 * s, 236 * s), fill=COOLDOWN_MASK)

    return supersample((256, 256), paint)


def build_action_glyph(output_name: str) -> Image.Image:
    if output_name == "glyph_ultimate.png":
        return draw_ultimate_glyph()

    output_size = RUNTIME_SPECS[output_name][0]
    source_path = IMAGEGEN / ACTION_SOURCE_NAMES[output_name]
    if source_path.exists():
        keyed = remove_green_chroma(load_rgba(source_path))
    else:
        keyed = extract_legacy_glyph(LEGACY / ACTION_LEGACY_NAMES[output_name])

    if output_name == "glyph_dash.png":
        keyed = keep_largest_components(keyed, 1)
    elif output_name == "glyph_attack_ranged.png":
        keyed = keep_largest_components(keyed, 1)
    elif output_name == "glyph_weapon_swap.png":
        keyed = keep_largest_components(keyed, 3)
    else:
        keyed = keep_largest_components(keyed, 1)

    keyed = recolour_action_glyph(keyed)
    target = (176, 176) if output_size == (256, 256) else (220, 220)
    return trim_and_fit(keyed, output_size, target)


SUMMON_OUTER_NORMALIZED = [
    (0.18, 0.02),
    (0.91, 0.02),
    (0.99, 0.15),
    (0.86, 0.86),
    (0.76, 0.98),
    (0.15, 0.92),
    (0.02, 0.77),
    (0.08, 0.18),
]


def summon_points(size: tuple[int, int]) -> list[tuple[float, float]]:
    return [(x * size[0], y * size[1]) for x, y in SUMMON_OUTER_NORMALIZED]


def draw_summon_frame(size: tuple[int, int], primary: bool) -> Image.Image:
    outer_width = 7 if primary else 6
    metal_width = 6 if primary else 5
    pearl_width = 2

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        outer = summon_points(size)
        metal = inset_points(outer, outer_width, outer_width)
        pearl = inset_points(metal, metal_width, metal_width)
        band = inset_points(pearl, pearl_width, pearl_width)
        aperture = inset_points(outer, size[0] * 0.055, size[1] * 0.055)
        draw.polygon(scale_points(outer, s), fill=OUTER_KEY)
        draw.polygon(scale_points(metal, s), fill=GUNMETAL)
        draw.polygon(scale_points(pearl, s), fill=PEARL_RIM)
        draw.polygon(scale_points(band, s), fill=PANEL_RAISED)
        draw.polygon(scale_points(aperture, s), fill=CLEAR)
        if primary:
            draw.line(
                scale_points([outer[0], outer[1]], s),
                fill=CHAMPAGNE,
                width=3 * s,
            )

    result = supersample(size, paint)
    micro_path = IMAGEGEN / "panel_microtexture.png"
    return add_micrograin(result, load_rgba(micro_path) if micro_path.exists() else None, 2.0)


def draw_summon_mask() -> Image.Image:
    size = (320, 344)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        aperture = inset_points(summon_points(size), size[0] * 0.055, size[1] * 0.055)
        draw.polygon(scale_points(aperture, s), fill=(255, 255, 255, 255))

    return supersample(size, paint)


def draw_summon_state_arc() -> Image.Image:
    size = (320, 344)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        # One restrained 72 degree accent around the portrait's upper-right.
        draw.arc((28 * s, 34 * s, 292 * s, 298 * s), -88, -16, fill=READY_CYAN, width=10 * s)

    return supersample(size, paint)


def draw_summon_cost_tab() -> Image.Image:
    size = (92, 54)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        outer = [(8, 2), (84, 2), (91, 11), (91, 46), (83, 53), (8, 53), (1, 45), (1, 10)]
        metal = inset_points(outer, 4, 4)
        pearl = inset_points(metal, 4, 4)
        body = inset_points(pearl, 2, 2)
        draw.polygon(scale_points(outer, s), fill=OUTER_KEY)
        draw.polygon(scale_points(metal, s), fill=GUNMETAL_DARK)
        draw.polygon(scale_points(pearl, s), fill=PEARL_RIM)
        draw.polygon(scale_points(body, s), fill=PANEL_INNER)

    return supersample(size, paint)


def octagon_points(center: tuple[float, float], radius: float) -> list[tuple[float, float]]:
    cx, cy = center
    return [
        (
            cx + math.cos(math.radians(22.5 + index * 45.0)) * radius,
            cy + math.sin(math.radians(22.5 + index * 45.0)) * radius,
        )
        for index in range(8)
    ]


def draw_player_portrait_frame() -> Image.Image:
    size = (200, 200)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        outer = octagon_points((100, 100), 100 - 7)
        metal = octagon_points((100, 100), 100 - 14)
        pearl = octagon_points((100, 100), 100 - 20)
        aperture = octagon_points((100, 100), 100 - 23)
        draw.polygon(scale_points(outer, s), fill=OUTER_KEY)
        draw.polygon(scale_points(metal, s), fill=GUNMETAL)
        draw.polygon(scale_points(pearl, s), fill=PEARL_RIM)
        draw.polygon(scale_points(aperture, s), fill=CLEAR)
        draw.arc((13 * s, 13 * s, 187 * s, 187 * s), 201, 219, fill=CHAMPAGNE, width=4 * s)

    return supersample(size, paint)


def draw_player_portrait_mask() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.polygon(scale_points(octagon_points((100, 100), 77), s), fill=(255, 255, 255, 255))

    return supersample((200, 200), paint)


def draw_player_ammo_plate() -> Image.Image:
    size = (256, 144)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        outer = [(12, 12), (222, 12), (248, 38), (248, 106), (222, 132), (12, 132), (4, 120), (4, 24)]
        metal = inset_points(outer, 6, 6)
        pearl = inset_points(metal, 5, 5)
        body = inset_points(pearl, 2, 2)
        draw.polygon(scale_points(outer, s), fill=OUTER_KEY)
        draw.polygon(scale_points(metal, s), fill=GUNMETAL_DARK)
        draw.polygon(scale_points(pearl, s), fill=PEARL_RIM)
        draw.polygon(scale_points(body, s), fill=PANEL_INNER)

    result = supersample(size, paint)
    micro_path = IMAGEGEN / "panel_microtexture.png"
    return add_micrograin(result, load_rgba(micro_path) if micro_path.exists() else None, 2.0)


def draw_bullet_glyph() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        body = [(50, 25), (78, 25), (86, 38), (86, 94), (78, 106), (50, 106), (42, 94), (42, 38)]
        draw.polygon(scale_points(body, s), fill=ICON_PEARL)
        draw.rectangle((48 * s, 83 * s, 80 * s, 96 * s), fill=GUNMETAL)
        draw.line(((51 * s, 39 * s), (77 * s, 39 * s)), fill=GUNMETAL, width=4 * s)

    return supersample((128, 128), paint)


def draw_ranged_mode_glyph() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.ellipse((31 * s, 31 * s, 97 * s, 97 * s), outline=ICON_PEARL, width=5 * s)
        draw.ellipse((59 * s, 59 * s, 69 * s, 69 * s), fill=READY_CYAN)
        for start, end in (
            ((64, 18), (64, 43)),
            ((64, 85), (64, 110)),
            ((18, 64), (43, 64)),
            ((85, 64), (110, 64)),
        ):
            draw.line(((start[0] * s, start[1] * s), (end[0] * s, end[1] * s)), fill=ICON_PEARL, width=5 * s)

    return supersample((128, 128), paint)


def draw_joystick_base() -> Image.Image:
    size = (320, 320)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.ellipse((9 * s, 9 * s, 311 * s, 311 * s), fill=(11, 17, 24, 38))
        draw.ellipse((9 * s, 9 * s, 311 * s, 311 * s), outline=(216, 217, 211, 58), width=4 * s)
        draw.ellipse((57 * s, 57 * s, 263 * s, 263 * s), outline=(216, 217, 211, 34), width=2 * s)

    return supersample(size, paint)


def draw_joystick_knob() -> Image.Image:
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.ellipse((6 * s, 6 * s, 122 * s, 122 * s), fill=OUTER_KEY)
        draw.ellipse((11 * s, 11 * s, 117 * s, 117 * s), fill=GUNMETAL)
        draw.ellipse((15 * s, 15 * s, 113 * s, 113 * s), fill=PEARL_RIM)
        draw.ellipse((16 * s, 16 * s, 112 * s, 112 * s), fill=(31, 38, 48, 190))

    return supersample((128, 128), paint)


def draw_reticle_needle() -> Image.Image:
    size = (192, 192)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        draw.polygon(
            scale_points([(38, 91.5), (73.5, 91.5), (80, 96), (73.5, 100.5), (38, 100.5)], s),
            fill=(5, 8, 13, 205),
        )
        draw.polygon(
            scale_points([(40.5, 93.5), (73, 93.5), (78.5, 96), (73, 98.5), (40.5, 98.5)], s),
            fill=(241, 239, 232, 244),
        )
        draw.polygon(
            scale_points([(72, 94.25), (79.1, 96), (72, 97.75)], s),
            fill=(57, 214, 232, 224),
        )

    return supersample(size, paint)


def draw_reticle_dot() -> Image.Image:
    size = (192, 192)

    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, s: int) -> None:
        for radius, color in (
            (8.0, (5, 8, 13, 218)),
            (5.8, (241, 239, 232, 252)),
            (3.55, (57, 214, 232, 250)),
            (1.45, (255, 255, 250, 255)),
        ):
            draw.ellipse(
                ((96 - radius) * s, (96 - radius) * s, (96 + radius) * s, (96 + radius) * s),
                fill=color,
            )

    return supersample(size, paint)


def build_micrograin() -> Image.Image:
    source_path = IMAGEGEN / "panel_microtexture.png"
    if not source_path.exists():
        # Deterministic, non-placeholder neutral grain fallback.
        yy, xx = np.mgrid[0:128, 0:128]
        detail = np.sin(xx * 0.73 + yy * 1.17) + np.sin(xx * 1.91 - yy * 0.43)
    else:
        source = ImageOps.grayscale(load_rgba(source_path).convert("RGB"))
        source = ImageOps.fit(source, (512, 512), method=Image.Resampling.LANCZOS)
        source = source.resize((128, 128), Image.Resampling.LANCZOS)
        blur = source.filter(ImageFilter.GaussianBlur(radius=9.0))
        detail = np.asarray(source, dtype=np.float32) - np.asarray(blur, dtype=np.float32)
    deviation = max(0.001, float(np.percentile(np.abs(detail), 95.0)))
    detail = np.clip(detail / deviation, -1.0, 1.0)
    # ±3 RGB, as specified. Edge cross-fade makes the small texture tile safely.
    data = np.clip(np.rint(128.0 + detail * 3.0), 0, 255).astype(np.uint8)
    edge = 12
    for index in range(edge):
        t = index / max(1, edge - 1)
        mixed_lr = np.rint(data[:, index] * t + data[:, -(edge - index)] * (1.0 - t)).astype(np.uint8)
        data[:, index] = mixed_lr
        data[:, -(index + 1)] = mixed_lr
        mixed_tb = np.rint(data[index, :] * t + data[-(edge - index), :] * (1.0 - t)).astype(np.uint8)
        data[index, :] = mixed_tb
        data[-(index + 1), :] = mixed_tb
    rgba = np.dstack((data, data, data, np.full_like(data, 255)))
    return Image.fromarray(rgba, "RGBA")


def build_runtime_assets() -> dict[str, Image.Image]:
    assets: dict[str, Image.Image] = {}
    assets["objective_frame.png"] = draw_objective_frame()
    assets["boss_name_tab.png"] = draw_boss_name_tab()
    assets["boss_hp_track.png"] = draw_meter_track((1024, 80), (20, 60), 18, 5, 6, 2)
    assets["boss_hp_fill.png"] = gradient_fill(
        (1024, 24), [(0, 1), (1010, 1), (1023, 12), (1010, 23), (0, 23)], ENEMY_RED
    )
    assets["boss_cost_track.png"] = draw_meter_track((1024, 48), (13, 35), 14, 4, 5, 2)
    assets["boss_cost_fill.png"] = gradient_fill(
        (1024, 18), [(0, 1), (1012, 1), (1023, 9), (1012, 17), (0, 17)], READY_CYAN
    )
    assets["pause_plate.png"] = draw_pause_plate()
    assets["glyph_pause.png"] = draw_pause_glyph()
    assets["action_plate.png"] = draw_procedural_action_plate()
    assets["action_ready_arc.png"] = draw_action_ready_arc()
    assets["action_cooldown_disc.png"] = draw_action_cooldown_disc()
    for output_name in ACTION_SOURCE_NAMES:
        assets[output_name] = build_action_glyph(output_name)
    assets["summon_mask.png"] = draw_summon_mask()
    assets["summon_frame_s1.png"] = draw_summon_frame((320, 344), primary=True)
    assets["summon_frame_s2.png"] = draw_summon_frame((288, 316), primary=False)
    assets["summon_frame_s3.png"] = draw_summon_frame((288, 316), primary=False)
    assets["summon_state_arc.png"] = draw_summon_state_arc()
    assets["summon_cost_tab.png"] = draw_summon_cost_tab()
    for slot in (1, 2, 3):
        assets[f"summon_portrait_s{slot}.png"] = load_rgba(LEGACY / f"summon_s{slot}_portrait.png")
    assets["player_portrait_frame.png"] = draw_player_portrait_frame()
    assets["player_portrait_mask.png"] = draw_player_portrait_mask()
    assets["player_portrait.png"] = load_rgba(LEGACY / "player_portrait.png")
    assets["player_hp_track.png"] = draw_meter_track((1024, 56), (14, 42), 20, 4, 5, 2)
    assets["player_hp_fill.png"] = gradient_fill(
        (1024, 24), [(0, 1), (1010, 1), (1023, 12), (1010, 23), (0, 23)], ICON_PEARL
    )
    assets["player_en_track.png"] = draw_meter_track((1024, 44), (11, 33), 14, 4, 4, 2)
    assets["player_en_fill.png"] = gradient_fill(
        (1024, 20), [(0, 1), (1010, 1), (1023, 10), (1010, 19), (0, 19)], READY_CYAN
    )
    assets["player_ammo_plate.png"] = draw_player_ammo_plate()
    assets["glyph_bullet.png"] = draw_bullet_glyph()
    assets["glyph_mode_ranged.png"] = draw_ranged_mode_glyph()
    assets["joystick_base.png"] = draw_joystick_base()
    assets["joystick_knob.png"] = draw_joystick_knob()
    assets["reticle_needle.png"] = draw_reticle_needle()
    assets["reticle_dot.png"] = draw_reticle_dot()
    return assets


def checkerboard(size: tuple[int, int], cell: int = 16) -> Image.Image:
    width, height = size
    yy, xx = np.mgrid[0:height, 0:width]
    pattern = ((xx // cell + yy // cell) % 2).astype(np.uint8)
    dark = np.array((24, 29, 37, 255), dtype=np.uint8)
    light = np.array((43, 50, 61, 255), dtype=np.uint8)
    data = np.where(pattern[..., None] == 0, dark, light)
    return Image.fromarray(data, "RGBA")


def fit_preview(image: Image.Image, box: tuple[int, int]) -> Image.Image:
    return ImageOps.contain(image.convert("RGBA"), box, method=Image.Resampling.LANCZOS)


def paste_center(canvas: Image.Image, image: Image.Image, center: tuple[int, int]) -> None:
    canvas.alpha_composite(image, (center[0] - image.width // 2, center[1] - image.height // 2))


def label(draw: ImageDraw.ImageDraw, position: tuple[int, int], text: str) -> None:
    draw.text(position, text, fill=(245, 244, 240, 255), font=ImageFont.load_default())


def composed_action(
    assets: dict[str, Image.Image],
    glyph_name: str,
    size: int,
    ready: bool,
) -> Image.Image:
    canvas = Image.new("RGBA", (size, size), CLEAR)
    plate = assets["action_plate.png"].resize((size, size), Image.Resampling.LANCZOS)
    canvas.alpha_composite(plate)
    if ready:
        arc = assets["action_ready_arc.png"].resize((size, size), Image.Resampling.LANCZOS)
        canvas.alpha_composite(arc)
    glyph = assets[glyph_name]
    if glyph.size != (size, size):
        glyph = glyph.resize((size, size), Image.Resampling.LANCZOS)
    canvas.alpha_composite(glyph)
    return zero_transparent_rgb(canvas)


def build_action_contact_sheet(assets: dict[str, Image.Image]) -> Image.Image:
    sheet = checkerboard((1400, 620), 20)
    draw = ImageDraw.Draw(sheet)
    names = [
        "glyph_weapon_swap.png",
        "glyph_ultimate.png",
        "glyph_dash.png",
        "glyph_attack_ranged.png",
    ]
    centers = [(170, 190), (500, 190), (830, 190), (1190, 190)]
    for name, center in zip(names, centers):
        size = 256 if name != "glyph_attack_ranged.png" else 320
        composed = composed_action(assets, name, size, ready=name == "glyph_ultimate.png")
        paste_center(sheet, fit_preview(composed, (280, 280)), center)
        label(draw, (center[0] - 100, 350), name)
    for index, name in enumerate(("action_plate.png", "action_ready_arc.png", "action_cooldown_disc.png")):
        preview = fit_preview(assets[name], (150, 150))
        center = (410 + index * 290, 500)
        paste_center(sheet, preview, center)
        label(draw, (center[0] - 90, 585), name)
    return sheet


def compose_summon(
    assets: dict[str, Image.Image],
    slot: int,
    ready: bool,
) -> Image.Image:
    frame_name = f"summon_frame_s{slot}.png"
    frame = assets[frame_name]
    size = frame.size
    mask = assets["summon_mask.png"].resize(size, Image.Resampling.LANCZOS)
    portrait = ImageOps.fit(
        assets[f"summon_portrait_s{slot}.png"],
        size,
        method=Image.Resampling.LANCZOS,
        centering=(0.5, 0.46),
    )
    portrait_alpha = ImageChops.multiply(portrait.getchannel("A"), mask.getchannel("A"))
    portrait.putalpha(portrait_alpha)
    canvas = Image.new("RGBA", size, CLEAR)
    canvas.alpha_composite(portrait)
    if ready:
        canvas.alpha_composite(assets["summon_state_arc.png"].resize(size, Image.Resampling.LANCZOS))
    canvas.alpha_composite(frame)
    tab = assets["summon_cost_tab.png"]
    if slot != 1:
        tab = tab.resize((84, 50), Image.Resampling.LANCZOS)
    canvas.alpha_composite(tab, (14, size[1] - tab.height - 14))
    return zero_transparent_rgb(canvas)


def build_summon_contact_sheet(assets: dict[str, Image.Image]) -> Image.Image:
    sheet = checkerboard((1200, 450), 18)
    draw = ImageDraw.Draw(sheet)
    for slot, center_x in zip((1, 2, 3), (220, 600, 980)):
        composed = compose_summon(assets, slot, ready=slot == 1)
        preview = fit_preview(composed, (300, 340))
        paste_center(sheet, preview, (center_x, 205))
        label(draw, (center_x - 95, 397), f"summon slot {slot} / no baked state")
    return sheet


def compose_reticle(assets: dict[str, Image.Image]) -> Image.Image:
    canvas = Image.new("RGBA", (192, 192), CLEAR)
    needle = assets["reticle_needle.png"]
    for turns in range(4):
        canvas.alpha_composite(needle.rotate(-90 * turns, resample=Image.Resampling.BICUBIC))
    canvas.alpha_composite(assets["reticle_dot.png"])
    return zero_transparent_rgb(canvas, alpha_threshold=3)


def build_component_contact_sheet(assets: dict[str, Image.Image]) -> Image.Image:
    sheet = checkerboard((1800, 1200), 20)
    draw = ImageDraw.Draw(sheet)
    entries = [
        ("objective_frame.png", (430, 125), (800, 165)),
        ("boss_name_tab.png", (1290, 125), (500, 100)),
        ("boss_hp_track.png", (480, 295), (850, 80)),
        ("boss_cost_track.png", (480, 390), (850, 55)),
        ("player_hp_track.png", (480, 500), (850, 65)),
        ("player_en_track.png", (480, 590), (850, 55)),
        ("player_portrait_frame.png", (1070, 400), (200, 200)),
        ("player_ammo_plate.png", (1410, 390), (300, 170)),
        ("pause_plate.png", (1070, 650), (160, 160)),
        ("joystick_base.png", (1420, 680), (300, 300)),
    ]
    for name, center, box in entries:
        preview = fit_preview(assets[name], box)
        paste_center(sheet, preview, center)
        label(draw, (center[0] - 95, center[1] + preview.height // 2 + 8), name)
    reticle = compose_reticle(assets)
    paste_center(sheet, reticle.resize((224, 224), Image.Resampling.NEAREST), (1080, 930))
    label(draw, (985, 1055), "reticle dot + 4x needle")
    joystick_knob = fit_preview(assets["joystick_knob.png"], (116, 116))
    paste_center(sheet, joystick_knob, (1420, 680))
    for name, y in (
        ("boss_hp_fill.png", 760),
        ("boss_cost_fill.png", 815),
        ("player_hp_fill.png", 870),
        ("player_en_fill.png", 925),
    ):
        preview = assets[name].resize((760, max(14, assets[name].height)), Image.Resampling.LANCZOS)
        sheet.alpha_composite(preview, (50, y))
        label(draw, (50, y + preview.height + 4), name)
    return sheet


def save_png(image: Image.Image, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    zero_transparent_rgb(image).save(destination, optimize=True, compress_level=9)


def validate_runtime_assets(assets: dict[str, Image.Image]) -> None:
    if set(assets) != set(RUNTIME_SPECS):
        missing = sorted(set(RUNTIME_SPECS) - set(assets))
        extra = sorted(set(assets) - set(RUNTIME_SPECS))
        raise RuntimeError(f"Runtime asset contract mismatch; missing={missing}, extra={extra}")
    for name, image in assets.items():
        expected_size = RUNTIME_SPECS[name][0]
        if image.size != expected_size:
            raise RuntimeError(f"{name} has {image.size}, expected {expected_size}")
        if image.mode != "RGBA":
            raise RuntimeError(f"{name} is {image.mode}, expected RGBA")
        if alpha_bbox(image, 4) is None:
            raise RuntimeError(f"{name} is empty")


def image_qa(path: Path, imagegen_derived: bool) -> dict[str, object]:
    image = load_rgba(path)
    data = np.asarray(image, dtype=np.uint8)
    alpha = data[..., 3]
    transparent_rgb_pixels = int(np.count_nonzero((alpha == 0) & np.any(data[..., :3] != 0, axis=2)))
    opaque = alpha > 8
    red = data[..., 0].astype(np.int16)
    green = data[..., 1].astype(np.int16)
    blue = data[..., 2].astype(np.int16)
    green_fringe = opaque & (green - np.maximum(red, blue) > 58)
    return {
        "path": path.relative_to(ROOT).as_posix(),
        "dimensions": [image.width, image.height],
        "mode": image.mode,
        "alpha_bbox_gt3": list(alpha_bbox(image, 4) or ()),
        "corner_alpha": [int(alpha[0, 0]), int(alpha[0, -1]), int(alpha[-1, 0]), int(alpha[-1, -1])],
        "transparent_rgb_pixels": transparent_rgb_pixels,
        "green_fringe_pixels": int(np.count_nonzero(green_fringe)) if imagegen_derived else 0,
        "imagegen_derived": imagegen_derived,
        "sha256": sha256(path),
    }


def write_qa_reports() -> None:
    generated_pngs = sorted(RUNTIME.glob("*.png")) + sorted(MOTION.glob("*.png")) + sorted(QA.glob("*.png"))
    imagegen_derived_names = {"action_plate.png", *ACTION_SOURCE_NAMES.keys()}
    reports = [
        image_qa(path, path.parent == RUNTIME and path.name in imagegen_derived_names)
        for path in generated_pngs
    ]
    if any(report["transparent_rgb_pixels"] != 0 for report in reports):
        raise RuntimeError("QA failed: non-zero RGB remains in fully transparent pixels")
    if any(report["green_fringe_pixels"] != 0 for report in reports):
        raise RuntimeError("QA failed: green chroma fringe remains in ImageGen-derived output")
    report_path = QA / "qa_report.json"
    report_path.write_text(json.dumps({"assets": reports}, indent=2) + "\n", encoding="utf-8")

    lines = [f"{report['sha256']}  {report['path']}" for report in reports]
    (QA / "hash_manifest.sha256").write_text("\n".join(lines) + "\n", encoding="utf-8")


def prepare_output(force: bool) -> None:
    RUNTIME.mkdir(parents=True, exist_ok=True)
    MOTION.mkdir(parents=True, exist_ok=True)
    QA.mkdir(parents=True, exist_ok=True)
    expected = [RUNTIME / name for name in RUNTIME_SPECS]
    expected += [
        MOTION / "DB_UI_CelestialFlow.png",
        MOTION / "panel_micrograin_128.png",
        QA / "action_components_contact_sheet.png",
        QA / "summon_components_contact_sheet.png",
        QA / "hud_components_contact_sheet.png",
        QA / "qa_report.json",
        QA / "hash_manifest.sha256",
    ]
    existing = [path for path in expected if path.exists()]
    if existing and not force:
        preview = "\n".join(str(path) for path in existing[:8])
        raise FileExistsError(
            "V2 outputs already exist; pass --force for an intentional deterministic rebuild:\n" + preview
        )


def main() -> None:
    args = parse_args()
    prepare_output(args.force)
    assets = build_runtime_assets()
    validate_runtime_assets(assets)
    for name, image in assets.items():
        save_png(image, RUNTIME / name)

    flow_source = LEGACY / "Motion" / "DB_UI_CelestialFlow.png"
    save_png(load_rgba(flow_source), MOTION / "DB_UI_CelestialFlow.png")
    save_png(build_micrograin(), MOTION / "panel_micrograin_128.png")
    save_png(build_action_contact_sheet(assets), QA / "action_components_contact_sheet.png")
    save_png(build_summon_contact_sheet(assets), QA / "summon_components_contact_sheet.png")
    save_png(build_component_contact_sheet(assets), QA / "hud_components_contact_sheet.png")
    write_qa_reports()
    print(f"Built {len(assets)} runtime sprites under {RUNTIME}")
    print(f"QA report: {QA / 'qa_report.json'}")


if __name__ == "__main__":
    main()
