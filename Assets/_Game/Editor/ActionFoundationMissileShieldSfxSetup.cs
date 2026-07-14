using System;
using System.IO;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationMissileShieldSfxSetup
    {
        private const string AudioRoot = "Assets/_Game/Art/Audio/SFX/MissileShield";
        private const string MissileLaunchClipPath = AudioRoot + "/DB_SFX_Missile_Launch_01.mp3";
        private const string MissileFlyLoopClipPath = AudioRoot + "/DB_SFX_Missile_Fly_Loop_01.mp3";
        private const string MissileImpactGroundClipPath = AudioRoot + "/DB_SFX_Missile_Impact_Ground_01.mp3";
        private const string MissileImpactShieldClipPath = AudioRoot + "/DB_SFX_Missile_Impact_Shield_01.mp3";
        private const string ShieldActivateClipPath = AudioRoot + "/DB_SFX_Shield_Activate_01.mp3";
        private const string ShieldBlockProjectileClipPath = AudioRoot + "/DB_SFX_Shield_Block_Projectile_01.mp3";
        private const string PerfectDodgeWindowPrefabPath =
            "Assets/_Game/Art/VFX/CombatCues/Prefabs/DB_VFX_PlayerPerfectDodgeWindow.prefab";
        private const string ShieldActivateAudioName = "ReviewedSfx_PlayerPerfectDodgeSuccess";
        private const string MissileFlyAudioName = "BossBarrageProjectileAudio_MissileFlyLoop";

        private static readonly string[] ImportedClipPaths =
        {
            AudioRoot + "/DB_SFX_Missile_Incoming_01.mp3",
            AudioRoot + "/DB_SFX_Missile_Incoming_02.mp3",
            MissileFlyLoopClipPath,
            MissileImpactGroundClipPath,
            MissileImpactShieldClipPath,
            MissileLaunchClipPath,
            ShieldActivateClipPath,
            ShieldBlockProjectileClipPath,
            AudioRoot + "/DB_SFX_Shield_End_01.mp3"
        };

        private static readonly string[] CanonicalScenePaths =
        {
            ActionFoundationCombatAssetPaths.OlympusCorridorScenePath,
            ActionFoundationCombatAssetPaths.OlympusStationScenePath
        };

        [MenuItem("DimensionBrawl/ActionFoundation/Apply Missile Shield SFX")]
        public static void ApplyMissileShieldSfxMenu()
        {
            ApplyMissileShieldSfx();
        }

        public static void RunBatchApplyMissileShieldSfx()
        {
            ApplyMissileShieldSfx();
        }

        private static void ApplyMissileShieldSfx()
        {
            ImportAudioClips();
            ApplyPerfectDodgeWindowAudio();
            ApplyCombatCueProfileAudio();
            ApplyBossProjectileAudio();
            ApplySceneReferences();
            AssetDatabase.SaveAssets();
            Debug.Log("Applied missile and shield SFX.");
        }

        private static void ImportAudioClips()
        {
            EnsureFolder(AudioRoot);
            for (int i = 0; i < ImportedClipPaths.Length; i++)
            {
                string clipPath = ImportedClipPaths[i];
                string absolutePath = ToProjectAbsolutePath(clipPath);
                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException($"Missing missile/shield SFX clip at {clipPath}.");
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

        private static void ApplyPerfectDodgeWindowAudio()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PerfectDodgeWindowPrefabPath);
            try
            {
                Transform existing = prefabRoot.transform.Find(ShieldActivateAudioName);
                if (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                GameObject audioObject = new GameObject(ShieldActivateAudioName);
                audioObject.transform.SetParent(prefabRoot.transform, worldPositionStays: false);
                AudioSource source = audioObject.AddComponent<AudioSource>();
                source.clip = null;
                source.playOnAwake = false;
                source.loop = false;
                source.volume = 0.34f;
                source.pitch = 1f;
                source.spatialBlend = 0.08f;
                source.dopplerLevel = 0f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 3f;
                source.maxDistance = 28f;
                source.priority = 130;

                CombatVfxCueAudioRandomizer randomizer = audioObject.AddComponent<CombatVfxCueAudioRandomizer>();
                randomizer.Configure(
                    source,
                    new[] { LoadClip(ShieldActivateClipPath) },
                    0.34f,
                    0.98f,
                    1.04f,
                    0.92f,
                    1.02f);

                EditorUtility.SetDirty(audioObject);
                EditorUtility.SetDirty(source);
                EditorUtility.SetDirty(randomizer);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PerfectDodgeWindowPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ApplyCombatCueProfileAudio()
        {
            CombatVfxCueProfile profile =
                AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);
            if (profile == null)
            {
                throw new FileNotFoundException(
                    $"Missing combat VFX cue profile at {ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath}.");
            }

            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty cues = RequireProperty(serializedObject, "cues");
            SetCueAudio(
                cues,
                CombatVfxCueId.PlayerPerfectDodgeShieldBlockImpact,
                new[] { ShieldBlockProjectileClipPath, MissileImpactShieldClipPath },
                0.5f,
                0.96f,
                1.04f,
                0.88f,
                1.04f,
                0.1f,
                3f,
                28f,
                126);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ApplyBossProjectileAudio()
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(ActionFoundationCombatAssetPaths.BossProjectilePrefabPath);
            try
            {
                BossBarrageProjectile projectile = prefabRoot.GetComponent<BossBarrageProjectile>();
                if (projectile == null)
                {
                    throw new InvalidOperationException("Boss barrage projectile prefab is missing BossBarrageProjectile.");
                }

                SerializedObject projectileObject = new SerializedObject(projectile);
                RequireProperty(projectileObject, "impactSfx").objectReferenceValue = LoadClip(MissileImpactGroundClipPath);
                RequireProperty(projectileObject, "impactSfxVolume").floatValue = 0.42f;
                RequireProperty(projectileObject, "impactSfxPitchRange").vector2Value = new Vector2(0.96f, 1.04f);
                projectileObject.ApplyModifiedPropertiesWithoutUndo();

                Transform audioTransform = prefabRoot.transform.Find(MissileFlyAudioName);
                if (audioTransform == null)
                {
                    audioTransform = new GameObject(MissileFlyAudioName).transform;
                    audioTransform.SetParent(prefabRoot.transform, worldPositionStays: false);
                }

                audioTransform.localPosition = Vector3.zero;
                audioTransform.localRotation = Quaternion.identity;
                audioTransform.localScale = Vector3.one;
                AudioSource source = EnsureComponent<AudioSource>(audioTransform.gameObject);
                source.clip = LoadClip(MissileFlyLoopClipPath);
                source.playOnAwake = false;
                source.loop = true;
                source.volume = 0.22f;
                source.pitch = 1f;
                source.spatialBlend = 0.68f;
                source.dopplerLevel = 0f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 4f;
                source.maxDistance = 32f;
                source.priority = 142;

                EditorUtility.SetDirty(projectile);
                EditorUtility.SetDirty(audioTransform.gameObject);
                EditorUtility.SetDirty(source);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, ActionFoundationCombatAssetPaths.BossProjectilePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ApplySceneReferences()
        {
            AudioClip launchClip = LoadClip(MissileLaunchClipPath);
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
                    PerfectDodgeVfxDirector[] directors =
                        roots[rootIndex].GetComponentsInChildren<PerfectDodgeVfxDirector>(includeInactive: true);
                    for (int directorIndex = 0; directorIndex < directors.Length; directorIndex++)
                    {
                        SerializedObject directorObject = new SerializedObject(directors[directorIndex]);
                        RequireProperty(directorObject, "successClips").arraySize = 0;
                        directorObject.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(directors[directorIndex]);
                        changed = true;
                    }

                    BossBasicFireEmitter[] emitters =
                        roots[rootIndex].GetComponentsInChildren<BossBasicFireEmitter>(includeInactive: true);
                    for (int emitterIndex = 0; emitterIndex < emitters.Length; emitterIndex++)
                    {
                        ApplyBossEmitterAudio(emitters[emitterIndex], launchClip);
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

        private static void ApplyBossEmitterAudio(BossBasicFireEmitter emitter, AudioClip launchClip)
        {
            SerializedObject emitterObject = new SerializedObject(emitter);
            SerializedProperty clips = RequireProperty(emitterObject, "volleySfxClips");
            clips.arraySize = 1;
            clips.GetArrayElementAtIndex(0).objectReferenceValue = launchClip;
            RequireProperty(emitterObject, "volleySfxVolume").floatValue = 0.4f;
            RequireProperty(emitterObject, "volleySfxPitchRange").vector2Value = new Vector2(0.96f, 1.04f);
            AudioSource source = RequireProperty(emitterObject, "volleyAudioSource").objectReferenceValue as AudioSource;
            emitterObject.ApplyModifiedPropertiesWithoutUndo();

            if (source != null)
            {
                source.volume = 0.4f;
                source.spatialBlend = 0.25f;
                source.dopplerLevel = 0f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 4f;
                source.maxDistance = 32f;
                source.priority = 138;
                EditorUtility.SetDirty(source);
            }

            EditorUtility.SetDirty(emitter);
        }

        private static void SetCueAudio(
            SerializedProperty cues,
            CombatVfxCueId cueId,
            string[] clipPaths,
            float baseVolume,
            float minimumPitch,
            float maximumPitch,
            float minimumVolumeMultiplier,
            float maximumVolumeMultiplier,
            float spatialBlend,
            float minDistance,
            float maxDistance,
            int priority)
        {
            SerializedProperty cue = FindCue(cues, cueId);
            if (cue == null)
            {
                throw new InvalidOperationException($"Combat VFX cue profile is missing {cueId}.");
            }

            SerializedProperty audioClips = cue.FindPropertyRelative("audioClips");
            audioClips.arraySize = clipPaths.Length;
            for (int i = 0; i < clipPaths.Length; i++)
            {
                audioClips.GetArrayElementAtIndex(i).objectReferenceValue = LoadClip(clipPaths[i]);
            }

            cue.FindPropertyRelative("audioBaseVolume").floatValue = baseVolume;
            cue.FindPropertyRelative("audioMinimumPitch").floatValue = minimumPitch;
            cue.FindPropertyRelative("audioMaximumPitch").floatValue = maximumPitch;
            cue.FindPropertyRelative("audioMinimumVolumeMultiplier").floatValue = minimumVolumeMultiplier;
            cue.FindPropertyRelative("audioMaximumVolumeMultiplier").floatValue = maximumVolumeMultiplier;
            cue.FindPropertyRelative("audioSpatialBlend").floatValue = spatialBlend;
            cue.FindPropertyRelative("audioMinDistance").floatValue = minDistance;
            cue.FindPropertyRelative("audioMaxDistance").floatValue = maxDistance;
            cue.FindPropertyRelative("audioPriority").intValue = priority;
        }

        private static SerializedProperty FindCue(SerializedProperty cues, CombatVfxCueId cueId)
        {
            int cueIdValue = (int)cueId;
            for (int i = 0; i < cues.arraySize; i++)
            {
                SerializedProperty cue = cues.GetArrayElementAtIndex(i);
                if (cue.FindPropertyRelative("cueId").intValue == cueIdValue)
                {
                    return cue;
                }
            }

            return null;
        }

        private static AudioClip LoadClip(string clipPath)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null)
            {
                throw new FileNotFoundException($"Missing missile/shield SFX clip at {clipPath}.");
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

        private static void EnsureFolder(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string ToProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string relativePath = assetPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRoot, relativePath);
        }
    }
}
