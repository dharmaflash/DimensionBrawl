using System;
using DimensionBrawl.Combat;
using DimensionBrawl.UI.StageClear;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace DimensionBrawl.LevelDesign
{
    public enum StageRunLifecycleState
    {
        Created = 0,
        CorridorActive = 1,
        HandoffPending = 2,
        StationActive = 3,
        TerminalFinalizing = 4,
        TerminalFinalizationOwnersSealed = 5,
        OutcomeFactsSealed = 6,
        CommitRequested = 7,
        Committed = 8,
        Presented = 9,
        CommitRecoveryPending = 10,
        AbortClosing = 20,
        Aborted = 21,
        Faulted = 90,
        CommitPersistenceFaulted = 91,
        ClosureFaulted = 92,
        Disposed = 100
    }

    public enum StageTerminalOrRestartLatchState
    {
        Open = 0,
        TerminalWon = 1
    }

    public sealed class StageRunIdentity
    {
        public const int CurrentSchemaVersion = 1;

        internal StageRunIdentity(string runId, StageRunRouteSnapshot routeSnapshot)
        {
            SchemaVersion = routeSnapshot.SchemaVersion;
            RunId = runId;
            PlayableStageId = routeSnapshot.PlayableStageId;
            RouteRevision = routeSnapshot.RouteRevision;
            RouteSnapshotDigest = routeSnapshot.CanonicalDigest;
            EntrySegmentId = routeSnapshot.GetSegment(0).SegmentId;
        }

        public int SchemaVersion { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public string EntrySegmentId { get; }
    }

    public sealed class StageRunContext
    {
        private StageRunSingleLoadDispatch pendingDispatch;
        private readonly StageRunFactAccumulator factAccumulator;
        private int currentSegmentIndex;
        private int currentSceneHandle;
        private long handoffSequence;
        private long loaderGeneration;
        private long handoffEvidenceSequence;
        private long finalizationSequence;
        private long abortSequence;
        private long dispatchFaultSequence;
        private StageSegmentEntryReceipt segmentEntryReceipt;
        private StageSegmentHandoffTerminalReceipt handoffTerminalReceipt;
        private StageRunTerminalRecord terminalRecord;
        private TerminalEpochClosureRecord terminalEpochClosureRecord;
        private TerminalFinalizationAuthority terminalFinalizationAuthority;
        private TerminalFinalizationOwnerCoverageRecord ownerCoverageRecord;
        private StageRunResultSummary commitCandidateSummary;
        private TerminalFinalizationOwnerCoverageRecord commitCandidateCoverage;
        private StageRunResultCommitPreparation commitPreparation;
        private StageRunResultSummary committedSummary;
        private StageRunResultCommitReceipt commitReceipt;
        private StageRunResolvedTerminalAction selectedTerminalAction;
        private StageResultPresentationSnapshot resultPresentationSnapshot;
        private StageResultPresentationAuditEnvelope resultPresentationAudit;
        private StageOutcomeFact diagnosticOutcomeFactCandidate;
        private StageRunAbortCloseAuthority abortCloseAuthority;
        private StageRunAbortRecord abortRecord;
        private StageDispatchClosureFaultRecord dispatchClosureFaultRecord;
        private bool terminalActionDispatchStarted;
        private StageTerminalOrRestartLatchState terminalOrRestartLatch;

        internal StageRunContext(
            string runId,
            StageRunRouteSnapshot routeSnapshot,
            StageRunResultProgressionJoinSnapshot resultProgressionJoinSnapshot,
            int entrySceneHandle)
        {
            RouteSnapshot = routeSnapshot ?? throw new ArgumentNullException(nameof(routeSnapshot));
            ResultProgressionJoinSnapshot = resultProgressionJoinSnapshot
                ?? throw new ArgumentNullException(nameof(resultProgressionJoinSnapshot));
            Identity = new StageRunIdentity(runId, routeSnapshot);
            factAccumulator = new StageRunFactAccumulator(Identity, routeSnapshot);
            LifecycleState = StageRunLifecycleState.Created;
            currentSegmentIndex = 0;
            currentSceneHandle = entrySceneHandle;
        }

        public StageRunIdentity Identity { get; }
        public StageRunRouteSnapshot RouteSnapshot { get; }
        public StageRunResultProgressionJoinSnapshot ResultProgressionJoinSnapshot { get; }
        public StageRunLifecycleState LifecycleState { get; private set; }
        public StageRunSegmentSnapshot CurrentSegment => RouteSnapshot.GetSegment(currentSegmentIndex);
        public StageRunHandoffToken PendingHandoffToken => pendingDispatch?.Token;
        public StageSegmentEntryReceipt SegmentEntryReceipt => segmentEntryReceipt;
        public StageSegmentHandoffTerminalReceipt HandoffTerminalReceipt =>
            handoffTerminalReceipt;
        public int CurrentSceneHandle => currentSceneHandle;
        public string FaultReason { get; private set; } = string.Empty;
        public StageRunTerminalRecord TerminalRecord => terminalRecord;
        public TerminalEpochClosureRecord TerminalEpochClosureRecord => terminalEpochClosureRecord;
        public TerminalFinalizationAuthority TerminalFinalizationAuthority =>
            terminalFinalizationAuthority;
        public TerminalFinalizationOwnerCoverageRecord OwnerCoverageRecord => ownerCoverageRecord;
        public StageTerminalOrRestartLatchState TerminalOrRestartLatch => terminalOrRestartLatch;
        public StageRunResultSummary CommittedSummary => committedSummary;
        public StageRunResultCommitReceipt CommitReceipt => commitReceipt;
        public StageRunResolvedTerminalAction SelectedTerminalAction => selectedTerminalAction;
        public StageResultPresentationSnapshot ResultPresentationSnapshot =>
            resultPresentationSnapshot;
        public StageResultPresentationAuditEnvelope ResultPresentationAudit =>
            resultPresentationAudit;
        public StageRunAbortCloseAuthority AbortCloseAuthority => abortCloseAuthority;
        public StageRunAbortRecord AbortRecord => abortRecord;
        public StageDispatchClosureFaultRecord DispatchClosureFaultRecord =>
            dispatchClosureFaultRecord;
        public StageTutorialRouteSummaryFact TutorialRouteSummaryFact => factAccumulator.TutorialRouteSummary;
        public int TerminalRecordReceiptCount { get; private set; }

        internal void ActivateFirstSegment()
        {
            if (LifecycleState != StageRunLifecycleState.Created)
            {
                throw new InvalidOperationException("Stage run first segment was already activated.");
            }

            factAccumulator.ActivateFirstSegment();
            LifecycleState = StageRunLifecycleState.CorridorActive;
        }

        public bool TryPulseActiveTime(
            double realtimeSeconds,
            bool isFocused,
            bool isExplicitlyPaused,
            bool combatEligible,
            bool forwardRiskEligible,
            out string error)
        {
            return factAccumulator.TryPulse(
                realtimeSeconds,
                LifecycleState,
                isFocused,
                isExplicitlyPaused,
                combatEligible,
                forwardRiskEligible,
                out error);
        }

        public bool TrySealTutorialRouteCompletion(out string error)
        {
            error = string.Empty;
            if (LifecycleState != StageRunLifecycleState.CorridorActive)
            {
                error = $"Tutorial route facts cannot seal from {LifecycleState}.";
                return false;
            }

            return factAccumulator.TrySealTutorialRouteCompletion(out error);
        }

        internal bool TryBindStationFactCollector(int sceneHandle, out string error)
        {
            error = string.Empty;
            if (LifecycleState != StageRunLifecycleState.StationActive || sceneHandle != currentSceneHandle)
            {
                error = "Station fact collector does not belong to the active Station run scene.";
                return false;
            }

            return factAccumulator.TryBindStationCollector(out error);
        }

        internal bool TryMarkStationGuideReleased(out string error)
        {
            if (LifecycleState != StageRunLifecycleState.StationActive)
            {
                error = $"Station guide release cannot be recorded from {LifecycleState}.";
                return false;
            }

            return factAccumulator.TryMarkStationGuideReleased(out error);
        }

        internal bool TryRecordResolvedPlayerDamage(float amount, out string error)
        {
            return factAccumulator.TryRecordPlayerDamage(amount, out error);
        }

        internal bool TryRecordPlayerDown(out string error)
        {
            return factAccumulator.TryRecordPlayerDown(out error);
        }

        internal bool TryRecordPerfectDodge(out string error)
        {
            return factAccumulator.TryRecordPerfectDodge(out error);
        }

        internal bool TryRecordSummonUse(string slotRoleId, int spentTier, out string error)
        {
            return factAccumulator.TryRecordSummonUse(slotRoleId, spentTier, out error);
        }

        internal bool TryRecordSemanticProof(
            string proofId,
            string sourceKind,
            double actualValue,
            bool qualified,
            out string error)
        {
            return factAccumulator.TryRecordSemanticProof(
                proofId,
                sourceKind,
                actualValue,
                qualified,
                out error);
        }

        public bool TrySealCurrentSegmentForSingleLoad(
            string completionConditionId,
            out StageRunSingleLoadDispatch dispatch,
            out string error)
        {
            dispatch = null;
            error = string.Empty;
            if (LifecycleState == StageRunLifecycleState.HandoffPending
                && pendingDispatch != null
                && string.Equals(
                    pendingDispatch.Token.ConditionId,
                    completionConditionId,
                    StringComparison.Ordinal))
            {
                dispatch = pendingDispatch;
                return true;
            }

            if (LifecycleState != StageRunLifecycleState.CorridorActive)
            {
                error = $"Stage run cannot seal a single-load handoff from {LifecycleState}.";
                return false;
            }

            StageRunSegmentSnapshot source = CurrentSegment;
            if (!string.Equals(source.ExitConditionId, completionConditionId, StringComparison.Ordinal))
            {
                return Fault(
                    $"Handoff condition mismatch. expected={source.ExitConditionId}, actual={completionConditionId}.",
                    out error);
            }

            int destinationIndex = currentSegmentIndex + 1;
            if (source.HandoffPolicy != StageSceneHandoffPolicy.SingleLoad
                || source.SuccessorKind != StageSegmentSuccessorKind.NextOrderedSegment
                || source.DestinationSceneKind
                    != StageSegmentDestinationSceneKind.SuccessorStageDefinitionScene
                || source.TransitionTokenKind
                    != StageSegmentTransitionTokenKind.SealedCurrentRunSegmentHandoff
                || source.LoaderGenerationKind
                    != StageSegmentLoaderGenerationKind.ActiveRunRouteLoaderGeneration
                || source.NavigationAuthorityKind
                    != StageSegmentNavigationAuthorityKind.P1AStageRunRouteOwner
                || destinationIndex >= RouteSnapshot.SegmentCount)
            {
                return Fault("Current route segment is not a valid P1-A SingleLoad boundary.", out error);
            }

            StageRunSegmentSnapshot destination = RouteSnapshot.GetSegment(destinationIndex);
            if (!string.Equals(
                source.ExitConditionId,
                destination.EntryConditionId,
                StringComparison.Ordinal)
                || source.ExitConditionKind != destination.EntryConditionKind)
            {
                return Fault("Source exit and destination entry conditions do not match.", out error);
            }

            if (!factAccumulator.TryCompleteCurrentSegment(out error))
            {
                return Fault(error, out error);
            }

            long sequence = ++handoffSequence;
            var token = new StageRunHandoffToken(
                Identity,
                sequence,
                ++loaderGeneration,
                ++handoffEvidenceSequence,
                source,
                destination);
            pendingDispatch = new StageRunSingleLoadDispatch(token);
            LifecycleState = StageRunLifecycleState.HandoffPending;
            dispatch = pendingDispatch;
            return true;
        }

        public bool TryAdvanceCurrentSegmentInScene(
            string completionConditionId,
            Scene hostScene,
            out StageSegmentEntryReceipt receipt,
            out string error)
        {
            receipt = null;
            error = string.Empty;
            if (LifecycleState == StageRunLifecycleState.StationActive
                && currentSegmentIndex > 0
                && currentSceneHandle == hostScene.handle
                && segmentEntryReceipt != null
                && handoffTerminalReceipt != null
                && !segmentEntryReceipt.FromHandoffPending
                && handoffTerminalReceipt.LoaderGeneration == 0
                && !handoffTerminalReceipt.LoaderGenerationInvalidated
                && string.Equals(
                    RouteSnapshot.GetSegment(currentSegmentIndex - 1).ExitConditionId,
                    completionConditionId,
                    StringComparison.Ordinal))
            {
                receipt = segmentEntryReceipt;
                return true;
            }

            if (LifecycleState != StageRunLifecycleState.CorridorActive)
            {
                error = $"Stage run cannot advance in-scene from {LifecycleState}.";
                return false;
            }

            StageRunSegmentSnapshot source = CurrentSegment;
            if (!string.Equals(source.ExitConditionId, completionConditionId, StringComparison.Ordinal))
            {
                return Fault(
                    $"In-scene condition mismatch. expected={source.ExitConditionId}, actual={completionConditionId}.",
                    out error);
            }

            int destinationIndex = currentSegmentIndex + 1;
            if (source.HandoffPolicy != StageSceneHandoffPolicy.InSceneAdvance
                || source.SuccessorKind != StageSegmentSuccessorKind.NextOrderedSegment
                || source.DestinationSceneKind
                    != StageSegmentDestinationSceneKind.SuccessorStageDefinitionScene
                || source.TransitionTokenKind
                    != StageSegmentTransitionTokenKind.SealedCurrentRunSegmentHandoff
                || source.LoaderGenerationKind != StageSegmentLoaderGenerationKind.None
                || source.NavigationAuthorityKind
                    != StageSegmentNavigationAuthorityKind.P1AStageRunRouteOwner
                || source.ReturnOwnerKind != StageSegmentReturnOwnerKind.None
                || source.ReturnOwnerReceiptPolicy != StageReturnOwnerReceiptPolicy.None
                || destinationIndex >= RouteSnapshot.SegmentCount)
            {
                return Fault("Current route segment is not a valid in-scene advance boundary.", out error);
            }

            StageRunSegmentSnapshot destination = RouteSnapshot.GetSegment(destinationIndex);
            string hostPath = NormalizePath(hostScene.path);
            if (!hostScene.IsValid()
                || !hostScene.isLoaded
                || hostScene.handle != currentSceneHandle
                || string.IsNullOrWhiteSpace(hostPath)
                || !string.Equals(hostPath, source.ScenePath, StringComparison.Ordinal)
                || !string.Equals(hostPath, destination.ScenePath, StringComparison.Ordinal)
                || !string.Equals(
                    source.ExitConditionId,
                    destination.EntryConditionId,
                    StringComparison.Ordinal)
                || source.ExitConditionKind != destination.EntryConditionKind)
            {
                return Fault(
                    "In-scene destination does not match the active run, host scene, or condition boundary.",
                    out error);
            }

            if (!factAccumulator.TryCompleteCurrentSegment(out error))
            {
                return Fault(error, out error);
            }

            var token = new StageRunHandoffToken(
                Identity,
                ++handoffSequence,
                loaderGeneration: 0,
                issuedSequence: ++handoffEvidenceSequence,
                source: source,
                destination: destination);
            var candidateEntryReceipt = new StageSegmentEntryReceipt(
                Identity,
                token,
                destination,
                hostScene,
                fromHandoffPending: false,
                entrySequence: ++handoffEvidenceSequence);
            var candidateTerminalReceipt = new StageSegmentHandoffTerminalReceipt(
                Identity,
                token,
                candidateEntryReceipt,
                loaderGenerationInvalidated: false,
                terminalSequence: ++handoffEvidenceSequence);
            currentSegmentIndex = destinationIndex;
            segmentEntryReceipt = candidateEntryReceipt;
            handoffTerminalReceipt = candidateTerminalReceipt;
            factAccumulator.EnterSegment(currentSegmentIndex);
            LifecycleState = StageRunLifecycleState.StationActive;
            receipt = candidateEntryReceipt;
            return true;
        }

        internal bool TryEnterPendingSegment(Scene scene, out string error)
        {
            error = string.Empty;
            if (LifecycleState == StageRunLifecycleState.StationActive
                && currentSceneHandle == scene.handle
                && string.Equals(CurrentSegment.ScenePath, NormalizePath(scene.path), StringComparison.Ordinal)
                && segmentEntryReceipt != null
                && handoffTerminalReceipt != null)
            {
                return true;
            }

            if (LifecycleState != StageRunLifecycleState.HandoffPending || pendingDispatch == null)
            {
                error = $"Stage run has no pending handoff for scene {scene.path}.";
                return false;
            }

            string loadedPath = NormalizePath(scene.path);
            StageRunHandoffToken token = pendingDispatch.Token;
            StageRunSegmentSnapshot destination = RouteSnapshot.GetSegment(currentSegmentIndex + 1);
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(loadedPath, pendingDispatch.DestinationScenePath, StringComparison.Ordinal)
                || token.LoaderGeneration <= 0
                || token.LoaderGeneration != pendingDispatch.LoaderGeneration
                || !string.Equals(
                    token.DestinationSceneName,
                    pendingDispatch.DestinationSceneName,
                    StringComparison.Ordinal)
                || !string.Equals(
                    token.DestinationScenePath,
                    pendingDispatch.DestinationScenePath,
                    StringComparison.Ordinal)
                || !string.Equals(token.RunId, Identity.RunId, StringComparison.Ordinal)
                || !string.Equals(token.PlayableStageId, Identity.PlayableStageId, StringComparison.Ordinal)
                || token.RouteRevision != Identity.RouteRevision
                || !string.Equals(token.RouteDigest, Identity.RouteSnapshotDigest, StringComparison.Ordinal)
                || !string.Equals(token.SourceSegmentId, CurrentSegment.SegmentId, StringComparison.Ordinal)
                || !string.Equals(
                    token.SourceStageDefinitionId,
                    CurrentSegment.StageDefinitionId,
                    StringComparison.Ordinal)
                || !string.Equals(token.DestinationSegmentId, destination.SegmentId, StringComparison.Ordinal)
                || !string.Equals(
                    token.DestinationStageDefinitionId,
                    destination.StageDefinitionId,
                    StringComparison.Ordinal)
                || !string.Equals(token.ConditionId, destination.EntryConditionId, StringComparison.Ordinal))
            {
                return Fault("Loaded segment does not match the active run snapshot and handoff token.", out error);
            }

            var candidateEntryReceipt = new StageSegmentEntryReceipt(
                Identity,
                token,
                destination,
                scene,
                ++handoffEvidenceSequence);
            var candidateTerminalReceipt = new StageSegmentHandoffTerminalReceipt(
                Identity,
                token,
                candidateEntryReceipt,
                ++handoffEvidenceSequence);
            currentSegmentIndex++;
            currentSceneHandle = scene.handle;
            segmentEntryReceipt = candidateEntryReceipt;
            handoffTerminalReceipt = candidateTerminalReceipt;
            pendingDispatch = null;
            factAccumulator.EnterSegment(currentSegmentIndex);
            LifecycleState = StageRunLifecycleState.StationActive;
            return true;
        }

        internal bool TryCommitTerminalResolution(
            EncounterTerminalResolution resolution,
            EncounterTerminalEpochEvidence terminalEpochEvidence,
            EncounterTerminalCoordinatorState coordinatorState,
            out StageRunResultSummary summary,
            out StageRunResultCommitReceipt receipt,
            out string error)
        {
            summary = null;
            receipt = null;
            error = string.Empty;
            if (committedSummary != null)
            {
                if (terminalRecord != null
                    && terminalRecord.Matches(resolution)
                    && terminalEpochClosureRecord != null
                    && (terminalEpochEvidence == null
                        || terminalEpochClosureRecord.Matches(terminalEpochEvidence)))
                {
                    if (!StageRunResultCommitStore.TryCommit(
                        committedSummary,
                        ownerCoverageRecord,
                        commitPreparation ?? StageRunResultCommitPreparation.NotRequired,
                        out StageRunResultSummary storedSummary,
                        out TerminalFinalizationOwnerCoverageRecord storedCoverage,
                        out StageRunResultCommitReceipt storedReceipt,
                        out _,
                        out error))
                    {
                        return false;
                    }

                    committedSummary = storedSummary;
                    ownerCoverageRecord = storedCoverage;
                    commitReceipt = storedReceipt;
                    summary = committedSummary;
                    receipt = commitReceipt;
                    return true;
                }

                error = "A different terminal record attempted to replace the committed run result.";
                return false;
            }

            if (LifecycleState != StageRunLifecycleState.StationActive)
            {
                error = $"Terminal commit is not legal from {LifecycleState}.";
                return false;
            }

            StageRunSegmentSnapshot segment = CurrentSegment;
            if (segment.HandoffPolicy != StageSceneHandoffPolicy.ReturnToOwner
                || segment.SuccessorKind != StageSegmentSuccessorKind.None
                || segment.DestinationSceneKind != StageSegmentDestinationSceneKind.None
                || segment.TransitionTokenKind != StageSegmentTransitionTokenKind.None
                || segment.LoaderGenerationKind != StageSegmentLoaderGenerationKind.None
                || segment.NavigationAuthorityKind != StageSegmentNavigationAuthorityKind.None
                || segment.ReturnOwnerKind != StageSegmentReturnOwnerKind.P1AStageRunRouteOwner
                || segment.ReturnOwnerReceiptPolicy
                    != StageReturnOwnerReceiptPolicy.ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented)
            {
                return AbortTerminalFinalizationFailure(
                    "Station segment does not carry the frozen ReturnToOwner contract.",
                    resolution,
                    out error);
            }

            if (coordinatorState != EncounterTerminalCoordinatorState.TerminalClosed
                || terminalEpochEvidence == null
                || resolution.RunGeneration <= 0
                || resolution.RootAdmissionSequence <= 0
                || resolution.Epoch <= 0
                || (resolution.Outcome != EncounterTerminalOutcome.Clear
                    && resolution.Outcome != EncounterTerminalOutcome.Fail))
            {
                return AbortTerminalFinalizationFailure(
                    "Terminal resolution is not a valid current-run TerminalClosed record.",
                    resolution,
                    out error);
            }

            if (!TerminalEpochClosureRecord.TryCreate(
                    Identity,
                    RouteSnapshot,
                    terminalEpochEvidence,
                    out TerminalEpochClosureRecord candidateClosure,
                    out error)
                || candidateClosure.CoordinatorRunGeneration != resolution.RunGeneration
                || candidateClosure.RootAdmissionSequence != resolution.RootAdmissionSequence
                || candidateClosure.TerminalEpoch != resolution.Epoch)
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Terminal epoch closure does not match the current terminal resolution.";
                }

                return AbortTerminalFinalizationFailure(error, resolution, out error);
            }

            StageRunTerminalPolicySnapshot policy = RouteSnapshot.TerminalPolicy;
            if ((resolution.Outcome == EncounterTerminalOutcome.Clear
                    && policy.RequiresBossCandidateAndFinalDead
                    && !resolution.BossDead)
                || (resolution.Outcome == EncounterTerminalOutcome.Fail
                    && policy.RequiresPlayerCandidateAndFinalDown
                    && !resolution.PlayerDown))
            {
                return AbortTerminalFinalizationFailure(
                    "Terminal candidate and final subject state do not match the frozen policy.",
                    resolution,
                    out error);
            }

            if (TerminalRecordReceiptCount != 0)
            {
                return AbortTerminalFinalizationFailure(
                    "The route/run owner already received a terminal record.",
                    resolution,
                    out error);
            }

            if (terminalOrRestartLatch != StageTerminalOrRestartLatchState.Open)
            {
                return AbortTerminalFinalizationFailure(
                    "The terminal-or-restart latch is already sealed.",
                    resolution,
                    out error);
            }

            TerminalRecordReceiptCount = 1;
            var candidateTerminal = new StageRunTerminalRecord(Identity, resolution);
            terminalEpochClosureRecord = candidateClosure;
            terminalRecord = candidateTerminal;
            terminalOrRestartLatch = StageTerminalOrRestartLatchState.TerminalWon;
            terminalFinalizationAuthority = new TerminalFinalizationAuthority(
                Identity,
                candidateClosure,
                ++finalizationSequence);
            LifecycleState = StageRunLifecycleState.TerminalFinalizing;
            if (!factAccumulator.TrySealTerminalFacts(
                    resolution,
                    out StageRunFactBundle factBundle,
                    out error))
            {
                return AbortTerminalFinalizationFailure(error, resolution, out error);
            }

            diagnosticOutcomeFactCandidate = factBundle.Outcome;
            long ownerCoverageSequence = ++finalizationSequence;
            if (!TerminalFinalizationOwnerCoverageRecord.TryCreateCurrentSnapshot(
                    Identity,
                    terminalFinalizationAuthority,
                    StageTerminalFinalizationContext.NonCourseStationTerminal,
                    ownerCoverageSequence,
                    out TerminalFinalizationOwnerCoverageRecord candidateCoverage,
                    out error))
            {
                return AbortTerminalFinalizationFailure(error, resolution, out error);
            }

            ownerCoverageRecord = candidateCoverage;
            LifecycleState = StageRunLifecycleState.TerminalFinalizationOwnersSealed;
            LifecycleState = StageRunLifecycleState.OutcomeFactsSealed;
            var candidateSummary = new StageRunResultSummary(
                Identity,
                RouteSnapshot,
                candidateTerminal,
                factBundle);
            commitCandidateSummary = candidateSummary;
            commitCandidateCoverage = candidateCoverage;
            commitPreparation = StageRunResultCommitPreparation.NotRequired;
            LifecycleState = StageRunLifecycleState.CommitRequested;
            return TryCommitCandidate(out summary, out receipt, out error);
        }

        private bool AbortTerminalFinalizationFailure(
            string failure,
            EncounterTerminalResolution resolution,
            out string error)
        {
            string failureMessage = string.IsNullOrWhiteSpace(failure)
                ? "Terminal finalization integrity validation failed."
                : failure;
            StageRunAbortOrigin origin = terminalFinalizationAuthority != null
                ? StageRunAbortOrigin.TerminalFinalizationFailure
                : StageRunAbortOrigin.DiagnosticAbort;
            if (!TryAbort(
                    origin,
                    StageRunAbortReason.TerminalFinalizationFailed,
                    StageRunTerminalCoordinatorInvalidationDisposition.TerminalAuthorityInvalidated,
                    resolution.RootAdmissionSequence,
                    resolution.Epoch,
                    out _,
                    out string abortError))
            {
                FaultReason = failureMessage + " Abort closure failed: " + abortError;
                LifecycleState = StageRunLifecycleState.ClosureFaulted;
                error = FaultReason;
                return false;
            }

            error = failureMessage;
            return false;
        }

        internal bool TryRecoverPendingCommit(
            out StageRunResultSummary summary,
            out StageRunResultCommitReceipt receipt,
            out string error)
        {
            summary = null;
            receipt = null;
            error = string.Empty;
            if (LifecycleState != StageRunLifecycleState.CommitRecoveryPending)
            {
                error = $"Result commit recovery is not legal from {LifecycleState}.";
                return false;
            }

            LifecycleState = StageRunLifecycleState.CommitRequested;
            return TryCommitCandidate(out summary, out receipt, out error);
        }

        private bool TryCommitCandidate(
            out StageRunResultSummary summary,
            out StageRunResultCommitReceipt receipt,
            out string error)
        {
            summary = null;
            receipt = null;
            if (commitCandidateSummary == null
                || commitCandidateCoverage == null
                || commitPreparation == null)
            {
                error = "Result commit candidate was not retained for durable comparison.";
                FaultReason = error;
                LifecycleState = StageRunLifecycleState.CommitPersistenceFaulted;
                return false;
            }

            if (!StageRunResultCommitStore.TryCommit(
                commitCandidateSummary,
                commitCandidateCoverage,
                commitPreparation,
                out StageRunResultSummary storedSummary,
                out TerminalFinalizationOwnerCoverageRecord storedCoverage,
                out StageRunResultCommitReceipt storedReceipt,
                out StageRunResultCommitStoreFailureKind failureKind,
                out error))
            {
                FaultReason = error;
                LifecycleState = failureKind == StageRunResultCommitStoreFailureKind.RecoveryPending
                    ? StageRunLifecycleState.CommitRecoveryPending
                    : StageRunLifecycleState.CommitPersistenceFaulted;
                return false;
            }

            terminalRecord = storedSummary.TerminalRecord;
            ownerCoverageRecord = storedCoverage;
            committedSummary = storedSummary;
            commitReceipt = storedReceipt;
            LifecycleState = StageRunLifecycleState.Committed;
            summary = committedSummary;
            receipt = commitReceipt;
            return true;
        }

        internal bool TryPrepareResultPresentation(
            StageRunResultSummary summary,
            StageRunResultProgressionJoinSnapshot joinSnapshot,
            string requestedLocaleId,
            out StageResultPresentationSnapshot presentation,
            out StageResultPresentationAuditEnvelope audit,
            out string error)
        {
            presentation = null;
            audit = null;
            error = string.Empty;
            if (!TryValidateExactCommittedSummary(summary, out _))
            {
                error = "Result presentation does not reference the exact committed summary.";
                return false;
            }

            if (!ReferenceEquals(joinSnapshot, ResultProgressionJoinSnapshot))
            {
                error = "Result presentation does not reference the exact admission join snapshot.";
                return false;
            }

            if (LifecycleState != StageRunLifecycleState.Committed
                && LifecycleState != StageRunLifecycleState.Presented)
            {
                error = $"Result presentation preparation is not legal from {LifecycleState}.";
                return false;
            }

            if (resultPresentationSnapshot != null || resultPresentationAudit != null)
            {
                if (resultPresentationSnapshot == null
                    || resultPresentationAudit == null
                    || !resultPresentationAudit.TryValidate(
                        summary,
                        joinSnapshot,
                        resultPresentationSnapshot,
                        out error))
                {
                    return false;
                }

                presentation = resultPresentationSnapshot;
                audit = resultPresentationAudit;
                return true;
            }

            if (!joinSnapshot.TryValidateIntegrity(out error)
                || joinSnapshot.PresentationSource == null
                || !joinSnapshot.PresentationSource.TryCreatePresentation(
                    summary,
                    requestedLocaleId,
                    out StageResultPresentationSnapshot resolved,
                    out error)
                || !StageResultPresentationAuditEnvelope.TryCreate(
                    summary,
                    joinSnapshot,
                    resolved,
                    out StageResultPresentationAuditEnvelope resolvedAudit,
                    out error))
            {
                return false;
            }

            resultPresentationSnapshot = resolved;
            resultPresentationAudit = resolvedAudit;
            presentation = resolved;
            audit = resolvedAudit;
            return true;
        }

        internal bool TryMarkPresented(
            StageRunResultSummary summary,
            StageResultPresentationSnapshot presentation,
            StageResultPresentationAuditEnvelope audit,
            out string error)
        {
            error = string.Empty;
            if (!TryValidateExactCommittedSummary(summary, out _))
            {
                error = "Result presentation does not reference the exact committed summary.";
                return false;
            }

            if (!ReferenceEquals(presentation, resultPresentationSnapshot)
                || !ReferenceEquals(audit, resultPresentationAudit)
                || presentation == null
                || audit == null
                || !audit.TryValidate(
                    summary,
                    ResultProgressionJoinSnapshot,
                    presentation,
                    out error))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Result presentation was not sealed from the exact admission snapshot.";
                }

                return false;
            }

            if (LifecycleState == StageRunLifecycleState.Presented)
            {
                return true;
            }

            if (LifecycleState != StageRunLifecycleState.Committed)
            {
                error = $"Result presentation is not legal from {LifecycleState}.";
                return false;
            }

            LifecycleState = StageRunLifecycleState.Presented;
            return true;
        }

        internal bool TrySealTerminalAction(
            StageRunResultSummary summary,
            StageRunActionSnapshot action,
            string destinationSceneName,
            string destinationScenePath,
            out StageRunResolvedTerminalAction selection,
            out string error)
        {
            selection = null;
            error = string.Empty;
            if (!TryValidatePresentedSummary(summary, out error))
            {
                return false;
            }

            if (selectedTerminalAction != null)
            {
                error = $"Terminal action {selectedTerminalAction.ActionId} already won selection.";
                return false;
            }

            if (action == null
                || !summary.TryGetOfferedAction(action.ActionId, out StageRunActionSnapshot offered)
                || !ReferenceEquals(offered, action)
                || !action.Allows(summary.Outcome)
                || string.IsNullOrWhiteSpace(destinationSceneName)
                || string.IsNullOrWhiteSpace(destinationScenePath))
            {
                error = "Terminal action is stale, unavailable, or has no resolved destination.";
                return false;
            }

            selectedTerminalAction = new StageRunResolvedTerminalAction(
                summary,
                action,
                destinationSceneName,
                destinationScenePath);
            selection = selectedTerminalAction;
            return true;
        }

        internal bool TryValidatePresentedSummary(
            StageRunResultSummary summary,
            out string error)
        {
            if (LifecycleState != StageRunLifecycleState.Presented)
            {
                error = "Terminal action does not reference the current presented result.";
                return false;
            }

            if (!TryValidateExactCommittedSummary(summary, out string integrityError))
            {
                error = "Terminal action does not reference the current presented result. "
                    + integrityError;
                return false;
            }

            try
            {
                StageRunResultProgressionJoinSnapshot join = ResultProgressionJoinSnapshot;
                StageResultPresentationSnapshot presentation = resultPresentationSnapshot;
                StageResultPresentationAuditEnvelope audit = resultPresentationAudit;
                string authorityError = string.Empty;
                if (join == null
                    || presentation == null
                    || audit == null
                    || !join.TryValidateIntegrity(out authorityError)
                    || !string.Equals(
                        presentation.CanonicalDigest,
                        presentation.RecomputeCanonicalDigest(),
                        StringComparison.Ordinal)
                    || !audit.TryValidate(summary, join, presentation, out authorityError))
                {
                    error = "Terminal action does not reference the current presented result authority. "
                        + authorityError;
                    return false;
                }
            }
            catch (Exception)
            {
                error = "Terminal action does not reference the current presented result authority. "
                    + "The pinned presentation state is damaged.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private bool TryValidateExactCommittedSummary(
            StageRunResultSummary summary,
            out string error)
        {
            error = string.Empty;
            try
            {
                bool valid = summary != null
                    && committedSummary != null
                    && ReferenceEquals(summary, committedSummary)
                    && summary.Identity != null
                    && summary.RouteSnapshot != null
                    && Identity.SchemaVersion == StageRunIdentity.CurrentSchemaVersion
                    && RouteSnapshot.SchemaVersion == StageRunIdentity.CurrentSchemaVersion
                    && summary.Identity.SchemaVersion == StageRunIdentity.CurrentSchemaVersion
                    && summary.RouteSnapshot.SchemaVersion == StageRunIdentity.CurrentSchemaVersion
                    && summary.HasValidCanonicalDigest()
                    && commitReceipt != null
                    && commitReceipt.HasValidIntegrity()
                    && ownerCoverageRecord != null
                    && string.Equals(summary.Identity.RunId, Identity.RunId, StringComparison.Ordinal)
                    && string.Equals(
                        summary.Identity.PlayableStageId,
                        Identity.PlayableStageId,
                        StringComparison.Ordinal)
                    && summary.Identity.RouteRevision == Identity.RouteRevision
                    && string.Equals(
                        summary.Identity.RouteSnapshotDigest,
                        Identity.RouteSnapshotDigest,
                        StringComparison.Ordinal)
                    && string.Equals(commitReceipt.RunId, Identity.RunId, StringComparison.Ordinal)
                    && string.Equals(
                        commitReceipt.PlayableStageId,
                        Identity.PlayableStageId,
                        StringComparison.Ordinal)
                    && commitReceipt.RouteRevision == Identity.RouteRevision
                    && string.Equals(
                        commitReceipt.RouteDigest,
                        Identity.RouteSnapshotDigest,
                        StringComparison.Ordinal)
                    && string.Equals(
                        summary.RouteSnapshot.ComputeCanonicalDigest(),
                        commitReceipt.RouteDigest,
                        StringComparison.Ordinal)
                    && string.Equals(
                        summary.ResultSummaryDigest,
                        commitReceipt.ResultSummaryDigest,
                        StringComparison.Ordinal)
                    && string.Equals(
                        commitReceipt.TerminalFinalizationOwnerCoverageRecordId,
                        ownerCoverageRecord.TerminalFinalizationOwnerCoverageRecordId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        commitReceipt.TerminalFinalizationOwnerCoverageDigest,
                        ownerCoverageRecord.CanonicalDigest,
                        StringComparison.Ordinal);
                if (valid)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // Treat a damaged immutable graph as an integrity rejection.
            }

            error = "Committed result summary integrity validation failed.";
            return false;
        }

        internal bool TryBeginTerminalActionDispatch(
            StageRunResolvedTerminalAction selection,
            out string error)
        {
            error = string.Empty;
            if (!ReferenceEquals(selection, selectedTerminalAction)
                || terminalActionDispatchStarted
                || LifecycleState != StageRunLifecycleState.Presented)
            {
                error = "Terminal action dispatch is stale or already started.";
                return false;
            }

            bool closureIntegrityValid = Identity.SchemaVersion
                    == StageRunIdentity.CurrentSchemaVersion
                && RouteSnapshot.SchemaVersion == StageRunIdentity.CurrentSchemaVersion
                && selection.HasValidCanonicalDigest();
#if UNITY_INCLUDE_TESTS
            if (StageRunRuntime.ConsumeTerminalActionClosureIntegrityFailureForTests())
            {
                closureIntegrityValid = false;
            }
#endif
            if (!closureIntegrityValid)
            {
                error = "Terminal action closure integrity validation failed.";
                FailTerminalActionDispatch(
                    selection,
                    StageDispatchClosureFailedBoundary.ClosureIntegrity,
                    error);
                return false;
            }

            terminalActionDispatchStarted = true;
            return true;
        }

        internal bool IsTerminalActionDispatchInProgress(
            StageRunResolvedTerminalAction selection)
        {
            return ReferenceEquals(selection, selectedTerminalAction)
                && terminalActionDispatchStarted
                && LifecycleState == StageRunLifecycleState.Presented;
        }

        internal void CompleteTerminalActionDispatch(StageRunResolvedTerminalAction selection)
        {
            if (ReferenceEquals(selection, selectedTerminalAction) && terminalActionDispatchStarted)
            {
                LifecycleState = StageRunLifecycleState.Disposed;
            }
        }

        internal void FailTerminalActionDispatch(
            StageRunResolvedTerminalAction selection,
            StageDispatchClosureFailedBoundary failedBoundary,
            string reason)
        {
            if (!ReferenceEquals(selection, selectedTerminalAction))
            {
                return;
            }

            terminalActionDispatchStarted = false;
            FaultReason = reason ?? string.Empty;
            dispatchClosureFaultRecord ??= new StageDispatchClosureFaultRecord(
                committedSummary,
                selection,
                failedBoundary,
                FaultReason,
                ++dispatchFaultSequence);
            LifecycleState = StageRunLifecycleState.Presented;
        }

        internal bool CanAbortBeforeCommit()
        {
            return LifecycleState == StageRunLifecycleState.Created
                || LifecycleState == StageRunLifecycleState.CorridorActive
                || LifecycleState == StageRunLifecycleState.HandoffPending
                || LifecycleState == StageRunLifecycleState.StationActive
                || LifecycleState == StageRunLifecycleState.TerminalFinalizing
                || LifecycleState == StageRunLifecycleState.TerminalFinalizationOwnersSealed
                || LifecycleState == StageRunLifecycleState.OutcomeFactsSealed;
        }

        internal bool IsExpectedHandoffDestination(Scene scene)
        {
            return LifecycleState == StageRunLifecycleState.HandoffPending
                && pendingDispatch != null
                && scene.IsValid()
                && string.Equals(
                    NormalizePath(scene.path),
                    pendingDispatch.DestinationScenePath,
                    StringComparison.Ordinal);
        }

        internal bool IsExpectedTerminalActionDestination(Scene scene)
        {
            return LifecycleState == StageRunLifecycleState.Presented
                && terminalActionDispatchStarted
                && selectedTerminalAction != null
                && scene.IsValid()
                && string.Equals(
                    NormalizePath(scene.path),
                    NormalizePath(selectedTerminalAction.DestinationScenePath),
                    StringComparison.Ordinal);
        }

        internal string DescribeTerminalActionDestination(Scene scene)
        {
            return $"dispatchStarted={terminalActionDispatchStarted}, "
                + $"actualName={scene.name}, actualPath={NormalizePath(scene.path)}, "
                + $"expectedName={selectedTerminalAction?.DestinationSceneName ?? string.Empty}, "
                + $"expectedPath={NormalizePath(selectedTerminalAction?.DestinationScenePath)}";
        }

        internal bool TryAbort(
            StageRunAbortOrigin origin,
            StageRunAbortReason reason,
            StageRunTerminalCoordinatorInvalidationDisposition coordinatorDisposition,
            long coordinatorRootAdmissionSequence,
            long coordinatorEpoch,
            out StageRunAbortRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            if (Identity.SchemaVersion != StageRunIdentity.CurrentSchemaVersion
                || RouteSnapshot.SchemaVersion != StageRunIdentity.CurrentSchemaVersion)
            {
                FaultReason = "Current-schema NotAdmitted abort closure cannot represent a future route schema.";
                LifecycleState = StageRunLifecycleState.ClosureFaulted;
                error = FaultReason;
                return false;
            }

            if (abortRecord != null)
            {
                if (abortCloseAuthority != null
                    && abortCloseAuthority.Origin == origin
                    && abortCloseAuthority.AbortReason == reason
                    && abortCloseAuthority.CoordinatorInvalidationDisposition
                        == coordinatorDisposition
                    && abortCloseAuthority.CoordinatorRootAdmissionSequence
                        == coordinatorRootAdmissionSequence
                    && abortCloseAuthority.CoordinatorEpoch == coordinatorEpoch)
                {
                    record = abortRecord;
                    return true;
                }

                error = "A different diagnostic abort authority tuple already owns this run.";
                return false;
            }

            if (!CanAbortBeforeCommit())
            {
                error = $"Diagnostic abort is not legal from {LifecycleState}.";
                return false;
            }

            if ((origin == StageRunAbortOrigin.TerminalFinalizationFailure)
                != (terminalFinalizationAuthority != null))
            {
                error = "Terminal-finalization abort authority presence does not match its origin.";
                return false;
            }

            StageRunLifecycleState lastLifecycleState = LifecycleState;
            LifecycleState = StageRunLifecycleState.AbortClosing;
            long coordinatorInvalidationSequence = ++abortSequence;
            abortCloseAuthority = new StageRunAbortCloseAuthority(
                Identity,
                origin,
                reason,
                lastLifecycleState,
                origin == StageRunAbortOrigin.TerminalFinalizationFailure
                    ? terminalFinalizationAuthority
                    : null,
                coordinatorDisposition,
                coordinatorRootAdmissionSequence,
                coordinatorEpoch,
                coordinatorInvalidationSequence,
                ++abortSequence);

            StageRunRouteHandoffCoverage handoffCoverage;
            if (handoffTerminalReceipt != null)
            {
                handoffCoverage = StageRunRouteHandoffCoverage.Succeeded(handoffTerminalReceipt);
            }
            else if (pendingDispatch != null)
            {
                handoffTerminalReceipt = new StageSegmentHandoffTerminalReceipt(
                    Identity,
                    pendingDispatch.Token,
                    abortCloseAuthority,
                    reason.ToString(),
                    ++handoffEvidenceSequence);
                pendingDispatch = null;
                handoffCoverage = StageRunRouteHandoffCoverage.Succeeded(handoffTerminalReceipt);
            }
            else if (handoffSequence == 0)
            {
                handoffCoverage = StageRunRouteHandoffCoverage.NotIssued();
            }
            else
            {
                FaultReason = "An issued route handoff has no terminal closure evidence.";
                LifecycleState = StageRunLifecycleState.ClosureFaulted;
                error = FaultReason;
                return false;
            }

            StageRunOutcomeFactCoverage outcomeCoverage =
                lastLifecycleState == StageRunLifecycleState.OutcomeFactsSealed
                    && diagnosticOutcomeFactCandidate != null
                    ? StageRunOutcomeFactCoverage.SealedDiagnosticOnly(
                        diagnosticOutcomeFactCandidate)
                    : StageRunOutcomeFactCoverage.NotSealedBeforeAbort();
            abortRecord = new StageRunAbortRecord(
                Identity,
                lastLifecycleState,
                abortCloseAuthority,
                handoffCoverage,
                outcomeCoverage,
                ++abortSequence);
            FaultReason = reason.ToString();
            LifecycleState = StageRunLifecycleState.Aborted;
            LifecycleState = StageRunLifecycleState.Disposed;
            record = abortRecord;
            return true;
        }

        internal void DisposeForReplacement()
        {
            pendingDispatch = null;
            LifecycleState = StageRunLifecycleState.Disposed;
        }

        internal void FailAbortClosure(string reason)
        {
            pendingDispatch = null;
            FaultReason = string.IsNullOrWhiteSpace(reason)
                ? "Abort closure failed before coordinator cancellation was verified."
                : reason;
            LifecycleState = StageRunLifecycleState.ClosureFaulted;
        }

        private bool Fault(string reason, out string error)
        {
            pendingDispatch = null;
            FaultReason = reason;
            LifecycleState = StageRunLifecycleState.Faulted;
            error = reason;
            return false;
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }
    }

    public static class StageRunRuntime
    {
        private sealed class ExplicitAbortRequestReceipt
        {
            public ExplicitAbortRequestReceipt(
                string runId,
                CombatEncounterController encounter,
                StageRunAbortReason reason,
                StageRunAbortRecord record)
            {
                RunId = runId ?? string.Empty;
                Encounter = encounter;
                Reason = reason;
                Record = record;
            }

            public string RunId { get; }
            public CombatEncounterController Encounter { get; }
            public StageRunAbortReason Reason { get; }
            public StageRunAbortRecord Record { get; }

            public bool Matches(
                StageRunContext context,
                CombatEncounterController encounter,
                StageRunAbortReason reason)
            {
                return context != null
                    && string.Equals(RunId, context.Identity.RunId, StringComparison.Ordinal)
                    && ReferenceEquals(Encounter, encounter)
                    && Reason == reason
                    && ReferenceEquals(Record, context.AbortRecord);
            }
        }

        private sealed class UnityStageRunSceneLoader : IStageRunSceneLoader
        {
            public StageRunSceneLoadCompletionMode CompletionMode =>
                StageRunSceneLoadCompletionMode.RequestAccepted;

            public bool TryLoadSingle(string sceneName, string scenePath, out string error)
            {
                error = string.Empty;
                try
                {
                    Time.timeScale = 1f;
#if UNITY_EDITOR
                    EditorSceneManager.LoadSceneInPlayMode(
                        scenePath,
                        new LoadSceneParameters(LoadSceneMode.Single));
#else
                    SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
#endif
                    return true;
                }
                catch (Exception exception)
                {
                    error = exception.Message;
                    return false;
                }
            }
        }

        private static StageRunContext activeContext;
        private static StageRunAbortRecord lastAbortRecord;
        private static IStageRunSceneLoader sceneLoader = new UnityStageRunSceneLoader();
        private static EncounterTerminalResolutionCoordinator registeredStationCoordinator;
        private static int registeredStationCoordinatorSceneHandle;
        private static string registeredStationCoordinatorRunId = string.Empty;
        private static ExplicitAbortRequestReceipt explicitAbortRequestReceipt;
#if UNITY_INCLUDE_TESTS
        private static bool injectTerminalActionClosureIntegrityFailure;
#endif

        public static bool HasActiveContext => activeContext != null
            && activeContext.LifecycleState != StageRunLifecycleState.Disposed;
        public static StageRunContext ActiveContext => activeContext;
        public static StageRunAbortRecord LastAbortRecord => lastAbortRecord;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntime()
        {
            InstallSceneObservers();
            activeContext = null;
            lastAbortRecord = null;
            sceneLoader = new UnityStageRunSceneLoader();
            ClearRegisteredStationCoordinator();
            explicitAbortRequestReceipt = null;
#if UNITY_INCLUDE_TESTS
            injectTerminalActionClosureIntegrityFailure = false;
#endif
#if UNITY_INCLUDE_TESTS
            StageRunResultCommitStore.ConfigureIsolatedTestStorage();
#else
            StageRunResultCommitStore.ConfigureProductionStorage();
#endif
        }

        public static bool TryAdmitFirstSegment(
            PlayableStageDefinition definition,
            Scene scene,
            out StageRunContext context,
            out string error)
        {
            InstallSceneObservers();
            context = null;
            error = string.Empty;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                error = "First-segment scene is not loaded.";
                return false;
            }

            if (!StageRunRouteSnapshot.TryCreate(definition, out StageRunRouteSnapshot snapshot, out error))
            {
                return false;
            }

            StageRunSegmentSnapshot entry = snapshot.GetSegment(0);
            string loadedPath = NormalizePath(scene.path);
            if (!string.Equals(loadedPath, entry.ScenePath, StringComparison.Ordinal)
                || entry.EntryConditionKind
                    != StageSegmentConditionKind.RunEntrySnapshotValidatedAndFirstSegmentActivated)
            {
                error = "First-segment scene or admission condition does not match the route snapshot.";
                return false;
            }

            if (activeContext != null
                && activeContext.CurrentSceneHandle == scene.handle
                && activeContext.LifecycleState == StageRunLifecycleState.CorridorActive
                && string.Equals(
                    activeContext.Identity.RouteSnapshotDigest,
                    snapshot.CanonicalDigest,
                    StringComparison.Ordinal))
            {
                context = activeContext;
                return true;
            }

            if (!StageRunResultProgressionJoinSnapshot.TryCreate(
                    definition,
                    out StageRunResultProgressionJoinSnapshot resultProgressionJoinSnapshot,
                    out error))
            {
                return false;
            }

            if (activeContext != null
                && activeContext.IsExpectedTerminalActionDestination(scene))
            {
                activeContext.CompleteTerminalActionDispatch(
                    activeContext.SelectedTerminalAction);
            }
            else if (activeContext != null && activeContext.CanAbortBeforeCommit())
            {
                if (!TryAbortForDirectFirstSegmentReplacement(activeContext, out error))
                {
                    return false;
                }
            }
            else if (activeContext != null
                && activeContext.LifecycleState != StageRunLifecycleState.Disposed)
            {
                error = $"A {activeContext.LifecycleState} run cannot be replaced by direct entry. "
                    + activeContext.DescribeTerminalActionDestination(scene);
                return false;
            }

            ClearRegisteredStationCoordinator();
            explicitAbortRequestReceipt = null;
            activeContext = new StageRunContext(
                Guid.NewGuid().ToString("N"),
                snapshot,
                resultProgressionJoinSnapshot,
                scene.handle);
            activeContext.ActivateFirstSegment();
            context = activeContext;
            return true;
        }

        public static bool TryAbortActiveRun(
            CombatEncounterController encounter,
            StageRunAbortReason reason,
            out StageRunAbortRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            StageRunContext context = activeContext;
            if (context == null)
            {
                error = "No active canonical run can own this explicit abort.";
                return false;
            }

            if (context.AbortRecord != null && context.AbortCloseAuthority != null)
            {
                if (explicitAbortRequestReceipt == null
                    || !explicitAbortRequestReceipt.Matches(context, encounter, reason))
                {
                    error = "The explicit abort replay does not match the original run, encounter, reason, and record.";
                    return false;
                }

                StageRunAbortCloseAuthority authority = context.AbortCloseAuthority;
                return TryAbortActiveContext(
                    reason,
                    authority.CoordinatorInvalidationDisposition,
                    authority.CoordinatorRootAdmissionSequence,
                    authority.CoordinatorEpoch,
                    out record,
                    out error);
            }

            if (context.LifecycleState == StageRunLifecycleState.Created
                || context.LifecycleState == StageRunLifecycleState.CorridorActive
                || context.LifecycleState == StageRunLifecycleState.HandoffPending)
            {
                if (encounter != null)
                {
                    error = "A pre-Station explicit abort cannot claim a Station coordinator.";
                    return false;
                }

                return TrySealExplicitAbortRequest(
                    context,
                    encounter,
                    reason,
                    StageRunTerminalCoordinatorInvalidationDisposition.NotBoundBeforeStation,
                    0,
                    0,
                    out record,
                    out error);
            }

            if (!TryCaptureAndCancelStationCoordinator(
                    context,
                    encounter,
                    out long rootAdmissionSequence,
                    out long epoch,
                    out error))
            {
                return false;
            }

            return TrySealExplicitAbortRequest(
                context,
                encounter,
                reason,
                StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested,
                rootAdmissionSequence,
                epoch,
                out record,
                out error);
        }

        internal static bool TryRegisterStationCoordinator(
            CombatEncounterController encounter,
            out string error)
        {
            error = string.Empty;
            StageRunContext context = activeContext;
            EncounterTerminalResolutionCoordinator coordinator = encounter?.TerminalCoordinator;
            if (context == null
                || encounter == null
                || coordinator == null
                || !encounter.UsesCoordinatedTerminalResolution
                || encounter.gameObject.scene.handle != context.CurrentSceneHandle
                || context.LifecycleState != StageRunLifecycleState.StationActive)
            {
                error = "No exact live Station coordinator can be registered for the active run.";
                return false;
            }

            if (registeredStationCoordinator != null)
            {
                if (ReferenceEquals(registeredStationCoordinator, coordinator)
                    && registeredStationCoordinatorSceneHandle == context.CurrentSceneHandle
                    && string.Equals(
                        registeredStationCoordinatorRunId,
                        context.Identity.RunId,
                        StringComparison.Ordinal))
                {
                    return true;
                }

                error = "A different Station coordinator is already registered for the active run.";
                return false;
            }

            registeredStationCoordinator = coordinator;
            registeredStationCoordinatorSceneHandle = context.CurrentSceneHandle;
            registeredStationCoordinatorRunId = context.Identity.RunId;
            return true;
        }

        public static bool TryAbortFromCoordinatorDiagnostic(
            CombatEncounterController encounter,
            EncounterTerminalDiagnostic diagnostic,
            out StageRunAbortRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            StageRunContext context = activeContext;
            EncounterTerminalResolutionCoordinator coordinator = encounter?.TerminalCoordinator;
            if (context == null
                || encounter == null
                || coordinator == null
                || encounter.gameObject.scene.handle != context.CurrentSceneHandle
                || !IsRegisteredStationCoordinatorFor(context, coordinator)
                || coordinator.State != EncounterTerminalCoordinatorState.Faulted
                || !encounter.HasDiagnostic
                || !coordinator.HasDiagnostic
                || diagnostic.RunGeneration <= 0
                || diagnostic.RunGeneration != encounter.RunGeneration
                || diagnostic.RunGeneration != coordinator.RunGeneration
                || !DiagnosticsEqual(diagnostic, encounter.Diagnostic)
                || !DiagnosticsEqual(diagnostic, coordinator.Diagnostic))
            {
                error = "Coordinator diagnostic is not the exact fault published by the registered active-run coordinator.";
                return false;
            }

            return TryAbortActiveContext(
                StageRunAbortReason.CoordinatorDiagnostic,
                StageRunTerminalCoordinatorInvalidationDisposition.Faulted,
                diagnostic.RootAdmissionSequence,
                diagnostic.Epoch,
                out record,
                out error);
        }

        public static bool TryAbortFromStationAdapterLoss(
            Component adapter,
            CombatEncounterController encounter,
            StageRunAbortReason reason,
            out StageRunAbortRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            bool validTypedAdapter =
                (reason == StageRunAbortReason.StationFactCollectorLost
                    && adapter is OlympusStationRunFactCollector)
                || (reason == StageRunAbortReason.StationResultPresenterLost
                    && adapter is OlympusStationCombatResultPresenter);
            StageRunContext context = activeContext;
            if (!validTypedAdapter
                || context == null
                || adapter == null
                || encounter == null
                || adapter.gameObject.scene.handle != context.CurrentSceneHandle
                || encounter.gameObject.scene.handle != context.CurrentSceneHandle
                || !encounter.UsesCoordinatedTerminalResolution
                || !context.CanAbortBeforeCommit())
            {
                error = "Station adapter loss does not belong to an abortable active run.";
                return false;
            }

            if (!TryCaptureAndCancelStationCoordinator(
                    context,
                    encounter,
                    out long rootAdmissionSequence,
                    out long epoch,
                    out error))
            {
                return false;
            }

            return TryAbortActiveContext(
                reason,
                StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested,
                rootAdmissionSequence,
                epoch,
                out record,
                out error);
        }

        public static bool TryEnterPendingSegment(Scene scene, out StageRunContext context, out string error)
        {
            context = activeContext;
            if (context == null)
            {
                error = "No active canonical stage run exists for this scene.";
                return false;
            }

            return context.TryEnterPendingSegment(scene, out error);
        }

        public static bool TryCommitTerminalResolution(
            CombatEncounterController encounter,
            EncounterTerminalResolution resolution,
            out StageRunResultSummary summary,
            out StageRunResultCommitReceipt receipt,
            out string error)
        {
            summary = null;
            receipt = null;
            error = string.Empty;
            StageRunContext context = activeContext;
            if (context == null)
            {
                error = "No active canonical stage run owns this terminal resolution.";
                return false;
            }

            if (encounter == null
                || encounter.gameObject.scene.handle != context.CurrentSceneHandle
                || !encounter.UsesCoordinatedTerminalResolution
                || !encounter.HasTerminalResolution)
            {
                error = "Terminal resolution does not belong to the active Station encounter.";
                return false;
            }

            if (context.CommittedSummary != null)
            {
                EncounterTerminalResolutionCoordinator committedCoordinator =
                    encounter.TerminalCoordinator;
                if (context.TerminalRecord == null
                    || !context.TerminalRecord.Matches(resolution)
                    || (committedCoordinator != null
                        && committedCoordinator.HasTerminalEpochEvidence
                        && (context.TerminalEpochClosureRecord == null
                            || !context.TerminalEpochClosureRecord.Matches(
                                committedCoordinator.TerminalEpochEvidence))))
                {
                    error = "A different terminal record attempted to replace the committed run result.";
                    return false;
                }

                return context.TryCommitTerminalResolution(
                    resolution,
                    committedCoordinator != null && committedCoordinator.HasTerminalEpochEvidence
                        ? committedCoordinator.TerminalEpochEvidence
                        : null,
                    EncounterTerminalCoordinatorState.TerminalClosed,
                    out summary,
                    out receipt,
                    out error);
            }

            EncounterTerminalResolutionCoordinator coordinator = encounter.TerminalCoordinator;
            if (coordinator == null
                || coordinator.State != EncounterTerminalCoordinatorState.TerminalClosed
                || coordinator.RunGeneration != resolution.RunGeneration
                || !coordinator.HasTerminalEpochEvidence)
            {
                error = "Terminal resolution or its epoch-closure evidence does not belong to the active Station coordinator.";
                return false;
            }

            return context.TryCommitTerminalResolution(
                resolution,
                coordinator.TerminalEpochEvidence,
                coordinator.State,
                out summary,
                out receipt,
                out error);
        }

        public static bool TryPrepareResultPresentation(
            StageRunResultSummary summary,
            StageRunResultProgressionJoinSnapshot joinSnapshot,
            string requestedLocaleId,
            out StageResultPresentationSnapshot presentation,
            out StageResultPresentationAuditEnvelope audit,
            out string error)
        {
            presentation = null;
            audit = null;
            if (activeContext == null)
            {
                error = "No active canonical stage run owns this result presentation.";
                return false;
            }

            return activeContext.TryPrepareResultPresentation(
                summary,
                joinSnapshot,
                requestedLocaleId,
                out presentation,
                out audit,
                out error);
        }

        public static bool TryMarkResultPresented(
            StageRunResultSummary summary,
            StageResultPresentationSnapshot presentation,
            StageResultPresentationAuditEnvelope audit,
            out string error)
        {
            if (activeContext == null)
            {
                error = "No active canonical stage run owns this result presentation.";
                return false;
            }

            return activeContext.TryMarkPresented(summary, presentation, audit, out error);
        }

        public static bool TryRecoverPendingResultCommit(
            out StageRunResultSummary summary,
            out StageRunResultCommitReceipt receipt,
            out string error)
        {
            summary = null;
            receipt = null;
            if (activeContext == null)
            {
                error = "No active canonical stage run owns a pending result commit.";
                return false;
            }

            return activeContext.TryRecoverPendingCommit(out summary, out receipt, out error);
        }

        public static bool TryReadCommittedResultDecision(
            string runId,
            out StageRunResultCommitReceipt receipt,
            out string error)
        {
            return StageRunResultCommitStore.TryReadReceipt(
                runId,
                out receipt,
                out _,
                out error);
        }

        public static bool TryDispatchTerminalAction(
            StageRunResultSummary summary,
            string actionId,
            IStageRunUiRouteResolver uiRouteResolver,
            out StageRunResolvedTerminalAction selection,
            out string error)
        {
            selection = null;
            error = string.Empty;
            StageRunContext context = activeContext;
            if (context == null)
            {
                error = "No active presented result offers this terminal action.";
                return false;
            }

            if (!context.TryValidatePresentedSummary(summary, out error))
            {
                return false;
            }

            if (!summary.TryGetOfferedAction(actionId, out StageRunActionSnapshot action))
            {
                error = "No active presented result offers this terminal action.";
                return false;
            }

            string destinationSceneName;
            string destinationScenePath;
            if (action.ActionKind == StageRouteActionKind.Replay
                || action.ActionKind == StageRouteActionKind.Retry)
            {
                StageRunSegmentSnapshot entry = context.RouteSnapshot.GetSegment(0);
                if (!string.Equals(
                    action.TargetPlayableStageId,
                    context.Identity.PlayableStageId,
                    StringComparison.Ordinal))
                {
                    error = "Replay/Retry target does not match the immutable active route snapshot.";
                    return false;
                }

                destinationSceneName = entry.SceneName;
                destinationScenePath = entry.ScenePath;
            }
            else if (action.ActionKind == StageRouteActionKind.UIRoute
                && uiRouteResolver != null
                && uiRouteResolver.TryResolve(
                    action.TargetUiRouteId,
                    out StageRunUiRouteTarget routeTarget,
                    out error)
                && routeTarget != null
                && routeTarget.RouteId == action.TargetUiRouteId)
            {
                destinationSceneName = routeTarget.SceneName;
                destinationScenePath = routeTarget.ScenePath;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "UI route resolver rejected the terminal action target.";
                }

                return false;
            }

            if (!context.TrySealTerminalAction(
                summary,
                action,
                destinationSceneName,
                destinationScenePath,
                out selection,
                out error)
                || !context.TryBeginTerminalActionDispatch(selection, out error))
            {
                return false;
            }

            if (!sceneLoader.TryLoadSingle(
                    selection.DestinationSceneName,
                    selection.DestinationScenePath,
                    out error))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Stage scene loader rejected the selected terminal action.";
                }

                context.FailTerminalActionDispatch(
                    selection,
                    StageDispatchClosureFailedBoundary.SceneLoad,
                    error);
                return false;
            }

            if (sceneLoader.CompletionMode
                    == StageRunSceneLoadCompletionMode.DestinationActivatedSynchronously
                && context.IsTerminalActionDispatchInProgress(selection))
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (!context.IsExpectedTerminalActionDestination(activeScene))
                {
                    error = "Terminal action loader completed outside the sealed destination scene.";
                    context.FailTerminalActionDispatch(
                        selection,
                        StageDispatchClosureFailedBoundary.UnexpectedSceneExit,
                        error);
                    return false;
                }

                context.CompleteTerminalActionDispatch(selection);
            }
            else if (context.IsTerminalActionDispatchInProgress(selection))
            {
                context.CompleteTerminalActionDispatch(selection);
            }
            else if (context.LifecycleState != StageRunLifecycleState.Disposed)
            {
                error = string.IsNullOrWhiteSpace(context.FaultReason)
                    ? "Terminal action dispatch lost its sealed lifecycle authority."
                    : context.FaultReason;
                return false;
            }

            if (ReferenceEquals(activeContext, context))
            {
                activeContext = null;
            }

            return true;
        }

