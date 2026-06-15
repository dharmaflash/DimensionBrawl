# Architecture Boundaries

## Ownership

### Player

Owns player movement, facing, dodge, local-defense attack input interpretation, target-bias intent, and player animation requests.

Must not own enemy spawning, summon AI, summon roster/economy, encounter pacing, global UI, or boss phase logic.

### Summon

Owns one summon actor or assist action's entry, target use, role action, animation requests, impact, exit, and cleanup.

Must not own player input beyond a narrow summon-slot request, enemy spawning, stage progression, full summon inventory/economy, global UI layout, or boss phase logic.

### Enemy

Owns enemy movement, target choice, attack execution, hit reaction, and death.

Must not own player input, summon-slot input, camera control, scene setup, or global progression.

### Combat

Owns damage events, hit validation, health, team/faction rules, temporary combat effects, and reviewed combat resources such as the first `EN LV1~LV3` skill/summon energy ladder.

Must not own authored asset import, prefab construction, UI layout, account progression, rarity, inventory, or permanent summon upgrades.

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
