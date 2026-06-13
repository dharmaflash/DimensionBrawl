using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class StageSelectScreenPresenter : MonoBehaviour
    {
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
        }

        private void OnDisable()
        {
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

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
