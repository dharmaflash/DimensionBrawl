using System.Collections;
using System.Collections.Generic;
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

namespace DimensionBrawl.Tests
{
    public sealed class ActionFoundationBossBarrageLaneReviewSceneTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity";
        private const string PatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_NeedleLock.asset";
        private const string CoverFirePatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_CoverFire.asset";
        private const string EscortScreenPatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_EscortScreen.asset";
        private const string LayeredSalvoPatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_LayeredSalvo.asset";
        private const string StaggeredCrossfirePatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_StaggeredCrossfire.asset";
        private const string TwinSweepPatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_TwinSweep.asset";
        private const string LeftClampPatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_LeftClamp.asset";
        private const string RightClampPatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_RightClamp.asset";
        private const string PunishNetPatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_PunishNet.asset";
        private const string LinePressurePatternProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarrage_LinePressure.asset";
        private const string ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_NeedleLock.prefab";
        private const string LocalDefenseProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_PlayerAction_BossBarrageLocalDefense.asset";
        private const string Skill1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerSkill1Projectile_LaneBolt.prefab";
        private const string SummonSlot1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";
        private const string SummonSlot1EntryCuePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1EntryCue_MagicCircle.prefab";
        private const string SummonSlot1ActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab";
        private const string LaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string BossHumanoidVisualName = "BossBarrageLaneReview_HumanoidBossVisual_SummonCallerElite";
        private const string BossProjectileCoreName = "BossBarrageLaneReview_BossProxyMarker";
        private const string CloseThreatRootName = "BossBarrageLaneReview_CloseThreat_ClosePunish";
        private const string ProjectilePoolRootName = "BossBarrageLaneReview_ProjectilePool";
        private const string ActionCuePoolRootName = "BossBarrageLaneReview_ActionCuePool";
        private const string SummonActorPoolRootName = "BossBarrageLaneReview_SummonActorPool";
        private const string PocketOwnerRootName = "BossBarrageLaneReview_PocketOwner";
        private const string BossTelegraphRootName = "BossBarrageLaneReview_BossBarrageTelegraphMarkers";
        private const string EnergyZoneRootName = "BossBarrageLaneReview_EnergyRiskZones";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";
        private const string BacklineEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageBacklineEnergyZone.mat";
        private const string MidEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageMidEnergyZone.mat";
        private const string ForwardEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageForwardEnergyZone.mat";
        private static readonly string[] RequiredBossPatternCueIds =
        {
            "NeedleLock",
            "CoverFire",
            "EscortScreen",
            "LayeredSalvo",
            "StaggeredCrossfire",
            "TwinSweep",
            "LeftClamp",
            "RightClamp",
            "PunishNet",
            "LinePressure"
        };

