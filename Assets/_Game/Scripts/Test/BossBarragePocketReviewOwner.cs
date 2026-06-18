using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Test
{
    public sealed class BossBarragePocketReviewOwner : MonoBehaviour
    {
        public enum ReviewPhase
        {
            ThreatDefense,
            SummonBlock,
            SummonFollowup,
            PressureBreak,
            Cleared,
            Failed
        }

        private enum PocketState
        {
            Running,
            Cleared,
            Failed
        }

        [Header("Combatants")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth closeThreatHealth;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private SummonEnergyLadder energyLadder;

        [Header("Player Actions")]
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;

        [Header("Pressure")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossPressureCostLadder bossPressureCostLadder;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private bool stopBarrageOnClear = true;
        [SerializeField] private bool stopBarrageOnFail = true;
        [SerializeField] private bool stopBossPressureCostOnEnd = true;
        [SerializeField] private bool stopBossPressureActionsOnEnd = true;
        [SerializeField, Min(0f)] private float closeThreatDefeatPressureReliefSeconds = 0.9f;
        [SerializeField, Min(0f)] private float summonPressureBreakReliefSeconds = 2.4f;
        [SerializeField, Min(0f)] private float summonFollowupWindowSeconds = 1.4f;
        [SerializeField, Min(0f)] private float summonFollowupEnergyPulse = 100f;
        [SerializeField, Min(0f)] private float summonPressureBreakTierTwoBonusSeconds = 0.35f;
        [SerializeField, Min(0f)] private float summonPressureBreakTierThreeBonusSeconds = 0.7f;
        [SerializeField, Min(0f)] private float summonFollowupWindowTierTwoBonusSeconds = 0.2f;
        [SerializeField, Min(0f)] private float summonFollowupWindowTierThreeBonusSeconds = 0.45f;
        [SerializeField, Min(0f)] private float summonFollowupEnergyPulseTierTwo = 155f;
        [SerializeField, Min(0f)] private float summonFollowupEnergyPulseTierThree = 200f;

        [Header("Follow-up Result")]
        [SerializeField] private bool requireSkill1FollowupHitToClear = true;

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
        private bool grantedSummonFollowupEnergy;
        private bool usedSkill1DuringSummonFollowup;
        private bool skill1FollowupHitConfirmed;
        private float skill1FollowupDamage;
        private int pressureBlocksAtCloseThreatDefeat;
        private int pressureBlocksConsumedBySummonBreak;
        private int bossPressureBlocksAtSummonBreakStart;
        private int bossPressureBlocksConsumedDuringFollowup;
        private int observedSkillUseCount;
        private int observedSummonUseCount;
        private int skillUsesAtSummonBreakStart;
        private int highestSkillTier;
        private int highestSummonTier;
        private int highestSummonPressureTier;
        private int highestSummonFollowupSkillTier;
        private int highestSkill1FollowupHitTier;
        private int lastSummonPressureBreakTier;
        private float lastSummonPressureBreakDuration;
        private float lastSummonFollowupWindowDuration;
        private float lastGrantedSummonFollowupEnergyPulse;
        private CombatHealth subscribedBossHealth;
        private bool followupMissedNotified;
        private bool bossBlockedSkill1Followup;

        public event Action<int> SummonFollowupWindowOpened;
        public event Action<int, float> SummonFollowupHitConfirmed;
        public event Action SummonFollowupMissed;
        public event Action SummonBlockOpportunityOpened;

        public bool IsRunning => state == PocketState.Running;
        public bool IsCleared => state == PocketState.Cleared;
        public bool IsFailed => state == PocketState.Failed;
        public bool UsedSkill1 => usedSkill1;
        public bool UsedSummonSlot1 => usedSummonSlot1;
        public bool CloseThreatDefeated => closeThreatDefeated;
        public bool BlockedBossPressureWithSummon => blockedBossPressureWithSummon;
        public bool GrantedSummonFollowupEnergy => grantedSummonFollowupEnergy;
        public bool UsedSkill1DuringSummonFollowup => usedSkill1DuringSummonFollowup;
        public bool Skill1FollowupHitConfirmed => skill1FollowupHitConfirmed;
        public bool BossBlockedSkill1Followup => bossBlockedSkill1Followup;
        public int BossPressureBlocksDuringSummonFollowup => bossPressureBlocksConsumedDuringFollowup;
        public bool IsPressureReliefActive => pressurePacing.IsCloseThreatReliefActive;
        public bool IsSummonPressureBreakActive => pressurePacing.IsSummonPressureBreakActive;
        public bool IsSummonFollowupWindowActive => pressurePacing.IsSummonFollowupWindowActive;
        public float PressureReliefRemainingSeconds => pressurePacing.CloseThreatReliefRemainingSeconds;
        public float SummonPressureBreakRemainingSeconds => pressurePacing.SummonPressureBreakRemainingSeconds;
        public float SummonFollowupWindowRemainingSeconds => pressurePacing.SummonFollowupWindowRemainingSeconds;
        public float SummonFollowupEnergyPulse => lastGrantedSummonFollowupEnergyPulse > 0f
            ? lastGrantedSummonFollowupEnergyPulse
            : summonFollowupEnergyPulse;
        public bool RequireSkill1FollowupHitToClear => requireSkill1FollowupHitToClear;
        public int PressureBlocksAfterCloseThreatDefeated => CountPressureBlocksAfterCloseThreatDefeated();
        public int HighestSkillTier => highestSkillTier;
        public int HighestSummonTier => highestSummonTier;
        public int HighestSummonPressureTier => highestSummonPressureTier;
        public int HighestSummonFollowupSkillTier => highestSummonFollowupSkillTier;
        public int HighestSkill1FollowupHitTier => highestSkill1FollowupHitTier;
        public float Skill1FollowupDamage => skill1FollowupDamage;
        public int LastSummonPressureBreakTier => lastSummonPressureBreakTier;
        public float LastSummonPressureBreakDuration => lastSummonPressureBreakDuration;
        public float LastSummonFollowupWindowDuration => lastSummonFollowupWindowDuration;
        public ReviewPhase CurrentPhase
        {
            get
            {
                return state switch
                {
                    PocketState.Cleared => ReviewPhase.Cleared,
                    PocketState.Failed => ReviewPhase.Failed,
                    _ when pressurePacing.IsSummonPressureBreakActive && pressurePacing.IsSummonFollowupWindowActive => ReviewPhase.SummonFollowup,
                    _ when pressurePacing.IsSummonPressureBreakActive => ReviewPhase.PressureBreak,
                    _ when closeThreatDefeated => ReviewPhase.SummonBlock,
                    _ => ReviewPhase.ThreatDefense
                };
            }
        }
        public string ObjectiveCue
        {
            get
            {
                if (pressurePacing.IsSummonPressureBreakActive)
                {
                    if (skill1FollowupHitConfirmed)
                    {
                        return "Follow-up Skill1 hit confirmed";
                    }

                    if (usedSkill1DuringSummonFollowup)
                    {
                        if (bossBlockedSkill1Followup)
                        {
                            return "Boss screen blocked follow-up Skill1";
                        }

                        return "Follow-up Skill1 fired";
                    }

                    return pressurePacing.IsSummonFollowupWindowActive
                        ? ResolveFollowupReadyCue()
                        : requireSkill1FollowupHitToClear
                            ? "Follow-up missed; boss pressure returning"
                            : "Boss pressure is broken briefly";
                }

                if (closeThreatDefeated
                    && blockedBossPressureWithSummon
                    && requireSkill1FollowupHitToClear
                    && !skill1FollowupHitConfirmed)
                {
                    return energyLadder != null && !energyLadder.CanSpend
                        ? "Regain EN, then block boss fire again"
                        : bossBlockedSkill1Followup
                            ? "Boss screen blocked Skill1; block boss fire again"
                            : "Follow-up missed; block boss fire again";
                }

                if (closeThreatDefeated)
                {
                    return energyLadder != null && !energyLadder.CanSpend
                        ? "Advance for EN and block boss fire with SummonSlot1"
                        : "Block boss fire with SummonSlot1";
                }

                return energyLadder != null && !energyLadder.CanSpend
                    ? "Advance for EN, then defeat close threat"
                    : "Defeat close threat and prepare SummonSlot1";
            }
        }

        public void Configure(
            CombatHealth newPlayerHealth,
            CombatHealth newCloseThreatHealth,
            CombatHealth newBossHealth,
            SummonEnergyLadder newEnergyLadder,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            BossBarrageEmitter newBossBarrageEmitter,
            GameObject newClearMarker,
            GameObject newFailMarker,
            BossPressureCostLadder newBossPressureCostLadder = null,
            BossPressureActionDirector newBossPressureActionDirector = null)
        {
            playerHealth = newPlayerHealth;
            closeThreatHealth = newCloseThreatHealth;
            bossHealth = newBossHealth;
            energyLadder = newEnergyLadder;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            bossPressureCostLadder = newBossPressureCostLadder;
            bossPressureActionDirector = newBossPressureActionDirector;
            clearMarker = newClearMarker;
            failMarker = newFailMarker;
            ResetPocket();
            SubscribeBossHealth();
        }

        public void ResetPocket()
        {
            state = PocketState.Running;
            usedSkill1 = false;
            usedSummonSlot1 = false;
            closeThreatDefeated = false;
            blockedBossPressureWithSummon = false;
            grantedSummonFollowupEnergy = false;
            usedSkill1DuringSummonFollowup = false;
            skill1FollowupHitConfirmed = false;
            skill1FollowupDamage = 0f;
            pressureBlocksAtCloseThreatDefeat = 0;
            pressureBlocksConsumedBySummonBreak = 0;
            bossPressureBlocksAtSummonBreakStart = GetBossPressureScreenBlockCount();
            bossPressureBlocksConsumedDuringFollowup = 0;
            observedSkillUseCount = GetSkillUseCount();
            observedSummonUseCount = GetSummonUseCount();
            skillUsesAtSummonBreakStart = observedSkillUseCount;
            pressurePacing.Reset();
            followupMissedNotified = false;
            bossBlockedSkill1Followup = false;
            highestSkillTier = 0;
            highestSummonTier = 0;
            highestSummonPressureTier = 0;
            highestSummonFollowupSkillTier = 0;
            highestSkill1FollowupHitTier = 0;
            lastSummonPressureBreakTier = 0;
            lastSummonPressureBreakDuration = 0f;
            lastSummonFollowupWindowDuration = 0f;
            lastGrantedSummonFollowupEnergyPulse = 0f;
            SetBarrageEnabled(true);
            SetEnergyGainEnabled(true);
            SetBossPressureCostGainEnabled(true);
            SetBossPressureActionsEnabled(true);
            SetMarkers();
        }

        private void OnEnable()
        {
            ResetPocket();
            SubscribeBossHealth();
        }

        private void OnDisable()
        {
            UnsubscribeBossHealth();
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
            CaptureBossBlockedFollowup();
            UpdatePressurePacing(deltaTime);

            if (CanClearPocket())
            {
                ClearPocket();
            }
        }

        private void CaptureActionUse()
        {
            int currentSkillUseCount = GetSkillUseCount();
            if (skill1Action != null && currentSkillUseCount > observedSkillUseCount)
            {
                usedSkill1 = true;
                highestSkillTier = Mathf.Max(highestSkillTier, skill1Action.LastSpentTier);
                if (pressurePacing.IsSummonFollowupWindowActive
                    && currentSkillUseCount > skillUsesAtSummonBreakStart)
                {
                    usedSkill1DuringSummonFollowup = true;
                    highestSummonFollowupSkillTier = Mathf.Max(
                        highestSummonFollowupSkillTier,
                        skill1Action.LastSpentTier);
                }

                observedSkillUseCount = currentSkillUseCount;
            }

            int currentSummonUseCount = GetSummonUseCount();
            if (summonSlot1Action != null && currentSummonUseCount > observedSummonUseCount)
            {
                usedSummonSlot1 = true;
                highestSummonTier = Mathf.Max(highestSummonTier, summonSlot1Action.LastSpentTier);
                observedSummonUseCount = currentSummonUseCount;
            }

            int pressureBlocksAfterCloseThreatDefeated = CountPressureBlocksAfterCloseThreatDefeated();
            if (pressureBlocksAfterCloseThreatDefeated > pressureBlocksConsumedBySummonBreak)
            {
                pressureBlocksConsumedBySummonBreak = pressureBlocksAfterCloseThreatDefeated;
                if (!pressurePacing.IsSummonPressureBreakActive)
                {
                    StartSummonPressureBreak(summonSlot1Action.LastPressureScreenInterceptTier);
                }

                blockedBossPressureWithSummon = true;
                highestSummonPressureTier = Mathf.Max(
                    highestSummonPressureTier,
                    summonSlot1Action.LastPressureScreenInterceptTier);
            }
        }

        private bool CanClearPocket()
        {
            if (!closeThreatDefeated
                || !usedSummonSlot1
                || !blockedBossPressureWithSummon
                || pressurePacing.IsSummonPressureBreakActive)
            {
                return false;
            }

            return !requireSkill1FollowupHitToClear || skill1FollowupHitConfirmed;
        }

        private void OnBossDamaged(DamageInfo damageInfo)
        {
            if (state != PocketState.Running
                || !pressurePacing.IsSummonFollowupWindowActive
                || damageInfo.SourceTeam != DamageTeam.Player
                || GetSkillUseCount() <= skillUsesAtSummonBreakStart)
            {
                return;
            }

            usedSkill1DuringSummonFollowup = true;
            bool wasHitConfirmed = skill1FollowupHitConfirmed;
            skill1FollowupHitConfirmed = true;
            skill1FollowupDamage += damageInfo.Amount;
            int spentTier = skill1Action != null ? skill1Action.LastSpentTier : 0;
            highestSummonFollowupSkillTier = Mathf.Max(highestSummonFollowupSkillTier, spentTier);
            highestSkill1FollowupHitTier = Mathf.Max(highestSkill1FollowupHitTier, spentTier);
            if (!wasHitConfirmed)
            {
                SummonFollowupHitConfirmed?.Invoke(spentTier, damageInfo.Amount);
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
            SummonBlockOpportunityOpened?.Invoke();
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

        private void StartSummonPressureBreak(int pressureTier)
        {
            int resolvedTier = Mathf.Clamp(pressureTier, 1, 3);
            float pressureBreakSeconds = ResolveSummonPressureBreakSeconds(resolvedTier);
            float followupWindowSeconds = ResolveSummonFollowupWindowSeconds(resolvedTier);
            float followupEnergyPulse = ResolveSummonFollowupEnergyPulse(resolvedTier);
            skillUsesAtSummonBreakStart = GetSkillUseCount();
            grantedSummonFollowupEnergy = false;
            usedSkill1DuringSummonFollowup = false;
            skill1FollowupHitConfirmed = false;
            bossBlockedSkill1Followup = false;
            skill1FollowupDamage = 0f;
            followupMissedNotified = false;
            bossPressureBlocksAtSummonBreakStart = GetBossPressureScreenBlockCount();
            bossPressureBlocksConsumedDuringFollowup = 0;
            lastSummonPressureBreakTier = resolvedTier;
            lastSummonPressureBreakDuration = pressureBreakSeconds;
            lastSummonFollowupWindowDuration = followupWindowSeconds;
            pressurePacing.StartSummonPressureBreak(
                pressureBreakSeconds,
                followupWindowSeconds);
            GrantSummonFollowupEnergyPulse(followupEnergyPulse);
            ApplyRunningBarragePacing();
            SummonFollowupWindowOpened?.Invoke(resolvedTier);
        }

        private void UpdatePressurePacing(float deltaTime)
        {
            bool wasFollowupWindowActive = pressurePacing.IsSummonFollowupWindowActive;
            pressurePacing.Tick(deltaTime);
            CaptureFollowupMiss(wasFollowupWindowActive);
            ApplyRunningBarragePacing();
        }

        private void CaptureFollowupMiss(bool wasFollowupWindowActive)
        {
            if (!wasFollowupWindowActive
                || pressurePacing.IsSummonFollowupWindowActive
                || !requireSkill1FollowupHitToClear
                || skill1FollowupHitConfirmed
                || followupMissedNotified)
            {
                return;
            }

            followupMissedNotified = true;
            SummonFollowupMissed?.Invoke();
        }

        private void CaptureBossBlockedFollowup()
        {
            if (!pressurePacing.IsSummonFollowupWindowActive
                || !usedSkill1DuringSummonFollowup
                || skill1FollowupHitConfirmed)
            {
                return;
            }

            int blocksAfterWindowStart = Mathf.Max(
                0,
                GetBossPressureScreenBlockCount() - bossPressureBlocksAtSummonBreakStart);
            if (blocksAfterWindowStart <= bossPressureBlocksConsumedDuringFollowup)
            {
                return;
            }

            bossPressureBlocksConsumedDuringFollowup = blocksAfterWindowStart;
            bossBlockedSkill1Followup = true;
        }

        private void ClearPocket()
        {
            state = PocketState.Cleared;
            ClearPressurePacing();
            SetBarrageEnabled(!stopBarrageOnClear);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetBossPressureCostGainEnabled(!stopBossPressureCostOnEnd);
            SetBossPressureActionsEnabled(!stopBossPressureActionsOnEnd);
            SetMarkers();
        }

        private void FailPocket()
        {
            state = PocketState.Failed;
            ClearPressurePacing();
            SetBarrageEnabled(!stopBarrageOnFail);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetBossPressureCostGainEnabled(!stopBossPressureCostOnEnd);
            SetBossPressureActionsEnabled(!stopBossPressureActionsOnEnd);
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

        private void GrantSummonFollowupEnergyPulse(float energyAmount)
        {
            if (grantedSummonFollowupEnergy
                || energyLadder == null
                || energyAmount <= 0f)
            {
                return;
            }

            energyLadder.GrantCurrentTierEnergy(energyAmount);
            grantedSummonFollowupEnergy = true;
            lastGrantedSummonFollowupEnergyPulse = energyAmount;
        }

        private int GetSkillUseCount()
        {
            return skill1Action != null ? skill1Action.TotalUseCount : 0;
        }

        private int GetSummonUseCount()
        {
            return summonSlot1Action != null ? summonSlot1Action.TotalUseCount : 0;
        }

        private int GetBossPressureScreenBlockCount()
        {
            BossSummonPressureAction pressureAction = bossPressureActionDirector != null
                ? bossPressureActionDirector.SummonPressureAction
                : null;
            return pressureAction != null ? pressureAction.TotalPressureScreenInterceptCount : 0;
        }

        private string ResolveFollowupReadyCue()
        {
            if (energyLadder == null || !energyLadder.CanSpend)
            {
                return "Hold lane and take EN for the follow-up";
            }

            return $"Use Skill1 LV{energyLadder.AvailableTier} during summon follow-up";
        }

        private float ResolveSummonPressureBreakSeconds(int tier)
        {
            return tier switch
            {
                3 => summonPressureBreakReliefSeconds + summonPressureBreakTierThreeBonusSeconds,
                2 => summonPressureBreakReliefSeconds + summonPressureBreakTierTwoBonusSeconds,
                _ => summonPressureBreakReliefSeconds
            };
        }

        private float ResolveSummonFollowupWindowSeconds(int tier)
        {
            return tier switch
            {
                3 => summonFollowupWindowSeconds + summonFollowupWindowTierThreeBonusSeconds,
                2 => summonFollowupWindowSeconds + summonFollowupWindowTierTwoBonusSeconds,
                _ => summonFollowupWindowSeconds
            };
        }

        private float ResolveSummonFollowupEnergyPulse(int tier)
        {
            return tier switch
            {
                3 => summonFollowupEnergyPulseTierThree,
                2 => summonFollowupEnergyPulseTierTwo,
                _ => summonFollowupEnergyPulse
            };
        }

        private void SubscribeBossHealth()
        {
            if (subscribedBossHealth == bossHealth)
            {
                return;
            }

            UnsubscribeBossHealth();
            if (bossHealth == null)
            {
                return;
            }

            subscribedBossHealth = bossHealth;
            subscribedBossHealth.Damaged += OnBossDamaged;
        }

        private void UnsubscribeBossHealth()
        {
            if (subscribedBossHealth == null)
            {
                return;
            }

            subscribedBossHealth.Damaged -= OnBossDamaged;
            subscribedBossHealth = null;
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

        private void SetBossPressureCostGainEnabled(bool enabled)
        {
            if (bossPressureCostLadder != null)
            {
                bossPressureCostLadder.SetGainEnabled(enabled);
            }
        }

        private void SetBossPressureActionsEnabled(bool enabled)
        {
            if (bossPressureActionDirector != null)
            {
                bossPressureActionDirector.SetActionsEnabled(enabled);
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
