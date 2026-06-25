using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Presentation;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class IntroGatePodCutsceneReviewSetup
    {
        public const string ReviewScenePath = "Assets/_Game/Scenes/IntroGatePodCutsceneReview.unity";
        public const string ProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening.asset";
        public const string TimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening.playable";
        public const string ReportPath = "C:/tmp/DimensionBrawl-IntroGatePodCutsceneReview.md";
        public const string OpeningCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-01-Opening.png";
        public const string RevealCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-02-Reveal.png";
        public const string LeftScanCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-03-ScanLeft.png";
        public const string RightScanCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-04-ScanRight.png";
        public const string HandsCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-05-LookDownHands.png";
        public const string CommandoLegsCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-06-CommandoLegs.png";
        public const string HeavenExplosionCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-07-HeavenExplosion.png";
        public const string CommandoPushCapturePath = "C:/tmp/DimensionBrawl-IntroGatePod-08-CommandoPush.png";
        public const string SampleDebugPath = "C:/tmp/DimensionBrawl-IntroGatePod-SampleDebug.md";

        private const string GateModelPath = "Assets/_Game/Art/Environment/UnityChan/Gate/gate.fbx";
        private const string RifleGirlSourcePrefabPath =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/RifleGirl/Prefab/Rifle_Full_Body.prefab";
        private const string RifleModelPath = "Assets/_Game/Art/Characters/Player/RifleGirl/Weapons/Weapon_Rifle.fbx";
        private const string SwordModelPath = "Assets/_Imported/SpecialSkillsEffectsPack/Models/Sword_1.fbx";
        private const string SciFiCommandoPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_GeneralDeck.prefab";
        private const string SciFiCommandoControllerPath =
            "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/SciFiSoldier01/DB_SciFiSoldier01_GeneralDeck.controller";
        private const string SciFiCommandoRunStateName = "RunForward";
        private const string UniGasFirePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Gas_Fire.prefab";
        private const string UniLongSmokePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Long_Smoke.prefab";
        private const string UniDeviceFirePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Device_Fire.prefab";
        private const string UniSmallFirePrefabPath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs/UNI_Small_Fire.prefab";
        private const string UniHighExplosionTexturePath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Textures/uni_high_explosion.tga";
        private const string UniSmokeBigTexturePath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Textures/uni_smoke_big.tga";
        private const string UniSmallFireballsTexturePath =
            "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Textures/uni_smallfireballs.tga";

        private const string SceneRootName = "IntroGatePodReview_Root";
        private const string StageRootName = "IntroGatePodReview_StageRoot";
        private const string GatePodRootName = "IntroGatePodReview_GatePods";
        private const string InvasionBridgeRootName = "IntroGatePodReview_InvasionBridge";
        private const string InvasionCommandoGroupName = "IntroGatePodReview_CommandoRunGroup";
        private const string InvasionExplosionRootName = "IntroGatePodReview_HeavenBackgroundExplosion";
        private const string InvasionScreenEffectRootName = "IntroGatePodReview_InvasionScreenEffects";
        private const string InvasionImpactFlashGroupName = "IntroGatePodReview_InvasionImpactFlash";
        private const string InvasionWarningSweepGroupName = "IntroGatePodReview_InvasionWarningSweep";
        private const string InoriPlacementRootName = "IntroGatePodReview_InoriPlacement";
        private const string InoriRootName = "IntroGatePodReview_Inori";
        private const string RunnerRootName = "IntroGatePodReview_Runner";
        private const string CinemachineRootName = "IntroGatePodReview_CinemachineShots";
        private const string CinemachineShotPlayerName = "IntroGatePodReview_CinemachineShotPlayer";
        private const string CueDirectorName = "IntroGatePodReview_CueDirector";
        private const string TimelineDirectorName = "IntroGatePodReview_TimelineDirector";
        private const string TimelineAudioRootName = "IntroGatePodReview_TimelineAudio";
        private const string TimelineVoiceAudioName = "IntroGatePodReview_VoiceTimelineAudio";
        private const string TimelineBgmAudioName = "IntroGatePodReview_BgmTimelineAudio";
        private const string FadeOverlayName = "IntroGatePodReview_TimelineFadeOverlay";
        private const string FirstPersonRendererMaskName = "IntroGatePodReview_FirstPersonRendererMask";
        private const string InoriPlacementTrackName = "Inori Placement";
        private const string InoriPlacementClipName = "Inori Placement Keys";
        private const string InoriBodyTrackName = "Inori Body";
        private const string OpeningDollyTrackName = "Opening Dolly";
        private const string OpeningDollyClipName = "Opening Capsule Dolly";
        private const string OpeningDollyCueId = "src_c01_capsule_left_dolly";
        private const string OpeningDollyCurveProperty = "m_SplineSettings.Position";
        private const int PositionOnlyMatchTargetFields = 7;
        private const string OpeningDollySplineName = "IntroGatePodReview_OpeningCapsuleDollySpline";
        private const string IntroLookAtHandsStateName = "CIN_IntroLookAtHands";
        private const string CombatReadyStateName = "CIN_CombatReady";
        private const string RifleName = "InoriRifle";
        private const string FloorRifleName = "IntroGatePodReview_FloorRifle";
        private const string FloorSwordName = "IntroGatePodReview_FloorSword";
        private const string CameraName = "Main Camera";
        private const string ThreatAnchorName = "IntroGatePodReview_ThreatAnchor";
        private const string FirstPersonViewMarkerName = "IntroGatePodReview_FirstPersonViewMarker";

        private const string MaterialRoot = "Assets/_Game/Art/Environment/UnityChan/Gate/Materials";
        private const string TextureRoot = "Assets/_Game/Art/Environment/UnityChan/Gate/Textures";
        private const string PodShellMaterialPath = MaterialRoot + "/DB_GatePod_Shell.mat";
        private const string PodGlowMaterialPath = MaterialRoot + "/DB_GatePod_CyanGlow.mat";
        private const string StageFloorMaterialPath = MaterialRoot + "/DB_GatePodReview_Floor.mat";
        private const string StageStoneMaterialPath = MaterialRoot + "/DB_GatePodReview_HeavenStone.mat";
        private const string StageWarningMaterialPath = MaterialRoot + "/DB_GatePodReview_InvasionWarning.mat";
        private const string StageGoldMaterialPath = MaterialRoot + "/DB_GatePodReview_GoldTrim.mat";
        private const string InvasionExplosionBillboardMaterialPath =
            MaterialRoot + "/DB_GatePodReview_UniHighExplosionBillboard.mat";
        private const string InvasionSmokeBillboardMaterialPath =
            MaterialRoot + "/DB_GatePodReview_UniSmokeBillboard.mat";
        private const string InvasionSparkBillboardMaterialPath =
            MaterialRoot + "/DB_GatePodReview_UniSparkBillboard.mat";
        private const string PodAlbedoTexturePath = TextureRoot + "/pods.psd";
        private const string PodEmissionTexturePath = TextureRoot + "/pods_L.psd";
        private const string VoiceRoot = "Assets/_Game/Art/Audio/Voice/Cinematics/IntroGatePod";
        private const string VoiceZeroPath = VoiceRoot + "/0.mp3";
        private const string VoiceOnePath = VoiceRoot + "/1.mp3";
        private const string VoiceTwoPath = VoiceRoot + "/2.mp3";
        private const string VoiceThreePath = VoiceRoot + "/3.mp3";
        private const string BgmPath = VoiceRoot + "/BGM.mp3";

        private const float SourceFadeInSeconds = 2.0f;
        private const float SourceC01CameraEndSeconds = 3.0666666f;
        private const float SourceC03CameraStartSeconds = 6.1f;
        private const float SourceC04CameraStartSeconds = 8.133333f;
        private const float FallbackIntroGameplayHandoffSeconds = 14.55f;
        private const float VoiceTwoStartOffsetSeconds = 0.30f;
        private const float VoiceGapAfterLineSeconds = 0.35f;
        private const float FirstPersonBlackoutLeadSeconds = 0.50f;
        private const float FirstPersonFadeInSeconds = 1.05f;
        private const float ScanSideHoldSeconds = 1.55f;
        private const float HandLookHoldLeadSeconds = 1.45f;
        private const float InvasionBridgeHandLookHoldSeconds = 1.75f;
        private const float InvasionBridgeRunDurationSeconds = 4.35f;
        private const float InvasionBridgeExplosionOffsetSeconds = 1.15f;
        private const float InvasionBridgeExplosionDurationSeconds = 1.25f;
        private const float InvasionBridgePushShotOffsetSeconds = 2.05f;

        private static readonly Vector3 FirstPersonViewMarkerPosition =
            new Vector3(-4.02049f, 0.9818602f, -0.5965308f);
        private static readonly Quaternion FirstPersonViewMarkerRotation =
            new Quaternion(-0.020559791f, -0.6982825f, 0.020178175f, -0.7152425f);

        private readonly struct InvasionScreenEffectBindings
        {
            public InvasionScreenEffectBindings(CanvasGroup impactFlashGroup, CanvasGroup warningSweepGroup)
            {
                ImpactFlashGroup = impactFlashGroup;
                WarningSweepGroup = warningSweepGroup;
            }

            public CanvasGroup ImpactFlashGroup { get; }
            public CanvasGroup WarningSweepGroup { get; }
        }

        [MenuItem("DimensionBrawl/Cinematics/Reapply Intro GatePod Review Scene")]
        public static void ReapplyReviewSceneMenu()
        {
            EnsureReviewScene();
            Debug.Log("Reapplied intro GatePod cutscene review scene.");
        }

        [MenuItem("DimensionBrawl/Cinematics/Validate Intro GatePod Review Scene")]
        public static void ValidateReviewSceneMenu()
        {
            ValidateReviewScene();
            Debug.Log("Intro GatePod cutscene review scene validation passed.");
        }

        public static void RunBatchReviewSceneGeneration()
        {
            EnsureReviewScene();
        }

        public static void RunBatchValidation()
        {
            EnsureReviewScene();
            ValidateReviewScene();
        }

        public static void RunBatchSetupCaptureAndValidation()
        {
            EnsureReviewScene();
            CaptureReviewSamples();
            ValidateReviewScene();
        }

        [MenuItem("DimensionBrawl/Cinematics/Fix Intro GatePod Inori Rotation Drift")]
        public static void FixInoriRotationDriftMenu()
        {
            FixInoriRotationDrift();
            Debug.Log("Fixed intro GatePod Inori rotation drift sources.");
        }

        public static void RunBatchFixInoriRotationDrift()
        {
            BuildResubmissionCinematicAnimationSetup.RebuildInoriCinematicP0Animations();
            FixInoriRotationDrift();
        }

        [MenuItem("DimensionBrawl/Cinematics/Polish Existing Intro GatePod Invasion Bridge")]
        public static void PolishExistingInvasionBridgeMenu()
        {
            BackupExistingReviewAuthoringFiles("manual-polish");
            PolishExistingInvasionBridge();
            Debug.Log("Polished existing intro GatePod invasion bridge without regenerating the review scene.");
        }

        public static void RunBatchPolishExistingInvasionBridgeCaptureAndValidation()
        {
            BackupExistingReviewAuthoringFiles("batch-polish");
            PolishExistingInvasionBridge();
            CaptureReviewSamples();
            ValidateReviewScene();
        }

        private static void EnsureReviewScene()
        {
            AssetDatabase.Refresh();
            ConfigureGateModelImporter();
            EnsureCinematicControllerReady();
            CinematicSequenceProfile profile = ConfigureProfile();

            EnsureFolder(PathParent(ReviewScenePath));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = Path.GetFileNameWithoutExtension(ReviewScenePath);

            GameObject root = new GameObject(SceneRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            ConfigureRenderSettings();
            CreateDirectionalLight(scene);
            CreateStageDressing(scene, root.transform);
            Transform threatAnchor = CreateThreatAnchor(scene, root.transform);
            GameObject gatePods = CreateGatePods(scene, root.transform);
            CreateFirstPersonViewMarker(scene, root.transform);
            GameObject inori = CreateInoriActor(scene, root.transform);
            GameObject inoriPlacement = CreateInoriPlacementRoot(scene, root.transform, inori);
            Animator inoriPlacementAnimator = inoriPlacement.GetComponent<Animator>();
            Animator inoriAnimator = inori.GetComponentInChildren<Animator>(includeInactive: true);
            CinematicBlendShapeExpressionPlayer expressionPlayer = ConfigureExpressionPlayer(inori);
            CreateWeaponFloorProps(scene, inori.transform);
            GameObject rifle = CreateInoriRifle(scene, inori.transform, inoriAnimator);
            rifle.SetActive(false);

            ActionCameraController cameraController = CreateReviewCamera(scene, inori.transform, threatAnchor);
            IntroGatePodCinemachineShotPlayer shotPlayer = CreateCinemachineShots(
                scene,
                root.transform,
                profile,
                inori.transform,
                gatePods.transform,
                cameraController.GetComponent<CinemachineBrain>());
            IntroGatePodTimelineFadeOverlay fadeOverlay =
                CreateTimelineFadeOverlay(scene, root.transform);
            IntroGatePodCutsceneCueDirector cueDirector = CreateCutsceneCueDirector(
                scene,
                root.transform,
                shotPlayer);
            PlayableDirector timelineDirector = CreateTimelineDirector(
                scene,
                root.transform,
                profile,
                cameraController.GetComponent<CinemachineBrain>(),
                shotPlayer,
                inoriPlacementAnimator,
                inoriAnimator,
                fadeOverlay);
            IntroGatePodInvasionBridgeCue invasionBridgeCue =
                CreateInvasionBridge(scene, root.transform, timelineDirector, cameraController.GetComponent<Camera>());
            CreateFirstPersonRendererMask(scene, root.transform, timelineDirector, inori);
            CinematicSequenceRunner runner = CreateRunner(scene, profile, inori, inoriAnimator, expressionPlayer, cameraController);
            ApplyProfileSample(runner, profile, 0.1f);
            ApplyCinemachineSample(shotPlayer, cueDirector, cameraController.GetComponent<Camera>(), 0.1f);
            invasionBridgeCue.Sample(0.1f);

            EditorUtility.SetDirty(timelineDirector);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ReviewScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BackupExistingReviewAuthoringFiles(string label)
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string backupDirectory = $"C:/tmp/DimensionBrawl-IntroGatePod-{label}-{stamp}";
            Directory.CreateDirectory(backupDirectory);
            CopyAuthoringFileIfExists(ReviewScenePath, backupDirectory);
            CopyAuthoringFileIfExists(ReviewScenePath + ".meta", backupDirectory);
            CopyAuthoringFileIfExists(TimelinePath, backupDirectory);
            CopyAuthoringFileIfExists(TimelinePath + ".meta", backupDirectory);
            CopyAuthoringFileIfExists(ProfilePath, backupDirectory);
            CopyAuthoringFileIfExists(ProfilePath + ".meta", backupDirectory);
            Debug.Log($"Backed up intro GatePod authoring files to {backupDirectory}.");
        }

        private static void CopyAuthoringFileIfExists(string assetPath, string backupDirectory)
        {
            if (!File.Exists(assetPath))
            {
                return;
            }

            File.Copy(assetPath, Path.Combine(backupDirectory, Path.GetFileName(assetPath)), overwrite: true);
        }

        private static void PolishExistingInvasionBridge()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject root = FindRootOrDescendant(scene, SceneRootName)
                ?? throw new InvalidOperationException($"Missing {SceneRootName} in existing review scene.");
            IntroGatePodInvasionBridgeCue invasionBridgeCue =
                FindComponentInScene<IntroGatePodInvasionBridgeCue>(scene)
                ?? throw new InvalidOperationException("Missing existing IntroGatePodInvasionBridgeCue.");
            Camera camera = FindComponentInScene<Camera>(scene)
                ?? throw new InvalidOperationException("Missing review Camera for invasion camera impact.");

            GameObject explosionRoot = invasionBridgeCue.ExplosionRoot
                ?? FindRootOrDescendant(scene, InvasionExplosionRootName);
            if (explosionRoot == null)
            {
                throw new InvalidOperationException($"Missing {InvasionExplosionRootName} in existing review scene.");
            }

            EnsureInvasionExplosionVfx(scene, explosionRoot.transform);
            InvasionScreenEffectBindings screenEffects =
                CreateInvasionScreenEffectOverlay(scene, root.transform);
            invasionBridgeCue.ConfigurePresentation(
                camera,
                screenEffects.ImpactFlashGroup,
                screenEffects.WarningSweepGroup,
                2.65f,
                0.42f,
                0.62f,
                0.78f,
                new Vector3(0.060f, 0.044f, 0.012f),
                new Vector3(1.55f, 2.15f, 0.72f),
                0.78f);
            SetFloat(invasionBridgeCue, "explosionDurationSeconds", 1.42f);
            SetVector3(invasionBridgeCue, "explosionRestScale", Vector3.one * 0.10f);
            SetVector3(invasionBridgeCue, "explosionPeakScale", new Vector3(1.18f, 0.78f, 1.18f));
            SetFloat(invasionBridgeCue, "explosionPeakLightIntensity", 8.6f);

            EnsureInvasionFadeCues(scene);
            EnsureTimelineInvasionFadeClips();

            EditorUtility.SetDirty(invasionBridgeCue);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureInvasionExplosionVfx(Scene scene, Transform explosionRoot)
        {
            EnsureVfxChild(
                scene,
                explosionRoot,
                UniGasFirePrefabPath,
                "UNI_Gas_Fire_BackgroundBurst",
                Vector3.zero,
                Quaternion.identity,
                Vector3.one * 1.32f);
            EnsureVfxChild(
                scene,
                explosionRoot,
                UniLongSmokePrefabPath,
                "UNI_Long_Smoke_BackgroundBurst",
                new Vector3(0f, 0.14f, 0.06f),
                Quaternion.Euler(-8f, 0f, 0f),
                Vector3.one * 1.12f);
            EnsureVfxChild(
                scene,
                explosionRoot,
                UniDeviceFirePrefabPath,
                "UNI_Device_Fire_ImpactCore",
                new Vector3(-0.04f, -0.02f, -0.02f),
                Quaternion.Euler(0f, 0f, -8f),
                Vector3.one * 0.72f);
            EnsureVfxChild(
                scene,
                explosionRoot,
                UniSmallFirePrefabPath,
                "UNI_Small_Fire_Sparks",
                new Vector3(0.10f, 0.05f, 0.02f),
                Quaternion.Euler(0f, 0f, 16f),
                Vector3.one * 0.58f);
            EnsureExplosionBillboard(
                scene,
                explosionRoot,
                "UNI_HighExplosion_Billboard",
                InvasionExplosionBillboardMaterialPath,
                UniHighExplosionTexturePath,
                new Vector3(-0.03f, 0.02f, -0.045f),
                Quaternion.Euler(0f, 0f, -8f),
                new Vector3(1.82f, 1.42f, 1f),
                new Color(1f, 0.72f, 0.36f, 0.92f),
                new Vector2(0.125f, 0.125f),
                new Vector2(0.375f, 0.625f));
            EnsureExplosionBillboard(
                scene,
                explosionRoot,
                "UNI_SmokeBig_Billboard",
                InvasionSmokeBillboardMaterialPath,
                UniSmokeBigTexturePath,
                new Vector3(0.10f, 0.10f, 0.02f),
                Quaternion.Euler(0f, 0f, 11f),
                new Vector3(2.55f, 1.88f, 1f),
                new Color(0.42f, 0.36f, 0.34f, 0.54f),
                new Vector2(0.125f, 0.125f),
                new Vector2(0.250f, 0.625f));
            EnsureExplosionBillboard(
                scene,
                explosionRoot,
                "UNI_SmallFireballs_Billboard",
                InvasionSparkBillboardMaterialPath,
                UniSmallFireballsTexturePath,
                new Vector3(-0.06f, -0.04f, -0.065f),
                Quaternion.Euler(0f, 0f, 18f),
                new Vector3(1.42f, 1.08f, 1f),
                new Color(1f, 0.86f, 0.48f, 0.82f),
                new Vector2(0.125f, 0.125f),
                new Vector2(0.500f, 0.750f));
        }

        private static void EnsureVfxChild(
            Scene scene,
            Transform parent,
            string prefabPath,
            string name,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            Transform child = FindDescendant(parent, name);
            GameObject instance = child != null ? child.gameObject : null;
            if (instance == null)
            {
                GameObject prefab = LoadAsset<GameObject>(prefabPath);
                instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to instantiate VFX prefab at {prefabPath}.");
                }

                instance.name = name;
            }

            instance.transform.SetParent(parent, worldPositionStays: false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            EditorUtility.SetDirty(instance);
        }

        private static void EnsureExplosionBillboard(
            Scene scene,
            Transform parent,
            string name,
            string materialPath,
            string texturePath,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Color color,
            Vector2 textureScale,
            Vector2 textureOffset)
        {
            Transform child = FindDescendant(parent, name);
            GameObject billboard = child != null ? child.gameObject : null;
            if (billboard == null)
            {
                billboard = GameObject.CreatePrimitive(PrimitiveType.Quad);
                billboard.name = name;
                SceneManager.MoveGameObjectToScene(billboard, scene);
                Collider collider = billboard.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            billboard.transform.SetParent(parent, worldPositionStays: false);
            billboard.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            billboard.transform.localScale = localScale;
            Renderer renderer = billboard.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = LoadOrCreateTransparentTextureMaterial(
                    materialPath,
                    texturePath,
                    color,
                    textureScale,
                    textureOffset);
            }

            EditorUtility.SetDirty(billboard);
        }

        private static InvasionScreenEffectBindings CreateInvasionScreenEffectOverlay(
            Scene scene,
            Transform parent)
        {
            GameObject existing = FindRootOrDescendant(scene, InvasionScreenEffectRootName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            GameObject overlay = new GameObject(InvasionScreenEffectRootName, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(overlay, scene);
            overlay.transform.SetParent(parent, worldPositionStays: false);
            ConfigureFullScreenRect(overlay.GetComponent<RectTransform>());

            Canvas canvas = overlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 31980;
            CanvasScaler scaler = overlay.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup warningSweepGroup = CreateScreenEffectGroup(
                scene,
                overlay.transform,
                InvasionWarningSweepGroupName);
            CreateScreenEffectImage(
                scene,
                warningSweepGroup.transform,
                "IntroGatePodReview_InvasionWarningSweepBar",
                new Color(1f, 0.05f, 0.10f, 0.62f),
                new Vector2(0.5f, 0.60f),
                new Vector2(2250f, 145f),
                -13f);
            CreateScreenEffectImage(
                scene,
                warningSweepGroup.transform,
                "IntroGatePodReview_InvasionWarningSweepCore",
                new Color(1f, 0.82f, 0.72f, 0.38f),
                new Vector2(0.5f, 0.60f),
                new Vector2(2250f, 28f),
                -13f);

            CanvasGroup impactFlashGroup = CreateScreenEffectGroup(
                scene,
                overlay.transform,
                InvasionImpactFlashGroupName);
            CreateScreenEffectImage(
                scene,
                impactFlashGroup.transform,
                "IntroGatePodReview_InvasionImpactWarmFlash",
                new Color(1f, 0.48f, 0.16f, 0.72f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                0f);
            CreateScreenEffectImage(
                scene,
                impactFlashGroup.transform,
                "IntroGatePodReview_InvasionImpactWhiteCore",
                new Color(1f, 0.92f, 0.72f, 0.42f),
                new Vector2(0.47f, 0.52f),
                new Vector2(1540f, 840f),
                -6f);

            EditorUtility.SetDirty(canvas);
            EditorUtility.SetDirty(scaler);
            return new InvasionScreenEffectBindings(impactFlashGroup, warningSweepGroup);
        }

        private static CanvasGroup CreateScreenEffectGroup(
            Scene scene,
            Transform parent,
            string name)
        {
            GameObject groupObject = new GameObject(name, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(groupObject, scene);
            groupObject.transform.SetParent(parent, worldPositionStays: false);
            ConfigureFullScreenRect(groupObject.GetComponent<RectTransform>());
            CanvasGroup group = groupObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            EditorUtility.SetDirty(group);
            return group;
        }

        private static void CreateScreenEffectImage(
            Scene scene,
            Transform parent,
            string name,
            Color color,
            Vector2 anchor,
            Vector2 sizeDelta,
            float zRotation)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(imageObject, scene);
            imageObject.transform.SetParent(parent, worldPositionStays: false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            if (sizeDelta == Vector2.zero)
            {
                ConfigureFullScreenRect(rect);
            }
            else
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = sizeDelta;
            }

            rect.localRotation = Quaternion.Euler(0f, 0f, zRotation);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            EditorUtility.SetDirty(rect);
            EditorUtility.SetDirty(image);
        }

        private static void ConfigureFullScreenRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void EnsureInvasionFadeCues(Scene scene)
        {
            IntroGatePodCutsceneCueDirector cueDirector =
                FindComponentInScene<IntroGatePodCutsceneCueDirector>(scene);
            if (cueDirector == null)
            {
                return;
            }

            List<IntroGatePodCutsceneCueDirector.FadeCue> fadeCues =
                new List<IntroGatePodCutsceneCueDirector.FadeCue>(cueDirector.FadeCues);
            float explosionStartSeconds = ResolveInvasionExplosionStartSeconds(ResolveInvasionBridgeStartSeconds());
            AddFadeCueIfMissing(
                fadeCues,
                "invasion_pre_impact_black_snap",
                explosionStartSeconds - 0.075f,
                0.075f,
                0f,
                0.58f);
            AddFadeCueIfMissing(
                fadeCues,
                "invasion_impact_black_recover",
                explosionStartSeconds,
                0.24f,
                0.58f,
                0f);
            cueDirector.Configure(
                cueDirector.DollyCues,
                cueDirector.VoiceCues,
                fadeCues.ToArray(),
                false,
                true);
            EditorUtility.SetDirty(cueDirector);
        }

        private static void AddFadeCueIfMissing(
            List<IntroGatePodCutsceneCueDirector.FadeCue> fadeCues,
            string cueId,
            float startSeconds,
            float durationSeconds,
            float fromAlpha,
            float toAlpha)
        {
            for (int i = 0; i < fadeCues.Count; i++)
            {
                if (string.Equals(fadeCues[i].CueId, cueId, StringComparison.Ordinal))
                {
                    return;
                }
            }

            fadeCues.Add(new IntroGatePodCutsceneCueDirector.FadeCue(
                cueId,
                startSeconds,
                durationSeconds,
                fromAlpha,
                toAlpha));
        }

        private static void EnsureTimelineInvasionFadeClips()
        {
            TimelineAsset timeline = LoadAsset<TimelineAsset>(TimelinePath);
            IntroGatePodFadeTrack fadeTrack = FindTimelineTrack<IntroGatePodFadeTrack>(timeline, "Fade");
            if (fadeTrack == null)
            {
                throw new InvalidOperationException("Timeline is missing the Fade track.");
            }

            float explosionStartSeconds = ResolveInvasionExplosionStartSeconds(ResolveInvasionBridgeStartSeconds());
            EnsureFadeClip(
                fadeTrack,
                "Invasion Impact Blink In",
                explosionStartSeconds - 0.075f,
                0.075f,
                0f,
                0.58f);
            EnsureFadeClip(
                fadeTrack,
                "Invasion Impact Blink Out",
                explosionStartSeconds,
                0.24f,
                0.58f,
                0f);
            EditorUtility.SetDirty(fadeTrack);
            EditorUtility.SetDirty(timeline);
        }

        private static CinematicSequenceProfile ConfigureProfile()
        {
            EnsureFolder(PathParent(ProfilePath));
            CinematicSequenceProfile profile = AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CinematicSequenceProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            float voiceThreeStartSeconds = ResolveVoiceThreeStartSeconds();
            float introGameplayHandoffSeconds = ResolveIntroGameplayHandoffSeconds();
            float introDurationSeconds = ResolveIntroDurationSeconds();
            float scanLeftStartSeconds = ResolveScanCameraStartSeconds(voiceThreeStartSeconds);
            float scanRightStartSeconds = ResolveScanRightCameraStartSeconds(scanLeftStartSeconds);
            float handLookStartSeconds = ResolveHandLookCameraStartSeconds(scanRightStartSeconds);
            float invasionExplosionStartSeconds = ResolveInvasionExplosionStartSeconds(voiceThreeStartSeconds);
            float invasionPushStartSeconds = ResolveInvasionPushShotStartSeconds(voiceThreeStartSeconds);

            profile.Configure(
                "intro_gatepod_awakening",
                "Intro GatePod Awakening",
                CinematicSequenceProfile.SequenceCategory.IntroAwakening,
                "Source-referenced GatePod awakening pass using The Phantom Knowledge C01/C03/C04 camera timing, Fader timing, Cinemachine cameras, a Spline Dolly opening move, and a next-scene invasion bridge where voice 3 plays over Commando lower-body running and a heaven-background explosion.",
                introDurationSeconds,
                92,
                true,
                true,
                true,
                true,
                true,
                new[]
                {
                    ShotCamera(
                        "src_c01_capsule_left_dolly",
                        CinematicSequenceProfile.ShotPurpose.NewInformation,
                        CinematicSequenceProfile.CameraBlendKind.Ease,
                        0f,
                        SourceC03CameraStartSeconds,
                        new Vector3(2.35f, 1.75f, -7.10f),
                        new Vector3(0f, 1.64f, -0.92f),
                        31.2f),
                    ShotCamera(
                        "src_c03_first_person_eye_open",
                        CinematicSequenceProfile.ShotPurpose.EmotionChange,
                        CinematicSequenceProfile.CameraBlendKind.Cut,
                        SourceC03CameraStartSeconds,
                        scanLeftStartSeconds - SourceC03CameraStartSeconds,
                        FirstPersonViewMarkerPosition,
                        ResolveFirstPersonViewLookAt(4.2f),
                        46f),
                    ShotCamera(
                        "src_c04_first_person_scan_left",
                        CinematicSequenceProfile.ShotPurpose.ThreatDirection,
                        CinematicSequenceProfile.CameraBlendKind.Reframe,
                        scanLeftStartSeconds,
                        scanRightStartSeconds - scanLeftStartSeconds,
                        FirstPersonViewMarkerPosition,
                        ResolveFirstPersonViewLeftScanLookAt(),
                        48f),
                    ShotCamera(
                        "src_c05_first_person_scan_right",
                        CinematicSequenceProfile.ShotPurpose.ThreatDirection,
                        CinematicSequenceProfile.CameraBlendKind.Reframe,
                        scanRightStartSeconds,
                        handLookStartSeconds - scanRightStartSeconds,
                        FirstPersonViewMarkerPosition,
                        ResolveFirstPersonViewRightScanLookAt(),
                        48f),
                    ShotCamera(
                        "src_c06_first_person_look_down_hands",
                        CinematicSequenceProfile.ShotPurpose.EmotionChange,
                        CinematicSequenceProfile.CameraBlendKind.Reframe,
                        handLookStartSeconds,
                        voiceThreeStartSeconds - handLookStartSeconds,
                        FirstPersonViewMarkerPosition,
                        ResolveFirstPersonViewHandsLookAt(),
                        58f),
                    ShotCamera(
                        "src_c07_commando_bridge_legs_run",
                        CinematicSequenceProfile.ShotPurpose.ThreatDirection,
                        CinematicSequenceProfile.CameraBlendKind.Cut,
                        voiceThreeStartSeconds,
                        invasionExplosionStartSeconds - voiceThreeStartSeconds,
                        new Vector3(0.38f, 0.18f, 3.38f),
                        new Vector3(0.28f, 0.31f, 4.30f),
                        39f),
                    ShotCamera(
                        "src_c08_heaven_background_explosion",
                        CinematicSequenceProfile.ShotPurpose.NewInformation,
                        CinematicSequenceProfile.CameraBlendKind.Reframe,
                        invasionExplosionStartSeconds,
                        invasionPushStartSeconds - invasionExplosionStartSeconds,
                        new Vector3(-3.45f, 1.18f, 3.72f),
                        new Vector3(-0.88f, 1.55f, 7.18f),
                        56f),
                    ShotCamera(
                        "src_c09_commando_bridge_push_past",
                        CinematicSequenceProfile.ShotPurpose.ThreatDirection,
                        CinematicSequenceProfile.CameraBlendKind.PushIn,
                        invasionPushStartSeconds,
                        introGameplayHandoffSeconds - invasionPushStartSeconds,
                        new Vector3(-1.45f, 0.48f, 2.90f),
                        new Vector3(0.48f, 0.82f, 4.82f),
                        58f)
                },
                new[]
                {
                    WeaponVisibility("hide_attached_rifle_at_wake", 0f, RifleName, false),
                    Body("wake_confused_hands", handLookStartSeconds, 2.25f, IntroLookAtHandsStateName),
                    Face("wake_first_surprise", 0.58f, 1.5f, "Surprised"),
                    Face("scan_confused", 2.15f, 2.6f, "Confused"),
                    Body("body_reveal_surprise", 3.10f, 2.1f, "CIN_IntroSurprised"),
                    Body("weapon_pickup_motion", 8.20f, 2.0f, "CIN_IntroPickUp"),
                    WeaponVisibility("hide_floor_rifle_after_pickup", 10.20f, FloorRifleName, false),
                    WeaponVisibility("show_attached_rifle_for_handoff", 10.22f, RifleName, true),
                    Body(
                        "combat_ready_handoff",
                        handLookStartSeconds + 2.35f,
                        Mathf.Max(0.45f, introGameplayHandoffSeconds - handLookStartSeconds - 2.35f),
                        CombatReadyStateName),
                    Face("resolve_after_alarm", 10.48f, 3.1f, "Angry")
                },
                Array.Empty<CinematicSequenceProfile.VfxCue>(),
                Array.Empty<CinematicSequenceProfile.TutorialCue>(),
                new CinematicSequenceProfile.GameplayHandoffCue(
                    CinematicSequenceProfile.GameplayReturnMode.MatchGameplayBackView,
                    introGameplayHandoffSeconds,
                    "first_stage_intro_gatepod",
                    0.05f,
                    true,
                    true,
                    false));

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CinematicSequenceProfile.CameraCue ShotCamera(
            string cueId,
            CinematicSequenceProfile.ShotPurpose purpose,
            CinematicSequenceProfile.CameraBlendKind blendKind,
            float startSeconds,
            float durationSeconds,
            Vector3 cameraLocalPosition,
            Vector3 lookAtLocalPosition,
            float fieldOfView)
        {
            return new CinematicSequenceProfile.CameraCue(
                cueId,
                purpose,
                blendKind,
                startSeconds,
                durationSeconds,
                Vector3.zero,
                0f,
                0f,
                0f,
                0f,
                cameraLocalPosition,
                lookAtLocalPosition,
                fieldOfView);
        }

        private static CinematicSequenceProfile.ActorCue Body(
            string cueId,
            float startSeconds,
            float durationSeconds,
            string stateName)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                CinematicSequenceProfile.ActorRole.Inori,
                CinematicSequenceProfile.ActorCueKind.BodyState,
                startSeconds,
                durationSeconds,
                stateName);
        }

        private static CinematicSequenceProfile.ActorCue Face(
            string cueId,
            float startSeconds,
            float durationSeconds,
            string faceStateName)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                CinematicSequenceProfile.ActorRole.Inori,
                CinematicSequenceProfile.ActorCueKind.FaceState,
                startSeconds,
                durationSeconds,
                string.Empty,
                faceStateName: faceStateName);
        }

        private static CinematicSequenceProfile.ActorCue WeaponVisibility(
            string cueId,
            float startSeconds,
            string objectPath,
            bool active)
        {
            return new CinematicSequenceProfile.ActorCue(
                cueId,
                CinematicSequenceProfile.ActorRole.Inori,
                CinematicSequenceProfile.ActorCueKind.WeaponVisibility,
                startSeconds,
                0f,
                string.Empty,
                socketPath: objectPath,
                requireSocket: true,
                objectActive: active);
        }

        private static float ResolveVoiceZeroStartSeconds()
        {
            return Mathf.Max(
                0.20f,
                ResolveVoiceOneStartSeconds()
                - ResolveAudioClipLengthSeconds(VoiceZeroPath, 1.056f)
                - VoiceGapAfterLineSeconds);
        }

        private static float ResolveVoiceOneStartSeconds()
        {
            return SourceC01CameraEndSeconds;
        }

        private static float ResolveVoiceTwoStartSeconds()
        {
            return SourceC03CameraStartSeconds + VoiceTwoStartOffsetSeconds;
        }

        private static float ResolveVoiceThreeStartSeconds()
        {
            return ResolveInvasionBridgeStartSeconds();
        }

        private static float ResolveScanCameraStartSeconds(float voiceThreeStartSeconds)
        {
            return SourceC04CameraStartSeconds;
        }

        private static float ResolveScanRightCameraStartSeconds(float scanLeftStartSeconds)
        {
            return scanLeftStartSeconds + ScanSideHoldSeconds;
        }

        private static float ResolveHandLookCameraStartSeconds(float scanRightStartSeconds)
        {
            return scanRightStartSeconds + HandLookHoldLeadSeconds;
        }

        private static float ResolveInvasionBridgeStartSeconds()
        {
            float scanLeftStartSeconds = SourceC04CameraStartSeconds;
            float scanRightStartSeconds = ResolveScanRightCameraStartSeconds(scanLeftStartSeconds);
            float handLookStartSeconds = ResolveHandLookCameraStartSeconds(scanRightStartSeconds);
            return handLookStartSeconds + InvasionBridgeHandLookHoldSeconds;
        }

        private static float ResolveInvasionExplosionStartSeconds(float invasionBridgeStartSeconds)
        {
            return invasionBridgeStartSeconds + InvasionBridgeExplosionOffsetSeconds;
        }

        private static float ResolveInvasionPushShotStartSeconds(float invasionBridgeStartSeconds)
        {
            return invasionBridgeStartSeconds + InvasionBridgePushShotOffsetSeconds;
        }

        private static float ResolveIntroGameplayHandoffSeconds()
        {
            float invasionBridgeStartSeconds = ResolveInvasionBridgeStartSeconds();
            return Mathf.Max(
                FallbackIntroGameplayHandoffSeconds,
                invasionBridgeStartSeconds
                + Mathf.Max(
                    InvasionBridgeRunDurationSeconds,
                    ResolveAudioClipLengthSeconds(VoiceThreePath, 3.267f) + 0.85f));
        }

        private static float ResolveIntroDurationSeconds()
        {
            return ResolveIntroGameplayHandoffSeconds() + 0.65f;
        }

        private static float ResolveAudioClipLengthSeconds(string clipPath, float fallbackSeconds)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            return clip != null && clip.length > 0.01f ? clip.length : fallbackSeconds;
        }

        private static void ConfigureGateModelImporter()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GateModelPath) == null)
            {
                AssetDatabase.ImportAsset(GateModelPath, ImportAssetOptions.ForceSynchronousImport);
            }

            ModelImporter importer = AssetImporter.GetAtPath(GateModelPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing GatePod model importer at {GateModelPath}.");
            }

            bool changed = false;
            changed |= SetIfDifferent(() => importer.importAnimation, value => importer.importAnimation = value, false);
            changed |= SetIfDifferent(() => importer.importCameras, value => importer.importCameras = value, false);
            changed |= SetIfDifferent(() => importer.importLights, value => importer.importLights = value, false);
            changed |= SetIfDifferent(
                () => importer.materialImportMode,
                value => importer.materialImportMode = value,
                ModelImporterMaterialImportMode.None);
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void EnsureCinematicControllerReady()
        {
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    BuildResubmissionCinematicAnimationSetup.CinematicControllerPath);
            if (controller == null)
            {
                BuildResubmissionCinematicAnimationSetup.RebuildInoriCinematicP0Animations();
            }
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.30f, 0.30f, 0.34f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.09f, 0.10f, 0.13f, 1f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.015f;
        }

        private static void CreateDirectionalLight(Scene scene)
        {
            GameObject lightObject = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.96f, 0.98f, 1.0f, 1f);
            light.intensity = 0.75f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private static void CreateStageDressing(Scene scene, Transform parent)
        {
            GameObject root = new GameObject(StageRootName);
            root.transform.SetParent(parent, worldPositionStays: false);

            Material floor = LoadOrCreateMaterial(
                StageFloorMaterialPath,
                new Color(0.50f, 0.52f, 0.58f, 1f),
                Color.black,
                0.54f,
                0.03f);
            Material stone = LoadOrCreateMaterial(
                StageStoneMaterialPath,
                new Color(0.68f, 0.70f, 0.76f, 1f),
                Color.black,
                0.40f,
                0.02f);
            Material warning = LoadOrCreateMaterial(
                StageWarningMaterialPath,
                new Color(0.95f, 0.22f, 0.32f, 1f),
                new Color(1.4f, 0.12f, 0.18f, 1f),
                0.62f,
                0.0f);
            Material gold = LoadOrCreateMaterial(
                StageGoldMaterialPath,
                new Color(0.95f, 0.78f, 0.36f, 1f),
                new Color(0.22f, 0.12f, 0.02f, 1f),
                0.58f,
                0.08f);
            Material podGlow = LoadOrCreateMaterial(
                PodGlowMaterialPath,
                new Color(0.10f, 0.82f, 0.90f, 1f),
                new Color(0.0f, 1.45f, 1.65f, 1f),
                0.48f,
                0.0f);

            CreatePanelCube(root.transform, "IntroGatePodReview_Floor", new Vector3(0f, -0.07f, 1.9f), Quaternion.identity, new Vector3(7.2f, 0.12f, 10.8f), floor);
            CreatePanelCube(root.transform, "IntroGatePodReview_PodBaseReadability", new Vector3(0f, -0.015f, -0.10f), Quaternion.identity, new Vector3(2.35f, 0.035f, 2.10f), stone);
            CreatePanelCube(root.transform, "IntroGatePodReview_LeftTemplePier", new Vector3(-5.80f, 1.15f, 7.25f), Quaternion.identity, new Vector3(0.22f, 2.38f, 1.80f), stone);
            CreatePanelCube(root.transform, "IntroGatePodReview_RightTemplePier", new Vector3(5.80f, 1.15f, 7.25f), Quaternion.identity, new Vector3(0.22f, 2.38f, 1.80f), stone);
            CreatePanelCube(root.transform, "IntroGatePodReview_HeavenTrimL", new Vector3(-1.82f, 0.02f, 2.10f), Quaternion.Euler(0f, -6f, 0f), new Vector3(0.045f, 0.04f, 7.60f), gold);
            CreatePanelCube(root.transform, "IntroGatePodReview_HeavenTrimR", new Vector3(1.82f, 0.02f, 2.10f), Quaternion.Euler(0f, 6f, 0f), new Vector3(0.045f, 0.04f, 7.60f), gold);
            CreatePanelCube(root.transform, "IntroGatePodReview_PodCyanGroundLine", new Vector3(0f, 0.01f, -0.95f), Quaternion.identity, new Vector3(1.70f, 0.025f, 0.045f), podGlow);
            CreatePanelCube(root.transform, "IntroGatePodReview_BreachBackPlate", new Vector3(0f, 1.28f, 7.30f), Quaternion.identity, new Vector3(3.55f, 2.20f, 0.12f), stone);
            CreatePanelCube(root.transform, "IntroGatePodReview_InvasionSlashA", new Vector3(-0.72f, 1.72f, 7.22f), Quaternion.Euler(0f, 0f, -24f), new Vector3(0.09f, 1.85f, 0.08f), warning);
            CreatePanelCube(root.transform, "IntroGatePodReview_InvasionSlashB", new Vector3(0.58f, 1.28f, 7.21f), Quaternion.Euler(0f, 0f, 32f), new Vector3(0.08f, 1.35f, 0.08f), warning);

            CreateReviewLight(root.transform, "IntroGatePodReview_PodKeyCyanLight", new Vector3(0.0f, 1.20f, -0.55f), new Color(0.20f, 0.95f, 1.0f, 1f), 2.4f, 4.2f);
            CreateReviewLight(root.transform, "IntroGatePodReview_InvasionRimLight", new Vector3(-2.25f, 1.45f, 3.25f), new Color(1.0f, 0.16f, 0.25f, 1f), 1.0f, 6.2f);
            CreateReviewLight(root.transform, "IntroGatePodReview_FaceSoftFill", new Vector3(0.72f, 1.95f, 1.35f), new Color(0.84f, 0.90f, 1.0f, 1f), 1.25f, 4.8f);
        }

        private static Transform CreateThreatAnchor(Scene scene, Transform parent)
        {
            GameObject anchor = new GameObject(ThreatAnchorName);
            anchor.transform.SetParent(parent, worldPositionStays: false);
            anchor.transform.localPosition = new Vector3(0f, 1.20f, 5.30f);
            return anchor.transform;
        }

        private static IntroGatePodInvasionBridgeCue CreateInvasionBridge(
            Scene scene,
            Transform parent,
            PlayableDirector director,
            Camera impactCamera)
        {
            GameObject bridgeRoot = new GameObject(InvasionBridgeRootName);
            SceneManager.MoveGameObjectToScene(bridgeRoot, scene);
            bridgeRoot.transform.SetParent(parent, worldPositionStays: false);

            Material floor = LoadOrCreateMaterial(
                StageFloorMaterialPath,
                new Color(0.50f, 0.52f, 0.58f, 1f),
                Color.black,
                0.54f,
                0.03f);
            Material warning = LoadOrCreateMaterial(
                StageWarningMaterialPath,
                new Color(0.95f, 0.22f, 0.32f, 1f),
                new Color(1.4f, 0.12f, 0.18f, 1f),
                0.62f,
                0.0f);
            CreatePanelCube(
                bridgeRoot.transform,
                "IntroGatePodReview_CommandoBridgeDeck",
                new Vector3(0f, 0.015f, 5.15f),
                Quaternion.identity,
                new Vector3(4.45f, 0.035f, 4.35f),
                floor);
            CreatePanelCube(
                bridgeRoot.transform,
                "IntroGatePodReview_CommandoBridgeWarningLine",
                new Vector3(0f, 0.055f, 4.05f),
                Quaternion.identity,
                new Vector3(3.70f, 0.022f, 0.055f),
                warning);

            GameObject commandoGroup = new GameObject(InvasionCommandoGroupName);
            SceneManager.MoveGameObjectToScene(commandoGroup, scene);
            commandoGroup.transform.SetParent(bridgeRoot.transform, worldPositionStays: false);

            float startSeconds = ResolveInvasionBridgeStartSeconds();
            float endSeconds = startSeconds + InvasionBridgeRunDurationSeconds;
            IntroGatePodInvasionBridgeCue.CommandoCue[] commandos =
            {
                CreateCommandoCue(
                    scene,
                    commandoGroup.transform,
                    1,
                    new Vector3(-0.72f, 0.02f, 5.85f),
                    new Vector3(-0.42f, 0.02f, 1.82f),
                    0.00f,
                    startSeconds,
                    endSeconds),
                CreateCommandoCue(
                    scene,
                    commandoGroup.transform,
                    2,
                    new Vector3(0.40f, 0.02f, 6.15f),
                    new Vector3(0.20f, 0.02f, 2.08f),
                    0.37f,
                    startSeconds + 0.10f,
                    endSeconds + 0.12f),
                CreateCommandoCue(
                    scene,
                    commandoGroup.transform,
                    3,
                    new Vector3(1.08f, 0.02f, 5.55f),
                    new Vector3(0.82f, 0.02f, 1.55f),
                    0.68f,
                    startSeconds + 0.24f,
                    endSeconds + 0.22f)
            };

            GameObject explosionRoot = CreateInvasionExplosion(scene, bridgeRoot.transform);
            Light explosionLight = explosionRoot.GetComponentInChildren<Light>(includeInactive: true);

            IntroGatePodInvasionBridgeCue cue = bridgeRoot.AddComponent<IntroGatePodInvasionBridgeCue>();
            cue.Configure(
                director,
                commandos,
                explosionRoot,
                explosionLight,
                ResolveInvasionExplosionStartSeconds(startSeconds),
                InvasionBridgeExplosionDurationSeconds,
                Vector3.one * 0.10f,
                new Vector3(1.18f, 0.78f, 1.18f),
                8.6f);
            InvasionScreenEffectBindings screenEffects =
                CreateInvasionScreenEffectOverlay(scene, parent);
            cue.ConfigurePresentation(
                impactCamera,
                screenEffects.ImpactFlashGroup,
                screenEffects.WarningSweepGroup,
                2.65f,
                0.42f,
                0.62f,
                0.78f,
                new Vector3(0.060f, 0.044f, 0.012f),
                new Vector3(1.55f, 2.15f, 0.72f),
                0.78f);

            EditorUtility.SetDirty(cue);
            return cue;
        }

        private static IntroGatePodInvasionBridgeCue.CommandoCue CreateCommandoCue(
            Scene scene,
            Transform parent,
            int index,
            Vector3 startLocalPosition,
            Vector3 endLocalPosition,
            float normalizedTimeOffset,
            float startSeconds,
            float endSeconds)
        {
            GameObject prefab = LoadAsset<GameObject>(SciFiCommandoPrefabPath);
            GameObject actor = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (actor == null)
            {
                throw new InvalidOperationException($"Failed to instantiate Commando prefab from {SciFiCommandoPrefabPath}.");
            }

            actor.name = $"IntroGatePodReview_Commando_{index:00}";
            actor.transform.localPosition = startLocalPosition;
            actor.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            actor.transform.localScale = Vector3.one * (index == 2 ? 0.88f : 0.82f);

            DisableCutsceneActorGameplay(actor);
            Animator animator = actor.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator != null)
            {
                RuntimeAnimatorController controller =
                    AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SciFiCommandoControllerPath);
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                }

                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Play(SciFiCommandoRunStateName, 0, normalizedTimeOffset);
                animator.Update(0f);
                EditorUtility.SetDirty(animator);
            }

            actor.SetActive(false);
            EditorUtility.SetDirty(actor);
            return new IntroGatePodInvasionBridgeCue.CommandoCue(
                actor.transform,
                animator,
                SciFiCommandoRunStateName,
                startSeconds,
                endSeconds,
                startLocalPosition,
                endLocalPosition,
                new Vector3(0f, 180f, 0f),
                normalizedTimeOffset);
        }

        private static void DisableCutsceneActorGameplay(GameObject actor)
        {
            MonoBehaviour[] behaviours = actor.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null)
                {
                    behaviours[i].enabled = false;
                    EditorUtility.SetDirty(behaviours[i]);
                }
            }
        }

        private static GameObject CreateInvasionExplosion(Scene scene, Transform parent)
        {
            GameObject root = new GameObject(InvasionExplosionRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.localPosition = new Vector3(-1.08f, 1.58f, 7.22f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one * 0.10f;

            EnsureInvasionExplosionVfx(scene, root.transform);

            Light light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.42f, 0.16f, 1f);
            light.range = 7.6f;
            light.intensity = 0f;

            root.SetActive(false);
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(light);
            return root;
        }

        private static void TryCreateVfxChild(
            Scene scene,
            Transform parent,
            string prefabPath,
            string name,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                return;
            }

            instance.name = name;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = localScale;
            EditorUtility.SetDirty(instance);
        }

        private static Transform CreateFirstPersonViewMarker(Scene scene, Transform parent)
        {
            GameObject marker = new GameObject(FirstPersonViewMarkerName);
            SceneManager.MoveGameObjectToScene(marker, scene);
            marker.transform.SetParent(parent, worldPositionStays: false);
            marker.transform.SetLocalPositionAndRotation(
                FirstPersonViewMarkerPosition,
                FirstPersonViewMarkerRotation);
            EditorUtility.SetDirty(marker);
            return marker.transform;
        }

        private static IntroGatePodTimelineFadeOverlay CreateTimelineFadeOverlay(Scene scene, Transform parent)
        {
            GameObject fadeObject = new GameObject(FadeOverlayName, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(fadeObject, scene);
            fadeObject.transform.SetParent(parent, worldPositionStays: false);
            RectTransform fadeRect = fadeObject.GetComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.offsetMin = Vector2.zero;
            fadeRect.offsetMax = Vector2.zero;

            Canvas canvas = fadeObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler canvasScaler = fadeObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            CanvasGroup canvasGroup = fadeObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            GameObject blackoutObject = new GameObject("IntroGatePodReview_TimelineFadeBlackout", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(blackoutObject, scene);
            blackoutObject.transform.SetParent(fadeObject.transform, worldPositionStays: false);
            RectTransform blackoutRect = blackoutObject.GetComponent<RectTransform>();
            blackoutRect.anchorMin = Vector2.zero;
            blackoutRect.anchorMax = Vector2.one;
            blackoutRect.offsetMin = Vector2.zero;
            blackoutRect.offsetMax = Vector2.zero;

            Image blackoutImage = blackoutObject.AddComponent<Image>();
            blackoutImage.color = Color.black;
            blackoutImage.raycastTarget = false;

            IntroGatePodTimelineFadeOverlay fadeOverlay =
                fadeObject.AddComponent<IntroGatePodTimelineFadeOverlay>();
            fadeOverlay.Configure(canvasGroup);
            fadeOverlay.Alpha = 0f;
            EditorUtility.SetDirty(canvas);
            EditorUtility.SetDirty(canvasScaler);
            EditorUtility.SetDirty(canvasGroup);
            EditorUtility.SetDirty(blackoutImage);
            EditorUtility.SetDirty(fadeOverlay);
            return fadeOverlay;
        }

        private static Vector3 ResolveFirstPersonViewLookAt(float distance)
        {
            return FirstPersonViewMarkerPosition
                + (FirstPersonViewMarkerRotation * Vector3.forward * Mathf.Max(0.1f, distance));
        }

        private static Vector3 ResolveFirstPersonViewLeftScanLookAt()
        {
            Quaternion scanRotation = FirstPersonViewMarkerRotation * Quaternion.Euler(-5.5f, -23f, 0f);
            return FirstPersonViewMarkerPosition + (scanRotation * Vector3.forward * 6.2f);
        }

        private static Vector3 ResolveFirstPersonViewRightScanLookAt()
        {
            Quaternion scanRotation = FirstPersonViewMarkerRotation * Quaternion.Euler(-4f, 22f, 0f);
            return FirstPersonViewMarkerPosition + (scanRotation * Vector3.forward * 6.2f);
        }

        private static Vector3 ResolveFirstPersonViewHandsLookAt()
        {
            Vector3 forward = FirstPersonViewMarkerRotation * Vector3.forward;
            Vector3 right = FirstPersonViewMarkerRotation * Vector3.right;
            return FirstPersonViewMarkerPosition
                + (forward * 0.48f)
                + (right * 0.05f)
                + (Vector3.down * 0.72f);
        }

        private static GameObject CreateGatePods(Scene scene, Transform parent)
        {
            GameObject source = LoadAsset<GameObject>(GateModelPath);
            GameObject gateInstance = PrefabUtility.InstantiatePrefab(source, scene) as GameObject;
            if (gateInstance == null)
            {
                throw new InvalidOperationException("Failed to instantiate GatePod source model.");
            }

            PrefabUtility.UnpackPrefabInstance(
                gateInstance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);

            Transform pods = FindDescendant(gateInstance.transform, "gate:pods")
                ?? FindDescendantContains(gateInstance.transform, "pods");
            if (pods == null)
            {
                UnityEngine.Object.DestroyImmediate(gateInstance);
                throw new InvalidOperationException($"{GateModelPath} does not contain gate:pods.");
            }

            GameObject holder = new GameObject(GatePodRootName);
            holder.transform.SetParent(parent, worldPositionStays: false);
            pods.SetParent(holder.transform, worldPositionStays: true);
            UnityEngine.Object.DestroyImmediate(gateInstance);

            AssignGatePodMaterials(holder);
            holder.transform.rotation = Quaternion.identity;
            holder.transform.localScale = Vector3.one;
            FitRootToBounds(holder.transform, 5.00f, new Vector3(0f, 0f, -0.58f));
            EditorUtility.SetDirty(holder);
            return holder;
        }

        private static GameObject CreateInoriActor(Scene scene, Transform parent)
        {
            GameObject sourcePrefab = LoadAsset<GameObject>(ActionFoundationInoriPlayerVisualAssetSetup.SourcePrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("Failed to instantiate Inori source prefab.");
            }

            PrefabUtility.UnpackPrefabInstance(
                instance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            instance.name = InoriRootName;
            instance.transform.SetParent(parent, worldPositionStays: false);
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;

            Animator animator = instance.GetComponentInChildren<Animator>(includeInactive: true)
                ?? instance.AddComponent<Animator>();
            animator.runtimeAnimatorController = LoadAsset<RuntimeAnimatorController>(
                BuildResubmissionCinematicAnimationSetup.CinematicControllerPath);
            animator.avatar = ActionFoundationInoriPlayerVisualAssetSetup.LoadPromotedAvatar();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            PlaceInoriForFirstPersonCapsule(instance, animator);
            AssignInoriPromotedMaterials(instance);
            return instance;
        }

        private static GameObject CreateInoriPlacementRoot(Scene scene, Transform parent, GameObject inori)
        {
            if (inori == null)
            {
                throw new InvalidOperationException("Cannot create Inori placement root without Inori.");
            }

            GameObject placement = new GameObject(InoriPlacementRootName);
            SceneManager.MoveGameObjectToScene(placement, scene);
            placement.transform.SetParent(parent, worldPositionStays: false);
            placement.transform.SetPositionAndRotation(inori.transform.position, inori.transform.rotation);
            placement.transform.localScale = Vector3.one;

            inori.transform.SetParent(placement.transform, worldPositionStays: true);
            inori.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            inori.transform.localScale = Vector3.one;

            Animator animator = placement.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            EditorUtility.SetDirty(placement);
            EditorUtility.SetDirty(inori);
            EditorUtility.SetDirty(animator);
            return placement;
        }

        private static void PlaceInoriForFirstPersonCapsule(GameObject inori, Animator animator)
        {
            if (inori == null)
            {
                return;
            }

            Quaternion bodyRotation = ResolveFirstPersonBodyRotation();
            inori.transform.SetLocalPositionAndRotation(
                ResolveFirstPersonBodyRootPosition(bodyRotation),
                bodyRotation);

            if (animator == null)
            {
                EditorUtility.SetDirty(inori);
                return;
            }

            SampleInoriBodyState(animator, IntroLookAtHandsStateName, 0.38f);
            AlignInoriHandsToFirstPersonView(inori, animator);
            SampleInoriBodyState(animator, IntroLookAtHandsStateName, 0.38f);
            EditorUtility.SetDirty(inori);
            EditorUtility.SetDirty(animator);
        }

        private static void AlignInoriHandsToFirstPersonView(GameObject inori, Animator animator)
        {
            if (inori == null || animator == null || !animator.isHuman)
            {
                return;
            }

            Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (leftHand == null || rightHand == null)
            {
                return;
            }

            Vector3 currentHandCenter = (leftHand.position + rightHand.position) * 0.5f;
            Vector3 viewForward = (ResolveFirstPersonViewHandsLookAt() - FirstPersonViewMarkerPosition).normalized;
            Vector3 targetHandCenter = FirstPersonViewMarkerPosition + (viewForward * 0.88f);
            inori.transform.position += targetHandCenter - currentHandCenter;
        }

        private static Quaternion ResolveFirstPersonBodyRotation()
        {
            Vector3 bodyForward = Vector3.ProjectOnPlane(
                FirstPersonViewMarkerRotation * Vector3.forward,
                Vector3.up);
            if (bodyForward.sqrMagnitude < 0.0001f)
            {
                bodyForward = Vector3.forward;
            }

            return Quaternion.LookRotation(bodyForward.normalized, Vector3.up);
        }

        private static Vector3 ResolveFirstPersonBodyRootPosition(Quaternion bodyRotation)
        {
            Vector3 bodyForward = bodyRotation * Vector3.forward;
            Vector3 bodyRight = bodyRotation * Vector3.right;
            return FirstPersonViewMarkerPosition
                - (bodyForward * 0.52f)
                - (bodyRight * 0.04f)
                - (Vector3.up * 0.96f);
        }

        private static void SampleInoriBodyState(Animator animator, string stateName, float normalizedTime)
        {
            if (animator == null || string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            animator.Rebind();
            animator.Update(0f);
            animator.Play(stateName, 0, Mathf.Clamp01(normalizedTime));
            animator.Update(0.01f);
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

        private static void CreateWeaponFloorProps(Scene scene, Transform inoriRoot)
        {
            GameObject rifle = InstantiateModelPrefab(RifleModelPath, scene, FloorRifleName);
            rifle.transform.SetParent(inoriRoot, worldPositionStays: false);
            rifle.transform.localPosition = new Vector3(0.38f, 0.12f, 0.82f);
            rifle.transform.localRotation = Quaternion.Euler(10f, -28f, 92f);
            rifle.transform.localScale = Vector3.one * 0.72f;

            GameObject sword = InstantiateModelPrefab(SwordModelPath, scene, FloorSwordName);
            sword.transform.SetParent(inoriRoot, worldPositionStays: false);
            sword.transform.localPosition = new Vector3(-0.42f, 0.06f, 0.92f);
            sword.transform.localRotation = Quaternion.Euler(6f, 36f, 86f);
            sword.transform.localScale = Vector3.one * 0.95f;
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

            GameObject socketObject = new GameObject("IntroGatePodReview_InoriRifleSocket");
            socketObject.transform.SetParent(targetRightHand, worldPositionStays: false);
            ApplyRetargetedRifleSocket(sourceWeapon, sourceRightHand, targetRightHand, socketObject.transform, tuningProfile);

            GameObject weapon = UnityEngine.Object.Instantiate(sourceWeapon.gameObject, socketObject.transform);
            weapon.name = RifleName;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.transform.localScale = sourceWeapon.localScale;
            RemoveConstraints(weapon);
            ApplyInoriRifleMeshCorrection(weapon.transform, tuningProfile);

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

        private static void ApplyInoriRifleMeshCorrection(
            Transform weaponRoot,
            InoriRiflePoseTuningProfile tuningProfile)
        {
            Transform rifleMesh = FindDescendant(weaponRoot, tuningProfile.RifleMeshName);
            if (rifleMesh == null)
            {
                throw new InvalidOperationException($"{weaponRoot.name} is missing {tuningProfile.RifleMeshName}.");
            }

            rifleMesh.localPosition = tuningProfile.RifleMeshLocalPosition;
            rifleMesh.localRotation = tuningProfile.RifleMeshLocalRotation;
            rifleMesh.localScale = Vector3.one;
            EditorUtility.SetDirty(rifleMesh);
        }

        private static ActionCameraController CreateReviewCamera(Scene scene, Transform target, Transform threat)
        {
            GameObject cameraObject = new GameObject(CameraName);
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 36f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 150f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";

            ActionCameraController cameraController = cameraObject.AddComponent<ActionCameraController>();
            SetObjectReference(cameraController, "target", target);
            SetObjectReference(cameraController, "threat", threat);
            SetVector3(cameraController, "cameraOffset", new Vector3(0f, 1.12f, -4.05f));
            SetVector3(cameraController, "lookOffset", new Vector3(0f, 1.16f, 0.95f));
            cameraController.enabled = false;

            CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
            brain.ShowDebugText = true;
            brain.ShowCameraFrustum = true;
            brain.IgnoreTimeScale = true;
            brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            brain.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            EditorUtility.SetDirty(brain);
            return cameraController;
        }

        private static IntroGatePodCinemachineShotPlayer CreateCinemachineShots(
            Scene scene,
            Transform root,
            CinematicSequenceProfile profile,
            Transform cueSpace,
            Transform gatePods,
            CinemachineBrain brain)
        {
            GameObject camerasRoot = new GameObject(CinemachineRootName);
            camerasRoot.transform.SetParent(root, worldPositionStays: false);

            Bounds openingFocusBounds = CalculateGatePodFocusBounds(gatePods);
            Vector3 openingLookAt = ResolveOpeningLookAt(openingFocusBounds);
            SplineContainer openingDollySpline = CreateOpeningDollySpline(
                camerasRoot.transform,
                openingFocusBounds,
                openingLookAt,
                31.2f);

            CinematicSequenceProfile.CameraCue[] cameraCues = profile.CameraCues;
            IntroGatePodCinemachineShotPlayer.Shot[] shots =
                new IntroGatePodCinemachineShotPlayer.Shot[cameraCues.Length];

            for (int i = 0; i < cameraCues.Length; i++)
            {
                CinematicSequenceProfile.CameraCue cue = cameraCues[i];
                GameObject cameraObject = new GameObject($"CM_{i + 1:00}_{SanitizeObjectName(cue.CueId)}");
                cameraObject.transform.SetParent(camerasRoot.transform, worldPositionStays: false);

                Vector3 position = cue.CameraLocalPosition;
                Vector3 lookAt = cue.LookAtLocalPosition;
                if (string.Equals(cue.CueId, OpeningDollyCueId, StringComparison.Ordinal))
                {
                    position = ToVector3(openingDollySpline.EvaluatePosition(0f));
                    lookAt = openingLookAt;
                }

                cameraObject.transform.SetPositionAndRotation(position, ResolveLookRotation(position, lookAt));
                GameObject lookAtObject = new GameObject($"{cameraObject.name}_LookAt");
                lookAtObject.transform.SetParent(camerasRoot.transform, worldPositionStays: false);
                lookAtObject.transform.position = lookAt;

                CinemachineCamera virtualCamera = cameraObject.AddComponent<CinemachineCamera>();
                virtualCamera.Priority = 0;
                virtualCamera.StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Never;
                virtualCamera.LookAt = lookAtObject.transform;
                LensSettings lens = LensSettings.Default;
                lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                lens.FieldOfView = cue.FieldOfView > 0f ? cue.FieldOfView : 36f;
                lens.NearClipPlane = 0.03f;
                lens.FarClipPlane = 150f;
                virtualCamera.Lens = lens;
                cameraObject.AddComponent<CinemachineHardLookAt>();

                if (string.Equals(cue.CueId, OpeningDollyCueId, StringComparison.Ordinal))
                {
                    Animator animator = cameraObject.AddComponent<Animator>();
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                    CinemachineSplineDolly dolly = cameraObject.AddComponent<CinemachineSplineDolly>();
                    dolly.Spline = openingDollySpline;
                    dolly.PositionUnits = PathIndexUnit.Normalized;
                    dolly.CameraPosition = 0f;
                    dolly.CameraRotation = CinemachineSplineDolly.RotationMode.Default;
                    dolly.Damping.Enabled = false;
                    EditorUtility.SetDirty(animator);
                    EditorUtility.SetDirty(dolly);
                }

                CinemachineBlendDefinition.Styles blendStyle = ResolveCinemachineBlendStyle(cue.BlendKind);
                float blendSeconds = i == 0
                    ? 0f
                    : ResolveCinemachineBlendSeconds(cue.BlendKind);
                shots[i] = new IntroGatePodCinemachineShotPlayer.Shot(
                    cue.CueId,
                    cue.StartSeconds,
                    virtualCamera,
                    blendStyle,
                    blendSeconds);
                EditorUtility.SetDirty(virtualCamera);
            }

            GameObject playerObject = new GameObject(CinemachineShotPlayerName);
            playerObject.transform.SetParent(root, worldPositionStays: false);
            IntroGatePodCinemachineShotPlayer shotPlayer =
                playerObject.AddComponent<IntroGatePodCinemachineShotPlayer>();
            shotPlayer.Configure(brain, shots, false, true);
            shotPlayer.enabled = false;
            EditorUtility.SetDirty(shotPlayer);
            return shotPlayer;
        }

        private static PlayableDirector CreateTimelineDirector(
            Scene scene,
            Transform root,
            CinematicSequenceProfile profile,
            CinemachineBrain brain,
            IntroGatePodCinemachineShotPlayer shotPlayer,
            Animator inoriPlacementAnimator,
            Animator inoriAnimator,
            IntroGatePodTimelineFadeOverlay fadeOverlay)
        {
            TimelineAsset timeline = CreateFreshTimelineAsset();
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = ResolveIntroDurationSeconds();
            timeline.editorSettings.frameRate = 30d;

            GameObject directorObject = new GameObject(TimelineDirectorName);
            SceneManager.MoveGameObjectToScene(directorObject, scene);
            directorObject.transform.SetParent(root, worldPositionStays: false);

            PlayableDirector director = directorObject.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.playOnAwake = true;
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.extrapolationMode = DirectorWrapMode.Hold;

            CreateCinemachineTimelineTrack(timeline, director, brain, shotPlayer.Shots);
            CreateOpeningDollyTimelineTrack(timeline, director, shotPlayer.Shots);
            CreateAudioTimelineTracks(timeline, director, directorObject.transform);
            CreateFadeTimelineTrack(timeline, director, fadeOverlay);
            CreateInoriBodyTimelineTrack(timeline, director, profile, inoriAnimator);

            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            AssetDatabase.SaveAssets();
            return director;
        }

        private static void CreateInoriPlacementTimelineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            Animator inoriPlacementAnimator)
        {
            if (inoriPlacementAnimator == null)
            {
                throw new InvalidOperationException("Missing Inori placement Animator for Timeline binding.");
            }

            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(InoriPlacementTrackName);
            track.trackOffset = TrackOffset.ApplySceneOffsets;
            director.SetGenericBinding(track, inoriPlacementAnimator);

            TimelineClip clip = track.CreateRecordableClip(InoriPlacementClipName);
            clip.displayName = InoriPlacementClipName;
            clip.start = 0d;
            clip.duration = ResolveIntroDurationSeconds();

            AnimationPlayableAsset animationAsset = clip.asset as AnimationPlayableAsset;
            if (animationAsset == null || animationAsset.clip == null)
            {
                throw new InvalidOperationException("Failed to create Inori placement animation clip.");
            }

            animationAsset.removeStartOffset = false;
            animationAsset.applyFootIK = false;
            animationAsset.loop = AnimationPlayableAsset.LoopMode.Off;

            AnimationClip animationClip = animationAsset.clip;
            animationClip.name = InoriPlacementClipName;
            animationClip.frameRate = 30f;
            animationClip.wrapMode = WrapMode.ClampForever;

            Transform placement = inoriPlacementAnimator.transform;
            float durationSeconds = ResolveIntroDurationSeconds();
            SetConstantTransformCurve(animationClip, "m_LocalPosition.x", placement.localPosition.x, durationSeconds);
            SetConstantTransformCurve(animationClip, "m_LocalPosition.y", placement.localPosition.y, durationSeconds);
            SetConstantTransformCurve(animationClip, "m_LocalPosition.z", placement.localPosition.z, durationSeconds);

            EditorUtility.SetDirty(inoriPlacementAnimator);
            EditorUtility.SetDirty(animationClip);
            EditorUtility.SetDirty(animationAsset);
            EditorUtility.SetDirty(track);
        }

        private static void SetConstantTransformCurve(
            AnimationClip clip,
            string propertyName,
            float value,
            float durationSeconds)
        {
            AnimationCurve curve = AnimationCurve.Constant(0f, Mathf.Max(0.05f, durationSeconds), value);
            clip.SetCurve(string.Empty, typeof(Transform), propertyName, curve);
        }

        private static void FixInoriRotationDrift()
        {
            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                throw new InvalidOperationException($"Missing TimelineAsset at {TimelinePath}.");
            }

            AnimationTrack placementTrack =
                FindTimelineTrack<AnimationTrack>(timeline, InoriPlacementTrackName);
            if (placementTrack != null)
            {
                foreach (TimelineClip timelineClip in placementTrack.GetClips())
                {
                    AnimationPlayableAsset animationAsset = timelineClip.asset as AnimationPlayableAsset;
                    AnimationClip animationClip = animationAsset != null ? animationAsset.clip : null;
                    if (animationClip == null)
                    {
                        continue;
                    }

                    RemoveTransformCurve(animationClip, "m_LocalRotation.x");
                    RemoveTransformCurve(animationClip, "m_LocalRotation.y");
                    RemoveTransformCurve(animationClip, "m_LocalRotation.z");
                    RemoveTransformCurve(animationClip, "m_LocalRotation.w");
                    RemoveTransformCurve(animationClip, "localEulerAnglesRaw.x");
                    RemoveTransformCurve(animationClip, "localEulerAnglesRaw.y");
                    RemoveTransformCurve(animationClip, "localEulerAnglesRaw.z");
                    RemoveTransformCurve(animationClip, "localEulerAnglesBaked.x");
                    RemoveTransformCurve(animationClip, "localEulerAnglesBaked.y");
                    RemoveTransformCurve(animationClip, "localEulerAnglesBaked.z");
                    RemoveTransformCurve(animationClip, "localEulerAngles.x");
                    RemoveTransformCurve(animationClip, "localEulerAngles.y");
                    RemoveTransformCurve(animationClip, "localEulerAngles.z");

                    EditorUtility.SetDirty(animationClip);
                    EditorUtility.SetDirty(animationAsset);
                }

                EditorUtility.SetDirty(placementTrack);
            }

            AnimationTrack bodyTrack =
                FindTimelineTrack<AnimationTrack>(timeline, InoriBodyTrackName);
            if (bodyTrack != null)
            {
                bodyTrack.trackOffset = TrackOffset.ApplySceneOffsets;
                SetTimelineMatchTargetFields(bodyTrack, PositionOnlyMatchTargetFields);
                foreach (TimelineClip timelineClip in bodyTrack.GetClips())
                {
                    ConfigureRootStableBodyTimelineClip(timelineClip);
                }

                EditorUtility.SetDirty(bodyTrack);
            }

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();
        }

        private static void RemoveTransformCurve(AnimationClip clip, string propertyName)
        {
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(Transform),
                propertyName);
            AnimationUtility.SetEditorCurve(clip, binding, null);
        }

        private static void CreateInoriBodyTimelineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            CinematicSequenceProfile profile,
            Animator inoriAnimator)
        {
            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(InoriBodyTrackName);
            track.trackOffset = TrackOffset.ApplySceneOffsets;
            SetTimelineMatchTargetFields(track, PositionOnlyMatchTargetFields);
            director.SetGenericBinding(track, inoriAnimator);

            CinematicSequenceProfile.ActorCue[] actorCues = profile.ActorCues;
            for (int i = 0; i < actorCues.Length; i++)
            {
                CinematicSequenceProfile.ActorCue cue = actorCues[i];
                if (!cue.Enabled
                    || cue.Role != CinematicSequenceProfile.ActorRole.Inori
                    || cue.CueKind != CinematicSequenceProfile.ActorCueKind.BodyState
                    || string.IsNullOrWhiteSpace(cue.AnimatorStateName))
                {
                    continue;
                }

                AnimationClip clip = LoadCinematicBodyClip(cue.AnimatorStateName);
                TimelineClip timelineClip = track.CreateClip(clip);
                timelineClip.displayName = cue.CueId;
                timelineClip.start = Mathf.Max(0f, cue.StartSeconds);
                timelineClip.duration = Mathf.Max(0.05f, cue.DurationSeconds);
                ConfigureRootStableBodyTimelineClip(timelineClip);
            }

            EditorUtility.SetDirty(track);
        }

        private static void ConfigureRootStableBodyTimelineClip(TimelineClip timelineClip)
        {
            if (timelineClip == null)
            {
                return;
            }

            SetTimelineClipExtrapolation(timelineClip, TimelineClip.ClipExtrapolation.None);

            AnimationPlayableAsset animationAsset = timelineClip.asset as AnimationPlayableAsset;
            if (animationAsset == null)
            {
                return;
            }

            animationAsset.removeStartOffset = false;
            SetTimelineMatchTargetFields(animationAsset, PositionOnlyMatchTargetFields);
            EditorUtility.SetDirty(animationAsset);
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

        private static void SetTimelineMatchTargetFields(UnityEngine.Object target, int matchTargetFields)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty matchFields = serializedObject.FindProperty("m_MatchTargetFields");
            if (matchFields != null)
            {
                matchFields.intValue = matchTargetFields;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static AnimationClip LoadCinematicBodyClip(string stateName)
        {
            string path = $"{BuildResubmissionCinematicAnimationSetup.AnimationRoot}/{stateName}.fbx";
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip
                    && string.Equals(clip.name, stateName, StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip
                    && !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            throw new InvalidOperationException($"Missing cinematic body clip {stateName} at {path}.");
        }

        private static IntroGatePodFirstPersonRendererMask CreateFirstPersonRendererMask(
            Scene scene,
            Transform root,
            PlayableDirector director,
            GameObject inori)
        {
            GameObject maskObject = new GameObject(FirstPersonRendererMaskName);
            SceneManager.MoveGameObjectToScene(maskObject, scene);
            maskObject.transform.SetParent(root, worldPositionStays: false);

            IntroGatePodFirstPersonRendererMask mask =
                maskObject.AddComponent<IntroGatePodFirstPersonRendererMask>();
            mask.Configure(
                director,
                ResolveFirstPersonHiddenRenderers(inori),
                SourceC03CameraStartSeconds,
                ResolveIntroGameplayHandoffSeconds());
            EditorUtility.SetDirty(mask);
            return mask;
        }

        private static Renderer[] ResolveFirstPersonHiddenRenderers(GameObject inori)
        {
            if (inori == null)
            {
                return Array.Empty<Renderer>();
            }

            Renderer[] renderers = inori.GetComponentsInChildren<Renderer>(includeInactive: true);
            List<Renderer> hiddenRenderers = new List<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && IsFirstPersonHeadOccluder(renderer.transform))
                {
                    hiddenRenderers.Add(renderer);
                }
            }

            return hiddenRenderers.ToArray();
        }

        private static bool IsFirstPersonHeadOccluder(Transform transform)
        {
            for (Transform current = transform; current != null; current = current.parent)
            {
                string name = current.name;
                if (name.IndexOf("Hair", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Head", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Face", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Brow", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Mouth", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (string.Equals(current.name, InoriRootName, StringComparison.Ordinal))
                {
                    break;
                }
            }

            return false;
        }

        private static TimelineAsset CreateFreshTimelineAsset()
        {
            EnsureFolder(PathParent(TimelinePath));
            TimelineAsset existing = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(TimelinePath);
            }

            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.name = Path.GetFileNameWithoutExtension(TimelinePath);
            AssetDatabase.CreateAsset(timeline, TimelinePath);
            return timeline;
        }

        private static void CreateCinemachineTimelineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            CinemachineBrain brain,
            IntroGatePodCinemachineShotPlayer.Shot[] shots)
        {
            CinemachineTrack track = timeline.CreateTrack<CinemachineTrack>("Cinemachine Shots");
            track.TrackPriority = 200;
            director.SetGenericBinding(track, brain);

            float sequenceEndSeconds = ResolveIntroGameplayHandoffSeconds();
            for (int i = 0; i < shots.Length; i++)
            {
                float blendSeconds = i == 0 ? 0f : shots[i].BlendSeconds;
                float clipStartSeconds = Mathf.Max(0f, shots[i].StartSeconds - blendSeconds);
                float nextStartSeconds = i < shots.Length - 1
                    ? shots[i + 1].StartSeconds
                    : sequenceEndSeconds;
                float clipDurationSeconds = Mathf.Max(0.1f, nextStartSeconds - clipStartSeconds);

                TimelineClip clip = track.CreateClip<CinemachineShot>();
                clip.displayName = shots[i].ShotId;
                clip.start = clipStartSeconds;
                clip.duration = clipDurationSeconds;
                if (blendSeconds > 0.001f)
                {
                    clip.blendInDuration = blendSeconds;
                    clip.easeInDuration = blendSeconds;
                }

                CinemachineShot shotAsset = clip.asset as CinemachineShot;
                if (shotAsset == null || shots[i].Camera == null)
                {
                    continue;
                }

                shotAsset.DisplayName = shots[i].ShotId;
                PropertyName exposedName = new PropertyName($"cm_{i + 1:00}_{shots[i].ShotId}");
                shotAsset.VirtualCamera.exposedName = exposedName;
                director.SetReferenceValue(exposedName, shots[i].Camera);
                EditorUtility.SetDirty(shotAsset);
            }

            EditorUtility.SetDirty(track);
        }

        private static void CreateOpeningDollyTimelineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            IntroGatePodCinemachineShotPlayer.Shot[] shots)
        {
            CinemachineSplineDolly openingDolly = ResolveOpeningDolly(shots);
            Animator openingAnimator = openingDolly.GetComponent<Animator>();
            if (openingAnimator == null)
            {
                openingAnimator = openingDolly.gameObject.AddComponent<Animator>();
            }

            openingAnimator.applyRootMotion = false;
            openingAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            AnimationTrack track = timeline.CreateTrack<AnimationTrack>(OpeningDollyTrackName);
            track.trackOffset = TrackOffset.ApplySceneOffsets;
            director.SetGenericBinding(track, openingAnimator);

            TimelineClip clip = track.CreateRecordableClip(OpeningDollyClipName);
            clip.displayName = OpeningDollyClipName;
            clip.start = 0d;
            clip.duration = SourceC03CameraStartSeconds;

            AnimationPlayableAsset animationAsset = clip.asset as AnimationPlayableAsset;
            if (animationAsset == null || animationAsset.clip == null)
            {
                throw new InvalidOperationException("Failed to create opening dolly animation clip.");
            }

            animationAsset.removeStartOffset = false;
            animationAsset.applyFootIK = false;
            animationAsset.loop = AnimationPlayableAsset.LoopMode.Off;

            AnimationClip animationClip = animationAsset.clip;
            animationClip.name = OpeningDollyClipName;
            animationClip.frameRate = 30f;
            animationClip.wrapMode = WrapMode.ClampForever;
            AnimationCurve dollyCurve = AnimationCurve.EaseInOut(0f, 0f, SourceC03CameraStartSeconds, 1f);
            animationClip.SetCurve(
                string.Empty,
                typeof(CinemachineSplineDolly),
                OpeningDollyCurveProperty,
                dollyCurve);

            EditorUtility.SetDirty(openingAnimator);
            EditorUtility.SetDirty(animationClip);
            EditorUtility.SetDirty(animationAsset);
            EditorUtility.SetDirty(track);
        }

        private static CinemachineSplineDolly ResolveOpeningDolly(
            IntroGatePodCinemachineShotPlayer.Shot[] shots)
        {
            for (int i = 0; i < shots.Length; i++)
            {
                if (!string.Equals(shots[i].ShotId, OpeningDollyCueId, StringComparison.Ordinal)
                    || shots[i].Camera == null)
                {
                    continue;
                }

                CinemachineSplineDolly dolly = shots[i].Camera.GetComponent<CinemachineSplineDolly>();
                if (dolly != null)
                {
                    return dolly;
                }
            }

            throw new InvalidOperationException("Opening capsule dolly shot is missing CinemachineSplineDolly.");
        }

        private static void CreateAudioTimelineTracks(
            TimelineAsset timeline,
            PlayableDirector director,
            Transform parent)
        {
            GameObject audioRoot = new GameObject(TimelineAudioRootName);
            audioRoot.transform.SetParent(parent, worldPositionStays: false);

            AudioSource voiceSource = CreateTimelineAudioSource(
                audioRoot.transform,
                TimelineVoiceAudioName,
                1f,
                96);
            AudioSource bgmSource = CreateTimelineAudioSource(
                audioRoot.transform,
                TimelineBgmAudioName,
                0.34f,
                160);

            AudioTrack voiceTrack = timeline.CreateTrack<AudioTrack>("Voice");
            director.SetGenericBinding(voiceTrack, voiceSource);
            CreateAudioClip(voiceTrack, "Voice 00", VoiceZeroPath, ResolveVoiceZeroStartSeconds(), false);
            CreateAudioClip(voiceTrack, "Voice 01", VoiceOnePath, ResolveVoiceOneStartSeconds(), false);
            CreateAudioClip(voiceTrack, "Voice 02", VoiceTwoPath, ResolveVoiceTwoStartSeconds(), false);
            CreateAudioClip(voiceTrack, "Voice 03", VoiceThreePath, ResolveVoiceThreeStartSeconds(), false);

            AudioTrack bgmTrack = timeline.CreateTrack<AudioTrack>("BGM");
            director.SetGenericBinding(bgmTrack, bgmSource);
            TimelineClip bgmClip = CreateAudioClip(bgmTrack, "BGM", BgmPath, 0f, true);
            bgmClip.duration = ResolveIntroDurationSeconds();

            EditorUtility.SetDirty(voiceTrack);
            EditorUtility.SetDirty(bgmTrack);
        }

        private static AudioSource CreateTimelineAudioSource(
            Transform parent,
            string objectName,
            float volume,
            int priority)
        {
            GameObject sourceObject = new GameObject(objectName);
            sourceObject.transform.SetParent(parent, worldPositionStays: false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = Mathf.Clamp01(volume);
            source.priority = priority;
            EditorUtility.SetDirty(source);
            return source;
        }

        private static TimelineClip CreateAudioClip(
            AudioTrack track,
            string displayName,
            string clipPath,
            float startSeconds,
            bool loop)
        {
            AudioClip audioClip = LoadAsset<AudioClip>(clipPath);
            TimelineClip timelineClip = track.CreateClip(audioClip);
            timelineClip.displayName = displayName;
            timelineClip.start = Mathf.Max(0f, startSeconds);
            AudioPlayableAsset asset = timelineClip.asset as AudioPlayableAsset;
            if (asset != null)
            {
                asset.loop = loop;
                EditorUtility.SetDirty(asset);
            }

            return timelineClip;
        }

        private static void CreateFadeTimelineTrack(
            TimelineAsset timeline,
            PlayableDirector director,
            IntroGatePodTimelineFadeOverlay fadeOverlay)
        {
            IntroGatePodFadeTrack fadeTrack = timeline.CreateTrack<IntroGatePodFadeTrack>("Fade");
            director.SetGenericBinding(fadeTrack, fadeOverlay);
            CreateFadeClip(fadeTrack, "Opening Fade In", 0f, SourceFadeInSeconds, 1f, 0f);
            CreateFadeClip(
                fadeTrack,
                "Pre-POV Blackout",
                SourceC03CameraStartSeconds - FirstPersonBlackoutLeadSeconds,
                FirstPersonBlackoutLeadSeconds,
                0f,
                1f);
            CreateFadeClip(
                fadeTrack,
                "POV Eye Open",
                SourceC03CameraStartSeconds,
                FirstPersonFadeInSeconds,
                1f,
                0f);
            float invasionExplosionStartSeconds =
                ResolveInvasionExplosionStartSeconds(ResolveInvasionBridgeStartSeconds());
            CreateFadeClip(
                fadeTrack,
                "Invasion Impact Blink In",
                invasionExplosionStartSeconds - 0.075f,
                0.075f,
                0f,
                0.58f);
            CreateFadeClip(
                fadeTrack,
                "Invasion Impact Blink Out",
                invasionExplosionStartSeconds,
                0.24f,
                0.58f,
                0f);
            EditorUtility.SetDirty(fadeTrack);
        }

        private static void CreateFadeClip(
            IntroGatePodFadeTrack track,
            string displayName,
            float startSeconds,
            float durationSeconds,
            float fromAlpha,
            float toAlpha)
        {
            TimelineClip clip = track.CreateClip<IntroGatePodFadeClip>();
            clip.displayName = displayName;
            clip.start = Mathf.Max(0f, startSeconds);
            clip.duration = Mathf.Max(0.05f, durationSeconds);
            IntroGatePodFadeClip asset = clip.asset as IntroGatePodFadeClip;
            if (asset != null)
            {
                asset.FromAlpha = fromAlpha;
                asset.ToAlpha = toAlpha;
                EditorUtility.SetDirty(asset);
            }
        }

        private static void EnsureFadeClip(
            IntroGatePodFadeTrack track,
            string displayName,
            float startSeconds,
            float durationSeconds,
            float fromAlpha,
            float toAlpha)
        {
            foreach (TimelineClip existingClip in track.GetClips())
            {
                if (string.Equals(existingClip.displayName, displayName, StringComparison.Ordinal))
                {
                    existingClip.start = Mathf.Max(0f, startSeconds);
                    existingClip.duration = Mathf.Max(0.05f, durationSeconds);
                    IntroGatePodFadeClip existingAsset = existingClip.asset as IntroGatePodFadeClip;
                    if (existingAsset != null)
                    {
                        existingAsset.FromAlpha = fromAlpha;
                        existingAsset.ToAlpha = toAlpha;
                        EditorUtility.SetDirty(existingAsset);
                    }

                    EditorUtility.SetDirty(track);
                    return;
                }
            }

            CreateFadeClip(track, displayName, startSeconds, durationSeconds, fromAlpha, toAlpha);
        }

        private static SplineContainer CreateOpeningDollySpline(
            Transform parent,
            Bounds focusBounds,
            Vector3 lookAt,
            float fieldOfView)
        {
            GameObject splineObject = new GameObject(OpeningDollySplineName);
            splineObject.transform.SetParent(parent, worldPositionStays: false);
            SplineContainer container = splineObject.AddComponent<SplineContainer>();

            Vector3 viewDirection = new Vector3(0.18f, 0.08f, -1f).normalized;
            float verticalRadians = fieldOfView * Mathf.Deg2Rad;
            float horizontalRadians = Camera.VerticalToHorizontalFieldOfView(fieldOfView, 16f / 9f) * Mathf.Deg2Rad;
            float halfHeightDistance = Mathf.Max(0.8f, focusBounds.extents.y) / Mathf.Tan(verticalRadians * 0.5f);
            float halfWidthDistance = Mathf.Max(0.8f, focusBounds.extents.x) / Mathf.Tan(horizontalRadians * 0.5f);
            float distance = Mathf.Max(halfHeightDistance, halfWidthDistance) * 1.16f;
            Vector3 basePosition = lookAt + (viewDirection * distance);
            Vector3 dollySide = Vector3.Cross(Vector3.up, (lookAt - basePosition).normalized).normalized;
            if (dollySide.sqrMagnitude < 0.0001f)
            {
                dollySide = Vector3.right;
            }

            float dollyWidth = Mathf.Clamp(focusBounds.size.x * 0.22f, 0.75f, 1.85f);
            container.Spline = new Spline(
                new[]
                {
                    ToFloat3(basePosition + (dollySide * dollyWidth)),
                    ToFloat3(basePosition),
                    ToFloat3(basePosition - (dollySide * dollyWidth))
                },
                TangentMode.AutoSmooth,
                closed: false);
            EditorUtility.SetDirty(container);
            return container;
        }

        private static Bounds CalculateGatePodFocusBounds(Transform gatePods)
        {
            if (gatePods == null)
            {
                return new Bounds(new Vector3(0f, 2.1f, -0.58f), new Vector3(3f, 4.2f, 2.2f));
            }

            Renderer[] renderers = gatePods.GetComponentsInChildren<Renderer>(includeInactive: true);
            bool hasBounds = false;
            Bounds bounds = default;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !IsGatePodFocusRenderer(renderer))
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

            if (!hasBounds)
            {
                return CreateGatePodLocalFocusBounds(gatePods);
            }

            if (bounds.size.x > 7.0f || bounds.size.z > 5.0f)
            {
                return CreateGatePodLocalFocusBounds(gatePods);
            }

            return bounds;
        }

        private static Bounds CreateGatePodLocalFocusBounds(Transform gatePods)
        {
            Vector3 baseCenter = gatePods != null
                ? gatePods.position
                : new Vector3(0f, 0f, -0.58f);
            return new Bounds(
                baseCenter + new Vector3(-5.10f, 2.25f, 0f),
                new Vector3(2.95f, 4.50f, 2.80f));
        }

        private static bool IsGatePodFocusRenderer(Renderer renderer)
        {
            if (renderer.name.IndexOf("MeshPart1", StringComparison.OrdinalIgnoreCase) >= 0
                || renderer.name.IndexOf("light", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null
                    && material.name.IndexOf("CyanGlow", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector3 ResolveOpeningLookAt(Bounds focusBounds)
        {
            Vector3 center = focusBounds.center;
            float y = Mathf.Lerp(focusBounds.min.y, focusBounds.max.y, 0.54f);
            return new Vector3(center.x, y, center.z);
        }

        private static IntroGatePodCutsceneCueDirector CreateCutsceneCueDirector(
            Scene scene,
            Transform root,
            IntroGatePodCinemachineShotPlayer shotPlayer)
        {
            CinemachineSplineDolly openingDolly = ResolveOpeningDolly(shotPlayer.Shots);

            GameObject directorObject = new GameObject(CueDirectorName);
            SceneManager.MoveGameObjectToScene(directorObject, scene);
            directorObject.transform.SetParent(root, worldPositionStays: false);

            IntroGatePodCutsceneCueDirector cueDirector =
                directorObject.AddComponent<IntroGatePodCutsceneCueDirector>();
            float invasionExplosionStartSeconds =
                ResolveInvasionExplosionStartSeconds(ResolveInvasionBridgeStartSeconds());
            cueDirector.Configure(
                new[]
                {
                    new IntroGatePodCutsceneCueDirector.DollyCue(
                        "opening_capsule_left_dolly",
                        0f,
                        SourceC03CameraStartSeconds,
                        openingDolly,
                        0f,
                        1f)
                },
                Array.Empty<IntroGatePodCutsceneCueDirector.VoiceCue>(),
                new[]
                {
                    new IntroGatePodCutsceneCueDirector.FadeCue(
                        "source_fader_opening_fade_in",
                        0f,
                        SourceFadeInSeconds,
                        1f,
                        0f),
                    new IntroGatePodCutsceneCueDirector.FadeCue(
                        "first_person_pre_cut_blackout",
                        SourceC03CameraStartSeconds - FirstPersonBlackoutLeadSeconds,
                        FirstPersonBlackoutLeadSeconds,
                        0f,
                        1f),
                    new IntroGatePodCutsceneCueDirector.FadeCue(
                        "first_person_eye_open",
                        SourceC03CameraStartSeconds,
                        FirstPersonFadeInSeconds,
                        1f,
                        0f),
                    new IntroGatePodCutsceneCueDirector.FadeCue(
                        "invasion_pre_impact_black_snap",
                        invasionExplosionStartSeconds - 0.075f,
                        0.075f,
                        0f,
                        0.58f),
                    new IntroGatePodCutsceneCueDirector.FadeCue(
                        "invasion_impact_black_recover",
                        invasionExplosionStartSeconds,
                        0.24f,
                        0.58f,
                        0f)
                },
                false,
                true);
            EditorUtility.SetDirty(cueDirector);
            return cueDirector;
        }

        private static CinematicSequenceRunner CreateRunner(
            Scene scene,
            CinematicSequenceProfile profile,
            GameObject inori,
            Animator inoriAnimator,
            CinematicBlendShapeExpressionPlayer expressionPlayer,
            ActionCameraController cameraController)
        {
            GameObject runnerObject = new GameObject(RunnerRootName);
            SceneManager.MoveGameObjectToScene(runnerObject, scene);

            CinematicSequenceRunner runner = runnerObject.AddComponent<CinematicSequenceRunner>();
            SerializedObject serializedRunner = new SerializedObject(runner);
            SetObjectReference(serializedRunner, "sequenceProfile", profile);
            SetObjectReference(
                serializedRunner,
                "bodyControllerOverride",
                LoadAsset<RuntimeAnimatorController>(BuildResubmissionCinematicAnimationSetup.CinematicControllerPath));
            SetObjectReference(serializedRunner, "cameraController", cameraController);
            SetObjectReference(serializedRunner, "cinematicCamera", cameraController.GetComponent<Camera>());
            RequireProperty(serializedRunner, "driveCameraTransformFromProfile").boolValue = false;
            RequireProperty(serializedRunner, "disableActionCameraControllerDuringPoseDrive").boolValue = true;
            SetObjectReference(serializedRunner, "cueSpace", inori.transform);

            SerializedProperty bindings = RequireProperty(serializedRunner, "actorBindings");
            bindings.arraySize = 1;
            SerializedProperty binding = bindings.GetArrayElementAtIndex(0);
            SetRelativeEnum(binding, "role", (int)CinematicSequenceProfile.ActorRole.Inori);
            SetRelativeObjectReference(binding, "bodyAnimator", inoriAnimator);
            SetRelativeObjectReference(binding, "faceAnimator", null);
            SetRelativeObjectReference(binding, "expressionPlayer", expressionPlayer);
            SetRelativeObjectReference(binding, "anchor", inori.transform);
            serializedRunner.ApplyModifiedPropertiesWithoutUndo();

            CinematicSequenceAutoPlay autoPlay = runnerObject.AddComponent<CinematicSequenceAutoPlay>();
            SetObjectReference(autoPlay, "runner", runner);
            SetBool(autoPlay, "playOnStart", false);
            SetFloat(autoPlay, "startDelaySeconds", 0.05f);

            EditorUtility.SetDirty(runner);
            EditorUtility.SetDirty(autoPlay);
            return runner;
        }

        private static void CaptureReviewSamples()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            CinematicSequenceRunner runner = FindComponentInScene<CinematicSequenceRunner>(scene)
                ?? throw new InvalidOperationException("Missing intro GatePod cinematic runner.");
            IntroGatePodCinemachineShotPlayer shotPlayer = FindComponentInScene<IntroGatePodCinemachineShotPlayer>(scene)
                ?? throw new InvalidOperationException("Missing intro GatePod Cinemachine shot player.");
            IntroGatePodCutsceneCueDirector cueDirector = FindComponentInScene<IntroGatePodCutsceneCueDirector>(scene)
                ?? throw new InvalidOperationException("Missing intro GatePod cutscene cue director.");
            IntroGatePodFirstPersonRendererMask rendererMask =
                FindComponentInScene<IntroGatePodFirstPersonRendererMask>(scene);
            IntroGatePodInvasionBridgeCue invasionBridgeCue =
                FindComponentInScene<IntroGatePodInvasionBridgeCue>(scene);
            CinematicSequenceProfile profile = LoadAsset<CinematicSequenceProfile>(ProfilePath);
            Camera camera = runner.CinematicCamera;
            if (camera == null)
            {
                throw new InvalidOperationException("Missing intro GatePod review camera.");
            }

            float voiceThreeStartSeconds = ResolveVoiceThreeStartSeconds();
            float scanLeftStartSeconds = ResolveScanCameraStartSeconds(voiceThreeStartSeconds);
            float scanRightStartSeconds = ResolveScanRightCameraStartSeconds(scanLeftStartSeconds);
            float handLookStartSeconds = ResolveHandLookCameraStartSeconds(scanRightStartSeconds);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, 2.40f, OpeningCapturePath);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, 6.70f, RevealCapturePath);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, scanLeftStartSeconds + 0.95f, LeftScanCapturePath);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, scanRightStartSeconds + 0.95f, RightScanCapturePath);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, handLookStartSeconds + 0.85f, HandsCapturePath);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, voiceThreeStartSeconds + 1.05f, CommandoLegsCapturePath);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, ResolveInvasionExplosionStartSeconds(voiceThreeStartSeconds) + 0.45f, HeavenExplosionCapturePath);
            CaptureSample(runner, shotPlayer, cueDirector, rendererMask, invasionBridgeCue, profile, camera, ResolveInvasionPushShotStartSeconds(voiceThreeStartSeconds) + 0.95f, CommandoPushCapturePath);
            ApplyProfileSample(runner, profile, 0.1f);
            ApplyCinemachineSample(shotPlayer, cueDirector, camera, 0.1f);
            rendererMask?.ApplyForReview(0.1f);
            invasionBridgeCue?.Sample(0.1f);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CaptureSample(
            CinematicSequenceRunner runner,
            IntroGatePodCinemachineShotPlayer shotPlayer,
            IntroGatePodCutsceneCueDirector cueDirector,
            IntroGatePodFirstPersonRendererMask rendererMask,
            IntroGatePodInvasionBridgeCue invasionBridgeCue,
            CinematicSequenceProfile profile,
            Camera camera,
            float elapsedSeconds,
            string outputPath)
        {
            ApplyProfileSample(runner, profile, elapsedSeconds);
            ApplyCinemachineSample(shotPlayer, cueDirector, camera, elapsedSeconds);
            rendererMask?.ApplyForReview(elapsedSeconds);
            invasionBridgeCue?.Sample(elapsedSeconds);
            if (string.Equals(outputPath, HandsCapturePath, StringComparison.Ordinal))
            {
                WriteSampleDebugReport(runner, shotPlayer, cueDirector, camera, elapsedSeconds);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "C:/tmp");
            RenderTexture previous = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
                image.Apply();
                if (invasionBridgeCue != null)
                {
                    ApplyInvasionScreenEffectToCapture(
                        image,
                        invasionBridgeCue.CurrentImpactFlashAlpha,
                        invasionBridgeCue.CurrentWarningSweepAlpha);
                }

                ApplyFadeOverlayToCapture(image, cueDirector.CurrentFadeAlpha);
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        private static void WriteSampleDebugReport(
            CinematicSequenceRunner runner,
            IntroGatePodCinemachineShotPlayer shotPlayer,
            IntroGatePodCutsceneCueDirector cueDirector,
            Camera camera,
            float elapsedSeconds)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod Sample Debug");
            builder.AppendLine();
            builder.AppendLine($"- Time: {elapsedSeconds:0.000}");
            builder.AppendLine($"- FadeAlpha: {cueDirector.CurrentFadeAlpha:0.000}");
            builder.AppendLine($"- Camera position: {FormatVector(camera.transform.position)}");
            builder.AppendLine($"- Camera rotation euler: {FormatVector(camera.transform.eulerAngles)}");
            builder.AppendLine($"- Camera forward: {FormatVector(camera.transform.forward)}");
            CinemachineCamera activeCamera = shotPlayer.ActiveCamera;
            builder.AppendLine($"- Active CM: {(activeCamera != null ? activeCamera.name : "<none>")}");
            if (activeCamera != null)
            {
                builder.AppendLine($"- Active CM sample position: {FormatVector(ResolveCinemachineCameraSamplePosition(activeCamera))}");
                builder.AppendLine($"- Active CM lookAt: {(activeCamera.LookAt != null ? FormatVector(activeCamera.LookAt.position) : "<none>")}");
            }

            GameObject inori = FindRootOrDescendant(camera.gameObject.scene, InoriRootName);
            if (inori != null)
            {
                builder.AppendLine($"- Inori root: {FormatVector(inori.transform.position)}");
                Animator animator = inori.GetComponentInChildren<Animator>(includeInactive: true);
                AppendBone(builder, animator, HumanBodyBones.Head, "Head");
                AppendBone(builder, animator, HumanBodyBones.Chest, "Chest");
                AppendBone(builder, animator, HumanBodyBones.LeftHand, "LeftHand");
                AppendBone(builder, animator, HumanBodyBones.RightHand, "RightHand");
            }

            File.WriteAllText(SampleDebugPath, builder.ToString(), Encoding.UTF8);
        }

        private static void AppendBone(StringBuilder builder, Animator animator, HumanBodyBones bone, string label)
        {
            Transform transform = animator != null && animator.isHuman
                ? animator.GetBoneTransform(bone)
                : null;
            builder.AppendLine($"- {label}: {(transform != null ? FormatVector(transform.position) : "<none>")}");
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x:0.000}, {value.y:0.000}, {value.z:0.000})";
        }

        private static void ValidateReviewScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            List<string> issues = new List<string>();

            CinematicSequenceProfile profile = AssetDatabase.LoadAssetAtPath<CinematicSequenceProfile>(ProfilePath);
            if (profile == null)
            {
                issues.Add($"Missing profile at {ProfilePath}.");
            }
            else
            {
                List<string> profileIssues = new List<string>();
                profile.CollectValidationIssues(profileIssues);
                if (profile.CameraCues.Length == 0)
                {
                    profileIssues.RemoveAll(issue => issue.EndsWith(": no camera cues are authored.", StringComparison.Ordinal));
                }

                issues.AddRange(profileIssues);

                for (int i = 0; i < profile.CameraCues.Length; i++)
                {
                    if (!profile.CameraCues[i].DriveCameraPose)
                    {
                        issues.Add($"Camera cue {profile.CameraCues[i].CueId} is not a driven shot pose.");
                    }
                }
            }

            GameObject inori = FindRootOrDescendant(scene, InoriRootName);
            if (inori == null)
            {
                issues.Add($"Missing {InoriRootName}.");
            }
            else
            {
                Animator animator = inori.GetComponentInChildren<Animator>(includeInactive: true);
                RuntimeAnimatorController expectedController =
                    LoadAsset<RuntimeAnimatorController>(BuildResubmissionCinematicAnimationSetup.CinematicControllerPath);
                if (animator == null || animator.runtimeAnimatorController != expectedController)
                {
                    issues.Add("Inori does not use DB_Inori_CinematicP0.controller.");
                }

                ValidateInoriMaterials(inori, issues);
                if (FindDescendant(inori.transform, RifleName) == null)
                {
                    issues.Add("Missing attached InoriRifle under Inori.");
                }
            }

            GameObject gatePods = FindRootOrDescendant(scene, GatePodRootName);
            if (gatePods == null)
            {
                issues.Add($"Missing {GatePodRootName}.");
            }
            else if (gatePods.GetComponentsInChildren<Renderer>(includeInactive: true).Length == 0)
            {
                issues.Add($"{GatePodRootName} has no renderers.");
            }

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(PodAlbedoTexturePath) == null)
            {
                issues.Add($"Missing GatePod albedo texture at {PodAlbedoTexturePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(PodEmissionTexturePath) == null)
            {
                issues.Add($"Missing GatePod emission texture at {PodEmissionTexturePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<AudioClip>(VoiceZeroPath) == null)
            {
                issues.Add($"Missing intro voice cue at {VoiceZeroPath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<AudioClip>(VoiceOnePath) == null)
            {
                issues.Add($"Missing intro voice cue at {VoiceOnePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<AudioClip>(VoiceTwoPath) == null)
            {
                issues.Add($"Missing intro voice cue at {VoiceTwoPath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<AudioClip>(VoiceThreePath) == null)
            {
                issues.Add($"Missing intro voice cue at {VoiceThreePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<AudioClip>(BgmPath) == null)
            {
                issues.Add($"Missing intro BGM at {BgmPath}.");
            }

            CinematicSequenceRunner runner = FindComponentInScene<CinematicSequenceRunner>(scene);
            if (runner == null)
            {
                issues.Add("Missing CinematicSequenceRunner.");
            }
            else if (profile != null && runner.SequenceProfile != profile)
            {
                issues.Add("Runner is not bound to the intro GatePod profile.");
            }

            CinemachineBrain brain = FindComponentInScene<CinemachineBrain>(scene);
            if (brain == null)
            {
                issues.Add("Missing CinemachineBrain on the review camera.");
            }

            IntroGatePodCinemachineShotPlayer shotPlayer =
                FindComponentInScene<IntroGatePodCinemachineShotPlayer>(scene);
            if (shotPlayer == null)
            {
                issues.Add("Missing IntroGatePodCinemachineShotPlayer.");
            }
            else if (profile != null && shotPlayer.Shots.Length != profile.CameraCues.Length)
            {
                issues.Add("Cinemachine shot count does not match profile camera cue count.");
            }

            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                issues.Add($"Missing TimelineAsset at {TimelinePath}.");
            }

            PlayableDirector director = FindComponentInScene<PlayableDirector>(scene);
            if (director == null)
            {
                issues.Add("Missing PlayableDirector for the intro Timeline.");
            }
            else if (timeline != null && director.playableAsset != timeline)
            {
                issues.Add("PlayableDirector is not bound to the intro TimelineAsset.");
            }

            if (FindComponentInScene<CinemachineSequencerCamera>(scene) != null)
            {
                issues.Add("CinemachineSequencerCamera should not be present after Timeline migration.");
            }

            IntroGatePodFirstPersonRendererMask rendererMask =
                FindComponentInScene<IntroGatePodFirstPersonRendererMask>(scene);
            if (rendererMask == null)
            {
                issues.Add("Missing first-person renderer mask for head/hair occlusion control.");
            }
            else if (rendererMask.HiddenRendererCount < 1)
            {
                issues.Add("First-person renderer mask has no head or hair renderers assigned.");
            }

            IntroGatePodTimelineFadeOverlay fadeOverlay =
                FindComponentInScene<IntroGatePodTimelineFadeOverlay>(scene);
            if (fadeOverlay == null)
            {
                issues.Add("Missing Timeline fade overlay.");
            }
            else if (!fadeOverlay.HasCanvasGroup)
            {
                issues.Add("Timeline fade overlay must drive a CanvasGroup instead of legacy OnGUI fade.");
            }

            IntroGatePodInvasionBridgeCue invasionBridgeCue =
                FindComponentInScene<IntroGatePodInvasionBridgeCue>(scene);
            if (invasionBridgeCue == null)
            {
                issues.Add("Missing invasion bridge cue for voice 3 Commando run and background explosion.");
            }
            else
            {
                if (invasionBridgeCue.Commandos.Length < 3)
                {
                    issues.Add("Invasion bridge cue should bind three Commando runners.");
                }

                if (invasionBridgeCue.ExplosionRoot == null)
                {
                    issues.Add("Invasion bridge cue is missing the heaven-background explosion root.");
                }

                if (FindRootOrDescendant(scene, InvasionScreenEffectRootName) == null)
                {
                    issues.Add("Invasion bridge is missing ArkData-style screen effect overlay for warning and impact flash.");
                }
            }

            if (timeline != null)
            {
                CinemachineTrack cameraTrack = FindTimelineTrack<CinemachineTrack>(timeline, "Cinemachine Shots");
                if (cameraTrack == null)
                {
                    issues.Add("Timeline is missing the Cinemachine Shots track.");
                }
                else
                {
                    if (profile != null && CountTimelineClips(cameraTrack) != profile.CameraCues.Length)
                    {
                        issues.Add("Timeline Cinemachine clip count does not match profile camera cue count.");
                    }

                    if (director != null && director.GetGenericBinding(cameraTrack) == null)
                    {
                        issues.Add("Timeline Cinemachine track is not bound to the review CinemachineBrain.");
                    }
                }

                AnimationTrack openingDollyTrack =
                    FindTimelineTrack<AnimationTrack>(timeline, OpeningDollyTrackName);
                if (openingDollyTrack == null)
                {
                    issues.Add("Timeline is missing the Opening Dolly animation track.");
                }
                else
                {
                    if (CountTimelineClips(openingDollyTrack) != 1)
                    {
                        issues.Add("Timeline Opening Dolly track must contain one animation clip.");
                    }

                    if (director != null && director.GetGenericBinding(openingDollyTrack) == null)
                    {
                        issues.Add("Timeline Opening Dolly track is not bound to the opening camera Animator.");
                    }

                    ValidateOpeningDollyAnimationTrack(openingDollyTrack, issues);
                }

                AnimationTrack inoriPlacementTrack =
                    FindTimelineTrack<AnimationTrack>(timeline, InoriPlacementTrackName);
                if (inoriPlacementTrack != null)
                {
                    issues.Add("Timeline must not contain an Inori Placement track; adjust the placement parent directly in the scene.");
                }

                AnimationTrack inoriBodyTrack = FindTimelineTrack<AnimationTrack>(timeline, "Inori Body");
                if (inoriBodyTrack == null)
                {
                    issues.Add("Timeline is missing the Inori Body AnimationTrack.");
                }
                else
                {
                    if (CountTimelineClips(inoriBodyTrack) < 2)
                    {
                        issues.Add("Timeline Inori Body track must contain the wake and combat-ready authored body clips.");
                    }
                    else if (!HasTimelineClip(inoriBodyTrack, "wake_confused_hands")
                        || !HasTimelineClip(inoriBodyTrack, "combat_ready_handoff"))
                    {
                        issues.Add("Timeline Inori Body track is missing wake_confused_hands or combat_ready_handoff.");
                    }

                    if (director != null && director.GetGenericBinding(inoriBodyTrack) == null)
                    {
                        issues.Add("Timeline Inori Body track is not bound to Inori's Animator.");
                    }
                }

                AudioTrack voiceTrack = FindTimelineTrack<AudioTrack>(timeline, "Voice");
                if (voiceTrack == null || CountTimelineClips(voiceTrack) != 4)
                {
                    issues.Add("Timeline Voice track must contain four clips for voice 0-3.");
                }

                AudioTrack bgmTrack = FindTimelineTrack<AudioTrack>(timeline, "BGM");
                if (bgmTrack == null || CountTimelineClips(bgmTrack) != 1)
                {
                    issues.Add("Timeline BGM track must contain one clip starting at 0.");
                }

                IntroGatePodFadeTrack fadeTrack = FindTimelineTrack<IntroGatePodFadeTrack>(timeline, "Fade");
                if (fadeTrack == null || CountTimelineClips(fadeTrack) < 3)
                {
                    issues.Add("Timeline Fade track must contain opening fade, blackout, and eye-open clips.");
                }
                else if (director != null && director.GetGenericBinding(fadeTrack) != fadeOverlay)
                {
                    issues.Add("Timeline Fade track is not bound to the scene CanvasGroup fade overlay.");
                }

                ActivationTrack activationTrack = FindTimelineTrack<ActivationTrack>(timeline, "Inori Activation");
                if (activationTrack != null)
                {
                    issues.Add("Timeline must not contain an Inori Activation track that disables the character root.");
                }
            }

            IntroGatePodCutsceneCueDirector cueDirector =
                FindComponentInScene<IntroGatePodCutsceneCueDirector>(scene);
            if (cueDirector == null)
            {
                issues.Add("Missing IntroGatePodCutsceneCueDirector.");
            }
            else
            {
                if (cueDirector.DollyCues.Length != 1)
                {
                    issues.Add("Cue director must have one opening dolly cue.");
                }

                if (cueDirector.VoiceCues.Length != 0)
                {
                    issues.Add("Cue director should not own voice playback after Timeline migration.");
                }

                if (cueDirector.FadeCues.Length < 3)
                {
                    issues.Add("Cue director sampler must mirror opening fade, blackout, and eye-open fade cues.");
                }

            }

            CinemachineCamera[] cinemachineCameras = FindComponentsInScene<CinemachineCamera>(scene);
            if (profile != null && cinemachineCameras.Length < profile.CameraCues.Length)
            {
                issues.Add("Scene has fewer CinemachineCamera objects than authored camera cues.");
            }

            CinemachineSplineDolly openingSceneDolly = FindComponentInScene<CinemachineSplineDolly>(scene);
            if (openingSceneDolly == null)
            {
                issues.Add("Opening capsule camera is missing CinemachineSplineDolly.");
            }
            else if (openingSceneDolly.GetComponent<Animator>() == null)
            {
                issues.Add("Opening capsule dolly camera is missing the Animator required by Timeline.");
            }

            if (FindRootOrDescendant(scene, FirstPersonViewMarkerName) == null)
            {
                issues.Add($"Missing {FirstPersonViewMarkerName}.");
            }

            WriteValidationReport(issues);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException("Intro GatePod review scene validation failed:\n" + string.Join("\n", issues));
            }
        }

        private static T FindTimelineTrack<T>(TimelineAsset timeline, string trackName)
            where T : TrackAsset
        {
            if (timeline == null)
            {
                return null;
            }

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is T typedTrack
                    && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    return typedTrack;
                }
            }

            return null;
        }

        private static int CountTimelineClips(TrackAsset track)
        {
            if (track == null)
            {
                return 0;
            }

            int count = 0;
            foreach (TimelineClip clip in track.GetClips())
            {
                if (clip != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasTimelineClip(TrackAsset track, string displayName)
        {
            if (track == null)
            {
                return false;
            }

            foreach (TimelineClip clip in track.GetClips())
            {
                if (clip != null
                    && string.Equals(clip.displayName, displayName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateOpeningDollyAnimationTrack(
            AnimationTrack track,
            List<string> issues)
        {
            TimelineClip dollyTimelineClip = null;
            foreach (TimelineClip clip in track.GetClips())
            {
                if (clip == null)
                {
                    continue;
                }

                dollyTimelineClip = clip;
                break;
            }

            if (dollyTimelineClip == null)
            {
                return;
            }

            if (dollyTimelineClip.start > 0.001d
                || Mathf.Abs((float)dollyTimelineClip.duration - SourceC03CameraStartSeconds) > 0.01f)
            {
                issues.Add("Timeline Opening Dolly clip must run from 0.0 to the first-person cut.");
            }

            AnimationPlayableAsset animationAsset = dollyTimelineClip.asset as AnimationPlayableAsset;
            AnimationClip animationClip = animationAsset != null ? animationAsset.clip : null;
            if (animationClip == null)
            {
                issues.Add("Timeline Opening Dolly clip is missing its AnimationClip.");
                return;
            }

            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(CinemachineSplineDolly),
                OpeningDollyCurveProperty);
            AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);
            if (curve == null || curve.length < 2)
            {
                issues.Add("Timeline Opening Dolly clip is missing the CinemachineSplineDolly position curve.");
                return;
            }

            float startValue = curve.Evaluate(0f);
            float endValue = curve.Evaluate(SourceC03CameraStartSeconds);
            if (Mathf.Abs(startValue) > 0.001f || Mathf.Abs(endValue - 1f) > 0.001f)
            {
                issues.Add("Timeline Opening Dolly curve must move CinemachineSplineDolly from 0 to 1.");
            }
        }

        private static void WriteValidationReport(IReadOnlyList<string> issues)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Intro GatePod Cutscene Review");
            builder.AppendLine();
            builder.AppendLine($"- Scene: `{ReviewScenePath}`");
            builder.AppendLine($"- Profile: `{ProfilePath}`");
            builder.AppendLine($"- Timeline: `{TimelinePath}`");
            builder.AppendLine($"- Gate asset: `{GateModelPath}`");
            builder.AppendLine($"- GatePod textures: `{PodAlbedoTexturePath}`, `{PodEmissionTexturePath}`");
            builder.AppendLine($"- Voice folder: `{VoiceRoot}` (`0.mp3`, `1.mp3`, `2.mp3`, `3.mp3`).");
            builder.AppendLine($"- BGM: `{BgmPath}` starts at 0.0 on the Timeline `BGM` AudioTrack.");
            float voiceZeroStartSeconds = ResolveVoiceZeroStartSeconds();
            float voiceOneStartSeconds = ResolveVoiceOneStartSeconds();
            float voiceTwoStartSeconds = ResolveVoiceTwoStartSeconds();
            float voiceThreeStartSeconds = ResolveVoiceThreeStartSeconds();
            float voiceZeroLengthSeconds = ResolveAudioClipLengthSeconds(VoiceZeroPath, 1.056f);
            float voiceOneLengthSeconds = ResolveAudioClipLengthSeconds(VoiceOnePath, 0.888f);
            float voiceTwoLengthSeconds = ResolveAudioClipLengthSeconds(VoiceTwoPath, 2.441f);
            float voiceThreeLengthSeconds = ResolveAudioClipLengthSeconds(VoiceThreePath, 3.267f);
            float scanLeftStartSeconds = ResolveScanCameraStartSeconds(voiceThreeStartSeconds);
            float scanRightStartSeconds = ResolveScanRightCameraStartSeconds(scanLeftStartSeconds);
            float handLookStartSeconds = ResolveHandLookCameraStartSeconds(scanRightStartSeconds);
            float invasionExplosionStartSeconds = ResolveInvasionExplosionStartSeconds(voiceThreeStartSeconds);
            float invasionPushStartSeconds = ResolveInvasionPushShotStartSeconds(voiceThreeStartSeconds);
            builder.AppendLine("- Source reference: The Phantom Knowledge `GeneralTimeline_nD 2.playable` first section: Fader 0.0-2.0, C01 camera 0.0-3.0667, original voice 01 at 5.3667, C03 at 6.1, C04 at 8.1333.");
            builder.AppendLine($"- Current pass timing: opening fade 0.0-2.0, pre-POV blackout {SourceC03CameraStartSeconds - FirstPersonBlackoutLeadSeconds:0.000}-{SourceC03CameraStartSeconds:0.000}, capsule-inside POV at 6.1, voice 0 at {voiceZeroStartSeconds:0.000} for {voiceZeroLengthSeconds:0.000}s, voice 1 at {voiceOneStartSeconds:0.000} for {voiceOneLengthSeconds:0.000}s, voice 2 at {voiceTwoStartSeconds:0.000} for {voiceTwoLengthSeconds:0.000}s, left scan at {scanLeftStartSeconds:0.000}, right scan at {scanRightStartSeconds:0.000}, hand look-down at {handLookStartSeconds:0.000}, voice 3 at {voiceThreeStartSeconds:0.000} for {voiceThreeLengthSeconds:0.000}s.");
            builder.AppendLine($"- Voice 3 invasion bridge: Commando lower-body run starts at {voiceThreeStartSeconds:0.000}, heaven-background explosion starts at {invasionExplosionStartSeconds:0.000}, final Commando push shot starts at {invasionPushStartSeconds:0.000}.");
            builder.AppendLine($"- Invasion actors: three promoted `{SciFiCommandoPrefabPath}` instances are used as presentation-only Commando runners; gameplay MonoBehaviours are disabled for the cutscene scene.");
            builder.AppendLine("- Camera package: Cinemachine 3.x `CinemachineBrain`, Timeline `CinemachineTrack`, editable child `CinemachineCamera` shot objects, `CinemachineHardLookAt`, and `CinemachineSplineDolly` for the opening capsule move.");
            builder.AppendLine("- Capsule POV: first-person cameras use the user-authored view marker `IntroGatePodReview_FirstPersonViewMarker`; the sequence now goes eye-open -> left scan -> right scan -> look down at hands while Inori remains active for animation review.");
            builder.AppendLine("- Timeline tracks: `Cinemachine Shots`, `Opening Dolly`, `Inori Body`, `Voice`, `BGM`, and CanvasGroup-driven `Fade`; `IntroGatePodReview_InoriPlacement` is adjusted directly as a scene object.");
            builder.AppendLine("- Inori material rule: all Inori renderer slots must resolve to promoted `DB_Inori_*` materials.");
            builder.AppendLine($"- Captures: `{OpeningCapturePath}`, `{RevealCapturePath}`, `{LeftScanCapturePath}`, `{RightScanCapturePath}`, `{HandsCapturePath}`, `{CommandoLegsCapturePath}`, `{HeavenExplosionCapturePath}`, `{CommandoPushCapturePath}`.");
            builder.AppendLine();
            if (issues.Count == 0)
            {
                builder.AppendLine("Validation: PASS");
            }
            else
            {
                builder.AppendLine("Validation: FAIL");
                for (int i = 0; i < issues.Count; i++)
                {
                    builder.AppendLine($"- {issues[i]}");
                }
            }

            File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        }

        private static void ValidateInoriMaterials(GameObject inori, List<string> issues)
        {
            Renderer[] renderers = inori.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                if (IsReviewWeaponRenderer(renderers[i].transform))
                {
                    continue;
                }

                for (int j = 0; j < materials.Length; j++)
                {
                    Material material = materials[j];
                    if (material == null)
                    {
                        issues.Add($"{renderers[i].name} material slot {j} is empty.");
                        continue;
                    }

                    if (!material.name.StartsWith("DB_Inori_", StringComparison.Ordinal))
                    {
                        issues.Add($"{renderers[i].name} material slot {j} uses non-promoted Inori material {material.name}.");
                    }
                }
            }
        }

        private static bool IsReviewWeaponRenderer(Transform transform)
        {
            return HasAncestorOrSelf(transform, RifleName)
                || HasAncestorOrSelf(transform, FloorRifleName)
                || HasAncestorOrSelf(transform, FloorSwordName);
        }

        private static void ApplyProfileSample(CinematicSequenceRunner runner, CinematicSequenceProfile profile, float elapsedSeconds)
        {
            if (runner == null || profile == null)
            {
                return;
            }

            runner.TryApplyProfileSampleForReview(profile, elapsedSeconds, Vector3.forward);
        }

        private static void ApplyCinemachineSample(
            IntroGatePodCinemachineShotPlayer shotPlayer,
            IntroGatePodCutsceneCueDirector cueDirector,
            Camera camera,
            float elapsedSeconds)
        {
            if (shotPlayer == null || camera == null)
            {
                return;
            }

            cueDirector?.ApplySampleForReview(elapsedSeconds);
            shotPlayer.ApplySampleForReview(elapsedSeconds);
            CinemachineCamera activeCamera = shotPlayer.ActiveCamera;
            if (activeCamera == null)
            {
                return;
            }

            Vector3 position = ResolveCinemachineCameraSamplePosition(activeCamera);
            Transform lookAt = activeCamera.LookAt;
            Quaternion rotation = lookAt != null
                ? ResolveLookRotation(position, lookAt.position)
                : activeCamera.transform.rotation;
            camera.transform.SetPositionAndRotation(position, rotation);
            camera.fieldOfView = activeCamera.Lens.FieldOfView;
            camera.nearClipPlane = activeCamera.Lens.NearClipPlane;
            camera.farClipPlane = activeCamera.Lens.FarClipPlane;
        }

        private static Vector3 ResolveCinemachineCameraSamplePosition(CinemachineCamera activeCamera)
        {
            CinemachineSplineDolly dolly = activeCamera.GetComponent<CinemachineSplineDolly>();
            if (dolly == null || dolly.Spline == null || dolly.PositionUnits != PathIndexUnit.Normalized)
            {
                return activeCamera.transform.position;
            }

            float normalizedPosition = Mathf.Clamp01(dolly.CameraPosition);
            float3 worldPosition = dolly.Spline.EvaluatePosition(normalizedPosition);
            return new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
        }

        private static void ApplyFadeOverlayToCapture(Texture2D image, float fadeAlpha)
        {
            if (image == null || fadeAlpha <= 0.001f)
            {
                return;
            }

            float multiplier = 1f - Mathf.Clamp01(fadeAlpha);
            Color32[] pixels = image.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                pixel.r = (byte)Mathf.RoundToInt(pixel.r * multiplier);
                pixel.g = (byte)Mathf.RoundToInt(pixel.g * multiplier);
                pixel.b = (byte)Mathf.RoundToInt(pixel.b * multiplier);
                pixels[i] = pixel;
            }

            image.SetPixels32(pixels);
            image.Apply();
        }

        private static void ApplyInvasionScreenEffectToCapture(
            Texture2D image,
            float impactFlashAlpha,
            float warningSweepAlpha)
        {
            if (image == null)
            {
                return;
            }

            impactFlashAlpha = Mathf.Clamp01(impactFlashAlpha);
            warningSweepAlpha = Mathf.Clamp01(warningSweepAlpha);
            if (impactFlashAlpha <= 0.001f && warningSweepAlpha <= 0.001f)
            {
                return;
            }

            Color32[] pixels = image.GetPixels32();
            int width = image.width;
            int height = image.height;
            Color impactWarm = new Color(1f, 0.48f, 0.16f, 1f);
            Color impactCore = new Color(1f, 0.92f, 0.72f, 1f);
            Color warningRed = new Color(1f, 0.04f, 0.08f, 1f);
            float diagonalSlope = Mathf.Tan(-13f * Mathf.Deg2Rad);

            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % width;
                int y = i / width;
                Color color = pixels[i];

                if (impactFlashAlpha > 0.001f)
                {
                    float u = ((float)x / Mathf.Max(1, width - 1)) - 0.47f;
                    float v = ((float)y / Mathf.Max(1, height - 1)) - 0.52f;
                    float core = Mathf.Clamp01(1f - ((u * u * 1.42f) + (v * v * 2.1f)) * 4.2f);
                    color = Color.Lerp(color, impactWarm, impactFlashAlpha * 0.48f);
                    color = Color.Lerp(color, impactCore, impactFlashAlpha * core * 0.46f);
                }

                if (warningSweepAlpha > 0.001f)
                {
                    float centerY = (height * 0.60f) + ((x - (width * 0.5f)) * diagonalSlope);
                    float distance = Mathf.Abs(y - centerY);
                    float band = Mathf.Clamp01(1f - (distance / 82f));
                    float core = Mathf.Clamp01(1f - (distance / 18f));
                    color = Color.Lerp(color, warningRed, warningSweepAlpha * band * 0.50f);
                    color = Color.Lerp(color, Color.white, warningSweepAlpha * core * 0.22f);
                }

                pixels[i] = color;
            }

            image.SetPixels32(pixels);
            image.Apply();
        }

        private static void AssignGatePodMaterials(GameObject root)
        {
            Texture2D podAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>(PodAlbedoTexturePath);
            Texture2D podEmission = AssetDatabase.LoadAssetAtPath<Texture2D>(PodEmissionTexturePath);

            Material shell = LoadOrCreateMaterial(
                PodShellMaterialPath,
                Color.white,
                new Color(0.0f, 1.15f, 1.35f, 1f),
                0.52f,
                0.08f);
            ApplyMaterialTextureMaps(shell, podAlbedo, podEmission);

            Material glow = LoadOrCreateMaterial(
                PodGlowMaterialPath,
                new Color(0.10f, 0.82f, 0.90f, 1f),
                new Color(0.0f, 2.10f, 2.45f, 1f),
                0.48f,
                0.0f);
            ApplyMaterialTextureMaps(glow, podEmission != null ? podEmission : podAlbedo, podEmission);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material selected = renderers[i].name.IndexOf("MeshPart1", StringComparison.OrdinalIgnoreCase) >= 0
                    || renderers[i].name.IndexOf("light", StringComparison.OrdinalIgnoreCase) >= 0
                        ? glow
                        : shell;
                Material[] slots = renderers[i].sharedMaterials;
                for (int j = 0; j < slots.Length; j++)
                {
                    slots[j] = selected;
                }

                renderers[i].sharedMaterials = slots;
                EditorUtility.SetDirty(renderers[i]);
            }
        }

        private static void ApplyMaterialTextureMaps(Material material, Texture2D baseMap, Texture2D emissionMap)
        {
            if (material == null)
            {
                return;
            }

            if (baseMap != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", baseMap);
                }

                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", baseMap);
                }
            }

            if (emissionMap != null)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionMap"))
                {
                    material.SetTexture("_EmissionMap", emissionMap);
                }
            }

            EditorUtility.SetDirty(material);
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
                    Shape("jawOpen", 48f)),
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
                    Shape("mouthFrownLeft", 56f))
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

        private static GameObject InstantiateModelPrefab(string modelPath, Scene scene, string objectName)
        {
            GameObject model = LoadAsset<GameObject>(modelPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(model, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to instantiate {modelPath}.");
            }

            PrefabUtility.UnpackPrefabInstance(
                instance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            instance.name = objectName;
            return instance;
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

        private static Material LoadOrCreateMaterial(
            string path,
            Color color,
            Color emissionColor,
            float smoothness,
            float metallic)
        {
            EnsureFolder(PathParent(path));
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
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
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", Mathf.Clamp01(metallic));
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTransparentTextureMaterial(
            string path,
            string texturePath,
            Color color,
            Vector2 textureScale,
            Vector2 textureOffset)
        {
            EnsureFolder(PathParent(path));
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Transparent")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            material.color = color;
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                    material.SetTextureScale("_BaseMap", textureScale);
                    material.SetTextureOffset("_BaseMap", textureOffset);
                }

                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", texture);
                    material.SetTextureScale("_MainTex", textureScale);
                    material.SetTextureOffset("_MainTex", textureOffset);
                }
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            }

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void FitRootToBounds(Transform root, float targetHeight, Vector3 desiredBaseCenter)
        {
            Bounds bounds = CalculateBounds(root);
            if (bounds.size.y <= 0.0001f)
            {
                return;
            }

            float scale = targetHeight / bounds.size.y;
            root.localScale *= scale;
            bounds = CalculateBounds(root);
            Vector3 currentBaseCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            root.position += desiredBaseCenter - currentBaseCenter;
        }

        private static Bounds CalculateBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static Quaternion ResolveLookRotation(Vector3 position, Vector3 lookAt)
        {
            Vector3 forward = lookAt - position;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static float3 ToFloat3(Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }

        private static Vector3 ToVector3(float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }

        private static CinemachineBlendDefinition.Styles ResolveCinemachineBlendStyle(
            CinematicSequenceProfile.CameraBlendKind blendKind)
        {
            switch (blendKind)
            {
                case CinematicSequenceProfile.CameraBlendKind.Cut:
                    return CinemachineBlendDefinition.Styles.Cut;
                case CinematicSequenceProfile.CameraBlendKind.PushIn:
                case CinematicSequenceProfile.CameraBlendKind.PullBack:
                    return CinemachineBlendDefinition.Styles.EaseOut;
                case CinematicSequenceProfile.CameraBlendKind.Reframe:
                case CinematicSequenceProfile.CameraBlendKind.GameplayMatch:
                case CinematicSequenceProfile.CameraBlendKind.Ease:
                default:
                    return CinemachineBlendDefinition.Styles.EaseInOut;
            }
        }

        private static float ResolveCinemachineBlendSeconds(CinematicSequenceProfile.CameraBlendKind blendKind)
        {
            switch (blendKind)
            {
                case CinematicSequenceProfile.CameraBlendKind.Cut:
                    return 0f;
                case CinematicSequenceProfile.CameraBlendKind.PushIn:
                case CinematicSequenceProfile.CameraBlendKind.PullBack:
                    return 0.65f;
                case CinematicSequenceProfile.CameraBlendKind.Reframe:
                    return 1.0f;
                case CinematicSequenceProfile.CameraBlendKind.GameplayMatch:
                    return 0.85f;
                case CinematicSequenceProfile.CameraBlendKind.Ease:
                default:
                    return 0.72f;
            }
        }

        private static string SanitizeObjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Shot";
            }

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                builder.Append(char.IsLetterOrDigit(character) ? character : '_');
            }

            return builder.ToString();
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

        private static Transform FindDescendant(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDescendantContains(Transform root, string objectNamePart)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectNamePart))
            {
                return null;
            }

            if (root.name.IndexOf(objectNamePart, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendantContains(root.GetChild(i), objectNamePart);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject FindRootOrDescendant(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, objectName, StringComparison.OrdinalIgnoreCase))
                {
                    return roots[i];
                }

                Transform found = FindDescendant(roots[i].transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static bool HasAncestorOrSelf(Transform transform, string objectName)
        {
            Transform current = transform;
            while (current != null)
            {
                if (string.Equals(current.name, objectName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
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

        private static T[] FindComponentsInScene<T>(Scene scene) where T : Component
        {
            List<T> components = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                components.AddRange(roots[i].GetComponentsInChildren<T>(includeInactive: true));
            }

            return components.ToArray();
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing asset at {path}.");
            }

            return asset;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = PathParent(folderPath);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }

        private static void DeleteFileIfExists(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string PathParent(string path)
        {
            string normalized = path.Replace('\\', '/');
            int separator = normalized.LastIndexOf('/');
            return separator > 0 ? normalized.Substring(0, separator) : string.Empty;
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
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetRelativeObjectReference(SerializedProperty property, string propertyName, UnityEngine.Object value)
        {
            property.FindPropertyRelative(propertyName).objectReferenceValue = value;
        }

        private static void SetRelativeEnum(SerializedProperty property, string propertyName, int value)
        {
            property.FindPropertyRelative(propertyName).enumValueIndex = value;
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

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.GetType().Name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static bool SetIfDifferent<T>(Func<T> getValue, Action<T> setValue, T targetValue)
        {
            if (EqualityComparer<T>.Default.Equals(getValue(), targetValue))
            {
                return false;
            }

            setValue(targetValue);
            return true;
        }
    }
}
