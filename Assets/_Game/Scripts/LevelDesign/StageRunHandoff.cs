using System;
using System.Text;
using UnityEngine.SceneManagement;
using static DimensionBrawl.LevelDesign.StageRunHandoffDigest;

namespace DimensionBrawl.LevelDesign
{
    public enum StageSegmentHandoffClosedDisposition
    {
        DestinationBound = 1,
        ClosedBeforeDestination = 2
    }

    public sealed class StageRunHandoffToken
    {
        internal StageRunHandoffToken(
            StageRunIdentity identity,
            long sequence,
            long loaderGeneration,
            long issuedSequence,
            StageRunSegmentSnapshot source,
            StageRunSegmentSnapshot destination)
        {
            TokenId = $"{identity.RunId}:handoff:{sequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteDigest = identity.RouteSnapshotDigest;
            Sequence = sequence;
            SourceSegmentId = source.SegmentId;
            SourceStageDefinitionId = source.StageDefinitionId;
            DestinationSegmentId = destination.SegmentId;
            DestinationStageDefinitionId = destination.StageDefinitionId;
            ConditionId = source.ExitConditionId;
            DestinationSceneName = destination.SceneName;
            DestinationScenePath = destination.ScenePath;
            LoaderGeneration = loaderGeneration;
            IssuedSequence = issuedSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("stage-run-handoff-token", CanonicalDigest);
        }

