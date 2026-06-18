using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class BossBarrageLaneReviewHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth closeThreatHealth;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedAimController rangedAimController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossPressureCostLadder bossPressureCostLadder;
        [SerializeField] private BossPressurePositionController bossPressurePositionController;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private BossSummonPressureAction bossSummonPressureAction;
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;

        [Header("Display")]
        [SerializeField] private bool showHud = true;
        [SerializeField, Min(1f)] private float width = 430f;
        [SerializeField, Min(1f)] private float height = 280f;
        [SerializeField, Min(0f)] private float margin = 18f;
        [SerializeField] private bool showCenterReticle;

        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle boxStyle;

        public void Configure(
            CombatHealth newPlayerHealth,
            CombatHealth newCloseThreatHealth,
            CombatHealth newBossHealth,
            SummonEnergyLadder newEnergyLadder,
            SummonLaneSpace newLaneSpace,
            Transform newPlayer,
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newRangedAimController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            BossBarrageEmitter newBossBarrageEmitter,
            BossBarragePocketReviewOwner newPocketReviewOwner,
            BossPressureCostLadder newBossPressureCostLadder = null,
            BossPressurePositionController newBossPressurePositionController = null,
            BossPressureActionDirector newBossPressureActionDirector = null,
            BossSummonPressureAction newBossSummonPressureAction = null)
        {
            playerHealth = newPlayerHealth;
            closeThreatHealth = newCloseThreatHealth;
            bossHealth = newBossHealth;
            energyLadder = newEnergyLadder;
            laneSpace = newLaneSpace;
            player = newPlayer;
            combatModeController = newCombatModeController;
            rangedAimController = newRangedAimController;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            bossPressureCostLadder = newBossPressureCostLadder;
            bossPressurePositionController = newBossPressurePositionController;
            bossPressureActionDirector = newBossPressureActionDirector;
            bossSummonPressureAction = newBossSummonPressureAction;
            pocketReviewOwner = newPocketReviewOwner;
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            GUI.depth = -1000;
            Matrix4x4 previousMatrix = GUI.matrix;
            float uiScale = ResolveUiScale();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1f));
            EnsureStyles();
            float areaHeight = Mathf.Max(height, (Screen.height / uiScale) - (margin * 2f));
            GUILayout.BeginArea(new Rect(margin, margin, width, areaHeight), boxStyle);
            GUILayout.Label("Boss Barrage Lane Review", titleStyle);
            GUILayout.Label(ResolveHealthLine(), labelStyle);
            GUILayout.Label(ResolvePhaseLine(), labelStyle);
            GUILayout.Label(ResolveEnergyLine(), labelStyle);
            GUILayout.Label(ResolveRiskLine(), labelStyle);
            GUILayout.Label(ResolveBossBarrageLine(), labelStyle);
            GUILayout.Label(ResolveBossPressureLine(), labelStyle);
            GUILayout.Label(ResolveBossSummonLine(), labelStyle);
            GUILayout.Label(ResolveWeaponModeLine(), labelStyle);
            GUILayout.Label(ResolveRangedFireLine(), labelStyle);
            GUILayout.Label(ResolveActionLine(), labelStyle);
            GUILayout.Label(ResolveActionHintLine(), labelStyle);
            GUILayout.Label(ResolveSummonExchangeLine(), labelStyle);
            GUILayout.Label(ResolveObjectiveLine(), labelStyle);
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
            DrawReticleIfNeeded();
        }

        private string ResolveHealthLine()
        {
            string playerLine = playerHealth != null
                ? $"HP {playerHealth.CurrentHealth:0}/{playerHealth.MaxHealth:0}"
                : "HP -";
            string threatLine = closeThreatHealth != null
                ? $"Threat {closeThreatHealth.CurrentHealth:0}/{closeThreatHealth.MaxHealth:0}"
                : "Threat -";
            string bossLine = bossHealth != null
                ? $"Boss {bossHealth.CurrentHealth:0}/{bossHealth.MaxHealth:0}"
                : "Boss -";
            return $"{playerLine}   {threatLine}   {bossLine}";
        }

        private string ResolvePhaseLine()
        {
            if (pocketReviewOwner == null)
            {
                return "Phase -";
            }

            return $"Phase {pocketReviewOwner.CurrentPhase}";
        }

        private string ResolveEnergyLine()
        {
            if (energyLadder == null)
            {
                return "EN -";
            }

            string ready = energyLadder.CanSpend
                ? $"READY LV{energyLadder.AvailableTier}"
                : "not ready";
            return $"EN next LV{energyLadder.ChargingTier} {energyLadder.CurrentTierFillRatio * 100f:0}%   {ready}";
        }

        private string ResolveRiskLine()
        {
            float risk = energyLadder != null
                ? energyLadder.CurrentForwardRisk01
                : laneSpace != null && player != null
                    ? laneSpace.EvaluateForwardRisk01(player.position)
                    : 0f;
            float gain = energyLadder != null ? energyLadder.CurrentGainMultiplier : 0f;
            string band = energyLadder != null ? ResolveRiskBandLabel(energyLadder.CurrentRiskBand) : "BackSafety";
            return $"Risk {band} {risk * 100f:0}%   EN Gain x{gain:0.00}";
        }

        private string ResolveBossBarrageLine()
        {
            if (bossBarrageEmitter == null)
            {
                return "Boss Pattern -";
            }

            BossBarragePatternProfile pattern = bossBarrageEmitter.CurrentPattern;
            if (pattern == null)
            {
                return "Boss Pattern -";
            }

            string pressureState = bossBarrageEmitter.IsWindupActive
                ? $"windup risk {bossBarrageEmitter.PendingForwardRisk01 * 100f:0}%"
                : $"shots {bossBarrageEmitter.ActiveProjectileCount}";
            return $"Boss P{bossBarrageEmitter.CurrentPatternSequenceIndex + 1}: {pattern.PatternId} [{pattern.LateralShape}]   {pressureState}";
        }

        private string ResolveBossPressureLine()
        {
            if (bossPressureCostLadder == null)
            {
                return "Boss Cost -";
            }

            string ready = bossPressureCostLadder.CanSpend
                ? $"READY LV{bossPressureCostLadder.AvailableTier}"
                : "not ready";
            string action = bossPressureActionDirector != null && bossPressureActionDirector.TotalActionCount > 0
                ? $"{bossPressureActionDirector.LastActionKind}"
                : "-";
            string pattern = bossPressureActionDirector != null && bossPressureActionDirector.LastQueuedPattern != null
                ? bossPressureActionDirector.LastQueuedPattern.PatternId
                : "-";
            string position = bossPressurePositionController != null
                ? $" Pos {bossPressurePositionController.CurrentRisk01 * 100f:0}->{bossPressurePositionController.CurrentTargetRisk01 * 100f:0}%"
                : string.Empty;
            return $"Boss Cost next LV{bossPressureCostLadder.ChargingTier} "
                + $"{bossPressureCostLadder.CurrentTierFillRatio * 100f:0}%   {ready}   "
                + $"Risk {bossPressureCostLadder.CurrentRiskBand} x{bossPressureCostLadder.CurrentGainMultiplier:0.00}{position}   "
                + $"Last {action}/{pattern}";
        }

        private string ResolveBossSummonLine()
        {
            if (bossSummonPressureAction == null)
            {
                return "Boss Summon -";
            }

            string tier = bossSummonPressureAction.LastReleasedTier > 0
                ? $"LV{bossSummonPressureAction.LastReleasedTier}"
                : "LV-";
            return $"Boss Summon {tier} proxy {bossSummonPressureAction.ActiveSummonActorCount} "
                + $"shield {bossSummonPressureAction.ActivePressureScreenCount} "
                + $"blocks {bossSummonPressureAction.ActivePressureScreenRemainingIntercepts} "
                + $"used {bossSummonPressureAction.TotalReleaseCount}";
        }

        private string ResolveActionLine()
        {
            string skill = skill1Action != null && energyLadder != null && energyLadder.CanSpend
                ? $"Skill1 LV{energyLadder.AvailableTier}"
                : "Skill1 not ready";
            string summon = summonSlot1Action != null && energyLadder != null && energyLadder.CanSpend
                ? $"SummonSlot1 LV{energyLadder.AvailableTier}"
                : "SummonSlot1 not ready";
            return $"{skill}   {summon}";
        }

        private string ResolveWeaponModeLine()
        {
            string mode = combatModeController != null ? combatModeController.CurrentMode.ToString() : "-";
            string aim = rangedAimController != null && rangedAimController.IsAiming ? "AIM" : "hip";
            return $"Weapon {mode}   Aim {aim}";
        }

        private string ResolveRangedFireLine()
        {
            if (rangedBasicAttackAction == null)
            {
                return "Ranged Fire -";
            }

            string ready = rangedBasicAttackAction.IsFireReady
                ? "READY"
                : $"{rangedBasicAttackAction.FireCooldownRemaining:0.00}s";
            return $"Ranged Fire {ready}   bolts {rangedBasicAttackAction.ActiveProjectileCount}";
        }

        private string ResolveSummonExchangeLine()
        {
            string skillShots = skill1Action != null
                ? $"Skill bolts {skill1Action.ActiveProjectileCount}"
                : "Skill bolts -";
            if (summonSlot1Action == null)
            {
                return $"{skillShots}   Summon -";
            }

            string tier = summonSlot1Action.LastSpentTier > 0
                ? $"LV{summonSlot1Action.LastSpentTier}"
                : "LV-";
            return $"{skillShots}   Summon {tier} proxy {summonSlot1Action.ActiveSummonActorCount} "
                + $"bolts {summonSlot1Action.ActiveProjectileCount} shield {summonSlot1Action.ActivePressureScreenCount} "
                + $"blocks {summonSlot1Action.ActivePressureScreenRemainingIntercepts}"
                + ResolveFollowupLine();
        }

        private string ResolveActionHintLine()
        {
            if (skill1Action != null && skill1Action.ShowUseBlockedHint && !string.IsNullOrWhiteSpace(skill1Action.LastUseBlockedReason))
            {
                return $"Hint: {skill1Action.LastUseBlockedReason}";
            }

            if (summonSlot1Action != null && summonSlot1Action.ShowUseBlockedHint && !string.IsNullOrWhiteSpace(summonSlot1Action.LastUseBlockedReason))
            {
                return $"Hint: {summonSlot1Action.LastUseBlockedReason}";
            }

            if (rangedBasicAttackAction != null
                && rangedBasicAttackAction.ShowUseBlockedHint
                && !string.IsNullOrWhiteSpace(rangedBasicAttackAction.LastUseBlockedReason))
            {
                return $"Hint: {rangedBasicAttackAction.LastUseBlockedReason}";
            }

            return "Hint: -";
        }

        private string ResolveFollowupLine()
        {
            if (pocketReviewOwner == null || !pocketReviewOwner.IsSummonPressureBreakActive)
            {
                return ResolveLastPressureRewardLine();
            }

            string followup = pocketReviewOwner.IsSummonFollowupWindowActive
                ? $" follow-up {pocketReviewOwner.SummonFollowupWindowRemainingSeconds:0.0}s"
                : " relief";
            if (pocketReviewOwner.Skill1FollowupHitConfirmed)
            {
                return $"{followup} Skill1 hit {pocketReviewOwner.Skill1FollowupDamage:0}";
            }

            return pocketReviewOwner.UsedSkill1DuringSummonFollowup
                ? $"{followup} Skill1 fired"
                : $"{followup} EN pulse";
        }

        private string ResolveLastPressureRewardLine()
        {
            if (pocketReviewOwner == null || pocketReviewOwner.LastSummonPressureBreakTier <= 0)
            {
                return string.Empty;
            }

            return $" last break LV{pocketReviewOwner.LastSummonPressureBreakTier}"
                + $" {pocketReviewOwner.LastSummonPressureBreakDuration:0.0}s"
                + $" pulse {pocketReviewOwner.SummonFollowupEnergyPulse:0}";
        }

        private string ResolveObjectiveLine()
        {
            if (pocketReviewOwner == null)
            {
                return "Objective -";
            }

            string state = pocketReviewOwner.IsCleared
                ? "CLEARED"
                : pocketReviewOwner.IsFailed
                    ? "FAILED"
                    : "RUNNING";
            return $"{state}: {pocketReviewOwner.ObjectiveCue}";
        }

        private void EnsureStyles()
        {
            if (labelStyle != null && titleStyle != null && boxStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                normal = { textColor = Color.white }
            };
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(18, 18, 16, 16),
                normal = { textColor = Color.white }
            };
        }

        private static float ResolveUiScale()
        {
            return Mathf.Clamp(Screen.height / 1440f, 1f, 2f);
        }

        private void DrawReticleIfNeeded()
        {
            if (!showCenterReticle)
            {
                return;
            }

            if (combatModeController != null && !combatModeController.IsRangedMode)
            {
                return;
            }

            if (rangedBasicAttackAction == null)
            {
                return;
            }

            bool aiming = rangedAimController != null && rangedAimController.IsAiming;
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;
            float gap = aiming ? 9f : 6f;
            float length = aiming ? 18f : 10f;
            float thickness = aiming ? 3f : 2f;
            Color previousColor = GUI.color;
            GUI.color = aiming
                ? new Color(0.42f, 0.95f, 1f, 0.92f)
                : new Color(1f, 1f, 1f, 0.42f);

            GUI.DrawTexture(new Rect(centerX - gap - length, centerY - thickness * 0.5f, length, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX + gap, centerY - thickness * 0.5f, length, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - thickness * 0.5f, centerY - gap - length, thickness, length), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - thickness * 0.5f, centerY + gap, thickness, length), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - thickness * 0.5f, centerY - thickness * 0.5f, thickness, thickness), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private static string ResolveRiskBandLabel(SummonEnergyRiskBand riskBand)
        {
            return riskBand switch
            {
                SummonEnergyRiskBand.ForwardRisk => "ForwardRisk",
                SummonEnergyRiskBand.MidCharge => "MidCharge",
                _ => "BackSafety"
            };
        }
    }
}
