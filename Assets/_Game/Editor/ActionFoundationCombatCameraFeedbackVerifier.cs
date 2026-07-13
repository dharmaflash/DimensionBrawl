using System.IO;
using DimensionBrawl.Verification;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    [InitializeOnLoad]
    public static class ActionFoundationCombatCameraFeedbackVerifier
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity";
        private const string ProbeName =
            "CombatCameraFeedbackProbe";
        private const string ActiveKey =
            "DimensionBrawl.CombatCameraFeedback.Active";
        private const string StartedAtKey =
            "DimensionBrawl.CombatCameraFeedback.StartedAt";
        private const string ResultPathKey =
            "DimensionBrawl.CombatCameraFeedback.ResultPath";
        private const string ProbeInstalledKey =
            "DimensionBrawl.CombatCameraFeedback.ProbeInstalled";

        public const string ResultPath =
            "C:/tmp/DimensionBrawl-CombatCameraFeedback.result";

        static ActionFoundationCombatCameraFeedbackVerifier()
        {
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
        }

        public static void RunBatchVerification()
        {
            ActionFoundationBatchVerificationResult.DeleteIfExists(ResultPath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EditorPrefs.SetBool(ActiveKey, true);
            EditorPrefs.SetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            EditorPrefs.SetString(ResultPathKey, ResultPath);
            EditorPrefs.SetBool(ProbeInstalledKey, false);
            EditorApplication.update -= Monitor;
            EditorApplication.update += Monitor;
            EditorApplication.isPlaying = true;
        }

        private static void Monitor()
        {
            if (!EditorPrefs.GetBool(ActiveKey, false))
            {
                return;
            }

            string resultPath = EditorPrefs.GetString(ResultPathKey, ResultPath);
            if (!string.IsNullOrWhiteSpace(resultPath) && File.Exists(resultPath))
            {
                bool passed = ActionFoundationBatchVerificationResult.IsPassMarkerFile(resultPath);
                Clear();
                Debug.Log(
                    passed
                        ? $"Combat camera feedback verification passed. See {resultPath}."
                        : $"Combat camera feedback verification failed. See {resultPath}.");
                EditorApplication.Exit(passed ? 0 : 1);
                return;
            }

            if (EditorApplication.isPlaying && !EditorPrefs.GetBool(ProbeInstalledKey, false))
            {
                InstallProbe(SceneManager.GetActiveScene(), resultPath);
                EditorPrefs.SetBool(ProbeInstalledKey, true);
                Debug.Log("Installed combat camera feedback probe.");
            }

            float startedAt = EditorPrefs.GetFloat(StartedAtKey, (float)EditorApplication.timeSinceStartup);
            if (EditorApplication.timeSinceStartup - startedAt <= 20f)
            {
                return;
            }

            ActionFoundationBatchVerificationResult.WriteResult(
                resultPath,
                false,
                "TIMEOUT",
                string.Empty,
                new[] { "Combat camera feedback verification timed out." });
            Clear();
            EditorApplication.Exit(1);
        }

        private static void InstallProbe(Scene scene, string resultPath)
        {
            GameObject existing = FindRoot(scene, ProbeName);
            if (existing != null)
            {
                Object.Destroy(existing);
            }

            GameObject probeObject = new GameObject(ProbeName);
            SceneManager.MoveGameObjectToScene(probeObject, scene);
            CombatCameraFeedbackProbe probe = probeObject.AddComponent<CombatCameraFeedbackProbe>();
            probe.Configure(resultPath, 4f);
            probe.BeginVerification();
        }

        private static GameObject FindRoot(Scene scene, string rootName)
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

        private static void Clear()
        {
            EditorPrefs.DeleteKey(ActiveKey);
            EditorPrefs.DeleteKey(StartedAtKey);
            EditorPrefs.DeleteKey(ResultPathKey);
            EditorPrefs.DeleteKey(ProbeInstalledKey);
            EditorApplication.update -= Monitor;
        }
    }
}
