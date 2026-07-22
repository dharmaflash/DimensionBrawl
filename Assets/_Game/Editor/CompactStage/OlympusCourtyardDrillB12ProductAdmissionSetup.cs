using System;
using System.IO;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Admits the already-validated B1-1 Courtyard Drill pack into the product catalog,
    /// Stage Select, and Build Settings as one rollback-safe B1-2 operation.
    /// </summary>
    public static class OlympusCourtyardDrillB12ProductAdmissionSetup
    {
        private const string StageCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string RouteTablePath =
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string StageSelectPrefabPath =
            "Assets/_Game/UI/StageSelect/PF_UI_StageSelectScreen.prefab";
        private const string EditorBuildSettingsPath =
            "ProjectSettings/EditorBuildSettings.asset";
        private const string AcceptedRoutePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";

        private const string AcceptedCatalogEntryId = "story_v1_training_route";
        private const string CandidateCatalogEntryId = "story_v1_courtyard_drill_route";
        private const string AcceptedPlayableStageId = "OLYMPUS-INVASION-01";
        private const string CandidatePlayableStageId = "OLYMPUS-COURTYARD-DRILL-01";
        private const string LoadingCardId = "stage_to_combat_mood_bridge";
        private const string CandidateDisplayName = "Olympus Courtyard Drill";
        private const string CandidateSummary =
            "Defeat the Courtyard terminal boss under Rifle Crossfire pressure.";

        private const int BaselineCatalogGeneration = 2;
        private const int ProductCatalogGeneration = 3;
        private const string BaselineAcceptedProjectionDigest =
            "571b79d2fb47619383be714f88870752c4f8e1ce4d2864d6dc846307aecb6f1d";
        private const string ProductAcceptedProjectionDigest =
            "7bf7637516466673a3362b6caf761454632c6b1c7404d83d9c5e5ed2a6d59562";
        private const string ProductCandidateProjectionDigest =
            "588473db6022e05ccac3c8ebfe8c9cd5a5cf1ea50d1e02b5b6f4bce2e6594e34";

        private const string AcceptedRouteDigest =
            "878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0";
        private const string AcceptedPolicyDigest =
            "f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2";
        private const string AcceptedJoinDigest =
            "d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71";
        private const string CandidateRouteDigest =
            "c900c97a840057b48b09f2e555900769c47747903879845aba948d6a6c634dc2";
        private const string CandidatePolicyDigest =
            "8cf22bc4eb0da2c2adc0cf424ccce47ac810fbfd29cd9073bc9bd4621d8f33f4";
        private const string CandidateJoinDigest =
            "36eca65ccffef055b59c5c51547c053e5b368cbc7b0ba87a6330b646f461ee64";
        private const string ProductManifestDigest =
            "38ed64a5266b6d3e6c46755f5f138d54cddb3a684896eef0776ef4c4c3c966a5";

        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";

        private static readonly string[] ProductScenePaths =
        {
            CanonicalUiBuildSettings.LoginScenePath,
            CanonicalUiBuildSettings.LobbyScenePath,
            CanonicalUiBuildSettings.StageSelectScenePath,
            CorridorScenePath,
            OlympusCourtyardDrillAuthoredPackValidator.ScenePath,
            CanonicalUiBuildSettings.StageClearScenePath
        };

        [MenuItem("DimensionBrawl/B1-2/Apply Olympus Courtyard Drill Product Admission")]
        public static void ApplyMenu()
        {
            ApplyProductAdmission();
        }

        [MenuItem("DimensionBrawl/B1-2/Validate Olympus Courtyard Drill Product Admission")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
            LogPass("PRODUCT_ADMISSION_PASS");
        }

        public static void RunBatchSetup()
        {
            try
            {
                ApplyProductAdmission();
                LogPass("BATCH_PRODUCT_ADMISSION_SETUP_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[OlympusCourtyardDrillB12ProductAdmissionSetup] "
                    + "BATCH_PRODUCT_ADMISSION_SETUP_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchVerification()
        {
            try
            {
                ValidateOrThrow();
                LogPass("BATCH_PRODUCT_ADMISSION_VALIDATION_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[OlympusCourtyardDrillB12ProductAdmissionSetup] "
                    + "BATCH_PRODUCT_ADMISSION_VALIDATION_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void ApplyProductAdmission()
        {
            UIStageCatalog catalog = LoadRequired<UIStageCatalog>(StageCatalogPath);
            PlayableStageDefinition accepted = LoadRequired<PlayableStageDefinition>(
                AcceptedRoutePath);
            PlayableStageDefinition candidate = LoadRequired<PlayableStageDefinition>(
                OlympusCourtyardDrillAuthoredPackValidator.PlayableStagePath);
            ValidatePermittedStartingState(catalog, accepted, candidate);
            ValidateTransactionTargetsAreClean(catalog);

            FileSnapshot catalogSnapshot = FileSnapshot.Capture(StageCatalogPath);
            FileSnapshot prefabSnapshot = FileSnapshot.Capture(StageSelectPrefabPath);
            FileSnapshot buildSettingsSnapshot = FileSnapshot.Capture(EditorBuildSettingsPath);
            EditorBuildSettingsScene[] originalScenes = CloneBuildScenes(
                EditorBuildSettings.scenes);

            try
            {
                ConfigureCatalog(catalog, candidate);
                StageSelectMotionPrefabSetup.ApplyStageSelectMotion();
                UIV1BuildSettingsReadinessReporter.ApplyProductBuildSettings();
                ValidateOrThrow();
                AssetDatabase.SaveAssetIfDirty(catalog);
            }
            catch (Exception admissionException)
            {
                try
                {
                    RestoreTransaction(
                        catalogSnapshot,
                        prefabSnapshot,
                        buildSettingsSnapshot,
                        originalScenes);
                }
                catch (Exception rollbackException)
                {
                    throw new AggregateException(
                        "B1-2 product admission failed and its exact-file rollback also failed.",
                        admissionException,
                        rollbackException);
                }

                throw new InvalidOperationException(
                    "B1-2 product admission failed; catalog, Stage Select prefab, and "
                    + "Build Settings were restored from exact snapshots.",
                    admissionException);
            }

            LogPass("PRODUCT_ADMISSION_APPLY_PASS");
        }

        public static void ValidateOrThrow()
        {
            OlympusCourtyardDrillAuthoredPackValidator.ValidateOrThrow();

            UIStageCatalog catalog = LoadRequired<UIStageCatalog>(StageCatalogPath);
            PlayableStageDefinition accepted = LoadRequired<PlayableStageDefinition>(
                AcceptedRoutePath);
            PlayableStageDefinition candidate = LoadRequired<PlayableStageDefinition>(
                OlympusCourtyardDrillAuthoredPackValidator.PlayableStagePath);

            ValidateRouteIdentity(
                accepted,
                AcceptedPlayableStageId,
                AcceptedRouteDigest,
                AcceptedPolicyDigest,
                AcceptedJoinDigest,
                "accepted Olympus route",
                out StageRunRouteSnapshot acceptedRoute,
                out StageRunResultProgressionJoinSnapshot acceptedJoin);
            ValidateRouteIdentity(
                candidate,
                CandidatePlayableStageId,
                CandidateRouteDigest,
                CandidatePolicyDigest,
                CandidateJoinDigest,
                "Courtyard candidate route",
                out StageRunRouteSnapshot candidateRoute,
                out StageRunResultProgressionJoinSnapshot candidateJoin);

            ValidateProductCatalog(catalog, accepted, candidate, acceptedRoute, candidateRoute);
            UIProductBuildRouteManifest manifest = BuildManifestOrThrow(catalog);
            ValidateProductManifest(
                manifest,
                acceptedRoute,
                acceptedJoin,
                candidateRoute,
                candidateJoin);
            ValidateExactBuildSettings();

            PlayableStageDefinitionValidator.ValidateOrThrow();
            UIV1BuildSettingsReadinessReporter.ValidateCurrentReadinessOrThrow();
        }

        private static void ValidatePermittedStartingState(
            UIStageCatalog catalog,
            PlayableStageDefinition accepted,
            PlayableStageDefinition candidate)
        {
            Require(
                catalog.ProjectionSchemaVersion
                    == UIStageCatalog.SupportedProjectionSchemaVersion,
                $"{StageCatalogPath} must use projection schema "
                + $"{UIStageCatalog.SupportedProjectionSchemaVersion}.");
            Require(
                catalog.TryValidateEntryIdentities(
                    out UIStageRouteProjectionRejectReason identityReject),
                $"{StageCatalogPath} entry identities are invalid: {identityReject}.");

            bool isBaseline = catalog.CatalogProjectionGeneration
                    == BaselineCatalogGeneration
                && catalog.StageCount == 1
                && IsExactEntryIdentity(
                    catalog.GetStage(0),
                    AcceptedCatalogEntryId,
                    accepted)
                && string.Equals(
                    catalog.GetStage(0).CanonicalProjectionDigest,
                    BaselineAcceptedProjectionDigest,
                    StringComparison.Ordinal)
                && HasExactComputedProjectionDigest(
                    catalog,
                    0,
                    BaselineAcceptedProjectionDigest);

            bool isAlreadyAdmitted = catalog.CatalogProjectionGeneration
                    == ProductCatalogGeneration
                && catalog.StageCount == 2
                && IsExactEntryIdentity(
                    catalog.GetStage(0),
                    AcceptedCatalogEntryId,
                    accepted)
                && IsExactEntryIdentity(
                    catalog.GetStage(1),
                    CandidateCatalogEntryId,
                    candidate)
                && string.Equals(
                    catalog.GetStage(0).CanonicalProjectionDigest,
                    ProductAcceptedProjectionDigest,
                    StringComparison.Ordinal)
                && string.Equals(
                    catalog.GetStage(1).CanonicalProjectionDigest,
                    ProductCandidateProjectionDigest,
                    StringComparison.Ordinal)
                && HasExactComputedProjectionDigest(
                    catalog,
                    0,
                    ProductAcceptedProjectionDigest)
                && HasExactComputedProjectionDigest(
                    catalog,
                    1,
                    ProductCandidateProjectionDigest);

            Require(
                isBaseline || isAlreadyAdmitted,
                "B1-2 setup only accepts the exact generation-2 one-row baseline or "
                + "the exact generation-3 two-row admitted identity. Unknown rows, order, "
                + "route references, or generations will not be truncated or replaced.");
        }

        private static bool HasExactComputedProjectionDigest(
            UIStageCatalog catalog,
            int index,
            string expectedDigest)
        {
            return catalog.TryComputeCanonicalProjectionDigest(
                    index,
                    UIRouteId.Combat,
                    out string computedDigest,
                    out _)
                && string.Equals(
                    computedDigest,
                    expectedDigest,
                    StringComparison.Ordinal);
        }

        private static void ValidateTransactionTargetsAreClean(UIStageCatalog catalog)
        {
            GameObject stageSelectPrefab = LoadRequired<GameObject>(StageSelectPrefabPath);
            Require(
                !EditorUtility.IsDirty(catalog)
                    && !EditorUtility.IsDirty(stageSelectPrefab),
                "B1-2 product admission refuses to overwrite unsaved catalog or Stage Select "
                + "prefab changes. Save or revert those two target assets first.");
        }

        private static bool IsExactEntryIdentity(
            UIStageCatalog.StageEntry entry,
            string expectedId,
            PlayableStageDefinition expectedRoute)
        {
            return string.Equals(entry.Id, expectedId, StringComparison.Ordinal)
                && ReferenceEquals(entry.PlayableStage, expectedRoute)
                && string.Equals(
                    AssetDatabase.GetAssetPath(entry.PlayableStage),
                    AssetDatabase.GetAssetPath(expectedRoute),
                    StringComparison.Ordinal);
        }

        private static void ConfigureCatalog(
            UIStageCatalog catalog,
            PlayableStageDefinition candidate)
        {
            var serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.Update();

            SerializedProperty schema = RequireProperty(
                serializedCatalog,
                "projectionSchemaVersion");
            SerializedProperty generation = RequireProperty(
                serializedCatalog,
                "catalogProjectionGeneration");
            SerializedProperty stages = RequireProperty(serializedCatalog, "stages");
            Require(schema.intValue == UIStageCatalog.SupportedProjectionSchemaVersion,
                "Catalog projection schema changed before B1-2 mutation.");
            Require(stages.isArray && (stages.arraySize == 1 || stages.arraySize == 2),
                "Catalog stage rows changed before B1-2 mutation.");

            generation.intValue = ProductCatalogGeneration;
            stages.arraySize = 2;
            SerializedProperty candidateEntry = stages.GetArrayElementAtIndex(1);
            SetString(candidateEntry, "id", CandidateCatalogEntryId);
            SetString(candidateEntry, "displayName", CandidateDisplayName);
            SetString(candidateEntry, "summary", CandidateSummary);
            SetString(candidateEntry, "threatTags", string.Empty);
            SetString(candidateEntry, "recommendedSummonRole", string.Empty);
            SetString(candidateEntry, "mockRewardPreview", string.Empty);
            RequireRelative(candidateEntry, "presentationProvenance").enumValueIndex =
                (int)UIStagePresentationProvenance.LegacyPresentationOnly;
            RequireRelative(candidateEntry, "playableStage").objectReferenceValue = candidate;
            SetString(candidateEntry, "loadingCardId", LoadingCardId);

            SetString(
                stages.GetArrayElementAtIndex(0),
                "canonicalProjectionDigest",
                string.Empty);
            SetString(candidateEntry, "canonicalProjectionDigest", string.Empty);
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);

            string acceptedDigest = ComputeProjectionDigestOrThrow(catalog, 0);
            string candidateDigest = ComputeProjectionDigestOrThrow(catalog, 1);
            RequireEqual(
                acceptedDigest,
                ProductAcceptedProjectionDigest,
                "generation-3 accepted projection digest");
            RequireEqual(
                candidateDigest,
                ProductCandidateProjectionDigest,
                "generation-3 Courtyard projection digest");

            serializedCatalog.Update();
            stages = RequireProperty(serializedCatalog, "stages");
            SetString(
                stages.GetArrayElementAtIndex(0),
                "canonicalProjectionDigest",
                acceptedDigest);
            SetString(
                stages.GetArrayElementAtIndex(1),
                "canonicalProjectionDigest",
                candidateDigest);
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssetIfDirty(catalog);

            RequireEqual(
                catalog.GetStage(0).CanonicalProjectionDigest,
                ProductAcceptedProjectionDigest,
                "stored generation-3 accepted projection digest");
            RequireEqual(
                catalog.GetStage(1).CanonicalProjectionDigest,
                ProductCandidateProjectionDigest,
                "stored generation-3 Courtyard projection digest");
        }

        private static string ComputeProjectionDigestOrThrow(
            UIStageCatalog catalog,
            int index)
        {
            Require(
                catalog.TryComputeCanonicalProjectionDigest(
                    index,
                    UIRouteId.Combat,
                    out string digest,
                    out UIStageRouteProjectionRejectReason rejectReason),
                $"Catalog row {index} projection digest rejected {rejectReason}.");
            return digest;
        }

        private static void ValidateProductCatalog(
            UIStageCatalog catalog,
            PlayableStageDefinition accepted,
            PlayableStageDefinition candidate,
            StageRunRouteSnapshot acceptedRoute,
            StageRunRouteSnapshot candidateRoute)
        {
            Require(
                catalog.ProjectionSchemaVersion
                    == UIStageCatalog.SupportedProjectionSchemaVersion
                && catalog.CatalogProjectionGeneration == ProductCatalogGeneration
                && catalog.StageCount == 2,
                "B1-2 product catalog must be schema 1, generation 3, with exactly two rows.");
            Require(
                catalog.TryValidateEntryIdentities(
                    out UIStageRouteProjectionRejectReason identityReject),
                $"B1-2 product catalog identities rejected {identityReject}.");

            ValidateCatalogEntry(
                catalog,
                0,
                AcceptedCatalogEntryId,
                accepted,
                acceptedRoute,
                ProductAcceptedProjectionDigest,
                requireCandidatePresentation: false);
            ValidateCatalogEntry(
                catalog,
                1,
                CandidateCatalogEntryId,
                candidate,
                candidateRoute,
                ProductCandidateProjectionDigest,
                requireCandidatePresentation: true);
        }

        private static void ValidateCatalogEntry(
            UIStageCatalog catalog,
            int index,
            string expectedEntryId,
            PlayableStageDefinition expectedRoute,
            StageRunRouteSnapshot expectedRouteSnapshot,
            string expectedProjectionDigest,
            bool requireCandidatePresentation)
        {
            UIStageCatalog.StageEntry entry = catalog.GetStage(index);
            Require(
                IsExactEntryIdentity(entry, expectedEntryId, expectedRoute),
                $"Catalog row {index} identity or route reference is stale.");
            RequireEqual(
                entry.LoadingCardId,
                LoadingCardId,
                $"catalog row {index} loadingCardId");
            Require(
                entry.PresentationProvenance
                    == UIStagePresentationProvenance.LegacyPresentationOnly,
                $"Catalog row {index} presentation provenance must be LegacyPresentationOnly.");
            RequireEqual(
                entry.CanonicalProjectionDigest,
                expectedProjectionDigest,
                $"catalog row {index} stored projection digest");
            RequireEqual(
                ComputeProjectionDigestOrThrow(catalog, index),
                expectedProjectionDigest,
                $"catalog row {index} computed projection digest");

            Require(
                catalog.TryCreateRouteProjection(
                    index,
                    UIRouteId.Combat,
                    out UIStageRouteProjection projection,
                    out UIStageRouteProjectionRejectReason createReject),
                $"Catalog row {index} runtime projection rejected {createReject}.");
            Require(
                catalog.IsProjectionCurrent(
                    projection,
                    UIRouteId.Combat,
                    out UIStageRouteProjectionRejectReason currentReject),
                $"Catalog row {index} runtime projection is stale: {currentReject}.");
            Require(
                ReferenceEquals(projection.PlayableStage, expectedRoute)
                && projection.CatalogProjectionGeneration == ProductCatalogGeneration
                && projection.ProjectionSchemaVersion
                    == UIStageCatalog.SupportedProjectionSchemaVersion
                && projection.UiRouteId == UIRouteId.Combat,
                $"Catalog row {index} runtime projection ownership is stale.");
            RequireEqual(
                projection.CatalogEntryId,
                expectedEntryId,
                $"catalog row {index} projection catalogEntryId");
            RequireEqual(
                projection.PlayableStageId,
                expectedRouteSnapshot.PlayableStageId,
                $"catalog row {index} projection playableStageId");
            RequireEqual(
                projection.CanonicalRouteDigest,
                expectedRouteSnapshot.CanonicalDigest,
                $"catalog row {index} projection route digest");
            RequireEqual(
                projection.CanonicalProjectionDigest,
                expectedProjectionDigest,
                $"catalog row {index} projection digest");
            RequireEqual(
                projection.EntrySegmentId,
                expectedRouteSnapshot.GetSegment(0).SegmentId,
                $"catalog row {index} projection entry segment");
            RequireEqual(
                projection.EntryScenePath,
                expectedRouteSnapshot.GetSegment(0).ScenePath,
                $"catalog row {index} projection entry scene");

            if (!requireCandidatePresentation)
            {
                return;
            }

            RequireEqual(entry.DisplayName, CandidateDisplayName, "Courtyard displayName");
            RequireEqual(entry.Summary, CandidateSummary, "Courtyard summary");
            RequireEmpty(entry.ThreatTags, "Courtyard threatTags");
            RequireEmpty(entry.RecommendedSummonRole, "Courtyard recommendedSummonRole");
            RequireEmpty(entry.MockRewardPreview, "Courtyard mockRewardPreview");
            RequireEqual(projection.DisplayName, CandidateDisplayName,
                "Courtyard projection displayName");
            RequireEqual(projection.Summary, CandidateSummary,
                "Courtyard projection summary");
            RequireEmpty(projection.ThreatTags, "Courtyard projection threatTags");
            RequireEmpty(
                projection.RecommendedSummonRole,
                "Courtyard projection recommendedSummonRole");
            RequireEmpty(projection.RewardPreview, "Courtyard projection rewardPreview");
            Require(
                projection.Briefing != null,
                "Courtyard projection requires an immutable briefing read model.");
            RequireEqual(
                projection.Briefing.Title,
                CandidateDisplayName,
                "Courtyard briefing title");
            RequireEqual(
                projection.Briefing.Objective,
                CandidateSummary,
                "Courtyard briefing objective");
        }

        private static void ValidateRouteIdentity(
            PlayableStageDefinition route,
            string expectedPlayableStageId,
            string expectedRouteDigest,
            string expectedPolicyDigest,
            string expectedJoinDigest,
            string label,
            out StageRunRouteSnapshot routeSnapshot,
            out StageRunResultProgressionJoinSnapshot joinSnapshot)
        {
            Require(route != null, $"{label} is missing.");
            RequireEqual(route.PlayableStageId, expectedPlayableStageId,
                $"{label} playableStageId");
            RequireEqual(route.CanonicalRouteDigest, expectedRouteDigest,
                $"{label} stored route digest");
            Require(route.TerminalResolutionPolicy != null,
                $"{label} terminal policy is missing.");
            RequireEqual(
                route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                expectedPolicyDigest,
                $"{label} terminal policy digest");
            Require(route.ResultProgressionJoin != null,
                $"{label} result/progression join is missing.");
            RequireEqual(route.ResultProgressionJoin.CanonicalDigest, expectedJoinDigest,
                $"{label} stored result/progression join digest");

            Require(
                StageRunRouteSnapshot.TryCreate(
                    route,
                    out routeSnapshot,
                    out string routeError),
                $"{label} route snapshot rejected: {routeError}");
            RequireEqual(routeSnapshot.CanonicalDigest, expectedRouteDigest,
                $"{label} route snapshot digest");
            RequireEqual(routeSnapshot.TerminalPolicy.PolicyDigest, expectedPolicyDigest,
                $"{label} policy snapshot digest");

            Require(
                StageRunResultProgressionJoinSnapshot.TryCreate(
                    route,
                    out joinSnapshot,
                    out string joinError),
                $"{label} result/progression join snapshot rejected: {joinError}");
            RequireEqual(joinSnapshot.CanonicalDigest, expectedJoinDigest,
                $"{label} result/progression join snapshot digest");
        }

        private static UIProductBuildRouteManifest BuildManifestOrThrow(
            UIStageCatalog catalog)
        {
            UIScreenRouteTable routeTable = LoadRequired<UIScreenRouteTable>(RouteTablePath);
            Require(
                UIProductBuildRouteManifest.TryCreate(
                    routeTable,
                    catalog,
                    CanonicalUiBuildSettings.StageClearScenePath,
                    out UIProductBuildRouteManifest manifest,
                    out UIProductBuildManifestRejectReason rejectReason,
                    out string error),
                $"B1-2 product manifest rejected {rejectReason}: {error}");
            return manifest;
        }

        private static void ValidateProductManifest(
            UIProductBuildRouteManifest manifest,
            StageRunRouteSnapshot acceptedRoute,
            StageRunResultProgressionJoinSnapshot acceptedJoin,
            StageRunRouteSnapshot candidateRoute,
            StageRunResultProgressionJoinSnapshot candidateJoin)
        {
            Require(
                manifest.SchemaVersion == UIProductBuildRouteManifest.SupportedSchemaVersion
                && manifest.CatalogEntryCount == 2
                && manifest.RouteSegmentCount == 3
                && manifest.SceneCount == ProductScenePaths.Length,
                "B1-2 manifest must contain exactly 2 catalog rows, 3 logical route "
                + "segments, and 6 physical scenes.");
            RequireEqual(manifest.CanonicalDigest, ProductManifestDigest,
                "B1-2 product manifest digest");

            ValidateManifestCatalogEntry(
                manifest.GetCatalogEntry(0),
                0,
                AcceptedCatalogEntryId,
                acceptedRoute,
                acceptedJoin,
                ProductAcceptedProjectionDigest);
            ValidateManifestCatalogEntry(
                manifest.GetCatalogEntry(1),
                1,
                CandidateCatalogEntryId,
                candidateRoute,
                candidateJoin,
                ProductCandidateProjectionDigest);

            ValidateManifestSegment(
                manifest.GetRouteSegment(0),
                0,
                0,
                AcceptedCatalogEntryId,
                acceptedRoute,
                acceptedRoute.GetSegment(0));
            ValidateManifestSegment(
                manifest.GetRouteSegment(1),
                0,
                1,
                AcceptedCatalogEntryId,
                acceptedRoute,
                acceptedRoute.GetSegment(1));
            ValidateManifestSegment(
                manifest.GetRouteSegment(2),
                1,
                0,
                CandidateCatalogEntryId,
                candidateRoute,
                candidateRoute.GetSegment(0));

            for (int i = 0; i < ProductScenePaths.Length; i++)
            {
                UIProductBuildScene scene = manifest.GetScene(i);
                Require(scene.BuildIndex == i,
                    $"Manifest scene {i} build index must remain {i}.");
                RequireEqual(scene.ScenePath, ProductScenePaths[i],
                    $"manifest scene {i} path");
            }
        }

        private static void ValidateManifestCatalogEntry(
            UIProductBuildCatalogEntry entry,
            int expectedIndex,
            string expectedCatalogEntryId,
            StageRunRouteSnapshot route,
            StageRunResultProgressionJoinSnapshot join,
            string expectedProjectionDigest)
        {
            Require(entry.AuthoredIndex == expectedIndex,
                $"Manifest catalog row {expectedIndex} authored index is stale.");
            RequireEqual(entry.CatalogEntryId, expectedCatalogEntryId,
                $"manifest catalog row {expectedIndex} id");
            RequireEqual(entry.PlayableStageId, route.PlayableStageId,
                $"manifest catalog row {expectedIndex} playableStageId");
            RequireEqual(entry.CanonicalRouteDigest, route.CanonicalDigest,
                $"manifest catalog row {expectedIndex} route digest");
            RequireEqual(entry.CanonicalProjectionDigest, expectedProjectionDigest,
                $"manifest catalog row {expectedIndex} projection digest");
            RequireEqual(entry.ResultDefinitionId, join.ResultDefinition.ResultDefinitionId,
                $"manifest catalog row {expectedIndex} resultDefinitionId");
            RequireEqual(entry.ProgressionNodeId, join.ProgressionNode.ProgressionNodeId,
                $"manifest catalog row {expectedIndex} progressionNodeId");
        }

        private static void ValidateManifestSegment(
            UIProductBuildRouteSegment actual,
            int expectedCatalogIndex,
            int expectedSegmentIndex,
            string expectedCatalogEntryId,
            StageRunRouteSnapshot route,
            StageRunSegmentSnapshot expected)
        {
            Require(
                actual.CatalogIndex == expectedCatalogIndex
                && actual.SegmentIndex == expectedSegmentIndex
                && actual.SequenceIndex == expected.SequenceIndex,
                $"Manifest route segment {expectedCatalogIndex}:{expectedSegmentIndex} "
                + "index identity is stale.");
            RequireEqual(actual.CatalogEntryId, expectedCatalogEntryId,
                "manifest route segment catalogEntryId");
            RequireEqual(actual.PlayableStageId, route.PlayableStageId,
                "manifest route segment playableStageId");
            RequireEqual(actual.CanonicalRouteDigest, route.CanonicalDigest,
                "manifest route segment route digest");
            RequireEqual(actual.SegmentId, expected.SegmentId,
                "manifest route segment segmentId");
            RequireEqual(actual.StageDefinitionId, expected.StageDefinitionId,
                "manifest route segment stageDefinitionId");
            RequireEqual(actual.ScenePath, expected.ScenePath,
                "manifest route segment scenePath");
        }

        private static void ValidateExactBuildSettings()
        {
            EditorBuildSettingsScene[] scenes =
                EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            Require(
                scenes.Length == ProductScenePaths.Length,
                "Build Settings must contain exactly the six B1-2 product scenes, with "
                + "no disabled or duplicate extras.");
            for (int i = 0; i < ProductScenePaths.Length; i++)
            {
                Require(scenes[i].enabled,
                    $"Build Settings scene {i} must be enabled.");
                RequireEqual(NormalizePath(scenes[i].path), ProductScenePaths[i],
                    $"Build Settings scene {i} path");
            }
        }

        private static void RestoreTransaction(
            FileSnapshot catalogSnapshot,
            FileSnapshot prefabSnapshot,
            FileSnapshot buildSettingsSnapshot,
            EditorBuildSettingsScene[] originalScenes)
        {
            EditorBuildSettings.scenes = CloneBuildScenes(originalScenes);
            catalogSnapshot.Restore();
            prefabSnapshot.Restore();
            buildSettingsSnapshot.Restore();

            AssetDatabase.ImportAsset(
                StageCatalogPath,
                ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(
                StageSelectPrefabPath,
                ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            catalogSnapshot.RequireRestored();
            prefabSnapshot.RequireRestored();
            buildSettingsSnapshot.RequireRestored();
            RequireBuildScenesEqual(originalScenes, EditorBuildSettings.scenes);
        }

        private static EditorBuildSettingsScene[] CloneBuildScenes(
            EditorBuildSettingsScene[] source)
        {
            source ??= Array.Empty<EditorBuildSettingsScene>();
            var clone = new EditorBuildSettingsScene[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = new EditorBuildSettingsScene(source[i].path, source[i].enabled);
            }

            return clone;
        }

        private static void RequireBuildScenesEqual(
            EditorBuildSettingsScene[] expected,
            EditorBuildSettingsScene[] actual)
        {
            expected ??= Array.Empty<EditorBuildSettingsScene>();
            actual ??= Array.Empty<EditorBuildSettingsScene>();
            Require(expected.Length == actual.Length,
                "Rollback did not restore the exact Build Settings scene count.");
            for (int i = 0; i < expected.Length; i++)
            {
                Require(
                    expected[i].enabled == actual[i].enabled
                    && string.Equals(expected[i].path, actual[i].path,
                        StringComparison.Ordinal),
                    $"Rollback did not restore Build Settings scene {i} exactly.");
            }
        }

        private static SerializedProperty RequireProperty(
            SerializedObject serializedObject,
            string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Require(property != null,
                $"{serializedObject.targetObject.name} is missing serialized property "
                + $"'{propertyName}'.");
            return property;
        }

        private static SerializedProperty RequireRelative(
            SerializedProperty parent,
            string propertyName)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            Require(property != null,
                $"Serialized catalog entry is missing property '{propertyName}'.");
            return property;
        }

        private static void SetString(
            SerializedProperty parent,
            string propertyName,
            string value)
        {
            RequireRelative(parent, propertyName).stringValue = value ?? string.Empty;
        }

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null,
                $"B1-2 product admission requires {typeof(T).Name} at exact path '{path}'.");
            RequireEqual(AssetDatabase.GetAssetPath(asset), path,
                $"{typeof(T).Name} persistent asset path");
            return asset;
        }

        private static string ToAbsoluteProjectPath(string projectRelativePath)
        {
            DirectoryInfo projectDirectory = Directory.GetParent(Application.dataPath);
            Require(projectDirectory != null,
                "Could not resolve the Unity project directory for B1-2 snapshots.");
            return Path.GetFullPath(
                Path.Combine(
                    projectDirectory.FullName,
                    projectRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/');
        }

        private static void LogPass(string passKind)
        {
            Debug.Log(
                "[OlympusCourtyardDrillB12ProductAdmissionSetup] "
                + $"{passKind} catalogGeneration={ProductCatalogGeneration}, "
                + "catalogEntries=2, routeSegments=3, scenes=6, "
                + $"acceptedProjection={ProductAcceptedProjectionDigest}, "
                + $"candidateProjection={ProductCandidateProjectionDigest}, "
                + $"manifest={ProductManifestDigest}.");
        }

        private static void RequireEmpty(string value, string label)
        {
            Require(string.IsNullOrEmpty(value), $"{label} must be authored-empty.");
        }

        private static void RequireEqual(string actual, string expected, string label)
        {
            Require(
                string.Equals(actual, expected, StringComparison.Ordinal),
                $"{label} must be '{expected}', but was '{actual}'.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private readonly struct FileSnapshot
        {
            private FileSnapshot(
                string projectRelativePath,
                string absolutePath,
                byte[] bytes)
            {
                ProjectRelativePath = projectRelativePath;
                AbsolutePath = absolutePath;
                Bytes = bytes;
            }

            private string ProjectRelativePath { get; }
            private string AbsolutePath { get; }
            private byte[] Bytes { get; }

            public static FileSnapshot Capture(string projectRelativePath)
            {
                string absolutePath = ToAbsoluteProjectPath(projectRelativePath);
                Require(File.Exists(absolutePath),
                    $"B1-2 transaction snapshot source is missing: {projectRelativePath}.");
                return new FileSnapshot(
                    projectRelativePath,
                    absolutePath,
                    File.ReadAllBytes(absolutePath));
            }

            public void Restore()
            {
                File.WriteAllBytes(AbsolutePath, Bytes);
            }

            public void RequireRestored()
            {
                Require(File.Exists(AbsolutePath),
                    $"Rollback target disappeared: {ProjectRelativePath}.");
                byte[] restored = File.ReadAllBytes(AbsolutePath);
                Require(BytesEqual(Bytes, restored),
                    $"Rollback did not restore exact bytes for {ProjectRelativePath}.");
            }

            private static bool BytesEqual(byte[] left, byte[] right)
            {
                if (ReferenceEquals(left, right))
                {
                    return true;
                }

                if (left == null || right == null || left.Length != right.Length)
                {
                    return false;
                }

                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] != right[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
