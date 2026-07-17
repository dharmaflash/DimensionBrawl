using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.ChapterHubReview;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.ChapterHubReview
{
    /// <summary>
    /// Builds an authored, review-only chapter hub sample. It deliberately stays outside Build
    /// Settings and never owns a route request, StageRun, progression, reward, or account state.
    /// </summary>
    public static class OlympusChapterHubReviewSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Review/UI_OlympusChapterHubReview.unity";
        public const string ProfilePath =
            "Assets/_Game/DesignData/UI/Review/DB_UIChapterHub_OlympusReview.asset";

        public const string ChapterId = "review.chapter.olympus";
        public const string CanonicalStageId = "review.stage.memory-corridor";
        public const string InProductionStageId = "review.stage.broken-dock";
        public const string AnnouncedStageId = "review.stage.gate-depths";
        public const string CanonicalCatalogEntryId = "story_v1_training_route";

        private const string StageCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string BackgroundArtPath =
            "Assets/_Game/UI/ChapterHubReview/Art/BG_OlympusChapterHub_Review.png";
        private const string MediumFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_Medium_Dynamic.asset";
        private const string SemiBoldFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_SemiBold_Dynamic.asset";

        private static readonly string[] CanonicalAssetsThatMustRemainUntouched =
        {
            StageCatalogPath,
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset",
            "Assets/_Game/DesignData/UI/DB_UIScreenCatalog.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusStationCombat.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusInvasionTutorialStationRun.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusInvasion.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusInvasion.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusInvasion.asset",
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/Scenes/UI/UI_StageSelect.unity",
            "Assets/_Game/Scenes/UI/UI_StageClear.unity",
            "Assets/_Game/UI/StageSelect/PF_UI_StageSelectScreen.prefab"
        };

        private static readonly Color Ink = new Color(0.012f, 0.022f, 0.047f, 0.98f);
        private static readonly Color InkSoft = new Color(0.028f, 0.052f, 0.092f, 0.94f);
        private static readonly Color Panel = new Color(0.035f, 0.070f, 0.118f, 0.95f);
        private static readonly Color PanelSoft = new Color(0.060f, 0.105f, 0.165f, 0.90f);
        private static readonly Color Cyan = new Color(0.25f, 0.90f, 1.00f, 1f);
        private static readonly Color CyanSoft = new Color(0.38f, 0.70f, 0.84f, 1f);
        private static readonly Color Amber = new Color(1.00f, 0.67f, 0.26f, 1f);
        private static readonly Color Violet = new Color(0.72f, 0.60f, 1.00f, 1f);
        private static readonly Color White = new Color(0.94f, 0.98f, 1.00f, 1f);
        private static readonly Color Muted = new Color(0.57f, 0.68f, 0.79f, 1f);

        [MenuItem("Tools/DimensionBrawl/Review/Setup Olympus Chapter Hub Review")]
        public static void SetupMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Setup();
        }

        [MenuItem("Tools/DimensionBrawl/Review/Validate Olympus Chapter Hub Review")]
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
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> issues = ValidateGeneratedReview(scene);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Olympus chapter hub review validation failed:\n- "
                    + string.Join("\n- ", issues));
            }

            Debug.Log(
                "Olympus chapter hub review validation passed. "
                + "The scene remains review-only and outside Build Settings.");
        }

        private static void Setup()
        {
            Dictionary<string, CanonicalAssetFingerprint> canonicalFingerprints =
                CaptureCanonicalFingerprints();
            EnsureAssetFolder(PathParent(ScenePath));
            EnsureAssetFolder(PathParent(ProfilePath));
            EnsureAssetFolder(PathParent(BackgroundArtPath));

            ChapterHubReviewProfile profile = EnsureProfile();
            UIStageCatalog stageCatalog = LoadRequired<UIStageCatalog>(StageCatalogPath);
            TMP_FontAsset mediumFont = LoadRequired<TMP_FontAsset>(MediumFontPath);
            TMP_FontAsset semiBoldFont = LoadRequired<TMP_FontAsset>(SemiBoldFontPath);
            Sprite background = EnsureBackgroundSprite();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("OlympusChapterHubReview_Root");
            SceneManager.MoveGameObjectToScene(root, scene);

            Camera reviewCamera = CreateReviewCamera(root.transform);
            Canvas canvas = CreateCanvas(root.transform, reviewCamera);
            ReviewUiRefs ui = CreateReviewUi(
                canvas.GetComponent<RectTransform>(),
                mediumFont,
                semiBoldFont,
                background);
            EnsureEventSystem(root.transform);

            GameObject controllerObject = new GameObject(
                "ChapterHubReviewFlow",
                typeof(OlympusChapterHubReviewController));
            controllerObject.transform.SetParent(root.transform, false);
            OlympusChapterHubReviewController controller =
                controllerObject.GetComponent<OlympusChapterHubReviewController>();
            ConfigureController(controller, profile, stageCatalog, ui);

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(canvas);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save review scene `{ScenePath}`.");
            }

            AssetDatabase.SaveAssetIfDirty(profile);
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> issues = ValidateGeneratedReview(scene);
            AppendCanonicalFingerprintIssues(canonicalFingerprints, issues);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "Olympus chapter hub review setup failed:\n- "
                    + string.Join("\n- ", issues));
            }

            Debug.Log(
                $"Created `{ScenePath}` with one canonical node, one InProduction node, "
                + "one Announced node, and no product route or StageRun mutation.");
        }

        private static ChapterHubReviewProfile EnsureProfile()
        {
            ChapterHubReviewProfile profile =
                AssetDatabase.LoadAssetAtPath<ChapterHubReviewProfile>(ProfilePath);
            if (profile == null)
            {
                UnityEngine.Object existingAsset =
                    AssetDatabase.LoadMainAssetAtPath(ProfilePath);
                if (existingAsset != null
                    || File.Exists(AssetPathToAbsolutePath(ProfilePath)))
                {
                    throw new InvalidOperationException(
                        $"Review profile path `{ProfilePath}` is occupied by an unexpected or "
                        + "unimportable asset; refusing to overwrite it.");
                }

                profile = ScriptableObject.CreateInstance<ChapterHubReviewProfile>();
                profile.name = "DB_UIChapterHub_OlympusReview";
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            var chapter = new ChapterHubReviewProfile.ChapterDefinition(
                ChapterId,
                "EP 01",
                "ui.review.chapter.olympus.title",
                "올림포스 균열");
            ChapterHubReviewProfile.StageDefinition[] stages =
            {
                new ChapterHubReviewProfile.StageDefinition(
                    CanonicalStageId,
                    ChapterId,
                    "01-01",
                    "ui.review.stage.memory-corridor.title",
                    "기억의 회랑",
                    new Vector2(0.32f, 0.53f),
                    ChapterHubReviewContentStatus.CanonicalPlayable,
                    CanonicalCatalogEntryId),
                new ChapterHubReviewProfile.StageDefinition(
                    InProductionStageId,
                    ChapterId,
                    "01-02",
                    "ui.review.stage.broken-dock.title",
                    "파손된 도크",
                    new Vector2(0.55f, 0.47f),
                    ChapterHubReviewContentStatus.InProduction),
                new ChapterHubReviewProfile.StageDefinition(
                    AnnouncedStageId,
                    ChapterId,
                    "01-03",
                    "ui.review.stage.gate-depths.title",
                    "게이트 심부",
                    new Vector2(0.74f, 0.38f),
                    ChapterHubReviewContentStatus.Announced)
            };
            profile.Configure(new[] { chapter }, stages);
            if (!profile.TryValidate(out string error))
            {
                throw new InvalidOperationException("Chapter hub review profile invalid: " + error);
            }

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static Sprite EnsureBackgroundSprite()
        {
            AssetDatabase.ImportAsset(BackgroundArtPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(BackgroundArtPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Generated background `{BackgroundArtPath}` has no TextureImporter.");
            }

            bool changed = importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || importer.wrapMode != TextureWrapMode.Clamp
                || importer.mipmapEnabled
                || !importer.sRGBTexture
                || importer.alphaIsTransparency
                || importer.maxTextureSize != 2048;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.maxTextureSize = 2048;
            if (changed)
            {
                importer.SaveAndReimport();
            }

            return LoadRequired<Sprite>(BackgroundArtPath);
        }

        private static Camera CreateReviewCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject(
                "ReviewCamera",
                typeof(Camera),
                typeof(AudioListener));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
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
            GameObject canvasObject = new GameObject(
                "ChapterHubReviewCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
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
                "ChapterMapBackground",
                Color.white,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            backgroundImage.sprite = background;
            backgroundImage.preserveAspect = false;
            AspectRatioFitter fitter = backgroundImage.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 1672f / 941f;
            CreateImage(
                canvasRect,
                "BackgroundWash",
                new Color(0.005f, 0.015f, 0.035f, 0.43f),
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
            ConfigureRuntimeSafeArea(
                safeObject.GetComponent<UISafeAreaRoot>(),
                safeRect,
                UISafeAreaMode.InsetsOnly,
                24f);

            CreateImage(
                safeRect,
                "TopRail",
                new Color(0.015f, 0.035f, 0.065f, 0.94f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -54f),
                new Vector2(0f, 108f));
            CreateText(
                safeRect,
                "ProductBreadcrumb",
                "DIMENSION BRAWL  /  OPERATIONS ARCHIVE",
                semiBoldFont,
                25f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(38f, -54f),
                new Vector2(700f, 52f));
            CreateText(
                safeRect,
                "ReviewBoundary",
                "CHUB-01  •  REVIEW SAMPLE  •  TEMP_DO_NOT_SHIP",
                mediumFont,
                18f,
                Amber,
                TextAlignmentOptions.MidlineRight,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-38f, -54f),
                new Vector2(700f, 48f));

            refs.ChapterHubPanel = CreateFlowGroup(safeRect, "ChapterHubPanel");
            refs.StageMapPanel = CreateFlowGroup(safeRect, "StageMapPanel");
            refs.StageDetailPanel = CreateFlowGroup(safeRect, "StageDetailPanel");
            refs.ReviewConfirmPanel = CreateFlowGroup(safeRect, "ReviewConfirmPanel");

            BuildChapterHub(refs, mediumFont, semiBoldFont);
            BuildStageMap(refs, mediumFont, semiBoldFont);
            BuildStageDetail(refs, mediumFont, semiBoldFont);
            BuildReviewConfirm(refs, mediumFont, semiBoldFont);
            return refs;
        }

        private static void BuildChapterHub(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.ChapterHubPanel.GetComponent<RectTransform>();
            CreateImage(
                root,
                "HubPlate",
                new Color(0.018f, 0.043f, 0.078f, 0.84f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -32f),
                new Vector2(1660f, 760f));
            refs.HubEpisode = CreateText(
                root,
                "HubEpisode",
                "EP 01",
                semiBoldFont,
                22f,
                Cyan,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-720f, 272f),
                new Vector2(360f, 38f));
            refs.HubTitle = CreateText(
                root,
                "HubTitle",
                "올림포스 균열",
                semiBoldFont,
                58f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-500f, 210f),
                new Vector2(800f, 84f));
            refs.HubStatus = CreateText(
                root,
                "HubStatus",
                "REVIEW SAMPLE / LOCAL BROWSE",
                mediumFont,
                18f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-600f, 150f),
                new Vector2(600f, 38f));

            ChapterCardRefs activeCard = CreateChapterCard(
                root,
                "OlympusChapterCard",
                new Vector2(-340f, -60f),
                new Vector2(760f, 360f),
                Cyan,
                mediumFont,
                semiBoldFont,
                true);
            activeCard.Episode.text = "EP 01";
            activeCard.Title.text = "올림포스 균열";
            activeCard.Status.text = "OPEN OPERATIONS MAP";
            refs.ChapterBindings = new[]
            {
                new OlympusChapterHubReviewController.ChapterButtonBinding(
                    ChapterId,
                    activeCard.Button,
                    activeCard.Group,
                    activeCard.Episode,
                    activeCard.Title)
            };

            ChapterCardRefs futureCard = CreateChapterCard(
                root,
                "FutureArchiveCard",
                new Vector2(480f, -80f),
                new Vector2(570f, 320f),
                Violet,
                mediumFont,
                semiBoldFont,
                false);
            futureCard.Episode.text = "EP --";
            futureCard.Title.text = "미개척 기록";
            futureCard.Status.text = "ARCHIVE SLOT / NOT AUTHORED";
            futureCard.Button.interactable = false;
            CreateText(
                root,
                "HubBoundaryNote",
                "정적 콘텐츠 정의만 표시합니다. 계정 진행도·재화·보상·일정 상태는 연결하지 않았습니다.",
                mediumFont,
                20f,
                Muted,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -298f),
                new Vector2(1450f, 44f));
        }

        private static void BuildStageMap(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.StageMapPanel.GetComponent<RectTransform>();
            refs.MapBackButton = CreateButton(
                root,
                "MapBackButton",
                "‹  CHAPTER HUB",
                semiBoldFont,
                21f,
                White,
                Panel,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(20f, -194f),
                new Vector2(260f, 64f));
            refs.MapEpisode = CreateText(
                root,
                "MapEpisode",
                "EP 01",
                semiBoldFont,
                20f,
                Cyan,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(360f, -140f),
                new Vector2(240f, 40f));
            refs.MapTitle = CreateText(
                root,
                "MapChapterTitle",
                "올림포스 균열",
                semiBoldFont,
                42f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(360f, -188f),
                new Vector2(720f, 72f));
            refs.MapStatus = CreateText(
                root,
                "MapStatus",
                "SELECT AN AUTHORED REVIEW NODE",
                mediumFont,
                17f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(360f, -232f),
                new Vector2(600f, 36f));

            CreateImage(
                root,
                "RouteBandA",
                new Color(Cyan.r, Cyan.g, Cyan.b, 0.38f),
                new Vector2(0.32f, 0.50f),
                new Vector2(0.32f, 0.50f),
                new Vector2(210f, -8f),
                new Vector2(430f, 4f));
            Image routeBandB = CreateImage(
                root,
                "RouteBandB",
                new Color(Amber.r, Amber.g, Amber.b, 0.33f),
                new Vector2(0.55f, 0.44f),
                new Vector2(0.55f, 0.44f),
                new Vector2(170f, -6f),
                new Vector2(360f, 4f));
            routeBandB.rectTransform.localEulerAngles = new Vector3(0f, 0f, -11f);

            StageNodeRefs canonical = CreateStageNode(
                root,
                "CanonicalStageNode",
                new Vector2(0.32f, 0.53f),
                Cyan,
                mediumFont,
                semiBoldFont);
            StageNodeRefs inProduction = CreateStageNode(
                root,
                "InProductionStageNode",
                new Vector2(0.55f, 0.47f),
                Amber,
                mediumFont,
                semiBoldFont);
            StageNodeRefs announced = CreateStageNode(
                root,
                "AnnouncedStageNode",
                new Vector2(0.74f, 0.38f),
                Violet,
                mediumFont,
                semiBoldFont);
            refs.StageNodeBindings = new[]
            {
                canonical.ToBinding(CanonicalStageId),
                inProduction.ToBinding(InProductionStageId),
                announced.ToBinding(AnnouncedStageId)
            };
            CreateText(
                root,
                "MapLegend",
                "CYAN  CANONICAL DATA    •    AMBER  IN PRODUCTION    •    VIOLET  ANNOUNCED",
                mediumFont,
                17f,
                Muted,
                TextAlignmentOptions.Midline,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 58f),
                new Vector2(1180f, 34f));
        }

        private static void BuildStageDetail(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.StageDetailPanel.GetComponent<RectTransform>();
            CreateImage(
                root,
                "DetailMapShade",
                new Color(0.008f, 0.020f, 0.040f, 0.42f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            Image panelImage = CreateImage(
                root,
                "StageDetailPlate",
                Panel,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-390f, -22f),
                new Vector2(720f, 850f));
            RectTransform panel = panelImage.rectTransform;
            refs.DetailBackButton = CreateButton(
                panel,
                "DetailBackButton",
                "‹  OPERATIONS MAP",
                semiBoldFont,
                18f,
                White,
                InkSoft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(145f, -74f),
                new Vector2(250f, 56f));
            refs.DetailBackButton.GetComponent<RectTransform>().pivot =
                new Vector2(0.5f, 0.5f);
            refs.DetailStageCode = CreateText(
                panel,
                "DetailStageCode",
                "01-01",
                semiBoldFont,
                21f,
                Cyan,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(54f, -108f),
                new Vector2(220f, 38f));
            refs.DetailTitle = CreateText(
                panel,
                "DetailTitle",
                "기억의 회랑",
                semiBoldFont,
                42f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(335f, -181f),
                new Vector2(610f, 70f));
            refs.DetailTitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            refs.DetailStatus = CreateText(
                panel,
                "DetailStatus",
                OlympusChapterHubReviewController.CanonicalReviewStatus,
                mediumFont,
                16f,
                Amber,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(335f, -233f),
                new Vector2(610f, 34f));
            refs.DetailStatus.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            DetailRowRefs objective = CreateDetailRow(
                panel,
                "ObjectiveRow",
                "OBJECTIVE",
                new Vector2(0f, 100f),
                new Vector2(630f, 108f),
                mediumFont,
                semiBoldFont);
            DetailRowRefs lesson = CreateDetailRow(
                panel,
                "CombatLessonRow",
                "COMBAT LESSON",
                new Vector2(0f, -30f),
                new Vector2(630f, 136f),
                mediumFont,
                semiBoldFont);
            DetailRowRefs story = CreateDetailRow(
                panel,
                "StoryRow",
                "STORY ENTRY",
                new Vector2(0f, -155f),
                new Vector2(630f, 84f),
                mediumFont,
                semiBoldFont);
            DetailRowRefs segment = CreateDetailRow(
                panel,
                "SegmentRow",
                "ROUTE SEGMENTS",
                new Vector2(0f, -265f),
                new Vector2(630f, 104f),
                mediumFont,
                semiBoldFont);
            refs.DetailObjectiveRow = objective.Root;
            refs.DetailObjective = objective.Body;
            refs.DetailCombatLessonRow = lesson.Root;
            refs.DetailCombatLesson = lesson.Body;
            refs.DetailStoryRow = story.Root;
            refs.DetailStory = story.Body;
            refs.DetailSegmentRow = segment.Root;
            refs.DetailSegment = segment.Body;

            refs.DetailAvailability = CreateText(
                panel,
                "DetailAvailability",
                string.Empty,
                mediumFont,
                22f,
                Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(335f, -340f),
                new Vector2(600f, 180f));
            refs.DetailAvailability.rectTransform.pivot = new Vector2(0.5f, 1f);

            refs.UnverifiedRows = new GameObject[6];
            string[] unverifiedNames =
            {
                "RecommendedPowerRow_HIDDEN",
                "LoadoutRow_HIDDEN",
                "DurationRow_HIDDEN",
                "ThreatRow_HIDDEN",
                "SummonRow_HIDDEN",
                "RewardRow_HIDDEN"
            };
            for (int i = 0; i < unverifiedNames.Length; i++)
            {
                GameObject hidden = new GameObject(unverifiedNames[i], typeof(RectTransform));
                hidden.transform.SetParent(panel, false);
                hidden.SetActive(false);
                refs.UnverifiedRows[i] = hidden;
            }

            refs.DetailReviewButton = CreateButton(
                panel,
                "DetailReviewButton",
                "OPEN REVIEW CONFIRMATION  ›",
                semiBoldFont,
                22f,
                Ink,
                Cyan,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 25f),
                new Vector2(620f, 74f));
            CreateText(
                root,
                "DetailBoundaryNote",
                "LOCAL REVIEW DETAIL  /  PRODUCT ROUTING DISCONNECTED",
                mediumFont,
                17f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(390f, 54f),
                new Vector2(700f, 34f));
        }

        private static void BuildReviewConfirm(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.ReviewConfirmPanel.GetComponent<RectTransform>();
            CreateImage(
                root,
                "ConfirmDim",
                new Color(0.002f, 0.008f, 0.018f, 0.78f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            Image panelImage = CreateImage(
                root,
                "ConfirmPlate",
                Panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -16f),
                new Vector2(940f, 620f));
            RectTransform panel = panelImage.rectTransform;
            CreateText(
                panel,
                "ConfirmEyebrow",
                "CHUB-01 / LOCAL REVIEW BOUNDARY",
                mediumFont,
                18f,
                Cyan,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -70f),
                new Vector2(760f, 36f));
            refs.ConfirmTitle = CreateText(
                panel,
                "ConfirmTitle",
                "기억의 회랑",
                semiBoldFont,
                48f,
                White,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -142f),
                new Vector2(800f, 76f));
            refs.ConfirmSummary = CreateText(
                panel,
                "ConfirmSummary",
                "Canonical stage information was resolved for this local review path.",
                mediumFont,
                24f,
                White,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 26f),
                new Vector2(780f, 150f));
            refs.ConfirmStatus = CreateText(
                panel,
                "ConfirmStatus",
                "REVIEW SAMPLE / CONFIRMATION ONLY",
                semiBoldFont,
                18f,
                Amber,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -92f),
                new Vector2(760f, 42f));
            refs.ConfirmAcceptButton = CreateButton(
                panel,
                "ConfirmAcceptButton",
                "ACKNOWLEDGE REVIEW",
                semiBoldFont,
                23f,
                Ink,
                Cyan,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 122f),
                new Vector2(560f, 76f));
            refs.ConfirmBackButton = CreateButton(
                panel,
                "ConfirmBackButton",
                "BACK / RESTART",
                semiBoldFont,
                19f,
                White,
                InkSoft,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 48f),
                new Vector2(320f, 54f));
        }

        private static ChapterCardRefs CreateChapterCard(
            RectTransform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color accent,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont,
            bool interactable)
        {
            Image body = CreateImage(
                parent,
                name,
                new Color(Panel.r, Panel.g, Panel.b, 0.94f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size);
            body.raycastTarget = true;
            Button button = body.gameObject.AddComponent<Button>();
            button.targetGraphic = body;
            button.interactable = interactable;
            CanvasGroup group = body.gameObject.AddComponent<CanvasGroup>();
            CreateImage(
                body.rectTransform,
                "Accent",
                accent,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(5f, 0f),
                new Vector2(10f, 0f));
            TMP_Text episode = CreateText(
                body.rectTransform,
                "Episode",
                string.Empty,
                semiBoldFont,
                20f,
                accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(46f, -56f),
                new Vector2(size.x - 80f, 38f));
            TMP_Text title = CreateText(
                body.rectTransform,
                "Title",
                string.Empty,
                semiBoldFont,
                38f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(size.x * 0.5f, 28f),
                new Vector2(size.x - 86f, 70f));
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            TMP_Text status = CreateText(
                body.rectTransform,
                "Status",
                string.Empty,
                mediumFont,
                17f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(size.x * 0.5f, 50f),
                new Vector2(size.x - 86f, 38f));
            status.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return new ChapterCardRefs(button, group, episode, title, status);
        }

        private static StageNodeRefs CreateStageNode(
            RectTransform parent,
            string name,
            Vector2 normalizedPosition,
            Color accent,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            Image body = CreateImage(
                parent,
                name,
                new Color(PanelSoft.r, PanelSoft.g, PanelSoft.b, 0.97f),
                normalizedPosition,
                normalizedPosition,
                Vector2.zero,
                new Vector2(286f, 126f));
            body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            body.raycastTarget = true;
            Button button = body.gameObject.AddComponent<Button>();
            button.targetGraphic = body;
            CanvasGroup group = body.gameObject.AddComponent<CanvasGroup>();
            CreateImage(
                body.rectTransform,
                "NodeAccent",
                accent,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(4f, 0f),
                new Vector2(8f, 0f));
            TMP_Text code = CreateText(
                body.rectTransform,
                "StageCode",
                string.Empty,
                semiBoldFont,
                20f,
                accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(30f, -18f),
                new Vector2(230f, 30f));
            TMP_Text title = CreateText(
                body.rectTransform,
                "StageTitle",
                string.Empty,
                semiBoldFont,
                22f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(142f, -7f),
                new Vector2(236f, 36f));
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            TMP_Text status = CreateText(
                body.rectTransform,
                "StageStatus",
                string.Empty,
                mediumFont,
                12f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(142f, 17f),
                new Vector2(236f, 26f));
            status.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return new StageNodeRefs(button, group, body.rectTransform, code, title, status);
        }

        private static DetailRowRefs CreateDetailRow(
            RectTransform parent,
            string name,
            string label,
            Vector2 position,
            Vector2 size,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            Image rowImage = CreateImage(
                parent,
                name,
                new Color(InkSoft.r, InkSoft.g, InkSoft.b, 0.82f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size);
            CreateText(
                rowImage.rectTransform,
                "Label",
                label,
                semiBoldFont,
                14f,
                CyanSoft,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(22f, -18f),
                new Vector2(size.x - 44f, 24f));
            TMP_Text body = CreateText(
                rowImage.rectTransform,
                "Body",
                string.Empty,
                mediumFont,
                18f,
                White,
                TextAlignmentOptions.TopLeft,
                Vector2.zero,
                Vector2.one,
                new Vector2(0f, -12f),
                new Vector2(-44f, -44f));
            RectTransform bodyRect = body.rectTransform;
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.pivot = new Vector2(0.5f, 0.5f);
            bodyRect.offsetMin = new Vector2(22f, 12f);
            bodyRect.offsetMax = new Vector2(-22f, -44f);
            body.margin = new Vector4(4f, 2f, 4f, 2f);
            return new DetailRowRefs(rowImage.gameObject, body);
        }

        private static void ConfigureController(
            OlympusChapterHubReviewController controller,
            ChapterHubReviewProfile profile,
            UIStageCatalog stageCatalog,
            ReviewUiRefs ui)
        {
            controller.ConfigureCore(profile, stageCatalog);
            controller.ConfigurePanels(
                ui.ChapterHubPanel,
                ui.StageMapPanel,
                ui.StageDetailPanel,
                ui.ReviewConfirmPanel);
            controller.ConfigureChapterView(
                ui.HubEpisode,
                ui.HubTitle,
                ui.HubStatus,
                ui.ChapterBindings);
            controller.ConfigureStageMapView(
                ui.MapEpisode,
                ui.MapTitle,
                ui.MapStatus,
                ui.MapBackButton,
                ui.StageNodeBindings);
            controller.ConfigureStageDetailView(
                ui.DetailStageCode,
                ui.DetailTitle,
                ui.DetailStatus,
                ui.DetailObjectiveRow,
                ui.DetailObjective,
                ui.DetailCombatLessonRow,
                ui.DetailCombatLesson,
                ui.DetailStoryRow,
                ui.DetailStory,
                ui.DetailSegmentRow,
                ui.DetailSegment,
                ui.DetailBackButton,
                ui.DetailReviewButton);
            controller.ConfigureUnverifiedDetailRows(
                ui.UnverifiedRows[0],
                ui.UnverifiedRows[1],
                ui.UnverifiedRows[2],
                ui.UnverifiedRows[3],
                ui.UnverifiedRows[4],
                ui.UnverifiedRows[5]);
            controller.ConfigureAvailabilityText(ui.DetailAvailability);
            controller.ConfigureConfirmationView(
                ui.ConfirmTitle,
                ui.ConfirmSummary,
                ui.ConfirmStatus,
                ui.ConfirmBackButton,
                ui.ConfirmAcceptButton);
            controller.BeginReview();
        }

        private static List<string> ValidateGeneratedReview(Scene scene)
        {
            var issues = new List<string>();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                issues.Add("Review scene is not loaded.");
                return issues;
            }

            if (!string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                issues.Add($"Loaded scene path is `{scene.path}` instead of `{ScenePath}`.");
            }

            if (EditorBuildSettings.scenes.Any(
                    entry => entry.enabled
                        && string.Equals(entry.path, ScenePath, StringComparison.Ordinal)))
            {
                issues.Add("Review scene must remain outside enabled Build Settings.");
            }

            ChapterHubReviewProfile profile =
                AssetDatabase.LoadAssetAtPath<ChapterHubReviewProfile>(ProfilePath);
            if (profile == null)
            {
                issues.Add("Review profile is missing.");
            }
            else if (!profile.TryValidate(out string profileError))
            {
                issues.Add("Review profile is invalid: " + profileError);
            }
            else
            {
                ValidateProfileComposition(profile, issues);
            }

            UIStageCatalog stageCatalog =
                AssetDatabase.LoadAssetAtPath<UIStageCatalog>(StageCatalogPath);
            if (stageCatalog == null || stageCatalog.StageCount != 1)
            {
                issues.Add("Canonical UI stage catalog must remain present with exactly one entry.");
            }
            else if (!stageCatalog.TryCreateRouteProjection(
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
            }
            else
            {
                StageBriefingReadModel briefing = projection.Briefing;
                if (briefing == null
                    || briefing.TitleDisposition != StageBriefingValueDisposition.Present
                    || briefing.ObjectiveDisposition != StageBriefingValueDisposition.Present
                    || briefing.CombatLessonDisposition != StageBriefingValueDisposition.Present
                    || briefing.StoryEntryDisposition != StageReferenceDisposition.Present
                    || briefing.SegmentCount <= 0)
                {
                    issues.Add(
                        "Canonical briefing no longer admits the title, objective, combat lesson, "
                        + "story entry, and route segment evidence rendered by CHUB-01.");
                }

                if (briefing != null
                    && (briefing.RecommendedPowerDisposition == StageBriefingValueDisposition.Present
                        || briefing.RecommendedLoadoutDisposition == StageBriefingValueDisposition.Present
                        || briefing.TargetRunDurationDisposition == StageBriefingValueDisposition.Present
                        || briefing.FeaturedThreatDisposition == StageBriefingValueDisposition.Present
                        || briefing.FeaturedSummonNeedDisposition == StageBriefingValueDisposition.Present
                        || briefing.RewardPreviewDisposition == StageBriefingValueDisposition.Present))
                {
                    issues.Add("CHUB-01 expected optional product values to remain unverified and hidden.");
                }
            }

            OlympusChapterHubReviewController[] controllers =
                FindComponentsInScene<OlympusChapterHubReviewController>(scene);
            if (controllers.Length != 1)
            {
                issues.Add($"Review scene needs exactly one controller; found {controllers.Length}.");
            }
            else
            {
                ValidateControllerReferences(controllers[0], profile, stageCatalog, issues);
            }

            if (FindComponentsInScene<UISceneFlowRouter>(scene).Length != 0
                || FindComponentsInScene<UISceneRouteLoader>(scene).Length != 0)
            {
                issues.Add("Review scene must not contain a scene router or route loader.");
            }

            Canvas[] canvases = FindComponentsInScene<Canvas>(scene);
            if (canvases.Length != 1)
            {
                issues.Add($"Review scene needs exactly one Canvas; found {canvases.Length}.");
            }
            else
            {
                CanvasScaler scaler = canvases[0].GetComponent<CanvasScaler>();
                Camera[] sceneCameras = FindComponentsInScene<Camera>(scene);
                if (scaler == null
                    || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                    || scaler.referenceResolution != new Vector2(1920f, 1080f)
                    || scaler.screenMatchMode
                        != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
                    || !Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f)
                    || canvases[0].renderMode != RenderMode.ScreenSpaceCamera
                    || sceneCameras.Length != 1
                    || canvases[0].worldCamera != sceneCameras[0]
                    || canvases[0].GetComponent<GraphicRaycaster>() == null)
                {
                    issues.Add(
                        "Review Canvas camera, raycaster, and 1920x1080 Scale With Screen Size "
                        + "baseline are incomplete.");
                }
            }

            UISafeAreaRoot[] safeAreaRoots = FindComponentsInScene<UISafeAreaRoot>(scene);
            if (safeAreaRoots.Length != 1)
            {
                issues.Add(
                    $"Review scene needs exactly one UISafeAreaRoot; found "
                    + $"{safeAreaRoots.Length}.");
            }
            else
            {
                UISafeAreaRoot safeAreaRoot = safeAreaRoots[0];
                RectTransform safeRect = safeAreaRoot.transform as RectTransform;
                var safeSerialized = new SerializedObject(safeAreaRoot);
                bool safeAreaValid = safeRect != null
                    && safeSerialized.FindProperty("applyOnEnable")?.boolValue == true
                    && safeSerialized.FindProperty("target")?.objectReferenceValue == safeRect
                    && safeSerialized.FindProperty("mode")?.intValue
                        == (int)UISafeAreaMode.InsetsOnly
                    && Mathf.Approximately(
                        safeSerialized.FindProperty("extraInsetPixels")?.floatValue ?? -1f,
                        24f)
                    && safeRect.anchorMin == Vector2.zero
                    && safeRect.anchorMax == Vector2.one
                    && safeRect.offsetMin == Vector2.zero
                    && safeRect.offsetMax == Vector2.zero;
                if (!safeAreaValid)
                {
                    issues.Add(
                        "Review safe-area policy must serialize runtime InsetsOnly/24px while "
                        + "the authored RectTransform remains fully stretched.");
                }
            }

            EventSystem[] eventSystems = FindComponentsInScene<EventSystem>(scene);
            if (eventSystems.Length != 1
                || eventSystems[0].GetComponent<InputSystemUIInputModule>() == null)
            {
                issues.Add("Review scene needs one InputSystem EventSystem.");
            }

            Camera[] cameras = FindComponentsInScene<Camera>(scene);
            AudioListener[] listeners = FindComponentsInScene<AudioListener>(scene);
            if (cameras.Length != 1 || listeners.Length != 1 || cameras[0].tag != "MainCamera")
            {
                issues.Add("Review scene needs one MainCamera and one AudioListener.");
            }

            TextureImporter importer = AssetImporter.GetAtPath(BackgroundArtPath) as TextureImporter;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundArtPath);
            Image mapBackground = FindComponentsInScene<Image>(scene).FirstOrDefault(
                image => string.Equals(image.gameObject.name, "ChapterMapBackground", StringComparison.Ordinal));
            if (importer == null
                || importer.textureType != TextureImporterType.Sprite
                || importer.wrapMode != TextureWrapMode.Clamp
                || importer.mipmapEnabled
                || sprite == null
                || mapBackground == null
                || mapBackground.sprite != sprite)
            {
                issues.Add("Generated chapter-map background import or scene binding is invalid.");
            }

            foreach (Button button in FindComponentsInScene<Button>(scene))
            {
                Rect rect = (button.transform as RectTransform)?.rect ?? Rect.zero;
                if (rect.width < 48f || rect.height < 48f)
                {
                    issues.Add(
                        $"Button `{button.gameObject.name}` is smaller than the 48px reference target.");
                }

                if (button.onClick.GetPersistentEventCount() != 0)
                {
                    issues.Add(
                        $"Button `{button.gameObject.name}` must not contain authored persistent "
                        + "callbacks in the review scene.");
                }
            }

            return issues;
        }

        private static void ValidateProfileComposition(
            ChapterHubReviewProfile profile,
            List<string> issues)
        {
            if (profile.ChapterCount != 1 || profile.StageCount != 3)
            {
                issues.Add(
                    $"CHUB-01 requires one chapter and three review slots; found "
                    + $"{profile.ChapterCount} chapters and {profile.StageCount} stages.");
                return;
            }

            ChapterHubReviewProfile.ChapterDefinition chapter = profile.GetChapter(0);
            if (chapter == null
                || !string.Equals(chapter.ChapterId, ChapterId, StringComparison.Ordinal))
            {
                issues.Add($"CHUB-01 requires the review chapter id `{ChapterId}`.");
            }

            int canonical = 0;
            int inProduction = 0;
            int announced = 0;
            var stageIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < profile.StageCount; i++)
            {
                ChapterHubReviewProfile.StageDefinition stage = profile.GetStage(i);
                stageIds.Add(stage.StageId);
                if (!string.Equals(stage.ChapterId, ChapterId, StringComparison.Ordinal))
                {
                    issues.Add($"Stage `{stage.StageId}` is outside the Olympus review chapter.");
                }

                switch (stage.ContentStatus)
                {
                    case ChapterHubReviewContentStatus.CanonicalPlayable:
                        canonical++;
                        break;
                    case ChapterHubReviewContentStatus.InProduction:
                        inProduction++;
                        break;
                    case ChapterHubReviewContentStatus.Announced:
                        announced++;
                        break;
                }

                if (string.Equals(stage.StageId, CanonicalStageId, StringComparison.Ordinal))
                {
                    if (stage.ContentStatus
                            != ChapterHubReviewContentStatus.CanonicalPlayable
                        || !string.Equals(
                            stage.CanonicalCatalogEntryId,
                            CanonicalCatalogEntryId,
                            StringComparison.Ordinal))
                    {
                        issues.Add(
                            $"Canonical review slot `{CanonicalStageId}` must bind exact catalog entry "
                            + $"`{CanonicalCatalogEntryId}`.");
                    }
                }
                else if (string.Equals(
                             stage.StageId,
                             InProductionStageId,
                             StringComparison.Ordinal))
                {
                    if (stage.ContentStatus != ChapterHubReviewContentStatus.InProduction)
                    {
                        issues.Add(
                            $"Review slot `{InProductionStageId}` must remain InProduction.");
                    }
                }
                else if (string.Equals(
                             stage.StageId,
                             AnnouncedStageId,
                             StringComparison.Ordinal))
                {
                    if (stage.ContentStatus != ChapterHubReviewContentStatus.Announced)
                    {
                        issues.Add($"Review slot `{AnnouncedStageId}` must remain Announced.");
                    }
                }
                else
                {
                    issues.Add($"Unexpected CHUB-01 review stage id `{stage.StageId}`.");
                }
            }

            if (canonical != 1 || inProduction != 1 || announced != 1)
            {
                issues.Add(
                    "CHUB-01 requires exactly one CanonicalPlayable, InProduction, and Announced slot.");
            }

            string[] expectedStageIds =
            {
                CanonicalStageId,
                InProductionStageId,
                AnnouncedStageId
            };
            foreach (string expectedStageId in expectedStageIds)
            {
                if (!stageIds.Contains(expectedStageId))
                {
                    issues.Add($"CHUB-01 review profile is missing `{expectedStageId}`.");
                }
            }
        }

        private static void ValidateControllerReferences(
            OlympusChapterHubReviewController controller,
            ChapterHubReviewProfile profile,
            UIStageCatalog stageCatalog,
            List<string> issues)
        {
            SerializedObject serialized = new SerializedObject(controller);
            ValidateObjectReference(serialized, "profile", profile, issues);
            ValidateObjectReference(serialized, "stageCatalog", stageCatalog, issues);
            string[] requiredReferences =
            {
                "chapterHubPanel",
                "stageMapPanel",
                "stageDetailPanel",
                "reviewConfirmPanel",
                "hubEpisodeCodeText",
                "hubTitleText",
                "hubStatusText",
                "mapEpisodeCodeText",
                "mapChapterTitleText",
                "mapStatusText",
                "mapBackButton",
                "detailStageCodeText",
                "detailTitleText",
                "detailStatusText",
                "detailAvailabilityText",
                "detailObjectiveRow",
                "detailObjectiveText",
                "detailCombatLessonRow",
                "detailCombatLessonText",
                "detailStoryRow",
                "detailStoryText",
                "detailSegmentRow",
                "detailSegmentText",
                "detailRecommendedPowerRow",
                "detailLoadoutRow",
                "detailDurationRow",
                "detailThreatRow",
                "detailSummonRow",
                "detailRewardRow",
                "detailBackButton",
                "detailReviewButton",
                "confirmTitleText",
                "confirmSummaryText",
                "confirmStatusText",
                "confirmBackButton",
                "confirmAcceptButton"
            };
            foreach (string propertyName in requiredReferences)
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null || property.objectReferenceValue == null)
                {
                    issues.Add($"Review controller is missing `{propertyName}`.");
                }
            }

            SerializedProperty chapterBindings = serialized.FindProperty("chapterBindings");
            SerializedProperty stageBindings = serialized.FindProperty("stageNodeBindings");
            if (chapterBindings == null || chapterBindings.arraySize != 1)
            {
                issues.Add("Review controller needs exactly one chapter binding.");
            }
            else
            {
                ValidateChapterBinding(chapterBindings.GetArrayElementAtIndex(0), issues);
            }
            if (stageBindings == null || stageBindings.arraySize != 3)
            {
                issues.Add("Review controller needs exactly three stage-node bindings.");
            }
            else
            {
                var expectedStageIds = new HashSet<string>(StringComparer.Ordinal)
                {
                    CanonicalStageId,
                    InProductionStageId,
                    AnnouncedStageId
                };
                for (int i = 0; i < stageBindings.arraySize; i++)
                {
                    ValidateStageBinding(
                        stageBindings.GetArrayElementAtIndex(i),
                        i,
                        expectedStageIds,
                        issues);
                }

                foreach (string missingStageId in expectedStageIds)
                {
                    issues.Add(
                        $"Review controller is missing stage binding `{missingStageId}`.");
                }
            }

            if (controller.ConfirmationEvent.GetPersistentEventCount() != 0)
            {
                issues.Add(
                    "Review confirmation event must not contain authored persistent callbacks.");
            }

            ValidateNoOverlap(
                serialized,
                "detailObjectiveRow",
                "detailCombatLessonRow",
                issues);
            ValidateNoOverlap(
                serialized,
                "detailCombatLessonRow",
                "detailStoryRow",
                issues);
            ValidateNoOverlap(
                serialized,
                "detailStoryRow",
                "detailSegmentRow",
                issues);
            ValidateNoOverlap(
                serialized,
                "detailSegmentRow",
                "detailReviewButton",
                issues);
            ValidateNoOverlap(
                serialized,
                "detailStageCodeText",
                "detailTitleText",
                issues);
            ValidateNoOverlap(
                serialized,
                "detailTitleText",
                "detailStatusText",
                issues);
        }

        private static void ValidateNoOverlap(
            SerializedObject serialized,
            string firstPropertyName,
            string secondPropertyName,
            List<string> issues)
        {
            RectTransform first = ResolveRectTransform(
                serialized.FindProperty(firstPropertyName)?.objectReferenceValue);
            RectTransform second = ResolveRectTransform(
                serialized.FindProperty(secondPropertyName)?.objectReferenceValue);
            if (first == null || second == null)
            {
                return;
            }

            Rect firstRect = CalculateWorldRect(first);
            Rect secondRect = CalculateWorldRect(second);
            if (firstRect.Overlaps(secondRect))
            {
                issues.Add(
                    $"Review layout elements `{firstPropertyName}` and "
                    + $"`{secondPropertyName}` overlap.");
            }
        }

        private static RectTransform ResolveRectTransform(UnityEngine.Object value)
        {
            return value switch
            {
                GameObject gameObject => gameObject.transform as RectTransform,
                Component component => component.transform as RectTransform,
                _ => null
            };
        }

        private static Rect CalculateWorldRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float xMin = corners.Min(corner => corner.x);
            float xMax = corners.Max(corner => corner.x);
            float yMin = corners.Min(corner => corner.y);
            float yMax = corners.Max(corner => corner.y);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static void ValidateChapterBinding(
            SerializedProperty binding,
            List<string> issues)
        {
            if (binding == null)
            {
                issues.Add("Review chapter binding is missing.");
                return;
            }

            SerializedProperty chapterId = binding.FindPropertyRelative("chapterId");
            if (chapterId == null
                || !string.Equals(chapterId.stringValue, ChapterId, StringComparison.Ordinal))
            {
                issues.Add($"Review chapter binding must use id `{ChapterId}`.");
            }

            ValidateBindingObjectReference(binding, "button", "chapter", 0, issues);
            ValidateBindingObjectReference(binding, "canvasGroup", "chapter", 0, issues);
            ValidateBindingObjectReference(binding, "episodeCodeText", "chapter", 0, issues);
            ValidateBindingObjectReference(binding, "titleText", "chapter", 0, issues);
        }

        private static void ValidateStageBinding(
            SerializedProperty binding,
            int index,
            HashSet<string> expectedStageIds,
            List<string> issues)
        {
            if (binding == null)
            {
                issues.Add($"Review stage binding {index} is missing.");
                return;
            }

            SerializedProperty stageId = binding.FindPropertyRelative("stageId");
            string resolvedStageId = stageId?.stringValue ?? string.Empty;
            if (!expectedStageIds.Remove(resolvedStageId))
            {
                issues.Add(
                    $"Review stage binding {index} has duplicate or unexpected id "
                    + $"`{resolvedStageId}`.");
            }

            ValidateBindingObjectReference(binding, "button", "stage", index, issues);
            ValidateBindingObjectReference(binding, "canvasGroup", "stage", index, issues);
            ValidateBindingObjectReference(binding, "mapAnchor", "stage", index, issues);
            ValidateBindingObjectReference(binding, "stageCodeText", "stage", index, issues);
            ValidateBindingObjectReference(binding, "titleText", "stage", index, issues);
            ValidateBindingObjectReference(binding, "statusText", "stage", index, issues);
            ValidateBindingNoOverlap(
                binding,
                "stageCodeText",
                "titleText",
                index,
                issues);
            ValidateBindingNoOverlap(
                binding,
                "titleText",
                "statusText",
                index,
                issues);
        }

        private static void ValidateBindingNoOverlap(
            SerializedProperty binding,
            string firstRelativePropertyName,
            string secondRelativePropertyName,
            int index,
            List<string> issues)
        {
            RectTransform first = ResolveRectTransform(
                binding.FindPropertyRelative(firstRelativePropertyName)?.objectReferenceValue);
            RectTransform second = ResolveRectTransform(
                binding.FindPropertyRelative(secondRelativePropertyName)?.objectReferenceValue);
            if (first == null || second == null)
            {
                return;
            }

            if (CalculateWorldRect(first).Overlaps(CalculateWorldRect(second)))
            {
                issues.Add(
                    $"Review stage binding {index} elements `{firstRelativePropertyName}` and "
                    + $"`{secondRelativePropertyName}` overlap.");
            }
        }

        private static void ValidateBindingObjectReference(
            SerializedProperty binding,
            string relativePropertyName,
            string bindingKind,
            int index,
            List<string> issues)
        {
            SerializedProperty property = binding.FindPropertyRelative(relativePropertyName);
            if (property == null || property.objectReferenceValue == null)
            {
                issues.Add(
                    $"Review {bindingKind} binding {index} is missing "
                    + $"`{relativePropertyName}`.");
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
            Color labelColor,
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
            colors.pressedColor = new Color(0.72f, 0.86f, 0.94f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.35f, 0.40f, 0.47f, 0.64f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            CreateText(
                image.rectTransform,
                "Label",
                label,
                font,
                fontSize,
                labelColor,
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
            if (safeAreaRoot == null || target == null)
            {
                throw new InvalidOperationException(
                    "Review safe-area root and target are required.");
            }

            var serialized = new SerializedObject(safeAreaRoot);
            serialized.FindProperty("applyOnEnable").boolValue = true;
            serialized.FindProperty("target").objectReferenceValue = target;
            serialized.FindProperty("mode").intValue = (int)mode;
            serialized.FindProperty("extraInsetPixels").floatValue =
                Mathf.Max(0f, extraInsetPixels);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Scene authoring must not bake the batch process' transient Screen size.
            // UISafeAreaRoot applies the serialized policy against the real device on enable.
            Stretch(target);
            EditorUtility.SetDirty(safeAreaRoot);
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
                Mathf.Approximately(anchorMin.x, anchorMax.x) ? anchorMin.x : 0.5f,
                Mathf.Approximately(anchorMin.y, anchorMax.y) ? anchorMin.y : 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localScale = Vector3.one;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
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
                issues.Add($"Review controller `{propertyName}` is missing or stale.");
            }
        }

        private static T[] FindComponentsInScene<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }
            return results.ToArray();
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Required {typeof(T).Name} asset is missing at `{path}`.");
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
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static string PathParent(string path)
        {
            return Path.GetDirectoryName(path)?.Replace('\\', '/') ?? string.Empty;
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
                if (!pair.Value.Exists)
                {
                    issues.Add(
                        $"Canonical boundary file was missing before setup: `{pair.Key}`.");
                }
                else if (!after.Exists)
                {
                    issues.Add(
                        $"Canonical boundary file disappeared during setup: `{pair.Key}`.");
                }
                else if (pair.Value.Length != after.Length
                    || !string.Equals(
                        pair.Value.Sha256,
                        after.Sha256,
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Canonical boundary file content changed during setup: `{pair.Key}`.");
                }
            }
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

                var info = new FileInfo(absolutePath);
                using FileStream stream = File.OpenRead(absolutePath);
                using SHA256 sha = SHA256.Create();
                string hash = BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
                return new CanonicalAssetFingerprint(true, info.Length, hash);
            }
        }

        private sealed class ReviewUiRefs
        {
            public CanvasGroup ChapterHubPanel;
            public CanvasGroup StageMapPanel;
            public CanvasGroup StageDetailPanel;
            public CanvasGroup ReviewConfirmPanel;
            public TMP_Text HubEpisode;
            public TMP_Text HubTitle;
            public TMP_Text HubStatus;
            public OlympusChapterHubReviewController.ChapterButtonBinding[] ChapterBindings;
            public TMP_Text MapEpisode;
            public TMP_Text MapTitle;
            public TMP_Text MapStatus;
            public Button MapBackButton;
            public OlympusChapterHubReviewController.StageNodeBinding[] StageNodeBindings;
            public TMP_Text DetailStageCode;
            public TMP_Text DetailTitle;
            public TMP_Text DetailStatus;
            public TMP_Text DetailAvailability;
            public GameObject DetailObjectiveRow;
            public TMP_Text DetailObjective;
            public GameObject DetailCombatLessonRow;
            public TMP_Text DetailCombatLesson;
            public GameObject DetailStoryRow;
            public TMP_Text DetailStory;
            public GameObject DetailSegmentRow;
            public TMP_Text DetailSegment;
            public GameObject[] UnverifiedRows;
            public Button DetailBackButton;
            public Button DetailReviewButton;
            public TMP_Text ConfirmTitle;
            public TMP_Text ConfirmSummary;
            public TMP_Text ConfirmStatus;
            public Button ConfirmBackButton;
            public Button ConfirmAcceptButton;
        }

        private readonly struct ChapterCardRefs
        {
            public ChapterCardRefs(
                Button button,
                CanvasGroup group,
                TMP_Text episode,
                TMP_Text title,
                TMP_Text status)
            {
                Button = button;
                Group = group;
                Episode = episode;
                Title = title;
                Status = status;
            }
            public Button Button { get; }
            public CanvasGroup Group { get; }
            public TMP_Text Episode { get; }
            public TMP_Text Title { get; }
            public TMP_Text Status { get; }
        }

        private readonly struct StageNodeRefs
        {
            public StageNodeRefs(
                Button button,
                CanvasGroup group,
                RectTransform anchor,
                TMP_Text code,
                TMP_Text title,
                TMP_Text status)
            {
                Button = button;
                Group = group;
                Anchor = anchor;
                Code = code;
                Title = title;
                Status = status;
            }
            public Button Button { get; }
            public CanvasGroup Group { get; }
            public RectTransform Anchor { get; }
            public TMP_Text Code { get; }
            public TMP_Text Title { get; }
            public TMP_Text Status { get; }
            public OlympusChapterHubReviewController.StageNodeBinding ToBinding(string stageId)
            {
                return new OlympusChapterHubReviewController.StageNodeBinding(
                    stageId,
                    Button,
                    Group,
                    Anchor,
                    Code,
                    Title,
                    Status);
            }
        }

        private readonly struct DetailRowRefs
        {
            public DetailRowRefs(GameObject root, TMP_Text body)
            {
                Root = root;
                Body = body;
            }
            public GameObject Root { get; }
            public TMP_Text Body { get; }
        }
    }
}
