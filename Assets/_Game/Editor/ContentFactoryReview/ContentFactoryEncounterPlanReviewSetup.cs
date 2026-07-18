using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.ContentFactoryReview;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.ContentFactoryReview
{
    /// <summary>
    /// Deterministically authors the isolated CF-01 content-factory inspection board.
    /// The board reviews a pure encounter plan and never owns runtime spawning, combat
    /// outcome, StageRun facts, rewards, persistence, routing, or server state.
    /// </summary>
    public static class ContentFactoryEncounterPlanReviewSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Review/UI_ContentFactoryEncounterPlanReview.unity";
        public const string ProfilePath =
            "Assets/_Game/DesignData/UI/Review/DB_ContentFactoryEncounterPlan_CF01.asset";

        public const string PlanId = "cf01.review.encounter-plan";
        public const string StageId = "cf01.review.stage";
        public const string EncounterId = "cf01.review.encounter.required";

        private const string MediumFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_Medium_Dynamic.asset";
        private const string SemiBoldFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_SemiBold_Dynamic.asset";
        private const string BackgroundArtPath =
            "Assets/_Game/UI/ChapterHubReview/Art/BG_OlympusChapterHub_Review.png";
        private const string ResponsiveCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIResponsiveLayouts.asset";
        private const string ResponsiveCatalogGuid =
            "964233ec7542aff4381a9e70ee1edfbd";
        private const string ControllerScriptPath =
            "Assets/_Game/UI/ContentFactoryReview/ContentFactoryEncounterPlanReviewController.cs";
        private const string EditorBuildSettingsAssetPath =
            "ProjectSettings/EditorBuildSettings.asset";

        private static readonly string[] ImmutableProtectedAssets =
        {
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusStationCombat.asset",
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/Scripts/Combat/CombatEncounterController.cs",
            "Assets/_Game/Scripts/LevelDesign/StageCountOneEncounterExecutor.cs",
            BackgroundArtPath,
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-Medium.otf",
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf",
            ResponsiveCatalogPath
        };

        private static readonly string[] RequiredDynamicFontAssets =
        {
            MediumFontPath,
            SemiBoldFontPath
        };

        private static readonly Dictionary<string, int> ExpectedMediumTextRoles =
            new(StringComparer.Ordinal)
            {
                ["IdentityText"] = 1,
                ["ObjectiveText"] = 1,
                ["ProgressText"] = 1,
                ["CurrentSpawnText"] = 1,
                ["OwnershipBoundaryText"] = 1,
                ["TimelineSubtitle"] = 1,
                ["WaveDetailText"] = 3,
                ["FooterBoundary"] = 1
            };

        private static readonly Dictionary<string, int> ExpectedSemiBoldTextRoles =
            new(StringComparer.Ordinal)
            {
                ["ProductBreadcrumb"] = 1,
                ["AdmissionBoundaryText"] = 1,
                ["SectionKicker"] = 1,
                ["TitleText"] = 1,
                ["ObjectiveLabel"] = 1,
                ["StateText"] = 1,
                ["TimelineTitle"] = 1,
                ["OrderBadge"] = 3,
                ["WaveTitleText"] = 3,
                ["WaveStateText"] = 3,
                ["Label"] = 5
            };

        private static readonly Type[] ForbiddenComponentTypes =
        {
            typeof(DimensionBrawl.LevelDesign.StageCountOneEncounterExecutor),
            typeof(DimensionBrawl.LevelDesign.OlympusCorridorCombatFlowController),
            typeof(DimensionBrawl.LevelDesign.OlympusStationCombatResultPresenter),
            typeof(DimensionBrawl.LevelDesign.OlympusStageClearOverlay),
            typeof(DimensionBrawl.LevelDesign.OlympusStationRunFactCollector),
            typeof(DimensionBrawl.UI.StageClear.StageClearScreenPresenter),
            typeof(DimensionBrawl.UI.UISceneFlowRouter),
            typeof(DimensionBrawl.UI.UISceneRouteLoader)
        };

        private static readonly Color Ink = new(0.008f, 0.018f, 0.036f, 0.99f);
        private static readonly Color InkSoft = new(0.018f, 0.044f, 0.076f, 0.97f);
        private static readonly Color Panel = new(0.025f, 0.068f, 0.108f, 0.96f);
        private static readonly Color PanelSoft = new(0.044f, 0.105f, 0.155f, 0.96f);
        private static readonly Color Cyan = new(0.22f, 0.91f, 1.00f, 1f);
        private static readonly Color Blue = new(0.20f, 0.48f, 0.92f, 1f);
        private static readonly Color Amber = new(1.00f, 0.67f, 0.23f, 1f);
        private static readonly Color Green = new(0.34f, 0.96f, 0.68f, 1f);
        private static readonly Color White = new(0.94f, 0.98f, 1.00f, 1f);
        private static readonly Color Muted = new(0.58f, 0.70f, 0.80f, 1f);

        [MenuItem("Tools/DimensionBrawl/Review/Setup Content Factory Encounter Plan Review")]
        public static void SetupMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Setup();
            }
        }

        [MenuItem("Tools/DimensionBrawl/Review/Validate Content Factory Encounter Plan Review")]
        public static void ValidateMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                RunBatchVerification();
            }
        }

        public static void RunBatchSetup()
        {
            Setup();
        }

        public static void RunBatchVerification()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> issues = ValidateGeneratedReview(scene);
            ThrowIfIssues("CF-01 encounter-plan review validation failed", issues);
            Debug.Log(
                "CF-01 encounter-plan review validation passed. "
                + "Exact fixture, serialized bindings, TMP font roles, missing-script, protected-input, "
                + "Build Settings exclusion, and enumerated forbidden-component checks passed.");
        }

        public static string ComputeProtectedAssetDigest()
        {
            var builder = new StringBuilder(4096);
            foreach (string assetPath in ImmutableProtectedAssets)
            {
                AppendFileDigest(builder, assetPath);
                AppendFileDigest(builder, assetPath + ".meta");
            }

            using SHA256 sha = SHA256.Create();
            return ToLowerHex(sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString())));
        }

        private static void Setup()
        {
            string protectedDigestBefore = ComputeProtectedAssetDigest();
            EnsureAssetFolder(PathParent(ScenePath));
            EnsureAssetFolder(PathParent(ProfilePath));

            var preflightIssues = new List<string>();
            ValidateProtectedInputPresence(preflightIssues);
            if (!File.Exists(AssetPathToAbsolutePath(EditorBuildSettingsAssetPath)))
            {
                preflightIssues.Add(
                    $"Unity Build Settings asset `{EditorBuildSettingsAssetPath}` is missing.");
            }

            ThrowIfIssues("CF-01 encounter-plan review setup preflight failed", preflightIssues);
            TMP_FontAsset mediumFont = LoadRequired<TMP_FontAsset>(MediumFontPath);
            TMP_FontAsset semiBoldFont = LoadRequired<TMP_FontAsset>(SemiBoldFontPath);
            Sprite background = LoadRequired<Sprite>(BackgroundArtPath);
            _ = LoadRequired<UIResponsiveLayoutCatalog>(ResponsiveCatalogPath);

            GuardProfileOutputPath();
            GuardSceneOutputPath();

            OutputFileSnapshot outputSnapshot = OutputFileSnapshot.Capture(
                ProfilePath,
                ScenePath);
            try
            {
                SetupCore(
                    protectedDigestBefore,
                    mediumFont,
                    semiBoldFont,
                    background);
            }
            catch (Exception setupException)
            {
                try
                {
                    outputSnapshot.Restore();
                }
                catch (Exception restoreException)
                {
                    throw new AggregateException(
                        "CF-01 setup failed and its exact output rollback also failed.",
                        setupException,
                        restoreException);
                }

                throw;
            }
        }

        private static void SetupCore(
            string protectedDigestBefore,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            Sprite background)
        {
            StageEncounterPlanProfile profile = EnsureProfile();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject root = new GameObject("ContentFactoryEncounterPlanReview_Root");
            SceneManager.MoveGameObjectToScene(root, scene);

            Camera camera = CreateReviewCamera(root.transform);
            Canvas canvas = CreateCanvas(root.transform, camera);
            ReviewUiRefs ui = BuildReviewUi(
                canvas.GetComponent<RectTransform>(),
                mediumFont,
                semiBoldFont,
                background);
            ConfigureResponsiveRoot(
                canvas.GetComponent<UIResponsiveRoot>(),
                canvas.GetComponent<CanvasScaler>(),
                ui.SafeAreaRoot);
            EnsureEventSystem(root.transform);

            GameObject controllerObject = new GameObject(
                "ContentFactoryEncounterPlanReviewFlow",
                typeof(ContentFactoryEncounterPlanReviewController));
            controllerObject.transform.SetParent(root.transform, false);
            ContentFactoryEncounterPlanReviewController controller =
                controllerObject.GetComponent<ContentFactoryEncounterPlanReviewController>();
            controller.ConfigureCore(profile);
            controller.ConfigureTextView(
                ui.AdmissionBoundaryText,
                ui.TitleText,
                ui.IdentityText,
                ui.ObjectiveText,
                ui.StateText,
                ui.ProgressText,
                ui.CurrentSpawnText,
                ui.OwnershipBoundaryText);
            controller.ConfigureWaveCards(
                ui.WaveTitleTexts,
                ui.WaveStateTexts,
                ui.WaveDetailTexts,
                ui.WaveAccentImages);
            controller.ConfigureActions(
                ui.BeginButton,
                ui.ResolveButton,
                ui.AdvanceButton,
                ui.InterruptButton,
                ui.ResetButton);

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException(
                    $"Failed to save CF-01 review scene `{ScenePath}`.");
            }

            AssetDatabase.SaveAssetIfDirty(profile);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> issues = ValidateGeneratedReview(scene);
            string protectedDigestAfter = ComputeProtectedAssetDigest();
            if (!string.Equals(
                    protectedDigestBefore,
                    protectedDigestAfter,
                    StringComparison.Ordinal))
            {
                issues.Add(
                    "A protected canonical product/font/responsive asset changed during CF-01 setup.");
            }

            ThrowIfIssues("CF-01 encounter-plan review setup failed", issues);
            Debug.Log(
                $"Created `{ScenePath}` with 3 ordered review waves, "
                + "7 simulated combatants, deterministic digest `"
                + profile.CanonicalDigest
                + "`; exact scene/font bindings and enumerated forbidden-component checks passed.");
        }

        private static StageEncounterPlanProfile EnsureProfile()
        {
            StageEncounterPlanProfile profile =
                AssetDatabase.LoadAssetAtPath<StageEncounterPlanProfile>(ProfilePath);
            if (profile == null)
            {
                UnityEngine.Object occupied = AssetDatabase.LoadMainAssetAtPath(ProfilePath);
                if (occupied != null || File.Exists(AssetPathToAbsolutePath(ProfilePath)))
                {
                    throw new InvalidOperationException(
                        $"CF-01 profile path `{ProfilePath}` is occupied by another asset.");
                }

                profile = ScriptableObject.CreateInstance<StageEncounterPlanProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.name = "DB_ContentFactoryEncounterPlan_CF01";
            profile.Configure(
                1,
                1,
                PlanId,
                StageId,
                StageEncounterPlanAdmissionDisposition.ReviewOnlyNotAdmitted,
                StageEncounterPlanOutcomeOwner.ExistingStageRun,
                StageEncounterPlanRewardOwner.ExternalRewardLedger,
                CreateExpectedEncounterDefinition());

            EditorUtility.SetDirty(profile);
            if (!profile.TryValidate(out string error))
            {
                throw new InvalidOperationException(
                    "Generated CF-01 profile is invalid: " + error);
            }

            return profile;
        }

        private static StageEncounterPlanProfile.EncounterDefinition
            CreateExpectedEncounterDefinition()
        {
            return new StageEncounterPlanProfile.EncounterDefinition(
                EncounterId,
                new[]
                {
                    Wave(
                        "cf01.review.wave.01-entry",
                        0,
                        StageEncounterWaveActivation.EncounterStart,
                        Spawn(
                            "cf01.review.spawn.01-entry-left",
                            "dimensionbrawl.enemy.melee-probe",
                            "cf01.review.anchor.entry-left",
                            2,
                            0f)),
                    Wave(
                        "cf01.review.wave.02-crossfire",
                        1,
                        StageEncounterWaveActivation.PreviousWaveDefeated,
                        Spawn(
                            "cf01.review.spawn.02-crossfire-left",
                            "dimensionbrawl.enemy.ranged-probe",
                            "cf01.review.anchor.crossfire-left",
                            1,
                            0.25f),
                        Spawn(
                            "cf01.review.spawn.02-crossfire-right",
                            "dimensionbrawl.enemy.melee-probe",
                            "cf01.review.anchor.crossfire-right",
                            2,
                            0.75f)),
                    Wave(
                        "cf01.review.wave.03-final",
                        2,
                        StageEncounterWaveActivation.PreviousWaveDefeated,
                        Spawn(
                            "cf01.review.spawn.03-final-center",
                            "dimensionbrawl.enemy.guard-probe",
                            "cf01.review.anchor.final-center",
                            1,
                            0f),
                        Spawn(
                            "cf01.review.spawn.03-final-rear",
                            "dimensionbrawl.enemy.ranged-probe",
                            "cf01.review.anchor.final-rear",
                            1,
                            0.50f))
                });
        }

        private static string ComputeExpectedProfileDigest()
        {
            StageEncounterPlanProfile expected =
                ScriptableObject.CreateInstance<StageEncounterPlanProfile>();
            try
            {
                expected.Configure(
                    1,
                    1,
                    PlanId,
                    StageId,
                    StageEncounterPlanAdmissionDisposition.ReviewOnlyNotAdmitted,
                    StageEncounterPlanOutcomeOwner.ExistingStageRun,
                    StageEncounterPlanRewardOwner.ExternalRewardLedger,
                    CreateExpectedEncounterDefinition());
                return expected.CanonicalDigest;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(expected);
            }
        }

        private static StageEncounterPlanProfile.WaveDefinition Wave(
            string waveId,
            int waveIndex,
            StageEncounterWaveActivation activation,
            params StageEncounterPlanProfile.SpawnDefinition[] spawns)
        {
            return new StageEncounterPlanProfile.WaveDefinition(
                waveId,
                waveIndex,
                activation,
                StageEncounterObjective.DefeatAll,
                spawns);
        }

        private static StageEncounterPlanProfile.SpawnDefinition Spawn(
            string spawnId,
            string payloadId,
            string anchorId,
            int count,
            float delaySeconds)
        {
            return new StageEncounterPlanProfile.SpawnDefinition(
                spawnId,
                payloadId,
                anchorId,
                count,
                delaySeconds);
        }

        private static Camera CreateReviewCamera(Transform parent)
        {
            GameObject gameObject = new(
                "ReviewCamera",
                typeof(Camera),
                typeof(AudioListener));
            gameObject.tag = "MainCamera";
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = new Vector3(0f, 0f, -10f);
            Camera camera = gameObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Ink;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            return camera;
        }

        private static Canvas CreateCanvas(Transform parent, Camera camera)
        {
            GameObject gameObject = new(
                "ContentFactoryEncounterPlanReviewCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(UIResponsiveRoot));
            gameObject.transform.SetParent(parent, false);
            Canvas canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;

            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static ReviewUiRefs BuildReviewUi(
            RectTransform canvasRect,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            Sprite background)
        {
            var refs = new ReviewUiRefs();
            Image backgroundImage = CreateImage(
                canvasRect,
                "ReviewBackground",
                Color.white,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            backgroundImage.sprite = background;
            backgroundImage.preserveAspect = false;
            AspectRatioFitter fitter =
                backgroundImage.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = Mathf.Max(
                0.1f,
                background.rect.width / background.rect.height);
            CreateImage(
                canvasRect,
                "BackgroundWash",
                new Color(0.004f, 0.016f, 0.034f, 0.76f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            GameObject safeObject = new(
                "SafeArea",
                typeof(RectTransform),
                typeof(UISafeAreaRoot));
            RectTransform safeRect = safeObject.GetComponent<RectTransform>();
            safeRect.SetParent(canvasRect, false);
            Stretch(safeRect);
            refs.SafeAreaRoot = safeObject.GetComponent<UISafeAreaRoot>();
            ConfigureSafeArea(refs.SafeAreaRoot, safeRect, 24f);

            CreateImage(
                safeRect,
                "TopRail",
                new Color(0.008f, 0.022f, 0.044f, 0.97f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -45f),
                new Vector2(0f, 90f));
            CreateText(
                safeRect,
                "ProductBreadcrumb",
                "DIMENSION BRAWL  /  CONTENT FACTORY  /  ENCOUNTER PLAN",
                semiBoldFont,
                22f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0.55f, 1f),
                new Vector2(8f, -45f),
                new Vector2(-48f, 44f));
            refs.AdmissionBoundaryText = CreateText(
                safeRect,
                "AdmissionBoundaryText",
                "CF-01  /  REVIEW ONLY  /  RUNTIME NOT ADMITTED",
                semiBoldFont,
                17f,
                Amber,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.55f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-8f, -45f),
                new Vector2(-48f, 42f));

            RectTransform left = CreatePanel(
                safeRect,
                "PlanIdentityPanel",
                Panel,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(36f, -51f),
                new Vector2(510f, -198f));
            CreateImage(
                left,
                "IdentityAccent",
                Cyan,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -3f),
                new Vector2(0f, 6f));
            CreateText(
                left,
                "SectionKicker",
                "CF-01  /  AUTHORING CONTRACT",
                semiBoldFont,
                18f,
                Cyan,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-13f, -52f),
                new Vector2(-78f, 34f));
            refs.TitleText = CreateText(
                left,
                "TitleText",
                "Encounter Plan Review",
                semiBoldFont,
                34f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-13f, -108f),
                new Vector2(-78f, 86f));
            refs.IdentityText = CreateText(
                left,
                "IdentityText",
                string.Empty,
                mediumFont,
                18f,
                Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-13f, -214f),
                new Vector2(-78f, 116f));

            RectTransform objectivePlate = CreatePanel(
                left,
                "ObjectivePlate",
                PanelSoft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-13f, -353f),
                new Vector2(-78f, 126f));
            CreateText(
                objectivePlate,
                "ObjectiveLabel",
                "OBJECTIVE CONTRACT",
                semiBoldFont,
                15f,
                Cyan,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-9f, -17f),
                new Vector2(-54f, 26f));
            refs.ObjectiveText = CreateText(
                objectivePlate,
                "ObjectiveText",
                string.Empty,
                mediumFont,
                20f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-9f, -55f),
                new Vector2(-54f, 58f));

            refs.StateText = CreateText(
                left,
                "StateText",
                string.Empty,
                semiBoldFont,
                25f,
                Green,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-13f, -454f),
                new Vector2(-78f, 42f));
            refs.ProgressText = CreateText(
                left,
                "ProgressText",
                string.Empty,
                mediumFont,
                18f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-13f, -508f),
                new Vector2(-78f, 56f));
            refs.CurrentSpawnText = CreateText(
                left,
                "CurrentSpawnText",
                string.Empty,
                mediumFont,
                17f,
                Cyan,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-13f, -581f),
                new Vector2(-78f, 82f));
            refs.OwnershipBoundaryText = CreateText(
                left,
                "OwnershipBoundaryText",
                string.Empty,
                mediumFont,
                16f,
                Amber,
                TextAlignmentOptions.BottomLeft,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-13f, 20f),
                new Vector2(-78f, 128f));

            RectTransform main = CreatePanel(
                safeRect,
                "EncounterTimelinePanel",
                new Color(Panel.r, Panel.g, Panel.b, 0.975f),
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(269f, 10f),
                new Vector2(-610f, -320f));
            CreateText(
                main,
                "TimelineTitle",
                "ORDERED WAVE PLAN",
                semiBoldFont,
                23f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-12f, -42f),
                new Vector2(-72f, 46f));
            CreateText(
                main,
                "TimelineSubtitle",
                "Explicit order  /  explicit spawn tickets  /  wave-local DefeatAll",
                mediumFont,
                16f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-12f, -79f),
                new Vector2(-72f, 32f));

            refs.WaveTitleTexts = new TMP_Text[3];
            refs.WaveStateTexts = new TMP_Text[3];
            refs.WaveDetailTexts = new TMP_Text[3];
            refs.WaveAccentImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                float leftAnchor = i / 3f;
                float rightAnchor = (i + 1) / 3f;
                RectTransform card = CreatePanel(
                    main,
                    $"WaveCard_{i + 1}",
                    InkSoft,
                    new Vector2(leftAnchor, 0f),
                    new Vector2(rightAnchor, 1f),
                    new Vector2(0f, -10f),
                    new Vector2(-36f, -220f));
                refs.WaveAccentImages[i] = CreateImage(
                    card,
                    "Accent",
                    i == 0 ? Cyan : Blue,
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, -4f),
                    new Vector2(0f, 8f));
                CreateText(
                    card,
                    "OrderBadge",
                    $"0{i + 1}",
                    semiBoldFont,
                    28f,
                    Muted,
                    TextAlignmentOptions.TopRight,
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(-16f, -21f),
                    new Vector2(52f, 44f));
                refs.WaveTitleTexts[i] = CreateText(
                    card,
                    "WaveTitleText",
                    $"WAVE {i + 1}",
                    semiBoldFont,
                    23f,
                    White,
                    TextAlignmentOptions.TopLeft,
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(-27.5f, -26f),
                    new Vector2(-89f, 48f));
                refs.WaveStateTexts[i] = CreateText(
                    card,
                    "WaveStateText",
                    "PENDING",
                    semiBoldFont,
                    16f,
                    Amber,
                    TextAlignmentOptions.TopLeft,
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, -82f),
                    new Vector2(-34f, 30f));
                refs.WaveDetailTexts[i] = CreateText(
                    card,
                    "WaveDetailText",
                    string.Empty,
                    mediumFont,
                    16f,
                    Muted,
                    TextAlignmentOptions.TopLeft,
                    new Vector2(0f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, -54f),
                    new Vector2(-34f, -144f));
            }

            RectTransform actionBar = CreatePanel(
                safeRect,
                "ActionBar",
                new Color(0.010f, 0.030f, 0.055f, 0.985f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(269f, 70f),
                new Vector2(-610f, 88f));
            refs.BeginButton = CreateButton(
                actionBar,
                "BeginButton",
                "BEGIN REVIEW",
                semiBoldFont,
                Cyan,
                new Vector2(0f, 0.5f),
                new Vector2(0.2f, 0.5f),
                Vector2.zero,
                new Vector2(-12f, 58f));
            refs.ResolveButton = CreateButton(
                actionBar,
                "ResolveButton",
                "RESOLVE TARGET",
                semiBoldFont,
                Green,
                new Vector2(0.2f, 0.5f),
                new Vector2(0.4f, 0.5f),
                Vector2.zero,
                new Vector2(-12f, 58f));
            refs.AdvanceButton = CreateButton(
                actionBar,
                "AdvanceButton",
                "NEXT WAVE",
                semiBoldFont,
                Cyan,
                new Vector2(0.4f, 0.5f),
                new Vector2(0.6f, 0.5f),
                Vector2.zero,
                new Vector2(-12f, 58f));
            refs.InterruptButton = CreateButton(
                actionBar,
                "InterruptButton",
                "INTERRUPT",
                semiBoldFont,
                Amber,
                new Vector2(0.6f, 0.5f),
                new Vector2(0.8f, 0.5f),
                Vector2.zero,
                new Vector2(-12f, 58f));
            refs.ResetButton = CreateButton(
                actionBar,
                "ResetButton",
                "RESET",
                semiBoldFont,
                White,
                new Vector2(0.8f, 0.5f),
                new Vector2(1f, 0.5f),
                Vector2.zero,
                new Vector2(-12f, 58f));

            CreateText(
                safeRect,
                "FooterBoundary",
                "SIMULATION ONLY  /  NO PREFABS  /  NO HEALTH  /  NO RESULT  /  NO REWARD",
                mediumFont,
                15f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-18f, 13f),
                new Vector2(-108f, 28f));
            return refs;
        }

        private static List<string> ValidateGeneratedReview(Scene scene)
        {
            var issues = new List<string>();
            ValidateProtectedInputPresence(issues);
            if (!File.Exists(AssetPathToAbsolutePath(EditorBuildSettingsAssetPath)))
            {
                issues.Add(
                    $"Unity Build Settings asset `{EditorBuildSettingsAssetPath}` is missing.");
            }

            StageEncounterPlanProfile profile =
                AssetDatabase.LoadAssetAtPath<StageEncounterPlanProfile>(ProfilePath);
            bool profileIsValid = false;
            if (profile == null)
            {
                issues.Add("The CF-01 profile asset is missing.");
            }
            else
            {
                profileIsValid = profile.TryValidate(out string profileError);
                if (!profileIsValid)
                {
                    issues.Add("The CF-01 profile is invalid: " + profileError);
                }

                if (profile.SchemaVersion != 1
                    || profile.Revision != 1
                    || profile.PlanId != PlanId
                    || profile.StageId != StageId
                    || profile.EncounterId != EncounterId
                    || profile.WaveCount != 3)
                {
                    issues.Add("The CF-01 profile identity or three-wave contract drifted.");
                }

                if (profileIsValid
                    && !string.Equals(
                        profile.CanonicalDigest,
                        ComputeExpectedProfileDigest(),
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        "The CF-01 profile is valid but does not match the exact reviewed plan digest.");
                }

                if (profileIsValid)
                {
                    int totalCombatants = 0;
                    for (int i = 0; i < profile.WaveCount; i++)
                    {
                        StageEncounterPlanProfile.WaveDefinition wave = profile.GetWave(i);
                        if (wave == null)
                        {
                            issues.Add($"CF-01 wave {i} is null.");
                            continue;
                        }

                        totalCombatants += wave.TotalCombatantCount;
                    }

                    if (totalCombatants != 7)
                    {
                        issues.Add(
                            $"CF-01 must expose 7 simulated combatants; found {totalCombatants}.");
                    }
                }
            }

            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                issues.Add($"The active review scene must be `{ScenePath}`.");
                return issues;
            }

            Camera[] cameras = FindAllInScene<Camera>(scene);
            Canvas[] canvases = FindAllInScene<Canvas>(scene);
            ContentFactoryEncounterPlanReviewController[] controllers =
                FindAllInScene<ContentFactoryEncounterPlanReviewController>(scene);
            UISafeAreaRoot[] safeAreas = FindAllInScene<UISafeAreaRoot>(scene);
            UIResponsiveRoot[] responsiveRoots = FindAllInScene<UIResponsiveRoot>(scene);
            EventSystem[] eventSystems = FindAllInScene<EventSystem>(scene);
            InputSystemUIInputModule[] inputModules =
                FindAllInScene<InputSystemUIInputModule>(scene);
            Button[] buttons = FindAllInScene<Button>(scene);
            RequireExactCount(issues, cameras, 1, "Camera");
            RequireExactCount(issues, canvases, 1, "Canvas");
            RequireExactCount(issues, controllers, 1, "CF-01 controller");
            RequireExactCount(issues, safeAreas, 1, "UISafeAreaRoot");
            RequireExactCount(issues, responsiveRoots, 1, "UIResponsiveRoot");
            RequireExactCount(issues, eventSystems, 1, "EventSystem");
            RequireExactCount(issues, inputModules, 1, "InputSystemUIInputModule");
            RequireExactCount(issues, buttons, 5, "local action Button");
            ValidateNoMissingMonoBehaviours(issues, scene);
            ValidateTextFontBindings(issues, scene);

            if (canvases.Length == 1)
            {
                Canvas canvas = canvases[0];
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (canvas.renderMode != RenderMode.ScreenSpaceCamera
                    || canvas.worldCamera == null
                    || (cameras.Length == 1 && canvas.worldCamera != cameras[0])
                    || scaler == null
                    || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                    || scaler.referenceResolution != new Vector2(1920f, 1080f)
                    || canvas.GetComponent<GraphicRaycaster>() == null)
                {
                    issues.Add("CF-01 Canvas camera/scaler/raycaster configuration is invalid.");
                }
            }

            if (responsiveRoots.Length == 1
                && safeAreas.Length == 1
                && canvases.Length == 1)
            {
                var responsiveSerialized = new SerializedObject(responsiveRoots[0]);
                responsiveSerialized.UpdateIfRequiredOrScript();
                SerializedProperty catalogProperty =
                    responsiveSerialized.FindProperty("catalog");
                SerializedProperty scalerProperty =
                    responsiveSerialized.FindProperty("canvasScaler");
                SerializedProperty safeProperty =
                    responsiveSerialized.FindProperty("safeAreaRoot");
                if (catalogProperty == null
                    || scalerProperty == null
                    || safeProperty == null
                    || !ReferenceEquals(
                        catalogProperty.objectReferenceValue,
                        AssetDatabase.LoadAssetAtPath<UIResponsiveLayoutCatalog>(
                            ResponsiveCatalogPath))
                    || !ReferenceEquals(
                        scalerProperty.objectReferenceValue,
                        canvases[0].GetComponent<CanvasScaler>())
                    || !ReferenceEquals(
                        safeProperty.objectReferenceValue,
                        safeAreas[0]))
                {
                    issues.Add("CF-01 responsive catalog/scaler/safe-area bindings are invalid.");
                }
            }

            if (controllers.Length == 1)
            {
                ValidateController(issues, controllers[0], profile, buttons);
            }

            foreach (Button button in buttons)
            {
                if (button.onClick.GetPersistentEventCount() != 0)
                {
                    issues.Add($"Button `{button.name}` has a persistent callback.");
                }

                Rect rect = (button.transform as RectTransform)?.rect ?? Rect.zero;
                if (rect.width < 48f || rect.height < 48f)
                {
                    issues.Add($"Button `{button.name}` is below the 48 px touch target.");
                }
            }

            foreach (MonoBehaviour behaviour in FindAllInScene<MonoBehaviour>(scene))
            {
                if (behaviour == null)
                {
                    continue;
                }

                Type behaviourType = behaviour.GetType();
                if (IsForbiddenRuntimeComponent(behaviourType))
                {
                    issues.Add(
                        $"Forbidden runtime component `{behaviourType.FullName}` exists in CF-01.");
                }
            }

            if (EditorBuildSettings.scenes.Any(
                    entry => string.Equals(entry.path, ScenePath, StringComparison.Ordinal)))
            {
                issues.Add("CF-01 review scene must remain absent from Build Settings.");
            }

            return issues;
        }

        private static void ValidateController(
            List<string> issues,
            ContentFactoryEncounterPlanReviewController controller,
            StageEncounterPlanProfile profile,
            Button[] sceneButtons)
        {
            var serialized = new SerializedObject(controller);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty profileProperty = serialized.FindProperty("profile");
            if (profileProperty == null
                || !ReferenceEquals(profileProperty.objectReferenceValue, profile))
            {
                issues.Add("CF-01 controller is not bound to the exact profile asset.");
            }

            string[] textFields =
            {
                "admissionBoundaryText",
                "titleText",
                "identityText",
                "objectiveText",
                "stateText",
                "progressText",
                "currentSpawnText",
                "ownershipBoundaryText"
            };
            foreach (string field in textFields)
            {
                SerializedProperty property = serialized.FindProperty(field);
                if (property == null || property.objectReferenceValue == null)
                {
                    issues.Add($"CF-01 controller text binding `{field}` is missing.");
                }
            }

            string[] arrayFields =
            {
                "waveTitleTexts",
                "waveStateTexts",
                "waveDetailTexts",
                "waveAccentImages"
            };
            foreach (string field in arrayFields)
            {
                SerializedProperty property = serialized.FindProperty(field);
                if (property == null || !property.isArray || property.arraySize != 3)
                {
                    issues.Add($"CF-01 controller array `{field}` must contain exactly 3 rows.");
                    continue;
                }

                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        issues.Add($"CF-01 controller `{field}[{i}]` is missing.");
                    }
                }
            }

            string[] buttonFields =
            {
                "beginButton",
                "resolveButton",
                "advanceButton",
                "interruptButton",
                "resetButton"
            };
            string[] expectedButtonNames =
            {
                "BeginButton",
                "ResolveButton",
                "AdvanceButton",
                "InterruptButton",
                "ResetButton"
            };
            var boundButtonIds = new HashSet<int>();
            for (int i = 0; i < buttonFields.Length; i++)
            {
                string field = buttonFields[i];
                string expectedButtonName = expectedButtonNames[i];
                SerializedProperty property = serialized.FindProperty(field);
                Button boundButton = property?.objectReferenceValue as Button;
                if (boundButton == null)
                {
                    issues.Add($"CF-01 controller action binding `{field}` is missing.");
                    continue;
                }

                if (!boundButtonIds.Add(boundButton.GetInstanceID()))
                {
                    issues.Add(
                        $"CF-01 controller action binding `{field}` reuses `{boundButton.name}`.");
                }

                Button[] exactNamedButtons = (sceneButtons ?? Array.Empty<Button>())
                    .Where(
                        candidate => candidate != null
                            && string.Equals(
                                candidate.name,
                                expectedButtonName,
                                StringComparison.Ordinal))
                    .ToArray();
                if (exactNamedButtons.Length != 1)
                {
                    issues.Add(
                        $"CF-01 requires exactly one scene Button named `{expectedButtonName}`; "
                        + $"found {exactNamedButtons.Length}.");
                    continue;
                }

                if (!ReferenceEquals(boundButton, exactNamedButtons[0]))
                {
                    issues.Add(
                        $"CF-01 controller action binding `{field}` must reference the exact "
                        + $"`{expectedButtonName}` scene Button; found `{boundButton.name}`.");
                }
            }
        }

        private static void ValidateProtectedInputPresence(List<string> issues)
        {
            foreach (string assetPath in ImmutableProtectedAssets)
            {
                if (!File.Exists(AssetPathToAbsolutePath(assetPath)))
                {
                    issues.Add($"Protected CF-01 input `{assetPath}` is missing.");
                }

                string metaPath = assetPath + ".meta";
                if (!File.Exists(AssetPathToAbsolutePath(metaPath)))
                {
                    issues.Add($"Protected CF-01 input metadata `{metaPath}` is missing.");
                }
            }

            foreach (string assetPath in RequiredDynamicFontAssets)
            {
                if (!File.Exists(AssetPathToAbsolutePath(assetPath)))
                {
                    issues.Add($"Required CF-01 dynamic font `{assetPath}` is missing.");
                }

                string metaPath = assetPath + ".meta";
                if (!File.Exists(AssetPathToAbsolutePath(metaPath)))
                {
                    issues.Add(
                        $"Required CF-01 dynamic font metadata `{metaPath}` is missing.");
                }
            }
        }

        private static void ValidateTextFontBindings(
            List<string> issues,
            Scene scene)
        {
            TMP_FontAsset mediumFont =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MediumFontPath);
            TMP_FontAsset semiBoldFont =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SemiBoldFontPath);
            if (mediumFont == null || semiBoldFont == null)
            {
                issues.Add(
                    "CF-01 requires both exact dynamic TMP font assets for role validation.");
                return;
            }

            TMP_Text[] texts = FindAllInScene<TMP_Text>(scene);
            int expectedTextCount = ExpectedMediumTextRoles.Values.Sum()
                + ExpectedSemiBoldTextRoles.Values.Sum();
            if (texts.Length != expectedTextCount)
            {
                issues.Add(
                    $"CF-01 requires exactly {expectedTextCount} TMP text roles; "
                    + $"found {texts.Length}.");
            }

            ValidateTextRoleCounts(issues, texts, ExpectedMediumTextRoles);
            ValidateTextRoleCounts(issues, texts, ExpectedSemiBoldTextRoles);

            foreach (TMP_Text text in texts)
            {
                bool expectsMedium = ExpectedMediumTextRoles.ContainsKey(text.name);
                bool expectsSemiBold = ExpectedSemiBoldTextRoles.ContainsKey(text.name);
                if (!expectsMedium && !expectsSemiBold)
                {
                    issues.Add(
                        $"Unexpected CF-01 TMP text role `{GetHierarchyPath(text.transform)}`.");
                    continue;
                }

                if (text.name == "Label"
                    && (text.transform.parent == null
                        || text.transform.parent.GetComponent<Button>() == null))
                {
                    issues.Add(
                        $"CF-01 button label `{GetHierarchyPath(text.transform)}` "
                        + "is not parented directly under an action Button.");
                }

                TMP_FontAsset expectedFont = expectsMedium ? mediumFont : semiBoldFont;
                string expectedPath = expectsMedium ? MediumFontPath : SemiBoldFontPath;
                if (!ReferenceEquals(text.font, expectedFont)
                    || !string.Equals(
                        AssetDatabase.GetAssetPath(text.font),
                        expectedPath,
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        $"CF-01 text `{GetHierarchyPath(text.transform)}` must bind exact font "
                        + $"`{expectedPath}`.");
                }
            }
        }

        private static void ValidateTextRoleCounts(
            List<string> issues,
            TMP_Text[] texts,
            IReadOnlyDictionary<string, int> expectedRoles)
        {
            foreach (KeyValuePair<string, int> expected in expectedRoles)
            {
                int actual = texts.Count(text => text.name == expected.Key);
                if (actual != expected.Value)
                {
                    issues.Add(
                        $"CF-01 TMP role `{expected.Key}` requires {expected.Value} instance(s); "
                        + $"found {actual}.");
                }
            }
        }

        private static void ValidateNoMissingMonoBehaviours(
            List<string> issues,
            Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform current in root.GetComponentsInChildren<Transform>(true))
                {
                    int missingCount = current.gameObject
                        .GetComponents<Component>()
                        .Count(component => component == null);
                    if (missingCount > 0)
                    {
                        issues.Add(
                            $"CF-01 object `{GetHierarchyPath(current)}` contains "
                            + $"{missingCount} missing MonoBehaviour script(s).");
                    }
                }
            }
        }

        private static bool IsForbiddenRuntimeComponent(Type componentType)
        {
            if (componentType == null)
            {
                return false;
            }

            if (ForbiddenComponentTypes.Contains(componentType))
            {
                return true;
            }

            if (typeof(DimensionBrawl.AI.ICombatAiAgent).IsAssignableFrom(componentType))
            {
                return true;
            }

            string componentNamespace = componentType.Namespace ?? string.Empty;
            return IsNamespaceOrChild(componentNamespace, "DimensionBrawl.Combat")
                || IsNamespaceOrChild(componentNamespace, "DimensionBrawl.AI")
                || IsNamespaceOrChild(componentNamespace, "DimensionBrawl.Enemies")
                || IsNamespaceOrChild(componentNamespace, "IsekaiBrawl.Gameplay");
        }

        private static bool IsNamespaceOrChild(string value, string namespaceRoot)
        {
            return string.Equals(value, namespaceRoot, StringComparison.Ordinal)
                || value.StartsWith(namespaceRoot + ".", StringComparison.Ordinal);
        }

        private static string GetHierarchyPath(Transform current)
        {
            var names = new Stack<string>();
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }

        private static RectTransform CreatePanel(
            RectTransform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMaxOrSize)
        {
            return CreateImage(
                    parent,
                    name,
                    color,
                    anchorMin,
                    anchorMax,
                    offsetMin,
                    offsetMaxOrSize)
                .rectTransform;
        }

        private static Image CreateImage(
            RectTransform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPositionOrOffsetMin,
            Vector2 sizeDeltaOrOffsetMax)
        {
            GameObject gameObject = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            ConfigureRect(
                rect,
                anchorMin,
                anchorMax,
                anchoredPositionOrOffsetMin,
                sizeDeltaOrOffsetMax);
            Image image = gameObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(
            RectTransform parent,
            string name,
            string value,
            TMP_FontAsset font,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPositionOrOffsetMin,
            Vector2 sizeDeltaOrOffsetMax)
        {
            GameObject gameObject = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            ConfigureRect(
                rect,
                anchorMin,
                anchorMax,
                anchoredPositionOrOffsetMin,
                sizeDeltaOrOffsetMax);
            TMP_Text text = gameObject.GetComponent<TMP_Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Normal;
            text.color = color;
            text.alignment = alignment;
            text.text = value ?? string.Empty;
            text.raycastTarget = false;
            text.richText = true;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private static Button CreateButton(
            RectTransform parent,
            string name,
            string label,
            TMP_FontAsset font,
            Color accent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            Image image = CreateImage(
                parent,
                name,
                new Color(0.052f, 0.112f, 0.162f, 0.98f),
                anchorMin,
                anchorMax,
                anchoredPosition,
                sizeDelta);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.10f, 1.10f, 1.10f, 1f);
            colors.pressedColor = new Color(0.64f, 0.82f, 0.90f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.30f, 0.36f, 0.42f, 0.62f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            CreateText(
                image.rectTransform,
                "Label",
                label,
                font,
                16f,
                accent,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            return button;
        }

        private static void ConfigureRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 positionOrOffsetMin,
            Vector2 sizeOrOffsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(
                Mathf.Approximately(anchorMin.x, anchorMax.x)
                    ? anchorMin.x
                    : 0.5f,
                Mathf.Approximately(anchorMin.y, anchorMax.y)
                    ? anchorMin.y
                    : 0.5f);
            rect.anchoredPosition = positionOrOffsetMin;
            rect.sizeDelta = sizeOrOffsetMax;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            GameObject gameObject = new(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            gameObject.transform.SetParent(parent, false);
        }

        private static void ConfigureSafeArea(
            UISafeAreaRoot safeAreaRoot,
            RectTransform target,
            float extraInsetPixels)
        {
            var serialized = new SerializedObject(safeAreaRoot);
            serialized.FindProperty("applyOnEnable").boolValue = true;
            serialized.FindProperty("target").objectReferenceValue = target;
            serialized.FindProperty("mode").intValue = (int)UISafeAreaMode.InsetsOnly;
            serialized.FindProperty("extraInsetPixels").floatValue = extraInsetPixels;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Stretch(target);
        }

        private static void ConfigureResponsiveRoot(
            UIResponsiveRoot responsiveRoot,
            CanvasScaler scaler,
            UISafeAreaRoot safeAreaRoot)
        {
            UIResponsiveLayoutCatalog catalog =
                AssetDatabase.LoadAssetAtPath<UIResponsiveLayoutCatalog>(
                    ResponsiveCatalogPath);
            string guid = AssetDatabase.AssetPathToGUID(ResponsiveCatalogPath);
            if (responsiveRoot == null
                || scaler == null
                || safeAreaRoot == null
                || catalog == null
                || !string.Equals(guid, ResponsiveCatalogGuid, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "CF-01 responsive root or protected layout catalog is invalid: "
                    + $"root={responsiveRoot != null}, scaler={scaler != null}, "
                    + $"safe={safeAreaRoot != null}, catalog={catalog != null}, "
                    + $"guid=`{guid}`, expected=`{ResponsiveCatalogGuid}`.");
            }

            var serialized = new SerializedObject(responsiveRoot);
            serialized.FindProperty("catalog").objectReferenceValue = catalog;
            serialized.FindProperty("canvasScaler").objectReferenceValue = scaler;
            serialized.FindProperty("safeAreaRoot").objectReferenceValue = safeAreaRoot;
            serialized.FindProperty("breakpointText").objectReferenceValue = null;
            serialized.FindProperty("applyCanvasScaler").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var found = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                found.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return found.ToArray();
        }

        private static void RequireExactCount<T>(
            List<string> issues,
            T[] values,
            int expected,
            string label)
        {
            if (values == null || values.Length != expected)
            {
                issues.Add($"CF-01 requires exactly {expected} {label}; found {values?.Length ?? 0}.");
            }
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new FileNotFoundException($"Required asset `{path}` is missing.", path);
            }

            return asset;
        }

        private static void GuardSceneOutputPath()
        {
            UnityEngine.Object occupied = AssetDatabase.LoadMainAssetAtPath(ScenePath);
            if (occupied == null)
            {
                if (File.Exists(AssetPathToAbsolutePath(ScenePath)))
                {
                    throw new InvalidOperationException(
                        $"Review scene path `{ScenePath}` contains an unimported file.");
                }

                return;
            }

            if (occupied is not SceneAsset)
            {
                throw new InvalidOperationException(
                    $"Review scene path `{ScenePath}` is occupied by `{occupied.GetType().Name}`.");
            }

            string[] dependencies = AssetDatabase.GetDependencies(ScenePath, true);
            string serializedScene = File.ReadAllText(AssetPathToAbsolutePath(ScenePath));
            bool ownsController = dependencies.Contains(
                ControllerScriptPath,
                StringComparer.Ordinal);
            bool ownsRoot = serializedScene.Contains(
                "m_Name: ContentFactoryEncounterPlanReview_Root",
                StringComparison.Ordinal);
            bool ownsFlow = serializedScene.Contains(
                "m_Name: ContentFactoryEncounterPlanReviewFlow",
                StringComparison.Ordinal);
            if (!ownsController || !ownsRoot || !ownsFlow)
            {
                throw new InvalidOperationException(
                    $"Existing scene `{ScenePath}` is not an owned CF-01 review output.");
            }
        }

        private static void GuardProfileOutputPath()
        {
            UnityEngine.Object occupied = AssetDatabase.LoadMainAssetAtPath(ProfilePath);
            if (occupied == null)
            {
                if (File.Exists(AssetPathToAbsolutePath(ProfilePath)))
                {
                    throw new InvalidOperationException(
                        $"CF-01 profile path `{ProfilePath}` contains an unimported file.");
                }

                return;
            }

            if (occupied is not StageEncounterPlanProfile profile)
            {
                throw new InvalidOperationException(
                    $"CF-01 profile path `{ProfilePath}` is occupied by "
                    + $"`{occupied.GetType().Name}`.");
            }

            if (!string.Equals(profile.PlanId, PlanId, StringComparison.Ordinal)
                || !string.Equals(profile.StageId, StageId, StringComparison.Ordinal)
                || !string.Equals(profile.EncounterId, EncounterId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Existing profile `{ProfilePath}` is not an owned CF-01 review output.");
            }
        }

        private sealed class OutputFileSnapshot
        {
            private readonly FileSnapshot[] files;

            private OutputFileSnapshot(FileSnapshot[] files)
            {
                this.files = files;
            }

            public static OutputFileSnapshot Capture(params string[] assetPaths)
            {
                FileSnapshot[] snapshots = assetPaths
                    .SelectMany(path => new[] { path, path + ".meta" })
                    .Select(path => new FileSnapshot(AssetPathToAbsolutePath(path)))
                    .ToArray();
                return new OutputFileSnapshot(snapshots);
            }

            public void Restore()
            {
                UnityEngine.Object profile =
                    AssetDatabase.LoadMainAssetAtPath(ProfilePath);
                if (profile != null)
                {
                    EditorUtility.ClearDirty(profile);
                }

                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);
                foreach (FileSnapshot file in files)
                {
                    file.Restore();
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }

        private sealed class FileSnapshot
        {
            private readonly string absolutePath;
            private readonly bool existed;
            private readonly byte[] bytes;

            public FileSnapshot(string absolutePath)
            {
                this.absolutePath = absolutePath;
                existed = File.Exists(absolutePath);
                bytes = existed ? File.ReadAllBytes(absolutePath) : Array.Empty<byte>();
            }

            public void Restore()
            {
                if (existed)
                {
                    File.WriteAllBytes(absolutePath, bytes);
                    return;
                }

                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = PathParent(folderPath);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureAssetFolder(parent);
            }

            string name = Path.GetFileName(folderPath);
            if (string.IsNullOrWhiteSpace(AssetDatabase.CreateFolder(parent, name)))
            {
                throw new InvalidOperationException($"Failed to create asset folder `{folderPath}`.");
            }
        }

        private static string PathParent(string path)
        {
            string normalized = path.Replace('\\', '/').TrimEnd('/');
            int separator = normalized.LastIndexOf('/');
            return separator > 0 ? normalized.Substring(0, separator) : "Assets";
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root is unavailable.");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }

        private static void AppendFileDigest(StringBuilder builder, string assetPath)
        {
            string absolutePath = AssetPathToAbsolutePath(assetPath);
            builder.Append(assetPath).Append('|');
            if (!File.Exists(absolutePath))
            {
                builder.Append("MISSING\n");
                return;
            }

            using SHA256 sha = SHA256.Create();
            builder.Append(ToLowerHex(sha.ComputeHash(File.ReadAllBytes(absolutePath))))
                .Append('\n');
        }

        private static string ToLowerHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
            {
                builder.Append(value.ToString("x2"));
            }

            return builder.ToString();
        }

        private static void ThrowIfIssues(string title, List<string> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                title + ":\n- " + string.Join("\n- ", issues));
        }

        private sealed class ReviewUiRefs
        {
            public UISafeAreaRoot SafeAreaRoot;
            public TMP_Text AdmissionBoundaryText;
            public TMP_Text TitleText;
            public TMP_Text IdentityText;
            public TMP_Text ObjectiveText;
            public TMP_Text StateText;
            public TMP_Text ProgressText;
            public TMP_Text CurrentSpawnText;
            public TMP_Text OwnershipBoundaryText;
            public TMP_Text[] WaveTitleTexts;
            public TMP_Text[] WaveStateTexts;
            public TMP_Text[] WaveDetailTexts;
            public Image[] WaveAccentImages;
            public Button BeginButton;
            public Button ResolveButton;
            public Button AdvanceButton;
            public Button InterruptButton;
            public Button ResetButton;
        }
    }
}
