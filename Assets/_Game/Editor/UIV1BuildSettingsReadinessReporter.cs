using System;
using System.Collections.Generic;
using System.Text;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class UIV1BuildSettingsReadinessReporter
    {
        private const string RouteTablePath =
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string StageCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";

        [MenuItem("DimensionBrawl/UI V1/Report Build Settings Readiness")]
        public static void ReportMenu()
        {
            ReportCurrentReadiness();
        }

        [MenuItem("DimensionBrawl/UI V1/Apply Product Build Settings")]
        public static void ApplyMenu()
        {
            ApplyProductBuildSettings();
        }

        public static void RunBatchVerification()
        {
            try
            {
                ValidateCurrentReadinessOrThrow();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ApplyProductBuildSettings()
        {
            UIProductBuildRouteManifest manifest = BuildManifestOrThrow(
                out UIStageCatalog stageCatalog);
            ValidateManifestSceneAssets(manifest);

            EditorBuildSettingsScene[] originalScenes =
                EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            EditorBuildSettingsScene[] candidateScenes =
                new EditorBuildSettingsScene[manifest.SceneCount];
            for (int i = 0; i < manifest.SceneCount; i++)
            {
                candidateScenes[i] = new EditorBuildSettingsScene(
                    manifest.GetScene(i).ScenePath,
                    true);
            }

            try
            {
                EditorBuildSettings.scenes = candidateScenes;
                ValidateExactEnabledOrder(manifest);
                ValidateRuntimeCatalogProjections(stageCatalog);
            }
            catch (Exception exception)
            {
                EditorBuildSettings.scenes = originalScenes;
                throw new InvalidOperationException(
                    "Product Build Settings apply failed and the previous scene list was restored.",
                    exception);
            }

            Debug.Log(
                "Product Build Settings applied transactionally. "
                + $"scenes={manifest.SceneCount}, catalogEntries={manifest.CatalogEntryCount}, "
                + $"routeSegments={manifest.RouteSegmentCount}, digest={manifest.CanonicalDigest}.");
        }

        public static void ValidateCurrentReadinessOrThrow()
        {
            UIProductBuildRouteManifest manifest = BuildManifestOrThrow(
                out UIStageCatalog stageCatalog);
            ValidateManifestSceneAssets(manifest);
            ValidateExactEnabledOrder(manifest);
            ValidateRuntimeCatalogProjections(stageCatalog);
            Debug.Log(
                "Product Build Settings readiness passed. "
                + $"scenes={manifest.SceneCount}, catalogEntries={manifest.CatalogEntryCount}, "
                + $"routeSegments={manifest.RouteSegmentCount}, digest={manifest.CanonicalDigest}.");
        }

        public static void ReportCurrentReadiness()
        {
            try
            {
                ValidateCurrentReadinessOrThrow();
            }
            catch (Exception exception)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("UI V1 Build Settings readiness failed.");
                builder.AppendLine("ProjectSettings were not modified by this report.");
                builder.Append(exception.Message);
                Debug.LogWarning(builder.ToString());
            }
        }

        private static UIProductBuildRouteManifest BuildManifestOrThrow(
            out UIStageCatalog stageCatalog)
        {
            UIScreenRouteTable routeTable =
                AssetDatabase.LoadAssetAtPath<UIScreenRouteTable>(RouteTablePath);
            if (routeTable == null)
            {
                throw new InvalidOperationException(
                    $"Product route table is missing at {RouteTablePath}.");
            }

            stageCatalog = AssetDatabase.LoadAssetAtPath<UIStageCatalog>(StageCatalogPath);
            if (stageCatalog == null)
            {
                throw new InvalidOperationException(
                    $"Product stage catalog is missing at {StageCatalogPath}.");
            }

            if (!UIProductBuildRouteManifest.TryCreate(
                    routeTable,
                    stageCatalog,
                    CanonicalUiBuildSettings.StageClearScenePath,
                    out UIProductBuildRouteManifest manifest,
                    out UIProductBuildManifestRejectReason rejectReason,
                    out string error))
            {
                throw new InvalidOperationException(
                    $"Product build route manifest rejected {rejectReason}: {error}");
            }

            return manifest;
        }

        private static void ValidateManifestSceneAssets(
            UIProductBuildRouteManifest manifest)
        {
            for (int i = 0; i < manifest.SceneCount; i++)
            {
                UIProductBuildScene scene = manifest.GetScene(i);
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.ScenePath) == null)
                {
                    throw new InvalidOperationException(
                        $"Product manifest scene {i} ({DescribeScene(scene)}) is missing: "
                        + scene.ScenePath);
                }
            }
        }

        private static void ValidateExactEnabledOrder(
            UIProductBuildRouteManifest manifest)
        {
            List<string> enabledScenes = CollectEnabledBuildScenes();
            if (enabledScenes.Count != manifest.SceneCount)
            {
                throw new InvalidOperationException(
                    "Enabled Build Settings scene count does not match the product manifest: "
                    + $"actual={enabledScenes.Count}, expected={manifest.SceneCount}.");
            }

            for (int i = 0; i < manifest.SceneCount; i++)
            {
                string expected = manifest.GetScene(i).ScenePath;
                if (!string.Equals(enabledScenes[i], expected, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Enabled Build Settings scene {i} is out of route order: "
                        + $"actual='{enabledScenes[i]}', expected='{expected}'.");
                }
            }
        }

        private static void ValidateRuntimeCatalogProjections(UIStageCatalog stageCatalog)
        {
            for (int i = 0; i < stageCatalog.StageCount; i++)
            {
                UIStageCatalog.StageEntry entry = stageCatalog.GetStage(i);
                if (!stageCatalog.TryCreateRouteProjection(
                        i,
                        UIRouteId.Combat,
                        out UIStageRouteProjection projection,
                        out UIStageRouteProjectionRejectReason createReject))
                {
                    throw new InvalidOperationException(
                        $"Catalog entry '{entry.Id}' runtime projection rejected {createReject}.");
                }

                if (!stageCatalog.IsProjectionCurrent(
                        projection,
                        UIRouteId.Combat,
                        out UIStageRouteProjectionRejectReason currentReject))
                {
                    throw new InvalidOperationException(
                        $"Catalog entry '{entry.Id}' runtime projection is stale: {currentReject}.");
                }
            }
        }

        private static List<string> CollectEnabledBuildScenes()
        {
            var scenePaths = new List<string>();
            EditorBuildSettingsScene[] scenes =
                EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && !string.IsNullOrWhiteSpace(scenes[i].path))
                {
                    scenePaths.Add(scenes[i].path.Replace('\\', '/'));
                }
            }

            return scenePaths;
        }

        private static string DescribeScene(UIProductBuildScene scene)
        {
            return scene.SourceKind switch
            {
                UIProductBuildSceneSourceKind.UiRoute => $"UI route {scene.UiRouteId}",
                UIProductBuildSceneSourceKind.StageRouteSegment =>
                    $"catalog {scene.CatalogEntryId} segment {scene.SegmentIndex}",
                UIProductBuildSceneSourceKind.StageClear => "Stage Clear",
                _ => "unknown source"
            };
        }
    }
}
