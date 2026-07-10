# Linear Stage Design Foundation

## Purpose

This document fixes the first scalable level-design shape for DimensionBrawl's fixed-rear boss-barrage lane route. It turns the collected ARPG, summon, boss/enemy-run, and stage/reward research into authoring data before runtime spawning, progression, rewards, boss phases, or full summon economy is implemented.

The foundation is intentionally data-first:

- `LinearStageTemplateProfile` defines a stage template that can become a stage-select row later.
- `LinearStageSegmentProfile` defines a linear route beat such as entry read, break gate, backline pressure, relief, boss handoff, or final stand.
- `LinearStagePocket` defines a smaller encounter pocket inside a segment.
- `StageEnemyRoleSlot` references existing `CombatEnemyRoleProfile` assets only.
- The first playable lane must additionally define player-side bounds, a forward boundary, boss/proxy side, projectile pressure lanes, and summon-energy risk zones before it becomes a runtime system.

This is not a wave spawner, scene generator, full summon system, reward system, or boss controller.

## V1 Authoring Contract

Use these terms when discussing the first linear stage skeleton:

| Design Term | Unity Data | Responsibility |
|---|---|---|
| `StageRoute` | `LinearStageTemplateProfile` | Names one stage template, route order, target run duration, featured future summon need, mastery text, and exclusions. |
| `CombatSegment` | `LinearStageSegmentProfile` | Names one route beat, pacing envelope, camera readability intent, relief requirement, and its encounter pockets. |
| `EncounterSlot` | `LinearStagePocket` | Names one authored encounter pocket inside a segment, including intensity, duration, future summon need, objective, and enemy role slots. |
| `Objective` | `LinearStageObjectiveKind` plus `objectiveCue` | Classifies what the pocket asks the player to do, while the cue gives the human-readable instruction. |

The first implementation route should consume data in that order: `StageRoute -> CombatSegment -> EncounterSlot -> Objective/RoleSlot`. Do not invert it by starting from prefabs, runtime spawns, rewards, or a full summon roster.

The current objective kinds are:

| Objective | Use |
|---|---|
| `ReadThreat` | Teach a first tell or safe enemy read. |
| `PunishRecovery` | Reinforce dodge, counter, and close-pressure recovery punish. |
| `BreakGuard` | Teach guarded/armored pressure and the Break summon answer. |
| `PrioritizeBackline` | Teach chasing or answering rear line/projectile pressure. |
| `SurvivePressure` | Create overload where Tank/Heal-style summon answers would matter. |
| `RecoverPosition` | Provide relief, camera recenter, and spacing reset. |
| `ReadPhaseHandoff` | Rehearse pre-boss phase/deck handoff grammar without a real boss. |
| `FinalClear` | Combine learned reads into the stage clear condition. |

## Reference Basis

The current data and research support the following authoring rules:

- `COMBAT_V1_SPEC.md` defines the current game direction as fixed-rear boss-barrage + summon-first lane combat. Stage pockets should create readable reasons to take forward risk, charge summon energy, and use summon roles instead of treating summons as late decoration.
- `BOSS_ENEMY_RUN_REFERENCE_RESEARCH.md` identifies pressure modules, telegraph stages, relief windows, boss handoff states, and the need for encounter definitions to name pressure rhythm and intended answers.
- `STAGE_REWARD_GROWTH_REFERENCE_RESEARCH.md` proposes the first stage set: `S1-1 Break Gate`, `S1-2 Backline Signal`, `S1-3 Tank Rescue`, `S1-4 Heal Pocket`, and `S1-5 Boss Stand`.
- `ARPG_REFERENCE_RESEARCH.md` supports small named camera/pressure presets, readable target priority, 3-5 minute run rhythm, pressure relief, and boss-phase handoff grammar.
- Existing `CombatEnemyRoleProfile` assets already cover the run segments needed for first-pass stage composition.
- `C:/Ark/SubcultureGameData/games/arknights/notes/route-topology-pressure-2026-06-14.md` and `pressure-curves-2026-06-14.md` provide usable pressure-shape references for burst/readability sequencing.

Quantitative guidance for lane shaping:

- route weighted pressure median/p90/max: `22 / 58 / 600.85`
- stage weighted pressure median/p90/max: `654.4 / 1437.5 / 5209.9`
- 15-second peak pressure share median/p90: `28.98 / 45.49`
- 15-second top-3 pressure share median/p90: `66.36 / 85.9`
- dominant endpoint pair pressure share median/p90: `39.05 / 63.8`
- endpoint pair weighted pressure median/p90/max: `71 / 370.9 / 2547.05`

Design translation for this project:

- Keep runs readable if they contain one designed burst then relief by default; avoid flat pressure for the first two review routes.
- Avoid 2+ consecutive dominant-lane pockets by default; if one route pocket is intentionally dominant, separate with relief or role-switch pocket.
- When forward-risk zones are the key decision point, annotate those pockets with stronger lane differentiation and stronger follow-up timing risk.
- For backline pockets, intentionally lower immediate threat density but preserve a clear read of how fast enemies can re-enter pressure.

## Arknights-Style Pressure Gates (Design-only)

