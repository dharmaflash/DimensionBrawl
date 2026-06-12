# Decisions

## 2026-06-12: Restart From Clean Baseline

Decision: Start `DimensionBrawl` as a clean Unity project instead of continuing to repair the old project.

Reason: The previous project had unstable AI-generated code and unclear direction. A small baseline is safer than further salvage work.

## 2026-06-12: Raw Asset Packs Are Local-Only

Decision: Store imported asset packs under `Assets/_Imported/` and ignore them in Git.

Reason: The packs are large and should not pollute the repository history. Only curated game-ready assets should be copied or authored under `Assets/_Game/`.

## 2026-06-12: Prefab/Scene Authoring Before Runtime Generation

Decision: Prefer authored prefabs, scene objects, ScriptableObjects, and Inspector configuration over runtime mass generation.

Reason: The project must stay inspectable in Unity and avoid giant AI-authored runtime builders.

## 2026-06-12: No Legacy/Fallback By Default

Decision: New systems should not start with legacy compatibility, broad fallback paths, or old-project restoration logic.

Reason: The project is a restart. Compatibility code is allowed only when a concrete current feature needs it and the removal condition is documented.

## 2026-06-12: Small Vertical Slice First

Decision: Build the first playable around one player, one basic enemy, one attack loop, and one defeat condition.

Reason: A small complete loop exposes real needs earlier than large speculative architecture.

