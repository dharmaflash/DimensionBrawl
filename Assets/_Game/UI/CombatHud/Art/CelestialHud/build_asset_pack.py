"""Build the review-approved Celestial HUD sprite pack.

The pack is assembled deterministically from the v16/v17 component renders,
the v19 presentation settings, and the project's real summon/player artwork.
No labels, counters, or other runtime text are baked into these sprites.

Run with the bundled Python runtime from the repository Assets directory:

    python _Game/UI/CombatHud/Art/CelestialHud/build_asset_pack.py

The command refuses to overwrite generated PNGs unless ``--force`` is passed.
"""

from __future__ import annotations

import argparse
import hashlib
import math
import shutil
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageEnhance


OUT = Path(__file__).resolve().parent
ASSETS = OUT.parents[4]
PROJECT_ART = OUT.parent / "DimensionHud"
PLAYER_ICON = ASSETS / "_Game" / "UI" / "Apk_Icon" / "Apk_Icon.png"

CONCEPT = (
    Path.home()
    / ".codex"
    / "visualizations"
    / "2026"
    / "08"
    / "13"
    / "019ffa49-5536-79e0-a1ec-de549a4b6e40"
    / "combat-hud-concept"
)
V16 = CONCEPT / "celestial-elements-v16" / "elements"
V17 = CONCEPT / "celestial-elements-v17" / "elements"
FLOW_SOURCE = CONCEPT / "celestial-flow-v21" / "seamless_flow_128x64.png"

TRANSPARENT = (0, 0, 0, 0)

OUTPUT_SPECS = {
    "objective_frame.png": (
        "PGR-inspired top-left mission ribbon: sparse translucent matte navy, "
        "open fade at right; runtime objective text remains separate."
    ),
    "boss_frame.png": "Combined boss HP/cost underlay matching the v19 layout.",
    "boss_hp_track.png": "Upper boss HP rail only.",
    "boss_hp_fill.png": "Runtime-filled boss HP color strip.",
    "boss_cost_track.png": "Lower boss cost rail only.",
    "boss_cost_fill.png": "Runtime-filled boss cost color strip.",
    "pause.png": "Pause button chrome and pause glyph.",
    "summon_s1_frame.png": "Primary summon frame, portrait aperture left transparent.",
    "summon_s1_portrait.png": "Real project summon slot 1 render, copied pixel-for-pixel.",
    "summon_s2_frame.png": "Secondary summon frame, portrait aperture left transparent.",
    "summon_s2_portrait.png": "Real project summon slot 2 render, copied pixel-for-pixel.",
    "summon_s3_frame.png": "Secondary summon frame, portrait aperture left transparent.",
    "summon_s3_portrait.png": "Real project summon slot 3 render, copied pixel-for-pixel.",
    "action_weapon_swap.png": "Weapon switch action button.",
    "action_ultimate.png": "Skill/ultimate action button.",
    "action_dash.png": "Dash/dodge action button.",
    "action_attack_ranged.png": "Primary ranged attack action button.",
    "joystick_base.png": "Low-opacity joystick annulus; center is clear for a moving knob.",
    "joystick_knob.png": "Independent joystick knob.",
    "player_portrait_frame.png": "Player portrait frame only.",
    "player_portrait.png": "Circular portrait derived from the project's Apk_Icon.",
    "player_hp_rail.png": "Player HP frame/track.",
    "player_hp_fill.png": "Runtime-filled player HP color strip.",
    "player_en_rail.png": "Player EN frame/track.",
    "player_en_fill.png": "Runtime-filled player EN color strip.",
    "player_ammo_chip.png": "Ammo counter chrome; runtime icon/text remains separate.",
    "reticle.png": "Compact precision reticle with a center point and four cardinal needles.",
    "Motion/DB_UI_CelestialFlow.png": "Tiny seamless grayscale flow map for shader UV scrolling.",
    "QA/summon_overlay_contact_sheet.png": "QA only: clean frames composited over the real project portraits.",
}


