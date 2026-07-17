using System;
using System.Collections.Generic;
using System.Text;

namespace DimensionBrawl.LevelDesign
{
    public enum StageBriefingBuildRejectReason
    {
        None = 0,
        MissingReferenceBlock = 1,
        UnsupportedReferenceSchema = 2,
        InvalidReferenceRevision = 3,
        MissingTemplate = 4,
        UnsupportedTemplateSchema = 5,
        InvalidTemplateRevision = 6,
        MissingTemplateDigest = 7,
        TemplateDigestMismatch = 8,
        InvalidTemplateValues = 9,
        RouteTemplateMismatch = 10,
        InvalidPocketContract = 11,
        MissingReferenceDigest = 12,
        ReferenceDigestMismatch = 13,
        InvalidReferenceDisposition = 14,
        InvalidStoryContract = 15,
        UnsupportedBriefingSchema = 16,
        InvalidBriefingRevision = 17,
        MissingBriefingDigest = 18,
        InvalidActiveRunRestartPolicy = 19,
        InvalidActionContract = 20,
        BriefingDigestMismatch = 21
    }

    public sealed class StageBriefingSegmentReadModel
    {
        private readonly string[] pocketIds;

        internal StageBriefingSegmentReadModel(
            string templateSegmentId,
            string routeSegmentId,
            int routeSequenceIndex,
            string[] pocketIds)
        {
            TemplateSegmentId = templateSegmentId ?? string.Empty;
            RouteSegmentId = routeSegmentId ?? string.Empty;
            RouteSequenceIndex = routeSequenceIndex;
            this.pocketIds = pocketIds != null
                ? (string[])pocketIds.Clone()
                : Array.Empty<string>();
        }

        public string TemplateSegmentId { get; }
        public string RouteSegmentId { get; }
        public int RouteSequenceIndex { get; }
        public int PocketCount => pocketIds.Length;

        public string GetPocketId(int index)
        {
            if (index < 0 || index >= pocketIds.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return pocketIds[index];
        }
    }

