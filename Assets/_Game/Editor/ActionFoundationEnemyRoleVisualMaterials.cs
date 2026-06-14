using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Enemies;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    internal static partial class ActionFoundationEnemyRoleVisualSetup
    {
        private static RoleTelegraphSpec CreateTelegraphSpec(TelegraphStyle style)
        {
            return style switch
            {
                TelegraphStyle.SmallRead => Telegraph(new Vector3(0.3f, 0.018f, 0.5f), new Vector3(0.9f, 0.02f, 1.25f), new Vector3(1.08f, 0.026f, 1.45f), new Color(0.25f, 0.85f, 1f, 1f), new Color(0.7f, 0.95f, 1f, 1f), Color.white),
                TelegraphStyle.Guard => Telegraph(new Vector3(0.45f, 0.02f, 0.7f), new Vector3(1.24f, 0.024f, 1.72f), new Vector3(1.42f, 0.03f, 1.95f), new Color(1f, 0.62f, 0.14f, 1f), new Color(1f, 0.22f, 0.04f, 1f), new Color(1f, 0.9f, 0.48f, 1f)),
                TelegraphStyle.Lunge => Telegraph(new Vector3(0.25f, 0.018f, 0.82f), new Vector3(0.86f, 0.022f, 2.08f), new Vector3(1f, 0.028f, 2.42f), new Color(1f, 0.24f, 0.1f, 1f), new Color(1f, 0.05f, 0.02f, 1f), new Color(1f, 0.82f, 0.62f, 1f)),
                TelegraphStyle.Line => Telegraph(new Vector3(0.18f, 0.016f, 1.25f), new Vector3(0.52f, 0.018f, 3.05f), new Vector3(0.62f, 0.024f, 3.45f), new Color(0.2f, 0.66f, 1f, 1f), new Color(0.05f, 0.3f, 1f, 1f), new Color(0.86f, 0.96f, 1f, 1f)),
                TelegraphStyle.Fan => Telegraph(new Vector3(0.56f, 0.018f, 0.74f), new Vector3(1.9f, 0.02f, 1.25f), new Vector3(2.25f, 0.026f, 1.55f), new Color(0.88f, 0.25f, 1f, 1f), new Color(0.5f, 0.04f, 1f, 1f), new Color(1f, 0.82f, 1f, 1f)),
                TelegraphStyle.EliteGuard => Telegraph(new Vector3(0.48f, 0.024f, 0.88f), new Vector3(1.18f, 0.026f, 2.18f), new Vector3(1.4f, 0.034f, 2.55f), new Color(1f, 0.5f, 0.05f, 1f), new Color(1f, 0.1f, 0.02f, 1f), new Color(1f, 0.9f, 0.48f, 1f)),
                TelegraphStyle.EliteAura => Telegraph(new Vector3(0.65f, 0.024f, 0.82f), new Vector3(1.72f, 0.026f, 1.9f), new Vector3(2.05f, 0.034f, 2.22f), new Color(0.1f, 0.95f, 0.78f, 1f), new Color(0.05f, 0.62f, 0.42f, 1f), new Color(0.76f, 1f, 0.92f, 1f)),
                TelegraphStyle.EliteLine => Telegraph(new Vector3(0.34f, 0.022f, 1.1f), new Vector3(0.92f, 0.024f, 2.65f), new Vector3(1.08f, 0.032f, 3.08f), new Color(0.22f, 0.44f, 1f, 1f), new Color(0.08f, 0.14f, 1f, 1f), new Color(0.82f, 0.9f, 1f, 1f)),
                TelegraphStyle.FinalStand => Telegraph(new Vector3(0.74f, 0.026f, 1.08f), new Vector3(1.88f, 0.03f, 2.58f), new Vector3(2.28f, 0.038f, 3.08f), new Color(1f, 0.22f, 0.04f, 1f), new Color(0.8f, 0.02f, 0.01f, 1f), new Color(1f, 0.82f, 0.58f, 1f)),
                _ => throw new InvalidOperationException($"Unsupported telegraph style {style}.")
            };
        }

        private static RoleTelegraphSpec Telegraph(Vector3 startScale, Vector3 endScale, Vector3 activeScale, Color startColor, Color endColor, Color activeColor)
        {
            return new RoleTelegraphSpec
            {
                WindupStartScale = startScale,
                WindupEndScale = endScale,
                ActiveScale = activeScale,
                WindupStartColor = startColor,
                WindupEndColor = endColor,
                ActiveColor = activeColor
            };
        }

        private static Material[] PromoteMaterials(Material[] sourceMaterials, string materialRoot, string textureRoot)
        {
            Material[] promoted = new Material[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                promoted[i] = PromoteMaterial(sourceMaterials[i], materialRoot, textureRoot);
            }

            return promoted;
        }

        private static Material PromoteMaterial(Material sourceMaterial, string materialRoot, string textureRoot)
        {
            if (sourceMaterial == null)
            {
                return null;
            }

            EnsureFolder(materialRoot);
            EnsureFolder(textureRoot);

            string targetPath = $"{materialRoot}/{SanitizeAssetName(sourceMaterial.name)}.mat";
            Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? sourceMaterial.shader;
            if (targetMaterial == null)
            {
                targetMaterial = new Material(shader);
                AssetDatabase.CreateAsset(targetMaterial, targetPath);
            }
            else
            {
                targetMaterial.shader = shader;
            }

            targetMaterial.CopyPropertiesFromMaterial(sourceMaterial);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_BaseMap", TextureUsage.Color, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_MainTex", TextureUsage.Color, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_BumpMap", TextureUsage.Normal, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_EmissionMap", TextureUsage.Color, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_MetallicGlossMap", TextureUsage.Linear, textureRoot);
            CopyTextureProperty(sourceMaterial, targetMaterial, "_OcclusionMap", TextureUsage.Linear, textureRoot);
            PromoteImportedTextureProperties(sourceMaterial, targetMaterial, textureRoot);
            EnableKeywordIfTexture(targetMaterial, "_BumpMap", "_NORMALMAP");
            EnableKeywordIfTexture(targetMaterial, "_MetallicGlossMap", "_METALLICSPECGLOSSMAP");
            EnableKeywordIfTexture(targetMaterial, "_OcclusionMap", "_OCCLUSIONMAP");
            EnableKeywordIfTexture(targetMaterial, "_EmissionMap", "_EMISSION");
            EditorUtility.SetDirty(targetMaterial);
            return targetMaterial;
        }

        private static void CopyTextureProperty(Material sourceMaterial, Material targetMaterial, string propertyName, TextureUsage usage, string textureRoot)
        {
            if (!sourceMaterial.HasProperty(propertyName) || !targetMaterial.HasProperty(propertyName))
            {
                return;
            }

            Texture sourceTexture = sourceMaterial.GetTexture(propertyName);
            targetMaterial.SetTexture(propertyName, sourceTexture != null ? PromoteTexture(sourceTexture, usage, textureRoot) : null);
        }

        private static void PromoteImportedTextureProperties(Material sourceMaterial, Material targetMaterial, string textureRoot)
        {
            string[] textureProperties = targetMaterial.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                string propertyName = textureProperties[i];
                if (!targetMaterial.HasProperty(propertyName))
                {
                    continue;
                }

                Texture sourceTexture = sourceMaterial.HasProperty(propertyName)
                    ? sourceMaterial.GetTexture(propertyName)
                    : targetMaterial.GetTexture(propertyName);
                if (sourceTexture == null)
                {
                    continue;
                }

                string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
                if (!sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    continue;
                }

                targetMaterial.SetTexture(propertyName, PromoteTexture(sourceTexture, InferTextureUsage(propertyName), textureRoot));
                if (sourceMaterial.HasProperty(propertyName))
                {
                    targetMaterial.SetTextureScale(propertyName, sourceMaterial.GetTextureScale(propertyName));
                    targetMaterial.SetTextureOffset(propertyName, sourceMaterial.GetTextureOffset(propertyName));
                }
            }
        }

        private static TextureUsage InferTextureUsage(string propertyName)
        {
            if (propertyName.IndexOf("normal", StringComparison.OrdinalIgnoreCase) >= 0
                || propertyName.IndexOf("bump", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TextureUsage.Normal;
            }

            if (propertyName.IndexOf("metal", StringComparison.OrdinalIgnoreCase) >= 0
                || propertyName.IndexOf("occlusion", StringComparison.OrdinalIgnoreCase) >= 0
                || propertyName.IndexOf("rough", StringComparison.OrdinalIgnoreCase) >= 0
                || propertyName.IndexOf("smooth", StringComparison.OrdinalIgnoreCase) >= 0
                || propertyName.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TextureUsage.Linear;
            }

            return TextureUsage.Color;
        }

        private static Texture PromoteTexture(Texture sourceTexture, TextureUsage usage, string textureRoot)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            EnsureFolder(textureRoot);
            string fileName = sourcePath.Substring(sourcePath.LastIndexOf('/') + 1);
            string targetPath = $"{textureRoot}/{fileName}";
            if (AssetDatabase.LoadAssetAtPath<Texture>(targetPath) == null && !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new InvalidOperationException($"Failed to promote texture from {sourcePath} to {targetPath}.");
            }

            ConfigureTextureImporter(targetPath, usage);
            return AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
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
            importer.SaveAndReimport();
        }

        private static void EnableKeywordIfTexture(Material material, string textureProperty, string keyword)
        {
            if (material.HasProperty(textureProperty) && material.GetTexture(textureProperty) != null)
            {
                material.EnableKeyword(keyword);
            }
        }
    }
}
