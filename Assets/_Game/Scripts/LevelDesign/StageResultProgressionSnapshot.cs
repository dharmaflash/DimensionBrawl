using System;
using System.Collections.Generic;
using System.Text;
using DimensionBrawl.UI.StageClear;

namespace DimensionBrawl.LevelDesign
{
    public sealed class StageResultActionPresentationMappingSnapshot
    {
        internal StageResultActionPresentationMappingSnapshot(
            StageRouteOutcome outcome,
            string actionId,
            string labelKey,
            StageResultActionPresentationRole role,
            int displayOrder)
        {
            Outcome = outcome;
            ActionId = actionId ?? string.Empty;
            LabelKey = labelKey ?? string.Empty;
            Role = role;
            DisplayOrder = displayOrder;
        }

        public StageRouteOutcome Outcome { get; }
        public string ActionId { get; }
        public string LabelKey { get; }
        public StageResultActionPresentationRole Role { get; }
        public int DisplayOrder { get; }
    }

    public sealed class StageResultDefinitionSnapshot
    {
        private StageResultDefinitionSnapshot(
            StageResultDefinition source,
            StageResultPresentationSourceSnapshot presentationSource)
        {
            SchemaVersion = source.SchemaVersion;
            ResultDefinitionId = source.ResultDefinitionId ?? string.Empty;
            Revision = source.Revision;
            EvaluationContentRevision = source.EvaluationContentRevision;
            PlayableStageId = source.PlayableStageId ?? string.Empty;
            SupportedRunSchemaVersion = source.SupportedRunSchemaVersion;
            MasterySetDisposition = source.MasterySetDisposition;
            MasterySetId = source.MasterySetId ?? string.Empty;
            MasterySetRevision = source.MasterySetRevision;
            MasterySetSemanticDigest = source.MasterySetSemanticDigest ?? string.Empty;
            RequiredFactCapabilitiesDisposition = source.RequiredFactCapabilitiesDisposition;
            RequiredFactCapabilityCount = source.RequiredFactCapabilityCount;
            RequiredFactCapabilitiesDigest = source.RequiredFactCapabilitiesDigest ?? string.Empty;
            AllowedSemanticProofsDisposition = source.AllowedSemanticProofsDisposition;
            AllowedSemanticProofCount = source.AllowedSemanticProofCount;
            AllowedSemanticProofsDigest = source.AllowedSemanticProofsDigest ?? string.Empty;
            PresentationSource = presentationSource;
            EvaluationContentDigest = RecomputeEvaluationContentDigest();
        }

        public int SchemaVersion { get; }
        public string ResultDefinitionId { get; }
        public int Revision { get; }
        public int EvaluationContentRevision { get; }
        public string PlayableStageId { get; }
        public int SupportedRunSchemaVersion { get; }
        public StageResultProgressionReferenceDisposition MasterySetDisposition { get; }
        public string MasterySetId { get; }
        public int MasterySetRevision { get; }
        public string MasterySetSemanticDigest { get; }
        public StageResultProgressionReferenceDisposition RequiredFactCapabilitiesDisposition { get; }
        public int RequiredFactCapabilityCount { get; }
        public string RequiredFactCapabilitiesDigest { get; }
        public StageResultProgressionReferenceDisposition AllowedSemanticProofsDisposition { get; }
        public int AllowedSemanticProofCount { get; }
        public string AllowedSemanticProofsDigest { get; }
        public StageResultPresentationSourceSnapshot PresentationSource { get; }
        public string EvaluationContentDigest { get; }

        internal static bool TryCreate(
            StageResultDefinition source,
            out StageResultDefinitionSnapshot snapshot,
            out string error)
        {
            try
            {
                return TryBuildCandidate(source, true, out snapshot, out error);
            }
            catch (Exception)
            {
                snapshot = null;
                error = "Stage result definition contains damaged nested data.";
                return false;
            }
        }

        internal static bool TryComputeCanonicalDigests(
            StageResultDefinition source,
            out string evaluationContentDigest,
            out string presentationBindingDigest,
            out string presentationSourceDigest,
            out string error)
        {
            evaluationContentDigest = string.Empty;
            presentationBindingDigest = string.Empty;
            presentationSourceDigest = string.Empty;
            try
            {
                if (!TryBuildCandidate(
                        source,
                        false,
                        out StageResultDefinitionSnapshot candidate,
                        out error))
                {
                    return false;
                }

                evaluationContentDigest = candidate.EvaluationContentDigest;
                presentationBindingDigest = candidate.PresentationSource.PresentationBindingDigest;
                presentationSourceDigest = candidate.PresentationSource.CanonicalDigest;
                return true;
            }
            catch (Exception)
            {
                evaluationContentDigest = string.Empty;
                presentationBindingDigest = string.Empty;
                presentationSourceDigest = string.Empty;
                error = "Stage result definition digest source contains damaged nested data.";
                return false;
            }
        }

        private static bool TryBuildCandidate(
            StageResultDefinition source,
            bool requireStoredDigests,
            out StageResultDefinitionSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (source == null
                || source.SchemaVersion != 1
                || source.Revision != 1
                || source.EvaluationContentRevision != 1
                || source.SupportedRunSchemaVersion != 1
                || source.PresentationBindingRevision != 1
                || source.LocaleResolutionPolicy
                    != StageResultLocaleResolutionPolicy
                        .ExactThenLanguageThenDefaultOrdinalIgnoreCase
                || string.IsNullOrWhiteSpace(source.ResultDefinitionId)
                || string.IsNullOrWhiteSpace(source.PlayableStageId))
            {
                error = "Stage result definition identity/schema is incomplete or unsupported.";
                return false;
            }

            if (!HasCurrentTypedAbsence(
                    source.MasterySetDisposition,
                    source.MasterySetId,
                    source.MasterySetRevision,
                    source.MasterySetSemanticDigest)
                || source.RequiredFactCapabilitiesDisposition
                    != StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema
                || source.RequiredFactCapabilityCount != 0
                || !string.IsNullOrEmpty(source.RequiredFactCapabilitiesDigest)
                || source.AllowedSemanticProofsDisposition
                    != StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema
                || source.AllowedSemanticProofCount != 0
                || !string.IsNullOrEmpty(source.AllowedSemanticProofsDigest))
            {
                error = "Stage result definition current-schema typed absences are invalid.";
                return false;
            }

            if (source.CanonicalPresentationCatalog == null
                || !source.CanonicalPresentationCatalog.TryValidateExactSources(
                    source.PlayableStageId,
                    source.PresentationProfile,
                    source.LocalizationTable,
                    out error)
                || source.PresentationProfile == null
                || source.LocalizationTable == null
                || !source.PresentationProfile.TryValidate(source.LocalizationTable, out error)
                || !string.Equals(
                    source.PresentationProfile.PlayableStageId,
                    source.PlayableStageId,
                    StringComparison.Ordinal)
                || source.PresentationProfile.SupportedRunSchemaVersion
                    != source.SupportedRunSchemaVersion
                || !StageResultPresentationProfileSnapshot.TryCreate(
                    source.PresentationProfile,
                    out StageResultPresentationProfileSnapshot profile,
                    out error)
                || !StageResultLocalizationSnapshot.TryCreate(
                    source.LocalizationTable,
                    out StageResultLocalizationSnapshot localization,
                    out error))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Stage result definition presentation sources are invalid.";
                }

