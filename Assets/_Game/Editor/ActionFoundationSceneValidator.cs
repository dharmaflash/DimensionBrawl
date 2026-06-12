using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationSceneValidator
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";
        private const string PlayerVisualName = "CombatGirlSwordShield_PlayerVisual";
        private const string LegacyPlayerVisualName = "CombatGirls_Sword_Shield";
        private const string CombatGirlMaterialRoot = "Assets/_Game/Art/Characters/Player/CombatGirlSwordShield/Materials";
        private const string CombatGirlAnimatorControllerPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";
        private const string StartRunClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_StartRun.fbx";
        private const string StopStepClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_StopStep.fbx";
        private const string TurnLeft90ClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_TurnLeft90.fbx";
        private const string TurnRight90ClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_TurnRight90.fbx";
        private const string Attack1ClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_Attack1.fbx";
        private const string Attack2ClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_Attack2.fbx";
        private const string Attack3ClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_Attack3.fbx";
        private const string Attack4ClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_Attack4.fbx";
        private const string Attack5ClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_Attack5.fbx";
        private const string DodgeForwardClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_DodgeForward.fbx";
        private const string DodgeBackClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_DodgeBack.fbx";
        private const string DodgeLeftClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_DodgeLeft.fbx";
        private const string DodgeRightClipPath = "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/SS_DodgeRight.fbx";
        private const string EnemyVisualName = "MaintenanceWorker_BasicSoldierVisual";
        private const string EnemyPlaceholderBodyName = "SciFiSoldierPlaceholderBody";
        private const string EnemyModelPath = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Models/SK_MaintenanceWorkerAllMeshes.fbx";
        private const string EnemyMaterialRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Materials";
        private const string EnemyTextureRoot = "Assets/_Game/Art/Characters/Enemies/SciFiSoldiers/MaintenanceWorker/Textures";
        private const string EnemyAnimatorControllerPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/DB_MaintenanceWorker_BasicSoldier.controller";
        private const string EnemyIdleClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_IdleCombat.fbx";
        private const string EnemyRunClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_RunCombat.fbx";
        private const string EnemyAttackClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_AttackCombat.fbx";
        private const string EnemyHitClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_GetHitFrontLight.fbx";
        private const string EnemyDeathClipPath = "Assets/_Game/Art/Animations/Enemies/SciFiSoldiers/MaintenanceWorker/MW_DeathFront.fbx";

        [MenuItem("DimensionBrawl/Validate Action Foundation Test Scene")]
        public static void ValidateMenu()
        {
            ValidateSceneAsset();
            Debug.Log("Action foundation test scene validation passed.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation CombatGirl Materials")]
        public static void ReapplyCombatGirlMaterialsMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject[] roots = scene.GetRootGameObjects();
            GameObject playerVisual = FindNamedObject(roots, PlayerVisualName) ?? FindNamedObject(roots, LegacyPlayerVisualName);
            if (playerVisual == null)
            {
                throw new InvalidOperationException($"Missing required {PlayerVisualName} in {ScenePath}.");
            }

            playerVisual.name = PlayerVisualName;
            int reassignedCount = ReapplyCombatGirlMaterials(playerVisual);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Reapplied CombatGirl materials on {reassignedCount} renderer slots in {ScenePath}.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation CombatGirl Weapon Sockets")]
        public static void ReapplyCombatGirlWeaponSocketsMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject[] roots = scene.GetRootGameObjects();
            GameObject playerVisual = RequireNamedObject(roots, PlayerVisualName, "player visual");
            CombatGirlWeaponSocketBinder weaponBinder = playerVisual.GetComponent<CombatGirlWeaponSocketBinder>();
            if (weaponBinder == null)
            {
                weaponBinder = playerVisual.AddComponent<CombatGirlWeaponSocketBinder>();
            }

            Transform leftHand = RequireNamedObject(roots, "hand_l", "left hand").transform;
            Transform rightHand = RequireNamedObject(roots, "hand_r", "right hand").transform;
            Transform leftWeaponSocket = RequireNamedObject(roots, "add_weapon_l", "left weapon socket").transform;
            Transform rightWeaponSocket = RequireNamedObject(roots, "add_weapon_r", "right weapon socket").transform;

            weaponBinder.ConfigureWeaponSockets(leftHand, leftWeaponSocket, rightHand, rightWeaponSocket);
            weaponBinder.ApplyBindings();
            EditorUtility.SetDirty(weaponBinder);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied CombatGirl weapon socket bindings in ActionFoundationTest.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation StopStep Responsiveness")]
        public static void ReapplyStopStepResponsivenessMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerMovementController movement = RequireObject<PlayerMovementController>(roots, "player movement");
            SetFloat(movement, "stopSettleInputHoldSeconds", 0.16f);
            SetFloat(movement, "animatorMoveDampSeconds", 0.06f);
            EditorUtility.SetDirty(movement);

            ConfigureStopStepClip();
            ConfigureStopStepAnimator();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied StopStep responsiveness tuning in ActionFoundationTest.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Shared Combat AI")]
        public static void ReapplySharedCombatAiMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(playerActions.gameObject, "player health");
            CombatHealth soldierHealth = RequireComponent<CombatHealth>(soldier.gameObject, "basic soldier health");
            GameObject soldierBody = RequireNamedObject(roots, EnemyPlaceholderBodyName, "basic soldier placeholder body");
            GameObject soldierVisual = FindNamedObject(roots, EnemyVisualName);
            Transform poseRoot = soldierVisual != null ? soldierVisual.transform : soldierBody.transform;
            GameObject telegraphObject = RequireNamedObject(roots, "ReadableAttackTelegraph", "basic soldier attack telegraph");
            Renderer telegraphRenderer = RequireComponent<Renderer>(telegraphObject, "basic soldier attack telegraph renderer");
            CombatTargetSensor targetSensor = EnsureComponent<CombatTargetSensor>(soldier.gameObject);
            EnemyAttackTelegraphPresenter telegraphPresenter = EnsureComponent<EnemyAttackTelegraphPresenter>(soldier.gameObject);

            SetObjectReference(targetSensor, "selfHealth", soldierHealth);
            SetFloat(targetSensor, "searchRadius", 12f);
            SetFloat(targetSensor, "retargetIntervalSeconds", 0.2f);
            SetObjectReferenceArray(targetSensor, "targetCandidates", new UnityEngine.Object[] { playerHealth });
            SetObjectReference(telegraphPresenter, "telegraphObject", telegraphObject);
            SetObjectReference(telegraphPresenter, "telegraphTransform", telegraphObject.transform);
            SetObjectReference(telegraphPresenter, "telegraphRenderer", telegraphRenderer);
            SetObjectReference(telegraphPresenter, "poseRoot", poseRoot);
            SetVector3(telegraphPresenter, "windupStartScale", new Vector3(0.35f, 0.02f, 0.65f));
            SetVector3(telegraphPresenter, "windupEndScale", new Vector3(1.05f, 0.02f, 1.55f));
            SetVector3(telegraphPresenter, "activeScale", new Vector3(1.25f, 0.025f, 1.8f));
            SetVector3(telegraphPresenter, "windupPoseOffset", new Vector3(0f, 0f, -0.08f));
            SetVector3(telegraphPresenter, "activePoseOffset", new Vector3(0f, 0f, 0.12f));
            SetObjectReference(soldier, "targetSensor", targetSensor);
            SetObjectReference(soldier, "telegraphPresenter", telegraphPresenter);
            SetString(soldier, "enemyTypeId", "SciFiSoldier.Basic");
            SetString(soldier, "patternId", "ClosePunish");

            EditorUtility.SetDirty(targetSensor);
            EditorUtility.SetDirty(telegraphPresenter);
            EditorUtility.SetDirty(soldier);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Reapplied shared combat AI target sensor wiring in ActionFoundationTest.");
        }

        public static void ValidateSceneAsset()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            try
            {
                ValidateScene(scene);
                Debug.Log("Action foundation test scene validation passed.");
            }
            finally
            {
                if (previousScene.IsValid() && !string.IsNullOrEmpty(previousScene.path) && previousScene.path != ScenePath)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }
        }

        private static void ValidateScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Scene is invalid: {ScenePath}");
            }

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionProfile playerActionProfile = LoadProfile<PlayerActionProfile>(ActionFoundationProfileSetup.PlayerActionProfilePath);
            ActionCameraCueProfile cameraCueProfile = LoadProfile<ActionCameraCueProfile>(ActionFoundationProfileSetup.CameraCueProfilePath);
            EnemyPatternProfile enemyPatternProfile = LoadProfile<EnemyPatternProfile>(ActionFoundationProfileSetup.EnemyPatternProfilePath);
            PlayerMovementController movement = RequireObject<PlayerMovementController>(roots, "player movement");
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(playerActions.gameObject, "player health");
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");
            CombatTargetSensor soldierTargetSensor = RequireComponent<CombatTargetSensor>(soldier.gameObject, "basic soldier target sensor");
            EnemyAttackTelegraphPresenter soldierTelegraphPresenter = RequireComponent<EnemyAttackTelegraphPresenter>(soldier.gameObject, "basic soldier telegraph presenter");
            CombatHitFeedback soldierHitFeedback = RequireComponent<CombatHitFeedback>(soldier.gameObject, "basic soldier hit feedback");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(roots, "action camera");
            ActionCameraCueDriver cameraCueDriver = RequireObject<ActionCameraCueDriver>(roots, "action camera cue driver");
            Type dodgeFeedbackType = RequireType("DimensionBrawl.Presentation.PlayerDodgeFeedback, DimensionBrawl.Runtime");
            Component dodgeFeedback = RequireObject(roots, dodgeFeedbackType, "player dodge feedback");
            GameObject playerVisual = RequireNamedObject(roots, PlayerVisualName, "player visual");
            Animator playerVisualAnimator = RequireComponent<Animator>(playerVisual, "player visual animator");
            CombatGirlWeaponSocketBinder weaponBinder = RequireComponent<CombatGirlWeaponSocketBinder>(playerVisual, "weapon socket binder");
            List<CombatHitFeedback> hitFeedbacks = CollectObjects<CombatHitFeedback>(roots);
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(roots, "test encounter");
            GameObject soldierVisual = RequireNamedObject(roots, EnemyVisualName, "promoted basic soldier visual");
            Animator soldierVisualAnimator = RequireComponent<Animator>(soldierVisual, "basic soldier visual animator");
            Renderer[] soldierVisualRenderers = CollectPresentableRenderers(soldierVisual);
            GameObject soldierBody = RequireNamedObject(roots, EnemyPlaceholderBodyName, "basic soldier placeholder body");
            GameObject telegraphObject = RequireNamedObject(roots, "ReadableAttackTelegraph", "basic soldier attack telegraph");

            List<CombatHealth> healths = CollectObjects<CombatHealth>(roots);
            RequireAtLeast(healths.Count, 2, "player and enemy health components");
            RequireAtLeast(hitFeedbacks.Count, 2, "player and enemy hit feedback components");
            RequireAtLeast(soldierVisualRenderers.Length, 1, "basic soldier promoted visual renderers");

            ValidateReference(movement, "referenceCamera");
            ValidateReference(movement, "animator");
            ValidateFloat(movement, "stopSettleSeconds", 0.26f);
            ValidateFloat(movement, "stopSettleInputHoldSeconds", 0.16f);
            ValidateString(movement, "startRunTrigger", "StartRun");
            ValidateString(movement, "stopStepTrigger", "StopStep");
            ValidateString(movement, "turnLeft90Trigger", "TurnLeft90");
            ValidateString(movement, "turnRight90Trigger", "TurnRight90");
            ValidateFloat(movement, "animatorMoveDampSeconds", 0.06f);
            ValidateFloat(movement, "stopSettleAnimatorSpeedFloor", 0.24f);
            ValidateFloat(movement, "sharpTurnTriggerAngle", 65f);
            ValidateFloat(movement, "sharpTurnMinimumSpeedRatio", 0.35f);
            ValidateFloat(movement, "sharpTurnCooldownSeconds", 0.32f);
            ValidateReference(playerActions, "movement");
            ValidateReference(playerActions, "health");
            ValidateReference(playerActions, "animator");
            ValidateObjectReferenceAssetPath(playerActions, "actionProfile", ActionFoundationProfileSetup.PlayerActionProfilePath);
            ValidatePlayerActionProfile(playerActionProfile);

            ValidateObjectReferenceAssetPath(soldier, "patternProfile", ActionFoundationProfileSetup.EnemyPatternProfilePath);
            ValidateEnemyPatternProfile(enemyPatternProfile);
            ValidateReference(soldier, "targetSensor");
            ValidateReference(soldier, "target");
            ValidateReference(soldier, "targetHealth");
            ValidateReference(soldier, "selfHealth");
            ValidateReference(soldier, "telegraphIndicator");
            ValidateReference(soldier, "telegraphPresenter");
            ValidateObjectReference(soldier, "animator", soldierVisualAnimator);
            ValidateObjectReference(soldier, "bodyRenderer", soldierVisualRenderers[0]);
            ValidateBool(soldier, "usePrototypeBodyColors", false);
            ValidateReference(soldierTargetSensor, "selfHealth");
            ValidateFloat(soldierTargetSensor, "searchRadius", 12f);
            ValidateFloat(soldierTargetSensor, "retargetIntervalSeconds", 0.2f);
            ValidateObjectReferenceArray(soldierTargetSensor, "targetCandidates", new UnityEngine.Object[] { playerHealth });
            ValidateObjectReference(soldierTelegraphPresenter, "telegraphObject", telegraphObject);
            ValidateObjectReference(soldierTelegraphPresenter, "telegraphTransform", telegraphObject.transform);
            ValidateObjectReference(soldierTelegraphPresenter, "telegraphRenderer", RequireComponent<Renderer>(telegraphObject, "basic soldier attack telegraph renderer"));
            ValidateObjectReference(soldierTelegraphPresenter, "poseRoot", soldierVisual.transform);
            ValidateVector3(soldierTelegraphPresenter, "windupStartScale", new Vector3(0.35f, 0.02f, 0.65f));
            ValidateVector3(soldierTelegraphPresenter, "windupEndScale", new Vector3(1.05f, 0.02f, 1.55f));
            ValidateVector3(soldierTelegraphPresenter, "activeScale", new Vector3(1.25f, 0.025f, 1.8f));
            ValidateVector3(soldierTelegraphPresenter, "windupPoseOffset", new Vector3(0f, 0f, -0.08f));
            ValidateVector3(soldierTelegraphPresenter, "activePoseOffset", new Vector3(0f, 0f, 0.12f));
            ValidateBool(soldierHitFeedback, "applyIdleColorOnEnable", false);
            ValidateArrayMinSize(soldierHitFeedback, "flashRenderers", 1);

            if (soldierBody.activeSelf)
            {
                throw new InvalidOperationException($"{EnemyPlaceholderBodyName} should stay inactive once the promoted MaintenanceWorker visual is wired.");
            }

            ValidateReference(cameraController, "target");
            ValidateReference(cameraController, "threat");
            ValidateVector3(cameraController, "cameraOffset", new Vector3(0f, 1.05f, -4.2f));
            ValidateVector3(cameraController, "lookOffset", new Vector3(0f, 1.2f, 0.55f));
            ValidateFloat(cameraController, "manualYawSpeedDegrees", 150f);
            ValidateFloat(cameraController, "mouseYawDegreesPerPixel", 0.12f);
            ValidateFloat(cameraController, "targetYawAssist", 0.18f);
            ValidateFloat(cameraController, "targetYawAssistSpeed", 2.2f);
            ValidateFloat(cameraController, "orbitInputDeadZone", 0.08f);
            ValidateFloat(cameraController, "threatBias", 0.25f);
            ValidateFloat(cameraController, "maxLeadFromPlayerSpeed", 0.35f);
            ValidateFloat(cameraController, "defaultCueSeconds", 0.24f);
            ValidateFloat(cameraController, "maxCueOffset", 0.55f);
            ValidateFloat(cameraController, "maxCueFieldOfViewDelta", 4f);
            ValidateFloat(cameraController, "maxCueCameraDistanceDelta", 0.45f);
            ValidateFloat(cameraController, "maxCueFocusHeightDelta", 0.25f);
            ValidateFloat(cameraController, "cueFieldOfViewSmooth", 18f);
            ValidateReference(cameraCueDriver, "actionController");
            ValidateReference(cameraCueDriver, "movement");
            ValidateReference(cameraCueDriver, "cameraController");
            ValidateReference(cameraCueDriver, "cueSpace");
            ValidateObjectReferenceAssetPath(cameraCueDriver, "cueProfile", ActionFoundationProfileSetup.CameraCueProfilePath);
            ValidateActionCameraCueProfile(cameraCueProfile);

            ValidateReference(playerVisualAnimator, "m_Avatar");
            ValidateReference(playerVisualAnimator, "m_Controller");
            ValidateBool(playerVisualAnimator, "m_ApplyRootMotion", false);
            ValidateWeaponSocketBinder(weaponBinder);
            ValidateAnimatorParameter(playerVisualAnimator, "StartRun", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "StopStep", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "TurnLeft90", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "TurnRight90", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "Attack1", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "Attack2", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "Attack3", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "Attack4", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "Attack5", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "DodgeForward", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "DodgeBack", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "DodgeLeft", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(playerVisualAnimator, "DodgeRight", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorStateMotion(playerVisualAnimator, "StartRun", StartRunClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "StopStep", StopStepClipPath);
            ValidateAnimatorStateSpeed(playerVisualAnimator, "StopStep", 1.45f);
            ValidateStopStepTransitionResponsiveness(playerVisualAnimator);
            ValidateAnimatorStateMotion(playerVisualAnimator, "TurnLeft90", TurnLeft90ClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "TurnRight90", TurnRight90ClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "Attack1", Attack1ClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "Attack2", Attack2ClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "Attack3", Attack3ClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "Attack4", Attack4ClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "Attack5", Attack5ClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "DodgeForward", DodgeForwardClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "DodgeBack", DodgeBackClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "DodgeLeft", DodgeLeftClipPath);
            ValidateAnimatorStateMotion(playerVisualAnimator, "DodgeRight", DodgeRightClipPath);
            ValidateAnimatorTriggerTransition(playerVisualAnimator, "StartRun", "StopStep", "StopStep");
            ValidateAnimatorTriggerTransition(playerVisualAnimator, "TurnLeft90", "StopStep", "StopStep");
            ValidateAnimatorTriggerTransition(playerVisualAnimator, "TurnRight90", "StopStep", "StopStep");
            ValidateAnyStateTriggerTransition(playerVisualAnimator, "DodgeForward", "DodgeForward");
            ValidateAnyStateTriggerTransition(playerVisualAnimator, "DodgeBack", "DodgeBack");
            ValidateAnyStateTriggerTransition(playerVisualAnimator, "DodgeLeft", "DodgeLeft");
            ValidateAnyStateTriggerTransition(playerVisualAnimator, "DodgeRight", "DodgeRight");
            ValidateCombatGirlMaterials(playerVisual);

            ValidateObjectReferenceAssetPath(soldierVisualAnimator, "m_Avatar", EnemyModelPath);
            ValidateObjectReferenceAssetPath(soldierVisualAnimator, "m_Controller", EnemyAnimatorControllerPath);
            ValidateBool(soldierVisualAnimator, "m_ApplyRootMotion", false);
            ValidateAnimatorParameter(soldierVisualAnimator, "MoveSpeed", AnimatorControllerParameterType.Float);
            ValidateAnimatorParameter(soldierVisualAnimator, "Attack", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(soldierVisualAnimator, "Hit", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorParameter(soldierVisualAnimator, "Death", AnimatorControllerParameterType.Trigger);
            ValidateAnimatorStateMotion(soldierVisualAnimator, "Idle", EnemyIdleClipPath);
            ValidateAnimatorStateMotion(soldierVisualAnimator, "Run", EnemyRunClipPath);
            ValidateAnimatorStateMotion(soldierVisualAnimator, "Attack", EnemyAttackClipPath);
            ValidateAnimatorStateMotion(soldierVisualAnimator, "Hit", EnemyHitClipPath);
            ValidateAnimatorStateMotion(soldierVisualAnimator, "Death", EnemyDeathClipPath);
            ValidateAnimationClipHeightFromFeet(EnemyDeathClipPath, "MW_DeathFront");
            ValidateAnyStateTriggerTransition(soldierVisualAnimator, "Attack", "Attack");
            ValidateAnyStateTriggerTransition(soldierVisualAnimator, "Hit", "Hit");
            ValidateAnyStateTriggerTransition(soldierVisualAnimator, "Death", "Death");
            ValidateAnyStateTriggerPriority(soldierVisualAnimator, "Death", "Hit", "Attack");
            ValidateMaintenanceWorkerMaterials(soldierVisual);

            ValidateReference(dodgeFeedback, "actionController");
            ValidateArrayMinSize(dodgeFeedback, "targetRenderers", 1);

            ValidateReference(encounter, "playerHealth");
            ValidateReference(encounter, "enemyHealth");
            ValidateReference(encounter, "winMarker");
            ValidateReference(encounter, "failMarker");
        }

        private static void ValidateWeaponSocketBinder(CombatGirlWeaponSocketBinder weaponBinder)
        {
            if (weaponBinder.BindingCount != 2 || !weaponBinder.AllBindingsValid)
            {
                throw new InvalidOperationException($"{weaponBinder.name} must bind left and right CombatGirl weapon sockets to the hand bones.");
            }

            weaponBinder.ApplyBindings();
            if (!weaponBinder.AreBindingsAligned(0.005f, 0.5f))
            {
                throw new InvalidOperationException($"{weaponBinder.name} weapon sockets are not aligned with their configured hand offsets.");
            }
        }

        private static void ConfigureStopStepClip()
        {
            ModelImporter importer = AssetImporter.GetAtPath(StopStepClipPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Missing StopStep ModelImporter: {StopStepClipPath}");
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }

            if (clips == null || clips.Length == 0)
            {
                throw new InvalidOperationException($"StopStep clip has no imported animation clips: {StopStepClipPath}");
            }

            clips[0].name = "SS_StopStep";
            clips[0].firstFrame = 4f;
            clips[0].lastFrame = 45f;
            clips[0].loopTime = false;
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static void ConfigureStopStepAnimator()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CombatGirlAnimatorControllerPath);
            if (controller == null)
            {
                throw new InvalidOperationException($"Missing Animator Controller: {CombatGirlAnimatorControllerPath}");
            }

            AnimatorState stopStepState = FindAnimatorState(controller, "StopStep");
            if (stopStepState == null)
            {
                throw new InvalidOperationException($"{controller.name} is missing StopStep state.");
            }

            stopStepState.speed = 1.45f;

            for (int i = 0; i < controller.layers.Length; i++)
            {
                AnimatorStateMachine stateMachine = controller.layers[i].stateMachine;
                ConfigureStopStepTransitions(stateMachine.anyStateTransitions);

                ChildAnimatorState[] states = stateMachine.states;
                for (int j = 0; j < states.Length; j++)
                {
                    if (states[j].state != null)
                    {
                        ConfigureStopStepTransitions(states[j].state.transitions);
                    }
                }
            }

            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureStopStepTransitions(AnimatorStateTransition[] transitions)
        {
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition transition = transitions[i];
                if (!IsStopStepTransition(transition))
                {
                    continue;
                }

                transition.duration = 0.015f;
                transition.hasExitTime = false;
                transition.exitTime = 0f;
                EditorUtility.SetDirty(transition);
            }
        }

        private static bool IsStopStepTransition(AnimatorStateTransition transition)
        {
            if (transition == null || transition.destinationState == null || transition.destinationState.name != "StopStep")
            {
                return false;
            }

            AnimatorCondition[] conditions = transition.conditions;
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i].mode == AnimatorConditionMode.If
                    && string.Equals(conditions[i].parameter, "StopStep", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static T RequireObject<T>(GameObject[] roots, string label) where T : Component
        {
            List<T> found = CollectObjects<T>(roots);
            if (found.Count == 0)
            {
                throw new InvalidOperationException($"Missing required {label}.");
            }

            return found[0];
        }

        private static GameObject RequireNamedObject(GameObject[] roots, string objectName, string label)
        {
            GameObject found = FindNamedObject(roots, objectName);
            if (found == null)
            {
                throw new InvalidOperationException($"Missing required {label}: {objectName}.");
            }

            return found;
        }

        private static GameObject FindNamedObject(GameObject[] roots, string objectName)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j] != null && transforms[j].name == objectName)
                    {
                        return transforms[j].gameObject;
                    }
                }
            }

            return null;
        }

        private static T RequireComponent<T>(GameObject owner, string label) where T : Component
        {
            if (!owner.TryGetComponent(out T component))
            {
                throw new InvalidOperationException($"{owner.name} is missing required {label}.");
            }

            return component;
        }

        private static T EnsureComponent<T>(GameObject owner) where T : Component
        {
            if (!owner.TryGetComponent(out T component))
            {
                component = owner.AddComponent<T>();
            }

            return component;
        }

        private static Component RequireObject(GameObject[] roots, Type componentType, string label)
        {
            List<Component> found = CollectObjects(roots, componentType);
            if (found.Count == 0)
            {
                throw new InvalidOperationException($"Missing required {label}.");
            }

            return found[0];
        }

        private static Type RequireType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new InvalidOperationException($"Missing required runtime type {typeName}.");
            }

            return type;
        }

        private static List<T> CollectObjects<T>(GameObject[] roots) where T : Component
        {
            List<T> results = new List<T>();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                results.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }

            return results;
        }

        private static List<Component> CollectObjects(GameObject[] roots, Type componentType)
        {
            List<Component> results = new List<Component>();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                results.AddRange(roots[i].GetComponentsInChildren(componentType, true));
            }

            return results;
        }

        private static int ReapplyCombatGirlMaterials(GameObject playerVisual)
        {
            Dictionary<string, Material> materialByAlias = BuildCombatGirlMaterialAliases();
            Renderer[] renderers = playerVisual.GetComponentsInChildren<Renderer>(true);
            int reassignedCount = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer targetRenderer = renderers[i];
                Material[] sharedMaterials = targetRenderer.sharedMaterials;
                bool changed = false;

                for (int j = 0; j < sharedMaterials.Length; j++)
                {
                    string key = ResolveMaterialAlias(sharedMaterials[j], targetRenderer);
                    if (key == null || !materialByAlias.TryGetValue(key, out Material replacement))
                    {
                        continue;
                    }

                    if (sharedMaterials[j] != replacement)
                    {
                        sharedMaterials[j] = replacement;
                        changed = true;
                        reassignedCount++;
                    }
                }

                if (changed)
                {
                    targetRenderer.sharedMaterials = sharedMaterials;
                    EditorUtility.SetDirty(targetRenderer);
                }
            }

            return reassignedCount;
        }

        private static Dictionary<string, Material> BuildCombatGirlMaterialAliases()
        {
            Dictionary<string, Material> materials = new Dictionary<string, Material>();
            AddCombatGirlMaterial(materials, "DB_CombatGirl_Body", "Body", "Body");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_Eye", "Eye", "Eye", "eye");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_Face", "Face", "Face");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_ShieldCloth01", "ShieldCloth01", "Shield_Cloth", "Shiled_Cloth", "Shield_Sword_Cloth_01");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_ShieldHair01", "ShieldHair01", "Shield_Hair", "Shiled_Hair", "Shield_Sword_Hair_01");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_SquireCloth", "SquireCloth", "Squire_Cloth");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_WeaponAxe01", "WeaponAxe01", "Weapon_Axe_Shiled", "Weapon_Shield_Axe_01");
            AddCombatGirlMaterial(materials, "DB_CombatGirl_WeaponSword01", "WeaponSword01", "Shield_Weapon", "Weapon_Shiled_Sword", "Weapon_Sword_Shiled", "Weapon_Shield_Sword_01");
            return materials;
        }

        private static void AddCombatGirlMaterial(Dictionary<string, Material> materials, string assetName, string fileAlias, params string[] aliases)
        {
            string path = $"{CombatGirlMaterialRoot}/{assetName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                throw new InvalidOperationException($"Missing CombatGirl material asset: {path}");
            }

            materials[NormalizeMaterialAlias(assetName)] = material;
            materials[NormalizeMaterialAlias(fileAlias)] = material;
            for (int i = 0; i < aliases.Length; i++)
            {
                materials[NormalizeMaterialAlias(aliases[i])] = material;
            }
        }

        private static string ResolveMaterialAlias(Material material, Renderer renderer)
        {
            if (material != null)
            {
                return NormalizeMaterialAlias(material.name);
            }

            return renderer != null ? NormalizeMaterialAlias(renderer.name) : null;
        }

        private static string NormalizeMaterialAlias(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized = value.Replace("(Instance)", string.Empty).ToLowerInvariant();
            char[] buffer = new char[normalized.Length];
            int count = 0;
            for (int i = 0; i < normalized.Length; i++)
            {
                char c = normalized[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    buffer[count] = c;
                    count++;
                }
            }

            return new string(buffer, 0, count);
        }

        private static void ValidateCombatGirlMaterials(GameObject playerVisual)
        {
            Renderer[] renderers = playerVisual.GetComponentsInChildren<Renderer>(true);
            RequireAtLeast(renderers.Length, 1, "CombatGirl visual renderers");

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] sharedMaterials = renderers[i].sharedMaterials;
                for (int j = 0; j < sharedMaterials.Length; j++)
                {
                    Material material = sharedMaterials[j];
                    if (material == null)
                    {
                        throw new InvalidOperationException($"{renderers[i].name} has a missing CombatGirl material slot.");
                    }

                    string materialPath = AssetDatabase.GetAssetPath(material).Replace('\\', '/');
                    if (!materialPath.StartsWith(CombatGirlMaterialRoot + "/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{renderers[i].name} uses non-game-owned CombatGirl material: {materialPath}");
                    }
                }
            }
        }

        private static void ValidateMaintenanceWorkerMaterials(GameObject soldierVisual)
        {
            Renderer[] renderers = CollectPresentableRenderers(soldierVisual);
            RequireAtLeast(renderers.Length, 1, "MaintenanceWorker visual renderers");

            int baseTextureCount = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] sharedMaterials = renderers[i].sharedMaterials;
                for (int j = 0; j < sharedMaterials.Length; j++)
                {
                    Material material = sharedMaterials[j];
                    if (material == null)
                    {
                        throw new InvalidOperationException($"{renderers[i].name} has a missing MaintenanceWorker material slot.");
                    }

                    string materialPath = AssetDatabase.GetAssetPath(material).Replace('\\', '/');
                    if (!materialPath.StartsWith(EnemyMaterialRoot + "/", StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{renderers[i].name} uses non-game-owned MaintenanceWorker material: {materialPath}");
                    }

                    if (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null)
                    {
                        string texturePath = AssetDatabase.GetAssetPath(material.GetTexture("_BaseMap")).Replace('\\', '/');
                        if (!texturePath.StartsWith(EnemyTextureRoot + "/", StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException($"{material.name} uses non-game-owned MaintenanceWorker base texture: {texturePath}");
                        }

                        baseTextureCount++;
                    }
                }
            }

            RequireAtLeast(baseTextureCount, 1, "MaintenanceWorker promoted base textures");
        }

        private static Renderer[] CollectPresentableRenderers(GameObject root)
        {
            return root
                .GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.enabled && IsActiveInPrefabHierarchy(renderer.transform, root.transform))
                .ToArray();
        }

        private static bool IsActiveInPrefabHierarchy(Transform candidate, Transform root)
        {
            for (Transform current = candidate; current != null; current = current.parent)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                if (current == root)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RequireAtLeast(int actual, int expected, string label)
        {
            if (actual < expected)
            {
                throw new InvalidOperationException($"Expected at least {expected} {label}, found {actual}.");
            }
        }

        private static void ValidateReference(UnityEngine.Object target, string propertyName)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property.objectReferenceValue == null)
            {
                throw new InvalidOperationException($"{target.name} has no reference assigned for {propertyName}.");
            }
        }

        private static void ValidateObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property.objectReferenceValue != expected)
            {
                UnityEngine.Object actual = property.objectReferenceValue;
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected.name}, found {(actual != null ? actual.name : "null")}.");
            }
        }

        private static void ValidateObjectReferenceAssetPath(UnityEngine.Object target, string propertyName, string expectedPath)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            UnityEngine.Object actual = property.objectReferenceValue;
            if (actual == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected asset {expectedPath}, found null.");
            }

            string actualPath = AssetDatabase.GetAssetPath(actual).Replace('\\', '/');
            if (!string.Equals(actualPath, expectedPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected asset {expectedPath}, found {actualPath}.");
            }
        }

        private static T LoadProfile<T>(string path) where T : ScriptableObject
        {
            T profile = AssetDatabase.LoadAssetAtPath<T>(path);
            if (profile == null)
            {
                throw new InvalidOperationException($"Missing required gameplay profile asset at {path}.");
            }

            return profile;
        }

        private static void ValidateArraySize(UnityEngine.Object target, string propertyName, int expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!property.isArray || property.arraySize != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected array size {expected}, found {property.arraySize}.");
            }
        }

        private static void ValidateArrayMinSize(UnityEngine.Object target, string propertyName, int minimum)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!property.isArray || property.arraySize < minimum)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected at least {minimum}, found {property.arraySize}.");
            }
        }

        private static void ValidatePlayerActionProfile(PlayerActionProfile profile)
        {
            ValidateArraySize(profile, "basicCombo", 5);
            ValidateAttackStep(profile, "basicCombo", 0, "Attack1", 0.12f, 0.08f, 0.28f, 0.10f, 0.06f, 20f, 0.55f, 1.35f, 0.03f);
            ValidateAttackStep(profile, "basicCombo", 1, "Attack2", 0.14f, 0.09f, 0.32f, 0.10f, 0.08f, 24f, 0.60f, 1.45f, 0.03f);
            ValidateAttackStep(profile, "basicCombo", 2, "Attack3", 0.16f, 0.10f, 0.30f, 0.12f, 0.10f, 34f, 0.70f, 1.55f, 0.04f);
            ValidateAttackStep(profile, "basicCombo", 3, "Attack4", 0.17f, 0.10f, 0.34f, 0.12f, 0.12f, 40f, 0.72f, 1.62f, 0.05f);
            ValidateAttackStep(profile, "basicCombo", 4, "Attack5", 0.20f, 0.12f, 0.46f, 0.12f, 0.14f, 56f, 0.82f, 1.75f, 0.05f);
            ValidateFloat(profile, "comboResetSeconds", 0.75f);
            ValidateFloat(profile, "comboQueueOpenAfterSeconds", 0.10f);
            ValidateFloat(profile, "comboChainRecoveryRatio", 0.45f);
            ValidateFloat(profile, "dodgeDurationSeconds", 0.56f);
            ValidateFloat(profile, "dodgeInvulnerableFromSeconds", 0.05f);
            ValidateFloat(profile, "dodgeInvulnerableToSeconds", 0.32f);
            ValidateFloat(profile, "dodgeRecoverySeconds", 0.14f);
            ValidateFloat(profile, "dodgeSpeed", 10.2f);
            ValidateString(profile, "dodgeTrigger", "DodgeForward");
            ValidateString(profile, "dodgeBackTrigger", "DodgeBack");
            ValidateString(profile, "dodgeLeftTrigger", "DodgeLeft");
            ValidateString(profile, "dodgeRightTrigger", "DodgeRight");
            ValidateString(profile, "dodgingParameter", "IsDodging");
        }

        private static void ValidateActionCameraCueProfile(ActionCameraCueProfile profile)
        {
            ValidateCameraCueProfile(profile, "runStartCue", new Vector3(0f, 0.02f, -0.10f), 0.08f, 0.8f, -0.08f, 0.01f, 0.20f, 1f);
            ValidateCameraCueProfile(profile, "stopSettleCue", new Vector3(0f, -0.02f, -0.06f), -0.02f, -0.8f, -0.12f, -0.02f, 0.22f, 1f);
            ValidateCameraCueProfile(profile, "sharpTurnCue", new Vector3(0.08f, 0f, -0.10f), 0.06f, 0.6f, -0.06f, 0f, 0.24f, 1f);
            ValidateCameraCueProfile(profile, "dodgeCue", new Vector3(0f, 0.04f, -0.2f), -0.18f, 2.2f, -0.2f, 0.03f, 0.28f, 1f);
            ValidateCameraCueProfile(profile, "attackStartCue", new Vector3(0f, -0.03f, 0.14f), 0.08f, -1.2f, 0.12f, -0.02f, 0.22f, 1.2f);
            ValidateCameraCueProfile(profile, "attackHitCue", new Vector3(0f, 0.03f, 0.12f), 0.06f, -1.8f, 0.16f, 0.01f, 0.18f, 1.3f);
        }

        private static void ValidateEnemyPatternProfile(EnemyPatternProfile profile)
        {
            ValidateString(profile, "enemyTypeId", "SciFiSoldier.Basic");
            ValidateString(profile, "patternId", "ClosePunish");
            ValidateFloat(profile, "approachSpeed", 2.7f);
            ValidateFloat(profile, "turnRateDegrees", 540f);
            ValidateFloat(profile, "gravity", -24f);
            ValidateFloat(profile, "attackRange", 1.65f);
            ValidateFloat(profile, "telegraphSeconds", 0.65f);
            ValidateFloat(profile, "activeSeconds", 0.14f);
            ValidateFloat(profile, "recoverySeconds", 0.45f);
            ValidateFloat(profile, "damage", 15f);
            ValidateFloat(profile, "hitStopSeconds", 0.03f);
            ValidateFloat(profile, "hitReactionSeconds", 0.24f);
            ValidateFloat(profile, "knockbackSpeed", 2f);
            ValidateString(profile, "moveSpeedParameter", "MoveSpeed");
            ValidateString(profile, "attackTrigger", "Attack");
            ValidateString(profile, "hitTrigger", "Hit");
            ValidateString(profile, "deathTrigger", "Death");
        }

        private static void ValidateObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!property.isArray || property.arraySize != expected.Length)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected array size {expected.Length}, found {property.arraySize}.");
            }

            for (int i = 0; i < expected.Length; i++)
            {
                UnityEngine.Object actual = property.GetArrayElementAtIndex(i).objectReferenceValue;
                if (actual != expected[i])
                {
                    throw new InvalidOperationException($"{target.name}.{propertyName}[{i}] expected {expected[i].name}, found {(actual != null ? actual.name : "null")}.");
                }
            }
        }

        private static void ValidateAttackStep(
            UnityEngine.Object target,
            string propertyName,
            int index,
            string animationTrigger,
            float startupSeconds,
            float activeSeconds,
            float recoverySeconds,
            float inputBufferSeconds,
            float dodgeCancelAfterSeconds,
            float damage,
            float hitRadius,
            float hitDistance,
            float hitStopSeconds)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!property.isArray || property.arraySize <= index)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} missing attack step {index}.");
            }

            SerializedProperty step = property.GetArrayElementAtIndex(index);
            ValidateRelativeString(target, step, "animationTrigger", animationTrigger);
            ValidateRelativeFloat(target, step, "startupSeconds", startupSeconds);
            ValidateRelativeFloat(target, step, "activeSeconds", activeSeconds);
            ValidateRelativeFloat(target, step, "recoverySeconds", recoverySeconds);
            ValidateRelativeFloat(target, step, "inputBufferSeconds", inputBufferSeconds);
            ValidateRelativeFloat(target, step, "dodgeCancelAfterSeconds", dodgeCancelAfterSeconds);
            ValidateRelativeFloat(target, step, "damage", damage);
            ValidateRelativeFloat(target, step, "hitRadius", hitRadius);
            ValidateRelativeFloat(target, step, "hitDistance", hitDistance);
            ValidateRelativeFloat(target, step, "hitStopSeconds", hitStopSeconds);
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!Mathf.Approximately(property.floatValue, expected))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {property.floatValue}.");
            }
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.floatValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetVector3(UnityEngine.Object target, string propertyName, Vector3 value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.vector3Value = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.stringValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized array property {propertyName}.");
            }

            property.ClearArray();
            for (int i = 0; i < values.Length; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void ValidateString(UnityEngine.Object target, string propertyName, string expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!string.Equals(property.stringValue, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {property.stringValue}.");
            }
        }

        private static void ValidateRelativeFloat(UnityEngine.Object target, SerializedProperty owner, string propertyName, float expected)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            if (!Mathf.Approximately(property.floatValue, expected))
            {
                throw new InvalidOperationException($"{target.name}.{owner.propertyPath}.{propertyName} expected {expected}, found {property.floatValue}.");
            }
        }

        private static void ValidateRelativeString(UnityEngine.Object target, SerializedProperty owner, string propertyName, string expected)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            if (!string.Equals(property.stringValue, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{owner.propertyPath}.{propertyName} expected {expected}, found {property.stringValue}.");
            }
        }

        private static void ValidateCameraCueProfile(
            UnityEngine.Object target,
            string profileName,
            Vector3 localOffset,
            float planarDirectionOffset,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta,
            float durationSeconds,
            float finisherScale)
        {
            ValidateBool(target, $"{profileName}.enabled", true);
            ValidateVector3(target, $"{profileName}.localOffset", localOffset);
            ValidateFloat(target, $"{profileName}.planarDirectionOffset", planarDirectionOffset);
            ValidateFloat(target, $"{profileName}.fieldOfViewDelta", fieldOfViewDelta);
            ValidateFloat(target, $"{profileName}.cameraDistanceDelta", cameraDistanceDelta);
            ValidateFloat(target, $"{profileName}.focusHeightDelta", focusHeightDelta);
            ValidateFloat(target, $"{profileName}.durationSeconds", durationSeconds);
            ValidateFloat(target, $"{profileName}.finisherScale", finisherScale);
        }

        private static void ValidateAnimatorParameter(
            Animator animator,
            string parameterName,
            AnimatorControllerParameterType expectedType)
        {
            AnimatorController controller = RequireAnimatorController(animator);
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == parameterName && parameters[i].type == expectedType)
                {
                    return;
                }
            }

            throw new InvalidOperationException($"{controller.name} is missing {expectedType} parameter {parameterName}.");
        }

        private static void ValidateAnimatorStateMotion(Animator animator, string stateName, string expectedMotionPath)
        {
            AnimatorController controller = RequireAnimatorController(animator);
            for (int i = 0; i < controller.layers.Length; i++)
            {
                ChildAnimatorState[] states = controller.layers[i].stateMachine.states;
                for (int j = 0; j < states.Length; j++)
                {
                    AnimatorState state = states[j].state;
                    if (state == null || state.name != stateName)
                    {
                        continue;
                    }

                    string motionPath = AssetDatabase.GetAssetPath(state.motion).Replace('\\', '/');
                    if (!string.Equals(motionPath, expectedMotionPath, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"{controller.name}.{stateName} expected motion {expectedMotionPath}, found {motionPath}.");
                    }

                    return;
                }
            }

            throw new InvalidOperationException($"{controller.name} is missing Animator state {stateName}.");
        }

        private static void ValidateAnimationClipHeightFromFeet(string clipPath, string clipName)
        {
            if (AssetImporter.GetAtPath(clipPath) is not ModelImporter importer)
            {
                throw new InvalidOperationException($"Missing animation importer at {clipPath}.");
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0
                ? importer.clipAnimations
                : importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                if (!string.Equals(clips[i].name, clipName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!clips[i].heightFromFeet || clips[i].keepOriginalPositionY)
                {
                    throw new InvalidOperationException(
                        $"{clipPath} should use feet-based Y import for {clipName} so the death pose stays grounded.");
                }

                return;
            }

            throw new InvalidOperationException($"{clipPath} is missing imported clip {clipName}.");
        }

        private static void ValidateAnimatorStateSpeed(Animator animator, string stateName, float expectedSpeed)
        {
            AnimatorController controller = RequireAnimatorController(animator);
            AnimatorState state = FindAnimatorState(controller, stateName);
            if (state == null)
            {
                throw new InvalidOperationException($"{controller.name} is missing Animator state {stateName}.");
            }

            if (!Mathf.Approximately(state.speed, expectedSpeed))
            {
                throw new InvalidOperationException($"{controller.name}.{stateName} expected speed {expectedSpeed}, found {state.speed}.");
            }
        }

        private static void ValidateStopStepTransitionResponsiveness(Animator animator)
        {
            AnimatorController controller = RequireAnimatorController(animator);
            int foundCount = 0;
            for (int i = 0; i < controller.layers.Length; i++)
            {
                AnimatorStateMachine stateMachine = controller.layers[i].stateMachine;
                foundCount += ValidateStopStepTransitions(controller, stateMachine.anyStateTransitions);

                ChildAnimatorState[] states = stateMachine.states;
                for (int j = 0; j < states.Length; j++)
                {
                    if (states[j].state != null)
                    {
                        foundCount += ValidateStopStepTransitions(controller, states[j].state.transitions);
                    }
                }
            }

            if (foundCount == 0)
            {
                throw new InvalidOperationException($"{controller.name} has no StopStep trigger transitions.");
            }
        }

        private static int ValidateStopStepTransitions(AnimatorController controller, AnimatorStateTransition[] transitions)
        {
            int foundCount = 0;
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition transition = transitions[i];
                if (!IsStopStepTransition(transition))
                {
                    continue;
                }

                foundCount++;
                if (transition.hasExitTime || transition.exitTime > 0f || transition.duration > 0.0151f)
                {
                    throw new InvalidOperationException($"{controller.name} StopStep transition must be immediate, found duration {transition.duration}, exitTime {transition.exitTime}, hasExitTime {transition.hasExitTime}.");
                }
            }

            return foundCount;
        }

        private static void ValidateAnimatorTriggerTransition(
            Animator animator,
            string sourceStateName,
            string destinationStateName,
            string triggerName)
        {
            AnimatorController controller = RequireAnimatorController(animator);
            for (int i = 0; i < controller.layers.Length; i++)
            {
                ChildAnimatorState[] states = controller.layers[i].stateMachine.states;
                AnimatorState source = null;
                AnimatorState destination = null;
                for (int j = 0; j < states.Length; j++)
                {
                    AnimatorState state = states[j].state;
                    if (state == null)
                    {
                        continue;
                    }

                    if (state.name == sourceStateName)
                    {
                        source = state;
                    }
                    else if (state.name == destinationStateName)
                    {
                        destination = state;
                    }
                }

                if (source == null || destination == null)
                {
                    continue;
                }

                AnimatorStateTransition[] transitions = source.transitions;
                for (int j = 0; j < transitions.Length; j++)
                {
                    AnimatorStateTransition transition = transitions[j];
                    if (transition == null || transition.destinationState != destination)
                    {
                        continue;
                    }

                    AnimatorCondition[] conditions = transition.conditions;
                    for (int k = 0; k < conditions.Length; k++)
                    {
                        if (conditions[k].mode == AnimatorConditionMode.If
                            && string.Equals(conditions[k].parameter, triggerName, StringComparison.Ordinal))
                        {
                            return;
                        }
                    }
                }
            }

            throw new InvalidOperationException($"{controller.name} is missing {sourceStateName} -> {destinationStateName} transition on trigger {triggerName}.");
        }

        private static void ValidateAnyStateTriggerTransition(Animator animator, string destinationStateName, string triggerName)
        {
            AnimatorController controller = RequireAnimatorController(animator);
            for (int i = 0; i < controller.layers.Length; i++)
            {
                AnimatorStateTransition[] transitions = controller.layers[i].stateMachine.anyStateTransitions;
                for (int j = 0; j < transitions.Length; j++)
                {
                    AnimatorStateTransition transition = transitions[j];
                    if (transition == null
                        || transition.destinationState == null
                        || transition.destinationState.name != destinationStateName)
                    {
                        continue;
                    }

                    AnimatorCondition[] conditions = transition.conditions;
                    for (int k = 0; k < conditions.Length; k++)
                    {
                        if (conditions[k].mode == AnimatorConditionMode.If
                            && string.Equals(conditions[k].parameter, triggerName, StringComparison.Ordinal))
                        {
                            return;
                        }
                    }
                }
            }

            throw new InvalidOperationException($"{controller.name} is missing Any State -> {destinationStateName} transition on trigger {triggerName}.");
        }

        private static void ValidateAnyStateTriggerPriority(Animator animator, string requiredFirstTrigger, params string[] lowerPriorityTriggers)
        {
            AnimatorController controller = RequireAnimatorController(animator);
            for (int i = 0; i < controller.layers.Length; i++)
            {
                AnimatorStateTransition[] transitions = controller.layers[i].stateMachine.anyStateTransitions;
                int requiredIndex = FindAnyStateTriggerTransitionIndex(transitions, requiredFirstTrigger);
                if (requiredIndex < 0)
                {
                    continue;
                }

                for (int j = 0; j < lowerPriorityTriggers.Length; j++)
                {
                    int lowerPriorityIndex = FindAnyStateTriggerTransitionIndex(transitions, lowerPriorityTriggers[j]);
                    if (lowerPriorityIndex >= 0 && requiredIndex > lowerPriorityIndex)
                    {
                        throw new InvalidOperationException(
                            $"{controller.name} Any State trigger {requiredFirstTrigger} must be evaluated before {lowerPriorityTriggers[j]} so death is not consumed by another reaction.");
                    }
                }

                return;
            }

            throw new InvalidOperationException($"{controller.name} is missing Any State trigger {requiredFirstTrigger}.");
        }

        private static int FindAnyStateTriggerTransitionIndex(AnimatorStateTransition[] transitions, string triggerName)
        {
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition transition = transitions[i];
                if (transition == null)
                {
                    continue;
                }

                AnimatorCondition[] conditions = transition.conditions;
                for (int j = 0; j < conditions.Length; j++)
                {
                    if (conditions[j].mode == AnimatorConditionMode.If
                        && string.Equals(conditions[j].parameter, triggerName, StringComparison.Ordinal))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static AnimatorController RequireAnimatorController(Animator animator)
        {
            if (animator.runtimeAnimatorController is AnimatorController controller)
            {
                return controller;
            }

            throw new InvalidOperationException($"{animator.name} must use an AnimatorController for action-foundation validation.");
        }

        private static AnimatorState FindAnimatorState(AnimatorController controller, string stateName)
        {
            for (int i = 0; i < controller.layers.Length; i++)
            {
                ChildAnimatorState[] states = controller.layers[i].stateMachine.states;
                for (int j = 0; j < states.Length; j++)
                {
                    AnimatorState state = states[j].state;
                    if (state != null && state.name == stateName)
                    {
                        return state;
                    }
                }
            }

            return null;
        }

        private static void ValidateVector3(UnityEngine.Object target, string propertyName, Vector3 expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if ((property.vector3Value - expected).sqrMagnitude > 0.0001f)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {property.vector3Value}.");
            }
        }

        private static void ValidateBool(UnityEngine.Object target, string propertyName, bool expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (property.boolValue != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {property.boolValue}.");
            }
        }

        private static SerializedProperty FindProperty(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            return property;
        }
    }
}
