# Current State

Last updated: 2026-06-12 KST

## Repository

- Git repository is initialized at `C:\Git\DimensionBrawl`.
- Main branch is `main`.
- Raw imported asset packs are local-only under `Assets/_Imported/`.
- Game-owned work should live under `Assets/_Game/`.

## Committed Baseline

- Unity project baseline exists.
- Combat reference docs and datasets are imported.
- `COMBAT_V1_SPEC.md` defines the first direct-control ARPG action slice before summon implementation.
- `ACTION_FEEL_TARGETS.md` defines the movement, camera, attack, dodge, hit, and enemy feel quality targets that action work must improve.
- `ACTION_FOUNDATION_OWNERSHIP.md` defines the narrow runtime ownership split for the first action-feel implementation.
- `ACTION_FOUNDATION_TESTING.md` records the first test setup, control map, reference-backed values, first-pass deviations, and deliberate exclusions.
- `Assets/_Game/Scenes/ActionFoundationTest.unity` exists as the first authored action-feel inspection scene.
- `ActionFoundationTest.unity` uses game-owned URP test materials under `Assets/_Game/Art/Materials/ActionFoundation/` so the inspection scene does not rely on Unity's built-in default material or render pink/missing-shader placeholders.
- The player inspection placeholder has `PlayerDodgeFeedback`, a presentation-only dodge tint driven by `PlayerActionController` dodge events, so the dodge window is visible even before real player animations are wired.
- `ActionFoundationTest.unity` now keeps `Player_CombatGirl_ActionFoundation` as the gameplay root and mounts a curated `CombatGirlSwordShield_PlayerVisual` child under that root; movement, combat, health, and feedback ownership remain on the existing root components.
- The CombatGirl visual slice promotes only selected game-owned assets into `_Game`: one model/avatar, six minimal clips, one Animator Controller, primary albedo textures, and Unity Toon Shader materials. It should not reference `_Imported/`.
- The old capsule/sword proxy objects remain inactive fallback/reference objects in the scene. The active player hit/dodge feedback now targets CombatGirl renderers, and the old cyan idle tint is disabled so authored material colors can show between hit/dodge flashes.
- `DimensionBrawl > Validate Action Foundation Test Scene` validates required scene objects, component ownership, key references, and reference-backed timing values from inside the Unity Editor.
- `DimensionBrawl > Validate Action Foundation Test Scene` also validates player Animator wiring and root-motion-off state for the curated visual child.
- `DimensionBrawl > Reapply Action Foundation CombatGirl Materials` exists for the open-Editor stale-scene case: unpacked CombatGirl renderers can keep old material slots after `Assets > Refresh`, so this menu reassigns the open scene to `_Game` CombatGirl material assets and saves it.
- Unity batchmode validation passes with `Action foundation test scene validation passed.` after closing the open Editor instance.
- Isolated Unity PlayMode smoke tests against a temp project copy passed for player movement, dodge movement, basic attack damage, win state, and fail state.
- `ActionFoundationPlayModeTests` now also includes a compiled dodge tint feedback test; rerun the PlayMode suite or confirm the tint manually in the open Editor before treating the visual dodge feedback as fully revalidated.
- Manual open-Editor inspection confirmed the player placeholder visibly flashes/tints when dodging with `Left Shift`.
- Raw imported asset packs are ignored.
- `_Game/Art` folder structure exists for curated game-ready assets.
- Project governance docs now define AI limits, folder ownership, code style, architecture boundaries, session workflow, and review checks.

## Local Assets

Raw packs currently staged only as local files:

- `Assets/_Imported/AssetStore/CombatGirlsCharacterPack`
- `Assets/_Imported/AssetStore/Animation`
- `Assets/_Imported/AssetStore/Protofactor`
- `Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3`

These must not be committed directly.

## Next Safe Step

Use `Assets/_Game/DesignDocs/COMBAT_V1_SPEC.md`, `Assets/_Game/DesignDocs/ACTION_FEEL_TARGETS.md`, `Assets/_Game/DesignDocs/ACTION_FOUNDATION_OWNERSHIP.md`, and `Assets/_Game/DesignDocs/ACTION_FOUNDATION_TESTING.md` as implementation guardrails. The active implementation step is to validate movement/dodge/basic attack against the curated CombatGirl visual and then tune direct player action feel before adding summons or boss work.

## Current Risk

- Unity may recreate project setting folders while packages are inspected. Do not commit package/project setting changes unless they are intentionally reviewed.
- Unity may briefly report stale or malformed `.meta` data while files are being promoted. Verify the final `.meta` GUIDs and refresh/reimport in the open Editor before assuming the asset files themselves are broken.
- If the already-open `ActionFoundationTest` scene shows a pink CombatGirl after material promotion, run `DimensionBrawl > Reapply Action Foundation CombatGirl Materials`; refresh alone does not always update unpacked scene renderer material slots.
- Unity batchmode reruns may be blocked by the Unity Licensing Client/headless editor state while Editor instances are open; use the open Editor validation menu for quick checks, or close Unity before a full batchmode rerun.
- The project direction is direct-control ARPG first, summon system second. Do not start by building summon behavior, boss phases, progression, or a full mobile UI shell before the player action loop is playable.
- Action feel is the first quality gate. Do not add larger systems to hide weak movement, camera, attack, dodge, or hit feedback.
- The current CombatGirl material pass is primary-color recovery only. Do not mass-promote original matcaps, masks, or the full asset-pack material stack without a separate reviewed slice.
