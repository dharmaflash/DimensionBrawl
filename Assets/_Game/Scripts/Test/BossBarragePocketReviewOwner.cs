using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Test
{
    public sealed class BossBarragePocketReviewOwner : MonoBehaviour
    {
        private enum PocketState
        {
            Running,
            Cleared,
            Failed
        }

        [Header("Combatants")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth closeThreatHealth;
        [SerializeField] private SummonEnergyLadder energyLadder;

        [Header("Player Actions")]
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;

        [Header("Pressure")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private bool stopBarrageOnClear = true;
        [SerializeField] private bool stopBarrageOnFail = true;

        [Header("Resource")]
        [SerializeField] private bool stopEnergyGainOnEnd = true;

        [Header("Inspectable Result Markers")]
        [SerializeField] private GameObject clearMarker;
        [SerializeField] private GameObject failMarker;

        private PocketState state;
        private bool usedSkill1;
        private bool usedSummonSlot1;
        private bool closeThreatDefeated;
        private bool blockedBossPressureWithSummon;
        private int pressureBlocksAtCloseThreatDefeat;
        private int highestSkillTier;
        private int highestSummonTier;
        private int highestSummonPressureTier;

        public bool IsRunning => state == PocketState.Running;
        public bool IsCleared => state == PocketState.Cleared;
        public bool IsFailed => state == PocketState.Failed;
        public bool UsedSkill1 => usedSkill1;
        public bool UsedSummonSlot1 => usedSummonSlot1;
        public bool CloseThreatDefeated => closeThreatDefeated;
        public bool BlockedBossPressureWithSummon => blockedBossPressureWithSummon;
        public int PressureBlocksAfterCloseThreatDefeated => CountPressureBlocksAfterCloseThreatDefeated();
        public int HighestSkillTier => highestSkillTier;
        public int HighestSummonTier => highestSummonTier;
        public int HighestSummonPressureTier => highestSummonPressureTier;
        public string ObjectiveCue => "Defeat close threat and block boss fire with SummonSlot1";

        public void Configure(
            CombatHealth newPlayerHealth,
            CombatHealth newCloseThreatHealth,
            SummonEnergyLadder newEnergyLadder,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            BossBarrageEmitter newBossBarrageEmitter,
            GameObject newClearMarker,
            GameObject newFailMarker)
        {
            playerHealth = newPlayerHealth;
            closeThreatHealth = newCloseThreatHealth;
            energyLadder = newEnergyLadder;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            clearMarker = newClearMarker;
            failMarker = newFailMarker;
            ResetPocket();
        }

        public void ResetPocket()
        {
            state = PocketState.Running;
            usedSkill1 = false;
            usedSummonSlot1 = false;
            closeThreatDefeated = false;
            blockedBossPressureWithSummon = false;
            pressureBlocksAtCloseThreatDefeat = 0;
            highestSkillTier = 0;
            highestSummonTier = 0;
            highestSummonPressureTier = 0;
            SetBarrageEnabled(true);
            SetEnergyGainEnabled(true);
            SetMarkers();
        }

        private void OnEnable()
        {
            ResetPocket();
        }

        private void Update()
        {
            if (state != PocketState.Running)
            {
                return;
            }

            CaptureActionUse();
            CaptureCloseThreatDefeat();

            if (playerHealth != null && !playerHealth.IsAlive)
            {
                FailPocket();
                return;
            }

            if (closeThreatDefeated
                && usedSummonSlot1
                && blockedBossPressureWithSummon)
            {
                ClearPocket();
            }
        }

        private void CaptureActionUse()
        {
            if (skill1Action != null && skill1Action.LastSpentTier > 0)
            {
                usedSkill1 = true;
                highestSkillTier = Mathf.Max(highestSkillTier, skill1Action.LastSpentTier);
            }

            if (summonSlot1Action != null && summonSlot1Action.LastSpentTier > 0)
            {
                usedSummonSlot1 = true;
                highestSummonTier = Mathf.Max(highestSummonTier, summonSlot1Action.LastSpentTier);
            }

            if (summonSlot1Action != null
                && closeThreatDefeated
                && summonSlot1Action.TotalPressureScreenInterceptCount > pressureBlocksAtCloseThreatDefeat)
            {
                blockedBossPressureWithSummon = true;
                highestSummonPressureTier = Mathf.Max(
                    highestSummonPressureTier,
                    summonSlot1Action.LastPressureScreenInterceptTier);
            }
        }

        private void CaptureCloseThreatDefeat()
        {
            if (closeThreatDefeated
                || closeThreatHealth == null
                || closeThreatHealth.IsAlive)
            {
                return;
            }

            closeThreatDefeated = true;
            pressureBlocksAtCloseThreatDefeat = summonSlot1Action != null
                ? summonSlot1Action.TotalPressureScreenInterceptCount
                : 0;
        }

        private int CountPressureBlocksAfterCloseThreatDefeated()
        {
            if (!closeThreatDefeated || summonSlot1Action == null)
            {
                return 0;
            }

            return Mathf.Max(0, summonSlot1Action.TotalPressureScreenInterceptCount - pressureBlocksAtCloseThreatDefeat);
        }

        private void ClearPocket()
        {
            state = PocketState.Cleared;
            SetBarrageEnabled(!stopBarrageOnClear);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetMarkers();
        }

        private void FailPocket()
        {
            state = PocketState.Failed;
            SetBarrageEnabled(!stopBarrageOnFail);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetMarkers();
        }

        private void SetBarrageEnabled(bool enabled)
        {
            if (bossBarrageEmitter != null)
            {
                bossBarrageEmitter.SetFiringEnabled(enabled);
            }
        }

        private void SetEnergyGainEnabled(bool enabled)
        {
            if (energyLadder != null)
            {
                energyLadder.SetGainEnabled(enabled);
            }
        }

        private void SetMarkers()
        {
            if (clearMarker != null)
            {
                clearMarker.SetActive(state == PocketState.Cleared);
            }

            if (failMarker != null)
            {
                failMarker.SetActive(state == PocketState.Failed);
            }
        }
    }
}
