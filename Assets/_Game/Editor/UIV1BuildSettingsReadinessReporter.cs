using System.Collections.Generic;
using System.Text;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class UIV1BuildSettingsReadinessReporter
    {
        private const string RouteTablePath = "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string StageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string StationCombatScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";

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

        public static void ApplyProductBuildSettings()
        {
            UIScreenRouteTable routeTable = AssetDatabase.LoadAssetAtPath<UIScreenRouteTable>(RouteTablePath);
            if (routeTable == null)
            {
                throw new System.InvalidOperationException($"Product build settings apply could not find route table at {RouteTablePath}.");
            }

            List<RouteScene> routeScenes = CollectBuildRouteScenes(routeTable);
            if (routeScenes.Count == 0)
            {
                throw new System.InvalidOperationException("Product route must contain at least one buildable scene.");
            }

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[routeScenes.Count];
            for (int i = 0; i < routeScenes.Count; i++)
            {
                string scenePath = routeScenes[i].ScenePath;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    throw new System.InvalidOperationException($"Product route scene is missing: {scenePath}");
                }

                buildScenes[i] = new EditorBuildSettingsScene(scenePath, true);
            }

            EditorBuildSettings.scenes = buildScenes;
            AssetDatabase.SaveAssets();
            Debug.Log($"Product build settings applied. {routeScenes.Count} canonical scene(s) are enabled, starting at {routeScenes[0].ScenePath}.");
        }

        public static void ReportCurrentReadiness()
        {
            UIScreenRouteTable routeTable = AssetDatabase.LoadAssetAtPath<UIScreenRouteTable>(RouteTablePath);
            if (routeTable == null)
            {
                Debug.LogWarning($"UI V1 build settings readiness could not find route table at {RouteTablePath}.");
                return;
            }

            List<RouteScene> routeScenes = CollectBuildRouteScenes(routeTable);
            List<string> enabledBuildScenes = CollectEnabledBuildScenes();
            HashSet<string> enabledBuildSceneSet = new HashSet<string>(enabledBuildScenes);
            List<RouteScene> missingScenes = new List<RouteScene>();

            for (int i = 0; i < routeScenes.Count; i++)
            {
                if (!enabledBuildSceneSet.Contains(routeScenes[i].ScenePath))
                {
                    missingScenes.Add(routeScenes[i]);
                }
            }

            bool routeOrderMatches = enabledBuildScenes.Count == routeScenes.Count;
            if (routeOrderMatches)
            {
                for (int i = 0; i < routeScenes.Count; i++)
                {
                    if (!string.Equals(enabledBuildScenes[i], routeScenes[i].ScenePath, System.StringComparison.Ordinal))
                    {
                        routeOrderMatches = false;
                        break;
                    }
                }
            }

            if (missingScenes.Count == 0 && routeOrderMatches)
            {
                Debug.Log($"Product build settings readiness passed. {routeScenes.Count} canonical scene(s) are enabled in exact route order.");
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("UI V1 build settings readiness warning: ");
            builder.Append(missingScenes.Count);
            builder.Append('/');
            builder.Append(routeScenes.Count);
            builder.AppendLine(" canonical scene(s) are not enabled in Build Settings.");
            builder.AppendLine("ProjectSettings were not modified by this report. Apply Product Build Settings in a reviewed settings pass:");

            for (int i = 0; i < missingScenes.Count; i++)
            {
                builder.Append("- ");
                builder.Append(missingScenes[i].Label);
                builder.Append(": ");
                builder.AppendLine(missingScenes[i].ScenePath);
            }

            if (!routeOrderMatches)
            {
                builder.AppendLine("Enabled Build Settings scenes do not exactly match canonical product route order.");
            }

            Debug.LogWarning(builder.ToString());
        }

        private static List<RouteScene> CollectBuildRouteScenes(UIScreenRouteTable routeTable)
        {
            List<RouteScene> routeScenes = CollectRouteScenes(routeTable);
            UIStageCatalog stageCatalog = AssetDatabase.LoadAssetAtPath<UIStageCatalog>(StageCatalogPath);
            AddStageCatalogScenes(routeScenes, stageCatalog);
            AddUniqueRouteScene(
                routeScenes,
                "CombatContinuation",
                StationCombatScenePath);
            AddUniqueRouteScene(
                routeScenes,
                "StageClear",
                CanonicalUiBuildSettings.StageClearScenePath);
            return routeScenes;
        }

        private static List<RouteScene> CollectRouteScenes(UIScreenRouteTable routeTable)
        {
            SerializedObject serializedObject = new SerializedObject(routeTable);
            SerializedProperty routes = serializedObject.FindProperty("routes");
            List<RouteScene> routeScenes = new List<RouteScene>();

            if (routes == null || !routes.isArray)
            {
                return routeScenes;
            }

            for (int i = 0; i < routes.arraySize; i++)
            {
                SerializedProperty route = routes.GetArrayElementAtIndex(i);
                UIRouteId routeId = (UIRouteId)route.FindPropertyRelative("routeId").intValue;
                string scenePath = route.FindPropertyRelative("scenePath").stringValue;

                if (routeId != UIRouteId.None && !string.IsNullOrWhiteSpace(scenePath))
                {
                    AddUniqueRouteScene(routeScenes, routeId.ToString(), scenePath);
                }
            }

            return routeScenes;
        }

        private static void AddStageCatalogScenes(List<RouteScene> routeScenes, UIStageCatalog stageCatalog)
        {
            if (stageCatalog == null)
            {
                return;
            }

            for (int i = 0; i < stageCatalog.StageCount; i++)
            {
                if (stageCatalog.TryCreateRouteProjection(
                    i,
                    UIRouteId.Combat,
                    out UIStageRouteProjection projection,
                    out _))
                {
                    AddUniqueRouteScene(
                        routeScenes,
                        projection.UiRouteId.ToString(),
                        projection.EntryScenePath);
                }
            }
        }

        private static void AddUniqueRouteScene(List<RouteScene> routeScenes, string label, string scenePath)
        {
            string normalizedPath = scenePath.Replace('\\', '/');
            for (int i = 0; i < routeScenes.Count; i++)
            {
                if (string.Equals(routeScenes[i].ScenePath, normalizedPath, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            routeScenes.Add(new RouteScene(label, normalizedPath));
        }

        private static List<string> CollectEnabledBuildScenes()
        {
            List<string> scenePaths = new List<string>();
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && !string.IsNullOrWhiteSpace(scenes[i].path))
                {
                    scenePaths.Add(scenes[i].path.Replace('\\', '/'));
                }
            }

            return scenePaths;
        }

        private readonly struct RouteScene
        {
            public RouteScene(string label, string scenePath)
            {
                Label = label;
                ScenePath = scenePath;
            }

            public string Label { get; }
            public string ScenePath { get; }
        }
    }
}
