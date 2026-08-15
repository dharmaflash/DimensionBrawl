"""Build the pixel-led Celestial HUD target asset pack.

This pack treats the approved 1672x941 combat mockup as immutable optical
reference, while rebuilding every runtime surface as a deterministic atomic
RGBA sprite.  No labels, values, cooldown numbers, or state-dependent copy are
baked into runtime art.

The golden composite is used only for measured proportions and QA provenance.
Summon identities are exact content extractions from the approved high-resolution
v2 summon sheet; no newly interpreted creature art or palette grade is applied.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps


OUT = Path(__file__).resolve().parent
RUNTIME = OUT / "Runtime"
SOURCE = OUT / "Source" / "Reference"
QA = OUT / "QA"

REFERENCE_FILES = {
    "golden": SOURCE / "golden_composite_1672x941.png",
    "objective": SOURCE / "golden_objective_composite_1672x941.png",
    "clean": SOURCE / "clean_gameplay_plate.png",
    "player": SOURCE / "player_portrait_source.png",
    "summon_sheet": SOURCE / "approved_summon_vertical_stack_v2_941x1672.png",
}

# Coordinates are in the authoritative 941x1672 v2 sheet.  Each inset polygon
# follows only the illustration aperture and deliberately notches around the
# baked cost tab; frame lines, cooldown arcs and state bars remain separate.
SUMMON_SOURCE_REGIONS = {
    1: {
        "box": (395, 329, 783, 662),
        "aperture": [(510, 342), (734, 342), (770, 379), (691, 650), (533, 650), (533, 573), (409, 573), (482, 342)],
    },
    2: {
        "box": (432, 750, 710, 999),
        "aperture": [(508, 756), (657, 756), (699, 784), (682, 985), (546, 985), (546, 911), (442, 911), (481, 762)],
    },
    3: {
        "box": (437, 1058, 685, 1268),
        "aperture": [(502, 1066), (635, 1066), (675, 1096), (655, 1207), (540, 1207), (540, 1201), (446, 1201), (483, 1073)],
    },
}

TRANSPARENT = (0, 0, 0, 0)
GRAPHITE = (10, 13, 18, 244)
CHARCOAL = (20, 24, 31, 238)
CHARCOAL_LIGHT = (42, 48, 57, 210)
SILVER = (203, 208, 211, 238)
PEARL = (246, 246, 241, 250)
CYAN = (34, 205, 231, 250)
CYAN_LIGHT = (113, 232, 240, 250)
GOLD = (218, 173, 87, 246)
ORANGE = (243, 126, 37, 248)
RED = (226, 55, 71, 255)

EXPECTED: dict[str, tuple[int, int]] = {
    "Objective/objective_body.png": (960, 199),
    "Objective/objective_facets_top.png": (960, 199),
    "Objective/objective_facets_bottom.png": (960, 199),
    "Boss/boss_chassis.png": (1100, 150),
    "Boss/boss_name_tab.png": (420, 84),
    "Boss/boss_hp_track.png": (1024, 56),
    "Boss/boss_hp_fill.png": (1024, 28),
    "Boss/boss_cost_track.png": (1024, 44),
    "Boss/boss_cost_fill.png": (1024, 22),
    "System/pause_plate.png": (192, 192),
    "System/pause_glyph.png": (192, 192),
    "Action/action_plate.png": (512, 512),
    "Action/action_ready_arc.png": (512, 512),
    "Action/action_cooldown_disc.png": (512, 512),
    "Action/glyph_weapon_swap.png": (512, 512),
    "Action/glyph_ultimate.png": (512, 512),
    "Action/glyph_dash.png": (512, 512),
    "Action/glyph_attack.png": (512, 512),
    "Summon/summon_mask_s1.png": (384, 340),
    "Summon/summon_mask_s2.png": (360, 260),
    "Summon/summon_mask_s3.png": (340, 242),
    "Summon/summon_frame_s1.png": (384, 340),
    "Summon/summon_frame_s2.png": (360, 260),
    "Summon/summon_frame_s3.png": (340, 242),
    "Summon/summon_accent_s1.png": (384, 340),
    "Summon/summon_accent_s2.png": (360, 260),
    "Summon/summon_accent_s3.png": (340, 242),
    "Summon/summon_cost_tab_s1.png": (128, 72),
    "Summon/summon_cost_tab_s2.png": (112, 64),
    "Summon/summon_cost_tab_s3.png": (112, 64),
    "Summon/summon_portrait_s1.png": (512, 512),
    "Summon/summon_portrait_s2.png": (512, 512),
    "Summon/summon_portrait_s3.png": (512, 512),
    "Joystick/joystick_base_glass.png": (512, 512),
    "Joystick/joystick_ring_ticks.png": (512, 512),
    "Joystick/joystick_knob.png": (192, 192),
    "Player/player_portrait.png": (512, 512),
    "Player/player_portrait_mask.png": (256, 256),
    "Player/player_portrait_frame.png": (256, 256),
    "Player/player_chassis.png": (1400, 200),
    "Player/player_hp_track.png": (1024, 48),
    "Player/player_hp_fill.png": (1024, 26),
    "Player/player_cost_track.png": (1024, 42),
    "Player/player_cost_fill_segmented.png": (1024, 24),
    "Player/player_state_pips.png": (128, 64),
    "Player/player_mode_glyph.png": (128, 128),
    "Player/player_ammo_plate_compact.png": (320, 112),
    "Player/player_bullet_glyph.png": (128, 128),
    "Player/player_ammo_separator.png": (32, 112),
    "Reticle/reticle_precision_dot.png": (192, 192),
    "Reticle/reticle_precision_needle.png": (192, 192),
}


def load_rgba(path: Path) -> Image.Image:
    if not path.exists():
        raise FileNotFoundError(path)
    return Image.open(path).convert("RGBA")


def zero_transparent_rgb(image: Image.Image) -> Image.Image:
    data = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    data[data[..., 3] == 0, :3] = 0
    return Image.fromarray(data, "RGBA")


def alpha_bbox(image: Image.Image, threshold: int = 3):
    alpha = np.asarray(image.convert("RGBA").getchannel("A"), dtype=np.uint8)
    ys, xs = np.where(alpha > threshold)
    if not len(xs):
        return None
    return int(xs.min()), int(ys.min()), int(xs.max() + 1), int(ys.max() + 1)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def save(image: Image.Image, relative: str) -> None:
    expected = EXPECTED[relative]
    image = zero_transparent_rgb(image)
    if image.size != expected:
        raise ValueError(f"{relative}: expected {expected}, got {image.size}")
    destination = RUNTIME / relative
    destination.parent.mkdir(parents=True, exist_ok=True)
    image.save(destination, optimize=True)


def supersampled(size: tuple[int, int], painter, scale: int = 4) -> Image.Image:
    large = Image.new("RGBA", (size[0] * scale, size[1] * scale), TRANSPARENT)
    painter(large, ImageDraw.Draw(large, "RGBA"), scale)
    return zero_transparent_rgb(large.resize(size, Image.Resampling.LANCZOS))


def scaled_points(points, scale: int):
    return [(round(x * scale), round(y * scale)) for x, y in points]


def ring_gradient(
    size: tuple[int, int],
    center: tuple[float, float],
    radius: float,
    inner: tuple[int, int, int, int],
    outer: tuple[int, int, int, int],
) -> Image.Image:
    width, height = size
    yy, xx = np.mgrid[0:height, 0:width].astype(np.float32)
    distance = np.sqrt((xx + 0.5 - center[0]) ** 2 + (yy + 0.5 - center[1]) ** 2)
    t = np.clip(distance / radius, 0.0, 1.0)[..., None]
    a = np.asarray(inner, dtype=np.float32)
    b = np.asarray(outer, dtype=np.float32)
    rgba = np.rint(a * (1.0 - t) + b * t).astype(np.uint8)
    edge = np.clip((radius + 1.2 - distance) / 1.8, 0.0, 1.0)
    rgba[..., 3] = np.rint(rgba[..., 3].astype(np.float32) * edge).astype(np.uint8)
    return zero_transparent_rgb(Image.fromarray(rgba, "RGBA"))


def prepare_references() -> dict[str, Path]:
    refs = REFERENCE_FILES.copy()
    missing = [path for path in refs.values() if not path.exists()]
    if missing:
        raise FileNotFoundError(
            "Checked-in HUD reference source is missing: "
            + ", ".join(path.relative_to(OUT).as_posix() for path in missing)
        )
    dimensions = {key: Image.open(path).size for key, path in refs.items()}
    if dimensions["golden"] != (1672, 941):
        raise ValueError(f"Unexpected golden dimensions: {dimensions['golden']}")
    if dimensions["objective"] != (1672, 941):
        raise ValueError(f"Unexpected objective dimensions: {dimensions['objective']}")
    if dimensions["summon_sheet"] != (941, 1672):
        raise ValueError(f"Unexpected summon sheet dimensions: {dimensions['summon_sheet']}")
    return refs


def build_objective() -> None:
    size = EXPECTED["Objective/objective_body.png"]

    def body_painter(canvas, draw, s):
        body = [(0, 58), (914, 58), (960, 80), (932, 140), (0, 140)]
        draw.polygon(scaled_points(body, s), fill=(20, 25, 32, 222))
        lower = [(0, 112), (873, 112), (932, 140), (0, 140)]
        draw.polygon(scaled_points(lower, s), fill=(8, 11, 16, 48))

    def top_painter(canvas, draw, s):
        step = 112
        for index, x in enumerate(range(-34, 930, step)):
            peak = 17 + (index % 2) * 2
            points = [(x, 58), (x + 58, peak), (x + 116, 58)]
            draw.polygon(
                scaled_points(points, s),
                fill=(43, 49, 58, 82 if index % 2 == 0 else 66),
            )
        draw.line(
            scaled_points([(0, 58), (912, 58)], s),
            fill=(194, 201, 203, 28),
            width=max(1, s),
        )

    def bottom_painter(canvas, draw, s):
        step = 112
        for index, x in enumerate(range(-34, 930, step)):
            # Preserve the approved source silhouette depth while keeping its
            # visual weight neutral and quiet on bright Olympus backgrounds.
            points = [(x, 140), (x + 58, 180), (x + 116, 140)]
            draw.polygon(
                scaled_points(points, s),
                fill=(34, 34, 34, 42 if index % 2 else 36),
            )
        draw.line(
            scaled_points([(0, 140), (928, 140)], s),
            fill=(12, 12, 12, 36),
            width=max(1, s),
        )

    save(supersampled(size, body_painter), "Objective/objective_body.png")
    save(supersampled(size, top_painter), "Objective/objective_facets_top.png")
    save(supersampled(size, bottom_painter), "Objective/objective_facets_bottom.png")


def angular_track(size: tuple[int, int], inset: int, border: int = 3) -> Image.Image:
    width, height = size

    def painter(canvas, draw, s):
        outer = [
            (12, 1),
            (width - 2, 1),
            (width - 22, height - 2),
            (1, height - 2),
        ]
        inner = [
            (inset + 10, inset),
            (width - inset - 9, inset),
            (width - inset - 20, height - inset),
            (inset, height - inset),
        ]
        draw.polygon(scaled_points(outer, s), fill=(5, 8, 12, 232))
        draw.line(
            scaled_points(outer + [outer[0]], s),
            fill=SILVER,
            width=border * s,
            joint="curve",
        )
        draw.polygon(scaled_points(inner, s), fill=(23, 27, 34, 218))
        draw.line(
            scaled_points([(18, 5), (width - 12, 5)], s),
            fill=(243, 244, 239, 82),
            width=max(1, s),
        )

    return supersampled(size, painter)


def trapezoid_fill(
    size: tuple[int, int], left_rgb: tuple[int, int, int], right_rgb: tuple[int, int, int]
) -> Image.Image:
    width, height = size
    t = np.linspace(0.0, 1.0, width, dtype=np.float32)[None, :, None]
    left = np.asarray(left_rgb, dtype=np.float32)[None, None, :]
    right = np.asarray(right_rgb, dtype=np.float32)[None, None, :]
    rgb = np.rint(left * (1.0 - t) + right * t).astype(np.uint8)
    rgb = np.repeat(rgb, height, axis=0)
    mask = supersampled(
        size,
        lambda canvas, draw, s: draw.polygon(
            scaled_points(
                [(6, 1), (width - 1, 1), (width - 18, height - 2), (0, height - 2)],
                s,
            ),
            fill=(255, 255, 255, 255),
        ),
    ).getchannel("A")
    rgba = np.dstack((rgb, np.asarray(mask, dtype=np.uint8)))
    return zero_transparent_rgb(Image.fromarray(rgba, "RGBA"))


def build_boss() -> None:
    def chassis_painter(canvas, draw, s):
        width, height = 1100, 150
        top = [(24, 14), (348, 14), (380, 54), (1058, 54), (1094, 71), (1070, 94), (32, 94), (0, 75)]
        lower = [(34, 88), (1038, 88), (1068, 104), (1045, 132), (19, 132), (3, 115)]
        draw.polygon(scaled_points(top, s), fill=(10, 13, 18, 124))
        draw.polygon(scaled_points(lower, s), fill=(6, 9, 13, 116))
        draw.polygon(
            scaled_points([(332, 18), (346, 18), (372, 46), (356, 46)], s),
            fill=(87, 94, 103, 115),
        )
        draw.polygon(
            scaled_points([(350, 18), (362, 18), (385, 44), (372, 44)], s),
            fill=(87, 94, 103, 86),
        )

    def name_painter(canvas, draw, s):
        w, h = 420, 84
        outer = [(30, 5), (370, 5), (417, 22), (390, 72), (0, 72)]
        inner = [(35, 11), (350, 11), (385, 25), (368, 64), (15, 64)]
        draw.polygon(scaled_points(outer, s), fill=(5, 7, 11, 250))
        draw.line(
            scaled_points(outer + [outer[0]], s),
            fill=(184, 190, 194, 220),
            width=2 * s,
        )
        draw.polygon(scaled_points(inner, s), fill=(29, 33, 40, 238))
        for x in (350, 369):
            draw.polygon(
                scaled_points([(x, 14), (x + 10, 14), (x + 30, 40), (x + 20, 40)], s),
                fill=(91, 98, 108, 110),
            )

    save(supersampled((1100, 150), chassis_painter), "Boss/boss_chassis.png")
    save(supersampled((420, 84), name_painter), "Boss/boss_name_tab.png")
    save(angular_track((1024, 56), 8), "Boss/boss_hp_track.png")
    save(trapezoid_fill((1024, 28), (241, 82, 94), (210, 47, 66)), "Boss/boss_hp_fill.png")
    save(angular_track((1024, 44), 7, border=2), "Boss/boss_cost_track.png")
    save(trapezoid_fill((1024, 22), (69, 220, 235), (25, 178, 208)), "Boss/boss_cost_fill.png")


def build_pause() -> None:
    def plate_painter(canvas, draw, s):
        points = [(24, 6), (168, 6), (186, 24), (186, 168), (168, 186), (24, 186), (6, 168), (6, 24)]
        inner = [(29, 15), (163, 15), (177, 29), (177, 163), (163, 177), (29, 177), (15, 163), (15, 29)]
        draw.polygon(scaled_points(points, s), fill=(4, 7, 11, 242))
        draw.line(scaled_points(points + [points[0]], s), fill=SILVER, width=3 * s)
        draw.polygon(scaled_points(inner, s), fill=(22, 26, 32, 235))
        draw.line(scaled_points(inner + [inner[0]], s), fill=(106, 114, 121, 195), width=s)

    def glyph_painter(canvas, draw, s):
        for x in (69, 111):
            draw.rounded_rectangle(
                (x * s, 54 * s, (x + 16) * s, 138 * s),
                radius=3 * s,
                fill=PEARL,
            )

    save(supersampled((192, 192), plate_painter), "System/pause_plate.png")
    save(supersampled((192, 192), glyph_painter), "System/pause_glyph.png")


def build_action_plate() -> None:
    size = (512, 512)
    plate = ring_gradient(size, (256, 256), 222, (29, 34, 42, 252), (12, 16, 22, 252))

    def rim_painter(canvas, draw, s):
        cx = cy = 256 * s
        for radius, width, color in (
            (246, 13, (2, 4, 8, 235)),
            (237, 6, (214, 218, 219, 245)),
            (226, 8, (42, 47, 55, 248)),
            (216, 3, (183, 190, 194, 218)),
        ):
            r = radius * s
            draw.ellipse((cx - r, cy - r, cx + r, cy + r), outline=color, width=width * s)
        draw.arc(
            (37 * s, 37 * s, 475 * s, 475 * s),
            start=205,
            end=330,
            fill=(249, 249, 244, 115),
            width=3 * s,
        )

    plate.alpha_composite(supersampled(size, rim_painter))

    def arc_painter(canvas, draw, s):
        box = (29 * s, 29 * s, 483 * s, 483 * s)
        draw.arc(box, start=195, end=355, fill=(32, 213, 235, 248), width=12 * s)
        draw.arc(box, start=3, end=42, fill=(32, 213, 235, 248), width=12 * s)
        glow = Image.new("RGBA", canvas.size, TRANSPARENT)
        gd = ImageDraw.Draw(glow, "RGBA")
        gd.arc(box, start=195, end=355, fill=(41, 218, 239, 110), width=25 * s)
        gd.arc(box, start=3, end=42, fill=(41, 218, 239, 110), width=25 * s)
        canvas.alpha_composite(glow.filter(ImageFilter.GaussianBlur(4 * s)))

    def cooldown_painter(canvas, draw, s):
        draw.ellipse((29 * s, 29 * s, 483 * s, 483 * s), fill=(0, 3, 7, 166))

    save(plate, "Action/action_plate.png")
    save(supersampled(size, arc_painter), "Action/action_ready_arc.png")
    save(supersampled(size, cooldown_painter), "Action/action_cooldown_disc.png")


def build_action_glyphs() -> None:
    size = (512, 512)

    def weapon_swap_painter(canvas, draw, s):
        # Compact carbine plus a two-way exchange mark.  At the smaller
        # top-left action-button size the weapon silhouette remains primary.
        carbine = [
            (89, 218), (126, 196), (218, 196), (244, 177), (304, 177),
            (325, 195), (401, 195), (428, 215), (406, 235), (324, 235),
            (288, 255), (238, 255), (215, 302), (170, 302), (181, 253),
            (126, 253), (91, 238),
        ]
        draw.polygon(scaled_points(carbine, s), fill=PEARL)
        draw.polygon(scaled_points([(105, 218), (57, 188), (57, 266), (105, 238)], s), fill=PEARL)
        draw.polygon(scaled_points([(245, 252), (290, 252), (274, 322), (226, 322)], s), fill=PEARL)
        # Swap arrows are deliberately slimmer than the gun face.
        draw.line(scaled_points([(132, 139), (310, 139), (344, 161)], s), fill=PEARL, width=13 * s)
        draw.polygon(scaled_points([(344, 161), (310, 121), (310, 201)], s), fill=PEARL)
        draw.line(scaled_points([(376, 350), (198, 350), (164, 328)], s), fill=PEARL, width=13 * s)
        draw.polygon(scaled_points([(164, 328), (198, 288), (198, 368)], s), fill=PEARL)

    def ultimate_painter(canvas, draw, s):
        # High-energy impact/starburst, reserved for the upper-right ultimate.
        center = (256, 256)
        points = []
        radii = (142, 56, 102, 47, 136, 52, 100, 48, 143, 58, 104, 48, 128, 50, 96, 48)
        for index, radius in enumerate(radii):
            angle = -math.pi / 2 + index * math.pi / 8
            points.append((center[0] + math.cos(angle) * radius, center[1] + math.sin(angle) * radius))
        draw.polygon(scaled_points(points, s), fill=PEARL)
        draw.polygon(
            scaled_points([(90, 244), (139, 226), (126, 258), (84, 267)], s),
            fill=(236, 239, 236, 235),
        )
        draw.polygon(scaled_points([(369, 141), (381, 180), (421, 192), (382, 207), (368, 246), (354, 207), (314, 192), (354, 178)], s), fill=PEARL)

    def dash_painter(canvas, draw, s):
        for offset in (0, 58, 116):
            points = [(132 + offset, 146), (248 + offset, 256), (132 + offset, 366), (174 + offset, 256)]
            draw.polygon(scaled_points(points, s), fill=PEARL)
        for y, length in ((205, 72), (256, 92), (307, 70)):
            draw.rounded_rectangle((55 * s, (y - 9) * s, (55 + length) * s, (y + 9) * s), radius=8 * s, fill=PEARL)

    def attack_painter(canvas, draw, s):
        # Large primary ranged-attack face: unmistakable carbine and muzzle flash.
        body = [
            (73, 232), (121, 193), (263, 193), (292, 164), (354, 164),
            (377, 190), (430, 190), (448, 213), (426, 237), (357, 237),
            (320, 267), (244, 267), (218, 335), (157, 335), (175, 267),
            (117, 267), (73, 251),
        ]
        draw.polygon(scaled_points(body, s), fill=PEARL)
        draw.polygon(scaled_points([(104, 210), (35, 174), (35, 291), (106, 259)], s), fill=PEARL)
        draw.polygon(scaled_points([(254, 262), (316, 262), (290, 365), (228, 365)], s), fill=PEARL)
        draw.rounded_rectangle((274 * s, 179 * s, 337 * s, 199 * s), radius=7 * s, fill=(18, 23, 29, 255))
        # Three recoil rays and a compact impact diamond keep the icon readable.
        for y, length in ((183, 44), (214, 62), (245, 44)):
            draw.rounded_rectangle(((456 - length) * s, (y - 6) * s, 456 * s, (y + 6) * s), radius=5 * s, fill=PEARL)
        flash = [(468, 189), (478, 207), (501, 214), (479, 222), (469, 242), (460, 222), (438, 214), (458, 206)]
        draw.polygon(scaled_points(flash, s), fill=PEARL)

    save(supersampled(size, weapon_swap_painter), "Action/glyph_weapon_swap.png")
    save(supersampled(size, ultimate_painter), "Action/glyph_ultimate.png")
    save(supersampled(size, dash_painter), "Action/glyph_dash.png")
    save(supersampled(size, attack_painter), "Action/glyph_attack.png")


SUMMON_GEOMETRY = {
    1: {
        "size": (384, 340),
        "outer": [(74, 5), (338, 5), (378, 43), (335, 326), (45, 326), (5, 262), (45, 43)],
        "inner": [(88, 28), (321, 28), (351, 55), (316, 296), (66, 296), (30, 250), (65, 56)],
    },
    2: {
        "size": (360, 260),
        "outer": [(62, 4), (323, 4), (357, 39), (322, 248), (39, 248), (5, 207), (42, 39)],
        "inner": [(77, 25), (305, 25), (331, 52), (302, 222), (60, 222), (31, 194), (61, 52)],
    },
    3: {
        "size": (340, 242),
        "outer": [(59, 4), (304, 4), (337, 36), (303, 230), (38, 230), (5, 194), (41, 36)],
        "inner": [(74, 24), (286, 24), (312, 49), (285, 205), (58, 205), (31, 181), (59, 49)],
    },
}


def polygon_mask(size: tuple[int, int], points) -> Image.Image:
    return supersampled(
        size,
        lambda canvas, draw, s: draw.polygon(scaled_points(points, s), fill=(255, 255, 255, 255)),
    )


def draw_summon_frame(slot: int) -> Image.Image:
    geometry = SUMMON_GEOMETRY[slot]
    size = geometry["size"]
    outer = geometry["outer"]
    inner = geometry["inner"]

    def painter(canvas, draw, s):
        draw.polygon(scaled_points(outer, s), fill=(5, 8, 12, 250))
        # The frame is an overlay, so its portrait aperture must be genuinely
        # transparent.  Unity's child Mask supplies clipping/background.
        draw.polygon(scaled_points(inner, s), fill=TRANSPARENT)
        draw.line(scaled_points(outer + [outer[0]], s), fill=(14, 18, 24, 255), width=12 * s, joint="curve")
        draw.line(scaled_points(outer + [outer[0]], s), fill=SILVER, width=4 * s, joint="curve")
        draw.line(scaled_points(inner + [inner[0]], s), fill=(71, 80, 88, 242), width=8 * s, joint="curve")
        draw.line(scaled_points(inner + [inner[0]], s), fill=(211, 216, 216, 220), width=2 * s, joint="curve")
        if slot == 1:
            draw.line(scaled_points([(74, 6), (338, 6)], s), fill=GOLD, width=7 * s)

    return supersampled(size, painter)


def draw_summon_accent(slot: int) -> Image.Image:
    size = SUMMON_GEOMETRY[slot]["size"]

    def painter(canvas, draw, s):
        if slot == 1:
            draw.line(scaled_points([(44, 205), (17, 251), (37, 288)], s), fill=CYAN, width=10 * s, joint="curve")
            draw.line(scaled_points([(330, 35), (358, 57), (341, 106)], s), fill=GOLD, width=7 * s)
        elif slot == 2:
            draw.arc((238 * s, 36 * s, 344 * s, 180 * s), start=276, end=70, fill=CYAN, width=15 * s)
            for y in (49, 83, 117, 151):
                draw.line(scaled_points([(322, y), (341, y + 4)], s), fill=CYAN_LIGHT, width=3 * s)
        else:
            draw.polygon(scaled_points([(285, 197), (325, 175), (311, 215)], s), fill=ORANGE)
            for index in range(4):
                x = 118 + index * 43
                draw.polygon(scaled_points([(x, 209), (x + 27, 209), (x + 10, 229), (x - 15, 229)], s), fill=(116, 123, 129, 205))

    return supersampled(size, painter)


def extract_approved_summon(sheet: Image.Image, slot: int) -> Image.Image:
    """Extract exact illustration pixels while excluding all baked HUD pixels."""

    region = SUMMON_SOURCE_REGIONS[slot]
    left, top, right, bottom = region["box"]
    crop = sheet.crop((left, top, right, bottom)).convert("RGBA")
    local_aperture = [(x - left, y - top) for x, y in region["aperture"]]
    aperture = Image.new("L", crop.size, 0)
    ImageDraw.Draw(aperture).polygon(local_aperture, fill=255)

    rgba = np.asarray(crop, dtype=np.uint8).copy()
    # Keep the approved aperture's black matte so the original head scale and
    # silhouette survive.  Only outside-HUD pixels are made transparent.
    rgba[..., 3] = np.asarray(aperture, dtype=np.uint8)
    extracted = zero_transparent_rgb(Image.fromarray(rgba, "RGBA"))
    return zero_transparent_rgb(extracted.resize((512, 512), Image.Resampling.LANCZOS))


def tight_fit(image: Image.Image, size: tuple[int, int], pad: int = 5) -> Image.Image:
    bbox = alpha_bbox(image, threshold=3)
    if bbox is None:
        raise ValueError("Cannot fit empty image")
    crop = image.crop(bbox)
    usable = (size[0] - 2 * pad, size[1] - 2 * pad)
    scale = min(usable[0] / crop.width, usable[1] / crop.height)
    resized = crop.resize((max(1, round(crop.width * scale)), max(1, round(crop.height * scale))), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", size, TRANSPARENT)
    canvas.alpha_composite(resized, ((size[0] - resized.width) // 2, (size[1] - resized.height) // 2))
    return zero_transparent_rgb(canvas)


def draw_cost_tab(size: tuple[int, int]) -> Image.Image:
    width, height = size

    def painter(canvas, draw, s):
        points = [(14, 3), (width - 4, 3), (width - 13, height - 3), (3, height - 3), (0, 18)]
        inner = [(18, 10), (width - 13, 10), (width - 20, height - 11), (11, height - 11), (8, 21)]
        draw.polygon(scaled_points(points, s), fill=(5, 8, 12, 247))
        draw.line(scaled_points(points + [points[0]], s), fill=SILVER, width=3 * s)
        draw.polygon(scaled_points(inner, s), fill=(24, 29, 36, 232))

    return supersampled(size, painter)


def build_summons(refs: dict[str, Path]) -> None:
    approved_sheet = load_rgba(refs["summon_sheet"])
    for slot in (1, 2, 3):
        size = SUMMON_GEOMETRY[slot]["size"]
        mask = polygon_mask(size, SUMMON_GEOMETRY[slot]["inner"])
        save(mask, f"Summon/summon_mask_s{slot}.png")
        save(draw_summon_frame(slot), f"Summon/summon_frame_s{slot}.png")
        save(draw_summon_accent(slot), f"Summon/summon_accent_s{slot}.png")
        tab_size = (128, 72) if slot == 1 else (112, 64)
        save(draw_cost_tab(tab_size), f"Summon/summon_cost_tab_s{slot}.png")
        save(extract_approved_summon(approved_sheet, slot), f"Summon/summon_portrait_s{slot}.png")


def build_joystick() -> None:
    size = (512, 512)
    # The input geometry and footprint stay unchanged; the translucent graphite
    # value is raised just enough to survive white Olympus floors and columns.
    base = ring_gradient(size, (256, 256), 226, (17, 21, 28, 88), (29, 34, 41, 54))

    def rings_painter(canvas, draw, s):
        cx = cy = 256 * s
        # Dark support strokes keep the silver rings readable on either a dark
        # floor or a pale sky without turning the control into a black disc.
        for radius, support_width, support_color, width, color in (
            (230, 9, (5, 8, 12, 166), 4, (207, 213, 216, 176)),
            (208, 6, (6, 9, 14, 142), 2, (151, 160, 168, 150)),
            (126, 7, (5, 8, 12, 156), 3, (150, 159, 167, 166)),
        ):
            r = radius * s
            draw.ellipse(
                (cx - r, cy - r, cx + r, cy + r),
                outline=support_color,
                width=support_width * s,
            )
            draw.ellipse((cx - r, cy - r, cx + r, cy + r), outline=color, width=width * s)
        arrows = {
            "top": [(240, 48), (256, 26), (272, 48), (256, 41)],
            "bottom": [(240, 464), (256, 486), (272, 464), (256, 471)],
            "left": [(48, 240), (26, 256), (48, 272), (41, 256)],
            "right": [(464, 240), (486, 256), (464, 272), (471, 256)],
        }
        for points in arrows.values():
            scaled = scaled_points(points, s)
            draw.line(
                scaled + [scaled[0]],
                fill=(4, 7, 11, 205),
                width=7 * s,
                joint="curve",
            )
            draw.polygon(scaled, fill=(235, 239, 238, 228))

    def knob_painter(canvas, draw, s):
        cx = cy = 96 * s
        draw.ellipse((7 * s, 7 * s, 185 * s, 185 * s), fill=(4, 7, 11, 205), outline=(23, 27, 34, 245), width=10 * s)
        draw.ellipse((19 * s, 19 * s, 173 * s, 173 * s), fill=(24, 29, 36, 225), outline=(119, 127, 134, 165), width=3 * s)

    save(base, "Joystick/joystick_base_glass.png")
    save(supersampled(size, rings_painter), "Joystick/joystick_ring_ticks.png")
    save(supersampled((192, 192), knob_painter), "Joystick/joystick_knob.png")


def player_portrait_mask() -> Image.Image:
    points = [(62, 8), (194, 8), (246, 74), (218, 224), (67, 246), (8, 180), (23, 71)]
    return polygon_mask((256, 256), points)


def build_player(refs: dict[str, Path]) -> None:
    portrait = load_rgba(refs["player"])
    save(tight_fit(portrait, (512, 512), pad=0), "Player/player_portrait.png")
    save(player_portrait_mask(), "Player/player_portrait_mask.png")

    def portrait_frame_painter(canvas, draw, s):
        outer = [(60, 4), (198, 4), (251, 71), (222, 229), (65, 252), (3, 183), (19, 68)]
        middle = [(65, 14), (191, 14), (239, 76), (213, 218), (70, 240), (15, 179), (29, 74)]
        inner = [(70, 24), (185, 24), (228, 80), (204, 207), (75, 229), (28, 174), (39, 80)]
        draw.line(scaled_points(outer + [outer[0]], s), fill=(4, 7, 11, 250), width=13 * s, joint="curve")
        draw.line(scaled_points(middle + [middle[0]], s), fill=SILVER, width=5 * s, joint="curve")
        draw.line(scaled_points(inner + [inner[0]], s), fill=(89, 98, 106, 230), width=3 * s, joint="curve")

    def chassis_painter(canvas, draw, s):
        outer = [(110, 18), (1320, 18), (1392, 52), (1350, 176), (92, 176), (4, 104)]
        inner = [(125, 34), (1298, 34), (1368, 61), (1334, 158), (106, 158), (28, 102)]
        draw.polygon(scaled_points(outer, s), fill=(5, 8, 12, 222))
        draw.line(scaled_points(outer + [outer[0]], s), fill=(188, 195, 198, 220), width=4 * s, joint="curve")
        draw.polygon(scaled_points(inner, s), fill=(20, 24, 30, 202))
        draw.line(scaled_points([(145, 43), (1287, 43)], s), fill=(242, 243, 238, 54), width=2 * s)
        for x in (1010, 1212):
            draw.line(scaled_points([(x, 42), (x - 20, 154)], s), fill=(124, 132, 138, 94), width=2 * s)

    def state_pips_painter(canvas, draw, s):
        for index in range(2):
            x = 24 + index * 44
            draw.polygon(scaled_points([(x + 12, 8), (x + 38, 8), (x + 24, 55), (0 + x, 55)], s), fill=ORANGE)

    def mode_glyph_painter(canvas, draw, s):
        cx = cy = 64
        draw.ellipse((22 * s, 22 * s, 106 * s, 106 * s), outline=PEARL, width=3 * s)
        for turn in range(4):
            angle = turn * math.pi / 2
            p1 = (cx + math.cos(angle) * 22, cy + math.sin(angle) * 22)
            p2 = (cx + math.cos(angle) * 58, cy + math.sin(angle) * 58)
            draw.line(scaled_points([p1, p2], s), fill=PEARL, width=4 * s)
        draw.ellipse((58 * s, 58 * s, 70 * s, 70 * s), fill=CYAN_LIGHT)

    def ammo_plate_painter(canvas, draw, s):
        w, h = 320, 112
        outer = [(22, 4), (306, 4), (318, 25), (293, 108), (14, 108), (2, 88)]
        inner = [(28, 13), (294, 13), (306, 29), (284, 98), (21, 98), (12, 84)]
        draw.polygon(scaled_points(outer, s), fill=(5, 8, 12, 242))
        draw.line(scaled_points(outer + [outer[0]], s), fill=SILVER, width=3 * s)
        draw.polygon(scaled_points(inner, s), fill=(23, 27, 34, 226))

    def bullet_painter(canvas, draw, s):
        for index in range(3):
            x = 23 + index * 36
            draw.polygon(scaled_points([(x, 43), (x + 12, 18), (x + 24, 43), (x + 24, 110), (x, 110)], s), fill=PEARL)

    def separator_painter(canvas, draw, s):
        draw.line(scaled_points([(17, 12), (17, 100)], s), fill=(185, 191, 194, 165), width=2 * s)

    save(supersampled((256, 256), portrait_frame_painter), "Player/player_portrait_frame.png")
    save(supersampled((1400, 200), chassis_painter), "Player/player_chassis.png")
    save(angular_track((1024, 48), 7, border=2), "Player/player_hp_track.png")
    save(trapezoid_fill((1024, 26), (255, 255, 252), (225, 227, 225)), "Player/player_hp_fill.png")
    save(angular_track((1024, 42), 7, border=2), "Player/player_cost_track.png")

    cost_fill = trapezoid_fill((1024, 24), (46, 216, 237), (22, 173, 210))
    cost_data = np.asarray(cost_fill, dtype=np.uint8).copy()
    for x in range(194, 1024, 205):
        cost_data[:, max(0, x - 6) : min(1024, x + 6), :] = 0
    save(Image.fromarray(cost_data, "RGBA"), "Player/player_cost_fill_segmented.png")
    save(supersampled((128, 64), state_pips_painter), "Player/player_state_pips.png")
    save(supersampled((128, 128), mode_glyph_painter), "Player/player_mode_glyph.png")
    save(supersampled((320, 112), ammo_plate_painter), "Player/player_ammo_plate_compact.png")
    save(supersampled((128, 128), bullet_painter), "Player/player_bullet_glyph.png")
    save(supersampled((32, 112), separator_painter), "Player/player_ammo_separator.png")


def build_reticle() -> None:
    size = (192, 192)

    def dot_painter(canvas, draw, s):
        cx = cy = 96 * s
        for radius, color in (
            (8, (2, 6, 10, 225)),
            (5, (245, 248, 244, 252)),
            (3, (73, 221, 237, 255)),
            (1, (255, 255, 250, 255)),
        ):
            r = radius * s
            draw.ellipse((cx - r, cy - r, cx + r, cy + r), fill=color)

    def needle_painter(canvas, draw, s):
        outer = [(37, 91), (72, 91), (82, 96), (72, 101), (37, 101)]
        inner = [(40, 93), (72, 93), (78, 96), (72, 99), (40, 99)]
        tip = [(70, 94), (80, 96), (70, 98)]
        draw.polygon(scaled_points(outer, s), fill=(2, 7, 11, 220))
        draw.polygon(scaled_points(inner, s), fill=PEARL)
        draw.polygon(scaled_points(tip, s), fill=CYAN_LIGHT)

    save(supersampled(size, dot_painter), "Reticle/reticle_precision_dot.png")
    save(supersampled(size, needle_painter), "Reticle/reticle_precision_needle.png")


def checker(size: tuple[int, int], cell: int = 24) -> Image.Image:
    image = Image.new("RGBA", size, (24, 29, 36, 255))
    draw = ImageDraw.Draw(image)
    for y in range(0, size[1], cell):
        for x in range(0, size[0], cell):
            if (x // cell + y // cell) % 2:
                draw.rectangle((x, y, x + cell - 1, y + cell - 1), fill=(34, 40, 49, 255))
    return image


def composite_action(glyph_name: str, size: int, ready: bool = False) -> Image.Image:
    plate = load_rgba(RUNTIME / "Action" / "action_plate.png").resize((size, size), Image.Resampling.LANCZOS)
    glyph = load_rgba(RUNTIME / "Action" / glyph_name).resize((size, size), Image.Resampling.LANCZOS)
    composed = Image.new("RGBA", (size, size), TRANSPARENT)
    composed.alpha_composite(plate)
    if ready:
        arc = load_rgba(RUNTIME / "Action" / "action_ready_arc.png").resize((size, size), Image.Resampling.LANCZOS)
        composed.alpha_composite(arc)
    composed.alpha_composite(glyph)
    return composed


def make_qa(refs: dict[str, Path]) -> None:
    QA.mkdir(parents=True, exist_ok=True)

    action_sheet = checker((1320, 380), 24)
    action_specs = [
        ("glyph_weapon_swap.png", 230, True),
        ("glyph_ultimate.png", 230, False),
        ("glyph_dash.png", 230, False),
        ("glyph_attack.png", 310, False),
    ]
    x = 28
    for name, size, ready in action_specs:
        action_sheet.alpha_composite(composite_action(name, size, ready), (x, (380 - size) // 2))
        x += size + 38
    action_sheet.save(QA / "action_atomic_contact_sheet.png", optimize=True)

    summon_sheet = checker((1320, 410), 24)
    x = 38
    for slot in (1, 2, 3):
        geometry = SUMMON_GEOMETRY[slot]
        size = geometry["size"]
        portrait = load_rgba(RUNTIME / "Summon" / f"summon_portrait_s{slot}.png")
        portrait = portrait.resize(size, Image.Resampling.LANCZOS)
        mask = load_rgba(RUNTIME / "Summon" / f"summon_mask_s{slot}.png").getchannel("A")
        composed = Image.new("RGBA", size, TRANSPARENT)
        # Preview the Unity Mask contract without replacing the portrait's own
        # alpha (which would turn its transparent RGB into an opaque black veil).
        backing = Image.new("RGBA", size, (8, 12, 17, 255))
        backing.putalpha(mask)
        composed.alpha_composite(backing)
        portrait_alpha = np.minimum(
            np.asarray(portrait.getchannel("A"), dtype=np.uint8),
            np.asarray(mask, dtype=np.uint8),
        )
        portrait.putalpha(Image.fromarray(portrait_alpha, "L"))
        composed.alpha_composite(portrait)
        composed.alpha_composite(load_rgba(RUNTIME / "Summon" / f"summon_frame_s{slot}.png"))
        composed.alpha_composite(load_rgba(RUNTIME / "Summon" / f"summon_accent_s{slot}.png"))
        tab = load_rgba(RUNTIME / "Summon" / f"summon_cost_tab_s{slot}.png")
        composed.alpha_composite(tab, (15, size[1] - tab.height - 9))
        summon_sheet.alpha_composite(composed, (x, 32 + (340 - size[1]) // 2))
        x += size[0] + 38
    summon_sheet.save(QA / "summon_atomic_contact_sheet.png", optimize=True)

    hud_sheet = checker((1800, 1120), 24)
    objective = Image.new("RGBA", (960, 199), TRANSPARENT)
    for name in ("objective_facets_top.png", "objective_body.png", "objective_facets_bottom.png"):
        objective.alpha_composite(load_rgba(RUNTIME / "Objective" / name))
    hud_sheet.alpha_composite(objective.resize((768, 159), Image.Resampling.LANCZOS), (24, 30))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Boss" / "boss_name_tab.png"), (900, 30))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Boss" / "boss_hp_track.png").resize((780, 43), Image.Resampling.LANCZOS), (900, 128))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Boss" / "boss_hp_fill.png").resize((620, 22), Image.Resampling.LANCZOS), (914, 139))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Boss" / "boss_cost_track.png").resize((780, 34), Image.Resampling.LANCZOS), (900, 184))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Boss" / "boss_cost_fill.png").resize((520, 18), Image.Resampling.LANCZOS), (914, 191))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "System" / "pause_plate.png"), (920, 275))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "System" / "pause_glyph.png"), (920, 275))
    joystick = load_rgba(RUNTIME / "Joystick" / "joystick_base_glass.png")
    joystick.alpha_composite(load_rgba(RUNTIME / "Joystick" / "joystick_ring_ticks.png"))
    joystick.alpha_composite(load_rgba(RUNTIME / "Joystick" / "joystick_knob.png"), (160, 160))
    hud_sheet.alpha_composite(joystick, (1170, 250))

    player = load_rgba(RUNTIME / "Player" / "player_chassis.png")
    hud_sheet.alpha_composite(player.resize((1260, 180), Image.Resampling.LANCZOS), (380, 820))
    portrait = load_rgba(RUNTIME / "Player" / "player_portrait.png").resize((153, 153), Image.Resampling.LANCZOS)
    portrait_mask = load_rgba(RUNTIME / "Player" / "player_portrait_mask.png").resize((153, 153), Image.Resampling.LANCZOS).getchannel("A")
    portrait.putalpha(Image.fromarray(np.minimum(np.asarray(portrait.getchannel("A")), np.asarray(portrait_mask)).astype(np.uint8), "L"))
    hud_sheet.alpha_composite(portrait, (392, 833))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_portrait_frame.png").resize((153, 153), Image.Resampling.LANCZOS), (392, 833))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_hp_track.png").resize((672, 32), Image.Resampling.LANCZOS), (610, 856))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_hp_fill.png").resize((652, 20), Image.Resampling.LANCZOS), (620, 862))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_cost_track.png").resize((672, 28), Image.Resampling.LANCZOS), (610, 901))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_cost_fill_segmented.png").resize((652, 16), Image.Resampling.LANCZOS), (620, 907))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_state_pips.png").resize((86, 24), Image.Resampling.LANCZOS), (1190, 934))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_mode_glyph.png").resize((64, 64), Image.Resampling.LANCZOS), (1302, 850))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_ammo_plate_compact.png").resize((194, 68), Image.Resampling.LANCZOS), (1382, 848))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_bullet_glyph.png").resize((30, 54), Image.Resampling.LANCZOS), (1400, 855))
    hud_sheet.alpha_composite(load_rgba(RUNTIME / "Player" / "player_ammo_separator.png").resize((20, 64), Image.Resampling.LANCZOS), (1450, 850))

    reticle = Image.new("RGBA", (192, 192), TRANSPARENT)
    needle = load_rgba(RUNTIME / "Reticle" / "reticle_precision_needle.png")
    for turn in range(4):
        reticle.alpha_composite(needle.rotate(turn * 90, resample=Image.Resampling.BICUBIC))
    reticle.alpha_composite(load_rgba(RUNTIME / "Reticle" / "reticle_precision_dot.png"))
    hud_sheet.alpha_composite(reticle, (80, 810))
    hud_sheet.save(QA / "hud_atomic_contact_sheet.png", optimize=True)


def validate_and_manifest(refs: dict[str, Path]) -> None:
    rows = []
    report = {"reference": {}, "assets": []}
    for key, path in refs.items():
        report["reference"][key] = {
            "path": path.relative_to(OUT).as_posix(),
            "sha256": sha256(path),
            "dimensions": Image.open(path).size,
        }
    for relative, dimensions in EXPECTED.items():
        path = RUNTIME / relative
        if not path.exists():
            raise FileNotFoundError(path)
        image = Image.open(path).convert("RGBA")
        if image.size != dimensions:
            raise ValueError(f"{relative}: {image.size} != {dimensions}")
        bbox = alpha_bbox(image)
        if bbox is None:
            raise ValueError(f"{relative}: empty alpha")
        data = np.asarray(image, dtype=np.uint8)
        transparent_rgb = int(np.count_nonzero((data[..., 3] == 0) & np.any(data[..., :3] != 0, axis=2)))
        if transparent_rgb:
            raise ValueError(f"{relative}: {transparent_rgb} non-zero transparent RGB pixels")
        # Flag visible chroma-key green contamination, not sub-8-bit RGB noise in
        # the feathered edge of an otherwise blue/cyan premultiplied glow.
        green_fringe = int(
            np.count_nonzero(
                (data[..., 3] > 16)
                & (data[..., 1] > 100)
                & (data[..., 1] > data[..., 0] * 1.65)
                & (data[..., 1] > data[..., 2] * 1.65)
            )
        )
        if green_fringe:
            raise ValueError(f"{relative}: {green_fringe} chroma-like green pixels")
        digest = sha256(path)
        rows.append(f"| `{relative}` | {dimensions[0]}x{dimensions[1]} | `{bbox}` | `{digest}` |")
        report["assets"].append(
            {
                "path": relative,
                "dimensions": dimensions,
                "alpha_bbox_gt3": bbox,
                "transparent_rgb_pixels": transparent_rgb,
                "green_fringe_pixels": green_fringe,
                "sha256": digest,
            }
        )

    manifest = f"""# Celestial HUD Target atomic asset pack

