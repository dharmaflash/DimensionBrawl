using System;
using System.IO;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class BuildResubmissionCinematicReviewSceneSetup
    {
        public const string ReviewScenePath = "Assets/_Game/Scenes/ActionFoundationCinematicP0Review.unity";
        public const string PreviewCapturePath = "C:/tmp/DimensionBrawl-CinematicP0Review-UltimateRelease.png";
        public const string OpeningFacePreviewCapturePath = "C:/tmp/DimensionBrawl-CinematicP0Review-OpeningFace.png";
        public const string PlaylistStripCapturePath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlaylistStrip.png";
        public const string PlaylistStripReportPath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlaylistStrip.md";
        public const string RunnerDrivenStripCapturePath = "C:/tmp/DimensionBrawl-CinematicP0Review-RunnerDrivenStrip.png";
        public const string RunnerDrivenStripReportPath = "C:/tmp/DimensionBrawl-CinematicP0Review-RunnerDrivenStrip.md";
        public const string P1RunnerDrivenStripCapturePath = "C:/tmp/DimensionBrawl-CinematicP1Review-RunnerDrivenStrip.png";
        public const string P1RunnerDrivenStripReportPath = "C:/tmp/DimensionBrawl-CinematicP1Review-RunnerDrivenStrip.md";
        public const string PlayModeRouteStripCapturePath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeRouteStrip.png";
        public const string PlayModeRouteReportPath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeRoute.md";
        public const string PlayModeRouteResultPath = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeRoute.result";
        public const string PlayModeTimelineStripCapturePath = "C:/tmp/DimensionBrawl-CinematicP0Review-TimelineStrip.png";
        public const string PlayModeTimelineReportPath = "C:/tmp/DimensionBrawl-CinematicP0Review-Timeline.md";

        private const string InoriSourcePrefabPath =
            "Assets/_Imported/AssetStore/RoloArt/Inori/Prefabs/Inori_MagicaCloth2_Costume1.prefab";
        private const string UltimateProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_UltimateCutIn.asset";
        private const string IntroProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroAwakening.asset";
        private const string GameplayHandoffProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_GameplayHandoff.asset";
        private const string QteProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_QTEAssist.asset";
        private const string DangerProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_DangerCue.asset";
        private const string TutorialProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_CombatTutorialOverlay.asset";
        private const string BossIntroProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_BossIntro.asset";
        private const string PhaseTransitionProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_PhaseTransition.asset";
        private const string BreakMomentProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_BreakMoment.asset";
        private const string DialogueReactionBeatProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_DialogueReactionBeat.asset";
        private const string ResultBridgeProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_ResultBridge.asset";
        private const string SummonEntryProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_SummonEntry.asset";
        private const string SummonFollowupHitProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_SummonFollowupHit.asset";
        private const string SummonEmpowerProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_SummonEmpower.asset";
        private const string SummonRecallProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_SummonRecall.asset";
        private const string BossSummonPressureProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_BossSummonPressure.asset";
        private const string RifleGirlSourcePrefabPath =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/RifleGirl/Prefab/Rifle_Full_Body.prefab";
        private const string EnemyPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_GeneralDeck.prefab";

        private const string ActorRootName = "CinematicP0Review_Inori";
        private const string RunnerRootName = "CinematicP0Review_UltimateCutInRunner";
        private const string CameraName = "Main Camera";
        private const string EnemyRootName = "CinematicP0Review_EnemyTarget";
        private const string SummonActorRootName = "CinematicP0Review_SummonVanguard";
        private const string DragonSummonRootName = "CinematicP0Review_VolcanoDragonSummon";
        private const string StageRootName = "CinematicP0Review_StageRoot";
        private const string RifleName = "CinematicP0Review_InoriRifle";
        private const string DragonSummonSourcePrefabPath =
            "Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/Elemental Dragons Pack/Volcano Dragon/Prefabs/VolcanoDragon_PBR.prefab";
        private const string DragonSummonAttackStateName = "FlyStationarySpitFireBall";
        private const string PlaylistFrameDirectory = "C:/tmp/DimensionBrawl-CinematicP0Review-PlaylistFrames";
        private const string RunnerDrivenFrameDirectory = "C:/tmp/DimensionBrawl-CinematicP0Review-RunnerDrivenFrames";
        private const string P1RunnerDrivenFrameDirectory = "C:/tmp/DimensionBrawl-CinematicP1Review-RunnerDrivenFrames";
        private const string PlayModeRouteFrameDirectory = "C:/tmp/DimensionBrawl-CinematicP0Review-PlayModeFrames";
        private const string PlayModeTimelineFrameDirectory = "C:/tmp/DimensionBrawl-CinematicP0Review-TimelineFrames";
        private const string PlayModeCaptureProbeName = "CinematicP0Review_PlayModeRouteCaptureProbe";

        [MenuItem("DimensionBrawl/Cinematics/Reapply P0 Cinematic Review Scene")]
        public static void ReapplyReviewSceneMenu()
        {
            EnsureReviewScene();
            Debug.Log("Reapplied P0 cinematic review scene.");
        }

        [MenuItem("DimensionBrawl/Cinematics/Validate P0 Cinematic Review Scene")]
        public static void ValidateReviewSceneMenu()
        {
            ValidateReviewScene();
            Debug.Log("P0 cinematic review scene validation passed.");
        }

        public static void RunBatchReviewSceneGeneration()
        {
            EnsureReviewScene();
        }

        public static void RunBatchPreviewCapture()
        {
            CaptureUltimateChargePreview();
        }

        public static void RunBatchPlaylistStripCapture()
        {
            EnsureReviewScene();
            CapturePlaylistStrip();
        }

        public static void RunBatchRunnerDrivenPlaylistCapture()
        {
            EnsureReviewScene();
            CaptureRunnerDrivenPlaylistStrip();
        }

        public static void RunBatchP1RunnerDrivenPlaylistCapture()
        {
            EnsureReviewScene();
            CaptureP1RunnerDrivenPlaylistStrip();
        }

        public static void RunBatchPlayModeRouteCapture()
        {
            EnsureReviewScene();
            EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            BuildResubmissionCinematicPlayModeCaptureBatch.Start(PlayModeRouteResultPath, 180f);
            EditorApplication.isPlaying = true;
        }

        public static void RunBatchStatePreviewCapture()
        {
            CaptureStatePreviewSet();
        }

        public static void EnsureReviewScene()
        {
            BuildResubmissionCinematicProfileSetup.RebuildP0Profiles();
            ActionFoundationInoriPlayerVisualAssetSetup.EnsureInoriPlayerVisualAssets();
            CombatVfxCueProfile vfxCueProfile = ActionFoundationCombatVfxSetup.EnsureCombatVfxAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureReviewRenderSettings();
            CreateDirectionalLight(scene);
            CreateCinematicStageDressing(scene);

            GameObject inori = CreateInoriActor(scene);
            Animator inoriAnimator = RequireComponentInChildren<Animator>(inori, "Inori body Animator");
            CinematicBlendShapeExpressionPlayer expressionPlayer =
                ConfigureExpressionPlayer(inori);
            GameObject rifle = CreateInoriRifle(scene, inori.transform, inoriAnimator);
            GameObject enemy = CreateEnemyTarget(scene);
            GameObject summonActor = CreateSummonActor(scene);
            Animator summonAnimator = summonActor.GetComponentInChildren<Animator>(includeInactive: true);
            GameObject dragonSummon = CreateDragonSummonActor(scene);
            Animator dragonAnimator = dragonSummon.GetComponentInChildren<Animator>(includeInactive: true);
            ActionCameraController cameraController = CreateReviewCamera(scene, inori.transform, enemy.transform);
            ApplyInitialReviewCameraPose(cameraController.GetComponent<Camera>(), inori.transform);
            CinematicSequenceRunner runner = CreateRunner(
                scene,
                inori,
                inoriAnimator,
                expressionPlayer,
                summonActor,
                summonAnimator,
                dragonSummon,
                dragonAnimator,
                cameraController,
                vfxCueProfile);

            EditorUtility.SetDirty(inori);
            EditorUtility.SetDirty(rifle);
            EditorUtility.SetDirty(enemy);
            EditorUtility.SetDirty(summonActor);
            EditorUtility.SetDirty(dragonSummon);
            EditorUtility.SetDirty(cameraController);
            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ReviewScenePath);
            AssetDatabase.SaveAssets();
            ValidateReviewScene();
        }

        public static void ValidateReviewScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ReviewScenePath)
            {
                scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            }

            GameObject inori = FindRoot(scene, ActorRootName)
                ?? throw new InvalidOperationException($"Missing {ActorRootName}.");
            Animator animator = RequireComponentInChildren<Animator>(inori, "Inori body Animator");
            if (animator.runtimeAnimatorController != LoadAsset<RuntimeAnimatorController>(
                    BuildResubmissionCinematicAnimationSetup.CinematicControllerPath))
            {
                throw new InvalidOperationException("Inori review actor must use DB_Inori_CinematicP0.controller.");
            }

            if (inori.GetComponentInChildren<CinematicBlendShapeExpressionPlayer>(true) == null)
            {
                throw new InvalidOperationException("Inori review actor is missing CinematicBlendShapeExpressionPlayer.");
            }

            if (FindDescendant(inori.transform, RifleName) == null)
            {
                throw new InvalidOperationException("Inori review actor is missing the attached rifle.");
            }

            GameObject runnerObject = FindRoot(scene, RunnerRootName)
                ?? throw new InvalidOperationException($"Missing {RunnerRootName}.");
            GameObject summonActor = FindRoot(scene, SummonActorRootName)
                ?? throw new InvalidOperationException($"Missing {SummonActorRootName}.");
            if (summonActor.GetComponentInChildren<SummonFrontlineProxy>(true) == null)
            {
                throw new InvalidOperationException("Review scene summon actor must use SummonFrontlineProxy.");
            }

            if (summonActor.GetComponentInChildren<Animator>(true) == null)
            {
                throw new InvalidOperationException("Review scene summon actor must expose an Animator for ActorRole.Summon.");
            }

            GameObject dragonSummon = FindRoot(scene, DragonSummonRootName)
                ?? throw new InvalidOperationException($"Missing {DragonSummonRootName}.");
            Animator dragonAnimator = dragonSummon.GetComponentInChildren<Animator>(true);
            if (dragonAnimator == null)
            {
                throw new InvalidOperationException("Review scene Volcano Dragon summon candidate must expose an Animator.");
            }

            CinematicSequenceRunner runner = RequireComponent<CinematicSequenceRunner>(runnerObject, "cinematic runner");
            if (runner.SequenceProfile != LoadAsset<CinematicSequenceProfile>(UltimateProfilePath))
            {
                throw new InvalidOperationException("Review runner must bind DB_Cinematic_UltimateCutIn.asset.");
            }

            ValidateRunnerActorBinding(
                runner,
                CinematicSequenceProfile.ActorRole.Environment,
                dragonAnimator,
                dragonSummon.transform);

            CinematicTutorialPromptPresenter promptPresenter =
                RequireComponent<CinematicTutorialPromptPresenter>(runnerObject, "cinematic tutorial prompt presenter");
            if (runner.TutorialPromptPresenter != promptPresenter)
            {
                throw new InvalidOperationException("Review runner must bind CinematicTutorialPromptPresenter.");
            }

            CinematicSequenceAutoPlay autoPlay = RequireComponent<CinematicSequenceAutoPlay>(runnerObject, "cinematic auto play");
            SerializedObject serializedAutoPlay = new SerializedObject(autoPlay);
            if (RequireProperty(serializedAutoPlay, "playOnStart").boolValue)
            {
                throw new InvalidOperationException("Single-profile CinematicSequenceAutoPlay must be disabled when the P0 playlist is active.");
            }

            CinematicSequencePlaylistRunner playlistRunner =
                RequireComponent<CinematicSequencePlaylistRunner>(runnerObject, "cinematic playlist runner");
            if (playlistRunner.EntryCount != 6)
            {
                throw new InvalidOperationException($"P0 playlist runner must contain 6 entries, but has {playlistRunner.EntryCount}.");
            }

            ValidatePlaylistOrder(playlistRunner);
            GameObject cameraObject = FindRoot(scene, CameraName)
                ?? throw new InvalidOperationException("Missing Main Camera.");
            Camera camera = RequireComponent<Camera>(cameraObject, "review camera");
            RequireComponent<ActionCameraController>(cameraObject, "action camera controller");
            ValidateReviewCameraStartsOnOpeningShot(camera, inori.transform);
            if (FindRoot(scene, EnemyRootName) == null)
            {
                throw new InvalidOperationException($"Missing {EnemyRootName}.");
            }

            GameObject stageRoot = FindRoot(scene, StageRootName)
                ?? throw new InvalidOperationException($"Missing {StageRootName}.");
            if (FindDescendant(stageRoot.transform, "CinematicP0Review_BackScreen") == null
                || FindDescendant(stageRoot.transform, "CinematicP0Review_PlayerReadabilityField") == null)
            {
                throw new InvalidOperationException("Review scene is missing required cinematic stage dressing.");
            }
        }

        private static void ValidateRunnerActorBinding(
            CinematicSequenceRunner runner,
            CinematicSequenceProfile.ActorRole expectedRole,
            Animator expectedBodyAnimator,
            Transform expectedAnchor)
        {
            SerializedProperty bindings = RequireProperty(new SerializedObject(runner), "actorBindings");
            for (int i = 0; i < bindings.arraySize; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                if (binding.FindPropertyRelative("role").enumValueIndex != (int)expectedRole)
                {
                    continue;
                }

                UnityEngine.Object bodyAnimator =
                    binding.FindPropertyRelative("bodyAnimator").objectReferenceValue;
                UnityEngine.Object anchor =
                    binding.FindPropertyRelative("anchor").objectReferenceValue;
                if (bodyAnimator != expectedBodyAnimator)
                {
                    throw new InvalidOperationException(
                        $"Review runner binds {expectedRole} to {bodyAnimator}, expected {expectedBodyAnimator}.");
                }

                if (anchor != expectedAnchor)
                {
                    throw new InvalidOperationException(
                        $"Review runner binds {expectedRole} anchor to {anchor}, expected {expectedAnchor}.");
                }

                return;
            }

            throw new InvalidOperationException($"Review runner is missing an actor binding for {expectedRole}.");
        }

        private static void ValidatePlaylistOrder(CinematicSequencePlaylistRunner playlistRunner)
        {
            SerializedObject serializedPlaylist = new SerializedObject(playlistRunner);
            SerializedProperty entries = RequireProperty(serializedPlaylist, "entries");
            RequirePlaylistEntry(entries, 0, IntroProfilePath);
            RequirePlaylistEntry(entries, 1, QteProfilePath);
            RequirePlaylistEntry(entries, 2, UltimateProfilePath);
            RequirePlaylistEntry(entries, 3, DangerProfilePath);
            RequirePlaylistEntry(entries, 4, TutorialProfilePath);
            RequirePlaylistEntry(entries, 5, GameplayHandoffProfilePath);
        }

        private static void RequirePlaylistEntry(SerializedProperty entries, int index, string expectedProfilePath)
        {
            if (entries == null || entries.arraySize <= index)
            {
                throw new InvalidOperationException($"P0 playlist is missing entry {index}.");
            }

            CinematicSequenceProfile expected = LoadAsset<CinematicSequenceProfile>(expectedProfilePath);
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            UnityEngine.Object actual = entry.FindPropertyRelative("profile").objectReferenceValue;
            if (actual != expected)
            {
                throw new InvalidOperationException($"P0 playlist entry {index} must be {expected.name}.");
            }
        }

        public static void CaptureUltimateChargePreview()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            CaptureStatePreview(scene, "CIN_IntroLookAtHands", "Confused", OpeningFacePreviewCapturePath, "capsule_wakeup_first_person", IntroProfilePath, 0.1f);
            CaptureStatePreview(scene, "CIN_UltimateRelease", "Angry", PreviewCapturePath, "ultimate_release_hit", UltimateProfilePath, 1.95f);
            CaptureStatePreview(scene, "CIN_IntroLookAtHands", "Confused", "C:/tmp/DimensionBrawl-CinematicP0Review-IntroAwakening.png", "capsule_open_body_reveal", IntroProfilePath, 10.2f);
            CaptureStatePreview(scene, "CIN_QTEMagicShot", "Angry", "C:/tmp/DimensionBrawl-CinematicP0Review-QTEAssist.png", "assist_hit_confirm", QteProfilePath, 1.65f);
            CaptureStatePreview(scene, "CIN_CombatReady", "Surprised", "C:/tmp/DimensionBrawl-CinematicP0Review-DangerCue.png", "danger_threat_reframe", DangerProfilePath, 0.2f);
            CaptureStatePreview(scene, "CIN_CombatReady", "CalmEye", "C:/tmp/DimensionBrawl-CinematicP0Review-TutorialOverlay.png", "tutorial_basic_attack_focus", TutorialProfilePath, 0.2f);
            Debug.Log("Captured cinematic P0 module previews to C:/tmp.");
        }

        public static void CapturePlaylistStrip()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            PlaylistStripSample[] samples = CreatePlaylistStripSamples();
            string[] framePaths = new string[samples.Length];
            Directory.CreateDirectory(PlaylistFrameDirectory);

            for (int i = 0; i < samples.Length; i++)
            {
                PlaylistStripSample sample = samples[i];
                string framePath = Path.Combine(PlaylistFrameDirectory, $"{i + 1:00}_{sample.Slug}.png")
                    .Replace('\\', '/');
                CaptureStatePreview(
                    scene,
                    sample.StateName,
                    sample.ExpressionName,
                    framePath,
                    sample.CameraCueId,
                    sample.ProfilePath,
                    sample.SampleSeconds);
                framePaths[i] = framePath;
            }

            CreatePlaylistContactSheet(framePaths, samples, PlaylistStripCapturePath, 320, 180, 3);
            WritePlaylistStripReport(samples, framePaths);
            Debug.Log($"Captured cinematic P0 playlist strip to {PlaylistStripCapturePath}.");
        }

        public static void CaptureRunnerDrivenPlaylistStrip()
        {
            CaptureRunnerDrivenStrip(
                CreateRunnerDrivenPlaylistSamples(),
                RunnerDrivenFrameDirectory,
                RunnerDrivenStripCapturePath,
                RunnerDrivenStripReportPath,
                "DimensionBrawl Cinematic P0 Runner-Driven Strip",
                "P0");
        }

        public static void CaptureP1RunnerDrivenPlaylistStrip()
        {
            CaptureRunnerDrivenStrip(
                CreateP1RunnerDrivenPlaylistSamples(),
                P1RunnerDrivenFrameDirectory,
                P1RunnerDrivenStripCapturePath,
                P1RunnerDrivenStripReportPath,
                "DimensionBrawl Cinematic P1 Runner-Driven Strip",
                "P1");
        }

        private static void CaptureRunnerDrivenStrip(
            PlaylistStripSample[] samples,
            string frameDirectory,
            string stripCapturePath,
            string stripReportPath,
            string reportTitle,
            string reviewScopeLabel)
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            CinematicSequenceRunner runner = FindComponentInScene<CinematicSequenceRunner>(scene)
                ?? throw new InvalidOperationException("Cannot capture runner-driven strip without CinematicSequenceRunner.");
            Camera camera = runner.CinematicCamera
                ?? throw new InvalidOperationException("Cannot capture runner-driven strip without runner camera.");
            CombatVfxCuePlayer cuePlayer = FindComponentInScene<CombatVfxCuePlayer>(scene);
            GameObject inori = FindRoot(scene, ActorRootName)
                ?? throw new InvalidOperationException($"Cannot capture runner-driven strip without {ActorRootName}.");
            GameObject summonActor = FindRoot(scene, SummonActorRootName);
            GameObject dragonSummon = FindRoot(scene, DragonSummonRootName);
            Animator animator = inori.GetComponentInChildren<Animator>(includeInactive: true)
                ?? throw new InvalidOperationException("Cannot capture runner-driven strip without Inori Animator.");

            RunnerDrivenSampleResult[] results = new RunnerDrivenSampleResult[samples.Length];
            string[] framePaths = new string[samples.Length];
            Directory.CreateDirectory(frameDirectory);

            for (int i = 0; i < samples.Length; i++)
            {
                PlaylistStripSample sample = samples[i];
                ResetActorForRunnerDrivenSample(inori.transform, animator);
                PrepareSummonActorsForRunnerDrivenSample(
                    summonActor,
                    dragonSummon,
                    IsSummonReviewProfile(sample.ProfilePath));
                ResetSceneEffectsForRunnerDrivenSample(scene, cuePlayer);
                CinematicSequenceProfile profile = LoadAsset<CinematicSequenceProfile>(sample.ProfilePath);
                if (!runner.TryApplyProfileSampleForReview(profile, sample.SampleSeconds, Vector3.forward))
                {
                    throw new InvalidOperationException($"Runner failed to apply sample {sample.Label}.");
                }

                UpdateSceneAnimators(scene, 0.12f);
                string framePath = Path.Combine(frameDirectory, $"{i + 1:00}_{sample.Slug}.png")
                    .Replace('\\', '/');
                CaptureCamera(camera, framePath, 1280, 720);
                framePaths[i] = framePath;
                results[i] = new RunnerDrivenSampleResult(
                    runner.LastCameraCueId,
                    runner.LastActorCueId,
                    runner.LastVfxCueId,
                    runner.LastTutorialCueId,
                    runner.TotalCameraCueCount,
                    runner.TotalActorCueCount,
                    runner.TotalVfxCueCount,
                    runner.TotalTutorialCueCount,
                    runner.GameplayHandoffReached);
            }

            CreatePlaylistContactSheet(framePaths, samples, stripCapturePath, 320, 180, 3);
            WriteRunnerDrivenStripReport(
                samples,
                results,
                framePaths,
                stripCapturePath,
                stripReportPath,
                reportTitle,
                reviewScopeLabel);
            Debug.Log($"Captured runner-driven cinematic {reviewScopeLabel} playlist strip to {stripCapturePath}.");
        }

        private static PlaylistStripSample[] CreatePlaylistStripSamples()
        {
            return new[]
            {
                new PlaylistStripSample(
                    "Intro start: face/front wake shot",
                    "intro_01_wake_front",
                    IntroProfilePath,
                    "CIN_IntroLookAtHands",
                    "Confused",
                    "capsule_wakeup_first_person",
                    0.1f,
                    weaponVisible: false),
                new PlaylistStripSample(
                    "Intro middle: body reveal",
                    "intro_02_body_reveal",
                    IntroProfilePath,
                    "CIN_IntroSurprised",
                    "Surprised",
                    "capsule_open_body_reveal",
                    10.95f,
                    weaponVisible: false),
                new PlaylistStripSample(
                    "Intro end: rifle ready payoff",
                    "intro_03_rifle_pickup",
                    IntroProfilePath,
                    "CIN_CombatReady",
                    "Angry",
                    "gun_pickup_action",
                    26.8f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "QTE assist: hit confirm",
                    "qte_01_hit_confirm",
                    QteProfilePath,
                    "CIN_QTEMagicShot",
                    "Angry",
                    "assist_hit_confirm",
                    1.65f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Ultimate start: focus close",
                    "ultimate_01_focus",
                    UltimateProfilePath,
                    "CIN_QTEMagicShot",
                    "CalmEye",
                    "ultimate_focus_close",
                    0.1f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Ultimate release: impact beat",
                    "ultimate_02_release",
                    UltimateProfilePath,
                    "CIN_UltimateRelease",
                    "Angry",
                    "ultimate_release_hit",
                    1.95f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Danger cue: threat reframe",
                    "danger_01_reframe",
                    DangerProfilePath,
                    "CIN_CombatReady",
                    "Surprised",
                    "danger_threat_reframe",
                    0.2f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Tutorial cue: basic attack focus",
                    "tutorial_01_basic_attack",
                    TutorialProfilePath,
                    "CIN_CombatReady",
                    "CalmEye",
                    "tutorial_basic_attack_focus",
                    0.2f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Gameplay handoff: readable match camera",
                    "handoff_01_rear_match",
                    GameplayHandoffProfilePath,
                    "CIN_CombatReady",
                    "CalmEye",
                    "combat_ready_rear_match",
                    0.2f,
                    weaponVisible: true)
            };
        }

        private static PlaylistStripSample[] CreateRunnerDrivenPlaylistSamples()
        {
            return new[]
            {
                new PlaylistStripSample(
                    "Intro start: runner wake body/face cues",
                    "intro_01_runner_wake",
                    IntroProfilePath,
                    "CIN_IntroLookAtHands",
                    "Surprised",
                    "capsule_wakeup_first_person",
                    1.35f,
                    weaponVisible: false),
                new PlaylistStripSample(
                    "Intro middle: runner body reveal",
                    "intro_02_runner_body_reveal",
                    IntroProfilePath,
                    "CIN_IntroSurprised",
                    "Surprised",
                    "capsule_open_body_reveal",
                    10.95f,
                    weaponVisible: false),
                new PlaylistStripSample(
                    "Intro end: runner rifle ready payoff",
                    "intro_03_runner_rifle_ready",
                    IntroProfilePath,
                    "CIN_CombatReady",
                    "Angry",
                    "gun_pickup_action",
                    26.85f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "QTE assist: runner hit confirm",
                    "qte_01_runner_hit_confirm",
                    QteProfilePath,
                    "CIN_QTEMagicShot",
                    "Angry",
                    "assist_hit_confirm",
                    1.70f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Ultimate start: runner focus close",
                    "ultimate_01_runner_focus",
                    UltimateProfilePath,
                    "CIN_QTEMagicShot",
                    "CalmEye",
                    "ultimate_focus_close",
                    0.65f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Ultimate release: runner impact beat",
                    "ultimate_02_runner_release",
                    UltimateProfilePath,
                    "CIN_UltimateRelease",
                    "Angry",
                    "ultimate_release_hit",
                    1.95f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Danger cue: runner threat reframe",
                    "danger_01_runner_reframe",
                    DangerProfilePath,
                    "CIN_CombatReady",
                    "Surprised",
                    "danger_threat_reframe",
                    0.45f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Tutorial cue: runner basic attack focus",
                    "tutorial_01_runner_basic_attack",
                    TutorialProfilePath,
                    "CIN_CombatReady",
                    "CalmEye",
                    "tutorial_basic_attack_focus",
                    0.25f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Gameplay handoff: runner readable match",
                    "handoff_01_runner_match",
                    GameplayHandoffProfilePath,
                    "CIN_CombatReady",
                    "CalmEye",
                    "combat_ready_rear_match",
                    0.25f,
                    weaponVisible: true)
            };
        }

        private static PlaylistStripSample[] CreateP1RunnerDrivenPlaylistSamples()
        {
            return new[]
            {
                new PlaylistStripSample(
                    "Boss intro: Inori reaction",
                    "p1_01_boss_intro_inori_reaction",
                    BossIntroProfilePath,
                    "CIN_IntroSurprised",
                    "Surprised",
                    "inori_boss_reaction",
                    1.85f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Phase transition: counter release",
                    "p1_02_phase_counter_release",
                    PhaseTransitionProfilePath,
                    "CIN_BackViewProjectileFire",
                    "Angry",
                    "phase_counter_release",
                    2.34f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Break moment: hit confirm",
                    "p1_03_break_hit_confirm",
                    BreakMomentProfilePath,
                    "CIN_BackViewProjectileFire",
                    "Angry",
                    "break_hit_confirm",
                    0.98f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Dialogue reaction: answer shift",
                    "p1_04_dialogue_answer_shift",
                    DialogueReactionBeatProfilePath,
                    "CIN_IntroSurprised",
                    "Surprised",
                    "dialogue_answer_shift",
                    1.35f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Result bridge: settle",
                    "p1_05_result_settle",
                    ResultBridgeProfilePath,
                    "CIN_ResultSettle",
                    "CalmEye",
                    "result_inori_settle",
                    1.45f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Summon entry: proxy hit",
                    "p1_06_summon_proxy_hit",
                    SummonEntryProfilePath,
                    "CIN_BackViewProjectileBurst + Summon.Attack",
                    "Angry",
                    "summon_proxy_hit",
                    2.24f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Summon follow-up: clash",
                    "p1_07_summon_followup_clash",
                    SummonFollowupHitProfilePath,
                    "CIN_BackViewProjectileFire + Summon.Attack",
                    "Angry",
                    "summon_followup_clash",
                    0.96f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Summon empower: transfer",
                    "p1_08_summon_empower_transfer",
                    SummonEmpowerProfilePath,
                    "CIN_BackViewProjectileFire + Summon.Attack",
                    "Angry",
                    "summon_empower_transfer",
                    1.12f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Summon recall: collapse",
                    "p1_09_summon_recall_collapse",
                    SummonRecallProfilePath,
                    "CIN_BackViewProjectileRecover + Summon.Attack",
                    "CalmEye",
                    "summon_recall_collapse",
                    1.08f,
                    weaponVisible: true),
                new PlaylistStripSample(
                    "Boss summon pressure: guard",
                    "p1_10_boss_summon_pressure_guard",
                    BossSummonPressureProfilePath,
                    "CIN_BackViewProjectileFire + Summon.Attack",
                    "Angry",
                    "boss_summon_pressure_guard",
                    1.02f,
                    weaponVisible: true)
            };
        }

        private static bool IsSummonReviewProfile(string profilePath)
        {
            return string.Equals(profilePath, SummonEntryProfilePath, StringComparison.Ordinal)
                || string.Equals(profilePath, SummonFollowupHitProfilePath, StringComparison.Ordinal)
                || string.Equals(profilePath, SummonEmpowerProfilePath, StringComparison.Ordinal)
                || string.Equals(profilePath, SummonRecallProfilePath, StringComparison.Ordinal)
                || string.Equals(profilePath, BossSummonPressureProfilePath, StringComparison.Ordinal);
        }

        private static void ResetActorForRunnerDrivenSample(Transform actorRoot, Animator animator)
        {
            animator.Rebind();
            animator.Update(0.01f);
            CinematicBlendShapeExpressionPlayer expressionPlayer =
                actorRoot.GetComponentInChildren<CinematicBlendShapeExpressionPlayer>(includeInactive: true);
            expressionPlayer?.ApplyExpressionImmediate("CalmEye");

            Transform rifle = FindDescendant(actorRoot, RifleName) ?? FindDescendantContains(actorRoot, RifleName);
            if (rifle != null)
            {
                rifle.gameObject.SetActive(false);
            }
        }

        private static void ResetSceneEffectsForRunnerDrivenSample(Scene scene, CombatVfxCuePlayer cuePlayer)
        {
            cuePlayer?.StopAllActiveCuesForReview();

            CombatVfxCueVisual[] cueVisuals = UnityEngine.Object.FindObjectsByType<CombatVfxCueVisual>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < cueVisuals.Length; i++)
            {
                CombatVfxCueVisual cueVisual = cueVisuals[i];
                if (cueVisual != null && cueVisual.gameObject.scene == scene)
                {
                    cueVisual.StopNow();
                }
            }

            ParticleSystem[] particles = UnityEngine.Object.FindObjectsByType<ParticleSystem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < particles.Length; i++)
            {
                ParticleSystem particle = particles[i];
                if (particle == null || particle.gameObject.scene != scene)
                {
                    continue;
                }

                particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Clear(withChildren: true);
            }
        }

        private static void UpdateSceneAnimators(Scene scene, float deltaSeconds)
        {
            Animator[] animators = UnityEngine.Object.FindObjectsByType<Animator>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator == null || animator.gameObject.scene != scene)
                {
                    continue;
                }

                animator.Update(Mathf.Max(0f, deltaSeconds));
            }
        }

        public static void ConfigurePlayModeRouteCaptureProbe(Scene scene)
        {
            GameObject existing = FindRoot(scene, PlayModeCaptureProbeName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            GameObject probeObject = new GameObject(PlayModeCaptureProbeName);
            SceneManager.MoveGameObjectToScene(probeObject, scene);
            CinematicPlaylistPlayModeCaptureProbe probe =
                probeObject.AddComponent<CinematicPlaylistPlayModeCaptureProbe>();

            SerializedObject serializedProbe = new SerializedObject(probe);
            RequireProperty(serializedProbe, "outputDirectory").stringValue = PlayModeRouteFrameDirectory;
            RequireProperty(serializedProbe, "stripPath").stringValue = PlayModeRouteStripCapturePath;
            RequireProperty(serializedProbe, "reportPath").stringValue = PlayModeRouteReportPath;
            RequireProperty(serializedProbe, "resultPath").stringValue = PlayModeRouteResultPath;
            RequireProperty(serializedProbe, "captureTimeline").boolValue = true;
            RequireProperty(serializedProbe, "timelineDirectory").stringValue = PlayModeTimelineFrameDirectory;
            RequireProperty(serializedProbe, "timelineStripPath").stringValue = PlayModeTimelineStripCapturePath;
            RequireProperty(serializedProbe, "timelineReportPath").stringValue = PlayModeTimelineReportPath;
            RequireProperty(serializedProbe, "timelineIntervalSeconds").floatValue = 2.5f;
            RequireProperty(serializedProbe, "minimumTimelineFrameCount").intValue = 12;
            RequireProperty(serializedProbe, "timelineCaptureWidth").intValue = 640;
            RequireProperty(serializedProbe, "timelineCaptureHeight").intValue = 360;
            RequireProperty(serializedProbe, "timelineStripColumns").intValue = 5;
            RequireProperty(serializedProbe, "captureWidth").intValue = 1280;
            RequireProperty(serializedProbe, "captureHeight").intValue = 720;
            RequireProperty(serializedProbe, "maxRouteSeconds").floatValue = 62f;

            SerializedProperty samples = RequireProperty(serializedProbe, "samples");
            samples.arraySize = 9;
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(0), "01_intro_wake", 1.35f, "intro_awakening", "capsule_wakeup_first_person", false);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(1), "02_intro_body_reveal", 10.95f, "intro_awakening", "capsule_open_body_reveal", false);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(2), "03_intro_rifle_ready", 26.85f, "intro_awakening", "gun_pickup_action", true);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(3), "04_qte_hit_confirm", 41.05f, "qte_assist", "assist_hit_confirm", true);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(4), "05_ultimate_release", 44.75f, "ultimate_cutin", "ultimate_release_hit", true);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(5), "06_danger_warning", 47.25f, "danger_cue", "danger_threat_reframe", true);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(6), "07_tutorial_basic", 49.10f, "combat_tutorial_overlay", "tutorial_basic_attack_focus", true);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(7), "08_tutorial_skill", 50.50f, "combat_tutorial_overlay", "tutorial_skill_focus", true);
            ConfigurePlayModeSample(samples.GetArrayElementAtIndex(8), "09_handoff_match", 53.55f, "gameplay_handoff", "combat_ready_rear_match", true);
            serializedProbe.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(probe);

            if (EditorApplication.isPlaying)
            {
                probe.BeginCapture();
            }
        }

        private static void ConfigurePlayModeSample(
            SerializedProperty sample,
            string label,
            float routeSeconds,
            string expectedProfileId,
            string expectedCameraCueId,
            bool expectedWeaponVisible)
        {
            sample.FindPropertyRelative("label").stringValue = label;
            sample.FindPropertyRelative("routeSeconds").floatValue = routeSeconds;
            sample.FindPropertyRelative("expectedProfileId").stringValue = expectedProfileId;
            sample.FindPropertyRelative("expectedCameraCueId").stringValue = expectedCameraCueId;
            sample.FindPropertyRelative("expectedWeaponVisible").boolValue = expectedWeaponVisible;
        }

        public static void CaptureStatePreviewSet()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            CaptureStatePreview(scene, "CIN_CombatReady", "CalmEye", "C:/tmp/DimensionBrawl-CinematicState-CIN_CombatReady.png");
            CaptureStatePreview(scene, "CIN_QTEMagicShot", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_QTEMagicShot.png");
            CaptureStatePreview(scene, "CIN_UltimateRelease", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_UltimateRelease.png");
            CaptureStatePreview(scene, "CIN_UltimateImpact", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_UltimateImpact.png");
            CaptureStatePreview(scene, "CIN_UltimateRecover", "CalmEye", "C:/tmp/DimensionBrawl-CinematicState-CIN_UltimateRecover.png");
            CaptureStatePreview(scene, "CIN_SwordCharge", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_SwordCharge.png");
            CaptureStatePreview(scene, "CIN_IntroPickUp", "Confused", "C:/tmp/DimensionBrawl-CinematicState-CIN_IntroPickUp.png");
            CaptureStatePreview(scene, "CIN_TwinSwordIdle", "CalmEye", "C:/tmp/DimensionBrawl-CinematicState-CIN_TwinSwordIdle.png");
            CaptureStatePreview(scene, "CIN_BossIntroReady", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_BossIntroReady.png");
            CaptureStatePreview(scene, "CIN_BossIntroAnswer", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_BossIntroAnswer.png");
            CaptureStatePreview(scene, "CIN_PhaseCounterRelease", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_PhaseCounterRelease.png");
            CaptureStatePreview(scene, "CIN_BreakHitConfirm", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_BreakHitConfirm.png");
            CaptureStatePreview(scene, "CIN_SummonProxyHit", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_SummonProxyHit.png");
            CaptureStatePreview(scene, "CIN_ResultSettle", "CalmEye", "C:/tmp/DimensionBrawl-CinematicState-CIN_ResultSettle.png");
            CaptureStatePreview(scene, "CIN_BackViewProjectileAim", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_BackViewProjectileAim.png");
            CaptureStatePreview(scene, "CIN_BackViewProjectileCharge", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_BackViewProjectileCharge.png");
            CaptureStatePreview(scene, "CIN_BackViewProjectileFire", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_BackViewProjectileFire.png");
            CaptureStatePreview(scene, "CIN_BackViewProjectileBurst", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_BackViewProjectileBurst.png");
            CaptureStatePreview(scene, "CIN_BackViewProjectileRecover", "CalmEye", "C:/tmp/DimensionBrawl-CinematicState-CIN_BackViewProjectileRecover.png");
            CaptureStatePreview(scene, "CIN_LucyAmbushAnswer", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_LucyAmbushAnswer.png");
            CaptureStatePreview(scene, "CIN_LucyExecutionFinisher", "Angry", "C:/tmp/DimensionBrawl-CinematicState-CIN_LucyExecutionFinisher.png");
            Debug.Log("Captured cinematic state preview set to C:/tmp.");
        }

        private static void CaptureStatePreview(
            Scene scene,
            string stateName,
            string expressionName,
            string outputPath,
            string cameraCueId = "",
            string profilePath = UltimateProfilePath,
            float sampleSeconds = 0.5f)
        {
            GameObject cameraObject = FindRoot(scene, CameraName)
                ?? throw new InvalidOperationException("Cannot capture preview without Main Camera.");
            Camera camera = RequireComponent<Camera>(cameraObject, "preview camera");
            GameObject inori = FindRoot(scene, ActorRootName)
                ?? throw new InvalidOperationException($"Cannot capture preview without {ActorRootName}.");
            Animator animator = inori.GetComponentInChildren<Animator>(includeInactive: true)
                ?? throw new InvalidOperationException("Cannot capture preview without Inori Animator.");
            CinematicBlendShapeExpressionPlayer expressionPlayer =
                inori.GetComponentInChildren<CinematicBlendShapeExpressionPlayer>(includeInactive: true);

            animator.Rebind();
            animator.Update(0.01f);
            animator.Play(stateName, 0, 0.45f);
            animator.Update(0.08f);
            expressionPlayer?.ApplyExpressionImmediate(expressionName);
            ApplyPreviewActorCues(inori.transform, profilePath, sampleSeconds);
            ApplyPreviewCameraPose(camera, inori.transform, cameraCueId, profilePath);
            CaptureCamera(camera, outputPath, 1280, 720);
        }

        private static void ApplyPreviewActorCues(Transform actorRoot, string profilePath, float sampleSeconds)
        {
            if (actorRoot == null)
            {
                return;
            }

            CinematicSequenceProfile profile = LoadAsset<CinematicSequenceProfile>(profilePath);
            CinematicSequenceProfile.ActorCue[] actorCues = profile.ActorCues;
            for (int i = 0; i < actorCues.Length; i++)
            {
                CinematicSequenceProfile.ActorCue cue = actorCues[i];
                if (!cue.Enabled
                    || cue.CueKind != CinematicSequenceProfile.ActorCueKind.WeaponVisibility
                    || cue.StartSeconds > sampleSeconds)
                {
                    continue;
                }

                Transform target = FindDescendant(actorRoot, cue.SocketPath) ?? FindDescendantContains(actorRoot, cue.SocketPath);
                if (target != null)
                {
                    target.gameObject.SetActive(cue.ObjectActive);
                }
            }
        }

        private static void ApplyPreviewCameraPose(Camera camera, Transform cueSpace, string cameraCueId, string profilePath)
        {
            if (camera == null || cueSpace == null || string.IsNullOrWhiteSpace(cameraCueId))
            {
                return;
            }

            if (!TryResolveProfileCameraPose(
                profilePath,
                cueSpace,
                cameraCueId,
                camera.transform.forward,
                out Vector3 cameraPosition,
                out Quaternion cameraRotation,
                out float fieldOfView))
            {
                return;
            }

            camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
            camera.fieldOfView = fieldOfView;
        }

        private static void ApplyInitialReviewCameraPose(Camera camera, Transform cueSpace)
        {
            ApplyPreviewCameraPose(camera, cueSpace, "capsule_wakeup_first_person", IntroProfilePath);
            if (camera != null)
            {
                EditorUtility.SetDirty(camera);
            }
        }

        private static void ValidateReviewCameraStartsOnOpeningShot(Camera camera, Transform cueSpace)
        {
            if (camera == null)
            {
                throw new InvalidOperationException("Cannot validate opening camera pose without Main Camera.");
            }

            if (!TryResolveProfileCameraPose(
                IntroProfilePath,
                cueSpace,
                "capsule_wakeup_first_person",
                camera.transform.forward,
                out Vector3 expectedPosition,
                out Quaternion expectedRotation,
                out float expectedFieldOfView))
            {
                throw new InvalidOperationException("Cannot resolve ultimate_focus_close shot pose.");
            }

            float positionError = Vector3.Distance(camera.transform.position, expectedPosition);
            float rotationError = Quaternion.Angle(camera.transform.rotation, expectedRotation);
            float fovError = Mathf.Abs(camera.fieldOfView - expectedFieldOfView);
            if (positionError > 0.03f || rotationError > 0.5f || fovError > 0.05f)
            {
                throw new InvalidOperationException(
                    $"Review camera must start on capsule_wakeup_first_person. Position error {positionError:F3}, rotation error {rotationError:F3}, FOV error {fovError:F3}.");
            }
        }

        private static bool TryResolveProfileCameraPose(
            string profilePath,
            Transform cueSpace,
            string cameraCueId,
            Vector3 fallbackForward,
            out Vector3 cameraPosition,
            out Quaternion cameraRotation,
            out float fieldOfView)
        {
            cameraPosition = default;
            cameraRotation = default;
            fieldOfView = default;
            if (cueSpace == null || string.IsNullOrWhiteSpace(cameraCueId))
            {
                return false;
            }

            CinematicSequenceProfile profile = LoadAsset<CinematicSequenceProfile>(profilePath);
            CinematicSequenceProfile.CameraCue[] cameraCues = profile.CameraCues;
            for (int i = 0; i < cameraCues.Length; i++)
            {
                CinematicSequenceProfile.CameraCue cue = cameraCues[i];
                if (!cue.Enabled || !cue.DriveCameraPose || !string.Equals(cue.CueId, cameraCueId, StringComparison.Ordinal))
                {
                    continue;
                }

                cameraPosition = cueSpace.TransformPoint(cue.CameraLocalPosition);
                Vector3 lookAtPosition = cueSpace.TransformPoint(cue.LookAtLocalPosition);
                Vector3 forward = lookAtPosition - cameraPosition;
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = fallbackForward.sqrMagnitude > 0.0001f ? fallbackForward : Vector3.forward;
                }

                cameraRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
                fieldOfView = cue.FieldOfView > 0f ? cue.FieldOfView : 34f;
                return true;
            }

            return false;
        }

        private static GameObject CreateInoriActor(Scene scene)
        {
            GameObject sourcePrefab = LoadAsset<GameObject>(InoriSourcePrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("Failed to instantiate Inori source prefab.");
            }

            PrefabUtility.UnpackPrefabInstance(
                instance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            instance.name = ActorRootName;
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;

            Animator animator = instance.GetComponentInChildren<Animator>(includeInactive: true)
                ?? instance.AddComponent<Animator>();
            animator.runtimeAnimatorController = LoadAsset<RuntimeAnimatorController>(
                BuildResubmissionCinematicAnimationSetup.CinematicControllerPath);
            animator.avatar = ActionFoundationInoriPlayerVisualAssetSetup.LoadPromotedAvatar();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            AssignInoriPromotedMaterials(instance);
            return instance;
        }

        private static CinematicBlendShapeExpressionPlayer ConfigureExpressionPlayer(GameObject inori)
        {
            CinematicBlendShapeExpressionPlayer player =
                inori.GetComponent<CinematicBlendShapeExpressionPlayer>()
                ?? inori.AddComponent<CinematicBlendShapeExpressionPlayer>();
            player.Configure(CreateInoriExpressionPresets());
            EditorUtility.SetDirty(player);
            return player;
        }

        private static CinematicSequenceRunner CreateRunner(
            Scene scene,
            GameObject inori,
            Animator inoriAnimator,
            CinematicBlendShapeExpressionPlayer expressionPlayer,
            GameObject summonActor,
            Animator summonAnimator,
            GameObject dragonSummon,
            Animator dragonAnimator,
            ActionCameraController cameraController,
            CombatVfxCueProfile vfxCueProfile)
        {
            GameObject runnerObject = new GameObject(RunnerRootName);
            SceneManager.MoveGameObjectToScene(runnerObject, scene);
            runnerObject.transform.position = Vector3.zero;

            GameObject poolRoot = new GameObject("CinematicP0Review_VfxPool");
            poolRoot.transform.SetParent(runnerObject.transform, worldPositionStays: false);

            CombatVfxCuePlayer cuePlayer = runnerObject.AddComponent<CombatVfxCuePlayer>();
            SetObjectReference(cuePlayer, "profile", vfxCueProfile);
            SetObjectReference(cuePlayer, "pooledRoot", poolRoot.transform);

            CinematicTutorialPromptPresenter promptPresenter =
                runnerObject.AddComponent<CinematicTutorialPromptPresenter>();
            ConfigureTutorialPromptPresenter(promptPresenter, cameraController.GetComponent<Camera>());

            CinematicSequenceRunner runner = runnerObject.AddComponent<CinematicSequenceRunner>();
            SerializedObject serializedRunner = new SerializedObject(runner);
            SetObjectReference(serializedRunner, "sequenceProfile", LoadAsset<CinematicSequenceProfile>(UltimateProfilePath));
            SetObjectReference(
                serializedRunner,
                "bodyControllerOverride",
                LoadAsset<RuntimeAnimatorController>(BuildResubmissionCinematicAnimationSetup.CinematicControllerPath));
            SetObjectReference(serializedRunner, "cameraController", cameraController);
            SetObjectReference(serializedRunner, "cinematicCamera", cameraController.GetComponent<Camera>());
            RequireProperty(serializedRunner, "driveCameraTransformFromProfile").boolValue = true;
            RequireProperty(serializedRunner, "disableActionCameraControllerDuringPoseDrive").boolValue = true;
            SetObjectReference(serializedRunner, "combatVfxCuePlayer", cuePlayer);
            SetObjectReference(serializedRunner, "tutorialPromptPresenter", promptPresenter);
            SetObjectReference(serializedRunner, "cueSpace", inori.transform);
            SerializedProperty bindings = RequireProperty(serializedRunner, "actorBindings");
            int bindingCount = 1;
            if (summonAnimator != null)
            {
                bindingCount++;
            }

            if (dragonAnimator != null)
            {
                bindingCount++;
            }

            bindings.arraySize = bindingCount;
            SerializedProperty binding = bindings.GetArrayElementAtIndex(0);
            SetRelativeEnum(binding, "role", (int)CinematicSequenceProfile.ActorRole.Inori);
            SetRelativeObjectReference(binding, "bodyAnimator", inoriAnimator);
            SetRelativeObjectReference(binding, "faceAnimator", null);
            SetRelativeObjectReference(binding, "expressionPlayer", expressionPlayer);
            SetRelativeObjectReference(binding, "anchor", inori.transform);
            if (summonAnimator != null)
            {
                SerializedProperty summonBinding = bindings.GetArrayElementAtIndex(1);
                SetRelativeEnum(summonBinding, "role", (int)CinematicSequenceProfile.ActorRole.Summon);
                SetRelativeObjectReference(summonBinding, "bodyAnimator", summonAnimator);
                SetRelativeObjectReference(summonBinding, "faceAnimator", null);
                SetRelativeObjectReference(summonBinding, "expressionPlayer", null);
                SetRelativeObjectReference(summonBinding, "anchor", summonActor != null ? summonActor.transform : summonAnimator.transform);
            }

            if (dragonAnimator != null)
            {
                SerializedProperty dragonBinding = bindings.GetArrayElementAtIndex(bindings.arraySize - 1);
                SetRelativeEnum(dragonBinding, "role", (int)CinematicSequenceProfile.ActorRole.Environment);
                SetRelativeObjectReference(dragonBinding, "bodyAnimator", dragonAnimator);
                SetRelativeObjectReference(dragonBinding, "faceAnimator", null);
                SetRelativeObjectReference(dragonBinding, "expressionPlayer", null);
                SetRelativeObjectReference(dragonBinding, "anchor", dragonSummon != null ? dragonSummon.transform : dragonAnimator.transform);
            }

            serializedRunner.ApplyModifiedPropertiesWithoutUndo();

            ActionCinematicSequenceBridge sequenceBridge =
                runnerObject.AddComponent<ActionCinematicSequenceBridge>();
            ConfigureActionCinematicSequenceBridge(sequenceBridge, runner);

            CinematicSequenceAutoPlay autoPlay = runnerObject.AddComponent<CinematicSequenceAutoPlay>();
            SetObjectReference(autoPlay, "runner", runner);
            SetBool(autoPlay, "playOnStart", false);
            SetFloat(autoPlay, "startDelaySeconds", 0f);

            CinematicSequencePlaylistRunner playlistRunner = runnerObject.AddComponent<CinematicSequencePlaylistRunner>();
            ConfigurePlaylistRunner(playlistRunner, runner);

            EditorUtility.SetDirty(cuePlayer);
            EditorUtility.SetDirty(promptPresenter);
            EditorUtility.SetDirty(runner);
            EditorUtility.SetDirty(sequenceBridge);
            EditorUtility.SetDirty(autoPlay);
            EditorUtility.SetDirty(playlistRunner);
            return runner;
        }

        private static void ConfigureActionCinematicSequenceBridge(
            ActionCinematicSequenceBridge sequenceBridge,
            CinematicSequenceRunner runner)
        {
            SetObjectReference(sequenceBridge, "runner", runner);
            SetBool(sequenceBridge, "blockLegacyCameraShotsWhenPlayed", true);
            SetBool(sequenceBridge, "blockLegacySignalsWhenPlayed", true);
            SetFloat(sequenceBridge, "minimumLockSeconds", 0.12f);
            SetObjectReference(sequenceBridge, "skillCutInProfile", LoadAsset<CinematicSequenceProfile>(QteProfilePath));
            SetObjectReference(sequenceBridge, "summonEntryProfile", LoadAsset<CinematicSequenceProfile>(SummonEntryProfilePath));
            SetObjectReference(sequenceBridge, "ultimateCutInProfile", LoadAsset<CinematicSequenceProfile>(UltimateProfilePath));
            SetObjectReference(sequenceBridge, "bossPressureBreakProfile", LoadAsset<CinematicSequenceProfile>(BossSummonPressureProfilePath));
            SetObjectReference(sequenceBridge, "summonFollowupHitProfile", LoadAsset<CinematicSequenceProfile>(SummonFollowupHitProfilePath));
            SetObjectReference(sequenceBridge, "summonEmpowerProfile", LoadAsset<CinematicSequenceProfile>(SummonEmpowerProfilePath));
            SetObjectReference(sequenceBridge, "summonRecallProfile", LoadAsset<CinematicSequenceProfile>(SummonRecallProfilePath));
            SetObjectReference(sequenceBridge, "pocketClearProfile", LoadAsset<CinematicSequenceProfile>(ResultBridgeProfilePath));
            SetObjectReference(sequenceBridge, "pocketFailProfile", LoadAsset<CinematicSequenceProfile>(DangerProfilePath));
            SetObjectReference(sequenceBridge, "bossIntroProfile", LoadAsset<CinematicSequenceProfile>(BossIntroProfilePath));
            SetObjectReference(sequenceBridge, "phaseTransitionProfile", LoadAsset<CinematicSequenceProfile>(PhaseTransitionProfilePath));
            SetObjectReference(sequenceBridge, "dialogueReactionBeatProfile", LoadAsset<CinematicSequenceProfile>(DialogueReactionBeatProfilePath));
            EditorUtility.SetDirty(sequenceBridge);
        }

        private static void ConfigureTutorialPromptPresenter(
            CinematicTutorialPromptPresenter promptPresenter,
            Camera camera)
        {
            SerializedObject serializedPrompt = new SerializedObject(promptPresenter);
            SetObjectReference(serializedPrompt, "targetCamera", camera);
            RequireProperty(serializedPrompt, "promptDistance").floatValue = 2.35f;
            RequireProperty(serializedPrompt, "defaultScreenAnchor").vector2Value = new Vector2(0.5f, 0.72f);
            RequireProperty(serializedPrompt, "backdropSize").vector2Value = new Vector2(0.70f, 0.18f);
            RequireProperty(serializedPrompt, "titleCharacterSize").floatValue = 0.014f;
            RequireProperty(serializedPrompt, "guideCharacterSize").floatValue = 0.009f;
            serializedPrompt.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(promptPresenter);
        }

        private static void ConfigurePlaylistRunner(CinematicSequencePlaylistRunner playlistRunner, CinematicSequenceRunner runner)
        {
            SerializedObject serializedPlaylist = new SerializedObject(playlistRunner);
            SetObjectReference(serializedPlaylist, "runner", runner);
            RequireProperty(serializedPlaylist, "playOnStart").boolValue = true;
            RequireProperty(serializedPlaylist, "startDelaySeconds").floatValue = 0f;
            RequireProperty(serializedPlaylist, "loop").boolValue = false;

            SerializedProperty entries = RequireProperty(serializedPlaylist, "entries");
            entries.arraySize = 6;
            ConfigurePlaylistEntry(entries.GetArrayElementAtIndex(0), IntroProfilePath, 0.35f);
            ConfigurePlaylistEntry(entries.GetArrayElementAtIndex(1), QteProfilePath, 0.25f);
            ConfigurePlaylistEntry(entries.GetArrayElementAtIndex(2), UltimateProfilePath, 0.35f);
            ConfigurePlaylistEntry(entries.GetArrayElementAtIndex(3), DangerProfilePath, 0.25f);
            ConfigurePlaylistEntry(entries.GetArrayElementAtIndex(4), TutorialProfilePath, 0.25f);
            ConfigurePlaylistEntry(entries.GetArrayElementAtIndex(5), GameplayHandoffProfilePath, 0f);
            serializedPlaylist.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigurePlaylistEntry(SerializedProperty entry, string profilePath, float delayAfterSeconds)
        {
            entry.FindPropertyRelative("profile").objectReferenceValue = LoadAsset<CinematicSequenceProfile>(profilePath);
            entry.FindPropertyRelative("delayAfterSeconds").floatValue = delayAfterSeconds;
            entry.FindPropertyRelative("usePlanarDirectionOverride").boolValue = false;
            entry.FindPropertyRelative("planarDirectionOverride").vector3Value = Vector3.forward;
        }

        private static ActionCameraController CreateReviewCamera(Scene scene, Transform target, Transform threat)
        {
            GameObject cameraObject = new GameObject(CameraName);
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 34f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 120f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.055f, 0.070f, 1f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 1.42f, -4.5f), Quaternion.Euler(8f, 0f, 0f));

            ActionCameraController cameraController = cameraObject.AddComponent<ActionCameraController>();
            SetObjectReference(cameraController, "target", target);
            SetObjectReference(cameraController, "threat", threat);
            SetVector3(cameraController, "cameraOffset", new Vector3(0f, 1.08f, -4.15f));
            SetVector3(cameraController, "lookOffset", new Vector3(0f, 1.18f, 0.45f));
            return cameraController;
        }

        private static GameObject CreateEnemyTarget(Scene scene)
        {
            GameObject prefab = LoadAsset<GameObject>(EnemyPrefabPath);
            GameObject enemy = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (enemy == null)
            {
                throw new InvalidOperationException("Failed to instantiate review enemy target.");
            }

            enemy.name = EnemyRootName;
            enemy.transform.SetPositionAndRotation(new Vector3(0f, 0f, 5.2f), Quaternion.Euler(0f, 180f, 0f));
            return enemy;
        }

        private static GameObject CreateSummonActor(Scene scene)
        {
            GameObject prefab = LoadAsset<GameObject>(ActionFoundationBossBarrageLaneReviewSetup.SummonSlot3ActorPrefabPath);
            GameObject summonActor = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (summonActor == null)
            {
                throw new InvalidOperationException("Failed to instantiate summon vanguard actor prefab.");
            }

            summonActor.name = SummonActorRootName;
            PrepareSummonProxyForReview(summonActor, visible: true);
            summonActor.SetActive(false);
            EditorUtility.SetDirty(summonActor);
            return summonActor;
        }

        private static GameObject CreateDragonSummonActor(Scene scene)
        {
            GameObject prefab = LoadAsset<GameObject>(DragonSummonSourcePrefabPath);
            GameObject dragon = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (dragon == null)
            {
                throw new InvalidOperationException("Failed to instantiate Volcano Dragon summon candidate prefab.");
            }

            dragon.name = DragonSummonRootName;
            dragon.transform.SetPositionAndRotation(
                new Vector3(1.10f, 1.45f, 4.10f),
                Quaternion.Euler(0f, 205f, 0f));
            dragon.transform.localScale = Vector3.one * 0.105f;
            Animator animator = dragon.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            dragon.SetActive(false);
            EditorUtility.SetDirty(dragon);
            return dragon;
        }

        private static void PrepareSummonActorsForRunnerDrivenSample(
            GameObject summonActor,
            GameObject dragonSummon,
            bool visible)
        {
            PrepareSummonProxyForReview(summonActor, visible);
            if (dragonSummon == null)
            {
                return;
            }

            dragonSummon.SetActive(visible);
            if (!visible)
            {
                return;
            }

            dragonSummon.transform.SetPositionAndRotation(
                new Vector3(1.10f, 1.45f, 4.10f),
                Quaternion.Euler(0f, 205f, 0f));
            dragonSummon.transform.localScale = Vector3.one * 0.105f;
            Animator animator = dragonSummon.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind();
                animator.Update(0f);
                animator.Play(DragonSummonAttackStateName, 0, 0.32f);
                animator.Update(0.10f);
            }
        }

        private static void PrepareSummonProxyForReview(GameObject summonActor, bool visible)
        {
            if (summonActor == null)
            {
                return;
            }

            summonActor.SetActive(visible);
            if (!visible)
            {
                return;
            }

            Vector3 position = new Vector3(1.05f, 0f, 2.25f);
            Vector3 target = new Vector3(0.15f, 0f, 3.35f);
            summonActor.transform.SetPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, Vector3.up));

            SummonFrontlineProxy proxy = summonActor.GetComponentInChildren<SummonFrontlineProxy>(true);
            if (proxy != null)
            {
                proxy.Activate(position, Vector3.forward, 3, 0f, 1.1f, target, 0.35f, 520f, 1.25f);
                proxy.RequestAdvanceHold(3f);
                proxy.NotifyAttackPerformed(0.8f);
                proxy.Tick(0.12f);
            }

            SummonFrontlineProxyPresenter presenter = summonActor.GetComponentInChildren<SummonFrontlineProxyPresenter>(true);
            presenter?.RefreshNow();
        }

        private static void CreateDirectionalLight(Scene scene)
        {
            GameObject lightObject = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.85f;
            light.color = new Color(0.96f, 0.98f, 1.00f, 1f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(42f, -34f, 0f);
        }

        private static void ConfigureReviewRenderSettings()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.20f, 0.24f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.13f, 0.16f, 0.20f, 1f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
        }

        private static void CreateCinematicStageDressing(Scene scene)
        {
            GameObject root = new GameObject(StageRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            Material floorMaterial = CreateRuntimeMaterial(
                "CinematicP0Review_FloorMat",
                new Color(0.055f, 0.070f, 0.086f, 1f),
                new Color(0.010f, 0.018f, 0.024f, 1f));
            Material platformMaterial = CreateRuntimeMaterial(
                "CinematicP0Review_PlatformMat",
                new Color(0.075f, 0.086f, 0.105f, 1f),
                new Color(0.018f, 0.022f, 0.030f, 1f));
            Material panelMaterial = CreateRuntimeMaterial(
                "CinematicP0Review_BackPanelMat",
                new Color(0.060f, 0.072f, 0.092f, 1f),
                new Color(0.012f, 0.018f, 0.030f, 1f));
            Material cyanGlowMaterial = CreateRuntimeMaterial(
                "CinematicP0Review_CyanGuideGlow",
                new Color(0.16f, 0.78f, 0.92f, 1f),
                new Color(0.05f, 0.55f, 0.78f, 1f));
            Material roseGlowMaterial = CreateRuntimeMaterial(
                "CinematicP0Review_RoseWarningGlow",
                new Color(0.95f, 0.34f, 0.48f, 1f),
                new Color(0.68f, 0.08f, 0.18f, 1f));
            Material warmGlowMaterial = CreateRuntimeMaterial(
                "CinematicP0Review_WarmEdgeGlow",
                new Color(1.00f, 0.80f, 0.36f, 1f),
                new Color(0.72f, 0.38f, 0.08f, 1f));

            CreatePanelCube(root.transform, "CinematicP0Review_Floor", new Vector3(0f, -0.08f, 1.4f), Quaternion.identity, new Vector3(8.4f, 0.10f, 13.8f), floorMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_PlayerReadabilityField", new Vector3(0f, -0.018f, 0.35f), Quaternion.identity, new Vector3(2.8f, 0.018f, 3.8f), platformMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_CenterGuideLine", new Vector3(0f, -0.005f, 1.4f), Quaternion.identity, new Vector3(0.045f, 0.018f, 10.6f), cyanGlowMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_LeftGuideLine", new Vector3(-1.85f, -0.004f, 1.5f), Quaternion.Euler(0f, -8f, 0f), new Vector3(0.035f, 0.016f, 9.4f), roseGlowMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_RightGuideLine", new Vector3(1.85f, -0.004f, 1.5f), Quaternion.Euler(0f, 8f, 0f), new Vector3(0.035f, 0.016f, 9.4f), warmGlowMaterial);

            CreatePanelCube(root.transform, "CinematicP0Review_BackScreen", new Vector3(0f, 1.55f, -2.85f), Quaternion.identity, new Vector3(7.2f, 3.1f, 0.10f), panelMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_BackScreenTopGlow", new Vector3(0f, 2.95f, -2.78f), Quaternion.identity, new Vector3(6.4f, 0.045f, 0.08f), cyanGlowMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_BackScreenLowGlow", new Vector3(0f, 0.40f, -2.77f), Quaternion.identity, new Vector3(5.2f, 0.035f, 0.08f), roseGlowMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_LeftDepthPanel", new Vector3(-3.85f, 1.35f, -0.45f), Quaternion.Euler(0f, 18f, 0f), new Vector3(0.10f, 2.35f, 4.4f), panelMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_RightDepthPanel", new Vector3(3.85f, 1.35f, -0.45f), Quaternion.Euler(0f, -18f, 0f), new Vector3(0.10f, 2.35f, 4.4f), panelMaterial);

            CreatePanelCube(root.transform, "CinematicP0Review_CapsuleLeftLight", new Vector3(-1.65f, 1.14f, -1.25f), Quaternion.identity, new Vector3(0.055f, 2.15f, 0.055f), cyanGlowMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_CapsuleRightLight", new Vector3(1.65f, 1.14f, -1.25f), Quaternion.identity, new Vector3(0.055f, 2.15f, 0.055f), roseGlowMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_ThreatGate", new Vector3(0f, 1.45f, 5.85f), Quaternion.identity, new Vector3(3.3f, 2.1f, 0.08f), panelMaterial);
            CreatePanelCube(root.transform, "CinematicP0Review_ThreatGateGlow", new Vector3(0f, 2.45f, 5.78f), Quaternion.identity, new Vector3(3.1f, 0.055f, 0.07f), warmGlowMaterial);

            CreateReviewLight(root.transform, "CinematicP0Review_KeyFaceLight", new Vector3(0.4f, 2.3f, 2.6f), new Color(0.82f, 0.92f, 1.00f, 1f), 2.0f, 6.5f);
            CreateReviewLight(root.transform, "CinematicP0Review_RoseRimLight", new Vector3(-2.9f, 1.5f, 0.2f), new Color(1.00f, 0.36f, 0.48f, 1f), 0.75f, 5.0f);
            CreateReviewLight(root.transform, "CinematicP0Review_WarmWeaponLight", new Vector3(2.4f, 1.1f, 1.6f), new Color(1.00f, 0.78f, 0.42f, 1f), 0.62f, 4.6f);
        }

        private static GameObject CreatePanelCube(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, worldPositionStays: false);
            cube.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            cube.transform.localScale = localScale;
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return cube;
        }

        private static Light CreateReviewLight(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Color color,
            float intensity,
            float range)
        {
            GameObject lightObject = new GameObject(objectName);
            lightObject.transform.SetParent(parent, worldPositionStays: false);
            lightObject.transform.localPosition = localPosition;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            light.bounceIntensity = 0f;
            return light;
        }

        private static GameObject CreateInoriRifle(Scene scene, Transform inoriRoot, Animator inoriAnimator)
        {
            GameObject sourcePrefab = LoadAsset<GameObject>(RifleGirlSourcePrefabPath);
            GameObject sourceInstance = PrefabUtility.InstantiatePrefab(sourcePrefab, scene) as GameObject;
            if (sourceInstance == null)
            {
                throw new InvalidOperationException("Failed to instantiate RifleGirl source prefab for rifle extraction.");
            }

            PrefabUtility.UnpackPrefabInstance(
                sourceInstance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);

            InoriRiflePoseTuningProfile tuningProfile =
                LoadAsset<InoriRiflePoseTuningProfile>(ActionFoundationInoriPlayerVisualAssetSetup.RiflePoseTuningProfilePath);
            Transform sourceWeapon = FindRifleWeaponRoot(sourceInstance.transform);
            Transform sourceRightHand = FindLikelyRightHand(sourceInstance.transform);
            Transform targetRightHand = FindLikelyRightHand(inoriRoot);
            if (sourceWeapon == null || sourceRightHand == null || targetRightHand == null)
            {
                UnityEngine.Object.DestroyImmediate(sourceInstance);
                throw new InvalidOperationException("Cannot extract rifle: source weapon/source hand/Inori hand is missing.");
            }

            GameObject socketObject = new GameObject("CinematicP0Review_InoriRifleSocket");
            socketObject.transform.SetParent(targetRightHand, worldPositionStays: false);
            ApplyRetargetedRifleSocket(sourceWeapon, sourceRightHand, targetRightHand, socketObject.transform, tuningProfile);

            GameObject weapon = UnityEngine.Object.Instantiate(sourceWeapon.gameObject, socketObject.transform);
            weapon.name = RifleName;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.transform.localScale = sourceWeapon.localScale;
            RemoveConstraints(weapon);

            Transform leftHandle = FindDescendant(weapon.transform, "Left_Handle");
            if (leftHandle == null)
            {
                leftHandle = new GameObject("Left_Handle").transform;
                leftHandle.SetParent(weapon.transform, worldPositionStays: false);
            }

            leftHandle.localPosition = tuningProfile.LeftHandleLocalPosition;
            leftHandle.localRotation = tuningProfile.LeftHandleLocalRotation;
            leftHandle.localScale = Vector3.one;

            RifleGirlWeaponSocketDriver socketDriver =
                inoriAnimator.gameObject.GetComponent<RifleGirlWeaponSocketDriver>()
                ?? inoriAnimator.gameObject.AddComponent<RifleGirlWeaponSocketDriver>();
            socketDriver.Configure(inoriAnimator, null, leftHandle);
            SetObjectReference(socketDriver, "animator", inoriAnimator);
            SetObjectReference(socketDriver, "rifleConstraint", null);
            SetObjectReference(socketDriver, "leftHandIkTarget", leftHandle);
            SetString(socketDriver, "defaultCommands", "To_Hand_R_Socket, IK_OFF_Left_Handle");
            SetFloat(socketDriver, "leftIkMaxWeight", tuningProfile.LeftIkPositionWeight);
            SetFloat(socketDriver, "leftIkRotationMaxWeight", tuningProfile.LeftIkRotationWeight);
            socketDriver.SwitchSocketByString("To_Hand_R_Socket, IK_OFF_Left_Handle");

            UnityEngine.Object.DestroyImmediate(sourceInstance);
            EditorUtility.SetDirty(socketObject);
            EditorUtility.SetDirty(weapon);
            EditorUtility.SetDirty(socketDriver);
            return weapon;
        }

        private static void ApplyRetargetedRifleSocket(
            Transform sourceWeapon,
            Transform sourceHand,
            Transform targetHand,
            Transform targetSocket,
            InoriRiflePoseTuningProfile tuningProfile)
        {
            Quaternion handAxisCorrection = Quaternion.Inverse(targetHand.rotation) * sourceHand.rotation;
            Vector3 sourceLocalPosition = sourceHand.InverseTransformPoint(sourceWeapon.position);
            Quaternion sourceLocalRotation = Quaternion.Inverse(sourceHand.rotation) * sourceWeapon.rotation;
            targetSocket.localPosition = (handAxisCorrection * sourceLocalPosition)
                + tuningProfile.RightGripLocalPosition;
            targetSocket.localRotation = (handAxisCorrection * sourceLocalRotation)
                * tuningProfile.RightGripLocalRotation;
            targetSocket.localScale = Vector3.one;
        }

        private static void AssignInoriPromotedMaterials(GameObject visualRoot)
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    string hint = materials[j] != null ? materials[j].name : string.Empty;
                    materials[j] = ActionFoundationInoriPlayerVisualAssetSetup.ResolvePromotedMaterial(hint, j);
                }

                renderers[i].sharedMaterials = materials;
                EditorUtility.SetDirty(renderers[i]);
            }
        }

        private static CinematicBlendShapeExpressionPlayer.ExpressionPreset[] CreateInoriExpressionPresets()
        {
            return new[]
            {
                Preset("Surprised",
                    Shape("browInnerUpSurprised", 80f),
                    Shape("vrc.v_oh", 70f),
                    Shape("eyeWideRight", 82f),
                    Shape("eyeWideLeft", 82f),
                    Shape("jawOpen", 48f),
                    Shape("mouthStretchLeft", 24f),
                    Shape("mouthStretchRight", 24f)),
                Preset("Confused",
                    Shape("browInnerUpSurprised", 46f),
                    Shape("vrc.v_ou", 42f),
                    Shape("jawOpen", 15f)),
                Preset("Angry",
                    Shape("browDownRight", 70f),
                    Shape("browDownLeft", 70f),
                    Shape("noseSneerRight", 42f),
                    Shape("noseSneerLeft", 42f),
                    Shape("mouthFrownRight", 56f),
                    Shape("mouthFrownLeft", 56f)),
                Preset("CalmEye",
                    Shape("eyeBlinkRight", 14f),
                    Shape("eyeBlinkLeft", 14f)),
                Preset("Smile",
                    Shape("mouthSmileRight", 64f),
                    Shape("mouthSmileLeft", 64f),
                    Shape("eyeBlinkRight", 10f),
                    Shape("eyeBlinkLeft", 10f)),
                Preset("Joy",
                    Shape("eyeBlinkRight", 28f),
                    Shape("eyeBlinkLeft", 28f),
                    Shape("cheekSquintRight", 42f),
                    Shape("cheekSquintLeft", 42f),
                    Shape("mouthSmileRight", 78f),
                    Shape("mouthSmileLeft", 78f))
            };
        }

        private static CinematicBlendShapeExpressionPlayer.ExpressionPreset Preset(
            string expressionName,
            params CinematicBlendShapeExpressionPlayer.ShapeWeight[] shapes)
        {
            return new CinematicBlendShapeExpressionPlayer.ExpressionPreset(expressionName, shapes);
        }

        private static CinematicBlendShapeExpressionPlayer.ShapeWeight Shape(string shapeName, float weight)
        {
            return new CinematicBlendShapeExpressionPlayer.ShapeWeight(shapeName, weight);
        }

        private static Transform FindRifleWeaponRoot(Transform root)
        {
            ParentConstraint[] constraints = root.GetComponentsInChildren<ParentConstraint>(includeInactive: true);
            for (int i = 0; i < constraints.Length; i++)
            {
                if (constraints[i] != null && constraints[i].name.IndexOf("Weapon_Rifle", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return constraints[i].transform;
                }
            }

            return FindDescendantContains(root, "Weapon_Rifle") ?? FindDescendantContains(root, "Rifle");
        }

        private static Transform FindLikelyRightHand(Transform root)
        {
            return FindDescendant(root, "hand.r")
                ?? FindDescendant(root, "Hand_R_Socket")
                ?? FindDescendant(root, "RightHand")
                ?? FindDescendantContains(root, "RightHand")
                ?? FindDescendantContains(root, "Hand_R");
        }

        private static void RemoveConstraints(GameObject root)
        {
            ParentConstraint[] constraints = root.GetComponentsInChildren<ParentConstraint>(includeInactive: true);
            for (int i = constraints.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(constraints[i]);
            }
        }

        private static Material CreateRuntimeMaterial(string materialName, Color color)
        {
            return CreateRuntimeMaterial(materialName, color, Color.black);
        }

        private static Material CreateRuntimeMaterial(string materialName, Color color, Color emissionColor)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard");
            Material material = new Material(shader)
            {
                name = materialName,
                color = color
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (emissionColor.maxColorComponent > 0.001f)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", emissionColor);
                }
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.52f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.05f);
            }

            return material;
        }

        private static GameObject FindRoot(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == objectName)
                {
                    return roots[i];
                }
            }

            return null;
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

        private static void CreatePlaylistContactSheet(
            string[] imagePaths,
            PlaylistStripSample[] samples,
            string outputPath,
            int thumbnailWidth,
            int thumbnailHeight,
            int columns)
        {
            if (imagePaths == null || imagePaths.Length == 0)
            {
                throw new InvalidOperationException("Cannot create playlist contact sheet without source images.");
            }

            int resolvedColumns = Mathf.Max(1, columns);
            int rows = Mathf.CeilToInt(imagePaths.Length / (float)resolvedColumns);
            int labelBandHeight = 44;
            int cellHeight = thumbnailHeight + labelBandHeight;
            Texture2D contactSheet = new Texture2D(
                thumbnailWidth * resolvedColumns,
                cellHeight * rows,
                TextureFormat.RGBA32,
                mipChain: false);

            try
            {
                Color background = new Color(0.045f, 0.052f, 0.064f, 1f);
                Color labelBackground = new Color(0.025f, 0.031f, 0.043f, 1f);
                Color labelText = new Color(0.94f, 0.97f, 1f, 1f);
                Color stateText = new Color(0.55f, 0.92f, 1f, 1f);
                Color[] backgroundPixels = new Color[contactSheet.width * contactSheet.height];
                for (int i = 0; i < backgroundPixels.Length; i++)
                {
                    backgroundPixels[i] = background;
                }

                contactSheet.SetPixels(backgroundPixels);

                for (int i = 0; i < imagePaths.Length; i++)
                {
                    Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                    Texture2D resized = null;
                    try
                    {
                        if (!source.LoadImage(File.ReadAllBytes(imagePaths[i])))
                        {
                            throw new InvalidOperationException($"Failed to load playlist frame {imagePaths[i]}.");
                        }

                        resized = ResizeTexture(source, thumbnailWidth, thumbnailHeight);
                        int column = i % resolvedColumns;
                        int row = i / resolvedColumns;
                        int targetX = column * thumbnailWidth;
                        int cellY = contactSheet.height - ((row + 1) * cellHeight);
                        int targetY = cellY + labelBandHeight;
                        CopyTexture(resized, contactSheet, targetX, targetY);
                        FillRect(contactSheet, targetX, cellY, thumbnailWidth, labelBandHeight, labelBackground);
                        DrawBitmapText(
                            contactSheet,
                            BuildContactSheetLabel(samples, i),
                            targetX + 8,
                            cellY + 25,
                            2,
                            labelText);
                        DrawBitmapText(
                            contactSheet,
                            BuildContactSheetStateLabel(samples, i),
                            targetX + 8,
                            cellY + 7,
                            2,
                            stateText);
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(source);
                        if (resized != null)
                        {
                            UnityEngine.Object.DestroyImmediate(resized);
                        }
                    }
                }

                contactSheet.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "C:/tmp");
                File.WriteAllBytes(outputPath, contactSheet.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(contactSheet);
            }
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            Texture2D resized = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = height > 1 ? y / (float)(height - 1) : 0f;
                for (int x = 0; x < width; x++)
                {
                    float u = width > 1 ? x / (float)(width - 1) : 0f;
                    pixels[x + (y * width)] = source.GetPixelBilinear(u, v);
                }
            }

            resized.SetPixels(pixels);
            resized.Apply();
            return resized;
        }

        private static void CopyTexture(Texture2D source, Texture2D destination, int targetX, int targetY)
        {
            Color[] pixels = source.GetPixels();
            destination.SetPixels(targetX, targetY, source.width, source.height, pixels);
        }

        private static string BuildContactSheetLabel(PlaylistStripSample[] samples, int index)
        {
            string label = samples != null && index >= 0 && index < samples.Length
                ? samples[index].Label
                : $"Frame {index + 1}";
            return TrimBitmapText($"{index + 1:00} {SanitizeBitmapText(label)}", 25);
        }

        private static string BuildContactSheetStateLabel(PlaylistStripSample[] samples, int index)
        {
            if (samples == null || index < 0 || index >= samples.Length)
            {
                return string.Empty;
            }

            string stateName = SanitizeBitmapText(samples[index].StateName);
            return TrimBitmapText(stateName, 25);
        }

        private static string SanitizeBitmapText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(text.Length);
            bool previousWasSpace = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = char.ToUpperInvariant(text[i]);
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '/')
                {
                    builder.Append(c);
                    previousWasSpace = false;
                }
                else if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }
            }

            return builder.ToString().Trim();
        }

        private static string TrimBitmapText(string text, int maxCharacters)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxCharacters)
            {
                return text ?? string.Empty;
            }

            return text.Substring(0, Mathf.Max(0, maxCharacters));
        }

        private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            int minX = Mathf.Clamp(x, 0, texture.width);
            int minY = Mathf.Clamp(y, 0, texture.height);
            int maxX = Mathf.Clamp(x + width, 0, texture.width);
            int maxY = Mathf.Clamp(y + height, 0, texture.height);
            for (int py = minY; py < maxY; py++)
            {
                for (int px = minX; px < maxX; px++)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }

        private static void DrawBitmapText(Texture2D texture, string text, int x, int y, int scale, Color color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            int cursorX = x;
            int resolvedScale = Mathf.Max(1, scale);
            for (int i = 0; i < text.Length; i++)
            {
                string[] glyph = GetBitmapGlyph(text[i]);
                for (int row = 0; row < glyph.Length; row++)
                {
                    string glyphRow = glyph[row];
                    for (int column = 0; column < glyphRow.Length; column++)
                    {
                        if (glyphRow[column] == ' ')
                        {
                            continue;
                        }

                        int pixelX = cursorX + (column * resolvedScale);
                        int pixelY = y + ((glyph.Length - 1 - row) * resolvedScale);
                        FillRect(texture, pixelX, pixelY, resolvedScale, resolvedScale, color);
                    }
                }

                cursorX += 6 * resolvedScale;
            }
        }

        private static string[] GetBitmapGlyph(char c)
        {
            switch (c)
            {
                case '0': return new[] { " ### ", "#   #", "#  ##", "# # #", "##  #", "#   #", " ### " };
                case '1': return new[] { "  #  ", " ##  ", "# #  ", "  #  ", "  #  ", "  #  ", "#####" };
                case '2': return new[] { " ### ", "#   #", "    #", "   # ", "  #  ", " #   ", "#####" };
                case '3': return new[] { "#### ", "    #", "    #", " ### ", "    #", "    #", "#### " };
                case '4': return new[] { "#   #", "#   #", "#   #", "#####", "    #", "    #", "    #" };
                case '5': return new[] { "#####", "#    ", "#    ", "#### ", "    #", "    #", "#### " };
                case '6': return new[] { " ### ", "#    ", "#    ", "#### ", "#   #", "#   #", " ### " };
                case '7': return new[] { "#####", "    #", "   # ", "  #  ", " #   ", " #   ", " #   " };
                case '8': return new[] { " ### ", "#   #", "#   #", " ### ", "#   #", "#   #", " ### " };
                case '9': return new[] { " ### ", "#   #", "#   #", " ####", "    #", "    #", " ### " };
                case 'A': return new[] { " ### ", "#   #", "#   #", "#####", "#   #", "#   #", "#   #" };
                case 'B': return new[] { "#### ", "#   #", "#   #", "#### ", "#   #", "#   #", "#### " };
                case 'C': return new[] { " ### ", "#   #", "#    ", "#    ", "#    ", "#   #", " ### " };
                case 'D': return new[] { "#### ", "#   #", "#   #", "#   #", "#   #", "#   #", "#### " };
                case 'E': return new[] { "#####", "#    ", "#    ", "#### ", "#    ", "#    ", "#####" };
                case 'F': return new[] { "#####", "#    ", "#    ", "#### ", "#    ", "#    ", "#    " };
                case 'G': return new[] { " ### ", "#   #", "#    ", "# ###", "#   #", "#   #", " ### " };
                case 'H': return new[] { "#   #", "#   #", "#   #", "#####", "#   #", "#   #", "#   #" };
                case 'I': return new[] { "#####", "  #  ", "  #  ", "  #  ", "  #  ", "  #  ", "#####" };
                case 'J': return new[] { "#####", "    #", "    #", "    #", "    #", "#   #", " ### " };
                case 'K': return new[] { "#   #", "#  # ", "# #  ", "##   ", "# #  ", "#  # ", "#   #" };
                case 'L': return new[] { "#    ", "#    ", "#    ", "#    ", "#    ", "#    ", "#####" };
                case 'M': return new[] { "#   #", "## ##", "# # #", "#   #", "#   #", "#   #", "#   #" };
                case 'N': return new[] { "#   #", "##  #", "# # #", "#  ##", "#   #", "#   #", "#   #" };
                case 'O': return new[] { " ### ", "#   #", "#   #", "#   #", "#   #", "#   #", " ### " };
                case 'P': return new[] { "#### ", "#   #", "#   #", "#### ", "#    ", "#    ", "#    " };
                case 'Q': return new[] { " ### ", "#   #", "#   #", "#   #", "# # #", "#  # ", " ## #" };
                case 'R': return new[] { "#### ", "#   #", "#   #", "#### ", "# #  ", "#  # ", "#   #" };
                case 'S': return new[] { " ####", "#    ", "#    ", " ### ", "    #", "    #", "#### " };
                case 'T': return new[] { "#####", "  #  ", "  #  ", "  #  ", "  #  ", "  #  ", "  #  " };
                case 'U': return new[] { "#   #", "#   #", "#   #", "#   #", "#   #", "#   #", " ### " };
                case 'V': return new[] { "#   #", "#   #", "#   #", "#   #", "#   #", " # # ", "  #  " };
                case 'W': return new[] { "#   #", "#   #", "#   #", "#   #", "# # #", "## ##", "#   #" };
                case 'X': return new[] { "#   #", "#   #", " # # ", "  #  ", " # # ", "#   #", "#   #" };
                case 'Y': return new[] { "#   #", "#   #", " # # ", "  #  ", "  #  ", "  #  ", "  #  " };
                case 'Z': return new[] { "#####", "    #", "   # ", "  #  ", " #   ", "#    ", "#####" };
                case '-': return new[] { "     ", "     ", "     ", " ### ", "     ", "     ", "     " };
                case '/': return new[] { "    #", "    #", "   # ", "  #  ", " #   ", "#    ", "#    " };
                case ' ': return new[] { "     ", "     ", "     ", "     ", "     ", "     ", "     " };
                default: return new[] { " ### ", "#   #", "    #", "   # ", "  #  ", "     ", "  #  " };
            }
        }

        private static void WritePlaylistStripReport(PlaylistStripSample[] samples, string[] framePaths)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# DimensionBrawl Cinematic P0 Playlist Strip");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Contact sheet: `{PlaylistStripCapturePath}`");
            builder.AppendLine($"Review scene: `{ReviewScenePath}`");
            builder.AppendLine();
            builder.AppendLine("| # | Beat | Profile | Camera cue | State | Face | Weapon | Frame |");
            builder.AppendLine("|---|------|---------|------------|-------|------|--------|-------|");

            for (int i = 0; i < samples.Length; i++)
            {
                PlaylistStripSample sample = samples[i];
                string profileName = Path.GetFileNameWithoutExtension(sample.ProfilePath);
                string weapon = sample.WeaponVisible ? "visible" : "hidden";
                builder.AppendLine(
                    $"| {i + 1} | {sample.Label} | `{profileName}` | `{sample.CameraCueId}` | `{sample.StateName}` | `{sample.ExpressionName}` | {weapon} | `{framePaths[i]}` |");
            }

            builder.AppendLine();
            builder.AppendLine("This strip is an editor capture of the P0 playlist beats. It is intended to catch wrong-scene, wrong-camera, back-facing, and missing-weapon regressions before runtime video QA.");
            Directory.CreateDirectory(Path.GetDirectoryName(PlaylistStripReportPath) ?? "C:/tmp");
            File.WriteAllText(PlaylistStripReportPath, builder.ToString());
        }

        private static void WriteRunnerDrivenStripReport(
            PlaylistStripSample[] samples,
            RunnerDrivenSampleResult[] results,
            string[] framePaths,
            string stripCapturePath,
            string stripReportPath,
            string reportTitle,
            string reviewScopeLabel)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"# {reportTitle}");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Contact sheet: `{stripCapturePath}`");
            builder.AppendLine($"Review scene: `{ReviewScenePath}`");
            builder.AppendLine();
            builder.AppendLine("This capture calls `CinematicSequenceRunner.TryApplyProfileSampleForReview(...)`, so camera, actor, weapon, VFX, and tutorial cue counters come from the runtime runner path rather than manual editor pose setup.");
            builder.AppendLine();
            builder.AppendLine("| # | Beat | Expected camera | Runner camera | Expected actor | Runner actor | VFX | Tutorial | Weapon | Frame |");
            builder.AppendLine("|---|------|-----------------|---------------|----------------|--------------|-----|----------|--------|-------|");

            for (int i = 0; i < samples.Length; i++)
            {
                PlaylistStripSample sample = samples[i];
                RunnerDrivenSampleResult result = results[i];
                string weapon = sample.WeaponVisible ? "visible" : "hidden";
                builder.AppendLine(
                    $"| {i + 1} | {sample.Label} | `{sample.CameraCueId}` | `{result.LastCameraCueId}` ({result.TotalCameraCueCount}) | `{sample.StateName}`/`{sample.ExpressionName}` | `{result.LastActorCueId}` ({result.TotalActorCueCount}) | `{result.LastVfxCueId}` ({result.TotalVfxCueCount}) | `{result.LastTutorialCueId}` ({result.TotalTutorialCueCount}) | {weapon} | `{framePaths[i]}` |");
            }

            builder.AppendLine();
            builder.AppendLine($"A blank runner VFX or tutorial cue means the sampled {reviewScopeLabel} profile/time has not reached that cue yet, not necessarily a failure. Camera and actor columns should match the expected beat intent.");
            Directory.CreateDirectory(Path.GetDirectoryName(stripReportPath) ?? "C:/tmp");
            File.WriteAllText(stripReportPath, builder.ToString());
        }

        private static void CaptureCamera(Camera camera, string outputPath, int width, int height)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "C:/tmp");
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (string.Equals(root.name, childName, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDescendantContains(Transform root, string namePart)
        {
            if (root.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendantContains(root.GetChild(i), namePart);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private struct PlaylistStripSample
        {
            public readonly string Label;
            public readonly string Slug;
            public readonly string ProfilePath;
            public readonly string StateName;
            public readonly string ExpressionName;
            public readonly string CameraCueId;
            public readonly float SampleSeconds;
            public readonly bool WeaponVisible;

            public PlaylistStripSample(
                string label,
                string slug,
                string profilePath,
                string stateName,
                string expressionName,
                string cameraCueId,
                float sampleSeconds,
                bool weaponVisible)
            {
                Label = label;
                Slug = slug;
                ProfilePath = profilePath;
                StateName = stateName;
                ExpressionName = expressionName;
                CameraCueId = cameraCueId;
                SampleSeconds = sampleSeconds;
                WeaponVisible = weaponVisible;
            }
        }

        private struct RunnerDrivenSampleResult
        {
            public readonly string LastCameraCueId;
            public readonly string LastActorCueId;
            public readonly string LastVfxCueId;
            public readonly string LastTutorialCueId;
            public readonly int TotalCameraCueCount;
            public readonly int TotalActorCueCount;
            public readonly int TotalVfxCueCount;
            public readonly int TotalTutorialCueCount;
            public readonly bool GameplayHandoffReached;

            public RunnerDrivenSampleResult(
                string lastCameraCueId,
                string lastActorCueId,
                string lastVfxCueId,
                string lastTutorialCueId,
                int totalCameraCueCount,
                int totalActorCueCount,
                int totalVfxCueCount,
                int totalTutorialCueCount,
                bool gameplayHandoffReached)
            {
                LastCameraCueId = lastCameraCueId;
                LastActorCueId = lastActorCueId;
                LastVfxCueId = lastVfxCueId;
                LastTutorialCueId = lastTutorialCueId;
                TotalCameraCueCount = totalCameraCueCount;
                TotalActorCueCount = totalActorCueCount;
                TotalVfxCueCount = totalVfxCueCount;
                TotalTutorialCueCount = totalTutorialCueCount;
                GameplayHandoffReached = gameplayHandoffReached;
            }
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {path}.");
            }

            return asset;
        }

        private static T RequireComponent<T>(GameObject gameObject, string label) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{label} is missing on {gameObject.name}.");
            }

            return component;
        }

        private static T RequireComponentInChildren<T>(GameObject gameObject, string label) where T : Component
        {
            T component = gameObject.GetComponentInChildren<T>(includeInactive: true);
            if (component == null)
            {
                throw new InvalidOperationException($"{label} is missing on {gameObject.name}.");
            }

            return component;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SetObjectReference(serializedObject, propertyName, value);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.objectReferenceValue = value;
        }

        private static void SetRelativeObjectReference(SerializedProperty owner, string propertyName, UnityEngine.Object value)
        {
            owner.FindPropertyRelative(propertyName).objectReferenceValue = value;
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetVector3(UnityEngine.Object target, string propertyName, Vector3 value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).vector3Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetRelativeEnum(SerializedProperty owner, string propertyName, int value)
        {
            owner.FindPropertyRelative(propertyName).enumValueIndex = value;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }
    }

    [InitializeOnLoad]
    internal static class BuildResubmissionCinematicPlayModeCaptureBatch
    {
        private const string ActiveKey = "DimensionBrawl.CinematicP0.PlayModeCapture.Active";
        private const string ResultPathKey = "DimensionBrawl.CinematicP0.PlayModeCapture.ResultPath";
        private const string StartedAtKey = "DimensionBrawl.CinematicP0.PlayModeCapture.StartedAt";
        private const string TimeoutSecondsKey = "DimensionBrawl.CinematicP0.PlayModeCapture.TimeoutSeconds";
        private const string ProbeInstalledKey = "DimensionBrawl.CinematicP0.PlayModeCapture.ProbeInstalled";

        static BuildResubmissionCinematicPlayModeCaptureBatch()
        {
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
        }

        public static void Start(string resultPath, float timeoutSeconds)
        {
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            EditorPrefs.SetBool(ActiveKey, true);
            EditorPrefs.SetString(ResultPathKey, resultPath);
            EditorPrefs.SetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            EditorPrefs.SetFloat(TimeoutSecondsKey, timeoutSeconds);
            EditorPrefs.SetBool(ProbeInstalledKey, false);
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
            Debug.Log($"Started cinematic Play Mode route capture monitor: {resultPath}");
        }

        private static void Monitor()
        {
            if (!EditorPrefs.GetBool(ActiveKey, false))
            {
                return;
            }

            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            string resultPath = EditorPrefs.GetString(ResultPathKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(resultPath) && File.Exists(resultPath))
            {
                string result = File.ReadAllText(resultPath);
                bool passed = result.Contains("RESULT=PASS");
                Clear();
                if (passed)
                {
                    Debug.Log($"Cinematic Play Mode route capture passed. See {resultPath}.");
                }
                else
                {
                    Debug.LogError($"Cinematic Play Mode route capture failed. See {resultPath}.");
                }

                EditorApplication.Exit(passed ? 0 : 1);
                return;
            }

            if (EditorApplication.isPlaying && !EditorPrefs.GetBool(ProbeInstalledKey, false))
            {
                BuildResubmissionCinematicReviewSceneSetup.ConfigurePlayModeRouteCaptureProbe(
                    SceneManager.GetActiveScene());
                EditorPrefs.SetBool(ProbeInstalledKey, true);
                Debug.Log("Installed cinematic Play Mode route capture probe in active scene.");
            }

            float startedAt = EditorPrefs.GetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            float timeoutSeconds = EditorPrefs.GetFloat(TimeoutSecondsKey, 180f);
            if (EditorApplication.timeSinceStartup - startedAt <= timeoutSeconds)
            {
                return;
            }

            Clear();
            Debug.LogError($"Cinematic Play Mode route capture timed out after {timeoutSeconds:F1}s.");
            EditorApplication.Exit(1);
        }

        private static void Clear()
        {
            EditorPrefs.DeleteKey(ActiveKey);
            EditorPrefs.DeleteKey(ResultPathKey);
            EditorPrefs.DeleteKey(StartedAtKey);
            EditorPrefs.DeleteKey(TimeoutSecondsKey);
            EditorPrefs.DeleteKey(ProbeInstalledKey);
            EditorApplication.update -= Monitor;
        }
    }
}
