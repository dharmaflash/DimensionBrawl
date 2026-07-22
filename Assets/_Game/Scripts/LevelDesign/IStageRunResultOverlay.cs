namespace DimensionBrawl.LevelDesign
{
    public interface IStageRunResultOverlay
    {
        string PendingResultDigest { get; }
        string PresentedResultDigest { get; }

        event System.Action<StageRunResultSummary> PresentationSucceeded;
        event System.Action<StageRunResultSummary, string> PresentationFailed;

        bool TryShow(StageRunResultSummary summary, out string error);
    }
}
