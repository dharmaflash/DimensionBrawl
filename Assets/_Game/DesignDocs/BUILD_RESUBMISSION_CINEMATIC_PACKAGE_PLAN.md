# Build Resubmission Cinematic Package Plan

Last updated: 2026-06-25 KST

## Purpose

This document fixes the current cutscene direction for the next build resubmission.

The goal is not a middle-presentation-only intro cutscene, and it is not only an early-game first-impression pass. The goal is a reusable cutscene, tutorial, and combat-presentation foundation that can support all authored cutscenes needed by the build.

The first awakening cutscene remains important because it is a high-risk first impression and a good vertical integration test, but it is only one instance of the system. The build also needs readable story beats, QTE moments, ultimate cut-ins, boss/elite entrances, danger/tutorial prompts, phase transitions, result bridges, and clean returns to gameplay.

## Execution North Star

Do not lose this sentence:

> Build a reusable cinematic package for every cutscene, and make Inori move through real animation, facial expression, weapon, VFX, and camera presentation.

Coding rules and ownership documents are guardrails. They must prevent brittle systems, hidden ownership, dirty state, and unsafe asset references, but they must not become an excuse for low-quality output. If a narrow implementation would produce a visibly weak cutscene, use a better scoped data/profile/editor workflow rather than falling back to capsules, static actors, tiny labels, or disconnected weapons.

## Nine-Step Execution Tracker

Current execution order:

1. Direction lock: ArkData, Unity-chan Timeline reference, quality guardrails, and project ownership rules are fixed as the baseline.
2. Asset intake: external bundles stay under `_Imported/AssetStore`; build-facing pieces are promoted into `_Game` only after review.
3. Inori foundation: `Inori_MagicaCloth2_Costume1` is the primary actor; verify Avatar, cloth, face clips, sockets, Animator, and material state.
4. Animation library: inventory existing and newly imported clips, build the gap list, then collect/retarget only missing clips.
5. Cutscene system foundation: create reusable sequence, camera, actor, VFX, tutorial cue, and gameplay handoff profiles/runners.
6. P0 modules: implement `IntroAwakening`, `GameplayHandoff`, `QTEAssist`, `UltimateCutIn`, `DangerCue`, and `CombatTutorialOverlay`.
7. P1 expansion: implement `BossIntro`, `PhaseTransition`, `BreakMoment`, `DialogueReactionBeat`, `ResultBridge`, and `SummonEntry`.
8. Build integration: attach modules to playable flow with input, UI, camera priority, time scale, VFX, and cleanup guarantees.
9. Quality pass: reject primitive actors, weapon-only motion, tiny unreadable text, dirty state, and unintended body-only camera language. Preserve intentional corridor back-view projectile language for combat.

Status:

- Step 1: Complete. Direction, source hierarchy, quality guardrails, and the execution north star are fixed in this document.
- Step 2: Complete for intake. `KAWAII_ANIMATIONS_100` and related imported folders were moved under `_Imported/AssetStore`; clip promotion into `_Game` is still pending per-clip review.
- Step 3: Complete. Inori foundation passed Unity batch verification with humanoid Avatar, blend shapes, face clips, cloth objects/colliders, hands, body controller, face controller, and P0 candidate animation FBX checks.
- Step 4: Complete for the initial P0 gap list. Actual retargeted clip promotion and visual inspection on Inori remain per-module work.
- Step 5: Complete for the reusable foundation. Runtime sequence profiles, actor/VFX/tutorial/handoff cues, promoted animation controller binding, direct camera shot-pose data, and the first review runner exist.
- Step 6: In progress. P0 profile assets exist, every enabled P0 camera cue now has authored direct shot-pose data, module preview captures exist, weapon visibility is profile-driven, the review scene has a six-module P0 playlist route with sampled Play Mode visual QA, QTE/tutorial prompts now render as camera-captured readable overlays, and the review scene has a reusable dressed stage/lighting shell. A continuous Play Mode timeline frame capture now generates labeled route strips and 23 timeline frames including the final gameplay handoff. Remaining P0 work is animation safety per module, production art polish, and production-style movie capture.
- Step 7: In progress. First-pass P1 profile assets now exist for `BossIntro`, `PhaseTransition`, `BreakMoment`, `DialogueReactionBeat`, `ResultBridge`, `SummonEntry`, `SummonFollowupHit`, `SummonEmpower`, `SummonRecall`, and `BossSummonPressure`; each has authored direct camera shot poses, Inori body/face cues, profile-driven rifle visibility, VFX where relevant, and explicit gameplay/result handoff data. Remaining P1 work is broader non-summon variants, visual tuning on Inori, and integration into actual game triggers.
- Step 8: In progress. Action cue integration now has a bridge from existing action cinematic cue requests to reusable build-resubmission cinematic sequence profiles. Boss barrage review generation binds the bridge, runner, Inori animator controller override, expression player, VFX player, support-dragon actor binding, and camera handoff path for ultimate, summon entry, summon follow-up, summon empower, summon recall, boss-summon pressure, break, and result cues. Play Mode input-route verification now proves tier-3 `Skill1` and `SummonSlot1` can trigger the mapped reusable cinematic sequences from the actual player action methods, dispatch bound actor cues, and produce multi-beat camera captures for summon entry plus direct bridge captures for boss-summon pressure, summon empower, summon recall, pocket-clear result, and pocket-fail danger routes.
- Step 9: In progress. Boss-barrage action bridge route verification now writes a labeled route contact sheet/report and fails summon-scoped route frames if the support dragon is expected but not visible in the active camera frustum. This is still evidence capture and guardrail work, not final production movie polish.

Current blockers:

- No active compiler blocker after the current cinematic package pass.
- Keep unrelated untracked Battle/UI/tutorial prototype code out of the cinematic package until it is intentionally integrated.
- The non-Kawaii imported support/sample folders that arrived with the bundle were quarantined under `_Imported/AssetStore` with `~` suffixes so their sample scripts do not pollute compilation.
- Do not promote or depend on quarantined support folders unless they are intentionally reviewed later.

## Implementation Progress

### 2026-06-24 Step 3 Verification

Unity batch verifier:

- method: `DimensionBrawl.Editor.CinematicInoriFoundationVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicInoriFoundationVerifier.log`
- report: `C:\tmp\DimensionBrawl-CinematicInoriFoundationVerifier.md`
- result: PASS, failures 0, warnings 1

The remaining warning is expected: the source Inori prefab has no Animator Controller assigned. Build-facing setup must assign the gameplay/cutscene controller explicitly.

Confirmed:

- promoted Inori model is Humanoid
- Avatar is valid/human
- promoted model imports blend shapes
- source prefab has 14 skinned renderers
- source prefab has 170 blend shapes across 3 blend-shape renderers
- source prefab has 5 Magica Cloth objects and 11 Magica collider objects
- `hand.r` and `hand.l` exist
- body controller has required rifle states and 14 bound motions
- face controller and expression clips pass
- P0 Kawaii candidate FBX clips import as Humanoid and expose animation clips

### 2026-06-24 Step 5 First Pass

Runtime/editor code added:

- `Assets/_Game/Scripts/Presentation/CinematicSequenceProfile.cs`
- `Assets/_Game/Scripts/Presentation/CinematicSequenceRunner.cs`
- `Assets/_Game/Editor/BuildResubmissionCinematicProfileSetup.cs`

Generated P0 profile assets:

- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroAwakening.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_GameplayHandoff.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_QTEAssist.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_UltimateCutIn.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_DangerCue.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_CombatTutorialOverlay.asset`

Unity generation batch:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicProfileSetup.RunBatchProfileGeneration`
- log: `C:\tmp\DimensionBrawl-BuildResubmissionCinematicProfiles.log`
- result: PASS, exit code 0

This is still a first-pass package foundation. The next quality gate is to bind these profiles into an inspectable Unity scene/prefab with Inori body Animator, face Animator, camera controller, VFX cue player, and gameplay handoff references, then replace placeholder state names with promoted/retargeted animation clips where the current rifle states are not expressive enough.

### 2026-06-24 First Inspectable P0 Scene

Generated scene:

- `Assets/_Game/Scenes/ActionFoundationCinematicP0Review.unity`

Scene contents:

- `CinematicP0Review_Inori`
- `CinematicP0Review_InoriRifle`
- `CinematicP0Review_UltimateCutInRunner`
- `CinematicBlendShapeExpressionPlayer`
- `CinematicSequenceRunner`
- `CinematicSequenceAutoPlay`
- `ActionCameraController`
- `CombatVfxCuePlayer`
- `CinematicP0Review_EnemyTarget`

The scene currently binds `DB_Cinematic_UltimateCutIn.asset` as the first inspectable P0 sample. It is intentionally short because it tests the reusable high-impact path first: Inori body state, face expression, attached weapon, camera cue, VFX cue, and gameplay handoff timing.

Unity generation batch:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchReviewSceneGeneration`
- log: `C:\tmp\DimensionBrawl-CinematicP0ReviewScene.log`
- result: PASS, exit code 0

Known quality caveat:

- The current first scene now uses promoted Kawaii humanoid clips through `DB_Inori_CinematicP0.controller` instead of existing rifle-only controller states.
- `CIN_UltimateCharge` and `CIN_SwordCharge` were promoted but should not be used in P0 profiles yet. Preview capture showed they produce unsafe/distorted or prone framing on Inori in the current review setup.
- `DB_Cinematic_UltimateCutIn.asset` currently uses `CIN_QTEMagicShot`, `CIN_UltimateRelease`, and `CIN_CombatReady` for a grounded first-pass cut-in.
- The next Step 6 quality pass should improve camera shot framing and choose stronger inspected clips per module, but the package is no longer a capsule/static-weapon/rifle-state-only placeholder.

### 2026-06-24 P0 Animation Promotion Pass

Promoted animation root:

- `Assets/_Game/Art/Animations/Cinematics/Inori/KawaiiP0`

Generated Inori cinematic controller:

- `Assets/_Game/Art/Animations/Cinematics/Inori/DB_Inori_CinematicP0.controller`

Promoted/available states:

- `CIN_IntroLookAtHands`
- `CIN_IntroSurprised`
- `CIN_IntroStumble`
- `CIN_IntroPickUp`
- `CIN_QTEEntryDash`
- `CIN_QTEMagicShot`
- `CIN_UltimateCharge`
- `CIN_UltimateRelease`
- `CIN_UltimateImpact`
- `CIN_UltimateRecover`
- `CIN_CombatReady`
- `CIN_SwordCharge`

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.log`
- result: PASS, failures 0, warnings 0

