using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationBossBarrageLaneReviewSetup
    {
        public const string ReviewScenePath = "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity";
        public const string PatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_NeedleLock.asset";
        public const string CoverFirePatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_CoverFire.asset";
        public const string EscortScreenPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_EscortScreen.asset";
        public const string LayeredSalvoPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_LayeredSalvo.asset";
        public const string StaggeredCrossfirePatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_StaggeredCrossfire.asset";
        public const string TwinSweepPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_TwinSweep.asset";
        public const string LeftClampPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_LeftClamp.asset";
        public const string RightClampPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_RightClamp.asset";
        public const string PunishNetPatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_PunishNet.asset";
        public const string LinePressurePatternProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossBarrage_LinePressure.asset";
        public const string ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossBarrageProjectile_NeedleLock.prefab";
        public const string LocalDefenseProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_PlayerAction_BossBarrageLocalDefense.asset";
        public const string MeleeActionProfilePath =
            ActionFoundationProfileSetup.PlayerActionProfilePath;
        public const string ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile.mat";
        public const string Skill1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerSkill1Projectile_LaneBolt.prefab";
        public const string RangedBasicProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab";
        public const string SummonSlot1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";
        public const string SummonSlot1EntryCuePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1EntryCue_MagicCircle.prefab";
        public const string SummonSlot1ActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab";
        public const string BossSummonPressureActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_BossSummonPressureActor_Proxy.prefab";
        public const string SummonSlot1ActionProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonSlot1_ShieldBreaker.asset";
        public const string BossSummonPressureProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossSummonPressure_SummonCaller.asset";
        public const string BossPressureActionDeckProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_BossPressureActionDeck_PocketReview.asset";
        public const string SummonOpportunityProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonOpportunity_BossPressureBlock.asset";
        public const string SummonSlot1PresentationCandidateProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonPresentation_PlayerShieldBreaker.asset";
        public const string BossSummonPressurePresentationCandidateProfilePath =
            ActionFoundationProfileSetup.ProfileRoot + "/DB_SummonPresentation_BossAuraCaptain.asset";
        private const string SummonSlot1ActorVisualName = "SummonSlot1Visual_ShieldBreakerElite";
        private const string BossSummonPressureActorVisualName = "BossSummonPressureVisual_AuraCaptainElite";
        private const string SummonSlot1ActorVisualRoleId = "SciFiSoldier.Elite.ShieldBreaker";
        private const string BossSummonPressureActorVisualRoleId = "SciFiSoldier.Elite.AuraCaptain";
        private const string Skill1ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_PlayerSkill1Projectile.mat";
        private const string RangedBasicProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_PlayerRangedBasicProjectile.mat";
        private const string SummonSlot1ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1Projectile.mat";
        private const string SummonSlot1EntryCueMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1EntryCue.mat";
        private const string SummonSlot1ActorMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1Actor.mat";
        private const string SummonPressureScreenMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonPressureScreen.mat";
        private const string SummonSlot1ActorPulseMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1ActorPulse.mat";
        private const string BossSummonPressureActorMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossSummonPressureActor.mat";
        private const string BossSummonPressureScreenMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossSummonPressureScreen.mat";
        private const string BossSummonPressureActorPulseMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossSummonPressureActorPulse.mat";

        private const string ReviewRootPrefix = "BossBarrageLaneReview_";
        private const string LaneRootName = ReviewRootPrefix + "SummonLaneSpace";
        private const string BossProxyRootName = ReviewRootPrefix + "BossProxy_NeedleLock";
        private const string CloseThreatRootName = ReviewRootPrefix + "CloseThreat_ClosePunish";
        private const string ProjectilePoolRootName = ReviewRootPrefix + "ProjectilePool";
        private const string ActionCuePoolRootName = ReviewRootPrefix + "ActionCuePool";
        private const string SummonActorPoolRootName = ReviewRootPrefix + "SummonActorPool";
        private const string BossSummonActorPoolRootName = ReviewRootPrefix + "BossSummonActorPool";
        private const string PocketOwnerRootName = ReviewRootPrefix + "PocketOwner";
        private const string HudRootName = ReviewRootPrefix + "DebugHud";
        private const string MarkerRootName = ReviewRootPrefix + "Markers";
        private const string EnergyZoneRootName = ReviewRootPrefix + "EnergyRiskZones";
        private const string PocketClearMarkerName = ReviewRootPrefix + "PocketClearMarker";
        private const string PocketFailMarkerName = ReviewRootPrefix + "PocketFailMarker";
        private const string SummonEntryMarkerName = ReviewRootPrefix + "SummonEntryMarker";
        private const string BossProxyMarkerName = ReviewRootPrefix + "BossProxyMarker";
        private const string BossTelegraphRootName = ReviewRootPrefix + "BossBarrageTelegraphMarkers";
        private const string BossProxyHumanoidVisualName = ReviewRootPrefix + "HumanoidBossVisual_SummonCallerElite";
        private const string RangedPlayerVisualRootName = ReviewRootPrefix + "RangedVisual_RifleGirl";
        private const string RangedPlayerModelName = ReviewRootPrefix + "RangedModel_RifleGirl";
        private const string RangedPlayerWeaponName = ReviewRootPrefix + "RangedWeapon_Rifle";
        private const string MeleePlayerWeaponRootName = ReviewRootPrefix + "MeleeWeapons_CombatGirlSwordShield";
        private const string RifleGirlSourcePrefabPath =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack_RifleGirl/RifleGirl/Prefab/Rifle_Full_Body.prefab";
        private const string RifleGirlRangedControllerPath =
            ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateControllerPath;
        private const string CombatGirlAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";
        private const string BossProxyVisualMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossProxy.mat";
        private const string BossTelegraphMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageIncomingTelegraph.mat";
        private const string LaneRailMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageLaneRail.mat";
        private const string PlayerBoundaryMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarragePlayerBoundary.mat";
        private const string SummonBoundaryMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageSummonBoundary.mat";
        private const string BacklineEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageBacklineEnergyZone.mat";
        private const string MidEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageMidEnergyZone.mat";
        private const string ForwardEnergyZoneMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageForwardEnergyZone.mat";

        private static readonly Vector3 PlayerStartPosition = new Vector3(0f, 0f, -8.5f);
        private static readonly Vector3 CameraStartOffset = new Vector3(0.14f, 0.68f, -4.25f);
        private static readonly Vector3 CameraLookOffset = new Vector3(0f, 1.18f, 1.5f);
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

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Barrage Lane Review Scene")]
        public static void ReapplyBossBarrageLaneReviewSceneMenu()
        {
            EnsureBossBarrageLaneReviewScene();
            Debug.Log("Reapplied ActionFoundation boss barrage lane review scene.");
        }

        [MenuItem("DimensionBrawl/Validate Action Foundation Boss Barrage Lane Review Scene")]
        public static void ValidateBossBarrageLaneReviewSceneMenu()
        {
            ValidateBossBarrageLaneReviewScene();
            Debug.Log("ActionFoundation boss barrage lane review scene validation passed.");
        }

        public static void EnsureBossBarrageLaneReviewScene()
        {
            ActionFoundationPlayerCombatModeAssetSetup.EnsureRangedCandidateAssets();
            BossBarragePatternProfile patternProfile = EnsurePatternProfile();
            BossBarragePatternProfile coverFirePatternProfile = EnsureCoverFirePatternProfile();
            BossBarragePatternProfile escortScreenPatternProfile = EnsureEscortScreenPatternProfile();
            BossBarragePatternProfile layeredSalvoPatternProfile = EnsureLayeredSalvoPatternProfile();
            BossBarragePatternProfile staggeredCrossfirePatternProfile = EnsureStaggeredCrossfirePatternProfile();
            BossBarragePatternProfile twinSweepPatternProfile = EnsureTwinSweepPatternProfile();
            BossBarragePatternProfile leftClampPatternProfile = EnsureLeftClampPatternProfile();
            BossBarragePatternProfile rightClampPatternProfile = EnsureRightClampPatternProfile();
            BossBarragePatternProfile punishNetPatternProfile = EnsurePunishNetPatternProfile();
            BossBarragePatternProfile linePressurePatternProfile = EnsureLinePressurePatternProfile();
            BossBarrageProjectile projectilePrefab = EnsureProjectilePrefab();
            PlayerActionProfile localDefenseProfile = EnsureLocalDefenseProfile();
            LaneActionProjectile skill1ProjectilePrefab = EnsureLaneActionProjectilePrefab(
                Skill1ProjectilePrefabPath,
                "PF_PlayerSkill1Projectile_LaneBolt",
                Skill1ProjectileMaterialPath,
                new Color(0.45f, 0.9f, 1f, 1f),
                0.42f,
                allowVerticalTravel: false);
            LaneActionProjectile rangedBasicProjectilePrefab = EnsureLaneActionProjectilePrefab(
                RangedBasicProjectilePrefabPath,
                "PF_PlayerRangedBasicProjectile_AimBolt",
                RangedBasicProjectileMaterialPath,
                new Color(0.75f, 0.98f, 1f, 1f),
                0.28f,
                allowVerticalTravel: true);
            LaneActionProjectile summonSlot1ProjectilePrefab = EnsureLaneActionProjectilePrefab(
                SummonSlot1ProjectilePrefabPath,
                "PF_SummonSlot1Projectile_AssistBolt",
                SummonSlot1ProjectileMaterialPath,
                new Color(0.55f, 1f, 0.72f, 1f),
                0.58f,
                allowVerticalTravel: false);
            GameObject summonEntryCuePrefab = EnsureSummonEntryCuePrefab();
            SummonFrontlineProxy summonActorPrefab = EnsureSummonActorPrefab();
            SummonFrontlineProxy bossSummonActorPrefab = EnsureBossSummonPressureActorPrefab();
            EnsureSummonPresentationCandidateProfiles();
            Scene scene = EditorSceneManager.OpenScene(ActionFoundationProfileSetup.ScenePath, OpenSceneMode.Single);
            patternProfile = LoadAsset<BossBarragePatternProfile>(PatternProfilePath);
            coverFirePatternProfile = LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath);
            escortScreenPatternProfile = LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath);
            layeredSalvoPatternProfile = LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath);
            staggeredCrossfirePatternProfile = LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath);
            twinSweepPatternProfile = LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath);
            leftClampPatternProfile = LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath);
            rightClampPatternProfile = LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath);
            punishNetPatternProfile = LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath);
            linePressurePatternProfile = LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath);
            projectilePrefab = LoadPrefabComponent<BossBarrageProjectile>(ProjectilePrefabPath);
            localDefenseProfile = LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath);
            skill1ProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(Skill1ProjectilePrefabPath);
            rangedBasicProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(RangedBasicProjectilePrefabPath);
            summonSlot1ProjectilePrefab = LoadPrefabComponent<LaneActionProjectile>(SummonSlot1ProjectilePrefabPath);
            summonEntryCuePrefab = LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath);
            summonActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot1ActorPrefabPath);
            bossSummonActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(BossSummonPressureActorPrefabPath);
            RemoveReviewAndEnemyRoots(scene);

            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player.gameObject, "player action controller");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionCameraTargetBridge cameraTargetBridge = RequireObject<ActionCameraTargetBridge>(scene, "action camera target bridge");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");

            SummonLaneSpace laneSpace = CreateLaneSpace(scene);
            player.transform.SetPositionAndRotation(PlayerStartPosition, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            SetObjectReference(player, "laneSpace", laneSpace);

            SummonEnergyLadder energyLadder = EnsureComponent<SummonEnergyLadder>(player.gameObject);
            SetObjectReference(energyLadder, "laneSpace", laneSpace);
            SetObjectReference(energyLadder, "trackedPlayer", player.transform);

            GameObject projectileRoot = CreateRoot(scene, ProjectilePoolRootName);
            GameObject actionCueRoot = CreateRoot(scene, ActionCuePoolRootName);
            GameObject summonActorRoot = CreateRoot(scene, SummonActorPoolRootName);
            GameObject bossSummonActorRoot = CreateRoot(scene, BossSummonActorPoolRootName);
            GameObject bossProxy = CreateBossProxy(
                scene,
                laneSpace,
                patternProfile,
                coverFirePatternProfile,
                escortScreenPatternProfile,
                layeredSalvoPatternProfile,
                staggeredCrossfirePatternProfile,
                twinSweepPatternProfile,
                leftClampPatternProfile,
                rightClampPatternProfile,
                punishNetPatternProfile,
                linePressurePatternProfile,
                projectilePrefab,
                projectileRoot.transform,
                bossSummonActorPrefab,
                bossSummonActorRoot.transform);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            GameObject closeThreat = CreateCloseThreat(scene, laneSpace, player.transform, playerHealth, cameraController);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");
            ConfigureLocalDefenseProfile(playerActionController, localDefenseProfile);
            ConfigurePlayerEnergyActions(
                player.gameObject,
                playerHealth,
                targetSelector,
                bossHealth,
                energyLadder,
                laneSpace,
                skill1ProjectilePrefab,
                summonSlot1ProjectilePrefab,
                summonEntryCuePrefab,
                summonActorPrefab,
                projectileRoot.transform,
                actionCueRoot.transform,
                summonActorRoot.transform);
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            ConfigureTargetReferences(targetSelector, cameraTargetBridge, cameraController, player, playerHealth, closeThreatHealth, bossHealth);
            ConfigureEncounter(encounter, playerHealth, closeThreatHealth);
            BossBarrageEmitter bossBarrageEmitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            BossBarragePocketReviewOwner pocketOwner = CreatePocketOwner(
                scene,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                bossPressureCost,
                bossPressureActionDirector,
                laneSpace);
            ConfigureFixedRearCamera(cameraController, player.transform, bossProxy.transform, laneSpace.transform);
            PlayerCombatModeVisualBinding combatModeVisuals = CreatePlayerCombatModeVisuals(scene, player.gameObject);
            ConfigureCombatModeController(player.gameObject, playerActionController, player, localDefenseProfile, combatModeVisuals);
            ConfigureRangedAimController(player.gameObject, cameraController, combatModeVisuals.RangedAnimator);
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction = ConfigurePlayerRangedBasicAttack(
                player.gameObject,
                combatModeController,
                rangedAimController,
                player,
                targetSelector,
                playerHealth,
                cameraController,
                combatModeVisuals.RangedAnimator,
                rangedBasicProjectilePrefab,
                projectileRoot.transform,
                combatModeVisuals.RangedFireOrigin);
            ConfigureRifleGirlNativeBridge(
                combatModeVisuals.NativeAnimatorBridge,
                combatModeVisuals.RangedAnimator,
                player,
                playerActionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction);
            ConfigureCombatModeActionLinks(combatModeController, rangedAimController, rangedBasicAttackAction);
            CreateReviewHud(
                scene,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                laneSpace,
                player.transform,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                pocketOwner,
                bossPressureCost,
                RequireComponent<BossPressurePositionController>(bossBarrageEmitter.gameObject, "boss pressure position controller"),
                bossPressureActionDirector,
                bossSummonPressureAction);
            ConfigureActionCameraCueDriver(
                cameraController,
                playerActionController,
                player,
                skill1Action,
                summonSlot1Action);
            ConfigurePocketCueBridges(
                pocketOwner,
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver"),
                RequireComponent<PlayerCombatVfxCueDriver>(player.gameObject, "player combat VFX cue driver"),
                RequireComponent<CombatVfxCuePlayer>(player.gameObject, "player combat VFX cue player"),
                bossProxy.transform);
            ConfigureBossBarrageCameraCueDriver(
                cameraController,
                bossBarrageEmitter,
                bossPressureActionDirector,
                player.transform);
            ConfigureArenaInfluenceTargets(scene, player.transform, bossProxy.transform, closeThreat.transform);
            CreateLaneMarkers(scene, laneSpace);
            CreateEnergyRiskZoneMarkers(scene, laneSpace);
            CreateBossBarrageTelegraphMarkers(scene, laneSpace, bossBarrageEmitter);
            // Keep the serialized default aligned with the ranged starting mode after all visual swaps are rebuilt.
            ConfigureLocalDefenseProfile(playerActionController, localDefenseProfile);

            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save boss barrage lane review scene at {ReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        public static void ValidateBossBarrageLaneReviewScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerActionController playerActionController =
                RequireComponent<PlayerActionController>(player.gameObject, "player action controller");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionCameraController cameraController = RequireObject<ActionCameraController>(scene, "action camera");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            SummonLaneSpace laneSpace = RequireComponent<SummonLaneSpace>(RequireRoot(scene, LaneRootName), "lane space");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            BossPressurePositionController bossPressurePosition =
                RequireComponent<BossPressurePositionController>(bossProxy, "boss pressure position controller");
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            ValidateBossProxyVisual(bossProxy);
            GameObject closeThreat = RequireRoot(scene, CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedAimController rangedAimController =
                RequireComponent<PlayerRangedAimController>(player.gameObject, "player ranged aim controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(scene, PocketOwnerRootName), "boss barrage pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(scene, HudRootName), "boss barrage review HUD");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(RequireRoot(scene, HudRootName), "boss barrage mobile review HUD");
            ActionCameraCueDriver actionCameraCueDriver =
                RequireComponent<ActionCameraCueDriver>(cameraController.gameObject, "action camera cue driver");
            PlayerCombatVfxCueDriver playerVfxCueDriver =
                RequireComponent<PlayerCombatVfxCueDriver>(player.gameObject, "player combat VFX cue driver");
            CombatVfxCuePlayer playerCuePlayer =
                RequireComponent<CombatVfxCuePlayer>(player.gameObject, "player combat VFX cue player");

            ValidateObjectReference(player, "laneSpace", laneSpace);
            ValidateObjectReference(playerActionController, "actionProfile", LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath));
            ValidateCombatModeController(
                combatModeController,
                playerActionController,
                player,
                rangedAimController,
                rangedBasicAttackAction);
            Animator rangedAnimator = RequireReferencedObject<Animator>(combatModeController, "rangedAnimator");
            ValidateRangedAimController(rangedAimController, combatModeController, cameraController, rangedAnimator);
            ValidatePlayerRangedBasicAttack(
                rangedBasicAttackAction,
                combatModeController,
                rangedAimController,
                player,
                targetSelector,
                playerHealth,
                cameraController,
                rangedAnimator,
                RequireRoot(scene, ProjectilePoolRootName).transform,
                RequireReferencedObject<Transform>(rangedBasicAttackAction, "fireOrigin"));
            ValidateRifleGirlNativeBridge(
                RequireComponent<RifleGirlNativeGameplayAnimatorBridge>(
                    rangedAnimator.gameObject,
                    "RifleGirl native gameplay animator bridge"),
                rangedAnimator,
                player,
                playerActionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction);
            ValidateObjectReference(energyLadder, "laneSpace", laneSpace);
            ValidateObjectReference(energyLadder, "trackedPlayer", player.transform);
            ValidatePlayerEnergyActions(skill1Action, summonSlot1Action, energyLadder, playerHealth, targetSelector, bossHealth, laneSpace);
            ValidateObjectReference(emitter, "laneSpace", laneSpace);
            ValidateObjectReference(emitter, "trackedPlayer", player.transform);
            ValidateObjectReference(emitter, "sourceHealth", bossHealth);
            ValidateObjectReference(emitter, "patternProfile", LoadAsset<BossBarragePatternProfile>(PatternProfilePath));
            ValidateArrayReference(emitter, "patternSequence", 0, LoadAsset<BossBarragePatternProfile>(PatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                1,
                LoadAsset<BossBarragePatternProfile>(CoverFirePatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                2,
                LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                3,
                LoadAsset<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                4,
                LoadAsset<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                5,
                LoadAsset<BossBarragePatternProfile>(TwinSweepPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                6,
                LoadAsset<BossBarragePatternProfile>(LeftClampPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                7,
                LoadAsset<BossBarragePatternProfile>(RightClampPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                8,
                LoadAsset<BossBarragePatternProfile>(PunishNetPatternProfilePath));
            ValidateArrayReference(
                emitter,
                "patternSequence",
                9,
                LoadAsset<BossBarragePatternProfile>(LinePressurePatternProfilePath));
            ValidateInt(emitter, "wavesPerPattern", 1);
            ValidateObjectReference(emitter, "projectilePrefabObject", LoadAsset<GameObject>(ProjectilePrefabPath));
            ValidateBossPressureLoop(
                bossPressureCost,
                bossPressureActionDirector,
                bossSummonPressureAction,
                bossPressurePosition,
                laneSpace,
                bossProxy.transform,
                emitter,
                player.transform);
            ValidateObjectReference(targetSelector, "selfHealth", playerHealth);
            ValidateArrayReference(targetSelector, "targetCandidates", 0, closeThreatHealth);
            ValidateArrayReference(targetSelector, "targetCandidates", 1, bossHealth);
            ValidateCloseThreat(closeThreat, closeThreatHealth, playerHealth, cameraController);
            ValidateObjectReference(cameraController, "target", player.transform);
            ValidateObjectReference(cameraController, "threat", bossProxy.transform);
            ValidateActionCameraCueDriver(
                actionCameraCueDriver,
                playerActionController,
                player,
                cameraController,
                skill1Action,
                summonSlot1Action);
            ValidateBossBarrageCameraCueDriver(
                RequireComponent<BossBarrageCameraCueDriver>(cameraController.gameObject, "boss barrage camera cue driver"),
                cameraController,
                emitter,
                player.transform);
            ValidateBossBarrageLaneTelegraphPresenter(
                RequireComponent<BossBarrageLaneTelegraphPresenter>(
                    RequireRoot(scene, BossTelegraphRootName),
                    "boss barrage lane telegraph presenter"),
                emitter,
                laneSpace);
            ValidateEnergyRiskZoneMarkers(scene, laneSpace);
            ValidateObjectReference(encounter, "playerHealth", playerHealth);
            ValidateObjectReference(encounter, "enemyHealth", closeThreatHealth);
            ValidatePocketOwner(
                pocketOwner,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                emitter,
                bossPressureCost,
                bossPressureActionDirector);
            ValidatePocketCueBridges(
                pocketOwner,
                actionCameraCueDriver,
                playerVfxCueDriver,
                playerCuePlayer,
                bossProxy.transform);
            ValidateReviewHud(
                reviewHud,
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                laneSpace,
                player.transform,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                emitter,
                pocketOwner,
                bossPressureCost,
                bossPressurePosition,
                bossPressureActionDirector,
                bossSummonPressureAction);
            ValidateMobileReviewHud(
                mobileHud,
                player,
                playerActionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action);
            ValidateFixedRearCamera(cameraController, player.transform, laneSpace.transform);
            ValidateSummonForwardSpace(laneSpace);
            ValidateSummonPresentationCandidateProfiles();
            ValidateNoImportedAssetReference(ProjectilePrefabPath);
            ValidateNoImportedAssetReference(PatternProfilePath);
            ValidateNoImportedAssetReference(CoverFirePatternProfilePath);
            ValidateNoImportedAssetReference(EscortScreenPatternProfilePath);
            ValidateNoImportedAssetReference(LayeredSalvoPatternProfilePath);
            ValidateNoImportedAssetReference(StaggeredCrossfirePatternProfilePath);
            ValidateNoImportedAssetReference(TwinSweepPatternProfilePath);
            ValidateNoImportedAssetReference(LeftClampPatternProfilePath);
            ValidateNoImportedAssetReference(RightClampPatternProfilePath);
            ValidateNoImportedAssetReference(PunishNetPatternProfilePath);
            ValidateNoImportedAssetReference(LinePressurePatternProfilePath);
            ValidateNoImportedAssetReference(LocalDefenseProfilePath);
            ValidateNoImportedAssetReference(Skill1ProjectilePrefabPath);
            ValidateNoImportedAssetReference(RangedBasicProjectilePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1ProjectilePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1EntryCuePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1ActorPrefabPath);
            ValidateNoImportedAssetReference(BossSummonPressureActorPrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1PresentationCandidateProfilePath);
            ValidateNoImportedAssetReference(BossSummonPressurePresentationCandidateProfilePath);
            ValidateNoImportedAssetReference(SummonPressureScreenMaterialPath);
            ValidateNoImportedAssetReference(SummonSlot1ActorPulseMaterialPath);
            ValidateNoImportedAssetReference(BossSummonPressureActorMaterialPath);
            ValidateNoImportedAssetReference(BossSummonPressureScreenMaterialPath);
            ValidateNoImportedAssetReference(BossSummonPressureActorPulseMaterialPath);
            ValidateNoImportedAssetReference(BossTelegraphMaterialPath);
            ValidateNoImportedAssetReference(BacklineEnergyZoneMaterialPath);
            ValidateNoImportedAssetReference(MidEnergyZoneMaterialPath);
            ValidateNoImportedAssetReference(ForwardEnergyZoneMaterialPath);
        }

        private static BossBarragePatternProfile EnsurePatternProfile()
        {
            EnsureFolderForAsset(PatternProfilePath);
            BossBarragePatternProfile profile = AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(PatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, PatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "NeedleLock";
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.CenterSpread;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.8f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 0.75f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 4.8f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 3;
            RequireProperty(serializedObject, "damage").floatValue = 18f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.5f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 4.6f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.34f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.2f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.05f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.35f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsurePunishNetPatternProfile()
        {
            EnsureFolderForAsset(PunishNetPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(PunishNetPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, PunishNetPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "PunishNet";
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.PunishNet;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.1f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.8f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 5;
            RequireProperty(serializedObject, "damage").floatValue = 14f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.4f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.2f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.29f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.45f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 0.92f;
            RequireProperty(serializedObject, "punishNetInnerSpreadRatio").floatValue = 0.34f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.52f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureCoverFirePatternProfile()
        {
            EnsureFolderForAsset(CoverFirePatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(CoverFirePatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, CoverFirePatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "CoverFire";
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.CenterSpread;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.25f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 0.85f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.2f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 5;
            RequireProperty(serializedObject, "damage").floatValue = 15f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.5f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 4.8f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.2f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.25f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.38f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureEscortScreenPatternProfile()
        {
            EnsureFolderForAsset(EscortScreenPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(EscortScreenPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, EscortScreenPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "EscortScreen";
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.EscortScreen;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.0f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.7f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 6;
            RequireProperty(serializedObject, "damage").floatValue = 13f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.6f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.1f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.28f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.0f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.6f;
            RequireProperty(serializedObject, "escortScreenInnerGapRatio").floatValue = 0.35f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.4f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.9f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.45f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureLayeredSalvoPatternProfile()
        {
            EnsureFolderForAsset(LayeredSalvoPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(LayeredSalvoPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, LayeredSalvoPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "LayeredSalvo";
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.LayeredSalvo;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.35f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.1f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.4f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 9;
            RequireProperty(serializedObject, "damage").floatValue = 14f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 11.8f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.6f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.2f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.75f;
            RequireProperty(serializedObject, "layeredSalvoRowCount").intValue = 3;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 3.2f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 1.1f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.5f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureLinePressurePatternProfile()
        {
            EnsureFolderForAsset(LinePressurePatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(LinePressurePatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, LinePressurePatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "LinePressure";
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.LinePressure;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.0f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.6f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 15f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 13.0f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.1f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.0f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 3.0f;
            RequireProperty(serializedObject, "linePressureDirection").floatValue = 1f;
            RequireProperty(serializedObject, "linePressureCenterRatio").floatValue = 0.72f;
            RequireProperty(serializedObject, "linePressureHalfSpreadRatio").floatValue = 0.08f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.2f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.85f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.5f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureStaggeredCrossfirePatternProfile()
        {
            EnsureFolderForAsset(StaggeredCrossfirePatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(StaggeredCrossfirePatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, StaggeredCrossfirePatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "StaggeredCrossfire";
            RequireProperty(serializedObject, "targetingRule").enumValueIndex = (int)BossBarrageTargetingRule.LaneCenter;
            RequireProperty(serializedObject, "laneCenterLateralRatio").floatValue = 0f;
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.StaggeredCrossfire;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.3f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.15f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 6.2f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 6;
            RequireProperty(serializedObject, "damage").floatValue = 17f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 10.6f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.8f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.38f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 4.35f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.95f;
            RequireProperty(serializedObject, "crossfireInnerGapRatio").floatValue = 0.30f;
            RequireProperty(serializedObject, "backlineDepthSpread").floatValue = 2.8f;
            RequireProperty(serializedObject, "forwardDepthSpread").floatValue = 0.95f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.65f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureTwinSweepPatternProfile()
        {
            EnsureFolderForAsset(TwinSweepPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(TwinSweepPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, TwinSweepPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "TwinSweep";
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.TwinColumns;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 0.95f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.2f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 4;
            RequireProperty(serializedObject, "damage").floatValue = 15f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.2f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 4.9f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.31f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.65f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.45f;
            RequireProperty(serializedObject, "twinColumnInnerSpreadRatio").floatValue = 0.42f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.42f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureLeftClampPatternProfile()
        {
            EnsureFolderForAsset(LeftClampPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(LeftClampPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, LeftClampPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "LeftClamp";
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.SideClamp;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.05f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.4f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 5;
            RequireProperty(serializedObject, "damage").floatValue = 16f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.8f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.0f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.85f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.7f;
            RequireProperty(serializedObject, "sideClampDirection").floatValue = -1f;
            RequireProperty(serializedObject, "sideClampCrossReachRatio").floatValue = 0.24f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.48f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarragePatternProfile EnsureRightClampPatternProfile()
        {
            EnsureFolderForAsset(RightClampPatternProfilePath);
            BossBarragePatternProfile profile =
                AssetDatabase.LoadAssetAtPath<BossBarragePatternProfile>(RightClampPatternProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BossBarragePatternProfile>();
                AssetDatabase.CreateAsset(profile, RightClampPatternProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            RequireProperty(serializedObject, "patternId").stringValue = "RightClamp";
            RequireProperty(serializedObject, "lateralShape").enumValueIndex = (int)BossBarrageLateralShape.SideClamp;
            RequireProperty(serializedObject, "initialDelaySeconds").floatValue = 0.2f;
            RequireProperty(serializedObject, "windupSeconds").floatValue = 1.05f;
            RequireProperty(serializedObject, "waveIntervalSeconds").floatValue = 5.4f;
            RequireProperty(serializedObject, "projectilesPerWave").intValue = 5;
            RequireProperty(serializedObject, "damage").floatValue = 16f;
            RequireProperty(serializedObject, "projectileSpeed").floatValue = 12.8f;
            RequireProperty(serializedObject, "projectileLifetimeSeconds").floatValue = 5.0f;
            RequireProperty(serializedObject, "projectileRadius").floatValue = 0.3f;
            RequireProperty(serializedObject, "backlineHalfSpread").floatValue = 3.85f;
            RequireProperty(serializedObject, "forwardHalfSpread").floatValue = 1.7f;
            RequireProperty(serializedObject, "sideClampDirection").floatValue = 1f;
            RequireProperty(serializedObject, "sideClampCrossReachRatio").floatValue = 0.24f;
            RequireProperty(serializedObject, "spawnHeight").floatValue = 1.48f;
            RequireProperty(serializedObject, "targetHeight").floatValue = 1.05f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static PlayerActionProfile EnsureLocalDefenseProfile()
        {
            EnsureFolderForAsset(LocalDefenseProfilePath);
            PlayerActionProfile profile = AssetDatabase.LoadAssetAtPath<PlayerActionProfile>(LocalDefenseProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PlayerActionProfile>();
                AssetDatabase.CreateAsset(profile, LocalDefenseProfilePath);
            }

            var serializedObject = new SerializedObject(profile);
            SerializedProperty basicCombo = RequireProperty(serializedObject, "basicCombo");
            basicCombo.arraySize = 1;
            SerializedProperty step = basicCombo.GetArrayElementAtIndex(0);
            RequireRelativeProperty(step, "animationTrigger").stringValue = "Attack1";
            RequireRelativeProperty(step, "startupSeconds").floatValue = 0.1f;
            RequireRelativeProperty(step, "activeSeconds").floatValue = 0.08f;
            RequireRelativeProperty(step, "recoverySeconds").floatValue = 0.26f;
            RequireRelativeProperty(step, "inputBufferSeconds").floatValue = 0.08f;
            RequireRelativeProperty(step, "dodgeCancelAfterSeconds").floatValue = 0.05f;
            RequireRelativeProperty(step, "forwardAdvanceDistance").floatValue = 0.22f;
            RequireRelativeProperty(step, "forwardAdvanceDurationSeconds").floatValue = 0.10f;
            RequireRelativeProperty(step, "damage").floatValue = 42f;
            RequireRelativeProperty(step, "hitRadius").floatValue = 0.78f;
            RequireRelativeProperty(step, "hitDistance").floatValue = 1.65f;
            RequireRelativeProperty(step, "hitStopSeconds").floatValue = 0f;

            RequireProperty(serializedObject, "comboResetSeconds").floatValue = 0.32f;
            RequireProperty(serializedObject, "comboQueueOpenAfterSeconds").floatValue = 0.12f;
            RequireProperty(serializedObject, "comboChainRecoveryRatio").floatValue = 1f;
            RequireProperty(serializedObject, "attackFacingHoldPaddingSeconds").floatValue = 0.04f;
            RequireProperty(serializedObject, "snapBasicAttackFacing").boolValue = true;
            RequireProperty(serializedObject, "basicAttackMoveInputSpeedScale").floatValue = 0f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static BossBarrageProjectile EnsureProjectilePrefab()
        {
            EnsureFolderForAsset(ProjectilePrefabPath);
            Material material = LoadOrCreateMaterial(ProjectileMaterialPath, new Color(1f, 0.72f, 0.12f, 1f));
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(ProjectilePrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            try
            {
                editableRoot.name = "PF_BossBarrageProjectile_NeedleLock";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one * 0.55f;

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                renderer.enabled = false;

                SphereCollider collider = EnsureComponent<SphereCollider>(editableRoot);
                collider.isTrigger = true;
                collider.radius = 0.5f;

                Rigidbody rigidbody = EnsureComponent<Rigidbody>(editableRoot);
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;

                EnsureComponent<BossBarrageProjectile>(editableRoot);
                PrefabUtility.SaveAsPrefabAsset(editableRoot, ProjectilePrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadPrefabComponent<BossBarrageProjectile>(ProjectilePrefabPath);
        }

        private static LaneActionProjectile EnsureLaneActionProjectilePrefab(
            string prefabPath,
            string prefabName,
            string materialPath,
            Color color,
            float scale,
            bool allowVerticalTravel)
        {
            EnsureFolderForAsset(prefabPath);
            Material material = LoadOrCreateMaterial(materialPath, color);
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(prefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            try
            {
                editableRoot.name = prefabName;
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;

                SphereCollider collider = EnsureComponent<SphereCollider>(editableRoot);
                collider.isTrigger = true;
                collider.radius = 0.5f;

                Rigidbody rigidbody = EnsureComponent<Rigidbody>(editableRoot);
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;

                LaneActionProjectile projectile = EnsureComponent<LaneActionProjectile>(editableRoot);
                SetBool(projectile, "allowVerticalTravel", allowVerticalTravel);
                PrefabUtility.SaveAsPrefabAsset(editableRoot, prefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadPrefabComponent<LaneActionProjectile>(prefabPath);
        }

        private static GameObject EnsureSummonEntryCuePrefab()
        {
            EnsureFolderForAsset(SummonSlot1EntryCuePrefabPath);
            Material material = LoadOrCreateMaterial(SummonSlot1EntryCueMaterialPath, new Color(0.25f, 1f, 0.68f, 1f));
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(SummonSlot1EntryCuePrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(SummonSlot1EntryCuePrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            try
            {
                editableRoot.name = "PF_SummonSlot1EntryCue_MagicCircle";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = new Vector3(1f, 0.04f, 1f);

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                PrefabUtility.SaveAsPrefabAsset(editableRoot, SummonSlot1EntryCuePrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath);
        }

        private static SummonFrontlineProxy EnsureSummonActorPrefab()
        {
            EnsureFolderForAsset(SummonSlot1ActorPrefabPath);
            Material material = LoadOrCreateMaterial(SummonSlot1ActorMaterialPath, new Color(0.2f, 1f, 0.78f, 1f));
            Material pressureScreenMaterial = LoadOrCreateTransparentMaterial(
                SummonPressureScreenMaterialPath,
                new Color(0.18f, 1f, 0.78f, 0.38f));
            Material pulseMaterial = LoadOrCreateTransparentMaterial(
                SummonSlot1ActorPulseMaterialPath,
                new Color(0.45f, 0.95f, 1f, 0.72f));
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(SummonSlot1ActorPrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(SummonSlot1ActorPrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            try
            {
                editableRoot.name = "PF_SummonSlot1Actor_Proxy";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one;

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                SummonFrontlineProxy proxy = EnsureComponent<SummonFrontlineProxy>(editableRoot);
                Transform projectileOrigin = EnsureChild(editableRoot.transform, "ProjectileOrigin");
                projectileOrigin.localPosition = new Vector3(0f, 0.85f, 0.35f);
                projectileOrigin.localRotation = Quaternion.identity;
                projectileOrigin.localScale = Vector3.one;
                SetObjectReference(proxy, "projectileOrigin", projectileOrigin);

                SummonPressureScreen pressureScreen = EnsureComponent<SummonPressureScreen>(editableRoot);
                SphereCollider screenCollider = EnsureComponent<SphereCollider>(editableRoot);
                screenCollider.isTrigger = true;
                screenCollider.radius = 1.35f;

                Rigidbody screenRigidbody = EnsureComponent<Rigidbody>(editableRoot);
                screenRigidbody.useGravity = false;
                screenRigidbody.isKinematic = true;

                SetEnum(pressureScreen, "ownerTeam", (int)DamageTeam.AllySummon);
                SetInt(pressureScreen, "defaultMaxIntercepts", 2);
                SetFloat(pressureScreen, "defaultLifetimeSeconds", 1.2f);
                SetFloat(pressureScreen, "defaultRadius", 1.35f);
                SetObjectReference(proxy, "pressureScreen", pressureScreen);

                Transform pressureScreenVisual = EnsureChild(editableRoot.transform, "PressureScreenVisual");
                pressureScreenVisual.localPosition = new Vector3(0f, 0.72f, 0.2f);
                pressureScreenVisual.localRotation = Quaternion.identity;
                pressureScreenVisual.localScale = Vector3.one;
                MeshFilter visualFilter = EnsureComponent<MeshFilter>(pressureScreenVisual.gameObject);
                visualFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer visualRenderer = EnsureComponent<MeshRenderer>(pressureScreenVisual.gameObject);
                visualRenderer.sharedMaterial = pressureScreenMaterial;
                visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
                visualRenderer.receiveShadows = false;
                visualRenderer.allowOcclusionWhenDynamic = false;
                Collider visualCollider = pressureScreenVisual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualCollider);
                }

                SummonPressureScreenPresenter presenter = EnsureComponent<SummonPressureScreenPresenter>(editableRoot);
                SetObjectReference(presenter, "pressureScreen", pressureScreen);
                SetObjectReference(presenter, "visualRoot", pressureScreenVisual);
                SetObjectReferenceArray(presenter, "screenRenderers", new UnityEngine.Object[] { visualRenderer });
                SetColor(presenter, "activeColor", new Color(0.22f, 1f, 0.82f, 0.42f));
                SetColor(presenter, "interceptColor", new Color(0.92f, 1f, 1f, 0.88f));
                SetFloat(presenter, "activationFlashSeconds", 0.12f);
                SetFloat(presenter, "interceptFlashSeconds", 0.18f);
                SetFloat(presenter, "finalHitLingerSeconds", 0.16f);
                SetFloat(presenter, "pulseSpeed", 9f);
                SetFloat(presenter, "pulseScale", 0.04f);

                Transform tierPulseCore = EnsureChild(editableRoot.transform, "TierPulseCore");
                tierPulseCore.localPosition = new Vector3(0f, 1.08f, 0.08f);
                tierPulseCore.localRotation = Quaternion.identity;
                tierPulseCore.localScale = Vector3.one * 0.32f;
                MeshFilter pulseFilter = EnsureComponent<MeshFilter>(tierPulseCore.gameObject);
                pulseFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer pulseRenderer = EnsureComponent<MeshRenderer>(tierPulseCore.gameObject);
                pulseRenderer.sharedMaterial = pulseMaterial;
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

                Transform summonVisual = AttachRoleVisualOnly(
                    editableRoot.transform,
                    SummonSlot1ActorVisualRoleId,
                    ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerElitePrefabPath,
                    SummonSlot1ActorVisualName,
                    new Vector3(0f, -0.04f, -0.08f),
                    Vector3.zero,
                    new Vector3(0.62f, 0.62f, 0.62f));

                SummonFrontlineProxyPresenter actorPresenter =
                    EnsureComponent<SummonFrontlineProxyPresenter>(editableRoot);
                SetObjectReference(actorPresenter, "proxy", proxy);
                SetObjectReference(actorPresenter, "pulseRoot", tierPulseCore);
                SetObjectReferenceArray(
                    actorPresenter,
                    "actorRenderers",
                    BuildRendererReferenceArray(summonVisual.gameObject, pulseRenderer));
                SetColor(actorPresenter, "tierOneColor", new Color(0.24f, 1f, 0.78f, 0.78f));
                SetColor(actorPresenter, "tierTwoColor", new Color(0.38f, 0.74f, 1f, 0.9f));
                SetColor(actorPresenter, "tierThreeColor", new Color(1f, 0.76f, 0.24f, 1f));
                SetColor(actorPresenter, "flashColor", Color.white);
                SetFloat(actorPresenter, "entryFlashSeconds", 0.22f);
                SetFloat(actorPresenter, "impactFlashSeconds", 0.18f);
                SetFloat(actorPresenter, "impactFlashProgress", 0.86f);
                SetFloat(actorPresenter, "pulseSpeed", 8f);
                SetFloat(actorPresenter, "pulseScale", 0.08f);
                SetFloat(actorPresenter, "tierScaleStep", 0.18f);
                SetFloat(actorPresenter, "flashScale", 0.22f);

                PrefabUtility.SaveAsPrefabAsset(editableRoot, SummonSlot1ActorPrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot1ActorPrefabPath);
        }

        private static SummonFrontlineProxy EnsureBossSummonPressureActorPrefab()
        {
            EnsureFolderForAsset(BossSummonPressureActorPrefabPath);
            Material material = LoadOrCreateMaterial(BossSummonPressureActorMaterialPath, new Color(1f, 0.36f, 0.64f, 1f));
            Material pressureScreenMaterial = LoadOrCreateTransparentMaterial(
                BossSummonPressureScreenMaterialPath,
                new Color(1f, 0.22f, 0.55f, 0.38f));
            Material pulseMaterial = LoadOrCreateTransparentMaterial(
                BossSummonPressureActorPulseMaterialPath,
                new Color(1f, 0.62f, 0.28f, 0.74f));
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(BossSummonPressureActorPrefabPath) != null;
            GameObject editableRoot = prefabExists
                ? PrefabUtility.LoadPrefabContents(BossSummonPressureActorPrefabPath)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            try
            {
                editableRoot.name = "PF_BossSummonPressureActor_Proxy";
                editableRoot.transform.localPosition = Vector3.zero;
                editableRoot.transform.localRotation = Quaternion.identity;
                editableRoot.transform.localScale = Vector3.one;

                MeshRenderer renderer = EnsureComponent<MeshRenderer>(editableRoot);
                renderer.sharedMaterial = material;
                renderer.enabled = false;

                Collider collider = editableRoot.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }

                SummonFrontlineProxy proxy = EnsureComponent<SummonFrontlineProxy>(editableRoot);
                Transform projectileOrigin = EnsureChild(editableRoot.transform, "PressureOrigin");
                projectileOrigin.localPosition = new Vector3(0f, 0.92f, -0.28f);
                projectileOrigin.localRotation = Quaternion.identity;
                projectileOrigin.localScale = Vector3.one;
                SetObjectReference(proxy, "projectileOrigin", projectileOrigin);

                SummonPressureScreen pressureScreen = EnsureComponent<SummonPressureScreen>(editableRoot);
                SphereCollider screenCollider = EnsureComponent<SphereCollider>(editableRoot);
                screenCollider.isTrigger = true;
                screenCollider.radius = 1.45f;

                Rigidbody screenRigidbody = EnsureComponent<Rigidbody>(editableRoot);
                screenRigidbody.useGravity = false;
                screenRigidbody.isKinematic = true;

                SetEnum(pressureScreen, "ownerTeam", (int)DamageTeam.Enemy);
                SetInt(pressureScreen, "defaultMaxIntercepts", 3);
                SetFloat(pressureScreen, "defaultLifetimeSeconds", 1.45f);
                SetFloat(pressureScreen, "defaultRadius", 1.45f);
                SetObjectReference(proxy, "pressureScreen", pressureScreen);

                Transform pressureScreenVisual = EnsureChild(editableRoot.transform, "PressureScreenVisual");
                pressureScreenVisual.localPosition = new Vector3(0f, 0.72f, -0.12f);
                pressureScreenVisual.localRotation = Quaternion.identity;
                pressureScreenVisual.localScale = Vector3.one;
                MeshFilter visualFilter = EnsureComponent<MeshFilter>(pressureScreenVisual.gameObject);
                visualFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer visualRenderer = EnsureComponent<MeshRenderer>(pressureScreenVisual.gameObject);
                visualRenderer.sharedMaterial = pressureScreenMaterial;
                visualRenderer.shadowCastingMode = ShadowCastingMode.Off;
                visualRenderer.receiveShadows = false;
                visualRenderer.allowOcclusionWhenDynamic = false;
                Collider visualCollider = pressureScreenVisual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(visualCollider);
                }

                SummonPressureScreenPresenter presenter = EnsureComponent<SummonPressureScreenPresenter>(editableRoot);
                SetObjectReference(presenter, "pressureScreen", pressureScreen);
                SetObjectReference(presenter, "visualRoot", pressureScreenVisual);
                SetObjectReferenceArray(presenter, "screenRenderers", new UnityEngine.Object[] { visualRenderer });
                SetColor(presenter, "activeColor", new Color(1f, 0.22f, 0.55f, 0.42f));
                SetColor(presenter, "interceptColor", new Color(1f, 0.86f, 0.64f, 0.9f));
                SetFloat(presenter, "activationFlashSeconds", 0.14f);
                SetFloat(presenter, "interceptFlashSeconds", 0.2f);
                SetFloat(presenter, "finalHitLingerSeconds", 0.16f);
                SetFloat(presenter, "pulseSpeed", 8.2f);
                SetFloat(presenter, "pulseScale", 0.055f);

                Transform tierPulseCore = EnsureChild(editableRoot.transform, "TierPressureCore");
                tierPulseCore.localPosition = new Vector3(0f, 1.08f, -0.08f);
                tierPulseCore.localRotation = Quaternion.identity;
                tierPulseCore.localScale = Vector3.one * 0.34f;
                MeshFilter pulseFilter = EnsureComponent<MeshFilter>(tierPulseCore.gameObject);
                pulseFilter.sharedMesh = LoadPrimitiveMesh(PrimitiveType.Sphere);
                MeshRenderer pulseRenderer = EnsureComponent<MeshRenderer>(tierPulseCore.gameObject);
                pulseRenderer.sharedMaterial = pulseMaterial;
                pulseRenderer.shadowCastingMode = ShadowCastingMode.Off;
                pulseRenderer.receiveShadows = false;
                pulseRenderer.allowOcclusionWhenDynamic = false;
                Collider pulseCollider = tierPulseCore.GetComponent<Collider>();
                if (pulseCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(pulseCollider);
                }

                Transform summonVisual = AttachRoleVisualOnly(
                    editableRoot.transform,
                    BossSummonPressureActorVisualRoleId,
                    ActionFoundationEnemyRoleCandidateSetup.AuraCaptainElitePrefabPath,
                    BossSummonPressureActorVisualName,
                    new Vector3(0f, -0.04f, 0.1f),
                    Vector3.zero,
                    new Vector3(0.66f, 0.66f, 0.66f));

                SummonFrontlineProxyPresenter actorPresenter =
                    EnsureComponent<SummonFrontlineProxyPresenter>(editableRoot);
                SetObjectReference(actorPresenter, "proxy", proxy);
                SetObjectReference(actorPresenter, "pulseRoot", tierPulseCore);
                SetObjectReferenceArray(
                    actorPresenter,
                    "actorRenderers",
                    BuildRendererReferenceArray(summonVisual.gameObject, pulseRenderer));
                SetColor(actorPresenter, "tierOneColor", new Color(1f, 0.32f, 0.55f, 0.82f));
                SetColor(actorPresenter, "tierTwoColor", new Color(1f, 0.62f, 0.24f, 0.92f));
                SetColor(actorPresenter, "tierThreeColor", new Color(1f, 0.22f, 0.9f, 1f));
                SetColor(actorPresenter, "flashColor", new Color(1f, 0.95f, 0.84f, 1f));
                SetFloat(actorPresenter, "entryFlashSeconds", 0.24f);
                SetFloat(actorPresenter, "impactFlashSeconds", 0.2f);
                SetFloat(actorPresenter, "impactFlashProgress", 0.82f);
                SetFloat(actorPresenter, "pulseSpeed", 7.4f);
                SetFloat(actorPresenter, "pulseScale", 0.1f);
                SetFloat(actorPresenter, "tierScaleStep", 0.2f);
                SetFloat(actorPresenter, "flashScale", 0.24f);

                PrefabUtility.SaveAsPrefabAsset(editableRoot, BossSummonPressureActorPrefabPath);
            }
            finally
            {
                if (prefabExists)
                {
                    PrefabUtility.UnloadPrefabContents(editableRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(editableRoot);
                }
            }

            return LoadPrefabComponent<SummonFrontlineProxy>(BossSummonPressureActorPrefabPath);
        }

        private static void EnsureSummonPresentationCandidateProfiles()
        {
            CombatVfxCueProfile vfxCueProfile =
                LoadAsset<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);

            ConfigureSummonPresentationCandidateProfile(
                LoadOrCreateSummonPresentationCandidateProfile(SummonSlot1PresentationCandidateProfilePath),
                "PlayerSummon.ShieldBreaker",
                "Player Summon - Shield Breaker Elite",
                SummonPresentationSide.PlayerSummon,
                SummonSlot1ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerEliteCandidateProfilePath,
                SummonSlot1ActorVisualName,
                SummonSlot1ActorVisualRoleId,
                vfxCueProfile,
                "Promoted ShieldBreakerElite role Animator stands in for the first ally summon block-and-break read.",
                "Magic-circle entry, ally pressure screen, tier pulse, assist bolt, and counter bolt carry the current read.",
                "Replace the actor prefab or promoted visual source after a dedicated ally summon model and animation set are reviewed.");

            ConfigureSummonPresentationCandidateProfile(
                LoadOrCreateSummonPresentationCandidateProfile(BossSummonPressurePresentationCandidateProfilePath),
                "BossPressure.AuraCaptain",
                "Boss Pressure Summon - Aura Captain Elite",
                SummonPresentationSide.BossPressure,
                BossSummonPressureActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.AuraCaptainEliteCandidateProfilePath,
                BossSummonPressureActorVisualName,
                BossSummonPressureActorVisualRoleId,
                vfxCueProfile,
                "Promoted AuraCaptainElite role Animator stands in for boss-side summon-pressure command reads.",
                "Enemy pressure screen, pressure pulse, and boss-side intercept colors distinguish it from the player summon.",
                "Replace with a dedicated boss pressure summon after boss roster art is reviewed, without changing boss cost data.");

            AssetDatabase.SaveAssets();
        }

        private static SummonPresentationCandidateProfile LoadOrCreateSummonPresentationCandidateProfile(string assetPath)
        {
            EnsureFolderForAsset(assetPath);
            SummonPresentationCandidateProfile profile =
                AssetDatabase.LoadAssetAtPath<SummonPresentationCandidateProfile>(assetPath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<SummonPresentationCandidateProfile>();
            AssetDatabase.CreateAsset(profile, assetPath);
            return profile;
        }

        private static void ConfigureSummonPresentationCandidateProfile(
            SummonPresentationCandidateProfile profile,
            string candidateId,
            string displayName,
            SummonPresentationSide side,
            string actorPrefabPath,
            string roleCandidateProfilePath,
            string visualChildName,
            string sourceRoleId,
            CombatVfxCueProfile vfxCueProfile,
            string animationRead,
            string vfxRead,
            string replacementPlan)
        {
            GameObject actorPrefab = LoadAsset<GameObject>(actorPrefabPath);
            CombatEnemyRoleCandidateProfile roleCandidate =
                LoadAsset<CombatEnemyRoleCandidateProfile>(roleCandidateProfilePath);
            RuntimeAnimatorController animatorController =
                ResolveActorVisualAnimatorController(actorPrefab, visualChildName);

            SetString(profile, "candidateId", candidateId);
            SetString(profile, "displayName", displayName);
            SetEnum(profile, "side", (int)side);
            SetObjectReference(profile, "actorPrefab", actorPrefab);
            SetObjectReference(profile, "visualSourceAsset", roleCandidate.PromotedVisualSource);
            SetString(profile, "visualChildName", visualChildName);
            SetString(profile, "sourceRoleId", sourceRoleId);
            SetObjectReference(profile, "animatorController", animatorController);
            SetObjectReference(profile, "vfxCueProfile", vfxCueProfile);
            SetString(profile, "animationRead", animationRead);
            SetString(profile, "vfxRead", vfxRead);
            SetString(profile, "replacementPlan", replacementPlan);
            SetString(
                profile,
                "ownershipNotes",
                "Presentation candidate only; runtime cost, tier, projectile, and screen values remain in gameplay profiles.");
        }

        private static RuntimeAnimatorController ResolveActorVisualAnimatorController(
            GameObject actorPrefab,
            string visualChildName)
        {
            Transform visual = actorPrefab.transform.Find(visualChildName);
            if (visual == null)
            {
                throw new InvalidOperationException($"{actorPrefab.name} is missing visual child {visualChildName}.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException($"{visualChildName} is missing an Animator controller.");
            }

            return animator.runtimeAnimatorController;
        }

        private static SummonLaneSpace CreateLaneSpace(Scene scene)
        {
            GameObject laneRoot = CreateRoot(scene, LaneRootName);
            SummonLaneSpace laneSpace = laneRoot.AddComponent<SummonLaneSpace>();
            laneRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            SetFloat(laneSpace, "halfWidth", 5.25f);
            SetFloat(laneSpace, "backLimitZ", -12f);
            SetFloat(laneSpace, "forwardBoundaryZ", 0f);
            SetFloat(laneSpace, "bossProxyZ", 18f);
            SetFloat(laneSpace, "summonEntryZ", 2.25f);
            return laneSpace;
        }

        private static GameObject CreateBossProxy(
            Scene scene,
            SummonLaneSpace laneSpace,
            BossBarragePatternProfile patternProfile,
            BossBarragePatternProfile coverFirePatternProfile,
            BossBarragePatternProfile escortScreenPatternProfile,
            BossBarragePatternProfile layeredSalvoPatternProfile,
            BossBarragePatternProfile staggeredCrossfirePatternProfile,
            BossBarragePatternProfile twinSweepPatternProfile,
            BossBarragePatternProfile leftClampPatternProfile,
            BossBarragePatternProfile rightClampPatternProfile,
            BossBarragePatternProfile punishNetPatternProfile,
            BossBarragePatternProfile linePressurePatternProfile,
            BossBarrageProjectile projectilePrefab,
            Transform projectileRoot,
            SummonFrontlineProxy bossSummonActorPrefab,
            Transform bossSummonActorRoot)
        {
            GameObject bossProxy = CreateRoot(scene, BossProxyRootName);
            bossProxy.transform.SetPositionAndRotation(
                laneSpace.GetLaneWorldPoint(0f, laneSpace.BossProxyZ, 1.6f),
                Quaternion.LookRotation(Vector3.back, Vector3.up));
            Transform playerTransform = RequireObject<PlayerMovementController>(scene, "player movement").transform;

            CombatHealth bossHealth = EnsureComponent<CombatHealth>(bossProxy);
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            SetFloat(bossHealth, "maxHealth", 5000f);

            BossBarrageEmitter emitter = EnsureComponent<BossBarrageEmitter>(bossProxy);
            SetObjectReference(emitter, "laneSpace", laneSpace);
            SetObjectReference(emitter, "trackedPlayer", playerTransform);
            SetObjectReference(emitter, "sourceHealth", bossHealth);
            SetObjectReference(emitter, "patternProfile", patternProfile);
            SetObjectReferenceArray(
                emitter,
                "patternSequence",
                new UnityEngine.Object[]
                {
                    patternProfile,
                    coverFirePatternProfile,
                    escortScreenPatternProfile,
                    layeredSalvoPatternProfile,
                    staggeredCrossfirePatternProfile,
                    twinSweepPatternProfile,
                    leftClampPatternProfile,
                    rightClampPatternProfile,
                    punishNetPatternProfile,
                    linePressurePatternProfile
                });
            SetInt(emitter, "wavesPerPattern", 1);
            SetObjectReference(emitter, "projectilePrefab", projectilePrefab);
            SetObjectReference(emitter, "projectilePrefabObject", LoadAsset<GameObject>(ProjectilePrefabPath));
            SetObjectReference(emitter, "projectileRoot", projectileRoot);
            SetInt(emitter, "sourceTeam", (int)DamageTeam.Enemy);
            SetBool(emitter, "firingEnabled", true);
            SetInt(emitter, "prewarmCount", 24);

            BossPressureCostLadder bossPressureCost = EnsureComponent<BossPressureCostLadder>(bossProxy);
            bossPressureCost.ConfigureReferences(laneSpace, bossProxy.transform);
            SetFloat(bossPressureCost, "baseCostPerSecond", 18f);
            SetFloat(bossPressureCost, "fallbackBossForwardRisk01", 0.25f);

            BossSummonPressureAction bossSummonPressureAction = EnsureComponent<BossSummonPressureAction>(bossProxy);
            bossSummonPressureAction.ConfigureReferences(
                laneSpace,
                playerTransform,
                bossSummonActorPrefab,
                bossSummonActorRoot);
            SetObjectReference(bossSummonPressureAction, "summonActorPrefab", bossSummonActorPrefab);
            SetObjectReference(
                bossSummonPressureAction,
                "summonActorPrefabObject",
                LoadAsset<GameObject>(BossSummonPressureActorPrefabPath));
            SetObjectReference(bossSummonPressureAction, "summonActorRoot", bossSummonActorRoot);
            SetEnum(bossSummonPressureAction, "ownerTeam", (int)DamageTeam.Enemy);
            SetInt(bossSummonPressureAction, "actorPrewarmCount", 2);
            bossSummonPressureAction.ConfigurePressureProfile(
                LoadAsset<BossSummonPressureProfile>(BossSummonPressureProfilePath));

            BossPressureActionDirector bossPressureActionDirector =
                EnsureComponent<BossPressureActionDirector>(bossProxy);
            bossPressureActionDirector.ConfigureReferences(
                bossPressureCost,
                emitter,
                bossSummonPressureAction,
                laneSpace,
                playerTransform);
            bossPressureActionDirector.ConfigureActionDeck(
                LoadAsset<BossPressureActionDeckProfile>(BossPressureActionDeckProfilePath));
            SetBool(bossPressureActionDirector, "actionsEnabled", true);

            BossPressurePositionController bossPressurePosition =
                EnsureComponent<BossPressurePositionController>(bossProxy);
            bossPressurePosition.ConfigureReferences(
                laneSpace,
                bossPressureCost,
                bossPressureActionDirector,
                bossProxy.transform);
            SetObjectReference(bossPressurePosition, "movedTransform", bossProxy.transform);
            SetFloat(bossPressurePosition, "restRisk01", 0.08f);
            SetFloat(bossPressurePosition, "maxCommitRisk01", 0.62f);
            SetFloat(bossPressurePosition, "advanceRiskPerSecond", 0.25f);
            SetFloat(bossPressurePosition, "retreatRiskPerSecond", 0.42f);
            SetBool(bossPressurePosition, "returnToRestWhenActionsDisabled", true);
            SetBool(bossPressurePosition, "movementEnabled", true);
            EditorUtility.SetDirty(bossPressureCost);
            EditorUtility.SetDirty(bossSummonPressureAction);
            EditorUtility.SetDirty(bossPressureActionDirector);
            EditorUtility.SetDirty(bossPressurePosition);

            CreateBossProxyVisual(bossProxy.transform);
            ConfigureBossProxyVisualCueDriver(bossProxy, emitter, bossPressureActionDirector);
            return bossProxy;
        }

        private static GameObject CreateCloseThreat(
            Scene scene,
            SummonLaneSpace laneSpace,
            Transform player,
            CombatHealth playerHealth,
            ActionCameraController cameraController)
        {
            GameObject prefab = LoadAsset<GameObject>(ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath);
            GameObject closeThreat = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (closeThreat == null)
            {
                throw new InvalidOperationException($"Could not instantiate close-threat prefab {ActionFoundationEnemyPrefabSetup.MeleeSoldierPrefabPath}.");
            }

            Vector3 position = laneSpace.GetLaneWorldPoint(-1.35f, -2.65f, 0f);
            Vector3 toPlayer = Vector3.ProjectOnPlane(player.position - position, Vector3.up);
            if (toPlayer.sqrMagnitude <= 0.0001f)
            {
                toPlayer = Vector3.back;
            }

            closeThreat.name = CloseThreatRootName;
            closeThreat.transform.SetPositionAndRotation(position, Quaternion.LookRotation(toPlayer.normalized, Vector3.up));
            closeThreat.transform.localScale = Vector3.one;
            closeThreat.SetActive(true);

            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(closeThreat, "close threat soldier");
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(closeThreat, "close threat target sensor");
            EnemyActionCameraCueDriver cameraCueDriver =
                RequireComponent<EnemyActionCameraCueDriver>(closeThreat, "close threat camera cue driver");

            SetObjectReference(targetSensor, "selfHealth", closeThreatHealth);
            SetObjectReferenceArray(targetSensor, "targetCandidates", new UnityEngine.Object[] { playerHealth });
            SetObjectReference(soldier, "targetSensor", targetSensor);
            SetObjectReference(soldier, "target", null);
            SetObjectReference(soldier, "targetHealth", null);
            SetObjectReference(soldier, "selfHealth", closeThreatHealth);
            SetObjectReference(cameraCueDriver, "agentSource", soldier);
            SetObjectReference(cameraCueDriver, "cameraController", cameraController);
            SetObjectReference(cameraCueDriver, "cueSpace", closeThreat.transform);
            SetFloat(closeThreatHealth, "maxHealth", 72f);
            return closeThreat;
        }

        private static void CreateBossProxyVisual(Transform parent)
        {
            CreateHumanoidBossProxyVisual(parent);
            CreateBossProjectileCore(parent);
        }

        private static void CreateHumanoidBossProxyVisual(Transform parent)
        {
            CombatEnemyRoleCandidateProfile candidate = LoadAsset<CombatEnemyRoleCandidateProfile>(
                ActionFoundationEnemyRoleCandidateSetup.SummonCallerEliteCandidateProfilePath);
            string rolePrefabPath = AssetDatabase.GetAssetPath(candidate.RolePrefab).Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(rolePrefabPath))
            {
                throw new InvalidOperationException("SummonCallerElite boss candidate is missing its role prefab asset path.");
            }

            EnemyRoleVisualSpec visualSpec =
                ActionFoundationEnemyRoleVisualSetup.CreateForRole("SciFiSoldier.Elite.SummonCaller");
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(rolePrefabPath);
            try
            {
                Transform sourceVisual = prefabContents.transform.Find(visualSpec.VisualName);
                if (sourceVisual == null)
                {
                    throw new InvalidOperationException($"{rolePrefabPath} is missing {visualSpec.VisualName}.");
                }

                GameObject visual = UnityEngine.Object.Instantiate(sourceVisual.gameObject);
                visual.name = BossProxyHumanoidVisualName;
                SceneManager.MoveGameObjectToScene(visual, parent.gameObject.scene);
                visual.transform.SetParent(parent, worldPositionStays: false);
                visual.transform.localPosition = new Vector3(0f, -1.58f, 0f);
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale *= 1.18f;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        private static void CreateBossProjectileCore(Transform parent)
        {
            Material material = LoadOrCreateMaterial(BossProxyVisualMaterialPath, new Color(1f, 0.55f, 0.05f, 1f));
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = BossProxyMarkerName;
            visual.transform.SetParent(parent, worldPositionStays: false);
            visual.transform.localPosition = new Vector3(0f, 0.15f, -0.25f);
            visual.transform.localScale = new Vector3(0.46f, 0.46f, 0.46f);
            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static void ConfigureBossProxyVisualCueDriver(
            GameObject bossProxy,
            BossBarrageEmitter emitter,
            BossPressureActionDirector bossPressureActionDirector)
        {
            Transform visual = bossProxy.transform.Find(BossProxyHumanoidVisualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyHumanoidVisualName} before cue binding.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} is missing Animator for boss barrage cues.");
            }

            Transform projectileCore = bossProxy.transform.Find(BossProxyMarkerName);
            if (projectileCore == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyMarkerName} before cue binding.");
            }

            BossBarrageVisualCueDriver cueDriver = EnsureComponent<BossBarrageVisualCueDriver>(bossProxy);
            cueDriver.ConfigurePresentation(
                emitter,
                animator,
                projectileCore,
                projectileCore.GetComponentsInChildren<Renderer>(includeInactive: true));
            cueDriver.ConfigurePressureActionSource(bossPressureActionDirector);
            cueDriver.ResetToDefaultPatternCues();
            cueDriver.ResetToDefaultPressureActionCues();
            EditorUtility.SetDirty(cueDriver);
        }

        private static void CreateLaneMarkers(Scene scene, SummonLaneSpace laneSpace)
        {
            GameObject markerRoot = CreateRoot(scene, MarkerRootName);
            Material railMaterial = LoadOrCreateMaterial(LaneRailMaterialPath, new Color(0.15f, 0.72f, 1f, 1f));
            Material boundaryMaterial = LoadOrCreateMaterial(PlayerBoundaryMaterialPath, new Color(1f, 0.18f, 0.65f, 1f));
            Material summonMaterial = LoadOrCreateMaterial(SummonBoundaryMaterialPath, new Color(0.25f, 1f, 0.65f, 1f));

            float length = laneSpace.BossProxyZ - laneSpace.BackLimitZ;
            float centerZ = (laneSpace.BossProxyZ + laneSpace.BackLimitZ) * 0.5f;
            CreateMarker(
                markerRoot.transform,
                "Left_PlayerLaneRail",
                laneSpace.GetLaneWorldPoint(-laneSpace.HalfWidth, centerZ, 0.035f),
                new Vector3(0.08f, 0.05f, length),
                railMaterial);
            CreateMarker(
                markerRoot.transform,
                "Right_PlayerLaneRail",
                laneSpace.GetLaneWorldPoint(laneSpace.HalfWidth, centerZ, 0.035f),
                new Vector3(0.08f, 0.05f, length),
                railMaterial);
            CreateMarker(
                markerRoot.transform,
                "PlayerForwardBoundary_DoNotCross",
                laneSpace.GetLaneWorldPoint(0f, laneSpace.ForwardBoundaryZ, 0.06f),
                new Vector3(laneSpace.HalfWidth * 2f, 0.08f, 0.12f),
                boundaryMaterial);
            CreateMarker(
                markerRoot.transform,
                "SummonEntryLine_CanCross",
                laneSpace.GetLaneWorldPoint(0f, laneSpace.SummonEntryZ, 0.08f),
                new Vector3(laneSpace.HalfWidth * 2f, 0.08f, 0.12f),
                summonMaterial);
            CreateMarker(
                markerRoot.transform,
                SummonEntryMarkerName,
                laneSpace.GetLaneWorldPoint(0f, laneSpace.SummonEntryZ, 0.6f),
                new Vector3(0.7f, 1.2f, 0.7f),
                summonMaterial);
            CreateMarker(
                markerRoot.transform,
                "SummonOffLaneReach_CanCrossRail",
                laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1.2f, laneSpace.SummonEntryZ, 0.45f),
                new Vector3(0.55f, 0.9f, 0.55f),
                summonMaterial);
        }

        private static void CreateBossBarrageTelegraphMarkers(
            Scene scene,
            SummonLaneSpace laneSpace,
            BossBarrageEmitter bossBarrageEmitter)
        {
            GameObject root = CreateRoot(scene, BossTelegraphRootName);
            Material material = LoadOrCreateTransparentMaterial(
                BossTelegraphMaterialPath,
                new Color(1f, 0.62f, 0.18f, 0.56f));
            var markerTransforms = new Transform[9];
            var markerRenderers = new Renderer[markerTransforms.Length];
            for (int i = 0; i < markerTransforms.Length; i++)
            {
                float lateral01 = markerTransforms.Length <= 1 ? 0.5f : (float)i / (markerTransforms.Length - 1);
                float lateralX = Mathf.Lerp(-laneSpace.HalfWidth, laneSpace.HalfWidth, lateral01);
                GameObject marker = CreateMarker(
                    root.transform,
                    $"IncomingLaneTelegraph_{i:00}",
                    laneSpace.GetLaneWorldPoint(lateralX, laneSpace.ForwardBoundaryZ - 1.4f, 0.075f),
                    new Vector3(0.85f, 0.035f, 0.9f),
                    material);
                marker.SetActive(false);
                markerTransforms[i] = marker.transform;
                markerRenderers[i] = marker.GetComponent<MeshRenderer>();
            }

            BossBarrageLaneTelegraphPresenter presenter = root.AddComponent<BossBarrageLaneTelegraphPresenter>();
            presenter.Configure(bossBarrageEmitter, laneSpace, root.transform, markerTransforms, markerRenderers);
            EditorUtility.SetDirty(presenter);
        }

        private static void CreateEnergyRiskZoneMarkers(Scene scene, SummonLaneSpace laneSpace)
        {
            GameObject root = CreateRoot(scene, EnergyZoneRootName);
            Material backlineMaterial = LoadOrCreateTransparentMaterial(
                BacklineEnergyZoneMaterialPath,
                new Color(0.18f, 0.64f, 1f, 0.2f));
            Material midMaterial = LoadOrCreateTransparentMaterial(
                MidEnergyZoneMaterialPath,
                new Color(0.35f, 1f, 0.72f, 0.22f));
            Material forwardMaterial = LoadOrCreateTransparentMaterial(
                ForwardEnergyZoneMaterialPath,
                new Color(1f, 0.6f, 0.18f, 0.25f));

            float backZ = laneSpace.BackLimitZ;
            float forwardZ = laneSpace.ForwardBoundaryZ;
            float backEndZ = Mathf.Lerp(backZ, forwardZ, 1f / 3f);
            float midEndZ = Mathf.Lerp(backZ, forwardZ, 2f / 3f);

            CreateEnergyZoneMarker(
                root.transform,
                laneSpace,
                "BackSafety_ENSlow_0_33",
                backZ,
                backEndZ,
                backlineMaterial);
            CreateEnergyZoneMarker(
                root.transform,
                laneSpace,
                "MidCharge_ENBase_33_66",
                backEndZ,
                midEndZ,
                midMaterial);
            CreateEnergyZoneMarker(
                root.transform,
                laneSpace,
                "ForwardRisk_ENFast_66_100",
                midEndZ,
                forwardZ,
                forwardMaterial);
        }

        private static void CreateEnergyZoneMarker(
            Transform root,
            SummonLaneSpace laneSpace,
            string markerName,
            float startZ,
            float endZ,
            Material material)
        {
            float centerZ = (startZ + endZ) * 0.5f;
            float depth = Mathf.Abs(endZ - startZ);
            CreateMarker(
                root,
                markerName,
                laneSpace.GetLaneWorldPoint(0f, centerZ, 0.026f),
                new Vector3(laneSpace.HalfWidth * 2f, 0.025f, depth),
                material,
                removeCollider: true);
        }

        private static GameObject CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool removeCollider = false)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, worldPositionStays: true);
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = scale;
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (removeCollider)
            {
                Collider collider = marker.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            return marker;
        }

        private static void ConfigureTargetReferences(
            PlayerCombatTargetSelector targetSelector,
            ActionCameraTargetBridge cameraTargetBridge,
            ActionCameraController cameraController,
            PlayerMovementController player,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth)
        {
            ActionFoundationProfileSetup.ConfigurePlayerTargetSelector(
                targetSelector,
                player.transform,
                playerHealth,
                cameraController.transform,
                new[] { closeThreatHealth, bossHealth });
            // Seed the boss-lane review radius so a rebuilt scene can see the far proxy.
            // Designers may tune these values in the scene; validation does not exact-lock them.
            SetFloat(targetSelector, "selectionRadius", 35f);
            SetFloat(targetSelector, "attackAimRadius", 9f);
            SetObjectReference(cameraTargetBridge, "cameraController", cameraController);
            SetObjectReference(cameraTargetBridge, "targetSelector", targetSelector);
            SetObjectReference(cameraTargetBridge, "followTarget", player.transform);
            SetObjectReference(cameraController, "target", player.transform);
            SetObjectReference(cameraController, "threat", bossHealth.transform);
        }

        private static void ConfigureEncounter(
            ActionFoundationTestEncounter encounter,
            CombatHealth playerHealth,
            CombatHealth enemyHealth)
        {
            SetObjectReference(encounter, "playerHealth", playerHealth);
            SetObjectReference(encounter, "enemyHealth", enemyHealth);
        }

        private static void ConfigureLocalDefenseProfile(
            PlayerActionController playerActionController,
            PlayerActionProfile localDefenseProfile)
        {
            SetObjectReference(playerActionController, "actionProfile", localDefenseProfile);
        }

        private static PlayerCombatModeVisualBinding CreatePlayerCombatModeVisuals(Scene scene, GameObject player)
        {
            Transform playerTransform = player.transform;
            DestroyChildIfPresent(playerTransform, RangedPlayerVisualRootName);

            GameObject rangedRoot = new GameObject(RangedPlayerVisualRootName);
            rangedRoot.transform.SetParent(playerTransform, worldPositionStays: false);
            rangedRoot.transform.localPosition = Vector3.zero;
            rangedRoot.transform.localRotation = Quaternion.identity;
            rangedRoot.transform.localScale = Vector3.one;

            GameObject modelAsset = LoadAsset<GameObject>(RifleGirlSourcePrefabPath);
            GameObject modelInstance = PrefabUtility.InstantiatePrefab(modelAsset, scene) as GameObject;
            if (modelInstance == null)
            {
                throw new InvalidOperationException("Failed to instantiate RifleGirl source prefab for combat mode promotion.");
            }

            PrefabUtility.UnpackPrefabInstance(
                modelInstance,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            modelInstance.name = RangedPlayerModelName;
            modelInstance.transform.SetParent(rangedRoot.transform, worldPositionStays: false);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;
            StripNonGameMonoBehaviours(modelInstance);
            RemapRangedCandidateMeshes(modelInstance);
            AssignRangedCandidateMaterials(modelInstance);

            Animator rangedAnimator = modelInstance.GetComponentInChildren<Animator>(includeInactive: true)
                ?? modelInstance.AddComponent<Animator>();
            rangedAnimator.runtimeAnimatorController = LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath);
            rangedAnimator.avatar = LoadPromotedRifleGirlAvatar();
            rangedAnimator.applyRootMotion = false;
            RifleGirlNativeGameplayAnimatorBridge nativeBridge =
                modelInstance.GetComponent<RifleGirlNativeGameplayAnimatorBridge>()
                ?? modelInstance.AddComponent<RifleGirlNativeGameplayAnimatorBridge>();
            EditorUtility.SetDirty(nativeBridge);

            GameObject weaponInstance = FindRifleConstraintWeaponRoot(modelInstance.transform);
            if (weaponInstance == null)
            {
                throw new InvalidOperationException("RifleGirl source prefab must keep its constrained rifle object.");
            }

            weaponInstance.name = RangedPlayerWeaponName;
            Transform muzzle = FindOrCreateRifleMuzzle(weaponInstance.transform);
            RifleGirlWeaponSocketDriver weaponSocketDriver =
                ConfigureRifleGirlWeaponSocketDriver(modelInstance, rangedAnimator, weaponInstance);

            GameObject meleeRoot = FindPlayerMeleeVisualRoot(playerTransform);
            if (meleeRoot == null)
            {
                throw new InvalidOperationException("CombatGirl melee visual root is required as the sword/shield source for RifleGirl weapon-only swap.");
            }

            GameObject meleeWeaponRoot = CreateMeleeWeaponRoot(scene, rangedRoot.transform, modelInstance.transform, meleeRoot);
            if (meleeWeaponRoot == null)
            {
                throw new InvalidOperationException("Failed to create RifleGirl-mounted melee weapon root.");
            }

            rangedRoot.SetActive(true);
            meleeRoot.SetActive(false);
            meleeWeaponRoot.SetActive(false);

            EditorUtility.SetDirty(rangedRoot);
            EditorUtility.SetDirty(modelInstance);
            EditorUtility.SetDirty(weaponInstance);
            EditorUtility.SetDirty(weaponSocketDriver);
            EditorUtility.SetDirty(meleeRoot);
            EditorUtility.SetDirty(meleeWeaponRoot);

            return new PlayerCombatModeVisualBinding(
                rangedRoot,
                meleeRoot,
                weaponInstance,
                muzzle,
                meleeWeaponRoot,
                nativeBridge,
                rangedAnimator,
                rangedAnimator);
        }

        private static GameObject CreateMeleeWeaponRoot(
            Scene scene,
            Transform parent,
            Transform rangedModelRoot,
            GameObject meleeRoot)
        {
            DestroyChildIfPresent(parent, MeleePlayerWeaponRootName);

            if (meleeRoot == null)
            {
                return null;
            }

            Transform rightHand = FindLikelyRightHandSocket(rangedModelRoot);
            Transform leftHand = FindLikelyLeftHandSocket(rangedModelRoot);
            if (rightHand == null || leftHand == null)
            {
                throw new InvalidOperationException("RifleGirl model must expose both hand sockets before melee weapons can be attached.");
            }

            Transform sourceRightHand = FindLikelyRightHandSocket(meleeRoot.transform);
            Transform sourceLeftHand = FindLikelyLeftHandSocket(meleeRoot.transform);
            if (sourceRightHand == null || sourceLeftHand == null)
            {
                throw new InvalidOperationException("CombatGirl source visual must expose both hand sockets for preserving sword/shield offsets.");
            }

            Transform sourceRightWeapon = FindDescendant(meleeRoot.transform, "add_weapon_r");
            Transform sourceLeftWeapon = FindDescendant(meleeRoot.transform, "add_weapon_l");
            if (sourceRightWeapon == null || sourceLeftWeapon == null)
            {
                throw new InvalidOperationException("CombatGirl melee visual must expose add_weapon_r and add_weapon_l weapon objects.");
            }

            GameObject root = new GameObject(MeleePlayerWeaponRootName);
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            Transform rightWeapon = CloneWeaponObject(sourceRightWeapon, sourceRightHand, rightHand, "MeleeWeapon_RightHand");
            Transform leftWeapon = CloneWeaponObject(sourceLeftWeapon, sourceLeftHand, leftHand, "MeleeWeapon_LeftHand");
            CombatGirlWeaponSocketBinder binder = root.AddComponent<CombatGirlWeaponSocketBinder>();
            binder.ConfigureWeaponSockets(leftHand, leftWeapon, rightHand, rightWeapon);
            binder.ApplyBindings();
            EditorUtility.SetDirty(binder);
            return root;

            Transform CloneWeaponObject(Transform sourceWeapon, Transform sourceHand, Transform targetHand, string cloneName)
            {
                GameObject clone = UnityEngine.Object.Instantiate(sourceWeapon.gameObject, root.transform);
                clone.name = cloneName;
                Vector3 localPositionOffset = sourceHand.InverseTransformPoint(sourceWeapon.position);
                Quaternion localRotationOffset = Quaternion.Inverse(sourceHand.rotation) * sourceWeapon.rotation;
                clone.transform.SetPositionAndRotation(
                    targetHand.TransformPoint(localPositionOffset),
                    targetHand.rotation * localRotationOffset);
                clone.transform.localScale = sourceWeapon.localScale;
                EditorUtility.SetDirty(clone);
                return clone.transform;
            }
        }

        private static GameObject FindRifleConstraintWeaponRoot(Transform modelRoot)
        {
            ParentConstraint[] constraints = modelRoot.GetComponentsInChildren<ParentConstraint>(includeInactive: true);
            for (int i = 0; i < constraints.Length; i++)
            {
                if (constraints[i] != null
                    && constraints[i].name.Contains("Weapon_Rifle", StringComparison.Ordinal))
                {
                    return constraints[i].gameObject;
                }
            }

            Transform[] candidates = modelRoot.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < candidates.Length; i++)
            {
                if (string.Equals(candidates[i].name, "Weapon_Rifle", StringComparison.Ordinal)
                    && candidates[i].GetComponent<ParentConstraint>() != null)
                {
                    return candidates[i].gameObject;
                }
            }

            return null;
        }

        private static Transform FindOrCreateRifleMuzzle(Transform weaponRoot)
        {
            Transform existing = FindDescendant(weaponRoot, "Muzzle");
            if (existing != null)
            {
                return existing;
            }

            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(weaponRoot, worldPositionStays: false);
            muzzle.transform.localPosition = new Vector3(-0.92f, 0.055f, 0.035f);
            muzzle.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            muzzle.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(muzzle);
            return muzzle.transform;
        }

        private static RifleGirlWeaponSocketDriver ConfigureRifleGirlWeaponSocketDriver(
            GameObject modelInstance,
            Animator rangedAnimator,
            GameObject weaponInstance)
        {
            ParentConstraint weaponConstraint = weaponInstance.GetComponent<ParentConstraint>();
            if (weaponConstraint == null)
            {
                throw new InvalidOperationException("RifleGirl ranged weapon must keep its ParentConstraint.");
            }

            Transform leftHandle = FindDescendant(weaponInstance.transform, "Left_Handle");
            if (leftHandle == null)
            {
                throw new InvalidOperationException("RifleGirl ranged weapon must expose Left_Handle for support-hand IK.");
            }

            RifleGirlWeaponSocketDriver driver =
                modelInstance.GetComponent<RifleGirlWeaponSocketDriver>()
                ?? modelInstance.AddComponent<RifleGirlWeaponSocketDriver>();
            driver.Configure(rangedAnimator, weaponConstraint, leftHandle);
            SetObjectReference(driver, "animator", rangedAnimator);
            SetObjectReference(driver, "rifleConstraint", weaponConstraint);
            SetObjectReference(driver, "leftHandIkTarget", leftHandle);
            SetString(driver, "defaultCommands", "To_Hand_R_Socket, IK_ON_Left_Handle");
            SetString(driver, "handSocketCommand", "To_Hand_R_Socket");
            SetString(driver, "holsterSocketCommand", "To_Put_Socket_Rifle");
            SetString(driver, "aimSocketCommand", "To_add_weapon_r");
            SetString(driver, "leftIkOnCommand", "IK_ON_Left_Handle");
            SetString(driver, "leftIkOffCommand", "IK_OFF_Left_Handle");
            SetFloat(driver, "leftIkMaxWeight", 1f);
            SetFloat(driver, "leftIkBlendSpeed", 15f);
            driver.SwitchSocketByString("To_Hand_R_Socket, IK_ON_Left_Handle");
            EditorUtility.SetDirty(driver);
            return driver;
        }

        private static void ConfigureCombatModeController(
            GameObject player,
            PlayerActionController playerActionController,
            PlayerMovementController playerMovementController,
            PlayerActionProfile localDefenseProfile,
            PlayerCombatModeVisualBinding visualBinding)
        {
            PlayerCombatModeController combatModeController = EnsureComponent<PlayerCombatModeController>(player);
            SetObjectReference(combatModeController, "actionController", playerActionController);
            SetObjectReference(combatModeController, "movementController", playerMovementController);
            SetObjectReference(combatModeController, "rangedActionProfile", localDefenseProfile);
            SetObjectReference(combatModeController, "meleeActionProfile", LoadAsset<PlayerActionProfile>(MeleeActionProfilePath));
            SetObjectReference(combatModeController, "rangedVisualRoot", visualBinding.RangedRoot);
            SetObjectReference(combatModeController, "meleeVisualRoot", visualBinding.MeleeRoot);
            SetObjectReference(combatModeController, "rangedWeaponRoot", visualBinding.RangedWeaponRoot);
            SetObjectReference(combatModeController, "meleeWeaponRoot", visualBinding.MeleeWeaponRoot);
            SetObjectReference(combatModeController, "rangedAnimator", visualBinding.RangedAnimator);
            SetObjectReference(combatModeController, "meleeAnimator", visualBinding.MeleeAnimator);
            SetObjectReference(
                combatModeController,
                "rangedAnimatorController",
                LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath));
            SetObjectReference(
                combatModeController,
                "meleeAnimatorController",
                LoadAsset<RuntimeAnimatorController>(CombatGirlAnimatorControllerPath));
            SetBool(combatModeController, "routeAnimatorsByMode", true);
            SetBool(combatModeController, "rangedAnimatorUsesExternalPresentationBridge", true);
            SetBool(combatModeController, "useSingleCharacterVisual", true);
            SetEnum(combatModeController, "startingMode", (int)PlayerCombatMode.Ranged);
            SetObjectReference(playerActionController, "combatModeController", combatModeController);
            SetObjectReference(playerActionController, "animator", visualBinding.RangedAnimator);
            SetObjectReference(playerMovementController, "animator", visualBinding.RangedAnimator);
            SetBool(playerActionController, "blockBasicAttackInRangedMode", true);
        }

        private static void ConfigureCombatModeActionLinks(
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            SetObjectReference(combatModeController, "rangedAimController", rangedAimController);
            SetObjectReference(combatModeController, "rangedBasicAttackAction", rangedBasicAttackAction);
        }

        private static void ConfigureRifleGirlNativeBridge(
            RifleGirlNativeGameplayAnimatorBridge nativeBridge,
            Animator rangedAnimator,
            PlayerMovementController movement,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            nativeBridge.Configure(
                rangedAnimator,
                movement,
                actionController,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction);
            SetObjectReference(nativeBridge, "animator", rangedAnimator);
            SetObjectReference(nativeBridge, "movement", movement);
            SetObjectReference(nativeBridge, "actionController", actionController);
            SetObjectReference(nativeBridge, "combatModeController", combatModeController);
            SetObjectReference(nativeBridge, "rangedAimController", rangedAimController);
            SetObjectReference(nativeBridge, "rangedBasicAttackAction", rangedBasicAttackAction);
            SetString(nativeBridge, "normalIdleTrigger", "IDLE");
            SetString(nativeBridge, "normalWalkTrigger", "WALK");
            SetString(nativeBridge, "normalRunTrigger", "RUN");
            SetString(nativeBridge, "idleTrigger", "IDLE 0");
            SetString(nativeBridge, "shootTrigger", "SHOOT");
            SetString(nativeBridge, "autoShootTrigger", "AUTO SHOOT");
            SetString(nativeBridge, "jogTrigger", "JOG");
            SetString(nativeBridge, "walkForwardTrigger", "WALK F");
            SetString(nativeBridge, "walkBackTrigger", "WALK B");
            SetString(nativeBridge, "walkForwardLeftTrigger", "WALK FL");
            SetString(nativeBridge, "walkForwardRightTrigger", "WALK FR");
            SetString(nativeBridge, "walkBackLeftTrigger", "WALK BL");
            SetString(nativeBridge, "walkBackRightTrigger", "WALK BR");
            SetString(nativeBridge, "dodgeTrigger", "EVADE");
            SetBool(nativeBridge, "useNativeAutoShootLoop", false);
            SetBool(nativeBridge, "triggerAutoShootOncePerHold", true);
            SetFloat(nativeBridge, "stationaryFirePoseHoldSeconds", 0.36f);
            SetBool(nativeBridge, "keepMovingLocomotionDuringFire", true);
            SetFloat(nativeBridge, "locomotionTriggerHoldSeconds", 0.18f);
            EditorUtility.SetDirty(nativeBridge);
        }

        private static void ValidateCombatModeController(
            PlayerCombatModeController combatModeController,
            PlayerActionController playerActionController,
            PlayerMovementController playerMovementController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            GameObject rangedRoot = RequireReferencedObject<GameObject>(combatModeController, "rangedVisualRoot");
            Animator rangedAnimator = RequireReferencedObject<Animator>(combatModeController, "rangedAnimator");
            GameObject meleeRoot = RequireReferencedObject<GameObject>(combatModeController, "meleeVisualRoot");
            Animator meleeAnimator = RequireReferencedObject<Animator>(combatModeController, "meleeAnimator");
            GameObject rangedWeaponRoot = RequireReferencedObject<GameObject>(combatModeController, "rangedWeaponRoot");
            GameObject meleeWeaponRoot = RequireReferencedObject<GameObject>(combatModeController, "meleeWeaponRoot");
            ValidateObjectReference(combatModeController, "actionController", playerActionController);
            ValidateObjectReference(combatModeController, "movementController", playerMovementController);
            ValidateObjectReference(combatModeController, "rangedAimController", rangedAimController);
            ValidateObjectReference(combatModeController, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(
                combatModeController,
                "rangedActionProfile",
                LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath));
            ValidateObjectReference(
                combatModeController,
                "meleeActionProfile",
                LoadAsset<PlayerActionProfile>(MeleeActionProfilePath));
            ValidateObjectReference(
                rangedAnimator,
                "m_Controller",
                LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath));
            ValidateObjectReference(
                combatModeController,
                "rangedAnimatorController",
                LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath));
            ValidateObjectReference(
                combatModeController,
                "meleeAnimatorController",
                LoadAsset<RuntimeAnimatorController>(CombatGirlAnimatorControllerPath));
            ValidateBool(combatModeController, "routeAnimatorsByMode", true);
            ValidateBool(combatModeController, "rangedAnimatorUsesExternalPresentationBridge", true);
            ValidateBool(combatModeController, "useSingleCharacterVisual", true);
            ValidateEnum(combatModeController, "startingMode", (int)PlayerCombatMode.Ranged);
            ValidatePlayerCombatModeVisual(rangedRoot, rangedAnimator, rangedWeaponRoot, meleeWeaponRoot);
            if (meleeRoot.activeSelf)
            {
                throw new InvalidOperationException("CombatGirl source visual root must stay inactive; RifleGirl is the single visible player body.");
            }

            if (meleeAnimator != rangedAnimator)
            {
                throw new InvalidOperationException("Melee mode must reuse the RifleGirl Animator so weapon swap does not replace the character body.");
            }

            ValidateObjectReference(playerActionController, "combatModeController", combatModeController);
            ValidateObjectReference(playerActionController, "animator", rangedAnimator);
            ValidateObjectReference(playerMovementController, "animator", rangedAnimator);
            ValidateBool(playerActionController, "blockBasicAttackInRangedMode", true);
        }

        private static void ValidateRifleGirlNativeBridge(
            RifleGirlNativeGameplayAnimatorBridge nativeBridge,
            Animator rangedAnimator,
            PlayerMovementController movement,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            ValidateObjectReference(nativeBridge, "animator", rangedAnimator);
            ValidateObjectReference(nativeBridge, "movement", movement);
            ValidateObjectReference(nativeBridge, "actionController", actionController);
            ValidateObjectReference(nativeBridge, "combatModeController", combatModeController);
            ValidateObjectReference(nativeBridge, "rangedAimController", rangedAimController);
            ValidateObjectReference(nativeBridge, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateString(nativeBridge, "normalIdleTrigger", "IDLE");
            ValidateString(nativeBridge, "normalWalkTrigger", "WALK");
            ValidateString(nativeBridge, "normalRunTrigger", "RUN");
            ValidateString(nativeBridge, "idleTrigger", "IDLE 0");
            ValidateString(nativeBridge, "shootTrigger", "SHOOT");
            ValidateString(nativeBridge, "autoShootTrigger", "AUTO SHOOT");
            ValidateString(nativeBridge, "jogTrigger", "JOG");
            ValidateString(nativeBridge, "walkForwardTrigger", "WALK F");
            ValidateString(nativeBridge, "walkBackTrigger", "WALK B");
            ValidateString(nativeBridge, "walkForwardLeftTrigger", "WALK FL");
            ValidateString(nativeBridge, "walkForwardRightTrigger", "WALK FR");
            ValidateString(nativeBridge, "walkBackLeftTrigger", "WALK BL");
            ValidateString(nativeBridge, "walkBackRightTrigger", "WALK BR");
            ValidateString(nativeBridge, "dodgeTrigger", "EVADE");
            ValidateBool(nativeBridge, "useNativeAutoShootLoop", false);
            ValidateBool(nativeBridge, "triggerAutoShootOncePerHold", true);
            ValidateFloat(nativeBridge, "stationaryFirePoseHoldSeconds", 0.36f);
            ValidateBool(nativeBridge, "keepMovingLocomotionDuringFire", true);
            ValidateFloat(nativeBridge, "locomotionTriggerHoldSeconds", 0.18f);
        }

        private static BossBarragePocketReviewOwner CreatePocketOwner(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector,
            SummonLaneSpace laneSpace)
        {
            GameObject root = CreateRoot(scene, PocketOwnerRootName);
            BossBarragePocketReviewOwner owner = root.AddComponent<BossBarragePocketReviewOwner>();
            GameObject clearMarker = CreateResultMarker(
                root.transform,
                PocketClearMarkerName,
                laneSpace.GetBattlefieldWorldPoint(-laneSpace.HalfWidth - 1.35f, laneSpace.ForwardBoundaryZ - 0.5f, 0.75f),
                new Color(0.25f, 1f, 0.5f, 1f));
            GameObject failMarker = CreateResultMarker(
                root.transform,
                PocketFailMarkerName,
                laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1.35f, laneSpace.ForwardBoundaryZ - 0.5f, 0.75f),
                new Color(1f, 0.16f, 0.18f, 1f));
            owner.Configure(
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                clearMarker,
                failMarker,
                bossPressureCost,
                bossPressureActionDirector);
            SetObjectReference(
                owner,
                "summonPressureBlockOpportunity",
                LoadAsset<SummonOpportunityWindowProfile>(SummonOpportunityProfilePath));
            EditorUtility.SetDirty(owner);
            return owner;
        }

        private static GameObject CreateResultMarker(Transform parent, string name, Vector3 position, Color color)
        {
            Material material = LoadOrCreateMaterial(
                $"Assets/_Game/Art/Materials/ActionFoundation/{name}.mat",
                color);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, worldPositionStays: true);
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = new Vector3(0.75f, 1.5f, 0.75f);
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
            marker.SetActive(false);
            return marker;
        }

        private static void ConfigurePocketCueBridges(
            BossBarragePocketReviewOwner pocketOwner,
            ActionCameraCueDriver cameraCueDriver,
            PlayerCombatVfxCueDriver playerVfxCueDriver,
            CombatVfxCuePlayer cuePlayer,
            Transform directionTarget)
        {
            BossBarragePocketCameraCueBridge cameraBridge =
                EnsureComponent<BossBarragePocketCameraCueBridge>(pocketOwner.gameObject);
            SetObjectReference(cameraBridge, "pocketReviewOwner", pocketOwner);
            SetObjectReference(cameraBridge, "cameraCueDriver", cameraCueDriver);

            BossBarragePocketVfxCueBridge vfxBridge =
                EnsureComponent<BossBarragePocketVfxCueBridge>(pocketOwner.gameObject);
            SetObjectReference(vfxBridge, "pocketReviewOwner", pocketOwner);
            SetObjectReference(vfxBridge, "cuePlayer", cuePlayer);
            SetObjectReference(vfxBridge, "followupWindowAnchor", ReadObjectReference<Transform>(playerVfxCueDriver, "attackAnchor"));
            SetObjectReference(vfxBridge, "followupHitAnchor", directionTarget);
            SetObjectReference(vfxBridge, "followupMissedAnchor", ReadObjectReference<Transform>(playerVfxCueDriver, "dodgeAnchor"));
            SetObjectReference(vfxBridge, "directionTarget", directionTarget);
            EditorUtility.SetDirty(cameraBridge);
            EditorUtility.SetDirty(vfxBridge);
        }

        private static void CreateReviewHud(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            Transform player,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBarragePocketReviewOwner pocketOwner,
            BossPressureCostLadder bossPressureCost,
            BossPressurePositionController bossPressurePosition,
            BossPressureActionDirector bossPressureActionDirector,
            BossSummonPressureAction bossSummonPressureAction)
        {
            GameObject hudRoot = CreateRoot(scene, HudRootName);
            BossBarrageLaneReviewHud hud = hudRoot.AddComponent<BossBarrageLaneReviewHud>();
            hud.Configure(
                playerHealth,
                closeThreatHealth,
                bossHealth,
                energyLadder,
                laneSpace,
                player,
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                pocketOwner,
                bossPressureCost,
                bossPressurePosition,
                bossPressureActionDirector,
                bossSummonPressureAction);
            SetBool(hud, "showCenterReticle", false);
            BossBarrageLaneReviewMobileHud mobileHud = hudRoot.AddComponent<BossBarrageLaneReviewMobileHud>();
            mobileHud.Configure(
                player.GetComponent<PlayerMovementController>(),
                player.GetComponent<PlayerActionController>(),
                combatModeController,
                rangedAimController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action);
            SetBool(mobileHud, "screenDragControlsAim", true);
            SetBool(mobileHud, "rightMouseDragControlsAim", false);
            SetBool(mobileHud, "leftMouseDragControlsAim", true);
            SetBool(mobileHud, "fireDragControlsAim", true);
            // Touch/reticle composition is review-scene HUD tuning. Keep it Inspector-authored.
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(mobileHud);
        }

        private static void ConfigurePlayerEnergyActions(
            GameObject playerRoot,
            CombatHealth playerHealth,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth frontlineTargetHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            LaneActionProjectile skill1ProjectilePrefab,
            LaneActionProjectile summonSlot1ProjectilePrefab,
            GameObject summonEntryCuePrefab,
            SummonFrontlineProxy summonActorPrefab,
            Transform projectileRoot,
            Transform actionCueRoot,
            Transform summonActorRoot)
        {
            PlayerSkill1Action skill1Action = EnsureComponent<PlayerSkill1Action>(playerRoot);
            SetObjectReference(skill1Action, "energyLadder", energyLadder);
            SetObjectReference(skill1Action, "sourceHealth", playerHealth);
            SetObjectReference(skill1Action, "targetSelector", targetSelector);
            SetObjectReference(skill1Action, "projectilePrefab", skill1ProjectilePrefab);
            SetObjectReference(skill1Action, "projectilePrefabObject", LoadAsset<GameObject>(Skill1ProjectilePrefabPath));
            SetObjectReference(skill1Action, "projectileRoot", projectileRoot);
            SetEnum(skill1Action, "sourceTeam", (int)DamageTeam.Player);
            SetInt(skill1Action, "prewarmCount", 6);

            PlayerSummonSlot1Action summonSlot1Action = EnsureComponent<PlayerSummonSlot1Action>(playerRoot);
            SetObjectReference(summonSlot1Action, "energyLadder", energyLadder);
            SetObjectReference(summonSlot1Action, "sourceHealth", playerHealth);
            SetObjectReference(summonSlot1Action, "targetSelector", targetSelector);
            SetObjectReference(summonSlot1Action, "frontlineTargetHealth", frontlineTargetHealth);
            SetObjectReference(summonSlot1Action, "laneSpace", laneSpace);
            SetObjectReference(summonSlot1Action, "projectilePrefab", summonSlot1ProjectilePrefab);
            SetObjectReference(summonSlot1Action, "projectilePrefabObject", LoadAsset<GameObject>(SummonSlot1ProjectilePrefabPath));
            SetObjectReference(summonSlot1Action, "entryCuePrefab", summonEntryCuePrefab);
            SetObjectReference(summonSlot1Action, "summonActorPrefab", summonActorPrefab);
            SetObjectReference(summonSlot1Action, "summonActorPrefabObject", LoadAsset<GameObject>(SummonSlot1ActorPrefabPath));
            SetObjectReference(summonSlot1Action, "projectileRoot", projectileRoot);
            SetObjectReference(summonSlot1Action, "cueRoot", actionCueRoot);
            SetObjectReference(summonSlot1Action, "summonActorRoot", summonActorRoot);
            SetEnum(summonSlot1Action, "sourceTeam", (int)DamageTeam.AllySummon);
            SetInt(summonSlot1Action, "prewarmCount", 8);
            SetInt(summonSlot1Action, "actorPrewarmCount", 2);
            summonSlot1Action.ConfigureSummonActionProfile(
                LoadAsset<SummonSlotActionProfile>(SummonSlot1ActionProfilePath));
            EditorUtility.SetDirty(summonSlot1Action);
        }

        private static void ConfigureFixedRearCamera(
            ActionCameraController cameraController,
            Transform player,
            Transform bossProxy,
            Transform rearYawReference)
        {
            Vector3 lookTarget = player.position + CameraLookOffset;
            float orbitYaw = rearYawReference != null ? rearYawReference.eulerAngles.y : player.eulerAngles.y;
            Quaternion orbitRotation = Quaternion.Euler(0f, orbitYaw, 0f);
            Vector3 position = lookTarget + orbitRotation * CameraStartOffset;
            Vector3 lookDirection = lookTarget - position;
            cameraController.transform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(lookDirection.normalized, Vector3.up));

            Camera camera = cameraController.GetComponent<Camera>();
            if (camera != null)
            {
                camera.fieldOfView = 54f;
                EditorUtility.SetDirty(camera);
            }

            SetObjectReference(cameraController, "target", player);
            SetObjectReference(cameraController, "threat", bossProxy);
            SetVector3(cameraController, "cameraOffset", CameraStartOffset);
            SetVector3(cameraController, "lookOffset", CameraLookOffset);
            SetBool(cameraController, "useFixedRearYaw", true);
            SetObjectReference(cameraController, "fixedRearYawReference", rearYawReference);
            SetFloat(cameraController, "fixedRearYawOffsetDegrees", 0f);
            SetFloat(cameraController, "orbitYawDegrees", orbitYaw);
            SetBool(cameraController, "useDeviceFallbackWhenActionMissing", false);
            SetFloat(cameraController, "manualYawSpeedDegrees", 0f);
            SetFloat(cameraController, "mouseYawDegreesPerPixel", 0f);
            SetFloat(cameraController, "targetYawAssist", 0f);
            SetFloat(cameraController, "threatBias", 0f);
            SetFloat(cameraController, "maxThreatFocusOffset", 0.75f);
            SetFloat(cameraController, "maxLeadFromPlayerSpeed", 0f);
            // Aim framing and blend speed are scene-authored tuning.
        }

        private static void ConfigureRangedAimController(
            GameObject player,
            ActionCameraController cameraController,
            Animator rangedAnimator)
        {
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player, "player combat mode controller");
            PlayerRangedAimController aimController = EnsureComponent<PlayerRangedAimController>(player);
            aimController.ConfigureReferences(combatModeController, cameraController, rangedAnimator);
            SetObjectReference(aimController, "combatModeController", combatModeController);
            SetObjectReference(aimController, "cameraController", cameraController);
            SetObjectReference(aimController, "animator", rangedAnimator);
            SetBool(aimController, "holdToAim", true);
            // Review-only PC fallback. Production scenes should bind explicit Input Actions instead.
            SetBool(aimController, "useDeviceFallbackWhenActionMissing", true);
            SetBool(aimController, "allowMouseAimFallback", false);
            SetString(aimController, "aimingParameter", string.Empty);
        }

        private static PlayerRangedBasicAttackAction ConfigurePlayerRangedBasicAttack(
            GameObject player,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerMovementController movement,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth playerHealth,
            ActionCameraController cameraController,
            Animator rangedAnimator,
            LaneActionProjectile projectilePrefab,
            Transform projectileRoot,
            Transform fireOrigin)
        {
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                EnsureComponent<PlayerRangedBasicAttackAction>(player);
            rangedBasicAttackAction.SetFireOrigin(fireOrigin);
            rangedBasicAttackAction.ConfigureReferences(
                combatModeController,
                rangedAimController,
                movement,
                targetSelector,
                playerHealth,
                cameraController,
                rangedAnimator);
            SetObjectReference(rangedBasicAttackAction, "combatModeController", combatModeController);
            SetObjectReference(rangedBasicAttackAction, "aimController", rangedAimController);
            SetObjectReference(rangedBasicAttackAction, "movement", movement);
            SetObjectReference(rangedBasicAttackAction, "targetSelector", targetSelector);
            SetObjectReference(rangedBasicAttackAction, "sourceHealth", playerHealth);
            SetObjectReference(rangedBasicAttackAction, "cameraController", cameraController);
            SetObjectReference(rangedBasicAttackAction, "animator", rangedAnimator);
            SetObjectReference(rangedBasicAttackAction, "projectilePrefab", projectilePrefab);
            SetObjectReference(rangedBasicAttackAction, "projectilePrefabObject", LoadAsset<GameObject>(RangedBasicProjectilePrefabPath));
            SetObjectReference(rangedBasicAttackAction, "projectileRoot", projectileRoot);
            SetObjectReference(rangedBasicAttackAction, "fireOrigin", fireOrigin);
            SetEnum(rangedBasicAttackAction, "sourceTeam", (int)DamageTeam.Player);
            SetInt(rangedBasicAttackAction, "prewarmCount", 8);
            SetBool(rangedBasicAttackAction, "allowMouseFireFallback", false);
            SetBool(rangedBasicAttackAction, "snapFacingOnFire", false);
            SetBool(rangedBasicAttackAction, "suppressFacingOnFireWhileMoving", true);
            SetFloat(rangedBasicAttackAction, "movingFacingSuppressSpeed", 0.08f);
            SetString(rangedBasicAttackAction, "fireTrigger", string.Empty);
            // Damage, shot cadence, aim assist, muzzle framing, and fire camera feedback are authored tuning.
            EditorUtility.SetDirty(rangedBasicAttackAction);
            return rangedBasicAttackAction;
        }

        private static void ConfigureActionCameraCueDriver(
            ActionCameraController cameraController,
            PlayerActionController actionController,
            PlayerMovementController movement,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action)
        {
            ActionCameraCueDriver cueDriver = EnsureComponent<ActionCameraCueDriver>(cameraController.gameObject);
            SetObjectReference(cueDriver, "actionController", actionController);
            SetObjectReference(cueDriver, "movement", movement);
            SetObjectReference(cueDriver, "skill1Action", skill1Action);
            SetObjectReference(cueDriver, "summonSlot1Action", summonSlot1Action);
            SetObjectReference(cueDriver, "cameraController", cameraController);
            SetObjectReference(cueDriver, "cueSpace", movement.transform);
        }

        private static void ConfigureBossBarrageCameraCueDriver(
            ActionCameraController cameraController,
            BossBarrageEmitter bossBarrageEmitter,
            BossPressureActionDirector bossPressureActionDirector,
            Transform cueSpace)
        {
            BossBarrageCameraCueDriver cueDriver = EnsureComponent<BossBarrageCameraCueDriver>(cameraController.gameObject);
            cueDriver.Configure(bossBarrageEmitter, cameraController, cueSpace, bossPressureActionDirector);
            SetObjectReference(cueDriver, "bossBarrageEmitter", bossBarrageEmitter);
            SetObjectReference(cueDriver, "bossPressureActionDirector", bossPressureActionDirector);
            SetObjectReference(cueDriver, "cameraController", cameraController);
            SetObjectReference(cueDriver, "cueSpace", cueSpace);
            EditorUtility.SetDirty(cueDriver);
        }

        private static void ValidateFixedRearCamera(
            ActionCameraController cameraController,
            Transform player,
            Transform rearYawReference)
        {
            Vector3 planarOffset = Vector3.ProjectOnPlane(cameraController.transform.position - player.position, Vector3.up);
            if (Vector3.Dot(player.forward, planarOffset) >= -0.1f)
            {
                throw new InvalidOperationException("Boss barrage lane camera should start behind the player.");
            }

            ValidateBool(cameraController, "useFixedRearYaw", true);
            ValidateObjectReference(cameraController, "fixedRearYawReference", rearYawReference);
            ValidateBool(cameraController, "useDeviceFallbackWhenActionMissing", false);
        }

        private static void ValidateRangedAimController(
            PlayerRangedAimController aimController,
            PlayerCombatModeController combatModeController,
            ActionCameraController cameraController,
            Animator rangedAnimator)
        {
            ValidateObjectReference(aimController, "combatModeController", combatModeController);
            ValidateObjectReference(aimController, "cameraController", cameraController);
            ValidateObjectReference(aimController, "animator", rangedAnimator);
            ValidateBool(aimController, "holdToAim", true);
            ValidateBool(aimController, "allowMouseAimFallback", false);
            ValidateString(aimController, "aimingParameter", string.Empty);
        }

        private static void ValidatePlayerRangedBasicAttack(
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerMovementController movement,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth playerHealth,
            ActionCameraController cameraController,
            Animator rangedAnimator,
            Transform projectileRoot,
            Transform fireOrigin)
        {
            ValidateObjectReference(rangedBasicAttackAction, "combatModeController", combatModeController);
            ValidateObjectReference(rangedBasicAttackAction, "aimController", rangedAimController);
            ValidateObjectReference(rangedBasicAttackAction, "movement", movement);
            ValidateObjectReference(rangedBasicAttackAction, "targetSelector", targetSelector);
            ValidateObjectReference(rangedBasicAttackAction, "sourceHealth", playerHealth);
            ValidateObjectReference(rangedBasicAttackAction, "cameraController", cameraController);
            ValidateObjectReference(rangedBasicAttackAction, "animator", rangedAnimator);
            ValidateObjectReference(
                rangedBasicAttackAction,
                "projectilePrefabObject",
                LoadAsset<GameObject>(RangedBasicProjectilePrefabPath));
            ValidateObjectReference(rangedBasicAttackAction, "projectileRoot", projectileRoot);
            ValidateObjectReference(rangedBasicAttackAction, "fireOrigin", fireOrigin);
            ValidateEnum(rangedBasicAttackAction, "sourceTeam", (int)DamageTeam.Player);
            ValidateInt(rangedBasicAttackAction, "prewarmCount", 8);
            ValidateBool(rangedBasicAttackAction, "allowMouseFireFallback", false);
            ValidateBool(rangedBasicAttackAction, "snapFacingOnFire", false);
            ValidateBool(rangedBasicAttackAction, "suppressFacingOnFireWhileMoving", true);
            ValidateFloat(rangedBasicAttackAction, "movingFacingSuppressSpeed", 0.08f);
            ValidateString(rangedBasicAttackAction, "fireTrigger", string.Empty);
        }

        private static void ValidateActionCameraCueDriver(
            ActionCameraCueDriver cueDriver,
            PlayerActionController actionController,
            PlayerMovementController movement,
            ActionCameraController cameraController,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action)
        {
            ValidateObjectReference(cueDriver, "actionController", actionController);
            ValidateObjectReference(cueDriver, "movement", movement);
            ValidateObjectReference(cueDriver, "skill1Action", skill1Action);
            ValidateObjectReference(cueDriver, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(cueDriver, "cameraController", cameraController);
            ValidateObjectReference(cueDriver, "cueSpace", movement.transform);
        }

        private static void ValidateBossBarrageCameraCueDriver(
            BossBarrageCameraCueDriver cueDriver,
            ActionCameraController cameraController,
            BossBarrageEmitter bossBarrageEmitter,
            Transform cueSpace)
        {
            ValidateObjectReference(cueDriver, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(
                cueDriver,
                "bossPressureActionDirector",
                RequireComponent<BossPressureActionDirector>(bossBarrageEmitter.gameObject, "boss pressure action director"));
            ValidateObjectReference(cueDriver, "cameraController", cameraController);
            ValidateObjectReference(cueDriver, "cueSpace", cueSpace);
        }

        private static void ValidateBossBarrageLaneTelegraphPresenter(
            BossBarrageLaneTelegraphPresenter presenter,
            BossBarrageEmitter bossBarrageEmitter,
            SummonLaneSpace laneSpace)
        {
            ValidateObjectReference(presenter, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(presenter, "laneSpace", laneSpace);
            ValidateAssignedObjectReference(presenter, "markerRoot");

            if (presenter.MarkerCount < 9)
            {
                throw new InvalidOperationException("Boss barrage lane telegraph presenter should own nine authored marker slots.");
            }

            for (int i = 0; i < 9; i++)
            {
                ValidateArrayAssignedReference(presenter, "markerTransforms", i);
                Renderer renderer = ValidateArrayAssignedReference<Renderer>(presenter, "markerRenderers", i);
                ValidateGameOwnedAsset(renderer.sharedMaterial, $"boss barrage telegraph marker {i} material");
            }
        }

        private static void ValidateEnergyRiskZoneMarkers(Scene scene, SummonLaneSpace laneSpace)
        {
            Transform root = RequireRoot(scene, EnergyZoneRootName).transform;
            ValidateEnergyRiskZoneMarker(
                root,
                "BackSafety_ENSlow_0_33",
                laneSpace,
                0f,
                1f / 3f,
                BacklineEnergyZoneMaterialPath);
            ValidateEnergyRiskZoneMarker(
                root,
                "MidCharge_ENBase_33_66",
                laneSpace,
                1f / 3f,
                2f / 3f,
                MidEnergyZoneMaterialPath);
            ValidateEnergyRiskZoneMarker(
                root,
                "ForwardRisk_ENFast_66_100",
                laneSpace,
                2f / 3f,
                1f,
                ForwardEnergyZoneMaterialPath);
        }

        private static void ValidateEnergyRiskZoneMarker(
            Transform root,
            string markerName,
            SummonLaneSpace laneSpace,
            float startRisk01,
            float endRisk01,
            string materialPath)
        {
            Transform marker = root.Find(markerName);
            if (marker == null)
            {
                throw new InvalidOperationException($"Missing energy risk zone marker {markerName}.");
            }

            float startZ = Mathf.Lerp(laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ, startRisk01);
            float endZ = Mathf.Lerp(laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ, endRisk01);
            float expectedCenterZ = (startZ + endZ) * 0.5f;
            float expectedDepth = Mathf.Abs(endZ - startZ);
            Vector2 coordinates = laneSpace.GetLaneCoordinates(marker.position);
            if (Mathf.Abs(coordinates.y - expectedCenterZ) > 0.05f)
            {
                throw new InvalidOperationException($"{markerName} is not centered in the expected lane zone.");
            }

            if (Mathf.Abs(marker.localScale.z - expectedDepth) > 0.05f)
            {
                throw new InvalidOperationException($"{markerName} does not cover the expected lane depth.");
            }

            if (Mathf.Abs(marker.localScale.x - laneSpace.HalfWidth * 2f) > 0.05f)
            {
                throw new InvalidOperationException($"{markerName} does not cover the player lane width.");
            }

            if (marker.GetComponent<Collider>() != null)
            {
                throw new InvalidOperationException($"{markerName} must remain visual-only and not block movement.");
            }

            Renderer renderer = RequireComponent<Renderer>(marker.gameObject, markerName);
            ValidateObjectReference(renderer, "m_Materials.Array.data[0]", LoadAsset<Material>(materialPath));
            ValidateGameOwnedAsset(renderer.sharedMaterial, $"{markerName} material");
        }

        private static void ValidateSummonForwardSpace(SummonLaneSpace laneSpace)
        {
            Vector3 playerIllegalPoint = laneSpace.GetLaneWorldPoint(0f, laneSpace.BossProxyZ, 0f);
            Vector3 clamped = laneSpace.ClampPlayerPosition(playerIllegalPoint);
            if (laneSpace.IsPastForwardBoundary(clamped))
            {
                throw new InvalidOperationException("Player clamp must keep the player before the forward boundary.");
            }

            Vector3 summonEntry = laneSpace.GetLaneWorldPoint(0f, laneSpace.SummonEntryZ, 0f);
            if (!laneSpace.IsPastForwardBoundary(summonEntry))
            {
                throw new InvalidOperationException("Summon entry must remain valid beyond the player forward boundary.");
            }

            Vector3 offLaneSummonPoint = laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1f, laneSpace.SummonEntryZ, 0f);
            if (laneSpace.GetLaneCoordinates(offLaneSummonPoint).x <= laneSpace.HalfWidth)
            {
                throw new InvalidOperationException("Summon battlefield coordinates must be able to cross lateral lane rails.");
            }
        }

        private static void ValidatePlayerEnergyActions(
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            SummonEnergyLadder energyLadder,
            CombatHealth playerHealth,
            PlayerCombatTargetSelector targetSelector,
            CombatHealth frontlineTargetHealth,
            SummonLaneSpace laneSpace)
        {
            GameObject projectileRoot = RequireRoot(SceneManager.GetActiveScene(), ProjectilePoolRootName);
            GameObject actionCueRoot = RequireRoot(SceneManager.GetActiveScene(), ActionCuePoolRootName);
            GameObject summonActorRoot = RequireRoot(SceneManager.GetActiveScene(), SummonActorPoolRootName);

            ValidateObjectReference(skill1Action, "energyLadder", energyLadder);
            ValidateObjectReference(skill1Action, "sourceHealth", playerHealth);
            ValidateObjectReference(skill1Action, "targetSelector", targetSelector);
            ValidateObjectReference(skill1Action, "projectilePrefabObject", LoadAsset<GameObject>(Skill1ProjectilePrefabPath));
            ValidateObjectReference(skill1Action, "projectileRoot", projectileRoot.transform);
            ValidateEnum(skill1Action, "sourceTeam", (int)DamageTeam.Player);

            ValidateObjectReference(summonSlot1Action, "energyLadder", energyLadder);
            ValidateObjectReference(summonSlot1Action, "sourceHealth", playerHealth);
            ValidateObjectReference(summonSlot1Action, "targetSelector", targetSelector);
            ValidateObjectReference(summonSlot1Action, "frontlineTargetHealth", frontlineTargetHealth);
            ValidateObjectReference(summonSlot1Action, "laneSpace", laneSpace);
            ValidateObjectReference(summonSlot1Action, "projectilePrefabObject", LoadAsset<GameObject>(SummonSlot1ProjectilePrefabPath));
            ValidateObjectReference(summonSlot1Action, "entryCuePrefab", LoadAsset<GameObject>(SummonSlot1EntryCuePrefabPath));
            ValidateObjectReference(summonSlot1Action, "summonActorPrefabObject", LoadAsset<GameObject>(SummonSlot1ActorPrefabPath));
            ValidateObjectReference(summonSlot1Action, "projectileRoot", projectileRoot.transform);
            ValidateObjectReference(summonSlot1Action, "cueRoot", actionCueRoot.transform);
            ValidateObjectReference(summonSlot1Action, "summonActorRoot", summonActorRoot.transform);
            ValidateEnum(summonSlot1Action, "sourceTeam", (int)DamageTeam.AllySummon);
            SummonSlotActionProfile summonSlot1Profile = LoadAsset<SummonSlotActionProfile>(SummonSlot1ActionProfilePath);
            ValidateObjectReference(
                summonSlot1Action,
                "summonActionProfile",
                summonSlot1Profile);
            ValidateSummonSlotReadout(
                summonSlot1Profile,
                1,
                "LV1 Guard Entry",
                "Emergency pressure screen for urgent boss fire after close-threat relief.",
                "Spend early when the pocket needs an immediate boss-fire block.",
                "Small ShieldBreaker entry, two-shot screen, one assist bolt.");
            ValidateSummonSlotReadout(
                summonSlot1Profile,
                2,
                "LV2 Frontline Push",
                "Mid-tier exchange that starts converting a successful block into forward damage.",
                "Hold forward-risk long enough for LV2 when the barrage is readable.",
                "Wider screen, four-shot block budget, two assist bolts.");
            ValidateSummonSlotReadout(
                summonSlot1Profile,
                3,
                "LV3 Break Window",
                "High-risk payoff that should visibly win the pressure exchange and open the Skill1 follow-up.",
                "Save for hard boss pressure when retreat alone will not stabilize the pocket.",
                "Large ShieldBreaker screen, seven-shot block budget, three assist bolts.");

            SummonFrontlineProxy summonActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot1ActorPrefabPath);
            SummonPressureScreen pressureScreen = LoadPrefabComponent<SummonPressureScreen>(SummonSlot1ActorPrefabPath);
            SummonPressureScreenPresenter presenter =
                LoadPrefabComponent<SummonPressureScreenPresenter>(SummonSlot1ActorPrefabPath);
            SummonFrontlineProxyPresenter actorPresenter =
                LoadPrefabComponent<SummonFrontlineProxyPresenter>(SummonSlot1ActorPrefabPath);
            Transform pressureScreenVisual = summonActorPrefab.transform.Find("PressureScreenVisual");
            if (pressureScreenVisual == null)
            {
                throw new InvalidOperationException("SummonSlot1 actor prefab is missing PressureScreenVisual.");
            }

            MeshRenderer pressureScreenRenderer = pressureScreenVisual.GetComponent<MeshRenderer>();
            if (pressureScreenRenderer == null)
            {
                throw new InvalidOperationException("PressureScreenVisual is missing a MeshRenderer.");
            }

            Transform tierPulseCore = summonActorPrefab.transform.Find("TierPulseCore");
            if (tierPulseCore == null)
            {
                throw new InvalidOperationException("SummonSlot1 actor prefab is missing TierPulseCore.");
            }

            MeshRenderer pulseRenderer = tierPulseCore.GetComponent<MeshRenderer>();
            if (pulseRenderer == null)
            {
                throw new InvalidOperationException("TierPulseCore is missing a MeshRenderer.");
            }

            ValidateObjectReference(summonActorPrefab, "pressureScreen", pressureScreen);
            ValidateEnum(pressureScreen, "ownerTeam", (int)DamageTeam.AllySummon);
            ValidateInt(pressureScreen, "defaultMaxIntercepts", 2);
            ValidateFloat(pressureScreen, "defaultLifetimeSeconds", 1.2f);
            ValidateFloat(pressureScreen, "defaultRadius", 1.35f);
            ValidateObjectReference(presenter, "pressureScreen", pressureScreen);
            ValidateObjectReference(presenter, "visualRoot", pressureScreenVisual);
            ValidateArrayReference(presenter, "screenRenderers", 0, pressureScreenRenderer);
            ValidateObjectReference(actorPresenter, "proxy", summonActorPrefab);
            ValidateObjectReference(actorPresenter, "pulseRoot", tierPulseCore);
            Transform summonActorVisual = ValidateSummonActorRoleVisual(
                summonActorPrefab.gameObject,
                SummonSlot1ActorVisualName);
            Renderer[] summonActorVisualRenderers = CollectEnabledRenderers(summonActorVisual.gameObject);
            ValidateArrayContainsReference(
                actorPresenter,
                "actorRenderers",
                summonActorVisualRenderers[0],
                $"{SummonSlot1ActorVisualName} renderer");
            ValidateArrayContainsReference(
                actorPresenter,
                "actorRenderers",
                pulseRenderer,
                "TierPulseCore renderer");
            ValidateFloat(actorPresenter, "entryFlashSeconds", 0.22f);
            ValidateFloat(actorPresenter, "impactFlashSeconds", 0.18f);
            ValidateFloat(actorPresenter, "impactFlashProgress", 0.86f);
            ValidateFloat(actorPresenter, "pulseSpeed", 8f);
            ValidateFloat(actorPresenter, "pulseScale", 0.08f);
            ValidateFloat(actorPresenter, "tierScaleStep", 0.18f);
            ValidateFloat(actorPresenter, "flashScale", 0.22f);
        }

        private static void ValidateBossPressureLoop(
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector,
            BossSummonPressureAction bossSummonPressureAction,
            BossPressurePositionController bossPressurePosition,
            SummonLaneSpace laneSpace,
            Transform bossTransform,
            BossBarrageEmitter bossBarrageEmitter,
            Transform playerTransform)
        {
            ValidateObjectReference(bossPressureCost, "laneSpace", laneSpace);
            ValidateObjectReference(bossPressureCost, "trackedBoss", bossTransform);
            ValidateFloat(bossPressureCost, "baseCostPerSecond", 18f);
            ValidateFloat(bossPressureCost, "fallbackBossForwardRisk01", 0.25f);

            ValidateObjectReference(bossPressurePosition, "laneSpace", laneSpace);
            ValidateObjectReference(bossPressurePosition, "costLadder", bossPressureCost);
            ValidateObjectReference(bossPressurePosition, "actionDirector", bossPressureActionDirector);
            ValidateObjectReference(bossPressurePosition, "movedTransform", bossTransform);
            ValidateFloat(bossPressurePosition, "restRisk01", 0.08f);
            ValidateFloat(bossPressurePosition, "maxCommitRisk01", 0.62f);
            ValidateFloat(bossPressurePosition, "advanceRiskPerSecond", 0.25f);
            ValidateFloat(bossPressurePosition, "retreatRiskPerSecond", 0.42f);
            ValidateBool(bossPressurePosition, "returnToRestWhenActionsDisabled", true);
            ValidateBool(bossPressurePosition, "movementEnabled", true);

            ValidateObjectReference(bossSummonPressureAction, "laneSpace", laneSpace);
            ValidateObjectReference(bossSummonPressureAction, "trackedPlayer", playerTransform);
            ValidateObjectReference(
                bossSummonPressureAction,
                "summonActorPrefabObject",
                LoadAsset<GameObject>(BossSummonPressureActorPrefabPath));
            ValidateObjectReference(
                bossSummonPressureAction,
                "summonActorRoot",
                RequireRoot(SceneManager.GetActiveScene(), BossSummonActorPoolRootName).transform);
            ValidateEnum(bossSummonPressureAction, "ownerTeam", (int)DamageTeam.Enemy);
            ValidateInt(bossSummonPressureAction, "actorPrewarmCount", 2);
            BossSummonPressureProfile bossSummonPressureProfile = LoadAsset<BossSummonPressureProfile>(BossSummonPressureProfilePath);
            ValidateObjectReference(
                bossSummonPressureAction,
                "pressureProfile",
                bossSummonPressureProfile);
            ValidateBossSummonPressureReadout(
                bossSummonPressureProfile,
                1,
                "LV1 Escort Probe",
                "Low-cost boss proxy that tests whether the player can keep firing lanes clear.",
                "Strafe or use basic fire; do not spend summon unless pressure stacks.",
                "Usually save SummonSlot1 for the next boss screen.");
            ValidateBossSummonPressureReadout(
                bossSummonPressureProfile,
                2,
                "LV2 Pressure Screen",
                "Boss-side summon pressure that contests the frontline and can block player follow-up shots.",
                "Take EN only long enough to prepare a clean response.",
                "Use SummonSlot1 screen to absorb the boss curtain and reopen Skill1.");
            ValidateBossSummonPressureReadout(
                bossSummonPressureProfile,
                3,
                "LV3 Clamp Guard",
                "High-cost boss proxy that punishes overextension and forces a high-tier answer or retreat.",
                "Retreat from forward-risk lanes before firing back.",
                "A saved LV2/LV3 SummonSlot1 answer should create the relief window.");

            SummonFrontlineProxy bossSummonActorPrefab =
                LoadPrefabComponent<SummonFrontlineProxy>(BossSummonPressureActorPrefabPath);
            SummonFrontlineProxyPresenter bossSummonActorPresenter =
                LoadPrefabComponent<SummonFrontlineProxyPresenter>(BossSummonPressureActorPrefabPath);
            Transform bossSummonVisual = ValidateSummonActorRoleVisual(
                bossSummonActorPrefab.gameObject,
                BossSummonPressureActorVisualName);
            Renderer[] bossSummonVisualRenderers = CollectEnabledRenderers(bossSummonVisual.gameObject);
            Transform tierPressureCore = bossSummonActorPrefab.transform.Find("TierPressureCore");
            if (tierPressureCore == null)
            {
                throw new InvalidOperationException("Boss summon pressure actor prefab is missing TierPressureCore.");
            }

            MeshRenderer tierPressureRenderer = tierPressureCore.GetComponent<MeshRenderer>();
            if (tierPressureRenderer == null)
            {
                throw new InvalidOperationException("TierPressureCore is missing a MeshRenderer.");
            }

            ValidateObjectReference(bossSummonActorPresenter, "proxy", bossSummonActorPrefab);
            ValidateObjectReference(bossSummonActorPresenter, "pulseRoot", tierPressureCore);
            ValidateArrayContainsReference(
                bossSummonActorPresenter,
                "actorRenderers",
                bossSummonVisualRenderers[0],
                $"{BossSummonPressureActorVisualName} renderer");
            ValidateArrayContainsReference(
                bossSummonActorPresenter,
                "actorRenderers",
                tierPressureRenderer,
                "TierPressureCore renderer");

            ValidateObjectReference(bossPressureActionDirector, "costLadder", bossPressureCost);
            ValidateObjectReference(bossPressureActionDirector, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(bossPressureActionDirector, "summonPressureAction", bossSummonPressureAction);
            ValidateObjectReference(bossPressureActionDirector, "laneSpace", laneSpace);
            ValidateObjectReference(bossPressureActionDirector, "trackedPlayer", playerTransform);
            ValidateObjectReference(
                bossPressureActionDirector,
                "actionDeckProfile",
                LoadAsset<BossPressureActionDeckProfile>(BossPressureActionDeckProfilePath));
            ValidateBool(bossPressureActionDirector, "actionsEnabled", true);
            ValidateFloat(bossPressureActionDirector, "globalRecoverySeconds", 0.35f);
            ValidateBossPressureActionSlot(
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
            ValidateBossPressureActionSlot(
                bossPressureActionDirector,
                1,
                LoadAsset<BossBarragePatternProfile>(EscortScreenPatternProfilePath),
                BossPressureActionKind.SummonPressure,
                2,
                "SummonSlot1PressureBlock",
                "LV2 summon-pressure exchange that tests whether the player can answer boss fire with a frontline summon screen.",
                "Hold forward-risk only long enough to charge EN, then create space for the summon block.",
                "Spend SummonSlot1 to place a pressure screen and intercept the boss curtain.",
                true,
                0.32f,
                1f);
            ValidateBossPressureActionSlot(
                bossPressureActionDirector,
                2,
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
        }

        private static void ValidateBossPressureActionSlot(
            BossPressureActionDirector bossPressureActionDirector,
            int index,
            BossBarragePatternProfile expectedPattern,
            BossPressureActionKind expectedKind,
            int expectedMinimumTier,
            string expectedResponseId,
            string expectedStageLoopRole,
            string expectedPlayerAnswer,
            string expectedSummonAnswer,
            bool expectedUsePlayerForwardRiskGate,
            float expectedMinimumPlayerForwardRisk01,
            float expectedMaximumPlayerForwardRisk01)
        {
            if (!bossPressureActionDirector.TryGetActionSlot(
                    index,
                    out BossPressureActionDirector.BossPressureActionSlot slot))
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} is missing.");
            }

            if (slot.Pattern != expectedPattern)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} points to the wrong pattern.");
            }

            if (slot.ActionKind != expectedKind)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong action kind.");
            }

            if (slot.MinimumTier != expectedMinimumTier)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong minimum tier.");
            }

            if (!slot.HasResponsePlan)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} is missing its response plan.");
            }

            ValidateString(slot.ResponseId, expectedResponseId, $"Boss pressure action slot {index} has the wrong response id.");
            ValidateString(slot.StageLoopRole, expectedStageLoopRole, $"Boss pressure action slot {index} has the wrong stage-loop role.");
            ValidateString(slot.PlayerAnswer, expectedPlayerAnswer, $"Boss pressure action slot {index} has the wrong player answer.");
            ValidateString(slot.SummonAnswer, expectedSummonAnswer, $"Boss pressure action slot {index} has the wrong summon answer.");

            if (slot.UsePlayerForwardRiskGate != expectedUsePlayerForwardRiskGate)
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong player risk gate setting.");
            }

            if (!Mathf.Approximately(slot.MinimumPlayerForwardRisk01, expectedMinimumPlayerForwardRisk01)
                || !Mathf.Approximately(slot.MaximumPlayerForwardRisk01, expectedMaximumPlayerForwardRisk01))
            {
                throw new InvalidOperationException($"Boss pressure action slot {index} has the wrong player risk gate range.");
            }
        }

        private static void ValidateSummonSlotReadout(
            SummonSlotActionProfile profile,
            int tier,
            string expectedTierLabel,
            string expectedStageRole,
            string expectedPlayerUse,
            string expectedSummonRead)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("SummonSlot1 action profile is missing.");
            }

            if (!profile.TryGetTierReadout(tier, out SummonSlotActionProfile.SummonTierReadout readout))
            {
                throw new InvalidOperationException($"SummonSlot1 profile is missing tier {tier} readout.");
            }

            ValidateString(readout.TierLabel, expectedTierLabel, $"SummonSlot1 tier {tier} has the wrong label.");
            ValidateString(readout.StageRole, expectedStageRole, $"SummonSlot1 tier {tier} has the wrong stage role.");
            ValidateString(readout.PlayerUse, expectedPlayerUse, $"SummonSlot1 tier {tier} has the wrong player-use note.");
            ValidateString(readout.SummonRead, expectedSummonRead, $"SummonSlot1 tier {tier} has the wrong summon-read note.");
        }

        private static void ValidateBossSummonPressureReadout(
            BossSummonPressureProfile profile,
            int tier,
            string expectedTierLabel,
            string expectedStageRole,
            string expectedPlayerRead,
            string expectedSummonRead)
        {
            if (profile == null)
            {
                throw new InvalidOperationException("Boss summon pressure profile is missing.");
            }

            if (!profile.TryGetTierReadout(tier, out BossSummonPressureProfile.BossSummonTierReadout readout))
            {
                throw new InvalidOperationException($"Boss summon pressure profile is missing tier {tier} readout.");
            }

            ValidateString(readout.TierLabel, expectedTierLabel, $"Boss summon pressure tier {tier} has the wrong label.");
            ValidateString(readout.StageRole, expectedStageRole, $"Boss summon pressure tier {tier} has the wrong stage role.");
            ValidateString(readout.PlayerRead, expectedPlayerRead, $"Boss summon pressure tier {tier} has the wrong player-read note.");
            ValidateString(readout.SummonRead, expectedSummonRead, $"Boss summon pressure tier {tier} has the wrong summon-read note.");
        }

        private static void ValidateCloseThreat(
            GameObject closeThreat,
            CombatHealth closeThreatHealth,
            CombatHealth playerHealth,
            ActionCameraController cameraController)
        {
            BasicSoldierEnemy soldier = RequireComponent<BasicSoldierEnemy>(closeThreat, "close threat soldier");
            CombatTargetSensor targetSensor = RequireComponent<CombatTargetSensor>(closeThreat, "close threat target sensor");
            EnemyActionCameraCueDriver cameraCueDriver =
                RequireComponent<EnemyActionCameraCueDriver>(closeThreat, "close threat camera cue driver");

            ValidateObjectReference(targetSensor, "selfHealth", closeThreatHealth);
            ValidateArrayReference(targetSensor, "targetCandidates", 0, playerHealth);
            ValidateObjectReference(soldier, "targetSensor", targetSensor);
            ValidateObjectReference(soldier, "selfHealth", closeThreatHealth);
            ValidateObjectReference(cameraCueDriver, "agentSource", soldier);
            ValidateObjectReference(cameraCueDriver, "cameraController", cameraController);
            ValidateObjectReference(cameraCueDriver, "cueSpace", closeThreat.transform);
            ValidateFloat(closeThreatHealth, "maxHealth", 72f);
        }

        private static void ValidateBossProxyVisual(GameObject bossProxy)
        {
            Transform visual = bossProxy.transform.Find(BossProxyHumanoidVisualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyHumanoidVisualName}.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} should keep the promoted SummonCaller Animator.");
            }

            ValidateGameOwnedAsset(animator.runtimeAnimatorController, $"{BossProxyHumanoidVisualName} Animator Controller");

            if (visual.GetComponentInChildren<CombatHealth>(true) != null
                || visual.GetComponentInChildren<BasicSoldierEnemy>(true) != null
                || visual.GetComponentInChildren<CombatTargetSensor>(true) != null
                || visual.GetComponentInChildren<EnemyElitePatternController>(true) != null)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} must be visual-only and must not duplicate enemy gameplay components.");
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{BossProxyHumanoidVisualName} should expose promoted renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ValidateRendererAssets(renderers[i], $"{BossProxyHumanoidVisualName}.{renderers[i].name}");
            }

            Transform projectileCore = bossProxy.transform.Find(BossProxyMarkerName);
            if (projectileCore == null)
            {
                throw new InvalidOperationException($"Boss proxy should include {BossProxyMarkerName} as the readable projectile source core.");
            }

            MeshRenderer projectileCoreRenderer = projectileCore.GetComponent<MeshRenderer>();
            if (projectileCoreRenderer == null || projectileCoreRenderer.sharedMaterial == null)
            {
                throw new InvalidOperationException($"{BossProxyMarkerName} should keep a game-owned material.");
            }

            ValidateGameOwnedAsset(projectileCoreRenderer.sharedMaterial, $"{BossProxyMarkerName} material");

            BossBarrageVisualCueDriver cueDriver = RequireComponent<BossBarrageVisualCueDriver>(
                bossProxy,
                "boss barrage visual cue driver");
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            if (cueDriver.BossBarrageEmitter != emitter)
            {
                throw new InvalidOperationException("Boss visual cue driver should read from the boss barrage emitter.");
            }

            if (cueDriver.BossPressureActionDirector != bossPressureActionDirector)
            {
                throw new InvalidOperationException("Boss visual cue driver should read boss pressure action selections.");
            }

            if (cueDriver.Animator != animator)
            {
                throw new InvalidOperationException("Boss visual cue driver should drive the promoted humanoid Animator.");
            }

            if (cueDriver.PulseRoot != projectileCore)
            {
                throw new InvalidOperationException("Boss visual cue driver should pulse the authored projectile source core.");
            }

            if (cueDriver.PatternCueCount < 10)
            {
                throw new InvalidOperationException("Boss visual cue driver should map every current boss barrage pattern.");
            }

            ValidateBossVisualCueBindings(cueDriver, animator);
            ValidateBossPressureActionCueBindings(cueDriver, animator);

            if (cueDriver.PulseRendererCount <= 0)
            {
                throw new InvalidOperationException("Boss visual cue driver should have at least one pulse renderer.");
            }
        }

        private static Transform AttachRoleVisualOnly(
            Transform parent,
            string roleId,
            string rolePrefabPath,
            string targetVisualName,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            string visualPrefix = targetVisualName.Contains("_", StringComparison.Ordinal)
                ? targetVisualName.Substring(0, targetVisualName.LastIndexOf('_') + 1)
                : targetVisualName;
            RemoveChildrenWithPrefix(parent, visualPrefix);

            EnemyRoleVisualSpec visualSpec = ActionFoundationEnemyRoleVisualSetup.CreateForRole(roleId);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(rolePrefabPath);
            try
            {
                Transform sourceVisual = prefabContents.transform.Find(visualSpec.VisualName);
                if (sourceVisual == null)
                {
                    throw new InvalidOperationException($"{rolePrefabPath} is missing {visualSpec.VisualName}.");
                }

                GameObject visual = UnityEngine.Object.Instantiate(sourceVisual.gameObject);
                visual.name = targetVisualName;
                visual.transform.SetParent(parent, worldPositionStays: false);
                visual.transform.localPosition = localPosition;
                visual.transform.localRotation = Quaternion.Euler(localEulerAngles);
                visual.transform.localScale = localScale;
                ValidateSummonActorRoleVisualContents(visual, targetVisualName);
                return visual.transform;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        private static void RemoveChildrenWithPrefix(Transform parent, string prefix)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static UnityEngine.Object[] BuildRendererReferenceArray(GameObject visualRoot, Renderer pulseRenderer)
        {
            Renderer[] renderers = CollectEnabledRenderers(visualRoot);
            var references = new UnityEngine.Object[renderers.Length + 1];
            for (int i = 0; i < renderers.Length; i++)
            {
                references[i] = renderers[i];
            }

            references[references.Length - 1] = pulseRenderer;
            return references;
        }

        private static Transform ValidateSummonActorRoleVisual(GameObject prefabRoot, string visualName)
        {
            Transform visual = prefabRoot.transform.Find(visualName);
            if (visual == null)
            {
                throw new InvalidOperationException($"{prefabRoot.name} should include {visualName}.");
            }

            ValidateSummonActorRoleVisualContents(visual.gameObject, visualName);
            return visual;
        }

        private static void ValidateSummonPresentationCandidateProfiles()
        {
            CombatVfxCueProfile vfxCueProfile =
                LoadAsset<CombatVfxCueProfile>(ActionFoundationCombatVfxSetup.CombatVfxCueProfilePath);

            ValidateSummonPresentationCandidateProfile(
                LoadAsset<SummonPresentationCandidateProfile>(SummonSlot1PresentationCandidateProfilePath),
                "PlayerSummon.ShieldBreaker",
                SummonPresentationSide.PlayerSummon,
                SummonSlot1ActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.ShieldBreakerEliteCandidateProfilePath,
                SummonSlot1ActorVisualName,
                SummonSlot1ActorVisualRoleId,
                vfxCueProfile);

            ValidateSummonPresentationCandidateProfile(
                LoadAsset<SummonPresentationCandidateProfile>(BossSummonPressurePresentationCandidateProfilePath),
                "BossPressure.AuraCaptain",
                SummonPresentationSide.BossPressure,
                BossSummonPressureActorPrefabPath,
                ActionFoundationEnemyRoleCandidateSetup.AuraCaptainEliteCandidateProfilePath,
                BossSummonPressureActorVisualName,
                BossSummonPressureActorVisualRoleId,
                vfxCueProfile);
        }

        private static void ValidateSummonPresentationCandidateProfile(
            SummonPresentationCandidateProfile profile,
            string expectedCandidateId,
            SummonPresentationSide expectedSide,
            string actorPrefabPath,
            string roleCandidateProfilePath,
            string visualChildName,
            string sourceRoleId,
            CombatVfxCueProfile vfxCueProfile)
        {
            GameObject actorPrefab = LoadAsset<GameObject>(actorPrefabPath);
            CombatEnemyRoleCandidateProfile roleCandidate =
                LoadAsset<CombatEnemyRoleCandidateProfile>(roleCandidateProfilePath);
            RuntimeAnimatorController animatorController =
                ResolveActorVisualAnimatorController(actorPrefab, visualChildName);

            if (!string.Equals(profile.CandidateId, expectedCandidateId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.name} has the wrong summon presentation candidate id.");
            }

            if (profile.Side != expectedSide)
            {
                throw new InvalidOperationException($"{profile.name} has the wrong summon presentation side.");
            }

            if (profile.ActorPrefab != actorPrefab)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong actor prefab.");
            }

            if (profile.VisualSourceAsset != roleCandidate.PromotedVisualSource)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong promoted visual source.");
            }

            if (!string.Equals(profile.VisualChildName, visualChildName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.name} has the wrong visual child name.");
            }

            if (!string.Equals(profile.SourceRoleId, sourceRoleId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.name} has the wrong source role id.");
            }

            if (profile.AnimatorController != animatorController)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong Animator controller.");
            }

            if (profile.VfxCueProfile != vfxCueProfile)
            {
                throw new InvalidOperationException($"{profile.name} points to the wrong VFX cue profile.");
            }

            if (string.IsNullOrWhiteSpace(profile.DisplayName)
                || string.IsNullOrWhiteSpace(profile.AnimationRead)
                || string.IsNullOrWhiteSpace(profile.VfxRead)
                || string.IsNullOrWhiteSpace(profile.ReplacementPlan)
                || string.IsNullOrWhiteSpace(profile.OwnershipNotes))
            {
                throw new InvalidOperationException($"{profile.name} should document display, animation, VFX, replacement, and ownership notes.");
            }

            ValidateGameOwnedAsset(profile, $"{profile.name} asset");
            ValidateGameOwnedAsset(profile.ActorPrefab, $"{profile.name} actor prefab");
            ValidateGameOwnedAsset(profile.VisualSourceAsset, $"{profile.name} visual source");
            ValidateGameOwnedAsset(profile.AnimatorController, $"{profile.name} Animator controller");
            ValidateGameOwnedAsset(profile.VfxCueProfile, $"{profile.name} VFX cue profile");
            Transform visual = ValidateSummonActorRoleVisual(actorPrefab, visualChildName);
            ValidateSummonActorRoleVisualContents(visual.gameObject, profile.name);
        }

        private static void ValidateSummonActorRoleVisualContents(GameObject visual, string label)
        {
            if (visual.GetComponentInChildren<CombatHealth>(true) != null
                || visual.GetComponentInChildren<BasicSoldierEnemy>(true) != null
                || visual.GetComponentInChildren<CombatTargetSensor>(true) != null
                || visual.GetComponentInChildren<EnemyElitePatternController>(true) != null)
            {
                throw new InvalidOperationException($"{label} must be visual-only and must not duplicate enemy gameplay components.");
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException($"{label} should keep a promoted role Animator.");
            }

            ValidateGameOwnedAsset(animator.runtimeAnimatorController, $"{label} Animator Controller");

            Renderer[] renderers = CollectEnabledRenderers(visual);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{label} should expose promoted enabled renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ValidateRendererAssets(renderers[i], $"{label}.{renderers[i].name}");
            }
        }

        private static Renderer[] CollectEnabledRenderers(GameObject root)
        {
            Renderer[] allRenderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            var renderers = new List<Renderer>();
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i].enabled)
                {
                    renderers.Add(allRenderers[i]);
                }
            }

            return renderers.ToArray();
        }

        private static void ValidateBossVisualCueBindings(BossBarrageVisualCueDriver cueDriver, Animator animator)
        {
            var foundPatternIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cueDriver.PatternCueCount; i++)
            {
                if (!cueDriver.TryGetPatternCue(i, out BossBarrageVisualCueDriver.PatternAnimationCue cue))
                {
                    throw new InvalidOperationException($"Boss visual cue driver could not read pattern cue at index {i}.");
                }

                if (string.IsNullOrWhiteSpace(cue.PatternId))
                {
                    throw new InvalidOperationException($"Boss visual cue at index {i} has no pattern id.");
                }

                foundPatternIds.Add(cue.PatternId);
                ValidateAnimatorTrigger(animator, cue.WindupTrigger, $"{cue.PatternId} windup trigger");
                ValidateAnimatorTrigger(animator, cue.ReleaseTrigger, $"{cue.PatternId} release trigger");
            }

            for (int i = 0; i < RequiredBossPatternCueIds.Length; i++)
            {
                if (!foundPatternIds.Contains(RequiredBossPatternCueIds[i]))
                {
                    throw new InvalidOperationException($"Boss visual cue driver is missing pattern cue {RequiredBossPatternCueIds[i]}.");
                }
            }
        }

        private static void ValidateBossPressureActionCueBindings(BossBarrageVisualCueDriver cueDriver, Animator animator)
        {
            var foundActionKinds = new HashSet<BossPressureActionKind>();
            for (int i = 0; i < cueDriver.PressureActionCueCount; i++)
            {
                if (!cueDriver.TryGetPressureActionCue(i, out BossBarrageVisualCueDriver.PressureActionCue cue))
                {
                    throw new InvalidOperationException($"Boss visual cue driver could not read pressure action cue at index {i}.");
                }

                foundActionKinds.Add(cue.ActionKind);
                ValidateAnimatorTrigger(animator, cue.Trigger, $"{cue.ActionKind} pressure action trigger");
            }

            for (int i = 0; i < RequiredBossPressureActionCueKinds.Length; i++)
            {
                if (!foundActionKinds.Contains(RequiredBossPressureActionCueKinds[i]))
                {
                    throw new InvalidOperationException(
                        $"Boss visual cue driver is missing pressure action cue {RequiredBossPressureActionCueKinds[i]}.");
                }
            }
        }

        private static void ValidateAnimatorTrigger(Animator animator, string triggerName, string label)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
            {
                throw new InvalidOperationException($"Boss visual cue {label} is empty.");
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, triggerName, StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Boss visual cue {label} references missing Animator trigger {triggerName}.");
        }

        private static void ValidateRendererAssets(Renderer renderer, string label)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                ValidateGameOwnedAsset(meshFilter.sharedMesh, $"{label} mesh");
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
            {
                ValidateGameOwnedAsset(skinnedMeshRenderer.sharedMesh, $"{label} mesh");
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    ValidateGameOwnedAsset(materials[i], $"{label} material");
                }
            }
        }

        private static void ValidateGameOwnedAsset(UnityEngine.Object asset, string label)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"{label} must be assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/_Game/", StringComparison.Ordinal)
                || assetPath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} should reference a promoted `_Game` asset, found {assetPath}.");
            }
        }

        private static void ValidatePocketOwner(
            BossBarragePocketReviewOwner owner,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossPressureCostLadder bossPressureCost,
            BossPressureActionDirector bossPressureActionDirector)
        {
            ValidateObjectReference(owner, "playerHealth", playerHealth);
            ValidateObjectReference(owner, "closeThreatHealth", closeThreatHealth);
            ValidateObjectReference(owner, "bossHealth", bossHealth);
            ValidateObjectReference(owner, "energyLadder", energyLadder);
            ValidateObjectReference(owner, "skill1Action", skill1Action);
            ValidateObjectReference(owner, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(owner, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(owner, "bossPressureCostLadder", bossPressureCost);
            ValidateObjectReference(owner, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(
                owner,
                "summonPressureBlockOpportunity",
                LoadAsset<SummonOpportunityWindowProfile>(SummonOpportunityProfilePath));
            ValidateBool(owner, "stopBarrageOnClear", true);
            ValidateBool(owner, "stopBarrageOnFail", true);
            ValidateBool(owner, "stopBossPressureCostOnEnd", true);
            ValidateBool(owner, "stopBossPressureActionsOnEnd", true);
            ValidateBool(owner, "stopEnergyGainOnEnd", true);
            ValidateAssignedObjectReference(owner, "clearMarker");
            ValidateAssignedObjectReference(owner, "failMarker");
        }

        private static void ValidatePocketCueBridges(
            BossBarragePocketReviewOwner owner,
            ActionCameraCueDriver cameraCueDriver,
            PlayerCombatVfxCueDriver playerVfxCueDriver,
            CombatVfxCuePlayer cuePlayer,
            Transform directionTarget)
        {
            BossBarragePocketCameraCueBridge cameraBridge =
                RequireComponent<BossBarragePocketCameraCueBridge>(owner.gameObject, "pocket camera cue bridge");
            ValidateObjectReference(cameraBridge, "pocketReviewOwner", owner);
            ValidateObjectReference(cameraBridge, "cameraCueDriver", cameraCueDriver);

            BossBarragePocketVfxCueBridge vfxBridge =
                RequireComponent<BossBarragePocketVfxCueBridge>(owner.gameObject, "pocket VFX cue bridge");
            ValidateObjectReference(vfxBridge, "pocketReviewOwner", owner);
            ValidateObjectReference(vfxBridge, "cuePlayer", cuePlayer);
            ValidateObjectReference(
                vfxBridge,
                "followupWindowAnchor",
                ReadObjectReference<Transform>(playerVfxCueDriver, "attackAnchor"));
            ValidateObjectReference(vfxBridge, "followupHitAnchor", directionTarget);
            ValidateObjectReference(
                vfxBridge,
                "followupMissedAnchor",
                ReadObjectReference<Transform>(playerVfxCueDriver, "dodgeAnchor"));
            ValidateObjectReference(vfxBridge, "directionTarget", directionTarget);
        }

        private static void ValidateReviewHud(
            BossBarrageLaneReviewHud hud,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            Transform player,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBarragePocketReviewOwner pocketOwner,
            BossPressureCostLadder bossPressureCost,
            BossPressurePositionController bossPressurePosition,
            BossPressureActionDirector bossPressureActionDirector,
            BossSummonPressureAction bossSummonPressureAction)
        {
            ValidateObjectReference(hud, "playerHealth", playerHealth);
            ValidateObjectReference(hud, "closeThreatHealth", closeThreatHealth);
            ValidateObjectReference(hud, "bossHealth", bossHealth);
            ValidateObjectReference(hud, "energyLadder", energyLadder);
            ValidateObjectReference(hud, "laneSpace", laneSpace);
            ValidateObjectReference(hud, "player", player);
            ValidateObjectReference(hud, "combatModeController", combatModeController);
            ValidateObjectReference(hud, "rangedAimController", rangedAimController);
            ValidateObjectReference(hud, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(hud, "skill1Action", skill1Action);
            ValidateObjectReference(hud, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(hud, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(hud, "bossPressureCostLadder", bossPressureCost);
            ValidateObjectReference(hud, "bossPressurePositionController", bossPressurePosition);
            ValidateObjectReference(hud, "bossPressureActionDirector", bossPressureActionDirector);
            ValidateObjectReference(hud, "bossSummonPressureAction", bossSummonPressureAction);
            ValidateObjectReference(hud, "pocketReviewOwner", pocketOwner);
            ValidateBool(hud, "showCenterReticle", false);
        }

        private static void ValidateMobileReviewHud(
            BossBarrageLaneReviewMobileHud hud,
            PlayerMovementController movement,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController rangedAimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action)
        {
            ValidateObjectReference(hud, "movement", movement);
            ValidateObjectReference(hud, "actionController", actionController);
            ValidateObjectReference(hud, "combatModeController", combatModeController);
            ValidateObjectReference(hud, "aimController", rangedAimController);
            ValidateObjectReference(hud, "rangedBasicAttackAction", rangedBasicAttackAction);
            ValidateObjectReference(hud, "skill1Action", skill1Action);
            ValidateObjectReference(hud, "summonSlot1Action", summonSlot1Action);
            ValidateString(hud, "moveActionName", "Move");
            ValidateString(hud, "basicDefenseActionName", "BasicDefenseAttack");
            ValidateString(hud, "dodgeActionName", "Dodge");
            ValidateString(hud, "skill1ActionName", "Skill1");
            ValidateString(hud, "summonSlot1ActionName", "SummonSlot1");
            ValidateString(hud, "rangedAimActionName", "RangedAim");
            ValidateString(hud, "weaponSwapActionName", "WeaponSwap");
            ValidateBool(hud, "screenDragControlsAim", true);
            ValidateBool(hud, "rightMouseDragControlsAim", false);
            ValidateBool(hud, "leftMouseDragControlsAim", true);
            ValidateBool(hud, "fireDragControlsAim", true);
        }

        private static void ConfigureArenaInfluenceTargets(Scene scene, Transform player, params Transform[] influenceTargets)
        {
            ActionFoundationArenaShapeInfluenceDriver[] drivers = CollectComponents<ActionFoundationArenaShapeInfluenceDriver>(scene);
            var targets = new UnityEngine.Object[1 + (influenceTargets != null ? influenceTargets.Length : 0)];
            targets[0] = player;
            if (influenceTargets != null)
            {
                for (int i = 0; i < influenceTargets.Length; i++)
                {
                    targets[i + 1] = influenceTargets[i];
                }
            }

            for (int i = 0; i < drivers.Length; i++)
            {
                SetObjectReferenceArray(drivers[i], "influenceTargets", targets);
            }
        }

        private static void DestroyChildIfPresent(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static GameObject FindPlayerMeleeVisualRoot(Transform player)
        {
            Transform swordShieldVisual = FindDescendant(player, "CombatGirlSwordShield_PlayerVisual");
            if (swordShieldVisual != null)
            {
                return swordShieldVisual.gameObject;
            }

            Transform placeholderBody = FindDescendant(player, "CombatGirlPlaceholderBody");
            return placeholderBody != null ? placeholderBody.gameObject : null;
        }

        private static Transform FindLikelyRightHandSocket(Transform root)
        {
            Transform[] candidates = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < candidates.Length; i++)
            {
                string normalized = NormalizeTransformName(candidates[i].name);
                if (normalized.Contains("righthand", StringComparison.Ordinal)
                    || normalized.Contains("rhand", StringComparison.Ordinal)
                    || normalized.Contains("handr", StringComparison.Ordinal)
                    || (normalized.Contains("right", StringComparison.Ordinal)
                        && normalized.Contains("hand", StringComparison.Ordinal)))
                {
                    return candidates[i];
                }
            }

            return null;
        }

        private static Transform FindLikelyLeftHandSocket(Transform root)
        {
            Transform[] candidates = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < candidates.Length; i++)
            {
                string normalized = NormalizeTransformName(candidates[i].name);
                if (normalized.Contains("lefthand", StringComparison.Ordinal)
                    || normalized.Contains("lhand", StringComparison.Ordinal)
                    || normalized.Contains("handl", StringComparison.Ordinal)
                    || (normalized.Contains("left", StringComparison.Ordinal)
                        && normalized.Contains("hand", StringComparison.Ordinal)))
                {
                    return candidates[i];
                }
            }

            return null;
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < children.Length; i++)
            {
                if (string.Equals(children[i].name, childName, StringComparison.Ordinal))
                {
                    return children[i];
                }
            }

            return null;
        }

        private static Transform RequireDescendant(Transform root, string childName)
        {
            Transform descendant = FindDescendant(root, childName);
            if (descendant == null)
            {
                throw new InvalidOperationException($"{root.name} must contain descendant {childName}.");
            }

            return descendant;
        }

        private static string NormalizeTransformName(string value)
        {
            return value
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(":", string.Empty)
                .ToLowerInvariant();
        }

        private static void AssignRangedCandidateMaterials(GameObject visualRoot)
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    string hint = $"{renderer.name} {materials[materialIndex]?.name ?? string.Empty}";
                    materials[materialIndex] = ResolveRangedCandidateMaterial(hint, materialIndex);
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void RemapRangedCandidateMeshes(GameObject visualRoot)
        {
            Dictionary<string, Mesh> promotedMeshes = LoadPromotedMeshMap(
                ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateModelPath,
                ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateWeaponModelPath);

            MeshFilter[] meshFilters = visualRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Mesh promotedMesh = ResolvePromotedMesh(meshFilters[i].sharedMesh, promotedMeshes, $"{meshFilters[i].name} mesh");
                if (meshFilters[i].sharedMesh != promotedMesh)
                {
                    meshFilters[i].sharedMesh = promotedMesh;
                    EditorUtility.SetDirty(meshFilters[i]);
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                Mesh promotedMesh = ResolvePromotedMesh(
                    skinnedRenderers[i].sharedMesh,
                    promotedMeshes,
                    $"{skinnedRenderers[i].name} skinned mesh");
                if (skinnedRenderers[i].sharedMesh != promotedMesh)
                {
                    skinnedRenderers[i].sharedMesh = promotedMesh;
                    EditorUtility.SetDirty(skinnedRenderers[i]);
                }
            }
        }

        private static Dictionary<string, Mesh> LoadPromotedMeshMap(params string[] assetPaths)
        {
            var meshes = new Dictionary<string, Mesh>(StringComparer.Ordinal);
            for (int pathIndex = 0; pathIndex < assetPaths.Length; pathIndex++)
            {
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPaths[pathIndex]);
                for (int assetIndex = 0; assetIndex < assets.Length; assetIndex++)
                {
                    if (assets[assetIndex] is Mesh mesh && !meshes.ContainsKey(mesh.name))
                    {
                        meshes.Add(mesh.name, mesh);
                    }
                }
            }

            if (meshes.Count == 0)
            {
                throw new InvalidOperationException("Promoted RifleGirl mesh assets are missing.");
            }

            return meshes;
        }

        private static Mesh ResolvePromotedMesh(
            Mesh sourceMesh,
            IReadOnlyDictionary<string, Mesh> promotedMeshes,
            string label)
        {
            if (sourceMesh == null)
            {
                return null;
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceMesh).Replace('\\', '/');
            if (sourcePath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (!sourcePath.Contains("/_Imported/", StringComparison.Ordinal))
            {
                return sourceMesh;
            }

            if (promotedMeshes.TryGetValue(sourceMesh.name, out Mesh promotedMesh))
            {
                return promotedMesh;
            }

            throw new InvalidOperationException($"Missing promoted RifleGirl mesh for {label}: {sourceMesh.name}.");
        }

        private static Avatar LoadPromotedRifleGirlAvatar()
        {
            string assetPath = ActionFoundationPlayerCombatModeAssetSetup.RangedCandidateModelPath;
            GameObject promotedModel = LoadAsset<GameObject>(assetPath);
            Animator promotedAnimator = promotedModel.GetComponent<Animator>();
            if (promotedAnimator != null && promotedAnimator.avatar != null)
            {
                return promotedAnimator.avatar;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar)
                {
                    return avatar;
                }
            }

            throw new InvalidOperationException("Promoted RifleGirl model must expose a game-owned Avatar.");
        }

        private static void StripNonGameMonoBehaviours(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transforms[i].gameObject);
            }

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
                string scriptPath = script != null
                    ? AssetDatabase.GetAssetPath(script).Replace('\\', '/')
                    : string.Empty;
                if (!scriptPath.StartsWith("Assets/_Game/", StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static Material ResolveRangedCandidateMaterial(string hint, int slotIndex)
        {
            string lower = hint.ToLowerInvariant();
            if (lower.Contains("eye"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Eye.mat");
            }

            if (lower.Contains("face"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Face.mat");
            }

            if (lower.Contains("hair"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Hair01.mat");
            }

            if (lower.Contains("cloth"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Cloth01.mat");
            }

            if (lower.Contains("sport"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Sportswear.mat");
            }

            if (lower.Contains("weapon") || lower.Contains("rifle"))
            {
                return LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_RangedFocus.mat");
            }

            return slotIndex switch
            {
                1 => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Face.mat"),
                2 => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Eye.mat"),
                3 => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Hair01.mat"),
                _ => LoadAsset<Material>("Assets/_Game/Art/Characters/Player/RifleGirl/Materials/DB_RifleGirl_Body.mat")
            };
        }

        private static void AssignMaterialToAllRenderers(GameObject visualRoot, Material material)
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Material[] materials = renderers[rendererIndex].sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = material;
                }

                renderers[rendererIndex].sharedMaterials = materials;
                EditorUtility.SetDirty(renderers[rendererIndex]);
            }
        }

        private static void ValidatePlayerCombatModeVisual(
            GameObject rangedRoot,
            Animator rangedAnimator,
            GameObject rangedWeaponRoot,
            GameObject meleeWeaponRoot)
        {
            if (!rangedRoot.activeSelf)
            {
                throw new InvalidOperationException("Ranged player visual root should be active for the review scene starting mode.");
            }

            if (rangedAnimator.runtimeAnimatorController
                != LoadAsset<RuntimeAnimatorController>(RifleGirlRangedControllerPath))
            {
                throw new InvalidOperationException("Ranged player visual must use the promoted RifleGirl controller for review.");
            }

            ValidateGameOwnedAsset(rangedAnimator.avatar, "Ranged player visual Avatar");

            Renderer[] renderers = rangedRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Ranged player visual must contain renderers.");
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ValidateRendererUsesGameOwnedAssets(renderers[i], renderers[i].name);
                Material[] materials = renderers[i].sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        ValidateRenderableMaterialShader(materials[materialIndex], $"{renderers[i].name} material shader");
                    }
                }
            }

            Transform weapon = FindDescendant(rangedRoot.transform, RangedPlayerWeaponName);
            if (weapon == null)
            {
                throw new InvalidOperationException($"Ranged player visual is missing {RangedPlayerWeaponName}.");
            }

            if (weapon.parent == rangedRoot.transform)
            {
                throw new InvalidOperationException("Ranged weapon must stay inside the RifleGirl authored weapon hierarchy.");
            }

            if (weapon.GetComponentsInChildren<Renderer>(includeInactive: true).Length == 0)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} must include visible renderers.");
            }

            ParentConstraint weaponConstraint = weapon.GetComponent<ParentConstraint>();
            if (weaponConstraint == null)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} must preserve the source rifle ParentConstraint.");
            }

            if (weaponConstraint.sourceCount < 2)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} ParentConstraint must keep authored weapon sockets.");
            }

            if (!weaponConstraint.constraintActive)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} ParentConstraint must start active.");
            }

            RifleGirlWeaponSocketDriver weaponSocketDriver =
                rangedAnimator.GetComponent<RifleGirlWeaponSocketDriver>();
            if (weaponSocketDriver == null || !weaponSocketDriver.IsConfigured)
            {
                throw new InvalidOperationException("Ranged player visual must bind the RifleGirl weapon socket and left-hand IK driver.");
            }

            ValidateObjectReference(weaponSocketDriver, "animator", rangedAnimator);
            ValidateObjectReference(weaponSocketDriver, "rifleConstraint", weaponConstraint);
            Transform leftHandle = FindDescendant(weapon.transform, "Left_Handle");
            if (leftHandle == null)
            {
                throw new InvalidOperationException($"{RangedPlayerWeaponName} must expose Left_Handle for support-hand IK.");
            }

            ValidateObjectReference(weaponSocketDriver, "leftHandIkTarget", leftHandle);
            ValidateString(weaponSocketDriver, "defaultCommands", "To_Hand_R_Socket, IK_ON_Left_Handle");
            ValidateString(weaponSocketDriver, "handSocketCommand", "To_Hand_R_Socket");
            ValidateString(weaponSocketDriver, "holsterSocketCommand", "To_Put_Socket_Rifle");
            ValidateString(weaponSocketDriver, "aimSocketCommand", "To_add_weapon_r");
            ValidateString(weaponSocketDriver, "leftIkOnCommand", "IK_ON_Left_Handle");
            ValidateString(weaponSocketDriver, "leftIkOffCommand", "IK_OFF_Left_Handle");

            if (FindDescendant(rangedRoot.transform, "Hand_R_Socket") == null)
            {
                throw new InvalidOperationException("Ranged player visual must expose a right-hand socket for weapon-only swap.");
            }

            if (FindDescendant(rangedRoot.transform, "Put_Socket_Rifle") == null
                || FindDescendant(rangedRoot.transform, "R_Weapon_Bone_Dymmy_R") == null)
            {
                throw new InvalidOperationException("Ranged player visual must preserve RifleGirl authored rifle sockets.");
            }

            if (rangedWeaponRoot != weapon.gameObject)
            {
                throw new InvalidOperationException("Combat mode controller must reference the actual ranged weapon root.");
            }

            if (!rangedWeaponRoot.activeSelf)
            {
                throw new InvalidOperationException("Ranged weapon should start active with the ranged channel.");
            }

            if (meleeWeaponRoot.name != MeleePlayerWeaponRootName)
            {
                throw new InvalidOperationException("Melee weapon root should keep the expected review-scene name.");
            }

            if (meleeWeaponRoot.activeSelf)
            {
                throw new InvalidOperationException("Melee weapon root should start inactive because the review scene starts in ranged mode.");
            }

            CombatGirlWeaponSocketBinder weaponBinder = meleeWeaponRoot.GetComponent<CombatGirlWeaponSocketBinder>();
            if (weaponBinder == null || !weaponBinder.AllBindingsValid)
            {
                throw new InvalidOperationException("Melee weapon root must keep valid RifleGirl hand socket bindings.");
            }

            if (FindDescendant(meleeWeaponRoot.transform, "MeleeWeapon_RightHand") == null
                || FindDescendant(meleeWeaponRoot.transform, "MeleeWeapon_LeftHand") == null)
            {
                throw new InvalidOperationException("Melee weapon root must include both cloned sword/shield weapon objects.");
            }

            if (meleeWeaponRoot.GetComponentsInChildren<Renderer>(includeInactive: true).Length == 0)
            {
                throw new InvalidOperationException("Melee weapon root must include visible sword/shield renderers.");
            }
        }

        private static void ValidateRendererUsesGameOwnedAssets(Renderer renderer, string label)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                ValidateGameOwnedAsset(meshFilter.sharedMesh, $"{label} mesh");
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
            {
                ValidateGameOwnedAsset(skinnedMeshRenderer.sharedMesh, $"{label} skinned mesh");
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    ValidateGameOwnedAsset(materials[i], $"{label} material");
                    ValidateRenderableMaterialShader(materials[i], $"{label} material shader");
                }
            }
        }

        private static void ValidateRenderableMaterialShader(Material material, string label)
        {
            if (material.shader == null ||
                string.Equals(material.shader.name, "Hidden/InternalErrorShader", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} must not use Unity's missing/error shader.");
            }
        }

        private static Material LoadOrCreateMaterial(string assetPath, Color color)
        {
            EnsureFolderForAsset(assetPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(ResolveUnlitShader());
                AssetDatabase.CreateAsset(material, assetPath);
            }

            if (material.shader == null)
            {
                material.shader = ResolveUnlitShader();
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", color * 1.35f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTransparentMaterial(string assetPath, Color color)
        {
            Material material = LoadOrCreateMaterial(assetPath, color);
            SetMaterialFloatIfPresent(material, "_Surface", 1f);
            SetMaterialFloatIfPresent(material, "_Blend", 0f);
            SetMaterialFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetMaterialFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetMaterialFloatIfPresent(material, "_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static Mesh LoadPrimitiveMesh(PrimitiveType primitiveType)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            Mesh mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(primitive);
            return mesh;
        }

        private static Shader ResolveUnlitShader()
        {
            return Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
        }

        private static void RemoveReviewAndEnemyRoots(Scene scene)
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
            return rootName.StartsWith(ReviewRootPrefix, StringComparison.Ordinal)
                || rootName.StartsWith("Enemy_SciFiSoldier_", StringComparison.Ordinal)
                || rootName.StartsWith("EnemyPrefabReview_", StringComparison.Ordinal)
                || rootName.StartsWith("EnemyRoleReview_", StringComparison.Ordinal)
                || rootName.StartsWith("ReadableAttackTelegraph", StringComparison.Ordinal);
        }

        private static GameObject CreateRoot(Scene scene, string rootName)
        {
            GameObject root = new GameObject(rootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
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

        private static GameObject RequireRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            throw new InvalidOperationException($"Missing root {rootName} in {scene.path}.");
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

        private static T EnsureComponent<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            return component != null ? component : root.AddComponent<T>();
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            var childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, worldPositionStays: false);
            return childObject.transform;
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

        private static T LoadPrefabComponent<T>(string assetPath) where T : Component
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is T component)
                {
                    return component;
                }
            }

            GameObject prefab = LoadAsset<GameObject>(assetPath);
            T loadedComponent = prefab.GetComponent<T>();
            if (loadedComponent == null)
            {
                throw new InvalidOperationException($"{assetPath} is missing required component {typeof(T).Name}.");
            }

            return loadedComponent;
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            string folder = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder))
            {
                throw new InvalidOperationException($"Could not resolve folder for {assetPath}.");
            }

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void ValidateNoImportedAssetReference(string assetPath)
        {
            if (assetPath.Replace('\\', '/').Contains("/_Imported/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{assetPath} must not point at raw _Imported assets.");
            }
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

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnum(UnityEngine.Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).enumValueIndex = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetVector3(UnityEngine.Object target, string propertyName, Vector3 value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).vector3Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetColor(UnityEngine.Object target, string propertyName, Color value)
        {
            var serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).colorValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static T ReadObjectReference<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            UnityEngine.Object value = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            return value as T;
        }

        private static T RequireReferencedObject<T>(UnityEngine.Object target, string propertyName) where T : UnityEngine.Object
        {
            T value = ReadObjectReference<T>(target, propertyName);
            if (value == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} must be assigned.");
            }

            return value;
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

        private static void ValidateAssignedObjectReference(UnityEngine.Object target, string propertyName)
        {
            UnityEngine.Object actual = RequireProperty(new SerializedObject(target), propertyName).objectReferenceValue;
            if (actual == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} must be assigned.");
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

        private static void ValidateArrayContainsReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object expected,
            string label)
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (!array.isArray)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should be an array.");
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == expected)
                {
                    return;
                }
            }

            string expectedName = expected != null ? expected.name : "null";
            throw new InvalidOperationException(
                $"{target.name}.{propertyName} should contain {label} ({expectedName}).");
        }

        private static UnityEngine.Object ValidateArrayAssignedReference(
            UnityEngine.Object target,
            string propertyName,
            int index)
        {
            return ValidateArrayAssignedReference<UnityEngine.Object>(target, propertyName, index);
        }

        private static T ValidateArrayAssignedReference<T>(
            UnityEngine.Object target,
            string propertyName,
            int index) where T : UnityEngine.Object
        {
            SerializedProperty array = RequireProperty(new SerializedObject(target), propertyName);
            if (!array.isArray || array.arraySize <= index)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} should contain index {index}.");
            }

            var actual = array.GetArrayElementAtIndex(index).objectReferenceValue as T;
            if (actual == null)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName}[{index}] must be assigned.");
            }

            return actual;
        }

        private static void ValidateBool(UnityEngine.Object target, string propertyName, bool expected)
        {
            bool actual = RequireProperty(new SerializedObject(target), propertyName).boolValue;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateString(UnityEngine.Object target, string propertyName, string expected)
        {
            string actual = RequireProperty(new SerializedObject(target), propertyName).stringValue;
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateString(string actual, string expected, string errorMessage)
        {
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static void ValidateFloat(UnityEngine.Object target, string propertyName, float expected)
        {
            float actual = RequireProperty(new SerializedObject(target), propertyName).floatValue;
            if (!Mathf.Approximately(actual, expected))
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateInt(UnityEngine.Object target, string propertyName, int expected)
        {
            int actual = RequireProperty(new SerializedObject(target), propertyName).intValue;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateEnum(UnityEngine.Object target, string propertyName, int expected)
        {
            int actual = RequireProperty(new SerializedObject(target), propertyName).enumValueIndex;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected enum index {expected}, found {actual}.");
            }
        }

        private static void ValidateVector3(UnityEngine.Object target, string propertyName, Vector3 expected)
        {
            Vector3 actual = RequireProperty(new SerializedObject(target), propertyName).vector3Value;
            if ((actual - expected).sqrMagnitude > 0.000001f)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
            }
        }

        private static void ValidateColor(UnityEngine.Object target, string propertyName, Color expected)
        {
            Color actual = RequireProperty(new SerializedObject(target), propertyName).colorValue;
            float maxDelta = Mathf.Max(
                Mathf.Abs(actual.r - expected.r),
                Mathf.Abs(actual.g - expected.g),
                Mathf.Abs(actual.b - expected.b),
                Mathf.Abs(actual.a - expected.a));
            if (maxDelta > 0.000001f)
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

        private static SerializedProperty RequireRelativeProperty(SerializedProperty property, string relativeName)
        {
            SerializedProperty relative = property.FindPropertyRelative(relativeName);
            if (relative == null)
            {
                throw new InvalidOperationException($"{property.propertyPath} is missing serialized property {relativeName}.");
            }

            return relative;
        }

        private readonly struct PlayerCombatModeVisualBinding
        {
            public PlayerCombatModeVisualBinding(
                GameObject rangedRoot,
                GameObject meleeRoot,
                GameObject rangedWeaponRoot,
                Transform rangedFireOrigin,
                GameObject meleeWeaponRoot,
                RifleGirlNativeGameplayAnimatorBridge nativeAnimatorBridge,
                Animator rangedAnimator,
                Animator meleeAnimator)
            {
                RangedRoot = rangedRoot;
                MeleeRoot = meleeRoot;
                RangedWeaponRoot = rangedWeaponRoot;
                RangedFireOrigin = rangedFireOrigin;
                MeleeWeaponRoot = meleeWeaponRoot;
                NativeAnimatorBridge = nativeAnimatorBridge;
                RangedAnimator = rangedAnimator;
                MeleeAnimator = meleeAnimator;
            }

            public GameObject RangedRoot { get; }
            public GameObject MeleeRoot { get; }
            public GameObject RangedWeaponRoot { get; }
            public Transform RangedFireOrigin { get; }
            public GameObject MeleeWeaponRoot { get; }
            public RifleGirlNativeGameplayAnimatorBridge NativeAnimatorBridge { get; }
            public Animator RangedAnimator { get; }
            public Animator MeleeAnimator { get; }
        }
    }
}
