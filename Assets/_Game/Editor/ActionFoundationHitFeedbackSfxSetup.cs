using System;
using System.Collections.Generic;
using System.IO;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationHitFeedbackSfxSetup
    {
        private const string SourceFolderName = "\uC0C8 \uD3F4\uB354";
        private const string TargetRoot = "Assets/_Game/Art/Audio/SFX/HitFeedback";
        private const string ProfilePath = "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset";
        private const string EnemyHitVisualPrefabPath = "Assets/_Game/Art/VFX/CombatCues/Prefabs/DB_VFX_EnemyHit.prefab";

        private static readonly ClipCopy[] ClipCopies =
        {
            new("EnemyHit_Armor_01_Bu_#3-1783106970922.mp3", "Enemy", "DB_SFX_EnemyHit_Armor_01.mp3"),
            new("EnemyHit_Armor_02_Bo_#4-1783106988303.mp3", "Enemy", "DB_SFX_EnemyHit_Armor_02.mp3"),
            new("EnemyHit_Armor_03_De_#3-1783107009673.mp3", "Enemy", "DB_SFX_EnemyHit_Armor_03.mp3"),
            new("EnemyHit_Final_01_Fi_#3-1783107063815.mp3", "Enemy", "DB_SFX_EnemyHit_Final_01.mp3"),
            new("EnemyHit_Final_02_La_#2-1783107084387.mp3", "Enemy", "DB_SFX_EnemyHit_Final_02.mp3"),
            new("EnemyHit_Heavy_01_He_#1-1783106726875.mp3", "Enemy", "DB_SFX_EnemyHit_Heavy_01.mp3"),
            new("EnemyHit_Heavy_02_Po_#4-1783106830680.mp3", "Enemy", "DB_SFX_EnemyHit_Heavy_02.mp3"),
            new("EnemyHit_Heavy_03_St_#4-1783106903742.mp3", "Enemy", "DB_SFX_EnemyHit_Heavy_03.mp3"),
            new("EnemyHit_Light_01_Sh_#2-1783106452025.mp3", "Enemy", "DB_SFX_EnemyHit_Light_01.mp3"),
            new("EnemyHit_Light_02_Qu_#4-1783106481398.mp3", "Enemy", "DB_SFX_EnemyHit_Light_02.mp3"),
            new("EnemyHit_Light_03_Li_#3-1783106812149.mp3", "Enemy", "DB_SFX_EnemyHit_Light_03.mp3"),
            new("EnemyHit_Light_04_Sm_#1-1783106697083.mp3", "Enemy", "DB_SFX_EnemyHit_Light_04.mp3"),
            new("HitWhooshMicro_01_Ve_#3-1783107867614.mp3", "Shared", "DB_SFX_HitWhooshMicro_01.mp3"),
            new("HitWhooshMicro_02_Mi_#4-1783107884416.mp3", "Shared", "DB_SFX_HitWhooshMicro_02.mp3"),
            new("HitWhooshMicro_03_Ti_#2-1783107909705.mp3", "Shared", "DB_SFX_HitWhooshMicro_03.mp3"),
            new("PlayerCritical_01_Cr_#2-1783107567836.mp3", "Player", "DB_SFX_PlayerCritical_01.mp3"),
            new("PlayerCritical_02_Da_#3-1783107591807.mp3", "Player", "DB_SFX_PlayerCritical_02.mp3"),
            new("PlayerDamaged_Heavy__#2-1783107300040.mp3", "Player", "DB_SFX_PlayerDamaged_Heavy_01.mp3"),
            new("PlayerDamaged_Heavy__#4-1783107228339.mp3", "Player", "DB_SFX_PlayerDamaged_Heavy_02.mp3"),
            new("PlayerDamaged_Heavy__#4-1783107380704.mp3", "Player", "DB_SFX_PlayerDamaged_Heavy_03.mp3"),
            new("PlayerDamaged_Heavy__#4-1783107524085.mp3", "Player", "DB_SFX_PlayerDamaged_Heavy_04.mp3"),
            new("PlayerDamaged_Light__#1-1783107173587.mp3", "Player", "DB_SFX_PlayerDamaged_Light_01.mp3"),
            new("PlayerDamaged_Light__#3-1783107188438.mp3", "Player", "DB_SFX_PlayerDamaged_Light_02.mp3"),
            new("ShieldOrGuardHit_01__#4-1783107634341.mp3", "Enemy", "DB_SFX_ShieldOrGuardHit_01.mp3"),
            new("ShieldOrGuardHit_02__#4-1783107668598.mp3", "Enemy", "DB_SFX_ShieldOrGuardHit_02.mp3"),
            new("ShieldOrGuardHit_03__#4-1783107700897.mp3", "Enemy", "DB_SFX_ShieldOrGuardHit_03.mp3"),
            new("WeakpointOrPreciseHi_#1-1783107729609.mp3", "Enemy", "DB_SFX_WeakpointOrPreciseHit_01.mp3"),
            new("WeakpointOrPreciseHi_#1-1783107758270.mp3", "Enemy", "DB_SFX_WeakpointOrPreciseHit_02.mp3"),
            new("WeakpointOrPreciseHi_#1-1783107844194.mp3", "Enemy", "DB_SFX_WeakpointOrPreciseHit_03.mp3"),
        };

        private static readonly string[] EnemyHitLightClipPaths =
        {
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_02.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_03.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_04.mp3",
        };

        private static readonly string[] EnemyHitCueClipPaths =
        {
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_02.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_03.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_04.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_02.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_03.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Light_04.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Heavy_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Heavy_02.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Heavy_03.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Armor_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Armor_02.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Armor_03.mp3",
            TargetRoot + "/Enemy/DB_SFX_ShieldOrGuardHit_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_ShieldOrGuardHit_02.mp3",
            TargetRoot + "/Enemy/DB_SFX_ShieldOrGuardHit_03.mp3",
            TargetRoot + "/Enemy/DB_SFX_WeakpointOrPreciseHit_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_WeakpointOrPreciseHit_02.mp3",
            TargetRoot + "/Enemy/DB_SFX_WeakpointOrPreciseHit_03.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Final_01.mp3",
            TargetRoot + "/Enemy/DB_SFX_EnemyHit_Final_02.mp3",
        };

        private static readonly string[] PlayerDamagedCueClipPaths =
        {
            TargetRoot + "/Player/DB_SFX_PlayerDamaged_Light_01.mp3",
            TargetRoot + "/Player/DB_SFX_PlayerDamaged_Light_02.mp3",
            TargetRoot + "/Player/DB_SFX_PlayerDamaged_Heavy_01.mp3",
            TargetRoot + "/Player/DB_SFX_PlayerDamaged_Heavy_02.mp3",
            TargetRoot + "/Player/DB_SFX_PlayerDamaged_Heavy_03.mp3",
            TargetRoot + "/Player/DB_SFX_PlayerDamaged_Heavy_04.mp3",
        };

        private static readonly string[] PlayerCriticalClipPaths =
        {
            TargetRoot + "/Player/DB_SFX_PlayerCritical_01.mp3",
            TargetRoot + "/Player/DB_SFX_PlayerCritical_02.mp3",
        };

        private static readonly string[] HitWhooshMicroClipPaths =
        {
            TargetRoot + "/Shared/DB_SFX_HitWhooshMicro_01.mp3",
            TargetRoot + "/Shared/DB_SFX_HitWhooshMicro_02.mp3",
            TargetRoot + "/Shared/DB_SFX_HitWhooshMicro_03.mp3",
        };

        private static string SourceRoot =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), SourceFolderName);

        [MenuItem("DimensionBrawl/ActionFoundation/Import Master Hit Feedback SFX")]
        public static void ImportAndAssignMasterHitFeedbackSfx()
        {
            CopySfxIntoProject();
            StripReviewedAudioFromPrefab(EnemyHitVisualPrefabPath);
            UpdateCueProfile();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Imported and assigned master hit feedback SFX to original hit VFX cues.");
        }

        [MenuItem("DimensionBrawl/ActionFoundation/Validate Master Hit Feedback SFX")]
        public static void ValidateMasterHitFeedbackSfx()
        {
            ValidateImportedClips();
            ValidateCueProfile();
            Debug.Log("Validated master hit feedback SFX cue audio assignments.");
        }

        public static string[] GetEnemyHitLightClipPaths() => (string[])EnemyHitLightClipPaths.Clone();
        public static string[] GetEnemyHitCueClipPaths() => (string[])EnemyHitCueClipPaths.Clone();
        public static string[] GetPlayerDamagedCueClipPaths() => (string[])PlayerDamagedCueClipPaths.Clone();
        public static string[] GetPlayerCriticalClipPaths() => (string[])PlayerCriticalClipPaths.Clone();
        public static string[] GetHitWhooshMicroClipPaths() => (string[])HitWhooshMicroClipPaths.Clone();

        private static void CopySfxIntoProject()
        {
            if (!Directory.Exists(SourceRoot))
            {
                throw new DirectoryNotFoundException($"Missing master SFX source folder: {SourceRoot}");
            }

            EnsureFolder(TargetRoot);
            for (int i = 0; i < ClipCopies.Length; i++)
            {
                ClipCopy clipCopy = ClipCopies[i];
                string sourcePath = Path.Combine(SourceRoot, clipCopy.SourceFileName);
                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException($"Missing master SFX source clip: {sourcePath}");
                }

                string targetDirectory = $"{TargetRoot}/{clipCopy.Category}";
                EnsureFolder(targetDirectory);
                string targetPath = $"{targetDirectory}/{clipCopy.TargetFileName}";
                File.Copy(sourcePath, targetPath, overwrite: true);
                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
                ConfigureAudioImporter(targetPath);
            }
        }

        private static void ConfigureAudioImporter(string assetPath)
        {
            if (AssetImporter.GetAtPath(assetPath) is not AudioImporter importer)
            {
                return;
            }

            importer.forceToMono = true;
            importer.loadInBackground = false;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.preloadAudioData = true;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
            settings.quality = 0.9f;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }

        private static void StripReviewedAudioFromPrefab(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                List<GameObject> reviewedAudioChildren = new List<GameObject>();
                Transform[] children = prefabRoot.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < children.Length; i++)
                {
                    Transform child = children[i];
                    if (child != null
                        && child != prefabRoot.transform
                        && child.name.StartsWith("ReviewedSfx_", StringComparison.Ordinal))
                    {
                        reviewedAudioChildren.Add(child.gameObject);
                    }
                }

                for (int i = 0; i < reviewedAudioChildren.Count; i++)
                {
                    UnityEngine.Object.DestroyImmediate(reviewedAudioChildren[i]);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void UpdateCueProfile()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(ProfilePath);
            if (profile == null)
            {
                throw new FileNotFoundException($"Missing combat VFX cue profile at {ProfilePath}.");
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty cues = serializedProfile.FindProperty("cues");
            RemoveInvalidCueEntries(cues);

            UpdateOrAddCue(
                cues,
                CombatVfxCueId.EnemyHit,
                EnemyHitVisualPrefabPath,
                new Vector3(0f, 0.1f, 0f),
                Vector3.zero,
                Vector3.one,
                0.32f,
                false,
                true,
                new AudioCueDefinition(EnemyHitCueClipPaths, 0.48f, 0.96f, 1.05f, 0.98f, 1.1f, 0.05f, 10f, 45f, 48));
            UpdateOrAddCue(
                cues,
                CombatVfxCueId.PlayerDamaged,
                EnemyHitVisualPrefabPath,
                new Vector3(0f, 0.72f, 0f),
                Vector3.zero,
                new Vector3(0.14f, 0.12f, 0.14f),
                0.22f,
                true,
                false,
                new AudioCueDefinition(PlayerDamagedCueClipPaths, 0.49f, 0.97f, 1.04f, 1f, 1.12f, 0f, 6f, 36f, 40));
            UpdateOrAddCue(
                cues,
                CombatVfxCueId.PlayerCritical,
                EnemyHitVisualPrefabPath,
                new Vector3(0f, 0.82f, 0f),
                Vector3.zero,
                new Vector3(0.1f, 0.09f, 0.1f),
                0.22f,
                true,
                false,
                new AudioCueDefinition(PlayerCriticalClipPaths, 0.5f, 0.94f, 1f, 1f, 1.12f, 0f, 6f, 36f, 32));
            UpdateCueAudio(
                cues,
                CombatVfxCueId.PlayerRangedProjectileImpact,
                new AudioCueDefinition(HitWhooshMicroClipPaths, 0.36f, 1.02f, 1.1f, 0.92f, 1.04f, 0.02f, 8f, 40f, 72));

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void UpdateOrAddCue(
            SerializedProperty cues,
            CombatVfxCueId cueId,
            string prefabPath,
            Vector3 localPositionOffset,
            Vector3 localEulerOffset,
            Vector3 localScale,
            float lifetimeSeconds,
            bool parentToAnchor,
            bool alignForwardToDirection,
            AudioCueDefinition audioCue)
        {
            SerializedProperty cue = FindCue(cues, cueId);
            if (cue == null)
            {
                cues.arraySize++;
                cue = cues.GetArrayElementAtIndex(cues.arraySize - 1);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new FileNotFoundException($"Missing hit feedback VFX cue prefab at {prefabPath}.");
            }

            cue.FindPropertyRelative("cueId").intValue = (int)cueId;
            cue.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            cue.FindPropertyRelative("localPositionOffset").vector3Value = localPositionOffset;
            cue.FindPropertyRelative("localEulerOffset").vector3Value = localEulerOffset;
            cue.FindPropertyRelative("localScale").vector3Value = localScale;
            cue.FindPropertyRelative("lifetimeSeconds").floatValue = lifetimeSeconds;
            cue.FindPropertyRelative("prewarmCount").intValue = 0;
            cue.FindPropertyRelative("parentToAnchor").boolValue = parentToAnchor;
            cue.FindPropertyRelative("alignForwardToDirection").boolValue = alignForwardToDirection;
            SetCueAudio(cue, audioCue);
        }

        private static void UpdateCueAudio(
            SerializedProperty cues,
            CombatVfxCueId cueId,
            AudioCueDefinition audioCue)
        {
            SerializedProperty cue = FindCue(cues, cueId);
            if (cue == null)
            {
                return;
            }

            SetCueAudio(cue, audioCue);
        }

        private static void SetCueAudio(SerializedProperty cue, AudioCueDefinition audioCue)
        {
            SerializedProperty audioClips = cue.FindPropertyRelative("audioClips");
            audioClips.arraySize = audioCue.ClipPaths.Length;
            for (int i = 0; i < audioCue.ClipPaths.Length; i++)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioCue.ClipPaths[i]);
                if (clip == null)
                {
                    throw new FileNotFoundException($"Missing imported master hit SFX clip at {audioCue.ClipPaths[i]}.");
                }

                audioClips.GetArrayElementAtIndex(i).objectReferenceValue = clip;
            }

            cue.FindPropertyRelative("audioBaseVolume").floatValue = audioCue.BaseVolume;
            cue.FindPropertyRelative("audioMinimumPitch").floatValue = audioCue.MinimumPitch;
            cue.FindPropertyRelative("audioMaximumPitch").floatValue = audioCue.MaximumPitch;
            cue.FindPropertyRelative("audioMinimumVolumeMultiplier").floatValue = audioCue.MinimumVolumeMultiplier;
            cue.FindPropertyRelative("audioMaximumVolumeMultiplier").floatValue = audioCue.MaximumVolumeMultiplier;
            cue.FindPropertyRelative("audioSpatialBlend").floatValue = audioCue.SpatialBlend;
            cue.FindPropertyRelative("audioMinDistance").floatValue = audioCue.MinDistance;
            cue.FindPropertyRelative("audioMaxDistance").floatValue = audioCue.MaxDistance;
            cue.FindPropertyRelative("audioPriority").intValue = audioCue.Priority;
        }

        private static void RemoveInvalidCueEntries(SerializedProperty cues)
        {
            int validCueCount = Enum.GetValues(typeof(CombatVfxCueId)).Length;
            for (int i = cues.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty cue = cues.GetArrayElementAtIndex(i);
                int cueId = cue.FindPropertyRelative("cueId").intValue;
                if (cueId < 0 || cueId >= validCueCount)
                {
                    cues.DeleteArrayElementAtIndex(i);
                }
            }
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

        private static void ValidateImportedClips()
        {
            HashSet<string> importedPaths = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < ClipCopies.Length; i++)
            {
                ClipCopy clipCopy = ClipCopies[i];
                string assetPath = $"{TargetRoot}/{clipCopy.Category}/{clipCopy.TargetFileName}";
                importedPaths.Add(assetPath);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip == null)
                {
                    throw new FileNotFoundException($"Missing imported master hit SFX clip at {assetPath}.");
                }

                if (AssetImporter.GetAtPath(assetPath) is not AudioImporter importer)
                {
                    throw new InvalidOperationException($"Imported hit SFX clip is not using an AudioImporter: {assetPath}.");
                }

                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                if (!importer.forceToMono || !settings.preloadAudioData || settings.loadType != AudioClipLoadType.DecompressOnLoad)
                {
                    throw new InvalidOperationException($"Imported hit SFX clip has unexpected importer settings: {assetPath}.");
                }
            }

            ValidateAllImportedClipsAreAssigned(importedPaths);
        }

        private static void ValidateAllImportedClipsAreAssigned(HashSet<string> importedPaths)
        {
            HashSet<string> assignedPaths = new HashSet<string>(StringComparer.Ordinal);
            AddAssignedPaths(assignedPaths, EnemyHitCueClipPaths);
            AddAssignedPaths(assignedPaths, PlayerDamagedCueClipPaths);
            AddAssignedPaths(assignedPaths, PlayerCriticalClipPaths);
            AddAssignedPaths(assignedPaths, HitWhooshMicroClipPaths);

            foreach (string importedPath in importedPaths)
            {
                if (!assignedPaths.Contains(importedPath))
                {
                    throw new InvalidOperationException($"Imported master hit SFX clip is not assigned to a cue: {importedPath}.");
                }
            }
        }

        private static void AddAssignedPaths(HashSet<string> assignedPaths, string[] clipPaths)
        {
            for (int i = 0; i < clipPaths.Length; i++)
            {
                assignedPaths.Add(clipPaths[i]);
            }
        }

        private static void ValidateCueProfile()
        {
            CombatVfxCueProfile profile = AssetDatabase.LoadAssetAtPath<CombatVfxCueProfile>(ProfilePath);
            if (profile == null)
            {
                throw new FileNotFoundException($"Missing combat VFX cue profile at {ProfilePath}.");
            }

            ValidateCueProfileEntry(profile, CombatVfxCueId.EnemyHit, EnemyHitVisualPrefabPath, EnemyHitCueClipPaths.Length);
            ValidateCueProfileEntry(profile, CombatVfxCueId.PlayerDamaged, EnemyHitVisualPrefabPath, PlayerDamagedCueClipPaths.Length);
            ValidateCueProfileEntry(profile, CombatVfxCueId.PlayerCritical, EnemyHitVisualPrefabPath, PlayerCriticalClipPaths.Length);
            ValidateCueAudio(profile, CombatVfxCueId.PlayerRangedProjectileImpact, HitWhooshMicroClipPaths.Length);
        }

        private static void ValidateCueProfileEntry(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string prefabPath,
            int expectedAudioClipCount)
        {
            if (!profile.AllowsPlayback(cueId))
            {
                throw new InvalidOperationException($"Combat VFX cue profile playback mode blocks hit feedback cue {cueId}.");
            }

            if (!profile.TryGetCue(cueId, out CombatVfxCue cue))
            {
                throw new InvalidOperationException($"Combat VFX cue profile is missing hit feedback cue {cueId}.");
            }

            GameObject expectedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (expectedPrefab == null)
            {
                throw new FileNotFoundException($"Missing expected hit feedback cue prefab at {prefabPath}.");
            }

            if (cue.Prefab != expectedPrefab)
            {
                string actualPath = cue.Prefab != null ? AssetDatabase.GetAssetPath(cue.Prefab) : "<null>";
                throw new InvalidOperationException($"Combat VFX cue {cueId} points to {actualPath}, expected {prefabPath}.");
            }

            ValidateCueAudio(profile, cueId, expectedAudioClipCount);
        }

        private static void ValidateCueAudio(CombatVfxCueProfile profile, CombatVfxCueId cueId, int expectedAudioClipCount)
        {
            if (!profile.TryGetCue(cueId, out CombatVfxCue cue))
            {
                throw new InvalidOperationException($"Combat VFX cue profile is missing cue audio entry {cueId}.");
            }

            if (cue.AudioClipCount != expectedAudioClipCount || cue.AudioBaseVolume <= 0f)
            {
                throw new InvalidOperationException(
                    $"Combat VFX cue {cueId} has {cue.AudioClipCount} assigned audio clips and volume {cue.AudioBaseVolume}, expected {expectedAudioClipCount} clips with audible volume.");
            }

            for (int i = 0; i < cue.AudioClipCount; i++)
            {
                if (cue.GetAudioClip(i) == null)
                {
                    throw new InvalidOperationException($"Combat VFX cue {cueId} has a null audio clip at index {i}.");
                }
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            folderPath = folderPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private readonly struct ClipCopy
        {
            public ClipCopy(string sourceFileName, string category, string targetFileName)
            {
                SourceFileName = sourceFileName;
                Category = category;
                TargetFileName = targetFileName;
            }

            public string SourceFileName { get; }
            public string Category { get; }
            public string TargetFileName { get; }
        }

        private readonly struct AudioCueDefinition
        {
            public AudioCueDefinition(
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
                ClipPaths = clipPaths;
                BaseVolume = baseVolume;
                MinimumPitch = minimumPitch;
                MaximumPitch = maximumPitch;
                MinimumVolumeMultiplier = minimumVolumeMultiplier;
                MaximumVolumeMultiplier = maximumVolumeMultiplier;
                SpatialBlend = spatialBlend;
                MinDistance = minDistance;
                MaxDistance = maxDistance;
                Priority = priority;
            }

            public string[] ClipPaths { get; }
            public float BaseVolume { get; }
            public float MinimumPitch { get; }
            public float MaximumPitch { get; }
            public float MinimumVolumeMultiplier { get; }
            public float MaximumVolumeMultiplier { get; }
            public float SpatialBlend { get; }
            public float MinDistance { get; }
            public float MaxDistance { get; }
            public int Priority { get; }
        }
    }
}