Deterministic reconstruction of the approved dark-angular combat HUD.  The
1672x941 golden composite is an optical/placement reference, not a runtime
texture.  Every runtime label, value, cooldown, and state remains a separate
Unity text or state layer.

## Immutable reference and approved exceptions

- Golden: `Source/Reference/golden_composite_1672x941.png`
- Objective candidate: `Source/Reference/golden_objective_composite_1672x941.png`
- Objective lower facets: approved 40-design-pixel silhouette depth retained
  (about a 45px alpha bbox), but neutral smoke alpha 36/42 prevents a broad
  cyan surface on bright stages.
- Joystick legibility: unchanged 512px footprint and input geometry, with a
  translucent graphite base plus dark-supported silver travel rings and ticks.
- Precision reticle: newly drawn as `dot + 4 rotated needle` at true screen center.
- Player status: connected dark-angular silhouette retained, but HP/cost rails
  expand to 672 design pixels; `RANGED` text is removed, mode is a 64px glyph,
  and ammo is a compact 194x68 chip.
- Summon identity: exact illustration pixels are fixed-cropped from
  `Source/Reference/approved_summon_vertical_stack_v2_941x1672.png`; no new AI
  interpretation or palette remap is applied.  Runtime frame, accent, cost tab,
  state arc and text remain separate layers.

