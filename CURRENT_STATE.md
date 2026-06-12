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

Use `Assets/_Game/DesignDocs/COMBAT_V1_SPEC.md` as the implementation guardrail. The next safe implementation step is to prepare the smallest player action slice: one player candidate, one basic sci-fi soldier candidate, one inspectable combat test scene, movement, short manual basic attack, dodge, health/damage, and a clear/fail condition.

## Current Risk

- Unity may recreate project setting folders while packages are inspected. Do not commit package/project setting changes unless they are intentionally reviewed.
- The project direction is direct-control ARPG first, summon system second. Do not start by building summon behavior, boss phases, progression, or a full mobile UI shell before the player action loop is playable.
