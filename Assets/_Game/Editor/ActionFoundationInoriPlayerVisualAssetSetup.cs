using System;
using System.Collections.Generic;
using System.IO;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationInoriPlayerVisualAssetSetup
    {
        public const string SourcePrefabPath =
            "Assets/_Imported/AssetStore/RoloArt/Inori/Prefabs/Inori_MagicaCloth2_Costume1.prefab";
        public const string ModelPath =
            "Assets/_Game/Art/Characters/Player/Inori/Models/Inori_Unity.fbx";
        public const string RiflePoseTuningProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_InoriRiflePoseTuning.asset";

        private const string SourceRoot = "Assets/_Imported/AssetStore/RoloArt/Inori";
        private const string SourceModelPath = SourceRoot + "/FBX/Inori_Unity.fbx";
        private const string MaterialRoot = "Assets/_Game/Art/Characters/Player/Inori/Materials";
        private const string TextureRoot = "Assets/_Game/Art/Characters/Player/Inori/Textures";
        private const string CharacterToonShaderName = "Toon";

        private static readonly MaterialSpec[] MaterialSpecs =
        {
            new MaterialSpec("M_Inori_Body", "DB_Inori_Body"),
            new MaterialSpec("M_Inori_Body_Costume1", "DB_Inori_Body_Costume1"),
            new MaterialSpec("M_Inori_Body_Costume2", "DB_Inori_Body_Costume2"),
            new MaterialSpec("M_Inori_CostumeExtra", "DB_Inori_CostumeExtra"),
            new MaterialSpec("M_Inori_Costume_A", "DB_Inori_Costume_A"),
            new MaterialSpec("M_Inori_Costume_B", "DB_Inori_Costume_B"),
            new MaterialSpec("M_Inori_Expressions", "DB_Inori_Expressions"),
            new MaterialSpec("M_Inori_EyeAlpha", "DB_Inori_EyeAlpha"),
            new MaterialSpec("M_Inori_Glasses", "DB_Inori_Glasses"),
            new MaterialSpec("M_Inori_Hair", "DB_Inori_Hair"),
            new MaterialSpec("M_Inori_Head", "DB_Inori_Head")
        };

        [MenuItem("DimensionBrawl/Reapply Action Foundation Inori Player Visual Assets")]
        public static void ReapplyInoriPlayerVisualAssetsMenu()
        {
            EnsureInoriPlayerVisualAssets();
            Debug.Log("Reapplied ActionFoundation Inori player visual assets.");
        }

        public static void EnsureInoriPlayerVisualAssets()
        {
            PromoteModel();
            PromoteMaterials();
            EnsureRiflePoseTuningProfile();
            AssetDatabase.SaveAssets();
        }

        public static Avatar LoadPromotedAvatar()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(ModelPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar && avatar.isHuman && avatar.isValid)
                {
                    return avatar;
                }
            }

            throw new InvalidOperationException($"Missing valid promoted Inori humanoid Avatar at {ModelPath}.");
        }

        public static Material ResolvePromotedMaterial(string sourceMaterialName, int slotIndex)
        {
            string lower = string.IsNullOrWhiteSpace(sourceMaterialName)
                ? string.Empty
                : sourceMaterialName.ToLowerInvariant();

            if (lower.Contains("costumeextra", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_CostumeExtra");
            }

            if (lower.Contains("costume_a", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Costume_A");
            }

            if (lower.Contains("costume_b", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Costume_B");
            }

            if (lower.Contains("costume2", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Body_Costume2");
            }

            if (lower.Contains("costume1", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Body_Costume1");
            }

            if (lower.Contains("express", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Expressions");
            }

            if (lower.Contains("eyealpha", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_EyeAlpha");
            }

            if (lower.Contains("glass", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Glasses");
            }

            if (lower.Contains("hair", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Hair");
            }

            if (lower.Contains("head", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Head");
            }

            if (lower.Contains("body", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Body");
            }

            return slotIndex switch
            {
                1 => LoadMaterial("DB_Inori_Head"),
                2 => LoadMaterial("DB_Inori_Hair"),
                _ => LoadMaterial("DB_Inori_Body")
            };
        }

        private static void PromoteModel()
        {
            EnsureFolder(PathParent(ModelPath));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) == null &&
                !AssetDatabase.CopyAsset(SourceModelPath, ModelPath))
            {
                throw new InvalidOperationException($"Failed to promote Inori model from {SourceModelPath} to {ModelPath}.");
            }

            ModelImporter importer = RequireModelImporter(ModelPath);
            ModelImporter sourceImporter = RequireModelImporter(SourceModelPath);
            bool changed = false;
            changed |= SetIfDifferent(() => importer.importAnimation, value => importer.importAnimation = value, false);
            changed |= SetIfDifferent(
                () => importer.materialImportMode,
                value => importer.materialImportMode = value,
                ModelImporterMaterialImportMode.None);
            changed |= SetIfDifferent(
                () => importer.animationType,
                value => importer.animationType = value,
                ModelImporterAnimationType.Human);
            changed |= SetIfDifferent(
                () => importer.avatarSetup,
                value => importer.avatarSetup = value,
                ModelImporterAvatarSetup.CreateFromThisModel);

            if (!HumanDescriptionsMatch(importer.humanDescription, sourceImporter.humanDescription))
            {
                importer.humanDescription = sourceImporter.humanDescription;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void PromoteMaterials()
        {
            EnsureFolder(MaterialRoot);
            EnsureFolder(TextureRoot);
            for (int i = 0; i < MaterialSpecs.Length; i++)
            {
                PromoteMaterial(MaterialSpecs[i]);
            }
        }

        private static void PromoteMaterial(MaterialSpec spec)
        {
            string sourcePath = $"{SourceRoot}/Materials/{spec.SourceName}.mat";
            string targetPath = MaterialPath(spec.TargetName);
            Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
            if (sourceMaterial == null)
            {
                throw new InvalidOperationException($"Missing source Inori material at {sourcePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<Material>(targetPath) == null &&
                !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote Inori material from {sourcePath} to {targetPath}.");
            }

            Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            if (targetMaterial == null)
            {
                throw new InvalidOperationException($"Promoted Inori material missing at {targetPath}.");
            }

            ApplyPromotedCharacterMaterialShader(sourceMaterial, targetMaterial);
            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                CopyTextureProperty(sourceMaterial, targetMaterial, textureProperties[i]);
            }

            ApplyPromotedCharacterMainTexture(sourceMaterial, targetMaterial);
            EditorUtility.SetDirty(targetMaterial);
        }

        private static Material LoadMaterial(string materialName)
        {
            string path = MaterialPath(materialName);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                throw new InvalidOperationException($"Missing promoted Inori material at {path}.");
            }

            return material;
        }

        private static string MaterialPath(string materialName)
        {
            return $"{MaterialRoot}/{materialName}.mat";
        }

        private static void CopyTextureProperty(Material sourceMaterial, Material targetMaterial, string propertyName)
        {
            if (!targetMaterial.HasProperty(propertyName))
            {
                return;
            }

            Texture sourceTexture = sourceMaterial.GetTexture(propertyName);
            targetMaterial.SetTexture(
                propertyName,
                sourceTexture != null ? PromoteTexture(sourceTexture) : null);
        }

        private static void ApplyPromotedCharacterMaterialShader(Material sourceMaterial, Material targetMaterial)
        {
            Shader toonShader = Shader.Find(CharacterToonShaderName);
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            targetMaterial.shader = toonShader != null ? toonShader : litShader ?? sourceMaterial.shader;

            Color baseColor = Color.white;
            if (sourceMaterial.HasProperty("_BaseColor"))
            {
                baseColor = sourceMaterial.GetColor("_BaseColor");
            }
            else if (sourceMaterial.HasProperty("_Color"))
            {
                baseColor = sourceMaterial.GetColor("_Color");
            }

            SetColorIfPresent(targetMaterial, "_BaseColor", baseColor);
            SetColorIfPresent(targetMaterial, "_Color", baseColor);
        }

        private static void ApplyPromotedCharacterMainTexture(Material sourceMaterial, Material targetMaterial)
        {
            Texture mainTexture = PromoteTextureProperty(sourceMaterial, "_MainTex")
                ?? PromoteTextureProperty(sourceMaterial, "_BaseMap");
            if (mainTexture == null)
            {
                return;
            }

            SetTextureIfPresent(targetMaterial, "_BaseMap", mainTexture);
            SetTextureIfPresent(targetMaterial, "_MainTex", mainTexture);
            SetTextureIfPresent(targetMaterial, "_1st_ShadeMap", mainTexture);
        }

        private static Texture PromoteTextureProperty(Material sourceMaterial, string propertyName)
        {
            if (!sourceMaterial.HasProperty(propertyName))
            {
                return null;
            }

            Texture sourceTexture = sourceMaterial.GetTexture(propertyName);
            return sourceTexture != null ? PromoteTexture(sourceTexture) : null;
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static Texture PromoteTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string targetPath = $"{TextureRoot}/{Path.GetFileName(sourcePath)}";
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null &&
                !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote Inori texture from {sourcePath} to {targetPath}.");
            }

            MirrorTextureImporter(sourcePath, targetPath);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (promotedTexture == null)
            {
                throw new InvalidOperationException($"Promoted Inori texture missing at {targetPath}.");
            }

            return promotedTexture;
        }

        private static void MirrorTextureImporter(string sourcePath, string targetPath)
        {
            TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            TextureImporter targetImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;
            if (sourceImporter == null || targetImporter == null)
            {
                return;
            }

            bool changed = false;
            changed |= SetIfDifferent(
                () => targetImporter.textureType,
                value => targetImporter.textureType = value,
                sourceImporter.textureType);
            changed |= SetIfDifferent(
                () => targetImporter.sRGBTexture,
                value => targetImporter.sRGBTexture = value,
                sourceImporter.sRGBTexture);
            changed |= SetIfDifferent(
                () => targetImporter.alphaSource,
                value => targetImporter.alphaSource = value,
                sourceImporter.alphaSource);
            changed |= SetIfDifferent(
                () => targetImporter.alphaIsTransparency,
                value => targetImporter.alphaIsTransparency = value,
                sourceImporter.alphaIsTransparency);
            changed |= SetIfDifferent(
                () => targetImporter.mipmapEnabled,
                value => targetImporter.mipmapEnabled = value,
                sourceImporter.mipmapEnabled);
            changed |= SetIfDifferent(
                () => targetImporter.streamingMipmaps,
                value => targetImporter.streamingMipmaps = value,
                sourceImporter.streamingMipmaps);
            changed |= SetIfDifferent(
                () => targetImporter.npotScale,
                value => targetImporter.npotScale = value,
                sourceImporter.npotScale);
            changed |= SetIfDifferent(
                () => targetImporter.maxTextureSize,
                value => targetImporter.maxTextureSize = value,
                sourceImporter.maxTextureSize);
            changed |= SetIfDifferent(
                () => targetImporter.textureCompression,
                value => targetImporter.textureCompression = value,
                sourceImporter.textureCompression);
            changed |= SetIfDifferent(
                () => targetImporter.compressionQuality,
                value => targetImporter.compressionQuality = value,
                sourceImporter.compressionQuality);
            changed |= SetIfDifferent(
                () => targetImporter.crunchedCompression,
                value => targetImporter.crunchedCompression = value,
                sourceImporter.crunchedCompression);
            changed |= SetIfDifferent(
                () => targetImporter.filterMode,
                value => targetImporter.filterMode = value,
                sourceImporter.filterMode);
            changed |= SetIfDifferent(
                () => targetImporter.anisoLevel,
                value => targetImporter.anisoLevel = value,
                sourceImporter.anisoLevel);
            changed |= SetIfDifferent(
                () => targetImporter.wrapMode,
                value => targetImporter.wrapMode = value,
                sourceImporter.wrapMode);

            TextureImporterPlatformSettings sourceDefaultSettings = sourceImporter.GetDefaultPlatformTextureSettings();
            if (JsonUtility.ToJson(targetImporter.GetDefaultPlatformTextureSettings()) != JsonUtility.ToJson(sourceDefaultSettings))
            {
                targetImporter.SetPlatformTextureSettings(sourceDefaultSettings);
                changed = true;
            }

            if (changed)
            {
                targetImporter.SaveAndReimport();
            }
        }

        private static void EnsureRiflePoseTuningProfile()
        {
            EnsureFolder(PathParent(RiflePoseTuningProfilePath));
            if (AssetDatabase.LoadAssetAtPath<InoriRiflePoseTuningProfile>(RiflePoseTuningProfilePath) != null)
            {
                return;
            }

            InoriRiflePoseTuningProfile profile = ScriptableObject.CreateInstance<InoriRiflePoseTuningProfile>();
            AssetDatabase.CreateAsset(profile, RiflePoseTuningProfilePath);
            EditorUtility.SetDirty(profile);
        }

        private static ModelImporter RequireModelImporter(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing model importer at {path}.");
            }

            return importer;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = PathParent(folderPath);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }

        private static string PathParent(string path)
        {
            string normalized = path.Replace('\\', '/');
            return normalized.Substring(0, normalized.LastIndexOf('/'));
        }

        private static bool SetIfDifferent<T>(Func<T> getValue, Action<T> setValue, T targetValue)
        {
            if (EqualityComparer<T>.Default.Equals(getValue(), targetValue))
            {
                return false;
            }

            setValue(targetValue);
            return true;
        }

        private static bool HumanDescriptionsMatch(HumanDescription left, HumanDescription right)
        {
            return JsonUtility.ToJson(left) == JsonUtility.ToJson(right);
        }

        private readonly struct MaterialSpec
        {
            public MaterialSpec(string sourceName, string targetName)
            {
                SourceName = sourceName;
                TargetName = targetName;
            }

            public string SourceName { get; }
            public string TargetName { get; }
        }

    }
}
