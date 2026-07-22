using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DimensionBrawl.LevelDesign;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.UI
{
    public enum UIStageRouteProjectionRejectReason
    {
        None = 0,
        MissingCatalogEntryId = 1,
        MissingPlayableStage = 2,
        UnsupportedRouteSchema = 3,
        MissingPlayableStageId = 4,
        InvalidRouteRevision = 5,
        MissingCanonicalRouteDigest = 6,
        CanonicalRouteDigestMismatch = 7,
        MissingEntrySegment = 8,
        InvalidEntrySequence = 9,
        MissingEntryStageDefinition = 10,
        MissingEntryStageDefinitionId = 11,
        MissingEntryScenePath = 12,
        MissingLoadingCardId = 13,
        UnsupportedProjectionSchema = 14,
        InvalidCatalogProjectionGeneration = 15,
        InvalidCatalogEntryCount = 16,
        CatalogEntryNotFound = 17,
        DuplicateCatalogEntryId = 18,
        MissingCanonicalProjectionDigest = 19,
        CanonicalProjectionDigestMismatch = 20,
        UnsupportedUiRoute = 21,
        MissingPresentationProvenance = 22,
        UnverifiedRewardPreview = 23,
        EntrySceneNotInBuildSettings = 24,
        StaleProjectionGeneration = 25,
        StaleProjectionBundle = 26,
        SourceObjectMismatch = 27,
        MissingStageReferenceBlock = 28,
        UnsupportedStageReferenceSchema = 29,
        InvalidStageReferenceRevision = 30,
        MissingStageTemplate = 31,
        UnsupportedStageTemplateSchema = 32,
        InvalidStageTemplateRevision = 33,
        MissingCanonicalStageTemplateDigest = 34,
        CanonicalStageTemplateDigestMismatch = 35,
        InvalidStageTemplateContract = 36,
        StageTemplateRouteMismatch = 37,
        InvalidStageTemplatePocketContract = 38,
        MissingCanonicalStageReferenceDigest = 39,
        CanonicalStageReferenceDigestMismatch = 40,
        InvalidStageReferenceContract = 41,
        InvalidStoryReferenceContract = 42,
        UnsupportedStageBriefingSchema = 43,
        InvalidStageBriefingRevision = 44,
        MissingCanonicalStageBriefingDigest = 45,
        InvalidActiveRunRestartPolicy = 46,
        InvalidStageBriefingActionContract = 47,
        CanonicalStageBriefingDigestMismatch = 48,
        PresentationMirrorMismatch = 49,
        StaleBriefingBundle = 50,
        InvalidResultProgressionJoin = 51,
        StaleResultProgressionPreflight = 52,
        InvalidStageSelectionBindings = 53
    }

    public enum UIStagePresentationProvenance
    {
        None = 0,
        LegacyPresentationOnly = 1
    }

    public sealed class UIStageRouteProjection
    {
        internal UIStageRouteProjection(
            int projectionSchemaVersion,
            int catalogProjectionGeneration,
            string catalogEntryId,
            PlayableStageDefinition playableStage,
            int routeSchemaVersion,
            string playableStageId,
            int routeRevision,
            string storedCanonicalRouteDigest,
            string recomputedCanonicalRouteDigest,
            string entrySegmentId,
            int entrySequenceIndex,
            StageDefinitionProfile entryStageDefinition,
            string entryStageDefinitionId,
            string entryScenePath,
            string entrySceneName,
            string loadingCardId,
            UIRouteId uiRouteId,
            string canonicalProjectionDigest,
            LinearStageTemplateProfile stageTemplate,
            string canonicalReferenceDigest,
            string canonicalTemplateDigest,
            string canonicalBriefingDigest,
            StageBriefingReadModel briefing,
            StageRunResultProgressionJoinSnapshot resultProgressionJoinPreflight,
            UIStagePresentationProvenance presentationProvenance,
            string displayName,
            string summary,
            string threatTags,
            string recommendedSummonRole,
            string rewardPreview)
        {
            ProjectionSchemaVersion = projectionSchemaVersion;
            CatalogProjectionGeneration = catalogProjectionGeneration;
            CatalogEntryId = catalogEntryId;
            PlayableStage = playableStage;
            RouteSchemaVersion = routeSchemaVersion;
            PlayableStageId = playableStageId;
            RouteRevision = routeRevision;
            StoredCanonicalRouteDigest = storedCanonicalRouteDigest;
            RecomputedCanonicalRouteDigest = recomputedCanonicalRouteDigest;
            EntrySegmentId = entrySegmentId;
            EntrySequenceIndex = entrySequenceIndex;
            EntryStageDefinition = entryStageDefinition;
            EntryStageDefinitionId = entryStageDefinitionId;
            EntryScenePath = entryScenePath;
            EntrySceneName = entrySceneName;
            LoadingCardId = loadingCardId;
            UiRouteId = uiRouteId;
            CanonicalProjectionDigest = canonicalProjectionDigest;
            StageTemplate = stageTemplate;
            CanonicalReferenceDigest = canonicalReferenceDigest;
            CanonicalTemplateDigest = canonicalTemplateDigest;
            CanonicalBriefingDigest = canonicalBriefingDigest;
            Briefing = briefing;
            ResultProgressionJoinPreflight = resultProgressionJoinPreflight;
            PresentationProvenance = presentationProvenance;
            DisplayName = displayName;
            Summary = summary;
            ThreatTags = threatTags;
            RecommendedSummonRole = recommendedSummonRole;
            RewardPreview = rewardPreview;
        }

        public int ProjectionSchemaVersion { get; }
        public int CatalogProjectionGeneration { get; }
        public string CatalogEntryId { get; }
        public PlayableStageDefinition PlayableStage { get; }
        public int RouteSchemaVersion { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string StoredCanonicalRouteDigest { get; }
        public string RecomputedCanonicalRouteDigest { get; }
        public string CanonicalRouteDigest => StoredCanonicalRouteDigest;
        public string EntrySegmentId { get; }
        public int EntrySequenceIndex { get; }
        public StageDefinitionProfile EntryStageDefinition { get; }
        public string EntryStageDefinitionId { get; }
        public string EntryScenePath { get; }
        public string EntrySceneName { get; }
        public string LoadingCardId { get; }
        public UIRouteId UiRouteId { get; }
        public string CanonicalProjectionDigest { get; }
        public LinearStageTemplateProfile StageTemplate { get; }
        public string CanonicalReferenceDigest { get; }
        public string CanonicalTemplateDigest { get; }
        public string CanonicalBriefingDigest { get; }
        public StageBriefingReadModel Briefing { get; }
        public StageRunResultProgressionJoinSnapshot ResultProgressionJoinPreflight { get; }
        public UIStagePresentationProvenance PresentationProvenance { get; }
        public string DisplayName { get; }
        public string Summary { get; }
        public string ThreatTags { get; }
        public string RecommendedSummonRole { get; }
        public string RewardPreview { get; }
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Stage Catalog")]
    public sealed class UIStageCatalog : ScriptableObject
    {
        public const int SupportedProjectionSchemaVersion = 1;
        public const int InitialCatalogProjectionGeneration = 1;

        [Serializable]
        public struct StageEntry
        {
            [SerializeField] private string id;
            [SerializeField] private string displayName;
            [SerializeField, TextArea] private string summary;
            [SerializeField] private string threatTags;
            [SerializeField] private string recommendedSummonRole;
            [SerializeField] private string mockRewardPreview;
            [SerializeField] private UIStagePresentationProvenance presentationProvenance;
            [SerializeField] private PlayableStageDefinition playableStage;
            [SerializeField] private string loadingCardId;
            [SerializeField] private string canonicalProjectionDigest;

            public string Id => id;
            public string DisplayName => displayName;
            public string Summary => summary;
            public string ThreatTags => threatTags;
            public string RecommendedSummonRole => recommendedSummonRole;
            public string MockRewardPreview => mockRewardPreview;
            public UIStagePresentationProvenance PresentationProvenance => presentationProvenance;
            public PlayableStageDefinition PlayableStage => playableStage;
            public string LoadingCardId => loadingCardId;
            public string CanonicalProjectionDigest => canonicalProjectionDigest;
        }

        private sealed class ProjectionData
        {
            public int ProjectionSchemaVersion;
            public int CatalogProjectionGeneration;
            public string CatalogEntryId;
            public PlayableStageDefinition PlayableStage;
            public int RouteSchemaVersion;
            public string PlayableStageId;
            public int RouteRevision;
            public string StoredCanonicalRouteDigest;
            public string RecomputedCanonicalRouteDigest;
            public string EntrySegmentId;
            public int EntrySequenceIndex;
            public StageDefinitionProfile EntryStageDefinition;
            public string EntryStageDefinitionId;
            public string EntryScenePath;
            public string EntrySceneName;
            public string LoadingCardId;
            public UIRouteId UiRouteId;
            public string CanonicalProjectionDigest;
            public LinearStageTemplateProfile StageTemplate;
            public string CanonicalReferenceDigest;
            public string CanonicalTemplateDigest;
            public string CanonicalBriefingDigest;
            public StageBriefingReadModel Briefing;
            public StageRunResultProgressionJoinSnapshot ResultProgressionJoinPreflight;
            public UIStagePresentationProvenance PresentationProvenance;
            public string DisplayName;
            public string Summary;
            public string ThreatTags;
            public string RecommendedSummonRole;
            public string RewardPreview;
        }

        [SerializeField] private int projectionSchemaVersion = SupportedProjectionSchemaVersion;
        [SerializeField, Min(1)] private int catalogProjectionGeneration =
            InitialCatalogProjectionGeneration;
        [SerializeField] private StageEntry[] stages = Array.Empty<StageEntry>();

        public int ProjectionSchemaVersion => projectionSchemaVersion;
        public int CatalogProjectionGeneration => catalogProjectionGeneration;
        public int StageCount => stages != null ? stages.Length : 0;

        public StageEntry GetStage(int index)
        {
            if (stages == null || index < 0 || index >= stages.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return stages[index];
        }

        public bool TryGetStage(string id, out StageEntry stage)
        {
            if (string.IsNullOrWhiteSpace(id)
                || !TryValidateEntryIdentities(out _))
            {
                stage = default;
                return false;
            }

            for (int i = 0; i < stages.Length; i++)
            {
                if (string.Equals(stages[i].Id, id, StringComparison.Ordinal))
                {
                    stage = stages[i];
                    return true;
                }
            }

            stage = default;
            return false;
        }

        public bool TryGetFirstStage(out StageEntry stage)
        {
            if (TryValidateEntryIdentities(out _))
            {
                stage = stages[0];
                return true;
            }

            stage = default;
            return false;
        }

        public bool TryValidateEntryIdentities(
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            if (StageCount < 1)
            {
                rejectReason = UIStageRouteProjectionRejectReason.InvalidCatalogEntryCount;
                return false;
            }

            for (int i = 0; i < stages.Length; i++)
            {
                string id = stages[i].Id;
                if (string.IsNullOrWhiteSpace(id))
                {
                    rejectReason = UIStageRouteProjectionRejectReason.MissingCatalogEntryId;
                    return false;
                }

                for (int previousIndex = 0; previousIndex < i; previousIndex++)
                {
                    if (string.Equals(stages[previousIndex].Id, id, StringComparison.Ordinal))
                    {
                        rejectReason = UIStageRouteProjectionRejectReason.DuplicateCatalogEntryId;
                        return false;
                    }
                }
            }

            rejectReason = UIStageRouteProjectionRejectReason.None;
            return true;
        }

        public bool TryCreateRouteProjection(
            string catalogEntryId,
            UIRouteId uiRouteId,
            out UIStageRouteProjection projection,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            projection = null;
            if (!TryValidateEntryIdentities(out rejectReason))
            {
                return false;
            }

            if (!TryFindUniqueStage(catalogEntryId, out StageEntry stage, out rejectReason))
            {
                return false;
            }

            return TryCreateRouteProjection(stage, uiRouteId, out projection, out rejectReason);
        }

        public bool TryCreateFirstRouteProjection(
            UIRouteId uiRouteId,
            out UIStageRouteProjection projection,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            projection = null;
            if (!TryValidateEntryIdentities(out rejectReason))
            {
                return false;
            }

            return TryCreateRouteProjection(stages[0], uiRouteId, out projection, out rejectReason);
        }

        public bool TryCreateRouteProjection(
            int index,
            UIRouteId uiRouteId,
            out UIStageRouteProjection projection,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            projection = null;
            if (!TryValidateEntryIdentities(out rejectReason))
            {
                return false;
            }

            if (index < 0 || index >= StageCount)
            {
                rejectReason = UIStageRouteProjectionRejectReason.CatalogEntryNotFound;
                return false;
            }

            return TryCreateRouteProjection(stages[index], uiRouteId, out projection, out rejectReason);
        }

        public bool TryComputeCanonicalProjectionDigest(
            int index,
            UIRouteId uiRouteId,
            out string canonicalProjectionDigest,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            canonicalProjectionDigest = string.Empty;
            if (!TryValidateEntryIdentities(out rejectReason))
            {
                return false;
            }

            if (index < 0 || index >= StageCount)
            {
                rejectReason = UIStageRouteProjectionRejectReason.CatalogEntryNotFound;
                return false;
            }

            if (!TryBuildProjectionData(
                    stages[index],
                    uiRouteId,
                    false,
                    false,
                    out ProjectionData data,
                    out rejectReason))
            {
                return false;
            }

            canonicalProjectionDigest = data.CanonicalProjectionDigest;
            return true;
        }

        public bool IsProjectionCurrent(
            UIStageRouteProjection projection,
            UIRouteId uiRouteId,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            if (!TryValidateEntryIdentities(out rejectReason))
            {
                return false;
            }

            if (projection == null)
            {
                rejectReason = UIStageRouteProjectionRejectReason.CatalogEntryNotFound;
                return false;
            }

            if (projection.ProjectionSchemaVersion != SupportedProjectionSchemaVersion
                || projectionSchemaVersion != projection.ProjectionSchemaVersion)
            {
                rejectReason = UIStageRouteProjectionRejectReason.UnsupportedProjectionSchema;
                return false;
            }

            if (projection.CatalogProjectionGeneration != catalogProjectionGeneration)
            {
                rejectReason = UIStageRouteProjectionRejectReason.StaleProjectionGeneration;
                return false;
            }

            if (projection.UiRouteId != uiRouteId || uiRouteId != UIRouteId.Combat)
            {
                rejectReason = UIStageRouteProjectionRejectReason.UnsupportedUiRoute;
                return false;
            }

            if (!TryFindUniqueStage(
                    projection.CatalogEntryId,
                    out StageEntry stage,
                    out rejectReason))
            {
                return false;
            }

            if (!ReferenceEquals(stage.PlayableStage, projection.PlayableStage))
            {
                rejectReason = UIStageRouteProjectionRejectReason.SourceObjectMismatch;
                return false;
            }

            if (!TryBuildProjectionData(
                    stage,
                    uiRouteId,
                    true,
                    true,
                    out ProjectionData data,
                    out rejectReason))
            {
                return false;
            }

            if (projection.ResultProgressionJoinPreflight == null
                || data.ResultProgressionJoinPreflight == null
                || !projection.ResultProgressionJoinPreflight.TryValidateIntegrity(out _)
                || !string.Equals(
                    projection.ResultProgressionJoinPreflight.CanonicalDigest,
                    data.ResultProgressionJoinPreflight.CanonicalDigest,
                    StringComparison.Ordinal))
            {
                rejectReason = UIStageRouteProjectionRejectReason.StaleResultProgressionPreflight;
                return false;
            }

            if (!ProjectionMatchesData(projection, data))
            {
                rejectReason = UIStageRouteProjectionRejectReason.SourceObjectMismatch;
                return false;
            }

            rejectReason = UIStageRouteProjectionRejectReason.None;
            return true;
        }

        private bool TryCreateRouteProjection(
            StageEntry stage,
            UIRouteId uiRouteId,
            out UIStageRouteProjection projection,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            projection = null;
            if (!TryBuildProjectionData(
                    stage,
                    uiRouteId,
                    true,
                    true,
                    out ProjectionData data,
                    out rejectReason))
            {
                return false;
            }

            projection = new UIStageRouteProjection(
                data.ProjectionSchemaVersion,
                data.CatalogProjectionGeneration,
                data.CatalogEntryId,
                data.PlayableStage,
                data.RouteSchemaVersion,
                data.PlayableStageId,
                data.RouteRevision,
                data.StoredCanonicalRouteDigest,
                data.RecomputedCanonicalRouteDigest,
                data.EntrySegmentId,
                data.EntrySequenceIndex,
                data.EntryStageDefinition,
                data.EntryStageDefinitionId,
                data.EntryScenePath,
                data.EntrySceneName,
                data.LoadingCardId,
                data.UiRouteId,
                data.CanonicalProjectionDigest,
                data.StageTemplate,
                data.CanonicalReferenceDigest,
                data.CanonicalTemplateDigest,
                data.CanonicalBriefingDigest,
                data.Briefing,
                data.ResultProgressionJoinPreflight,
                data.PresentationProvenance,
                data.DisplayName,
                data.Summary,
                data.ThreatTags,
                data.RecommendedSummonRole,
                data.RewardPreview);
            rejectReason = UIStageRouteProjectionRejectReason.None;
            return true;
        }

        private bool TryBuildProjectionData(
            StageEntry stage,
            UIRouteId uiRouteId,
            bool requireStoredProjectionDigest,
            bool requireEntrySceneInBuildSettings,
            out ProjectionData data,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            data = null;
            if (projectionSchemaVersion != SupportedProjectionSchemaVersion)
            {
                rejectReason = UIStageRouteProjectionRejectReason.UnsupportedProjectionSchema;
                return false;
            }

            if (catalogProjectionGeneration < InitialCatalogProjectionGeneration)
            {
                rejectReason = UIStageRouteProjectionRejectReason.InvalidCatalogProjectionGeneration;
                return false;
            }

            if (string.IsNullOrWhiteSpace(stage.Id))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingCatalogEntryId;
                return false;
            }

            PlayableStageDefinition playableStage = stage.PlayableStage;
            if (playableStage == null)
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingPlayableStage;
                return false;
            }

            if (playableStage.SchemaVersion != 1)
            {
                rejectReason = UIStageRouteProjectionRejectReason.UnsupportedRouteSchema;
                return false;
            }

            if (string.IsNullOrWhiteSpace(playableStage.PlayableStageId))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingPlayableStageId;
                return false;
            }

            if (playableStage.RouteRevision < 1)
            {
                rejectReason = UIStageRouteProjectionRejectReason.InvalidRouteRevision;
                return false;
            }

            string storedRouteDigest = playableStage.CanonicalRouteDigest;
            if (string.IsNullOrWhiteSpace(storedRouteDigest))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingCanonicalRouteDigest;
                return false;
            }

            string recomputedRouteDigest = playableStage.ComputeCanonicalRouteDigest();
            if (!string.Equals(storedRouteDigest, recomputedRouteDigest, StringComparison.Ordinal))
            {
                rejectReason = UIStageRouteProjectionRejectReason.CanonicalRouteDigestMismatch;
                return false;
            }

            if (!playableStage.TryCreateBriefingReadModel(
                    out StageBriefingReadModel briefing,
                    out StageBriefingBuildRejectReason briefingRejectReason))
            {
                rejectReason = MapBriefingRejectReason(briefingRejectReason);
                return false;
            }

            if (!StageRunResultProgressionJoinSnapshot.TryCreate(
                    playableStage,
                    out StageRunResultProgressionJoinSnapshot resultProgressionJoinPreflight,
                    out _))
            {
                rejectReason = UIStageRouteProjectionRejectReason.InvalidResultProgressionJoin;
                return false;
            }

            if (!string.Equals(stage.DisplayName, briefing.Title, StringComparison.Ordinal)
                || !string.Equals(stage.Summary, briefing.Objective, StringComparison.Ordinal))
            {
                rejectReason = UIStageRouteProjectionRejectReason.PresentationMirrorMismatch;
                return false;
            }

            if (playableStage.SceneSegmentCount < 1)
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingEntrySegment;
                return false;
            }

            StageSceneSegmentRef entrySegment = playableStage.GetSceneSegment(0);
            if (entrySegment == null || string.IsNullOrWhiteSpace(entrySegment.SegmentId))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingEntrySegment;
                return false;
            }

            int sequenceZeroCount = 0;
            for (int i = 0; i < playableStage.SceneSegmentCount; i++)
            {
                if (playableStage.GetSceneSegment(i)?.SequenceIndex == 0)
                {
                    sequenceZeroCount++;
                }
            }

            if (entrySegment.SequenceIndex != 0 || sequenceZeroCount != 1)
            {
                rejectReason = UIStageRouteProjectionRejectReason.InvalidEntrySequence;
                return false;
            }

            StageDefinitionProfile entryDefinition = entrySegment.StageDefinition;
            if (entryDefinition == null)
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingEntryStageDefinition;
                return false;
            }

            if (string.IsNullOrWhiteSpace(entryDefinition.StageId))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingEntryStageDefinitionId;
                return false;
            }

            string entryScenePath = NormalizeScenePath(entryDefinition.MapScenePath);
            if (string.IsNullOrWhiteSpace(entryScenePath))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingEntryScenePath;
                return false;
            }

            string entrySceneName = ResolveSceneName(entryScenePath);
            if (string.IsNullOrWhiteSpace(entrySceneName))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingEntryScenePath;
                return false;
            }

            if (requireEntrySceneInBuildSettings
                && SceneUtility.GetBuildIndexByScenePath(entryScenePath) < 0)
            {
                rejectReason = UIStageRouteProjectionRejectReason.EntrySceneNotInBuildSettings;
                return false;
            }

            if (string.IsNullOrWhiteSpace(stage.LoadingCardId))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingLoadingCardId;
                return false;
            }

            if (uiRouteId != UIRouteId.Combat)
            {
                rejectReason = UIStageRouteProjectionRejectReason.UnsupportedUiRoute;
                return false;
            }

            if (stage.PresentationProvenance != UIStagePresentationProvenance.LegacyPresentationOnly)
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingPresentationProvenance;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(stage.MockRewardPreview))
            {
                rejectReason = UIStageRouteProjectionRejectReason.UnverifiedRewardPreview;
                return false;
            }

            data = new ProjectionData
            {
                ProjectionSchemaVersion = projectionSchemaVersion,
                CatalogProjectionGeneration = catalogProjectionGeneration,
                CatalogEntryId = stage.Id,
                PlayableStage = playableStage,
                RouteSchemaVersion = playableStage.SchemaVersion,
                PlayableStageId = playableStage.PlayableStageId,
                RouteRevision = playableStage.RouteRevision,
                StoredCanonicalRouteDigest = storedRouteDigest,
                RecomputedCanonicalRouteDigest = recomputedRouteDigest,
                EntrySegmentId = entrySegment.SegmentId,
                EntrySequenceIndex = entrySegment.SequenceIndex,
                EntryStageDefinition = entryDefinition,
                EntryStageDefinitionId = entryDefinition.StageId,
                EntryScenePath = entryScenePath,
                EntrySceneName = entrySceneName,
                LoadingCardId = stage.LoadingCardId,
                UiRouteId = uiRouteId,
                StageTemplate = playableStage.ReferenceBlock.StageTemplate,
                CanonicalReferenceDigest = briefing.CanonicalReferenceDigest,
                CanonicalTemplateDigest = briefing.CanonicalTemplateDigest,
                CanonicalBriefingDigest = briefing.CanonicalBriefingDigest,
                Briefing = briefing,
                ResultProgressionJoinPreflight = resultProgressionJoinPreflight,
                PresentationProvenance = stage.PresentationProvenance,
                DisplayName = briefing.Title,
                Summary = briefing.Objective,
                ThreatTags = briefing.FeaturedThreatDisposition
                    == StageBriefingValueDisposition.Present
                    ? briefing.FeaturedThreat
                    : string.Empty,
                RecommendedSummonRole = briefing.RecommendedLoadoutDisposition
                    == StageBriefingValueDisposition.Present
                    ? briefing.RecommendedLoadout
                    : string.Empty,
                RewardPreview = briefing.RewardPreviewDisposition
                    == StageBriefingValueDisposition.Present
                    ? briefing.RewardPreview
                    : string.Empty
            };
            data.CanonicalProjectionDigest = ComputeCanonicalProjectionDigest(data);

            if (requireStoredProjectionDigest)
            {
                if (string.IsNullOrWhiteSpace(stage.CanonicalProjectionDigest))
                {
                    data = null;
                    rejectReason = UIStageRouteProjectionRejectReason.MissingCanonicalProjectionDigest;
                    return false;
                }

                if (!string.Equals(
                    stage.CanonicalProjectionDigest,
                    data.CanonicalProjectionDigest,
                    StringComparison.Ordinal))
                {
                    data = null;
                    rejectReason = UIStageRouteProjectionRejectReason.CanonicalProjectionDigestMismatch;
                    return false;
                }
            }

            rejectReason = UIStageRouteProjectionRejectReason.None;
            return true;
        }

        private bool TryFindUniqueStage(
            string id,
            out StageEntry stage,
            out UIStageRouteProjectionRejectReason rejectReason)
        {
            stage = default;
            if (string.IsNullOrWhiteSpace(id))
            {
                rejectReason = UIStageRouteProjectionRejectReason.MissingCatalogEntryId;
                return false;
            }

            int matchCount = 0;
            if (stages != null)
            {
                for (int i = 0; i < stages.Length; i++)
                {
                    if (string.Equals(stages[i].Id, id, StringComparison.Ordinal))
                    {
                        stage = stages[i];
                        matchCount++;
                    }
                }
            }

            if (matchCount == 0)
            {
                rejectReason = UIStageRouteProjectionRejectReason.CatalogEntryNotFound;
                return false;
            }

            if (matchCount != 1)
            {
                stage = default;
                rejectReason = UIStageRouteProjectionRejectReason.DuplicateCatalogEntryId;
                return false;
            }

            rejectReason = UIStageRouteProjectionRejectReason.None;
            return true;
        }

        private static bool ProjectionMatchesData(
            UIStageRouteProjection projection,
            ProjectionData data)
        {
            if (projection.ProjectionSchemaVersion != data.ProjectionSchemaVersion
                || projection.CatalogProjectionGeneration != data.CatalogProjectionGeneration
                || !string.Equals(projection.CatalogEntryId, data.CatalogEntryId, StringComparison.Ordinal)
                || !ReferenceEquals(projection.PlayableStage, data.PlayableStage)
                || projection.RouteSchemaVersion != data.RouteSchemaVersion
                || !string.Equals(projection.PlayableStageId, data.PlayableStageId, StringComparison.Ordinal)
                || projection.RouteRevision != data.RouteRevision
                || !string.Equals(
                    projection.StoredCanonicalRouteDigest,
                    data.StoredCanonicalRouteDigest,
                    StringComparison.Ordinal)
                || !string.Equals(
                    projection.RecomputedCanonicalRouteDigest,
                    data.RecomputedCanonicalRouteDigest,
                    StringComparison.Ordinal)
                || !string.Equals(projection.EntrySegmentId, data.EntrySegmentId, StringComparison.Ordinal)
                || projection.EntrySequenceIndex != data.EntrySequenceIndex
                || !ReferenceEquals(projection.EntryStageDefinition, data.EntryStageDefinition)
                || !string.Equals(
                    projection.EntryStageDefinitionId,
                    data.EntryStageDefinitionId,
                    StringComparison.Ordinal)
                || !string.Equals(projection.EntryScenePath, data.EntryScenePath, StringComparison.Ordinal)
                || !string.Equals(projection.EntrySceneName, data.EntrySceneName, StringComparison.Ordinal)
                || !string.Equals(projection.LoadingCardId, data.LoadingCardId, StringComparison.Ordinal)
                || projection.UiRouteId != data.UiRouteId
                || !string.Equals(
                    projection.CanonicalProjectionDigest,
                    data.CanonicalProjectionDigest,
                    StringComparison.Ordinal)
                || !ReferenceEquals(projection.StageTemplate, data.StageTemplate)
                || !string.Equals(
                    projection.CanonicalReferenceDigest,
                    data.CanonicalReferenceDigest,
                    StringComparison.Ordinal)
                || !string.Equals(
                    projection.CanonicalTemplateDigest,
                    data.CanonicalTemplateDigest,
                    StringComparison.Ordinal)
                || !string.Equals(
                    projection.CanonicalBriefingDigest,
                    data.CanonicalBriefingDigest,
                    StringComparison.Ordinal)
                || projection.Briefing == null
                || data.Briefing == null
                || !string.Equals(
                    projection.Briefing.CanonicalBriefingDigest,
                    data.Briefing.CanonicalBriefingDigest,
                    StringComparison.Ordinal)
                || projection.ResultProgressionJoinPreflight == null
                || data.ResultProgressionJoinPreflight == null
                || !projection.ResultProgressionJoinPreflight.TryValidateIntegrity(out _)
                || !data.ResultProgressionJoinPreflight.TryValidateIntegrity(out _)
                || !string.Equals(
                    projection.ResultProgressionJoinPreflight.CanonicalDigest,
                    data.ResultProgressionJoinPreflight.CanonicalDigest,
                    StringComparison.Ordinal)
                || !string.Equals(projection.DisplayName, data.DisplayName, StringComparison.Ordinal)
                || !string.Equals(projection.Summary, data.Summary, StringComparison.Ordinal)
                || !string.Equals(projection.ThreatTags, data.ThreatTags, StringComparison.Ordinal)
                || !string.Equals(
                    projection.RecommendedSummonRole,
                    data.RecommendedSummonRole,
                    StringComparison.Ordinal)
                || !string.Equals(
                    projection.RewardPreview,
                    data.RewardPreview,
                    StringComparison.Ordinal))
            {
                return false;
            }

            return string.Equals(
                projection.CanonicalProjectionDigest,
                ComputeCanonicalProjectionDigest(projection),
                StringComparison.Ordinal);
        }

        private static UIStageRouteProjectionRejectReason MapBriefingRejectReason(
            StageBriefingBuildRejectReason rejectReason)
        {
            return rejectReason switch
            {
                StageBriefingBuildRejectReason.MissingReferenceBlock =>
                    UIStageRouteProjectionRejectReason.MissingStageReferenceBlock,
                StageBriefingBuildRejectReason.UnsupportedReferenceSchema =>
                    UIStageRouteProjectionRejectReason.UnsupportedStageReferenceSchema,
                StageBriefingBuildRejectReason.InvalidReferenceRevision =>
                    UIStageRouteProjectionRejectReason.InvalidStageReferenceRevision,
                StageBriefingBuildRejectReason.MissingTemplate =>
                    UIStageRouteProjectionRejectReason.MissingStageTemplate,
                StageBriefingBuildRejectReason.UnsupportedTemplateSchema =>
                    UIStageRouteProjectionRejectReason.UnsupportedStageTemplateSchema,
                StageBriefingBuildRejectReason.InvalidTemplateRevision =>
                    UIStageRouteProjectionRejectReason.InvalidStageTemplateRevision,
                StageBriefingBuildRejectReason.MissingTemplateDigest =>
                    UIStageRouteProjectionRejectReason.MissingCanonicalStageTemplateDigest,
                StageBriefingBuildRejectReason.TemplateDigestMismatch =>
                    UIStageRouteProjectionRejectReason.CanonicalStageTemplateDigestMismatch,
                StageBriefingBuildRejectReason.InvalidTemplateValues =>
                    UIStageRouteProjectionRejectReason.InvalidStageTemplateContract,
                StageBriefingBuildRejectReason.RouteTemplateMismatch =>
                    UIStageRouteProjectionRejectReason.StageTemplateRouteMismatch,
                StageBriefingBuildRejectReason.InvalidPocketContract =>
                    UIStageRouteProjectionRejectReason.InvalidStageTemplatePocketContract,
                StageBriefingBuildRejectReason.MissingReferenceDigest =>
                    UIStageRouteProjectionRejectReason.MissingCanonicalStageReferenceDigest,
                StageBriefingBuildRejectReason.ReferenceDigestMismatch =>
                    UIStageRouteProjectionRejectReason.CanonicalStageReferenceDigestMismatch,
                StageBriefingBuildRejectReason.InvalidReferenceDisposition =>
                    UIStageRouteProjectionRejectReason.InvalidStageReferenceContract,
                StageBriefingBuildRejectReason.InvalidStoryContract =>
                    UIStageRouteProjectionRejectReason.InvalidStoryReferenceContract,
                StageBriefingBuildRejectReason.UnsupportedBriefingSchema =>
                    UIStageRouteProjectionRejectReason.UnsupportedStageBriefingSchema,
                StageBriefingBuildRejectReason.InvalidBriefingRevision =>
                    UIStageRouteProjectionRejectReason.InvalidStageBriefingRevision,
                StageBriefingBuildRejectReason.MissingBriefingDigest =>
                    UIStageRouteProjectionRejectReason.MissingCanonicalStageBriefingDigest,
                StageBriefingBuildRejectReason.InvalidActiveRunRestartPolicy =>
                    UIStageRouteProjectionRejectReason.InvalidActiveRunRestartPolicy,
                StageBriefingBuildRejectReason.InvalidActionContract =>
                    UIStageRouteProjectionRejectReason.InvalidStageBriefingActionContract,
                StageBriefingBuildRejectReason.BriefingDigestMismatch =>
                    UIStageRouteProjectionRejectReason.CanonicalStageBriefingDigestMismatch,
                _ => UIStageRouteProjectionRejectReason.SourceObjectMismatch
            };
        }

        private static string ComputeCanonicalProjectionDigest(ProjectionData data)
        {
            return ComputeCanonicalProjectionDigest(
                data.ProjectionSchemaVersion,
                data.CatalogProjectionGeneration,
                data.CatalogEntryId,
                data.RouteSchemaVersion,
                data.PlayableStageId,
                data.RouteRevision,
                data.StoredCanonicalRouteDigest,
                data.EntrySegmentId,
                data.EntrySequenceIndex,
                data.EntryStageDefinitionId,
                data.EntryScenePath,
                data.EntrySceneName,
                data.LoadingCardId,
                data.UiRouteId);
        }

        private static string ComputeCanonicalProjectionDigest(UIStageRouteProjection projection)
        {
            return ComputeCanonicalProjectionDigest(
                projection.ProjectionSchemaVersion,
                projection.CatalogProjectionGeneration,
                projection.CatalogEntryId,
                projection.RouteSchemaVersion,
                projection.PlayableStageId,
                projection.RouteRevision,
                projection.StoredCanonicalRouteDigest,
                projection.EntrySegmentId,
                projection.EntrySequenceIndex,
                projection.EntryStageDefinitionId,
                projection.EntryScenePath,
                projection.EntrySceneName,
                projection.LoadingCardId,
                projection.UiRouteId);
        }

        private static string ComputeCanonicalProjectionDigest(
            int projectionVersion,
            int projectionGeneration,
            string catalogEntryId,
            int routeSchemaVersion,
            string playableStageId,
            int routeRevision,
            string canonicalRouteDigest,
            string entrySegmentId,
            int entrySequenceIndex,
            string entryStageDefinitionId,
            string entryScenePath,
            string entrySceneName,
            string loadingCardId,
            UIRouteId uiRouteId)
        {
            var builder = new StringBuilder(1024);
            AppendCanonicalField(builder, "projectionSchemaVersion", projectionVersion);
            AppendCanonicalField(builder, "catalogProjectionGeneration", projectionGeneration);
            AppendCanonicalField(builder, "catalogEntryId", catalogEntryId);
            AppendCanonicalField(builder, "routeSchemaVersion", routeSchemaVersion);
            AppendCanonicalField(builder, "playableStageId", playableStageId);
            AppendCanonicalField(builder, "routeRevision", routeRevision);
            AppendCanonicalField(builder, "canonicalRouteDigest", canonicalRouteDigest);
            AppendCanonicalField(builder, "entrySegmentId", entrySegmentId);
            AppendCanonicalField(builder, "entrySequenceIndex", entrySequenceIndex);
            AppendCanonicalField(builder, "entryStageDefinitionId", entryStageDefinitionId);
            AppendCanonicalField(builder, "entryScenePath", NormalizeScenePath(entryScenePath));
            AppendCanonicalField(builder, "entrySceneName", entrySceneName);
            AppendCanonicalField(builder, "loadingCardId", loadingCardId);
            AppendCanonicalField(builder, "uiRouteId", (int)uiRouteId);

            byte[] payload = Encoding.UTF8.GetBytes(builder.ToString());
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

        private static void AppendCanonicalField(StringBuilder builder, string key, int value)
        {
            AppendCanonicalField(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendCanonicalField(StringBuilder builder, string key, string value)
        {
            string safeValue = value ?? string.Empty;
            builder.Append(key);
            builder.Append('=');
            builder.Append(safeValue.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(safeValue);
            builder.Append('\n');
        }

        private static string NormalizeScenePath(string scenePath)
        {
            return string.IsNullOrWhiteSpace(scenePath)
                ? string.Empty
                : scenePath.Replace('\\', '/');
        }

        private static string ResolveSceneName(string scenePath)
        {
            return string.IsNullOrWhiteSpace(scenePath)
                ? string.Empty
                : Path.GetFileNameWithoutExtension(NormalizeScenePath(scenePath));
        }
    }
}
