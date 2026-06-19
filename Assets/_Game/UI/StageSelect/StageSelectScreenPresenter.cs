using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
        [SerializeField] private string selectedStageId = "story_v1_training_route";
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

        public void SelectStage(string stageId)
        {
            selectedStageId = stageId;
            ApplySelectedStage();
            QueueSelectedStageFocus(true);
        }

        public void HandleStartClicked()
        {
            startRequested.Invoke();

            if (router != null)
            {
                router.RequestRoute(startRoute);
            }
        }

        public void HandleBackClicked()
        {
            backRequested.Invoke();

            if (router != null)
            {
                router.RequestRoute(backRoute);
            }
        }

        private void ApplySelectedStage()
        {
            if (stageCatalog != null && stageCatalog.TryGetStage(selectedStageId, out UIStageCatalog.StageEntry stage))
            {
                SetText(stageNameText, stage.DisplayName);
                SetText(summaryText, stage.Summary);
                SetText(threatTagsText, stage.ThreatTags);
                SetText(summonHintText, stage.RecommendedSummonRole);
                SetText(rewardPreviewText, stage.MockRewardPreview);
                SetText(statusText, "Mission prep UI only");
                return;
            }

            SetText(stageNameText, "Story V1 Training Route");
            SetText(summaryText, "A UI-only mission prep placeholder before the combat HUD test.");
            SetText(threatTagsText, "Threat: Basic soldier pressure");
            SetText(summonHintText, "Summon role hint: visual placeholder");
            SetText(rewardPreviewText, "Reward preview: disabled");
            SetText(statusText, "No stage catalog assigned");
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
    }
}
