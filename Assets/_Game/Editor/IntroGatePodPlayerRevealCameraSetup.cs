using System;
using System.Collections.Generic;
using System.IO;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor
{
    public static class IntroGatePodPlayerRevealCameraSetup
    {
        private const string OlympusStageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string OlympusCombinedTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string OlympusCombinedProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening_OlympusBombingPrelude.asset";
        private const string StageDefinitionProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string RinaCameraClipPath =
            "Assets/_Imported/Reference/ZZZ_RinaLoopKit/Generated/Rina_QuestStart_OriginalExtracted.anim";
        private const string CombatStartReadyEnterClipPath =
            "Assets/_Game/Art/Animations/Cinematics/Inori/KawaiiP0/CIN_BossIntroReady.fbx";
        private const string CombatReadyClipPath =
            "Assets/_Game/Art/Animations/Cinematics/Inori/KawaiiP0/CIN_CombatReady.fbx";
        private const string RevealRigRootName = "IntroGatePodReview_PlayerRevealCameraRig";
        private const string RevealCameraName = "CM_03_src_c10_player_reveal_rina_quest_start";
        private const string RevealShotId = "src_c10_player_reveal_rina_quest_start";
        private const string RevealAnimationTrackName = "Player Reveal Rina Camera Motion";
        private const string ObsoleteRevealPlacementTrackName = "Player Reveal PodBase Placement";
        private const string ObsoleteCombatStartPlacementTrackName = "Player Combat Start PodBase Placement";
        private const string CombatStartVisualPlacementName = "IntroGatePodReview_CombatStartInoriPlacement";
        private const string CombatStartVisualActivationTrackName = "Combat Start Inori Active";
        private const string CombatStartVisualBodyTrackName = "Combat Start Inori Body";
        private const string InoriBodyTrackName = "Inori Body";
        private const string CombatStartReadyEnterClipName = "combat_start_ready_enter";
        private const string CombatStartReadySettleClipName = "combat_start_ready_settle";
        private const string CombatStartReadyEnterCueId = "combat_start_ready_enter";
        private const string CombatStartReadySettleCueId = "combat_start_ready_settle";
        private const string CombatStartReadyEnterStateName = "CIN_BossIntroReady";
        private const string CombatReadyStateName = "CIN_CombatReady";
        private const string CinemachineTrackName = "Cinemachine Shots";
        private const string BombingCinemachineTrackName = "Bombing Prelude Cinemachine Shots";
        private const string CombatReadyActorCueId = "combat_ready_handoff";
        private const string CombatReadyTimelineClipName = "combat_ready_handoff";
        private const string LastCommandoShotId = "src_c09_commando_bridge_push_past";
        private const string InoriObjectName = "IntroGatePodReview_Inori";
        private const string InoriPlacementObjectName = "IntroGatePodReview_InoriPlacement";
        private const string PodBaseReadabilityObjectName = "IntroGatePodReview_PodBaseReadability";
        private const string CorridorFacingAnchorName = "IntroCutscene_End_PlayerHandoffAnchor";
        private const string ShotPlayerObjectName = "IntroGatePodReview_CinemachineShotPlayer";
        private const string PlayerCameraAnchorName = "Player_LeftShoulderCameraAnchor";
        private const string GameplayCombatStartAnchorName = "Gameplay_CombatStartAnchor";
        private const string StageSpawnerPlayerStartName = "StageSpawner_PlayerStart";
        private const string GameplayHandoffPortName = "GameplayHandoffPort";
        private const float OlympusCorridorGameplayYawDegrees = 90f;
        private const string ReportPath = "C:/tmp/DimensionBrawl-IntroGatePodPlayerRevealCamera.md";

        private const double RevealDurationSeconds = 7.5000005d;
        private const double TimelineTailSeconds = 0.65d;
        private const double CombatStartReadyEnterDurationSeconds = 1.25d;
        private const double BombingPreludeEndSeconds = 8.50d;
        private const double CameraTransitionToleranceSeconds = 0.002d;
        private const double FirstPersonPlacementValidationSeconds = 18.5d;
        private const float HandoffCameraHeight = 1.8f;
        private const float RevealFieldOfView = 60.001953f;
        private const float ViewportMargin = 0.035f;

        private static readonly string[] MainShotIds =
        {
            "src_c01_capsule_left_dolly",
            "src_c03_first_person_eye_open",
            "src_c04_first_person_scan_left",
            "src_c05_first_person_scan_right",
            "src_c06_first_person_look_down_hands",
            "src_c07_commando_bridge_legs_run",
            "src_c08_heaven_background_explosion",
            "src_c09_commando_bridge_push_past",
            RevealShotId
        };

        private static readonly double[] MainIncomingBlendSeconds =
        {
            0d,
            0d,
            1d,
            1d,
            1d,
            0d,
            1d,
            0.65d,
            0.42d
        };

        private static readonly CameraTransitionSpec[] BombingShotContract =
        {
            new CameraTransitionSpec("cm_01_formation_join", 0d, 3.72d, 0d),
            new CameraTransitionSpec("cm_02_bomb_release", 3.72d, 1.12d, 0.16d),
            new CameraTransitionSpec("cm_03_falling_payload", 4.84d, 0.72d, 0.10d),
            new CameraTransitionSpec("cm_04_target_reframe", 5.56d, 0.36d, 0.30d),
            new CameraTransitionSpec("cm_05_impact_chain", 5.92d, 1.18d, 0.10d),
            new CameraTransitionSpec("cm_06_aftershock", 7.10d, 0.96d, 0.10d),
            new CameraTransitionSpec("cm_07_smoke_handoff", 8.06d, 0.44d, 0.10d)
        };

        [MenuItem("Tools/DimensionBrawl/Intro GatePod/Setup Player Reveal Camera")]
        public static void SetupPlayerRevealCameraMenu()
        {
            SetupPlayerRevealCamera(writeReport: true);
        }

        public static void RunBatchSetupPlayerRevealCamera()
        {
            SetupPlayerRevealCamera(writeReport: true);
        }

        private static void SetupPlayerRevealCamera(bool writeReport)
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(OlympusStageScenePath, OpenSceneMode.Single);
            TimelineAsset timeline = LoadRequired<TimelineAsset>(OlympusCombinedTimelinePath);
            AnimationClip rinaClip = LoadRequired<AnimationClip>(RinaCameraClipPath);
            AnimationClip combatStartReadyEnterClip = LoadAnimationClip(CombatStartReadyEnterClipPath, CombatStartReadyEnterStateName);
            AnimationClip combatReadyClip = LoadAnimationClip(CombatReadyClipPath, CombatReadyStateName);
            CinematicSequenceProfile profile = LoadRequired<CinematicSequenceProfile>(OlympusCombinedProfilePath);
            StageDefinitionProfile stageDefinition = LoadRequired<StageDefinitionProfile>(StageDefinitionProfilePath);
            PlayableDirector director = FindDirectorBoundToTimeline(scene, timeline)
                ?? throw new InvalidOperationException("Could not find the Olympus combined Timeline director.");
            Transform inori = RequireObjectInScene(scene, InoriObjectName).transform;
            Transform inoriPlacement = RequireObjectInScene(scene, InoriPlacementObjectName).transform;
            Transform podBaseReadability = RequireObjectInScene(scene, PodBaseReadabilityObjectName).transform;
            Transform corridorFacingAnchor = RequireObjectInScene(scene, CorridorFacingAnchorName).transform;
            CinemachineBrain brain = FindComponentInScene<CinemachineBrain>(scene)
                ?? throw new InvalidOperationException("Could not find CinemachineBrain in Olympus scene.");
            Animator inoriBodyAnimator = inori.GetComponentInChildren<Animator>(includeInactive: true)
                ?? throw new InvalidOperationException($"`{InoriObjectName}` is missing a body Animator.");

            // c09 deliberately extends past its nominal end to cover the c10 blend.  The next
            // setup run must keep using the profile beat, not that outgoing overlap tail.
            double revealStartSeconds = FindCameraCueEnd(profile, LastCommandoShotId);
            double revealEndSeconds = revealStartSeconds + RevealDurationSeconds;
            double authoredEndSeconds = revealEndSeconds + TimelineTailSeconds;

            Vector3 revealFootOrigin = podBaseReadability.position;
            Quaternion revealFacingRotation = Quaternion.Euler(0f, OlympusCorridorGameplayYawDegrees, 0f);
            Vector3 handoffCameraPosition = revealFootOrigin + (Vector3.up * HandoffCameraHeight);
            TransformSnapshot originalInoriPlacement = TransformSnapshot.Capture(inoriPlacement);
            Transform combatStartVisualPlacement = EnsureCombatStartVisual(
                scene,
                inoriPlacement,
                inori,
                revealFootOrigin,
                revealFacingRotation);
            Transform combatStartVisualInori = FindChildRecursive(combatStartVisualPlacement, InoriObjectName)
                ?? throw new InvalidOperationException($"`{CombatStartVisualPlacementName}` is missing `{InoriObjectName}`.");
            Animator combatStartVisualAnimator = combatStartVisualInori.GetComponent<Animator>()
                ?? combatStartVisualInori.GetComponentInChildren<Animator>(includeInactive: true)
                ?? throw new InvalidOperationException($"`{CombatStartVisualPlacementName}` is missing an Animator.");

            ApplyCombatStartAnchors(scene, revealFootOrigin, handoffCameraPosition, revealFacingRotation);
            UpdateStageDefinitionContract(stageDefinition, revealFootOrigin, handoffCameraPosition, revealFacingRotation.eulerAngles);

            Transform revealRoot = EnsureRevealRoot(scene);
            CinemachineCamera revealCamera = EnsureRevealCamera(revealRoot);
            Animator revealAnimator = revealCamera.GetComponent<Animator>();

            PositionRevealRigForPlayer(revealRoot, revealCamera.transform, revealFootOrigin, revealFacingRotation, rinaClip);
            ConfigureRevealCamera(revealCamera);
            AddOrUpdateCinemachineClip(timeline, director, brain, revealCamera, revealStartSeconds);
            AddOrUpdateAnimationTrack(timeline, director, revealAnimator, rinaClip, revealStartSeconds);
            RemoveTimelineTrack(timeline, ObsoleteRevealPlacementTrackName, director);
            RemoveTimelineTrack(timeline, ObsoleteCombatStartPlacementTrackName, director);
            AddOrUpdateActivationTrack(
                timeline,
                director,
                CombatStartVisualActivationTrackName,
                combatStartVisualPlacement.gameObject,
                revealStartSeconds,
                RevealDurationSeconds + TimelineTailSeconds);
            AddOrUpdateCombatStartVisualBodyClips(
                timeline,
                director,
                combatStartVisualAnimator,
                combatReadyClip,
                combatStartReadyEnterClip,
                revealStartSeconds,
                revealEndSeconds);
            UpdateShotPlayer(scene, brain, revealCamera, revealStartSeconds);
            ExtendLetterboxTimelineClip(timeline, authoredEndSeconds);
            UpdateProfile(profile, revealStartSeconds, revealEndSeconds, authoredEndSeconds);
            ApplyCinemachineTransitionContract(timeline, profile);

            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = authoredEndSeconds;

            List<string> issues = ValidateCameraAnchor(revealRoot, podBaseReadability.position, revealFacingRotation);
            ValidateCombatStartPlacement(
                issues,
                director,
                inoriPlacement,
                combatStartVisualInori,
                originalInoriPlacement,
                revealFootOrigin,
                revealStartSeconds);
            originalInoriPlacement.Apply(inoriPlacement);
            rinaClip.SampleAnimation(revealCamera.gameObject, 0f);
            if (Math.Abs(timeline.fixedDuration - authoredEndSeconds) > 0.01d)
            {
                issues.Add($"Timeline fixed duration is {timeline.fixedDuration:0.###}, expected {authoredEndSeconds:0.###}.");
            }

            EditorUtility.SetDirty(revealRoot.gameObject);
            EditorUtility.SetDirty(revealCamera);
            EditorUtility.SetDirty(revealAnimator);
            EditorUtility.SetDirty(combatStartVisualPlacement.gameObject);
            EditorUtility.SetDirty(combatStartVisualAnimator);
            EditorUtility.SetDirty(inoriBodyAnimator);
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(profile);
            EditorUtility.SetDirty(stageDefinition);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (writeReport)
            {
                WriteReport(
                    issues,
                    revealStartSeconds,
                    revealEndSeconds,
                    authoredEndSeconds,
                    revealRoot,
                    podBaseReadability.position,
                    revealFacingRotation.eulerAngles);
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Intro GatePod player reveal camera setup failed:\n" + string.Join("\n", issues));
            }
        }

        internal static void ApplyCinemachineTransitionContract(
            TimelineAsset timeline,
            CinematicSequenceProfile profile)
        {
            if (timeline == null)
            {
                throw new ArgumentNullException(nameof(timeline));
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            List<string> buildIssues = new List<string>();
            CameraTransitionSpec[] mainContract = BuildMainShotContract(profile, buildIssues);
            if (buildIssues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Could not build the intro Cinemachine transition contract:\n"
                    + string.Join("\n", buildIssues));
            }

            ApplyTrackTransitionContract(
                timeline,
                CinemachineTrackName,
                mainContract,
                TimelineTailSeconds);
            ApplyTrackTransitionContract(
                timeline,
                BombingCinemachineTrackName,
                BombingShotContract,
                0d);
            EditorUtility.SetDirty(timeline);
        }

        public static IReadOnlyList<string> CollectCinemachineTransitionContractIssues(
            TimelineAsset timeline,
            CinematicSequenceProfile profile)
        {
            List<string> issues = new List<string>();
            if (timeline == null)
            {
                issues.Add("The combined intro Timeline is missing.");
                return issues;
            }

            if (profile == null)
            {
                issues.Add("The combined intro cinematic profile is missing.");
                return issues;
            }

            CameraTransitionSpec[] mainContract = BuildMainShotContract(profile, issues);
            if (mainContract.Length == MainShotIds.Length)
            {
                ValidateTrackTransitionContract(
                    timeline,
                    CinemachineTrackName,
                    mainContract,
                    TimelineTailSeconds,
                    issues);
            }

            ValidateTrackTransitionContract(
                timeline,
                BombingCinemachineTrackName,
                BombingShotContract,
                0d,
                issues);
            return issues;
        }

        private static CameraTransitionSpec[] BuildMainShotContract(
            CinematicSequenceProfile profile,
            List<string> issues)
        {
            List<CameraTransitionSpec> contract = new List<CameraTransitionSpec>(MainShotIds.Length);
            CinematicSequenceProfile.CameraCue[] cues = profile.CameraCues;
            for (int shotIndex = 0; shotIndex < MainShotIds.Length; shotIndex++)
            {
                string shotId = MainShotIds[shotIndex];
                int matchCount = 0;
                CinematicSequenceProfile.CameraCue cue = default;
                for (int cueIndex = 0; cueIndex < cues.Length; cueIndex++)
                {
                    if (!string.Equals(cues[cueIndex].CueId, shotId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    cue = cues[cueIndex];
                    matchCount++;
                }

                if (matchCount != 1)
                {
                    issues.Add(
                        $"Main intro profile must author `{shotId}` exactly once; found {matchCount}.");
                    continue;
                }

                contract.Add(new CameraTransitionSpec(
                    shotId,
                    cue.StartSeconds,
                    cue.DurationSeconds,
                    MainIncomingBlendSeconds[shotIndex]));
            }

            if (contract.Count == MainShotIds.Length)
            {
                if (Math.Abs(contract[0].NominalStartSeconds - BombingPreludeEndSeconds)
                    > CameraTransitionToleranceSeconds)
                {
                    issues.Add(
                        $"Main intro must begin at the {BombingPreludeEndSeconds:0.###}s bombing handoff; "
                        + $"found {contract[0].NominalStartSeconds:0.###}s.");
                }

                for (int i = 1; i < contract.Count; i++)
                {
                    double previousNominalEnd = contract[i - 1].NominalStartSeconds
                        + contract[i - 1].NominalDurationSeconds;
                    if (Math.Abs(contract[i].NominalStartSeconds - previousNominalEnd)
                        <= CameraTransitionToleranceSeconds)
                    {
                        continue;
                    }

                    issues.Add(
                        $"Profile shot `{contract[i].ShotId}` begins at "
                        + $"{contract[i].NominalStartSeconds:0.###}s instead of the previous nominal "
                        + $"beat end {previousNominalEnd:0.###}s.");
                }
            }

            return contract.ToArray();
        }

        private static void ApplyTrackTransitionContract(
            TimelineAsset timeline,
            string trackName,
            IReadOnlyList<CameraTransitionSpec> contract,
            double finalTailSeconds)
        {
            CinemachineTrack track = FindTimelineTrack<CinemachineTrack>(timeline, trackName)
                ?? throw new InvalidOperationException($"Timeline is missing `{trackName}`.");
            if (CountTimelineTracks<CinemachineTrack>(timeline, trackName) != 1)
            {
                throw new InvalidOperationException(
                    $"Timeline must contain exactly one `{trackName}` track.");
            }

            if (CountClips(track) != contract.Count)
            {
                throw new InvalidOperationException(
                    $"`{trackName}` must contain exactly {contract.Count} camera clips.");
            }

            TimelineClip[] clips = new TimelineClip[contract.Count];
            for (int i = 0; i < contract.Count; i++)
            {
                clips[i] = FindUniqueClip(track, contract[i].ShotId);
                clips[i].start = contract[i].NominalStartSeconds;
                clips[i].duration = ResolveAuthoredClipDuration(contract, i, finalTailSeconds);
                SetTimelineClipExtrapolation(clips[i], TimelineClip.ClipExtrapolation.None);
            }

            for (int i = 0; i < contract.Count; i++)
            {
                double incomingBlend = contract[i].IncomingBlendSeconds;
                double outgoingBlend = i + 1 < contract.Count
                    ? contract[i + 1].IncomingBlendSeconds
                    : 0d;
                TimelineClip clip = clips[i];

                // Blends are backed by real clip overlap. Easing an isolated clip fades the
                // Cinemachine track itself from zero and briefly exposes the base c01 camera.
                clip.easeInDuration = 0d;
                clip.easeOutDuration = 0d;
                clip.blendInDuration = incomingBlend;
                clip.blendOutDuration = outgoingBlend;

                if (clip.asset is UnityEngine.Object clipAsset)
                {
                    EditorUtility.SetDirty(clipAsset);
                }
            }

            EditorUtility.SetDirty(track);
        }

        private static void ValidateTrackTransitionContract(
            TimelineAsset timeline,
            string trackName,
            IReadOnlyList<CameraTransitionSpec> contract,
            double finalTailSeconds,
            List<string> issues)
        {
            int trackCount = CountTimelineTracks<CinemachineTrack>(timeline, trackName);
            if (trackCount != 1)
            {
                issues.Add($"Timeline must contain exactly one `{trackName}` track; found {trackCount}.");
                return;
            }

            CinemachineTrack track = FindTimelineTrack<CinemachineTrack>(timeline, trackName);
            int clipCount = CountClips(track);
            if (clipCount != contract.Count)
            {
                issues.Add(
                    $"`{trackName}` must contain exactly {contract.Count} camera clips; found {clipCount}.");
            }

            TimelineClip previous = null;
            for (int i = 0; i < contract.Count; i++)
            {
                CameraTransitionSpec spec = contract[i];
                TimelineClip clip = FindClip(track, spec.ShotId, out int matchCount);
                if (matchCount != 1)
                {
                    issues.Add(
                        $"`{trackName}` must contain `{spec.ShotId}` exactly once; found {matchCount}.");
                    previous = null;
                    continue;
                }

                double expectedDuration = ResolveAuthoredClipDuration(contract, i, finalTailSeconds);
                double expectedOutgoingBlend = i + 1 < contract.Count
                    ? contract[i + 1].IncomingBlendSeconds
                    : 0d;
                AppendTimingIssue(
                    issues,
                    clip.start,
                    spec.NominalStartSeconds,
                    $"`{trackName}/{spec.ShotId}` start");
                AppendTimingIssue(
                    issues,
                    clip.duration,
                    expectedDuration,
                    $"`{trackName}/{spec.ShotId}` duration");
                AppendTimingIssue(
                    issues,
                    clip.blendInDuration,
                    spec.IncomingBlendSeconds,
                    $"`{trackName}/{spec.ShotId}` incoming overlap");
                AppendTimingIssue(
                    issues,
                    clip.blendOutDuration,
                    expectedOutgoingBlend,
                    $"`{trackName}/{spec.ShotId}` outgoing overlap");
                AppendTimingIssue(
                    issues,
                    clip.easeInDuration,
                    0d,
                    $"`{trackName}/{spec.ShotId}` isolated ease-in");
                AppendTimingIssue(
                    issues,
                    clip.easeOutDuration,
                    0d,
                    $"`{trackName}/{spec.ShotId}` isolated ease-out");

                if (previous != null)
                {
                    double actualOverlap = previous.end - clip.start;
                    AppendTimingIssue(
                        issues,
                        actualOverlap,
                        spec.IncomingBlendSeconds,
                        $"`{trackName}` transition into `{spec.ShotId}` actual overlap");
                }

                previous = clip;
            }

            if (previous != null)
            {
                double expectedTrackEnd = contract[contract.Count - 1].NominalStartSeconds
                    + contract[contract.Count - 1].NominalDurationSeconds
                    + finalTailSeconds;
                AppendTimingIssue(
                    issues,
                    previous.end,
                    expectedTrackEnd,
                    $"`{trackName}` final camera coverage");
            }
        }

        private static double ResolveAuthoredClipDuration(
            IReadOnlyList<CameraTransitionSpec> contract,
            int index,
            double finalTailSeconds)
        {
            double outgoingBlend = index + 1 < contract.Count
                ? contract[index + 1].IncomingBlendSeconds
                : finalTailSeconds;
            return contract[index].NominalDurationSeconds + outgoingBlend;
        }

        private static int CountTimelineTracks<T>(TimelineAsset timeline, string trackName)
            where T : TrackAsset
        {
            int count = 0;
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is T && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountClips(TrackAsset track)
        {
            int count = 0;
            foreach (TimelineClip ignored in track.GetClips())
            {
                count++;
            }

            return count;
        }

        private static TimelineClip FindUniqueClip(TrackAsset track, string displayName)
        {
            TimelineClip clip = FindClip(track, displayName, out int matchCount);
            if (matchCount != 1)
            {
                throw new InvalidOperationException(
                    $"`{track.name}` must contain `{displayName}` exactly once; found {matchCount}.");
            }

            return clip;
        }

        private static TimelineClip FindClip(
            TrackAsset track,
            string displayName,
            out int matchCount)
        {
            TimelineClip match = null;
            matchCount = 0;
            foreach (TimelineClip clip in track.GetClips())
            {
                if (!string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                {
                    continue;
                }

                match = clip;
                matchCount++;
            }

            return match;
        }

        private static void AppendTimingIssue(
            List<string> issues,
            double actual,
            double expected,
            string label)
        {
            if (Math.Abs(actual - expected) <= CameraTransitionToleranceSeconds)
            {
                return;
            }

            issues.Add($"{label} is {actual:0.###}s; expected {expected:0.###}s.");
        }

        private static Transform EnsureRevealRoot(Scene scene)
        {
            GameObject existing = FindObjectInScene(scene, RevealRigRootName);
            GameObject rootObject = existing != null ? existing : new GameObject(RevealRigRootName);
            SceneManager.MoveGameObjectToScene(rootObject, scene);
            rootObject.SetActive(true);
            Transform root = rootObject.transform;
            root.localScale = Vector3.one;
            return root;
        }

        private static CinemachineCamera EnsureRevealCamera(Transform revealRoot)
        {
            Transform existing = FindChildRecursive(revealRoot, RevealCameraName);
            GameObject cameraObject = existing != null ? existing.gameObject : new GameObject(RevealCameraName);
            cameraObject.transform.SetParent(revealRoot, worldPositionStays: false);
            cameraObject.SetActive(true);

            CinemachineCamera camera = cameraObject.GetComponent<CinemachineCamera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<CinemachineCamera>();
            }

            Animator animator = cameraObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = cameraObject.AddComponent<Animator>();
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            camera.Priority = 0;
            camera.StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
            return camera;
        }

        private static void ConfigureRevealCamera(CinemachineCamera camera)
        {
            LensSettings lens = LensSettings.Default;
            lens.ModeOverride = LensSettings.OverrideModes.Perspective;
            lens.FieldOfView = RevealFieldOfView;
            lens.NearClipPlane = 0.05f;
            lens.FarClipPlane = 250f;
            camera.Lens = lens;
            camera.Follow = null;
            camera.LookAt = null;
        }

        private static void PositionRevealRigForPlayer(
            Transform revealRoot,
            Transform revealCameraTransform,
            Vector3 playerFootOrigin,
            Quaternion playerFacingRotation,
            AnimationClip rinaClip)
        {
            revealRoot.SetPositionAndRotation(playerFootOrigin, playerFacingRotation);
            rinaClip.SampleAnimation(revealCameraTransform.gameObject, 0f);
        }

        private static float ScoreRevealPlacement(Transform cameraTransform, Transform player, AnimationClip clip)
        {
            Bounds bounds = ResolveRenderableBounds(player);
            PlayerFramePoints points = ResolvePlayerFramePoints(player, bounds);
            using CameraSampler sampler = new CameraSampler(RevealFieldOfView);

            float score = 0f;
            score += ScoreViewportPoint(sampler, cameraTransform, clip, 0.18f, points.Foot, new Vector2(0.50f, 0.34f), 2.5f);
            score += ScoreViewportPoint(sampler, cameraTransform, clip, 3.15f, points.Chest, new Vector2(0.50f, 0.52f), 1.5f);
            score += ScoreViewportPoint(sampler, cameraTransform, clip, 4.75f, points.Head, new Vector2(0.50f, 0.57f), 2.0f);
            score += ScoreBoundsVisible(sampler, cameraTransform, clip, 7.35f, bounds) * 2.5f;
            return score;
        }

        private static float ScoreViewportPoint(
            CameraSampler sampler,
            Transform cameraTransform,
            AnimationClip clip,
            float localTime,
            Vector3 worldPoint,
            Vector2 desiredViewport,
            float weight)
        {
            Vector3 viewport = sampler.Sample(cameraTransform, clip, localTime, worldPoint);
            if (viewport.z <= 0f)
            {
                return -weight * 4f;
            }

            float inside = IsInsideViewport(viewport, ViewportMargin) ? 1f : -1.5f;
            Vector2 delta = new Vector2(viewport.x, viewport.y) - desiredViewport;
            return (inside - delta.magnitude) * weight;
        }

        private static float ScoreBoundsVisible(
            CameraSampler sampler,
            Transform cameraTransform,
            AnimationClip clip,
            float localTime,
            Bounds bounds)
        {
            Vector3[] points = BuildBoundsPoints(bounds);
            int visible = 0;
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 viewport = sampler.Sample(cameraTransform, clip, localTime, points[i]);
                if (viewport.z > 0f && IsInsideViewport(viewport, -0.05f))
                {
                    visible++;
                }
            }

            return visible / (float)points.Length;
        }

        private static void ApplyCombatStartAnchors(
            Scene scene,
            Vector3 playerStartPosition,
            Vector3 cameraAnchorPosition,
            Quaternion facingRotation)
        {
            SetSceneObjectPose(scene, PlayerCameraAnchorName, cameraAnchorPosition, facingRotation);
            SetSceneObjectPose(scene, CorridorFacingAnchorName, cameraAnchorPosition, facingRotation);
            SetSceneObjectPose(scene, GameplayCombatStartAnchorName, playerStartPosition, facingRotation);
            SetSceneObjectPose(scene, StageSpawnerPlayerStartName, playerStartPosition, facingRotation);
            SetSceneObjectPose(scene, GameplayHandoffPortName, playerStartPosition, facingRotation);
        }

        private static void SetSceneObjectPose(Scene scene, string objectName, Vector3 position, Quaternion rotation)
        {
            Transform transform = RequireObjectInScene(scene, objectName).transform;
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one;
            EditorUtility.SetDirty(transform);
            EditorUtility.SetDirty(transform.gameObject);
        }

        private static Transform EnsureCombatStartVisual(
            Scene scene,
            Transform sourcePlacement,
            Transform sourcePlayer,
            Vector3 targetFootPosition,
            Quaternion targetRotation)
        {
            GameObject existing = FindObjectInScene(scene, CombatStartVisualPlacementName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            GameObject visualObject = UnityEngine.Object.Instantiate(sourcePlacement.gameObject);
            visualObject.name = CombatStartVisualPlacementName;
            SceneManager.MoveGameObjectToScene(visualObject, scene);
            visualObject.SetActive(false);

            Transform visualPlacement = visualObject.transform;
            Transform visualPlayer = FindChildRecursive(visualPlacement, sourcePlayer.name)
                ?? throw new InvalidOperationException($"Duplicated combat-start visual is missing `{sourcePlayer.name}`.");
            visualPlacement.SetPositionAndRotation(targetFootPosition, targetRotation);
            visualPlacement.localScale = sourcePlacement.localScale;

            Vector3 footDelta = targetFootPosition - ResolvePlayerFootOrigin(visualPlayer);
            visualPlacement.position += footDelta;
            visualObject.SetActive(false);

            EditorUtility.SetDirty(visualObject);
            return visualPlacement;
        }

        private static void AddOrUpdateCinemachineClip(
            TimelineAsset timeline,
            PlayableDirector director,
            CinemachineBrain brain,
            CinemachineCamera revealCamera,
            double startSeconds)
        {
            CinemachineTrack track = FindTimelineTrack<CinemachineTrack>(timeline, CinemachineTrackName)
                ?? throw new InvalidOperationException($"Timeline is missing `{CinemachineTrackName}`.");
            director.SetGenericBinding(track, brain);
            DeleteClipsByDisplayName(track, RevealShotId);

            TimelineClip clip = track.CreateClip<CinemachineShot>();
            clip.displayName = RevealShotId;
            clip.start = startSeconds;
            clip.duration = RevealDurationSeconds;
            clip.blendInDuration = 0.42d;
            clip.easeInDuration = 0.42d;

            CinemachineShot shotAsset = clip.asset as CinemachineShot;
            if (shotAsset == null)
            {
                throw new InvalidOperationException("Created Cinemachine clip did not contain a CinemachineShot asset.");
            }

            shotAsset.DisplayName = RevealShotId;
            PropertyName exposedName = new PropertyName("cm_09_src_c10_player_reveal_rina_quest_start");
            shotAsset.VirtualCamera.exposedName = exposedName;
            director.SetReferenceValue(exposedName, revealCamera);
            EditorUtility.SetDirty(shotAsset);
            EditorUtility.SetDirty(track);
        }

        private static void AddOrUpdateAnimationTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            Animator animator,
            AnimationClip clipAsset,
            double startSeconds)
        {
            RemoveTimelineTrack(timeline, RevealAnimationTrackName, director);

            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(RevealAnimationTrackName);
            track.trackOffset = TrackOffset.Auto;
            director.SetGenericBinding(track, animator);
            TimelineClip clip = track.CreateClip(clipAsset);
            clip.displayName = clipAsset.name;
            clip.start = startSeconds;
            clip.duration = RevealDurationSeconds;
            SetTimelineClipExtrapolation(clip, TimelineClip.ClipExtrapolation.None);

            AnimationPlayableAsset playableAsset = clip.asset as AnimationPlayableAsset;
            if (playableAsset != null)
            {
                playableAsset.removeStartOffset = false;
                playableAsset.applyFootIK = false;
                playableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
                EditorUtility.SetDirty(playableAsset);
            }

            EditorUtility.SetDirty(track);
        }

        private static void SetTimelineClipExtrapolation(
            TimelineClip timelineClip,
            TimelineClip.ClipExtrapolation extrapolation)
        {
            if (timelineClip == null)
            {
                return;
            }

            const System.Reflection.BindingFlags Flags =
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic;
            typeof(TimelineClip).GetField("m_PreExtrapolationMode", Flags)?.SetValue(
                timelineClip,
                extrapolation);
            typeof(TimelineClip).GetField("m_PostExtrapolationMode", Flags)?.SetValue(
                timelineClip,
                extrapolation);
        }

        private static void AddOrUpdateActivationTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            string trackName,
            GameObject target,
            double startSeconds,
            double durationSeconds)
        {
            RemoveTimelineTrack(timeline, trackName, director);

            ActivationTrack track = timeline.CreateTrack<ActivationTrack>(trackName);
            director.SetGenericBinding(track, target);
            TimelineClip clip = track.CreateDefaultClip();
            clip.displayName = trackName;
            clip.start = startSeconds;
            clip.duration = durationSeconds;
            SetTimelineClipExtrapolation(clip, TimelineClip.ClipExtrapolation.None);
            track.postPlaybackState = ActivationTrack.PostPlaybackState.Inactive;
            EditorUtility.SetDirty(track);
        }

        private static void AddOrUpdateCombatStartVisualBodyClips(
            TimelineAsset timeline,
            PlayableDirector director,
            Animator animator,
            AnimationClip combatReadyClip,
            AnimationClip readyEnterClip,
            double revealStartSeconds,
            double revealEndSeconds)
        {
            RemoveTimelineTrack(timeline, CombatStartVisualBodyTrackName, director);

            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(CombatStartVisualBodyTrackName);
            track.trackOffset = TrackOffset.Auto;
            director.SetGenericBinding(track, animator);

            CreateAnimationClip(
                track,
                readyEnterClip,
                CombatStartReadyEnterClipName,
                revealStartSeconds,
                CombatStartReadyEnterDurationSeconds,
                0.12d,
                0.16d);

            double settleStart = revealStartSeconds + Math.Max(0.2d, CombatStartReadyEnterDurationSeconds - 0.18d);
            CreateAnimationClip(
                track,
                combatReadyClip,
                CombatStartReadySettleClipName,
                settleStart,
                Math.Max(0.1d, revealEndSeconds - settleStart),
                0.2d,
                0d);

            EditorUtility.SetDirty(track);
        }

        private static TimelineClip CreateAnimationClip(
            AnimationTrack track,
            AnimationClip clipAsset,
            string displayName,
            double startSeconds,
            double durationSeconds,
            double easeInSeconds = 0d,
            double easeOutSeconds = 0d)
        {
            TimelineClip clip = track.CreateClip(clipAsset);
            clip.displayName = displayName;
            clip.start = startSeconds;
            clip.duration = durationSeconds;
            clip.easeInDuration = easeInSeconds;
            clip.easeOutDuration = easeOutSeconds;

            AnimationPlayableAsset playableAsset = clip.asset as AnimationPlayableAsset;
            if (playableAsset != null)
            {
                playableAsset.removeStartOffset = false;
                playableAsset.applyFootIK = true;
                playableAsset.loop = AnimationPlayableAsset.LoopMode.Off;
                EditorUtility.SetDirty(playableAsset);
            }

            return clip;
        }

        private static double TryFindClipStart(TrackAsset track, string displayName, double fallback)
        {
            foreach (TimelineClip clip in track.GetClips())
            {
                if (string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                {
                    return clip.start;
                }
            }

            return fallback;
        }

        private static void UpdateShotPlayer(
            Scene scene,
            CinemachineBrain brain,
            CinemachineCamera revealCamera,
            double revealStartSeconds)
        {
            IntroGatePodCinemachineShotPlayer shotPlayer =
                RequireObjectInScene(scene, ShotPlayerObjectName).GetComponent<IntroGatePodCinemachineShotPlayer>();
            if (shotPlayer == null)
            {
                throw new InvalidOperationException($"`{ShotPlayerObjectName}` is missing IntroGatePodCinemachineShotPlayer.");
            }

            List<IntroGatePodCinemachineShotPlayer.Shot> shots =
                new List<IntroGatePodCinemachineShotPlayer.Shot>(shotPlayer.Shots);
            shots.RemoveAll(shot => string.Equals(shot.ShotId, RevealShotId, StringComparison.Ordinal));
            shots.Add(new IntroGatePodCinemachineShotPlayer.Shot(
                RevealShotId,
                (float)revealStartSeconds,
                revealCamera,
                CinemachineBlendDefinition.Styles.EaseInOut,
                0.42f));
            shots.Sort((a, b) => a.StartSeconds.CompareTo(b.StartSeconds));
            shotPlayer.Configure(brain, shots.ToArray(), false, true);
            shotPlayer.enabled = false;
            EditorUtility.SetDirty(shotPlayer);
        }

        private static void ExtendCombatReadyTimelineClip(TimelineAsset timeline, double revealEndSeconds)
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    if (!string.Equals(clip.displayName, CombatReadyTimelineClipName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    clip.duration = Math.Max(clip.duration, revealEndSeconds - clip.start);
                    EditorUtility.SetDirty(track);
                }
            }
        }

        private static void ExtendLetterboxTimelineClip(TimelineAsset timeline, double authoredEndSeconds)
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null || !string.Equals(track.name, "Letterbox", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    clip.duration = Math.Max(clip.duration, authoredEndSeconds - clip.start);
                }

                EditorUtility.SetDirty(track);
            }
        }

        private static void UpdateProfile(
            CinematicSequenceProfile profile,
            double revealStartSeconds,
            double revealEndSeconds,
            double authoredEndSeconds)
        {
            SerializedObject serialized = new SerializedObject(profile);
            RequireProperty(serialized, "authoredDurationSeconds").floatValue = (float)authoredEndSeconds;

            SerializedProperty cameraCues = RequireProperty(serialized, "cameraCues");
            SerializedProperty revealCue = FindArrayElementByString(cameraCues, "cueId", RevealShotId);
            if (revealCue == null)
            {
                int index = cameraCues.arraySize;
                cameraCues.InsertArrayElementAtIndex(index);
                revealCue = cameraCues.GetArrayElementAtIndex(index);
            }

            SetBool(revealCue, "enabled", true);
            SetString(revealCue, "cueId", RevealShotId);
            SetInt(revealCue, "purpose", 2);
            SetInt(revealCue, "blendKind", 2);
            SetFloat(revealCue, "startSeconds", (float)revealStartSeconds);
            SetFloat(revealCue, "durationSeconds", (float)RevealDurationSeconds);
            SetVector3(revealCue, "localOffset", Vector3.zero);
            SetFloat(revealCue, "planarDirectionOffset", 0f);
            SetFloat(revealCue, "fieldOfViewDelta", 0f);
            SetFloat(revealCue, "cameraDistanceDelta", 0f);
            SetFloat(revealCue, "focusHeightDelta", 0f);
            SetFloat(revealCue, "impulseScale", 1f);
            SetBool(revealCue, "driveCameraPose", true);
            SetVector3(revealCue, "cameraLocalPosition", new Vector3(0f, 1.35f, -3.0f));
            SetVector3(revealCue, "lookAtLocalPosition", new Vector3(0f, 1.18f, 0f));
            SetFloat(revealCue, "fieldOfView", RevealFieldOfView);

            SerializedProperty actorCues = RequireProperty(serialized, "actorCues");
            SerializedProperty combatCue = FindArrayElementByString(actorCues, "cueId", CombatReadyActorCueId);
            if (combatCue != null)
            {
                float startSeconds = combatCue.FindPropertyRelative("startSeconds").floatValue;
                SetFloat(combatCue, "durationSeconds", Mathf.Max(0.01f, (float)revealStartSeconds - startSeconds));
            }

            SetActorBodyStateCue(
                actorCues,
                CombatStartReadyEnterCueId,
                (float)revealStartSeconds,
                (float)CombatStartReadyEnterDurationSeconds,
                CombatStartReadyEnterStateName);
            SetActorBodyStateCue(
                actorCues,
                CombatStartReadySettleCueId,
                (float)(revealStartSeconds + Math.Max(0.2d, CombatStartReadyEnterDurationSeconds - 0.18d)),
                Mathf.Max(0.1f, (float)(revealEndSeconds - (revealStartSeconds + Math.Max(0.2d, CombatStartReadyEnterDurationSeconds - 0.18d)))),
                CombatReadyStateName);

            SerializedProperty gameplayHandoff = RequireProperty(serialized, "gameplayHandoff");
            SetFloat(gameplayHandoff, "startSeconds", (float)revealEndSeconds);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void SetActorBodyStateCue(
            SerializedProperty actorCues,
            string cueId,
            float startSeconds,
            float durationSeconds,
            string animatorStateName)
        {
            SerializedProperty cue = FindArrayElementByString(actorCues, "cueId", cueId);
            if (cue == null)
            {
                int index = actorCues.arraySize;
                actorCues.InsertArrayElementAtIndex(index);
                cue = actorCues.GetArrayElementAtIndex(index);
            }

            SetBool(cue, "enabled", true);
            SetString(cue, "cueId", cueId);
            SetInt(cue, "role", 0);
            SetInt(cue, "cueKind", 0);
            SetFloat(cue, "startSeconds", startSeconds);
            SetFloat(cue, "durationSeconds", durationSeconds);
            SetObject(cue, "clip", null);
            SetObject(cue, "avatarMask", null);
            SetString(cue, "animatorStateName", animatorStateName);
            SetString(cue, "animatorTriggerName", string.Empty);
            SetString(cue, "faceStateName", string.Empty);
            SetString(cue, "socketPath", string.Empty);
            SetBool(cue, "requireSocket", false);
            SetObject(cue, "controllerOverride", null);
            SetBool(cue, "objectActive", true);
        }

        private static void UpdateStageDefinitionContract(
            StageDefinitionProfile stageDefinition,
            Vector3 playerStartPosition,
            Vector3 cameraAnchorPosition,
            Vector3 facingEuler)
        {
            SerializedObject serialized = new SerializedObject(stageDefinition);
            SerializedProperty anchors = RequireProperty(serialized, "anchors");

            SetAnchorExpected(anchors, PlayerCameraAnchorName, cameraAnchorPosition, facingEuler);
            SetAnchorExpected(anchors, CorridorFacingAnchorName, cameraAnchorPosition, facingEuler);
            SetAnchorExpected(anchors, GameplayCombatStartAnchorName, playerStartPosition, facingEuler);
            SetAnchorExpected(anchors, StageSpawnerPlayerStartName, playerStartPosition, facingEuler);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(stageDefinition);
        }

        private static void SetAnchorExpected(
            SerializedProperty anchors,
            string anchorId,
            Vector3 expectedPosition,
            Vector3 expectedEuler)
        {
            SerializedProperty anchor = FindArrayElementByString(anchors, "anchorId", anchorId)
                ?? throw new InvalidOperationException($"Stage definition is missing anchor `{anchorId}`.");
            SetVector3(anchor, "expectedPosition", expectedPosition);
            SetVector3(anchor, "expectedEuler", expectedEuler);
        }

        private static List<string> ValidateFraming(
            Transform cameraTransform,
            Transform player,
            AnimationClip clip,
            double revealStartSeconds,
            double revealEndSeconds)
        {
            List<string> issues = new List<string>();
            Bounds bounds = ResolveRenderableBounds(player);
            PlayerFramePoints points = ResolvePlayerFramePoints(player, bounds);
            using CameraSampler sampler = new CameraSampler(RevealFieldOfView);

            ValidatePoint(issues, sampler, cameraTransform, clip, 0.18f, points.Foot, "player foot opening");
            ValidatePoint(issues, sampler, cameraTransform, clip, 3.15f, points.Chest, "player torso sweep");
            ValidatePoint(issues, sampler, cameraTransform, clip, 4.75f, points.Head, "player face reveal");

            float finalVisible = ScoreBoundsVisible(sampler, cameraTransform, clip, 7.35f, bounds);
            if (finalVisible < 0.62f)
            {
                issues.Add($"Final third-person framing only keeps {finalVisible:P0} of player bounds points in view.");
            }

            if (revealEndSeconds <= revealStartSeconds)
            {
                issues.Add("Reveal end time is not after reveal start time.");
            }

            return issues;
        }

        private static List<string> ValidateCameraAnchor(
            Transform revealRoot,
            Vector3 expectedPosition,
            Quaternion expectedRotation)
        {
            List<string> issues = new List<string>();
            float positionDistance = Vector3.Distance(revealRoot.position, expectedPosition);
            float rotationAngle = Quaternion.Angle(revealRoot.rotation, expectedRotation);
            if (positionDistance > 0.02f)
            {
                issues.Add(
                    $"Reveal camera root is {positionDistance:0.###}m from PodBaseReadability. actual=`{FormatVector(revealRoot.position)}`, expected=`{FormatVector(expectedPosition)}`.");
            }

            if (rotationAngle > 0.5f)
            {
                issues.Add(
                    $"Reveal camera root rotation differs from corridor-facing anchor by {rotationAngle:0.###} degrees.");
            }

            return issues;
        }

        private static void ValidateCombatStartPlacement(
            List<string> issues,
            PlayableDirector director,
            Transform placement,
            Transform player,
            TransformSnapshot originalPlacement,
            Vector3 expectedFootPosition,
            double revealStartSeconds)
        {
            originalPlacement.Apply(placement);
            director.time = Math.Min(FirstPersonPlacementValidationSeconds, Math.Max(0d, revealStartSeconds - 0.2d));
            director.Evaluate();
            float preRevealDrift = Vector3.Distance(placement.localPosition, originalPlacement.LocalPosition);
            float preRevealRotationDrift = Quaternion.Angle(placement.localRotation, originalPlacement.LocalRotation);
            if (preRevealDrift > 0.01f || preRevealRotationDrift > 0.5f)
            {
                issues.Add(
                    $"First-person Inori placement drifted before combat-start relocation. positionDrift={preRevealDrift:0.###}m, rotationDrift={preRevealRotationDrift:0.###}deg.");
            }

            originalPlacement.Apply(placement);
            director.time = revealStartSeconds + 0.15d;
            director.Evaluate();
            Vector3 actualFootPosition = ResolvePlayerFootOrigin(player);
            float distance = Vector3.Distance(actualFootPosition, expectedFootPosition);
            if (distance > 0.08f)
            {
                issues.Add(
                    $"Combat-start placement did not move player to PodBaseReadability. actualFoot=`{FormatVector(actualFootPosition)}`, expected=`{FormatVector(expectedFootPosition)}`, distance={distance:0.###}m.");
            }
        }

        private static void ValidatePoint(
            List<string> issues,
            CameraSampler sampler,
            Transform cameraTransform,
            AnimationClip clip,
            float localTime,
            Vector3 point,
            string label)
        {
            Vector3 viewport = sampler.Sample(cameraTransform, clip, localTime, point);
            if (viewport.z <= 0f || !IsInsideViewport(viewport, ViewportMargin))
            {
                issues.Add(
                    $"{label} is outside camera frame at {localTime:0.###}s: viewport=({viewport.x:0.###}, {viewport.y:0.###}, z={viewport.z:0.###}).");
            }
        }

        private static void WriteReport(
            IReadOnlyList<string> issues,
            double revealStartSeconds,
            double revealEndSeconds,
            double authoredEndSeconds,
            Transform revealRoot,
            Vector3 podBasePosition,
            Vector3 corridorFacingEuler)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            using StreamWriter writer = new StreamWriter(ReportPath, false);
            writer.WriteLine("# Intro GatePod Player Reveal Camera");
            writer.WriteLine();
            writer.WriteLine($"Status: {(issues.Count == 0 ? "PASS" : "FAIL")}");
            writer.WriteLine();
            writer.WriteLine($"- Source camera clip: `{RinaCameraClipPath}`");
            writer.WriteLine($"- Reveal shot id: `{RevealShotId}`");
            writer.WriteLine($"- Reveal start: `{revealStartSeconds:0.###}s`");
            writer.WriteLine($"- Reveal end/gameplay handoff: `{revealEndSeconds:0.###}s`");
            writer.WriteLine($"- Timeline authored end: `{authoredEndSeconds:0.###}s`");
            writer.WriteLine($"- PodBaseReadability world position: `{FormatVector(podBasePosition)}`");
            writer.WriteLine($"- Corridor-facing euler: `{FormatVector(corridorFacingEuler)}`");
            writer.WriteLine($"- Reveal root position: `{FormatVector(revealRoot.position)}`");
            writer.WriteLine($"- Reveal root euler: `{FormatVector(revealRoot.eulerAngles)}`");
            if (issues.Count > 0)
            {
                writer.WriteLine();
                writer.WriteLine("## Issues");
                for (int i = 0; i < issues.Count; i++)
                {
                    writer.WriteLine($"- {issues[i]}");
                }
            }
        }

        private static double FindCameraCueEnd(
            CinematicSequenceProfile profile,
            string cueId)
        {
            CinematicSequenceProfile.CameraCue[] cues = profile.CameraCues;
            int matchCount = 0;
            double endSeconds = 0d;
            for (int i = 0; i < cues.Length; i++)
            {
                if (!string.Equals(cues[i].CueId, cueId, StringComparison.Ordinal))
                {
                    continue;
                }

                endSeconds = cues[i].EndSeconds;
                matchCount++;
            }

            if (matchCount != 1)
            {
                throw new InvalidOperationException(
                    $"Cinematic profile must contain `{cueId}` exactly once; found {matchCount}.");
            }

            return endSeconds;
        }

        private static void DeleteClipsByDisplayName(TrackAsset track, string displayName)
        {
            List<TimelineClip> matches = new List<TimelineClip>();
            foreach (TimelineClip clip in track.GetClips())
            {
                if (string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                {
                    matches.Add(clip);
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                track.DeleteClip(matches[i]);
            }
        }

        private static bool RemoveTimelineTrack(TimelineAsset timeline, string trackName, PlayableDirector director)
        {
            List<TrackAsset> matches = new List<TrackAsset>();
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track != null && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    matches.Add(track);
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                director.ClearGenericBinding(matches[i]);
                timeline.DeleteTrack(matches[i]);
            }

            if (matches.Count > 0)
            {
                EditorUtility.SetDirty(timeline);
            }

            return matches.Count > 0;
        }

        private static Vector3 ResolvePlayerFootOrigin(Transform player)
        {
            Bounds bounds = ResolveRenderableBounds(player);
            return ResolvePlayerFramePoints(player, bounds).Foot;
        }

        private static PlayerFramePoints ResolvePlayerFramePoints(Transform player, Bounds bounds)
        {
            Animator animator = player.GetComponentInChildren<Animator>(includeInactive: true);
            Transform leftFoot = ResolveHumanBone(animator, HumanBodyBones.LeftFoot);
            Transform rightFoot = ResolveHumanBone(animator, HumanBodyBones.RightFoot);
            Transform head = ResolveHumanBone(animator, HumanBodyBones.Head);
            Transform chest = ResolveHumanBone(animator, HumanBodyBones.UpperChest)
                ?? ResolveHumanBone(animator, HumanBodyBones.Chest);

            Vector3 boundsCenter = bounds.center;
            Vector3 foot = leftFoot != null && rightFoot != null
                ? (leftFoot.position + rightFoot.position) * 0.5f
                : new Vector3(boundsCenter.x, bounds.min.y + 0.06f, boundsCenter.z);
            Vector3 resolvedHead = head != null
                ? head.position + (Vector3.up * 0.045f)
                : new Vector3(boundsCenter.x, bounds.max.y - 0.10f, boundsCenter.z);
            Vector3 resolvedChest = chest != null
                ? chest.position
                : Vector3.Lerp(foot, resolvedHead, 0.62f);

            return new PlayerFramePoints(foot, resolvedChest, resolvedHead);
        }

        private static Transform ResolveHumanBone(Animator animator, HumanBodyBones bone)
        {
            if (animator == null || !animator.isHuman)
            {
                return null;
            }

            return animator.GetBoneTransform(bone);
        }

        private static Bounds ResolveRenderableBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            bool hasBounds = false;
            Bounds bounds = new Bounds(root.position + Vector3.up, Vector3.one);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }

        private static Quaternion ResolveFlatRotation(Transform transform)
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static Vector3 SampleCameraWorldPosition(Transform cameraTransform, AnimationClip clip, float localTime)
        {
            clip.SampleAnimation(cameraTransform.gameObject, Mathf.Clamp(localTime, 0f, clip.length));
            return cameraTransform.position;
        }

        private static bool IsInsideViewport(Vector3 viewport, float margin)
        {
            return viewport.x >= margin
                && viewport.x <= 1f - margin
                && viewport.y >= margin
                && viewport.y <= 1f - margin;
        }

        private static Vector3[] BuildBoundsPoints(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 center = bounds.center;
            return new[]
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z),
                new Vector3(center.x, min.y, center.z),
                new Vector3(center.x, max.y, center.z),
                center
            };
        }

        private static T FindTimelineTrack<T>(TimelineAsset timeline, string trackName) where T : TrackAsset
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is T typed && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    return typed;
                }
            }

            return null;
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset `{path}`.");
            }

            return asset;
        }

        private static AnimationClip LoadAnimationClip(string assetPath, string clipName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip
                    && string.Equals(clip.name, clipName, StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            throw new InvalidOperationException($"Missing animation clip `{clipName}` in `{assetPath}`.");
        }

        private static PlayableDirector FindDirectorBoundToTimeline(Scene scene, TimelineAsset timeline)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                PlayableDirector[] directors = root.GetComponentsInChildren<PlayableDirector>(includeInactive: true);
                for (int i = 0; i < directors.Length; i++)
                {
                    if (directors[i] != null && directors[i].playableAsset == timeline)
                    {
                        return directors[i];
                    }
                }
            }

            return null;
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static Animator EnsureAnimator(Transform transform)
        {
            Animator animator = transform.GetComponent<Animator>();
            if (animator == null)
            {
                animator = transform.gameObject.AddComponent<Animator>();
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            return animator;
        }

        private static GameObject RequireObjectInScene(Scene scene, string objectName)
        {
            GameObject found = FindObjectInScene(scene, objectName);
            if (found == null)
            {
                throw new InvalidOperationException($"Scene `{scene.path}` is missing `{objectName}`.");
            }

            return found;
        }

        private static GameObject FindObjectInScene(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindChildRecursive(root.transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, childName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Serialized object `{serializedObject.targetObject.name}` is missing `{propertyName}`.");
            }

            return property;
        }

        private static SerializedProperty FindArrayElementByString(
            SerializedProperty array,
            string fieldName,
            string value)
        {
            if (array == null || !array.isArray)
            {
                return null;
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                SerializedProperty field = element.FindPropertyRelative(fieldName);
                if (field != null && string.Equals(field.stringValue, value, StringComparison.Ordinal))
                {
                    return element;
                }
            }

            return null;
        }

        private static void SetBool(SerializedProperty parent, string propertyName, bool value)
        {
            parent.FindPropertyRelative(propertyName).boolValue = value;
        }

        private static void SetInt(SerializedProperty parent, string propertyName, int value)
        {
            parent.FindPropertyRelative(propertyName).intValue = value;
        }

        private static void SetFloat(SerializedProperty parent, string propertyName, float value)
        {
            parent.FindPropertyRelative(propertyName).floatValue = value;
        }

        private static void SetString(SerializedProperty parent, string propertyName, string value)
        {
            parent.FindPropertyRelative(propertyName).stringValue = value;
        }

        private static void SetVector3(SerializedProperty parent, string propertyName, Vector3 value)
        {
            parent.FindPropertyRelative(propertyName).vector3Value = value;
        }

        private static void SetObject(SerializedProperty parent, string propertyName, UnityEngine.Object value)
        {
            parent.FindPropertyRelative(propertyName).objectReferenceValue = value;
        }

        private static string FormatVector(Vector3 value)
        {
            return $"{value.x:0.###}, {value.y:0.###}, {value.z:0.###}";
        }

        private readonly struct PlayerFramePoints
        {
            public PlayerFramePoints(Vector3 foot, Vector3 chest, Vector3 head)
            {
                Foot = foot;
                Chest = chest;
                Head = head;
            }

            public readonly Vector3 Foot;
            public readonly Vector3 Chest;
            public readonly Vector3 Head;
        }

        private readonly struct CameraTransitionSpec
        {
            public CameraTransitionSpec(
                string shotId,
                double nominalStartSeconds,
                double nominalDurationSeconds,
                double incomingBlendSeconds)
            {
                ShotId = shotId;
                NominalStartSeconds = nominalStartSeconds;
                NominalDurationSeconds = nominalDurationSeconds;
                IncomingBlendSeconds = incomingBlendSeconds;
            }

            public readonly string ShotId;
            public readonly double NominalStartSeconds;
            public readonly double NominalDurationSeconds;
            public readonly double IncomingBlendSeconds;
        }

        private readonly struct TransformSnapshot
        {
            public TransformSnapshot(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
            {
                LocalPosition = localPosition;
                LocalRotation = localRotation;
                LocalScale = localScale;
            }

            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
            public readonly Vector3 LocalScale;

            public static TransformSnapshot Capture(Transform transform)
            {
                return new TransformSnapshot(transform.localPosition, transform.localRotation, transform.localScale);
            }

            public TransformSnapshot WithLocalPosition(Vector3 localPosition)
            {
                return new TransformSnapshot(localPosition, LocalRotation, LocalScale);
            }

            public void Apply(Transform transform)
            {
                transform.localPosition = LocalPosition;
                transform.localRotation = LocalRotation;
                transform.localScale = LocalScale;
            }
        }

        private sealed class CameraSampler : IDisposable
        {
            private readonly Camera camera;

            public CameraSampler(float fieldOfView)
            {
                GameObject cameraObject = new GameObject("IntroGatePod_PlayerRevealValidationCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.fieldOfView = fieldOfView;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 250f;
                camera.aspect = 16f / 9f;
            }

            public Vector3 Sample(Transform cameraTransform, AnimationClip clip, float localTime, Vector3 worldPoint)
            {
                clip.SampleAnimation(cameraTransform.gameObject, Mathf.Clamp(localTime, 0f, clip.length));
                camera.transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
                return camera.WorldToViewportPoint(worldPoint);
            }

            public void Dispose()
            {
                if (camera != null)
                {
                    UnityEngine.Object.DestroyImmediate(camera.gameObject);
                }
            }
        }
    }
}
