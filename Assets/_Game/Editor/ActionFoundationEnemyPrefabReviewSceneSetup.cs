using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationEnemyPrefabReviewSceneSetup
    {
        public const string ReviewScenePath = "Assets/_Game/Scenes/ActionFoundationEnemyPrefabReview.unity";

        private const string ReviewEnemyRootName = "EnemyPrefabReview_SciFiSoldier_Melee";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Enemy Prefab Review Scene")]
        public static void ReapplyEnemyPrefabReviewSceneMenu()
        {
            EnsureEnemyPrefabReviewScene();
            Debug.Log("Reapplied ActionFoundation enemy prefab review scene.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Enemy Prefab Review Scene")]
        public static void ValidateEnemyPrefabReviewSceneMenu()
        {
            ValidateEnemyPrefabReviewScene();
            Debug.Log("ActionFoundation enemy prefab review scene validation passed.");
        }

        public static void EnsureEnemyPrefabReviewScene()
        {
            ActionFoundationEnemyPrefabSetup.EnsureEnemyPrefabCandidates();
            GameObject prefab = LoadAsset<GameObject>(ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath);
            Scene scene = EditorSceneManager.OpenScene(ActionFoundationProfileSetup.ScenePath, OpenSceneMode.Single);

            RemoveEnemySampleRoots(scene);

            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerCombatTargetSelector playerTargetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionCameraTargetBridge cameraTargetBridge = RequireObject<ActionCameraTargetBridge>(scene, "action camera target bridge");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");

            player.transform.SetPositionAndRotation(new Vector3(0f, 0f, -1.25f), Quaternion.LookRotation(Vector3.forward, Vector3.up));

            GameObject enemy = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (enemy == null)
            {
                throw new InvalidOperationException($"Could not instantiate prefab candidate {ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath} into review scene.");
            }

            enemy.name = ReviewEnemyRootName;
            enemy.transform.SetPositionAndRotation(new Vector3(0f, 0f, 2.35f), Quaternion.LookRotation(Vector3.back, Vector3.up));
            enemy.transform.localScale = Vector3.one;
            enemy.SetActive(true);

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(enemy, ReviewEnemyRootName);
            CombatHealth enemyHealth = RequireComponent<CombatHealth>(enemy, $"{ReviewEnemyRootName} health");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(enemy, $"{ReviewEnemyRootName} target sensor");
            EnemyActionCameraCueDriver enemyCameraCueDriver = RequireComponent<EnemyActionCameraCueDriver>(enemy, $"{ReviewEnemyRootName} enemy camera cue driver");

            SetObjectReference(targetSensor, "selfHealth", enemyHealth);
            SetObjectReferenceArray(targetSensor, "targetCandidates", new UnityEngine.Object[] { playerHealth });
            SetObjectReference(soldier, "targetSensor", targetSensor);
            SetObjectReference(soldier, "target", player.transform);
            SetObjectReference(soldier, "targetHealth", playerHealth);
            SetObjectReference(soldier, "selfHealth", enemyHealth);
            SetObjectReference(enemyCameraCueDriver, "agentSource", soldier);
            SetObjectReference(enemyCameraCueDriver, "cameraController", cameraController);
            SetObjectReference(enemyCameraCueDriver, "cueSpace", enemy.transform);

            ActionFoundationProfileSetup.ConfigurePlayerTargetSelector(
                playerTargetSelector,
                player.transform,
                playerHealth,
                cameraController.transform,
                new[] { enemyHealth });

            SetObjectReference(cameraTargetBridge, "cameraController", cameraController);
            SetObjectReference(cameraTargetBridge, "targetSelector", playerTargetSelector);
            SetObjectReference(cameraTargetBridge, "followTarget", player.transform);
            SetObjectReference(cameraController, "target", player.transform);
            SetObjectReference(cameraController, "threat", enemy.transform);

            SetObjectReference(encounter, "playerHealth", playerHealth);
            SetObjectReference(encounter, "enemyHealth", enemyHealth);
            ConfigureArenaInfluenceTargets(scene, player.transform, enemy.transform);

            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save enemy prefab review scene at {ReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        public static void ValidateEnemyPrefabReviewScene()
        {
            ActionFoundationEnemyPrefabSetup.ValidateEnemyPrefabCandidates();
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject prefab = LoadAsset<GameObject>(ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerCombatTargetSelector playerTargetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");
            BasicSoldierEnemy[] soldiers = CollectComponents<BasicSoldierEnemy>(scene);

            if (soldiers.Length != 1)
            {
                throw new InvalidOperationException($"Review scene should contain exactly one BasicSoldierEnemy, found {soldiers.Length}.");
            }

            BasicSoldierEnemy soldier = soldiers[0];
            if (!string.Equals(soldier.name, ReviewEnemyRootName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Review soldier should be named {ReviewEnemyRootName}, found {soldier.name}.");
            }

            string sourcePath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(soldier.gameObject)).Replace('\\', '/');
            if (!string.Equals(sourcePath, ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Review soldier should be a prefab instance of {ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath}, found {sourcePath}.");
            }

            if (PrefabUtility.GetCorrespondingObjectFromSource(soldier.gameObject) != prefab)
            {
                throw new InvalidOperationException("Review soldier should keep its prefab instance connection for Inspector review.");
            }

            CombatHealth enemyHealth = RequireComponent<CombatHealth>(soldier.gameObject, "review enemy health");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(soldier.gameObject, "review enemy target sensor");
            EnemyActionCameraCueDriver enemyCameraCueDriver = RequireComponent<EnemyActionCameraCueDriver>(soldier.gameObject, "review enemy camera cue driver");

            ValidateObjectReference(soldier, "targetSensor", targetSensor);
            ValidateObjectReference(soldier, "target", player.transform);
            ValidateObjectReference(soldier, "targetHealth", playerHealth);
            ValidateObjectReference(soldier, "selfHealth", enemyHealth);
            ValidateObjectReference(targetSensor, "selfHealth", enemyHealth);
            ValidateArrayReference(targetSensor, "targetCandidates", 0, playerHealth);
            ValidateObjectReference(enemyCameraCueDriver, "cameraController", cameraController);
            ValidateObjectReference(playerTargetSelector, "selfHealth", playerHealth);
            ValidateArrayReference(playerTargetSelector, "targetCandidates", 0, enemyHealth);
            ValidateObjectReference(cameraController, "target", player.transform);
            ValidateObjectReference(cameraController, "threat", soldier.transform);
            ValidateObjectReference(encounter, "playerHealth", playerHealth);
            ValidateObjectReference(encounter, "enemyHealth", enemyHealth);
        }

        private static void RemoveEnemySampleRoots(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = roots.Length - 1; i >= 0; i--)
            {
                GameObject root = roots[i];
                if (root == null || !ShouldRemoveRoot(root.name))
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static bool ShouldRemoveRoot(string rootName)
        {
            return rootName.StartsWith("Enemy_SciFiSoldier_", StringComparison.Ordinal)
                || rootName.StartsWith("ReadableAttackTelegraph", StringComparison.Ordinal);
        }

        private static void ConfigureArenaInfluenceTargets(Scene scene, Transform player, Transform enemy)
        {
            ActionFoundationArenaShapeInfluenceDriver[] drivers = CollectComponents<ActionFoundationArenaShapeInfluenceDriver>(scene);
            for (int i = 0; i < drivers.Length; i++)
            {
                SetObjectReferenceArray(drivers[i], "influenceTargets", new UnityEngine.Object[] { player, enemy });
            }
        }

        private static T RequireObject<T>(Scene scene, string label) where T : Component
        {
            T[] found = CollectComponents<T>(scene);
            if (found.Length == 0)
            {
                throw new InvalidOperationException($"Missing {label} in {scene.path}.");
            }

            return found[0];
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            T component = root.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{label} is missing required component {typeof(T).Name}.");
            }

            return component;
        }

        private static T[] CollectComponents<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                results.AddRange(roots[i].GetComponentsInChildren<T>(includeInactive: true));
            }

            return results.ToArray();
        }

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {assetPath}.");
            }

            return asset;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReferenceArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty array = RequireProperty(serializedObject, propertyName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void ValidateObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object expected)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual != expected)
            {
                string expectedName = expected != null ? expected.name : "null";
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expectedName}, found {actualName}.");
            }
        }

        private static void ValidateArrayReference(UnityEngine.Object target, string propertyName, int index, UnityEngine.Object expected)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (!array.isArray || array.arraySize <= index)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should contain index {index}.");
            }

            UnityEngine.Object actual = array.GetArrayElementAtIndex(index).objectReferenceValue;
            if (actual != expected)
            {
                string expectedName = expected != null ? expected.name : "null";
                string actualName = actual != null ? actual.name : "null";
                throw new InvalidOperationException($"{target.name}.{propertyName}[{index}] expected {expectedName}, found {actualName}.");
            }
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
    }
}