## Unity 2560x1440 placement contract

| Group | Rect / rule |
|---|---|
| Objective | `(0,327,806,167)`; body + top facets + bottom facets + runtime TMP |
| Boss | overall `(796,52,1056,132)`; reference strong-pixel bbox `(827,61,945,126)` |
| Pause visual | `(2402,44,103,96)` inside a minimum 160x160 hit target |
| Summon 1 | `(2206,173,297,259)` |
| Summon 2 | `(2212,430,276,197)` |
| Summon 3 | `(2214,640,260,185)` |
| Weapon swap | `(1991,928.5,208,208)` |
| Ultimate | `(2229,891.5,208,208)` |
| Dodge | `(1909,1137.5,208,208)` |
| Basic attack | `(2167,1120,260,260)` |
| Joystick visual | `(190,966,296,305)`; activation target >=381x381 |
| Player composite | `(686,1245,1182,170)` |
| Player portrait | `(686,1262,153,153)` |
| Player HP track/fill | `(888,1307,672,32)` / `(898,1314,652,20)` |
| Player cost track/fill | `(888,1347,672,28)` / `(898,1353,652,16)` |
| Tiny mode glyph | `(1580,1294,64,64)`; no mode text |
| Compact ammo | `(1654,1290,194,68)` |
| Precision reticle | centered `(1280,720)`, 112x112 composition canvas |

