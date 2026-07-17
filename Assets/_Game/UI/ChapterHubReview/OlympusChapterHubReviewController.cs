using System;
using System.Text;
using DimensionBrawl.LevelDesign;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI.ChapterHubReview
{
    public enum ChapterHubReviewPanel
    {
        None = 0,
        ChapterHub = 10,
        StageMap = 20,
        StageDetail = 30,
        ReviewConfirm = 40
    }

    [DisallowMultipleComponent]
    public sealed class OlympusChapterHubReviewController : MonoBehaviour
    {
        public const string PlannedReviewStatus = "REVIEW SAMPLE / IN PRODUCTION";
        public const string AnnouncedReviewStatus = "REVIEW SAMPLE / ANNOUNCED";
        public const string CanonicalReviewStatus = "REVIEW SAMPLE / CANONICAL DETAIL";
        public const string ConfirmedReviewStatus = "REVIEW CONFIRMED / NO ROUTE DISPATCH";

        [Serializable]
        public sealed class ReviewConfirmedEvent : UnityEvent<string>
        {
        }

        [Serializable]
        public sealed class ChapterButtonBinding
        {
            [SerializeField] private string chapterId = string.Empty;
            [SerializeField] private Button button;
            [SerializeField] private CanvasGroup canvasGroup;
            [SerializeField] private TMP_Text episodeCodeText;
            [SerializeField] private TMP_Text titleText;

            public ChapterButtonBinding()
            {
            }

            public ChapterButtonBinding(
                string chapterId,
                Button button,
                CanvasGroup canvasGroup = null,
                TMP_Text episodeCodeText = null,
                TMP_Text titleText = null)
            {
                Configure(chapterId, button, canvasGroup, episodeCodeText, titleText);
            }

            public string ChapterId => chapterId;
            public Button Button => button;
            public CanvasGroup CanvasGroup => canvasGroup;
            public TMP_Text EpisodeCodeText => episodeCodeText;
            public TMP_Text TitleText => titleText;

            public void Configure(
                string newChapterId,
                Button newButton,
                CanvasGroup newCanvasGroup = null,
                TMP_Text newEpisodeCodeText = null,
                TMP_Text newTitleText = null)
            {
                chapterId = newChapterId ?? string.Empty;
                button = newButton;
                canvasGroup = newCanvasGroup;
                episodeCodeText = newEpisodeCodeText;
                titleText = newTitleText;
            }
        }

        [Serializable]
        public sealed class StageNodeBinding
        {
            [SerializeField] private string stageId = string.Empty;
            [SerializeField] private Button button;
            [SerializeField] private CanvasGroup canvasGroup;
            [SerializeField] private RectTransform mapAnchor;
            [SerializeField] private TMP_Text stageCodeText;
            [SerializeField] private TMP_Text titleText;
            [SerializeField] private TMP_Text statusText;

            public StageNodeBinding()
            {
            }

            public StageNodeBinding(
                string stageId,
                Button button,
                CanvasGroup canvasGroup = null,
                RectTransform mapAnchor = null,
                TMP_Text stageCodeText = null,
                TMP_Text titleText = null,
                TMP_Text statusText = null)
            {
                Configure(
                    stageId,
                    button,
                    canvasGroup,
                    mapAnchor,
                    stageCodeText,
                    titleText,
                    statusText);
            }

            public string StageId => stageId;
            public Button Button => button;
            public CanvasGroup CanvasGroup => canvasGroup;
            public RectTransform MapAnchor => mapAnchor;
            public TMP_Text StageCodeText => stageCodeText;
            public TMP_Text TitleText => titleText;
            public TMP_Text StatusText => statusText;

            public void Configure(
                string newStageId,
                Button newButton,
                CanvasGroup newCanvasGroup = null,
                RectTransform newMapAnchor = null,
                TMP_Text newStageCodeText = null,
                TMP_Text newTitleText = null,
                TMP_Text newStatusText = null)
            {
                stageId = newStageId ?? string.Empty;
                button = newButton;
                canvasGroup = newCanvasGroup;
                mapAnchor = newMapAnchor;
                stageCodeText = newStageCodeText;
                titleText = newTitleText;
                statusText = newStatusText;
            }
        }

        [Header("Local Review Model")]
        [SerializeField] private ChapterHubReviewProfile profile;
        [SerializeField] private UIStageCatalog stageCatalog;

        [Header("Authored Panels")]
        [SerializeField] private CanvasGroup chapterHubPanel;
        [SerializeField] private CanvasGroup stageMapPanel;
        [SerializeField] private CanvasGroup stageDetailPanel;
        [SerializeField] private CanvasGroup reviewConfirmPanel;

        [Header("Chapter Hub")]
        [SerializeField] private TMP_Text hubEpisodeCodeText;
        [SerializeField] private TMP_Text hubTitleText;
        [SerializeField] private TMP_Text hubStatusText;
        [SerializeField] private ChapterButtonBinding[] chapterBindings =
            Array.Empty<ChapterButtonBinding>();

        [Header("Stage Map")]
        [SerializeField] private TMP_Text mapEpisodeCodeText;
        [SerializeField] private TMP_Text mapChapterTitleText;
        [SerializeField] private TMP_Text mapStatusText;
        [SerializeField] private Button mapBackButton;
        [SerializeField] private StageNodeBinding[] stageNodeBindings =
            Array.Empty<StageNodeBinding>();

        [Header("Stage Detail - Verified Fields")]
        [SerializeField] private TMP_Text detailStageCodeText;
        [SerializeField] private TMP_Text detailTitleText;
        [SerializeField] private TMP_Text detailStatusText;
        [SerializeField] private TMP_Text detailAvailabilityText;
        [SerializeField] private GameObject detailObjectiveRow;
        [SerializeField] private TMP_Text detailObjectiveText;
        [SerializeField] private GameObject detailCombatLessonRow;
        [SerializeField] private TMP_Text detailCombatLessonText;
        [SerializeField] private GameObject detailStoryRow;
        [SerializeField] private TMP_Text detailStoryText;
        [SerializeField] private GameObject detailSegmentRow;
        [SerializeField] private TMP_Text detailSegmentText;

        [Header("Stage Detail - Unverified Fields (Hidden)")]
        [SerializeField] private GameObject detailRecommendedPowerRow;
        [SerializeField] private GameObject detailLoadoutRow;
        [SerializeField] private GameObject detailDurationRow;
        [SerializeField] private GameObject detailThreatRow;
        [SerializeField] private GameObject detailSummonRow;
        [SerializeField] private GameObject detailRewardRow;

        [Header("Stage Detail Navigation")]
        [SerializeField] private Button detailBackButton;
        [SerializeField] private Button detailReviewButton;

        [Header("Review Confirmation")]
        [SerializeField] private TMP_Text confirmTitleText;
        [SerializeField] private TMP_Text confirmSummaryText;
        [SerializeField] private TMP_Text confirmStatusText;
        [SerializeField] private Button confirmBackButton;
        [SerializeField] private Button confirmAcceptButton;
        [SerializeField] private ReviewConfirmedEvent reviewConfirmedEvent = new();

        private ChapterHubReviewSession session;
        private UIStageRouteProjection currentProjection;
        private UIStageRouteProjectionRejectReason lastProjectionRejectReason;
        private UnityAction[] chapterButtonActions = Array.Empty<UnityAction>();
        private UnityAction[] stageButtonActions = Array.Empty<UnityAction>();
        private bool interactionsBound;
        private int confirmationDispatchCount;
        private int projectionRefreshCount;
        private string lastConfirmedCatalogEntryId = string.Empty;

        public event Action<ChapterHubReviewPanel> PanelChanged;
        public event Action<string> ReviewConfirmed;

        public ChapterHubReviewSession Session => session;
        public ChapterHubReviewPhase CurrentPhase =>
            session != null ? session.Phase : ChapterHubReviewPhase.Overview;
        public ChapterHubReviewPanel CurrentPanel { get; private set; }
        public UIStageRouteProjection CurrentProjection => currentProjection;
        public UIStageRouteProjection StageProjection => currentProjection;
        public UIStageRouteProjectionRejectReason LastProjectionRejectReason =>
            lastProjectionRejectReason;
        public int ConfirmationDispatchCount => confirmationDispatchCount;
        public int ProjectionRefreshCount => projectionRefreshCount;
        public string LastConfirmedCatalogEntryId => lastConfirmedCatalogEntryId;
        public ReviewConfirmedEvent ConfirmationEvent => reviewConfirmedEvent;
        public bool HasCurrentCanonicalDetail => currentProjection != null;
        public bool IsConfirmationAvailable =>
            session != null
            && !session.IsConfirmationAccepted
            && session.SelectedStage != null
            && session.SelectedStage.IsCanonicalPlayable
            && currentProjection != null;

        public void ConfigureCore(
            ChapterHubReviewProfile newProfile,
            UIStageCatalog newStageCatalog)
        {
            bool rebind = BeginRuntimeReconfiguration();
            profile = newProfile;
            stageCatalog = newStageCatalog;
            BeginReview();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigurePanels(
            CanvasGroup newChapterHubPanel,
            CanvasGroup newStageMapPanel,
            CanvasGroup newStageDetailPanel,
            CanvasGroup newReviewConfirmPanel)
        {
            chapterHubPanel = newChapterHubPanel;
            stageMapPanel = newStageMapPanel;
            stageDetailPanel = newStageDetailPanel;
            reviewConfirmPanel = newReviewConfirmPanel;
            ApplyCurrentView();
        }

        public void ConfigureChapterView(
            TMP_Text episodeCode,
            TMP_Text title,
            TMP_Text status,
            ChapterButtonBinding[] bindings)
        {
            bool rebind = BeginRuntimeReconfiguration();
            hubEpisodeCodeText = episodeCode;
            hubTitleText = title;
            hubStatusText = status;
            chapterBindings = bindings ?? Array.Empty<ChapterButtonBinding>();
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureStageMapView(
            TMP_Text episodeCode,
            TMP_Text chapterTitle,
            TMP_Text status,
            Button backButton,
            StageNodeBinding[] bindings)
        {
            bool rebind = BeginRuntimeReconfiguration();
            mapEpisodeCodeText = episodeCode;
            mapChapterTitleText = chapterTitle;
            mapStatusText = status;
            mapBackButton = backButton;
            stageNodeBindings = bindings ?? Array.Empty<StageNodeBinding>();
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureStageDetailView(
            TMP_Text stageCode,
            TMP_Text title,
            TMP_Text status,
            GameObject objectiveRow,
            TMP_Text objective,
            GameObject combatLessonRow,
            TMP_Text combatLesson,
            GameObject storyRow,
            TMP_Text story,
            GameObject segmentRow,
            TMP_Text segment,
            Button backButton,
            Button reviewButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            detailStageCodeText = stageCode;
            detailTitleText = title;
            detailStatusText = status;
            detailObjectiveRow = objectiveRow;
            detailObjectiveText = objective;
            detailCombatLessonRow = combatLessonRow;
            detailCombatLessonText = combatLesson;
            detailStoryRow = storyRow;
            detailStoryText = story;
            detailSegmentRow = segmentRow;
            detailSegmentText = segment;
            detailBackButton = backButton;
            detailReviewButton = reviewButton;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureUnverifiedDetailRows(
            GameObject recommendedPowerRow,
            GameObject loadoutRow,
            GameObject durationRow,
            GameObject threatRow,
            GameObject summonRow,
            GameObject rewardRow)
        {
            detailRecommendedPowerRow = recommendedPowerRow;
            detailLoadoutRow = loadoutRow;
            detailDurationRow = durationRow;
            detailThreatRow = threatRow;
            detailSummonRow = summonRow;
            detailRewardRow = rewardRow;
            HideUnverifiedDetailRows();
        }

        public void ConfigureAvailabilityText(TMP_Text availabilityText)
        {
            detailAvailabilityText = availabilityText;
            ApplyCurrentView();
        }

        public void ConfigureConfirmationView(
            TMP_Text title,
            TMP_Text summary,
            TMP_Text status,
            Button backButton,
            Button acceptButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            confirmTitleText = title;
            confirmSummaryText = summary;
            confirmStatusText = status;
            confirmBackButton = backButton;
            confirmAcceptButton = acceptButton;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BindInteractions();
            if (session == null)
            {
                BeginReview();
            }
            else
            {
                ApplyCurrentView();
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UnbindInteractions();
        }

        public bool BeginReview()
        {
            session = null;
            currentProjection = null;
            lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;
            confirmationDispatchCount = 0;
            projectionRefreshCount = 0;
            lastConfirmedCatalogEntryId = string.Empty;

            if (profile != null && profile.TryValidate(out _))
            {
                try
                {
                    session = new ChapterHubReviewSession(profile);
                }
                catch (ArgumentException)
                {
                    session = null;
                }
            }

            ApplyCurrentView();
            return session != null;
        }

        public bool RestartReview()
        {
            return BeginReview();
        }

        public bool OpenChapterMap(string chapterId)
        {
            return SelectChapter(chapterId);
        }

        public bool SelectChapter(string chapterId)
        {
            if (session == null || !session.TrySelectChapter(chapterId))
            {
                return false;
            }

            currentProjection = null;
            lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;
            ApplyCurrentView();
            return true;
        }

        public bool OpenStageDetail(string stageId)
        {
            return SelectStage(stageId);
        }

        public bool SelectStage(string stageId)
        {
            if (session == null || !session.TrySelectStage(stageId))
            {
                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool OpenReviewConfirm()
        {
            if (session == null
                || session.Phase != ChapterHubReviewPhase.StageDetail
                || !TryRefreshSelectedCanonicalProjection()
                || !session.TryOpenReviewConfirm())
            {
                if (session != null
                    && session.Phase == ChapterHubReviewPhase.StageDetail)
                {
                    RenderStageDetail();
                }
                else
                {
                    ApplyDetailReviewButtonState();
                }

                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool ConfirmSelectedStage()
        {
            if (session == null
                || session.Phase != ChapterHubReviewPhase.ReviewConfirm
                || !TryRefreshSelectedCanonicalProjection()
                || !session.TryConfirmSelectedStage(out string canonicalCatalogEntryId))
            {
                if (session != null
                    && session.Phase == ChapterHubReviewPhase.ReviewConfirm)
                {
                    RenderConfirmation();
                }
                else
                {
                    ApplyConfirmationButtonState();
                }

                return false;
            }

            confirmationDispatchCount++;
            lastConfirmedCatalogEntryId = canonicalCatalogEntryId;
            reviewConfirmedEvent?.Invoke(canonicalCatalogEntryId);
            ReviewConfirmed?.Invoke(canonicalCatalogEntryId);
            RenderConfirmation();
            SelectDefaultControl(ChapterHubReviewPanel.ReviewConfirm);
            return true;
        }

        public bool NavigateBack()
        {
            if (session != null
                && session.IsConfirmationAccepted
                && session.Phase == ChapterHubReviewPhase.ReviewConfirm)
            {
                return RestartReview();
            }

            if (session == null || !session.TryBack())
            {
                return false;
            }

            if (session.Phase == ChapterHubReviewPhase.StageMap
                || session.Phase == ChapterHubReviewPhase.Overview)
            {
                currentProjection = null;
                lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;
            }

            ApplyCurrentView();
            return true;
        }

        public bool Back()
        {
            return NavigateBack();
        }

        public bool CloseReviewConfirm()
        {
            return NavigateBack();
        }

        public void RefreshCurrentView()
        {
            ApplyCurrentView();
        }

        private void BindInteractions()
        {
            if (interactionsBound)
            {
                return;
            }

            chapterButtonActions = new UnityAction[chapterBindings?.Length ?? 0];
            for (int i = 0; i < chapterButtonActions.Length; i++)
            {
                ChapterButtonBinding binding = chapterBindings[i];
                if (binding == null || binding.Button == null)
                {
                    continue;
                }

                string chapterId = binding.ChapterId;
                UnityAction action = () => SelectChapter(chapterId);
                chapterButtonActions[i] = action;
                binding.Button.onClick.AddListener(action);
            }

            stageButtonActions = new UnityAction[stageNodeBindings?.Length ?? 0];
            for (int i = 0; i < stageButtonActions.Length; i++)
            {
                StageNodeBinding binding = stageNodeBindings[i];
                if (binding == null || binding.Button == null)
                {
                    continue;
                }

                string stageId = binding.StageId;
                UnityAction action = () => SelectStage(stageId);
                stageButtonActions[i] = action;
                binding.Button.onClick.AddListener(action);
            }

            AddButtonListener(mapBackButton, HandleBackClicked);
            AddButtonListener(detailBackButton, HandleBackClicked);
            AddButtonListener(detailReviewButton, HandleOpenReviewConfirmClicked);
            AddButtonListener(confirmBackButton, HandleBackClicked);
            AddButtonListener(confirmAcceptButton, HandleConfirmSelectedStageClicked);
            interactionsBound = true;
        }

        private void UnbindInteractions()
        {
            if (!interactionsBound)
            {
                return;
            }

            int chapterCount = Math.Min(
                chapterBindings?.Length ?? 0,
                chapterButtonActions?.Length ?? 0);
            for (int i = 0; i < chapterCount; i++)
            {
                ChapterButtonBinding binding = chapterBindings[i];
                UnityAction action = chapterButtonActions[i];
                if (binding?.Button != null && action != null)
                {
                    binding.Button.onClick.RemoveListener(action);
                }
            }

            int stageCount = Math.Min(
                stageNodeBindings?.Length ?? 0,
                stageButtonActions?.Length ?? 0);
            for (int i = 0; i < stageCount; i++)
            {
                StageNodeBinding binding = stageNodeBindings[i];
                UnityAction action = stageButtonActions[i];
                if (binding?.Button != null && action != null)
                {
                    binding.Button.onClick.RemoveListener(action);
                }
            }

            RemoveButtonListener(mapBackButton, HandleBackClicked);
            RemoveButtonListener(detailBackButton, HandleBackClicked);
            RemoveButtonListener(detailReviewButton, HandleOpenReviewConfirmClicked);
            RemoveButtonListener(confirmBackButton, HandleBackClicked);
            RemoveButtonListener(confirmAcceptButton, HandleConfirmSelectedStageClicked);
            chapterButtonActions = Array.Empty<UnityAction>();
            stageButtonActions = Array.Empty<UnityAction>();
            interactionsBound = false;
        }

        private bool BeginRuntimeReconfiguration()
        {
            bool rebind = interactionsBound;
            if (rebind)
            {
                UnbindInteractions();
            }

            return rebind;
        }

        private void HandleBackClicked()
        {
            NavigateBack();
        }

        private void HandleOpenReviewConfirmClicked()
        {
            OpenReviewConfirm();
        }

        private void HandleConfirmSelectedStageClicked()
        {
            ConfirmSelectedStage();
        }

        private void EndRuntimeReconfiguration(bool rebind)
        {
            if (rebind && Application.isPlaying && isActiveAndEnabled)
            {
                BindInteractions();
            }
        }

        private void ApplyCurrentView()
        {
            RefreshChapterBindings();
            RefreshStageNodeBindings();

            if (session == null)
            {
                RenderUnavailableOverview();
                ShowOnly(ChapterHubReviewPanel.ChapterHub);
                return;
            }

            switch (session.Phase)
            {
                case ChapterHubReviewPhase.StageMap:
                    RenderStageMap();
                    ShowOnly(ChapterHubReviewPanel.StageMap);
                    break;
                case ChapterHubReviewPhase.StageDetail:
                    RenderStageDetail();
                    ShowOnly(ChapterHubReviewPanel.StageDetail);
                    break;
                case ChapterHubReviewPhase.ReviewConfirm:
                    RenderConfirmation();
                    ShowOnly(ChapterHubReviewPanel.ReviewConfirm);
                    break;
                default:
                    RenderOverview();
                    ShowOnly(ChapterHubReviewPanel.ChapterHub);
                    break;
            }
        }

        private void RenderUnavailableOverview()
        {
            SetText(hubEpisodeCodeText, string.Empty);
            SetText(hubTitleText, "CHAPTER REVIEW UNAVAILABLE");
            SetText(hubStatusText, "REVIEW PROFILE INVALID OR MISSING");
        }

        private void RenderOverview()
        {
            ChapterHubReviewProfile.ChapterDefinition chapter =
                session.ChapterCount > 0 ? session.GetChapter(0) : null;
            SetText(hubEpisodeCodeText, chapter?.EpisodeCode ?? string.Empty);
            SetText(hubTitleText, chapter?.TitleFallback ?? "CHAPTER REVIEW");
            SetText(hubStatusText, "REVIEW SAMPLE / LOCAL BROWSE");
        }

        private void RenderStageMap()
        {
            ChapterHubReviewProfile.ChapterDefinition chapter = session.SelectedChapter;
            SetText(mapEpisodeCodeText, chapter?.EpisodeCode ?? string.Empty);
            SetText(mapChapterTitleText, chapter?.TitleFallback ?? string.Empty);
            SetText(mapStatusText, "SELECT AN AUTHORED REVIEW NODE");
            SetButtonInteractable(mapBackButton, !session.IsConfirmationAccepted);
            RefreshStageNodeBindings();
        }

        private void RenderStageDetail()
        {
            HideVerifiedDetailRows();
            HideUnverifiedDetailRows();
            SetTextVisible(detailAvailabilityText, string.Empty, false);

            ChapterHubReviewProfile.StageDefinition selectedStage = session.SelectedStage;
            if (selectedStage == null)
            {
                currentProjection = null;
                SetText(detailStageCodeText, string.Empty);
                SetTextVisible(detailTitleText, string.Empty, false);
                SetText(detailStatusText, "REVIEW STAGE UNAVAILABLE");
                SetTextVisible(
                    detailAvailabilityText,
                    "선택한 리뷰 슬롯을 확인할 수 없습니다.",
                    true);
                ApplyDetailReviewButtonState();
                return;
            }

            SetText(detailStageCodeText, selectedStage.StageCode);
            SetButtonInteractable(detailBackButton, !session.IsConfirmationAccepted);

            if (!selectedStage.IsCanonicalPlayable)
            {
                currentProjection = null;
                lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;
                SetTextVisible(detailTitleText, selectedStage.TitleFallback, true);
                SetText(
                    detailStatusText,
                    ResolveContentStatusLabel(selectedStage.ContentStatus));
                SetTextVisible(
                    detailAvailabilityText,
                    ResolveAvailabilityCopy(selectedStage.ContentStatus),
                    true);
                ApplyDetailReviewButtonState();
                return;
            }

            if (!TryRefreshSelectedCanonicalProjection())
            {
                SetTextVisible(detailTitleText, string.Empty, false);
                SetText(
                    detailStatusText,
                    $"CANONICAL DETAIL UNAVAILABLE / {lastProjectionRejectReason}");
                SetTextVisible(
                    detailAvailabilityText,
                    "정식 카탈로그 투영을 확인할 수 없습니다.\n"
                    + "표시 가능한 검증 데이터가 없어 상세 정보를 닫았습니다.",
                    true);
                ApplyDetailReviewButtonState();
                return;
            }

            StageBriefingReadModel briefing = currentProjection.Briefing;
            bool titlePresent = IsPresent(briefing.TitleDisposition, briefing.Title);
            SetTextVisible(detailTitleText, briefing.Title, titlePresent);
            ApplyVerifiedDetailRow(
                detailObjectiveRow,
                detailObjectiveText,
                briefing.ObjectiveDisposition,
                briefing.Objective);
            ApplyVerifiedDetailRow(
                detailCombatLessonRow,
                detailCombatLessonText,
                briefing.CombatLessonDisposition,
                briefing.CombatLesson);

            bool storyPresent = briefing.StoryEntryDisposition
                == StageReferenceDisposition.Present;
            string storyText = storyPresent ? BuildStoryText(briefing) : string.Empty;
            SetDetailRow(
                detailStoryRow,
                detailStoryText,
                storyPresent && !string.IsNullOrWhiteSpace(storyText),
                storyText);

            string segmentText = BuildSegmentText(briefing);
            SetDetailRow(
                detailSegmentRow,
                detailSegmentText,
                briefing.SegmentCount > 0 && !string.IsNullOrWhiteSpace(segmentText),
                segmentText);
            SetText(detailStatusText, CanonicalReviewStatus);
            ApplyDetailReviewButtonState();
        }

        private void RenderConfirmation()
        {
            HideUnverifiedDetailRows();
            bool hasCurrentProjection = TryRefreshSelectedCanonicalProjection();
            StageBriefingReadModel briefing = hasCurrentProjection
                ? currentProjection.Briefing
                : null;
            bool titlePresent = briefing != null
                && IsPresent(briefing.TitleDisposition, briefing.Title);
            bool objectivePresent = briefing != null
                && IsPresent(briefing.ObjectiveDisposition, briefing.Objective);

            SetTextVisible(confirmTitleText, briefing?.Title, titlePresent);
            SetTextVisible(confirmSummaryText, briefing?.Objective, objectivePresent);
            SetText(
                confirmStatusText,
                session != null && session.IsConfirmationAccepted
                    ? ConfirmedReviewStatus
                    : hasCurrentProjection
                        ? "REVIEW SAMPLE / CONFIRMATION ONLY"
                        : $"CANONICAL DETAIL UNAVAILABLE / {lastProjectionRejectReason}");
            ApplyConfirmationButtonState();
        }

        private bool TryRefreshSelectedCanonicalProjection()
        {
            projectionRefreshCount++;
            currentProjection = null;
            lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;

            ChapterHubReviewProfile.StageDefinition selectedStage = session?.SelectedStage;
            if (selectedStage == null
                || !selectedStage.IsCanonicalPlayable
                || string.IsNullOrWhiteSpace(selectedStage.CanonicalCatalogEntryId))
            {
                return false;
            }

            if (stageCatalog == null)
            {
                lastProjectionRejectReason =
                    UIStageRouteProjectionRejectReason.CatalogEntryNotFound;
                return false;
            }

            if (!stageCatalog.TryCreateRouteProjection(
                    selectedStage.CanonicalCatalogEntryId,
                    UIRouteId.Combat,
                    out UIStageRouteProjection projection,
                    out UIStageRouteProjectionRejectReason rejectReason))
            {
                lastProjectionRejectReason = rejectReason;
                return false;
            }

            if (!stageCatalog.IsProjectionCurrent(
                    projection,
                    UIRouteId.Combat,
                    out rejectReason))
            {
                lastProjectionRejectReason = rejectReason;
                return false;
            }

            currentProjection = projection;
            lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;
            return true;
        }

        private void ApplyDetailReviewButtonState()
        {
            bool available = session != null
                && session.Phase == ChapterHubReviewPhase.StageDetail
                && !session.IsConfirmationAccepted
                && session.SelectedStage != null
                && session.SelectedStage.IsCanonicalPlayable
                && currentProjection != null;
            if (detailReviewButton != null)
            {
                SetActive(detailReviewButton.gameObject, available);
            }

            SetButtonInteractable(detailReviewButton, available);
        }

        private void ApplyConfirmationButtonState()
        {
            bool accepted = session != null && session.IsConfirmationAccepted;
            SetButtonLabel(
                confirmAcceptButton,
                accepted ? "REVIEW ACKNOWLEDGED" : "ACKNOWLEDGE REVIEW");
            SetButtonInteractable(
                confirmAcceptButton,
                session != null
                && session.Phase == ChapterHubReviewPhase.ReviewConfirm
                && !accepted
                && currentProjection != null);
            SetButtonInteractable(confirmBackButton, true);
        }

        private void RefreshChapterBindings()
        {
            ChapterButtonBinding[] bindings = chapterBindings
                ?? Array.Empty<ChapterButtonBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                ChapterButtonBinding binding = bindings[i];
                if (binding == null)
                {
                    continue;
                }

                ChapterHubReviewProfile.ChapterDefinition chapter = null;
                bool found = session != null
                    && session.TryGetChapter(
                        binding.ChapterId,
                        out chapter);
                SetCanvasGroup(binding.CanvasGroup, found, found);
                SetButtonInteractable(
                    binding.Button,
                    found
                    && session.Phase == ChapterHubReviewPhase.Overview
                    && !session.IsConfirmationAccepted);
                SetText(binding.EpisodeCodeText, found ? chapter.EpisodeCode : string.Empty);
                SetText(binding.TitleText, found ? chapter.TitleFallback : string.Empty);
            }
        }

        private void RefreshStageNodeBindings()
        {
            StageNodeBinding[] bindings = stageNodeBindings ?? Array.Empty<StageNodeBinding>();
            string chapterId = session?.SelectedChapterId ?? string.Empty;
            for (int i = 0; i < bindings.Length; i++)
            {
                StageNodeBinding binding = bindings[i];
                if (binding == null)
                {
                    continue;
                }

                ChapterHubReviewProfile.StageDefinition stage = null;
                bool found = session != null
                    && session.TryGetStage(
                        binding.StageId,
                        out stage)
                    && string.Equals(stage.ChapterId, chapterId, StringComparison.Ordinal);
                string resolvedTitle = string.Empty;
                bool presentationAvailable = found
                    && TryResolveStageNodeTitle(stage, out resolvedTitle);
                bool interactive = found
                    && presentationAvailable
                    && session.Phase == ChapterHubReviewPhase.StageMap
                    && !session.IsConfirmationAccepted;
                SetCanvasGroup(binding.CanvasGroup, found, interactive);
                SetButtonInteractable(binding.Button, interactive);
                SetText(binding.StageCodeText, found ? stage.StageCode : string.Empty);
                SetText(
                    binding.TitleText,
                    presentationAvailable ? resolvedTitle : string.Empty);
                SetText(
                    binding.StatusText,
                    !found
                        ? string.Empty
                        : presentationAvailable
                            ? ResolveContentStatusLabel(stage.ContentStatus)
                            : "CANONICAL DATA UNAVAILABLE");

                if (found && binding.MapAnchor != null)
                {
                    Vector2 position = stage.NormalizedMapPosition;
                    binding.MapAnchor.anchorMin = position;
                    binding.MapAnchor.anchorMax = position;
                    binding.MapAnchor.anchoredPosition = Vector2.zero;
                }
            }
        }

        private bool TryResolveStageNodeTitle(
            ChapterHubReviewProfile.StageDefinition stage,
            out string title)
        {
            title = string.Empty;
            if (stage == null)
            {
                return false;
            }

            if (!stage.IsCanonicalPlayable)
            {
                title = stage.TitleFallback;
                return !string.IsNullOrWhiteSpace(title);
            }

            if (stageCatalog == null
                || string.IsNullOrWhiteSpace(stage.CanonicalCatalogEntryId)
                || !stageCatalog.TryCreateRouteProjection(
                    stage.CanonicalCatalogEntryId,
                    UIRouteId.Combat,
                    out UIStageRouteProjection projection,
                    out UIStageRouteProjectionRejectReason rejectReason)
                || !stageCatalog.IsProjectionCurrent(
                    projection,
                    UIRouteId.Combat,
                    out rejectReason))
            {
                return false;
            }

            StageBriefingReadModel briefing = projection.Briefing;
            if (briefing == null
                || !IsPresent(briefing.TitleDisposition, briefing.Title))
            {
                return false;
            }

            title = briefing.Title;
            return true;
        }

        private void ShowOnly(ChapterHubReviewPanel panel)
        {
            SetCanvasGroup(
                chapterHubPanel,
                panel == ChapterHubReviewPanel.ChapterHub,
                panel == ChapterHubReviewPanel.ChapterHub);
            SetCanvasGroup(
                stageMapPanel,
                panel == ChapterHubReviewPanel.StageMap,
                panel == ChapterHubReviewPanel.StageMap);
            SetCanvasGroup(
                stageDetailPanel,
                panel == ChapterHubReviewPanel.StageDetail,
                panel == ChapterHubReviewPanel.StageDetail);
            SetCanvasGroup(
                reviewConfirmPanel,
                panel == ChapterHubReviewPanel.ReviewConfirm,
                panel == ChapterHubReviewPanel.ReviewConfirm);

            if (CurrentPanel != panel)
            {
                CurrentPanel = panel;
                PanelChanged?.Invoke(panel);
            }

            SelectDefaultControl(panel);
        }

        private void SelectDefaultControl(ChapterHubReviewPanel panel)
        {
            if (!Application.isPlaying || EventSystem.current == null)
            {
                return;
            }

            Button target = panel switch
            {
                ChapterHubReviewPanel.ChapterHub => FindFirstInteractableChapterButton(),
                ChapterHubReviewPanel.StageMap =>
                    FindFirstInteractableStageButton() ?? mapBackButton,
                ChapterHubReviewPanel.StageDetail =>
                    IsUsableButton(detailReviewButton) ? detailReviewButton : detailBackButton,
                ChapterHubReviewPanel.ReviewConfirm =>
                    IsUsableButton(confirmAcceptButton) ? confirmAcceptButton : confirmBackButton,
                _ => null
            };
            if (IsUsableButton(target)
                && EventSystem.current.currentSelectedGameObject != target.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(target.gameObject);
            }
        }

        private Button FindFirstInteractableChapterButton()
        {
            ChapterButtonBinding[] bindings = chapterBindings
                ?? Array.Empty<ChapterButtonBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                Button button = bindings[i]?.Button;
                if (IsUsableButton(button))
                {
                    return button;
                }
            }

            return null;
        }

        private Button FindFirstInteractableStageButton()
        {
            StageNodeBinding[] bindings = stageNodeBindings
                ?? Array.Empty<StageNodeBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                Button button = bindings[i]?.Button;
                if (IsUsableButton(button))
                {
                    return button;
                }
            }

            return null;
        }

        private static bool IsUsableButton(Button button)
        {
            return button != null
                && button.gameObject.activeInHierarchy
                && button.interactable;
        }

        private void HideVerifiedDetailRows()
        {
            SetDetailRow(detailObjectiveRow, detailObjectiveText, false, string.Empty);
            SetDetailRow(
                detailCombatLessonRow,
                detailCombatLessonText,
                false,
                string.Empty);
            SetDetailRow(detailStoryRow, detailStoryText, false, string.Empty);
            SetDetailRow(detailSegmentRow, detailSegmentText, false, string.Empty);
        }

        private void HideUnverifiedDetailRows()
        {
            SetActive(detailRecommendedPowerRow, false);
            SetActive(detailLoadoutRow, false);
            SetActive(detailDurationRow, false);
            SetActive(detailThreatRow, false);
            SetActive(detailSummonRow, false);
            SetActive(detailRewardRow, false);
        }

        private static void ApplyVerifiedDetailRow(
            GameObject row,
            TMP_Text text,
            StageBriefingValueDisposition disposition,
            string value)
        {
            bool visible = IsPresent(disposition, value);
            SetDetailRow(row, text, visible, value);
        }

        private static bool IsPresent(
            StageBriefingValueDisposition disposition,
            string value)
        {
            return disposition == StageBriefingValueDisposition.Present
                && !string.IsNullOrWhiteSpace(value);
        }

        private static string ResolveContentStatusLabel(
            ChapterHubReviewContentStatus contentStatus)
        {
            return contentStatus switch
            {
                ChapterHubReviewContentStatus.CanonicalPlayable => CanonicalReviewStatus,
                ChapterHubReviewContentStatus.Announced => AnnouncedReviewStatus,
                ChapterHubReviewContentStatus.InProduction => PlannedReviewStatus,
                _ => string.Empty
            };
        }

        private static string ResolveAvailabilityCopy(
            ChapterHubReviewContentStatus contentStatus)
        {
            return contentStatus switch
            {
                ChapterHubReviewContentStatus.InProduction =>
                    "현재 구조와 화면 동선을 제작 중인 리뷰 슬롯입니다.\n"
                    + "플레이 가능 상태·일정·보상 정보는 연결하지 않았습니다.",
                ChapterHubReviewContentStatus.Announced =>
                    "향후 콘텐츠 구조를 위한 공지 슬롯입니다.\n"
                    + "플레이 가능 상태·일정·보상 정보는 이 샘플에서 주장하지 않습니다.",
                _ => string.Empty
            };
        }

        private static string BuildStoryText(StageBriefingReadModel briefing)
        {
            string segmentId = briefing?.StoryEntrySegmentId ?? string.Empty;
            string handoffId = briefing?.StoryEntryHandoffId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(segmentId))
            {
                return handoffId;
            }

            return string.IsNullOrWhiteSpace(handoffId)
                ? segmentId
                : segmentId + "  /  " + handoffId;
        }

        private static string BuildSegmentText(StageBriefingReadModel briefing)
        {
            var builder = new StringBuilder(512);
            for (int i = 0; i < briefing.SegmentCount; i++)
            {
                StageBriefingSegmentReadModel segment = briefing.GetSegment(i);
                if (segment == null || string.IsNullOrWhiteSpace(segment.RouteSegmentId))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append((i + 1).ToString("00"));
                builder.Append("  ");
                builder.Append(segment.RouteSegmentId);
            }

            return builder.ToString();
        }

        private static void AppendLabeledValue(
            StringBuilder builder,
            string label,
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append('\n');
            }

            builder.Append(label);
            builder.Append("  ");
            builder.Append(value);
        }

        private static void SetDetailRow(
            GameObject row,
            TMP_Text text,
            bool visible,
            string value)
        {
            SetActive(row, visible);
            if (row == null && text != null)
            {
                SetActive(text.gameObject, visible);
            }

            SetText(text, visible ? value : string.Empty);
        }

        private static void SetTextVisible(TMP_Text target, string value, bool visible)
        {
            if (target == null)
            {
                return;
            }

            target.text = visible ? value ?? string.Empty : string.Empty;
            target.gameObject.SetActive(visible);
        }

        private static void SetCanvasGroup(
            CanvasGroup group,
            bool visible,
            bool interactive)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible && interactive;
            group.blocksRaycasts = visible && interactive;
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button == null)
            {
                return;
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = value ?? string.Empty;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void AddButtonListener(Button button, UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveButtonListener(Button button, UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }
    }
}
