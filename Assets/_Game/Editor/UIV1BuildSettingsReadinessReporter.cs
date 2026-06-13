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

        [MenuItem("DimensionBrawl/UI V1/Report Build Settings Readiness")]
        public static void ReportMenu()
        {
            ReportCurrentReadiness();
        }

        [MenuItem("DimensionBrawl/UI V1/Apply UI Test Build Settings")]
        public static void ApplyMenu()
        {
            ApplyUiTestBuildSettings();
        }

        public static void ApplyUiTestBuildSettings()
        {
            UIScreenRouteTable routeTable = AssetDatabase.LoadAssetAtPath<UIScreenRouteTable>(RouteTablePath);
            if (routeTable == null)
            {
                throw new System.InvalidOperationException($"UI V1 build settings apply could not find route table at {RouteTablePath}.");
            }

            List<RouteScene> routeScenes = CollectRouteScenes(routeTable);
            if (routeScenes.Count == 0)
            {
                throw new System.InvalidOperationException("UI V1 route table must contain at least one buildable route scene.");
            }

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[routeScenes.Count];
            for (int i = 0; i < routeScenes.Count; i++)
            {
                string scenePath = routeScenes[i].ScenePath;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    throw new System.InvalidOperationException($"UI V1 route scene is missing: {scenePath}");
                }

                buildScenes[i] = new EditorBuildSettingsScene(scenePath, true);
            }

            EditorBuildSettings.scenes = buildScenes;
            AssetDatabase.SaveAssets();
            Debug.Log($"UI V1 build settings applied. {routeScenes.Count} route scene(s) are enabled, starting at {routeScenes[0].ScenePath}.");
        }

        public static void ReportCurrentReadiness()
        {
            UIScreenRouteTable routeTable = AssetDatabase.LoadAssetAtPath<UIScreenRouteTable>(RouteTablePath);
            if (routeTable == null)
            {
                Debug.LogWarning($"UI V1 build settings readiness could not find route table at {RouteTablePath}.");
                return;
            }

            List<RouteScene> routeScenes = CollectRouteScenes(routeTable);
            HashSet<string> enabledBuildScenes = CollectEnabledBuildScenes();
            string firstEnabledBuildScene = GetFirstEnabledBuildScene();
            List<RouteScene> missingScenes = new List<RouteScene>();

            for (int i = 0; i < routeScenes.Count; i++)
            {
                if (!enabledBuildScenes.Contains(routeScenes[i].ScenePath))
                {
                    missingScenes.Add(routeScenes[i]);
                }
            }

            bool firstSceneMatchesRouteStart = routeScenes.Count > 0
                && string.Equals(firstEnabledBuildScene, routeScenes[0].ScenePath, System.StringComparison.Ordinal);

            if (missingScenes.Count == 0 && firstSceneMatchesRouteStart)
            {
                Debug.Log($"UI V1 build settings readiness passed. {routeScenes.Count} route scene(s) are enabled in Build Settings and the first scene is {firstEnabledBuildScene}.");
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("UI V1 build settings readiness warning: ");
            builder.Append(missingScenes.Count);
            builder.Append('/');
            builder.Append(routeScenes.Count);
            builder.AppendLine(" route scene(s) are not enabled in Build Settings.");
            builder.AppendLine("ProjectSettings were not modified by this report. Apply the UI test Build Settings in a reviewed settings pass:");

            for (int i = 0; i < missingScenes.Count; i++)
            {
                builder.Append("- ");
                builder.Append(missingScenes[i].RouteId);
                builder.Append(": ");
                builder.AppendLine(missingScenes[i].ScenePath);
            }

            if (!firstSceneMatchesRouteStart && routeScenes.Count > 0)
            {
                builder.Append("First enabled build scene should be ");
                builder.Append(routeScenes[0].ScenePath);
                builder.Append(", found ");
                builder.AppendLine(string.IsNullOrWhiteSpace(firstEnabledBuildScene) ? "<none>" : firstEnabledBuildScene);
            }

            Debug.LogWarning(builder.ToString());
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
                    routeScenes.Add(new RouteScene(routeId, scenePath.Replace('\\', '/')));
                }
            }

            return routeScenes;
        }

        private static HashSet<string> CollectEnabledBuildScenes()
        {
            HashSet<string> scenePaths = new HashSet<string>();
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

        private static string GetFirstEnabledBuildScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && !string.IsNullOrWhiteSpace(scenes[i].path))
                {
                    return scenes[i].path.Replace('\\', '/');
                }
            }

            return string.Empty;
        }

        private readonly struct RouteScene
        {
            public RouteScene(UIRouteId routeId, string scenePath)
            {
                RouteId = routeId;
                ScenePath = scenePath;
            }

            public UIRouteId RouteId { get; }
            public string ScenePath { get; }
        }
    }
}
