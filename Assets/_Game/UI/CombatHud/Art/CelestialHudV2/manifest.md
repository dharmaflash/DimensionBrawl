# Celestial HUD V2 asset pack

Deterministic, Unity-ready components for the approved mobile combat HUD. The
pack is separated at meaningful runtime state boundaries: plates, glyphs,
portraits, masks, rails, fills, arcs, and reticle pieces remain independent.
No runtime label, number, ammo value, cost, cooldown, boss name, or objective
copy is baked into any `Runtime` sprite.

## Directory contract

- `Runtime/` — flat production contract consumed by the V22 prefab assembler.
- `Source/ImageGen/` — chroma sources for the shared action surface and glyphs.
- `Prompts/` — exact ImageGen source prompts.
- `Motion/` — tiny reusable flow/micrograin textures.
- `QA/` — contact sheets, alpha/fringe report, and deterministic SHA-256 list.

The runtime directory intentionally stays flat. Semantic `Core`, `Glyph`, and
`Portrait` roles are recorded below instead of duplicating Unity assets across
subdirectories.

## Runtime sprites

| File | Dimensions | Role | Composition contract |
|---|---:|---|---|
| `objective_frame.png` | 960x199 | Core | Borderless left-bleed mission strip; runtime text only. |
| `boss_name_tab.png` | 512x72 | Core | Boss name tab; runtime name only. |
| `boss_hp_track.png` | 1024x80 | Core | HP rail with transparent fill aperture. |
| `boss_hp_fill.png` | 1024x24 | Core | Horizontal boss HP fill. |
| `boss_cost_track.png` | 1024x48 | Core | Cost rail with transparent fill aperture. |
| `boss_cost_fill.png` | 1024x18 | Core | Horizontal boss cost fill. |
| `pause_plate.png` | 160x160 | Core | System plate without glyph. |
| `glyph_pause.png` | 160x160 | Glyph | Pause bars only. |
| `action_plate.png` | 256x256 | Core | Shared small action plate; uniformly scale for primary attack. |
| `action_ready_arc.png` | 256x256 | Core | Shared 72-degree ready arc, starting at 12 o'clock. |
| `action_cooldown_disc.png` | 256x256 | Core | Shared radial cooldown overlay source. |
| `glyph_weapon_swap.png` | 256x256 | Glyph | Carbine, sword, and one bridge cue. |
| `glyph_ultimate.png` | 256x256 | Glyph | Deterministic two-blade orbit glyph; no star/lightning. |
| `glyph_dash.png` | 256x256 | Glyph | One connected forward dash silhouette. |
| `glyph_attack_ranged.png` | 320x320 | Glyph | One connected rifle silhouette. |
| `summon_mask.png` | 320x344 | Core | Shared alpha aperture; scale with slot frame. |
| `summon_frame_s1.png` | 320x344 | Core | Primary summon frame. |
| `summon_frame_s2.png` | 288x316 | Core | Secondary summon frame. |
| `summon_frame_s3.png` | 288x316 | Core | Secondary summon frame. |
| `summon_state_arc.png` | 320x344 | Core | Shared 72-degree availability/ready arc. |
| `summon_cost_tab.png` | 92x54 | Core | Cost tab without a baked value; scale to 84x50 for secondary slots. |
| `summon_portrait_s1.png` | 600x600 | Portrait | Identity-preserved project summon 1. |
| `summon_portrait_s2.png` | 600x600 | Portrait | Identity-preserved project summon 2. |
| `summon_portrait_s3.png` | 600x600 | Portrait | Identity-preserved project summon 3. |
| `player_portrait_frame.png` | 200x200 | Core | Octagonal portrait socket frame. |
| `player_portrait_mask.png` | 200x200 | Core | Player portrait aperture mask. |
| `player_portrait.png` | 512x512 | Portrait | Identity-preserved project player image. |
| `player_hp_track.png` | 1024x56 | Core | Stretchable player HP rail. |
| `player_hp_fill.png` | 1024x24 | Core | Horizontal player HP fill. |
| `player_en_track.png` | 1024x44 | Core | Stretchable player EN rail. |
| `player_en_fill.png` | 1024x20 | Core | Horizontal player EN fill. |
| `player_ammo_plate.png` | 256x144 | Core | Ammo/mode plate without content. |
| `glyph_bullet.png` | 128x128 | Glyph | Ammo glyph only. |
| `glyph_mode_ranged.png` | 128x128 | Glyph | Ranged-mode glyph only. |
| `joystick_base.png` | 320x320 | Core | Low-alpha two-ring joystick base. |
| `joystick_knob.png` | 128x128 | Core | Independent joystick knob. |
| `reticle_needle.png` | 192x192 | Core | One left needle on a centered canvas; instantiate at 0/90/180/270 degrees. |
| `reticle_dot.png` | 192x192 | Core | Center point on a centered canvas. |

