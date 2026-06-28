# Intro GatePod First-Person Lock Audit

Date: 2026-06-28

This note exists because the invasion-extension work touched, regenerated, or
validated through code paths that also own the capsule and first-person section.
That is not acceptable for the next pass.

## Immediate User Rule

Do not touch `0.000-12.883s` unless the user explicitly asks.

The locked section is:

| Time | Cue | Status |
| --- | --- | --- |
| `0.000-6.100s` | `src_c01_capsule_left_dolly` | locked |
| `6.100-8.133s` | `src_c03_first_person_eye_open` | locked |
| `8.133-9.683s` | `src_c04_first_person_scan_left` | locked |
| `9.683-11.133s` | `src_c05_first_person_scan_right` | locked |
| `11.133-12.883s` | `src_c06_first_person_look_down_hands` | locked |

Allowed invasion work starts at `ark_c07_heaven_air_raid_wide`, currently
`12.883333s`.

## Current Workspace State At Audit

- `git status --short --branch` showed only one unrelated modified file:
  `_Game/Art/Materials/ActionFoundation/AF_SummonPressureScreen.mat`.
- The aborted first-person repair attempt was reverted.
- Cutscene authoring files were clean after that revert:
  - `_Game/Editor/IntroGatePodCutsceneReviewSetup.cs`
  - `_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening.asset`
  - `_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening.playable`
  - `_Game/Scenes/IntroGatePodCutsceneReview.unity`

## Relevant Commit Range

Cutscene review generator and scene history:

| Commit | Meaning For This Audit |
| --- | --- |
| `7f3d07bc` | Initial intro GatePod cutscene review save. First generator-owned first-person paths already exist here. |
| `e86364473` | Polished invasion bridge. |
| `2199affbd` | Split invasion run/explosion beats. |
| `c018c5dca` | Polished Olympus backdrop. |
| `a2fa3f179` | Added ArkData-driven invasion extension and changed/extended post-`12.883s` authoring. |
| `eadc1d23` | Refreshed Olympus runtime bridge from review scene. |

Important correction: the issue is not only that the latest pass touched a bad
number. The deeper issue is that `EnsureReviewScene()` regenerates the whole
review scene and profile, so any invasion pass that runs it can re-author the
locked first-person section.

## Current Contamination Routes

These paths exist in `_Game/Editor/IntroGatePodCutsceneReviewSetup.cs` and must
not be used casually for invasion-only work:

| Code Path | Why It Is Dangerous |
| --- | --- |
| `EnsureReviewScene()` | Recreates scene, profile, timeline, actors, cameras, and invasion bridge together. This is too broad for invasion-only edits. |
| `ConfigureProfile()` | Writes all camera cues, including `src_c01` through `src_c06`, not only `ark_c07+`. |
| `CreateFirstPersonViewMarker()` | Recreates `IntroGatePodReview_FirstPersonViewMarker` from hard-coded transform values. |
| `PlaceInoriForFirstPersonCapsule()` | Repositions Inori for the first-person capsule section. |
| `AlignInoriHandsToFirstPersonView()` | Moves Inori from hand/camera math. This is exactly the kind of coordinate repair that should not be attempted again without explicit request. |
| `CreateTimelineDirector()` | Rebuilds timeline bindings and clips, including first-person-era body/audio/fade clips. |
| `CreateFirstPersonRendererMask()` | Rebinds renderer hiding for the first-person section. Safe only when rebuilding the first-person cutscene deliberately. |

## Evidence Document Mismatch

`INTRO_GATEPOD_INVASION_ARKDATA_EVIDENCE.md` still contains a stale statement:

- It says current authored duration is `20.574354s`.
- Current profile now reports `authoredDurationSeconds: 34.250336`.

This proves the evidence note cannot be treated as a complete current-state
source. It is useful for ArkData grammar, but not sufficient as a lock audit.

## Required Next Architecture

The next invasion pass must be split from first-person generation.

| Layer | Allowed To Edit? | Notes |
| --- | --- | --- |
| `src_c01`-`src_c06` profile cues | no | Keep exact current serialized values unless user asks. |
| Timeline clips before `12.883s` | no | Voice, BGM, fade, first-person body, and camera clips remain locked. |
| Inori first-person placement / marker / renderer mask | no | Do not "fix" by camera math. |
| `ark_c07+` profile cues | yes | Air raid, portal, soldier surge, protagonist interrupt, sword ready, Rina back-view only. |
| Invasion bridge runtime data | yes | Additive objects, timed effects, soldiers, impact cue arrays are allowed. |
| Olympus intro visual root | targeted only | Do not replace user-adjusted `IntroGatePodPortPayload_Visuals`. |

## Implementation Rule For Next Pass

Do not run `EnsureReviewScene()` for invasion-only work.

Use a targeted method instead:

1. Open existing review scene.
2. Snapshot/fingerprint locked section values.
3. Modify only `ark_c07+` cues, invasion bridge objects, and post-`12.883s`
   timeline clips.
4. Re-read the locked section.
5. Fail if any `src_c01`-`src_c06` cue, first-person marker, or pre-`12.883s`
   timeline clip changed.

## Minimum Lock Verification

Before and after any future invasion edit, compare:

| Asset | Must Match |
| --- | --- |
| `DB_Cinematic_IntroGatePodAwakening.asset` | `src_c01` through `src_c06` cue ids, start, duration, camera position, look-at, FOV, blend kind |
| `DB_Timeline_IntroGatePodAwakening.playable` | camera clips before `12.883s`, voice clips, BGM start, fade clips before `12.883s`, `wake_confused_hands` |
| `IntroGatePodCutsceneReview.unity` | `IntroGatePodReview_FirstPersonViewMarker`, first-person Cinemachine cameras/look-at objects, Inori first-person placement root |
| `OlympusCorridorInvasionStage.unity` | same copied first-person payload values if Olympus is refreshed |

## What To Do If The Locked Section Is Bad

Do not repair it as part of invasion work.

If the first-person section is visibly wrong, make a separate task and choose
one of these explicit strategies:

1. Revert the first-person section to a known good committed/manual state.
2. Re-author the first-person section from source references with its own review
   pass and screenshots.
3. Remove first-person body/hand visibility deliberately and replace it with a
   simpler locked POV shot.

That decision belongs to the first-person cutscene task, not the invasion task.

## Next Allowed Work

The next real work item is not more coordinate repair. It is:

1. Add lock snapshot/validation code.
2. Build a targeted invasion-only updater.
3. Rework `ark_c07+` composition using the existing ArkData grammar evidence.
4. Validate and capture only post-`12.883s` shots.

