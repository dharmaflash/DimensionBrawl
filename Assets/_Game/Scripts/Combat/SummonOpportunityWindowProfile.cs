using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum SummonOpportunityTrigger
    {
        CloseThreatCleared = 0,
        BossPatternBreak = 1,
        PressureDanger = 2,
        StructureBreak = 3,
        PerfectDodge = 4
    }

    [CreateAssetMenu(
        fileName = "DB_SummonOpportunityWindow",
        menuName = "DimensionBrawl/Combat/Summon Opportunity Window")]
    public sealed class SummonOpportunityWindowProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string windowId = "BossPressureBlock";
        [SerializeField] private SummonOpportunityTrigger trigger = SummonOpportunityTrigger.CloseThreatCleared;
        [SerializeField] private string primaryAnswerAction = "SummonSlot1";
        [SerializeField] private string followupAction = "Skill1";

        [Header("Readability")]
        [SerializeField] private string readyCue = "Prepare SummonSlot1 block";
        [SerializeField] private string chargeCue = "Forward EN now";
        [SerializeField] private string payoffCue = "Skill1 follow-up";

        [Header("Opportunity Timing")]
        [SerializeField, Min(0f)] private float opportunityCueSeconds = 1.35f;
        [SerializeField, Min(0f)] private float pressureBreakSeconds = 3f;
        [SerializeField, Min(0f)] private float followupWindowSeconds = 2.1f;
        [SerializeField, Min(0f)] private float followupEnergyPulse = 125f;

        [Header("Tier Scaling")]
        [SerializeField, Min(0f)] private float pressureBreakTierTwoBonusSeconds = 0.35f;
        [SerializeField, Min(0f)] private float pressureBreakTierThreeBonusSeconds = 0.6f;
        [SerializeField, Min(0f)] private float followupWindowTierTwoBonusSeconds = 0.35f;
        [SerializeField, Min(0f)] private float followupWindowTierThreeBonusSeconds = 0.75f;
        [SerializeField, Min(0f)] private float followupEnergyPulseTierTwo = 185f;
        [SerializeField, Min(0f)] private float followupEnergyPulseTierThree = 240f;

        public string WindowId => windowId;
        public SummonOpportunityTrigger Trigger => trigger;
        public string PrimaryAnswerAction => primaryAnswerAction;
        public string FollowupAction => followupAction;
        public string ReadyCue => readyCue;
        public string ChargeCue => chargeCue;
        public string PayoffCue => payoffCue;
        public float OpportunityCueSeconds => opportunityCueSeconds;
        public float PressureBreakSeconds => pressureBreakSeconds;
        public float FollowupWindowSeconds => followupWindowSeconds;
        public float FollowupEnergyPulse => followupEnergyPulse;

        public float ResolvePressureBreakSeconds(int tier)
        {
            return Mathf.Max(0f, pressureBreakSeconds + ResolvePressureBreakBonusSeconds(tier));
        }

        public float ResolveFollowupWindowSeconds(int tier)
        {
            return Mathf.Max(0f, followupWindowSeconds + ResolveFollowupWindowBonusSeconds(tier));
        }

        public float ResolveFollowupEnergyPulse(int tier)
        {
            return tier switch
            {
                >= 3 => followupEnergyPulseTierThree,
                2 => followupEnergyPulseTierTwo,
                _ => followupEnergyPulse
            };
        }

        private float ResolvePressureBreakBonusSeconds(int tier)
        {
            return tier switch
            {
                >= 3 => pressureBreakTierThreeBonusSeconds,
                2 => pressureBreakTierTwoBonusSeconds,
                _ => 0f
            };
        }

        private float ResolveFollowupWindowBonusSeconds(int tier)
        {
            return tier switch
            {
                >= 3 => followupWindowTierThreeBonusSeconds,
                2 => followupWindowTierTwoBonusSeconds,
                _ => 0f
            };
        }
    }
}
