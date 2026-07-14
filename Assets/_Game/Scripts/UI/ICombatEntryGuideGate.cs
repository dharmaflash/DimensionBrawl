namespace DimensionBrawl.UI
{
    public interface ICombatEntryGuideGate
    {
        bool IsGuidePlaying { get; }
        bool IsAwaitingAdvance { get; }

        void RequestAdvance();
    }
}