## Unity hierarchy contract

The canvas uses `Scale With Screen Size`, reference resolution `2560x1440`,
and `Match Width Or Height = 0.5`.  Omit the legacy mission-timer node.

```text
CombatHudRoot
+-- ObjectivePanel [anchor top-left]
|   +-- FacetsTop / Body / FacetsBottom / ObjectiveTMP
+-- BossPanel [anchor top-center]
|   +-- Chassis / NameTab / NameTMP
|   +-- HpTrack / HpFill / HpValueTMP
|   +-- CostTrack / CostFill / CostValueTMP
+-- PauseHitTarget [anchor top-right, >=160x160]
|   +-- Plate / Glyph
+-- SummonStack [anchor right]
|   +-- Slot1 / Slot2 / Slot3
|       +-- PortraitMask + Portrait / Frame / Accent / CostTab / CostTMP
+-- ActionCluster [anchor bottom-right]
|   +-- WeaponSwapHit [carbine + swap arrows]
|   +-- UltimateHit [impact starburst]
|   +-- DashHit [chevrons]
|   +-- RangedAttackHit [large carbine + muzzle flash]
|       +-- Plate / ReadyArc / CooldownDisc / Glyph / CooldownTMP
+-- JoystickActivation [anchor bottom-left, >=381x381]
|   +-- BaseGlass / RingTicks / Knob
+-- PlayerStatus [anchor bottom-center]
|   +-- Chassis / PortraitMask + Portrait / PortraitFrame
|   +-- HpTrack / HpFill / HpValueTMP
|   +-- CostTrack / CostFillSegmented / StatePips
|   +-- ModeGlyph
|   +-- AmmoPlate / BulletGlyph / Separator / AmmoTMP
+-- PrecisionReticle [anchor true center]
    +-- Needle0 / Needle90 / Needle180 / Needle270 / Dot
```

