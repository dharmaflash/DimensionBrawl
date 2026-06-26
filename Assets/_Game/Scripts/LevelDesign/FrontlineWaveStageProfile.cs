using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [CreateAssetMenu(
        menuName = "DimensionBrawl/Profiles/Frontline Wave Stage Profile",
        fileName = "DB_FrontlineWaveStage")]
    public sealed class FrontlineWaveStageProfile : ScriptableObject
    {
        [Serializable]
        public struct StageBeat
        {
            [SerializeField] private string beatId;
            [SerializeField] private string label;
            [TextArea, SerializeField] private string objectiveCue;
            [TextArea, SerializeField] private string observedEvent;
            [TextArea, SerializeField] private string sourcePattern;

            public string BeatId => beatId;
            public string Label => label;
            public string ObjectiveCue => objectiveCue;
            public string ObservedEvent => observedEvent;
            public string SourcePattern => sourcePattern;
        }

        [Serializable]
        public struct SourceReference
        {
            [SerializeField] private string sourceId;
            [SerializeField] private string sourcePath;
            [TextArea, SerializeField] private string localTakeaway;

            public string SourceId => sourceId;
            public string SourcePath => sourcePath;
            public string LocalTakeaway => localTakeaway;
        }

        [Serializable]
        public struct PressureSlot
        {
            [SerializeField] private string slotId;
            [SerializeField] private string label;
            [SerializeField] private string spawnFamily;
            [SerializeField] private string wavePathPattern;
            [TextArea, SerializeField] private string playerRead;
            [TextArea, SerializeField] private string observerEvent;
            [SerializeField, Min(0f)] private float routePressureWeight;

            public string SlotId => slotId;
            public string Label => label;
            public string SpawnFamily => spawnFamily;
            public string WavePathPattern => wavePathPattern;
            public string PlayerRead => playerRead;
            public string ObserverEvent => observerEvent;
            public float RoutePressureWeight => Mathf.Max(0f, routePressureWeight);
        }

        [Header("Identity")]
        [SerializeField] private string stageId = "FRONTLINE-MOTIVATION-REVIEW-01";
        [SerializeField] private string displayName = "HP Pressure Review";
        [SerializeField] private string stageEpisodeLabel = "EP 03 Survival Pressure";
        [SerializeField] private string objectiveBadgeLabel = "HP";
        [TextArea, SerializeField] private string combatPromise =
            "Survive boss pressure; summons buy the opening";
        [TextArea, SerializeField] private string entryCue = "Stay alive; block boss pressure, then confirm Skill1";

        [Header("Data Pattern")]
        [SerializeField, Min(1f)] private float targetDurationSeconds = 90f;
        [SerializeField] private string waveSlotPattern = "CloseProbe -> AimShot -> ScreenCurtain -> BodyRush -> CoreExpose";
        [SerializeField] private string spawnFamilyPattern = "Drop | Dash | Jump | Normal";
        [SerializeField] private string observerLoop = "condition gate -> combat observer -> completion record -> reward/state hook";
        [SerializeField] private string routeEvidencePattern = "trigger -> threat -> answer -> cue -> log";
        [SerializeField] private string rewardHook = "No payout or progression grant.";

        [Header("Pressure Control")]
        [SerializeField, Range(0f, 1f)] private float routeStabilityStart01 = 0.62f;
        [SerializeField, Min(0f)] private float closeProbeRouteDrainPerSecond = 0.045f;
        [SerializeField, Min(0f)] private float summonAnswerRouteDrainPerSecond = 0.06f;
        [SerializeField, Min(0f)] private float counterWaveRouteDrainPerSecond = 0.08f;
        [SerializeField, Range(0f, 1f)] private float closeProbeDefeatRouteBonus01 = 0.12f;
        [SerializeField, Range(0f, 1f)] private float summonBlockRouteBonus01 = 0.18f;
        [SerializeField, Range(0f, 1f)] private float followupHitRouteBonus01 = 0.20f;
        [SerializeField, Range(0f, 1f)] private float counterWaveEntryRoutePenalty01 = 0.10f;
        [SerializeField, Range(0f, 1f)] private float counterWaveStabilizeRouteBonus01 = 0.14f;
        [SerializeField, Min(0f)] private float counterWaveAllyHoldSeconds = 0.45f;
        [SerializeField, Range(0.1f, 1f)] private float unstableCounterWaveFinalWindowScale = 0.85f;
        [SerializeField, Range(0.1f, 1f)] private float criticalCounterWaveFinalWindowScale = 0.65f;

        [Header("Objective Copy")]
        [SerializeField, Min(1)] private int objectiveStepCount = 3;
        [SerializeField] private string stepPrefix = "Survive";
        [TextArea, SerializeField] private string preThreatChargeCue = "Keep HP safe, build EN, then stop the close probe";
        [TextArea, SerializeField] private string preThreatReadyCue = "Stop the close probe and keep SummonSlot1 ready for boss curtain";
        [TextArea, SerializeField] private string summonChargeCue = "Build EN for SummonSlot1; boss curtain is returning";
        [TextArea, SerializeField] private string summonReadyCue = "Spend SummonSlot1 to block boss curtain";
        [TextArea, SerializeField] private string summonOpportunityCue = "Summon cover is open";
        [TextArea, SerializeField] private string followupReadyCue = "Confirm the summon opening with Skill1";
        [TextArea, SerializeField] private string followupFiredCue = "Skill1 committed into the summon opening";
        [TextArea, SerializeField] private string followupHitCue = "Summon opening confirmed; Skill1 hit logged";
        [TextArea, SerializeField] private string followupBlockedCue = "Boss screen absorbed the follow-up; rebuild the summon answer";
        [TextArea, SerializeField] private string followupMissedCue = "Follow-up window missed; boss pressure is returning";
        [TextArea, SerializeField] private string pressureBreakCue = "Boss curtain suppressed briefly; read the follow-up window";
        [TextArea, SerializeField] private string counterWaveCue = "Counter pressure entered; keep HP safe and answer with summon";
        [TextArea, SerializeField] private string counterWaveStabilizedCue = "Counter pressure held by summon; final strike window reopened";
        [TextArea, SerializeField] private string clearObjectiveCue = "Boss pressure broken; player HP survived";
        [TextArea, SerializeField] private string failObjectiveCue = "Player HP reached zero before the answer completed";

        [Header("Result Copy")]
        [SerializeField] private string clearTitle = "PRESSURE BROKEN";
        [SerializeField] private string clearFollowupDetail = "Summon opening confirmed; Skill1 follow-up landed";
        [SerializeField] private string clearCounterDetail = "Counter pressure held; final follow-up confirmed";
        [SerializeField] private string clearPressureDetail = "Boss curtain suppressed; survival answer recorded";
        [SerializeField] private string failTitle = "PLAYER DOWN";
        [SerializeField] private string failDetail = "Player HP reached zero before the boss pressure was answered";
        [SerializeField] private string routeCollapseFailDetail = "Pressure control hit zero, but HP survival remains the fail state";
        [TextArea, SerializeField] private string cleanRouteRewardHook =
            "Clean survival logged: summon cover created a Skill1 confirm before counter pressure arrived.";
        [TextArea, SerializeField] private string counterRecoveryRewardHook =
            "Counter recovery logged: summon absorbed pressure and reopened the final strike window.";
        [TextArea, SerializeField] private string failedRouteRewardHook =
            "Failure analysis logged: player HP reached zero before the answer was complete.";
        [TextArea, SerializeField] private string cleanRouteNextObjective =
            "Next run: keep HP clean by confirming before counter pressure enters.";
        [TextArea, SerializeField] private string counterRecoveryNextObjective =
            "Next run: answer counter pressure earlier so recovery becomes a clean survival answer.";
        [TextArea, SerializeField] private string failedRouteNextObjective =
            "Next run: protect HP first, then spend summon on the visible curtain.";

        [Header("In-Match Motivation Copy")]
        [TextArea, SerializeField] private string openingRecordPreview =
            "Stop close probe, block curtain, then confirm Skill1.";
        [TextArea, SerializeField] private string summonRecordPreview =
            "Summon cover opens the Skill1 answer.";
        [TextArea, SerializeField] private string cleanFollowupRecordPreview =
            "Skill1 can secure HP-safe clear before counter pressure.";
        [TextArea, SerializeField] private string counterRecoveryRecordPreview =
            "Keep summon pressure held to reopen final follow-up.";
        [TextArea, SerializeField] private string collapseWarningRecordPreview =
            "HP is the fail state; pressure is critical.";

        [Header("Review Evidence")]
        [SerializeField] private StageBeat[] beats = Array.Empty<StageBeat>();
        [SerializeField] private PressureSlot[] pressureSlots = Array.Empty<PressureSlot>();
        [SerializeField] private SourceReference[] sourceReferences = Array.Empty<SourceReference>();

        public string StageId => stageId;
        public string DisplayName => displayName;
        public string StageEpisodeLabel => stageEpisodeLabel;
        public string ObjectiveBadgeLabel => objectiveBadgeLabel;
        public string CombatPromise => combatPromise;
        public string EntryCue => entryCue;
        public float TargetDurationSeconds => targetDurationSeconds;
        public string WaveSlotPattern => waveSlotPattern;
        public string SpawnFamilyPattern => spawnFamilyPattern;
        public string ObserverLoop => observerLoop;
        public string RouteEvidencePattern => routeEvidencePattern;
        public string RewardHook => rewardHook;
        public float RouteStabilityStart01 => Mathf.Clamp01(routeStabilityStart01);
        public float CloseProbeRouteDrainPerSecond => Mathf.Max(0f, closeProbeRouteDrainPerSecond);
        public float SummonAnswerRouteDrainPerSecond => Mathf.Max(0f, summonAnswerRouteDrainPerSecond);
        public float CounterWaveRouteDrainPerSecond => Mathf.Max(0f, counterWaveRouteDrainPerSecond);
        public float CloseProbeDefeatRouteBonus01 => Mathf.Clamp01(closeProbeDefeatRouteBonus01);
        public float SummonBlockRouteBonus01 => Mathf.Clamp01(summonBlockRouteBonus01);
        public float FollowupHitRouteBonus01 => Mathf.Clamp01(followupHitRouteBonus01);
        public float CounterWaveEntryRoutePenalty01 => Mathf.Clamp01(counterWaveEntryRoutePenalty01);
        public float CounterWaveStabilizeRouteBonus01 => Mathf.Clamp01(counterWaveStabilizeRouteBonus01);
        public float CounterWaveAllyHoldSeconds => Mathf.Max(0f, counterWaveAllyHoldSeconds);
        public float UnstableCounterWaveFinalWindowScale => Mathf.Clamp(unstableCounterWaveFinalWindowScale, 0.1f, 1f);
        public float CriticalCounterWaveFinalWindowScale => Mathf.Clamp(criticalCounterWaveFinalWindowScale, 0.1f, 1f);
        public int ObjectiveStepCount => Mathf.Max(1, objectiveStepCount);
        public string StepPrefix => string.IsNullOrWhiteSpace(stepPrefix) ? "Survive" : stepPrefix;
        public string PreThreatChargeCue => preThreatChargeCue;
        public string PreThreatReadyCue => preThreatReadyCue;
        public string SummonChargeCue => summonChargeCue;
        public string SummonReadyCue => summonReadyCue;
        public string SummonOpportunityCue => summonOpportunityCue;
        public string FollowupReadyCue => followupReadyCue;
        public string FollowupFiredCue => followupFiredCue;
        public string FollowupHitCue => followupHitCue;
        public string FollowupBlockedCue => followupBlockedCue;
        public string FollowupMissedCue => followupMissedCue;
        public string PressureBreakCue => pressureBreakCue;
        public string CounterWaveCue => counterWaveCue;
        public string CounterWaveStabilizedCue => counterWaveStabilizedCue;
        public string ClearObjectiveCue => clearObjectiveCue;
        public string FailObjectiveCue => failObjectiveCue;
        public string ClearTitle => clearTitle;
        public string ClearFollowupDetail => clearFollowupDetail;
        public string ClearCounterDetail => clearCounterDetail;
        public string ClearPressureDetail => clearPressureDetail;
        public string FailTitle => failTitle;
        public string FailDetail => failDetail;
        public string RouteCollapseFailDetail => routeCollapseFailDetail;
        public string CleanRouteRewardHook => cleanRouteRewardHook;
        public string CounterRecoveryRewardHook => counterRecoveryRewardHook;
        public string FailedRouteRewardHook => failedRouteRewardHook;
        public string CleanRouteNextObjective => cleanRouteNextObjective;
        public string CounterRecoveryNextObjective => counterRecoveryNextObjective;
        public string FailedRouteNextObjective => failedRouteNextObjective;
        public string OpeningRecordPreview => openingRecordPreview;
        public string SummonRecordPreview => summonRecordPreview;
        public string CleanFollowupRecordPreview => cleanFollowupRecordPreview;
        public string CounterRecoveryRecordPreview => counterRecoveryRecordPreview;
        public string CollapseWarningRecordPreview => collapseWarningRecordPreview;
        public int BeatCount => beats != null ? beats.Length : 0;
        public int PressureSlotCount => pressureSlots != null ? pressureSlots.Length : 0;
        public int SourceReferenceCount => sourceReferences != null ? sourceReferences.Length : 0;

        public StageBeat GetBeat(int index)
        {
            if (beats == null || index < 0 || index >= beats.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return beats[index];
        }

        public SourceReference GetSourceReference(int index)
        {
            if (sourceReferences == null || index < 0 || index >= sourceReferences.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return sourceReferences[index];
        }

        public PressureSlot GetPressureSlot(int index)
        {
            if (pressureSlots == null || index < 0 || index >= pressureSlots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return pressureSlots[index];
        }

        public string SelectText(string profileText, string fallback)
        {
            return string.IsNullOrWhiteSpace(profileText) ? fallback : profileText;
        }
    }
}
