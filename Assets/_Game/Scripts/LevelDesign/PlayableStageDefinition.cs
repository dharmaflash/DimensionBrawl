using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.Playables;

namespace DimensionBrawl.LevelDesign
{
    public enum StageSceneHandoffPolicy
    {
        SingleLoad = 1,
        Additive = 2,
        ReturnToOwner = 3,
        InSceneAdvance = 4
    }

    public enum StageSegmentConditionKind
    {
        RunEntrySnapshotValidatedAndFirstSegmentActivated = 1,
        CorridorTutorialFactsAndClosureSealedForSingleLoad = 2,
        StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched = 3,
        CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance = 4
    }

    public enum StageSegmentReturnOwnerKind
    {
        None = 0,
        P1AStageRunRouteOwner = 1
    }

    public enum StageSegmentSuccessorKind
    {
        None = 0,
        NextOrderedSegment = 1
    }

    public enum StageSegmentDestinationSceneKind
    {
        None = 0,
        SuccessorStageDefinitionScene = 1
    }

    public enum StageSegmentTransitionTokenKind
    {
        None = 0,
        SealedCurrentRunSegmentHandoff = 1
    }

    public enum StageSegmentLoaderGenerationKind
    {
        None = 0,
        ActiveRunRouteLoaderGeneration = 1
    }

    public enum StageSegmentNavigationAuthorityKind
    {
        None = 0,
        P1AStageRunRouteOwner = 1
    }

    public enum StageReturnOwnerReceiptPolicy
    {
        None = 0,
        ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented = 1
    }

    public enum StageRouteActionKind
    {
        Retry = 1,
        Replay = 2,
        UIRoute = 3
    }

    [Flags]
    public enum StageRouteOutcome
    {
        None = 0,
        Clear = 1,
        Fail = 2
    }

    public enum StageUiRouteId
    {
        None = 0,
        Lobby = 20
    }

    public enum StageTerminalWindowKind
    {
        SameTerminalResolutionEpoch = 1
    }

    public enum StageTerminalBatchOwnerKind
    {
        EncounterTerminalResolutionCoordinator = 1
    }

    public enum StageTerminalRootAdmissionKind
    {
        CanonicalCombatRootAdmission = 1
    }

    public enum StageTerminalRootOrderKind
    {
        RootAdmissionSequence = 1
    }

    public enum StageTerminalRootIssuePoint
    {
        BeforeTerminalStateMutationAndCallbacks = 1
    }

    public enum StageTerminalBatchBoundaryKind
    {
        RootResolutionToken = 1
    }

    [Flags]
    public enum StageTerminalSubjectRole
    {
        None = 0,
        Player = 1,
        Boss = 2
    }

    public enum StageTerminalCoveragePolicy
    {
        ExclusiveQueuedTerminalStateMutationForBoundSubjects = 1
    }

    public enum StageTerminalWorkExecutionKind
    {
        SynchronousNonYieldingResolution = 1
    }

    public enum StageTerminalNestedRequestPolicy
    {
        SameRootSameEpoch = 1
    }

    public enum StageTerminalIndependentRequestPolicy
    {
        LowerAdmissionSequenceThenNextEpoch = 1
    }

    public enum StageTerminalEpochStampKind
    {
        EncounterTerminalEpoch = 1
    }

    public enum StageTerminalCoordinatorLifecycleKind
    {
        IdleOpenDrainingFinalizingEpochClosedTerminalClosedFaultedCancelled = 1
    }

    public enum StageTerminalSubjectFinalizationKind
    {
        SynchronousTwoSubjectSnapshot = 1
    }

    public enum StageTerminalTokenStatePolicy
    {
        ExplicitIdleActiveDeferredClosedWrongRunPostTerminal = 1
    }

    public enum StageTerminalFlushBarrierKind
    {
        QueueDrainedAndSubjectsFinalized = 1
    }

    public enum StageTerminalSimultaneousOutcome
    {
        Clear = 1
    }

    [Serializable]
    public sealed class StagePresentationHandoffRef
    {
        [SerializeField] private bool enabled;
        [SerializeField] private StageDefinitionProfile stageDefinition;
        [SerializeField] private string handoffId;
        [SerializeField] private CinematicSequenceProfile cinematicProfile;
        [SerializeField] private string expectedPortId;
        [SerializeField] private PlayableAsset expectedPlayableAsset;
        [SerializeField] private string triggerConditionId;
        [SerializeField] private string completionConditionId;

