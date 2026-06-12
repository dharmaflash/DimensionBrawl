using System;
using System.Collections.Generic;
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
    public static class ActionFoundationProfileSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";
        public const string ProfileRoot = "Assets/_Game/DesignData/Profiles/ActionFoundation";
        public const string PlayerActionProfilePath = ProfileRoot + "/DB_PlayerAction_ActionFoundation.asset";
        public const string CameraCueProfilePath = ProfileRoot + "/DB_ActionCameraCues_ActionFoundation.asset";
        public const string EnemyPatternProfilePath = ProfileRoot + "/DB_BasicSoldier_ClosePunish.asset";
        public const string EnemyLungePatternProfilePath = ProfileRoot + "/DB_BasicSoldier_LungeStrike.asset";
        public const string EnemyHeavyWindupPatternProfilePath = ProfileRoot + "/DB_BasicSoldier_HeavyWindup.asset";
        public const string ClosePunishEnemyRootName = "Enemy_SciFiSoldier_ClosePunish";
        public const string LungeStrikeEnemyRootName = "Enemy_SciFiSoldier_LungeStrike";
        public const string HeavyWindupEnemyRootName = "Enemy_SciFiSoldier_HeavyWindup";
        private const string LegacyEnemyRootName = "Enemy_SciFiSoldier_ActionFoundation";
        private const string EnemyVisualName = "MaintenanceWorker_BasicSoldierVisual";
        private const string EnemyPlaceholderBodyName = "SciFiSoldierPlaceholderBody";
        private const string EnemyTelegraphName = "ReadableAttackTelegraph";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Gameplay Profiles")]
        public static void ReapplyGameplayProfilesMenu()
        {
            EnsureProfileAssets(
                out PlayerActionProfile playerActionProfile,
                out ActionCameraCueProfile cameraCueProfile,
                out CombatAiPatternProfile enemyPatternProfile,
                out CombatAiPatternProfile enemyLungePatternProfile,
                out CombatAiPatternProfile enemyHeavyWindupPatternProfile);

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            playerActionProfile = LoadOrCreate<PlayerActionProfile>(PlayerActionProfilePath);
            cameraCueProfile = LoadOrCreate<ActionCameraCueProfile>(CameraCueProfilePath);
            enemyPatternProfile = LoadOrCreateEnemyPatternProfile(EnemyPatternProfilePath);
            enemyLungePatternProfile = LoadOrCreateEnemyPatternProfile(EnemyLungePatternProfilePath);
            enemyHeavyWindupPatternProfile = LoadOrCreateEnemyPatternProfile(EnemyHeavyWindupPatternProfilePath);

            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(roots, "action camera");
            ActionCameraCueDriver cameraCueDriver = RequireObject<ActionCameraCueDriver>(roots, "action camera cue driver");
            PlayerCombatTargetSelector targetSelector = EnsureComponent<PlayerCombatTargetSelector>(playerActions.gameObject);
            ActionCameraTargetBridge cameraTargetBridge = EnsureComponent<ActionCameraTargetBridge>(cameraController.gameObject);
            BasicSoldierEnemy soldier = RequirePrimarySoldier(roots);
            CombatHealth playerHealth = RequireComponent<CombatHealth>(playerActions.gameObject, "player health");

            SetObjectReference(playerActions, "actionProfile", playerActionProfile);
            SetObjectReference(playerActions, "targetSelector", targetSelector);
            SetObjectReference(cameraCueDriver, "cueProfile", cameraCueProfile);
            soldier.gameObject.name = ClosePunishEnemyRootName;
            SetObjectReference(soldier, "patternProfile", enemyPatternProfile);
            RemoveCameraAttachedEnemyCueDriver(cameraController);

            CombatHealth[] enemyCandidates = EnsurePatternSampleEnemies(
                scene,
                soldier,
                playerActions.transform,
                playerHealth,
                cameraController,
                enemyPatternProfile,
                enemyLungePatternProfile,
                enemyHeavyWindupPatternProfile);

            ConfigurePlayerTargetSelector(targetSelector, playerActions.transform, playerHealth, cameraController.transform, enemyCandidates);
            ConfigureCameraTargetBridge(cameraTargetBridge, cameraController, targetSelector, playerActions.transform);
            SetObjectReference(cameraController, "target", playerActions.transform);
            SetObjectReference(cameraController, "threat", enemyCandidates.Length > 0 ? enemyCandidates[0].transform : null);

            EditorUtility.SetDirty(playerActions);
            EditorUtility.SetDirty(cameraCueDriver);
            EditorUtility.SetDirty(soldier);
            EditorUtility.SetDirty(targetSelector);
            EditorUtility.SetDirty(cameraTargetBridge);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            ValidatePersistedSceneReferences();
            Debug.Log("Reapplied ActionFoundation gameplay profile assets and scene references.");
        }

        private static CombatHealth[] EnsurePatternSampleEnemies(
            Scene scene,
            BasicSoldierEnemy template,
            Transform player,
            CombatHealth playerHealth,
            ActionCameraController cameraController,
            CombatAiPatternProfile closePunishProfile,
            CombatAiPatternProfile lungeStrikeProfile,
            CombatAiPatternProfile heavyWindupProfile)
        {
            BasicSoldierEnemy closePunish = EnsurePatternSampleEnemy(scene, template, ClosePunishEnemyRootName);
            BasicSoldierEnemy lungeStrike = EnsurePatternSampleEnemy(scene, template, LungeStrikeEnemyRootName);
            BasicSoldierEnemy heavyWindup = EnsurePatternSampleEnemy(scene, template, HeavyWindupEnemyRootName);

            ConfigurePatternSampleEnemy(
                closePunish,
                ClosePunishEnemyRootName,
                "ClosePunish",
                closePunishProfile,
                new Vector3(0f, 0f, 3.25f),
                player,
                playerHealth,
                cameraController,
                null);
            ConfigurePatternSampleEnemy(
                lungeStrike,
                LungeStrikeEnemyRootName,
                "LungeStrike",
                lungeStrikeProfile,
                new Vector3(-3.2f, 0f, 5.5f),
                player,
                playerHealth,
                cameraController,
                "LungeStrike");
            ConfigurePatternSampleEnemy(
                heavyWindup,
                HeavyWindupEnemyRootName,
                "HeavyWindup",
                heavyWindupProfile,
                new Vector3(3.2f, 0f, 5.5f),
                player,
                playerHealth,
                cameraController,
                "HeavyWindup");

            return new[]
            {
                RequireComponent<CombatHealth>(closePunish.gameObject, $"{ClosePunishEnemyRootName} health"),
                RequireComponent<CombatHealth>(lungeStrike.gameObject, $"{LungeStrikeEnemyRootName} health"),
                RequireComponent<CombatHealth>(heavyWindup.gameObject, $"{HeavyWindupEnemyRootName} health")
            };
        }

        private static BasicSoldierEnemy EnsurePatternSampleEnemy(Scene scene, BasicSoldierEnemy template, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            BasicSoldierEnemy existing = FindRootComponent<BasicSoldierEnemy>(roots, rootName);
            if (existing != null)
            {
                return existing;
            }

            GameObject clone = UnityEngine.Object.Instantiate(template.gameObject);
            clone.name = rootName;
            SceneManager.MoveGameObjectToScene(clone, scene);
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(clone, rootName);
            return soldier;
        }

        private static void ConfigurePatternSampleEnemy(
            BasicSoldierEnemy soldier,
            string rootName,
            string patternId,
            CombatAiPatternProfile patternProfile,
            Vector3 position,
            Transform player,
            CombatHealth playerHealth,
            ActionCameraController cameraController,
            string childNameSuffix)
        {
            soldier.gameObject.name = rootName;
            soldier.transform.position = position;
            soldier.transform.rotation = BuildLookRotation(position, player.position);

            CharacterController characterController = RequireComponent<CharacterController>(soldier.gameObject, $"{rootName} character controller");
            CombatHealth selfHealth = RequireComponent<CombatHealth>(soldier.gameObject, $"{rootName} health");
            CombatTargetSensor targetSensor = EnsureComponent<CombatTargetSensor>(soldier.gameObject);
            EnemyAttackTelegraphPresenter telegraphPresenter = EnsureComponent<EnemyAttackTelegraphPresenter>(soldier.gameObject);
            CombatHitFeedback hitFeedback = EnsureComponent<CombatHitFeedback>(soldier.gameObject);
            EnemyActionCameraCueDriver enemyCameraCueDriver = EnsureComponent<EnemyActionCameraCueDriver>(soldier.gameObject);
            Animator animator = soldier.GetComponentInChildren<Animator>(includeInactive: true);
            GameObject telegraphObject = FindPatternChildObject(soldier.transform, "ReadableAttackTelegraph", childNameSuffix);
            Renderer telegraphRenderer = telegraphObject != null
                ? telegraphObject.GetComponentInChildren<Renderer>(includeInactive: true)
                : null;
            Renderer[] renderers = CollectFeedbackRenderers(soldier, telegraphRenderer);
            Renderer bodyRenderer = ResolvePrimaryBodyRenderer(renderers, telegraphRenderer);

            RenamePatternSampleChildren(soldier.transform, childNameSuffix);

            SetObjectReference(targetSensor, "selfHealth", selfHealth);
            SetFloat(targetSensor, "searchRadius", 12f);
            SetFloat(targetSensor, "retargetIntervalSeconds", 0.2f);
            SetObjectReferenceArray(targetSensor, "targetCandidates", new UnityEngine.Object[] { playerHealth });

            if (telegraphObject != null)
            {
                SetObjectReference(telegraphPresenter, "telegraphObject", telegraphObject);
                SetObjectReference(telegraphPresenter, "telegraphTransform", telegraphObject.transform);
            }

            if (telegraphRenderer != null)
            {
                SetObjectReference(telegraphPresenter, "telegraphRenderer", telegraphRenderer);
            }

            if (animator != null)
            {
                SetObjectReference(telegraphPresenter, "poseRoot", animator.transform);
                SetObjectReference(soldier, "animator", animator);
            }

            ConfigureSoldierSerializedFields(
                soldier,
                patternId,
                patternProfile,
                targetSensor,
                player,
                playerHealth,
                selfHealth,
                characterController,
                telegraphPresenter,
                telegraphObject,
                bodyRenderer);

            SetObjectReference(hitFeedback, "health", selfHealth);
            SetObjectReferenceArray(hitFeedback, "flashRenderers", renderers);
            SetBool(hitFeedback, "applyIdleColorOnEnable", false);

            SetObjectReference(enemyCameraCueDriver, "agentSource", soldier);
            SetObjectReference(enemyCameraCueDriver, "cameraController", cameraController);
            SetObjectReference(enemyCameraCueDriver, "cueSpace", soldier.transform);

            EditorUtility.SetDirty(soldier.gameObject);
            EditorUtility.SetDirty(soldier);
            EditorUtility.SetDirty(targetSensor);
            EditorUtility.SetDirty(telegraphPresenter);
            EditorUtility.SetDirty(hitFeedback);
            EditorUtility.SetDirty(enemyCameraCueDriver);
        }

        private static void ConfigureSoldierSerializedFields(
            BasicSoldierEnemy soldier,
            string patternId,
            CombatAiPatternProfile patternProfile,
            CombatTargetSensor targetSensor,
            Transform player,
            CombatHealth playerHealth,
            CombatHealth selfHealth,
            CharacterController characterController,
            EnemyAttackTelegraphPresenter telegraphPresenter,
            GameObject telegraphObject,
            Renderer bodyRenderer)
        {
            SerializedObject serializedObject = new SerializedObject(soldier);
            serializedObject.Update();
            SetObjectReference(serializedObject, "patternProfile", patternProfile);
            SetString(serializedObject, "patternId", patternId);
            SetObjectReference(serializedObject, "targetSensor", targetSensor);
            SetObjectReference(serializedObject, "target", player);
            SetObjectReference(serializedObject, "targetHealth", playerHealth);
            SetObjectReference(serializedObject, "selfHealth", selfHealth);
            SetObjectReference(serializedObject, "characterController", characterController);
            SetObjectReference(serializedObject, "telegraphPresenter", telegraphPresenter);
            SetObjectReference(serializedObject, "telegraphIndicator", telegraphObject);
            SetObjectReference(serializedObject, "bodyRenderer", bodyRenderer);
            serializedObject.ApplyModifiedProperties();

            SerializedProperty profileProperty = serializedObject.FindProperty("patternProfile");
            if (profileProperty == null || profileProperty.objectReferenceValue != patternProfile)
            {
                string expectedName = patternProfile != null ? patternProfile.name : "null";
                string actualName = profileProperty != null && profileProperty.objectReferenceValue != null
                    ? profileProperty.objectReferenceValue.name
                    : "null";
                throw new InvalidOperationException($"{soldier.name}.patternProfile expected {expectedName}, found {actualName} after batch assignment.");
            }

            EditorUtility.SetDirty(soldier);
            PrefabUtility.RecordPrefabInstancePropertyModifications(soldier);
            EditorSceneManager.MarkSceneDirty(soldier.gameObject.scene);
        }

        private static void ConfigurePlayerTargetSelector(
            PlayerCombatTargetSelector targetSelector,
            Transform player,
            CombatHealth playerHealth,
            Transform cameraTransform,
            CombatHealth[] enemyCandidates)
        {
            SetObjectReference(targetSelector, "selfHealth", playerHealth);
            SetObjectReference(targetSelector, "selectionOrigin", player);
            SetObjectReference(targetSelector, "viewReference", cameraTransform);
            SetObjectReferenceArray(targetSelector, "targetCandidates", enemyCandidates);
            SetFloat(targetSelector, "selectionRadius", 12f);
            SetFloat(targetSelector, "retargetIntervalSeconds", 0.12f);
            SetFloat(targetSelector, "contactStickinessSeconds", 0.35f);
            SetFloat(targetSelector, "distanceWeight", 0.35f);
            SetFloat(targetSelector, "ownerForwardWeight", 0.3f);
            SetFloat(targetSelector, "viewForwardWeight", 0.2f);
            SetFloat(targetSelector, "threatStateWeight", 0.35f);
            SetFloat(targetSelector, "currentTargetStickiness", 0.18f);
            SetFloat(targetSelector, "minimumReadableForwardDot", -0.35f);
        }

        private static void ConfigureCameraTargetBridge(
            ActionCameraTargetBridge cameraTargetBridge,
            ActionCameraController cameraController,
            PlayerCombatTargetSelector targetSelector,
            Transform player)
        {
            SetObjectReference(cameraTargetBridge, "cameraController", cameraController);
            SetObjectReference(cameraTargetBridge, "targetSelector", targetSelector);
            SetObjectReference(cameraTargetBridge, "followTarget", player);
        }

        private static void RemoveCameraAttachedEnemyCueDriver(ActionCameraController cameraController)
        {
            EnemyActionCameraCueDriver cameraAttachedDriver = cameraController.GetComponent<EnemyActionCameraCueDriver>();
            if (cameraAttachedDriver != null)
            {
                UnityEngine.Object.DestroyImmediate(cameraAttachedDriver);
            }
        }

        private static BasicSoldierEnemy RequirePrimarySoldier(GameObject[] roots)
        {
            return FindRootComponent<BasicSoldierEnemy>(roots, ClosePunishEnemyRootName)
                ?? FindRootComponent<BasicSoldierEnemy>(roots, LegacyEnemyRootName)
                ?? RequireObject<BasicSoldierEnemy>(roots, "basic soldier");
        }

        private static T FindRootComponent<T>(GameObject[] roots, string rootName) where T : Component
        {
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null && root.name == rootName)
                {
                    return root.GetComponent<T>();
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

        private static Quaternion BuildLookRotation(Vector3 fromPosition, Vector3 toPosition)
        {
            Vector3 direction = Vector3.ProjectOnPlane(toPosition - fromPosition, Vector3.up);
            return direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
        }

        private static GameObject FindPatternChildObject(Transform root, string baseName, string childNameSuffix)
        {
            if (!string.IsNullOrEmpty(childNameSuffix))
            {
                GameObject suffixed = FindChildObject(root, $"{baseName}_{childNameSuffix}");
                if (suffixed != null)
                {
                    return suffixed;
                }
            }

            return FindChildObject(root, baseName);
        }

        private static GameObject FindChildObject(Transform root, string objectName)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == objectName)
                {
                    return children[i].gameObject;
                }
            }

            return null;
        }

        private static Renderer[] CollectFeedbackRenderers(BasicSoldierEnemy soldier, Renderer telegraphRenderer)
        {
            Renderer[] allRenderers = soldier.GetComponentsInChildren<Renderer>(true);
            List<Renderer> renderers = new List<Renderer>(allRenderers.Length);
            Transform telegraphRoot = telegraphRenderer != null ? telegraphRenderer.transform : null;

            for (int i = 0; i < allRenderers.Length; i++)
            {
                Renderer renderer = allRenderers[i];
                if (renderer == null || renderer == telegraphRenderer)
                {
                    continue;
                }

                if (telegraphRoot != null && renderer.transform.IsChildOf(telegraphRoot))
                {
                    continue;
                }

                renderers.Add(renderer);
            }

            return renderers.ToArray();
        }

        private static Renderer ResolvePrimaryBodyRenderer(Renderer[] renderers, Renderer telegraphRenderer)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer != telegraphRenderer && renderer.gameObject.activeInHierarchy)
                {
                    return renderer;
                }
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer != telegraphRenderer)
                {
                    return renderer;
                }
            }

            return null;
        }

        private static void RenamePatternSampleChildren(Transform root, string childNameSuffix)
        {
            if (string.IsNullOrEmpty(childNameSuffix))
            {
                return;
            }

            RenameChildIfPresent(root, EnemyVisualName, childNameSuffix);
            RenameChildIfPresent(root, EnemyPlaceholderBodyName, childNameSuffix);
            RenameChildIfPresent(root, EnemyTelegraphName, childNameSuffix);
        }

        private static void RenameChildIfPresent(Transform root, string baseName, string childNameSuffix)
        {
            GameObject child = FindPatternChildObject(root, baseName, childNameSuffix);
            if (child != null)
            {
                child.name = $"{baseName}_{childNameSuffix}";
            }
        }

        public static void EnsureProfileAssets(
            out PlayerActionProfile playerActionProfile,
            out ActionCameraCueProfile cameraCueProfile,
            out CombatAiPatternProfile enemyPatternProfile,
            out CombatAiPatternProfile enemyLungePatternProfile,
            out CombatAiPatternProfile enemyHeavyWindupPatternProfile)
        {
            EnsureFolder(ProfileRoot);
            playerActionProfile = LoadOrCreate<PlayerActionProfile>(PlayerActionProfilePath);
            cameraCueProfile = LoadOrCreate<ActionCameraCueProfile>(CameraCueProfilePath);
            enemyPatternProfile = LoadOrCreateEnemyPatternProfile(EnemyPatternProfilePath);
            enemyLungePatternProfile = LoadOrCreateEnemyPatternProfile(EnemyLungePatternProfilePath);
            enemyHeavyWindupPatternProfile = LoadOrCreateEnemyPatternProfile(EnemyHeavyWindupPatternProfilePath);

            ConfigurePlayerActionProfile(playerActionProfile);
            ConfigureCameraCueProfile(cameraCueProfile);
            ConfigureEnemyPatternProfile(
                enemyPatternProfile,
                "SciFiSoldier.Basic",
                "ClosePunish",
                2.7f,
                540f,
                -24f,
                1.65f,
                -0.15f,
                0.65f,
                0.14f,
                0f,
                0.45f,
                15f,
                0.03f,
                0.24f,
                2f,
                1.4f,
                0.18f,
                CombatAiCameraCueKind.ClosePunish,
                0.85f,
                0.75f,
                0.6f,
                new Vector3(0.35f, 0.02f, 0.65f),
                new Vector3(1.05f, 0.02f, 1.55f),
                new Vector3(1.25f, 0.025f, 1.8f),
                new Vector3(0f, 0f, -0.08f),
                new Vector3(0f, 0f, 0.12f),
                new Color(1f, 0.45f, 0.08f, 1f),
                new Color(1f, 0.08f, 0.02f, 1f),
                Color.white,
                "MoveSpeed",
                "Attack",
                "Hit",
                "Death");
            ConfigureEnemyPatternProfile(
                enemyLungePatternProfile,
                "SciFiSoldier.Basic",
                "LungeStrike",
                2.45f,
                500f,
                -24f,
                2.15f,
                0f,
                0.82f,
                0.22f,
                4.25f,
                0.68f,
                22f,
                0.04f,
                0.28f,
                2.4f,
                0.6f,
                0.12f,
                CombatAiCameraCueKind.LungeStrike,
                1.1f,
                1.2f,
                0.6f,
                new Vector3(0.22f, 0.02f, 1.1f),
                new Vector3(0.46f, 0.02f, 3.0f),
                new Vector3(0.58f, 0.025f, 3.7f),
                new Vector3(0f, 0f, -0.16f),
                new Vector3(0f, 0f, 0.34f),
                new Color(0.35f, 0.75f, 1f, 1f),
                new Color(0.05f, 0.38f, 1f, 1f),
                new Color(0.85f, 0.95f, 1f, 1f),
                "MoveSpeed",
                "Attack",
                "Hit",
                "Death");
            ConfigureEnemyPatternProfile(
                enemyHeavyWindupPatternProfile,
                "SciFiSoldier.Basic",
                "HeavyWindup",
                2.1f,
                420f,
                -24f,
                1.9f,
                0.1f,
                1.05f,
                0.18f,
                1.25f,
                0.85f,
                30f,
                0.05f,
                0.32f,
                2.8f,
                0.35f,
                0.16f,
                CombatAiCameraCueKind.HeavyWindup,
                1.45f,
                1.25f,
                0.65f,
                new Vector3(0.55f, 0.02f, 0.9f),
                new Vector3(1.9f, 0.02f, 2.05f),
                new Vector3(2.25f, 0.03f, 2.35f),
                new Vector3(0f, 0f, -0.22f),
                new Vector3(0f, 0f, 0.18f),
                new Color(1f, 0.72f, 0.08f, 1f),
                new Color(1f, 0.02f, 0.02f, 1f),
                new Color(1f, 0.9f, 0.35f, 1f),
                "MoveSpeed",
                "Attack",
                "Hit",
                "Death");
            AssetDatabase.SaveAssets();
        }

        private static void ConfigurePlayerActionProfile(PlayerActionProfile profile)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SerializedProperty combo = serializedObject.FindProperty("basicCombo");
            combo.arraySize = 5;
            SetAttackStep(combo.GetArrayElementAtIndex(0), "Attack1", 0.12f, 0.08f, 0.28f, 0.10f, 0.06f, 20f, 0.55f, 1.35f, 0.03f);
            SetAttackStep(combo.GetArrayElementAtIndex(1), "Attack2", 0.14f, 0.09f, 0.32f, 0.10f, 0.08f, 24f, 0.60f, 1.45f, 0.03f);
            SetAttackStep(combo.GetArrayElementAtIndex(2), "Attack3", 0.16f, 0.10f, 0.30f, 0.12f, 0.10f, 34f, 0.70f, 1.55f, 0.04f);
            SetAttackStep(combo.GetArrayElementAtIndex(3), "Attack4", 0.17f, 0.10f, 0.34f, 0.12f, 0.12f, 40f, 0.72f, 1.62f, 0.05f);
            SetAttackStep(combo.GetArrayElementAtIndex(4), "Attack5", 0.20f, 0.12f, 0.46f, 0.12f, 0.14f, 56f, 0.82f, 1.75f, 0.05f);
            SetFloat(serializedObject, "comboResetSeconds", 0.75f);
            SetFloat(serializedObject, "comboQueueOpenAfterSeconds", 0.10f);
            SetFloat(serializedObject, "comboChainRecoveryRatio", 0.45f);
            SetFloat(serializedObject, "dodgeDurationSeconds", 0.56f);
            SetFloat(serializedObject, "dodgeInvulnerableFromSeconds", 0.05f);
            SetFloat(serializedObject, "dodgeInvulnerableToSeconds", 0.32f);
            SetFloat(serializedObject, "dodgeRecoverySeconds", 0.14f);
            SetFloat(serializedObject, "dodgeSpeed", 10.2f);
            SetString(serializedObject, "dodgeTrigger", "DodgeForward");
            SetString(serializedObject, "dodgeBackTrigger", "DodgeBack");
            SetString(serializedObject, "dodgeLeftTrigger", "DodgeLeft");
            SetString(serializedObject, "dodgeRightTrigger", "DodgeRight");
            SetString(serializedObject, "dodgingParameter", "IsDodging");
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureCameraCueProfile(ActionCameraCueProfile profile)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SetCameraCue(serializedObject.FindProperty("runStartCue"), new Vector3(0f, 0.02f, -0.10f), 0.08f, 0.8f, -0.08f, 0.01f, 0.20f, 1f);
            SetCameraCue(serializedObject.FindProperty("stopSettleCue"), new Vector3(0f, -0.02f, -0.06f), -0.02f, -0.8f, -0.12f, -0.02f, 0.22f, 1f);
            SetCameraCue(serializedObject.FindProperty("sharpTurnCue"), new Vector3(0.08f, 0f, -0.10f), 0.06f, 0.6f, -0.06f, 0f, 0.24f, 1f);
            SetCameraCue(serializedObject.FindProperty("dodgeCue"), new Vector3(0f, 0.04f, -0.20f), -0.18f, 2.2f, -0.20f, 0.03f, 0.28f, 1f);
            SetCameraCue(serializedObject.FindProperty("attackStartCue"), new Vector3(0f, -0.03f, 0.14f), 0.08f, -1.2f, 0.12f, -0.02f, 0.22f, 1.2f);
            SetCameraCue(serializedObject.FindProperty("attackHitCue"), new Vector3(0f, 0.03f, 0.12f), 0.06f, -1.8f, 0.16f, 0.01f, 0.18f, 1.3f);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureEnemyPatternProfile(
            CombatAiPatternProfile profile,
            string actorTypeId,
            string patternId,
            float approachSpeed,
            float turnRateDegrees,
            float gravity,
            float attackRange,
            float attackFacingDotThreshold,
            float telegraphSeconds,
            float activeSeconds,
            float activeLungeSpeed,
            float recoverySeconds,
            float damage,
            float hitStopSeconds,
            float hitReactionSeconds,
            float knockbackSpeed,
            float recoveryRetreatSpeed,
            float recoveryRetreatSeconds,
            CombatAiCameraCueKind cameraCueKind,
            float windupThreatLevel,
            float activeCameraCueStrength,
            float deathCameraCueStrength,
            Vector3 telegraphWindupStartScale,
            Vector3 telegraphWindupEndScale,
            Vector3 telegraphActiveScale,
            Vector3 windupPoseOffset,
            Vector3 activePoseOffset,
            Color windupStartColor,
            Color windupEndColor,
            Color activeColor,
            string moveSpeedParameter,
            string attackTrigger,
            string hitTrigger,
            string deathTrigger)
        {
            SerializedObject serializedObject = new SerializedObject(profile);
            SetString(serializedObject, "actorTypeId", actorTypeId);
            SetString(serializedObject, "patternId", patternId);
            SetFloat(serializedObject, "approachSpeed", approachSpeed);
            SetFloat(serializedObject, "turnRateDegrees", turnRateDegrees);
            SetFloat(serializedObject, "gravity", gravity);
            SetFloat(serializedObject, "attackRange", attackRange);
            SetFloat(serializedObject, "attackFacingDotThreshold", attackFacingDotThreshold);
            SetFloat(serializedObject, "telegraphSeconds", telegraphSeconds);
            SetFloat(serializedObject, "activeSeconds", activeSeconds);
            SetFloat(serializedObject, "activeLungeSpeed", activeLungeSpeed);
            SetFloat(serializedObject, "recoverySeconds", recoverySeconds);
            SetFloat(serializedObject, "damage", damage);
            SetFloat(serializedObject, "hitStopSeconds", hitStopSeconds);
            SetFloat(serializedObject, "hitReactionSeconds", hitReactionSeconds);
            SetFloat(serializedObject, "knockbackSpeed", knockbackSpeed);
            SetFloat(serializedObject, "recoveryRetreatSpeed", recoveryRetreatSpeed);
            SetFloat(serializedObject, "recoveryRetreatSeconds", recoveryRetreatSeconds);
            SetEnum(serializedObject, "cameraCueKind", (int)cameraCueKind);
            SetFloat(serializedObject, "windupThreatLevel", windupThreatLevel);
            SetFloat(serializedObject, "activeCameraCueStrength", activeCameraCueStrength);
            SetFloat(serializedObject, "deathCameraCueStrength", deathCameraCueStrength);
            SetVector3(serializedObject, "telegraphWindupStartScale", telegraphWindupStartScale);
            SetVector3(serializedObject, "telegraphWindupEndScale", telegraphWindupEndScale);
            SetVector3(serializedObject, "telegraphActiveScale", telegraphActiveScale);
            SetVector3(serializedObject, "windupPoseOffset", windupPoseOffset);
            SetVector3(serializedObject, "activePoseOffset", activePoseOffset);
            SetColor(serializedObject, "windupStartColor", windupStartColor);
            SetColor(serializedObject, "windupEndColor", windupEndColor);
            SetColor(serializedObject, "activeColor", activeColor);
            SetString(serializedObject, "moveSpeedParameter", moveSpeedParameter);
            SetString(serializedObject, "attackTrigger", attackTrigger);
            SetString(serializedObject, "hitTrigger", hitTrigger);
            SetString(serializedObject, "deathTrigger", deathTrigger);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                string loadedPath = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(loadedPath))
                {
                    throw new InvalidOperationException($"{assetPath} loaded as a non-persistent enemy pattern profile object.");
                }

                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static CombatAiPatternProfile LoadOrCreateEnemyPatternProfile(string assetPath)
        {
            CombatAiPatternProfile asset = AssetDatabase.LoadAssetAtPath<EnemyPatternProfile>(assetPath);
            if (asset == null)
            {
                asset = AssetDatabase.LoadAssetAtPath<CombatAiPatternProfile>(assetPath);
            }

            if (asset == null)
            {
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is CombatAiPatternProfile profile)
                    {
                        asset = profile;
                        break;
                    }
                }
            }

            if (asset == null && AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) is CombatAiPatternProfile scriptableProfile)
            {
                asset = scriptableProfile;
            }

            if (asset != null)
            {
                return asset;
            }

            if (AssetDatabase.AssetPathExists(assetPath))
            {
                throw new InvalidOperationException($"{assetPath} exists but did not load as an enemy pattern profile.");
            }

            asset = ScriptableObject.CreateInstance<EnemyPatternProfile>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static void SetAttackStep(
            SerializedProperty step,
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
            SetRelativeString(step, "animationTrigger", animationTrigger);
            SetRelativeFloat(step, "startupSeconds", startupSeconds);
            SetRelativeFloat(step, "activeSeconds", activeSeconds);
            SetRelativeFloat(step, "recoverySeconds", recoverySeconds);
            SetRelativeFloat(step, "inputBufferSeconds", inputBufferSeconds);
            SetRelativeFloat(step, "dodgeCancelAfterSeconds", dodgeCancelAfterSeconds);
            SetRelativeFloat(step, "damage", damage);
            SetRelativeFloat(step, "hitRadius", hitRadius);
            SetRelativeFloat(step, "hitDistance", hitDistance);
            SetRelativeFloat(step, "hitStopSeconds", hitStopSeconds);
        }

        private static void SetCameraCue(
            SerializedProperty cue,
            Vector3 localOffset,
            float planarDirectionOffset,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta,
            float durationSeconds,
            float finisherScale)
        {
            SetRelativeBool(cue, "enabled", true);
            SetRelativeVector3(cue, "localOffset", localOffset);
            SetRelativeFloat(cue, "planarDirectionOffset", planarDirectionOffset);
            SetRelativeFloat(cue, "fieldOfViewDelta", fieldOfViewDelta);
            SetRelativeFloat(cue, "cameraDistanceDelta", cameraDistanceDelta);
            SetRelativeFloat(cue, "focusHeightDelta", focusHeightDelta);
            SetRelativeFloat(cue, "durationSeconds", durationSeconds);
            SetRelativeFloat(cue, "finisherScale", finisherScale);
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            if (property.objectReferenceValue != value)
            {
                string expectedName = value != null ? value.name : "null";
                string actualName = property.objectReferenceValue != null ? property.objectReferenceValue.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expectedName}, found {actualName} after assignment.");
            }

            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();
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
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.boolValue = value;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.floatValue = value;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.stringValue = value;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static void ValidatePersistedSceneReferences()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            ActionCameraCueDriver cameraCueDriver = RequireObject<ActionCameraCueDriver>(roots, "action camera cue driver");
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");

            ValidateObjectReferenceAssetPath(playerActions, "actionProfile", PlayerActionProfilePath);
            ValidateObjectReferenceAssetPath(cameraCueDriver, "cueProfile", CameraCueProfilePath);
            ValidateObjectReferenceAssetPath(soldier, "patternProfile", EnemyPatternProfilePath);
        }

        private static void ValidateObjectReferenceAssetPath(UnityEngine.Object target, string propertyName, string expectedPath)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");
            }

            UnityEngine.Object actual = property.objectReferenceValue;
            if (actual == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected asset {expectedPath}, found null after scene save.");
            }

            string actualPath = AssetDatabase.GetAssetPath(actual).Replace('\\', '/');
            if (!string.Equals(actualPath, expectedPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected asset {expectedPath}, found {actualPath} after scene save.");
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.floatValue = value;
        }

        private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.vector3Value = value;
        }

        private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.colorValue = value;
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.objectReferenceValue = value;
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.stringValue = value;
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = RequireProperty(serializedObject, propertyName);
            property.enumValueIndex = value;
        }

        private static void SetRelativeFloat(SerializedProperty owner, string propertyName, float value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.floatValue = value;
        }

        private static void SetRelativeString(SerializedProperty owner, string propertyName, string value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.stringValue = value;
        }

        private static void SetRelativeBool(SerializedProperty owner, string propertyName, bool value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.boolValue = value;
        }

        private static void SetRelativeVector3(SerializedProperty owner, string propertyName, Vector3 value)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            property.vector3Value = value;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static T RequireObject<T>(GameObject[] roots, string label) where T : Component
        {
            for (int i = 0; i < roots.Length; i++)
            {
                T component = roots[i].GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            throw new InvalidOperationException($"Missing required {label}.");
        }

        private static T EnsureComponent<T>(GameObject owner) where T : Component
        {
            if (!owner.TryGetComponent(out T component))
            {
                component = owner.AddComponent<T>();
            }

            return component;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            int separatorIndex = folderPath.LastIndexOf('/');
            string parent = folderPath.Substring(0, separatorIndex);
            string name = folderPath.Substring(separatorIndex + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
