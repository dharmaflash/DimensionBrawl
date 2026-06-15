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

        [Header("Player Actions")]
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;

        [Header("Pressure")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private bool stopBarrageOnClear = true;

        [Header("Inspectable Result Markers")]
        [SerializeField] private GameObject clearMarker;
        [SerializeField] private GameObject failMarker;

        private PocketState state;
        private bool usedSkill1;
        private bool usedSummonSlot1;
        private int highestSkillTier;
        private int highestSummonTier;

        public bool IsRunning => state == PocketState.Running;
        public bool IsCleared => state == PocketState.Cleared;
        public bool IsFailed => state == PocketState.Failed;
        public bool UsedSkill1 => usedSkill1;
        public bool UsedSummonSlot1 => usedSummonSlot1;
        public int HighestSkillTier => highestSkillTier;
        public int HighestSummonTier => highestSummonTier;
        public string ObjectiveCue => "Defeat close threat and spend SummonSlot1";

        public void Configure(
            CombatHealth newPlayerHealth,
            CombatHealth newCloseThreatHealth,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            BossBarrageEmitter newBossBarrageEmitter,
            GameObject newClearMarker,
            GameObject newFailMarker)
        {
            playerHealth = newPlayerHealth;
            closeThreatHealth = newCloseThreatHealth;
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
            highestSkillTier = 0;
            highestSummonTier = 0;
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

            if (playerHealth != null && !playerHealth.IsAlive)
            {
                FailPocket();
                return;
            }

            if (closeThreatHealth != null && !closeThreatHealth.IsAlive && usedSummonSlot1)
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
        }

        private void ClearPocket()
        {
            state = PocketState.Cleared;
            if (stopBarrageOnClear && bossBarrageEmitter != null)
            {
                bossBarrageEmitter.SetFiringEnabled(false);
            }

            SetMarkers();
        }

        private void FailPocket()
        {
            state = PocketState.Failed;
            SetMarkers();
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
