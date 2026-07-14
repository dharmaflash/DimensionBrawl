using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class CanonicalMobileContentImportOptimizer
    {
        private const string ReportPath = "C:/tmp/DimensionBrawl-CanonicalMobileContentImportOptimization.md";
        private const string AndroidPlatformName = "Android";
        private const string DimensionHudRoot = "Assets/_Game/UI/CombatHud/Art/DimensionHud/";
        private const string GameVfxRoot = "Assets/_Game/Art/VFX/";
        private const string GameAudioRoot = "Assets/_Game/Art/Audio/";
        private const string GameBgmRoot = "Assets/_Game/Art/Audio/BGM/";
        private const string GameCharacterRoot = "Assets/_Game/Art/Characters/";
        private const string GamePlayerCharacterRoot = "Assets/_Game/Art/Characters/Player/";
        private const string GameEnemyRoleWeaponRoot =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/";
        private const string GameEnvironmentRoot = "Assets/_Game/Art/Environment/";
        private const string GameCinematicVoiceRoot = "Assets/_Game/Art/Audio/Voice/Cinematics/";
        private const string CombatGirlAnimationRoot =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield";
        private const string CombatGirlRuntimeClipRoot =
            CombatGirlAnimationRoot + "/RuntimeClips";
        private const string CombatGirlControllerPath =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string CanonicalTmpMobileShaderPath =
            "Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader";
        private const string PretendardMediumFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_Medium_Dynamic.asset";
        private const string CanonicalInoriModelPath =
            "Assets/_Game/Art/Characters/Player/Inori/Models/Inori_Unity.fbx";

        private static readonly string[] CanonicalPretendardFontPaths =
        {
            PretendardMediumFontPath,
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_SemiBold_Dynamic.asset"
        };

        private static readonly string[] RuntimeScriptEditorIconPaths =
        {
            "Assets/_Imported/AssetStore/MagicaCloth2/Scripts/Core/Cloth/MagicaCloth.cs",
            "Assets/_Imported/AssetStore/MagicaCloth2/Scripts/Core/Cloth/Collider/MagicaSphereCollider.cs",
            "Assets/_Imported/AssetStore/MagicaCloth2/Scripts/Core/Cloth/Collider/MagicaPlaneCollider.cs",
            "Assets/_Imported/AssetStore/MagicaCloth2/Scripts/Core/Cloth/Collider/MagicaCapsuleCollider.cs"
        };

        private static readonly string[] ImportedCharacterAndWeaponRoots =
        {
            "Assets/_Imported/AssetStore/Protofactor/Sci Fi/",
            "Assets/_Imported/AssetStore/RoloArt/Inori/",
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack/",
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/",
            "Assets/_Imported/AssetStore/HEROIC FANTASY CREATURES FULL PACK VOL3/"
        };

        private static readonly string[] VfxRoots =
        {
            GameVfxRoot,
            "Assets/_Imported/AssetStore/VFX/",
            "Assets/_Imported/AssetStore/FORGE3D/Sci-Fi Effects/",
            "Assets/_Imported/SpecialSkillsEffectsPack/"
        };

        private static readonly TextureReferenceConsolidation[] TextureReferenceConsolidations =
        {
            new(
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/MissileLauncher/Textures/T_BazookaMagazine_BaseColor.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AircraftModels/T_BazookaMagazine_BaseColor_c58cc6a9.png",
                "Assets/_Game/Art/Materials/ActionFoundation/IntroGatePodBombingReview/AF_BombingReview_BombOriginal.mat"),
            new(
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AerialBomb/Explosion_6_0d04b1c2.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AerialBomb2/Explosion_6_0d04b1c2.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Materials/AerialBomb2/Effect_40_Explosion.mat"),
            new(
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AirstrikeBombExplosion/Explosion_1_9064114c.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/ShellExplosion/Explosion_1_9064114c.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Materials/ShellExplosion/Effect_10_Explosion.mat")
        };

        private static readonly string[] CanonicalScenePaths =
        {
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity"
        };

        // Unity packs every Resources folder. These vendor samples and starter font assets are not
        // loaded by game code, so preserve their GUIDs while removing the special folder behavior.
        private static readonly ResourceFolderRelocation[] NonRuntimeResourceFolders =
        {
            new(
                "Assets/_Imported/AssetStore/MagicaCloth2/Example (Can be deleted)/Common/Resources",
                "Assets/_Imported/AssetStore/MagicaCloth2/Example (Can be deleted)/Common/DemoResources"),
            new(
                "Assets/_Imported/AssetStore/VFX/Vefects/Combat Flipbook VFX/Demo/Resources",
                "Assets/_Imported/AssetStore/VFX/Vefects/Combat Flipbook VFX/Demo/DemoResources"),
            new(
                "Assets/_Imported/AssetStore/VFX/Vefects/Flipbook VFX/Demo/Resources",
                "Assets/_Imported/AssetStore/VFX/Vefects/Flipbook VFX/Demo/DemoResources"),
            new(
                "Assets/_Imported/AssetStore/VFX/Vefects/Pixel Craft VFX/Demo/Resources",
                "Assets/_Imported/AssetStore/VFX/Vefects/Pixel Craft VFX/Demo/DemoResources"),
            new(
                "Assets/_Imported/AssetStore/VFX/Vefects_ShotsVFXURP/Shots VFX URP/Demo/Resources",
                "Assets/_Imported/AssetStore/VFX/Vefects_ShotsVFXURP/Shots VFX URP/Demo/DemoResources"),
            new(
                "Assets/_Imported/AssetStore/FORGE3D/Resources",
                "Assets/_Imported/AssetStore/FORGE3D/SourceResources"),
            new(
                "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/UI_&_environmental/TextMesh Pro/Resources",
                "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/UI_&_environmental/TextMesh Pro/SourceResources"),
            new(
                "Assets/_Imported/AssetStore/VFX/ShapesFX_Pack/Resources",
                "Assets/_Imported/AssetStore/VFX/ShapesFX_Pack/SourceResources"),
            new(
                "Assets/TextMesh Pro/Resources/Fonts & Materials",
                "Assets/TextMesh Pro/Default Font Assets")
        };

        [MenuItem("DimensionBrawl/Performance/Apply Canonical Mobile Content Import Budgets")]
        public static void ApplyMenuOptimization()
        {
            ApplyBatchOptimization();
        }

        public static void ApplyBatchOptimization()
        {
            List<string> canonicalReferenceChanges = EnsureCanonicalTmpRuntimeAssets();
            List<string> resourceRelocations = RelocateNonRuntimeResources();
            canonicalReferenceChanges.AddRange(EnsureCanonicalCharacterMeshMemoryBudgets());
            canonicalReferenceChanges.AddRange(EnsureCanonicalTextureReferenceConsolidation());
            canonicalReferenceChanges.AddRange(EnsureCombatGirlRuntimeAnimationClips());
            canonicalReferenceChanges.AddRange(StripRuntimeScriptEditorIconDependencies());
            HashSet<string> dependencyPaths = CollectCanonicalDependencies(out int resourcesAssetCount);
            List<TextureDecision> textureDecisions = CollectTextureDecisions(dependencyPaths);
            List<AudioDecision> audioDecisions = CollectAudioDecisions(dependencyPaths);
            OptimizationReport report = new()
            {
                GeneratedUtc = DateTime.UtcNow.ToString("O"),
                UnityVersion = Application.unityVersion,
                DependencyCount = dependencyPaths.Count,
                ResourcesAssetCount = resourcesAssetCount,
                TextureCandidateCount = textureDecisions.Count,
                AudioCandidateCount = audioDecisions.Count
            };
            report.ResourceFolderRelocationCount = resourceRelocations.Count;
            report.Changes.AddRange(resourceRelocations);
            report.Changes.AddRange(canonicalReferenceChanges);

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < textureDecisions.Count; i++)
                {
                    ApplyTextureDecision(textureDecisions[i], report);
                }

                for (int i = 0; i < audioDecisions.Count; i++)
                {
                    ApplyAudioDecision(audioDecisions[i], report);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            WriteReport(report);
            Debug.Log(
                $"Canonical mobile content import optimization complete. " +
                $"Relocated {report.ResourceFolderRelocationCount} non-runtime Resources folder(s), " +
                $"Changed {report.ChangedTextureCount} texture(s) and {report.ChangedAudioCount} audio clip(s). " +
                $"Report: {ReportPath}");
        }

        private static List<string> EnsureCanonicalTmpRuntimeAssets()
        {
            var changes = new List<string>();
            Shader canonicalShader = AssetDatabase.LoadAssetAtPath<Shader>(CanonicalTmpMobileShaderPath);
            if (canonicalShader == null)
            {
                throw new InvalidOperationException(
                    $"Canonical TMP mobile shader is missing: {CanonicalTmpMobileShaderPath}");
            }

            TMP_FontAsset mediumFont = null;
            for (int i = 0; i < CanonicalPretendardFontPaths.Length; i++)
            {
                string fontPath = CanonicalPretendardFontPaths[i];
                TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
                if (fontAsset == null)
                {
                    throw new InvalidOperationException($"Canonical Pretendard font is missing: {fontPath}");
                }

                if (fontPath == PretendardMediumFontPath)
                {
                    mediumFont = fontAsset;
                }

                Material material = fontAsset.material;
                if (material == null)
                {
                    throw new InvalidOperationException($"Canonical Pretendard font has no material: {fontPath}");
                }

                if (material.shader != canonicalShader)
                {
                    throw new InvalidOperationException(
                        $"Canonical Pretendard font must use {CanonicalTmpMobileShaderPath}: {fontPath}");
                }

                var serializedFont = new SerializedObject(fontAsset);
                SerializedProperty getFontFeatures = serializedFont.FindProperty("m_GetFontFeatures");
                if (getFontFeatures == null || getFontFeatures.boolValue)
                {
                    throw new InvalidOperationException(
                        $"Canonical Pretendard font must disable runtime font feature extraction: {fontPath}");
                }
            }

            TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
            if (settings == null || mediumFont == null)
            {
                throw new InvalidOperationException($"TMP runtime settings are incomplete: {TmpSettingsPath}");
            }

            var serializedSettings = new SerializedObject(settings);
            SerializedProperty defaultFont = serializedSettings.FindProperty("m_defaultFontAsset");
            SerializedProperty defaultFontPath = serializedSettings.FindProperty("m_defaultFontAssetPath");
            SerializedProperty getFontFeaturesAtRuntime =
                serializedSettings.FindProperty("m_GetFontFeaturesAtRuntime");
            if (defaultFont == null || defaultFontPath == null || getFontFeaturesAtRuntime == null)
            {
                throw new InvalidOperationException("TMP Settings serialization contract changed.");
            }

            bool settingsChanged = defaultFont.objectReferenceValue != mediumFont
                || !string.IsNullOrEmpty(defaultFontPath.stringValue)
                || getFontFeaturesAtRuntime.boolValue;
            if (settingsChanged)
            {
                defaultFont.objectReferenceValue = mediumFont;
                defaultFontPath.stringValue = string.Empty;
                getFontFeaturesAtRuntime.boolValue = false;
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
                changes.Add(
                    $"TMP | runtime default assigned without vendor fallback or feature-table mutation | " +
                    $"`{PretendardMediumFontPath}`");
            }

            if (changes.Count > 0)
            {
                AssetDatabase.SaveAssets();
            }

            return changes;
        }

        private static List<string> EnsureCanonicalCharacterMeshMemoryBudgets()
        {
            var changes = new List<string>();
            ModelImporter importer = AssetImporter.GetAtPath(CanonicalInoriModelPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Canonical Inori model importer is missing: {CanonicalInoriModelPath}");
            }

            if (importer.importAnimation)
            {
                throw new InvalidOperationException(
                    $"Canonical Inori model unexpectedly contains runtime animation data: {CanonicalInoriModelPath}");
            }

            if (!importer.importBlendShapes)
            {
                throw new InvalidOperationException(
                    $"Canonical Inori model must retain facial blend shapes: {CanonicalInoriModelPath}");
            }

            if (!importer.isReadable)
            {
                return changes;
            }

            importer.isReadable = false;
            importer.SaveAndReimport();
            changes.Add(
                $"Mesh | audited skinned character CPU copy disabled; blend shapes and GPU skinning retained | `{CanonicalInoriModelPath}`");
            return changes;
        }

        private static List<string> EnsureCanonicalTextureReferenceConsolidation()
        {
            var changes = new List<string>();
            for (int consolidationIndex = 0;
                 consolidationIndex < TextureReferenceConsolidations.Length;
                 consolidationIndex++)
            {
                TextureReferenceConsolidation consolidation =
                    TextureReferenceConsolidations[consolidationIndex];
                Texture canonical = AssetDatabase.LoadAssetAtPath<Texture>(
                    consolidation.CanonicalTexturePath);
                Texture duplicate = AssetDatabase.LoadAssetAtPath<Texture>(
                    consolidation.DuplicateTexturePath);
                Material consumer = AssetDatabase.LoadAssetAtPath<Material>(
                    consolidation.ConsumerMaterialPath);
                if (canonical == null || duplicate == null || consumer == null)
                {
                    throw new InvalidOperationException(
                        $"Canonical texture consolidation asset is missing: " +
                        $"{consolidation.CanonicalTexturePath}, " +
                        $"{consolidation.DuplicateTexturePath}, " +
                        $"{consolidation.ConsumerMaterialPath}");
                }

                string[] propertyNames = consumer.GetTexturePropertyNames();
                bool foundOwnedTexture = false;
                bool needsReplacement = false;
                for (int propertyIndex = 0; propertyIndex < propertyNames.Length; propertyIndex++)
                {
                    Texture current = consumer.GetTexture(propertyNames[propertyIndex]);
                    if (current == canonical || current == duplicate)
                    {
                        foundOwnedTexture = true;
                    }

                    needsReplacement |= current == duplicate;
                }

                if (!foundOwnedTexture)
                {
                    throw new InvalidOperationException(
                        $"Canonical consumer no longer owns either reviewed texture: " +
                        $"{consolidation.ConsumerMaterialPath}");
                }

                if (!needsReplacement)
                {
                    continue;
                }

                if (!FilesAreByteIdentical(
                        consolidation.CanonicalTexturePath,
                        consolidation.DuplicateTexturePath)
                    || !ImporterMetadataEquivalent(
                        consolidation.CanonicalTexturePath,
                        consolidation.DuplicateTexturePath))
                {
                    throw new InvalidOperationException(
                        $"Reviewed duplicate texture payload or importer policy diverged: " +
                        $"{consolidation.DuplicateTexturePath}");
                }

                int replacementCount = 0;
                for (int propertyIndex = 0; propertyIndex < propertyNames.Length; propertyIndex++)
                {
                    string propertyName = propertyNames[propertyIndex];
                    if (consumer.GetTexture(propertyName) != duplicate)
                    {
                        continue;
                    }

                    consumer.SetTexture(propertyName, canonical);
                    replacementCount++;
                }

                if (replacementCount <= 0)
                {
                    throw new InvalidOperationException(
                        $"Duplicate texture reference could not be replaced: " +
                        $"{consolidation.ConsumerMaterialPath}");
                }

                EditorUtility.SetDirty(consumer);
                changes.Add(
                    $"Texture | {replacementCount} byte-identical material reference(s) consolidated | " +
                    $"`{consolidation.ConsumerMaterialPath}` -> `{consolidation.CanonicalTexturePath}`");
            }

            if (changes.Count > 0)
            {
                AssetDatabase.SaveAssets();
            }

            return changes;
        }

        private static bool FilesAreByteIdentical(string leftPath, string rightPath)
        {
            var leftInfo = new FileInfo(leftPath);
            var rightInfo = new FileInfo(rightPath);
            if (!leftInfo.Exists || !rightInfo.Exists || leftInfo.Length != rightInfo.Length)
            {
                return false;
            }

            using FileStream left = File.OpenRead(leftPath);
            using FileStream right = File.OpenRead(rightPath);
            var leftBuffer = new byte[64 * 1024];
            var rightBuffer = new byte[leftBuffer.Length];
            while (true)
            {
                int leftRead = left.Read(leftBuffer, 0, leftBuffer.Length);
                int rightRead = right.Read(rightBuffer, 0, rightBuffer.Length);
                if (leftRead != rightRead)
                {
                    return false;
                }

                if (leftRead == 0)
                {
                    return true;
                }

                for (int i = 0; i < leftRead; i++)
                {
                    if (leftBuffer[i] != rightBuffer[i])
                    {
                        return false;
                    }
                }
            }
        }

        private static bool ImporterMetadataEquivalent(string leftPath, string rightPath)
        {
            return string.Equals(
                NormalizeImporterMetadata(leftPath + ".meta"),
                NormalizeImporterMetadata(rightPath + ".meta"),
                StringComparison.Ordinal);
        }

        private static string NormalizeImporterMetadata(string metaPath)
        {
            if (!File.Exists(metaPath))
            {
                return string.Empty;
            }

            string[] lines = File.ReadAllLines(metaPath);
            var builder = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("guid: ", StringComparison.Ordinal))
                {
                    continue;
                }

                builder.AppendLine(lines[i]);
            }

            return builder.ToString();
        }

        private static List<string> StripRuntimeScriptEditorIconDependencies()
        {
            var changes = new List<string>();
            for (int i = 0; i < RuntimeScriptEditorIconPaths.Length; i++)
            {
                string scriptPath = RuntimeScriptEditorIconPaths[i];
                MonoImporter importer = AssetImporter.GetAtPath(scriptPath) as MonoImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"Runtime script importer is missing: {scriptPath}");
                }

                Texture2D icon = importer.GetIcon();
                if (icon == null)
                {
                    continue;
                }

                string iconPath = AssetDatabase.GetAssetPath(icon).Replace('\\', '/');
                importer.SetIcon(null);
                importer.SaveAndReimport();
                changes.Add(
                    $"Script | editor-only icon removed from runtime dependency | `{scriptPath}` (was `{iconPath}`)");
            }

            return changes;
        }

        private static List<string> EnsureCombatGirlRuntimeAnimationClips()
        {
            var changes = new List<string>();
            if (!AssetDatabase.IsValidFolder(CombatGirlRuntimeClipRoot))
            {
                AssetDatabase.CreateFolder(CombatGirlAnimationRoot, "RuntimeClips");
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CombatGirlControllerPath);
            if (controller == null)
            {
                throw new InvalidOperationException(
                    $"CombatGirl Animator Controller is missing: {CombatGirlControllerPath}");
            }

            var sourceClipsByKey = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
            var runtimeClipsBySource = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
            string[] sourcePaths = Directory.GetFiles(
                CombatGirlAnimationRoot,
                "*.fbx",
                SearchOption.TopDirectoryOnly);
            for (int sourceIndex = 0; sourceIndex < sourcePaths.Length; sourceIndex++)
            {
                string sourcePath = sourcePaths[sourceIndex].Replace('\\', '/');

                AnimationClip sourceClip = LoadPrimaryAnimationClip(sourcePath);
                if (sourceClip == null)
                {
                    continue;
                }

                sourceClipsByKey[RuntimeClipSourceKey(sourcePath, sourceClip.name)] = sourceClip;
            }

            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                CollectFbxMotions(layers[i].stateMachine, sourceClipsByKey);
            }

            foreach (KeyValuePair<string, AnimationClip> pair in sourceClipsByKey)
            {
                AnimationClip sourceClip = pair.Value;
                string sourcePath = AssetDatabase.GetAssetPath(sourceClip).Replace('\\', '/');

                string runtimePath = $"{CombatGirlRuntimeClipRoot}/{sourceClip.name}.anim";
                AnimationClip runtimeClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(runtimePath);
                bool created = runtimeClip == null;
                if (created)
                {
                    runtimeClip = new AnimationClip();
                    EditorUtility.CopySerialized(sourceClip, runtimeClip);
                    runtimeClip.name = sourceClip.name;
                    runtimeClip.hideFlags = HideFlags.None;
                    AssetDatabase.CreateAsset(runtimeClip, runtimePath);
                }
                else
                {
                    EditorUtility.CopySerializedIfDifferent(sourceClip, runtimeClip);
                    runtimeClip.name = sourceClip.name;
                }

                runtimeClipsBySource[pair.Key] = runtimeClip;
                if (created || EditorUtility.IsDirty(runtimeClip))
                {
                    changes.Add(
                        $"Animation | native runtime clip extracted from FBX | `{sourcePath}` -> `{runtimePath}`");
                }
            }

            int replacedMotionCount = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                replacedMotionCount += ReplaceRuntimeMotions(
                    layers[i].stateMachine,
                    runtimeClipsBySource);
            }

            if (replacedMotionCount > 0)
            {
                EditorUtility.SetDirty(controller);
                changes.Add(
                    $"Animator | replaced {replacedMotionCount} CombatGirl FBX motion reference(s) with native runtime clips | `{CombatGirlControllerPath}`");
            }

            AssetDatabase.SaveAssets();
            string[] controllerDependencies = AssetDatabase.GetDependencies(
                CombatGirlControllerPath,
                recursive: true);
            for (int i = 0; i < controllerDependencies.Length; i++)
            {
                if (controllerDependencies[i].StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"CombatGirl controller still references imported asset {controllerDependencies[i]}.");
                }
            }

            return changes;
        }

        private static AnimationClip LoadPrimaryAnimationClip(string assetPath)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip
                    && !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            return null;
        }

        private static void CollectFbxMotions(
            AnimatorStateMachine stateMachine,
            IDictionary<string, AnimationClip> sourceClipsByKey)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                CollectFbxMotion(states[i].state.motion, sourceClipsByKey);
            }

            ChildAnimatorStateMachine[] childStateMachines = stateMachine.stateMachines;
            for (int i = 0; i < childStateMachines.Length; i++)
            {
                CollectFbxMotions(childStateMachines[i].stateMachine, sourceClipsByKey);
            }
        }

        private static void CollectFbxMotion(
            Motion motion,
            IDictionary<string, AnimationClip> sourceClipsByKey)
        {
            if (motion is AnimationClip clip)
            {
                string path = AssetDatabase.GetAssetPath(clip).Replace('\\', '/');
                if (path.StartsWith(CombatGirlAnimationRoot + "/", StringComparison.Ordinal)
                    && path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                {
                    sourceClipsByKey[RuntimeClipSourceKey(path, clip.name)] = clip;
                }

                return;
            }

            if (motion is not BlendTree blendTree)
            {
                return;
            }

            ChildMotion[] children = blendTree.children;
            for (int i = 0; i < children.Length; i++)
            {
                CollectFbxMotion(children[i].motion, sourceClipsByKey);
            }
        }

        private static int ReplaceRuntimeMotions(
            AnimatorStateMachine stateMachine,
            IReadOnlyDictionary<string, AnimationClip> runtimeClipsBySource)
        {
            int replacedCount = 0;
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                Motion motion = states[i].state.motion;
                Motion replacement = ReplaceRuntimeMotion(
                    motion,
                    runtimeClipsBySource,
                    ref replacedCount);
                if (replacement != motion)
                {
                    states[i].state.motion = replacement;
                    EditorUtility.SetDirty(states[i].state);
                }
            }

            ChildAnimatorStateMachine[] childStateMachines = stateMachine.stateMachines;
            for (int i = 0; i < childStateMachines.Length; i++)
            {
                replacedCount += ReplaceRuntimeMotions(
                    childStateMachines[i].stateMachine,
                    runtimeClipsBySource);
            }

            return replacedCount;
        }

        private static Motion ReplaceRuntimeMotion(
            Motion motion,
            IReadOnlyDictionary<string, AnimationClip> runtimeClipsBySource,
            ref int replacedCount)
        {
            if (motion is AnimationClip clip)
            {
                string sourcePath = AssetDatabase.GetAssetPath(clip).Replace('\\', '/');
                string sourceKey = RuntimeClipSourceKey(sourcePath, clip.name);
                if (runtimeClipsBySource.TryGetValue(sourceKey, out AnimationClip runtimeClip))
                {
                    replacedCount++;
                    return runtimeClip;
                }
            }

            if (motion is not BlendTree blendTree)
            {
                return motion;
            }

            ChildMotion[] children = blendTree.children;
            bool changed = false;
            for (int i = 0; i < children.Length; i++)
            {
                Motion replacement = ReplaceRuntimeMotion(
                    children[i].motion,
                    runtimeClipsBySource,
                    ref replacedCount);
                if (replacement == children[i].motion)
                {
                    continue;
                }

                children[i].motion = replacement;
                changed = true;
            }

            if (changed)
            {
                blendTree.children = children;
                EditorUtility.SetDirty(blendTree);
            }

            return blendTree;
        }

        private static string RuntimeClipSourceKey(string sourcePath, string clipName)
        {
            return sourcePath + "|" + clipName;
        }

        private static List<string> RelocateNonRuntimeResources()
        {
            List<string> changes = new();
            for (int i = 0; i < NonRuntimeResourceFolders.Length; i++)
            {
                ResourceFolderRelocation relocation = NonRuntimeResourceFolders[i];
                bool sourceExists = AssetDatabase.IsValidFolder(relocation.SourcePath);
                bool destinationExists = AssetDatabase.IsValidFolder(relocation.DestinationPath);
                if (!sourceExists)
                {
                    continue;
                }

                if (destinationExists)
                {
                    throw new InvalidOperationException(
                        $"Cannot relocate demo Resources because both paths exist: " +
                        $"{relocation.SourcePath} and {relocation.DestinationPath}");
                }

                string error = AssetDatabase.MoveAsset(relocation.SourcePath, relocation.DestinationPath);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new InvalidOperationException(
                        $"Failed to relocate demo Resources folder {relocation.SourcePath}: {error}");
                }

                changes.Add(
                    $"Resources | non-runtime root relocated with GUIDs preserved | " +
                    $"`{relocation.SourcePath}` -> `{relocation.DestinationPath}`");
            }

            if (changes.Count > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            return changes;
        }

        private static HashSet<string> CollectCanonicalDependencies(out int resourcesAssetCount)
        {
            HashSet<string> dependencies = new(StringComparer.Ordinal);
            for (int i = 0; i < CanonicalScenePaths.Length; i++)
            {
                string scenePath = CanonicalScenePaths[i];
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    throw new InvalidOperationException($"Canonical scene is missing: {scenePath}");
                }

                AddDependencies(scenePath, dependencies);
            }

            resourcesAssetCount = 0;
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string path = allAssetPaths[i];
                if (!IsRuntimeResourcesAsset(path))
                {
                    continue;
                }

                resourcesAssetCount++;
                AddDependencies(path, dependencies);
            }

            return dependencies;
        }

        private static void AddDependencies(string assetPath, HashSet<string> dependencies)
        {
            string[] assetDependencies = AssetDatabase.GetDependencies(assetPath, recursive: true);
            for (int i = 0; i < assetDependencies.Length; i++)
            {
                dependencies.Add(assetDependencies[i]);
            }
        }

        private static bool IsRuntimeResourcesAsset(string path)
        {
            return path.StartsWith("Assets/", StringComparison.Ordinal)
                && path.IndexOf("/Resources/", StringComparison.Ordinal) >= 0
                && path.IndexOf("/Editor/", StringComparison.Ordinal) < 0
                && !AssetDatabase.IsValidFolder(path);
        }

        private static List<TextureDecision> CollectTextureDecisions(HashSet<string> dependencies)
        {
            List<TextureDecision> decisions = new();
            foreach (string path in dependencies)
            {
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                {
                    continue;
                }

                importer.GetSourceTextureWidthAndHeight(out int sourceWidth, out int sourceHeight);
                int largestAxis = Math.Max(sourceWidth, sourceHeight);
                bool dimensionHud = path.StartsWith(DimensionHudRoot, StringComparison.Ordinal);
                bool vfx = IsVfxTexture(path);
                bool largeVfx = vfx && largestAxis >= 2048;
                bool enemyRoleWeaponBaseColor = IsEnemyRoleWeaponBaseColor(path)
                    && largestAxis >= 2048;
                bool characterAuxiliary = IsCharacterAuxiliaryTexture(path) && largestAxis >= 2048;
                bool environmentAuxiliary = IsEnvironmentAuxiliaryTexture(path) && largestAxis >= 2048;
                bool largeCanonicalTexture = largestAxis >= 4096;
                bool removeVfxReadWrite = vfx && importer.isReadable;
                bool streamableLargeTexture = largestAxis >= 2048
                    && importer.mipmapEnabled
                    && !dimensionHud
                    && !vfx
                    && (importer.textureType == TextureImporterType.Default
                        || importer.textureType == TextureImporterType.NormalMap);
                TextureImporterPlatformSettings currentAndroid =
                    importer.GetPlatformTextureSettings(AndroidPlatformName);
                bool needsExplicitAndroidBudget = streamableLargeTexture && !currentAndroid.overridden;
                if (!dimensionHud
                    && !largeVfx
                    && !enemyRoleWeaponBaseColor
                    && !characterAuxiliary
                    && !environmentAuxiliary
                    && !largeCanonicalTexture
                    && !removeVfxReadWrite
                    && !streamableLargeTexture)
                {
                    continue;
                }

                int androidMaxSize = 0;
                int compressionQuality = 80;
                string budgetReason = string.Empty;
                if (dimensionHud)
                {
                    androidMaxSize = 1024;
                    compressionQuality = 100;
                    budgetReason = "Dimension HUD automatic Android compression with source resolution preserved";
                }
                else if (largeVfx)
                {
                    androidMaxSize = 1024;
                    budgetReason = "Canonical large VFX texture capped at 1K on Android";
                }
                else if (enemyRoleWeaponBaseColor)
                {
                    androidMaxSize = 1024;
                    budgetReason = "Enemy role-weapon base color capped at 1K on Android";
                }
                else if (characterAuxiliary)
                {
                    androidMaxSize = 1024;
                    budgetReason = "Character auxiliary texture capped at 1K on Android";
                }
                else if (environmentAuxiliary)
                {
                    androidMaxSize = 1024;
                    budgetReason = "Environment ID, mask, and normal detail capped at 1K on Android";
                }
                else if (largeCanonicalTexture)
                {
                    androidMaxSize = 2048;
                    compressionQuality = 100;
                    budgetReason = "Canonical 4K texture capped at 2K on Android";
                }
                else if (needsExplicitAndroidBudget)
                {
                    androidMaxSize = Math.Min(2048, largestAxis);
                    budgetReason = "Canonical 2K texture receives an explicit Android source-resolution budget";
                }

                decisions.Add(new TextureDecision(
                    path,
                    sourceWidth,
                    sourceHeight,
                    androidMaxSize,
                    compressionQuality,
                    dimensionHud,
                    removeVfxReadWrite,
                    streamableLargeTexture,
                    budgetReason));
            }

            decisions.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.Ordinal));
            return decisions;
        }

        private static bool IsVfxTexture(string path)
        {
            for (int i = 0; i < VfxRoots.Length; i++)
            {
                if (path.StartsWith(VfxRoots[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsEnemyRoleWeaponBaseColor(string path)
        {
            return path.StartsWith(GameEnemyRoleWeaponRoot, StringComparison.Ordinal)
                && Path.GetFileNameWithoutExtension(path)
                    .IndexOf("BaseColor", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsCharacterAuxiliaryTexture(string path)
        {
            bool characterOrWeapon = path.StartsWith(GameCharacterRoot, StringComparison.Ordinal);
            for (int i = 0; !characterOrWeapon && i < ImportedCharacterAndWeaponRoots.Length; i++)
            {
                characterOrWeapon = path.StartsWith(
                    ImportedCharacterAndWeaponRoots[i],
                    StringComparison.Ordinal);
            }

            if (!characterOrWeapon)
            {
                return false;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            bool normal = name.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0;
            if (normal && path.StartsWith(GamePlayerCharacterRoot, StringComparison.Ordinal))
            {
                return false;
            }

            return normal
                || name.IndexOf("Metallic", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Occlusion", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Emissive", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Spec", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Mask", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Rim", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Outline", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsEnvironmentAuxiliaryTexture(string path)
        {
            if (!path.StartsWith(GameEnvironmentRoot, StringComparison.Ordinal))
            {
                return false;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            return name.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("_ID", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Mask", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Metallic", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Occlusion", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static List<AudioDecision> CollectAudioDecisions(HashSet<string> dependencies)
        {
            List<AudioDecision> decisions = new();
            foreach (string path in dependencies)
            {
                if (!path.StartsWith(GameAudioRoot, StringComparison.Ordinal)
                    || AssetImporter.GetAtPath(path) is not AudioImporter)
                {
                    continue;
                }

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null)
                {
                    continue;
                }

                bool ambience = path.IndexOf("/Ambience/", StringComparison.Ordinal) >= 0;
                bool longMusic = path.StartsWith(GameBgmRoot, StringComparison.Ordinal)
                    || path.EndsWith("/BGM.mp3", StringComparison.OrdinalIgnoreCase);
                bool cinematicVoice = path.StartsWith(GameCinematicVoiceRoot, StringComparison.Ordinal)
                    && clip.length >= 10f;
                bool stream = (ambience && clip.length >= 30f)
                    || (longMusic && clip.length >= 60f)
                    || cinematicVoice;
                string fullPath = Path.GetFullPath(path);
                long sourceBytes = File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0L;
                bool compressedOneShot = !stream
                    && (clip.length >= 10f || sourceBytes >= 1024L * 1024L);
                if (!stream && !compressedOneShot)
                {
                    continue;
                }

                decisions.Add(new AudioDecision(path, clip.length, stream));
            }

            decisions.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.Ordinal));
            return decisions;
        }

        private static void ApplyTextureDecision(TextureDecision decision, OptimizationReport report)
        {
            TextureImporter importer = AssetImporter.GetAtPath(decision.AssetPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Texture importer disappeared: {decision.AssetPath}");
            }

            bool changed = false;
            if (decision.EnsureCompressed
                && importer.textureCompression != TextureImporterCompression.CompressedHQ)
            {
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                changed = true;
            }

            if (decision.RemoveReadWrite && importer.isReadable)
            {
                importer.isReadable = false;
                changed = true;
            }

            if (decision.EnableStreamingMipmaps && !importer.streamingMipmaps)
            {
                importer.streamingMipmaps = true;
                changed = true;
            }

            if (decision.AndroidMaxSize > 0)
            {
                TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings(AndroidPlatformName);
                bool platformChanged = !android.overridden
                    || android.maxTextureSize != decision.AndroidMaxSize
                    || android.format != TextureImporterFormat.Automatic
                    || android.compressionQuality != decision.CompressionQuality;
                if (platformChanged)
                {
                    android.name = AndroidPlatformName;
                    android.overridden = true;
                    android.maxTextureSize = decision.AndroidMaxSize;
                    android.format = TextureImporterFormat.Automatic;
                    android.compressionQuality = decision.CompressionQuality;
                    importer.SetPlatformTextureSettings(android);
                    changed = true;
                }
            }

            string details = BuildTextureDetails(decision);
            if (changed)
            {
                EditorUtility.SetDirty(importer);
                AssetDatabase.WriteImportSettingsIfDirty(decision.AssetPath);
                report.ChangedTextureCount++;
                report.Changes.Add($"Texture | {details} | `{decision.AssetPath}`");
            }
            else
            {
                report.CompliantTextureCount++;
            }
        }

        private static void ApplyAudioDecision(AudioDecision decision, OptimizationReport report)
        {
            AudioImporter importer = AssetImporter.GetAtPath(decision.AssetPath) as AudioImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Audio importer disappeared: {decision.AssetPath}");
            }

            AudioImporterSampleSettings android = importer.ContainsSampleSettingsOverride(AndroidPlatformName)
                ? importer.GetOverrideSampleSettings(AndroidPlatformName)
                : importer.defaultSampleSettings;
            AudioClipLoadType loadType = decision.Stream
                ? AudioClipLoadType.Streaming
                : AudioClipLoadType.CompressedInMemory;
            AudioCompressionFormat compressionFormat = decision.Stream
                ? AudioCompressionFormat.Vorbis
                : AudioCompressionFormat.ADPCM;
            float quality = decision.Stream ? 0.45f : 0.7f;
            bool preloadAudioData = !decision.Stream;
            bool settingsChanged = android.loadType != loadType
                || android.compressionFormat != compressionFormat
                || !Mathf.Approximately(android.quality, quality)
                || android.sampleRateSetting != AudioSampleRateSetting.OptimizeSampleRate
                || android.preloadAudioData != preloadAudioData;
            bool changed = settingsChanged || !importer.loadInBackground;
            if (settingsChanged)
            {
                android.loadType = loadType;
                android.compressionFormat = compressionFormat;
                android.quality = quality;
                android.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
                android.preloadAudioData = preloadAudioData;
                importer.SetOverrideSampleSettings(AndroidPlatformName, android);
            }

            if (!importer.loadInBackground)
            {
                importer.loadInBackground = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(importer);
                AssetDatabase.WriteImportSettingsIfDirty(decision.AssetPath);
                report.ChangedAudioCount++;
                report.Changes.Add(
                    decision.Stream
                        ? $"Audio | {decision.LengthSeconds:0.0}s, Android Streaming Vorbis q0.45 | `{decision.AssetPath}`"
                        : $"Audio | {decision.LengthSeconds:0.0}s, Android CompressedInMemory ADPCM | `{decision.AssetPath}`");
            }
            else
            {
                report.CompliantAudioCount++;
            }
        }

        private static string BuildTextureDetails(TextureDecision decision)
        {
            List<string> details = new();
            if (!string.IsNullOrEmpty(decision.BudgetReason))
            {
                details.Add(decision.BudgetReason);
            }

            if (decision.RemoveReadWrite)
            {
                details.Add("Read/Write disabled");
            }

            if (decision.EnableStreamingMipmaps)
            {
                details.Add("mipmap streaming enabled");
            }

            details.Add($"source {decision.SourceWidth}x{decision.SourceHeight}");
            return string.Join(", ", details);
        }

        private static void WriteReport(OptimizationReport report)
        {
            StringBuilder builder = new();
            builder.AppendLine("# DimensionBrawl Canonical Mobile Content Import Optimization");
            builder.AppendLine();
            builder.AppendLine($"- Generated UTC: {report.GeneratedUtc}");
            builder.AppendLine($"- Unity: {report.UnityVersion}");
            builder.AppendLine($"- Canonical and runtime Resources dependencies: {report.DependencyCount:N0}");
            builder.AppendLine($"- Runtime Resources root assets: {report.ResourcesAssetCount:N0}");
            builder.AppendLine($"- Non-runtime Resources folders relocated: {report.ResourceFolderRelocationCount:N0}");
            builder.AppendLine($"- Texture candidates: {report.TextureCandidateCount:N0}");
            builder.AppendLine($"- Textures changed: {report.ChangedTextureCount:N0}");
            builder.AppendLine($"- Textures already compliant: {report.CompliantTextureCount:N0}");
            builder.AppendLine($"- Audio candidates: {report.AudioCandidateCount:N0}");
            builder.AppendLine($"- Audio clips changed: {report.ChangedAudioCount:N0}");
            builder.AppendLine($"- Audio clips already compliant: {report.CompliantAudioCount:N0}");
            builder.AppendLine("- Texture policy: Android-only automatic compression; 4K canonical/Resources textures cap at 2K, large VFX, enemy role-weapon base colors, and character/environment auxiliary maps cap at 1K, and 2K mipped Default/Normal textures stream while player base color and Dimension HUD retain their source budget.");
            builder.AppendLine("- Mesh policy: the canonical Inori model keeps its authored geometry, skinning, and blend shapes while dropping the audited unused CPU-readable mesh copy.");
            builder.AppendLine("- Duplicate policy: canonical runtime materials share byte-identical texture payloads only when source bytes and complete importer metadata match.");
            builder.AppendLine("- Audio policy: long ambience/BGM/cinematic voice streams in the background; large combat one-shots stay preloaded as low-decode-cost ADPCM compressed data.");
            builder.AppendLine();
            builder.AppendLine("## Changes");
            builder.AppendLine();
            for (int i = 0; i < report.Changes.Count; i++)
            {
                builder.AppendLine($"- {report.Changes[i]}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "C:/tmp");
            File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        }

        private readonly struct TextureDecision
        {
            public TextureDecision(
                string assetPath,
                int sourceWidth,
                int sourceHeight,
                int androidMaxSize,
                int compressionQuality,
                bool ensureCompressed,
                bool removeReadWrite,
                bool enableStreamingMipmaps,
                string budgetReason)
            {
                AssetPath = assetPath;
                SourceWidth = sourceWidth;
                SourceHeight = sourceHeight;
                AndroidMaxSize = androidMaxSize;
                CompressionQuality = compressionQuality;
                EnsureCompressed = ensureCompressed;
                RemoveReadWrite = removeReadWrite;
                EnableStreamingMipmaps = enableStreamingMipmaps;
                BudgetReason = budgetReason;
            }

            public string AssetPath { get; }
            public int SourceWidth { get; }
            public int SourceHeight { get; }
            public int AndroidMaxSize { get; }
            public int CompressionQuality { get; }
            public bool EnsureCompressed { get; }
            public bool RemoveReadWrite { get; }
            public bool EnableStreamingMipmaps { get; }
            public string BudgetReason { get; }
        }

        private readonly struct AudioDecision
        {
            public AudioDecision(string assetPath, float lengthSeconds, bool stream)
            {
                AssetPath = assetPath;
                LengthSeconds = lengthSeconds;
                Stream = stream;
            }

            public string AssetPath { get; }
            public float LengthSeconds { get; }
            public bool Stream { get; }
        }

        private readonly struct ResourceFolderRelocation
        {
            public ResourceFolderRelocation(string sourcePath, string destinationPath)
            {
                SourcePath = sourcePath;
                DestinationPath = destinationPath;
            }

            public string SourcePath { get; }
            public string DestinationPath { get; }
        }

        private readonly struct TextureReferenceConsolidation
        {
            public TextureReferenceConsolidation(
                string canonicalTexturePath,
                string duplicateTexturePath,
                string consumerMaterialPath)
            {
                CanonicalTexturePath = canonicalTexturePath;
                DuplicateTexturePath = duplicateTexturePath;
                ConsumerMaterialPath = consumerMaterialPath;
            }

            public string CanonicalTexturePath { get; }
            public string DuplicateTexturePath { get; }
            public string ConsumerMaterialPath { get; }
        }

        private sealed class OptimizationReport
        {
            public string GeneratedUtc;
            public string UnityVersion;
            public int DependencyCount;
            public int ResourcesAssetCount;
            public int ResourceFolderRelocationCount;
            public int TextureCandidateCount;
            public int ChangedTextureCount;
            public int CompliantTextureCount;
            public int AudioCandidateCount;
            public int ChangedAudioCount;
            public int CompliantAudioCount;
            public readonly List<string> Changes = new();
        }
    }
}
