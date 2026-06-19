using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationBossSummonDuelReviewSceneTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationBossSummonDuelReview.unity";

        [UnitySetUp]
        public IEnumerator LoadBossSummonDuelReviewScene()
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
        public IEnumerator DuelReviewSceneRequiresBossSummonAndPlayerSupportExchange()
        {
            yield return null;

            BossSummonDuelReviewOwner duelOwner = RequireObject<BossSummonDuelReviewOwner>();
            PlayerSkill1Action skill1Action = RequireObject<PlayerSkill1Action>();
            PlayerSummonSlot1Action summonSlot1Action = RequireObject<PlayerSummonSlot1Action>();
            PlayerSupportSummonSlotAction summonSlot2Action = RequireSupportSummonAction("SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action = RequireSupportSummonAction("SummonSlot3");
            BossPressureActionDirector bossPressureActionDirector = RequireObject<BossPressureActionDirector>();
            BossSummonPressureAction bossSummonPressureAction = RequireObject<BossSummonPressureAction>();
            BossBarrageEmitter bossBarrageEmitter = RequireObject<BossBarrageEmitter>();
            BossPressureCostLadder bossPressureCostLadder = RequireObject<BossPressureCostLadder>();
            SummonEnergyLadder energyLadder = RequireObject<SummonEnergyLadder>();
            SummonLaneSpace laneSpace = RequireObject<SummonLaneSpace>();
            BossBarrageLaneReviewHud reviewHud = RequireObject<BossBarrageLaneReviewHud>();
            BossBarrageLaneReviewMobileHud mobileHud = RequireObject<BossBarrageLaneReviewMobileHud>();

            Assert.AreEqual(2, duelOwner.RequiredBossPressureActions);
            Assert.AreEqual(1, duelOwner.RequiredBossSkillPatterns);
            Assert.AreEqual(1, duelOwner.RequiredBossSummonPressureActions);
            Assert.AreEqual(0, duelOwner.RequiredBossPunishPatterns);
            Assert.AreEqual(1, duelOwner.RequiredBossSummonReleases);
            Assert.AreEqual(1, duelOwner.RequiredBossPressureBlocks);
            Assert.AreEqual(2, duelOwner.RequiredPlayerSummonUses);
            Assert.AreEqual(1, duelOwner.RequiredSupportSummonUses);
            Assert.AreEqual(1, duelOwner.RequiredAllyPressureBlocks);
            Assert.AreEqual(1, duelOwner.RequiredSummonClashes);
            Assert.AreEqual(1, duelOwner.RequiredSkill1ResponseUses);
            Assert.Greater(duelOwner.RequiredSkill1ResponseDamage, 0f);
            Assert.Greater(duelOwner.RequiredBossDamage, duelOwner.RequiredSkill1ResponseDamage);
            StringAssert.Contains("bossSP", duelOwner.ProgressLine);
            StringAssert.Contains("support", duelOwner.ProgressLine);
            StringAssert.Contains("clash", duelOwner.ProgressLine);

            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(duelOwner, "energyLadder"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(duelOwner, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(duelOwner, "summonSlot1Action"));
            Assert.AreSame(summonSlot2Action, GetObjectReference<PlayerSupportSummonSlotAction>(duelOwner, "summonSlot2Action"));
            Assert.AreSame(summonSlot3Action, GetObjectReference<PlayerSupportSummonSlotAction>(duelOwner, "summonSlot3Action"));
            Assert.AreSame(bossBarrageEmitter, GetObjectReference<BossBarrageEmitter>(duelOwner, "bossBarrageEmitter"));
            Assert.AreSame(
                bossPressureCostLadder,
                GetObjectReference<BossPressureCostLadder>(duelOwner, "bossPressureCostLadder"));
            Assert.AreSame(
                bossPressureActionDirector,
                GetObjectReference<BossPressureActionDirector>(duelOwner, "bossPressureActionDirector"));
            Assert.AreSame(
                bossSummonPressureAction,
                GetObjectReference<BossSummonPressureAction>(duelOwner, "bossSummonPressureAction"));

            Assert.IsTrue(
                bossPressureActionDirector.HoldForNextTierActionWhenGateAllows,
                "Duel review should let the boss bank LV1 pressure into a visible LV2 summon-pressure exchange.");
            Assert.AreEqual(3, bossPressureActionDirector.ActionSlotCount);
            AssertBossPressureSlot(
                bossPressureActionDirector,
                0,
                BossPressureActionKind.SkillPattern,
                1,
                "DodgeLineOrUseSkill1");
            AssertBossPressureSlot(
                bossPressureActionDirector,
                1,
                BossPressureActionKind.SummonPressure,
                2,
                "SummonSlot1PressureBlock");
            AssertBossPressureSlot(
                bossPressureActionDirector,
                2,
                BossPressureActionKind.PunishOverextend,
                3,
                "RetreatOrSpendHighTierAnswer");

            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(bossPressureCostLadder, "laneSpace"));
            Assert.IsTrue(bossSummonPressureAction.HasPressureProfile);
            Assert.AreEqual(3, bossSummonPressureAction.PressureProfile.TierCount);
            Assert.IsTrue(bossSummonPressureAction.CanRelease);
            AssertBossProxyBodyContract(bossSummonPressureAction.gameObject);

            AssertSummonActorPrefabContract(
                GetObjectReference<GameObject>(summonSlot1Action, "summonActorPrefabObject"),
                DamageTeam.AllySummon,
                "SummonSlot1 actor prefab",
                expectPressureScreen: true);
            AssertSupportSummonAction(summonSlot2Action, "SummonSlot2");
            AssertSupportSummonAction(summonSlot3Action, "SummonSlot3");
            GameObject summonSlot2ActorPrefab =
                GetObjectReference<GameObject>(summonSlot2Action, "summonActorPrefabObject");
            GameObject summonSlot3ActorPrefab =
                GetObjectReference<GameObject>(summonSlot3Action, "summonActorPrefabObject");
            AssertSummonActorPrefabContract(
                summonSlot2ActorPrefab,
                DamageTeam.AllySummon,
                "SummonSlot2 actor prefab",
                expectPressureScreen: false);
            AssertSummonActorPrefabContract(
                summonSlot3ActorPrefab,
                DamageTeam.AllySummon,
                "SummonSlot3 actor prefab",
                expectPressureScreen: true);
            Assert.Less(
                summonSlot2ActorPrefab.GetComponent<CombatHealth>().MaxHealth,
                summonSlot3ActorPrefab.GetComponent<CombatHealth>().MaxHealth,
                "S2 should stay the fragile ranged-support body while S3 owns the tankier frontline role.");
            AssertSummonActorPrefabContract(
                GetObjectReference<GameObject>(bossSummonPressureAction, "summonActorPrefabObject"),
                DamageTeam.Enemy,
                "Boss summon pressure actor prefab",
                expectPressureScreen: true);

            Assert.AreSame(duelOwner, GetObjectReference<BossSummonDuelReviewOwner>(reviewHud, "duelReviewOwner"));
            Assert.AreSame(summonSlot2Action, GetObjectReference<PlayerSupportSummonSlotAction>(reviewHud, "summonSlot2Action"));
            Assert.AreSame(summonSlot3Action, GetObjectReference<PlayerSupportSummonSlotAction>(reviewHud, "summonSlot3Action"));
            Assert.AreSame(summonSlot2Action, GetObjectReference<PlayerSupportSummonSlotAction>(mobileHud, "summonSlot2Action"));
            Assert.AreSame(summonSlot3Action, GetObjectReference<PlayerSupportSummonSlotAction>(mobileHud, "summonSlot3Action"));
            Assert.AreEqual("SummonSlot2", mobileHud.SummonSlot2ActionName);
            Assert.AreEqual("SummonSlot3", mobileHud.SummonSlot3ActionName);
        }

        [UnityTest]
        public IEnumerator DuelReviewSupportSummonsSpendEnergyAndCreateVisibleActors()
        {
            yield return null;

            SummonEnergyLadder energyLadder = RequireObject<SummonEnergyLadder>();
            PlayerSupportSummonSlotAction summonSlot2Action = RequireSupportSummonAction("SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action = RequireSupportSummonAction("SummonSlot3");
            BossSummonPressureAction bossSummonPressureAction = RequireObject<BossSummonPressureAction>();

            energyLadder.SetGainEnabled(false);
            GrantEnergyToTier(energyLadder, 1);
            Assert.IsTrue(summonSlot2Action.TryUseSummon());
            Assert.AreEqual(1, summonSlot2Action.LastSpentTier);
            Assert.AreEqual(1, summonSlot2Action.TotalUseCount);
            Assert.Greater(summonSlot2Action.ActiveSummonActorCount, 0);
            Assert.IsTrue(
                summonSlot2Action.LastSummonActorHasHealth,
                "S2 should create a summon actor with body health, not only a temporary visual marker.");
            Assert.Greater(summonSlot2Action.LastSummonActorRemainingLifetimeSeconds, 0f);
            Assert.IsTrue(
                float.IsPositiveInfinity(summonSlot2Action.LastSummonActorRemainingLifetimeSeconds),
                "S2 is a normal body-bearing summon and should persist until defeated or recalled, not expire as a short effect.");
            Assert.AreEqual(SummonFrontlineProxyExitReason.None, summonSlot2Action.LastSummonActorExitReason);
            yield return new WaitForSeconds(0.15f);
            Assert.Greater(
                summonSlot2Action.ActiveProjectileCount,
                0,
                "S2 should read as a ranged support summon by firing visible lane projectiles after entry.");

            GrantEnergyToTier(energyLadder, 3);
            Assert.IsTrue(summonSlot3Action.TryUseSummon());
            Assert.AreEqual(3, summonSlot3Action.LastSpentTier);
            Assert.AreEqual(1, summonSlot3Action.TotalUseCount);
            Assert.Greater(summonSlot3Action.ActiveSummonActorCount, 0);
            Assert.IsTrue(
                summonSlot3Action.LastSummonActorHasHealth,
                "S3 should create a summon actor with body health, not only a temporary visual marker.");
            Assert.Greater(summonSlot3Action.LastSummonActorRemainingLifetimeSeconds, 0f);
            Assert.IsTrue(
                float.IsPositiveInfinity(summonSlot3Action.LastSummonActorRemainingLifetimeSeconds),
                "S3 is a normal body-bearing summon and should persist until defeated or recalled, not expire as a short effect.");
            Assert.AreEqual(SummonFrontlineProxyExitReason.None, summonSlot3Action.LastSummonActorExitReason);
            yield return new WaitForSeconds(0.15f);
            Assert.Greater(
                summonSlot3Action.ActiveProjectileCount,
                0,
                "S3 should read as a vanguard support summon with a visible projectile response, not only a button state.");

            Assert.IsTrue(bossSummonPressureAction.TryReleasePressureSummon(2));
            Assert.Greater(bossSummonPressureAction.ActiveSummonActorCount, 0);
            Assert.IsTrue(
                bossSummonPressureAction.LastSummonActorHasHealth,
                "Boss summon pressure should also release a damageable frontline actor for summon-vs-summon exchange.");
            Assert.Greater(bossSummonPressureAction.LastSummonActorRemainingLifetimeSeconds, 0f);
            Assert.IsTrue(
                float.IsPositiveInfinity(bossSummonPressureAction.LastSummonActorRemainingLifetimeSeconds),
                "Boss summon pressure should create a persistent opposing actor until the player answers it.");
            Assert.AreEqual(SummonFrontlineProxyExitReason.None, bossSummonPressureAction.LastSummonActorExitReason);
        }

        private static void AssertBossPressureSlot(
            BossPressureActionDirector director,
            int index,
            BossPressureActionKind expectedKind,
            int expectedTier,
            string expectedResponseId)
        {
            Assert.IsTrue(director.TryGetActionSlot(index, out BossPressureActionDirector.BossPressureActionSlot slot));
            Assert.IsNotNull(slot.Pattern);
            Assert.AreEqual(expectedKind, slot.ActionKind);
            Assert.AreEqual(expectedTier, slot.MinimumTier);
            Assert.AreEqual(expectedResponseId, slot.ResponseId);
            Assert.IsTrue(slot.HasResponsePlan);
        }

        private static void AssertSupportSummonAction(
            PlayerSupportSummonSlotAction action,
            string expectedActionName)
        {
            Assert.AreEqual(expectedActionName, action.SlotActionName);
            Assert.IsTrue(action.HasRequiredPresentation);
            Assert.AreEqual(
                1.35f,
                GetFloat(action, "entryForwardOffset"),
                0.001f,
                $"{expectedActionName} should enter in front of the player body before crossing into summon space.");
            Assert.GreaterOrEqual(
                GetFloat(action, "actorEntryCatchupSecondsPerMeter"),
                0.3f,
                $"{expectedActionName} should march across the corridor instead of snapping from entry to target.");
            Assert.AreEqual(
                1,
                GetInt(action, "maxActiveSummonActors"),
                $"{expectedActionName} should keep the first review slice to one active actor until multi-summon policies are authored.");
            for (int tier = 1; tier <= 3; tier++)
            {
                Assert.IsTrue(
                    action.TryGetTierReadout(tier, out SummonSlotActionProfile.SummonTierReadout readout),
                    $"{expectedActionName} should expose a reviewed tier {tier} readout.");
                Assert.IsTrue(readout.HasReadout);
            }
        }

        private static void AssertSummonActorPrefabContract(
            GameObject prefabRoot,
            DamageTeam expectedTeam,
            string label,
            bool expectPressureScreen)
        {
            Assert.IsNotNull(prefabRoot, $"{label} must be assigned.");
            SummonFrontlineProxy proxy = RequireComponent<SummonFrontlineProxy>(prefabRoot, label);
            SummonFrontlineClash clash = RequireComponent<SummonFrontlineClash>(prefabRoot, label);
            SummonFrontlineProxyPresenter presenter =
                RequireComponent<SummonFrontlineProxyPresenter>(prefabRoot, label);
            SummonFrontlineHealthBarPresenter healthBarPresenter =
                RequireComponent<SummonFrontlineHealthBarPresenter>(prefabRoot, label);
            CombatHealth health = RequireComponent<CombatHealth>(prefabRoot, label);
            SphereCollider bodyCollider = RequireComponent<SphereCollider>(prefabRoot, label);
            Rigidbody bodyRigidbody = RequireComponent<Rigidbody>(prefabRoot, label);

            Assert.AreSame(health, GetObjectReference<CombatHealth>(proxy, "health"));
            Assert.AreSame(proxy, GetObjectReference<SummonFrontlineProxy>(clash, "proxy"));
            Assert.AreSame(health, GetObjectReference<CombatHealth>(clash, "health"));
            Assert.AreSame(proxy, GetObjectReference<SummonFrontlineProxy>(presenter, "proxy"));
            Assert.AreSame(clash, GetObjectReference<SummonFrontlineClash>(presenter, "clash"));
            Assert.AreSame(proxy, healthBarPresenter.Proxy);
            Assert.AreSame(health, healthBarPresenter.Health);
            Assert.IsNotNull(healthBarPresenter.BarRoot, $"{label} should carry an in-world HP bar root.");
            Assert.IsNotNull(healthBarPresenter.FillRoot, $"{label} should carry an in-world HP fill.");
            Assert.GreaterOrEqual(
                healthBarPresenter.RendererCount,
                2,
                $"{label} should render both HP bar back and fill after being summoned.");
            if (expectPressureScreen)
            {
                Assert.IsNotNull(proxy.PressureScreen, $"{label} should carry a visible pressure screen.");
                Assert.AreNotSame(
                    proxy.transform,
                    proxy.PressureScreen.transform,
                    $"{label} pressure screen should stay separate from the body hitbox.");
                Assert.IsNotNull(
                    prefabRoot.GetComponent<SummonPressureScreenPresenter>(),
                    $"{label} pressure screen should have presentation feedback.");
            }
            else
            {
                Assert.IsNull(proxy.PressureScreen, $"{label} should not fake a tank screen.");
            }

            Assert.GreaterOrEqual(
                GetFloat(presenter, "clashFlashSeconds"),
                0.08f,
                $"{label} clash feedback should stay within the short readability window.");
            Assert.LessOrEqual(
                GetFloat(presenter, "clashFlashSeconds"),
                0.18f,
                $"{label} clash feedback should not become a long cinematic hold.");
            Assert.Greater(
                GetFloat(presenter, "clashFlashScale"),
                0f,
                $"{label} should visibly pulse on summon-vs-summon body clash.");
            Assert.AreEqual(expectedTeam, health.Team, $"{label} should carry the expected combat team.");
            Assert.IsTrue(bodyCollider.isTrigger, $"{label} body collider should be a trigger for clash feedback.");
            Assert.Greater(bodyCollider.radius, 0f, $"{label} body collider should have a positive radius.");
            Assert.IsTrue(bodyRigidbody.isKinematic, $"{label} body Rigidbody should be kinematic.");
            Assert.IsFalse(bodyRigidbody.useGravity, $"{label} body Rigidbody should not use gravity.");
        }

        private static void AssertBossProxyBodyContract(GameObject bossProxy)
        {
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy");
            SphereCollider bodyCollider = RequireComponent<SphereCollider>(bossProxy, "boss proxy");
            Rigidbody bodyRigidbody = RequireComponent<Rigidbody>(bossProxy, "boss proxy");

            Assert.AreEqual(DamageTeam.Enemy, bossHealth.Team, "Boss proxy body should carry enemy health on its root.");
            Assert.IsFalse(bodyCollider.isTrigger, "Boss proxy body should be a solid collider so summon trigger bodies can stop against it.");
            Assert.GreaterOrEqual(
                bodyCollider.radius,
                1f,
                "Boss proxy body collider should be wide enough for frontline summons to meet the readable humanoid body.");
            Assert.IsTrue(bodyRigidbody.isKinematic, "Boss proxy body Rigidbody should be kinematic.");
            Assert.IsFalse(bodyRigidbody.useGravity, "Boss proxy body Rigidbody should not use gravity.");
        }

        private static PlayerSupportSummonSlotAction RequireSupportSummonAction(string actionName)
        {
            PlayerSupportSummonSlotAction[] actions = Object.FindObjectsByType<PlayerSupportSummonSlotAction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null && actions[i].SlotActionName == actionName)
                {
                    return actions[i];
                }
            }

            Assert.Fail($"Missing support summon action {actionName}.");
            return null;
        }

        private static T RequireComponent<T>(GameObject owner, string label) where T : Component
        {
            T component = owner.GetComponent<T>();
            Assert.IsNotNull(component, $"{label} is missing {typeof(T).Name}.");
            return component;
        }

        private static void GrantEnergyToTier(SummonEnergyLadder energyLadder, int tier)
        {
            energyLadder.ResetLadder();
            for (int i = 0; i < tier; i++)
            {
                energyLadder.GrantCurrentTierEnergy(energyLadder.CurrentTierTarget);
            }

            Assert.AreEqual(tier, energyLadder.AvailableTier);
        }

        private static T RequireObject<T>() where T : Component
        {
            T[] found = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.Greater(found.Length, 0, $"Missing required object {typeof(T).Name}.");
            return found[0];
        }

        private static T GetObjectReference<T>(Object target, string propertyName) where T : Object
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{target.name} is missing serialized property {propertyName}.");
            return property.objectReferenceValue as T;
        }

        private static float GetFloat(Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{target.name} is missing serialized property {propertyName}.");
            return property.floatValue;
        }

        private static int GetInt(Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{target.name} is missing serialized property {propertyName}.");
            return property.intValue;
        }
    }
}
