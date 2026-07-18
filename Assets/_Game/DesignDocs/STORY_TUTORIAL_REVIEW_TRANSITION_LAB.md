# Story -> Tutorial Review Transition Lab (ST-01)

Status: implemented and verified review-local acceptance lab

Date: 2026-07-18

Scope label: `REVIEW SAMPLE / TEMP_DO_NOT_SHIP`

Canonical product route admitted: no

## Outcome

The isolated Olympus narrative review now exercises an explicit story-to-tutorial
boundary:

```text
VisualNovel
  -> generation-scoped terminal request
  -> story-owned work release
  -> exact review presentation-state restoration
  -> immutable story-transition receipt
  -> claim the existing sole review Director
  -> Evaluate + Play
  -> confirm only after PlayState.Playing
  -> TutorialCutscene
```

Normal narrative completion and explicit narrative skip follow the same boundary. Missing
bindings, a missing tutorial target, restore failure, owner disable, duplicate signals, and
stale-generation callbacks all fail closed and produce no confirmed tutorial-start probe.

This closes ST-01 only as a review-local lab. It does not attach a visual novel to the
canonical Olympus route and does not create tutorial, combat, route, result, save, or
progression authority.

## Relationship to the prerequisite terminal lab

`NARRATIVE_TUTORIAL_REVIEW_TERMINAL_LAB.md` hardened the existing second half of the review
flow:

```text
TutorialCutscene -> owned-work cleanup -> tutorial receipt -> StageBriefing
```

ST-01 adds the missing first half and requires both halves to agree. Review briefing is now
eligible only when all three current-generation facts are present:

1. the story-transition receipt authorizes review tutorial dispatch;
2. the existing tutorial terminal receipt authorizes review briefing;
3. the review tutorial-start probe confirms that the existing Director actually entered
   `PlayState.Playing` for the same generation.

A claim made before `PlayableDirector.Play()` is not confirmation. A sealed story receipt
without successful playback cannot open the review briefing.

## Review-only captured domains

`OlympusStoryTutorialTransitionReviewGate` directly binds two local cameras, two local
listeners, one `CanvasGroup`, one input stand-in, and one tutorial-start probe. It captures
the exact initial values before applying the visual-novel presentation state.

| Domain | Exact captured state | Story presentation override | Restore rule |
| --- | --- | --- | --- |
| camera | gameplay and narrative `enabled` values | gameplay off, narrative on | restore both captured booleans |
| HUD | root `activeSelf`, alpha, interactable, blocks-raycasts | inactive, alpha 0, no interaction or raycasts | restore all four values |
| input | bound review `Behaviour.enabled` | disabled | restore captured boolean |
| listener | gameplay and narrative `enabled` values | gameplay off, narrative on | restore both captured booleans |
| time | `Time.timeScale` | 0 | restore the captured float last |

Restoration is best effort across domains. Destruction of one binding records restore
failure but does not prevent the remaining domains or `Time.timeScale` from being restored.
The snapshot is then consumed so duplicate terminal requests cannot replay restoration.

These bindings are explicit review stand-ins. `ReviewGameplayInputProbe` is not the
authoritative input owner, the HUD is not a product HUD lease, the two cameras/listeners are
not route-owned presentation leases, and `Time.timeScale` has no product ownership token.

## Pure lifecycle and immutable receipt

`StoryTutorialReviewTransitionSession` owns no Unity objects. It provides a monotonic
generation gate with these phases:

```text
Idle -> StoryPresenting -> Terminating -> Terminated
```

One terminal request is accepted per live generation. Duplicate requests are idempotent,
older generations are stale, and sealing before terminal acceptance is invalid. The sealed
`StoryTutorialReviewReceipt` records:

- generation;
- terminal reason;
- whether controller-owned story work was released;
- whether exact presentation-state restoration succeeded;
- whether the existing tutorial target was available at release time.

`CanDispatchReviewTutorialStart` is true only for `Completed` or `Skipped` with all three
release/restore/target facts true. It authorizes only an attempt to start the existing
review Director.

