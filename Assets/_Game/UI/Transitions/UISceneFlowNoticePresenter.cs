using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UISceneFlowNoticePresenter : MonoBehaviour
    {
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private UIToastPresenter toastPresenter;
        [SerializeField] private string busyToastId = "Common.RouteBusy";
        [SerializeField] private string failedToastId = "Common.RouteFailed";
        [SerializeField] private bool showRejectedRequests = true;
        [SerializeField] private bool showFailedRoutes = true;

        private UISceneFlowPhase lastPhase = UISceneFlowPhase.Idle;

        private void OnEnable()
        {
            Subscribe();
            lastPhase = router != null ? router.CurrentState.Phase : UISceneFlowPhase.Idle;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Bind(UISceneFlowRouter sceneRouter)
        {
            if (router == sceneRouter)
            {
                return;
            }

            Unsubscribe();
            router = sceneRouter;
            lastPhase = router != null ? router.CurrentState.Phase : UISceneFlowPhase.Idle;
            Subscribe();
        }

        private void Subscribe()
        {
            if (router == null)
            {
                return;
            }

            router.StateChanged += HandleStateChanged;
            router.RouteRejected += HandleRouteRejected;
        }

        private void Unsubscribe()
        {
            if (router == null)
            {
                return;
            }

            router.StateChanged -= HandleStateChanged;
            router.RouteRejected -= HandleRouteRejected;
        }

        private void HandleStateChanged(UISceneFlowState state)
        {
            if (showFailedRoutes && state.Phase == UISceneFlowPhase.Failed && lastPhase != UISceneFlowPhase.Failed)
            {
                ShowToast(failedToastId);
            }

            lastPhase = state.Phase;
        }

        private void HandleRouteRejected(UIRouteId routeId, UIRouteRejectReason reason)
        {
            if (!showRejectedRequests)
            {
                return;
            }

            ShowToast(reason == UIRouteRejectReason.RouterBusy ? busyToastId : failedToastId);
        }

        private void ShowToast(string toastId)
        {
            if (toastPresenter != null)
            {
                toastPresenter.ShowToast(toastId);
            }
        }
    }
}