def load_rgba(path: Path) -> Image.Image:
    if not path.exists():
        raise FileNotFoundError(path)
    return Image.open(path).convert("RGBA")


def zero_transparent_rgb(image: Image.Image) -> Image.Image:
    data = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    data[data[..., 3] == 0, :3] = 0
    return Image.fromarray(data, "RGBA")


def alpha_bbox(image: Image.Image, threshold: int = 1) -> tuple[int, int, int, int] | None:
    alpha = image.convert("RGBA").getchannel("A")
    if threshold > 1:
        alpha = alpha.point(lambda value: 255 if value >= threshold else 0)
    return alpha.getbbox()


def draw_objective_ribbon(size: tuple[int, int] = (960, 202)) -> Image.Image:
    """Draw the restrained mission ribbon used by the top-left combat HUD.

    The silhouette follows the information hierarchy of modern mobile action
    HUDs: a large matte field starts at the screen edge and then yields back to
    gameplay through a long alpha fade.  There is deliberately no crest, timer
    cell, full perimeter stroke, bevel, texture, or glow; those treatments made
    the previous objective element read as an ornate widget instead of a
    quickly scannable mission prompt.
    """
    width, height = size
    if size != (960, 202):
        raise ValueError("The production objective ribbon is authored at 960x202")

    supersample = 4
    large_size = (width * supersample, height * supersample)
    mask_large = Image.new("L", large_size, 0)
    mask_draw = ImageDraw.Draw(mask_large)

    # One broad, slightly asymmetric plane.  The right end stays open and is
    # resolved by alpha rather than by a decorative cap or sci-fi bracket.
    polygon = [
        (-12, 23),
        (808, 23),
        (949, 54),
        (960, 129),
        (829, 179),
        (-12, 179),
    ]
    mask_draw.polygon(
        [(x * supersample, y * supersample) for x, y in polygon],
        fill=255,
    )
    mask = mask_large.resize(size, Image.Resampling.LANCZOS)
    mask_data = np.asarray(mask, dtype=np.float32) / 255.0

    # Smoothstep keeps the body optically solid beneath the label, then makes
    # the final quarter genuinely transparent instead of ending in a hard cap.
    xx = np.arange(width, dtype=np.float32)
    fade_t = np.clip((xx - 701.0) / (945.0 - 701.0), 0.0, 1.0)
    smooth = fade_t * fade_t * (3.0 - 2.0 * fade_t)
    mask_data *= (1.0 - smooth)[None, :]

    yy = np.linspace(0.0, 1.0, height, dtype=np.float32)[:, None]
    top_rgb = np.array((16.0, 22.0, 31.0), dtype=np.float32)
    bottom_rgb = np.array((5.0, 8.0, 13.0), dtype=np.float32)
    rgb_rows = top_rgb[None, :] * (1.0 - yy) + bottom_rgb[None, :] * yy

    data = np.zeros((height, width, 4), dtype=np.uint8)
    data[..., :3] = np.rint(rgb_rows[:, None, :]).astype(np.uint8)
    base_alpha = 188.0 - 12.0 * np.abs(yy - 0.48)
    data[..., 3] = np.rint(mask_data * base_alpha).astype(np.uint8)
    ribbon = Image.fromarray(data, "RGBA")

    # A single planar lower shade gives the matte surface enough separation on
    # both bright and dark stages.  It is intentionally not an outline/bevel.
    shade_large = Image.new("RGBA", large_size, TRANSPARENT)
    shade_draw = ImageDraw.Draw(shade_large, "RGBA")
    shade_draw.polygon(
        [
            (0, 121 * supersample),
            (621 * supersample, 115 * supersample),
            (866 * supersample, 153 * supersample),
            (823 * supersample, 179 * supersample),
            (0, 179 * supersample),
        ],
        fill=(0, 0, 0, 42),
    )
    shade = shade_large.resize(size, Image.Resampling.LANCZOS)
    shade_data = np.asarray(shade, dtype=np.uint8).copy()
    shade_alpha = shade_data[..., 3].astype(np.float32)
    shade_alpha *= mask_data
    shade_data[..., 3] = np.rint(shade_alpha).astype(np.uint8)
    ribbon.alpha_composite(Image.fromarray(shade_data, "RGBA"))

    # Two incomplete one-pixel seams retain crisp HUD legibility while leaving
    # the silhouette unboxed.  Their low opacity prevents a neon/glow reading.
    seam_large = Image.new("RGBA", large_size, TRANSPARENT)
    seam_draw = ImageDraw.Draw(seam_large, "RGBA")
    seam_draw.line(
        ((0, 24 * supersample), (662 * supersample, 24 * supersample)),
        fill=(218, 222, 220, 54),
        width=supersample,
    )
    seam_draw.line(
        ((0, 177 * supersample), (482 * supersample, 177 * supersample)),
        fill=(168, 177, 181, 38),
        width=supersample,
    )
    seam = seam_large.resize(size, Image.Resampling.LANCZOS)
    ribbon.alpha_composite(seam)
    return zero_transparent_rgb(ribbon)


