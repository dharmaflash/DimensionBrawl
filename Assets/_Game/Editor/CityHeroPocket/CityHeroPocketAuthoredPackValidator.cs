using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.CityHeroPocket
{
    /// <summary>
    /// Fails closed on CITY-GATE-01 asset, scene, combat and look ownership drift.
    /// This validator intentionally admits the direct editor-load proof before a
    /// future StageDefinition/catalog integration exists.
    /// </summary>
    public static class CityHeroPocketAuthoredPackValidator
    {
        public const string TemporaryLilToonShaderDebtRoot =
            "Assets/_Imported/AssetStore/lilToon/Shader/";

        private static readonly string[] ForbiddenTokyoFragments =
        {
            "/Wall_Door_04",
            "/Roof_Wall_04",
            "/Flowers",
            "/Scenes/",
            "/Other/Door.cs",
            "/Other/SimpleCameraController.cs",
            "/Other/Leaves.shader",
            "/Other/Decals.shader",
            "/Other/Terrain.asset",
        };

        [MenuItem("DimensionBrawl/Validate/PV City Hero Pocket Authored Pack")]
        public static void ValidateFromMenu()
        {
            ValidateAuthoredOutputs();
            Debug.Log("[CityHeroPocketAuthoredPackValidator] VALIDATION_PASS");
        }

        public static void RunBatchValidation()
        {
            try
            {
                ValidateAuthoredOutputs();
                Debug.Log("[CityHeroPocketAuthoredPackValidator] BATCH_VALIDATION_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[CityHeroPocketAuthoredPackValidator] BATCH_VALIDATION_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateAuthoredOutputs()
        {
            ValidateAssetContract();
            ValidatePlayerPrefab();
            ValidateSavedDependencyClosure();

            Scene previousActiveScene = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(CityHeroPocketSceneSetup.ScenePath);
            bool openedForValidation = !existing.IsValid() || !existing.isLoaded;
            Scene scene = openedForValidation
                ? EditorSceneManager.OpenScene(
                    CityHeroPocketSceneSetup.ScenePath,
                    OpenSceneMode.Additive)
                : existing;
            try
            {
                Require(SceneManager.SetActiveScene(scene),
                    "Failed to make CityHeroPocket active for scene-owned RenderSettings validation.");
                ValidateLoadedScene(scene);
            }
            finally
            {
                if (previousActiveScene.IsValid()
                    && previousActiveScene.isLoaded
                    && previousActiveScene.handle != scene.handle)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }
                if (openedForValidation && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        public static void ValidateAssetContract()
        {
            Require(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    CityHeroPocketSceneSetup.SourceStationScenePath) != null,
                "Canonical Station player-source scene is missing.");
            Require(CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths.Length == 24,
                "CITY-GATE-01 must retain the exact Door-free Rich24 Tokyo seed set.");
            Require(string.Equals(
                    CityHeroPocketSceneSetup.ComputeTokyoModuleGoldenSha256(),
                    CityHeroPocketSceneSetup.TokyoModuleGoldenSha256,
                    StringComparison.Ordinal),
                $"Tokyo 69-row table drifted from authoritative recipe JSON " +
                $"{CityHeroPocketSceneSetup.LayoutRecipeJsonSha256}.");

            var uniquePaths = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0;
                 i < CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths.Length;
                 i++)
            {
                string path = CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths[i];
                Require(uniquePaths.Add(path), $"Duplicate Tokyo seed path: {path}");
                Require(path.StartsWith(
                        CityHeroPocketSceneSetup.TokyoRoot + "/",
                        StringComparison.Ordinal),
                    $"Tokyo seed escaped the promoted product root: {path}");
                Require(AssetDatabase.LoadAssetAtPath<GameObject>(path) != null,
                    $"Promoted Tokyo seed is missing: {path}");
                for (int fragmentIndex = 0;
                     fragmentIndex < ForbiddenTokyoFragments.Length;
                     fragmentIndex++)
                {
                    Require(path.IndexOf(
                            ForbiddenTokyoFragments[fragmentIndex],
                            StringComparison.OrdinalIgnoreCase) < 0,
                        $"Forbidden Tokyo dependency entered the seed set: {path}");
                }
            }

            Require(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    CityHeroPocketSceneSetup.PlayerPrefabPath) != null,
                $"Compact player prefab is missing: {CityHeroPocketSceneSetup.PlayerPrefabPath}");
            Require(
                AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                    CityHeroPocketSceneSetup.CityLookProfilePath) != null,
                $"Owned city look profile is missing: {CityHeroPocketSceneSetup.CityLookProfilePath}");
            Require(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    CityHeroPocketSceneSetup.ExitPortalPrefabPath) != null,
                $"Promoted City exit portal is missing: " +
                CityHeroPocketSceneSetup.ExitPortalPrefabPath);
            Require(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    CityHeroPocketSceneSetup.ScenePath) != null,
                $"Direct-load city scene is missing: {CityHeroPocketSceneSetup.ScenePath}");
        }

        public static void ValidatePlayerPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CityHeroPocketSceneSetup.PlayerPrefabPath);
            Require(prefab != null, "Compact Inori ranged player prefab is missing.");
            Require(prefab.activeSelf,
                "Compact player prefab root must be authored active.");
            Require(prefab.transform.localPosition.Equals(Vector3.zero)
                    && prefab.transform.localRotation.Equals(Quaternion.identity)
                    && prefab.transform.localScale.Equals(Vector3.one),
                "Compact player prefab root must retain canonical zero/identity/one transform.");
            Require(prefab.GetComponentsInChildren<CombatHealth>(true).Length == 1,
                "Compact player requires one CombatHealth.");
            Require(prefab.GetComponentsInChildren<PlayerMovementController>(true).Length == 1,
                "Compact player requires one PlayerMovementController.");
            Require(prefab.GetComponentsInChildren<PlayerActionController>(true).Length == 1,
                "Compact player requires one PlayerActionController.");
            Require(prefab.GetComponentsInChildren<PlayerCombatTargetSelector>(true).Length == 1,
                "Compact player requires one PlayerCombatTargetSelector.");
            Require(prefab.GetComponentsInChildren<PlayerCombatModeController>(true).Length == 1,
                "Compact player requires one PlayerCombatModeController.");
            Require(prefab.GetComponentsInChildren<PlayerRangedAimController>(true).Length == 1,
                "Compact player requires one PlayerRangedAimController.");
            Require(prefab.GetComponentsInChildren<PlayerRangedBasicAttackAction>(true).Length == 1,
                "Compact player requires one PlayerRangedBasicAttackAction.");
            Require(prefab.GetComponentsInChildren<PlayerLockTargetController>(true).Length == 1,
                "Compact player requires one PlayerLockTargetController.");
            Require(prefab.GetComponentsInChildren<SummonEnergyLadder>(true).Length == 0,
                "Compact city player may not carry the Station summon-energy package.");
            Require(prefab.GetComponentsInChildren<PlayerSummonSlot1Action>(true).Length == 0,
                "Compact city player may not carry summon slot 1.");
            Require(prefab.GetComponentsInChildren<PlayerSupportSummonSlotAction>(true).Length == 0,
                "Compact city player may not carry support summon slots.");
            Require(prefab.GetComponentsInChildren<PlayerSkill1Action>(true).Length == 0,
                "Compact city player may not carry the Station skill package.");
            Require(prefab.GetComponentsInChildren<PlayerSkill1LaserSweepAction>(true).Length == 0,
                "Compact city player may not carry the Station laser skill package.");
            ValidateNoComponentNamespace(prefab, "MagicaCloth2");
            string[] compactDependencies = AssetDatabase.GetDependencies(
                CityHeroPocketSceneSetup.PlayerPrefabPath,
                recursive: true);
            for (int dependencyIndex = 0;
                 dependencyIndex < compactDependencies.Length;
                 dependencyIndex++)
            {
                string dependency = compactDependencies[dependencyIndex].Replace('\\', '/');
                Require(!dependency.StartsWith(
                        "Assets/_Imported/AssetStore/MagicaCloth2/",
                        StringComparison.Ordinal),
                    $"Compact player retained forbidden MagicaCloth2 dependency: {dependency}");
            }

            Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
            Require(animators.Length == 1,
                $"Compact player requires one Inori Animator; found {animators.Length}.");
            Require(string.Equals(
                    AssetDatabase.GetAssetPath(animators[0].runtimeAnimatorController),
                    "Assets/_Game/Art/Animations/Player/RifleGirl/DB_RifleGirl_RangedCandidate.controller",
                    StringComparison.Ordinal),
                "Compact player no longer owns the canonical ranged AnimatorController.");

            PlayerActionController action =
                prefab.GetComponentInChildren<PlayerActionController>(true);
            Require(string.Equals(
                    AssetDatabase.GetAssetPath(action.ActionProfile),
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_PlayerAction_BossBarrageLocalDefense.asset",
                    StringComparison.Ordinal),
                "Compact player no longer owns the reviewed local-defense dodge profile.");
            Require(FindDescendant(prefab.transform, "Inori_RangedVisual") != null,
                "Compact player is missing the promoted Inori ranged visual.");
            Require(FindDescendant(prefab.transform, "CombatGirlPlaceholderBody") == null,
                "Compact player retained the obsolete primitive placeholder.");
            Require(FindDescendant(prefab.transform, "ShortComboSwordProxy") == null,
                "Compact player retained the obsolete sword proxy.");
            Require(FindDescendant(prefab.transform, "CombatGirlSwordShield_PlayerVisual") == null,
                "Compact player retained the superseded melee visual.");
            Require(FindDescendant(
                    prefab.transform,
                    "BossBarrageLaneReview_MeleeWeapons_CombatGirlSwordShield") == null,
                "Compact ranged-only player retained the inactive melee weapon package.");

            PlayerMovementController movement =
                prefab.GetComponentInChildren<PlayerMovementController>(true);
            CharacterController[] capsules =
                prefab.GetComponentsInChildren<CharacterController>(true);
            Require(capsules.Length == 1,
                $"Compact player requires one CharacterController; found {capsules.Length}.");
            CharacterController capsule = capsules[0];
            Require(capsule != null
                    && Mathf.Abs(capsule.radius - 0.45f) <= 0.0001f
                    && Mathf.Abs(capsule.height - 1.8f) <= 0.0001f
                    && (capsule.center - new Vector3(0f, 0.9f, 0f)).sqrMagnitude <= 0.0001f,
                "Compact player CharacterController drifted from the reviewed 0.45/1.8/0.9 capsule.");
            PlayerCombatModeController mode =
                prefab.GetComponentInChildren<PlayerCombatModeController>(true);
            PlayerRangedAimController aim =
                prefab.GetComponentInChildren<PlayerRangedAimController>(true);
            PlayerRangedBasicAttackAction ranged =
                prefab.GetComponentInChildren<PlayerRangedBasicAttackAction>(true);
            Transform rangedVisual = FindDescendant(prefab.transform, "Inori_RangedVisual");
            Transform rangedWeapon = FindDescendant(
                prefab.transform,
                "BossBarrageLaneReview_RangedWeapon_Rifle");
            Require(rangedVisual != null && rangedWeapon != null,
                "Compact player lost its exact ranged visual/weapon roots.");
            RequireSerializedObjectReference(movement, "animator", null,
                "Native Inori presentation requires PlayerMovementController.animator=null.");
            RequireSerializedObjectReference(action, "animator", null,
                "Native Inori presentation requires PlayerActionController.animator=null.");
            RequireSerializedBool(mode, "rangedAnimatorUsesExternalPresentationBridge", true,
                "Inori ranged mode must retain external native Animator bridge ownership.");
            RequireSerializedObjectReference(mode, "combatModeSwapAction", null,
                "Compact ranged-only player may not retain a combat-mode swap action.");
            RequireSerializedBool(mode, "useKeyboardWhenActionMissing", false,
                "Compact ranged-only player may not retain the Station Tab fallback.");
            RequireSerializedObjectReference(mode, "actionController", action,
                "Compact mode controller lost its player action reference.");
            RequireSerializedObjectReference(mode, "movementController", movement,
                "Compact mode controller lost its movement reference.");
            RequireSerializedObjectReference(mode, "rangedAimController", aim,
                "Compact mode controller lost its ranged aim reference.");
            RequireSerializedObjectReference(mode, "rangedBasicAttackAction", ranged,
                "Compact mode controller lost its ranged attack reference.");
            RequireSerializedObjectReference(mode, "rangedVisualRoot", rangedVisual.gameObject,
                "Compact mode controller lost its Inori visual root.");
            RequireSerializedObjectReference(mode, "rangedWeaponRoot", rangedWeapon.gameObject,
                "Compact mode controller lost its rifle root.");
            RequireSerializedObjectReference(mode, "rangedAnimator", animators[0],
                "Compact mode controller lost its Inori Animator.");
            RequireSerializedAssetPath(mode, "rangedAnimatorController",
                "Assets/_Game/Art/Animations/Player/RifleGirl/DB_RifleGirl_RangedCandidate.controller");
            RequireSerializedAssetPath(mode, "rangedActionProfile",
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_PlayerAction_BossBarrageLocalDefense.asset");
            RequireSerializedBool(mode, "routeAnimatorsByMode", true,
                "Compact mode controller must keep its ranged presentation routing.");
            RequireSerializedBool(mode, "useSingleCharacterVisual", true,
                "Compact mode controller must keep one Inori character visual.");
            RequireSerializedEnumValue(mode, "startingMode", (int)PlayerCombatMode.Ranged,
                "Compact player must serialize Ranged as its starting mode.");
            RequireSerializedObjectReference(mode, "meleeVisualRoot", null,
                "Compact ranged-only player retained a melee visual route.");
            RequireSerializedObjectReference(mode, "meleeWeaponRoot", null,
                "Compact ranged-only player retained a melee weapon route.");
            RequireSerializedObjectReference(mode, "meleeAnimator", null,
                "Compact ranged-only player retained a melee Animator route.");
            RequireSerializedObjectReference(mode, "meleeAnimatorController", null,
                "Compact ranged-only player retained a melee controller route.");
            RequireSerializedObjectReference(mode, "meleeActionProfile", null,
                "Compact ranged-only player retained a melee action profile.");
            RequireSerializedObjectReference(aim, "animator", animators[0],
                "Compact ranged aim lost its Inori Animator reference.");
            RequireSerializedString(aim, "aimingParameter", string.Empty,
                "Native bridge owns aiming presentation; generic aimingParameter must stay empty.");
            RequireSerializedObjectReference(ranged, "animator", animators[0],
                "Compact ranged attack lost its Inori Animator reference.");
            RequireSerializedString(ranged, "fireTrigger", string.Empty,
                "Native bridge owns fire presentation; generic fireTrigger must stay empty.");

            Renderer[] visualRenderers =
                rangedVisual.GetComponentsInChildren<Renderer>(true);
            Require(visualRenderers.Length > 0,
                "Compact Inori visual must retain renderers for dodge/hit feedback.");
            PlayerDodgeFeedback[] dodgeFeedbacks =
                prefab.GetComponentsInChildren<PlayerDodgeFeedback>(true);
            CombatHitFeedback[] hitFeedbacks =
                prefab.GetComponentsInChildren<CombatHitFeedback>(true);
            Require(dodgeFeedbacks.Length == 1,
                $"Compact player requires exactly one PlayerDodgeFeedback; " +
                $"found {dodgeFeedbacks.Length}.");
            Require(hitFeedbacks.Length == 1,
                $"Compact player requires exactly one CombatHitFeedback; " +
                $"found {hitFeedbacks.Length}.");
            RequireSerializedRendererArrayExact(
                dodgeFeedbacks[0],
                "targetRenderers",
                rangedVisual,
                visualRenderers,
                "Dodge feedback");
            RequireSerializedRendererArrayExact(
                hitFeedbacks[0],
                "flashRenderers",
                rangedVisual,
                visualRenderers,
                "Hit feedback");

            RifleGirlNativeGameplayAnimatorBridge[] nativeBridges =
                prefab.GetComponentsInChildren<RifleGirlNativeGameplayAnimatorBridge>(true);
            Require(nativeBridges.Length == 1,
                $"Compact player requires exactly one native RifleGirl bridge; found {nativeBridges.Length}.");
            RequireSerializedObjectReference(nativeBridges[0], "animator", animators[0],
                "Native bridge Animator wiring drifted.");
            RequireSerializedObjectReference(nativeBridges[0], "movement", movement,
                "Native bridge movement wiring drifted.");
            RequireSerializedObjectReference(nativeBridges[0], "actionController", action,
                "Native bridge dodge wiring drifted.");
            RequireSerializedObjectReference(nativeBridges[0], "combatModeController", mode,
                "Native bridge combat-mode wiring drifted.");
            RequireSerializedObjectReference(nativeBridges[0], "rangedAimController", aim,
                "Native bridge aim wiring drifted.");
            RequireSerializedObjectReference(nativeBridges[0], "rangedBasicAttackAction", ranged,
                "Native bridge fire wiring drifted.");
            ValidateNoMissingScripts(prefab);
            ValidateRendererMaterials(prefab, "compact player");
            ValidatePrefabObjectReferenceOwnership(prefab);
        }

        private static void ValidateSavedDependencyClosure()
        {
            string[] productDependencies = AssetDatabase.GetDependencies(
                new[]
                {
                    CityHeroPocketSceneSetup.ScenePath,
                    CityHeroPocketSceneSetup.PlayerPrefabPath,
                },
                recursive: true);
            int allowedLilToonDependencyCount = 0;
            for (int i = 0; i < productDependencies.Length; i++)
            {
                string dependency = productDependencies[i].Replace('\\', '/');
                if (!dependency.StartsWith("Assets/_Imported/", StringComparison.Ordinal))
                {
                    continue;
                }
                Require(dependency.StartsWith(
                        TemporaryLilToonShaderDebtRoot,
                        StringComparison.Ordinal),
                    $"CITY-GATE-01 introduced forbidden raw _Imported dependency: {dependency}. " +
                    $"Temporary debt is limited to {TemporaryLilToonShaderDebtRoot}");
                allowedLilToonDependencyCount++;
            }
            Require(allowedLilToonDependencyCount > 0,
                "Expected Inori's documented temporary lilToon shader dependency was not observed; " +
                "update the explicit debt contract instead of silently broadening it.");

            for (int prefabIndex = 0;
                 prefabIndex < CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths.Length;
                 prefabIndex++)
            {
                string prefabPath = CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths[prefabIndex];
                string[] dependencies = AssetDatabase.GetDependencies(prefabPath, recursive: true);
                for (int dependencyIndex = 0;
                     dependencyIndex < dependencies.Length;
                     dependencyIndex++)
                {
                    string dependency = dependencies[dependencyIndex].Replace('\\', '/');
                    bool isPromotedTokyo = dependency.StartsWith(
                        CityHeroPocketSceneSetup.TokyoRoot + "/",
                        StringComparison.Ordinal);
                    bool isPackage = dependency.StartsWith("Packages/", StringComparison.Ordinal);
                    Require(isPromotedTokyo || isPackage,
                        $"Tokyo prefab dependency escaped the promoted CityHeroPocket closure: " +
                        $"{prefabPath} -> {dependency}");
                }
            }
        }

        public static void ValidateLoadedScene(Scene scene)
        {
            Require(scene.IsValid() && scene.isLoaded,
                "City Hero Pocket validation requires a loaded scene.");
            Require(string.Equals(
                    scene.path,
                    CityHeroPocketSceneSetup.ScenePath,
                    StringComparison.Ordinal),
                $"Unexpected City Hero Pocket scene path: {scene.path}");

            GameObject stageRoot = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.StageRootName);
            GameObject mapRoot = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.MapRootName);
            GameObject runtimeRoot = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.RuntimeRootName);
            GameObject playerRoot = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.PlayerRootName);
            GameObject enemyRoot = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.EnemyRootName);
            GameObject hudRoot = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.HudRootName);

            Require(stageRoot.activeInHierarchy,
                "City stage root must be active for direct editor load.");
            Require(mapRoot.transform.IsChildOf(stageRoot.transform),
                "City map must remain owned by the isolated stage root.");
            Require(runtimeRoot.activeInHierarchy
                && playerRoot.activeInHierarchy
                && enemyRoot.activeInHierarchy
                && hudRoot.activeInHierarchy,
                "Direct-load combat roots must all start active.");

            ValidateCamera(scene, playerRoot, enemyRoot);
            ValidateLook(scene);
            ValidateCombat(scene, runtimeRoot, playerRoot, enemyRoot, hudRoot);
            ValidateExitTransition(
                scene,
                runtimeRoot,
                playerRoot,
                enemyRoot,
                hudRoot);
            ValidateTokyoComposition(scene, mapRoot);
            ValidateSceneIntegrity(scene);
        }

        private static void ValidateCamera(
            Scene scene,
            GameObject playerRoot,
            GameObject enemyRoot)
        {
            Camera camera = RequireSingleSceneComponent<Camera>(scene);
            Require(camera.CompareTag("MainCamera"),
                "City gameplay camera must retain MainCamera tag.");
            Require(Mathf.Abs(camera.fieldOfView - 52f) <= 0.001f,
                $"City gameplay camera FOV drifted: {camera.fieldOfView:0.###}");
            Require(Mathf.Abs(camera.nearClipPlane - 0.08f) <= 0.001f,
                $"City gameplay camera near clip drifted: {camera.nearClipPlane:0.###}");
            Require((camera.transform.position - CityHeroPocketSceneSetup.CameraPosition)
                    .sqrMagnitude <= 0.0001f,
                "City gameplay camera G02 start position drifted.");
            Require(camera.allowHDR && camera.allowMSAA,
                "City gameplay camera must preserve HDR and MSAA authoring intent.");
            Require(scene.GetRootGameObjects().Length > 0,
                "City scene contains no roots.");
            Require(CountSceneComponents<AudioListener>(scene) == 1,
                "City scene must own exactly one AudioListener.");
            Require(CountSceneComponents<EventSystem>(scene) == 1,
                "City scene must own exactly one EventSystem.");
            InputSystemUIInputModule inputModule =
                RequireSingleSceneComponent<InputSystemUIInputModule>(scene);
            Require(inputModule.isActiveAndEnabled
                    && CountSceneComponents<BaseInputModule>(scene) == 1
                    && CountSceneComponents<StandaloneInputModule>(scene) == 0,
                "City EventSystem must own one active InputSystem module and no legacy double-dispatch path.");
            Require(inputModule.actionsAsset != null
                    && inputModule.point != null
                    && inputModule.leftClick != null
                    && inputModule.move != null
                    && inputModule.submit != null
                    && inputModule.cancel != null,
                "City EventSystem lost one or more default UI input actions after save/reopen.");

            UniversalAdditionalCameraData cameraData =
                camera.GetComponent<UniversalAdditionalCameraData>();
            Require(cameraData != null && cameraData.renderPostProcessing,
                "City gameplay camera must render its owned post-process profile.");
            Require(cameraData.antialiasing
                    == AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                "City gameplay camera must retain SMAA.");

            ActionCameraController actionCamera =
                RequireSingleSceneComponent<ActionCameraController>(scene);
            Require(ReferenceEquals(actionCamera.gameObject, camera.gameObject),
                "ActionCameraController must live on the single gameplay camera.");
            Require(ReferenceEquals(actionCamera.Target, playerRoot.transform),
                "Action camera target is not the city player.");
            Require(ReferenceEquals(actionCamera.Threat, enemyRoot.transform),
                "Action camera threat is not the city RifleCrossfire enemy.");
            RequireSerializedVector3(actionCamera, "cameraOffset",
                new Vector3(0.85f, 1.25f, -3.8f),
                "Action camera shoulder offset drifted from the G02 follow rig.");
            RequireSerializedVector3(actionCamera, "lookOffset",
                new Vector3(0f, 1.1f, 0f),
                "Action camera pivot height drifted from the G02 follow rig.");
            RequireSerializedBool(actionCamera, "threatFocusAffectsCameraPosition", false,
                "City must use threat focus for look rotation without moving its G02 anchor.");
            RequireSerializedFloat(actionCamera, "threatBias", 0.67f,
                "City look focus drifted from the reviewed player/enemy framing bias.");
            RequireSerializedFloat(actionCamera, "maxThreatFocusOffset", 8.1f,
                "City look focus drifted from the reviewed enemy framing reach.");
            RequireSerializedFloat(actionCamera, "maxLeadFromPlayerSpeed", 0f,
                "City follow-camera position must not pop from an always-on lead offset.");
        }

        private static void ValidateLook(Scene scene)
        {
            Volume[] volumes = FindSceneComponents<Volume>(scene);
            Require(volumes.Length == 1,
                $"City scene requires exactly one Volume owner; found {volumes.Length}.");
            Volume volume = volumes[0];
            Require(string.Equals(
                    volume.name,
                    CityHeroPocketSceneSetup.GlobalVolumeName,
                    StringComparison.Ordinal),
                "City scene Volume owner name drifted.");
            Require(volume.isGlobal
                && Mathf.Abs(volume.priority - 40f) <= 0.001f
                && Mathf.Abs(volume.weight - 1f) <= 0.001f,
                "City GameplayBase Volume must be global priority 40 weight 1.");
            Require(string.Equals(
                    AssetDatabase.GetAssetPath(volume.sharedProfile),
                    CityHeroPocketSceneSetup.CityLookProfilePath,
                    StringComparison.Ordinal),
                "City Volume no longer owns its isolated profile.");
            Require(volume.sharedProfile != null,
                "City Volume profile is missing.");
            ValidateVolumeProfile(volume.sharedProfile);
            ValidateOwnedLitMaterial(
                CityHeroPocketSceneSetup.AsphaltMaterialPath,
                CityHeroPocketSceneSetup.AsphaltColor,
                CityHeroPocketSceneSetup.AsphaltMetallic,
                CityHeroPocketSceneSetup.AsphaltSmoothness);
            ValidateOwnedLitMaterial(
                CityHeroPocketSceneSetup.SidewalkMaterialPath,
                CityHeroPocketSceneSetup.SidewalkColor,
                CityHeroPocketSceneSetup.SidewalkMetallic,
                CityHeroPocketSceneSetup.SidewalkSmoothness);
            ValidateLighting(scene);
        }

        private static void ValidateVolumeProfile(VolumeProfile profile)
        {
            Type[] expectedTypes =
            {
                typeof(Tonemapping),
                typeof(WhiteBalance),
                typeof(ColorAdjustments),
                typeof(LiftGammaGain),
                typeof(Bloom),
                typeof(Vignette),
                typeof(DepthOfField),
            };
            Require(profile.components.Count == expectedTypes.Length,
                $"Owned city Volume profile requires exactly {expectedTypes.Length} overrides; " +
                $"found {profile.components.Count}.");
            var observedTypes = new HashSet<Type>();
            for (int i = 0; i < profile.components.Count; i++)
            {
                VolumeComponent component = profile.components[i];
                Require(component != null && observedTypes.Add(component.GetType()),
                    "Owned city Volume profile contains a null or duplicate override.");
            }
            for (int i = 0; i < expectedTypes.Length; i++)
            {
                Require(observedTypes.Contains(expectedTypes[i]),
                    $"Owned city Volume profile is missing {expectedTypes[i].Name}.");
            }

            Require(profile.TryGet(out Tonemapping tonemapping) && tonemapping.active,
                "City Tonemapping override is missing or inactive.");
            RequireOverrideCount(tonemapping, 1);
            RequireExactOverride(tonemapping.mode, TonemappingMode.Neutral,
                "City Tonemapping must remain Neutral.");

            Require(profile.TryGet(out WhiteBalance whiteBalance) && whiteBalance.active,
                "City WhiteBalance override is missing or inactive.");
            RequireOverrideCount(whiteBalance, 2);
            RequireExactOverride(whiteBalance.temperature, 0f,
                "City WhiteBalance temperature drifted.");
            RequireExactOverride(whiteBalance.tint, 0f,
                "City WhiteBalance tint drifted.");

            Require(profile.TryGet(out ColorAdjustments color) && color.active,
                "City ColorAdjustments override is missing or inactive.");
            RequireOverrideCount(color, 5);
            RequireExactOverride(color.postExposure, 0.22f,
                "City post exposure drifted.");
            RequireExactOverride(color.contrast, -4f,
                "City contrast drifted.");
            RequireExactOverride(color.colorFilter, Color.white,
                "City color filter must remain neutral white.");
            RequireExactOverride(color.hueShift, 0f,
                "City hue shift drifted.");
            RequireExactOverride(color.saturation, 0f,
                "City saturation drifted.");

            Require(profile.TryGet(out LiftGammaGain wheels) && wheels.active,
                "City LiftGammaGain override is missing or inactive.");
            RequireOverrideCount(wheels, 3);
            RequireExactOverride(wheels.lift, new Vector4(1f, 1f, 1f, 0.015f),
                "City lift wheel drifted.");
            RequireExactOverride(wheels.gamma, new Vector4(1f, 1f, 1f, 0f),
                "City gamma wheel drifted.");
            RequireExactOverride(wheels.gain, new Vector4(1f, 1f, 1f, 0f),
                "City gain wheel drifted.");

            Require(profile.TryGet(out Bloom bloom) && bloom.active,
                "City Bloom override is missing or inactive.");
            RequireOverrideCount(bloom, 6);
            RequireExactOverride(bloom.threshold, 0.86f,
                "City Bloom threshold drifted.");
            RequireExactOverride(bloom.intensity, 0.42f,
                "City Bloom intensity drifted.");
            RequireExactOverride(bloom.scatter, 0.76f,
                "City Bloom scatter drifted.");
            RequireExactOverride(bloom.downscale, BloomDownscaleMode.Half,
                "City Bloom downscale mode drifted.");
            RequireExactOverride(bloom.maxIterations, 7,
                "City Bloom iteration count drifted.");
            RequireExactOverride(bloom.highQualityFiltering, true,
                "City Bloom high-quality filtering drifted.");

            Require(profile.TryGet(out Vignette vignette) && vignette.active,
                "City Vignette override is missing or inactive.");
            RequireOverrideCount(vignette, 5);
            RequireExactOverride(vignette.color, Color.black,
                "City Vignette color drifted.");
            RequireExactOverride(vignette.center, new Vector2(0.5f, 0.5f),
                "City Vignette center drifted.");
            RequireExactOverride(vignette.intensity, 0.11f,
                "City Vignette intensity drifted.");
            RequireExactOverride(vignette.smoothness, 0.58f,
                "City Vignette smoothness drifted.");
            RequireExactOverride(vignette.rounded, false,
                "City Vignette rounded flag drifted.");

            Require(profile.TryGet(out DepthOfField depthOfField)
                    && !depthOfField.active,
                "Gameplay Depth of Field must exist but remain disabled in the city proof.");
            RequireOverrideCount(depthOfField, 0);
        }

        private static void ValidateOwnedLitMaterial(
            string path,
            Color expectedColor,
            float expectedMetallic,
            float expectedSmoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Require(material != null
                    && material.shader != null
                    && string.Equals(
                        material.shader.name,
                        "Universal Render Pipeline/Lit",
                        StringComparison.Ordinal),
                $"Owned city material is missing or not canonical URP Lit: {path}");
            Require(material.shaderKeywords.Length == 0,
                $"Owned city material retained stale shader keywords: {path}");
            Require(material.rawRenderQueue == -1,
                $"Owned city material retained a custom render queue: {path}");
            Require(!material.enableInstancing
                    && !material.doubleSidedGI
                    && material.globalIlluminationFlags
                        == MaterialGlobalIlluminationFlags.EmissiveIsBlack,
                $"Owned city material render flags drifted: {path}");
            RequireMaterialColor(material, "_BaseColor", expectedColor, path);
            RequireMaterialColor(material, "_Color", expectedColor, path, required: false);
            RequireMaterialColor(material, "_EmissionColor", Color.black, path);
            RequireMaterialFloat(material, "_Metallic", expectedMetallic, path);
            RequireMaterialFloat(material, "_Smoothness", expectedSmoothness, path);
            RequireMaterialFloat(material, "_WorkflowMode", 1f, path);
            RequireMaterialFloat(material, "_Surface", 0f, path);
            RequireMaterialFloat(material, "_Blend", 0f, path);
            RequireMaterialFloat(material, "_AlphaClip", 0f, path);
            RequireMaterialFloat(material, "_SrcBlend", (float)BlendMode.One, path);
            RequireMaterialFloat(material, "_DstBlend", (float)BlendMode.Zero, path);
            RequireMaterialFloat(material, "_ZWrite", 1f, path);
            RequireMaterialFloat(material, "_Cull", (float)CullMode.Back, path);
            RequireMaterialFloat(material, "_QueueOffset", 0f, path);
            RequireMaterialFloat(material, "_ReceiveShadows", 1f, path);

            string[] textureProperties = material.GetTexturePropertyNames();
            for (int i = 0; i < textureProperties.Length; i++)
            {
                string property = textureProperties[i];
                Require(material.GetTexture(property) == null
                        && (material.GetTextureScale(property) - Vector2.one).sqrMagnitude
                            <= 0.0001f
                        && material.GetTextureOffset(property).sqrMagnitude <= 0.0001f,
                    $"Owned city material retained a stale texture/map transform: " +
                    $"{path}.{property}");
            }
        }

        private static void ValidateLighting(Scene scene)
        {
            Light[] lights = FindSceneComponents<Light>(scene);
            Require(lights.Length == 2,
                $"City look requires exactly two lights; found {lights.Length}.");
            Light key = RequireUniqueSceneObject(scene, "CityHeroPocket_NeutralKey")
                .GetComponent<Light>();
            Light fill = RequireUniqueSceneObject(scene, "CityHeroPocket_NeutralFill")
                .GetComponent<Light>();
            Require(key != null
                    && key.type == LightType.Directional
                    && ColorsApproximately(key.color, Color.white)
                    && Mathf.Abs(key.intensity - 1.28f) <= 0.0001f
                    && key.shadows == LightShadows.Soft
                    && Mathf.Abs(key.shadowStrength - 0.62f) <= 0.0001f
                    && Quaternion.Angle(
                        key.transform.rotation,
                        Quaternion.Euler(46f, -32f, 0f)) <= 0.001f,
                "City neutral key light drifted from its exact authored contract.");
            Require(fill != null
                    && fill.type == LightType.Directional
                    && ColorsApproximately(fill.color, Color.white)
                    && Mathf.Abs(fill.intensity - 0.24f) <= 0.0001f
                    && fill.shadows == LightShadows.None
                    && Quaternion.Angle(
                        fill.transform.rotation,
                        Quaternion.Euler(58f, 142f, 0f)) <= 0.001f,
                "City neutral fill light drifted from its exact authored contract.");
            Require(ReferenceEquals(RenderSettings.sun, key),
                "City RenderSettings.sun no longer references the neutral key.");
            Require(RenderSettings.ambientMode == AmbientMode.Trilight
                    && Mathf.Abs(RenderSettings.ambientIntensity - 1f) <= 0.0001f
                    && ColorsApproximately(
                        RenderSettings.ambientSkyColor,
                        new Color(0.62f, 0.70f, 0.79f))
                    && ColorsApproximately(
                        RenderSettings.ambientEquatorColor,
                        new Color(0.42f, 0.45f, 0.48f))
                    && ColorsApproximately(
                        RenderSettings.ambientGroundColor,
                        new Color(0.19f, 0.20f, 0.22f)),
                "City ambient trilight contract drifted.");
            Require(RenderSettings.fog
                    && RenderSettings.fogMode == FogMode.Linear
                    && ColorsApproximately(
                        RenderSettings.fogColor,
                        new Color(0.58f, 0.66f, 0.73f))
                    && Mathf.Abs(RenderSettings.fogStartDistance - 32f) <= 0.0001f
                    && Mathf.Abs(RenderSettings.fogEndDistance - 105f) <= 0.0001f,
                "City linear fog contract drifted.");
        }

        private static void ValidateCombat(
            Scene scene,
            GameObject runtimeRoot,
            GameObject playerRoot,
            GameObject enemyRoot,
            GameObject hudRoot)
        {
            string playerPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                playerRoot);
            Require(string.Equals(
                    playerPrefabPath,
                    CityHeroPocketSceneSetup.PlayerPrefabPath,
                    StringComparison.Ordinal),
                $"City player is not an instance of the compact product prefab: {playerPrefabPath}");
            Require(playerRoot.transform.position.Equals(
                    CityHeroPocketSceneSetup.PlayerPosition),
                "City player start position drifted.");
            Require(playerRoot.activeSelf
                    && playerRoot.activeInHierarchy
                    && playerRoot.transform.localRotation.Equals(Quaternion.identity)
                    && playerRoot.transform.localScale.Equals(Vector3.one),
                "City player instance must remain active with identity rotation/unit scale.");
            Require((enemyRoot.transform.position - CityHeroPocketSceneSetup.EnemyPosition)
                    .sqrMagnitude <= 0.0001f,
                "City enemy start position drifted.");

            CombatHealth playerHealth = RequireSingle<CombatHealth>(playerRoot);
            CombatHealth enemyHealth = RequireSingle<CombatHealth>(enemyRoot);
            Require(playerHealth.Team == DamageTeam.Player
                && enemyHealth.Team == DamageTeam.Enemy,
                "City combatants must retain hostile player/enemy teams.");
            Require(Mathf.Abs(playerHealth.MaxHealth - 480f) <= 0.001f
                    && Mathf.Abs(enemyHealth.MaxHealth - 90f) <= 0.001f,
                "City combatant authored max-health values did not survive save/reopen.");

            PlayerMovementController movement =
                RequireSingle<PlayerMovementController>(playerRoot);
            CharacterController capsule = RequireSingle<CharacterController>(playerRoot);
            Require(Mathf.Abs(capsule.radius - 0.45f) <= 0.0001f
                    && Mathf.Abs(capsule.height - 1.8f) <= 0.0001f
                    && (capsule.center - new Vector3(0f, 0.9f, 0f)).sqrMagnitude <= 0.0001f,
                "City player CharacterController capsule drifted after scene save/reopen.");
            PlayerCombatTargetSelector selector =
                RequireSingle<PlayerCombatTargetSelector>(playerRoot);
            PlayerActionController action =
                RequireSingle<PlayerActionController>(playerRoot);
            PlayerCombatModeController mode =
                RequireSingle<PlayerCombatModeController>(playerRoot);
            PlayerRangedAimController aim =
                RequireSingle<PlayerRangedAimController>(playerRoot);
            PlayerRangedBasicAttackAction ranged =
                RequireSingle<PlayerRangedBasicAttackAction>(playerRoot);
            PlayerLockTargetController lockTarget =
                RequireSingle<PlayerLockTargetController>(playerRoot);
            Animator playerAnimator = RequireSingle<Animator>(playerRoot);
            RifleGirlNativeGameplayAnimatorBridge nativeBridge =
                RequireSingle<RifleGirlNativeGameplayAnimatorBridge>(playerRoot);
            Require(movement.LaneSpace == null,
                "City player must not retain Station lane-space ownership.");
            RequireSerializedObjectReference(movement, "animator", null,
                "City movement must defer animation to the native RifleGirl bridge.");
            RequireSerializedObjectReference(action, "animator", null,
                "City dodge/action must defer animation to the native RifleGirl bridge.");
            Require(mode.IsRangedMode,
                "City player must start in ranged combat mode.");
            RequireSerializedObjectReference(mode, "combatModeSwapAction", null,
                "City scene player regained a combat-mode swap action.");
            RequireSerializedBool(mode, "useKeyboardWhenActionMissing", false,
                "City scene player regained the Station Tab swap fallback.");
            RequireSerializedBool(mode, "rangedAnimatorUsesExternalPresentationBridge", true,
                "City scene player lost native ranged presentation ownership.");
            RequireSerializedEnumValue(mode, "startingMode", (int)PlayerCombatMode.Ranged,
                "City scene player no longer serializes Ranged as its starting mode.");
            RequireSerializedObjectReference(mode, "meleeVisualRoot", null,
                "City scene player regained a melee visual route.");
            RequireSerializedObjectReference(mode, "meleeWeaponRoot", null,
                "City scene player regained a melee weapon route.");
            RequireSerializedObjectReference(mode, "meleeAnimator", null,
                "City scene player regained a melee Animator route.");
            RequireSerializedObjectReference(mode, "meleeAnimatorController", null,
                "City scene player regained a melee controller route.");
            RequireSerializedObjectReference(mode, "meleeActionProfile", null,
                "City scene player regained a melee action profile.");
            RequireSerializedObjectReference(aim, "animator", playerAnimator,
                "City ranged aim lost its Inori Animator reference.");
            RequireSerializedString(aim, "aimingParameter", string.Empty,
                "City generic aimingParameter must stay empty under the native bridge.");
            RequireSerializedObjectReference(ranged, "animator", playerAnimator,
                "City ranged fire lost its Inori Animator reference.");
            RequireSerializedString(ranged, "fireTrigger", string.Empty,
                "City generic fireTrigger must stay empty under the native bridge.");
            RequireSerializedObjectReference(nativeBridge, "animator", playerAnimator,
                "City native bridge lost its Inori Animator reference.");
            RequireSerializedObjectReference(nativeBridge, "movement", movement,
                "City native bridge lost its movement reference.");
            RequireSerializedObjectReference(nativeBridge, "actionController", action,
                "City native bridge lost its dodge reference.");
            RequireSerializedObjectReference(nativeBridge, "combatModeController", mode,
                "City native bridge lost its mode reference.");
            RequireSerializedObjectReference(nativeBridge, "rangedAimController", aim,
                "City native bridge lost its aim reference.");
            RequireSerializedObjectReference(nativeBridge, "rangedBasicAttackAction", ranged,
                "City native bridge lost its fire reference.");
            Require(selector.SelfHealth == playerHealth
                && selector.TargetCandidateCount == 1,
                "City player selector must own one explicit hostile target.");
            Require(string.Equals(
                    AssetDatabase.GetAssetPath(action.ActionProfile),
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_PlayerAction_BossBarrageLocalDefense.asset",
                    StringComparison.Ordinal),
                "City dodge profile drifted from the reviewed local-defense contract.");
            Require(aim != null && lockTarget != null,
                "City ranged aim/lock package is incomplete.");
            Require(ranged.ProjectileRoot != null
                && string.Equals(
                    ranged.ProjectileRoot.name,
                    CityHeroPocketSceneSetup.PlayerProjectileRootName,
                    StringComparison.Ordinal)
                && !ranged.ProjectileRoot.IsChildOf(playerRoot.transform)
                && ranged.ProjectileRoot.IsChildOf(runtimeRoot.transform),
                "Player projectiles require an independent runtime-owned scene root.");
            Require(ranged.FireOrigin != null
                && ranged.FireOrigin.IsChildOf(playerRoot.transform),
                "Player ranged fire origin must remain on the Inori/rifle hierarchy.");
            RequireSerializedAssetPath(
                ranged,
                "projectilePrefabObject",
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab");

            BasicSoldierEnemy soldier = RequireSingle<BasicSoldierEnemy>(enemyRoot);
            CombatTargetSensor sensor = RequireSingle<CombatTargetSensor>(enemyRoot);
            BasicSoldierProjectileAttackDriver enemyProjectiles =
                RequireSingle<BasicSoldierProjectileAttackDriver>(enemyRoot);
            Require(enemyProjectiles.IsConfiguredFor(soldier, enemyHealth, sensor),
                "RifleCrossfire projectile driver lost its soldier/health/sensor/pool contract.");
            Require(enemyProjectiles.ProjectileOrigin != null
                    && enemyProjectiles.ProjectileOrigin.IsChildOf(enemyRoot.transform),
                "RifleCrossfire projectile origin must remain inside the enemy hierarchy.");
            Require(enemyProjectiles.ProjectilePoolRoot != null
                    && enemyProjectiles.ProjectilePoolRoot.IsChildOf(enemyRoot.transform),
                "RifleCrossfire authored pool must remain inside the enemy hierarchy.");
            Require(string.Equals(
                    AssetDatabase.GetAssetPath(enemyProjectiles.ProjectilePrefab),
                    "Assets/_Game/Prefabs/Combat/PF_EnemyProjectile_RifleCrossfire.prefab",
                    StringComparison.Ordinal),
                "RifleCrossfire projectile driver lost its reviewed projectile prefab.");
            Require(sensor.ContainsTargetCandidate(playerHealth),
                "RifleCrossfire sensor does not own the city player candidate.");
            float planarDistance = Vector3.ProjectOnPlane(
                playerRoot.transform.position - enemyRoot.transform.position,
                Vector3.up).magnitude;
            Require(sensor.SearchRadius <= 0f
                || planarDistance <= sensor.SearchRadius + 0.001f,
                $"RifleCrossfire starts outside sensor range ({planarDistance:0.###}m > {sensor.SearchRadius:0.###}m).");
            Require(ReferenceEquals(soldier.SelfHealth, enemyHealth),
                "RifleCrossfire soldier health wiring drifted.");
            RequireSerializedObjectReference(soldier, "target", playerRoot.transform,
                "RifleCrossfire target Transform did not survive save/reopen.");
            RequireSerializedObjectReference(soldier, "targetHealth", playerHealth,
                "RifleCrossfire target health did not survive save/reopen.");

            CityHeroPocketEnemyProjectileRootBinder projectileBinder =
                RequireSingleSceneComponent<CityHeroPocketEnemyProjectileRootBinder>(scene);
            Require(projectileBinder.IsConfigured
                    && ReferenceEquals(projectileBinder.Driver, enemyProjectiles)
                    && projectileBinder.ProjectileRoot != null
                    && string.Equals(
                        projectileBinder.ProjectileRoot.name,
                        CityHeroPocketSceneSetup.EnemyProjectileRootName,
                        StringComparison.Ordinal),
                "Serialized city enemy projectile-root binder did not survive save/reopen.");
            projectileBinder.ApplyBinding();
            Require(enemyProjectiles.RuntimeProjectileRoot != null
                && string.Equals(
                    enemyProjectiles.RuntimeProjectileRoot.name,
                    CityHeroPocketSceneSetup.EnemyProjectileRootName,
                    StringComparison.Ordinal)
                && enemyProjectiles.HasIndependentRuntimeProjectileRoot
                && enemyProjectiles.RuntimeProjectileRoot.IsChildOf(runtimeRoot.transform),
                "Enemy projectiles require an independent runtime-owned scene root.");

            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(scene);
            Require(ReferenceEquals(encounter.PlayerHealth, playerHealth)
                && ReferenceEquals(encounter.EnemyHealth, enemyHealth)
                && encounter.UsesCoordinatedTerminalResolution,
                "Direct-load city encounter combatant wiring drifted.");
            CombatHudPresenter hudPresenter = RequireSingle<CombatHudPresenter>(hudRoot);
            CombatHudInputBridge inputBridge = RequireSingle<CombatHudInputBridge>(hudRoot);
            CombatHudVirtualJoystick joystick =
                RequireSingle<CombatHudVirtualJoystick>(hudRoot);
            OneRowCombatHudBinder hudBinder = RequireSingle<OneRowCombatHudBinder>(hudRoot);
            Require(hudRoot.GetComponentsInChildren<BossBarrageLaneReviewCombatHudBinder>(true)
                    .Length == 0,
                "City HUD retained Station-specific route ownership.");
            CombatSessionOverlayPresenter sessionOverlay =
                RequireSingle<CombatSessionOverlayPresenter>(hudRoot);
            RectTransform joystickKnob =
                FindDescendant(hudRoot.transform, "MoveJoystickKnob") as RectTransform;
            Require(joystickKnob != null,
                "City HUD is missing its reviewed virtual-joystick knob.");
            RequireSerializedObjectReference(inputBridge, "presenter", hudPresenter,
                "City HUD input bridge lost its presenter reference.");
            RequireSerializedObjectReference(joystick, "movementController", movement,
                "City HUD joystick lost its player movement reference.");
            RequireSerializedObjectReference(joystick, "knob", joystickKnob,
                "City HUD joystick lost its knob reference.");
            RequireSerializedObjectReference(hudBinder, "hudPresenter", hudPresenter,
                "City HUD binder lost its presenter reference.");
            RequireSerializedObjectReference(hudBinder, "inputBridge", inputBridge,
                "City HUD binder lost its input bridge reference.");
            RequireSerializedObjectReference(hudBinder, "moveJoystick", joystick,
                "City HUD binder lost its joystick reference.");
            RequireSerializedObjectReference(hudBinder, "sessionOverlayBehaviour", sessionOverlay,
                "City HUD binder lost its session overlay reference.");
            RequireSerializedObjectReference(hudBinder, "encounterController", encounter,
                "City HUD binder lost its encounter reference.");
            RequireSerializedObjectReference(hudBinder, "playerHealth", playerHealth,
                "City HUD binder lost its player health reference.");
            RequireSerializedObjectReference(hudBinder, "bossHealth", enemyHealth,
                "City HUD binder lost its enemy health reference.");
            RequireSerializedObjectReference(hudBinder, "movementController", movement,
                "City HUD binder lost its movement reference.");
            RequireSerializedObjectReference(hudBinder, "actionController", action,
                "City HUD binder lost its dodge reference.");
            RequireSerializedObjectReference(hudBinder, "combatModeController", mode,
                "City HUD binder lost its ranged-mode reference.");
            RequireSerializedObjectReference(hudBinder, "rangedBasicAttackAction", ranged,
                "City HUD binder lost its ranged-fire reference.");
            RequireSerializedObjectReference(hudBinder, "skill1Action", null,
                "City HUD binder retained removed Skill1 ownership.");
            RequireSerializedObjectReference(hudBinder, "summonSlot1Action", null,
                "City HUD binder retained removed Summon1 ownership.");
            RequireSerializedObjectReference(hudBinder, "summonSlot2Action", null,
                "City HUD binder retained removed Summon2 ownership.");
            RequireSerializedObjectReference(hudBinder, "summonSlot3Action", null,
                "City HUD binder retained removed Summon3 ownership.");
            RequireSerializedString(hudBinder, "objectiveText",
                CityHeroPocketSceneSetup.ProductObjectiveText,
                "City HUD objective copy drifted.");
            RequireSerializedObjectReference(sessionOverlay, "retryButton", null,
                "Direct-load City proof must not route Retry to the shared Corridor route.");
            Transform retryTransform = FindDescendant(hudRoot.transform, "RetryButton");
            Require(retryTransform == null || !retryTransform.gameObject.activeSelf,
                "Direct-load City Retry control must remain hidden until it owns a city reload route.");
            RequireHudAction(hudRoot, "BasicAttackButton", CombatHudActionId.BasicAttack,
                sendHoldState: true, interactable: true);
            RequireHudAction(hudRoot, "DodgeButton", CombatHudActionId.Dodge,
                sendHoldState: false, interactable: true);
            RequireHudAction(hudRoot, "PauseButton", CombatHudActionId.Pause,
                sendHoldState: false, interactable: true);
            RequireUnavailableHudAction(hudRoot, "Skill1Button");
            RequireUnavailableHudAction(hudRoot, "UltimateButton");
            RequireUnavailableHudAction(hudRoot, "SummonSlot1Button");
            RequireUnavailableHudAction(hudRoot, "SummonSlot2Button");
            RequireUnavailableHudAction(hudRoot, "SummonSlot3Button");
            ValidateAimDragArea(
                hudRoot,
                movement,
                mode,
                aim,
                ranged,
                RequireSingleSceneComponent<ActionCameraController>(scene));
        }

        private static void ValidateExitTransition(
            Scene scene,
            GameObject runtimeRoot,
            GameObject playerRoot,
            GameObject enemyRoot,
            GameObject hudRoot)
        {
            Require(CityHeroPocketExitTransitionController.HudFadeFrameCount == 18
                    && CityHeroPocketExitTransitionController.PortalGrowFrameCount == 42
                    && CityHeroPocketExitTransitionController.CoverFadeStartFrame == 234
                    && CityHeroPocketExitTransitionController.ExitReadyFrame == 294,
                "City exit fixed-frame presentation contract drifted.");
            Require(Mathf.Abs(
                    CityHeroPocketExitTransitionController.InitialPortalScaleFactor - 0.08f)
                    <= 0.0001f,
                "City exit portal must start at exactly 0.08 of authored scale.");

            GameObject triggerObject = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.ExitTriggerName);
            GameObject focusObject = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.TransitionFocusName);
            GameObject portalObject = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.ExitPortalRootName);
            GameObject coverObject = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.ExitCoverRootName);
            GameObject dodgeBeatAnchor = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.DodgeBeatAnchorName);
            GameObject reserveEnemyAnchor = RequireUniqueSceneObject(
                scene,
                CityHeroPocketSceneSetup.ReserveEnemyAnchorName);

            Require(triggerObject.transform.IsChildOf(runtimeRoot.transform)
                    && focusObject.transform.IsChildOf(runtimeRoot.transform)
                    && portalObject.transform.IsChildOf(runtimeRoot.transform)
                    && dodgeBeatAnchor.transform.IsChildOf(runtimeRoot.transform)
                    && reserveEnemyAnchor.transform.IsChildOf(runtimeRoot.transform),
                "City exit trigger, portal and capture anchors must stay runtime-owned.");
            Require(coverObject.transform.parent == null && coverObject.activeInHierarchy,
                "City exit cover must remain an active scene-owned overlay root.");
            Require((triggerObject.transform.localPosition
                        - CityHeroPocketSceneSetup.ExitTriggerPosition).sqrMagnitude
                    <= 0.0001f,
                "City exit trigger position drifted.");
            Require((focusObject.transform.localPosition
                        - CityHeroPocketSceneSetup.TransitionFocusPosition).sqrMagnitude
                    <= 0.0001f,
                "City transition focus position drifted.");
            Require((portalObject.transform.localPosition
                        - CityHeroPocketSceneSetup.TransitionFocusPosition).sqrMagnitude
                    <= 0.0001f
                    && Quaternion.Angle(
                        portalObject.transform.localRotation,
                        Quaternion.Euler(CityHeroPocketSceneSetup.ExitPortalEuler))
                        <= 0.001f,
                "City exit portal focus transform drifted.");
            Require((dodgeBeatAnchor.transform.localPosition
                        - CityHeroPocketSceneSetup.DodgeBeatAnchorPosition).sqrMagnitude
                    <= 0.0001f
                    && (reserveEnemyAnchor.transform.localPosition
                        - CityHeroPocketSceneSetup.ReserveEnemyAnchorPosition).sqrMagnitude
                    <= 0.0001f,
                "City capture beat anchors drifted.");

            BoxCollider trigger = RequireSingle<BoxCollider>(triggerObject);
            Require(trigger.isTrigger
                    && (trigger.size - CityHeroPocketSceneSetup.ExitTriggerSize)
                        .sqrMagnitude <= 0.0001f
                    && (trigger.center - CityHeroPocketSceneSetup.ExitTriggerCenter)
                        .sqrMagnitude <= 0.0001f,
                "City exit requires the reviewed, road-separated 10.8x2x0.6 trigger volume.");
            Rigidbody triggerBody = RequireSingle<Rigidbody>(triggerObject);
            Require(triggerBody.isKinematic
                    && !triggerBody.useGravity
                    && triggerBody.collisionDetectionMode == CollisionDetectionMode.Discrete,
                "City exit trigger requires its deterministic kinematic Rigidbody.");

            string portalPrefabPath =
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(portalObject);
            Require(string.Equals(
                    portalPrefabPath,
                    CityHeroPocketSceneSetup.ExitPortalPrefabPath,
                    StringComparison.Ordinal),
                $"City exit portal lost promoted prefab ownership: {portalPrefabPath}");
            Vector3 expectedInitialPortalScale =
                CityHeroPocketSceneSetup.ExitPortalAuthoredScale
                * CityHeroPocketExitTransitionController.InitialPortalScaleFactor;
            Require(!portalObject.activeSelf
                    && (portalObject.transform.localScale - expectedInitialPortalScale)
                        .sqrMagnitude <= 0.0001f,
                "City exit portal must save inactive at 0.08 authored scale.");
            ParticleSystem[] particles =
                portalObject.GetComponentsInChildren<ParticleSystem>(true);
            Require(particles.Length > 0,
                "City exit portal contains no particle systems.");
            for (int i = 0; i < particles.Length; i++)
            {
                Require(!particles[i].useAutoRandomSeed
                        && particles[i].randomSeed
                            == CityHeroPocketExitTransitionController.FirstParticleRandomSeed
                                + (uint)i,
                    $"City exit particle {i} lost deterministic seed ownership.");
            }

            Canvas coverCanvas = RequireSingle<Canvas>(coverObject);
            CanvasGroup coverGroup = RequireSingle<CanvasGroup>(coverObject);
            Image coverImage = RequireSingle<Image>(coverObject);
            Require(coverCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    && coverCanvas.sortingOrder == 32000,
                "City exit cover must stay above the gameplay HUD as ScreenSpaceOverlay.");
            Require(Mathf.Abs(coverGroup.alpha) <= 0.0001f
                    && !coverGroup.interactable
                    && !coverGroup.blocksRaycasts,
                "City exit cover must save transparent and non-interactive.");
            Require(!coverImage.raycastTarget
                    && coverImage.color == CityHeroPocketSceneSetup.ExitCoverColor
                    && coverImage.transform is RectTransform coverRect
                    && coverRect.anchorMin == Vector2.zero
                    && coverRect.anchorMax == Vector2.one
                    && coverRect.anchoredPosition.sqrMagnitude <= 0.0001f
                    && coverRect.sizeDelta.sqrMagnitude <= 0.0001f,
                "City exit cyan-white full-cover image drifted.");

            CityHeroPocketExitTransitionController transition =
                RequireSingleSceneComponent<CityHeroPocketExitTransitionController>(scene);
            CharacterController playerController =
                RequireSingle<CharacterController>(playerRoot);
            CombatEncounterController encounter =
                RequireSingleSceneComponent<CombatEncounterController>(scene);
            PlayerMovementController movement =
                RequireSingle<PlayerMovementController>(playerRoot);
            PlayerActionController action =
                RequireSingle<PlayerActionController>(playerRoot);
            PlayerCombatModeController mode =
                RequireSingle<PlayerCombatModeController>(playerRoot);
            PlayerRangedBasicAttackAction ranged =
                RequireSingle<PlayerRangedBasicAttackAction>(playerRoot);
            BasicSoldierEnemy enemyAi = RequireSingle<BasicSoldierEnemy>(enemyRoot);
            BasicSoldierProjectileAttackDriver enemyProjectileDriver =
                RequireSingle<BasicSoldierProjectileAttackDriver>(enemyRoot);
            CanvasGroup hudGroup = hudRoot.GetComponent<CanvasGroup>();

            Require(ReferenceEquals(transition.gameObject, triggerObject)
                    && transition.IsConfigured
                    && ReferenceEquals(transition.Encounter, encounter)
                    && ReferenceEquals(transition.PlayerController, playerController)
                    && ReferenceEquals(transition.ExitTrigger, trigger)
                    && ReferenceEquals(transition.TransitionFocus, focusObject.transform)
                    && ReferenceEquals(transition.PortalRoot, portalObject.transform)
                    && ReferenceEquals(transition.HudCanvasGroup, hudGroup)
                    && ReferenceEquals(transition.CoverCanvasGroup, coverGroup)
                    && ReferenceEquals(transition.PlayerMovement, movement)
                    && ReferenceEquals(transition.PlayerAction, action)
                    && ReferenceEquals(transition.PlayerCombatMode, mode)
                    && ReferenceEquals(transition.PlayerRangedAttack, ranged)
                    && ReferenceEquals(transition.EnemyAi, enemyAi)
                    && ReferenceEquals(
                        transition.EnemyProjectileDriver,
                        enemyProjectileDriver),
                "City exit controller serialized reference contract drifted.");
            Require((transition.PortalAuthoredScale
                        - CityHeroPocketSceneSetup.ExitPortalAuthoredScale).sqrMagnitude
                    <= 0.0001f,
                "City exit controller lost authored portal scale.");
            Require(!transition.IsArmed
                    && !transition.IsTransitionRunning
                    && !transition.IsHudHidden
                    && !transition.IsFullCover
                    && !transition.IsExitReady
                    && !transition.IsInputLocked
                    && !transition.IsAiLocked
                    && transition.PresentationFrame == 0
                    && transition.RejectedTriggerEnterCount == 0
                    && transition.TriggerAcceptedCount == 0
                    && transition.TransitionStartedCount == 0
                    && transition.HudHiddenCount == 0
                    && transition.FullCoverCount == 0
                    && transition.ExitReadyCount == 0,
                "City exit controller must save in its exact pre-Won restart state.");
        }

        private static void ValidateAimDragArea(
            GameObject hudRoot,
            PlayerMovementController movement,
            PlayerCombatModeController mode,
            PlayerRangedAimController aim,
            PlayerRangedBasicAttackAction ranged,
            ActionCameraController actionCamera)
        {
            CombatHudAimDragInput[] inputs =
                hudRoot.GetComponentsInChildren<CombatHudAimDragInput>(true);
            Require(inputs.Length == 1,
                $"City HUD requires exactly one full-screen AimDragArea; found {inputs.Length}.");
            CombatHudAimDragInput input = inputs[0];
            Require(string.Equals(input.name, "AimDragArea", StringComparison.Ordinal),
                "City HUD touch-aim owner must be named AimDragArea.");
            Require(input.transform is RectTransform rect
                    && rect.anchorMin == Vector2.zero
                    && rect.anchorMax == Vector2.one
                    && rect.anchoredPosition.sqrMagnitude <= 0.0001f
                    && rect.sizeDelta.sqrMagnitude <= 0.0001f
                    && rect.GetSiblingIndex() == 0,
                "AimDragArea must be full-screen and behind actionable HUD controls.");
            Image image = input.GetComponent<Image>();
            Require(image != null && image.raycastTarget && image.color.a <= 0.001f,
                "AimDragArea requires a transparent raycast-target Image.");
            RequireSerializedObjectReference(input, "movementController", movement,
                "AimDragArea movement wiring did not survive save/reopen.");
            RequireSerializedObjectReference(input, "combatModeController", mode,
                "AimDragArea combat-mode wiring did not survive save/reopen.");
            RequireSerializedObjectReference(input, "aimController", aim,
                "AimDragArea aim wiring did not survive save/reopen.");
            RequireSerializedObjectReference(input, "rangedBasicAttackAction", ranged,
                "AimDragArea ranged-fire wiring did not survive save/reopen.");
            if (actionCamera != null)
            {
                RequireSerializedObjectReference(input, "cameraController", actionCamera,
                    "AimDragArea action-camera wiring did not survive save/reopen.");
            }
        }

        private static void ValidateTokyoComposition(Scene scene, GameObject mapRoot)
        {
            Transform modules = FindDescendant(
                mapRoot.transform,
                "TokyoStreet_CuratedHeroBlock");
            Require(modules != null,
                "City map is missing the curated Tokyo hero-block root.");

            var representedPaths = new HashSet<string>(StringComparer.Ordinal);
            var representedIds = new HashSet<string>(StringComparer.Ordinal);
            IReadOnlyList<CityHeroPocketSceneSetup.ModuleSpec> recipe =
                CityHeroPocketSceneSetup.ReviewedTokyoModuleSpecs;
            Require(recipe.Count == CityHeroPocketSceneSetup.TokyoModuleInstanceCount,
                "Reviewed Tokyo module-spec table no longer contains exactly 69 rows.");
            Require(modules.childCount == CityHeroPocketSceneSetup.TokyoModuleInstanceCount,
                $"City hero block requires exactly {CityHeroPocketSceneSetup.TokyoModuleInstanceCount} " +
                $"recipe instances; found {modules.childCount}.");
            for (int i = 0; i < modules.childCount; i++)
            {
                GameObject candidate = modules.GetChild(i).gameObject;
                CityHeroPocketSceneSetup.ModuleSpec spec = recipe[i];
                Require(representedIds.Add(candidate.name),
                    $"Tokyo recipe contains duplicate instance id '{candidate.name}'.");
                Require(string.Equals(candidate.name, spec.Id, StringComparison.Ordinal),
                    $"Tokyo recipe row {i} expected id '{spec.Id}', found '{candidate.name}'.");
                Require(PrefabUtility.IsAnyPrefabInstanceRoot(candidate),
                    $"Tokyo recipe object '{candidate.name}' is not a prefab instance root.");
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(candidate);
                Require(!string.IsNullOrWhiteSpace(path),
                    $"Tokyo recipe object '{candidate.name}' lost its source prefab path.");
                Require(string.Equals(path, spec.PrefabPath, StringComparison.Ordinal),
                    $"Tokyo recipe '{spec.Id}' expected prefab '{spec.PrefabPath}', found '{path}'.");
                Require((candidate.transform.localPosition - spec.Position).sqrMagnitude
                        <= 0.000001f,
                    $"Tokyo recipe '{spec.Id}' local position drifted.");
                Require(Quaternion.Angle(
                            candidate.transform.localRotation,
                            Quaternion.Euler(spec.Euler)) <= 0.001f,
                    $"Tokyo recipe '{spec.Id}' local rotation drifted.");
                Require((candidate.transform.localScale - spec.Scale).sqrMagnitude
                        <= 0.000001f,
                    $"Tokyo recipe '{spec.Id}' local scale drifted.");
                representedPaths.Add(path);
            }

            for (int i = 0;
                 i < CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths.Length;
                 i++)
            {
                string path = CityHeroPocketSceneSetup.RequiredTokyoPrefabPaths[i];
                Require(representedPaths.Contains(path),
                    $"Rich24 Tokyo seed is promoted but absent from the product composition: {path}");
            }

            Collider[] packageColliders = modules.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < packageColliders.Length; i++)
            {
                Collider collider = packageColliders[i];
                if (!collider.enabled || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }
                Require(!IntersectsLowCombatLane(collider.bounds),
                    $"Enabled Tokyo collider '{GetHierarchyPath(collider.transform)}' " +
                    "intrudes into X[-6,6], Z[-9,9] below Y=2.2m.");
                if (!collider.isTrigger)
                {
                    Require(!IntersectsReviewedCameraSweep(collider.bounds),
                        $"Solid Tokyo collider '{GetHierarchyPath(collider.transform)}' " +
                        "violates the reviewed 0.25m camera-sphere clearance inside " +
                        "X[-5.5,5.5], Y[1.8,3.0], Z[-10.3,8.5].");
                }
            }

            Transform boundaries = FindDescendant(mapRoot.transform, "CityCombatBoundaries");
            Require(boundaries != null,
                "City map is missing isolated combat boundaries.");
            Require(boundaries.GetComponentsInChildren<BoxCollider>(true).Length == 4,
                "City clear lane requires exactly four authored boundary colliders.");
            RequireBoundary(boundaries, "Boundary_West",
                new Vector3(-6.15f, 1f, 0f), new Vector3(0.3f, 2f, 18.6f));
            RequireBoundary(boundaries, "Boundary_East",
                new Vector3(6.15f, 1f, 0f), new Vector3(0.3f, 2f, 18.6f));
            RequireBoundary(boundaries, "Boundary_South",
                new Vector3(0f, 1f, -9.15f), new Vector3(12.6f, 2f, 0.3f));
            RequireBoundary(boundaries, "Boundary_North",
                new Vector3(0f, 1f, 9.15f), new Vector3(12.6f, 2f, 0.3f));

            Transform surfaces = FindDescendant(mapRoot.transform, "AuthoredSurfaces");
            Require(surfaces != null && surfaces.childCount == 4,
                "City map requires exactly four product-owned primitive surfaces.");
            RequireSurface(surfaces, "Road_Asphalt",
                new Vector3(0f, -0.15f, 0f), new Vector3(12f, 0.3f, 20f),
                CityHeroPocketSceneSetup.AsphaltMaterialPath);
            RequireSurface(surfaces, "Sidewalk_West",
                new Vector3(-6.85f, -0.02f, 0f), new Vector3(1.3f, 0.16f, 20f),
                CityHeroPocketSceneSetup.SidewalkMaterialPath);
            RequireSurface(surfaces, "Sidewalk_East",
                new Vector3(6.85f, -0.02f, 0f), new Vector3(1.3f, 0.16f, 20f),
                CityHeroPocketSceneSetup.SidewalkMaterialPath);
            RequireSurface(surfaces, "EndPlatform_North",
                new Vector3(0f, -0.15f, 12.2f), new Vector3(12f, 0.3f, 4.4f),
                CityHeroPocketSceneSetup.AsphaltMaterialPath);

            int sourceTokyoLod0Slots = CountSourceTokyoLod0RendererSlots();
            Require(sourceTokyoLod0Slots
                    == CityHeroPocketSceneSetup.TokyoModuleLod0RendererSlots,
                $"Promoted Tokyo source-prefab LOD0 budget drifted; expected " +
                $"{CityHeroPocketSceneSetup.TokyoModuleLod0RendererSlots}, " +
                $"found {sourceTokyoLod0Slots}.");
            int lod0RendererSlots = CountLod0RendererSlots(mapRoot);
            int independentlyExpectedProductSlots = sourceTokyoLod0Slots + 4;
            Require(independentlyExpectedProductSlots
                    == CityHeroPocketSceneSetup.ProductLod0RendererSlots
                    && lod0RendererSlots == independentlyExpectedProductSlots,
                $"City hero block LOD0/product renderer budget drifted; expected " +
                $"{independentlyExpectedProductSlots}, " +
                $"found {lod0RendererSlots}.");
        }

        internal static int CountSourceTokyoLod0RendererSlots()
        {
            IReadOnlyList<CityHeroPocketSceneSetup.ModuleSpec> recipe =
                CityHeroPocketSceneSetup.ReviewedTokyoModuleSpecs;
            var slotsByPrefabPath = new Dictionary<string, int>(StringComparer.Ordinal);
            int total = 0;
            for (int i = 0; i < recipe.Count; i++)
            {
                string path = recipe[i].PrefabPath;
                if (!slotsByPrefabPath.TryGetValue(path, out int slots))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Require(prefab != null,
                        $"Cannot count missing promoted Tokyo prefab: {path}");
                    slots = CountLod0RendererSlots(prefab);
                    slotsByPrefabPath.Add(path, slots);
                }
                total += slots;
            }
            return total;
        }

        private static int CountLod0RendererSlots(GameObject root)
        {
            var renderersOwnedByLodGroups = new HashSet<Renderer>();
            var counted = new HashSet<Renderer>();
            LODGroup[] groups = root.GetComponentsInChildren<LODGroup>(true);
            for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                LOD[] lods = groups[groupIndex].GetLODs();
                for (int lodIndex = 0; lodIndex < lods.Length; lodIndex++)
                {
                    Renderer[] lodRenderers = lods[lodIndex].renderers;
                    for (int rendererIndex = 0;
                         rendererIndex < lodRenderers.Length;
                         rendererIndex++)
                    {
                        Renderer renderer = lodRenderers[rendererIndex];
                        if (renderer != null)
                        {
                            renderersOwnedByLodGroups.Add(renderer);
                            if (lodIndex == 0)
                            {
                                counted.Add(renderer);
                            }
                        }
                    }
                }
            }

            Renderer[] allRenderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < allRenderers.Length; i++)
            {
                Renderer renderer = allRenderers[i];
                if (renderer != null && !renderersOwnedByLodGroups.Contains(renderer))
                {
                    counted.Add(renderer);
                }
            }
            return counted.Count;
        }

        private static bool IntersectsLowCombatLane(Bounds bounds)
        {
            const float epsilon = 0.001f;
            return bounds.max.x > -6f + epsilon
                && bounds.min.x < 6f - epsilon
                && bounds.max.z > -9f + epsilon
                && bounds.min.z < 9f - epsilon
                && bounds.max.y > 0f + epsilon
                && bounds.min.y < 2.2f - epsilon;
        }

        private static bool IntersectsReviewedCameraSweep(Bounds colliderBounds)
        {
            const float cameraRadius = 0.25f;
            Bounds expandedCollider = colliderBounds;
            expandedCollider.Expand(cameraRadius * 2f);
            var reviewedSweep = new Bounds(
                new Vector3(0f, 2.4f, -0.9f),
                new Vector3(11f, 1.2f, 18.8f));
            return expandedCollider.Intersects(reviewedSweep);
        }

        private static void RequireBoundary(
            Transform parent,
            string name,
            Vector3 expectedPosition,
            Vector3 expectedSize)
        {
            Transform transform = FindDescendant(parent, name);
            Require(transform != null && transform.parent == parent,
                $"Missing direct boundary '{name}'.");
            BoxCollider collider = transform.GetComponent<BoxCollider>();
            Require(collider != null && collider.enabled && !collider.isTrigger,
                $"Boundary '{name}' requires one enabled solid BoxCollider.");
            Require(transform.gameObject.layer == 2,
                $"Boundary '{name}' must use built-in Ignore Raycast layer 2.");
            // Recipe contract explicitly excludes these layer-2 player boundaries
            // from camera collision; only camera-collidable package/surface solids
            // participate in the 0.25m sweep audit.
            Require((transform.localPosition - expectedPosition).sqrMagnitude <= 0.0001f
                    && (collider.size - expectedSize).sqrMagnitude <= 0.0001f,
                $"Boundary '{name}' transform/size drifted from the 12x18m lane contract.");
        }

        private static void RequireSurface(
            Transform parent,
            string name,
            Vector3 expectedPosition,
            Vector3 expectedScale,
            string expectedMaterialPath)
        {
            Transform transform = FindDescendant(parent, name);
            Require(transform != null && transform.parent == parent,
                $"Missing direct product surface '{name}'.");
            BoxCollider collider = transform.GetComponent<BoxCollider>();
            MeshRenderer renderer = transform.GetComponent<MeshRenderer>();
            Require(collider != null && collider.enabled && !collider.isTrigger,
                $"Product surface '{name}' must retain one enabled solid BoxCollider.");
            Require(renderer != null && renderer.enabled,
                $"Product surface '{name}' must retain an enabled MeshRenderer.");
            Require(renderer.sharedMaterial != null
                    && string.Equals(
                        AssetDatabase.GetAssetPath(renderer.sharedMaterial),
                        expectedMaterialPath,
                        StringComparison.Ordinal),
                $"Product surface '{name}' material drifted; expected " +
                $"'{expectedMaterialPath}', found " +
                $"'{AssetDatabase.GetAssetPath(renderer.sharedMaterial)}'.");
            Require((transform.localPosition - expectedPosition).sqrMagnitude <= 0.0001f
                    && (transform.localScale - expectedScale).sqrMagnitude <= 0.0001f,
                $"Product surface '{name}' transform drifted from the layout recipe.");
        }

        private static void RequireHudAction(
            GameObject hudRoot,
            string buttonName,
            CombatHudActionId expectedAction,
            bool sendHoldState,
            bool interactable)
        {
            Transform transform = FindDescendant(hudRoot.transform, buttonName);
            Require(transform != null, $"City HUD is missing '{buttonName}'.");
            Button button = transform.GetComponent<Button>();
            Require(button != null && button.interactable == interactable,
                $"City HUD '{buttonName}' interactable override did not survive save/reopen.");
            CombatHudPointerActionInput[] inputs =
                transform.GetComponents<CombatHudPointerActionInput>();
            Require(inputs.Length == 1
                    && inputs[0].ActionId == expectedAction
                    && inputs[0].SendsHoldState == sendHoldState,
                $"City HUD '{buttonName}' pointer action override did not survive save/reopen.");
            RequireSerializedObjectReference(
                inputs[0],
                "inputBridge",
                RequireSingle<CombatHudInputBridge>(hudRoot),
                $"City HUD '{buttonName}' lost its exact input-bridge reference.");
        }

        private static void RequireUnavailableHudAction(GameObject hudRoot, string buttonName)
        {
            Transform transform = FindDescendant(hudRoot.transform, buttonName);
            if (transform == null)
            {
                return;
            }
            Button button = transform.GetComponent<Button>();
            Require(button == null || !button.interactable,
                $"Unavailable city HUD action '{buttonName}' remained interactable.");
            Require(transform.GetComponents<CombatHudPointerActionInput>().Length == 0,
                $"Unavailable city HUD action '{buttonName}' retained pointer input.");
        }

        private static void ValidateSceneIntegrity(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                ValidateNoMissingScripts(roots[i]);
                ValidateRendererMaterials(roots[i], $"scene root '{roots[i].name}'");
            }
        }

        private static void ValidateNoMissingScripts(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
            {
                Component[] components = transforms[transformIndex].GetComponents<Component>();
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Require(components[componentIndex] != null,
                        $"Missing script on '{GetHierarchyPath(transforms[transformIndex])}'.");
                }
            }
        }

        private static void ValidateNoComponentNamespace(
            GameObject root,
            string namespacePrefix)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }
                string componentNamespace = component.GetType().Namespace ?? string.Empty;
                Require(!string.Equals(
                            componentNamespace,
                            namespacePrefix,
                            StringComparison.Ordinal)
                        && !componentNamespace.StartsWith(
                            namespacePrefix + ".",
                            StringComparison.Ordinal),
                    $"Compact player retained forbidden {component.GetType().FullName} component.");
            }
        }

        private static void ValidatePrefabObjectReferenceOwnership(GameObject prefabRoot)
        {
            Component[] components = prefabRoot.GetComponentsInChildren<Component>(true);
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Component component = components[componentIndex];
                if (component == null)
                {
                    continue;
                }

                SerializedObject serialized = new(component);
                SerializedProperty property = serialized.GetIterator();
                while (property.Next(enterChildren: true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference
                        || string.Equals(property.propertyPath, "m_Script", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    UnityEngine.Object referenced = property.objectReferenceValue;
                    if (referenced == null)
                    {
                        continue;
                    }

                    Transform referencedTransform = referenced switch
                    {
                        GameObject gameObject => gameObject.transform,
                        Component referencedComponent => referencedComponent.transform,
                        _ => null,
                    };
                    bool internalReference = referencedTransform != null
                        && (ReferenceEquals(referencedTransform, prefabRoot.transform)
                            || referencedTransform.IsChildOf(prefabRoot.transform));
                    bool persistentAsset = AssetDatabase.Contains(referenced)
                        || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(referenced));
                    Require(internalReference || persistentAsset,
                        $"Compact player retained non-asset external object reference at " +
                        $"{GetHierarchyPath(component.transform)}.{property.propertyPath} -> " +
                        $"{referenced.name} ({referenced.GetType().Name}).");
                }
            }
        }

        private static void ValidateRendererMaterials(GameObject root, string owner)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Material[] materials = renderers[rendererIndex].sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    Require(material != null,
                        $"Null material slot on '{GetHierarchyPath(renderers[rendererIndex].transform)}' ({owner}).");
                    Require(material.shader != null
                        && !string.Equals(material.shader.name, "Hidden/InternalErrorShader",
                            StringComparison.Ordinal),
                        $"Invalid shader on material '{material.name}' ({owner}).");
                }
            }
        }

        private static void RequireSerializedAssetPath(
            UnityEngine.Object owner,
            string propertyName,
            string expectedPath)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null
                && property.propertyType == SerializedPropertyType.ObjectReference,
                $"Missing serialized object property '{propertyName}' on {owner.name}.");
            string actualPath = AssetDatabase.GetAssetPath(property.objectReferenceValue);
            Require(string.Equals(actualPath, expectedPath, StringComparison.Ordinal),
                $"{owner.name}.{propertyName} expected '{expectedPath}', found '{actualPath}'.");
        }

        private static void RequireSerializedObjectReference(
            UnityEngine.Object owner,
            string propertyName,
            UnityEngine.Object expected,
            string message)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null
                    && property.propertyType == SerializedPropertyType.ObjectReference
                    && ReferenceEquals(property.objectReferenceValue, expected),
                message);
        }

        private static void RequireSerializedBool(
            UnityEngine.Object owner,
            string propertyName,
            bool expected,
            string message)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null
                    && property.propertyType == SerializedPropertyType.Boolean
                    && property.boolValue == expected,
                message);
        }

        private static void RequireSerializedString(
            UnityEngine.Object owner,
            string propertyName,
            string expected,
            string message)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null
                    && property.propertyType == SerializedPropertyType.String
                    && string.Equals(property.stringValue, expected, StringComparison.Ordinal),
                message);
        }

        private static void RequireSerializedEnumValue(
            UnityEngine.Object owner,
            string propertyName,
            int expected,
            string message)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null
                    && property.propertyType == SerializedPropertyType.Enum
                    && property.intValue == expected,
                message);
        }

        private static void RequireSerializedVector3(
            UnityEngine.Object owner,
            string propertyName,
            Vector3 expected,
            string message)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null
                    && property.propertyType == SerializedPropertyType.Vector3
                    && (property.vector3Value - expected).sqrMagnitude <= 0.0001f,
                message);
        }

        private static void RequireSerializedFloat(
            UnityEngine.Object owner,
            string propertyName,
            float expected,
            string message)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null
                    && property.propertyType == SerializedPropertyType.Float
                    && Mathf.Abs(property.floatValue - expected) <= 0.0001f,
                message);
        }

        private static T RequireSingleSceneComponent<T>(Scene scene)
            where T : Component
        {
            T[] components = FindSceneComponents<T>(scene);
            Require(components.Length == 1,
                $"Scene requires exactly one {typeof(T).Name}; found {components.Length}.");
            return components[0];
        }

        private static void RequireSerializedRendererArrayExact(
            Component owner,
            string propertyName,
            Transform visualRoot,
            IReadOnlyList<Renderer> expectedRenderers,
            string contractName)
        {
            SerializedObject serialized = new(owner);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null && property.isArray,
                $"{contractName} is missing serialized renderer array '{propertyName}'.");
            Require(property.arraySize == expectedRenderers.Count,
                $"{contractName} renderer count drifted; expected " +
                $"{expectedRenderers.Count}, found {property.arraySize}.");

            var expected = new HashSet<Renderer>();
            for (int i = 0; i < expectedRenderers.Count; i++)
            {
                Renderer renderer = expectedRenderers[i];
                Require(renderer != null,
                    $"{contractName} expected renderer set contains null at index {i}.");
                expected.Add(renderer);
            }

            var actual = new HashSet<Renderer>();
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                Require(element.propertyType == SerializedPropertyType.ObjectReference,
                    $"{contractName} renderer element {i} is not an object reference.");
                Renderer renderer = element.objectReferenceValue as Renderer;
                Require(renderer != null,
                    $"{contractName} renderer element {i} is null or not a Renderer.");
                Require(renderer.transform == visualRoot
                        || renderer.transform.IsChildOf(visualRoot),
                    $"{contractName} renderer '{renderer.name}' escaped Inori_RangedVisual.");
                Require(actual.Add(renderer),
                    $"{contractName} duplicates renderer '{renderer.name}'.");
            }

            Require(actual.SetEquals(expected),
                $"{contractName} must bind the exact current Inori renderer set.");
        }

        private static int CountSceneComponents<T>(Scene scene)
            where T : Component
        {
            return FindSceneComponents<T>(scene).Length;
        }

        private static T[] FindSceneComponents<T>(Scene scene)
            where T : Component
        {
            var found = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                found.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }
            return found.ToArray();
        }

        private static T RequireSingle<T>(GameObject root)
            where T : Component
        {
            T[] components = root.GetComponentsInChildren<T>(true);
            Require(components.Length == 1,
                $"'{root.name}' requires exactly one {typeof(T).Name}; found {components.Length}.");
            return components[0];
        }

        private static GameObject RequireUniqueSceneObject(Scene scene, string name)
        {
            GameObject found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    if (!string.Equals(transforms[transformIndex].name, name, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    Require(found == null,
                        $"Scene contains duplicate object '{name}'.");
                    found = transforms[transformIndex].gameObject;
                }
            }
            Require(found != null, $"Scene is missing object '{name}'.");
            return found;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }
            if (string.Equals(root.name, name, StringComparison.Ordinal))
            {
                return root;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private static void RequireOverrideCount(
            VolumeComponent component,
            int expectedCount)
        {
            int actualCount = 0;
            IReadOnlyList<VolumeParameter> parameters = component.parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].overrideState)
                {
                    actualCount++;
                }
            }
            Require(actualCount == expectedCount,
                $"{component.GetType().Name} expected {expectedCount} active parameter " +
                $"overrides, found {actualCount}.");
        }

        private static void RequireExactOverride(
            VolumeParameter<float> parameter,
            float expected,
            string message)
        {
            Require(parameter.overrideState
                    && Mathf.Abs(parameter.value - expected) <= 0.0001f,
                message);
        }

        private static void RequireExactOverride(
            VolumeParameter<Color> parameter,
            Color expected,
            string message)
        {
            Require(parameter.overrideState
                    && ColorsApproximately(parameter.value, expected),
                message);
        }

        private static void RequireExactOverride(
            VolumeParameter<Vector2> parameter,
            Vector2 expected,
            string message)
        {
            Require(parameter.overrideState
                    && (parameter.value - expected).sqrMagnitude <= 0.0001f,
                message);
        }

        private static void RequireExactOverride(
            VolumeParameter<Vector4> parameter,
            Vector4 expected,
            string message)
        {
            Require(parameter.overrideState
                    && (parameter.value - expected).sqrMagnitude <= 0.0001f,
                message);
        }

        private static void RequireExactOverride<T>(
            VolumeParameter<T> parameter,
            T expected,
            string message)
        {
            Require(parameter.overrideState
                    && EqualityComparer<T>.Default.Equals(parameter.value, expected),
                message);
        }

        private static void RequireMaterialFloat(
            Material material,
            string propertyName,
            float expected,
            string path)
        {
            Require(material.HasProperty(propertyName)
                    && Mathf.Abs(material.GetFloat(propertyName) - expected) <= 0.0001f,
                $"Owned city material property drifted: {path}.{propertyName}");
        }

        private static void RequireMaterialColor(
            Material material,
            string propertyName,
            Color expected,
            string path,
            bool required = true)
        {
            if (!material.HasProperty(propertyName))
            {
                Require(!required,
                    $"Owned city material is missing property: {path}.{propertyName}");
                return;
            }
            Require(ColorsApproximately(material.GetColor(propertyName), expected),
                $"Owned city material color drifted: {path}.{propertyName}");
        }

        private static bool ColorsApproximately(Color actual, Color expected)
        {
            return Mathf.Abs(actual.r - expected.r) <= 0.0001f
                && Mathf.Abs(actual.g - expected.g) <= 0.0001f
                && Mathf.Abs(actual.b - expected.b) <= 0.0001f
                && Mathf.Abs(actual.a - expected.a) <= 0.0001f;
        }

        private static bool IsNeutralWhite(Color color)
        {
            return Mathf.Abs(color.r - color.g) <= 0.001f
                && Mathf.Abs(color.g - color.b) <= 0.001f;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
