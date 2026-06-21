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
        private const string BossBasicFireProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBasicFire_LanePoke.asset";
        private const string BossBasicFireProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBasicFireProjectile.mat";
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
        private const string SummonSlot2ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot2Projectile_MarksmanBolt.prefab";
        private const string SummonSlot3ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot3Projectile_VanguardBolt.prefab";
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
        private const int InputSystemKeyE = 19;
        private const int InputSystemKeyQ = 31;
        private const string RifleGirlRangedControllerPath =
            "Assets/_Game/Art/Animations/Player/RifleGirl/DB_RifleGirl_RangedCandidate.controller";
        private const string InoriRifleAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Player/Inori/DB_Inori_Rifle_ActionFoundation.controller";
        private const string RifleGirlModelPath =
            "Assets/_Game/Art/Characters/Player/RifleGirl/Models/Rifle_Full_Body.fbx";
        private const string InoriModelPath =
            "Assets/_Game/Art/Characters/Player/Inori/Models/Inori_Unity.fbx";
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
        private const string AmbientVfxRootName = "BossBarrageLaneReview_AmbientVfx";
        private const string HudRootName = "BossBarrageLaneReview_DebugHud";
        private const string BacklineEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageBacklineEnergyZone.mat";
        private const string MidEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageMidEnergyZone.mat";
        private const string ForwardEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageForwardEnergyZone.mat";
        private const string LaneAmbientFlowMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageLaneAmbientFlow.mat";
        private const string BossPressureHorizonMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarragePressureHorizon.mat";
        private const string SummonRouteWispMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageSummonRouteWisp.mat";
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
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossRoot, "boss basic fire emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossRoot, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossRoot, "boss pressure action director");
            BossPressureActionDeckProfile bossPressureActionDeck =
                LoadAsset<BossPressureActionDeckProfile>(BossPressureActionDeckProfilePath);
            BossBasicFireProfile bossBasicFireProfile = LoadAsset<BossBasicFireProfile>(BossBasicFireProfilePath);
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
            PlayerRangedBasicVfxCueDriver rangedBasicVfxCueDriver =
                RequireComponent<PlayerRangedBasicVfxCueDriver>(player.gameObject, "player ranged basic VFX cue driver");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(HudRootName), "boss barrage HUD");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(HudRootName), "boss barrage mobile HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(RequireRoot(HudRootName), "action screen cue presenter");
            BossBarrageLaneTelegraphPresenter telegraphPresenter =
                RequireComponent<BossBarrageLaneTelegraphPresenter>(
                    RequireRoot(BossTelegraphRootName),
                    "boss barrage lane telegraph presenter");
            GameObject energyZoneRoot = RequireRoot(EnergyZoneRootName);

            Assert.AreSame(laneSpace, player.LaneSpace, "Player movement must clamp through the authored lane space.");
            Assert.AreSame(emitter, telegraphPresenter.BossBarrageEmitter);
            Assert.AreSame(laneSpace, telegraphPresenter.LaneSpace);
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(bossBasicFireEmitter, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(bossBasicFireEmitter, "trackedPlayer"));
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(bossBasicFireEmitter, "sourceHealth"));
            Assert.AreSame(rangedBasicAttackAction, GetObjectReference<PlayerRangedBasicAttackAction>(rangedBasicVfxCueDriver, "rangedBasicAttackAction"));
            Assert.AreSame(playerCuePlayer, GetObjectReference<CombatVfxCuePlayer>(rangedBasicVfxCueDriver, "cuePlayer"));
            Assert.AreSame(
                GetObjectReference<Transform>(rangedBasicAttackAction, "fireOrigin"),
                GetObjectReference<Transform>(rangedBasicVfxCueDriver, "muzzleAnchor"));
            Assert.AreEqual(CombatVfxCueId.PlayerRangedMuzzleFlash, GetEnum<CombatVfxCueId>(rangedBasicVfxCueDriver, "muzzleFlashCueId"));
            Assert.AreEqual(1f, GetFloat(rangedBasicVfxCueDriver, "muzzleFlashIntensity"), 0.001f);
            Assert.AreEqual(CombatVfxCueId.PlayerRangedProjectileImpact, GetEnum<CombatVfxCueId>(rangedBasicVfxCueDriver, "impactCueId"));
            Assert.AreEqual(1f, GetFloat(rangedBasicVfxCueDriver, "impactIntensity"), 0.001f);
            AssertBossBarrageCombatCueAssetOverlays();
            Assert.AreSame(bossBasicFireProfile, GetObjectReference<BossBasicFireProfile>(bossBasicFireEmitter, "fireProfile"));
            Assert.AreSame(
                LoadAsset<GameObject>(ProjectilePrefabPath),
                GetObjectReference<GameObject>(bossBasicFireEmitter, "projectilePrefabObject"));
            Assert.AreSame(projectileRoot.transform, GetObjectReference<Transform>(bossBasicFireEmitter, "projectileRoot"));
            Assert.AreEqual(DamageTeam.Enemy, GetEnum<DamageTeam>(bossBasicFireEmitter, "sourceTeam"));
            Assert.IsTrue(GetBool(bossBasicFireEmitter, "firingEnabled"));
            Assert.AreEqual(10, GetInt(bossBasicFireEmitter, "prewarmCount"));
            Assert.AreEqual("LanePoke", bossBasicFireProfile.FireId);
            Assert.AreEqual("Lane Poke", bossBasicFireProfile.ReadoutLabel);
            Assert.AreEqual(2, bossBasicFireProfile.ProjectilesPerVolley);
            Assert.AreEqual(5f, bossBasicFireProfile.Damage, 0.001f);
            Assert.AreEqual(2.2f, bossBasicFireProfile.FireIntervalSeconds, 0.001f);
            Assert.AreEqual(0.22f, bossBasicFireProfile.ProjectileRadius, 0.001f);
            Assert.AreSame(LoadAsset<Material>(BossBasicFireProjectileMaterialPath), bossBasicFireProfile.ProjectileMaterial);
            Assert.Greater(
                bossBasicFireProfile.GetLateralOffset(1, 2, 0f),
                bossBasicFireProfile.GetLateralOffset(1, 2, 1f),
                "Boss basic fire should keep the same front/back risk grammar: safer backline gaps, tighter forward pressure.");
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
            AssertSummonActorVfx(
                bossSummonActorPrefabObject,
                expectPressureScreen: true,
                label: "Boss summon pressure actor prefab");
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
                1f,
                true,
                1);
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
            AssertLaneAmbientVfx(RequireRoot(AmbientVfxRootName));
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
            Assert.IsNotNull(rangedVisualRoot, "Review scene must bind a ranged RifleGirl visual root.");
            Assert.IsNotNull(meleeVisualRoot, "Review scene must keep the CombatGirl melee source visual for fallback mode switching.");
            Assert.IsNotNull(rangedWeaponRoot, "Review scene must bind a ranged weapon root.");
            Assert.IsNotNull(meleeWeaponRoot, "Review scene must bind a melee visual root reference.");
            Assert.IsNotNull(rangedAnimator, "Review scene must bind the RifleGirl ranged Animator.");
            Assert.AreNotSame(rangedAnimator, meleeAnimator, "Fallback presentation should use separate RifleGirl and CombatGirl Animators.");
            Assert.AreEqual(RangedPlayerVisualRootName, rangedVisualRoot.name);
            Assert.IsTrue(rangedVisualRoot.activeSelf, "RifleGirl ranged body should be active for the review starting mode.");
            Assert.IsFalse(meleeVisualRoot.activeSelf, "CombatGirl melee body should stay inactive while the review starts in ranged mode.");
            Assert.IsTrue(rangedWeaponRoot.activeSelf, "Rifle should start visible in ranged mode.");
            Assert.IsFalse(meleeWeaponRoot.activeSelf, "Melee visual should start hidden in ranged mode.");
            Assert.AreSame(LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath), rangedAnimator.runtimeAnimatorController);
            Assert.AreSame(
                LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath),
                GetObjectReference<RuntimeAnimatorController>(combatModeController, "rangedAnimatorController"));
            Assert.AreSame(
                LoadAsset<RuntimeAnimatorController>(CombatGirlMeleeControllerPath),
                GetObjectReference<RuntimeAnimatorController>(combatModeController, "meleeAnimatorController"));
            Assert.IsFalse(GetBool(combatModeController, "useSingleCharacterVisual"));
            Assert.IsTrue(GetBool(combatModeController, "rangedAnimatorUsesExternalPresentationBridge"));
            Assert.IsNull(
                GetOptionalObjectReference<Animator>(player, "animator"),
                "RifleGirl ranged mode should let the native bridge drive locomotion instead of generic player movement parameters.");
            Assert.IsNull(
                GetOptionalObjectReference<Animator>(playerActionController, "animator"),
                "RifleGirl ranged mode should let the native bridge drive fire triggers.");
            RifleGirlNativeGameplayAnimatorBridge nativeBridge =
                RequireComponent<RifleGirlNativeGameplayAnimatorBridge>(rangedAnimator.gameObject, "RifleGirl native ranged animator bridge");
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(nativeBridge, "animator"));
            Assert.AreSame(player, GetObjectReference<PlayerMovementController>(nativeBridge, "movement"));
            Assert.AreSame(playerActionController, GetObjectReference<PlayerActionController>(nativeBridge, "actionController"));
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(nativeBridge, "combatModeController"));
            AssertFallbackCombatModeVisual(rangedVisualRoot, rangedAnimator, rangedWeaponRoot, meleeWeaponRoot);
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(rangedAimController, "combatModeController"));
            Assert.AreSame(cameraController, GetObjectReference<ActionCameraController>(rangedAimController, "cameraController"));
            Assert.AreSame(player, GetObjectReference<PlayerMovementController>(rangedAimController, "movement"));
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(rangedAimController, "animator"));
            Assert.IsTrue(GetBool(rangedAimController, "faceCameraForwardWhileAiming"));
            Assert.IsFalse(GetBool(rangedAimController, "snapAimingFacing"));
            AssertNoPrivateField<PlayerRangedAimController>("keyboardTestKey");
            AssertNoPrivateField<PlayerSkill1Action>("keyboardTestKey");
            AssertNoPrivateField<PlayerSkill1Action>("useKeyboardWhenActionMissing");
            Assert.AreSame(combatModeController, GetObjectReference<PlayerCombatModeController>(rangedBasicAttackAction, "combatModeController"));
            Assert.AreSame(rangedAimController, GetObjectReference<PlayerRangedAimController>(rangedBasicAttackAction, "aimController"));
            Assert.AreSame(player, GetObjectReference<PlayerMovementController>(rangedBasicAttackAction, "movement"));
            Assert.AreSame(targetSelector, GetObjectReference<PlayerCombatTargetSelector>(rangedBasicAttackAction, "targetSelector"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(rangedBasicAttackAction, "sourceHealth"));
            Assert.AreSame(cameraController, GetObjectReference<ActionCameraController>(rangedBasicAttackAction, "cameraController"));
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(rangedBasicAttackAction, "animator"));
            Assert.IsTrue(
                string.IsNullOrEmpty(GetString(rangedBasicAttackAction, "fireTrigger")),
                "RifleGirl ranged fire should stay projectile/VFX-led instead of forcing a mismatched melee attack trigger.");
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "holdFireActivatesAim"));
            Assert.AreEqual(0.18f, GetFloat(rangedBasicAttackAction, "aimInputDeadZone"), 0.001f);
            Assert.AreEqual(34f, GetFloat(rangedBasicAttackAction, "aimInputYawDegrees"), 0.001f);
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "aimFromCameraViewport"));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "useFixedCenterAimViewport"));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "preserveVerticalAim"));
            Assert.AreEqual(26f, GetFloat(rangedBasicAttackAction, "cameraAimFallbackDistance"), 0.001f);
            Assert.AreEqual(0.39f, GetFloat(rangedBasicAttackAction, "aimInputViewportOffsetX"), 0.001f);
            Assert.AreEqual(0.20f, GetFloat(rangedBasicAttackAction, "aimInputViewportOffsetY"), 0.001f);
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "useStableAimOrigin"));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "useAimAssist"));
            Assert.IsFalse(GetBool(rangedBasicAttackAction, "disableAimAssistWithManualInput"));
            Assert.IsFalse(GetBool(rangedBasicAttackAction, "requestFacingOnFire"));
            Assert.Greater(GetFloat(rangedBasicAttackAction, "projectileRadius"), 0f);
            Assert.That(GetFloat(rangedBasicAttackAction, "aimAssistDistance"), Is.InRange(0.01f, 25f));
            Assert.That(GetFloat(rangedBasicAttackAction, "hipAimAssistAngleDegrees"), Is.InRange(0.01f, 20f));
            Assert.That(GetFloat(rangedBasicAttackAction, "aimedAimAssistAngleDegrees"), Is.InRange(0.01f, 20f));
            Assert.That(GetFloat(rangedBasicAttackAction, "aimAssistMaxTurnDegrees"), Is.InRange(0.01f, 20f));
            Assert.IsTrue(GetBool(rangedBasicAttackAction, "driveCameraAimAssist"));
            Assert.That(GetFloat(rangedBasicAttackAction, "cameraAimAssistStrengthScale"), Is.InRange(0.01f, 1f));
            Assert.That(GetFloat(rangedBasicAttackAction, "cameraAimAssistMinStrength"), Is.InRange(0f, 0.5f));
            GameObject rangedBasicProjectilePrefab = LoadAsset<GameObject>(RangedBasicProjectilePrefabPath);
            Assert.AreSame(rangedBasicProjectilePrefab, GetObjectReference<GameObject>(rangedBasicAttackAction, "projectilePrefabObject"));
            Assert.IsTrue(
                rangedBasicProjectilePrefab.GetComponent<LaneActionProjectile>().AllowsVerticalTravel,
                "Player basic ranged fire should allow vertical travel when the center camera ray carries height.");
            MeshRenderer rangedBasicRootRenderer = rangedBasicProjectilePrefab.GetComponent<MeshRenderer>();
            Assert.IsNotNull(
                rangedBasicRootRenderer,
                "Player basic ranged projectile should keep the collision root renderer available for editor repair.");
            Assert.IsFalse(
                rangedBasicRootRenderer.enabled,
                "Player basic ranged projectile should not show its collision root sphere over the Vefects asset shot.");
            Transform rangedBasicShotVfx =
                rangedBasicProjectilePrefab.transform.Find("RangedBasicProjectileVfx_VefectsRifleShotLoop");
            Assert.IsNotNull(
                rangedBasicShotVfx,
                "Player basic ranged projectile should use the Vefects rifle shot loop asset VFX, not generated tracer primitives.");
            Assert.IsNull(
                rangedBasicProjectilePrefab.GetComponent<TrailRenderer>(),
                "Player basic ranged projectile should not fall back to generated TrailRenderer visuals.");
            ParticleSystem[] rangedBasicShotParticles =
                rangedBasicShotVfx.GetComponentsInChildren<ParticleSystem>(true);
            Assert.GreaterOrEqual(
                rangedBasicShotParticles.Length,
                4,
                "Player basic ranged projectile should preserve the authored multi-part Vefects particle setup.");
            for (int particleIndex = 0; particleIndex < rangedBasicShotParticles.Length; particleIndex++)
            {
                ParticleSystem.LightsModule lights = rangedBasicShotParticles[particleIndex].lights;
                if (lights.enabled && lights.light != null)
                {
                    AssertGameOwnedAsset(lights.light, $"{rangedBasicShotParticles[particleIndex].name} shot VFX light");
                }
            }

            ParticleSystemRenderer[] rangedBasicShotRenderers =
                rangedBasicShotVfx.GetComponentsInChildren<ParticleSystemRenderer>(true);
            Assert.Greater(rangedBasicShotRenderers.Length, 0, "Vefects shot VFX should expose particle renderers.");
            for (int rendererIndex = 0; rendererIndex < rangedBasicShotRenderers.Length; rendererIndex++)
            {
                ParticleSystemRenderer renderer = rangedBasicShotRenderers[rendererIndex];
                AssertGameOwnedAsset(renderer.sharedMaterial, $"{renderer.name} shot VFX material");
                AssertVefectsFlipbookMaterial(renderer.sharedMaterial, $"{renderer.name} shot VFX material");
                if (renderer.mesh != null)
                {
                    AssertGameOwnedAsset(renderer.mesh, $"{renderer.name} shot VFX mesh");
                }
            }
            Assert.AreSame(projectileRoot.transform, GetObjectReference<Transform>(rangedBasicAttackAction, "projectileRoot"));
            combatModeController.SetMeleeMode();
            yield return null;
            Assert.IsTrue(combatModeController.IsMeleeMode);
            Assert.IsFalse(rangedVisualRoot.activeSelf, "Melee mode should hide the RifleGirl ranged body.");
            Assert.IsTrue(meleeVisualRoot.activeSelf, "Melee mode should reveal the CombatGirl melee body.");
            Assert.IsFalse(rangedWeaponRoot.activeInHierarchy, "Rifle should hide with the ranged body in melee mode.");
            Assert.IsTrue(meleeWeaponRoot.activeInHierarchy, "Melee visual should show in melee mode.");
            Assert.AreSame(meleeAnimator, GetObjectReference<Animator>(player, "animator"));
            Assert.AreSame(meleeAnimator, GetObjectReference<Animator>(playerActionController, "animator"));
            Assert.AreSame(LoadAsset<RuntimeAnimatorController>(CombatGirlMeleeControllerPath), meleeAnimator.runtimeAnimatorController);
            combatModeController.SetRangedMode();
            yield return null;
            Assert.IsTrue(combatModeController.IsRangedMode);
            Assert.IsTrue(rangedVisualRoot.activeSelf);
            Assert.IsFalse(meleeVisualRoot.activeSelf);
            Assert.IsTrue(rangedWeaponRoot.activeInHierarchy, "Rifle should show again after returning to ranged mode.");
            Assert.IsFalse(meleeWeaponRoot.activeInHierarchy, "Melee visual should hide again after returning to ranged mode.");
            Assert.IsNull(GetOptionalObjectReference<Animator>(player, "animator"));
            Assert.IsNull(GetOptionalObjectReference<Animator>(playerActionController, "animator"));
            Assert.AreSame(LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath), rangedAnimator.runtimeAnimatorController);
            Assert.AreSame(laneSpace, GetObjectReference<SummonLaneSpace>(energyLadder, "laneSpace"));
            Assert.AreSame(player.transform, GetObjectReference<Transform>(energyLadder, "trackedPlayer"));
            Assert.AreEqual(16.5f, GetFloat(energyLadder, "baseEnergyPerSecond"), 0.001f);
            Assert.AreSame(energyLadder, GetObjectReference<SummonEnergyLadder>(skill1Action, "energyLadder"));
            Assert.AreSame(playerHealth, GetObjectReference<CombatHealth>(skill1Action, "sourceHealth"));
            Assert.AreSame(targetSelector, GetObjectReference<PlayerCombatTargetSelector>(skill1Action, "targetSelector"));
            Assert.AreSame(LoadAsset<GameObject>(Skill1ProjectilePrefabPath), GetObjectReference<GameObject>(skill1Action, "projectilePrefabObject"));
            Assert.IsFalse(
                LoadAsset<GameObject>(Skill1ProjectilePrefabPath).GetComponent<LaneActionProjectile>().AllowsVerticalTravel,
                "Lane skill projectiles should stay planar until authored as aimed shots.");
            AssertMagicMissilesLaneProjectile(
                Skill1ProjectilePrefabPath,
                "LaneActionProjectileVfx_MagicMissilesArcaneBolt",
                "Skill1 lane bolt");
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
            AssertMagicMissilesLaneProjectile(
                SummonSlot1ProjectilePrefabPath,
                "LaneActionProjectileVfx_MagicMissilesLightAssistBolt",
                "SummonSlot1 assist bolt");
            AssertMagicMissilesLaneProjectile(
                SummonSlot2ProjectilePrefabPath,
                "LaneActionProjectileVfx_MagicMissilesArcaneMarksmanBolt",
                "SummonSlot2 marksman bolt");
            AssertMagicMissilesLaneProjectile(
                SummonSlot3ProjectilePrefabPath,
                "LaneActionProjectileVfx_MagicMissilesHolyVanguardBolt",
                "SummonSlot3 vanguard bolt");
            GameObject summonEntryCuePrefabObject = LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath);
            Assert.AreSame(summonEntryCuePrefabObject, GetObjectReference<GameObject>(summonSlot1Action, "entryCuePrefab"));
            AssertSummonEntryCueVfx(summonEntryCuePrefabObject);
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
            AssertSummonActorVfx(
                summonActorPrefabObject,
                expectPressureScreen: true,
                label: "SummonSlot1 actor prefab");
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
            BossBarragePatternProfile needleLockPattern = LoadAsset<BossBarragePatternProfile>(PatternProfilePath);
            Assert.AreSame(needleLockPattern, GetObjectReference<BossBarragePatternProfile>(emitter, "patternProfile"));
            BossBarragePatternProfile linePressurePattern = LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath);
            Assert.AreSame(
                linePressurePattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 0));
            Assert.AreEqual(BossBarrageLateralShape.LinePressure, linePressurePattern.LateralShape);
            Assert.Greater(linePressurePattern.LinePressureDirection, 0f);
            AssertBossPatternSkillGrammar(
                linePressurePattern,
                LaneSkillPatternFamily.LinePressure,
                LaneSkillTransferMode.SharedPvpSkillCandidate);
            AssertBossPatternTelegraphRead(
                linePressurePattern,
                0.48f,
                1.85f,
                new Color(0.12f, 0.9f, 1f, 0.72f));
            AssertBossPatternProjectileRead(
                linePressurePattern,
                new Color(0.2f, 0.95f, 1f, 1f),
                new Vector3(0.72f, 0.72f, 2.35f));
            AssertBossPatternTightensForwardRisk(linePressurePattern);
            BossBarragePatternProfile layeredSalvoPattern = LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath);
            Assert.AreSame(
                layeredSalvoPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 1));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, layeredSalvoPattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.LayeredSalvo, layeredSalvoPattern.LateralShape);
            AssertBossPatternSkillGrammar(
                layeredSalvoPattern,
                LaneSkillPatternFamily.LayeredSalvo,
                LaneSkillTransferMode.SharedPvpSkillCandidate);
            AssertBossPatternTelegraphRead(
                layeredSalvoPattern,
                1.28f,
                0.58f,
                new Color(1f, 0.24f, 0.72f, 0.7f));
            AssertBossPatternProjectileRead(
                layeredSalvoPattern,
                new Color(1f, 0.28f, 0.78f, 1f),
                new Vector3(1.45f, 0.58f, 0.9f));
            AssertBossPatternTightensForwardRisk(layeredSalvoPattern);
            Assert.AreSame(
                needleLockPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 2));
            AssertBossPatternSkillGrammar(
                needleLockPattern,
                LaneSkillPatternFamily.DirectLock,
                LaneSkillTransferMode.BossOnly);
            AssertBossPatternTightensForwardRisk(needleLockPattern);
            BossBarragePatternProfile coverFirePattern = LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath);
            Assert.AreSame(
                coverFirePattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 3));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, coverFirePattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.CenterSpread, coverFirePattern.LateralShape);
            AssertBossPatternSkillGrammar(
                coverFirePattern,
                LaneSkillPatternFamily.CenterCover,
                LaneSkillTransferMode.CostedPlayerSkillCandidate);
            AssertBossPatternTightensForwardRisk(coverFirePattern);
            BossBarragePatternProfile escortScreenPattern = LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath);
            Assert.AreSame(
                escortScreenPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 4));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, escortScreenPattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.EscortScreen, escortScreenPattern.LateralShape);
            AssertBossPatternSkillGrammar(
                escortScreenPattern,
                LaneSkillPatternFamily.EscortScreen,
                LaneSkillTransferMode.SharedPvpSkillCandidate);
            AssertBossPatternTightensForwardRisk(escortScreenPattern);
            BossBarragePatternProfile twinSweepPattern = LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath);
            BossBarragePatternProfile staggeredCrossfirePattern =
                LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath);
            Assert.AreSame(
                staggeredCrossfirePattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 5));
            Assert.AreEqual(BossBarrageTargetingRule.LaneCenter, staggeredCrossfirePattern.TargetingRule);
            Assert.AreEqual(BossBarrageLateralShape.StaggeredCrossfire, staggeredCrossfirePattern.LateralShape);
            AssertBossPatternSkillGrammar(
                staggeredCrossfirePattern,
                LaneSkillPatternFamily.StaggeredCrossfire,
                LaneSkillTransferMode.SharedPvpSkillCandidate);
            AssertBossPatternTightensForwardRisk(staggeredCrossfirePattern);
            Assert.AreSame(
                twinSweepPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 6));
            Assert.AreEqual(BossBarrageLateralShape.TwinColumns, twinSweepPattern.LateralShape);
            AssertBossPatternSkillGrammar(
                twinSweepPattern,
                LaneSkillPatternFamily.TwinSweep,
                LaneSkillTransferMode.SharedPvpSkillCandidate);
            AssertBossPatternTightensForwardRisk(twinSweepPattern);
            BossBarragePatternProfile leftClampPattern = LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath);
            Assert.AreSame(
                leftClampPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 7));
            Assert.AreEqual(BossBarrageLateralShape.SideClamp, leftClampPattern.LateralShape);
            Assert.Less(leftClampPattern.SideClampDirection, 0f);
            AssertBossPatternSkillGrammar(
                leftClampPattern,
                LaneSkillPatternFamily.SideClamp,
                LaneSkillTransferMode.SharedPvpSkillCandidate);
            AssertBossPatternTightensForwardRisk(leftClampPattern);
            BossBarragePatternProfile rightClampPattern = LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath);
            Assert.AreSame(
                rightClampPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 8));
            Assert.AreEqual(BossBarrageLateralShape.SideClamp, rightClampPattern.LateralShape);
            Assert.Greater(rightClampPattern.SideClampDirection, 0f);
            AssertBossPatternSkillGrammar(
                rightClampPattern,
                LaneSkillPatternFamily.SideClamp,
                LaneSkillTransferMode.SharedPvpSkillCandidate);
            AssertBossPatternTightensForwardRisk(rightClampPattern);
            BossBarragePatternProfile punishNetPattern = LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath);
            Assert.AreSame(
                punishNetPattern,
                GetArrayObjectReference<BossBarragePatternProfile>(emitter, "patternSequence", 9));
            Assert.AreEqual(BossBarrageLateralShape.PunishNet, punishNetPattern.LateralShape);
            AssertBossPatternSkillGrammar(
                punishNetPattern,
                LaneSkillPatternFamily.PunishNet,
                LaneSkillTransferMode.CostedPlayerSkillCandidate);
            AssertBossPatternTightensForwardRisk(punishNetPattern);
            GameObject bossProjectilePrefabObject = LoadAsset<GameObject>(ProjectilePrefabPath);
            Assert.AreSame(bossProjectilePrefabObject, GetObjectReference<GameObject>(emitter, "projectilePrefabObject"));
            AssertBossBarrageProjectileVisible(bossProjectilePrefabObject, "boss barrage projectile prefab");
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
            Assert.AreSame(bossBasicFireEmitter, GetObjectReference<BossBasicFireEmitter>(pocketOwner, "bossBasicFireEmitter"));
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
            Assert.AreSame(bossBasicFireEmitter, GetObjectReference<BossBasicFireEmitter>(reviewHud, "bossBasicFireEmitter"));
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
            Assert.AreSame(playerActionController, GetObjectReference<PlayerActionController>(screenCuePresenter, "actionController"));
            Assert.AreSame(rangedBasicAttackAction, GetObjectReference<PlayerRangedBasicAttackAction>(screenCuePresenter, "rangedBasicAttackAction"));
            Assert.AreSame(skill1Action, GetObjectReference<PlayerSkill1Action>(screenCuePresenter, "skill1Action"));
            Assert.AreSame(summonSlot1Action, GetObjectReference<PlayerSummonSlot1Action>(screenCuePresenter, "summonSlot1Action"));
            Assert.AreSame(
                GetObjectReference<PlayerSupportSummonSlotAction>(mobileHud, "summonSlot2Action"),
                GetObjectReference<PlayerSupportSummonSlotAction>(screenCuePresenter, "summonSlot2Action"));
            Assert.AreSame(
                GetObjectReference<PlayerSupportSummonSlotAction>(mobileHud, "summonSlot3Action"),
                GetObjectReference<PlayerSupportSummonSlotAction>(screenCuePresenter, "summonSlot3Action"));
            Assert.AreSame(emitter, GetObjectReference<BossBarrageEmitter>(screenCuePresenter, "bossBarrageEmitter"));
            Assert.AreSame(bossPressureActionDirector, GetObjectReference<BossPressureActionDirector>(screenCuePresenter, "bossPressureActionDirector"));
            Assert.AreSame(pocketOwner, GetObjectReference<BossBarragePocketReviewOwner>(screenCuePresenter, "pocketReviewOwner"));
            Assert.IsTrue(screenCuePresenter.ShowScreenCues);
            Assert.AreEqual(0.16f, screenCuePresenter.MaxFullScreenAlpha, 0.001f);
            Assert.AreEqual(0.34f, screenCuePresenter.MaxEdgeAlpha, 0.001f);
            Assert.AreEqual(118f, screenCuePresenter.EdgeThickness, 0.001f);
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
            Assert.IsFalse(GetBool(mobileHud, "routeAimToMovementLook"));
            Assert.IsTrue(GetBool(mobileHud, "keyboardPeekControlsAim"));
            Assert.AreEqual(InputSystemKeyQ, GetEnumIndex(mobileHud, "keyboardPeekLeftKey"));
            Assert.AreEqual(InputSystemKeyE, GetEnumIndex(mobileHud, "keyboardPeekRightKey"));
            Assert.IsTrue(GetBool(mobileHud, "keyboardPeekRequiresActiveAim"));
            Assert.IsTrue(GetBool(mobileHud, "fireAimReticleUsesScreenCenter"));
            Assert.AreEqual(0.08f, GetFloat(mobileHud, "lookAimDragDeadZone"), 0.001f);
            Assert.AreEqual(230f, GetFloat(mobileHud, "lookAimDragRadius"), 0.001f);
            Assert.AreEqual(30f, GetFloat(mobileHud, "lookAimKnobSize"), 0.001f);
            Assert.AreEqual(0f, GetFloat(mobileHud, "lookAimScreenMinX"), 0.001f);
            Assert.IsTrue(GetBool(mobileHud, "showFireAimReticle"));
            Assert.Greater(GetFloat(mobileHud, "fireAimReticleSize"), 0f);
            Assert.Greater(GetFloat(mobileHud, "fireAimReticleGap"), 0f);
            Assert.Greater(GetFloat(mobileHud, "fireAimReticleThickness"), 0f);
            Assert.Greater(GetFloat(mobileHud, "fireAimAssistGapTighten"), 0f);
            Assert.Greater(GetFloat(mobileHud, "fireAimAssistSizeBoost"), 0f);
            Assert.Greater(GetFloat(mobileHud, "fireAimAssistThicknessBoost"), 0f);
            Assert.IsTrue(GetBool(mobileHud, "fireAimReticleFollowsAssist"));
            Assert.Greater(GetFloat(mobileHud, "fireAimAssistReticleMaxOffset"), 0f);

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
            Assert.AreEqual(0.5f, viewportPoint.x, 0.001f);
            Assert.AreEqual(0.5f, viewportPoint.y, 0.001f);

            rangedBasicAttackAction.SetFireHeld(false);
            yield return null;

            Assert.IsFalse(rangedBasicAttackAction.IsFireHeld);
            Assert.IsFalse(aimController.IsAiming);
        }

        [UnityTest]
        public IEnumerator RangedAimTargetBiasMovesPlayerAttachedShoulderRigWithinFortyFiveDegrees()
        {
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(
                    RequireObject<PlayerMovementController>().gameObject,
                    "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(
                    combatModeController.gameObject,
                    "player ranged aim controller");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            Camera camera = RequireComponent<Camera>(cameraController.gameObject, "review camera");

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            aimController.SetAimInput(Vector2.zero);
            yield return WaitSeconds(0.35f);

            Vector3 shoulderDirectionBeforePeek = Vector3.ProjectOnPlane(
                cameraController.transform.position - combatModeController.transform.position,
                Vector3.up).normalized;
            Vector3 playerViewportBeforePeek =
                camera.WorldToViewportPoint(combatModeController.transform.position + Vector3.up * 1.15f);

            aimController.SetAimInput(Vector2.right);
            yield return WaitSeconds(0.35f);

            Assert.AreEqual(1f, cameraController.AimOrbitInput.x, 0.001f);
            Assert.That(cameraController.AimYawOffsetDegrees, Is.InRange(30f, 45.01f));
            Vector3 shoulderDirectionAfterPeek = Vector3.ProjectOnPlane(
                cameraController.transform.position - combatModeController.transform.position,
                Vector3.up).normalized;
            float shoulderAngle = Vector3.Angle(shoulderDirectionBeforePeek, shoulderDirectionAfterPeek);
            Assert.Greater(
                shoulderAngle,
                18f,
                "Aim peek should move the shoulder camera position with the center aim line so player and camera read as one linked rig.");
            Vector3 playerViewportAfterPeek =
                camera.WorldToViewportPoint(combatModeController.transform.position + Vector3.up * 1.15f);
            Assert.Less(
                Mathf.Abs(playerViewportAfterPeek.x - playerViewportBeforePeek.x),
                0.06f,
                "TPS aim peek should keep the player near the same horizontal screen anchor instead of sliding across the view.");
            Assert.Less(
                Mathf.Abs(playerViewportAfterPeek.y - playerViewportBeforePeek.y),
                0.06f,
                "TPS aim peek should keep the player near the same vertical screen anchor instead of sliding across the view.");

            aimController.SetAimInput(Vector2.right * 5f);
            yield return WaitSeconds(0.1f);
            Assert.LessOrEqual(
                Mathf.Abs(cameraController.AimYawOffsetDegrees),
                45.01f,
                "Aim camera peek should stay inside the authored forward 45-degree cone.");

            float heldRightYaw = cameraController.AimYawOffsetDegrees;
            aimController.SetAimInput(Vector2.zero);
            yield return WaitSeconds(0.22f);
            Assert.AreEqual(
                heldRightYaw,
                cameraController.AimYawOffsetDegrees,
                1.5f,
                "Aim camera peek should hold while ranged aim/fire remains active.");

            aimController.SetAimInput(Vector2.left);
            yield return WaitSeconds(0.35f);
            Assert.That(cameraController.AimYawOffsetDegrees, Is.InRange(-45.01f, -30f));

            aimController.SetAimHeld(false);
            yield return WaitSeconds(0.22f);
            Assert.Less(
                Mathf.Abs(cameraController.AimYawOffsetDegrees),
                1f,
                "Aim camera peek should recenter when aim mode ends.");
        }

        [UnityTest]
        public IEnumerator RangedAimKeepsPeekInputUnmodifiedWhenFireAssistTargetIsActive()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            rangedBasicAttackAction.SetAimInput(Vector2.right);
            aimController.SetAimInput(Vector2.right);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.35f);

            Vector3 aimPlanarDirection = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up);
            Assert.Greater(aimPlanarDirection.sqrMagnitude, 0.0001f);
            aimPlanarDirection.Normalize();
            Vector3 aimRight = Vector3.Cross(Vector3.up, aimPlanarDirection).normalized;
            Vector3 closeThreatPosition =
                player.transform.position + aimPlanarDirection * 4.6f + aimRight * 0.35f;
            closeThreatPosition.y = closeThreatRoot.transform.position.y;
            closeThreatRoot.transform.SetPositionAndRotation(
                closeThreatPosition,
                Quaternion.LookRotation(-aimPlanarDirection, Vector3.up));
            closeThreatHealth.ResetHealthToFull();
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            rangedBasicAttackAction.SetAimInput(Vector2.right);
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out _));
            Assert.IsTrue(rangedBasicAttackAction.HasAimAssistTarget);
            Assert.AreSame(closeThreatHealth, rangedBasicAttackAction.AimAssistTargetHealth);

            aimController.SetAimInput(Vector2.right);

            Assert.AreEqual(
                1f,
                aimController.AimInput.x,
                0.001f,
                "Q/E-style review peek must remain camera input, not hidden aim-assist friction.");
            Assert.AreEqual(
                1f,
                cameraController.AimOrbitInput.x,
                0.001f,
                "Fire assist may bend the shot, but it must not silently change the camera peek input.");

            rangedBasicAttackAction.ClearAimInput();
            aimController.SetAimInput(Vector2.zero);
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = true;
            }
        }

        [UnityTest]
        public IEnumerator RangedFireAssistPullsCameraAimAxisTowardCloseThreat()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            aimController.SetAimInput(Vector2.zero);
            rangedBasicAttackAction.ClearAimInput();
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.22f);

            Vector3 initialAimForward = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up);
            Assert.Greater(initialAimForward.sqrMagnitude, 0.0001f);
            initialAimForward.Normalize();
            Vector3 aimRight = Vector3.Cross(Vector3.up, initialAimForward).normalized;
            Vector3 closeThreatPosition =
                player.transform.position + initialAimForward * 4.6f + aimRight * 0.45f;
            closeThreatPosition.y = closeThreatRoot.transform.position.y;
            closeThreatRoot.transform.SetPositionAndRotation(
                closeThreatPosition,
                Quaternion.LookRotation(-initialAimForward, Vector3.up));
            closeThreatHealth.ResetHealthToFull();
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out _));
            Assert.IsTrue(rangedBasicAttackAction.HasAimAssistTarget);
            Assert.AreSame(closeThreatHealth, rangedBasicAttackAction.AimAssistTargetHealth);
            Assert.That(cameraController.AimOrbitInput.x, Is.EqualTo(0f).Within(0.001f));

            Vector3 initialCameraTargetDirection =
                Vector3.ProjectOnPlane(closeThreatPosition - cameraController.transform.position, Vector3.up).normalized;
            float signedAngleBeforePull =
                Vector3.SignedAngle(initialAimForward, initialCameraTargetDirection, Vector3.up);
            Assert.Greater(Mathf.Abs(signedAngleBeforePull), 0.1f);
            rangedBasicAttackAction.SetFireHeld(true);
            yield return WaitSeconds(0.18f);

            Vector3 pulledAimForward = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up);
            Assert.Greater(pulledAimForward.sqrMagnitude, 0.0001f);
            pulledAimForward.Normalize();
            float signedPullAngle = Vector3.SignedAngle(initialAimForward, pulledAimForward, Vector3.up);
            Assert.Greater(
                Mathf.Abs(cameraController.AimAssistYawOffsetDegrees),
                0.05f,
                "Fire assist should pull the camera aim axis, not only bend an invisible projectile path.");
            Assert.Greater(
                signedPullAngle * signedAngleBeforePull,
                0f,
                "The center aim axis should yaw toward the close assisted target while FIRE is held.");
            Assert.That(cameraController.AimOrbitInput.x, Is.EqualTo(0f).Within(0.001f));

            rangedBasicAttackAction.SetFireHeld(false);
            rangedBasicAttackAction.ClearAimInput();
            aimController.SetAimInput(Vector2.zero);
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = true;
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedAimKeepsPlayerFacingCameraForwardWhileMovingSideways()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(HudRootName), "boss barrage mobile HUD");

            mobileHud.enabled = false;
            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            aimController.SetAimInput(Vector2.zero);
            player.SetMoveInput(Vector2.right);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.35f);

            Vector3 playerForward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up).normalized;
            Vector3 moveDirection = Vector3.ProjectOnPlane(player.CurrentMoveDirection, Vector3.up).normalized;
            Assert.Less(
                Vector3.Angle(playerForward, cameraForward),
                14f,
                "While ranged aim is held, the player body should keep facing the aim camera forward instead of turning into movement.");
            Assert.Greater(
                Vector3.Angle(playerForward, moveDirection),
                35f,
                "Side movement during ranged aim should read as a strafe/backstep layer, not as the main facing owner.");

            player.SetMoveInput(Vector2.zero);
            aimController.SetAimHeld(false);
            mobileHud.enabled = true;
            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedBasicFireUsesCenterCameraRayAfterAimPeek()
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
            aimController.SetAimInput(Vector2.right);
            rangedBasicAttackAction.SetAimInput(Vector2.right);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.22f);

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewViewportPoint(out Vector2 viewportPoint));
            Assert.AreEqual(0.5f, viewportPoint.x, 0.001f);
            Assert.AreEqual(0.5f, viewportPoint.y, 0.001f);
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.IsTrue(rangedBasicAttackAction.TryFire());
            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, previewDirection),
                0.5f,
                "The actual basic-fire projectile should use the same direction as the aim preview reticle.");
            Assert.Greater(
                Vector3.Dot(playerProjectile.TravelDirection, Vector3.right),
                0.2f,
                "Look/TargetBias should steer the aim camera, then basic fire should use the center camera ray.");
            Assert.Greater(
                Vector3.Dot(playerProjectile.TravelDirection, Vector3.forward),
                0.7f,
                "Center-ray aim should stay in the forward lane instead of becoming a side-only shot.");

            aimController.SetAimInput(Vector2.zero);
            rangedBasicAttackAction.ClearAimInput();
            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedBasicAimAssistLandsNearCloseThreatBodyFromMuzzle()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController aimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>();
            ActionCameraController cameraController = RequireObject<ActionCameraController>();
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            GameObject closeThreatRoot = RequireRoot(CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreatRoot, "close threat health");
            Collider closeThreatCollider = RequireCombatHitCollider(closeThreatRoot, closeThreatHealth, "close threat");
            BasicSoldierEnemy closeThreatEnemy = closeThreatRoot.GetComponent<BasicSoldierEnemy>();

            combatModeController.SetRangedMode();
            aimController.SetAimHeld(true);
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = false;
            }

            player.transform.position =
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, player.transform.position.y);
            aimController.SetAimInput(Vector2.right);
            rangedBasicAttackAction.SetAimInput(Vector2.right);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.35f);

            Vector3 aimPlanarDirection = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up);
            Assert.Greater(aimPlanarDirection.sqrMagnitude, 0.0001f);
            aimPlanarDirection.Normalize();
            Vector3 aimRight = Vector3.Cross(Vector3.up, aimPlanarDirection).normalized;
            Vector3 closeThreatPosition =
                player.transform.position + aimPlanarDirection * 4.6f + aimRight * 0.35f;
            closeThreatPosition.y = closeThreatRoot.transform.position.y;
            closeThreatRoot.transform.SetPositionAndRotation(
                closeThreatPosition,
                Quaternion.LookRotation(-aimPlanarDirection, Vector3.up));
            closeThreatHealth.ResetHealthToFull();
            targetSelector.NotifyTargetContact(closeThreatHealth);
            targetSelector.RefreshTarget();
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.IsTrue(rangedBasicAttackAction.HasAimAssistTarget);
            Assert.AreSame(closeThreatHealth, rangedBasicAttackAction.AimAssistTargetHealth);
            Assert.Greater(rangedBasicAttackAction.AimAssistStrength01, 0f);
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimAssistPreviewViewportPoint(out Vector2 assistViewportPoint));
            Assert.That(assistViewportPoint.x, Is.InRange(0f, 1f));
            Assert.That(assistViewportPoint.y, Is.InRange(0f, 1f));
            Assert.Greater(
                Vector2.Distance(assistViewportPoint, new Vector2(0.5f, 0.5f)),
                0.003f,
                "The main fire reticle needs a moving screen point so the player can see where weak fire assist is pulling the shot.");
            Assert.IsTrue(rangedBasicAttackAction.TryFire());

            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, previewDirection),
                0.5f,
                "The fired projectile should use the same assisted direction shown by the aim preview.");

            float projectileRadius = GetFloat(rangedBasicAttackAction, "projectileRadius");
            float pathMissDistance = DistanceFromRayToBounds(
                playerProjectile.transform.position,
                playerProjectile.TravelDirection,
                closeThreatCollider.bounds);
            Assert.LessOrEqual(
                pathMissDistance,
                projectileRadius + 0.03f,
                "Assisted ranged basic fire should route the actual muzzle projectile through the close-threat body bounds.");

            float healthBeforeImpact = closeThreatHealth.CurrentHealth;
            Assert.IsTrue(playerProjectile.TryApplyImpact(closeThreatCollider, playerProjectile.transform.position));
            Assert.Less(closeThreatHealth.CurrentHealth, healthBeforeImpact);

            rangedBasicAttackAction.ClearAimInput();
            aimController.SetAimInput(Vector2.zero);
            if (closeThreatEnemy != null)
            {
                closeThreatEnemy.enabled = true;
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator RangedBasicFireKeepsCenterRayStableWhenMuzzleMoves()
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
            aimController.SetAimInput(new Vector2(0.65f, 0.15f));
            rangedBasicAttackAction.SetAimInput(new Vector2(0.65f, 0.15f));
            Physics.SyncTransforms();
            yield return WaitSeconds(0.22f);

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
                "Center camera-ray aim should stay stable when the animated muzzle moves between frames.");
            Assert.IsTrue(rangedBasicAttackAction.TryFire());

            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, initialPreviewDirection),
                0.5f,
                "The fired projectile should follow the stable center aim direction instead of the current muzzle wobble.");

            fireOrigin.localPosition = originalFireOriginLocalPosition;
            fireOrigin.localRotation = originalFireOriginLocalRotation;
            aimController.SetAimInput(Vector2.zero);
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
            aimController.SetAimInput(Vector2.right);
            rangedBasicAttackAction.SetAimInput(Vector2.right);
            Physics.SyncTransforms();
            yield return WaitSeconds(0.22f);

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
                0.2f,
                "Aim-camera peek should steer the center-ray shot even when root facing is stable.");

            aimController.SetAimInput(Vector2.zero);
            rangedBasicAttackAction.ClearAimInput();
            yield return null;
        }

        [UnityTest]
        public IEnumerator MobileHudFireButtonHoldDoesNotRouteAimOrRotateStandingPlayerRoot()
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
            rangedBasicAttackAction.ClearAimInput();
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirectionBefore));

            SetPrivateField(mobileHud, "firePointerHeld", true);
            InvokePrivateMethod(mobileHud, "UpdateHudLookAim");

            Assert.Less(
                GetVector2(player, "mobileLookInput").sqrMagnitude,
                0.0001f,
                "Fire-button hold should not be routed into movement look/facing input.");
            Assert.Less(
                rangedBasicAttackAction.AimInput.sqrMagnitude,
                0.0001f,
                "Fire-button hold should not create a manual ranged aim input.");
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.Less(
                Vector3.Angle(previewDirectionBefore, previewDirection),
                0.5f,
                "Fire-button hold should stay a fire gesture, not a joystick-style ranged aim input.");
            yield return null;
            yield return null;
            Assert.Less(
                Quaternion.Angle(rootRotationBefore, player.transform.rotation),
                0.5f,
                "Fire-button hold should not spin the player root.");

            SetPrivateField(mobileHud, "firePointerHeld", false);
            InvokePrivateMethod(mobileHud, "ReleaseHudLookAim");
            yield return null;
        }

        [UnityTest]
        public IEnumerator RifleGirlRangedVisualKeepsAuthoredWeaponConstraintAndSupportHand()
        {
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(
                    RequireObject<PlayerMovementController>().gameObject,
                    "player combat mode controller");
            Animator rangedAnimator = GetObjectReference<Animator>(combatModeController, "rangedAnimator");
            RifleGirlWeaponSocketDriver socketDriver =
                RequireComponent<RifleGirlWeaponSocketDriver>(rangedAnimator.gameObject, "RifleGirl rifle socket driver");

            Assert.IsTrue(socketDriver.IsConfigured, "RifleGirl rifle socket driver should be configured for the authored rifle constraint.");
            ParentConstraint rifleConstraint = GetObjectReference<ParentConstraint>(socketDriver, "rifleConstraint");
            Assert.IsNotNull(rifleConstraint, "Fallback RifleGirl ranged visual should keep the authored ParentConstraint.");
            Assert.GreaterOrEqual(rifleConstraint.sourceCount, 3, "RifleGirl rifle constraint should keep hand, holster, and aim sources.");
            Assert.IsTrue(GetBool(socketDriver, "ignoreRedundantSocketCommands"));
            Assert.IsNotNull(
                GetObjectReference<Transform>(socketDriver, "leftHandIkTarget"),
                "RifleGirl rifle socket driver should bind the authored support-hand IK target.");
            Assert.AreEqual("To_Hand_R_Socket, IK_ON_Left_Handle", GetString(socketDriver, "defaultCommands"));
            Assert.AreEqual(
                1f,
                GetFloat(socketDriver, "leftIkMaxWeight"),
                0.001f,
                "RifleGirl should use the authored full support-hand IK weight.");
            Assert.AreEqual(
                1f,
                GetFloat(socketDriver, "leftIkRotationMaxWeight"),
                0.001f,
                "RifleGirl should preserve the authored support-hand wrist roll.");
            GameObject rangedWeaponRoot = GetObjectReference<GameObject>(combatModeController, "rangedWeaponRoot");
            Assert.IsNull(
                rangedWeaponRoot.GetComponent<RetargetedHandWeaponAttachment>(),
                "Fallback RifleGirl rifle should use its native ParentConstraint instead of the failed retargeted hand attachment.");
            yield return null;
            Transform rightHand = rangedAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            Assert.IsNotNull(rightHand, "RifleGirl should resolve a humanoid right hand socket.");
            Transform leftHand = rangedAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            Assert.IsNotNull(leftHand, "RifleGirl should resolve a humanoid left hand socket.");
            Transform leftHandle = FindDescendant(rangedWeaponRoot.transform, "Left_Handle");
            Assert.IsNotNull(leftHandle, "RifleGirl rifle should keep the authored support-hand handle.");
            Assert.Less(
                Vector3.Distance(leftHand.position, leftHandle.position),
                0.12f,
                "RifleGirl left hand should stay on the authored support handle instead of floating away from the gun.");
            Assert.Greater(
                rangedWeaponRoot.GetComponentsInChildren<Renderer>(true).Length,
                0,
                "RifleGirl ranged weapon should keep visible promoted renderers.");
            Vector3 rifleToHandle = (leftHandle.position - rangedWeaponRoot.transform.position).normalized;
            Assert.Greater(
                Vector3.Dot(rifleToHandle, (leftHand.position - rangedWeaponRoot.transform.position).normalized),
                0.95f,
                "Support hand and handle should remain on the same authored side of the rifle.");

            socketDriver.SwitchSocketByString("IK_OFF_Left_Handle");
            yield return null;
            socketDriver.SwitchSocketByString("IK_ON_Left_Handle");
            yield return null;

            Assert.GreaterOrEqual(
                socketDriver.RifleConstraintSourceApplyCount,
                1,
                "RifleGirl socket events should apply the authored weapon ParentConstraint source.");
            Assert.AreEqual(0, socketDriver.ActiveRifleConstraintSourceIndex);
        }

        [UnityTest]
        public IEnumerator RangedBasicFireKeepsFpsStyleCenterReticleForVerticalAimInput()
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
            aimController.SetAimInput(Vector2.up);
            rangedBasicAttackAction.SetAimInput(Vector2.up);
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewViewportPoint(out Vector2 viewportPoint));
            Assert.AreEqual(0.5f, viewportPoint.x, 0.001f);
            Assert.AreEqual(
                0.5f,
                viewportPoint.y,
                0.001f,
                "Look/TargetBias should not move the FPS-style center reticle.");
            Assert.IsTrue(rangedBasicAttackAction.TryGetAimPreviewDirection(out Vector3 previewDirection));
            Assert.IsTrue(rangedBasicAttackAction.TryFire());

            LaneActionProjectile playerProjectile = RequireActivePlayerRangedProjectile();
            Assert.IsTrue(playerProjectile.AllowsVerticalTravel);
            Assert.Less(
                Vector3.Angle(playerProjectile.TravelDirection, previewDirection),
                0.5f,
                "The actual projectile should match the center camera-ray preview direction.");

            aimController.SetAimInput(Vector2.zero);
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
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossRoot, "boss basic fire emitter");
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
            bossBasicFireEmitter.Tick(20f);
            Assert.IsFalse(
                bossBasicFireEmitter.IsFiringEnabled,
                "Pocket clear should stop boss basic fire alongside committed barrage patterns.");
            Assert.AreEqual(0, bossBasicFireEmitter.ActiveProjectileCount);
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
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossRoot, "boss basic fire emitter");
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
            bossBasicFireEmitter.Tick(20f);
            Assert.IsFalse(
                bossBasicFireEmitter.IsFiringEnabled,
                "Pocket failure should stop boss basic fire alongside committed barrage patterns.");
            Assert.AreEqual(0, bossBasicFireEmitter.ActiveProjectileCount);
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
        public IEnumerator BossBasicFireEmitterFiresVisibleWeakProjectilesFromBossSide()
        {
            PlayerMovementController player = RequireObject<PlayerMovementController>();
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            Collider playerHitCollider = RequireCombatHitCollider(player.gameObject, playerHealth, "player");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            GameObject bossRoot = RequireRoot(BossRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossRoot, "boss health");
            BossBasicFireEmitter basicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossRoot, "boss basic fire emitter");
            BossBasicFireProfile profile = LoadAsset<BossBasicFireProfile>(BossBasicFireProfilePath);

            basicFireEmitter.SetFiringEnabled(false);
            basicFireEmitter.SetFiringEnabled(true);
            float playerHealthBefore = playerHealth.CurrentHealth;
            int firedCount = basicFireEmitter.FireVolley();

            Assert.AreEqual(profile.ProjectilesPerVolley, firedCount);
            Assert.AreEqual(firedCount, basicFireEmitter.LastVolleyProjectileCount);
            Assert.AreEqual(1, basicFireEmitter.TotalVolleysFired);
            Assert.AreSame(profile, basicFireEmitter.FireProfile);
            Assert.GreaterOrEqual(basicFireEmitter.ActiveProjectileCount, firedCount);
            Assert.AreEqual(laneSpace.EvaluateForwardRisk01(player.transform.position), basicFireEmitter.LastForwardRisk01, 0.001f);

            BossBarrageProjectile basicProjectile = RequireActiveBossProjectileWithMaterial(profile.ProjectileMaterial);
            Assert.AreEqual(DamageTeam.Enemy, basicProjectile.SourceTeam);
            AssertBossBarrageProjectilePresentation(
                basicProjectile,
                profile.ProjectileColor,
                profile.ProjectileVisualScale,
                profile.ProjectileMaterial,
                "boss basic fire projectile");
            Vector2 projectileLanePoint = laneSpace.GetLaneCoordinates(basicProjectile.transform.position);
            Assert.Greater(
                projectileLanePoint.y,
                laneSpace.ForwardBoundaryZ,
                "Boss basic fire should still originate from the boss/frontline side.");

            Assert.IsTrue(basicProjectile.TryApplyImpact(playerHitCollider, basicProjectile.transform.position));
            Assert.AreEqual(
                playerHealthBefore - profile.Damage,
                playerHealth.CurrentHealth,
                0.001f,
                "Boss basic fire should use a weak regular-fire damage value, not the heavier boss skill-pattern damage.");
            Assert.AreSame(bossHealth, GetObjectReference<CombatHealth>(basicFireEmitter, "sourceHealth"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossBarrageEmitterFiresVisiblePooledProjectilesFromBossSide()
        {
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(LaneRootName), "lane space");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(RequireRoot(BossRootName), "boss barrage emitter");
            BossBarragePatternProfile pattern = LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath);
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
                LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should surface LayeredSalvo immediately after the opening LinePressure wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(PatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should return to NeedleLock after the two visible pattern openers.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from NeedleLock into the center-path CoverFire pattern after the third wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from CoverFire into the escort-screen pressure pattern after the fourth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from EscortScreen into the staggered crossfire barrage after the fifth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from StaggeredCrossfire into the twin-column barrage pattern after the sixth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from TwinSweep into the first side-clamp barrage pattern after the seventh wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should mirror side-clamp pressure after the eighth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should move from mirrored side clamp into the player-centered PunishNet pattern after the ninth wave.");

            Assert.IsTrue(emitter.BeginWindup());
            emitter.FirePendingWave();
            Assert.AreSame(
                LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath),
                emitter.CurrentPattern,
                "Review boss should loop back into the visible LinePressure opener after PunishNet.");

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
                    AssertBossBarrageProjectileVisible(
                        projectiles[i].gameObject,
                        "active boss barrage projectile");
                    foundBossSideProjectile = true;
                    break;
                }
            }

            Assert.IsTrue(foundBossSideProjectile, "Boss barrage projectiles should spawn from the boss/frontline side.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossBarrageTelegraphUsesPatternSpecificLineAndLayeredReads()
        {
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(RequireRoot(BossRootName), "boss barrage emitter");
            BossBarrageLaneTelegraphPresenter telegraphPresenter =
                RequireComponent<BossBarrageLaneTelegraphPresenter>(
                    RequireRoot(BossTelegraphRootName),
                    "boss barrage lane telegraph presenter");
            BossBarragePatternProfile linePressurePattern =
                LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath);
            BossBarragePatternProfile layeredSalvoPattern =
                LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath);

            emitter.ConfigurePatternSequence(new[] { linePressurePattern }, 1);
            Assert.IsTrue(emitter.BeginWindup());
            telegraphPresenter.RefreshNow();

            Vector3 lineScale = telegraphPresenter.LastMarkerScale;
            Color lineColor = telegraphPresenter.LastMarkerColor;
            Assert.AreSame(linePressurePattern, telegraphPresenter.VisiblePattern);
            Assert.AreEqual(linePressurePattern.ProjectilesPerWave, telegraphPresenter.VisibleMarkerCount);
            Assert.Less(
                lineScale.x / lineScale.z,
                0.4f,
                "LinePressure telegraph should read as a narrow depth rail, not a generic square marker.");
            Assert.Greater(
                lineColor.g,
                lineColor.r,
                "LinePressure should use its cyan rail warning color instead of the generic orange warning.");

            emitter.FirePendingWave();
            BossBarrageProjectile lineProjectile = RequireActiveBossProjectile();
            AssertBossBarrageProjectilePresentation(
                lineProjectile,
                linePressurePattern.ProjectileColor,
                linePressurePattern.ProjectileVisualScale,
                linePressurePattern.ProjectileMaterial,
                "LinePressure fired projectile");
            emitter.SetFiringEnabled(false);
            emitter.ConfigurePatternSequence(new[] { layeredSalvoPattern }, 1);
            emitter.SetFiringEnabled(true);
            Assert.IsTrue(emitter.BeginWindup());
            telegraphPresenter.RefreshNow();

            Vector3 layeredScale = telegraphPresenter.LastMarkerScale;
            Color layeredColor = telegraphPresenter.LastMarkerColor;
            Assert.AreSame(layeredSalvoPattern, telegraphPresenter.VisiblePattern);
            Assert.AreEqual(layeredSalvoPattern.ProjectilesPerWave, telegraphPresenter.VisibleMarkerCount);
            Assert.Greater(
                layeredScale.x / layeredScale.z,
                lineScale.x / lineScale.z,
                "LayeredSalvo telegraph should read as row plates instead of the LinePressure rail shape.");
            Assert.Greater(
                layeredColor.r,
                layeredColor.g,
                "LayeredSalvo should use its magenta row warning color before release.");
            emitter.FirePendingWave();
            BossBarrageProjectile layeredProjectile = RequireActiveBossProjectile();
            AssertBossBarrageProjectilePresentation(
                layeredProjectile,
                layeredSalvoPattern.ProjectileColor,
                layeredSalvoPattern.ProjectileVisualScale,
                layeredSalvoPattern.ProjectileMaterial,
                "LayeredSalvo fired projectile");

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
            float expectedMaximumPlayerForwardRisk01 = 1f,
            bool expectedUsePlayerSummonResponseGate = false,
            int expectedMinimumPlayerSummonTier = 1)
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
            Assert.AreEqual(expectedUsePlayerSummonResponseGate, slot.UsePlayerSummonResponseGate);
            Assert.AreEqual(expectedMinimumPlayerSummonTier, slot.MinimumPlayerSummonTier);
        }

        private static void AssertBossPatternSkillGrammar(
            BossBarragePatternProfile pattern,
            LaneSkillPatternFamily expectedFamily,
            LaneSkillTransferMode expectedTransferMode)
        {
            Assert.IsNotNull(pattern);
            Assert.AreEqual(expectedFamily, pattern.SkillPatternFamily);
            Assert.AreEqual(expectedTransferMode, pattern.SkillTransferMode);
            Assert.AreEqual(
                expectedTransferMode != LaneSkillTransferMode.BossOnly,
                pattern.IsPlayerSkillCandidate,
                $"{pattern.PatternId} should expose whether it can become a costed player/PvP skill.");
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(pattern.CounterplayNote),
                $"{pattern.PatternId} should document the readable answer before reuse as shared skill grammar.");

            if (pattern.IsPlayerSkillCandidate)
            {
                Assert.IsFalse(
                    string.IsNullOrWhiteSpace(pattern.PlayerSkillTranslationNote),
                    $"{pattern.PatternId} should document how it can move to a costed player/PvP skill.");
            }
        }

        private static void AssertBossPatternTelegraphRead(
            BossBarragePatternProfile pattern,
            float expectedWidthScale,
            float expectedDepthScale,
            Color expectedWindupColor)
        {
            Assert.IsNotNull(pattern);
            Assert.AreEqual(expectedWidthScale, pattern.TelegraphMarkerWidthScale, 0.001f);
            Assert.AreEqual(expectedDepthScale, pattern.TelegraphMarkerDepthScale, 0.001f);
            Assert.AreEqual(expectedWindupColor.r, pattern.TelegraphWindupColor.r, 0.001f);
            Assert.AreEqual(expectedWindupColor.g, pattern.TelegraphWindupColor.g, 0.001f);
            Assert.AreEqual(expectedWindupColor.b, pattern.TelegraphWindupColor.b, 0.001f);
            Assert.AreEqual(expectedWindupColor.a, pattern.TelegraphWindupColor.a, 0.001f);
        }

        private static void AssertBossPatternProjectileRead(
            BossBarragePatternProfile pattern,
            Color expectedColor,
            Vector3 expectedScale)
        {
            Assert.IsNotNull(pattern);
            AssertColorNear(expectedColor, pattern.ProjectileColor, $"{pattern.PatternId} projectile color");
            AssertVectorNear(expectedScale, pattern.ProjectileVisualScale, $"{pattern.PatternId} projectile visual scale");
            Assert.IsNotNull(pattern.ProjectileMaterial, $"{pattern.PatternId} should bind a game-owned projectile material.");
            AssertGameOwnedAsset(pattern.ProjectileMaterial, $"{pattern.PatternId} projectile material");
            AssertColorNear(expectedColor, ReadMaterialColor(pattern.ProjectileMaterial), $"{pattern.PatternId} projectile material color");
        }

        private static void AssertBossBarrageProjectilePresentation(
            BossBarrageProjectile projectile,
            Color expectedColor,
            Vector3 expectedScale,
            Material expectedMaterial,
            string label)
        {
            Assert.IsNotNull(projectile, $"{label} should exist.");
            AssertColorNear(expectedColor, projectile.LastPresentationColor, $"{label} color");
            AssertVectorNear(expectedScale, projectile.LastPresentationScale, $"{label} scale");
            Assert.AreSame(expectedMaterial, projectile.LastPresentationMaterial, $"{label} should remember its pattern material.");
            MeshRenderer rootRenderer = projectile.GetComponent<MeshRenderer>();
            Assert.IsNotNull(rootRenderer, $"{label} should keep a hidden collision root MeshRenderer.");
            Assert.IsFalse(rootRenderer.enabled, $"{label} should not render its collision root sphere.");

            ParticleSystemRenderer[] renderers = projectile.GetComponentsInChildren<ParticleSystemRenderer>(true);
            Assert.Greater(renderers.Length, 0, $"{label} should render through authored asset particle VFX.");
            for (int i = 0; i < renderers.Length; i++)
            {
                ParticleSystemRenderer visualRenderer = renderers[i];
                Assert.IsNotNull(visualRenderer.sharedMaterial, $"{label} asset VFX material should be assigned.");
                Assert.AreNotSame(
                    expectedMaterial,
                    visualRenderer.sharedMaterial,
                    $"{label} asset particle material should not be overwritten by the pattern color carrier.");
                AssertGameOwnedAsset(visualRenderer.sharedMaterial, $"{label} asset VFX material");
                AssertRenderableMaterialShader(visualRenderer.sharedMaterial, $"{label} asset VFX material shader");
            }
        }

        private static void AssertBossBarrageProjectileVisible(GameObject projectileObject, string label)
        {
            Assert.IsNotNull(projectileObject, $"{label} should be assigned.");
            MeshRenderer renderer = projectileObject.GetComponent<MeshRenderer>();
            Assert.IsNotNull(renderer, $"{label} should keep a hidden collision root MeshRenderer.");
            Assert.IsFalse(renderer.enabled, $"{label} root MeshRenderer must stay disabled so child VFX defines the shot.");
            Assert.IsNotNull(renderer.sharedMaterial, $"{label} should keep a game-owned root material for editor repair.");
            AssertGameOwnedAsset(renderer.sharedMaterial, $"{label} hidden root material");
            AssertRenderableMaterialShader(renderer.sharedMaterial, $"{label} hidden root material shader");
            AssertPromotedParticleVfx(
                projectileObject.transform.Find("BossBarrageProjectileVfx_MagicMissilesFireShot"),
                $"{label} MagicMissiles fire shot",
                2);
            TrailRenderer trail = projectileObject.GetComponent<TrailRenderer>();
            Assert.IsNotNull(trail, $"{label} should include a TrailRenderer for incoming shot readability.");
            Assert.IsNotNull(trail.sharedMaterial, $"{label} trail should keep a visible material.");
            AssertGameOwnedAsset(trail.sharedMaterial, $"{label} trail material");
            AssertRenderableMaterialShader(trail.sharedMaterial, $"{label} trail material shader");
        }

        private static void AssertSummonEntryCueVfx(GameObject entryCuePrefab)
        {
            MeshRenderer rootRenderer = entryCuePrefab.GetComponent<MeshRenderer>();
            Assert.IsNotNull(rootRenderer, "summon entry cue should keep its root renderer for editor repair.");
            Assert.IsFalse(rootRenderer.enabled, "summon entry cue should hide its primitive root.");
            AssertPromotedParticleVfx(
                entryCuePrefab.transform.Find("SummonEntryVfx_MagicMissilesArcaneCircle"),
                "summon entry MagicMissiles circle",
                2);
        }

        private static void AssertSummonActorVfx(GameObject actorPrefab, bool expectPressureScreen, string label)
        {
            AssertPromotedParticleVfx(
                actorPrefab.transform.Find("SummonPulseVfx_MagicMissilesPulse"),
                $"{label} MagicMissiles pulse",
                1);
            AssertPromotedParticleVfx(
                FindChildWithPrefix(actorPrefab.transform, "SummonStateVfx_"),
                $"{label} MagicMissiles state aura",
                1);
            if (!expectPressureScreen)
            {
                return;
            }

            AssertPromotedParticleVfx(
                actorPrefab.transform.Find("SummonShieldVfx_MagicMissilesShieldCircle"),
                $"{label} MagicMissiles shield circle",
                2);
        }

        private static void AssertBossBarrageCombatCueAssetOverlays()
        {
            CombatVfxCueProfile profile = LoadAsset<CombatVfxCueProfile>(
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.PlayerRangedProjectileImpact,
                "CueAssetVfx_MagicMissilesLightImpact",
                "player ranged impact MagicMissiles overlay");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EnemyHit,
                "CueAssetVfx_MagicMissilesLightImpact",
                "enemy hit MagicMissiles overlay");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EnemyDeath,
                "CueAssetVfx_MagicMissilesDeathBurst",
                "enemy death MagicMissiles overlay");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteShieldSignal,
                "CueAssetVfx_MagicMissilesGuardState",
                "elite shield MagicMissiles overlay");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteAuraSignal,
                "CueAssetVfx_MagicMissilesActiveAura",
                "elite aura MagicMissiles overlay");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.EliteSummonSignal,
                "CueAssetVfx_MagicMissilesSummonState",
                "elite summon MagicMissiles overlay");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.SummonBlockOpportunity,
                "CueAssetVfx_MagicMissilesSummonState",
                "summon block opportunity MagicMissiles overlay");
            AssertCombatCueAssetOverlay(
                profile,
                CombatVfxCueId.SummonFollowupWindow,
                "CueAssetVfx_MagicMissilesSummonState",
                "summon follow-up window MagicMissiles overlay");
        }

        private static void AssertCombatCueAssetOverlay(
            CombatVfxCueProfile profile,
            CombatVfxCueId cueId,
            string childName,
            string label)
        {
            Assert.IsTrue(profile.TryGetCue(cueId, out CombatVfxCue cue), $"{cueId} should exist.");
            Assert.IsNotNull(cue.Prefab, $"{cueId} should keep a cue prefab.");
            AssertPromotedParticleVfx(cue.Prefab.transform.Find(childName), label, 1);
            AssertGameOwnedAsset(cue.Prefab, $"{cueId} cue prefab");
        }

        private static void AssertMagicMissilesLaneProjectile(string prefabPath, string childName, string label)
        {
            GameObject projectileObject = LoadAsset<GameObject>(prefabPath);
            Assert.IsNotNull(projectileObject, $"{label} prefab should be assigned.");
            MeshRenderer rootRenderer = projectileObject.GetComponent<MeshRenderer>();
            Assert.IsNotNull(rootRenderer, $"{label} should keep a hidden collision root MeshRenderer.");
            Assert.IsFalse(rootRenderer.enabled, $"{label} root MeshRenderer must stay disabled behind asset VFX.");
            Assert.IsNull(
                projectileObject.GetComponent<TrailRenderer>(),
                $"{label} should not fall back to generated TrailRenderer visuals.");
            AssertPromotedParticleVfx(
                projectileObject.transform.Find(childName),
                label,
                2);
        }

        private static void AssertPromotedParticleVfx(Transform root, string label, int minimumParticleSystems)
        {
            Assert.IsNotNull(root, $"{label} should be authored in the prefab.");
            Assert.IsNull(root.GetComponentInChildren<Collider>(true), $"{label} should stay visual-only.");
            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
            Assert.GreaterOrEqual(
                particles.Length,
                minimumParticleSystems,
                $"{label} should preserve its authored particle system stack.");
            ParticleSystemRenderer[] renderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
            Assert.Greater(renderers.Length, 0, $"{label} should expose particle renderers.");
            for (int i = 0; i < renderers.Length; i++)
            {
                ParticleSystemRenderer renderer = renderers[i];
                Assert.IsNotNull(renderer.sharedMaterial, $"{label}.{renderer.name} should keep a material.");
                AssertGameOwnedAsset(renderer.sharedMaterial, $"{label}.{renderer.name} material");
                AssertRenderableMaterialShader(renderer.sharedMaterial, $"{label}.{renderer.name} material shader");
                if (renderer.mesh != null)
                {
                    AssertGameOwnedAsset(renderer.mesh, $"{label}.{renderer.name} mesh");
                }
            }

            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                if (audioSource.clip == null)
                {
                    continue;
                }

                AssertGameOwnedAsset(audioSource.clip, $"{label}.{audioSource.name} audio clip");
            }
        }

        private static Transform FindChildWithPrefix(Transform parent, string prefix)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static void AssertColorNear(Color expected, Color actual, string label)
        {
            Assert.AreEqual(expected.r, actual.r, 0.001f, $"{label} red channel.");
            Assert.AreEqual(expected.g, actual.g, 0.001f, $"{label} green channel.");
            Assert.AreEqual(expected.b, actual.b, 0.001f, $"{label} blue channel.");
            Assert.AreEqual(expected.a, actual.a, 0.001f, $"{label} alpha channel.");
        }

        private static void AssertVectorNear(Vector3 expected, Vector3 actual, string label)
        {
            Assert.AreEqual(expected.x, actual.x, 0.001f, $"{label} x.");
            Assert.AreEqual(expected.y, actual.y, 0.001f, $"{label} y.");
            Assert.AreEqual(expected.z, actual.z, 0.001f, $"{label} z.");
        }

        private static Color ReadMaterialColor(Material material)
        {
            Assert.IsNotNull(material);
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }

            Assert.Fail($"{material.name} should expose _BaseColor or _Color for projectile readability.");
            return Color.clear;
        }

        private static void AssertBossPatternTightensForwardRisk(BossBarragePatternProfile pattern)
        {
            Assert.IsNotNull(pattern);
            float backlineHalfSpread = pattern.EvaluateHalfSpread(0f);
            float forwardHalfSpread = pattern.EvaluateHalfSpread(1f);
            Assert.Less(
                forwardHalfSpread,
                backlineHalfSpread,
                $"{pattern.PatternId} should keep forward-risk lateral gaps tighter than backline gaps.");

            float backlineWidth = ResolveBossPatternWidth(pattern, 0f);
            float forwardWidth = ResolveBossPatternWidth(pattern, 1f);
            Assert.Less(
                forwardWidth,
                backlineWidth,
                $"{pattern.PatternId} should preview/fire with narrower forward-risk spacing than backline spacing.");

            if (!BossPatternUsesDepthSpacing(pattern.LateralShape))
            {
                return;
            }

            float backlineDepthWidth = ResolveBossPatternDepthWidth(pattern, 0f);
            float forwardDepthWidth = ResolveBossPatternDepthWidth(pattern, 1f);
            Assert.Greater(backlineDepthWidth, 0f, $"{pattern.PatternId} should use target-depth spacing.");
            Assert.Less(
                forwardDepthWidth,
                backlineDepthWidth,
                $"{pattern.PatternId} should tighten target-depth spacing near the forward-risk boundary.");
        }

        private static float ResolveBossPatternWidth(BossBarragePatternProfile pattern, float forwardRisk01)
        {
            int count = Mathf.Max(1, pattern.ProjectilesPerWave);
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                float offset = pattern.GetLateralOffset(i, count, forwardRisk01);
                min = Mathf.Min(min, offset);
                max = Mathf.Max(max, offset);
            }

            return max - min;
        }

        private static float ResolveBossPatternDepthWidth(BossBarragePatternProfile pattern, float forwardRisk01)
        {
            int count = Mathf.Max(1, pattern.ProjectilesPerWave);
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                float offset = pattern.GetTargetDepthOffset(i, count, forwardRisk01);
                min = Mathf.Min(min, offset);
                max = Mathf.Max(max, offset);
            }

            return max - min;
        }

        private static bool BossPatternUsesDepthSpacing(BossBarrageLateralShape shape)
        {
            return shape == BossBarrageLateralShape.LinePressure
                || shape == BossBarrageLateralShape.EscortScreen
                || shape == BossBarrageLateralShape.LayeredSalvo
                || shape == BossBarrageLateralShape.StaggeredCrossfire;
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

        private static BossBarrageProjectile RequireActiveBossProjectileWithMaterial(Material material)
        {
            BossBarrageProjectile[] bossProjectiles = Object.FindObjectsByType<BossBarrageProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < bossProjectiles.Length; i++)
            {
                if (bossProjectiles[i].IsActive
                    && bossProjectiles[i].SourceTeam == DamageTeam.Enemy
                    && bossProjectiles[i].LastPresentationMaterial == material)
                {
                    return bossProjectiles[i];
                }
            }

            Assert.Fail("Expected an active enemy boss projectile with the requested presentation material.");
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

        private static void AssertInoriAvatarUsesAuthoredMapping()
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(InoriModelPath) as ModelImporter;
            Assert.IsNotNull(modelImporter, $"Missing ModelImporter for {InoriModelPath}.");
            Assert.AreEqual(ModelImporterAnimationType.Human, modelImporter.animationType);
            Assert.AreEqual(ModelImporterAvatarSetup.CreateFromThisModel, modelImporter.avatarSetup);
            AssertHumanBoneMapped(modelImporter, "hand.r", "RightHand");
            AssertHumanBoneMapped(modelImporter, "hand.l", "LeftHand");
        }

        private static void AssertHumanBoneMapped(ModelImporter importer, string boneName, string humanName)
        {
            HumanBone[] humanBones = importer.humanDescription.human;
            for (int i = 0; i < humanBones.Length; i++)
            {
                if (humanBones[i].boneName == boneName && humanBones[i].humanName == humanName)
                {
                    return;
                }
            }

            Assert.Fail($"{boneName} must be mapped as {humanName} on the promoted Inori avatar.");
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

        private static void AssertAnimatorStateUsesMotion(
            AnimatorStateMachine stateMachine,
            string stateName,
            Motion expectedMotion)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state.name == stateName)
                {
                    Assert.AreSame(expectedMotion, states[i].state.motion, $"{stateName} should use the expected promoted clip.");
                    return;
                }
            }

            Assert.Fail($"Missing Animator state {stateName}.");
        }

        private static void AssertRifleGirlControllerHasMotion(AnimatorStateMachine stateMachine, Motion expectedMotion)
        {
            Assert.IsNotNull(expectedMotion, "Expected RifleGirl motion should exist.");
            if (StateMachineUsesMotion(stateMachine, expectedMotion))
            {
                return;
            }

            Assert.Fail($"{stateMachine.name} should reference promoted RifleGirl motion {expectedMotion.name}.");
        }

        private static bool StateMachineUsesMotion(AnimatorStateMachine stateMachine, Motion expectedMotion)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (MotionUsesExpectedClip(states[i].state.motion, expectedMotion))
                {
                    return true;
                }
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = 0; i < childMachines.Length; i++)
            {
                if (StateMachineUsesMotion(childMachines[i].stateMachine, expectedMotion))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MotionUsesExpectedClip(Motion motion, Motion expectedMotion)
        {
            if (motion == null)
            {
                return false;
            }

            if (motion == expectedMotion)
            {
                return true;
            }

            if (motion is not BlendTree blendTree)
            {
                return false;
            }

            ChildMotion[] children = blendTree.children;
            for (int i = 0; i < children.Length; i++)
            {
                if (MotionUsesExpectedClip(children[i].motion, expectedMotion))
                {
                    return true;
                }
            }

            return false;
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

        private static void AssertFallbackCombatModeVisual(
            GameObject rangedRoot,
            Animator rangedAnimator,
            GameObject rangedWeaponRoot,
            GameObject meleeVisualRoot)
        {
            Assert.AreEqual(RangedPlayerVisualRootName, rangedRoot.name);
            Assert.AreSame(
                LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath),
                rangedAnimator.runtimeAnimatorController);
            AssertGameOwnedAsset(rangedAnimator.runtimeAnimatorController, "RifleGirl player Animator Controller");
            AssertGameOwnedAsset(rangedAnimator.avatar, "RifleGirl ranged Avatar");
            AnimatorController rangedController = LoadAsset<AnimatorController>(RifleGirlRangedControllerPath);
            Assert.IsTrue(
                rangedController.layers[0].iKPass,
                "RifleGirl ranged controller must keep IK pass enabled for support-hand correction.");
            AnimatorStateMachine inoriStateMachine = rangedController.layers[0].stateMachine;
            Assert.IsNotNull(
                inoriStateMachine.defaultState,
                "RifleGirl ranged controller should preserve an explicit default state.");
            AssertRifleGirlControllerHasMotion(inoriStateMachine, LoadAsset<AnimationClip>(RifleGirlIdleClipPath));
            AssertRifleGirlControllerHasMotion(inoriStateMachine, LoadAsset<AnimationClip>(RifleGirlAimIdleClipPath));
            AssertControllerUsesGameOwnedMotions(rangedController);
            AssertRifleGirlAvatarUsesAuthoredMapping();

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
            Assert.AreNotSame(rangedRoot.transform, weapon.parent, "Ranged weapon should stay under the RifleGirl authored hierarchy.");
            Assert.IsTrue(weapon.IsChildOf(rangedAnimator.transform), "Ranged weapon should be parented to the active RifleGirl player model.");
            Assert.Greater(
                weapon.GetComponentsInChildren<Renderer>(true).Length,
                0,
                "Ranged weapon should be a visible promoted model, not a hidden data marker.");
            RifleGirlWeaponSocketDriver weaponSocketDriver =
                rangedAnimator.GetComponent<RifleGirlWeaponSocketDriver>();
            Assert.IsNotNull(weaponSocketDriver, "Ranged visual should bind a game-owned RifleGirl rifle socket driver.");
            Assert.IsTrue(weaponSocketDriver.IsConfigured, "Ranged visual weapon socket driver should be fully configured.");
            Assert.AreSame(rangedAnimator, GetObjectReference<Animator>(weaponSocketDriver, "animator"));
            ParentConstraint weaponConstraint = weapon.GetComponent<ParentConstraint>();
            Assert.IsNotNull(weaponConstraint, "RifleGirl rifle should keep its authored ParentConstraint.");
            Assert.AreSame(
                weaponConstraint,
                GetObjectReference<ParentConstraint>(weaponSocketDriver, "rifleConstraint"));
            Assert.AreEqual("To_Hand_R_Socket, IK_ON_Left_Handle", GetString(weaponSocketDriver, "defaultCommands"));
            Assert.AreEqual(
                1f,
                GetFloat(weaponSocketDriver, "leftIkMaxWeight"),
                0.001f,
                "RifleGirl rifle socket should use full support-hand IK from the authored setup.");
            Assert.AreEqual(
                1f,
                GetFloat(weaponSocketDriver, "leftIkRotationMaxWeight"),
                0.001f,
                "RifleGirl rifle socket should preserve the authored wrist rotation.");
            Transform leftHandle = FindDescendant(weapon, "Left_Handle");
            Assert.IsNotNull(leftHandle, "Rifle should expose Left_Handle for later authored support-hand pose work.");
            Assert.AreSame(leftHandle, GetObjectReference<Transform>(weaponSocketDriver, "leftHandIkTarget"));
            Assert.AreEqual(
                AnimatorCullingMode.AlwaysAnimate,
                rangedAnimator.cullingMode,
                "RifleGirl ranged Animator should always update so rifle pose and support-hand IK stay stable.");
            Assert.IsNotNull(
                FindLikelyHand(rangedRoot.transform, rightHand: true),
                "RifleGirl ranged visual should expose a right hand bone for weapon parenting.");
            Assert.IsNotNull(
                FindLikelyHand(rangedRoot.transform, rightHand: false),
                "RifleGirl ranged visual should expose a left hand bone for support-hand IK.");
            Assert.Greater(
                meleeVisualRoot.GetComponentsInChildren<Renderer>(true).Length,
                0,
                "CombatGirl melee visual should contain visible sword/shield renderers.");
            AssertAnimatorParameter(rangedAnimator, "IDLE 0", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "IDLE", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "SHOOT", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "WALK", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "RUN", AnimatorControllerParameterType.Trigger);
            AssertAnimatorParameter(rangedAnimator, "WALK F", AnimatorControllerParameterType.Trigger);
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

        private static Transform FindLikelyHand(Transform root, bool rightHand)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                string normalized = NormalizeTransformNameForLookup(children[i].name);
                if (rightHand)
                {
                    if (normalized == "handr" ||
                        normalized == "righthand" ||
                        normalized == "handright" ||
                        normalized == "handsocketr")
                    {
                        return children[i];
                    }
                }
                else if (normalized == "handl" ||
                         normalized == "lefthand" ||
                         normalized == "handleft" ||
                         normalized == "handsocketl")
                {
                    return children[i];
                }
            }

            return null;
        }

        private static string NormalizeTransformNameForLookup(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("_", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace(".", string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLowerInvariant();
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

        private static void AssertVefectsFlipbookMaterial(Material material, string label)
        {
            AssertGameOwnedAsset(material.shader, $"{label} shader");
            Assert.IsTrue(material.HasProperty("_Flipbook"), $"{label} should use the promoted Vefects flipbook shader.");
            Texture flipbook = material.GetTexture("_Flipbook");
            Assert.IsNotNull(flipbook, $"{label} should keep an assigned Vefects flipbook texture.");
            AssertGameOwnedAsset(flipbook, $"{label} flipbook texture");
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

        private static void AssertLaneAmbientVfx(GameObject root)
        {
            AssertVisualOnlySceneVfx(
                root.transform,
                "AmbientFlow_LeftRail_00",
                LaneAmbientFlowMaterialPath,
                expectMotion: true,
                expectFloating: false);
            AssertVisualOnlySceneVfx(
                root.transform,
                "AmbientFlow_RightRail_00",
                LaneAmbientFlowMaterialPath,
                expectMotion: true,
                expectFloating: false);
            AssertVisualOnlySceneVfx(
                root.transform,
                "AmbientDepthTick_04",
                BossPressureHorizonMaterialPath,
                expectMotion: false,
                expectFloating: false);
            AssertVisualOnlySceneVfx(
                root.transform,
                "BossPressureHorizon_Curtain",
                BossPressureHorizonMaterialPath,
                expectMotion: true,
                expectFloating: false);
            AssertVisualOnlySceneVfx(
                root.transform,
                "SummonRouteWisp_00",
                SummonRouteWispMaterialPath,
                expectMotion: false,
                expectFloating: true);
            AssertVisualOnlySceneVfx(
                root.transform,
                "SummonRouteWisp_03",
                SummonRouteWispMaterialPath,
                expectMotion: false,
                expectFloating: true);
        }

        private static void AssertVisualOnlySceneVfx(
            Transform root,
            string childName,
            string materialPath,
            bool expectMotion,
            bool expectFloating)
        {
            Transform child = root.Find(childName);
            Assert.IsNotNull(child, $"{childName} should be authored under the ambient VFX root.");
            Assert.IsNull(child.GetComponent<Collider>(), $"{childName} should stay visual-only.");

            Renderer renderer = RequireComponent<Renderer>(child.gameObject, childName);
            Assert.AreSame(LoadAsset<Material>(materialPath), renderer.sharedMaterial, $"{childName} should use its authored material.");
            AssertGameOwnedAsset(renderer.sharedMaterial, $"{childName} material");
            AssertRenderableMaterialShader(renderer.sharedMaterial, $"{childName} material shader");
            if (expectMotion)
            {
                Assert.IsNotNull(
                    child.GetComponent<ActionFoundationArenaTransformMotion>(),
                    $"{childName} should use ambient transform motion.");
            }

            if (expectFloating)
            {
                Assert.IsNotNull(
                    child.GetComponent<ActionFoundationArenaFloatingShape>(),
                    $"{childName} should use floating pulse motion.");
            }
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

        private static Vector3 GetVector3(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).vector3Value;
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

        private static IEnumerator WaitSeconds(float seconds)
        {
            float remaining = seconds;
            while (remaining > 0f)
            {
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        private static float DistanceFromRayToBounds(Vector3 rayOrigin, Vector3 rayDirection, Bounds bounds)
        {
            Vector3 direction = rayDirection.sqrMagnitude > 0.0001f
                ? rayDirection.normalized
                : Vector3.forward;
            Vector3 toCenter = bounds.center - rayOrigin;
            float projectedDistance = Mathf.Max(0f, Vector3.Dot(toCenter, direction));
            Vector3 closestPointOnRay = rayOrigin + direction * projectedDistance;
            return Mathf.Sqrt(bounds.SqrDistance(closestPointOnRay));
        }

        private static FieldInfo RequirePrivateField(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"{target.GetType().Name} should define private field {fieldName}.");
            return field;
        }

        private static void AssertNoPrivateField<T>(string fieldName)
        {
            FieldInfo field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNull(field, $"{typeof(T).Name} should not keep temporary keyboard fallback field {fieldName}.");
        }

        private static float GetFloat(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).floatValue;
        }

        private static int GetInt(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).intValue;
        }

        private static T GetEnum<T>(Object target, string propertyName) where T : System.Enum
        {
            int value = RequireProperty(new SerializedObject(target), propertyName).enumValueIndex;
            return (T)System.Enum.ToObject(typeof(T), value);
        }

        private static int GetEnumIndex(Object target, string propertyName)
        {
            return RequireProperty(new SerializedObject(target), propertyName).enumValueIndex;
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
