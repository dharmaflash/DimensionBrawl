using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.ChapterHubReview;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.ChapterHubReview
{
    /// <summary>
    /// Play-mode visual QA harness for the independent Olympus Chapter Hub review scene.
    ///
    /// Batch invocation must omit Unity's -quit argument because this runner owns the asynchronous
    /// play-mode lifecycle and exits the Editor with a verified code when capture finishes:
    /// -executeMethod DimensionBrawl.Editor.ChapterHubReview.OlympusChapterHubReviewVisualQaCapture.RunBatchCaptureAndVerify
    /// </summary>
    [InitializeOnLoad]
    public static class OlympusChapterHubReviewVisualQaCapture
    {
        public const string ScenePath = OlympusChapterHubReviewSetup.ScenePath;
        public const string OutputDirectory =
            "C:/tmp/DimensionBrawl-OlympusChapterHubReview-QA";

        private const string ManifestPath = OutputDirectory + "/capture-manifest.json";
        private const string ReportPath = OutputDirectory + "/capture-report.md";
        private const string SessionPrefix =
            "DimensionBrawl.OlympusChapterHubReview.VisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";
        private const int InitialWarmupFrames = 8;
        private const int StateSettleFrames = 4;
        private const int ExpectedCaptureCount = 18;
        private const double LaunchTimeoutSeconds = 120d;

        private static readonly string[] UnverifiedRowPropertyNames =
        {
            "detailRecommendedPowerRow",
            "detailLoadoutRow",
            "detailDurationRow",
            "detailThreatRow",
            "detailSummonRow",
            "detailRewardRow"
        };

        private enum RunnerPhase
        {
            None = 0,
            RequestedPlayMode = 1,
            Capturing = 2,
            SuccessAwaitingEditMode = 3,
            FailureAwaitingEditMode = 4
        }

        private enum ReviewCaptureState
        {
            ChapterHub = 0,
            StageMap = 1,
            CanonicalDetail = 2,
            InProductionDetail = 3,
            AnnouncedDetail = 4,
            ReviewConfirm = 5
        }

        private static readonly CapturePlan[] Plans = BuildCapturePlans();
        private static readonly List<CaptureRecord> Records = new List<CaptureRecord>();

        private static OlympusChapterHubReviewController controller;
        private static Camera reviewCamera;
        private static Canvas reviewCanvas;
        private static CanvasScaler reviewCanvasScaler;
        private static UISafeAreaRoot safeAreaRoot;
        private static UIStageCatalog stageCatalog;
        private static TMP_Text hubStatusText;
        private static TMP_Text mapStatusText;
        private static TMP_Text detailStatusText;
        private static TMP_Text detailTitleText;
        private static TMP_Text detailObjectiveText;
        private static TMP_Text detailCombatLessonText;
        private static TMP_Text detailStoryText;
        private static TMP_Text detailSegmentText;
        private static TMP_Text confirmStatusText;
        private static TMP_Text confirmTitleText;
        private static TMP_Text confirmSummaryText;
        private static GameObject[] unverifiedRows = Array.Empty<GameObject>();
        private static int planIndex;
        private static int readyAtFrame;
        private static bool statePrepared;
        private static bool runtimeInitialized;
        private static int observedConfirmationEventCount;
        private static int confirmationEventBaseline;
        private static string observedConfirmationCatalogEntryId = string.Empty;
        private static bool firstConfirmationAccepted;
        private static bool duplicateConfirmationAccepted;

        static OlympusChapterHubReviewVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("Tools/DimensionBrawl/Review/Capture Olympus Chapter Hub Review Visual QA")]
        public static void CaptureFromMenu()
        {
            StartCapture(exitEditorWhenFinished: false);
        }

        public static void RunBatchCaptureAndVerify()
        {
            StartCapture(exitEditorWhenFinished: true);
        }

        public static void RunBatchCapture()
        {
            RunBatchCaptureAndVerify();
        }

        private static void StartCapture(bool exitEditorWhenFinished)
        {
            try
            {
                if (SessionState.GetBool(ActiveKey, false))
                {
                    throw new InvalidOperationException(
                        "An Olympus Chapter Hub review visual QA capture is already active.");
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "Visual QA capture must be started from Edit mode.");
                }

                if (!File.Exists(AssetPathToAbsolutePath(ScenePath)))
                {
                    throw new FileNotFoundException(
                        "Generate the independent Olympus Chapter Hub review scene before capture.",
                        ScenePath);
                }

                if (EditorBuildSettings.scenes.Any(
                        scene => scene.enabled
                            && string.Equals(scene.path, ScenePath, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException(
                        "The Olympus Chapter Hub review scene must remain outside enabled Build Settings.");
                }

                if (!exitEditorWhenFinished
                    && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                ResetOutputArtifacts();
                ResetRuntimeFields();
                SessionState.SetBool(ActiveKey, true);
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));

                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log(
                    $"[OlympusChapterHubReviewVisualQA] Entering Play mode for "
                    + $"{ExpectedCaptureCount} exact-resolution captures.");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception)
            {
                HandleLaunchFailure(exception, exitEditorWhenFinished);
            }
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
                ResetRuntimeFields();
                readyAtFrame = Time.frameCount + InitialWarmupFrames;
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            RunnerPhase phase = (RunnerPhase)SessionState.GetInt(
                PhaseKey,
                (int)RunnerPhase.None);
            if ((phase == RunnerPhase.SuccessAwaitingEditMode
                    || phase == RunnerPhase.FailureAwaitingEditMode)
                && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                FinalizeEditorSession(phase == RunnerPhase.SuccessAwaitingEditMode);
                return;
            }

            if (phase == RunnerPhase.RequestedPlayMode)
            {
                if (EditorApplication.isPlaying)
                {
                    SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
                    ResetRuntimeFields();
                    readyAtFrame = Time.frameCount + InitialWarmupFrames;
                    return;
                }

                if (HasLaunchTimedOut())
                {
                    FinishWithFailure("Timed out while waiting for Play mode to start.");
                }

                return;
            }

            if (phase != RunnerPhase.Capturing
                || !EditorApplication.isPlaying
                || EditorApplication.isPaused
                || EditorApplication.isCompiling)
            {
                return;
            }

            try
            {
                TickCaptureInPlayMode();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                FinishWithFailure(exception.ToString());
            }
        }

        private static void TickCaptureInPlayMode()
        {
            if (!runtimeInitialized)
            {
                ResolveRuntimeBindings();
                runtimeInitialized = true;
                readyAtFrame = Time.frameCount + InitialWarmupFrames;
                return;
            }

            if (Time.frameCount < readyAtFrame)
            {
                return;
            }

            if (planIndex >= Plans.Length)
            {
                CompleteCaptureSet();
                return;
            }

            CapturePlan plan = Plans[planIndex];
            if (!statePrepared)
            {
                PrepareState(plan.State);
                statePrepared = true;
                readyAtFrame = Time.frameCount + StateSettleFrames;
                return;
            }

            ValidateExpectedRuntimeState(plan.State);
            CaptureRecord record = CapturePlanFrame(plan);
            Records.Add(record);
            Debug.Log(
                $"[OlympusChapterHubReviewVisualQA] CAPTURE_PASS "
                + $"{record.State} {record.Width}x{record.Height} `{record.Path}`");

            planIndex++;
            statePrepared = false;
            readyAtFrame = Time.frameCount + 1;
        }

        private static void ResolveRuntimeBindings()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()
                || !string.Equals(activeScene.path, ScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Active Play mode scene is `{activeScene.path}`, expected `{ScenePath}`.");
            }

            controller = FindSingleInScene<OlympusChapterHubReviewController>(activeScene);
            reviewCamera = FindSingleInScene<Camera>(activeScene);
            reviewCanvas = FindSingleInScene<Canvas>(activeScene);
            reviewCanvasScaler = reviewCanvas.GetComponent<CanvasScaler>()
                ?? throw new InvalidOperationException(
                    "Olympus Chapter Hub review Canvas is missing its CanvasScaler.");
            safeAreaRoot = FindSingleInScene<UISafeAreaRoot>(activeScene);

            if (reviewCanvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                throw new InvalidOperationException(
                    "Olympus Chapter Hub review CanvasScaler must use ScaleWithScreenSize.");
            }

            if (FindAllInScene<UISceneFlowRouter>(activeScene).Length != 0
                || FindAllInScene<UISceneRouteLoader>(activeScene).Length != 0)
            {
                throw new InvalidOperationException(
                    "Review scene must not contain UISceneFlowRouter or UISceneRouteLoader.");
            }

            var serialized = new SerializedObject(controller);
            serialized.UpdateIfRequiredOrScript();
            stageCatalog = RequireObjectReference<UIStageCatalog>(serialized, "stageCatalog");
            hubStatusText = RequireObjectReference<TMP_Text>(serialized, "hubStatusText");
            mapStatusText = RequireObjectReference<TMP_Text>(serialized, "mapStatusText");
            detailStatusText = RequireObjectReference<TMP_Text>(serialized, "detailStatusText");
            detailTitleText = RequireObjectReference<TMP_Text>(serialized, "detailTitleText");
            detailObjectiveText = RequireObjectReference<TMP_Text>(
                serialized,
                "detailObjectiveText");
            detailCombatLessonText = RequireObjectReference<TMP_Text>(
                serialized,
                "detailCombatLessonText");
            detailStoryText = RequireObjectReference<TMP_Text>(serialized, "detailStoryText");
            detailSegmentText = RequireObjectReference<TMP_Text>(serialized, "detailSegmentText");
            confirmStatusText = RequireObjectReference<TMP_Text>(serialized, "confirmStatusText");
            confirmTitleText = RequireObjectReference<TMP_Text>(serialized, "confirmTitleText");
            confirmSummaryText = RequireObjectReference<TMP_Text>(
                serialized,
                "confirmSummaryText");
            unverifiedRows = new GameObject[UnverifiedRowPropertyNames.Length];
            for (int i = 0; i < UnverifiedRowPropertyNames.Length; i++)
            {
                unverifiedRows[i] = RequireObjectReference<GameObject>(
                    serialized,
                    UnverifiedRowPropertyNames[i]);
            }

            controller.ReviewConfirmed -= HandleReviewConfirmed;
            controller.ReviewConfirmed += HandleReviewConfirmed;

            if (!stageCatalog.TryValidateEntryIdentities(out _)
                || !stageCatalog.TryGetStage(
                    OlympusChapterHubReviewSetup.CanonicalCatalogEntryId,
                    out _))
            {
                throw new InvalidOperationException(
                    "Canonical UIStageCatalog must retain the Olympus review entry inside a valid catalog.");
            }

            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException(
                    "Review scene entered Play mode with an active StageRun context.");
            }
        }

        private static void PrepareState(ReviewCaptureState state)
        {
            confirmationEventBaseline = observedConfirmationEventCount;
            observedConfirmationCatalogEntryId = string.Empty;
            firstConfirmationAccepted = false;
            duplicateConfirmationAccepted = false;

            RequireNavigation(controller.RestartReview(), "RestartReview", state);
            if (state == ReviewCaptureState.ChapterHub)
            {
                return;
            }

            RequireNavigation(
                controller.OpenChapterMap(OlympusChapterHubReviewSetup.ChapterId),
                "OpenChapterMap",
                state);
            if (state == ReviewCaptureState.StageMap)
            {
                return;
            }

            string stageId = ResolveStageId(state);
            RequireNavigation(controller.OpenStageDetail(stageId), "OpenStageDetail", state);
            if (state != ReviewCaptureState.ReviewConfirm)
            {
                return;
            }

            RequireNavigation(controller.OpenReviewConfirm(), "OpenReviewConfirm", state);
            firstConfirmationAccepted = controller.ConfirmSelectedStage();
            duplicateConfirmationAccepted = controller.ConfirmSelectedStage();
        }

        private static void ValidateExpectedRuntimeState(ReviewCaptureState state)
        {
            ChapterHubReviewSession session = controller.Session
                ?? throw new InvalidOperationException(
                    $"Review session is unavailable while validating {state}.");
            ChapterHubReviewPhase expectedPhase = ResolveExpectedPhase(state);
            ChapterHubReviewPanel expectedPanel = ResolveExpectedPanel(state);
            string expectedChapterId = state == ReviewCaptureState.ChapterHub
                ? string.Empty
                : OlympusChapterHubReviewSetup.ChapterId;
            string expectedStageId = ResolveExpectedSelectedStageId(state);

            if (controller.CurrentPhase != expectedPhase || session.Phase != expectedPhase)
            {
                throw new InvalidOperationException(
                    $"Expected phase {expectedPhase} for {state}; controller/session reported "
                    + $"{controller.CurrentPhase}/{session.Phase}.");
            }

            if (controller.CurrentPanel != expectedPanel)
            {
                throw new InvalidOperationException(
                    $"Expected panel {expectedPanel} for {state}; got {controller.CurrentPanel}.");
            }

            if (!string.Equals(
                    session.SelectedChapterId,
                    expectedChapterId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    session.SelectedStageId,
                    expectedStageId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Unexpected selection for {state}: chapter=`{session.SelectedChapterId}`, "
                    + $"stage=`{session.SelectedStageId}`; expected chapter=`{expectedChapterId}`, "
                    + $"stage=`{expectedStageId}`.");
            }

            ChapterHubReviewContentStatus expectedStatus = ResolveExpectedContentStatus(state);
            ChapterHubReviewProfile.StageDefinition selectedStage = session.SelectedStage;
            ChapterHubReviewContentStatus actualStatus = selectedStage?.ContentStatus
                ?? ChapterHubReviewContentStatus.None;
            if (actualStatus != expectedStatus)
            {
                throw new InvalidOperationException(
                    $"Expected content status {expectedStatus} for {state}; got {actualStatus}.");
            }

            string renderedStatus = ResolveRenderedStatusText(state);
            string expectedRenderedStatus = ResolveExpectedRenderedStatus(state);
            if (!string.Equals(renderedStatus, expectedRenderedStatus, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Expected rendered status `{expectedRenderedStatus}` for {state}; "
                    + $"got `{renderedStatus}`.");
            }

            bool projectionExpected = RequiresCanonicalProjection(state);
            UIStageRouteProjection projection = controller.CurrentProjection;
            if (projectionExpected != (projection != null))
            {
                throw new InvalidOperationException(
                    $"Projection expectation mismatch for {state}: expected={projectionExpected}, "
                    + $"resolved={projection != null}.");
            }

            if (projection != null)
            {
                ValidateCanonicalProjection(projection, state);
            }

            if (!AreUnverifiedRowsHidden())
            {
                throw new InvalidOperationException(
                    $"One or more unverified detail rows are active while capturing {state}.");
            }

            if (projection?.Briefing != null
                && !AreUnverifiedBriefingFieldsNotPresent(projection.Briefing))
            {
                throw new InvalidOperationException(
                    $"Canonical briefing exposes a field that CHUB-01 currently classifies as unverified in {state}.");
            }

            Canvas.ForceUpdateCanvases();
            if (state == ReviewCaptureState.CanonicalDetail)
            {
                ValidateVisibleTextFits("detail title", detailTitleText);
                ValidateVisibleTextFits("objective", detailObjectiveText);
                ValidateVisibleTextFits("combat lesson", detailCombatLessonText);
                ValidateVisibleTextFits("story entry", detailStoryText);
                ValidateVisibleTextFits("route segments", detailSegmentText);
            }
            else if (state == ReviewCaptureState.ReviewConfirm)
            {
                ValidateVisibleTextFits("confirmation title", confirmTitleText);
                ValidateVisibleTextFits("confirmation summary", confirmSummaryText);
            }

            bool confirmationAvailableExpected =
                state == ReviewCaptureState.CanonicalDetail;
            if (controller.IsConfirmationAvailable != confirmationAvailableExpected)
            {
                throw new InvalidOperationException(
                    $"Confirmation availability mismatch for {state}: expected="
                    + $"{confirmationAvailableExpected}, actual={controller.IsConfirmationAvailable}.");
            }

            int confirmationEventDelta =
                observedConfirmationEventCount - confirmationEventBaseline;
            if (state == ReviewCaptureState.ReviewConfirm)
            {
                if (!firstConfirmationAccepted
                    || duplicateConfirmationAccepted
                    || !session.IsConfirmationAccepted
                    || controller.ConfirmationDispatchCount != 1
                    || confirmationEventDelta != 1
                    || !string.Equals(
                        controller.LastConfirmedCatalogEntryId,
                        OlympusChapterHubReviewSetup.CanonicalCatalogEntryId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        observedConfirmationCatalogEntryId,
                        OlympusChapterHubReviewSetup.CanonicalCatalogEntryId,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "ReviewConfirm did not preserve exact-once confirmation: "
                        + $"first={firstConfirmationAccepted}, duplicate={duplicateConfirmationAccepted}, "
                        + $"sessionAccepted={session.IsConfirmationAccepted}, "
                        + $"dispatches={controller.ConfirmationDispatchCount}, "
                        + $"eventDelta={confirmationEventDelta}, "
                        + $"last=`{controller.LastConfirmedCatalogEntryId}`, "
                        + $"observed=`{observedConfirmationCatalogEntryId}`.");
                }
            }
            else if (firstConfirmationAccepted
                || duplicateConfirmationAccepted
                || session.IsConfirmationAccepted
                || controller.ConfirmationDispatchCount != 0
                || confirmationEventDelta != 0
                || !string.IsNullOrEmpty(controller.LastConfirmedCatalogEntryId))
            {
                throw new InvalidOperationException(
                    $"Non-confirm state {state} unexpectedly changed confirmation state.");
            }

            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException(
                    $"Review capture {state} unexpectedly created an active StageRun context.");
            }
        }

        private static void ValidateCanonicalProjection(
            UIStageRouteProjection projection,
            ReviewCaptureState state)
        {
            if (!string.Equals(
                    projection.CatalogEntryId,
                    OlympusChapterHubReviewSetup.CanonicalCatalogEntryId,
                    StringComparison.Ordinal)
                || projection.UiRouteId != UIRouteId.Combat
                || projection.Briefing == null
                || string.IsNullOrWhiteSpace(projection.CanonicalProjectionDigest)
                || string.IsNullOrWhiteSpace(projection.CanonicalBriefingDigest))
            {
                throw new InvalidOperationException(
                    $"Canonical projection payload is incomplete or mismatched in {state}.");
            }

            if (!stageCatalog.IsProjectionCurrent(
                    projection,
                    UIRouteId.Combat,
                    out UIStageRouteProjectionRejectReason rejectReason))
            {
                throw new InvalidOperationException(
                    $"Canonical projection is stale in {state}: {rejectReason}.");
            }
        }

        private static void ValidateVisibleTextFits(string label, TMP_Text text)
        {
            if (text == null || !text.gameObject.activeInHierarchy)
            {
                return;
            }

            text.ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
            float availableHeight = text.rectTransform.rect.height;
            float preferredHeight = text.preferredHeight;
            if (preferredHeight > availableHeight + 0.5f)
            {
                throw new InvalidOperationException(
                    $"Visible {label} text is vertically clipped: preferred="
                    + $"{preferredHeight:0.0}, available={availableHeight:0.0}.");
            }
        }

        private static CaptureRecord CapturePlanFrame(CapturePlan plan)
        {
            string path = ResolveCapturePath(plan);
            RenderTexture previousTarget = reviewCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderMode previousRenderMode = reviewCanvas.renderMode;
            Camera previousWorldCamera = reviewCanvas.worldCamera;
            float previousPlaneDistance = reviewCanvas.planeDistance;
            float previousCanvasScaleFactor = reviewCanvas.scaleFactor;
            float previousCameraAspect = reviewCamera.aspect;
            bool previousScalerEnabled = reviewCanvasScaler.enabled;
            bool previousSafeAreaEnabled = safeAreaRoot.enabled;
            RectTransform safeAreaRect = safeAreaRoot.transform as RectTransform;
            Vector2 previousSafeAnchorMin = safeAreaRect.anchorMin;
            Vector2 previousSafeAnchorMax = safeAreaRect.anchorMax;
            Vector2 previousSafeOffsetMin = safeAreaRect.offsetMin;
            Vector2 previousSafeOffsetMax = safeAreaRect.offsetMax;

            RenderTexture target = new RenderTexture(
                plan.Width,
                plan.Height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB)
            {
                name = $"OlympusChapterHubReviewQA_{plan.Width}x{plan.Height}",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Texture2D image = new Texture2D(
                plan.Width,
                plan.Height,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: false);

            try
            {
                target.Create();
                reviewCamera.targetTexture = target;
                reviewCamera.aspect = plan.Width / (float)plan.Height;
                reviewCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                reviewCanvas.worldCamera = reviewCamera;
                reviewCanvas.planeDistance = Mathf.Max(
                    reviewCamera.nearClipPlane + 0.10f,
                    0.50f);
                reviewCanvasScaler.enabled = false;
                reviewCanvas.scaleFactor = ResolveCanvasScaleFactor(
                    plan.Width,
                    plan.Height,
                    reviewCanvasScaler.referenceResolution,
                    reviewCanvasScaler.matchWidthOrHeight);
                ApplyVirtualSafeArea(safeAreaRect, plan.Width, plan.Height, 24f);

                Canvas.ForceUpdateCanvases();
                reviewCamera.Render();
                RenderTexture.active = target;
                image.ReadPixels(
                    new Rect(0f, 0f, plan.Width, plan.Height),
                    0,
                    0,
                    recalculateMipMaps: false);
                image.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                PixelAudit audit = AuditPixels(image);
                if (!audit.IsUsable)
                {
                    throw new InvalidOperationException(
                        $"Captured frame `{path}` is blank or has insufficient visual range "
                        + $"(mean={audit.MeanLuminance:0.0000}, "
                        + $"range={audit.LuminanceRange:0.0000}).");
                }

                byte[] png = image.EncodeToPNG();
                if (png == null || png.Length < 1024)
                {
                    throw new InvalidOperationException(
                        $"Captured frame `{path}` produced an invalid PNG payload.");
                }

                File.WriteAllBytes(path, png);
                return BuildCaptureRecord(plan, path, png.LongLength, audit);
            }
            finally
            {
                reviewCanvasScaler.enabled = previousScalerEnabled;
                reviewCanvas.scaleFactor = previousCanvasScaleFactor;
                safeAreaRect.anchorMin = previousSafeAnchorMin;
                safeAreaRect.anchorMax = previousSafeAnchorMax;
                safeAreaRect.offsetMin = previousSafeOffsetMin;
                safeAreaRect.offsetMax = previousSafeOffsetMax;
                safeAreaRoot.enabled = previousSafeAreaEnabled;
                reviewCanvas.renderMode = previousRenderMode;
                reviewCanvas.worldCamera = previousWorldCamera;
                reviewCanvas.planeDistance = previousPlaneDistance;
                reviewCamera.targetTexture = previousTarget;
                reviewCamera.aspect = previousCameraAspect;
                RenderTexture.active = previousActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static void ApplyVirtualSafeArea(
            RectTransform safeAreaRect,
            int width,
            int height,
            float insetPixels)
        {
            if (safeAreaRect == null)
            {
                throw new InvalidOperationException("Visual QA safe-area RectTransform is missing.");
            }

            safeAreaRoot.enabled = false;
            float insetX = Mathf.Clamp01(insetPixels / Mathf.Max(1f, width));
            float insetY = Mathf.Clamp01(insetPixels / Mathf.Max(1f, height));
            safeAreaRect.anchorMin = new Vector2(insetX, insetY);
            safeAreaRect.anchorMax = new Vector2(1f - insetX, 1f - insetY);
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
        }

        private static CaptureRecord BuildCaptureRecord(
            CapturePlan plan,
            string path,
            long fileBytes,
            PixelAudit audit)
        {
            ChapterHubReviewSession session = controller.Session;
            ChapterHubReviewProfile.StageDefinition selectedStage = session?.SelectedStage;
            UIStageRouteProjection projection = controller.CurrentProjection;
            bool projectionCurrent = projection != null
                && stageCatalog.IsProjectionCurrent(
                    projection,
                    UIRouteId.Combat,
                    out _);
            int confirmationEventDelta =
                observedConfirmationEventCount - confirmationEventBaseline;

            return new CaptureRecord
            {
                Sequence = plan.Sequence,
                State = plan.State.ToString(),
                Width = plan.Width,
                Height = plan.Height,
                Path = path,
                FileBytes = fileBytes,
                MeanLuminance = audit.MeanLuminance,
                LuminanceRange = audit.LuminanceRange,
                ControllerPhase = controller.CurrentPhase.ToString(),
                ControllerPanel = controller.CurrentPanel.ToString(),
                SelectedChapterId = session?.SelectedChapterId ?? string.Empty,
                SelectedStageId = session?.SelectedStageId ?? string.Empty,
                SelectedContentStatus = (selectedStage?.ContentStatus
                    ?? ChapterHubReviewContentStatus.None).ToString(),
                RenderedStatus = ResolveRenderedStatusText(plan.State),
                StageProjectionResolved = projection != null,
                StageProjectionCurrent = projectionCurrent,
                ProjectionCatalogEntryId = projection?.CatalogEntryId ?? string.Empty,
                ProjectionDigest = projection?.CanonicalProjectionDigest ?? string.Empty,
                BriefingDigest = projection?.CanonicalBriefingDigest ?? string.Empty,
                UnverifiedRowsHidden = AreUnverifiedRowsHidden(),
                UnverifiedBriefingFieldsNotPresent = projection?.Briefing == null
                    || AreUnverifiedBriefingFieldsNotPresent(projection.Briefing),
                UnverifiedBriefingDispositions = BuildUnverifiedDispositionSummary(
                    projection?.Briefing),
                ConfirmationFirstAccepted = firstConfirmationAccepted,
                ConfirmationDuplicateAccepted = duplicateConfirmationAccepted,
                ConfirmationDispatchCount = controller.ConfirmationDispatchCount,
                ConfirmationEventDelta = confirmationEventDelta,
                ConfirmationAccepted = session?.IsConfirmationAccepted == true,
                ConfirmedCatalogEntryId = controller.LastConfirmedCatalogEntryId,
                StageRunActive = StageRunRuntime.HasActiveContext,
                RuntimeContractValidated = true
            };
        }

        private static float ResolveCanvasScaleFactor(
            int width,
            int height,
            Vector2 referenceResolution,
            float matchWidthOrHeight)
        {
            float safeReferenceWidth = Mathf.Max(1f, referenceResolution.x);
            float safeReferenceHeight = Mathf.Max(1f, referenceResolution.y);
            float logWidth = Mathf.Log(width / safeReferenceWidth, 2f);
            float logHeight = Mathf.Log(height / safeReferenceHeight, 2f);
            float logWeightedAverage = Mathf.Lerp(
                logWidth,
                logHeight,
                Mathf.Clamp01(matchWidthOrHeight));
            return Mathf.Pow(2f, logWeightedAverage);
        }

        private static PixelAudit AuditPixels(Texture2D image)
        {
            const int HorizontalSamples = 32;
            const int VerticalSamples = 18;
            float minimum = 1f;
            float maximum = 0f;
            double total = 0d;
            int count = 0;
            for (int sampleY = 0; sampleY < VerticalSamples; sampleY++)
            {
                int y = Mathf.RoundToInt(
                    (image.height - 1) * (sampleY / (float)(VerticalSamples - 1)));
                for (int sampleX = 0; sampleX < HorizontalSamples; sampleX++)
                {
                    int x = Mathf.RoundToInt(
                        (image.width - 1) * (sampleX / (float)(HorizontalSamples - 1)));
                    Color pixel = image.GetPixel(x, y);
                    float luminance = (pixel.r * 0.2126f)
                        + (pixel.g * 0.7152f)
                        + (pixel.b * 0.0722f);
                    minimum = Mathf.Min(minimum, luminance);
                    maximum = Mathf.Max(maximum, luminance);
                    total += luminance;
                    count++;
                }
            }

            float mean = count > 0 ? (float)(total / count) : 0f;
            float range = Mathf.Max(0f, maximum - minimum);
            return new PixelAudit(
                mean,
                range,
                maximum > 0.045f && range > 0.018f && mean > 0.006f);
        }

        private static void CompleteCaptureSet()
        {
            List<string> issues = ValidateOutputSet();
            bool automatedPassed = issues.Count == 0;
            string failure = automatedPassed ? string.Empty : string.Join("\n", issues);
            WriteReports(automatedPassed, failure);

            if (automatedPassed)
            {
                Debug.Log(
                    "[OlympusChapterHubReviewVisualQA] BATCH_CAPTURE_CHECK_PASS "
                    + $"captures={Records.Count} humanReview=pending "
                    + $"output=`{OutputDirectory}`");
                SessionState.SetInt(
                    PhaseKey,
                    (int)RunnerPhase.SuccessAwaitingEditMode);
            }
            else
            {
                Debug.LogError(
                    "[OlympusChapterHubReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL\n"
                    + failure);
                SessionState.SetString(FailureKey, failure);
                SessionState.SetInt(
                    PhaseKey,
                    (int)RunnerPhase.FailureAwaitingEditMode);
            }

            EditorApplication.ExitPlaymode();
        }

        private static List<string> ValidateOutputSet()
        {
            var issues = new List<string>();
            if (Records.Count != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected {ExpectedCaptureCount} capture records, found {Records.Count}.");
            }

            string[] pngFiles = Directory.Exists(OutputDirectory)
                ? Directory.GetFiles(OutputDirectory, "*.png", SearchOption.TopDirectoryOnly)
                : Array.Empty<string>();
            if (pngFiles.Length != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected exactly {ExpectedCaptureCount} top-level PNG files in the dedicated "
                    + $"output directory; found {pngFiles.Length}.");
            }

            for (int i = 0; i < Plans.Length; i++)
            {
                CapturePlan plan = Plans[i];
                CaptureRecord record = Records.FirstOrDefault(
                    candidate => candidate.Sequence == plan.Sequence);
                if (record == null)
                {
                    issues.Add($"Missing capture record {plan.Sequence:00} ({plan.State}).");
                    continue;
                }

                if (!File.Exists(record.Path))
                {
                    issues.Add($"Capture file is missing: `{record.Path}`.");
                    continue;
                }

                byte[] png = File.ReadAllBytes(record.Path);
                Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!decoded.LoadImage(png, markNonReadable: false))
                    {
                        issues.Add($"Could not decode capture PNG `{record.Path}`.");
                    }
                    else if (decoded.width != plan.Width || decoded.height != plan.Height)
                    {
                        issues.Add(
                            $"Capture `{record.Path}` is {decoded.width}x{decoded.height}; "
                            + $"expected {plan.Width}x{plan.Height}.");
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(decoded);
                }

                if (!string.Equals(record.State, plan.State.ToString(), StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} state mismatch: "
                        + $"{record.State} vs {plan.State}.");
                }

                if (!record.RuntimeContractValidated
                    || !record.UnverifiedRowsHidden
                    || !record.UnverifiedBriefingFieldsNotPresent
                    || record.StageRunActive)
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} failed a recorded runtime contract check.");
                }

                bool projectionExpected = RequiresCanonicalProjection(plan.State);
                if (record.StageProjectionResolved != projectionExpected
                    || (projectionExpected && !record.StageProjectionCurrent))
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} projection evidence does not match {plan.State}.");
                }

                if (plan.State == ReviewCaptureState.ReviewConfirm
                    && (!record.ConfirmationFirstAccepted
                        || record.ConfirmationDuplicateAccepted
                        || !record.ConfirmationAccepted
                        || record.ConfirmationDispatchCount != 1
                        || record.ConfirmationEventDelta != 1
                        || !string.Equals(
                            record.ConfirmedCatalogEntryId,
                            OlympusChapterHubReviewSetup.CanonicalCatalogEntryId,
                            StringComparison.Ordinal)))
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} lacks exact-once confirmation evidence.");
                }
            }

            foreach (IGrouping<string, CaptureRecord> stateGroup in Records.GroupBy(
                         record => record.State,
                         StringComparer.Ordinal))
            {
                if (stateGroup.Count() != 3)
                {
                    issues.Add(
                        $"State {stateGroup.Key} has {stateGroup.Count()} captures; expected 3.");
                }
            }

            foreach (IGrouping<string, CaptureRecord> resolutionGroup in Records.GroupBy(
                         record => $"{record.Width}x{record.Height}",
                         StringComparer.Ordinal))
            {
                if (resolutionGroup.Count() != 6)
                {
                    issues.Add(
                        $"Resolution {resolutionGroup.Key} has {resolutionGroup.Count()} captures; "
                        + "expected 6.");
                }
            }

            return issues;
        }

        private static void ResetOutputArtifacts()
        {
            Directory.CreateDirectory(OutputDirectory);

            // The QA directory may contain manually added files or subfolders. Cleanup is
            // deliberately constrained to the 18 exact generated top-level PNG paths and the two
            // known top-level reports.
            for (int i = 0; i < Plans.Length; i++)
            {
                string pngPath = ResolveCapturePath(Plans[i]);
                if (File.Exists(pngPath))
                {
                    File.Delete(pngPath);
                }
            }

            if (File.Exists(ManifestPath))
            {
                File.Delete(ManifestPath);
            }

            if (File.Exists(ReportPath))
            {
                File.Delete(ReportPath);
            }
        }

        private static void FinishWithFailure(string failure)
        {
            string resolvedFailure = string.IsNullOrWhiteSpace(failure)
                ? "Unknown visual QA failure."
                : failure;
            SessionState.SetString(FailureKey, resolvedFailure);
            WriteReports(automatedPassed: false, resolvedFailure);
            Debug.LogError(
                "[OlympusChapterHubReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL\n"
                + resolvedFailure);
            SessionState.SetInt(
                PhaseKey,
                (int)RunnerPhase.FailureAwaitingEditMode);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
            }
        }

        private static void WriteReports(bool automatedPassed, string failure)
        {
            Directory.CreateDirectory(OutputDirectory);
            var manifest = new CaptureManifest
            {
                Scene = ScenePath,
                GeneratedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                AutomatedPassed = automatedPassed,
                HumanReviewRequired = true,
                HumanReviewed = false,
                Failure = failure ?? string.Empty,
                ExpectedCaptureCount = ExpectedCaptureCount,
                OutputCleanupScope = "top-level generated PNG/JSON/Markdown only",
                NavigationBoundary =
                    "RestartReview/OpenChapterMap/OpenStageDetail/OpenReviewConfirm/ConfirmSelectedStage",
                Captures = Records.ToArray()
            };
            File.WriteAllText(
                ManifestPath,
                JsonUtility.ToJson(manifest, prettyPrint: true),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var report = new StringBuilder();
            report.AppendLine("# Olympus Chapter Hub Review Visual QA");
            report.AppendLine();
            report.AppendLine(
                automatedPassed
                    ? "Automated capture check: PASS"
                    : "Automated capture check: FAIL");
            report.AppendLine("Human visual review: PENDING (record separately after inspection)");
            report.AppendLine();
            report.AppendLine($"- Scene: `{ScenePath}`");
            report.AppendLine($"- Output: `{OutputDirectory}`");
            report.AppendLine($"- Captures: `{Records.Count}` / `{ExpectedCaptureCount}`");
            report.AppendLine("- Resolutions: `1920x1080`, `2400x1080`, `2520x1080`");
            report.AppendLine("- States: ChapterHub, StageMap, canonical/InProduction/Announced detail, ReviewConfirm");
            report.AppendLine("- State preparation: controller public navigation only");
            report.AppendLine("- Rendering: Camera RenderTexture + ScreenSpaceCamera Canvas");
            report.AppendLine("- Output cleanup: generated top-level artifacts only; no recursive deletion");
            report.AppendLine("- Canonical route/StageRun/progression mutation: none");
            report.AppendLine();
            if (!string.IsNullOrWhiteSpace(failure))
            {
                report.AppendLine("## Failure");
                report.AppendLine();
                report.AppendLine("```");
                report.AppendLine(failure.Trim());
                report.AppendLine("```");
                report.AppendLine();
            }

            report.AppendLine("## Runtime contract evidence");
            report.AppendLine();
            report.AppendLine(
                "- Phase, panel, selected chapter ID, selected stage ID, and content status "
                + "are checked before every capture.");
            report.AppendLine(
                "- Canonical detail and ReviewConfirm require a current projection for "
                + "`story_v1_training_route`; all other states require no projection.");
            report.AppendLine("- Recommended power, loadout, duration, threat, summon, and reward rows must remain inactive.");
            report.AppendLine(
                "- Every ReviewConfirm capture accepts once, rejects the duplicate, and "
                + "observes one public confirmation event.");
            report.AppendLine("- Every state rejects an active `StageRunRuntime` context.");
            report.AppendLine();
            report.AppendLine("## Captures");
            report.AppendLine();
            report.AppendLine(
                "| # | State | Resolution | Phase / panel | Selected chapter / stage / status "
                + "| Rendered status | Projection | Hidden | Confirm | Bytes | Luma mean/range | Path |");
            report.AppendLine("|---:|---|---:|---|---|---|---|---:|---:|---:|---:|---|");
            for (int i = 0; i < Records.Count; i++)
            {
                CaptureRecord record = Records[i];
                string projection = record.StageProjectionResolved
                    ? $"current={record.StageProjectionCurrent}; `{record.ProjectionCatalogEntryId}`"
                    : "none";
                string confirm = record.State == ReviewCaptureState.ReviewConfirm.ToString()
                    ? $"first={record.ConfirmationFirstAccepted}; "
                        + $"duplicate={record.ConfirmationDuplicateAccepted}; "
                        + $"events={record.ConfirmationEventDelta}"
                    : "n/a";
                report.AppendLine(
                    $"| {record.Sequence:00} | {record.State} | {record.Width}x{record.Height} | "
                    + $"{record.ControllerPhase} / {record.ControllerPanel} | "
                    + $"`{record.SelectedChapterId}` / `{record.SelectedStageId}` / "
                    + $"{record.SelectedContentStatus} | `{record.RenderedStatus}` | "
                    + $"{projection} | {record.UnverifiedRowsHidden} | {confirm} | "
                    + $"{record.FileBytes} | "
                    + $"{record.MeanLuminance:0.0000}/{record.LuminanceRange:0.0000} | "
                    + $"`{record.Path}` |");
            }

            File.WriteAllText(
                ReportPath,
                report.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void HandleLaunchFailure(Exception exception, bool exitEditorWhenFinished)
        {
            Debug.LogException(exception);
            Directory.CreateDirectory(OutputDirectory);
            Records.Clear();
            WriteReports(automatedPassed: false, exception.ToString());
            ClearSessionState();
            Debug.LogError("[OlympusChapterHubReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL");
            if (exitEditorWhenFinished)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void FinalizeEditorSession(bool passed)
        {
            bool exitEditor = SessionState.GetBool(BatchExitKey, false);
            string failure = SessionState.GetString(FailureKey, string.Empty);
            ClearSessionState();
            if (passed)
            {
                Debug.Log(
                    $"[OlympusChapterHubReviewVisualQA] Automated capture check complete; "
                    + $"human visual review remains external: `{ReportPath}`.");
            }
            else
            {
                Debug.LogError(
                    "[OlympusChapterHubReviewVisualQA] Visual QA failed: " + failure);
            }

            if (exitEditor)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static bool HasLaunchTimedOut()
        {
            string ticksText = SessionState.GetString(StartedUtcTicksKey, string.Empty);
            if (!long.TryParse(
                    ticksText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long ticks))
            {
                return false;
            }

            DateTime started = new DateTime(ticks, DateTimeKind.Utc);
            return (DateTime.UtcNow - started).TotalSeconds > LaunchTimeoutSeconds;
        }

        private static void ResetRuntimeFields()
        {
            if (controller != null)
            {
                controller.ReviewConfirmed -= HandleReviewConfirmed;
            }

            controller = null;
            reviewCamera = null;
            reviewCanvas = null;
            reviewCanvasScaler = null;
            stageCatalog = null;
            hubStatusText = null;
            mapStatusText = null;
            detailStatusText = null;
            confirmStatusText = null;
            unverifiedRows = Array.Empty<GameObject>();
            planIndex = 0;
            readyAtFrame = 0;
            statePrepared = false;
            runtimeInitialized = false;
            observedConfirmationEventCount = 0;
            confirmationEventBaseline = 0;
            observedConfirmationCatalogEntryId = string.Empty;
            firstConfirmationAccepted = false;
            duplicateConfirmationAccepted = false;
            Records.Clear();
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(BatchExitKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseString(FailureKey);
            SessionState.EraseString(StartedUtcTicksKey);
            ResetRuntimeFields();
        }

        private static void HandleReviewConfirmed(string canonicalCatalogEntryId)
        {
            observedConfirmationEventCount++;
            observedConfirmationCatalogEntryId = canonicalCatalogEntryId ?? string.Empty;
        }

        private static CapturePlan[] BuildCapturePlans()
        {
            var plans = new List<CapturePlan>(ExpectedCaptureCount);
            int sequence = 1;
            AddResolution(plans, ref sequence, 1920, 1080);
            AddResolution(plans, ref sequence, 2400, 1080);
            AddResolution(plans, ref sequence, 2520, 1080);
            return plans.ToArray();
        }

        private static void AddResolution(
            List<CapturePlan> plans,
            ref int sequence,
            int width,
            int height)
        {
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.ChapterHub));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.StageMap));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.CanonicalDetail));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.InProductionDetail));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.AnnouncedDetail));
            plans.Add(new CapturePlan(sequence++, width, height, ReviewCaptureState.ReviewConfirm));
        }

        private static void RequireNavigation(
            bool accepted,
            string operation,
            ReviewCaptureState targetState)
        {
            if (!accepted)
            {
                throw new InvalidOperationException(
                    $"Controller public navigation `{operation}` was rejected while preparing {targetState}.");
            }
        }

        private static string ResolveCapturePath(CapturePlan plan)
        {
            return $"{OutputDirectory}/{plan.Sequence:00}_{plan.State}_"
                + $"{plan.Width}x{plan.Height}.png";
        }

        private static string ResolveStageId(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.InProductionDetail =>
                    OlympusChapterHubReviewSetup.InProductionStageId,
                ReviewCaptureState.AnnouncedDetail =>
                    OlympusChapterHubReviewSetup.AnnouncedStageId,
                _ => OlympusChapterHubReviewSetup.CanonicalStageId
            };
        }

        private static string ResolveExpectedSelectedStageId(ReviewCaptureState state)
        {
            return state == ReviewCaptureState.ChapterHub
                || state == ReviewCaptureState.StageMap
                    ? string.Empty
                    : ResolveStageId(state);
        }

        private static ChapterHubReviewPhase ResolveExpectedPhase(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.ChapterHub => ChapterHubReviewPhase.Overview,
                ReviewCaptureState.StageMap => ChapterHubReviewPhase.StageMap,
                ReviewCaptureState.ReviewConfirm => ChapterHubReviewPhase.ReviewConfirm,
                _ => ChapterHubReviewPhase.StageDetail
            };
        }

        private static ChapterHubReviewPanel ResolveExpectedPanel(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.ChapterHub => ChapterHubReviewPanel.ChapterHub,
                ReviewCaptureState.StageMap => ChapterHubReviewPanel.StageMap,
                ReviewCaptureState.ReviewConfirm => ChapterHubReviewPanel.ReviewConfirm,
                _ => ChapterHubReviewPanel.StageDetail
            };
        }

        private static ChapterHubReviewContentStatus ResolveExpectedContentStatus(
            ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.CanonicalDetail =>
                    ChapterHubReviewContentStatus.CanonicalPlayable,
                ReviewCaptureState.ReviewConfirm =>
                    ChapterHubReviewContentStatus.CanonicalPlayable,
                ReviewCaptureState.InProductionDetail =>
                    ChapterHubReviewContentStatus.InProduction,
                ReviewCaptureState.AnnouncedDetail =>
                    ChapterHubReviewContentStatus.Announced,
                _ => ChapterHubReviewContentStatus.None
            };
        }

        private static string ResolveRenderedStatusText(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.ChapterHub => hubStatusText?.text ?? string.Empty,
                ReviewCaptureState.StageMap => mapStatusText?.text ?? string.Empty,
                ReviewCaptureState.ReviewConfirm => confirmStatusText?.text ?? string.Empty,
                _ => detailStatusText?.text ?? string.Empty
            };
        }

        private static string ResolveExpectedRenderedStatus(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.ChapterHub => "REVIEW SAMPLE / LOCAL BROWSE",
                ReviewCaptureState.StageMap => "SELECT AN AUTHORED REVIEW NODE",
                ReviewCaptureState.CanonicalDetail =>
                    OlympusChapterHubReviewController.CanonicalReviewStatus,
                ReviewCaptureState.InProductionDetail =>
                    OlympusChapterHubReviewController.PlannedReviewStatus,
                ReviewCaptureState.AnnouncedDetail =>
                    OlympusChapterHubReviewController.AnnouncedReviewStatus,
                ReviewCaptureState.ReviewConfirm =>
                    OlympusChapterHubReviewController.ConfirmedReviewStatus,
                _ => string.Empty
            };
        }

        private static bool RequiresCanonicalProjection(ReviewCaptureState state)
        {
            return state == ReviewCaptureState.CanonicalDetail
                || state == ReviewCaptureState.ReviewConfirm;
        }

        private static bool AreUnverifiedRowsHidden()
        {
            return unverifiedRows.Length == UnverifiedRowPropertyNames.Length
                && unverifiedRows.All(row => row != null && !row.activeSelf);
        }

        private static bool AreUnverifiedBriefingFieldsNotPresent(
            StageBriefingReadModel briefing)
        {
            return briefing != null
                && briefing.RecommendedPowerDisposition != StageBriefingValueDisposition.Present
                && briefing.RecommendedLoadoutDisposition != StageBriefingValueDisposition.Present
                && briefing.TargetRunDurationDisposition != StageBriefingValueDisposition.Present
                && briefing.FeaturedThreatDisposition != StageBriefingValueDisposition.Present
                && briefing.FeaturedSummonNeedDisposition != StageBriefingValueDisposition.Present
                && briefing.RewardPreviewDisposition != StageBriefingValueDisposition.Present;
        }

        private static string BuildUnverifiedDispositionSummary(StageBriefingReadModel briefing)
        {
            if (briefing == null)
            {
                return string.Empty;
            }

            return "power=" + briefing.RecommendedPowerDisposition
                + ";loadout=" + briefing.RecommendedLoadoutDisposition
                + ";duration=" + briefing.TargetRunDurationDisposition
                + ";threat=" + briefing.FeaturedThreatDisposition
                + ";summon=" + briefing.FeaturedSummonNeedDisposition
                + ";reward=" + briefing.RewardPreviewDisposition;
        }

        private static T RequireObjectReference<T>(
            SerializedObject serialized,
            string propertyName)
            where T : UnityEngine.Object
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            T value = property?.objectReferenceValue as T;
            if (value == null)
            {
                throw new InvalidOperationException(
                    $"Review controller is missing required `{propertyName}` reference.");
            }

            return value;
        }

        private static T FindSingleInScene<T>(Scene scene) where T : Component
        {
            T[] components = FindAllInScene<T>(scene);
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene `{scene.path}` must contain exactly one {typeof(T).Name}; "
                    + $"found {components.Length}.");
            }

            return components[0];
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var components = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                components.AddRange(root.GetComponentsInChildren<T>(includeInactive: true));
            }

            return components.ToArray();
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            return Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private readonly struct CapturePlan
        {
            public CapturePlan(
                int sequence,
                int width,
                int height,
                ReviewCaptureState state)
            {
                Sequence = sequence;
                Width = width;
                Height = height;
                State = state;
            }

            public int Sequence { get; }
            public int Width { get; }
            public int Height { get; }
            public ReviewCaptureState State { get; }
        }

        private readonly struct PixelAudit
        {
            public PixelAudit(
                float meanLuminance,
                float luminanceRange,
                bool isUsable)
            {
                MeanLuminance = meanLuminance;
                LuminanceRange = luminanceRange;
                IsUsable = isUsable;
            }

            public float MeanLuminance { get; }
            public float LuminanceRange { get; }
            public bool IsUsable { get; }
        }

        [Serializable]
        private sealed class CaptureRecord
        {
            public int Sequence;
            public string State;
            public int Width;
            public int Height;
            public string Path;
            public long FileBytes;
            public float MeanLuminance;
            public float LuminanceRange;
            public string ControllerPhase;
            public string ControllerPanel;
            public string SelectedChapterId;
            public string SelectedStageId;
            public string SelectedContentStatus;
            public string RenderedStatus;
            public bool StageProjectionResolved;
            public bool StageProjectionCurrent;
            public string ProjectionCatalogEntryId;
            public string ProjectionDigest;
            public string BriefingDigest;
            public bool UnverifiedRowsHidden;
            public bool UnverifiedBriefingFieldsNotPresent;
            public string UnverifiedBriefingDispositions;
            public bool ConfirmationFirstAccepted;
            public bool ConfirmationDuplicateAccepted;
            public int ConfirmationDispatchCount;
            public int ConfirmationEventDelta;
            public bool ConfirmationAccepted;
            public string ConfirmedCatalogEntryId;
            public bool StageRunActive;
            public bool RuntimeContractValidated;
        }

        [Serializable]
        private sealed class CaptureManifest
        {
            public string Scene;
            public string GeneratedUtc;
            public bool AutomatedPassed;
            public bool HumanReviewRequired;
            public bool HumanReviewed;
            public string Failure;
            public int ExpectedCaptureCount;
            public string OutputCleanupScope;
            public string NavigationBoundary;
            public CaptureRecord[] Captures;
        }
    }
}
