using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class StageSelectMockStageControls : MonoBehaviour
    {
        [SerializeField] private StageSelectScreenPresenter presenter;
        [SerializeField] private Button primaryStageButton;
        [SerializeField] private Button alternateStageButton;
        [SerializeField] private string primaryStageId = "story_v1_training_route";
        [SerializeField] private string alternateStageId = "story_v1_retry_route";

        private void OnEnable()
        {
            if (primaryStageButton != null)
            {
                primaryStageButton.onClick.AddListener(SelectPrimaryStage);
            }

            if (alternateStageButton != null)
            {
                alternateStageButton.onClick.AddListener(SelectAlternateStage);
            }
        }

        private void OnDisable()
        {
            if (primaryStageButton != null)
            {
                primaryStageButton.onClick.RemoveListener(SelectPrimaryStage);
            }

            if (alternateStageButton != null)
            {
                alternateStageButton.onClick.RemoveListener(SelectAlternateStage);
            }
        }

        public void SelectPrimaryStage()
        {
            presenter?.SelectStage(primaryStageId);
        }

        public void SelectAlternateStage()
        {
            presenter?.SelectStage(alternateStageId);
        }
    }
}