        public bool IsPresent => enabled;
        public StageDefinitionProfile StageDefinition => stageDefinition;
        public string HandoffId => handoffId;
        public CinematicSequenceProfile CinematicProfile => cinematicProfile;
        public string ExpectedPortId => expectedPortId;
        public PlayableAsset ExpectedPlayableAsset => expectedPlayableAsset;
        public string TriggerConditionId => triggerConditionId;
        public string CompletionConditionId => completionConditionId;
    }

    [Serializable]
    public sealed class StageSceneSegmentRef
    {
        [SerializeField] private string segmentId;
        [SerializeField, Min(0)] private int sequenceIndex;
        [SerializeField] private StageDefinitionProfile stageDefinition;
        [SerializeField] private string entryConditionId;
        [SerializeField] private StageSegmentConditionKind entryConditionKind;
        [SerializeField] private string exitConditionId;
        [SerializeField] private StageSegmentConditionKind exitConditionKind;
        [SerializeField] private StageSceneHandoffPolicy handoffPolicy;
        [SerializeField] private StageSegmentSuccessorKind successorKind;
        [SerializeField] private StageSegmentDestinationSceneKind destinationSceneKind;
        [SerializeField] private StageSegmentTransitionTokenKind transitionTokenKind;
        [SerializeField] private StageSegmentLoaderGenerationKind loaderGenerationKind;
        [SerializeField] private StageSegmentNavigationAuthorityKind navigationAuthorityKind;
        [SerializeField] private StageSegmentReturnOwnerKind returnOwnerKind;
        [SerializeField] private StageReturnOwnerReceiptPolicy returnOwnerReceiptPolicy;
        [SerializeField] private StagePresentationHandoffRef entryPresentation;
        [SerializeField] private StagePresentationHandoffRef exitPresentation;

        public string SegmentId => segmentId;
        public int SequenceIndex => sequenceIndex;
        public StageDefinitionProfile StageDefinition => stageDefinition;
        public string EntryConditionId => entryConditionId;
        public StageSegmentConditionKind EntryConditionKind => entryConditionKind;
        public string ExitConditionId => exitConditionId;
        public StageSegmentConditionKind ExitConditionKind => exitConditionKind;
        public StageSceneHandoffPolicy HandoffPolicy => handoffPolicy;
        public StageSegmentSuccessorKind SuccessorKind => successorKind;
        public StageSegmentDestinationSceneKind DestinationSceneKind => destinationSceneKind;
        public StageSegmentTransitionTokenKind TransitionTokenKind => transitionTokenKind;
        public StageSegmentLoaderGenerationKind LoaderGenerationKind => loaderGenerationKind;
        public StageSegmentNavigationAuthorityKind NavigationAuthorityKind => navigationAuthorityKind;
        public StageSegmentReturnOwnerKind ReturnOwnerKind => returnOwnerKind;
        public StageReturnOwnerReceiptPolicy ReturnOwnerReceiptPolicy => returnOwnerReceiptPolicy;
        public StagePresentationHandoffRef EntryPresentation => entryPresentation;
        public StagePresentationHandoffRef ExitPresentation => exitPresentation;
    }

    [Serializable]
    public sealed class StageRouteActionRef
    {
        [SerializeField] private string actionId;
        [SerializeField] private StageRouteActionKind actionKind;
        [SerializeField] private string targetPlayableStageId;
        [SerializeField] private StageUiRouteId targetUiRouteId;
        [SerializeField] private StageRouteOutcome allowedOutcomes;

        public string ActionId => actionId;
        public StageRouteActionKind ActionKind => actionKind;
        public string TargetPlayableStageId => targetPlayableStageId;
        public StageUiRouteId TargetUiRouteId => targetUiRouteId;
        public StageRouteOutcome AllowedOutcomes => allowedOutcomes;

        public bool Allows(StageRouteOutcome outcome)
        {
            return outcome != StageRouteOutcome.None
                && (allowedOutcomes & outcome) == outcome;
        }
    }

