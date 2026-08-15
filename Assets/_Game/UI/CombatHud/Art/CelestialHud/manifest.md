# Celestial HUD asset pack

Deterministic Unity-ready raster components assembled from the v16/v17 element
renders with the v19 presentation treatment. Runtime labels, numbers, HP/EN
values, cooldown counters, and ammo counts are intentionally not baked.

Alpha bounding boxes use `(left, top, right, bottom)` pixel coordinates.

| File | Dimensions | Mode | Alpha bbox | Intended use |
|---|---:|---|---|---|
| `objective_frame.png` | 960x202 | RGBA | `(0,20,938,182)` | PGR-inspired top-left mission ribbon: sparse translucent matte navy, open fade at right; runtime objective text remains separate. |
| `boss_frame.png` | 1024x128 | RGBA | `(8,18,1016,110)` | Combined boss HP/cost underlay matching the v19 layout. |
| `boss_hp_track.png` | 1024x80 | RGBA | `(2,4,1022,75)` | Upper boss HP rail only. |
| `boss_hp_fill.png` | 1024x24 | RGBA | `(0,1,1022,23)` | Runtime-filled boss HP color strip. |
| `boss_cost_track.png` | 1024x48 | RGBA | `(2,6,1022,42)` | Lower boss cost rail only. |
| `boss_cost_fill.png` | 1024x18 | RGBA | `(0,0,1022,17)` | Runtime-filled boss cost color strip. |
| `pause.png` | 160x160 | RGBA | `(8,6,152,154)` | Pause button chrome and pause glyph. |
| `summon_s1_frame.png` | 320x344 | RGBA | `(19,5,301,340)` | Primary summon frame, portrait aperture left transparent. |
| `summon_s1_portrait.png` | 600x600 | RGBA | `(43,57,559,576)` | Real project summon slot 1 render, copied pixel-for-pixel. |
| `summon_s2_frame.png` | 288x316 | RGBA | `(17,5,271,312)` | Secondary summon frame, portrait aperture left transparent. |
| `summon_s2_portrait.png` | 600x600 | RGBA | `(82,63,553,583)` | Real project summon slot 2 render, copied pixel-for-pixel. |
| `summon_s3_frame.png` | 288x316 | RGBA | `(17,5,271,312)` | Secondary summon frame, portrait aperture left transparent. |
| `summon_s3_portrait.png` | 600x600 | RGBA | `(57,57,560,580)` | Real project summon slot 3 render, copied pixel-for-pixel. |
| `action_weapon_swap.png` | 256x256 | RGBA | `(5,4,250,252)` | Weapon switch action button. |
| `action_ultimate.png` | 256x256 | RGBA | `(6,4,250,252)` | Skill/ultimate action button. |
| `action_dash.png` | 256x256 | RGBA | `(6,4,249,252)` | Dash/dodge action button. |
| `action_attack_ranged.png` | 320x320 | RGBA | `(5,4,315,316)` | Primary ranged attack action button. |
| `joystick_base.png` | 320x320 | RGBA | `(8,9,312,310)` | Low-opacity joystick annulus; center is clear for a moving knob. |
| `joystick_knob.png` | 128x128 | RGBA | `(2,2,126,126)` | Independent joystick knob. |
| `player_portrait_frame.png` | 200x200 | RGBA | `(6,14,184,190)` | Player portrait frame only. |
| `player_portrait.png` | 512x512 | RGBA | `(0,0,512,512)` | Circular portrait derived from the project's Apk_Icon. |
| `player_hp_rail.png` | 1024x56 | RGBA | `(80,3,944,53)` | Player HP frame/track. |
| `player_hp_fill.png` | 1024x24 | RGBA | `(0,1,1022,23)` | Runtime-filled player HP color strip. |
| `player_en_rail.png` | 1024x44 | RGBA | `(30,3,994,41)` | Player EN frame/track. |
| `player_en_fill.png` | 1024x20 | RGBA | `(0,1,1022,19)` | Runtime-filled player EN color strip. |
| `player_ammo_chip.png` | 256x144 | RGBA | `(4,6,252,137)` | Ammo counter chrome; runtime icon/text remains separate. |
| `reticle.png` | 192x192 | RGBA | `(37,37,155,155)` | Compact precision reticle with a center point and four cardinal needles. |
| `Motion/DB_UI_CelestialFlow.png` | 128x64 | RGBA | `(0,0,128,64)` | Tiny seamless grayscale flow map for shader UV scrolling. |
| `QA/summon_overlay_contact_sheet.png` | 1080x400 | RGBA | `(0,0,1080,400)` | QA only: clean frames composited over the real project portraits. |

