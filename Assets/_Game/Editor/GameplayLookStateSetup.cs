using System;
using System.Collections.Generic;
using System.Linq;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Gives the two Olympus product scenes one explicit gameplay look owner.
    /// The environment Volume remains active; presentation Volumes start at
    /// weight zero and may only be raised through GameplayLookStateController.
    /// </summary>
    public static class GameplayLookStateSetup
    {
        public const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        public const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";

        private const string RuntimeRootName =
            "OlympusCorridorInvasionLookdev_RuntimeFreePass";
        private const string GameplayBaseVolumeName =
            "OlympusCorridor_GlobalPostProcess";
        private const string CharacterFocusVolumeName =
            "InoriPresentation_WarmPostProcess";
        private const string PhaseTwoVolumeName =
            "AkazaPhase2_SourceSoftPostVolume";

        [MenuItem("DimensionBrawl/Presentation/Apply Olympus Gameplay Look States")]
        public static void ApplyMenu()
        {
            ApplyAll(restoreOriginalScene: true);
            Debug.Log("Olympus gameplay look-state setup passed.");
        }

        public static void RunBatchSetup()
        {
            try
            {
                ApplyAll(restoreOriginalScene: false);
                Debug.Log("Olympus gameplay look-state setup passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static GameplayLookStateController ConfigureLoadedScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException("A loaded product scene is required.");
            }

            bool isStation = string.Equals(
                scene.path,
                StationScenePath,
                StringComparison.OrdinalIgnoreCase);
            bool isCorridor = string.Equals(
                scene.path,
                CorridorScenePath,
                StringComparison.OrdinalIgnoreCase);
            if (!isStation && !isCorridor)
            {
                throw new InvalidOperationException(
                    $"Unsupported gameplay look-state scene: {scene.path}");
            }

            GameObject runtimeRoot = FindUniqueSceneObject(scene, RuntimeRootName);
            Volume gameplayBase = RequireVolume(scene, GameplayBaseVolumeName);
            Volume characterFocus = RequireVolume(scene, CharacterFocusVolumeName);
            GameplayLookStateController controller =
                runtimeRoot.GetComponent<GameplayLookStateController>()
                ?? runtimeRoot.AddComponent<GameplayLookStateController>();

            gameplayBase.isGlobal = true;
            gameplayBase.priority = 40f;
            gameplayBase.weight = 1f;
            characterFocus.isGlobal = true;
            characterFocus.priority = 95f;
            characterFocus.weight = 0f;

            var bindings = new List<GameplayLookStateController.OverlayBinding>
            {
                new GameplayLookStateController.OverlayBinding(
                    GameplayLookState.CharacterFocus,
                    characterFocus,
                    0.15f,
                    0.12f),
            };

            Volume phaseTwo = null;
            AkazaPhase2CinematicLookDriver lookDriver = null;
            if (isStation)
            {
                phaseTwo = RequireVolume(scene, PhaseTwoVolumeName);
                phaseTwo.isGlobal = true;
                phaseTwo.priority = 220f;
                phaseTwo.weight = 0f;
                bindings.Add(new GameplayLookStateController.OverlayBinding(
                    GameplayLookState.Phase2Cinematic,
                    phaseTwo,
                    0f,
                    0f));

                lookDriver = FindUniqueSceneComponent<AkazaPhase2CinematicLookDriver>(scene);
            }

            controller.Configure(gameplayBase, bindings.ToArray());
            lookDriver?.ConfigureLookStateController(controller);

            EditorUtility.SetDirty(gameplayBase);
            EditorUtility.SetDirty(characterFocus);
            EditorUtility.SetDirty(controller);
            if (phaseTwo != null)
            {
                EditorUtility.SetDirty(phaseTwo);
            }

            if (lookDriver != null)
            {
                EditorUtility.SetDirty(lookDriver);
            }

            Validate(scene, controller, gameplayBase, characterFocus, phaseTwo, lookDriver);
            return controller;
        }

        private static void ApplyAll(bool restoreOriginalScene)
        {
            EnsureNoDirtyScenes();
            string originalScenePath = SceneManager.GetActiveScene().path;
            try
            {
                ApplyScene(CorridorScenePath);
                ApplyScene(StationScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                if (restoreOriginalScene && !string.IsNullOrWhiteSpace(originalScenePath))
                {
                    EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
                }
                else if (!restoreOriginalScene)
                {
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }
            }
        }

        private static void ApplyScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ConfigureLoadedScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Could not save {scenePath}.");
            }
        }

        private static void EnsureNoDirtyScenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.IsValid() && scene.isLoaded && scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"Save or discard dirty scene '{scene.path}' before applying gameplay look states.");
                }
            }
        }

        private static void Validate(
            Scene scene,
            GameplayLookStateController controller,
            Volume gameplayBase,
            Volume characterFocus,
            Volume phaseTwo,
            AkazaPhase2CinematicLookDriver lookDriver)
        {
            if (controller == null
                || controller.GameplayBaseVolume != gameplayBase
                || controller.CurrentState != GameplayLookState.GameplayBase
                || !controller.HasBinding(GameplayLookState.CharacterFocus)
                || controller.GetOverlayVolume(GameplayLookState.CharacterFocus) != characterFocus
                || !gameplayBase.isGlobal
                || Mathf.Abs(gameplayBase.priority - 40f) > 0.0001f
                || Mathf.Abs(gameplayBase.weight - 1f) > 0.0001f
                || !characterFocus.isGlobal
                || Mathf.Abs(characterFocus.priority - 95f) > 0.0001f
                || characterFocus.weight > 0.0001f)
            {
                throw new InvalidOperationException(
                    $"Gameplay look-state base/CharacterFocus contract failed in {scene.path}.");
            }

            if (phaseTwo == null)
            {
                if (controller.HasBinding(GameplayLookState.Phase2Cinematic))
                {
                    throw new InvalidOperationException(
                        "Corridor may not claim a Phase2 cinematic overlay.");
                }

                return;
            }

            if (lookDriver == null
                || lookDriver.LookStateController != controller
                || !controller.HasBinding(GameplayLookState.Phase2Cinematic)
                || controller.GetOverlayVolume(GameplayLookState.Phase2Cinematic) != phaseTwo
                || !phaseTwo.isGlobal
                || Mathf.Abs(phaseTwo.priority - 220f) > 0.0001f
                || phaseTwo.weight > 0.0001f)
            {
                throw new InvalidOperationException(
                    "Station Phase2 cinematic look-state contract failed.");
            }
        }

        private static Volume RequireVolume(Scene scene, string gameObjectName)
        {
            GameObject target = FindUniqueSceneObject(scene, gameObjectName);
            Volume volume = target.GetComponent<Volume>();
            if (volume == null)
            {
                throw new InvalidOperationException(
                    $"{gameObjectName} in {scene.path} has no Volume component.");
            }

            return volume;
        }

        private static T FindUniqueSceneComponent<T>(Scene scene)
            where T : Component
        {
            T[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one {typeof(T).Name} in {scene.path}, found {matches.Length}.");
            }

            return matches[0];
        }

        private static GameObject FindUniqueSceneObject(Scene scene, string objectName)
        {
            GameObject[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(includeInactive: true))
                .Where(transform => transform.name == objectName)
                .Select(transform => transform.gameObject)
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one '{objectName}' in {scene.path}, found {matches.Length}.");
            }

            return matches[0];
        }
    }
}
