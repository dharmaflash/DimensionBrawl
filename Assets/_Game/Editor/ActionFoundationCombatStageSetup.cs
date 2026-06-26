using System;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        public const string CombatStageScenePath = "Assets/_Game/Scenes/ActionFoundationCombatStage.unity";

        private const string CombatStageRangedInoriVisualName = "BossBarrageLaneReview_RangedVisual_Inori";
        private const string CombatStageRetiredRifleGirlVisualName = "BossBarrageLaneReview_RangedVisual_RifleGirl";
        private const float CombatStageAimAssistAngleDegrees = 28f;
        private const float CombatStageAimAssistMaxTurnDegrees = 60f;

        [MenuItem("DimensionBrawl/Action Foundation/Reapply Combat Stage Scene")]
        public static void ReapplyCombatStageSceneMenu()
        {
            EnsureCombatStageScene();
            Debug.Log($"Reapplied combat stage scene at {CombatStageScenePath}.");
        }

        public static void RunBatchEnsureCombatStageScene()
        {
            EnsureCombatStageScene();
        }

        public static void EnsureCombatStageScene()
        {
            bool hasExistingCombatStage =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(CombatStageScenePath) != null;
            string scenePath = hasExistingCombatStage ? CombatStageScenePath : ReviewScenePath;
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            PlayerMovementController player = RequireObject<PlayerMovementController>(scene, "player movement");
            CombatHealth playerHealth = RequireComponent<CombatHealth>(player.gameObject, "player health");
            PlayerCombatTargetSelector targetSelector = RequireObject<PlayerCombatTargetSelector>(scene, "player target selector");
            ActionFoundationTestEncounter encounter = RequireObject<ActionFoundationTestEncounter>(scene, "test encounter");
            SummonEnergyLadder energyLadder = RequireComponent<SummonEnergyLadder>(player.gameObject, "summon energy ladder");
            PlayerSkill1Action skill1Action = RequireComponent<PlayerSkill1Action>(player.gameObject, "player Skill1 action");
            PlayerSummonSlot1Action summonSlot1Action =
                RequireComponent<PlayerSummonSlot1Action>(player.gameObject, "player SummonSlot1 action");
            PlayerSupportSummonSlotAction summonSlot2Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot2");
            PlayerSupportSummonSlotAction summonSlot3Action =
                RequireSupportSummonSlotAction(player.gameObject, "SummonSlot3");
            SummonLaneSpace laneSpace = RequireObject<SummonLaneSpace>(scene, "summon lane space");
            GameObject bossProxy = RequireRoot(scene, BossProxyRootName);
            CombatHealth bossHealth = RequireComponent<CombatHealth>(bossProxy, "boss proxy health");
            ValidateBossProxyBodyContract(bossProxy, bossHealth);
            BossBarrageEmitter emitter = RequireComponent<BossBarrageEmitter>(bossProxy, "boss barrage emitter");
            BossBasicFireEmitter bossBasicFireEmitter =
                RequireComponent<BossBasicFireEmitter>(bossProxy, "boss basic fire emitter");
            BossPressureCostLadder bossPressureCost =
                RequireComponent<BossPressureCostLadder>(bossProxy, "boss pressure cost ladder");
            BossPressureActionDirector bossPressureActionDirector =
                RequireComponent<BossPressureActionDirector>(bossProxy, "boss pressure action director");
            BossSummonPressureAction bossSummonPressureAction =
                RequireComponent<BossSummonPressureAction>(bossProxy, "boss summon pressure action");
            GameObject closeThreat = RequireRoot(scene, CloseThreatRootName);
            GameObject pocketOwner = RequireRoot(scene, PocketOwnerRootName);
            BossBarrageLaneReviewHud reviewHud =
                RequireComponent<BossBarrageLaneReviewHud>(RequireRoot(scene, HudRootName), "boss barrage review HUD");
            BossBarrageLaneReviewMobileHud mobileHud =
                RequireComponent<BossBarrageLaneReviewMobileHud>(reviewHud.gameObject, "boss barrage mobile review HUD");
            ActionScreenCuePresenter screenCuePresenter =
                RequireComponent<ActionScreenCuePresenter>(reviewHud.gameObject, "action screen cue presenter");
            PlayerCombatModeController combatModeController =
                RequireComponent<PlayerCombatModeController>(player.gameObject, "player combat mode controller");
            PlayerRangedBasicAttackAction rangedBasicAttackAction =
                RequireComponent<PlayerRangedBasicAttackAction>(player.gameObject, "player ranged basic attack action");

            RequireCombatStageInoriPlayerVisual(player.gameObject, combatModeController);
            RebindBossBarrageLaneReviewSingleCharacterMode(scene);
            if (!hasExistingCombatStage)
            {
                SeedCombatStageAimDefaults(rangedBasicAttackAction, mobileHud);
            }

            closeThreat.SetActive(false);
            pocketOwner.SetActive(false);
            SetObjectReferenceArray(targetSelector, "targetCandidates", new UnityEngine.Object[] { bossHealth });
            SetObjectReference(encounter, "enemyHealth", bossHealth);

            GameObject existingDuelOwnerRoot = FindRoot(scene, DuelOwnerRootName);
            if (existingDuelOwnerRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(existingDuelOwnerRoot);
            }

            GameObject duelOwnerRoot = CreateRoot(scene, DuelOwnerRootName);
            BossSummonDuelReviewOwner duelOwner = EnsureComponent<BossSummonDuelReviewOwner>(duelOwnerRoot);
            GameObject clearMarker = EnsureResultMarker(
                duelOwnerRoot.transform,
                DuelClearMarkerName,
                laneSpace.GetBattlefieldWorldPoint(-laneSpace.HalfWidth - 1.35f, laneSpace.ForwardBoundaryZ + 1.0f, 0.75f),
                new Color(0.25f, 1f, 0.5f, 1f));
            GameObject failMarker = EnsureResultMarker(
                duelOwnerRoot.transform,
                DuelFailMarkerName,
                laneSpace.GetBattlefieldWorldPoint(laneSpace.HalfWidth + 1.35f, laneSpace.ForwardBoundaryZ + 1.0f, 0.75f),
                new Color(1f, 0.16f, 0.18f, 1f));
            ConfigureBossSummonDuelOwner(
                duelOwner,
                playerHealth,
                bossHealth,
                energyLadder,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                emitter,
                bossBasicFireEmitter,
                bossPressureCost,
                bossPressureActionDirector,
                bossSummonPressureAction,
                clearMarker,
                failMarker);

            SetObjectReference(reviewHud, "closeThreatHealth", null);
            SetObjectReference(reviewHud, "pocketReviewOwner", null);
            SetObjectReference(reviewHud, "duelReviewOwner", duelOwner);
            SetObjectReference(screenCuePresenter, "pocketReviewOwner", null);
            SetObjectReference(screenCuePresenter, "duelReviewOwner", duelOwner);

            EditorUtility.SetDirty(player.gameObject);
            EditorUtility.SetDirty(reviewHud);
            EditorUtility.SetDirty(mobileHud);
            EditorUtility.SetDirty(screenCuePresenter);
            EditorSceneManager.MarkSceneDirty(scene);

            if (!EditorSceneManager.SaveScene(scene, CombatStageScenePath))
            {
                throw new InvalidOperationException($"Failed to save combat stage scene at {CombatStageScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        private static void RequireCombatStageInoriPlayerVisual(
            GameObject player,
            PlayerCombatModeController combatModeController)
        {
            Transform inoriVisual = FindDescendant(player.transform, CombatStageRangedInoriVisualName);
            if (inoriVisual == null)
            {
                throw new InvalidOperationException(
                    $"{CombatStageScenePath} must reuse {CombatStageRangedInoriVisualName} from {ReviewScenePath}.");
            }

            Transform retiredRifleGirlVisual = FindDescendant(player.transform, CombatStageRetiredRifleGirlVisualName);
            if (retiredRifleGirlVisual != null)
            {
                throw new InvalidOperationException(
                    $"{CombatStageScenePath} must not keep retired ranged visual {CombatStageRetiredRifleGirlVisualName}.");
            }

            GameObject rangedVisualRoot = RequireReferencedObject<GameObject>(combatModeController, "rangedVisualRoot");
            if (rangedVisualRoot != inoriVisual.gameObject)
            {
                throw new InvalidOperationException(
                    $"{CombatStageScenePath} should bind rangedVisualRoot to {CombatStageRangedInoriVisualName}.");
            }
        }

        private static void SeedCombatStageAimDefaults(
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            BossBarrageLaneReviewMobileHud mobileHud)
        {
            SetBool(rangedBasicAttackAction, "useAimAssist", true);
            SetFloat(rangedBasicAttackAction, "hipAimAssistAngleDegrees", CombatStageAimAssistAngleDegrees);
            SetFloat(rangedBasicAttackAction, "aimedAimAssistAngleDegrees", CombatStageAimAssistAngleDegrees);
            SetFloat(rangedBasicAttackAction, "aimAssistMaxTurnDegrees", CombatStageAimAssistMaxTurnDegrees);
            SetBool(mobileHud, "fireAimReticleUsesScreenCenter", true);
            SetBool(mobileHud, "fireAimReticleFollowsAssist", false);
            EditorUtility.SetDirty(rangedBasicAttackAction);
            EditorUtility.SetDirty(mobileHud);
        }
    }
}
