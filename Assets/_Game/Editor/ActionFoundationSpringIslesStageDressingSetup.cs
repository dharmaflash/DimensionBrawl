using System;
using System.Collections.Generic;
using System.IO;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationSpringIslesStageDressingSetup
    {
        private const string ToonScapesRoot = "Assets/_Imported/AssetStore/ToonScapes";
        private const string SpringIslesRoot = ToonScapesRoot + "/Spring Isles";
        private const string PromotedRoot = "Assets/_Game/Art/Environment/SpringIsles";
        private const string PrefabRoot = PromotedRoot + "/Prefabs";
        private const string MaterialRoot = PromotedRoot + "/Materials";
        private const string MeshRoot = PromotedRoot + "/Meshes";
        private const string TextureRoot = PromotedRoot + "/Textures";
        private const string SourceAssetRoot = PromotedRoot + "/Source";
        private const string ProfileRoot = PromotedRoot + "/Profiles";
        private const string SceneDressingRootName = "StageBreakGateReview_SpringIslesDressing";
        private const string StageProgressionRootName = "StageBreakGateReview_ProgressionGates";
        private const string PostProcessProfilePath = ProfileRoot + "/DB_SpringIsles_Stage_PostProcess.asset";
        private const int MaxPromotedTextureDimension = 4096;

        private static readonly Dictionary<string, string> PromotedAssetPaths = new();

        [MenuItem("DimensionBrawl/Reapply Action Foundation Spring Isles Stage Dressing")]
        public static void ReapplySpringIslesStageDressingMenu()
        {
            ActionFoundationStageReviewSceneSetup.EnsureStageBreakGateReviewScene();
            ValidateSpringIslesStageDressingScene();
            Debug.Log("Reapplied S1-1 Spring Isles stage dressing.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Spring Isles Stage Dressing")]
        public static void ValidateSpringIslesStageDressingMenu()
        {
            ValidateSpringIslesStageDressingScene();
            Debug.Log("S1-1 Spring Isles stage dressing validation passed.");
        }

        public static void ApplyToOpenStageScene(Scene scene)
        {
            EnsurePromotedAssets();
            ConfigureAtmosphere(scene);
            ConfigureGround(scene);

            RemoveRoot(scene, SceneDressingRootName);
            RemoveRoot(scene, StageProgressionRootName);
            RemoveRoot(scene, "ActionFoundation_ArenaVfx");
            RemoveRoot(scene, "ActionFoundation_ArenaGrid");

            GameObject root = new GameObject(SceneDressingRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            StageMaterials materials = EnsureStageMaterials();
            CreateRouteStones(root.transform);
            CreateSideSilhouette(root.transform);
            CreateInvasionReadability(root.transform, materials);
            CreateExitRift(root.transform, materials);
            CreateLighting(root.transform);
            CreateProgressionGates(scene, materials);

            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        public static void ValidateSpringIslesStageDressingScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ActionFoundationStageReviewSceneSetup.BreakGateReviewScenePath, OpenSceneMode.Single);
            ValidateOpenScene(scene);
        }

        public static void ValidateOpenScene(Scene scene)
        {
            GameObject root = RequireRoot(scene, SceneDressingRootName);
            RequireChild(root.transform, "Route");
            RequireChild(root.transform, "SideSilhouette");
            RequireChild(root.transform, "InvasionReadability");
            RequireChild(root.transform, "ExitRift");

            GameObject progressionRoot = RequireRoot(scene, StageProgressionRootName);
            RequireChild(progressionRoot.transform, "PocketGates");
            RequireChild(progressionRoot.transform, "PocketObjectiveMarkers");
            RequireChild(progressionRoot.transform, "LaneBoundaryBlockers");
            RequireChild(progressionRoot.transform, "RouteFlowCues");
            Transform collisionRoot = RequireChild(progressionRoot.transform, "RouteCollision");

            StagePocketProgressionGatePresenter presenter = progressionRoot.GetComponent<StagePocketProgressionGatePresenter>();
            if (presenter == null || presenter.GateCount != 4)
            {
                throw new InvalidOperationException("S1-1 review scene should have four authored progression gates.");
            }

            for (int i = 0; i < presenter.GateCount; i++)
            {
                if (presenter.GetGateColliderCount(i) <= 0)
                {
                    throw new InvalidOperationException($"Progression gate {i} should include a blocking collider.");
                }
            }

            if (collisionRoot.GetComponentsInChildren<Collider>(includeInactive: true).Length < 3)
            {
                throw new InvalidOperationException("S1-1 route collision should cover start, route pockets, and exit approach.");
            }

            if (root.GetComponentsInChildren<Collider>(includeInactive: true).Length > 0)
            {
                throw new InvalidOperationException("Spring Isles dressing should stay presentation-only and must not add colliders.");
            }

            if (RenderSettings.skybox == null || !AssetDatabase.GetAssetPath(RenderSettings.skybox).StartsWith(PromotedRoot, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Spring Isles stage review should use a promoted _Game skybox material.");
            }

            ValidateNoImportedDependencies(root, SceneDressingRootName);
            ValidateRendererAssetDependencies(progressionRoot, StageProgressionRootName);
            ValidateFolderAssetDependencies(PrefabRoot);
            ValidateFolderAssetDependencies(MaterialRoot);
        }

        private static void EnsurePromotedAssets()
        {
            PromotedAssetPaths.Clear();
            EnsureFolder(PromotedRoot);
            EnsureFolder(PrefabRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(MeshRoot);
            EnsureFolder(TextureRoot);
            EnsureFolder(SourceAssetRoot);
            EnsureFolder(ProfileRoot);
            DeletePromotedModelImports();
            DeletePromotedRawTextureImports();

            PromoteMaterial(SpringIslesRoot + "/Skybox/Materials/TSI_Skybox_02A.mat");

            PromotePresentationPrefab("TSI_Stone_Pavement_05A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Pavement_05A.prefab");
            PromotePresentationPrefab("TSI_Stone_Pavement_06A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Pavement_06A.prefab");
            PromotePresentationPrefab("TSI_Stone_Pavement_01A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Pavement_01A.prefab");
            PromotePresentationPrefab("TSI_Stone_Pavement_02A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Pavement_02A.prefab");
            PromotePresentationPrefab("TSI_Stone_Pavement_03A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Pavement_03A.prefab");
            PromotePresentationPrefab("TSI_Stone_Pavement_04A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Pavement_04A.prefab");
            PromotePresentationPrefab("TSI_Stone_Floor_01A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Floor_01A.prefab");
            PromotePresentationPrefab("TSI_Stone_Floor_02A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Floor_02A.prefab");
            PromotePresentationPrefab("TSI_Stone_Platform_01A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Platform_01A.prefab");
            PromotePresentationPrefab("TSI_Stone_Bridge_01A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Bridge_01A.prefab");
            PromotePresentationPrefab("TSI_Stone_Arch_01A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Arch_01A.prefab");
            PromotePresentationPrefab("TSI_Stone_Stairs_02B", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Stairs_02B.prefab");
            PromotePresentationPrefab("TSI_Stone_Step_06A_Module_3", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Step_06A_Module_3.prefab");
            PromotePresentationPrefab("TSI_Stone_Block_16B", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Block_16B.prefab");
            PromotePresentationPrefab("TSI_Stone_Block_19A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Block_19A.prefab");
            PromotePresentationPrefab("TSI_Stone_Block_20B", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Block_20B.prefab");
            PromotePresentationPrefab("TSI_Stone_Wall_Straight_01A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Wall_Straight_01A.prefab");
            PromotePresentationPrefab("TSI_Stone_Wall_Curved_01A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Wall_Curved_01A.prefab");
            PromotePresentationPrefab("TSI_Stone_Lantern_02A", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Lantern_02A.prefab");
            PromotePresentationPrefab("TSI_Stone_Lantern_01B", SpringIslesRoot + "/Prefabs/Building Props/Stone Kit/TSI_Stone_Lantern_01B.prefab");
            PromotePresentationPrefab("TSI_Torii_Gate_01A", SpringIslesRoot + "/Prefabs/Building Props/Torii Gate/TSI_Torii_Gate_01A.prefab");
            PromotePresentationPrefab("TSI_Raked_Sand_02A", SpringIslesRoot + "/Prefabs/Props/Zen Garden Props/TSI_Raked_Sand_02A.prefab");
            PromotePresentationPrefab("TSI_Raked_Sand_03A", SpringIslesRoot + "/Prefabs/Props/Zen Garden Props/TSI_Raked_Sand_03A.prefab");
            PromotePresentationPrefab("TSI_River_Rock_01A", SpringIslesRoot + "/Prefabs/Props/Zen Garden Props/TSI_River_Rock_01A.prefab");
            PromotePresentationPrefab("TSI_River_Rock_02A", SpringIslesRoot + "/Prefabs/Props/Zen Garden Props/TSI_River_Rock_02A.prefab");
            PromotePresentationPrefab("TSI_Waterfall_02A", SpringIslesRoot + "/Prefabs/Water/TSI_Waterfall_02A.prefab");
            PromotePresentationPrefab("TSI_Gazebo_Board_01A", SpringIslesRoot + "/Prefabs/Building Props/Gazebo Kit/TSI_Gazebo_Board_01A.prefab");
            PromotePresentationPrefab("TSI_Gazebo_Railing_02A", SpringIslesRoot + "/Prefabs/Building Props/Gazebo Kit/TSI_Gazebo_Railing_02A.prefab");
            PromotePresentationPrefab("TSI_Gazebo_Roof_01A", SpringIslesRoot + "/Prefabs/Building Props/Gazebo Kit/TSI_Gazebo_Roof_01A.prefab");
            PromotePresentationPrefab("TSI_Paper_Lamp_03A", SpringIslesRoot + "/Prefabs/Props/Ornamental Props/TSI_Paper_Lamp_03A.prefab");
            PromotePresentationPrefab("TSI_BG_Cliff_01A", SpringIslesRoot + "/Prefabs/Background/TSI_BG_Cliff_01A.prefab");
            PromotePresentationPrefab("TSI_BG_Hill_01A", SpringIslesRoot + "/Prefabs/Background/TSI_BG_Hill_01A.prefab");
            PromotePresentationPrefab("TSI_BG_Hill_02A", SpringIslesRoot + "/Prefabs/Background/TSI_BG_Hill_02A.prefab");
            PromotePresentationPrefab("TSI_BG_Mountain_01A", SpringIslesRoot + "/Prefabs/Background/TSI_BG_Mountain_01A.prefab");
            PromotePresentationPrefab("TSI_BG_Mountain_02A", SpringIslesRoot + "/Prefabs/Background/TSI_BG_Mountain_02A.prefab");
            PromotePresentationPrefab("TSI_BG_Mountain_03A", SpringIslesRoot + "/Prefabs/Background/TSI_BG_Mountain_03A.prefab");
            PromotePresentationPrefab("TSI_Rock_Large_03A", SpringIslesRoot + "/Prefabs/Rocks/TSI_Rock_Large_03A.prefab");
            PromotePresentationPrefab("TSI_Rock_Medium_02A", SpringIslesRoot + "/Prefabs/Rocks/TSI_Rock_Medium_02A.prefab");
            PromotePresentationPrefab("TSI_Amberleaf_Tree_01A", SpringIslesRoot + "/Prefabs/Vegetation/Trees/TSI_Amberleaf_Tree_01A.prefab");
            PromotePresentationPrefab("TSI_Broadleaf_Tree_03A", SpringIslesRoot + "/Prefabs/Vegetation/Trees/TSI_Broadleaf_Tree_03A.prefab");
            PromotePresentationPrefab("TSI_Bamboo_03A", SpringIslesRoot + "/Prefabs/Vegetation/Bamboo/TSI_Bamboo_03A.prefab");
            PromotePresentationPrefab("TSI_Bamboo_04A", SpringIslesRoot + "/Prefabs/Vegetation/Bamboo/TSI_Bamboo_04A.prefab");
            PromotePresentationPrefab("TSI_Bamboo_06A", SpringIslesRoot + "/Prefabs/Vegetation/Bamboo/TSI_Bamboo_06A.prefab");
            PromotePresentationPrefab("TSI_Bush_02A", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Bush_02A.prefab");
            PromotePresentationPrefab("TSI_Bush_02B", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Bush_02B.prefab");
            PromotePresentationPrefab("TSI_Grass_Patch_03A", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Grass_Patch_03A.prefab");
            PromotePresentationPrefab("TSI_Grass_Patch_04A", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Grass_Patch_04A.prefab");
            PromotePresentationPrefab("TSI_Flower_Patch_02A", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Flower_Patch_02A.prefab");
            PromotePresentationPrefab("TSI_Flower_Bush_01A", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Flower_Bush_01A.prefab");
            PromotePresentationPrefab("TSI_Plant_21C", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Plant_21C.prefab");
            PromotePresentationPrefab("TSI_Wheat_Patch_01A", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Wheat_Patch_01A.prefab");
            PromotePresentationPrefab("TSI_Petals_01A", SpringIslesRoot + "/Prefabs/Vegetation/Plants & Flowers/TSI_Petals_01A.prefab");
            PromotePresentationPrefab("TSI_Blowing_Petals_01A", SpringIslesRoot + "/Particles/TSI_Blowing_Petals_01A.prefab");

            AssetDatabase.SaveAssets();
        }

        private static void ConfigureAtmosphere(Scene scene)
        {
            Material skybox = PromoteMaterial(SpringIslesRoot + "/Skybox/Materials/TSI_Skybox_02A.mat");
            RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.46f, 0.48f, 0.43f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.47f, 0.52f, 0.47f, 1f);
            RenderSettings.fogDensity = 0.009f;

            GameObject cameraObject = FindByName(scene.GetRootGameObjects(), "Main Camera");
            if (cameraObject != null && cameraObject.TryGetComponent(out Camera camera))
            {
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.allowHDR = true;
                camera.allowMSAA = true;
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = true;
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                EditorUtility.SetDirty(camera);
                EditorUtility.SetDirty(cameraData);
            }

            ConfigureDirectionalLights(scene);
        }

        private static void ConfigureDirectionalLights(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Light[] lights = roots[i].GetComponentsInChildren<Light>(includeInactive: true);
                for (int j = 0; j < lights.Length; j++)
                {
                    Light light = lights[j];
                    if (light == null || light.type != LightType.Directional)
                    {
                        continue;
                    }

                    light.transform.rotation = Quaternion.Euler(46f, -28f, 0f);
                    light.color = new Color(1f, 0.86f, 0.66f, 1f);
                    light.intensity = 1.05f;
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = 0.26f;
                    light.bounceIntensity = 0.15f;
                    EditorUtility.SetDirty(light);
                    EditorUtility.SetDirty(light.transform);
                }
            }
        }

        private static void ConfigureGround(Scene scene)
        {
            GameObject ground = FindByName(scene.GetRootGameObjects(), "ActionTest_Ground");
            if (ground == null || !ground.TryGetComponent(out Renderer renderer))
            {
                return;
            }

            renderer.sharedMaterial = EnsureStageMaterials().Ground;
            renderer.enabled = false;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            ground.transform.localPosition = new Vector3(0f, -0.08f, 25f);
            ground.transform.localScale = new Vector3(16f, 0.16f, 86f);
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(ground.transform);
        }

        private static void CreateRouteStones(Transform root)
        {
            Transform route = CreateChild(root, "Route", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform foundation = CreateChild(route, "StoneRouteFoundation", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform pockets = CreateChild(route, "PocketLandmarks", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform setPieces = CreateChild(route, "RouteSetPieces", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform ornaments = CreateChild(route, "RouteOrnaments", Vector3.zero, Quaternion.identity, Vector3.one).transform;

            string[] pavementKeys =
            {
                "TSI_Stone_Pavement_01A",
                "TSI_Stone_Pavement_02A",
                "TSI_Stone_Pavement_03A",
                "TSI_Stone_Pavement_04A",
                "TSI_Stone_Pavement_05A",
                "TSI_Stone_Pavement_06A"
            };

            for (int i = 0; i < 14; i++)
            {
                float z = -10f + i * 5.7f;
                string key = pavementKeys[i % pavementKeys.Length];
                PlacePrefab(foundation, key, $"Route_Pavement_{i + 1:00}", new Vector3(0f, 0.035f, z), Quaternion.Euler(0f, i * 13f, 0f), new Vector3(2.25f, 1f, 2.25f));
                if (i % 2 == 0)
                {
                    PlacePrefab(foundation, "TSI_Stone_Floor_01A", $"Route_FloorUnderlay_{i + 1:00}", new Vector3(-2.65f, 0.015f, z + 1.4f), Quaternion.Euler(0f, 8f - i * 5f, 0f), new Vector3(1.15f, 1f, 1.15f));
                    PlacePrefab(foundation, "TSI_Stone_Floor_02A", $"Route_FloorUnderlay_Right_{i + 1:00}", new Vector3(2.65f, 0.015f, z - 1.2f), Quaternion.Euler(0f, -11f + i * 3f, 0f), new Vector3(1.05f, 1f, 1.05f));
                }
            }

            PlacePrefab(pockets, "TSI_Stone_Platform_01A", "Pocket_EntryRead_Platform", new Vector3(0f, 0.055f, 1.6f), Quaternion.identity, new Vector3(1.65f, 1f, 1.65f));
            PlacePrefab(pockets, "TSI_Stone_Platform_01A", "Pocket_BasicPressure_Platform", new Vector3(0f, 0.055f, 13.5f), Quaternion.Euler(0f, 25f, 0f), new Vector3(1.95f, 1f, 1.75f));
            PlacePrefab(pockets, "TSI_Stone_Platform_01A", "Pocket_BreakGate_Platform", new Vector3(0f, 0.055f, 26.5f), Quaternion.Euler(0f, -18f, 0f), new Vector3(2.15f, 1f, 1.9f));
            PlacePrefab(pockets, "TSI_Stone_Bridge_01A", "Pocket_Relief_BridgeReset", new Vector3(0f, 0.035f, 36.0f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.55f, 1f, 1.35f));
            PlacePrefab(pockets, "TSI_Stone_Platform_01A", "Pocket_FinalStand_Platform", new Vector3(0f, 0.055f, 48.5f), Quaternion.Euler(0f, 35f, 0f), new Vector3(2.65f, 1f, 2.2f));

            PlacePrefab(setPieces, "TSI_Torii_Gate_01A", "Route_CorruptedTorii_Entrance", new Vector3(0f, 0.0f, -3.0f), Quaternion.Euler(0f, 0f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
            PlacePrefab(setPieces, "TSI_Stone_Arch_01A", "Route_BreakGate_StoneArch", new Vector3(0f, 0.0f, 23.5f), Quaternion.Euler(0f, 0f, 0f), new Vector3(1.3f, 1.3f, 1.3f));
            PlacePrefab(setPieces, "TSI_Torii_Gate_01A", "Route_CorruptedTorii_FinalGate", new Vector3(0f, 0.0f, 41.0f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.35f, 1.35f, 1.35f));
            PlacePrefab(setPieces, "TSI_Stone_Stairs_02B", "Route_ReliefBrokenStairs_Left", new Vector3(-4.6f, 0.0f, 34.4f), Quaternion.Euler(0f, 18f, 0f), new Vector3(0.95f, 0.95f, 0.95f));
            PlacePrefab(setPieces, "TSI_Stone_Step_06A_Module_3", "Route_ReliefBrokenStep_Right", new Vector3(4.8f, 0.0f, 37.4f), Quaternion.Euler(0f, -22f, 0f), new Vector3(1.05f, 1.05f, 1.05f));

            PlaceRouteLampPair(ornaments, "EntryLamp", -1.2f, 0.85f);
            PlaceRouteLampPair(ornaments, "BasicLamp", 12.8f, 0.9f);
            PlaceRouteLampPair(ornaments, "BreakLamp", 26.2f, 1.0f);
            PlaceRouteLampPair(ornaments, "FinalLamp", 48.2f, 1.08f);
            PlacePrefab(ornaments, "TSI_Gazebo_Board_01A", "Route_WeatheredBoard_Basic", new Vector3(-6.4f, 0f, 12.6f), Quaternion.Euler(0f, 24f, 0f), new Vector3(0.9f, 0.9f, 0.9f));
            PlacePrefab(ornaments, "TSI_Gazebo_Railing_02A", "Route_FracturedRailing_Relief", new Vector3(6.2f, 0f, 34.2f), Quaternion.Euler(0f, -18f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
            PlacePrefab(ornaments, "TSI_Paper_Lamp_03A", "Route_HangingLamp_FinalLeft", new Vector3(-5.8f, 2.25f, 42.0f), Quaternion.Euler(0f, 18f, 0f), new Vector3(0.75f, 0.75f, 0.75f));
            PlacePrefab(ornaments, "TSI_Paper_Lamp_03A", "Route_HangingLamp_FinalRight", new Vector3(5.6f, 2.2f, 43.8f), Quaternion.Euler(0f, -22f, 0f), new Vector3(0.75f, 0.75f, 0.75f));
        }

        private static void CreateSideSilhouette(Transform root)
        {
            Transform side = CreateChild(root, "SideSilhouette", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform vegetation = CreateChild(side, "VegetationDensity", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform water = CreateChild(side, "WaterAndRavineBackdrop", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform ruins = CreateChild(side, "RuinedShrineSilhouette", Vector3.zero, Quaternion.identity, Vector3.one).transform;

            PlaceSidePair(side, "TSI_Stone_Wall_Straight_01A", "SideWall_Entry", 8.8f, 3.0f, 1.1f, 0f);
            PlaceSidePair(side, "TSI_Stone_Wall_Curved_01A", "SideWall_BasicPressure", 9.5f, 15.5f, 1.2f, 12f);
            PlaceSidePair(side, "TSI_Stone_Wall_Straight_01A", "SideWall_BreakGate", 9.2f, 28.0f, 1.15f, -10f);
            PlaceSidePair(side, "TSI_Stone_Block_16B", "ChunkyBoundary_Basic", 10.6f, 12.0f, 1.1f, 19f);
            PlaceSidePair(side, "TSI_Stone_Block_19A", "ChunkyBoundary_Break", 10.8f, 25.2f, 1.25f, -12f);
            PlaceSidePair(side, "TSI_Stone_Block_20B", "ChunkyBoundary_Final", 11.0f, 47.8f, 1.3f, 28f);
            PlaceSidePair(side, "TSI_Rock_Large_03A", "LargeRock_Final", 11.2f, 49.0f, 1.7f, 24f);
            PlaceSidePair(side, "TSI_Rock_Medium_02A", "MediumRock_Relief", 7.4f, 36.2f, 1.2f, -18f);

            PlacePrefab(vegetation, "TSI_Amberleaf_Tree_01A", "Canopy_Left_Entry", new Vector3(-12.4f, 0f, 0.5f), Quaternion.Euler(0f, 26f, 0f), new Vector3(1.25f, 1.25f, 1.25f));
            PlacePrefab(vegetation, "TSI_Broadleaf_Tree_03A", "Canopy_Right_Basic", new Vector3(12.6f, 0f, 14.2f), Quaternion.Euler(0f, -31f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
            PlacePrefab(vegetation, "TSI_Amberleaf_Tree_01A", "Canopy_Left_Break", new Vector3(-13.2f, 0f, 29.0f), Quaternion.Euler(0f, -18f, 0f), new Vector3(1.35f, 1.35f, 1.35f));
            PlacePrefab(vegetation, "TSI_Broadleaf_Tree_03A", "Canopy_Right_Final", new Vector3(13.8f, 0f, 50.5f), Quaternion.Euler(0f, 40f, 0f), new Vector3(1.45f, 1.45f, 1.45f));
            CreateVegetationClusters(vegetation);

            PlacePrefab(water, "TSI_Raked_Sand_02A", "ZenSand_LeftReliefBed", new Vector3(-9.4f, 0.02f, 35.4f), Quaternion.Euler(0f, 12f, 0f), new Vector3(1.4f, 1f, 1.4f));
            PlacePrefab(water, "TSI_Raked_Sand_03A", "ZenSand_RightFinalBed", new Vector3(9.3f, 0.02f, 45.8f), Quaternion.Euler(0f, -18f, 0f), new Vector3(1.55f, 1f, 1.55f));
            PlacePrefab(water, "TSI_Waterfall_02A", "DistantWaterfall_LeftBreak", new Vector3(-17.5f, -0.4f, 31.0f), Quaternion.Euler(0f, 72f, 0f), new Vector3(1.4f, 1.4f, 1.4f));
            PlacePrefab(water, "TSI_River_Rock_01A", "RiverRock_LeftReliefA", new Vector3(-8.0f, 0f, 33.8f), Quaternion.Euler(0f, 21f, 0f), new Vector3(1.1f, 1.1f, 1.1f));
            PlacePrefab(water, "TSI_River_Rock_02A", "RiverRock_RightReliefB", new Vector3(8.4f, 0f, 37.8f), Quaternion.Euler(0f, -16f, 0f), new Vector3(1.25f, 1.25f, 1.25f));

            PlacePrefab(ruins, "TSI_Gazebo_Roof_01A", "BrokenGazeboRoof_BackLeft", new Vector3(-13.6f, 1.15f, 20.4f), Quaternion.Euler(0f, 28f, -8f), new Vector3(0.92f, 0.92f, 0.92f));
            PlacePrefab(ruins, "TSI_Gazebo_Railing_02A", "BrokenGazeboRailing_BackRight", new Vector3(11.6f, 0f, 22.6f), Quaternion.Euler(0f, -42f, 0f), new Vector3(1.15f, 1.15f, 1.15f));
            PlacePrefab(ruins, "TSI_Gazebo_Board_01A", "BrokenGazeboBoard_Final", new Vector3(-10.5f, 0f, 53.0f), Quaternion.Euler(0f, 14f, 0f), new Vector3(1.1f, 1.1f, 1.1f));

            PlacePrefab(side, "TSI_BG_Hill_01A", "Background_Hill_Left", new Vector3(-23f, -0.6f, 62f), Quaternion.Euler(0f, 22f, 0f), new Vector3(5.4f, 5.4f, 5.4f));
            PlacePrefab(side, "TSI_BG_Hill_02A", "Background_Hill_RightLayer", new Vector3(21f, -0.8f, 58f), Quaternion.Euler(0f, -32f, 0f), new Vector3(5.2f, 5.2f, 5.2f));
            PlacePrefab(side, "TSI_BG_Cliff_01A", "Background_Cliff_Right", new Vector3(24f, -0.7f, 66f), Quaternion.Euler(0f, -24f, 0f), new Vector3(5.8f, 5.8f, 5.8f));
            PlacePrefab(side, "TSI_BG_Mountain_01A", "Background_Mountain_Center", new Vector3(0f, -1.2f, 84f), Quaternion.identity, new Vector3(8.5f, 8.5f, 8.5f));
            PlacePrefab(side, "TSI_BG_Mountain_02A", "Background_Mountain_LeftLayer", new Vector3(-18f, -1.1f, 80f), Quaternion.Euler(0f, 16f, 0f), new Vector3(7.2f, 7.2f, 7.2f));
            PlacePrefab(side, "TSI_BG_Mountain_03A", "Background_Mountain_RightLayer", new Vector3(18f, -1.4f, 88f), Quaternion.Euler(0f, -19f, 0f), new Vector3(7.8f, 7.8f, 7.8f));
        }

        private static void CreateInvasionReadability(Transform root, StageMaterials materials)
        {
            Transform invasion = CreateChild(root, "InvasionReadability", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            CreateScanBand(invasion, "ScanBand_EntryRead", new Vector3(0f, 0.08f, 1.6f), new Vector3(14f, 0.035f, 0.22f), 3.6f, 0.12f, materials.RiftBlue);
            CreateScanBand(invasion, "ScanBand_BreakGate", new Vector3(0f, 0.09f, 26.5f), new Vector3(16f, 0.035f, 0.26f), 4.4f, 0.1f, materials.RiftBlue);
            CreateScanBand(invasion, "ScanBand_FinalStand", new Vector3(0f, 0.1f, 48.5f), new Vector3(19f, 0.035f, 0.3f), 5.0f, 0.085f, materials.RiftBlue);

            CreateCrack(invasion, "RiftCrack_BasicPressure_Left", new Vector3(-3.8f, 0.075f, 13.2f), Quaternion.Euler(0f, 28f, 0f), new Vector3(0.18f, 0.03f, 4.8f), materials.FireEmber);
            CreateCrack(invasion, "RiftCrack_BasicPressure_Right", new Vector3(4.2f, 0.075f, 15.0f), Quaternion.Euler(0f, -35f, 0f), new Vector3(0.16f, 0.03f, 3.9f), materials.RiftViolet);
            CreateCrack(invasion, "RiftCrack_BreakGate_CrossA", new Vector3(-0.8f, 0.08f, 26.4f), Quaternion.Euler(0f, 55f, 0f), new Vector3(0.22f, 0.035f, 7.2f), materials.FireEmber);
            CreateCrack(invasion, "RiftCrack_BreakGate_CrossB", new Vector3(0.8f, 0.082f, 26.7f), Quaternion.Euler(0f, -58f, 0f), new Vector3(0.18f, 0.035f, 6.2f), materials.RiftViolet);
            CreateCrack(invasion, "RiftCrack_FinalStand_Center", new Vector3(0f, 0.085f, 48.6f), Quaternion.Euler(0f, 18f, 0f), new Vector3(0.26f, 0.04f, 9.2f), materials.FireEmber);

            for (int i = 0; i < 12; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float z = 5f + i * 4.2f;
                float x = side * (5.8f + (i % 3) * 1.1f);
                GameObject ember = CreatePrimitive(invasion, $"FloatingEmberShard_{i + 1:00}", PrimitiveType.Cube, new Vector3(x, 1.8f + (i % 4) * 0.45f, z), Quaternion.Euler(i * 11f, i * 37f, i * 19f), new Vector3(0.16f, 0.08f, 0.46f), materials.FireEmber);
                ActionFoundationArenaTransformMotion motion = ember.AddComponent<ActionFoundationArenaTransformMotion>();
                motion.Configure(new Vector3(0f, 38f + i * 3f, 12f), new Vector3(0.15f, 1f, 0.2f), 0.34f, 0.16f + i * 0.05f, i * 0.31f);
                EditorUtility.SetDirty(motion);
            }
        }

        private static void CreateExitRift(Transform root, StageMaterials materials)
        {
            Transform exit = CreateChild(root, "ExitRift", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            GameObject disc = CreatePrimitive(exit, "ExitRift_DimensionalDisc", PrimitiveType.Cylinder, new Vector3(0f, 6.5f, 66f), Quaternion.Euler(90f, 0f, 0f), new Vector3(8.2f, 0.04f, 8.2f), materials.RiftBlue);
            ActionFoundationArenaFloatingShape discMotion = disc.AddComponent<ActionFoundationArenaFloatingShape>();
            discMotion.Configure(new Vector3(0f, 0f, 6.5f), Vector3.up, 0.28f, 0.16f, 0.4f, new Color(0.28f, 0.86f, 1f, 0.55f), new Color(0.08f, 0.72f, 1.25f, 1f), 0.18f, 0.4f);
            EditorUtility.SetDirty(discMotion);

            GameObject core = CreatePrimitive(exit, "ExitRift_Core", PrimitiveType.Sphere, new Vector3(0f, 6.5f, 65.8f), Quaternion.identity, new Vector3(2.2f, 2.2f, 2.2f), materials.RiftViolet);
            ActionFoundationArenaFloatingShape coreMotion = core.AddComponent<ActionFoundationArenaFloatingShape>();
            coreMotion.Configure(new Vector3(0f, 24f, 0f), Vector3.up, 0.18f, 0.22f, 1.1f, new Color(0.46f, 0.28f, 1f, 0.48f), new Color(0.32f, 0.18f, 1.4f, 1f), 0.22f, 0.52f);
            EditorUtility.SetDirty(coreMotion);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 5.2f, 6.5f + Mathf.Sin(i * 1.7f) * 1.1f, 65f + Mathf.Sin(angle) * 2.4f);
                GameObject shard = CreatePrimitive(exit, $"ExitRift_OrbitShard_{i + 1:00}", PrimitiveType.Cube, position, Quaternion.Euler(i * 13f, i * 45f, i * 23f), new Vector3(0.34f, 0.11f, 0.9f), materials.RiftBlue);
                ActionFoundationArenaTransformMotion motion = shard.AddComponent<ActionFoundationArenaTransformMotion>();
                motion.Configure(new Vector3(12f, 46f, 8f), Vector3.up, 0.25f, 0.18f, i * 0.4f);
                EditorUtility.SetDirty(motion);
            }
        }

        private static void CreateLighting(Transform root)
        {
            Transform lighting = CreateChild(root, "Lighting", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            CreatePointLight(lighting, "RiftCoolBackLight", new Vector3(0f, 5.2f, 56f), new Color(0.24f, 0.62f, 1f, 1f), 2.3f, 30f);
            CreatePointLight(lighting, "FireSideLight_Left", new Vector3(-6.2f, 2.2f, 25.5f), new Color(1f, 0.34f, 0.11f, 1f), 0.9f, 12f);
            CreatePointLight(lighting, "FireSideLight_Right", new Vector3(6.4f, 2.1f, 48.5f), new Color(1f, 0.42f, 0.14f, 1f), 0.8f, 13f);
        }

        private static void CreateProgressionGates(Scene scene, StageMaterials materials)
        {
            StageEncounterReviewOwner owner = RequireComponentInScene<StageEncounterReviewOwner>(scene, "stage encounter review owner");

            GameObject root = new GameObject(StageProgressionRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            StagePocketProgressionGatePresenter presenter = root.AddComponent<StagePocketProgressionGatePresenter>();
            Transform gateRoot = CreateChild(root.transform, "PocketGates", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform markerRoot = CreateChild(root.transform, "PocketObjectiveMarkers", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform boundaryRoot = CreateChild(root.transform, "LaneBoundaryBlockers", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform cueRoot = CreateChild(root.transform, "RouteFlowCues", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform collisionRoot = CreateChild(root.transform, "RouteCollision", Vector3.zero, Quaternion.identity, Vector3.one).transform;

            StagePocketProgressionGateBinding[] gates =
            {
                CreateProgressionGate(gateRoot, "Gate_01_EntryRead_ClearWall", 0, 7.6f, 13.6f, materials),
                CreateProgressionGate(gateRoot, "Gate_02_BasicPressure_ClearWall", 1, 20.6f, 14.4f, materials),
                CreateProgressionGate(gateRoot, "Gate_03_BreakGate_ClearWall", 2, 32.8f, 15.2f, materials),
                CreateProgressionGate(gateRoot, "Gate_04_Relief_ClearWall", 3, 41.4f, 16.8f, materials)
            };

            CreatePocketObjectiveMarkers(markerRoot, materials);
            CreateLaneBoundaryBlockers(boundaryRoot, materials);
            CreateRouteFlowCues(cueRoot, materials);
            CreateRouteCollision(collisionRoot, materials);

            presenter.Configure(owner, gates);
            EditorUtility.SetDirty(presenter);
            EditorUtility.SetDirty(root);
        }

        private static StagePocketProgressionGateBinding CreateProgressionGate(
            Transform parent,
            string name,
            int unlockAfterPocketIndex,
            float z,
            float width,
            StageMaterials materials)
        {
            Transform gate = CreateChild(parent, name, Vector3.zero, Quaternion.identity, Vector3.one).transform;
            var visuals = new List<Renderer>();

            GameObject panel = CreateBlockingPanel(
                gate,
                name + "_Barrier",
                new Vector3(0f, 1.45f, z),
                Quaternion.identity,
                new Vector3(width, 2.9f, 0.22f),
                materials.GateWall,
                out Collider blocker);
            AddRenderer(visuals, panel);

            for (int i = 0; i < 3; i++)
            {
                float y = 0.55f + i * 0.85f;
                GameObject scan = CreatePrimitive(
                    gate,
                    name + $"_ScanLine_{i + 1:00}",
                    PrimitiveType.Cube,
                    new Vector3(0f, y, z - 0.14f),
                    Quaternion.identity,
                    new Vector3(width * 0.96f, 0.035f, 0.08f),
                    materials.RiftBlue);
                ActionFoundationArenaTransformMotion motion = scan.AddComponent<ActionFoundationArenaTransformMotion>();
                motion.Configure(Vector3.zero, Vector3.right, width * 0.08f, 0.22f + i * 0.04f, i * 0.2f);
                EditorUtility.SetDirty(motion);
                AddRenderer(visuals, scan);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * (width * 0.5f + 0.55f);
                PlacePrefab(
                    gate,
                    "TSI_Stone_Lantern_02A",
                    name + (side < 0 ? "_LeftPylon" : "_RightPylon"),
                    new Vector3(x, 0f, z - 0.35f),
                    Quaternion.Euler(0f, side < 0 ? 90f : -90f, 0f),
                    new Vector3(0.9f, 0.9f, 0.9f));
                GameObject edge = CreatePrimitive(
                    gate,
                    name + (side < 0 ? "_LeftLightSpine" : "_RightLightSpine"),
                    PrimitiveType.Cube,
                    new Vector3(x, 1.65f, z - 0.15f),
                    Quaternion.identity,
                    new Vector3(0.1f, 2.5f, 0.1f),
                    materials.RiftViolet);
                AddRenderer(visuals, edge);
            }

            return new StagePocketProgressionGateBinding(
                name,
                unlockAfterPocketIndex,
                gate.gameObject,
                new[] { blocker },
                visuals.ToArray());
        }

        private static void CreatePocketObjectiveMarkers(Transform parent, StageMaterials materials)
        {
            CreatePocketMarker(parent, "Pocket_01_EntryRead_ReadThreat", new Vector3(0f, 0.07f, 1.6f), 5.5f, materials.RiftBlue);
            CreatePocketMarker(parent, "Pocket_02_BasicPressure_PunishRecovery", new Vector3(0f, 0.075f, 13.5f), 7f, materials.RiftBlue);
            CreatePocketMarker(parent, "Pocket_03_BreakGate_BreakGuard", new Vector3(0f, 0.08f, 26.5f), 7.5f, materials.RiftViolet);
            CreatePocketMarker(parent, "Pocket_04_Relief_RecoverPosition", new Vector3(0f, 0.07f, 36f), 5.8f, materials.RiftBlue);
            CreatePocketMarker(parent, "Pocket_05_FinalStand_FinalClear", new Vector3(0f, 0.085f, 48.5f), 10.5f, materials.FireEmber);
        }

        private static void CreatePocketMarker(
            Transform parent,
            string name,
            Vector3 localPosition,
            float radius,
            Material material)
        {
            GameObject marker = CreatePrimitive(
                parent,
                name + "_GroundField",
                PrimitiveType.Cylinder,
                localPosition,
                Quaternion.identity,
                new Vector3(radius * 2f, 0.015f, radius * 2f),
                material);
            ActionFoundationArenaTransformMotion fieldMotion = marker.AddComponent<ActionFoundationArenaTransformMotion>();
            fieldMotion.Configure(new Vector3(0f, 18f, 0f), Vector3.up, 0.02f, 0.12f, radius * 0.03f);
            EditorUtility.SetDirty(fieldMotion);

            GameObject beacon = CreatePrimitive(
                parent,
                name + "_ObjectiveBeacon",
                PrimitiveType.Cube,
                localPosition + new Vector3(0f, 2.6f, 0f),
                Quaternion.Euler(45f, 45f, 0f),
                new Vector3(0.42f, 0.42f, 0.42f),
                material);
            ActionFoundationArenaFloatingShape beaconMotion = beacon.AddComponent<ActionFoundationArenaFloatingShape>();
            beaconMotion.Configure(Vector3.up * 18f, Vector3.up, 0.22f, 0.18f, radius * 0.08f, new Color(0.2f, 0.85f, 1f, 0.4f), new Color(0.2f, 0.85f, 1.4f, 1f), 0.18f, 0.34f);
            EditorUtility.SetDirty(beaconMotion);
        }

        private static void CreateLaneBoundaryBlockers(Transform parent, StageMaterials materials)
        {
            CreateBoundarySide(parent, "LaneBoundary_Left", -12.2f, materials);
            CreateBoundarySide(parent, "LaneBoundary_Right", 12.2f, materials);
        }

        private static void CreateBoundarySide(Transform parent, string name, float x, StageMaterials materials)
        {
            GameObject wall = CreateBlockingPanel(
                parent,
                name + "_SoftWall",
                new Vector3(x, 1.4f, 25.5f),
                Quaternion.identity,
                new Vector3(0.28f, 2.8f, 80f),
                materials.BoundaryWall,
                out _);
            Renderer wallRenderer = wall.GetComponent<Renderer>();
            if (wallRenderer != null)
            {
                wallRenderer.enabled = true;
            }

            for (int i = 0; i < 7; i++)
            {
                CreatePrimitive(
                    parent,
                    name + $"_GuidePulse_{i + 1:00}",
                    PrimitiveType.Cube,
                    new Vector3(x * 0.98f, 0.12f, -8f + i * 11f),
                    Quaternion.identity,
                    new Vector3(0.08f, 0.035f, 3.6f),
                    materials.RiftBlue);
            }
        }

        private static void CreateRouteFlowCues(Transform parent, StageMaterials materials)
        {
            CreateStartCue(parent, materials);
            CreateForwardCue(parent, "RouteCue_EntryToBasic", 5.2f, 3, materials.RiftBlue);
            CreateForwardCue(parent, "RouteCue_BasicToBreak", 18.2f, 3, materials.RiftBlue);
            CreateForwardCue(parent, "RouteCue_BreakToRelief", 31.1f, 2, materials.RiftViolet);
            CreateForwardCue(parent, "RouteCue_ReliefToFinal", 39.2f, 3, materials.RiftBlue);
            CreateForwardCue(parent, "RouteCue_FinalToExit", 59.2f, 4, materials.FireEmber);
        }

        private static void CreateStartCue(Transform parent, StageMaterials materials)
        {
            CreateScanBand(
                parent,
                "StartBoundary_EntryWake",
                new Vector3(0f, 0.11f, -10.8f),
                new Vector3(11.5f, 0.04f, 0.2f),
                1.4f,
                0.16f,
                materials.RiftBlue);
        }

        private static void CreateForwardCue(
            Transform parent,
            string name,
            float startZ,
            int count,
            Material material)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject cue = CreatePrimitive(
                    parent,
                    name + $"_{i + 1:00}",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0.12f, startZ + i * 2.2f),
                    Quaternion.Euler(0f, 45f, 0f),
                    new Vector3(0.55f, 0.035f, 1.5f),
                    material);
                ActionFoundationArenaTransformMotion motion = cue.AddComponent<ActionFoundationArenaTransformMotion>();
                motion.Configure(Vector3.zero, Vector3.forward, 0.45f, 0.18f, i * 0.18f);
                EditorUtility.SetDirty(motion);
            }
        }

        private static void CreateRouteCollision(Transform parent, StageMaterials materials)
        {
            GameObject mainRoute = CreateBlockingPanel(
                parent,
                "RouteFloorCollider_MainPath",
                new Vector3(0f, -0.08f, 26f),
                Quaternion.identity,
                new Vector3(15.5f, 0.16f, 78f),
                materials.CollisionPreview,
                out _);
            HideRenderer(mainRoute);

            GameObject startApron = CreateBlockingPanel(
                parent,
                "RouteFloorCollider_StartApron",
                new Vector3(0f, -0.08f, -10.2f),
                Quaternion.identity,
                new Vector3(13.5f, 0.16f, 9.5f),
                materials.CollisionPreview,
                out _);
            HideRenderer(startApron);

            GameObject finalApron = CreateBlockingPanel(
                parent,
                "RouteFloorCollider_FinalExit",
                new Vector3(0f, -0.08f, 62.5f),
                Quaternion.identity,
                new Vector3(18f, 0.16f, 12f),
                materials.CollisionPreview,
                out _);
            HideRenderer(finalApron);
        }

        private static void HideRenderer(GameObject target)
        {
            Renderer renderer = target != null ? target.GetComponent<Renderer>() : null;
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        private static void PlaceRouteLampPair(Transform parent, string baseName, float z, float scale)
        {
            PlacePrefab(parent, "TSI_Stone_Lantern_01B", baseName + "_Left", new Vector3(-5.4f, 0f, z), Quaternion.Euler(0f, 88f, 0f), new Vector3(scale, scale, scale));
            PlacePrefab(parent, "TSI_Stone_Lantern_01B", baseName + "_Right", new Vector3(5.4f, 0f, z + 0.6f), Quaternion.Euler(0f, -88f, 0f), new Vector3(scale, scale, scale));
        }

        private static void CreateVegetationClusters(Transform parent)
        {
            string[] denseKeys =
            {
                "TSI_Grass_Patch_03A",
                "TSI_Grass_Patch_04A",
                "TSI_Bush_02A",
                "TSI_Bush_02B",
                "TSI_Plant_21C",
                "TSI_Wheat_Patch_01A"
            };

            for (int i = 0; i < 24; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float z = -4f + i * 2.7f;
                float x = side * (6.8f + (i % 4) * 1.15f);
                string key = denseKeys[i % denseKeys.Length];
                PlacePrefab(
                    parent,
                    key,
                    $"RouteVegetation_{i + 1:00}_{key}",
                    new Vector3(x, 0.02f, z),
                    Quaternion.Euler(0f, i * 31f, 0f),
                    Vector3.one * (0.72f + (i % 5) * 0.08f));
            }

            for (int i = 0; i < 10; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                string bambooKey = i % 3 == 0 ? "TSI_Bamboo_03A" : i % 3 == 1 ? "TSI_Bamboo_04A" : "TSI_Bamboo_06A";
                PlacePrefab(
                    parent,
                    bambooKey,
                    $"BambooScreen_{i + 1:00}",
                    new Vector3(side * (10.5f + (i % 2) * 1.3f), 0f, 4f + i * 5.1f),
                    Quaternion.Euler(0f, side * (18f + i * 7f), 0f),
                    Vector3.one * (0.95f + (i % 4) * 0.1f));
            }

            PlacePrefab(parent, "TSI_Flower_Patch_02A", "FlowerPatch_Relief_Left", new Vector3(-6.4f, 0.02f, 35.8f), Quaternion.Euler(0f, 28f, 0f), Vector3.one);
            PlacePrefab(parent, "TSI_Flower_Bush_01A", "FlowerBush_Final_Right", new Vector3(7.4f, 0f, 49.6f), Quaternion.Euler(0f, -42f, 0f), Vector3.one * 0.95f);
            PlacePrefab(parent, "TSI_Petals_01A", "PetalLayer_Entry", new Vector3(-4.2f, 0.035f, 2.2f), Quaternion.Euler(0f, 12f, 0f), Vector3.one * 1.2f);
            PlacePrefab(parent, "TSI_Blowing_Petals_01A", "BlowingPetals_FinalBreeze", new Vector3(4.8f, 1.8f, 46.5f), Quaternion.Euler(0f, -35f, 0f), Vector3.one);
        }

        private static void PlaceSidePair(Transform parent, string prefabKey, string baseName, float x, float z, float scale, float yaw)
        {
            PlacePrefab(parent, prefabKey, baseName + "_Left", new Vector3(-x, 0f, z), Quaternion.Euler(0f, yaw, 0f), new Vector3(scale, scale, scale));
            PlacePrefab(parent, prefabKey, baseName + "_Right", new Vector3(x, 0f, z), Quaternion.Euler(0f, 180f - yaw, 0f), new Vector3(scale, scale, scale));
        }

        private static GameObject PlacePrefab(Transform parent, string prefabKey, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject prefab = LoadPromotedPrefab(prefabKey);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to instantiate promoted Spring Isles prefab {prefabKey}.");
            }

            instance.name = name;
            instance.transform.SetParent(parent, worldPositionStays: false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            RemoveColliders(instance);
            return instance;
        }

        private static GameObject LoadPromotedPrefab(string prefabKey)
        {
            string path = PrefabRoot + "/PF_Env_SpringIsles_" + prefabKey + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Missing promoted Spring Isles prefab {path}.");
            }

            return prefab;
        }

        private static void CreateScanBand(Transform parent, string name, Vector3 localPosition, Vector3 localScale, float amplitude, float frequency, Material material)
        {
            GameObject band = CreatePrimitive(parent, name, PrimitiveType.Cube, localPosition, Quaternion.identity, localScale, material);
            ActionFoundationArenaTransformMotion motion = band.AddComponent<ActionFoundationArenaTransformMotion>();
            motion.Configure(Vector3.zero, Vector3.forward, amplitude, frequency, 0.35f);
            EditorUtility.SetDirty(motion);
        }

        private static void CreateCrack(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            CreatePrimitive(parent, name, PrimitiveType.Cube, localPosition, localRotation, localScale, material);
        }

        private static GameObject CreatePrimitive(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material)
        {
            GameObject target = GameObject.CreatePrimitive(primitiveType);
            target.name = name;
            target.transform.SetParent(parent, worldPositionStays: false);
            target.transform.localPosition = localPosition;
            target.transform.localRotation = localRotation;
            target.transform.localScale = localScale;
            RemoveColliders(target);

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return target;
        }

        private static GameObject CreateBlockingPanel(
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material,
            out Collider blocker)
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = name;
            target.transform.SetParent(parent, worldPositionStays: false);
            target.transform.localPosition = localPosition;
            target.transform.localRotation = localRotation;
            target.transform.localScale = localScale;

            blocker = target.GetComponent<Collider>();
            if (blocker != null)
            {
                blocker.isTrigger = false;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return target;
        }

        private static void AddRenderer(List<Renderer> renderers, GameObject target)
        {
            Renderer renderer = target != null ? target.GetComponent<Renderer>() : null;
            if (renderer != null)
            {
                renderers.Add(renderer);
            }
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 localPosition, Color color, float intensity, float range)
        {
            GameObject target = CreateChild(parent, name, localPosition, Quaternion.identity, Vector3.one);
            Light light = target.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            light.bounceIntensity = 0f;
            EditorUtility.SetDirty(light);
        }

        private static GameObject CreateChild(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = localRotation;
            child.transform.localScale = localScale;
            return child;
        }

        private static StageMaterials EnsureStageMaterials()
        {
            Material ground = LoadOrCreateOpaqueMaterial(
                MaterialRoot + "/DB_SpringIsles_MutedRouteGround.mat",
                new Color(0.22f, 0.245f, 0.205f, 1f),
                new Color(0.015f, 0.025f, 0.02f, 1f),
                0.25f);
            Material riftBlue = LoadOrCreateTransparentMaterial(
                MaterialRoot + "/DB_SpringIsles_RiftBlue.mat",
                new Color(0.05f, 0.76f, 1f, 0.46f),
                new Color(0.03f, 0.85f, 1.6f, 1f),
                additive: true);
            Material riftViolet = LoadOrCreateTransparentMaterial(
                MaterialRoot + "/DB_SpringIsles_RiftViolet.mat",
                new Color(0.55f, 0.24f, 1f, 0.48f),
                new Color(0.52f, 0.22f, 1.7f, 1f),
                additive: true);
            Material fireEmber = LoadOrCreateTransparentMaterial(
                MaterialRoot + "/DB_SpringIsles_FireEmber.mat",
                new Color(1f, 0.32f, 0.08f, 0.58f),
                new Color(1.8f, 0.32f, 0.06f, 1f),
                additive: true);
            Material gateWall = LoadOrCreateTransparentMaterial(
                MaterialRoot + "/DB_SpringIsles_GateWall.mat",
                new Color(0.1f, 0.92f, 1f, 0.28f),
                new Color(0.05f, 0.95f, 1.4f, 1f),
                additive: false);
            Material boundaryWall = LoadOrCreateTransparentMaterial(
                MaterialRoot + "/DB_SpringIsles_BoundaryWall.mat",
                new Color(0.08f, 0.42f, 0.52f, 0.16f),
                new Color(0.02f, 0.62f, 0.8f, 1f),
                additive: false);
            Material collisionPreview = LoadOrCreateTransparentMaterial(
                MaterialRoot + "/DB_SpringIsles_CollisionPreview.mat",
                new Color(0f, 0.4f, 0.35f, 0.08f),
                new Color(0f, 0.2f, 0.12f, 1f),
                additive: false);

            return new StageMaterials(ground, riftBlue, riftViolet, fireEmber, gateWall, boundaryWall, collisionPreview);
        }

        private static Material LoadOrCreateOpaqueMaterial(string path, Color baseColor, Color emissionColor, float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = Path.GetFileNameWithoutExtension(path);
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_Color", baseColor);
            material.SetColor("_EmissionColor", emissionColor);
            SetFloatIfPresent(material, "_Smoothness", smoothness);
            SetFloatIfPresent(material, "_Metallic", 0f);
            material.EnableKeyword("_EMISSION");
            ConfigureOpaqueMaterial(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTransparentMaterial(string path, Color baseColor, Color emissionColor, bool additive)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = Path.GetFileNameWithoutExtension(path);
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_Color", baseColor);
            material.SetColor("_EmissionColor", emissionColor);
            ConfigureTransparentMaterial(material, additive);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureOpaqueMaterial(Material material)
        {
            SetFloatIfPresent(material, "_Surface", 0f);
            SetFloatIfPresent(material, "_Blend", 0f);
            SetFloatIfPresent(material, "_AlphaClip", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.One);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.Zero);
            SetFloatIfPresent(material, "_ZWrite", 1f);
            material.renderQueue = -1;
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        private static void ConfigureTransparentMaterial(Material material, bool additive)
        {
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", additive ? 2f : 0f);
            SetFloatIfPresent(material, "_AlphaClip", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_QueueOffset", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_EMISSION");
            if (additive)
            {
                material.EnableKeyword("_BLENDMODE_ADD");
            }
        }

        private static string PromotePresentationPrefab(string prefabKey, string sourcePath)
        {
            string destination = PrefabRoot + "/PF_Env_SpringIsles_" + prefabKey + ".prefab";
            GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (sourcePrefab == null)
            {
                throw new InvalidOperationException($"Missing ToonScapes source prefab at {sourcePath}.");
            }

            GameObject temp = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (temp == null)
            {
                throw new InvalidOperationException($"Failed to instantiate ToonScapes source prefab {sourcePath}.");
            }

            PrefabUtility.UnpackPrefabInstance(temp, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            temp.name = "PF_Env_SpringIsles_" + prefabKey;
            RemoveColliders(temp);
            RemoveSourceScripts(temp);
            ReplaceMeshReferences(temp);
            ReplaceRendererMaterials(temp);

            EnsureFolder(Path.GetDirectoryName(destination)?.Replace("\\", "/"));
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(temp, destination);
            UnityEngine.Object.DestroyImmediate(temp);
            if (saved == null)
            {
                throw new InvalidOperationException($"Failed to save promoted Spring Isles prefab at {destination}.");
            }

            EditorUtility.SetDirty(saved);
            return destination;
        }

        private static void ReplaceMeshReferences(GameObject root)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Mesh mesh = meshFilters[i].sharedMesh;
                if (mesh != null)
                {
                    meshFilters[i].sharedMesh = PromoteMesh(mesh);
                    EditorUtility.SetDirty(meshFilters[i]);
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                Mesh mesh = skinnedRenderers[i].sharedMesh;
                if (mesh != null)
                {
                    skinnedRenderers[i].sharedMesh = PromoteMesh(mesh);
                    EditorUtility.SetDirty(skinnedRenderers[i]);
                }
            }

            ParticleSystemRenderer[] particleRenderers = root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                Mesh[] meshes = new Mesh[4];
                int meshCount = particleRenderers[i].GetMeshes(meshes);
                bool changed = false;
                for (int j = 0; j < meshCount; j++)
                {
                    if (meshes[j] == null)
                    {
                        continue;
                    }

                    Mesh promoted = PromoteMesh(meshes[j]);
                    if (promoted != meshes[j])
                    {
                        meshes[j] = promoted;
                        changed = true;
                    }
                }

                if (changed)
                {
                    particleRenderers[i].SetMeshes(meshes, meshCount);
                    EditorUtility.SetDirty(particleRenderers[i]);
                }
            }
        }

        private static void ReplaceRendererMaterials(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] != null)
                    {
                        materials[j] = PromoteMaterial(materials[j]);
                    }
                }

                renderers[i].sharedMaterials = materials;
                renderers[i].shadowCastingMode = ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
                EditorUtility.SetDirty(renderers[i]);
            }
        }

        private static Mesh PromoteMesh(Mesh sourceMesh)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith(ToonScapesRoot, StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            string destination = ToPromotedMeshPath(sourcePath, sourceMesh.name);
            Mesh promoted = AssetDatabase.LoadAssetAtPath<Mesh>(destination);
            if (promoted != null)
            {
                return promoted;
            }

            Mesh mesh = UnityEngine.Object.Instantiate(sourceMesh);
            mesh.name = sourceMesh.name;
            EnsureFolder(Path.GetDirectoryName(destination)?.Replace("\\", "/"));
            AssetDatabase.CreateAsset(mesh, destination);
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static Material PromoteMaterial(Material sourceMaterial)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial);
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith(ToonScapesRoot, StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            string destination = ToPromotedAssetPath(sourcePath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(destination);
            if (material == null)
            {
                Shader shader = PromoteShader(sourceMaterial.shader);
                material = new Material(shader);
                EnsureFolder(Path.GetDirectoryName(destination)?.Replace("\\", "/"));
                AssetDatabase.CreateAsset(material, destination);
            }

            material.CopyPropertiesFromMaterial(sourceMaterial);
            material.shader = PromoteShader(sourceMaterial.shader);
            PromoteMaterialTextures(sourceMaterial, material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material PromoteMaterial(string sourcePath)
        {
            Material source = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
            if (source == null)
            {
                throw new InvalidOperationException($"Missing ToonScapes material at {sourcePath}.");
            }

            return PromoteMaterial(source);
        }

        private static void PromoteMaterialTextures(Material sourceMaterial, Material destinationMaterial)
        {
            Shader shader = sourceMaterial.shader;
            if (shader == null)
            {
                return;
            }

            int propertyCount = shader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                if (shader.GetPropertyType(i) != ShaderPropertyType.Texture)
                {
                    continue;
                }

                string propertyName = shader.GetPropertyName(i);
                if (!sourceMaterial.HasProperty(propertyName) || !destinationMaterial.HasProperty(propertyName))
                {
                    continue;
                }

                Texture texture = sourceMaterial.GetTexture(propertyName);
                if (texture == null)
                {
                    continue;
                }

                Texture promotedTexture = PromoteTexture(texture);
                destinationMaterial.SetTexture(propertyName, promotedTexture);
                destinationMaterial.SetTextureScale(propertyName, sourceMaterial.GetTextureScale(propertyName));
                destinationMaterial.SetTextureOffset(propertyName, sourceMaterial.GetTextureOffset(propertyName));
            }

            PromoteSavedTextureEnvs(destinationMaterial);
        }

        private static void PromoteSavedTextureEnvs(Material material)
        {
            var serialized = new SerializedObject(material);
            SerializedProperty texEnvs = serialized.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texEnvs == null || !texEnvs.isArray)
            {
                return;
            }

            for (int i = 0; i < texEnvs.arraySize; i++)
            {
                SerializedProperty textureProperty = texEnvs
                    .GetArrayElementAtIndex(i)
                    .FindPropertyRelative("second.m_Texture");
                if (textureProperty?.objectReferenceValue is not Texture texture)
                {
                    continue;
                }

                textureProperty.objectReferenceValue = PromoteTexture(texture);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Texture PromoteTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith(ToonScapesRoot, StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string destination = PromoteTextureFile(sourceTexture, sourcePath);
            Texture promoted = AssetDatabase.LoadAssetAtPath<Texture>(destination);
            return promoted != null ? promoted : sourceTexture;
        }

        private static string PromoteTextureFile(Texture sourceTexture, string sourcePath)
        {
            if (PromotedAssetPaths.TryGetValue(sourcePath, out string existing))
            {
                return existing;
            }

            string destination = ToPromotedTexturePath(sourcePath);
            EnsureFolder(Path.GetDirectoryName(destination)?.Replace("\\", "/"));
            if (!AssetDatabase.AssetPathExists(destination))
            {
                SavePromotedTextureCopy(sourceTexture, sourcePath, destination);
                AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
                ConfigurePromotedTextureImporter(sourcePath, destination);
            }

            PromotedAssetPaths[sourcePath] = destination;
            return destination;
        }

        private static void SavePromotedTextureCopy(Texture sourceTexture, string sourcePath, string destination)
        {
            if (sourceTexture.width <= 0 || sourceTexture.height <= 0)
            {
                throw new InvalidOperationException($"Cannot promote ToonScapes texture with invalid dimensions: {sourcePath}.");
            }

            int maxDimension = Mathf.Max(sourceTexture.width, sourceTexture.height);
            float scale = Mathf.Min(1f, MaxPromotedTextureDimension / (float)maxDimension);
            int width = Mathf.Max(1, Mathf.RoundToInt(sourceTexture.width * scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(sourceTexture.height * scale));
            bool linear = IsLinearTexture(sourcePath);

            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32,
                linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);

            Texture2D readable = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false, linear);
            try
            {
                Graphics.Blit(sourceTexture, temporary);
                RenderTexture.active = temporary;
                readable.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                readable.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                File.WriteAllBytes(destination, readable.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
                UnityEngine.Object.DestroyImmediate(readable);
            }
        }

        private static void ConfigurePromotedTextureImporter(string sourcePath, string destination)
        {
            TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            TextureImporter destinationImporter = AssetImporter.GetAtPath(destination) as TextureImporter;
            if (destinationImporter == null)
            {
                return;
            }

            if (sourceImporter != null)
            {
                destinationImporter.textureType = sourceImporter.textureType;
                destinationImporter.textureShape = sourceImporter.textureShape;
                destinationImporter.alphaSource = sourceImporter.alphaSource;
                destinationImporter.sRGBTexture = sourceImporter.sRGBTexture;
                destinationImporter.mipmapEnabled = sourceImporter.mipmapEnabled;
                destinationImporter.wrapMode = sourceImporter.wrapMode;
                destinationImporter.filterMode = sourceImporter.filterMode;
                destinationImporter.anisoLevel = sourceImporter.anisoLevel;
            }

            destinationImporter.maxTextureSize = Mathf.Min(
                destinationImporter.maxTextureSize,
                MaxPromotedTextureDimension);
            destinationImporter.SaveAndReimport();
        }

        private static bool IsLinearTexture(string sourcePath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            return importer != null && !importer.sRGBTexture;
        }

        private static Shader PromoteShader(Shader sourceShader)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceShader);
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith(ToonScapesRoot, StringComparison.Ordinal))
            {
                return sourceShader;
            }

            string destination = PromoteAssetFile(sourcePath);
            Shader promoted = AssetDatabase.LoadAssetAtPath<Shader>(destination);
            return promoted != null ? promoted : sourceShader;
        }

        private static string PromoteAssetFile(string sourcePath)
        {
            if (PromotedAssetPaths.TryGetValue(sourcePath, out string existing))
            {
                return existing;
            }

            string destination = ToPromotedAssetPath(sourcePath);
            EnsureFolder(Path.GetDirectoryName(destination)?.Replace("\\", "/"));
            if (!AssetDatabase.AssetPathExists(destination) && !AssetDatabase.CopyAsset(sourcePath, destination))
            {
                throw new InvalidOperationException($"Failed to promote ToonScapes asset from {sourcePath} to {destination}.");
            }

            PromotedAssetPaths[sourcePath] = destination;
            return destination;
        }

        private static string ToPromotedAssetPath(string sourcePath)
        {
            string relative = sourcePath.Substring(ToonScapesRoot.Length + 1);
            return SourceAssetRoot + "/" + relative;
        }

        private static string ToPromotedTexturePath(string sourcePath)
        {
            string relative = sourcePath.Substring(ToonScapesRoot.Length + 1);
            string fileName = SanitizeAssetFileName(Path.ChangeExtension(relative, null));
            return TextureRoot + "/" + fileName + ".png";
        }

        private static string ToPromotedMeshPath(string sourcePath, string meshName)
        {
            string relative = sourcePath.Substring(ToonScapesRoot.Length + 1);
            string fileName = SanitizeAssetFileName(Path.ChangeExtension(relative, null) + "_" + meshName);
            return MeshRoot + "/" + fileName + ".asset";
        }

        private static string SanitizeAssetFileName(string value)
        {
            char[] characters = value.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                if (!char.IsLetterOrDigit(characters[i]) && characters[i] != '_' && characters[i] != '-')
                {
                    characters[i] = '_';
                }
            }

            return new string(characters);
        }

        private static void DeletePromotedModelImports()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { SourceAssetRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void DeletePromotedRawTextureImports()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { SourceAssetRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void RemoveColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void RemoveSourceScripts(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            for (int i = behaviours.Length - 1; i >= 0; i--)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour.GetType().Namespace?.StartsWith("ToonScapes", StringComparison.Ordinal) == true)
                {
                    UnityEngine.Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static void ValidateNoImportedDependencies(UnityEngine.Object target, string label)
        {
            UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(new[] { target });
            for (int i = 0; i < dependencies.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(dependencies[i]);
                if (path.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{label} still depends on raw imported asset {path}.");
                }
            }
        }

        private static void ValidateFolderAssetDependencies(string folderPath)
        {
            string[] assetGuids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
            for (int i = 0; i < assetGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    ValidateNoImportedDependencies(path);
                }
            }
        }

        private static void ValidateRendererAssetDependencies(GameObject root, string label)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    string materialPath = AssetDatabase.GetAssetPath(materials[j]);
                    if (materialPath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{label} renderer {renderers[i].name} still uses raw imported material {materialPath}.");
                    }
                }
            }

            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                string meshPath = AssetDatabase.GetAssetPath(meshFilters[i].sharedMesh);
                if (meshPath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{label} mesh filter {meshFilters[i].name} still uses raw imported mesh {meshPath}.");
                }
            }

            ParticleSystemRenderer[] particleRenderers = root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                Mesh[] meshes = new Mesh[4];
                int meshCount = particleRenderers[i].GetMeshes(meshes);
                for (int j = 0; j < meshCount; j++)
                {
                    string meshPath = AssetDatabase.GetAssetPath(meshes[j]);
                    if (meshPath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{label} particle renderer {particleRenderers[i].name} still uses raw imported mesh {meshPath}.");
                    }
                }
            }
        }

        private static void ValidateNoImportedDependencies(string assetPath)
        {
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, recursive: true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (dependencies[i].StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{assetPath} still depends on raw imported asset {dependencies[i]}.");
                }
            }
        }

        private static void RemoveRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = roots.Length - 1; i >= 0; i--)
            {
                if (string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(roots[i]);
                }
            }
        }

        private static GameObject RequireRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, name, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            throw new InvalidOperationException($"Missing root {name} in {scene.path}.");
        }

        private static T RequireComponentInScene<T>(Scene scene, string label) where T : Component
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

            throw new InvalidOperationException($"Missing {label} in {scene.path}.");
        }

        private static Transform RequireChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                throw new InvalidOperationException($"{parent.name} is missing child {name}.");
            }

            return child;
        }

        private static GameObject FindByName(GameObject[] roots, string name)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(includeInactive: true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j].name == name)
                    {
                        return transforms[j].gameObject;
                    }
                }
            }

            return null;
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string name = Path.GetFileName(folderPath);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException($"Invalid folder path {folderPath}.");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private readonly struct StageMaterials
        {
            public StageMaterials(
                Material ground,
                Material riftBlue,
                Material riftViolet,
                Material fireEmber,
                Material gateWall,
                Material boundaryWall,
                Material collisionPreview)
            {
                Ground = ground;
                RiftBlue = riftBlue;
                RiftViolet = riftViolet;
                FireEmber = fireEmber;
                GateWall = gateWall;
                BoundaryWall = boundaryWall;
                CollisionPreview = collisionPreview;
            }

            public Material Ground { get; }
            public Material RiftBlue { get; }
            public Material RiftViolet { get; }
            public Material FireEmber { get; }
            public Material GateWall { get; }
            public Material BoundaryWall { get; }
            public Material CollisionPreview { get; }
        }
    }
}