These are design-side checks only for review routes and are not runtime formulas:

- `PressureBurstIndex`: at least one high-intensity pocket per stage route before end-state.
- `ReliefSpacingTarget`: relief within 1–2 active pressure pockets in early routes.
- `RouteBurdenConcentration`: dominant lane share should be high enough to read, but not so high that only one path is viable.
- `ForwardBackRiskDifferential`: forward pocket reads should feel sharper/shorter than backline pockets to keep summon timing meaningful.

## Stage Template Types

### `TutorialRun`

Use for the earliest route where one combat lesson is isolated. It should start with `EntryRead`, include one featured pressure lesson, include a relief beat if the pressure spikes, and end with `FinalStand`.

### `StandardStoryRun`

Use for a normal 3-5 minute ARPG route. It should mix basic pressure, a featured pressure pocket, a relief beat, and a final stand without adding new rules in the final segment.

### `BacklineLesson`

Use when the lesson is priority targeting, projectile/line pressure, or Arrow-style summon answer. The stage can use `LineCaster`, `BacklineShooter`, and optional `AuraCaptainElite`, but it must remain readable without a finished lock-on UI.

### `ElitePressureRun`

Use when the route needs sustained pressure and Tank/Heal-style summon answer windows. It can mix general roles with one elite role, but should still name the intended answer and relief beat.

### `BossHandoffDrill`

Use before a real dragon boss exists. It validates phase/deck handoff language with `PhaseDuelistElite` and `FinalStandCommanderElite`, but must not reference a dragon prefab or boss controller.

## Segment Types

| Segment | Purpose | Common Roles |
|---|---|---|
| `EntryRead` | First safe read for camera, movement, and one tell. | `EntryProbe` |
| `BasicPressure` | Reinforce dodge, punish, and target-facing basics. | `CloseGuard`, optional `LungeChaser` |
| `BreakGate` | Teach guarded pressure and Break summon answer. | `CloseGuard`, optional `ShieldBreakerElite` |
| `BacklinePressure` | Teach backline priority and line/projectile reads. | `LineCaster`, `BacklineShooter`, optional `AuraCaptainElite` |
| `PressureRescue` | Build overload where Tank/Heal-style summon answers matter. | `FanSuppressor`, `LungeChaser`, `Skirmisher`, optional `SummonCallerElite` |
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

These names are stage-design promises only. They do not pay rewards, spawn waves, create boss phases, or imply the full summon economy exists. A narrow `SummonSlot1` review slice may consume one lane pocket later when the boss-barrage/summon-first loop is ready.

## First Review Route Skeleton

Use `S1-1 Break Gate` as the preserved first encounter-composition review target. It is short enough to inspect manually and still exercises the whole route chain. For the new pivot, treat it as a data/progression reference until a fixed-rear boss-barrage + `SummonSlot1` lane pocket is authored.

The first new pivot review pocket should prove:

- fixed rear camera readability,
- a player-side movement zone with an uncrossable forward boundary,
- a back safety zone and forward risk zone,
- `EN LV1~LV3` charge pressure tied to forward risk,
- faster summon-energy gain near the forward boundary,
- skill/summon buttons upgrading by available EN tier and resetting after spend,
- boss/proxy projectiles with tighter front-position risk and looser back-position risk,
- close or approaching monsters that can be answered by `BasicDefenseAttack`,
- one `SummonSlot1` action that changes the boss-barrage exchange.

| Order | Segment | EncounterSlot | Objective | Intended Roles | Review Question |
|---|---|---|---|---|---|
| 1 | `EntryRead` | `entry_probe_teach` | `ReadThreat` | `EntryProbe` | Can the player read one enemy tell with the current camera and movement? |
| 2 | `BasicPressure` | `close_guard_reinforce` | `PunishRecovery` | `CloseGuard`, optional `LungeChaser` | Does close pressure invite dodge, punish, and target-facing basics without clutter? |
| 3 | `BreakGate` | `guard_gate_spike` | `BreakGuard` | `CloseGuard`, optional `ShieldBreakerElite` | Is the guarded threat obvious enough to justify a future Break answer? |
| 4 | `Relief` | `reset_breath` | `RecoverPosition` | none | Does the route breathe before the final stand? |
| 5 | `FinalStand` | `final_stand_mix` | `FinalClear` | `FinalStandCommanderElite`, `BacklineShooter`, `FanSuppressor`, optional `Skirmisher` | Does the final pocket combine prior reads without adding a new rule? |

The former `ActionFoundationStageBreakGateReview` scene and its review-only runtime owners have been retired. The ScriptableObject route data remains authoritative for this foundation; playable integration belongs in the canonical combat reviews and the Olympus runtime stage scenes.

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
2. Validate playable route readability in the canonical combat reviews and Olympus runtime stages.
3. Keep full summon economy, reward payout, boss phases, and stage-select UI separate until this foundation is accepted.
4. For the current pivot, author the next review pocket around fixed-rear boss projectile pressure, forward-risk `EN LV1~LV3` charge, tiered skill/summon spend reset, `BasicDefenseAttack` close-threat handling, and one `SummonSlot1` answer before expanding a whole chapter.