def trim(image: Image.Image, threshold: int = 1) -> Image.Image:
    bbox = alpha_bbox(image, threshold)
    if bbox is None:
        raise RuntimeError("Refusing to trim an empty image")
    return image.crop(bbox)


def fit(
    image: Image.Image,
    size: tuple[int, int],
    *,
    pad: int = 0,
    threshold: int = 1,
) -> Image.Image:
    image = trim(zero_transparent_rgb(image), threshold)
    target_w, target_h = size
    usable_w = target_w - 2 * pad
    usable_h = target_h - 2 * pad
    scale = min(usable_w / image.width, usable_h / image.height)
    resized = image.resize(
        (max(1, round(image.width * scale)), max(1, round(image.height * scale))),
        Image.Resampling.LANCZOS,
    )
    canvas = Image.new("RGBA", size, TRANSPARENT)
    canvas.alpha_composite(
        resized,
        ((target_w - resized.width) // 2, (target_h - resized.height) // 2),
    )
    return zero_transparent_rgb(canvas)


def presentation_tune(
    image: Image.Image,
    *,
    saturation: float = 1.0,
    brightness: float = 1.0,
    opacity: float = 1.0,
) -> Image.Image:
    tuned = ImageEnhance.Color(image.convert("RGBA")).enhance(saturation)
    tuned = ImageEnhance.Brightness(tuned).enhance(brightness)
    data = np.asarray(tuned, dtype=np.uint8).copy()
    data[..., 3] = np.rint(data[..., 3].astype(np.float32) * opacity).astype(np.uint8)
    return zero_transparent_rgb(Image.fromarray(data, "RGBA"))


def radial_mask(
    size: tuple[int, int],
    center: tuple[float, float],
    inner: float,
    outer: float,
    feather: float = 3.0,
) -> np.ndarray:
    width, height = size
    yy, xx = np.mgrid[0:height, 0:width].astype(np.float32)
    distance = np.sqrt((xx + 0.5 - center[0]) ** 2 + (yy + 0.5 - center[1]) ** 2)
    inner_alpha = np.clip((distance - (inner - feather)) / (2 * feather), 0.0, 1.0)
    outer_alpha = 1.0 - np.clip((distance - (outer - feather)) / (2 * feather), 0.0, 1.0)
    return inner_alpha * outer_alpha


def draw_summon_frame(
    size: tuple[int, int], accent: tuple[int, int, int, int]
) -> Image.Image:
    """Draw a clean v19-compatible ring without inheriting a legacy portrait."""
    width, height = size
    scale = 4
    canvas = Image.new("RGBA", (width * scale, height * scale), TRANSPARENT)
    draw = ImageDraw.Draw(canvas, "RGBA")

    radius = min((width - 18) / 2.0, (height - 70) / 2.0)
    center_x = width / 2.0
    center_y = 11.0 + radius

    def ellipse_box(radius_value: float) -> tuple[int, int, int, int]:
        return tuple(
            round(value * scale)
            for value in (
                center_x - radius_value,
                center_y - radius_value,
                center_x + radius_value,
                center_y + radius_value,
            )
        )

    # Narrow layered strokes deliberately replace the contaminated AI-generated
    # portrait/frame composite while retaining its v19 pearl/graphite cadence.
    draw.ellipse(ellipse_box(radius + 1), outline=(8, 11, 15, 178), width=9 * scale)
    draw.ellipse(ellipse_box(radius), outline=(236, 233, 226, 242), width=4 * scale)
    draw.ellipse(ellipse_box(radius - 5), outline=(91, 101, 111, 226), width=5 * scale)
    draw.ellipse(ellipse_box(radius - 10), outline=(209, 207, 201, 222), width=2 * scale)
    draw.arc(
        ellipse_box(radius - 5),
        start=298,
        end=356,
        fill=accent,
        width=8 * scale,
    )

    tick_top = round((center_y - radius - 3) * scale)
    draw.line(
        (
            round(center_x * scale),
            tick_top,
            round(center_x * scale),
            tick_top + 15 * scale,
        ),
        fill=(242, 239, 232, 235),
        width=2 * scale,
    )

    chip_w = width * 0.48
    chip_h = height * 0.17
    chip = tuple(
        round(value * scale)
        for value in (
            center_x - chip_w / 2,
            height - chip_h - 7,
            center_x + chip_w / 2,
            height - 7,
        )
    )
    draw.rounded_rectangle(
        chip,
        radius=round(chip_h * 0.42 * scale),
        fill=(27, 32, 39, 232),
        outline=(226, 222, 214, 238),
        width=3 * scale,
    )
    chip_inner = tuple(
        value + offset
        for value, offset in zip(chip, (5 * scale, 5 * scale, -5 * scale, -5 * scale))
    )
    draw.rounded_rectangle(
        chip_inner,
        radius=round(chip_h * 0.31 * scale),
        outline=(93, 103, 114, 190),
        width=1 * scale,
    )

    return zero_transparent_rgb(
        canvas.resize(size, Image.Resampling.LANCZOS)
    )


def draw_player_portrait_frame(size: int = 200) -> Image.Image:
    """Draw only the portrait ring, with no rail tail baked into the sprite."""
    scale = 4
    canvas = Image.new("RGBA", (size * scale, size * scale), TRANSPARENT)
    draw = ImageDraw.Draw(canvas, "RGBA")
    center_x = size * 0.51
    center_y = size * 0.48
    radius = size * 0.39

    def box(radius_value: float) -> tuple[int, int, int, int]:
        return tuple(
            round(value * scale)
            for value in (
                center_x - radius_value,
                center_y - radius_value,
                center_x + radius_value,
                center_y + radius_value,
            )
        )

    draw.ellipse(box(radius + 1), outline=(9, 12, 17, 165), width=9 * scale)
    draw.ellipse(box(radius), outline=(239, 236, 228, 246), width=5 * scale)
    draw.ellipse(box(radius - 6), outline=(111, 146, 158, 226), width=3 * scale)
    draw.ellipse(box(radius - 11), outline=(221, 216, 207, 235), width=2 * scale)

    star_x = center_x - radius - 2
    star_y = center_y
    star_points = (
        (star_x, star_y - 13),
        (star_x + 5, star_y - 4),
        (star_x + 14, star_y),
        (star_x + 5, star_y + 4),
        (star_x, star_y + 13),
        (star_x - 5, star_y + 4),
        (star_x - 14, star_y),
        (star_x - 5, star_y - 4),
    )
    draw.polygon(
        tuple((round(x * scale), round(y * scale)) for x, y in star_points),
        fill=(235, 228, 204, 245),
        outline=(154, 126, 78, 235),
    )

    # One restrained lower fin echoes the original v16 portrait module.
    fin = (
        (center_x - radius * 0.50, center_y + radius * 0.78),
        (center_x - radius * 0.16, center_y + radius + 13),
        (center_x - radius * 0.24, center_y + radius * 0.72),
    )
    draw.polygon(
        tuple((round(x * scale), round(y * scale)) for x, y in fin),
        fill=(220, 216, 208, 225),
        outline=(121, 137, 145, 215),
    )
    return zero_transparent_rgb(
        canvas.resize((size, size), Image.Resampling.LANCZOS)
    )


def isolate_joystick_base(image: Image.Image) -> Image.Image:
    tuned = presentation_tune(image, saturation=0.76, brightness=0.94, opacity=0.65)
    data = np.asarray(tuned, dtype=np.uint8).copy()
    yy, xx = np.mgrid[0:tuned.height, 0:tuned.width].astype(np.float32)
    distance = np.sqrt((xx + 0.5 - tuned.width / 2) ** 2 + (yy + 0.5 - tuned.height / 2) ** 2)
    # Clear the baked center knob so the actual joystick knob can move freely.
    keep = np.clip((distance - 59.0) / 6.0, 0.0, 1.0)
    data[..., 3] = np.rint(data[..., 3].astype(np.float32) * keep).astype(np.uint8)
    return zero_transparent_rgb(Image.fromarray(data, "RGBA"))


def isolate_joystick_knob(image: Image.Image) -> Image.Image:
    crop = image.crop((100, 100, 220, 220))
    tuned = presentation_tune(crop, saturation=0.82, brightness=0.96, opacity=0.92)
    data = np.asarray(tuned, dtype=np.uint8).copy()
    yy, xx = np.mgrid[0:tuned.height, 0:tuned.width].astype(np.float32)
    distance = np.sqrt((xx + 0.5 - 60.0) ** 2 + (yy + 0.5 - 60.0) ** 2)
    keep = np.clip((59.0 - distance) / 2.0, 0.0, 1.0)
    data[..., 3] = np.rint(data[..., 3].astype(np.float32) * keep).astype(np.uint8)
    return fit(zero_transparent_rgb(Image.fromarray(data, "RGBA")), (128, 128), pad=2)


def circular_player_portrait(image: Image.Image, size: int = 512) -> Image.Image:
    portrait = image.convert("RGBA").resize((size, size), Image.Resampling.LANCZOS)
    scale = 4
    mask_large = Image.new("L", (size * scale, size * scale), 0)
    draw = ImageDraw.Draw(mask_large)
    inset = 3 * scale
    draw.ellipse(
        (inset, inset, size * scale - inset - 1, size * scale - inset - 1),
        fill=255,
    )
    mask = mask_large.resize((size, size), Image.Resampling.LANCZOS)
    portrait.putalpha(mask)
    return zero_transparent_rgb(portrait)


def build_summon_contact_sheet(
    frames: dict[int, Image.Image], portraits: dict[int, Image.Image]
) -> Image.Image:
    card_width = 360
    card_height = 400
    sheet = Image.new("RGBA", (card_width * 3, card_height), (17, 21, 26, 255))

    for slot in (1, 2, 3):
        frame = frames[slot]
        portrait = portraits[slot]
        radius = min((frame.width - 18) / 2.0, (frame.height - 70) / 2.0)
        center_x = frame.width / 2.0
        center_y = 11.0 + radius
        portrait_size = round((radius - 12.0) * 2.0)
        portrait_layer = fit(portrait, (portrait_size, portrait_size), threshold=2)

        composed = Image.new("RGBA", frame.size, TRANSPARENT)
        portrait_x = round(center_x - portrait_size / 2.0)
        portrait_y = round(center_y - portrait_size / 2.0)
        composed.alpha_composite(portrait_layer, (portrait_x, portrait_y))
        composed.alpha_composite(frame)

        card_x = (slot - 1) * card_width
        checker = Image.new("RGBA", (card_width, card_height), (24, 29, 35, 255))
        checker_draw = ImageDraw.Draw(checker, "RGBA")
        cell = 24
        for y in range(0, card_height, cell):
            for x in range(0, card_width, cell):
                if ((x // cell) + (y // cell)) % 2:
                    checker_draw.rectangle(
                        (x, y, x + cell - 1, y + cell - 1),
                        fill=(30, 35, 42, 255),
                    )
        sheet.alpha_composite(checker, (card_x, 0))
        sheet.alpha_composite(
            composed,
            (
                card_x + (card_width - composed.width) // 2,
                18 + (344 - composed.height) // 2,
            ),
        )

        draw = ImageDraw.Draw(sheet, "RGBA")
        draw.rectangle(
            (card_x, 0, card_x + card_width - 1, card_height - 1),
            outline=(220, 217, 209, 90),
            width=1,
        )
        draw.text(
            (card_x + 16, card_height - 36),
            f"SLOT {slot} / REAL PROJECT PORTRAIT",
            fill=(238, 236, 230, 235),
        )

    return sheet


def gradient_bar(
    size: tuple[int, int],
    left_color: tuple[int, int, int],
    right_color: tuple[int, int, int],
) -> Image.Image:
    width, height = size
    t = np.linspace(0.0, 1.0, width, dtype=np.float32)[None, :, None]
    left = np.asarray(left_color, dtype=np.float32)[None, None, :]
    right = np.asarray(right_color, dtype=np.float32)[None, None, :]
    rgb = np.rint(left * (1.0 - t) + right * t).astype(np.uint8)
    rgb = np.repeat(rgb, height, axis=0)

    scale = 4
    mask_large = Image.new("L", (width * scale, height * scale), 0)
    draw = ImageDraw.Draw(mask_large)
    draw.polygon(
        (
            (4 * scale, 2 * scale),
            ((width - 5) * scale, 2 * scale),
            ((width - 16) * scale, (height - 2) * scale),
            (0, (height - 2) * scale),
        ),
        fill=255,
    )
    alpha = np.asarray(
        mask_large.resize((width, height), Image.Resampling.LANCZOS), dtype=np.uint8
    )
    rgba = np.dstack((rgb, alpha))
    return zero_transparent_rgb(Image.fromarray(rgba, "RGBA"))


def draw_precision_reticle() -> Image.Image:
    """Draw the fixed-center rifle reticle used by ranged combat.

    Four short cardinal needles converge on one impact point.  This replaces
    the former wide corner brackets, whose open square read as a shotgun spread
    indicator despite the weapon firing a single projectile without spread.
    """
    canvas = 192
    supersample = 4
    center = canvas / 2.0
    image = Image.new(
        "RGBA",
        (canvas * supersample, canvas * supersample),
        TRANSPARENT,
    )
    draw = ImageDraw.Draw(image, "RGBA")

    def rotate_point(
        point: tuple[float, float], quarter_turns: int
    ) -> tuple[float, float]:
        dx = point[0] - center
        dy = point[1] - center
        for _ in range(quarter_turns % 4):
            dx, dy = -dy, dx
        return center + dx, center + dy

    def rotated_points(
        polygon: list[tuple[float, float]], quarter_turns: int
    ) -> list[tuple[int, int]]:
        return [
            (
                round(x * supersample),
                round(y * supersample),
            )
            for x, y in (
                rotate_point(point, quarter_turns) for point in polygon
            )
        ]

    outer_tick = [
        (38.0, 91.5),
        (73.5, 91.5),
        (80.0, 96.0),
        (73.5, 100.5),
        (38.0, 100.5),
    ]
    pearl_tick = [
        (40.5, 93.5),
        (73.0, 93.5),
        (78.5, 96.0),
        (73.0, 98.5),
        (40.5, 98.5),
    ]
    cyan_tip = [
        (72.0, 94.25),
        (79.1, 96.0),
        (72.0, 97.75),
    ]

    for turn in range(4):
        draw.polygon(
            rotated_points(outer_tick, turn),
            fill=(3, 8, 13, 205),
        )
    for turn in range(4):
        draw.polygon(
            rotated_points(pearl_tick, turn),
            fill=(244, 247, 242, 244),
        )
        draw.polygon(
            rotated_points(cyan_tip, turn),
            fill=(125, 231, 239, 224),
        )

    center_scaled = center * supersample
    for radius, color in (
        (8.0, (2, 7, 12, 218)),
        (5.8, (244, 247, 243, 252)),
        (3.55, (111, 229, 239, 250)),
        (1.45, (255, 255, 250, 255)),
    ):
        scaled_radius = radius * supersample
        draw.ellipse(
            (
                round(center_scaled - scaled_radius),
                round(center_scaled - scaled_radius),
                round(center_scaled + scaled_radius),
                round(center_scaled + scaled_radius),
            ),
            fill=color,
        )

    image = image.resize((canvas, canvas), Image.Resampling.LANCZOS)
    pixels = np.asarray(image, dtype=np.uint8).copy()
    pixels[pixels[..., 3] <= 3, :] = 0
    return Image.fromarray(pixels, "RGBA")


def save(image: Image.Image, relative_path: str) -> None:
    destination = OUT / relative_path
    destination.parent.mkdir(parents=True, exist_ok=True)
    zero_transparent_rgb(image).save(destination, optimize=True)


def source_hash(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def write_manifest(source_paths: list[Path]) -> None:
    rows: list[str] = []
    for relative_path, purpose in OUTPUT_SPECS.items():
        path = OUT / relative_path
        image = Image.open(path)
        rgba = image.convert("RGBA")
        bbox = alpha_bbox(rgba, threshold=1)
        bbox_text = "empty" if bbox is None else ",".join(str(value) for value in bbox)
        rows.append(
            f"| `{relative_path}` | {image.width}x{image.height} | {image.mode} | "
            f"`({bbox_text})` | {purpose} |"
        )

    sources = "\n".join(
        f"- `{path}` — SHA-256 `{source_hash(path)}`" for path in source_paths
    )
    manifest = f"""# Celestial HUD asset pack

Deterministic Unity-ready raster components assembled from the v16/v17 element
renders with the v19 presentation treatment. Runtime labels, numbers, HP/EN
values, cooldown counters, and ammo counts are intentionally not baked.

Alpha bounding boxes use `(left, top, right, bottom)` pixel coordinates.

| File | Dimensions | Mode | Alpha bbox | Intended use |
|---|---:|---|---|---|
{chr(10).join(rows)}

## Source integrity

{sources}

## Rebuild

Run `build_asset_pack.py`. The script refuses to overwrite generated PNGs by
default; pass `--force` only when intentionally regenerating this pack.
"""
    (OUT / "manifest.md").write_text(manifest, encoding="utf-8", newline="\n")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--force",
        action="store_true",
        help="overwrite this script's generated PNG outputs",
    )
    args = parser.parse_args()

    existing = [relative for relative in OUTPUT_SPECS if (OUT / relative).exists()]
    if existing and not args.force:
        joined = "\n  ".join(existing)
        raise FileExistsError(
            "Generated outputs already exist; refusing to overwrite:\n  " + joined
        )

    boss_source = V16 / "boss_frame.png"
    pause_source = V16 / "pause.png"
    joystick_source = V16 / "joystick.png"
    player_frame_source = V16 / "player_frame.png"
    hp_rail_source = V17 / "player_hp_rail.png"
    en_rail_source = V17 / "player_en_rail.png"
    ammo_source = V17 / "player_ammo_chip.png"
    action_sources = {
        name: V16 / name
        for name in (
            "action_weapon_swap.png",
            "action_ultimate.png",
            "action_dash.png",
            "action_attack_ranged.png",
        )
    }
    summon_sources = {
        slot: V16 / f"summon_s{slot}.png" for slot in (1, 2, 3)
    }
    portrait_sources = {
        slot: PROJECT_ART / f"Hud_SummonSlot{slot}Icon.png" for slot in (1, 2, 3)
    }

    source_paths = [
        boss_source,
        pause_source,
        joystick_source,
        player_frame_source,
        hp_rail_source,
        en_rail_source,
        ammo_source,
        PLAYER_ICON,
        FLOW_SOURCE,
        *action_sources.values(),
        *summon_sources.values(),
        *portrait_sources.values(),
    ]
    missing = [path for path in source_paths if not path.exists()]
    if missing:
        raise FileNotFoundError("Missing source files:\n" + "\n".join(map(str, missing)))

    save(draw_objective_ribbon(), "objective_frame.png")

    boss = load_rgba(boss_source)
    save(boss, "boss_frame.png")
    save(fit(boss.crop((0, 8, 1024, 88)), (1024, 80), pad=2), "boss_hp_track.png")
    save(fit(boss.crop((0, 76, 1024, 120)), (1024, 48), pad=2), "boss_cost_track.png")
    save(gradient_bar((1024, 24), (241, 106, 112), (207, 62, 84)), "boss_hp_fill.png")
    save(gradient_bar((1024, 18), (138, 231, 235), (49, 201, 215)), "boss_cost_fill.png")

    save(load_rgba(pause_source), "pause.png")
    save(isolate_joystick_base(load_rgba(joystick_source)), "joystick_base.png")
    save(isolate_joystick_knob(load_rgba(joystick_source)), "joystick_knob.png")
    save(draw_player_portrait_frame(), "player_portrait_frame.png")
    save(circular_player_portrait(load_rgba(PLAYER_ICON)), "player_portrait.png")
    save(fit(load_rgba(hp_rail_source), (1024, 56), pad=3, threshold=2), "player_hp_rail.png")
    save(fit(load_rgba(en_rail_source), (1024, 44), pad=3, threshold=2), "player_en_rail.png")
    save(fit(load_rgba(ammo_source), (256, 144), pad=4, threshold=2), "player_ammo_chip.png")
    save(gradient_bar((1024, 24), (255, 253, 247), (220, 213, 196)), "player_hp_fill.png")
    save(gradient_bar((1024, 20), (145, 237, 240), (49, 201, 215)), "player_en_fill.png")
    save(draw_precision_reticle(), "reticle.png")

    action_sizes = {
        "action_weapon_swap.png": (256, 256),
        "action_ultimate.png": (256, 256),
        "action_dash.png": (256, 256),
        "action_attack_ranged.png": (320, 320),
    }
    for name, source in action_sources.items():
        save(fit(load_rgba(source), action_sizes[name], pad=4, threshold=2), name)

    summon_sizes = {1: (320, 344), 2: (288, 316), 3: (288, 316)}
    summon_accents = {
        1: (49, 201, 215, 245),
        2: (217, 184, 120, 238),
        3: (225, 107, 93, 238),
    }
    contact_frames: dict[int, Image.Image] = {}
    contact_portraits: dict[int, Image.Image] = {}
    for slot in (1, 2, 3):
        frame = draw_summon_frame(summon_sizes[slot], summon_accents[slot])
        save(frame, f"summon_s{slot}_frame.png")
        # Preserve the project summon render exactly, including its authored crop.
        destination = OUT / f"summon_s{slot}_portrait.png"
        shutil.copyfile(portrait_sources[slot], destination)
        contact_frames[slot] = frame
        contact_portraits[slot] = load_rgba(portrait_sources[slot])

    save(
        build_summon_contact_sheet(contact_frames, contact_portraits),
        "QA/summon_overlay_contact_sheet.png",
    )

    flow = load_rgba(FLOW_SOURCE)
    save(flow, "Motion/DB_UI_CelestialFlow.png")

    write_manifest(source_paths)
    print(f"Wrote {len(OUTPUT_SPECS)} PNG assets to {OUT}")
    for relative in OUTPUT_SPECS:
        image = Image.open(OUT / relative)
        bbox = alpha_bbox(image.convert("RGBA"), threshold=1)
        print(f"{relative}: {image.width}x{image.height} {image.mode} alpha_bbox={bbox}")


if __name__ == "__main__":
    main()
