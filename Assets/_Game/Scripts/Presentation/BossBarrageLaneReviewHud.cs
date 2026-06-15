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
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;

        [Header("Display")]
        [SerializeField] private bool showHud = true;
        [SerializeField, Min(1f)] private float width = 430f;
        [SerializeField, Min(1f)] private float height = 205f;
        [SerializeField, Min(0f)] private float margin = 18f;

        private GUIStyle labelStyle;
        private GUIStyle boxStyle;

        public void Configure(
            CombatHealth newPlayerHealth,
            CombatHealth newCloseThreatHealth,
            SummonEnergyLadder newEnergyLadder,
            SummonLaneSpace newLaneSpace,
            Transform newPlayer,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            BossBarrageEmitter newBossBarrageEmitter,
            BossBarragePocketReviewOwner newPocketReviewOwner)
        {
            playerHealth = newPlayerHealth;
            closeThreatHealth = newCloseThreatHealth;
            energyLadder = newEnergyLadder;
            laneSpace = newLaneSpace;
            player = newPlayer;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            pocketReviewOwner = newPocketReviewOwner;
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            EnsureStyles();
            GUILayout.BeginArea(new Rect(margin, margin, width, height), boxStyle);
            GUILayout.Label("Boss Barrage Lane Review", labelStyle);
            GUILayout.Label(ResolveHealthLine(), labelStyle);
            GUILayout.Label(ResolveEnergyLine(), labelStyle);
            GUILayout.Label(ResolveRiskLine(), labelStyle);
            GUILayout.Label(ResolveBossBarrageLine(), labelStyle);
            GUILayout.Label(ResolveActionLine(), labelStyle);
            GUILayout.Label(ResolveObjectiveLine(), labelStyle);
            GUILayout.EndArea();
        }

        private string ResolveHealthLine()
        {
            string playerLine = playerHealth != null
                ? $"HP {playerHealth.CurrentHealth:0}/{playerHealth.MaxHealth:0}"
                : "HP -";
            string threatLine = closeThreatHealth != null
                ? $"Close Threat {closeThreatHealth.CurrentHealth:0}/{closeThreatHealth.MaxHealth:0}"
                : "Close Threat -";
            return $"{playerLine}   {threatLine}";
        }

        private string ResolveEnergyLine()
        {
            if (energyLadder == null)
            {
                return "EN -";
            }

            string ready = energyLadder.CanSpend ? $"READY LV{energyLadder.AvailableTier}" : "charging";
            return $"EN LV{energyLadder.ChargingTier} {energyLadder.CurrentTierFillRatio * 100f:0}%   {ready}";
        }

        private string ResolveRiskLine()
        {
            float risk = laneSpace != null && player != null ? laneSpace.EvaluateForwardRisk01(player.position) : 0f;
            float gain = energyLadder != null ? energyLadder.CurrentGainMultiplier : 0f;
            return $"Forward Risk {risk * 100f:0}%   EN Gain x{gain:0.00}";
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
            if (labelStyle != null && boxStyle != null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = Color.white }
            };
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 12, 12),
                normal = { textColor = Color.white }
            };
        }
    }
}
