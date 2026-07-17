using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public enum StageBriefingValueDisposition
    {
        None = 0,
        Present = 1,
        NoVerifiedSource = 2,
        NotAdmittedByCurrentSchema = 3,
        NotAuthoredForCurrentSchema = 4
    }

    public enum StageReferenceDisposition
    {
        None = 0,
        Present = 1,
        NotAuthoredForCurrentSchema = 2,
        NotAdmittedByCurrentSchema = 3,
        NoVerifiedSource = 4,
        NoFinalSegmentExitPresentationAuthored = 5
    }

    public enum StageTemplatePocketObjectiveKind
    {
        None = 0,
        CompleteTutorialPlan = 1,
        CompleteEntryGuide = 2,
        DefeatBoss = 3
    }

    public enum StageTemplateCurrentExecutionOwnerDisposition
    {
        None = 0,
        ExistingSceneOwner = 1
    }

    public enum StageTemplateP1CAdmissionDisposition
    {
        None = 0,
        NotAdmitted = 1
    }

    public enum StageTemplateSourceDisposition
    {
        None = 0,
        CanonicalSemanticDigest = 1,
        RuntimeStateBoundary = 2,
        RouteConditionBoundary = 3
    }

    [Serializable]
    public sealed class StageTemplatePocketRef
    {
        [SerializeField] private string pocketId;
        [SerializeField, Min(0)] private int sequenceIndex;
        [SerializeField] private StageTemplatePocketObjectiveKind objectiveKind;
        [SerializeField]
        private StageTemplateCurrentExecutionOwnerDisposition currentExecutionOwnerDisposition;
        [SerializeField] private StageTemplateP1CAdmissionDisposition p1cAdmissionDisposition;
        [SerializeField] private StageTemplateSourceDisposition sourceDisposition;
        [SerializeField] private string sourceSemanticId;
        [SerializeField, Min(0)] private int sourceRevision;
        [SerializeField] private string sourceSemanticDigest;
        [SerializeField, Min(0)] private int enemyRoleCount;

        public string PocketId => pocketId;
        public int SequenceIndex => sequenceIndex;
        public StageTemplatePocketObjectiveKind ObjectiveKind => objectiveKind;
        public StageTemplateCurrentExecutionOwnerDisposition CurrentExecutionOwnerDisposition =>
            currentExecutionOwnerDisposition;
        public StageTemplateP1CAdmissionDisposition P1CAdmissionDisposition =>
            p1cAdmissionDisposition;
        public StageTemplateSourceDisposition SourceDisposition => sourceDisposition;
        public string SourceSemanticId => sourceSemanticId;
        public int SourceRevision => sourceRevision;
        public string SourceSemanticDigest => sourceSemanticDigest;
        public int EnemyRoleCount => enemyRoleCount;

        internal void AppendCanonicalFields(System.Text.StringBuilder builder, string prefix)
        {
            StageCanonicalDigest.Append(builder, prefix + ".id", pocketId);
            StageCanonicalDigest.Append(builder, prefix + ".sequenceIndex", sequenceIndex);
            StageCanonicalDigest.Append(builder, prefix + ".objectiveKind", (int)objectiveKind);
            StageCanonicalDigest.Append(
                builder,
                prefix + ".currentExecutionOwnerDisposition",
                (int)currentExecutionOwnerDisposition);
            StageCanonicalDigest.Append(
                builder,
                prefix + ".p1cAdmissionDisposition",
                (int)p1cAdmissionDisposition);
            StageCanonicalDigest.Append(
                builder,
                prefix + ".sourceDisposition",
                (int)sourceDisposition);
            StageCanonicalDigest.Append(builder, prefix + ".sourceSemanticId", sourceSemanticId);
            StageCanonicalDigest.Append(builder, prefix + ".sourceRevision", sourceRevision);
            StageCanonicalDigest.Append(
                builder,
                prefix + ".sourceSemanticDigest",
                sourceSemanticDigest);
            StageCanonicalDigest.Append(builder, prefix + ".enemyRoleCount", enemyRoleCount);
        }
    }

    [Serializable]
    public sealed class StageTemplateRouteSegmentRef
    {
        [SerializeField] private string templateSegmentId;
        [SerializeField] private string routeSegmentId;
        [SerializeField, Min(0)] private int routeSequenceIndex;
        [SerializeField] private StageTemplatePocketRef[] pockets =
            Array.Empty<StageTemplatePocketRef>();

        public string TemplateSegmentId => templateSegmentId;
        public string RouteSegmentId => routeSegmentId;
        public int RouteSequenceIndex => routeSequenceIndex;
        public int PocketCount => pockets != null ? pockets.Length : 0;

        public StageTemplatePocketRef GetPocket(int index)
        {
            if (pockets == null || index < 0 || index >= pockets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return pockets[index];
        }

        internal void AppendCanonicalFields(System.Text.StringBuilder builder, string prefix)
        {
            StageCanonicalDigest.Append(builder, prefix + ".id", templateSegmentId);
            StageCanonicalDigest.Append(builder, prefix + ".routeSegmentId", routeSegmentId);
            StageCanonicalDigest.Append(builder, prefix + ".routeSequenceIndex", routeSequenceIndex);
            StageCanonicalDigest.Append(builder, prefix + ".pocketCount", PocketCount);
            for (int i = 0; i < PocketCount; i++)
            {
                StageTemplatePocketRef pocket = pockets[i];
                string pocketPrefix = prefix + $".pocket[{i}]";
                if (pocket == null)
                {
                    AppendMissingPocket(builder, pocketPrefix);
                }
                else
                {
                    pocket.AppendCanonicalFields(builder, pocketPrefix);
                }
            }
        }

        private static void AppendMissingPocket(
            System.Text.StringBuilder builder,
            string prefix)
        {
            StageCanonicalDigest.Append(builder, prefix + ".id", string.Empty);
            StageCanonicalDigest.Append(builder, prefix + ".sequenceIndex", -1);
            StageCanonicalDigest.Append(builder, prefix + ".objectiveKind", 0);
            StageCanonicalDigest.Append(
                builder,
                prefix + ".currentExecutionOwnerDisposition",
                0);
            StageCanonicalDigest.Append(builder, prefix + ".p1cAdmissionDisposition", 0);
            StageCanonicalDigest.Append(builder, prefix + ".sourceDisposition", 0);
            StageCanonicalDigest.Append(builder, prefix + ".sourceSemanticId", string.Empty);
            StageCanonicalDigest.Append(builder, prefix + ".sourceRevision", 0);
            StageCanonicalDigest.Append(
                builder,
                prefix + ".sourceSemanticDigest",
                string.Empty);
            StageCanonicalDigest.Append(builder, prefix + ".enemyRoleCount", 0);
        }
    }

    [Serializable]
    public sealed class StageReferenceBlock
    {
        [SerializeField] private bool enabled;
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField, Min(1)] private int revision = 1;
        [SerializeField] private string canonicalReferenceDigest;
        [SerializeField] private LinearStageTemplateProfile stageTemplate;
        [SerializeField, Min(1)] private int briefingSchemaVersion = 1;
        [SerializeField, Min(1)] private int briefingRevision = 1;
        [SerializeField] private string canonicalBriefingDigest;
        [SerializeField] private StageReferenceDisposition storyEntryDisposition;
        [SerializeField] private StageReferenceDisposition storyExitDisposition;
        [SerializeField] private StageReferenceDisposition resultDefinitionDisposition;
        [SerializeField] private StageReferenceDisposition progressionNodeDisposition;
        [SerializeField] private StageReferenceDisposition ruleSetDisposition;
        [SerializeField] private StageReferenceDisposition modifierDisposition;
        [SerializeField] private StageReferenceDisposition enemyVariantDisposition;
        [SerializeField] private StageReferenceDisposition tutorialCourseDisposition;
        [SerializeField] private StageReferenceDisposition rewardPlanDisposition;
        [SerializeField]
        private StageBriefingValueDisposition activeRunRestartPolicyDisposition;
        [SerializeField] private string activeRunRestartPolicyDigest;

        public bool IsPresent => enabled;
        public int SchemaVersion => schemaVersion;
        public int Revision => revision;
        public string CanonicalReferenceDigest => canonicalReferenceDigest;
        public LinearStageTemplateProfile StageTemplate => stageTemplate;
        public int BriefingSchemaVersion => briefingSchemaVersion;
        public int BriefingRevision => briefingRevision;
        public string CanonicalBriefingDigest => canonicalBriefingDigest;
        public StageReferenceDisposition StoryEntryDisposition => storyEntryDisposition;
        public StageReferenceDisposition StoryExitDisposition => storyExitDisposition;
        public StageReferenceDisposition ResultDefinitionDisposition => resultDefinitionDisposition;
        public StageReferenceDisposition ProgressionNodeDisposition => progressionNodeDisposition;
        public StageReferenceDisposition RuleSetDisposition => ruleSetDisposition;
        public StageReferenceDisposition ModifierDisposition => modifierDisposition;
        public StageReferenceDisposition EnemyVariantDisposition => enemyVariantDisposition;
        public StageReferenceDisposition TutorialCourseDisposition => tutorialCourseDisposition;
        public StageReferenceDisposition RewardPlanDisposition => rewardPlanDisposition;
        public StageBriefingValueDisposition ActiveRunRestartPolicyDisposition =>
            activeRunRestartPolicyDisposition;
        public string ActiveRunRestartPolicyDigest => activeRunRestartPolicyDigest;
    }
}
