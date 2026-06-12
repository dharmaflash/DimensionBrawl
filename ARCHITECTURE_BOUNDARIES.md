# Architecture Boundaries

## Ownership

### Player

Owns player movement, facing, dodge, attack input interpretation, and player animation requests.

Must not own enemy spawning, encounter pacing, global UI, or boss phase logic.

### Enemy

Owns enemy movement, target choice, attack execution, hit reaction, and death.

Must not own player input, camera control, scene setup, or global progression.

### Combat

Owns damage events, hit validation, health, team/faction rules, and temporary combat effects.

Must not own authored asset import, prefab construction, or UI layout.

### Presentation

Owns animation, VFX, audio, camera cues, and UI presentation glue.

Must not decide gameplay outcomes.

### Content Data

Owns reusable tuning: enemy stats, attack timing, movement values, encounter definitions, and animation references.

Must not contain executable gameplay logic.

## Prefab Policy

- Player and enemy prefabs should be authored assets.
- Runtime should instantiate selected prefabs, not construct them from empty GameObjects.
- Generated scaffolds are allowed only for temporary editor tools and must be marked as such.
- Spawned runtime objects must have a clear owner and lifecycle. If no one owns cleanup, the object should probably be authored or pooled differently.

## Scene Policy

- A playable scene should be visible and inspectable in Unity.
- Runtime bootstrapping should connect existing authored anchors.
- Avoid invisible self-building scenes.

## Dependency Direction

Game code may depend on data and presentation interfaces. Data must not depend on gameplay services. Presentation must not decide combat state.

## Growth Rule

If a new feature needs more than three new scripts, pause and write the ownership split first.
