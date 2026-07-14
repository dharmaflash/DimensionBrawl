#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace DimensionBrawl.Editor
{
    public static class CanonicalUiBuildSettings
    {
        public const string LoginScenePath = "Assets/_Game/Scenes/UI/UI_Login.unity";
        public const string LobbyScenePath = "Assets/_Game/Scenes/UI/UI_Lobby.unity";
        public const string StageSelectScenePath = "Assets/_Game/Scenes/UI/UI_StageSelect.unity";
        public const string StageClearScenePath = "Assets/_Game/Scenes/UI/UI_StageClear.unity";

        private static readonly string[] CanonicalScenePaths =
        {
            LoginScenePath,
            LobbyScenePath,
            StageSelectScenePath,
            StageClearScenePath
        };

        [MenuItem("DimensionBrawl/UI/Ensure Canonical Build Settings")]
        public static void EnsureScenesRegistered()
        {
            EditorBuildSettingsScene[] currentScenes =
                EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            var updatedScenes = new List<EditorBuildSettingsScene>(currentScenes.Length + 4);
            var pathIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < currentScenes.Length; i++)
            {
                EditorBuildSettingsScene scene = currentScenes[i];
                string scenePath = scene.path;
                if (pathIndices.TryGetValue(scenePath, out int existingIndex))
                {
                    if (scene.enabled && !updatedScenes[existingIndex].enabled)
                    {
                        updatedScenes[existingIndex] = new EditorBuildSettingsScene(scenePath, true);
                    }

                    continue;
                }

                pathIndices.Add(scenePath, updatedScenes.Count);
                updatedScenes.Add(new EditorBuildSettingsScene(scenePath, scene.enabled));
            }

            for (int i = 0; i < CanonicalScenePaths.Length; i++)
            {
                string scenePath = CanonicalScenePaths[i];
                if (pathIndices.TryGetValue(scenePath, out int existingIndex))
                {
                    if (!updatedScenes[existingIndex].enabled)
                    {
                        updatedScenes[existingIndex] = new EditorBuildSettingsScene(scenePath, true);
                    }

                    continue;
                }

                pathIndices.Add(scenePath, updatedScenes.Count);
                updatedScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = updatedScenes.ToArray();
        }
    }
}
#endif
