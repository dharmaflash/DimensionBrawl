using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Prevents a user double-click from replacing any game scene while the Editor is entering,
    /// running, or leaving Play Mode. Scripted scene opens used by validators are unaffected.
    /// </summary>
    public static class ProjectSceneAssetOpenGuard
    {
        private const string GameSceneRoot = "Assets/_Game/Scenes/";

        [OnOpenAsset(0)]
        public static bool BlockUnsafeSceneAssetOpen(int instanceId, int line)
        {
            string path = AssetDatabase.GetAssetPath(instanceId);
            bool shouldBlock = ShouldBlock(
                path,
                EditorApplication.isPlaying,
                EditorApplication.isPlayingOrWillChangePlaymode);
            if (!shouldBlock)
            {
                return false;
            }

            const string message =
                "Scene switch blocked while Play Mode is active or changing. "
                + "Stop Play Mode, wait for Edit Mode, then open the scene again.";
            Debug.LogWarning($"{message} Requested scene: {path}");
            EditorWindow.focusedWindow?.ShowNotification(new GUIContent(message));
            return true;
        }

        public static void RunBatchVerification()
        {
            var failures = new List<string>();
            VerifyDecision(
                failures,
                "Edit Mode game scene",
                false,
                "Assets/_Game/Scenes/Review/UI_OlympusChapterNarrativeReview.unity",
                false,
                false);
            VerifyDecision(
                failures,
                "Entering Play Mode game scene",
                true,
                "Assets/_Game/Scenes/UI/UI_Login.unity",
                false,
                true);
            VerifyDecision(
                failures,
                "Running Play Mode game scene",
                true,
                "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
                true,
                true);
            VerifyDecision(
                failures,
                "Leaving Play Mode game scene",
                true,
                "Assets/_Game/Scenes/Review/UI_OlympusChapterHubReview.unity",
                true,
                false);
            VerifyDecision(
                failures,
                "Non-scene asset",
                false,
                "Assets/_Game/DesignDocs/STORY_TUTORIAL_REVIEW_TRANSITION_LAB.md",
                true,
                true);
            VerifyDecision(
                failures,
                "Package scene outside game root",
                false,
                "Packages/example/Scene.unity",
                true,
                true);

            if (failures.Count > 0)
            {
                throw new InvalidOperationException(
                    "Project scene asset-open guard verification failed:\n- "
                    + string.Join("\n- ", failures));
            }

            Debug.Log("Project scene asset-open guard verification passed (6/6 decisions).");
        }

        internal static bool ShouldBlock(
            string assetPath,
            bool isPlaying,
            bool isPlayingOrWillChangePlaymode)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            string normalizedPath = assetPath.Replace('\\', '/');
            bool isGameScene = normalizedPath.StartsWith(
                    GameSceneRoot,
                    StringComparison.Ordinal)
                && normalizedPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
            return isGameScene && (isPlaying || isPlayingOrWillChangePlaymode);
        }

        private static void VerifyDecision(
            ICollection<string> failures,
            string label,
            bool expected,
            string path,
            bool isPlaying,
            bool isPlayingOrWillChangePlaymode)
        {
            bool actual = ShouldBlock(path, isPlaying, isPlayingOrWillChangePlaymode);
            if (actual != expected)
            {
                failures.Add($"{label}: expected {expected}, observed {actual}.");
            }
        }
    }
}
