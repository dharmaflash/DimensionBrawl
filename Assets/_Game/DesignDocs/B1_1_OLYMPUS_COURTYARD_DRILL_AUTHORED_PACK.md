# B1-1 Olympus Courtyard Drill Authored Pack

Status: `IMPLEMENTED / VISUAL + FULL-REGRESSION VERIFIED / PRODUCT-QUARANTINED`

Authored: 2026-07-21 KST

Verification checkpoint: 2026-07-22 KST

Historical note: this B1-1 quarantine record is a historical checkpoint. B1-2
superseded its pre-admission product state on 2026-07-22.

## Outcome

B1-1 authors the first real compact second-stage content pack on top of the reusable B0
runtime seams. It contains one independently identified scene, one one-row playable route,
and nine distinct persistent data assets. It does not publish a second product catalog
entry or add the scene to Unity Build Settings.

The admitted route is deliberately bounded:

- `OLYMPUS-COURTYARD-DRILL-01` owns one `courtyard_drill_combat` segment;
- segment zero is both `Entry` and `Terminal`, then returns to the route owner;
- one scene-authored boss `CombatHealth` is the encounter's actual terminal enemy;
- one `SciFiSoldier.Ranged` Rifle Crossfire Add is activated by the neutral
  `StageCountOneEncounterExecutor` at `SceneReady`;
- the Add is an independent runtime participant, never `encounter.EnemyHealth`, and is
  cancelled when the boss or player closes the terminal encounter; and
- the route authors no cinematic, tutorial requirement, reward, availability edge, or
  persistent progression behavior.

This is a content-authoring and fully regressed playable-stage proof. It is not yet a
second shipped Stage Select card.

## Reused local foundations

The pack reuses project-owned seams instead of copying either full Olympus stage scene:

| Existing foundation | B1-1 use |
|---|---|
| B0-1 bounded one-row route topology | Admit one exact `Entry | Terminal` segment without handoff evidence |
| B0-2 truthful one-row facts/result | Seal one segment and an empty tutorial digest for Clear or Fail |
| B0-3 neutral scene adapters | Use `OneRowStageRunBootstrap`, `OneRowStageRunFactAdapter`, and `OneRowStageRunResultPresenter` rather than route-named Corridor/Station owners |
| B0-4 catalog/build manifest seam | Keep the authored candidate outside the accepted product catalog and every Build Settings row |
| A1/A2 ordered Add execution | Reuse the count-one executor and promoted ranged archetype with exact runtime ownership and cleanup |
| Shared combat presentation | Reuse the project combat HUD, `OneRowCombatHudBinder`, result-overlay interface, promoted character/enemy visuals, and modular Olympus environment prefabs |

The scene contains no `OlympusCorridorCombatFlowController`,
`OlympusStationRunFactCollector`, or `OlympusStationCombatResultPresenter`. The result and
progression sidecars are independent B1-1 audit/admission sources, not aliases of the
accepted Olympus product assets.

## Exact authored scene contract

`OlympusCourtyardDrillStage.unity` owns one active scene binding and these exact authored
combat positions:

| Role | Anchor | Position ID | Runtime ownership |
|---|---|---:|---|
| Player | `Player_Start` | `1101` | Scene-authored player subject |
| Terminal boss | `Boss_Terminal` | `1201` | Scene-authored `encounter.EnemyHealth` |
| Rifle Crossfire Add | `Add_RifleCrossfire` | `1301` | Runtime-instantiated by the sole count-one executor |

The static scene owns exactly two `CombatHealth` subjects: player and terminal boss. The
Add's health, AI, target sensor, projectile ownership, and player-target registration are
created and released by its executor. The player selector retains the boss as its one
authored candidate while accepting the live Add as a runtime candidate.

The scene also owns exactly one coordinated `CombatEncounterController`, one each of the
three neutral one-row adapters, one `StageCountOneEncounterExecutor`, one
`OneRowCombatHudBinder`, one player target selector, one active camera, one active audio
listener, and one active event system. The Add executor is sealed to `SceneReady`, requires
an active stage run, and cancels on terminal encounter resolution.

## Persistent authored pack

The scene and nine persistent data assets are:

