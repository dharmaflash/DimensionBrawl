using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Linear Stage Template Profile", fileName = "DB_LinearStageTemplate")]
    public sealed class LinearStageTemplateProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string stageTemplateId = "S1-1.BreakGate";
        [SerializeField] private string displayName = "S1-1 Break Gate";
        [SerializeField] private LinearStageTemplateKind templateKind = LinearStageTemplateKind.TutorialRun;

        [Header("Stage Card Handoff")]
        [SerializeField, Min(0)] private int recommendedPowerTier;
        [SerializeField, Min(0f)] private float targetRunDurationSeconds = 180f;
        [SerializeField] private StageSummonNeed featuredSummonNeed = StageSummonNeed.Break;
        [SerializeField] private string combatLesson = "Break opens structure relief.";
        [SerializeField] private string masteryObjective = "Clear and solve the featured pressure lesson.";
        [SerializeField] private string rewardHook = "First-clear reward should teach one growth path.";

        [Header("Linear Route")]
        [SerializeField] private LinearStageSegmentProfile[] segments = Array.Empty<LinearStageSegmentProfile>();

        [Header("Boundaries")]
        [SerializeField] private string excludedScope = "No reward payout, runtime wave spawning, or summon implementation in this template.";

        public string StageTemplateId => stageTemplateId;
        public string DisplayName => displayName;
        public LinearStageTemplateKind TemplateKind => templateKind;
        public int RecommendedPowerTier => recommendedPowerTier;
        public float TargetRunDurationSeconds => targetRunDurationSeconds;
        public StageSummonNeed FeaturedSummonNeed => featuredSummonNeed;
        public string CombatLesson => combatLesson;
        public string MasteryObjective => masteryObjective;
        public string RewardHook => rewardHook;
        public string ExcludedScope => excludedScope;
        public int SegmentCount => segments != null ? segments.Length : 0;

        public LinearStageSegmentProfile GetSegment(int index)
        {
            if (segments == null || index < 0 || index >= segments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return segments[index];
        }
    }
}
