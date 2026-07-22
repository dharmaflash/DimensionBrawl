# B1-3 Courtyard Product Route

Status: `IMPLEMENTED / EXACT PRODUCT PATH + FULL 483-TEST REGRESSION VERIFIED`

Verification checkpoint: 2026-07-22 KST

## Purpose

B1-3 proves that the B1-2 Courtyard admission is a complete playable product loop rather
than a catalog-only destination. The proof begins in the authored Stage Select scene,
uses the real `01-2` card and Start button, admits the exact selected one-row route in the
Courtyard scene, publishes the shared additive result surface, and dispatches the real
Replay, Retry, and Lobby buttons to their sealed destinations.

No Courtyard-only route table, result scene, or parallel runtime owner was added. The path
continues to use the existing product seams:

1. `StageSelectScreenPresenter` selects the exact catalog projection and requests Start.
2. `UISceneFlowRouter` and `UISceneRouteLoader` perform the single-scene load.
3. `OneRowStageRunBootstrap` admits the serialized `PlayableStageDefinition`.
4. `OneRowStageRunResultPresenter` publishes the committed terminal result.
5. `OlympusStageClearOverlay` loads `UI_StageClear` additively.
6. `StageClearScreenPresenter` dispatches Replay, Retry, or Lobby through
   `StageRunRuntime.TryDispatchTerminalAction`.

## Exact Stage Select to runtime identity

The product-path fixture captures the selected `UIStageRouteProjection` before Start and
then requires the destination bootstrap and admitted context to retain the same authored
identity. It verifies:

- catalog entry `story_v1_courtyard_drill_route`;
- playable-stage ID `OLYMPUS-COURTYARD-DRILL-01`;
- the exact `PlayableStageDefinition` asset reference;
- route revision and 64-character canonical route digest;
- entry segment ID and sequence index;
- physical destination `Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity`;
- exactly one live `OneRowStageRunBootstrap` and one coordinated encounter;
- no fabricated reward or unlock contract.

This closes the false-positive case where Stage Select and the destination scene could
independently reference different route assets that happened to share a playable-stage
ID.

## Terminal action and fresh-run proof

The shared result surface remains additive while the Courtyard scene retains active-scene
ownership. Both Clear and Fail publish one committed segment result and exactly two
offered terminal actions.

Each real result button is verified beyond destination reachability. The retired context
must seal the exact selected action ID, action kind, outcome, target, destination scene,
selection ID, route/result digests, and canonical selection digest:

| Outcome | Button contract | Exact action | Destination |
|---|---|---|---|
| Clear | Replay | `olympus-courtyard-drill.replay` / `Replay` | fresh Courtyard run |
| Fail | Retry | `olympus-courtyard-drill.retry` / `Retry` | fresh Courtyard run |
| Clear | Lobby | `olympus-courtyard-drill.to-lobby` / `UIRoute` | canonical Lobby |

Replay and Retry must unload the retired Courtyard and additive result scenes, create a
different scene handle, context instance, and Run ID, restore a live non-terminal
encounter, and leave exactly one loaded scene. Lobby must dispose the retired context,
clear `StageRunRuntime.ActiveContext`, unload both prior scenes, expose one active enabled
`LobbyScreenPresenter`, and leave its primary CTA active and interactable.

## `timeScale` ownership correction

The result overlay records the combat speed and owns a temporary `Time.timeScale = 0`
lock while the result surface is open. A terminal scene loader restores destination speed
to `1` before the old combat scene is disabled. Previously, the old overlay could then
unconditionally write its saved pre-result value back during `OnDisable`, leaking combat
slow motion into Replay, Retry, or Lobby.

`OlympusStageClearOverlay.RestoreCombatTimeScale` now restores its saved value only while
the zero lock it owns is still present. If the terminal loader has already superseded the
value, the overlay releases ownership without another write.

The focused ownership tests cover both sides:

- a non-terminal disable restores the overlay-owned pre-result value;
- a terminal loader that supersedes the lock with `1` is not overwritten.

The product-path tests additionally set combat speed to `0.37` immediately before the
terminal hit, require `0` on the additive result surface, and require `1` after every
Replay, Retry, and Lobby destination load.

## Verification checkpoint

| Gate | Current B1-3 result | Evidence |
|---|---|---|
| `OlympusCourtyardProductRoutePlayModeTests` | `4/4 CLEAN` | `C:\tmp\DimensionBrawl-B1-3-ProductRoute-Final.xml` (`duration=28.5928141s`) |
| `PresentationIdleLoopOptimizationTests` timeScale focus | `3/3 CLEAN` | `C:\tmp\DimensionBrawl-B1-3-TimeScaleOwnership.xml` |
| `CanonicalUiRoutePlayModeTests` | `38/38 CLEAN` | `C:\tmp\DimensionBrawl-B1-3-CanonicalUiRoute.xml` (`duration=111.0502497s`) |
| Full project PlayMode regression | `483/483 CLEAN` | `C:\tmp\DimensionBrawl-B1-3-FullPlayMode-Final.xml` (`failed=0`, `skipped=0`, `duration=494.1557656s`) |
| `DimensionBrawl.PlayModeTests.csproj` | `BUILD CLEAN` | `0` warnings, `0` errors |

The product-scene fixture requires the normal hidden GPU-backed Unity batch used for the
final evidence. An explicit `-nographics` run reaches the same product assertions but the
Lobby's authored RenderTexture camera produces Unity/URP's NullGfx
`RenderTexture.Create failed` engine error. That engine limitation is not masked with
`LogAssert`; NullGfx is not claimed as a supported execution environment for this visual
product-scene fixture.

## Explicit product boundary

B1-3 proves navigation, run identity, result presentation, terminal action selection,
fresh-run creation, Lobby cleanup, and time-scale ownership. It does not add reward
grants, first-clear rewards, account persistence, unlock progression, prerequisite edges,
availability scheduling, live-operations services, or a new content row. Reward and
unlock references remain explicitly absent under the admitted schema.

## ArkData structural boundary

The earlier ArkData review informed only the separation between product projection,
runtime route identity, result presentation, and terminal navigation ownership. No
third-party code, IDs, field names, copy, art, audio, layout, encounter values, or
implementation details were copied. B1-3's route assertions, ownership guard, IDs, and
test evidence are local DimensionBrawl decisions.
