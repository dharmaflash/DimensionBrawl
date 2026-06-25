using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class BuildResubmissionCinematicPackageVerifier
    {
        private const string ReportPath = "C:/tmp/DimensionBrawl-CinematicPackageVerifier.md";
        private const string ProfileRoot = BuildResubmissionCinematicProfileSetup.ProfileRoot;

        private static readonly string[] RequiredProfiles =
        {
            "DB_Cinematic_IntroAwakening.asset",
            "DB_Cinematic_GameplayHandoff.asset",
            "DB_Cinematic_QTEAssist.asset",
            "DB_Cinematic_UltimateCutIn.asset",
            "DB_Cinematic_DangerCue.asset",
            "DB_Cinematic_CombatTutorialOverlay.asset",
            "DB_Cinematic_BossIntro.asset",
            "DB_Cinematic_PhaseTransition.asset",
            "DB_Cinematic_BreakMoment.asset",
            "DB_Cinematic_DialogueReactionBeat.asset",
            "DB_Cinematic_ResultBridge.asset",
            "DB_Cinematic_SummonEntry.asset",
            "DB_Cinematic_SummonFollowupHit.asset",
            "DB_Cinematic_SummonEmpower.asset",
            "DB_Cinematic_SummonRecall.asset",
            "DB_Cinematic_BossSummonPressure.asset"
        };

        private static readonly string[] RequiredPlaylistProfiles =
        {
            "DB_Cinematic_IntroAwakening.asset",
            "DB_Cinematic_QTEAssist.asset",
            "DB_Cinematic_UltimateCutIn.asset",
            "DB_Cinematic_DangerCue.asset",
            "DB_Cinematic_CombatTutorialOverlay.asset",
            "DB_Cinematic_GameplayHandoff.asset"
        };

        [MenuItem("DimensionBrawl/Cinematics/Verify Build Resubmission Cinematic Package")]
        public static void VerifyPackageMenu()
        {
            VerifyPackage();
            Debug.Log($"Wrote cinematic package verification report to {ReportPath}.");
        }

        public static void RunBatchVerification()
        {
            if (!VerifyPackage())
            {
                EditorApplication.Exit(1);
            }
        }

        public static bool VerifyPackage()
        {
            List<string> report = new List<string>
            {
                "# Build Resubmission Cinematic Package Verification",
                string.Empty,
                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                string.Empty
            };
            VerificationState state = new VerificationState(report);

            VerifyPromotedAnimations(state);
            VerifyCinematicController(state);
            VerifyCinematicProfiles(state);
            VerifyReviewScene(state);

            report.Add("## Result");
            report.Add(string.Empty);
            report.Add(state.FailCount == 0 ? "PASS" : "FAIL");
            report.Add(string.Empty);
            report.Add($"Failures: {state.FailCount}");
            report.Add($"Warnings: {state.WarningCount}");
            report.Add(string.Empty);

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllLines(ReportPath, report);

            if (state.FailCount > 0)
            {
                Debug.LogError($"Cinematic package verification failed. See {ReportPath}.");
                return false;
            }

            Debug.Log($"Cinematic package verification passed. See {ReportPath}.");
            return true;
        }

        private static void VerifyPromotedAnimations(VerificationState state)
        {
            state.Header("Promoted Kawaii P0 Animation Clips");
            IReadOnlyList<string> stateNames = BuildResubmissionCinematicAnimationSetup.RequiredStateNames;
            for (int i = 0; i < stateNames.Count; i++)
            {
                string stateName = stateNames[i];
                string path = $"{BuildResubmissionCinematicAnimationSetup.AnimationRoot}/{stateName}.fbx";
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                state.Check(importer != null, $"Promoted FBX exists with ModelImporter: `{stateName}`.");
                if (importer == null)
                {
                    continue;
                }

                state.Check(importer.importAnimation, $"{stateName} imports animation.");
                state.Check(importer.animationType == ModelImporterAnimationType.Human, $"{stateName} imports as Humanoid.");
                state.Check(importer.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel, $"{stateName} creates its source Avatar from the promoted clip.");

                AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .FirstOrDefault(candidate => string.Equals(candidate.name, stateName, StringComparison.Ordinal));
                state.Check(clip != null, $"{stateName} exposes a named AnimationClip.");
            }

            state.Blank();
        }

        private static void VerifyCinematicController(VerificationState state)
        {
            state.Header("Inori Cinematic P0 Animator Controller");
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    BuildResubmissionCinematicAnimationSetup.CinematicControllerPath);
            state.Check(controller != null, $"Controller exists: `{BuildResubmissionCinematicAnimationSetup.CinematicControllerPath}`.");
            if (controller == null)
            {
                state.Blank();
                return;
            }

            Dictionary<string, AnimatorState> states = CollectStates(controller);
            IReadOnlyList<string> stateNames = BuildResubmissionCinematicAnimationSetup.RequiredStateNames;
            for (int i = 0; i < stateNames.Count; i++)
            {
                string stateName = stateNames[i];
                state.Check(states.TryGetValue(stateName, out AnimatorState animatorState), $"Controller has state `{stateName}`.");
                if (animatorState != null)
                {
                    state.Check(animatorState.motion != null, $"State `{stateName}` has a motion.");
                }
            }

            state.Blank();
        }

        private static void VerifyCinematicProfiles(VerificationState state)
        {
            state.Header("P0/P1 Cinematic Profiles");
            Dictionary<string, AnimatorState> controllerStates = CollectStates(
                AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    BuildResubmissionCinematicAnimationSetup.CinematicControllerPath));

            for (int i = 0; i < RequiredProfiles.Length; i++)
            {
                string path = $"{ProfileRoot}/{RequiredProfiles[i]}";
                CinematicSequenceProfile profile = AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(path);
                state.Check(profile != null, $"Profile exists: `{RequiredProfiles[i]}`.");
                if (profile == null)
                {
                    continue;
                }

                List<string> issues = new List<string>();
                profile.CollectValidationIssues(issues);
                for (int j = 0; j < issues.Count; j++)
                {
                    state.Fail(issues[j]);
                }

                int enabledCameraCueCount = 0;
                int drivenCameraPoseCount = 0;
                CinematicSequenceProfile.CameraCue[] cameraCues = profile.CameraCues;
                for (int j = 0; j < cameraCues.Length; j++)
                {
                    CinematicSequenceProfile.CameraCue cue = cameraCues[j];
                    if (!cue.Enabled)
                    {
                        continue;
                    }

                    enabledCameraCueCount++;
                    state.Check(cue.DriveCameraPose, $"{profile.name} camera cue `{cue.CueId}` drives a direct shot pose.");
                    if (!cue.DriveCameraPose)
                    {
                        continue;
                    }

                    drivenCameraPoseCount++;
                    state.Check(cue.FieldOfView > 0f, $"{profile.name} camera cue `{cue.CueId}` has an authored FOV.");
                    state.Check(
                        (cue.CameraLocalPosition - cue.LookAtLocalPosition).sqrMagnitude > 0.0025f,
                        $"{profile.name} camera cue `{cue.CueId}` has separated camera/look-at positions.");
                }

                state.Check(
                    enabledCameraCueCount > 0 && drivenCameraPoseCount == enabledCameraCueCount,
                    $"{profile.name} drives all enabled camera cues with direct shot poses ({drivenCameraPoseCount}/{enabledCameraCueCount}).");

                if (profile.Category == CinematicSequenceProfile.SequenceCategory.UltimateCutIn)
                {
                    state.Check(
                        drivenCameraPoseCount >= 4,
                        $"{profile.name} UltimateCutIn has direct camera shot poses for focus, charge, release, and handoff.");
                }

                CinematicSequenceProfile.ActorCue[] actorCues = profile.ActorCues;
                int weaponVisibilityCueCount = 0;
                bool hidesWeapon = false;
                bool showsWeapon = false;
                float firstHideSeconds = float.MaxValue;
                float firstShowSeconds = float.MaxValue;
                for (int j = 0; j < actorCues.Length; j++)
                {
                    CinematicSequenceProfile.ActorCue cue = actorCues[j];
                    if (!cue.Enabled || cue.CueKind != CinematicSequenceProfile.ActorCueKind.WeaponVisibility)
                    {
                        continue;
                    }

                    if (cue.Role != CinematicSequenceProfile.ActorRole.Inori)
                    {
                        state.Check(
                            !string.IsNullOrWhiteSpace(cue.CueId),
                            $"{profile.name} external actor visibility cue `{cue.CueId}` is authored for role `{cue.Role}`.");
                        continue;
                    }

                    weaponVisibilityCueCount++;
                    state.Check(
                        !string.IsNullOrWhiteSpace(cue.SocketPath),
                        $"{profile.name} weapon visibility cue `{cue.CueId}` has a target object path.");
                    if (cue.ObjectActive)
                    {
                        showsWeapon = true;
                        firstShowSeconds = Mathf.Min(firstShowSeconds, cue.StartSeconds);
                    }
                    else
                    {
                        hidesWeapon = true;
                        firstHideSeconds = Mathf.Min(firstHideSeconds, cue.StartSeconds);
                    }
                }

                state.Check(weaponVisibilityCueCount > 0, $"{profile.name} has profile-driven weapon visibility cues.");
                if (profile.Category == CinematicSequenceProfile.SequenceCategory.IntroAwakening)
                {
                    state.Check(hidesWeapon, $"{profile.name} hides the rifle before the pickup beat.");
                    state.Check(showsWeapon, $"{profile.name} shows the rifle after the pickup beat.");
                    state.Check(firstHideSeconds < firstShowSeconds, $"{profile.name} rifle hide cue occurs before the show cue.");
                }
                else
                {
                    state.Check(
                        showsWeapon && firstShowSeconds <= 0.05f,
                        $"{profile.name} starts with the rifle visible for combat-facing or story-reaction playback.");
                }

                for (int j = 0; j < actorCues.Length; j++)
                {
                    CinematicSequenceProfile.ActorCue cue = actorCues[j];
                    if (!cue.Enabled || cue.CueKind != CinematicSequenceProfile.ActorCueKind.BodyState)
                    {
                        continue;
                    }

                    string stateName = cue.AnimatorStateName;
                    if (cue.Role == CinematicSequenceProfile.ActorRole.Inori)
                    {
                        state.Check(
                            controllerStates.ContainsKey(stateName),
                            $"{profile.name} body cue `{cue.CueId}` references controller state `{stateName}`.");
                    }
                    else
                    {
                        state.Check(
                            !string.IsNullOrWhiteSpace(stateName),
                            $"{profile.name} external actor body cue `{cue.CueId}` has actor state `{stateName}` for role `{cue.Role}`.");
                    }

                    state.Check(
                        !stateName.StartsWith("R_", StringComparison.Ordinal),
                        $"{profile.name} body cue `{cue.CueId}` no longer uses rifle-only state `{stateName}`.");
                }

                if (profile.Category == CinematicSequenceProfile.SequenceCategory.BossIntro
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.PhaseTransition
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.BreakMoment
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.DialogueReactionBeat
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.ResultBridge
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.SummonEntry
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.SummonFollowupHit
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.SummonEmpower
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.SummonRecall
                    || profile.Category == CinematicSequenceProfile.SequenceCategory.BossSummonPressure)
                {
                    state.Check(
                        profile.GameplayHandoff.Enabled,
                        $"{profile.name} P1 profile has an explicit gameplay/result handoff.");
                    state.Check(
                        profile.ActorCues.Any(cue => cue.Enabled && cue.CueKind == CinematicSequenceProfile.ActorCueKind.FaceState),
                        $"{profile.name} P1 profile drives an Inori face expression.");
                    state.Check(
                        profile.ActorCues.Any(cue => cue.Enabled && cue.CueKind == CinematicSequenceProfile.ActorCueKind.BodyState),
                        $"{profile.name} P1 profile drives an Inori body animation.");
                }
            }

            state.Blank();
        }

        private static void VerifyReviewScene(VerificationState state)
        {
            state.Header("Inspectable P0 Review Scene");
            try
            {
                BuildResubmissionCinematicReviewSceneSetup.ValidateReviewScene();
                state.Check(true, $"Review scene validates: `{BuildResubmissionCinematicReviewSceneSetup.ReviewScenePath}`.");
            }
            catch (Exception exception)
            {
                state.Fail($"Review scene validation threw: {exception.Message}");
                state.Blank();
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                BuildResubmissionCinematicReviewSceneSetup.ReviewScenePath,
                OpenSceneMode.Single);
            CinematicSequenceRunner runner = FindComponentInScene<CinematicSequenceRunner>(scene);
            state.Check(runner != null, "Review scene has CinematicSequenceRunner.");
            if (runner != null)
            {
                state.Check(runner.SequenceProfile != null, "Review runner has a sequence profile.");
                state.Check(
                    runner.SequenceProfile != null
                    && runner.SequenceProfile.Category == CinematicSequenceProfile.SequenceCategory.UltimateCutIn,
                    "Review runner binds the UltimateCutIn profile.");
                state.Check(runner.CinematicCamera != null, "Review runner has a direct cinematic camera assigned.");
                state.Check(runner.DriveCameraTransformFromProfile, "Review runner drives camera transform from profile shot poses.");
                state.Check(runner.TutorialPromptPresenter != null, "Review runner has a cinematic tutorial prompt presenter assigned.");
            }

            ActionCinematicSequenceBridge sequenceBridge =
                FindComponentInScene<ActionCinematicSequenceBridge>(scene);
            state.Check(sequenceBridge != null, "Review scene has ActionCinematicSequenceBridge for action-cue integration.");
            if (sequenceBridge != null)
            {
                SerializedObject serializedBridge = new SerializedObject(sequenceBridge);
                VerifyBridgeProfile(
                    state,
                    serializedBridge,
                    "summonEntryProfile",
                    "DB_Cinematic_SummonEntry.asset",
                    "Action bridge routes SummonEntry to the reusable summon profile.");
                VerifyBridgeProfile(
                    state,
                    serializedBridge,
                    "ultimateCutInProfile",
                    "DB_Cinematic_UltimateCutIn.asset",
                    "Action bridge routes UltimateCutIn to the reusable ultimate profile.");
                VerifyBridgeProfile(
                    state,
                    serializedBridge,
                    "bossPressureBreakProfile",
                    "DB_Cinematic_BossSummonPressure.asset",
                    "Action bridge routes BossPressureBreak to the reusable boss-summon pressure profile.");
                VerifyBridgeProfile(
                    state,
                    serializedBridge,
                    "summonFollowupHitProfile",
                    "DB_Cinematic_SummonFollowupHit.asset",
                    "Action bridge routes SummonFollowupHit to the reusable summon follow-up profile.");
                VerifyBridgeProfile(
                    state,
                    serializedBridge,
                    "summonEmpowerProfile",
                    "DB_Cinematic_SummonEmpower.asset",
                    "Action bridge routes SummonEmpower to the reusable summon empower profile.");
                VerifyBridgeProfile(
                    state,
                    serializedBridge,
                    "summonRecallProfile",
                    "DB_Cinematic_SummonRecall.asset",
                    "Action bridge routes SummonRecall to the reusable summon recall profile.");
                VerifyBridgeProfile(
                    state,
                    serializedBridge,
                    "pocketClearProfile",
                    "DB_Cinematic_ResultBridge.asset",
                    "Action bridge routes PocketClear to the reusable result profile.");
            }

            CinematicSequenceAutoPlay autoPlay = FindComponentInScene<CinematicSequenceAutoPlay>(scene);
            state.Check(autoPlay != null, "Review scene keeps a single-profile AutoPlay component for manual fallback.");
            if (autoPlay != null)
            {
                SerializedObject serializedAutoPlay = new SerializedObject(autoPlay);
                SerializedProperty playOnStart = serializedAutoPlay.FindProperty("playOnStart");
                state.Check(playOnStart != null && !playOnStart.boolValue, "Single-profile AutoPlay is disabled while the P0 playlist is active.");
            }

            CinematicSequencePlaylistRunner playlistRunner =
                FindComponentInScene<CinematicSequencePlaylistRunner>(scene);
            state.Check(playlistRunner != null, "Review scene has CinematicSequencePlaylistRunner.");
            if (playlistRunner != null)
            {
                state.Check(playlistRunner.EntryCount == 6, "P0 playlist runner has six module entries.");
                SerializedObject serializedPlaylist = new SerializedObject(playlistRunner);
                SerializedProperty playOnStart = serializedPlaylist.FindProperty("playOnStart");
                state.Check(playOnStart != null && playOnStart.boolValue, "P0 playlist runner auto-plays for review.");
                VerifyPlaylistOrder(state, serializedPlaylist);
            }

            Animator animator = FindComponentInScene<Animator>(scene);
            RuntimeAnimatorController expectedController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    BuildResubmissionCinematicAnimationSetup.CinematicControllerPath);
            state.Check(
                animator != null && animator.runtimeAnimatorController == expectedController,
                "Review scene Inori Animator uses DB_Inori_CinematicP0.controller.");

            CinematicBlendShapeExpressionPlayer expressionPlayer =
                FindComponentInScene<CinematicBlendShapeExpressionPlayer>(scene);
            state.Check(expressionPlayer != null, "Review scene has CinematicBlendShapeExpressionPlayer.");
            if (expressionPlayer != null)
            {
                SerializedObject serializedObject = new SerializedObject(expressionPlayer);
                SerializedProperty presets = serializedObject.FindProperty("presets");
                state.Check(presets != null && presets.arraySize >= 6, "Expression player has at least six expression presets.");
            }

            state.Check(
                FindComponentInScene<CinematicTutorialPromptPresenter>(scene) != null,
                "Review scene has CinematicTutorialPromptPresenter for readable QTE/tutorial overlays.");
            state.Check(FindTransform(scene, "CinematicP0Review_InoriRifle") != null, "Review scene has attached Inori rifle.");
            state.Check(FindTransform(scene, "CinematicP0Review_StageRoot") != null, "Review scene has cinematic stage dressing root.");
            state.Check(FindTransform(scene, "CinematicP0Review_BackScreen") != null, "Review scene has a dressed back screen instead of a blank horizon.");
            state.Check(FindTransform(scene, "CinematicP0Review_PlayerReadabilityField") != null, "Review scene has a player readability field under Inori.");
            state.Check(FindTransform(scene, "CinematicP0Review_KeyFaceLight") != null, "Review scene has a key face light for Inori cut-ins.");
            state.Blank();
        }

        private static void VerifyPlaylistOrder(VerificationState state, SerializedObject serializedPlaylist)
        {
            SerializedProperty entries = serializedPlaylist.FindProperty("entries");
            state.Check(
                entries != null && entries.arraySize == RequiredPlaylistProfiles.Length,
                "P0 playlist serialized entries match the required module count.");
            if (entries == null)
            {
                return;
            }

            int count = Mathf.Min(entries.arraySize, RequiredPlaylistProfiles.Length);
            for (int i = 0; i < count; i++)
            {
                string assetName = RequiredPlaylistProfiles[i];
                CinematicSequenceProfile expected =
                    AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>($"{ProfileRoot}/{assetName}");
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                UnityEngine.Object actual = entry.FindPropertyRelative("profile").objectReferenceValue;
                state.Check(actual == expected, $"P0 playlist entry {i + 1} is `{assetName}`.");
            }
        }

        private static void VerifyBridgeProfile(
            VerificationState state,
            SerializedObject serializedBridge,
            string propertyName,
            string expectedAssetName,
            string label)
        {
            SerializedProperty property = serializedBridge.FindProperty(propertyName);
            CinematicSequenceProfile expected =
                AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>($"{ProfileRoot}/{expectedAssetName}");
            state.Check(
                property != null && property.objectReferenceValue == expected,
                label);
        }

        private static Dictionary<string, AnimatorState> CollectStates(AnimatorController controller)
        {
            Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>(StringComparer.Ordinal);
            if (controller == null)
            {
                return states;
            }

            for (int i = 0; i < controller.layers.Length; i++)
            {
                ChildAnimatorState[] childStates = controller.layers[i].stateMachine.states;
                for (int j = 0; j < childStates.Length; j++)
                {
                    states[childStates[j].state.name] = childStates[j].state;
                }
            }

            return states;
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static Transform FindTransform(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindTransformRecursive(roots[i].transform, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindTransformRecursive(Transform root, string name)
        {
            if (string.Equals(root.name, name, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindTransformRecursive(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private sealed class VerificationState
        {
            private readonly List<string> report;

            public VerificationState(List<string> report)
            {
                this.report = report;
            }

            public int FailCount { get; private set; }
            public int WarningCount { get; private set; }

            public void Header(string title)
            {
                report.Add($"## {title}");
                report.Add(string.Empty);
            }

            public void Blank()
            {
                report.Add(string.Empty);
            }

            public void Check(bool condition, string message)
            {
                if (condition)
                {
                    report.Add($"- PASS: {message}");
                    return;
                }

                Fail(message);
            }

            public void Fail(string message)
            {
                FailCount++;
                report.Add($"- FAIL: {message}");
            }
        }
    }
}
