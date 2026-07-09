#if UNITY_EDITOR
using System;
using UnityEditor;

namespace DimensionBrawl.Editor
{
    public static class OlympusStationCombatStageBuildSettings
    {
        public const string ScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";

        [MenuItem("DimensionBrawl/Stage/Olympus Station/Ensure Combat Scene Build Settings")]
        public static void EnsureSceneRegistered()
        {
            EditorBuildSettingsScene[] scenes =
                EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (!string.Equals(scene.path, ScenePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!scene.enabled)
                {
                    scene.enabled = true;
                    EditorBuildSettings.scenes = scenes;
                }

                return;
            }

            EditorBuildSettingsScene[] updatedScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            Array.Copy(scenes, updatedScenes, scenes.Length);
            updatedScenes[updatedScenes.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = updatedScenes;
        }
    }
}
#endif