Preview captures:

- representative safe preview: `C:\tmp\DimensionBrawl-CinematicP0Review-UltimateRelease.png`
- state preview set: `C:\tmp\DimensionBrawl-CinematicState-CIN_*.png`

Visual review note:

- `CIN_UltimateCharge` produced a broken airborne/upside-down read in the current review camera and was removed from the active `UltimateCutIn` profile.
- `CIN_SwordCharge` produced an unsafe prone read in the current review camera and is reserved for later re-evaluation, not active P0 playback.
- `CIN_QTEMagicShot`, `CIN_UltimateRelease`, and `CIN_CombatReady` are currently the safest inspected P0 body states for the first review sample.

### 2026-06-24 Direct Shot Camera Pass

Runtime/editor code updated:

- `Assets/_Game/Scripts/Presentation/CinematicSequenceProfile.cs`
- `Assets/_Game/Scripts/Presentation/CinematicSequenceRunner.cs`
- `Assets/_Game/Editor/BuildResubmissionCinematicProfileSetup.cs`
- `Assets/_Game/Editor/BuildResubmissionCinematicReviewSceneSetup.cs`
- `Assets/_Game/Editor/BuildResubmissionCinematicPackageVerifier.cs`

What changed:

- `CinematicSequenceProfile.CameraCue` now supports authored direct shot pose data: camera local position, look-at local position, and FOV.
- `CinematicSequenceRunner` can drive the actual cinematic camera transform/FOV from those shot poses, disable `ActionCameraController` during direct cutscene camera playback, and restore the original gameplay camera afterward.
- `CinematicSequenceRunner` now prewarms opening direct shot poses, so a 0-second cutscene camera cue does not start by blending from the default back-view gameplay camera.
- `DB_Cinematic_UltimateCutIn.asset` now has four direct shot poses: focus close, charge arc, release hit, and gameplay handoff.
- `ActionFoundationCinematicP0Review.unity` now assigns the Main Camera directly to the runner and enables profile-driven camera transforms.
- The representative preview capture now uses the `ultimate_release_hit` shot pose instead of the default gameplay back-view.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-CameraShot-Front.log`
- result: PASS, failures 0, warnings 0

Preview:

- opening face proof: `C:\tmp\DimensionBrawl-CinematicP0Review-OpeningFace.png`
- `C:\tmp\DimensionBrawl-CinematicP0Review-UltimateRelease.png`

Visual review note:

- The first direct shot attempt was still over-shoulder/back-view heavy, so it was rejected and retuned.
- The current `ultimate_release_hit` preview reads as a front/three-quarter Inori cut-in with visible face, expression, and weapon.
- The opening camera was then fixed again after review: the first direct shot no longer begins by blending from a behind-the-head gameplay camera.
- Correction after live review: the earlier proof image was not sufficient because it used a manual preview camera pose. The saved review scene camera is now validated against the active review route's opening shot instead of relying on a standalone PNG.
- This is a meaningful improvement over the earlier action-camera view, but it is not the final lighting/background/post-process pass.

Next Step 6 order:

1. Apply direct camera shot-pose review to `QTEAssist`, `DangerCue`, `IntroAwakening`, `GameplayHandoff`, and `CombatTutorialOverlay`.
2. Promote or reject additional animation clips per module based on actual Inori previews, not asset names.
3. Add readability checks for tutorial/QTE prompts so tiny labels do not regress.
4. Build the first broader P0 review route that plays multiple modules in sequence instead of only `UltimateCutIn`. Complete for the first playlist route; runtime visual QA remains.

### 2026-06-24 P0 Direct Shot Coverage Pass

Updated P0 profiles:

- `DB_Cinematic_IntroAwakening.asset`: 8/8 enabled camera cues drive direct shot poses.
- `DB_Cinematic_GameplayHandoff.asset`: 2/2 enabled camera cues drive direct shot poses.
- `DB_Cinematic_QTEAssist.asset`: 4/4 enabled camera cues drive direct shot poses.
- `DB_Cinematic_UltimateCutIn.asset`: 4/4 enabled camera cues drive direct shot poses.
- `DB_Cinematic_DangerCue.asset`: 2/2 enabled camera cues drive direct shot poses.
- `DB_Cinematic_CombatTutorialOverlay.asset`: 3/3 enabled camera cues drive direct shot poses.

Verifier hardening:

- `BuildResubmissionCinematicPackageVerifier` now fails if any enabled P0 camera cue does not drive a direct shot pose.
- The verifier still checks authored FOV and separated camera/look-at positions for every direct shot.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-InitialCameraPose.log`
- result: PASS, failures 0, warnings 0

Module preview captures:

- `C:\tmp\DimensionBrawl-CinematicP0Review-OpeningFace.png`
- `C:\tmp\DimensionBrawl-CinematicP0Review-UltimateRelease.png`
- `C:\tmp\DimensionBrawl-CinematicP0Review-IntroAwakening.png`
- `C:\tmp\DimensionBrawl-CinematicP0Review-QTEAssist.png`
- `C:\tmp\DimensionBrawl-CinematicP0Review-DangerCue.png`
- `C:\tmp\DimensionBrawl-CinematicP0Review-TutorialOverlay.png`

Quality notes:

- The first QTE preview cropped Inori's face; the `assist_hit_confirm` shot was retuned and the corrected preview keeps face and weapon readable.
- The first Danger preview used a mid-dash state and rendered empty because the actor moved out of frame; P0 Danger now uses `CIN_CombatReady` with expression changes for a stable warning/brace read.
- The Intro preview still shows the rifle because the current single review actor always attaches it. This is acceptable as a system proof, but not final intro direction; next P0 pass must add profile-driven weapon visibility/attach timing.
- Tutorial preview intentionally remains closer to gameplay composition, but UI text/prompt readability still needs a dedicated pass before calling Step 6 complete.
- After live review feedback, `ActionFoundationCinematicP0Review.unity` now saves Main Camera at the opening face shot instead of the previous rear action-camera transform. The verifier validates this in `ValidateReviewScene()`.

### 2026-06-24 Profile-Driven Weapon Visibility Pass

Runtime/profile changes:

- `CinematicSequenceProfile.ActorCueKind` now includes `WeaponVisibility`.
- `CinematicSequenceProfile.ActorCue` now carries an `objectActive` flag for visibility cues.
- `CinematicSequenceRunner` can find actor child objects by path/name/partial name and toggle them active/inactive during profile playback.

P0 weapon timing:

- `IntroAwakening`: hides the rifle from sequence start, then shows it after the pickup beat.
- `GameplayHandoff`, `QTEAssist`, `UltimateCutIn`, `DangerCue`, and `CombatTutorialOverlay`: start with the rifle visible for combat-facing playback.

Verifier hardening:

- `BuildResubmissionCinematicPackageVerifier` now fails if a P0 profile has no weapon visibility cue.
- The verifier checks that `IntroAwakening` hides the rifle before it shows it.
- The verifier checks that combat-facing P0 profiles start with rifle visibility enabled.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-WeaponVisibility.log`
- result: PASS, failures 0, warnings 0

Preview evidence:

- `C:\tmp\DimensionBrawl-CinematicP0Review-IntroAwakening.png`: intro early/body-reveal shot has no rifle visible.
- `C:\tmp\DimensionBrawl-CinematicP0Review-QTEAssist.png`: QTE combat-facing shot has rifle visible.
- `C:\tmp\DimensionBrawl-CinematicP0Review-UltimateRelease.png`: ultimate combat-facing shot has rifle visible.

### 2026-06-24 P0 Playlist Review Route

Runtime/editor code added:

- `Assets/_Game/Scripts/Presentation/CinematicSequencePlaylistRunner.cs`
- `CinematicSequenceRunner.TryPlayProfile(...)`

Review scene route:

1. `DB_Cinematic_IntroAwakening.asset`
2. `DB_Cinematic_QTEAssist.asset`
3. `DB_Cinematic_UltimateCutIn.asset`
4. `DB_Cinematic_DangerCue.asset`
5. `DB_Cinematic_CombatTutorialOverlay.asset`
6. `DB_Cinematic_GameplayHandoff.asset`

Scene behavior:

- `ActionFoundationCinematicP0Review.unity` now has `CinematicSequencePlaylistRunner`.
- The single-profile `CinematicSequenceAutoPlay` remains present for fallback/manual testing, but it is disabled while the playlist is active.
- The playlist auto-plays for review and uses the same Inori actor, expression player, weapon object, VFX cue player, and direct cinematic camera.
- The saved Main Camera now starts on the playlist's first Intro opening shot (`capsule_wakeup_first_person`) rather than the old Ultimate-only opening shot.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-PlaylistOrder.log`
- result: PASS, failures 0, warnings 0

Verifier checks:

- review scene has `CinematicSequencePlaylistRunner`
- playlist has six entries
- playlist auto-plays for review
- single-profile AutoPlay is disabled
- playlist order is Intro, QTE, Ultimate, Danger, Tutorial, Handoff

