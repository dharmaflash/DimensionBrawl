# Linear Stage Design Foundation

## Purpose

This document fixes the first scalable level-design shape for DimensionBrawl's direct-control ARPG route. It turns the collected ARPG, boss/enemy-run, and stage/reward research into authoring data before runtime spawning, progression, rewards, or summon behavior is implemented.

The foundation is intentionally data-first:

- `LinearStageTemplateProfile` defines a stage template that can become a stage-select row later.
- `LinearStageSegmentProfile` defines a linear route beat such as entry read, break gate, backline pressure, relief, boss handoff, or final stand.
- `LinearStagePocket` defines a smaller encounter pocket inside a segment.
- `StageEnemyRoleSlot` references existing `CombatEnemyRoleProfile` assets only.

This is not a wave spawner, scene generator, summon system, reward system, or boss controller.

## V1 Authoring Contract

Use these terms when discussing the first linear stage skeleton:

| Design Term | Unity Data | Responsibility |
|---|---|---|
| `StageRoute` | `LinearStageTemplateProfile` | Names one stage template, route order, target run duration, featured future summon need, mastery text, and exclusions. |
| `CombatSegment` | `LinearStageSegmentProfile` | Names one route beat, pacing envelope, camera readability intent, relief requirement, and its encounter pockets. |
| `EncounterSlot` | `LinearStagePocket` | Names one authored encounter pocket inside a segment, including intensity, duration, future summon need, objective, and enemy role slots. |
| `Objective` | `LinearStageObjectiveKind` plus `objectiveCue` | Classifies what the pocket asks the player to do, while the cue gives the human-readable instruction. |

The first implementation route should consume data in that order: `StageRoute -> CombatSegment -> EncounterSlot -> Objective/RoleSlot`. Do not invert it by starting from prefabs, runtime spawns, rewards, or summon units.

The current objective kinds are:

| Objective | Use |
|---|---|
| `ReadThreat` | Teach a first tell or safe enemy read. |
| `PunishRecovery` | Reinforce dodge, counter, and close-pressure recovery punish. |
| `BreakGuard` | Teach guarded/armored pressure and the future Break answer. |
| `PrioritizeBackline` | Teach chasing or answering rear line/projectile pressure. |
| `SurvivePressure` | Create overload where future Tank/Heal answers would matter. |
| `RecoverPosition` | Provide relief, camera recenter, and spacing reset. |
| `ReadPhaseHandoff` | Rehearse pre-boss phase/deck handoff grammar without a real boss. |
| `FinalClear` | Combine learned reads into the stage clear condition. |

## Reference Basis

The current data and research support the following authoring rules:

- `COMBAT_V1_SPEC.md` defines the game as a direct-control ARPG that clears linear combat sections before the summon system is implemented.
- `BOSS_ENEMY_RUN_REFERENCE_RESEARCH.md` identifies pressure modules, telegraph stages, relief windows, boss handoff states, and the need for encounter definitions to name pressure rhythm and intended answers.
- `STAGE_REWARD_GROWTH_REFERENCE_RESEARCH.md` proposes the first stage set: `S1-1 Break Gate`, `S1-2 Backline Signal`, `S1-3 Tank Rescue`, `S1-4 Heal Pocket`, and `S1-5 Boss Stand`.
- `ARPG_REFERENCE_RESEARCH.md` supports small named camera/pressure presets, readable target priority, 3-5 minute run rhythm, pressure relief, and boss-phase handoff grammar.
- Existing `CombatEnemyRoleProfile` assets already cover the run segments needed for first-pass stage composition.

## Stage Template Types

### `TutorialRun`

Use for the earliest route where one combat lesson is isolated. It should start with `EntryRead`, include one featured pressure lesson, include a relief beat if the pressure spikes, and end with `FinalStand`.

### `StandardStoryRun`

Use for a normal 3-5 minute ARPG route. It should mix basic pressure, a featured pressure pocket, a relief beat, and a final stand without adding new rules in the final segment.

### `BacklineLesson`

Use when the lesson is priority targeting, projectile/line pressure, or future Arrow-style answer. The stage can use `LineCaster`, `BacklineShooter`, and optional `AuraCaptainElite`, but it must remain readable without a finished lock-on UI.

### `ElitePressureRun`

Use when the route needs sustained pressure and future Tank/Heal answer windows. It can mix general roles with one elite role, but should still name the intended answer and relief beat.

### `BossHandoffDrill`

Use before a real dragon boss exists. It validates phase/deck handoff language with `PhaseDuelistElite` and `FinalStandCommanderElite`, but must not reference a dragon prefab or boss controller.

## Segment Types

