using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DimensionBrawl.Combat;

namespace DimensionBrawl.LevelDesign
{
    public enum StageRunMasteryEvaluationState
    {
        NotEvaluated = 0
    }

    public sealed class StageRunTerminalRecord
    {
        internal StageRunTerminalRecord(StageRunIdentity identity, EncounterTerminalResolution resolution)
        {
            TerminalRecordId =
                $"{identity.RunId}:terminal:{resolution.RunGeneration}:{resolution.RootAdmissionSequence}:{resolution.Epoch}";
            RunId = identity.RunId;
            RouteDigest = identity.RouteSnapshotDigest;
            CoordinatorRunGeneration = resolution.RunGeneration;
            RootAdmissionSequence = resolution.RootAdmissionSequence;
            Epoch = resolution.Epoch;
            Outcome = resolution.Outcome == EncounterTerminalOutcome.Clear
                ? StageRouteOutcome.Clear
                : StageRouteOutcome.Fail;
            Reason = resolution.Reason;
            PlayerDown = resolution.PlayerDown;
            BossDead = resolution.BossDead;
            PlayerHealth = resolution.PlayerHealth;
            BossHealth = resolution.BossHealth;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public string TerminalRecordId { get; }
        public string RunId { get; }
        public string RouteDigest { get; }
        public long CoordinatorRunGeneration { get; }
        public long RootAdmissionSequence { get; }
        public long Epoch { get; }
        public StageRouteOutcome Outcome { get; }
        public EncounterTerminalReason Reason { get; }
        public bool PlayerDown { get; }
        public bool BossDead { get; }
        public float PlayerHealth { get; }
        public float BossHealth { get; }
        public string CanonicalDigest { get; }

        public bool Matches(EncounterTerminalResolution resolution)
        {
            return CoordinatorRunGeneration == resolution.RunGeneration
                && RootAdmissionSequence == resolution.RootAdmissionSequence
                && Epoch == resolution.Epoch
                && Outcome == (resolution.Outcome == EncounterTerminalOutcome.Clear
                    ? StageRouteOutcome.Clear
                    : StageRouteOutcome.Fail)
                && Reason == resolution.Reason
                && PlayerDown == resolution.PlayerDown
                && BossDead == resolution.BossDead
                && PlayerHealth.Equals(resolution.PlayerHealth)
                && BossHealth.Equals(resolution.BossHealth);
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(512);
            StageCanonicalDigest.Append(builder, "terminal.id", TerminalRecordId);
            StageCanonicalDigest.Append(builder, "terminal.runId", RunId);
            StageCanonicalDigest.Append(builder, "terminal.routeDigest", RouteDigest);
            StageCanonicalDigest.Append(builder, "terminal.runGeneration", CoordinatorRunGeneration);
            StageCanonicalDigest.Append(builder, "terminal.rootAdmissionSequence", RootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "terminal.epoch", Epoch);
            StageCanonicalDigest.Append(builder, "terminal.outcome", (int)Outcome);
            StageCanonicalDigest.Append(builder, "terminal.reason", (int)Reason);
            StageCanonicalDigest.Append(builder, "terminal.playerDown", PlayerDown);
            StageCanonicalDigest.Append(builder, "terminal.bossDead", BossDead);
            StageCanonicalDigest.Append(
                builder,
                "terminal.playerHealth",
                PlayerHealth.ToString("R", CultureInfo.InvariantCulture));
            StageCanonicalDigest.Append(
                builder,
                "terminal.bossHealth",
                BossHealth.ToString("R", CultureInfo.InvariantCulture));
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunResultSummary
    {
        private readonly StageRunActionSnapshot[] offeredActions;
        private readonly StageSceneSegmentResult[] segmentResults;
        private readonly StageRunSemanticProofFact[] semanticProofs;

        internal StageRunResultSummary(
            StageRunIdentity identity,
            StageRunRouteSnapshot routeSnapshot,
            StageRunTerminalRecord terminalRecord,
            StageRunFactBundle factBundle)
        {
            Identity = identity;
            RouteSnapshot = routeSnapshot;
            TerminalRecord = terminalRecord;
            if (factBundle == null
                || factBundle.Outcome == null
                || factBundle.TutorialRouteSummary == null
                || factBundle.CombatFacts == null)
            {
                throw new ArgumentNullException(nameof(factBundle));
            }

            OutcomeFact = factBundle.Outcome;
            TutorialRouteSummaryFact = factBundle.TutorialRouteSummary;
            CombatFacts = factBundle.CombatFacts;
            segmentResults = (StageSceneSegmentResult[])factBundle.SegmentResults.Clone();
            semanticProofs = (StageRunSemanticProofFact[])factBundle.SemanticProofs.Clone();
            Outcome = terminalRecord.Outcome;
            MasteryEvaluationState = StageRunMasteryEvaluationState.NotEvaluated;

            var offered = new List<StageRunActionSnapshot>(routeSnapshot.ActionCount);
            for (int i = 0; i < routeSnapshot.ActionCount; i++)
            {
                StageRunActionSnapshot action = routeSnapshot.GetAction(i);
                if (action.Allows(Outcome))
                {
                    offered.Add(action);
                }
            }

            offeredActions = offered.ToArray();
            ResultSummaryId = $"{identity.RunId}:result:1";
            ResultSummaryDigest = ComputeCanonicalDigest();
        }

        public string ResultSummaryId { get; }
        public StageRunIdentity Identity { get; }
        public StageRunRouteSnapshot RouteSnapshot { get; }
        public StageRunTerminalRecord TerminalRecord { get; }
        public StageOutcomeFact OutcomeFact { get; }
        public StageTutorialRouteSummaryFact TutorialRouteSummaryFact { get; }
        public StageRunCombatFacts CombatFacts { get; }
        public StageRouteOutcome Outcome { get; }
        public StageRunMasteryEvaluationState MasteryEvaluationState { get; }
        public int SegmentResultCount => segmentResults.Length;
        public int SemanticProofCount => semanticProofs.Length;
        public int OfferedActionCount => offeredActions.Length;
        public string ResultSummaryDigest { get; }

        public StageSceneSegmentResult GetSegmentResult(int index)
        {
            if (index < 0 || index >= segmentResults.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return segmentResults[index];
        }

        public StageRunSemanticProofFact GetSemanticProof(int index)
        {
            if (index < 0 || index >= semanticProofs.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return semanticProofs[index];
        }

        public bool TryGetSemanticProof(string proofId, out StageRunSemanticProofFact proof)
        {
            if (!string.IsNullOrWhiteSpace(proofId))
            {
                for (int i = 0; i < semanticProofs.Length; i++)
                {
                    if (string.Equals(semanticProofs[i].ProofId, proofId, StringComparison.Ordinal))
                    {
                        proof = semanticProofs[i];
                        return true;
                    }
                }
            }

            proof = null;
            return false;
        }

        public StageRunActionSnapshot GetOfferedAction(int index)
        {
            if (index < 0 || index >= offeredActions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return offeredActions[index];
        }

        public bool TryGetOfferedAction(string actionId, out StageRunActionSnapshot action)
        {
            if (!string.IsNullOrWhiteSpace(actionId))
            {
                for (int i = 0; i < offeredActions.Length; i++)
                {
                    if (string.Equals(offeredActions[i].ActionId, actionId, StringComparison.Ordinal))
                    {
                        action = offeredActions[i];
                        return true;
                    }
                }
            }

            action = null;
            return false;
        }

#if UNITY_INCLUDE_TESTS
        public static bool TryCreateCallbackOrderDigestComparisonForTests(
            StageRunRouteSnapshot routeSnapshot,
            EncounterTerminalEpochEvidence firstEvidence,
            EncounterTerminalEpochEvidence secondEvidence,
            out string firstClosureDigest,
            out string secondClosureDigest,
            out string firstSummaryDigest,
            out string secondSummaryDigest,
            out string error)
        {
            firstClosureDigest = string.Empty;
            secondClosureDigest = string.Empty;
            firstSummaryDigest = string.Empty;
            secondSummaryDigest = string.Empty;
            error = string.Empty;
            if (routeSnapshot == null || firstEvidence == null || secondEvidence == null)
            {
                error = "Callback-order comparison requires a route and two terminal evidence records.";
                return false;
            }

            var identity = new StageRunIdentity("fixed-callback-order-run", routeSnapshot);
            if (!TerminalEpochClosureRecord.TryCreate(
                    identity,
                    routeSnapshot,
                    firstEvidence,
                    out TerminalEpochClosureRecord firstClosure,
                    out error)
                || !TerminalEpochClosureRecord.TryCreate(
                    identity,
                    routeSnapshot,
                    secondEvidence,
                    out TerminalEpochClosureRecord secondClosure,
                    out error))
            {
                return false;
            }

            EncounterTerminalResolution firstSource = firstEvidence.Resolution;
            EncounterTerminalResolution secondSource = secondEvidence.Resolution;
            var firstResolution = new EncounterTerminalResolution(
                1,
                1,
                1,
                firstSource.Outcome,
                firstSource.Reason,
                firstSource.PlayerDown,
                firstSource.BossDead,
                firstSource.PlayerHealth,
                firstSource.BossHealth);
            var secondResolution = new EncounterTerminalResolution(
                1,
                1,
                1,
                secondSource.Outcome,
                secondSource.Reason,
                secondSource.PlayerDown,
                secondSource.BossDead,
                secondSource.PlayerHealth,
                secondSource.BossHealth);
            var firstTerminal = new StageRunTerminalRecord(identity, firstResolution);
            var secondTerminal = new StageRunTerminalRecord(identity, secondResolution);
            var facts = new StageRunFactBundle(
                new[]
                {
                    new StageSceneSegmentResult(
                        "corridor_intro_tutorial",
                        0,
                        true,
                        true,
                        StageSceneSegmentExitReason.Completed,
                        100),
                    new StageSceneSegmentResult(
                        "station_entry_combat",
                        1,
                        true,
                        true,
                        StageSceneSegmentExitReason.Completed,
                        200)
                },
                new StageTutorialRouteSummaryFact(identity, "corridor_intro_tutorial", 100),
                new StageRunCombatFacts(
                    0d,
                    1,
                    0,
                    Array.Empty<StageRunSummonUseFact>(),
                    0),
                Array.Empty<StageRunSemanticProofFact>(),
                new StageOutcomeFact(
                    identity,
                    firstResolution,
                    "station_entry_combat",
                    300,
                    200,
                    1));
            var firstSummary = new StageRunResultSummary(
                identity,
                routeSnapshot,
                firstTerminal,
                facts);
            var secondSummary = new StageRunResultSummary(
                identity,
                routeSnapshot,
                secondTerminal,
                facts);
            firstClosureDigest = firstClosure.CanonicalDigest;
            secondClosureDigest = secondClosure.CanonicalDigest;
            firstSummaryDigest = firstSummary.ResultSummaryDigest;
            secondSummaryDigest = secondSummary.ResultSummaryDigest;
            return true;
        }
#endif

        internal bool HasValidCanonicalDigest()
        {
            return string.Equals(
                ResultSummaryDigest,
                ComputeCanonicalDigest(),
                StringComparison.Ordinal);
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1024);
            StageCanonicalDigest.Append(builder, "result.id", ResultSummaryId);
            StageCanonicalDigest.Append(builder, "result.schemaVersion", Identity.SchemaVersion);
            StageCanonicalDigest.Append(builder, "result.runId", Identity.RunId);
            StageCanonicalDigest.Append(builder, "result.playableStageId", Identity.PlayableStageId);
            StageCanonicalDigest.Append(builder, "result.routeRevision", Identity.RouteRevision);
            StageCanonicalDigest.Append(builder, "result.routeDigest", Identity.RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "result.terminalRecordId", TerminalRecord.TerminalRecordId);
            StageCanonicalDigest.Append(builder, "result.terminalRecordDigest", TerminalRecord.CanonicalDigest);
            StageCanonicalDigest.Append(builder, "result.outcome", (int)Outcome);
            StageCanonicalDigest.Append(builder, "result.outcomeFactDigest", OutcomeFact.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "result.tutorialRouteSummaryFactDigest",
                TutorialRouteSummaryFact.CanonicalDigest);
            StageCanonicalDigest.Append(builder, "result.combatFactsDigest", CombatFacts.CanonicalDigest);
            StageCanonicalDigest.Append(builder, "result.segmentResultCount", segmentResults.Length);
            for (int i = 0; i < segmentResults.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"result.segmentResult[{i}].digest",
                    segmentResults[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(builder, "result.semanticProofCount", semanticProofs.Length);
            for (int i = 0; i < semanticProofs.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"result.semanticProof[{i}].digest",
                    semanticProofs[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(builder, "result.mastery", (int)MasteryEvaluationState);

            StageRunActionSnapshot[] sortedActions = (StageRunActionSnapshot[])offeredActions.Clone();
            Array.Sort(
                sortedActions,
                (left, right) => string.Compare(left.ActionId, right.ActionId, StringComparison.Ordinal));
            StageCanonicalDigest.Append(builder, "result.offeredActionCount", sortedActions.Length);
            for (int i = 0; i < sortedActions.Length; i++)
            {
                StageRunActionSnapshot action = sortedActions[i];
                string prefix = $"result.action[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", action.ActionId);
                StageCanonicalDigest.Append(builder, prefix + ".kind", (int)action.ActionKind);
                StageCanonicalDigest.Append(builder, prefix + ".playableTarget", action.TargetPlayableStageId);
                StageCanonicalDigest.Append(builder, prefix + ".uiRouteTarget", (int)action.TargetUiRouteId);
                StageCanonicalDigest.Append(builder, prefix + ".allowedOutcomes", (int)action.AllowedOutcomes);
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public enum StageRunResultCommitPreparationKind
    {
        NotRequired = 0
    }

    public sealed class StageRunResultCommitPreparation
    {
        private static readonly StageRunResultCommitPreparation NotRequiredValue =
            new(StageRunResultCommitPreparationKind.NotRequired);

        private StageRunResultCommitPreparation(StageRunResultCommitPreparationKind kind)
        {
            Kind = kind;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public static StageRunResultCommitPreparation NotRequired => NotRequiredValue;
        public StageRunResultCommitPreparationKind Kind { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(128);
            StageCanonicalDigest.Append(builder, "commitPreparation.kind", (int)Kind);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunResultCommitReceipt
    {
        internal StageRunResultCommitReceipt(
            StageRunResultSummary summary,
            TerminalFinalizationOwnerCoverageRecord coverage,
            StageRunResultCommitPreparation preparation,
            long summaryCommittedAtSequence)
        {
            SchemaVersion = 2;
            CommitReceiptId = $"{summary.Identity.RunId}:result-commit:1";
            RunId = summary.Identity.RunId;
            PlayableStageId = summary.Identity.PlayableStageId;
            RouteRevision = summary.Identity.RouteRevision;
            RouteDigest = summary.Identity.RouteSnapshotDigest;
            ResultSummaryDigest = summary.ResultSummaryDigest;
            TerminalFinalizationOwnerCoverageRecordId =
                coverage.TerminalFinalizationOwnerCoverageRecordId;
            TerminalFinalizationOwnerCoverageDigest = coverage.CanonicalDigest;
            Preparation = preparation ?? throw new ArgumentNullException(nameof(preparation));
            SummaryCommittedAtSequence = summaryCommittedAtSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum();
        }

        internal StageRunResultCommitReceipt(
            int schemaVersion,
            string commitReceiptId,
            string runId,
            string playableStageId,
            int routeRevision,
            string routeDigest,
            string resultSummaryDigest,
            string terminalFinalizationOwnerCoverageRecordId,
            string terminalFinalizationOwnerCoverageDigest,
            StageRunResultCommitPreparation preparation,
            long summaryCommittedAtSequence,
            string canonicalDigest,
            string envelopeChecksum)
        {
            SchemaVersion = schemaVersion;
            CommitReceiptId = commitReceiptId ?? string.Empty;
            RunId = runId ?? string.Empty;
            PlayableStageId = playableStageId ?? string.Empty;
            RouteRevision = routeRevision;
            RouteDigest = routeDigest ?? string.Empty;
            ResultSummaryDigest = resultSummaryDigest ?? string.Empty;
            TerminalFinalizationOwnerCoverageRecordId =
                terminalFinalizationOwnerCoverageRecordId ?? string.Empty;
            TerminalFinalizationOwnerCoverageDigest =
                terminalFinalizationOwnerCoverageDigest ?? string.Empty;
            Preparation = preparation;
            SummaryCommittedAtSequence = summaryCommittedAtSequence;
            CanonicalDigest = canonicalDigest ?? string.Empty;
            EnvelopeChecksum = envelopeChecksum ?? string.Empty;
        }

        public int SchemaVersion { get; }
        public string CommitReceiptId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteDigest { get; }
        public string ResultSummaryDigest { get; }
        public string TerminalFinalizationOwnerCoverageRecordId { get; }
        public string TerminalFinalizationOwnerCoverageDigest { get; }
        public StageRunResultCommitPreparation Preparation { get; }
        public long SummaryCommittedAtSequence { get; }
        public long CommitSequence => SummaryCommittedAtSequence;
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        internal bool HasValidIntegrity()
        {
            return SchemaVersion == 2
                && Preparation != null
                && SummaryCommittedAtSequence > 0
                && string.Equals(CanonicalDigest, ComputeCanonicalDigest(), StringComparison.Ordinal)
                && string.Equals(EnvelopeChecksum, ComputeEnvelopeChecksum(), StringComparison.Ordinal);
        }

        internal string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(768);
            StageCanonicalDigest.Append(builder, "commit.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "commit.id", CommitReceiptId);
            StageCanonicalDigest.Append(builder, "commit.runId", RunId);
            StageCanonicalDigest.Append(builder, "commit.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "commit.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "commit.routeDigest", RouteDigest);
            StageCanonicalDigest.Append(builder, "commit.resultSummaryDigest", ResultSummaryDigest);
            StageCanonicalDigest.Append(
                builder,
                "commit.terminalFinalizationOwnerCoverageId",
                TerminalFinalizationOwnerCoverageRecordId);
            StageCanonicalDigest.Append(
                builder,
                "commit.terminalFinalizationOwnerCoverageDigest",
                TerminalFinalizationOwnerCoverageDigest);
            StageCanonicalDigest.Append(
                builder,
                "commit.preparationKind",
                Preparation != null ? (int)Preparation.Kind : -1);
            StageCanonicalDigest.Append(
                builder,
                "commit.preparationDigest",
                Preparation?.CanonicalDigest);
            StageCanonicalDigest.Append(builder, "commit.sequence", SummaryCommittedAtSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        internal string ComputeEnvelopeChecksum()
        {
            StringBuilder builder = new(256);
            StageCanonicalDigest.Append(builder, "commit.envelope.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "commit.envelope.canonicalDigest", CanonicalDigest);
            StageCanonicalDigest.Append(builder, "commit.envelope.preparationDigest", Preparation?.CanonicalDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunUiRouteTarget
    {
        public StageRunUiRouteTarget(StageUiRouteId routeId, string sceneName, string scenePath)
        {
            RouteId = routeId;
            SceneName = sceneName ?? string.Empty;
            ScenePath = scenePath ?? string.Empty;
        }

        public StageUiRouteId RouteId { get; }
        public string SceneName { get; }
        public string ScenePath { get; }
    }

    public interface IStageRunUiRouteResolver
    {
        bool TryResolve(
            StageUiRouteId routeId,
            out StageRunUiRouteTarget target,
            out string error);
    }

    public interface IStageRunSceneLoader
    {
        StageRunSceneLoadCompletionMode CompletionMode { get; }

        bool TryLoadSingle(
            string sceneName,
            string scenePath,
            out string error);
    }

    public enum StageRunSceneLoadCompletionMode
    {
        RequestAccepted = 1,
        DestinationActivatedSynchronously = 2
    }

    public sealed class StageRunResolvedTerminalAction
    {
        internal StageRunResolvedTerminalAction(
            StageRunResultSummary summary,
            StageRunActionSnapshot action,
            string destinationSceneName,
            string destinationScenePath)
        {
            SelectionId = $"{summary.Identity.RunId}:terminal-action:1";
            RunId = summary.Identity.RunId;
            RouteDigest = summary.Identity.RouteSnapshotDigest;
            ResultSummaryDigest = summary.ResultSummaryDigest;
            ActionId = action.ActionId;
            ActionKind = action.ActionKind;
            Outcome = summary.Outcome;
            TargetPlayableStageId = action.TargetPlayableStageId;
            TargetUiRouteId = action.TargetUiRouteId;
            DestinationSceneName = destinationSceneName ?? string.Empty;
            DestinationScenePath = destinationScenePath ?? string.Empty;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public string SelectionId { get; }
        public string RunId { get; }
        public string RouteDigest { get; }
        public string ResultSummaryDigest { get; }
        public string ActionId { get; }
        public StageRouteActionKind ActionKind { get; }
        public StageRouteOutcome Outcome { get; }
        public string TargetPlayableStageId { get; }
        public StageUiRouteId TargetUiRouteId { get; }
        public string DestinationSceneName { get; }
        public string DestinationScenePath { get; }
        public string CanonicalDigest { get; }

        internal bool HasValidCanonicalDigest()
        {
            return string.Equals(CanonicalDigest, ComputeCanonicalDigest(), StringComparison.Ordinal);
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(512);
            StageCanonicalDigest.Append(builder, "selection.id", SelectionId);
            StageCanonicalDigest.Append(builder, "selection.runId", RunId);
            StageCanonicalDigest.Append(builder, "selection.routeDigest", RouteDigest);
            StageCanonicalDigest.Append(builder, "selection.resultSummaryDigest", ResultSummaryDigest);
            StageCanonicalDigest.Append(builder, "selection.actionId", ActionId);
            StageCanonicalDigest.Append(builder, "selection.actionKind", (int)ActionKind);
            StageCanonicalDigest.Append(builder, "selection.outcome", (int)Outcome);
            StageCanonicalDigest.Append(builder, "selection.targetPlayableStageId", TargetPlayableStageId);
            StageCanonicalDigest.Append(builder, "selection.targetUiRouteId", (int)TargetUiRouteId);
            StageCanonicalDigest.Append(builder, "selection.destinationSceneName", DestinationSceneName);
            StageCanonicalDigest.Append(builder, "selection.destinationScenePath", DestinationScenePath);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

}