        public string TokenId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteDigest { get; }
        public long Sequence { get; }
        public string SourceSegmentId { get; }
        public string SourceStageDefinitionId { get; }
        public string DestinationSegmentId { get; }
        public string DestinationStageDefinitionId { get; }
        public string ConditionId { get; }
        public string DestinationSceneName { get; }
        public string DestinationScenePath { get; }
        public long LoaderGeneration { get; }
        public long IssuedSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1536);
            StageCanonicalDigest.Append(builder, "handoffToken.id", TokenId);
            StageCanonicalDigest.Append(builder, "handoffToken.runId", RunId);
            StageCanonicalDigest.Append(builder, "handoffToken.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "handoffToken.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "handoffToken.routeDigest", RouteDigest);
            StageCanonicalDigest.Append(builder, "handoffToken.sequence", Sequence);
            StageCanonicalDigest.Append(builder, "handoffToken.sourceSegmentId", SourceSegmentId);
            StageCanonicalDigest.Append(builder, "handoffToken.sourceStageDefinitionId", SourceStageDefinitionId);
            StageCanonicalDigest.Append(builder, "handoffToken.destinationSegmentId", DestinationSegmentId);
            StageCanonicalDigest.Append(
                builder,
                "handoffToken.destinationStageDefinitionId",
                DestinationStageDefinitionId);
            StageCanonicalDigest.Append(builder, "handoffToken.conditionId", ConditionId);
            StageCanonicalDigest.Append(builder, "handoffToken.destinationSceneName", DestinationSceneName);
            StageCanonicalDigest.Append(builder, "handoffToken.destinationScenePath", DestinationScenePath);
            StageCanonicalDigest.Append(builder, "handoffToken.loaderGeneration", LoaderGeneration);
            StageCanonicalDigest.Append(builder, "handoffToken.issuedSequence", IssuedSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageRunSingleLoadDispatch
    {
        internal StageRunSingleLoadDispatch(StageRunHandoffToken token)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            DispatchId = $"{token.RunId}:single-load-dispatch:{token.Sequence}";
            DestinationSceneName = token.DestinationSceneName;
            DestinationScenePath = token.DestinationScenePath;
            LoaderGeneration = token.LoaderGeneration;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("stage-run-single-load-dispatch", CanonicalDigest);
        }

        public string DispatchId { get; }
        public StageRunHandoffToken Token { get; }
        public string DestinationSceneName { get; }
        public string DestinationScenePath { get; }
        public long LoaderGeneration { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(768);
            StageCanonicalDigest.Append(builder, "singleLoadDispatch.id", DispatchId);
            StageCanonicalDigest.Append(builder, "singleLoadDispatch.tokenId", Token.TokenId);
            StageCanonicalDigest.Append(builder, "singleLoadDispatch.tokenDigest", Token.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "singleLoadDispatch.destinationSceneName",
                DestinationSceneName);
            StageCanonicalDigest.Append(
                builder,
                "singleLoadDispatch.destinationScenePath",
                DestinationScenePath);
            StageCanonicalDigest.Append(builder, "singleLoadDispatch.loaderGeneration", LoaderGeneration);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageSegmentEntryReceipt
    {
        internal StageSegmentEntryReceipt(
            StageRunIdentity identity,
            StageRunHandoffToken token,
            StageRunSegmentSnapshot destination,
            Scene actualScene,
            long entrySequence)
            : this(
                identity,
                token,
                destination,
                actualScene,
                fromHandoffPending: true,
                entrySequence: entrySequence)
        {
        }

        internal StageSegmentEntryReceipt(
            StageRunIdentity identity,
            StageRunHandoffToken token,
            StageRunSegmentSnapshot destination,
            Scene actualScene,
            bool fromHandoffPending,
            long entrySequence)
        {
            SegmentEntryReceiptId = $"{identity.RunId}:segment-entry:{destination.SequenceIndex}:{entrySequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            SourceSegmentId = token.SourceSegmentId;
            DestinationSegmentId = destination.SegmentId;
            DestinationStageDefinitionId = destination.StageDefinitionId;
            TransitionTokenId = token.TokenId;
            TransitionTokenDigest = token.CanonicalDigest;
            RequestedSceneName = token.DestinationSceneName;
            RequestedScenePath = token.DestinationScenePath;
            ActualSceneName = actualScene.name ?? string.Empty;
            ActualScenePath = NormalizePath(actualScene.path);
            FromHandoffPending = fromHandoffPending;
            ToDestinationActive = true;
            EntrySequence = entrySequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("stage-segment-entry-receipt", CanonicalDigest);
        }

        public string SegmentEntryReceiptId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public string SourceSegmentId { get; }
        public string DestinationSegmentId { get; }
        public string DestinationStageDefinitionId { get; }
        public string TransitionTokenId { get; }
        public string TransitionTokenDigest { get; }
        public string RequestedSceneName { get; }
        public string RequestedScenePath { get; }
        public string ActualSceneName { get; }
        public string ActualScenePath { get; }
        public bool FromHandoffPending { get; }
        public bool ToDestinationActive { get; }
        public long EntrySequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1536);
            StageCanonicalDigest.Append(builder, "entry.id", SegmentEntryReceiptId);
            StageCanonicalDigest.Append(builder, "entry.runId", RunId);
            StageCanonicalDigest.Append(builder, "entry.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "entry.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "entry.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "entry.sourceSegmentId", SourceSegmentId);
            StageCanonicalDigest.Append(builder, "entry.destinationSegmentId", DestinationSegmentId);
            StageCanonicalDigest.Append(
                builder,
                "entry.destinationStageDefinitionId",
                DestinationStageDefinitionId);
            StageCanonicalDigest.Append(builder, "entry.transitionTokenId", TransitionTokenId);
            StageCanonicalDigest.Append(builder, "entry.transitionTokenDigest", TransitionTokenDigest);
            StageCanonicalDigest.Append(builder, "entry.requestedSceneName", RequestedSceneName);
            StageCanonicalDigest.Append(builder, "entry.requestedScenePath", RequestedScenePath);
            StageCanonicalDigest.Append(builder, "entry.actualSceneName", ActualSceneName);
            StageCanonicalDigest.Append(builder, "entry.actualScenePath", ActualScenePath);
            StageCanonicalDigest.Append(builder, "entry.fromHandoffPending", FromHandoffPending);
            StageCanonicalDigest.Append(builder, "entry.toDestinationActive", ToDestinationActive);
            StageCanonicalDigest.Append(builder, "entry.sequence", EntrySequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }
    }

    public sealed class StageSegmentHandoffTerminalReceipt
    {
        internal StageSegmentHandoffTerminalReceipt(
            StageRunIdentity identity,
            StageRunHandoffToken token,
            StageSegmentEntryReceipt entryReceipt,
            long terminalSequence)
            : this(
                identity,
                token,
                entryReceipt,
                loaderGenerationInvalidated: true,
                terminalSequence: terminalSequence)
        {
        }

        internal StageSegmentHandoffTerminalReceipt(
            StageRunIdentity identity,
            StageRunHandoffToken token,
            StageSegmentEntryReceipt entryReceipt,
            bool loaderGenerationInvalidated,
            long terminalSequence)
        {
            SegmentHandoffTerminalReceiptId =
                $"{identity.RunId}:segment-handoff-terminal:{token.Sequence}:{terminalSequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            TransitionTokenId = token.TokenId;
            TransitionTokenDigest = token.CanonicalDigest;
            LoaderGeneration = token.LoaderGeneration;
            RequestedDestinationSceneName = token.DestinationSceneName;
            RequestedDestinationScenePath = token.DestinationScenePath;
            Disposition = StageSegmentHandoffClosedDisposition.DestinationBound;
            SegmentEntryReceiptId = entryReceipt.SegmentEntryReceiptId;
            SegmentEntryReceiptDigest = entryReceipt.CanonicalDigest;
            CloseAuthorityKind = string.Empty;
            CloseAuthorityId = string.Empty;
            CloseAuthorityDigest = string.Empty;
            CloseReason = string.Empty;
            LoaderGenerationInvalidated = loaderGenerationInvalidated;
            PendingLoadCallbackCount = 0;
            PendingBindCallbackCount = 0;
            PendingUnloadCallbackCount = 0;
            RejectLateBindAfterTerminal = true;
            TerminalSequence = terminalSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum(
                "stage-segment-handoff-terminal-receipt",
                CanonicalDigest);
        }

        internal StageSegmentHandoffTerminalReceipt(
            StageRunIdentity identity,
            StageRunHandoffToken token,
            StageRunAbortCloseAuthority closeAuthority,
            string closeReason,
            long terminalSequence)
        {
            SegmentHandoffTerminalReceiptId =
                $"{identity.RunId}:segment-handoff-terminal:{token.Sequence}:{terminalSequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            TransitionTokenId = token.TokenId;
            TransitionTokenDigest = token.CanonicalDigest;
            LoaderGeneration = token.LoaderGeneration;
            RequestedDestinationSceneName = token.DestinationSceneName;
            RequestedDestinationScenePath = token.DestinationScenePath;
            Disposition = StageSegmentHandoffClosedDisposition.ClosedBeforeDestination;
            SegmentEntryReceiptId = string.Empty;
            SegmentEntryReceiptDigest = string.Empty;
            CloseAuthorityKind = nameof(StageRunAbortCloseAuthority);
            CloseAuthorityId = closeAuthority.AbortCloseAuthorityId;
            CloseAuthorityDigest = closeAuthority.CanonicalDigest;
            CloseReason = closeReason ?? string.Empty;
            LoaderGenerationInvalidated = true;
            PendingLoadCallbackCount = 0;
            PendingBindCallbackCount = 0;
            PendingUnloadCallbackCount = 0;
            RejectLateBindAfterTerminal = true;
            TerminalSequence = terminalSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum(
                "stage-segment-handoff-terminal-receipt",
                CanonicalDigest);
        }

        public string SegmentHandoffTerminalReceiptId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public string TransitionTokenId { get; }
        public string TransitionTokenDigest { get; }
        public long LoaderGeneration { get; }
        public string RequestedDestinationSceneName { get; }
        public string RequestedDestinationScenePath { get; }
        public StageSegmentHandoffClosedDisposition Disposition { get; }
        public string SegmentEntryReceiptId { get; }
        public string SegmentEntryReceiptDigest { get; }
        public string CloseAuthorityKind { get; }
        public string CloseAuthorityId { get; }
        public string CloseAuthorityDigest { get; }
        public string CloseReason { get; }
        public bool LoaderGenerationInvalidated { get; }
        public int PendingLoadCallbackCount { get; }
        public int PendingBindCallbackCount { get; }
        public int PendingUnloadCallbackCount { get; }
        public bool RejectLateBindAfterTerminal { get; }
        public long TerminalSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1792);
            StageCanonicalDigest.Append(builder, "handoffTerminal.id", SegmentHandoffTerminalReceiptId);
            StageCanonicalDigest.Append(builder, "handoffTerminal.runId", RunId);
            StageCanonicalDigest.Append(builder, "handoffTerminal.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "handoffTerminal.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "handoffTerminal.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "handoffTerminal.tokenId", TransitionTokenId);
            StageCanonicalDigest.Append(builder, "handoffTerminal.tokenDigest", TransitionTokenDigest);
            StageCanonicalDigest.Append(builder, "handoffTerminal.loaderGeneration", LoaderGeneration);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.requestedDestinationSceneName",
                RequestedDestinationSceneName);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.requestedDestinationScenePath",
                RequestedDestinationScenePath);
            StageCanonicalDigest.Append(builder, "handoffTerminal.disposition", (int)Disposition);
            StageCanonicalDigest.Append(builder, "handoffTerminal.entryReceiptId", SegmentEntryReceiptId);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.entryReceiptDigest",
                SegmentEntryReceiptDigest);
            StageCanonicalDigest.Append(builder, "handoffTerminal.closeAuthorityKind", CloseAuthorityKind);
            StageCanonicalDigest.Append(builder, "handoffTerminal.closeAuthorityId", CloseAuthorityId);
            StageCanonicalDigest.Append(builder, "handoffTerminal.closeAuthorityDigest", CloseAuthorityDigest);
            StageCanonicalDigest.Append(builder, "handoffTerminal.closeReason", CloseReason);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.loaderGenerationInvalidated",
                LoaderGenerationInvalidated);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.pendingLoadCallbackCount",
                PendingLoadCallbackCount);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.pendingBindCallbackCount",
                PendingBindCallbackCount);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.pendingUnloadCallbackCount",
                PendingUnloadCallbackCount);
            StageCanonicalDigest.Append(
                builder,
                "handoffTerminal.rejectLateBindAfterTerminal",
                RejectLateBindAfterTerminal);
            StageCanonicalDigest.Append(builder, "handoffTerminal.sequence", TerminalSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    internal static class StageRunHandoffDigest
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