    public sealed class StageBriefingActionReadModel
    {
        internal StageBriefingActionReadModel(
            string actionId,
            StageRouteActionKind actionKind,
            string targetPlayableStageId,
            StageUiRouteId targetUiRouteId,
            StageRouteOutcome allowedOutcomes)
        {
            ActionId = actionId ?? string.Empty;
            ActionKind = actionKind;
            TargetPlayableStageId = targetPlayableStageId ?? string.Empty;
            TargetUiRouteId = targetUiRouteId;
            AllowedOutcomes = allowedOutcomes;
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

    public sealed class StageBriefingReadModel
    {
        private readonly StageBriefingSegmentReadModel[] segments;
        private readonly StageBriefingActionReadModel[] actions;

        internal StageBriefingReadModel(
            int schemaVersion,
            int revision,
            string playableStageId,
            int routeRevision,
            string canonicalRouteDigest,
            int referenceSchemaVersion,
            int referenceRevision,
            string canonicalReferenceDigest,
            int templateSchemaVersion,
            string templateId,
            int templateRevision,
            string canonicalTemplateDigest,
            StageBriefingValueDisposition titleDisposition,
            string title,
            StageBriefingValueDisposition titleLocalizationKeyDisposition,
            string titleLocalizationKey,
            StageBriefingValueDisposition objectiveDisposition,
            string objective,
            StageBriefingValueDisposition combatLessonDisposition,
            string combatLesson,
            StageBriefingValueDisposition recommendedPowerDisposition,
            int recommendedPower,
            StageBriefingValueDisposition recommendedLoadoutDisposition,
            string recommendedLoadout,
            StageBriefingValueDisposition targetRunDurationDisposition,
            int targetRunDurationMilliseconds,
            StageBriefingValueDisposition featuredThreatDisposition,
            string featuredThreat,
            StageBriefingValueDisposition featuredSummonNeedDisposition,
            StageSummonNeed featuredSummonNeed,
            StageBriefingValueDisposition restrictionsDisposition,
            int restrictionCount,
            StageBriefingValueDisposition masteryPreviewDisposition,
            string masteryPreview,
            StageBriefingValueDisposition enemyPreviewDisposition,
            int enemyPreviewCount,
            StageBriefingValueDisposition rewardPreviewDisposition,
            string rewardPreview,
            StageBriefingValueDisposition courseSummaryDisposition,
            string courseSummary,
            StageBriefingSegmentReadModel[] segments,
            StageReferenceDisposition storyEntryDisposition,
            string storyEntrySegmentId,
            string storyEntryHandoffId,
            string storyEntryCinematicSequenceId,
            string storyEntryExpectedPortId,
            string storyEntryStageAnchorId,
            string storyEntryStageRuntimeStateId,
            string storyEntryTriggerConditionId,
            string storyEntryCompletionConditionId,
            StageReferenceDisposition storyExitDisposition,
            StageBriefingValueDisposition activeRunRestartPolicyDisposition,
            string activeRunRestartPolicyDigest,
            StageBriefingActionReadModel[] actions)
        {
            SchemaVersion = schemaVersion;
            Revision = revision;
            PlayableStageId = playableStageId ?? string.Empty;
            RouteRevision = routeRevision;
            CanonicalRouteDigest = canonicalRouteDigest ?? string.Empty;
            ReferenceSchemaVersion = referenceSchemaVersion;
            ReferenceRevision = referenceRevision;
            CanonicalReferenceDigest = canonicalReferenceDigest ?? string.Empty;
            TemplateSchemaVersion = templateSchemaVersion;
            TemplateId = templateId ?? string.Empty;
            TemplateRevision = templateRevision;
            CanonicalTemplateDigest = canonicalTemplateDigest ?? string.Empty;
            TitleDisposition = titleDisposition;
            Title = title ?? string.Empty;
            TitleLocalizationKeyDisposition = titleLocalizationKeyDisposition;
            TitleLocalizationKey = titleLocalizationKey ?? string.Empty;
            ObjectiveDisposition = objectiveDisposition;
            Objective = objective ?? string.Empty;
            CombatLessonDisposition = combatLessonDisposition;
            CombatLesson = combatLesson ?? string.Empty;
            RecommendedPowerDisposition = recommendedPowerDisposition;
            RecommendedPower = recommendedPower;
            RecommendedLoadoutDisposition = recommendedLoadoutDisposition;
            RecommendedLoadout = recommendedLoadout ?? string.Empty;
            TargetRunDurationDisposition = targetRunDurationDisposition;
            TargetRunDurationMilliseconds = targetRunDurationMilliseconds;
            FeaturedThreatDisposition = featuredThreatDisposition;
            FeaturedThreat = featuredThreat ?? string.Empty;
            FeaturedSummonNeedDisposition = featuredSummonNeedDisposition;
            FeaturedSummonNeed = featuredSummonNeed;
            RestrictionsDisposition = restrictionsDisposition;
            RestrictionCount = restrictionCount;
            MasteryPreviewDisposition = masteryPreviewDisposition;
            MasteryPreview = masteryPreview ?? string.Empty;
            EnemyPreviewDisposition = enemyPreviewDisposition;
            EnemyPreviewCount = enemyPreviewCount;
            RewardPreviewDisposition = rewardPreviewDisposition;
            RewardPreview = rewardPreview ?? string.Empty;
            CourseSummaryDisposition = courseSummaryDisposition;
            CourseSummary = courseSummary ?? string.Empty;
            this.segments = segments != null
                ? (StageBriefingSegmentReadModel[])segments.Clone()
                : Array.Empty<StageBriefingSegmentReadModel>();
            StoryEntryDisposition = storyEntryDisposition;
            StoryEntrySegmentId = storyEntrySegmentId ?? string.Empty;
            StoryEntryHandoffId = storyEntryHandoffId ?? string.Empty;
            StoryEntryCinematicSequenceId = storyEntryCinematicSequenceId ?? string.Empty;
            StoryEntryExpectedPortId = storyEntryExpectedPortId ?? string.Empty;
            StoryEntryStageAnchorId = storyEntryStageAnchorId ?? string.Empty;
            StoryEntryStageRuntimeStateId = storyEntryStageRuntimeStateId ?? string.Empty;
            StoryEntryTriggerConditionId = storyEntryTriggerConditionId ?? string.Empty;
            StoryEntryCompletionConditionId = storyEntryCompletionConditionId ?? string.Empty;
            StoryExitDisposition = storyExitDisposition;
            ActiveRunRestartPolicyDisposition = activeRunRestartPolicyDisposition;
            ActiveRunRestartPolicyDigest = activeRunRestartPolicyDigest ?? string.Empty;
            this.actions = actions != null
                ? (StageBriefingActionReadModel[])actions.Clone()
                : Array.Empty<StageBriefingActionReadModel>();
            CanonicalBriefingDigest = ComputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public int Revision { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string CanonicalRouteDigest { get; }
        public int ReferenceSchemaVersion { get; }
        public int ReferenceRevision { get; }
        public string CanonicalReferenceDigest { get; }
        public int TemplateSchemaVersion { get; }
        public string TemplateId { get; }
        public int TemplateRevision { get; }
        public string CanonicalTemplateDigest { get; }
        public string CanonicalBriefingDigest { get; }
        public StageBriefingValueDisposition TitleDisposition { get; }
        public string Title { get; }
        public StageBriefingValueDisposition TitleLocalizationKeyDisposition { get; }
        public string TitleLocalizationKey { get; }
        public StageBriefingValueDisposition ObjectiveDisposition { get; }
        public string Objective { get; }
        public StageBriefingValueDisposition CombatLessonDisposition { get; }
        public string CombatLesson { get; }
        public StageBriefingValueDisposition RecommendedPowerDisposition { get; }
        public int RecommendedPower { get; }
        public StageBriefingValueDisposition RecommendedLoadoutDisposition { get; }
        public string RecommendedLoadout { get; }
        public StageBriefingValueDisposition TargetRunDurationDisposition { get; }
        public int TargetRunDurationMilliseconds { get; }
        public StageBriefingValueDisposition FeaturedThreatDisposition { get; }
        public string FeaturedThreat { get; }
        public StageBriefingValueDisposition FeaturedSummonNeedDisposition { get; }
        public StageSummonNeed FeaturedSummonNeed { get; }
        public StageBriefingValueDisposition RestrictionsDisposition { get; }
        public int RestrictionCount { get; }
        public StageBriefingValueDisposition MasteryPreviewDisposition { get; }
        public string MasteryPreview { get; }
        public StageBriefingValueDisposition EnemyPreviewDisposition { get; }
        public int EnemyPreviewCount { get; }
        public StageBriefingValueDisposition RewardPreviewDisposition { get; }
        public string RewardPreview { get; }
        public StageBriefingValueDisposition CourseSummaryDisposition { get; }
        public string CourseSummary { get; }
        public int SegmentCount => segments.Length;
        public StageReferenceDisposition StoryEntryDisposition { get; }
        public string StoryEntrySegmentId { get; }
        public string StoryEntryHandoffId { get; }
        public string StoryEntryCinematicSequenceId { get; }
        public string StoryEntryExpectedPortId { get; }
        public string StoryEntryStageAnchorId { get; }
        public string StoryEntryStageRuntimeStateId { get; }
        public string StoryEntryTriggerConditionId { get; }
        public string StoryEntryCompletionConditionId { get; }
        public StageReferenceDisposition StoryExitDisposition { get; }
        public StageBriefingValueDisposition ActiveRunRestartPolicyDisposition { get; }
        public string ActiveRunRestartPolicyDigest { get; }
        public int ActionCount => actions.Length;

        public StageBriefingSegmentReadModel GetSegment(int index)
        {
            if (index < 0 || index >= segments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return segments[index];
        }

        public StageBriefingActionReadModel GetAction(int index)
        {
            if (index < 0 || index >= actions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return actions[index];
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(6144);
            StageCanonicalDigest.Append(builder, "briefing.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "briefing.revision", Revision);
            StageCanonicalDigest.Append(builder, "briefing.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "briefing.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(
                builder,
                "briefing.canonicalRouteDigest",
                CanonicalRouteDigest);
            StageCanonicalDigest.Append(
                builder,
                "briefing.referenceSchemaVersion",
                ReferenceSchemaVersion);
            StageCanonicalDigest.Append(builder, "briefing.referenceRevision", ReferenceRevision);
            StageCanonicalDigest.Append(
                builder,
                "briefing.canonicalReferenceDigest",
                CanonicalReferenceDigest);
            StageCanonicalDigest.Append(
                builder,
                "briefing.templateSchemaVersion",
                TemplateSchemaVersion);
            StageCanonicalDigest.Append(builder, "briefing.templateId", TemplateId);
            StageCanonicalDigest.Append(builder, "briefing.templateRevision", TemplateRevision);
            StageCanonicalDigest.Append(
                builder,
                "briefing.canonicalTemplateDigest",
                CanonicalTemplateDigest);
            StageCanonicalDigest.Append(
                builder,
                "briefing.titleDisposition",
                (int)TitleDisposition);
            StageCanonicalDigest.Append(builder, "briefing.title", Title);
            StageCanonicalDigest.Append(
                builder,
                "briefing.titleLocalizationKeyDisposition",
                (int)TitleLocalizationKeyDisposition);
            StageCanonicalDigest.Append(
                builder,
                "briefing.titleLocalizationKey",
                TitleLocalizationKey);
            StageCanonicalDigest.Append(
                builder,
                "briefing.objectiveDisposition",
                (int)ObjectiveDisposition);
            StageCanonicalDigest.Append(builder, "briefing.objective", Objective);
            StageCanonicalDigest.Append(
                builder,
                "briefing.combatLessonDisposition",
                (int)CombatLessonDisposition);
            StageCanonicalDigest.Append(builder, "briefing.combatLesson", CombatLesson);
            StageCanonicalDigest.Append(
                builder,
                "briefing.recommendedPowerDisposition",
                (int)RecommendedPowerDisposition);
            StageCanonicalDigest.Append(builder, "briefing.recommendedPower", RecommendedPower);
            StageCanonicalDigest.Append(
                builder,
                "briefing.recommendedLoadoutDisposition",
                (int)RecommendedLoadoutDisposition);
            StageCanonicalDigest.Append(
                builder,
                "briefing.recommendedLoadout",
                RecommendedLoadout);
            StageCanonicalDigest.Append(
                builder,
                "briefing.targetRunDurationDisposition",
                (int)TargetRunDurationDisposition);
            StageCanonicalDigest.Append(
                builder,
                "briefing.targetRunDurationMilliseconds",
                TargetRunDurationMilliseconds);
            StageCanonicalDigest.Append(
                builder,
                "briefing.featuredThreatDisposition",
                (int)FeaturedThreatDisposition);
            StageCanonicalDigest.Append(builder, "briefing.featuredThreat", FeaturedThreat);
            StageCanonicalDigest.Append(
                builder,
                "briefing.featuredSummonNeedDisposition",
                (int)FeaturedSummonNeedDisposition);
            StageCanonicalDigest.Append(
                builder,
                "briefing.featuredSummonNeed",
                (int)FeaturedSummonNeed);
            StageCanonicalDigest.Append(
                builder,
                "briefing.restrictionsDisposition",
                (int)RestrictionsDisposition);
            StageCanonicalDigest.Append(builder, "briefing.restrictionCount", RestrictionCount);
            StageCanonicalDigest.Append(
                builder,
                "briefing.masteryPreviewDisposition",
                (int)MasteryPreviewDisposition);
            StageCanonicalDigest.Append(builder, "briefing.masteryPreview", MasteryPreview);
            StageCanonicalDigest.Append(
                builder,
                "briefing.enemyPreviewDisposition",
                (int)EnemyPreviewDisposition);
            StageCanonicalDigest.Append(builder, "briefing.enemyPreviewCount", EnemyPreviewCount);
            StageCanonicalDigest.Append(
                builder,
                "briefing.rewardPreviewDisposition",
                (int)RewardPreviewDisposition);
            StageCanonicalDigest.Append(builder, "briefing.rewardPreview", RewardPreview);
            StageCanonicalDigest.Append(
                builder,
                "briefing.courseSummaryDisposition",
                (int)CourseSummaryDisposition);
            StageCanonicalDigest.Append(builder, "briefing.courseSummary", CourseSummary);
            StageCanonicalDigest.Append(builder, "briefing.segmentCount", SegmentCount);
            for (int i = 0; i < SegmentCount; i++)
            {
                StageBriefingSegmentReadModel segment = segments[i];
                string prefix = $"briefing.segment[{i}]";
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".templateSegmentId",
                    segment.TemplateSegmentId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".routeSegmentId",
                    segment.RouteSegmentId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".routeSequenceIndex",
                    segment.RouteSequenceIndex);
                StageCanonicalDigest.Append(builder, prefix + ".pocketCount", segment.PocketCount);
                for (int pocketIndex = 0; pocketIndex < segment.PocketCount; pocketIndex++)
                {
                    StageCanonicalDigest.Append(
                        builder,
                        prefix + $".pocket[{pocketIndex}].id",
                        segment.GetPocketId(pocketIndex));
                }
            }

            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryDisposition",
                (int)StoryEntryDisposition);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntrySegmentId",
                StoryEntrySegmentId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryHandoffId",
                StoryEntryHandoffId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryCinematicSequenceId",
                StoryEntryCinematicSequenceId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryExpectedPortId",
                StoryEntryExpectedPortId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryStageAnchorId",
                StoryEntryStageAnchorId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryStageRuntimeStateId",
                StoryEntryStageRuntimeStateId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryTriggerConditionId",
                StoryEntryTriggerConditionId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyEntryCompletionConditionId",
                StoryEntryCompletionConditionId);
            StageCanonicalDigest.Append(
                builder,
                "briefing.storyExitDisposition",
                (int)StoryExitDisposition);
            StageCanonicalDigest.Append(
                builder,
                "briefing.activeRunRestartPolicyDisposition",
                (int)ActiveRunRestartPolicyDisposition);
            StageCanonicalDigest.Append(
                builder,
                "briefing.activeRunRestartPolicyDigest",
                ActiveRunRestartPolicyDigest);
            StageCanonicalDigest.Append(builder, "briefing.actionCount", ActionCount);
            for (int i = 0; i < ActionCount; i++)
            {
                StageBriefingActionReadModel action = actions[i];
                string prefix = $"briefing.action[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", action.ActionId);
                StageCanonicalDigest.Append(builder, prefix + ".kind", (int)action.ActionKind);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".targetPlayableStageId",
                    action.TargetPlayableStageId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".targetUiRouteId",
                    (int)action.TargetUiRouteId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".allowedOutcomes",
                    (int)action.AllowedOutcomes);
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    internal static class StageBriefingReadModelFactory
    {
        public static bool TryCreate(
            PlayableStageDefinition route,
            out StageBriefingReadModel briefing,
            out StageBriefingBuildRejectReason rejectReason)
        {
            return TryCreateCore(
                route,
                true,
                out briefing,
                out rejectReason);
        }

        public static bool TryComputeCanonicalDigest(
            PlayableStageDefinition route,
            out string canonicalBriefingDigest,
            out StageBriefingBuildRejectReason rejectReason)
        {
            canonicalBriefingDigest = string.Empty;
            if (!TryCreateCore(
                    route,
                    false,
                    out StageBriefingReadModel briefing,
                    out rejectReason))
            {
                return false;
            }

            canonicalBriefingDigest = briefing.CanonicalBriefingDigest;
            return true;
        }

        private static bool TryCreateCore(
            PlayableStageDefinition route,
            bool requireStoredBriefingDigest,
            out StageBriefingReadModel briefing,
            out StageBriefingBuildRejectReason rejectReason)
        {
            briefing = null;
            if (route?.ReferenceBlock == null || !route.ReferenceBlock.IsPresent)
            {
                rejectReason = StageBriefingBuildRejectReason.MissingReferenceBlock;
                return false;
            }

            StageReferenceBlock block = route.ReferenceBlock;
            if (block.SchemaVersion != 1)
            {
                rejectReason = StageBriefingBuildRejectReason.UnsupportedReferenceSchema;
                return false;
            }

            if (block.Revision < 1)
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidReferenceRevision;
                return false;
            }

            LinearStageTemplateProfile template = block.StageTemplate;
            if (template == null)
            {
                rejectReason = StageBriefingBuildRejectReason.MissingTemplate;
                return false;
            }

            if (template.TemplateSchemaVersion != 1)
            {
                rejectReason = StageBriefingBuildRejectReason.UnsupportedTemplateSchema;
                return false;
            }

            if (template.TemplateRevision < 1)
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidTemplateRevision;
                return false;
            }

            if (string.IsNullOrWhiteSpace(template.CanonicalTemplateDigest))
            {
                rejectReason = StageBriefingBuildRejectReason.MissingTemplateDigest;
                return false;
            }

            if (!string.Equals(
                    template.CanonicalTemplateDigest,
                    template.ComputeCanonicalTemplateDigest(),
                    StringComparison.Ordinal))
            {
                rejectReason = StageBriefingBuildRejectReason.TemplateDigestMismatch;
                return false;
            }

            if (!ValidateTemplateValues(template))
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidTemplateValues;
                return false;
            }

            if (!TryBuildSegments(route, template, out StageBriefingSegmentReadModel[] segments))
            {
                rejectReason = StageBriefingBuildRejectReason.RouteTemplateMismatch;
                return false;
            }

            if (!ValidatePocketContracts(template))
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidPocketContract;
                return false;
            }

            if (string.IsNullOrWhiteSpace(block.CanonicalReferenceDigest))
            {
                rejectReason = StageBriefingBuildRejectReason.MissingReferenceDigest;
                return false;
            }

            if (!string.Equals(
                    block.CanonicalReferenceDigest,
                    route.ComputeCanonicalReferenceDigest(),
                    StringComparison.Ordinal))
            {
                rejectReason = StageBriefingBuildRejectReason.ReferenceDigestMismatch;
                return false;
            }

            if (!ValidateReferenceDispositions(block))
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidReferenceDisposition;
                return false;
            }

            if (!TryResolveStory(
                    route,
                    block,
                    out string storySegmentId,
                    out string storyHandoffId,
                    out string cinematicSequenceId,
                    out string expectedPortId,
                    out string stageAnchorId,
                    out string stageRuntimeStateId,
                    out string triggerConditionId,
                    out string completionConditionId))
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidStoryContract;
                return false;
            }

            if (block.BriefingSchemaVersion != 1)
            {
                rejectReason = StageBriefingBuildRejectReason.UnsupportedBriefingSchema;
                return false;
            }

            if (block.BriefingRevision < 1)
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidBriefingRevision;
                return false;
            }

