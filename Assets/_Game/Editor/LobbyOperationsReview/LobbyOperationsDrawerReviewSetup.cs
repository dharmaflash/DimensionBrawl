using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.LobbyOperationsReview;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.LobbyOperationsReview
{
    /// <summary>
    /// Deterministically authors the isolated OPS-01 lobby operations drawer review scene.
    /// The generated scene is a review sample, remains outside Build Settings, and never owns
    /// routing, networking, account state, persistence, rewards, or StageRun state.
    /// </summary>
    public static class LobbyOperationsDrawerReviewSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Review/UI_LobbyOperationsDrawerReview.unity";
        public const string ProfilePath =
            "Assets/_Game/DesignData/UI/Review/DB_UILobbyOperationsReview.asset";

        private const string BackgroundPath =
            "Assets/_Game/UI/Lobby/Art/Dimension_Lobby_UI_0000_Background.png";
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
            "Assets/_Game/Scenes/UI/UI_Lobby.unity",
            "Assets/_Game/UI/Lobby/PF_UI_LobbyScreen.prefab",
            "Assets/_Game/UI/Lobby/PF_UI_LobbyCharacterStage.prefab",
            "Assets/_Game/UI/Presentation/PF_UI_LobbyPresentation.prefab",
            BackgroundPath,
            // TMP dynamic atlas tables are transient editor caches and are cleared on a clean
            // Editor exit. Protect the immutable source fonts and import metadata instead.
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-Medium.otf",
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf",
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset",
            "Assets/_Game/DesignData/UI/DB_UIScreenCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UIPanelCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UITextCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UIStateMessages.asset",
            "Assets/_Game/DesignData/UI/DB_UIMotionCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UICueBundles.asset",
            ResponsiveCatalogPath
        };

        private static readonly Color Ink = new Color(0.012f, 0.021f, 0.040f, 0.98f);
        private static readonly Color InkSoft = new Color(0.026f, 0.049f, 0.080f, 0.96f);
        private static readonly Color Panel = new Color(0.035f, 0.068f, 0.108f, 0.975f);
        private static readonly Color PanelSoft = new Color(0.062f, 0.105f, 0.153f, 0.96f);
        private static readonly Color Cyan = new Color(0.28f, 0.91f, 1.00f, 1f);
        private static readonly Color CyanSoft = new Color(0.48f, 0.77f, 0.88f, 1f);
        private static readonly Color Amber = new Color(1.00f, 0.69f, 0.29f, 1f);
        private static readonly Color White = new Color(0.95f, 0.985f, 1.00f, 1f);
        private static readonly Color Muted = new Color(0.64f, 0.73f, 0.81f, 1f);

        [MenuItem("Tools/DimensionBrawl/Review/Setup Lobby Operations Drawer Review")]
        public static void SetupMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Setup();
        }

        [MenuItem("Tools/DimensionBrawl/Review/Validate Lobby Operations Drawer Review")]
        public static void ValidateMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            RunBatchVerification();
        }

        /// <summary>
        /// Batch entry point. The caller deliberately owns dirty-scene policy in batch mode.
        /// </summary>
        public static void RunBatchSetup()
        {
            Setup();
        }

        public static void RunBatchVerification()
        {
            Dictionary<string, CanonicalAssetFingerprint> fingerprints =
                CaptureCanonicalFingerprints();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var issues = ValidateGeneratedReview(scene);
            AppendCanonicalFingerprintIssues(fingerprints, issues);
            ThrowIfIssues("Lobby operations drawer review validation failed", issues);
            Debug.Log(
                "OPS-01 lobby operations drawer review validation passed. "
                + "The scene remains review-only and outside Build Settings.");
        }

        private static void Setup()
        {
            Dictionary<string, CanonicalAssetFingerprint> fingerprints =
                CaptureCanonicalFingerprints();

            EnsureAssetFolder(PathParent(ScenePath));
            EnsureAssetFolder(PathParent(ProfilePath));
            GuardSceneOutputPath();

            LobbyOperationsReviewProfile profile = EnsureProfile();
            TMP_FontAsset mediumFont = LoadRequired<TMP_FontAsset>(MediumFontPath);
            TMP_FontAsset semiBoldFont = LoadRequired<TMP_FontAsset>(SemiBoldFontPath);
            Sprite background = LoadRequired<Sprite>(BackgroundPath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("LobbyOperationsDrawerReview_Root");
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
                "LobbyOperationsReviewFlow",
                typeof(LobbyOperationsReviewController));
            controllerObject.transform.SetParent(root.transform, false);
            LobbyOperationsReviewController controller =
                controllerObject.GetComponent<LobbyOperationsReviewController>();
            ConfigureController(controller, profile, ui);

            // Re-apply after all authored components have run Reset/configuration hooks, then
            // fail before SaveScene if the persistent catalog cannot be represented.
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
                    $"Failed to save OPS-01 review scene `{ScenePath}`.");
            }

            // This is the only ScriptableObject that OPS-01 is allowed to author or save.
            AssetDatabase.SaveAssetIfDirty(profile);

            // Unity emits a trailing space for serialized empty strings. Normalize only the
            // generated review YAML so repeated setup stays diff-check clean without changing
            // any semantic value or touching canonical product assets.
            NormalizeGeneratedYamlWhitespace(ScenePath);

            // A save/reopen round trip guards against transient editor-only references.
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var issues = ValidateGeneratedReview(scene);
            AppendCanonicalFingerprintIssues(fingerprints, issues);
            ThrowIfIssues("Lobby operations drawer review setup failed", issues);

            Debug.Log(
                $"Created `{ScenePath}` with exactly four review entries, four flow panels, "
                + "and no route, service, persistence, or StageRun ownership.");
        }

        private static LobbyOperationsReviewProfile EnsureProfile()
        {
            LobbyOperationsReviewProfile profile =
                AssetDatabase.LoadAssetAtPath<LobbyOperationsReviewProfile>(ProfilePath);
            if (profile == null)
            {
                UnityEngine.Object existingAsset = AssetDatabase.LoadMainAssetAtPath(ProfilePath);
                if (existingAsset != null
                    || File.Exists(AssetPathToAbsolutePath(ProfilePath))
                    || File.Exists(AssetPathToAbsolutePath(ProfilePath + ".meta")))
                {
                    throw new InvalidOperationException(
                        $"Review profile path `{ProfilePath}` is occupied by an unexpected or "
                        + "unimportable asset; refusing to overwrite it.");
                }

                profile = ScriptableObject.CreateInstance<LobbyOperationsReviewProfile>();
                profile.name = "DB_UILobbyOperationsReview";
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.Configure(LobbyOperationsReviewProfile.CreateDefaultEntries());
            if (!profile.TryValidate(out string error))
            {
                throw new InvalidOperationException(
                    "Lobby operations review profile is invalid: " + error);
            }

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void GuardSceneOutputPath()
        {
            SceneAsset existingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            UnityEngine.Object occupiedAsset = AssetDatabase.LoadMainAssetAtPath(ScenePath);
            bool sceneFileExists = File.Exists(AssetPathToAbsolutePath(ScenePath));
            bool sceneMetaExists = File.Exists(AssetPathToAbsolutePath(ScenePath + ".meta"));

            // A valid imported SceneAsset is the one supported deterministic overwrite case.
            if (existingScene != null)
            {
                if (occupiedAsset != existingScene || !sceneFileExists || !sceneMetaExists)
                {
                    throw new InvalidOperationException(
                        $"Existing review scene `{ScenePath}` is not a complete imported SceneAsset; "
                        + "refusing deterministic regeneration.");
                }

                return;
            }

            if (occupiedAsset != null || sceneFileExists || sceneMetaExists)
            {
                throw new InvalidOperationException(
                    $"Review scene path `{ScenePath}` is occupied by a wrong-type, unimportable, "
                    + "or orphan-metadata asset; refusing to overwrite it.");
            }
        }

        private static Camera CreateReviewCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject(
                "ReviewCamera",
                typeof(Camera),
                typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -10f);
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
                "LobbyOperationsReviewCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(UIResponsiveRoot));
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
                "NeutralLobbyBackground",
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
            fitter.aspectRatio = Mathf.Max(0.1f, background.rect.width / background.rect.height);

            CreateImage(
                canvasRect,
                "LobbyAtmosphereWash",
                new Color(0.02f, 0.05f, 0.09f, 0.24f),
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
            ConfigureRuntimeSafeArea(refs.SafeAreaRoot, safeRect, UISafeAreaMode.InsetsOnly, 24f);

            CreateImage(
                safeRect,
                "TopRail",
                new Color(0.012f, 0.029f, 0.052f, 0.91f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -50f),
                new Vector2(0f, 100f));
            CreateText(
                safeRect,
                "ProductBreadcrumb",
                "DIMENSION BRAWL  /  OLYMPUS LOBBY",
                semiBoldFont,
                24f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(34f, -50f),
                new Vector2(720f, 48f));
            CreateText(
                safeRect,
                "ReviewBoundary",
                "OPS-01  •  REVIEW SAMPLE  •  TEMP - DO NOT SHIP",
                mediumFont,
                17f,
                Amber,
                TextAlignmentOptions.MidlineRight,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-34f, -50f),
                new Vector2(720f, 44f));

            refs.ClosedPanel = CreateFlowGroup(safeRect, "DrawerClosedPanel");
            refs.DirectoryPanel = CreateFlowGroup(safeRect, "DirectoryPanel");
            refs.DetailPanel = CreateFlowGroup(safeRect, "EntryDetailPanel");
            refs.ConfirmPanel = CreateFlowGroup(safeRect, "ReviewConfirmPanel");

            BuildClosed(refs, mediumFont, semiBoldFont);
            BuildDirectory(refs, mediumFont, semiBoldFont);
            BuildDetail(refs, mediumFont, semiBoldFont);
            BuildConfirm(refs, mediumFont, semiBoldFont);
            return refs;
        }

        private static void BuildClosed(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.ClosedPanel.GetComponent<RectTransform>();
            Image plate = CreateImage(
                root,
                "ClosedReviewPlate",
                new Color(0.018f, 0.045f, 0.074f, 0.88f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(34f, 36f),
                new Vector2(760f, 270f));
            refs.ClosedReviewLabel = CreateText(
                plate.rectTransform,
                "ClosedReviewLabel",
                "LOBBY OPERATIONS",
                semiBoldFont,
                30f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -40f),
                new Vector2(680f, 50f));
            refs.ClosedStatus = CreateText(
                plate.rectTransform,
                "ClosedStatus",
                "LOCAL REVIEW DIRECTORY / SOURCES ARE EXPLICIT",
                mediumFont,
                16f,
                CyanSoft,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -102f),
                new Vector2(680f, 34f));
            refs.OpenButton = CreateButton(
                plate.rectTransform,
                "OpenOperationsReviewButton",
                "OPEN OPERATIONS REVIEW  ›",
                semiBoldFont,
                21f,
                Ink,
                Cyan,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(28f, 24f),
                new Vector2(520f, 62f));
        }

        private static void BuildDirectory(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.DirectoryPanel.GetComponent<RectTransform>();
            CreateBlockingScrim(root, "DirectoryScrim");
            RectTransform drawer = CreateDrawer(root, "DirectoryDrawer", 800f, 876f);
            refs.DirectoryBackButton = CreateButton(
                drawer,
                "DirectoryBackButton",
                "‹  BACK",
                semiBoldFont,
                18f,
                White,
                InkSoft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -28f),
                new Vector2(150f, 54f));
            refs.DirectoryCloseButton = CreateButton(
                drawer,
                "DirectoryCloseButton",
                "CLOSE",
                semiBoldFont,
                17f,
                White,
                InkSoft,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-28f, -28f),
                new Vector2(132f, 54f));
            refs.DirectoryTitle = CreateText(
                drawer,
                "DirectoryTitle",
                "OPERATIONS DIRECTORY",
                semiBoldFont,
                34f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -104f),
                new Vector2(720f, 54f));
            refs.DirectoryStatus = CreateText(
                drawer,
                "DirectoryStatus",
                "FOUR REVIEW ENTRIES / NO ACCOUNT OR SERVICE VERDICT",
                mediumFont,
                15f,
                Amber,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -153f),
                new Vector2(720f, 32f));

            string[] ids =
            {
                LobbyOperationsReviewProfile.NoticeEntryId,
                LobbyOperationsReviewProfile.MailboxEntryId,
                LobbyOperationsReviewProfile.MissionsEntryId,
                LobbyOperationsReviewProfile.EventCalendarEntryId
            };
            string[] titles = { "운영 안내 샘플", "우편함", "미션", "이벤트 일정" };
            string[] statuses =
            {
                "LOCAL REVIEW FIXTURE",
                "SERVICE + ACCOUNT: NO VERIFIED SOURCE",
                "ACCOUNT + PROGRESS: NO VERIFIED SOURCE",
                "DEFINITION ONLY / NO CLOCK VERDICT"
            };
            refs.EntryBindings = new LobbyOperationsReviewController.EntryButtonBinding[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                EntryRowRefs row = CreateEntryRow(
                    drawer,
                    $"DirectoryEntry_{i + 1:00}",
                    titles[i],
                    statuses[i],
                    new Vector2(28f, -204f - i * 124f),
                    new Vector2(744f, 110f),
                    mediumFont,
                    semiBoldFont);
                refs.EntryBindings[i] =
                    new LobbyOperationsReviewController.EntryButtonBinding(
                        ids[i],
                        row.Button,
                        row.Group,
                        row.Title,
                        row.Status);
            }

            CreateText(
                drawer,
                "DirectoryBoundaryNote",
                "NoVerifiedSource is not empty, zero, offline, unavailable, or locked.",
                mediumFont,
                16f,
                Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(28f, 30f),
                new Vector2(744f, 36f));
        }

        private static void BuildDetail(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.DetailPanel.GetComponent<RectTransform>();
            CreateBlockingScrim(root, "DetailScrim");
            RectTransform drawer = CreateDrawer(root, "EntryDetailDrawer", 980f, 876f);
            refs.DetailBackButton = CreateButton(
                drawer,
                "DetailBackButton",
                "‹  DIRECTORY",
                semiBoldFont,
                18f,
                White,
                InkSoft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -28f),
                new Vector2(196f, 54f));
            refs.DetailCloseButton = CreateButton(
                drawer,
                "DetailCloseButton",
                "CLOSE",
                semiBoldFont,
                17f,
                White,
                InkSoft,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-28f, -28f),
                new Vector2(132f, 54f));
            refs.DetailKind = CreateText(
                drawer,
                "DetailKind",
                "NOTICE / REVIEW FIXTURE",
                mediumFont,
                15f,
                Cyan,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(30f, -102f),
                new Vector2(700f, 30f));
            refs.DetailTitle = CreateText(
                drawer,
                "DetailTitle",
                "운영 안내 샘플",
                semiBoldFont,
                34f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(30f, -137f),
                new Vector2(900f, 54f));
            refs.DetailStatus = CreateText(
                drawer,
                "DetailStatus",
                "SOURCE RESPONSIBILITIES REMAIN SEPARATE",
                mediumFont,
                15f,
                Amber,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(30f, -188f),
                new Vector2(900f, 30f));

            Image explanationPlate = CreateImage(
                drawer,
                "DetailExplanationPlate",
                InkSoft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(30f, -231f),
                new Vector2(920f, 112f));
            refs.DetailExplanation = CreateText(
                explanationPlate.rectTransform,
                "DetailExplanation",
                "로컬 UI 검토용 설명입니다. 실제 공지나 서비스 응답을 뜻하지 않습니다.",
                mediumFont,
                18f,
                White,
                TextAlignmentOptions.MidlineLeft,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-40f, -24f));

            LobbyOperationsReviewDispositionRowKind[] kinds =
            {
                LobbyOperationsReviewDispositionRowKind.Production,
                LobbyOperationsReviewDispositionRowKind.Service,
                LobbyOperationsReviewDispositionRowKind.Account,
                LobbyOperationsReviewDispositionRowKind.ServerClock,
                LobbyOperationsReviewDispositionRowKind.Schedule,
                LobbyOperationsReviewDispositionRowKind.Progress,
                LobbyOperationsReviewDispositionRowKind.Attention,
                LobbyOperationsReviewDispositionRowKind.Action
            };
            string[] labels =
            {
                "PRODUCTION",
                "SERVICE",
                "ACCOUNT",
                "SERVER CLOCK",
                "SCHEDULE",
                "PROGRESS",
                "ATTENTION",
                "ACTION"
            };
            refs.DispositionRows =
                new LobbyOperationsReviewController.DispositionRowBinding[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
            {
                DispositionRowRefs row = CreateDispositionRow(
                    drawer,
                    $"Disposition_{labels[i].Replace(" ", string.Empty)}",
                    labels[i],
                    new Vector2(30f, -359f - i * 55f),
                    new Vector2(920f, 49f),
                    mediumFont,
                    semiBoldFont);
                refs.DispositionRows[i] =
                    new LobbyOperationsReviewController.DispositionRowBinding(
                        kinds[i],
                        row.Root,
                        row.Label,
                        row.Value);
            }

            refs.DetailReviewButton = CreateButton(
                drawer,
                "DetailReviewFixtureButton",
                "REVIEW THIS FIXTURE  ›",
                semiBoldFont,
                20f,
                Ink,
                Cyan,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 22f),
                new Vector2(620f, 58f));
        }

        private static void BuildConfirm(
            ReviewUiRefs refs,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            RectTransform root = refs.ConfirmPanel.GetComponent<RectTransform>();
            CreateBlockingScrim(root, "ConfirmScrim", 0.80f);
            RectTransform drawer = CreateDrawer(root, "ReviewConfirmDrawer", 860f, 820f);
            refs.ConfirmBackButton = CreateButton(
                drawer,
                "ConfirmBackButton",
                "‹  DETAIL",
                semiBoldFont,
                18f,
                White,
                InkSoft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -28f),
                new Vector2(160f, 54f));
            refs.ConfirmCloseButton = CreateButton(
                drawer,
                "ConfirmCloseButton",
                "CLOSE",
                semiBoldFont,
                17f,
                White,
                InkSoft,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-28f, -28f),
                new Vector2(132f, 54f));
            CreateText(
                drawer,
                "ConfirmEyebrow",
                "OPS-01 / LOCAL REVIEW ACKNOWLEDGEMENT",
                mediumFont,
                16f,
                Cyan,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(42f, -120f),
                new Vector2(760f, 34f));
            refs.ConfirmTitle = CreateText(
                drawer,
                "ConfirmTitle",
                "운영 안내 샘플 검토",
                semiBoldFont,
                38f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(42f, -166f),
                new Vector2(770f, 62f));
            refs.ConfirmSummary = CreateText(
                drawer,
                "ConfirmSummary",
                "이 확인은 로컬 UI 경로를 살펴봤다는 검토 신호만 남깁니다. "
                + "읽음, 수령, 보상, 저장, 서비스 요청은 수행하지 않습니다.",
                mediumFont,
                22f,
                White,
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(42f, -250f),
                new Vector2(770f, 170f));
            refs.ConfirmStatus = CreateText(
                drawer,
                "ConfirmStatus",
                "READY / LOCAL SESSION ONLY",
                semiBoldFont,
                18f,
                Amber,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -80f),
                new Vector2(740f, 44f));
            refs.ConfirmAcknowledgeButton = CreateButton(
                drawer,
                "ConfirmAcknowledgeButton",
                "ACKNOWLEDGE REVIEW",
                semiBoldFont,
                22f,
                Ink,
                Cyan,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 126f),
                new Vector2(600f, 72f));
            CreateText(
                drawer,
                "ConfirmBoundaryNote",
                "Exact once per local review session. No persistence or product mutation.",
                mediumFont,
                15f,
                Muted,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 55f),
                new Vector2(760f, 34f));
        }

        private static EntryRowRefs CreateEntryRow(
            RectTransform parent,
            string name,
            string title,
            string status,
            Vector2 position,
            Vector2 size,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            Image body = CreateImage(
                parent,
                name,
                PanelSoft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                position,
                size);
            body.raycastTarget = true;
            Button button = body.gameObject.AddComponent<Button>();
            button.targetGraphic = body;
            CanvasGroup group = body.gameObject.AddComponent<CanvasGroup>();
            CreateImage(
                body.rectTransform,
                "Accent",
                Cyan,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(4f, 0f),
                new Vector2(8f, 0f));
            TMP_Text titleText = CreateText(
                body.rectTransform,
                "Title",
                title,
                semiBoldFont,
                24f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(32f, -25f),
                new Vector2(size.x - 74f, 42f));
            TMP_Text statusText = CreateText(
                body.rectTransform,
                "SourceStatus",
                status,
                mediumFont,
                14f,
                CyanSoft,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(32f, 24f),
                new Vector2(size.x - 74f, 34f));
            CreateText(
                body.rectTransform,
                "Chevron",
                "›",
                semiBoldFont,
                32f,
                Cyan,
                TextAlignmentOptions.Center,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-30f, 0f),
                new Vector2(34f, 50f));
            return new EntryRowRefs(button, group, titleText, statusText);
        }

        private static DispositionRowRefs CreateDispositionRow(
            RectTransform parent,
            string name,
            string label,
            Vector2 position,
            Vector2 size,
            TMP_FontAsset mediumFont,
            TMP_FontAsset semiBoldFont)
        {
            Image body = CreateImage(
                parent,
                name,
                new Color(InkSoft.r, InkSoft.g, InkSoft.b, 0.88f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                position,
                size);
            TMP_Text labelText = CreateText(
                body.rectTransform,
                "Label",
                label,
                semiBoldFont,
                13f,
                CyanSoft,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(18f, 0f),
                new Vector2(210f, 0f));
            TMP_Text valueText = CreateText(
                body.rectTransform,
                "Value",
                string.Empty,
                mediumFont,
                15f,
                White,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(238f, 0f),
                new Vector2(-258f, 0f));
            RectTransform valueRect = valueText.rectTransform;
            valueRect.offsetMin = new Vector2(238f, 0f);
            valueRect.offsetMax = new Vector2(-18f, 0f);
            return new DispositionRowRefs(body.gameObject, labelText, valueText);
        }

        private static RectTransform CreateDrawer(
            RectTransform parent,
            string name,
            float width,
            float height)
        {
            Image image = CreateImage(
                parent,
                name,
                Panel,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-24f, -34f),
                new Vector2(width, height));
            image.raycastTarget = true;
            CreateImage(
                image.rectTransform,
                "DrawerAccent",
                Cyan,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(3f, 0f),
                new Vector2(6f, 0f));
            return image.rectTransform;
        }

        private static void CreateBlockingScrim(
            RectTransform parent,
            string name,
            float alpha = 0.62f)
        {
            Image scrim = CreateImage(
                parent,
                name,
                new Color(0.002f, 0.008f, 0.016f, alpha),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            scrim.raycastTarget = true;
        }

        private static void ConfigureController(
            LobbyOperationsReviewController controller,
            LobbyOperationsReviewProfile profile,
            ReviewUiRefs ui)
        {
            controller.ConfigureCore(profile);
            controller.ConfigurePanels(
                ui.ClosedPanel,
                ui.DirectoryPanel,
                ui.DetailPanel,
                ui.ConfirmPanel);
            controller.ConfigureClosedView(
                ui.ClosedReviewLabel,
                ui.ClosedStatus,
                ui.OpenButton);
            controller.ConfigureDirectoryView(
                ui.DirectoryTitle,
                ui.DirectoryStatus,
                ui.DirectoryBackButton,
                ui.DirectoryCloseButton,
                ui.EntryBindings);
            controller.ConfigureDetailView(
                ui.DetailKind,
                ui.DetailTitle,
                ui.DetailExplanation,
                ui.DetailStatus,
                ui.DispositionRows,
                ui.DetailBackButton,
                ui.DetailCloseButton,
                ui.DetailReviewButton);
            controller.ConfigureConfirmationView(
                ui.ConfirmTitle,
                ui.ConfirmSummary,
                ui.ConfirmStatus,
                ui.ConfirmBackButton,
                ui.ConfirmCloseButton,
                ui.ConfirmAcknowledgeButton);
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

            if (scene.GetRootGameObjects().Length != 1)
            {
                issues.Add(
                    $"OPS-01 expects one authored scene root; found "
                    + $"{scene.GetRootGameObjects().Length}.");
            }

            if (EditorBuildSettings.scenes.Any(
                    entry => entry.enabled
                        && string.Equals(entry.path, ScenePath, StringComparison.Ordinal)))
            {
                issues.Add("Review scene must remain outside enabled Build Settings.");
            }

            LobbyOperationsReviewProfile profile =
                AssetDatabase.LoadAssetAtPath<LobbyOperationsReviewProfile>(ProfilePath);
            if (profile == null)
            {
                issues.Add("OPS-01 review profile is missing or has the wrong asset type.");
            }
            else if (!profile.TryValidate(out string profileError))
            {
                issues.Add("OPS-01 review profile is invalid: " + profileError);
            }
            else
            {
                ValidateProfileComposition(profile, issues);
            }

            LobbyOperationsReviewController[] controllers =
                FindComponentsInScene<LobbyOperationsReviewController>(scene);
            if (controllers.Length != 1)
            {
                issues.Add($"Review scene needs exactly one controller; found {controllers.Length}.");
            }
            else
            {
                ValidateController(controllers[0], profile, issues);
            }

            ValidateSceneInfrastructure(scene, issues);
            ValidateBackground(scene, issues);
            ValidateButtons(scene, issues);
            ValidateForbiddenOwnership(scene, issues);
            ValidatePanelGeometry(scene, issues);
            return issues;
        }

        private static void ValidateProfileComposition(
            LobbyOperationsReviewProfile profile,
            List<string> issues)
        {
            string[] expectedIds =
            {
                LobbyOperationsReviewProfile.NoticeEntryId,
                LobbyOperationsReviewProfile.MailboxEntryId,
                LobbyOperationsReviewProfile.MissionsEntryId,
                LobbyOperationsReviewProfile.EventCalendarEntryId
            };
            LobbyOperationsReviewEntryKind[] expectedKinds =
            {
                LobbyOperationsReviewEntryKind.Notice,
                LobbyOperationsReviewEntryKind.Mailbox,
                LobbyOperationsReviewEntryKind.Missions,
                LobbyOperationsReviewEntryKind.EventCalendar
            };
            if (profile.EntryCount != expectedIds.Length)
            {
                issues.Add(
                    $"OPS-01 requires exactly {expectedIds.Length} entries; found "
                    + $"{profile.EntryCount}.");
                return;
            }

            for (int i = 0; i < expectedIds.Length; i++)
            {
                LobbyOperationsReviewProfile.EntryDefinition entry = profile.GetEntry(i);
                if (entry == null
                    || !string.Equals(entry.EntryId, expectedIds[i], StringComparison.Ordinal)
                    || entry.Kind != expectedKinds[i])
                {
                    issues.Add(
                        $"OPS-01 profile entry {i} must be `{expectedIds[i]}` / "
                        + $"`{expectedKinds[i]}` in stable order.");
                }
            }
        }

        private static void ValidateController(
            LobbyOperationsReviewController controller,
            LobbyOperationsReviewProfile profile,
            List<string> issues)
        {
            var serialized = new SerializedObject(controller);
            ValidateObjectReference(serialized, "profile", profile, issues);
            string[] requiredObjectReferences =
            {
                "closedPanel",
                "directoryPanel",
                "detailPanel",
                "confirmPanel",
                "closedReviewLabelText",
                "closedStatusText",
                "closedOpenButton",
                "directoryTitleText",
                "directoryStatusText",
                "directoryBackButton",
                "directoryCloseButton",
                "detailKindText",
                "detailTitleText",
                "detailExplanationText",
                "detailStatusText",
                "detailBackButton",
                "detailCloseButton",
                "detailReviewCtaButton",
                "confirmTitleText",
                "confirmSummaryText",
                "confirmStatusText",
                "confirmBackButton",
                "confirmCloseButton",
                "confirmAcknowledgeButton"
            };
            foreach (string propertyName in requiredObjectReferences)
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null || property.objectReferenceValue == null)
                {
                    issues.Add($"OPS-01 controller is missing `{propertyName}`.");
                }
            }

            if (!controller.HasExactEntryBindings
                || controller.EntryBindingCount != LobbyOperationsReviewProfile.RequiredEntryCount)
            {
                issues.Add("OPS-01 controller must bind the exact four profile entries.");
            }
            else
            {
                string[] expectedIds =
                {
                    LobbyOperationsReviewProfile.NoticeEntryId,
                    LobbyOperationsReviewProfile.MailboxEntryId,
                    LobbyOperationsReviewProfile.MissionsEntryId,
                    LobbyOperationsReviewProfile.EventCalendarEntryId
                };
                for (int i = 0; i < expectedIds.Length; i++)
                {
                    LobbyOperationsReviewController.EntryButtonBinding binding =
                        controller.GetEntryBinding(i);
                    if (binding == null
                        || !string.Equals(binding.EntryId, expectedIds[i], StringComparison.Ordinal)
                        || binding.Button == null
                        || binding.CanvasGroup == null
                        || binding.TitleText == null
                        || binding.SourceStatusText == null)
                    {
                        issues.Add(
                            $"OPS-01 directory binding {i} is incomplete or not `{expectedIds[i]}`.");
                    }
                }
            }

            LobbyOperationsReviewDispositionRowKind[] expectedRows =
            {
                LobbyOperationsReviewDispositionRowKind.Production,
                LobbyOperationsReviewDispositionRowKind.Service,
                LobbyOperationsReviewDispositionRowKind.Account,
                LobbyOperationsReviewDispositionRowKind.ServerClock,
                LobbyOperationsReviewDispositionRowKind.Schedule,
                LobbyOperationsReviewDispositionRowKind.Progress,
                LobbyOperationsReviewDispositionRowKind.Attention,
                LobbyOperationsReviewDispositionRowKind.Action
            };
            if (controller.DispositionRowCount != expectedRows.Length)
            {
                issues.Add(
                    $"OPS-01 controller needs {expectedRows.Length} separate disposition rows; "
                    + $"found {controller.DispositionRowCount}.");
            }
            else
            {
                for (int i = 0; i < expectedRows.Length; i++)
                {
                    LobbyOperationsReviewController.DispositionRowBinding binding =
                        controller.GetDispositionRowBinding(i);
                    if (binding == null
                        || binding.RowKind != expectedRows[i]
                        || binding.RowRoot == null
                        || binding.LabelText == null
                        || binding.ValueText == null)
                    {
                        issues.Add(
                            $"OPS-01 disposition row {i} is incomplete or not "
                            + $"`{expectedRows[i]}`.");
                    }
                }
            }

            if (controller.ReviewAcknowledgedEvent == null
                || controller.ReviewAcknowledgedEvent.GetPersistentEventCount() != 0)
            {
                issues.Add(
                    "OPS-01 review acknowledgement must exist with zero authored persistent callbacks.");
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
                issues.Add($"OPS-01 needs exactly one Canvas; found {canvases.Length}.");
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
                    || canvas.worldCamera != cameras.FirstOrDefault()
                    || canvas.GetComponent<GraphicRaycaster>() == null)
                {
                    issues.Add(
                        "OPS-01 Canvas camera/raycaster/1920x1080 responsive baseline is incomplete.");
                }
            }

            if (cameras.Length != 1
                || listeners.Length != 1
                || cameras[0].tag != "MainCamera")
            {
                issues.Add("OPS-01 needs one MainCamera and one AudioListener.");
            }

            if (eventSystems.Length != 1
                || eventSystems[0].GetComponent<InputSystemUIInputModule>() == null)
            {
                issues.Add("OPS-01 needs one InputSystem EventSystem.");
            }

            if (safeAreas.Length != 1)
            {
                issues.Add($"OPS-01 needs one UISafeAreaRoot; found {safeAreas.Length}.");
            }
            else
            {
                RectTransform safeRect = safeAreas[0].transform as RectTransform;
                var safeSerialized = new SerializedObject(safeAreas[0]);
                bool valid = safeRect != null
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
                if (!valid)
                {
                    issues.Add(
                        "OPS-01 safe area must serialize InsetsOnly/24px on a stretched root.");
                }
            }

            if (responsiveRoots.Length != 1)
            {
                issues.Add(
                    $"OPS-01 needs one UIResponsiveRoot; found {responsiveRoots.Length}.");
            }
            else
            {
                var responsiveSerialized = new SerializedObject(responsiveRoots[0]);
                responsiveSerialized.UpdateIfRequiredOrScript();
                UnityEngine.Object serializedCatalog =
                    responsiveSerialized.FindProperty("catalog")?.objectReferenceValue;
                string serializedCatalogPath = AssetDatabase.GetAssetPath(serializedCatalog);
                string serializedCatalogGuid = string.IsNullOrWhiteSpace(serializedCatalogPath)
                    ? string.Empty
                    : AssetDatabase.AssetPathToGUID(serializedCatalogPath);
                if (serializedCatalog == null
                    || serializedCatalog.GetType() != typeof(UIResponsiveLayoutCatalog)
                    || !string.Equals(
                        serializedCatalogPath,
                        ResponsiveCatalogPath,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        serializedCatalogGuid,
                        ResponsiveCatalogGuid,
                        StringComparison.Ordinal)
                    || responsiveSerialized.FindProperty("canvasScaler")?.objectReferenceValue
                        != canvases.FirstOrDefault()?.GetComponent<CanvasScaler>()
                    || responsiveSerialized.FindProperty("safeAreaRoot")?.objectReferenceValue
                        != safeAreas.FirstOrDefault()
                    || responsiveSerialized.FindProperty("applyCanvasScaler")?.boolValue != true)
                {
                    issues.Add(
                        "OPS-01 UIResponsiveRoot references are incomplete after scene roundtrip "
                        + $"(catalog path=`{serializedCatalogPath}`, guid=`{serializedCatalogGuid}`).");
                }
            }

            ValidateResponsiveCatalogYamlRoundTrip(scene, issues);
        }

        private static void ValidateResponsiveCatalogYamlRoundTrip(
            Scene scene,
            List<string> issues)
        {
            if (!scene.IsValid()
                || !string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                return;
            }

            string absoluteScenePath = AssetPathToAbsolutePath(ScenePath);
            if (!File.Exists(absoluteScenePath))
            {
                issues.Add("OPS-01 scene YAML is unavailable for catalog roundtrip proof.");
                return;
            }

            string catalogLine = File.ReadLines(absoluteScenePath)
                .Select(line => line.Trim())
                .FirstOrDefault(line => line.StartsWith("catalog:", StringComparison.Ordinal));
            if (string.IsNullOrWhiteSpace(catalogLine)
                || catalogLine.IndexOf(
                    "fileID: 11400000",
                    StringComparison.Ordinal) < 0
                || catalogLine.IndexOf(
                    "guid: " + ResponsiveCatalogGuid,
                    StringComparison.Ordinal) < 0)
            {
                issues.Add(
                    "OPS-01 saved scene YAML does not serialize UIResponsiveRoot.catalog with "
                    + $"protected GUID `{ResponsiveCatalogGuid}` (line=`{catalogLine}`).");
            }
        }

        private static void ValidateBackground(Scene scene, List<string> issues)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            Image background = FindComponentsInScene<Image>(scene).FirstOrDefault(
                image => string.Equals(
                    image.gameObject.name,
                    "NeutralLobbyBackground",
                    StringComparison.Ordinal));
            if (sprite == null
                || background == null
                || background.sprite != sprite
                || background.GetComponent<AspectRatioFitter>() == null)
            {
                issues.Add(
                    "OPS-01 neutral Lobby background is missing or not bound without importer changes.");
            }
        }

        private static void ValidateButtons(Scene scene, List<string> issues)
        {
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
                        $"Button `{button.gameObject.name}` has authored persistent callbacks.");
                }
            }
        }

        private static void ValidateForbiddenOwnership(Scene scene, List<string> issues)
        {
            if (FindComponentsInScene<UISceneFlowRouter>(scene).Length != 0
                || FindComponentsInScene<UISceneRouteLoader>(scene).Length != 0
                || FindComponentsInScene<UIPanelRouter>(scene).Length != 0)
            {
                issues.Add("OPS-01 must not contain a router, route loader, or UIPanelRouter.");
            }

            if (StageRunRuntime.HasActiveContext)
            {
                issues.Add("OPS-01 validation observed an active StageRun context.");
            }

            var allowedMonoBehaviourTypes = new HashSet<Type>
            {
                typeof(LobbyOperationsReviewController),
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
                "RuntimeBuilder",
                "HierarchyBuilder"
            };
            foreach (MonoBehaviour behaviour in FindComponentsInScene<MonoBehaviour>(scene))
            {
                if (behaviour == null)
                {
                    issues.Add("OPS-01 contains a missing MonoBehaviour script.");
                    continue;
                }

                Type behaviourType = behaviour.GetType();
                string fullName = behaviourType.FullName ?? behaviourType.Name;
                if (!allowedMonoBehaviourTypes.Contains(behaviourType))
                {
                    issues.Add(
                        $"OPS-01 deterministic scene contains non-allowlisted MonoBehaviour "
                        + $"`{fullName}`.");
                }

                if (forbiddenTypeTokens.Any(
                        token => fullName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    issues.Add($"OPS-01 contains forbidden ownership component `{fullName}`.");
                }
            }

            string[] forbiddenObjectTokens =
            {
                "Currency",
                "Stamina",
                "UnreadCount",
                "RewardAmount",
                "ProgressValue",
                "EventTimer",
                "AccountId",
                "ProfileLevel"
            };
            foreach (Transform transform in FindComponentsInScene<Transform>(scene))
            {
                if (transform.GetComponents<Component>().Any(component => component == null))
                {
                    issues.Add(
                        $"OPS-01 object `{transform.gameObject.name}` contains a missing script slot.");
                }

                if (forbiddenObjectTokens.Any(
                        token => transform.gameObject.name.IndexOf(
                            token,
                            StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    issues.Add(
                        $"OPS-01 contains forbidden product-data surface "
                        + $"`{transform.gameObject.name}`.");
                }
            }
        }

        private static void ValidatePanelGeometry(Scene scene, List<string> issues)
        {
            string[] expectedPanels =
            {
                "DrawerClosedPanel",
                "DirectoryPanel",
                "EntryDetailPanel",
                "ReviewConfirmPanel"
            };
            foreach (string panelName in expectedPanels)
            {
                CanvasGroup[] matches = FindComponentsInScene<CanvasGroup>(scene)
                    .Where(group => string.Equals(
                        group.gameObject.name,
                        panelName,
                        StringComparison.Ordinal))
                    .ToArray();
                if (matches.Length != 1)
                {
                    issues.Add(
                        $"OPS-01 needs exactly one `{panelName}` CanvasGroup; found "
                        + $"{matches.Length}.");
                }
            }

            string[] drawers =
            {
                "DirectoryDrawer",
                "EntryDetailDrawer",
                "ReviewConfirmDrawer"
            };
            foreach (string drawerName in drawers)
            {
                RectTransform rect = FindComponentsInScene<RectTransform>(scene)
                    .FirstOrDefault(candidate => string.Equals(
                        candidate.gameObject.name,
                        drawerName,
                        StringComparison.Ordinal));
                if (rect == null
                    || !Mathf.Approximately(rect.anchorMin.x, 1f)
                    || !Mathf.Approximately(rect.anchorMax.x, 1f)
                    || rect.rect.width < 720f
                    || rect.rect.width > 1000f)
                {
                    issues.Add(
                        $"OPS-01 `{drawerName}` must remain a right-anchored fixed-width drawer.");
                }
            }

            // The authored disposition rows are vertically adjacent and must not overlap.
            RectTransform[] rows = FindComponentsInScene<RectTransform>(scene)
                .Where(rect => rect.gameObject.name.StartsWith(
                    "Disposition_",
                    StringComparison.Ordinal))
                .OrderByDescending(rect => rect.anchoredPosition.y)
                .ToArray();
            if (rows.Length != 8)
            {
                issues.Add($"OPS-01 needs eight disposition rows; found {rows.Length}.");
            }
            for (int i = 1; i < rows.Length; i++)
            {
                if (CalculateWorldRect(rows[i - 1]).Overlaps(CalculateWorldRect(rows[i])))
                {
                    issues.Add(
                        $"OPS-01 disposition rows `{rows[i - 1].name}` and "
                        + $"`{rows[i].name}` overlap.");
                }
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
            text.richText = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            // Dynamic shared TMP assets must not gain the ellipsis glyph merely because a
            // review label chooses Ellipsis overflow. QA rejects overflow instead.
            text.overflowMode = TextOverflowModes.Truncate;
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
            colors.disabledColor = new Color(0.38f, 0.43f, 0.49f, 0.62f);
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
                    "OPS-01 responsive root, CanvasScaler, and safe-area root are required.");
            }

            UIResponsiveLayoutCatalog persistentCatalog =
                AssetDatabase.LoadAssetAtPath<UIResponsiveLayoutCatalog>(ResponsiveCatalogPath);
            if (persistentCatalog == null)
            {
                throw new InvalidOperationException(
                    $"OPS-01 responsive catalog load failed at `{ResponsiveCatalogPath}`.");
            }

            UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(
                ResponsiveCatalogPath);
            if (mainAsset == null
                || mainAsset.GetType() != typeof(UIResponsiveLayoutCatalog)
                || mainAsset != persistentCatalog)
            {
                throw new InvalidOperationException(
                    $"OPS-01 responsive catalog main asset is missing, wrong-type, or differs "
                    + $"from the typed load at `{ResponsiveCatalogPath}`.");
            }

            if (!AssetDatabase.Contains(persistentCatalog))
            {
                throw new InvalidOperationException(
                    "OPS-01 responsive catalog is not contained by AssetDatabase.");
            }

            string persistentPath = AssetDatabase.GetAssetPath(persistentCatalog);
            if (!string.Equals(
                    persistentPath,
                    ResponsiveCatalogPath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"OPS-01 responsive catalog resolved path `{persistentPath}`, expected "
                    + $"`{ResponsiveCatalogPath}`.");
            }

            string persistentGuid = AssetDatabase.AssetPathToGUID(ResponsiveCatalogPath);
            if (string.IsNullOrWhiteSpace(persistentGuid)
                || !string.Equals(
                    persistentGuid,
                    ResponsiveCatalogGuid,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"OPS-01 responsive catalog GUID `{persistentGuid}` does not match protected "
                    + $"GUID `{ResponsiveCatalogGuid}`.");
            }

            // Reflection gives the managed component an immediate, type-safe value even when a
            // newly-created scene component has not yet synchronized its serialized backing
            // store. SerializedObject then records the durable scene representation.
            SetResponsiveRootPrivateField(responsiveRoot, "catalog", persistentCatalog);
            SetResponsiveRootPrivateField(responsiveRoot, "canvasScaler", scaler);
            SetResponsiveRootPrivateField(responsiveRoot, "safeAreaRoot", safeAreaRoot);
            SetResponsiveRootPrivateField(responsiveRoot, "breakpointText", null);
            SetResponsiveRootPrivateField(responsiveRoot, "applyCanvasScaler", true);

            var serialized = new SerializedObject(responsiveRoot);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty catalogProperty = serialized.FindProperty("catalog");
            SerializedProperty scalerProperty = serialized.FindProperty("canvasScaler");
            SerializedProperty safeAreaProperty = serialized.FindProperty("safeAreaRoot");
            SerializedProperty breakpointProperty = serialized.FindProperty("breakpointText");
            SerializedProperty applyProperty = serialized.FindProperty("applyCanvasScaler");
            if (catalogProperty == null
                || scalerProperty == null
                || safeAreaProperty == null
                || breakpointProperty == null
                || applyProperty == null)
            {
                throw new InvalidOperationException(
                    "OPS-01 UIResponsiveRoot serialized schema is incomplete.");
            }

            catalogProperty.objectReferenceValue = persistentCatalog;
            scalerProperty.objectReferenceValue = scaler;
            safeAreaProperty.objectReferenceValue = safeAreaRoot;
            breakpointProperty.objectReferenceValue = null;
            applyProperty.boolValue = true;
            serialized.ApplyModifiedProperties();

            // Keep the managed fields and serialized properties in agreement after Apply.
            SetResponsiveRootPrivateField(responsiveRoot, "catalog", persistentCatalog);
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
                persistentCatalog,
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
                    $"OPS-01 UIResponsiveRoot private field `{fieldName}` is unavailable.");
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
            if (responsiveRoot == null)
            {
                throw new InvalidOperationException(
                    $"OPS-01 UIResponsiveRoot is missing {checkpoint}.");
            }

            var serialized = new SerializedObject(responsiveRoot);
            serialized.UpdateIfRequiredOrScript();
            var issues = new List<string>();
            UnityEngine.Object serializedCatalog =
                serialized.FindProperty("catalog")?.objectReferenceValue;
            string serializedCatalogPath = AssetDatabase.GetAssetPath(serializedCatalog);
            string serializedCatalogGuid = string.IsNullOrWhiteSpace(serializedCatalogPath)
                ? string.Empty
                : AssetDatabase.AssetPathToGUID(serializedCatalogPath);
            if (serializedCatalog == null
                || serializedCatalog.GetType() != typeof(UIResponsiveLayoutCatalog)
                || !string.Equals(
                    serializedCatalogPath,
                    ResponsiveCatalogPath,
                    StringComparison.Ordinal)
                || !string.Equals(
                    serializedCatalogGuid,
                    ResponsiveCatalogGuid,
                    StringComparison.Ordinal))
            {
                issues.Add(
                    $"catalog(type={serializedCatalog?.GetType().Name ?? "null"}, "
                    + $"path={serializedCatalogPath}, guid={serializedCatalogGuid})");
            }
            if (serialized.FindProperty("canvasScaler")?.objectReferenceValue != scaler)
            {
                issues.Add("canvasScaler");
            }
            if (serialized.FindProperty("safeAreaRoot")?.objectReferenceValue != safeAreaRoot)
            {
                issues.Add("safeAreaRoot");
            }
            if (serialized.FindProperty("breakpointText")?.objectReferenceValue != null)
            {
                issues.Add("breakpointText");
            }
            if (serialized.FindProperty("applyCanvasScaler")?.boolValue != true)
            {
                issues.Add("applyCanvasScaler");
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    $"OPS-01 UIResponsiveRoot references are incomplete {checkpoint}: "
                    + string.Join(", ", issues)
                    + ". "
                    + "refusing to save a partial review scene.");
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

        private static Rect CalculateWorldRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return Rect.MinMaxRect(
                corners.Min(corner => corner.x),
                corners.Min(corner => corner.y),
                corners.Max(corner => corner.x),
                corners.Max(corner => corner.y));
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
                issues.Add($"OPS-01 controller `{propertyName}` is missing or stale.");
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
                    $"Required {typeof(T).Name} at `{path}` is {occupancy}; refusing fallback or overwrite.");
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

            File.WriteAllText(absolutePath, normalized, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
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
                    "OPS-01 canonical hash boundary is incomplete:\n- "
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
                        $"Canonical boundary file disappeared during OPS-01 setup: `{pair.Key}`.");
                }
                else if (pair.Value.Length != after.Length
                    || !string.Equals(pair.Value.Sha256, after.Sha256, StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Canonical boundary file changed during OPS-01 setup: `{pair.Key}`.");
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
            public CanvasGroup ClosedPanel;
            public CanvasGroup DirectoryPanel;
            public CanvasGroup DetailPanel;
            public CanvasGroup ConfirmPanel;
            public TMP_Text ClosedReviewLabel;
            public TMP_Text ClosedStatus;
            public Button OpenButton;
            public TMP_Text DirectoryTitle;
            public TMP_Text DirectoryStatus;
            public Button DirectoryBackButton;
            public Button DirectoryCloseButton;
            public LobbyOperationsReviewController.EntryButtonBinding[] EntryBindings;
            public TMP_Text DetailKind;
            public TMP_Text DetailTitle;
            public TMP_Text DetailExplanation;
            public TMP_Text DetailStatus;
            public LobbyOperationsReviewController.DispositionRowBinding[] DispositionRows;
            public Button DetailBackButton;
            public Button DetailCloseButton;
            public Button DetailReviewButton;
            public TMP_Text ConfirmTitle;
            public TMP_Text ConfirmSummary;
            public TMP_Text ConfirmStatus;
            public Button ConfirmBackButton;
            public Button ConfirmCloseButton;
            public Button ConfirmAcknowledgeButton;
        }

        private readonly struct EntryRowRefs
        {
            public EntryRowRefs(
                Button button,
                CanvasGroup group,
                TMP_Text title,
                TMP_Text status)
            {
                Button = button;
                Group = group;
                Title = title;
                Status = status;
            }

            public Button Button { get; }
            public CanvasGroup Group { get; }
            public TMP_Text Title { get; }
            public TMP_Text Status { get; }
        }

        private readonly struct DispositionRowRefs
        {
            public DispositionRowRefs(GameObject root, TMP_Text label, TMP_Text value)
            {
                Root = root;
                Label = label;
                Value = value;
            }

            public GameObject Root { get; }
            public TMP_Text Label { get; }
            public TMP_Text Value { get; }
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
    }
}