## Atomic runtime assets

| File | Dimensions | Alpha bbox | SHA-256 |
|---|---:|---|---|
{chr(10).join(rows)}

## Composition rules

- Frames, masks, fills, glyphs, and portraits are separate state boundaries.
- All visual children use `raycastTarget=false`; only explicit hit-target
  parents receive input.
- Action buttons share `Action/action_plate.png`; primary attack scales the
  same plate uniformly rather than using a second generated surface.
- Summon portraits are children of their slot mask; frame, accent, cost tab,
  and runtime text render above the mask.
- Meter fills use horizontal fill/clip.  Tracks render above fills.
- The precision needle keeps its full 192x192 pivot canvas and is instantiated
  at rotations 0/90/180/270.

## Rebuild

From `Assets/`:

```powershell
uv run --with pillow --with numpy python `
  _Game/UI/CombatHud/Art/CelestialHudTarget/build_target_asset_pack.py --force
```
"""
    (OUT / "manifest.md").write_text(manifest, encoding="utf-8", newline="\n")
    (QA / "qa_report.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8", newline="\n")
    hash_lines = [f"{item['sha256']}  Runtime/{item['path']}" for item in report["assets"]]
    (QA / "hash_manifest.sha256").write_text("\n".join(hash_lines) + "\n", encoding="utf-8", newline="\n")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--force", action="store_true", help="overwrite generated runtime and QA outputs")
    args = parser.parse_args()

    existing = [relative for relative in EXPECTED if (RUNTIME / relative).exists()]
    if existing and not args.force:
        raise FileExistsError("Generated outputs already exist; pass --force to rebuild")

    refs = prepare_references()
    build_objective()
    build_boss()
    build_pause()
    build_action_plate()
    build_action_glyphs()
    build_summons(refs)
    build_joystick()
    build_player(refs)
    build_reticle()
    make_qa(refs)
    validate_and_manifest(refs)
    print(f"Wrote {len(EXPECTED)} runtime sprites to {RUNTIME}")
    print(f"QA: {QA}")


if __name__ == "__main__":
    main()
