using System;
using System.Collections;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UISceneFlowRouter : MonoBehaviour
    {
        [SerializeField] private UIScreenRouteTable routeTable;
        [SerializeField] private UISceneRouteLoader routeLoader;
        [SerializeField] private UIRouteId defaultRoute = UIRouteId.Lobby;

        private bool isRouting;
        private bool routeLoadFailed;
        private int routeRequestCount;
        private UISceneFlowState currentState = UISceneFlowState.Idle;

        public event Action<UISceneFlowState> StateChanged;
        public event Action<UIRouteId, UIRouteRejectReason> RouteRejected;

        public UISceneFlowState CurrentState => currentState;
        public bool IsRouting => isRouting;
        public int RouteRequestCount => routeRequestCount;

        public void RequestDefaultRoute()
        {
            RequestRoute(defaultRoute);
        }

        public bool RequestRoute(UIRouteId routeId)
        {
            routeRequestCount++;
            if (!CanStartRoute(routeId))
            {
                return false;
            }

            if (!TryResolveRoute(routeId, out UIScreenRouteTable.Route route))
            {
                return false;
            }

            StartCoroutine(RouteRoutine(route));
            return true;
        }

        public bool RequestRouteWithScene(
            UIRouteId routeId,
            string sceneName,
            string scenePath,
            string loadingCardId = null)
        {
            routeRequestCount++;
            if (!CanStartRoute(routeId))
            {
                return false;
            }

            if (!TryResolveRoute(routeId, out UIScreenRouteTable.Route route))
            {
                return false;
            }

            StartCoroutine(RouteRoutine(route.WithScene(sceneName, scenePath, loadingCardId)));
            return true;
        }

        private bool CanStartRoute(UIRouteId routeId)
        {
            if (routeId == UIRouteId.None)
            {
                NotifyRouteRejected(routeId, UIRouteRejectReason.InvalidRoute);
                return false;
            }

            if (isRouting)
            {
                NotifyRouteRejected(routeId, UIRouteRejectReason.RouterBusy);
                return false;
            }

            return true;
        }

        private bool TryResolveRoute(UIRouteId routeId, out UIScreenRouteTable.Route route)
        {
            route = default;
            if (routeTable == null || !routeTable.TryGetRoute(routeId, out route))
            {
                Debug.LogWarning($"UI route is not configured: {routeId}", this);
                NotifyRouteRejected(routeId, UIRouteRejectReason.RouteMissing);
                SetState(routeId, string.Empty, UISceneFlowPhase.Failed, 0f, "Route missing", false);
                return false;
            }

            return true;
        }

        private IEnumerator RouteRoutine(UIScreenRouteTable.Route route)
        {
            isRouting = true;
            routeLoadFailed = false;
            SetRouteState(route, UISceneFlowPhase.Preparing, 0f, "Preparing", true);

            if (routeLoader == null)
            {
                Debug.LogWarning("UI scene route loader is not configured.", this);
                isRouting = false;
                NotifyRouteRejected(route.RouteId, UIRouteRejectReason.RouteLoaderMissing);
                SetRouteState(route, UISceneFlowPhase.Failed, 0f, "Route loader missing", false);
                yield break;
            }

            yield return routeLoader.Load(route, SetRouteState, HandleRouteLoadFailed);
            if (routeLoadFailed)
            {
                yield break;
            }

            isRouting = false;
            SetRouteState(route, UISceneFlowPhase.Completed, 1f, "Complete", false);
        }

        private void HandleRouteLoadFailed(string reason)
        {
            routeLoadFailed = true;
            isRouting = false;
        }

        private void SetRouteState(
            UIScreenRouteTable.Route route,
            UISceneFlowPhase phase,
            float normalizedProgress,
            string label,
            bool routing = true)
        {
            SetState(route.RouteId, route.SceneName, phase, normalizedProgress, label, routing);
        }

        private void SetState(
            UIRouteId routeId,
            string sceneName,
            UISceneFlowPhase phase,
            float normalizedProgress,
            string label,
            bool routing)
        {
            float clampedProgress = Mathf.Clamp01(normalizedProgress);
            currentState = new UISceneFlowState(routeId, sceneName, phase, clampedProgress, label, routing);
            if (routeLoader != null)
            {
                routeLoader.SetProgress(clampedProgress, label);
            }

            StateChanged?.Invoke(currentState);
        }

        private void NotifyRouteRejected(UIRouteId routeId, UIRouteRejectReason reason)
        {
            RouteRejected?.Invoke(routeId, reason);
        }
    }
}
