using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationOlympusCorridorLookdevSetup
    {
        private const string SourceScenePath = "Assets/_Game/Art/Environment/OlympusTemple/HDRP/Scene/L_Olympus.unity";
        private const string SceneRoot = "Assets/_Game/Scenes/Lookdev";
        private const string TargetScenePath = SceneRoot + "/OlympusCorridorLookdev.unity";
        private const string DenseTargetScenePath = SceneRoot + "/OlympusCorridorDenseLookdev.unity";
        private const string InvasionTargetScenePath = SceneRoot + "/OlympusCorridorInvasionLookdev.unity";
        private const string StageSceneRoot = "Assets/_Game/Scenes";
        private const string StageTargetScenePath = StageSceneRoot + "/OlympusCorridorInvasionStage.unity";
        private const string ArtRoot = "Assets/_Game/Art/Environment/OlympusCorridor";
        private const string ProfileRoot = ArtRoot + "/Profiles";
        private const string MaterialRoot = ArtRoot + "/Materials";
        private const string TextureRoot = ArtRoot + "/Textures";
        private const string SkyTextureRoot = TextureRoot + "/Sky";
        private const string DecalTextureRoot = TextureRoot + "/Decals";
        private const string ShaderRoot = ArtRoot + "/Shaders";
        private const string DisallowedToonEnvironmentMaterialRoot = MaterialRoot + "/Toonized";
        private const string PgrPreserveMaterialRoot = MaterialRoot + "/PgrPreserve";
        private const string PostProcessProfilePath = ProfileRoot + "/DB_OlympusCorridor_PostProcess.asset";

        private const string BlueGlowMaterialPath = MaterialRoot + "/DB_OlympusCorridor_BlueGlow.mat";
        private const string GoldGlowMaterialPath = MaterialRoot + "/DB_OlympusCorridor_GoldGlow.mat";
        private const string WhiteGlowMaterialPath = MaterialRoot + "/DB_OlympusCorridor_WhiteGlow.mat";
        private const string ScorchedStoneMaterialPath = MaterialRoot + "/DB_OlympusCorridor_ScorchedStone.mat";
        private const string DamagedMarbleMaterialPath = MaterialRoot + "/DB_OlympusCorridor_DamagedMarble.mat";
        private const string InvasionCrackDecalMaterialPath = MaterialRoot + "/DB_OlympusCorridor_InvasionCrackDecal.mat";
        private const string InvasionCrackOverlayMaterialPath = MaterialRoot + "/DB_OlympusCorridor_InvasionCrackOverlay.mat";
        private const string SkyboxMaterialPath = MaterialRoot + "/DB_OlympusCorridor_HeavenlySkybox.mat";

        private const string AllSkyRoot = "Assets/_Imported/AssetStore/Sky/Allsky";
        private const string PromotedUniFireSmokePrefabRoot = "Assets/_Game/Art/VFX/UNI VFX/Realistic Explosions, Fire & Smoke/Prefabs";
        private const string UniGroundFirePrefabPath = PromotedUniFireSmokePrefabRoot + "/UNI_Ground_Fire.prefab";
        private const string UniSmallFirePrefabPath = PromotedUniFireSmokePrefabRoot + "/UNI_Small_Fire.prefab";
        private const string UniDeviceFirePrefabPath = PromotedUniFireSmokePrefabRoot + "/UNI_Device_Fire.prefab";
        private const string UniGasFirePrefabPath = PromotedUniFireSmokePrefabRoot + "/UNI_Gas_Fire.prefab";
        private const string UniLongSmokePrefabPath = PromotedUniFireSmokePrefabRoot + "/UNI_Long_Smoke.prefab";
        private const string AllSkySourceSkyboxPath = AllSkyRoot + "/Cartoon/Cartoon Base BlueSky/Day_BlueSky_Nothing.mat";
        private const string InvasionCrackDecalTexturePath = DecalTextureRoot + "/DB_OlympusCorridor_InvasionCrackDecal.png";
        private const string MobileRendererDataPath = "Assets/Settings/Mobile_Renderer.asset";
        private const string PcRendererDataPath = "Assets/Settings/PC_Renderer.asset";
        private const string ToonOutlineMaterialPath = MaterialRoot + "/DB_OlympusCorridor_ToonOutline.mat";
        private const string BillboardBlueMaterialPath = MaterialRoot + "/DB_OlympusCorridor_BillboardBlue.mat";
        private const string BillboardWhiteMaterialPath = MaterialRoot + "/DB_OlympusCorridor_BillboardWhite.mat";
        private const string BillboardGoldMaterialPath = MaterialRoot + "/DB_OlympusCorridor_BillboardGold.mat";
        private const string BillboardDarkMaterialPath = MaterialRoot + "/DB_OlympusCorridor_BillboardDark.mat";
        private const string BillboardBlueCoreMaterialPath = MaterialRoot + "/DB_OlympusCorridor_BillboardBlueCore.mat";
        private const string BillboardFloorMaterialPath = MaterialRoot + "/DB_OlympusCorridor_BillboardFloor.mat";
        private const string PromotedSkyTexturePrefix = "DB_OlympusCorridor_Sky_";
        private const string ShapesFxRoot = "Assets/_Game/Art/VFX/ShapesFXArena";
        private const string ShapesFxGeometryRoot = ShapesFxRoot + "/Geometry";
        private const string ShapesFxMaterialRoot = ShapesFxRoot + "/Materials/ShapesFX";
        private const string LookdevRootName = "OlympusCorridorLookdev_RuntimeFreePass";
        private const string DenseLookdevRootName = "OlympusCorridorDenseLookdev_RuntimeFreePass";
        private const string InvasionLookdevRootName = "OlympusCorridorInvasionLookdev_RuntimeFreePass";
        private const string ImportedSkyFogVolumeName = "Sky and Fog Global Volume";
        private const string ImportedLightingRootName = "Lights";
        private const string VolumeName = "OlympusCorridor_GlobalPostProcess";
        private const string CameraName = "OlympusCorridor_LookdevCamera";
        private const string LightingName = "OlympusCorridor_LookdevLighting";
        private const string SanctuaryVisualsName = "OlympusCorridor_BlueRiftSanctuary";
        private const string InvasionFireBillboardsRootName = "InvasionFireBillboards";
        private const string InvasionSmokeBillboardsRootName = "InvasionSmokeBillboards";
        private const string PromotedUniFireSmokeVfxRootName = "PromotedUniFireSmokeVfx";
        private const string CombatAnchorsName = "OlympusCorridor_CombatReadAnchors";
        private const string StageRootName = "OlympusCorridorStageRoot";
        private const string StageMapRootName = "OlympusCorridorStageMap";
        private const string StageAnchorsName = "OlympusCorridorStageAnchors";
        private const string StageCombatAnchorsName = "CombatSpawnAnchors";
        private const string StageCutsceneAnchorsName = "CutsceneHandoffAnchors";
        private const string StageRuntimeAnchorsName = "RuntimeStateAnchors";
        private const string StagePreviewCameraName = "OlympusCorridorStage_PreviewCamera";
        private const string PreviewFileName = "olympus-corridor-lookdev-preview.png";
        private const string ClosePreviewFileName = "olympus-corridor-lookdev-close-preview.png";
        private const string HighPreviewFileName = "olympus-corridor-lookdev-high-preview.png";
        private const string DensePreviewFileName = "olympus-corridor-dense-lookdev-preview.png";
        private const string InvasionPreviewFileName = "olympus-corridor-invasion-lookdev-preview.png";
        private const string StagePreviewFileName = "olympus-corridor-invasion-stage-preview.png";
        private const string InvasionPlayPreviewFileName = "olympus-corridor-invasion-play-preview.png";
        private const string InvasionPlayPreviewPendingKey = "DimensionBrawl.OlympusCorridor.InvasionPlayPreviewPending";
        private const float InvasionPlayPreviewWarmupSeconds = 3.0f;
        private const float StageMapScale = 1.5f;
        private const bool EnableInvasionFireVisuals = true;
        private static double invasionPlayPreviewCaptureStartTime;
        [MenuItem("DimensionBrawl/Reapply Olympus Corridor Lookdev")]
        public static void ReapplyOlympusCorridorLookdevMenu()
        {
            ReapplyOlympusCorridorLookdev();
            Debug.Log("Reapplied Olympus corridor lookdev scene and profile.");
        }

        [MenuItem("DimensionBrawl/Validate Olympus Corridor Lookdev")]
        public static void ValidateOlympusCorridorLookdevMenu()
        {
            ValidateOlympusCorridorLookdev();
            Debug.Log("Olympus corridor lookdev validation passed.");
        }

        [MenuItem("DimensionBrawl/Render Olympus Corridor Lookdev Preview")]
        public static void RenderOlympusCorridorLookdevPreviewMenu()
        {
            string previewPath = RenderOlympusCorridorLookdevPreview();
            Debug.Log($"Rendered Olympus corridor lookdev preview: {previewPath}");
        }

        private static void EnsureLookdevSceneExists(string targetScenePath)
        {
            EnsureFolder("Assets/_Game/Scenes");
            EnsureFolder(SceneRoot);
            EnsureFolder("Assets/_Game/Art");
            EnsureFolder("Assets/_Game/Art/Environment");
            EnsureFolder(ArtRoot);
            EnsureFolder(ProfileRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(TextureRoot);
            EnsureFolder(SkyTextureRoot);
            EnsureFolder(DecalTextureRoot);
            EnsureFolder(PgrPreserveMaterialRoot);
            EnsureFolder(ShaderRoot);

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath))
            {
                throw new InvalidOperationException($"Missing promoted Olympus source scene: {SourceScenePath}");
            }

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScenePath))
            {
                if (!AssetDatabase.CopyAsset(SourceScenePath, targetScenePath))
                {
                    throw new InvalidOperationException($"Failed to copy Olympus source scene to {targetScenePath}.");
                }
            }
        }
        public static void ReapplyOlympusCorridorLookdev()
        {
            EnsureLookdevSceneExists(TargetScenePath);


            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            ConfigureOpenCombatSightline(scene);
            ConfigureImportedLighting(scene);
            RestoreSourceEnvironmentMaterials(scene);
            ConfigurePgrPreserveEnvironmentMaterials(scene);
            ConfigureSceneAtmosphere(scene);
            ConfigureImportedSkyFogVolumes(scene);
            ConfigureLookdevRoot(scene);
            ClearGeneratedLightingData(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        public static void ValidateOlympusCorridorLookdev()
        {
            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Scene is invalid: {TargetScenePath}");
            }

            GameObject lookdevRoot = RequireRoot(scene, LookdevRootName);
            RequireChild(lookdevRoot.transform, VolumeName);
            RequireChild(lookdevRoot.transform, CameraName);
            RequireChild(lookdevRoot.transform, LightingName);
            RequireChild(lookdevRoot.transform, SanctuaryVisualsName);
            RequireChild(lookdevRoot.transform, CombatAnchorsName);

            Volume volume = RequireComponent<Volume>(RequireChild(lookdevRoot.transform, VolumeName).gameObject);
            if (!volume.isGlobal || volume.sharedProfile == null)
            {
                throw new InvalidOperationException("Olympus corridor lookdev requires a global Volume with the shared corridor profile.");
            }

            string profilePath = AssetDatabase.GetAssetPath(volume.sharedProfile);
            if (!string.Equals(profilePath, PostProcessProfilePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Olympus corridor Volume should use {PostProcessProfilePath}, not {profilePath}.");
            }

            Camera camera = RequireComponent<Camera>(RequireChild(lookdevRoot.transform, CameraName).gameObject);
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            if (!camera.allowHDR || !cameraData.renderPostProcessing)
            {
                throw new InvalidOperationException("Olympus corridor lookdev camera should render HDR post processing.");
            }

            if (Math.Abs(camera.fieldOfView - 62f) > 0.01f)
            {
                throw new InvalidOperationException("Olympus corridor lookdev camera should keep the reference-backed 62 degree combat FOV.");
            }

            if (RenderSettings.skybox == null || !string.Equals(AssetDatabase.GetAssetPath(RenderSettings.skybox), SkyboxMaterialPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("RenderSettings.skybox should use the promoted Olympus AllSky material.");
            }

            VolumeProfile profile = RequireAsset<VolumeProfile>(PostProcessProfilePath);
            ValidateVolumeProfile(profile);
            RequireVolumeComponent<Bloom>(profile);
            RequireVolumeComponent<ColorAdjustments>(profile);
            RequireVolumeComponent<Tonemapping>(profile);
            RequireVolumeComponent<WhiteBalance>(profile);
            RequireVolumeComponent<Vignette>(profile);
            RequireVolumeComponent<DepthOfField>(profile);

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath))
            {
                throw new InvalidOperationException($"Promoted Olympus source scene should remain available: {SourceScenePath}");
            }

            if (string.Equals(SourceScenePath, TargetScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Olympus corridor lookdev must not edit the imported source scene in place.");
            }

            GameObject[] roots = scene.GetRootGameObjects();
            if (roots.Length < 4)
            {
                throw new InvalidOperationException("Olympus corridor lookdev scene should preserve the imported demo roots plus the generated lookdev root.");
            }

            int rendererCount = CountEnabledRenderersInScene(roots);
            if (rendererCount < 80)
            {
                throw new InvalidOperationException("Olympus corridor lookdev scene should preserve enough source renderers for corridor composition review.");
            }
            int toonizedMaterialSlots = CountMaterialSlotsWithPath(roots, DisallowedToonEnvironmentMaterialRoot + "/");
            if (toonizedMaterialSlots > 0)
            {
                throw new InvalidOperationException($"Olympus corridor lookdev should avoid BA-style toonized environment materials. Found {toonizedMaterialSlots} stale toonized material slots.");
            }

            int pgrPreserveMaterialSlots = CountMaterialSlotsWithPath(roots, PgrPreserveMaterialRoot + "/");
            if (pgrPreserveMaterialSlots < 80)
            {
                throw new InvalidOperationException($"Olympus corridor lookdev should use PGR-preserve material variants copied from the source Olympus materials. Found {pgrPreserveMaterialSlots} PGR-preserve material slots.");
            }

            int sourceMaterialSlots = CountMaterialSlotsWithPath(roots, GetSourceMaterialRoot() + "/");
            if (sourceMaterialSlots + pgrPreserveMaterialSlots < 80)
            {
                throw new InvalidOperationException($"Olympus corridor lookdev should keep source-backed environment material coverage. Found {sourceMaterialSlots} source and {pgrPreserveMaterialSlots} PGR-preserve material slots.");
            }

            if (RequireChild(lookdevRoot.transform, LightingName).GetComponentsInChildren<Light>(includeInactive: true).Length < 5)
            {
                throw new InvalidOperationException("Olympus corridor lookdev lighting should include corridor, rift, and gold accent anchors.");
            }

            Transform sanctuaryRoot = RequireChild(lookdevRoot.transform, SanctuaryVisualsName);
            if (sanctuaryRoot.GetComponentsInChildren<LineRenderer>(includeInactive: true).Length < 28)
            {
                throw new InvalidOperationException("Olympus corridor sanctuary visuals should include rift, portal, backdrop, and floor line renderers.");
            }

            if (CountNamedRenderers(sanctuaryRoot, "Backdrop_") < 8)
            {
                throw new InvalidOperationException("Olympus corridor sanctuary visuals should include far celestial backdrop silhouettes.");
            }

            if (CountNamedRenderers(sanctuaryRoot, "SkyConstellation_") < 2)
            {
                throw new InvalidOperationException("Olympus corridor sanctuary visuals should include sky constellation depth lines.");
            }

            if (CountNamedRenderers(sanctuaryRoot, "Billboard_") < 6)
            {
                throw new InvalidOperationException("Olympus corridor sanctuary visuals should include camera-facing toon billboard reads.");
            }

            if (CountNamedRenderers(sanctuaryRoot, "ShapesFX_") < 7)
            {
                throw new InvalidOperationException("Olympus corridor sanctuary visuals should include promoted ShapesFX ring and shard meshes.");
            }

            if (RequireChild(lookdevRoot.transform, CombatAnchorsName).childCount < 5)
            {
                throw new InvalidOperationException("Olympus corridor combat anchors should preserve player, boss, and add placement handles.");
            }
        }


        public static void RebuildOlympusCorridorWowStageOnly()
        {
            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            ConfigureOpenCombatSightline(scene);
            ConfigureImportedLighting(scene);
            RestoreSourceEnvironmentMaterials(scene);
            ConfigurePgrPreserveEnvironmentMaterials(scene);
            ConfigureSceneAtmosphere(scene);
            ConfigureImportedSkyFogVolumes(scene);
            ConfigureLookdevRoot(scene);
            ClearGeneratedLightingData(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        [MenuItem("DimensionBrawl/Rebuild Olympus Corridor Dense Lookdev")]
        public static void RebuildOlympusCorridorDenseLookdevMenu()
        {
            RebuildOlympusCorridorDenseLookdev();
            Debug.Log("Rebuilt Olympus corridor dense lookdev scene.");
        }

        public static void RebuildOlympusCorridorDenseLookdev()
        {
            EnsureLookdevSceneExists(DenseTargetScenePath);
            Scene scene = EditorSceneManager.OpenScene(DenseTargetScenePath, OpenSceneMode.Single);
            RestoreFullDemoRendererDensity(scene);
            ConfigureImportedLighting(scene);
            RestoreSourceEnvironmentMaterials(scene);
            ConfigurePgrPreserveEnvironmentMaterials(scene);
            ConfigureSceneAtmosphere(scene);
            ConfigureImportedSkyFogVolumes(scene);
            ConfigureDenseLookdevRoot(scene);
            ClearGeneratedLightingData(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("DimensionBrawl/Rebuild Olympus Corridor Invasion Lookdev")]
        public static void RebuildOlympusCorridorInvasionLookdevMenu()
        {
            RebuildOlympusCorridorInvasionLookdev();
            Debug.Log("Rebuilt Olympus corridor invasion lookdev scene.");
        }

        public static void RebuildOlympusCorridorInvasionLookdev()
        {
            EnsureOlympusCorridorDecalRendererFeature();
            EnsureLookdevSceneExists(InvasionTargetScenePath);
            Scene scene = EditorSceneManager.OpenScene(InvasionTargetScenePath, OpenSceneMode.Single);
            RestoreFullDemoRendererDensity(scene);
            ConfigureImportedLighting(scene);
            RestoreSourceEnvironmentMaterials(scene);
            ConfigurePgrPreserveEnvironmentMaterials(scene);
            ConfigureSceneAtmosphere(scene);
            ConfigureImportedSkyFogVolumes(scene);
            ConfigureInvasionLookdevRoot(scene);
            ClearGeneratedLightingData(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("DimensionBrawl/Rebuild Olympus Corridor Invasion Stage")]
        public static void RebuildOlympusCorridorInvasionStageMenu()
        {
            RebuildOlympusCorridorInvasionStage();
            Debug.Log("Rebuilt Olympus corridor invasion stage scene.");
        }

        public static void RebuildOlympusCorridorInvasionStage()
        {
            EnsureFolder(StageSceneRoot);
            EnsureLookdevSceneExists(InvasionTargetScenePath);

            Scene stageScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Scene sourceScene = default;
            bool sourceSceneOpened = false;

            try
            {
                sourceScene = EditorSceneManager.OpenScene(InvasionTargetScenePath, OpenSceneMode.Additive);
                sourceSceneOpened = true;
                RequireRoot(sourceScene, InvasionLookdevRootName);

                SceneManager.SetActiveScene(stageScene);

                GameObject stageRoot = new GameObject(StageRootName);
                SceneManager.MoveGameObjectToScene(stageRoot, stageScene);

                GameObject stageMapRoot = CreateChild(
                    stageRoot.transform,
                    StageMapRootName,
                    Vector3.zero,
                    Quaternion.identity,
                    Vector3.one * StageMapScale);

                foreach (GameObject sourceRoot in sourceScene.GetRootGameObjects())
                {
                    GameObject copy = UnityEngine.Object.Instantiate(sourceRoot);
                    copy.name = sourceRoot.name;
                    SceneManager.MoveGameObjectToScene(copy, stageScene);
                    copy.transform.SetParent(stageMapRoot.transform, worldPositionStays: false);
                }

                DisableCopiedStageCameras(stageMapRoot.transform);
                ConfigureStageAnchors(stageRoot.transform);
                ConfigureStagePreviewCamera(stageRoot.transform);
                ConfigureSceneAtmosphere(stageScene);

                EditorUtility.SetDirty(stageRoot);
                EditorSceneManager.MarkSceneDirty(stageScene);
                if (!EditorSceneManager.SaveScene(stageScene, StageTargetScenePath))
                {
                    throw new InvalidOperationException($"Failed to save Olympus corridor invasion stage scene: {StageTargetScenePath}");
                }
            }
            finally
            {
                if (sourceSceneOpened && sourceScene.IsValid())
                {
                    EditorSceneManager.CloseScene(sourceScene, removeScene: true);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("DimensionBrawl/Validate Olympus Corridor Invasion Stage")]
        public static void ValidateOlympusCorridorInvasionStageMenu()
        {
            ValidateOlympusCorridorInvasionStage();
            Debug.Log("Olympus corridor invasion stage validation passed.");
        }

        public static void ValidateOlympusCorridorInvasionStage()
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(StageTargetScenePath))
            {
                throw new InvalidOperationException($"Missing Olympus corridor invasion stage scene: {StageTargetScenePath}");
            }

            Scene scene = EditorSceneManager.OpenScene(StageTargetScenePath, OpenSceneMode.Single);
            GameObject stageRoot = RequireRoot(scene, StageRootName);
            Transform stageMapRoot = RequireChild(stageRoot.transform, StageMapRootName);

            if (Mathf.Abs(stageMapRoot.localScale.x - StageMapScale) > 0.001f
                || Mathf.Abs(stageMapRoot.localScale.y - StageMapScale) > 0.001f
                || Mathf.Abs(stageMapRoot.localScale.z - StageMapScale) > 0.001f)
            {
                throw new InvalidOperationException($"Olympus corridor stage map root must stay uniformly scaled to {StageMapScale}.");
            }

            RequireChild(stageMapRoot, InvasionLookdevRootName);

            int rendererCount = CountEnabledRenderersInScene(scene.GetRootGameObjects());
            if (rendererCount < 80)
            {
                throw new InvalidOperationException($"Olympus corridor stage should preserve the invasion map renderer set. Found {rendererCount} enabled renderers.");
            }

            foreach (Camera camera in stageMapRoot.GetComponentsInChildren<Camera>(includeInactive: true))
            {
                if (camera.enabled)
                {
                    throw new InvalidOperationException($"Copied lookdev camera should be disabled in stage map root: {camera.name}");
                }
            }

            Camera previewCamera = RequireComponent<Camera>(RequireChild(stageRoot.transform, StagePreviewCameraName).gameObject);
            if (!previewCamera.enabled)
            {
                throw new InvalidOperationException("Stage preview camera should remain enabled for editor review.");
            }

            Transform anchorsRoot = RequireChild(stageRoot.transform, StageAnchorsName);
            Transform combatAnchors = RequireChild(anchorsRoot, StageCombatAnchorsName);
            Transform cutsceneAnchors = RequireChild(anchorsRoot, StageCutsceneAnchorsName);
            Transform runtimeAnchors = RequireChild(anchorsRoot, StageRuntimeAnchorsName);

            RequireChild(combatAnchors, "Player_LeftShoulderCameraAnchor");
            RequireChild(combatAnchors, "Boss_CenterLaneAnchor");
            RequireChild(combatAnchors, "Add_LeftLaneAnchor");
            RequireChild(combatAnchors, "Add_RightLaneAnchor");
            RequireChild(combatAnchors, "Rift_BackdropAnchor");
            RequireChild(cutsceneAnchors, "IntroCutscene_End_PlayerHandoffAnchor");
            RequireChild(cutsceneAnchors, "BossEntrance_BossRevealAnchor");
            RequireChild(cutsceneAnchors, "Gameplay_CombatStartAnchor");
            RequireChild(runtimeAnchors, "StageSpawner_PlayerStart");
            RequireChild(runtimeAnchors, "StageSpawner_BossCenter");
            RequireChild(runtimeAnchors, "StageClear_CorridorExit");
        }

        [MenuItem("DimensionBrawl/Render Olympus Corridor Invasion Stage Preview")]
        public static void RenderOlympusCorridorInvasionStagePreviewMenu()
        {
            string previewPath = RenderOlympusCorridorInvasionStagePreview();
            Debug.Log($"Rendered Olympus corridor invasion stage preview: {previewPath}");
        }

        public static string RenderOlympusCorridorInvasionStagePreview()
        {
            Scene scene = EditorSceneManager.OpenScene(StageTargetScenePath, OpenSceneMode.Single);
            GameObject stageRoot = RequireRoot(scene, StageRootName);
            Camera camera = RequireComponent<Camera>(RequireChild(stageRoot.transform, StagePreviewCameraName).gameObject);
            return RenderPreview(camera, StagePreviewFileName);
        }

        [MenuItem("DimensionBrawl/Apply Promoted UNI Fire VFX To Olympus Invasion")]
        public static void ApplyPromotedUniFireVfxToOlympusInvasionMenu()
        {
            ApplyPromotedUniFireVfxToOlympusInvasion();
            Debug.Log("Applied promoted UNI fire VFX to the Olympus invasion lookdev scene.");
        }

        public static void ApplyPromotedUniFireVfxToOlympusInvasion()
        {
            Scene scene = EditorSceneManager.OpenScene(InvasionTargetScenePath, OpenSceneMode.Single);
            GameObject root = RequireRoot(scene, InvasionLookdevRootName);
            Transform sanctuary = RequireChild(root.transform, SanctuaryVisualsName);
            sanctuary.gameObject.SetActive(true);

            RemoveChild(sanctuary, InvasionFireBillboardsRootName);
            RemoveChild(sanctuary, InvasionSmokeBillboardsRootName);
            RemoveChild(sanctuary, PromotedUniFireSmokeVfxRootName);

            if (EnableInvasionFireVisuals)
            {
                CreateInvasionFireAndSmoke(sanctuary);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }


        [MenuItem("DimensionBrawl/Ensure Olympus Corridor Decal Renderer Feature")]
        public static void EnsureOlympusCorridorDecalRendererFeatureMenu()
        {
            EnsureOlympusCorridorDecalRendererFeature();
            Debug.Log("Ensured Olympus corridor URP decal renderer features.");
        }
        public static void DisableOlympusCorridorImportedLightingOnly()
        {
            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            ConfigureImportedLighting(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        public static string RenderOlympusCorridorLookdevPreview()
        {
            Camera camera = OpenLookdevCamera();
            return RenderPreview(camera, PreviewFileName);
        }

        [MenuItem("DimensionBrawl/Render Olympus Corridor Close Preview")]
        public static void RenderOlympusCorridorLookdevClosePreviewMenu()
        {
            string previewPath = RenderOlympusCorridorLookdevClosePreview();
            Debug.Log($"Rendered Olympus corridor close preview: {previewPath}");
        }

        public static string RenderOlympusCorridorLookdevClosePreview()
        {
            Camera camera = OpenLookdevCamera();
            return RenderPreviewWithTemporaryCameraPose(
                camera,
                ClosePreviewFileName,
                new Vector3(-10.75f, 2.35f, 0f),
                Quaternion.Euler(11f, 90f, 0f),
                60f);
        }

        [MenuItem("DimensionBrawl/Render Olympus Corridor High Preview")]
        public static void RenderOlympusCorridorLookdevHighPreviewMenu()
        {
            string previewPath = RenderOlympusCorridorLookdevHighPreview();
            Debug.Log($"Rendered Olympus corridor high preview: {previewPath}");
        }

        public static string RenderOlympusCorridorLookdevHighPreview()
        {
            Camera camera = OpenLookdevCamera();
            return RenderPreviewWithTemporaryCameraPose(
                camera,
                HighPreviewFileName,
                new Vector3(-12.25f, 3.15f, 0f),
                Quaternion.Euler(15f, 90f, 0f),
                58f);
        }

        [MenuItem("DimensionBrawl/Render Olympus Corridor Dense Preview")]
        public static void RenderOlympusCorridorDenseLookdevPreviewMenu()
        {
            string previewPath = RenderOlympusCorridorDenseLookdevPreview();
            Debug.Log($"Rendered Olympus corridor dense preview: {previewPath}");
        }

        public static string RenderOlympusCorridorDenseLookdevPreview()
        {
            Camera camera = OpenLookdevCamera(DenseTargetScenePath, DenseLookdevRootName);
            return RenderPreviewWithTemporaryCameraPose(
                camera,
                DensePreviewFileName,
                new Vector3(-9.8f, 2.85f, 0f),
                Quaternion.Euler(12.5f, 90f, 0f),
                68f);
        }
        [MenuItem("DimensionBrawl/Render Olympus Corridor Invasion Preview")]
        public static void RenderOlympusCorridorInvasionLookdevPreviewMenu()
        {
            string previewPath = RenderOlympusCorridorInvasionLookdevPreview();
            Debug.Log($"Rendered Olympus corridor invasion preview: {previewPath}");
        }

        public static string RenderOlympusCorridorInvasionLookdevPreview()
        {
            Camera camera = OpenLookdevCamera(InvasionTargetScenePath, InvasionLookdevRootName);
            return RenderPreviewWithTemporaryCameraPose(
                camera,
                InvasionPreviewFileName,
                new Vector3(-9.35f, 3.05f, 0f),
                Quaternion.Euler(13.5f, 90f, 0f),
                66f);
        }

        [InitializeOnLoadMethod]
        private static void RegisterInvasionPlayPreviewWatcher()
        {
            EditorApplication.playModeStateChanged -= HandleInvasionPlayPreviewStateChanged;
            EditorApplication.playModeStateChanged += HandleInvasionPlayPreviewStateChanged;
            if (EditorApplication.isPlaying && SessionState.GetBool(InvasionPlayPreviewPendingKey, false))
            {
                BeginInvasionPlayPreviewCapture();
            }
        }

        [MenuItem("DimensionBrawl/Capture Olympus Corridor Invasion Play Preview")]
        public static void CaptureOlympusCorridorInvasionPlayPreviewMenu()
        {
            if (EditorApplication.isPlaying)
            {
                SessionState.SetBool(InvasionPlayPreviewPendingKey, true);
                BeginInvasionPlayPreviewCapture();
                return;
            }

            EditorSceneManager.OpenScene(InvasionTargetScenePath, OpenSceneMode.Single);
            SessionState.SetBool(InvasionPlayPreviewPendingKey, true);
            EditorApplication.EnterPlaymode();
        }

        private static void HandleInvasionPlayPreviewStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(InvasionPlayPreviewPendingKey, false))
            {
                BeginInvasionPlayPreviewCapture();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.update -= CaptureInvasionPlayPreviewWhenReady;
            }
        }

        private static void BeginInvasionPlayPreviewCapture()
        {
            invasionPlayPreviewCaptureStartTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= CaptureInvasionPlayPreviewWhenReady;
            EditorApplication.update += CaptureInvasionPlayPreviewWhenReady;
        }

        private static void CaptureInvasionPlayPreviewWhenReady()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup - invasionPlayPreviewCaptureStartTime < InvasionPlayPreviewWarmupSeconds)
            {
                return;
            }

            int exitCode = 0;
            try
            {
                string previewPath = CaptureOlympusCorridorInvasionPlayPreviewFromActiveScene();
                Debug.Log($"Captured Olympus corridor invasion play preview: {previewPath}");
            }
            catch (Exception exception)
            {
                exitCode = 1;
                Debug.LogException(exception);
            }
            finally
            {
                SessionState.SetBool(InvasionPlayPreviewPendingKey, false);
                EditorApplication.update -= CaptureInvasionPlayPreviewWhenReady;
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
                else
                {
                    EditorApplication.ExitPlaymode();
                }
            }
        }

        public static string CaptureOlympusCorridorInvasionPlayPreviewFromActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject lookdevRoot = RequireRoot(scene, InvasionLookdevRootName);
            Camera camera = RequireComponent<Camera>(RequireChild(lookdevRoot.transform, CameraName).gameObject);
            return CapturePlayScreenWithTemporaryCameraPose(
                camera,
                InvasionPlayPreviewFileName,
                new Vector3(-9.35f, 3.05f, 0f),
                Quaternion.Euler(13.5f, 90f, 0f),
                66f);
        }

        private static string CapturePlayScreenWithTemporaryCameraPose(Camera camera, string fileName, Vector3 position, Quaternion rotation, float fieldOfView)
        {
            Vector3 previousPosition = camera.transform.position;
            Quaternion previousRotation = camera.transform.rotation;
            float previousFieldOfView = camera.fieldOfView;

            try
            {
                camera.transform.position = position;
                camera.transform.rotation = rotation;
                camera.fieldOfView = fieldOfView;
                camera.enabled = true;
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
                camera.Render();
                return CapturePlayScreen(fileName);
            }
            finally
            {
                camera.transform.position = previousPosition;
                camera.transform.rotation = previousRotation;
                camera.fieldOfView = previousFieldOfView;
            }
        }

        private static string CapturePlayScreen(string fileName)
        {
            string previewPath = Path.Combine(Path.GetTempPath(), fileName);
            Texture2D preview = ScreenCapture.CaptureScreenshotAsTexture();
            if (preview == null)
            {
                throw new InvalidOperationException("ScreenCapture.CaptureScreenshotAsTexture returned null during Play Mode preview capture.");
            }

            try
            {
                File.WriteAllBytes(previewPath, preview.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(preview);
            }

            return previewPath;
        }
        private static Camera OpenLookdevCamera()
        {
            return OpenLookdevCamera(TargetScenePath, LookdevRootName);
        }

        private static Camera OpenLookdevCamera(string scenePath, string lookdevRootName)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject lookdevRoot = RequireRoot(scene, lookdevRootName);
            return RequireComponent<Camera>(RequireChild(lookdevRoot.transform, CameraName).gameObject);
        }

        private static string RenderPreviewWithTemporaryCameraPose(Camera camera, string fileName, Vector3 position, Quaternion rotation, float fieldOfView)
        {
            Vector3 previousPosition = camera.transform.position;
            Quaternion previousRotation = camera.transform.rotation;
            float previousFieldOfView = camera.fieldOfView;

            try
            {
                camera.transform.position = position;
                camera.transform.rotation = rotation;
                camera.fieldOfView = fieldOfView;
                return RenderPreview(camera, fileName);
            }
            finally
            {
                camera.transform.position = previousPosition;
                camera.transform.rotation = previousRotation;
                camera.fieldOfView = previousFieldOfView;
            }
        }

        private static string RenderPreview(Camera camera, string fileName)
        {
            const int width = 1280;
            const int height = 720;
            string previewPath = Path.Combine(Path.GetTempPath(), fileName);

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D preview = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                SimulateSceneVisualEffects(camera.gameObject.scene, 1.8f);
                camera.Render();
                preview.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                preview.Apply();
                File.WriteAllBytes(previewPath, preview.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(preview);
            }

            return previewPath;
        }

        private struct PgrEnvironmentMaterialStyle
        {
            public PgrEnvironmentMaterialStyle(Color baseColor, Color emissionColor, float metallic, float smoothness, float sourceColorBlend)
            {
                BaseColor = baseColor;
                EmissionColor = emissionColor;
                Metallic = metallic;
                Smoothness = smoothness;
                SourceColorBlend = sourceColorBlend;
            }

            public Color BaseColor;
            public Color EmissionColor;
            public float Metallic;
            public float Smoothness;
            public float SourceColorBlend;
        }
        private static void RestoreSourceEnvironmentMaterials(Scene scene)
        {
            int restoredSlots = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (IsGeneratedLookdevRoot(root.name))
                {
                    continue;
                }

                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    if (renderer is LineRenderer || renderer is TrailRenderer || renderer is ParticleSystemRenderer || renderer is SpriteRenderer)
                    {
                        continue;
                    }

                    Material[] materials = renderer.sharedMaterials;
                    bool changed = false;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (!TryResolveSourceMaterialFromGeneratedMaterial(materials[i], out Material sourceMaterial))
                        {
                            continue;
                        }

                        materials[i] = sourceMaterial;
                        restoredSlots++;
                        changed = true;
                    }

                    if (changed)
                    {
                        renderer.sharedMaterials = materials;
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        renderer.receiveShadows = true;
                        EditorUtility.SetDirty(renderer);
                    }
                }
            }

            Debug.Log($"Olympus corridor lookdev restored {restoredSlots} generated material slots back to source Olympus materials.");
        }


        private static void ConfigureImportedLighting(Scene scene)
        {
            int disabledRootCount = 0;
            int disabledLightCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (!string.Equals(root.name, ImportedLightingRootName, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (Light light in root.GetComponentsInChildren<Light>(includeInactive: true))
                {
                    if (light.enabled)
                    {
                        light.enabled = false;
                        disabledLightCount++;
                        EditorUtility.SetDirty(light);
                    }
                }

                if (root.activeSelf)
                {
                    root.SetActive(false);
                    disabledRootCount++;
                    EditorUtility.SetDirty(root);
                }
            }

            Debug.Log($"Olympus corridor lookdev disabled {disabledRootCount} imported lighting roots and {disabledLightCount} imported light components so lookdev lighting is not doubled.");
        }
        private static void ConfigurePgrPreserveEnvironmentMaterials(Scene scene)
        {
            Dictionary<Material, Material> remapCache = new Dictionary<Material, Material>();
            int remappedSlots = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (IsGeneratedLookdevRoot(root.name))
                {
                    continue;
                }

                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    if (renderer is LineRenderer || renderer is TrailRenderer || renderer is ParticleSystemRenderer || renderer is SpriteRenderer)
                    {
                        continue;
                    }

                    Material[] materials = renderer.sharedMaterials;
                    bool changed = false;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material sourceMaterial = materials[i];
                        if (TryResolveSourceMaterialFromGeneratedMaterial(sourceMaterial, out Material resolvedSourceMaterial))
                        {
                            sourceMaterial = resolvedSourceMaterial;
                        }

                        if (!ShouldUsePgrPreserveSourceMaterial(sourceMaterial))
                        {
                            continue;
                        }

                        if (!remapCache.TryGetValue(sourceMaterial, out Material pgrMaterial))
                        {
                            pgrMaterial = EnsurePgrPreserveEnvironmentMaterial(sourceMaterial);
                            remapCache[sourceMaterial] = pgrMaterial;
                        }

                        if (pgrMaterial != null && materials[i] != pgrMaterial)
                        {
                            materials[i] = pgrMaterial;
                            remappedSlots++;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        renderer.sharedMaterials = materials;
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        renderer.receiveShadows = true;
                        EditorUtility.SetDirty(renderer);
                    }
                }
            }

            Debug.Log($"Olympus corridor lookdev remapped {remappedSlots} material slots into {remapCache.Count} PGR-preserve Olympus material variants.");
        }

        private static bool IsGeneratedLookdevRoot(string rootName)
        {
            return string.Equals(rootName, LookdevRootName, StringComparison.Ordinal)
                || string.Equals(rootName, DenseLookdevRootName, StringComparison.Ordinal)
                || string.Equals(rootName, InvasionLookdevRootName, StringComparison.Ordinal);
        }

        private static void RestoreFullDemoRendererDensity(Scene scene)
        {
            int restoredRendererCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (IsGeneratedLookdevRoot(root.name))
                {
                    continue;
                }

                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
                {
                    if (renderer.enabled)
                    {
                        continue;
                    }

                    renderer.enabled = true;
                    restoredRendererCount++;
                    EditorUtility.SetDirty(renderer);
                }
            }

            Debug.Log($"Olympus corridor dense lookdev restored {restoredRendererCount} source mesh renderers for demo-level structure density.");
        }
        private static bool ShouldUsePgrPreserveSourceMaterial(Material sourceMaterial)
        {
            if (sourceMaterial == null)
            {
                return false;
            }

            string materialPath = AssetDatabase.GetAssetPath(sourceMaterial);
            if (string.IsNullOrEmpty(materialPath))
            {
                return false;
            }

            return materialPath.StartsWith(GetSourceMaterialRoot(), StringComparison.Ordinal) && ShouldUseOlympusSourceMaterialName(sourceMaterial.name);
        }

        private static Material EnsurePgrPreserveEnvironmentMaterial(Material sourceMaterial)
        {
            string materialPath = PgrPreserveMaterialRoot + "/DB_OlympusCorridor_PGR_" + SanitizeAssetName(sourceMaterial.name) + ".mat";
            Material pgrMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (pgrMaterial == null)
            {
                pgrMaterial = new Material(sourceMaterial);
                AssetDatabase.CreateAsset(pgrMaterial, materialPath);
            }
            else
            {
                pgrMaterial.CopyPropertiesFromMaterial(sourceMaterial);
            }

            pgrMaterial.name = Path.GetFileNameWithoutExtension(materialPath);
            ApplyPgrMaterialStyle(pgrMaterial, sourceMaterial);
            pgrMaterial.enableInstancing = true;
            EditorUtility.SetDirty(pgrMaterial);
            return pgrMaterial;
        }

        private static void ApplyPgrMaterialStyle(Material targetMaterial, Material sourceMaterial)
        {
            PgrEnvironmentMaterialStyle style = BuildPgrEnvironmentMaterialStyle(sourceMaterial);
            Color sampledColor = ExtractSourceMaterialColor(sourceMaterial, style.BaseColor);
            Color baseColor = Color.Lerp(style.BaseColor, sampledColor, style.SourceColorBlend);
            Color lightColor = Color.Lerp(baseColor, Color.white, 0.32f);
            Color coolAccent = Color.Lerp(baseColor, new Color(0.58f, 0.78f, 1f, 1f), 0.22f);
            Color softShadow = Color.Lerp(baseColor, new Color(0.56f, 0.68f, 0.9f, 1f), 0.18f);
            Color flatGrunge = Color.Lerp(baseColor, Color.white, 0.24f);
            softShadow.a = 0f;
            flatGrunge.a = 0f;

            SetMaterialColor(targetMaterial, "_BaseColor", baseColor);
            SetMaterialColor(targetMaterial, "_Color", baseColor);
            SetMaterialColor(targetMaterial, "_ColorR", lightColor);
            SetMaterialColor(targetMaterial, "_ColorG", baseColor);
            SetMaterialColor(targetMaterial, "_ColorB", coolAccent);
            SetMaterialColor(targetMaterial, "_ColorBlack", softShadow);
            SetMaterialColor(targetMaterial, "_ColorGrunge", flatGrunge);
            SetMaterialColor(targetMaterial, "_PatternASharpness", new Color(2.5f, 2.5f, 2.5f, 0f));
            SetMaterialColor(targetMaterial, "_EmissionColor", style.EmissionColor);
            SetMaterialColor(targetMaterial, "_Emissive_Color", style.EmissionColor);
            SetMaterialFloat(targetMaterial, "_Metallic", style.Metallic);
            SetMaterialFloat(targetMaterial, "_Smoothness", style.Smoothness);
            SetMaterialFloat(targetMaterial, "_SmoothnessRemapMin", Mathf.Clamp01(style.Smoothness - 0.12f));
            SetMaterialFloat(targetMaterial, "_SmoothnessRemapMax", Mathf.Clamp01(style.Smoothness + 0.06f));
            SetMaterialFloat(targetMaterial, "_NormalStrength", 0.04f);
            SetMaterialFloat(targetMaterial, "_BumpScale", 0.04f);
            SetMaterialFloat(targetMaterial, "_ColorGrungeBlend", 0.16f);
            SetMaterialFloat(targetMaterial, "_ColorGrungeInt", 1f);
            SetMaterialFloat(targetMaterial, "_Tiling_Grunge", 0.24f);
            SetMaterialFloat(targetMaterial, "_Roughness", 10f);
            SetMaterialFloat(targetMaterial, "_Tiling", 1.05f);
            SetMaterialFloat(targetMaterial, "_ReceivesSSR", 0f);
            CopyMaterialTexture(sourceMaterial, targetMaterial, "_IDMap");
            ClearMaterialTexture(targetMaterial, "_NormalMap");
            ClearMaterialTexture(targetMaterial, "_BumpMap");
            ClearMaterialTexture(targetMaterial, "_MRAEMap");
            ClearMaterialTexture(targetMaterial, "_GrungeMap");
        }

        private static PgrEnvironmentMaterialStyle BuildPgrEnvironmentMaterialStyle(Material sourceMaterial)
        {
            string materialName = sourceMaterial.name.ToLowerInvariant();
            if (ContainsAny(materialName, "gold", "trim", "lamp"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(1f, 0.78f, 0.36f, 1f), new Color(0.12f, 0.07f, 0.02f, 1f), 0.72f, 0.72f, 0.08f);
            }

            if (ContainsAny(materialName, "glass"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(0.62f, 0.86f, 1f, 0.78f), new Color(0.02f, 0.06f, 0.12f, 1f), 0f, 0.62f, 0.04f);
            }

            if (ContainsAny(materialName, "cloth", "flag", "wave"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(0.52f, 0.72f, 1f, 1f), new Color(0.01f, 0.025f, 0.07f, 1f), 0f, 0.46f, 0f);
            }

            if (ContainsAny(materialName, "green", "tile"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(0.72f, 0.86f, 0.98f, 1f), Color.black, 0.02f, 0.48f, 0.02f);
            }

            if (ContainsAny(materialName, "dark"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(0.94f, 0.98f, 1f, 1f), Color.black, 0.01f, 0.52f, 0f);
            }

            if (ContainsAny(materialName, "ground", "marble", "sculpture", "cayn", "gray", "cylinder"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(0.98f, 1f, 1f, 1f), Color.black, 0.01f, 0.54f, 0f);
            }

            if (ContainsAny(materialName, "pine", "leaf", "leaves", "bark"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(0.46f, 0.68f, 0.68f, 1f), Color.black, 0f, 0.42f, 0.04f);
            }

            if (ContainsAny(materialName, "cloud", "emissive"))
            {
                return new PgrEnvironmentMaterialStyle(new Color(0.72f, 0.88f, 1f, 1f), new Color(0.035f, 0.09f, 0.18f, 1f), 0f, 0.48f, 0.02f);
            }

            return new PgrEnvironmentMaterialStyle(new Color(0.76f, 0.88f, 1f, 1f), Color.black, 0.01f, 0.5f, 0.02f);
        }
        private static bool TryResolveSourceMaterialFromGeneratedMaterial(Material generatedMaterial, out Material sourceMaterial)
        {
            sourceMaterial = null;
            if (generatedMaterial == null)
            {
                return false;
            }

            string generatedPath = AssetDatabase.GetAssetPath(generatedMaterial);
            if (string.IsNullOrEmpty(generatedPath))
            {
                return false;
            }

            if (!TryGetGeneratedSourceMaterialName(generatedPath, PgrPreserveMaterialRoot, "DB_OlympusCorridor_PGR_", out string sanitizedSourceName))
            {
                return false;
            }

            foreach (string sourceGuid in AssetDatabase.FindAssets("t:Material", new[] { GetSourceMaterialRoot() }))
            {
                string sourcePath = AssetDatabase.GUIDToAssetPath(sourceGuid);
                Material candidate = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
                if (ShouldUsePgrPreserveSourceMaterial(candidate) && string.Equals(SanitizeAssetName(candidate.name), sanitizedSourceName, StringComparison.Ordinal))
                {
                    sourceMaterial = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetGeneratedSourceMaterialName(string generatedPath, string generatedRoot, string generatedPrefix, out string sanitizedSourceName)
        {
            sanitizedSourceName = null;
            if (!generatedPath.StartsWith(generatedRoot + "/", StringComparison.Ordinal))
            {
                return false;
            }

            string generatedName = Path.GetFileNameWithoutExtension(generatedPath);
            if (!generatedName.StartsWith(generatedPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            sanitizedSourceName = generatedName.Substring(generatedPrefix.Length);
            return true;
        }

        private static bool ShouldUseOlympusSourceMaterialName(string materialName)
        {
            if (string.IsNullOrEmpty(materialName) || !materialName.StartsWith("M_", StringComparison.Ordinal))
            {
                return false;
            }

            string lowerName = materialName.ToLowerInvariant();
            return !ContainsAny(lowerName, "decal", "dust");
        }

        private static string GetSourceAssetRoot()
        {
            const string sceneMarker = "/Scene/";
            int sceneIndex = SourceScenePath.IndexOf(sceneMarker, StringComparison.Ordinal);
            if (sceneIndex < 0)
            {
                throw new InvalidOperationException($"Source scene path should contain {sceneMarker}: {SourceScenePath}");
            }

            return SourceScenePath.Substring(0, sceneIndex + 1);
        }


        private static string GetSourceMaterialRoot()
        {
            return GetSourceAssetRoot() + "Art/Materials";
        }

        private static Color ExtractSourceMaterialColor(Material sourceMaterial, Color fallback)
        {
            Color sum = Color.black;
            int count = 0;
            AccumulateMaterialColor(sourceMaterial, "_ColorR", ref sum, ref count);
            AccumulateMaterialColor(sourceMaterial, "_ColorG", ref sum, ref count);
            AccumulateMaterialColor(sourceMaterial, "_ColorB", ref sum, ref count);
            AccumulateMaterialColor(sourceMaterial, "_BaseColor", ref sum, ref count);
            AccumulateMaterialColor(sourceMaterial, "_Color", ref sum, ref count);
            if (count == 0)
            {
                return fallback;
            }

            return new Color(sum.r / count, sum.g / count, sum.b / count, 1f);
        }

        private static void AccumulateMaterialColor(Material material, string propertyName, ref Color sum, ref int count)
        {
            if (!material.HasProperty(propertyName))
            {
                return;
            }

            Color color = material.GetColor(propertyName);
            if (color.maxColorComponent < 0.02f || color.maxColorComponent > 1.4f)
            {
                return;
            }

            sum += new Color(color.r, color.g, color.b, 1f);
            count++;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
            {
                if (value.Contains(needles[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static string SanitizeAssetName(string name)
        {
            char[] chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private static void SetMaterialColor(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void CopyMaterialTexture(Material sourceMaterial, Material targetMaterial, string propertyName)
        {
            if (sourceMaterial.HasProperty(propertyName) && targetMaterial.HasProperty(propertyName))
            {
                targetMaterial.SetTexture(propertyName, sourceMaterial.GetTexture(propertyName));
            }
        }

        private static void ClearMaterialTexture(Material material, string propertyName)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, null);
            }
        }

        private static void ConfigureLookdevRoot(Scene scene)
        {
            RemoveRoot(scene, LookdevRootName);

            GameObject root = new GameObject(LookdevRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            ConfigurePostProcessVolume(root.transform);
            ConfigureLookdevCamera(root.transform);
            ConfigureLookdevLighting(root.transform);
            ConfigureSanctuaryVisuals(root.transform);
            ConfigureCombatAnchors(root.transform);

            EditorUtility.SetDirty(root);
        }

        private static void ConfigurePostProcessVolume(Transform root)
        {
            VolumeProfile profile = EnsurePostProcessProfile();
            GameObject volumeObject = CreateChild(root, VolumeName, Vector3.zero, Quaternion.identity, Vector3.one);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 40f;
            volume.weight = 1f;
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(volume);
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
            SetParameter(bloom.threshold, 1f);
            SetParameter(bloom.intensity, 0.26f);
            SetParameter(bloom.scatter, 0.38f);
            SetParameter(bloom.clamp, 3.4f);
            SetParameter(bloom.tint, new Color(0.94f, 0.97f, 1f, 1f));
            SetParameter(bloom.highQualityFiltering, false);
            SetParameter(bloom.maxIterations, 4);

            Tonemapping tonemapping = GetOrAddVolumeComponent<Tonemapping>(profile);
            tonemapping.active = true;
            SetParameter(tonemapping.mode, TonemappingMode.ACES);

            ColorAdjustments color = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            color.active = true;
            SetParameter(color.postExposure, 0.64f);
            SetParameter(color.contrast, 15f);
            SetParameter(color.colorFilter, new Color(0.96f, 0.985f, 1f, 1f));
            SetParameter(color.saturation, 7f);

            WhiteBalance whiteBalance = GetOrAddVolumeComponent<WhiteBalance>(profile);
            whiteBalance.active = true;
            SetParameter(whiteBalance.temperature, -3f);
            SetParameter(whiteBalance.tint, 1f);

            Vignette vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.active = true;
            SetParameter(vignette.color, new Color(0.05f, 0.055f, 0.07f, 1f));
            SetParameter(vignette.center, new Vector2(0.5f, 0.48f));
            SetParameter(vignette.intensity, 0.018f);
            SetParameter(vignette.smoothness, 0.55f);

            DepthOfField depthOfField = GetOrAddVolumeComponent<DepthOfField>(profile);
            depthOfField.active = false;
            SetParameter(depthOfField.mode, DepthOfFieldMode.Gaussian);
            SetParameter(depthOfField.gaussianStart, 18f);
            SetParameter(depthOfField.gaussianEnd, 52f);
            SetParameter(depthOfField.gaussianMaxRadius, 0.75f);
            SetParameter(depthOfField.highQualitySampling, false);

            EditorUtility.SetDirty(profile);
            return profile;
        }


        private static void ConfigureInvasionLookdevRoot(Scene scene)
        {
            RemoveRoot(scene, InvasionLookdevRootName);

            GameObject root = new GameObject(InvasionLookdevRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            ConfigurePostProcessVolume(root.transform);
            ConfigureDenseLookdevCamera(root.transform);
            ConfigureLookdevLighting(root.transform);
            ConfigureInvasionLighting(root.transform);
            ConfigureInvasionSanctuaryVisuals(root.transform);
            ConfigureCombatAnchors(root.transform);

            EditorUtility.SetDirty(root);
        }

        private static void DisableCopiedStageCameras(Transform stageMapRoot)
        {
            foreach (Camera camera in stageMapRoot.GetComponentsInChildren<Camera>(includeInactive: true))
            {
                camera.enabled = false;
                EditorUtility.SetDirty(camera);
            }

            foreach (AudioListener audioListener in stageMapRoot.GetComponentsInChildren<AudioListener>(includeInactive: true))
            {
                audioListener.enabled = false;
                EditorUtility.SetDirty(audioListener);
            }
        }

        private static void ConfigureStagePreviewCamera(Transform root)
        {
            GameObject cameraObject = CreateChild(
                root,
                StagePreviewCameraName,
                new Vector3(-14.7f, 4.28f, 0f),
                Quaternion.Euler(12.5f, 90f, 0f),
                Vector3.one);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 66f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 330f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.depth = 20f;

            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;

            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(cameraData);
        }

        private static void ConfigureStageAnchors(Transform root)
        {
            GameObject anchorsRoot = CreateChild(root, StageAnchorsName, Vector3.zero, Quaternion.identity, Vector3.one);
            Transform combatAnchors = CreateChild(anchorsRoot.transform, StageCombatAnchorsName, Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform cutsceneAnchors = CreateChild(anchorsRoot.transform, StageCutsceneAnchorsName, Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform runtimeAnchors = CreateChild(anchorsRoot.transform, StageRuntimeAnchorsName, Vector3.zero, Quaternion.identity, Vector3.one).transform;

            CreateChild(combatAnchors, "Player_LeftShoulderCameraAnchor", new Vector3(-16.5f, 1.8f, -4.65f), Quaternion.Euler(0f, 82f, 0f), Vector3.one);
            CreateChild(combatAnchors, "Boss_CenterLaneAnchor", new Vector3(15.3f, 0f, 0f), Quaternion.identity, Vector3.one);
            CreateChild(combatAnchors, "Add_LeftLaneAnchor", new Vector3(13.35f, 0f, -1.875f), Quaternion.identity, Vector3.one);
            CreateChild(combatAnchors, "Add_RightLaneAnchor", new Vector3(13.35f, 0f, 1.875f), Quaternion.identity, Vector3.one);
            CreateChild(combatAnchors, "Rift_BackdropAnchor", new Vector3(22.2f, 3.975f, 0f), Quaternion.identity, Vector3.one);

            CreateChild(cutsceneAnchors, "IntroCutscene_End_PlayerHandoffAnchor", new Vector3(-16.5f, 1.8f, -4.65f), Quaternion.Euler(0f, 82f, 0f), Vector3.one);
            CreateChild(cutsceneAnchors, "BossEntrance_BossRevealAnchor", new Vector3(15.3f, 1.6f, 0f), Quaternion.identity, Vector3.one);
            CreateChild(cutsceneAnchors, "Gameplay_CombatStartAnchor", new Vector3(-16.5f, 0f, -4.65f), Quaternion.Euler(0f, 82f, 0f), Vector3.one);

            CreateChild(runtimeAnchors, "StageSpawner_PlayerStart", new Vector3(-16.5f, 0f, -4.65f), Quaternion.Euler(0f, 82f, 0f), Vector3.one);
            CreateChild(runtimeAnchors, "StageSpawner_BossCenter", new Vector3(15.3f, 0f, 0f), Quaternion.identity, Vector3.one);
            CreateChild(runtimeAnchors, "StageClear_CorridorExit", new Vector3(27f, 0f, 0f), Quaternion.identity, Vector3.one);

            EditorUtility.SetDirty(anchorsRoot);
        }

        private static void ConfigureDenseLookdevRoot(Scene scene)
        {
            RemoveRoot(scene, DenseLookdevRootName);

            GameObject root = new GameObject(DenseLookdevRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            ConfigurePostProcessVolume(root.transform);
            ConfigureDenseLookdevCamera(root.transform);
            ConfigureLookdevLighting(root.transform);
            ConfigureSanctuaryVisuals(root.transform);
            ConfigureCombatAnchors(root.transform);

            EditorUtility.SetDirty(root);
        }
        private static void ConfigureLookdevCamera(Transform root)
        {
            GameObject cameraObject = CreateChild(
                root,
                CameraName,
                new Vector3(-18f, 2.35f, 0f),
                Quaternion.Euler(18f, 90f, 0f),
                Vector3.one);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 180f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.depth = 10f;

            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;

            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(cameraData);
        }

        private static void ConfigureDenseLookdevCamera(Transform root)
        {
            GameObject cameraObject = CreateChild(
                root,
                CameraName,
                new Vector3(-9.8f, 2.85f, 0f),
                Quaternion.Euler(12.5f, 90f, 0f),
                Vector3.one);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 66f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 220f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.depth = 10f;

            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;

            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(cameraData);
        }
        private static void ConfigureInvasionLighting(Transform root)
        {
            GameObject lightingRoot = CreateChild(root, "OlympusCorridor_InvasionFxLighting", Vector3.zero, Quaternion.identity, Vector3.one);

            Light leftPlanterFire = CreateLight(lightingRoot.transform, "FireFx_LeftOuterFloorNear", new Vector3(-4.15f, 0.92f, -3.25f));
            leftPlanterFire.type = LightType.Point;
            leftPlanterFire.color = new Color(1f, 0.34f, 0.08f, 1f);
            leftPlanterFire.intensity = 0.72f;
            leftPlanterFire.range = 5.8f;
            leftPlanterFire.shadows = LightShadows.None;
            leftPlanterFire.bounceIntensity = 0f;

            Light rightRailFire = CreateLight(lightingRoot.transform, "FireFx_RightOuterFloorNear", new Vector3(-2.4f, 0.9f, 3.2f));
            rightRailFire.type = LightType.Point;
            rightRailFire.color = new Color(1f, 0.28f, 0.06f, 1f);
            rightRailFire.intensity = 0.68f;
            rightRailFire.range = 5.6f;
            rightRailFire.shadows = LightShadows.None;
            rightRailFire.bounceIntensity = 0f;

            Light leftGateFire = CreateLight(lightingRoot.transform, "FireFx_LeftOuterFloorBack", new Vector3(8.6f, 1f, -3.2f));
            leftGateFire.type = LightType.Point;
            leftGateFire.color = new Color(1f, 0.3f, 0.06f, 1f);
            leftGateFire.intensity = 0.86f;
            leftGateFire.range = 7f;
            leftGateFire.shadows = LightShadows.None;
            leftGateFire.bounceIntensity = 0f;

            Light rightGateFire = CreateLight(lightingRoot.transform, "FireFx_RightOuterFloorBack", new Vector3(9f, 1f, 3.2f));
            rightGateFire.type = LightType.Point;
            rightGateFire.color = new Color(1f, 0.28f, 0.06f, 1f);
            rightGateFire.intensity = 0.9f;
            rightGateFire.range = 7f;
            rightGateFire.shadows = LightShadows.None;
            rightGateFire.bounceIntensity = 0f;

            Light farLeftCanopyFire = CreateLight(lightingRoot.transform, "FireFx_FarLeftBackgroundFloor", new Vector3(12.4f, 1.2f, -3.45f));
            farLeftCanopyFire.type = LightType.Point;
            farLeftCanopyFire.color = new Color(1f, 0.24f, 0.05f, 1f);
            farLeftCanopyFire.intensity = 0.75f;
            farLeftCanopyFire.range = 7.5f;
            farLeftCanopyFire.shadows = LightShadows.None;
            farLeftCanopyFire.bounceIntensity = 0f;

            Light farRightCanopyFire = CreateLight(lightingRoot.transform, "FireFx_FarRightBackgroundFloor", new Vector3(12.9f, 1.2f, 3.45f));
            farRightCanopyFire.type = LightType.Point;
            farRightCanopyFire.color = new Color(1f, 0.25f, 0.055f, 1f);
            farRightCanopyFire.intensity = 0.78f;
            farRightCanopyFire.range = 7.5f;
            farRightCanopyFire.shadows = LightShadows.None;
            farRightCanopyFire.bounceIntensity = 0f;

            Light rearGateFire = CreateLight(lightingRoot.transform, "FireFx_RearBackgroundFloor", new Vector3(14.2f, 1f, 0f));
            rearGateFire.type = LightType.Point;
            rearGateFire.color = new Color(1f, 0.3f, 0.08f, 1f);
            rearGateFire.intensity = 0.35f;
            rearGateFire.range = 6.5f;
            rearGateFire.shadows = LightShadows.None;
            rearGateFire.bounceIntensity = 0f;
        }
        private static void ConfigureLookdevLighting(Transform root)
        {
            GameObject lightingRoot = CreateChild(root, LightingName, Vector3.zero, Quaternion.identity, Vector3.one);

            Light key = CreateLight(lightingRoot.transform, "CoolCorridorKey", new Vector3(0f, 4.8f, -3.6f));
            key.type = LightType.Point;
            key.color = new Color(0.74f, 0.84f, 1f, 1f);
            key.intensity = 1.32f;
            key.range = 18f;
            key.shadows = LightShadows.None;
            key.bounceIntensity = 0f;


            Light sun = CreateLight(lightingRoot.transform, "HeavenlyCorridorSun", new Vector3(-8f, 7.5f, -5.5f));
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(48f, 112f, 0f);
            sun.color = new Color(0.9f, 0.95f, 1f, 1f);
            sun.intensity = 0.72f;
            sun.shadows = LightShadows.None;
            sun.bounceIntensity = 0f;
            Light gold = CreateLight(lightingRoot.transform, "WarmGoldAccent", new Vector3(0f, 3.1f, 2.8f));
            gold.type = LightType.Point;
            gold.color = new Color(1f, 0.77f, 0.42f, 1f);
            gold.intensity = 1.24f;
            gold.range = 12f;
            gold.shadows = LightShadows.None;
            gold.bounceIntensity = 0f;

            Light endGlow = CreateLight(lightingRoot.transform, "EndPortalBacklight", new Vector3(18f, 3.8f, 0f));
            endGlow.type = LightType.Point;
            endGlow.color = new Color(0.58f, 0.8f, 1f, 1f);
            endGlow.intensity = 3.18f;
            endGlow.range = 34f;
            endGlow.shadows = LightShadows.None;
            endGlow.bounceIntensity = 0f;

            Light riftCore = CreateLight(lightingRoot.transform, "BlueRiftCore", new Vector3(14.8f, 2.65f, 0f));
            riftCore.type = LightType.Point;
            riftCore.color = new Color(0.34f, 0.62f, 1f, 1f);
            riftCore.intensity = 2.42f;
            riftCore.range = 26f;
            riftCore.shadows = LightShadows.None;
            riftCore.bounceIntensity = 0f;

            Light gateGold = CreateLight(lightingRoot.transform, "GateGoldCrown", new Vector3(14.2f, 3.15f, 0f));
            gateGold.type = LightType.Point;
            gateGold.color = new Color(1f, 0.68f, 0.24f, 1f);
            gateGold.intensity = 1.25f;
            gateGold.range = 12f;
            gateGold.shadows = LightShadows.None;
            gateGold.bounceIntensity = 0f;

            Light floorBounce = CreateLight(lightingRoot.transform, "WetFloorBlueBounce", new Vector3(6f, 0.55f, 0f));
            floorBounce.type = LightType.Point;
            floorBounce.color = new Color(0.18f, 0.46f, 1f, 1f);
            floorBounce.intensity = 0.64f;
            floorBounce.range = 12f;
            floorBounce.shadows = LightShadows.None;
            floorBounce.bounceIntensity = 0f;

            Light leftFill = CreateLight(lightingRoot.transform, "LeftWallBlueFill", new Vector3(-3f, 2.25f, -3.35f));
            leftFill.type = LightType.Point;
            leftFill.color = new Color(0.24f, 0.5f, 1f, 1f);
            leftFill.intensity = 0.34f;
            leftFill.range = 11f;
            leftFill.shadows = LightShadows.None;
            leftFill.bounceIntensity = 0f;

            Light rightFill = CreateLight(lightingRoot.transform, "RightWallBlueFill", new Vector3(-3f, 2.25f, 3.35f));
            rightFill.type = LightType.Point;
            rightFill.color = new Color(0.24f, 0.5f, 1f, 1f);
            rightFill.intensity = 0.34f;
            rightFill.range = 11f;
            rightFill.shadows = LightShadows.None;
            rightFill.bounceIntensity = 0f;
        }

        private static void ConfigureInvasionSanctuaryVisuals(Transform root)
        {
            Material goldGlow = EnsureMaterial(GoldGlowMaterialPath, new Color(1f, 0.68f, 0.26f, 1f), new Color(2.35f, 1.36f, 0.36f, 1f));
            Material scorchedStone = EnsureMaterial(ScorchedStoneMaterialPath, new Color(0.42f, 0.38f, 0.36f, 1f), new Color(0.055f, 0.025f, 0.012f, 1f));
            Material damagedMarble = EnsureMaterial(DamagedMarbleMaterialPath, new Color(0.68f, 0.66f, 0.66f, 1f), new Color(0.08f, 0.055f, 0.045f, 1f));
            GameObject visualsRoot = CreateChild(root, SanctuaryVisualsName, Vector3.zero, Quaternion.identity, Vector3.one);

            if (EnableInvasionFireVisuals)
            {
                CreateInvasionFireAndSmoke(visualsRoot.transform);
            }

            CreateInvasionRubble(visualsRoot.transform, scorchedStone, damagedMarble, goldGlow);
            CreateInvasionCrackDecals(visualsRoot.transform);
        }

        private static void CreateInvasionCrackDecals(Transform parent)
        {
            Material crackDecal = EnsureInvasionCrackDecalMaterial();
            Material crackOverlay = EnsureInvasionCrackOverlayMaterial();
            Transform decalsRoot = CreateChild(parent, "OfficialDecal_CrackScorchProjectors", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            Transform overlaysRoot = CreateChild(parent, "VisibleCrackScorchOverlays", Vector3.zero, Quaternion.identity, Vector3.one).transform;

            CreateInvasionCrackMark(decalsRoot, overlaysRoot, crackDecal, crackOverlay, "LeftOuterNear", new Vector3(-4.25f, 0.72f, -3.18f), 18f, new Vector2(3.25f, 2.05f), 0.98f);
            CreateInvasionCrackMark(decalsRoot, overlaysRoot, crackDecal, crackOverlay, "RightOuterNear", new Vector3(-2.25f, 0.72f, 3.16f), -22f, new Vector2(2.9f, 1.85f), 0.95f);
            CreateInvasionCrackMark(decalsRoot, overlaysRoot, crackDecal, crackOverlay, "LeftOuterMid", new Vector3(2.85f, 0.72f, -3.28f), -9f, new Vector2(3.55f, 2.05f), 0.98f);
            CreateInvasionCrackMark(decalsRoot, overlaysRoot, crackDecal, crackOverlay, "RightOuterBack", new Vector3(8.95f, 0.72f, 3.12f), 26f, new Vector2(3.85f, 2.25f), 0.98f);
            CreateInvasionCrackMark(decalsRoot, overlaysRoot, crackDecal, crackOverlay, "RearBackgroundFloor", new Vector3(13.7f, 0.72f, -2.72f), -16f, new Vector2(4.3f, 2.5f), 0.92f);
        }

        private static void CreateInvasionCrackMark(Transform decalsParent, Transform overlaysParent, Material decalMaterial, Material overlayMaterial, string suffix, Vector3 position, float yawDegrees, Vector2 size, float fade)
        {
            CreateInvasionCrackDecal(decalsParent, decalMaterial, "Decal_Crack_" + suffix, position + Vector3.up * 1.15f, yawDegrees, size, fade);
            CreateInvasionCrackOverlay(overlaysParent, overlayMaterial, "Overlay_Crack_" + suffix, position + Vector3.up * 0.08f, yawDegrees, size, fade);
        }

        private static void CreateInvasionCrackDecal(Transform parent, Material material, string name, Vector3 position, float yawDegrees, Vector2 size, float fade)
        {
            Quaternion floorProjection = Quaternion.Euler(90f, yawDegrees, 0f);
            GameObject decalObject = CreateChild(parent, name, position, floorProjection, Vector3.one);
            DecalProjector projector = decalObject.AddComponent<DecalProjector>();
            projector.material = material;
            projector.drawDistance = 80f;
            projector.fadeScale = 0.95f;
            projector.startAngleFade = 180f;
            projector.endAngleFade = 180f;
            projector.size = new Vector3(size.x, size.y, 3.2f);
            projector.pivot = new Vector3(0f, 0f, 1.6f);
            projector.fadeFactor = fade;
            EditorUtility.SetDirty(projector);
        }

        private static void CreateInvasionCrackOverlay(Transform parent, Material material, string name, Vector3 position, float yawDegrees, Vector2 size, float fade)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, worldPositionStays: false);
            quad.transform.localPosition = position;
            quad.transform.localRotation = Quaternion.Euler(90f, yawDegrees, 0f);
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            AssignMaterial(quad, material);
            MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sortingOrder = 12;
                EditorUtility.SetDirty(renderer);
            }
            DestroyCollider(quad);
            EditorUtility.SetDirty(quad);
        }
        private static void ConfigureSanctuaryVisuals(Transform root)
        {
            Material blueGlow = EnsureMaterial(BlueGlowMaterialPath, new Color(0.09f, 0.38f, 1f, 1f), new Color(0.32f, 0.9f, 3.55f, 1f));
            Material goldGlow = EnsureMaterial(GoldGlowMaterialPath, new Color(1f, 0.68f, 0.26f, 1f), new Color(2.35f, 1.36f, 0.36f, 1f));
            Material whiteGlow = EnsureMaterial(WhiteGlowMaterialPath, new Color(0.9f, 0.97f, 1f, 1f), new Color(1.25f, 1.58f, 2.18f, 1f));
            Material billboardBlue = EnsureBillboardMaterial(BillboardBlueMaterialPath, new Color(0.06f, 0.3f, 0.95f, 0.45f), new Color(0.58f, 0.82f, 1.45f, 1f), 0f, 0.78f);
            Material billboardWhite = EnsureBillboardMaterial(BillboardWhiteMaterialPath, new Color(0.82f, 0.94f, 1f, 0.42f), new Color(1.25f, 1.56f, 2.1f, 1f), 0f, 0.72f);
            Material billboardGold = EnsureBillboardMaterial(BillboardGoldMaterialPath, new Color(1f, 0.58f, 0.16f, 0.5f), new Color(1.22f, 0.78f, 0.28f, 1f), 2f, 0.56f);
            Material billboardBlueCore = EnsureBillboardMaterial(BillboardBlueCoreMaterialPath, new Color(0.16f, 0.62f, 1f, 0.76f), new Color(0.74f, 0.9f, 1.48f, 1f), 2f, 0.92f);
            Material billboardFloor = EnsureBillboardMaterial(BillboardFloorMaterialPath, new Color(0.06f, 0.34f, 0.95f, 0.32f), new Color(0.46f, 0.7f, 1.08f, 1f), 3f, 0.58f);
            GameObject visualsRoot = CreateChild(root, SanctuaryVisualsName, Vector3.zero, Quaternion.identity, Vector3.one);

            Vector3 gateCenter = new Vector3(14.8f, 2.65f, 0f);
            CreateCelestialGate(visualsRoot.transform, blueGlow, goldGlow, whiteGlow, gateCenter);
            CreateCelestialDepthBackdrop(visualsRoot.transform, blueGlow, goldGlow, whiteGlow, gateCenter);
            CreatePromotedShapesFxGate(visualsRoot.transform, gateCenter, blueGlow, goldGlow, whiteGlow);
            CreateRiftStarburst(visualsRoot.transform, blueGlow, gateCenter);
            CreateFloorCracks(visualsRoot.transform, blueGlow, goldGlow);
            CreateCelestialLaneAccents(visualsRoot.transform, whiteGlow, goldGlow);
            CreateReadableCombatBillboards(visualsRoot.transform, root, billboardWhite, billboardBlue, billboardGold, billboardBlueCore, billboardFloor);
        }


        private static void CreateCelestialGate(Transform parent, Material blueGlow, Material goldGlow, Material whiteGlow, Vector3 center)
        {
            CreateCircleLine(parent, "GateHalo_Outer", blueGlow, center, 4.65f, 0.082f, 160);
            CreateCircleLine(parent, "GateHalo_Middle", goldGlow, center + new Vector3(-0.04f, 0f, 0f), 3.28f, 0.048f, 144);
            CreateCircleLine(parent, "GateHalo_Core", blueGlow, center + new Vector3(-0.08f, 0f, 0f), 1.38f, 0.05f, 112);
            CreateCircleLine(parent, "GateHalo_WhiteInner", whiteGlow, center + new Vector3(-0.12f, 0f, 0f), 2.38f, 0.05f, 144);
            CreateLine(parent, "GateSpire_Vertical", blueGlow, 0.06f, new[]
            {
                center + new Vector3(0f, -2.95f, 0f), center + new Vector3(0f, -1.2f, 0f), center,
                center + new Vector3(0f, 1.8f, 0f), center + new Vector3(0f, 4.65f, 0f)
            });
            CreateLine(parent, "GateCrown_Gold", goldGlow, 0.06f, new[]
            {
                center + new Vector3(0f, 1.72f, -1.35f), center + new Vector3(0f, 2.22f, -0.48f), center + new Vector3(0f, 2.48f, 0f),
                center + new Vector3(0f, 2.22f, 0.48f), center + new Vector3(0f, 1.72f, 1.35f)
            });
            CreateLine(parent, "GateFlare_WhiteHorizontal", whiteGlow, 0.042f, new[] { center + new Vector3(-0.16f, 0f, -1.65f), center, center + new Vector3(-0.16f, 0f, 1.65f) });
            CreateLine(parent, "GateFlare_WhiteDiagonalA", whiteGlow, 0.03f, new[] { center + new Vector3(-0.14f, -1f, -1.05f), center, center + new Vector3(-0.14f, 1f, 1.05f) });
            CreateLine(parent, "GateFlare_WhiteDiagonalB", whiteGlow, 0.03f, new[] { center + new Vector3(-0.14f, -1f, 1.05f), center, center + new Vector3(-0.14f, 1f, -1.05f) });
            CreateLine(parent, "GateWing_Left", blueGlow, 0.04f, new[]
            {
                center + new Vector3(-0.1f, 0.35f, -2.65f), center + new Vector3(-0.15f, 1.35f, -3.35f), center + new Vector3(-0.18f, 2.35f, -4.05f)
            });
            CreateLine(parent, "GateWing_Right", blueGlow, 0.04f, new[]
            {
                center + new Vector3(-0.1f, 0.35f, 2.65f), center + new Vector3(-0.15f, 1.35f, 3.35f), center + new Vector3(-0.18f, 2.35f, 4.05f)
            });
            CreateLine(parent, "GateFloorConvergence_Left", goldGlow, 0.085f, new[] { new Vector3(-6f, 0.19f, -2.25f), new Vector3(4.5f, 0.19f, -1.35f), center + new Vector3(0f, -2.25f, -0.45f) });
            CreateLine(parent, "GateFloorConvergence_Right", goldGlow, 0.085f, new[] { new Vector3(-6f, 0.19f, 2.25f), new Vector3(4.5f, 0.19f, 1.35f), center + new Vector3(0f, -2.25f, 0.45f) });
        }

        private static void CreateCelestialDepthBackdrop(Transform parent, Material blueGlow, Material goldGlow, Material whiteGlow, Vector3 gateCenter)
        {
            Vector3 farCenter = gateCenter + new Vector3(3.4f, 0.5f, 0f);
            CreateCircleLine(parent, "Backdrop_Halo_FarBlue", blueGlow, farCenter, 3.1f, 0.028f, 160);
            CreateCircleLine(parent, "Backdrop_Halo_FarGold", goldGlow, farCenter + new Vector3(-0.08f, 0.1f, 0f), 2.32f, 0.026f, 144);
            CreateLine(parent, "Backdrop_Arch_WhiteCrown", whiteGlow, 0.038f, new[]
            {
                farCenter + new Vector3(-0.25f, -1.15f, -4.05f), farCenter + new Vector3(-0.1f, 1.3f, -3.05f),
                farCenter + new Vector3(0f, 2.85f, -1.05f), farCenter + new Vector3(0.06f, 3.22f, 0f),
                farCenter + new Vector3(0f, 2.85f, 1.05f), farCenter + new Vector3(-0.1f, 1.3f, 3.05f),
                farCenter + new Vector3(-0.25f, -1.15f, 4.05f)
            });
            CreateLine(parent, "Backdrop_Spire_Center", whiteGlow, 0.06f, new[]
            {
                farCenter + new Vector3(0.1f, -1.45f, 0f), farCenter + new Vector3(0.05f, 0.2f, 0f),
                farCenter + new Vector3(0f, 2.45f, 0f), farCenter + new Vector3(0.1f, 4.25f, 0f)
            });
            CreateLine(parent, "Backdrop_Spire_LeftA", blueGlow, 0.045f, new[] { farCenter + new Vector3(0f, -1.1f, -1.28f), farCenter + new Vector3(0.08f, 1.15f, -1.28f), farCenter + new Vector3(0.16f, 3.25f, -1.28f) });
            CreateLine(parent, "Backdrop_Spire_RightA", blueGlow, 0.045f, new[] { farCenter + new Vector3(0f, -1.1f, 1.28f), farCenter + new Vector3(0.08f, 1.15f, 1.28f), farCenter + new Vector3(0.16f, 3.25f, 1.28f) });
            CreateLine(parent, "Backdrop_Spire_LeftB", goldGlow, 0.035f, new[] { farCenter + new Vector3(-0.12f, -0.75f, -2.45f), farCenter + new Vector3(0f, 0.95f, -2.45f), farCenter + new Vector3(0.14f, 2.55f, -2.45f) });
            CreateLine(parent, "Backdrop_Spire_RightB", goldGlow, 0.035f, new[] { farCenter + new Vector3(-0.12f, -0.75f, 2.45f), farCenter + new Vector3(0f, 0.95f, 2.45f), farCenter + new Vector3(0.14f, 2.55f, 2.45f) });
            CreateLine(parent, "Backdrop_FloorMirror_Left", whiteGlow, 0.028f, new[] { new Vector3(2.2f, 0.235f, -2.8f), new Vector3(9.5f, 0.225f, -2.25f), farCenter + new Vector3(-0.35f, -2.25f, -0.95f) });
            CreateLine(parent, "Backdrop_FloorMirror_Right", whiteGlow, 0.028f, new[] { new Vector3(2.2f, 0.235f, 2.8f), new Vector3(9.5f, 0.225f, 2.25f), farCenter + new Vector3(-0.35f, -2.25f, 0.95f) });
            CreateLine(parent, "SkyConstellation_Left", goldGlow, 0.026f, new[]
            {
                new Vector3(5.5f, 4.05f, -4.55f), new Vector3(8.2f, 4.85f, -3.95f), new Vector3(11.7f, 4.5f, -3.45f),
                new Vector3(14.4f, 5.35f, -2.55f), new Vector3(17.4f, 4.95f, -1.35f)
            });
            CreateLine(parent, "SkyConstellation_Right", whiteGlow, 0.024f, new[]
            {
                new Vector3(4.8f, 4.35f, 4.35f), new Vector3(7.7f, 5.1f, 3.6f), new Vector3(10.8f, 4.65f, 3.0f),
                new Vector3(13.6f, 5.55f, 2.25f), new Vector3(16.8f, 5.15f, 1.18f)
            });
            CreateLine(parent, "SideArc_LeftUpperBlue", blueGlow, 0.022f, new[] { new Vector3(8.4f, 3.15f, -2.95f), new Vector3(11.4f, 3.55f, -2.62f), new Vector3(14.6f, 3.72f, -2.15f) });
            CreateLine(parent, "SideArc_RightUpperBlue", blueGlow, 0.022f, new[] { new Vector3(8.4f, 3.15f, 2.95f), new Vector3(11.4f, 3.55f, 2.62f), new Vector3(14.6f, 3.72f, 2.15f) });
            CreateLine(parent, "SideArc_LeftGoldNeedle", goldGlow, 0.018f, new[] { new Vector3(7.8f, 2.05f, -2.55f), new Vector3(10.8f, 2.55f, -2.08f), new Vector3(13.8f, 2.9f, -1.42f) });
            CreateLine(parent, "SideArc_RightGoldNeedle", goldGlow, 0.018f, new[] { new Vector3(7.8f, 2.05f, 2.55f), new Vector3(10.8f, 2.55f, 2.08f), new Vector3(13.8f, 2.9f, 1.42f) });
        }

        private static void CreatePromotedShapesFxGate(Transform parent, Vector3 gateCenter, Material blueGlow, Material goldGlow, Material whiteGlow)
        {
            Mesh sphere = LoadMesh(ShapesFxGeometryRoot + "/Geo_Sphere_Hi.fbx");
            Mesh torus = LoadMesh(ShapesFxGeometryRoot + "/Geo_Torus_Hex_Hi.fbx");
            Mesh icosa = LoadMesh(ShapesFxGeometryRoot + "/Geo_Icosahedron_Hex.fbx");
            Mesh dodeca = LoadMesh(ShapesFxGeometryRoot + "/Geo_Dodecahedron.fbx");

            Material portalOuter = RequireAsset<Material>(ShapesFxMaterialRoot + "/DB_ShapesFX_PortalOuter.mat");
            Material portalInner = RequireAsset<Material>(ShapesFxMaterialRoot + "/DB_ShapesFX_PortalInner.mat");
            Material blueSphere = RequireAsset<Material>(ShapesFxMaterialRoot + "/DB_ShapesFX_BlueSphere.mat");
            Material cyanDodeca = RequireAsset<Material>(ShapesFxMaterialRoot + "/DB_ShapesFX_CyanDodeca.mat");
            Material violetIcosa = RequireAsset<Material>(ShapesFxMaterialRoot + "/DB_ShapesFX_VioletIcosa.mat");
            Material deepCube = RequireAsset<Material>(ShapesFxMaterialRoot + "/DB_ShapesFX_DeepCube.mat");

            Transform shapesRoot = CreateChild(parent, "ShapesFX_CelestialGateComposition", Vector3.zero, Quaternion.identity, Vector3.one).transform;
            CreateShapeFxObject(shapesRoot, "ShapesFX_GateOuterRing", torus, portalOuter, gateCenter + new Vector3(-0.3f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 5.15f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_GateTiltRing_A", torus, portalInner, gateCenter + new Vector3(-0.38f, 0.02f, 0f), Quaternion.Euler(18f, 90f, 35f), Vector3.one * 3.65f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_GateTiltRing_B", torus, portalInner, gateCenter + new Vector3(-0.44f, -0.03f, 0f), Quaternion.Euler(-14f, 90f, -28f), Vector3.one * 2.46f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_GateCoreOrb", sphere, blueSphere, gateCenter + new Vector3(-0.7f, 0f, 0f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 0.68f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_LeftFloatingShard", icosa, violetIcosa, gateCenter + new Vector3(-1.02f, 1.25f, -1.55f), Quaternion.Euler(18f, 38f, 11f), Vector3.one * 0.36f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_RightFloatingShard", dodeca, cyanDodeca, gateCenter + new Vector3(-0.96f, 1.18f, 1.52f), Quaternion.Euler(-16f, -34f, 9f), Vector3.one * 0.34f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_UpperDeepShard", icosa, deepCube, gateCenter + new Vector3(-0.82f, 2.25f, 0.2f), Quaternion.Euler(28f, 12f, 40f), Vector3.one * 0.28f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_InvasionShard_Left", icosa, deepCube, gateCenter + new Vector3(-0.62f, 1.78f, -1.05f), Quaternion.Euler(42f, -24f, 58f), Vector3.one * 0.46f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_InvasionShard_Right", dodeca, deepCube, gateCenter + new Vector3(-0.58f, 1.52f, 1.08f), Quaternion.Euler(-36f, 28f, -46f), Vector3.one * 0.42f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_InvasionShard_Lower", icosa, deepCube, gateCenter + new Vector3(-0.72f, -1.28f, 0.18f), Quaternion.Euler(18f, 72f, 34f), Vector3.one * 0.38f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_ReadableBlueRingShell", torus, blueGlow, gateCenter + new Vector3(-0.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 5.55f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_ReadableGoldInnerShell", torus, goldGlow, gateCenter + new Vector3(-0.54f, 0f, 0f), Quaternion.Euler(16f, 90f, -24f), Vector3.one * 2.82f);
            CreateShapeFxObject(shapesRoot, "ShapesFX_ReadableWhiteCoreShard", dodeca, whiteGlow, gateCenter + new Vector3(-0.58f, 0f, 0f), Quaternion.Euler(0f, 45f, 0f), Vector3.one * 0.32f);
        }

        private static GameObject CreateShapeFxObject(Transform parent, string name, Mesh mesh, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
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
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            EditorUtility.SetDirty(renderer);
            return target;
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

            throw new InvalidOperationException($"Missing promoted ShapesFX mesh at {path}");
        }
        private static void CreateCelestialLaneAccents(Transform parent, Material whiteGlow, Material goldGlow)
        {
            CreateLine(parent, "LaneWhite_LeftRim", whiteGlow, 0.075f, new[] { new Vector3(-11f, 0.36f, -2.38f), new Vector3(-1f, 0.34f, -2.22f), new Vector3(9f, 0.31f, -1.85f), new Vector3(18f, 0.28f, -1.25f) });
            CreateLine(parent, "LaneWhite_RightRim", whiteGlow, 0.075f, new[] { new Vector3(-11f, 0.36f, 2.38f), new Vector3(-1f, 0.34f, 2.22f), new Vector3(9f, 0.31f, 1.85f), new Vector3(18f, 0.28f, 1.25f) });
            CreateLine(parent, "LaneGold_LeftInner", goldGlow, 0.09f, new[] { new Vector3(-10.5f, 0.22f, -1.58f), new Vector3(1.5f, 0.21f, -1.16f), new Vector3(14.8f, 0.2f, -0.42f) });
            CreateLine(parent, "LaneGold_RightInner", goldGlow, 0.09f, new[] { new Vector3(-10.5f, 0.22f, 1.58f), new Vector3(1.5f, 0.21f, 1.16f), new Vector3(14.8f, 0.2f, 0.42f) });
            CreateFloorCircleLine(parent, "FloorSigil_OuterGold", goldGlow, new Vector3(5.2f, 0.205f, 0f), 1.7f, 0.06f, 96);
            CreateFloorCircleLine(parent, "FloorSigil_InnerWhite", whiteGlow, new Vector3(5.2f, 0.21f, 0f), 0.92f, 0.045f, 80);
            CreateLine(parent, "FloorSigil_CrossWhite", whiteGlow, 0.045f, new[] { new Vector3(3.55f, 0.215f, 0f), new Vector3(5.2f, 0.215f, 0f), new Vector3(6.85f, 0.215f, 0f) });
            CreateLine(parent, "FloorSigil_CrossGold", goldGlow, 0.045f, new[] { new Vector3(5.2f, 0.216f, -1.45f), new Vector3(5.2f, 0.216f, 0f), new Vector3(5.2f, 0.216f, 1.45f) });
        }

        private static void CreateFloorCircleLine(Transform parent, string name, Material material, Vector3 center, float radius, float width, int segments)
        {
            Vector3[] positions = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                positions[i] = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            CreateLine(parent, name, material, width, positions);
        }
        private static void CreateReadableCombatBillboards(Transform parent, Transform lookdevRoot, Material heavenMaterial, Material riftMaterial, Material goldCoreMaterial, Material blueCoreMaterial, Material floorMaterial)
        {
            Transform cameraTransform = RequireChild(lookdevRoot, CameraName);
            CreateCameraBillboard(parent, cameraTransform, "Billboard_HeavenGateBloom", heavenMaterial, 32.2f, 0f, 0.58f, new Vector2(12.2f, 7.35f));
            CreateCameraBillboard(parent, cameraTransform, "Billboard_RiftGlow", riftMaterial, 31.9f, 0f, 0.42f, new Vector2(8.25f, 5.2f));
            CreateCameraBillboard(parent, cameraTransform, "Billboard_RiftCore", goldCoreMaterial, 31.8f, 0f, 0.3f, new Vector2(2.05f, 2.05f));
            CreateCameraBillboard(parent, cameraTransform, "Billboard_BlueSanctuaryCore", blueCoreMaterial, 27.6f, 0f, -0.38f, new Vector2(0.58f, 0.58f));
            CreateCameraBillboard(parent, cameraTransform, "Billboard_FloorRiftRead", floorMaterial, 18.5f, 0f, -1.9f, new Vector2(7.4f, 0.34f));
        }

        private static void CreateCameraBillboard(Transform parent, Transform cameraTransform, string name, Material material, float forward, float right, float up, Vector2 size)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, worldPositionStays: true);
            quad.transform.position = cameraTransform.position + cameraTransform.forward * forward + cameraTransform.right * right + cameraTransform.up * up;
            quad.transform.rotation = cameraTransform.rotation * Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            AssignMaterial(quad, material);
            DestroyCollider(quad);
            EditorUtility.SetDirty(quad);
        }

        private static void CreateInvasionFireAndSmoke(Transform parent)
        {
            parent.gameObject.SetActive(true);
            Transform vfxRoot = CreateChild(parent, PromotedUniFireSmokeVfxRootName, Vector3.zero, Quaternion.identity, Vector3.one).transform;
            vfxRoot.gameObject.SetActive(true);

            CreatePromotedVfxInstance(vfxRoot, UniSmallFirePrefabPath, "UNI_SmallFire_LeftCollapsedSlab", new Vector3(-4.2f, 0.46f, -2.06f), Quaternion.identity, Vector3.one * 1.65f);
            CreatePromotedVfxInstance(vfxRoot, UniSmallFirePrefabPath, "UNI_SmallFire_LeftBrokenCapstone", new Vector3(0.8f, 0.48f, -1.72f), Quaternion.identity, Vector3.one * 1.35f);
            CreatePromotedVfxInstance(vfxRoot, UniGroundFirePrefabPath, "UNI_GroundFire_RightCollapsedSlab", new Vector3(1.55f, 0.42f, 1.94f), Quaternion.identity, Vector3.one * 1.48f);
            CreatePromotedVfxInstance(vfxRoot, UniDeviceFirePrefabPath, "UNI_DeviceFire_RightBrokenRail", new Vector3(6.6f, 0.7f, 2.08f), Quaternion.identity, Vector3.one * 1.18f);
            CreatePromotedVfxInstance(vfxRoot, UniGroundFirePrefabPath, "UNI_GroundFire_RightBackShard", new Vector3(8.5f, 0.38f, 1.68f), Quaternion.identity, Vector3.one * 1.22f);
            CreatePromotedVfxInstance(vfxRoot, UniLongSmokePrefabPath, "UNI_LongSmoke_RightBrokenRail", new Vector3(6.6f, 0.8f, 2.08f), Quaternion.identity, Vector3.one * 0.62f);
            CreatePromotedVfxInstance(vfxRoot, UniLongSmokePrefabPath, "UNI_LongSmoke_RightBackShard", new Vector3(8.5f, 0.58f, 1.68f), Quaternion.identity, Vector3.one * 0.56f);

            CreateInvasionFireLight(vfxRoot, "FireGlow_LeftCollapsedSlab", new Vector3(-4.2f, 0.78f, -2.06f), 0.55f, 2.9f);
            CreateInvasionFireLight(vfxRoot, "FireGlow_LeftBrokenCapstone", new Vector3(0.8f, 0.75f, -1.72f), 0.45f, 2.6f);
            CreateInvasionFireLight(vfxRoot, "FireGlow_RightCollapsedSlab", new Vector3(1.55f, 0.72f, 1.94f), 0.45f, 2.6f);
            CreateInvasionFireLight(vfxRoot, "FireGlow_RightBrokenRail", new Vector3(6.6f, 1.02f, 2.08f), 0.38f, 2.5f);
        }

        private static void CreateInvasionFireLight(Transform parent, string name, Vector3 localPosition, float intensity, float range)
        {
            Light light = CreateLight(parent, name, localPosition);
            light.type = LightType.Point;
            light.color = new Color(1f, 0.34f, 0.08f, 1f);
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            light.bounceIntensity = 0f;
            EditorUtility.SetDirty(light);
        }

        private static GameObject CreatePromotedVfxInstance(Transform parent, string prefabPath, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject sourcePrefab = RequireAsset<GameObject>(prefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab, parent.gameObject.scene) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(sourcePrefab);
            }

            instance.name = name;
            instance.transform.SetParent(parent, worldPositionStays: false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            instance.SetActive(true);

            foreach (Transform child in instance.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                child.gameObject.SetActive(true);
            }

            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                renderer.enabled = true;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                EditorUtility.SetDirty(renderer);
            }

            foreach (VisualEffect effect in instance.GetComponentsInChildren<VisualEffect>(includeInactive: true))
            {
                effect.enabled = true;
                effect.Reinit();
                effect.SendEvent("OnPlay");
                effect.Play();
                effect.Simulate(1f / 30f, 90u);
                EditorUtility.SetDirty(effect);
            }

            foreach (ParticleSystem particleSystem in instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true))
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Clear(withChildren: true);
                particleSystem.Play(withChildren: true);
                particleSystem.Simulate(1.8f, withChildren: true, restart: true, fixedTimeStep: true);
                EditorUtility.SetDirty(particleSystem);
            }

            foreach (AudioSource audioSource in instance.GetComponentsInChildren<AudioSource>(includeInactive: true))
            {
                audioSource.enabled = false;
                EditorUtility.SetDirty(audioSource);
            }

            EditorUtility.SetDirty(instance);
            return instance;
        }

        private static void SimulateSceneVisualEffects(Scene scene, float seconds)
        {
            uint steps = (uint)Mathf.Max(1, Mathf.CeilToInt(seconds * 30f));
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (VisualEffect effect in root.GetComponentsInChildren<VisualEffect>(includeInactive: true))
                {
                    effect.enabled = true;
                    effect.Reinit();
                    effect.SendEvent("OnPlay");
                    effect.Play();
                    effect.Simulate(1f / 30f, steps);
                    EditorUtility.SetDirty(effect);
                }

                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    if (renderer.GetComponent<VisualEffect>() != null)
                    {
                        renderer.enabled = true;
                        EditorUtility.SetDirty(renderer);
                    }
                }

                foreach (ParticleSystem particleSystem in root.GetComponentsInChildren<ParticleSystem>(includeInactive: true))
                {
                    particleSystem.gameObject.SetActive(true);
                    particleSystem.Clear(withChildren: true);
                    particleSystem.Play(withChildren: true);
                    particleSystem.Simulate(seconds, withChildren: true, restart: true, fixedTimeStep: true);
                    EditorUtility.SetDirty(particleSystem);
                }
            }
        }

        private static void CreateInvasionRubble(Transform parent, Material scorchedStone, Material damagedMarble, Material goldGlow)
        {
            CreatePrimitiveVisual(parent, "FallenPillar_LeftNear", PrimitiveType.Cylinder, new Vector3(-1.7f, 0.52f, -2.22f), Quaternion.Euler(0f, 0f, 78f), new Vector3(0.28f, 1.45f, 0.28f), damagedMarble);
            CreatePrimitiveVisual(parent, "FallenPillar_RightMid", PrimitiveType.Cylinder, new Vector3(4.35f, 0.5f, 2.18f), Quaternion.Euler(0f, 0f, -74f), new Vector3(0.26f, 1.35f, 0.26f), damagedMarble);
            CreatePrimitiveVisual(parent, "CollapsedMarbleSlab_LeftNear", PrimitiveType.Cube, new Vector3(-4.2f, 0.28f, -2.06f), Quaternion.Euler(5f, 26f, -11f), new Vector3(1.15f, 0.18f, 0.48f), damagedMarble);
            CreatePrimitiveVisual(parent, "CollapsedMarbleSlab_RightMid", PrimitiveType.Cube, new Vector3(1.55f, 0.28f, 1.94f), Quaternion.Euler(-8f, -28f, 9f), new Vector3(1.05f, 0.18f, 0.42f), damagedMarble);
            CreatePrimitiveVisual(parent, "BrokenCapstone_Left", PrimitiveType.Cube, new Vector3(0.8f, 0.3f, -1.72f), Quaternion.Euler(8f, 36f, -12f), new Vector3(0.78f, 0.22f, 0.42f), scorchedStone);
            CreatePrimitiveVisual(parent, "BrokenCapstone_Right", PrimitiveType.Cube, new Vector3(7.8f, 0.31f, 1.48f), Quaternion.Euler(-10f, -28f, 16f), new Vector3(0.86f, 0.2f, 0.38f), scorchedStone);
            CreatePrimitiveVisual(parent, "BrokenGoldRail_LeftFront", PrimitiveType.Cube, new Vector3(-2.8f, 0.72f, -2.36f), Quaternion.Euler(0f, 10f, -24f), new Vector3(1.7f, 0.07f, 0.07f), goldGlow);
            CreatePrimitiveVisual(parent, "BrokenGoldRail_RightGate", PrimitiveType.Cube, new Vector3(6.6f, 0.78f, 2.08f), Quaternion.Euler(0f, -18f, 18f), new Vector3(1.45f, 0.07f, 0.07f), goldGlow);
            CreatePrimitiveVisual(parent, "ScorchedShard_LeftA", PrimitiveType.Cube, new Vector3(3.2f, 0.17f, -1.18f), Quaternion.Euler(18f, 24f, 12f), new Vector3(0.26f, 0.16f, 0.32f), scorchedStone);
            CreatePrimitiveVisual(parent, "ScorchedShard_LeftB", PrimitiveType.Cube, new Vector3(4.1f, 0.19f, -1.42f), Quaternion.Euler(-12f, 48f, -9f), new Vector3(0.32f, 0.18f, 0.22f), scorchedStone);
            CreatePrimitiveVisual(parent, "ScorchedShard_RightA", PrimitiveType.Cube, new Vector3(6.6f, 0.17f, 1.14f), Quaternion.Euler(11f, -40f, 6f), new Vector3(0.24f, 0.14f, 0.34f), scorchedStone);
            CreatePrimitiveVisual(parent, "ScorchedShard_RightB", PrimitiveType.Cube, new Vector3(8.5f, 0.21f, 1.68f), Quaternion.Euler(-18f, 18f, 22f), new Vector3(0.38f, 0.16f, 0.24f), scorchedStone);
        }
        private static GameObject CreatePrimitiveVisual(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            GameObject target = GameObject.CreatePrimitive(primitiveType);
            target.name = name;
            target.transform.SetParent(parent, worldPositionStays: false);
            target.transform.localPosition = localPosition;
            target.transform.localRotation = localRotation;
            target.transform.localScale = localScale;
            AssignMaterial(target, material);
            DestroyCollider(target);

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                EditorUtility.SetDirty(renderer);
            }

            EditorUtility.SetDirty(target);
            return target;
        }
        private static void ConfigureCombatAnchors(Transform root)
        {
            GameObject anchorsRoot = CreateChild(root, CombatAnchorsName, Vector3.zero, Quaternion.identity, Vector3.one);
            CreateChild(anchorsRoot.transform, "Player_LeftShoulderCameraAnchor", new Vector3(-11f, 1.2f, -3.1f), Quaternion.Euler(0f, 82f, 0f), Vector3.one);
            CreateChild(anchorsRoot.transform, "Boss_CenterLaneAnchor", new Vector3(10.2f, 0f, 0f), Quaternion.identity, Vector3.one);
            CreateChild(anchorsRoot.transform, "Add_LeftLaneAnchor", new Vector3(8.9f, 0f, -1.25f), Quaternion.identity, Vector3.one);
            CreateChild(anchorsRoot.transform, "Add_RightLaneAnchor", new Vector3(8.9f, 0f, 1.25f), Quaternion.identity, Vector3.one);
            CreateChild(anchorsRoot.transform, "Rift_BackdropAnchor", new Vector3(14.8f, 2.65f, 0f), Quaternion.identity, Vector3.one);
        }

        private static void CreateRiftStarburst(Transform parent, Material material, Vector3 center)
        {
            CreateLine(parent, "RiftBolt_Vertical", material, 0.052f, new[]
            {
                center + new Vector3(0f, -3.55f, 0f), center + new Vector3(0f, -1.2f, 0.18f), center,
                center + new Vector3(0f, 1.45f, -0.18f), center + new Vector3(0f, 3.35f, 0.08f)
            });
            CreateLine(parent, "RiftBolt_LeftFork", material, 0.036f, new[]
            {
                center, center + new Vector3(-0.95f, 0.8f, -0.9f), center + new Vector3(-1.8f, 1.55f, -1.55f), center + new Vector3(-2.35f, 2.1f, -2.05f)
            });
            CreateLine(parent, "RiftBolt_RightFork", material, 0.036f, new[]
            {
                center, center + new Vector3(-0.9f, 0.65f, 0.85f), center + new Vector3(-1.75f, 1.35f, 1.65f), center + new Vector3(-2.4f, 1.95f, 2.2f)
            });
            CreateLine(parent, "RiftBolt_FloorFork", material, 0.028f, new[]
            {
                center + new Vector3(-0.2f, -0.2f, 0f), center + new Vector3(-1.5f, -1.3f, -0.95f), center + new Vector3(-2.7f, -2.2f, -1.8f)
            });
            CreateLine(parent, "RiftBolt_UpperArc", material, 0.026f, new[]
            {
                center + new Vector3(-3.9f, 3.2f, -3.6f), center + new Vector3(-2.2f, 3.65f, -1.5f), center + new Vector3(-0.4f, 3.95f, 0.5f), center + new Vector3(-2.1f, 3.55f, 2.6f)
            });
        }

        private static void CreateInvadingHeavenBreach(Transform parent, Material invasionGlow, Material blueGlow, Material goldGlow, Vector3 center)
        {
            CreateLine(parent, "InvasionBreach_LeftClaw", invasionGlow, 0.066f, new[]
            {
                center + new Vector3(-0.34f, 2.28f, -1.92f), center + new Vector3(-0.44f, 1.45f, -1.18f),
                center + new Vector3(-0.54f, 0.62f, -0.46f), center + new Vector3(-0.64f, -0.12f, -0.12f)
            });
            CreateLine(parent, "InvasionBreach_RightClaw", invasionGlow, 0.066f, new[]
            {
                center + new Vector3(-0.34f, 2.08f, 1.82f), center + new Vector3(-0.48f, 1.22f, 1.08f),
                center + new Vector3(-0.58f, 0.42f, 0.42f), center + new Vector3(-0.66f, -0.18f, 0.08f)
            });
            CreateLine(parent, "InvasionBreach_CeilingTear", invasionGlow, 0.052f, new[]
            {
                center + new Vector3(-2.3f, 3.38f, -0.98f), center + new Vector3(-1.28f, 3.08f, -0.34f),
                center + new Vector3(-0.36f, 2.72f, 0.04f), center + new Vector3(-1.2f, 3f, 0.7f),
                center + new Vector3(-2.05f, 3.22f, 1.18f)
            });
            CreateLine(parent, "InvasionBreach_SealBreakGold", goldGlow, 0.036f, new[]
            {
                center + new Vector3(-1.8f, -1.88f, -0.85f), center + new Vector3(-0.95f, -1.52f, -0.34f),
                center + new Vector3(-0.25f, -1.24f, 0.08f), center + new Vector3(-0.92f, -1.5f, 0.42f),
                center + new Vector3(-1.72f, -1.82f, 0.92f)
            });
            CreateLine(parent, "InvasionBreach_BlueCounterSeal", blueGlow, 0.032f, new[]
            {
                center + new Vector3(-2.25f, 0.88f, -1.38f), center + new Vector3(-1.48f, 0.72f, -0.62f),
                center + new Vector3(-0.7f, 0.48f, 0f), center + new Vector3(-1.46f, 0.72f, 0.66f),
                center + new Vector3(-2.18f, 0.92f, 1.42f)
            });
        }
        private static void CreateFloorCracks(Transform parent, Material blueGlow, Material goldGlow)
        {
            CreateLine(parent, "FloorRift_CenterSpine", blueGlow, 0.07f, new[]
            {
                new Vector3(-8f, 0.16f, 0.05f), new Vector3(-3.2f, 0.16f, -0.18f), new Vector3(2.5f, 0.16f, 0.18f),
                new Vector3(8.5f, 0.16f, -0.12f), new Vector3(16.5f, 0.16f, 0.05f)
            });
            CreateLine(parent, "FloorRift_LeftBranch", blueGlow, 0.045f, new[]
            {
                new Vector3(3f, 0.17f, 0f), new Vector3(5.8f, 0.17f, -1.2f), new Vector3(8.1f, 0.17f, -2.5f), new Vector3(11.8f, 0.17f, -3.25f)
            });
            CreateLine(parent, "FloorRift_RightBranch", blueGlow, 0.045f, new[]
            {
                new Vector3(5f, 0.17f, 0.05f), new Vector3(7.1f, 0.17f, 1.35f), new Vector3(10.6f, 0.17f, 2.55f), new Vector3(14.2f, 0.17f, 3.1f)
            });
            CreateLine(parent, "FloorGold_LeftGuide", goldGlow, 0.078f, new[] { new Vector3(-11f, 0.18f, -1.9f), new Vector3(18f, 0.18f, -1.9f) });
            CreateLine(parent, "FloorGold_RightGuide", goldGlow, 0.078f, new[] { new Vector3(-11f, 0.18f, 1.9f), new Vector3(18f, 0.18f, 1.9f) });
        }

        private static void CreateEnemyProxySet(Transform parent, Material bodyMaterial, Material eyeMaterial, Material outlineMaterial)
        {
            CreateEnemyProxy(parent, "BossSilhouette_Center", bodyMaterial, eyeMaterial, outlineMaterial, new Vector3(12.35f, 0.86f, 0f), 0.74f);
            CreateEnemyProxy(parent, "AddSilhouette_Left", bodyMaterial, eyeMaterial, outlineMaterial, new Vector3(11.65f, 0.66f, -1.62f), 0.44f);
            CreateEnemyProxy(parent, "AddSilhouette_Right", bodyMaterial, eyeMaterial, outlineMaterial, new Vector3(11.65f, 0.66f, 1.62f), 0.44f);
        }

        private static void CreateEnemyProxy(Transform parent, string name, Material bodyMaterial, Material eyeMaterial, Material outlineMaterial, Vector3 position, float scale)
        {
            GameObject proxy = CreateChild(parent, name, position, Quaternion.identity, Vector3.one * scale);
            GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            outline.name = "ToonOutlineShell";
            outline.transform.SetParent(proxy.transform, worldPositionStays: false);
            outline.transform.localPosition = new Vector3(0f, -0.015f, 0.035f);
            outline.transform.localScale = new Vector3(0.98f, 1.48f, 0.98f);
            AssignMaterial(outline, outlineMaterial);
            DestroyCollider(outline);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(proxy.transform, worldPositionStays: false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.05f, 1.55f, 1.05f);
            AssignMaterial(body, bodyMaterial);
            DestroyCollider(body);

            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "BlueCore";
            eye.transform.SetParent(proxy.transform, worldPositionStays: false);
            eye.transform.localPosition = new Vector3(-0.48f, 0.55f, 0f);
            eye.transform.localScale = new Vector3(0.18f, 0.38f, 0.38f);
            AssignMaterial(eye, eyeMaterial);
            DestroyCollider(eye);
        }

        private static void CreateCircleLine(Transform parent, string name, Material material, Vector3 center, float radius, float width, int segments)
        {
            Vector3[] positions = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                positions[i] = center + new Vector3(0f, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }

            CreateLine(parent, name, material, width, positions);
        }

        private static LineRenderer CreateLine(Transform parent, string name, Material material, float width, Vector3[] positions)
        {
            GameObject lineObject = CreateChild(parent, name, Vector3.zero, Quaternion.identity, Vector3.one);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = positions.Length;
            line.SetPositions(positions);
            line.widthMultiplier = width;
            line.numCapVertices = 4;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = material;
            EditorUtility.SetDirty(line);
            return line;
        }

        private static Material EnsureBillboardMaterial(string path, Color color, Color edgeColor, float shape, float glowPower)
        {
            Shader shader = Shader.Find("DimensionBrawl/Lookdev/ToonBillboardAlways") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            if (material.HasProperty("_EdgeColor"))
            {
                material.SetColor("_EdgeColor", edgeColor);
            }
            if (material.HasProperty("_Shape"))
            {
                material.SetFloat("_Shape", shape);
            }
            if (material.HasProperty("_GlowPower"))
            {
                material.SetFloat("_GlowPower", glowPower);
            }

            material.renderQueue = 4080;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureOlympusCorridorDecalRendererFeature()
        {
            bool changed = EnsureDecalRendererFeature(MobileRendererDataPath);
            changed |= EnsureDecalRendererFeature(PcRendererDataPath);
            if (changed)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static bool EnsureDecalRendererFeature(string rendererDataPath)
        {
            ScriptableRendererData rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(rendererDataPath);
            if (rendererData == null)
            {
                throw new InvalidOperationException($"Missing URP renderer data for decals: {rendererDataPath}");
            }

            bool changed = false;
            if (!rendererData.TryGetRendererFeature(out DecalRendererFeature decalFeature))
            {
                decalFeature = ScriptableObject.CreateInstance<DecalRendererFeature>();
                decalFeature.name = "DecalRendererFeature";
                AssetDatabase.AddObjectToAsset(decalFeature, rendererData);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(decalFeature, out string _, out long localId);

                SerializedObject rendererObject = new SerializedObject(rendererData);
                SerializedProperty features = rendererObject.FindProperty("m_RendererFeatures");
                SerializedProperty featureMap = rendererObject.FindProperty("m_RendererFeatureMap");
                features.arraySize++;
                features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = decalFeature;
                featureMap.arraySize++;
                featureMap.GetArrayElementAtIndex(featureMap.arraySize - 1).longValue = localId;
                rendererObject.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            if (!decalFeature.isActive)
            {
                decalFeature.SetActive(true);
                changed = true;
            }

            SerializedObject featureObject = new SerializedObject(decalFeature);
            SerializedProperty settings = featureObject.FindProperty("m_Settings");
            bool settingsChanged = false;
            if (settings != null)
            {
                settingsChanged |= SetSerializedRelativeInt(settings, "technique", 2);
                settingsChanged |= SetSerializedRelativeFloat(settings, "maxDrawDistance", 80f);
                settingsChanged |= SetSerializedRelativeBool(settings, "decalLayers", false);
                SerializedProperty screenSpace = settings.FindPropertyRelative("screenSpaceSettings");
                if (screenSpace != null)
                {
                    settingsChanged |= SetSerializedRelativeInt(screenSpace, "normalBlend", 0);
                }
            }

            if (settingsChanged)
            {
                featureObject.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            if (changed)
            {
                decalFeature.Create();
                rendererData.SetDirty();
                EditorUtility.SetDirty(decalFeature);
                EditorUtility.SetDirty(rendererData);
            }

            return changed;
        }

        private static bool SetSerializedRelativeInt(SerializedProperty parent, string propertyName, int value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property == null)
            {
                return false;
            }

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                if (property.enumValueIndex == value)
                {
                    return false;
                }

                property.enumValueIndex = value;
                return true;
            }

            if (property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool SetSerializedRelativeFloat(SerializedProperty parent, string propertyName, float value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetSerializedRelativeBool(SerializedProperty parent, string propertyName, bool value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static Material EnsureInvasionCrackDecalMaterial()
        {
            Texture2D crackTexture = EnsureInvasionCrackDecalTexture();
            Shader shader = Shader.Find("Shader Graphs/Decal") ?? Shader.Find("Universal Render Pipeline/Lit");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(InvasionCrackDecalMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, InvasionCrackDecalMaterialPath);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            if (material.HasProperty("Base_Map"))
            {
                material.SetTexture("Base_Map", crackTexture);
            }
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", crackTexture);
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", crackTexture);
            }
            if (material.HasProperty("Base_Color"))
            {
                material.SetColor("Base_Color", new Color(0.12f, 0.075f, 0.045f, 0.76f));
            }
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", new Color(0.12f, 0.075f, 0.045f, 0.76f));
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", new Color(0.045f, 0.032f, 0.026f, 0.76f));
            }
            SetMaterialFloat(material, "_Smoothness", 0f);
            SetMaterialFloat(material, "_Metallic", 0f);
            SetMaterialFloat(material, "_DrawOrder", 18f);
            SetMaterialFloat(material, "_EdgeSharpness", 0.68f);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }


        private static Material EnsureInvasionCrackOverlayMaterial()
        {
            Texture2D crackTexture = EnsureInvasionCrackDecalTexture();
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(InvasionCrackOverlayMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, InvasionCrackOverlayMaterialPath);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", crackTexture);
            }
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", crackTexture);
            }
            Color overlayColor = new Color(0.12f, 0.055f, 0.028f, 0.9f);
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", overlayColor);
            }
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", overlayColor);
            }
            SetMaterialFloat(material, "_Surface", 1f);
            SetMaterialFloat(material, "_Blend", 0f);
            SetMaterialFloat(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            SetMaterialFloat(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            SetMaterialFloat(material, "_ZWrite", 0f);
            SetMaterialFloat(material, "_Cull", 0f);
            material.renderQueue = 4100;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }
        private static Texture2D EnsureInvasionCrackDecalTexture()
        {
            EnsureFolder(DecalTextureRoot);
            const int size = 512;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 0f);
            }

            DrawCrackLine(pixels, size, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.54f), 9.4f, 1f, 1.6f);
            DrawCrackLine(pixels, size, new Vector2(0.36f, 0.51f), new Vector2(0.14f, 0.24f), 6.2f, 0.88f, 2.7f);
            DrawCrackLine(pixels, size, new Vector2(0.46f, 0.51f), new Vector2(0.56f, 0.2f), 5.5f, 0.82f, 3.4f);
            DrawCrackLine(pixels, size, new Vector2(0.58f, 0.53f), new Vector2(0.84f, 0.29f), 6.4f, 0.86f, 4.2f);
            DrawCrackLine(pixels, size, new Vector2(0.62f, 0.54f), new Vector2(0.8f, 0.78f), 4.8f, 0.72f, 5.1f);
            DrawCrackLine(pixels, size, new Vector2(0.28f, 0.49f), new Vector2(0.12f, 0.72f), 4.6f, 0.68f, 6.6f);
            texture.SetPixels(pixels);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            File.WriteAllBytes(InvasionCrackDecalTexturePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(InvasionCrackDecalTexturePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(InvasionCrackDecalTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(InvasionCrackDecalTexturePath);
        }

        private static void DrawCrackLine(Color[] pixels, int textureSize, Vector2 from, Vector2 to, float radius, float alpha, float waveSeed)
        {
            Vector2 start = from * textureSize;
            Vector2 end = to * textureSize;
            Vector2 direction = end - start;
            Vector2 normal = new Vector2(-direction.y, direction.x).normalized;
            int steps = Mathf.Max(8, Mathf.CeilToInt(direction.magnitude * 1.25f));
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                float wiggle = Mathf.Sin((t * 13.7f + waveSeed) * Mathf.PI) * 2.4f + Mathf.Sin((t * 31.1f + waveSeed * 0.37f) * Mathf.PI) * 0.85f;
                Vector2 point = Vector2.Lerp(start, end, t) + normal * wiggle;
                float taperedRadius = radius * Mathf.Lerp(0.62f, 1f, Mathf.Sin(t * Mathf.PI));
                PaintCrackStamp(pixels, textureSize, point, taperedRadius, alpha);
            }
        }

        private static void PaintCrackStamp(Color[] pixels, int textureSize, Vector2 center, float radius, float alpha)
        {
            int minX = Mathf.Clamp(Mathf.FloorToInt(center.x - radius * 2f), 0, textureSize - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt(center.x + radius * 2f), 0, textureSize - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt(center.y - radius * 2f), 0, textureSize - 1);
            int maxY = Mathf.Clamp(Mathf.CeilToInt(center.y + radius * 2f), 0, textureSize - 1);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float distance = Vector2.Distance(center, new Vector2(x + 0.5f, y + 0.5f));
                    float falloff = Mathf.Clamp01(1f - distance / Mathf.Max(0.01f, radius * 1.8f));
                    if (falloff <= 0f)
                    {
                        continue;
                    }

                    int index = y * textureSize + x;
                    float nextAlpha = Mathf.Max(pixels[index].a, alpha * falloff * falloff);
                    pixels[index] = new Color(0.026f, 0.019f, 0.015f, nextAlpha);
                }
            }
        }
        private static Material EnsureMaterial(string path, Color baseColor, Color emissionColor)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", baseColor);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", emissionColor);
            }

            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AssignMaterial(GameObject gameObject, Material material)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void DestroyCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void ConfigureOpenCombatSightline(Scene scene)
        {
            int disabledCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (IsGeneratedLookdevRoot(root.name))
                {
                    continue;
                }

                foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
                {
                    if (ShouldDisableForCombatSightline(renderer))
                    {
                        renderer.enabled = false;
                        disabledCount++;
                        EditorUtility.SetDirty(renderer);
                    }
                }
            }

            Debug.Log($"Olympus corridor lookdev disabled {disabledCount} large overhead renderers for combat sightline.");
        }

        private static bool ShouldDisableForCombatSightline(Renderer renderer)
        {
            Bounds bounds = renderer.bounds;
            bool broadOverheadSlab = bounds.center.y > 1.75f && bounds.size.x > 4.5f && bounds.size.z > 3f;
            bool highCanopy = bounds.center.y > 3.2f && bounds.size.x > 2.5f && bounds.size.z > 1.8f;
            bool centerHangingCloth = bounds.center.y > 1.65f
                && Mathf.Abs(bounds.center.z) < 0.95f
                && bounds.size.y > 1.4f
                && RendererUsesMaterialName(renderer, "cloth");
            bool tallHangingTextile = bounds.center.y > 1.55f
                && bounds.size.y > 1.05f
                && bounds.size.x < 4.2f
                && (RendererUsesMaterialName(renderer, "cloth") || RendererUsesMaterialName(renderer, "flag") || RendererUsesMaterialName(renderer, "wave") || RendererUsesMaterialName(renderer, "banner"));
            return broadOverheadSlab || highCanopy || centerHangingCloth || tallHangingTextile;
        }


        private static bool RendererUsesMaterialName(Renderer renderer, string namePart)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                string materialName = material.name.ToLowerInvariant();
                string materialPath = AssetDatabase.GetAssetPath(material).ToLowerInvariant();
                if (materialName.Contains(namePart) || materialPath.Contains(namePart))
                {
                    return true;
                }
            }

            return false;
        }
        private static void ConfigureSceneAtmosphere(Scene scene)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.8f, 0.84f, 0.98f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.7f, 0.9f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.32f, 0.34f, 0.46f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.5f, 0.62f, 0.86f, 1f);
            RenderSettings.fogDensity = 0.00056f;
            RenderSettings.skybox = EnsurePromotedAllSkySkybox();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Camera camera in root.GetComponentsInChildren<Camera>(includeInactive: true))
                {
                    camera.allowHDR = true;
                    UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                    cameraData.renderPostProcessing = true;
                    EditorUtility.SetDirty(camera);
                    EditorUtility.SetDirty(cameraData);
                }
            }
        }


        private static Material EnsurePromotedAllSkySkybox()
        {
            return EnsurePromotedAllSkySkybox(SkyboxMaterialPath, new Color(0.96f, 0.975f, 1f, 0.86f), 0.94f, 28f);
        }

        private static Material EnsurePromotedAllSkySkybox(string skyboxPath, Color tint, float exposure, float rotation)
        {
            Material sourceSkybox = AssetDatabase.LoadAssetAtPath<Material>(AllSkySourceSkyboxPath);
            Material skybox = AssetDatabase.LoadAssetAtPath<Material>(skyboxPath);
            if (sourceSkybox == null)
            {
                if (skybox == null)
                {
                    throw new InvalidOperationException($"Missing promoted skybox {skyboxPath}; install the local AllSky source pack at {AllSkySourceSkyboxPath} to regenerate it.");
                }

                SetMaterialColor(skybox, "_Tint", tint);
                SetMaterialFloat(skybox, "_Exposure", exposure);
                SetMaterialFloat(skybox, "_Rotation", rotation);
                EditorUtility.SetDirty(skybox);
                return skybox;
            }

            if (skybox == null)
            {
                skybox = new Material(sourceSkybox.shader);
                AssetDatabase.CreateAsset(skybox, skyboxPath);
            }

            EnsureFolder(TextureRoot);
            EnsureFolder(SkyTextureRoot);
            skybox.CopyPropertiesFromMaterial(sourceSkybox);
            HashSet<string> promotedSkyTexturePaths = new HashSet<string>(StringComparer.Ordinal);
            PromoteSkyboxTextures(sourceSkybox, skybox, promotedSkyTexturePaths);
            DeleteUnusedPromotedSkyTextures(promotedSkyTexturePaths);
            skybox.shader = sourceSkybox.shader;
            skybox.name = Path.GetFileNameWithoutExtension(skyboxPath);
            SetMaterialColor(skybox, "_Tint", tint);
            SetMaterialFloat(skybox, "_Exposure", exposure);
            SetMaterialFloat(skybox, "_Rotation", rotation);
            EditorUtility.SetDirty(skybox);
            return skybox;
        }

        private static void PromoteSkyboxTextures(Material sourceSkybox, Material destinationSkybox, HashSet<string> promotedTexturePaths)
        {
            Shader shader = sourceSkybox.shader;
            int propertyCount = shader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                if (shader.GetPropertyType(i) != ShaderPropertyType.Texture)
                {
                    continue;
                }

                PromoteSkyboxTextureProperty(sourceSkybox, destinationSkybox, shader.GetPropertyName(i), promotedTexturePaths);
            }

            string[] copiedMaterialTextureProperties = { "_BackTex", "_DownTex", "_FrontTex", "_LeftTex", "_RightTex", "_Tex", "_UpTex" };
            for (int i = 0; i < copiedMaterialTextureProperties.Length; i++)
            {
                PromoteSkyboxTextureProperty(sourceSkybox, destinationSkybox, copiedMaterialTextureProperties[i], promotedTexturePaths);
            }
        }

        private static void PromoteSkyboxTextureProperty(Material sourceSkybox, Material destinationSkybox, string propertyName, HashSet<string> promotedTexturePaths)
        {
            Texture sourceTexture = sourceSkybox.GetTexture(propertyName);
            if (sourceTexture == null)
            {
                return;
            }

            Texture promotedTexture = PromoteSkyboxTexture(sourceTexture, out string promotedPath);
            if (promotedTexture != null)
            {
                destinationSkybox.SetTexture(propertyName, promotedTexture);
            }

            if (!string.IsNullOrEmpty(promotedPath))
            {
                promotedTexturePaths.Add(promotedPath);
            }
        }

        private static Texture PromoteSkyboxTexture(Texture sourceTexture, out string promotedPath)
        {
            promotedPath = null;
            string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return sourceTexture;
            }

            string extension = Path.GetExtension(sourcePath);
            string sourceName = Path.GetFileNameWithoutExtension(sourcePath);
            string destinationPath = SkyTextureRoot + "/" + PromotedSkyTexturePrefix + SanitizeAssetName(sourceName) + extension;
            promotedPath = destinationPath;
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinationPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
                {
                    throw new InvalidOperationException($"Failed to promote Allsky texture from {sourcePath} to {destinationPath}.");
                }

                AssetDatabase.ImportAsset(destinationPath);
            }

            Texture exactTexture = LoadPromotedTexture(destinationPath, sourceTexture.GetType());
            return exactTexture != null ? exactTexture : AssetDatabase.LoadAssetAtPath<Texture>(destinationPath);
        }

        private static void DeleteUnusedPromotedSkyTextures(HashSet<string> activeTexturePaths)
        {
            foreach (string textureGuid in AssetDatabase.FindAssets("t:Texture", new[] { SkyTextureRoot }))
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(textureGuid);
                string textureName = Path.GetFileNameWithoutExtension(texturePath);
                if (!textureName.StartsWith(PromotedSkyTexturePrefix, StringComparison.Ordinal) || activeTexturePaths.Contains(texturePath))
                {
                    continue;
                }

                AssetDatabase.DeleteAsset(texturePath);
            }
        }
        private static Texture LoadPromotedTexture(string path, Type preferredType)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Texture texture && preferredType.IsInstanceOfType(texture))
                {
                    return texture;
                }
            }

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Texture texture)
                {
                    return texture;
                }
            }

            return null;
        }
        private static void ConfigureImportedSkyFogVolumes(Scene scene)
        {
            int disabledCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (IsGeneratedLookdevRoot(root.name))
                {
                    continue;
                }

                foreach (Transform transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
                {
                    if (!string.Equals(transform.name, ImportedSkyFogVolumeName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (transform.gameObject.activeSelf)
                    {
                        transform.gameObject.SetActive(false);
                        disabledCount++;
                        EditorUtility.SetDirty(transform.gameObject);
                    }
                }
            }

            Debug.Log($"Olympus corridor lookdev disabled {disabledCount} imported sky/fog volume objects so the generated procedural sky controls the copied scene.");
        }
        private static void ClearGeneratedLightingData(Scene scene)
        {
            Lightmapping.SetLightingDataAssetForScene(scene, null);
            LightmapSettings.lightmaps = Array.Empty<LightmapData>();
            LightmapSettings.lightProbes = null;
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

        private static void ValidateVolumeProfile(VolumeProfile profile)
        {
            if (profile.components.Count < 6)
            {
                throw new InvalidOperationException($"Olympus corridor post-process profile should contain the planned lookdev stack. Found {profile.components.Count} components.");
            }

            for (int i = 0; i < profile.components.Count; i++)
            {
                VolumeComponent component = profile.components[i];
                if (component == null)
                {
                    throw new InvalidOperationException($"Olympus corridor post-process profile has a null component at index {i}.");
                }

                string componentPath = AssetDatabase.GetAssetPath(component);
                if (!string.Equals(componentPath, PostProcessProfilePath, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Volume component {component.name} should be embedded in {PostProcessProfilePath}, not {componentPath}.");
                }
            }
        }

        private static int CountComponentsInScene<T>(GameObject[] roots) where T : Component
        {
            int count = 0;
            foreach (GameObject root in roots)
            {
                count += root.GetComponentsInChildren<T>(includeInactive: true).Length;
            }

            return count;
        }

        private static int CountNamedRenderers(Transform parent, string namePrefix)
        {
            int count = 0;
            foreach (Renderer renderer in parent.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                if (renderer.name.StartsWith(namePrefix, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountMaterialSlotsWithPath(GameObject[] roots, string pathPrefix)
        {
            int count = 0;
            foreach (GameObject root in roots)
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material == null)
                        {
                            continue;
                        }

                        string materialPath = AssetDatabase.GetAssetPath(material);
                        if (materialPath.StartsWith(pathPrefix, StringComparison.Ordinal))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }
        private static int CountEnabledRenderersInScene(GameObject[] roots)
        {
            int count = 0;
            foreach (GameObject root in roots)
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    if (renderer.enabled)
                    {
                        count++;
                    }
                }
            }

            return count;
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

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folder = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folder))
            {
                throw new InvalidOperationException($"Invalid folder path: {path}");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static GameObject CreateChild(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = position;
            child.transform.localRotation = rotation;
            child.transform.localScale = scale;
            return child;
        }

        private static Light CreateLight(Transform parent, string name, Vector3 position)
        {
            GameObject lightObject = CreateChild(parent, name, position, Quaternion.identity, Vector3.one);
            return lightObject.AddComponent<Light>();
        }

        private static void RemoveRoot(Scene scene, string rootName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == rootName)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                    return;
                }
            }
        }

        private static void RemoveChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static GameObject RequireRoot(Scene scene, string rootName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == rootName)
                {
                    return root;
                }
            }

            throw new InvalidOperationException($"Missing root object {rootName} in {scene.path}.");
        }

        private static Transform RequireChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException($"Missing child {childName} under {parent.name}.");
            }

            return child;
        }

        private static T RequireComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"Missing {typeof(T).Name} on {gameObject.name}.");
            }

            return component;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset: {path}");
            }

            return asset;
        }

        private static void RequireVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T _))
            {
                throw new InvalidOperationException($"Missing {typeof(T).Name} in {PostProcessProfilePath}.");
            }
        }
    }
}