Remaining caveat:

- This pass proves the route is serialized and validated. It does not yet prove the full 50+ second route is visually clean at runtime by itself, so the following pass adds editor-sampled playlist visual QA.

### 2026-06-24 P0 Playlist Capture Strip

Why this was added:

- Live review showed the previous claim was too optimistic: some shots still read as rear/head-back views or cropped faces when checked visually.
- `OpeningFace` also used an Ultimate sample despite the name, which made it unclear whether the Intro opening shot was really being reviewed.

New capture outputs:

- contact sheet: `C:\tmp\DimensionBrawl-CinematicP0Review-PlaylistStrip.png`
- frame report: `C:\tmp\DimensionBrawl-CinematicP0Review-PlaylistStrip.md`
- individual frames: `C:\tmp\DimensionBrawl-CinematicP0Review-PlaylistFrames\*.png`

Fixes applied in this pass:

- `capsule_wakeup_first_person` now pulls back enough to keep the face/head in frame.
- `gun_pickup_action` now samples the rifle-ready payoff after weapon visibility, instead of forcing the pickup animation at an off-profile time.
- tutorial and handoff review shots moved from pure rear camera to readable front/three-quarter camera framing.
- `OpeningFace` preview now captures the Intro opening shot instead of the Ultimate opening shot.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchPlaylistStripCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP0Review-PlaylistStrip-Fix2.log`
- result: PASS, generated the 9-frame playlist strip
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchPreviewCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP0Review-Preview-Fix2-Retry.log`
- result: PASS, refreshed module preview PNGs
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-AfterPlaylistStripFix.log`
- result: PASS, failures 0, warnings 0

Remaining caveat:

- The strip is editor-authored visual QA. The next pass adds runner-driven sampling so the capture uses the runtime cinematic runner's cue dispatch path.

### 2026-06-24 Runner-Driven P0 Playlist QA

Why this was added:

- The editor strip proved framing, but it still applied pose/expression/camera samples manually.
- To reduce the gap between preview QA and runtime behavior, `CinematicSequenceRunner` now exposes `TryApplyProfileSampleForReview(...)`.
- Runner-driven capture uses the same runner dispatch path for camera cues, actor cues, weapon visibility, VFX cue counters, and tutorial cue counters.

New capture outputs:

- contact sheet: `C:\tmp\DimensionBrawl-CinematicP0Review-RunnerDrivenStrip.png`
- frame report: `C:\tmp\DimensionBrawl-CinematicP0Review-RunnerDrivenStrip.md`
- individual frames: `C:\tmp\DimensionBrawl-CinematicP0Review-RunnerDrivenFrames\*.png`

Runtime/editor code added or changed:

- `CinematicSequenceRunner.TryApplyProfileSampleForReview(...)`
- `CinematicSequenceRunner` review sampling now resolves body animation normalized time from the sampled profile time.
- `CombatVfxCuePlayer.StopAllActiveCuesForReview()` lets QA captures isolate each sampled beat without VFX residue.
- `BuildResubmissionCinematicReviewSceneSetup.RunBatchRunnerDrivenPlaylistCapture`

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchRunnerDrivenPlaylistCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP0Review-RunnerDrivenStrip-Fix2.log`
- result: PASS, generated the runner-driven 9-frame playlist strip
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-RunnerDrivenStrip.log`
- result: PASS, failures 0, warnings 0

Runner-driven report expectations:

- each sampled beat's runner camera cue should match the expected camera cue
- each sampled beat should have the expected actor cue family applied
- QTE, Ultimate, Danger, and Tutorial samples should show their VFX/tutorial cue counters where applicable
- Intro early samples keep the rifle hidden; combat-facing samples keep it visible

Remaining caveat:

- This is stronger than manual editor sampling, but it is still a deterministic sample capture, not a continuous Play Mode video of the entire 50+ second playlist. The next quality gate should be a full Play Mode route capture/timeline recording.

### 2026-06-24 Play Mode P0 Route Capture

Why this was added:

- Runner-driven sampling was still too forgiving because it could force camera poses immediately and did not expose runtime blend timing problems.
- Live review caught that the `gun_pickup_action` and `capsule_open_body_reveal` beats could technically pass cue-ID validation while still rendering cropped or top-of-head views.

New capture outputs:

- contact sheet: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRouteStrip.png`
- frame report: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRoute.md`
- result marker: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRoute.result`
- individual frames: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeFrames\*.png`

Runtime/editor code added or changed:

- `CinematicPlaylistPlayModeCaptureProbe` captures the actual review playlist during Play Mode by rendering the active cinematic camera at route timestamps.
- `BuildResubmissionCinematicReviewSceneSetup.RunBatchPlayModeRouteCapture` now creates the review scene, enters Play Mode, installs the capture probe after Play Mode starts, and exits Unity based on the probe result file.
- `gun_pickup_action` camera blend was shortened from 4.8s to 1.1s because Play Mode capture exposed that the camera was still mid-blend and cropped Inori during the rifle-ready payoff.
- `capsule_open_body_reveal` now uses a 1.1s front/three-quarter reveal camera and `CIN_IntroSurprised` instead of sampling the early `CIN_IntroStumble` fall frame, which read as the back/top of Inori's head.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchPlayModeRouteCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRoute-Fix6.log`
- result: PASS, generated the 9-frame Play Mode route strip
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-PlayModeRouteFix6.log`
- result: PASS, failures 0, warnings 0

Play Mode route expectations:

- Intro wake and body reveal keep the rifle hidden and show Inori's face.
- Rifle ready, QTE, Ultimate, Danger, Tutorial, and Handoff samples keep the rifle visible.
- QTE, Ultimate, Danger, and Tutorial samples carry their expected camera/VFX/tutorial cue IDs through the runtime playlist.

Remaining caveat:

- This is a sampled Play Mode route, not a finished cinematic movie render. The next quality gate should add continuous MP4/timeline capture, stronger scene dressing/lighting, and final tutorial/QTE prompt readability.

### 2026-06-24 Readable QTE/Tutorial Prompt Presenter

Why this was added:

- The previous P0 route report proved tutorial cue IDs, but the Play Mode PNGs showed no actual prompt overlay on the camera-rendered frames.
- A regular `OnGUI` prompt would not be enough for the current verifier because the Play Mode probe captures via `Camera.Render()`.

Runtime/editor code added or changed:

- `CinematicTutorialPromptPresenter` renders a camera-attached TextMesh prompt banner, so QTE/tutorial/warning prompts are visible in camera render textures and Play Mode PNGs.
- `CinematicSequenceRunner` now dispatches `TutorialCue` data to the presenter instead of only counting the cue ID.
- `BuildResubmissionCinematicReviewSceneSetup` adds and configures the presenter on the review runner and binds it to the cinematic camera.
- `CinematicPlaylistPlayModeCaptureProbe` now records an active `Prompt` column and fails when a sampled tutorial cue has no matching active prompt.
- QTE prompt duration was extended to 2.35s so the hit-confirm sample still has visible timing text.
- Prompt anchors and scale were retuned so QTE, Danger, Attack, and Skill prompts stay readable without covering Inori's face.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchPlayModeRouteCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRoute-PromptPresenter-Fix3.log`
- result: PASS, generated the 9-frame Play Mode route strip with active prompt IDs
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-PromptPresenter-Fix3.log`
- result: PASS, failures 0, warnings 0

Prompt evidence:

- `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeFrames\04_04_qte_hit_confirm.png`: shows `QTE` / `TIMING`.
- `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeFrames\06_06_danger_warning.png`: shows `DANGER` / `EVADE`.
- `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeFrames\07_07_tutorial_basic.png`: shows `ATTACK` / `BASIC`.
- `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeFrames\08_08_tutorial_skill.png`: shows `SKILL` / `CAST`.

Remaining caveat:

- These are functional review overlays, not the final production HUD art direction. The final pass should replace the TextMesh banner with the production UI owner once the gameplay HUD style is locked.

### 2026-06-24 P0 Review Stage Dressing Pass

Why this was added:

- The previous Play Mode route showed Inori, camera, weapon, VFX, and prompts, but the frames still read as a gray test floor with a default sky horizon.
- A reusable cutscene package needs a stable review environment so camera framing, silhouettes, prompt readability, and weapon/VFX contrast can be judged without false negatives from a blank scene.

Runtime/editor code added or changed:

- `BuildResubmissionCinematicReviewSceneSetup` now creates `CinematicP0Review_StageRoot` when regenerating `ActionFoundationCinematicP0Review.unity`.
- The generated stage includes a dark back screen, side depth panels, player readability floor field, colored floor guide lines, capsule-side light pillars, threat gate panels, and three authored point lights.
- The review camera now uses a dark solid background color instead of the default skybox, so side-angle shots no longer expose blue horizon gaps.
- `BuildResubmissionCinematicPackageVerifier` now fails if the review scene loses the stage root, back screen, readability field, or key face light.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchPlayModeRouteCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRoute-StageDressing-Fix2.log`
- result: PASS, generated the 9-frame Play Mode route strip with dressed background
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-StageDressing-Fix2.log`
- result: PASS, failures 0, warnings 0

Stage evidence:

- contact sheet: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRouteStrip.png`
- saved scene: `Assets/_Game/Scenes/ActionFoundationCinematicP0Review.unity`
- required scene objects: `CinematicP0Review_StageRoot`, `CinematicP0Review_BackScreen`, `CinematicP0Review_PlayerReadabilityField`, `CinematicP0Review_KeyFaceLight`

Remaining caveat:

- This is a reusable review shell, not final shipped environment art. The next visual pass should replace primitive panels with production stage assets or authored meshes once the final setting is selected.

### 2026-06-24 Play Mode Timeline Capture

Why this is being added:

