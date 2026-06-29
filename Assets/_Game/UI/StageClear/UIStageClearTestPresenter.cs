using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI.StageClear
{
    [DisallowMultipleComponent]
    public sealed class UIStageClearTestPresenter : MonoBehaviour
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextStageButton;

        public int RetryClickCount { get; private set; }
        public int NextStageClickCount { get; private set; }

        private void OnEnable()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(HandleRetryClicked);
            }

            if (nextStageButton != null)
            {
                nextStageButton.onClick.AddListener(HandleNextStageClicked);
            }
        }

        private void OnDisable()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(HandleRetryClicked);
            }

            if (nextStageButton != null)
            {
                nextStageButton.onClick.RemoveListener(HandleNextStageClicked);
            }
        }

        private void HandleRetryClicked()
        {
            RetryClickCount++;
            Debug.Log("Stage clear retry button clicked.");
        }

        private void HandleNextStageClicked()
        {
            NextStageClickCount++;
            Debug.Log("Stage clear next stage button clicked.");
        }
    }
}