#if UNITY_INCLUDE_TESTS
        public static void ResetForTests()
        {
            InstallSceneObservers();
            activeContext?.DisposeForReplacement();
            activeContext = null;
            lastAbortRecord = null;
            sceneLoader = new UnityStageRunSceneLoader();
            ClearRegisteredStationCoordinator();
            StageRunResultCommitStore.ConfigureIsolatedTestStorage();
        }

        public static void SetSceneLoaderForTests(IStageRunSceneLoader testSceneLoader)
        {
            sceneLoader = testSceneLoader ?? new UnityStageRunSceneLoader();
        }

        public static void ClearResultCommitMemoryCacheForTests()
        {
            StageRunResultCommitStore.ClearMemoryCache();
        }

        public static void SimulateProcessLossForTests()
        {
            activeContext = null;
            lastAbortRecord = null;
            sceneLoader = new UnityStageRunSceneLoader();
            ClearRegisteredStationCoordinator();
            explicitAbortRequestReceipt = null;
            StageRunResultCommitStore.ClearMemoryCache();
        }

        public static string GetResultCommitDecisionPathForTests(string runId)
        {
            return StageRunResultCommitStore.GetDecisionPathForTests(runId);
        }

        public static bool SeedConflictingResultDecisionForTests(
            StageRunIdentity identity,
            out string error)
        {
            return StageRunResultCommitStore.SeedConflictingDecisionForTests(identity, out error);
        }

        public static bool SeedCorruptResultDecisionForTests(string runId, out string error)
        {
            return StageRunResultCommitStore.SeedCorruptDecisionForTests(runId, out error);
        }

        public static void InjectTransientResultDecisionIoFailuresForTests(
            int writeFailureCount,
            int readFailureCount)
        {
            StageRunResultCommitStore.InjectTransientIoFailuresForTests(
                writeFailureCount,
                readFailureCount);
        }

        public static void InjectTerminalActionClosureIntegrityFailureForTests()
        {
            injectTerminalActionClosureIntegrityFailure = true;
        }

        public static bool TryReplayAbortTupleForTests(
            StageRunAbortReason reason,
            StageRunTerminalCoordinatorInvalidationDisposition coordinatorDisposition,
            long coordinatorRootAdmissionSequence,
            long coordinatorEpoch,
            out StageRunAbortRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            StageRunContext context = activeContext;
            if (context == null)
            {
                error = "No active canonical run can replay an abort tuple.";
                return false;
            }

            StageRunAbortOrigin origin = context.TerminalFinalizationAuthority != null
                ? StageRunAbortOrigin.TerminalFinalizationFailure
                : StageRunAbortOrigin.DiagnosticAbort;
            return context.TryAbort(
                origin,
                reason,
                coordinatorDisposition,
                coordinatorRootAdmissionSequence,
                coordinatorEpoch,
                out record,
                out error);
        }

        public static bool TryCommitCandidateFinalMismatchForTests(
            CombatEncounterController encounter,
            out string error)
        {
            error = string.Empty;
            StageRunContext context = activeContext;
            if (context == null
                || encounter == null
                || !encounter.HasTerminalResolution
                || encounter.TerminalCoordinator == null
                || !encounter.TerminalCoordinator.HasTerminalEpochEvidence)
            {
                error = "No terminal evidence is available for candidate/final mismatch injection.";
                return false;
            }

            EncounterTerminalResolution valid = encounter.TerminalResolution;
            var malformed = new EncounterTerminalResolution(
                valid.RunGeneration,
                valid.RootAdmissionSequence,
                valid.Epoch,
                valid.Outcome,
                valid.Reason,
                valid.PlayerDown,
                false,
                valid.PlayerHealth,
                valid.BossHealth);
            return context.TryCommitTerminalResolution(
                malformed,
                encounter.TerminalCoordinator.TerminalEpochEvidence,
                encounter.TerminalCoordinator.State,
                out _,
                out _,
                out error);
        }

        internal static bool ConsumeTerminalActionClosureIntegrityFailureForTests()
        {
            bool injected = injectTerminalActionClosureIntegrityFailure;
            injectTerminalActionClosureIntegrityFailure = false;
            return injected;
        }
