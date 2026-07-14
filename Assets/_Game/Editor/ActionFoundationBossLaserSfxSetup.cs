using System;
using System.IO;
using DimensionBrawl.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationBossLaserSfxSetup
    {
        private const string LaserSustainLoopAudioName = "BossLaserSustainLoopAudio";

        private static readonly string[] ImportedClipPaths =
        {
            ActionFoundationCombatAssetPaths.BossLaserTelegraphSfxClipPath,
            ActionFoundationCombatAssetPaths.BossLaserSustainLoopSfxClipPath,
            ActionFoundationCombatAssetPaths.BossLaserEndSfxClipPath
        };

        private static readonly string[] CanonicalScenePaths =
        {
            ActionFoundationCombatAssetPaths.OlympusCorridorScenePath,
            ActionFoundationCombatAssetPaths.OlympusStationScenePath
        };

        [MenuItem("DimensionBrawl/ActionFoundation/Apply Boss Laser SFX")]
        public static void ApplyBossLaserSfxMenu()
        {
            ApplyBossLaserSfx();
        }

        public static void RunBatchApplyBossLaserSfx()
        {
            ApplyBossLaserSfx();
        }

        private static void ApplyBossLaserSfx()
        {
            ImportAudioClips();
            ApplyBossLaserPrefab();
            ApplySceneReferences();
            AssetDatabase.SaveAssets();
            Debug.Log("Applied boss laser SFX.");
        }

        private static void ImportAudioClips()
        {
            for (int i = 0; i < ImportedClipPaths.Length; i++)
            {
                string clipPath = ImportedClipPaths[i];
                string absolutePath = ToProjectAbsolutePath(clipPath);
                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException($"Missing boss laser SFX clip at {clipPath}.");
                }

                AssetDatabase.ImportAsset(clipPath, ImportAssetOptions.ForceUpdate);
                if (AssetImporter.GetAtPath(clipPath) is AudioImporter importer)
                {
                    importer.forceToMono = true;
                    importer.loadInBackground = false;
                    AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                    settings.loadType = AudioClipLoadType.DecompressOnLoad;
                    settings.preloadAudioData = true;
                    importer.defaultSampleSettings = settings;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void ApplyBossLaserPrefab()
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(ActionFoundationCombatAssetPaths.BossLaserSummonActorPrefabPath);
            try
            {
                BossLaserSummonPattern laserPattern = prefabRoot.GetComponent<BossLaserSummonPattern>();
                if (laserPattern == null)
                {
                    throw new InvalidOperationException("Boss laser summon prefab is missing BossLaserSummonPattern.");
                }

                ApplyPatternAudio(laserPattern);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, ActionFoundationCombatAssetPaths.BossLaserSummonActorPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ApplySceneReferences()
        {
            for (int i = 0; i < CanonicalScenePaths.Length; i++)
            {
                string scenePath = CanonicalScenePaths[i];
                if (!File.Exists(ToProjectAbsolutePath(scenePath)))
                {
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                GameObject[] roots = scene.GetRootGameObjects();
                bool changed = false;
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    BossLaserSummonPattern[] patterns =
                        roots[rootIndex].GetComponentsInChildren<BossLaserSummonPattern>(includeInactive: true);
                    for (int patternIndex = 0; patternIndex < patterns.Length; patternIndex++)
                    {
                        ApplyPatternAudio(patterns[patternIndex]);
                        changed = true;
                    }
                }

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        private static void ApplyPatternAudio(BossLaserSummonPattern laserPattern)
        {
            AudioSource oneShotSource = EnsureComponent<AudioSource>(laserPattern.gameObject);
            ConfigureOneShotAudioSource(oneShotSource);

            AudioSource sustainSource = EnsureSustainLoopAudioSource(laserPattern.transform);
            ConfigureSustainLoopAudioSource(sustainSource);

            SerializedObject serializedPattern = new SerializedObject(laserPattern);
            RequireProperty(serializedPattern, "audioSource").objectReferenceValue = oneShotSource;
            RequireProperty(serializedPattern, "laserSustainLoopAudioSource").objectReferenceValue = sustainSource;
            RequireProperty(serializedPattern, "telegraphSfx").objectReferenceValue =
                LoadClip(ActionFoundationCombatAssetPaths.BossLaserTelegraphSfxClipPath);
            RequireProperty(serializedPattern, "laserFireSfx").objectReferenceValue =
                LoadClip(ActionFoundationCombatAssetPaths.BossLaserFireSfxClipPath);
            RequireProperty(serializedPattern, "laserSustainLoopSfx").objectReferenceValue =
                LoadClip(ActionFoundationCombatAssetPaths.BossLaserSustainLoopSfxClipPath);
            RequireProperty(serializedPattern, "laserEndSfx").objectReferenceValue =
                LoadClip(ActionFoundationCombatAssetPaths.BossLaserEndSfxClipPath);
            RequireProperty(serializedPattern, "telegraphSfxVolume").floatValue = 0.72f;
            RequireProperty(serializedPattern, "laserFireSfxVolume").floatValue = 0f;
            RequireProperty(serializedPattern, "laserSustainLoopSfxVolume").floatValue = 0.56f;
            RequireProperty(serializedPattern, "laserEndSfxVolume").floatValue = 0.52f;
            serializedPattern.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(laserPattern);
            EditorUtility.SetDirty(oneShotSource);
            EditorUtility.SetDirty(sustainSource);
            EditorUtility.SetDirty(sustainSource.gameObject);
        }

        private static void ConfigureOneShotAudioSource(AudioSource source)
        {
            source.clip = null;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = 0.72f;
            source.pitch = 1f;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1.6f;
            source.maxDistance = 18f;
            source.priority = 128;
        }

        private static void ConfigureSustainLoopAudioSource(AudioSource source)
        {
            source.clip = LoadClip(ActionFoundationCombatAssetPaths.BossLaserSustainLoopSfxClipPath);
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0.56f;
            source.pitch = 1f;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1.6f;
            source.maxDistance = 20f;
            source.priority = 132;
        }

        private static AudioSource EnsureSustainLoopAudioSource(Transform root)
        {
            Transform audioRoot = root.Find(LaserSustainLoopAudioName);
            if (audioRoot == null)
            {
                audioRoot = new GameObject(LaserSustainLoopAudioName).transform;
                audioRoot.SetParent(root, worldPositionStays: false);
            }

            audioRoot.localPosition = Vector3.zero;
            audioRoot.localRotation = Quaternion.identity;
            audioRoot.localScale = Vector3.one;
            return EnsureComponent<AudioSource>(audioRoot.gameObject);
        }

        private static AudioClip LoadClip(string clipPath)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null)
            {
                throw new FileNotFoundException($"Missing boss laser SFX clip at {clipPath}.");
            }

            return clip;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static T EnsureComponent<T>(GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();
            return component != null ? component : owner.AddComponent<T>();
        }

        private static string ToProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string relativePath = assetPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRoot, relativePath);
        }
    }
}
