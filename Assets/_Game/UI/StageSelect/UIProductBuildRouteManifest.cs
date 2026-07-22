using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DimensionBrawl.LevelDesign;

namespace DimensionBrawl.UI
{
    public enum UIProductBuildSceneSourceKind
    {
        None = 0,
        UiRoute = 1,
        StageRouteSegment = 2,
        StageClear = 3
    }

    public enum UIProductBuildManifestRejectReason
    {
        None = 0,
        MissingRouteTable = 1,
        EmptyRouteTable = 2,
        InvalidUiRouteId = 3,
        DuplicateUiRouteId = 4,
        MissingUiRouteScenePath = 5,
        InvalidUiRouteScenePath = 6,
        DuplicateUiRouteScenePath = 7,
        MissingStageCatalog = 8,
        InvalidStageCatalogIdentity = 9,
        MissingPlayableStage = 10,
        MissingCanonicalProjectionDigest = 11,
        InvalidCanonicalProjection = 12,
        CanonicalProjectionDigestMismatch = 13,
        InvalidStageRoute = 14,
        InvalidResultProgressionJoin = 15,
        MissingPlayableStageId = 16,
        DuplicatePlayableStageId = 17,
        MissingResultDefinitionId = 18,
        DuplicateResultDefinitionId = 19,
        MissingProgressionNodeId = 20,
        DuplicateProgressionNodeId = 21,
        MissingStageRouteSegmentScenePath = 22,
        InvalidStageRouteSegmentScenePath = 23,
        MissingStageClearScenePath = 24,
        InvalidStageClearScenePath = 25,
        EmptyBuildSceneManifest = 26,
        DamagedSourceData = 27
    }

    public sealed class UIProductBuildUiRoute
    {
        internal UIProductBuildUiRoute(
            int authoredIndex,
            UIRouteId routeId,
            string authoredSceneName,
            string scenePath)
        {
            AuthoredIndex = authoredIndex;
            RouteId = routeId;
            AuthoredSceneName = authoredSceneName ?? string.Empty;
            ScenePath = scenePath ?? string.Empty;
            SceneName = ResolveSceneName(ScenePath);
        }

        public int AuthoredIndex { get; }
        public UIRouteId RouteId { get; }
        public string AuthoredSceneName { get; }
        public string ScenePath { get; }
        public string SceneName { get; }

        private static string ResolveSceneName(string scenePath)
        {
            return Path.GetFileNameWithoutExtension(scenePath) ?? string.Empty;
        }
    }

    public sealed class UIProductBuildCatalogEntry
    {
        internal UIProductBuildCatalogEntry(
            int authoredIndex,
            string catalogEntryId,
            string playableStageId,
            string resultDefinitionId,
            string progressionNodeId,
            string canonicalRouteDigest,
            string canonicalProjectionDigest)
        {
            AuthoredIndex = authoredIndex;
            CatalogEntryId = catalogEntryId ?? string.Empty;
            PlayableStageId = playableStageId ?? string.Empty;
            ResultDefinitionId = resultDefinitionId ?? string.Empty;
            ProgressionNodeId = progressionNodeId ?? string.Empty;
            CanonicalRouteDigest = canonicalRouteDigest ?? string.Empty;
            CanonicalProjectionDigest = canonicalProjectionDigest ?? string.Empty;
        }

        public int AuthoredIndex { get; }
        public string CatalogEntryId { get; }
        public string PlayableStageId { get; }
        public string ResultDefinitionId { get; }
        public string ProgressionNodeId { get; }
        public string CanonicalRouteDigest { get; }
        public string CanonicalProjectionDigest { get; }
    }

    public sealed class UIProductBuildRouteSegment
    {
        internal UIProductBuildRouteSegment(
            int catalogIndex,
            int segmentIndex,
            string catalogEntryId,
            string playableStageId,
            string canonicalRouteDigest,
            string resultDefinitionId,
            string progressionNodeId,
            string segmentId,
            int sequenceIndex,
            string stageDefinitionId,
            string scenePath)
        {
            CatalogIndex = catalogIndex;
            SegmentIndex = segmentIndex;
            CatalogEntryId = catalogEntryId ?? string.Empty;
            PlayableStageId = playableStageId ?? string.Empty;
            CanonicalRouteDigest = canonicalRouteDigest ?? string.Empty;
            ResultDefinitionId = resultDefinitionId ?? string.Empty;
            ProgressionNodeId = progressionNodeId ?? string.Empty;
            SegmentId = segmentId ?? string.Empty;
            SequenceIndex = sequenceIndex;
            StageDefinitionId = stageDefinitionId ?? string.Empty;
            ScenePath = scenePath ?? string.Empty;
            SceneName = Path.GetFileNameWithoutExtension(ScenePath) ?? string.Empty;
        }