## Source integrity

- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\boss_frame.png` — SHA-256 `67b1c7a3a56acc9d421f3a4612aed6240314264ea4b1d44ac37f089431f120c4`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\pause.png` — SHA-256 `7f9b190af1e45606b0f15d39d55b3243d4fe0417985caa0670a9b246d6f3bcf5`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\joystick.png` — SHA-256 `c900fb62c56f3d3c7f5e2c6080f647ff2d1d203163f53b0c2e63dcbfaee4732d`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\player_frame.png` — SHA-256 `7cb937807a565a28246ea4a5157616a9d19d126755ac15d5f06a7818a1e5dde6`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v17\elements\player_hp_rail.png` — SHA-256 `6baaf484b7a3da9af4d1c4f87c24db46038f56b347dd55a422f8b7f633aeb87d`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v17\elements\player_en_rail.png` — SHA-256 `138ad04f54e87e9155a50136458a13627b5d28fb0fd07c08b2a15b1a5870ec73`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v17\elements\player_ammo_chip.png` — SHA-256 `fa4db2d57bc5c427c4f3d3c10c9c83fe28cc57cc6dc7245af157818b266c0b90`
- `C:\Git\DimensionBrawl\Assets\_Game\UI\Apk_Icon\Apk_Icon.png` — SHA-256 `5e8bb9a06953012ce0c237b3b2d4d9c7df2547213be23109173937b6cbc6df4a`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-flow-v21\seamless_flow_128x64.png` — SHA-256 `19d0cd2b8a3643caa0609e0bcbba896b4b985bd653bafdfb33988276a79439e3`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\action_weapon_swap.png` — SHA-256 `426f59e7eed450af83d97a794cecd202e40b1c78852f3fa217c40c9058901228`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\action_ultimate.png` — SHA-256 `fc037bc5373df7b647df8913485515700cd66b2c22ac927e46dcbc5cf810b2fb`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\action_dash.png` — SHA-256 `6adb634bc13ed1077c6c71c65127969a903a9567a82cc4c1fe91abe2dff87d10`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\action_attack_ranged.png` — SHA-256 `537bae2ff7ced61b1d0f8f9a366432946ec512e4180ac98c762161bfb3b23609`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\summon_s1.png` — SHA-256 `06c732c6096381339f9becc55cc6aed7ffbf73e5b4520740cfaf0e406580e5db`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\summon_s2.png` — SHA-256 `2f342cd2a946c89cfbd58ab79246c199f21e678c860cc1641ba8aa3634f24ad8`
- `C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\celestial-elements-v16\elements\summon_s3.png` — SHA-256 `86db8f4c8826d3243528192c95c2474144d7dc8c5e51f466799d2e3d49431371`
- `C:\Git\DimensionBrawl\Assets\_Game\UI\CombatHud\Art\DimensionHud\Hud_SummonSlot1Icon.png` — SHA-256 `bece4d0eb3ac6995b1ab2bcedb217e5f35f5904438181943d3b017aab9ed9397`
- `C:\Git\DimensionBrawl\Assets\_Game\UI\CombatHud\Art\DimensionHud\Hud_SummonSlot2Icon.png` — SHA-256 `baeba8d9e15b1c6aac678ef47ac0852974bf3b04701445b570c9d29a8d1af65c`
- `C:\Git\DimensionBrawl\Assets\_Game\UI\CombatHud\Art\DimensionHud\Hud_SummonSlot3Icon.png` — SHA-256 `6bab2235e69b76903bcef3534df5a7e3decab77b67e9ae08c6ee7618758de0fd`

## Rebuild

Run `build_asset_pack.py`. The script refuses to overwrite generated PNGs by
default; pass `--force` only when intentionally regenerating this pack.
