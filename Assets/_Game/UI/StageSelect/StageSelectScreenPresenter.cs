using System;
using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class StageSelectScreenPresenter : MonoBehaviour
    {
        [Serializable]
        private struct StageFocusEntry
        {
            [SerializeField] private string stageId;
            [SerializeField] private Button selectionButton;
            [SerializeField] private RectTransform stageTarget;
            [SerializeField] private RectTransform chapterTarget;

            public string StageId => stageId;
            public Button SelectionButton => selectionButton;
            public RectTransform StageTarget => stageTarget;
            public RectTransform ChapterTarget => chapterTarget;
        }

        private sealed class SelectedRouteBundle
        {
            public SelectedRouteBundle(UIStageRouteProjection projection)
            {
                Projection = projection;
                CatalogProjectionGeneration = projection.CatalogProjectionGeneration;
                CanonicalProjectionDigest = projection.CanonicalProjectionDigest;
                PlayableStage = projection.PlayableStage;
                StageTemplate = projection.StageTemplate;
                CanonicalReferenceDigest = projection.CanonicalReferenceDigest;
                CanonicalTemplateDigest = projection.CanonicalTemplateDigest;
                CanonicalBriefingDigest = projection.CanonicalBriefingDigest;
                Briefing = projection.Briefing;
                ResultProgressionJoinPreflight = projection.ResultProgressionJoinPreflight;
            }

            public UIStageRouteProjection Projection { get; }
            public int CatalogProjectionGeneration { get; }
            public string CanonicalProjectionDigest { get; }
            public PlayableStageDefinition PlayableStage { get; }
            public LinearStageTemplateProfile StageTemplate { get; }
            public string CanonicalReferenceDigest { get; }
            public string CanonicalTemplateDigest { get; }
            public string CanonicalBriefingDigest { get; }
            public StageBriefingReadModel Briefing { get; }
            public StageRunResultProgressionJoinSnapshot ResultProgressionJoinPreflight { get; }
            public bool RequestAccepted { get; set; }
        }

        [SerializeField] private UIStageCatalog stageCatalog;
        [SerializeField] private string selectedStageId;
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Text combatLessonText;
        [SerializeField] private Text threatTagsText;
        [SerializeField] private Text summonHintText;
        [SerializeField] private Text rewardPreviewText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private UIRouteId startRoute = UIRouteId.Combat;
        [SerializeField] private UIRouteId backRoute = UIRouteId.Lobby;
        [SerializeField] private UnityEvent startRequested = new UnityEvent();
        [SerializeField] private UnityEvent backRequested = new UnityEvent();
        [SerializeField] private UIScrollRectMotionPresenter stageScrollMotion;
        [SerializeField] private UIScrollRectMotionPresenter chapterScrollMotion;
        [SerializeField] private StageFocusEntry[] stageFocusEntries = Array.Empty<StageFocusEntry>();
        [SerializeField] private bool requireExactStageCardBindings;
        [SerializeField] private bool focusSelectedStageOnEnable = true;
        [SerializeField] private bool backWithEscape = true;
        [SerializeField, Min(0f)] private float focusDelaySeconds = 0.02f;
        [SerializeField, Min(0f)] private float initialFocusDurationSeconds = 0.18f;
        [SerializeField, Min(0f)] private float selectedFocusDurationSeconds = 0.3f;
        [Header("Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip sceneEnterSfx;
        [SerializeField] private AudioClip startButtonSfx;
        [SerializeField] private AudioClip backButtonSfx;
        [SerializeField, Range(0f, 1f)] private float sceneEnterSfxVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float startButtonSfxVolume = 0.9f;
        [SerializeField, Range(0f, 1f)] private float backButtonSfxVolume = 0.8f;

        private Coroutine focusRoutine;
        private bool sceneEnterSfxPlayed;
        private SelectedRouteBundle selectedRouteBundle;
        private UIStageRouteProjectionRejectReason selectedRouteRejectReason;
        private Button[] boundStageSelectionButtons = Array.Empty<Button>();
        private UnityAction[] boundStageSelectionActions = Array.Empty<UnityAction>();

        public bool HasSelectedRouteProjection => selectedRouteBundle != null;
        public UIStageRouteProjection SelectedRouteProjection => selectedRouteBundle?.Projection;
        public UIStageRouteProjectionRejectReason SelectedRouteRejectReason => selectedRouteRejectReason;
        public bool HasAcceptedStartRequest => selectedRouteBundle?.RequestAccepted == true;

        private void OnEnable()
        {
            bool stageBindingsValid = BindStageSelectionListeners();
            if (requireExactStageCardBindings && !stageBindingsValid)
            {
                RejectSelectedRoute(
                    UIStageRouteProjectionRejectReason.InvalidStageSelectionBindings);
            }
            else
            {
                ApplySelectedStage();
            }

            PlaySceneEnterSfxOnce();

            if (startButton != null)
            {
                startButton.onClick.AddListener(HandleStartClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }

            if (focusSelectedStageOnEnable && selectedRouteBundle != null)
            {
                QueueSelectedStageFocus(false);
            }
        }

        private void OnDisable()
        {
            InvalidateSelectedRouteBundle(UIStageRouteProjectionRejectReason.None);
            RemoveStageSelectionListeners();

            if (focusRoutine != null)
            {
                StopCoroutine(focusRoutine);
                focusRoutine = null;
            }

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackClicked);
            }
        }

        private void Update()
        {
            if (backWithEscape && WasBackPressedThisFrame())
            {
                HandleBackClicked();
            }
        }

        public void SelectStage(string stageId)
        {
            InvalidateSelectedRouteBundle(UIStageRouteProjectionRejectReason.None);
            if (string.IsNullOrWhiteSpace(stageId))
            {
                selectedStageId = string.Empty;
                selectedRouteRejectReason = UIStageRouteProjectionRejectReason.MissingCatalogEntryId;
                ClearStageDetails();
                SetText(statusText, "Stage route unavailable");
                SetStartInteractable(false);
                return;
            }

            selectedStageId = stageId;
            ApplySelectedStage();
            QueueSelectedStageFocus(true);
        }

        public void HandleStartClicked()
        {
            SelectedRouteBundle bundle = selectedRouteBundle;
            if (bundle?.RequestAccepted == true || router == null || router.IsRouting)
            {
                return;
            }

            if (bundle == null)
            {
                SetText(statusText, "Stage route unavailable");
                return;
            }

            UIStageRouteProjection projection = bundle.Projection;
            if (projection == null
                || projection.CatalogProjectionGeneration != bundle.CatalogProjectionGeneration
                || !string.Equals(
                    projection.CanonicalProjectionDigest,
                    bundle.CanonicalProjectionDigest,
                    StringComparison.Ordinal)
                || !ReferenceEquals(projection.PlayableStage, bundle.PlayableStage)
                || !ReferenceEquals(projection.StageTemplate, bundle.StageTemplate)
                || !ReferenceEquals(projection.Briefing, bundle.Briefing)
                || !ReferenceEquals(
                    projection.ResultProgressionJoinPreflight,
                    bundle.ResultProgressionJoinPreflight)
                || projection.ResultProgressionJoinPreflight == null
                || !projection.ResultProgressionJoinPreflight.TryValidateIntegrity(out _)
                || !string.Equals(
                    projection.CanonicalReferenceDigest,
                    bundle.CanonicalReferenceDigest,
                    StringComparison.Ordinal)
                || !string.Equals(
                    projection.CanonicalTemplateDigest,
                    bundle.CanonicalTemplateDigest,
                    StringComparison.Ordinal)
                || !string.Equals(
                    projection.CanonicalBriefingDigest,
                    bundle.CanonicalBriefingDigest,
                    StringComparison.Ordinal)
                || projection.Briefing == null
                || !string.Equals(
                    projection.Briefing.CanonicalBriefingDigest,
                    bundle.CanonicalBriefingDigest,
                    StringComparison.Ordinal)
                || !string.Equals(
                    selectedStageId,
                    projection.CatalogEntryId,
                    StringComparison.Ordinal))
            {
                RejectSelectedRoute(UIStageRouteProjectionRejectReason.StaleProjectionBundle);
                return;
            }

            UIStageRouteProjectionRejectReason rejectReason =
                UIStageRouteProjectionRejectReason.SourceObjectMismatch;
            if (stageCatalog == null
                || !stageCatalog.IsProjectionCurrent(
                    projection,
                    startRoute,
                    out rejectReason))
            {
                RejectSelectedRoute(
                    stageCatalog == null
                        ? UIStageRouteProjectionRejectReason.SourceObjectMismatch
                        : rejectReason);
                return;
            }

            bool accepted = router.RequestRouteWithScene(
                projection.UiRouteId,
                projection.EntrySceneName,
                projection.EntryScenePath,
                projection.LoadingCardId);
            if (!accepted)
            {
                SetText(statusText, "Stage route unavailable");
                return;
            }

            bundle.RequestAccepted = true;
            SetStartInteractable(false);
            SetText(statusText, string.Empty);
            startRequested.Invoke();
            PlayOneShot(startButtonSfx, startButtonSfxVolume);
        }

        public void HandleBackClicked()
        {
            if (router != null && router.IsRouting)
            {
                return;
            }

            backRequested.Invoke();
            PlayOneShot(backButtonSfx, backButtonSfxVolume);

            if (router != null)
            {
                router.RequestRoute(backRoute);
            }
        }

        private void ApplySelectedStage()
        {
            InvalidateSelectedRouteBundle(UIStageRouteProjectionRejectReason.None);

            if (requireExactStageCardBindings && !TryValidateStageCardBindings())
            {
                selectedRouteRejectReason =
                    UIStageRouteProjectionRejectReason.InvalidStageSelectionBindings;
                ClearStageDetails();
                SetText(statusText, "Stage route unavailable");
                SetStartInteractable(false);
                return;
            }

            bool projected = false;
            UIStageRouteProjection projection = null;
            UIStageRouteProjectionRejectReason rejectReason =
                UIStageRouteProjectionRejectReason.CatalogEntryNotFound;
            if (stageCatalog != null)
            {
                projected = string.IsNullOrWhiteSpace(selectedStageId)
                    ? stageCatalog.TryCreateFirstRouteProjection(
                        startRoute,
                        out projection,
                        out rejectReason)
                    : stageCatalog.TryCreateRouteProjection(
                        selectedStageId,
                        startRoute,
                        out projection,
                        out rejectReason);
            }

            if (projected)
            {
                selectedStageId = projection.CatalogEntryId;
                selectedRouteBundle = new SelectedRouteBundle(projection);
                selectedRouteRejectReason = UIStageRouteProjectionRejectReason.None;
                StageBriefingReadModel briefing = projection.Briefing;
                SetText(stageNameText, briefing.Title);
                SetText(summaryText, briefing.Objective);
                SetOptionalText(combatLessonText, briefing.CombatLesson);
                SetOptionalText(threatTagsText, projection.ThreatTags);
                SetOptionalText(summonHintText, projection.RecommendedSummonRole);
                SetOptionalText(rewardPreviewText, projection.RewardPreview);
                SetText(statusText, string.Empty);
                SetStartInteractable(true);
                return;
            }

            selectedRouteRejectReason = stageCatalog == null
                ? UIStageRouteProjectionRejectReason.SourceObjectMismatch
                : rejectReason;
            ClearStageDetails();
            SetText(statusText, "Stage route unavailable");
            SetStartInteractable(false);
        }

        private void InvalidateSelectedRouteBundle(
            UIStageRouteProjectionRejectReason rejectReason)
        {
            selectedRouteBundle = null;
            selectedRouteRejectReason = rejectReason;
        }

        private void RejectSelectedRoute(UIStageRouteProjectionRejectReason rejectReason)
        {
            InvalidateSelectedRouteBundle(rejectReason);
            SetText(statusText, "Stage route unavailable");
            SetStartInteractable(false);
        }

        private void ClearStageDetails()
        {
            SetText(stageNameText, string.Empty);
            SetText(summaryText, string.Empty);
            SetOptionalText(combatLessonText, string.Empty);
            SetOptionalText(threatTagsText, string.Empty);
            SetOptionalText(summonHintText, string.Empty);
            SetOptionalText(rewardPreviewText, string.Empty);
        }

        private void SetStartInteractable(bool interactable)
        {
            if (startButton != null)
            {
                startButton.interactable = interactable;
            }
        }

        private bool BindStageSelectionListeners()
        {
            RemoveStageSelectionListeners();
            if (!TryValidateStageCardBindings())
            {
                return false;
            }

            if (stageFocusEntries == null || stageFocusEntries.Length == 0)
            {
                return true;
            }

            boundStageSelectionButtons = new Button[stageFocusEntries.Length];
            boundStageSelectionActions = new UnityAction[stageFocusEntries.Length];
            for (int i = 0; i < stageFocusEntries.Length; i++)
            {
                StageFocusEntry entry = stageFocusEntries[i];
                string stageId = entry.StageId;
                Button button = entry.SelectionButton;
                UnityAction action = () => SelectStage(stageId);
                boundStageSelectionButtons[i] = button;
                boundStageSelectionActions[i] = action;
                button.onClick.AddListener(action);
            }

            return true;
        }

        private void RemoveStageSelectionListeners()
        {
            int count = Math.Min(
                boundStageSelectionButtons?.Length ?? 0,
                boundStageSelectionActions?.Length ?? 0);
            for (int i = 0; i < count; i++)
            {
                Button button = boundStageSelectionButtons[i];
                UnityAction action = boundStageSelectionActions[i];
                if (button != null && action != null)
                {
                    button.onClick.RemoveListener(action);
                }
            }

            boundStageSelectionButtons = Array.Empty<Button>();
            boundStageSelectionActions = Array.Empty<UnityAction>();
        }

        private bool TryValidateStageCardBindings()
        {
            if (stageFocusEntries == null || stageFocusEntries.Length == 0)
            {
                return !requireExactStageCardBindings;
            }

            if (stageCatalog == null
                || !stageCatalog.TryValidateEntryIdentities(out _)
                || (requireExactStageCardBindings
                    && stageFocusEntries.Length != stageCatalog.StageCount))
            {
                return false;
            }

            var stageIds = new HashSet<string>(StringComparer.Ordinal);
            var buttons = new HashSet<Button>();
            for (int i = 0; i < stageFocusEntries.Length; i++)
            {
                StageFocusEntry entry = stageFocusEntries[i];
                if (string.IsNullOrWhiteSpace(entry.StageId)
                    || entry.SelectionButton == null
                    || entry.StageTarget == null
                    || entry.SelectionButton.transform != entry.StageTarget
                    || !stageIds.Add(entry.StageId)
                    || !buttons.Add(entry.SelectionButton)
                    || !stageCatalog.TryGetStage(entry.StageId, out _))
                {
                    return false;
                }
            }

            if (!requireExactStageCardBindings)
            {
                return true;
            }

            for (int i = 0; i < stageCatalog.StageCount; i++)
            {
                if (!stageIds.Contains(stageCatalog.GetStage(i).Id))
                {
                    return false;
                }
            }

            return true;
        }

        private void QueueSelectedStageFocus(bool animate)
        {
            if (focusRoutine != null)
            {
                StopCoroutine(focusRoutine);
            }

            focusRoutine = StartCoroutine(FocusSelectedStageRoutine(animate));
        }

        private IEnumerator FocusSelectedStageRoutine(bool animate)
        {
            yield return null;

            if (focusDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(focusDelaySeconds);
            }

            FocusSelectedStage(animate);
            focusRoutine = null;
        }

        private void FocusSelectedStage(bool animate)
        {
            if (!TryGetFocusEntry(out StageFocusEntry focusEntry))
            {
                return;
            }

            float duration = animate ? selectedFocusDurationSeconds : initialFocusDurationSeconds;
            if (stageScrollMotion != null && focusEntry.StageTarget != null)
            {
                stageScrollMotion.FocusTarget(focusEntry.StageTarget, duration);
            }

            if (chapterScrollMotion != null && focusEntry.ChapterTarget != null)
            {
                chapterScrollMotion.FocusTarget(focusEntry.ChapterTarget, duration);
            }
        }

        private bool TryGetFocusEntry(out StageFocusEntry focusEntry)
        {
            for (int i = 0; i < stageFocusEntries.Length; i++)
            {
                StageFocusEntry entry = stageFocusEntries[i];
                if (string.Equals(entry.StageId, selectedStageId, StringComparison.Ordinal))
                {
                    focusEntry = entry;
                    return true;
                }
            }

            focusEntry = default;
            return false;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static void SetOptionalText(Text target, string value)
        {
            if (target == null)
            {
                return;
            }

            bool hasValue = !string.IsNullOrWhiteSpace(value);
            target.text = hasValue ? value : string.Empty;
            target.gameObject.SetActive(hasValue);
        }

        private static bool WasBackPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        private void PlaySceneEnterSfxOnce()
        {
            if (sceneEnterSfxPlayed)
            {
                return;
            }

            sceneEnterSfxPlayed = true;
            PlayOneShot(sceneEnterSfx, sceneEnterSfxVolume);
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource source = ResolveUiAudioSource();
            if (source == null)
            {
                return;
            }

            source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private AudioSource ResolveUiAudioSource()
        {
            if (uiAudioSource != null)
            {
                return uiAudioSource;
            }

            uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
            }

            uiAudioSource.playOnAwake = false;
            uiAudioSource.loop = false;
            uiAudioSource.spatialBlend = 0f;
            uiAudioSource.priority = 32;
            return uiAudioSource;
        }
    }
}