        public int CatalogIndex { get; }
        public int SegmentIndex { get; }
        public string CatalogEntryId { get; }
        public string PlayableStageId { get; }
        public string CanonicalRouteDigest { get; }
        public string ResultDefinitionId { get; }
        public string ProgressionNodeId { get; }
        public string SegmentId { get; }
        public int SequenceIndex { get; }
        public string StageDefinitionId { get; }
        public string ScenePath { get; }
        public string SceneName { get; }
    }

    public sealed class UIProductBuildScene
    {
        internal UIProductBuildScene(
            int buildIndex,
            UIProductBuildSceneSourceKind sourceKind,
            UIRouteId uiRouteId,
            int catalogIndex,
            int segmentIndex,
            string catalogEntryId,
            string playableStageId,
            string scenePath)
        {
            BuildIndex = buildIndex;
            SourceKind = sourceKind;
            UiRouteId = uiRouteId;
            CatalogIndex = catalogIndex;
            SegmentIndex = segmentIndex;
            CatalogEntryId = catalogEntryId ?? string.Empty;
            PlayableStageId = playableStageId ?? string.Empty;
            ScenePath = scenePath ?? string.Empty;
            SceneName = Path.GetFileNameWithoutExtension(ScenePath) ?? string.Empty;
        }

        public int BuildIndex { get; }
        public UIProductBuildSceneSourceKind SourceKind { get; }
        public UIRouteId UiRouteId { get; }
        public int CatalogIndex { get; }
        public int SegmentIndex { get; }
        public string CatalogEntryId { get; }
        public string PlayableStageId { get; }
        public string ScenePath { get; }
        public string SceneName { get; }
    }

    public sealed class UIProductBuildRouteManifest
    {
        public const int SupportedSchemaVersion = 1;

        private readonly UIProductBuildUiRoute[] uiRoutes;
        private readonly UIProductBuildCatalogEntry[] catalogEntries;
        private readonly UIProductBuildRouteSegment[] routeSegments;
        private readonly UIProductBuildScene[] scenes;

        private UIProductBuildRouteManifest(
            UIProductBuildUiRoute[] uiRoutes,
            UIProductBuildCatalogEntry[] catalogEntries,
            UIProductBuildRouteSegment[] routeSegments,
            UIProductBuildScene[] scenes,
            string canonicalDigest)
        {
            this.uiRoutes = (UIProductBuildUiRoute[])uiRoutes.Clone();
            this.catalogEntries = (UIProductBuildCatalogEntry[])catalogEntries.Clone();
            this.routeSegments = (UIProductBuildRouteSegment[])routeSegments.Clone();
            this.scenes = (UIProductBuildScene[])scenes.Clone();
            CanonicalDigest = canonicalDigest ?? string.Empty;
        }

        public int SchemaVersion => SupportedSchemaVersion;
        public int UiRouteCount => uiRoutes.Length;
        public int CatalogEntryCount => catalogEntries.Length;
        public int RouteSegmentCount => routeSegments.Length;
        public int SceneCount => scenes.Length;
        public string CanonicalDigest { get; }

