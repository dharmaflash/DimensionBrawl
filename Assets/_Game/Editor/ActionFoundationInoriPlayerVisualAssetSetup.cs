using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationInoriPlayerVisualAssetSetup
    {
        public const string SourcePrefabPath =
            "Assets/_Imported/AssetStore/RoloArt/Inori/Prefabs/Inori_BasicSetup_Costume2.prefab";
        public const string ModelPath =
            "Assets/_Game/Art/Characters/Player/Inori/Models/Inori_Unity.fbx";

        private const string SourceRoot = "Assets/_Imported/AssetStore/RoloArt/Inori";
        private const string SourceModelPath = SourceRoot + "/FBX/Inori_Unity.fbx";
        private const string MaterialRoot = "Assets/_Game/Art/Characters/Player/Inori/Materials";
        private const string TextureRoot = "Assets/_Game/Art/Characters/Player/Inori/Textures";
        private const string ReferenceToonMaterialPath =
            "Assets/_Game/Art/Characters/Player/CombatGirlSwordShield/Materials/DB_CombatGirl_Body.mat";

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

            if (lower.Contains("costume2", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Body_Costume2");
            }

            if (lower.Contains("costume1", StringComparison.Ordinal))
            {
                return LoadMaterial("DB_Inori_Body_Costume1");
            }

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
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.humanDescription = sourceImporter.humanDescription;
            importer.SaveAndReimport();
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

            targetMaterial.shader = ResolvePlayerToonShader() ?? sourceMaterial.shader;
            string[] textureProperties = sourceMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                CopyTextureProperty(sourceMaterial, targetMaterial, textureProperties[i]);
            }

            CopyMainTextureToCommonBaseSlots(sourceMaterial, targetMaterial);
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

        private static Shader ResolvePlayerToonShader()
        {
            Material referenceMaterial = AssetDatabase.LoadAssetAtPath<Material>(ReferenceToonMaterialPath);
            if (referenceMaterial != null &&
                referenceMaterial.shader != null &&
                !string.Equals(referenceMaterial.shader.name, "Hidden/InternalErrorShader", StringComparison.Ordinal))
            {
                return referenceMaterial.shader;
            }

            return Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
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
                sourceTexture != null ? PromoteTexture(sourceTexture, ClassifyTextureUsage(propertyName)) : null);
        }

        private static void CopyMainTextureToCommonBaseSlots(Material sourceMaterial, Material targetMaterial)
        {
            if (!sourceMaterial.HasProperty("_MainTex"))
            {
                return;
            }

            Texture sourceTexture = sourceMaterial.GetTexture("_MainTex");
            if (sourceTexture == null)
            {
                return;
            }

            Texture promotedTexture = PromoteTexture(sourceTexture, TextureUsage.Color);
            SetTextureIfPresent(targetMaterial, "_BaseMap", promotedTexture);
            SetTextureIfPresent(targetMaterial, "_1st_ShadeMap", promotedTexture);
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static Texture PromoteTexture(Texture sourceTexture, TextureUsage usage)
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

            ConfigureTextureImporter(targetPath, usage);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
            if (promotedTexture == null)
            {
                throw new InvalidOperationException($"Promoted Inori texture missing at {targetPath}.");
            }

            return promotedTexture;
        }

        private static void ConfigureTextureImporter(string path, TextureUsage usage)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = usage == TextureUsage.Normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = usage == TextureUsage.Color;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static TextureUsage ClassifyTextureUsage(string propertyName)
        {
            string lower = propertyName.ToLowerInvariant();
            if (lower.Contains("normal", StringComparison.Ordinal) || lower.Contains("bump", StringComparison.Ordinal))
            {
                return TextureUsage.Normal;
            }

            if (lower.Contains("metal", StringComparison.Ordinal)
                || lower.Contains("spec", StringComparison.Ordinal)
                || lower.Contains("mask", StringComparison.Ordinal)
                || lower.Contains("matcap", StringComparison.Ordinal))
            {
                return TextureUsage.Linear;
            }

            return TextureUsage.Color;
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

        private enum TextureUsage
        {
            Color,
            Normal,
            Linear
        }
    }
}
