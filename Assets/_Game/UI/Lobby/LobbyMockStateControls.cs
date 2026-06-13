using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyMockStateControls : MonoBehaviour
    {
        [SerializeField] private LobbyScreenPresenter presenter;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button returnedButton;
        [SerializeField] private Button rewardButton;
        [SerializeField] private Button summonReadyButton;

        private void OnEnable()
        {
            AddListeners();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        public void ShowNormal()
        {
            presenter?.SetMockState(LobbyMockState.Normal);
        }

        public void ShowReturnedFromCombat()
        {
            presenter?.SetMockState(LobbyMockState.ReturnedFromCombat);
        }

        public void ShowNewReward()
        {
            presenter?.SetMockState(LobbyMockState.NewReward);
        }

        public void ShowSummonReady()
        {
            presenter?.SetMockState(LobbyMockState.SummonReady);
        }

        private void AddListeners()
        {
            if (normalButton != null)
            {
                normalButton.onClick.AddListener(ShowNormal);
            }

            if (returnedButton != null)
            {
                returnedButton.onClick.AddListener(ShowReturnedFromCombat);
            }

            if (rewardButton != null)
            {
                rewardButton.onClick.AddListener(ShowNewReward);
            }

            if (summonReadyButton != null)
            {
                summonReadyButton.onClick.AddListener(ShowSummonReady);
            }
        }

        private void RemoveListeners()
        {
            if (normalButton != null)
            {
                normalButton.onClick.RemoveListener(ShowNormal);
            }

            if (returnedButton != null)
            {
                returnedButton.onClick.RemoveListener(ShowReturnedFromCombat);
            }

            if (rewardButton != null)
            {
                rewardButton.onClick.RemoveListener(ShowNewReward);
            }

            if (summonReadyButton != null)
            {
                summonReadyButton.onClick.RemoveListener(ShowSummonReady);
            }
        }
    }
}
