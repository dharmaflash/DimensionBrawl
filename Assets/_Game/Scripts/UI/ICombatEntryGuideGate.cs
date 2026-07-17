using System;

namespace DimensionBrawl.UI
{
    public enum CombatEntryGuideState
    {
        NotStarted = 0,
        Playing = 1,
        Released = 2,
        Interrupted = 3
    }

    public interface ICombatEntryGuideGate
    {
        CombatEntryGuideState State { get; }
        bool IsGuidePlaying { get; }
        bool IsAwaitingAdvance { get; }

        event Action<CombatEntryGuideState> StateChanged;

        void RequestAdvance();
    }
}
