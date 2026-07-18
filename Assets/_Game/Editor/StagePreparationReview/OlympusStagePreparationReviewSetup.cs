using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StagePreparationReview;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.StagePreparationReview
{
    /// <summary>
    /// Deterministically authors the isolated PREP-01 stage-preparation review scene.
    /// The generated scene remains outside Build Settings and projects canonical stage/summon
    /// read models without owning account loadout, recommendation, routing, combat start,
    /// persistence, rewards, or StageRun state.
    /// </summary>
    public static class OlympusStagePreparationReviewSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Review/UI_OlympusStagePreparationReview.unity";
        public const string ProfilePath =
            "Assets/_Game/DesignData/UI/Review/DB_UIStagePreparation_OlympusReview.asset";
        public const string StageCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        public const string SummonSlot1ProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset";
        public const string SummonSlot2ProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot2_LaserSoldier.asset";
        public const string SummonSlot3ProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot3_FireDragon.asset";
        public const string Slot1IconPath =
            "Assets/_Game/UI/CombatHud/Art/DimensionHud/Hud_SummonSlot1Icon.png";
        public const string Slot2IconPath =
            "Assets/_Game/UI/CombatHud/Art/DimensionHud/Hud_SummonSlot2Icon.png";
        public const string Slot3IconPath =
            "Assets/_Game/UI/CombatHud/Art/DimensionHud/Hud_SummonSlot3Icon.png";
        public const string BackgroundArtPath =
            "Assets/_Game/UI/ChapterHubReview/Art/BG_OlympusChapterHub_Review.png";

        public const string ReviewId = "PREP-01";
        public const string CanonicalCatalogEntryId = "story_v1_training_route";

        private const string TemporaryReviewBoundaryStatus =
            "PREP-01  •  REVIEW SAMPLE  •  TEMP_DO_NOT_SHIP";
        private const string ReviewTitleLocalizationKey =
            "ui.review.stage_preparation.olympus.title";
        private const string ReviewTitleFallback = "올림포스 스테이지 준비 검토";
        private const string PilotPresentationId = "review.pilot.fixed.olympus";
        private const string PilotTitleLocalizationKey =
            "ui.review.stage_preparation.fixed_pilot.title";
        private const string PilotTitleFallback = "고정 파일럿 프레젠테이션";
        private const string PilotBoundaryFallback =
            "로컬 검토용 고정 프레젠테이션이며 스테이지 추천이 아닙니다.";

        private const string MediumFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_Medium_Dynamic.asset";
        private const string SemiBoldFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_SemiBold_Dynamic.asset";
        private const string ResponsiveCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIResponsiveLayouts.asset";
        private const string ResponsiveCatalogGuid =
            "964233ec7542aff4381a9e70ee1edfbd";

        private static readonly string[] CanonicalAssetsThatMustRemainUntouched =
        {
            StageCatalogPath,
            SummonSlot1ProfilePath,
            SummonSlot2ProfilePath,
            SummonSlot3ProfilePath,
            Slot1IconPath,
            Slot2IconPath,
            Slot3IconPath,
            BackgroundArtPath,
            ResponsiveCatalogPath,
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-Medium.otf",
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf"
        };

        private static readonly Color Ink = new Color(0.010f, 0.020f, 0.040f, 0.985f);
        private static readonly Color InkSoft = new Color(0.025f, 0.050f, 0.082f, 0.965f);
        private static readonly Color Panel = new Color(0.032f, 0.066f, 0.108f, 0.975f);
        private static readonly Color PanelSoft = new Color(0.060f, 0.110f, 0.170f, 0.94f);
        private static readonly Color Cyan = new Color(0.25f, 0.90f, 1.00f, 1f);
        private static readonly Color CyanSoft = new Color(0.43f, 0.75f, 0.88f, 1f);
        private static readonly Color Amber = new Color(1.00f, 0.68f, 0.27f, 1f);
        private static readonly Color White = new Color(0.95f, 0.985f, 1.00f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.72f, 0.81f, 1f);

        [MenuItem("Tools/DimensionBrawl/Review/Setup Olympus Stage Preparation Review")]
        public static void SetupMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Setup();
        }

        [MenuItem("Tools/DimensionBrawl/Review/Validate Olympus Stage Preparation Review")]
        public static void ValidateMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            RunBatchVerification();
        }

        public static void RunBatchSetup()
        {
            Setup();
        }

        public static void RunBatchVerification()
        {
            Dictionary<string, CanonicalAssetFingerprint> fingerprints =
                CaptureCanonicalFingerprints();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> issues = ValidateGeneratedReview(scene);
            AppendCanonicalFingerprintIssues(fingerprints, issues);
            ThrowIfIssues("Olympus stage-preparation review validation failed", issues);
            Debug.Log(
                "PREP-01 Olympus stage-preparation review validation passed. "
                + "The scene remains review-only and outside Build Settings.");
        }

        public static string ComputeCanonicalBoundaryDigest()
        {
            var builder = new StringBuilder(2048);
            foreach (string assetPath in CanonicalAssetsThatMustRemainUntouched)
            {
                AppendDigestField(builder, assetPath);
                AppendDigestField(builder, assetPath + ".meta");
            }

            using SHA256 sha = SHA256.Create();
            byte[] digest = sha.ComputeHash(
                Encoding.UTF8.GetBytes(builder.ToString()));
            return ToLowerHex(digest);
        }

        private static void Setup()
        {
            Dictionary<string, CanonicalAssetFingerprint> fingerprints =
                CaptureCanonicalFingerprints();

            EnsureAssetFolder(PathParent(ScenePath));
            EnsureAssetFolder(PathParent(ProfilePath));
            GuardSceneOutputPath();

            StagePreparationReviewProfile profile = EnsureProfile();
            UIStageCatalog stageCatalog = LoadRequired<UIStageCatalog>(StageCatalogPath);
            TMP_FontAsset mediumFont = LoadRequired<TMP_FontAsset>(MediumFontPath);
            TMP_FontAsset semiBoldFont = LoadRequired<TMP_FontAsset>(SemiBoldFontPath);
            Sprite background = LoadRequired<Sprite>(BackgroundArtPath);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject root = new GameObject("OlympusStagePreparationReview_Root");
            SceneManager.MoveGameObjectToScene(root, scene);

            Camera camera = CreateReviewCamera(root.transform);
            Canvas canvas = CreateCanvas(root.transform, camera);
            ReviewUiRefs ui = CreateReviewUi(
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
                "StagePreparationReviewFlow",
                typeof(OlympusStagePreparationReviewController));
            controllerObject.transform.SetParent(root.transform, false);
            OlympusStagePreparationReviewController controller =
                controllerObject.GetComponent<OlympusStagePreparationReviewController>();
            ConfigureController(controller, profile, stageCatalog, ui);

            ConfigureResponsiveRoot(
                canvas.GetComponent<UIResponsiveRoot>(),
                canvas.GetComponent<CanvasScaler>(),
                ui.SafeAreaRoot);
            ValidateResponsiveRootReferencesOrThrow(
                canvas.GetComponent<UIResponsiveRoot>(),
                LoadRequired<UIResponsiveLayoutCatalog>(ResponsiveCatalogPath),
                canvas.GetComponent<CanvasScaler>(),
                ui.SafeAreaRoot,
                "before scene save");

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException(
                    $"Failed to save PREP-01 review scene `{ScenePath}`.");
            }

            AssetDatabase.SaveAssetIfDirty(profile);
            NormalizeGeneratedYamlWhitespace(ProfilePath);
            NormalizeGeneratedYamlWhitespace(ScenePath);

            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> issues = ValidateGeneratedReview(scene);
            AppendCanonicalFingerprintIssues(fingerprints, issues);
            ThrowIfIssues("Olympus stage-preparation review setup failed", issues);

            Debug.Log(
                $"Created `{ScenePath}` with four review panels, three canonical summon "
                + "profiles, and no account loadout, recommendation, combat-start, or "
                + "StageRun ownership.");
        }

        private static StagePreparationReviewProfile EnsureProfile()
        {
            StagePreparationReviewProfile profile =
                AssetDatabase.LoadAssetAtPath<StagePreparationReviewProfile>(ProfilePath);
            if (profile == null)
            {
                UnityEngine.Object occupied = AssetDatabase.LoadMainAssetAtPath(ProfilePath);
                if (occupied != null
                    || File.Exists(AssetPathToAbsolutePath(ProfilePath))
                    || File.Exists(AssetPathToAbsolutePath(ProfilePath + ".meta")))
                {
                    throw new InvalidOperationException(
                        $"Review profile path `{ProfilePath}` is occupied by a wrong-type, "
                        + "unimportable, or orphan-metadata asset; refusing to overwrite it.");
                }

                profile = ScriptableObject.CreateInstance<StagePreparationReviewProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.name = "DB_UIStagePreparation_OlympusReview";

            SummonSlotActionProfile slot1 =
                LoadRequired<SummonSlotActionProfile>(SummonSlot1ProfilePath);
            SummonSlotActionProfile slot2 =
                LoadRequired<SummonSlotActionProfile>(SummonSlot2ProfilePath);
            SummonSlotActionProfile slot3 =
                LoadRequired<SummonSlotActionProfile>(SummonSlot3ProfilePath);
            Sprite icon1 = LoadRequired<Sprite>(Slot1IconPath);
            Sprite icon2 = LoadRequired<Sprite>(Slot2IconPath);
            Sprite icon3 = LoadRequired<Sprite>(Slot3IconPath);

            StagePreparationReviewProfile.SlotDefinition[] slots =
            {
                new StagePreparationReviewProfile.SlotDefinition(
                    "SummonSlot1",
                    "ui.review.stage_preparation.summon_slot1.title",
                    "차지 브루저",
                    "저장 EN 전열 돌파",
                    slot1,
                    icon1),
                new StagePreparationReviewProfile.SlotDefinition(
                    "SummonSlot2",
                    "ui.review.stage_preparation.summon_slot2.title",
                    "레이저 솔저",
                    "저비용 후열 원거리 압박",
                    slot2,
                    icon2),
                new StagePreparationReviewProfile.SlotDefinition(
                    "SummonSlot3",
                    "ui.review.stage_preparation.summon_slot3.title",
                    "파이어 드래곤",
                    "고비용 광역 화력",
                    slot3,
                    icon3)
            };
            profile.Configure(
                ReviewId,
                CanonicalCatalogEntryId,
                ReviewTitleLocalizationKey,
                ReviewTitleFallback,
                PilotPresentationId,
                PilotTitleLocalizationKey,
                PilotTitleFallback,
                PilotBoundaryFallback,
                slots);
            if (!profile.TryValidate(out string error))
            {
                throw new InvalidOperationException(
                    "Stage-preparation review profile is invalid: " + error);
            }

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void GuardSceneOutputPath()
        {
            SceneAsset existingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            UnityEngine.Object occupied = AssetDatabase.LoadMainAssetAtPath(ScenePath);
            bool fileExists = File.Exists(AssetPathToAbsolutePath(ScenePath));
            bool metaExists = File.Exists(AssetPathToAbsolutePath(ScenePath + ".meta"));
            if (existingScene != null)
            {
                if (occupied != existingScene || !fileExists || !metaExists)
                {
                    throw new InvalidOperationException(
                        $"Existing review scene `{ScenePath}` is not a complete imported "
                        + "SceneAsset; refusing deterministic regeneration.");
                }

                return;
            }

            if (occupied != null || fileExists || metaExists)
            {
                throw new InvalidOperationException(
                    $"Review scene path `{ScenePath}` is occupied by a wrong-type, "
                    + "unimportable, or orphan-metadata asset; refusing to overwrite it.");
            }
        }

        private static Camera CreateReviewCamera(Transform parent)
        {
            GameObject gameObject = new GameObject(
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
            GameObject gameObject = new GameObject(
                "OlympusStagePreparationReviewCanvas",
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

        private static ReviewUiRefs CreateReviewUi(
            RectTransform canvasRect,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            Sprite background)
        {
            var refs = new ReviewUiRefs();
            Image backgroundImage = CreateImage(
                canvasRect,
                "OlympusChapterHubBackground",
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
                new Color(0.01f, 0.025f, 0.05f, 0.45f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            GameObject safeObject = new GameObject(
                "SafeArea",
                typeof(RectTransform),
                typeof(UISafeAreaRoot));
            RectTransform safeRect = safeObject.GetComponent<RectTransform>();
            safeRect.SetParent(canvasRect, false);
            Stretch(safeRect);
            refs.SafeAreaRoot = safeObject.GetComponent<UISafeAreaRoot>();
            ConfigureRuntimeSafeArea(
                refs.SafeAreaRoot,
                safeRect,
                UISafeAreaMode.InsetsOnly,
                24f);

            CreateImage(
                safeRect,
                "TopRail",
                new Color(0.010f, 0.025f, 0.048f, 0.93f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -50f),
                new Vector2(0f, 100f));
            CreateText(
                safeRect,
                "ProductBreadcrumb",
                "DIMENSION BRAWL  /  OLYMPUS  /  STAGE PREPARATION",
                semiBoldFont,
                22f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(34f, -50f),
                new Vector2(820f, 44f));
            CreateText(
                safeRect,
                "ReviewBoundary",
                TemporaryReviewBoundaryStatus,
                semiBoldFont,
                17f,
                Amber,
                TextAlignmentOptions.MidlineRight,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-34f, -50f),
                new Vector2(760f, 42f));
            CreateText(
                safeRect,
                "ProductBoundary",
                "CANONICAL RUNTIME PRESET / NOT A STAGE RECOMMENDATION",
                mediumFont,
                15f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(34f, 26f),
                new Vector2(900f, 34f));

            refs.StageIntelPanel = CreateFlowGroup(safeRect, "StageIntelPanel");
            refs.LoadoutOverviewPanel = CreateFlowGroup(safeRect, "LoadoutOverviewPanel");
            refs.SummonDetailPanel = CreateFlowGroup(safeRect, "SummonDetailPanel");
            refs.ReviewConfirmPanel = CreateFlowGroup(safeRect, "ReviewConfirmPanel");

            BuildStageIntel(refs, mediumFont, semiBoldFont);
            BuildLoadoutOverview(refs, mediumFont, semiBoldFont);
            BuildSummonDetail(refs, mediumFont, semiBoldFont);
            BuildReviewConfirm(refs, mediumFont, semiBoldFont);

            SetGroup(refs.StageIntelPanel, true);
            SetGroup(refs.LoadoutOverviewPanel, false);
            SetGroup(refs.SummonDetailPanel, false);
            SetGroup(refs.ReviewConfirmPanel, false);
            return refs;
        }

        private static void BuildStageIntel(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.StageIntelPanel.transform as RectTransform;
            Image plate = CreateImage(
                root,
                "StageIntelPlate",
                new Color(Panel.r, Panel.g, Panel.b, 0.95f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(72f, -14f),
                new Vector2(980f, 760f));
            CreateImage(
                plate.rectTransform,
                "StageIntelAccent",
                Cyan,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(3f, 0f),
                new Vector2(6f, 0f));
            refs.IntelReviewTitle = CreateText(
                plate.rectTransform,
                "IntelReviewTitle",
                ReviewTitleFallback,
                semiBoldFont,
                42f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -48f),
                new Vector2(-92f, 64f));
            refs.IntelStageCode = CreateText(
                plate.rectTransform,
                "IntelStageCode",
                CanonicalCatalogEntryId,
                semiBoldFont,
                17f,
                Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -122f),
                new Vector2(-92f, 32f));
            refs.IntelStageTitle = CreateText(
                plate.rectTransform,
                "IntelStageTitle",
                "CANONICAL STAGE PROJECTION",
                semiBoldFont,
                30f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -174f),
                new Vector2(-92f, 50f));
            refs.IntelSummary = CreateText(
                plate.rectTransform,
                "IntelSummary",
                "DB_UIStageCatalog의 검증된 프레젠테이션을 읽습니다.",
                mediumFont,
                22f,
                Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -238f),
                new Vector2(-92f, 82f));
            CreateText(
                plate.rectTransform,
                "ObjectiveLabel",
                "OBJECTIVE",
                semiBoldFont,
                16f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(46f, -340f),
                new Vector2(220f, 28f));
            refs.IntelObjective = CreateText(
                plate.rectTransform,
                "IntelObjective",
                "Canonical objective unavailable.",
                mediumFont,
                22f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -380f),
                new Vector2(-92f, 78f));
            CreateText(
                plate.rectTransform,
                "ThreatLabel",
                "FEATURED THREAT",
                semiBoldFont,
                16f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(46f, -478f),
                new Vector2(220f, 28f));
            refs.IntelThreatTags = CreateText(
                plate.rectTransform,
                "IntelThreatTags",
                "NO VERIFIED SOURCE / FEATURED THREAT HIDDEN",
                mediumFont,
                20f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -514f),
                new Vector2(-92f, 42f));
            CreateText(
                plate.rectTransform,
                "SummonRoleLabel",
                "FEATURED SUMMON NEED",
                semiBoldFont,
                16f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(46f, -570f),
                new Vector2(320f, 28f));
            refs.IntelRecommendedSummonRole = CreateText(
                plate.rectTransform,
                "IntelRecommendedSummonRole",
                "NO VERIFIED SOURCE / FEATURED SUMMON NEED HIDDEN",
                mediumFont,
                20f,
                Amber,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -606f),
                new Vector2(-92f, 42f));
            refs.IntelStatus = CreateText(
                plate.rectTransform,
                "IntelStatus",
                "CANONICAL STAGE READ-ONLY / REVIEW SAMPLE",
                semiBoldFont,
                16f,
                Amber,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 42f),
                new Vector2(-92f, 48f));

            Image sideNote = CreateImage(
                root,
                "StageIntelBoundaryCard",
                new Color(InkSoft.r, InkSoft.g, InkSoft.b, 0.90f),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-74f, -32f),
                new Vector2(650f, 560f));
            CreateText(
                sideNote.rectTransform,
                "BoundaryEyebrow",
                "READ-ONLY PREPARATION SAMPLE",
                semiBoldFont,
                17f,
                Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -42f),
                new Vector2(-76f, 34f));
            CreateText(
                sideNote.rectTransform,
                "BoundaryTitle",
                "스테이지 진입 전 정보 구조",
                semiBoldFont,
                32f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -94f),
                new Vector2(-76f, 76f));
            CreateText(
                sideNote.rectTransform,
                "BoundaryBody",
                "로컬 고정 프레젠테이션에서 정보 구조와 선택 흐름만 검토합니다. "
                + "정식 StageCatalog와 SummonSlot 프로필을 읽기 전용으로 투영합니다.",
                mediumFont,
                22f,
                Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -194f),
                new Vector2(-76f, 190f));
            refs.IntelContinueButton = CreateButton(
                sideNote.rectTransform,
                "OpenLoadoutButton",
                "고정 편성 검토",
                semiBoldFont,
                24f,
                Cyan,
                new Color(0.02f, 0.10f, 0.14f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 58f),
                new Vector2(520f, 76f));
        }

        private static void BuildLoadoutOverview(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.LoadoutOverviewPanel.transform as RectTransform;
            CreateImage(
                root,
                "LoadoutBackdrop",
                new Color(0.008f, 0.018f, 0.036f, 0.72f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            refs.LoadoutPilotTitle = CreateText(
                root,
                "LoadoutPilotTitle",
                PilotTitleFallback,
                semiBoldFont,
                38f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(64f, -142f),
                new Vector2(820f, 58f));
            refs.LoadoutPilotBoundary = CreateText(
                root,
                "LoadoutPilotBoundary",
                PilotBoundaryFallback,
                mediumFont,
                19f,
                Amber,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -198f),
                new Vector2(-128f, 42f));

            refs.SlotCards = new[]
            {
                CreateSlotCard(
                    root,
                    "SummonSlot1Card",
                    "SUMMON SLOT 1",
                    "차지 브루저",
                    "저장 EN 전열 돌파",
                    new Vector2(-544f, -40f),
                    mediumFont,
                    semiBoldFont),
                CreateSlotCard(
                    root,
                    "SummonSlot2Card",
                    "SUMMON SLOT 2",
                    "레이저 솔저",
                    "저비용 후열 원거리 압박",
                    new Vector2(0f, -40f),
                    mediumFont,
                    semiBoldFont),
                CreateSlotCard(
                    root,
                    "SummonSlot3Card",
                    "SUMMON SLOT 3",
                    "파이어 드래곤",
                    "고비용 광역 화력",
                    new Vector2(544f, -40f),
                    mediumFont,
                    semiBoldFont)
            };
            refs.SlotCards[0].Icon.sprite = LoadRequired<Sprite>(Slot1IconPath);
            refs.SlotCards[1].Icon.sprite = LoadRequired<Sprite>(Slot2IconPath);
            refs.SlotCards[2].Icon.sprite = LoadRequired<Sprite>(Slot3IconPath);

            refs.LoadoutStatus = CreateText(
                root,
                "LoadoutStatus",
                "CANONICAL RUNTIME PRESET / NOT A STAGE RECOMMENDATION",
                semiBoldFont,
                16f,
                Amber,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 126f),
                new Vector2(780f, 32f));
            refs.LoadoutBackButton = CreateButton(
                root,
                "LoadoutBackButton",
                "‹  STAGE INTEL",
                semiBoldFont,
                18f,
                White,
                new Color(0.025f, 0.055f, 0.085f, 0.96f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(152f, 60f),
                new Vector2(240f, 64f));
            refs.LoadoutReviewButton = CreateButton(
                root,
                "OpenReviewConfirmButton",
                "REVIEW FIXED PRESET  ›",
                semiBoldFont,
                20f,
                Cyan,
                new Color(0.02f, 0.10f, 0.14f, 0.98f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-206f, 60f),
                new Vector2(350f, 68f));
        }

        private static SlotCardRefs CreateSlotCard(
            RectTransform parent,
            string name,
            string eyebrow,
            string title,
            string role,
            Vector2 anchoredPosition,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            Image card = CreateImage(
                parent,
                name,
                Panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPosition,
                new Vector2(500f, 540f));
            CreateImage(
                card.rectTransform,
                "CardAccent",
                Cyan,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                Vector2.zero,
                new Vector2(0f, 4f));
            CreateText(
                card.rectTransform,
                "SlotEyebrow",
                eyebrow,
                semiBoldFont,
                16f,
                CyanSoft,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -38f),
                new Vector2(-50f, 30f));
            Image icon = CreateImage(
                card.rectTransform,
                "SlotIcon",
                Color.white,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -92f),
                new Vector2(136f, 136f));
            icon.preserveAspect = true;
            TMP_Text titleText = CreateText(
                card.rectTransform,
                "SlotTitle",
                title,
                semiBoldFont,
                28f,
                White,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -232f),
                new Vector2(-52f, 50f));
            TMP_Text roleText = CreateText(
                card.rectTransform,
                "SlotRole",
                role,
                mediumFont,
                19f,
                Muted,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -286f),
                new Vector2(-52f, 56f));
            TMP_Text tierText = CreateText(
                card.rectTransform,
                "SlotTier",
                "SELECTED TIER  01",
                semiBoldFont,
                16f,
                Amber,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 130f),
                new Vector2(-52f, 30f));
            Button button = CreateButton(
                card.rectTransform,
                "InspectSlotButton",
                "DETAIL",
                semiBoldFont,
                19f,
                Cyan,
                new Color(0.02f, 0.10f, 0.14f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 52f),
                new Vector2(408f, 66f));
            return new SlotCardRefs(button, icon, titleText, roleText, tierText);
        }

        private static void BuildSummonDetail(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.SummonDetailPanel.transform as RectTransform;
            CreateBlockingScrim(root, "SummonDetailBlockingScrim", 0.70f);
            Image drawer = CreateImage(
                root,
                "SummonDetailDrawer",
                Panel,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-24f, -34f),
                new Vector2(980f, 900f));
            drawer.raycastTarget = true;
            CreateImage(
                drawer.rectTransform,
                "DrawerAccent",
                Cyan,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(3f, 0f),
                new Vector2(6f, 0f));

            refs.DetailBackButton = CreateButton(
                drawer.rectTransform,
                "SummonDetailBackButton",
                "‹  LOADOUT",
                semiBoldFont,
                17f,
                White,
                new Color(0.02f, 0.045f, 0.075f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(118f, -48f),
                new Vector2(200f, 56f));
            refs.DetailIcon = CreateImage(
                drawer.rectTransform,
                "SummonDetailIcon",
                Color.white,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(44f, -116f),
                new Vector2(128f, 128f));
            refs.DetailIcon.preserveAspect = true;
            refs.DetailTitle = CreateText(
                drawer.rectTransform,
                "SummonDetailTitle",
                "SUMMON SLOT",
                semiBoldFont,
                36f,
                White,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(68f, -132f),
                new Vector2(-220f, 58f));
            refs.DetailRole = CreateText(
                drawer.rectTransform,
                "SummonDetailRole",
                "CANONICAL RUNTIME ROLE",
                mediumFont,
                19f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(68f, -186f),
                new Vector2(-220f, 36f));

            CreateText(
                drawer.rectTransform,
                "TierPrompt",
                "REVIEW TIER",
                semiBoldFont,
                15f,
                Muted,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(44f, -256f),
                new Vector2(180f, 28f));
            refs.DetailTier1Button = CreateButton(
                drawer.rectTransform,
                "Tier1Button",
                "TIER 1",
                semiBoldFont,
                17f,
                Cyan,
                new Color(0.02f, 0.10f, 0.14f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(126f, -306f),
                new Vector2(164f, 58f));
            refs.DetailTier2Button = CreateButton(
                drawer.rectTransform,
                "Tier2Button",
                "TIER 2",
                semiBoldFont,
                17f,
                CyanSoft,
                new Color(0.025f, 0.060f, 0.095f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(302f, -306f),
                new Vector2(164f, 58f));
            refs.DetailTier3Button = CreateButton(
                drawer.rectTransform,
                "Tier3Button",
                "TIER 3",
                semiBoldFont,
                17f,
                CyanSoft,
                new Color(0.025f, 0.060f, 0.095f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(478f, -306f),
                new Vector2(164f, 58f));
            refs.DetailSelectedTier = CreateText(
                drawer.rectTransform,
                "DetailSelectedTier",
                "SELECTED TIER  01",
                semiBoldFont,
                16f,
                Amber,
                TextAlignmentOptions.Right,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-44f, -306f),
                new Vector2(280f, 34f));

            refs.DetailStageRole = CreateReadoutBlock(
                drawer.rectTransform,
                "StageRoleReadout",
                "STAGE ROLE",
                "Canonical tier readout",
                -374f,
                mediumFont,
                semiBoldFont);
            refs.DetailPlayerUse = CreateReadoutBlock(
                drawer.rectTransform,
                "PlayerUseReadout",
                "PLAYER USE",
                "Canonical tier readout",
                -520f,
                mediumFont,
                semiBoldFont);
            refs.DetailSummonRead = CreateReadoutBlock(
                drawer.rectTransform,
                "SummonReadReadout",
                "SUMMON READ",
                "Canonical tier readout",
                -666f,
                mediumFont,
                semiBoldFont);
            refs.DetailStatus = CreateText(
                drawer.rectTransform,
                "SummonDetailStatus",
                "CANONICAL PROFILE READ-ONLY / SESSION TIER ONLY",
                semiBoldFont,
                15f,
                Amber,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 34f),
                new Vector2(-88f, 30f));
        }

        private static TMP_Text CreateReadoutBlock(
            RectTransform parent,
            string name,
            string label,
            string value,
            float y,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            Image plate = CreateImage(
                parent,
                name,
                new Color(InkSoft.r, InkSoft.g, InkSoft.b, 0.88f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, y),
                new Vector2(-88f, 128f));
            CreateText(
                plate.rectTransform,
                "Label",
                label,
                semiBoldFont,
                14f,
                CyanSoft,
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(22f, -22f),
                new Vector2(200f, 24f));
            return CreateText(
                plate.rectTransform,
                "Value",
                value,
                mediumFont,
                17f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, -14f),
                new Vector2(-44f, -54f));
        }

        private static void BuildReviewConfirm(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.ReviewConfirmPanel.transform as RectTransform;
            CreateBlockingScrim(root, "ReviewConfirmBlockingScrim", 0.76f);
            Image card = CreateImage(
                root,
                "ReviewConfirmCard",
                Panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -16f),
                new Vector2(960f, 650f));
            card.raycastTarget = true;
            CreateImage(
                card.rectTransform,
                "ConfirmAccent",
                Cyan,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                Vector2.zero,
                new Vector2(0f, 4f));
            refs.ConfirmTitle = CreateText(
                card.rectTransform,
                "ReviewConfirmTitle",
                "고정 프리셋 검토 확인",
                semiBoldFont,
                36f,
                White,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -64f),
                new Vector2(-90f, 58f));
            refs.ConfirmSummary = CreateText(
                card.rectTransform,
                "ReviewConfirmSummary",
                "세 개의 canonical runtime slot과 세션 내 tier 선택을 검토했다는 "
                + "표시만 남깁니다. 저장이나 전투 진입은 수행하지 않습니다.",
                mediumFont,
                21f,
                Muted,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -154f),
                new Vector2(-112f, 140f));
            refs.ConfirmDigest = CreateText(
                card.rectTransform,
                "ReviewSelectionDigest",
                "SummonSlot1:T1  •  SummonSlot2:T1  •  SummonSlot3:T1",
                semiBoldFont,
                18f,
                CyanSoft,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(0f, 0f),
                new Vector2(-100f, 52f));
            refs.ConfirmStatus = CreateText(
                card.rectTransform,
                "ReviewConfirmStatus",
                "SESSION REVIEW ONLY / NO PRODUCT MUTATION",
                semiBoldFont,
                16f,
                Amber,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(0f, -64f),
                new Vector2(-100f, 48f));
            refs.ConfirmBackButton = CreateButton(
                card.rectTransform,
                "ReviewConfirmBackButton",
                "‹  LOADOUT",
                semiBoldFont,
                18f,
                White,
                new Color(0.025f, 0.055f, 0.085f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(88f, 58f),
                new Vector2(300f, 64f));
            refs.ConfirmAcceptButton = CreateButton(
                card.rectTransform,
                "AcceptReviewButton",
                "ACKNOWLEDGE REVIEW",
                semiBoldFont,
                20f,
                Cyan,
                new Color(0.02f, 0.10f, 0.14f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-88f, 58f),
                new Vector2(380f, 68f));
            refs.ConfirmRestartButton = CreateButton(
                card.rectTransform,
                "RestartReviewButton",
                "RESTART LOCAL REVIEW",
                semiBoldFont,
                18f,
                CyanSoft,
                new Color(0.025f, 0.060f, 0.095f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 144f),
                new Vector2(360f, 62f));
        }

        private static void ConfigureController(
            OlympusStagePreparationReviewController controller,
            StagePreparationReviewProfile profile,
            UIStageCatalog stageCatalog,
            ReviewUiRefs ui)
        {
            var bindings = new OlympusStagePreparationReviewController.SlotBinding[
                StagePreparationReviewProfile.RequiredSlotCount];
            for (int i = 0; i < bindings.Length; i++)
            {
                bindings[i] = new OlympusStagePreparationReviewController.SlotBinding();
                SlotCardRefs card = ui.SlotCards[i];
                bindings[i].Configure(
                    $"SummonSlot{i + 1}",
                    card.InspectButton,
                    card.Icon,
                    card.Title,
                    card.Role,
                    card.Tier);
            }

            controller.ConfigureCore(profile, stageCatalog);
            controller.ConfigurePanels(
                ui.StageIntelPanel,
                ui.LoadoutOverviewPanel,
                ui.SummonDetailPanel,
                ui.ReviewConfirmPanel);
            controller.ConfigureIntelView(
                ui.IntelReviewTitle,
                ui.IntelStageCode,
                ui.IntelStageTitle,
                ui.IntelSummary,
                ui.IntelObjective,
                ui.IntelThreatTags,
                ui.IntelRecommendedSummonRole,
                ui.IntelStatus,
                ui.IntelContinueButton);
            controller.ConfigureLoadoutView(
                ui.LoadoutPilotTitle,
                ui.LoadoutPilotBoundary,
                ui.LoadoutStatus,
                ui.LoadoutBackButton,
                ui.LoadoutReviewButton,
                bindings);
            controller.ConfigureDetailView(
                ui.DetailIcon,
                ui.DetailTitle,
                ui.DetailRole,
                ui.DetailSelectedTier,
                ui.DetailStageRole,
                ui.DetailPlayerUse,
                ui.DetailSummonRead,
                ui.DetailStatus,
                ui.DetailTier1Button,
                ui.DetailTier2Button,
                ui.DetailTier3Button,
                ui.DetailBackButton);
            controller.ConfigureConfirmationView(
                ui.ConfirmTitle,
                ui.ConfirmSummary,
                ui.ConfirmDigest,
                ui.ConfirmStatus,
                ui.ConfirmBackButton,
                ui.ConfirmAcceptButton,
                ui.ConfirmRestartButton);
            controller.RestartReview();
        }

        private static List<string> ValidateGeneratedReview(Scene scene)
        {
            var issues = new List<string>();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                issues.Add("PREP-01 review scene is not loaded.");
                return issues;
            }

            if (!string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                issues.Add($"Loaded scene path is `{scene.path}` instead of `{ScenePath}`.");
            }

            if (scene.GetRootGameObjects().Length != 1)
            {
                issues.Add(
                    $"PREP-01 expects one authored scene root; found "
                    + $"{scene.GetRootGameObjects().Length}.");
            }

            if (EditorBuildSettings.scenes.Any(
                    entry => string.Equals(
                        entry.path,
                        ScenePath,
                        StringComparison.Ordinal)))
            {
                issues.Add("PREP-01 review scene must remain absent from Build Settings.");
            }

            StagePreparationReviewProfile profile =
                AssetDatabase.LoadAssetAtPath<StagePreparationReviewProfile>(ProfilePath);
            if (profile == null)
            {
                issues.Add("PREP-01 profile is missing or has the wrong asset type.");
            }
            else if (!profile.TryValidate(out string profileError))
            {
                issues.Add("PREP-01 profile is invalid: " + profileError);
            }
            else
            {
                ValidateProfileComposition(profile, issues);
            }

            UIStageCatalog stageCatalog =
                AssetDatabase.LoadAssetAtPath<UIStageCatalog>(StageCatalogPath);
            ValidateCanonicalProjection(stageCatalog, issues);

            OlympusStagePreparationReviewController[] controllers =
                FindComponentsInScene<OlympusStagePreparationReviewController>(scene);
            if (controllers.Length != 1)
            {
                issues.Add(
                    $"PREP-01 needs exactly one controller; found {controllers.Length}.");
            }
            else
            {
                ValidateController(controllers[0], profile, stageCatalog, issues);
            }

            ValidateSceneInfrastructure(scene, issues);
            ValidateFontReferences(scene, issues);
            ValidateBackground(scene, issues);
            ValidateButtons(scene, issues);
            ValidateBlockingSurfaces(scene, issues);
            ValidateForbiddenOwnership(scene, issues);
            ValidatePanelGeometry(scene, issues);
            ValidateGeneratedYamlRoundTrip(scene, issues);
            return issues;
        }

        private static void ValidateProfileComposition(
            StagePreparationReviewProfile profile,
            List<string> issues)
        {
            if (!string.Equals(
                    profile.name,
                    "DB_UIStagePreparation_OlympusReview",
                    StringComparison.Ordinal)
                || !string.Equals(profile.ReviewId, ReviewId, StringComparison.Ordinal)
                || !string.Equals(
                    profile.CanonicalCatalogEntryId,
                    CanonicalCatalogEntryId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    profile.TitleLocalizationKey,
                    ReviewTitleLocalizationKey,
                    StringComparison.Ordinal)
                || !string.Equals(
                    profile.TitleFallback,
                    ReviewTitleFallback,
                    StringComparison.Ordinal)
                || !string.Equals(
                    profile.PilotPresentationId,
                    PilotPresentationId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    profile.PilotTitleLocalizationKey,
                    PilotTitleLocalizationKey,
                    StringComparison.Ordinal)
                || !string.Equals(
                    profile.PilotTitleFallback,
                    PilotTitleFallback,
                    StringComparison.Ordinal)
                || !string.Equals(
                    profile.PilotBoundaryFallback,
                    PilotBoundaryFallback,
                    StringComparison.Ordinal))
            {
                issues.Add("PREP-01 profile identity or fixed-pilot boundary drifted.");
            }

            SummonSlotActionProfile[] expectedProfiles =
            {
                AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(SummonSlot1ProfilePath),
                AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(SummonSlot2ProfilePath),
                AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(SummonSlot3ProfilePath)
            };
            Sprite[] expectedIcons =
            {
                AssetDatabase.LoadAssetAtPath<Sprite>(Slot1IconPath),
                AssetDatabase.LoadAssetAtPath<Sprite>(Slot2IconPath),
                AssetDatabase.LoadAssetAtPath<Sprite>(Slot3IconPath)
            };
            string[] expectedTitleLocalizationKeys =
            {
                "ui.review.stage_preparation.summon_slot1.title",
                "ui.review.stage_preparation.summon_slot2.title",
                "ui.review.stage_preparation.summon_slot3.title"
            };
            string[] expectedTitles = { "차지 브루저", "레이저 솔저", "파이어 드래곤" };
            string[] expectedRoles =
            {
                "저장 EN 전열 돌파",
                "저비용 후열 원거리 압박",
                "고비용 광역 화력"
            };
            string[] expectedActionIds =
            {
                "SummonSlot1.ChargeBruiser",
                "SummonSlot2.LaserSoldier",
                "SummonSlot3.FireDragon"
            };
            if (profile.SlotCount != StagePreparationReviewProfile.RequiredSlotCount)
            {
                issues.Add(
                    $"PREP-01 needs exactly three slots; found {profile.SlotCount}.");
                return;
            }

            for (int i = 0; i < profile.SlotCount; i++)
            {
                StagePreparationReviewProfile.SlotDefinition slot = profile.GetSlot(i);
                string expectedSlotId = $"SummonSlot{i + 1}";
                if (slot == null
                    || !string.Equals(slot.SlotId, expectedSlotId, StringComparison.Ordinal)
                    || slot.ActionProfile != expectedProfiles[i]
                    || slot.Icon != expectedIcons[i]
                    || !string.Equals(
                        slot.TitleLocalizationKey,
                        expectedTitleLocalizationKeys[i],
                        StringComparison.Ordinal)
                    || !string.Equals(
                        slot.TitleFallback,
                        expectedTitles[i],
                        StringComparison.Ordinal)
                    || !string.Equals(
                        slot.RoleFallback,
                        expectedRoles[i],
                        StringComparison.Ordinal)
                    || !string.Equals(
                        slot.ActionId,
                        expectedActionIds[i],
                        StringComparison.Ordinal)
                    || slot.TierCount != StagePreparationReviewProfile.RequiredTierCount
                    || slot.TierReadoutCount
                        != StagePreparationReviewProfile.RequiredTierCount)
                {
                    issues.Add(
                        $"PREP-01 slot {i} does not match the exact canonical profile/icon "
                        + "contract.");
                    continue;
                }

                for (int tier = 1;
                     tier <= StagePreparationReviewProfile.RequiredTierCount;
                     tier++)
                {
                    if (!slot.TryGetTierReadout(tier, out SummonSlotActionProfile.SummonTierReadout readout)
                        || !readout.HasReadout)
                    {
                        issues.Add(
                            $"PREP-01 `{expectedSlotId}` tier {tier} lacks its canonical "
                            + "readout.");
                    }
                }
            }
        }

        private static void ValidateCanonicalProjection(
            UIStageCatalog stageCatalog,
            List<string> issues)
        {
            if (stageCatalog == null || stageCatalog.StageCount != 1)
            {
                issues.Add(
                    "Canonical UIStageCatalog must remain present with exactly one entry.");
                return;
            }

            if (!stageCatalog.TryCreateRouteProjection(
                    CanonicalCatalogEntryId,
                    UIRouteId.Combat,
                    out UIStageRouteProjection projection,
                    out UIStageRouteProjectionRejectReason rejectReason)
                || !stageCatalog.IsProjectionCurrent(
                    projection,
                    UIRouteId.Combat,
                    out rejectReason))
            {
                issues.Add("Canonical stage projection is unavailable: " + rejectReason);
                return;
            }

            StageBriefingReadModel briefing = projection.Briefing;
            if (!string.Equals(
                    projection.CatalogEntryId,
                    CanonicalCatalogEntryId,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(projection.CanonicalProjectionDigest)
                || string.IsNullOrWhiteSpace(projection.CanonicalBriefingDigest)
                || briefing == null
                || briefing.TitleDisposition != StageBriefingValueDisposition.Present
                || briefing.ObjectiveDisposition != StageBriefingValueDisposition.Present
                || briefing.CombatLessonDisposition != StageBriefingValueDisposition.Present)
            {
                issues.Add(
                    "Canonical projection lacks the exact stage-intel read model required by "
                    + "PREP-01.");
            }

            if (briefing != null
                && (briefing.RecommendedLoadoutDisposition
                        != StageBriefingValueDisposition.NoVerifiedSource
                    || briefing.FeaturedThreatDisposition
                        != StageBriefingValueDisposition.NoVerifiedSource
                    || briefing.FeaturedSummonNeedDisposition
                        != StageBriefingValueDisposition.NoVerifiedSource))
            {
                issues.Add(
                    "PREP-01 requires RecommendedLoadout, FeaturedThreat, and "
                    + "FeaturedSummonNeed to remain NoVerifiedSource and hidden.");
            }

            if (!string.IsNullOrEmpty(projection.ThreatTags)
                || !string.IsNullOrEmpty(projection.RecommendedSummonRole)
                || !string.IsNullOrEmpty(projection.RewardPreview))
            {
                issues.Add(
                    "PREP-01 canonical projection must expose empty threat, loadout, and "
                    + "reward presentation mirrors when their sources are unverified.");
            }
        }

        private static void ValidateController(
            OlympusStagePreparationReviewController controller,
            StagePreparationReviewProfile profile,
            UIStageCatalog stageCatalog,
            List<string> issues)
        {
            var serialized = new SerializedObject(controller);
            serialized.UpdateIfRequiredOrScript();
            ValidateObjectReference(serialized, "profile", profile, issues);
            ValidateObjectReference(serialized, "stageCatalog", stageCatalog, issues);
            string[] requiredReferences =
            {
                "stageIntelPanel",
                "loadoutOverviewPanel",
                "summonDetailPanel",
                "reviewConfirmPanel",
                "intelReviewTitleText",
                "intelStageCodeText",
                "intelStageTitleText",
                "intelSummaryText",
                "intelObjectiveText",
                "intelThreatTagsText",
                "intelRecommendedSummonRoleText",
                "intelStatusText",
                "intelContinueButton",
                "loadoutPilotTitleText",
                "loadoutPilotBoundaryText",
                "loadoutStatusText",
                "loadoutBackButton",
                "loadoutReviewButton",
                "detailIconImage",
                "detailTitleText",
                "detailRoleText",
                "detailSelectedTierText",
                "detailStageRoleText",
                "detailPlayerUseText",
                "detailSummonReadText",
                "detailStatusText",
                "detailTier1Button",
                "detailTier2Button",
                "detailTier3Button",
                "detailBackButton",
                "confirmTitleText",
                "confirmSummaryText",
                "confirmDigestText",
                "confirmStatusText",
                "confirmBackButton",
                "confirmAcceptButton",
                "confirmRestartButton"
            };
            foreach (string propertyName in requiredReferences)
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null || property.objectReferenceValue == null)
                {
                    issues.Add($"PREP-01 controller is missing `{propertyName}`.");
                }
            }

            if (controller.SlotBindingCount
                != StagePreparationReviewProfile.RequiredSlotCount)
            {
                issues.Add(
                    $"PREP-01 controller needs exactly three slot bindings; found "
                    + $"{controller.SlotBindingCount}.");
            }
            else
            {
                for (int i = 0; i < controller.SlotBindingCount; i++)
                {
                    OlympusStagePreparationReviewController.SlotBinding binding =
                        controller.GetSlotBinding(i);
                    if (binding == null
                        || !string.Equals(
                            binding.SlotId,
                            $"SummonSlot{i + 1}",
                            StringComparison.Ordinal)
                        || binding.InspectButton == null
                        || binding.IconImage == null
                        || binding.TitleText == null
                        || binding.RoleText == null
                        || binding.SelectedTierText == null)
                    {
                        issues.Add(
                            $"PREP-01 slot binding {i} is incomplete or out of order.");
                    }
                }
            }

            if (controller.ConfirmationEvent == null
                || controller.ConfirmationEvent.GetPersistentEventCount() != 0)
            {
                issues.Add(
                    "PREP-01 confirmation event must exist with zero persistent callbacks.");
            }
        }

        private static void ValidateSceneInfrastructure(Scene scene, List<string> issues)
        {
            Canvas[] canvases = FindComponentsInScene<Canvas>(scene);
            Camera[] cameras = FindComponentsInScene<Camera>(scene);
            AudioListener[] listeners = FindComponentsInScene<AudioListener>(scene);
            EventSystem[] eventSystems = FindComponentsInScene<EventSystem>(scene);
            UISafeAreaRoot[] safeAreas = FindComponentsInScene<UISafeAreaRoot>(scene);
            UIResponsiveRoot[] responsiveRoots = FindComponentsInScene<UIResponsiveRoot>(scene);

            if (canvases.Length != 1)
            {
                issues.Add($"PREP-01 needs exactly one Canvas; found {canvases.Length}.");
            }
            else
            {
                Canvas canvas = canvases[0];
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null
                    || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                    || scaler.referenceResolution != new Vector2(1920f, 1080f)
                    || scaler.screenMatchMode
                        != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
                    || !Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f)
                    || canvas.renderMode != RenderMode.ScreenSpaceCamera
                    || cameras.Length != 1
                    || canvas.worldCamera != cameras[0]
                    || canvas.GetComponent<GraphicRaycaster>() == null)
                {
                    issues.Add(
                        "PREP-01 Canvas camera/raycaster/1920x1080 mobile-landscape "
                        + "baseline is incomplete.");
                }
            }

            if (cameras.Length != 1
                || listeners.Length != 1
                || cameras.FirstOrDefault()?.tag != "MainCamera")
            {
                issues.Add("PREP-01 needs one MainCamera and one AudioListener.");
            }

            if (eventSystems.Length != 1
                || eventSystems[0].GetComponent<InputSystemUIInputModule>() == null)
            {
                issues.Add("PREP-01 needs one InputSystem EventSystem.");
            }

            if (safeAreas.Length != 1)
            {
                issues.Add($"PREP-01 needs one UISafeAreaRoot; found {safeAreas.Length}.");
            }
            else
            {
                RectTransform safeRect = safeAreas[0].transform as RectTransform;
                var serialized = new SerializedObject(safeAreas[0]);
                serialized.UpdateIfRequiredOrScript();
                bool valid = safeRect != null
                    && serialized.FindProperty("applyOnEnable")?.boolValue == true
                    && serialized.FindProperty("target")?.objectReferenceValue == safeRect
                    && serialized.FindProperty("mode")?.intValue
                        == (int)UISafeAreaMode.InsetsOnly
                    && Mathf.Approximately(
                        serialized.FindProperty("extraInsetPixels")?.floatValue ?? -1f,
                        24f)
                    && safeRect.anchorMin == Vector2.zero
                    && safeRect.anchorMax == Vector2.one
                    && safeRect.offsetMin == Vector2.zero
                    && safeRect.offsetMax == Vector2.zero;
                if (!valid)
                {
                    issues.Add(
                        "PREP-01 safe area must serialize InsetsOnly/24px on a stretched root.");
                }
            }

            if (responsiveRoots.Length != 1)
            {
                issues.Add(
                    $"PREP-01 needs one UIResponsiveRoot; found {responsiveRoots.Length}.");
            }
            else
            {
                var serialized = new SerializedObject(responsiveRoots[0]);
                serialized.UpdateIfRequiredOrScript();
                UnityEngine.Object catalog =
                    serialized.FindProperty("catalog")?.objectReferenceValue;
                string path = AssetDatabase.GetAssetPath(catalog);
                string guid = string.IsNullOrWhiteSpace(path)
                    ? string.Empty
                    : AssetDatabase.AssetPathToGUID(path);
                if (catalog == null
                    || catalog.GetType() != typeof(UIResponsiveLayoutCatalog)
                    || !string.Equals(path, ResponsiveCatalogPath, StringComparison.Ordinal)
                    || !string.Equals(guid, ResponsiveCatalogGuid, StringComparison.Ordinal)
                    || serialized.FindProperty("canvasScaler")?.objectReferenceValue
                        != canvases.FirstOrDefault()?.GetComponent<CanvasScaler>()
                    || serialized.FindProperty("safeAreaRoot")?.objectReferenceValue
                        != safeAreas.FirstOrDefault()
                    || serialized.FindProperty("applyCanvasScaler")?.boolValue != true)
                {
                    issues.Add(
                        "PREP-01 UIResponsiveRoot references are incomplete after scene "
                        + $"roundtrip (catalog path=`{path}`, guid=`{guid}`).");
                }
            }

            TMP_Text boundary = FindComponentsInScene<TMP_Text>(scene).FirstOrDefault(
                text => string.Equals(
                    text.gameObject.name,
                    "ProductBoundary",
                    StringComparison.Ordinal));
            if (boundary == null
                || !string.Equals(
                    boundary.text,
                    OlympusStagePreparationReviewController.PresetBoundaryStatus,
                    StringComparison.Ordinal))
            {
                issues.Add(
                    "PREP-01 must persist the exact `NOT A STAGE RECOMMENDATION` "
                    + "boundary across every panel.");
            }

            TMP_Text[] temporaryBoundaries = FindComponentsInScene<TMP_Text>(scene)
                .Where(text => string.Equals(
                    text.gameObject.name,
                    "ReviewBoundary",
                    StringComparison.Ordinal))
                .ToArray();
            if (temporaryBoundaries.Length != 1
                || !string.Equals(
                    temporaryBoundaries[0].text,
                    TemporaryReviewBoundaryStatus,
                    StringComparison.Ordinal))
            {
                issues.Add(
                    "PREP-01 must contain exactly one ReviewBoundary with the exact "
                    + "`TEMP_DO_NOT_SHIP` marker.");
            }
        }

        private static void ValidateBackground(Scene scene, List<string> issues)
        {
            Sprite expected = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundArtPath);
            Image background = FindComponentsInScene<Image>(scene).FirstOrDefault(
                image => string.Equals(
                    image.gameObject.name,
                    "OlympusChapterHubBackground",
                    StringComparison.Ordinal));
            if (expected == null
                || background == null
                || background.sprite != expected
                || background.GetComponent<AspectRatioFitter>() == null
                || background.GetComponent<AspectRatioFitter>().aspectMode
                    != AspectRatioFitter.AspectMode.EnvelopeParent)
            {
                issues.Add(
                    "PREP-01 must reference the existing ChapterHub background with "
                    + "non-stretching EnvelopeParent behavior.");
            }
        }

        private static void ValidateFontReferences(Scene scene, List<string> issues)
        {
            TMP_FontAsset expectedMedium =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MediumFontPath);
            TMP_FontAsset expectedSemiBold =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SemiBoldFontPath);
            TMP_Text[] texts = FindComponentsInScene<TMP_Text>(scene);
            if (expectedMedium == null || expectedSemiBold == null)
            {
                issues.Add("PREP-01 exact dynamic TMP font dependencies are missing.");
                return;
            }

            int mediumCount = 0;
            int semiBoldCount = 0;
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_FontAsset font = texts[i].font;
                if (font == expectedMedium)
                {
                    mediumCount++;
                }
                else if (font == expectedSemiBold)
                {
                    semiBoldCount++;
                }
                else
                {
                    issues.Add(
                        $"PREP-01 text `{texts[i].name}` references an unexpected TMP font.");
                }
            }

            if (texts.Length == 0 || mediumCount == 0 || semiBoldCount == 0)
            {
                issues.Add(
                    "PREP-01 scene must use both exact Pretendard dynamic TMP font assets.");
            }
        }

        private static void ValidateButtons(Scene scene, List<string> issues)
        {
            Button[] buttons = FindComponentsInScene<Button>(scene);
            if (buttons.Length != 13)
            {
                issues.Add($"PREP-01 expects exactly 13 buttons; found {buttons.Length}.");
            }

            foreach (Button button in buttons)
            {
                Rect rect = (button.transform as RectTransform)?.rect ?? Rect.zero;
                if (rect.width < 48f || rect.height < 48f)
                {
                    issues.Add(
                        $"PREP-01 button `{button.gameObject.name}` is smaller than the "
                        + "48px reference target.");
                }

                if (button.onClick.GetPersistentEventCount() != 0)
                {
                    issues.Add(
                        $"PREP-01 button `{button.gameObject.name}` has a persistent callback.");
                }
            }
        }

        private static void ValidateBlockingSurfaces(Scene scene, List<string> issues)
        {
            string[] blockingNames =
            {
                "SummonDetailBlockingScrim",
                "SummonDetailDrawer",
                "ReviewConfirmBlockingScrim",
                "ReviewConfirmCard"
            };
            foreach (string name in blockingNames)
            {
                Image[] matches = FindComponentsInScene<Image>(scene)
                    .Where(image => string.Equals(
                        image.gameObject.name,
                        name,
                        StringComparison.Ordinal))
                    .ToArray();
                if (matches.Length != 1 || !matches[0].raycastTarget)
                {
                    issues.Add(
                        $"PREP-01 blocking surface `{name}` must exist exactly once and "
                        + "receive raycasts.");
                }
            }
        }

        private static void ValidateForbiddenOwnership(Scene scene, List<string> issues)
        {
            if (FindComponentsInScene<UISceneFlowRouter>(scene).Length != 0
                || FindComponentsInScene<UISceneRouteLoader>(scene).Length != 0
                || FindComponentsInScene<UIPanelRouter>(scene).Length != 0)
            {
                issues.Add("PREP-01 must not contain a router, route loader, or panel router.");
            }

            if (StageRunRuntime.HasActiveContext)
            {
                issues.Add("PREP-01 validation observed an active StageRun context.");
            }

            var allowedMonoBehaviourTypes = new HashSet<Type>
            {
                typeof(OlympusStagePreparationReviewController),
                typeof(UISafeAreaRoot),
                typeof(UIResponsiveRoot),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(Image),
                typeof(TextMeshProUGUI),
                typeof(Button),
                typeof(AspectRatioFitter),
                typeof(EventSystem),
                typeof(InputSystemUIInputModule)
            };
            string[] forbiddenTypeTokens =
            {
                "Network",
                "Http",
                "WebRequest",
                "Persistence",
                "SaveGame",
                "ServiceClient",
                "AccountRepository",
                "Inventory",
                "RuntimeBuilder",
                "HierarchyBuilder"
            };
            foreach (MonoBehaviour behaviour in FindComponentsInScene<MonoBehaviour>(scene))
            {
                if (behaviour == null)
                {
                    issues.Add("PREP-01 contains a missing MonoBehaviour script.");
                    continue;
                }

                Type type = behaviour.GetType();
                string fullName = type.FullName ?? type.Name;
                if (!allowedMonoBehaviourTypes.Contains(type))
                {
                    issues.Add(
                        $"PREP-01 deterministic scene contains non-allowlisted "
                        + $"MonoBehaviour `{fullName}`.");
                }

                if (forbiddenTypeTokens.Any(
                        token => fullName.IndexOf(
                            token,
                            StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    issues.Add($"PREP-01 contains forbidden ownership component `{fullName}`.");
                }
            }

            string[] forbiddenObjectTokens =
            {
                "AccountId",
                "Owned",
                "Inventory",
                "Currency",
                "Reward",
                "Stamina",
                "CombatStart",
                "StartBattle",
                "PowerBadge",
                "Roster",
                "HeroList",
                "TeamType",
                "DefaultTeam"
            };
            foreach (Transform transform in FindComponentsInScene<Transform>(scene))
            {
                if (transform.GetComponents<Component>().Any(component => component == null))
                {
                    issues.Add(
                        $"PREP-01 object `{transform.gameObject.name}` contains a missing "
                        + "script slot.");
                }

                if (forbiddenObjectTokens.Any(
                        token => transform.gameObject.name.IndexOf(
                            token,
                            StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    issues.Add(
                        $"PREP-01 contains forbidden product-data surface "
                        + $"`{transform.gameObject.name}`.");
                }
            }
        }

        private static void ValidatePanelGeometry(Scene scene, List<string> issues)
        {
            string[] panelNames =
            {
                "StageIntelPanel",
                "LoadoutOverviewPanel",
                "SummonDetailPanel",
                "ReviewConfirmPanel"
            };
            foreach (string panelName in panelNames)
            {
                int count = FindComponentsInScene<CanvasGroup>(scene).Count(
                    group => string.Equals(
                        group.gameObject.name,
                        panelName,
                        StringComparison.Ordinal));
                if (count != 1)
                {
                    issues.Add(
                        $"PREP-01 needs exactly one `{panelName}` CanvasGroup; found {count}.");
                }
            }

            RectTransform drawer = FindComponentsInScene<RectTransform>(scene).FirstOrDefault(
                rect => string.Equals(
                    rect.gameObject.name,
                    "SummonDetailDrawer",
                    StringComparison.Ordinal));
            if (drawer == null
                || !Mathf.Approximately(drawer.anchorMin.x, 1f)
                || !Mathf.Approximately(drawer.anchorMax.x, 1f)
                || drawer.rect.width < 900f
                || drawer.rect.width > 1050f)
            {
                issues.Add(
                    "PREP-01 SummonDetailDrawer must remain a right-anchored fixed-width drawer.");
            }

            RectTransform[] cards = FindComponentsInScene<RectTransform>(scene)
                .Where(rect => rect.gameObject.name.StartsWith(
                    "SummonSlot",
                    StringComparison.Ordinal)
                    && rect.gameObject.name.EndsWith("Card", StringComparison.Ordinal))
                .ToArray();
            if (cards.Length != 3)
            {
                issues.Add($"PREP-01 needs three loadout cards; found {cards.Length}.");
            }
            for (int i = 0; i < cards.Length; i++)
            {
                for (int j = i + 1; j < cards.Length; j++)
                {
                    if (CalculateWorldRect(cards[i]).Overlaps(CalculateWorldRect(cards[j])))
                    {
                        issues.Add(
                            $"PREP-01 loadout cards `{cards[i].name}` and `{cards[j].name}` "
                            + "overlap.");
                    }
                }
            }
        }

        private static void ValidateGeneratedYamlRoundTrip(
            Scene scene,
            List<string> issues)
        {
            if (!scene.IsValid()
                || !string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                return;
            }

            string sceneAbsolute = AssetPathToAbsolutePath(ScenePath);
            string profileAbsolute = AssetPathToAbsolutePath(ProfilePath);
            if (!File.Exists(sceneAbsolute) || !File.Exists(profileAbsolute))
            {
                issues.Add("PREP-01 generated scene/profile YAML is unavailable.");
                return;
            }

            string sceneYaml = File.ReadAllText(sceneAbsolute);
            string profileYaml = File.ReadAllText(profileAbsolute);
            string profileGuid = AssetDatabase.AssetPathToGUID(ProfilePath);
            string stageCatalogGuid = AssetDatabase.AssetPathToGUID(StageCatalogPath);
            if (string.IsNullOrWhiteSpace(profileGuid)
                || string.IsNullOrWhiteSpace(stageCatalogGuid)
                || sceneYaml.IndexOf("guid: " + profileGuid, StringComparison.Ordinal) < 0
                || sceneYaml.IndexOf("guid: " + stageCatalogGuid, StringComparison.Ordinal) < 0
                || sceneYaml.IndexOf(
                    "guid: " + ResponsiveCatalogGuid,
                    StringComparison.Ordinal) < 0)
            {
                issues.Add(
                    "PREP-01 scene YAML did not round-trip profile, stage catalog, and "
                    + "responsive catalog references.");
            }

            string[] profileDependencyPaths =
            {
                SummonSlot1ProfilePath,
                SummonSlot2ProfilePath,
                SummonSlot3ProfilePath,
                Slot1IconPath,
                Slot2IconPath,
                Slot3IconPath
            };
            foreach (string path in profileDependencyPaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrWhiteSpace(guid)
                    || profileYaml.IndexOf("guid: " + guid, StringComparison.Ordinal) < 0)
                {
                    issues.Add(
                        $"PREP-01 profile YAML did not round-trip canonical reference `{path}`.");
                }
            }

            if (Regex.IsMatch(sceneYaml, @"[ \t]+(?=\r?$)", RegexOptions.Multiline))
            {
                issues.Add("PREP-01 generated scene YAML retains trailing whitespace.");
            }

            if (Regex.IsMatch(profileYaml, @"[ \t]+(?=\r?$)", RegexOptions.Multiline))
            {
                issues.Add("PREP-01 generated profile YAML retains trailing whitespace.");
            }
        }

        private static Image CreateImage(
            RectTransform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            ConfigureRect(rect, anchorMin, anchorMax, anchoredPosition, sizeDelta);
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
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            ConfigureRect(rect, anchorMin, anchorMax, anchoredPosition, sizeDelta);
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
            float fontSize,
            Color accent,
            Color background,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            Image image = CreateImage(
                parent,
                name,
                background,
                anchorMin,
                anchorMax,
                anchoredPosition,
                sizeDelta);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.72f, 0.86f, 0.92f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.36f, 0.42f, 0.48f, 0.65f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;
            CreateText(
                image.rectTransform,
                "Label",
                label,
                font,
                fontSize,
                accent,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            return button;
        }

        private static CanvasGroup CreateFlowGroup(RectTransform parent, string name)
        {
            GameObject gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasGroup));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            return gameObject.GetComponent<CanvasGroup>();
        }

        private static void CreateBlockingScrim(
            RectTransform parent,
            string name,
            float alpha)
        {
            Image image = CreateImage(
                parent,
                name,
                new Color(0.002f, 0.008f, 0.016f, Mathf.Clamp01(alpha)),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            image.raycastTarget = true;
        }

        private static void SetGroup(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            GameObject gameObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            gameObject.transform.SetParent(parent, false);
        }

        private static void ConfigureRuntimeSafeArea(
            UISafeAreaRoot safeAreaRoot,
            RectTransform target,
            UISafeAreaMode mode,
            float extraInsetPixels)
        {
            var serialized = new SerializedObject(safeAreaRoot);
            serialized.FindProperty("applyOnEnable").boolValue = true;
            serialized.FindProperty("target").objectReferenceValue = target;
            serialized.FindProperty("mode").intValue = (int)mode;
            serialized.FindProperty("extraInsetPixels").floatValue =
                Mathf.Max(0f, extraInsetPixels);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Stretch(target);
            EditorUtility.SetDirty(safeAreaRoot);
        }

        private static void ConfigureResponsiveRoot(
            UIResponsiveRoot responsiveRoot,
            CanvasScaler scaler,
            UISafeAreaRoot safeAreaRoot)
        {
            if (responsiveRoot == null || scaler == null || safeAreaRoot == null)
            {
                throw new InvalidOperationException(
                    "PREP-01 responsive root, CanvasScaler, and safe area are required.");
            }

            UIResponsiveLayoutCatalog catalog =
                AssetDatabase.LoadAssetAtPath<UIResponsiveLayoutCatalog>(
                    ResponsiveCatalogPath);
            UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(
                ResponsiveCatalogPath);
            string guid = AssetDatabase.AssetPathToGUID(ResponsiveCatalogPath);
            if (catalog == null
                || mainAsset != catalog
                || mainAsset.GetType() != typeof(UIResponsiveLayoutCatalog)
                || !AssetDatabase.Contains(catalog)
                || !string.Equals(
                    AssetDatabase.GetAssetPath(catalog),
                    ResponsiveCatalogPath,
                    StringComparison.Ordinal)
                || !string.Equals(guid, ResponsiveCatalogGuid, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"PREP-01 responsive catalog is missing, wrong-type, or not protected at "
                    + $"`{ResponsiveCatalogPath}` / `{ResponsiveCatalogGuid}`.");
            }

            SetResponsiveRootPrivateField(responsiveRoot, "catalog", catalog);
            SetResponsiveRootPrivateField(responsiveRoot, "canvasScaler", scaler);
            SetResponsiveRootPrivateField(responsiveRoot, "safeAreaRoot", safeAreaRoot);
            SetResponsiveRootPrivateField(responsiveRoot, "breakpointText", null);
            SetResponsiveRootPrivateField(responsiveRoot, "applyCanvasScaler", true);

            var serialized = new SerializedObject(responsiveRoot);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty catalogProperty = serialized.FindProperty("catalog");
            SerializedProperty scalerProperty = serialized.FindProperty("canvasScaler");
            SerializedProperty safeProperty = serialized.FindProperty("safeAreaRoot");
            SerializedProperty breakpointProperty = serialized.FindProperty("breakpointText");
            SerializedProperty applyProperty = serialized.FindProperty("applyCanvasScaler");
            if (catalogProperty == null
                || scalerProperty == null
                || safeProperty == null
                || breakpointProperty == null
                || applyProperty == null)
            {
                throw new InvalidOperationException(
                    "PREP-01 UIResponsiveRoot serialized schema is incomplete.");
            }

            catalogProperty.objectReferenceValue = catalog;
            scalerProperty.objectReferenceValue = scaler;
            safeProperty.objectReferenceValue = safeAreaRoot;
            breakpointProperty.objectReferenceValue = null;
            applyProperty.boolValue = true;
            serialized.ApplyModifiedProperties();

            SetResponsiveRootPrivateField(responsiveRoot, "catalog", catalog);
            SetResponsiveRootPrivateField(responsiveRoot, "canvasScaler", scaler);
            SetResponsiveRootPrivateField(responsiveRoot, "safeAreaRoot", safeAreaRoot);
            SetResponsiveRootPrivateField(responsiveRoot, "breakpointText", null);
            SetResponsiveRootPrivateField(responsiveRoot, "applyCanvasScaler", true);
            EditorUtility.SetDirty(responsiveRoot);
            if (responsiveRoot.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(responsiveRoot.gameObject.scene);
            }

            ValidateResponsiveRootReferencesOrThrow(
                responsiveRoot,
                catalog,
                scaler,
                safeAreaRoot,
                "immediately after configuration");
        }

        private static void SetResponsiveRootPrivateField(
            UIResponsiveRoot responsiveRoot,
            string fieldName,
            object value)
        {
            FieldInfo field = typeof(UIResponsiveRoot).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"PREP-01 UIResponsiveRoot private field `{fieldName}` is unavailable.");
            }

            field.SetValue(responsiveRoot, value);
        }

        private static void ValidateResponsiveRootReferencesOrThrow(
            UIResponsiveRoot responsiveRoot,
            UIResponsiveLayoutCatalog catalog,
            CanvasScaler scaler,
            UISafeAreaRoot safeAreaRoot,
            string checkpoint)
        {
            var serialized = new SerializedObject(responsiveRoot);
            serialized.UpdateIfRequiredOrScript();
            var issues = new List<string>();
            UnityEngine.Object serializedCatalog =
                serialized.FindProperty("catalog")?.objectReferenceValue;
            string path = AssetDatabase.GetAssetPath(serializedCatalog);
            string guid = string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : AssetDatabase.AssetPathToGUID(path);
            if (serializedCatalog != catalog
                || serializedCatalog?.GetType() != typeof(UIResponsiveLayoutCatalog)
                || !string.Equals(path, ResponsiveCatalogPath, StringComparison.Ordinal)
                || !string.Equals(guid, ResponsiveCatalogGuid, StringComparison.Ordinal))
            {
                issues.Add($"catalog(type/path/guid={serializedCatalog?.GetType().Name}/{path}/{guid})");
            }

            if (serialized.FindProperty("canvasScaler")?.objectReferenceValue != scaler)
            {
                issues.Add("canvasScaler");
            }

            if (serialized.FindProperty("safeAreaRoot")?.objectReferenceValue != safeAreaRoot)
            {
                issues.Add("safeAreaRoot");
            }

            if (serialized.FindProperty("applyCanvasScaler")?.boolValue != true)
            {
                issues.Add("applyCanvasScaler");
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    $"PREP-01 responsive references are incomplete {checkpoint}: "
                    + string.Join(", ", issues));
            }
        }

        private static void ConfigureRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
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
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static Rect CalculateWorldRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return Rect.MinMaxRect(
                corners[0].x,
                corners[0].y,
                corners[2].x,
                corners[2].y);
        }

        private static void ValidateObjectReference(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object expected,
            List<string> issues)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue != expected)
            {
                issues.Add($"PREP-01 controller `{propertyName}` is missing or stale.");
            }
        }

        private static T[] FindComponentsInScene<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(includeInactive: true));
            }

            return results.ToArray();
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                UnityEngine.Object occupied = AssetDatabase.LoadMainAssetAtPath(path);
                string occupancy = occupied == null
                    ? "missing or unimportable"
                    : $"occupied by {occupied.GetType().Name}";
                throw new InvalidOperationException(
                    $"Required {typeof(T).Name} at `{path}` is {occupancy}; refusing "
                    + "fallback or overwrite.");
            }

            return asset;
        }

        private static void EnsureAssetFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = PathParent(path);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureAssetFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static string PathParent(string path)
        {
            return Path.GetDirectoryName(path)?.Replace('\\', '/') ?? string.Empty;
        }

        private static void NormalizeGeneratedYamlWhitespace(string assetPath)
        {
            string absolutePath = AssetPathToAbsolutePath(assetPath);
            string source = File.ReadAllText(absolutePath);
            string normalized = Regex.Replace(
                source,
                @"[ \t]+(?=\r?$)",
                string.Empty,
                RegexOptions.Multiline);
            if (string.Equals(source, normalized, StringComparison.Ordinal))
            {
                return;
            }

            File.WriteAllText(
                absolutePath,
                normalized,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceSynchronousImport);
        }

        private static Dictionary<string, CanonicalAssetFingerprint>
            CaptureCanonicalFingerprints()
        {
            var result = new Dictionary<string, CanonicalAssetFingerprint>(
                StringComparer.Ordinal);
            foreach (string assetPath in CanonicalAssetsThatMustRemainUntouched)
            {
                CaptureCanonicalFingerprint(result, assetPath);
                CaptureCanonicalFingerprint(result, assetPath + ".meta");
            }

            string[] missing = result
                .Where(pair => !pair.Value.Exists)
                .Select(pair => pair.Key)
                .ToArray();
            if (missing.Length > 0)
            {
                throw new FileNotFoundException(
                    "PREP-01 canonical SHA-256 boundary is incomplete:\n- "
                    + string.Join("\n- ", missing));
            }

            return result;
        }

        private static void CaptureCanonicalFingerprint(
            Dictionary<string, CanonicalAssetFingerprint> result,
            string assetPath)
        {
            result[assetPath] = CanonicalAssetFingerprint.Capture(
                AssetPathToAbsolutePath(assetPath));
        }

        private static void AppendCanonicalFingerprintIssues(
            Dictionary<string, CanonicalAssetFingerprint> before,
            List<string> issues)
        {
            foreach (KeyValuePair<string, CanonicalAssetFingerprint> pair in before)
            {
                CanonicalAssetFingerprint after = CanonicalAssetFingerprint.Capture(
                    AssetPathToAbsolutePath(pair.Key));
                if (!after.Exists)
                {
                    issues.Add(
                        $"Canonical boundary file disappeared during PREP-01 setup/verification: "
                        + $"`{pair.Key}`.");
                }
                else if (pair.Value.Length != after.Length
                    || !string.Equals(
                        pair.Value.Sha256,
                        after.Sha256,
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Canonical boundary file changed during PREP-01 setup/verification: "
                        + $"`{pair.Key}`.");
                }
            }
        }

        private static void AppendDigestField(StringBuilder builder, string assetPath)
        {
            CanonicalAssetFingerprint fingerprint = CanonicalAssetFingerprint.Capture(
                AssetPathToAbsolutePath(assetPath));
            if (!fingerprint.Exists)
            {
                throw new FileNotFoundException(
                    $"PREP-01 canonical digest file is missing: `{assetPath}`.");
            }

            builder.Append(assetPath);
            builder.Append('|');
            builder.Append(fingerprint.Length);
            builder.Append('|');
            builder.Append(fingerprint.Sha256);
            builder.Append('\n');
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath)
                ?? throw new InvalidOperationException("Unity project root is unavailable.");
            return Path.GetFullPath(
                Path.Combine(
                    projectRoot,
                    assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string ToLowerHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static void ThrowIfIssues(string heading, List<string> issues)
        {
            if (issues.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                heading + ":\n- " + string.Join("\n- ", issues));
        }

        private sealed class ReviewUiRefs
        {
            public UISafeAreaRoot SafeAreaRoot;
            public CanvasGroup StageIntelPanel;
            public CanvasGroup LoadoutOverviewPanel;
            public CanvasGroup SummonDetailPanel;
            public CanvasGroup ReviewConfirmPanel;

            public TMP_Text IntelReviewTitle;
            public TMP_Text IntelStageCode;
            public TMP_Text IntelStageTitle;
            public TMP_Text IntelSummary;
            public TMP_Text IntelObjective;
            public TMP_Text IntelThreatTags;
            public TMP_Text IntelRecommendedSummonRole;
            public TMP_Text IntelStatus;
            public Button IntelContinueButton;

            public TMP_Text LoadoutPilotTitle;
            public TMP_Text LoadoutPilotBoundary;
            public TMP_Text LoadoutStatus;
            public Button LoadoutBackButton;
            public Button LoadoutReviewButton;
            public SlotCardRefs[] SlotCards;

            public Image DetailIcon;
            public TMP_Text DetailTitle;
            public TMP_Text DetailRole;
            public TMP_Text DetailSelectedTier;
            public TMP_Text DetailStageRole;
            public TMP_Text DetailPlayerUse;
            public TMP_Text DetailSummonRead;
            public TMP_Text DetailStatus;
            public Button DetailTier1Button;
            public Button DetailTier2Button;
            public Button DetailTier3Button;
            public Button DetailBackButton;

            public TMP_Text ConfirmTitle;
            public TMP_Text ConfirmSummary;
            public TMP_Text ConfirmDigest;
            public TMP_Text ConfirmStatus;
            public Button ConfirmBackButton;
            public Button ConfirmAcceptButton;
            public Button ConfirmRestartButton;
        }

        private sealed class SlotCardRefs
        {
            public SlotCardRefs(
                Button inspectButton,
                Image icon,
                TMP_Text title,
                TMP_Text role,
                TMP_Text tier)
            {
                InspectButton = inspectButton;
                Icon = icon;
                Title = title;
                Role = role;
                Tier = tier;
            }

            public Button InspectButton { get; }
            public Image Icon { get; }
            public TMP_Text Title { get; }
            public TMP_Text Role { get; }
            public TMP_Text Tier { get; }
        }

        private readonly struct CanonicalAssetFingerprint
        {
            private CanonicalAssetFingerprint(bool exists, long length, string sha256)
            {
                Exists = exists;
                Length = length;
                Sha256 = sha256 ?? string.Empty;
            }

            public bool Exists { get; }
            public long Length { get; }
            public string Sha256 { get; }

            public static CanonicalAssetFingerprint Capture(string absolutePath)
            {
                if (!File.Exists(absolutePath))
                {
                    return new CanonicalAssetFingerprint(false, 0L, string.Empty);
                }

                byte[] bytes = File.ReadAllBytes(absolutePath);
                using SHA256 sha = SHA256.Create();
                string digest = ToLowerHex(sha.ComputeHash(bytes));
                return new CanonicalAssetFingerprint(true, bytes.LongLength, digest);
            }
        }
    }
}
