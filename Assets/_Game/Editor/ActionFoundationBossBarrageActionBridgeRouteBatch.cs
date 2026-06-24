using System;
using System.IO;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        public const string ActionBridgeRouteResultPath =
            "C:/tmp/DimensionBrawl-BossBarrageActionBridgeRoute.result";

        private const string ActionBridgeRouteProbeName =
            "BossBarrageLaneReview_ActionBridgeRouteProbe";

        public static void RunBatchActionBridgeInputRouteVerification()
        {
            EnsureBossBarrageLaneReviewScene();
            EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            ActionFoundationBossBarrageActionBridgeRouteBatch.Start(
                ActionBridgeRouteResultPath,
                90f);
            EditorApplication.isPlaying = true;
        }

        internal static void ConfigureActionBridgeInputRouteProbe(Scene scene)
        {
            GameObject existing = FindActionBridgeRouteProbeRoot(scene, ActionBridgeRouteProbeName);
            if (existing != null)
            {
                if (EditorApplication.isPlaying)
                {
                    UnityEngine.Object.Destroy(existing);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(existing);
                }
            }

            GameObject probeObject = new GameObject(ActionBridgeRouteProbeName);
            SceneManager.MoveGameObjectToScene(probeObject, scene);
            BossBarrageActionBridgeRouteProbe probe =
                probeObject.AddComponent<BossBarrageActionBridgeRouteProbe>();

            SerializedObject serializedProbe = new SerializedObject(probe);
            RequireActionBridgeRouteProbeProperty(serializedProbe, "resultPath").stringValue =
                ActionBridgeRouteResultPath;
            RequireActionBridgeRouteProbeProperty(serializedProbe, "routeTimeoutSeconds").floatValue = 12f;
            RequireActionBridgeRouteProbeProperty(serializedProbe, "idleTimeoutSeconds").floatValue = 12f;
            RequireActionBridgeRouteProbeProperty(serializedProbe, "tierThreeGrantEnergy").floatValue = 1000f;
            RequireActionBridgeRouteProbeProperty(serializedProbe, "settleSeconds").floatValue = 0.2f;
            serializedProbe.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(probe);

            if (EditorApplication.isPlaying)
            {
                probe.BeginVerification();
            }
        }

        private static GameObject FindActionBridgeRouteProbeRoot(Scene scene, string rootName)
        {
            if (!scene.IsValid())
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == rootName)
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static SerializedProperty RequireActionBridgeRouteProbeProperty(
            SerializedObject serializedObject,
            string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }
    }

    [InitializeOnLoad]
    internal static class ActionFoundationBossBarrageActionBridgeRouteBatch
    {
        private const string ActiveKey =
            "DimensionBrawl.BossBarrage.ActionBridgeRoute.Active";
        private const string ResultPathKey =
            "DimensionBrawl.BossBarrage.ActionBridgeRoute.ResultPath";
        private const string StartedAtKey =
            "DimensionBrawl.BossBarrage.ActionBridgeRoute.StartedAt";
        private const string TimeoutSecondsKey =
            "DimensionBrawl.BossBarrage.ActionBridgeRoute.TimeoutSeconds";
        private const string ProbeInstalledKey =
            "DimensionBrawl.BossBarrage.ActionBridgeRoute.ProbeInstalled";

        static ActionFoundationBossBarrageActionBridgeRouteBatch()
        {
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
        }

        public static void Start(string resultPath, float timeoutSeconds)
        {
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            EditorPrefs.SetBool(ActiveKey, true);
            EditorPrefs.SetString(ResultPathKey, resultPath);
            EditorPrefs.SetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            EditorPrefs.SetFloat(TimeoutSecondsKey, timeoutSeconds);
            EditorPrefs.SetBool(ProbeInstalledKey, false);
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
            Debug.Log($"Started action bridge input-route verification monitor: {resultPath}");
        }

        private static void Monitor()
        {
            if (!EditorPrefs.GetBool(ActiveKey, false))
            {
                return;
            }

            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            string resultPath = EditorPrefs.GetString(ResultPathKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(resultPath) && File.Exists(resultPath))
            {
                string result = File.ReadAllText(resultPath);
                bool passed = result.Contains("RESULT=PASS");
                Clear();
                if (passed)
                {
                    Debug.Log($"Action bridge input-route verification passed. See {resultPath}.");
                }
                else
                {
                    Debug.LogError($"Action bridge input-route verification failed. See {resultPath}.");
                }

                EditorApplication.Exit(passed ? 0 : 1);
                return;
            }

            if (EditorApplication.isPlaying && !EditorPrefs.GetBool(ProbeInstalledKey, false))
            {
                ActionFoundationBossBarrageLaneReviewSetup.ConfigureActionBridgeInputRouteProbe(
                    SceneManager.GetActiveScene());
                EditorPrefs.SetBool(ProbeInstalledKey, true);
                Debug.Log("Installed action bridge input-route probe in active scene.");
            }

            float startedAt = EditorPrefs.GetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            float timeoutSeconds = EditorPrefs.GetFloat(TimeoutSecondsKey, 90f);
            if (EditorApplication.timeSinceStartup - startedAt <= timeoutSeconds)
            {
                return;
            }

            Clear();
            Debug.LogError($"Action bridge input-route verification timed out after {timeoutSeconds:F1}s.");
            EditorApplication.Exit(1);
        }

        private static void Clear()
        {
            EditorPrefs.DeleteKey(ActiveKey);
            EditorPrefs.DeleteKey(ResultPathKey);
            EditorPrefs.DeleteKey(StartedAtKey);
            EditorPrefs.DeleteKey(TimeoutSecondsKey);
            EditorPrefs.DeleteKey(ProbeInstalledKey);
            EditorApplication.update -= Monitor;
        }
    }
}
