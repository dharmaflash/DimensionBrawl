using System;
using DimensionBrawl.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DimensionBrawl.Editor
{
    public static class PerfectDodgeScreenDomainRendererSetup
    {
        private const string PcRendererDataPath = "Assets/Settings/PC_Renderer.asset";
        private const string MobileRendererDataPath = "Assets/Settings/Mobile_Renderer.asset";
        private const string ScreenDomainMaterialPath =
            "Assets/_Game/Art/VFX/CombatCues/Materials/DB_PerfectDodgeScreenDomain.mat";

        [MenuItem("DimensionBrawl/Action Foundation/Ensure Perfect Dodge Screen Domain Renderer Feature")]
        public static void EnsurePerfectDodgeScreenDomainRendererFeatureMenu()
        {
            EnsurePerfectDodgeScreenDomainRendererFeature();
        }

        public static void RunBatchEnsurePerfectDodgeScreenDomainRendererFeature()
        {
            EnsurePerfectDodgeScreenDomainRendererFeature();
        }

        private static void EnsurePerfectDodgeScreenDomainRendererFeature()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ScreenDomainMaterialPath);
            if (material == null)
            {
                throw new InvalidOperationException($"Missing perfect dodge screen domain material: {ScreenDomainMaterialPath}");
            }

            bool changed = EnsureRendererFeature(PcRendererDataPath, material);
            changed |= EnsureRendererFeature(MobileRendererDataPath, material);
            if (changed)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static bool EnsureRendererFeature(string rendererDataPath, Material material)
        {
            ScriptableRendererData rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(rendererDataPath);
            if (rendererData == null)
            {
                throw new InvalidOperationException($"Missing URP renderer data: {rendererDataPath}");
            }

            bool changed = false;
            if (!rendererData.TryGetRendererFeature(out PerfectDodgeScreenDomainRendererFeature feature))
            {
                feature = ScriptableObject.CreateInstance<PerfectDodgeScreenDomainRendererFeature>();
                feature.name = "PerfectDodgeScreenDomainRendererFeature";
                AssetDatabase.AddObjectToAsset(feature, rendererData);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId);

                SerializedObject rendererObject = new SerializedObject(rendererData);
                SerializedProperty features = rendererObject.FindProperty("m_RendererFeatures");
                SerializedProperty featureMap = rendererObject.FindProperty("m_RendererFeatureMap");
                features.arraySize++;
                features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = feature;
                featureMap.arraySize++;
                featureMap.GetArrayElementAtIndex(featureMap.arraySize - 1).longValue = localId;
                rendererObject.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            if (!feature.isActive)
            {
                feature.SetActive(true);
                changed = true;
            }

            SerializedObject featureObject = new SerializedObject(feature);
            SerializedProperty passMaterial = featureObject.FindProperty("passMaterial");
            if (passMaterial != null && passMaterial.objectReferenceValue != material)
            {
                passMaterial.objectReferenceValue = material;
                changed = true;
            }

            SerializedProperty injectionPoint = featureObject.FindProperty("injectionPoint");
            if (injectionPoint != null && injectionPoint.intValue != (int)RenderPassEvent.BeforeRenderingPostProcessing)
            {
                injectionPoint.intValue = (int)RenderPassEvent.BeforeRenderingPostProcessing;
                featureObject.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            featureObject.ApplyModifiedPropertiesWithoutUndo();
            feature.SetPassMaterial(material);
            feature.Create();
            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(rendererData);
            return changed;
        }
    }
}
