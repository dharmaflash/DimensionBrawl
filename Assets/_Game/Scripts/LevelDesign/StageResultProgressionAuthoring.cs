using System;
using DimensionBrawl.UI.StageClear;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public enum StageResultProgressionReferenceDisposition
    {
        None = 0,
        Present = 1,
        NotAuthoredForCurrentSchema = 2,
        NotAdmittedByCurrentSchema = 3,
        NoVerifiedSource = 4
    }

    public enum StageResultActionPresentationRole
    {
        None = 0,
        Primary = 1,
        Secondary = 2
    }

    public enum StageResultPresentationSourceKind
    {
        None = 0,
        DeepCopiedProfileLocalizationAndMappingsAtAdmission = 1
    }

    public enum StageResultLocaleResolutionPolicy
    {
        None = 0,
        ExactThenLanguageThenDefaultOrdinalIgnoreCase = 1
    }

    public enum StageProgressionCyclePolicy
    {
        None = 0,
        DisallowCyclesWithinEachRelation = 1
    }

    public enum StageProgressionRequirementKind
    {
        None = 0,
        Cleared = 1,
        MasteryObjectiveAchieved = 2
    }

    public enum StageResultJoinSemanticCoupling
    {
        None = 0,
        PresentationAuditSidecarOutsideP1ASemanticResult = 1
    }

    [Serializable]
    public sealed class StageResultActionPresentationMapping
    {
        [SerializeField] private StageRouteOutcome outcome;
        [SerializeField] private string actionId;
        [SerializeField] private string labelKey;
        [SerializeField] private StageResultActionPresentationRole role;
        [SerializeField, Min(0)] private int displayOrder;

        public StageRouteOutcome Outcome => outcome;
        public string ActionId => actionId;
        public string LabelKey => labelKey;
        public StageResultActionPresentationRole Role => role;
        public int DisplayOrder => displayOrder;
    }

    [Serializable]
    public sealed class StageProgressionPrerequisiteRef
    {
        [SerializeField] private StageProgressionNode targetProgressionNode;
        [SerializeField, Min(1)] private int targetProgressionNodeRevision = 1;
        [SerializeField] private StageProgressionRequirementKind requirementKind;
        [SerializeField] private string requiredObjectiveId;

        public StageProgressionNode TargetProgressionNode => targetProgressionNode;
        public int TargetProgressionNodeRevision => targetProgressionNodeRevision;
        public StageProgressionRequirementKind RequirementKind => requirementKind;
        public string RequiredObjectiveId => requiredObjectiveId;
    }

    [Serializable]
    public sealed class StageProgressionRecommendedNextRef
    {
        [SerializeField] private StageProgressionNode targetProgressionNode;
        [SerializeField, Min(1)] private int targetProgressionNodeRevision = 1;

        public StageProgressionNode TargetProgressionNode => targetProgressionNode;
        public int TargetProgressionNodeRevision => targetProgressionNodeRevision;
    }

    [Serializable]
    public sealed class StageResultProgressionJoinBlock
    {
        [SerializeField] private bool present;
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField, Min(1)] private int revision = 1;
        [SerializeField] private StageResultJoinSemanticCoupling semanticCoupling =
            StageResultJoinSemanticCoupling.PresentationAuditSidecarOutsideP1ASemanticResult;
        [SerializeField] private StageResultProgressionReferenceDisposition
            resultDefinitionDisposition = StageResultProgressionReferenceDisposition.Present;
        [SerializeField] private StageResultDefinition resultDefinition;
        [SerializeField] private StageResultPresentationCatalog canonicalPresentationCatalog;
        [SerializeField] private StageResultProgressionReferenceDisposition
            progressionNodeDisposition = StageResultProgressionReferenceDisposition.Present;
        [SerializeField] private StageProgressionNode progressionNode;
        [SerializeField] private StageResultProgressionReferenceDisposition
            progressionGraphDisposition = StageResultProgressionReferenceDisposition.Present;
        [SerializeField] private StageProgressionGraph progressionGraph;
        [SerializeField] private StageResultProgressionReferenceDisposition rewardPlanDisposition =
            StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema;
        [SerializeField] private string rewardPlanDigest;
        [SerializeField] private string canonicalDigest;

        public bool Present => present;
        public int SchemaVersion => schemaVersion;
        public int Revision => revision;
        public StageResultJoinSemanticCoupling SemanticCoupling => semanticCoupling;
        public StageResultProgressionReferenceDisposition ResultDefinitionDisposition =>
            resultDefinitionDisposition;
        public StageResultDefinition ResultDefinition => resultDefinition;
        public StageResultPresentationCatalog CanonicalPresentationCatalog =>
            canonicalPresentationCatalog;
        public StageResultProgressionReferenceDisposition ProgressionNodeDisposition =>
            progressionNodeDisposition;
        public StageProgressionNode ProgressionNode => progressionNode;
        public StageResultProgressionReferenceDisposition ProgressionGraphDisposition =>
            progressionGraphDisposition;
        public StageProgressionGraph ProgressionGraph => progressionGraph;
        public StageResultProgressionReferenceDisposition RewardPlanDisposition =>
            rewardPlanDisposition;
        public string RewardPlanDigest => rewardPlanDigest;
        public string CanonicalDigest => canonicalDigest;
    }
}
