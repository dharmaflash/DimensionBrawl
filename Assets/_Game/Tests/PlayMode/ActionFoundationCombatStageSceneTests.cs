using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationCombatStageSceneTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationCombatStage.unity";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";
        private const string LaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string InoriRangedVisualName = "BossBarrageLaneReview_RangedVisual_Inori";
        private const string RetiredRifleGirlRangedVisualName = "BossBarrageLaneReview_RangedVisual_RifleGirl";

        [UnityTest]
        public IEnumerator CombatStageKeepsReticleCenteredAndFiresAssistedBasicAtNearTarget()
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(HudRootName), "mobile HUD");
            SummonLaneSpace laneSpace =
                RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "summon lane space");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Transform inoriRangedVisual = player.transform.Find(InoriRangedVisualName);
            Transform retiredRifleGirlRangedVisual = player.transform.Find(RetiredRifleGirlRangedVisualName);

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            aimController.SetAimInput(Vector2.zero);
            rangedBasicAttackAction.ClearAimInput();
            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.25f);

            PlaceBossNearCenter(player, cameraController, bossRoot);
            bossHealth.ResetHealthToFull();
            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(GetBool(mobileHud, "fireAimReticleUsesScreenCenter"));
            Assert.IsFalse(GetBool(mobileHud, "fireAimReticleFollowsAssist"));
            Assert.NotNull(inoriRangedVisual, $"The combat stage player should reuse {InoriRangedVisualName}.");
            Assert.IsTrue(inoriRangedVisual.gameObject.activeSelf);
            Assert.IsNull(
                retiredRifleGirlRangedVisual,
                $"The combat stage should not keep retired visual {RetiredRifleGirlRangedVisualName}.");
            Assert.AreSame(
                inoriRangedVisual.gameObject,
                GetObject<GameObject>(combatModeController, "rangedVisualRoot"));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "useAimAssist"));

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Vector3 targetAimPoint =
                bossHealth.transform.position + Vector3.up * GetFloat(rangedBasicAttackAction, "targetHeight");
            Vector3 expectedTargetDirection =
                (targetAimPoint - rangedBasicAttackAction.FireOrigin.position).normalized;
            float rawToTargetAngle =
                Vector3.Angle(rangedBasicAttackAction.LastRawAimDirection, expectedTargetDirection);
            Assert.IsTrue(
                rangedBasicAttackAction.HasAimAssistTarget,
                "The combat stage should acquire soft aim assist for a near-center hostile target. "
                + $"rawToTargetAngle={rawToTargetAngle:0.###}, "
                + $"candidateCount={targetSelector.TargetCandidateCount}, "
                + $"currentTarget={(targetSelector.CurrentTargetHealth != null ? targetSelector.CurrentTargetHealth.name : "none")}, "
                + $"raw={rangedBasicAttackAction.LastRawAimDirection}, "
                + $"expected={expectedTargetDirection}.");
            CombatHealth assistedTargetHealth = rangedBasicAttackAction.AimAssistTargetHealth;
            Assert.NotNull(assistedTargetHealth);
            Vector3 assistedTargetAimPoint =
                assistedTargetHealth.transform.position + Vector3.up * GetFloat(rangedBasicAttackAction, "targetHeight");
            Vector3 expectedAssistedDirection =
                (assistedTargetAimPoint - rangedBasicAttackAction.FireOrigin.position).normalized;

            Assert.Less(
                Vector3.Angle(previewDirection, expectedAssistedDirection),
                18f,
                "The preview direction should stay in the assisted target cone even when the center ray resolves a body hit point.");

            LaneActionProjectile firedProjectile = null;
            rangedBasicAttackAction.RangedProjectileFired += projectile => firedProjectile = projectile;
            Assert.IsTrue(rangedBasicAttackAction.TryFire(), rangedBasicAttackAction.LastUseBlockedReason);
            Assert.NotNull(firedProjectile, "Ranged fire should publish the projectile used by the shot.");
            Assert.Less(
                Vector3.Angle(firedProjectile.TravelDirection, previewDirection),
                0.5f,
                "The fired projectile should use the same assisted direction as the preview instead of going its own way.");
        }

        private static void PlaceBossNearCenter(
            PlayerMovementController player,
            ActionCameraController cameraController,
            GameObject bossRoot)
        {
            Vector3 aimPlanarDirection = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up);
            if (aimPlanarDirection.sqrMagnitude <= 0.0001f)
            {
                aimPlanarDirection = cameraController.GetAimPlanarForward();
            }

            aimPlanarDirection.Normalize();
            Vector3 aimRight = Vector3.Cross(Vector3.up, aimPlanarDirection).normalized;
            Vector3 bossPosition = player.transform.position + aimPlanarDirection * 7f + aimRight * 1.5f;
            bossPosition.y = bossRoot.transform.position.y;
            bossRoot.transform.SetPositionAndRotation(
                bossPosition,
                Quaternion.LookRotation(-aimPlanarDirection, Vector3.up));
        }

        private static IEnumerator WaitSeconds(float seconds)
        {
            float remaining = seconds;
            while (remaining > 0f)
            {
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        private static GameObject RequireRoot(string objectName)
        {
            GameObject root = GameObject.Find(objectName);
            Assert.NotNull(root, $"Missing scene object {objectName}.");
            return root;
        }

        private static T RequireComponent<T>(GameObject gameObject, string label) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            Assert.NotNull(component, $"{gameObject.name} is missing {label}.");
            return component;
        }

        private static T RequireObject<T>() where T : Component
        {
            T[] found = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            Assert.AreEqual(1, found.Length, $"Expected exactly one {typeof(T).Name} in the combat stage scene.");
            return found[0];
        }

        private static float GetFloat(UnityEngine.Object target, string fieldName)
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            return (float)field.GetValue(target);
        }

        private static bool GetBool(UnityEngine.Object target, string fieldName)
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            return (bool)field.GetValue(target);
        }

        private static T GetObject<T>(UnityEngine.Object target, string fieldName) where T : UnityEngine.Object
        {
            FieldInfo field = RequireField(target.GetType(), fieldName);
            return (T)field.GetValue(target);
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{type.Name} is missing private field {fieldName}.");
            return field;
        }
    }
}
