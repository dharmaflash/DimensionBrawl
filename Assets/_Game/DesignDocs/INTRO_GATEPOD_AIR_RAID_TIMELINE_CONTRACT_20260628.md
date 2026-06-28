# Intro GatePod Air Raid Timeline Contract

Date: 2026-06-28

This is the hard contract for the next Intro GatePod air-raid pass. It exists
because prior notes were not enough: a preview object was created and treated as
progress even though the user had no real Timeline surface to scrub or adjust.

## Non-Negotiable Definition Of Done

Do not call the air-raid pass complete unless all of these are true:

- A real Unity Timeline asset exists under
  `Assets/_Game/DesignData/Timelines/Cinematics/`.
- A real `PlayableDirector` in a Unity scene references that Timeline asset.
- The shot switching is represented by actual camera shot objects or timeline
  bindings, not by empty timing-note objects.
- The user can open the scene in Unity, scrub the Timeline, and adjust shot
  timings or camera objects.
- The protected first-person/capsule section remains unchanged.
- The final report names the exact scene object root, Timeline asset path, and
  PlayableDirector object.

If any of these are missing, report the work as incomplete.

## Protected Area Lock

Do not edit, regenerate, key, or rebind these areas unless the user explicitly
asks for that exact protected area:

- Existing `0.000-12.883s` first-person capsule/hand/scan sequence.
- `IntroGatePodReview_FirstPersonViewMarker`.
- `IntroGatePodReview_InoriPlacement`.
- Existing voice, capsule, eye-open, hand-look, and scan timing.
- Existing manual `IntroGatePodPortPayload_Visuals` placement.
- Existing `3.mp3` timing. It is an intentional next-scene entry cue.

## Forbidden Commands And Paths

Do not run these during the air-raid pass:

- `EnsureReviewScene()`
- `RunBatchSetupCaptureAndValidation`
- `RunBatchReviewSceneGeneration`
- `RunBatchValidation`
- `ApplyIntroGatePodPayloadToOlympusStage`

These paths previously caused or risked overwriting manual scene work. If a
validation path calls `EnsureReviewScene()` internally, stop and do not use it.

## Allowed Work Shape

The air raid must start as a separate additive review/runtime layer.

Preferred names:

- Scene/root: `IntroGatePodAirRaidPreludeRuntime` or
  `IntroGatePodAirRaidPreludeReview`.
- Timeline asset:
  `Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroAirRaidPrelude.playable`.
- Report:
  `C:/tmp/DimensionBrawl-IntroAirRaidPrelude-Verification.md`.

The air raid should not be spliced into the existing protected awakening
Timeline until the user has reviewed the separate version.

## Required Preflight Before Any Scene/Profile/Timeline Edit

Before editing Unity assets, run or manually confirm:

1. `git status --short`
2. Dirty unrelated files are listed and excluded from the work.
3. Exact target files and scene roots are named in a user-visible update.
4. Current scene/timeline files are backed up to `C:/tmp` if they will be saved.
5. ArkData evidence and project asset candidates have been checked.
6. The edit is outside the protected first-person/capsule section.

If any item is uncertain, stop and research instead of editing.

## Required Evidence Before Implementation

The implementation must cite or update evidence for:

- ArkData command/timeline/camera rows used as rhythm or shot grammar.
- 2017 Unity Timeline/camera data used as Unity implementation reference.
- Project aircraft/bomber/bomb/explosion/cloud asset candidates.
- Whether each source is `direct`, `grammar-only`, or `fallback`.

No generic second-by-second layout is allowed without marking it as fallback.

## Air-Raid Beat Target

Initial target beat sequence:

| Beat | Job | Expected Surface |
| --- | --- | --- |
| 01 | Over-cloud three-aircraft entry, rear/upper chase view. | Timeline camera shot. |
| 02 | Aircraft formation crosses frame with engine trails. | Timeline camera shot plus bound aircraft objects. |
| 03 | Bomber or ordnance drop if a real asset exists. | Timeline actor/VFX binding. |
| 04 | Cut/reframe to below-cloud impact zone. | Timeline camera shot. |
| 05 | Explosion, screen flash, camera recoil, smoke/fire lifecycle. | Timeline/VFX bindings. |
| 06 | Short black or warning transition into the existing invasion context. | Timeline fade/screen-effect binding. |

This target can change after asset and ArkData inspection, but changes must be
documented before implementation.

## Stop Conditions

Stop immediately and report instead of continuing if:

- The work requires touching the protected first-person/capsule section.
- A generator would save over `IntroGatePodCutsceneReview.unity`.
- The result would only be a preview object, note object, screenshot, or static
  hierarchy without a real Timeline.
- Required aircraft/bomb/explosion assets cannot be found and the fallback would
  become the final presentation.
- `git status --short` shows unexpected changes in protected files.

## Commit Rules

Only commit at these checkpoints:

1. Evidence/contract/beat-plan only.
2. Runtime or editor helper only, after compile validation.
3. Scene/Timeline implementation, after Unity validation.
4. Visual polish/capture, after the user can inspect the Timeline surface.

After any commit, verify the commit contains the intended cutscene files with:

`git show --name-status --oneline HEAD`

Do not equate "a commit exists" with "the cutscene work was committed."
