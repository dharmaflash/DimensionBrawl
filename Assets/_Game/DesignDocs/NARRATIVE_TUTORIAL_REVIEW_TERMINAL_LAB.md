# Narrative + Tutorial Review Terminal Lifecycle Lab

Status: implemented and verified review sample
Date: 2026-07-18
Scope label: `REVIEW SAMPLE / TEMP_DO_NOT_SHIP`
Canonical product state changed: no

## Outcome

The existing Olympus narrative review scene now has a monotonic, generation-safe lifecycle
from its visual-novel sample to its tutorial Timeline and review briefing:

`VisualNovel -> TutorialCutscene -> terminal request -> owned-work cleanup -> review receipt -> StageBriefing`

The controller no longer treats disable, a missing cutscene binding, or an early external
`PlayableDirector.Stop()` as successful review completion. Only an explicit cutscene skip
or the controller-observed mandatory end frame can make the review briefing eligible.

This is prerequisite hardening discovered while scoping ST-01. It is a
`TutorialCutscene -> StageBriefing` terminal lab, not ST-01's story-completion -> tutorial-
start receipt or acceptance fixture. The follow-on review-local boundary is now implemented
in `STORY_TUTORIAL_REVIEW_TRANSITION_LAB.md`. Neither lab is the product P2-B adapter or
attaches a visual novel to the canonical Olympus route.

## Product boundary

- Existing scene: `Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity`
- The scene remains outside Build Settings.
- No canonical scene, route, stage catalog, result, reward, save, progression, or
  `StageRunRuntime` owner was changed.
- No new Timeline, cinematic router, tutorial framework, or combat-entry authority was
  introduced.
- `NarrativeTutorialReviewReceipt.CanEnterReviewBriefing` authorizes only the next panel in
  this isolated review flow. It is not tutorial proof, a gameplay lease receipt, or combat
  authority.

## Owned cleanup and explicit non-claims

The review controller can truthfully clean only work that it already owns:

- the current narrative session completion subscription;
- typewriter and auto-advance coroutines;
- the review voice source;
- the review `PlayableDirector.stopped` subscription and playback that it explicitly stops.

It does not acquire or restore gameplay camera pose, HUD state, gameplay input, audio
listeners, `Time.timeScale`, or route locks. Cleanup success in the review receipt therefore
means only that the owned list above was released. Product promotion must use the broader
captured-domain and quiescence contract in
`STAGE_PRESENTATION_HANDOFF_LIFECYCLE_SPEC.md`.

## Runtime contract

1. Beginning the visual novel issues a strictly increasing generation.
2. Narrative completion or narrative skip may move that live generation into the tutorial
   phase, but cannot make a terminal receipt or briefing eligibility by itself.
3. The tutorial requires the existing valid `StageCutscenePort`, its payload root, and its
   sole bound `PlayableDirector` before playback.
4. In the controller-owned end window, it applies the exact mandatory final director time
   with `Evaluate()`, accepts `Completed`, removes the callback, stops playback, cleans owned
   work, and seals one immutable receipt. A stopped callback that already reports the end
   tolerance may seal `Completed` without re-evaluating a graph that has already stopped;
   every earlier stopped callback seals `Cancelled`.
5. Explicit tutorial skip follows the same cleanup path but seals `Skipped` after applying
   the mandatory final director state.
6. A terminal request invalidates live generation work before cleanup. Duplicate requests
   and seals are idempotent, and callbacks captured from an older generation are rejected.
7. Stage briefing is reachable only after a valid ready receipt. Every non-ready receipt
   keeps briefing dispatch at zero; active tutorial cancellation or binding failure remains
   on the blocked tutorial surface, while owner disable leaves the component disabled.

| Terminal reason | Receipt ready | Review behavior |
| --- | --- | --- |
| `Completed` | yes, after tutorial entry and successful owned cleanup | enter review briefing once |
| `Skipped` | yes, after mandatory final state and successful owned cleanup | enter review briefing once |
| `Cancelled` | no | fail closed; no briefing |
| `OwnerDisabled` | no | stop/detach owned work; no briefing |
| `BindingUnavailable` | no | show blocked boundary; no playback or briefing |
| `SceneUnloading` | no | reserved kernel vocabulary for an explicit future host signal |

