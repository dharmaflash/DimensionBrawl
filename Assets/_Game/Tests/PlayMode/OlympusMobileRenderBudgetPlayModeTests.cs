using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
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
        private const string CanonicalNoCrossVfxPrefabPath =
            "Assets/_Game/Prefabs/VFX/Environment/PF_OlympusStation_NoCrossRedCubeZone.prefab";
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
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity"
        };

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
            }
        }

        private static void AssertCanonicalStationHudOwnership()
        {
            Behaviour legacyReviewHud = null;
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
                    case "DimensionBrawl.Presentation.BossBarrageLaneReviewHud":
                        legacyReviewHud = behaviour;
                        break;
                    case "DimensionBrawl.UI.CombatHudPresenter":
                        combatHudPresenter = behaviour;
                        break;
                    case "DimensionBrawl.UI.BossBarrageLaneReviewCombatHudBinder":
                        combatHudBinder = behaviour;
                        break;
                }
            }

            Assert.That(legacyReviewHud, Is.Not.Null, "Station should retain the legacy HUD as a data source.");
            Assert.That(legacyReviewHud.enabled, Is.False, "The legacy IMGUI HUD must not render under UGUI.");
            Assert.That(combatHudPresenter, Is.Not.Null);
            Assert.That(combatHudPresenter.isActiveAndEnabled, Is.True, "UGUI should own the visible combat HUD.");
            Assert.That(combatHudBinder, Is.Not.Null);
            Assert.That(combatHudBinder.isActiveAndEnabled, Is.True, "The canonical HUD binder should stay active.");
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
            detailCuller.Configure(mapRoot, camera, MobilePerformanceTier.Low);
            Assert.That(detailCuller.CullDistance, Is.EqualTo(90f).Within(0.001f));
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
            string scenePath = ScenePaths[0];
            EditorSceneManager.LoadSceneInPlayMode(
                scenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            DisableDynamicGraphicsForCapture();
            yield return WarmUpCaptureCamera();

            Camera camera = FindActiveCamera();
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
                Texture2D shaderWarmup = CaptureCamera(camera);
                UnityEngine.Object.Destroy(shaderWarmup);
                highQuality = CaptureCamera(camera);
                SetShadowMode(environmentRenderers, ShadowCastingMode.Off);
                SetLightEnabled(decorativeLights, false);
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
            DisableDynamicGraphicsForCapture();
            yield return WarmUpCaptureCamera();

            Camera camera = FindActiveCamera();
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
                Texture2D shaderWarmup = CaptureCamera(camera);
                UnityEngine.Object.Destroy(shaderWarmup);
                highQuality = CaptureCamera(camera);
                detailCuller.Configure(mapRoot, camera, MobilePerformanceTier.Balanced);
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

                Assert.That(difference.MeanAbsoluteError, Is.LessThan(0.1d));
                Assert.That(difference.PeakSignalToNoiseRatio, Is.GreaterThan(45d));
                Assert.That(difference.ExactPixelPercent, Is.GreaterThan(99d));
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

            SkinnedMeshRenderer[] skinnedRenderers =
                UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                skinnedRenderers[i].enabled = false;
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

        private static Texture2D CaptureCamera(Camera camera)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture target = new(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new(CaptureWidth, CaptureHeight, TextureFormat.RGBA32, false);
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
                UnityEngine.Object.Destroy(target);
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