#endif

        private static bool TryAbortForDirectFirstSegmentReplacement(
            StageRunContext context,
            out string error)
        {
            error = string.Empty;
            if (context == null || !ReferenceEquals(context, activeContext))
            {
                error = "Only the exact active run can be replaced by direct first-segment entry.";
                return false;
            }

            if (context.LifecycleState == StageRunLifecycleState.Created
                || context.LifecycleState == StageRunLifecycleState.CorridorActive
                || context.LifecycleState == StageRunLifecycleState.HandoffPending)
            {
                return TryAbortActiveContext(
                    StageRunAbortReason.RunReplacedBeforeCommit,
                    StageRunTerminalCoordinatorInvalidationDisposition.NotBoundBeforeStation,
                    0,
                    0,
                    out _,
                    out error);
            }

            TerminalFinalizationAuthority terminalAuthority =
                context.TerminalFinalizationAuthority;
            if (terminalAuthority != null)
            {
                return TryAbortActiveContext(
                    StageRunAbortReason.RunReplacedBeforeCommit,
                    StageRunTerminalCoordinatorInvalidationDisposition.TerminalAuthorityInvalidated,
                    terminalAuthority.RootAdmissionSequence,
                    terminalAuthority.TerminalEpoch,
                    out _,
                    out error);
            }

            if (!TryCaptureAndCancelStationCoordinator(
                    context,
                    null,
                    out long rootAdmissionSequence,
                    out long epoch,
                    out string cancellationError))
            {
                context.FailAbortClosure(
                    "Direct first-segment replacement could not verify Station coordinator cancellation: "
                    + cancellationError);
                error = context.FaultReason;
                return false;
            }

            return TryAbortActiveContext(
                StageRunAbortReason.RunReplacedBeforeCommit,
                StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested,
                rootAdmissionSequence,
                epoch,
                out _,
                out error);
        }

        private static bool TrySealExplicitAbortRequest(
            StageRunContext context,
            CombatEncounterController encounter,
            StageRunAbortReason reason,
            StageRunTerminalCoordinatorInvalidationDisposition coordinatorDisposition,
            long coordinatorRootAdmissionSequence,
            long coordinatorEpoch,
            out StageRunAbortRecord record,
            out string error)
        {
            if (!TryAbortActiveContext(
                    reason,
                    coordinatorDisposition,
                    coordinatorRootAdmissionSequence,
                    coordinatorEpoch,
                    out record,
                    out error))
            {
                return false;
            }

            explicitAbortRequestReceipt = new ExplicitAbortRequestReceipt(
                context.Identity.RunId,
                encounter,
                reason,
                record);
            return true;
        }

        private static bool DiagnosticsEqual(
            EncounterTerminalDiagnostic left,
            EncounterTerminalDiagnostic right)
        {
            return left.Reason == right.Reason
                && left.RunGeneration == right.RunGeneration
                && left.RootAdmissionSequence == right.RootAdmissionSequence
                && left.Epoch == right.Epoch
                && string.Equals(left.Message, right.Message, StringComparison.Ordinal);
        }

        private static bool TryAbortActiveContext(
            StageRunAbortReason reason,
            StageRunTerminalCoordinatorInvalidationDisposition coordinatorDisposition,
            long coordinatorRootAdmissionSequence,
            long coordinatorEpoch,
            out StageRunAbortRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            StageRunContext context = activeContext;
            if (context == null)
            {
                error = "No active canonical run can own this diagnostic abort.";
                return false;
            }

            StageRunAbortOrigin origin = context.TerminalFinalizationAuthority != null
                ? StageRunAbortOrigin.TerminalFinalizationFailure
                : StageRunAbortOrigin.DiagnosticAbort;
            if (!context.TryAbort(
                    origin,
                    reason,
                    coordinatorDisposition,
                    coordinatorRootAdmissionSequence,
                    coordinatorEpoch,
                    out record,
                    out error))
            {
                return false;
            }

            lastAbortRecord = record;
            return true;
        }

        private static void InstallSceneObservers()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            CombatEncounterController.CoordinatedRunStarted -= HandleCoordinatedRunStarted;
            CombatEncounterController.CoordinatedRunStarted += HandleCoordinatedRunStarted;
            CombatEncounterController.CoordinatedRunStopping -= HandleCoordinatedRunStopping;
            CombatEncounterController.CoordinatedRunStopping += HandleCoordinatedRunStopping;
        }

        private static void HandleCoordinatedRunStarted(CombatEncounterController encounter)
        {
            StageRunContext context = activeContext;
            if (context == null
                || context.LifecycleState != StageRunLifecycleState.StationActive
                || encounter == null
                || encounter.gameObject.scene.handle != context.CurrentSceneHandle)
            {
                return;
            }

            if (!TryRegisterStationCoordinator(encounter, out string error))
            {
                context.FailAbortClosure(
                    "Station coordinator registration failed at coordinated-run start: " + error);
            }
        }

        private static void HandleCoordinatedRunStopping(CombatEncounterController encounter)
        {
            StageRunContext context = activeContext;
            if (context == null
                || !context.CanAbortBeforeCommit()
                || encounter == null
                || encounter.gameObject.scene.handle != context.CurrentSceneHandle
                || context.LifecycleState == StageRunLifecycleState.CorridorActive
                || context.LifecycleState == StageRunLifecycleState.HandoffPending)
            {
                return;
            }

            if (!TryCaptureAndCancelStationCoordinator(
                    context,
                    encounter,
                    out long rootAdmissionSequence,
                    out long epoch,
                    out string cancellationError))
            {
                context.FailAbortClosure(
                    "Station coordinator stopped before cancellation could be verified: "
                    + cancellationError);
                return;
            }

            TryAbortActiveContext(
                StageRunAbortReason.UnexpectedSceneExit,
                StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested,
                rootAdmissionSequence,
                epoch,
                out _,
                out _);
        }

        private static void HandleActiveSceneChanged(Scene previous, Scene current)
        {
            StageRunContext context = activeContext;
            if (context == null
                || !context.CanAbortBeforeCommit()
                || context.IsExpectedHandoffDestination(current))
            {
                return;
            }

            StageRunAbortReason reason = context.LifecycleState == StageRunLifecycleState.HandoffPending
                ? StageRunAbortReason.WrongHandoffDestination
                : StageRunAbortReason.UnexpectedSceneExit;
            StageRunTerminalCoordinatorInvalidationDisposition disposition =
                context.LifecycleState == StageRunLifecycleState.CorridorActive
                    || context.LifecycleState == StageRunLifecycleState.HandoffPending
                    ? StageRunTerminalCoordinatorInvalidationDisposition.NotBoundBeforeStation
                    : StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested;
            long rootAdmissionSequence = 0;
            long epoch = 0;
            if (disposition
                == StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested)
            {
                if (!TryResolveAndCancelStationCoordinator(
                        context,
                        previous,
                        out rootAdmissionSequence,
                        out epoch,
                        out string cancellationError))
                {
                    context.FailAbortClosure(
                        "Active-scene exit could not verify Station coordinator cancellation: "
                        + cancellationError);
                    return;
                }
            }

            TryAbortActiveContext(
                reason,
                disposition,
                rootAdmissionSequence,
                epoch,
                out _,
                out _);
        }

        private static void HandleSceneUnloaded(Scene scene)
        {
            StageRunContext context = activeContext;
            if (context == null
                || !context.CanAbortBeforeCommit()
                || scene.handle != context.CurrentSceneHandle
                || context.LifecycleState == StageRunLifecycleState.HandoffPending)
            {
                return;
            }

            StageRunTerminalCoordinatorInvalidationDisposition disposition =
                context.LifecycleState == StageRunLifecycleState.CorridorActive
                    ? StageRunTerminalCoordinatorInvalidationDisposition.NotBoundBeforeStation
                    : StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested;
            long rootAdmissionSequence = 0;
            long epoch = 0;
            if (disposition
                == StageRunTerminalCoordinatorInvalidationDisposition.CancellationRequested
                && !TryCaptureAndCancelStationCoordinator(
                    context,
                    null,
                    out rootAdmissionSequence,
                    out epoch,
                    out string cancellationError))
            {
                context.FailAbortClosure(
                    "Scene unload completed before Station coordinator cancellation was verified: "
                    + cancellationError);
                return;
            }

            TryAbortActiveContext(
                StageRunAbortReason.UnexpectedSceneExit,
                disposition,
                rootAdmissionSequence,
                epoch,
                out _,
                out _);
        }

        private static bool TryResolveAndCancelStationCoordinator(
            StageRunContext context,
            Scene scene,
            out long rootAdmissionSequence,
            out long epoch,
            out string error)
        {
            rootAdmissionSequence = 0;
            epoch = 0;
            error = string.Empty;
            if (context == null
                || !scene.IsValid()
                || !scene.isLoaded
                || scene.handle != context.CurrentSceneHandle)
            {
                error = "The prior active scene is not the loaded scene owned by the active run.";
                return false;
            }

            if (IsRegisteredStationCoordinatorFor(context))
            {
                return TryCaptureAndCancelStationCoordinator(
                    context,
                    null,
                    out rootAdmissionSequence,
                    out epoch,
                    out error);
            }

            CombatEncounterController resolvedEncounter = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                CombatEncounterController[] encounters =
                    roots[rootIndex].GetComponentsInChildren<CombatEncounterController>(true);
                for (int encounterIndex = 0; encounterIndex < encounters.Length; encounterIndex++)
                {
                    CombatEncounterController encounter = encounters[encounterIndex];
                    EncounterTerminalResolutionCoordinator coordinator =
                        encounter != null && encounter.UsesCoordinatedTerminalResolution
                            ? encounter.TerminalCoordinator
                            : null;
                    if (coordinator == null)
                    {
                        continue;
                    }

                    if (resolvedEncounter != null)
                    {
                        error = "The active Station scene contains more than one live terminal coordinator.";
                        return false;
                    }

                    resolvedEncounter = encounter;
                }
            }

            if (resolvedEncounter == null)
            {
                error = "The active Station scene has no live terminal coordinator.";
                return false;
            }

            return TryCaptureAndCancelStationCoordinator(
                context,
                resolvedEncounter,
                out rootAdmissionSequence,
                out epoch,
                out error);
        }

        private static bool TryCaptureAndCancelStationCoordinator(
            StageRunContext context,
            CombatEncounterController encounter,
            out long rootAdmissionSequence,
            out long epoch,
            out string error)
        {
            rootAdmissionSequence = 0;
            epoch = 0;
            error = string.Empty;
            if (context == null)
            {
                error = "No active run owns a Station coordinator cancellation.";
                return false;
            }

            EncounterTerminalResolutionCoordinator coordinator;
            if (encounter != null)
            {
                if (!encounter.UsesCoordinatedTerminalResolution
                    || encounter.gameObject.scene.handle != context.CurrentSceneHandle
                    || encounter.TerminalCoordinator == null)
                {
                    error = "The supplied encounter is not the coordinated encounter in the active run scene.";
                    return false;
                }

                coordinator = encounter.TerminalCoordinator;
                if (registeredStationCoordinator != null
                    && !IsRegisteredStationCoordinatorFor(context, coordinator))
                {
                    error = "The supplied encounter conflicts with the registered Station coordinator.";
                    return false;
                }

                registeredStationCoordinator = coordinator;
                registeredStationCoordinatorSceneHandle = context.CurrentSceneHandle;
                registeredStationCoordinatorRunId = context.Identity.RunId;
            }
            else
            {
                if (!IsRegisteredStationCoordinatorFor(context))
                {
                    error = "The active run has no exact registered Station coordinator.";
                    return false;
                }

                coordinator = registeredStationCoordinator;
            }

            EncounterTerminalCoordinatorState stateBeforeCancellation = coordinator.State;
            if (stateBeforeCancellation == EncounterTerminalCoordinatorState.Unbound
                || stateBeforeCancellation == EncounterTerminalCoordinatorState.TerminalClosed
                || stateBeforeCancellation == EncounterTerminalCoordinatorState.Faulted
                || stateBeforeCancellation == EncounterTerminalCoordinatorState.Cancelled)
            {
                error = $"Station coordinator cannot accept cancellation from {stateBeforeCancellation}.";
                return false;
            }

            rootAdmissionSequence = coordinator.ActiveRootAdmissionSequence;
            epoch = coordinator.ActiveEpoch;
            bool activeResolution = stateBeforeCancellation == EncounterTerminalCoordinatorState.Open
                || stateBeforeCancellation == EncounterTerminalCoordinatorState.Draining
                || stateBeforeCancellation == EncounterTerminalCoordinatorState.Finalizing;
            if (activeResolution && (rootAdmissionSequence <= 0 || epoch <= 0))
            {
                error = "An active Station coordinator phase has no valid root/epoch authority.";
                rootAdmissionSequence = 0;
                epoch = 0;
                return false;
            }

            coordinator.Cancel();
            if (coordinator.State != EncounterTerminalCoordinatorState.Cancelled)
            {
                error = "Station coordinator did not enter Cancelled after cancellation was requested.";
                rootAdmissionSequence = 0;
                epoch = 0;
                return false;
            }

            return true;
        }

        private static bool IsRegisteredStationCoordinatorFor(StageRunContext context)
        {
            return IsRegisteredStationCoordinatorFor(context, registeredStationCoordinator);
        }

        private static bool IsRegisteredStationCoordinatorFor(
            StageRunContext context,
            EncounterTerminalResolutionCoordinator coordinator)
        {
            return context != null
                && coordinator != null
                && ReferenceEquals(registeredStationCoordinator, coordinator)
                && registeredStationCoordinatorSceneHandle == context.CurrentSceneHandle
                && string.Equals(
                    registeredStationCoordinatorRunId,
                    context.Identity.RunId,
                    StringComparison.Ordinal);
        }

        private static void ClearRegisteredStationCoordinator()
        {
            registeredStationCoordinator = null;
            registeredStationCoordinatorSceneHandle = 0;
            registeredStationCoordinatorRunId = string.Empty;
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }
    }
}