| Story terminal reason | Dispatch eligible | Review behavior |
| --- | --- | --- |
| `Completed` | yes, if release, restore, and target checks succeed | attempt existing tutorial start once |
| `Skipped` | yes, under the same checks | attempt existing tutorial start once |
| `Cancelled` | no | restore if leased; stop at boundary |
| `OwnerDisabled` | no | fail-closed cleanup and restore |
| `BindingUnavailable` | no | reject before mutation or stop at boundary |
| `StateApplyFailed` | no | best-effort rollback; no tutorial dispatch |

The existing review terminal vocabulary also adds `StoryTransitionUnavailable` so a failed
story-to-tutorial boundary cannot be mislabeled as successful tutorial completion or as the
older tutorial-specific binding failure.

## Controller ordering contract

The existing `OlympusChapterNarrativeReviewController` applies this order:

1. issue the existing review generation;
2. validate every transition binding before story mutation;
3. capture and apply the review presentation state;
4. create the narrative session and capture its completion callback with the generation;
5. on complete or skip, accept the story terminal before releasing the narrative session,
   typewriter/auto routines, voice, and utility panels;
6. restore all captured domains and seal the story receipt;
7. resolve the one existing `StageCutscenePort`, payload root, and sole Director;
8. claim the current generation once;
9. begin the existing tutorial lifecycle, bind its stopped callback, call `Evaluate()`, and
   call `Play()`;
10. record the tutorial-start probe only if Director state is `Playing`;
11. allow later review briefing only after the tutorial itself seals its ready receipt.

If `Evaluate()`, `Play()`, or confirmation fails, the tutorial lifecycle seals
`BindingUnavailable`, playback is stopped where possible, and briefing remains blocked.
The probe count stays zero when the tutorial target is missing or playback never starts.

## Failure and teardown behavior

- Missing or invalid direct bindings reject visual-novel entry before any captured state is
  changed.
- Missing tutorial target after story release produces `StoryTransitionBlocked`; restored
  gameplay presentation remains visible and the start probe stays zero.
- A stale narrative completion callback cannot terminate or dispatch a newer generation.
- Disable during the visual novel terminates the story lease as `OwnerDisabled`, releases
  controller-owned story work, restores presentation state, and authorizes nothing.
- Gate disable runs before ordinary review teardown through its review-local execution
  order and independently seals fail closed if the controller cannot finish cleanup.
- Duplicate complete, skip, stop, disable, and stale callbacks cannot dispatch the tutorial
  or briefing twice.
- Tests prove that `StageRunRuntime.ActiveContext` and `LastAbortRecord` retain the same
  references across the review flow.

## Generated review fixture

- Scene: `Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity`
- Timeline: `Assets/_Game/DesignData/Timelines/Review/DB_Timeline_OlympusTutorialReview.playable`
- The scene contains exactly two directly named cameras and two matching listeners.
- The gameplay camera/listener begin enabled; the narrative camera/listener begin disabled.
- The scene contains one review gate, one independent input probe, and one tutorial-start
  probe, all outside the leased HUD hierarchy.
- The small top-center HUD diagnostic exposes the local gameplay-presentation state without
  overlapping the existing skip control.
- The review setup verifier uses an allowlist for DimensionBrawl runtime components and
  rejects any unexpected route/gameplay owner.
- The scene is rejected if it appears in Build Settings, whether enabled or disabled.
- Existing buttons retain zero persistent listeners; runtime listeners remain controller
  owned.

No canonical scene, canonical Timeline, stage catalog row, stage definition, or Project
Settings entry is bound by this fixture.

## ArkData structural reference boundary

The bounded structural reference reviewed for this slice was:

`\\DESKTOP-69817L3\ArkData\_ArkArchive\apply-packs\TutorialSystem_ApplyData_2026-06-24\normalized_enhanced\tutorial_runner_contract.json`

Reusable lessons were limited to ordered execution, condition/observer separation,
fail-closed treatment of required targets, locally scoped event mapping, and idempotent
completion.

The source remains `PRIVATE REFERENCE / REVIEW NEEDED`. ST-01 copies no external code,
assets, story text, identifiers, layouts, timings, balance values, media, voices, or
implementation details. Higgsfield, ElevenLabs, and other external generation services are
not runtime dependencies and were not used as acceptance substitutes.

## Verification evidence

