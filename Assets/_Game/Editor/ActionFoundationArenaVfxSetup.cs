using System;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationArenaVfxSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";
        private const string SourceRoot = "Assets/_Imported/AssetStore/VFX/ShapesFX_Pack";
        private const string ArenaRootName = "ActionFoundation_ArenaVfx";
        private const string GridRootName = "ActionFoundation_ArenaGrid";
        private const string PromotedRoot = "Assets/_Game/Art/VFX/ShapesFXArena";
        private const string GeometryRoot = PromotedRoot + "/Geometry";
        private const string MaterialRoot = PromotedRoot + "/Materials";
        private const string ShapeMaterialRoot = MaterialRoot + "/ShapesFX";
        private const string ShaderRoot = PromotedRoot + "/Shaders";
        private const string TextureRoot = PromotedRoot + "/Textures";
        private const string ProfileRoot = PromotedRoot + "/Profiles";
        private const string PromotedShapeShaderPath = ShaderRoot + "/Shapes_Shader_ST.shader";
        private const string PostProcessProfilePath = ProfileRoot + "/DB_ActionFoundationArena_PostProcessProfile.asset";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Arena VFX")]
        public static void ReapplyArenaVfxMenu()
        {
            EnsurePromotedShapeAssets();
            ArenaMaterials materials = EnsureArenaMaterials();

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            ConfigureSceneAtmosphere(scene);

            GameObject root = FindRoot(scene, ArenaRootName);
            if (root == null)
            {
                root = new GameObject(ArenaRootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            ClearChildren(root.transform);
            ConfigurePostProcessVolume(root.transform);
            ConfigureLighting(root.transform);
            ConfigureGroundReadability(scene, materials.FloorSurface, materials.GridLine, materials.FloorGlow);

            Transform[] influenceTargets = CreateDynamicInfluenceTargets(root.transform);
            CreateReferenceTrainingSpace(root.transform, materials, influenceTargets);
            ClearGeneratedLightingData(scene);

            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied ActionFoundation arena VFX scene dressing with promoted ShapesFX materials.");
        }

        private static void EnsurePromotedShapeAssets()
        {
            EnsureFolder("Assets/_Game/Art/VFX");
            EnsureFolder(PromotedRoot);
            EnsureFolder(GeometryRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(ShapeMaterialRoot);
            EnsureFolder(ShaderRoot);
            EnsureFolder(TextureRoot);
            EnsureFolder(ProfileRoot);

            CopyAssetIfNeeded(SourceRoot + "/Shader/Shapes_Shader_ST.shader", PromotedShapeShaderPath);
            CopyAssetIfNeeded(SourceRoot + "/Geometries/Geo_Sphere_Hi.fbx", GeometryRoot + "/Geo_Sphere_Hi.fbx");
            CopyAssetIfNeeded(SourceRoot + "/Geometries/Geo_Sphere_low.fbx", GeometryRoot + "/Geo_Sphere_low.fbx");
            CopyAssetIfNeeded(SourceRoot + "/Geometries/Geo_Torus_Hex_Hi.fbx", GeometryRoot + "/Geo_Torus_Hex_Hi.fbx");
            CopyAssetIfNeeded(SourceRoot + "/Geometries/Geo_Icosahedron_Hex.fbx", GeometryRoot + "/Geo_Icosahedron_Hex.fbx");
            CopyAssetIfNeeded(SourceRoot + "/Geometries/Geo_Dodecahedron.fbx", GeometryRoot + "/Geo_Dodecahedron.fbx");
        }

        private static ArenaMaterials EnsureArenaMaterials()
        {
            return new ArenaMaterials
            {
                PortalOuter = PromoteShapeMaterialVariant(
                    "Materials/Materials_Dynamic/M_Hi_Torus_DynamicSample.mat",
                    "DB_ShapesFX_PortalOuter",
                    new Color(0.08f, 0.78f, 1.35f, 1f),
                    Color.white,
                    new Color(0.36f, 0.9f, 1.2f, 1f),
                    190f,
                    35f),
                PortalInner = PromoteShapeMaterialVariant(
                    "Materials/Materials_Shapes_HiRes/Torus/Hex/M_Hi_Torus_Hex_05.mat",
                    "DB_ShapesFX_PortalInner",
                    new Color(0.34f, 0.6f, 1.45f, 1f),
                    Color.white,
                    new Color(0.28f, 0.7f, 1f, 1f),
                    150f,
                    28f),
                BlueSphere = PromoteShapeMaterialVariant(
                    "Materials/Materials_Dynamic/M_Hi_Sphere_DynamicSample 1.mat",
                    "DB_ShapesFX_BlueSphere",
                    new Color(0.08f, 0.54f, 1.1f, 1f),
                    Color.white,
                    new Color(0.14f, 0.48f, 0.78f, 1f),
                    72f,
                    15f),
                CyanDodeca = PromoteShapeMaterialVariant(
                    "Materials/Materials_Shapes_HiRes/Dodecahedron/M_Hi_Dodecahedron_04.mat",
                    "DB_ShapesFX_CyanDodeca",
                    new Color(0.07f, 0.62f, 0.92f, 1f),
                    Color.white,
                    new Color(0.13f, 0.48f, 0.62f, 1f),
                    64f,
                    13f),
                VioletIcosa = PromoteShapeMaterialVariant(
                    "Materials/Materials_Shapes_HiRes/Icosahedron/Hex/M_Hi_Icosahedron_Hex_02.mat",
                    "DB_ShapesFX_VioletIcosa",
                    new Color(0.5f, 0.28f, 1.05f, 1f),
                    Color.white,
                    new Color(0.28f, 0.25f, 0.74f, 1f),
                    58f,
                    12f),
                DeepCube = PromoteShapeMaterialVariant(
                    "Materials/Materials_Dynamic/M_Hi_Cube_DynamicSample 2.mat",
                    "DB_ShapesFX_DeepCube",
                    new Color(0.14f, 0.5f, 0.95f, 1f),
                    Color.white,
                    new Color(0.18f, 0.45f, 0.8f, 1f),
                    90f,
                    20f),
                BackdropPanel = LoadOrCreateMaterial(
                    MaterialRoot + "/DB_ArenaVfx_BackdropPanel.mat",
                    new Color(0.115f, 0.13f, 0.145f, 1f),
                    new Color(0.008f, 0.025f, 0.035f, 1f),
                    0.04f),
                HazePanel = LoadOrCreateTransparentMaterial(
                    MaterialRoot + "/DB_ArenaVfx_HorizonHaze.mat",
                    new Color(0.44f, 0.51f, 0.56f, 0.18f),
                    new Color(0.03f, 0.07f, 0.09f, 1f),
                    additive: false),
                FloorSurface = LoadOrCreateMaterial(
                    MaterialRoot + "/DB_ArenaVfx_TestFloor.mat",
                    new Color(0.145f, 0.165f, 0.18f, 1f),
                    new Color(0.0f, 0.014f, 0.019f, 1f),
                    0.06f),
                GridLine = LoadOrCreateMaterial(
                    MaterialRoot + "/DB_ArenaVfx_GridLine.mat",
                    new Color(0.035f, 0.25f, 0.31f, 1f),
                    new Color(0.0f, 0.24f, 0.34f, 1f),
                    0.04f),
                FloorGlow = LoadOrCreateMaterial(
                    MaterialRoot + "/DB_ArenaVfx_FloorGlow.mat",
                    new Color(0.035f, 0.11f, 0.13f, 1f),
                    new Color(0.0f, 0.16f, 0.22f, 1f),
                    0.04f),
                StarPoint = LoadOrCreateTransparentMaterial(
                    MaterialRoot + "/DB_ArenaVfx_StarPoint.mat",
                    new Color(0.16f, 0.8f, 1f, 0.55f),
                    new Color(0.2f, 0.9f, 1.6f, 1f),
                    additive: true)
            };
        }

        private static void ConfigureLighting(Transform root)
        {
            Light portalLight = CreateLight(root, "ArenaVfx_DistantKeyLight", new Vector3(0f, 6.0f, 18.0f));
            portalLight.type = LightType.Point;
            portalLight.color = new Color(0.72f, 0.86f, 1f, 1f);
            portalLight.intensity = 1.35f;
            portalLight.range = 24f;
            portalLight.shadows = LightShadows.None;
            portalLight.bounceIntensity = 0f;

            Light lowFill = CreateLight(root, "ArenaVfx_LowBlueFill", new Vector3(0f, 1.6f, 3.8f));
            lowFill.type = LightType.Point;
            lowFill.color = new Color(0.22f, 0.48f, 0.68f, 1f);
            lowFill.intensity = 0.58f;
            lowFill.range = 14f;
            lowFill.shadows = LightShadows.None;
            lowFill.bounceIntensity = 0f;

            Light sideLight = CreateLight(root, "ArenaVfx_LeftFillLight", new Vector3(-7.5f, 3.2f, 5.2f));
            sideLight.type = LightType.Point;
            sideLight.color = new Color(0.24f, 0.42f, 0.58f, 1f);
            sideLight.intensity = 0.42f;
            sideLight.range = 11f;
            sideLight.shadows = LightShadows.None;
            sideLight.bounceIntensity = 0f;
        }

        private static void ClearGeneratedLightingData(Scene scene)
        {
            Lightmapping.SetLightingDataAssetForScene(scene, null);
            LightmapSettings.lightmaps = Array.Empty<LightmapData>();
            LightmapSettings.lightProbes = null;
        }

        private static void ConfigurePostProcessVolume(Transform root)
        {
            VolumeProfile profile = EnsurePostProcessProfile();
            GameObject volumeObject = CreateChild(root, "ArenaVfx_GlobalPostProcess", Vector3.zero, Quaternion.identity, Vector3.one);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 20f;
            volume.weight = 1f;
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(volume);
        }

        private static void ConfigureSceneAtmosphere(Scene scene)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.25f, 0.27f, 0.3f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.39f, 0.42f, 0.45f, 1f);
            RenderSettings.fogDensity = 0.013f;
            ConfigureAuthoredDirectionalLights(scene);

            GameObject cameraObject = FindByName(scene.GetRootGameObjects(), "Main Camera");
            if (cameraObject != null && cameraObject.TryGetComponent(out Camera camera))
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.4f, 0.43f, 0.46f, 1f);
                camera.allowHDR = true;
                camera.allowMSAA = true;
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = true;
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                EditorUtility.SetDirty(camera);
                EditorUtility.SetDirty(cameraData);
            }
        }

        private static void ConfigureAuthoredDirectionalLights(Scene scene)
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

                    light.color = new Color(0.78f, 0.86f, 1f, 1f);
                    light.intensity = 0.48f;
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = 0.18f;
                    light.bounceIntensity = 0f;
                    EditorUtility.SetDirty(light);
                }
            }
        }

        private static void ConfigureGroundReadability(Scene scene, Material floorMaterial, Material gridMaterial, Material floorGlowMaterial)
        {
            GameObject ground = FindByName(scene.GetRootGameObjects(), "ActionTest_Ground");
            if (ground != null && ground.TryGetComponent(out Renderer groundRenderer))
            {
                groundRenderer.sharedMaterial = floorMaterial;
                groundRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                groundRenderer.receiveShadows = true;
            }

            GameObject gridRoot = FindRoot(scene, GridRootName);
            if (gridRoot == null)
            {
                gridRoot = new GameObject(GridRootName);
                SceneManager.MoveGameObjectToScene(gridRoot, scene);
            }

            ClearChildren(gridRoot.transform);
            for (int i = -8; i <= 8; i++)
            {
                CreateLineCube(gridRoot.transform, $"GridLine_X_{i}", new Vector3(i * 2.5f, 0.012f, 7.5f), new Vector3(0.014f, 0.01f, 46f), gridMaterial);
                CreateLineCube(gridRoot.transform, $"GridLine_Z_{i}", new Vector3(0f, 0.014f, 1f + i * 2.5f), new Vector3(38f, 0.01f, 0.014f), gridMaterial);
            }

            CreateLineCube(gridRoot.transform, "FloorGlow_PlayerReadabilityField", new Vector3(0f, 0.018f, 5.8f), new Vector3(7.2f, 0.012f, 13.2f), floorGlowMaterial);
            GameObject scanBand = CreateLineCube(gridRoot.transform, "FloorGlow_SlowScanBand", new Vector3(0f, 0.021f, 6.2f), new Vector3(36f, 0.012f, 0.26f), floorGlowMaterial);
            ActionFoundationArenaTransformMotion scanMotion = scanBand.AddComponent<ActionFoundationArenaTransformMotion>();
            scanMotion.Configure(Vector3.zero, Vector3.forward, 8.4f, 0.055f, 0.4f);
            EditorUtility.SetDirty(scanMotion);
            EditorUtility.SetDirty(gridRoot);
        }

        private static Transform[] CreateDynamicInfluenceTargets(Transform root)
        {
            Transform anchorRoot = CreateChild(root, "ArenaVfx_DynamicInfluencers", new Vector3(0f, 0f, 6.8f), Quaternion.identity, Vector3.one).transform;
            Transform[] targets =
            {
                CreateMovingInfluence(anchorRoot, "Influence_Target_A", new Vector3(-2.7f, 2.2f, 0.8f), new Vector3(1.2f, 0.5f, 0.15f), 2.8f, 0.28f, 0.0f),
                CreateMovingInfluence(anchorRoot, "Influence_Target_B", new Vector3(2.6f, 3.0f, 1.6f), new Vector3(-0.55f, 0.8f, 0.25f), 2.4f, 0.34f, 1.1f),
                CreateMovingInfluence(anchorRoot, "Influence_Target_C", new Vector3(-1.1f, 4.1f, 3.8f), new Vector3(0.4f, -0.2f, 1f), 2.1f, 0.23f, 2.0f),
                CreateMovingInfluence(anchorRoot, "Influence_Target_D", new Vector3(1.4f, 1.5f, -1.7f), new Vector3(-0.8f, 0.4f, -0.35f), 2.6f, 0.31f, 2.8f)
            };
            return targets;
        }

        private static Transform CreateMovingInfluence(Transform parent, string name, Vector3 localPosition, Vector3 axis, float amplitude, float frequency, float phase)
        {
            GameObject target = CreateChild(parent, name, localPosition, Quaternion.identity, Vector3.one);
            ConfigureFloating(target, Vector3.zero, axis, amplitude, frequency, phase, Color.white, Color.black, 0f, 0f);
            return target.transform;
        }

        private static void CreateReferenceTrainingSpace(Transform root, ArenaMaterials materials, Transform[] influenceTargets)
        {
            CreateDistantFogAndStructures(root, materials);
            CreateFarShapeFxComposition(root, materials, influenceTargets);
            CreateFarDataSpecks(root, materials.StarPoint);
        }

        private static void CreateDistantFogAndStructures(Transform root, ArenaMaterials materials)
        {
            Transform roomRoot = CreateChild(root, "ArenaVfx_ReferenceTrainingRoom", Vector3.zero, Quaternion.identity, Vector3.one).transform;

            CreatePanelCube(roomRoot, "ArenaVfx_HorizonMistPanel", new Vector3(0f, 3.4f, 27f), Quaternion.identity, new Vector3(42f, 5.6f, 0.05f), materials.HazePanel);
            CreatePanelCube(roomRoot, "ArenaVfx_UpperSoftFog", new Vector3(0f, 8.2f, 24f), Quaternion.identity, new Vector3(40f, 0.04f, 19f), materials.HazePanel);

            CreatePanelCube(roomRoot, "ArenaVfx_BackStructure_CenterLow", new Vector3(0f, 1.55f, 25.8f), Quaternion.identity, new Vector3(12f, 2.2f, 0.12f), materials.BackdropPanel);
            CreatePanelCube(roomRoot, "ArenaVfx_BackStructure_LeftTall", new Vector3(-13.5f, 2.45f, 24.6f), Quaternion.Euler(0f, 7f, 0f), new Vector3(2.2f, 4.4f, 0.18f), materials.BackdropPanel);
            CreatePanelCube(roomRoot, "ArenaVfx_BackStructure_RightTall", new Vector3(13.8f, 2.3f, 24.8f), Quaternion.Euler(0f, -8f, 0f), new Vector3(2.0f, 4.1f, 0.18f), materials.BackdropPanel);
            CreatePanelCube(roomRoot, "ArenaVfx_BackStructure_LeftLow", new Vector3(-19.5f, 1.25f, 19.4f), Quaternion.Euler(0f, 15f, 0f), new Vector3(5.6f, 1.9f, 0.16f), materials.BackdropPanel);
            CreatePanelCube(roomRoot, "ArenaVfx_BackStructure_RightLow", new Vector3(19.0f, 1.35f, 20.2f), Quaternion.Euler(0f, -15f, 0f), new Vector3(5.0f, 2.0f, 0.16f), materials.BackdropPanel);

            CreatePanelCube(roomRoot, "ArenaVfx_LeftDistantPlane", new Vector3(-18.8f, 2.1f, 14.4f), Quaternion.Euler(0f, 17f, 0f), new Vector3(0.08f, 3.8f, 13.2f), materials.BackdropPanel);
            CreatePanelCube(roomRoot, "ArenaVfx_RightDistantPlane", new Vector3(18.8f, 2.1f, 14.4f), Quaternion.Euler(0f, -17f, 0f), new Vector3(0.08f, 3.8f, 13.2f), materials.BackdropPanel);
            CreatePanelCube(roomRoot, "ArenaVfx_LeftSoftFogPlane", new Vector3(-17.2f, 3.2f, 14.8f), Quaternion.Euler(0f, 17f, 0f), new Vector3(0.04f, 5.2f, 14.6f), materials.HazePanel);
            CreatePanelCube(roomRoot, "ArenaVfx_RightSoftFogPlane", new Vector3(17.2f, 3.2f, 14.8f), Quaternion.Euler(0f, -17f, 0f), new Vector3(0.04f, 5.2f, 14.6f), materials.HazePanel);
        }

        private static void CreateFarShapeFxComposition(Transform root, ArenaMaterials materials, Transform[] influenceTargets)
        {
            Mesh sphere = LoadMesh(GeometryRoot + "/Geo_Sphere_Hi.fbx");
            Mesh torus = LoadMesh(GeometryRoot + "/Geo_Torus_Hex_Hi.fbx");
            Mesh icosa = LoadMesh(GeometryRoot + "/Geo_Icosahedron_Hex.fbx");
            Mesh dodeca = LoadMesh(GeometryRoot + "/Geo_Dodecahedron.fbx");

            Transform skyRoot = CreateChild(root, "ArenaVfx_FarShapesFxComposition", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            CreateShapeObject(skyRoot, "ShapesFX_FarTrainingOrb_Core", sphere, materials.BlueSphere, new Vector3(0f, 6.9f, 25.6f), Quaternion.Euler(0f, 22f, 0f), new Vector3(2.45f, 2.45f, 2.45f), new Vector3(0f, 9f, 0f), 0.08f, 0.12f, 0.4f, influenceTargets);
            CreateShapeObject(skyRoot, "ShapesFX_FarTrainingRing_Core", torus, materials.PortalOuter, new Vector3(0f, 6.9f, 25.6f), Quaternion.Euler(72f, 0f, 0f), new Vector3(5.1f, 5.1f, 5.1f), new Vector3(0f, 0f, 7f), 0.05f, 0.1f, 1.0f, influenceTargets);
            CreateShapeObject(skyRoot, "ShapesFX_FarTrainingRing_Tilt", torus, materials.PortalInner, new Vector3(0f, 6.9f, 25.6f), Quaternion.Euler(68f, 0f, 42f), new Vector3(3.9f, 3.9f, 3.9f), new Vector3(0f, 0f, -10f), 0.04f, 0.12f, 2.2f, influenceTargets);

            CreateShapeObject(skyRoot, "ShapesFX_FarTrainingOrb_Left", sphere, materials.CyanDodeca, new Vector3(-11.8f, 5.6f, 22.2f), Quaternion.Euler(-12f, 28f, 16f), new Vector3(1.15f, 1.15f, 1.15f), new Vector3(3f, 11f, -4f), 0.18f, 0.13f, 1.3f, influenceTargets);
            CreateShapeObject(skyRoot, "ShapesFX_FarTrainingOrb_Right", sphere, materials.VioletIcosa, new Vector3(12.2f, 5.9f, 22.8f), Quaternion.Euler(12f, -32f, -14f), new Vector3(1.05f, 1.05f, 1.05f), new Vector3(-3f, -10f, 4f), 0.16f, 0.14f, 2.4f, influenceTargets);

            CreateShapeObject(skyRoot, "ShapesFX_FarShard_LeftHigh", icosa, materials.DeepCube, new Vector3(-15.2f, 7.4f, 20.8f), Quaternion.Euler(18f, 38f, 11f), new Vector3(0.7f, 0.7f, 0.7f), new Vector3(8f, 6f, -3f), 0.14f, 0.11f, 0.8f, influenceTargets);
            CreateShapeObject(skyRoot, "ShapesFX_FarShard_RightHigh", dodeca, materials.DeepCube, new Vector3(15.6f, 7.0f, 20.6f), Quaternion.Euler(-16f, -34f, 9f), new Vector3(0.62f, 0.62f, 0.62f), new Vector3(-7f, 5f, 4f), 0.12f, 0.12f, 1.7f, influenceTargets);
        }

        private static void CreateFarDataSpecks(Transform root, Material starMaterial)
        {
            Mesh sphere = LoadMesh(GeometryRoot + "/Geo_Sphere_low.fbx");
            Transform speckRoot = CreateChild(root, "ArenaVfx_FarDataSpecks", Vector3.zero, Quaternion.identity, Vector3.one).transform;

            for (int i = 0; i < 16; i++)
            {
                float t = i / 16f;
                float angle = t * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * (10.5f + (i % 4) * 1.6f);
                float y = 3.4f + (i % 5) * 0.75f;
                float z = 18.5f + Mathf.Sin(angle) * 4.5f + (i % 3) * 1.1f;
                GameObject speck = CreateMeshObject(
                    speckRoot,
                    $"ArenaVfx_FarDataSpeck_{i + 1:00}",
                    sphere,
                    starMaterial,
                    new Vector3(x, y, z),
                    Quaternion.identity,
                    Vector3.one * (0.035f + (i % 3) * 0.012f));
                ConfigureFloating(speck, new Vector3(0f, 18f + i, 0f), Vector3.up, 0.08f + (i % 2) * 0.04f, 0.14f + i * 0.006f, i * 0.37f, new Color(0.17f, 0.72f, 0.95f, 0.45f), new Color(0.12f, 0.5f, 0.8f, 1f), 0.24f, 0.35f);
            }
        }

        private static GameObject CreatePanelCube(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            GameObject panel = CreateLineCube(parent, name, localPosition, localScale, material);
            panel.transform.localRotation = localRotation;
            return panel;
        }

        private static GameObject CreateShapeObject(
            Transform parent,
            string name,
            Mesh mesh,
            Material material,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Vector3 rotationSpeed,
            float bobAmplitude,
            float bobFrequency,
            float phase,
            Transform[] influenceTargets)
        {
            GameObject shape = CreateMeshObject(parent, name, mesh, material, localPosition, localRotation, localScale);
            ConfigureFloating(shape, rotationSpeed, Vector3.up, bobAmplitude, bobFrequency, phase, Color.white, Color.black, 0f, 0f);
            ConfigureInfluence(shape, influenceTargets);
            return shape;
        }

        private static void ConfigureFloating(
            GameObject target,
            Vector3 rotationSpeed,
            Vector3 bobAxis,
            float bobAmplitude,
            float bobFrequency,
            float phase,
            Color baseColor,
            Color emission,
            float pulseAmount,
            float pulseFrequency)
        {
            ActionFoundationArenaFloatingShape floating = target.GetComponent<ActionFoundationArenaFloatingShape>();
            if (floating == null)
            {
                floating = target.AddComponent<ActionFoundationArenaFloatingShape>();
            }

            floating.Configure(rotationSpeed, bobAxis, bobAmplitude, bobFrequency, phase, baseColor, emission, pulseAmount, pulseFrequency);
            EditorUtility.SetDirty(floating);
        }

        private static void ConfigureInfluence(GameObject target, Transform[] influenceTargets)
        {
            ActionFoundationArenaShapeInfluenceDriver driver = target.GetComponent<ActionFoundationArenaShapeInfluenceDriver>();
            if (driver == null)
            {
                driver = target.AddComponent<ActionFoundationArenaShapeInfluenceDriver>();
            }

            driver.Configure(target.GetComponentsInChildren<Renderer>(includeInactive: true), influenceTargets);
            EditorUtility.SetDirty(driver);
        }

        private static GameObject CreateMeshObject(
            Transform parent,
            string name,
            Mesh mesh,
            Material material,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(parent, worldPositionStays: false);
            target.transform.localPosition = localPosition;
            target.transform.localRotation = localRotation;
            target.transform.localScale = localScale;
            MeshFilter meshFilter = target.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            MeshRenderer renderer = target.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return target;
        }

        private static GameObject CreateLineCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = name;
            line.transform.SetParent(parent, worldPositionStays: false);
            line.transform.localPosition = localPosition;
            line.transform.localRotation = Quaternion.identity;
            line.transform.localScale = localScale;
            UnityEngine.Object.DestroyImmediate(line.GetComponent<Collider>());
            MeshRenderer renderer = line.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return line;
        }

        private static Light CreateLight(Transform parent, string name, Vector3 localPosition)
        {
            GameObject target = new GameObject(name);
            target.transform.SetParent(parent, worldPositionStays: false);
            target.transform.localPosition = localPosition;
            return target.AddComponent<Light>();
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

        private static Material PromoteShapeMaterialVariant(
            string sourceRelativePath,
            string materialName,
            Color outlineColor,
            Color frontFaceColor,
            Color backFaceColor,
            float outlineOpacity,
            float colorBoost)
        {
            string sourcePath = SourceRoot + "/" + sourceRelativePath;
            Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
            if (sourceMaterial == null)
            {
                throw new InvalidOperationException($"Missing source ShapesFX material at {sourcePath}");
            }

            Shader shapeShader = AssetDatabase.LoadAssetAtPath<Shader>(PromotedShapeShaderPath);
            if (shapeShader == null)
            {
                throw new InvalidOperationException($"Missing promoted ShapesFX shader at {PromotedShapeShaderPath}");
            }

            string destination = ShapeMaterialRoot + "/" + materialName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(destination);
            if (material == null)
            {
                material = new Material(shapeShader);
                AssetDatabase.CreateAsset(material, destination);
            }

            material.CopyPropertiesFromMaterial(sourceMaterial);
            material.shader = shapeShader;
            material.shaderKeywords = sourceMaterial.shaderKeywords;
            SetColorIfPresent(material, "_Outline_Color", outlineColor);
            SetColorIfPresent(material, "_FrontFace_Color", frontFaceColor);
            SetColorIfPresent(material, "_BackFace_Color", backFaceColor);
            SetFloatIfPresent(material, "_Outline_Opacity", outlineOpacity);
            SetFloatIfPresent(material, "_DefaultOutlineOpacity", Mathf.Max(0.45f, outlineOpacity * 0.005f));
            SetFloatIfPresent(material, "_ColorBoost", colorBoost);
            SetFloatIfPresent(material, "_TargetMode", 1f);
            material.EnableKeyword("_TARGETMODE_ON");
            PromoteAndAssignTextureProperties(sourceMaterial, material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void PromoteAndAssignTextureProperties(Material sourceMaterial, Material destinationMaterial)
        {
            Shader sourceShader = sourceMaterial.shader;
            int propertyCount = sourceShader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                if (sourceShader.GetPropertyType(i) != ShaderPropertyType.Texture)
                {
                    continue;
                }

                string propertyName = sourceShader.GetPropertyName(i);
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
        }

        private static Texture PromoteTexture(Texture sourceTexture)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrWhiteSpace(sourcePath) || !sourcePath.StartsWith(SourceRoot, StringComparison.Ordinal))
            {
                return sourceTexture;
            }

            string relativePath = sourcePath.Substring(SourceRoot.Length + 1);
            string destination = TextureRoot + "/" + relativePath;
            EnsureFolder(System.IO.Path.GetDirectoryName(destination)?.Replace("\\", "/"));
            CopyAssetIfNeeded(sourcePath, destination);
            Texture promotedTexture = AssetDatabase.LoadAssetAtPath<Texture>(destination);
            return promotedTexture != null ? promotedTexture : sourceTexture;
        }

        private static VolumeProfile EnsurePostProcessProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PostProcessProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, PostProcessProfilePath);
            }

            Bloom bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.active = true;
            SetParameter(bloom.threshold, 0.78f);
            SetParameter(bloom.intensity, 0.46f);
            SetParameter(bloom.scatter, 0.42f);
            SetParameter(bloom.clamp, 3.2f);
            SetParameter(bloom.tint, new Color(0.78f, 0.9f, 1f, 1f));

            Tonemapping tonemapping = GetOrAddVolumeComponent<Tonemapping>(profile);
            tonemapping.active = true;
            SetParameter(tonemapping.mode, TonemappingMode.Neutral);

            ColorAdjustments colorAdjustments = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            colorAdjustments.active = true;
            SetParameter(colorAdjustments.postExposure, 0.08f);
            SetParameter(colorAdjustments.contrast, 4f);
            SetParameter(colorAdjustments.saturation, 5f);
            SetParameter(colorAdjustments.colorFilter, new Color(0.94f, 0.98f, 1f, 1f));

            Vignette vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.active = true;
            SetParameter(vignette.color, new Color(0.06f, 0.07f, 0.08f, 1f));
            SetParameter(vignette.intensity, 0.07f);
            SetParameter(vignette.smoothness, 0.46f);
            SetParameter(vignette.rounded, false);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T GetOrAddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            RemoveNullVolumeComponents(profile);
            if (!profile.TryGet(out T component))
            {
                component = profile.Add<T>(overrides: true);
            }

            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(component)))
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            EditorUtility.SetDirty(component);
            return component;
        }

        private static void RemoveNullVolumeComponents(VolumeProfile profile)
        {
            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                if (profile.components[i] == null)
                {
                    profile.components.RemoveAt(i);
                }
            }
        }

        private static void SetParameter<T>(VolumeParameter<T> parameter, T value)
        {
            parameter.overrideState = true;
            parameter.value = value;
        }

        private static Material LoadOrCreateMaterial(string path, Color baseColor, Color emissionColor, float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = System.IO.Path.GetFileNameWithoutExtension(path);
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_Color", baseColor);
            material.SetColor("_EmissionColor", emissionColor);
            material.SetFloat("_Smoothness", smoothness);
            SetFloatIfPresent(material, "_Metallic", 0f);
            SetFloatIfPresent(material, "_SpecularHighlights", 0f);
            SetFloatIfPresent(material, "_EnvironmentReflections", 0f);
            SetColorIfPresent(material, "_SpecColor", new Color(0.025f, 0.03f, 0.035f, 1f));
            material.EnableKeyword("_EMISSION");
            ConfigureOpaqueMaterial(material);
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
            material.DisableKeyword("_BLENDMODE_ADD");
        }

        private static Material LoadOrCreateTransparentMaterial(string path, Color baseColor, Color emissionColor, bool additive)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = System.IO.Path.GetFileNameWithoutExtension(path);
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_Color", baseColor);
            material.SetColor("_EmissionColor", emissionColor);
            ConfigureTransparentMaterial(material, additive);
            EditorUtility.SetDirty(material);
            return material;
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

        private static void SetColorIfPresent(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static Mesh LoadMesh(string path)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Mesh mesh)
                {
                    return mesh;
                }
            }

            throw new InvalidOperationException($"Missing promoted arena mesh at {path}");
        }

        private static void CopyAssetIfNeeded(string source, string destination)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destination) != null)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(source) == null)
            {
                throw new InvalidOperationException($"Missing source Shapes FX asset at {source}");
            }

            EnsureFolder(System.IO.Path.GetDirectoryName(destination)?.Replace("\\", "/"));
            if (!AssetDatabase.CopyAsset(source, destination))
            {
                throw new InvalidOperationException($"Failed to promote Shapes FX asset from {source} to {destination}");
            }
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException($"Invalid folder path {path}");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == name)
                {
                    return roots[i];
                }
            }

            return null;
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

        private static void ClearChildren(Transform target)
        {
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(target.GetChild(i).gameObject);
            }
        }

        private sealed class ArenaMaterials
        {
            public Material PortalOuter;
            public Material PortalInner;
            public Material BlueSphere;
            public Material CyanDodeca;
            public Material VioletIcosa;
            public Material DeepCube;
            public Material BackdropPanel;
            public Material HazePanel;
            public Material FloorSurface;
            public Material GridLine;
            public Material FloorGlow;
            public Material StarPoint;
        }
    }
}