| Kind | Path |
|---|---|
| Scene | `Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity` |
| Stage definition | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCourtyardDrillCombat.asset` |
| Playable route | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusCourtyardDrill.asset` |
| Linear template | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusCourtyardDrillRun.asset` |
| Result presentation profile | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentation_OlympusCourtyardDrill.asset` |
| Result localization | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultLocalization_OlympusCourtyardDrill.asset` |
| Result presentation catalog | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog_OlympusCourtyardDrill.asset` |
| Result definition | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusCourtyardDrill.asset` |
| Progression node | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusCourtyardDrill.asset` |
| Progression graph | `Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusCourtyardDrill.asset` |

The progression graph contains only the isolated Courtyard node. It has no prerequisite
or recommended-next edge, and the result definition admits no reward plan. These assets
preserve the existing exact route/result/progression join contract without claiming a
player-profile unlock system.

The result definition owns four outcome-specific presentation mappings, not three unique
rows:

| Outcome | Action | Role | Order |
|---|---|---|---:|
| Clear | Replay | Primary | 0 |
| Clear | Lobby | Secondary | 1 |
| Fail | Retry | Primary | 0 |
| Fail | Lobby | Secondary | 1 |

Lobby is the only repeated action ID, once for each outcome. Each row retains its exact
localization key.

## Authoring and validation entrypoints

| Purpose | Batch entrypoint or focused surface |
|---|---|
| Build/update the nine isolated sidecars | `DimensionBrawl.Editor.OlympusCourtyardDrillStagePackSetup.RunBatchSetup` |
| Fresh-load validation of those sidecars | `DimensionBrawl.Editor.OlympusCourtyardDrillStagePackSetup.RunBatchValidation` |
| Build the compact scene from an empty scene | `DimensionBrawl.Editor.OlympusCourtyardDrillStageSceneSetup.RunBatchSetup` |
| Validate exact asset identity, digest, isolation, and static scene contract | `DimensionBrawl.Editor.OlympusCourtyardDrillAuthoredPackValidator.RunBatchVerification` |
| Validate B1-1 catalog/Build Settings quarantine | `DimensionBrawl.Editor.OlympusCourtyardDrillB11QuarantineGate.RunBatchVerification` |
| Capture a stable direct-scene visual QA frame without product admission | `DimensionBrawl.Editor.OlympusCourtyardDrillStageVisualQaCapture.RunBatchCapture` |
| Focused stage runtime proof | `DimensionBrawl.Tests.OlympusCourtyardDrillStagePlayModeTests` |
| Shared HUD binder proof | `DimensionBrawl.Tests.OneRowCombatHudBinderPlayModeTests` |

The data setup uses accepted Olympus assets only as creation seeds. Validation requires
the resulting route, template, result, localization, presentation, and progression
authorities to be distinct persistent assets with recomputed local digests and no retained
dependency on the accepted Olympus route-owned sources.

## Product quarantine

Historical B1-1 checkpoint only: B1-2 superseded this quarantine state on 2026-07-22.
The claims below describe the preserved pre-admission boundary, not the current product
catalog or Build Settings.

B1-1 intentionally leaves the accepted product surface unchanged:

- the product catalog still contains only the accepted Olympus entry at generation `2`;
- no catalog row references the Courtyard route, playable-stage ID, or scene path;
- `OlympusCourtyardDrillStage.unity` is absent from every enabled or disabled Unity Build
  Settings row; and
- the accepted route-derived product manifest remains five physical scenes with digest
  `b0f1a128548f8f77aae5a0670586a2ac39c504d967ef722cf9681f56cd788d6b`.

B1-2 owns the later catalog-generation bump, exact second projection, Stage Select card,
and Build Settings admission. B1-1 does not reserve or invent that future catalog entry ID.

## ArkData structural evidence and copy boundary

ArkData informed only ownership boundaries. It did not supply gameplay values or a foreign
runtime schema.

- The reviewed PGR material under
  `\\DESKTOP-69817L3\ArkData\SubcultureGameData\games\punishing-gray-raven`
  keeps stage/guide material, wave/spawn candidates, stage-configuration material, and
  enemy/boss runtime families as separate review domains. This supports keeping route,
  scene/stage binding, Add payload execution, and boss terminal authority separate.
- The reviewed Aether Gazer
  `games/aether-gazer/enemies-stages/aether-gazer-stage-topology-wave-context` material
  exposes level rows with ordered `wave_list` identities while separate wave rows carry
  wave-facing metadata. This supports explicit ordering and separate row ownership; it
  does not prove a universal Wave runtime or exact spawn placement for DimensionBrawl.

