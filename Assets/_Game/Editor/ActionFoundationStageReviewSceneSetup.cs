using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationStageReviewSceneSetup
    {
        public const string BreakGateReviewScenePath = "Assets/_Game/Scenes/ActionFoundationStageBreakGateReview.unity";

        private const string BreakGateTemplatePath =
            ActionFoundationStageDesignSetup.TemplateRoot + "/DB_StageTemplate_S1_1_BreakGate.asset";
        private const string StageReviewRootPrefix = "StageBreakGateReview_";

        [MenuItem("DimensionBrawl/Reapply Action Foundation Stage BreakGate Review Scene")]
        public static void ReapplyStageBreakGateReviewSceneMenu()
        {
            EnsureStageBreakGateReviewScene();
            Debug.Log("Reapplied ActionFoundation S1-1 Break Gate stage review scene.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Stage BreakGate Review Scene")]
        public static void ValidateStageBreakGateReviewSceneMenu()
        {
            ValidateStageBreakGateReviewScene();
            Debug.Log("ActionFoundation S1-1 Break Gate stage review scene validation passed.");
        }

        public static void EnsureStageBreakGateReviewScene()
        {
            ActionFoundationStageDesignSetup.ValidateStageDesignAssets();
            ActionFoundationEnemyRoleCandidateSetup.ValidateEnemyRoleCandidates();
            ValidateBreakGateRouteTemplate();

            Scene scene = EditorSceneManager.OpenScene(ActionFoundationProfileSetup.ScenePath, OpenSceneMode.Single);
            RemoveEnemySampleRoots(scene);

            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerCombatTargetSelector playerTargetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionCameraTargetBridge cameraTargetBridge = RequireObject<ActionCameraTargetBridge>(scene, "action camera target bridge");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");

            StageReviewEnemySpec[] specs = GetBreakGateRouteSpecs();
            var enemyHealths = new CombatHealth[specs.Length];
            var enemyTransforms = new Transform[specs.Length];

            player.transform.SetPositionAndRotation(
                new Vector3(0f, 0f, -10f),
                Quaternion.LookRotation(Vector3.forward, Vector3.up));

            for (int i = 0; i < specs.Length; i++)
            {
                GameObject enemy = InstantiateReviewEnemy(scene, specs[i], player.transform.position);
                BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(enemy, specs[i].RootName);
                CombatHealth enemyHealth = RequireComponent<CombatHealth>(enemy, $"{specs[i].RootName} health");
                CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(enemy, $"{specs[i].RootName} target sensor");
                EnemyActionCameraCueDriver enemyCameraCueDriver =
                    RequireComponent<EnemyActionCameraCueDriver>(enemy, $"{specs[i].RootName} enemy camera cue driver");

                SetObjectReference(targetSensor, "selfHealth", enemyHealth);
                SetObjectReferenceArray(targetSensor, "targetCandidates", new UnityEngine.Object[] { playerHealth });
                SetObjectReference(soldier, "targetSensor", targetSensor);
                SetObjectReference(soldier, "target", null);
                SetObjectReference(soldier, "targetHealth", null);
                SetObjectReference(soldier, "selfHealth", enemyHealth);
                SetObjectReference(enemyCameraCueDriver, "agentSource", soldier);
                SetObjectReference(enemyCameraCueDriver, "cameraController", cameraController);
                SetObjectReference(enemyCameraCueDriver, "cueSpace", enemy.transform);

                enemyHealths[i] = enemyHealth;
                enemyTransforms[i] = enemy.transform;
            }

            ActionFoundationProfileSetup.ConfigurePlayerTargetSelector(
                playerTargetSelector,
                player.transform,
                playerHealth,
                cameraController.transform,
                enemyHealths);

            SetObjectReference(cameraTargetBridge, "cameraController", cameraController);
            SetObjectReference(cameraTargetBridge, "targetSelector", playerTargetSelector);
            SetObjectReference(cameraTargetBridge, "followTarget", player.transform);
            SetObjectReference(cameraController, "target", player.transform);
            SetObjectReference(cameraController, "threat", enemyTransforms[0]);
            SetBool(cameraController, "useDeviceFallbackWhenActionMissing", false);

            SetObjectReference(encounter, "playerHealth", playerHealth);
            SetObjectReference(encounter, "enemyHealth", enemyHealths[enemyHealths.Length - 1]);
            ConfigureArenaInfluenceTargets(scene, player.transform, enemyTransforms);

            if (!EditorSceneManager.SaveScene(scene, BreakGateReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save stage review scene at {BreakGateReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        public static void ValidateStageBreakGateReviewScene()
        {
            ActionFoundationStageDesignSetup.ValidateStageDesignAssets();
            ActionFoundationEnemyRoleCandidateSetup.ValidateEnemyRoleCandidates();
            ValidateBreakGateRouteTemplate();

            Scene scene = EditorSceneManager.OpenScene(BreakGateReviewScenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerCombatTargetSelector playerTargetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");
            StageReviewEnemySpec[] specs = GetBreakGateRouteSpecs();

            if (CollectComponents<BasicSoldierEnemy>(scene).Length != specs.Length)
            {
                throw new InvalidOperationException($"Break Gate review scene should contain {specs.Length} BasicSoldierEnemy instances.");
            }

            for (int i = 0; i < specs.Length; i++)
            {
                StageReviewEnemySpec spec = specs[i];
                GameObject enemy = RequireRoot(scene, spec.RootName);
                string sourcePath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(enemy)).Replace('\\', '/');
                if (!string.Equals(sourcePath, spec.PrefabPath, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{spec.RootName} should be an instance of {spec.PrefabPath}, found {sourcePath}.");
                }

                BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(enemy, spec.RootName);
                CombatHealth enemyHealth = RequireComponent<CombatHealth>(enemy, $"{spec.RootName} health");
                CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(enemy, $"{spec.RootName} target sensor");
                EnemyActionCameraCueDriver enemyCameraCueDriver =
                    RequireComponent<EnemyActionCameraCueDriver>(enemy, $"{spec.RootName} enemy camera cue driver");

                ValidateObjectReference(soldier, "targetSensor", targetSensor);
                ValidateObjectReference(soldier, "target", null);
                ValidateObjectReference(soldier, "targetHealth", null);
                ValidateObjectReference(soldier, "selfHealth", enemyHealth);
                ValidateObjectReference(targetSensor, "selfHealth", enemyHealth);
                ValidateArrayReference(targetSensor, "targetCandidates", 0, playerHealth);
                ValidateObjectReference(enemyCameraCueDriver, "agentSource", soldier);
                ValidateObjectReference(enemyCameraCueDriver, "cameraController", cameraController);
                ValidateObjectReference(enemyCameraCueDriver, "cueSpace", enemy.transform);
                ValidateArrayReference(playerTargetSelector, "targetCandidates", i, enemyHealth);

                if (Vector3.Distance(enemy.transform.position, spec.Position) > 0.01f)
                {
                    throw new InvalidOperationException($"{spec.RootName} is not at its authored route-review position.");
                }
            }

            ValidateObjectReference(playerTargetSelector, "selfHealth", playerHealth);
            ValidateObjectReference(cameraController, "target", player.transform);
            ValidateObjectReference(cameraController, "threat", RequireRoot(scene, specs[0].RootName).transform);
            ValidateBool(cameraController, "useDeviceFallbackWhenActionMissing", false);
            ValidateObjectReference(encounter, "playerHealth", playerHealth);
            ValidateObjectReference(
                encounter,
                "enemyHealth",
                RequireComponent<CombatHealth>(RequireRoot(scene, specs[specs.Length - 1].RootName), "final review enemy health"));
        }

        private static GameObject InstantiateReviewEnemy(Scene scene, StageReviewEnemySpec spec, Vector3 playerPosition)
        {
            GameObject prefab = LoadAsset<GameObject>(spec.PrefabPath);
            GameObject enemy = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (enemy == null)
            {
                throw new InvalidOperationException($"Could not instantiate stage review prefab {spec.PrefabPath}.");
            }

            Vector3 toPlayer = Vector3.ProjectOnPlane(playerPosition - spec.Position, Vector3.up).normalized;
            if (toPlayer.sqrMagnitude <= 0f)
            {
                toPlayer = Vector3.back;
            }

            enemy.name = spec.RootName;
            enemy.transform.SetPositionAndRotation(spec.Position, Quaternion.LookRotation(toPlayer, Vector3.up));
            enemy.transform.localScale = Vector3.one;
            enemy.SetActive(true);
            return enemy;
        }

        private static void ValidateBreakGateRouteTemplate()
        {
            LinearStageTemplateProfile template = LoadAsset<LinearStageTemplateProfile>(BreakGateTemplatePath);
            if (!string.Equals(template.StageTemplateId, "S1-1.BreakGate", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{BreakGateTemplatePath} is not the S1-1 Break Gate template.");
            }

            var expectedSegments = new[]
            {
                LinearStageSegmentKind.EntryRead,
                LinearStageSegmentKind.BasicPressure,
                LinearStageSegmentKind.BreakGate,
                LinearStageSegmentKind.Relief,
                LinearStageSegmentKind.FinalStand
            };
            var expectedObjectives = new[]
            {
                LinearStageObjectiveKind.ReadThreat,
                LinearStageObjectiveKind.PunishRecovery,
                LinearStageObjectiveKind.BreakGuard,
                LinearStageObjectiveKind.RecoverPosition,
                LinearStageObjectiveKind.FinalClear
            };

            if (template.SegmentCount != expectedSegments.Length)
            {
                throw new InvalidOperationException($"S1-1 Break Gate should have {expectedSegments.Length} review segments.");
            }

            for (int i = 0; i < expectedSegments.Length; i++)
            {
                LinearStageSegmentProfile segment = template.GetSegment(i);
                if (segment.SegmentKind != expectedSegments[i])
                {
                    throw new InvalidOperationException($"S1-1 segment {i} expected {expectedSegments[i]}, found {segment.SegmentKind}.");
                }

                if (segment.PocketCount <= 0 || segment.GetPocket(0).ObjectiveKind != expectedObjectives[i])
                {
                    throw new InvalidOperationException($"S1-1 segment {segment.SegmentId} does not expose the expected first objective {expectedObjectives[i]}.");
                }
            }
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
                || rootName.StartsWith("EnemyPrefabReview_", StringComparison.Ordinal)
                || rootName.StartsWith("EnemyRoleReview_", StringComparison.Ordinal)
                || rootName.StartsWith(StageReviewRootPrefix, StringComparison.Ordinal)
                || rootName.StartsWith("ReadableAttackTelegraph", StringComparison.Ordinal);
        }

        private static void ConfigureArenaInfluenceTargets(Scene scene, Transform player, Transform[] enemies)
        {
            ActionFoundationArenaShapeInfluenceDriver[] drivers = CollectComponents<ActionFoundationArenaShapeInfluenceDriver>(scene);
            var targets = new UnityEngine.Object[1 + enemies.Length];
            targets[0] = player;
            for (int i = 0; i < enemies.Length; i++)
            {
                targets[i + 1] = enemies[i];
            }

            for (int i = 0; i < drivers.Length; i++)
            {
                SetObjectReferenceArray(drivers[i], "influenceTargets", targets);
            }
        }

        private static StageReviewEnemySpec[] GetBreakGateRouteSpecs()
        {
            return new[]
            {
                CreateSpec("01_EntryRead_EntryProbe", ActionFoundationEnemyRoleCandidateSetup.EntryProbePrefabPath, new Vector3(0f, 0f, 1.6f)),
                CreateSpec("02_BasicPressure_CloseGuard", ActionFoundationEnemyRoleCandidateSetup.CloseGuardPrefabPath, new Vector3(-2.6f, 0f, 13f)),
                CreateSpec("02_BasicPressure_LungeChaser", ActionFoundationEnemyRoleCandidateSetup.LungeChaserPrefabPath, new Vector3(2.8f, 0f, 14f)),
                CreateSpec("03_BreakGate_CloseGuard", ActionFoundationEnemyRoleCandidateSetup.CloseGuardPrefabPath, new Vector3(-3.2f, 0f, 26f)),
                CreateSpec("03_BreakGate_ShieldBreakerElite", ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerElitePrefabPath, new Vector3(2.9f, 0f, 27f)),
                CreateSpec("05_FinalStand_CommanderElite", ActionFoundationEnemyRoleCandidateSetup.FinalStandCommanderElitePrefabPath, new Vector3(0f, 0f, 45f)),
                CreateSpec("05_FinalStand_BacklineShooter", ActionFoundationEnemyRoleCandidateSetup.BacklineShooterPrefabPath, new Vector3(-6.5f, 0f, 49f)),
                CreateSpec("05_FinalStand_FanSuppressor", ActionFoundationEnemyRoleCandidateSetup.FanSuppressorPrefabPath, new Vector3(6f, 0f, 47f)),
                CreateSpec("05_FinalStand_Skirmisher", ActionFoundationEnemyRoleCandidateSetup.SkirmisherPrefabPath, new Vector3(2.5f, 0f, 52f))
            };
        }

        private static StageReviewEnemySpec CreateSpec(string label, string prefabPath, Vector3 position)
        {
            return new StageReviewEnemySpec(StageReviewRootPrefix + label, prefabPath, position);
        }

        private static GameObject RequireRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null && string.Equals(root.name, rootName, StringComparison.Ordinal))
                {
                    return root;
                }
            }

            throw new InvalidOperationException($"Missing root {rootName} in {scene.path}.");
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

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).boolValue = value;
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

        private static void ValidateBool(UnityEngine.Object target, string propertyName, bool expected)
        {
            bool actual = RequireProperty(new SerializedObject(target), propertyName).boolValue;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
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

        private readonly struct StageReviewEnemySpec
        {
            public StageReviewEnemySpec(string rootName, string prefabPath, Vector3 position)
            {
                RootName = rootName;
                PrefabPath = prefabPath;
                Position = position;
            }

            public string RootName { get; }
            public string PrefabPath { get; }
            public Vector3 Position { get; }
        }
    }
}
