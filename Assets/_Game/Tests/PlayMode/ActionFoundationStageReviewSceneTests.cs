using System.Collections;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationStageReviewSceneTests
    {
        private const string StageBreakGateReviewScenePath =
            "Assets/_Game/Scenes/ActionFoundationStageBreakGateReview.unity";

        private static readonly string[] StageBreakGateRootNames =
        {
            "StageBreakGateReview_01_EntryRead_EntryProbe",
            "StageBreakGateReview_02_BasicPressure_CloseGuard",
            "StageBreakGateReview_02_BasicPressure_LungeChaser",
            "StageBreakGateReview_03_BreakGate_CloseGuard",
            "StageBreakGateReview_03_BreakGate_ShieldBreakerElite",
            "StageBreakGateReview_05_FinalStand_CommanderElite",
            "StageBreakGateReview_05_FinalStand_BacklineShooter",
            "StageBreakGateReview_05_FinalStand_FanSuppressor",
            "StageBreakGateReview_05_FinalStand_Skirmisher"
        };

        [UnityTest]
        public IEnumerator StageBreakGateReviewSceneWiresRouteEnemiesForManualReview()
        {
            EditorSceneManager.LoadSceneInPlayMode(StageBreakGateReviewScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = player.GetComponent<CombatHealth>();
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>();
            StageEncounterReviewOwner encounterOwner = RequireObject<StageEncounterReviewOwner>();
            SerializedProperty targetCandidates = new SerializedObject(targetSelector).FindProperty("targetCandidates");

            Assert.IsNotNull(playerHealth, "Stage review scene should keep player health on the player root.");
            Assert.AreEqual(
                StageBreakGateRootNames.Length,
                targetSelector.TargetCandidateCount,
                "Stage review scene should give the player every authored S1-1 route enemy as a candidate.");
            Assert.AreEqual(StageBreakGateRootNames.Length, targetCandidates.arraySize);

            for (int i = 0; i < StageBreakGateRootNames.Length; i++)
            {
                BasicSoldierEnemy soldier = RequireNamedRootComponent<BasicSoldierEnemy>(StageBreakGateRootNames[i]);
                CombatHealth enemyHealth = soldier.SelfHealth;
                EnemyActionCameraCueDriver cameraCueDriver = soldier.GetComponent<EnemyActionCameraCueDriver>();

                Assert.IsNotNull(enemyHealth, $"{StageBreakGateRootNames[i]} should expose local health.");
                Assert.AreEqual(1, soldier.TargetSensor.TargetCandidateCount, $"{StageBreakGateRootNames[i]} should receive the player as its only scene target candidate.");
                Assert.AreSame(
                    playerHealth,
                    new SerializedObject(soldier.TargetSensor).FindProperty("targetCandidates").GetArrayElementAtIndex(0).objectReferenceValue,
                    $"{StageBreakGateRootNames[i]} target sensor should serialize the player health candidate.");
                Assert.AreSame(
                    enemyHealth,
                    targetCandidates.GetArrayElementAtIndex(i).objectReferenceValue,
                    $"{StageBreakGateRootNames[i]} should be present in the player target selector at route order {i}.");
                Assert.IsNotNull(cameraCueDriver, $"{StageBreakGateRootNames[i]} should keep its enemy camera cue driver.");
                Assert.AreSame(cameraController, cameraCueDriver.CameraController, $"{StageBreakGateRootNames[i]} should receive the scene camera controller.");
            }

            BasicSoldierEnemy firstEnemy = RequireNamedRootComponent<BasicSoldierEnemy>(StageBreakGateRootNames[0]);
            BasicSoldierEnemy finalEnemy = RequireNamedRootComponent<BasicSoldierEnemy>(StageBreakGateRootNames[StageBreakGateRootNames.Length - 1]);
            Assert.IsTrue(
                firstEnemy.TargetSensor.TryGetCurrentTarget(out Transform firstTarget, out CombatHealth firstTargetHealth),
                "The first EntryRead enemy should start close enough to resolve the player target.");
            Assert.AreSame(playerHealth, firstTargetHealth);
            Assert.AreSame(playerHealth.transform, firstTarget);
            Assert.AreSame(player.transform, cameraController.Target, "Stage review camera should follow the player.");
            Assert.AreSame(firstEnemy.transform, cameraController.Threat, "Stage review camera should bias toward the first route enemy.");
            Assert.AreSame(
                finalEnemy.SelfHealth,
                new SerializedObject(encounter).FindProperty("enemyHealth").objectReferenceValue,
                "The temporary test encounter should point at the final route enemy, not the first pocket.");
            Assert.IsFalse(
                new SerializedObject(cameraController).FindProperty("useDeviceFallbackWhenActionMissing").boolValue,
                "Stage review camera should not auto-orbit from fallback device input while idle.");

            Assert.AreSame(player.transform, encounterOwner.Player);
            Assert.IsNotNull(encounterOwner.StageTemplate, "S1-1 review owner should reference the authored stage template.");
            Assert.AreEqual(5, encounterOwner.PocketCount, "S1-1 review owner should track the five authored route pockets.");
            AssertStagePocket(encounterOwner.StageTemplate, encounterOwner.GetPocketBinding(0), LinearStageObjectiveKind.ReadThreat, 1);
            AssertStagePocket(encounterOwner.StageTemplate, encounterOwner.GetPocketBinding(1), LinearStageObjectiveKind.PunishRecovery, 2);
            AssertStagePocket(encounterOwner.StageTemplate, encounterOwner.GetPocketBinding(2), LinearStageObjectiveKind.BreakGuard, 2);
            AssertStagePocket(encounterOwner.StageTemplate, encounterOwner.GetPocketBinding(3), LinearStageObjectiveKind.RecoverPosition, 0);
            AssertStagePocket(encounterOwner.StageTemplate, encounterOwner.GetPocketBinding(4), LinearStageObjectiveKind.FinalClear, 4);

            encounterOwner.ResetProgress();
            player.transform.position = encounterOwner.GetPocketBinding(0).EnterCenter.position;
            encounterOwner.RefreshProgress();
            Assert.AreEqual(0, encounterOwner.CurrentPocketIndex);
            Assert.AreEqual(LinearStageObjectiveKind.ReadThreat, encounterOwner.CurrentObjectiveKind);
            Assert.AreEqual(1, encounterOwner.RemainingEnemyCount);

            firstEnemy.SelfHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                firstEnemy.SelfHealth.MaxHealth,
                firstEnemy.transform.position,
                Vector3.forward,
                0f));
            yield return null;

            encounterOwner.RefreshProgress();
            Assert.AreEqual(1, encounterOwner.CompletedPocketCount);
            player.transform.position = encounterOwner.GetPocketBinding(1).EnterCenter.position;
            encounterOwner.RefreshProgress();
            Assert.AreEqual(1, encounterOwner.CurrentPocketIndex);
            Assert.AreEqual(LinearStageObjectiveKind.PunishRecovery, encounterOwner.CurrentObjectiveKind);
            Assert.AreEqual(2, encounterOwner.RemainingEnemyCount);
        }

        private static void AssertStagePocket(
            LinearStageTemplateProfile template,
            StageEncounterPocketBinding binding,
            LinearStageObjectiveKind expectedObjective,
            int expectedEnemyCount)
        {
            Assert.IsNotNull(binding.EnterCenter, $"{binding.Label} should have an authored entry anchor.");
            Assert.AreEqual(expectedEnemyCount, binding.EnemyCount, $"{binding.Label} should bind the authored review enemies.");
            Assert.IsTrue(binding.TryResolvePocket(template, out _, out LinearStagePocket pocket));
            Assert.AreEqual(expectedObjective, pocket.ObjectiveKind, $"{binding.Label} should mirror the S1-1 objective data.");
        }

        private static T RequireObject<T>() where T : Component
        {
            T found = Object.FindFirstObjectByType<T>();
            Assert.IsNotNull(found, $"Missing {typeof(T).Name} in loaded scene.");
            return found;
        }

        private static T RequireNamedRootComponent<T>(string rootName) where T : Component
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (!string.Equals(roots[i].name, rootName, System.StringComparison.Ordinal))
                {
                    continue;
                }

                T component = roots[i].GetComponent<T>();
                Assert.IsNotNull(component, $"{rootName} should expose {typeof(T).Name} on its root.");
                return component;
            }

            Assert.Fail($"Missing root {rootName} in {scene.path}.");
            return null;
        }
    }
}
