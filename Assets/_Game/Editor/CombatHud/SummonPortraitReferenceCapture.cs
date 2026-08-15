using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Produces neutral reference renders of the three canonical combat summon prefabs.
    /// This is an editor-only review utility; it does not alter runtime assets or scenes.
    /// </summary>
    public static class SummonPortraitReferenceCapture
    {
        private const int Size = 768;
        private const string OutputDirectory =
            @"C:\Users\dharm\.codex\visualizations\2026\08\13\019ffa49-5536-79e0-a1ec-de549a4b6e40\combat-hud-concept\v4\summon-reference-renders";

        private static readonly string[] PrefabPaths =
        {
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab",
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot2Actor_MarksmanProxy.prefab",
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot3Actor_VanguardProxy.prefab"
        };

        private static readonly string[] OutputNames =
        {
            "summon_s1_charge_bruiser.png",
            "summon_s2_laser_soldier.png",
            "summon_s3_fire_dragon.png"
        };

        public static void RunBatchCapture()
        {
            try
            {
                Directory.CreateDirectory(OutputDirectory);
                for (int i = 0; i < PrefabPaths.Length; i++)
                {
                    CapturePrefab(PrefabPaths[i], Path.Combine(OutputDirectory, OutputNames[i]));
                }

                Debug.Log($"[SummonPortraitReferenceCapture] Wrote {PrefabPaths.Length} renders to {OutputDirectory}");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void CapturePrefab(string prefabPath, string outputPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Summon prefab not found: {prefabPath}");
            }

            PreviewRenderUtility preview = new PreviewRenderUtility(true);
            try
            {
                preview.cameraFieldOfView = 28f;
                preview.camera.nearClipPlane = 0.01f;
                preview.camera.farClipPlane = 500f;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(0.035f, 0.05f, 0.075f, 1f);
                preview.camera.allowHDR = true;

                GameObject instance = preview.InstantiatePrefabInScene(prefab);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to instantiate summon prefab: {prefabPath}");
                }

                DisableNonPortraitObjects(instance.transform);
                Bounds bounds = CalculateRenderableBounds(instance);
                Vector3 center = bounds.center;
                float extent = Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z));
                if (extent < 0.01f)
                {
                    throw new InvalidOperationException($"No visible renderer bounds for summon prefab: {prefabPath}");
                }

                float distance = extent / Mathf.Tan(preview.cameraFieldOfView * 0.42f * Mathf.Deg2Rad);
                Vector3 viewDirection = new Vector3(0.72f, 0.12f, 1f).normalized;
                preview.camera.transform.position = center + viewDirection * distance;
                preview.camera.transform.LookAt(center + Vector3.up * bounds.extents.y * 0.06f);

                preview.lights[0].intensity = 1.25f;
                preview.lights[0].color = new Color(0.86f, 0.93f, 1f);
                preview.lights[0].transform.rotation = Quaternion.Euler(34f, 150f, 0f);
                preview.lights[1].intensity = 0.65f;
                preview.lights[1].color = new Color(0.45f, 0.65f, 1f);
                preview.lights[1].transform.rotation = Quaternion.Euler(320f, 25f, 0f);
                preview.ambientColor = new Color(0.22f, 0.26f, 0.34f);

                Rect rect = new Rect(0f, 0f, Size, Size);
                preview.BeginStaticPreview(rect);
                preview.camera.Render();
                Texture2D image = preview.EndStaticPreview();
                if (image == null)
                {
                    throw new InvalidOperationException($"Render failed for summon prefab: {prefabPath}");
                }

                try
                {
                    File.WriteAllBytes(outputPath, image.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(image);
                }
            }
            finally
            {
                preview.Cleanup();
            }
        }

        private static void DisableNonPortraitObjects(Transform root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                string name = child.name;
                if (name.IndexOf("HealthBar", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("TierPulse", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("PressureScreen", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("ProjectileOrigin", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("DamageVfx", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("EntryCue", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static Bounds CalculateRenderableBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(false);
            bool initialized = false;
            Bounds bounds = default;
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.enabled || renderer is ParticleSystemRenderer)
                {
                    continue;
                }

                if (!initialized)
                {
                    bounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }
    }
}