            if (requireStoredBriefingDigest
                && string.IsNullOrWhiteSpace(block.CanonicalBriefingDigest))
            {
                rejectReason = StageBriefingBuildRejectReason.MissingBriefingDigest;
                return false;
            }

            if (block.ActiveRunRestartPolicyDisposition
                    != StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                || !string.IsNullOrEmpty(block.ActiveRunRestartPolicyDigest))
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidActiveRunRestartPolicy;
                return false;
            }

            if (!TryBuildActions(route, out StageBriefingActionReadModel[] actions))
            {
                rejectReason = StageBriefingBuildRejectReason.InvalidActionContract;
                return false;
            }

            briefing = new StageBriefingReadModel(
                block.BriefingSchemaVersion,
                block.BriefingRevision,
                route.PlayableStageId,
                route.RouteRevision,
                route.CanonicalRouteDigest,
                block.SchemaVersion,
                block.Revision,
                block.CanonicalReferenceDigest,
                template.TemplateSchemaVersion,
                template.StageTemplateId,
                template.TemplateRevision,
                template.CanonicalTemplateDigest,
                template.TitleDisposition,
                template.Title,
                template.TitleLocalizationKeyDisposition,
                template.TitleLocalizationKey,
                template.ObjectiveDisposition,
                template.Objective,
                template.CombatLessonDisposition,
                template.CombatLesson,
                template.RecommendedPowerDisposition,
                template.RecommendedPowerTier,
                template.RecommendedLoadoutDisposition,
                template.RecommendedLoadout,
                template.TargetRunDurationDisposition,
                template.TargetRunDurationMilliseconds,
                template.FeaturedThreatDisposition,
                template.FeaturedThreat,
                template.FeaturedSummonNeedDisposition,
                template.FeaturedSummonNeed,
                template.RestrictionsDisposition,
                template.RestrictionCount,
                template.MasteryPreviewDisposition,
                template.MasteryPreview,
                template.EnemyPreviewDisposition,
                template.EnemyPreviewCount,
                template.RewardPreviewDisposition,
                template.RewardPreview,
                template.CourseSummaryDisposition,
                template.CourseSummary,
                segments,
                block.StoryEntryDisposition,
                storySegmentId,
                storyHandoffId,
                cinematicSequenceId,
                expectedPortId,
                stageAnchorId,
                stageRuntimeStateId,
                triggerConditionId,
                completionConditionId,
                block.StoryExitDisposition,
                block.ActiveRunRestartPolicyDisposition,
                block.ActiveRunRestartPolicyDigest,
                actions);