    [Serializable]
    public sealed class StageTerminalResolutionPolicy
    {
        [SerializeField] private string terminalResolutionPolicyId;
        [SerializeField, Min(1)] private int semanticRevision = 1;
        [SerializeField] private string terminalResolutionPolicyDigest;
        [SerializeField] private StageTerminalWindowKind windowKind;
        [SerializeField] private StageTerminalBatchOwnerKind batchOwnerKind;
        [SerializeField] private StageTerminalRootAdmissionKind rootAdmissionKind;
        [SerializeField] private StageTerminalRootOrderKind rootOrderKind;
        [SerializeField] private StageTerminalRootIssuePoint rootIssuePoint;
        [SerializeField] private StageTerminalBatchBoundaryKind batchBoundaryKind;
        [SerializeField] private StageTerminalSubjectRole terminalSubjectRoles;
        [SerializeField] private StageTerminalCoveragePolicy coveragePolicy;
        [SerializeField] private StageTerminalWorkExecutionKind workExecutionKind;
        [SerializeField] private StageTerminalNestedRequestPolicy nestedRequestPolicy;
        [SerializeField] private StageTerminalIndependentRequestPolicy independentRequestPolicy;
        [SerializeField] private StageTerminalEpochStampKind epochStampKind;
        [SerializeField] private StageTerminalCoordinatorLifecycleKind coordinatorLifecycleKind;
        [SerializeField] private StageTerminalSubjectFinalizationKind subjectFinalizationKind;
        [SerializeField] private StageTerminalTokenStatePolicy tokenStatePolicy;
        [SerializeField] private StageTerminalFlushBarrierKind flushBarrier;
        [SerializeField] private StageTerminalSimultaneousOutcome simultaneousOutcome;
        [SerializeField] private bool requiresBossCandidateAndFinalDead;
        [SerializeField] private bool requiresPlayerCandidateAndFinalDown;

        public string TerminalResolutionPolicyId => terminalResolutionPolicyId;
        public int SemanticRevision => semanticRevision;
        public string TerminalResolutionPolicyDigest => terminalResolutionPolicyDigest;
        public StageTerminalWindowKind WindowKind => windowKind;
        public StageTerminalBatchOwnerKind BatchOwnerKind => batchOwnerKind;
        public StageTerminalRootAdmissionKind RootAdmissionKind => rootAdmissionKind;
        public StageTerminalRootOrderKind RootOrderKind => rootOrderKind;
        public StageTerminalRootIssuePoint RootIssuePoint => rootIssuePoint;
        public StageTerminalBatchBoundaryKind BatchBoundaryKind => batchBoundaryKind;
        public StageTerminalSubjectRole TerminalSubjectRoles => terminalSubjectRoles;
        public StageTerminalCoveragePolicy CoveragePolicy => coveragePolicy;
        public StageTerminalWorkExecutionKind WorkExecutionKind => workExecutionKind;
        public StageTerminalNestedRequestPolicy NestedRequestPolicy => nestedRequestPolicy;
        public StageTerminalIndependentRequestPolicy IndependentRequestPolicy => independentRequestPolicy;
        public StageTerminalEpochStampKind EpochStampKind => epochStampKind;
        public StageTerminalCoordinatorLifecycleKind CoordinatorLifecycleKind => coordinatorLifecycleKind;
        public StageTerminalSubjectFinalizationKind SubjectFinalizationKind => subjectFinalizationKind;
        public StageTerminalTokenStatePolicy TokenStatePolicy => tokenStatePolicy;
        public StageTerminalFlushBarrierKind FlushBarrier => flushBarrier;
        public StageTerminalSimultaneousOutcome SimultaneousOutcome => simultaneousOutcome;
        public bool RequiresBossCandidateAndFinalDead => requiresBossCandidateAndFinalDead;
        public bool RequiresPlayerCandidateAndFinalDown => requiresPlayerCandidateAndFinalDown;

