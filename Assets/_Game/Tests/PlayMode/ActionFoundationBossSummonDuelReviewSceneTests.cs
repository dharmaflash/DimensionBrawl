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
        private const string SummonActorMoveSpeedParameter = "MoveSpeed";
        private const string SummonActorSpawnTrigger = "EliteSummonPackage";
        private const string SummonActorAttackTrigger = "Attack";
        private const string SummonActorHitTrigger = "Hit";
        private const string SummonActorDeathTrigger = "Death";

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
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            PlayerSummonSlot1Action summonSlot1Action = RequireObject<PlayerSummonSlot1Action>();
            PlayerSupportSummonSlotAction summonSlot2Action = RequireSupportSummonAction("SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action = RequireSupportSummonAction("SummonSlot3");
            BossPressureActionDirector bossPressureActionDirector = RequireObject<BossPressureActionDirector>();
            BossSummonPressureAction bossSummonPressureAction = RequireObject<BossSummonPressureAction>();
            BossBarrageEmitter bossBarrageEmitter = RequireObject<BossBarrageEmitter>();
            BossBasicFireEmitter bossBasicFireEmitter = RequireObject<BossBasicFireEmitter>();
            BossPressureCostLadder bossPressureCostLadder = RequireObject<BossPressureCostLadder>();
            SummonEnergyLadder energyLadder = RequireObject<SummonEnergyLadder>();
            SummonLaneSpace laneSpace = RequireObject<SummonLaneSpace>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
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
            Assert.AreEqual(1, duelOwner.RequiredBossResponsesToPlayerSummons);
            Assert.AreEqual(1, duelOwner.RequiredAllyPressureBlocks);
            Assert.AreEqual(1, duelOwner.RequiredSummonClashes);
            Assert.AreEqual(1, duelOwner.RequiredSummonActorDefeats);
            Assert.AreEqual(1, duelOwner.RequiredBossRepressureAfterSummonDefeat);
            Assert.AreEqual(1, duelOwner.RequiredFrontlineLoopCycles);
            Assert.AreEqual(1, duelOwner.RequiredSkill1ResponseUses);
            Assert.Greater(duelOwner.RequiredSkill1ResponseDamage, 0f);
            Assert.Greater(duelOwner.RequiredBossDamage, duelOwner.RequiredSkill1ResponseDamage);
            Assert.AreEqual(150f, GetFloat(duelOwner, "startingPlayerEnergy"), 0.001f);
            Assert.AreEqual(150f, GetFloat(duelOwner, "startingBossCost"), 0.001f);
            Assert.AreEqual(220f, duelOwner.RequiredBossDamage, 0.001f);
            Assert.AreEqual(16.5f, GetFloat(energyLadder, "baseEnergyPerSecond"), 0.001f);
            StringAssert.Contains("bossSP", duelOwner.ProgressLine);
            StringAssert.Contains("support", duelOwner.ProgressLine);
            StringAssert.Contains("bossReply", duelOwner.ProgressLine);
            StringAssert.Contains("clash", duelOwner.ProgressLine);
            StringAssert.Contains("defeat", duelOwner.ProgressLine);
            StringAssert.Contains("repressure", duelOwner.ProgressLine);
            StringAssert.Contains("loopCycle", duelOwner.ProgressLine);
            StringAssert.Contains("Loop", reviewHud.FrontlineLoopReadout);
            StringAssert.Contains("frontline", reviewHud.FrontlineLoopReadout);
            StringAssert.Contains("player", reviewHud.FrontlineLoopReadout);
            StringAssert.Contains("boss", reviewHud.FrontlineLoopReadout);
            StringAssert.Contains("Tune", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("EN", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("Cost", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("A0", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("E", reviewHud.FrontlineTuningReadout);

            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(duelOwner, "energyLadder"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(duelOwner, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(duelOwner, "summonSlot1Action"));
            Assert.AreSame(summonSlot2Action, GetObjectReference<PlayerSupportSummonSlotAction>(duelOwner, "summonSlot2Action"));
            Assert.AreSame(summonSlot3Action, GetObjectReference<PlayerSupportSummonSlotAction>(duelOwner, "summonSlot3Action"));
            Assert.AreSame(bossBarrageEmitter, GetObjectReference<BossBarrageEmitter>(duelOwner, "bossBarrageEmitter"));
            Assert.AreSame(bossBasicFireEmitter, GetObjectReference<BossBasicFireEmitter>(duelOwner, "bossBasicFireEmitter"));
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
            AssertVector3(
                new Vector3(0.75f, 0.88f, 3.12f),
                GetVector3(cameraController, "aimCameraOffset"),
                "Boss summon duel aim camera offset should keep the reviewed inspector framing.");
            Assert.AreEqual(4, bossPressureActionDirector.ActionSlotCount);
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
                1,
                "EscortProbeFrontlineCheck");
            AssertBossPressureSlot(
                bossPressureActionDirector,
                2,
                BossPressureActionKind.SummonPressure,
                2,
                "SummonSlot1PressureBlock",
                true,
                1);
            AssertBossPressureSlot(
                bossPressureActionDirector,
                3,
                BossPressureActionKind.PunishOverextend,
                3,
                "RetreatOrSpendHighTierAnswer");

            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(bossPressureCostLadder, "laneSpace"));
            Assert.IsTrue(bossSummonPressureAction.HasPressureProfile);
            Assert.AreEqual(3, bossSummonPressureAction.PressureProfile.TierCount);
            AssertBossSummonPressureRoleProfile(bossSummonPressureAction.PressureProfile);
            Assert.IsTrue(bossSummonPressureAction.CanRelease);
            Assert.AreEqual(
                2,
                GetInt(bossSummonPressureAction, "maxActiveSummonActors"),
                "Boss pressure should be allowed to keep a small frontline pair so it does not feel passive next to player support summons.");
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
            AssertSupportSummonRoleProfiles(summonSlot2Action, summonSlot3Action);
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
            Assert.AreSame(bossBasicFireEmitter, GetObjectReference<BossBasicFireEmitter>(reviewHud, "bossBasicFireEmitter"));
            Assert.IsTrue(
                targetSelector.IncludesActiveHostileSummons,
                "Player response targeting should include active hostile summon bodies without expanding authored scene candidates.");
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
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            PlayerSupportSummonSlotAction summonSlot2Action = RequireSupportSummonAction("SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action = RequireSupportSummonAction("SummonSlot3");
            BossSummonPressureAction bossSummonPressureAction = RequireObject<BossSummonPressureAction>();
            BossPressureActionDirector bossPressureActionDirector = RequireObject<BossPressureActionDirector>();
            BossPressureCostLadder bossPressureCostLadder = RequireObject<BossPressureCostLadder>();
            BossBarrageEmitter bossBarrageEmitter = RequireObject<BossBarrageEmitter>();
            BossSummonDuelReviewOwner duelOwner = RequireObject<BossSummonDuelReviewOwner>();
            BossBarrageLaneReviewHud reviewHud = RequireObject<BossBarrageLaneReviewHud>();

            energyLadder.SetGainEnabled(false);
            GrantEnergyToTier(energyLadder, 1);
            Assert.IsTrue(summonSlot2Action.TryUseSummon());
            Assert.AreEqual(1, summonSlot2Action.LastSpentTier);
            Assert.AreEqual(1, summonSlot2Action.TotalUseCount);
            Assert.AreEqual(1, duelOwner.ObservedPlayerSummonUses);
            Assert.AreEqual(1, duelOwner.ObservedSupportSummonUses);
            Assert.IsTrue(
                bossPressureActionDirector.IsPlayerSummonResponseWindowActive,
                "Support summon use should open a narrow boss response window for barrage/summon-pressure answers.");
            Assert.AreEqual(1, bossPressureActionDirector.LastObservedPlayerSummonTier);
            Assert.AreEqual("BacklineMarksman", summonSlot2Action.LastSummonActorRoleId);
            Assert.Greater(summonSlot2Action.ActiveSummonActorCount, 0);
            Assert.IsTrue(
                summonSlot2Action.LastSummonActorHasHealth,
                "S2 should create a summon actor with body health, not only a temporary visual marker.");
            Assert.Greater(summonSlot2Action.LastSummonActorRemainingLifetimeSeconds, 0f);
            Assert.IsTrue(
                float.IsPositiveInfinity(summonSlot2Action.LastSummonActorRemainingLifetimeSeconds),
                "S2 is a normal body-bearing summon and should persist until defeated or recalled, not expire as a short effect.");
            Assert.AreEqual(SummonFrontlineProxyExitReason.None, summonSlot2Action.LastSummonActorExitReason);
            SummonFrontlineProxy slot2Proxy = RequireActiveSummonProxy(DamageTeam.AllySummon, 1.35f, "S2");
            AssertSummonProxyIsMarching(slot2Proxy, 1.35f, "S2");
            float slot2EntryProgress = slot2Proxy.AdvanceProgress01;
            yield return new WaitForSeconds(0.15f);
            AssertSummonProxyAdvancedWithoutSnapping(slot2Proxy, slot2EntryProgress, "S2");
            Assert.Greater(
                summonSlot2Action.ActiveProjectileCount,
                0,
                "S2 should read as a ranged support summon by firing visible lane projectiles after entry.");
            Assert.GreaterOrEqual(
                summonSlot2Action.LastVolleyWaveCount,
                1,
                "S2 should expose its repeated support-volley behavior for HUD and review tests.");

            GrantEnergyToTier(energyLadder, 3);
            Assert.IsTrue(summonSlot3Action.TryUseSummon());
            Assert.AreEqual(3, summonSlot3Action.LastSpentTier);
            Assert.AreEqual(1, summonSlot3Action.TotalUseCount);
            Assert.AreEqual("VanguardCommander", summonSlot3Action.LastSummonActorRoleId);
            Assert.Greater(summonSlot3Action.ActiveSummonActorCount, 0);
            Assert.IsTrue(
                summonSlot3Action.LastSummonActorHasHealth,
                "S3 should create a summon actor with body health, not only a temporary visual marker.");
            Assert.Greater(summonSlot3Action.LastSummonActorRemainingLifetimeSeconds, 0f);
            Assert.IsTrue(
                float.IsPositiveInfinity(summonSlot3Action.LastSummonActorRemainingLifetimeSeconds),
                "S3 is a normal body-bearing summon and should persist until defeated or recalled, not expire as a short effect.");
            Assert.AreEqual(SummonFrontlineProxyExitReason.None, summonSlot3Action.LastSummonActorExitReason);
            SummonFrontlineProxy slot3Proxy = RequireActiveSummonProxy(DamageTeam.AllySummon, 1.25f, "S3");
            AssertSummonProxyIsMarching(slot3Proxy, 1.25f, "S3");
            float slot3EntryProgress = slot3Proxy.AdvanceProgress01;
            yield return new WaitForSeconds(0.15f);
            AssertSummonProxyAdvancedWithoutSnapping(slot3Proxy, slot3EntryProgress, "S3");
            Assert.Greater(
                summonSlot3Action.ActiveProjectileCount,
                0,
                "S3 should read as a vanguard support summon with a visible projectile response, not only a button state.");
            Assert.GreaterOrEqual(
                summonSlot3Action.LastVolleyWaveCount,
                1,
                "S3 should expose its slower vanguard counter-volley behavior for HUD and review tests.");

            Assert.IsTrue(bossSummonPressureAction.TryReleasePressureSummon(2));
            Assert.Greater(bossSummonPressureAction.ActiveSummonActorCount, 0);
            Assert.IsTrue(
                bossSummonPressureAction.LastSummonActorHasHealth,
                "Boss summon pressure should also release a damageable frontline actor for summon-vs-summon exchange.");
            StringAssert.Contains("frontline", reviewHud.FrontlineLoopReadout);
            StringAssert.Contains("ally", reviewHud.FrontlineLoopReadout);
            StringAssert.Contains("enemy", reviewHud.FrontlineLoopReadout);
            StringAssert.Contains("Tune", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("EN", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("Cost", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("hp", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("spd", reviewHud.FrontlineTuningReadout);
            StringAssert.Contains("dps", reviewHud.FrontlineTuningReadout);
            Assert.Greater(bossSummonPressureAction.LastSummonActorRemainingLifetimeSeconds, 0f);
            Assert.IsTrue(
                float.IsPositiveInfinity(bossSummonPressureAction.LastSummonActorRemainingLifetimeSeconds),
                "Boss summon pressure should create a persistent opposing actor until the player answers it.");
            Assert.AreEqual(SummonFrontlineProxyExitReason.None, bossSummonPressureAction.LastSummonActorExitReason);
            SummonFrontlineProxy enemyProxy = bossSummonPressureAction.LastSummonActor;
            Assert.IsNotNull(enemyProxy, "Boss summon pressure should expose the latest released summon actor.");
            Assert.AreEqual(DamageTeam.Enemy, enemyProxy.Health.Team);
            AssertSummonProxyIsMarching(enemyProxy, 1.42f, "boss summon");
            float enemyEntryProgress = enemyProxy.AdvanceProgress01;
            yield return new WaitForSeconds(0.15f);
            AssertSummonProxyAdvancedWithoutSnapping(enemyProxy, enemyEntryProgress, "boss summon");
            CombatHealth enemySummonHealth = enemyProxy.Health;
            targetSelector.NotifyTargetContact(enemySummonHealth);
            Assert.AreSame(
                enemySummonHealth,
                targetSelector.CurrentTargetHealth,
                "The review loop should let player Skill1/ranged fire respond to the active boss summon body.");

            Assert.IsTrue(
                enemySummonHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    enemySummonHealth.MaxHealth + 100f,
                    enemySummonHealth.transform.position,
                    Vector3.forward,
                    0f)),
                "The duel review should let the player side remove an enemy summon body through CombatHealth.");
            enemyProxy.Tick(0.01f);
            yield return null;
            Assert.AreEqual(
                SummonFrontlineProxyExitReason.Defeated,
                bossSummonPressureAction.LastSummonActorExitReason,
                "Boss summon pressure should expose defeated removal after its body HP is depleted.");
            Assert.GreaterOrEqual(
                duelOwner.ObservedSummonActorDefeats,
                1,
                "The review owner should count the first summon body removal before re-pressure is checked.");

            yield return QueueBossRepressureAfterDefeat(
                bossPressureActionDirector,
                bossBarrageEmitter,
                bossPressureCostLadder,
                duelOwner,
                1,
                "first loop");

            Assert.IsTrue(bossSummonPressureAction.TryReleasePressureSummon(1));
            SummonFrontlineProxy secondEnemyProxy = RequireActiveSummonProxy(DamageTeam.Enemy, 1.35f, "second boss summon");
            AssertSummonProxyIsMarching(secondEnemyProxy, 1.35f, "second boss summon");
            CombatHealth secondEnemySummonHealth = secondEnemyProxy.Health;
            Assert.IsTrue(
                secondEnemySummonHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    secondEnemySummonHealth.MaxHealth + 100f,
                    secondEnemySummonHealth.transform.position,
                    Vector3.forward,
                    0f)),
                "The second boss summon body should also be removable through CombatHealth.");
            yield return null;
            Assert.GreaterOrEqual(
                duelOwner.ObservedSummonActorDefeats,
                duelOwner.RequiredSummonActorDefeats,
                "The review owner should count repeated summon body removal before claiming the loop is stable.");

            Assert.IsTrue(
                bossSummonPressureAction.TryReleasePressureSummon(1),
                "Boss pressure should be able to release another summon after the second body falls.");
            yield return null;
            Assert.GreaterOrEqual(
                duelOwner.ObservedBossRepressureAfterSummonDefeat,
                duelOwner.RequiredBossRepressureAfterSummonDefeat,
                "The duel review should count boss re-pressure after summon body removal.");
            Assert.GreaterOrEqual(
                duelOwner.ObservedFrontlineLoopCycles,
                duelOwner.RequiredFrontlineLoopCycles,
                "The duel review should expose remove-to-repressure as a full frontline loop cycle.");
            StringAssert.Contains("defeat", duelOwner.ProgressLine);
            StringAssert.Contains("repressure", duelOwner.ProgressLine);
            StringAssert.Contains("loopCycle", duelOwner.ProgressLine);
        }

        private static IEnumerator QueueBossRepressureAfterDefeat(
            BossPressureActionDirector bossPressureActionDirector,
            BossBarrageEmitter bossBarrageEmitter,
            BossPressureCostLadder bossPressureCostLadder,
            BossSummonDuelReviewOwner duelOwner,
            int targetRepressureCount,
            string label)
        {
            if (duelOwner.ObservedBossRepressureAfterSummonDefeat >= targetRepressureCount)
            {
                yield break;
            }

            int repressureBefore = duelOwner.ObservedBossRepressureAfterSummonDefeat;
            bossPressureActionDirector.SetHoldForNextTierActionWhenGateAllows(false);
            bossBarrageEmitter.SetFiringEnabled(true);
            bossPressureCostLadder.ResetLadder();
            bossPressureCostLadder.GrantCurrentTierCost(bossPressureCostLadder.CurrentTierTarget);
            bossPressureActionDirector.NotifyPlayerSummonFrontlineCreated(1);

            bool sawBossRepressure = false;
            const float StepSeconds = 0.05f;
            for (int i = 0; i < 180; i++)
            {
                bossPressureActionDirector.NotifyPlayerSummonFrontlineCreated(1);
                bossPressureCostLadder.Tick(StepSeconds);
                bossBarrageEmitter.Tick(StepSeconds);
                bossPressureActionDirector.Tick(StepSeconds);
                sawBossRepressure |= duelOwner.ObservedBossRepressureAfterSummonDefeat > repressureBefore
                    && duelOwner.ObservedBossRepressureAfterSummonDefeat >= targetRepressureCount;
                if (sawBossRepressure)
                {
                    break;
                }

                yield return null;
            }

            Assert.IsTrue(
                sawBossRepressure,
                $"Boss pressure should be able to resume after a summon body is defeated during {label} "
                + $"({duelOwner.ObservedBossRepressureAfterSummonDefeat}/{targetRepressureCount}).");
        }

        private static void AssertBossPressureSlot(
            BossPressureActionDirector director,
            int index,
            BossPressureActionKind expectedKind,
            int expectedTier,
            string expectedResponseId,
            bool expectedUsePlayerSummonResponseGate = false,
            int expectedMinimumPlayerSummonTier = 1)
        {
            Assert.IsTrue(director.TryGetActionSlot(index, out BossPressureActionDirector.BossPressureActionSlot slot));
            Assert.IsNotNull(slot.Pattern);
            Assert.AreEqual(expectedKind, slot.ActionKind);
            Assert.AreEqual(expectedTier, slot.MinimumTier);
            Assert.AreEqual(expectedResponseId, slot.ResponseId);
            Assert.AreEqual(expectedUsePlayerSummonResponseGate, slot.UsePlayerSummonResponseGate);
            Assert.AreEqual(expectedMinimumPlayerSummonTier, slot.MinimumPlayerSummonTier);
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

        private static void AssertSupportSummonRoleProfiles(
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action)
        {
            SummonSlotActionProfile slot2Profile =
                GetObjectReference<SummonSlotActionProfile>(summonSlot2Action, "summonActionProfile");
            SummonSlotActionProfile slot3Profile =
                GetObjectReference<SummonSlotActionProfile>(summonSlot3Action, "summonActionProfile");
            PlayerSummonSlot1Action.SummonTierSettings[] slot2Tiers = slot2Profile.CopyTierSettings();
            PlayerSummonSlot1Action.SummonTierSettings[] slot3Tiers = slot3Profile.CopyTierSettings();

            Assert.AreEqual("SummonSlot2.BacklineMarksman", slot2Profile.ActionId);
            Assert.AreEqual("SummonSlot3.VanguardCommander", slot3Profile.ActionId);
            Assert.AreEqual(3, slot2Tiers.Length);
            Assert.AreEqual(3, slot3Tiers.Length);
            float[] slot2ExpectedHealth = { 160f, 190f, 225f };
            float[] slot2ExpectedMoveSpeed = { 1.35f, 1.42f, 1.5f };
            float[] slot2ExpectedDps = { 20f, 24f, 28f };
            float[] slot3ExpectedHealth = { 360f, 430f, 520f };
            float[] slot3ExpectedMoveSpeed = { 1.15f, 1.2f, 1.25f };
            float[] slot3ExpectedDps = { 24f, 32f, 42f };
            int[] slot3ExpectedScreens = { 2, 4, 7 };
            for (int i = 0; i < slot2Tiers.Length; i++)
            {
                Assert.AreEqual("BacklineMarksman", slot2Tiers[i].ActorRoleId);
                Assert.AreEqual(0f, slot2Tiers[i].ActorLifetimeSeconds, 0.001f);
                Assert.AreEqual(slot2ExpectedHealth[i], slot2Tiers[i].ActorMaxHealth, 0.001f);
                Assert.AreEqual(slot2ExpectedMoveSpeed[i], slot2Tiers[i].ActorMoveSpeed, 0.001f);
                Assert.AreEqual(slot2ExpectedDps[i], slot2Tiers[i].ActorAttackDamagePerSecond, 0.001f);
                Assert.AreEqual(0, slot2Tiers[i].ScreenIntercepts);
                Assert.AreEqual("VanguardCommander", slot3Tiers[i].ActorRoleId);
                Assert.AreEqual(0f, slot3Tiers[i].ActorLifetimeSeconds, 0.001f);
                Assert.AreEqual(slot3ExpectedHealth[i], slot3Tiers[i].ActorMaxHealth, 0.001f);
                Assert.AreEqual(slot3ExpectedMoveSpeed[i], slot3Tiers[i].ActorMoveSpeed, 0.001f);
                Assert.AreEqual(slot3ExpectedDps[i], slot3Tiers[i].ActorAttackDamagePerSecond, 0.001f);
                Assert.AreEqual(slot3ExpectedScreens[i], slot3Tiers[i].ScreenIntercepts);
                Assert.Greater(slot3Tiers[i].ScreenIntercepts, 0);
                Assert.Greater(slot3Tiers[i].ActorMaxHealth, slot2Tiers[i].ActorMaxHealth);
                Assert.Greater(
                    slot2Tiers[i].ActorMoveSpeed,
                    slot3Tiers[i].ActorMoveSpeed,
                    "S2 should keep the smaller/faster marksman read while S3 advances like a heavier frontline body.");
                Assert.Greater(
                    slot3Tiers[i].ActorAttackDamagePerSecond,
                    slot2Tiers[i].ActorAttackDamagePerSecond,
                    "S3 should win sustained body trades through HP and clash DPS instead of only projectile count.");
                Assert.LessOrEqual(
                    slot2Tiers[i].ActorMoveSpeed,
                    1.5f,
                    "Support summons should still march across the lane instead of snapping forward.");
                Assert.LessOrEqual(
                    slot3Tiers[i].ActorMoveSpeed,
                    1.25f,
                    "Vanguard support should visibly trudge forward as the heavier body.");
                Assert.GreaterOrEqual(
                    slot2Tiers[i].ProjectileCount,
                    slot3Tiers[i].ProjectileCount,
                    "S2 should keep the frequent marksman volley identity while S3 spends more budget on body/screen.");
            }

            Assert.Less(
                GetFloat(summonSlot2Action, "volleyIntervalSeconds"),
                GetFloat(summonSlot3Action, "volleyIntervalSeconds"),
                "S2 should cycle its support shots faster than the slower vanguard counter-volley.");
        }

        private static void AssertBossSummonPressureRoleProfile(BossSummonPressureProfile profile)
        {
            BossSummonPressureAction.BossSummonTierSettings[] tiers = profile.CopyTierSettings();
            string[] expectedRoles = { "EscortProbe", "PressureScreen", "ClampGuard" };
            float[] expectedHealth = { 220f, 320f, 460f };
            float[] expectedMoveSpeed = { 1.35f, 1.42f, 1.48f };
            float[] expectedDps = { 32f, 44f, 58f };
            int[] expectedScreens = { 2, 4, 7 };

            Assert.AreEqual(3, tiers.Length);
            for (int i = 0; i < tiers.Length; i++)
            {
                Assert.AreEqual(expectedRoles[i], tiers[i].ActorRoleId);
                Assert.AreEqual(0f, tiers[i].ActorLifetimeSeconds, 0.001f);
                Assert.AreEqual(expectedHealth[i], tiers[i].ActorMaxHealth, 0.001f);
                Assert.AreEqual(expectedMoveSpeed[i], tiers[i].ActorMoveSpeed, 0.001f);
                Assert.AreEqual(expectedDps[i], tiers[i].ActorAttackDamagePerSecond, 0.001f);
                Assert.AreEqual(expectedScreens[i], tiers[i].ScreenIntercepts);
                Assert.LessOrEqual(
                    tiers[i].ActorMoveSpeed,
                    1.5f,
                    "Boss pressure summons should walk into the player side instead of snapping like a projectile.");

                if (i == 0)
                {
                    continue;
                }

                Assert.Greater(tiers[i].ActorMaxHealth, tiers[i - 1].ActorMaxHealth);
                Assert.Greater(tiers[i].ActorAttackDamagePerSecond, tiers[i - 1].ActorAttackDamagePerSecond);
                Assert.Greater(tiers[i].ScreenIntercepts, tiers[i - 1].ScreenIntercepts);
            }
        }

        private static CombatHealth RequireActiveEnemySummonHealth()
        {
            for (int i = 0; i < SummonFrontlineProxy.ActiveRegisteredProxyCount; i++)
            {
                Assert.IsTrue(SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy));
                if (proxy != null
                    && proxy.Health != null
                    && proxy.Health.Team == DamageTeam.Enemy)
                {
                    return proxy.Health;
                }
            }

            Assert.Fail("Boss summon pressure should register an active enemy summon body for player response targeting.");
            return null;
        }

        private static SummonFrontlineProxy RequireActiveSummonProxy(
            DamageTeam expectedTeam,
            float expectedMoveSpeed,
            string label)
        {
            for (int i = 0; i < SummonFrontlineProxy.ActiveRegisteredProxyCount; i++)
            {
                Assert.IsTrue(SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy));
                if (proxy == null
                    || proxy.Health == null
                    || proxy.Health.Team != expectedTeam)
                {
                    continue;
                }

                if (Mathf.Abs(proxy.ActiveMoveSpeed - expectedMoveSpeed) <= 0.001f)
                {
                    return proxy;
                }
            }

            Assert.Fail($"{label} should register an active {expectedTeam} summon body moving at {expectedMoveSpeed:0.00}.");
            return null;
        }

        private static void AssertSummonProxyIsMarching(
            SummonFrontlineProxy proxy,
            float expectedMoveSpeed,
            string label)
        {
            Assert.IsNotNull(proxy, $"{label} summon proxy should be active.");
            Assert.AreEqual(expectedMoveSpeed, proxy.ActiveMoveSpeed, 0.001f);
            Assert.AreEqual(
                SummonFrontlineProxyState.Advancing,
                proxy.CurrentState,
                $"{label} should start in an advancing state so the player can read it entering the frontline.");
            Assert.Greater(
                proxy.AdvanceDistance,
                0.5f,
                $"{label} should have real travel distance instead of spawning already at its target.");
            Assert.Less(
                proxy.AdvanceProgress01,
                0.25f,
                $"{label} should not snap most of the way to the target on spawn.");
        }

        private static void AssertSummonProxyAdvancedWithoutSnapping(
            SummonFrontlineProxy proxy,
            float previousProgress,
            string label)
        {
            Assert.IsNotNull(proxy, $"{label} summon proxy should remain active while entering.");
            Assert.Greater(
                proxy.AdvanceProgress01,
                previousProgress,
                $"{label} should visibly advance after entering the lane.");
            Assert.Less(
                proxy.AdvanceProgress01,
                0.65f,
                $"{label} should still be marching after a short review beat instead of instantly reaching the target.");
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
            AssertSummonProxyAnimatorPresentation(prefabRoot, presenter, label);
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

        private static void AssertSummonProxyAnimatorPresentation(
            GameObject prefabRoot,
            SummonFrontlineProxyPresenter presenter,
            string label)
        {
            Animator[] animators = prefabRoot.GetComponentsInChildren<Animator>(true);
            Assert.AreEqual(1, animators.Length, $"{label} should keep one promoted visual Animator.");
            Animator animator = animators[0];
            Assert.IsNotNull(animator.runtimeAnimatorController, $"{label} Animator should keep its controller.");
            Assert.AreSame(animator, presenter.Animator, $"{label} presenter should target the promoted visual Animator.");
            Assert.AreEqual(SummonActorMoveSpeedParameter, presenter.MoveSpeedParameter);
            Assert.AreEqual(SummonActorSpawnTrigger, presenter.SpawnTrigger);
            Assert.AreEqual(SummonActorAttackTrigger, presenter.AttackTrigger);
            Assert.AreEqual(SummonActorHitTrigger, presenter.HitTrigger);
            Assert.AreEqual(SummonActorDeathTrigger, presenter.DeathTrigger);
            AssertAnimatorParameter(animator, presenter.MoveSpeedParameter, AnimatorControllerParameterType.Float);
            AssertAnimatorParameter(animator, presenter.SpawnTrigger, AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(animator, presenter.AttackTrigger, AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(animator, presenter.HitTrigger, AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(animator, presenter.DeathTrigger, AnimatorControllerParameterType.Trigger);
        }

        private static void AssertAnimatorParameter(
            Animator animator,
            string parameterName,
            AnimatorControllerParameterType expectedType)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == expectedType
                    && string.Equals(parameter.name, parameterName, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            Assert.Fail($"{animator.name} is missing {expectedType} parameter {parameterName}.");
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

        private static Vector3 GetVector3(Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{target.name} is missing serialized property {propertyName}.");
            return property.vector3Value;
        }

        private static int GetInt(Object target, string propertyName)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{target.name} is missing serialized property {propertyName}.");
            return property.intValue;
        }

        private static void AssertVector3(Vector3 expected, Vector3 actual, string message)
        {
            Assert.AreEqual(expected.x, actual.x, 0.001f, message);
            Assert.AreEqual(expected.y, actual.y, 0.001f, message);
            Assert.AreEqual(expected.z, actual.z, 0.001f, message);
        }
    }
}