            if (requireStoredBriefingDigest
                && !string.Equals(
                    block.CanonicalBriefingDigest,
                    briefing.CanonicalBriefingDigest,
                    StringComparison.Ordinal))
            {
                briefing = null;
                rejectReason = StageBriefingBuildRejectReason.BriefingDigestMismatch;
                return false;
            }

            rejectReason = StageBriefingBuildRejectReason.None;
            return true;
        }

        private static bool ValidateTemplateValues(LinearStageTemplateProfile template)
        {
            return template.TitleDisposition == StageBriefingValueDisposition.Present
                && !string.IsNullOrWhiteSpace(template.Title)
                && template.TitleLocalizationKeyDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.TitleLocalizationKey)
                && template.ObjectiveDisposition == StageBriefingValueDisposition.Present
                && !string.IsNullOrWhiteSpace(template.Objective)
                && template.CombatLessonDisposition == StageBriefingValueDisposition.Present
                && !string.IsNullOrWhiteSpace(template.CombatLesson)
                && template.RecommendedPowerDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && template.RecommendedPowerTier == 0
                && template.RecommendedLoadoutDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.RecommendedLoadout)
                && template.TargetRunDurationDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && template.TargetRunDurationMilliseconds == 0
                && template.FeaturedThreatDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.FeaturedThreat)
                && template.FeaturedSummonNeedDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && template.FeaturedSummonNeed == StageSummonNeed.None
                && template.RestrictionsDisposition
                    == StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                && template.RestrictionCount == 0
                && template.MasteryPreviewDisposition
                    == StageBriefingValueDisposition.NotAuthoredForCurrentSchema
                && string.IsNullOrEmpty(template.MasteryPreview)
                && template.EnemyPreviewDisposition
                    == StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                && template.EnemyPreviewCount == 0
                && template.RewardPreviewDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.RewardPreview)
                && template.CourseSummaryDisposition
                    == StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                && string.IsNullOrEmpty(template.CourseSummary);
        }

        private static bool TryBuildSegments(
            PlayableStageDefinition route,
            LinearStageTemplateProfile template,
            out StageBriefingSegmentReadModel[] segments)
        {
            segments = null;
            if (template.CanonicalRouteSegmentCount != route.SceneSegmentCount
                || route.SceneSegmentCount < 1)
            {
                return false;
            }

            var templateIds = new HashSet<string>(StringComparer.Ordinal);
            var routeIds = new HashSet<string>(StringComparer.Ordinal);
            var pocketIds = new HashSet<string>(StringComparer.Ordinal);
            segments = new StageBriefingSegmentReadModel[route.SceneSegmentCount];
            for (int i = 0; i < route.SceneSegmentCount; i++)
            {
                StageSceneSegmentRef routeSegment = route.GetSceneSegment(i);
                StageTemplateRouteSegmentRef templateSegment =
                    template.GetCanonicalRouteSegment(i);
                if (routeSegment == null
                    || templateSegment == null
                    || string.IsNullOrWhiteSpace(templateSegment.TemplateSegmentId)
                    || !templateIds.Add(templateSegment.TemplateSegmentId)
                    || string.IsNullOrWhiteSpace(templateSegment.RouteSegmentId)
                    || !routeIds.Add(templateSegment.RouteSegmentId)
                    || !string.Equals(
                        templateSegment.RouteSegmentId,
                        routeSegment.SegmentId,
                        StringComparison.Ordinal)
                    || templateSegment.RouteSequenceIndex != routeSegment.SequenceIndex
                    || templateSegment.PocketCount < 1)
                {
                    segments = null;
                    return false;
                }

                string[] segmentPocketIds = new string[templateSegment.PocketCount];
                for (int pocketIndex = 0;
                    pocketIndex < templateSegment.PocketCount;
                    pocketIndex++)
                {
                    StageTemplatePocketRef pocket = templateSegment.GetPocket(pocketIndex);
                    if (pocket == null
                        || string.IsNullOrWhiteSpace(pocket.PocketId)
                        || pocket.SequenceIndex != pocketIndex
                        || !pocketIds.Add(pocket.PocketId))
                    {
                        segments = null;
                        return false;
                    }

                    segmentPocketIds[pocketIndex] = pocket.PocketId;
                }

                segments[i] = new StageBriefingSegmentReadModel(
                    templateSegment.TemplateSegmentId,
                    templateSegment.RouteSegmentId,
                    templateSegment.RouteSequenceIndex,
                    segmentPocketIds);
            }

            return true;
        }

        private static bool ValidatePocketContracts(LinearStageTemplateProfile template)
        {
            for (int segmentIndex = 0;
                segmentIndex < template.CanonicalRouteSegmentCount;
                segmentIndex++)
            {
                StageTemplateRouteSegmentRef segment =
                    template.GetCanonicalRouteSegment(segmentIndex);
                for (int pocketIndex = 0; pocketIndex < segment.PocketCount; pocketIndex++)
                {
                    StageTemplatePocketRef pocket = segment.GetPocket(pocketIndex);
                    if (pocket.ObjectiveKind == StageTemplatePocketObjectiveKind.None
                        || pocket.CurrentExecutionOwnerDisposition
                            != StageTemplateCurrentExecutionOwnerDisposition.ExistingSceneOwner
                        || pocket.P1CAdmissionDisposition
                            != StageTemplateP1CAdmissionDisposition.NotAdmitted
                        || pocket.EnemyRoleCount != 0
                        || !ValidateSourceDisposition(pocket))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ValidateSourceDisposition(StageTemplatePocketRef pocket)
        {
            if (string.IsNullOrWhiteSpace(pocket.SourceSemanticId))
            {
                return false;
            }

            return pocket.SourceDisposition switch
            {
                StageTemplateSourceDisposition.CanonicalSemanticDigest =>
                    pocket.SourceRevision >= 1
                    && !string.IsNullOrWhiteSpace(pocket.SourceSemanticDigest),
                StageTemplateSourceDisposition.RuntimeStateBoundary =>
                    pocket.SourceRevision == 0
                    && string.IsNullOrEmpty(pocket.SourceSemanticDigest),
                StageTemplateSourceDisposition.RouteConditionBoundary =>
                    pocket.SourceRevision >= 1
                    && string.IsNullOrEmpty(pocket.SourceSemanticDigest),
                _ => false
            };
        }

        private static bool ValidateReferenceDispositions(StageReferenceBlock block)
        {
            return block.ResultDefinitionDisposition
                    == StageReferenceDisposition.NotAuthoredForCurrentSchema
                && block.ProgressionNodeDisposition
                    == StageReferenceDisposition.NotAuthoredForCurrentSchema
                && block.RuleSetDisposition
                    == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.ModifierDisposition
                    == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.EnemyVariantDisposition
                    == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.TutorialCourseDisposition
                    == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.RewardPlanDisposition == StageReferenceDisposition.NoVerifiedSource;
        }

        private static bool TryResolveStory(
            PlayableStageDefinition route,
            StageReferenceBlock block,
            out string segmentId,
            out string handoffId,
            out string cinematicSequenceId,
            out string expectedPortId,
            out string stageAnchorId,
            out string stageRuntimeStateId,
            out string triggerConditionId,
            out string completionConditionId)
        {
            segmentId = string.Empty;
            handoffId = string.Empty;
            cinematicSequenceId = string.Empty;
            expectedPortId = string.Empty;
            stageAnchorId = string.Empty;
            stageRuntimeStateId = string.Empty;
            triggerConditionId = string.Empty;
            completionConditionId = string.Empty;

            StageSceneSegmentRef entry = route.SceneSegmentCount > 0
                ? route.GetSceneSegment(0)
                : null;
            StagePresentationHandoffRef story = entry?.EntryPresentation;
            if (block.StoryEntryDisposition == StageReferenceDisposition.Present)
            {
                if (entry == null
                    || story == null
                    || !story.IsPresent
                    || story.CinematicProfile == null
                    || string.IsNullOrWhiteSpace(entry.SegmentId)
                    || string.IsNullOrWhiteSpace(story.HandoffId)
                    || string.IsNullOrWhiteSpace(story.CinematicProfile.SequenceId)
                    || string.IsNullOrWhiteSpace(story.ExpectedPortId)
                    || string.IsNullOrWhiteSpace(story.CinematicProfile.StageAnchorId)
                    || string.IsNullOrWhiteSpace(story.CinematicProfile.StageRuntimeStateId)
                    || string.IsNullOrWhiteSpace(story.TriggerConditionId)
                    || string.IsNullOrWhiteSpace(story.CompletionConditionId))
                {
                    return false;
                }

                segmentId = entry.SegmentId;
                handoffId = story.HandoffId;
                cinematicSequenceId = story.CinematicProfile.SequenceId;
                expectedPortId = story.ExpectedPortId;
                stageAnchorId = story.CinematicProfile.StageAnchorId;
                stageRuntimeStateId = story.CinematicProfile.StageRuntimeStateId;
                triggerConditionId = story.TriggerConditionId;
                completionConditionId = story.CompletionConditionId;
            }
            else if (block.StoryEntryDisposition != StageReferenceDisposition.None
                || story?.IsPresent == true)
            {
                return false;
            }

            return block.StoryExitDisposition is StageReferenceDisposition.None
                or StageReferenceDisposition.NoFinalSegmentExitPresentationAuthored;
        }

        private static bool TryBuildActions(
            PlayableStageDefinition route,
            out StageBriefingActionReadModel[] actions)
        {
            var rows = new List<StageBriefingActionReadModel>(route.TerminalActionCount);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < route.TerminalActionCount; i++)
            {
                StageRouteActionRef action = route.GetTerminalAction(i);
                if (action == null
                    || string.IsNullOrWhiteSpace(action.ActionId)
                    || !ids.Add(action.ActionId)
                    || action.ActionKind == 0
                    || action.AllowedOutcomes == StageRouteOutcome.None)
                {
                    actions = null;
                    return false;
                }

                rows.Add(new StageBriefingActionReadModel(
                    action.ActionId,
                    action.ActionKind,
                    action.TargetPlayableStageId,
                    action.TargetUiRouteId,
                    action.AllowedOutcomes));
            }

            rows.Sort((left, right) => string.Compare(
                left.ActionId,
                right.ActionId,
                StringComparison.Ordinal));
            actions = rows.ToArray();
            return true;
        }
    }
}
