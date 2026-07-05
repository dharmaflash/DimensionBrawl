#if UNITY_EDITOR
using System;
using DimensionBrawl.LevelDesign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class OlympusStationCombatStageNoCrossLineSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string RootName = "OlympusStation_NoCrossCenterLine";
        private const string HovlBlueEffectPrefabPath =
            "Assets/_Imported/AssetStore/VFX/Hovl Studio/Sci-fi effects 2/Prefabs/Hex shield.prefab";
        private const string HovlShieldSourceMaterialFallbackPath =
            "Assets/_Imported/AssetStore/VFX/Hovl Studio/HSFiles/Materials/HexShield3Dshield.mat";
        private const string HovlShieldMaterialName = "HexShield3Dshield";
        private const string HologramMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_OlympusNoCrossCenterLine_HovlBlue.mat";
        private const string HologramBrightMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_OlympusNoCrossCenterLine_HovlBlue_Bright.mat";

        [MenuItem("DimensionBrawl/Stage/Olympus Station/Apply No-Cross Center Line")]
        public static void ApplyMenu()
        {
            ApplyToScene();
        }

        public static void RunBatchApplyNoCrossLine()
        {
            try
            {
                ApplyToScene();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ApplyToScene()
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SummonLaneSpace laneSpace = FindSceneComponent<SummonLaneSpace>(scene);
            if (laneSpace == null)
            {
                throw new InvalidOperationException(
                    $"Could not find a {nameof(SummonLaneSpace)} in {ScenePath}.");
            }

            GameObject existing = FindRoot(scene, RootName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            Material hologram = EnsureHovlBlueMaterial(
                HologramMaterialPath,
                "AF_OlympusNoCrossCenterLine_HovlBlue",
                new Color(0.06f, 0.78f, 1f, 0.5f),
                opacity: 0.46f,
                emission: 5.2f);
            Material brightHologram = EnsureHovlBlueMaterial(
                HologramBrightMaterialPath,
                "AF_OlympusNoCrossCenterLine_HovlBlue_Bright",
                new Color(0.16f, 0.92f, 1f, 0.82f),
                opacity: 0.78f,
                emission: 6.8f);

            Vector3 center = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, 0f);
            float halfWidth = laneSpace.HalfWidth;
            float lineWidth = halfWidth * 2f + 0.9f;

            GameObject root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = center;
            root.transform.rotation = laneSpace.transform.rotation;

            CreateStrip(root.transform, "HardBoundary_GlowFloor", new Vector3(0f, 0.032f, 0f),
                new Vector3(lineWidth + 0.6f, 0.028f, 0.52f), hologram);
            CreateStrip(root.transform, "HardBoundary_CoreLine", new Vector3(0f, 0.062f, 0f),
                new Vector3(lineWidth, 0.055f, 0.16f), brightHologram);
            CreateStrip(root.transform, "HardBoundary_StopWall", new Vector3(0f, 0.68f, 0.018f),
                new Vector3(lineWidth, 1.18f, 0.045f), hologram);

            int chevronCount = 9;
            float chevronSpacing = lineWidth / chevronCount;
            float startX = -lineWidth * 0.5f + chevronSpacing * 0.5f;
            for (int i = 0; i < chevronCount; i++)
            {
                float x = startX + i * chevronSpacing;
                CreateChevron(root.transform, $"PushbackChevron_{i:00}_Left", x - 0.12f, -28f, brightHologram);
                CreateChevron(root.transform, $"PushbackChevron_{i:00}_Right", x + 0.12f, 28f, brightHologram);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Applied no-cross center line to {ScenePath}.");
        }

        private static void CreateChevron(Transform root, string name, float x, float yawDegrees, Material material)
        {
            GameObject strip = CreateStrip(root, name, new Vector3(x, 0.09f, -0.37f),
                new Vector3(0.58f, 0.038f, 0.075f), material);
            strip.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
        }

        private static GameObject CreateStrip(
            Transform root,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = name;
            strip.transform.SetParent(root, false);
            strip.transform.localPosition = localPosition;
            strip.transform.localRotation = Quaternion.identity;
            strip.transform.localScale = localScale;
            Collider collider = strip.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Renderer renderer = strip.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return strip;
        }

        private static Material EnsureHovlBlueMaterial(
            string path,
            string materialName,
            Color color,
            float opacity,
            float emission)
        {
            Material source = LoadHovlShieldMaterial();
            if (source == null)
            {
                throw new InvalidOperationException(
                    $"Could not find Hovl shield material from {HovlBlueEffectPrefabPath}.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(source)
                {
                    name = materialName
                };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.CopyPropertiesFromMaterial(source);
            }

            material.name = materialName;
            material.shader = source.shader;
            material.enableInstancing = true;
            SetColorIfPresent(material, "_BaseColor", color);
            SetColorIfPresent(material, "_Color", color);
            SetColorIfPresent(material, "_TintColor", color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", new Color(0.36f, 0.96f, 1f, color.a) * 2.2f);
            }

            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_BUILTIN_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Opacity", opacity);
            SetFloatIfPresent(material, "_Textureopacity", opacity);
            SetFloatIfPresent(material, "_Texturesopacity", opacity);
            SetFloatIfPresent(material, "_Emission", emission);
            SetFloatIfPresent(material, "_Color_power", 1.65f);
            SetFloatIfPresent(material, "_Fresnelpower", 2.4f);
            SetFloatIfPresent(material, "_Fresnelscale", 1.15f);
            SetFloatIfPresent(material, "_Triplanar_tiling", 0.22f);
            SetFloatIfPresent(material, "_Shield_step", 1f);
            material.renderQueue = (int)RenderQueue.Transparent + 35;

            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadHovlShieldMaterial()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HovlBlueEffectPrefabPath);
            if (prefab != null)
            {
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(includeInactive: true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Material[] sharedMaterials = renderers[i].sharedMaterials;
                    for (int j = 0; j < sharedMaterials.Length; j++)
                    {
                        Material candidate = sharedMaterials[j];
                        if (candidate != null &&
                            string.Equals(candidate.name, HovlShieldMaterialName, StringComparison.Ordinal))
                        {
                            return candidate;
                        }
                    }
                }
            }

            return AssetDatabase.LoadAssetAtPath<Material>(HovlShieldSourceMaterialFallbackPath);
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static T FindSceneComponent<T>(Scene scene)
            where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }
    }
}
#endif