- Unity 6000.3.5f2 setup/import and generated-scene validation: passed.
- Focused PlayMode tests: 52/52 passed.
  - pure narrative and story-transition lifecycle cases;
  - direct gate capture/restore/disable/destroy/stale/duplicate cases;
  - review-controller complete, skip, missing binding, missing target, failed start,
    actual-Play ordering, current-generation, and StageRun non-mutation cases.
- Final review setup verifier: passed with Unity exit 0; the scene remains outside Build
  Settings and satisfies the direct-binding/runtime-component allowlist.
- Visual QA: 15/15 captures passed and were inspected.
  - states: Chapter Entry, Visual Novel, Tutorial Cutscene, Stage Briefing, Complete;
  - resolutions: 1920x1080, 2400x1080, 2520x1080;
  - the first diagnostic-chip placement overlapped the skip control, was moved to top
    center, recaptured, and re-inspected.
- Independent final audit: blocker 0, major 0.
- Final broad review/canonical-route regression: 137/137 passed, failed 0, skipped 0,
  Unity exit 0.
- Evidence paths:
  - `C:/tmp/DimensionBrawl-ST01-Focused-1.xml`
  - `C:/tmp/DimensionBrawl-ST01-Focused-1.log`
  - `C:/tmp/DimensionBrawl-ST01-Verifier-Final.log`
  - `C:/tmp/DimensionBrawl-ST01-VisualQA-2.log`
  - `C:/tmp/DimensionBrawl-OlympusNarrativeReview-QA/capture-report.md`
  - `C:/tmp/DimensionBrawl-ST01-Regression-Final3.xml`
  - `C:/tmp/DimensionBrawl-ST01-Regression-Final3.log`

## Broader regression findings fixed separately

The first broad run passed 132/137 and exposed five failures outside ST-01. All five also
reproduced in clean individual Unity processes, so they were not hidden as order noise.

1. Three canonical Corridor tutorial cases timed out at `Melee -> Move`. A prior dynamic-Add
   hardening change correctly rejected disabled health from global runtime target/contact
   resolution, but the older tutorial intentionally kept its passive target
   `CombatHealth` components disabled while colliders stayed active. The local tutorial
   contract now keeps target health enabled while AI, sensors, and presentation behaviors
   are quiesced; terminal cleanup disables health again. The global Add invariant remains
   strict.
2. Replacing the Corridor scene could receive the old intro Director's `stopped` callback
   before `OnDisable`, falsely begin gameplay in an unloading scene, and prewarm a VFX pool
   into that scene. The flow callback now requires an active owner in a valid, loaded scene
   before treating stop as completion.

The exact three combat cases and the repeated-scene identity case passed after those narrow
fixes. They are canonical safety repairs discovered by this regression pass, not authority
granted to the ST-01 review fixture.

Ordered reruns also exposed two test-fixture assumptions. The FOV diagnostic now uses the
same real mobile-joystick movement helper as the canonical tutorial tests instead of a
direct input shortcut, and the Add sensor-lease fault assertion accepts either destroyed or
inactive owned roots after a yielded cleanup frame. Both exact cases passed, followed by the
final 137/137 ordered regression.

## Product-promotion gates and explicit non-claims

ST-01 must not move into a canonical product scene until at least:

- route-owned camera and listener leases exist with exact acquire/release receipts;
- authoritative HUD and input owners expose revocable leases rather than review probes;
- time pause uses an ownership token or stack so restoration cannot overwrite a concurrent
  product change;
- tutorial-start confirmation observes a product Director/router capability instead of
  trusting review-controller call ordering;
- the product route defines complete, skip, cancel, disable, unload, retry, stale, duplicate,
  fault, and restore policy across both story and tutorial owners;
- product continuation consumes a typed route receipt without manufacturing tutorial facts,
  combat admission, result, reward, save, or progression state;
- P1-C/P1-D/P1-E and P2-A/P2-B ownership and quiescence gates are admitted.

The gate's `ConfirmTutorialStarted()` method intentionally remains a review-local probe. It
does not accept Director evidence itself and must not be treated as product proof. Likewise,
the review `Time.timeScale` snapshot is safe only in this isolated allowlisted scene.

VN-02 remains next: a reusable multi-character narrative presenter with persistent
left/center/right portraits, expression state, typewriter, auto, choices, log, and skip,
still isolated until a separate product admission decision.
