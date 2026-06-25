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
        [SerializeField] private string displayName = "Frontline Motivation Review";
        [SerializeField] private string stageEpisodeLabel = "EP 03 Frontline Stabilization";
        [SerializeField] private string objectiveBadgeLabel = "LANE";
        [TextArea, SerializeField] private string combatPromise =
            "Bodies stay split; waves and summons contest the line";
        [TextArea, SerializeField] private string entryCue = "Hold line; prove summon route";

        [Header("Data Pattern")]
        [SerializeField, Min(1f)] private float targetDurationSeconds = 90f;
        [SerializeField] private string waveSlotPattern = "CloseProbe -> LineProjectile -> ScreenCurtain -> FrontlineBody -> CoreExpose";
        [SerializeField] private string spawnFamilyPattern = "Drop | Dash | Jump | Normal";
        [SerializeField] private string observerLoop = "condition gate -> combat observer -> completion record -> reward/state hook";
        [SerializeField] private string rewardHook = "Review-only result hook; no payout or progression grant.";

        [Header("Route Stability")]
        [SerializeField, Range(0f, 1f)] private float routeStabilityStart01 = 0.62f;
        [SerializeField, Min(0f)] private float closeProbeRouteDrainPerSecond = 0.045f;
        [SerializeField, Min(0f)] private float summonAnswerRouteDrainPerSecond = 0.06f;
        [SerializeField, Min(0f)] private float counterWaveRouteDrainPerSecond = 0.08f;
        [SerializeField, Range(0f, 1f)] private float closeProbeDefeatRouteBonus01 = 0.12f;
        [SerializeField, Range(0f, 1f)] private float summonBlockRouteBonus01 = 0.18f;
        [SerializeField, Range(0f, 1f)] private float followupHitRouteBonus01 = 0.20f;
        [SerializeField, Range(0f, 1f)] private float counterWaveStabilizeRouteBonus01 = 0.14f;

        [Header("Objective Copy")]
        [SerializeField, Min(1)] private int objectiveStepCount = 3;
        [SerializeField] private string stepPrefix = "Route";
        [TextArea, SerializeField] private string preThreatChargeCue = "Build EN while holding the player line, then stop the close probe";
        [TextArea, SerializeField] private string preThreatReadyCue = "Stop the close probe and keep SummonSlot1 ready for boss curtain";
        [TextArea, SerializeField] private string summonChargeCue = "Build EN for SummonSlot1; boss curtain is returning";
        [TextArea, SerializeField] private string summonReadyCue = "Send SummonSlot1 across the line to block boss curtain";
        [TextArea, SerializeField] private string summonOpportunityCue = "Summon route is open";
        [TextArea, SerializeField] private string followupReadyCue = "Confirm the summon opening with Skill1";
        [TextArea, SerializeField] private string followupFiredCue = "Skill1 committed into the summon opening";
        [TextArea, SerializeField] private string followupHitCue = "Summon route analyzed; Skill1 hit confirmed";
        [TextArea, SerializeField] private string followupBlockedCue = "Boss screen absorbed the follow-up; rebuild the summon answer";
        [TextArea, SerializeField] private string followupMissedCue = "Follow-up window missed; boss pressure is returning";
        [TextArea, SerializeField] private string pressureBreakCue = "Boss curtain suppressed briefly; read the follow-up window";
        [TextArea, SerializeField] private string counterWaveCue = "Counter wave entered the line; hold frontline and answer with summon";
        [TextArea, SerializeField] private string counterWaveStabilizedCue = "Counter wave held by summon; rebuild the route opening";
        [TextArea, SerializeField] private string clearObjectiveCue = "Frontline stabilized; summon route secured";
        [TextArea, SerializeField] private string failObjectiveCue = "Player line collapsed before route stabilization";

        [Header("Result Copy")]
        [SerializeField] private string clearTitle = "FRONTLINE STABILIZED";
        [SerializeField] private string clearFollowupDetail = "Summon route analyzed; Skill1 follow-up confirmed";
        [SerializeField] private string clearCounterDetail = "Counter wave held; final follow-up confirmed";
        [SerializeField] private string clearPressureDetail = "Boss curtain suppressed; frontline route recorded";
        [SerializeField] private string failTitle = "LINE COLLAPSED";
        [SerializeField] private string failDetail = "Player down before the frontline route could stabilize";
        [SerializeField] private string routeCollapseFailDetail = "Route stability collapsed before the frontline could stabilize";
        [TextArea, SerializeField] private string cleanRouteRewardHook =
            "Clean route logged: summon screen created a Skill1 confirm before the counter wave arrived.";
        [TextArea, SerializeField] private string counterRecoveryRewardHook =
            "Counter recovery logged: summon restored a broken frontline and reopened the final strike window.";
        [TextArea, SerializeField] private string failedRouteRewardHook =
            "Failure analysis logged: route stability fell before the frontline answer was complete.";
        [TextArea, SerializeField] private string cleanRouteNextObjective =
            "Next run: keep the route clean by confirming before the counter wave enters.";
        [TextArea, SerializeField] private string counterRecoveryNextObjective =
            "Next run: answer the counter wave earlier so recovery becomes a clean summon route.";
        [TextArea, SerializeField] private string failedRouteNextObjective =
            "Next run: stop the close probe, build forward EN, then spend summon on the visible curtain.";

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
        public string RewardHook => rewardHook;
        public float RouteStabilityStart01 => Mathf.Clamp01(routeStabilityStart01);
        public float CloseProbeRouteDrainPerSecond => Mathf.Max(0f, closeProbeRouteDrainPerSecond);
        public float SummonAnswerRouteDrainPerSecond => Mathf.Max(0f, summonAnswerRouteDrainPerSecond);
        public float CounterWaveRouteDrainPerSecond => Mathf.Max(0f, counterWaveRouteDrainPerSecond);
        public float CloseProbeDefeatRouteBonus01 => Mathf.Clamp01(closeProbeDefeatRouteBonus01);
        public float SummonBlockRouteBonus01 => Mathf.Clamp01(summonBlockRouteBonus01);
        public float FollowupHitRouteBonus01 => Mathf.Clamp01(followupHitRouteBonus01);
        public float CounterWaveStabilizeRouteBonus01 => Mathf.Clamp01(counterWaveStabilizeRouteBonus01);
        public int ObjectiveStepCount => Mathf.Max(1, objectiveStepCount);
        public string StepPrefix => string.IsNullOrWhiteSpace(stepPrefix) ? "Route" : stepPrefix;
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