No external code, IDs, field names, stage composition, enemy values, timings, positions,
text, UI, art, audio, or implementation details were copied. The one-row topology,
Courtyard identities, anchor positions, SceneReady activation, boss/Add relationship,
result mapping, and quarantine policy are local product decisions validated by the
project's own code and tests.

## Verification checkpoint

The current focused checkpoint records:

| Gate | Result |
|---|---|
| Isolated sidecar setup | `PASS` |
| Fresh-load sidecar validation | `PASS` |
| Final authored asset/digest/isolation/static-scene validator | `PASS` |
| Final stage + HUD matrix | `6/6 CLEAN` |
| Input/performance/hot-path targeted sequence | `9/9 CLEAN` |
| Direct-scene visual QA capture | `PASS — 1600x900` |
| Final B1-1 quarantine gate | `PASS — authoredReady=true, shippedReachable=false` |
| Full project PlayMode regression | `476/476 CLEAN` |
| Accepted product manifest | `b0f1a128548f8f77aae5a0670586a2ac39c504d967ef722cf9681f56cd788d6b` |

The stage matrix covers direct one-row admission, independent Rifle Crossfire damage,
boss Clear, player-down Fail, truthful single-segment result, and terminal cancellation
of the Add. The binder matrix covers the shared HUD's exact runtime binding surface.

### Visual QA

The direct-scene visual QA runner produced `CAPTURE_PASS` at 1600x900:
`C:\tmp\DimensionBrawl-B1-1-CourtyardDrill.png`
(`C:\tmp\DimensionBrawl-B1-1-VisualQaCapture-Final.log`).

The runner verified the exact Courtyard scene, one active camera, one live encounter, an
alive player and terminal boss, the shared combat HUD presenter/binder, a valid non-blank
PNG, and no Build Settings or scene-dependency mutation. Manual review confirmed a
readable compact courtyard prototype: player, terminal boss, Rifle Crossfire Add,
objective/timer card, virtual joystick, basic attack, dodge, and pause affordances remain
inside the 16:9 mobile frame. This is a prototype visual-QA pass, not a claim of final
lighting, animation, VFX, balance, localization, or production art polish.

### Input, callback-budget, and test-isolation cleanup

The first broad PlayMode run exposed an idle keyboard polling loop and stale reviewed
callback budgets from the already-integrated continuous-stage owners. `PlayerSkill1Action`
no longer declares `Update`; its temporary keyboard fallback is an event-driven disposable
`InputAction` subscribed and released with the component lifecycle. The shared Courtyard
HUD exposes only implemented inputs: virtual joystick, held basic attack, dodge, and
pause. Unavailable Skill, Ultimate, and Summon buttons remain disabled.

The canonical runtime callback budgets now admit only reviewed continuous owners:
Station `19`, Corridor pre-handoff `21`, and Corridor post-handoff `15`.
`OlympusCorridorCombatFlowController` advances its run clock only inside the finite intro
and active-phase observation routines rather than a permanent `Update`. The benchmark
fixture restores a neutral runtime scene and `Time.timeScale = 1` during teardown, and the
hot-path fixture starts and ends from that same neutral boundary. This prevents an early
scene assertion from contaminating later scheduler, collider-cache, or time-warp tests.

The combined benchmark, seven hot-path checks, and idle-presentation check passed `9/9`
(`C:\tmp\DimensionBrawl-B1-1-TargetedSequence-Retry.xml`). The complete PlayMode matrix
then passed `476/476` in a fresh process
(`C:\tmp\DimensionBrawl-B1-1-FullPlayModeRegression-Final.xml`, 468.07 seconds).

## Explicit deferrals

B1-1 also does not claim encounter balance, production-quality animation polish, mobile
performance acceptance, rewards, first-clear persistence, account/service integration,
live-operations content, multi-wave authoring, a generic objective system, or a second
published product route.

B1-2 remains the sole owner of product admission: catalog generation bump, exact second
catalog projection, Stage Select card, and Build Settings migration. Reward grants,
first-clear persistence, availability/prerequisite edges, profile/service integration,
multi-wave authoring, and live-operations behavior remain deferred beyond this authored
pack proof.

## Decision and next gate

Historical B1-1 checkpoint only: this decision was superseded by B1-2 product admission
on 2026-07-22.

B1-1 has a real isolated scene and content pack with direct visual evidence and a clean
full-project PlayMode regression while the accepted product remains unchanged. Keep the
Courtyard route quarantined until B1-2 explicitly migrates catalog projection, Stage
Select, and Build Settings together.