## Runtime composition rules

- Only action, summon, joystick, and system-control hit targets raycast. Every
  visual child in this pack must use `raycastTarget = false`.
- Circular plates/glyphs use `Image.Type.Simple` with `preserveAspect = true`.
- Meter fills use horizontal fill or a rectangular clip. Their rails render
  above the fill and define the final silhouette.
- `action_ready_arc` and `summon_state_arc` begin at 12 o'clock and progress
  clockwise. Cyan is state feedback, not neutral decoration.
- `summon_mask` is the `Mask` graphic with `showMaskGraphic = false`. Portraits
  are its children; frames and state arcs render above it.
- `reticle_needle` keeps the 192x192 canvas because its center is the rotation
  pivot. Four rotated instances plus `reticle_dot` reproduce the exact centered
  precision reticle without a shotgun/spread reading.
- Recommended player layout uses a 456x26 HP track and 456x26 EN track with
  444x16 and 444x14 fills respectively. Source masters remain 1024 pixels wide
  to preserve clean downscaling.

## Source handling

The action plate and four action glyph sources were generated separately on a
flat green key. `build_asset_pack_v2.py` performs deterministic border-key
sampling, soft alpha extraction, despill, semantic connected-component cleanup,
palette normalization, exact occupancy fitting, and Lanczos downsampling.
The ultimate source is intentionally replaced by deterministic two-blade
geometry because the source read as a star/lightning burst.

Visible player and summon portrait pixels are preserved from the current
project art. Fully transparent RGB is zeroed during PNG encoding to prevent
sampling fringe; no face, armor, silhouette, or color edit is made.

## Motion

| File | Dimensions | Purpose |
|---|---:|---|
| `DB_UI_CelestialFlow.png` | 128x64 | Existing seamless HUD flow map. |
| `panel_micrograin_128.png` | 128x128 | ImageGen-derived, high-pass normalized, tile-softened micrograin limited to +/-3 RGB. |

## QA

- `QA/action_components_contact_sheet.png` — neutral action buttons; ready arc
  appears only on the ultimate sample.
- `QA/summon_components_contact_sheet.png` — the three real portraits clipped
  by masks and composed below frames/tabs.
- `QA/hud_components_contact_sheet.png` — objective, boss/player rails and
  fills, portrait/ammo/system components, joystick, and reticle assembly.
- `QA/qa_report.json` — dimensions, mode, alpha bbox, corner alpha,
  transparent-RGB count, green-fringe count, and SHA-256 for every generated
  PNG.
- `QA/hash_manifest.sha256` — compact deterministic hash list.

The build fails if any runtime sprite is absent, has the wrong dimensions or
mode, is empty, leaves non-zero RGB in fully transparent pixels, or retains a
green chroma fringe in an ImageGen-derived output.

## Rebuild

From `Assets/`:

```powershell
uv run --with pillow --with numpy python `
  _Game/UI/CombatHud/Art/CelestialHudV2/build_asset_pack_v2.py --force
```

Without `--force`, the builder refuses to replace an existing pack.