- The 9-frame Play Mode route strip proves key beats, but it can still miss a bad transition between samples.
- Review needs a quick way to scan the full route and catch back-of-head framing, unreadable prompt timing, weapon-only motion, or awkward camera blends across the actual runtime playlist.

Runtime/editor code added or changed:

- `CinematicPlaylistPlayModeCaptureProbe` now supports a timeline capture pass in addition to the 9 sampled route frames.
- The timeline pass captures the active cinematic camera every 2.5 seconds, writes individual frames under `C:\tmp\DimensionBrawl-CinematicP0Review-TimelineFrames`, creates `C:\tmp\DimensionBrawl-CinematicP0Review-TimelineStrip.png`, and writes `C:\tmp\DimensionBrawl-CinematicP0Review-Timeline.md`.
- The result marker now includes `TIMELINE_FRAMES`, `TIMELINE_REPORT`, and `TIMELINE_STRIP` fields so batch verification output can point directly to the review artifacts.
- `BuildResubmissionCinematicReviewSceneSetup.ConfigurePlayModeRouteCaptureProbe` configures timeline capture width, height, interval, minimum expected frame count, and output paths when the Play Mode route capture probe is installed.
- Both the 9-sample route strip and the 23-frame timeline strip now draw large in-image labels for sample number, route time, profile, and camera cue, so the PNG is readable without cross-checking the markdown table first.
- The timeline capture also forces one final route-end frame after the last sampled beat, so `gameplay_handoff` is visible in the timeline strip instead of only in the sampled route strip.
- `enemy_standoff_threat_direction` and `danger_brace_return` were retuned to preserve front/three-quarter Inori readability; the intentional rear/gameplay view is now kept to the explicit gameplay handoff beats.

Verification:

- `dotnet build C:\Git\DimensionBrawl\DimensionBrawl.Runtime.csproj` with temporary outputs under `C:\tmp`: PASS, warnings only.
- `dotnet build C:\Git\DimensionBrawl\Assembly-CSharp-Editor.csproj` against copied Unity-built dependency DLLs under `C:\tmp`: PASS, warnings only.
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchPlayModeRouteCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRoute-FinalTimelineFrame.log`
- result: PASS, generated 9 labeled route frames and 23 labeled timeline frames
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-FinalTimelineFrame.log`
- result: PASS, failures 0, warnings 0

Review outputs:

- labeled sample strip: `C:\tmp\DimensionBrawl-CinematicP0Review-PlayModeRouteStrip.png`
- labeled timeline strip: `C:\tmp\DimensionBrawl-CinematicP0Review-TimelineStrip.png`
- timeline report: `C:\tmp\DimensionBrawl-CinematicP0Review-Timeline.md`
- timeline frames: `C:\tmp\DimensionBrawl-CinematicP0Review-TimelineFrames\*.png`

Remaining caveat:

- This is still a frame-sequence/contact-sheet route capture rather than a final MP4 movie render. It is enough to review continuity and bad framing quickly, but production submission should still get a dedicated movie-capture path if the build pipeline needs video artifacts.

### 2026-06-24 P1 Reusable Profile Expansion

Why this was added:

- The build needs more than the opening and immediate combat prompts. Reusable coverage must include boss/elite entrances, phase transitions, break payoffs, dialogue reactions, result bridges, and summon/assist entries.
- These profiles are not final authored scenes yet, but they make the reusable cinematic package broader and force P1 situations through the same camera/actor/VFX/handoff data model as P0.

Generated P1 profile assets:

- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_BossIntro.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_PhaseTransition.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_BreakMoment.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_DialogueReactionBeat.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_ResultBridge.asset`
- `Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_SummonEntry.asset`

Runtime/editor code added or changed:

- `BuildResubmissionCinematicProfileSetup` now generates both P0 and P1 cinematic sequence profiles.
- The P1 profiles use authored direct shot poses instead of default action-camera fallback.
- Every P1 profile drives Inori body animation, Inori face expression, rifle visibility, and explicit gameplay/result handoff data.
- `BossIntro`, `PhaseTransition`, `BreakMoment`, `ResultBridge`, and `SummonEntry` also bind combat VFX cue IDs for threat, phase, break, result, or summon feedback.
- `BuildResubmissionCinematicPackageVerifier` now requires all 12 build-resubmission cinematic profiles and checks P1 profiles for direct shot poses, body cues, face cues, weapon visibility, and handoff coverage.

Verification:

- `dotnet build C:\Git\DimensionBrawl\DimensionBrawl.Runtime.csproj` with temporary outputs under `C:\tmp`: PASS, warnings 0.
- `dotnet build C:\Git\DimensionBrawl\Assembly-CSharp-Editor.csproj` against copied Unity-built dependency DLLs under `C:\tmp`: PASS, warnings only.
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicProfileSetup.RunBatchProfileGeneration`
- log: `C:\tmp\DimensionBrawl-BuildResubmissionCinematicProfiles-P1.log`
- result: PASS, generated P0/P1 profile assets
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-P1Profiles.log`
- result: PASS, failures 0, warnings 0

Remaining caveat:

- P1 profiles are data-complete first-pass modules, not yet visually approved route captures. The next quality gate should create an inspectable P1 playlist or preview strip, then retune any unsafe animation/camera pair on Inori before treating Step 7 as complete.

### 2026-06-24 Corridor Back-View Projectile Correction

Core action language:

- The main combat read is corridor-style back-view / over-shoulder projectile firing.
- Action, QTE payoff, break confirm, phase counter, summon follow-up, and gameplay handoff shots should show Inori from behind or rear three-quarter when the point is projectile direction, lane depth, muzzle timing, impact timing, and return-to-gameplay alignment.
- A rear shot is not automatically a failure. It is a failure only when the beat needs emotion/story readability and instead shows the back of the head, cropped torso, or unreadable body motion.
- Dialogue, shock, resolve, and other emotional beats still need readable face/eyes/expression before returning to the combat back-view.

Implementation correction:

- Promote explicit `CIN_BackViewProjectile*` animation states from the GreatSword Bow Shoot in-place clips instead of using sword slashes for projectile-heavy beats.
- P1 action payoff cameras should sit behind Inori with negative local Z and look forward into positive local Z so the review scene reads as a lane/corridor shot, not a detached front-side pose preview.
- Projectile payoff beats should pair body animation with `PlayerRangedMuzzleFlash` and `PlayerRangedProjectileImpact` cues where possible.
- Review samples must label the intended state/camera cue clearly enough to tell back-view projectile beats apart from accidental back-of-head emotional shots.

### 2026-06-24 Summon Actor And Dragon Read Correction

Core summon language:

- Summon beats should stay in the same corridor/action grammar as projectile combat unless the beat is explicitly emotional or story-facing.
- A summon cutscene is not only an entry flash. It needs at least one readable actor layer: summon manifestation, Inori command/empower beat, summon attack/clash, summon defeat/recall, or boss pressure summon.
- `SummonEntry` now binds an `ActorRole.Summon` runtime actor in the review scene so body-trigger cues can target the summon actor instead of leaving summon VFX detached from a visible performer.
- The review scene uses the existing `SummonFrontlineProxy` vanguard prefab as the first build-facing summon actor, because it is closer to the actual combat system than a primitive proxy.
- The Volcano Dragon PBR prefab is included as a summon-candidate background layer for the same `SummonEntry` sample. It is not final gameplay balance proof; it is an inspectable candidate for large summoned-creature framing, scale, and animation readability.

Implementation correction:

- The P1 runner-driven review strip now samples `SummonEntry` with Inori firing from the corridor back-view, the vanguard summon proxy visible in front-right space, and the Volcano Dragon in a higher rear layer using `FlyStationarySpitFireBall`.
- The summon frame should prove three separate reads at once: Inori command/projectile intent, summon actor presence, and large-creature support silhouette.
- Keep dragon and other large summons out of the primary emotional face layer unless the shot is authored for them. In combat samples, they should support depth, threat, and spectacle without hiding Inori, the enemy, or the summon proxy.
- Future summon variants should branch into command, empower, clash, recall, and boss-summon-pressure modules rather than replacing the corridor projectile language.

### 2026-06-24 Action Cue Bridge Integration

Why this was added:

- A cinematic package that only plays in the isolated review scene is not enough for build resubmission.
- Existing combat input already requests action cinematic cues through `ActionCinematicCueDirector`; the reusable `CinematicSequenceRunner` now needs to be reachable from that route.

Runtime/editor changes:

- Added `ActionCinematicSequenceBridge`, which maps existing `ActionCinematicCueProfile.CueKind` values to reusable `CinematicSequenceProfile` assets.
- `ActionCinematicCueDirector` now detects the bridge, plays the mapped sequence when present, suppresses the older short camera/signals while the mapped sequence owns presentation, and keeps movement/input locks alive for the mapped sequence duration.
- `CinematicSequenceRunner` now supports a temporary body Animator controller override. This lets gameplay Inori keep the normal rifle controller outside cutscenes, then switch to `DB_Inori_CinematicP0.controller` only while a build-resubmission cinematic profile is playing.
- The controller override is role-gated to Inori/Player bindings so summon actors keep their own Animator controllers.
- Boss barrage review scene generation now binds ultimate, summon entry, boss-pressure break, summon follow-up hit, pocket clear, and pocket fail routes to build-resubmission profiles. Normal low-tier skill cut-in remains unmapped there so routine shots do not become long QTE-style cutscenes.
- Added `BossBarrageActionBridgeRouteProbe` plus a batch runner that enters Play Mode in `ActionFoundationBossBarrageLaneReview.unity`, grants tier-3 EN, calls `PlayerSkill1Action.TryUseSkill1()` and `PlayerSummonSlot1Action.TryUseSummonSlot1()`, and writes whether the cue director, sequence bridge, and cinematic runner all observed the expected route.
- The input-route probe now captures the active cinematic camera to PNG frames for the tier-3 ultimate and summon-entry routes, so the route is both mechanically verified and visually inspectable.
- `SummonEntry` moved its first two camera poses from front/face closeups to rear and side-rear corridor positions after route capture showed the summon proxy could fully occlude Inori. The current read keeps Inori in back view with the large summon support silhouette and entry circle in front.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchReviewSceneGeneration`
- log: `C:\tmp\DimensionBrawl-CinematicReviewScene-ActionBridge.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-ActionBridge.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.EnsureBossBarrageLaneReviewScene`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-ActionBridge-Ensure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.ValidateBossBarrageLaneReviewScene`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-ActionBridge-Validate.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.RunBatchActionBridgeInputRouteVerification`
- log: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute-Final.log`
- result file: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute.result`
- result: PASS, exit code 0
- observed route: `skill1_tier3_ultimate` -> `UltimateCutIn` -> `ultimate_cutin`
- observed route: `summon_slot1_tier3_entry` -> `SummonEntry` -> `summon_entry`
- capture: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\01_skill1_tier3_ultimate.png`
- capture: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\02_summon_slot1_tier3_entry.png`
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-SummonCameraFix.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0

