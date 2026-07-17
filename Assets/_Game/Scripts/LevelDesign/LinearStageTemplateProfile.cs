using System;
using System.Text;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Linear Stage Template Profile", fileName = "DB_LinearStageTemplate")]
    public sealed class LinearStageTemplateProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string stageTemplateId = "S1-1.BreakGate";
        [SerializeField] private string displayName = "S1-1 Break Gate";
        [SerializeField] private LinearStageTemplateKind templateKind = LinearStageTemplateKind.TutorialRun;

        [Header("Canonical Current-Route Contract")]
        [SerializeField, Min(1)] private int templateSchemaVersion;
        [SerializeField, Min(1)] private int templateRevision;
        [SerializeField] private string canonicalTemplateDigest;
        [SerializeField] private StageBriefingValueDisposition titleDisposition;
        [SerializeField] private string title;
        [SerializeField]
        private StageBriefingValueDisposition titleLocalizationKeyDisposition;
        [SerializeField] private string titleLocalizationKey;
        [SerializeField] private StageBriefingValueDisposition objectiveDisposition;
        [SerializeField, TextArea] private string objective;
        [SerializeField] private StageBriefingValueDisposition combatLessonDisposition;
        [SerializeField] private StageBriefingValueDisposition recommendedPowerDisposition;
        [SerializeField]
        private StageBriefingValueDisposition recommendedLoadoutDisposition;
        [SerializeField] private string recommendedLoadout;
        [SerializeField]
        private StageBriefingValueDisposition targetRunDurationDisposition;
        [SerializeField, Min(0)] private int targetRunDurationMilliseconds;
        [SerializeField] private StageBriefingValueDisposition featuredThreatDisposition;
        [SerializeField] private string featuredThreat;
        [SerializeField]
        private StageBriefingValueDisposition featuredSummonNeedDisposition;
        [SerializeField] private StageBriefingValueDisposition restrictionsDisposition;
        [SerializeField, Min(0)] private int restrictionCount;
        [SerializeField] private StageBriefingValueDisposition masteryPreviewDisposition;
        [SerializeField] private string masteryPreview;
        [SerializeField] private StageBriefingValueDisposition enemyPreviewDisposition;
        [SerializeField, Min(0)] private int enemyPreviewCount;
        [SerializeField] private StageBriefingValueDisposition rewardPreviewDisposition;
        [SerializeField] private string rewardPreview;
        [SerializeField] private StageBriefingValueDisposition courseSummaryDisposition;
        [SerializeField] private string courseSummary;
        [SerializeField] private StageTemplateRouteSegmentRef[] canonicalRouteSegments =
            Array.Empty<StageTemplateRouteSegmentRef>();

        [Header("Stage Card Handoff")]
        [SerializeField, Min(0)] private int recommendedPowerTier;
        [SerializeField, Min(0f)] private float targetRunDurationSeconds = 180f;
        [SerializeField] private StageSummonNeed featuredSummonNeed = StageSummonNeed.Break;
        [SerializeField] private string combatLesson = "Break opens structure relief.";
        [SerializeField] private string masteryObjective = "Clear and solve the featured pressure lesson.";
        [SerializeField] private string rewardHook = "First-clear reward should teach one growth path.";

        [Header("Linear Route")]
        [SerializeField] private LinearStageSegmentProfile[] segments = Array.Empty<LinearStageSegmentProfile>();

        [Header("Boundaries")]
        [SerializeField] private string excludedScope = "No reward payout, runtime wave spawning, or summon implementation in this template.";

        public string StageTemplateId => stageTemplateId;
        public string DisplayName => displayName;
        public LinearStageTemplateKind TemplateKind => templateKind;
        public int TemplateSchemaVersion => templateSchemaVersion;
        public int TemplateRevision => templateRevision;
        public string CanonicalTemplateDigest => canonicalTemplateDigest;
        public StageBriefingValueDisposition TitleDisposition => titleDisposition;
        public string Title => title;
        public StageBriefingValueDisposition TitleLocalizationKeyDisposition =>
            titleLocalizationKeyDisposition;
        public string TitleLocalizationKey => titleLocalizationKey;
        public StageBriefingValueDisposition ObjectiveDisposition => objectiveDisposition;
        public string Objective => objective;
        public StageBriefingValueDisposition CombatLessonDisposition => combatLessonDisposition;
        public StageBriefingValueDisposition RecommendedPowerDisposition =>
            recommendedPowerDisposition;
        public StageBriefingValueDisposition RecommendedLoadoutDisposition =>
            recommendedLoadoutDisposition;
        public string RecommendedLoadout => recommendedLoadout;
        public StageBriefingValueDisposition TargetRunDurationDisposition =>
            targetRunDurationDisposition;
        public int TargetRunDurationMilliseconds => targetRunDurationMilliseconds;
        public StageBriefingValueDisposition FeaturedThreatDisposition =>
            featuredThreatDisposition;
        public string FeaturedThreat => featuredThreat;
        public StageBriefingValueDisposition FeaturedSummonNeedDisposition =>
            featuredSummonNeedDisposition;
        public StageBriefingValueDisposition RestrictionsDisposition => restrictionsDisposition;
        public int RestrictionCount => restrictionCount;
        public StageBriefingValueDisposition MasteryPreviewDisposition =>
            masteryPreviewDisposition;
        public string MasteryPreview => masteryPreview;
        public StageBriefingValueDisposition EnemyPreviewDisposition => enemyPreviewDisposition;
        public int EnemyPreviewCount => enemyPreviewCount;
        public StageBriefingValueDisposition RewardPreviewDisposition => rewardPreviewDisposition;
        public string RewardPreview => rewardPreview;
        public StageBriefingValueDisposition CourseSummaryDisposition => courseSummaryDisposition;
        public string CourseSummary => courseSummary;
        public int CanonicalRouteSegmentCount =>
            canonicalRouteSegments != null ? canonicalRouteSegments.Length : 0;
        public int RecommendedPowerTier => recommendedPowerTier;
        public float TargetRunDurationSeconds => targetRunDurationSeconds;
        public StageSummonNeed FeaturedSummonNeed => featuredSummonNeed;
        public string CombatLesson => combatLesson;
        public string MasteryObjective => masteryObjective;
        public string RewardHook => rewardHook;
        public string ExcludedScope => excludedScope;
        public int SegmentCount => segments != null ? segments.Length : 0;

        public LinearStageSegmentProfile GetSegment(int index)
        {
            if (segments == null || index < 0 || index >= segments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return segments[index];
        }

        public StageTemplateRouteSegmentRef GetCanonicalRouteSegment(int index)
        {
            if (canonicalRouteSegments == null
                || index < 0
                || index >= canonicalRouteSegments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return canonicalRouteSegments[index];
        }

        public string ComputeCanonicalTemplateDigest()
        {
            StringBuilder builder = new(4096);
            StageCanonicalDigest.Append(builder, "template.schemaVersion", templateSchemaVersion);
            StageCanonicalDigest.Append(builder, "template.id", stageTemplateId);
            StageCanonicalDigest.Append(builder, "template.revision", templateRevision);
            StageCanonicalDigest.Append(builder, "template.kind", (int)templateKind);
            StageCanonicalDigest.Append(
                builder,
                "template.titleDisposition",
                (int)titleDisposition);
            StageCanonicalDigest.Append(builder, "template.title", title);
            StageCanonicalDigest.Append(
                builder,
                "template.titleLocalizationKeyDisposition",
                (int)titleLocalizationKeyDisposition);
            StageCanonicalDigest.Append(
                builder,
                "template.titleLocalizationKey",
                titleLocalizationKey);
            StageCanonicalDigest.Append(
                builder,
                "template.objectiveDisposition",
                (int)objectiveDisposition);
            StageCanonicalDigest.Append(builder, "template.objective", objective);
            StageCanonicalDigest.Append(
                builder,
                "template.combatLessonDisposition",
                (int)combatLessonDisposition);
            StageCanonicalDigest.Append(builder, "template.combatLesson", combatLesson);
            StageCanonicalDigest.Append(
                builder,
                "template.recommendedPowerDisposition",
                (int)recommendedPowerDisposition);
            StageCanonicalDigest.Append(builder, "template.recommendedPower", recommendedPowerTier);
            StageCanonicalDigest.Append(
                builder,
                "template.recommendedLoadoutDisposition",
                (int)recommendedLoadoutDisposition);
            StageCanonicalDigest.Append(
                builder,
                "template.recommendedLoadout",
                recommendedLoadout);
            StageCanonicalDigest.Append(
                builder,
                "template.targetRunDurationDisposition",
                (int)targetRunDurationDisposition);
            StageCanonicalDigest.Append(
                builder,
                "template.targetRunDurationMilliseconds",
                targetRunDurationMilliseconds);
            StageCanonicalDigest.Append(
                builder,
                "template.featuredThreatDisposition",
                (int)featuredThreatDisposition);
            StageCanonicalDigest.Append(builder, "template.featuredThreat", featuredThreat);
            StageCanonicalDigest.Append(
                builder,
                "template.featuredSummonNeedDisposition",
                (int)featuredSummonNeedDisposition);
            StageCanonicalDigest.Append(
                builder,
                "template.featuredSummonNeed",
                (int)featuredSummonNeed);
            StageCanonicalDigest.Append(
                builder,
                "template.restrictionsDisposition",
                (int)restrictionsDisposition);
            StageCanonicalDigest.Append(builder, "template.restrictionCount", restrictionCount);
            StageCanonicalDigest.Append(
                builder,
                "template.masteryPreviewDisposition",
                (int)masteryPreviewDisposition);
            StageCanonicalDigest.Append(builder, "template.masteryPreview", masteryPreview);
            StageCanonicalDigest.Append(
                builder,
                "template.enemyPreviewDisposition",
                (int)enemyPreviewDisposition);
            StageCanonicalDigest.Append(builder, "template.enemyPreviewCount", enemyPreviewCount);
            StageCanonicalDigest.Append(
                builder,
                "template.rewardPreviewDisposition",
                (int)rewardPreviewDisposition);
            StageCanonicalDigest.Append(builder, "template.rewardPreview", rewardPreview);
            StageCanonicalDigest.Append(
                builder,
                "template.courseSummaryDisposition",
                (int)courseSummaryDisposition);
            StageCanonicalDigest.Append(builder, "template.courseSummary", courseSummary);
            StageCanonicalDigest.Append(
                builder,
                "template.segmentCount",
                CanonicalRouteSegmentCount);
            for (int i = 0; i < CanonicalRouteSegmentCount; i++)
            {
                StageTemplateRouteSegmentRef segment = canonicalRouteSegments[i];
                string prefix = $"template.segment[{i}]";
                if (segment == null)
                {
                    StageCanonicalDigest.Append(builder, prefix + ".id", string.Empty);
                    StageCanonicalDigest.Append(
                        builder,
                        prefix + ".routeSegmentId",
                        string.Empty);
                    StageCanonicalDigest.Append(builder, prefix + ".routeSequenceIndex", -1);
                    StageCanonicalDigest.Append(builder, prefix + ".pocketCount", 0);
                }
                else
                {
                    segment.AppendCanonicalFields(builder, prefix);
                }
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }
}
