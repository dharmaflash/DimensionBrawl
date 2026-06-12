using System;
using System.Collections.Generic;
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
    public static class ActionFoundationSceneValidator
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";

        [MenuItem("DimensionBrawl/Validate Action Foundation Test Scene")]
        public static void ValidateMenu()
        {
            ValidateSceneAsset();
            Debug.Log("Action foundation test scene validation passed.");
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
            PlayerMovementController movement = RequireObject<PlayerMovementController>(roots, "player movement");
            PlayerActionController playerActions = RequireObject<PlayerActionController>(roots, "player actions");
            BasicSoldierEnemy soldier = RequireObject<BasicSoldierEnemy>(roots, "basic soldier");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(roots, "action camera");
            Type dodgeFeedbackType = RequireType("DimensionBrawl.Presentation.PlayerDodgeFeedback, DimensionBrawl.Runtime");
            Component dodgeFeedback = RequireObject(roots, dodgeFeedbackType, "player dodge feedback");
            List<CombatHitFeedback> hitFeedbacks = CollectObjects<CombatHitFeedback>(roots);
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(roots, "test encounter");

            List<CombatHealth> healths = CollectObjects<CombatHealth>(roots);
            RequireAtLeast(healths.Count, 2, "player and enemy health components");
            RequireAtLeast(hitFeedbacks.Count, 2, "player and enemy hit feedback components");

            ValidateReference(movement, "referenceCamera");
            ValidateReference(playerActions, "movement");
            ValidateReference(playerActions, "health");
            ValidateArraySize(playerActions, "basicCombo", 3);
            ValidateFloat(playerActions, "dodgeDurationSeconds", 0.62f);
            ValidateFloat(playerActions, "dodgeInvulnerableFromSeconds", 0.05f);
            ValidateFloat(playerActions, "dodgeInvulnerableToSeconds", 0.40f);
            ValidateFloat(playerActions, "dodgeRecoverySeconds", 0.18f);

            ValidateReference(soldier, "target");
            ValidateReference(soldier, "targetHealth");
            ValidateReference(soldier, "selfHealth");
            ValidateReference(soldier, "telegraphIndicator");
            ValidateFloat(soldier, "telegraphSeconds", 0.65f);
            ValidateFloat(soldier, "activeSeconds", 0.14f);
            ValidateFloat(soldier, "recoverySeconds", 0.45f);
            ValidateFloat(soldier, "hitReactionSeconds", 0.24f);

            ValidateReference(cameraController, "target");
            ValidateReference(cameraController, "threat");
            ValidateFloat(cameraController, "defaultCueSeconds", 0.24f);

            ValidateReference(dodgeFeedback, "actionController");
            ValidateArraySize(dodgeFeedback, "targetRenderers", 2);

            ValidateReference(encounter, "playerHealth");
            ValidateReference(encounter, "enemyHealth");
            ValidateReference(encounter, "winMarker");
            ValidateReference(encounter, "failMarker");
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

        private static void ValidateArraySize(UnityEngine.Object target, string propertyName, int expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!property.isArray || property.arraySize != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected array size {expected}, found {property.arraySize}.");
            }
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            SerializedProperty property = FindProperty(target, propertyName);
            if (!Mathf.Approximately(property.floatValue, expected))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {property.floatValue}.");
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