                return false;
            }

            var mappings = new StageResultActionPresentationMappingSnapshot[
                source.ActionMappingCount];
            var identities = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < mappings.Length; i++)
            {
                StageResultActionPresentationMapping mapping = source.GetActionMapping(i);
                if (mapping == null
                    || (mapping.Outcome != StageRouteOutcome.Clear
                        && mapping.Outcome != StageRouteOutcome.Fail)
                    || string.IsNullOrWhiteSpace(mapping.ActionId)
                    || string.IsNullOrWhiteSpace(mapping.LabelKey)
                    || (mapping.Role != StageResultActionPresentationRole.Primary
                        && mapping.Role != StageResultActionPresentationRole.Secondary)
                    || mapping.DisplayOrder < 0
                    || !identities.Add($"{(int)mapping.Outcome}:{mapping.ActionId}"))
                {
                    error = $"Stage result presentation mapping {i} is invalid or duplicated.";
                    return false;
                }

                mappings[i] = new StageResultActionPresentationMappingSnapshot(
                    mapping.Outcome,
                    mapping.ActionId,
                    mapping.LabelKey,
                    mapping.Role,
                    mapping.DisplayOrder);
                if (i > 0 && CompareMappings(mappings[i - 1], mappings[i]) >= 0)
                {
                    error = "Stage result presentation mappings are not in canonical order.";
                    return false;
                }
            }

            if (mappings.Length != 4
                || !HasExactOutcomeRolesAndProfileKeys(mappings, profile))
            {
                error = "Stage result presentation mappings do not define exact Clear/Fail actions.";
                return false;
            }

            var presentation = new StageResultPresentationSourceSnapshot(
                source.ResultDefinitionId,
                source.PresentationBindingRevision,
                source.SupportedRunSchemaVersion,
                source.LocaleResolutionPolicy,
                profile,
                localization,
                mappings);
            var candidate = new StageResultDefinitionSnapshot(source, presentation);
            if (requireStoredDigests
                && (!string.Equals(
                        candidate.EvaluationContentDigest,
                        source.EvaluationContentDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        presentation.PresentationBindingDigest,
                        source.PresentationBindingDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        presentation.CanonicalDigest,
                        source.PresentationSourceDigest,
                        StringComparison.Ordinal)))
            {
                error = "Stage result definition stored canonical digest is stale.";
                return false;
            }

            snapshot = candidate;
            return true;
        }

        internal bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            if (SchemaVersion != 1
                || Revision != 1
                || EvaluationContentRevision != 1
                || SupportedRunSchemaVersion != 1
                || string.IsNullOrWhiteSpace(ResultDefinitionId)
                || string.IsNullOrWhiteSpace(PlayableStageId)
                || PresentationSource == null
                || !PresentationSource.TryValidateIntegrity(out error)
                || !string.Equals(
                    EvaluationContentDigest,
                    RecomputeEvaluationContentDigest(),
                    StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "Deep-copied stage result definition is damaged.";
                }

                return false;
            }

            error = string.Empty;
            return true;
        }

        internal string RecomputeEvaluationContentDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "resultEvaluation.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.resultDefinitionId",
                ResultDefinitionId);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.evaluationContentRevision",
                EvaluationContentRevision);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.playableStageId",
                PlayableStageId);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.supportedRunSchemaVersion",
                SupportedRunSchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.masterySetDisposition",
                (int)MasterySetDisposition);
            StageCanonicalDigest.Append(builder, "resultEvaluation.masterySetId", MasterySetId);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.masterySetRevision",
                MasterySetRevision);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.masterySetSemanticDigest",
                MasterySetSemanticDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.requiredFactCapabilitiesDisposition",
                (int)RequiredFactCapabilitiesDisposition);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.requiredFactCapabilityCount",
                RequiredFactCapabilityCount);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.requiredFactCapabilitiesDigest",
                RequiredFactCapabilitiesDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.allowedSemanticProofsDisposition",
                (int)AllowedSemanticProofsDisposition);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.allowedSemanticProofCount",
                AllowedSemanticProofCount);
            StageCanonicalDigest.Append(
                builder,
                "resultEvaluation.allowedSemanticProofsDigest",
                AllowedSemanticProofsDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private static bool HasCurrentTypedAbsence(
            StageResultProgressionReferenceDisposition disposition,
            string id,
            int revision,
            string digest)
        {
            return disposition
                    == StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema
                && string.IsNullOrEmpty(id)
                && revision == 0
                && string.IsNullOrEmpty(digest);
        }

        private static int CompareMappings(
            StageResultActionPresentationMappingSnapshot left,
            StageResultActionPresentationMappingSnapshot right)
        {
            int comparison = ((int)left.Outcome).CompareTo((int)right.Outcome);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.DisplayOrder.CompareTo(right.DisplayOrder);
            return comparison != 0
                ? comparison
                : string.Compare(left.ActionId, right.ActionId, StringComparison.Ordinal);
        }

        private static bool HasExactOutcomeRolesAndProfileKeys(
            StageResultActionPresentationMappingSnapshot[] mappings,
            StageResultPresentationProfileSnapshot profile)
        {
            return HasMapping(
                    mappings,
                    StageRouteOutcome.Clear,
                    StageResultActionPresentationRole.Primary,
                    0,
                    profile.ReplayActionKey)
                && HasMapping(
                    mappings,
                    StageRouteOutcome.Clear,
                    StageResultActionPresentationRole.Secondary,
                    1,
                    profile.LobbyActionKey)
                && HasMapping(
                    mappings,
                    StageRouteOutcome.Fail,
                    StageResultActionPresentationRole.Primary,
                    0,
                    profile.RetryActionKey)
                && HasMapping(
                    mappings,
                    StageRouteOutcome.Fail,
                    StageResultActionPresentationRole.Secondary,
                    1,
                    profile.LobbyActionKey);
        }

        private static bool HasMapping(
            StageResultActionPresentationMappingSnapshot[] mappings,
            StageRouteOutcome outcome,
            StageResultActionPresentationRole role,
            int displayOrder,
            string labelKey)
        {
            for (int i = 0; i < mappings.Length; i++)
            {
                StageResultActionPresentationMappingSnapshot mapping = mappings[i];
                if (mapping.Outcome == outcome
                    && mapping.Role == role
                    && mapping.DisplayOrder == displayOrder
                    && string.Equals(mapping.LabelKey, labelKey, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class StageProgressionPrerequisiteSnapshot
    {
        internal StageProgressionPrerequisiteSnapshot(
            string targetProgressionNodeId,
            int targetProgressionNodeRevision,
            StageProgressionRequirementKind requirementKind,
            string requiredObjectiveId)
        {
            TargetProgressionNodeId = targetProgressionNodeId ?? string.Empty;
            TargetProgressionNodeRevision = targetProgressionNodeRevision;
            RequirementKind = requirementKind;
            RequiredObjectiveId = requiredObjectiveId ?? string.Empty;
        }

        public string TargetProgressionNodeId { get; }
        public int TargetProgressionNodeRevision { get; }
        public StageProgressionRequirementKind RequirementKind { get; }
        public string RequiredObjectiveId { get; }
    }

    public sealed class StageProgressionRecommendedNextSnapshot
    {
        internal StageProgressionRecommendedNextSnapshot(
            string targetProgressionNodeId,
            int targetProgressionNodeRevision)
        {
            TargetProgressionNodeId = targetProgressionNodeId ?? string.Empty;
            TargetProgressionNodeRevision = targetProgressionNodeRevision;
        }

        public string TargetProgressionNodeId { get; }
        public int TargetProgressionNodeRevision { get; }
    }

    public sealed class StageProgressionNodeSnapshot
    {
        private readonly StageProgressionPrerequisiteSnapshot[] prerequisites;
        private readonly StageProgressionRecommendedNextSnapshot[] recommendedNext;

        private StageProgressionNodeSnapshot(
            StageProgressionNode source,
            StageProgressionPrerequisiteSnapshot[] prerequisites,
            StageProgressionRecommendedNextSnapshot[] recommendedNext)
        {
            SchemaVersion = source.SchemaVersion;
            ProgressionNodeId = source.ProgressionNodeId ?? string.Empty;
            Revision = source.Revision;
            ContentRevision = source.ContentRevision;
            BattleStageDisposition = source.BattleStageDisposition;
            BattleStageId = source.BattleStageId ?? string.Empty;
            this.prerequisites = prerequisites
                ?? Array.Empty<StageProgressionPrerequisiteSnapshot>();
            this.recommendedNext = recommendedNext
                ?? Array.Empty<StageProgressionRecommendedNextSnapshot>();
            PreBattleStoryDisposition = source.PreBattleStoryDisposition;
            PreBattleStoryId = source.PreBattleStoryId ?? string.Empty;
            PostBattleStoryDisposition = source.PostBattleStoryDisposition;
            PostBattleStoryId = source.PostBattleStoryId ?? string.Empty;
            AfterClearScriptDisposition = source.AfterClearScriptDisposition;
            AfterClearScriptId = source.AfterClearScriptId ?? string.Empty;
            RewardPlanDisposition = source.RewardPlanDisposition;
            RewardPlanId = source.RewardPlanId ?? string.Empty;
            RewardPlanRevision = source.RewardPlanRevision;
            RewardPlanDigest = source.RewardPlanDigest ?? string.Empty;
            BindingRevision = source.BindingRevision;
            PlayableStageId = source.PlayableStageId ?? string.Empty;
            RouteRevision = source.RouteRevision;
            CanonicalRouteDigest = source.CanonicalRouteDigest ?? string.Empty;
            ProgressionGraphId = source.ProgressionGraphId ?? string.Empty;
            ProgressionGraphRevision = source.ProgressionGraphRevision;
            ContentDigest = RecomputeContentDigest();
            BindingDigest = RecomputeBindingDigest();
        }

        public int SchemaVersion { get; }
        public string ProgressionNodeId { get; }
        public int Revision { get; }
        public int ContentRevision { get; }
        public StageResultProgressionReferenceDisposition BattleStageDisposition { get; }
        public string BattleStageId { get; }
        public int PrerequisiteCount => prerequisites.Length;
        public int RecommendedNextCount => recommendedNext.Length;
        public StageResultProgressionReferenceDisposition PreBattleStoryDisposition { get; }
        public string PreBattleStoryId { get; }
        public StageResultProgressionReferenceDisposition PostBattleStoryDisposition { get; }
        public string PostBattleStoryId { get; }
        public StageResultProgressionReferenceDisposition AfterClearScriptDisposition { get; }
        public string AfterClearScriptId { get; }
        public StageResultProgressionReferenceDisposition RewardPlanDisposition { get; }
        public string RewardPlanId { get; }
        public int RewardPlanRevision { get; }
        public string RewardPlanDigest { get; }
        public int BindingRevision { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string CanonicalRouteDigest { get; }
        public string ProgressionGraphId { get; }
        public int ProgressionGraphRevision { get; }
        public string ContentDigest { get; }
        public string BindingDigest { get; }

        public StageProgressionPrerequisiteSnapshot GetPrerequisite(int index)
        {
            if (index < 0 || index >= prerequisites.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return prerequisites[index];
        }

        public StageProgressionRecommendedNextSnapshot GetRecommendedNext(int index)
        {
            if (index < 0 || index >= recommendedNext.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return recommendedNext[index];
        }

        internal static bool TryCreate(
            StageProgressionNode source,
            out StageProgressionNodeSnapshot snapshot,
            out string error)
        {
            try
            {
                return TryBuildCandidate(source, true, out snapshot, out error);
            }
            catch (Exception)
            {
                snapshot = null;
                error = "Stage progression node contains damaged nested data.";
                return false;
            }
        }

        internal static bool TryComputeCanonicalDigests(
            StageProgressionNode source,
            out string contentDigest,
            out string bindingDigest,
            out string error)
        {
            contentDigest = string.Empty;
            bindingDigest = string.Empty;
            try
            {
                if (!TryBuildCandidate(
                        source,
                        false,
                        out StageProgressionNodeSnapshot candidate,
                        out error))
                {
                    return false;
                }

                contentDigest = candidate.ContentDigest;
                bindingDigest = candidate.BindingDigest;
                return true;
            }
            catch (Exception)
            {
                contentDigest = string.Empty;
                bindingDigest = string.Empty;
                error = "Stage progression node digest source contains damaged nested data.";
                return false;
            }
        }

        private static bool TryBuildCandidate(
            StageProgressionNode source,
            bool requireStoredDigests,
            out StageProgressionNodeSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (source == null
                || source.SchemaVersion != 1
                || source.Revision != 1
                || source.ContentRevision != 1
                || (source.BindingRevision != 1 && source.BindingRevision != 2)
                || source.RouteRevision < 1
                || source.ProgressionGraphRevision < 1
                || string.IsNullOrWhiteSpace(source.ProgressionNodeId)
                || string.IsNullOrWhiteSpace(source.PlayableStageId)
                || string.IsNullOrWhiteSpace(source.CanonicalRouteDigest)
                || string.IsNullOrWhiteSpace(source.ProgressionGraphId))
            {
                error = "Stage progression node identity/schema is incomplete or unsupported.";
                return false;
            }

            if (!HasTypedEmpty(
                    source.BattleStageDisposition,
                    StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema,
                    source.BattleStageId)
                || !HasTypedEmpty(
                    source.PreBattleStoryDisposition,
                    StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema,
                    source.PreBattleStoryId)
                || !HasTypedEmpty(
                    source.PostBattleStoryDisposition,
                    StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema,
                    source.PostBattleStoryId)
                || !HasTypedEmpty(
                    source.AfterClearScriptDisposition,
                    StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema,
                    source.AfterClearScriptId)
                || source.RewardPlanDisposition
                    != StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema
                || !string.IsNullOrEmpty(source.RewardPlanId)
                || source.RewardPlanRevision != 0
                || !string.IsNullOrEmpty(source.RewardPlanDigest))
            {
                error = "Stage progression node current-schema typed absences are invalid.";
                return false;
            }

            var prerequisiteCopies = new StageProgressionPrerequisiteSnapshot[
                source.PrerequisiteCount];
            var prerequisiteTargets = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < prerequisiteCopies.Length; i++)
            {
                StageProgressionPrerequisiteRef edge = source.GetPrerequisite(i);
                StageProgressionNode target = edge?.TargetProgressionNode;
                string targetId = target?.ProgressionNodeId ?? string.Empty;
                if (edge == null
                    || target == null
                    || string.IsNullOrWhiteSpace(targetId)
                    || targetId == source.ProgressionNodeId
                    || edge.TargetProgressionNodeRevision < 1
                    || target.Revision != edge.TargetProgressionNodeRevision
                    || !prerequisiteTargets.Add(targetId)
                    || !HasValidObjective(edge.RequirementKind, edge.RequiredObjectiveId))
                {
                    error = $"Stage progression prerequisite {i} is invalid.";
                    return false;
                }

                prerequisiteCopies[i] = new StageProgressionPrerequisiteSnapshot(
                    targetId,
                    edge.TargetProgressionNodeRevision,
                    edge.RequirementKind,
                    edge.RequiredObjectiveId);
                if (i > 0
                    && ComparePrerequisites(prerequisiteCopies[i - 1], prerequisiteCopies[i]) >= 0)
                {
                    error = "Stage progression prerequisites are not in canonical tuple order.";
                    return false;
                }
            }

            var recommendedCopies = new StageProgressionRecommendedNextSnapshot[
                source.RecommendedNextCount];
            var recommendedTargets = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < recommendedCopies.Length; i++)
            {
                StageProgressionRecommendedNextRef edge = source.GetRecommendedNext(i);
                StageProgressionNode target = edge?.TargetProgressionNode;
                string targetId = target?.ProgressionNodeId ?? string.Empty;
                if (edge == null
                    || target == null
                    || string.IsNullOrWhiteSpace(targetId)
                    || targetId == source.ProgressionNodeId
                    || edge.TargetProgressionNodeRevision < 1
                    || target.Revision != edge.TargetProgressionNodeRevision
                    || !recommendedTargets.Add(targetId))
                {
                    error = $"Stage progression recommended-next edge {i} is invalid.";
                    return false;
                }

                recommendedCopies[i] = new StageProgressionRecommendedNextSnapshot(
                    targetId,
                    edge.TargetProgressionNodeRevision);
                if (i > 0
                    && CompareRecommended(recommendedCopies[i - 1], recommendedCopies[i]) >= 0)
                {
                    error = "Stage progression recommended-next edges are not in canonical tuple order.";
                    return false;
                }
            }

            var candidate = new StageProgressionNodeSnapshot(
                source,
                prerequisiteCopies,
                recommendedCopies);
            if (requireStoredDigests
                && (!string.Equals(
                        candidate.ContentDigest,
                        source.ContentDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        candidate.BindingDigest,
                        source.BindingDigest,
                        StringComparison.Ordinal)))
            {
                error = "Stage progression node stored canonical digest is stale.";
                return false;
            }

            snapshot = candidate;
            return true;
        }

        internal bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            if (SchemaVersion != 1
                || Revision != 1
                || ContentRevision != 1
                || (BindingRevision != 1 && BindingRevision != 2)
                || string.IsNullOrWhiteSpace(ProgressionNodeId)
                || !string.Equals(ContentDigest, RecomputeContentDigest(), StringComparison.Ordinal)
                || !string.Equals(BindingDigest, RecomputeBindingDigest(), StringComparison.Ordinal))
            {
                error = "Deep-copied stage progression node is damaged.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        internal string RecomputeContentDigest()
        {
            StringBuilder builder = new(4096);
            StageCanonicalDigest.Append(builder, "progressionNode.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "progressionNode.id", ProgressionNodeId);
            StageCanonicalDigest.Append(builder, "progressionNode.contentRevision", ContentRevision);
            StageCanonicalDigest.Append(
                builder,
                "progressionNode.battleStageDisposition",
                (int)BattleStageDisposition);
            StageCanonicalDigest.Append(builder, "progressionNode.battleStageId", BattleStageId);
            StageCanonicalDigest.Append(builder, "progressionNode.prerequisiteCount", prerequisites.Length);
            StageCanonicalDigest.Append(builder, "progressionNode.recommendedNextCount", recommendedNext.Length);
            for (int i = 0; i < prerequisites.Length; i++)
            {
                StageProgressionPrerequisiteSnapshot edge = prerequisites[i];
                string prefix = $"progressionNode.prerequisite[{i}]";
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".targetProgressionNodeId",
                    edge.TargetProgressionNodeId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".targetProgressionNodeRevision",
                    edge.TargetProgressionNodeRevision);
                StageCanonicalDigest.Append(builder, prefix + ".requirementKind", (int)edge.RequirementKind);
                StageCanonicalDigest.Append(builder, prefix + ".requiredObjectiveId", edge.RequiredObjectiveId);
            }

            for (int i = 0; i < recommendedNext.Length; i++)
            {
                StageProgressionRecommendedNextSnapshot edge = recommendedNext[i];
                string prefix = $"progressionNode.recommendedNext[{i}]";
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".targetProgressionNodeId",
                    edge.TargetProgressionNodeId);
                StageCanonicalDigest.Append(
                    builder,
                    prefix + ".targetProgressionNodeRevision",
                    edge.TargetProgressionNodeRevision);
            }

            StageCanonicalDigest.Append(
                builder,
                "progressionNode.preBattleStoryDisposition",
                (int)PreBattleStoryDisposition);
            StageCanonicalDigest.Append(builder, "progressionNode.preBattleStoryId", PreBattleStoryId);
            StageCanonicalDigest.Append(
                builder,
                "progressionNode.postBattleStoryDisposition",
                (int)PostBattleStoryDisposition);
            StageCanonicalDigest.Append(builder, "progressionNode.postBattleStoryId", PostBattleStoryId);
            StageCanonicalDigest.Append(
                builder,
                "progressionNode.afterClearScriptDisposition",
                (int)AfterClearScriptDisposition);
            StageCanonicalDigest.Append(builder, "progressionNode.afterClearScriptId", AfterClearScriptId);
            StageCanonicalDigest.Append(
                builder,
                "progressionNode.rewardPlanDisposition",
                (int)RewardPlanDisposition);
            StageCanonicalDigest.Append(builder, "progressionNode.rewardPlanId", RewardPlanId);
            StageCanonicalDigest.Append(builder, "progressionNode.rewardPlanRevision", RewardPlanRevision);
            StageCanonicalDigest.Append(builder, "progressionNode.rewardPlanDigest", RewardPlanDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        internal string RecomputeBindingDigest()
        {
            StringBuilder builder = new(1024);
            StageCanonicalDigest.Append(builder, "progressionBinding.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "progressionBinding.progressionNodeId",
                ProgressionNodeId);
            StageCanonicalDigest.Append(
                builder,
                "progressionBinding.bindingRevision",
                BindingRevision);
            StageCanonicalDigest.Append(builder, "progressionBinding.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "progressionBinding.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(
                builder,
                "progressionBinding.canonicalRouteDigest",
                CanonicalRouteDigest);
            StageCanonicalDigest.Append(
                builder,
                "progressionBinding.progressionGraphId",
                ProgressionGraphId);
            StageCanonicalDigest.Append(
                builder,
                "progressionBinding.progressionGraphRevision",
                ProgressionGraphRevision);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private static bool HasTypedEmpty(
            StageResultProgressionReferenceDisposition actual,
            StageResultProgressionReferenceDisposition expected,
            string value)
        {
            return actual == expected && string.IsNullOrEmpty(value);
        }

        private static bool HasValidObjective(
            StageProgressionRequirementKind kind,
            string objectiveId)
        {
            return kind == StageProgressionRequirementKind.Cleared
                ? string.IsNullOrEmpty(objectiveId)
                : kind == StageProgressionRequirementKind.MasteryObjectiveAchieved
                    && !string.IsNullOrWhiteSpace(objectiveId);
        }

        private static int ComparePrerequisites(
            StageProgressionPrerequisiteSnapshot left,
            StageProgressionPrerequisiteSnapshot right)
        {
            int comparison = string.Compare(
                left.TargetProgressionNodeId,
                right.TargetProgressionNodeId,
                StringComparison.Ordinal);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.TargetProgressionNodeRevision.CompareTo(
                right.TargetProgressionNodeRevision);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = ((int)left.RequirementKind).CompareTo((int)right.RequirementKind);
            return comparison != 0
                ? comparison
                : string.Compare(
                    left.RequiredObjectiveId,
                    right.RequiredObjectiveId,
                    StringComparison.Ordinal);
        }

        private static int CompareRecommended(
            StageProgressionRecommendedNextSnapshot left,
            StageProgressionRecommendedNextSnapshot right)
        {
            int comparison = string.Compare(
                left.TargetProgressionNodeId,
                right.TargetProgressionNodeId,
                StringComparison.Ordinal);
            return comparison != 0
                ? comparison
                : left.TargetProgressionNodeRevision.CompareTo(
                    right.TargetProgressionNodeRevision);
        }
    }

    public sealed class StageProgressionGraphSnapshot
    {
        private readonly StageProgressionNodeSnapshot[] nodes;

        private StageProgressionGraphSnapshot(
            StageProgressionGraph source,
            StageProgressionNodeSnapshot[] nodes)
        {
            SchemaVersion = source.SchemaVersion;
            ProgressionGraphId = source.ProgressionGraphId ?? string.Empty;
            Revision = source.Revision;
            CyclePolicy = source.CyclePolicy;
            this.nodes = nodes ?? Array.Empty<StageProgressionNodeSnapshot>();
            CanonicalDigest = RecomputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public string ProgressionGraphId { get; }
        public int Revision { get; }
        public StageProgressionCyclePolicy CyclePolicy { get; }
        public int NodeCount => nodes.Length;
        public string CanonicalDigest { get; }

        public StageProgressionNodeSnapshot GetNode(int index)
        {
            if (index < 0 || index >= nodes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return nodes[index];
        }

        public bool TryGetNode(
            string progressionNodeId,
            int revision,
            out StageProgressionNodeSnapshot node)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                StageProgressionNodeSnapshot candidate = nodes[i];
                if (candidate.Revision == revision
                    && string.Equals(
                        candidate.ProgressionNodeId,
                        progressionNodeId,
                        StringComparison.Ordinal))
                {
                    node = candidate;
                    return true;
                }
            }

            node = null;
            return false;
        }

        internal static bool TryCreate(
            StageProgressionGraph source,
            out StageProgressionGraphSnapshot snapshot,
            out string error)
        {
            try
            {
                return TryBuildCandidate(source, true, out snapshot, out error);
            }
            catch (Exception)
            {
                snapshot = null;
                error = "Stage progression graph contains damaged nested data.";
                return false;
            }
        }

        internal static bool TryComputeCanonicalDigest(
            StageProgressionGraph source,
            out string canonicalDigest,
            out string error)
        {
            canonicalDigest = string.Empty;
            try
            {
                if (!TryBuildCandidate(
                        source,
                        false,
                        out StageProgressionGraphSnapshot candidate,
                        out error))
                {
                    return false;
                }

                canonicalDigest = candidate.CanonicalDigest;
                return true;
            }
            catch (Exception)
            {
                canonicalDigest = string.Empty;
                error = "Stage progression graph digest source contains damaged nested data.";
                return false;
            }
        }

        private static bool TryBuildCandidate(
            StageProgressionGraph source,
            bool requireStoredDigest,
            out StageProgressionGraphSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (source == null
                || source.SchemaVersion != 1
                || (source.Revision != 1 && source.Revision != 2)
                || source.CyclePolicy
                    != StageProgressionCyclePolicy.DisallowCyclesWithinEachRelation
                || string.IsNullOrWhiteSpace(source.ProgressionGraphId)
                || source.NodeCount == 0)
            {
                error = "Stage progression graph identity/schema is incomplete or unsupported.";
                return false;
            }

            var nodeCopies = new StageProgressionNodeSnapshot[source.NodeCount];
            var authoredNodes = new Dictionary<string, StageProgressionNode>(StringComparer.Ordinal);
            for (int i = 0; i < nodeCopies.Length; i++)
            {
                StageProgressionNode authored = source.GetNode(i);
                if (authored == null
                    || !authoredNodes.TryAdd(authored.ProgressionNodeId, authored)
                    || (i > 0
                        && string.Compare(
                            source.GetNode(i - 1)?.ProgressionNodeId,
                            authored.ProgressionNodeId,
                            StringComparison.Ordinal) >= 0)
                    || !string.Equals(
                        authored.ProgressionGraphId,
                        source.ProgressionGraphId,
                        StringComparison.Ordinal)
                    || authored.ProgressionGraphRevision != source.Revision
                    || !authored.TryCreateSnapshot(out nodeCopies[i], out error))
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        error = $"Stage progression graph node {i} is invalid or duplicated.";
                    }

                    return false;
                }
            }

            for (int i = 0; i < source.NodeCount; i++)
            {
                StageProgressionNode authored = source.GetNode(i);
                StageProgressionNodeSnapshot copied = nodeCopies[i];
                for (int edgeIndex = 0; edgeIndex < copied.PrerequisiteCount; edgeIndex++)
                {
                    StageProgressionPrerequisiteRef edge = authored.GetPrerequisite(edgeIndex);
                    StageProgressionPrerequisiteSnapshot edgeCopy = copied.GetPrerequisite(edgeIndex);
                    if (!TryResolveExactTarget(
                            edge.TargetProgressionNode,
                            edgeCopy.TargetProgressionNodeId,
                            edgeCopy.TargetProgressionNodeRevision,
                            authoredNodes))
                    {
                        error = "Stage progression prerequisite does not resolve exact graph identity.";
                        return false;
                    }
                }

                for (int edgeIndex = 0; edgeIndex < copied.RecommendedNextCount; edgeIndex++)
                {
                    StageProgressionRecommendedNextRef edge = authored.GetRecommendedNext(edgeIndex);
                    StageProgressionRecommendedNextSnapshot edgeCopy = copied.GetRecommendedNext(edgeIndex);
                    if (!TryResolveExactTarget(
                            edge.TargetProgressionNode,
                            edgeCopy.TargetProgressionNodeId,
                            edgeCopy.TargetProgressionNodeRevision,
                            authoredNodes))
                    {
                        error = "Stage progression recommended-next edge does not resolve exact graph identity.";
                        return false;
                    }
                }
            }

            if (HasCycle(nodeCopies, true) || HasCycle(nodeCopies, false))
            {
                error = "Stage progression graph contains a cycle within one directed relation.";
                return false;
            }

            var candidate = new StageProgressionGraphSnapshot(source, nodeCopies);
            if (requireStoredDigest
                && !string.Equals(
                    candidate.CanonicalDigest,
                    source.CanonicalDigest,
                    StringComparison.Ordinal))
            {
                error = "Stage progression graph stored canonical digest is stale.";
                return false;
            }

            snapshot = candidate;
            return true;
        }

        internal bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            if (SchemaVersion != 1
                || (Revision != 1 && Revision != 2)
                || CyclePolicy != StageProgressionCyclePolicy.DisallowCyclesWithinEachRelation
                || string.IsNullOrWhiteSpace(ProgressionGraphId)
                || nodes.Length == 0
                || !string.Equals(
                    CanonicalDigest,
                    RecomputeCanonicalDigest(),
                    StringComparison.Ordinal))
            {
                error = "Deep-copied stage progression graph is damaged.";
                return false;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null || !nodes[i].TryValidateIntegrity(out error))
                {
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        internal string RecomputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "progressionGraph.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "progressionGraph.id", ProgressionGraphId);
            StageCanonicalDigest.Append(builder, "progressionGraph.revision", Revision);
            StageCanonicalDigest.Append(builder, "progressionGraph.cyclePolicy", (int)CyclePolicy);
            StageCanonicalDigest.Append(builder, "progressionGraph.nodeCount", nodes.Length);
            for (int i = 0; i < nodes.Length; i++)
            {
                StageProgressionNodeSnapshot node = nodes[i];
                string prefix = $"progressionGraph.node[{i}]";
                StageCanonicalDigest.Append(builder, prefix + ".id", node?.ProgressionNodeId);
                StageCanonicalDigest.Append(builder, prefix + ".revision", node?.Revision ?? 0);
                StageCanonicalDigest.Append(builder, prefix + ".contentDigest", node?.ContentDigest);
                StageCanonicalDigest.Append(builder, prefix + ".bindingDigest", node?.BindingDigest);
            }

            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private static bool TryResolveExactTarget(
            StageProgressionNode authoredTarget,
            string targetId,
            int targetRevision,
            Dictionary<string, StageProgressionNode> authoredNodes)
        {
            return authoredTarget != null
                && authoredNodes.TryGetValue(targetId, out StageProgressionNode exactTarget)
                && ReferenceEquals(authoredTarget, exactTarget)
                && exactTarget.Revision == targetRevision;
        }

        private static bool HasCycle(
            StageProgressionNodeSnapshot[] nodeSnapshots,
            bool prerequisites)
        {
            var byId = new Dictionary<string, StageProgressionNodeSnapshot>(StringComparer.Ordinal);
            for (int i = 0; i < nodeSnapshots.Length; i++)
            {
                byId.Add(nodeSnapshots[i].ProgressionNodeId, nodeSnapshots[i]);
            }

            var states = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < nodeSnapshots.Length; i++)
            {
                if (Visit(nodeSnapshots[i], prerequisites, byId, states))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Visit(
            StageProgressionNodeSnapshot node,
            bool prerequisites,
            Dictionary<string, StageProgressionNodeSnapshot> byId,
            Dictionary<string, int> states)
        {
            states.TryGetValue(node.ProgressionNodeId, out int state);
            if (state == 1)
            {
                return true;
            }

            if (state == 2)
            {
                return false;
            }

            states[node.ProgressionNodeId] = 1;
            int count = prerequisites ? node.PrerequisiteCount : node.RecommendedNextCount;
            for (int i = 0; i < count; i++)
            {
                string targetId = prerequisites
                    ? node.GetPrerequisite(i).TargetProgressionNodeId
                    : node.GetRecommendedNext(i).TargetProgressionNodeId;
                if (byId.TryGetValue(targetId, out StageProgressionNodeSnapshot target)
                    && Visit(target, prerequisites, byId, states))
                {
                    return true;
                }
            }

            states[node.ProgressionNodeId] = 2;
            return false;
        }
    }

    public sealed class StageRunResultProgressionJoinSnapshot
    {
        private StageRunResultProgressionJoinSnapshot(
            PlayableStageDefinition route,
            StageResultDefinitionSnapshot resultDefinition,
            StageProgressionNodeSnapshot progressionNode,
            StageProgressionGraphSnapshot progressionGraph,
            StageResultProgressionJoinBlock block,
            string canonicalReferenceDigest,
            string canonicalBriefingDigest)
        {
            SchemaVersion = block.SchemaVersion;
            Revision = block.Revision;
            PlayableStageId = route.PlayableStageId ?? string.Empty;
            RouteRevision = route.RouteRevision;
            CanonicalRouteDigest = route.CanonicalRouteDigest ?? string.Empty;
            ReferenceSchemaVersion = route.ReferenceBlock?.SchemaVersion ?? 0;
            ReferenceRevision = route.ReferenceBlock?.Revision ?? 0;
            CanonicalReferenceDigest = canonicalReferenceDigest ?? string.Empty;
            BriefingSchemaVersion = route.ReferenceBlock?.BriefingSchemaVersion ?? 0;
            BriefingRevision = route.ReferenceBlock?.BriefingRevision ?? 0;
            CanonicalBriefingDigest = canonicalBriefingDigest ?? string.Empty;
            SemanticCoupling = block.SemanticCoupling;
            ResultDefinitionDisposition = block.ResultDefinitionDisposition;
            ResultDefinition = resultDefinition;
            ProgressionNodeDisposition = block.ProgressionNodeDisposition;
            ProgressionNode = progressionNode;
            ProgressionGraphDisposition = block.ProgressionGraphDisposition;
            ProgressionGraph = progressionGraph;
            RewardPlanDisposition = block.RewardPlanDisposition;
            RewardPlanDigest = block.RewardPlanDigest ?? string.Empty;
            CanonicalDigest = RecomputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public int Revision { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string CanonicalRouteDigest { get; }
        public int ReferenceSchemaVersion { get; }
        public int ReferenceRevision { get; }
        public string CanonicalReferenceDigest { get; }
        public int BriefingSchemaVersion { get; }
        public int BriefingRevision { get; }
        public string CanonicalBriefingDigest { get; }
        public StageResultJoinSemanticCoupling SemanticCoupling { get; }
        public StageResultProgressionReferenceDisposition ResultDefinitionDisposition { get; }
        public StageResultDefinitionSnapshot ResultDefinition { get; }
        public StageResultProgressionReferenceDisposition ProgressionNodeDisposition { get; }
        public StageProgressionNodeSnapshot ProgressionNode { get; }
        public StageResultProgressionReferenceDisposition ProgressionGraphDisposition { get; }
        public StageProgressionGraphSnapshot ProgressionGraph { get; }
        public StageResultProgressionReferenceDisposition RewardPlanDisposition { get; }
        public string RewardPlanDigest { get; }
        public StageResultPresentationSourceSnapshot PresentationSource =>
            ResultDefinition?.PresentationSource;
        public string CanonicalDigest { get; }

        public static bool TryCreate(
            PlayableStageDefinition route,
            out StageRunResultProgressionJoinSnapshot snapshot,
            out string error)
        {
            try
            {
                return TryBuildCandidate(route, true, out snapshot, out error);
            }
            catch (Exception)
            {
                snapshot = null;
                error = "Stage result/progression admission source contains damaged nested data.";
                return false;
            }
        }

        internal static bool TryComputeCanonicalDigest(
            PlayableStageDefinition route,
            out string canonicalDigest,
            out string error)
        {
            canonicalDigest = string.Empty;
            try
            {
                if (!TryBuildCandidate(
                        route,
                        false,
                        out StageRunResultProgressionJoinSnapshot candidate,
                        out error))
                {
                    return false;
                }

                canonicalDigest = candidate.CanonicalDigest;
                return true;
            }
            catch (Exception)
            {
                canonicalDigest = string.Empty;
                error = "Stage result/progression digest source contains damaged nested data.";
                return false;
            }
        }

        private static bool TryBuildCandidate(
            PlayableStageDefinition route,
            bool requireStoredDigest,
            out StageRunResultProgressionJoinSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            StageResultProgressionJoinBlock block = route?.ResultProgressionJoin;
            if (route == null
                || route.SchemaVersion != 1
                || string.IsNullOrWhiteSpace(route.PlayableStageId)
                || route.RouteRevision < 1
                || string.IsNullOrWhiteSpace(route.CanonicalRouteDigest)
                || !string.Equals(
                    route.CanonicalRouteDigest,
                    route.ComputeCanonicalRouteDigest(),
                    StringComparison.Ordinal)
                || route.ReferenceBlock == null
                || !string.Equals(
                    route.ReferenceBlock.CanonicalReferenceDigest,
                    route.ComputeCanonicalReferenceDigest(),
                    StringComparison.Ordinal))
            {
                error = "Stage result/progression join route identity is invalid.";
                return false;
            }

            if (!route.TryComputeCanonicalBriefingDigest(
                    out string canonicalBriefingDigest,
                    out StageBriefingBuildRejectReason briefingRejectReason)
                || !string.Equals(
                    route.ReferenceBlock.CanonicalBriefingDigest,
                    canonicalBriefingDigest,
                    StringComparison.Ordinal))
            {
                error = $"Stage result/progression join briefing is invalid: {briefingRejectReason}.";
                return false;
            }

            if (block == null
                || !block.Present
                || block.SchemaVersion != 1
                || (block.Revision != 1 && block.Revision != 2)
                || block.SemanticCoupling
                    != StageResultJoinSemanticCoupling
                        .PresentationAuditSidecarOutsideP1ASemanticResult
                || block.ResultDefinitionDisposition
                    != StageResultProgressionReferenceDisposition.Present
                || block.ProgressionNodeDisposition
                    != StageResultProgressionReferenceDisposition.Present
                || block.ProgressionGraphDisposition
                    != StageResultProgressionReferenceDisposition.Present
                || block.RewardPlanDisposition
                    != StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema
                || !string.IsNullOrEmpty(block.RewardPlanDigest)
                || block.ResultDefinition == null
                || block.CanonicalPresentationCatalog == null
                || block.ProgressionNode == null
                || block.ProgressionGraph == null)
            {
                error = "Stage result/progression join sidecar is missing or unsupported.";
                return false;
            }

            if (!ReferenceEquals(
                    block.CanonicalPresentationCatalog,
                    block.ResultDefinition.CanonicalPresentationCatalog))
            {
                error =
                    "Stage result definition does not reference the route-owned canonical presentation catalog.";
                return false;
            }

            if (!block.ResultDefinition.TryCreateSnapshot(
                    out StageResultDefinitionSnapshot resultDefinition,
                    out error)
                || !block.ProgressionGraph.TryCreateSnapshot(
                    out StageProgressionGraphSnapshot progressionGraph,
                    out error))
            {
                return false;
            }

            bool exactNodeFound = false;
            for (int i = 0; i < block.ProgressionGraph.NodeCount; i++)
            {
                if (ReferenceEquals(block.ProgressionGraph.GetNode(i), block.ProgressionNode))
                {
                    exactNodeFound = true;
                    break;
                }
            }

            if (!exactNodeFound
                || !progressionGraph.TryGetNode(
                    block.ProgressionNode.ProgressionNodeId,
                    block.ProgressionNode.Revision,
                    out StageProgressionNodeSnapshot progressionNode)
                || !string.Equals(
                    resultDefinition.PlayableStageId,
                    route.PlayableStageId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    progressionNode.PlayableStageId,
                    route.PlayableStageId,
                    StringComparison.Ordinal)
                || progressionNode.RouteRevision != route.RouteRevision
                || !string.Equals(
                    progressionNode.CanonicalRouteDigest,
                    route.CanonicalRouteDigest,
                    StringComparison.Ordinal))
            {
                error = "Stage result/progression join direct identities do not match the route.";
                return false;
            }

            var candidate = new StageRunResultProgressionJoinSnapshot(
                route,
                resultDefinition,
                progressionNode,
                progressionGraph,
                block,
                route.ReferenceBlock.CanonicalReferenceDigest,
                canonicalBriefingDigest);
            if (requireStoredDigest
                && !string.Equals(
                    candidate.CanonicalDigest,
                    block.CanonicalDigest,
                    StringComparison.Ordinal))
            {
                error = "Stage result/progression join stored canonical digest is stale.";
                return false;
            }

            snapshot = candidate;
            return true;
        }

        public bool TryValidateIntegrity(out string error)
        {
            error = string.Empty;
            try
            {
                if (SchemaVersion != 1
                    || (Revision != 1 && Revision != 2)
                    || ResultDefinitionDisposition
                        != StageResultProgressionReferenceDisposition.Present
                    || ProgressionNodeDisposition
                        != StageResultProgressionReferenceDisposition.Present
                    || ProgressionGraphDisposition
                        != StageResultProgressionReferenceDisposition.Present
                    || RewardPlanDisposition
                        != StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema
                    || ResultDefinition == null
                    || ProgressionNode == null
                    || ProgressionGraph == null
                    || !ResultDefinition.TryValidateIntegrity(out error)
                    || !ProgressionNode.TryValidateIntegrity(out error)
                    || !ProgressionGraph.TryValidateIntegrity(out error)
                    || !ProgressionGraph.TryGetNode(
                        ProgressionNode.ProgressionNodeId,
                        ProgressionNode.Revision,
                        out StageProgressionNodeSnapshot graphNode)
                    || !ReferenceEquals(graphNode, ProgressionNode)
                    || !string.Equals(
                        CanonicalDigest,
                        RecomputeCanonicalDigest(),
                        StringComparison.Ordinal))
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        error = "Deep-copied stage result/progression join is damaged.";
                    }

                    return false;
                }

                error = string.Empty;
                return true;
            }
            catch (Exception)
            {
                error = "Deep-copied stage result/progression join contains damaged nested data.";
                return false;
            }
        }

        internal string RecomputeCanonicalDigest()
        {
            StringBuilder builder = new(4096);
            StageCanonicalDigest.Append(builder, "resultProgressionJoin.present", true);
            StageCanonicalDigest.Append(builder, "resultProgressionJoin.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(builder, "resultProgressionJoin.revision", Revision);
            StageCanonicalDigest.Append(builder, "resultProgressionJoin.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "resultProgressionJoin.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.canonicalRouteDigest",
                CanonicalRouteDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.referenceSchemaVersion",
                ReferenceSchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.referenceRevision",
                ReferenceRevision);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.canonicalReferenceDigest",
                CanonicalReferenceDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.briefingSchemaVersion",
                BriefingSchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.briefingRevision",
                BriefingRevision);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.canonicalBriefingDigest",
                CanonicalBriefingDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.semanticCoupling",
                (int)SemanticCoupling);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.resultDefinitionDisposition",
                (int)ResultDefinitionDisposition);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.resultDefinitionSchemaVersion",
                ResultDefinition?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.resultDefinitionId",
                ResultDefinition?.ResultDefinitionId);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.resultDefinitionRevision",
                ResultDefinition?.Revision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.evaluationContentRevision",
                ResultDefinition?.EvaluationContentRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.evaluationContentDigest",
                ResultDefinition?.EvaluationContentDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.presentationBindingRevision",
                ResultDefinition?.PresentationSource?.PresentationBindingRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.presentationBindingDigest",
                ResultDefinition?.PresentationSource?.PresentationBindingDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.presentationSourceSchemaVersion",
                ResultDefinition?.PresentationSource?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.presentationSourceDigest",
                ResultDefinition?.PresentationSource?.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeDisposition",
                (int)ProgressionNodeDisposition);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeSchemaVersion",
                ProgressionNode?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeId",
                ProgressionNode?.ProgressionNodeId);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeRevision",
                ProgressionNode?.Revision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeContentRevision",
                ProgressionNode?.ContentRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeContentDigest",
                ProgressionNode?.ContentDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeBindingRevision",
                ProgressionNode?.BindingRevision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionNodeBindingDigest",
                ProgressionNode?.BindingDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionGraphDisposition",
                (int)ProgressionGraphDisposition);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionGraphSchemaVersion",
                ProgressionGraph?.SchemaVersion ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionGraphId",
                ProgressionGraph?.ProgressionGraphId);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionGraphRevision",
                ProgressionGraph?.Revision ?? 0);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.progressionGraphDigest",
                ProgressionGraph?.CanonicalDigest);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.rewardPlanDisposition",
                (int)RewardPlanDisposition);
            StageCanonicalDigest.Append(
                builder,
                "resultProgressionJoin.rewardPlanDigest",
                RewardPlanDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageResultPresentationAuditEnvelope
    {
        private StageResultPresentationAuditEnvelope(
            StageRunResultSummary summary,
            StageRunResultProgressionJoinSnapshot join,
            StageResultPresentationSnapshot presentation)
        {
            SchemaVersion = 1;
            SourceResultSummaryId = summary.ResultSummaryId;
            SourceResultSummaryDigest = summary.ResultSummaryDigest;
            PlayableStageId = summary.Identity.PlayableStageId;
            RouteRevision = summary.Identity.RouteRevision;
            CanonicalRouteDigest = summary.Identity.RouteSnapshotDigest;
            JoinSnapshotSchemaVersion = join.SchemaVersion;
            JoinSnapshotDigest = join.CanonicalDigest;
            PresentationSourceSchemaVersion = join.PresentationSource.SchemaVersion;
            PresentationSourceDigest = join.PresentationSource.CanonicalDigest;
            Outcome = summary.Outcome;
            LocaleId = presentation.LocaleId;
            RenderedPresentationSnapshotDigest = presentation.CanonicalDigest;
            CanonicalDigest = RecomputeCanonicalDigest();
        }

        public int SchemaVersion { get; }
        public string SourceResultSummaryId { get; }
        public string SourceResultSummaryDigest { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string CanonicalRouteDigest { get; }
        public int JoinSnapshotSchemaVersion { get; }
        public string JoinSnapshotDigest { get; }
        public int PresentationSourceSchemaVersion { get; }
        public string PresentationSourceDigest { get; }
        public StageRouteOutcome Outcome { get; }
        public string LocaleId { get; }
        public string RenderedPresentationSnapshotDigest { get; }
        public string CanonicalDigest { get; }

        internal static bool TryCreate(
            StageRunResultSummary summary,
            StageRunResultProgressionJoinSnapshot join,
            StageResultPresentationSnapshot presentation,
            out StageResultPresentationAuditEnvelope audit,
            out string error)
        {
            audit = null;
            error = string.Empty;
            try
            {
                if (summary == null
                    || summary.Identity == null
                    || join == null
                    || presentation == null
                    || !join.TryValidateIntegrity(out error)
                    || !string.Equals(
                        summary.ResultSummaryId,
                        presentation.SourceResultSummaryId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        summary.ResultSummaryDigest,
                        presentation.SourceResultSummaryDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        summary.Identity.PlayableStageId,
                        join.PlayableStageId,
                        StringComparison.Ordinal)
                    || summary.Identity.RouteRevision != join.RouteRevision
                    || !string.Equals(
                        summary.Identity.RouteSnapshotDigest,
                        join.CanonicalRouteDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        presentation.CanonicalDigest,
                        presentation.RecomputeCanonicalDigest(),
                        StringComparison.Ordinal))
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        error = "Stage result presentation audit inputs are inconsistent.";
                    }

                    return false;
                }

                audit = new StageResultPresentationAuditEnvelope(summary, join, presentation);
                error = string.Empty;
                return true;
            }
            catch (Exception)
            {
                error = "Stage result presentation audit inputs contain damaged nested data.";
                return false;
            }
        }

        public bool TryValidate(
            StageRunResultSummary summary,
            StageRunResultProgressionJoinSnapshot join,
            StageResultPresentationSnapshot presentation,
            out string error)
        {
            error = string.Empty;
            try
            {
                if (!string.Equals(
                        CanonicalDigest,
                        RecomputeCanonicalDigest(),
                        StringComparison.Ordinal)
                    || !TryCreate(
                        summary,
                        join,
                        presentation,
                        out StageResultPresentationAuditEnvelope exact,
                        out error)
                    || !string.Equals(CanonicalDigest, exact.CanonicalDigest, StringComparison.Ordinal))
                {
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        error = "Stage result presentation audit envelope is damaged.";
                    }

                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                error = "Stage result presentation audit envelope contains damaged nested data.";
                return false;
            }
        }

        private string RecomputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            StageCanonicalDigest.Append(builder, "presentationAudit.schemaVersion", SchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.sourceResultSummaryId",
                SourceResultSummaryId);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.sourceResultSummaryDigest",
                SourceResultSummaryDigest);
            StageCanonicalDigest.Append(builder, "presentationAudit.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "presentationAudit.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.canonicalRouteDigest",
                CanonicalRouteDigest);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.joinSnapshotSchemaVersion",
                JoinSnapshotSchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.joinSnapshotDigest",
                JoinSnapshotDigest);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.presentationSourceSchemaVersion",
                PresentationSourceSchemaVersion);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.presentationSourceDigest",
                PresentationSourceDigest);
            StageCanonicalDigest.Append(builder, "presentationAudit.outcome", (int)Outcome);
            StageCanonicalDigest.Append(builder, "presentationAudit.localeId", LocaleId);
            StageCanonicalDigest.Append(
                builder,
                "presentationAudit.renderedPresentationSnapshotDigest",
                RenderedPresentationSnapshotDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }
}
