using System;
using System.Collections;
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
            [SerializeField] private RectTransform stageTarget;
            [SerializeField] private RectTransform chapterTarget;

            public string StageId => stageId;
            public RectTransform StageTarget => stageTarget;
            public RectTransform ChapterTarget => chapterTarget;
        }

        [SerializeField] private UIStageCatalog stageCatalog;
        [SerializeField] private string selectedStageId;
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Text threatTagsText;
        [SerializeField] private Text summonHintText;
        [SerializeField] private Text rewardPreviewText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private UIRouteId startRoute = UIRouteId.CombatHud;
        [SerializeField] private UIRouteId backRoute = UIRouteId.Lobby;
        [SerializeField] private UnityEvent startRequested = new UnityEvent();
        [SerializeField] private UnityEvent backRequested = new UnityEvent();
        [SerializeField] private UIScrollRectMotionPresenter stageScrollMotion;
        [SerializeField] private UIScrollRectMotionPresenter chapterScrollMotion;
        [SerializeField] private StageFocusEntry[] stageFocusEntries = Array.Empty<StageFocusEntry>();
        [SerializeField] private bool focusSelectedStageOnEnable = true;
        [SerializeField] private bool backWithEscape = true;
        [SerializeField, Min(0f)] private float focusDelaySeconds = 0.02f;
        [SerializeField, Min(0f)] private float initialFocusDurationSeconds = 0.18f;
        [SerializeField, Min(0f)] private float selectedFocusDurationSeconds = 0.3f;

        private Coroutine focusRoutine;

        private void OnEnable()
        {
            ApplySelectedStage();

            if (startButton != null)
            {
                startButton.onClick.AddListener(HandleStartClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }

            if (focusSelectedStageOnEnable)
            {
                QueueSelectedStageFocus(false);
            }
        }

        private void OnDisable()
        {
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
            if (string.IsNullOrWhiteSpace(stageId))
            {
                return;
            }

            selectedStageId = stageId;
            ApplySelectedStage();
            QueueSelectedStageFocus(true);
        }

        public void HandleStartClicked()
        {
            startRequested.Invoke();

            if (router != null)
            {
                if (TryResolveSelectedStage(out UIStageCatalog.StageEntry stage) && stage.HasSceneRoute)
                {
                    router.RequestRouteWithScene(
                        startRoute,
                        stage.SceneName,
                        stage.ScenePath,
                        stage.LoadingCardId);
                    return;
                }

                router.RequestRoute(startRoute);
            }
        }

        public void HandleBackClicked()
        {
            if (router != null && router.IsRouting)
            {
                return;
            }

            backRequested.Invoke();

            if (router != null)
            {
                router.RequestRoute(backRoute);
            }
        }

        private void ApplySelectedStage()
        {
            if (TryResolveSelectedStage(out UIStageCatalog.StageEntry stage))
            {
                SetText(stageNameText, stage.DisplayName);
                SetText(summaryText, stage.Summary);
                SetText(threatTagsText, stage.ThreatTags);
                SetText(summonHintText, stage.RecommendedSummonRole);
                SetText(rewardPreviewText, stage.MockRewardPreview);
                SetText(statusText, string.Empty);
                return;
            }

            ClearStageDetails();
        }

        private bool TryResolveSelectedStage(out UIStageCatalog.StageEntry stage)
        {
            if (stageCatalog == null)
            {
                stage = default;
                return false;
            }

            if (string.IsNullOrWhiteSpace(selectedStageId))
            {
                return stageCatalog.TryGetFirstStage(out stage);
            }

            return stageCatalog.TryGetStage(selectedStageId, out stage);
        }

        private void ClearStageDetails()
        {
            SetText(stageNameText, string.Empty);
            SetText(summaryText, string.Empty);
            SetText(threatTagsText, string.Empty);
            SetText(summonHintText, string.Empty);
            SetText(rewardPreviewText, string.Empty);
            SetText(statusText, string.Empty);
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
    }
}
