# Celestial HUD Target atomic asset pack

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
| `Objective/objective_body.png` | 960x199 | `(0, 57, 960, 141)` | `1e8d2c4cea90129e190a86cea291c3c62e5e4cbf1d5b119f89f2ef88c8d6508a` |
| `Objective/objective_facets_top.png` | 960x199 | `(0, 17, 960, 59)` | `debda39f9fb008a288a0332156748edd86dc586b1962788a476a61d966e094bb` |
| `Objective/objective_facets_bottom.png` | 960x199 | `(0, 139, 960, 180)` | `18ed0c9d919201b668bf5e3a52c59e2d7f18bca54e4651a2905f954c5453116b` |
| `Boss/boss_chassis.png` | 1100x150 | `(0, 13, 1095, 133)` | `d30c02f8e1c0de106e7468534ddeab2515b514133d60f2675497d7e02b66d99c` |
| `Boss/boss_name_tab.png` | 420x84 | `(0, 4, 419, 74)` | `aa3c0f0b793872afe11c4d384d843cf9c23d44cdb2345c3474200e00fbccdc75` |
| `Boss/boss_hp_track.png` | 1024x56 | `(0, 0, 1024, 56)` | `c9c2ebd6c7c8a241671679987ae479d9cc776926437258f88941fc9802980488` |
| `Boss/boss_hp_fill.png` | 1024x28 | `(0, 0, 1024, 27)` | `a07acf1ccab070790bf96a97d86b1abca9ac3d732700e3ce346ca5657ca5c039` |
| `Boss/boss_cost_track.png` | 1024x44 | `(0, 0, 1024, 44)` | `af695a51b7fe0e91e237b205f1b9a24605be1df7949ad5017866d87dc3c0762a` |
| `Boss/boss_cost_fill.png` | 1024x22 | `(0, 0, 1024, 21)` | `8ce64c39920d0785ba0b75436670f2483af61b68646708207eec630a3feeb950` |
| `System/pause_plate.png` | 192x192 | `(2, 4, 188, 190)` | `cbd15d253d27a6af19e57b00dfc2f36d31c0e424cd6927ab2f0eb6134d45cfa5` |
| `System/pause_glyph.png` | 192x192 | `(68, 53, 128, 139)` | `72796ad77484541cbf4f4877ed9c88c606802e5402f574520ea7292ace7900ec` |
| `Action/action_plate.png` | 512x512 | `(8, 8, 504, 504)` | `c1b831db03d3a523e27c9b2c1b17ccde3eaf298d08043b8d641aed695a4bc2fe` |
| `Action/action_ready_arc.png` | 512x512 | `(31, 22, 490, 413)` | `ae4b42d70300958bf57e45d2b5466071d5c4d57a18b644ee2853a39de8cc0aab` |
| `Action/action_cooldown_disc.png` | 512x512 | `(28, 28, 484, 484)` | `ce6aeffe6a3ddf0a08c917da82367ceb0d1fd96f9623df7a1a41fc95e64e40e5` |
| `Action/glyph_weapon_swap.png` | 512x512 | `(56, 121, 429, 369)` | `1ca96db69df46a12852b241ede59064a1d8211688f2f1e41dcb0234babbecc7f` |
| `Action/glyph_ultimate.png` | 512x512 | `(82, 114, 422, 400)` | `5e01a5d2074539f99fa735d0a875f5d173f58e905c684ac30cc57efd12f6afab` |
| `Action/glyph_dash.png` | 512x512 | `(53, 145, 365, 367)` | `65e948181d5594c702c63b75564d78aa0a9a443083573f2e80e26a970d28d2e2` |
| `Action/glyph_attack.png` | 512x512 | `(34, 163, 502, 366)` | `a937e26a3a36bfc16cf6367dd6b383e9b8ebe8964253f9f8fecfc590825d04e6` |
| `Summon/summon_mask_s1.png` | 384x340 | `(28, 27, 353, 297)` | `0ad7ffcdcfc63a9d603149ca432565c98c0130b98711a13b1818af905352ac95` |
| `Summon/summon_mask_s2.png` | 360x260 | `(29, 24, 333, 223)` | `cf601955de807a13b92850eee4c5c65b8ffcf318b8dc27e333cdbc3c4ec8b841` |
| `Summon/summon_mask_s3.png` | 340x242 | `(29, 23, 314, 206)` | `80efc98618bfbeeb1ee4bf32f1ab210edf8f028cf47c59ee826a903ad87e5554` |
| `Summon/summon_frame_s1.png` | 384x340 | `(0, 0, 384, 334)` | `ff00619b561fec546fd85b8c49422f8c1b8e991d020126ec9dbf22abf4ae787f` |
| `Summon/summon_frame_s2.png` | 360x260 | `(0, 0, 360, 256)` | `c59534ebd4cffb9bf454662fa07482e937fbd4cbf5d45956cb36f56305e99647` |
| `Summon/summon_frame_s3.png` | 340x242 | `(0, 0, 340, 238)` | `3594b9ad62bea64a13c1cac5de627e465ab5f662e82aa6232766fcfaac5b3b47` |
| `Summon/summon_accent_s1.png` | 384x340 | `(10, 32, 362, 293)` | `f736c3304d0ea9d904e166bd9c8044a686e6f23528bc94d97aa8adda4a631658` |
| `Summon/summon_accent_s2.png` | 360x260 | `(292, 34, 346, 176)` | `4996cf8580b4b8762ad449c18405780ecf0f55121042488c4b3f7243ac226304` |
| `Summon/summon_accent_s3.png` | 340x242 | `(103, 174, 327, 230)` | `4f8f80a35e6a9600b14ba65606bca0ce280da595bc5d9912e1e1e906a155652c` |
| `Summon/summon_cost_tab_s1.png` | 128x72 | `(0, 1, 128, 71)` | `bfea049dbaabe2967a39ae9011e1d290db6da3be35228b09f4e8da82aed1fbfc` |
| `Summon/summon_cost_tab_s2.png` | 112x64 | `(0, 1, 112, 63)` | `a00fd976e5df3375949e5cd13c244f5f01d6f1335acbe262a4cc1cead3a667e0` |
| `Summon/summon_cost_tab_s3.png` | 112x64 | `(0, 1, 112, 63)` | `a00fd976e5df3375949e5cd13c244f5f01d6f1335acbe262a4cc1cead3a667e0` |
| `Summon/summon_portrait_s1.png` | 512x512 | `(17, 17, 499, 496)` | `42604c12c7bce905d22d0f6b0c0fa290c266e930e5a583ac7131fb7a9c347f18` |
| `Summon/summon_portrait_s2.png` | 512x512 | `(14, 8, 498, 490)` | `a702d13919fbe3fb43af28800409726464cea1af54fc105e246a5b6beaa511bd` |
| `Summon/summon_portrait_s3.png` | 512x512 | `(14, 14, 498, 371)` | `1a77e79cda3247e5b41c4d8cd578767ea3d50ad37deba09e2a303a0372880b87` |
| `Joystick/joystick_base_glass.png` | 512x512 | `(29, 29, 483, 483)` | `169551a5ba536bee8ce8aaf1a2c3b56051f16afb12b2c7f0da43085481c56c21` |
| `Joystick/joystick_ring_ticks.png` | 512x512 | `(22, 22, 492, 492)` | `c1bb6c110a230d3ae71aab47bba46aa225413ad5b6bbe87059f67945fcdc5ce0` |
| `Joystick/joystick_knob.png` | 192x192 | `(5, 5, 187, 187)` | `89859d1572c9c0ee23297be515c4c307eab3997d139da01a0b5fe58e232458ce` |
| `Player/player_portrait.png` | 512x512 | `(0, 0, 512, 512)` | `f77e3b6805919c46915502b37f9780cc47437d7c77a74be30d7829aa3c0ae6e9` |
| `Player/player_portrait_mask.png` | 256x256 | `(6, 7, 248, 248)` | `0b205c495f26d98a33a4daada40d378cef0e3e185cc0e2d4b10b31989c50ec06` |
| `Player/player_portrait_frame.png` | 256x256 | `(0, 0, 256, 256)` | `f65dd6202c75c9096b25c506ec1709b7838f1408846a0f5f78c4085421bb8f66` |
| `Player/player_chassis.png` | 1400x200 | `(2, 16, 1395, 179)` | `b52f642d689245a0ef17a2303fb8c519bac0677f81e26e836b8721d688488032` |
| `Player/player_hp_track.png` | 1024x48 | `(0, 0, 1024, 48)` | `569fcbba6b076dd15677e2b3f0372f79435d61feff9f7d08ffbfed0cec3e1c56` |
| `Player/player_hp_fill.png` | 1024x26 | `(0, 0, 1024, 25)` | `e69ab659cd4af4bd4a9e6cb9c18d08be17177a7b6304e864e9afcf755be94efc` |
| `Player/player_cost_track.png` | 1024x42 | `(0, 0, 1024, 42)` | `13e94b2c7068a5c55db6a59e09d10a6f4679bd06f8259b9f1a52ea07ca79d6ef` |
| `Player/player_cost_fill_segmented.png` | 1024x24 | `(0, 0, 1024, 23)` | `6130f9eeca1ac7253632eb37b494ce6ae444bfa407f169c3dc5f5e94ea1ea777` |
| `Player/player_state_pips.png` | 128x64 | `(22, 7, 108, 56)` | `def4027e42999a205740b45b07dd57db04e0754fcfc632dea6d50770798d9aa3` |
| `Player/player_mode_glyph.png` | 128x128 | `(5, 5, 123, 123)` | `26b1a1fb72dea6b5c88698a8f7f9a35046a1456a0c1d0984b86305178c040b2b` |
| `Player/player_ammo_plate_compact.png` | 320x112 | `(0, 2, 320, 112)` | `977f777f1c50d404ddb28d7d2a172e36ca2fbf447f143c449cb162b17cabc2e6` |
| `Player/player_bullet_glyph.png` | 128x128 | `(22, 18, 120, 111)` | `6477afda871a5d52c3f61a67bcec88f76730146f05874e426616297bec07435d` |
| `Player/player_ammo_separator.png` | 32x112 | `(16, 11, 19, 101)` | `7b609c6738e7df69d5a9cafbb9d852c83b35787adc2f2b023301dc03da034808` |
| `Reticle/reticle_precision_dot.png` | 192x192 | `(86, 87, 106, 105)` | `114fd491c2799b52f97db7f4f280b736d299e4db7beb94c297cc6afbb2fd1775` |
| `Reticle/reticle_precision_needle.png` | 192x192 | `(36, 90, 83, 102)` | `e5deb7bd2655b3d21a218ba2083d93c47dee5a49f7c184d8235cf5da7a772eff` |

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
