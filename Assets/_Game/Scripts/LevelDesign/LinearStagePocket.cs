using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [Serializable]
    public struct LinearStagePocket
    {
        [SerializeField] private string pocketId;
        [SerializeField] private EncounterPocketKind pocketKind;
        [SerializeField] private LinearStageObjectiveKind objectiveKind;
        [SerializeField, Min(0f)] private float targetDurationSeconds;
        [SerializeField, Range(0f, 1f)] private float targetIntensity;
        [SerializeField, Range(0f, 100f)] private float targetPeakWindowSharePct;
        [SerializeField, Range(0f, 100f)] private float targetTop3WindowSharePct;
        [SerializeField, Range(0f, 1f)] private float routeDominanceShare;
        [SerializeField, Range(0f, 1f)] private float entryExitLaneBias;
        [SerializeField, Min(0f)] private float timeToNextReliefWindowSeconds;
        [SerializeField, Range(0f, 1f)] private float riskDifferential;
        [SerializeField] private StageSummonNeed featuredSummonNeed;
        [SerializeField] private string objectiveCue;
        [SerializeField] private string designNotes;
        [SerializeField] private StageEnemyRoleSlot[] enemyRoles;

        public string PocketId => pocketId;
        public EncounterPocketKind PocketKind => pocketKind;
        public LinearStageObjectiveKind ObjectiveKind => objectiveKind;
        public float TargetDurationSeconds => targetDurationSeconds;
        public float TargetIntensity => targetIntensity;
        public float TargetPeakWindowSharePct => targetPeakWindowSharePct;
        public float TargetTop3WindowSharePct => targetTop3WindowSharePct;
        public float RouteDominanceShare => routeDominanceShare;
        public float EntryExitLaneBias => entryExitLaneBias;
        public float TimeToNextReliefWindowSeconds => timeToNextReliefWindowSeconds;
        public float RiskDifferential => riskDifferential;
        public StageSummonNeed FeaturedSummonNeed => featuredSummonNeed;
        public string ObjectiveCue => objectiveCue;
        public string DesignNotes => designNotes;
        public int EnemyRoleCount => enemyRoles != null ? enemyRoles.Length : 0;
        public bool AllowsNoEnemies => pocketKind == EncounterPocketKind.Relief;
        public bool HasObjective => objectiveKind != LinearStageObjectiveKind.None && !string.IsNullOrWhiteSpace(objectiveCue);

        public StageEnemyRoleSlot GetEnemyRole(int index)
        {
            if (enemyRoles == null || index < 0 || index >= enemyRoles.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return enemyRoles[index];
        }
    }
}
