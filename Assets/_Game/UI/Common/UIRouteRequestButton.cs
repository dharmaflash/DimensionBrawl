using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIRouteRequestButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private UIRouteId routeId = UIRouteId.None;
        [SerializeField] private bool disableWhileRouting = true;

        private bool baseInteractable = true;
        private bool subscribed;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            baseInteractable = button == null || button.interactable;
            Subscribe();
            Apply(router != null ? router.CurrentState : UISceneFlowState.Idle);

            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
                button.interactable = baseInteractable;
            }

            Unsubscribe();
        }

        public void Configure(UISceneFlowRouter sceneRouter, UIRouteId targetRoute)
        {
            if (router != sceneRouter)
            {
                Unsubscribe();
                router = sceneRouter;
                Subscribe();
            }

            router = sceneRouter;
            routeId = targetRoute;
            Apply(router != null ? router.CurrentState : UISceneFlowState.Idle);
        }

        private void HandleClicked()
        {
            if (router != null)
            {
                router.RequestRoute(routeId);
            }
        }

        private void Subscribe()
        {
            if (subscribed || router == null || !isActiveAndEnabled)
            {
                return;
            }

            router.StateChanged += HandleRouterStateChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || router == null)
            {
                subscribed = false;
                return;
            }

            router.StateChanged -= HandleRouterStateChanged;
            subscribed = false;
        }

        private void HandleRouterStateChanged(UISceneFlowState state)
        {
            Apply(state);
        }

        private void Apply(UISceneFlowState state)
        {
            if (button != null)
            {
                button.interactable = baseInteractable && (!disableWhileRouting || !state.IsRouting);
            }
        }
    }
}
