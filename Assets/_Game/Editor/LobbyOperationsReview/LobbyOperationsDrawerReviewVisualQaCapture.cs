using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    /// Play-mode visual QA runner for OPS-01.
    ///
    /// Batch invocation must omit Unity's -quit argument. This class owns the asynchronous
    /// Play-mode lifecycle and exits the Editor after all 24 captures and evidence checks:
    /// -executeMethod DimensionBrawl.Editor.LobbyOperationsReview.LobbyOperationsDrawerReviewVisualQaCapture.RunBatchCaptureAndVerify
    /// </summary>
    [InitializeOnLoad]
    public static class LobbyOperationsDrawerReviewVisualQaCapture
    {
        public const string ScenePath = LobbyOperationsDrawerReviewSetup.ScenePath;
        public const string OutputDirectory =
            "C:/tmp/DimensionBrawl-LobbyOperationsDrawerReview-QA";

        private const string ManifestPath = OutputDirectory + "/capture-manifest.json";
        private const string ReportPath = OutputDirectory + "/capture-report.md";
        private const string SessionPrefix =
            "DimensionBrawl.LobbyOperationsDrawerReview.VisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";
        private const string CanonicalDigestKey = SessionPrefix + "CanonicalDigest";
        private const int InitialWarmupFrames = 8;
        private const int StateSettleFrames = 4;
        private const int ExpectedCaptureCount = 24;
        private const double LaunchTimeoutSeconds = 180d;

        private static readonly string[] CanonicalHashPaths =
        {
            "Assets/_Game/Scenes/UI/UI_Lobby.unity",
            "Assets/_Game/UI/Lobby/PF_UI_LobbyScreen.prefab",
            "Assets/_Game/UI/Lobby/PF_UI_LobbyCharacterStage.prefab",
            "Assets/_Game/UI/Presentation/PF_UI_LobbyPresentation.prefab",
            "Assets/_Game/UI/Lobby/Art/Dimension_Lobby_UI_0000_Background.png",
            // The generated TMP atlas is an in-process cache. Hash the immutable source fonts
            // and their import metadata; final worktree hygiene separately proves cache cleanup.
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-Medium.otf",
            "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf",
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset",
            "Assets/_Game/DesignData/UI/DB_UIScreenCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UIPanelCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UITextCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UIStateMessages.asset",
            "Assets/_Game/DesignData/UI/DB_UIMotionCatalog.asset",
            "Assets/_Game/DesignData/UI/DB_UICueBundles.asset",
            "Assets/_Game/DesignData/UI/DB_UIResponsiveLayouts.asset"
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
            Closed = 0,
            Directory = 1,
            NoticeDetail = 2,
            MailboxDetail = 3,
            MissionsDetail = 4,
            EventCalendarDetail = 5,
            NoticeConfirmBefore = 6,
            NoticeConfirmAfter = 7
        }

        private enum VirtualNotchOrientation
        {
            Left = 0,
            Right = 1
        }

        private static readonly CapturePlan[] Plans = BuildCapturePlans();
        private static readonly List<CaptureRecord> Records = new List<CaptureRecord>();

        private static LobbyOperationsReviewController controller;
        private static LobbyOperationsReviewProfile profile;
        private static Camera reviewCamera;
        private static Canvas reviewCanvas;
        private static CanvasScaler reviewCanvasScaler;
        private static UISafeAreaRoot safeAreaRoot;
        private static UIResponsiveRoot responsiveRoot;
        private static CanvasGroup closedPanel;
        private static CanvasGroup directoryPanel;
        private static CanvasGroup detailPanel;
        private static CanvasGroup confirmPanel;
        private static TMP_Text closedStatusText;
        private static TMP_Text directoryStatusText;
        private static TMP_Text detailStatusText;
        private static TMP_Text confirmStatusText;
        private static TMP_Text confirmSummaryText;
        private static Button closedOpenButton;
        private static Button detailBackButton;
        private static Button confirmBackButton;
        private static Button confirmAcknowledgeButton;
        private static int planIndex;
        private static int readyAtFrame;
        private static bool statePrepared;
        private static bool runtimeInitialized;
        private static int observedAcknowledgementEventCount;
        private static int acknowledgementEventBaseline;
        private static string observedAcknowledgedEntryId = string.Empty;
        private static bool firstAcknowledgementAccepted;
        private static bool duplicateAcknowledgementAccepted;

        static LobbyOperationsDrawerReviewVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("Tools/DimensionBrawl/Review/Capture Lobby Operations Drawer Visual QA")]
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
                        "An OPS-01 visual QA capture is already active.");
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "OPS-01 visual QA must start from Edit mode.");
                }

                if (!File.Exists(AssetPathToAbsolutePath(ScenePath)))
                {
                    throw new FileNotFoundException(
                        "Generate the OPS-01 review scene before visual QA.",
                        ScenePath);
                }

                if (!exitEditorWhenFinished
                    && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                LobbyOperationsDrawerReviewSetup.RunBatchVerification();
                ResetOutputArtifacts();
                ResetRuntimeFields();
                SessionState.SetBool(ActiveKey, true);
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
                SessionState.SetString(CanonicalDigestKey, ComputeCanonicalDigest());

                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log(
                    $"[LobbyOperationsDrawerReviewVisualQA] Entering Play mode for "
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

            if (HasTimedOut())
            {
                FinishWithFailure(
                    $"OPS-01 visual QA exceeded {LaunchTimeoutSeconds:0} seconds.");
                return;
            }

            if (phase != RunnerPhase.Capturing || !EditorApplication.isPlaying)
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
                $"[LobbyOperationsDrawerReviewVisualQA] CAPTURE_PASS "
                + $"{record.State} {record.Width}x{record.Height} "
                + $"notch={record.NotchOrientation} `{record.Path}`");
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
                    $"Active Play-mode scene is `{activeScene.path}`, expected `{ScenePath}`.");
            }

            controller = FindSingleInScene<LobbyOperationsReviewController>(activeScene);
            reviewCamera = FindSingleInScene<Camera>(activeScene);
            reviewCanvas = FindSingleInScene<Canvas>(activeScene);
            safeAreaRoot = FindSingleInScene<UISafeAreaRoot>(activeScene);
            responsiveRoot = FindSingleInScene<UIResponsiveRoot>(activeScene);
            reviewCanvasScaler = reviewCanvas.GetComponent<CanvasScaler>()
                ?? throw new InvalidOperationException("OPS-01 CanvasScaler is missing.");
            profile = AssetDatabase.LoadAssetAtPath<LobbyOperationsReviewProfile>(
                LobbyOperationsDrawerReviewSetup.ProfilePath)
                ?? throw new InvalidOperationException("OPS-01 profile is missing in Play mode.");

            var serialized = new SerializedObject(controller);
            serialized.UpdateIfRequiredOrScript();
            closedPanel = RequireObjectReference<CanvasGroup>(serialized, "closedPanel");
            directoryPanel = RequireObjectReference<CanvasGroup>(serialized, "directoryPanel");
            detailPanel = RequireObjectReference<CanvasGroup>(serialized, "detailPanel");
            confirmPanel = RequireObjectReference<CanvasGroup>(serialized, "confirmPanel");
            closedStatusText = RequireObjectReference<TMP_Text>(serialized, "closedStatusText");
            directoryStatusText = RequireObjectReference<TMP_Text>(serialized, "directoryStatusText");
            detailStatusText = RequireObjectReference<TMP_Text>(serialized, "detailStatusText");
            confirmStatusText = RequireObjectReference<TMP_Text>(serialized, "confirmStatusText");
            confirmSummaryText = RequireObjectReference<TMP_Text>(serialized, "confirmSummaryText");
            closedOpenButton = RequireObjectReference<Button>(serialized, "closedOpenButton");
            detailBackButton = RequireObjectReference<Button>(serialized, "detailBackButton");
            confirmBackButton = RequireObjectReference<Button>(serialized, "confirmBackButton");
            confirmAcknowledgeButton = RequireObjectReference<Button>(
                serialized,
                "confirmAcknowledgeButton");

            if (!profile.TryValidate(out string profileError))
            {
                throw new InvalidOperationException("OPS-01 profile is invalid: " + profileError);
            }

            if (FindAllInScene<UISceneFlowRouter>(activeScene).Length != 0
                || FindAllInScene<UISceneRouteLoader>(activeScene).Length != 0
                || FindAllInScene<UIPanelRouter>(activeScene).Length != 0)
            {
                throw new InvalidOperationException(
                    "OPS-01 scene contains forbidden routing ownership.");
            }

            ValidateDeterministicMonoBehaviourAllowlist(activeScene);

            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException(
                    "OPS-01 entered Play mode with an active StageRun context.");
            }

            controller.ReviewAcknowledged -= HandleReviewAcknowledged;
            controller.ReviewAcknowledged += HandleReviewAcknowledged;
            RequireNavigation(controller.RestartReview(), "RestartReview", ReviewCaptureState.Closed);
        }

        private static void PrepareState(ReviewCaptureState state)
        {
            acknowledgementEventBaseline = observedAcknowledgementEventCount;
            observedAcknowledgedEntryId = string.Empty;
            firstAcknowledgementAccepted = false;
            duplicateAcknowledgementAccepted = false;

            RequireNavigation(controller.RestartReview(), "RestartReview", state);
            if (state == ReviewCaptureState.Closed)
            {
                return;
            }

            RequireNavigation(controller.OpenDrawer(), "OpenDrawer", state);
            if (state == ReviewCaptureState.Directory)
            {
                return;
            }

            string entryId = ResolveEntryId(state);
            RequireNavigation(controller.SelectEntry(entryId), $"SelectEntry({entryId})", state);
            if (state != ReviewCaptureState.NoticeConfirmBefore
                && state != ReviewCaptureState.NoticeConfirmAfter)
            {
                return;
            }

            RequireNavigation(controller.OpenReviewConfirm(), "OpenReviewConfirm", state);
            if (state == ReviewCaptureState.NoticeConfirmAfter)
            {
                firstAcknowledgementAccepted = controller.AcknowledgeReview();
                duplicateAcknowledgementAccepted = controller.AcknowledgeReview();
            }
        }

        private static void ValidateExpectedRuntimeState(ReviewCaptureState state)
        {
            LobbyOperationsReviewPhase expectedPhase = ResolveExpectedPhase(state);
            LobbyOperationsReviewPanel expectedPanel = ResolveExpectedPanel(state);
            string expectedEntryId = ResolveExpectedSelectedEntryId(state);
            if (controller.Session == null
                || controller.CurrentPhase != expectedPhase
                || controller.CurrentPanel != expectedPanel
                || !string.Equals(
                    controller.SelectedEntryId,
                    expectedEntryId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"OPS-01 state mismatch for {state}: phase={controller.CurrentPhase}, "
                    + $"panel={controller.CurrentPanel}, selected=`{controller.SelectedEntryId}`.");
            }

            ValidatePanelVisibility(expectedPanel);
            ValidateExactBindings();
            ValidatePersistentCallbacks();
            ValidateExpectedFocus(expectedPanel, state);

            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException($"StageRun became active while preparing {state}.");
            }

            bool expectsDetail = IsDetailState(state);
            bool expectsNoticeCta = state == ReviewCaptureState.NoticeDetail;
            if (controller.IsReviewCtaVisible != expectsNoticeCta)
            {
                throw new InvalidOperationException(
                    $"OPS-01 CTA visibility mismatch for {state}: "
                    + $"{controller.IsReviewCtaVisible}.");
            }

            if (state == ReviewCaptureState.Closed)
            {
                RequireText(closedStatusText, LobbyOperationsReviewController.ClosedReviewStatus);
            }
            else if (state == ReviewCaptureState.Directory)
            {
                RequireText(
                    directoryStatusText,
                    LobbyOperationsReviewController.DirectoryReviewStatus);
                ValidateDirectorySourceStatuses();
            }
            else if (expectsDetail)
            {
                ValidateDetailContract(state);
            }
            else
            {
                ValidateConfirmationContract(state);
            }
        }

        private static void ValidatePanelVisibility(LobbyOperationsReviewPanel expected)
        {
            var panels = new[]
            {
                new KeyValuePair<LobbyOperationsReviewPanel, CanvasGroup>(
                    LobbyOperationsReviewPanel.Closed,
                    closedPanel),
                new KeyValuePair<LobbyOperationsReviewPanel, CanvasGroup>(
                    LobbyOperationsReviewPanel.Directory,
                    directoryPanel),
                new KeyValuePair<LobbyOperationsReviewPanel, CanvasGroup>(
                    LobbyOperationsReviewPanel.Detail,
                    detailPanel),
                new KeyValuePair<LobbyOperationsReviewPanel, CanvasGroup>(
                    LobbyOperationsReviewPanel.Confirm,
                    confirmPanel)
            };
            int visibleCount = 0;
            foreach (KeyValuePair<LobbyOperationsReviewPanel, CanvasGroup> pair in panels)
            {
                bool shouldShow = pair.Key == expected;
                bool visible = Mathf.Approximately(pair.Value.alpha, shouldShow ? 1f : 0f)
                    && pair.Value.interactable == shouldShow
                    && pair.Value.blocksRaycasts == shouldShow;
                if (!visible)
                {
                    throw new InvalidOperationException(
                        $"Panel `{pair.Key}` does not match expected visibility for `{expected}`.");
                }

                if (pair.Value.alpha > 0.5f)
                {
                    visibleCount++;
                }
            }

            if (visibleCount != 1)
            {
                throw new InvalidOperationException(
                    $"OPS-01 must expose exactly one panel; found {visibleCount}.");
            }
        }

        private static void ValidateExactBindings()
        {
            string[] expectedIds =
            {
                LobbyOperationsReviewProfile.NoticeEntryId,
                LobbyOperationsReviewProfile.MailboxEntryId,
                LobbyOperationsReviewProfile.MissionsEntryId,
                LobbyOperationsReviewProfile.EventCalendarEntryId
            };
            if (!controller.HasExactEntryBindings
                || !controller.HasExactDispositionRows
                || controller.EntryBindingCount != expectedIds.Length
                || controller.DispositionRowCount != 8)
            {
                throw new InvalidOperationException(
                    "OPS-01 exact entry/disposition binding contract is unavailable.");
            }

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
                    throw new InvalidOperationException(
                        $"OPS-01 entry binding {i} is incomplete or out of order.");
                }
            }
        }

        private static void ValidateDirectorySourceStatuses()
        {
            string[] expected =
            {
                LobbyOperationsReviewController.NoticeSourceStatus,
                LobbyOperationsReviewController.MailboxSourceStatus,
                LobbyOperationsReviewController.MissionsSourceStatus,
                LobbyOperationsReviewController.EventCalendarSourceStatus
            };
            for (int i = 0; i < expected.Length; i++)
            {
                LobbyOperationsReviewController.EntryButtonBinding binding =
                    controller.GetEntryBinding(i);
                RequireText(binding.SourceStatusText, expected[i]);
                if (!binding.CanvasGroup.interactable
                    || !binding.CanvasGroup.blocksRaycasts
                    || !binding.Button.interactable)
                {
                    throw new InvalidOperationException(
                        $"OPS-01 directory row {i} is not available for explanation navigation.");
                }
            }
        }

        private static void ValidateDetailContract(ReviewCaptureState state)
        {
            LobbyOperationsReviewProfile.EntryDefinition entry = controller.SelectedEntry
                ?? throw new InvalidOperationException($"{state} has no selected entry.");
            string expectedEntryId = ResolveEntryId(state);
            if (!string.Equals(entry.EntryId, expectedEntryId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{state} selected `{entry.EntryId}` instead of `{expectedEntryId}`.");
            }

            ValidateSelectedEntryDispositionEnums(entry);
            RequireText(detailStatusText, ResolveExpectedDetailStatus(state));
            if (!string.Equals(controller.CurrentDetailTitle, entry.TitleFallback, StringComparison.Ordinal)
                || !string.Equals(
                    controller.CurrentDetailExplanation,
                    entry.ExplanationFallback,
                    StringComparison.Ordinal)
                || !string.Equals(
                    controller.CurrentDetailStatus,
                    ResolveExpectedDetailStatus(state),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"OPS-01 detail read model is stale or incomplete for {state}.");
            }

            string[] expectedValues = ResolveExpectedDispositionValues(entry.EntryId);
            string[] expectedLabels =
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
            for (int i = 0; i < expectedValues.Length; i++)
            {
                LobbyOperationsReviewController.DispositionRowBinding row =
                    controller.GetDispositionRowBinding(i);
                if (row == null || row.RowRoot == null || !row.RowRoot.activeSelf)
                {
                    throw new InvalidOperationException(
                        $"OPS-01 disposition row {i} is hidden for {state}.");
                }

                RequireText(row.LabelText, expectedLabels[i]);
                RequireText(row.ValueText, expectedValues[i]);
            }
        }

        private static void ValidateConfirmationContract(ReviewCaptureState state)
        {
            LobbyOperationsReviewProfile.EntryDefinition entry = controller.SelectedEntry
                ?? throw new InvalidOperationException($"{state} has no selected Notice.");
            if (!string.Equals(
                    entry.EntryId,
                    LobbyOperationsReviewProfile.NoticeEntryId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{state} confirmation is not bound to the local Notice fixture.");
            }

            ValidateSelectedEntryDispositionEnums(entry);
            RequireText(confirmSummaryText, LobbyOperationsReviewController.ConfirmSummary);
            bool after = state == ReviewCaptureState.NoticeConfirmAfter;
            RequireText(
                confirmStatusText,
                after
                    ? LobbyOperationsReviewController.ConfirmedReviewStatus
                    : LobbyOperationsReviewController.ConfirmReadyStatus);

            if (after)
            {
                int eventDelta = observedAcknowledgementEventCount
                    - acknowledgementEventBaseline;
                if (!firstAcknowledgementAccepted
                    || duplicateAcknowledgementAccepted
                    || !controller.IsReviewAcknowledged
                    || controller.AcknowledgementDispatchCount != 1
                    || eventDelta != 1
                    || !string.Equals(
                        controller.LastAcknowledgedEntryId,
                        LobbyOperationsReviewProfile.NoticeEntryId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        observedAcknowledgedEntryId,
                        LobbyOperationsReviewProfile.NoticeEntryId,
                        StringComparison.Ordinal)
                    || confirmAcknowledgeButton.interactable)
                {
                    throw new InvalidOperationException(
                        "OPS-01 acknowledgement is not exact-once or mutated the expected latch contract.");
                }
            }
            else if (controller.IsReviewAcknowledged
                || controller.AcknowledgementDispatchCount != 0
                || !confirmAcknowledgeButton.interactable)
            {
                throw new InvalidOperationException(
                    "OPS-01 pre-acknowledgement confirmation state is invalid.");
            }
        }

        private static void ValidateSelectedEntryDispositionEnums(
            LobbyOperationsReviewProfile.EntryDefinition entry)
        {
            bool valid = entry.EntryId switch
            {
                LobbyOperationsReviewProfile.NoticeEntryId =>
                    entry.ProductionDisposition
                        == LobbyOperationsReviewProductionDisposition.LocalReviewFixture
                    && entry.ServiceDisposition
                        == LobbyOperationsReviewServiceDisposition.NotRequiredForReview
                    && entry.AccountDisposition
                        == LobbyOperationsReviewAccountDisposition.NotRequiredForReview
                    && entry.ServerClockDisposition
                        == LobbyOperationsReviewServerClockDisposition.NotRequiredForReview
                    && entry.ScheduleDisposition
                        == LobbyOperationsReviewScheduleDisposition.NotRequiredForReview
                    && entry.ProgressDisposition
                        == LobbyOperationsReviewProgressDisposition.NotRequiredForReview
                    && entry.AttentionDisposition
                        == LobbyOperationsReviewAttentionDisposition.NotRequiredForReview
                    && entry.ActionDisposition
                        == LobbyOperationsReviewActionDisposition.LocalReviewConfirm,
                LobbyOperationsReviewProfile.MailboxEntryId =>
                    entry.ProductionDisposition
                        == LobbyOperationsReviewProductionDisposition.ReviewShellNoProductCommitment
                    && entry.ServiceDisposition
                        == LobbyOperationsReviewServiceDisposition.NoVerifiedSource
                    && entry.AccountDisposition
                        == LobbyOperationsReviewAccountDisposition.NoVerifiedSource
                    && entry.ServerClockDisposition
                        == LobbyOperationsReviewServerClockDisposition.NotRequiredForReview
                    && entry.ScheduleDisposition
                        == LobbyOperationsReviewScheduleDisposition.NotRequiredForReview
                    && entry.ProgressDisposition
                        == LobbyOperationsReviewProgressDisposition.NotRequiredForReview
                    && entry.AttentionDisposition
                        == LobbyOperationsReviewAttentionDisposition.NoVerifiedSource
                    && entry.ActionDisposition
                        == LobbyOperationsReviewActionDisposition.ExplanationOnly,
                LobbyOperationsReviewProfile.MissionsEntryId =>
                    entry.ProductionDisposition
                        == LobbyOperationsReviewProductionDisposition.ReviewShellNoProductCommitment
                    && entry.ServiceDisposition
                        == LobbyOperationsReviewServiceDisposition.NotRequiredForReview
                    && entry.AccountDisposition
                        == LobbyOperationsReviewAccountDisposition.NoVerifiedSource
                    && entry.ServerClockDisposition
                        == LobbyOperationsReviewServerClockDisposition.NotRequiredForReview
                    && entry.ScheduleDisposition
                        == LobbyOperationsReviewScheduleDisposition.NotRequiredForReview
                    && entry.ProgressDisposition
                        == LobbyOperationsReviewProgressDisposition.NoVerifiedSource
                    && entry.AttentionDisposition
                        == LobbyOperationsReviewAttentionDisposition.NoVerifiedSource
                    && entry.ActionDisposition
                        == LobbyOperationsReviewActionDisposition.ExplanationOnly,
                LobbyOperationsReviewProfile.EventCalendarEntryId =>
                    entry.ProductionDisposition
                        == LobbyOperationsReviewProductionDisposition.DefinitionOnlyReviewShell
                    && entry.ServiceDisposition
                        == LobbyOperationsReviewServiceDisposition.NoVerifiedSource
                    && entry.AccountDisposition
                        == LobbyOperationsReviewAccountDisposition.NotRequiredForReview
                    && entry.ServerClockDisposition
                        == LobbyOperationsReviewServerClockDisposition.NoVerifiedSource
                    && entry.ScheduleDisposition
                        == LobbyOperationsReviewScheduleDisposition.DefinitionOnlyNoVerdict
                    && entry.ProgressDisposition
                        == LobbyOperationsReviewProgressDisposition.NotRequiredForReview
                    && entry.AttentionDisposition
                        == LobbyOperationsReviewAttentionDisposition.NoVerifiedSource
                    && entry.ActionDisposition
                        == LobbyOperationsReviewActionDisposition.ExplanationOnly,
                _ => false
            };
            if (!valid)
            {
                throw new InvalidOperationException(
                    $"OPS-01 selected entry `{entry.EntryId}` violates the exact disposition contract.");
            }
        }

        private static void ValidateExpectedFocus(
            LobbyOperationsReviewPanel panel,
            ReviewCaptureState state)
        {
            Button expected = panel switch
            {
                LobbyOperationsReviewPanel.Closed => closedOpenButton,
                LobbyOperationsReviewPanel.Directory => controller.GetEntryBinding(0).Button,
                LobbyOperationsReviewPanel.Detail => detailBackButton,
                LobbyOperationsReviewPanel.Confirm =>
                    state == ReviewCaptureState.NoticeConfirmAfter
                        ? confirmBackButton
                        : confirmAcknowledgeButton,
                _ => null
            };
            if (expected == null || controller.LastFocusTarget != expected)
            {
                throw new InvalidOperationException(
                    $"OPS-01 focus mismatch for {state}; expected `{expected?.name}`.");
            }

            if (EventSystem.current == null
                || EventSystem.current.currentSelectedGameObject != expected.gameObject)
            {
                throw new InvalidOperationException(
                    $"OPS-01 EventSystem focus did not settle on `{expected.name}` for {state}.");
            }
        }

        private static void ValidatePersistentCallbacks()
        {
            if (controller.ReviewAcknowledgedEvent == null
                || controller.ReviewAcknowledgedEvent.GetPersistentEventCount() != 0)
            {
                throw new InvalidOperationException(
                    "OPS-01 acknowledgement event contains an authored persistent callback.");
            }

            foreach (Button button in FindAllInScene<Button>(SceneManager.GetActiveScene()))
            {
                if (button.onClick.GetPersistentEventCount() != 0)
                {
                    throw new InvalidOperationException(
                        $"OPS-01 button `{button.name}` contains a persistent callback.");
                }

                Rect rect = (button.transform as RectTransform)?.rect ?? Rect.zero;
                if (rect.width < 48f || rect.height < 48f)
                {
                    throw new InvalidOperationException(
                        $"OPS-01 button `{button.name}` is below the 48px target.");
                }
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
            bool previousResponsiveEnabled = responsiveRoot.enabled;
            RectTransform safeRect = safeAreaRoot.transform as RectTransform;
            Vector2 previousAnchorMin = safeRect.anchorMin;
            Vector2 previousAnchorMax = safeRect.anchorMax;
            Vector2 previousOffsetMin = safeRect.offsetMin;
            Vector2 previousOffsetMax = safeRect.offsetMax;

            var target = new RenderTexture(
                plan.Width,
                plan.Height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB)
            {
                name = $"LobbyOperationsDrawerReviewQA_{plan.Width}x{plan.Height}",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var image = new Texture2D(
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
                responsiveRoot.enabled = false;
                reviewCanvasScaler.enabled = false;
                Vector2 reference = plan.Width >= 2400
                    ? new Vector2(2400f, 1080f)
                    : new Vector2(1920f, 1080f);
                reviewCanvas.scaleFactor = ResolveCanvasScaleFactor(
                    plan.Width,
                    plan.Height,
                    reference,
                    0.5f);
                ApplyVirtualSafeArea(safeRect, plan);

                Canvas.ForceUpdateCanvases();
                CaptureLayoutEvidence layoutEvidence =
                    ValidateCaptureLayout(plan, safeRect);
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
                        $"Capture `{path}` is blank or lacks visual range "
                        + $"(mean={audit.MeanLuminance:0.0000}, "
                        + $"range={audit.LuminanceRange:0.0000}).");
                }

                byte[] png = image.EncodeToPNG();
                if (png == null || png.Length < 1024)
                {
                    throw new InvalidOperationException(
                        $"Capture `{path}` produced an invalid PNG payload.");
                }

                File.WriteAllBytes(path, png);
                return BuildCaptureRecord(
                    plan,
                    path,
                    png.LongLength,
                    audit,
                    layoutEvidence);
            }
            finally
            {
                reviewCanvasScaler.enabled = previousScalerEnabled;
                reviewCanvas.scaleFactor = previousCanvasScaleFactor;
                safeRect.anchorMin = previousAnchorMin;
                safeRect.anchorMax = previousAnchorMax;
                safeRect.offsetMin = previousOffsetMin;
                safeRect.offsetMax = previousOffsetMax;
                safeAreaRoot.enabled = previousSafeAreaEnabled;
                responsiveRoot.enabled = previousResponsiveEnabled;
                reviewCanvas.renderMode = previousRenderMode;
                reviewCanvas.worldCamera = previousWorldCamera;
                reviewCanvas.planeDistance = previousPlaneDistance;
                reviewCamera.aspect = previousCameraAspect;
                reviewCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static void ApplyVirtualSafeArea(RectTransform safeRect, CapturePlan plan)
        {
            if (safeRect == null)
            {
                throw new InvalidOperationException("OPS-01 safe-area RectTransform is missing.");
            }

            safeAreaRoot.enabled = false;
            float leading = 112f;
            float trailing = 28f;
            float left = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? leading
                : trailing;
            float right = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? trailing
                : leading;
            safeRect.anchorMin = new Vector2(
                Mathf.Clamp01(left / Mathf.Max(1f, plan.Width)),
                Mathf.Clamp01(28f / Mathf.Max(1f, plan.Height)));
            safeRect.anchorMax = new Vector2(
                1f - Mathf.Clamp01(right / Mathf.Max(1f, plan.Width)),
                1f - Mathf.Clamp01(28f / Mathf.Max(1f, plan.Height)));
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;
        }

        private static CaptureLayoutEvidence ValidateCaptureLayout(
            CapturePlan plan,
            RectTransform safeRect)
        {
            float expectedLeft = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? 112f
                : 28f;
            float expectedRight = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? 28f
                : 112f;
            if (!Mathf.Approximately(
                    safeRect.anchorMin.x,
                    expectedLeft / plan.Width)
                || !Mathf.Approximately(
                    safeRect.anchorMax.x,
                    1f - expectedRight / plan.Width)
                || safeRect.offsetMin != Vector2.zero
                || safeRect.offsetMax != Vector2.zero)
            {
                throw new InvalidOperationException(
                    $"OPS-01 virtual {plan.NotchOrientation} notch was not applied exactly.");
            }

            Rect safeBounds = CalculateWorldRect(safeRect);
            Graphic[] visibleForegroundGraphics = safeRect
                .GetComponentsInChildren<Graphic>(includeInactive: true)
                .Where(graphic => IsActuallyVisible(graphic.gameObject))
                .ToArray();
            if (visibleForegroundGraphics.Length == 0)
            {
                throw new InvalidOperationException(
                    $"OPS-01 has no visible foreground graphics for {plan.State}.");
            }

            foreach (Graphic graphic in visibleForegroundGraphics)
            {
                RectTransform foregroundRect = graphic.transform as RectTransform;
                if (foregroundRect == null
                    || !Contains(safeBounds, CalculateWorldRect(foregroundRect), 1.5f))
                {
                    throw new InvalidOperationException(
                        $"OPS-01 visible foreground `{graphic.gameObject.name}` escapes the "
                        + "virtual safe area at "
                        + $"{plan.Width}x{plan.Height}.");
                }
            }

            TMP_Text[] visibleTexts = safeRect
                .GetComponentsInChildren<TMP_Text>(includeInactive: true)
                .Where(text => IsActuallyVisible(text.gameObject)
                    && !string.IsNullOrWhiteSpace(text.text))
                .ToArray();
            if (visibleTexts.Length == 0)
            {
                throw new InvalidOperationException(
                    $"OPS-01 has no visible text evidence for {plan.State}.");
            }
            foreach (TMP_Text text in visibleTexts)
            {
                ValidateVisibleTextFits(text);
            }

            int overlapPairCount = ValidateHeaderAgainstCurrentDrawer();
            if (controller.CurrentPanel == LobbyOperationsReviewPanel.Closed)
            {
                overlapPairCount += ValidateNamedVerticalGap(
                    "ClosedStatus",
                    "OpenOperationsReviewButton",
                    16f,
                    "closed status / opener CTA");
            }
            else if (controller.CurrentPanel == LobbyOperationsReviewPanel.Directory)
            {
                RectTransform[] rows = Enumerable.Range(0, controller.EntryBindingCount)
                    .Select(index => controller.GetEntryBinding(index).Button.transform
                        as RectTransform)
                    .ToArray();
                overlapPairCount += ValidateNoOverlap(rows, "directory entries");
                overlapPairCount += ValidateNamedPairNoOverlap(
                    "DirectoryEntry_04",
                    "DirectoryBoundaryNote",
                    "last directory row / boundary note");
            }
            else if (controller.CurrentPanel == LobbyOperationsReviewPanel.Detail)
            {
                RectTransform[] rows = Enumerable.Range(0, controller.DispositionRowCount)
                    .Select(index => controller.GetDispositionRowBinding(index).RowRoot.transform
                        as RectTransform)
                    .ToArray();
                overlapPairCount += ValidateNoOverlap(rows, "disposition rows");
                overlapPairCount += ValidateNamedPairNoOverlap(
                    "DetailExplanationPlate",
                    rows.FirstOrDefault()?.gameObject.name,
                    "detail explanation / first disposition");
                if (controller.IsReviewCtaVisible)
                {
                    overlapPairCount += ValidateNamedPairNoOverlap(
                        rows.LastOrDefault()?.gameObject.name,
                        "DetailReviewFixtureButton",
                        "last disposition / review CTA");
                }
            }
            else if (controller.CurrentPanel == LobbyOperationsReviewPanel.Confirm)
            {
                overlapPairCount += ValidateNamedPairNoOverlap(
                    "ConfirmTitle",
                    "ConfirmSummary",
                    "confirm title / summary");
                overlapPairCount += ValidateNamedPairNoOverlap(
                    "ConfirmSummary",
                    "ConfirmStatus",
                    "confirm summary / status");
                overlapPairCount += ValidateNamedPairNoOverlap(
                    "ConfirmStatus",
                    "ConfirmAcknowledgeButton",
                    "confirm status / acknowledgement CTA");
            }

            if (overlapPairCount == 0)
            {
                throw new InvalidOperationException(
                    $"OPS-01 produced no cross-element overlap evidence for {plan.State}.");
            }

            return new CaptureLayoutEvidence(
                visibleForegroundGraphics.Length,
                visibleTexts.Length,
                overlapPairCount,
                safeAreaContainmentValidated: true,
                visibleTextFitValidated: true,
                crossGroupOverlapValidated: true);
        }

        private static void ValidateVisibleTextFits(TMP_Text text)
        {
            text.ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
            float availableWidth = text.rectTransform.rect.width;
            float availableHeight = text.rectTransform.rect.height;
            bool noWrap = text.textWrappingMode == TextWrappingModes.NoWrap;
            if (text.preferredHeight > availableHeight + 0.75f
                || (noWrap && text.preferredWidth > availableWidth + 0.75f)
                || text.isTextOverflowing)
            {
                throw new InvalidOperationException(
                    $"Visible text `{text.name}` is clipped or overflowing: preferred="
                    + $"{text.preferredWidth:0.0}x{text.preferredHeight:0.0}, available="
                    + $"{availableWidth:0.0}x{availableHeight:0.0}, "
                    + $"isOverflowing={text.isTextOverflowing}.");
            }
        }

        private static int ValidateNoOverlap(RectTransform[] rects, string label)
        {
            int pairCount = 0;
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i] == null)
                {
                    throw new InvalidOperationException($"OPS-01 {label} contains a null rect.");
                }

                for (int j = i + 1; j < rects.Length; j++)
                {
                    pairCount++;
                    if (CalculateWorldRect(rects[i]).Overlaps(CalculateWorldRect(rects[j])))
                    {
                        throw new InvalidOperationException(
                            $"OPS-01 {label} `{rects[i].name}` and `{rects[j].name}` overlap.");
                    }
                }
            }

            return pairCount;
        }

        private static int ValidateHeaderAgainstCurrentDrawer()
        {
            RectTransform drawer = ResolveCurrentPanelGroup()
                .GetComponentsInChildren<RectTransform>(includeInactive: true)
                .FirstOrDefault(rect => rect.gameObject.name.EndsWith(
                    "Drawer",
                    StringComparison.Ordinal));
            if (drawer == null)
            {
                // Closed uses a lower-left opener plate rather than a drawer.
                return ValidateNamedPairNoOverlap(
                    "ReviewBoundary",
                    "ClosedReviewPlate",
                    "global review boundary / closed opener plate");
            }

            int pairs = ValidateRectPairNoOverlap(
                FindRectByName("ProductBreadcrumb"),
                drawer,
                "product breadcrumb / current drawer");
            pairs += ValidateRectPairNoOverlap(
                FindRectByName("ReviewBoundary"),
                drawer,
                "global review boundary / current drawer");
            return pairs;
        }

        private static int ValidateNamedPairNoOverlap(
            string firstName,
            string secondName,
            string label)
        {
            return ValidateRectPairNoOverlap(
                FindRectByName(firstName),
                FindRectByName(secondName),
                label);
        }

        private static int ValidateRectPairNoOverlap(
            RectTransform first,
            RectTransform second,
            string label)
        {
            if (first == null || second == null)
            {
                throw new InvalidOperationException(
                    $"OPS-01 overlap evidence `{label}` is missing an authored RectTransform.");
            }

            if (IsActuallyVisible(first.gameObject)
                && IsActuallyVisible(second.gameObject)
                && CalculateWorldRect(first).Overlaps(CalculateWorldRect(second)))
            {
                throw new InvalidOperationException(
                    $"OPS-01 cross-element overlap detected: {label}.");
            }

            return 1;
        }

        private static int ValidateNamedVerticalGap(
            string upperName,
            string lowerName,
            float minimumGap,
            string label)
        {
            RectTransform upper = FindRectByName(upperName);
            RectTransform lower = FindRectByName(lowerName);
            if (upper == null || lower == null)
            {
                throw new InvalidOperationException(
                    $"OPS-01 spacing evidence `{label}` is missing an authored RectTransform.");
            }

            if (IsActuallyVisible(upper.gameObject) && IsActuallyVisible(lower.gameObject))
            {
                Rect upperBounds = CalculateScreenRect(upper);
                Rect lowerBounds = CalculateScreenRect(lower);
                float actualGap = upperBounds.yMin - lowerBounds.yMax;
                if (actualGap < minimumGap)
                {
                    throw new InvalidOperationException(
                        $"OPS-01 vertical spacing `{label}` is {actualGap:0.0}px; "
                        + $"expected at least {minimumGap:0.0}px.");
                }
            }

            return 1;
        }

        private static RectTransform FindRectByName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            return FindAllInScene<RectTransform>(SceneManager.GetActiveScene())
                .FirstOrDefault(rect => string.Equals(
                    rect.gameObject.name,
                    objectName,
                    StringComparison.Ordinal));
        }

        private static bool IsActuallyVisible(GameObject gameObject)
        {
            if (gameObject == null || !gameObject.activeInHierarchy)
            {
                return false;
            }

            Transform cursor = gameObject.transform;
            while (cursor != null)
            {
                CanvasGroup group = cursor.GetComponent<CanvasGroup>();
                if (group != null && group.alpha <= 0.001f)
                {
                    return false;
                }

                cursor = cursor.parent;
            }

            return true;
        }

        private static CaptureRecord BuildCaptureRecord(
            CapturePlan plan,
            string path,
            long fileBytes,
            PixelAudit audit,
            CaptureLayoutEvidence layoutEvidence)
        {
            LobbyOperationsReviewProfile.EntryDefinition selected = controller.SelectedEntry;
            int eventDelta = observedAcknowledgementEventCount - acknowledgementEventBaseline;
            return new CaptureRecord
            {
                Sequence = plan.Sequence,
                State = plan.State.ToString(),
                Width = plan.Width,
                Height = plan.Height,
                NotchOrientation = plan.NotchOrientation.ToString(),
                VirtualLeftInsetPixels = plan.NotchOrientation == VirtualNotchOrientation.Left
                    ? 112
                    : 28,
                VirtualRightInsetPixels = plan.NotchOrientation == VirtualNotchOrientation.Left
                    ? 28
                    : 112,
                Path = path,
                FileBytes = fileBytes,
                MeanLuminance = audit.MeanLuminance,
                LuminanceRange = audit.LuminanceRange,
                ControllerPhase = controller.CurrentPhase.ToString(),
                ControllerPanel = controller.CurrentPanel.ToString(),
                SelectedEntryId = controller.SelectedEntryId,
                SelectedEntryKind = selected?.Kind.ToString() ?? string.Empty,
                RenderedStatus = ResolveRenderedStatus(plan.State),
                DispositionSummary = selected == null
                    ? string.Empty
                    : BuildDispositionSummary(selected.EntryId),
                DispositionRowsExact = !IsDetailState(plan.State)
                    || AreRenderedDispositionRowsExact(selected.EntryId),
                ReviewCtaVisible = controller.IsReviewCtaVisible,
                ReviewAcknowledged = controller.IsReviewAcknowledged,
                AcknowledgementFirstAccepted = firstAcknowledgementAccepted,
                AcknowledgementDuplicateAccepted = duplicateAcknowledgementAccepted,
                AcknowledgementDispatchCount = controller.AcknowledgementDispatchCount,
                AcknowledgementEventDelta = eventDelta,
                AcknowledgedEntryId = controller.LastAcknowledgedEntryId,
                FocusTarget = controller.LastFocusTarget?.gameObject.name ?? string.Empty,
                PanelContractValidated = true,
                SafeAreaValidated = layoutEvidence.SafeAreaContainmentValidated,
                TextFitValidated = layoutEvidence.VisibleTextFitValidated,
                OverlapValidated = layoutEvidence.CrossGroupOverlapValidated,
                VisibleForegroundRectCount = layoutEvidence.VisibleForegroundRectCount,
                VisibleTextCount = layoutEvidence.VisibleTextCount,
                EvaluatedOverlapPairCount = layoutEvidence.EvaluatedOverlapPairCount,
                StageRunActive = StageRunRuntime.HasActiveContext,
                ForbiddenProductFieldsAbsent = HasNoForbiddenProductFields(),
                CanonicalDigest = ComputeCanonicalDigest(),
                RuntimeContractValidated = true
            };
        }

        private static bool AreRenderedDispositionRowsExact(string entryId)
        {
            string[] expected = ResolveExpectedDispositionValues(entryId);
            if (expected.Length != controller.DispositionRowCount)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                LobbyOperationsReviewController.DispositionRowBinding row =
                    controller.GetDispositionRowBinding(i);
                if (row?.ValueText == null
                    || !string.Equals(row.ValueText.text, expected[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static string BuildDispositionSummary(string entryId)
        {
            string[] labels =
            {
                "production",
                "service",
                "account",
                "serverClock",
                "schedule",
                "progress",
                "attention",
                "action"
            };
            string[] values = ResolveExpectedDispositionValues(entryId);
            return string.Join(
                ";",
                labels.Zip(values, (label, value) => label + "=" + value));
        }

        private static string[] ResolveExpectedDispositionValues(string entryId)
        {
            return entryId switch
            {
                LobbyOperationsReviewProfile.NoticeEntryId => new[]
                {
                    "LOCAL REVIEW FIXTURE",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "LOCAL REVIEW CONFIRM"
                },
                LobbyOperationsReviewProfile.MailboxEntryId => new[]
                {
                    "REVIEW SHELL / NO PRODUCT COMMITMENT",
                    "NO VERIFIED SOURCE",
                    "NO VERIFIED SOURCE",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "NO VERIFIED SOURCE",
                    "EXPLANATION ONLY"
                },
                LobbyOperationsReviewProfile.MissionsEntryId => new[]
                {
                    "REVIEW SHELL / NO PRODUCT COMMITMENT",
                    "NOT REQUIRED FOR REVIEW",
                    "NO VERIFIED SOURCE",
                    "NOT REQUIRED FOR REVIEW",
                    "NOT REQUIRED FOR REVIEW",
                    "NO VERIFIED SOURCE",
                    "NO VERIFIED SOURCE",
                    "EXPLANATION ONLY"
                },
                LobbyOperationsReviewProfile.EventCalendarEntryId => new[]
                {
                    "DEFINITION-ONLY REVIEW SHELL",
                    "NO VERIFIED SOURCE",
                    "NOT REQUIRED FOR REVIEW",
                    "NO VERIFIED SOURCE",
                    "DEFINITION ONLY / NO VERDICT",
                    "NOT REQUIRED FOR REVIEW",
                    "NO VERIFIED SOURCE",
                    "EXPLANATION ONLY"
                },
                _ => Array.Empty<string>()
            };
        }

        private static bool HasNoForbiddenProductFields()
        {
            string[] forbiddenExactNames =
            {
                "accountId",
                "timestamp",
                "startDate",
                "endDate",
                "unreadCount",
                "progressValue",
                "reward",
                "attachment",
                "cost",
                "currency",
                "transaction",
                "transactionId",
                "url",
                "route",
                "servicePayload"
            };
            Type entryType = typeof(LobbyOperationsReviewProfile.EntryDefinition);
            string[] memberNames = entryType
                .GetFields(
                    System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic)
                .Select(field => field.Name)
                .Concat(entryType.GetProperties().Select(property => property.Name))
                .ToArray();
            return !forbiddenExactNames.Any(
                forbidden => memberNames.Any(
                    member => string.Equals(
                        member,
                        forbidden,
                        StringComparison.OrdinalIgnoreCase)));
        }

        private static float ResolveCanvasScaleFactor(
            int width,
            int height,
            Vector2 referenceResolution,
            float matchWidthOrHeight)
        {
            float logWidth = Mathf.Log(
                width / Mathf.Max(1f, referenceResolution.x),
                2f);
            float logHeight = Mathf.Log(
                height / Mathf.Max(1f, referenceResolution.y),
                2f);
            return Mathf.Pow(
                2f,
                Mathf.Lerp(logWidth, logHeight, Mathf.Clamp01(matchWidthOrHeight)));
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
            var issues = ValidateOutputSet();
            string initialDigest = SessionState.GetString(CanonicalDigestKey, string.Empty);
            string finalDigest = ComputeCanonicalDigest();
            if (string.IsNullOrWhiteSpace(initialDigest)
                || !string.Equals(initialDigest, finalDigest, StringComparison.Ordinal))
            {
                issues.Add(
                    "Canonical Lobby/UI/background hash boundary changed during visual QA.");
            }

            bool passed = issues.Count == 0;
            string failure = passed ? string.Empty : string.Join("\n", issues);
            WriteReports(passed, failure);
            if (passed)
            {
                Debug.Log(
                    "[LobbyOperationsDrawerReviewVisualQA] BATCH_CAPTURE_CHECK_PASS "
                    + $"captures={Records.Count} humanReview=pending output=`{OutputDirectory}`");
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.SuccessAwaitingEditMode);
            }
            else
            {
                Debug.LogError(
                    "[LobbyOperationsDrawerReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL\n"
                    + failure);
                SessionState.SetString(FailureKey, failure);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.FailureAwaitingEditMode);
            }

            EditorApplication.ExitPlaymode();
        }

        private static List<string> ValidateOutputSet()
        {
            var issues = new List<string>();
            if (Records.Count != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected {ExpectedCaptureCount} records; found {Records.Count}.");
            }

            string[] pngFiles = Directory.Exists(OutputDirectory)
                ? Directory.GetFiles(OutputDirectory, "*.png", SearchOption.TopDirectoryOnly)
                : Array.Empty<string>();
            if (pngFiles.Length != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected {ExpectedCaptureCount} top-level PNGs; found {pngFiles.Length}.");
            }

            string expectedDigest = SessionState.GetString(CanonicalDigestKey, string.Empty);
            foreach (CapturePlan plan in Plans)
            {
                CaptureRecord record = Records.FirstOrDefault(
                    candidate => candidate.Sequence == plan.Sequence);
                if (record == null)
                {
                    issues.Add($"Missing record {plan.Sequence:00} / {plan.State}.");
                    continue;
                }

                if (!File.Exists(record.Path))
                {
                    issues.Add($"Capture file missing: `{record.Path}`.");
                    continue;
                }

                byte[] png = File.ReadAllBytes(record.Path);
                var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!decoded.LoadImage(png, markNonReadable: false)
                        || decoded.width != plan.Width
                        || decoded.height != plan.Height)
                    {
                        issues.Add(
                            $"Capture `{record.Path}` is not exact {plan.Width}x{plan.Height}.");
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(decoded);
                }

                if (!record.RuntimeContractValidated
                    || !record.PanelContractValidated
                    || !record.SafeAreaValidated
                    || !record.TextFitValidated
                    || !record.OverlapValidated
                    || record.VisibleForegroundRectCount <= 0
                    || record.VisibleTextCount <= 0
                    || record.EvaluatedOverlapPairCount <= 0
                    || !record.ForbiddenProductFieldsAbsent
                    || record.StageRunActive
                    || !string.Equals(record.CanonicalDigest, expectedDigest, StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Capture {record.Sequence:00} failed a recorded runtime/boundary check.");
                }

                if (IsDetailState(plan.State) && !record.DispositionRowsExact)
                {
                    issues.Add(
                        $"Capture {record.Sequence:00} lacks exact disposition rendering.");
                }

                if (plan.State == ReviewCaptureState.NoticeConfirmAfter
                    && (!record.ReviewAcknowledged
                        || !record.AcknowledgementFirstAccepted
                        || record.AcknowledgementDuplicateAccepted
                        || record.AcknowledgementDispatchCount != 1
                        || record.AcknowledgementEventDelta != 1
                        || !string.Equals(
                            record.AcknowledgedEntryId,
                            LobbyOperationsReviewProfile.NoticeEntryId,
                            StringComparison.Ordinal)))
                {
                    issues.Add(
                        $"Capture {record.Sequence:00} lacks exact-once acknowledgement evidence.");
                }
            }

            foreach (ReviewCaptureState state in Enum.GetValues(typeof(ReviewCaptureState)))
            {
                int count = Records.Count(record => string.Equals(
                    record.State,
                    state.ToString(),
                    StringComparison.Ordinal));
                if (count != 3)
                {
                    issues.Add($"State {state} has {count} captures; expected 3.");
                }
            }

            foreach (string resolution in new[] { "1920x1080", "2400x1080", "2520x1080" })
            {
                CaptureRecord[] group = Records.Where(
                    record => $"{record.Width}x{record.Height}" == resolution).ToArray();
                if (group.Length != 8
                    || group.Count(record => record.NotchOrientation == "Left") != 4
                    || group.Count(record => record.NotchOrientation == "Right") != 4)
                {
                    issues.Add(
                        $"Resolution {resolution} must have 8 captures and 4/4 notch directions.");
                }
            }

            return issues;
        }

        private static void ResetOutputArtifacts()
        {
            Directory.CreateDirectory(OutputDirectory);
            // This directory is dedicated to this deterministic runner. Clear every top-level
            // PNG so a stale filename cannot masquerade as or block the exact 24-file matrix.
            foreach (string path in Directory.GetFiles(
                         OutputDirectory,
                         "*.png",
                         SearchOption.TopDirectoryOnly))
            {
                File.Delete(path);
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
            string resolved = string.IsNullOrWhiteSpace(failure)
                ? "Unknown OPS-01 visual QA failure."
                : failure;
            SessionState.SetString(FailureKey, resolved);
            WriteReports(automatedPassed: false, resolved);
            Debug.LogError(
                "[LobbyOperationsDrawerReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL\n" + resolved);
            SessionState.SetInt(PhaseKey, (int)RunnerPhase.FailureAwaitingEditMode);
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
                NavigationBoundary =
                    "RestartReview/OpenDrawer/SelectEntry/OpenReviewConfirm/AcknowledgeReview",
                CanonicalHashBoundary =
                    "15 canonical product assets (UI_Lobby scene, 3 UI prefabs, lobby background, 2 Pretendard OTF sources, 8 UI catalogs) + each corresponding .meta file",
                Captures = Records.ToArray()
            };
            File.WriteAllText(
                ManifestPath,
                JsonUtility.ToJson(manifest, prettyPrint: true),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var report = new StringBuilder();
            report.AppendLine("# OPS-01 Lobby Operations Drawer Visual QA");
            report.AppendLine();
            report.AppendLine(
                automatedPassed
                    ? "Automated capture check: PASS"
                    : "Automated capture check: FAIL");
            report.AppendLine("Human visual review: PENDING (must be recorded separately)");
            report.AppendLine();
            report.AppendLine($"- Scene: `{ScenePath}`");
            report.AppendLine($"- Output: `{OutputDirectory}`");
            report.AppendLine($"- Captures: `{Records.Count}` / `{ExpectedCaptureCount}`");
            report.AppendLine("- Resolutions: `1920x1080`, `2400x1080`, `2520x1080`");
            report.AppendLine(
                "- States: Closed, Directory, four detail entries, Notice confirm before/after");
            report.AppendLine("- Virtual safe area: asymmetric left/right notch, 4/4 per resolution");
            report.AppendLine("- State preparation: public controller navigation only");
            report.AppendLine("- HumanReviewRequired: `true`; HumanReviewed: `false`");
            report.AppendLine("- StageRun/router/network/persistence ownership: absent");
            report.AppendLine();
            if (!string.IsNullOrWhiteSpace(failure))
            {
                report.AppendLine("## Failure");
                report.AppendLine();
                report.AppendLine("```text");
                report.AppendLine(failure);
                report.AppendLine("```");
                report.AppendLine();
            }

            report.AppendLine("## Captures");
            report.AppendLine();
            report.AppendLine(
                "| # | State | Resolution | Notch | Phase / Panel | Selected | CTA | Ack | Status |");
            report.AppendLine("|---:|---|---|---|---|---|---:|---:|---|");
            foreach (CaptureRecord record in Records.OrderBy(item => item.Sequence))
            {
                report.AppendLine(
                    $"| {record.Sequence:00} | {record.State} | "
                    + $"{record.Width}x{record.Height} | {record.NotchOrientation} | "
                    + $"{record.ControllerPhase} / {record.ControllerPanel} | "
                    + $"{record.SelectedEntryId} | {record.ReviewCtaVisible} | "
                    + $"{record.ReviewAcknowledged} | {EscapeMarkdown(record.RenderedStatus)} |");
            }
            report.AppendLine();
            report.AppendLine(
                "Automated success does not attest composition, contrast, hierarchy, or visual polish. "
                + "Inspect all 24 PNGs before recording human review.");
            File.WriteAllText(
                ReportPath,
                report.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void FinalizeEditorSession(bool success)
        {
            bool exit = SessionState.GetBool(BatchExitKey, false);
            string failure = SessionState.GetString(FailureKey, string.Empty);
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(BatchExitKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseString(FailureKey);
            SessionState.EraseString(StartedUtcTicksKey);
            SessionState.EraseString(CanonicalDigestKey);
            ResetRuntimeFields();

            if (!success && !string.IsNullOrWhiteSpace(failure))
            {
                Debug.LogError(failure);
            }

            if (exit)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void HandleLaunchFailure(Exception exception, bool exitEditor)
        {
            Debug.LogException(exception);
            SessionState.SetBool(ActiveKey, false);
            SessionState.SetString(FailureKey, exception.ToString());
            WriteReports(automatedPassed: false, exception.ToString());
            if (exitEditor)
            {
                EditorApplication.Exit(1);
            }
        }

        private static bool HasTimedOut()
        {
            string raw = SessionState.GetString(StartedUtcTicksKey, string.Empty);
            return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks)
                && (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds
                    > LaunchTimeoutSeconds;
        }

        private static void ResetRuntimeFields()
        {
            if (controller != null)
            {
                controller.ReviewAcknowledged -= HandleReviewAcknowledged;
            }

            Records.Clear();
            controller = null;
            profile = null;
            reviewCamera = null;
            reviewCanvas = null;
            reviewCanvasScaler = null;
            safeAreaRoot = null;
            responsiveRoot = null;
            closedPanel = null;
            directoryPanel = null;
            detailPanel = null;
            confirmPanel = null;
            closedStatusText = null;
            directoryStatusText = null;
            detailStatusText = null;
            confirmStatusText = null;
            confirmSummaryText = null;
            closedOpenButton = null;
            detailBackButton = null;
            confirmBackButton = null;
            confirmAcknowledgeButton = null;
            planIndex = 0;
            readyAtFrame = 0;
            statePrepared = false;
            runtimeInitialized = false;
            observedAcknowledgementEventCount = 0;
            acknowledgementEventBaseline = 0;
            observedAcknowledgedEntryId = string.Empty;
            firstAcknowledgementAccepted = false;
            duplicateAcknowledgementAccepted = false;
        }

        private static void HandleReviewAcknowledged(string entryId)
        {
            observedAcknowledgementEventCount++;
            observedAcknowledgedEntryId = entryId ?? string.Empty;
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
            ReviewCaptureState[] states =
            {
                ReviewCaptureState.Closed,
                ReviewCaptureState.Directory,
                ReviewCaptureState.NoticeDetail,
                ReviewCaptureState.MailboxDetail,
                ReviewCaptureState.MissionsDetail,
                ReviewCaptureState.EventCalendarDetail,
                ReviewCaptureState.NoticeConfirmBefore,
                ReviewCaptureState.NoticeConfirmAfter
            };
            for (int i = 0; i < states.Length; i++)
            {
                plans.Add(new CapturePlan(
                    sequence++,
                    width,
                    height,
                    states[i],
                    i % 2 == 0
                        ? VirtualNotchOrientation.Left
                        : VirtualNotchOrientation.Right));
            }
        }

        private static void RequireNavigation(
            bool accepted,
            string operation,
            ReviewCaptureState target)
        {
            if (!accepted)
            {
                throw new InvalidOperationException(
                    $"Public navigation `{operation}` was rejected while preparing {target}.");
            }
        }

        private static string ResolveCapturePath(CapturePlan plan)
        {
            return $"{OutputDirectory}/{plan.Sequence:00}_{plan.State}_"
                + $"{plan.Width}x{plan.Height}_{plan.NotchOrientation}Notch.png";
        }

        private static string ResolveEntryId(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.MailboxDetail =>
                    LobbyOperationsReviewProfile.MailboxEntryId,
                ReviewCaptureState.MissionsDetail =>
                    LobbyOperationsReviewProfile.MissionsEntryId,
                ReviewCaptureState.EventCalendarDetail =>
                    LobbyOperationsReviewProfile.EventCalendarEntryId,
                _ => LobbyOperationsReviewProfile.NoticeEntryId
            };
        }

        private static string ResolveExpectedSelectedEntryId(ReviewCaptureState state)
        {
            return state == ReviewCaptureState.Closed || state == ReviewCaptureState.Directory
                ? string.Empty
                : ResolveEntryId(state);
        }

        private static LobbyOperationsReviewPhase ResolveExpectedPhase(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.Closed => LobbyOperationsReviewPhase.Closed,
                ReviewCaptureState.Directory => LobbyOperationsReviewPhase.Directory,
                ReviewCaptureState.NoticeConfirmBefore =>
                    LobbyOperationsReviewPhase.ReviewConfirm,
                ReviewCaptureState.NoticeConfirmAfter =>
                    LobbyOperationsReviewPhase.ReviewConfirm,
                _ => LobbyOperationsReviewPhase.EntryDetail
            };
        }

        private static LobbyOperationsReviewPanel ResolveExpectedPanel(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.Closed => LobbyOperationsReviewPanel.Closed,
                ReviewCaptureState.Directory => LobbyOperationsReviewPanel.Directory,
                ReviewCaptureState.NoticeConfirmBefore => LobbyOperationsReviewPanel.Confirm,
                ReviewCaptureState.NoticeConfirmAfter => LobbyOperationsReviewPanel.Confirm,
                _ => LobbyOperationsReviewPanel.Detail
            };
        }

        private static bool IsDetailState(ReviewCaptureState state)
        {
            return state == ReviewCaptureState.NoticeDetail
                || state == ReviewCaptureState.MailboxDetail
                || state == ReviewCaptureState.MissionsDetail
                || state == ReviewCaptureState.EventCalendarDetail;
        }

        private static string ResolveExpectedDetailStatus(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.NoticeDetail =>
                    LobbyOperationsReviewController.NoticeDetailStatus,
                ReviewCaptureState.MailboxDetail =>
                    LobbyOperationsReviewController.MailboxDetailStatus,
                ReviewCaptureState.MissionsDetail =>
                    LobbyOperationsReviewController.MissionsDetailStatus,
                ReviewCaptureState.EventCalendarDetail =>
                    LobbyOperationsReviewController.EventCalendarDetailStatus,
                _ => string.Empty
            };
        }

        private static string ResolveRenderedStatus(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.Closed => closedStatusText?.text ?? string.Empty,
                ReviewCaptureState.Directory => directoryStatusText?.text ?? string.Empty,
                ReviewCaptureState.NoticeConfirmBefore => confirmStatusText?.text ?? string.Empty,
                ReviewCaptureState.NoticeConfirmAfter => confirmStatusText?.text ?? string.Empty,
                _ => detailStatusText?.text ?? string.Empty
            };
        }

        private static CanvasGroup ResolveCurrentPanelGroup()
        {
            return controller.CurrentPanel switch
            {
                LobbyOperationsReviewPanel.Closed => closedPanel,
                LobbyOperationsReviewPanel.Directory => directoryPanel,
                LobbyOperationsReviewPanel.Detail => detailPanel,
                LobbyOperationsReviewPanel.Confirm => confirmPanel,
                _ => throw new InvalidOperationException("OPS-01 has no current panel group.")
            };
        }

        private static void RequireText(TMP_Text text, string expected)
        {
            if (text == null || !string.Equals(text.text, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"OPS-01 text mismatch on `{text?.name}`: expected `{expected}`, "
                    + $"found `{text?.text}`.");
            }
        }

        private static bool Contains(Rect outer, Rect inner, float tolerance)
        {
            return inner.xMin >= outer.xMin - tolerance
                && inner.xMax <= outer.xMax + tolerance
                && inner.yMin >= outer.yMin - tolerance
                && inner.yMax <= outer.yMax + tolerance;
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

        private static Rect CalculateScreenRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector2[] screenCorners = corners
                .Select(corner => RectTransformUtility.WorldToScreenPoint(camera, corner))
                .ToArray();
            return Rect.MinMaxRect(
                screenCorners.Min(corner => corner.x),
                screenCorners.Min(corner => corner.y),
                screenCorners.Max(corner => corner.x),
                screenCorners.Max(corner => corner.y));
        }

        private static T RequireObjectReference<T>(
            SerializedObject serialized,
            string propertyName)
            where T : UnityEngine.Object
        {
            T value = serialized.FindProperty(propertyName)?.objectReferenceValue as T;
            if (value == null)
            {
                throw new InvalidOperationException(
                    $"OPS-01 controller is missing `{propertyName}`.");
            }

            return value;
        }

        private static T FindSingleInScene<T>(Scene scene) where T : Component
        {
            T[] components = FindAllInScene<T>(scene);
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene `{scene.path}` needs one {typeof(T).Name}; found {components.Length}.");
            }

            return components[0];
        }

        private static void ValidateDeterministicMonoBehaviourAllowlist(Scene scene)
        {
            var allowedTypes = new HashSet<Type>
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
            foreach (MonoBehaviour behaviour in FindAllInScene<MonoBehaviour>(scene))
            {
                if (behaviour == null || !allowedTypes.Contains(behaviour.GetType()))
                {
                    throw new InvalidOperationException(
                        "OPS-01 contains a missing or non-allowlisted MonoBehaviour: `"
                        + (behaviour == null
                            ? "MissingScript"
                            : behaviour.GetType().FullName)
                        + "`.");
                }
            }

            foreach (Transform transform in FindAllInScene<Transform>(scene))
            {
                if (transform.GetComponents<Component>().Any(component => component == null))
                {
                    throw new InvalidOperationException(
                        $"OPS-01 object `{transform.gameObject.name}` has a missing script slot.");
                }
            }
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

        private static string ComputeCanonicalDigest()
        {
            var aggregate = new StringBuilder();
            foreach (string assetPath in CanonicalHashPaths)
            {
                AppendFileDigest(aggregate, assetPath);
                AppendFileDigest(aggregate, assetPath + ".meta");
            }

            using SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(aggregate.ToString());
            return BitConverter.ToString(sha.ComputeHash(bytes))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static void AppendFileDigest(StringBuilder aggregate, string assetPath)
        {
            string absolutePath = AssetPathToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException(
                    $"Canonical hash boundary file is missing: `{assetPath}`.",
                    absolutePath);
            }

            using FileStream stream = File.OpenRead(absolutePath);
            using SHA256 sha = SHA256.Create();
            string digest = BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
            aggregate.Append(assetPath).Append('=').Append(digest).Append('\n');
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

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private readonly struct CapturePlan
        {
            public CapturePlan(
                int sequence,
                int width,
                int height,
                ReviewCaptureState state,
                VirtualNotchOrientation notchOrientation)
            {
                Sequence = sequence;
                Width = width;
                Height = height;
                State = state;
                NotchOrientation = notchOrientation;
            }

            public int Sequence { get; }
            public int Width { get; }
            public int Height { get; }
            public ReviewCaptureState State { get; }
            public VirtualNotchOrientation NotchOrientation { get; }
        }

        private readonly struct PixelAudit
        {
            public PixelAudit(float meanLuminance, float luminanceRange, bool isUsable)
            {
                MeanLuminance = meanLuminance;
                LuminanceRange = luminanceRange;
                IsUsable = isUsable;
            }

            public float MeanLuminance { get; }
            public float LuminanceRange { get; }
            public bool IsUsable { get; }
        }

        private readonly struct CaptureLayoutEvidence
        {
            public CaptureLayoutEvidence(
                int visibleForegroundRectCount,
                int visibleTextCount,
                int evaluatedOverlapPairCount,
                bool safeAreaContainmentValidated,
                bool visibleTextFitValidated,
                bool crossGroupOverlapValidated)
            {
                VisibleForegroundRectCount = visibleForegroundRectCount;
                VisibleTextCount = visibleTextCount;
                EvaluatedOverlapPairCount = evaluatedOverlapPairCount;
                SafeAreaContainmentValidated = safeAreaContainmentValidated;
                VisibleTextFitValidated = visibleTextFitValidated;
                CrossGroupOverlapValidated = crossGroupOverlapValidated;
            }

            public int VisibleForegroundRectCount { get; }
            public int VisibleTextCount { get; }
            public int EvaluatedOverlapPairCount { get; }
            public bool SafeAreaContainmentValidated { get; }
            public bool VisibleTextFitValidated { get; }
            public bool CrossGroupOverlapValidated { get; }
        }

        [Serializable]
        private sealed class CaptureRecord
        {
            public int Sequence;
            public string State;
            public int Width;
            public int Height;
            public string NotchOrientation;
            public int VirtualLeftInsetPixels;
            public int VirtualRightInsetPixels;
            public string Path;
            public long FileBytes;
            public float MeanLuminance;
            public float LuminanceRange;
            public string ControllerPhase;
            public string ControllerPanel;
            public string SelectedEntryId;
            public string SelectedEntryKind;
            public string RenderedStatus;
            public string DispositionSummary;
            public bool DispositionRowsExact;
            public bool ReviewCtaVisible;
            public bool ReviewAcknowledged;
            public bool AcknowledgementFirstAccepted;
            public bool AcknowledgementDuplicateAccepted;
            public int AcknowledgementDispatchCount;
            public int AcknowledgementEventDelta;
            public string AcknowledgedEntryId;
            public string FocusTarget;
            public bool PanelContractValidated;
            public bool SafeAreaValidated;
            public bool TextFitValidated;
            public bool OverlapValidated;
            public int VisibleForegroundRectCount;
            public int VisibleTextCount;
            public int EvaluatedOverlapPairCount;
            public bool StageRunActive;
            public bool ForbiddenProductFieldsAbsent;
            public string CanonicalDigest;
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
            public string NavigationBoundary;
            public string CanonicalHashBoundary;
            public CaptureRecord[] Captures;
        }
    }
}