The current MonoBehaviour reports Unity disable and scene teardown through
`OwnerDisabled`; it does not claim a distinct scene-unload callback.

## ArkData structural reference boundary

The bounded reference reviewed for this slice was:

`\\DESKTOP-69817L3\ArkData\_ArkArchive\apply-packs\TutorialSystem_ApplyData_2026-06-24\normalized_enhanced\tutorial_runner_contract.json`

Reusable structural lessons were limited to ordered execution, explicit runtime kinds,
separation of conditions from observers, pause/report behavior for missing required
targets, locally scoped combat-event mapping, and idempotent completion.

The source remains `PRIVATE REFERENCE / REVIEW NEEDED`. No external code, assets, story
text, identifiers, layouts, timings, balance values, media, or implementation details were
copied. No external generation service is a runtime dependency or acceptance substitute.

## Verification evidence

- Unity 6000.3.5f2 script import and compile: passed.
- Focused PlayMode run: 29/29 passed.
  - 15 narrative/lifecycle-kernel cases.
  - 14 review-controller cases.
- Covered controller paths: explicit skip, mandatory end, early external stop, missing
  binding, disable during visual novel, disable during cutscene, duplicate signals, stale
  narrative session, stale prior-generation director callback, and paused-graph skip,
  disable, and reset cleanup. The controller suite also rejects premature tutorial entry,
  exercises GameTime and UnscaledGameTime at non-default time scales, preserves selected
  choice IDs after session release, observes the mandatory final playable state, and
  rejects active runtime reconfiguration without dropping the owned Director graph.
- Existing editor verifier:
  `DimensionBrawl.Editor.NarrativeReview.OlympusChapterNarrativeReviewSetup.RunBatchVerification`
  passed and confirmed that the scene remains review-only and outside Build Settings.
- Final broader review/canonical-route ordered regression: 62/65 passed. All three failures
  were existing `SpatialOneShotVfxPool` attempts to move an object into a scene already
  being unloaded. They are recorded as order-dependent teardown debt rather than hidden or
  attributed to this review lifecycle. Each exact failed test passed 1/1 in its own clean
  Unity process (isolated retries: 3/3).
- Verification logs:
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Final5.xml`
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Final5.log`
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Verifier-Final2.log`
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Regression-Final.xml`
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Regression-Final.log`
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Retry-ResultDeepCopy.xml`
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Retry-StationAddFixture.xml`
  - `C:/tmp/DimensionBrawl-NarrativeTutorialTerminal-Retry-StationFailureLobby.xml`

## Deferred product promotion gates

This lab must not be promoted into the canonical Station or Corridor flow until at least:

- the P1-C ordered encounter/quiescence owner is admitted;
- P1-D typed proof/progress and P1-E tutorial attempt/reset boundaries are closed;
- P2-A restart policy is admitted;
- authoritative gameplay input, HUD, camera, listener/BGM, and time owners are identified;
- the existing canonical `intro-to-stage` sole-Director fixture proves complete, skip,
  cancel, disable, unload, retry, restore, and quiescence parity;
- a route-owned typed continuation consumes a product receipt before starting tutorial or
  releasing combat.
- fault injection and recovery are added for exceptions thrown after terminal acceptance
  during mandatory final-state `Evaluate()` or graph `Stop()`; the current path fails closed
  but may remain `Terminating` until this recovery policy is implemented.

Separate hardening debt remains in the canonical Station notice/tutorial bridge: its
notice-owned coroutine and bridge release lifetime need an explicit disable/unload audit so
that a late coroutine cannot release a superseded bridge and a notice teardown cannot leave
the bridge lock stranded. That debt was discovered during ST-01 scoping but is not changed
or claimed fixed by this sample.

VN-02 remains the next review-only presentation slice: a reusable multi-character
presenter with persistent portrait state and inspectable typewriter, auto, choice, log, and
skip behavior.
