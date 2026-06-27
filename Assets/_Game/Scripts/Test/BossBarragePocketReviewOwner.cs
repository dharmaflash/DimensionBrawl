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

        public enum RouteResultKind
        {
            None,
            CleanFollowupClear,
            CounterRecoveryClear,
            PressureSuppressionClear,
            PlayerDownFail,
            PressureControlFail
        }

        public readonly struct RouteResultRecord
        {
            public RouteResultRecord(
                bool isCommitted,
                RouteResultKind resultKind,
                bool isClear,
                RouteFailureReason failureReason,
                CounterWaveSource counterWaveSource,
                float elapsedSeconds,
                float routeStability01,
                int completedObjectiveStepCount,
                int objectiveStepCount,
                string completionReadout,
                string proofReadout,
                string decisionState,
                string decisionReadout,
                string title,
                string summary,
                string routeLabel,
                string rewardHook,
                string nextObjective,
                string resultTokenId,
                string nextStateHookId)
            {
                IsCommitted = isCommitted;
                ResultKind = resultKind;
                IsClear = isClear;
                FailureReason = failureReason;
                CounterWaveSource = counterWaveSource;
                ElapsedSeconds = elapsedSeconds;
                RouteStability01 = routeStability01;
                CompletedObjectiveStepCount = completedObjectiveStepCount;
                ObjectiveStepCount = objectiveStepCount;
                CompletionReadout = completionReadout ?? string.Empty;
                ProofReadout = proofReadout ?? string.Empty;
                DecisionState = decisionState ?? string.Empty;
                DecisionReadout = decisionReadout ?? string.Empty;
                Title = title ?? string.Empty;
                Summary = summary ?? string.Empty;
                RouteLabel = routeLabel ?? string.Empty;
                RewardHook = rewardHook ?? string.Empty;
                NextObjective = nextObjective ?? string.Empty;
                ResultTokenId = resultTokenId ?? string.Empty;
                NextStateHookId = nextStateHookId ?? string.Empty;
            }

            public bool IsCommitted { get; }
            public RouteResultKind ResultKind { get; }
            public bool IsClear { get; }
            public RouteFailureReason FailureReason { get; }
            public CounterWaveSource CounterWaveSource { get; }
            public float ElapsedSeconds { get; }
            public float RouteStability01 { get; }
            public int CompletedObjectiveStepCount { get; }
            public int ObjectiveStepCount { get; }
            public string CompletionReadout { get; }
            public string ProofReadout { get; }
            public string DecisionState { get; }
            public string DecisionReadout { get; }
            public string Title { get; }
            public string Summary { get; }
            public string RouteLabel { get; }
            public string RewardHook { get; }
            public string NextObjective { get; }
            public string ResultTokenId { get; }
            public string NextStateHookId { get; }
        }

        public readonly struct RouteDecisionSnapshot
        {
            public RouteDecisionSnapshot(
                string state,
                string readout,
                string incentiveCue,
                ReviewPhase phase,
                int stageBeatIndex,
                string completionReadout)
            {
                State = state ?? string.Empty;
                Readout = readout ?? string.Empty;
                IncentiveCue = incentiveCue ?? string.Empty;
                Phase = phase;
                StageBeatIndex = stageBeatIndex;
                CompletionReadout = completionReadout ?? string.Empty;
            }

            public string State { get; }
            public string Readout { get; }
            public string IncentiveCue { get; }
            public ReviewPhase Phase { get; }
            public int StageBeatIndex { get; }
            public string CompletionReadout { get; }
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
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;

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
        [SerializeField, Min(0f)] private float counterWaveAnswerEnergyPulse;
        [SerializeField, Range(1, 3)] private int bossScreenSuppressMinimumSummonTier = 3;
        [SerializeField] private bool allowVanguardAssistToSuppressBossScreen = true;
        [SerializeField, Min(0f)] private float vanguardAssistSuppressSeconds = 8f;
        [SerializeField, Range(1, 3)] private int vanguardAssistSuppressMinimumTier = 3;

        [Header("Follow-up Result")]
        [SerializeField] private bool requireSkill1FollowupHitToClear = true;
        [SerializeField, Min(0f)] private float counterWaveAllyHoldSeconds = 0.45f;

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
        private CombatHealth subscribedPlayerHealth;
        private CombatHealth subscribedBossHealth;
        private BossSummonPressureAction subscribedBossSummonPressureAction;
        private PlayerSupportSummonSlotAction subscribedSummonSlot2Action;
        private PlayerSupportSummonSlotAction subscribedSummonSlot3Action;
        private bool followupMissedNotified;
        private bool bossBlockedSkill1Followup;
        private bool bossScreenSuppressedByFollowup;
        private int bossPressureScreensSuppressedByFollowup;
        private int highestBossScreenSuppressSummonTier;
        private int vanguardAssistSuppressTier;
        private float vanguardAssistSuppressTimer;
        private string lastSupportSummonUseSlotId;
        private int lastSupportSummonUseTier;
        private bool counterWaveObserved;
        private bool counterWaveStabilized;
        private bool counterWaveFinalWindowOpened;
        private bool grantedCounterWaveAnswerEnergyPulse;
        private CounterWaveSource counterWaveSource;
        private float lastCounterWaveEntryPenalty;
        private float lastCounterWaveStabilityBonus;
        private float lastCounterWaveFinalWindowDuration;
        private float lastCounterWaveFinalWindowRouteScale = 1f;
        private float lastCounterWaveAnswerEnergyPulse;
        private float lastUnansweredBossHitRoutePenalty;
        private float totalUnansweredBossHitRoutePenalty;
        private int unansweredBossHitRoutePenaltyCount;
        private float counterWaveAllyHoldTimer;
        private int summonUsesAtCounterWaveStart;
        private bool counterWaveAllyHoldInterrupted;
        private int bossPressureSummonReleasesAtReset;
        private int announcedStageBeatIndex;
        private RouteStabilityBand announcedRouteStabilityBand;
        private RouteResultRecord lastResultRecord;
        private int resultRecordCommitCount;
        private RouteDecisionSnapshot lastRouteDecisionSnapshot;
        private int routeDecisionChangeCount;

        public event Action<int> SummonFollowupWindowOpened;
        public event Action<int, float> SummonFollowupHitConfirmed;
        public event Action SummonFollowupMissed;
        public event Action<int, int> BossScreenSuppressedByFollowupConfirmed;
        public event Action SummonBlockOpportunityOpened;
        public event Action<CounterWaveSource> CounterWaveObserved;
        public event Action CounterWaveStabilized;
        public event Action PocketCleared;
        public event Action PocketFailed;
        public event Action<int> StageBeatChanged;
        public event Action<RouteStabilityBand, float> RouteStabilityBandChanged;
        public event Action<RouteResultRecord> ResultRecordCommitted;
        public event Action<RouteDecisionSnapshot> RouteDecisionChanged;

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
        public float LastCounterWaveFinalWindowRouteScale => lastCounterWaveFinalWindowRouteScale;
        public float CounterWaveAllyHoldRequiredSeconds => ResolveCounterWaveAllyHoldSeconds();
        public float CounterWaveAllyHoldElapsedSeconds => Mathf.Max(0f, counterWaveAllyHoldTimer);
        public float CounterWaveAllyHoldRemainingSeconds => counterWaveObserved && !counterWaveStabilized
            ? Mathf.Max(0f, CounterWaveAllyHoldRequiredSeconds - CounterWaveAllyHoldElapsedSeconds)
            : 0f;
        public bool WasCounterWaveAllyHoldInterrupted => counterWaveAllyHoldInterrupted;
        public float CounterWaveAllyHoldProgress01
        {
            get
            {
                if (!counterWaveObserved)
                {
                    return 0f;
                }

                float requiredSeconds = CounterWaveAllyHoldRequiredSeconds;
                return requiredSeconds <= 0f
                    ? 1f
                    : Mathf.Clamp01(CounterWaveAllyHoldElapsedSeconds / requiredSeconds);
            }
        }
        public string RouteDecisionState => ResolveRouteDecisionState();
        public string RouteDecisionReadout => ResolveRouteDecisionReadout();
        public string RouteIncentiveCue => ResolveRouteIncentiveCue();
        public RouteDecisionSnapshot LastRouteDecisionSnapshot => lastRouteDecisionSnapshot;
        public int RouteDecisionChangeCount => routeDecisionChangeCount;
        public string CompletionRecordReadout => ResolveCompletionRecordReadout();
        public RouteResultRecord LastResultRecord => lastResultRecord;
        public bool HasCommittedResultRecord => lastResultRecord.IsCommitted;
        public int ResultRecordCommitCount => resultRecordCommitCount;
        public int RouteProofStepCount => 4;
        public int CompletedRouteProofStepCount => ResolveCompletedRouteProofStepCount();
        public string RouteProofState => ResolveRouteProofState();
        public string RouteProofReadout => ResolveRouteProofReadout();
        public bool IsSkill1FollowupClearCountdownActive => state == PocketState.Running
            && skill1FollowupHitConfirmed
            && skill1FollowupClearTimer > 0f;
        public bool BossBlockedSkill1Followup => bossBlockedSkill1Followup;
        public int BossPressureBlocksDuringSummonFollowup => bossPressureBlocksConsumedDuringFollowup;
        public bool BossScreenSuppressedByFollowup => bossScreenSuppressedByFollowup;
        public int BossPressureScreensSuppressedByFollowup => bossPressureScreensSuppressedByFollowup;
        public int HighestBossScreenSuppressSummonTier => highestBossScreenSuppressSummonTier;
        public int VanguardAssistSuppressTier => vanguardAssistSuppressTimer > 0f ? vanguardAssistSuppressTier : 0;
        public float VanguardAssistSuppressRemainingSeconds => vanguardAssistSuppressTimer;
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
        public float LastCounterWaveAnswerEnergyPulse => lastCounterWaveAnswerEnergyPulse;
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
        public int UnansweredBossHitRoutePenaltyCount => unansweredBossHitRoutePenaltyCount;
        public float LastUnansweredBossHitRoutePenalty => lastUnansweredBossHitRoutePenalty;
        public float TotalUnansweredBossHitRoutePenalty => totalUnansweredBossHitRoutePenalty;
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
                        "Boss pressure broken; player HP survived");
                }

                if (state == PocketState.Failed)
                {
                    return ResolveStageText(
                        stageProfile != null ? stageProfile.FailObjectiveCue : null,
                        "Player HP reached zero before the answer completed");
                }

                if (counterWaveObserved && !counterWaveStabilized)
                {
                    if (bossBlockedSkill1Followup)
                    {
                        return $"{ResolveFollowupBlockedCue()}: {ResolveObjectiveSummonAnswerLabel()}";
                    }

                    return $"{ResolveCounterWaveCue()}: {ResolveObjectiveSummonAnswerLabel()}";
                }

                if (pressurePacing.IsSummonFollowupWindowActive || usedSkill1DuringSummonFollowup)
                {
                    if (skill1FollowupHitConfirmed)
                    {
                        return ResolveStageText(
                            stageProfile != null ? stageProfile.FollowupHitCue : null,
                            "Summon opening confirmed; Skill1 hit logged");
                    }

                    if (usedSkill1DuringSummonFollowup)
                    {
                        if (bossBlockedSkill1Followup)
                        {
                            return ResolveFollowupBlockedCue();
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
                            ? $"{ResolveFollowupBlockedCue()}: {ResolveObjectiveSummonAnswerLabel()}"
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
                        : $"{ResolveStageText(stageProfile != null ? stageProfile.SummonReadyCue : null, "Spend SummonSlot1 to block boss curtain")}: {ResolveObjectiveSummonAnswerLabel()}";
                }

                return energyLadder != null && !energyLadder.CanSpend
                    ? ResolveStageText(
                        stageProfile != null ? stageProfile.PreThreatChargeCue : null,
                        "Keep HP safe, build EN, then stop the close probe")
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
            UnsubscribeBossSummonPressureAction();
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
            SubscribePlayerHealth();
            SubscribeBossHealth();
            SubscribeBossSummonPressureAction();
            SubscribeSupportSummonActions();
        }

        public void ConfigureSupportSummonActions(
            PlayerSupportSummonSlotAction newSummonSlot2Action,
            PlayerSupportSummonSlotAction newSummonSlot3Action)
        {
            UnsubscribeSupportSummonActions();
            summonSlot2Action = newSummonSlot2Action;
            summonSlot3Action = newSummonSlot3Action;
            SubscribeSupportSummonActions();
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
            bossScreenSuppressedByFollowup = false;
            bossPressureScreensSuppressedByFollowup = 0;
            highestBossScreenSuppressSummonTier = 0;
            vanguardAssistSuppressTier = 0;
            vanguardAssistSuppressTimer = 0f;
            counterWaveObserved = false;
            counterWaveStabilized = false;
            counterWaveFinalWindowOpened = false;
            grantedCounterWaveAnswerEnergyPulse = false;
            counterWaveSource = CounterWaveSource.None;
            lastCounterWaveEntryPenalty = 0f;
            lastCounterWaveStabilityBonus = 0f;
            lastCounterWaveFinalWindowDuration = 0f;
            lastCounterWaveFinalWindowRouteScale = 1f;
            lastCounterWaveAnswerEnergyPulse = 0f;
            counterWaveAllyHoldTimer = 0f;
            summonUsesAtCounterWaveStart = 0;
            counterWaveAllyHoldInterrupted = false;
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
            lastUnansweredBossHitRoutePenalty = 0f;
            totalUnansweredBossHitRoutePenalty = 0f;
            unansweredBossHitRoutePenaltyCount = 0;
            lastSupportSummonUseSlotId = string.Empty;
            lastSupportSummonUseTier = 0;
            lastResultRecord = default;
            announcedStageBeatIndex = ResolveCurrentStageBeatIndex();
            announcedRouteStabilityBand = CurrentRouteStabilityBand;
            resultRecordCommitCount = 0;
            lastRouteDecisionSnapshot = BuildRouteDecisionSnapshot();
            routeDecisionChangeCount = 0;
            SetBarrageEnabled(true);
            SetEnergyGainEnabled(true);
            SetBossPressureCostGainEnabled(true);
            SetBossPressureActionsEnabled(true);
            SetMarkers();
        }

        private void OnEnable()
        {
            ResetPocket();
            SubscribePlayerHealth();
            SubscribeBossHealth();
            SubscribeBossSummonPressureAction();
            SubscribeSupportSummonActions();
        }

        private void OnDisable()
        {
            UnsubscribePlayerHealth();
            UnsubscribeBossHealth();
            UnsubscribeBossSummonPressureAction();
            UnsubscribeSupportSummonActions();
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
            TickVanguardAssistSuppress(deltaTime);
            CaptureActionUse();
            if (playerHealth != null && !playerHealth.IsAlive)
            {
                FailPocket(RouteFailureReason.PlayerDown);
                PublishRouteDecisionChangeIfNeeded();
                PublishStageBeatChangeIfNeeded();
                return;
            }

            CaptureCloseThreatDefeat();
            CaptureBossBlockedFollowup();
            UpdatePressurePacing(deltaTime);
            CaptureCounterWavePressure();
            CaptureCounterWaveAnswer(deltaTime);
            TickSkill1FollowupClearTimer(deltaTime);
            TickRouteStability(deltaTime);

            if (CanClearPocket())
            {
                ClearPocket();
            }

            PublishRouteDecisionChangeIfNeeded();
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
                    TrySuppressBossScreenForHighTierFollowup();
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

        private void OnPlayerDamaged(DamageInfo damageInfo)
        {
            if (state != PocketState.Running
                || !IsRouteStabilityActive
                || damageInfo.Amount <= 0f
                || !CombatTeamUtility.AreHostile(DamageTeam.Player, damageInfo.SourceTeam)
                || skill1FollowupHitConfirmed)
            {
                return;
            }

            float penalty = ResolveUnansweredBossHitRoutePenalty01();
            if (penalty <= 0f)
            {
                return;
            }

            lastUnansweredBossHitRoutePenalty = penalty;
            totalUnansweredBossHitRoutePenalty += penalty;
            unansweredBossHitRoutePenaltyCount++;
            RemoveRouteStability(penalty);
        }

        private void OnBossDamaged(DamageInfo damageInfo)
        {
            bool acceptsFollowupDamage = pressurePacing.IsSummonFollowupWindowActive
                || (skill1FollowupHitConfirmed && skill1FollowupClearTimer > 0f);
            if (state != PocketState.Running
                || !acceptsFollowupDamage
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
            bossScreenSuppressedByFollowup = false;
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
            int currentBossPressureBlockCount = GetBossPressureScreenBlockCount();
            if (!pressurePacing.IsSummonFollowupWindowActive
                || !usedSkill1DuringSummonFollowup
                || skill1FollowupHitConfirmed)
            {
                return;
            }

            int blocksAfterWindowStart = Mathf.Max(
                0,
                currentBossPressureBlockCount - bossPressureBlocksAtSummonBreakStart);
            if (blocksAfterWindowStart <= bossPressureBlocksConsumedDuringFollowup)
            {
                return;
            }

            RecordBossScreenBlockedFollowup(blocksAfterWindowStart);
        }

        private void RecordBossScreenBlockedFollowup(int pressureBlockCount)
        {
            if (!pressurePacing.IsSummonFollowupWindowActive
                || skill1FollowupHitConfirmed
                || bossBlockedSkill1Followup
                || (!usedSkill1DuringSummonFollowup && GetSkillUseCount() <= skillUsesAtSummonBreakStart))
            {
                return;
            }

            usedSkill1 = true;
            usedSkill1DuringSummonFollowup = true;
            int spentTier = skill1Action != null ? skill1Action.LastSpentTier : 0;
            highestSkillTier = Mathf.Max(highestSkillTier, spentTier);
            highestSummonFollowupSkillTier = Mathf.Max(highestSummonFollowupSkillTier, spentTier);
            observedSkillUseCount = Mathf.Max(observedSkillUseCount, GetSkillUseCount());
            bossPressureBlocksConsumedDuringFollowup = Mathf.Max(1, pressureBlockCount);
            bossBlockedSkill1Followup = true;
            pressurePacing.EndSummonFollowupWindow();
            NotifySummonFollowupMissedOnce(CounterWaveSource.BossScreenBlock);
        }

        private void TrySuppressBossScreenForHighTierFollowup()
        {
            int suppressTier = ResolveFollowupBossScreenSuppressTier();
            if (bossScreenSuppressedByFollowup
                || bossBlockedSkill1Followup
                || skill1FollowupHitConfirmed
                || !pressurePacing.IsSummonFollowupWindowActive
                || suppressTier < Mathf.Clamp(bossScreenSuppressMinimumSummonTier, 1, 3))
            {
                return;
            }

            BossSummonPressureAction pressureAction = bossPressureActionDirector != null
                ? bossPressureActionDirector.SummonPressureAction
                : null;
            if (pressureAction == null || pressureAction.ActivePressureScreenCount <= 0)
            {
                return;
            }

            int suppressed = pressureAction.SuppressActivePressureScreens(suppressTier);
            if (suppressed <= 0)
            {
                return;
            }

            bossScreenSuppressedByFollowup = true;
            bossPressureScreensSuppressedByFollowup += suppressed;
            highestBossScreenSuppressSummonTier = Mathf.Max(
                highestBossScreenSuppressSummonTier,
                suppressTier);
            vanguardAssistSuppressTier = 0;
            vanguardAssistSuppressTimer = 0f;
            BossScreenSuppressedByFollowupConfirmed?.Invoke(suppressTier, suppressed);
        }

        private int ResolveFollowupBossScreenSuppressTier()
        {
            int supportAssistTier = vanguardAssistSuppressTimer > 0f ? vanguardAssistSuppressTier : 0;
            return Mathf.Max(lastSummonPressureBreakTier, supportAssistTier);
        }

        private void NotifySummonFollowupMissedOnce(
            CounterWaveSource source = CounterWaveSource.FollowupMissed)
        {
            if (followupMissedNotified)
            {
                return;
            }

            followupMissedNotified = true;
            ObserveCounterWave(source);
            SummonFollowupMissed?.Invoke();
        }

        private void CaptureCounterWavePressure()
        {
            if (counterWaveObserved || !blockedBossPressureWithSummon || skill1FollowupHitConfirmed)
            {
                return;
            }

            if (pressurePacing.IsSummonFollowupWindowActive
                && !followupMissedNotified
                && !bossBlockedSkill1Followup)
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
                counterWaveAllyHoldTimer = 0f;
                summonUsesAtCounterWaveStart = GetSummonUseCount();
                counterWaveAllyHoldInterrupted = false;
                ApplyCounterWaveEntryRoutePenalty();
                GrantCounterWaveAnswerEnergyPulse();
                CounterWaveObserved?.Invoke(counterWaveSource);
            }
        }

        private void ApplyCounterWaveEntryRoutePenalty()
        {
            lastCounterWaveEntryPenalty = ResolveCounterWaveEntryRoutePenalty01();
            RemoveRouteStability(lastCounterWaveEntryPenalty);
        }

        private void CaptureCounterWaveAnswer(float deltaTime)
        {
            if (!counterWaveObserved || counterWaveStabilized)
            {
                return;
            }

            if (!HasCounterWaveAllyAnswer())
            {
                if (counterWaveAllyHoldTimer > 0f)
                {
                    counterWaveAllyHoldInterrupted = true;
                }

                counterWaveAllyHoldTimer = 0f;
                return;
            }

            counterWaveAllyHoldInterrupted = false;
            float requiredSeconds = ResolveCounterWaveAllyHoldSeconds();
            if (requiredSeconds > 0f)
            {
                counterWaveAllyHoldTimer = Mathf.Min(
                    requiredSeconds,
                    counterWaveAllyHoldTimer + Mathf.Max(0f, deltaTime));
                if (counterWaveAllyHoldTimer < requiredSeconds)
                {
                    return;
                }
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
            lastCounterWaveFinalWindowRouteScale = ResolveCounterWaveFinalWindowRouteScale();
            float followupWindowSeconds =
                ResolveSummonFollowupWindowSeconds(resolvedTier) * lastCounterWaveFinalWindowRouteScale;
            float followupEnergyPulse = ResolveSummonFollowupEnergyPulse(resolvedTier);
            skillUsesAtSummonBreakStart = GetSkillUseCount();
            grantedSummonFollowupEnergy = false;
            usedSkill1DuringSummonFollowup = false;
            skill1FollowupHitConfirmed = false;
            bossBlockedSkill1Followup = false;
            bossScreenSuppressedByFollowup = false;
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

        private bool HasCounterWaveAllyAnswer()
        {
            return ActiveAllyFrontlineProxyCount > 0
                && GetSummonUseCount() > summonUsesAtCounterWaveStart;
        }

        private void ClearPocket()
        {
            resultElapsedSeconds = elapsedSeconds;
            state = PocketState.Cleared;
            failureReason = RouteFailureReason.None;
            ClearPressurePacing();
            DismissActiveSummonPressureScreens();
            SetBarrageEnabled(!stopBarrageOnClear);
            SetEnergyGainEnabled(!stopEnergyGainOnEnd);
            SetBossPressureCostGainEnabled(!stopBossPressureCostOnEnd);
            SetBossPressureActionsEnabled(!stopBossPressureActionsOnEnd);
            SetMarkers();
            CommitResultRecord();
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
            CommitResultRecord();
            PocketFailed?.Invoke();
        }

        private void CommitResultRecord()
        {
            if (lastResultRecord.IsCommitted)
            {
                return;
            }

            RouteResultKind resultKind = ResolveRouteResultKind();
            lastResultRecord = new RouteResultRecord(
                true,
                resultKind,
                state == PocketState.Cleared,
                failureReason,
                counterWaveSource,
                resultElapsedSeconds,
                RouteStability01,
                CompletedObjectiveStepCount,
                ObjectiveStepCount,
                CompletionRecordReadout,
                RouteProofReadout,
                RouteDecisionState,
                RouteDecisionReadout,
                ResolveRouteResultTitle(resultKind),
                ResolveRouteResultSummary(resultKind),
                ResolveRouteResultLabel(resultKind),
                ResolveRouteResultRewardHook(resultKind),
                ResolveRouteResultNextObjective(resultKind),
                ResolveRouteResultTokenId(resultKind),
                ResolveRouteResultNextStateHookId(resultKind));
            resultRecordCommitCount++;
            ResultRecordCommitted?.Invoke(lastResultRecord);
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

        private void GrantCounterWaveAnswerEnergyPulse()
        {
            float energyAmount = ResolveCounterWaveAnswerEnergyPulse();
            if (grantedCounterWaveAnswerEnergyPulse
                || energyLadder == null
                || energyAmount <= 0f)
            {
                return;
            }

            energyLadder.GrantCurrentTierEnergy(energyAmount);
            summonSlot1Action?.ClearSlotCooldown();
            grantedCounterWaveAnswerEnergyPulse = true;
            lastCounterWaveAnswerEnergyPulse = energyAmount;
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
                bool isCounterWaveRecoveryPending = counterWaveObserved && !counterWaveStabilized;
                if (pressurePacing.IsSummonFollowupWindowActive && !isCounterWaveRecoveryPending)
                {
                    return 0f;
                }

                float pressureBreakScale = pressurePacing.IsSummonPressureBreakActive && !isCounterWaveRecoveryPending
                    ? 0.35f
                    : 1f;
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

        private void PublishRouteDecisionChangeIfNeeded()
        {
            RouteDecisionSnapshot current = BuildRouteDecisionSnapshot();
            if (IsSameRouteDecision(current, lastRouteDecisionSnapshot))
            {
                return;
            }

            lastRouteDecisionSnapshot = current;
            routeDecisionChangeCount++;
            RouteDecisionChanged?.Invoke(current);
        }

        private RouteDecisionSnapshot BuildRouteDecisionSnapshot()
        {
            return new RouteDecisionSnapshot(
                ResolveRouteDecisionState(),
                ResolveRouteDecisionReadout(),
                ResolveRouteIncentiveCue(),
                CurrentPhase,
                ResolveCurrentStageBeatIndex(),
                CompletionRecordReadout);
        }

        private static bool IsSameRouteDecision(
            RouteDecisionSnapshot first,
            RouteDecisionSnapshot second)
        {
            return string.Equals(first.State, second.State, StringComparison.Ordinal)
                && string.Equals(first.Readout, second.Readout, StringComparison.Ordinal)
                && string.Equals(first.IncentiveCue, second.IncentiveCue, StringComparison.Ordinal);
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
            string pressureState = state switch
            {
                "contest" => "contested",
                "ally" => "covered",
                "enemy" => "pressed",
                _ => "open"
            };
            return $"pressure x{ResolveFrontlinePresenceDrainScale(allyCount, enemyCount):0.00} {pressureState}";
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

        private float ResolveUnansweredBossHitRoutePenalty01()
        {
            return stageProfile != null ? stageProfile.UnansweredBossHitRoutePenalty01 : 0f;
        }

        private float ResolveCounterWaveStabilizeRouteBonus01()
        {
            return stageProfile != null ? stageProfile.CounterWaveStabilizeRouteBonus01 : 0f;
        }

        private float ResolveCounterWaveAllyHoldSeconds()
        {
            return stageProfile != null
                ? stageProfile.CounterWaveAllyHoldSeconds
                : Mathf.Max(0f, counterWaveAllyHoldSeconds);
        }

        private float ResolveCounterWaveFinalWindowRouteScale()
        {
            if (!IsRouteStabilityActive)
            {
                return 1f;
            }

            return CurrentRouteStabilityBand switch
            {
                RouteStabilityBand.Critical => stageProfile != null
                    ? stageProfile.CriticalCounterWaveFinalWindowScale
                    : 0.65f,
                RouteStabilityBand.Unstable => stageProfile != null
                    ? stageProfile.UnstableCounterWaveFinalWindowScale
                    : 0.85f,
                _ => 1f
            };
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

        private void TickVanguardAssistSuppress(float deltaTime)
        {
            if (vanguardAssistSuppressTimer <= 0f)
            {
                vanguardAssistSuppressTier = 0;
                return;
            }

            vanguardAssistSuppressTimer = Mathf.Max(0f, vanguardAssistSuppressTimer - Mathf.Max(0f, deltaTime));
            if (vanguardAssistSuppressTimer <= 0f)
            {
                vanguardAssistSuppressTier = 0;
            }
        }

        private void SubscribeBossSummonPressureAction()
        {
            BossSummonPressureAction pressureAction = bossPressureActionDirector != null
                ? bossPressureActionDirector.SummonPressureAction
                : null;
            if (subscribedBossSummonPressureAction == pressureAction)
            {
                return;
            }

            UnsubscribeBossSummonPressureAction();
            if (pressureAction == null)
            {
                return;
            }

            subscribedBossSummonPressureAction = pressureAction;
            subscribedBossSummonPressureAction.PressureSummonIntercepted += OnBossPressureSummonIntercepted;
        }

        private void UnsubscribeBossSummonPressureAction()
        {
            if (subscribedBossSummonPressureAction == null)
            {
                return;
            }

            subscribedBossSummonPressureAction.PressureSummonIntercepted -= OnBossPressureSummonIntercepted;
            subscribedBossSummonPressureAction = null;
        }

        private void SubscribeSupportSummonActions()
        {
            UnsubscribeSupportSummonActions();
            if (summonSlot2Action != null)
            {
                subscribedSummonSlot2Action = summonSlot2Action;
                subscribedSummonSlot2Action.SummonUsed += OnSupportSummonUsed;
                subscribedSummonSlot2Action.SummonPressureBlocked += OnSupportSummonPressureBlocked;
            }

            if (summonSlot3Action != null)
            {
                subscribedSummonSlot3Action = summonSlot3Action;
                subscribedSummonSlot3Action.SummonUsed += OnSupportSummonUsed;
                subscribedSummonSlot3Action.SummonPressureBlocked += OnSupportSummonPressureBlocked;
            }
        }

        private void UnsubscribeSupportSummonActions()
        {
            if (subscribedSummonSlot2Action != null)
            {
                subscribedSummonSlot2Action.SummonUsed -= OnSupportSummonUsed;
                subscribedSummonSlot2Action.SummonPressureBlocked -= OnSupportSummonPressureBlocked;
                subscribedSummonSlot2Action = null;
            }

            if (subscribedSummonSlot3Action != null)
            {
                subscribedSummonSlot3Action.SummonUsed -= OnSupportSummonUsed;
                subscribedSummonSlot3Action.SummonPressureBlocked -= OnSupportSummonPressureBlocked;
                subscribedSummonSlot3Action = null;
            }
        }

        private void OnSupportSummonUsed(PlayerSupportSummonSlotAction action, int tier)
        {
            if (state != PocketState.Running || action == null)
            {
                return;
            }

            lastSupportSummonUseSlotId = action.SlotActionName ?? string.Empty;
            lastSupportSummonUseTier = Mathf.Clamp(tier, 1, 3);
        }

        private void OnBossPressureSummonIntercepted(BossSummonPressureAction action, int tier)
        {
            if (state != PocketState.Running)
            {
                return;
            }

            int currentBossPressureBlockCount = GetBossPressureScreenBlockCount();
            int blocksAfterWindowStart = Mathf.Max(
                0,
                currentBossPressureBlockCount - bossPressureBlocksAtSummonBreakStart);
            RecordBossScreenBlockedFollowup(
                Mathf.Max(
                    blocksAfterWindowStart,
                    bossPressureBlocksConsumedDuringFollowup + 1));
        }

        private void OnSupportSummonPressureBlocked(PlayerSupportSummonSlotAction action, int tier)
        {
            if (state != PocketState.Running
                || !allowVanguardAssistToSuppressBossScreen
                || action == null
                || action != summonSlot3Action)
            {
                return;
            }

            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            if (resolvedTier < Mathf.Clamp(vanguardAssistSuppressMinimumTier, 1, 3))
            {
                return;
            }

            vanguardAssistSuppressTier = Mathf.Max(vanguardAssistSuppressTier, resolvedTier);
            vanguardAssistSuppressTimer = Mathf.Max(
                vanguardAssistSuppressTimer,
                Mathf.Max(0f, vanguardAssistSuppressSeconds));
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

        private string ResolveFollowupBlockedCue()
        {
            return ResolveStageText(
                stageProfile != null ? stageProfile.FollowupBlockedCue : null,
                "Boss screen blocked Skill1; rebuild the summon answer");
        }

        private string ResolveCounterWaveCue()
        {
            return ResolveStageText(
                stageProfile != null ? stageProfile.CounterWaveCue : null,
                "Counter pressure entered; keep HP safe and answer with summon");
        }

        private string ResolveCounterWaveStabilizedCue()
        {
            return ResolveStageText(
                stageProfile != null ? stageProfile.CounterWaveStabilizedCue : null,
                "Counter pressure held by summon; final strike window reopened");
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
            float stagePulseOverride = stageProfile != null ? stageProfile.CleanFollowupEnergyPulseOverride : 0f;
            if (summonPressureBlockOpportunity != null)
            {
                float opportunityPulse = summonPressureBlockOpportunity.ResolveFollowupEnergyPulse(tier);
                return stagePulseOverride > 0f ? Mathf.Max(opportunityPulse, stagePulseOverride) : opportunityPulse;
            }

            float defaultPulse = tier switch
            {
                3 => summonFollowupEnergyPulseTierThree,
                2 => summonFollowupEnergyPulseTierTwo,
                _ => summonFollowupEnergyPulse
            };
            return stagePulseOverride > 0f ? Mathf.Max(defaultPulse, stagePulseOverride) : defaultPulse;
        }

        private float ResolveCounterWaveAnswerEnergyPulse()
        {
            float stagePulseOverride =
                stageProfile != null ? stageProfile.CounterWaveAnswerEnergyPulseOverride : 0f;
            return stagePulseOverride > 0f
                ? stagePulseOverride
                : Mathf.Max(0f, counterWaveAnswerEnergyPulse);
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
                + $"decision:{RouteDecisionState}({RouteDecisionReadout}) "
                + $"proof:{RouteProofState}({RouteProofReadout})";
        }

        private static string ResolveRecordState(bool completed)
        {
            return completed ? "recorded" : "pending";
        }

        private int ResolveCompletedRouteProofStepCount()
        {
            int completed = 0;
            if (ResolveRouteProofTriggerReadout() != "pending")
            {
                completed++;
            }

            string targetReadout = ResolveRouteProofTargetReadout();
            if (targetReadout == "boss_curtain" || targetReadout == "ally_hold")
            {
                completed++;
            }

            if (skill1FollowupHitConfirmed)
            {
                completed++;
            }

            if (state == PocketState.Cleared)
            {
                completed++;
            }

            return Mathf.Clamp(completed, 0, RouteProofStepCount);
        }

        private string ResolveRouteProofState()
        {
            if (state == PocketState.Cleared)
            {
                return "committed";
            }

            if (state == PocketState.Failed)
            {
                return "failed";
            }

            if (skill1FollowupHitConfirmed)
            {
                return "log_pending";
            }

            string targetReadout = ResolveRouteProofTargetReadout();
            if (targetReadout == "boss_curtain" || targetReadout == "ally_hold")
            {
                return "answer_pending";
            }

            return ResolveRouteProofTriggerReadout() != "pending" ? "threat_pending" : "pending";
        }

        private string ResolveRouteProofReadout()
        {
            return $"{CompletedRouteProofStepCount}/{RouteProofStepCount} "
                + $"trigger:{ResolveRouteProofTriggerReadout()} "
                + $"threat:{ResolveRouteProofTargetReadout()} "
                + $"answer:{ResolveRouteProofPayloadReadout()} "
                + $"log:{ResolveRouteProofLogReadout()}";
        }

        private string ResolveRouteProofTriggerReadout()
        {
            if (counterWaveObserved)
            {
                return "counter_wave";
            }

            return closeThreatDefeated ? "close_probe" : "pending";
        }

        private string ResolveRouteProofTargetReadout()
        {
            if (counterWaveObserved)
            {
                if (counterWaveStabilized)
                {
                    return "ally_hold";
                }

                if (HasCounterWaveAllyAnswer())
                {
                    return "ally_holding";
                }

                return counterWaveAllyHoldInterrupted ? "interrupted" : "ally_needed";
            }

            if (blockedBossPressureWithSummon)
            {
                return "boss_curtain";
            }

            return closeThreatDefeated ? "summon_needed" : "pending";
        }

        private string ResolveRouteProofPayloadReadout()
        {
            if (skill1FollowupHitConfirmed)
            {
                return IsCounterRecoveryRoute() ? "final_skill" : "skill1_confirm";
            }

            if (counterWaveFinalWindowOpened)
            {
                return "final_window";
            }

            if (pressurePacing.IsSummonFollowupWindowActive)
            {
                return "window_open";
            }

            return "pending";
        }

        private string ResolveRouteProofLogReadout()
        {
            if (state == PocketState.Cleared)
            {
                return "committed";
            }

            return state == PocketState.Failed ? "failed" : "pending";
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
                if (counterWaveAllyHoldInterrupted)
                {
                    return "interrupted";
                }

                return HasCounterWaveAllyAnswer()
                    ? $"holding_{CounterWaveAllyHoldProgress01 * 100f:0}%"
                    : "awaiting";
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

            return "survive";
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
                    ? "pressure_zero"
                    : "player_down";
            }

            if (counterWaveObserved)
            {
                if (counterWaveStabilized)
                {
                    return counterWaveFinalWindowOpened ? "final_window" : "counter_held";
                }

                return HasCounterWaveAllyAnswer() ? "ally_holding" : "answer_counter";
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

            return "keep_hp";
        }

        private string ResolveRouteIncentiveCue()
        {
            if (lastResultRecord.IsCommitted)
            {
                return lastResultRecord.IsClear
                    ? $"Pressure answer complete: {lastResultRecord.RouteLabel}"
                    : lastResultRecord.RewardHook;
            }

            if (state == PocketState.Failed)
            {
                return ResolveStageText(
                    stageProfile != null ? stageProfile.FailedRouteRewardHook : null,
                    "Failure analysis logged: player HP reached zero before the answer was complete.");
            }

            if (state == PocketState.Cleared)
            {
                return $"Pressure answer complete: {ResolveRouteResultLabel(ResolveRouteResultKind())}";
            }

            if (IsRouteStabilityActive && CurrentRouteStabilityBand == RouteStabilityBand.Critical)
            {
                return ResolveStageText(
                    stageProfile != null ? stageProfile.CollapseWarningRecordPreview : null,
                    "HP is the fail state; pressure is critical.");
            }

            if (counterWaveObserved && !skill1FollowupHitConfirmed)
            {
                return ResolveStageText(
                    stageProfile != null ? stageProfile.CounterRecoveryRecordPreview : null,
                    "Keep summon pressure held to reopen final follow-up.");
            }

            if (pressurePacing.IsSummonFollowupWindowActive
                || IsSkill1FollowupClearCountdownActive
                || pressurePacing.IsSummonPressureBreakActive)
            {
                return ResolveStageText(
                    stageProfile != null ? stageProfile.CleanFollowupRecordPreview : null,
                    "Skill1 can secure HP-safe clear before counter pressure.");
            }

            if (IsAwaitingSummonPressureBlock
                || IsSummonBlockOpportunityCueActive
                || closeThreatDefeated)
            {
                return ResolveStageText(
                    stageProfile != null ? stageProfile.SummonRecordPreview : null,
                    "Summon cover opens the Skill1 answer.");
            }

            return ResolveStageText(
                stageProfile != null ? stageProfile.OpeningRecordPreview : null,
                "Stop close probe, block curtain, then confirm Skill1.");
        }

        private bool IsCounterRecoveryRoute()
        {
            return counterWaveStabilized || counterWaveFinalWindowOpened;
        }

        private RouteResultKind ResolveRouteResultKind()
        {
            if (state == PocketState.Cleared)
            {
                if (IsCounterRecoveryRoute())
                {
                    return RouteResultKind.CounterRecoveryClear;
                }

                return skill1FollowupHitConfirmed
                    ? RouteResultKind.CleanFollowupClear
                    : RouteResultKind.PressureSuppressionClear;
            }

            if (state == PocketState.Failed)
            {
                return failureReason == RouteFailureReason.RouteStabilityCollapsed
                    ? RouteResultKind.PressureControlFail
                    : RouteResultKind.PlayerDownFail;
            }

            return RouteResultKind.None;
        }

        private string ResolveRouteResultLabel(RouteResultKind resultKind)
        {
            return resultKind switch
            {
                RouteResultKind.CleanFollowupClear => ResolveCleanFollowupResultLabel(),
                RouteResultKind.CounterRecoveryClear => "Counter recovery",
                RouteResultKind.PressureSuppressionClear => "Pressure suppression",
                RouteResultKind.PressureControlFail => "Pressure control zero",
                RouteResultKind.PlayerDownFail => "Player down",
                _ => "-"
            };
        }

        private string ResolveRouteResultTitle(RouteResultKind resultKind)
        {
            return resultKind switch
            {
                RouteResultKind.CleanFollowupClear
                    or RouteResultKind.CounterRecoveryClear
                    or RouteResultKind.PressureSuppressionClear => ResolveStageText(
                        stageProfile != null ? stageProfile.ClearTitle : null,
                        "PRESSURE BROKEN"),
                RouteResultKind.PlayerDownFail or RouteResultKind.PressureControlFail => ResolveStageText(
                    stageProfile != null ? stageProfile.FailTitle : null,
                    "PLAYER DOWN"),
                _ => string.Empty
            };
        }

        private string ResolveRouteResultSummary(RouteResultKind resultKind)
        {
            return resultKind switch
            {
                RouteResultKind.CounterRecoveryClear => ResolveStageText(
                    stageProfile != null ? stageProfile.ClearCounterDetail : null,
                    "Counter pressure held; final follow-up confirmed"),
                RouteResultKind.CleanFollowupClear => ResolveCleanFollowupResultSummary(),
                RouteResultKind.PressureSuppressionClear => ResolveStageText(
                    stageProfile != null ? stageProfile.ClearPressureDetail : null,
                    "Boss curtain suppressed; survival answer recorded"),
                RouteResultKind.PressureControlFail => ResolveStageText(
                    stageProfile != null ? stageProfile.RouteCollapseFailDetail : null,
                    "Pressure control hit zero, but HP survival remains the fail state"),
                RouteResultKind.PlayerDownFail => ResolveStageText(
                    stageProfile != null ? stageProfile.FailDetail : null,
                    "Player HP reached zero before the boss pressure was answered"),
                _ => string.Empty
            };
        }

        private string ResolveRouteResultRewardHook(RouteResultKind resultKind)
        {
            return resultKind switch
            {
                RouteResultKind.PlayerDownFail or RouteResultKind.PressureControlFail => ResolveStageText(
                    stageProfile != null ? stageProfile.FailedRouteRewardHook : null,
                    "Failure analysis logged: player HP reached zero before the answer was complete."),
                RouteResultKind.CounterRecoveryClear => ResolveStageText(
                    stageProfile != null ? stageProfile.CounterRecoveryRewardHook : null,
                    "Counter recovery logged: summon absorbed pressure and reopened the final strike window."),
                RouteResultKind.CleanFollowupClear => ResolveCleanFollowupRewardHook(),
                _ => ResolveStageText(
                    stageProfile != null ? stageProfile.RewardHook : null,
                    "No payout or progression grant.")
            };
        }

        private string ResolveRouteResultNextObjective(RouteResultKind resultKind)
        {
            return resultKind switch
            {
                RouteResultKind.PlayerDownFail or RouteResultKind.PressureControlFail => ResolveStageText(
                    stageProfile != null ? stageProfile.FailedRouteNextObjective : null,
                    "Next run: protect HP first, then spend summon on the visible curtain."),
                RouteResultKind.CounterRecoveryClear => ResolveStageText(
                    stageProfile != null ? stageProfile.CounterRecoveryNextObjective : null,
                    "Next run: answer counter pressure earlier so recovery becomes a clean survival answer."),
                RouteResultKind.CleanFollowupClear => ResolveCleanFollowupNextObjective(),
                RouteResultKind.PressureSuppressionClear => ResolveStageText(
                    stageProfile != null ? stageProfile.CleanRouteNextObjective : null,
                    "Next run: keep HP clean by confirming before counter pressure enters."),
                _ => string.Empty
            };
        }

        private string ResolveRouteResultTokenId(RouteResultKind resultKind)
        {
            return resultKind switch
            {
                RouteResultKind.PlayerDownFail or RouteResultKind.PressureControlFail =>
                    "review.failure.hp_pressure",
                RouteResultKind.CounterRecoveryClear =>
                    "review.clear.counter_recovery",
                RouteResultKind.CleanFollowupClear =>
                    ResolveCleanFollowupTokenId(),
                RouteResultKind.PressureSuppressionClear =>
                    "review.clear.pressure_suppression",
                _ => "review.pending"
            };
        }

        private string ResolveRouteResultNextStateHookId(RouteResultKind resultKind)
        {
            return resultKind switch
            {
                RouteResultKind.PlayerDownFail or RouteResultKind.PressureControlFail =>
                    "next.practice.hp_protection",
                RouteResultKind.CounterRecoveryClear =>
                    "next.practice.counter_answer_timing",
                RouteResultKind.CleanFollowupClear =>
                    ResolveCleanFollowupNextStateHookId(),
                RouteResultKind.PressureSuppressionClear =>
                    "next.practice.clean_followup_confirm",
                _ => string.Empty
            };
        }

        private string ResolveCleanFollowupResultLabel()
        {
            if (lastSupportSummonUseSlotId == "SummonSlot2")
            {
                return $"Clean LV{lastSupportSummonUseTier} marksman follow-up";
            }

            if (lastSupportSummonUseSlotId == "SummonSlot3")
            {
                return $"Clean LV{lastSupportSummonUseTier} vanguard follow-up";
            }

            return "Clean summon follow-up";
        }

        private string ResolveCleanFollowupResultSummary()
        {
            if (lastSupportSummonUseSlotId == "SummonSlot2")
            {
                return "Marksman support suppressed the frontline; Slot1 preserved the Skill1 confirm";
            }

            if (lastSupportSummonUseSlotId == "SummonSlot3")
            {
                return "Vanguard hold carried into boss-screen suppression; Skill1 follow-up landed";
            }

            return ResolveStageText(
                stageProfile != null ? stageProfile.ClearFollowupDetail : null,
                "Summon opening confirmed; Skill1 follow-up landed");
        }

        private string ResolveCleanFollowupRewardHook()
        {
            if (lastSupportSummonUseSlotId == "SummonSlot2")
            {
                return "Marksman combo logged: support fire preserved the main-answer summon.";
            }

            if (lastSupportSummonUseSlotId == "SummonSlot3")
            {
                return "Vanguard payoff logged: high-cost line hold converted into a direct boss-screen break.";
            }

            return ResolveStageText(
                stageProfile != null ? stageProfile.CleanRouteRewardHook : null,
                "Clean survival logged: summon cover created a Skill1 confirm before counter pressure arrived.");
        }

        private string ResolveCleanFollowupTokenId()
        {
            if (lastSupportSummonUseSlotId == "SummonSlot2")
            {
                return "review.clear.marksman_combo";
            }

            if (lastSupportSummonUseSlotId == "SummonSlot3")
            {
                return "review.clear.vanguard_payoff";
            }

            return "review.clear.clean_followup";
        }

        private string ResolveCleanFollowupNextObjective()
        {
            if (lastSupportSummonUseSlotId == "SummonSlot2")
            {
                return "Next run: spend Slot2 from a full bank when preserving Slot1 matters.";
            }

            if (lastSupportSummonUseSlotId == "SummonSlot3")
            {
                return "Next run: commit Slot3 when line safety is worth the delayed main answer.";
            }

            return ResolveStageText(
                stageProfile != null ? stageProfile.CleanRouteNextObjective : null,
                "Next run: keep HP clean by confirming before counter pressure enters.");
        }

        private string ResolveCleanFollowupNextStateHookId()
        {
            if (lastSupportSummonUseSlotId == "SummonSlot2")
            {
                return "next.practice.slot2_full_bank_combo";
            }

            if (lastSupportSummonUseSlotId == "SummonSlot3")
            {
                return "next.practice.slot3_vanguard_payoff";
            }

            return "next.practice.clean_followup_confirm";
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

        private void SubscribePlayerHealth()
        {
            if (subscribedPlayerHealth == playerHealth)
            {
                return;
            }

            UnsubscribePlayerHealth();
            if (playerHealth == null)
            {
                return;
            }

            subscribedPlayerHealth = playerHealth;
            subscribedPlayerHealth.Damaged += OnPlayerDamaged;
        }

        private void UnsubscribePlayerHealth()
        {
            if (subscribedPlayerHealth == null)
            {
                return;
            }

            subscribedPlayerHealth.Damaged -= OnPlayerDamaged;
            subscribedPlayerHealth = null;
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
