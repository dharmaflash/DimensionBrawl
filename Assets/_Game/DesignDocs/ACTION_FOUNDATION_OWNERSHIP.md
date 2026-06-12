# Action Foundation Ownership

Last updated: 2026-06-12 KST

This note permits the first action-feel foundation to add more than three gameplay scripts because the goal spans player, combat, enemy, presentation, and test-scene completion responsibilities. Each script must remain narrow and inspectable.

## Runtime Ownership

- `PlayerMovementController`: player locomotion, facing, camera-relative movement, stop-settle animation requests, and external planar movement bursts requested by player actions.
- `PlayerActionController`: player basic attack timing, dodge timing, player-side input buffering, melee hit checks, and player animation requests for attack/dodge.
- `CombatHealth`: health, damage validation, team filtering, temporary invulnerability, and death state.
- `BasicSoldierEnemy`: basic soldier approach, telegraph, attack execution, hit reaction, and death reaction.
- `ActionCameraController`: camera follow, target/threat bias, damping, and short additive cue offsets.
- `CombatHitFeedback`: presentation-only damage flash, death color, and short hit-stop response from damage events.
- `PlayerDodgeFeedback`: presentation-only dodge color cue driven by player action events.
- `ActionFoundationTestEncounter`: test setup win/fail state only.

## Explicit Non-Ownership

- No script owns summon behavior.
- No script owns boss phases.
- No script owns progression, rewards, currencies, or stage loops.
- No script constructs the full scene or full UI at runtime.
- No script depends on `Assets/_Imported/` paths or hardcoded asset GUIDs.
