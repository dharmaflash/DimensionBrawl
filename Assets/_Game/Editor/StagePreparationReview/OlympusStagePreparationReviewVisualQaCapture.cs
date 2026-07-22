using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.LevelDesign;
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
    /// Deterministic Play-mode visual QA for PREP-01.
    ///
    /// Batch invocation must omit Unity's -quit argument. This runner owns the asynchronous
    /// Play-mode lifecycle and exits only after 21 exact-resolution captures plus Edit-mode
    /// setup/digest postflight verification:
    /// -executeMethod DimensionBrawl.Editor.StagePreparationReview.OlympusStagePreparationReviewVisualQaCapture.RunBatchCaptureAndVerify
    /// </summary>
    [InitializeOnLoad]
    public static class OlympusStagePreparationReviewVisualQaCapture
    {
        public const string ScenePath = OlympusStagePreparationReviewSetup.ScenePath;
        public const string OutputDirectory =
            "C:/tmp/DimensionBrawl-OlympusStagePreparationReview-QA";

        private const string ManifestPath = OutputDirectory + "/capture-manifest.json";
        private const string ReportPath = OutputDirectory + "/capture-report.md";
        private const string SessionPrefix =
            "DimensionBrawl.OlympusStagePreparationReview.VisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";
        private const string SetupBeforeKey = SessionPrefix + "SetupBefore";
        private const string CanonicalDigestBeforeKey =
            SessionPrefix + "CanonicalDigestBefore";
        private const int InitialWarmupFrames = 8;
        private const int StateSettleFrames = 4;
        private const int ExpectedCaptureCount = 21;
        private const double LaunchTimeoutSeconds = 180d;
        private const int LeadingNotchInsetPixels = 112;
        private const int TrailingNotchInsetPixels = 28;
        private const int VerticalSafeInsetPixels = 28;
        private const float MinimumTouchTargetPixels = 48f;

        private static readonly string[] ExpectedSlotIds =
        {
            "SummonSlot1",
            "SummonSlot2",
            "SummonSlot3"
        };

        private static readonly string[] ExpectedActionIds =
        {
            "SummonSlot1.ChargeBruiser",
            "SummonSlot2.LaserSoldier",
            "SummonSlot3.FireDragon"
        };

        private static readonly string[] ForbiddenProgressionPhrases =
        {
            "ACCOUNT ID",
            "ACCOUNT LEVEL",
            "PLAYER ACCOUNT",
            "PLAYER LEVEL",
            "CHARACTER LEVEL",
            "RECOMMENDED LEVEL",
            "OWNED",
            "OWNERSHIP",
            "UNOWNED",
            "COMBAT POWER",
            "BATTLE POWER",
            "POWER SCORE",
            "계정",
            "소유",
            "레벨",
            "전투력"
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
            StageIntel = 0,
            LoadoutOverview = 1,
            Slot1Tier1Detail = 2,
            Slot2Tier2Detail = 3,
            Slot3Tier3Detail = 4,
            ReviewConfirmBefore = 5,
            ReviewConfirmAfter = 6
        }

        private enum VirtualNotchOrientation
        {
            Left = 0,
            Right = 1
        }

        private static readonly CapturePlan[] Plans = BuildCapturePlans();
        private static readonly List<CaptureRecord> Records = new List<CaptureRecord>();

        private static OlympusStagePreparationReviewController controller;
        private static StagePreparationReviewProfile profile;
        private static UIStageCatalog stageCatalog;
        private static Camera reviewCamera;
        private static Canvas reviewCanvas;
        private static CanvasScaler reviewCanvasScaler;
        private static GraphicRaycaster graphicRaycaster;
        private static UISafeAreaRoot safeAreaRoot;
        private static UIResponsiveRoot responsiveRoot;
        private static EventSystem eventSystem;
        private static CanvasGroup stageIntelPanel;
        private static CanvasGroup loadoutOverviewPanel;
        private static CanvasGroup summonDetailPanel;
        private static CanvasGroup reviewConfirmPanel;
        private static TMP_Text intelThreatTagsText;
        private static TMP_Text intelRecommendedSummonRoleText;
        private static Image detailIconImage;
        private static TMP_Text detailTitleText;
        private static TMP_Text detailRoleText;
        private static TMP_Text detailSelectedTierText;
        private static TMP_Text detailStageRoleText;
        private static TMP_Text detailPlayerUseText;
        private static TMP_Text detailSummonReadText;
        private static TMP_Text confirmDigestText;
        private static Button intelContinueButton;
        private static Button loadoutReviewButton;
        private static Button detailTier1Button;
        private static Button detailTier2Button;
        private static Button detailTier3Button;
        private static Button confirmBackButton;
        private static Button confirmAcceptButton;
        private static Button confirmRestartButton;
        private static int planIndex;
        private static int readyAtFrame;
        private static bool statePrepared;
        private static bool runtimeInitialized;
        private static int observedConfirmationEventCount;
        private static int confirmationEventBaseline;
        private static string observedConfirmedDigest = string.Empty;
        private static string preparedSelectionDigest = string.Empty;
        private static bool firstConfirmationAccepted;
        private static bool duplicateConfirmationAccepted;

        static OlympusStagePreparationReviewVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("Tools/DimensionBrawl/Review/Capture Olympus Stage Preparation Visual QA")]
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
                        "A PREP-01 visual QA capture is already active.");
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "PREP-01 visual QA must start from Edit mode.");
                }

                if (!File.Exists(AssetPathToAbsolutePath(ScenePath)))
                {
                    throw new FileNotFoundException(
                        "Generate the PREP-01 review scene before visual QA.",
                        ScenePath);
                }

                if (!exitEditorWhenFinished
                    && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                ResetOutputArtifacts();
                ResetRuntimeFields();
                ClearSessionState();
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetBool(SetupBeforeKey, false);
                SessionState.SetString(CanonicalDigestBeforeKey, string.Empty);

                OlympusStagePreparationReviewSetup.RunBatchVerification();
                string digestBefore =
                    OlympusStagePreparationReviewSetup.ComputeCanonicalBoundaryDigest();
                if (string.IsNullOrWhiteSpace(digestBefore))
                {
                    throw new InvalidOperationException(
                        "PREP-01 canonical boundary digest was empty before capture.");
                }

                SessionState.SetBool(SetupBeforeKey, true);
                SessionState.SetString(CanonicalDigestBeforeKey, digestBefore);
                SessionState.SetBool(ActiveKey, true);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));

                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log(
                    "[OlympusStagePreparationReviewVisualQA] Entering Play mode for "
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
                    $"PREP-01 visual QA exceeded {LaunchTimeoutSeconds:0} seconds.");
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
                "[OlympusStagePreparationReviewVisualQA] CAPTURE_PASS "
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

            controller = FindSingleInScene<OlympusStagePreparationReviewController>(activeScene);
            reviewCamera = FindSingleInScene<Camera>(activeScene);
            reviewCanvas = FindSingleInScene<Canvas>(activeScene);
            safeAreaRoot = FindSingleInScene<UISafeAreaRoot>(activeScene);
            responsiveRoot = FindSingleInScene<UIResponsiveRoot>(activeScene);
            eventSystem = FindSingleInScene<EventSystem>(activeScene);
            FindSingleInScene<InputSystemUIInputModule>(activeScene);
            reviewCanvasScaler = reviewCanvas.GetComponent<CanvasScaler>()
                ?? throw new InvalidOperationException(
                    "PREP-01 CanvasScaler is missing.");
            graphicRaycaster = reviewCanvas.GetComponent<GraphicRaycaster>()
                ?? throw new InvalidOperationException(
                    "PREP-01 GraphicRaycaster is missing.");

            profile = AssetDatabase.LoadAssetAtPath<StagePreparationReviewProfile>(
                OlympusStagePreparationReviewSetup.ProfilePath)
                ?? throw new InvalidOperationException(
                    "PREP-01 review profile is missing in Play mode.");
            stageCatalog = AssetDatabase.LoadAssetAtPath<UIStageCatalog>(
                OlympusStagePreparationReviewSetup.StageCatalogPath)
                ?? throw new InvalidOperationException(
                    "PREP-01 stage catalog is missing in Play mode.");

            var serialized = new SerializedObject(controller);
            serialized.UpdateIfRequiredOrScript();
            StagePreparationReviewProfile boundProfile =
                RequireObjectReference<StagePreparationReviewProfile>(serialized, "profile");
            UIStageCatalog boundStageCatalog =
                RequireObjectReference<UIStageCatalog>(serialized, "stageCatalog");
            if (!ReferenceEquals(boundProfile, profile)
                || !ReferenceEquals(boundStageCatalog, stageCatalog))
            {
                throw new InvalidOperationException(
                    "PREP-01 controller is not bound to the expected profile/catalog assets.");
            }

            stageIntelPanel = RequireObjectReference<CanvasGroup>(
                serialized,
                "stageIntelPanel");
            loadoutOverviewPanel = RequireObjectReference<CanvasGroup>(
                serialized,
                "loadoutOverviewPanel");
            summonDetailPanel = RequireObjectReference<CanvasGroup>(
                serialized,
                "summonDetailPanel");
            reviewConfirmPanel = RequireObjectReference<CanvasGroup>(
                serialized,
                "reviewConfirmPanel");
            intelThreatTagsText = RequireObjectReference<TMP_Text>(
                serialized,
                "intelThreatTagsText");
            intelRecommendedSummonRoleText = RequireObjectReference<TMP_Text>(
                serialized,
                "intelRecommendedSummonRoleText");
            detailIconImage = RequireObjectReference<Image>(serialized, "detailIconImage");
            detailTitleText = RequireObjectReference<TMP_Text>(serialized, "detailTitleText");
            detailRoleText = RequireObjectReference<TMP_Text>(serialized, "detailRoleText");
            detailSelectedTierText = RequireObjectReference<TMP_Text>(
                serialized,
                "detailSelectedTierText");
            detailStageRoleText = RequireObjectReference<TMP_Text>(
                serialized,
                "detailStageRoleText");
            detailPlayerUseText = RequireObjectReference<TMP_Text>(
                serialized,
                "detailPlayerUseText");
            detailSummonReadText = RequireObjectReference<TMP_Text>(
                serialized,
                "detailSummonReadText");
            confirmDigestText = RequireObjectReference<TMP_Text>(
                serialized,
                "confirmDigestText");
            intelContinueButton = RequireObjectReference<Button>(
                serialized,
                "intelContinueButton");
            loadoutReviewButton = RequireObjectReference<Button>(
                serialized,
                "loadoutReviewButton");
            detailTier1Button = RequireObjectReference<Button>(
                serialized,
                "detailTier1Button");
            detailTier2Button = RequireObjectReference<Button>(
                serialized,
                "detailTier2Button");
            detailTier3Button = RequireObjectReference<Button>(
                serialized,
                "detailTier3Button");
            confirmBackButton = RequireObjectReference<Button>(
                serialized,
                "confirmBackButton");
            confirmAcceptButton = RequireObjectReference<Button>(
                serialized,
                "confirmAcceptButton");
            confirmRestartButton = RequireObjectReference<Button>(
                serialized,
                "confirmRestartButton");

            if (!profile.TryValidate(out string profileError))
            {
                throw new InvalidOperationException(
                    "PREP-01 profile is invalid: " + profileError);
            }

            if (!stageCatalog.TryValidateEntryIdentities(out _)
                || !stageCatalog.TryGetStage(
                    OlympusStagePreparationReviewSetup.CanonicalCatalogEntryId,
                    out _))
            {
                throw new InvalidOperationException(
                    "PREP-01 requires the Olympus entry inside a valid stage catalog.");
            }

            if (reviewCanvasScaler.uiScaleMode
                != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                throw new InvalidOperationException(
                    "PREP-01 CanvasScaler must use ScaleWithScreenSize.");
            }

            if (controller.ConfirmationEvent == null
                || controller.ConfirmationEvent.GetPersistentEventCount() != 0)
            {
                throw new InvalidOperationException(
                    "PREP-01 confirmation event has an authored persistent callback.");
            }

            foreach (Button button in FindAllInScene<Button>(activeScene))
            {
                if (button.onClick.GetPersistentEventCount() != 0)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 button `{button.name}` has a persistent callback.");
                }

                Rect authoredRect = (button.transform as RectTransform)?.rect ?? Rect.zero;
                if (authoredRect.width < MinimumTouchTargetPixels
                    || authoredRect.height < MinimumTouchTargetPixels)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 button `{button.name}` is below the authored 48px target.");
                }
            }

            ValidateExactSlotBindingsAndActions();
            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException(
                    "PREP-01 entered Play mode with an active StageRun context.");
            }

            controller.ReviewConfirmed -= HandleReviewConfirmed;
            controller.ReviewConfirmed += HandleReviewConfirmed;
            RequireNavigation(
                controller.RestartReview(),
                "RestartReview",
                ReviewCaptureState.StageIntel);
            ValidateInitialSessionDefaults();
        }

        private static void PrepareState(ReviewCaptureState state)
        {
            confirmationEventBaseline = observedConfirmationEventCount;
            observedConfirmedDigest = string.Empty;
            preparedSelectionDigest = string.Empty;
            firstConfirmationAccepted = false;
            duplicateConfirmationAccepted = false;

            RequireNavigation(controller.RestartReview(), "RestartReview", state);
            ValidateInitialSessionDefaults();
            if (state == ReviewCaptureState.StageIntel)
            {
                preparedSelectionDigest = RequireSelectionDigest(state);
                return;
            }

            RequireNavigation(controller.OpenLoadout(), "OpenLoadout", state);
            if (state == ReviewCaptureState.LoadoutOverview)
            {
                preparedSelectionDigest = RequireSelectionDigest(state);
                return;
            }

            if (IsDetailState(state))
            {
                int slotIndex = ResolveDetailSlotIndex(state);
                int tier = ResolveDetailTier(state);
                RequireNavigation(
                    controller.InspectSlot(ExpectedSlotIds[slotIndex]),
                    $"InspectSlot({ExpectedSlotIds[slotIndex]})",
                    state);
                RequireNavigation(controller.SelectTier(tier), $"SelectTier({tier})", state);
                preparedSelectionDigest = RequireSelectionDigest(state);
                return;
            }

            ConfigureConfirmationSelection(state);
            RequireNavigation(
                controller.OpenReviewConfirm(),
                "OpenReviewConfirm",
                state);
            preparedSelectionDigest = RequireSelectionDigest(state);
            if (state == ReviewCaptureState.ReviewConfirmAfter)
            {
                firstConfirmationAccepted = controller.ConfirmReview();
                duplicateConfirmationAccepted = controller.ConfirmReview();
            }
        }

        private static void ConfigureConfirmationSelection(ReviewCaptureState state)
        {
            int[] tiers = ResolveExpectedTiers(state);
            for (int i = 0; i < ExpectedSlotIds.Length; i++)
            {
                RequireNavigation(
                    controller.InspectSlot(ExpectedSlotIds[i]),
                    $"InspectSlot({ExpectedSlotIds[i]})",
                    state);
                RequireNavigation(
                    controller.SelectTier(tiers[i]),
                    $"SelectTier({tiers[i]})",
                    state);
                RequireNavigation(controller.ReturnToLoadout(), "ReturnToLoadout", state);
            }
        }

        private static void ValidateExpectedRuntimeState(ReviewCaptureState state)
        {
            StagePreparationReviewSession session = controller.Session
                ?? throw new InvalidOperationException(
                    $"PREP-01 session is unavailable for {state}.");
            StagePreparationReviewPhase expectedPhase = ResolveExpectedPhase(state);
            StagePreparationReviewPanel expectedPanel = ResolveExpectedPanel(state);
            string expectedSlotId = ResolveExpectedSelectedSlotId(state);
            int expectedSelectedTier = IsDetailState(state)
                ? ResolveDetailTier(state)
                : 0;

            if (controller.CurrentPhase != expectedPhase
                || session.Phase != expectedPhase
                || controller.CurrentPanel != expectedPanel
                || !string.Equals(
                    controller.SelectedSlotId,
                    expectedSlotId,
                    StringComparison.Ordinal)
                || controller.SelectedTier != expectedSelectedTier)
            {
                throw new InvalidOperationException(
                    $"PREP-01 state mismatch for {state}: phase={controller.CurrentPhase}/"
                    + $"{session.Phase}, panel={controller.CurrentPanel}, "
                    + $"slot=`{controller.SelectedSlotId}`, tier={controller.SelectedTier}.");
            }

            ValidatePanelVisibility(expectedPanel);
            ValidateExactSlotBindingsAndActions();
            ValidateExpectedTierSnapshot(state);
            ValidateTierButtonPresentation(state);
            ValidateCanonicalProjection(state);
            ValidateNeutralRuntimeTextBoundary(state);
            ValidateExpectedFocus(state);
            ValidateImageTextSeparation(state);
            ValidateConfirmationButtonAffordance(state);
            ValidateConfirmationContract(state);

            string digest = RequireSelectionDigest(state);
            if (!string.Equals(digest, preparedSelectionDigest, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"PREP-01 selection digest changed after preparing {state}.");
            }

            if (IsDetailState(state))
            {
                ValidateTierReadout(state);
            }

            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException(
                    $"PREP-01 capture {state} created an active StageRun context.");
            }
        }

        private static void ValidatePanelVisibility(StagePreparationReviewPanel expected)
        {
            var panels = new[]
            {
                new KeyValuePair<StagePreparationReviewPanel, CanvasGroup>(
                    StagePreparationReviewPanel.StageIntel,
                    stageIntelPanel),
                new KeyValuePair<StagePreparationReviewPanel, CanvasGroup>(
                    StagePreparationReviewPanel.LoadoutOverview,
                    loadoutOverviewPanel),
                new KeyValuePair<StagePreparationReviewPanel, CanvasGroup>(
                    StagePreparationReviewPanel.SummonDetail,
                    summonDetailPanel),
                new KeyValuePair<StagePreparationReviewPanel, CanvasGroup>(
                    StagePreparationReviewPanel.ReviewConfirm,
                    reviewConfirmPanel)
            };
            int activeCount = 0;
            foreach (KeyValuePair<StagePreparationReviewPanel, CanvasGroup> pair in panels)
            {
                bool shouldShow = pair.Key == expected;
                bool exact = pair.Value.gameObject.activeSelf == shouldShow
                    && Mathf.Approximately(pair.Value.alpha, shouldShow ? 1f : 0f)
                    && pair.Value.interactable == shouldShow
                    && pair.Value.blocksRaycasts == shouldShow;
                if (!exact)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 panel `{pair.Key}` violates active/raycast state for `{expected}`.");
                }

                if (pair.Value.gameObject.activeInHierarchy && pair.Value.alpha > 0.5f)
                {
                    activeCount++;
                }
            }

            if (activeCount != 1)
            {
                throw new InvalidOperationException(
                    $"PREP-01 must expose exactly one panel; found {activeCount}.");
            }
        }

        private static void ValidateExactSlotBindingsAndActions()
        {
            if (profile == null
                || profile.SlotCount != StagePreparationReviewProfile.RequiredSlotCount
                || controller.SlotBindingCount != StagePreparationReviewProfile.RequiredSlotCount)
            {
                throw new InvalidOperationException(
                    "PREP-01 does not expose exactly three profile/controller slots.");
            }

            StagePreparationReviewSelection[] snapshot = controller.SelectionSnapshot;
            if (snapshot.Length != ExpectedSlotIds.Length)
            {
                throw new InvalidOperationException(
                    $"PREP-01 selection snapshot has {snapshot.Length} rows; expected three.");
            }

            for (int i = 0; i < ExpectedSlotIds.Length; i++)
            {
                StagePreparationReviewProfile.SlotDefinition slot = profile.GetSlot(i);
                OlympusStagePreparationReviewController.SlotBinding binding =
                    controller.GetSlotBinding(i);
                StagePreparationReviewSelection selection = snapshot[i];
                if (slot == null
                    || binding == null
                    || !string.Equals(slot.SlotId, ExpectedSlotIds[i], StringComparison.Ordinal)
                    || !string.Equals(binding.SlotId, ExpectedSlotIds[i], StringComparison.Ordinal)
                    || !string.Equals(selection.SlotId, ExpectedSlotIds[i], StringComparison.Ordinal)
                    || !string.Equals(slot.ActionId, ExpectedActionIds[i], StringComparison.Ordinal)
                    || !string.Equals(selection.ActionId, ExpectedActionIds[i], StringComparison.Ordinal)
                    || binding.InspectButton == null
                    || binding.IconImage == null
                    || binding.TitleText == null
                    || binding.RoleText == null
                    || binding.SelectedTierText == null)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 slot/action binding {i} is incomplete or out of order.");
                }

                if (slot.TierCount != StagePreparationReviewProfile.RequiredTierCount
                    || slot.TierReadoutCount
                        != StagePreparationReviewProfile.RequiredTierCount)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 `{slot.SlotId}` must retain three tiers/readouts.");
                }

                for (int tier = 1;
                    tier <= StagePreparationReviewProfile.RequiredTierCount;
                    tier++)
                {
                    if (!slot.TryGetTierReadout(tier, out var readout)
                        || !readout.HasReadout)
                    {
                        throw new InvalidOperationException(
                            $"PREP-01 `{slot.SlotId}` tier {tier} lacks a canonical readout.");
                    }
                }
            }
        }

        private static void ValidateInitialSessionDefaults()
        {
            if (controller.Session == null
                || controller.CurrentPhase != StagePreparationReviewPhase.StageIntel
                || controller.CurrentPanel != StagePreparationReviewPanel.StageIntel
                || controller.IsReviewAccepted
                || controller.ConfirmationDispatchCount != 0
                || !string.IsNullOrEmpty(controller.LastConfirmedSelectionDigest))
            {
                throw new InvalidOperationException(
                    "PREP-01 restart did not create a clean session-local review.");
            }

            ValidateExactSlotBindingsAndActions();
            StagePreparationReviewSelection[] snapshot = controller.SelectionSnapshot;
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i].SelectedTier != 1
                    || !controller.TryGetSelectedTier(ExpectedSlotIds[i], out int tier)
                    || tier != 1)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 restart did not reset `{ExpectedSlotIds[i]}` to session tier 1.");
                }
            }
        }

        private static void ValidateExpectedTierSnapshot(ReviewCaptureState state)
        {
            int[] expected = ResolveExpectedTiers(state);
            StagePreparationReviewSelection[] snapshot = controller.SelectionSnapshot;
            if (snapshot.Length != expected.Length)
            {
                throw new InvalidOperationException(
                    $"PREP-01 tier snapshot size mismatch for {state}.");
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (snapshot[i].SelectedTier != expected[i]
                    || !controller.TryGetSelectedTier(ExpectedSlotIds[i], out int selected)
                    || selected != expected[i])
                {
                    throw new InvalidOperationException(
                        $"PREP-01 session-local tier mismatch for {state} / "
                        + $"{ExpectedSlotIds[i]}: expected {expected[i]}.");
                }

                string renderedTier = controller.GetSlotBinding(i).SelectedTierText?.text
                    ?? string.Empty;
                if (!string.Equals(
                    renderedTier,
                    $"TIER {expected[i]}",
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"PREP-01 rendered tier mismatch for {ExpectedSlotIds[i]} in {state}.");
                }
            }
        }

        private static void ValidateTierButtonPresentation(ReviewCaptureState state)
        {
            if (!IsDetailState(state))
            {
                return;
            }

            Button[] buttons =
            {
                detailTier1Button,
                detailTier2Button,
                detailTier3Button
            };
            int selectedIndex = ResolveDetailTier(state) - 1;
            Color selectedBackground = RequireTargetGraphicColor(
                buttons[selectedIndex]);
            Color selectedLabel = RequireButtonLabel(buttons[selectedIndex]).color;
            Color? unselectedBackground = null;
            Color? unselectedLabel = null;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == selectedIndex)
                {
                    continue;
                }

                Color background = RequireTargetGraphicColor(buttons[i]);
                Color label = RequireButtonLabel(buttons[i]).color;
                if (unselectedBackground.HasValue
                    && (!ColorsApproximatelyEqual(
                            unselectedBackground.Value,
                            background)
                        || !ColorsApproximatelyEqual(
                            unselectedLabel.Value,
                            label)))
                {
                    throw new InvalidOperationException(
                        $"PREP-01 unselected tier buttons disagree for {state}.");
                }

                unselectedBackground = background;
                unselectedLabel = label;
            }

            if (!unselectedBackground.HasValue
                || ColorsApproximatelyEqual(
                    selectedBackground,
                    unselectedBackground.Value)
                || ColorsApproximatelyEqual(
                    selectedLabel,
                    unselectedLabel.Value))
            {
                throw new InvalidOperationException(
                    $"PREP-01 selected tier is not visually distinct for {state}.");
            }
        }

        private static Color RequireTargetGraphicColor(Button button)
        {
            if (button == null || button.targetGraphic == null)
            {
                throw new InvalidOperationException(
                    "PREP-01 tier button is missing its target graphic.");
            }

            return button.targetGraphic.color;
        }

        private static TMP_Text RequireButtonLabel(Button button)
        {
            TMP_Text label = button == null
                ? null
                : button.GetComponentInChildren<TMP_Text>(includeInactive: true);
            if (label == null)
            {
                throw new InvalidOperationException(
                    "PREP-01 tier button is missing its TMP label.");
            }

            return label;
        }

        private static void ValidateCanonicalProjection(ReviewCaptureState state)
        {
            UIStageRouteProjection projection = controller.CurrentProjection;
            if (projection == null
                || controller.StageProjection != projection
                || controller.LastProjectionRejectReason
                    != UIStageRouteProjectionRejectReason.None
                || controller.ProjectionRefreshCount <= 0
                || !string.Equals(
                    projection.CatalogEntryId,
                    OlympusStagePreparationReviewSetup.CanonicalCatalogEntryId,
                    StringComparison.Ordinal)
                || projection.UiRouteId != UIRouteId.Combat
                || projection.Briefing == null
                || string.IsNullOrWhiteSpace(projection.CanonicalProjectionDigest)
                || string.IsNullOrWhiteSpace(projection.CanonicalBriefingDigest))
            {
                throw new InvalidOperationException(
                    $"PREP-01 canonical projection is incomplete in {state}.");
            }

            if (!stageCatalog.IsProjectionCurrent(
                    projection,
                    UIRouteId.Combat,
                    out UIStageRouteProjectionRejectReason rejectReason))
            {
                throw new InvalidOperationException(
                    $"PREP-01 canonical projection is stale in {state}: {rejectReason}.");
            }

            StageBriefingReadModel briefing = projection.Briefing;
            if (!controller.HasNeutralStageRecommendationBoundary
                || briefing.FeaturedThreatDisposition
                    != StageBriefingValueDisposition.NoVerifiedSource
                || briefing.RecommendedLoadoutDisposition
                    != StageBriefingValueDisposition.NoVerifiedSource
                || briefing.FeaturedSummonNeedDisposition
                    != StageBriefingValueDisposition.NoVerifiedSource
                || !string.IsNullOrEmpty(briefing.RecommendedLoadout)
                || briefing.FeaturedSummonNeed != StageSummonNeed.None)
            {
                throw new InvalidOperationException(
                    $"PREP-01 neutral recommendation boundary is invalid in {state}.");
            }
        }

        private static void ValidateNeutralRuntimeTextBoundary(ReviewCaptureState state)
        {
            if (!string.Equals(
                    intelThreatTagsText.text,
                    OlympusStagePreparationReviewController.NeutralThreatPreviewStatus,
                    StringComparison.Ordinal)
                || !string.Equals(
                    intelRecommendedSummonRoleText.text,
                    OlympusStagePreparationReviewController.NeutralRuntimePresetStatus,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"PREP-01 neutral intel notation is not exact in {state}.");
            }

            TMP_Text[] visibleTexts = FindAllInScene<TMP_Text>(SceneManager.GetActiveScene())
                .Where(text => IsActuallyVisible(text.gameObject)
                    && !string.IsNullOrWhiteSpace(text.text))
                .ToArray();
            string combined = string.Join(
                "\n",
                visibleTexts.Select(text => text.GetParsedText() ?? text.text));
            if (combined.IndexOf(
                    OlympusStagePreparationReviewController.PresetBoundaryStatus,
                    StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException(
                    $"PREP-01 `{OlympusStagePreparationReviewController.PresetBoundaryStatus}` "
                    + $"is not visible in {state}.");
            }

            if (!stageCatalog.TryGetStage(
                    OlympusStagePreparationReviewSetup.CanonicalCatalogEntryId,
                    out UIStageCatalog.StageEntry rawCatalogEntry))
            {
                throw new InvalidOperationException(
                    "PREP-01 canonical catalog entry is unavailable.");
            }
            if (!string.Equals(
                    rawCatalogEntry.Id,
                    OlympusStagePreparationReviewSetup.CanonicalCatalogEntryId,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(rawCatalogEntry.ThreatTags)
                || string.IsNullOrWhiteSpace(rawCatalogEntry.RecommendedSummonRole))
            {
                throw new InvalidOperationException(
                    "PREP-01 raw catalog legacy-copy boundary is unavailable for a meaningful "
                    + $"non-exposure check in {state}.");
            }

            foreach (string legacy in new[]
            {
                rawCatalogEntry.ThreatTags,
                rawCatalogEntry.RecommendedSummonRole
            })
            {
                if (!string.IsNullOrWhiteSpace(legacy)
                    && combined.IndexOf(legacy, StringComparison.Ordinal) >= 0)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 exposes legacy projection copy `{legacy}` in {state}.");
                }
            }

            string upper = combined.ToUpperInvariant();
            foreach (string forbidden in ForbiddenProgressionPhrases)
            {
                if (upper.IndexOf(forbidden.ToUpperInvariant(), StringComparison.Ordinal) >= 0)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 exposes forbidden account/ownership/level/power copy "
                        + $"`{forbidden}` in {state}.");
                }
            }
        }

        private static void ValidateTierReadout(ReviewCaptureState state)
        {
            int slotIndex = ResolveDetailSlotIndex(state);
            int tier = ResolveDetailTier(state);
            StagePreparationReviewProfile.SlotDefinition slot = profile.GetSlot(slotIndex);
            if (!slot.TryGetTierReadout(tier, out var readout)
                || !readout.HasReadout
                || !string.Equals(
                    detailSelectedTierText.text,
                    readout.TierLabel,
                    StringComparison.Ordinal)
                || !string.Equals(
                    detailStageRoleText.text,
                    readout.StageRole,
                    StringComparison.Ordinal)
                || !string.Equals(
                    detailPlayerUseText.text,
                    readout.PlayerUse,
                    StringComparison.Ordinal)
                || !string.Equals(
                    detailSummonReadText.text,
                    readout.SummonRead,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"PREP-01 canonical tier readout mismatch for {state}.");
            }
        }

        private static void ValidateExpectedFocus(ReviewCaptureState state)
        {
            GameObject expected = state switch
            {
                ReviewCaptureState.StageIntel => intelContinueButton.gameObject,
                ReviewCaptureState.LoadoutOverview =>
                    controller.GetSlotBinding(0).InspectButton.gameObject,
                ReviewCaptureState.Slot1Tier1Detail => detailTier1Button.gameObject,
                ReviewCaptureState.Slot2Tier2Detail => detailTier2Button.gameObject,
                ReviewCaptureState.Slot3Tier3Detail => detailTier3Button.gameObject,
                ReviewCaptureState.ReviewConfirmBefore => confirmAcceptButton.gameObject,
                ReviewCaptureState.ReviewConfirmAfter => confirmRestartButton.gameObject,
                _ => null
            };
            if (expected == null
                || controller.LastFocusTarget != expected
                || eventSystem == null
                || eventSystem.currentSelectedGameObject != expected
                || !expected.activeInHierarchy)
            {
                throw new InvalidOperationException(
                    $"PREP-01 focus did not settle on `{expected?.name}` for {state}.");
            }
        }

        private static void ValidateImageTextSeparation(ReviewCaptureState state)
        {
            if (state == ReviewCaptureState.LoadoutOverview)
            {
                for (int i = 0; i < controller.SlotBindingCount; i++)
                {
                    OlympusStagePreparationReviewController.SlotBinding binding =
                        controller.GetSlotBinding(i);
                    RequireVisibleRectsDoNotOverlap(
                        binding.IconImage?.rectTransform,
                        binding.TitleText?.rectTransform,
                        $"loadout slot {i + 1} icon / title");
                }

                return;
            }

            if (!IsDetailState(state))
            {
                return;
            }

            RequireVisibleRectsDoNotOverlap(
                detailIconImage?.rectTransform,
                detailTitleText?.rectTransform,
                "detail icon / title");
            RequireVisibleRectsDoNotOverlap(
                detailIconImage?.rectTransform,
                detailRoleText?.rectTransform,
                "detail icon / role");
            RequireVisibleRectsDoNotOverlap(
                detailIconImage?.rectTransform,
                FindRectByName("TierPrompt"),
                "detail icon / tier prompt");
        }

        private static void ValidateConfirmationButtonAffordance(
            ReviewCaptureState state)
        {
            bool before = state == ReviewCaptureState.ReviewConfirmBefore;
            bool after = state == ReviewCaptureState.ReviewConfirmAfter;
            if (!before && !after)
            {
                return;
            }

            bool backUsable = IsActuallyVisible(confirmBackButton.gameObject)
                && confirmBackButton.IsInteractable();
            bool acceptUsable = IsActuallyVisible(confirmAcceptButton.gameObject)
                && confirmAcceptButton.IsInteractable();
            bool restartUsable = IsActuallyVisible(confirmRestartButton.gameObject)
                && confirmRestartButton.IsInteractable();
            if (!backUsable
                || (before
                    && (!acceptUsable
                        || confirmRestartButton.gameObject.activeSelf
                        || confirmRestartButton.interactable))
                || (after
                    && (confirmAcceptButton.gameObject.activeSelf
                        || confirmAcceptButton.interactable
                        || !restartUsable)))
            {
                throw new InvalidOperationException(
                    $"PREP-01 confirmation button affordance is ambiguous in {state}: "
                    + $"back={backUsable}, accept={acceptUsable}/"
                    + $"activeSelf={confirmAcceptButton.gameObject.activeSelf}, "
                    + $"restart={restartUsable}/"
                    + $"activeSelf={confirmRestartButton.gameObject.activeSelf}.");
            }

            RequireVisibleRectsDoNotOverlap(
                confirmBackButton.transform as RectTransform,
                before
                    ? confirmAcceptButton.transform as RectTransform
                    : confirmRestartButton.transform as RectTransform,
                before
                    ? "confirm back / accept buttons"
                    : "confirm back / restart buttons");
        }

        private static void RequireVisibleRectsDoNotOverlap(
            RectTransform first,
            RectTransform second,
            string label)
        {
            if (first == null
                || second == null
                || !IsActuallyVisible(first.gameObject)
                || !IsActuallyVisible(second.gameObject))
            {
                throw new InvalidOperationException(
                    $"PREP-01 overlap regression boundary `{label}` is not visibly authored.");
            }

            Rect firstRect = CalculateScreenRect(first);
            Rect secondRect = CalculateScreenRect(second);
            if (firstRect.Overlaps(secondRect))
            {
                throw new InvalidOperationException(
                    $"PREP-01 visual overlap regression: {label}; "
                    + $"first={firstRect}, second={secondRect}.");
            }
        }

        private static void ValidateConfirmationContract(ReviewCaptureState state)
        {
            int eventDelta = observedConfirmationEventCount - confirmationEventBaseline;
            bool before = state == ReviewCaptureState.ReviewConfirmBefore;
            bool after = state == ReviewCaptureState.ReviewConfirmAfter;
            if (before)
            {
                if (controller.IsReviewAccepted
                    || !controller.IsConfirmationAvailable
                    || controller.ConfirmationDispatchCount != 0
                    || eventDelta != 0
                    || firstConfirmationAccepted
                    || duplicateConfirmationAccepted
                    || !string.IsNullOrEmpty(controller.LastConfirmedSelectionDigest)
                    || !string.IsNullOrEmpty(observedConfirmedDigest)
                    || !string.Equals(
                        confirmDigestText.text,
                        preparedSelectionDigest,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "PREP-01 pre-confirm state violates the confirmation contract.");
                }

                return;
            }

            if (after)
            {
                if (!firstConfirmationAccepted
                    || duplicateConfirmationAccepted
                    || !controller.IsReviewAccepted
                    || controller.IsConfirmationAvailable
                    || controller.ConfirmationDispatchCount != 1
                    || eventDelta != 1
                    || !string.Equals(
                        controller.LastConfirmedSelectionDigest,
                        preparedSelectionDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        observedConfirmedDigest,
                        preparedSelectionDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        confirmDigestText.text,
                        preparedSelectionDigest,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "PREP-01 confirmation is not exact-once or changed its digest.");
                }

                return;
            }

            if (controller.IsReviewAccepted
                || controller.IsConfirmationAvailable
                || controller.ConfirmationDispatchCount != 0
                || eventDelta != 0
                || firstConfirmationAccepted
                || duplicateConfirmationAccepted
                || !string.IsNullOrEmpty(controller.LastConfirmedSelectionDigest)
                || !string.IsNullOrEmpty(observedConfirmedDigest))
            {
                throw new InvalidOperationException(
                    $"PREP-01 non-confirm state {state} changed confirmation state.");
            }
        }

        private static string RequireSelectionDigest(ReviewCaptureState state)
        {
            string digest = controller.CurrentSelectionDigest;
            if (string.IsNullOrWhiteSpace(digest) || digest.Length != 64)
            {
                throw new InvalidOperationException(
                    $"PREP-01 selection digest is unavailable for {state}.");
            }

            return digest;
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
            RectTransform safeRect = safeAreaRoot.transform as RectTransform
                ?? throw new InvalidOperationException(
                    "PREP-01 safe-area RectTransform is missing.");
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
                name = $"OlympusStagePreparationReviewQA_{plan.Width}x{plan.Height}",
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
                ValidateExpectedRuntimeState(plan.State);
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

        private static void ApplyVirtualSafeArea(
            RectTransform safeRect,
            CapturePlan plan)
        {
            safeAreaRoot.enabled = false;
            int left = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? LeadingNotchInsetPixels
                : TrailingNotchInsetPixels;
            int right = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? TrailingNotchInsetPixels
                : LeadingNotchInsetPixels;
            safeRect.anchorMin = new Vector2(
                left / (float)Mathf.Max(1, plan.Width),
                VerticalSafeInsetPixels / (float)Mathf.Max(1, plan.Height));
            safeRect.anchorMax = new Vector2(
                1f - (right / (float)Mathf.Max(1, plan.Width)),
                1f - (VerticalSafeInsetPixels / (float)Mathf.Max(1, plan.Height)));
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;
        }

        private static CaptureLayoutEvidence ValidateCaptureLayout(
            CapturePlan plan,
            RectTransform safeRect)
        {
            int expectedLeft = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? LeadingNotchInsetPixels
                : TrailingNotchInsetPixels;
            int expectedRight = plan.NotchOrientation == VirtualNotchOrientation.Left
                ? TrailingNotchInsetPixels
                : LeadingNotchInsetPixels;
            if (!Mathf.Approximately(
                    safeRect.anchorMin.x,
                    expectedLeft / (float)plan.Width)
                || !Mathf.Approximately(
                    safeRect.anchorMax.x,
                    1f - (expectedRight / (float)plan.Width))
                || !Mathf.Approximately(
                    safeRect.anchorMin.y,
                    VerticalSafeInsetPixels / (float)plan.Height)
                || !Mathf.Approximately(
                    safeRect.anchorMax.y,
                    1f - (VerticalSafeInsetPixels / (float)plan.Height))
                || safeRect.offsetMin != Vector2.zero
                || safeRect.offsetMax != Vector2.zero)
            {
                throw new InvalidOperationException(
                    $"PREP-01 virtual {plan.NotchOrientation} notch was not applied exactly.");
            }

            Rect safeBounds = CalculateScreenRect(safeRect);
            Graphic[] visibleGraphics = safeRect
                .GetComponentsInChildren<Graphic>(includeInactive: true)
                .Where(graphic => IsActuallyVisible(graphic.gameObject))
                .ToArray();
            if (visibleGraphics.Length == 0)
            {
                throw new InvalidOperationException(
                    $"PREP-01 has no visible safe-area graphics for {plan.State}.");
            }

            foreach (Graphic graphic in visibleGraphics)
            {
                RectTransform rect = graphic.transform as RectTransform;
                if (rect == null
                    || !Contains(safeBounds, CalculateScreenRect(rect), 1.5f))
                {
                    throw new InvalidOperationException(
                        $"PREP-01 visible graphic `{graphic.name}` escapes the virtual safe area "
                        + $"at {plan.Width}x{plan.Height}.");
                }
            }

            TMP_Text[] visibleTexts = visibleGraphics
                .OfType<TMP_Text>()
                .Where(text => !string.IsNullOrWhiteSpace(text.text))
                .ToArray();
            if (visibleTexts.Length == 0)
            {
                throw new InvalidOperationException(
                    $"PREP-01 has no visible text evidence for {plan.State}.");
            }

            foreach (TMP_Text text in visibleTexts)
            {
                ValidateVisibleTextFits(text);
            }

            int overlapPairs = ValidateRenderedTextNoOverlap(visibleTexts, plan.State);
            int targetCount = ValidateMinimumTargetsAndRaycasts(plan.State);
            ValidateNeutralRuntimeTextBoundary(plan.State);
            return new CaptureLayoutEvidence(
                visibleGraphics.Length,
                visibleTexts.Length,
                overlapPairs,
                targetCount,
                safeAreaContainmentValidated: true,
                visibleTextFitValidated: true,
                textOverlapValidated: true,
                raycastValidated: true,
                minimumTargetValidated: true);
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
                    $"PREP-01 visible text `{text.name}` is clipped or overflowing: preferred="
                    + $"{text.preferredWidth:0.0}x{text.preferredHeight:0.0}, available="
                    + $"{availableWidth:0.0}x{availableHeight:0.0}, "
                    + $"isOverflowing={text.isTextOverflowing}.");
            }
        }

        private static int ValidateRenderedTextNoOverlap(
            TMP_Text[] visibleTexts,
            ReviewCaptureState state)
        {
            var rendered = new List<KeyValuePair<TMP_Text, Rect>>(visibleTexts.Length);
            foreach (TMP_Text text in visibleTexts)
            {
                Rect bounds = CalculateRenderedTextScreenRect(text);
                if (bounds.width > 0.5f && bounds.height > 0.5f)
                {
                    rendered.Add(new KeyValuePair<TMP_Text, Rect>(text, bounds));
                }
            }

            int pairCount = 0;
            for (int i = 0; i < rendered.Count; i++)
            {
                for (int j = i + 1; j < rendered.Count; j++)
                {
                    pairCount++;
                    if (OverlapsWithTolerance(rendered[i].Value, rendered[j].Value, 1.5f))
                    {
                        throw new InvalidOperationException(
                            $"PREP-01 rendered TMP text `{rendered[i].Key.name}` and "
                            + $"`{rendered[j].Key.name}` overlap in {state}.");
                    }
                }
            }

            if (pairCount == 0)
            {
                throw new InvalidOperationException(
                    $"PREP-01 produced no TMP overlap evidence for {state}.");
            }

            return pairCount;
        }

        private static int ValidateMinimumTargetsAndRaycasts(ReviewCaptureState state)
        {
            if (graphicRaycaster == null || !graphicRaycaster.enabled || eventSystem == null)
            {
                throw new InvalidOperationException(
                    $"PREP-01 raycast infrastructure is unavailable in {state}.");
            }

            CanvasGroup activePanel = ResolveCurrentPanelGroup();
            Button[] visibleButtons = activePanel
                .GetComponentsInChildren<Button>(includeInactive: true)
                .Where(button => IsActuallyVisible(button.gameObject) && button.IsInteractable())
                .ToArray();
            if (visibleButtons.Length == 0)
            {
                throw new InvalidOperationException(
                    $"PREP-01 has no usable target/raycast evidence in {state}.");
            }

            foreach (Button button in visibleButtons)
            {
                RectTransform rectTransform = button.transform as RectTransform
                    ?? throw new InvalidOperationException(
                        $"PREP-01 button `{button.name}` lacks a RectTransform.");
                Rect screenRect = CalculateScreenRect(rectTransform);
                if (screenRect.width + 0.25f < MinimumTouchTargetPixels
                    || screenRect.height + 0.25f < MinimumTouchTargetPixels)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 target `{button.name}` is {screenRect.width:0.0}x"
                        + $"{screenRect.height:0.0}px in {state}; minimum is 48x48px.");
                }

                if (button.targetGraphic == null || !button.targetGraphic.raycastTarget)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 target `{button.name}` has no raycastable target graphic.");
                }

                var pointer = new PointerEventData(eventSystem)
                {
                    position = screenRect.center
                };
                var results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointer, results);
                Button topButton = results.Count > 0
                    ? results[0].gameObject.GetComponentInParent<Button>()
                    : null;
                if (topButton != button)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 raycast at `{button.name}` resolves to "
                        + $"`{topButton?.name ?? results.FirstOrDefault().gameObject?.name ?? "none"}` "
                        + $"in {state}.");
                }
            }

            foreach (CanvasGroup inactive in ResolveInactivePanelGroups())
            {
                if (inactive.gameObject.activeInHierarchy
                    || inactive.interactable
                    || inactive.blocksRaycasts
                    || inactive.alpha > 0.001f)
                {
                    throw new InvalidOperationException(
                        $"PREP-01 inactive panel `{inactive.name}` can still receive raycasts.");
                }
            }

            return visibleButtons.Length;
        }

        private static CaptureRecord BuildCaptureRecord(
            CapturePlan plan,
            string path,
            long fileBytes,
            PixelAudit audit,
            CaptureLayoutEvidence layout)
        {
            UIStageRouteProjection projection = controller.CurrentProjection;
            StagePreparationReviewSelection[] snapshot = controller.SelectionSnapshot;
            int eventDelta = observedConfirmationEventCount - confirmationEventBaseline;
            bool projectionCurrent = stageCatalog.IsProjectionCurrent(
                projection,
                UIRouteId.Combat,
                out UIStageRouteProjectionRejectReason projectionRejectReason);
            string canonicalDigest =
                OlympusStagePreparationReviewSetup.ComputeCanonicalBoundaryDigest();
            string expectedDigest = SessionState.GetString(
                CanonicalDigestBeforeKey,
                string.Empty);
            if (!string.Equals(canonicalDigest, expectedDigest, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"PREP-01 canonical boundary changed while capturing {plan.State}.");
            }

            return new CaptureRecord
            {
                Sequence = plan.Sequence,
                State = plan.State.ToString(),
                Width = plan.Width,
                Height = plan.Height,
                NotchOrientation = plan.NotchOrientation.ToString(),
                VirtualLeftInsetPixels = plan.NotchOrientation
                    == VirtualNotchOrientation.Left
                        ? LeadingNotchInsetPixels
                        : TrailingNotchInsetPixels,
                VirtualRightInsetPixels = plan.NotchOrientation
                    == VirtualNotchOrientation.Left
                        ? TrailingNotchInsetPixels
                        : LeadingNotchInsetPixels,
                Path = path,
                FileBytes = fileBytes,
                MeanLuminance = audit.MeanLuminance,
                LuminanceRange = audit.LuminanceRange,
                ControllerPhase = controller.CurrentPhase.ToString(),
                ControllerPanel = controller.CurrentPanel.ToString(),
                SelectedSlotId = controller.SelectedSlotId,
                SelectedTier = controller.SelectedTier,
                ActionIds = string.Join(";", snapshot.Select(item => item.ActionId)),
                TierSnapshot = string.Join(
                    ";",
                    snapshot.Select(item => item.SlotId + "=" + item.SelectedTier)),
                CurrentSelectionDigest = controller.CurrentSelectionDigest,
                PreparedSelectionDigest = preparedSelectionDigest,
                LastConfirmedSelectionDigest = controller.LastConfirmedSelectionDigest,
                ProjectionCatalogEntryId = projection?.CatalogEntryId ?? string.Empty,
                ProjectionDigest = projection?.CanonicalProjectionDigest ?? string.Empty,
                ProjectionBriefingDigest = projection?.CanonicalBriefingDigest ?? string.Empty,
                ProjectionCurrent = projectionCurrent,
                ProjectionRejectReason = projectionRejectReason.ToString(),
                NeutralRecommendationBoundaryValidated =
                    controller.HasNeutralStageRecommendationBoundary,
                NeutralThreatNotation = intelThreatTagsText.text,
                NeutralRuntimePresetNotation = intelRecommendedSummonRoleText.text,
                LegacyProjectionCopyAbsent = HasNoVisibleLegacyProjectionCopy(),
                ForbiddenProgressionCopyAbsent = HasNoVisibleForbiddenProgressionCopy(),
                TierReadoutValidated = !IsDetailState(plan.State)
                    || IsCurrentTierReadoutExact(plan.State),
                SessionLocalTierValidated = IsTierSnapshotExact(plan.State),
                ReviewAccepted = controller.IsReviewAccepted,
                ConfirmationFirstAccepted = firstConfirmationAccepted,
                ConfirmationDuplicateAccepted = duplicateConfirmationAccepted,
                ConfirmationDispatchCount = controller.ConfirmationDispatchCount,
                ConfirmationEventDelta = eventDelta,
                ObservedConfirmedDigest = observedConfirmedDigest,
                FocusTarget = controller.LastFocusTarget?.name ?? string.Empty,
                ActivePanelValidated = true,
                RaycastValidated = layout.RaycastValidated,
                MinimumTargetValidated = layout.MinimumTargetValidated,
                SafeAreaValidated = layout.SafeAreaContainmentValidated,
                TextFitValidated = layout.VisibleTextFitValidated,
                TextOverlapValidated = layout.TextOverlapValidated,
                VisibleGraphicCount = layout.VisibleGraphicCount,
                VisibleTextCount = layout.VisibleTextCount,
                EvaluatedTextOverlapPairCount = layout.EvaluatedTextOverlapPairCount,
                UsableTargetCount = layout.UsableTargetCount,
                StageRunActive = StageRunRuntime.HasActiveContext,
                CanonicalBoundaryDigest = canonicalDigest,
                RuntimeContractValidated = true
            };
        }

        private static bool HasNoVisibleLegacyProjectionCopy()
        {
            if (!stageCatalog.TryGetStage(
                    OlympusStagePreparationReviewSetup.CanonicalCatalogEntryId,
                    out UIStageCatalog.StageEntry rawCatalogEntry))
            {
                return false;
            }
            string combined = BuildVisibleTextAggregate();
            foreach (string legacy in new[]
            {
                rawCatalogEntry.ThreatTags,
                rawCatalogEntry.RecommendedSummonRole
            })
            {
                if (!string.IsNullOrWhiteSpace(legacy)
                    && combined.IndexOf(legacy, StringComparison.Ordinal) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasNoVisibleForbiddenProgressionCopy()
        {
            string upper = BuildVisibleTextAggregate().ToUpperInvariant();
            return ForbiddenProgressionPhrases.All(
                phrase => upper.IndexOf(
                    phrase.ToUpperInvariant(),
                    StringComparison.Ordinal) < 0);
        }

        private static string BuildVisibleTextAggregate()
        {
            return string.Join(
                "\n",
                FindAllInScene<TMP_Text>(SceneManager.GetActiveScene())
                    .Where(text => IsActuallyVisible(text.gameObject)
                        && !string.IsNullOrWhiteSpace(text.text))
                    .Select(text => text.GetParsedText() ?? text.text));
        }

        private static bool IsCurrentTierReadoutExact(ReviewCaptureState state)
        {
            int slotIndex = ResolveDetailSlotIndex(state);
            int tier = ResolveDetailTier(state);
            StagePreparationReviewProfile.SlotDefinition slot = profile.GetSlot(slotIndex);
            return slot.TryGetTierReadout(tier, out var readout)
                && string.Equals(
                    detailSelectedTierText.text,
                    readout.TierLabel,
                    StringComparison.Ordinal)
                && string.Equals(
                    detailStageRoleText.text,
                    readout.StageRole,
                    StringComparison.Ordinal)
                && string.Equals(
                    detailPlayerUseText.text,
                    readout.PlayerUse,
                    StringComparison.Ordinal)
                && string.Equals(
                    detailSummonReadText.text,
                    readout.SummonRead,
                    StringComparison.Ordinal);
        }

        private static bool IsTierSnapshotExact(ReviewCaptureState state)
        {
            int[] expected = ResolveExpectedTiers(state);
            StagePreparationReviewSelection[] snapshot = controller.SelectionSnapshot;
            return snapshot.Length == expected.Length
                && snapshot.Select((item, index) => item.SelectedTier == expected[index]).All(
                    value => value);
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
            List<string> issues = ValidateOutputSet();
            string digestBefore = SessionState.GetString(
                CanonicalDigestBeforeKey,
                string.Empty);
            string digestAfterCapture =
                OlympusStagePreparationReviewSetup.ComputeCanonicalBoundaryDigest();
            if (string.IsNullOrWhiteSpace(digestBefore)
                || !string.Equals(
                    digestBefore,
                    digestAfterCapture,
                    StringComparison.Ordinal))
            {
                issues.Add(
                    "PREP-01 canonical boundary changed during Play-mode visual QA.");
            }

            bool capturePassed = issues.Count == 0;
            string failure = capturePassed ? string.Empty : string.Join("\n", issues);
            SessionState.SetString(FailureKey, failure);
            WriteReports(
                automatedPassed: false,
                failure: capturePassed ? "EDIT-MODE POSTFLIGHT PENDING" : failure,
                setupBefore: SessionState.GetBool(SetupBeforeKey, false),
                setupAfter: false,
                digestBefore: digestBefore,
                digestAfter: digestAfterCapture,
                postflightPending: true);

            SessionState.SetInt(
                PhaseKey,
                (int)(capturePassed
                    ? RunnerPhase.SuccessAwaitingEditMode
                    : RunnerPhase.FailureAwaitingEditMode));
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

            string expectedDigest = SessionState.GetString(
                CanonicalDigestBeforeKey,
                string.Empty);
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
                    || !record.ActivePanelValidated
                    || !record.RaycastValidated
                    || !record.MinimumTargetValidated
                    || !record.SafeAreaValidated
                    || !record.TextFitValidated
                    || !record.TextOverlapValidated
                    || !record.ProjectionCurrent
                    || record.ProjectionRejectReason !=
                        UIStageRouteProjectionRejectReason.None.ToString()
                    || !record.NeutralRecommendationBoundaryValidated
                    || !record.LegacyProjectionCopyAbsent
                    || !record.ForbiddenProgressionCopyAbsent
                    || !record.TierReadoutValidated
                    || !record.SessionLocalTierValidated
                    || record.VisibleGraphicCount <= 0
                    || record.VisibleTextCount <= 0
                    || record.EvaluatedTextOverlapPairCount <= 0
                    || record.UsableTargetCount <= 0
                    || record.StageRunActive
                    || !string.Equals(
                        record.ActionIds,
                        string.Join(";", ExpectedActionIds),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        record.NeutralThreatNotation,
                        OlympusStagePreparationReviewController.NeutralThreatPreviewStatus,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        record.NeutralRuntimePresetNotation,
                        OlympusStagePreparationReviewController.NeutralRuntimePresetStatus,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        record.CurrentSelectionDigest,
                        record.PreparedSelectionDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        record.CanonicalBoundaryDigest,
                        expectedDigest,
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Capture {record.Sequence:00} failed a recorded PREP-01 contract check.");
                }

                if (plan.State == ReviewCaptureState.ReviewConfirmAfter
                    && (!record.ReviewAccepted
                        || !record.ConfirmationFirstAccepted
                        || record.ConfirmationDuplicateAccepted
                        || record.ConfirmationDispatchCount != 1
                        || record.ConfirmationEventDelta != 1
                        || !string.Equals(
                            record.LastConfirmedSelectionDigest,
                            record.PreparedSelectionDigest,
                            StringComparison.Ordinal)
                        || !string.Equals(
                            record.ObservedConfirmedDigest,
                            record.PreparedSelectionDigest,
                            StringComparison.Ordinal)))
                {
                    issues.Add(
                        $"Capture {record.Sequence:00} lacks exact-once digest evidence.");
                }
            }

            foreach (ReviewCaptureState state in Enum.GetValues(typeof(ReviewCaptureState)))
            {
                CaptureRecord[] group = Records.Where(record => string.Equals(
                    record.State,
                    state.ToString(),
                    StringComparison.Ordinal)).ToArray();
                int left = group.Count(record => record.NotchOrientation == "Left");
                int right = group.Count(record => record.NotchOrientation == "Right");
                if (group.Length != 3 || Math.Abs(left - right) != 1)
                {
                    issues.Add(
                        $"State {state} must have three captures and balanced notch directions.");
                }
            }

            foreach (string resolution in new[]
            {
                "1920x1080",
                "2400x1080",
                "2520x1080"
            })
            {
                CaptureRecord[] group = Records.Where(
                    record => $"{record.Width}x{record.Height}" == resolution).ToArray();
                int left = group.Count(record => record.NotchOrientation == "Left");
                int right = group.Count(record => record.NotchOrientation == "Right");
                if (group.Length != 7 || Math.Abs(left - right) != 1)
                {
                    issues.Add(
                        $"Resolution {resolution} must have seven captures and a 4/3 notch split.");
                }
            }

            int totalLeft = Records.Count(record => record.NotchOrientation == "Left");
            int totalRight = Records.Count(record => record.NotchOrientation == "Right");
            if (Math.Abs(totalLeft - totalRight) != 1)
            {
                issues.Add(
                    $"The 21-capture notch matrix is not balanced: {totalLeft}/{totalRight}.");
            }

            return issues;
        }

        private static void ResetOutputArtifacts()
        {
            Directory.CreateDirectory(OutputDirectory);
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
                ? "Unknown PREP-01 visual QA failure."
                : failure;
            SessionState.SetString(FailureKey, resolved);
            WriteReports(
                automatedPassed: false,
                failure: resolved,
                setupBefore: SessionState.GetBool(SetupBeforeKey, false),
                setupAfter: false,
                digestBefore: SessionState.GetString(
                    CanonicalDigestBeforeKey,
                    string.Empty),
                digestAfter: TryComputeCanonicalBoundaryDigest(),
                postflightPending: true);
            Debug.LogError(
                "[OlympusStagePreparationReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL\n"
                + resolved);
            SessionState.SetInt(PhaseKey, (int)RunnerPhase.FailureAwaitingEditMode);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
            }
        }

        private static void WriteReports(
            bool automatedPassed,
            string failure,
            bool setupBefore,
            bool setupAfter,
            string digestBefore,
            string digestAfter,
            bool postflightPending)
        {
            Directory.CreateDirectory(OutputDirectory);
            bool digestStable = !string.IsNullOrWhiteSpace(digestBefore)
                && string.Equals(digestBefore, digestAfter, StringComparison.Ordinal);
            var manifest = new CaptureManifest
            {
                Scene = ScenePath,
                GeneratedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                AutomatedPassed = automatedPassed,
                HumanReviewRequired = true,
                HumanReviewed = false,
                PostflightPending = postflightPending,
                Failure = failure ?? string.Empty,
                ExpectedCaptureCount = ExpectedCaptureCount,
                SetupVerificationBefore = setupBefore,
                SetupVerificationAfter = setupAfter,
                CanonicalDigestBefore = digestBefore ?? string.Empty,
                CanonicalDigestAfter = digestAfter ?? string.Empty,
                CanonicalDigestStable = digestStable,
                NavigationBoundary =
                    "RestartReview/OpenLoadout/InspectSlot/SelectTier/ReturnToLoadout/"
                    + "OpenReviewConfirm/ConfirmReview",
                CanonicalHashBoundary =
                    "UIStageCatalog + 3 summon action profiles + 3 slot icons + Olympus "
                    + "background + responsive catalog + 2 Pretendard source OTFs, including "
                    + "each .meta; dynamic TMP refs semantically validated; generated review "
                    + "profile/scene excluded",
                Captures = Records.OrderBy(record => record.Sequence).ToArray()
            };
            File.WriteAllText(
                ManifestPath,
                JsonUtility.ToJson(manifest, prettyPrint: true),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var report = new StringBuilder();
            report.AppendLine("# PREP-01 Olympus Stage Preparation Visual QA");
            report.AppendLine();
            report.AppendLine(
                automatedPassed
                    ? "Automated capture check: PASS"
                    : postflightPending
                        ? "Automated capture check: POSTFLIGHT PENDING"
                        : "Automated capture check: FAIL");
            report.AppendLine("Human visual review: PENDING (must be recorded separately)");
            report.AppendLine();
            report.AppendLine($"- Scene: `{ScenePath}`");
            report.AppendLine($"- Output: `{OutputDirectory}`");
            report.AppendLine($"- Captures: `{Records.Count}` / `{ExpectedCaptureCount}`");
            report.AppendLine("- Resolutions: `1920x1080`, `2400x1080`, `2520x1080`");
            report.AppendLine(
                "- States: StageIntel, LoadoutOverview, Slot1/Tier1, Slot2/Tier2, "
                + "Slot3/Tier3, ReviewConfirm before/after");
            report.AppendLine(
                "- Virtual safe area: asymmetric left/right notch, globally balanced 11/10");
            report.AppendLine("- State preparation: public controller navigation only");
            report.AppendLine(
                $"- Setup verification: before=`{setupBefore}`; after=`{setupAfter}`");
            report.AppendLine($"- Canonical digest before: `{digestBefore}`");
            report.AppendLine($"- Canonical digest after: `{digestAfter}`");
            report.AppendLine($"- Canonical digest stable: `{digestStable}`");
            report.AppendLine("- HumanReviewRequired: `true`; HumanReviewed: `false`");
            report.AppendLine(
                "- Projection: current canonical Combat projection; neutral briefing "
                + "dispositions only; legacy threat/recommended-role copy absent");
            report.AppendLine(
                "- Selection: exact three action IDs, session-local tiers/readouts, "
                + "exact-once confirmation digest");
            report.AppendLine(
                "- Runtime layout: one active/raycastable panel, exact focus, 48px targets, "
                + "safe-area containment, selected-tier presentation, TMP fit/overlap checks");
            report.AppendLine("- StageRun ownership/context: absent");
            report.AppendLine();
            if (!string.IsNullOrWhiteSpace(failure))
            {
                report.AppendLine("## Failure / pending state");
                report.AppendLine();
                report.AppendLine("```text");
                report.AppendLine(failure);
                report.AppendLine("```");
                report.AppendLine();
            }

            report.AppendLine("## Captures");
            report.AppendLine();
            report.AppendLine(
                "| # | State | Resolution | Notch | Phase / Panel | Slot / Tier | "
                + "Projection | Confirm | Focus |");
            report.AppendLine("|---:|---|---|---|---|---|---|---|---|");
            foreach (CaptureRecord record in Records.OrderBy(item => item.Sequence))
            {
                report.AppendLine(
                    $"| {record.Sequence:00} | {record.State} | "
                    + $"{record.Width}x{record.Height} | {record.NotchOrientation} | "
                    + $"{record.ControllerPhase} / {record.ControllerPanel} | "
                    + $"{record.SelectedSlotId} / {record.SelectedTier} | "
                    + $"current={record.ProjectionCurrent}; "
                    + $"`{EscapeMarkdown(record.ProjectionCatalogEntryId)}` | "
                    + $"accepted={record.ReviewAccepted}; dispatch="
                    + $"{record.ConfirmationDispatchCount} | "
                    + $"{EscapeMarkdown(record.FocusTarget)} |");
            }

            report.AppendLine();
            report.AppendLine(
                "Automated success does not attest composition, contrast, hierarchy, or "
                + "visual polish. Inspect all 21 PNGs before recording human review.");
            File.WriteAllText(
                ReportPath,
                report.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void FinalizeEditorSession(bool capturePhasePassed)
        {
            bool exit = SessionState.GetBool(BatchExitKey, false);
            bool setupBefore = SessionState.GetBool(SetupBeforeKey, false);
            string digestBefore = SessionState.GetString(
                CanonicalDigestBeforeKey,
                string.Empty);
            string failure = SessionState.GetString(FailureKey, string.Empty);
            RestoreRecordsFromManifest();

            bool setupAfter = false;
            string digestAfter = string.Empty;
            var postflightIssues = new List<string>();
            try
            {
                OlympusStagePreparationReviewSetup.RunBatchVerification();
                setupAfter = true;
                digestAfter =
                    OlympusStagePreparationReviewSetup.ComputeCanonicalBoundaryDigest();
            }
            catch (Exception exception)
            {
                postflightIssues.Add(
                    "Edit-mode setup postflight failed: " + exception.Message);
                Debug.LogException(exception);
            }

            if (!setupBefore)
            {
                postflightIssues.Add("Setup verification did not pass before capture.");
            }

            if (!setupAfter)
            {
                postflightIssues.Add("Setup verification did not pass after capture.");
            }

            if (string.IsNullOrWhiteSpace(digestBefore)
                || !string.Equals(digestBefore, digestAfter, StringComparison.Ordinal))
            {
                postflightIssues.Add(
                    "Canonical boundary digest differs before/after PREP-01 capture.");
            }

            if (!string.IsNullOrWhiteSpace(failure))
            {
                postflightIssues.Insert(0, failure);
            }

            bool success = capturePhasePassed && postflightIssues.Count == 0;
            string finalFailure = success
                ? string.Empty
                : string.Join("\n", postflightIssues.Distinct());
            WriteReports(
                success,
                finalFailure,
                setupBefore,
                setupAfter,
                digestBefore,
                digestAfter,
                postflightPending: false);

            if (success)
            {
                Debug.Log(
                    "[OlympusStagePreparationReviewVisualQA] BATCH_CAPTURE_CHECK_PASS "
                    + $"captures={Records.Count} humanReview=pending "
                    + $"output=`{OutputDirectory}`");
            }
            else
            {
                Debug.LogError(
                    "[OlympusStagePreparationReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL\n"
                    + finalFailure);
            }

            ClearSessionState();
            ResetRuntimeFields();
            if (exit)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void RestoreRecordsFromManifest()
        {
            CaptureRecord[] recovered = Array.Empty<CaptureRecord>();
            if (File.Exists(ManifestPath))
            {
                try
                {
                    CaptureManifest manifest = JsonUtility.FromJson<CaptureManifest>(
                        File.ReadAllText(ManifestPath));
                    recovered = manifest?.Captures ?? Array.Empty<CaptureRecord>();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            Records.Clear();
            Records.AddRange(recovered.Where(record => record != null));
        }

        private static void HandleLaunchFailure(
            Exception exception,
            bool exitEditorWhenFinished)
        {
            Debug.LogException(exception);
            SessionState.SetBool(ActiveKey, false);
            string digestBefore = SessionState.GetString(
                CanonicalDigestBeforeKey,
                string.Empty);
            WriteReports(
                automatedPassed: false,
                failure: exception.ToString(),
                setupBefore: SessionState.GetBool(SetupBeforeKey, false),
                setupAfter: false,
                digestBefore: digestBefore,
                digestAfter: TryComputeCanonicalBoundaryDigest(),
                postflightPending: false);
            ClearSessionState();
            if (exitEditorWhenFinished)
            {
                EditorApplication.Exit(1);
            }
        }

        private static bool HasTimedOut()
        {
            string raw = SessionState.GetString(StartedUtcTicksKey, string.Empty);
            return long.TryParse(
                    raw,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long ticks)
                && (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds
                    > LaunchTimeoutSeconds;
        }

        private static string TryComputeCanonicalBoundaryDigest()
        {
            try
            {
                return OlympusStagePreparationReviewSetup.ComputeCanonicalBoundaryDigest();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void ResetRuntimeFields()
        {
            if (controller != null)
            {
                controller.ReviewConfirmed -= HandleReviewConfirmed;
            }

            Records.Clear();
            controller = null;
            profile = null;
            stageCatalog = null;
            reviewCamera = null;
            reviewCanvas = null;
            reviewCanvasScaler = null;
            graphicRaycaster = null;
            safeAreaRoot = null;
            responsiveRoot = null;
            eventSystem = null;
            stageIntelPanel = null;
            loadoutOverviewPanel = null;
            summonDetailPanel = null;
            reviewConfirmPanel = null;
            intelThreatTagsText = null;
            intelRecommendedSummonRoleText = null;
            detailIconImage = null;
            detailTitleText = null;
            detailRoleText = null;
            detailSelectedTierText = null;
            detailStageRoleText = null;
            detailPlayerUseText = null;
            detailSummonReadText = null;
            confirmDigestText = null;
            intelContinueButton = null;
            loadoutReviewButton = null;
            detailTier1Button = null;
            detailTier2Button = null;
            detailTier3Button = null;
            confirmBackButton = null;
            confirmAcceptButton = null;
            confirmRestartButton = null;
            planIndex = 0;
            readyAtFrame = 0;
            statePrepared = false;
            runtimeInitialized = false;
            observedConfirmationEventCount = 0;
            confirmationEventBaseline = 0;
            observedConfirmedDigest = string.Empty;
            preparedSelectionDigest = string.Empty;
            firstConfirmationAccepted = false;
            duplicateConfirmationAccepted = false;
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(BatchExitKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseString(FailureKey);
            SessionState.EraseString(StartedUtcTicksKey);
            SessionState.EraseBool(SetupBeforeKey);
            SessionState.EraseString(CanonicalDigestBeforeKey);
        }

        private static void HandleReviewConfirmed(string digest)
        {
            observedConfirmationEventCount++;
            observedConfirmedDigest = digest ?? string.Empty;
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
                ReviewCaptureState.StageIntel,
                ReviewCaptureState.LoadoutOverview,
                ReviewCaptureState.Slot1Tier1Detail,
                ReviewCaptureState.Slot2Tier2Detail,
                ReviewCaptureState.Slot3Tier3Detail,
                ReviewCaptureState.ReviewConfirmBefore,
                ReviewCaptureState.ReviewConfirmAfter
            };
            foreach (ReviewCaptureState state in states)
            {
                plans.Add(new CapturePlan(
                    sequence,
                    width,
                    height,
                    state,
                    (sequence - 1) % 2 == 0
                        ? VirtualNotchOrientation.Left
                        : VirtualNotchOrientation.Right));
                sequence++;
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

        private static bool IsDetailState(ReviewCaptureState state)
        {
            return state == ReviewCaptureState.Slot1Tier1Detail
                || state == ReviewCaptureState.Slot2Tier2Detail
                || state == ReviewCaptureState.Slot3Tier3Detail;
        }

        private static int ResolveDetailSlotIndex(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.Slot1Tier1Detail => 0,
                ReviewCaptureState.Slot2Tier2Detail => 1,
                ReviewCaptureState.Slot3Tier3Detail => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private static int ResolveDetailTier(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.Slot1Tier1Detail => 1,
                ReviewCaptureState.Slot2Tier2Detail => 2,
                ReviewCaptureState.Slot3Tier3Detail => 3,
                _ => 0
            };
        }

        private static int[] ResolveExpectedTiers(ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.Slot2Tier2Detail => new[] { 1, 2, 1 },
                ReviewCaptureState.Slot3Tier3Detail => new[] { 1, 1, 3 },
                ReviewCaptureState.ReviewConfirmBefore => new[] { 1, 2, 3 },
                ReviewCaptureState.ReviewConfirmAfter => new[] { 1, 2, 3 },
                _ => new[] { 1, 1, 1 }
            };
        }

        private static string ResolveExpectedSelectedSlotId(ReviewCaptureState state)
        {
            return IsDetailState(state)
                ? ExpectedSlotIds[ResolveDetailSlotIndex(state)]
                : string.Empty;
        }

        private static StagePreparationReviewPhase ResolveExpectedPhase(
            ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.StageIntel => StagePreparationReviewPhase.StageIntel,
                ReviewCaptureState.LoadoutOverview =>
                    StagePreparationReviewPhase.LoadoutOverview,
                ReviewCaptureState.Slot1Tier1Detail =>
                    StagePreparationReviewPhase.SummonDetail,
                ReviewCaptureState.Slot2Tier2Detail =>
                    StagePreparationReviewPhase.SummonDetail,
                ReviewCaptureState.Slot3Tier3Detail =>
                    StagePreparationReviewPhase.SummonDetail,
                ReviewCaptureState.ReviewConfirmBefore =>
                    StagePreparationReviewPhase.ReviewConfirm,
                ReviewCaptureState.ReviewConfirmAfter =>
                    StagePreparationReviewPhase.ReviewConfirm,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private static StagePreparationReviewPanel ResolveExpectedPanel(
            ReviewCaptureState state)
        {
            return state switch
            {
                ReviewCaptureState.StageIntel => StagePreparationReviewPanel.StageIntel,
                ReviewCaptureState.LoadoutOverview =>
                    StagePreparationReviewPanel.LoadoutOverview,
                ReviewCaptureState.Slot1Tier1Detail =>
                    StagePreparationReviewPanel.SummonDetail,
                ReviewCaptureState.Slot2Tier2Detail =>
                    StagePreparationReviewPanel.SummonDetail,
                ReviewCaptureState.Slot3Tier3Detail =>
                    StagePreparationReviewPanel.SummonDetail,
                ReviewCaptureState.ReviewConfirmBefore =>
                    StagePreparationReviewPanel.ReviewConfirm,
                ReviewCaptureState.ReviewConfirmAfter =>
                    StagePreparationReviewPanel.ReviewConfirm,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private static CanvasGroup ResolveCurrentPanelGroup()
        {
            return controller.CurrentPanel switch
            {
                StagePreparationReviewPanel.StageIntel => stageIntelPanel,
                StagePreparationReviewPanel.LoadoutOverview => loadoutOverviewPanel,
                StagePreparationReviewPanel.SummonDetail => summonDetailPanel,
                StagePreparationReviewPanel.ReviewConfirm => reviewConfirmPanel,
                _ => throw new InvalidOperationException(
                    "PREP-01 controller has no active panel group.")
            };
        }

        private static CanvasGroup[] ResolveInactivePanelGroups()
        {
            CanvasGroup current = ResolveCurrentPanelGroup();
            return new[]
            {
                stageIntelPanel,
                loadoutOverviewPanel,
                summonDetailPanel,
                reviewConfirmPanel
            }.Where(panel => panel != current).ToArray();
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

        private static RectTransform FindRectByName(string objectName)
        {
            return FindAllInScene<RectTransform>(SceneManager.GetActiveScene())
                .FirstOrDefault(rect => string.Equals(
                    rect.gameObject.name,
                    objectName,
                    StringComparison.Ordinal));
        }

        private static bool Contains(Rect outer, Rect inner, float tolerance)
        {
            return inner.xMin >= outer.xMin - tolerance
                && inner.xMax <= outer.xMax + tolerance
                && inner.yMin >= outer.yMin - tolerance
                && inner.yMax <= outer.yMax + tolerance;
        }

        private static bool OverlapsWithTolerance(Rect first, Rect second, float tolerance)
        {
            Rect shrunkenFirst = Rect.MinMaxRect(
                first.xMin + tolerance,
                first.yMin + tolerance,
                first.xMax - tolerance,
                first.yMax - tolerance);
            Rect shrunkenSecond = Rect.MinMaxRect(
                second.xMin + tolerance,
                second.yMin + tolerance,
                second.xMax - tolerance,
                second.yMax - tolerance);
            return shrunkenFirst.width > 0f
                && shrunkenFirst.height > 0f
                && shrunkenSecond.width > 0f
                && shrunkenSecond.height > 0f
                && shrunkenFirst.Overlaps(shrunkenSecond);
        }

        private static bool ColorsApproximatelyEqual(Color first, Color second)
        {
            const float tolerance = 0.001f;
            return Mathf.Abs(first.r - second.r) <= tolerance
                && Mathf.Abs(first.g - second.g) <= tolerance
                && Mathf.Abs(first.b - second.b) <= tolerance
                && Mathf.Abs(first.a - second.a) <= tolerance;
        }

        private static Rect CalculateRenderedTextScreenRect(TMP_Text text)
        {
            text.ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
            Bounds bounds = text.textBounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3[] worldCorners =
            {
                text.transform.TransformPoint(new Vector3(min.x, min.y, 0f)),
                text.transform.TransformPoint(new Vector3(min.x, max.y, 0f)),
                text.transform.TransformPoint(new Vector3(max.x, max.y, 0f)),
                text.transform.TransformPoint(new Vector3(max.x, min.y, 0f))
            };
            return CalculateScreenRect(worldCorners, text.canvas);
        }

        private static Rect CalculateScreenRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return CalculateScreenRect(corners, rectTransform.GetComponentInParent<Canvas>());
        }

        private static Rect CalculateScreenRect(Vector3[] worldCorners, Canvas canvas)
        {
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector2[] screenCorners = worldCorners
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
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property?.objectReferenceValue is T value)
            {
                return value;
            }

            throw new InvalidOperationException(
                $"PREP-01 controller field `{propertyName}` is missing or wrong-type.");
        }

        private static T FindSingleInScene<T>(Scene scene) where T : Component
        {
            T[] matches = FindAllInScene<T>(scene);
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one {typeof(T).Name} in `{scene.path}`; "
                    + $"found {matches.Length}.");
            }

            return matches[0];
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
                .ToArray();
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
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
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
                int visibleGraphicCount,
                int visibleTextCount,
                int evaluatedTextOverlapPairCount,
                int usableTargetCount,
                bool safeAreaContainmentValidated,
                bool visibleTextFitValidated,
                bool textOverlapValidated,
                bool raycastValidated,
                bool minimumTargetValidated)
            {
                VisibleGraphicCount = visibleGraphicCount;
                VisibleTextCount = visibleTextCount;
                EvaluatedTextOverlapPairCount = evaluatedTextOverlapPairCount;
                UsableTargetCount = usableTargetCount;
                SafeAreaContainmentValidated = safeAreaContainmentValidated;
                VisibleTextFitValidated = visibleTextFitValidated;
                TextOverlapValidated = textOverlapValidated;
                RaycastValidated = raycastValidated;
                MinimumTargetValidated = minimumTargetValidated;
            }

            public int VisibleGraphicCount { get; }
            public int VisibleTextCount { get; }
            public int EvaluatedTextOverlapPairCount { get; }
            public int UsableTargetCount { get; }
            public bool SafeAreaContainmentValidated { get; }
            public bool VisibleTextFitValidated { get; }
            public bool TextOverlapValidated { get; }
            public bool RaycastValidated { get; }
            public bool MinimumTargetValidated { get; }
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
            public string SelectedSlotId;
            public int SelectedTier;
            public string ActionIds;
            public string TierSnapshot;
            public string CurrentSelectionDigest;
            public string PreparedSelectionDigest;
            public string LastConfirmedSelectionDigest;
            public string ProjectionCatalogEntryId;
            public string ProjectionDigest;
            public string ProjectionBriefingDigest;
            public bool ProjectionCurrent;
            public string ProjectionRejectReason;
            public bool NeutralRecommendationBoundaryValidated;
            public string NeutralThreatNotation;
            public string NeutralRuntimePresetNotation;
            public bool LegacyProjectionCopyAbsent;
            public bool ForbiddenProgressionCopyAbsent;
            public bool TierReadoutValidated;
            public bool SessionLocalTierValidated;
            public bool ReviewAccepted;
            public bool ConfirmationFirstAccepted;
            public bool ConfirmationDuplicateAccepted;
            public int ConfirmationDispatchCount;
            public int ConfirmationEventDelta;
            public string ObservedConfirmedDigest;
            public string FocusTarget;
            public bool ActivePanelValidated;
            public bool RaycastValidated;
            public bool MinimumTargetValidated;
            public bool SafeAreaValidated;
            public bool TextFitValidated;
            public bool TextOverlapValidated;
            public int VisibleGraphicCount;
            public int VisibleTextCount;
            public int EvaluatedTextOverlapPairCount;
            public int UsableTargetCount;
            public bool StageRunActive;
            public string CanonicalBoundaryDigest;
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
            public bool PostflightPending;
            public string Failure;
            public int ExpectedCaptureCount;
            public bool SetupVerificationBefore;
            public bool SetupVerificationAfter;
            public string CanonicalDigestBefore;
            public string CanonicalDigestAfter;
            public bool CanonicalDigestStable;
            public string NavigationBoundary;
            public string CanonicalHashBoundary;
            public CaptureRecord[] Captures;
        }
    }
}
