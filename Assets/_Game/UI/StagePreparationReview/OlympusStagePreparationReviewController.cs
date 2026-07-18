using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI.StagePreparationReview
{
    public enum StagePreparationReviewPanel
    {
        None = 0,
        StageIntel = 10,
        LoadoutOverview = 20,
        SummonDetail = 30,
        ReviewConfirm = 40
    }

    [DisallowMultipleComponent]
    public sealed class OlympusStagePreparationReviewController : MonoBehaviour
    {
        public const string CanonicalReviewStatus =
            "REVIEW SAMPLE / CANONICAL COMBAT PROJECTION";
        public const string LocalSelectionStatus =
            "REVIEW SAMPLE / SESSION-LOCAL TIER SELECTION";
        public const string PresetBoundaryStatus =
            "CANONICAL RUNTIME PRESET / NOT A STAGE RECOMMENDATION";
        public const string ConfirmationReviewStatus =
            "REVIEW SAMPLE / CONFIRMATION ONLY";
        public const string ConfirmedReviewStatus =
            "REVIEW CONFIRMED / NO ROUTE OR SAVE DISPATCH";
        public const string UnavailableProjectionStatusPrefix =
            "CANONICAL DATA UNAVAILABLE / ";
        public const string NeutralThreatPreviewStatus =
            "NO VERIFIED SOURCE / THREAT PREVIEW HIDDEN";
        public const string NeutralRuntimePresetStatus =
            "NO VERIFIED SOURCE / RUNTIME PRESET IS NOT A STAGE RECOMMENDATION";

        private const int SelectionDigestSchemaVersion = 1;
        private static readonly Color SelectedTierBackground =
            new Color(0.02f, 0.10f, 0.14f, 1f);
        private static readonly Color UnselectedTierBackground =
            new Color(0.025f, 0.060f, 0.095f, 1f);
        private static readonly Color SelectedTierLabel =
            new Color(0.25f, 0.90f, 1.00f, 1f);
        private static readonly Color UnselectedTierLabel =
            new Color(0.43f, 0.75f, 0.88f, 1f);

        [Serializable]
        public sealed class ReviewConfirmedEvent : UnityEvent<string>
        {
        }

        [Serializable]
        public sealed class SlotBinding
        {
            [SerializeField] private string slotId = string.Empty;
            [SerializeField] private Button inspectButton;
            [SerializeField] private Image iconImage;
            [SerializeField] private TMP_Text titleText;
            [SerializeField] private TMP_Text roleText;
            [SerializeField] private TMP_Text selectedTierText;

            public SlotBinding()
            {
            }

            public SlotBinding(
                string slotId,
                Button inspectButton,
                Image iconImage = null,
                TMP_Text titleText = null,
                TMP_Text roleText = null,
                TMP_Text selectedTierText = null)
            {
                Configure(
                    slotId,
                    inspectButton,
                    iconImage,
                    titleText,
                    roleText,
                    selectedTierText);
            }

            public string SlotId => slotId;
            public Button InspectButton => inspectButton;
            public Image IconImage => iconImage;
            public TMP_Text TitleText => titleText;
            public TMP_Text RoleText => roleText;
            public TMP_Text SelectedTierText => selectedTierText;

            public void Configure(
                string newSlotId,
                Button newInspectButton,
                Image newIconImage = null,
                TMP_Text newTitleText = null,
                TMP_Text newRoleText = null,
                TMP_Text newSelectedTierText = null)
            {
                slotId = newSlotId ?? string.Empty;
                inspectButton = newInspectButton;
                iconImage = newIconImage;
                titleText = newTitleText;
                roleText = newRoleText;
                selectedTierText = newSelectedTierText;
            }
        }

        [Header("Local Review Model")]
        [SerializeField] private StagePreparationReviewProfile profile;
        [SerializeField] private UIStageCatalog stageCatalog;

        [Header("Authored Panels")]
        [SerializeField] private CanvasGroup stageIntelPanel;
        [SerializeField] private CanvasGroup loadoutOverviewPanel;
        [SerializeField] private CanvasGroup summonDetailPanel;
        [SerializeField] private CanvasGroup reviewConfirmPanel;

        [Header("Stage Intel - Canonical Projection Only")]
        [SerializeField] private TMP_Text intelReviewTitleText;
        [SerializeField] private TMP_Text intelStageCodeText;
        [SerializeField] private TMP_Text intelStageTitleText;
        [SerializeField] private TMP_Text intelSummaryText;
        [SerializeField] private TMP_Text intelObjectiveText;
        [SerializeField] private TMP_Text intelThreatTagsText;
        [SerializeField] private TMP_Text intelRecommendedSummonRoleText;
        [SerializeField] private TMP_Text intelStatusText;
        [SerializeField] private Button intelContinueButton;

        [Header("Loadout Overview - Fixed Review Presentation")]
        [SerializeField] private TMP_Text loadoutPilotTitleText;
        [SerializeField] private TMP_Text loadoutPilotBoundaryText;
        [SerializeField] private TMP_Text loadoutStatusText;
        [SerializeField] private Button loadoutBackButton;
        [SerializeField] private Button loadoutReviewButton;
        [SerializeField] private SlotBinding[] slotBindings = Array.Empty<SlotBinding>();

        [Header("Summon Detail - Canonical Readouts")]
        [SerializeField] private Image detailIconImage;
        [SerializeField] private TMP_Text detailTitleText;
        [SerializeField] private TMP_Text detailRoleText;
        [SerializeField] private TMP_Text detailSelectedTierText;
        [SerializeField] private TMP_Text detailStageRoleText;
        [SerializeField] private TMP_Text detailPlayerUseText;
        [SerializeField] private TMP_Text detailSummonReadText;
        [SerializeField] private TMP_Text detailStatusText;
        [SerializeField] private Button detailTier1Button;
        [SerializeField] private Button detailTier2Button;
        [SerializeField] private Button detailTier3Button;
        [SerializeField] private Button detailBackButton;

        [Header("Review Confirmation")]
        [SerializeField] private TMP_Text confirmTitleText;
        [SerializeField] private TMP_Text confirmSummaryText;
        [SerializeField] private TMP_Text confirmDigestText;
        [SerializeField] private TMP_Text confirmStatusText;
        [SerializeField] private Button confirmBackButton;
        [SerializeField] private Button confirmAcceptButton;
        [SerializeField] private Button confirmRestartButton;
        [SerializeField] private ReviewConfirmedEvent reviewConfirmedEvent = new();

        private StagePreparationReviewSession session;
        private UIStageRouteProjection currentProjection;
        private UIStageRouteProjectionRejectReason lastProjectionRejectReason;
        private UnityAction[] slotButtonActions = Array.Empty<UnityAction>();
        private bool interactionsBound;
        private int projectionRefreshCount;
        private int confirmationDispatchCount;
        private string lastConfirmedSelectionDigest = string.Empty;
        private string preferredLoadoutFocusSlotId = string.Empty;
        private GameObject lastFocusTarget;

        public event Action<StagePreparationReviewPanel> PanelChanged;
        public event Action<string> ReviewConfirmed;

        public StagePreparationReviewSession Session => session;
        public StagePreparationReviewPhase CurrentPhase => session != null
            ? session.Phase
            : StagePreparationReviewPhase.StageIntel;
        public StagePreparationReviewPanel CurrentPanel { get; private set; }
        public UIStageRouteProjection CurrentProjection => currentProjection;
        public UIStageRouteProjection StageProjection => currentProjection;
        public UIStageRouteProjectionRejectReason LastProjectionRejectReason =>
            lastProjectionRejectReason;
        public int ProjectionRefreshCount => projectionRefreshCount;
        public string SelectedSlotId => session?.SelectedSlotId ?? string.Empty;
        public int SelectedTier => session?.SelectedTier ?? 0;
        public StagePreparationReviewSelection[] SelectionSnapshot => session != null
            ? session.CreateSelectionSnapshot()
            : Array.Empty<StagePreparationReviewSelection>();
        public string CurrentSelectionDigest =>
            TryComputeCurrentSelectionDigest(out string digest)
                ? digest
                : string.Empty;
        public string LastConfirmedSelectionDigest => lastConfirmedSelectionDigest;
        public bool IsReviewAccepted => session != null && session.IsReviewAccepted;
        public bool IsConfirmationAvailable => session != null
            && session.Phase == StagePreparationReviewPhase.ReviewConfirm
            && !session.IsReviewAccepted
            && currentProjection != null;
        public bool HasNeutralStageRecommendationBoundary =>
            HasExpectedNeutralRecommendationBoundary(currentProjection);
        public int ConfirmationDispatchCount => confirmationDispatchCount;
        public GameObject LastFocusTarget => lastFocusTarget;
        public int SlotBindingCount => slotBindings?.Length ?? 0;
        public ReviewConfirmedEvent ConfirmationEvent => reviewConfirmedEvent;

        public SlotBinding GetSlotBinding(int index)
        {
            if (index < 0 || index >= SlotBindingCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return slotBindings[index];
        }

        public bool TryGetSelectedTier(string slotId, out int tier)
        {
            if (session != null)
            {
                return session.TryGetSelectedTier(slotId, out tier);
            }

            tier = 0;
            return false;
        }

        public void ConfigureCore(
            StagePreparationReviewProfile newProfile,
            UIStageCatalog newStageCatalog)
        {
            bool rebind = BeginRuntimeReconfiguration();
            profile = newProfile;
            stageCatalog = newStageCatalog;
            BeginReview();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigurePanels(
            CanvasGroup newStageIntelPanel,
            CanvasGroup newLoadoutOverviewPanel,
            CanvasGroup newSummonDetailPanel,
            CanvasGroup newReviewConfirmPanel)
        {
            bool rebind = BeginRuntimeReconfiguration();
            stageIntelPanel = newStageIntelPanel;
            loadoutOverviewPanel = newLoadoutOverviewPanel;
            summonDetailPanel = newSummonDetailPanel;
            reviewConfirmPanel = newReviewConfirmPanel;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureIntelView(
            TMP_Text reviewTitle,
            TMP_Text stageCode,
            TMP_Text stageTitle,
            TMP_Text summary,
            TMP_Text objective,
            TMP_Text threatTags,
            TMP_Text recommendedSummonRole,
            TMP_Text status,
            Button continueButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            intelReviewTitleText = reviewTitle;
            intelStageCodeText = stageCode;
            intelStageTitleText = stageTitle;
            intelSummaryText = summary;
            intelObjectiveText = objective;
            intelThreatTagsText = threatTags;
            intelRecommendedSummonRoleText = recommendedSummonRole;
            intelStatusText = status;
            intelContinueButton = continueButton;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureLoadoutView(
            TMP_Text pilotTitle,
            TMP_Text pilotBoundary,
            TMP_Text status,
            Button backButton,
            Button reviewButton,
            SlotBinding[] bindings)
        {
            bool rebind = BeginRuntimeReconfiguration();
            loadoutPilotTitleText = pilotTitle;
            loadoutPilotBoundaryText = pilotBoundary;
            loadoutStatusText = status;
            loadoutBackButton = backButton;
            loadoutReviewButton = reviewButton;
            slotBindings = bindings ?? Array.Empty<SlotBinding>();
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureDetailView(
            Image icon,
            TMP_Text title,
            TMP_Text role,
            TMP_Text selectedTier,
            TMP_Text stageRole,
            TMP_Text playerUse,
            TMP_Text summonRead,
            TMP_Text status,
            Button tier1Button,
            Button tier2Button,
            Button tier3Button,
            Button backButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            detailIconImage = icon;
            detailTitleText = title;
            detailRoleText = role;
            detailSelectedTierText = selectedTier;
            detailStageRoleText = stageRole;
            detailPlayerUseText = playerUse;
            detailSummonReadText = summonRead;
            detailStatusText = status;
            detailTier1Button = tier1Button;
            detailTier2Button = tier2Button;
            detailTier3Button = tier3Button;
            detailBackButton = backButton;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureConfirmationView(
            TMP_Text title,
            TMP_Text summary,
            TMP_Text digest,
            TMP_Text status,
            Button backButton,
            Button acceptButton,
            Button restartButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            confirmTitleText = title;
            confirmSummaryText = summary;
            confirmDigestText = digest;
            confirmStatusText = status;
            confirmBackButton = backButton;
            confirmAcceptButton = acceptButton;
            confirmRestartButton = restartButton;
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
                TryRefreshCanonicalProjection();
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
            projectionRefreshCount = 0;
            confirmationDispatchCount = 0;
            lastConfirmedSelectionDigest = string.Empty;
            preferredLoadoutFocusSlotId = string.Empty;
            lastFocusTarget = null;

            if (profile != null && profile.TryValidate(out _))
            {
                try
                {
                    session = new StagePreparationReviewSession(profile);
                }
                catch (ArgumentException)
                {
                    session = null;
                }
            }

            if (session != null)
            {
                TryRefreshCanonicalProjection();
            }
            else
            {
                lastProjectionRejectReason =
                    UIStageRouteProjectionRejectReason.MissingCatalogEntryId;
            }

            ApplyCurrentView();
            return session != null && currentProjection != null;
        }

        public bool RestartReview()
        {
            return BeginReview();
        }

        public bool OpenLoadout()
        {
            if (session == null
                || session.IsReviewAccepted
                || !TryRefreshCanonicalProjection()
                || !session.TryOpenLoadout())
            {
                ApplyCurrentView();
                return false;
            }

            preferredLoadoutFocusSlotId = string.Empty;
            ApplyCurrentView();
            return true;
        }

        public bool InspectSlot(string slotId)
        {
            if (session == null
                || session.IsReviewAccepted
                || !TryRefreshCanonicalProjection()
                || !session.TryInspectSlot(slotId))
            {
                ApplyCurrentView();
                return false;
            }

            preferredLoadoutFocusSlotId = slotId;
            ApplyCurrentView();
            return true;
        }

        public bool SelectTier(int tier)
        {
            if (session == null
                || session.IsReviewAccepted
                || !TryRefreshCanonicalProjection()
                || !session.TrySelectTier(tier))
            {
                ApplyCurrentView();
                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool ReturnToLoadout()
        {
            if (session == null || !session.TryReturnToLoadout())
            {
                return false;
            }

            TryRefreshCanonicalProjection();
            ApplyCurrentView();
            return true;
        }

        public bool ReturnToStageIntel()
        {
            if (session == null || !session.TryReturnToStageIntel())
            {
                return false;
            }

            TryRefreshCanonicalProjection();
            ApplyCurrentView();
            return true;
        }

        public bool OpenReviewConfirm()
        {
            if (session == null
                || session.IsReviewAccepted
                || !TryRefreshCanonicalProjection()
                || !session.TryOpenReviewConfirm())
            {
                ApplyCurrentView();
                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool ConfirmReview()
        {
            if (session == null
                || session.IsReviewAccepted
                || session.Phase != StagePreparationReviewPhase.ReviewConfirm
                || !TryRefreshCanonicalProjection()
                || !TryComputeCurrentSelectionDigest(out string digest)
                || !session.TryAcceptReview())
            {
                ApplyCurrentView();
                return false;
            }

            confirmationDispatchCount++;
            lastConfirmedSelectionDigest = digest;
            reviewConfirmedEvent?.Invoke(digest);
            ReviewConfirmed?.Invoke(digest);
            ApplyCurrentView();
            return true;
        }

        public bool NavigateBack()
        {
            if (session == null)
            {
                return false;
            }

            if (session.IsReviewAccepted
                && session.Phase == StagePreparationReviewPhase.ReviewConfirm)
            {
                return RestartReview();
            }

            return session.Phase switch
            {
                StagePreparationReviewPhase.LoadoutOverview => ReturnToStageIntel(),
                StagePreparationReviewPhase.SummonDetail => ReturnToLoadout(),
                StagePreparationReviewPhase.ReviewConfirm => ReturnToLoadout(),
                _ => false
            };
        }

        public bool Back()
        {
            return NavigateBack();
        }

        public bool CloseReviewConfirm()
        {
            return ReturnToLoadout();
        }

        public void RefreshCurrentView()
        {
            if (session != null)
            {
                TryRefreshCanonicalProjection();
            }

            ApplyCurrentView();
        }

        private void BindInteractions()
        {
            if (interactionsBound)
            {
                return;
            }

            SlotBinding[] bindings = slotBindings ?? Array.Empty<SlotBinding>();
            slotButtonActions = new UnityAction[bindings.Length];
            for (int i = 0; i < bindings.Length; i++)
            {
                SlotBinding binding = bindings[i];
                if (binding?.InspectButton == null)
                {
                    continue;
                }

                string slotId = binding.SlotId;
                UnityAction action = () => InspectSlot(slotId);
                slotButtonActions[i] = action;
                binding.InspectButton.onClick.AddListener(action);
            }

            AddButtonListener(intelContinueButton, HandleOpenLoadoutClicked);
            AddButtonListener(loadoutBackButton, HandleNavigateBackClicked);
            AddButtonListener(loadoutReviewButton, HandleOpenReviewConfirmClicked);
            AddButtonListener(detailTier1Button, HandleTier1Clicked);
            AddButtonListener(detailTier2Button, HandleTier2Clicked);
            AddButtonListener(detailTier3Button, HandleTier3Clicked);
            AddButtonListener(detailBackButton, HandleNavigateBackClicked);
            AddButtonListener(confirmBackButton, HandleNavigateBackClicked);
            AddButtonListener(confirmAcceptButton, HandleConfirmReviewClicked);
            AddButtonListener(confirmRestartButton, HandleRestartReviewClicked);
            interactionsBound = true;
        }

        private void UnbindInteractions()
        {
            if (!interactionsBound)
            {
                return;
            }

            int count = Math.Min(
                slotBindings?.Length ?? 0,
                slotButtonActions?.Length ?? 0);
            for (int i = 0; i < count; i++)
            {
                SlotBinding binding = slotBindings[i];
                UnityAction action = slotButtonActions[i];
                if (binding?.InspectButton != null && action != null)
                {
                    binding.InspectButton.onClick.RemoveListener(action);
                }
            }

            RemoveButtonListener(intelContinueButton, HandleOpenLoadoutClicked);
            RemoveButtonListener(loadoutBackButton, HandleNavigateBackClicked);
            RemoveButtonListener(loadoutReviewButton, HandleOpenReviewConfirmClicked);
            RemoveButtonListener(detailTier1Button, HandleTier1Clicked);
            RemoveButtonListener(detailTier2Button, HandleTier2Clicked);
            RemoveButtonListener(detailTier3Button, HandleTier3Clicked);
            RemoveButtonListener(detailBackButton, HandleNavigateBackClicked);
            RemoveButtonListener(confirmBackButton, HandleNavigateBackClicked);
            RemoveButtonListener(confirmAcceptButton, HandleConfirmReviewClicked);
            RemoveButtonListener(confirmRestartButton, HandleRestartReviewClicked);
            slotButtonActions = Array.Empty<UnityAction>();
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

        private void EndRuntimeReconfiguration(bool rebind)
        {
            if (rebind && Application.isPlaying && isActiveAndEnabled)
            {
                BindInteractions();
            }
        }

        private void HandleOpenLoadoutClicked() => OpenLoadout();
        private void HandleNavigateBackClicked() => NavigateBack();
        private void HandleOpenReviewConfirmClicked() => OpenReviewConfirm();
        private void HandleTier1Clicked() => SelectTier(1);
        private void HandleTier2Clicked() => SelectTier(2);
        private void HandleTier3Clicked() => SelectTier(3);
        private void HandleConfirmReviewClicked() => ConfirmReview();
        private void HandleRestartReviewClicked() => RestartReview();

        private bool TryRefreshCanonicalProjection()
        {
            projectionRefreshCount++;
            currentProjection = null;
            lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;

            string catalogEntryId = session?.CanonicalCatalogEntryId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(catalogEntryId))
            {
                lastProjectionRejectReason =
                    UIStageRouteProjectionRejectReason.MissingCatalogEntryId;
                return false;
            }

            if (stageCatalog == null)
            {
                lastProjectionRejectReason =
                    UIStageRouteProjectionRejectReason.CatalogEntryNotFound;
                return false;
            }

            if (!stageCatalog.TryCreateRouteProjection(
                    catalogEntryId,
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

            if (!HasExpectedNeutralRecommendationBoundary(projection))
            {
                lastProjectionRejectReason =
                    UIStageRouteProjectionRejectReason.InvalidStageBriefingActionContract;
                return false;
            }

            currentProjection = projection;
            lastProjectionRejectReason = UIStageRouteProjectionRejectReason.None;
            return true;
        }

        private bool TryComputeCurrentSelectionDigest(out string digest)
        {
            digest = string.Empty;
            if (session == null || currentProjection == null)
            {
                return false;
            }

            StagePreparationReviewSelection[] selections =
                session.CreateSelectionSnapshot();
            if (selections.Length != StagePreparationReviewProfile.RequiredSlotCount)
            {
                return false;
            }

            var builder = new StringBuilder(1024);
            AppendCanonicalField(
                builder,
                "selectionDigestSchemaVersion",
                SelectionDigestSchemaVersion);
            AppendCanonicalField(builder, "reviewId", session.ReviewId);
            AppendCanonicalField(
                builder,
                "catalogEntryId",
                session.CanonicalCatalogEntryId);
            AppendCanonicalField(
                builder,
                "canonicalProjectionDigest",
                currentProjection.CanonicalProjectionDigest);
            for (int i = 0; i < selections.Length; i++)
            {
                StagePreparationReviewSelection selection = selections[i];
                AppendCanonicalField(builder, $"slot[{i}].id", selection.SlotId);
                AppendCanonicalField(builder, $"slot[{i}].actionId", selection.ActionId);
                AppendCanonicalField(
                    builder,
                    $"slot[{i}].selectedTier",
                    selection.SelectedTier);
            }

            byte[] payload = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(payload);
            }

            char[] characters = new char[hash.Length * 2];
            const string alphabet = "0123456789abcdef";
            for (int i = 0; i < hash.Length; i++)
            {
                characters[i * 2] = alphabet[hash[i] >> 4];
                characters[(i * 2) + 1] = alphabet[hash[i] & 0x0f];
            }

            digest = new string(characters);
            return true;
        }

        private void ApplyCurrentView()
        {
            RefreshSlotBindings();
            if (session == null)
            {
                RenderUnavailableStageIntel();
                ShowOnly(StagePreparationReviewPanel.StageIntel);
                return;
            }

            switch (session.Phase)
            {
                case StagePreparationReviewPhase.StageIntel:
                    RenderStageIntel();
                    ShowOnly(StagePreparationReviewPanel.StageIntel);
                    break;
                case StagePreparationReviewPhase.LoadoutOverview:
                    RenderLoadoutOverview();
                    ShowOnly(StagePreparationReviewPanel.LoadoutOverview);
                    break;
                case StagePreparationReviewPhase.SummonDetail:
                    RenderSummonDetail();
                    ShowOnly(StagePreparationReviewPanel.SummonDetail);
                    break;
                case StagePreparationReviewPhase.ReviewConfirm:
                    RenderConfirmation();
                    ShowOnly(StagePreparationReviewPanel.ReviewConfirm);
                    break;
                default:
                    RenderUnavailableStageIntel();
                    ShowOnly(StagePreparationReviewPanel.StageIntel);
                    break;
            }
        }

        private void RenderStageIntel()
        {
            SetText(intelReviewTitleText, profile?.TitleFallback);
            if (currentProjection == null)
            {
                ClearCanonicalIntel();
                SetText(
                    intelStatusText,
                    WithPresetBoundary(
                        UnavailableProjectionStatusPrefix
                        + lastProjectionRejectReason));
                SetButtonInteractable(intelContinueButton, false);
                return;
            }

            SetText(intelStageCodeText, currentProjection.PlayableStageId);
            SetText(intelStageTitleText, ResolveCanonicalStageTitle());
            SetText(
                intelSummaryText,
                currentProjection.Briefing != null
                    ? currentProjection.Briefing.CombatLesson
                    : string.Empty);
            SetText(
                intelObjectiveText,
                currentProjection.Briefing != null
                    ? currentProjection.Briefing.Objective
                    : string.Empty);
            SetText(intelThreatTagsText, NeutralThreatPreviewStatus);
            SetText(intelRecommendedSummonRoleText, NeutralRuntimePresetStatus);
            SetText(intelStatusText, WithPresetBoundary(CanonicalReviewStatus));
            SetButtonInteractable(
                intelContinueButton,
                !session.IsReviewAccepted);
        }

        private void RenderUnavailableStageIntel()
        {
            SetText(intelReviewTitleText, profile?.TitleFallback);
            ClearCanonicalIntel();
            SetText(
                intelStatusText,
                WithPresetBoundary(
                    UnavailableProjectionStatusPrefix
                    + lastProjectionRejectReason));
            SetButtonInteractable(intelContinueButton, false);
        }

        private void ClearCanonicalIntel()
        {
            SetText(intelStageCodeText, string.Empty);
            SetText(intelStageTitleText, string.Empty);
            SetText(intelSummaryText, string.Empty);
            SetText(intelObjectiveText, string.Empty);
            SetText(intelThreatTagsText, string.Empty);
            SetText(intelRecommendedSummonRoleText, string.Empty);
        }

        private void RenderLoadoutOverview()
        {
            SetText(loadoutPilotTitleText, profile?.PilotTitleFallback);
            SetText(loadoutPilotBoundaryText, profile?.PilotBoundaryFallback);
            SetText(
                loadoutStatusText,
                currentProjection != null
                    ? PresetBoundaryStatus
                    : WithPresetBoundary(
                        UnavailableProjectionStatusPrefix
                        + lastProjectionRejectReason));
            SetButtonInteractable(loadoutBackButton, true);
            SetButtonInteractable(
                loadoutReviewButton,
                currentProjection != null && !session.IsReviewAccepted);
        }

        private void RenderSummonDetail()
        {
            StagePreparationReviewProfile.SlotDefinition slot = session.SelectedSlot;
            int tier = session.SelectedTier;
            Player.SummonSlotActionProfile.SummonTierReadout readout = default;
            bool hasReadout = slot != null
                && slot.TryGetTierReadout(
                    tier,
                    out readout);

            ApplyImage(detailIconImage, slot?.Icon);
            SetText(detailTitleText, slot?.TitleFallback);
            SetText(detailRoleText, slot?.RoleFallback);
            SetText(
                detailSelectedTierText,
                hasReadout ? readout.TierLabel : string.Empty);
            SetText(
                detailStageRoleText,
                hasReadout ? readout.StageRole : string.Empty);
            SetText(
                detailPlayerUseText,
                hasReadout ? readout.PlayerUse : string.Empty);
            SetText(
                detailSummonReadText,
                hasReadout ? readout.SummonRead : string.Empty);
            SetText(
                detailStatusText,
                currentProjection != null && hasReadout
                    ? PresetBoundaryStatus
                    : currentProjection == null
                        ? WithPresetBoundary(
                            UnavailableProjectionStatusPrefix
                            + lastProjectionRejectReason)
                        : WithPresetBoundary(
                            "CANONICAL SUMMON READOUT UNAVAILABLE"));

            bool canSelect = currentProjection != null
                && hasReadout
                && !session.IsReviewAccepted;
            SetButtonInteractable(detailTier1Button, canSelect);
            SetButtonInteractable(detailTier2Button, canSelect);
            SetButtonInteractable(detailTier3Button, canSelect);
            ApplyTierButtonPresentation(detailTier1Button, tier == 1);
            ApplyTierButtonPresentation(detailTier2Button, tier == 2);
            ApplyTierButtonPresentation(detailTier3Button, tier == 3);
            SetButtonInteractable(detailBackButton, true);
        }

        private static void ApplyTierButtonPresentation(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            if (button.targetGraphic != null)
            {
                button.targetGraphic.color = selected
                    ? SelectedTierBackground
                    : UnselectedTierBackground;
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(includeInactive: true);
            if (label != null)
            {
                label.color = selected ? SelectedTierLabel : UnselectedTierLabel;
            }
        }

        private void RenderConfirmation()
        {
            bool hasProjection = currentProjection != null;
            string digest = session.IsReviewAccepted
                ? lastConfirmedSelectionDigest
                : CurrentSelectionDigest;
            SetText(
                confirmTitleText,
                hasProjection
                    ? ResolveCanonicalStageTitle()
                    : string.Empty);
            SetText(
                confirmSummaryText,
                hasProjection ? BuildConfirmationSummary() : string.Empty);
            SetText(confirmDigestText, digest);
            SetText(
                confirmStatusText,
                session.IsReviewAccepted
                    ? WithPresetBoundary(ConfirmedReviewStatus)
                    : hasProjection
                        ? WithPresetBoundary(ConfirmationReviewStatus)
                        : WithPresetBoundary(
                            UnavailableProjectionStatusPrefix
                            + lastProjectionRejectReason));
            SetButtonInteractable(confirmBackButton, true);
            SetActive(
                confirmAcceptButton != null
                    ? confirmAcceptButton.gameObject
                    : null,
                !session.IsReviewAccepted);
            SetButtonInteractable(
                confirmAcceptButton,
                hasProjection && !session.IsReviewAccepted);
            SetActive(
                confirmRestartButton != null
                    ? confirmRestartButton.gameObject
                    : null,
                session.IsReviewAccepted);
            SetButtonInteractable(confirmRestartButton, session.IsReviewAccepted);
        }

        private void RefreshSlotBindings()
        {
            SlotBinding[] bindings = slotBindings ?? Array.Empty<SlotBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                SlotBinding binding = bindings[i];
                if (binding == null)
                {
                    continue;
                }

                StagePreparationReviewProfile.SlotDefinition slot = null;
                bool found = session != null
                    && session.TryGetSlot(
                        binding.SlotId,
                        out slot);
                SetActive(
                    binding.InspectButton != null
                        ? binding.InspectButton.gameObject
                        : null,
                    found);
                ApplyImage(binding.IconImage, found ? slot.Icon : null);
                SetText(binding.TitleText, found ? slot.TitleFallback : string.Empty);
                SetText(binding.RoleText, found ? slot.RoleFallback : string.Empty);
                int tier = 0;
                if (found)
                {
                    session.TryGetSelectedTier(slot.SlotId, out tier);
                }

                SetText(
                    binding.SelectedTierText,
                    tier > 0 ? $"TIER {tier}" : string.Empty);
                SetButtonInteractable(
                    binding.InspectButton,
                    found
                    && currentProjection != null
                    && !session.IsReviewAccepted
                    && session.Phase == StagePreparationReviewPhase.LoadoutOverview);
            }
        }

        private string BuildConfirmationSummary()
        {
            var builder = new StringBuilder(512);
            builder.Append(ResolveCanonicalStageTitle());
            builder.Append('\n');
            builder.Append(profile?.PilotTitleFallback ?? string.Empty);

            StagePreparationReviewSelection[] selections =
                session.CreateSelectionSnapshot();
            for (int i = 0; i < selections.Length; i++)
            {
                StagePreparationReviewSelection selection = selections[i];
                builder.Append('\n');
                builder.Append(selection.SlotId);
                builder.Append(" / ");
                builder.Append(selection.ActionId);
                builder.Append(" / TIER ");
                builder.Append(selection.SelectedTier.ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private string ResolveCanonicalStageTitle()
        {
            if (currentProjection?.Briefing != null
                && !string.IsNullOrWhiteSpace(currentProjection.Briefing.Title))
            {
                return currentProjection.Briefing.Title;
            }

            return currentProjection?.DisplayName ?? string.Empty;
        }

        private static string WithPresetBoundary(string status)
        {
            return string.IsNullOrWhiteSpace(status)
                ? PresetBoundaryStatus
                : status + "\n" + PresetBoundaryStatus;
        }

        private static bool HasExpectedNeutralRecommendationBoundary(
            UIStageRouteProjection projection)
        {
            return projection?.Briefing != null
                && projection.Briefing.FeaturedThreatDisposition
                    == LevelDesign.StageBriefingValueDisposition.NoVerifiedSource
                && projection.Briefing.RecommendedLoadoutDisposition
                    == LevelDesign.StageBriefingValueDisposition.NoVerifiedSource
                && projection.Briefing.FeaturedSummonNeedDisposition
                    == LevelDesign.StageBriefingValueDisposition.NoVerifiedSource;
        }

        private void ShowOnly(StagePreparationReviewPanel panel)
        {
            SetPanelState(
                stageIntelPanel,
                panel == StagePreparationReviewPanel.StageIntel);
            SetPanelState(
                loadoutOverviewPanel,
                panel == StagePreparationReviewPanel.LoadoutOverview);
            SetPanelState(
                summonDetailPanel,
                panel == StagePreparationReviewPanel.SummonDetail);
            SetPanelState(
                reviewConfirmPanel,
                panel == StagePreparationReviewPanel.ReviewConfirm);

            bool changed = CurrentPanel != panel;
            CurrentPanel = panel;
            SelectDefaultControl(panel);
            if (changed)
            {
                PanelChanged?.Invoke(panel);
            }
        }

        private void SelectDefaultControl(StagePreparationReviewPanel panel)
        {
            Button target = panel switch
            {
                StagePreparationReviewPanel.StageIntel => intelContinueButton,
                StagePreparationReviewPanel.LoadoutOverview =>
                    FindPreferredLoadoutSlotButton()
                    ?? FindFirstUsableSlotButton()
                    ?? loadoutReviewButton
                    ?? loadoutBackButton,
                StagePreparationReviewPanel.SummonDetail => ResolveSelectedTierButton()
                    ?? detailBackButton,
                StagePreparationReviewPanel.ReviewConfirm =>
                    IsUsableButton(confirmAcceptButton)
                        ? confirmAcceptButton
                        : IsUsableButton(confirmRestartButton)
                            ? confirmRestartButton
                            : confirmBackButton,
                _ => null
            };

            lastFocusTarget = IsUsableButton(target) ? target.gameObject : null;
            if (lastFocusTarget != null
                && EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject != lastFocusTarget)
            {
                EventSystem.current.SetSelectedGameObject(lastFocusTarget);
            }
        }

        private Button ResolveSelectedTierButton()
        {
            return session?.SelectedTier switch
            {
                1 => detailTier1Button,
                2 => detailTier2Button,
                3 => detailTier3Button,
                _ => null
            };
        }

        private Button FindFirstUsableSlotButton()
        {
            SlotBinding[] bindings = slotBindings ?? Array.Empty<SlotBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                Button button = bindings[i]?.InspectButton;
                if (IsUsableButton(button))
                {
                    return button;
                }
            }

            return null;
        }

        private Button FindPreferredLoadoutSlotButton()
        {
            if (string.IsNullOrWhiteSpace(preferredLoadoutFocusSlotId))
            {
                return null;
            }

            SlotBinding[] bindings = slotBindings ?? Array.Empty<SlotBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                SlotBinding binding = bindings[i];
                if (binding != null
                    && string.Equals(
                        binding.SlotId,
                        preferredLoadoutFocusSlotId,
                        StringComparison.Ordinal)
                    && IsUsableButton(binding.InspectButton))
                {
                    return binding.InspectButton;
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

        private static void SetPanelState(CanvasGroup panel, bool visible)
        {
            if (panel == null)
            {
                return;
            }

            panel.gameObject.SetActive(visible);
            panel.alpha = visible ? 1f : 0f;
            panel.interactable = visible;
            panel.blocksRaycasts = visible;
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void ApplyImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
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

        private static void AppendCanonicalField(
            StringBuilder builder,
            string key,
            int value)
        {
            AppendCanonicalField(
                builder,
                key,
                value.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendCanonicalField(
            StringBuilder builder,
            string key,
            string value)
        {
            string safeValue = value ?? string.Empty;
            builder.Append(key);
            builder.Append('=');
            builder.Append(safeValue.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(safeValue);
            builder.Append('\n');
        }
    }
}