        public UIProductBuildUiRoute GetUiRoute(int index)
        {
            if (index < 0 || index >= uiRoutes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return uiRoutes[index];
        }

        public UIProductBuildCatalogEntry GetCatalogEntry(int index)
        {
            if (index < 0 || index >= catalogEntries.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return catalogEntries[index];
        }

        public UIProductBuildRouteSegment GetRouteSegment(int index)
        {
            if (index < 0 || index >= routeSegments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return routeSegments[index];
        }

        public UIProductBuildScene GetScene(int index)
        {
            if (index < 0 || index >= scenes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return scenes[index];
        }

        public static bool TryCreate(
            UIScreenRouteTable routeTable,
            UIStageCatalog stageCatalog,
            string stageClearScenePath,
            out UIProductBuildRouteManifest manifest,
            out UIProductBuildManifestRejectReason rejectReason,
            out string error)
        {
            try
            {
                return TryCreateCore(
                    routeTable,
                    stageCatalog,
                    stageClearScenePath,
                    out manifest,
                    out rejectReason,
                    out error);
            }
            catch (Exception exception)
            {
                manifest = null;
                rejectReason = UIProductBuildManifestRejectReason.DamagedSourceData;
                error = $"Product build route source data is damaged: {exception.Message}";
                return false;
            }
        }

        private static bool TryCreateCore(
            UIScreenRouteTable routeTable,
            UIStageCatalog stageCatalog,
            string stageClearScenePath,
            out UIProductBuildRouteManifest manifest,
            out UIProductBuildManifestRejectReason rejectReason,
            out string error)
        {
            manifest = null;
            rejectReason = UIProductBuildManifestRejectReason.None;
            error = string.Empty;

            if (routeTable == null)
            {
                return Reject(
                    UIProductBuildManifestRejectReason.MissingRouteTable,
                    "UI screen route table is missing.",
                    out manifest,
                    out rejectReason,
                    out error);
            }

            if (routeTable.RouteCount < 1)
            {
                return Reject(
                    UIProductBuildManifestRejectReason.EmptyRouteTable,
                    "UI screen route table contains no routes.",
                    out manifest,
                    out rejectReason,
                    out error);
            }

            var uiRoutes = new List<UIProductBuildUiRoute>(routeTable.RouteCount);
            var catalogEntries = new List<UIProductBuildCatalogEntry>();
            var routeSegments = new List<UIProductBuildRouteSegment>();
            var scenes = new List<UIProductBuildScene>();
            var physicalScenePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var uiRouteIds = new HashSet<UIRouteId>();
            var uiRouteScenePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int routeIndex = 0; routeIndex < routeTable.RouteCount; routeIndex++)
            {
                UIScreenRouteTable.Route route = routeTable.GetRoute(routeIndex);
                if (route.RouteId == UIRouteId.None
                    || !Enum.IsDefined(typeof(UIRouteId), route.RouteId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.InvalidUiRouteId,
                        $"UI route at authored index {routeIndex} has an invalid route id.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!uiRouteIds.Add(route.RouteId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.DuplicateUiRouteId,
                        $"UI route id '{route.RouteId}' is duplicated.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (string.IsNullOrWhiteSpace(route.ScenePath))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.MissingUiRouteScenePath,
                        $"UI route '{route.RouteId}' has no scene path.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!TryNormalizeScenePath(route.ScenePath, out string normalizedScenePath))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.InvalidUiRouteScenePath,
                        $"UI route '{route.RouteId}' has invalid scene path '{route.ScenePath}'.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!uiRouteScenePaths.Add(normalizedScenePath))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.DuplicateUiRouteScenePath,
                        $"UI route scene path '{normalizedScenePath}' is duplicated.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                uiRoutes.Add(
                    new UIProductBuildUiRoute(
                        routeIndex,
                        route.RouteId,
                        route.SceneName,
                        normalizedScenePath));
                AddPhysicalScene(
                    scenes,
                    physicalScenePaths,
                    UIProductBuildSceneSourceKind.UiRoute,
                    route.RouteId,
                    -1,
                    -1,
                    string.Empty,
                    string.Empty,
                    normalizedScenePath);
            }

            if (stageCatalog == null)
            {
                return Reject(
                    UIProductBuildManifestRejectReason.MissingStageCatalog,
                    "UI stage catalog is missing.",
                    out manifest,
                    out rejectReason,
                    out error);
            }

            if (!stageCatalog.TryValidateEntryIdentities(
                    out UIStageRouteProjectionRejectReason catalogRejectReason))
            {
                return Reject(
                    UIProductBuildManifestRejectReason.InvalidStageCatalogIdentity,
                    $"UI stage catalog identity is invalid: {catalogRejectReason}.",
                    out manifest,
                    out rejectReason,
                    out error);
            }

            catalogEntries.Capacity = stageCatalog.StageCount;
            var playableStageIds = new HashSet<string>(StringComparer.Ordinal);
            var resultDefinitionIds = new HashSet<string>(StringComparer.Ordinal);
            var progressionNodeIds = new HashSet<string>(StringComparer.Ordinal);

            for (int catalogIndex = 0; catalogIndex < stageCatalog.StageCount; catalogIndex++)
            {
                UIStageCatalog.StageEntry entry = stageCatalog.GetStage(catalogIndex);
                PlayableStageDefinition playableStage = entry.PlayableStage;
                if (playableStage == null)
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.MissingPlayableStage,
                        $"Catalog entry '{entry.Id}' has no playable stage.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (string.IsNullOrWhiteSpace(entry.CanonicalProjectionDigest))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.MissingCanonicalProjectionDigest,
                        $"Catalog entry '{entry.Id}' has no stored canonical projection digest.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!stageCatalog.TryComputeCanonicalProjectionDigest(
                        catalogIndex,
                        UIRouteId.Combat,
                        out string computedProjectionDigest,
                        out UIStageRouteProjectionRejectReason projectionRejectReason))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.InvalidCanonicalProjection,
                        $"Catalog entry '{entry.Id}' projection is invalid: {projectionRejectReason}.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!string.Equals(
                        entry.CanonicalProjectionDigest,
                        computedProjectionDigest,
                        StringComparison.Ordinal))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.CanonicalProjectionDigestMismatch,
                        $"Catalog entry '{entry.Id}' stored projection digest is stale.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!StageRunRouteSnapshot.TryCreate(
                        playableStage,
                        out StageRunRouteSnapshot routeSnapshot,
                        out string routeError))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.InvalidStageRoute,
                        $"Catalog entry '{entry.Id}' route is invalid: {routeError}",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!StageRunResultProgressionJoinSnapshot.TryCreate(
                        playableStage,
                        out StageRunResultProgressionJoinSnapshot joinSnapshot,
                        out string joinError))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.InvalidResultProgressionJoin,
                        $"Catalog entry '{entry.Id}' result/progression join is invalid: {joinError}",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                string playableStageId = routeSnapshot.PlayableStageId;
                if (string.IsNullOrWhiteSpace(playableStageId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.MissingPlayableStageId,
                        $"Catalog entry '{entry.Id}' has no playable stage id.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!playableStageIds.Add(playableStageId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.DuplicatePlayableStageId,
                        $"Playable stage id '{playableStageId}' is duplicated by the catalog.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                string resultDefinitionId = joinSnapshot.ResultDefinition?.ResultDefinitionId;
                if (string.IsNullOrWhiteSpace(resultDefinitionId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.MissingResultDefinitionId,
                        $"Catalog entry '{entry.Id}' has no result definition id.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!resultDefinitionIds.Add(resultDefinitionId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.DuplicateResultDefinitionId,
                        $"Result definition id '{resultDefinitionId}' is duplicated by the catalog.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                string progressionNodeId = joinSnapshot.ProgressionNode?.ProgressionNodeId;
                if (string.IsNullOrWhiteSpace(progressionNodeId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.MissingProgressionNodeId,
                        $"Catalog entry '{entry.Id}' has no progression node id.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                if (!progressionNodeIds.Add(progressionNodeId))
                {
                    return Reject(
                        UIProductBuildManifestRejectReason.DuplicateProgressionNodeId,
                        $"Progression node id '{progressionNodeId}' is duplicated by the catalog.",
                        out manifest,
                        out rejectReason,
                        out error);
                }

                var catalogEntry = new UIProductBuildCatalogEntry(
                    catalogIndex,
                    entry.Id,
                    playableStageId,
                    resultDefinitionId,
                    progressionNodeId,
                    routeSnapshot.CanonicalDigest,
                    computedProjectionDigest);
                catalogEntries.Add(catalogEntry);

                for (int segmentIndex = 0;
                    segmentIndex < routeSnapshot.SegmentCount;
                    segmentIndex++)
                {
                    StageRunSegmentSnapshot segment = routeSnapshot.GetSegment(segmentIndex);
                    if (segment == null || string.IsNullOrWhiteSpace(segment.ScenePath))
                    {
                        return Reject(
                            UIProductBuildManifestRejectReason.MissingStageRouteSegmentScenePath,
                            $"Catalog entry '{entry.Id}' segment {segmentIndex} has no scene path.",
                            out manifest,
                            out rejectReason,
                            out error);
                    }

                    if (!TryNormalizeScenePath(segment.ScenePath, out string segmentScenePath))
                    {
                        return Reject(
                            UIProductBuildManifestRejectReason.InvalidStageRouteSegmentScenePath,
                            $"Catalog entry '{entry.Id}' segment {segmentIndex} has invalid scene path '{segment.ScenePath}'.",
                            out manifest,
                            out rejectReason,
                            out error);
                    }

                    routeSegments.Add(
                        new UIProductBuildRouteSegment(
                            catalogIndex,
                            segmentIndex,
                            entry.Id,
                            playableStageId,
                            routeSnapshot.CanonicalDigest,
                            resultDefinitionId,
                            progressionNodeId,
                            segment.SegmentId,
                            segment.SequenceIndex,
                            segment.StageDefinitionId,
                            segmentScenePath));
                    AddPhysicalScene(
                        scenes,
                        physicalScenePaths,
                        UIProductBuildSceneSourceKind.StageRouteSegment,
                        UIRouteId.None,
                        catalogIndex,
                        segmentIndex,
                        entry.Id,
                        playableStageId,
                        segmentScenePath);
                }
            }

            if (string.IsNullOrWhiteSpace(stageClearScenePath))
            {
                return Reject(
                    UIProductBuildManifestRejectReason.MissingStageClearScenePath,
                    "Stage Clear scene path is missing.",
                    out manifest,
                    out rejectReason,
                    out error);
            }

            if (!TryNormalizeScenePath(stageClearScenePath, out string normalizedStageClearPath))
            {
                return Reject(
                    UIProductBuildManifestRejectReason.InvalidStageClearScenePath,
                    $"Stage Clear scene path '{stageClearScenePath}' is invalid.",
                    out manifest,
                    out rejectReason,
                    out error);
            }

            AddPhysicalScene(
                scenes,
                physicalScenePaths,
                UIProductBuildSceneSourceKind.StageClear,
                UIRouteId.None,
                -1,
                -1,
                string.Empty,
                string.Empty,
                normalizedStageClearPath);

            if (scenes.Count < 1)
            {
                return Reject(
                    UIProductBuildManifestRejectReason.EmptyBuildSceneManifest,
                    "Product build route manifest contains no physical scenes.",
                    out manifest,
                    out rejectReason,
                    out error);
            }

            UIProductBuildUiRoute[] uiRouteArray = uiRoutes.ToArray();
            UIProductBuildCatalogEntry[] catalogEntryArray = catalogEntries.ToArray();
            UIProductBuildRouteSegment[] routeSegmentArray = routeSegments.ToArray();
            UIProductBuildScene[] sceneArray = scenes.ToArray();
            string canonicalDigest = ComputeCanonicalDigest(
                uiRouteArray,
                catalogEntryArray,
                routeSegmentArray,
                sceneArray,
                normalizedStageClearPath);
            manifest = new UIProductBuildRouteManifest(
                uiRouteArray,
                catalogEntryArray,
                routeSegmentArray,
                sceneArray,
                canonicalDigest);
            rejectReason = UIProductBuildManifestRejectReason.None;
            error = string.Empty;
            return true;
        }

        private static void AddPhysicalScene(
            List<UIProductBuildScene> scenes,
            HashSet<string> physicalScenePaths,
            UIProductBuildSceneSourceKind sourceKind,
            UIRouteId uiRouteId,
            int catalogIndex,
            int segmentIndex,
            string catalogEntryId,
            string playableStageId,
            string scenePath)
        {
            if (!physicalScenePaths.Add(scenePath))
            {
                return;
            }

            scenes.Add(
                new UIProductBuildScene(
                    scenes.Count,
                    sourceKind,
                    uiRouteId,
                    catalogIndex,
                    segmentIndex,
                    catalogEntryId,
                    playableStageId,
                    scenePath));
        }

        private static bool TryNormalizeScenePath(string source, out string normalized)
        {
            normalized = string.IsNullOrWhiteSpace(source)
                ? string.Empty
                : source.Trim().Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(normalized)
                || !normalized.StartsWith("Assets/", StringComparison.Ordinal)
                || !normalized.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("//")
                || normalized.Contains("/../")
                || normalized.Contains("/./"))
            {
                normalized = string.Empty;
                return false;
            }

            return true;
        }

        private static string ComputeCanonicalDigest(
            UIProductBuildUiRoute[] uiRoutes,
            UIProductBuildCatalogEntry[] catalogEntries,
            UIProductBuildRouteSegment[] routeSegments,
            UIProductBuildScene[] scenes,
            string stageClearScenePath)
        {
            var builder = new StringBuilder(8192);
            Append(builder, "manifest.schemaVersion", SupportedSchemaVersion);
            Append(builder, "manifest.uiRouteCount", uiRoutes.Length);
            for (int i = 0; i < uiRoutes.Length; i++)
            {
                UIProductBuildUiRoute route = uiRoutes[i];
                string prefix = $"manifest.uiRoute[{i}]";
                Append(builder, prefix + ".authoredIndex", route.AuthoredIndex);
                Append(builder, prefix + ".routeId", (int)route.RouteId);
                Append(builder, prefix + ".authoredSceneName", route.AuthoredSceneName);
                Append(builder, prefix + ".scenePath", route.ScenePath);
            }

            Append(builder, "manifest.catalogEntryCount", catalogEntries.Length);
            for (int i = 0; i < catalogEntries.Length; i++)
            {
                UIProductBuildCatalogEntry entry = catalogEntries[i];
                string prefix = $"manifest.catalogEntry[{i}]";
                Append(builder, prefix + ".authoredIndex", entry.AuthoredIndex);
                Append(builder, prefix + ".catalogEntryId", entry.CatalogEntryId);
                Append(builder, prefix + ".playableStageId", entry.PlayableStageId);
                Append(builder, prefix + ".resultDefinitionId", entry.ResultDefinitionId);
                Append(builder, prefix + ".progressionNodeId", entry.ProgressionNodeId);
                Append(builder, prefix + ".canonicalRouteDigest", entry.CanonicalRouteDigest);
                Append(
                    builder,
                    prefix + ".canonicalProjectionDigest",
                    entry.CanonicalProjectionDigest);
            }

            Append(builder, "manifest.routeSegmentCount", routeSegments.Length);
            for (int i = 0; i < routeSegments.Length; i++)
            {
                UIProductBuildRouteSegment segment = routeSegments[i];
                string prefix = $"manifest.routeSegment[{i}]";
                Append(builder, prefix + ".catalogIndex", segment.CatalogIndex);
                Append(builder, prefix + ".segmentIndex", segment.SegmentIndex);
                Append(builder, prefix + ".catalogEntryId", segment.CatalogEntryId);
                Append(builder, prefix + ".playableStageId", segment.PlayableStageId);
                Append(builder, prefix + ".canonicalRouteDigest", segment.CanonicalRouteDigest);
                Append(builder, prefix + ".resultDefinitionId", segment.ResultDefinitionId);
                Append(builder, prefix + ".progressionNodeId", segment.ProgressionNodeId);
                Append(builder, prefix + ".segmentId", segment.SegmentId);
                Append(builder, prefix + ".sequenceIndex", segment.SequenceIndex);
                Append(builder, prefix + ".stageDefinitionId", segment.StageDefinitionId);
                Append(builder, prefix + ".scenePath", segment.ScenePath);
            }

            Append(builder, "manifest.stageClearScenePath", stageClearScenePath);
            Append(builder, "manifest.sceneCount", scenes.Length);
            for (int i = 0; i < scenes.Length; i++)
            {
                UIProductBuildScene scene = scenes[i];
                string prefix = $"manifest.scene[{i}]";
                Append(builder, prefix + ".buildIndex", scene.BuildIndex);
                Append(builder, prefix + ".sourceKind", (int)scene.SourceKind);
                Append(builder, prefix + ".uiRouteId", (int)scene.UiRouteId);
                Append(builder, prefix + ".catalogIndex", scene.CatalogIndex);
                Append(builder, prefix + ".segmentIndex", scene.SegmentIndex);
                Append(builder, prefix + ".catalogEntryId", scene.CatalogEntryId);
                Append(builder, prefix + ".playableStageId", scene.PlayableStageId);
                Append(builder, prefix + ".scenePath", scene.ScenePath);
            }

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

        private static void Append(StringBuilder builder, string key, string value)
        {
            string safeValue = value ?? string.Empty;
            builder.Append(key);
            builder.Append('=');
            builder.Append(safeValue.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(safeValue);
            builder.Append('\n');
        }

        private static void Append(StringBuilder builder, string key, int value)
        {
            Append(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        private static bool Reject(
            UIProductBuildManifestRejectReason reason,
            string message,
            out UIProductBuildRouteManifest manifest,
            out UIProductBuildManifestRejectReason rejectReason,
            out string error)
        {
            manifest = null;
            rejectReason = reason;
            error = message ?? string.Empty;
            return false;
        }
    }
}