        [UnitySetUp]
        public IEnumerator LoadBossBarrageLaneReviewScene()
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
        public IEnumerator ReviewSceneBindsPlayerEnergyAndBossBarrage()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player.gameObject, "player action controller");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            AssertBossHumanoidVisual(bossRoot);
            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarrageCameraCueDriver bossCameraCueDriver =
                RequireComponent<BossBarrageCameraCueDriver>(cameraController.gameObject, "boss barrage camera cue driver");
            GameObject projectileRoot = RequireRoot(ProjectilePoolRootName);
            GameObject actionCueRoot = RequireRoot(ActionCuePoolRootName);
            GameObject summonActorRoot = RequireRoot(SummonActorPoolRootName);
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "boss barrage HUD");
            BossBarrageLaneTelegraphPresenter telegraphPresenter =
                RequireComponent<BossBarrageLaneTelegraphPresenter>(
                    RequireRoot(BossTelegraphRootName),
                    "boss barrage lane telegraph presenter");
            GameObject energyZoneRoot = RequireRoot(EnergyZoneRootName);

            Assert.AreSame(laneSpace, player.LaneSpace, "Player movement must clamp through the authored lane space.");
            Assert.AreSame(emitter, telegraphPresenter.BossBarrageEmitter);
            Assert.AreSame(laneSpace, telegraphPresenter.LaneSpace);
            Assert.GreaterOrEqual(
                telegraphPresenter.MarkerCount,
                9,
                "Boss barrage lane telegraphs must be authored world markers, not HUD-only warning text.");
            AssertEnergyZoneMarker(
                energyZoneRoot.transform,
                "BackSafety_ENSlow_0_33",
                laneSpace,
                0f,
                1f / 3f,
                BacklineEnergyZoneMaterialPath);
            AssertEnergyZoneMarker(
                energyZoneRoot.transform,
                "MidCharge_ENBase_33_66",
                laneSpace,
                1f / 3f,
                2f / 3f,
                MidEnergyZoneMaterialPath);
            AssertEnergyZoneMarker(
                energyZoneRoot.transform,
                "ForwardRisk_ENFast_66_100",
                laneSpace,
                2f / 3f,
                1f,
                ForwardEnergyZoneMaterialPath);
            Assert.AreSame(LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath), playerActionController.ActionProfile);
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(energyLadder, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(energyLadder, "trackedPlayer"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(skill1Action, "energyLadder"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(skill1Action, "sourceHealth"));
            Assert.AreSame(targetSelector, GetObjectReference<PlayerCombatTargetSelector>(skill1Action, "targetSelector"));
            Assert.AreSame(LoadAsset<GameObject>(Skill1ProjectilePrefabPath), GetObjectReference<GameObject>(skill1Action, "projectilePrefabObject"));
            Assert.AreSame(projectileRoot.transform, GetObjectReference<Transform>(skill1Action, "projectileRoot"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(summonSlot1Action, "energyLadder"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(summonSlot1Action, "sourceHealth"));
            Assert.AreSame(targetSelector, GetObjectReference<PlayerCombatTargetSelector>(summonSlot1Action, "targetSelector"));
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(summonSlot1Action, "frontlineTargetHealth"));
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(summonSlot1Action, "laneSpace"));
            Assert.AreSame(LoadAsset<GameObject>(SummonSlot1ProjectilePrefabPath), GetObjectReference<GameObject>(summonSlot1Action, "projectilePrefabObject"));
            Assert.AreSame(LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath), GetObjectReference<GameObject>(summonSlot1Action, "entryCuePrefab"));
            GameObject summonActorPrefabObject = LoadAsset<GameObject>(SummonSlot1ActorPrefabPath);
            Assert.AreSame(summonActorPrefabObject, GetObjectReference<GameObject>(summonSlot1Action, "summonActorPrefabObject"));
            SummonFrontlineProxy summonActorPrefab =
                RequireComponent<SummonFrontlineProxy>(summonActorPrefabObject, "SummonSlot1 actor prefab");
            SummonPressureScreen summonPressureScreen =
                RequireComponent<SummonPressureScreen>(summonActorPrefabObject, "SummonSlot1 pressure screen");
            SummonPressureScreenPresenter summonPressureScreenPresenter =
                RequireComponent<SummonPressureScreenPresenter>(summonActorPrefabObject, "SummonSlot1 pressure screen presenter");
            SummonFrontlineProxyPresenter summonActorPresenter =
                RequireComponent<SummonFrontlineProxyPresenter>(summonActorPrefabObject, "SummonSlot1 actor presenter");
            Assert.AreSame(summonPressureScreen, summonActorPrefab.PressureScreen);
            Assert.AreSame(summonPressureScreen, summonPressureScreenPresenter.PressureScreen);
            Assert.Greater(summonPressureScreenPresenter.RendererCount, 0);
            Assert.AreSame(summonActorPrefab, summonActorPresenter.Proxy);
            Assert.IsNotNull(summonActorPresenter.PulseRoot);
            Assert.GreaterOrEqual(summonActorPresenter.RendererCount, 2);
            Assert.AreEqual(DamageTeam.AllySummon, summonPressureScreen.OwnerTeam);
            Assert.AreSame(projectileRoot.transform, GetObjectReference<Transform>(summonSlot1Action, "projectileRoot"));
            Assert.AreSame(actionCueRoot.transform, GetObjectReference<Transform>(summonSlot1Action, "cueRoot"));
            Assert.AreSame(summonActorRoot.transform, GetObjectReference<Transform>(summonSlot1Action, "summonActorRoot"));
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(emitter, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(emitter, "trackedPlayer"));
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(emitter, "sourceHealth"));
            Assert.AreSame(LoadAsset<BossBarragePatternProfile>(PatternProfilePath), GetObjectReference<BossBarragePatternProfile>(emitter, "patternProfile"));
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(PatternProfilePath),
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 0));
            BossBarragePatternProfile coverFirePattern = LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath);
            Assert.AreSame(
                coverFirePattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 1));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, coverFirePattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.CenterSpread, coverFirePattern.LateralShape);
            BossBarragePatternProfile escortScreenPattern = LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath);
            Assert.AreSame(
                escortScreenPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 2));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, escortScreenPattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.EscortScreen, escortScreenPattern.LateralShape);
            BossBarragePatternProfile layeredSalvoPattern = LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath);
            Assert.AreSame(
                layeredSalvoPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 3));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, layeredSalvoPattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.LayeredSalvo, layeredSalvoPattern.LateralShape);
            BossBarragePatternProfile twinSweepPattern = LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath);
            BossBarragePatternProfile staggeredCrossfirePattern =
                LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath);
            Assert.AreSame(
                staggeredCrossfirePattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 4));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, staggeredCrossfirePattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.StaggeredCrossfire, staggeredCrossfirePattern.LateralShape);
            Assert.AreSame(
                twinSweepPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 5));
            Assert.AreEqual(BossBarrageLateralShape.TwinColumns, twinSweepPattern.LateralShape);
            BossBarragePatternProfile leftClampPattern = LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath);
            Assert.AreSame(
                leftClampPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 6));
            Assert.AreEqual(BossBarrageLateralShape.SideClamp, leftClampPattern.LateralShape);
            Assert.Less(leftClampPattern.SideClampDirection, 0f);
            BossBarragePatternProfile rightClampPattern = LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath);
            Assert.AreSame(
                rightClampPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 7));
            Assert.AreEqual(BossBarrageLateralShape.SideClamp, rightClampPattern.LateralShape);
            Assert.Greater(rightClampPattern.SideClampDirection, 0f);
            BossBarragePatternProfile punishNetPattern = LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath);
            Assert.AreSame(
                punishNetPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 8));
            Assert.AreEqual(BossBarrageLateralShape.PunishNet, punishNetPattern.LateralShape);
            BossBarragePatternProfile linePressurePattern = LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath);
            Assert.AreSame(
                linePressurePattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 9));
            Assert.AreEqual(BossBarrageLateralShape.LinePressure, linePressurePattern.LateralShape);
            Assert.Greater(linePressurePattern.LinePressureDirection, 0f);
            Assert.AreSame(LoadAsset<GameObject>(ProjectilePrefabPath), GetObjectReference<GameObject>(emitter, "projectilePrefabObject"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(targetSelector, "selfHealth"));
            Assert.AreSame(closeThreatHealth, GetArrayObjectReference<CombatHealth>(targetSelector, "targetCandidates", 0));
            Assert.AreSame(bossHealth, GetArrayObjectReference<CombatHealth>(targetSelector, "targetCandidates", 1));
            Assert.AreSame(player.transform, cameraController.Target);
            Assert.IsTrue(
                cameraController.Threat == bossRoot.transform || cameraController.Threat == closeThreatRoot.transform,
                "Play mode target bridge may focus either the far boss proxy or the current close threat.");
            Assert.AreSame(playerActionController, GetObjectReference<PlayerActionController>(cameraCueDriver, "actionController"));
            Assert.AreSame(player, GetObjectReference<PlayerMovementController>(cameraCueDriver, "movement"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(cameraCueDriver, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(cameraCueDriver, "summonSlot1Action"));
            Assert.AreSame(cameraController, GetObjectReference<ActionCameraController>(cameraCueDriver, "cameraController"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(cameraCueDriver, "cueSpace"));
            Assert.AreSame(emitter, GetObjectReference<BossBarrageEmitter>(bossCameraCueDriver, "bossBarrageEmitter"));
            Assert.AreSame(cameraController, GetObjectReference<ActionCameraController>(bossCameraCueDriver, "cameraController"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(bossCameraCueDriver, "cueSpace"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(pocketOwner, "playerHealth"));
            Assert.AreSame(closeThreatHealth, GetObjectReference<CombatHealth>(pocketOwner, "closeThreatHealth"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(pocketOwner, "energyLadder"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(pocketOwner, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(pocketOwner, "summonSlot1Action"));
            Assert.AreSame(emitter, GetObjectReference<BossBarrageEmitter>(pocketOwner, "bossBarrageEmitter"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(reviewHud, "playerHealth"));
            Assert.AreSame(closeThreatHealth, GetObjectReference<CombatHealth>(reviewHud, "closeThreatHealth"));
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(reviewHud, "bossHealth"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(reviewHud, "energyLadder"));
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(reviewHud, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(reviewHud, "player"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(reviewHud, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(reviewHud, "summonSlot1Action"));
            Assert.AreSame(emitter, GetObjectReference<BossBarrageEmitter>(reviewHud, "bossBarrageEmitter"));
            Assert.AreSame(pocketOwner, GetObjectReference<BossBarragePocketReviewOwner>(reviewHud, "pocketReviewOwner"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator LocalDefenseAttackDamagesCloseThreatWithoutSolvingBoss()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player.gameObject, "player action controller");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(RequireRoot(BossRootName), "boss health");

            player.transform.SetPositionAndRotation(
                closeThreatHealth.transform.position + Vector3.back * 1.25f,
                Quaternion.LookRotation(Vector3.forward, Vector3.up));
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            float closeThreatBefore = closeThreatHealth.CurrentHealth;
            float bossBefore = bossHealth.CurrentHealth;

            playerActionController.QueueBasicAttack();
            float timeout = 0.8f;
            while (timeout > 0f && Mathf.Approximately(closeThreatHealth.CurrentHealth, closeThreatBefore))
            {
                yield return null;
                timeout -= Time.deltaTime;
            }

            Assert.Less(closeThreatHealth.CurrentHealth, closeThreatBefore);
            Assert.AreEqual(bossBefore, bossHealth.CurrentHealth, 0.001f);
        }

        [UnityTest]
        public IEnumerator Skill1SpendsAvailableEnergyAndFiresFromPlayerSide()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");

            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            targetSelector.NotifyTargetContact(bossHealth);
            FillEnergyToTier(energyLadder, 1);
            float bossHealthBeforeSkill = bossHealth.CurrentHealth;

            Assert.IsTrue(skill1Action.TryUseSkill1());
            Assert.AreEqual(1, skill1Action.LastSpentTier);
            Assert.AreEqual(1, skill1Action.LastFiredProjectileCount);
            Assert.AreEqual(0, energyLadder.AvailableTier);
            Assert.IsTrue(
                cameraController.HasActiveCue,
                "Skill1 should request a short camera cue through the existing action camera driver.");
            Assert.Greater(skill1Action.ActiveProjectileCount, 0, "Skill1 should create an immediate readable projectile.");

            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            LaneActionProjectile playerProjectile = null;
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].SourceTeam == DamageTeam.Player
                    && Vector3.Dot(projectiles[i].TravelDirection, Vector3.forward) > 0.5f)
                {
                    playerProjectile = projectiles[i];
                    break;
                }
            }

            Assert.IsNotNull(playerProjectile, "Skill1 should fire forward toward the boss lane.");
            Assert.IsTrue(
                playerProjectile.TryApplyImpact(bossHitCollider, playerProjectile.transform.position),
                "Skill1 should be able to resolve damage against the authored boss hit receiver.");
            Assert.Less(
                bossHealth.CurrentHealth,
                bossHealthBeforeSkill,
                "Skill1 should spend EN into a real boss/proxy health result, not only a visible projectile.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Skill1CanSpendLv2AsIntermediateChoice()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(RequireRoot(BossRootName), "boss health");

            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            targetSelector.NotifyTargetContact(bossHealth);
            FillEnergyToTier(energyLadder, 2);

            Assert.IsTrue(skill1Action.TryUseSkill1());
            Assert.AreEqual(2, skill1Action.LastSpentTier);
            Assert.AreEqual(2, skill1Action.LastFiredProjectileCount);
            Assert.AreEqual(0, energyLadder.AvailableTier);
            Assert.AreEqual(1, energyLadder.ChargingTier);
            Assert.AreEqual(0f, energyLadder.CurrentTierEnergy, 0.001f);
            Assert.GreaterOrEqual(
                skill1Action.ActiveProjectileCount,
                2,
                "Skill1 LV2 should be a visible intermediate spend, not only a numeric damage bump.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SummonSlot1SpendsEnergyAndCanCrossPlayerLaneRails()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();

            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            FillEnergyToTier(energyLadder, 3);

            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            Assert.AreEqual(3, summonSlot1Action.LastSpentTier);
            Assert.AreEqual(3, summonSlot1Action.LastFiredProjectileCount);
            Assert.AreEqual(7, summonSlot1Action.LastPressureScreenMaxIntercepts);
            Assert.AreEqual(0, energyLadder.AvailableTier);
            Assert.IsTrue(
                cameraController.HasActiveCue,
                "SummonSlot1 should request a short camera cue without needing production HUD/UI ownership.");
            Assert.Greater(summonSlot1Action.ActiveCueCount, 0, "SummonSlot1 should show a magic-circle entry cue.");
            Assert.Greater(summonSlot1Action.ActiveSummonActorCount, 0, "SummonSlot1 should show a visible frontline summon actor.");
            Assert.GreaterOrEqual(summonSlot1Action.ActiveProjectileCount, 3);
            Assert.GreaterOrEqual(
                summonSlot1Action.ActivePressureScreenCount,
                1,
                "SummonSlot1 should expose active pressure-screen state for the review HUD.");
            Assert.GreaterOrEqual(
                summonSlot1Action.ActivePressureScreenRemainingIntercepts,
                7,
                "SummonSlot1 LV3 should expose the strongest pressure-screen intercept budget to the review HUD.");
            SummonPressureScreen[] pressureScreens = Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            bool foundActiveScreen = false;
            for (int i = 0; i < pressureScreens.Length; i++)
            {
                if (pressureScreens[i].IsActive && pressureScreens[i].OwnerTeam == DamageTeam.AllySummon)
                {
                    foundActiveScreen = true;
                    Assert.GreaterOrEqual(
                        pressureScreens[i].RemainingIntercepts,
                        7,
                        "SummonSlot1 LV3 should open with the strongest pressure-screen intercept budget.");
                    break;
                }
            }

            Assert.IsTrue(
                foundActiveScreen,
                "SummonSlot1 should create a short-lived summon pressure screen for boss projectile exchanges.");
            SummonPressureScreenPresenter[] pressureScreenPresenters =
                Object.FindObjectsByType<SummonPressureScreenPresenter>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
            bool foundVisibleScreenPresenter = false;
            for (int i = 0; i < pressureScreenPresenters.Length; i++)
            {
                if (pressureScreenPresenters[i].PressureScreen != null
                    && pressureScreenPresenters[i].PressureScreen.IsActive
                    && pressureScreenPresenters[i].IsShowing)
                {
                    foundVisibleScreenPresenter = true;
                    break;
                }
            }

            Assert.IsTrue(
                foundVisibleScreenPresenter,
                "SummonSlot1 pressure screen should be visible immediately when the frontline proxy appears.");
            Assert.IsTrue(
                laneSpace.IsPastForwardBoundary(summonSlot1Action.LastEntryPosition),
                "Summon entry belongs to the forward battlefield, not the clamped player zone.");
            Assert.IsTrue(
                laneSpace.IsPastForwardBoundary(summonSlot1Action.LastSummonActorPosition),
                "Summon actor belongs to the frontline battlefield, not the clamped player zone.");

            SummonFrontlineProxy[] summonActors = Object.FindObjectsByType<SummonFrontlineProxy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            SummonFrontlineProxy activeSummonActor = null;
            for (int i = 0; i < summonActors.Length; i++)
            {
                if (summonActors[i].IsActive)
                {
                    activeSummonActor = summonActors[i];
                    break;
                }
            }

            Assert.IsNotNull(activeSummonActor, "SummonSlot1 should keep an active frontline actor.");
            SummonFrontlineProxyPresenter activeActorPresenter =
                RequireComponent<SummonFrontlineProxyPresenter>(activeSummonActor.gameObject, "active SummonSlot1 actor presenter");
            activeActorPresenter.RefreshNow();
            Assert.IsTrue(activeActorPresenter.IsShowing);
            Assert.AreEqual(3, activeActorPresenter.LastObservedTier);
            Assert.Greater(activeActorPresenter.EntryFlashCount, 0);
            Assert.IsNotNull(activeActorPresenter.PulseRoot);
            float actorStartLaneZ = laneSpace.GetLaneCoordinates(activeSummonActor.transform.position).y;
            Assert.IsTrue(activeSummonActor.IsAdvancing, "SummonSlot1 actor should surge into the frontline after entry.");
            activeSummonActor.Tick(0.24f);
            activeSummonActor.Tick(0.18f);
            activeActorPresenter.RefreshNow();
            Assert.Greater(
                activeActorPresenter.ImpactFlashCount,
                0,
                "SummonSlot1 actor presenter should flash when the frontline proxy reaches impact range.");
            float actorAdvancedLaneZ = laneSpace.GetLaneCoordinates(activeSummonActor.transform.position).y;
            Assert.Greater(
                actorAdvancedLaneZ,
                actorStartLaneZ + 0.5f,
                "SummonSlot1 LV3 actor should visibly advance into the boss/frontline exchange.");
            Assert.IsTrue(
                laneSpace.IsPastForwardBoundary(activeSummonActor.transform.position),
                "Summon actor advance must stay in frontline battlefield coordinates, not player-clamped movement.");

            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].SourceTeam == DamageTeam.AllySummon)
                {
                    projectiles[i].Tick(0.9f);
                }
            }

            bool foundOffLaneSummonProjectile = false;
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].SourceTeam != DamageTeam.AllySummon)
                {
                    continue;
                }

                Vector2 laneCoordinates = laneSpace.GetLaneCoordinates(projectiles[i].transform.position);
                if (Mathf.Abs(laneCoordinates.x) > laneSpace.HalfWidth)
                {
                    foundOffLaneSummonProjectile = true;
                    break;
                }
            }

            Assert.IsTrue(
                foundOffLaneSummonProjectile,
                "SummonSlot1 LV3 should be able to project attacks beyond player lane rails.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SummonSlot1CanSpendLv2AsIntermediateFrontlineChoice()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");

            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            FillEnergyToTier(energyLadder, 2);

            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            Assert.AreEqual(2, summonSlot1Action.LastSpentTier);
            Assert.AreEqual(2, summonSlot1Action.LastFiredProjectileCount);
            Assert.AreEqual(4, summonSlot1Action.LastPressureScreenMaxIntercepts);
            Assert.AreEqual(0, summonSlot1Action.LastPressureScreenInterceptCount);
            Assert.AreEqual(0, energyLadder.AvailableTier);
            Assert.AreEqual(1, energyLadder.ChargingTier);
            Assert.AreEqual(0f, energyLadder.CurrentTierEnergy, 0.001f);
            Assert.Greater(summonSlot1Action.ActiveCueCount, 0);
            Assert.Greater(summonSlot1Action.ActiveSummonActorCount, 0);
            Assert.GreaterOrEqual(summonSlot1Action.ActiveProjectileCount, 2);
            Assert.AreEqual(
                4,
                summonSlot1Action.ActivePressureScreenRemainingIntercepts,
                "SummonSlot1 LV2 should open a real mid-tier projectile screen before the player waits for LV3.");
            Assert.IsTrue(
                laneSpace.IsPastForwardBoundary(summonSlot1Action.LastSummonActorPosition),
                "SummonSlot1 LV2 still belongs to the frontline exchange beyond the player boundary.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SummonPressureScreenCountersInterceptedBossProjectiles()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");

            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            FillEnergyToTier(energyLadder, 3);

            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            SummonPressureScreen activeScreen = null;
            SummonPressureScreen[] pressureScreens = Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < pressureScreens.Length; i++)
            {
                if (pressureScreens[i].IsActive && pressureScreens[i].OwnerTeam == DamageTeam.AllySummon)
                {
                    activeScreen = pressureScreens[i];
                    break;
                }
            }

            Assert.IsNotNull(activeScreen, "SummonSlot1 should open a pressure screen before it can counter boss fire.");
            SummonPressureScreenPresenter activePresenter = null;
            SummonPressureScreenPresenter[] presenters = Object.FindObjectsByType<SummonPressureScreenPresenter>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < presenters.Length; i++)
            {
                if (presenters[i].PressureScreen == activeScreen)
                {
                    activePresenter = presenters[i];
                    break;
                }
            }

            Assert.IsNotNull(activePresenter, "The active summon pressure screen should keep a visible presenter.");
            int summonProjectileCountBeforeIntercept = summonSlot1Action.ActiveProjectileCount;
            HashSet<LaneActionProjectile> activeSummonProjectilesBeforeIntercept = CollectActiveSummonProjectiles();
            float bossHealthBeforeCounter = bossHealth.CurrentHealth;
            int pressureBlockCueCountBeforeIntercept = cameraCueDriver.SummonPressureBlockCueRequestCount;
            int presenterFlashCountBeforeIntercept = activePresenter.InterceptFlashCount;

            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile bossProjectile = null;
            BossBarrageProjectile[] bossProjectiles = Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < bossProjectiles.Length; i++)
            {
                if (bossProjectiles[i].IsActive && bossProjectiles[i].SourceTeam == DamageTeam.Enemy)
                {
                    bossProjectile = bossProjectiles[i];
                    break;
                }
            }

            Assert.IsNotNull(bossProjectile, "The boss barrage emitter should provide a projectile for the summon screen to intercept.");
            Assert.IsTrue(activeScreen.TryIntercept(bossProjectile));
            Assert.IsFalse(bossProjectile.IsActive, "Intercepted boss projectiles should be removed from the lane.");
            Assert.AreEqual(1, summonSlot1Action.LastPressureScreenInterceptCount);
            Assert.AreEqual(1, summonSlot1Action.TotalPressureScreenInterceptCount);
            Assert.AreEqual(3, summonSlot1Action.LastPressureScreenInterceptTier);
            Assert.AreEqual(
                pressureBlockCueCountBeforeIntercept + 1,
                cameraCueDriver.SummonPressureBlockCueRequestCount,
                "A summon pressure-screen intercept should request its own short camera read instead of relying on HUD text.");
            Assert.AreEqual(3, cameraCueDriver.LastSummonPressureBlockTier);
            Assert.IsTrue(cameraController.HasActiveCue);
            Assert.AreEqual(
                presenterFlashCountBeforeIntercept + 1,
                activePresenter.InterceptFlashCount,
                "The summon pressure-screen presenter should flash on the same block that triggers the counter exchange.");
            Assert.Greater(
                summonSlot1Action.ActiveProjectileCount,
                summonProjectileCountBeforeIntercept,
                "A summon pressure-screen intercept should fire a short counter bolt back into the boss lane.");
            LaneActionProjectile counterProjectile = RequireNewActiveSummonProjectile(activeSummonProjectilesBeforeIntercept);
            Assert.IsTrue(
                counterProjectile.TryApplyImpact(bossHitCollider, counterProjectile.transform.position),
                "The counter bolt should be able to resolve damage against the authored boss hit receiver.");
            Assert.Less(
                bossHealth.CurrentHealth,
                bossHealthBeforeCounter,
                "A summon screen counter should move the boss/proxy health state, not only delete incoming pressure.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SummonSlot1PrefersFrontlineTargetWhenCloseThreatIsSelected()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");

            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            closeThreatHealth.transform.position = laneSpace.GetLaneWorldPoint(
                0f,
                laneSpace.BackLimitZ + 0.75f,
                closeThreatHealth.transform.position.y);
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            FillEnergyToTier(energyLadder, 1);
            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            Assert.Less(
                laneSpace.GetLaneCoordinates(closeThreatHealth.transform.position).y,
                laneSpace.SummonEntryZ,
                "The selected close threat is intentionally behind the summon entry so fallback targeting would fire backward.");

            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            bool foundFrontlineSummonProjectile = false;
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].SourceTeam == DamageTeam.AllySummon
                    && Vector3.Dot(projectiles[i].TravelDirection, Vector3.forward) > 0.5f)
                {
                    foundFrontlineSummonProjectile = true;
                    break;
                }
            }

            Assert.IsTrue(
                foundFrontlineSummonProjectile,
                "SummonSlot1 should keep firing into the boss/frontline exchange even when local defense selected a close threat.");
        }

        [UnityTest]
        public IEnumerator ReviewSceneKeepsPlayerBoundedButSummonFieldCanCross()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");

            Vector3 illegalPlayerPoint = laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 4f, laneSpace.BossProxyZ, player.transform.position.y);
            Vector3 clampedPlayerPoint = laneSpace.ClampPlayerPosition(illegalPlayerPoint);
            Vector2 clampedCoordinates = laneSpace.GetLaneCoordinates(clampedPlayerPoint);
            Assert.LessOrEqual(clampedCoordinates.x, laneSpace.HalfWidth + 0.001f);
            Assert.LessOrEqual(clampedCoordinates.y, laneSpace.ForwardBoundaryZ + 0.001f);

            Vector3 summonPoint = laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 4f, laneSpace.SummonEntryZ, 0f);
            Vector2 summonCoordinates = laneSpace.GetLaneCoordinates(summonPoint);
            Assert.Greater(
                summonCoordinates.x,
                laneSpace.HalfWidth,
                "Summon/frontline actors must be able to cross lateral lane rails when their role needs it.");
            Assert.IsTrue(
                laneSpace.IsPastForwardBoundary(summonPoint),
                "Summon/frontline actors must be able to act beyond the player's uncrossable forward boundary.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator PocketClearsAfterCloseThreatDefeatedAndSummonBlocksBossFire()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(RequireRoot(BossRootName), "boss barrage emitter");
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");

            FillEnergyToTier(energyLadder, 1);
            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            closeThreatHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                closeThreatHealth.MaxHealth + 10f,
                closeThreatHealth.transform.position,
                Vector3.forward,
                0f));

            yield return null;

            Assert.IsTrue(
                pocketOwner.IsRunning,
                "The review pocket should not clear just because SummonSlot1 was spent; the summon must answer boss fire.");
            Assert.IsFalse(pocketOwner.BlockedBossPressureWithSummon);

            SummonPressureScreen activeScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile bossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(activeScreen.TryIntercept(bossProjectile));

            yield return null;

            Assert.IsTrue(pocketOwner.IsCleared);
            Assert.IsTrue(pocketOwner.UsedSummonSlot1);
            Assert.IsTrue(pocketOwner.BlockedBossPressureWithSummon);
            Assert.AreEqual(1, pocketOwner.HighestSummonTier);
            Assert.AreEqual(1, pocketOwner.HighestSummonPressureTier);
            float energyAfterClear = energyLadder.CurrentTierEnergy;
            energyLadder.Tick(1f);
            Assert.AreEqual(
                energyAfterClear,
                energyLadder.CurrentTierEnergy,
                0.001f,
                "Pocket clear should stop EN gain so the completed review state does not keep charging behind the result.");
        }

        [UnityTest]
        public IEnumerator PocketFailureStopsEnergyGainAndBossPressure()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(RequireRoot(BossRootName), "boss barrage emitter");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");

            playerHealth.TryApplyDamage(new DamageInfo(
                null,
                DamageTeam.Enemy,
                playerHealth.MaxHealth + 10f,
                player.transform.position,
                Vector3.back,
                0f));

            yield return null;

            Assert.IsTrue(pocketOwner.IsFailed);
            float energyAfterFail = energyLadder.CurrentTierEnergy;
            energyLadder.Tick(1f);
            Assert.AreEqual(
                energyAfterFail,
                energyLadder.CurrentTierEnergy,
                0.001f,
                "Pocket failure should stop EN gain so the failed review state does not keep charging behind the result.");

            emitter.Tick(20f);
            Assert.IsFalse(
                emitter.IsWindupActive,
                "Pocket failure should stop boss barrage progression for a readable fail state.");
            Assert.AreEqual(0, emitter.ActiveProjectileCount);
        }

        [UnityTest]
        public IEnumerator BossBarrageEmitterFiresVisiblePooledProjectilesFromBossSide()
        {
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(RequireRoot(BossRootName), "boss barrage emitter");
            BossBarragePatternProfile pattern = LoadAsset<BossBarragePatternProfile>(PatternProfilePath);
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            BossBarrageCameraCueDriver bossCameraCueDriver =
                RequireComponent<BossBarrageCameraCueDriver>(cameraController.gameObject, "boss barrage camera cue driver");
            BossBarrageLaneTelegraphPresenter telegraphPresenter =
                RequireComponent<BossBarrageLaneTelegraphPresenter>(
                    RequireRoot(BossTelegraphRootName),
                    "boss barrage lane telegraph presenter");

            int windupCueCountBefore = bossCameraCueDriver.WindupCueRequestCount;
            Assert.IsTrue(emitter.BeginWindup());
            telegraphPresenter.RefreshNow();
            Assert.AreEqual(
                windupCueCountBefore + 1,
                bossCameraCueDriver.WindupCueRequestCount,
                "Boss barrage windup should request a short camera cue through the dedicated presentation driver.");
            Assert.AreEqual(pattern.PatternId, telegraphPresenter.LastPatternId);
            Assert.AreEqual(pattern.ProjectilesPerWave, telegraphPresenter.LastPreviewCount);
            Assert.AreEqual(pattern.ProjectilesPerWave, telegraphPresenter.VisibleMarkerCount);
            Assert.GreaterOrEqual(
                telegraphPresenter.WindupRefreshCount,
                1,
                "Boss barrage windup should reveal lane-space target markers before the projectiles fire.");
            Assert.IsTrue(cameraController.HasActiveCue);
            BossBarrageVisualCueDriver cueDriver =
                RequireComponent<BossBarrageVisualCueDriver>(RequireRoot(BossRootName), "boss visual cue driver");
            Assert.IsTrue(cueDriver.IsCueActive, "Boss visual cue driver should react when barrage windup starts.");
            Assert.IsFalse(string.IsNullOrEmpty(cueDriver.LastWindupTrigger));
            int fireCueCountBefore = bossCameraCueDriver.FireCueRequestCount;
            int firedCount = emitter.FirePendingWave();
            telegraphPresenter.RefreshNow();

            Assert.AreEqual(pattern.ProjectilesPerWave, firedCount);
            Assert.AreEqual(pattern.ProjectilesPerWave, emitter.ActiveProjectileCount);
            Assert.AreEqual(pattern.PatternId, telegraphPresenter.LastPatternId);
            Assert.AreEqual(pattern.ProjectilesPerWave, telegraphPresenter.VisibleMarkerCount);
            Assert.GreaterOrEqual(
                telegraphPresenter.ReleaseFlashCount,
                1,
                "Boss barrage release should briefly flash the authored lane markers.");
            Assert.AreEqual(
                fireCueCountBefore + 1,
                bossCameraCueDriver.FireCueRequestCount,
                "Boss barrage release should request a short camera cue without owning boss pattern logic.");
            Assert.IsTrue(cameraController.HasActiveCue);
            Assert.IsFalse(string.IsNullOrEmpty(cueDriver.LastReleaseTrigger));
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from NeedleLock into the center-path CoverFire pattern after one wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from CoverFire into the escort-screen pressure pattern after the second wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from EscortScreen into the layered-salvo barrage pattern after the third wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from LayeredSalvo into the staggered crossfire barrage after the fourth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from StaggeredCrossfire into the twin-column barrage pattern after the fifth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from TwinSweep into the first side-clamp barrage pattern after the sixth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should mirror side-clamp pressure after the seventh wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from mirrored side clamp into the player-centered PunishNet pattern after the eighth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from PunishNet into the reference-backed LinePressure rail pattern after the ninth wave.");

            BossBarrageProjectile[] projectiles = Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            Assert.GreaterOrEqual(projectiles.Length, pattern.ProjectilesPerWave);
            bool foundBossSideProjectile = false;
            for (int i = 0; i < projectiles.Length; i++)
            {
                Vector2 laneCoordinates = laneSpace.GetLaneCoordinates(projectiles[i].transform.position);
                if (laneCoordinates.y > laneSpace.ForwardBoundaryZ)
                {
                    foundBossSideProjectile = true;
                    break;
                }
            }

            Assert.IsTrue(foundBossSideProjectile, "Boss barrage projectiles should spawn from the boss/frontline side.");
            yield return null;
        }

        private static GameObject RequireRoot(string rootName)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == rootName)
                {
                    return roots[i];
                }
            }

            Assert.Fail($"Missing root object {rootName}.");
            return null;
        }

        private static T RequireObject<T>() where T : Component
        {
            T[] found = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.Greater(found.Length, 0, $"Missing required object {typeof(T).Name}.");
            return found[0];
        }

        private static T RequireComponent<T>(GameObject root, string label) where T : Component
        {
            T component = root.GetComponent<T>();
            Assert.IsNotNull(component, $"{label} is missing {typeof(T).Name}.");
            return component;
        }

        private static Collider RequireCombatHitCollider(GameObject root, CombatHealth expectedHealth, string label)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactive: true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].GetComponentInParent<CombatHealth>() == expectedHealth)
                {
                    return colliders[i];
                }
            }

            Assert.Fail($"{label} should expose at least one child collider under its CombatHealth root.");
            return null;
        }

        private static HashSet<LaneActionProjectile> CollectActiveSummonProjectiles()
        {
            var activeProjectiles = new HashSet<LaneActionProjectile>();
            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive && projectiles[i].SourceTeam == DamageTeam.AllySummon)
                {
                    activeProjectiles.Add(projectiles[i]);
                }
            }

            return activeProjectiles;
        }

        private static LaneActionProjectile RequireNewActiveSummonProjectile(
            HashSet<LaneActionProjectile> activeProjectilesBefore)
        {
            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive
                    && projectiles[i].SourceTeam == DamageTeam.AllySummon
                    && !activeProjectilesBefore.Contains(projectiles[i]))
                {
                    return projectiles[i];
                }
            }

            Assert.Fail("Expected a newly active AllySummon projectile after the pressure-screen intercept.");
            return null;
        }

        private static SummonPressureScreen RequireActiveAllyPressureScreen()
        {
            SummonPressureScreen[] pressureScreens = Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < pressureScreens.Length; i++)
            {
                if (pressureScreens[i].IsActive && pressureScreens[i].OwnerTeam == DamageTeam.AllySummon)
                {
                    return pressureScreens[i];
                }
            }

            Assert.Fail("Expected an active AllySummon pressure screen.");
            return null;
        }

        private static BossBarrageProjectile RequireActiveBossProjectile()
        {
            BossBarrageProjectile[] bossProjectiles = Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < bossProjectiles.Length; i++)
            {
                if (bossProjectiles[i].IsActive && bossProjectiles[i].SourceTeam == DamageTeam.Enemy)
                {
                    return bossProjectiles[i];
                }
            }

            Assert.Fail("Expected an active enemy boss barrage projectile.");
            return null;
        }

        private static T LoadAsset<T>(string assetPath) where T : Object
        {
            Assert.IsFalse(assetPath.Contains("/_Imported/"), $"{assetPath} must not point at raw imported assets.");
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Missing required asset {assetPath}.");
            return asset;
        }

        private static void AssertBossHumanoidVisual(GameObject bossRoot)
        {
            Transform visual = bossRoot.transform.Find(BossHumanoidVisualName);
            Assert.IsNotNull(visual, $"Boss proxy should include {BossHumanoidVisualName}.");
            Animator animator = visual.GetComponent<Animator>();
            Assert.IsNotNull(animator, "Boss humanoid visual should use a promoted Animator.");
            Assert.IsNotNull(animator.runtimeAnimatorController, "Boss humanoid visual should keep its promoted Animator Controller.");
            AssertGameOwnedAsset(animator.runtimeAnimatorController, "boss humanoid Animator Controller");

            Assert.IsNull(
                visual.GetComponentInChildren<CombatHealth>(true),
                "Boss humanoid visual should not duplicate CombatHealth; the boss root owns health.");
            Assert.IsNull(
                visual.GetComponentInChildren<BasicSoldierEnemy>(true),
                "Boss humanoid visual should not run enemy AI as a nested visual.");
            Assert.IsNull(
                visual.GetComponentInChildren<CombatTargetSensor>(true),
                "Boss humanoid visual should not carry scene target sensing.");
            Assert.IsNull(
                visual.GetComponentInChildren<EnemyElitePatternController>(true),
                "Boss humanoid visual should not carry elite gameplay traits.");

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            Assert.Greater(renderers.Length, 0, "Boss humanoid visual should expose promoted renderers.");
            for (int i = 0; i < renderers.Length; i++)
            {
                AssertRendererUsesGameOwnedAssets(renderers[i], renderers[i].name);
            }

            Transform projectileCore = bossRoot.transform.Find(BossProjectileCoreName);
            Assert.IsNotNull(projectileCore, "Boss proxy should keep a readable projectile source core.");
            MeshRenderer coreRenderer = projectileCore.GetComponent<MeshRenderer>();
            Assert.IsNotNull(coreRenderer, "Boss projectile source core should be visible.");
            AssertGameOwnedAsset(coreRenderer.sharedMaterial, "boss projectile source material");

            BossBarrageVisualCueDriver cueDriver =
                RequireComponent<BossBarrageVisualCueDriver>(bossRoot, "boss visual cue driver");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            Assert.AreSame(emitter, cueDriver.BossBarrageEmitter);
            Assert.AreSame(animator, cueDriver.Animator);
            Assert.AreSame(projectileCore, cueDriver.PulseRoot);
            Assert.GreaterOrEqual(cueDriver.PatternCueCount, 10);
            AssertBossVisualCueBindings(cueDriver, animator);
            Assert.Greater(cueDriver.PulseRendererCount, 0);
        }

        private static void AssertBossVisualCueBindings(BossBarrageVisualCueDriver cueDriver, Animator animator)
        {
            var foundPatternIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cueDriver.PatternCueCount; i++)
            {
                Assert.IsTrue(cueDriver.TryGetPatternCue(i, out BossBarrageVisualCueDriver.PatternAnimationCue cue));
                Assert.IsFalse(string.IsNullOrWhiteSpace(cue.PatternId), $"Boss pattern cue {i} should have a pattern id.");
                foundPatternIds.Add(cue.PatternId);
                AssertAnimatorTrigger(animator, cue.WindupTrigger, $"{cue.PatternId} windup trigger");
                AssertAnimatorTrigger(animator, cue.ReleaseTrigger, $"{cue.PatternId} release trigger");
            }

            for (int i = 0; i < RequiredBossPatternCueIds.Length; i++)
            {
                Assert.IsTrue(
                    foundPatternIds.Contains(RequiredBossPatternCueIds[i]),
                    $"Boss visual cue driver should map {RequiredBossPatternCueIds[i]}.");
            }
        }

        private static void AssertAnimatorTrigger(Animator animator, string triggerName, string label)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(triggerName), $"Boss visual cue {label} should not be empty.");
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, triggerName, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            Assert.Fail($"Boss visual cue {label} references missing Animator trigger {triggerName}.");
        }

        private static void AssertRendererUsesGameOwnedAssets(Renderer renderer, string label)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                AssertGameOwnedAsset(meshFilter.sharedMesh, $"{label} mesh");
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
            {
                AssertGameOwnedAsset(skinnedMeshRenderer.sharedMesh, $"{label} mesh");
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    AssertGameOwnedAsset(materials[i], $"{label} material");
                }
            }
        }

        private static void AssertEnergyZoneMarker(
            Transform root,
            string markerName,
            SummonLaneSpace laneSpace,
            float startRisk01,
            float endRisk01,
            string materialPath)
        {
            Transform marker = root.Find(markerName);
            Assert.IsNotNull(marker, $"{markerName} should be authored in the review scene.");
            Assert.IsNull(marker.GetComponent<Collider>(), $"{markerName} should be visual-only and not block movement.");

            float startZ = Mathf.Lerp(laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ, startRisk01);
            float endZ = Mathf.Lerp(laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ, endRisk01);
            float expectedCenterZ = (startZ + endZ) * 0.5f;
            float expectedDepth = Mathf.Abs(endZ - startZ);
            Vector2 laneCoordinates = laneSpace.GetLaneCoordinates(marker.position);
            Assert.That(
                laneCoordinates.y,
                Is.EqualTo(expectedCenterZ).Within(0.05f),
                $"{markerName} should sit in the intended EN risk band.");
            Assert.That(
                marker.localScale.z,
                Is.EqualTo(expectedDepth).Within(0.05f),
                $"{markerName} should cover its EN risk band depth.");
            Assert.That(
                marker.localScale.x,
                Is.EqualTo(laneSpace.HalfWidth * 2f).Within(0.05f),
                $"{markerName} should cover the player lane width.");

            Renderer renderer = RequireComponent<Renderer>(marker.gameObject, markerName);
            Material expectedMaterial = LoadAsset<Material>(materialPath);
            Assert.AreSame(expectedMaterial, renderer.sharedMaterial, $"{markerName} should use its authored zone material.");
            AssertGameOwnedAsset(renderer.sharedMaterial, $"{markerName} material");
        }

        private static void AssertGameOwnedAsset(Object asset, string label)
        {
            Assert.IsNotNull(asset, $"{label} should be assigned.");
            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            Assert.IsTrue(assetPath.StartsWith("Assets/_Game/"), $"{label} should be game-owned, found {assetPath}.");
            Assert.IsFalse(assetPath.Contains("/_Imported/"), $"{label} should not point at raw imports.");
        }

        private static void FillEnergyToTier(SummonEnergyLadder energyLadder, int targetTier)
        {
            for (int i = 0; i < 120 && energyLadder.AvailableTier < targetTier; i++)
            {
                energyLadder.Tick(1f);
            }

            Assert.GreaterOrEqual(
                energyLadder.AvailableTier,
                targetTier,
                $"Energy ladder should reach tier {targetTier} during the review test.");
        }

        private static T GetObjectReference<T>(Object target, string propertyName) where T : Object
        {
            Object value = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            Assert.IsInstanceOf<T>(value);
            return (T)value;
        }

        private static T GetArrayObjectReference<T>(Object target, string propertyName, int index) where T : Object
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            Assert.IsTrue(array.isArray, $"{target.name}.{propertyName} must be an array.");
            Assert.Greater(array.arraySize, index, $"{target.name}.{propertyName} should contain index {index}.");
            Object value = array.GetArrayElementAtIndex(index).objectReferenceValue;
            Assert.IsInstanceOf<T>(value);
            return (T)value;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            return property;
        }
    }
}
