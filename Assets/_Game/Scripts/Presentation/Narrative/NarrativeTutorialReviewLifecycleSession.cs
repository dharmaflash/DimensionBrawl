using System;

namespace DimensionBrawl.Presentation.Narrative
{
    public enum NarrativeTutorialReviewPhase
    {
        Idle = 0,
        VisualNovel = 1,
        Tutorial = 2,
        Terminating = 3,
        Terminated = 4
    }

    public enum NarrativeTutorialReviewTerminalReason
    {
        Completed = 0,
        Skipped = 1,
        Cancelled = 2,
        OwnerDisabled = 3,
        SceneUnloading = 4,
        BindingUnavailable = 5
    }

    public enum NarrativeTutorialReviewSignalResult
    {
        Accepted = 0,
        AlreadyAccepted = 1,
        StaleGeneration = 2,
        InvalidPhase = 3
    }

    /// <summary>
    /// Evidence for entering the briefing only inside the TEMP_DO_NOT_SHIP review flow.
    /// It is not StageRun, tutorial-proof, or combat authority.
    /// </summary>
    public readonly struct NarrativeTutorialReviewReceipt
    {
        public NarrativeTutorialReviewReceipt(
            long generation,
            NarrativeTutorialReviewTerminalReason terminalReason,
            bool tutorialEntered,
            bool cleanupSucceeded)
        {
            Generation = generation;
            TerminalReason = terminalReason;
            TutorialEntered = tutorialEntered;
            CleanupSucceeded = cleanupSucceeded;
        }

        public long Generation { get; }
        public NarrativeTutorialReviewTerminalReason TerminalReason { get; }
        public bool TutorialEntered { get; }
        public bool CleanupSucceeded { get; }
        public bool IsValid => Generation > 0;
        public bool CanEnterReviewBriefing =>
            IsValid
            && TutorialEntered
            && CleanupSucceeded
            && (TerminalReason == NarrativeTutorialReviewTerminalReason.Completed
                || TerminalReason == NarrativeTutorialReviewTerminalReason.Skipped);
    }

    /// <summary>
    /// Pure lifecycle kernel that rejects stale signals and seals at most one review receipt.
    /// The controller remains responsible for stopping and releasing the work it actually owns.
    /// </summary>
    public sealed class NarrativeTutorialReviewLifecycleSession
    {
        private long nextGeneration;
        private long currentGeneration;
        private bool tutorialEntered;
        private NarrativeTutorialReviewTerminalReason terminalReason;
        private NarrativeTutorialReviewReceipt receipt;

        public NarrativeTutorialReviewPhase Phase { get; private set; }
            = NarrativeTutorialReviewPhase.Idle;
        public long CurrentGeneration => currentGeneration;
        public NarrativeTutorialReviewReceipt Receipt => receipt;
        public bool HasReceipt => receipt.IsValid;

        public event Action<NarrativeTutorialReviewReceipt> Released;

        public long Begin()
        {
            if (Phase == NarrativeTutorialReviewPhase.VisualNovel
                || Phase == NarrativeTutorialReviewPhase.Tutorial
                || Phase == NarrativeTutorialReviewPhase.Terminating)
            {
                throw new InvalidOperationException(
                    "The narrative tutorial review lifecycle is already active.");
            }

            currentGeneration = checked(++nextGeneration);
            tutorialEntered = false;
            terminalReason = default;
            receipt = default;
            Phase = NarrativeTutorialReviewPhase.VisualNovel;
            return currentGeneration;
        }

        public bool TryBeginTutorial(long generation)
        {
            if (!IsCurrentLiveGeneration(generation)
                || Phase != NarrativeTutorialReviewPhase.VisualNovel)
            {
                return false;
            }

            tutorialEntered = true;
            Phase = NarrativeTutorialReviewPhase.Tutorial;
            return true;
        }

        public NarrativeTutorialReviewSignalResult TryRequestTerminal(
            long generation,
            NarrativeTutorialReviewTerminalReason reason)
        {
            if (generation <= 0 || generation != currentGeneration)
            {
                return NarrativeTutorialReviewSignalResult.StaleGeneration;
            }

            if (Phase == NarrativeTutorialReviewPhase.Terminating
                || Phase == NarrativeTutorialReviewPhase.Terminated)
            {
                return NarrativeTutorialReviewSignalResult.AlreadyAccepted;
            }

            bool requiresTutorial = reason == NarrativeTutorialReviewTerminalReason.Completed
                || reason == NarrativeTutorialReviewTerminalReason.Skipped
                || reason == NarrativeTutorialReviewTerminalReason.BindingUnavailable;
            if ((requiresTutorial && Phase != NarrativeTutorialReviewPhase.Tutorial)
                || (!requiresTutorial
                    && Phase != NarrativeTutorialReviewPhase.VisualNovel
                    && Phase != NarrativeTutorialReviewPhase.Tutorial))
            {
                return NarrativeTutorialReviewSignalResult.InvalidPhase;
            }

            terminalReason = reason;
            Phase = NarrativeTutorialReviewPhase.Terminating;
            return NarrativeTutorialReviewSignalResult.Accepted;
        }

        public NarrativeTutorialReviewSignalResult TrySealRelease(
            long generation,
            bool cleanupSucceeded,
            out NarrativeTutorialReviewReceipt sealedReceipt)
        {
            sealedReceipt = default;
            if (generation <= 0 || generation != currentGeneration)
            {
                return NarrativeTutorialReviewSignalResult.StaleGeneration;
            }

            if (Phase == NarrativeTutorialReviewPhase.Terminated)
            {
                sealedReceipt = receipt;
                return NarrativeTutorialReviewSignalResult.AlreadyAccepted;
            }

            if (Phase != NarrativeTutorialReviewPhase.Terminating)
            {
                return NarrativeTutorialReviewSignalResult.InvalidPhase;
            }

            receipt = new NarrativeTutorialReviewReceipt(
                generation,
                terminalReason,
                tutorialEntered,
                cleanupSucceeded);
            sealedReceipt = receipt;
            Phase = NarrativeTutorialReviewPhase.Terminated;
            Released?.Invoke(receipt);
            return NarrativeTutorialReviewSignalResult.Accepted;
        }

        public bool IsCurrentLiveGeneration(long generation)
        {
            return generation > 0
                && generation == currentGeneration
                && (Phase == NarrativeTutorialReviewPhase.VisualNovel
                    || Phase == NarrativeTutorialReviewPhase.Tutorial);
        }
    }
}
