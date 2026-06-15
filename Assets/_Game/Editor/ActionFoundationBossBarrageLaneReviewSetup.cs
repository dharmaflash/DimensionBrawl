using System;
using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
        public const string ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageProjectile.mat";
        public const string Skill1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_PlayerSkill1Projectile_LaneBolt.prefab";
        public const string SummonSlot1ProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Projectile_AssistBolt.prefab";
        public const string SummonSlot1EntryCuePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1EntryCue_MagicCircle.prefab";
        public const string SummonSlot1ActorPrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_SummonSlot1Actor_Proxy.prefab";
        private const string Skill1ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_PlayerSkill1Projectile.mat";
        private const string SummonSlot1ProjectileMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1Projectile.mat";
        private const string SummonSlot1EntryCueMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1EntryCue.mat";
        private const string SummonSlot1ActorMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonSlot1Actor.mat";
        private const string SummonPressureScreenMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_SummonPressureScreen.mat";

        private const string ReviewRootPrefix = "BossBarrageLaneReview_";
        private const string LaneRootName = ReviewRootPrefix + "SummonLaneSpace";
        private const string BossProxyRootName = ReviewRootPrefix + "BossProxy_NeedleLock";
        private const string CloseThreatRootName = ReviewRootPrefix + "CloseThreat_ClosePunish";
        private const string ProjectilePoolRootName = ReviewRootPrefix + "ProjectilePool";
        private const string ActionCuePoolRootName = ReviewRootPrefix + "ActionCuePool";
        private const string SummonActorPoolRootName = ReviewRootPrefix + "SummonActorPool";
        private const string PocketOwnerRootName = ReviewRootPrefix + "PocketOwner";
        private const string HudRootName = ReviewRootPrefix + "DebugHud";
        private const string MarkerRootName = ReviewRootPrefix + "Markers";
        private const string PocketClearMarkerName = ReviewRootPrefix + "PocketClearMarker";
        private const string PocketFailMarkerName = ReviewRootPrefix + "PocketFailMarker";
        private const string SummonEntryMarkerName = ReviewRootPrefix + "SummonEntryMarker";
        private const string BossProxyMarkerName = ReviewRootPrefix + "BossProxyMarker";
        private const string BossProxyVisualMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossProxy.mat";
        private const string LaneRailMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageLaneRail.mat";
        private const string PlayerBoundaryMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarragePlayerBoundary.mat";
        private const string SummonBoundaryMaterialPath =
            "Assets/_Game/Art/Materials/ActionFoundation/AF_BossBarrageSummonBoundary.mat";

        private static readonly Vector3 PlayerStartPosition = new Vector3(0f, 0f, -8.5f);
        private static readonly Vector3 CameraStartOffset = new Vector3(0f, 2.6f, -8.2f);
        private static readonly Vector3 CameraLookOffset = new Vector3(0f, 1.4f, 5.5f);

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
                0.42f);
            LaneActionProjectile summonSlot1ProjectilePrefab = EnsureLaneActionProjectilePrefab(
                SummonSlot1ProjectilePrefabPath,
                "PF_SummonSlot1Projectile_AssistBolt",
                SummonSlot1ProjectileMaterialPath,
                new Color(0.55f, 1f, 0.72f, 1f),
                0.58f);
            GameObject summonEntryCuePrefab = EnsureSummonEntryCuePrefab();
            SummonFrontlineProxy summonActorPrefab = EnsureSummonActorPrefab();
            Scene scene = EditorSceneManager.OpenScene(ActionFoundationProfileSetup.ScenePath, OpenSceneMode.Single);
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
                projectileRoot.transform);
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
            BossBarragePocketReviewOwner pocketOwner = CreatePocketOwner(
                scene,
                playerHealth,
                closeThreatHealth,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                laneSpace);
            CreateReviewHud(
                scene,
                playerHealth,
                closeThreatHealth,
                energyLadder,
                laneSpace,
                player.transform,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                pocketOwner);
            ConfigureFixedRearCamera(cameraController, player.transform, bossProxy.transform);
            ConfigureArenaInfluenceTargets(scene, player.transform, bossProxy.transform, closeThreat.transform);
            CreateLaneMarkers(scene, laneSpace);

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
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            GameObject closeThreat = RequireRoot(scene, CloseThreatRootName);
            CombatHealth closeThreatHealth = RequireComponent<CombatHealth>(closeThreat, "close threat health");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            BossBarragePocketReviewOwner pocketOwner =
                RequireComponent<BossBarragePocketReviewOwner>(RequireRoot(scene, PocketOwnerRootName), "boss barrage pocket owner");
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(scene, HudRootName), "boss barrage review HUD");

            ValidateObjectReference(player, "laneSpace", laneSpace);
            ValidateObjectReference(playerActionController, "actionProfile", LoadAsset<PlayerActionProfile>(LocalDefenseProfilePath));
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
            ValidateObjectReference(targetSelector, "selfHealth", playerHealth);
            ValidateArrayReference(targetSelector, "targetCandidates", 0, closeThreatHealth);
            ValidateArrayReference(targetSelector, "targetCandidates", 1, bossHealth);
            ValidateFloat(targetSelector, "selectionRadius", 35f);
            ValidateFloat(targetSelector, "attackAimRadius", 9f);
            ValidateCloseThreat(closeThreat, closeThreatHealth, playerHealth, cameraController);
            ValidateObjectReference(cameraController, "target", player.transform);
            ValidateObjectReference(cameraController, "threat", bossProxy.transform);
            ValidateObjectReference(encounter, "playerHealth", playerHealth);
            ValidateObjectReference(encounter, "enemyHealth", closeThreatHealth);
            ValidatePocketOwner(pocketOwner, playerHealth, closeThreatHealth, skill1Action, summonSlot1Action, emitter);
            ValidateReviewHud(
                reviewHud,
                playerHealth,
                closeThreatHealth,
                energyLadder,
                laneSpace,
                player.transform,
                skill1Action,
                summonSlot1Action,
                emitter,
                pocketOwner);
            ValidateFixedRearCamera(cameraController, player.transform);
            ValidateSummonForwardSpace(laneSpace);
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
            ValidateNoImportedAssetReference(SummonSlot1ProjectilePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1EntryCuePrefabPath);
            ValidateNoImportedAssetReference(SummonSlot1ActorPrefabPath);
            ValidateNoImportedAssetReference(SummonPressureScreenMaterialPath);
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
            RequireRelativeProperty(step, "forwardAdvanceDistance").floatValue = 0.12f;
            RequireRelativeProperty(step, "forwardAdvanceDurationSeconds").floatValue = 0.08f;
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
            float scale)
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

                EnsureComponent<LaneActionProjectile>(editableRoot);
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
            Transform projectileRoot)
        {
            GameObject bossProxy = CreateRoot(scene, BossProxyRootName);
            bossProxy.transform.SetPositionAndRotation(
                laneSpace.GetLaneWorldPoint(0f, laneSpace.BossProxyZ, 1.6f),
                Quaternion.LookRotation(Vector3.back, Vector3.up));

            CombatHealth bossHealth = EnsureComponent<CombatHealth>(bossProxy);
            bossHealth.ConfigureTeam(DamageTeam.Enemy);
            SetFloat(bossHealth, "maxHealth", 5000f);

            BossBarrageEmitter emitter = EnsureComponent<BossBarrageEmitter>(bossProxy);
            SetObjectReference(emitter, "laneSpace", laneSpace);
            SetObjectReference(emitter, "trackedPlayer", RequireObject<PlayerMovementController>(scene, "player movement").transform);
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

            CreateBossProxyVisual(bossProxy.transform);
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
            Material material = LoadOrCreateMaterial(BossProxyVisualMaterialPath, new Color(1f, 0.55f, 0.05f, 1f));
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = BossProxyMarkerName;
            visual.transform.SetParent(parent, worldPositionStays: false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(1.35f, 1.35f, 1.35f);
            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
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

        private static void CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, worldPositionStays: true);
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = scale;
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
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

        private static BossBarragePocketReviewOwner CreatePocketOwner(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
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
            owner.Configure(playerHealth, closeThreatHealth, skill1Action, summonSlot1Action, bossBarrageEmitter, clearMarker, failMarker);
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

        private static void CreateReviewHud(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            Transform player,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBarragePocketReviewOwner pocketOwner)
        {
            GameObject hudRoot = CreateRoot(scene, HudRootName);
            BossBarrageLaneReviewHud hud = hudRoot.AddComponent<BossBarrageLaneReviewHud>();
            hud.Configure(
                playerHealth,
                closeThreatHealth,
                energyLadder,
                laneSpace,
                player,
                skill1Action,
                summonSlot1Action,
                bossBarrageEmitter,
                pocketOwner);
            EditorUtility.SetDirty(hud);
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
        }

        private static void ConfigureFixedRearCamera(
            ActionCameraController cameraController,
            Transform player,
            Transform bossProxy)
        {
            Vector3 position = player.position + CameraStartOffset;
            Vector3 lookTarget = player.position + CameraLookOffset;
            Vector3 lookDirection = lookTarget - position;
            cameraController.transform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(lookDirection.normalized, Vector3.up));

            Camera camera = cameraController.GetComponent<Camera>();
            if (camera != null)
            {
                camera.fieldOfView = 48f;
                EditorUtility.SetDirty(camera);
            }

            SetObjectReference(cameraController, "target", player);
            SetObjectReference(cameraController, "threat", bossProxy);
            SetVector3(cameraController, "cameraOffset", CameraStartOffset);
            SetVector3(cameraController, "lookOffset", CameraLookOffset);
            SetBool(cameraController, "useDeviceFallbackWhenActionMissing", false);
            SetFloat(cameraController, "manualYawSpeedDegrees", 0f);
            SetFloat(cameraController, "mouseYawDegreesPerPixel", 0f);
            SetFloat(cameraController, "targetYawAssist", 0f);
            SetFloat(cameraController, "threatBias", 0.12f);
            SetFloat(cameraController, "maxThreatFocusOffset", 2f);
        }

        private static void ValidateFixedRearCamera(ActionCameraController cameraController, Transform player)
        {
            Vector3 planarOffset = Vector3.ProjectOnPlane(cameraController.transform.position - player.position, Vector3.up);
            if (Vector3.Dot(player.forward, planarOffset) >= -0.1f)
            {
                throw new InvalidOperationException("Boss barrage lane camera should start behind the player.");
            }

            ValidateBool(cameraController, "useDeviceFallbackWhenActionMissing", false);
            ValidateFloat(cameraController, "manualYawSpeedDegrees", 0f);
            ValidateFloat(cameraController, "mouseYawDegreesPerPixel", 0f);
            ValidateFloat(cameraController, "targetYawAssist", 0f);
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

            SummonFrontlineProxy summonActorPrefab = LoadPrefabComponent<SummonFrontlineProxy>(SummonSlot1ActorPrefabPath);
            SummonPressureScreen pressureScreen = LoadPrefabComponent<SummonPressureScreen>(SummonSlot1ActorPrefabPath);
            SummonPressureScreenPresenter presenter =
                LoadPrefabComponent<SummonPressureScreenPresenter>(SummonSlot1ActorPrefabPath);
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

            ValidateObjectReference(summonActorPrefab, "pressureScreen", pressureScreen);
            ValidateEnum(pressureScreen, "ownerTeam", (int)DamageTeam.AllySummon);
            ValidateInt(pressureScreen, "defaultMaxIntercepts", 2);
            ValidateFloat(pressureScreen, "defaultLifetimeSeconds", 1.2f);
            ValidateFloat(pressureScreen, "defaultRadius", 1.35f);
            ValidateObjectReference(presenter, "pressureScreen", pressureScreen);
            ValidateObjectReference(presenter, "visualRoot", pressureScreenVisual);
            ValidateArrayReference(presenter, "screenRenderers", 0, pressureScreenRenderer);
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

        private static void ValidatePocketOwner(
            BossBarragePocketReviewOwner owner,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter)
        {
            ValidateObjectReference(owner, "playerHealth", playerHealth);
            ValidateObjectReference(owner, "closeThreatHealth", closeThreatHealth);
            ValidateObjectReference(owner, "skill1Action", skill1Action);
            ValidateObjectReference(owner, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(owner, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateAssignedObjectReference(owner, "clearMarker");
            ValidateAssignedObjectReference(owner, "failMarker");
        }

        private static void ValidateReviewHud(
            BossBarrageLaneReviewHud hud,
            CombatHealth playerHealth,
            CombatHealth closeThreatHealth,
            SummonEnergyLadder energyLadder,
            SummonLaneSpace laneSpace,
            Transform player,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            BossBarrageEmitter bossBarrageEmitter,
            BossBarragePocketReviewOwner pocketOwner)
        {
            ValidateObjectReference(hud, "playerHealth", playerHealth);
            ValidateObjectReference(hud, "closeThreatHealth", closeThreatHealth);
            ValidateObjectReference(hud, "energyLadder", energyLadder);
            ValidateObjectReference(hud, "laneSpace", laneSpace);
            ValidateObjectReference(hud, "player", player);
            ValidateObjectReference(hud, "skill1Action", skill1Action);
            ValidateObjectReference(hud, "summonSlot1Action", summonSlot1Action);
            ValidateObjectReference(hud, "bossBarrageEmitter", bossBarrageEmitter);
            ValidateObjectReference(hud, "pocketReviewOwner", pocketOwner);
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

        private static void ValidateBool(UnityEngine.Object target, string propertyName, bool expected)
        {
            bool actual = RequireProperty(new SerializedObject(target), propertyName).boolValue;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{target.name}.{propertyName} expected {expected}, found {actual}.");
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
    }
}
