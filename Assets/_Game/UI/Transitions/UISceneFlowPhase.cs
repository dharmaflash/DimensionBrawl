namespace DimensionBrawl.UI
{
    public enum UISceneFlowPhase
    {
        Idle = 0,
        Preparing = 10,
        TransitionOut = 20,
        Loading = 30,
        Activating = 40,
        Completed = 50,
        Failed = 90
    }
}
