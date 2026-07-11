using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Core
{
    public static class OlympusMobileRenderBudgetBootstrap
    {
        private const string CorridorStageRootName = "OlympusCorridorStageRoot";
        private const string StageMapRootName = "OlympusCorridorStageMap";

        private static readonly HashSet<string> DecorativeShadowMeshNames = new(StringComparer.Ordinal)
        {
            "SM_Arch_SM_Arch_base_gold_LOD0",
            "SM_Arch_SM_Arch_base_wall_LOD0",
            "SM_Arch_SM_Arch_top_gold_LOD0",
            "SM_Arch_SM_Arch_top_wall_LOD0",
            "SM_Chandellier_SM_Chandellier_chain_LOD0",
            "SM_SM_Cloud_LOD0",
            "SM_FlagConnection_part_rope_LOD0",
            "SM_ArchSmallModular_part_bot_LOD0",
            "SM_ArchSmallModular_part_mid_LOD0",
            "SM_ArchSmallModular_part_top_LOD0",
            "SM_FenceBase_part_1_LOD0",
            "SM_FloorBase_part_1_c_LOD0"
        };

        private static readonly HashSet<string> DecorativeLightNames = new(StringComparer.Ordinal)
        {
            "FireGlow_LeftCollapsedSlab",
            "FireGlow_RightCollapsedSlab",
            "FireGlow_LeftBrokenCapstone",
            "FireGlow_RightBrokenRail"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyToInitialScene()
        {
            ApplyToScene(SceneManager.GetActiveScene());
        }

        public static int ApplyToScene(Scene scene)
        {
#if UNITY_EDITOR
            MobilePerformanceTier tier = MobilePerformanceTier.Balanced;
#else
            MobilePerformanceTier tier = MobilePerformanceGovernor.ActiveInstance != null
                ? MobilePerformanceGovernor.ActiveInstance.CurrentTier
                : MobilePerformanceTier.Balanced;
#endif
            return ApplyToScene(scene, tier);
        }

        public static int ApplyToScene(Scene scene, MobilePerformanceTier tier)
        {
            if (!scene.IsValid() || !scene.isLoaded || !ShouldApplyOnCurrentPlatform())
            {
                return 0;
            }

            Transform mapRoot = FindStageMapRoot(scene);
            if (mapRoot == null)
            {
                return 0;
            }

            int changedCount = 0;
            bool disableEnvironmentShadows = tier != MobilePerformanceTier.High;
            MeshRenderer[] renderers = mapRoot.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer renderer = renderers[i];
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
                if (mesh == null
                    || (!disableEnvironmentShadows && !DecorativeShadowMeshNames.Contains(mesh.name))
                    || renderer.shadowCastingMode == ShadowCastingMode.Off)
                {
                    continue;
                }

                renderer.shadowCastingMode = ShadowCastingMode.Off;
                changedCount++;
            }

            Light[] lights = mapRoot.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (!DecorativeLightNames.Contains(light.name) || !light.enabled)
                {
                    continue;
                }

                light.enabled = false;
                changedCount++;
            }

            OlympusMobileEnvironmentDetailCuller detailCuller =
                mapRoot.GetComponent<OlympusMobileEnvironmentDetailCuller>();
            if (tier != MobilePerformanceTier.High && detailCuller == null)
            {
                detailCuller = mapRoot.gameObject.AddComponent<OlympusMobileEnvironmentDetailCuller>();
            }

            if (detailCuller != null && detailCuller.Configure(mapRoot, null, tier))
            {
                changedCount++;
            }

            return changedCount;
        }

        public static bool IsDecorativeShadowMeshName(string meshName)
        {
            return !string.IsNullOrEmpty(meshName) && DecorativeShadowMeshNames.Contains(meshName);
        }

        public static bool IsDecorativeLightName(string lightName)
        {
            return !string.IsNullOrEmpty(lightName) && DecorativeLightNames.Contains(lightName);
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyToScene(scene);
        }

        private static bool ShouldApplyOnCurrentPlatform()
        {
            return Application.isMobilePlatform || Application.isEditor;
        }

        private static Transform FindStageMapRoot(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (string.Equals(root.name, StageMapRootName, StringComparison.Ordinal))
                {
                    return root.transform;
                }

                if (!string.Equals(root.name, CorridorStageRootName, StringComparison.Ordinal))
                {
                    continue;
                }

                Transform childMapRoot = root.transform.Find(StageMapRootName);
                if (childMapRoot != null)
                {
                    return childMapRoot;
                }
            }

            return null;
        }
    }
}
