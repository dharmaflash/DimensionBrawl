using System;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationSceneValidator
    {
        [MenuItem("DimensionBrawl/Reapply Action Foundation CombatGirl Materials")]
        public static void ReapplyCombatGirlMaterialsMenu()
        {
            Scene scene = OpenSceneForRepair();

            GameObject[] roots = scene.GetRootGameObjects();
            GameObject playerVisual = FindNamedObject(roots, PlayerVisualName) ?? FindNamedObject(roots, LegacyPlayerVisualName);
            if (playerVisual == null)
            {
                throw new InvalidOperationException($"Missing required {PlayerVisualName} in {ScenePath}.");
            }

            playerVisual.name = PlayerVisualName;
            int reassignedCount = ReapplyCombatGirlMaterials(playerVisual);
            SaveRepairChanges(scene);
            Debug.Log($"Reapplied CombatGirl materials on {reassignedCount} renderer slots in {ScenePath}.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation CombatGirl Weapon Sockets")]
        public static void ReapplyCombatGirlWeaponSocketsMenu()
        {
            Scene scene = OpenSceneForRepair();

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
            SaveRepairChanges(scene);
            Debug.Log("Reapplied CombatGirl weapon socket bindings in ActionFoundationTest.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation StopStep Responsiveness")]
        public static void ReapplyStopStepResponsivenessMenu()
        {
            Scene scene = OpenSceneForRepair();

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerMovementController movement = RequireObject<PlayerMovementController>(roots, "player movement");
            SetFloat(movement, "stopSettleInputHoldSeconds", 0.16f);
            SetFloat(movement, "animatorMoveDampSeconds", 0.06f);
            EditorUtility.SetDirty(movement);

            ConfigureStopStepClip();
            ConfigureStopStepAnimator();

            SaveRepairChanges(scene);
            Debug.Log("Reapplied StopStep responsiveness tuning in ActionFoundationTest.");
        }

        [MenuItem("DimensionBrawl/Reapply Action Foundation Shared Combat AI")]
        public static void ReapplySharedCombatAiMenu()
        {
            Scene scene = OpenSceneForRepair();

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
            SaveRepairChanges(scene);
            Debug.Log("Reapplied shared combat AI target sensor wiring in ActionFoundationTest.");
        }

        private static Scene OpenSceneForRepair()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            return scene;
        }

        private static void SaveRepairChanges(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
