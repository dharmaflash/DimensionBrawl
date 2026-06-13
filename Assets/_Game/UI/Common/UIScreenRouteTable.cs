using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Screen Route Table")]
    public sealed class UIScreenRouteTable : ScriptableObject
    {
        [Serializable]
        public struct Route
        {
            [SerializeField] private UIRouteId routeId;
            [SerializeField] private string sceneName;
            [SerializeField] private string scenePath;
            [SerializeField] private string transitionId;
            [SerializeField] private string loadingCardId;
            [SerializeField] private bool useAsyncLoading;
            [SerializeField, Min(0f)] private float minimumLoadingSeconds;

            public UIRouteId RouteId => routeId;
            public string SceneName => sceneName;
            public string ScenePath => scenePath;
            public string TransitionId => transitionId;
            public string LoadingCardId => loadingCardId;
            public bool UseAsyncLoading => useAsyncLoading;
            public float MinimumLoadingSeconds => minimumLoadingSeconds;
        }

        [SerializeField] private Route[] routes = Array.Empty<Route>();

        public bool TryGetRoute(UIRouteId routeId, out Route route)
        {
            for (int i = 0; i < routes.Length; i++)
            {
                if (routes[i].RouteId == routeId)
                {
                    route = routes[i];
                    return true;
                }
            }

            route = default;
            return false;
        }
    }
}
