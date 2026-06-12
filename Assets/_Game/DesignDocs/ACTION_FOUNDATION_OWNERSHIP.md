# Action Foundation Ownership

Last updated: 2026-06-12 KST

This note permits the first action-feel foundation to add more than three gameplay scripts because the goal spans player, combat, enemy, presentation, and test-scene completion responsibilities. Each script must remain narrow and inspectable.

## Runtime Ownership

- `PlayerMovementController`: player locomotion, facing, camera-relative movement, stop-settle animation requests, and external planar movement bursts requested by player actions.
- `PlayerActionController`: player basic attack timing, dodge timing, player-side input buffering, melee hit checks, and player animation requests for attack/dodge.
- `CombatHealth`: health, damage validation, team filtering, temporary invulnerability, and death state.
- `CombatTargetSensor`: shared enemy/future-summon target candidate evaluation, hostile team filtering, range checks, retarget cadence, and current target exposure. Candidate lists must be authored or provided by encounter code, not found through scene-wide searches.
- `BasicSoldierEnemy`: basic soldier `ClosePunish` sample behavior, approach, telegraph, attack execution, hit reaction, and death reaction. Enemy identity, pattern id, visual model, Animator controller, and trigger names must stay serialized/prefab-level data so future soldier variants can swap assets without rewriting behavior.
- `ActionCameraController`: camera follow, target/threat bias, damping, and short additive cue offsets.
- `CombatHitFeedback`: presentation-only damage flash and death color from damage events. It must not change global time scale for normal hits.
- `PlayerDodgeFeedback`: presentation-only dodge color cue driven by player action events.
- `EnemyAttackTelegraphPresenter`: presentation-only enemy windup/active readability, including telegraph scale/color and temporary visual pose offsets. It must not choose targets, apply damage, or own enemy pattern state.
- `ActionFoundationTestEncounter`: test setup win/fail state only.

## Explicit Non-Ownership

- No script owns summon behavior.
- No script owns summon spawning, summon slots, summon cooldowns, or summon UI.
- No script owns boss phases.
- No script owns progression, rewards, currencies, or stage loops.
- No script constructs the full scene or full UI at runtime.
- No script depends on `Assets/_Imported/` paths or hardcoded asset GUIDs.
- No enemy script hardcodes a specific model, Animator Controller, animation clip path, or material path.
- No normal-hit script owns global slow motion. Time-scale effects belong to a later explicit perfect-dodge, counter, ultimate, or authored cue bundle slice.
