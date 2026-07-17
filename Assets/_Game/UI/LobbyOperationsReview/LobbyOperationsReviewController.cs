using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI.LobbyOperationsReview
{
    public enum LobbyOperationsReviewPanel
    {
        None = 0,
        Closed = 10,
        Directory = 20,
        Detail = 30,
        Confirm = 40
    }

    public enum LobbyOperationsReviewDispositionRowKind
    {
        None = 0,
        Production = 10,
        Service = 20,
        Account = 30,
        ServerClock = 40,
        Schedule = 50,
        Progress = 60,
        Attention = 70,
        Action = 80
    }

    [DisallowMultipleComponent]
    public sealed class LobbyOperationsReviewController : MonoBehaviour
    {
        public const string ClosedReviewStatus =
            "LOCAL UI REVIEW / NO LIVE SERVICE";
        public const string DirectoryReviewStatus =
            "FOUR EXPLANATION SURFACES / NO ACCOUNT OR SERVICE STATE";
        public const string NoticeSourceStatus =
            "LOCAL REVIEW FIXTURE / SOURCES NOT REQUIRED";
        public const string MailboxSourceStatus =
            "NO VERIFIED SERVICE / ACCOUNT / ATTENTION SOURCE";
        public const string MissionsSourceStatus =
            "NO VERIFIED ACCOUNT / PROGRESS / ATTENTION SOURCE";
        public const string EventCalendarSourceStatus =
            "DEFINITION-ONLY SCHEDULE / NO VERIFIED SERVER CLOCK OR VERDICT";
        public const string NoticeDetailStatus =
            "LOCAL REVIEW FIXTURE / CONFIRMATION ONLY";
        public const string MailboxDetailStatus =
            "EXPLANATION ONLY / NO VERIFIED SERVICE OR ACCOUNT SOURCE";
        public const string MissionsDetailStatus =
            "EXPLANATION ONLY / NO VERIFIED ACCOUNT OR PROGRESS SOURCE";
        public const string EventCalendarDetailStatus =
            "DEFINITION ONLY / NO SERVER-CLOCK VERDICT";
        public const string ConfirmReadyStatus =
            "LOCAL UI REVIEW / NO PRODUCT MUTATION";
        public const string ConfirmedReviewStatus =
            "REVIEW ACKNOWLEDGED / SESSION ONLY";
        public const string ConfirmSummary =
            "Confirms only that this local UI fixture was inspected. "
            + "No read state, route, service, reward, or persistence is changed.";

        private static readonly LobbyOperationsReviewDispositionRowKind[]
            RequiredDispositionRows =
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

        [Serializable]
        public sealed class LocalReviewAcknowledgedEvent : UnityEvent<string>
        {
        }

        [Serializable]
        public sealed class EntryButtonBinding
        {
            [SerializeField] private string entryId = string.Empty;
            [SerializeField] private Button button;
            [SerializeField] private CanvasGroup canvasGroup;
            [SerializeField] private TMP_Text titleText;
            [SerializeField] private TMP_Text sourceStatusText;

            public EntryButtonBinding()
            {
            }

            public EntryButtonBinding(
                string entryId,
                Button button,
                CanvasGroup canvasGroup = null,
                TMP_Text titleText = null,
                TMP_Text sourceStatusText = null)
            {
                Configure(entryId, button, canvasGroup, titleText, sourceStatusText);
            }

            public string EntryId => entryId;
            public Button Button => button;
            public CanvasGroup CanvasGroup => canvasGroup;
            public TMP_Text TitleText => titleText;
            public TMP_Text SourceStatusText => sourceStatusText;

            public void Configure(
                string newEntryId,
                Button newButton,
                CanvasGroup newCanvasGroup = null,
                TMP_Text newTitleText = null,
                TMP_Text newSourceStatusText = null)
            {
                entryId = newEntryId ?? string.Empty;
                button = newButton;
                canvasGroup = newCanvasGroup;
                titleText = newTitleText;
                sourceStatusText = newSourceStatusText;
            }
        }

        [Serializable]
        public sealed class DispositionRowBinding
        {
            [SerializeField] private LobbyOperationsReviewDispositionRowKind rowKind;
            [SerializeField] private GameObject rowRoot;
            [SerializeField] private TMP_Text labelText;
            [SerializeField] private TMP_Text valueText;

            public DispositionRowBinding()
            {
            }

            public DispositionRowBinding(
                LobbyOperationsReviewDispositionRowKind rowKind,
                GameObject rowRoot,
                TMP_Text labelText = null,
                TMP_Text valueText = null)
            {
                Configure(rowKind, rowRoot, labelText, valueText);
            }

            public LobbyOperationsReviewDispositionRowKind RowKind => rowKind;
            public GameObject RowRoot => rowRoot;
            public TMP_Text LabelText => labelText;
            public TMP_Text ValueText => valueText;

            public void Configure(
                LobbyOperationsReviewDispositionRowKind newRowKind,
                GameObject newRowRoot,
                TMP_Text newLabelText = null,
                TMP_Text newValueText = null)
            {
                rowKind = newRowKind;
                rowRoot = newRowRoot;
                labelText = newLabelText;
                valueText = newValueText;
            }
        }

        [Header("Local Review Model")]
        [SerializeField] private LobbyOperationsReviewProfile profile;

        [Header("Authored Panels")]
        [SerializeField] private CanvasGroup closedPanel;
        [SerializeField] private CanvasGroup directoryPanel;
        [SerializeField] private CanvasGroup detailPanel;
        [SerializeField] private CanvasGroup confirmPanel;

        [Header("Drawer Closed")]
        [SerializeField] private TMP_Text closedReviewLabelText;
        [SerializeField] private TMP_Text closedStatusText;
        [SerializeField] private Button closedOpenButton;

        [Header("Directory")]
        [SerializeField] private TMP_Text directoryTitleText;
        [SerializeField] private TMP_Text directoryStatusText;
        [SerializeField] private Button directoryBackButton;
        [SerializeField] private Button directoryCloseButton;
        [SerializeField] private EntryButtonBinding[] entryBindings =
            Array.Empty<EntryButtonBinding>();

        [Header("Entry Detail")]
        [SerializeField] private TMP_Text detailKindText;
        [SerializeField] private TMP_Text detailTitleText;
        [SerializeField] private TMP_Text detailExplanationText;
        [SerializeField] private TMP_Text detailStatusText;
        [SerializeField] private DispositionRowBinding[] dispositionRows =
            Array.Empty<DispositionRowBinding>();
        [SerializeField] private Button detailBackButton;
        [SerializeField] private Button detailCloseButton;
        [SerializeField] private Button detailReviewCtaButton;

        [Header("Review Confirmation")]
        [SerializeField] private TMP_Text confirmTitleText;
        [SerializeField] private TMP_Text confirmSummaryText;
        [SerializeField] private TMP_Text confirmStatusText;
        [SerializeField] private Button confirmBackButton;
        [SerializeField] private Button confirmCloseButton;
        [SerializeField] private Button confirmAcknowledgeButton;
        [SerializeField] private LocalReviewAcknowledgedEvent reviewAcknowledgedEvent = new();

        private LobbyOperationsReviewSession session;
        private UnityAction[] entryButtonActions = Array.Empty<UnityAction>();
        private bool interactionsBound;
        private int acknowledgementDispatchCount;
        private string lastAcknowledgedEntryId = string.Empty;

        public event Action<LobbyOperationsReviewPanel> PanelChanged;
        public event Action<string> ReviewAcknowledged;

        public LobbyOperationsReviewSession Session => session;
        public LobbyOperationsReviewPhase CurrentPhase =>
            session != null ? session.Phase : LobbyOperationsReviewPhase.Closed;
        public LobbyOperationsReviewPanel CurrentPanel { get; private set; }
        public string SelectedEntryId => session?.SelectedEntryId ?? string.Empty;
        public LobbyOperationsReviewProfile.EntryDefinition SelectedEntry =>
            session?.SelectedEntry;
        public bool IsReviewAcknowledged =>
            session != null && session.IsReviewAcknowledged;
        public bool IsReviewConfirmAvailable => CanOpenReviewConfirm();
        public bool IsReviewCtaVisible =>
            session != null
            && session.Phase == LobbyOperationsReviewPhase.EntryDetail
            && CanOpenReviewConfirm();
        public int AcknowledgementDispatchCount => acknowledgementDispatchCount;
        public string LastAcknowledgedEntryId => lastAcknowledgedEntryId;
        public LocalReviewAcknowledgedEvent ReviewAcknowledgedEvent =>
            reviewAcknowledgedEvent;
        public LocalReviewAcknowledgedEvent AcknowledgementEvent =>
            reviewAcknowledgedEvent;
        public int EntryBindingCount => entryBindings?.Length ?? 0;
        public int DispositionRowCount => dispositionRows?.Length ?? 0;
        public bool HasExactEntryBindings => ValidateExactEntryBindings();
        public bool HasExactDispositionRows => ValidateExactDispositionRows();
        public Button LastFocusTarget { get; private set; }
        public string CurrentDetailTitle { get; private set; } = string.Empty;
        public string CurrentDetailExplanation { get; private set; } = string.Empty;
        public string CurrentDetailStatus { get; private set; } = string.Empty;

        public EntryButtonBinding GetEntryBinding(int index)
        {
            if (index < 0 || index >= EntryBindingCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return entryBindings[index];
        }

        public DispositionRowBinding GetDispositionRowBinding(int index)
        {
            if (index < 0 || index >= DispositionRowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return dispositionRows[index];
        }

        public void ConfigureCore(LobbyOperationsReviewProfile newProfile)
        {
            bool rebind = BeginRuntimeReconfiguration();
            profile = newProfile;
            BeginReview();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigurePanels(
            CanvasGroup newClosedPanel,
            CanvasGroup newDirectoryPanel,
            CanvasGroup newDetailPanel,
            CanvasGroup newConfirmPanel)
        {
            closedPanel = newClosedPanel;
            directoryPanel = newDirectoryPanel;
            detailPanel = newDetailPanel;
            confirmPanel = newConfirmPanel;
            ApplyCurrentView();
        }

        public void ConfigureClosedView(
            TMP_Text reviewLabel,
            TMP_Text status,
            Button openButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            closedReviewLabelText = reviewLabel;
            closedStatusText = status;
            closedOpenButton = openButton;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureDirectoryView(
            TMP_Text title,
            TMP_Text status,
            Button backButton,
            Button closeButton,
            EntryButtonBinding[] bindings)
        {
            bool rebind = BeginRuntimeReconfiguration();
            directoryTitleText = title;
            directoryStatusText = status;
            directoryBackButton = backButton;
            directoryCloseButton = closeButton;
            entryBindings = bindings ?? Array.Empty<EntryButtonBinding>();
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureDetailView(
            TMP_Text kind,
            TMP_Text title,
            TMP_Text explanation,
            TMP_Text status,
            DispositionRowBinding[] rows,
            Button backButton,
            Button closeButton,
            Button reviewCtaButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            detailKindText = kind;
            detailTitleText = title;
            detailExplanationText = explanation;
            detailStatusText = status;
            dispositionRows = rows ?? Array.Empty<DispositionRowBinding>();
            detailBackButton = backButton;
            detailCloseButton = closeButton;
            detailReviewCtaButton = reviewCtaButton;
            ApplyCurrentView();
            EndRuntimeReconfiguration(rebind);
        }

        public void ConfigureConfirmationView(
            TMP_Text title,
            TMP_Text summary,
            TMP_Text status,
            Button backButton,
            Button closeButton,
            Button acknowledgeButton)
        {
            bool rebind = BeginRuntimeReconfiguration();
            confirmTitleText = title;
            confirmSummaryText = summary;
            confirmStatusText = status;
            confirmBackButton = backButton;
            confirmCloseButton = closeButton;
            confirmAcknowledgeButton = acknowledgeButton;
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

        private void OnValidate()
        {
            entryBindings ??= Array.Empty<EntryButtonBinding>();
            dispositionRows ??= Array.Empty<DispositionRowBinding>();
        }

        public bool BeginReview()
        {
            session = null;
            acknowledgementDispatchCount = 0;
            lastAcknowledgedEntryId = string.Empty;

            if (profile != null && profile.TryValidate(out _))
            {
                try
                {
                    session = new LobbyOperationsReviewSession(profile);
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

        public bool OpenDrawer()
        {
            if (session == null || !session.TryOpenDrawer())
            {
                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool SelectEntry(string entryId)
        {
            if (session == null || !session.TrySelectEntry(entryId))
            {
                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool OpenReviewConfirm()
        {
            if (!CanOpenReviewConfirm() || !session.TryOpenReviewConfirm())
            {
                ApplyCurrentView();
                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool AcknowledgeReview()
        {
            if (session == null
                || !session.TryAcknowledgeReview(out string acknowledgedEntryId))
            {
                ApplyCurrentView();
                return false;
            }

            acknowledgementDispatchCount++;
            lastAcknowledgedEntryId = acknowledgedEntryId;
            reviewAcknowledgedEvent?.Invoke(acknowledgedEntryId);
            ReviewAcknowledged?.Invoke(acknowledgedEntryId);
            ApplyCurrentView();
            return true;
        }

        public bool NavigateBack()
        {
            if (session == null || !session.TryBack())
            {
                return false;
            }

            ApplyCurrentView();
            return true;
        }

        public bool Back()
        {
            return NavigateBack();
        }

        public bool Close()
        {
            if (session == null || !session.TryClose())
            {
                return false;
            }

            ApplyCurrentView();
            return true;
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

            int bindingCount = entryBindings?.Length ?? 0;
            entryButtonActions = new UnityAction[bindingCount];
            bool exactBindings = ValidateExactEntryBindings();
            for (int index = 0; index < bindingCount; index++)
            {
                EntryButtonBinding binding = entryBindings[index];
                if (!exactBindings || binding?.Button == null)
                {
                    continue;
                }

                string entryId = binding.EntryId;
                UnityAction action = () => SelectEntry(entryId);
                entryButtonActions[index] = action;
                binding.Button.onClick.AddListener(action);
            }

            AddButtonListener(closedOpenButton, HandleOpenClicked);
            AddButtonListener(directoryBackButton, HandleBackClicked);
            AddButtonListener(directoryCloseButton, HandleCloseClicked);
            AddButtonListener(detailBackButton, HandleBackClicked);
            AddButtonListener(detailCloseButton, HandleCloseClicked);
            AddButtonListener(detailReviewCtaButton, HandleReviewCtaClicked);
            AddButtonListener(confirmBackButton, HandleBackClicked);
            AddButtonListener(confirmCloseButton, HandleCloseClicked);
            AddButtonListener(confirmAcknowledgeButton, HandleAcknowledgeClicked);
            interactionsBound = true;
        }

        private void UnbindInteractions()
        {
            if (!interactionsBound)
            {
                return;
            }

            int bindingCount = Math.Min(
                entryBindings?.Length ?? 0,
                entryButtonActions?.Length ?? 0);
            for (int index = 0; index < bindingCount; index++)
            {
                EntryButtonBinding binding = entryBindings[index];
                UnityAction action = entryButtonActions[index];
                if (binding?.Button != null && action != null)
                {
                    binding.Button.onClick.RemoveListener(action);
                }
            }

            RemoveButtonListener(closedOpenButton, HandleOpenClicked);
            RemoveButtonListener(directoryBackButton, HandleBackClicked);
            RemoveButtonListener(directoryCloseButton, HandleCloseClicked);
            RemoveButtonListener(detailBackButton, HandleBackClicked);
            RemoveButtonListener(detailCloseButton, HandleCloseClicked);
            RemoveButtonListener(detailReviewCtaButton, HandleReviewCtaClicked);
            RemoveButtonListener(confirmBackButton, HandleBackClicked);
            RemoveButtonListener(confirmCloseButton, HandleCloseClicked);
            RemoveButtonListener(confirmAcknowledgeButton, HandleAcknowledgeClicked);
            entryButtonActions = Array.Empty<UnityAction>();
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

        private void HandleOpenClicked()
        {
            OpenDrawer();
        }

        private void HandleBackClicked()
        {
            NavigateBack();
        }

        private void HandleCloseClicked()
        {
            Close();
        }

        private void HandleReviewCtaClicked()
        {
            OpenReviewConfirm();
        }

        private void HandleAcknowledgeClicked()
        {
            AcknowledgeReview();
        }

        private void ApplyCurrentView()
        {
            DisableAllNavigation();
            ClearDetailView();
            ClearConfirmationView();
            RefreshEntryBindings();

            if (session == null)
            {
                RenderUnavailableClosed();
                ShowOnly(LobbyOperationsReviewPanel.Closed);
                return;
            }

            switch (session.Phase)
            {
                case LobbyOperationsReviewPhase.Directory:
                    RenderDirectory();
                    ShowOnly(LobbyOperationsReviewPanel.Directory);
                    break;
                case LobbyOperationsReviewPhase.EntryDetail:
                    RenderDetail();
                    ShowOnly(LobbyOperationsReviewPanel.Detail);
                    break;
                case LobbyOperationsReviewPhase.ReviewConfirm:
                    RenderConfirmation();
                    ShowOnly(LobbyOperationsReviewPanel.Confirm);
                    break;
                default:
                    RenderClosed();
                    ShowOnly(LobbyOperationsReviewPanel.Closed);
                    break;
            }
        }

        private void RenderUnavailableClosed()
        {
            SetText(closedReviewLabelText, "OPERATIONS REVIEW UNAVAILABLE");
            SetText(closedStatusText, "REVIEW PROFILE INVALID OR MISSING");
            SetButtonInteractable(closedOpenButton, false);
        }

        private void RenderClosed()
        {
            SetText(closedReviewLabelText, "OPERATIONS REVIEW");
            SetText(closedStatusText, ClosedReviewStatus);
            SetButtonInteractable(closedOpenButton, true);
        }

        private void RenderDirectory()
        {
            SetText(directoryTitleText, "OPERATIONS DIRECTORY");
            SetText(directoryStatusText, DirectoryReviewStatus);
            SetButtonInteractable(directoryBackButton, true);
            SetButtonInteractable(directoryCloseButton, true);
            RefreshEntryBindings();
        }

        private void RenderDetail()
        {
            LobbyOperationsReviewProfile.EntryDefinition entry = session?.SelectedEntry;
            if (entry == null)
            {
                CurrentDetailStatus = "ENTRY DETAIL UNAVAILABLE";
                SetText(detailStatusText, CurrentDetailStatus);
                SetButtonInteractable(detailBackButton, true);
                SetButtonInteractable(detailCloseButton, true);
                return;
            }

            CurrentDetailTitle = entry.TitleFallback ?? string.Empty;
            CurrentDetailExplanation = entry.ExplanationFallback ?? string.Empty;
            CurrentDetailStatus = ResolveDetailStatus(entry.Kind);

            SetText(detailKindText, ResolveEntryKindLabel(entry.Kind));
            SetText(detailTitleText, CurrentDetailTitle);
            SetText(detailExplanationText, CurrentDetailExplanation);
            SetText(detailStatusText, CurrentDetailStatus);
            RenderDispositionRows(entry);
            SetButtonInteractable(detailBackButton, true);
            SetButtonInteractable(detailCloseButton, true);

            bool ctaVisible = IsReviewCtaVisible;
            SetActive(detailReviewCtaButton?.gameObject, ctaVisible);
            SetButtonInteractable(detailReviewCtaButton, ctaVisible);
            SetButtonLabel(detailReviewCtaButton, "REVIEW THIS FIXTURE");
        }

        private void RenderConfirmation()
        {
            LobbyOperationsReviewProfile.EntryDefinition entry = session?.SelectedEntry;
            bool validNotice = entry != null
                && entry.Kind == LobbyOperationsReviewEntryKind.Notice
                && string.Equals(
                    entry.EntryId,
                    LobbyOperationsReviewProfile.NoticeEntryId,
                    StringComparison.Ordinal)
                && entry.ActionDisposition
                    == LobbyOperationsReviewActionDisposition.LocalReviewConfirm;

            SetText(confirmTitleText, validNotice ? entry.TitleFallback : string.Empty);
            SetText(confirmSummaryText, validNotice ? ConfirmSummary : string.Empty);
            SetText(
                confirmStatusText,
                !validNotice
                    ? "REVIEW CONFIRMATION UNAVAILABLE"
                    : session.IsReviewAcknowledged
                        ? ConfirmedReviewStatus
                        : ConfirmReadyStatus);
            SetButtonInteractable(confirmBackButton, true);
            SetButtonInteractable(confirmCloseButton, true);
            SetButtonInteractable(
                confirmAcknowledgeButton,
                validNotice && !session.IsReviewAcknowledged);
            SetButtonLabel(
                confirmAcknowledgeButton,
                session.IsReviewAcknowledged
                    ? "REVIEW ACKNOWLEDGED"
                    : "ACKNOWLEDGE REVIEW");
        }

        private void RefreshEntryBindings()
        {
            EntryButtonBinding[] bindings = entryBindings
                ?? Array.Empty<EntryButtonBinding>();
            bool exactBindings = ValidateExactEntryBindings();
            for (int index = 0; index < bindings.Length; index++)
            {
                EntryButtonBinding binding = bindings[index];
                if (binding == null)
                {
                    continue;
                }

                LobbyOperationsReviewProfile.EntryDefinition entry = null;
                bool found = exactBindings
                    && session != null
                    && session.TryGetEntry(binding.EntryId, out entry);
                bool interactive = found
                    && session.Phase == LobbyOperationsReviewPhase.Directory;
                SetCanvasGroup(binding.CanvasGroup, found, interactive);
                SetButtonInteractable(binding.Button, interactive);
                SetText(binding.TitleText, found ? entry.TitleFallback : string.Empty);
                SetText(
                    binding.SourceStatusText,
                    found ? ResolveSourceStatus(entry.Kind) : string.Empty);
            }
        }

        private void RenderDispositionRows(
            LobbyOperationsReviewProfile.EntryDefinition entry)
        {
            if (entry == null || !ValidateExactDispositionRows())
            {
                return;
            }

            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.Production,
                entry.ProductionDisposition);
            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.Service,
                entry.ServiceDisposition);
            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.Account,
                entry.AccountDisposition);
            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.ServerClock,
                entry.ServerClockDisposition);
            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.Schedule,
                entry.ScheduleDisposition);
            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.Progress,
                entry.ProgressDisposition);
            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.Attention,
                entry.AttentionDisposition);
            ApplyDispositionRow(
                LobbyOperationsReviewDispositionRowKind.Action,
                entry.ActionDisposition);
        }

        private void ApplyDispositionRow(
            LobbyOperationsReviewDispositionRowKind rowKind,
            Enum disposition)
        {
            DispositionRowBinding binding = FindDispositionRow(rowKind);
            if (binding == null)
            {
                return;
            }

            SetRowVisible(binding, true);
            SetText(binding.LabelText, ResolveDispositionRowLabel(rowKind));
            SetText(binding.ValueText, ResolveDispositionValueLabel(disposition));
        }

        private DispositionRowBinding FindDispositionRow(
            LobbyOperationsReviewDispositionRowKind rowKind)
        {
            DispositionRowBinding[] rows = dispositionRows
                ?? Array.Empty<DispositionRowBinding>();
            for (int index = 0; index < rows.Length; index++)
            {
                DispositionRowBinding row = rows[index];
                if (row != null && row.RowKind == rowKind)
                {
                    return row;
                }
            }

            return null;
        }

        private void ClearDetailView()
        {
            CurrentDetailTitle = string.Empty;
            CurrentDetailExplanation = string.Empty;
            CurrentDetailStatus = string.Empty;
            SetText(detailKindText, string.Empty);
            SetText(detailTitleText, string.Empty);
            SetText(detailExplanationText, string.Empty);
            SetText(detailStatusText, string.Empty);

            DispositionRowBinding[] rows = dispositionRows
                ?? Array.Empty<DispositionRowBinding>();
            for (int index = 0; index < rows.Length; index++)
            {
                DispositionRowBinding row = rows[index];
                if (row == null)
                {
                    continue;
                }

                SetText(row.LabelText, string.Empty);
                SetText(row.ValueText, string.Empty);
                SetRowVisible(row, false);
            }

            SetActive(detailReviewCtaButton?.gameObject, false);
            SetButtonInteractable(detailReviewCtaButton, false);
        }

        private void ClearConfirmationView()
        {
            SetText(confirmTitleText, string.Empty);
            SetText(confirmSummaryText, string.Empty);
            SetText(confirmStatusText, string.Empty);
            SetButtonInteractable(confirmAcknowledgeButton, false);
        }

        private bool CanOpenReviewConfirm()
        {
            if (session == null
                || session.Phase != LobbyOperationsReviewPhase.EntryDetail
                || session.IsReviewAcknowledged)
            {
                return false;
            }

            LobbyOperationsReviewProfile.EntryDefinition entry = session.SelectedEntry;
            return entry != null
                && entry.Kind == LobbyOperationsReviewEntryKind.Notice
                && string.Equals(
                    entry.EntryId,
                    LobbyOperationsReviewProfile.NoticeEntryId,
                    StringComparison.Ordinal)
                && entry.ActionDisposition
                    == LobbyOperationsReviewActionDisposition.LocalReviewConfirm;
        }

        private bool ValidateExactEntryBindings()
        {
            EntryButtonBinding[] bindings = entryBindings
                ?? Array.Empty<EntryButtonBinding>();
            if (bindings.Length != LobbyOperationsReviewProfile.RequiredEntryCount)
            {
                return false;
            }

            for (int index = 0; index < bindings.Length; index++)
            {
                EntryButtonBinding binding = bindings[index];
                if (binding?.Button == null
                    || binding.CanvasGroup == null
                    || binding.TitleText == null
                    || binding.SourceStatusText == null
                    || !string.Equals(
                        binding.EntryId,
                        GetRequiredEntryId(index),
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateExactDispositionRows()
        {
            DispositionRowBinding[] rows = dispositionRows
                ?? Array.Empty<DispositionRowBinding>();
            if (rows.Length != RequiredDispositionRows.Length)
            {
                return false;
            }

            for (int index = 0; index < rows.Length; index++)
            {
                DispositionRowBinding row = rows[index];
                if (row == null
                    || row.RowKind != RequiredDispositionRows[index]
                    || row.RowRoot == null
                    || row.LabelText == null
                    || row.ValueText == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetRequiredEntryId(int index)
        {
            return index switch
            {
                0 => LobbyOperationsReviewProfile.NoticeEntryId,
                1 => LobbyOperationsReviewProfile.MailboxEntryId,
                2 => LobbyOperationsReviewProfile.MissionsEntryId,
                3 => LobbyOperationsReviewProfile.EventCalendarEntryId,
                _ => string.Empty
            };
        }

        private static string ResolveSourceStatus(
            LobbyOperationsReviewEntryKind kind)
        {
            return kind switch
            {
                LobbyOperationsReviewEntryKind.Notice => NoticeSourceStatus,
                LobbyOperationsReviewEntryKind.Mailbox => MailboxSourceStatus,
                LobbyOperationsReviewEntryKind.Missions => MissionsSourceStatus,
                LobbyOperationsReviewEntryKind.EventCalendar => EventCalendarSourceStatus,
                _ => string.Empty
            };
        }

        private static string ResolveEntryKindLabel(
            LobbyOperationsReviewEntryKind kind)
        {
            return kind switch
            {
                LobbyOperationsReviewEntryKind.Notice => "NOTICE",
                LobbyOperationsReviewEntryKind.Mailbox => "MAILBOX",
                LobbyOperationsReviewEntryKind.Missions => "MISSIONS",
                LobbyOperationsReviewEntryKind.EventCalendar => "EVENT CALENDAR",
                _ => string.Empty
            };
        }

        private static string ResolveDetailStatus(
            LobbyOperationsReviewEntryKind kind)
        {
            return kind switch
            {
                LobbyOperationsReviewEntryKind.Notice => NoticeDetailStatus,
                LobbyOperationsReviewEntryKind.Mailbox => MailboxDetailStatus,
                LobbyOperationsReviewEntryKind.Missions => MissionsDetailStatus,
                LobbyOperationsReviewEntryKind.EventCalendar =>
                    EventCalendarDetailStatus,
                _ => string.Empty
            };
        }

        private static string ResolveDispositionRowLabel(
            LobbyOperationsReviewDispositionRowKind rowKind)
        {
            return rowKind switch
            {
                LobbyOperationsReviewDispositionRowKind.Production => "PRODUCTION",
                LobbyOperationsReviewDispositionRowKind.Service => "SERVICE",
                LobbyOperationsReviewDispositionRowKind.Account => "ACCOUNT",
                LobbyOperationsReviewDispositionRowKind.ServerClock => "SERVER CLOCK",
                LobbyOperationsReviewDispositionRowKind.Schedule => "SCHEDULE",
                LobbyOperationsReviewDispositionRowKind.Progress => "PROGRESS",
                LobbyOperationsReviewDispositionRowKind.Attention => "ATTENTION",
                LobbyOperationsReviewDispositionRowKind.Action => "ACTION",
                _ => string.Empty
            };
        }

        private static string ResolveDispositionValueLabel(Enum disposition)
        {
            if (disposition == null)
            {
                return string.Empty;
            }

            return disposition.ToString() switch
            {
                "LocalReviewFixture" => "LOCAL REVIEW FIXTURE",
                "DefinitionOnlyReviewShell" => "DEFINITION-ONLY REVIEW SHELL",
                "ReviewShellNoProductCommitment" =>
                    "REVIEW SHELL / NO PRODUCT COMMITMENT",
                "NotRequiredForReview" => "NOT REQUIRED FOR REVIEW",
                "NoVerifiedSource" => "NO VERIFIED SOURCE",
                "DefinitionOnlyNoVerdict" => "DEFINITION ONLY / NO VERDICT",
                "LocalReviewConfirm" => "LOCAL REVIEW CONFIRM",
                "ExplanationOnly" => "EXPLANATION ONLY",
                _ => string.Empty
            };
        }

        private void DisableAllNavigation()
        {
            SetButtonInteractable(closedOpenButton, false);
            SetButtonInteractable(directoryBackButton, false);
            SetButtonInteractable(directoryCloseButton, false);
            SetButtonInteractable(detailBackButton, false);
            SetButtonInteractable(detailCloseButton, false);
            SetButtonInteractable(detailReviewCtaButton, false);
            SetButtonInteractable(confirmBackButton, false);
            SetButtonInteractable(confirmCloseButton, false);
            SetButtonInteractable(confirmAcknowledgeButton, false);
        }

        private void ShowOnly(LobbyOperationsReviewPanel panel)
        {
            SetCanvasGroup(
                closedPanel,
                panel == LobbyOperationsReviewPanel.Closed,
                panel == LobbyOperationsReviewPanel.Closed);
            SetCanvasGroup(
                directoryPanel,
                panel == LobbyOperationsReviewPanel.Directory,
                panel == LobbyOperationsReviewPanel.Directory);
            SetCanvasGroup(
                detailPanel,
                panel == LobbyOperationsReviewPanel.Detail,
                panel == LobbyOperationsReviewPanel.Detail);
            SetCanvasGroup(
                confirmPanel,
                panel == LobbyOperationsReviewPanel.Confirm,
                panel == LobbyOperationsReviewPanel.Confirm);

            if (CurrentPanel != panel)
            {
                CurrentPanel = panel;
                PanelChanged?.Invoke(panel);
            }

            SelectDefaultControl(panel);
        }

        private void SelectDefaultControl(LobbyOperationsReviewPanel panel)
        {
            Button target = panel switch
            {
                LobbyOperationsReviewPanel.Closed => closedOpenButton,
                LobbyOperationsReviewPanel.Directory => FindFirstEntryButton(),
                LobbyOperationsReviewPanel.Detail => detailBackButton,
                LobbyOperationsReviewPanel.Confirm =>
                    IsUsableButton(confirmAcknowledgeButton)
                        ? confirmAcknowledgeButton
                        : confirmBackButton,
                _ => null
            };

            LastFocusTarget = IsUsableButton(target) ? target : null;
            if (!Application.isPlaying
                || EventSystem.current == null
                || LastFocusTarget == null
                || EventSystem.current.currentSelectedGameObject
                    == LastFocusTarget.gameObject)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(LastFocusTarget.gameObject);
        }

        private Button FindFirstEntryButton()
        {
            EntryButtonBinding[] bindings = entryBindings
                ?? Array.Empty<EntryButtonBinding>();
            for (int index = 0; index < bindings.Length; index++)
            {
                Button button = bindings[index]?.Button;
                if (IsUsableButton(button))
                {
                    return button;
                }
            }

            return directoryBackButton;
        }

        private static bool IsUsableButton(Button button)
        {
            return button != null
                && button.gameObject.activeInHierarchy
                && button.interactable;
        }

        private static void SetRowVisible(
            DispositionRowBinding binding,
            bool visible)
        {
            if (binding == null)
            {
                return;
            }

            if (binding.RowRoot != null)
            {
                SetActive(binding.RowRoot, visible);
                return;
            }

            SetActive(binding.LabelText?.gameObject, visible);
            SetActive(binding.ValueText?.gameObject, visible);
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

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
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
    }
}
