# Code Style

## C# Basics

- Use namespaces rooted at `DimensionBrawl`.
- Use PascalCase for types, methods, properties, and events.
- Use camelCase for private fields and locals.
- Use `[SerializeField] private` for Inspector fields.
- Avoid public fields except constants or Unity-required data containers.
- Keep comments rare and useful. Explain why, not what.

## Unity Component Rules

- A MonoBehaviour should have one visible reason to exist.
- Prefer composition over inheritance for gameplay behavior.
- Avoid singletons unless the object is truly global and the decision is recorded in `DECISIONS.md`.
- Avoid `FindObjectOfType`, scene-wide searches, and tag lookups in hot paths.
- Cache component references in `Awake` or through serialized fields.
- Subscribe and unsubscribe events symmetrically.
- Do not mutate shared ScriptableObject data at runtime.

## File Size Guardrails

These are soft limits, but exceeding them requires a reason:

- MonoBehaviour: about 200 lines.
- Plain data/model class: about 150 lines.
- Method: about 40 lines.
- More than three responsibilities in one file means split the design first.

## Data And Tuning

- Gameplay tuning that designers may change belongs in serialized fields or ScriptableObjects.
- Magic numbers in code are allowed only for local, obvious math.
- Do not hardcode asset paths, GUIDs, scene names, layers, or tags unless the project has a named constants/data asset for them.

## Runtime Creation

Allowed:

- Projectiles
- Hitboxes
- Temporary VFX/audio emitters
- Pooled enemies or combat objects from authored prefabs

Not allowed by default:

- Generating player prefabs
- Generating enemy prefabs
- Generating full UI hierarchy
- Generating full scenes
- Rebuilding authored art wiring on play

## Runtime Object Lifetime

`Instantiate` is normal Unity code when it represents a real runtime event. The problem is not instantiation itself; the problem is unowned, repeated, or invisible object creation.

Use these rules:

- Instantiate authored prefabs, not ad hoc hierarchies assembled in code.
- Do not call `Instantiate` directly from `Update`, `FixedUpdate`, or per-frame polling paths.
- Creation should belong to a clear owner such as a spawner, weapon, projectile emitter, VFX trigger, or pool.
- Every spawned object needs a cleanup path: lifetime, despawn, pool return, scene unload, or owning system reset.
- Set parent transforms intentionally when spawned objects are organizational or presentation children.
- Use pooling for frequently repeated objects after profiling or when the expected rate is obviously high.
- Do not create objects to hide missing prefab references or broken scene setup.
- Prefer scene-authored anchors and serialized prefab references over hardcoded `Resources.Load` paths.

## Review Expectations

Every code change should make it clear:

- where the behavior lives,
- how it is configured,
- what prefab or scene setup is required,
- how to verify it,
- what was intentionally left out.
