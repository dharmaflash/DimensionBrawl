using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DimensionBrawl.LevelDesign
{
    [Flags]
    public enum StageRunSegmentRole
    {
        None = 0,
        Entry = 1,
        Terminal = 2
    }

    public enum StageRunTutorialFactRequirement
    {
        None = 0,
        LegacyCorridorCompletion = 1
    }

    public sealed class StageRunSegmentSnapshot
    {
        internal StageRunSegmentSnapshot(StageSceneSegmentRef source)
        {
            SegmentId = source.SegmentId ?? string.Empty;
            SequenceIndex = source.SequenceIndex;
            StageDefinitionId = source.StageDefinition.StageId ?? string.Empty;
            ScenePath = source.StageDefinition.MapScenePath ?? string.Empty;
            SceneName = Path.GetFileNameWithoutExtension(ScenePath) ?? string.Empty;
            EntryConditionId = source.EntryConditionId ?? string.Empty;
            EntryConditionKind = source.EntryConditionKind;
            ExitConditionId = source.ExitConditionId ?? string.Empty;
            ExitConditionKind = source.ExitConditionKind;
            HandoffPolicy = source.HandoffPolicy;
            SuccessorKind = source.SuccessorKind;
            DestinationSceneKind = source.DestinationSceneKind;
            TransitionTokenKind = source.TransitionTokenKind;
            LoaderGenerationKind = source.LoaderGenerationKind;
            NavigationAuthorityKind = source.NavigationAuthorityKind;
            ReturnOwnerKind = source.ReturnOwnerKind;
            ReturnOwnerReceiptPolicy = source.ReturnOwnerReceiptPolicy;
        }

        public string SegmentId { get; }
        public int SequenceIndex { get; }
        public string StageDefinitionId { get; }
        public string ScenePath { get; }
        public string SceneName { get; }
        public string EntryConditionId { get; }
        public StageSegmentConditionKind EntryConditionKind { get; }
        public string ExitConditionId { get; }
        public StageSegmentConditionKind ExitConditionKind { get; }
        public StageSceneHandoffPolicy HandoffPolicy { get; }
        public StageSegmentSuccessorKind SuccessorKind { get; }
        public StageSegmentDestinationSceneKind DestinationSceneKind { get; }
        public StageSegmentTransitionTokenKind TransitionTokenKind { get; }
        public StageSegmentLoaderGenerationKind LoaderGenerationKind { get; }
        public StageSegmentNavigationAuthorityKind NavigationAuthorityKind { get; }
        public StageSegmentReturnOwnerKind ReturnOwnerKind { get; }
        public StageReturnOwnerReceiptPolicy ReturnOwnerReceiptPolicy { get; }
    }

    public sealed class StageRunActionSnapshot
    {
        internal StageRunActionSnapshot(StageRouteActionRef source)
        {
            ActionId = source.ActionId ?? string.Empty;
            ActionKind = source.ActionKind;
            TargetPlayableStageId = source.TargetPlayableStageId ?? string.Empty;
            TargetUiRouteId = source.TargetUiRouteId;
            AllowedOutcomes = source.AllowedOutcomes;
        }

        public string ActionId { get; }
        public StageRouteActionKind ActionKind { get; }
        public string TargetPlayableStageId { get; }
        public StageUiRouteId TargetUiRouteId { get; }
        public StageRouteOutcome AllowedOutcomes { get; }

        public bool Allows(StageRouteOutcome outcome)
        {
            return outcome != StageRouteOutcome.None
                && (AllowedOutcomes & outcome) == outcome;
        }
    }

    public sealed class StageRunTerminalPolicySnapshot
    {
        internal StageRunTerminalPolicySnapshot(StageTerminalResolutionPolicy source)
        {
            PolicyId = source.TerminalResolutionPolicyId ?? string.Empty;
            SemanticRevision = source.SemanticRevision;
            PolicyDigest = source.TerminalResolutionPolicyDigest ?? string.Empty;
            WindowKind = source.WindowKind;
            BatchOwnerKind = source.BatchOwnerKind;
            RootAdmissionKind = source.RootAdmissionKind;
            RootOrderKind = source.RootOrderKind;
            RootIssuePoint = source.RootIssuePoint;
            BatchBoundaryKind = source.BatchBoundaryKind;
            TerminalSubjectRoles = source.TerminalSubjectRoles;
            CoveragePolicy = source.CoveragePolicy;
            WorkExecutionKind = source.WorkExecutionKind;
            NestedRequestPolicy = source.NestedRequestPolicy;
            IndependentRequestPolicy = source.IndependentRequestPolicy;
            EpochStampKind = source.EpochStampKind;
            CoordinatorLifecycleKind = source.CoordinatorLifecycleKind;
            SubjectFinalizationKind = source.SubjectFinalizationKind;
            TokenStatePolicy = source.TokenStatePolicy;
            FlushBarrier = source.FlushBarrier;
            SimultaneousOutcome = source.SimultaneousOutcome;
            RequiresBossCandidateAndFinalDead = source.RequiresBossCandidateAndFinalDead;
            RequiresPlayerCandidateAndFinalDown = source.RequiresPlayerCandidateAndFinalDown;
        }

        public string PolicyId { get; }
        public int SemanticRevision { get; }
        public string PolicyDigest { get; }
        public StageTerminalWindowKind WindowKind { get; }
        public StageTerminalBatchOwnerKind BatchOwnerKind { get; }
        public StageTerminalRootAdmissionKind RootAdmissionKind { get; }
        public StageTerminalRootOrderKind RootOrderKind { get; }
        public StageTerminalRootIssuePoint RootIssuePoint { get; }
        public StageTerminalBatchBoundaryKind BatchBoundaryKind { get; }
        public StageTerminalSubjectRole TerminalSubjectRoles { get; }
        public StageTerminalCoveragePolicy CoveragePolicy { get; }
        public StageTerminalWorkExecutionKind WorkExecutionKind { get; }
        public StageTerminalNestedRequestPolicy NestedRequestPolicy { get; }
        public StageTerminalIndependentRequestPolicy IndependentRequestPolicy { get; }
        public StageTerminalEpochStampKind EpochStampKind { get; }
        public StageTerminalCoordinatorLifecycleKind CoordinatorLifecycleKind { get; }
        public StageTerminalSubjectFinalizationKind SubjectFinalizationKind { get; }
        public StageTerminalTokenStatePolicy TokenStatePolicy { get; }
        public StageTerminalFlushBarrierKind FlushBarrier { get; }
        public StageTerminalSimultaneousOutcome SimultaneousOutcome { get; }
        public bool RequiresBossCandidateAndFinalDead { get; }
        public bool RequiresPlayerCandidateAndFinalDown { get; }

        public string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1024);
            AppendCanonicalFields(builder);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        internal void AppendCanonicalFields(StringBuilder builder)
        {
            StageCanonicalDigest.Append(builder, "policy.id", PolicyId);
            StageCanonicalDigest.Append(builder, "policy.revision", SemanticRevision);
            StageCanonicalDigest.Append(builder, "policy.window", (int)WindowKind);
            StageCanonicalDigest.Append(builder, "policy.batchOwner", (int)BatchOwnerKind);
            StageCanonicalDigest.Append(builder, "policy.rootAdmission", (int)RootAdmissionKind);
            StageCanonicalDigest.Append(builder, "policy.rootOrder", (int)RootOrderKind);
            StageCanonicalDigest.Append(builder, "policy.rootIssuePoint", (int)RootIssuePoint);
            StageCanonicalDigest.Append(builder, "policy.batchBoundary", (int)BatchBoundaryKind);
            StageCanonicalDigest.Append(builder, "policy.subjectRoles", (int)TerminalSubjectRoles);
            StageCanonicalDigest.Append(builder, "policy.coverage", (int)CoveragePolicy);
            StageCanonicalDigest.Append(builder, "policy.workExecution", (int)WorkExecutionKind);
            StageCanonicalDigest.Append(builder, "policy.nested", (int)NestedRequestPolicy);
            StageCanonicalDigest.Append(builder, "policy.independent", (int)IndependentRequestPolicy);
            StageCanonicalDigest.Append(builder, "policy.epochStamp", (int)EpochStampKind);
            StageCanonicalDigest.Append(builder, "policy.lifecycle", (int)CoordinatorLifecycleKind);
            StageCanonicalDigest.Append(builder, "policy.subjectFinalization", (int)SubjectFinalizationKind);
            StageCanonicalDigest.Append(builder, "policy.tokenStates", (int)TokenStatePolicy);
            StageCanonicalDigest.Append(builder, "policy.flushBarrier", (int)FlushBarrier);
            StageCanonicalDigest.Append(builder, "policy.simultaneousOutcome", (int)SimultaneousOutcome);
            StageCanonicalDigest.Append(
                builder,
                "policy.requiresBossCandidateAndFinalDead",
                RequiresBossCandidateAndFinalDead);
            StageCanonicalDigest.Append(
                builder,
                "policy.requiresPlayerCandidateAndFinalDown",
                RequiresPlayerCandidateAndFinalDown);
        }
    }

    public sealed class StageRunRouteSnapshot
    {
        private const string OlympusInvasionPlayableStageId = "OLYMPUS-INVASION-01";
        private const string RunEntryConditionId = "run.entry.admitted";
        private const string CorridorCompletedConditionId = "corridor.tutorial.completed";
        private const string StationEntryReachedConditionId = "corridor.station-entry.reached";
        private const string StationTerminalConditionId = "station.encounter.terminal";

        private readonly StageRunSegmentSnapshot[] segments;
        private readonly StageRunActionSnapshot[] actions;

        private StageRunRouteSnapshot(
            int schemaVersion,
            string playableStageId,
            int routeRevision,
            string canonicalDigest,
            StageRunSegmentSnapshot[] segments,
            StageRunActionSnapshot[] actions,
            StageRunTerminalPolicySnapshot terminalPolicy)
        {
            SchemaVersion = schemaVersion;
            PlayableStageId = playableStageId;
            RouteRevision = routeRevision;
            CanonicalDigest = canonicalDigest;
            this.segments = segments;
            this.actions = actions;
            TerminalPolicy = terminalPolicy;
        }

        public int SchemaVersion { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string CanonicalDigest { get; }
        public string CoreRouteSemanticDigest => CanonicalDigest;
        public int SegmentCount => segments.Length;
        public int ActionCount => actions.Length;
        public StageRunTerminalPolicySnapshot TerminalPolicy { get; }
        public StageRunTutorialFactRequirement TutorialFactRequirement =>
            ResolveTutorialFactRequirement();

        public StageRunSegmentSnapshot GetSegment(int index)
        {
            if (index < 0 || index >= segments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return segments[index];
        }

        public StageRunActionSnapshot GetAction(int index)
        {
            if (index < 0 || index >= actions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return actions[index];
        }

        public StageRunSegmentRole GetSegmentRoles(int index)
        {
            StageRunSegmentSnapshot segment = GetSegment(index);
            StageRunSegmentRole roles = index == 0
                ? StageRunSegmentRole.Entry
                : StageRunSegmentRole.None;
            if (index == segments.Length - 1
                && segment.HandoffPolicy == StageSceneHandoffPolicy.ReturnToOwner)
            {
                roles |= StageRunSegmentRole.Terminal;
            }

            return roles;
        }

        public bool IsEntrySegment(int index)
        {
            return (GetSegmentRoles(index) & StageRunSegmentRole.Entry) != 0;
        }

        public bool IsTerminalSegment(int index)
        {
            return (GetSegmentRoles(index) & StageRunSegmentRole.Terminal) != 0;
        }

        private StageRunTutorialFactRequirement ResolveTutorialFactRequirement()
        {
            if (segments.Length <= 1)
            {
                return StageRunTutorialFactRequirement.None;
            }

            StageSegmentConditionKind firstExitKind = segments[0].ExitConditionKind;
            return firstExitKind
                    == StageSegmentConditionKind.CorridorTutorialFactsAndClosureSealedForSingleLoad
                || firstExitKind
                    == StageSegmentConditionKind
                        .CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance
                    ? StageRunTutorialFactRequirement.LegacyCorridorCompletion
                    : StageRunTutorialFactRequirement.None;
        }

        public bool TryGetAction(string actionId, out StageRunActionSnapshot action)
        {
            if (!string.IsNullOrWhiteSpace(actionId))
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    if (string.Equals(actions[i].ActionId, actionId, StringComparison.Ordinal))
                    {
                        action = actions[i];
                        return true;
                    }
                }
            }

            action = null;
            return false;
        }

        public string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "route.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "route.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "route.revision", RouteRevision);
            StageCanonicalDigest.Append(builder, "route.segmentCount", segments.Length);
            for (int i = 0; i < segments.Length; i++)
            {
                StageRunSegmentSnapshot segment = segments[i];
                string prefix = $"route.segment[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", segment.SegmentId);
                StageCanonicalDigest.Append(builder, prefix + ".sequence", segment.SequenceIndex);
                StageCanonicalDigest.Append(builder, prefix + ".stageDefinitionId", segment.StageDefinitionId);
                StageCanonicalDigest.Append(builder, prefix + ".scenePath", segment.ScenePath);
                StageCanonicalDigest.Append(builder, prefix + ".entryCondition", segment.EntryConditionId);
                StageCanonicalDigest.Append(builder, prefix + ".entryConditionKind", (int)segment.EntryConditionKind);
                StageCanonicalDigest.Append(builder, prefix + ".exitCondition", segment.ExitConditionId);
                StageCanonicalDigest.Append(builder, prefix + ".exitConditionKind", (int)segment.ExitConditionKind);
                StageCanonicalDigest.Append(builder, prefix + ".handoffPolicy", (int)segment.HandoffPolicy);
                StageCanonicalDigest.Append(builder, prefix + ".successor", (int)segment.SuccessorKind);
                StageCanonicalDigest.Append(builder, prefix + ".destinationScene", (int)segment.DestinationSceneKind);
                StageCanonicalDigest.Append(builder, prefix + ".transitionToken", (int)segment.TransitionTokenKind);
                StageCanonicalDigest.Append(builder, prefix + ".loaderGeneration", (int)segment.LoaderGenerationKind);
                StageCanonicalDigest.Append(builder, prefix + ".navigationAuthority", (int)segment.NavigationAuthorityKind);
                StageCanonicalDigest.Append(builder, prefix + ".returnOwner", (int)segment.ReturnOwnerKind);
                StageCanonicalDigest.Append(builder, prefix + ".returnReceipt", (int)segment.ReturnOwnerReceiptPolicy);
            }

            StageRunActionSnapshot[] sortedActions = (StageRunActionSnapshot[])actions.Clone();
            Array.Sort(
                sortedActions,
                (left, right) => string.Compare(left.ActionId, right.ActionId, StringComparison.Ordinal));
            StageCanonicalDigest.Append(builder, "route.actionCount", sortedActions.Length);
            for (int i = 0; i < sortedActions.Length; i++)
            {
                StageRunActionSnapshot action = sortedActions[i];
                string prefix = $"route.action[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", action.ActionId);
                StageCanonicalDigest.Append(builder, prefix + ".kind", (int)action.ActionKind);
                StageCanonicalDigest.Append(builder, prefix + ".playableTarget", action.TargetPlayableStageId);
                StageCanonicalDigest.Append(builder, prefix + ".uiRouteTarget", (int)action.TargetUiRouteId);
                StageCanonicalDigest.Append(builder, prefix + ".allowedOutcomes", (int)action.AllowedOutcomes);
            }

            TerminalPolicy.AppendCanonicalFields(builder);
            StageCanonicalDigest.Append(builder, "route.terminalPolicyDigest", TerminalPolicy.PolicyDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        public static bool TryCreate(
            PlayableStageDefinition definition,
            out StageRunRouteSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (definition == null)
            {
                error = "PlayableStageDefinition is missing.";
                return false;
            }

            if (definition.SchemaVersion != 1
                || string.IsNullOrWhiteSpace(definition.PlayableStageId)
                || definition.RouteRevision < 1)
            {
                error = "Playable stage route identity is invalid.";
                return false;
            }

            if (definition.SceneSegmentCount <= 0 || definition.TerminalResolutionPolicy == null)
            {
                error = "Playable stage route is incomplete.";
                return false;
            }

            StageRunSegmentSnapshot[] copiedSegments = new StageRunSegmentSnapshot[definition.SceneSegmentCount];
            for (int i = 0; i < copiedSegments.Length; i++)
            {
                StageSceneSegmentRef source = definition.GetSceneSegment(i);
                if (source == null || source.StageDefinition == null)
                {
                    error = $"Route segment {i.ToString(CultureInfo.InvariantCulture)} has no stage definition.";
                    return false;
                }

                copiedSegments[i] = new StageRunSegmentSnapshot(source);
                if (string.IsNullOrWhiteSpace(copiedSegments[i].SegmentId)
                    || string.IsNullOrWhiteSpace(copiedSegments[i].StageDefinitionId)
                    || string.IsNullOrWhiteSpace(copiedSegments[i].ScenePath))
                {
                    error = $"Route segment {i.ToString(CultureInfo.InvariantCulture)} has incomplete identity.";
                    return false;
                }
            }

            if (!ValidateBoundedTopology(
                    definition.PlayableStageId,
                    copiedSegments,
                    out error))
            {
                return false;
            }

            if (!ValidateOlympusConditionSemantics(
                definition.PlayableStageId,
                definition.RouteRevision,
                copiedSegments,
                out error))
            {
                return false;
            }

            if (!ValidateTransitionShapes(copiedSegments, out error))
            {
                return false;
            }

            StageRunActionSnapshot[] copiedActions = new StageRunActionSnapshot[definition.TerminalActionCount];
            var actionIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < copiedActions.Length; i++)
            {
                StageRouteActionRef source = definition.GetTerminalAction(i);
                if (source == null
                    || string.IsNullOrWhiteSpace(source.ActionId)
                    || !actionIds.Add(source.ActionId))
                {
                    error = $"Route action {i.ToString(CultureInfo.InvariantCulture)} has incomplete or duplicate identity.";
                    return false;
                }

                copiedActions[i] = new StageRunActionSnapshot(source);
            }

            StageRunTerminalPolicySnapshot copiedPolicy =
                new(definition.TerminalResolutionPolicy);
            string computedPolicyDigest = copiedPolicy.ComputeCanonicalDigest();
            if (!string.Equals(copiedPolicy.PolicyDigest, computedPolicyDigest, StringComparison.Ordinal))
            {
                error = $"Terminal policy digest mismatch. expected={computedPolicyDigest}, stored={copiedPolicy.PolicyDigest}.";
                return false;
            }

            var candidate = new StageRunRouteSnapshot(
                definition.SchemaVersion,
                definition.PlayableStageId ?? string.Empty,
                definition.RouteRevision,
                definition.CanonicalRouteDigest ?? string.Empty,
                copiedSegments,
                copiedActions,
                copiedPolicy);
            string computedRouteDigest = candidate.ComputeCanonicalDigest();
            if (!string.Equals(candidate.CanonicalDigest, computedRouteDigest, StringComparison.Ordinal))
            {
                error = $"Route digest mismatch. expected={computedRouteDigest}, stored={candidate.CanonicalDigest}.";
                return false;
            }

            snapshot = candidate;
            return true;
        }

        private static bool ValidateBoundedTopology(
            string playableStageId,
            StageRunSegmentSnapshot[] copiedSegments,
            out string error)
        {
            error = string.Empty;
            if (copiedSegments.Length < 1 || copiedSegments.Length > 2)
            {
                error = "Playable stage route must contain one or two ordered segments.";
                return false;
            }

            var segmentIds = new HashSet<string>(StringComparer.Ordinal);
            var semanticConditionIds = new HashSet<string>(StringComparer.Ordinal)
            {
                copiedSegments[0].EntryConditionId
            };
            if (!string.Equals(
                    copiedSegments[0].EntryConditionId,
                    RunEntryConditionId,
                    StringComparison.Ordinal)
                || copiedSegments[0].EntryConditionKind
                    != StageSegmentConditionKind.RunEntrySnapshotValidatedAndFirstSegmentActivated)
            {
                error = "First route segment must carry the exact run-entry admission condition.";
                return false;
            }

            for (int i = 0; i < copiedSegments.Length; i++)
            {
                StageRunSegmentSnapshot segment = copiedSegments[i];
                bool isFinal = i == copiedSegments.Length - 1;
                if (segment.SequenceIndex != i
                    || !segmentIds.Add(segment.SegmentId)
                    || string.IsNullOrWhiteSpace(segment.EntryConditionId)
                    || string.IsNullOrWhiteSpace(segment.ExitConditionId))
                {
                    error = $"Route segment {i.ToString(CultureInfo.InvariantCulture)} has invalid sequence, duplicate identity, or missing conditions.";
                    return false;
                }

                if (i > 0)
                {
                    StageRunSegmentSnapshot source = copiedSegments[i - 1];
                    if (!string.Equals(
                            source.ExitConditionId,
                            segment.EntryConditionId,
                            StringComparison.Ordinal)
                        || source.ExitConditionKind != segment.EntryConditionKind)
                    {
                        error = $"Route boundary before segment {i.ToString(CultureInfo.InvariantCulture)} does not preserve the exact exit/entry condition.";
                        return false;
                    }
                }

                if (!semanticConditionIds.Add(segment.ExitConditionId))
                {
                    error = $"Route segment {i.ToString(CultureInfo.InvariantCulture)} reuses a condition ID for a different semantic boundary.";
                    return false;
                }

                if (isFinal)
                {
                    if (segment.ExitConditionKind
                        != StageSegmentConditionKind
                            .StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched)
                    {
                        error = "Final route segment must carry the terminal queue/finalization condition kind.";
                        return false;
                    }

                    if (!string.Equals(
                            playableStageId,
                            OlympusInvasionPlayableStageId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            segment.ExitConditionId,
                            StationTerminalConditionId,
                            StringComparison.Ordinal))
                    {
                        error = "A non-Olympus route must own a distinct terminal condition ID.";
                        return false;
                    }

                    continue;
                }

                StageSegmentConditionKind expectedBoundaryKind = segment.HandoffPolicy switch
                {
                    StageSceneHandoffPolicy.SingleLoad =>
                        StageSegmentConditionKind
                            .CorridorTutorialFactsAndClosureSealedForSingleLoad,
                    StageSceneHandoffPolicy.InSceneAdvance =>
                        StageSegmentConditionKind
                            .CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance,
                    _ => 0
                };
                if (expectedBoundaryKind == 0
                    || segment.ExitConditionKind != expectedBoundaryKind)
                {
                    error = $"Non-final route segment {i.ToString(CultureInfo.InvariantCulture)} has an invalid typed boundary condition.";
                    return false;
                }

                if (segment.HandoffPolicy == StageSceneHandoffPolicy.InSceneAdvance
                    && !string.Equals(
                        segment.ScenePath,
                        copiedSegments[i + 1].ScenePath,
                        StringComparison.Ordinal))
                {
                    error = $"In-scene route segment {i.ToString(CultureInfo.InvariantCulture)} does not share its successor scene.";
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateOlympusConditionSemantics(
            string playableStageId,
            int routeRevision,
            StageRunSegmentSnapshot[] copiedSegments,
            out string error)
        {
            error = string.Empty;
            if (!string.Equals(
                playableStageId,
                OlympusInvasionPlayableStageId,
                StringComparison.Ordinal)
                || (routeRevision != 1 && routeRevision != 2))
            {
                return true;
            }

            bool valid = copiedSegments.Length == 2
                && string.Equals(
                    copiedSegments[0].EntryConditionId,
                    RunEntryConditionId,
                    StringComparison.Ordinal)
                && copiedSegments[0].EntryConditionKind
                    == StageSegmentConditionKind.RunEntrySnapshotValidatedAndFirstSegmentActivated
                && string.Equals(
                    copiedSegments[0].ExitConditionId,
                    routeRevision == 1
                        ? CorridorCompletedConditionId
                        : StationEntryReachedConditionId,
                    StringComparison.Ordinal)
                && copiedSegments[0].ExitConditionKind
                    == (routeRevision == 1
                        ? StageSegmentConditionKind.CorridorTutorialFactsAndClosureSealedForSingleLoad
                        : StageSegmentConditionKind.CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance)
                && string.Equals(
                    copiedSegments[1].EntryConditionId,
                    routeRevision == 1
                        ? CorridorCompletedConditionId
                        : StationEntryReachedConditionId,
                    StringComparison.Ordinal)
                && copiedSegments[1].EntryConditionKind
                    == (routeRevision == 1
                        ? StageSegmentConditionKind.CorridorTutorialFactsAndClosureSealedForSingleLoad
                        : StageSegmentConditionKind.CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance)
                && string.Equals(
                    copiedSegments[1].ExitConditionId,
                    StationTerminalConditionId,
                    StringComparison.Ordinal)
                && copiedSegments[1].ExitConditionKind
                    == StageSegmentConditionKind.StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched;
            if (valid)
            {
                return true;
            }

            error = $"Olympus Invasion route revision {routeRevision} condition semantics are immutable. "
                + "A semantic change requires a new condition ID and a route revision/digest bump.";
            return false;
        }

        private static bool ValidateTransitionShapes(
            StageRunSegmentSnapshot[] copiedSegments,
            out string error)
        {
            error = string.Empty;
            for (int i = 0; i < copiedSegments.Length; i++)
            {
                StageRunSegmentSnapshot segment = copiedSegments[i];
                bool isFinal = i == copiedSegments.Length - 1;
                bool valid = segment.SequenceIndex == i;
                switch (segment.HandoffPolicy)
                {
                    case StageSceneHandoffPolicy.SingleLoad:
                        valid = valid
                            && !isFinal
                            && segment.SuccessorKind == StageSegmentSuccessorKind.NextOrderedSegment
                            && segment.DestinationSceneKind
                                == StageSegmentDestinationSceneKind.SuccessorStageDefinitionScene
                            && segment.TransitionTokenKind
                                == StageSegmentTransitionTokenKind.SealedCurrentRunSegmentHandoff
                            && segment.LoaderGenerationKind
                                == StageSegmentLoaderGenerationKind.ActiveRunRouteLoaderGeneration
                            && segment.NavigationAuthorityKind
                                == StageSegmentNavigationAuthorityKind.P1AStageRunRouteOwner
                            && segment.ReturnOwnerKind == StageSegmentReturnOwnerKind.None
                            && segment.ReturnOwnerReceiptPolicy == StageReturnOwnerReceiptPolicy.None;
                        break;
                    case StageSceneHandoffPolicy.InSceneAdvance:
                        valid = valid
                            && !isFinal
                            && segment.SuccessorKind == StageSegmentSuccessorKind.NextOrderedSegment
                            && segment.DestinationSceneKind
                                == StageSegmentDestinationSceneKind.SuccessorStageDefinitionScene
                            && segment.TransitionTokenKind
                                == StageSegmentTransitionTokenKind.SealedCurrentRunSegmentHandoff
                            && segment.LoaderGenerationKind == StageSegmentLoaderGenerationKind.None
                            && segment.NavigationAuthorityKind
                                == StageSegmentNavigationAuthorityKind.P1AStageRunRouteOwner
                            && segment.ReturnOwnerKind == StageSegmentReturnOwnerKind.None
                            && segment.ReturnOwnerReceiptPolicy == StageReturnOwnerReceiptPolicy.None;
                        break;
                    case StageSceneHandoffPolicy.ReturnToOwner:
                        valid = valid
                            && isFinal
                            && segment.SuccessorKind == StageSegmentSuccessorKind.None
                            && segment.DestinationSceneKind == StageSegmentDestinationSceneKind.None
                            && segment.TransitionTokenKind == StageSegmentTransitionTokenKind.None
                            && segment.LoaderGenerationKind == StageSegmentLoaderGenerationKind.None
                            && segment.NavigationAuthorityKind
                                == StageSegmentNavigationAuthorityKind.None
                            && segment.ReturnOwnerKind
                                == StageSegmentReturnOwnerKind.P1AStageRunRouteOwner
                            && segment.ReturnOwnerReceiptPolicy
                                == StageReturnOwnerReceiptPolicy.ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented;
                        break;
                    default:
                        valid = false;
                        break;
                }

                if (!valid)
                {
                    error = $"Route segment {i.ToString(CultureInfo.InvariantCulture)} has an invalid typed transition shape.";
                    return false;
                }
            }

            return true;
        }
    }
}
