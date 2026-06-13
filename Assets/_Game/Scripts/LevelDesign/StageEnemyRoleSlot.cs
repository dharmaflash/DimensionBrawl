using System;
using DimensionBrawl.AI;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [Serializable]
    public struct StageEnemyRoleSlot
    {
        [SerializeField] private CombatEnemyRoleProfile role;
        [SerializeField, Min(0)] private int minimumCount;
        [SerializeField, Min(0)] private int maximumCount;
        [SerializeField, Min(0f)] private float selectionWeight;
        [SerializeField] private StageSummonNeed suggestedAnswer;
        [SerializeField] private string placementHint;

        public CombatEnemyRoleProfile Role => role;
        public int MinimumCount => minimumCount;
        public int MaximumCount => maximumCount;
        public float SelectionWeight => selectionWeight;
        public StageSummonNeed SuggestedAnswer => suggestedAnswer;
        public string PlacementHint => placementHint;
        public bool HasRole => role != null;
        public bool HasValidCountRange => minimumCount >= 0 && maximumCount >= minimumCount;
    }
}
