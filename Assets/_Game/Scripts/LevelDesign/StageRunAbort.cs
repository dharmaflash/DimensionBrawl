using System;
using System.Text;
using static DimensionBrawl.LevelDesign.StageRunAbortDigest;

namespace DimensionBrawl.LevelDesign
{
    public enum StageRunAbortOrigin
    {
        DiagnosticAbort = 1,
        TerminalFinalizationFailure = 2
    }

    public enum StageRunAbortReason
    {
        UnexpectedSceneExit = 1,
        WrongHandoffDestination = 2,
        RunReplacedBeforeCommit = 3,
        CoordinatorDiagnostic = 4,
        TerminalFinalizationFailed = 5,
        ExplicitAbort = 6,
        StationFactCollectorLost = 7,
        StationResultPresenterLost = 8,
        TerminalFactAdapterLost = 9,
        TerminalResultPresenterLost = 10,
        SceneHandoffFailed = 11
    }

    public enum StageRunTerminalCoordinatorInvalidationDisposition
    {
        NotBoundBeforeStation = 1,
        CancellationRequested = 2,
        Faulted = 3,
        TerminalAuthorityInvalidated = 4,
        NotBoundBeforeTerminalCoordinator = 5
    }

    public enum StageRunRouteHandoffCoverageDisposition
    {
        NotIssued = 1,
        Succeeded = 2,
        Failed = 3
    }

    public enum StageRunOutcomeFactCoverageDisposition
    {
        NotSealedBeforeAbort = 1,
        SealedDiagnosticOnly = 2
    }

    public enum StageRunClosureOwnerKind
    {
        TutorialLesson = 1,
        TutorialCourse = 2,
        EncounterExecution = 3,
        StageVariability = 4,
        Presentation = 5
    }

    public enum StageRunClosureDisposition
    {
        Succeeded = 1,
        Failed = 2,
        NotAdmitted = 3,
        NotApplicable = 4
    }

    public sealed class StageRunAbortCloseAuthority
    {
        internal StageRunAbortCloseAuthority(
            StageRunIdentity identity,
            StageRunAbortOrigin origin,
            StageRunAbortReason reason,
            StageRunLifecycleState lifecycleStateEnteringAbort,
            TerminalFinalizationAuthority terminalFinalizationAuthority,
            StageRunTerminalCoordinatorInvalidationDisposition coordinatorDisposition,
            long coordinatorRootAdmissionSequence,
            long coordinatorEpoch,
            long coordinatorInvalidationSequence,
            long issuedSequence)
        {
            AbortCloseAuthorityId = $"{identity.RunId}:abort-close-authority:{issuedSequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            Origin = origin;
            AbortReason = reason;
            LifecycleStateEnteringAbort = lifecycleStateEnteringAbort;
            HasTerminalFinalizationAuthority = terminalFinalizationAuthority != null;
            TerminalFinalizationAuthorityId =
                terminalFinalizationAuthority?.TerminalFinalizationAuthorityId ?? string.Empty;
            TerminalFinalizationAuthorityDigest =
                terminalFinalizationAuthority?.CanonicalDigest ?? string.Empty;
            CoordinatorInvalidationDisposition = coordinatorDisposition;
            CoordinatorRootAdmissionSequence = coordinatorRootAdmissionSequence;
            CoordinatorEpoch = coordinatorEpoch;
            CoordinatorInvalidationSequence = coordinatorInvalidationSequence;
            IssuedSequence = issuedSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("stage-run-abort-close-authority", CanonicalDigest);
        }

