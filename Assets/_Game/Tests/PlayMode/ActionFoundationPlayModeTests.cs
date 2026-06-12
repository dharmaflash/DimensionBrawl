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
            Animator playerAnimator = RequireObject<Animator>();

            Vector3 startPosition = movement.transform.position;
            movement.SetMoveInput(Vector2.up);
            yield return new WaitForSeconds(0.35f);
            movement.SetMoveInput(Vector2.zero);
            yield return null;

            Assert.Greater(
                Vector3.Distance(startPosition, movement.transform.position),
                0.25f,
                "Player should move from shared Move input.");
            Assert.IsTrue(movement.IsStopSettling, "Releasing movement should briefly request stop-settle instead of snapping straight to idle.");
            yield return new WaitForSeconds(0.02f);
            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "StopStep"),
                "Movement release should route the CombatGirl Animator through the promoted StopStep state within the fast transition window.");

            movement.SetMoveInput(Vector2.left);
            yield return new WaitForSeconds(0.05f);
            Vector3 preDodgePosition = movement.transform.position;
            actions.QueueDodge();
            yield return new WaitForSeconds(0.1f);
            movement.SetMoveInput(Vector2.zero);

            Assert.IsTrue(actions.IsDodging, "Player should enter dodge state after QueueDodge.");
            Assert.IsTrue(
                AnimatorIsInAnyDirectionalDodge(playerAnimator),
                "Dodge should route through a directional Quickshift state instead of the old single generic dodge trigger.");
            yield return new WaitForSeconds(0.85f);
            Assert.Greater(
                Vector3.Distance(preDodgePosition, movement.transform.position),
                0.25f,
                "Dodge should apply a visible planar movement burst.");

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            float enemyStartHealth = enemyHealth.CurrentHealth;
            actions.QueueBasicAttack();
            yield return new WaitForSeconds(0.4f);

            Assert.Less(enemyHealth.CurrentHealth, enemyStartHealth, "Basic attack should damage the soldier.");
        }

        [UnityTest]
        public IEnumerator NoInputDodgeMovesBackwardFromFacing()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            Animator playerAnimator = RequireObject<Animator>();

            movement.SetMoveInput(Vector2.zero);
            movement.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Physics.SyncTransforms();
            yield return null;

            Vector3 startPosition = movement.transform.position;
            Vector3 facingDirection = movement.FacingDirection.normalized;

            actions.QueueDodge();
            yield return new WaitForSeconds(0.05f);

            Assert.IsTrue(actions.IsDodging, "No-input dodge should still start.");
            Assert.Less(
                Vector3.Dot(actions.LastDodgeDirection.normalized, facingDirection),
                -0.95f,
                "No-input dodge should resolve backward from the current facing direction.");
            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "DodgeBack"),
                "No-input dodge should route to the backward Quickshift state.");

            yield return new WaitForSeconds(0.2f);

            Assert.Less(
                Vector3.Dot(movement.transform.position - startPosition, facingDirection),
                -0.1f,
                "No-input dodge should move behind the current facing direction.");
        }

        [UnityTest]
        public IEnumerator PlayerRunStartAndSharpTurnUsePromotedLocomotionClips()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            RequireObject<ActionCameraCueDriver>();
            Animator playerAnimator = RequireObject<Animator>();

            movement.SetMoveInput(Vector2.up);
            yield return new WaitForSeconds(0.08f);

            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "StartRun"),
                "Movement start should route through the promoted StartRun clip before settling into Run.");
            Assert.IsTrue(cameraController.HasActiveCue, "Run start should request a short camera cue.");

            yield return new WaitForSeconds(0.75f);

            movement.SetMoveInput(Vector2.right);
            yield return new WaitForSeconds(0.10f);

            Assert.IsTrue(
                AnimatorIsInOrTransitioningTo(playerAnimator, "TurnLeft90")
                    || AnimatorIsInOrTransitioningTo(playerAnimator, "TurnRight90"),
                "A large movement-direction change while running should request a promoted 90-degree turn clip.");
            Assert.IsTrue(cameraController.HasActiveCue, "Sharp movement turn should request a short camera cue.");

            movement.SetMoveInput(Vector2.zero);
        }

        [UnityTest]
        public IEnumerator CombatGirlWeaponSocketsStayPinnedToHands()
        {
            CombatGirlWeaponSocketBinder weaponBinder = RequireObject<CombatGirlWeaponSocketBinder>();

            yield return null;

            Assert.AreEqual(2, weaponBinder.BindingCount, "CombatGirl weapon binder should keep one socket per hand.");
            Assert.IsTrue(weaponBinder.AllBindingsValid, "CombatGirl weapon binder should reference both hands and both weapon sockets.");

            weaponBinder.ApplyBindings();

            Assert.IsTrue(
                weaponBinder.AreBindingsAligned(0.01f, 1f),
                "Weapon sockets should stay aligned to captured hand offsets even when generic humanoid stop clips do not animate add_weapon bones.");
        }

        [UnityTest]
        public IEnumerator PlayerCanReachFiveHitBasicComboState()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            CombatHealth enemyHealth = RequireEnemyHealth();
            Animator playerAnimator = RequireObject<Animator>();

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            float enemyStartHealth = enemyHealth.CurrentHealth;
            bool reachedFifthHit = false;
            float timeout = 3f;
            float nextInputSeconds = 0.16f;

            actions.QueueBasicAttack();
            while (timeout > 0f)
            {
                yield return null;

                nextInputSeconds -= Time.deltaTime;
                if (nextInputSeconds <= 0f)
                {
                    actions.QueueBasicAttack();
                    nextInputSeconds = 0.18f;
                }

                reachedFifthHit = AnimatorIsInOrTransitioningTo(playerAnimator, "Attack5");
                if (reachedFifthHit)
                {
                    break;
                }

                timeout -= Time.deltaTime;
            }

            Assert.IsTrue(reachedFifthHit, "Queued basic attack input should be able to reach the promoted Attack5 finisher state.");
            Assert.Less(enemyHealth.CurrentHealth, enemyStartHealth, "The five-hit combo route should still apply damage through the combat validation path.");
        }

        [UnityTest]
        public IEnumerator BasicAttackHitDoesNotTriggerGlobalSlowMotion()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            CombatHealth enemyHealth = RequireEnemyHealth();

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            float enemyStartHealth = enemyHealth.CurrentHealth;
            actions.QueueBasicAttack();

            float timeout = 0.8f;
            while (timeout > 0f && Mathf.Approximately(enemyHealth.CurrentHealth, enemyStartHealth))
            {
                Assert.AreEqual(1f, Time.timeScale, 0.001f, "Normal attack feedback should not drive global slow motion.");
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.Less(enemyHealth.CurrentHealth, enemyStartHealth, "Basic attack should damage the soldier during the slow-motion guard test.");
            Assert.AreEqual(1f, Time.timeScale, 0.001f, "Successful normal attack damage should leave global time scale unchanged.");
        }

        [UnityTest]
        public IEnumerator DodgeAppliesAndClearsVisibleFeedbackTint()
        {
            PlayerActionController actions = RequireObject<PlayerActionController>();
            PlayerDodgeFeedback dodgeFeedback = RequireObject<PlayerDodgeFeedback>();
            Renderer[] targetRenderers = RequirePlayerFeedbackRenderers(dodgeFeedback.transform);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

            actions.QueueDodge();
            yield return null;

            Assert.IsTrue(
                AnyRendererHasColor(targetRenderers, propertyBlock, new Color(0.75f, 1f, 1f)),
                "Dodge feedback should tint the player renderer during the dodge movement window.");

            yield return new WaitForSeconds(0.7f);

            Assert.IsFalse(
                AnyRendererHasColor(targetRenderers, propertyBlock, new Color(0.75f, 1f, 1f)),
                "Dodge feedback should clear before or during recovery instead of sticking forever.");
        }

        [UnityTest]
        public IEnumerator ActionCameraCueDriverRequestsAndClearsShortCues()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            PlayerActionController actions = RequireObject<PlayerActionController>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            RequireObject<ActionCameraCueDriver>();
            CombatHealth enemyHealth = RequireEnemyHealth();
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Action foundation scene should have a main camera.");
            float baseFieldOfView = mainCamera.fieldOfView;

            actions.QueueDodge();
            yield return null;

            Assert.IsTrue(cameraController.HasActiveCue, "Dodge should request a short action camera cue.");
            yield return new WaitForSeconds(0.05f);
            Assert.Greater(mainCamera.fieldOfView, baseFieldOfView + 0.2f, "Dodge camera cue should briefly widen FOV for speed readability.");

            yield return new WaitForSeconds(0.35f);

            Assert.IsFalse(cameraController.HasActiveCue, "Dodge camera cue should clear instead of sticking.");

            yield return new WaitForSeconds(0.55f);

            PositionPlayerForAttack(movement.transform, enemyHealth.transform);
            Physics.SyncTransforms();
            yield return null;

            actions.QueueBasicAttack();
            yield return null;

            Assert.IsTrue(cameraController.HasActiveCue, "Basic attack should request a short action camera cue.");
        }

        [UnityTest]
        public IEnumerator ActionCameraOrbitStaysIndependentFromPlayerFacing()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();

            yield return null;

            float initialYaw = cameraController.OrbitYawDegrees;
            movement.transform.rotation = Quaternion.Euler(0f, 120f, 0f);
            yield return null;

            Assert.Less(
                Mathf.Abs(Mathf.DeltaAngle(initialYaw, cameraController.OrbitYawDegrees)),
                10f,
                "Player facing should not instantly rotate the base camera orbit.");

            cameraController.SetOrbitInput(Vector2.right);
            yield return new WaitForSeconds(0.12f);
            cameraController.SetOrbitInput(Vector2.zero);

            Assert.Greater(
                Mathf.Abs(Mathf.DeltaAngle(initialYaw, cameraController.OrbitYawDegrees)),
                5f,
                "Manual camera orbit input should rotate the camera independently from player movement.");
        }

        [UnityTest]
        public IEnumerator ActionCameraUsesCloseLowRearCombatPreset()
        {
            PlayerMovementController movement = RequireObject<PlayerMovementController>();
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Action foundation scene should have a main camera.");

            yield return new WaitForSeconds(0.35f);

            Vector3 relative = mainCamera.transform.position - movement.transform.position;
            float planarDistance = Vector3.ProjectOnPlane(relative, Vector3.up).magnitude;

            Assert.Less(relative.y, 2.8f, "Camera should sit lower than the earlier top-down inspection preset.");
            Assert.Greater(relative.y, 1.5f, "Camera should remain above shoulder/torso height for target readability.");
            Assert.Less(planarDistance, 4.2f, "Camera should stay closer to the character's rear than the old distant overview preset.");
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

        private static bool AnyRendererHasColor(
            IReadOnlyList<Renderer> renderers,
            MaterialPropertyBlock propertyBlock,
            Color expectedColor)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                Renderer targetRenderer = renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                if (ApproximatelyColor(expectedColor, propertyBlock.GetColor("_BaseColor")))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnimatorIsInOrTransitioningTo(Animator animator, string stateName)
        {
            if (animator == null)
            {
                return false;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            {
                return true;
            }

            return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(stateName);
        }

        private static bool AnimatorIsInAnyDirectionalDodge(Animator animator)
        {
            return AnimatorIsInOrTransitioningTo(animator, "DodgeForward")
                || AnimatorIsInOrTransitioningTo(animator, "DodgeBack")
                || AnimatorIsInOrTransitioningTo(animator, "DodgeLeft")
                || AnimatorIsInOrTransitioningTo(animator, "DodgeRight");
        }

        private static Renderer[] RequirePlayerFeedbackRenderers(Transform playerRoot)
        {
            Renderer[] renderers = playerRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Assert.Fail("Player dodge feedback should have at least one child renderer to tint.");
            }

            return renderers;
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