Remaining caveat:

- This proves the actual player-action route, not only generated scene references. It now includes boss-barrage route camera/frame capture, but it is still a QA frame sequence rather than a final movie render. The next gate should expand summon variants into command, empower, clash, recall, and boss-summon-pressure beats, then tune the large actor scale/lighting toward production polish.

### 2026-06-24 Boss Barrage Summon Route Actor Binding

Why this was added:

- The actual summon route must not only play camera/VFX data. It must prove that a visible summon actor or support actor receives the cinematic actor cues.
- The active `SummonFrontlineProxy` is spawned by gameplay code, so the reusable runner needs to find it dynamically instead of depending only on a scene-time serialized binding.
- Dragon support should be treated as an external actor role, not as Inori weapon visibility or Inori controller state.

Runtime/editor changes:

- `CinematicSequenceRunner` now tracks `TotalBoundActorCueCount` and dynamically resolves an active visible `SummonFrontlineProxy` when an `ActorRole.Summon` cue is dispatched without a serialized binding.
- `DB_Cinematic_SummonEntry.asset` now includes external support-dragon visibility/body cues for `ActorRole.Environment`, using the Volcano Dragon `FlyStationarySpitFireBall` state.
- `ActionFoundationBossBarrageLaneReview.unity` generation adds a review-only Volcano Dragon support actor, binds it as `ActorRole.Environment`, and places it on the right flank so the summon route can show Inori, the frontline summon, and a large-creature support layer in the same playable flow.
- `BossBarrageActionBridgeRouteProbe` now requires bound actor cue dispatch and captures three summon-entry beats: signal start, command/proxy attack, and hit/handoff.
- `BuildResubmissionCinematicPackageVerifier` now applies Inori controller/socket checks only to Inori cues, so external actors can carry their own body states and object visibility toggles.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicProfileSetup.RunBatchProfileGeneration`
- log: `C:\tmp\DimensionBrawl-BuildResubmissionCinematicProfiles-DragonSupport-OpenFov.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.EnsureBossBarrageLaneReviewScene`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-DragonSupport-RightFlank-Ensure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.ValidateBossBarrageLaneReviewSceneMenu`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-DragonSupport-FinalValidate.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.RunBatchActionBridgeInputRouteVerification`
- log: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute-DragonSupport-MultiCapture.log`
- result file: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute.result`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchReviewSceneGeneration`
- log: `C:\tmp\DimensionBrawl-CinematicReviewScene-DragonSupport.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-DragonSupport-Fix.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0

Route captures:

- `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\01_skill1_tier3_ultimate.png`
- `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\02_summon_slot1_tier3_entry.png`
- `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\03_summon_slot1_tier3_entry_command.png`
- `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\04_summon_slot1_tier3_entry_hit.png`

Quality note:

- The route is now mechanically real and visually inspectable: Skill1 triggers `UltimateCutIn`, SummonSlot1 triggers `SummonEntry`, bound actor cues are observed, and the hit/handoff capture shows Inori with the frontline summon and right-flank dragon support. It is still a review-scene composition pass, not final production cinematography.

### 2026-06-24 Summon Follow-up Profile Split

Why this was added:

- `ActionCinematicCueProfile.CueKind.SummonFollowupHit` existed, but the reusable sequence bridge was still temporarily mapping it to `BreakMoment`.
- That made summon follow-up hits look like generic pressure breaks instead of a distinct Inori command plus summon attack payoff.

Runtime/editor changes:

- Added `DB_Cinematic_SummonFollowupHit.asset` as a dedicated reusable P1 profile with command, clash, dragon-cross, and handoff camera cues.
- Added `SequenceCategory.SummonFollowupHit` without shifting existing serialized enum values.
- `ActionFoundationCinematicP0Review.unity` and `ActionFoundationBossBarrageLaneReview.unity` now map `summonFollowupHitProfile` to `DB_Cinematic_SummonFollowupHit.asset`.
- P1 runner-driven review now includes a seventh sample, `p1_07_summon_followup_clash`, with Inori left/rear command framing, the frontline summon in the lane, and Volcano Dragon support on the right flank.
- Package verifier now requires the new profile and rejects the old `SummonFollowupHit -> BreakMoment` bridge mapping.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicProfileSetup.RunBatchProfileGeneration`
- log: `C:\tmp\DimensionBrawl-BuildResubmissionCinematicProfiles-SummonFollowup-Retune.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchReviewSceneGeneration`
- log: `C:\tmp\DimensionBrawl-CinematicReviewScene-SummonFollowup.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.EnsureBossBarrageLaneReviewScene`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-SummonFollowup-Ensure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.ValidateBossBarrageLaneReviewSceneMenu`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-SummonFollowup-Validate.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-SummonFollowup-Retune.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchP1RunnerDrivenPlaylistCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP1RunnerDriven-SummonFollowup-Retune.log`
- result: PASS, exit code 0

Visual QA:

- contact sheet: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenStrip.png`
- frame: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenFrames\07_p1_07_summon_followup_clash.png`
- report: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenStrip.md`

Quality note:

- The tuned clash frame now keeps Inori, the frontline summon, VFX impact, and dragon support visible together. It is still a review-stage composition, but the follow-up hit is no longer a generic break moment.

### 2026-06-24 Summon Empower And Recall Expansion

Why this was added:

- The build needs reusable summon lifecycle beats, not only summon entry and hit confirmation.
- Combat-facing summon presentation should support Inori commanding a frontline summon, empowering it, recalling it, and using large-creature support in the same corridor/back-view grammar.

Runtime/editor changes:

- Added `SequenceCategory.SummonEmpower` and `SequenceCategory.SummonRecall` without shifting existing serialized enum values.
- Added `DB_Cinematic_SummonEmpower.asset` with channel, transfer, hold, and handoff camera cues; Inori charge/fire/aim body cues; summon manifest/attack triggers; dragon visibility/fire cues; and VFX for channel, transfer, guard, and release beats.
- Added `DB_Cinematic_SummonRecall.asset` with signal, collapse, dragon-exit, and handoff camera cues; Inori aim/recover/ready body cues; summon and dragon actor cues; and recall-safe VFX.
- P1 runner-driven review now includes `p1_08_summon_empower_transfer` and `p1_09_summon_recall_collapse`.
- Package verifier now requires both new profiles and treats them as P1 direct-shot/body/face/weapon/handoff coverage.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicProfileSetup.RunBatchProfileGeneration`
- log: `C:\tmp\DimensionBrawl-BuildResubmissionCinematicProfiles-SummonLifecycle-Retune3.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchReviewSceneGeneration`
- log: `C:\tmp\DimensionBrawl-CinematicReviewScene-SummonLifecycle-Retune2.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-SummonLifecycle-Retune3.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchP1RunnerDrivenPlaylistCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP1RunnerDriven-SummonLifecycle-Retune3.log`
- result: PASS, exit code 0

Visual QA:

- contact sheet: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenStrip.png`
- empower frame: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenFrames\08_p1_08_summon_empower_transfer.png`
- recall frame: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenFrames\09_p1_09_summon_recall_collapse.png`
- report: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenStrip.md`

Quality note:

- `SummonEmpower` reads as Inori sending power forward to the frontline summon while the support dragon remains visible in the rear layer.
- The first `SummonRecall` collapse pass was rejected because `SummonFollowupMissed` rendered as a large vertical death-column and blocked the frame. The accepted pass uses a smaller summon-opportunity cue and a safe corridor back-view camera so Inori, the enemy lane, dragon, and recall signal remain visible.

### 2026-06-24 Summon Lifecycle Action Bridge Integration

Why this was added:

- `SummonEmpower` and `SummonRecall` should not remain isolated review samples. They need a route through the same action cinematic director and sequence bridge used by actual boss-barrage review actions.
- The boss-barrage slice is the current best proof surface because it already exercises tier-3 Skill1, SummonSlot1, summon follow-up, visible summon actor binding, and Volcano Dragon support.

Runtime/editor changes:

