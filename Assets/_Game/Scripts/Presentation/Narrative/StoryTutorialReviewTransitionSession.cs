using System;

namespace DimensionBrawl.Presentation.Narrative
{
    public enum StoryTutorialReviewTransitionPhase
    {
        Idle = 0,
        StoryPresenting = 1,
        Terminating = 2,
        Terminated = 3
    }

    public enum StoryTutorialReviewTerminalReason
    {
        Completed = 0,
        Skipped = 1,
        Cancelled = 2,
        OwnerDisabled = 3,
        BindingUnavailable = 4,
        StateApplyFailed = 5
    }

    public enum StoryTutorialReviewSignalResult
    {
        Accepted = 0,
        AlreadyAccepted = 1,
        StaleGeneration = 2,
        InvalidPhase = 3
    }

    /// <summary>
    /// Review-only evidence that story-owned work and local presentation state were released.
    /// It is not product tutorial, route, StageRun, combat, save, or progression authority.
    /// </summary>
    public readonly struct StoryTutorialReviewReceipt
    {
        public StoryTutorialReviewReceipt(
            long generation,
            StoryTutorialReviewTerminalReason terminalReason,
            bool storyOwnedWorkReleased,
            bool stateRestoreSucceeded,
            bool tutorialTargetAvailable)
        {
            Generation = generation;
            TerminalReason = terminalReason;
            StoryOwnedWorkReleased = storyOwnedWorkReleased;
            StateRestoreSucceeded = stateRestoreSucceeded;
            TutorialTargetAvailable = tutorialTargetAvailable;
        }

        public long Generation { get; }
        public StoryTutorialReviewTerminalReason TerminalReason { get; }
        public bool StoryOwnedWorkReleased { get; }
        public bool StateRestoreSucceeded { get; }
        public bool TutorialTargetAvailable { get; }
        public bool IsSealed => Generation > 0;
        public bool CanDispatchReviewTutorialStart =>
            IsSealed
            && StoryOwnedWorkReleased
            && StateRestoreSucceeded
            && TutorialTargetAvailable
            && (TerminalReason == StoryTutorialReviewTerminalReason.Completed
                || TerminalReason == StoryTutorialReviewTerminalReason.Skipped);
    }

    /// <summary>
    /// Pure generation gate for the TEMP_DO_NOT_SHIP story-to-tutorial review boundary.
    /// Unity state capture and restoration remain the responsibility of the review gate.
    /// </summary>
    public sealed class StoryTutorialReviewTransitionSession
    {
        private long highestAcceptedGeneration;
        private long currentGeneration;
        private StoryTutorialReviewTerminalReason terminalReason;
        private StoryTutorialReviewReceipt receipt;

        public StoryTutorialReviewTransitionPhase Phase { get; private set; }
            = StoryTutorialReviewTransitionPhase.Idle;
        public long CurrentGeneration => currentGeneration;
        public StoryTutorialReviewReceipt Receipt => receipt;
        public bool HasReceipt => receipt.IsSealed;

        public event Action<StoryTutorialReviewReceipt> Released;

        public StoryTutorialReviewSignalResult TryBegin(long generation)
        {
            if (generation <= 0 || generation <= highestAcceptedGeneration)
            {
                return StoryTutorialReviewSignalResult.StaleGeneration;
            }

            if (Phase == StoryTutorialReviewTransitionPhase.StoryPresenting
                || Phase == StoryTutorialReviewTransitionPhase.Terminating)
            {
                return StoryTutorialReviewSignalResult.InvalidPhase;
            }

            highestAcceptedGeneration = generation;
            currentGeneration = generation;
            terminalReason = default;
            receipt = default;
            Phase = StoryTutorialReviewTransitionPhase.StoryPresenting;
            return StoryTutorialReviewSignalResult.Accepted;
        }

        public StoryTutorialReviewSignalResult TryRequestTerminal(
            long generation,
            StoryTutorialReviewTerminalReason reason)
        {
            if (generation <= 0 || generation != currentGeneration)
            {
                return StoryTutorialReviewSignalResult.StaleGeneration;
            }

            if (Phase == StoryTutorialReviewTransitionPhase.Terminating
                || Phase == StoryTutorialReviewTransitionPhase.Terminated)
            {
                return StoryTutorialReviewSignalResult.AlreadyAccepted;
            }

            if (Phase != StoryTutorialReviewTransitionPhase.StoryPresenting)
            {
                return StoryTutorialReviewSignalResult.InvalidPhase;
            }

            terminalReason = reason;
            Phase = StoryTutorialReviewTransitionPhase.Terminating;
            return StoryTutorialReviewSignalResult.Accepted;
        }

        public StoryTutorialReviewSignalResult TrySealRelease(
            long generation,
            bool storyOwnedWorkReleased,
            bool stateRestoreSucceeded,
            bool tutorialTargetAvailable,
            out StoryTutorialReviewReceipt sealedReceipt)
        {
            sealedReceipt = default;
            if (generation <= 0 || generation != currentGeneration)
            {
                return StoryTutorialReviewSignalResult.StaleGeneration;
            }

            if (Phase == StoryTutorialReviewTransitionPhase.Terminated)
            {
                sealedReceipt = receipt;
                return StoryTutorialReviewSignalResult.AlreadyAccepted;
            }

            if (Phase != StoryTutorialReviewTransitionPhase.Terminating)
            {
                return StoryTutorialReviewSignalResult.InvalidPhase;
            }

            receipt = new StoryTutorialReviewReceipt(
                generation,
                terminalReason,
                storyOwnedWorkReleased,
                stateRestoreSucceeded,
                tutorialTargetAvailable);
            sealedReceipt = receipt;
            Phase = StoryTutorialReviewTransitionPhase.Terminated;
            Released?.Invoke(receipt);
            return StoryTutorialReviewSignalResult.Accepted;
        }

        public bool IsCurrentStoryGeneration(long generation)
        {
            return generation > 0
                && generation == currentGeneration
                && Phase == StoryTutorialReviewTransitionPhase.StoryPresenting;
        }
    }
}
