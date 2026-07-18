using DimensionBrawl.Presentation.Narrative;
using NUnit.Framework;

namespace DimensionBrawl.Tests
{
    public sealed class StoryTutorialReviewTransitionSessionPlayModeTests
    {
        [TestCase(StoryTutorialReviewTerminalReason.Completed)]
        [TestCase(StoryTutorialReviewTerminalReason.Skipped)]
        public void ContinuationReasonsBecomeEligibleOnlyAfterSuccessfulRelease(
            StoryTutorialReviewTerminalReason terminalReason)
        {
            var session = new StoryTutorialReviewTransitionSession();
            const long generation = 17;
            int releaseCount = 0;
            StoryTutorialReviewReceipt publishedReceipt = default;
            session.Released += receipt =>
            {
                releaseCount++;
                publishedReceipt = receipt;
            };

            Assert.That(
                session.TryBegin(generation),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(session.Phase,
                Is.EqualTo(StoryTutorialReviewTransitionPhase.StoryPresenting));
            Assert.That(session.IsCurrentStoryGeneration(generation), Is.True);
            Assert.That(session.HasReceipt, Is.False);
            Assert.That(session.Receipt.CanDispatchReviewTutorialStart, Is.False);

            Assert.That(
                session.TryRequestTerminal(generation, terminalReason),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(session.Phase,
                Is.EqualTo(StoryTutorialReviewTransitionPhase.Terminating));
            Assert.That(session.IsCurrentStoryGeneration(generation), Is.False);
            Assert.That(session.HasReceipt, Is.False);
            Assert.That(releaseCount, Is.Zero);

            Assert.That(
                session.TrySealRelease(
                    generation,
                    storyOwnedWorkReleased: true,
                    stateRestoreSucceeded: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt receipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));

            Assert.That(session.Phase,
                Is.EqualTo(StoryTutorialReviewTransitionPhase.Terminated));
            Assert.That(session.HasReceipt, Is.True);
            Assert.That(receipt.IsSealed, Is.True);
            Assert.That(receipt.Generation, Is.EqualTo(generation));
            Assert.That(receipt.TerminalReason, Is.EqualTo(terminalReason));
            Assert.That(receipt.StoryOwnedWorkReleased, Is.True);
            Assert.That(receipt.StateRestoreSucceeded, Is.True);
            Assert.That(receipt.TutorialTargetAvailable, Is.True);
            Assert.That(receipt.CanDispatchReviewTutorialStart, Is.True);
            AssertReceiptsEqual(receipt, session.Receipt);
            AssertReceiptsEqual(receipt, publishedReceipt);
            Assert.That(releaseCount, Is.EqualTo(1));
        }

        [TestCase(StoryTutorialReviewTerminalReason.Cancelled)]
        [TestCase(StoryTutorialReviewTerminalReason.OwnerDisabled)]
        [TestCase(StoryTutorialReviewTerminalReason.BindingUnavailable)]
        [TestCase(StoryTutorialReviewTerminalReason.StateApplyFailed)]
        public void NonContinuationReasonsRemainIneligibleAfterSuccessfulRelease(
            StoryTutorialReviewTerminalReason terminalReason)
        {
            var session = new StoryTutorialReviewTransitionSession();
            const long generation = 23;

            Assert.That(
                session.TryBegin(generation),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TryRequestTerminal(generation, terminalReason),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TrySealRelease(
                    generation,
                    storyOwnedWorkReleased: true,
                    stateRestoreSucceeded: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt receipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));

            Assert.That(receipt.TerminalReason, Is.EqualTo(terminalReason));
            Assert.That(receipt.StoryOwnedWorkReleased, Is.True);
            Assert.That(receipt.StateRestoreSucceeded, Is.True);
            Assert.That(receipt.TutorialTargetAvailable, Is.True);
            Assert.That(receipt.CanDispatchReviewTutorialStart, Is.False);
        }

        [TestCase(StoryTutorialReviewTerminalReason.Completed, false, true, true)]
        [TestCase(StoryTutorialReviewTerminalReason.Completed, true, false, true)]
        [TestCase(StoryTutorialReviewTerminalReason.Completed, true, true, false)]
        [TestCase(StoryTutorialReviewTerminalReason.Skipped, false, true, true)]
        [TestCase(StoryTutorialReviewTerminalReason.Skipped, true, false, true)]
        [TestCase(StoryTutorialReviewTerminalReason.Skipped, true, true, false)]
        public void ContinuationReasonsFailClosedWhenAnyReleaseRequirementFails(
            StoryTutorialReviewTerminalReason terminalReason,
            bool storyOwnedWorkReleased,
            bool stateRestoreSucceeded,
            bool tutorialTargetAvailable)
        {
            var session = new StoryTutorialReviewTransitionSession();
            const long generation = 31;

            Assert.That(
                session.TryBegin(generation),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TryRequestTerminal(generation, terminalReason),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TrySealRelease(
                    generation,
                    storyOwnedWorkReleased,
                    stateRestoreSucceeded,
                    tutorialTargetAvailable,
                    out StoryTutorialReviewReceipt receipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));

            Assert.That(receipt.StoryOwnedWorkReleased,
                Is.EqualTo(storyOwnedWorkReleased));
            Assert.That(receipt.StateRestoreSucceeded,
                Is.EqualTo(stateRestoreSucceeded));
            Assert.That(receipt.TutorialTargetAvailable,
                Is.EqualTo(tutorialTargetAvailable));
            Assert.That(receipt.CanDispatchReviewTutorialStart, Is.False);
        }

        [Test]
        public void FirstTerminalAndReleaseWinAgainstConflictingDuplicates()
        {
            var session = new StoryTutorialReviewTransitionSession();
            const long generation = 41;
            int releaseCount = 0;
            session.Released += _ => releaseCount++;

            Assert.That(
                session.TryBegin(generation),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TryRequestTerminal(
                    generation,
                    StoryTutorialReviewTerminalReason.Completed),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TryRequestTerminal(
                    generation,
                    StoryTutorialReviewTerminalReason.Cancelled),
                Is.EqualTo(StoryTutorialReviewSignalResult.AlreadyAccepted));

            Assert.That(
                session.TrySealRelease(
                    generation,
                    storyOwnedWorkReleased: true,
                    stateRestoreSucceeded: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt firstReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(releaseCount, Is.EqualTo(1));

            Assert.That(
                session.TrySealRelease(
                    generation,
                    storyOwnedWorkReleased: false,
                    stateRestoreSucceeded: false,
                    tutorialTargetAvailable: false,
                    out StoryTutorialReviewReceipt duplicateReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.AlreadyAccepted));
            Assert.That(
                session.TryRequestTerminal(
                    generation,
                    StoryTutorialReviewTerminalReason.StateApplyFailed),
                Is.EqualTo(StoryTutorialReviewSignalResult.AlreadyAccepted));

            AssertReceiptsEqual(firstReceipt, duplicateReceipt);
            AssertReceiptsEqual(firstReceipt, session.Receipt);
            Assert.That(firstReceipt.TerminalReason,
                Is.EqualTo(StoryTutorialReviewTerminalReason.Completed));
            Assert.That(firstReceipt.CanDispatchReviewTutorialStart, Is.True);
            Assert.That(releaseCount, Is.EqualTo(1));
        }

        [Test]
        public void PriorGenerationSignalsCannotMutateFreshGeneration()
        {
            var session = new StoryTutorialReviewTransitionSession();
            const long firstGeneration = 52;
            const long freshGeneration = 77;

            Assert.That(
                session.TryBegin(firstGeneration),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TryRequestTerminal(
                    firstGeneration,
                    StoryTutorialReviewTerminalReason.Cancelled),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TrySealRelease(
                    firstGeneration,
                    storyOwnedWorkReleased: true,
                    stateRestoreSucceeded: true,
                    tutorialTargetAvailable: true,
                    out _),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));

            Assert.That(
                session.TryBegin(freshGeneration),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(session.CurrentGeneration, Is.EqualTo(freshGeneration));
            Assert.That(session.Phase,
                Is.EqualTo(StoryTutorialReviewTransitionPhase.StoryPresenting));
            Assert.That(session.HasReceipt, Is.False);
            Assert.That(session.IsCurrentStoryGeneration(firstGeneration), Is.False);
            Assert.That(session.IsCurrentStoryGeneration(freshGeneration), Is.True);

            Assert.That(
                session.TryRequestTerminal(
                    firstGeneration,
                    StoryTutorialReviewTerminalReason.Completed),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            Assert.That(
                session.TrySealRelease(
                    firstGeneration,
                    storyOwnedWorkReleased: true,
                    stateRestoreSucceeded: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt staleReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            Assert.That(staleReceipt.IsSealed, Is.False);
            Assert.That(
                session.TryBegin(firstGeneration),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            Assert.That(
                session.TryBegin(freshGeneration),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            Assert.That(
                session.TryBegin(freshGeneration - 1),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));

            Assert.That(session.CurrentGeneration, Is.EqualTo(freshGeneration));
            Assert.That(session.Phase,
                Is.EqualTo(StoryTutorialReviewTransitionPhase.StoryPresenting));
            Assert.That(session.HasReceipt, Is.False);
            Assert.That(session.IsCurrentStoryGeneration(freshGeneration), Is.True);
        }

        [Test]
        public void InvalidOrderingCannotFabricateAReleaseReceipt()
        {
            var session = new StoryTutorialReviewTransitionSession();
            const long generation = 91;
            int releaseCount = 0;
            session.Released += _ => releaseCount++;

            Assert.That(
                session.TryBegin(0),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            Assert.That(
                session.TryBegin(-1),
                Is.EqualTo(StoryTutorialReviewSignalResult.StaleGeneration));
            Assert.That(
                session.TryBegin(generation),
                Is.EqualTo(StoryTutorialReviewSignalResult.Accepted));
            Assert.That(
                session.TryBegin(generation + 1),
                Is.EqualTo(StoryTutorialReviewSignalResult.InvalidPhase));

            Assert.That(
                session.TrySealRelease(
                    generation,
                    storyOwnedWorkReleased: true,
                    stateRestoreSucceeded: true,
                    tutorialTargetAvailable: true,
                    out StoryTutorialReviewReceipt prematureReceipt),
                Is.EqualTo(StoryTutorialReviewSignalResult.InvalidPhase));
            Assert.That(prematureReceipt.IsSealed, Is.False);
            Assert.That(session.HasReceipt, Is.False);
            Assert.That(releaseCount, Is.Zero);
            Assert.That(session.CurrentGeneration, Is.EqualTo(generation));
            Assert.That(session.IsCurrentStoryGeneration(generation), Is.True);
        }

        private static void AssertReceiptsEqual(
            StoryTutorialReviewReceipt expected,
            StoryTutorialReviewReceipt actual)
        {
            Assert.That(actual.Generation, Is.EqualTo(expected.Generation));
            Assert.That(actual.TerminalReason, Is.EqualTo(expected.TerminalReason));
            Assert.That(actual.StoryOwnedWorkReleased,
                Is.EqualTo(expected.StoryOwnedWorkReleased));
            Assert.That(actual.StateRestoreSucceeded,
                Is.EqualTo(expected.StateRestoreSucceeded));
            Assert.That(actual.TutorialTargetAvailable,
                Is.EqualTo(expected.TutorialTargetAvailable));
            Assert.That(actual.CanDispatchReviewTutorialStart,
                Is.EqualTo(expected.CanDispatchReviewTutorialStart));
        }
    }
}