- Added `ActionCinematicCueProfile.CueKind.SummonEmpower` and `ActionCinematicCueProfile.CueKind.SummonRecall` at the end of the enum to avoid shifting existing serialized cue values.
- Added fallback `CueSequence` data for the two new action cue kinds so `ActionCinematicCueDirector.TryPlay(...)` can route them even before the reusable sequence bridge takes over camera/VFX playback.
- `ActionCinematicSequenceBridge` now has `summonEmpowerProfile` and `summonRecallProfile` slots and resolves them to `DB_Cinematic_SummonEmpower.asset` and `DB_Cinematic_SummonRecall.asset`.
- `BossBarragePocketCameraCueBridge` requests `SummonEmpower` when the summon block opportunity opens and `SummonRecall` when the summon follow-up window is missed.
- P0 review scene generation, boss-barrage review scene generation, boss-barrage validation, and package verification now all check the new bridge profile references.
- `BossBarrageActionBridgeRouteProbe` now verifies direct director-to-sequence bridge playback for `SummonEmpower` and `SummonRecall` after the real Skill1/SummonSlot1 route checks.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchReviewSceneGeneration`
- log: `C:\tmp\DimensionBrawl-CinematicReviewScene-SummonLifecycleBridge.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.EnsureBossBarrageLaneReviewScene`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-SummonLifecycleBridge-Ensure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.ValidateBossBarrageLaneReviewSceneMenu`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-SummonLifecycleBridge-Validate.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-SummonLifecycleBridge.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.RunBatchActionBridgeInputRouteVerification`
- log: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute-SummonLifecycleBridge-Retune.log`
- result: PASS, route result file reports `RESULT=PASS`

Route capture:

- result file: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute.result`
- frame 05: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\05_summon_empower_direct_bridge.png`
- frame 06: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\06_summon_recall_direct_bridge.png`

Quality note:

- The bridge route frames use the actual boss-barrage review stage, so they read better than the plain P1 review strip: Inori remains in corridor back-view, the frontline summon occupies the lane, and the Volcano Dragon remains visible as a support layer.
- This still proves deterministic route frames rather than final authored movie capture. It is enough to trust the new cue kinds and bridge mapping, but production polish still needs per-shot lighting, occlusion, animation, and continuous capture passes.

### 2026-06-25 Boss Summon Pressure Profile Split

Why this was added:

- `BossPressureBreak` was still routed to the generic `BreakMoment` profile.
- The boss-barrage route now has summon entry, summon follow-up, empower, and recall beats, so the pressure-break response should also use a summon-aware profile rather than a generic guard break shot.

Runtime/editor changes:

- Added `SequenceCategory.BossSummonPressure` at the end of the cinematic profile enum.
- Added `DB_Cinematic_BossSummonPressure.asset` as a dedicated P1 profile with pressure-wall, summon-guard, crack, and handoff camera cues.
- The profile drives Inori back-view projectile animation, Inori face state, weapon visibility, frontline summon actor cues, support-dragon cues, and VFX for pressure window, shield guard, muzzle answer, impact crack, and dragon mark.
- `BossPressureBreak` bridge mapping now points to `DB_Cinematic_BossSummonPressure.asset` in both the P0 cinematic review scene and the boss-barrage review scene.
- P1 runner-driven review adds `p1_10_boss_summon_pressure_guard`.
- The action bridge route probe now directly verifies `BossPressureBreak -> boss_summon_pressure`.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicProfileSetup.RunBatchProfileGeneration`
- log: `C:\tmp\DimensionBrawl-BuildResubmissionCinematicProfiles-BossSummonPressure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchReviewSceneGeneration`
- log: `C:\tmp\DimensionBrawl-CinematicReviewScene-BossSummonPressure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.EnsureBossBarrageLaneReviewScene`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-BossSummonPressure-Ensure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.ValidateBossBarrageLaneReviewSceneMenu`
- log: `C:\tmp\DimensionBrawl-BossBarrageLaneReview-BossSummonPressure-Validate.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-BossSummonPressure.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicReviewSceneSetup.RunBatchP1RunnerDrivenPlaylistCapture`
- log: `C:\tmp\DimensionBrawl-CinematicP1RunnerDriven-BossSummonPressure.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.RunBatchActionBridgeInputRouteVerification`
- log: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute-BossSummonPressure.log`
- result: PASS, route result file reports `RESULT=PASS`

Visual QA:

- P1 sample frame: `C:\tmp\DimensionBrawl-CinematicP1Review-RunnerDrivenFrames\10_p1_10_boss_summon_pressure_guard.png`
- action bridge route frame: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\05_boss_summon_pressure_direct_bridge.png`
- result file: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute.result`

Quality note:

- The route frame clearly reads as Inori supporting from corridor back-view while the frontline summon absorbs a boss pressure beat. Dragon support remains visible as a flank layer.
- The route result may show later director events after the direct bridge sample because boss-barrage follow-up events continue to tick, but the bridge, profile, camera, actor, and VFX evidence for `boss_summon_pressure` all pass before the follow-up event changes the final director label.

### 2026-06-25 Boss Barrage Route Strip And Dragon Gate

Why this was added:

- The boss-barrage route had individual PNG captures, but no single inspectable strip for the real action bridge path.
- Dragon support was present in the profile data, but the automated route proof did not fail if the support-dragon layer silently disappeared from the active shot.

Runtime/editor changes:

- `BossBarrageActionBridgeRouteProbe` now writes a labeled route contact sheet and markdown report after the Play Mode bridge verification.
- The route report records sequence id, last camera cue, actor cue, VFX cue, frame path, and support-dragon visibility/frustum status per sample.
- Summon-scoped route frames require the support dragon to be active and inside the cinematic camera frustum, except for the intentional `summon_slot1_tier3_entry_command` close/action beat where the frame is allowed to focus on Inori and the frontline summon.
- The same action route strip now includes direct `PocketClear -> result_bridge` and `PocketFail -> danger_cue` captures so result/failure handoffs are not only verifier-map assertions.
- `DB_Cinematic_ResultBridge.asset` and `DB_Cinematic_DangerCue.asset` were retuned so the route strip avoids an accidental body-only result close-up and keeps the combat corridor readable.
- `ActionFoundationBossBarrageLaneReviewSetup.RunBatchActionBridgeInputRouteVerification` configures the probe output paths and the dragon-visibility gate.

Verification:

- method: `DimensionBrawl.Editor.BuildResubmissionCinematicProfileSetup.RunBatchProfileGeneration`
- log: `C:\tmp\DimensionBrawl-BuildResubmissionCinematicProfiles-ResultDangerCameraRetune.log`
- result: PASS, exit code 0
- method: `DimensionBrawl.Editor.ActionFoundationBossBarrageLaneReviewSetup.RunBatchActionBridgeInputRouteVerification`
- log: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute-PocketResultRoutes-Retune.log`
- result: PASS, route result file reports `RESULT=PASS`
- method: `DimensionBrawl.Editor.BuildResubmissionCinematicPackageVerifier.RunBatchVerification`
- log: `C:\tmp\DimensionBrawl-CinematicPackageVerifier-ResultDangerCameraRetune.log`
- report: `C:\tmp\DimensionBrawl-CinematicPackageVerifier.md`
- result: PASS, failures 0, warnings 0

Visual QA:

- contact sheet: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteStrip.png`
- report: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute.md`
- result file: `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRoute.result`
- route frames:
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\01_skill1_tier3_ultimate.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\02_summon_slot1_tier3_entry.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\03_summon_slot1_tier3_entry_command.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\04_summon_slot1_tier3_entry_hit.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\05_boss_summon_pressure_direct_bridge.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\06_summon_empower_direct_bridge.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\07_summon_recall_direct_bridge.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\08_pocket_clear_result_direct_bridge.png`
  - `C:\tmp\DimensionBrawl-BossBarrageActionBridgeRouteFrames\09_pocket_fail_danger_direct_bridge.png`

Quality note:

- The strip proves that tier-3 Skill1, SummonSlot1 entry, boss-summon pressure, summon empower, summon recall, pocket-clear result, and pocket-fail danger all route through the reusable cinematic bridge and generate inspectable camera frames.
- The support dragon is visible in the required summon-scoped route frames and remains a separate `ActorRole.Environment` layer, not a replacement for Inori body/weapon presentation.
- The result bridge now reads as a back-view corridor settle with weapon and lane context instead of a cropped torso close-up. The danger bridge remains a short reaction/warning close-up.
- This is a route evidence and regression gate. It does not replace final per-shot cinematography, lighting, occlusion, animation retune, or continuous movie capture.

## Source Data Read

Use these local ArkData sources as production references:

- `\\DESKTOP-69817L3\ArkData\CutscenePattern_ApplyData_2026-06-24`
- `\\DESKTOP-69817L3\ArkData\TutorialSystem_ApplyData_2026-06-24`
- `\\DESKTOP-69817L3\ArkData\NarrativeCinematicFeaturePack\main_pc_3d_cinematic_data_bank\first_cutscene_workshop`
- `C:\ThePhantomKnowledge-1.0.0f3\ThePhantomKnowledge-1.0.0f3`

Do not treat these as raw asset import permission. Treat them as shot grammar, command-order evidence, timing envelopes, camera distribution references, tutorial trigger schemas, and production-shape evidence.

## Primary Actor Baseline

Use `Inori_MagicaCloth2_Costume1` as the main protagonist model for cutscene planning and implementation.

Current relevant assets:

- source prefab: `Assets/_Imported/AssetStore/RoloArt/Inori/Prefabs/Inori_MagicaCloth2_Costume1.prefab`
- promoted model: `Assets/_Game/Art/Characters/Player/Inori/Models/Inori_Unity.fbx`
- promoted materials: `Assets/_Game/Art/Characters/Player/Inori/Materials`
- promoted textures: `Assets/_Game/Art/Characters/Player/Inori/Textures`
- gameplay Animator Controller: `Assets/_Game/Art/Animations/Player/Inori/DB_Inori_Rifle_ActionFoundation.controller`
- face Animator Controller: `Assets/_Imported/AssetStore/RoloArt/Inori/FaceAnimations/Inorianim.controller`
- face expression clips: `Assets/_Imported/AssetStore/RoloArt/Inori/FaceAnimations/FaceExpressions`
- Inori asset promotion helper: `Assets/_Game/Editor/ActionFoundationInoriPlayerVisualAssetSetup.cs`
- rifle pose tuning profile: `Assets/_Game/DesignData/Profiles/ActionFoundation/DB_InoriRiflePoseTuning.asset`

