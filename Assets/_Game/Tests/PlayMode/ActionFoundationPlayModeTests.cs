using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationPlayModeTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";

        [UnitySetUp]
        public IEnumerator LoadActionFoundationScene()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerCanMoveDodgeAndDamageSoldier()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            CombatHealth enemyHealth = RequireEnemyHealth();

            Vector3 startPosition = movement.transform.position;
            movement.SetMoveInput(Vector2.up);
            yield return new WaitForSeconds(0.35f);
            movement.SetMoveInput(Vector2.zero);

            Assert.Greater(
                Vector3.Distance(startPosition, movement.transform.position),
                0.25f,
                "Player should move from shared Move input.");

            Vector3 preDodgePosition = movement.transform.position;
            actions.QueueDodge();
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(actions.IsDodging, "Player should enter dodge state after QueueDodge.");
            yield return new WaitForSeconds(0.7f);
            Assert.Greater(
                Vector3.Distance(preDodgePosition, movement.transform.position),
                0.25f,
                "Dodge should apply a visible planar movement burst.");

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            float enemyStartHealth = enemyHealth.CurrentHealth;
            actions.QueueBasicAttack();
            yield return new WaitForSeconds(0.25f);

            Assert.Less(enemyHealth.CurrentHealth, enemyStartHealth, "Basic attack should damage the soldier.");
        }

        [UnityTest]
        public IEnumerator DodgeAppliesAndClearsVisibleFeedbackTint()
        {
            PlayerActionController actions = RequireObject<PlayerActionController>();
            PlayerDodgeFeedback dodgeFeedback = RequireObject<PlayerDodgeFeedback>();
            Renderer targetRenderer = RequirePlayerFeedbackRenderer(dodgeFeedback.transform);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

            actions.QueueDodge();
            yield return null;

            targetRenderer.GetPropertyBlock(propertyBlock);
            Assert.IsTrue(
                ApproximatelyColor(new Color(0.75f, 1f, 1f), propertyBlock.GetColor("_BaseColor")),
                "Dodge feedback should tint the player renderer during the dodge movement window.");

            yield return new WaitForSeconds(0.7f);

            targetRenderer.GetPropertyBlock(propertyBlock);
            Assert.IsFalse(
                ApproximatelyColor(new Color(0.75f, 1f, 1f), propertyBlock.GetColor("_BaseColor")),
                "Dodge feedback should clear before or during recovery instead of sticking forever.");
        }

        [UnityTest]
        public IEnumerator EncounterCanWinAndFail()
        {
            CombatHealth playerHealth = RequirePlayerHealth();
            CombatHealth enemyHealth = RequireEnemyHealth();
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>();

            enemyHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                enemyHealth.MaxHealth,
                enemyHealth.transform.position,
                Vector3.forward,
                0f));
            yield return null;

            Assert.IsTrue(encounter.IsWon, "Encounter should win when the soldier dies.");

            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            playerHealth = RequirePlayerHealth();
            enemyHealth = RequireEnemyHealth();
            encounter = RequireObject<ActionFoundationTestEncounter>();

            playerHealth.TryApplyDamage(new DamageInfo(
                enemyHealth,
                DamageTeam.Enemy,
                playerHealth.MaxHealth,
                playerHealth.transform.position,
                Vector3.back,
                0f));
            yield return null;

            Assert.IsTrue(encounter.IsFailed, "Encounter should fail when the player dies.");
        }

        private static bool ApproximatelyColor(Color expected, Color actual)
        {
            return Mathf.Abs(expected.r - actual.r) <= 0.01f
                && Mathf.Abs(expected.g - actual.g) <= 0.01f
                && Mathf.Abs(expected.b - actual.b) <= 0.01f
                && Mathf.Abs(expected.a - actual.a) <= 0.01f;
        }

        private static Renderer RequirePlayerFeedbackRenderer(Transform playerRoot)
        {
            Renderer[] renderers = playerRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Assert.Fail("Player dodge feedback should have at least one child renderer to tint.");
            }

            return renderers[0];
        }

        private static void PositionPlayerForAttack(Transform player, Transform enemy)
        {
            player.position = enemy.position - Vector3.forward * 1.25f;
            player.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        private static CombatHealth RequirePlayerHealth()
        {
            return RequireHealth(DamageTeam.Player);
        }

        private static CombatHealth RequireEnemyHealth()
        {
            return RequireHealth(DamageTeam.Enemy);
        }

        private static CombatHealth RequireHealth(DamageTeam team)
        {
            List<CombatHealth> healths = CollectObjects<CombatHealth>();
            for (int i = 0; i < healths.Count; i++)
            {
                if (healths[i].Team == team)
                {
                    return healths[i];
                }
            }

            Assert.Fail($"Missing CombatHealth for team {team}.");
            return null;
        }

        private static T RequireObject<T>() where T : Component
        {
            List<T> found = CollectObjects<T>();
            if (found.Count == 0)
            {
                Assert.Fail($"Missing required component {typeof(T).Name}.");
            }

            return found[0];
        }

        private static List<T> CollectObjects<T>() where T : Component
        {
            List<T> results = new List<T>();
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                results.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }

            return results;
        }
    }
}
