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

Choose one player prefab candidate and one small animation subset, then create a game-owned prefab under `Assets/_Game/Art/Characters/Player/` or `Assets/_Game/Prefabs/` without writing gameplay code yet.

## Current Risk

Unity may recreate project setting folders while packages are inspected. Do not commit package/project setting changes unless they are intentionally reviewed.