        public string AbortCloseAuthorityId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public StageRunAbortOrigin Origin { get; }
        public StageRunAbortReason AbortReason { get; }
        public StageRunLifecycleState LifecycleStateEnteringAbort { get; }
        public bool HasTerminalFinalizationAuthority { get; }
        public string TerminalFinalizationAuthorityId { get; }
        public string TerminalFinalizationAuthorityDigest { get; }
        public StageRunTerminalCoordinatorInvalidationDisposition CoordinatorInvalidationDisposition { get; }
        public long CoordinatorRootAdmissionSequence { get; }
        public long CoordinatorEpoch { get; }
        public long CoordinatorInvalidationSequence { get; }
        public long IssuedSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1536);
            StageCanonicalDigest.Append(builder, "abortAuthority.id", AbortCloseAuthorityId);
            StageCanonicalDigest.Append(builder, "abortAuthority.runId", RunId);
            StageCanonicalDigest.Append(builder, "abortAuthority.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "abortAuthority.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "abortAuthority.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "abortAuthority.origin", (int)Origin);
            StageCanonicalDigest.Append(builder, "abortAuthority.reason", (int)AbortReason);
            StageCanonicalDigest.Append(
                builder,
                "abortAuthority.lifecycleState",
                (int)LifecycleStateEnteringAbort);
            StageCanonicalDigest.Append(
                builder,
                "abortAuthority.hasTerminalFinalizationAuthority",
                HasTerminalFinalizationAuthority);
            StageCanonicalDigest.Append(
                builder,
                "abortAuthority.terminalFinalizationAuthorityId",
                TerminalFinalizationAuthorityId);
            StageCanonicalDigest.Append(
                builder,
                "abortAuthority.terminalFinalizationAuthorityDigest",
                TerminalFinalizationAuthorityDigest);
            StageCanonicalDigest.Append(
                builder,
                "abortAuthority.coordinatorDisposition",
                (int)CoordinatorInvalidationDisposition);
            StageCanonicalDigest.Append(
                builder,
                "abortAuthority.coordinatorRootAdmissionSequence",
                CoordinatorRootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "abortAuthority.coordinatorEpoch", CoordinatorEpoch);
            StageCanonicalDigest.Append(
                builder,
                "abortAuthority.coordinatorInvalidationSequence",
                CoordinatorInvalidationSequence);
            StageCanonicalDigest.Append(builder, "abortAuthority.issuedSequence", IssuedSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunRouteHandoffCoverage
    {
        private StageRunRouteHandoffCoverage(
            StageRunRouteHandoffCoverageDisposition disposition,
            StageSegmentHandoffTerminalReceipt receipt)
        {
            Disposition = disposition;
            TerminalReceiptId = receipt?.SegmentHandoffTerminalReceiptId ?? string.Empty;
            TerminalReceiptDigest = receipt?.CanonicalDigest ?? string.Empty;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public StageRunRouteHandoffCoverageDisposition Disposition { get; }
        public string TerminalReceiptId { get; }
        public string TerminalReceiptDigest { get; }
        public string CanonicalDigest { get; }

        internal static StageRunRouteHandoffCoverage NotIssued()
        {
            return new StageRunRouteHandoffCoverage(
                StageRunRouteHandoffCoverageDisposition.NotIssued,
                null);
        }

        internal static StageRunRouteHandoffCoverage Succeeded(
            StageSegmentHandoffTerminalReceipt receipt)
        {
            if (receipt == null)
            {
                throw new ArgumentNullException(nameof(receipt));
            }

            return new StageRunRouteHandoffCoverage(
                StageRunRouteHandoffCoverageDisposition.Succeeded,
                receipt);
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(384);
            StageCanonicalDigest.Append(builder, "handoffCoverage.disposition", (int)Disposition);
            StageCanonicalDigest.Append(builder, "handoffCoverage.receiptId", TerminalReceiptId);
            StageCanonicalDigest.Append(builder, "handoffCoverage.receiptDigest", TerminalReceiptDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunOutcomeFactCoverage
    {
        private StageRunOutcomeFactCoverage(
            StageRunOutcomeFactCoverageDisposition disposition,
            StageOutcomeFact diagnosticFact)
        {
            Disposition = disposition;
            StageOutcomeFactDigest = diagnosticFact?.CanonicalDigest ?? string.Empty;
            OutcomeFactsSealedAtSequence = diagnosticFact?.OutcomeFactsSealedAtSequence ?? 0;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public StageRunOutcomeFactCoverageDisposition Disposition { get; }
        public string StageOutcomeFactDigest { get; }
        public long OutcomeFactsSealedAtSequence { get; }
        public string CanonicalDigest { get; }

        internal static StageRunOutcomeFactCoverage NotSealedBeforeAbort()
        {
            return new StageRunOutcomeFactCoverage(
                StageRunOutcomeFactCoverageDisposition.NotSealedBeforeAbort,
                null);
        }

        internal static StageRunOutcomeFactCoverage SealedDiagnosticOnly(StageOutcomeFact fact)
        {
            if (fact == null)
            {
                throw new ArgumentNullException(nameof(fact));
            }

            return new StageRunOutcomeFactCoverage(
                StageRunOutcomeFactCoverageDisposition.SealedDiagnosticOnly,
                fact);
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(384);
            StageCanonicalDigest.Append(builder, "outcomeCoverage.disposition", (int)Disposition);
            StageCanonicalDigest.Append(builder, "outcomeCoverage.factDigest", StageOutcomeFactDigest);
            StageCanonicalDigest.Append(
                builder,
                "outcomeCoverage.sealedAtSequence",
                OutcomeFactsSealedAtSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunClosureBarrierCoverageRow
    {
        internal StageRunClosureBarrierCoverageRow(
            StageRunClosureOwnerKind ownerKind,
            StageRunClosureDisposition disposition)
        {
            OwnerKind = ownerKind;
            Disposition = disposition;
            ReceiptId = string.Empty;
            ReceiptDigest = string.Empty;
            FaultEvidenceId = string.Empty;
            FaultEvidenceDigest = string.Empty;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public StageRunClosureOwnerKind OwnerKind { get; }
        public StageRunClosureDisposition Disposition { get; }
        public string ReceiptId { get; }
        public string ReceiptDigest { get; }
        public string FaultEvidenceId { get; }
        public string FaultEvidenceDigest { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(384);
            StageCanonicalDigest.Append(builder, "closureRow.ownerKind", (int)OwnerKind);
            StageCanonicalDigest.Append(builder, "closureRow.disposition", (int)Disposition);
            StageCanonicalDigest.Append(builder, "closureRow.receiptId", ReceiptId);
            StageCanonicalDigest.Append(builder, "closureRow.receiptDigest", ReceiptDigest);
            StageCanonicalDigest.Append(builder, "closureRow.faultEvidenceId", FaultEvidenceId);
            StageCanonicalDigest.Append(builder, "closureRow.faultEvidenceDigest", FaultEvidenceDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunAbortRecord
    {
        private readonly StageRunClosureBarrierCoverageRow[] closureRows;

        internal StageRunAbortRecord(
            StageRunIdentity identity,
            StageRunLifecycleState lastLifecycleState,
            StageRunAbortCloseAuthority closeAuthority,
            StageRunRouteHandoffCoverage handoffCoverage,
            StageRunOutcomeFactCoverage outcomeFactCoverage,
            long abortedSequence)
        {
            if (identity == null
                || identity.SchemaVersion != StageRunIdentity.CurrentSchemaVersion
                || closeAuthority == null
                || handoffCoverage == null
                || outcomeFactCoverage == null)
            {
                throw new InvalidOperationException(
                    "Current-schema abort closure requires exact current-run authority and coverage.");
            }

            SchemaVersion = 1;
            AbortRecordId = $"{identity.RunId}:abort-record:{abortedSequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            LastLifecycleState = lastLifecycleState;
            AbortReason = closeAuthority.AbortReason;
            AbortCloseAuthorityId = closeAuthority.AbortCloseAuthorityId;
            AbortCloseAuthorityDigest = closeAuthority.CanonicalDigest;
            CoordinatorRootAdmissionSequence = closeAuthority.CoordinatorRootAdmissionSequence;
            CoordinatorEpoch = closeAuthority.CoordinatorEpoch;
            RouteHandoffCoverage = handoffCoverage;
            OutcomeFactCoverage = outcomeFactCoverage;
            closureRows = new[]
            {
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.TutorialLesson,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.TutorialCourse,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.EncounterExecution,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.StageVariability,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.Presentation,
                    StageRunClosureDisposition.NotAdmitted)
            };
            PendingClosureOwnerCount = 0;
            AggregateClosureDigest = ComputeAggregateClosureDigest();
            AbortedSequence = abortedSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("stage-run-abort-record", CanonicalDigest);
        }

        public int SchemaVersion { get; }
        public string AbortRecordId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public StageRunLifecycleState LastLifecycleState { get; }
        public StageRunAbortReason AbortReason { get; }
        public string AbortCloseAuthorityId { get; }
        public string AbortCloseAuthorityDigest { get; }
        public long CoordinatorRootAdmissionSequence { get; }
        public long CoordinatorEpoch { get; }
        public StageRunRouteHandoffCoverage RouteHandoffCoverage { get; }
        public StageRunOutcomeFactCoverage OutcomeFactCoverage { get; }
        public int ClosureBarrierCount => closureRows.Length;
        public int PendingClosureOwnerCount { get; }
        public string AggregateClosureDigest { get; }
        public long AbortedSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        public StageRunClosureBarrierCoverageRow GetClosureBarrier(int index)
        {
            if (index < 0 || index >= closureRows.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return closureRows[index];
        }

        public bool HasValidIntegrity()
        {
            return SchemaVersion == StageRunIdentity.CurrentSchemaVersion
                && closureRows.Length == 5
                && PendingClosureOwnerCount == 0
                && string.Equals(
                    AggregateClosureDigest,
                    ComputeAggregateClosureDigest(),
                    StringComparison.Ordinal)
                && string.Equals(CanonicalDigest, ComputeCanonicalDigest(), StringComparison.Ordinal)
                && string.Equals(
                    EnvelopeChecksum,
                    ComputeEnvelopeChecksum("stage-run-abort-record", CanonicalDigest),
                    StringComparison.Ordinal);
        }

        private string ComputeAggregateClosureDigest()
        {
            StringBuilder builder = new(1536);
            StageCanonicalDigest.Append(builder, "aggregateClosure.runId", RunId);
            StageCanonicalDigest.Append(builder, "aggregateClosure.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "aggregateClosure.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "aggregateClosure.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "aggregateClosure.restartDispatchPresent", false);
            StageCanonicalDigest.Append(builder, "aggregateClosure.restartDispatchId", string.Empty);
            StageCanonicalDigest.Append(builder, "aggregateClosure.restartDispatchDigest", string.Empty);
            StageCanonicalDigest.Append(builder, "aggregateClosure.abortCloseAuthorityPresent", true);
            StageCanonicalDigest.Append(
                builder,
                "aggregateClosure.abortCloseAuthorityId",
                AbortCloseAuthorityId);
            StageCanonicalDigest.Append(
                builder,
                "aggregateClosure.abortCloseAuthorityDigest",
                AbortCloseAuthorityDigest);
            StageCanonicalDigest.Append(
                builder,
                "aggregateClosure.routeHandoffCoverage",
                RouteHandoffCoverage.CanonicalDigest);
            StageCanonicalDigest.Append(builder, "aggregateClosure.count", closureRows.Length);
            for (int i = 0; i < closureRows.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"aggregateClosure.row[{i}]",
                    closureRows[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(builder, "aggregateClosure.pendingCount", PendingClosureOwnerCount);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "abort.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "abort.id", AbortRecordId);
            StageCanonicalDigest.Append(builder, "abort.runId", RunId);
            StageCanonicalDigest.Append(builder, "abort.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "abort.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "abort.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "abort.lastLifecycleState", (int)LastLifecycleState);
            StageCanonicalDigest.Append(builder, "abort.reason", (int)AbortReason);
            StageCanonicalDigest.Append(builder, "abort.closeAuthorityId", AbortCloseAuthorityId);
            StageCanonicalDigest.Append(builder, "abort.closeAuthorityDigest", AbortCloseAuthorityDigest);
            StageCanonicalDigest.Append(
                builder,
                "abort.coordinatorRootAdmissionSequence",
                CoordinatorRootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "abort.coordinatorEpoch", CoordinatorEpoch);
            StageCanonicalDigest.Append(
                builder,
                "abort.routeHandoffCoverage",
                RouteHandoffCoverage.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "abort.outcomeFactCoverage",
                OutcomeFactCoverage.CanonicalDigest);
            StageCanonicalDigest.Append(builder, "abort.aggregateClosureDigest", AggregateClosureDigest);
            StageCanonicalDigest.Append(builder, "abort.abortedSequence", AbortedSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public enum StageDispatchClosureFailedBoundary
    {
        SceneLoad = 1,
        ClosureIntegrity = 2,
        UnexpectedSceneExit = 3
    }

    public sealed class StageDispatchClosureFaultRecord
    {
        private readonly StageRunClosureBarrierCoverageRow[] closureRows;

        internal StageDispatchClosureFaultRecord(
            StageRunResultSummary summary,
            StageRunResolvedTerminalAction selection,
            StageDispatchClosureFailedBoundary failedBoundary,
            string faultMessage,
            long faultSequence)
        {
            if (summary == null
                || summary.Identity == null
                || summary.RouteSnapshot == null
                || summary.Identity.SchemaVersion != StageRunIdentity.CurrentSchemaVersion
                || summary.RouteSnapshot.SchemaVersion != StageRunIdentity.CurrentSchemaVersion
                || selection == null)
            {
                throw new InvalidOperationException(
                    "Current-schema dispatch closure cannot fabricate future-schema NotAdmitted rows.");
            }

            DispatchClosureFaultRecordId =
                $"{summary.Identity.RunId}:dispatch-closure-fault:{faultSequence}";
            RunId = summary.Identity.RunId;
            PlayableStageId = summary.Identity.PlayableStageId;
            RouteRevision = summary.Identity.RouteRevision;
            RouteSnapshotDigest = summary.Identity.RouteSnapshotDigest;
            ResultSummaryDigest = summary.ResultSummaryDigest;
            TerminalActionSelectionId = selection.SelectionId;
            TerminalActionSelectionDigest = selection.CanonicalDigest;
            FailedBoundary = failedBoundary;
            FaultMessage = faultMessage ?? string.Empty;
            StageVariabilitySemanticDigest = string.Empty;
            TutorialCourseSemanticDigest = string.Empty;
            closureRows = new[]
            {
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.TutorialLesson,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.TutorialCourse,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.EncounterExecution,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.StageVariability,
                    StageRunClosureDisposition.NotAdmitted),
                new StageRunClosureBarrierCoverageRow(
                    StageRunClosureOwnerKind.Presentation,
                    StageRunClosureDisposition.NotAdmitted)
            };
            PendingClosureOwnerCount = 0;
            AggregateClosureDigest = ComputeAggregateClosureDigest();
            FaultSequence = faultSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum(
                "stage-dispatch-closure-fault-record",
                CanonicalDigest);
        }

        public string DispatchClosureFaultRecordId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public string ResultSummaryDigest { get; }
        public string TerminalActionSelectionId { get; }
        public string TerminalActionSelectionDigest { get; }
        public StageDispatchClosureFailedBoundary FailedBoundary { get; }
        public string FaultMessage { get; }
        public string StageVariabilitySemanticDigest { get; }
        public string TutorialCourseSemanticDigest { get; }
        public int ClosureBarrierCount => closureRows.Length;
        public int PendingClosureOwnerCount { get; }
        public string AggregateClosureDigest { get; }
        public long FaultSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        public StageRunClosureBarrierCoverageRow GetClosureBarrier(int index)
        {
            if (index < 0 || index >= closureRows.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return closureRows[index];
        }

        public bool HasValidIntegrity()
        {
            return closureRows.Length == 5
                && PendingClosureOwnerCount == 0
                && string.Equals(
                    AggregateClosureDigest,
                    ComputeAggregateClosureDigest(),
                    StringComparison.Ordinal)
                && string.Equals(CanonicalDigest, ComputeCanonicalDigest(), StringComparison.Ordinal)
                && string.Equals(
                    EnvelopeChecksum,
                    ComputeEnvelopeChecksum(
                        "stage-dispatch-closure-fault-record",
                        CanonicalDigest),
                    StringComparison.Ordinal);
        }

        private string ComputeAggregateClosureDigest()
        {
            StringBuilder builder = new(768);
            StageCanonicalDigest.Append(builder, "dispatchClosure.count", closureRows.Length);
            for (int i = 0; i < closureRows.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"dispatchClosure.row[{i}]",
                    closureRows[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(
                builder,
                "dispatchClosure.pendingCount",
                PendingClosureOwnerCount);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "dispatchFault.id", DispatchClosureFaultRecordId);
            StageCanonicalDigest.Append(builder, "dispatchFault.runId", RunId);
            StageCanonicalDigest.Append(builder, "dispatchFault.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "dispatchFault.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "dispatchFault.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "dispatchFault.resultSummaryDigest", ResultSummaryDigest);
            StageCanonicalDigest.Append(builder, "dispatchFault.selectionId", TerminalActionSelectionId);
            StageCanonicalDigest.Append(
                builder,
                "dispatchFault.selectionDigest",
                TerminalActionSelectionDigest);
            StageCanonicalDigest.Append(builder, "dispatchFault.failedBoundary", (int)FailedBoundary);
            StageCanonicalDigest.Append(builder, "dispatchFault.faultMessage", FaultMessage);
            StageCanonicalDigest.Append(
                builder,
                "dispatchFault.stageVariabilitySemanticDigest",
                StageVariabilitySemanticDigest);
            StageCanonicalDigest.Append(
                builder,
                "dispatchFault.tutorialCourseSemanticDigest",
                TutorialCourseSemanticDigest);
            StageCanonicalDigest.Append(
                builder,
                "dispatchFault.aggregateClosureDigest",
                AggregateClosureDigest);
            StageCanonicalDigest.Append(builder, "dispatchFault.sequence", FaultSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    internal static class StageRunAbortDigest
    {
        public static string ComputeEnvelopeChecksum(string kind, string canonicalDigest)
        {
            StringBuilder builder = new(192);
            StageCanonicalDigest.Append(builder, "envelope.kind", kind);
            StageCanonicalDigest.Append(builder, "envelope.canonicalDigest", canonicalDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }
}
