# AI Code Contract

This contract exists to prevent AI-style overbuilding.

## Required Flow

1. Read the relevant files first.
2. State the exact change scope.
3. Prefer editing existing narrow files over adding new systems.
4. Implement one small behavior at a time.
5. Verify with Unity compile/test/manual checklist when available.
6. Update `CURRENT_STATE.md` or `DECISIONS.md` only when the project state or a real decision changes.

For combat or action-feel work, `Assets/_Game/DesignDocs/COMBAT_V1_SPEC.md` and `Assets/_Game/DesignDocs/ACTION_FEEL_TARGETS.md` are required reading before implementation.

## Code Rules

- One MonoBehaviour should own one behavior.
- No manager class may silently own player, enemy, UI, camera, input, spawning, and progression together.
- Serialized fields are for authored configuration. Do not hide important tuning in private magic constants.
- ScriptableObject data is preferred for reusable tuning tables.
- Runtime object creation is allowed for bullets, hit effects, short-lived VFX, and pooled gameplay instances.
- Runtime creation is not allowed for whole scenes, full UI hierarchy, player prefab composition, monster prefab composition, or core art wiring unless explicitly requested.
- `Instantiate` must have an owner and a cleanup path. Do not use it in per-frame polling or as a substitute for authored prefab/scene setup.

## Forbidden Defaults

- Massive single-file implementations.
- Reflection-driven wiring for ordinary gameplay.
- Hardcoded absolute paths.
- Hardcoded Unity asset GUIDs.
- Automatic scene rebuilding on play.
- Per-frame or unbounded `Instantiate` calls.
- Broad `FindObjectOfType` dependency webs.
- Hidden fallback behavior that masks broken setup.
- Copying old-project code before checking whether the new project needs it.

## Acceptance Standard

A change is acceptable only if a reviewer can answer:

- What behavior changed?
- Which file owns it?
- How is it configured?
- How can it be tested?
- What did it deliberately not include?

For action-feel changes, a reviewer must also be able to answer which movement, camera, attack, dodge, hit, or enemy feel target improved.
