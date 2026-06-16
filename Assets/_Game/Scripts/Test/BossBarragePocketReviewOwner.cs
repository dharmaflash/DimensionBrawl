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
        [SerializeField, Min(0f)] private float closeThreatDefeatPressureReliefSeconds = 0.9f;
        [SerializeField, Min(0f)] private float summonPressureBreakReliefSeconds = 2.4f;
        [SerializeField, Min(0f)] private float summonFollowupWindowSeconds = 1.4f;

        [Header("Resource")]
        [SerializeField] private bool stopEnergyGainOnEnd = true;

        [Header("Inspectable Result Markers")]
        [SerializeField] private GameObject clearMarker;
        [SerializeField] private GameObject failMarker;

        private readonly BossBarragePocketPressurePacing pressurePacing = new BossBarragePocketPressurePacing();
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
        public bool IsPressureReliefActive => pressurePacing.IsCloseThreatReliefActive;
        public bool IsSummonPressureBreakActive => pressurePacing.IsSummonPressureBreakActive;
        public bool IsSummonFollowupWindowActive => pressurePacing.IsSummonFollowupWindowActive;
        public float PressureReliefRemainingSeconds => pressurePacing.CloseThreatReliefRemainingSeconds;
        public float SummonPressureBreakRemainingSeconds => pressurePacing.SummonPressureBreakRemainingSeconds;
        public float SummonFollowupWindowRemainingSeconds => pressurePacing.SummonFollowupWindowRemainingSeconds;
        public int PressureBlocksAfterCloseThreatDefeated => CountPressureBlocksAfterCloseThreatDefeated();
        public int HighestSkillTier => highestSkillTier;
        public int HighestSummonTier => highestSummonTier;
        public int HighestSummonPressureTier => highestSummonPressureTier;
        public string ObjectiveCue
        {
            get
            {
                if (pressurePacing.IsSummonPressureBreakActive)
                {
                    return pressurePacing.IsSummonFollowupWindowActive
                        ? "Summon block opened a follow-up window"
                        : "Boss pressure is broken briefly";
                }

                return closeThreatDefeated
                    ? "Block boss fire with SummonSlot1"
                    : "Defeat close threat and prepare SummonSlot1";
            }
        }

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
            pressurePacing.Reset();
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
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
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

            CaptureCloseThreatDefeat();
            UpdatePressurePacing(deltaTime);

            if (closeThreatDefeated
                && usedSummonSlot1
                && blockedBossPressureWithSummon
                && !pressurePacing.IsSummonPressureBreakActive)
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
                if (!blockedBossPressureWithSummon)
                {
                    StartSummonPressureBreak();
                }

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
            StartPressureRelief();
        }

        private int CountPressureBlocksAfterCloseThreatDefeated()
        {
            if (!closeThreatDefeated || summonSlot1Action == null)
            {
                return 0;
            }

            return Mathf.Max(0, summonSlot1Action.TotalPressureScreenInterceptCount - pressureBlocksAtCloseThreatDefeat);
        }

        private void StartPressureRelief()
        {
            pressurePacing.StartCloseThreatRelief(closeThreatDefeatPressureReliefSeconds);
            ApplyRunningBarragePacing();
        }

        private void StartSummonPressureBreak()
        {
            pressurePacing.StartSummonPressureBreak(
                summonPressureBreakReliefSeconds,
                summonFollowupWindowSeconds);
            ApplyRunningBarragePacing();
        }

        private void UpdatePressurePacing(float deltaTime)
        {
            pressurePacing.Tick(deltaTime);
            ApplyRunningBarragePacing();
        }

        private void ClearPocket()
        {
            state = PocketState.Cleared;
            ClearPressurePacing();
            SetBarrageEnabled(!stopBarrageOnClear);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetMarkers();
        }

        private void FailPocket()
        {
            state = PocketState.Failed;
            ClearPressurePacing();
            SetBarrageEnabled(!stopBarrageOnFail);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetMarkers();
        }

        private void ClearPressurePacing()
        {
            pressurePacing.Reset();
        }

        private void ApplyRunningBarragePacing()
        {
            if (state != PocketState.Running)
            {
                return;
            }

            SetBarrageEnabled(!pressurePacing.ShouldPauseBarrage);
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

        private sealed class BossBarragePocketPressurePacing
        {
            private float closeThreatReliefTimer;
            private float summonPressureBreakTimer;
            private float summonFollowupWindowTimer;
            private bool closeThreatReliefActive;
            private bool summonPressureBreakActive;
            private bool summonFollowupWindowActive;

            public bool IsCloseThreatReliefActive => closeThreatReliefActive;
            public bool IsSummonPressureBreakActive => summonPressureBreakActive;
            public bool IsSummonFollowupWindowActive => summonFollowupWindowActive;
            public float CloseThreatReliefRemainingSeconds => closeThreatReliefTimer;
            public float SummonPressureBreakRemainingSeconds => summonPressureBreakTimer;
            public float SummonFollowupWindowRemainingSeconds => summonFollowupWindowTimer;
            public bool ShouldPauseBarrage => closeThreatReliefActive || summonPressureBreakActive;

            public void Reset()
            {
                closeThreatReliefTimer = 0f;
                summonPressureBreakTimer = 0f;
                summonFollowupWindowTimer = 0f;
                closeThreatReliefActive = false;
                summonPressureBreakActive = false;
                summonFollowupWindowActive = false;
            }

            public void StartCloseThreatRelief(float seconds)
            {
                closeThreatReliefTimer = Mathf.Max(0f, seconds);
                closeThreatReliefActive = closeThreatReliefTimer > 0f;
            }

            public void StartSummonPressureBreak(float reliefSeconds, float followupWindowSeconds)
            {
                summonPressureBreakTimer = Mathf.Max(0f, reliefSeconds);
                summonFollowupWindowTimer = Mathf.Max(0f, followupWindowSeconds);
                summonPressureBreakActive = summonPressureBreakTimer > 0f;
                summonFollowupWindowActive = summonFollowupWindowTimer > 0f;
            }

            public void Tick(float deltaTime)
            {
                float safeDeltaTime = Mathf.Max(0f, deltaTime);
                TickCloseThreatRelief(safeDeltaTime);
                TickSummonPressureBreak(safeDeltaTime);
                TickSummonFollowupWindow(safeDeltaTime);
            }

            private void TickCloseThreatRelief(float deltaTime)
            {
                if (!closeThreatReliefActive)
                {
                    return;
                }

                closeThreatReliefTimer = Mathf.Max(0f, closeThreatReliefTimer - deltaTime);
                if (closeThreatReliefTimer <= 0f)
                {
                    closeThreatReliefActive = false;
                }
            }

            private void TickSummonPressureBreak(float deltaTime)
            {
                if (!summonPressureBreakActive)
                {
                    return;
                }

                summonPressureBreakTimer = Mathf.Max(0f, summonPressureBreakTimer - deltaTime);
                if (summonPressureBreakTimer <= 0f)
                {
                    summonPressureBreakActive = false;
                }
            }

            private void TickSummonFollowupWindow(float deltaTime)
            {
                if (!summonFollowupWindowActive)
                {
                    return;
                }

                summonFollowupWindowTimer = Mathf.Max(0f, summonFollowupWindowTimer - deltaTime);
                if (summonFollowupWindowTimer <= 0f)
                {
                    summonFollowupWindowActive = false;
                }
            }
        }
    }
}