| Segment | Purpose | Common Roles |
|---|---|---|
| `EntryRead` | First safe read for camera, movement, and one tell. | `EntryProbe` |
| `BasicPressure` | Reinforce dodge, punish, and target-facing basics. | `CloseGuard`, optional `LungeChaser` |
| `BreakGate` | Teach guarded pressure and future Break answer. | `CloseGuard`, optional `ShieldBreakerElite` |
| `BacklinePressure` | Teach backline priority and line/projectile reads. | `LineCaster`, `BacklineShooter`, optional `AuraCaptainElite` |
| `PressureRescue` | Build overload where future Tank/Heal answers matter. | `FanSuppressor`, `LungeChaser`, `Skirmisher`, optional `SummonCallerElite` |
| `Relief` | Give a reset beat after a spike. | no required enemies |
| `BossBreakHandoff` | Teach phase/deck handoff before real boss work. | `PhaseDuelistElite`, optional `ShieldBreakerElite`, `LineCaster` |
| `FinalStand` | Combine prior reads into the clear condition. | `FinalStandCommanderElite`, `BacklineShooter`, `FanSuppressor`, optional `Skirmisher` |

## First Stage Set

| Template | Featured Need | Linear Route |
|---|---|---|
| `S1-1 Break Gate` | `Break` | EntryRead -> BasicPressure -> BreakGate -> Relief -> FinalStand |
| `S1-2 Backline Signal` | `Arrow` | EntryRead -> BasicPressure -> BacklinePressure -> Relief -> FinalStand |
| `S1-3 Tank Rescue` | `Tank` | EntryRead -> BasicPressure -> PressureRescue -> Relief -> FinalStand |
| `S1-4 Heal Pocket` | `Heal` | EntryRead -> BasicPressure -> BacklinePressure -> PressureRescue -> Relief -> FinalStand |
| `S1-5 Boss Stand` | `Any` | EntryRead -> BasicPressure -> BreakGate -> BacklinePressure -> Relief -> PressureRescue -> BossBreakHandoff -> FinalStand |

These names are stage-design promises only. They do not unlock summons, pay rewards, spawn waves, or create boss phases.

## First Review Route Skeleton

Use `S1-1 Break Gate` as the first encounter-composition review target. It is short enough to inspect manually and still exercises the whole route chain.

| Order | Segment | EncounterSlot | Objective | Intended Roles | Review Question |
|---|---|---|---|---|---|
| 1 | `EntryRead` | `entry_probe_teach` | `ReadThreat` | `EntryProbe` | Can the player read one enemy tell with the current camera and movement? |
| 2 | `BasicPressure` | `close_guard_reinforce` | `PunishRecovery` | `CloseGuard`, optional `LungeChaser` | Does close pressure invite dodge, punish, and target-facing basics without clutter? |
| 3 | `BreakGate` | `guard_gate_spike` | `BreakGuard` | `CloseGuard`, optional `ShieldBreakerElite` | Is the guarded threat obvious enough to justify a future Break answer? |
| 4 | `Relief` | `reset_breath` | `RecoverPosition` | none | Does the route breathe before the final stand? |
| 5 | `FinalStand` | `final_stand_mix` | `FinalClear` | `FinalStandCommanderElite`, `BacklineShooter`, `FanSuppressor`, optional `Skirmisher` | Does the final pocket combine prior reads without adding a new rule? |

The review scene for this route should place already-authored role candidate prefabs by hand or through a dedicated editor setup slice. It should not add a runtime wave spawner, hidden prefab selector, reward payout, summon behavior, or boss phase logic.

## Authoring Boundaries

- Stage templates may reference `LinearStageSegmentProfile` assets.
- Segment pockets may reference `CombatEnemyRoleProfile` assets.
- Stage templates must not reference raw `_Imported` assets.
- Stage templates must not reference prefabs, scenes, cameras, UI objects, rewards, or actual summon units.
- Enemy role data remains behavior intent. Presentation assignment stays in role candidate and archetype profiles.
- Runtime systems may consume this data later, but they must not mutate these ScriptableObjects at runtime.

## Validation

Use:

- `DimensionBrawl > Validate Action Foundation Stage Design Templates`

Validation checks:

- All core segment types exist.
- Stage templates start with `EntryRead` and end with `FinalStand`.
- Each stage has enough route beats and at least one relief segment.
- Every pocket declares a `LinearStageObjectiveKind` and a readable `objectiveCue`.
- Non-relief pockets have at least one enemy role slot.
- Role slots use game-owned role profiles and valid count/weight data.

## Next Follow-Ups

1. Review the authored stage templates in Unity's Inspector.
2. Create a small encounter-composition review scene that places role candidate prefabs according to one template.
3. Only after review, add a narrow runtime encounter owner that consumes authored stage/segment data.
4. Keep summon implementation, reward payout, boss phases, and stage-select UI separate until this foundation is accepted.