        public string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1024);
            AppendCanonicalFields(builder);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        internal void AppendCanonicalFields(StringBuilder builder)
        {
            StageCanonicalDigest.Append(builder, "policy.id", terminalResolutionPolicyId);
            StageCanonicalDigest.Append(builder, "policy.revision", semanticRevision);
            StageCanonicalDigest.Append(builder, "policy.window", (int)windowKind);
            StageCanonicalDigest.Append(builder, "policy.batchOwner", (int)batchOwnerKind);
            StageCanonicalDigest.Append(builder, "policy.rootAdmission", (int)rootAdmissionKind);
            StageCanonicalDigest.Append(builder, "policy.rootOrder", (int)rootOrderKind);
            StageCanonicalDigest.Append(builder, "policy.rootIssuePoint", (int)rootIssuePoint);
            StageCanonicalDigest.Append(builder, "policy.batchBoundary", (int)batchBoundaryKind);
            StageCanonicalDigest.Append(builder, "policy.subjectRoles", (int)terminalSubjectRoles);
            StageCanonicalDigest.Append(builder, "policy.coverage", (int)coveragePolicy);
            StageCanonicalDigest.Append(builder, "policy.workExecution", (int)workExecutionKind);
            StageCanonicalDigest.Append(builder, "policy.nested", (int)nestedRequestPolicy);
            StageCanonicalDigest.Append(builder, "policy.independent", (int)independentRequestPolicy);
            StageCanonicalDigest.Append(builder, "policy.epochStamp", (int)epochStampKind);
            StageCanonicalDigest.Append(builder, "policy.lifecycle", (int)coordinatorLifecycleKind);
            StageCanonicalDigest.Append(builder, "policy.subjectFinalization", (int)subjectFinalizationKind);
            StageCanonicalDigest.Append(builder, "policy.tokenStates", (int)tokenStatePolicy);
            StageCanonicalDigest.Append(builder, "policy.flushBarrier", (int)flushBarrier);
            StageCanonicalDigest.Append(builder, "policy.simultaneousOutcome", (int)simultaneousOutcome);
            StageCanonicalDigest.Append(
                builder,
                "policy.requiresBossCandidateAndFinalDead",
                requiresBossCandidateAndFinalDead);
            StageCanonicalDigest.Append(
                builder,
                "policy.requiresPlayerCandidateAndFinalDown",
                requiresPlayerCandidateAndFinalDown);
        }
    }

    [CreateAssetMenu(
        menuName = "DimensionBrawl/Profiles/Playable Stage Definition",
        fileName = "DB_PlayableStage")]
    public sealed class PlayableStageDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private string playableStageId;
        [SerializeField, Min(1)] private int routeRevision = 1;
        [SerializeField] private string canonicalRouteDigest;
        [SerializeField] private StageSceneSegmentRef[] sceneSegments = Array.Empty<StageSceneSegmentRef>();
        [SerializeField] private StageRouteActionRef[] terminalActions = Array.Empty<StageRouteActionRef>();
        [SerializeField] private StageTerminalResolutionPolicy terminalResolutionPolicy;
        [SerializeField] private StageReferenceBlock referenceBlock;
        [SerializeField] private StageResultProgressionJoinBlock resultProgressionJoin = new();

        public int SchemaVersion => schemaVersion;
        public string PlayableStageId => playableStageId;
        public int RouteRevision => routeRevision;
        public string CanonicalRouteDigest => canonicalRouteDigest;
        public int SceneSegmentCount => sceneSegments != null ? sceneSegments.Length : 0;
        public int TerminalActionCount => terminalActions != null ? terminalActions.Length : 0;
        public StageTerminalResolutionPolicy TerminalResolutionPolicy => terminalResolutionPolicy;
        public StageReferenceBlock ReferenceBlock => referenceBlock;
        public StageResultProgressionJoinBlock ResultProgressionJoin => resultProgressionJoin;

        public StageSceneSegmentRef GetSceneSegment(int index)
        {
            if (sceneSegments == null || index < 0 || index >= sceneSegments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return sceneSegments[index];
        }

        public StageRouteActionRef GetTerminalAction(int index)
        {
            if (terminalActions == null || index < 0 || index >= terminalActions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return terminalActions[index];
        }

        public bool TryGetTerminalAction(string actionId, out StageRouteActionRef action)
        {
            if (terminalActions != null && !string.IsNullOrWhiteSpace(actionId))
            {
                for (int i = 0; i < terminalActions.Length; i++)
                {
                    StageRouteActionRef candidate = terminalActions[i];
                    if (candidate != null
                        && string.Equals(candidate.ActionId, actionId, StringComparison.Ordinal))
                    {
                        action = candidate;
                        return true;
                    }
                }
            }

            action = null;
            return false;
        }

        public string ComputeCanonicalReferenceDigest()
        {
            StageReferenceBlock block = referenceBlock;
            LinearStageTemplateProfile template = block?.StageTemplate;
            StageSceneSegmentRef entrySegment = SceneSegmentCount > 0 ? sceneSegments[0] : null;
            StagePresentationHandoffRef storyEntry = entrySegment?.EntryPresentation;
            CinematicSequenceProfile cinematic = storyEntry?.CinematicProfile;

            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(
                builder,
                "reference.present",
                block?.IsPresent == true);
            StageCanonicalDigest.Append(builder, "reference.schemaVersion", block?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(builder, "reference.revision", block?.Revision ?? 0);
            StageCanonicalDigest.Append(builder, "reference.playableStageId", playableStageId);
            StageCanonicalDigest.Append(builder, "reference.routeRevision", routeRevision);
            StageCanonicalDigest.Append(
                builder,
                "reference.canonicalRouteDigest",
                canonicalRouteDigest);
            StageCanonicalDigest.Append(
                builder,
                "reference.templateSchemaVersion",
                template != null ? template.TemplateSchemaVersion : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.templateId",
                template != null ? template.StageTemplateId : string.Empty);
            StageCanonicalDigest.Append(
                builder,
                "reference.templateRevision",
                template != null ? template.TemplateRevision : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.canonicalTemplateDigest",
                template != null ? template.CanonicalTemplateDigest : string.Empty);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryDisposition",
                block != null ? (int)block.StoryEntryDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntrySegmentId",
                entrySegment?.SegmentId);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryHandoffId",
                storyEntry?.HandoffId);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryCinematicSequenceId",
                cinematic != null ? cinematic.SequenceId : string.Empty);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryExpectedPortId",
                storyEntry?.ExpectedPortId);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryStageAnchorId",
                cinematic != null ? cinematic.StageAnchorId : string.Empty);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryStageRuntimeStateId",
                cinematic != null ? cinematic.StageRuntimeStateId : string.Empty);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryTriggerConditionId",
                storyEntry?.TriggerConditionId);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyEntryCompletionConditionId",
                storyEntry?.CompletionConditionId);
            StageCanonicalDigest.Append(
                builder,
                "reference.storyExitDisposition",
                block != null ? (int)block.StoryExitDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.resultDefinitionDisposition",
                block != null ? (int)block.ResultDefinitionDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.progressionNodeDisposition",
                block != null ? (int)block.ProgressionNodeDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.ruleSetDisposition",
                block != null ? (int)block.RuleSetDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.modifierDisposition",
                block != null ? (int)block.ModifierDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.enemyVariantDisposition",
                block != null ? (int)block.EnemyVariantDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.tutorialCourseDisposition",
                block != null ? (int)block.TutorialCourseDisposition : 0);
            StageCanonicalDigest.Append(
                builder,
                "reference.rewardPlanDisposition",
                block != null ? (int)block.RewardPlanDisposition : 0);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        public bool TryCreateBriefingReadModel(
            out StageBriefingReadModel briefing,
            out StageBriefingBuildRejectReason rejectReason)
        {
            return StageBriefingReadModelFactory.TryCreate(this, out briefing, out rejectReason);
        }

        public bool TryComputeCanonicalBriefingDigest(
            out string canonicalBriefingDigest,
            out StageBriefingBuildRejectReason rejectReason)
        {
            return StageBriefingReadModelFactory.TryComputeCanonicalDigest(
                this,
                out canonicalBriefingDigest,
                out rejectReason);
        }

        public bool TryComputeResultProgressionJoinDigest(
            out string canonicalDigest,
            out string error)
        {
            return StageRunResultProgressionJoinSnapshot.TryComputeCanonicalDigest(
                this,
                out canonicalDigest,
                out error);
        }

        public string ComputeCanonicalRouteDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "route.schemaVersion", schemaVersion);
            StageCanonicalDigest.Append(builder, "route.playableStageId", playableStageId);
            StageCanonicalDigest.Append(builder, "route.revision", routeRevision);
            StageCanonicalDigest.Append(builder, "route.segmentCount", SceneSegmentCount);
            for (int i = 0; i < SceneSegmentCount; i++)
            {
                StageSceneSegmentRef segment = sceneSegments[i];
                string prefix = $"route.segment[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", segment?.SegmentId);
                StageCanonicalDigest.Append(builder, prefix + ".sequence", segment?.SequenceIndex ?? -1);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".stageDefinitionId",
                    segment?.StageDefinition != null ? segment.StageDefinition.StageId : string.Empty);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".scenePath",
                    segment?.StageDefinition != null ? segment.StageDefinition.MapScenePath : string.Empty);
                StageCanonicalDigest.Append(builder, prefix + ".entryCondition", segment?.EntryConditionId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".entryConditionKind",
                    segment != null ? (int)segment.EntryConditionKind : 0);
                StageCanonicalDigest.Append(builder, prefix + ".exitCondition", segment?.ExitConditionId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".exitConditionKind",
                    segment != null ? (int)segment.ExitConditionKind : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".handoffPolicy",
                    segment != null ? (int)segment.HandoffPolicy : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".successor",
                    segment != null ? (int)segment.SuccessorKind : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".destinationScene",
                    segment != null ? (int)segment.DestinationSceneKind : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".transitionToken",
                    segment != null ? (int)segment.TransitionTokenKind : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".loaderGeneration",
                    segment != null ? (int)segment.LoaderGenerationKind : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".navigationAuthority",
                    segment != null ? (int)segment.NavigationAuthorityKind : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".returnOwner",
                    segment != null ? (int)segment.ReturnOwnerKind : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".returnReceipt",
                    segment != null ? (int)segment.ReturnOwnerReceiptPolicy : 0);
            }

            StageRouteActionRef[] sortedActions = terminalActions != null
                ? (StageRouteActionRef[])terminalActions.Clone()
                : Array.Empty<StageRouteActionRef>();
            Array.Sort(
                sortedActions,
                (left, right) => string.Compare(
                    left?.ActionId,
                    right?.ActionId,
                    StringComparison.Ordinal));
            StageCanonicalDigest.Append(builder, "route.actionCount", sortedActions.Length);
            for (int i = 0; i < sortedActions.Length; i++)
            {
                StageRouteActionRef action = sortedActions[i];
                string prefix = $"route.action[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", action?.ActionId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".kind",
                    action != null ? (int)action.ActionKind : 0);
                StageCanonicalDigest.Append(builder, prefix + ".playableTarget", action?.TargetPlayableStageId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".uiRouteTarget",
                    action != null ? (int)action.TargetUiRouteId : 0);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".allowedOutcomes",
                    action != null ? (int)action.AllowedOutcomes : 0);
            }

            if (terminalResolutionPolicy == null)
            {
                StageCanonicalDigest.Append(builder, "route.terminalPolicy", string.Empty);
            }
            else
            {
                terminalResolutionPolicy.AppendCanonicalFields(builder);
                StageCanonicalDigest.Append(
                    builder,
                    "route.terminalPolicyDigest",
                    terminalResolutionPolicy.TerminalResolutionPolicyDigest);
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    internal static class StageCanonicalDigest
    {
        public static void Append(StringBuilder builder, string key, string value)
        {
            string safeValue = value ?? string.Empty;
            builder.Append(key);
            builder.Append('=');
            builder.Append(safeValue.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(safeValue);
            builder.Append('\n');
        }

        public static void Append(StringBuilder builder, string key, int value)
        {
            Append(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void Append(StringBuilder builder, string key, long value)
        {
            Append(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void Append(StringBuilder builder, string key, bool value)
        {
            Append(builder, key, value ? "1" : "0");
        }

        public static string Compute(string canonicalPayload)
        {
            byte[] payload = Encoding.UTF8.GetBytes(canonicalPayload ?? string.Empty);
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(payload);
            }

            char[] characters = new char[hash.Length * 2];
            const string alphabet = "0123456789abcdef";
            for (int i = 0; i < hash.Length; i++)
            {
                characters[i * 2] = alphabet[hash[i] >> 4];
                characters[(i * 2) + 1] = alphabet[hash[i] & 0x0f];
            }

            return new string(characters);
        }
    }
}
