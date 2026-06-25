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

        [Header("Identity")]
        [SerializeField] private string stageId = "FRONTLINE-MOTIVATION-REVIEW-01";
        [SerializeField] private string displayName = "Frontline Motivation Review";
        [SerializeField] private string stageEpisodeLabel = "EP 03 Frontline Stabilization";
        [SerializeField] private string objectiveBadgeLabel = "LANE";

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
        [TextArea, SerializeField] private string clearObjectiveCue = "Frontline stabilized; summon route secured";
        [TextArea, SerializeField] private string failObjectiveCue = "Player line collapsed before route stabilization";

        [Header("Result Copy")]
        [SerializeField] private string clearTitle = "FRONTLINE STABILIZED";
        [SerializeField] private string clearFollowupDetail = "Summon route analyzed; Skill1 follow-up confirmed";
        [SerializeField] private string clearPressureDetail = "Boss curtain suppressed; frontline route recorded";
        [SerializeField] private string failTitle = "LINE COLLAPSED";
        [SerializeField] private string failDetail = "Player down before the frontline route could stabilize";

        [Header("Review Evidence")]
        [SerializeField] private StageBeat[] beats = Array.Empty<StageBeat>();
        [SerializeField] private SourceReference[] sourceReferences = Array.Empty<SourceReference>();

        public string StageId => stageId;
        public string DisplayName => displayName;
        public string StageEpisodeLabel => stageEpisodeLabel;
        public string ObjectiveBadgeLabel => objectiveBadgeLabel;
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
        public string ClearObjectiveCue => clearObjectiveCue;
        public string FailObjectiveCue => failObjectiveCue;
        public string ClearTitle => clearTitle;
        public string ClearFollowupDetail => clearFollowupDetail;
        public string ClearPressureDetail => clearPressureDetail;
        public string FailTitle => failTitle;
        public string FailDetail => failDetail;
        public int BeatCount => beats != null ? beats.Length : 0;
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

        public string SelectText(string profileText, string fallback)
        {
            return string.IsNullOrWhiteSpace(profileText) ? fallback : profileText;
        }
    }
}
