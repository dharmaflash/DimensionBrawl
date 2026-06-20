using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
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
        private const string RangedBasicProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab";
        private const string SummonSlot1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";
        private const string SummonSlot1EntryCuePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1EntryCue_MagicCircle.prefab";
        private const string SummonSlot1ActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab";
        private const string BossSummonPressureActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossSummonPressureActor_Proxy.prefab";
        private const string SummonSlot1ActionProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ShieldBreaker.asset";
        private const string BossSummonPressureProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossSummonPressure_SummonCaller.asset";
        private const string BossPressureActionDeckProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossPressureActionDeck_PocketReview.asset";
        private const string SummonOpportunityProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonOpportunity_BossPressureBlock.asset";
        private const string SummonSlot1PresentationCandidateProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonPresentation_PlayerShieldBreaker.asset";
        private const string BossSummonPressurePresentationCandidateProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonPresentation_BossAuraCaptain.asset";
        private const string ShieldBreakerEliteRoleCandidateProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyRoleCandidates/DB_RoleCandidate_ShieldBreakerElite.asset";
        private const string AuraCaptainEliteRoleCandidateProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyRoleCandidates/DB_RoleCandidate_AuraCaptainElite.asset";
        private const string SummonSlot1ActorVisualName = "SummonSlot1Visual_ShieldBreakerElite";
        private const string BossSummonPressureActorVisualName = "BossSummonPressureVisual_AuraCaptainElite";
        private const string SummonActorMoveSpeedParameter = "MoveSpeed";
        private const string SummonActorSpawnTrigger = "EliteSummonPackage";
        private const string SummonActorAttackTrigger = "Attack";
        private const string SummonActorHitTrigger = "Hit";
        private const string SummonActorDeathTrigger = "Death";
        private const string RifleGirlRangedControllerPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/DB_RifleGirl_RangedCandidate.controller";
        private const string RifleGirlModelPath =
            "Assets/_Game/Art/Characters/Player/RifleGirl/Models/Rifle_Full_Body.fbx";
        private const string RifleGirlIdleClipPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Idle.fbx";
        private const string RifleGirlAimIdleClipPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/RG_AimIdle.fbx";
        private const string RifleGirlShootClipPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/RG_Shoot.fbx";
        private const string RifleGirlDrawClipPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/RG_DrawRangedFocus.fbx";
        private const string RifleGirlHolsterClipPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/RG_HolsterRangedFocus.fbx";
        private const string CombatGirlMeleeControllerPath =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";
        private const string LaneRootName = "BossBarrageLaneReview_SummonLaneSpace";
        private const string BossRootName = "BossBarrageLaneReview_BossProxy_NeedleLock";
        private const string BossHumanoidVisualName = "BossBarrageLaneReview_HumanoidBossVisual_SummonCallerElite";
        private const string RangedPlayerVisualRootName = "BossBarrageLaneReview_RangedVisual_RifleGirl";
        private const string RangedPlayerWeaponName = "BossBarrageLaneReview_RangedWeapon_Rifle";
        private const string MeleePlayerWeaponRootName = "BossBarrageLaneReview_MeleeWeapons_CombatGirlSwordShield";
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
        private static readonly BossPressureActionKind[] RequiredBossPressureActionCueKinds =
        {
            BossPressureActionKind.SkillPattern,
            BossPressureActionKind.SummonPressure,
            BossPressureActionKind.PunishOverextend
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
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
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
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossRoot, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
            BossPressureActionDeckProfile bossPressureActionDeck =
                LoadAsset<BossPressureActionDeckProfile>(BossPressureActionDeckProfilePath);
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossRoot, "boss summon pressure action");
            BossSummonPressureProfile bossSummonPressureProfile =
                LoadAsset<BossSummonPressureProfile>(BossSummonPressureProfilePath);
            SummonPresentationCandidateProfile bossSummonPresentationCandidate =
                LoadAsset<SummonPresentationCandidateProfile>(BossSummonPressurePresentationCandidateProfilePath);
            BossPressurePositionController bossPressurePosition =
                RequireComponent<BossPressurePositionController>(bossRoot, "boss pressure position controller");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            SummonSlotActionProfile summonSlot1Profile =
                LoadAsset<SummonSlotActionProfile>(SummonSlot1ActionProfilePath);
            SummonPresentationCandidateProfile summonSlot1PresentationCandidate =
                LoadAsset<SummonPresentationCandidateProfile>(SummonSlot1PresentationCandidateProfilePath);
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarrageCameraCueDriver bossCameraCueDriver =
                RequireComponent<BossBarrageCameraCueDriver>(cameraController.gameObject, "boss barrage camera cue driver");
            GameObject projectileRoot = RequireRoot(ProjectilePoolRootName);
            GameObject actionCueRoot = RequireRoot(ActionCuePoolRootName);
            GameObject summonActorRoot = RequireRoot(SummonActorPoolRootName);
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");
            SummonOpportunityWindowProfile summonOpportunity =
                LoadAsset<SummonOpportunityWindowProfile>(SummonOpportunityProfilePath);
            BossBarragePocketCameraCueBridge pocketCameraCueBridge =
                RequireComponent<BossBarragePocketCameraCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket camera cue bridge");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket VFX cue bridge");
            CombatVfxCuePlayer playerCuePlayer =
                RequireComponent<CombatVfxCuePlayer>(player.gameObject, "player combat VFX cue player");
            PlayerCombatVfxCueDriver playerVfxCueDriver =
                RequireComponent<PlayerCombatVfxCueDriver>(player.gameObject, "player combat VFX cue driver");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "boss barrage HUD");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(HudRootName), "boss barrage mobile HUD");
            BossBarrageLaneTelegraphPresenter telegraphPresenter =
                RequireComponent<BossBarrageLaneTelegraphPresenter>(
                    RequireRoot(BossTelegraphRootName),
                    "boss barrage lane telegraph presenter");
            GameObject energyZoneRoot = RequireRoot(EnergyZoneRootName);

            Assert.AreSame(laneSpace, player.LaneSpace, "Player movement must clamp through the authored lane space.");
            Assert.AreSame(emitter, telegraphPresenter.BossBarrageEmitter);
            Assert.AreSame(laneSpace, telegraphPresenter.LaneSpace);
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(bossPressureCost, "laneSpace"));
            Assert.AreSame(bossRoot.transform, GetObjectReference<Transform>(bossPressureCost, "trackedBoss"));
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(bossPressurePosition, "laneSpace"));
            Assert.AreSame(
                bossPressureCost,
                GetObjectReference<BossPressureCostLadder>(bossPressurePosition, "costLadder"));
            Assert.AreSame(
                bossPressureActionDirector,
                GetObjectReference<BossPressureActionDirector>(bossPressurePosition, "actionDirector"));
            Assert.AreSame(bossRoot.transform, GetObjectReference<Transform>(bossPressurePosition, "movedTransform"));
            Assert.AreEqual(0.12f, GetFloat(bossPressurePosition, "restRisk01"), 0.001f);
            Assert.AreEqual(0.74f, GetFloat(bossPressurePosition, "maxCommitRisk01"), 0.001f);
            Assert.AreEqual(0.38f, GetFloat(bossPressurePosition, "advanceRiskPerSecond"), 0.001f);
            Assert.AreEqual(0.32f, GetFloat(bossPressurePosition, "retreatRiskPerSecond"), 0.001f);
            Assert.IsTrue(GetBool(bossPressurePosition, "returnToRestWhenActionsDisabled"));
            Assert.IsTrue(GetBool(bossPressurePosition, "movementEnabled"));
            Assert.AreSame(
                bossPressureCost,
                GetObjectReference<BossPressureCostLadder>(bossPressureActionDirector, "costLadder"));
            Assert.AreSame(
                emitter,
                GetObjectReference<BossBarrageEmitter>(bossPressureActionDirector, "bossBarrageEmitter"));
            Assert.AreSame(
                bossSummonPressureAction,
                GetObjectReference<BossSummonPressureAction>(bossPressureActionDirector, "summonPressureAction"));
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(bossPressureActionDirector, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(bossPressureActionDirector, "trackedPlayer"));
            Assert.AreSame(bossPressureActionDeck, bossPressureActionDirector.ActionDeckProfile);
            Assert.IsTrue(bossPressureActionDirector.HasActionDeckProfile);
            Assert.IsTrue(
                bossPressureActionDirector.HoldForNextTierActionWhenGateAllows,
                "Boss pressure should be allowed to bank LV1 cost when the next-tier exchange is gated open.");
            Assert.AreEqual("PocketReviewBoss", bossPressureActionDeck.DeckId);
            Assert.AreEqual(4, bossPressureActionDeck.ActionSlotCount);
            Assert.AreEqual(0.35f, bossPressureActionDeck.GlobalRecoverySeconds, 0.001f);
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(bossSummonPressureAction, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(bossSummonPressureAction, "trackedPlayer"));
            Assert.AreSame(
                LoadAsset<GameObject>(BossSummonPressureActorPrefabPath),
                GetObjectReference<GameObject>(bossSummonPressureAction, "summonActorPrefabObject"));
            Assert.AreSame(bossSummonPressureProfile, bossSummonPressureAction.PressureProfile);
            Assert.IsTrue(bossSummonPressureAction.HasPressureProfile);
            Assert.AreEqual("BossSummonPressure.SummonCaller", bossSummonPressureProfile.PressureId);
            Assert.AreEqual(3, bossSummonPressureProfile.TierCount);
            AssertBossSummonPressureReadout(
                bossSummonPressureProfile,
                1,
                "LV1 Escort Probe",
                "Low-cost boss proxy that holds the lane long enough for the player to answer with fire or a saved summon.",
                "Strafe and keep firing; spend SummonSlot1 only if the next barrage overlaps this proxy.",
                "A short relief answer should remove the screen and keep the lane from being locked.");
            AssertBossSummonPressureReadout(
                bossSummonPressureProfile,
                2,
                "LV2 Pressure Screen",
                "Boss-side summon pressure that contests the frontline for several seconds and blocks player follow-up shots.",
                "Take EN only long enough to prepare a clean response, then break the screen before the next boss pattern layers on top.",
                "Use SummonSlot1 or Vanguard support to absorb the curtain and reopen ranged punish time.");
            AssertBossSummonPressureReadout(
                bossSummonPressureProfile,
                3,
                "LV3 Clamp Guard",
                "High-cost boss proxy that punishes overextension and demands a committed high-tier answer or retreat.",
                "Back off from forward-risk lanes unless a summon answer is already charged.",
                "A saved LV2/LV3 summon should create a visible pressure-break window before counterfire.");
            GameObject bossSummonActorPrefabObject = LoadAsset<GameObject>(BossSummonPressureActorPrefabPath);
            SummonFrontlineProxyPresenter bossSummonActorPresenter =
                RequireComponent<SummonFrontlineProxyPresenter>(
                    bossSummonActorPrefabObject,
                    "Boss summon pressure actor presenter");
            Animator bossSummonAnimator = AssertSummonActorRoleVisual(
                bossSummonActorPrefabObject,
                BossSummonPressureActorVisualName);
            AssertSummonProxyAnimatorPresentation(
                bossSummonActorPresenter,
                bossSummonAnimator,
                "Boss summon pressure actor prefab");
            AssertSummonPresentationCandidateProfile(
                bossSummonPresentationCandidate,
                "BossPressure.AuraCaptain",
                SummonPresentationSide.BossPressure,
                bossSummonActorPrefabObject,
                AuraCaptainEliteRoleCandidateProfilePath,
                BossSummonPressureActorVisualName,
                "SciFiSoldier.Elite.AuraCaptain",
                LoadAsset<CombatVfxCueProfile>(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset"));
            Assert.AreEqual(DamageTeam.Enemy, GetEnum<DamageTeam>(bossSummonPressureAction, "ownerTeam"));
            AssertBossPressureActionSlot(
                bossPressureActionDirector,
                0,
                LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath),
                BossPressureActionKind.SkillPattern,
                1,
                "DodgeLineOrUseSkill1",
                "LV1 skill pressure that asks the player to read a committed rail before spending summon resources.",
                "Strafe or dodge out of the rail, then use ranged fire or Skill1 when the lane is clear.",
                "No summon is required; save SummonSlot1 for screen pressure.",
                false,
                0f,
                1f);
            AssertBossPressureActionSlot(
                bossPressureActionDirector,
                1,
                LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath),
                BossPressureActionKind.SummonPressure,
                1,
                "EscortProbeFrontlineCheck",
                "LV1 escort-probe summon pressure that lets the boss contest the frontline before the player reaches a full support stack.",
                "Use ranged fire or a cheap summon answer before the probe turns the lane into a screen trade.",
                "A low-tier summon can body-clash or absorb the probe without waiting for a perfect LV2 answer.",
                false,
                0f,
                1f);
            AssertBossPressureActionSlot(
                bossPressureActionDirector,
                2,
                LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath),
                BossPressureActionKind.SummonPressure,
                2,
                "SummonSlot1PressureBlock",
                "LV2 summon-pressure exchange that tests whether the player can answer boss fire with a frontline summon screen.",
                "Hold forward-risk only long enough to charge EN, then create space for the summon block.",
                "Spend SummonSlot1 to place a pressure screen and intercept the boss curtain.",
                true,
                0.22f,
                1f);
            AssertBossPressureActionSlot(
                bossPressureActionDirector,
                3,
                LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath),
                BossPressureActionKind.PunishOverextend,
                3,
                "RetreatOrSpendHighTierAnswer",
                "LV3 overextend punish that closes gaps when the player stays near the forward boundary too long.",
                "Retreat from forward-risk space or dodge through the shrinking net before firing back.",
                "A prepared high-tier summon screen can buy the follow-up window, but it should cost the player's stored EN.",
                true,
                0.66f,
                1f);
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
            Assert.IsTrue(combatModeController.IsRangedMode, "Review scene should start in the ranged channel.");
            Assert.AreSame(playerActionController, GetObjectReference<PlayerActionController>(combatModeController, "actionController"));
            Assert.AreSame(player, GetObjectReference<PlayerMovementController>(combatModeController, "movementController"));
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(playerActionController, "combatModeController"));
            Assert.IsTrue(GetBool(playerActionController, "blockBasicAttackInRangedMode"));
            GameObject rangedVisualRoot = GetObjectReference<GameObject>(combatModeController, "rangedVisualRoot");
            GameObject meleeVisualRoot = GetObjectReference<GameObject>(combatModeController, "meleeVisualRoot");
            GameObject rangedWeaponRoot = GetObjectReference<GameObject>(combatModeController, "rangedWeaponRoot");
            GameObject meleeWeaponRoot = GetObjectReference<GameObject>(combatModeController, "meleeWeaponRoot");
            Animator rangedAnimator = GetObjectReference<Animator>(combatModeController, "rangedAnimator");
            Animator meleeAnimator = GetObjectReference<Animator>(combatModeController, "meleeAnimator");
            Assert.IsNotNull(rangedVisualRoot, "Review scene must bind a ranged visual root for RifleGirl.");
            Assert.IsNotNull(meleeVisualRoot, "Review scene may keep the old melee source visual inactive for weapon/clip reuse.");
            Assert.IsNotNull(rangedWeaponRoot, "Review scene must bind a ranged weapon root.");
            Assert.IsNotNull(meleeWeaponRoot, "Review scene must bind a melee weapon root.");
            Assert.IsNotNull(rangedAnimator, "Review scene must bind the RifleGirl ranged Animator.");
            Assert.AreSame(rangedAnimator, meleeAnimator, "Weapon swap should reuse one visible player Animator instead of swapping character bodies.");
            Assert.AreEqual(RangedPlayerVisualRootName, rangedVisualRoot.name);
            Assert.IsTrue(rangedVisualRoot.activeSelf, "The single visible player body should stay active for the review starting mode.");
            Assert.IsFalse(meleeVisualRoot.activeSelf, "The old melee source visual should stay inactive; weapon swap must not swap character bodies.");
            Assert.IsTrue(rangedWeaponRoot.activeSelf, "Rifle should start visible in ranged mode.");
            Assert.IsFalse(meleeWeaponRoot.activeSelf, "Melee weapons should start hidden in ranged mode.");
            Assert.AreSame(LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath), rangedAnimator.runtimeAnimatorController);
            Assert.AreSame(
                LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath),
                GetObjectReference<RuntimeAnimatorController>(combatModeController, "rangedAnimatorController"));
            Assert.AreSame(
                LoadAsset<RuntimeAnimatorController>(CombatGirlMeleeControllerPath),
                GetObjectReference<RuntimeAnimatorController>(combatModeController, "meleeAnimatorController"));
            Assert.IsTrue(GetBool(combatModeController, "useSingleCharacterVisual"));
            Assert.IsTrue(GetBool(combatModeController, "rangedAnimatorUsesExternalPresentationBridge"));
            Assert.IsNull(
                GetOptionalObjectReference<Animator>(player, "animator"),
                "Ranged mode uses the RifleGirl native bridge, so generic movement Animator parameters should not route into it.");
            Assert.IsNull(
                GetOptionalObjectReference<Animator>(playerActionController, "animator"),
                "Ranged mode blocks generic basic attacks and should not route CombatGirl attack triggers into the RifleGirl native controller.");
            AssertSingleCharacterWeaponVisual(rangedVisualRoot, rangedAnimator, rangedWeaponRoot, meleeWeaponRoot);
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(rangedAimController, "combatModeController"));
            Assert.AreSame(cameraController, GetObjectReference<ActionCameraController>(rangedAimController, "cameraController"));
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(rangedAimController, "animator"));
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(rangedBasicAttackAction, "combatModeController"));
            Assert.AreSame(rangedAimController, GetObjectReference<PlayerRangedAimController>(rangedBasicAttackAction, "aimController"));
            Assert.AreSame(player, GetObjectReference<PlayerMovementController>(rangedBasicAttackAction, "movement"));
            Assert.AreSame(targetSelector, GetObjectReference<PlayerCombatTargetSelector>(rangedBasicAttackAction, "targetSelector"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(rangedBasicAttackAction, "sourceHealth"));
            Assert.AreSame(cameraController, GetObjectReference<ActionCameraController>(rangedBasicAttackAction, "cameraController"));
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(rangedBasicAttackAction, "animator"));
            Assert.IsTrue(
                string.IsNullOrEmpty(GetString(rangedBasicAttackAction, "fireTrigger")),
                "RifleGirl ranged fire animation should be routed through the native bridge, not a temporary Attack1 trigger.");
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "holdFireActivatesAim"));
            Assert.AreEqual(0.18f, GetFloat(rangedBasicAttackAction, "aimInputDeadZone"), 0.001f);
            Assert.AreEqual(34f, GetFloat(rangedBasicAttackAction, "aimInputYawDegrees"), 0.001f);
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "aimFromCameraViewport"));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "preserveVerticalAim"));
            Assert.AreEqual(26f, GetFloat(rangedBasicAttackAction, "cameraAimFallbackDistance"), 0.001f);
            Assert.AreEqual(0.39f, GetFloat(rangedBasicAttackAction, "aimInputViewportOffsetX"), 0.001f);
            Assert.AreEqual(0.20f, GetFloat(rangedBasicAttackAction, "aimInputViewportOffsetY"), 0.001f);
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "useStableAimOrigin"));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "useAimAssist"));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "disableAimAssistWithManualInput"));
            Assert.IsFalse(GetBool(rangedBasicAttackAction, "requestFacingOnFire"));
            Assert.AreEqual(18f, GetFloat(rangedBasicAttackAction, "aimAssistDistance"), 0.001f);
            Assert.AreEqual(12f, GetFloat(rangedBasicAttackAction, "hipAimAssistAngleDegrees"), 0.001f);
            Assert.AreEqual(7f, GetFloat(rangedBasicAttackAction, "aimedAimAssistAngleDegrees"), 0.001f);
            Assert.AreEqual(8f, GetFloat(rangedBasicAttackAction, "aimAssistMaxTurnDegrees"), 0.001f);
            Assert.AreSame(LoadAsset<GameObject>(RangedBasicProjectilePrefabPath), GetObjectReference<GameObject>(rangedBasicAttackAction, "projectilePrefabObject"));
            Assert.IsTrue(
                LoadAsset<GameObject>(RangedBasicProjectilePrefabPath).GetComponent<LaneActionProjectile>().AllowsVerticalTravel,
                "Player basic ranged fire should preserve vertical aim so look aim can move the reticle up/down.");
            Assert.AreSame(projectileRoot.transform, GetObjectReference<Transform>(rangedBasicAttackAction, "projectileRoot"));
            combatModeController.SetMeleeMode();
            yield return null;
            Assert.IsTrue(combatModeController.IsMeleeMode);
            Assert.IsTrue(rangedVisualRoot.activeSelf, "Melee mode should keep the same character body visible.");
            Assert.IsFalse(meleeVisualRoot.activeSelf, "Melee mode should not reveal the old second character body.");
            Assert.IsFalse(rangedWeaponRoot.activeSelf, "Rifle should hide in melee mode.");
            Assert.IsTrue(meleeWeaponRoot.activeSelf, "Melee weapons should show in melee mode.");
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(player, "animator"));
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(playerActionController, "animator"));
            Assert.AreSame(LoadAsset<RuntimeAnimatorController>(CombatGirlMeleeControllerPath), rangedAnimator.runtimeAnimatorController);
            combatModeController.SetRangedMode();
            yield return null;
            Assert.IsTrue(combatModeController.IsRangedMode);
            Assert.IsTrue(rangedVisualRoot.activeSelf);
            Assert.IsFalse(meleeVisualRoot.activeSelf);
            Assert.IsTrue(rangedWeaponRoot.activeSelf, "Rifle should show again after returning to ranged mode.");
            Assert.IsFalse(meleeWeaponRoot.activeSelf, "Melee weapons should hide again after returning to ranged mode.");
            Assert.IsNull(GetOptionalObjectReference<Animator>(player, "animator"));
            Assert.IsNull(GetOptionalObjectReference<Animator>(playerActionController, "animator"));
            Assert.AreSame(LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath), rangedAnimator.runtimeAnimatorController);
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(energyLadder, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(energyLadder, "trackedPlayer"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(skill1Action, "energyLadder"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(skill1Action, "sourceHealth"));
            Assert.AreSame(targetSelector, GetObjectReference<PlayerCombatTargetSelector>(skill1Action, "targetSelector"));
            Assert.AreSame(LoadAsset<GameObject>(Skill1ProjectilePrefabPath), GetObjectReference<GameObject>(skill1Action, "projectilePrefabObject"));
            Assert.IsFalse(
                LoadAsset<GameObject>(Skill1ProjectilePrefabPath).GetComponent<LaneActionProjectile>().AllowsVerticalTravel,
                "Lane skill projectiles should stay planar until authored as aimed shots.");
            Assert.AreSame(projectileRoot.transform, GetObjectReference<Transform>(skill1Action, "projectileRoot"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(summonSlot1Action, "energyLadder"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(summonSlot1Action, "sourceHealth"));
            Assert.AreSame(targetSelector, GetObjectReference<PlayerCombatTargetSelector>(summonSlot1Action, "targetSelector"));
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(summonSlot1Action, "frontlineTargetHealth"));
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(summonSlot1Action, "laneSpace"));
            Assert.AreSame(LoadAsset<GameObject>(SummonSlot1ProjectilePrefabPath), GetObjectReference<GameObject>(summonSlot1Action, "projectilePrefabObject"));
            Assert.IsFalse(
                LoadAsset<GameObject>(SummonSlot1ProjectilePrefabPath).GetComponent<LaneActionProjectile>().AllowsVerticalTravel,
                "Summon lane counter shots should keep the authored lane plane instead of inheriting player aim elevation.");
            Assert.AreSame(LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath), GetObjectReference<GameObject>(summonSlot1Action, "entryCuePrefab"));
            GameObject summonActorPrefabObject = LoadAsset<GameObject>(SummonSlot1ActorPrefabPath);
            Assert.AreSame(summonActorPrefabObject, GetObjectReference<GameObject>(summonSlot1Action, "summonActorPrefabObject"));
            SummonFrontlineProxy summonActorPrefab =
                RequireComponent<SummonFrontlineProxy>(summonActorPrefabObject, "SummonSlot1 actor prefab");
            SummonPressureScreen summonPressureScreen = summonActorPrefab.PressureScreen;
            Assert.IsNotNull(summonPressureScreen, "SummonSlot1 actor prefab should reference its pressure screen.");
            Assert.AreNotSame(
                summonActorPrefab.transform,
                summonPressureScreen.transform,
                "SummonSlot1 pressure screen should stay separate from the body hitbox.");
            SummonPressureScreenPresenter summonPressureScreenPresenter =
                RequireComponent<SummonPressureScreenPresenter>(summonActorPrefabObject, "SummonSlot1 pressure screen presenter");
            SummonFrontlineProxyPresenter summonActorPresenter =
                RequireComponent<SummonFrontlineProxyPresenter>(summonActorPrefabObject, "SummonSlot1 actor presenter");
            SummonFrontlineHealthBarPresenter summonHealthBarPresenter =
                RequireComponent<SummonFrontlineHealthBarPresenter>(summonActorPrefabObject, "SummonSlot1 health bar presenter");
            Assert.AreSame(summonPressureScreen, summonActorPrefab.PressureScreen);
            Assert.AreSame(summonPressureScreen, summonPressureScreenPresenter.PressureScreen);
            Assert.Greater(summonPressureScreenPresenter.RendererCount, 0);
            Assert.AreSame(summonActorPrefab, summonActorPresenter.Proxy);
            Assert.IsNotNull(summonActorPresenter.PulseRoot);
            Assert.AreEqual(
                1,
                summonActorPresenter.RendererCount,
                "SummonSlot1 actor presenter should tint only the tier pulse, not the promoted summon model.");
            Assert.AreSame(summonActorPrefab, summonHealthBarPresenter.Proxy);
            Assert.IsNotNull(summonHealthBarPresenter.BarRoot);
            Assert.IsNotNull(summonHealthBarPresenter.FillRoot);
            Assert.GreaterOrEqual(summonHealthBarPresenter.RendererCount, 2);
            Animator summonActorAnimator = AssertSummonActorRoleVisual(summonActorPrefabObject, SummonSlot1ActorVisualName);
            AssertSummonProxyAnimatorPresentation(
                summonActorPresenter,
                summonActorAnimator,
                "SummonSlot1 actor prefab");
            AssertSummonPresentationCandidateProfile(
                summonSlot1PresentationCandidate,
                "PlayerSummon.ShieldBreaker",
                SummonPresentationSide.PlayerSummon,
                summonActorPrefabObject,
                ShieldBreakerEliteRoleCandidateProfilePath,
                SummonSlot1ActorVisualName,
                "SciFiSoldier.Elite.ShieldBreaker",
                LoadAsset<CombatVfxCueProfile>(
                    "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset"));
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
            Assert.AreSame(
                bossPressureActionDirector,
                GetObjectReference<BossPressureActionDirector>(bossCameraCueDriver, "bossPressureActionDirector"));
            Assert.AreSame(cameraController, GetObjectReference<ActionCameraController>(bossCameraCueDriver, "cameraController"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(bossCameraCueDriver, "cueSpace"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(pocketOwner, "playerHealth"));
            Assert.AreSame(closeThreatHealth, GetObjectReference<CombatHealth>(pocketOwner, "closeThreatHealth"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(pocketOwner, "energyLadder"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(pocketOwner, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(pocketOwner, "summonSlot1Action"));
            Assert.AreSame(emitter, GetObjectReference<BossBarrageEmitter>(pocketOwner, "bossBarrageEmitter"));
            Assert.AreSame(bossPressureCost, GetObjectReference<BossPressureCostLadder>(pocketOwner, "bossPressureCostLadder"));
            Assert.AreSame(
                bossPressureActionDirector,
                GetObjectReference<BossPressureActionDirector>(pocketOwner, "bossPressureActionDirector"));
            Assert.AreSame(summonOpportunity, pocketOwner.SummonPressureBlockOpportunity);
            Assert.IsTrue(pocketOwner.HasSummonPressureBlockOpportunity);
            Assert.AreEqual("BossPressureBlock", summonOpportunity.WindowId);
            Assert.AreEqual(SummonOpportunityTrigger.CloseThreatCleared, summonOpportunity.Trigger);
            Assert.AreEqual("SummonSlot1", summonOpportunity.PrimaryAnswerAction);
            Assert.AreEqual("Skill1", summonOpportunity.FollowupAction);
            Assert.AreEqual(0.9f, summonOpportunity.OpportunityCueSeconds, 0.001f);
            Assert.AreEqual(2.4f, summonOpportunity.ResolvePressureBreakSeconds(1), 0.001f);
            Assert.AreEqual(3.1f, summonOpportunity.ResolvePressureBreakSeconds(3), 0.001f);
            Assert.AreEqual(1.4f, summonOpportunity.ResolveFollowupWindowSeconds(1), 0.001f);
            Assert.AreEqual(1.85f, summonOpportunity.ResolveFollowupWindowSeconds(3), 0.001f);
            Assert.AreEqual(100f, summonOpportunity.ResolveFollowupEnergyPulse(1), 0.001f);
            Assert.AreEqual(200f, summonOpportunity.ResolveFollowupEnergyPulse(3), 0.001f);
            Assert.AreSame(pocketOwner, pocketCameraCueBridge.PocketReviewOwner);
            Assert.AreSame(cameraCueDriver, pocketCameraCueBridge.CameraCueDriver);
            Assert.AreSame(pocketOwner, pocketVfxCueBridge.PocketReviewOwner);
            Assert.AreSame(playerCuePlayer, pocketVfxCueBridge.CuePlayer);
            Assert.AreSame(
                GetObjectReference<Transform>(playerVfxCueDriver, "attackAnchor"),
                pocketVfxCueBridge.FollowupWindowAnchor);
            Assert.AreSame(bossRoot.transform, pocketVfxCueBridge.FollowupHitAnchor);
            Assert.AreSame(
                GetObjectReference<Transform>(playerVfxCueDriver, "dodgeAnchor"),
                pocketVfxCueBridge.FollowupMissedAnchor);
            Assert.AreSame(bossRoot.transform, pocketVfxCueBridge.DirectionTarget);
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(reviewHud, "playerHealth"));
            Assert.AreSame(closeThreatHealth, GetObjectReference<CombatHealth>(reviewHud, "closeThreatHealth"));
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(reviewHud, "bossHealth"));
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(pocketOwner, "bossHealth"));
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(reviewHud, "energyLadder"));
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(reviewHud, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(reviewHud, "player"));
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(reviewHud, "combatModeController"));
            Assert.AreSame(rangedAimController, GetObjectReference<PlayerRangedAimController>(reviewHud, "rangedAimController"));
            Assert.AreSame(rangedBasicAttackAction, GetObjectReference<PlayerRangedBasicAttackAction>(reviewHud, "rangedBasicAttackAction"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(reviewHud, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(reviewHud, "summonSlot1Action"));
            Assert.AreSame(summonSlot1Profile, summonSlot1Action.SummonActionProfile);
            Assert.IsTrue(summonSlot1Action.HasSummonActionProfile);
            Assert.AreEqual("SummonSlot1.ShieldBreaker", summonSlot1Profile.ActionId);
            Assert.AreEqual(3, summonSlot1Profile.TierCount);
            AssertSummonSlotReadout(
                summonSlot1Profile,
                1,
                "LV1 Guard Entry",
                "Emergency pressure screen for urgent boss fire after close-threat relief.",
                "Spend early when the pocket needs an immediate boss-fire block.",
                "Small ShieldBreaker enters from the player front, advances toward the boss lane, and fires one assist bolt.");
            AssertSummonSlotReadout(
                summonSlot1Profile,
                2,
                "LV2 Frontline Push",
                "Mid-tier exchange that starts converting a successful block into forward damage.",
                "Hold forward-risk long enough for LV2 when the barrage is readable.",
                "Wider screen, four-shot block budget, two assist bolts, and a persistent frontline push.");
            AssertSummonSlotReadout(
                summonSlot1Profile,
                3,
                "LV3 Break Window",
                "High-risk payoff that should visibly win the pressure exchange and open the Skill1 follow-up.",
                "Save for hard boss pressure when retreat alone will not stabilize the pocket.",
                "Large ShieldBreaker screen, seven-shot block budget, three assist bolts, and a committed boss-lane push.");
            Assert.AreSame(emitter, GetObjectReference<BossBarrageEmitter>(reviewHud, "bossBarrageEmitter"));
            Assert.AreSame(
                bossPressureCost,
                GetObjectReference<BossPressureCostLadder>(reviewHud, "bossPressureCostLadder"));
            Assert.AreSame(
                bossPressurePosition,
                GetObjectReference<BossPressurePositionController>(reviewHud, "bossPressurePositionController"));
            Assert.AreSame(
                bossPressureActionDirector,
                GetObjectReference<BossPressureActionDirector>(reviewHud, "bossPressureActionDirector"));
            Assert.AreSame(
                bossSummonPressureAction,
                GetObjectReference<BossSummonPressureAction>(reviewHud, "bossSummonPressureAction"));
            Assert.AreSame(pocketOwner, GetObjectReference<BossBarragePocketReviewOwner>(reviewHud, "pocketReviewOwner"));
            Assert.IsFalse(GetBool(reviewHud, "showCenterReticle"), "Review text HUD should not draw a second center reticle.");
            Assert.AreSame(player, GetObjectReference<PlayerMovementController>(mobileHud, "movement"));
            Assert.AreSame(playerActionController, GetObjectReference<PlayerActionController>(mobileHud, "actionController"));
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(mobileHud, "combatModeController"));
            Assert.AreSame(rangedAimController, GetObjectReference<PlayerRangedAimController>(mobileHud, "aimController"));
            Assert.AreSame(rangedBasicAttackAction, GetObjectReference<PlayerRangedBasicAttackAction>(mobileHud, "rangedBasicAttackAction"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(mobileHud, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(mobileHud, "summonSlot1Action"));
            Assert.AreEqual("Move", GetString(mobileHud, "moveActionName"));
            Assert.AreEqual("BasicDefenseAttack", GetString(mobileHud, "basicDefenseActionName"));
            Assert.AreEqual("Dodge", GetString(mobileHud, "dodgeActionName"));
            Assert.AreEqual("Skill1", GetString(mobileHud, "skill1ActionName"));
            Assert.AreEqual("SummonSlot1", GetString(mobileHud, "summonSlot1ActionName"));
            Assert.AreEqual("RangedAim", GetString(mobileHud, "rangedAimActionName"));
            Assert.AreEqual("WeaponSwap", GetString(mobileHud, "weaponSwapActionName"));
            Assert.IsTrue(GetBool(mobileHud, "screenDragControlsAim"));
            Assert.IsFalse(GetBool(mobileHud, "rightMouseDragControlsAim"));
            Assert.IsTrue(GetBool(mobileHud, "leftMouseDragControlsAim"));
            Assert.IsTrue(GetBool(mobileHud, "fireDragControlsAim"));
            Assert.IsFalse(GetBool(mobileHud, "routeAimToMovementLook"));
            Assert.AreEqual(0.08f, GetFloat(mobileHud, "lookAimDragDeadZone"), 0.001f);
            Assert.AreEqual(230f, GetFloat(mobileHud, "lookAimDragRadius"), 0.001f);
            Assert.AreEqual(30f, GetFloat(mobileHud, "lookAimKnobSize"), 0.001f);
            Assert.AreEqual(0f, GetFloat(mobileHud, "lookAimScreenMinX"), 0.001f);
            Assert.IsTrue(GetBool(mobileHud, "showFireAimReticle"));
            Assert.AreEqual(34f, GetFloat(mobileHud, "fireAimReticleSize"), 0.001f);
            Assert.AreEqual(9f, GetFloat(mobileHud, "fireAimReticleGap"), 0.001f);
            Assert.AreEqual(2f, GetFloat(mobileHud, "fireAimReticleThickness"), 0.001f);

            yield return null;
        }

        [UnityTest]
        public IEnumerator LocalDefenseAttackDamagesCloseThreatWithoutSolvingBoss()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player.gameObject, "player action controller");
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(RequireRoot(BossRootName), "boss health");

            combatModeController.SetMeleeMode();
            Assert.IsTrue(combatModeController.IsMeleeMode, "Local defense melee combo should be tested through the melee channel.");
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
        public IEnumerator RangedBasicFireUsesRangedChannelAndAimCue()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            float bossHealthBefore = bossHealth.CurrentHealth;
            Assert.IsTrue(aimController.IsAiming);
            Assert.IsTrue(rangedBasicAttackAction.TryFire());
            Assert.IsTrue(
                cameraController.HasActiveCue,
                "Ranged basic fire should request a short shoulder-shot camera cue.");
            Assert.Greater(rangedBasicAttackAction.ActiveProjectileCount, 0);

            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Greater(
                Vector3.Dot(playerProjectile.TravelDirection, Vector3.forward),
                0.5f,
                "Ranged basic fire should travel toward the boss/frontline side.");
            Assert.IsTrue(
                playerProjectile.TryApplyImpact(bossHitCollider, playerProjectile.transform.position),
                "Ranged basic projectile should resolve damage against the authored boss hit receiver.");
            Assert.Less(bossHealth.CurrentHealth, bossHealthBefore);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedFireHoldEntersAimPreviewMode()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(false);
            rangedBasicAttackAction.SetFireHeld(true);
            yield return null;

            Assert.IsTrue(rangedBasicAttackAction.IsFireHeld);
            Assert.IsTrue(aimController.IsAiming, "Holding FIRE should enter the ranged aim camera/pose mode.");
            Assert.IsTrue(cameraController.IsAimModifierActive, "Holding FIRE should request the persistent aim camera modifier.");
            Assert.IsTrue(rangedBasicAttackAction.IsAimPreviewActive);
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewViewportPoint(out Vector2 viewportPoint));
            Assert.That(viewportPoint.x, Is.InRange(0.25f, 0.75f));
            Assert.That(viewportPoint.y, Is.InRange(0.25f, 0.75f));

            rangedBasicAttackAction.SetFireHeld(false);
            yield return null;

            Assert.IsFalse(rangedBasicAttackAction.IsFireHeld);
            Assert.IsFalse(aimController.IsAiming);
        }

        [UnityTest]
        public IEnumerator RangedBasicFireHonorsManualAimInputBeforeWeakAssist()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            rangedBasicAttackAction.SetAimInput(Vector2.right);
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewViewportPoint(out Vector2 viewportPoint));
            Assert.Greater(
                viewportPoint.x,
                0.5f,
                "Manual look aim should move the review reticle in the same direction as the shot.");
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.IsTrue(rangedBasicAttackAction.TryFire());
            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, previewDirection),
                0.5f,
                "The actual basic-fire projectile should use the same direction as the aim preview reticle.");
            Assert.Greater(
                Vector3.Dot(playerProjectile.TravelDirection, Vector3.right),
                0.45f,
                "Manual look aim should steer the basic shot before weak aim assist is considered.");
            Assert.Greater(
                Vector3.Dot(playerProjectile.TravelDirection, Vector3.forward),
                0.7f,
                "Manual look aim should stay in the forward lane instead of becoming a side-only shot.");

            rangedBasicAttackAction.ClearAimInput();
            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedBasicFireKeepsManualAimStableWhenMuzzleMoves()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            Transform fireOrigin = GetObjectReference<Transform>(rangedBasicAttackAction, "fireOrigin");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            rangedBasicAttackAction.SetAimInput(new Vector2(0.65f, 0.15f));
            Physics.SyncTransforms();
            yield return null;

            Vector3 originalFireOriginLocalPosition = fireOrigin.localPosition;
            Quaternion originalFireOriginLocalRotation = fireOrigin.localRotation;
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 initialPreviewDirection));

            fireOrigin.localPosition = originalFireOriginLocalPosition + new Vector3(0.35f, -0.16f, 0.22f);
            fireOrigin.localRotation = originalFireOriginLocalRotation * Quaternion.Euler(0f, 22f, -18f);
            Physics.SyncTransforms();

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 shiftedPreviewDirection));
            Assert.Less(
                Vector3.Angle(initialPreviewDirection, shiftedPreviewDirection),
                0.5f,
                "Manual ranged aim should stay stable when the animated muzzle moves between frames.");
            Assert.IsTrue(rangedBasicAttackAction.TryFire());

            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, initialPreviewDirection),
                0.5f,
                "The fired projectile should follow the stable aim direction instead of the current muzzle wobble.");

            fireOrigin.localPosition = originalFireOriginLocalPosition;
            fireOrigin.localRotation = originalFireOriginLocalRotation;
            rangedBasicAttackAction.ClearAimInput();
            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedBasicFireDoesNotRotateStandingPlayerRoot()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            player.transform.SetPositionAndRotation(
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y),
                Quaternion.LookRotation(Vector3.forward, Vector3.up));
            rangedBasicAttackAction.SetAimInput(Vector2.right);
            Physics.SyncTransforms();
            yield return null;

            Quaternion rootRotationBefore = player.transform.rotation;
            Assert.IsTrue(rangedBasicAttackAction.TryFire());
            yield return null;
            yield return null;

            Assert.Less(
                Quaternion.Angle(rootRotationBefore, player.transform.rotation),
                0.5f,
                "Standing basic fire should not rotate the player root; Look/TargetBias owns shot direction.");
            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Greater(
                Vector3.Dot(playerProjectile.TravelDirection, Vector3.right),
                0.45f,
                "Manual aim should still steer the shot even when root facing is stable.");

            rangedBasicAttackAction.ClearAimInput();
            yield return null;
        }

        [UnityTest]
        public IEnumerator MobileHudFireDragAimDoesNotRotateStandingPlayerRoot()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(HudRootName), "mobile HUD");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");

            combatModeController.SetRangedMode();
            player.transform.SetPositionAndRotation(
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y),
                Quaternion.LookRotation(Vector3.forward, Vector3.up));
            Physics.SyncTransforms();
            yield return null;

            Quaternion rootRotationBefore = player.transform.rotation;
            SetPrivateField(mobileHud, "firePointerHeld", true);
            SetPrivateField(mobileHud, "fireAimInput", Vector2.right);
            InvokePrivateMethod(mobileHud, "UpdateHudLookAim");

            Assert.Less(
                GetVector2(player, "mobileLookInput").sqrMagnitude,
                0.0001f,
                "Fire-button drag aim should not be routed into movement look/facing input.");
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.Greater(
                Vector3.Dot(previewDirection, Vector3.right),
                0.45f,
                "Fire-button drag aim should still steer the ranged aim preview.");
            yield return null;
            yield return null;
            Assert.Less(
                Quaternion.Angle(rootRotationBefore, player.transform.rotation),
                0.5f,
                "Fire-button drag aim should not feed movement look/facing and spin the player root.");

            SetPrivateField(mobileHud, "firePointerHeld", false);
            InvokePrivateMethod(mobileHud, "ReleaseHudLookAim");
            yield return null;
        }

        [UnityTest]
        public IEnumerator RifleGirlWeaponSocketIgnoresRepeatedShootSocketEvents()
        {
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(
                    RequireObject<PlayerMovementController>().gameObject,
                    "player combat mode controller");
            Animator rangedAnimator = GetObjectReference<Animator>(combatModeController, "rangedAnimator");
            RifleGirlWeaponSocketDriver socketDriver =
                RequireComponent<RifleGirlWeaponSocketDriver>(rangedAnimator.gameObject, "RifleGirl weapon socket driver");
            ParentConstraint rifleConstraint =
                GetObjectReference<ParentConstraint>(socketDriver, "rifleConstraint");

            Assert.IsTrue(GetBool(socketDriver, "ignoreRedundantSocketCommands"));
            socketDriver.SwitchSocketByString("To_Hand_R_Socket, IK_ON_Left_Handle");
            int applyCountBefore = socketDriver.RifleConstraintSourceApplyCount;
            int redundantCountBefore = socketDriver.RedundantRifleConstraintCommandCount;

            socketDriver.SwitchSocketByString("To_Hand_R_Socket");
            socketDriver.SwitchSocketByString("To_Hand_R_Socket");

            Assert.AreEqual(
                applyCountBefore,
                socketDriver.RifleConstraintSourceApplyCount,
                "Repeated RG_Shoot hand-socket events should not rewrite the same rifle ParentConstraint source.");
            Assert.GreaterOrEqual(
                socketDriver.RedundantRifleConstraintCommandCount,
                redundantCountBefore + 2,
                "Repeated hand-socket commands should be counted as redundant visual events.");
            Assert.AreEqual(0, socketDriver.ActiveRifleConstraintSourceIndex);
            Assert.AreEqual(1f, rifleConstraint.GetSource(0).weight, 0.001f);
            for (int i = 1; i < rifleConstraint.sourceCount; i++)
            {
                Assert.AreEqual(0f, rifleConstraint.GetSource(i).weight, 0.001f);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedBasicFirePreservesVerticalAimInput()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            player.transform.position = laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            rangedBasicAttackAction.SetAimInput(Vector2.up);
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewViewportPoint(out Vector2 viewportPoint));
            Assert.Greater(
                viewportPoint.y,
                0.5f,
                "Dragging look aim upward should move the review reticle upward, not collapse back to a horizontal-only lane.");
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.Greater(
                previewDirection.y,
                0.05f,
                "Camera-viewport aim should preserve vertical shot direction for player basic ranged fire.");
            Assert.IsTrue(rangedBasicAttackAction.TryFire());

            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.IsTrue(playerProjectile.AllowsVerticalTravel);
            Assert.Greater(
                playerProjectile.TravelDirection.y,
                0.05f,
                "The fired player projectile should keep the same upward aim implied by the reticle.");
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, previewDirection),
                0.5f,
                "The actual projectile should match the vertical aim preview direction.");

            rangedBasicAttackAction.ClearAimInput();
            yield return null;
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
            SummonFrontlineHealthBarPresenter activeHealthBarPresenter =
                RequireComponent<SummonFrontlineHealthBarPresenter>(activeSummonActor.gameObject, "active SummonSlot1 health bar presenter");
            activeHealthBarPresenter.RefreshNow();
            Assert.IsTrue(activeHealthBarPresenter.IsShowing);
            Assert.AreEqual(activeSummonActor.HealthRatio, activeHealthBarPresenter.LastHealthRatio, 0.001f);
            float actorStartLaneZ = laneSpace.GetLaneCoordinates(activeSummonActor.transform.position).y;
            Assert.IsTrue(activeSummonActor.IsAdvancing, "SummonSlot1 actor should march into the frontline after entry.");
            activeSummonActor.Tick(0.24f);
            activeSummonActor.Tick(0.18f);
            activeActorPresenter.RefreshNow();
            float actorAdvancedLaneZ = laneSpace.GetLaneCoordinates(activeSummonActor.transform.position).y;
            Assert.Greater(
                actorAdvancedLaneZ,
                actorStartLaneZ + 0.25f,
                "SummonSlot1 LV3 actor should visibly advance into the boss/frontline exchange without snapping.");
            Assert.Less(
                activeSummonActor.AdvanceProgress01,
                0.25f,
                "SummonSlot1 actor should still be marching after the first short review beat, not already at impact range.");
            Assert.AreEqual(
                0,
                activeActorPresenter.ImpactFlashCount,
                "SummonSlot1 actor impact flash should wait until the slow march reaches impact range.");
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
        public IEnumerator CloseThreatDefeatCreatesShortBossPressureRelief()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(RequireRoot(BossRootName), "boss barrage emitter");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket VFX cue bridge");

            Assert.IsTrue(emitter.IsFiringEnabled);
            int summonBlockOpportunityCueCountBefore = pocketVfxCueBridge.SummonBlockOpportunityCueRequestCount;
            int summonBlockOpportunityCameraCueCountBefore = cameraCueDriver.SummonBlockOpportunityCueRequestCount;

            closeThreatHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                closeThreatHealth.MaxHealth + 10f,
                closeThreatHealth.transform.position,
                Vector3.forward,
                0f));
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.CloseThreatDefeated);
            Assert.IsTrue(pocketOwner.IsPressureReliefActive);
            Assert.IsTrue(
                pocketOwner.IsSummonBlockOpportunityCueActive,
                "The close-threat relief beat should be an explicit summon-block opportunity cue state.");
            Assert.IsFalse(
                pocketOwner.IsAwaitingSummonPressureBlock,
                "The pocket should not ask for an actual projectile block while boss barrage is paused for the cue beat.");
            Assert.AreEqual(
                summonBlockOpportunityCueCountBefore + 1,
                pocketVfxCueBridge.SummonBlockOpportunityCueRequestCount,
                "Defeating the close threat should create an in-world summon-block opportunity read before HUD-only follow-up text.");
            Assert.AreEqual(
                summonBlockOpportunityCameraCueCountBefore + 1,
                cameraCueDriver.SummonBlockOpportunityCueRequestCount,
                "Defeating the close threat should also create a short camera read for the summon-block opportunity.");
            Assert.That(
                pocketOwner.PressureReliefRemainingSeconds,
                Is.EqualTo(0.9f).Within(0.001f),
                "The first close-threat relief window should start from the documented 0.9s blocker-break value.");
            Assert.That(
                pocketOwner.SummonBlockOpportunityRemainingSeconds,
                Is.EqualTo(0.9f).Within(0.001f),
                "The summon-block cue timer should expose the same authored relief beat for HUD/readability.");
            Assert.IsFalse(
                emitter.IsFiringEnabled,
                "The review pocket should pause automatic boss barrage briefly after the close threat is defeated.");
            Assert.That(
                pocketOwner.ObjectiveCue,
                Does.Contain("LV1 Guard Entry"),
                "The summon-block opportunity should name the current SummonSlot1 tier readout instead of only saying SummonSlot1.");

            pocketOwner.Tick(0.89f);
            Assert.IsTrue(pocketOwner.IsPressureReliefActive);
            Assert.IsFalse(emitter.IsFiringEnabled);
            Assert.AreEqual(
                summonBlockOpportunityCueCountBefore + 1,
                pocketVfxCueBridge.SummonBlockOpportunityCueRequestCount,
                "The summon-block opportunity cue should fire once for the close-threat defeat beat.");
            Assert.AreEqual(
                summonBlockOpportunityCameraCueCountBefore + 1,
                cameraCueDriver.SummonBlockOpportunityCueRequestCount,
                "The summon-block opportunity camera cue should fire once for the close-threat defeat beat.");

            pocketOwner.Tick(0.02f);
            Assert.IsFalse(pocketOwner.IsPressureReliefActive);
            Assert.IsFalse(pocketOwner.IsSummonBlockOpportunityCueActive);
            Assert.IsTrue(
                pocketOwner.IsAwaitingSummonPressureBlock,
                "After the cue beat ends, the pocket should explicitly wait for SummonSlot1 to block returning boss fire.");
            Assert.AreEqual(0f, pocketOwner.SummonBlockOpportunityRemainingSeconds, 0.001f);
            Assert.IsTrue(
                emitter.IsFiringEnabled,
                "Boss barrage should resume after the short relief beat if the pocket is still running.");
            Assert.That(
                pocketOwner.ObjectiveCue,
                Does.Contain("LV1 Guard Entry"),
                "After the cue beat ends, the pocket objective should still identify the current summon tier answer.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator PocketIgnoresSummonBlocksBeforeCloseThreatDefeat()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket VFX cue bridge");
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");

            FillEnergyToTier(energyLadder, 1);
            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            SummonPressureScreen activeScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile earlyBossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(activeScreen.TryIntercept(earlyBossProjectile));

            yield return null;

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.UsedSummonSlot1);
            Assert.IsFalse(pocketOwner.CloseThreatDefeated);
            Assert.IsFalse(
                pocketOwner.BlockedBossPressureWithSummon,
                "Boss-pressure blocks before the local close threat is defeated should not solve the pocket.");
            Assert.AreEqual(0, pocketOwner.PressureBlocksAfterCloseThreatDefeated);
            Assert.AreEqual(0, pocketOwner.HighestSummonPressureTier);

            closeThreatHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                closeThreatHealth.MaxHealth + 10f,
                closeThreatHealth.transform.position,
                Vector3.forward,
                0f));

            yield return null;

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.CloseThreatDefeated);
            Assert.IsFalse(pocketOwner.BlockedBossPressureWithSummon);
            Assert.AreEqual(0, pocketOwner.PressureBlocksAfterCloseThreatDefeated);

            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile lateBossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(activeScreen.TryIntercept(lateBossProjectile));

            int followupWindowCueCountBefore = cameraCueDriver.SummonFollowupWindowCueRequestCount;
            int followupMissedCueCountBefore = cameraCueDriver.SummonFollowupMissedCueRequestCount;
            int followupWindowVfxCueCountBefore = pocketVfxCueBridge.FollowupWindowCueRequestCount;
            int followupMissedVfxCueCountBefore = pocketVfxCueBridge.FollowupMissedCueRequestCount;
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.BlockedBossPressureWithSummon);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);
            Assert.AreEqual(1, pocketOwner.PressureBlocksAfterCloseThreatDefeated);
            Assert.AreEqual(1, pocketOwner.HighestSummonPressureTier);
            Assert.AreEqual(
                followupWindowCueCountBefore + 1,
                cameraCueDriver.SummonFollowupWindowCueRequestCount,
                "A correct SummonSlot1 block should open a readable follow-up camera cue.");
            Assert.AreEqual(1, cameraCueDriver.LastSummonFollowupWindowTier);
            Assert.AreEqual(
                followupWindowVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupWindowCueRequestCount,
                "A correct SummonSlot1 block should also open an in-world follow-up VFX read, not HUD text only.");
            Assert.AreEqual(1, pocketVfxCueBridge.LastFollowupWindowTier);

            pocketOwner.Tick(1.39f);
            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);

            pocketOwner.Tick(0.02f);
            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsFalse(pocketOwner.IsSummonFollowupWindowActive);
            Assert.AreEqual(
                followupMissedCueCountBefore + 1,
                cameraCueDriver.SummonFollowupMissedCueRequestCount,
                "Letting the follow-up window expire should leave a short missed-response camera read.");
            Assert.AreEqual(
                followupMissedVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupMissedCueRequestCount,
                "Letting the follow-up window expire should leave a short missed-response VFX read.");

            pocketOwner.Tick(1f);
            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsFalse(
                pocketOwner.IsCleared,
                "A summon pressure block should create the follow-up opening, but the pocket should not clear until Skill1 actually hits the boss/proxy.");
            Assert.IsFalse(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsTrue(emitter.IsFiringEnabled);
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("Follow-up missed"));

            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            SummonPressureScreen retryScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile retryBossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(retryScreen.TryIntercept(retryBossProjectile));

            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsTrue(
                pocketOwner.IsSummonFollowupWindowActive,
                "Missing the follow-up should return the player to boss pressure and allow a later SummonSlot1 block to reopen the Skill1 window.");
            Assert.AreEqual(2, pocketOwner.PressureBlocksAfterCloseThreatDefeated);
            Assert.AreEqual(
                followupWindowCueCountBefore + 2,
                cameraCueDriver.SummonFollowupWindowCueRequestCount);
            Assert.AreEqual(
                followupWindowVfxCueCountBefore + 2,
                pocketVfxCueBridge.FollowupWindowCueRequestCount);
        }

        [UnityTest]
        public IEnumerator PocketClearsAfterCloseThreatDefeatedAndSummonBlocksBossFire()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket VFX cue bridge");
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossRoot, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");
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

            int followupWindowCueCountBefore = cameraCueDriver.SummonFollowupWindowCueRequestCount;
            int followupHitCueCountBefore = cameraCueDriver.SummonFollowupHitCueRequestCount;
            int followupWindowVfxCueCountBefore = pocketVfxCueBridge.FollowupWindowCueRequestCount;
            int followupHitVfxCueCountBefore = pocketVfxCueBridge.FollowupHitCueRequestCount;
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.UsedSummonSlot1);
            Assert.IsTrue(pocketOwner.BlockedBossPressureWithSummon);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);
            Assert.That(
                pocketOwner.ObjectiveCue,
                Does.Contain("LV1 Guard Entry"),
                "The summon follow-up objective should preserve which tier of SummonSlot1 created the opening.");
            Assert.That(
                pocketOwner.SummonPressureBreakRemainingSeconds,
                Is.EqualTo(2.4f).Within(0.001f),
                "A correct SummonSlot1 block should open the documented boss-pressure break relief.");
            Assert.That(
                pocketOwner.SummonFollowupWindowRemainingSeconds,
                Is.EqualTo(1.4f).Within(0.001f),
                "The correct block should also expose a short summon follow-up window.");
            Assert.IsFalse(
                emitter.IsFiringEnabled,
                "Boss barrage should pause while the summon pressure-break relief is active.");
            Assert.AreEqual(1, pocketOwner.HighestSummonTier);
            Assert.AreEqual(1, pocketOwner.HighestSummonPressureTier);
            Assert.AreEqual(
                followupWindowCueCountBefore + 1,
                cameraCueDriver.SummonFollowupWindowCueRequestCount,
                "A correct SummonSlot1 block should make the follow-up opportunity readable without relying on HUD text only.");
            Assert.AreEqual(1, cameraCueDriver.LastSummonFollowupWindowTier);
            Assert.AreEqual(
                followupWindowVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupWindowCueRequestCount,
                "A correct SummonSlot1 block should also create an in-world follow-up VFX read.");
            Assert.AreEqual(1, pocketVfxCueBridge.LastFollowupWindowTier);
            Assert.IsTrue(
                pocketOwner.GrantedSummonFollowupEnergy,
                "The summon pressure break should pulse enough EN to make the short follow-up window actionable.");
            Assert.That(
                pocketOwner.SummonFollowupEnergyPulse,
                Is.EqualTo(100f).Within(0.001f),
                "The first follow-up reward should match the documented LV1 review-pulse tuning.");
            Assert.IsTrue(
                energyLadder.CanSpend,
                "After a correct summon block, the EN reward pulse should reopen at least LV1 for a follow-up choice.");
            Assert.AreEqual(1, energyLadder.AvailableTier);

            float bossHealthBeforeFollowup = bossHealth.CurrentHealth;
            Assert.IsTrue(skill1Action.TryUseSkill1());
            Assert.Greater(
                skill1Action.ActiveProjectileCount,
                0,
                "The follow-up Skill1 should create a real lane projectile before the hit is confirmed.");
            LaneActionProjectile followupProjectile = RequireActivePlayerSkillProjectile();
            Assert.IsTrue(
                followupProjectile.TryApplyImpact(bossHitCollider, followupProjectile.transform.position),
                "The follow-up Skill1 should resolve against the authored boss hit receiver during the break window.");
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.UsedSkill1DuringSummonFollowup);
            Assert.AreEqual(1, pocketOwner.HighestSummonFollowupSkillTier);
            Assert.IsTrue(pocketOwner.Skill1FollowupHitConfirmed);
            Assert.AreEqual(1, pocketOwner.HighestSkill1FollowupHitTier);
            Assert.Greater(
                pocketOwner.Skill1FollowupDamage,
                0f,
                "The follow-up response should be confirmed by boss damage, not only by pressing the button.");
            Assert.AreEqual(
                followupHitCueCountBefore + 1,
                cameraCueDriver.SummonFollowupHitCueRequestCount,
                "A confirmed Skill1 boss hit should produce the follow-up hit camera cue.");
            Assert.AreEqual(1, cameraCueDriver.LastSummonFollowupHitTier);
            Assert.Greater(cameraCueDriver.LastSummonFollowupHitDamage, 0f);
            Assert.AreEqual(
                followupHitVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupHitCueRequestCount,
                "A confirmed Skill1 boss hit should also produce a follow-up hit VFX cue.");
            Assert.AreEqual(1, pocketVfxCueBridge.LastFollowupHitTier);
            Assert.Greater(pocketVfxCueBridge.LastFollowupHitDamage, 0f);
            Assert.Less(bossHealth.CurrentHealth, bossHealthBeforeFollowup);

            pocketOwner.Tick(1.39f);
            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);

            pocketOwner.Tick(0.02f);
            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);
            Assert.IsFalse(
                pocketOwner.IsSummonFollowupWindowActive,
                "The follow-up opportunity should end before the longer pressure relief finishes.");

            pocketOwner.Tick(1f);
            Assert.IsTrue(pocketOwner.IsCleared);
            Assert.IsFalse(pocketOwner.IsSummonPressureBreakActive);
            float energyAfterClear = energyLadder.CurrentTierEnergy;
            energyLadder.Tick(1f);
            Assert.AreEqual(
                energyAfterClear,
                energyLadder.CurrentTierEnergy,
                0.001f,
                "Pocket clear should stop EN gain so the completed review state does not keep charging behind the result.");
            float bossCostAfterClear = bossPressureCost.CurrentTierCost;
            bossPressureCost.Tick(1f);
            Assert.AreEqual(
                bossCostAfterClear,
                bossPressureCost.CurrentTierCost,
                0.001f,
                "Pocket clear should stop boss cost gain so the completed review state does not keep charging boss skills.");
            Assert.IsFalse(
                bossPressureActionDirector.ActionsEnabled,
                "Pocket clear should disable boss costed actions so the boss does not queue skills behind the result.");
            int bossActionCountAfterClear = bossPressureActionDirector.TotalActionCount;
            bossPressureCost.GrantCurrentTierCost(300f);
            bossPressureActionDirector.Tick(1f);
            Assert.AreEqual(
                bossActionCountAfterClear,
                bossPressureActionDirector.TotalActionCount,
                "Pocket clear should keep boss costed actions disabled even if boss cost is later granted by a test or designer tool.");
        }

        [UnityTest]
        public IEnumerator PocketLv3SummonBlockCarriesOverflowIntoLv2FollowupChoice()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket VFX cue bridge");
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            Collider bossHitCollider = RequireCombatHitCollider(bossRoot, bossHealth, "boss proxy");
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");

            FillEnergyToTier(energyLadder, 3);
            Assert.IsTrue(summonSlot1Action.TryUseSummonSlot1());
            closeThreatHealth.TryApplyDamage(new DamageInfo(
                playerHealth,
                DamageTeam.Player,
                closeThreatHealth.MaxHealth + 10f,
                closeThreatHealth.transform.position,
                Vector3.forward,
                0f));

            yield return null;

            SummonPressureScreen activeScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile bossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(activeScreen.TryIntercept(bossProjectile));

            int followupWindowCueCountBefore = cameraCueDriver.SummonFollowupWindowCueRequestCount;
            int followupHitCueCountBefore = cameraCueDriver.SummonFollowupHitCueRequestCount;
            int followupWindowVfxCueCountBefore = pocketVfxCueBridge.FollowupWindowCueRequestCount;
            int followupHitVfxCueCountBefore = pocketVfxCueBridge.FollowupHitCueRequestCount;
            pocketOwner.Tick(0f);

            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.SummonFollowup, pocketOwner.CurrentPhase);
            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsTrue(pocketOwner.BlockedBossPressureWithSummon);
            Assert.AreEqual(3, pocketOwner.HighestSummonTier);
            Assert.AreEqual(3, pocketOwner.HighestSummonPressureTier);
            Assert.AreEqual(3, pocketOwner.LastSummonPressureBreakTier);
            Assert.That(pocketOwner.LastSummonPressureBreakDuration, Is.EqualTo(3.1f).Within(0.001f));
            Assert.That(pocketOwner.LastSummonFollowupWindowDuration, Is.EqualTo(1.85f).Within(0.001f));
            Assert.That(pocketOwner.SummonFollowupEnergyPulse, Is.EqualTo(200f).Within(0.001f));
            Assert.AreEqual(
                followupWindowCueCountBefore + 1,
                cameraCueDriver.SummonFollowupWindowCueRequestCount,
                "A LV3 summon block should still open a readable follow-up camera cue.");
            Assert.AreEqual(3, cameraCueDriver.LastSummonFollowupWindowTier);
            Assert.AreEqual(
                followupWindowVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupWindowCueRequestCount,
                "A LV3 summon block should also open the matching in-world follow-up VFX cue.");
            Assert.AreEqual(3, pocketVfxCueBridge.LastFollowupWindowTier);
            Assert.IsTrue(energyLadder.CanSpend);
            Assert.AreEqual(
                2,
                energyLadder.AvailableTier,
                "A LV3 summon block should carry overflow EN far enough to reopen a LV2 follow-up choice.");
            Assert.AreEqual(
                3,
                energyLadder.ChargingTier,
                "After reopening LV2, the EN ladder should keep charging toward LV3 instead of discarding the overflow.");
            Assert.That(
                energyLadder.CurrentTierEnergy,
                Is.InRange(0f, 5f),
                "After reopening LV2, only a tiny amount of fresh recharge should appear before the player spends the follow-up.");

            float bossHealthBeforeFollowup = bossHealth.CurrentHealth;
            Assert.IsTrue(skill1Action.TryUseSkill1());
            LaneActionProjectile followupProjectile = RequireActivePlayerSkillProjectile();
            Assert.IsTrue(
                followupProjectile.TryApplyImpact(bossHitCollider, followupProjectile.transform.position),
                "The reopened LV2 Skill1 follow-up should still land on the authored boss receiver.");
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.UsedSkill1DuringSummonFollowup);
            Assert.AreEqual(2, pocketOwner.HighestSummonFollowupSkillTier);
            Assert.AreEqual(2, pocketOwner.HighestSkill1FollowupHitTier);
            Assert.AreEqual(
                followupHitCueCountBefore + 1,
                cameraCueDriver.SummonFollowupHitCueRequestCount,
                "The upgraded follow-up should still trigger the hit-confirm camera cue.");
            Assert.AreEqual(2, cameraCueDriver.LastSummonFollowupHitTier);
            Assert.AreEqual(
                followupHitVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupHitCueRequestCount,
                "The upgraded follow-up should still trigger the hit-confirm VFX cue.");
            Assert.AreEqual(2, pocketVfxCueBridge.LastFollowupHitTier);
            Assert.Less(bossHealth.CurrentHealth, bossHealthBeforeFollowup);

            pocketOwner.Tick(1.84f);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.SummonFollowup, pocketOwner.CurrentPhase);
            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);

            pocketOwner.Tick(0.02f);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.PressureBreak, pocketOwner.CurrentPhase);
            Assert.IsFalse(pocketOwner.IsSummonFollowupWindowActive);
            Assert.IsTrue(pocketOwner.IsSummonPressureBreakActive);

            pocketOwner.Tick(1.25f);
            Assert.AreEqual(BossBarragePocketReviewOwner.ReviewPhase.Cleared, pocketOwner.CurrentPhase);
            Assert.IsTrue(pocketOwner.IsCleared);
            Assert.IsFalse(pocketOwner.IsSummonPressureBreakActive);
        }

        [UnityTest]
        public IEnumerator PocketFailureStopsEnergyGainAndBossPressure()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossRoot, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
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
            float bossCostAfterFail = bossPressureCost.CurrentTierCost;
            bossPressureCost.Tick(1f);
            Assert.AreEqual(
                bossCostAfterFail,
                bossPressureCost.CurrentTierCost,
                0.001f,
                "Pocket failure should stop boss cost gain so the failed review state does not keep charging boss skills.");
            Assert.IsFalse(
                bossPressureActionDirector.ActionsEnabled,
                "Pocket failure should disable boss costed actions so the boss does not queue skills behind the result.");
            int bossActionCountAfterFail = bossPressureActionDirector.TotalActionCount;
            bossPressureCost.GrantCurrentTierCost(300f);
            bossPressureActionDirector.Tick(1f);
            Assert.AreEqual(
                bossActionCountAfterFail,
                bossPressureActionDirector.TotalActionCount,
                "Pocket failure should keep boss costed actions disabled even if boss cost is later granted by a test or designer tool.");
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

        [UnityTest]
        public IEnumerator BossPressureCostActionTriggersBossVisualCue()
        {
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossRoot, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
            BossBarrageVisualCueDriver cueDriver =
                RequireComponent<BossBarrageVisualCueDriver>(bossRoot, "boss visual cue driver");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            BossBarrageCameraCueDriver cameraCueDriver =
                RequireComponent<BossBarrageCameraCueDriver>(cameraController.gameObject, "boss barrage camera cue driver");

            Assert.AreSame(emitter, cueDriver.BossBarrageEmitter);
            Assert.AreSame(bossPressureActionDirector, cueDriver.BossPressureActionDirector);
            Assert.AreSame(bossPressureActionDirector, cameraCueDriver.BossPressureActionDirector);
            int cueCountBefore = cueDriver.PressureActionCueRequestCount;
            int cameraCueCountBefore = cameraCueDriver.PressureActionCueRequestCount;

            bossPressureCost.GrantCurrentTierCost(300f);
            Assert.IsTrue(
                bossPressureActionDirector.TryQueueBestAvailableAction(),
                "Boss pressure director should be able to spend cost into an authored pressure action during the review.");
            BossBarragePatternProfile queuedPattern = bossPressureActionDirector.LastQueuedPattern;
            BossPressureActionDirector.BossPressureActionSlot queuedSlot =
                bossPressureActionDirector.LastQueuedActionSlot;
            int sequenceIndexBeforePriority = emitter.CurrentPatternSequenceIndex;

            Assert.AreEqual(cueCountBefore + 1, cueDriver.PressureActionCueRequestCount);
            Assert.AreEqual(bossPressureActionDirector.LastActionKind, cueDriver.LastPressureActionKind);
            Assert.AreEqual(bossPressureActionDirector.LastSpentTier, cueDriver.LastPressureActionTier);
            Assert.IsTrue(bossPressureActionDirector.HasLastQueuedActionSlot);
            Assert.AreSame(queuedPattern, queuedSlot.Pattern);
            Assert.IsTrue(queuedSlot.HasResponsePlan);
            Assert.AreEqual(bossPressureActionDirector.LastActionKind, queuedSlot.ActionKind);
            Assert.IsFalse(string.IsNullOrWhiteSpace(queuedSlot.ResponseId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(queuedSlot.StageLoopRole));
            Assert.IsTrue(emitter.CurrentPatternIsPriority);
            Assert.AreSame(queuedPattern, emitter.QueuedPriorityPattern);
            Assert.AreSame(queuedPattern, emitter.CurrentPattern);
            Assert.IsFalse(string.IsNullOrWhiteSpace(cueDriver.LastPressureActionTrigger));
            Assert.IsTrue(
                cueDriver.IsCueActive,
                "Boss costed skill/summon choices should create an in-world visual read, not only HUD text.");
            Assert.AreEqual(cameraCueCountBefore + 1, cameraCueDriver.PressureActionCueRequestCount);
            Assert.AreEqual(bossPressureActionDirector.LastActionKind, cameraCueDriver.LastPressureActionKind);
            Assert.AreEqual(bossPressureActionDirector.LastSpentTier, cameraCueDriver.LastPressureActionTier);
            Assert.IsTrue(
                cameraController.HasActiveCue,
                "Boss costed skill/summon choices should request a short camera read through the presentation driver.");

            Assert.IsTrue(emitter.BeginWindup());
            Assert.IsTrue(emitter.CurrentPatternIsPriority);
            Assert.Greater(emitter.FirePendingWave(), 0);
            Assert.IsTrue(
                emitter.LastFiredWaveWasPriority,
                "Costed boss actions should fire as a priority pattern instead of being indistinguishable from the regular basic sequence.");
            Assert.IsFalse(emitter.CurrentPatternIsPriority);
            Assert.AreEqual(
                sequenceIndexBeforePriority,
                emitter.CurrentPatternSequenceIndex,
                "A costed priority pattern should not advance the regular boss-pressure pattern sequence.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossSummonPressureBlocksPlayerSkillProjectile()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossRoot, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossRoot, "boss summon pressure action");

            float summonPressureRisk01 = 0.5f;
            float summonPressureLaneZ = Mathf.Lerp(
                laneSpace.BackLimitZ,
                laneSpace.ForwardBoundaryZ,
                summonPressureRisk01);
            player.transform.position = laneSpace.GetLaneWorldPoint(
                0f,
                summonPressureLaneZ,
                player.transform.position.y);
            targetSelector.NotifyTargetContact(bossHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            Assert.AreEqual(
                summonPressureRisk01,
                bossPressureActionDirector.CurrentPlayerForwardRisk01,
                0.001f);
            bossPressureCost.GrantCurrentTierCost(200f);
            Assert.IsTrue(
                bossPressureActionDirector.TryQueueBestAvailableAction(),
                "Boss pressure director should spend LV2 cost into the authored summon-pressure action when the player is forward.");
            Assert.AreEqual(BossPressureActionKind.SummonPressure, bossPressureActionDirector.LastActionKind);
            Assert.AreEqual(2, bossPressureActionDirector.LastSpentTier);
            Assert.AreEqual(2, bossSummonPressureAction.LastReleasedTier);

            SummonPressureScreen enemyPressureScreen = RequireActiveEnemyPressureScreen();
            SummonFrontlineProxy enemySummonActor = RequireActiveSummonActorForPressureScreen(enemyPressureScreen);
            Vector2 enemySummonStartLane = laneSpace.GetLaneCoordinates(enemySummonActor.AdvanceStartPosition);
            Vector2 enemySummonTargetLane = laneSpace.GetLaneCoordinates(enemySummonActor.AdvanceTargetPosition);
            Assert.Greater(
                enemySummonStartLane.y,
                laneSpace.ForwardBoundaryZ,
                "Boss summon pressure should enter from the boss/frontline side.");
            Assert.AreEqual(
                summonPressureLaneZ,
                enemySummonTargetLane.y,
                0.001f,
                "Boss summon pressure should target the player's lane depth instead of stopping at the forward boundary.");
            Assert.Less(
                enemySummonTargetLane.y,
                laneSpace.ForwardBoundaryZ - 0.5f,
                "Boss summon pressure should be allowed to cross the player forward boundary.");
            SummonPressureScreenPresenter presenter = RequirePresenterForPressureScreen(enemyPressureScreen);
            int bossPressureInterceptCountBefore = bossSummonPressureAction.LastPressureScreenInterceptCount;
            int bossPressureTotalInterceptCountBefore = bossSummonPressureAction.TotalPressureScreenInterceptCount;
            int presenterFlashCountBefore = presenter.InterceptFlashCount;

            FillEnergyToTier(energyLadder, 1);
            Assert.IsTrue(skill1Action.TryUseSkill1());
            LaneActionProjectile skillProjectile = RequireActivePlayerSkillProjectile();
            Assert.IsTrue(skillProjectile.IsActive);
            Assert.IsTrue(
                enemyPressureScreen.TryIntercept(skillProjectile),
                "Enemy-team boss summon pressure should be able to block hostile player Skill1 projectiles.");

            Assert.IsFalse(skillProjectile.IsActive);
            Assert.AreEqual(
                bossPressureInterceptCountBefore + 1,
                bossSummonPressureAction.LastPressureScreenInterceptCount,
                "Boss summon pressure HUD/test state should count intercepted player/summon lane projectiles.");
            Assert.AreEqual(
                bossPressureTotalInterceptCountBefore + 1,
                bossSummonPressureAction.TotalPressureScreenInterceptCount,
                "Boss summon pressure should expose cumulative intercepts for pocket-state reads.");
            Assert.AreEqual(2, bossSummonPressureAction.LastPressureScreenInterceptTier);
            Assert.AreEqual(
                presenterFlashCountBefore + 1,
                presenter.InterceptFlashCount,
                "Boss summon pressure screen should use the same visible intercept flash path as ally summon screens.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator PocketReadsBossSummonBlockAsFollowupFailure()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "SummonSlot1 action");
            GameObject bossRoot = RequireRoot(BossRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossRoot, "boss barrage emitter");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossRoot, "boss summon pressure action");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            ActionCameraCueDriver cameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            BossBarragePocketVfxCueBridge pocketVfxCueBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(
                    RequireRoot(PocketOwnerRootName),
                    "pocket VFX cue bridge");
            CombatHealth closeThreatHealth =
                RequireComponent<CombatHealth>(RequireRoot(CloseThreatRootName), "close threat health");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(PocketOwnerRootName), "pocket review owner");

            Assert.AreSame(
                bossSummonPressureAction,
                bossPressureActionDirector.SummonPressureAction,
                "Pocket review state should be able to read boss summon pressure through the authored boss pressure director.");

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

            SummonPressureScreen allyScreen = RequireActiveAllyPressureScreen();
            Assert.IsTrue(emitter.BeginWindup());
            Assert.Greater(emitter.FirePendingWave(), 0);
            BossBarrageProjectile bossProjectile = RequireActiveBossProjectile();
            Assert.IsTrue(allyScreen.TryIntercept(bossProjectile));
            pocketOwner.Tick(0f);

            Assert.IsTrue(pocketOwner.IsSummonFollowupWindowActive);
            Assert.IsTrue(energyLadder.CanSpend);

            int bossBlockCountBefore = bossSummonPressureAction.TotalPressureScreenInterceptCount;
            int followupMissedCueCountBefore = cameraCueDriver.SummonFollowupMissedCueRequestCount;
            int followupMissedVfxCueCountBefore = pocketVfxCueBridge.FollowupMissedCueRequestCount;
            Assert.IsTrue(
                bossSummonPressureAction.TryReleasePressureSummon(2),
                "The boss pressure summon actor should be directly releasable as the review's enemy-side guard answer.");
            SummonPressureScreen enemyScreen = RequireActiveEnemyPressureScreen();
            Assert.IsTrue(skill1Action.TryUseSkill1());
            LaneActionProjectile followupProjectile = RequireActivePlayerSkillProjectile();
            Assert.IsTrue(enemyScreen.TryIntercept(followupProjectile));
            pocketOwner.Tick(0f);

            Assert.AreEqual(
                bossBlockCountBefore + 1,
                bossSummonPressureAction.TotalPressureScreenInterceptCount);
            Assert.IsTrue(pocketOwner.UsedSkill1DuringSummonFollowup);
            Assert.IsTrue(
                pocketOwner.BossBlockedSkill1Followup,
                "A boss summon pressure screen blocking the follow-up shot should be a readable pocket state, not a silent generic miss.");
            Assert.AreEqual(1, pocketOwner.BossPressureBlocksDuringSummonFollowup);
            Assert.IsFalse(
                pocketOwner.IsSummonFollowupWindowActive,
                "A blocked follow-up shot should close the current follow-up window immediately.");
            Assert.IsTrue(
                pocketOwner.IsSummonPressureBreakActive,
                "A blocked follow-up shot should close only the response window, not erase the remaining pressure-break pacing.");
            Assert.IsFalse(pocketOwner.Skill1FollowupHitConfirmed);
            Assert.IsFalse(pocketOwner.IsCleared);
            Assert.That(pocketOwner.ObjectiveCue, Does.Contain("Boss screen blocked"));
            Assert.AreEqual(
                followupMissedCueCountBefore + 1,
                cameraCueDriver.SummonFollowupMissedCueRequestCount,
                "A boss screen block should immediately use the existing missed follow-up camera read.");
            Assert.AreEqual(
                followupMissedVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupMissedCueRequestCount,
                "A boss screen block should immediately use the existing missed follow-up VFX read.");

            pocketOwner.Tick(1.45f);
            Assert.IsFalse(pocketOwner.IsSummonFollowupWindowActive);
            Assert.IsTrue(pocketOwner.IsRunning);
            Assert.IsFalse(
                pocketOwner.IsCleared,
                "A blocked follow-up Skill1 should force another summon-pressure answer instead of clearing the review pocket.");
            Assert.AreEqual(
                followupMissedCueCountBefore + 1,
                cameraCueDriver.SummonFollowupMissedCueRequestCount,
                "The later follow-up timeout should not duplicate the already-read boss-screen block cue.");
            Assert.AreEqual(
                followupMissedVfxCueCountBefore + 1,
                pocketVfxCueBridge.FollowupMissedCueRequestCount,
                "The later follow-up timeout should not duplicate the already-read boss-screen block VFX cue.");
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

        private static LaneActionProjectile RequireActivePlayerSkillProjectile()
        {
            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive && projectiles[i].SourceTeam == DamageTeam.Player)
                {
                    return projectiles[i];
                }
            }

            Assert.Fail("Expected an active Player Skill1 projectile.");
            return null;
        }

        private static LaneActionProjectile RequireActivePlayerRangedProjectile()
        {
            LaneActionProjectile[] projectiles = Object.FindObjectsByType<LaneActionProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            GameObject expectedPrefab = LoadAsset<GameObject>(RangedBasicProjectilePrefabPath);
            float expectedRadius = expectedPrefab.transform.localScale.x;

            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].IsActive
                    && projectiles[i].SourceTeam == DamageTeam.Player
                    && Mathf.Abs(projectiles[i].transform.localScale.x - expectedRadius) < 0.001f)
                {
                    return projectiles[i];
                }
            }

            Assert.Fail("Expected an active Player ranged basic projectile.");
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

        private static SummonPressureScreen RequireActiveEnemyPressureScreen()
        {
            SummonPressureScreen[] pressureScreens = Object.FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < pressureScreens.Length; i++)
            {
                if (pressureScreens[i].IsActive && pressureScreens[i].OwnerTeam == DamageTeam.Enemy)
                {
                    return pressureScreens[i];
                }
            }

            Assert.Fail("Expected an active Enemy pressure screen.");
            return null;
        }

        private static SummonFrontlineProxy RequireActiveSummonActorForPressureScreen(SummonPressureScreen pressureScreen)
        {
            SummonFrontlineProxy[] proxies = Object.FindObjectsByType<SummonFrontlineProxy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < proxies.Length; i++)
            {
                if (proxies[i] != null
                    && proxies[i].IsActive
                    && proxies[i].PressureScreen == pressureScreen)
                {
                    return proxies[i];
                }
            }

            Assert.Fail("Expected an active summon actor for the pressure screen.");
            return null;
        }

        private static SummonPressureScreenPresenter RequirePresenterForPressureScreen(SummonPressureScreen pressureScreen)
        {
            SummonPressureScreenPresenter[] presenters = Object.FindObjectsByType<SummonPressureScreenPresenter>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < presenters.Length; i++)
            {
                if (presenters[i].PressureScreen == pressureScreen)
                {
                    return presenters[i];
                }
            }

            Assert.Fail("Expected an active presenter for the pressure screen.");
            return null;
        }

        private static void AssertBossPressureActionSlot(
            BossPressureActionDirector director,
            int index,
            BossBarragePatternProfile expectedPattern,
            BossPressureActionKind expectedKind,
            int expectedMinimumTier,
            string expectedResponseId,
            string expectedStageLoopRole,
            string expectedPlayerAnswer,
            string expectedSummonAnswer,
            bool expectedUsePlayerForwardRiskGate = false,
            float expectedMinimumPlayerForwardRisk01 = 0f,
            float expectedMaximumPlayerForwardRisk01 = 1f)
        {
            Assert.IsTrue(director.TryGetActionSlot(index, out BossPressureActionDirector.BossPressureActionSlot slot));
            Assert.AreSame(expectedPattern, slot.Pattern);
            Assert.AreEqual(expectedKind, slot.ActionKind);
            Assert.AreEqual(expectedMinimumTier, slot.MinimumTier);
            Assert.IsTrue(slot.HasResponsePlan, $"Boss pressure action slot {index} should declare its response plan.");
            Assert.AreEqual(expectedResponseId, slot.ResponseId);
            Assert.AreEqual(expectedStageLoopRole, slot.StageLoopRole);
            Assert.AreEqual(expectedPlayerAnswer, slot.PlayerAnswer);
            Assert.AreEqual(expectedSummonAnswer, slot.SummonAnswer);
            Assert.AreEqual(expectedUsePlayerForwardRiskGate, slot.UsePlayerForwardRiskGate);
            Assert.AreEqual(expectedMinimumPlayerForwardRisk01, slot.MinimumPlayerForwardRisk01, 0.001f);
            Assert.AreEqual(expectedMaximumPlayerForwardRisk01, slot.MaximumPlayerForwardRisk01, 0.001f);
        }

        private static void AssertSummonSlotReadout(
            SummonSlotActionProfile profile,
            int tier,
            string expectedTierLabel,
            string expectedStageRole,
            string expectedPlayerUse,
            string expectedSummonRead)
        {
            Assert.IsTrue(profile.TryGetTierReadout(tier, out SummonSlotActionProfile.SummonTierReadout readout));
            Assert.AreEqual(expectedTierLabel, readout.TierLabel);
            Assert.AreEqual(expectedStageRole, readout.StageRole);
            Assert.AreEqual(expectedPlayerUse, readout.PlayerUse);
            Assert.AreEqual(expectedSummonRead, readout.SummonRead);
        }

        private static void AssertBossSummonPressureReadout(
            BossSummonPressureProfile profile,
            int tier,
            string expectedTierLabel,
            string expectedStageRole,
            string expectedPlayerRead,
            string expectedSummonRead)
        {
            Assert.IsTrue(profile.TryGetTierReadout(tier, out BossSummonPressureProfile.BossSummonTierReadout readout));
            Assert.AreEqual(expectedTierLabel, readout.TierLabel);
            Assert.AreEqual(expectedStageRole, readout.StageRole);
            Assert.AreEqual(expectedPlayerRead, readout.PlayerRead);
            Assert.AreEqual(expectedSummonRead, readout.SummonRead);
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

        private static void AssertImportedClipHasMotion(string assetPath, string clipName)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            Assert.IsNotNull(importer, $"Missing ModelImporter for {assetPath}.");
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            Assert.Greater(clips.Length, 0, $"{clipName} should keep imported clip settings.");
            Assert.AreEqual(clipName, clips[0].name);
            Assert.IsFalse(string.IsNullOrWhiteSpace(clips[0].takeName), $"{clipName} should keep its source take name.");
            Assert.Greater(clips[0].lastFrame, clips[0].firstFrame, $"{clipName} should keep a non-empty frame range.");
        }

        private static void AssertImportedClipHasEvent(
            string assetPath,
            string clipName,
            string functionName,
            string stringParameter)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            Assert.IsNotNull(importer, $"Missing ModelImporter for {assetPath}.");
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            Assert.Greater(clips.Length, 0, $"{clipName} should keep imported clip settings.");
            AnimationEvent[] events = clips[0].events;
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].functionName == functionName && events[i].stringParameter == stringParameter)
                {
                    return;
                }
            }

            Assert.Fail(
                $"{clipName} should preserve animation event {functionName}({stringParameter}) " +
                "from the authored RifleGirl source clip.");
        }

        private static void AssertRifleGirlAvatarUsesAuthoredMapping()
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(RifleGirlModelPath) as ModelImporter;
            Assert.IsNotNull(modelImporter, $"Missing ModelImporter for {RifleGirlModelPath}.");
            Assert.AreEqual(ModelImporterAnimationType.Human, modelImporter.animationType);
            Assert.AreEqual(ModelImporterAvatarSetup.CreateFromThisModel, modelImporter.avatarSetup);
            AssertHumanBoneNotMapped(modelImporter, "hair_side_01_l", "LeftEye");
            AssertHumanBoneNotMapped(modelImporter, "hair_side_01_r", "RightEye");
            AssertHumanBoneNotMapped(modelImporter, "hair_front_01", "Jaw");

            AssertAnimationClipUsesPromotedAvatar(RifleGirlAimIdleClipPath, "RG_AimIdle");
            AssertAnimationClipUsesPromotedAvatar(RifleGirlShootClipPath, "RG_Shoot");
            AssertAnimationClipUsesPromotedAvatar(RifleGirlDrawClipPath, "RG_DrawRangedFocus");
        }

        private static void AssertHumanBoneNotMapped(ModelImporter importer, string boneName, string humanName)
        {
            HumanBone[] humanBones = importer.humanDescription.human;
            for (int i = 0; i < humanBones.Length; i++)
            {
                if (humanBones[i].boneName == boneName && humanBones[i].humanName == humanName)
                {
                    Assert.Fail($"{boneName} must not be auto-mapped as {humanName} on the promoted RifleGirl avatar.");
                }
            }
        }

        private static void AssertAnimationClipUsesPromotedAvatar(string assetPath, string clipName)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            Assert.IsNotNull(importer, $"Missing ModelImporter for {assetPath}.");
            Assert.AreEqual(ModelImporterAnimationType.Human, importer.animationType);
            Assert.AreEqual(
                ModelImporterAvatarSetup.CopyFromOther,
                importer.avatarSetup,
                $"{clipName} should use the promoted RifleGirl Avatar instead of generating a new auto-mapped Avatar.");
            AssertGameOwnedAsset(importer.sourceAvatar, $"{clipName} source Avatar");
        }

        private static void AssertControllerUsesGameOwnedMotions(AnimatorController controller)
        {
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                AssertStateMachineUsesGameOwnedMotions(layers[i].stateMachine, layers[i].name);
            }
        }

        private static void AssertStateMachineUsesGameOwnedMotions(AnimatorStateMachine stateMachine, string label)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AssertMotionIsGameOwned(states[i].state.motion, $"{label}/{states[i].state.name}");
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = 0; i < childMachines.Length; i++)
            {
                AssertStateMachineUsesGameOwnedMotions(
                    childMachines[i].stateMachine,
                    $"{label}/{childMachines[i].stateMachine.name}");
            }
        }

        private static void AssertMotionIsGameOwned(Motion motion, string label)
        {
            if (motion == null)
            {
                return;
            }

            BlendTree blendTree = motion as BlendTree;
            if (blendTree != null)
            {
                ChildMotion[] children = blendTree.children;
                for (int i = 0; i < children.Length; i++)
                {
                    AssertMotionIsGameOwned(children[i].motion, $"{label}/BlendTree[{i}]");
                }
            }

            string assetPath = AssetDatabase.GetAssetPath(motion).Replace('\\', '/');
            Assert.IsTrue(assetPath.StartsWith("Assets/_Game/"), $"{label} motion should be game-owned, found {assetPath}.");
            Assert.IsFalse(assetPath.Contains("/_Imported/"), $"{label} motion should not reference raw imported assets.");
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
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
            Assert.AreSame(emitter, cueDriver.BossBarrageEmitter);
            Assert.AreSame(bossPressureActionDirector, cueDriver.BossPressureActionDirector);
            Assert.AreSame(animator, cueDriver.Animator);
            Assert.AreSame(projectileCore, cueDriver.PulseRoot);
            Assert.GreaterOrEqual(cueDriver.PatternCueCount, 10);
            AssertBossVisualCueBindings(cueDriver, animator);
            Assert.GreaterOrEqual(cueDriver.PressureActionCueCount, RequiredBossPressureActionCueKinds.Length);
            AssertBossPressureActionCueBindings(cueDriver, animator);
            Assert.Greater(cueDriver.PulseRendererCount, 0);
        }

        private static void AssertSingleCharacterWeaponVisual(
            GameObject rangedRoot,
            Animator rangedAnimator,
            GameObject rangedWeaponRoot,
            GameObject meleeWeaponRoot)
        {
            Assert.AreEqual(RangedPlayerVisualRootName, rangedRoot.name);
            Assert.AreSame(
                LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath),
                rangedAnimator.runtimeAnimatorController);
            AssertGameOwnedAsset(rangedAnimator.runtimeAnimatorController, "RifleGirl ranged Animator Controller");
            AssertGameOwnedAsset(rangedAnimator.avatar, "RifleGirl ranged Avatar");
            AnimatorController rangedController = LoadAsset<AnimatorController>(RifleGirlRangedControllerPath);
            Assert.IsTrue(
                rangedController.layers[0].iKPass,
                "RifleGirl ranged controller must keep IK pass enabled for the support hand.");
            Assert.IsNotNull(
                rangedController.layers[0].stateMachine.defaultState,
                "RifleGirl ranged controller should preserve the native controller default state.");
            AssertControllerUsesGameOwnedMotions(rangedController);
            AssertRifleGirlAvatarUsesAuthoredMapping();
            AssertImportedClipHasMotion(RifleGirlIdleClipPath, "RG_Idle");
            AssertImportedClipHasMotion(RifleGirlAimIdleClipPath, "RG_AimIdle");
            AssertImportedClipHasMotion(RifleGirlShootClipPath, "RG_Shoot");
            AssertImportedClipHasEvent(
                RifleGirlAimIdleClipPath,
                "RG_AimIdle",
                "SwitchSocket",
                "To_Hand_R_Socket, IK_ON_Left_Handle");
            AssertImportedClipHasEvent(RifleGirlShootClipPath, "RG_Shoot", "SwitchSocket", "To_Hand_R_Socket");
            AssertImportedClipHasEvent(
                RifleGirlDrawClipPath,
                "RG_DrawRangedFocus",
                "SwitchSocket",
                "To_Hand_R_Socket, IK_OFF_Left_Handle");
            AssertImportedClipHasEvent(
                RifleGirlHolsterClipPath,
                "RG_HolsterRangedFocus",
                "SwitchSocket",
                "To_Put_Socket_Rifle");

            Assert.IsNull(
                rangedRoot.GetComponentInChildren<CombatHealth>(true),
                "Ranged player visual should not duplicate CombatHealth; the player root owns health.");
            Assert.IsNull(
                rangedRoot.GetComponentInChildren<PlayerMovementController>(true),
                "Ranged player visual should not duplicate movement; the player root owns movement.");
            Assert.IsNull(
                rangedRoot.GetComponentInChildren<PlayerActionController>(true),
                "Ranged player visual should not duplicate local defense actions.");

            Renderer[] renderers = rangedRoot.GetComponentsInChildren<Renderer>(true);
            Assert.Greater(renderers.Length, 0, "RifleGirl ranged visual should expose promoted renderers.");
            for (int i = 0; i < renderers.Length; i++)
            {
                AssertRendererUsesGameOwnedAssets(renderers[i], renderers[i].name);
            }

            Transform weapon = FindDescendant(rangedRoot.transform, RangedPlayerWeaponName);
            Assert.IsNotNull(weapon, $"RifleGirl ranged visual should include {RangedPlayerWeaponName}.");
            Assert.AreSame(rangedWeaponRoot.transform, weapon, "Ranged weapon reference should point at the actual rifle object.");
            Assert.AreNotSame(rangedRoot.transform, weapon.parent, "Ranged weapon should stay inside the RifleGirl authored hierarchy.");
            Assert.Greater(
                weapon.GetComponentsInChildren<Renderer>(true).Length,
                0,
                "Ranged weapon should be a visible promoted model, not a hidden data marker.");
            ParentConstraint weaponConstraint = weapon.GetComponent<ParentConstraint>();
            Assert.IsNotNull(weaponConstraint, "Ranged weapon should preserve the authored ParentConstraint from the source prefab.");
            Assert.GreaterOrEqual(
                weaponConstraint.sourceCount,
                2,
                "Ranged weapon ParentConstraint should keep the RifleGirl authored weapon sockets.");
            Assert.IsTrue(weaponConstraint.constraintActive, "Ranged weapon ParentConstraint should start active.");
            RifleGirlWeaponSocketDriver weaponSocketDriver =
                rangedAnimator.GetComponent<RifleGirlWeaponSocketDriver>();
            Assert.IsNotNull(weaponSocketDriver, "Ranged visual should bind a game-owned RifleGirl weapon socket driver.");
            Assert.IsTrue(weaponSocketDriver.IsConfigured, "Ranged visual weapon socket driver should be fully configured.");
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(weaponSocketDriver, "animator"));
            Assert.AreSame(weaponConstraint, GetObjectReference<ParentConstraint>(weaponSocketDriver, "rifleConstraint"));
            Transform leftHandle = FindDescendant(weapon, "Left_Handle");
            Assert.IsNotNull(leftHandle, "Rifle should expose Left_Handle for support-hand IK.");
            Assert.AreSame(leftHandle, GetObjectReference<Transform>(weaponSocketDriver, "leftHandIkTarget"));
            Assert.IsNotNull(
                FindDescendant(rangedRoot.transform, "Hand_R_Socket"),
                "Ranged visual should preserve the right-hand socket.");
            Assert.IsNotNull(
                FindDescendant(rangedRoot.transform, "Put_Socket_Rifle"),
                "Ranged visual should preserve the rifle put-away socket.");
            Assert.IsNotNull(
                FindDescendant(rangedRoot.transform, "R_Weapon_Bone_Dymmy_R"),
                "Ranged visual should preserve the rifle aiming socket.");
            Assert.AreEqual(MeleePlayerWeaponRootName, meleeWeaponRoot.name);
            Assert.Greater(
                meleeWeaponRoot.GetComponentsInChildren<Renderer>(true).Length,
                0,
                "Melee weapon root should contain visible sword/shield renderers.");
            CombatGirlWeaponSocketBinder meleeWeaponBinder =
                meleeWeaponRoot.GetComponent<CombatGirlWeaponSocketBinder>();
            Assert.IsNotNull(meleeWeaponBinder, "Melee weapon root should keep cloned sword/shield objects bound to the same visible hands.");
            Assert.IsTrue(meleeWeaponBinder.AllBindingsValid, "Melee weapon bindings should be valid.");
            AssertAnimatorParameter(rangedAnimator, "IDLE", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "IDLE 0", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "SHOOT", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "AUTO SHOOT", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "RELOAD", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "WALK F", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "WALK B", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "RUN", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "EVADE", AnimatorControllerParameterType.Trigger);
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

        private static void AssertBossPressureActionCueBindings(BossBarrageVisualCueDriver cueDriver, Animator animator)
        {
            var foundActionKinds = new HashSet<BossPressureActionKind>();
            for (int i = 0; i < cueDriver.PressureActionCueCount; i++)
            {
                Assert.IsTrue(cueDriver.TryGetPressureActionCue(i, out BossBarrageVisualCueDriver.PressureActionCue cue));
                foundActionKinds.Add(cue.ActionKind);
                AssertAnimatorTrigger(animator, cue.Trigger, $"{cue.ActionKind} pressure action trigger");
            }

            for (int i = 0; i < RequiredBossPressureActionCueKinds.Length; i++)
            {
                Assert.IsTrue(
                    foundActionKinds.Contains(RequiredBossPressureActionCueKinds[i]),
                    $"Boss visual cue driver should map pressure action {RequiredBossPressureActionCueKinds[i]}.");
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

        private static void AssertAnimatorParameter(
            Animator animator,
            string parameterName,
            AnimatorControllerParameterType expectedType)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type == expectedType
                    && string.Equals(parameters[i].name, parameterName, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            Assert.Fail($"Animator {animator.name} is missing {expectedType} parameter {parameterName}.");
        }

        private static Animator AssertSummonActorRoleVisual(GameObject prefab, string visualName)
        {
            Transform visual = prefab.transform.Find(visualName);
            Assert.IsNotNull(visual, $"{prefab.name} should include {visualName}.");
            Assert.IsNull(
                visual.GetComponentInChildren<CombatHealth>(true),
                $"{visualName} must be visual-only and must not duplicate CombatHealth.");
            Assert.IsNull(
                visual.GetComponentInChildren<BasicSoldierEnemy>(true),
                $"{visualName} must be visual-only and must not duplicate BasicSoldierEnemy.");
            Assert.IsNull(
                visual.GetComponentInChildren<CombatTargetSensor>(true),
                $"{visualName} must be visual-only and must not duplicate CombatTargetSensor.");
            Assert.IsNull(
                visual.GetComponentInChildren<EnemyElitePatternController>(true),
                $"{visualName} must be visual-only and must not duplicate EnemyElitePatternController.");

            Animator animator = visual.GetComponent<Animator>();
            Assert.IsNotNull(animator, $"{visualName} should keep its promoted role Animator.");
            Assert.IsNotNull(animator.runtimeAnimatorController, $"{visualName} should keep an Animator Controller.");
            AssertGameOwnedAsset(animator.runtimeAnimatorController, $"{visualName} Animator Controller");

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            Assert.Greater(renderers.Length, 0, $"{visualName} should expose promoted renderers.");
            for (int i = 0; i < renderers.Length; i++)
            {
                AssertRendererUsesGameOwnedAssets(renderers[i], $"{visualName}.{renderers[i].name}");
            }

            return animator;
        }

        private static void AssertSummonProxyAnimatorPresentation(
            SummonFrontlineProxyPresenter presenter,
            Animator animator,
            string label)
        {
            Assert.IsNotNull(presenter, $"{label} should have a summon proxy presenter.");
            Assert.IsNotNull(animator, $"{label} should have a promoted visual Animator.");
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

        private static void AssertSummonPresentationCandidateProfile(
            SummonPresentationCandidateProfile profile,
            string expectedCandidateId,
            SummonPresentationSide expectedSide,
            GameObject expectedActorPrefab,
            string roleCandidateProfilePath,
            string expectedVisualName,
            string expectedSourceRoleId,
            CombatVfxCueProfile expectedVfxCueProfile)
        {
            CombatEnemyRoleCandidateProfile roleCandidate =
                LoadAsset<CombatEnemyRoleCandidateProfile>(roleCandidateProfilePath);
            Transform visual = expectedActorPrefab.transform.Find(expectedVisualName);
            Assert.IsNotNull(visual, $"{expectedActorPrefab.name} should include {expectedVisualName}.");
            Animator animator = visual.GetComponent<Animator>();
            Assert.IsNotNull(animator, $"{expectedVisualName} should keep an Animator.");

            Assert.AreEqual(expectedCandidateId, profile.CandidateId);
            Assert.AreEqual(expectedSide, profile.Side);
            Assert.AreSame(expectedActorPrefab, profile.ActorPrefab);
            Assert.AreSame(roleCandidate.PromotedVisualSource, profile.VisualSourceAsset);
            Assert.AreEqual(expectedVisualName, profile.VisualChildName);
            Assert.AreEqual(expectedSourceRoleId, profile.SourceRoleId);
            Assert.AreSame(animator.runtimeAnimatorController, profile.AnimatorController);
            Assert.AreSame(expectedVfxCueProfile, profile.VfxCueProfile);
            Assert.IsNotEmpty(profile.DisplayName);
            Assert.IsNotEmpty(profile.AnimationRead);
            Assert.IsNotEmpty(profile.VfxRead);
            Assert.IsNotEmpty(profile.ReplacementPlan);
            Assert.IsNotEmpty(profile.OwnershipNotes);
            AssertGameOwnedAsset(profile, $"{profile.name} asset");
            AssertGameOwnedAsset(profile.ActorPrefab, $"{profile.name} actor prefab");
            AssertGameOwnedAsset(profile.VisualSourceAsset, $"{profile.name} visual source");
            AssertGameOwnedAsset(profile.AnimatorController, $"{profile.name} Animator controller");
            AssertGameOwnedAsset(profile.VfxCueProfile, $"{profile.name} VFX cue profile");
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (string.Equals(children[i].name, childName, System.StringComparison.Ordinal))
                {
                    return children[i];
                }
            }

            return null;
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
                    AssertRenderableMaterialShader(materials[i], $"{label} material shader");
                }
            }
        }

        private static void AssertRenderableMaterialShader(Material material, string label)
        {
            Assert.IsNotNull(material.shader, $"{label} should be assigned.");
            Assert.AreNotEqual(
                "Hidden/InternalErrorShader",
                material.shader.name,
                $"{label} should not use Unity's missing/error shader.");
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

        private static T GetOptionalObjectReference<T>(Object target, string propertyName) where T : Object
        {
            Object value = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (value == null)
            {
                return null;
            }

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

        private static bool GetBool(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).boolValue;
        }

        private static Vector2 GetVector2(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).vector2Value;
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = RequirePrivateField(target, fieldName);
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"{target.GetType().Name} should define private method {methodName}.");
            method.Invoke(target, null);
        }

        private static FieldInfo RequirePrivateField(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"{target.GetType().Name} should define private field {fieldName}.");
            return field;
        }

        private static float GetFloat(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).floatValue;
        }

        private static T GetEnum<T>(Object target, string propertyName) where T : System.Enum
        {
            int value = RequireProperty(new SerializedObject(target), propertyName).enumValueIndex;
            return (T)System.Enum.ToObject(typeof(T), value);
        }

        private static string GetString(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).stringValue;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, $"{serializedObject.targetObject.name} is missing serialized property {propertyName}.");
            return property;
        }
    }
}
