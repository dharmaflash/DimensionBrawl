# Action Foundation Ownership

Last updated: 2026-06-13 KST

This note permits the first action-feel foundation to add more than three gameplay scripts because the goal spans player, combat, enemy, presentation, and test-scene completion responsibilities. Each script must remain narrow and inspectable.

## Runtime Ownership

- `PlayerMovementController`: player locomotion, facing, camera-relative movement, stop-settle animation requests, and external planar movement bursts requested by player actions.
- `PlayerActionController`: player basic attack timing, dodge timing, player-side input buffering, melee hit checks, and player animation requests for attack/dodge.
- `CombatHealth`: health, damage validation, team filtering, temporary invulnerability, and death state.
- `CombatTargetSensor`: shared enemy/future-summon target candidate evaluation, hostile team filtering, range checks, retarget cadence, and current target exposure. Candidate lists must be authored or provided by encounter code, not found through scene-wide searches.
- `BasicSoldierEnemy`: basic soldier pattern sample execution, approach, telegraph, optional deck-based profile selection, profile-driven attack shape, attack execution, hit reaction, and death reaction. Enemy identity, pattern id, visual model, Animator controller, animation trigger names, timing, attack shape, direction-lock behavior, telegraph, and camera cue data must stay serialized/prefab-level, `CombatAiPatternProfile`, or `CombatAiPatternDeck` data so future soldier variants can swap assets and patterns without rewriting behavior.
- `CombatAiPatternDeck`: data-only pattern selection by target distance, cooldown, and priority. It may choose among authored `CombatAiPatternProfile` assets for one actor type, but it must not search the scene, load assets at runtime, instantiate gameplay objects, or branch on specific pattern ids.
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
- No enemy script hardcodes behavior by specific pattern id when the same outcome can be expressed through profile or deck data.
- No normal-hit script owns global slow motion. Time-scale effects belong to a later explicit perfect-dodge, counter, ultimate, or authored cue bundle slice.