Inori can support facial expression changes. Use that for story beats, QTE reactions, ultimate focus, danger cues, and short dialogue/reaction cutscenes instead of static talking-head presentation.

For build-facing content, prefer promoted `_Game` assets. If source face-expression clips or controllers are needed, promote or wrap them through an explicit reviewed setup instead of leaving final production profiles bound directly to `_Imported` paths.

Animation sourcing should target Inori first. Retargeted humanoid clips are useful only if they read correctly on Inori with costume cloth, weapon sockets, facial expression overlays, and gameplay camera scale.

## Current Asset Intake Snapshot

### Imported Animation Bundle

`KAWAII_ANIMATIONS_100` was moved under:

- `Assets/_Imported/AssetStore/KAWAII_ANIMATIONS_100`

Current observed contents:

- 332 `.FBX` animation files
- 35 demo Animator Controllers
- 380 `.meta` files
- demo scene/material/lighting/support assets

Sample import evidence:

- `@KA_Idle31_PickUp.FBX.meta` has `importAnimation: 1`, `animationType: 3`, and `avatarSetup: 2`.
- This means the bundle is a plausible humanoid retarget source, but every promoted clip still needs Inori inspection.

High-value candidate families:

- Movement: `Walk`, `Run`, `Dash`, `Jump`, `SuddenStop`, turn/start/stop variants
- Story reaction: `LookLeftAndRight`, `LookAtHands`, `LookAtFeet`, `LookingBack`, `Surprised`, `Angry`, `Cry`, `Shout`, `FingerSnap`
- Interaction: `PickUp`, `LeanAgainst`, sit/start/end variants
- Combat: `Combat_OHSword01`, `Combat_HeavySword`, `Combat_BareHands_Damage`, `Combat_Witch_Magic`
- Death/fail: `Death_Ground`

Guardrail-sensitive or low-priority families:

- `Kiss`, `Hug`, overly cute/shy/glamour poses, and dance-heavy loops should not drive the main cutscene language.
- These may be used only if a later scene has a clear non-glamour narrative reason and passes the cinematic quality guardrails.
- Baby/crawling/swimming/sleeping families are not P0 unless a specific scene needs them.

### Inori Foundation Findings

Observed from local assets:

- promoted Inori model has `importAnimation: 0`, `animationType: 3`, `avatarSetup: 1`, and `importBlendShapes: 1`
- source `Inori_MagicaCloth2_Costume1` prefab has an Animator and Magica Cloth objects/colliders
- source prefab Animator Controller is empty, so gameplay/cutscene controllers must be assigned by our setup
- Inori face clips animate `Body` blend shapes such as surprised brows, mouth shape, eye width, and jaw open
- face controller includes states such as `Surprised`, `Wink_R`, `Wink_L`, `Sad`, `Angry`, `Squint`, `CalmEye`, `Confused`, `Smile`, `Pout`, `BlushHeavy`, and `Joy`
- current `_Game` Inori controller is rifle-focused and contains states such as `R_Idle`, `R_Walk`, `R_Run`, `R_AimIdle`, `R_AimWalk*`, `R_Shoot`, and `R_Evade`

Immediate implication:

- Inori is viable as the primary cutscene actor.
- The next quality gate is not "find any animation"; it is "retarget and inspect chosen animation on Inori with face expression, cloth, weapon sockets, VFX, and gameplay camera scale."

## Initial P0 Animation Gap List

### `IntroAwakening`

Needed Inori clips:

- wake/limited-body motion inside capsule
- hand brace against capsule/glass
- step out or rise from capsule
- confused scan using face expression
- regain balance or stumble recovery
- practical sword pickup
- practical gun pickup
- combat-ready hold

Likely candidates:

- `@KA_Idle03_LookAtHands`
- `@KA_Idle04_LookAtFeet`
- `@KA_Idle11_LookingBack`
- `@KA_Idle17_StumbleAndFall`
- `@KA_Idle29_Surprised`
- `@KA_Idle31_PickUp`
- `@KA_Combat_OHSword01_Idle01`
- existing rifle `R_AimIdle` / `R_Evade` / `R_Run`

Known gaps:

- capsule-specific wake and exit probably need authored composition, partial-body masking, or external clip support
- weapon pickup must be checked on Inori hand sockets

### `GameplayHandoff`

Needed Inori clips:

- back-view combat-ready idle
- short settle into playable stance
- directional turn toward enemy

Likely candidates:

- existing rifle `R_Idle`, `R_AimIdle`, `R_Run`
- `@KA_Combat_OHSword01_Idle01`
- `@KA_Run*_Stop` or `@KA_SuddenStop*`

Known gaps:

- exact final pose must match gameplay camera and input state, not just look good in isolation

### `QTEAssist`

Needed clips:

- assist entry
- short landing or dash-in
- support hit/cast
- recovery and exit

Likely candidates:

- `@KA_Dash_Fwd`
- `@KA_Dash_Left`
- `@KA_Dash_Right`
- `@KA_Combat_Witch_Magic_Shot`
- `@KA_Combat_Witch_Magic_Impact`
- `@KA_Combat_BareHands_Combo*`

Known gaps:

- QTE needs actor entry timing, VFX, UI prompt, hit timing, and cleanup in one authored sequence

### `UltimateCutIn`

Needed clips:

- charge/focus
- face expression
- decisive cast/slash/fire
- impact hold
- recovery

Likely candidates:

- `@KA_Combat_Witch_Magic_Awakening`
- `@KA_Combat_Witch_Magic_Shot`
- `@KA_Combat_Witch_Magic_Impact`
- `@KA_Combat_OHSword01_ChargeAttack`
- `@KA_Combat_HeavySword_ChargeAttack`
- face clips `Angry`, `Surprised`, `CalmEye`

Known gaps:

- ultimate quality depends on VFX, camera impulse, time control, and hit timing as much as the body clip

### `DangerCue`

Needed clips:

- quick look to threat
- flinch or brace
- short dodge/readiness response

Likely candidates:

- `@KA_Idle02_LookLeftAndRight`
- `@KA_Idle11_LookingBack`
- `@KA_Combat_BareHands_Damage*`
- existing `R_Evade`
- face clips `Surprised`, `Angry`, `Confused`

Known gaps:

- cue must preserve threat direction and should not steal too much control

### `CombatTutorialOverlay`

Needed clips:

- basic attack/readiness stance
- skill/cast sample
- QTE prompt state
- ultimate ready state

Likely candidates:

- existing rifle controller states
- `@KA_Combat_Witch_Magic_*`
- `@KA_Combat_OHSword01_*`

Known gaps:

- overlay must remain readable without tiny labels; TutorialSystem should inform mask/prompt behavior, not force PGR UI hierarchy

## Useful Evidence

### Cutscene Pattern Apply Data

Key files:

- `normalized/cutscene_pattern_runtime_manifest.json`
- `normalized/first_cutscene_pattern_overlay.json`
- `normalized/pgr_command_sequence_patterns.csv`
- `normalized/pgr_actor_dialogue_animation_patterns.csv`
- `normalized/pgr_camera_shot_sequence_patterns.csv`
- `normalized/pgr_presentation_effect_patterns.csv`
- `docs/runtime_contract.md`
- `docs/main_pc_integration.md`
- `docs/data_quality_report.md`

Observed coverage:

- 8 scene proxy sequences
- 318 command rows
- 93 command sequence patterns
- 128 actor/dialogue/animation patterns
- 48 camera shot sequence patterns
- 13 animation vocabulary mapping rows
- 4 presentation effect patterns
- 8 first-cutscene overlay beats

High-value pattern types:

- `scene_camera_bootstrap`
- `actor_spawn_to_state`
- `emotion_before_dialogue`
- `dialogue_to_reaction`
- `camera_reframe_before_line`
- `line_to_camera_reframe`
- `camera_actor_state_pair`
- `black-screen-transition-helper`
- `screen-effect-lifecycle`

### Tutorial System Apply Data

Key files:

- `normalized/tutorial_runtime_manifest.json`
- `normalized/novice_stage_guides.csv`
- `normalized/guide_groups.csv`
- `normalized/ui_guide_chains.csv`
- `normalized/ui_guide_steps.csv`
- `normalized/teaching_input_rule_groups.csv`
- `normalized/fight_guide_step_bank_curated.csv`
- `normalized_enhanced/enhanced_manifest.json`
- `normalized_enhanced/condition_id_usage.csv`
- `normalized_enhanced/condition_runtime_handler_index.csv`
- `normalized_enhanced/fight_overlay_family_rollup.csv`
- `normalized_enhanced/overlay_sample_step_links.csv`
- `normalized_enhanced/statussyncfight_fightguide_dlc_steps.csv`
- `normalized_enhanced/teaching_skill_semantic_links.csv`

Observed coverage:

- 45 UI guide chains
- 239 UI guide steps
- 772 guide trigger rows
- 4 novice stage guide gates
- 48 teaching activities
- 283 teaching stage rows
- 1125 combat input detector groups
- 2052 curated in-fight guide rows
- 536 condition id usage rows
- 133 runtime condition handler rows
- 189 fight overlay family rollup rows
- 563 overlay sample step links
- 91 StatusSyncFight DLC guide steps
- 3776 teaching skill semantic links
- 2416 asset reference inventory rows

High-value tutorial/combat families:

- basic attack prompt
- skill orb prompt
- character switch prompt
- QTE prompt
- mask/click target prompt
- timed combat guide line
- stage open gate
- condition-gated guide dispatch

### Unity-chan Timeline Reference

