namespace DimensionBrawl.UI
{
    public readonly struct UISceneFlowState
    {
        public static readonly UISceneFlowState Idle = new UISceneFlowState(
            UIRouteId.None,
            string.Empty,
            UISceneFlowPhase.Idle,
            0f,
            "Idle",
            false);

        public UISceneFlowState(
            UIRouteId routeId,
            string sceneName,
            UISceneFlowPhase phase,
            float normalizedProgress,
            string statusText,
            bool isRouting)
        {
            RouteId = routeId;
            SceneName = sceneName ?? string.Empty;
            Phase = phase;
            NormalizedProgress = normalizedProgress;
            StatusText = statusText ?? string.Empty;
            IsRouting = isRouting;
        }

        public UIRouteId RouteId { get; }
        public string SceneName { get; }
        public UISceneFlowPhase Phase { get; }
        public float NormalizedProgress { get; }
        public string StatusText { get; }
        public bool IsRouting { get; }
    }
}
