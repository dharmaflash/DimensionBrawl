# Action Foundation Ownership

Last updated: 2026-06-13 KST

This note permits the first action-feel foundation to add more than three gameplay scripts because the goal spans player, combat, enemy, presentation, and test-scene completion responsibilities. Each script must remain narrow and inspectable.

## Runtime Ownership

- `PlayerMovementController`: player locomotion, facing, camera-relative movement, stop-settle animation requests, and external planar movement bursts requested by player actions.
- `PlayerActionController`: player basic attack timing, dodge timing, player-side input buffering, melee hit checks, and player animation requests for attack/dodge.
- `CombatHealth`: health, damage validation, team filtering, temporary invulnerability, damage-modification hook dispatch, and death state. It may expose a generic damage-modification context for shields, armor, future summon protection, or buffs, but it must not know specific enemy pattern ids.
- `CombatTargetSensor`: shared enemy/future-summon target candidate evaluation, hostile team filtering, range checks, retarget cadence, and current target exposure. Candidate lists must be authored or provided by encounter code, not found through scene-wide searches.
- `BasicSoldierEnemy`: basic soldier pattern sample execution, approach, optional pre-attack reposition, telegraph, optional deck-based profile selection, profile-driven attack shape, attack execution, hit reaction, and death reaction. Enemy identity, pattern id, visual model, Animator controller, prepare/attack/hit/death animation trigger names, timing, attack shape, direction-lock behavior, telegraph, and camera cue data must stay serialized/prefab-level, `CombatAiPatternProfile`, or `CombatAiPatternDeck` data so future soldier variants can swap assets and patterns without rewriting behavior.
- `CombatAiPatternDeck`: data-only pattern selection by target distance, cooldown, and priority. It may choose among authored `CombatAiPatternProfile` assets for one actor type, but it must not search the scene, load assets at runtime, instantiate gameplay objects, or branch on specific pattern ids.
- `CombatEnemyRoleProfile`: data-only enemy role catalog entry for linear run composition. It may name role identity, preferred run segment, recommended count range, pressure purpose, intended player/summon answer, starting pattern, pattern deck, and elite trait profiles. It must not spawn actors, select scene objects, load assets at runtime, or replace authored enemy prefab/model ownership.
- `CombatEnemyArchetypeProfile`: data-only role-to-presentation candidate mapping. It may group compatible role profiles, identify a mobile soldier/static turret/boss-candidate archetype, reference already promoted `_Game` prefab/model assets, and record raw-pack promotion notes. It must not point object references at `_Imported`, import packages, build prefabs, spawn enemies, or alter pattern deck selection.
- `CombatAiElitePatternProfile`: data-only elite trait profile for `ShieldCycle`, `ArmorBreak`, `AuraBuffer`, `SummonPackage`, and `PhaseSwap`. It may hold trigger ratios, guard meters, damage multipliers, signal durations, signal animation trigger names, colors, and replacement profile/deck references, but it must not execute combat or spawn units.
- `EnemyElitePatternController`: enemy-local elite trait runtime layer. It may consume `CombatAiElitePatternProfile` assets, request the profile's signal animation trigger on an authored Animator reference, modify incoming damage through `CombatHealth.DamageModifying`, maintain shield/armor break state, protect explicitly authored aura receiver targets, expose readable signal state, activate pre-authored summon signal objects, and swap the attached soldier profile/deck for phase changes. It must not own base approach/attack execution, search the scene for allies, instantiate summons, or branch on concrete profile ids.
- `ActionCameraController`: camera follow, target/threat bias, damping, and short additive cue offsets.
- `CombatHitFeedback`: presentation-only damage flash and death color from damage events. It must not change global time scale for normal hits.
- `PlayerDodgeFeedback`: presentation-only dodge color cue driven by player action events.
- `EnemyAttackTelegraphPresenter`: presentation-only enemy windup/active readability, including telegraph scale/color and temporary visual pose offsets. It must not choose targets, apply damage, or own enemy pattern state.
- `CombatVfxCueProfile`: data-only combat VFX cue table. It may reference selected game-owned VFX prefabs promoted under `_Game/Art/VFX`, cue offsets, lifetime, alignment, parent policy, and prewarm count. It must not reference raw `_Imported` assets directly.
- `CombatVfxCueVisual`: presentation-only cue-local mesh color/scale/spin/lift fade for promoted `_Game` VFX prefabs. It must not decide which cue plays, search for combat actors, or apply damage.
- `CombatVfxCuePlayer`: presentation-only VFX playback and bounded per-prefab pooling. It may instantiate from authored cue-profile prefabs into its local pool, restart `CombatVfxCueVisual` components, play particle systems or VFX Graph components when a reviewed promoted prefab needs them, and release instances after cue lifetime. It must not decide combat outcomes, search the scene for targets, or load assets by path.
- `PlayerCombatVfxCueDriver` and `EnemyCombatVfxCueDriver`: presentation-only event adapters from player action, enemy pattern state, damage, and death events into `CombatVfxCuePlayer`. They must not own action timing, enemy AI, hit validation, or asset selection beyond serialized cue anchors and profile references.
- `ActionFoundationTestEncounter`: test setup win/fail state only.

## Explicit Non-Ownership

- No script owns summon behavior.
- No script owns summon spawning, summon slots, summon cooldowns, or summon UI.
- `SummonPackage` may activate pre-authored signal objects only; no script may add unreviewed runtime add spawning under that name.
- `AuraBuffer` may protect explicitly authored receiver targets only; scene-search-based squad aura wiring needs an explicit reviewed slice.
- No script owns boss phases.
- No script owns progression, rewards, currencies, or stage loops.
- No script constructs the full scene or full UI at runtime.
- No script depends on `Assets/_Imported/` paths or hardcoded asset GUIDs.
- No VFX driver may reference source asset-pack prefabs directly. Promote a reviewed effect to `_Game/Art/VFX` and connect it through cue-profile data.
- No enemy script hardcodes a specific model, Animator Controller, animation clip path, or material path.
- No enemy script hardcodes behavior by specific pattern id when the same outcome can be expressed through profile or deck data.
- No role deck or archetype profile may directly reference raw asset-store prefabs. Promote reviewed prefab/model/VFX pieces into `_Game` before assigning object references.
- No normal-hit script owns global slow motion. Time-scale effects belong to a later explicit perfect-dodge, counter, ultimate, or authored cue bundle slice.
