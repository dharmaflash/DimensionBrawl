using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Creates the three shared presentation materials for the optional combat HUD flow.
    /// This setup deliberately does not open, edit, or save the combat HUD prefab.
    /// </summary>
    public static class CombatHudFlowPresentationSetup
    {
        private const string FlowTexturePath =
            "Assets/_Game/UI/CombatHud/Art/CelestialHud/Motion/DB_UI_CelestialFlow.png";
        private const string FlowShaderPath =
            "Assets/_Game/UI/CombatHud/Shaders/DB_UI_CelestialFlow.shader";
        private const string MaterialDirectory =
            "Assets/_Game/UI/CombatHud/Materials";
        private const string PanelMaterialPath =
            MaterialDirectory + "/DB_UI_CelestialFlow_Panel.mat";
        private const string VitalMaterialPath =
            MaterialDirectory + "/DB_UI_CelestialFlow_Vital.mat";
        private const string EnergyMaterialPath =
            MaterialDirectory + "/DB_UI_CelestialFlow_Energy.mat";

        [MenuItem("DimensionBrawl/UI V1/Create Combat HUD Flow Materials")]
        public static void CreateMaterialsMenu()
        {
            CreateMaterials();
        }

        public static void CreateMaterials()
        {
            Texture2D flowTexture = ConfigureFlowTextureImporter();
            Shader flowShader = AssetDatabase.LoadAssetAtPath<Shader>(FlowShaderPath);
            if (flowShader == null)
            {
                throw new InvalidOperationException($"Missing combat HUD flow shader: {FlowShaderPath}");
            }

            EnsureAssetFolder(MaterialDirectory);
            ConfigureMaterial(
                PanelMaterialPath,
                flowShader,
                flowTexture,
                new Color(0.78f, 0.93f, 1f, 0.16f),
                0.018f,
                new Vector2(1.35f, 1f),
                new Vector2(0.009f, 0.0015f),
                new Vector2(0f, 0f));
            ConfigureMaterial(
                VitalMaterialPath,
                flowShader,
                flowTexture,
                new Color(1f, 0.93f, 0.82f, 0.12f),
                0.028f,
                new Vector2(1.65f, 1f),
                new Vector2(0.014f, 0f),
                new Vector2(0.31f, 0.13f));
            ConfigureMaterial(
                EnergyMaterialPath,
                flowShader,
                flowTexture,
                new Color(0.63f, 0.96f, 1f, 0.18f),
                0.045f,
                new Vector2(1.8f, 1f),
                new Vector2(0.021f, -0.001f),
                new Vector2(0.63f, 0.27f));

            AssetDatabase.SaveAssets();
            Debug.Log(
                "Created combat HUD celestial-flow material assets only; "
                + "no prefab, scene, layout, or input binding was changed.");
        }

        private static Texture2D ConfigureFlowTextureImporter()
        {
            TextureImporter importer = AssetImporter.GetAtPath(FlowTexturePath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Missing seamless combat HUD flow texture: {FlowTexturePath}");
            }

            bool changed = importer.textureType != TextureImporterType.Default
                || importer.sRGBTexture
                || importer.mipmapEnabled
                || importer.alphaSource != TextureImporterAlphaSource.None
                || importer.wrapMode != TextureWrapMode.Repeat
                || importer.filterMode != FilterMode.Bilinear
                || importer.maxTextureSize != 512
                || importer.npotScale != TextureImporterNPOTScale.None
                || importer.textureCompression != TextureImporterCompression.Compressed;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 512;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Compressed;
            if (changed)
            {
                importer.SaveAndReimport();
            }

            Texture2D flowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FlowTexturePath);
            if (flowTexture == null)
            {
                throw new InvalidOperationException(
                    $"Could not import seamless combat HUD flow texture: {FlowTexturePath}");
            }

            return flowTexture;
        }

        private static void ConfigureMaterial(
            string path,
            Shader shader,
            Texture2D flowTexture,
            Color flowTint,
            float flowStrength,
            Vector2 flowTiling,
            Vector2 flowSpeed,
            Vector2 flowPhase)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = Path.GetFileNameWithoutExtension(path)
                };
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetTexture("_FlowTex", flowTexture);
            material.SetColor("_FlowTint", flowTint);
            material.SetFloat("_FlowStrength", flowStrength);
            material.SetVector("_FlowTiling", new Vector4(flowTiling.x, flowTiling.y, 0f, 0f));
            material.SetVector("_FlowSpeed", new Vector4(flowSpeed.x, flowSpeed.y, 0f, 0f));
            material.SetVector("_FlowPhase", new Vector4(flowPhase.x, flowPhase.y, 0f, 0f));
            EditorUtility.SetDirty(material);
        }

        private static void EnsureAssetFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetFolder)?.Replace('\\', '/');
            string child = Path.GetFileName(assetFolder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(child))
            {
                throw new InvalidOperationException($"Invalid asset folder: {assetFolder}");
            }

            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
