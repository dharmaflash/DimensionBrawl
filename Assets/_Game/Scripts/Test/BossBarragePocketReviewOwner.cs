using System;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Test
{
    // Review-only orchestration for the boss barrage lane slice; production encounter flow should use a separate owner.
    public sealed class BossBarragePocketReviewOwner : MonoBehaviour
    {
        public enum ReviewPhase
        {
            ThreatDefense,
            SummonBlock,
            SummonFollowup,
            PressureBreak,
            CounterWave,
            Cleared,
            Failed
        }

        public enum RouteFailureReason
        {
            None,
            PlayerDown,
            RouteStabilityCollapsed
        }

        public enum RouteStabilityBand
        {
            Stable,
            Unstable,
            Critical
        }

        public enum CounterWaveSource
        {
            None,
            FollowupMissed,
            BossScreenBlock,
            EnemyFrontlineBody,
            BossSummonRelease
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
        [SerializeField] private BossBasicFireEmitter bossBasicFireEmitter;
        [SerializeField] private BossPressureCostLadder bossPressureCostLadder;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private bool stopBarrageOnClear = true;
        [SerializeField] private bool stopBarrageOnFail = true;
        [SerializeField] private bool stopBossPressureCostOnEnd = true;
        [SerializeField] private bool stopBossPressureActionsOnEnd = true;

        [Header("Summon Opportunity")]
        [SerializeField] private SummonOpportunityWindowProfile summonPressureBlockOpportunity;

        [Header("Inline Opportunity Defaults")]
        [SerializeField, Min(0f)] private float closeThreatDefeatPressureReliefSeconds = 1.35f;
        [SerializeField, Min(0f)] private float summonPressureBreakReliefSeconds = 3f;
        [SerializeField, Min(0f)] private float summonFollowupWindowSeconds = 2.1f;
        [SerializeField, Min(0f)] private float summonFollowupEnergyPulse = 125f;
        [SerializeField, Min(0f)] private float summonPressureBreakTierTwoBonusSeconds = 0.35f;
        [SerializeField, Min(0f)] private float summonPressureBreakTierThreeBonusSeconds = 0.6f;
        [SerializeField, Min(0f)] private float summonFollowupWindowTierTwoBonusSeconds = 0.35f;
        [SerializeField, Min(0f)] private float summonFollowupWindowTierThreeBonusSeconds = 0.75f;
        [SerializeField, Min(0f)] private float summonFollowupEnergyPulseTierTwo = 185f;
        [SerializeField, Min(0f)] private float summonFollowupEnergyPulseTierThree = 240f;

        [Header("Follow-up Result")]
        [SerializeField] private bool requireSkill1FollowupHitToClear = true;

        [Header("Frontline Stage Review")]
        [SerializeField] private FrontlineWaveStageProfile stageProfile;

        [Header("Result Pace")]
        [SerializeField, Min(0f)] private float skill1FollowupClearDelaySeconds = 0.75f;

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
        private float skill1FollowupClearTimer;
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
        private float elapsedSeconds;
        private float resultElapsedSeconds;
        private float routeStability01 = 1f;
        private RouteFailureReason failureReason;
        private CombatHealth subscribedBossHealth;
        private bool followupMissedNotified;
        private bool bossBlockedSkill1Followup;
        private bool counterWaveObserved;
        private bool counterWaveStabilized;
        private bool counterWaveFinalWindowOpened;
        private CounterWaveSource counterWaveSource;
        private float lastCounterWaveEntryPenalty;
        private float lastCounterWaveStabilityBonus;
        private float lastCounterWaveFinalWindowDuration;
        private int bossPressureSummonReleasesAtReset;
        private int announcedStageBeatIndex;
        private RouteStabilityBand announcedRouteStabilityBand;

        public event Action<int> SummonFollowupWindowOpened;
        public event Action<int, float> SummonFollowupHitConfirmed;
        public event Action SummonFollowupMissed;
        public event Action SummonBlockOpportunityOpened;
        public event Action<CounterWaveSource> CounterWaveObserved;
        public event Action CounterWaveStabilized;
        public event Action PocketCleared;
        public event Action PocketFailed;
        public event Action<int> StageBeatChanged;
        public event Action<RouteStabilityBand, float> RouteStabilityBandChanged;

        public bool IsRunning => state == PocketState.Running;
        public bool IsCleared => state == PocketState.Cleared;
        public bool IsFailed => state == PocketState.Failed;
        public RouteFailureReason FailureReason => failureReason;
        public bool FailedFromRouteStabilityCollapse => failureReason == RouteFailureReason.RouteStabilityCollapsed;
        public bool UsedSkill1 => usedSkill1;
        public bool UsedSummonSlot1 => usedSummonSlot1;
        public bool CloseThreatDefeated => closeThreatDefeated;
        public bool BlockedBossPressureWithSummon => blockedBossPressureWithSummon;
        public bool GrantedSummonFollowupEnergy => grantedSummonFollowupEnergy;
        public bool UsedSkill1DuringSummonFollowup => usedSkill1DuringSummonFollowup;
        public bool Skill1FollowupHitConfirmed => skill1FollowupHitConfirmed;
        public bool IsCloseProbeCompletionRecorded => closeThreatDefeated;
        public bool IsSummonRouteCompletionRecorded => usedSummonSlot1 && blockedBossPressureWithSummon;
        public bool IsFollowupCompletionRecorded => skill1FollowupHitConfirmed;
        public bool IsCounterWaveCompletionRecorded => counterWaveObserved;
        public bool IsCounterWaveStabilized => counterWaveStabilized;
        public bool IsCounterWaveFinalWindowOpened => counterWaveFinalWindowOpened;
        public CounterWaveSource CounterWaveObservedSource => counterWaveSource;
        public string CounterWaveSourceReadout => ResolveCounterWaveSourceReadout();
        public string CounterWaveRecordState => ResolveCounterWaveRecordState();
        public string CounterWaveAnswerState => ResolveCounterWaveAnswerState();
        public string CounterWaveAnswerReadout => ResolveCounterWaveAnswerReadout();
        public float LastCounterWaveEntryPenalty => lastCounterWaveEntryPenalty;
        public float LastCounterWaveStabilityBonus => lastCounterWaveStabilityBonus;
        public string CounterWaveFinalWindowState => ResolveCounterWaveFinalWindowState();
        public string CounterWaveFinalWindowReadout => ResolveCounterWaveFinalWindowReadout();
        public float LastCounterWaveFinalWindowDuration => lastCounterWaveFinalWindowDuration;
        public string RouteDecisionState => ResolveRouteDecisionState();
        public string RouteDecisionReadout => ResolveRouteDecisionReadout();
        public string CompletionRecordReadout => ResolveCompletionRecordReadout();
        public bool IsSkill1FollowupClearCountdownActive => state == PocketState.Running
            && skill1FollowupHitConfirmed
            && skill1FollowupClearTimer > 0f;
        public bool BossBlockedSkill1Followup => bossBlockedSkill1Followup;
        public int BossPressureBlocksDuringSummonFollowup => bossPressureBlocksConsumedDuringFollowup;
        public SummonOpportunityWindowProfile SummonPressureBlockOpportunity => summonPressureBlockOpportunity;
        public bool HasSummonPressureBlockOpportunity => summonPressureBlockOpportunity != null;
        public bool IsPressureReliefActive => pressurePacing.IsCloseThreatReliefActive;
        public bool IsSummonBlockOpportunityCueActive => state == PocketState.Running
            && closeThreatDefeated
            && !blockedBossPressureWithSummon
            && pressurePacing.IsCloseThreatReliefActive;
        public bool IsAwaitingSummonPressureBlock => state == PocketState.Running
            && closeThreatDefeated
            && !blockedBossPressureWithSummon
            && !pressurePacing.IsCloseThreatReliefActive
            && !pressurePacing.IsSummonPressureBreakActive;
        public bool IsSummonPressureBreakActive => pressurePacing.IsSummonPressureBreakActive;
        public bool IsSummonFollowupWindowActive => pressurePacing.IsSummonFollowupWindowActive;
        public float PressureReliefRemainingSeconds => pressurePacing.CloseThreatReliefRemainingSeconds;
        public float SummonBlockOpportunityRemainingSeconds => IsSummonBlockOpportunityCueActive
            ? pressurePacing.CloseThreatReliefRemainingSeconds
            : 0f;
        public float SummonPressureBreakRemainingSeconds => pressurePacing.SummonPressureBreakRemainingSeconds;
        public float SummonFollowupWindowRemainingSeconds => pressurePacing.SummonFollowupWindowRemainingSeconds;
        public float SummonFollowupEnergyPulse => lastGrantedSummonFollowupEnergyPulse > 0f
            ? lastGrantedSummonFollowupEnergyPulse
            : ResolveSummonFollowupEnergyPulse(1);
        public bool RequireSkill1FollowupHitToClear => requireSkill1FollowupHitToClear;
        public int PressureBlocksAfterCloseThreatDefeated => CountPressureBlocksAfterCloseThreatDefeated();
        public int HighestSkillTier => highestSkillTier;
        public int HighestSummonTier => highestSummonTier;
        public int HighestSummonPressureTier => highestSummonPressureTier;
        public int HighestSummonFollowupSkillTier => highestSummonFollowupSkillTier;
        public int HighestSkill1FollowupHitTier => highestSkill1FollowupHitTier;
        public float Skill1FollowupDamage => skill1FollowupDamage;
        public float Skill1FollowupClearRemainingSeconds => skill1FollowupClearTimer;
        public int LastSummonPressureBreakTier => lastSummonPressureBreakTier;
        public float LastSummonPressureBreakDuration => lastSummonPressureBreakDuration;
        public float LastSummonFollowupWindowDuration => lastSummonFollowupWindowDuration;
        public FrontlineWaveStageProfile StageProfile => stageProfile;
        public int ObjectiveStepCount => stageProfile != null ? stageProfile.ObjectiveStepCount : 3;
        public int CompletedObjectiveStepCount => ResolveCompletedObjectiveStepCount();
        public float ElapsedSeconds => elapsedSeconds;
        public float ResultElapsedSeconds => state == PocketState.Running ? elapsedSeconds : resultElapsedSeconds;
        public int CurrentStageBeatIndex => ResolveCurrentStageBeatIndex();
        public bool IsRouteStabilityActive => stageProfile != null;
        public float RouteStability01 => IsRouteStabilityActive ? Mathf.Clamp01(routeStability01) : 1f;
        public float RouteStabilityPercent => RouteStability01 * 100f;
        public RouteStabilityBand CurrentRouteStabilityBand => ResolveRouteStabilityBand(RouteStability01);
        public float CurrentRouteStabilityDrainPerSecond => IsRouteStabilityActive && state == PocketState.Running
            ? ResolveRouteStabilityDrainPerSecond()
            : 0f;
        public int ActiveAllyFrontlineProxyCount => ResolveActiveFrontlineProxyCount(playerSide: true);
        public int ActiveEnemyFrontlineProxyCount => ResolveActiveFrontlineProxyCount(playerSide: false);
        public float CurrentFrontlinePresenceDrainScale => ResolveFrontlinePresenceDrainScale();
        public string FrontlinePresenceReadout => ResolveFrontlinePresenceReadout();
        public int CurrentPressureSlotIndex => ResolveCurrentPressureSlotIndex();
        public string CurrentPressureSlotLabel => ResolveCurrentPressureSlotLabel();
        public float CurrentRoutePressureWeight => ResolveCurrentRoutePressureWeight();
        public ReviewPhase CurrentPhase
        {
            get
            {
                return state switch
                {
                    PocketState.Cleared => ReviewPhase.Cleared,
                    PocketState.Failed => ReviewPhase.Failed,
                    _ when IsSkill1FollowupClearCountdownActive => ReviewPhase.SummonFollowup,
                    _ when counterWaveObserved && !counterWaveStabilized => ReviewPhase.CounterWave,
                    _ when pressurePacing.IsSummonFollowupWindowActive => ReviewPhase.SummonFollowup,
                    _ when pressurePacing.IsSummonPressureBreakActive => ReviewPhase.PressureBreak,
                    _ when counterWaveObserved => ReviewPhase.CounterWave,
                    _ when closeThreatDefeated => ReviewPhase.SummonBlock,
                    _ => ReviewPhase.ThreatDefense
                };
            }
        }
        public string ObjectiveCue
        {
            get
            {
                if (state == PocketState.Cleared)
                {
                    return ResolveStageText(
                        stageProfile != null ? stageProfile.ClearObjectiveCue : null,
                        "Frontline stabilized; summon route secured");
                }

                if (state == PocketState.Failed)
                {
                    return ResolveStageText(
                        stageProfile != null ? stageProfile.FailObjectiveCue : null,
                        "Player line collapsed before route stabilization");
                }

                if (counterWaveObserved && !counterWaveStabilized)
                {
                    return $"{ResolveCounterWaveCue()}: {ResolveObjectiveSummonAnswerLabel()}";
                }

                if (pressurePacing.IsSummonFollowupWindowActive || usedSkill1DuringSummonFollowup)
                {
                    if (skill1FollowupHitConfirmed)
                    {
                        return ResolveStageText(
                            stageProfile != null ? stageProfile.FollowupHitCue : null,
                            "Summon route analyzed; Skill1 hit confirmed");
                    }

                    if (usedSkill1DuringSummonFollowup)
                    {
                        if (bossBlockedSkill1Followup)
                        {
                            return ResolveStageText(
                                stageProfile != null ? stageProfile.FollowupBlockedCue : null,
                                "Boss screen absorbed the follow-up; rebuild the summon answer");
                        }

                        return ResolveStageText(
                            stageProfile != null ? stageProfile.FollowupFiredCue : null,
                            "Skill1 committed into the summon opening");
                    }

                    return ResolveFollowupReadyCue();
                }

                if (pressurePacing.IsSummonPressureBreakActive)
                {
                    return requireSkill1FollowupHitToClear
                        ? ResolveStageText(
                            stageProfile != null ? stageProfile.FollowupMissedCue : null,
                            "Follow-up window missed; boss pressure is returning")
                        : ResolveStageText(
                            stageProfile != null ? stageProfile.PressureBreakCue : null,
                            "Boss curtain suppressed briefly; read the follow-up window");
                }

                if (closeThreatDefeated
                    && blockedBossPressureWithSummon
                    && requireSkill1FollowupHitToClear
                    && !skill1FollowupHitConfirmed)
                {
                    if (counterWaveObserved)
                    {
                        return counterWaveStabilized
                            ? $"{ResolveCounterWaveStabilizedCue()}: {ResolveObjectiveSummonAnswerLabel()}"
                            : $"{ResolveCounterWaveCue()}: {ResolveObjectiveSummonAnswerLabel()}";
                    }

                    return energyLadder != null && !energyLadder.CanSpend
                        ? $"{ResolveStageText(stageProfile != null ? stageProfile.SummonChargeCue : null, "Regain EN, then block boss curtain")}: {ResolveObjectiveSummonAnswerLabel()}"
                        : bossBlockedSkill1Followup
                            ? $"{ResolveStageText(stageProfile != null ? stageProfile.FollowupBlockedCue : null, "Boss screen blocked Skill1; rebuild the summon answer")}: {ResolveObjectiveSummonAnswerLabel()}"
                            : $"{ResolveStageText(stageProfile != null ? stageProfile.FollowupMissedCue : null, "Follow-up missed; block boss fire again")}: {ResolveObjectiveSummonAnswerLabel()}";
                }

                if (closeThreatDefeated)
                {
                    if (IsSummonBlockOpportunityCueActive)
                    {
                        return ResolveSummonBlockOpportunityCue();
                    }

                    return energyLadder != null && !energyLadder.CanSpend
                        ? $"{ResolveStageText(stageProfile != null ? stageProfile.SummonChargeCue : null, "Build EN for SummonSlot1; boss curtain is returning")}: {ResolveObjectiveSummonAnswerLabel()}"
                        : $"{ResolveStageText(stageProfile != null ? stageProfile.SummonReadyCue : null, "Send SummonSlot1 across the line to block boss curtain")}: {ResolveObjectiveSummonAnswerLabel()}";
                }

                return energyLadder != null && !energyLadder.CanSpend
                    ? ResolveStageText(
                        stageProfile != null ? stageProfile.PreThreatChargeCue : null,
                        "Build EN while holding the player line, then stop the close probe")
                    : $"{ResolveStageText(stageProfile != null ? stageProfile.PreThreatReadyCue : null, "Stop the close probe and prepare the summon answer")}: {ResolveObjectiveSummonAnswerLabel()}";
            }
        }

#if UNITY_EDITOR
        public void AssignStageProfileForReview(FrontlineWaveStageProfile newStageProfile)
        {
            stageProfile = newStageProfile;
        }
#endif

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
            BossPressureActionDirector newBossPressureActionDirector = null,
            BossBasicFireEmitter newBossBasicFireEmitter = null)
        {
            playerHealth = newPlayerHealth;
            closeThreatHealth = newCloseThreatHealth;
            bossHealth = newBossHealth;
            energyLadder = newEnergyLadder;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            bossBarrageEmitter = newBossBarrageEmitter;
            bossBasicFireEmitter = newBossBasicFireEmitter;
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
            skill1FollowupClearTimer = 0f;
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
            counterWaveObserved = false;
            counterWaveStabilized = false;
            counterWaveFinalWindowOpened = false;
            counterWaveSource = CounterWaveSource.None;
            lastCounterWaveEntryPenalty = 0f;
            lastCounterWaveStabilityBonus = 0f;
            lastCounterWaveFinalWindowDuration = 0f;
            bossPressureSummonReleasesAtReset = GetBossPressureSummonReleaseCount();
            highestSkillTier = 0;
            highestSummonTier = 0;
            highestSummonPressureTier = 0;
            highestSummonFollowupSkillTier = 0;
            highestSkill1FollowupHitTier = 0;
            lastSummonPressureBreakTier = 0;
            lastSummonPressureBreakDuration = 0f;
            lastSummonFollowupWindowDuration = 0f;
            lastGrantedSummonFollowupEnergyPulse = 0f;
            elapsedSeconds = 0f;
            resultElapsedSeconds = 0f;
            routeStability01 = ResolveRouteStabilityStart01();
            failureReason = RouteFailureReason.None;
            announcedStageBeatIndex = ResolveCurrentStageBeatIndex();
            announcedRouteStabilityBand = CurrentRouteStabilityBand;
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

            elapsedSeconds += Mathf.Max(0f, deltaTime);
            CaptureActionUse();
            if (playerHealth != null && !playerHealth.IsAlive)
            {
                FailPocket(RouteFailureReason.PlayerDown);
                PublishStageBeatChangeIfNeeded();
                return;
            }

            CaptureCloseThreatDefeat();
            CaptureBossBlockedFollowup();
            UpdatePressurePacing(deltaTime);
            CaptureCounterWavePressure();
            CaptureCounterWaveAnswer();
            TickSkill1FollowupClearTimer(deltaTime);
            TickRouteStability(deltaTime);
            if (IsRouteStabilityActive && routeStability01 <= 0f)
            {
                FailPocket(RouteFailureReason.RouteStabilityCollapsed);
                PublishStageBeatChangeIfNeeded();
                return;
            }

            if (CanClearPocket())
            {
                ClearPocket();
            }

            PublishStageBeatChangeIfNeeded();
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
                || !blockedBossPressureWithSummon)
            {
                return false;
            }

            if (!requireSkill1FollowupHitToClear)
            {
                return !pressurePacing.IsSummonPressureBreakActive;
            }

            return skill1FollowupHitConfirmed && skill1FollowupClearTimer <= 0f;
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
                AddRouteStability(ResolveFollowupHitRouteBonus01());
                skill1FollowupClearTimer = skill1FollowupClearDelaySeconds;
                pressurePacing.EndSummonFollowupWindow();
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
            AddRouteStability(ResolveCloseProbeDefeatRouteBonus01());
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
            pressurePacing.StartCloseThreatRelief(ResolveCloseThreatReliefSeconds());
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
            skill1FollowupClearTimer = 0f;
            followupMissedNotified = false;
            bossPressureBlocksAtSummonBreakStart = GetBossPressureScreenBlockCount();
            bossPressureBlocksConsumedDuringFollowup = 0;
            lastSummonPressureBreakTier = resolvedTier;
            lastSummonPressureBreakDuration = pressureBreakSeconds;
            lastSummonFollowupWindowDuration = followupWindowSeconds;
            pressurePacing.StartSummonPressureBreak(
                pressureBreakSeconds,
                followupWindowSeconds);
            AddRouteStability(ResolveSummonBlockRouteBonus01(resolvedTier));
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

        private void TickSkill1FollowupClearTimer(float deltaTime)
        {
            if (!skill1FollowupHitConfirmed || skill1FollowupClearTimer <= 0f)
            {
                return;
            }

            skill1FollowupClearTimer = Mathf.Max(0f, skill1FollowupClearTimer - Mathf.Max(0f, deltaTime));
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

            NotifySummonFollowupMissedOnce();
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
            pressurePacing.EndSummonFollowupWindow();
            NotifySummonFollowupMissedOnce();
        }

        private void NotifySummonFollowupMissedOnce()
        {
            if (followupMissedNotified)
            {
                return;
            }

            followupMissedNotified = true;
            ObserveCounterWave(CounterWaveSource.FollowupMissed);
            SummonFollowupMissed?.Invoke();
        }

        private void CaptureCounterWavePressure()
        {
            if (counterWaveObserved || !blockedBossPressureWithSummon)
            {
                return;
            }

            if (followupMissedNotified)
            {
                ObserveCounterWave(CounterWaveSource.FollowupMissed);
            }
            else if (bossBlockedSkill1Followup)
            {
                ObserveCounterWave(CounterWaveSource.BossScreenBlock);
            }
            else if (GetBossPressureSummonReleaseCount() > bossPressureSummonReleasesAtReset)
            {
                ObserveCounterWave(CounterWaveSource.BossSummonRelease);
            }
            else if (ActiveEnemyFrontlineProxyCount > 0)
            {
                ObserveCounterWave(CounterWaveSource.EnemyFrontlineBody);
            }
        }

        private void ObserveCounterWave(CounterWaveSource source)
        {
            bool wasObserved = counterWaveObserved;
            counterWaveObserved = true;
            counterWaveSource = source == CounterWaveSource.None ? CounterWaveSource.EnemyFrontlineBody : source;
            if (!wasObserved)
            {
                ApplyCounterWaveEntryRoutePenalty();
                CounterWaveObserved?.Invoke(counterWaveSource);
            }
        }

        private void ApplyCounterWaveEntryRoutePenalty()
        {
            lastCounterWaveEntryPenalty = ResolveCounterWaveEntryRoutePenalty01();
            RemoveRouteStability(lastCounterWaveEntryPenalty);
        }

        private void CaptureCounterWaveAnswer()
        {
            if (!counterWaveObserved || counterWaveStabilized || ActiveAllyFrontlineProxyCount <= 0)
            {
                return;
            }

            counterWaveStabilized = true;
            lastCounterWaveStabilityBonus = ResolveCounterWaveStabilizeRouteBonus01();
            AddRouteStability(lastCounterWaveStabilityBonus);
            CounterWaveStabilized?.Invoke();
            OpenCounterWaveFinalWindow();
        }

        private void OpenCounterWaveFinalWindow()
        {
            if (counterWaveFinalWindowOpened)
            {
                return;
            }

            int resolvedTier = Mathf.Clamp(ResolveObjectiveSummonTier(), 1, 3);
            float followupWindowSeconds = ResolveSummonFollowupWindowSeconds(resolvedTier);
            float followupEnergyPulse = ResolveSummonFollowupEnergyPulse(resolvedTier);
            skillUsesAtSummonBreakStart = GetSkillUseCount();
            grantedSummonFollowupEnergy = false;
            usedSkill1DuringSummonFollowup = false;
            skill1FollowupHitConfirmed = false;
            bossBlockedSkill1Followup = false;
            skill1FollowupDamage = 0f;
            skill1FollowupClearTimer = 0f;
            followupMissedNotified = false;
            bossPressureBlocksAtSummonBreakStart = GetBossPressureScreenBlockCount();
            bossPressureBlocksConsumedDuringFollowup = 0;
            lastSummonPressureBreakTier = resolvedTier;
            lastSummonFollowupWindowDuration = followupWindowSeconds;
            counterWaveFinalWindowOpened = true;
            lastCounterWaveFinalWindowDuration = followupWindowSeconds;
            pressurePacing.StartSummonFollowupWindow(followupWindowSeconds);
            GrantSummonFollowupEnergyPulse(followupEnergyPulse);
            ApplyRunningBarragePacing();
            SummonFollowupWindowOpened?.Invoke(resolvedTier);
        }

        private void ClearPocket()
        {
            resultElapsedSeconds = elapsedSeconds;
            state = PocketState.Cleared;
            failureReason = RouteFailureReason.None;
            routeStability01 = 1f;
            ClearPressurePacing();
            DismissActiveSummonPressureScreens();
            SetBarrageEnabled(!stopBarrageOnClear);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetBossPressureCostGainEnabled(!stopBossPressureCostOnEnd);
            SetBossPressureActionsEnabled(!stopBossPressureActionsOnEnd);
            SetMarkers();
            PocketCleared?.Invoke();
        }

        private void FailPocket(RouteFailureReason reason)
        {
            resultElapsedSeconds = elapsedSeconds;
            state = PocketState.Failed;
            failureReason = reason;
            ClearPressurePacing();
            DismissActiveSummonPressureScreens();
            SetBarrageEnabled(!stopBarrageOnFail);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetBossPressureCostGainEnabled(!stopBossPressureCostOnEnd);
            SetBossPressureActionsEnabled(!stopBossPressureActionsOnEnd);
            SetMarkers();
            PocketFailed?.Invoke();
        }

        private void DismissActiveSummonPressureScreens()
        {
            summonSlot1Action?.DismissActivePressureScreens();
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

        private void TickRouteStability(float deltaTime)
        {
            if (!IsRouteStabilityActive || state != PocketState.Running)
            {
                return;
            }

            float drain = ResolveRouteStabilityDrainPerSecond();
            if (drain <= 0f)
            {
                return;
            }

            routeStability01 = Mathf.Clamp01(routeStability01 - drain * Mathf.Max(0f, deltaTime));
            PublishRouteStabilityBandChangeIfNeeded();
        }

        private float ResolveRouteStabilityDrainPerSecond()
        {
            if (!closeThreatDefeated)
            {
                return stageProfile.CloseProbeRouteDrainPerSecond
                    * ResolveCurrentRoutePressureWeight()
                    * ResolveFrontlinePresenceDrainScale();
            }

            if (!blockedBossPressureWithSummon)
            {
                float reliefScale = pressurePacing.IsCloseThreatReliefActive ? 0.45f : 1f;
                return stageProfile.SummonAnswerRouteDrainPerSecond
                    * reliefScale
                    * ResolveCurrentRoutePressureWeight()
                    * ResolveFrontlinePresenceDrainScale();
            }

            if (requireSkill1FollowupHitToClear && !skill1FollowupHitConfirmed)
            {
                if (pressurePacing.IsSummonFollowupWindowActive)
                {
                    return 0f;
                }

                float pressureBreakScale = pressurePacing.IsSummonPressureBreakActive ? 0.35f : 1f;
                return stageProfile.CounterWaveRouteDrainPerSecond
                    * pressureBreakScale
                    * ResolveCurrentRoutePressureWeight()
                    * ResolveFrontlinePresenceDrainScale();
            }

            return 0f;
        }

        private void AddRouteStability(float amount01)
        {
            if (!IsRouteStabilityActive || amount01 <= 0f)
            {
                return;
            }

            routeStability01 = Mathf.Clamp01(routeStability01 + amount01);
            PublishRouteStabilityBandChangeIfNeeded();
        }

        private void RemoveRouteStability(float amount01)
        {
            if (!IsRouteStabilityActive || amount01 <= 0f)
            {
                return;
            }

            routeStability01 = Mathf.Clamp01(routeStability01 - amount01);
            PublishRouteStabilityBandChangeIfNeeded();
        }

        private void PublishRouteStabilityBandChangeIfNeeded()
        {
            if (!IsRouteStabilityActive)
            {
                return;
            }

            RouteStabilityBand currentBand = CurrentRouteStabilityBand;
            if (currentBand == announcedRouteStabilityBand)
            {
                return;
            }

            announcedRouteStabilityBand = currentBand;
            RouteStabilityBandChanged?.Invoke(currentBand, RouteStability01);
        }

        private static RouteStabilityBand ResolveRouteStabilityBand(float stability01)
        {
            float safeStability = Mathf.Clamp01(stability01);
            if (safeStability <= 0.2f)
            {
                return RouteStabilityBand.Critical;
            }

            if (safeStability <= 0.4f)
            {
                return RouteStabilityBand.Unstable;
            }

            return RouteStabilityBand.Stable;
        }

        private string ResolveFrontlinePresenceReadout()
        {
            ResolveActiveFrontlineProxyCounts(out int allyCount, out int enemyCount);
            string state = allyCount > 0 && enemyCount > 0
                ? "contest"
                : allyCount > 0
                    ? "ally"
                    : enemyCount > 0
                        ? "enemy"
                        : "open";
            return $"frontline x{ResolveFrontlinePresenceDrainScale(allyCount, enemyCount):0.00} {state}";
        }

        private float ResolveFrontlinePresenceDrainScale()
        {
            ResolveActiveFrontlineProxyCounts(out int allyCount, out int enemyCount);
            return ResolveFrontlinePresenceDrainScale(allyCount, enemyCount);
        }

        private static float ResolveFrontlinePresenceDrainScale(int allyCount, int enemyCount)
        {
            if (allyCount > 0 && enemyCount > 0)
            {
                return 0.85f;
            }

            if (allyCount > 0)
            {
                return 0.70f;
            }

            if (enemyCount > 0)
            {
                return 1.20f;
            }

            return 1f;
        }

        private int ResolveActiveFrontlineProxyCount(bool playerSide)
        {
            ResolveActiveFrontlineProxyCounts(out int allyCount, out int enemyCount);
            return playerSide ? allyCount : enemyCount;
        }

        private static void ResolveActiveFrontlineProxyCounts(out int allyCount, out int enemyCount)
        {
            allyCount = 0;
            enemyCount = 0;
            int proxyCount = SummonFrontlineProxy.ActiveRegisteredProxyCount;
            for (int i = 0; i < proxyCount; i++)
            {
                if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy)
                    || proxy == null
                    || proxy.Health == null)
                {
                    continue;
                }

                if (CombatTeamUtility.IsPlayerSide(proxy.Health.Team))
                {
                    allyCount++;
                }
                else
                {
                    enemyCount++;
                }
            }
        }

        private float ResolveRouteStabilityStart01()
        {
            return stageProfile != null ? stageProfile.RouteStabilityStart01 : 1f;
        }

        private float ResolveCloseProbeDefeatRouteBonus01()
        {
            return stageProfile != null ? stageProfile.CloseProbeDefeatRouteBonus01 : 0f;
        }

        private float ResolveSummonBlockRouteBonus01(int tier)
        {
            float bonus = stageProfile != null ? stageProfile.SummonBlockRouteBonus01 : 0f;
            float tierScale = Mathf.Lerp(0.85f, 1.15f, Mathf.Clamp01((Mathf.Clamp(tier, 1, 3) - 1) / 2f));
            return bonus * tierScale;
        }

        private float ResolveFollowupHitRouteBonus01()
        {
            return stageProfile != null ? stageProfile.FollowupHitRouteBonus01 : 0f;
        }

        private float ResolveCounterWaveEntryRoutePenalty01()
        {
            return stageProfile != null ? stageProfile.CounterWaveEntryRoutePenalty01 : 0f;
        }

        private float ResolveCounterWaveStabilizeRouteBonus01()
        {
            return stageProfile != null ? stageProfile.CounterWaveStabilizeRouteBonus01 : 0f;
        }

        private int ResolveCurrentPressureSlotIndex()
        {
            if (stageProfile == null || stageProfile.PressureSlotCount <= 0)
            {
                return 0;
            }

            return Mathf.Clamp(ResolveCurrentStageBeatIndex(), 0, stageProfile.PressureSlotCount - 1);
        }

        private string ResolveCurrentPressureSlotLabel()
        {
            if (stageProfile == null || stageProfile.PressureSlotCount <= 0)
            {
                return "-";
            }

            var slot = stageProfile.GetPressureSlot(ResolveCurrentPressureSlotIndex());
            return string.IsNullOrWhiteSpace(slot.Label) ? slot.SlotId : slot.Label;
        }

        private float ResolveCurrentRoutePressureWeight()
        {
            if (stageProfile == null || stageProfile.PressureSlotCount <= 0)
            {
                return 1f;
            }

            float weight = stageProfile.GetPressureSlot(ResolveCurrentPressureSlotIndex()).RoutePressureWeight;
            return weight > 0f ? weight : 1f;
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

        private int GetBossPressureSummonReleaseCount()
        {
            BossSummonPressureAction pressureAction = bossPressureActionDirector != null
                ? bossPressureActionDirector.SummonPressureAction
                : null;
            return pressureAction != null ? pressureAction.TotalReleaseCount : 0;
        }

        private string ResolveFollowupReadyCue()
        {
            string summonTierLabel = ResolveSummonTierLabel(lastSummonPressureBreakTier);
            string cue = ResolveStageText(
                stageProfile != null ? stageProfile.FollowupReadyCue : null,
                "Confirm the summon opening with Skill1");
            if (energyLadder == null || !energyLadder.CanSpend)
            {
                return $"{cue}: hold lane and take EN for the {summonTierLabel} window";
            }

            return $"{cue}: Skill1 LV{energyLadder.AvailableTier} during {summonTierLabel} window";
        }

        private string ResolveCounterWaveCue()
        {
            return ResolveStageText(
                stageProfile != null ? stageProfile.CounterWaveCue : null,
                "Counter wave entered the line; hold frontline and answer with summon");
        }

        private string ResolveCounterWaveStabilizedCue()
        {
            return ResolveStageText(
                stageProfile != null ? stageProfile.CounterWaveStabilizedCue : null,
                "Counter wave held by summon; rebuild the route opening");
        }

        private string ResolveSummonBlockOpportunityCue()
        {
            bool shouldCharge = energyLadder != null && !energyLadder.CanSpend;
            string cue = shouldCharge
                ? summonPressureBlockOpportunity != null
                    ? summonPressureBlockOpportunity.ChargeCue
                    : "Forward EN now"
                : summonPressureBlockOpportunity != null
                    ? summonPressureBlockOpportunity.ReadyCue
                    : "Prepare SummonSlot1 block";
            string stageCue = ResolveStageText(
                stageProfile != null ? stageProfile.SummonOpportunityCue : null,
                cue);
            return $"{stageCue}: {ResolveObjectiveSummonTierLabel()}; boss curtain returns in {SummonBlockOpportunityRemainingSeconds:0.0}s";
        }

        private static string ResolveStageText(string profileText, string fallback)
        {
            return string.IsNullOrWhiteSpace(profileText) ? fallback : profileText;
        }

        private string ResolveObjectiveSummonTierLabel()
        {
            return ResolveSummonTierLabel(ResolveObjectiveSummonTier());
        }

        private string ResolveObjectiveSummonAnswerLabel()
        {
            return $"SummonSlot1 {ResolveObjectiveSummonTierLabel()}";
        }

        private int ResolveObjectiveSummonTier()
        {
            if (pressurePacing.IsSummonPressureBreakActive && lastSummonPressureBreakTier > 0)
            {
                return lastSummonPressureBreakTier;
            }

            if (blockedBossPressureWithSummon && highestSummonPressureTier > 0)
            {
                return highestSummonPressureTier;
            }

            if (summonSlot1Action != null && summonSlot1Action.LastSpentTier > 0 && usedSummonSlot1)
            {
                return summonSlot1Action.LastSpentTier;
            }

            if (energyLadder != null)
            {
                return energyLadder.CanSpend
                    ? energyLadder.AvailableTier
                    : energyLadder.ChargingTier;
            }

            return 1;
        }

        private string ResolveSummonTierLabel(int tier)
        {
            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            if (summonSlot1Action != null
                && summonSlot1Action.TryGetTierReadout(resolvedTier, out SummonSlotActionProfile.SummonTierReadout readout))
            {
                return readout.TierLabel;
            }

            return $"LV{resolvedTier}";
        }

        private float ResolveCloseThreatReliefSeconds()
        {
            return summonPressureBlockOpportunity != null
                ? summonPressureBlockOpportunity.OpportunityCueSeconds
                : closeThreatDefeatPressureReliefSeconds;
        }

        private float ResolveSummonPressureBreakSeconds(int tier)
        {
            if (summonPressureBlockOpportunity != null)
            {
                return summonPressureBlockOpportunity.ResolvePressureBreakSeconds(tier);
            }

            return tier switch
            {
                3 => summonPressureBreakReliefSeconds + summonPressureBreakTierThreeBonusSeconds,
                2 => summonPressureBreakReliefSeconds + summonPressureBreakTierTwoBonusSeconds,
                _ => summonPressureBreakReliefSeconds
            };
        }

        private float ResolveSummonFollowupWindowSeconds(int tier)
        {
            if (summonPressureBlockOpportunity != null)
            {
                return summonPressureBlockOpportunity.ResolveFollowupWindowSeconds(tier);
            }

            return tier switch
            {
                3 => summonFollowupWindowSeconds + summonFollowupWindowTierThreeBonusSeconds,
                2 => summonFollowupWindowSeconds + summonFollowupWindowTierTwoBonusSeconds,
                _ => summonFollowupWindowSeconds
            };
        }

        private float ResolveSummonFollowupEnergyPulse(int tier)
        {
            if (summonPressureBlockOpportunity != null)
            {
                return summonPressureBlockOpportunity.ResolveFollowupEnergyPulse(tier);
            }

            return tier switch
            {
                3 => summonFollowupEnergyPulseTierThree,
                2 => summonFollowupEnergyPulseTierTwo,
                _ => summonFollowupEnergyPulse
            };
        }

        private int ResolveCompletedObjectiveStepCount()
        {
            if (state == PocketState.Cleared)
            {
                return ObjectiveStepCount;
            }

            int completed = 0;
            if (closeThreatDefeated)
            {
                completed++;
            }

            if (blockedBossPressureWithSummon)
            {
                completed++;
            }

            if (requireSkill1FollowupHitToClear)
            {
                if (skill1FollowupHitConfirmed)
                {
                    completed++;
                }
            }
            else if (blockedBossPressureWithSummon && !pressurePacing.IsSummonPressureBreakActive)
            {
                completed++;
            }

            return Mathf.Clamp(completed, 0, ObjectiveStepCount);
        }

        private string ResolveCompletionRecordReadout()
        {
            return $"close:{ResolveRecordState(IsCloseProbeCompletionRecorded)} "
                + $"summon:{ResolveRecordState(IsSummonRouteCompletionRecorded)} "
                + $"followup:{ResolveRecordState(IsFollowupCompletionRecorded)} "
                + $"counter:{CounterWaveRecordState}({CounterWaveSourceReadout}) "
                + $"counter_answer:{CounterWaveAnswerState}({CounterWaveAnswerReadout}) "
                + $"counter_window:{CounterWaveFinalWindowState}({CounterWaveFinalWindowReadout}) "
                + $"decision:{RouteDecisionState}({RouteDecisionReadout})";
        }

        private static string ResolveRecordState(bool completed)
        {
            return completed ? "recorded" : "pending";
        }

        private string ResolveCounterWaveRecordState()
        {
            if (IsCounterWaveCompletionRecorded)
            {
                return "recorded";
            }

            return IsFollowupCompletionRecorded ? "avoided" : "pending";
        }

        private string ResolveCounterWaveSourceReadout()
        {
            return counterWaveSource switch
            {
                CounterWaveSource.FollowupMissed => "followup_miss",
                CounterWaveSource.BossScreenBlock => "boss_screen",
                CounterWaveSource.EnemyFrontlineBody => "enemy_body",
                CounterWaveSource.BossSummonRelease => "boss_summon",
                _ => "none"
            };
        }

        private string ResolveCounterWaveAnswerState()
        {
            if (counterWaveStabilized)
            {
                return "stabilized";
            }

            if (IsCounterWaveCompletionRecorded)
            {
                return "pending";
            }

            return IsFollowupCompletionRecorded ? "not_needed" : "pending";
        }

        private string ResolveCounterWaveAnswerReadout()
        {
            if (counterWaveStabilized)
            {
                return "ally_hold";
            }

            if (IsCounterWaveCompletionRecorded)
            {
                return "awaiting";
            }

            return IsFollowupCompletionRecorded ? "clean_followup" : "none";
        }

        private string ResolveCounterWaveFinalWindowState()
        {
            if (counterWaveFinalWindowOpened)
            {
                return "opened";
            }

            if (IsCounterWaveCompletionRecorded)
            {
                return "pending";
            }

            return IsFollowupCompletionRecorded ? "not_needed" : "pending";
        }

        private string ResolveCounterWaveFinalWindowReadout()
        {
            if (counterWaveFinalWindowOpened)
            {
                return "final_followup";
            }

            if (IsCounterWaveCompletionRecorded)
            {
                return "awaiting_answer";
            }

            return IsFollowupCompletionRecorded ? "clean_followup" : "none";
        }

        private string ResolveRouteDecisionState()
        {
            if (state == PocketState.Cleared)
            {
                return IsCounterRecoveryRoute() ? "recovery_clear" : "clean_clear";
            }

            if (state == PocketState.Failed)
            {
                return "failed";
            }

            if (counterWaveObserved)
            {
                return counterWaveStabilized ? "recovered" : "recovery_needed";
            }

            if (pressurePacing.IsSummonFollowupWindowActive || usedSkill1DuringSummonFollowup)
            {
                return "confirm";
            }

            if (blockedBossPressureWithSummon)
            {
                return "confirm";
            }

            if (closeThreatDefeated)
            {
                return IsAwaitingSummonPressureBlock ? "summon_now" : "prepare_summon";
            }

            return "build_route";
        }

        private string ResolveRouteDecisionReadout()
        {
            if (state == PocketState.Cleared)
            {
                return IsCounterRecoveryRoute() ? "counter_recovery" : "clean_followup";
            }

            if (state == PocketState.Failed)
            {
                return failureReason == RouteFailureReason.RouteStabilityCollapsed
                    ? "route_collapse"
                    : "player_down";
            }

            if (counterWaveObserved)
            {
                if (counterWaveStabilized)
                {
                    return counterWaveFinalWindowOpened ? "final_window" : "counter_held";
                }

                return "answer_counter";
            }

            if (pressurePacing.IsSummonFollowupWindowActive || usedSkill1DuringSummonFollowup)
            {
                return skill1FollowupHitConfirmed ? "hit_confirmed" : "followup_window";
            }

            if (blockedBossPressureWithSummon)
            {
                return bossBlockedSkill1Followup || followupMissedNotified
                    ? "rebuild_summon"
                    : "summon_opening";
            }

            if (closeThreatDefeated)
            {
                if (IsAwaitingSummonPressureBlock)
                {
                    return "boss_curtain";
                }

                return IsSummonBlockOpportunityCueActive ? "cue_window" : "build_en";
            }

            return "hold_line";
        }

        private bool IsCounterRecoveryRoute()
        {
            return counterWaveStabilized || counterWaveFinalWindowOpened;
        }

        private int ResolveCurrentStageBeatIndex()
        {
            if (state != PocketState.Running)
            {
                return 5;
            }

            if (skill1FollowupHitConfirmed)
            {
                return 5;
            }

            if (counterWaveObserved && !counterWaveStabilized)
            {
                return 4;
            }

            if (pressurePacing.IsSummonFollowupWindowActive || usedSkill1DuringSummonFollowup)
            {
                return 3;
            }

            if (blockedBossPressureWithSummon
                || pressurePacing.IsSummonPressureBreakActive
                || bossBlockedSkill1Followup
                || counterWaveObserved
                || followupMissedNotified)
            {
                return 4;
            }

            if (closeThreatDefeated)
            {
                return 2;
            }

            return elapsedSeconds <= 0.5f ? 0 : 1;
        }

        private void PublishStageBeatChangeIfNeeded()
        {
            int currentBeatIndex = ResolveCurrentStageBeatIndex();
            if (currentBeatIndex == announcedStageBeatIndex)
            {
                return;
            }

            announcedStageBeatIndex = currentBeatIndex;
            StageBeatChanged?.Invoke(currentBeatIndex);
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

            if (bossBasicFireEmitter != null)
            {
                bossBasicFireEmitter.SetFiringEnabled(enabled);
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

    }

}
