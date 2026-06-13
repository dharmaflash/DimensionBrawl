using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    public enum LobbyMockState
    {
        Normal = 0,
        ReturnedFromCombat = 10,
        NewReward = 20,
        SummonReady = 30
    }

    [DisallowMultipleComponent]
    public sealed class LobbyScreenPresenter : MonoBehaviour
    {
        [SerializeField] private Text guideNameText;
        [SerializeField] private Text guideLineText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text primaryCtaText;
        [SerializeField] private Button primaryCtaButton;
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private UIRouteId primaryRoute = UIRouteId.StageSelect;
        [SerializeField] private LobbyMockState mockState = LobbyMockState.Normal;
        [SerializeField] private UnityEvent primaryCtaRequested = new UnityEvent();

        private void OnEnable()
        {
            ApplyMockState(mockState);

            if (primaryCtaButton != null)
            {
                primaryCtaButton.onClick.AddListener(HandlePrimaryCtaClicked);
            }
        }

        private void OnDisable()
        {
            if (primaryCtaButton != null)
            {
                primaryCtaButton.onClick.RemoveListener(HandlePrimaryCtaClicked);
            }
        }

        public void SetMockState(LobbyMockState state)
        {
            mockState = state;
            ApplyMockState(mockState);
        }

        public void HandlePrimaryCtaClicked()
        {
            primaryCtaRequested.Invoke();

            if (router != null)
            {
                router.RequestRoute(primaryRoute);
            }
        }

        private void ApplyMockState(LobbyMockState state)
        {
            SetText(guideNameText, "Guide Unit");
            SetText(primaryCtaText, "Story PvE");

            switch (state)
            {
                case LobbyMockState.ReturnedFromCombat:
                    SetText(guideLineText, "Combat readback complete. Ready for another route.");
                    SetText(statusText, "Return from combat");
                    break;
                case LobbyMockState.NewReward:
                    SetText(guideLineText, "Reward review placeholder is waiting.");
                    SetText(statusText, "New reward");
                    break;
                case LobbyMockState.SummonReady:
                    SetText(guideLineText, "Summon slot presentation is ready, behavior is locked.");
                    SetText(statusText, "Summon ready display");
                    break;
                default:
                    SetText(guideLineText, "Choose a route when you are ready.");
                    SetText(statusText, "Lobby UI V1");
                    break;
            }
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