Use the previous Unity-chan project as a Timeline/Cinemachine composition reference, not as a direct final art target.

Useful known values:

- main Timeline length around 116.923 seconds
- 234 Timeline tracks
- 51 cameras
- 22 playable assets
- 164 FBX assets
- 57 WAV files
- 614 materials

Use it for:

- camera count density
- track layering
- dialogue/action/camera distribution
- cut and blend rhythm
- Timeline binding organization
- evidence that a scene can hold many authored cameras without becoming unreadable

Do not use it to:

- copy character identity as our production direction
- import restricted raw assets without license review
- replace our combat-facing ARPG camera identity

## Package Scope

The build resubmission package should include reusable cutscene modules, not one isolated scene.

This plan applies to every cutscene category that may appear in the build:

- main story opening and story transitions
- in-game dialogue and reaction beats
- combat intro and gameplay handoff
- QTE and assist entry
- ultimate and special attack cut-ins
- boss, elite, and phase-transition presentations
- danger warnings, break moments, and stagger emphasis
- tutorial prompt moments that temporarily steer attention
- stage clear, failure, reward, or route-complete bridges
- summon entry and summon impact presentations

Implementation can start with a few representative modules, but the architecture and quality rules must assume all cutscenes, not only the opening.

### P0 Modules

These are required first because they cover the widest set of cutscene needs:

- `IntroAwakening`: 30-45 second opening cinematic based on the 8-beat first cutscene draft.
- `GameplayHandoff`: cinematic camera aligns into the actual combat back-view camera with no visible snap.
- `QTEAssist`: short assist-entry or tag-in moment with UI prompt, actor entry, hit or support effect, and immediate gameplay return.
- `UltimateCutIn`: short high-impact ultimate shot with input lock, actor pose/action, VFX, hit stop, damage timing, and cleanup.
- `CombatTutorialOverlay`: early combat guidance using mask/click/prompt concepts from TutorialSystem, adapted to our UI.
- `DangerCue`: enemy/boss telegraph emphasis with camera impulse, warning UI, and readable threat direction.

### P1 Modules

These should follow after the P0 package is inspectable:

- `BossIntro`: boss or elite entrance with spatial relation and readable combat purpose.
- `PhaseTransition`: short boss/elite phase shift, screen effect, black-screen or impact transition, and return to camera.
- `BreakMoment`: enemy stagger, shield break, or vibration-break style emphasis.
- `FinalKillBridge`: final hit to result or route-complete bridge.

### P2 Modules

These are useful but should not block the first implementation pass:

- `DialogueReactionBeat`: short actor state before/after line.
- `SummonEntryVariant`: alternate summon entry camera variants.
- `RewardUnlockMoment`: small post-combat presentation beat.

## First Cutscene Baseline

The first cinematic uses the workshop draft:

- target duration: 39 seconds
- allowed duration: 30-45 seconds
- final beat must end on gameplay camera alignment

Beat order:

1. `beat_01_capsule_wakeup_first_person`
2. `beat_02_system_warning_scan`
3. `beat_03_capsule_open_body_reveal`
4. `beat_04_heaven_collapse_establishing`
5. `beat_05_sword_pickup`
6. `beat_06_gun_pickup`
7. `beat_07_enemy_standoff`
8. `beat_08_gameplay_backview_takeover`

This sequence is the first impression and the first vertical integration test, but it must also seed reusable camera, VFX, actor, input-lock, and gameplay-return logic for every other cutscene module.

## Cinematic Quality Rules

Follow `cinematic_quality_guardrails.md` from the first-cutscene workshop.

Hard bans:

- body-scan camera movement
- glamour close-ups
- meaningless slow orbit
- camera-aware posing
- sensual breathing or soft voice framing
- bedroom/stage-show lighting language
- weapon handling that reads as caressing
- long helpless posing
- VN-style dialogue-window dominance

Every shot must answer at least one of these purposes:

- provide new information
- show an action
- clarify threat direction
- connect to weapon/system mechanics
- bridge into gameplay camera

If a shot has no purpose, delete it or merge it into another shot.

## Expected Quality Ceiling

With current project assets only:

- camera and transition quality: medium-high
- structure quality: high
- animation quality: medium
- VFX and impact quality: medium
- final perceived polish: medium

With allowed external animation/model/VFX sourcing and proper promotion into `_Game`:

- camera and transition quality: high
- structure quality: high
- animation quality: medium-high
- VFX and impact quality: medium-high
- final perceived polish: high enough for build resubmission review

The main quality bottleneck is not data anymore. The main bottleneck is actor presentation: rigged model, animation clips, weapon attachment, VFX, SFX, and cleanup.

## Resource Policy

Do not leave final review scenes as capsules, static primitives, or disconnected weapons.

Allowed production direction:

- use already promoted project assets where possible
- source lawful external animation/model resources when current assets are not enough
- possible sourcing routes include Mixamo, Meshy, probe/prototype asset routes, and owned asset-store packages
- retarget humanoid clips where needed
- promote reviewed assets into `_Game` before production use
- bind assets through serialized references or data profiles

Do not:

- reference raw `_Imported` assets directly from runtime profiles
- hardcode external paths
- import proprietary clips, meshes, textures, audio, or camera files from other games
- claim exact PGR or Unity-chan Timeline restoration
- hide missing actor presentation behind capsules in final review modules

## Implementation Shape

Prefer narrow owners and data-backed modules.

Recommended runtime/editor pieces:

- `CinematicSequenceProfile`: data-only module definition for all authored cutscene categories, including intro, story beat, QTE, ultimate, boss intro, phase transition, result bridge, summon entry, and tutorial prompt sequences.
- `CinematicCameraCueProfile`: data-only shot/blend/FOV/target/impulse settings.
- `CinematicActorCueProfile`: data-only actor animation, facing, weapon attach, entry/exit, and pose settings.
- `CinematicVfxCueProfile`: data-only effect, flash, black-screen, screen-space warning, hit-stop, and cleanup settings.
- `CinematicSequenceRunner`: narrow runtime player for one authored sequence.
- `CinematicGameplayHandoff`: narrow owner for input lock, camera restore, time restore, UI restore, and gameplay camera match.
- `CombatTutorialCueProfile`: data-only prompt/mask/click/highlight mapping adapted from TutorialSystem concepts.

Keep ownership boundaries aligned with `ACTION_FOUNDATION_OWNERSHIP.md`.

The sequence runner must not become:

- a boss phase manager
- a full tutorial system
- an encounter spawner
- a reward/progression owner
- a broad UI constructor
- an importer or runtime asset loader

## Build Resubmission Acceptance Checks

The package is not acceptable until these checks pass:

- No cutscene reads as a cheap body-focused cutscene.
- The first cutscene ends in the actual playable back-view combat camera.
- QTE/assist has visible actor entry, readable target/action, and clean input return.
- Ultimate cut-in has clear startup, impact timing, damage/VFX cue, and cleanup.
- Tutorial overlay text is readable at gameplay distance and does not require tiny labels.
- At least one combat prompt uses a TutorialSystem-inspired mask/click/guide concept.
- No final module uses capsules as the primary actor representation.
- Story/dialogue cutscenes include actor state or reaction changes instead of static talking heads.
- Boss/elite cutscenes preserve spatial relation and threat direction.
- Result/clear/failure bridges resolve back to gameplay or UI without dirty camera, input, or time state.
- Weapons move with the actor and are attached to clear hand/socket anchors.
- Camera locks, input locks, time scale, camera priority, UI visibility, and VFX objects clean up after playback.
- All imported/promoted resources have an inspectable reason and are not raw proprietary game data.

## Immediate Production Order

1. Create the reusable cinematic package folder and data profile structure.
2. Lock Inori as the primary protagonist actor and verify promoted model, humanoid Avatar, materials, cloth setup, face expression clips, and hand/weapon attachment sockets.
3. Define the full cutscene category catalog so later work does not narrow back to only the opening.
4. Build the Inori animation gap list before collecting external animation clips.
5. Collect or promote only the missing animation clips needed by the P0 modules, then retarget and inspect them on Inori.
6. Build `IntroAwakening` as the first end-to-end sequence because it exercises many shared systems.
7. Replace primitive actor placeholders with Inori, working facial expressions, and weapon-bearing animation before judging camera quality.
8. Build `GameplayHandoff` and verify the final camera matches gameplay camera transform/FOV.
9. Build `QTEAssist` using TutorialSystem prompt and overlay concepts.
10. Build `UltimateCutIn` using short lock, camera impulse, hit-stop, VFX, and cleanup.
11. Build `DangerCue` so boss/elite threats can be emphasized without a full cutscene.
12. Add `CombatTutorialOverlay` for basic attack, skill, QTE, and ultimate prompts.
13. Add `BossIntro`, `PhaseTransition`, `DialogueReactionBeat`, and `ResultBridge` as the next coverage set.

## Notes For Future Codex Sessions

Do not narrow this plan back to only the opening cutscene or only early-game presentation.

The opening cutscene is a vertical integration test. The real deliverable is a build-resubmission cutscene package that can support all authored story, combat, tutorial, QTE, ultimate, boss, summon, transition, and result moments.

When in doubt, use the data hierarchy in this order:

1. Existing project ownership and runtime rules.
2. `cinematic_quality_guardrails.md`.
3. `first_cutscene_engine_input_draft.json`.
4. `first_cutscene_pattern_overlay.json`.
5. PGR command/camera/actor/effect pattern CSVs.
6. TutorialSystem guide/overlay/input detector CSVs.
7. Unity-chan Timeline only as Timeline/camera-density composition reference.

The strongest next step is not more abstract planning. The strongest next step is a small but complete package implementation with real animated actors, weapon sockets, VFX, readable prompts, and deterministic cleanup.
