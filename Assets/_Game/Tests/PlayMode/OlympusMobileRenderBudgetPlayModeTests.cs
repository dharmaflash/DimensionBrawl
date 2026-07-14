using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Core;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusMobileRenderBudgetPlayModeTests
    {
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 720;
        private const string VisualReportPath =
            "C:/tmp/DimensionBrawl-OlympusEnvironmentShadowVisualBudget.md";
        private const string StationBossCapturePath =
            "C:/Git/DimensionBrawl/Logs/olympus_station_commando_arsenal.png";
        private const string RangedWeaponMaterialPath =
            "Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_RangedFocus.mat";
        private static readonly string[] PromotedRifleGirlModelPaths =
        {
            "Assets/_Game/Art/Characters/Player/RifleGirl/Models/Rifle_Full_Body.fbx",
            "Assets/_Game/Art/Characters/Player/RifleGirl/Weapons/Weapon_Rifle.fbx"
        };
        private const string PromotedInoriModelPath =
            "Assets/_Game/Art/Characters/Player/Inori/Models/Inori_Unity.fbx";
        private const string CanonicalCommandoPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_SciFiSoldier01_CommandoVisual.prefab";
        private const string CanonicalCommandoAssaultRifleModelPath =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Weapons/SM_SciFiAssaultRifle_01.fbx";
        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string CanonicalBossRootName =
            "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string CanonicalBossVisualName =
            "BossBarrageLaneReview_HumanoidBossVisual_SciFiSoldier_01_Commando";
        private const string CanonicalBossVisualPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_SciFiSoldier01_CommandoVisual.prefab";
        private const string EnemyRoleWeaponRoot =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/";
        private const string CanonicalBossAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/SciFiSoldier01/DB_SciFiSoldier01_GeneralDeck.controller";
        private const string CanonicalBossModelPath =
            "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/SciFiSoldier01/Models/SK_SciFiSoldier01.fbx";
        private const string CanonicalNoCrossVfxPrefabPath =
            "Assets/_Game/Prefabs/VFX/Environment/PF_OlympusStation_NoCrossRedCubeZone.prefab";
        private const string StationLaneRootName =
            "BossBarrageLaneReview_SummonLaneSpace";
        private const string StationNoCrossRootName =
            "OlympusStation_NoCrossCenterLine";
        private const string StationNoCrossVisualName =
            "NoCross_RedCubeZone_Line";
        private const string ObsoleteStationBoundaryMarkerName =
            "PlayerForwardBoundary_DoNotCross";
        private const string ImportedHovlRoot =
            "Assets/_Imported/AssetStore/VFX/Hovl Studio/";
        private const string CombatGirlAnimationRoot =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield";
        private const string CombatGirlControllerPath =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string PretendardMediumFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_Medium_Dynamic.asset";
        private const string PretendardSemiBoldFontPath =
            "Assets/_Game/Art/Fonts/Pretendard/TMP_Pretendard_SemiBold_Dynamic.asset";
        private const string CanonicalTmpMobileShaderPath =
            "Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader";
        private static readonly string[] ScenePaths =
        {
            StationScenePath,
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity"
        };
        private static readonly string[] RealtimeCombatLightPrefabPaths =
        {
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot3Projectile_FireBreath.prefab",
            "Assets/_Game/Art/VFX/ActionFoundation/Summons/Prefabs/PF_SummonDragonFireBreath_FORGE3D.prefab"
        };
        private static readonly MaterialTextureConsolidationExpectation[]
            ConsolidatedWeaponMaterials =
        {
            new(
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/AssaultRifle/Materials/M_AssaultRifle.mat",
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/AssaultRifle/Textures/"),
            new(
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/BeamGun/Materials/M_LaserGun_01.mat",
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/BeamGun/Textures/T_LaserGun_01_"),
            new(
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/SkorpIO_Right/Materials/M_Skorp-IO.mat",
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/SkorpIO_Right/Textures/"),
            new(
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/SM_SciFiLaserGatlinGun/Materials/M_LaserGatlinGun.mat",
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/SM_SciFiLaserGatlinGun/Textures/")
        };
        private static readonly TextureReferenceConsolidationExpectation[]
            ConsolidatedRuntimeTextures =
        {
            new(
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/MissileLauncher/Textures/T_BazookaMagazine_BaseColor.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AircraftModels/T_BazookaMagazine_BaseColor_c58cc6a9.png",
                "Assets/_Game/Art/Materials/ActionFoundation/IntroGatePodBombingReview/AF_BombingReview_BombOriginal.mat"),
            new(
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AerialBomb/Explosion_6_0d04b1c2.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AerialBomb2/Explosion_6_0d04b1c2.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Materials/AerialBomb2/Effect_40_Explosion.mat"),
            new(
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/AirstrikeBombExplosion/Explosion_1_9064114c.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Textures/ShellExplosion/Explosion_1_9064114c.png",
                "Assets/_Game/Art/VFX/ActionFoundation/IntroGatePodBombingReview/Materials/ShellExplosion/Effect_10_Explosion.mat")
        };
        private static readonly string[] BossVisualScenePaths =
        {
            StationScenePath,
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity"
        };

        private static readonly BossWeaponExpectation[] CanonicalBossWeapons =
        {
            new("RefPosLightningGun_Action", "SM_SciFiLightingGun", "BeamGun"),
            new("RefPosLaserGatlinGun_Idle", "SM_SciFiLaserGatlinGun", "SM_SciFiLaserGatlinGun"),
            new("RefPosSkorp-IOLeft_Idle", "SM_SciFiSkorp-IO", "SkorpIO_Left"),
            new("RefPosLaserGatlinGun_Action", "SK_SciFiLaserGatlinGun", "SK_SciFiLaserGatlinGun"),
            new("RefPosAssaultRifle_Action", "SM_SciFiAssaultRifle_01", "AssaultRifle"),
            new("RefPosShotgun_Idle", "SM_SciFiShotgun", "Shotgun"),
            new("RefPosMissileLauncher_Idle", "SM_SciFiMissileLauncher", "MissileLauncher"),
            new("RefPosMissileLauncher_Action", "SM_SciFiMissileLauncher", "MissileLauncher"),
            new("RefPos2HandedGun_Action", "SM_SciFiLaserGun", "LaserGun"),
            new("RefPosSkorp-IOLeft_Action", "SM_SciFiSkorp-IO", "SkorpIO_Left"),
            new("RefPosSkorp-IORight_Idle", "SM_SciFiSkorp-IO", "SkorpIO_Right"),
            new("RefPos2HandedGun_Idle", "SM_SciFiLaserGun", "LaserGun"),
            new("RefPosLightningGun_Idle", "SM_SciFiLightingGun", "BeamGun"),
            new("RefPosLaserAssaultRifle_Idle", "SM_SciFiAssaultRifle_01", "AssaultRifle"),
            new("RefPosShotgun_Action", "SM_SciFiShotgun", "Shotgun"),
            new("RefPosSkorp-IORight_Action", "SM_SciFiSkorp-IO", "SkorpIO_Right")
        };

        private int originalQualityLevel;

        [UnitySetUp]
        public IEnumerator UseMobileRenderPipeline()
        {
            originalQualityLevel = QualitySettings.GetQualityLevel();
            int mobileQualityLevel = Array.IndexOf(QualitySettings.names, "Mobile");
            Assert.That(mobileQualityLevel, Is.GreaterThanOrEqualTo(0));

            QualitySettings.SetQualityLevel(mobileQualityLevel, applyExpensiveChanges: true);
            yield return null;

            Assert.That(
                AssetDatabase.GetAssetPath(QualitySettings.renderPipeline),
                Is.EqualTo("Assets/Settings/Mobile_RPAsset.asset"),
                "Mobile render-budget tests must exercise the shipping mobile pipeline asset.");
        }

        [UnityTearDown]
        public IEnumerator RestoreQualityLevel()
        {
            QualitySettings.SetQualityLevel(originalQualityLevel, applyExpensiveChanges: true);
            yield return null;
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator CanonicalOlympusScenesApplyBalancedEnvironmentShadowBudget()
        {
            for (int sceneIndex = 0; sceneIndex < ScenePaths.Length; sceneIndex++)
            {
                string scenePath = ScenePaths[sceneIndex];
                EditorSceneManager.LoadSceneInPlayMode(
                    scenePath,
                    new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                yield return null;

                Scene scene = SceneManager.GetActiveScene();
                Assert.That(scene.path, Is.EqualTo(scenePath));
                Assert.That(
                    CountMissingScripts(scene),
                    Is.Zero,
                    $"{scenePath} must not retain missing MonoBehaviour slots.");
                if (sceneIndex == 0)
                {
                    AssertCanonicalStationHudOwnership();
                }

                Transform mapRoot = FindStageMapRoot(scene);
                Assert.That(mapRoot, Is.Not.Null);
                MeshRenderer[] environmentRenderers =
                    mapRoot.GetComponentsInChildren<MeshRenderer>(true);
                int activeEnvironmentRendererCount = 0;
                int activeEnvironmentShadowCasterCount = 0;
                int decorativeRendererCount = 0;
                for (int i = 0; i < environmentRenderers.Length; i++)
                {
                    MeshRenderer renderer = environmentRenderers[i];
                    if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                    {
                        activeEnvironmentRendererCount++;
                        if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                        {
                            activeEnvironmentShadowCasterCount++;
                        }
                    }

                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
                    if (mesh != null && OlympusMobileRenderBudgetBootstrap.IsDecorativeShadowMeshName(mesh.name))
                    {
                        decorativeRendererCount++;
                    }
                }

                Assert.That(
                    decorativeRendererCount,
                    Is.EqualTo(471),
                    $"{scenePath} should retain the reviewed decorative renderer inventory.");
                Assert.That(
                    environmentRenderers.Length,
                    Is.GreaterThan(1600),
                    $"{scenePath} should retain the canonical environment renderer inventory.");
                Assert.That(
                    activeEnvironmentShadowCasterCount,
                    Is.Zero,
                    $"{scenePath} should remove static environment meshes from the balanced mobile shadow pass.");

                OlympusMobileEnvironmentDetailCuller detailCuller =
                    mapRoot.GetComponent<OlympusMobileEnvironmentDetailCuller>();
                Assert.That(detailCuller, Is.Not.Null);
                Assert.That(detailCuller.CandidateCount, Is.GreaterThan(300));
                Assert.That(detailCuller.CullDistance, Is.EqualTo(120f).Within(0.001f));
                Assert.That(detailCuller.ColliderCullDistance, Is.EqualTo(45f).Within(0.001f));
                if (sceneIndex == 1)
                {
                    Assert.That(detailCuller.CulledRendererCount, Is.GreaterThan(100));
                    Assert.That(activeEnvironmentRendererCount, Is.LessThan(environmentRenderers.Length));
                }

                Renderer[] sceneRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
                int retainedCharacterAndPropShadowCasterCount = 0;
                for (int i = 0; i < sceneRenderers.Length; i++)
                {
                    Renderer renderer = sceneRenderers[i];
                    if (renderer.enabled
                        && renderer.shadowCastingMode != ShadowCastingMode.Off
                        && !renderer.transform.IsChildOf(mapRoot))
                    {
                        retainedCharacterAndPropShadowCasterCount++;
                    }
                }

                Assert.That(
                    retainedCharacterAndPropShadowCasterCount,
                    Is.GreaterThan(0),
                    $"{scenePath} should retain character and major prop shadow casters.");

                Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                int decorativeLightCount = 0;
                int enabledDecorativeLightCount = 0;
                int enabledRetainedLightCount = 0;
                for (int i = 0; i < lights.Length; i++)
                {
                    if (OlympusMobileRenderBudgetBootstrap.IsDecorativeLightName(lights[i].name))
                    {
                        decorativeLightCount++;
                        if (lights[i].enabled)
                        {
                            enabledDecorativeLightCount++;
                        }
                    }
                    else if (lights[i].enabled)
                    {
                        enabledRetainedLightCount++;
                    }
                }

                Assert.That(decorativeLightCount, Is.EqualTo(4));
                Assert.That(enabledDecorativeLightCount, Is.Zero);
                Assert.That(enabledRetainedLightCount, Is.GreaterThan(20));
                Assert.That(
                    OlympusMobileRenderBudgetBootstrap.ApplyToScene(scene),
                    Is.Zero,
                    "The scene-load optimization should be idempotent.");

                AssertCanonicalMobilePipelineDependencies(scene);
            }

            AssertMobilePipelineFeatureConfiguration();
        }

        private static void AssertCanonicalMobilePipelineDependencies(Scene scene)
        {
            string scenePath = scene.path;
            List<Terrain> terrains = FindSceneComponents<Terrain>(scene);
            Assert.That(
                terrains,
                Is.Empty,
                $"{scenePath} should not require mobile terrain-hole shader variants.");

            List<ReflectionProbe> reflectionProbes = FindSceneComponents<ReflectionProbe>(scene);
            Assert.That(
                reflectionProbes,
                Has.Count.EqualTo(1),
                $"{scenePath} should keep its single authored reflection probe without multi-probe blending.");
            Assert.That(reflectionProbes[0].boxProjection, Is.False);

            List<DecalProjector> decals = FindSceneComponents<DecalProjector>(scene);
            Assert.That(
                decals.Count,
                Is.GreaterThan(0),
                $"{scenePath} requires the mobile decal renderer feature.");

            List<Behaviour> behaviours = FindSceneComponents<Behaviour>(scene);
            int lensFlareCount = 0;
            for (int i = 0; i < behaviours.Count; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour != null
                    && behaviour.GetType().FullName
                        == "UnityEngine.Rendering.LensFlareComponentSRP")
                {
                    lensFlareCount++;
                }
            }

            Assert.That(
                lensFlareCount,
                Is.Zero,
                $"{scenePath} should not require data-driven lens-flare support.");

            List<Light> lights = FindSceneComponents<Light>(scene);
            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                Assert.That(
                    light.cookie,
                    Is.Null,
                    $"{scenePath} light '{light.name}' unexpectedly requires cookie support.");
                Assert.That(
                    light.lightmapBakeType,
                    Is.Not.EqualTo(LightmapBakeType.Mixed),
                    $"{scenePath} light '{GetHierarchyPath(light.transform)}' unexpectedly requires " +
                    $"mixed-lighting support (active={light.isActiveAndEnabled}, " +
                    $"prefab='{PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(light.gameObject)}').");
                Assert.That(
                    light.renderingLayerMask,
                    Is.EqualTo(1u),
                    $"{scenePath} light '{light.name}' unexpectedly requires light-layer support.");
            }

            AssertParticleVelocityCurveModes(scene);
        }

        private static void AssertParticleVelocityCurveModes(Scene scene)
        {
            List<ParticleSystem> particleSystems = FindSceneComponents<ParticleSystem>(scene);
            List<string> mismatches = new();
            for (int i = 0; i < particleSystems.Count; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                ParticleSystem.VelocityOverLifetimeModule velocity =
                    particleSystem.velocityOverLifetime;
                if (!velocity.enabled)
                {
                    continue;
                }

                ParticleSystemCurveMode xMode = velocity.x.mode;
                ParticleSystemCurveMode yMode = velocity.y.mode;
                ParticleSystemCurveMode zMode = velocity.z.mode;
                if (xMode != yMode || xMode != zMode)
                {
                    mismatches.Add(
                        $"{GetHierarchyPath(particleSystem.transform)} " +
                        $"(x={xMode}, y={yMode}, z={zMode})");
                }
            }

            Assert.That(
                mismatches,
                Is.Empty,
                $"{scene.path} has mixed particle velocity curve modes:\n" +
                string.Join("\n", mismatches));
        }

        private static List<T> FindSceneComponents<T>(Scene scene)
            where T : Component
        {
            List<T> components = new();
            T[] candidates = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < candidates.Length; i++)
            {
                T candidate = candidates[i];
                if (candidate != null && candidate.gameObject.scene.handle == scene.handle)
                {
                    components.Add(candidate);
                }
            }

            return components;
        }

        private static int CountMissingScripts(Scene scene)
        {
            int missingScriptCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms =
                    roots[rootIndex].GetComponentsInChildren<Transform>(includeInactive: true);
                for (int transformIndex = 0;
                     transformIndex < transforms.Length;
                     transformIndex++)
                {
                    missingScriptCount +=
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            transforms[transformIndex].gameObject);
                }
            }

            return missingScriptCount;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = $"{transform.name}/{path}";
            }

            return path;
        }

        private static void AssertMobilePipelineFeatureConfiguration()
        {
            UniversalRenderPipelineAsset pipelineAsset =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    "Assets/Settings/Mobile_RPAsset.asset");
            Assert.That(pipelineAsset, Is.Not.Null);

            SerializedObject serializedPipeline = new(pipelineAsset);
            AssertSerializedBool(serializedPipeline, "m_SupportsTerrainHoles", false);
            AssertSerializedBool(serializedPipeline, "m_ReflectionProbeBlending", false);
            AssertSerializedBool(serializedPipeline, "m_ReflectionProbeBoxProjection", false);
            AssertSerializedBool(serializedPipeline, "m_MixedLightingSupported", false);
            AssertSerializedBool(serializedPipeline, "m_SupportsLightCookies", false);
            AssertSerializedBool(serializedPipeline, "m_SupportsLightLayers", true);
            AssertSerializedBool(serializedPipeline, "m_SupportDataDrivenLensFlare", false);
            AssertSerializedBool(serializedPipeline, "m_SupportScreenSpaceLensFlare", false);

            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(
                    "Assets/Settings/Mobile_Renderer.asset");
            Assert.That(rendererData, Is.Not.Null);

            bool hasActiveDecalFeature = false;
            bool hasActivePerfectDodgeFeature = false;
            IReadOnlyList<ScriptableRendererFeature> features = rendererData.rendererFeatures;
            for (int i = 0; i < features.Count; i++)
            {
                ScriptableRendererFeature feature = features[i];
                if (feature == null || !feature.isActive)
                {
                    continue;
                }

                hasActiveDecalFeature |= feature is DecalRendererFeature;
                hasActivePerfectDodgeFeature |=
                    feature.GetType().Name == "PerfectDodgeScreenDomainRendererFeature";
            }

            Assert.That(hasActiveDecalFeature, Is.True);
            Assert.That(
                hasActivePerfectDodgeFeature,
                Is.True,
                "Perfect-dodge screen-domain color preservation must remain active on mobile.");

            for (int prefabIndex = 0;
                 prefabIndex < RealtimeCombatLightPrefabPaths.Length;
                 prefabIndex++)
            {
                string prefabPath = RealtimeCombatLightPrefabPaths[prefabIndex];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.That(prefab, Is.Not.Null, prefabPath);

                Light[] lights = prefab.GetComponentsInChildren<Light>(includeInactive: true);
                Assert.That(lights.Length, Is.GreaterThan(0), prefabPath);
                for (int lightIndex = 0; lightIndex < lights.Length; lightIndex++)
                {
                    Assert.That(
                        lights[lightIndex].lightmapBakeType,
                        Is.EqualTo(LightmapBakeType.Realtime),
                        $"Combat VFX light '{prefabPath}/{lights[lightIndex].name}' must remain realtime.");
                }
            }
        }

        private static void AssertSerializedBool(
            SerializedObject serializedObject,
            string propertyName,
            bool expected)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"Missing serialized property '{propertyName}'.");
            Assert.That(property.boolValue, Is.EqualTo(expected), propertyName);
        }

        [Test]
        public void CanonicalWeaponMaterialsShareByteIdenticalTextureSets()
        {
            for (int expectationIndex = 0;
                 expectationIndex < ConsolidatedWeaponMaterials.Length;
                 expectationIndex++)
            {
                MaterialTextureConsolidationExpectation expectation =
                    ConsolidatedWeaponMaterials[expectationIndex];
                Material material = AssetDatabase.LoadAssetAtPath<Material>(expectation.MaterialPath);
                Assert.That(material, Is.Not.Null, expectation.MaterialPath);

                string[] dependencies =
                    AssetDatabase.GetDependencies(expectation.MaterialPath, recursive: true);
                int textureDependencyCount = 0;
                for (int dependencyIndex = 0;
                     dependencyIndex < dependencies.Length;
                     dependencyIndex++)
                {
                    string dependency = dependencies[dependencyIndex];
                    Assert.That(
                        dependency.StartsWith(
                            expectation.DuplicateTexturePrefix,
                            StringComparison.Ordinal),
                        Is.False,
                        $"{expectation.MaterialPath} retains byte-identical texture copy {dependency}.");

                    Type dependencyType = AssetDatabase.GetMainAssetTypeAtPath(dependency);
                    if (dependencyType != null
                        && typeof(Texture).IsAssignableFrom(dependencyType))
                    {
                        textureDependencyCount++;
                    }
                }

                Assert.That(
                    textureDependencyCount,
                    Is.GreaterThanOrEqualTo(5),
                    $"{expectation.MaterialPath} should retain its complete PBR texture set.");
            }
        }

        [Test]
        public void CanonicalBossRoleWeaponBaseColorsUseMobileOneKBudget()
        {
            string[] dependencies =
                AssetDatabase.GetDependencies(CanonicalBossVisualPrefabPath, recursive: true);
            int baseColorCount = 0;
            int downscaledBaseColorCount = 0;
            for (int dependencyIndex = 0;
                 dependencyIndex < dependencies.Length;
                 dependencyIndex++)
            {
                string dependency = dependencies[dependencyIndex];
                if (!dependency.StartsWith(EnemyRoleWeaponRoot, StringComparison.Ordinal)
                    || Path.GetFileNameWithoutExtension(dependency)
                        .IndexOf("BaseColor", StringComparison.OrdinalIgnoreCase) < 0
                    || AssetImporter.GetAtPath(dependency) is not TextureImporter importer)
                {
                    continue;
                }

                baseColorCount++;
                importer.GetSourceTextureWidthAndHeight(out int sourceWidth, out int sourceHeight);
                if (Math.Max(sourceWidth, sourceHeight) <= 1024)
                {
                    continue;
                }

                TextureImporterPlatformSettings android =
                    importer.GetPlatformTextureSettings("Android");
                Assert.That(
                    android.overridden,
                    Is.True,
                    $"{dependency} must have an explicit Android texture budget.");
                Assert.That(
                    android.maxTextureSize,
                    Is.LessThanOrEqualTo(1024),
                    $"{dependency} exceeds the enemy role-weapon 1K Android budget.");
                downscaledBaseColorCount++;
            }

            Assert.That(
                baseColorCount,
                Is.GreaterThanOrEqualTo(6),
                "The canonical boss arsenal should retain its authored base-color texture set.");
            Assert.That(
                downscaledBaseColorCount,
                Is.GreaterThanOrEqualTo(6),
                "The canonical boss arsenal should keep its large base colors on the 1K Android budget.");
        }

        [Test]
        public void CanonicalRuntimeMaterialsShareReviewedDuplicateTexturePayloads()
        {
            string[] corridorDependencies =
                AssetDatabase.GetDependencies(ScenePaths[1], recursive: true);
            for (int expectationIndex = 0;
                 expectationIndex < ConsolidatedRuntimeTextures.Length;
                 expectationIndex++)
            {
                TextureReferenceConsolidationExpectation expectation =
                    ConsolidatedRuntimeTextures[expectationIndex];
                string[] materialDependencies =
                    AssetDatabase.GetDependencies(expectation.ConsumerMaterialPath, recursive: true);
                Assert.That(
                    materialDependencies,
                    Does.Contain(expectation.CanonicalTexturePath),
                    $"{expectation.ConsumerMaterialPath} should use the canonical texture payload.");
                Assert.That(
                    materialDependencies,
                    Does.Not.Contain(expectation.DuplicateTexturePath),
                    $"{expectation.ConsumerMaterialPath} retains an exact duplicate texture payload.");
                Assert.That(
                    corridorDependencies,
                    Does.Not.Contain(expectation.DuplicateTexturePath),
                    $"The canonical corridor should not package {expectation.DuplicateTexturePath}.");
            }
        }

        private static void AssertCanonicalStationHudOwnership()
        {
            Behaviour combatHudPresenter = null;
            Behaviour combatHudBinder = null;
            Behaviour[] behaviours = UnityEngine.Object.FindObjectsByType<Behaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                switch (behaviour.GetType().FullName)
                {
                    case "DimensionBrawl.UI.CombatHudPresenter":
                        combatHudPresenter = behaviour;
                        break;
                    case "DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder":
                        combatHudBinder = behaviour;
                        break;
                }
            }

            Assert.That(combatHudPresenter, Is.Not.Null);
            Assert.That(combatHudPresenter.isActiveAndEnabled, Is.True, "UGUI should own the visible combat HUD.");
            Assert.That(combatHudBinder, Is.Not.Null);
            Assert.That(combatHudBinder.isActiveAndEnabled, Is.True, "The canonical HUD binder should stay active.");
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator RuntimeBossScenesUseAuthoredCommandoArsenalVisual()
        {
            for (int sceneIndex = 0; sceneIndex < BossVisualScenePaths.Length; sceneIndex++)
            {
                string scenePath = BossVisualScenePaths[sceneIndex];
                EditorSceneManager.LoadSceneInPlayMode(
                    scenePath,
                    new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                yield return null;

                Scene scene = SceneManager.GetActiveScene();
                Assert.That(scene.path, Is.EqualTo(scenePath));

                Transform bossRoot = FindSceneTransform(scene, CanonicalBossRootName);
                Assert.That(bossRoot, Is.Not.Null, $"{scenePath} is missing the canonical boss gameplay root.");

                Transform visual = bossRoot.Find(CanonicalBossVisualName);
                Assert.That(visual, Is.Not.Null, $"{scenePath} should use the authored Commando arsenal boss visual.");
                Assert.That(
                    bossRoot.Find("BossBarrageLaneReview_HumanoidBossVisual_FinalStandCommanderElite"),
                    Is.Null,
                    $"{scenePath} must not use the unrelated HeavyBattleArmor commander visual as its boss.");
                Assert.That(
                    bossRoot.Find("BossBarrageLaneReview_HumanoidBossVisual_LineCasterGatling"),
                    Is.Null,
                    $"{scenePath} must not use the regular SciFiSoldier LineCaster as its boss.");
                Assert.That(
                    bossRoot.Find("BossBarrageLaneReview_HumanoidBossVisual_AkazaPhase2"),
                    Is.Null,
                    $"{scenePath} must not use the unrelated Akaza review visual as its boss.");
                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(visual.gameObject);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    Assert.That(prefabPath, Is.EqualTo(CanonicalBossVisualPrefabPath));
                }

                Animator animator = visual.GetComponentInChildren<Animator>(includeInactive: true);
                Assert.That(animator, Is.Not.Null);
                Assert.That(animator.runtimeAnimatorController, Is.Not.Null);
                Assert.That(
                    AssetDatabase.GetAssetPath(animator.runtimeAnimatorController),
                    Is.EqualTo(CanonicalBossAnimatorControllerPath));

                BossBarrageVisualCueDriver cueDriver = bossRoot.GetComponent<BossBarrageVisualCueDriver>();
                Assert.That(cueDriver, Is.Not.Null);
                Assert.That(
                    cueDriver.Animator,
                    Is.SameAs(animator),
                    "Boss attack cues should drive the canonical Commando Animator directly.");

                AssertCanonicalCommandoArsenal(visual);
                Assert.That(
                    visual.GetComponentInChildren<CombatHealth>(includeInactive: true),
                    Is.Null,
                    "The visual prefab must not duplicate the boss gameplay owner.");

                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
                int renderReadyCount = 0;
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].enabled
                        && !renderers[i].forceRenderingOff)
                    {
                        renderReadyCount++;
                    }
                }

                Assert.That(
                    renderReadyCount,
                    Is.GreaterThan(0),
                    "The Commando arsenal boss should be render-ready when the encounter activates it.");

                BossBasicFireEmitter basicFireEmitter = bossRoot.GetComponent<BossBasicFireEmitter>();
                Assert.That(basicFireEmitter, Is.Not.Null);
                SerializedProperty fireOriginProperty =
                    new SerializedObject(basicFireEmitter).FindProperty("fireOrigin");
                Transform fireOrigin = fireOriginProperty?.objectReferenceValue as Transform;
                Assert.That(fireOrigin, Is.Not.Null);
                Assert.That(
                    fireOrigin.IsChildOf(
                        FindDescendant(
                            FindDescendant(visual, "RefPosMissileLauncher_Action"),
                            "SM_SciFiMissileLauncher")),
                    Is.True,
                    "Boss projectiles should originate from the action rocket launcher.");

                if (scenePath == StationScenePath)
                {
                    DisableVisualEffectsForCapture();
                    yield return null;
                    yield return null;
                    CaptureCanonicalStationBoss(bossRoot, visual);
                }
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator CanonicalOlympusBossHudTracksBoundBossHealth()
        {
            for (int sceneIndex = 0; sceneIndex < ScenePaths.Length; sceneIndex++)
            {
                string scenePath = ScenePaths[sceneIndex];
                EditorSceneManager.LoadSceneInPlayMode(
                    scenePath,
                    new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                yield return null;

                Behaviour presenter = FindBehaviour("DimensionBrawl.UI.CombatHudPresenter");
                Behaviour binder = FindBehaviour("DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder");
                Assert.That(presenter, Is.Not.Null, $"{scenePath} is missing the canonical combat HUD presenter.");
                Assert.That(binder, Is.Not.Null, $"{scenePath} is missing the canonical combat HUD binder.");
                if (!binder.gameObject.activeInHierarchy)
                {
                    binder.gameObject.SetActive(true);
                    yield return null;
                }

                System.Reflection.PropertyInfo visibleProperty = presenter.GetType().GetProperty("BossHudVisible");
                System.Reflection.PropertyInfo fillProperty = presenter.GetType().GetProperty("BossHealthFillAmount");
                System.Reflection.FieldInfo fillField = presenter.GetType().GetField(
                    "bossHealthFill",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                System.Reflection.FieldInfo bossHealthField = binder.GetType().GetField(
                    "bossHealth",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Assert.That(visibleProperty, Is.Not.Null);
                Assert.That(fillProperty, Is.Not.Null);
                Assert.That(fillField, Is.Not.Null);
                Assert.That(bossHealthField, Is.Not.Null);

                Image boundFill = fillField.GetValue(presenter) as Image;
                CombatHealth bossHealth = bossHealthField.GetValue(binder) as CombatHealth;
                Assert.That(boundFill, Is.Not.Null, $"{scenePath} has no bound boss HP fill.");
                RectTransform serializedBossRoot = presenter.GetType().GetField(
                    "bossHudRoot",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.GetValue(presenter) as RectTransform;
                Assert.That(
                    boundFill.gameObject.activeInHierarchy,
                    Is.True,
                    $"{scenePath} boundFillSelf={boundFill.gameObject.activeSelf} "
                    + $"parent={boundFill.transform.parent?.name} "
                    + $"parentActive={boundFill.transform.parent?.gameObject.activeInHierarchy} "
                    + $"bossRoot={serializedBossRoot?.name} "
                    + $"bossRootSelf={serializedBossRoot?.gameObject.activeSelf} "
                    + $"presenterActive={presenter.gameObject.activeInHierarchy}");
                Assert.That(bossHealth, Is.Not.Null, $"{scenePath} has no bound boss CombatHealth.");
                Assert.That((bool)visibleProperty.GetValue(presenter), Is.True);

                Image[] images = UnityEngine.Object.FindObjectsByType<Image>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                int activeBossHealthFillCount = 0;
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] != null
                        && images[i].name == "BossHpFill"
                        && images[i].gameObject.activeInHierarchy)
                    {
                        activeBossHealthFillCount++;
                    }
                }

                Assert.That(
                    activeBossHealthFillCount,
                    Is.EqualTo(1),
                    $"{scenePath} should render exactly one authoritative boss HP fill.");

                float damage = bossHealth.MaxHealth * 0.1f;
                Assert.That(
                    bossHealth.TryApplyDamage(new DamageInfo(
                        null,
                        DamageTeam.Player,
                        damage,
                        bossHealth.transform.position,
                        Vector3.forward,
                        0f,
                        DamageResponsePolicy.DamageOnly,
                        CombatControlLockPolicy.None)),
                    Is.True);
                yield return null;

                float displayedFill = (float)fillProperty.GetValue(presenter);
                Assert.That(
                    displayedFill,
                    Is.EqualTo(bossHealth.HealthRatio).Within(0.001f),
                    $"{scenePath} must update the same boss HP fill that remains visible.");
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator CanonicalCorridorInoriRifleUsesPromotedWeaponAssets()
        {
            string scenePath = ScenePaths[1];
            EditorSceneManager.LoadSceneInPlayMode(
                scenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene scene = SceneManager.GetActiveScene();
            Transform rifleRoot = FindSceneTransform(scene, "InoriRifle");
            Material canonicalMaterial = AssetDatabase.LoadAssetAtPath<Material>(RangedWeaponMaterialPath);
            Assert.That(rifleRoot, Is.Not.Null);
            Assert.That(canonicalMaterial, Is.Not.Null);

            Renderer[] renderers = rifleRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            Assert.That(renderers, Is.Not.Empty);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    AssertGameOwnedAsset(meshFilter.sharedMesh, $"{renderer.name} mesh");
                }

                Material[] materials = renderer.sharedMaterials;
                Assert.That(materials, Is.Not.Empty, $"{renderer.name} should keep its weapon material.");
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Assert.That(
                        materials[materialIndex],
                        Is.SameAs(canonicalMaterial),
                        $"{renderer.name} should use the promoted RifleGirl weapon material.");
                }
            }

            string[] textureProperties = canonicalMaterial.GetTexturePropertyNames();
            for (int propertyIndex = 0; propertyIndex < textureProperties.Length; propertyIndex++)
            {
                Texture texture = canonicalMaterial.GetTexture(textureProperties[propertyIndex]);
                if (texture != null)
                {
                    AssertGameOwnedAsset(texture, $"weapon material {textureProperties[propertyIndex]} texture");
                }
            }

            for (int modelIndex = 0; modelIndex < PromotedRifleGirlModelPaths.Length; modelIndex++)
            {
                AssertModelMaterialRemapsGameOwned(PromotedRifleGirlModelPaths[modelIndex]);
            }
        }

        [Test]
        public void PromotedInoriModelUsesGameOwnedMaterialRemaps()
        {
            AssertModelMaterialRemapsGameOwned(PromotedInoriModelPath);
        }

        [Test]
        public void PromotedInoriModelUsesGpuOnlyMeshPayload()
        {
            ModelImporter importer =
                AssetImporter.GetAtPath(PromotedInoriModelPath) as ModelImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(
                importer.isReadable,
                Is.False,
                "The canonical Inori model should not retain a duplicate CPU-readable mesh payload.");
            Assert.That(
                importer.importAnimation,
                Is.False,
                "The canonical Inori geometry model should keep animation clips in their dedicated assets.");
            Assert.That(
                importer.importBlendShapes,
                Is.True,
                "The CPU-copy optimization must preserve Inori facial blend shapes.");
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator CanonicalInoriGpuOnlyMeshesSupportRuntimeConsumers()
        {
            EditorSceneManager.LoadSceneInPlayMode(
                StationScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            SkinnedMeshRenderer[] renderers = UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            var bakedMeshes = new List<Mesh>();
            int inoriRendererCount = 0;
            int bakedRendererCount = 0;
            bool verifiedBlendShapeLookup = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                Mesh sourceMesh = renderer.sharedMesh;
                if (sourceMesh == null
                    || AssetDatabase.GetAssetPath(sourceMesh) != PromotedInoriModelPath)
                {
                    continue;
                }

                inoriRendererCount++;
                var bakedMesh = new Mesh
                {
                    name = sourceMesh.name + "_GpuOnlyContractTest"
                };
                bakedMeshes.Add(bakedMesh);
                renderer.BakeMesh(bakedMesh);
                Assert.That(
                    bakedMesh.vertexCount,
                    Is.EqualTo(sourceMesh.vertexCount),
                    $"{renderer.name} must remain compatible with perfect-dodge afterimage baking.");
                bakedRendererCount++;

                if (!verifiedBlendShapeLookup && sourceMesh.blendShapeCount > 0)
                {
                    string shapeName = sourceMesh.GetBlendShapeName(0);
                    Assert.That(shapeName, Is.Not.Empty);
                    Assert.That(sourceMesh.GetBlendShapeIndex(shapeName), Is.Zero);
                    verifiedBlendShapeLookup = true;
                }
            }

            MeshCollider[] colliders = UnityEngine.Object.FindObjectsByType<MeshCollider>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < colliders.Length; i++)
            {
                Mesh colliderMesh = colliders[i].sharedMesh;
                Assert.That(
                    colliderMesh == null
                        || AssetDatabase.GetAssetPath(colliderMesh) != PromotedInoriModelPath,
                    Is.True,
                    $"{colliders[i].name} must not require CPU access to the canonical Inori model.");
            }

            Assert.That(inoriRendererCount, Is.GreaterThanOrEqualTo(8));
            Assert.That(bakedRendererCount, Is.EqualTo(inoriRendererCount));
            Assert.That(
                verifiedBlendShapeLookup,
                Is.True,
                "At least one canonical Inori mesh should preserve expression blend shapes.");

            for (int i = 0; i < bakedMeshes.Count; i++)
            {
                UnityEngine.Object.Destroy(bakedMeshes[i]);
            }

            yield return null;
        }

        [Test]
        public void CanonicalCommandoPrefabHasNoImportedDependencies()
        {
            string[] dependencies = AssetDatabase.GetDependencies(CanonicalCommandoPrefabPath, recursive: true);
            Assert.That(dependencies, Is.Not.Empty);
            for (int i = 0; i < dependencies.Length; i++)
            {
                Assert.That(
                    dependencies[i],
                    Does.Not.StartWith("Assets/_Imported/"),
                    $"Canonical Commando prefab should not retain raw source dependency {dependencies[i]}.");
            }

            ModelImporter weaponImporter =
                AssetImporter.GetAtPath(CanonicalCommandoAssaultRifleModelPath) as ModelImporter;
            Assert.That(weaponImporter, Is.Not.Null);
            Assert.That(
                weaponImporter.isReadable,
                Is.False,
                "Static Commando assault rifle should not retain a CPU mesh copy.");
        }

        [Test]
        public void OlympusStationNoCrossVfxHasNoImportedHovlDependencies()
        {
            GameObject canonicalPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(CanonicalNoCrossVfxPrefabPath);
            Assert.That(canonicalPrefab, Is.Not.Null);

            string[] prefabDependencies =
                AssetDatabase.GetDependencies(CanonicalNoCrossVfxPrefabPath, recursive: true);
            Assert.That(prefabDependencies, Is.Not.Empty);
            for (int i = 0; i < prefabDependencies.Length; i++)
            {
                Assert.That(
                    prefabDependencies[i],
                    Does.Not.StartWith("Assets/_Imported/"),
                    $"Canonical no-cross VFX retains raw dependency {prefabDependencies[i]}.");
            }

            string[] sceneDependencies = AssetDatabase.GetDependencies(ScenePaths[0], recursive: true);
            Assert.That(sceneDependencies, Does.Contain(CanonicalNoCrossVfxPrefabPath));
            for (int i = 0; i < sceneDependencies.Length; i++)
            {
                Assert.That(
                    sceneDependencies[i],
                    Does.Not.StartWith(ImportedHovlRoot),
                    $"Olympus Station retains raw Hovl dependency {sceneDependencies[i]}.");
            }
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator OlympusStationNoCrossVisualMatchesRuntimePlayerBoundary()
        {
            EditorSceneManager.LoadSceneInPlayMode(
                StationScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene scene = SceneManager.GetActiveScene();
            Assert.That(scene.path, Is.EqualTo(StationScenePath));

            Transform laneRoot = FindSceneTransform(scene, StationLaneRootName);
            Assert.That(laneRoot, Is.Not.Null);
            SummonLaneSpace laneSpace = laneRoot.GetComponent<SummonLaneSpace>();
            Assert.That(laneSpace, Is.Not.Null);

            Transform noCrossRoot = FindSceneTransform(scene, StationNoCrossRootName);
            Assert.That(noCrossRoot, Is.Not.Null);
            Transform noCrossVisual = FindDescendant(noCrossRoot, StationNoCrossVisualName);
            Assert.That(noCrossVisual, Is.Not.Null);
            Assert.That(
                FindSceneTransform(scene, ObsoleteStationBoundaryMarkerName),
                Is.Null,
                "Station should not retain a second visual or collider for the player boundary.");

            Vector2 rootLaneCoordinates = laneSpace.GetLaneCoordinates(noCrossRoot.position);
            Vector2 visualLaneCoordinates = laneSpace.GetLaneCoordinates(noCrossVisual.position);
            Vector3 clampedBeyondBoundary = laneSpace.ClampPlayerPosition(
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ + 1f));
            Vector2 clampedLaneCoordinates = laneSpace.GetLaneCoordinates(clampedBeyondBoundary);

            Assert.That(rootLaneCoordinates.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(
                rootLaneCoordinates.y,
                Is.EqualTo(laneSpace.ForwardBoundaryZ).Within(0.001f));
            Assert.That(
                visualLaneCoordinates.y,
                Is.EqualTo(laneSpace.ForwardBoundaryZ).Within(0.001f),
                "The visible no-cross line should be centered on the runtime player clamp.");
            Assert.That(
                clampedLaneCoordinates.y,
                Is.EqualTo(visualLaneCoordinates.y).Within(0.001f),
                "The visible and physical forward boundaries should resolve to one lane coordinate.");
        }

        [Test]
        public void CanonicalScenesHaveNoRawImportedTextureDependencies()
        {
            var importedTextures = new HashSet<string>(StringComparer.Ordinal);
            for (int sceneIndex = 0; sceneIndex < ScenePaths.Length; sceneIndex++)
            {
                string[] dependencies =
                    AssetDatabase.GetDependencies(ScenePaths[sceneIndex], recursive: true);
                for (int dependencyIndex = 0; dependencyIndex < dependencies.Length; dependencyIndex++)
                {
                    string dependency = dependencies[dependencyIndex].Replace('\\', '/');
                    if (dependency.StartsWith("Assets/_Imported/", StringComparison.Ordinal)
                        && AssetDatabase.LoadAssetAtPath<Texture>(dependency) != null)
                    {
                        importedTextures.Add(dependency);
                    }
                }
            }

            Assert.That(
                importedTextures,
                Is.Empty,
                "Canonical scenes retain raw imported textures:\n" + string.Join("\n", importedTextures));
        }

        [Test]
        public void RuntimeTmpDefaultsHaveNoImportedFontDependencies()
        {
            UnityEngine.Object settings = AssetDatabase.LoadMainAssetAtPath(TmpSettingsPath);
            UnityEngine.Object mediumFont = AssetDatabase.LoadMainAssetAtPath(PretendardMediumFontPath);
            UnityEngine.Object semiBoldFont = AssetDatabase.LoadMainAssetAtPath(PretendardSemiBoldFontPath);

            Assert.That(settings, Is.Not.Null);
            Assert.That(mediumFont, Is.Not.Null);
            Assert.That(semiBoldFont, Is.Not.Null);

            var serializedSettings = new SerializedObject(settings);
            SerializedProperty defaultFont = serializedSettings.FindProperty("m_defaultFontAsset");
            SerializedProperty defaultFontPath = serializedSettings.FindProperty("m_defaultFontAssetPath");
            SerializedProperty getFontFeaturesAtRuntime =
                serializedSettings.FindProperty("m_GetFontFeaturesAtRuntime");
            Assert.That(defaultFont, Is.Not.Null);
            Assert.That(defaultFont.objectReferenceValue, Is.EqualTo(mediumFont));
            Assert.That(defaultFontPath, Is.Not.Null);
            Assert.That(defaultFontPath.stringValue, Is.Empty);
            Assert.That(getFontFeaturesAtRuntime, Is.Not.Null);
            Assert.That(getFontFeaturesAtRuntime.boolValue, Is.False);

            var serializedMediumFont = new SerializedObject(mediumFont);
            var serializedSemiBoldFont = new SerializedObject(semiBoldFont);
            Assert.That(serializedMediumFont.FindProperty("m_GetFontFeatures").boolValue, Is.False);
            Assert.That(serializedSemiBoldFont.FindProperty("m_GetFontFeatures").boolValue, Is.False);

            Material mediumMaterial = LoadEmbeddedMaterial(PretendardMediumFontPath);
            Material semiBoldMaterial = LoadEmbeddedMaterial(PretendardSemiBoldFontPath);
            Assert.That(mediumMaterial, Is.Not.Null);
            Assert.That(semiBoldMaterial, Is.Not.Null);
            Assert.That(
                AssetDatabase.GetAssetPath(mediumMaterial.shader).Replace('\\', '/'),
                Is.EqualTo(CanonicalTmpMobileShaderPath));
            Assert.That(
                AssetDatabase.GetAssetPath(semiBoldMaterial.shader).Replace('\\', '/'),
                Is.EqualTo(CanonicalTmpMobileShaderPath));
            Assert.That(
                AssetDatabase.IsValidFolder("Assets/TextMesh Pro/Resources/Fonts & Materials"),
                Is.False,
                "Unused TMP starter fonts should not be packed through Resources.");

            string[] runtimeTmpPaths =
            {
                TmpSettingsPath,
                PretendardMediumFontPath,
                PretendardSemiBoldFontPath
            };
            var importedDependencies = new HashSet<string>(StringComparer.Ordinal);
            for (int pathIndex = 0; pathIndex < runtimeTmpPaths.Length; pathIndex++)
            {
                string[] dependencies = AssetDatabase.GetDependencies(runtimeTmpPaths[pathIndex], recursive: true);
                for (int dependencyIndex = 0; dependencyIndex < dependencies.Length; dependencyIndex++)
                {
                    if (dependencies[dependencyIndex].StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                    {
                        importedDependencies.Add(dependencies[dependencyIndex]);
                    }
                }
            }

            Assert.That(
                importedDependencies,
                Is.Empty,
                "Runtime TMP defaults should not retain vendor font or shader dependencies.");
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator CanonicalCorridorCombatGirlSourceUsesGameOwnedAssets()
        {
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePaths[1],
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Transform sourceRoot = FindSceneTransform(
                SceneManager.GetActiveScene(),
                "CombatGirlSwordShield_PlayerVisual");
            Assert.That(sourceRoot, Is.Not.Null);

            var importedPaths = new HashSet<string>(StringComparer.Ordinal);
            Transform[] transforms = sourceRoot.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                AddImportedPath(
                    importedPaths,
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(transforms[i].gameObject));
            }

            Renderer[] renderers = sourceRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    AddImportedPath(importedPaths, AssetDatabase.GetAssetPath(meshFilter.sharedMesh));
                }

                if (renderer is SkinnedMeshRenderer skinnedRenderer)
                {
                    AddImportedPath(importedPaths, AssetDatabase.GetAssetPath(skinnedRenderer.sharedMesh));
                }

                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    AddImportedPath(importedPaths, AssetDatabase.GetAssetPath(material));
                    if (material == null)
                    {
                        continue;
                    }

                    string[] textureProperties = material.GetTexturePropertyNames();
                    for (int propertyIndex = 0; propertyIndex < textureProperties.Length; propertyIndex++)
                    {
                        AddImportedPath(
                            importedPaths,
                            AssetDatabase.GetAssetPath(material.GetTexture(textureProperties[propertyIndex])));
                    }
                }
            }

            Animator[] animators = sourceRoot.GetComponentsInChildren<Animator>(includeInactive: true);
            for (int i = 0; i < animators.Length; i++)
            {
                AddImportedPath(importedPaths, AssetDatabase.GetAssetPath(animators[i].avatar));
                AddImportedPath(importedPaths, AssetDatabase.GetAssetPath(animators[i].runtimeAnimatorController));
            }

            Assert.That(
                importedPaths,
                Is.Empty,
                "Hidden CombatGirl extraction source retains raw assets:\n" + string.Join("\n", importedPaths));
        }

        [Test]
        public void CombatGirlControllerUsesEquivalentNativeRuntimeClips()
        {
            string[] dependencies = AssetDatabase.GetDependencies(CombatGirlControllerPath, recursive: true);
            int runtimeClipCount = 0;
            for (int i = 0; i < dependencies.Length; i++)
            {
                string path = dependencies[i].Replace('\\', '/');
                if (path.StartsWith(CombatGirlAnimationRoot, StringComparison.Ordinal)
                    && path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.That(
                        path,
                        Is.EqualTo(CombatGirlAnimationRoot + "/SS_StopStep.fbx"),
                        $"Only the Unity-unloadable StopStep source clip may remain as FBX, found {path}.");
                    continue;
                }

                if (!path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                runtimeClipCount++;
                Assert.That(
                    path,
                    Does.StartWith(CombatGirlAnimationRoot + "/RuntimeClips/"));
                AnimationClip runtimeClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                string sourcePath = $"{CombatGirlAnimationRoot}/{runtimeClip.name}.fbx";
                AnimationClip sourceClip = null;
                UnityEngine.Object[] sourceAssets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
                for (int assetIndex = 0; assetIndex < sourceAssets.Length; assetIndex++)
                {
                    if (sourceAssets[assetIndex] is AnimationClip candidate
                        && !candidate.name.StartsWith("__preview__", StringComparison.Ordinal))
                    {
                        sourceClip = candidate;
                        break;
                    }
                }

                Assert.That(runtimeClip, Is.Not.Null);
                Assert.That(sourceClip, Is.Not.Null, $"Missing source clip for {runtimeClip.name}.");
                Assert.That(runtimeClip.length, Is.EqualTo(sourceClip.length).Within(0.0001f));
                Assert.That(runtimeClip.frameRate, Is.EqualTo(sourceClip.frameRate).Within(0.0001f));
                Assert.That(
                    AnimationUtility.GetCurveBindings(runtimeClip).Length,
                    Is.EqualTo(AnimationUtility.GetCurveBindings(sourceClip).Length));
                Assert.That(
                    AnimationUtility.GetObjectReferenceCurveBindings(runtimeClip).Length,
                    Is.EqualTo(AnimationUtility.GetObjectReferenceCurveBindings(sourceClip).Length));
                Assert.That(
                    AnimationUtility.GetAnimationEvents(runtimeClip).Length,
                    Is.EqualTo(AnimationUtility.GetAnimationEvents(sourceClip).Length));
            }

            Assert.That(runtimeClipCount, Is.EqualTo(15));
        }

        [Test]
        public void CombatGirlControllerHasNoImportedDependencies()
        {
            string[] dependencies = AssetDatabase.GetDependencies(CombatGirlControllerPath, recursive: true);
            var importedPaths = new List<string>();
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (dependencies[i].StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    importedPaths.Add(dependencies[i]);
                }
            }

            Assert.That(
                importedPaths,
                Is.Empty,
                "CombatGirl controller retains raw dependencies:\n" + string.Join("\n", importedPaths));
        }

        private static Behaviour FindBehaviour(string fullTypeName)
        {
            Behaviour[] behaviours = UnityEngine.Object.FindObjectsByType<Behaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().FullName == fullTypeName)
                {
                    return behaviour;
                }
            }

            return null;
        }

        private static Transform FindSceneTransform(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(includeInactive: true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    if (transforms[transformIndex].name == objectName)
                    {
                        return transforms[transformIndex];
                    }
                }
            }

            return null;
        }

        private static void AssertCanonicalCommandoArsenal(Transform visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            Assert.That(
                Array.Exists(renderers, renderer => RendererUsesMesh(renderer, CanonicalBossModelPath)),
                Is.True,
                $"Commando boss should use the canonical body model at {CanonicalBossModelPath}.");

            for (int i = 0; i < CanonicalBossWeapons.Length; i++)
            {
                BossWeaponExpectation expectation = CanonicalBossWeapons[i];
                Transform socket = FindDescendant(visual, expectation.SocketName);
                Assert.That(socket, Is.Not.Null, $"Commando boss is missing {expectation.SocketName}.");
                Transform weapon = FindDescendant(socket, expectation.WeaponName);
                Assert.That(
                    weapon,
                    Is.Not.Null,
                    $"Commando boss is missing {expectation.WeaponName} under {expectation.SocketName}.");
                Renderer[] weaponRenderers = weapon.GetComponentsInChildren<Renderer>(includeInactive: true);
                Assert.That(
                    Array.Exists(weaponRenderers, renderer => RendererUsesMesh(renderer, expectation.ModelPath)),
                    Is.True,
                    $"{expectation.SocketName}.{expectation.WeaponName} should render {expectation.ModelPath}.");
            }
        }

        private static bool RendererUsesMesh(Renderer renderer, string expectedPath)
        {
            Mesh mesh = renderer is SkinnedMeshRenderer skinnedRenderer
                ? skinnedRenderer.sharedMesh
                : renderer.GetComponent<MeshFilter>()?.sharedMesh;
            return mesh != null && AssetDatabase.GetAssetPath(mesh) == expectedPath;
        }

        private static Transform FindDescendant(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName)
                {
                    return transforms[i];
                }
            }

            return null;
        }

        private readonly struct BossWeaponExpectation
        {
            private const string ModelRoot =
                "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/RoleWeapons/";

            public BossWeaponExpectation(string socketName, string weaponName, string targetAssetName)
            {
                SocketName = socketName;
                WeaponName = weaponName;
                ModelPath = ModelRoot + targetAssetName + "/Models/" + targetAssetName + ".fbx";
            }

            public string SocketName { get; }
            public string WeaponName { get; }
            public string ModelPath { get; }
        }

        private readonly struct MaterialTextureConsolidationExpectation
        {
            public MaterialTextureConsolidationExpectation(
                string materialPath,
                string duplicateTexturePrefix)
            {
                MaterialPath = materialPath;
                DuplicateTexturePrefix = duplicateTexturePrefix;
            }

            public string MaterialPath { get; }
            public string DuplicateTexturePrefix { get; }
        }

        private readonly struct TextureReferenceConsolidationExpectation
        {
            public TextureReferenceConsolidationExpectation(
                string canonicalTexturePath,
                string duplicateTexturePath,
                string consumerMaterialPath)
            {
                CanonicalTexturePath = canonicalTexturePath;
                DuplicateTexturePath = duplicateTexturePath;
                ConsumerMaterialPath = consumerMaterialPath;
            }

            public string CanonicalTexturePath { get; }
            public string DuplicateTexturePath { get; }
            public string ConsumerMaterialPath { get; }
        }

        private static void AssertGameOwnedAsset(UnityEngine.Object asset, string label)
        {
            Assert.That(asset, Is.Not.Null, $"{label} should be assigned.");
            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            Assert.That(
                assetPath,
                Does.StartWith("Assets/_Game/"),
                $"{label} should use a promoted game-owned asset, found {assetPath}.");
        }

        private static void AssertModelMaterialRemapsGameOwned(string modelPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(modelPath);
            Assert.That(importer, Is.Not.Null);
            int materialRemapCount = 0;
            foreach (KeyValuePair<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> remap
                     in importer.GetExternalObjectMap())
            {
                if (remap.Key.type != typeof(Material) || remap.Value == null)
                {
                    continue;
                }

                materialRemapCount++;
                AssertGameOwnedAsset(remap.Value, $"{modelPath} {remap.Key.name} material remap");
            }

            Assert.That(materialRemapCount, Is.GreaterThan(0), $"{modelPath} should keep explicit material remaps.");
        }

        private static void AddImportedPath(ISet<string> importedPaths, string assetPath)
        {
            string normalizedPath = assetPath?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(normalizedPath)
                && normalizedPath.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
            {
                importedPaths.Add(normalizedPath);
            }
        }

        private static Material LoadEmbeddedMaterial(string assetPath)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Material material)
                {
                    return material;
                }
            }

            return null;
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator BalancedDetailCullRestoresRenderersNearTheCamera()
        {
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePaths[1],
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene scene = SceneManager.GetActiveScene();
            Transform mapRoot = FindStageMapRoot(scene);
            Camera camera = FindActiveCamera();
            Assert.That(mapRoot, Is.Not.Null);
            OlympusMobileEnvironmentDetailCuller detailCuller =
                mapRoot.GetComponent<OlympusMobileEnvironmentDetailCuller>();
            Assert.That(detailCuller, Is.Not.Null);
            Assert.That(detailCuller.CulledRendererCount, Is.GreaterThan(100));
            int balancedCulledCount = detailCuller.CulledRendererCount;
            Assert.That(detailCuller.CandidateColliderCount, Is.GreaterThan(0));
            Assert.That(detailCuller.CulledColliderCount, Is.GreaterThan(0));
            int balancedCulledColliderCount = detailCuller.CulledColliderCount;
            Assert.That(
                detailCuller.TryGetFirstColliderOnlyCullForTests(
                    out MeshRenderer colliderOnlyRenderer,
                    out Collider colliderOnlyCollider),
                Is.True,
                "Balanced mobile physics culling should preserve mid-distance visuals while removing their decorative colliders.");
            Assert.That(colliderOnlyRenderer.enabled, Is.True);
            Assert.That(colliderOnlyCollider.enabled, Is.False);
            detailCuller.Configure(mapRoot, camera, MobilePerformanceTier.Low);
            Assert.That(detailCuller.CullDistance, Is.EqualTo(90f).Within(0.001f));
            Assert.That(detailCuller.ColliderCullDistance, Is.EqualTo(32f).Within(0.001f));
            Assert.That(detailCuller.CulledRendererCount, Is.GreaterThanOrEqualTo(balancedCulledCount));
            Assert.That(detailCuller.CulledColliderCount, Is.GreaterThanOrEqualTo(balancedCulledColliderCount));
            Assert.That(
                detailCuller.TryGetFirstCulledColliderForTests(out Collider culledCollider),
                Is.True);
            Assert.That(culledCollider.enabled, Is.False);

            int lowCulledColliderCount = detailCuller.CulledColliderCount;
            Vector3 originalCameraPosition = camera.transform.position;
            try
            {
                camera.transform.position = culledCollider.transform.position;
                Assert.That(detailCuller.RefreshNow(), Is.True);
                Assert.That(culledCollider.enabled, Is.True);

                camera.transform.position = originalCameraPosition;
                Assert.That(detailCuller.RefreshNow(), Is.True);
                Assert.That(culledCollider.enabled, Is.False);
                Assert.That(detailCuller.CulledColliderCount, Is.EqualTo(lowCulledColliderCount));
            }
            finally
            {
                camera.transform.position = originalCameraPosition;
                detailCuller.RefreshNow();
            }

            detailCuller.Configure(mapRoot, camera, MobilePerformanceTier.Balanced);
            Assert.That(detailCuller.CulledRendererCount, Is.EqualTo(balancedCulledCount));
            Assert.That(detailCuller.CulledColliderCount, Is.EqualTo(balancedCulledColliderCount));
            Assert.That(
                detailCuller.TryGetFirstCulledColliderForTests(out Collider balancedCulledCollider),
                Is.True);
            Assert.That(balancedCulledCollider.enabled, Is.False);
            Assert.That(
                detailCuller.TryGetFirstCulledRendererForTests(out MeshRenderer culledRenderer),
                Is.True);

            int initialCulledCount = detailCuller.CulledRendererCount;
            originalCameraPosition = camera.transform.position;
            try
            {
                camera.transform.position = culledRenderer.bounds.center;
                Assert.That(detailCuller.RefreshNow(), Is.True);
                Assert.That(culledRenderer.enabled, Is.True);
                Assert.That(detailCuller.CulledRendererCount, Is.LessThan(initialCulledCount));

                camera.transform.position = originalCameraPosition;
                Assert.That(detailCuller.RefreshNow(), Is.True);
                Assert.That(culledRenderer.enabled, Is.False);
                Assert.That(detailCuller.CulledRendererCount, Is.EqualTo(initialCulledCount));
            }
            finally
            {
                camera.transform.position = originalCameraPosition;
                detailCuller.RefreshNow();
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator BalancedEnvironmentShadowOptimizationStaysWithinVisualBudget()
        {
            string scenePath = ScenePaths[1];
            EditorSceneManager.LoadSceneInPlayMode(
                scenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return PrepareGameplayCameraForVisualBudget();
            DisableDynamicGraphicsForCapture();
            yield return WarmUpCaptureCamera();

            Camera camera = FindActiveCamera();
            DisableCameraMotionForCapture(camera);
            Transform mapRoot = FindStageMapRoot(SceneManager.GetActiveScene());
            Assert.That(mapRoot, Is.Not.Null);
            List<MeshRenderer> environmentRenderers = new(
                mapRoot.GetComponentsInChildren<MeshRenderer>(true));
            List<Light> decorativeLights = FindDecorativeLights();
            Assert.That(environmentRenderers.Count, Is.GreaterThan(1600));
            Assert.That(
                environmentRenderers.FindAll(
                    renderer => renderer.shadowCastingMode != ShadowCastingMode.Off).Count,
                Is.Zero);
            Assert.That(decorativeLights.Count, Is.EqualTo(4));

            Texture2D balancedQuality = null;
            Texture2D highQuality = null;
            try
            {
                SetShadowMode(environmentRenderers, ShadowCastingMode.On);
                SetLightEnabled(decorativeLights, true);
                yield return WaitForCaptureState();
                Texture2D shaderWarmup = CaptureCamera(camera);
                UnityEngine.Object.Destroy(shaderWarmup);
                highQuality = CaptureCamera(camera);
                SetShadowMode(environmentRenderers, ShadowCastingMode.Off);
                SetLightEnabled(decorativeLights, false);
                yield return WaitForCaptureState();
                balancedQuality = CaptureCamera(camera);
            }
            finally
            {
                SetShadowMode(environmentRenderers, ShadowCastingMode.Off);
                SetLightEnabled(decorativeLights, false);
            }

            try
            {
                FrameDifference difference = CompareFrames(highQuality, balancedQuality);
                File.WriteAllBytes(
                    "C:/tmp/DimensionBrawl-OlympusEnvironmentShadow-High.png",
                    highQuality.EncodeToPNG());
                File.WriteAllBytes(
                    "C:/tmp/DimensionBrawl-OlympusEnvironmentShadow-Balanced.png",
                    balancedQuality.EncodeToPNG());
                WriteVisualReport(difference);

                Assert.That(difference.MeanAbsoluteError, Is.LessThan(2d));
                Assert.That(difference.PeakSignalToNoiseRatio, Is.GreaterThan(40d));
            }
            finally
            {
                UnityEngine.Object.Destroy(balancedQuality);
                UnityEngine.Object.Destroy(highQuality);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator BalancedCorridorDistantDetailCullStaysWithinVisualBudget()
        {
            string scenePath = ScenePaths[1];
            EditorSceneManager.LoadSceneInPlayMode(
                scenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return PrepareGameplayCameraForVisualBudget();
            DisableDynamicGraphicsForCapture();
            yield return WarmUpCaptureCamera();

            Camera camera = FindActiveCamera();
            DisableCameraMotionForCapture(camera);
            Transform mapRoot = FindStageMapRoot(SceneManager.GetActiveScene());
            Assert.That(mapRoot, Is.Not.Null);
            OlympusMobileEnvironmentDetailCuller detailCuller =
                mapRoot.GetComponent<OlympusMobileEnvironmentDetailCuller>();
            Assert.That(detailCuller, Is.Not.Null);
            Assert.That(detailCuller.CulledRendererCount, Is.GreaterThan(100));
            int culledRendererCount = detailCuller.CulledRendererCount;
            long culledTriangles = detailCuller.CulledTriangleCount;

            Texture2D highQuality = null;
            Texture2D balancedQuality = null;
            try
            {
                detailCuller.Configure(mapRoot, camera, MobilePerformanceTier.High);
                yield return WaitForCaptureState();
                Texture2D shaderWarmup = CaptureCamera(camera);
                UnityEngine.Object.Destroy(shaderWarmup);
                highQuality = CaptureCamera(camera);
                detailCuller.Configure(mapRoot, camera, MobilePerformanceTier.Balanced);
                yield return WaitForCaptureState();
                balancedQuality = CaptureCamera(camera);
            }
            finally
            {
                detailCuller.Configure(mapRoot, camera, MobilePerformanceTier.Balanced);
            }

            try
            {
                FrameDifference difference = CompareFrames(highQuality, balancedQuality);
                File.WriteAllBytes(
                    "C:/tmp/DimensionBrawl-OlympusDistantDetail-High.png",
                    highQuality.EncodeToPNG());
                File.WriteAllBytes(
                    "C:/tmp/DimensionBrawl-OlympusDistantDetail-Balanced.png",
                    balancedQuality.EncodeToPNG());
                string report = BuildVisualReport(
                    "corridor high detail vs balanced 120m small-detail cull",
                    difference)
                    + $"- Culled renderers: {culledRendererCount:N0}\n"
                    + $"- Culled triangles: {culledTriangles:N0}\n";
                File.WriteAllText(
                    "C:/tmp/DimensionBrawl-OlympusDistantDetailVisualBudget.md",
                    report,
                    Encoding.UTF8);

                Assert.That(difference.MeanAbsoluteError, Is.LessThan(0.4d));
                Assert.That(difference.PeakSignalToNoiseRatio, Is.GreaterThan(40d));
                Assert.That(difference.ExactPixelPercent, Is.GreaterThan(85d));
            }
            finally
            {
                UnityEngine.Object.Destroy(highQuality);
                UnityEngine.Object.Destroy(balancedQuality);
            }
        }

        private static Camera FindActiveCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.enabled && mainCamera.gameObject.activeInHierarchy)
            {
                return mainCamera;
            }

            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].enabled && cameras[i].gameObject.activeInHierarchy)
                {
                    return cameras[i];
                }
            }

            Assert.Fail("Olympus visual budget test needs an active camera.");
            return null;
        }

        private static void DisableDynamicGraphicsForCapture()
        {
            DisableVisualEffectsForCapture();

            SkinnedMeshRenderer[] skinnedRenderers =
                UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                skinnedRenderers[i].enabled = false;
            }
        }

        private static void DisableVisualEffectsForCapture()
        {
            Behaviour[] behaviours = UnityEngine.Object.FindObjectsByType<Behaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().FullName == "UnityEngine.VFX.VisualEffect")
                {
                    behaviour.enabled = false;
                }
            }
        }

        private static void DisableCameraMotionForCapture(Camera camera)
        {
            Behaviour[] behaviours = camera.GetComponents<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                string typeName = behaviour != null ? behaviour.GetType().FullName : null;
                if (typeName == "Unity.Cinemachine.CinemachineBrain"
                    || typeName == "DimensionBrawl.Presentation.ActionCameraController")
                {
                    behaviour.enabled = false;
                }
            }
        }

        private static IEnumerator WarmUpCaptureCamera()
        {
            const int warmupFrames = 15;
            for (int i = 0; i < warmupFrames; i++)
            {
                yield return null;
            }
        }

        private static IEnumerator PrepareGameplayCameraForVisualBudget()
        {
            OlympusCorridorCombatFlowController flow =
                UnityEngine.Object.FindFirstObjectByType<OlympusCorridorCombatFlowController>();
            Assert.That(flow, Is.Not.Null);
            flow.SkipIntroCutscene();
            yield return null;
            yield return null;
        }

        private static IEnumerator WaitForCaptureState()
        {
            yield return null;
            yield return null;
        }

        private static List<Light> FindDecorativeLights()
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            List<Light> decorativeLights = new();
            for (int i = 0; i < lights.Length; i++)
            {
                if (OlympusMobileRenderBudgetBootstrap.IsDecorativeLightName(lights[i].name))
                {
                    decorativeLights.Add(lights[i]);
                }
            }

            return decorativeLights;
        }

        private static Transform FindStageMapRoot(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform root = roots[i].transform;
                if (root.name == "OlympusCorridorStageMap")
                {
                    return root;
                }

                Transform mapRoot = root.Find("OlympusCorridorStageMap");
                if (mapRoot != null)
                {
                    return mapRoot;
                }
            }

            return null;
        }

        private static void SetShadowMode(List<MeshRenderer> renderers, ShadowCastingMode mode)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].shadowCastingMode = mode;
                }
            }
        }

        private static void SetLightEnabled(List<Light> lights, bool enabled)
        {
            if (lights == null)
            {
                return;
            }

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].enabled = enabled;
                }
            }
        }

        private static void CaptureCanonicalStationBoss(Transform bossRoot, Transform visual)
        {
            const int captureLayer = 31;
            bool bossWasActive = bossRoot.gameObject.activeSelf;
            bool visualWasActive = visual.gameObject.activeSelf;
            Transform[] visualTransforms = visual.GetComponentsInChildren<Transform>(includeInactive: true);
            int[] originalLayers = new int[visualTransforms.Length];
            GameObject cameraObject = null;
            GameObject keyLightObject = null;
            GameObject fillLightObject = null;
            try
            {
                bossRoot.gameObject.SetActive(true);
                visual.gameObject.SetActive(true);
                for (int i = 0; i < visualTransforms.Length; i++)
                {
                    originalLayers[i] = visualTransforms[i].gameObject.layer;
                    visualTransforms[i].gameObject.layer = captureLayer;
                }

                Animator animator = visual.GetComponentInChildren<Animator>(includeInactive: true);
                animator?.Update(0f);

                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
                bool hasBounds = false;
                Bounds bounds = default;
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null || !renderer.enabled || renderer.forceRenderingOff)
                    {
                        continue;
                    }

                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }

                Assert.That(hasBounds, Is.True, "Station Commando capture requires visible renderer bounds.");
                float radius = Mathf.Max(1.2f, bounds.extents.magnitude);
                Vector3 viewDirection =
                    (visual.forward * 0.82f + visual.right * 0.58f + Vector3.up * 0.12f).normalized;
                float fieldOfView = 38f;
                float distance = radius / Mathf.Sin(fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.12f;

                cameraObject = new GameObject("StationCommandoArsenalCaptureCamera");
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.018f, 0.024f, 0.038f, 1f);
                camera.cullingMask = 1 << captureLayer;
                camera.fieldOfView = fieldOfView;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = Mathf.Max(50f, distance * 3f);
                camera.allowHDR = true;
                camera.transform.position = bounds.center + viewDirection * distance;
                camera.transform.rotation = Quaternion.LookRotation(
                    bounds.center - camera.transform.position,
                    Vector3.up);

                keyLightObject = new GameObject("StationCommandoArsenalKeyLight");
                Light keyLight = keyLightObject.AddComponent<Light>();
                keyLight.type = LightType.Directional;
                keyLight.color = new Color(0.86f, 0.92f, 1f);
                keyLight.intensity = 1.35f;
                keyLight.cullingMask = 1 << captureLayer;
                keyLight.transform.rotation = camera.transform.rotation;

                fillLightObject = new GameObject("StationCommandoArsenalFillLight");
                Light fillLight = fillLightObject.AddComponent<Light>();
                fillLight.type = LightType.Point;
                fillLight.color = new Color(0.35f, 0.58f, 1f);
                fillLight.intensity = 2.2f;
                fillLight.range = radius * 4f;
                fillLight.cullingMask = 1 << captureLayer;
                fillLight.transform.position =
                    bounds.center - camera.transform.right * radius * 1.15f + Vector3.up * radius * 0.35f;

                Texture2D image = CaptureCamera(camera);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(StationBossCapturePath));
                    File.WriteAllBytes(StationBossCapturePath, image.EncodeToPNG());
                    Color32 background = camera.backgroundColor;
                    Color32[] pixels = image.GetPixels32();
                    int visiblePixelCount = 0;
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        Color32 pixel = pixels[i];
                        int difference = Mathf.Abs(pixel.r - background.r)
                            + Mathf.Abs(pixel.g - background.g)
                            + Mathf.Abs(pixel.b - background.b);
                        if (difference > 30)
                        {
                            visiblePixelCount++;
                        }
                    }

                    Assert.That(
                        visiblePixelCount,
                        Is.GreaterThan(pixels.Length * 0.025f),
                        "Station Commando capture should contain a nonblank boss silhouette and arsenal.");
                }
                finally
                {
                    UnityEngine.Object.Destroy(image);
                }
            }
            finally
            {
                for (int i = 0; i < visualTransforms.Length; i++)
                {
                    if (visualTransforms[i] != null)
                    {
                        visualTransforms[i].gameObject.layer = originalLayers[i];
                    }
                }

                visual.gameObject.SetActive(visualWasActive);
                bossRoot.gameObject.SetActive(bossWasActive);
                UnityEngine.Object.Destroy(cameraObject);
                UnityEngine.Object.Destroy(keyLightObject);
                UnityEngine.Object.Destroy(fillLightObject);
            }
        }

        private static Texture2D CaptureCamera(Camera camera)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(
                CaptureWidth,
                CaptureHeight,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D image = new(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                image.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
                image.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                return image;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
            }
        }

        private static FrameDifference CompareFrames(Texture2D reference, Texture2D optimized)
        {
            Color32[] referencePixels = reference.GetPixels32();
            Color32[] optimizedPixels = optimized.GetPixels32();
            Assert.That(optimizedPixels.Length, Is.EqualTo(referencePixels.Length));

            double absoluteSum = 0d;
            double squaredSum = 0d;
            int exactPixelCount = 0;
            for (int i = 0; i < referencePixels.Length; i++)
            {
                Color32 left = referencePixels[i];
                Color32 right = optimizedPixels[i];
                int redDifference = left.r - right.r;
                int greenDifference = left.g - right.g;
                int blueDifference = left.b - right.b;
                absoluteSum += Math.Abs(redDifference) + Math.Abs(greenDifference) + Math.Abs(blueDifference);
                squaredSum += redDifference * redDifference
                    + greenDifference * greenDifference
                    + blueDifference * blueDifference;
                if (redDifference == 0 && greenDifference == 0 && blueDifference == 0)
                {
                    exactPixelCount++;
                }
            }

            int channelCount = referencePixels.Length * 3;
            double meanAbsoluteError = absoluteSum / channelCount;
            double rootMeanSquaredError = Math.Sqrt(squaredSum / channelCount);
            double peakSignalToNoiseRatio = rootMeanSquaredError <= double.Epsilon
                ? 99d
                : 20d * Math.Log10(255d / rootMeanSquaredError);
            return new FrameDifference(
                meanAbsoluteError,
                rootMeanSquaredError,
                peakSignalToNoiseRatio,
                exactPixelCount * 100d / referencePixels.Length);
        }

        private static void WriteVisualReport(FrameDifference difference)
        {
            File.WriteAllText(
                VisualReportPath,
                BuildVisualReport(
                    "same-frame high environment shadows/lights vs balanced mobile environment budget",
                    difference),
                Encoding.UTF8);
        }

        private static string BuildVisualReport(string comparison, FrameDifference difference)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Olympus Mobile Render Visual Budget");
            builder.AppendLine();
            builder.AppendLine($"- Generated UTC: {DateTime.UtcNow:O}");
            builder.AppendLine($"- Comparison: {comparison}");
            builder.AppendLine($"- Mean absolute error: {difference.MeanAbsoluteError:0.0000} / 255");
            builder.AppendLine($"- Root mean squared error: {difference.RootMeanSquaredError:0.0000} / 255");
            builder.AppendLine($"- PSNR: {difference.PeakSignalToNoiseRatio:0.0000} dB");
            builder.AppendLine($"- Exact RGB pixels: {difference.ExactPixelPercent:0.0000}%");
            return builder.ToString();
        }

        private readonly struct FrameDifference
        {
            public FrameDifference(
                double meanAbsoluteError,
                double rootMeanSquaredError,
                double peakSignalToNoiseRatio,
                double exactPixelPercent)
            {
                MeanAbsoluteError = meanAbsoluteError;
                RootMeanSquaredError = rootMeanSquaredError;
                PeakSignalToNoiseRatio = peakSignalToNoiseRatio;
                ExactPixelPercent = exactPixelPercent;
            }

            public double MeanAbsoluteError { get; }
            public double RootMeanSquaredError { get; }
            public double PeakSignalToNoiseRatio { get; }
            public double ExactPixelPercent { get; }
        }
    }
}
