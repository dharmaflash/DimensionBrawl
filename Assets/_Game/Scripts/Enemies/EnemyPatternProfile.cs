using UnityEngine;

namespace DimensionBrawl.Enemies
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Enemy Pattern Profile")]
    public sealed class EnemyPatternProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyTypeId = "SciFiSoldier.Basic";
        [SerializeField] private string patternId = "ClosePunish";

        [Header("Movement")]
        [SerializeField, Min(0f)] private float approachSpeed = 2.7f;
        [SerializeField, Min(0f)] private float turnRateDegrees = 540f;
        [SerializeField] private float gravity = -24f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float attackRange = 1.65f;
        [SerializeField, Min(0f)] private float telegraphSeconds = 0.65f;
        [SerializeField, Min(0f)] private float activeSeconds = 0.14f;
        [SerializeField, Min(0f)] private float recoverySeconds = 0.45f;
        [SerializeField, Min(0f)] private float damage = 15f;
        [SerializeField, Min(0f)] private float hitStopSeconds = 0.03f;

        [Header("Hit Reaction")]
        [SerializeField, Min(0f)] private float hitReactionSeconds = 0.24f;
        [SerializeField, Min(0f)] private float knockbackSpeed = 2f;

        [Header("Animation Requests")]
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deathTrigger = "Death";

        public string EnemyTypeId => enemyTypeId;
        public string PatternId => patternId;
        public float ApproachSpeed => approachSpeed;
        public float TurnRateDegrees => turnRateDegrees;
        public float Gravity => gravity;
        public float AttackRange => attackRange;
        public float TelegraphSeconds => telegraphSeconds;
        public float ActiveSeconds => activeSeconds;
        public float RecoverySeconds => recoverySeconds;
        public float Damage => damage;
        public float HitStopSeconds => hitStopSeconds;
        public float HitReactionSeconds => hitReactionSeconds;
        public float KnockbackSpeed => knockbackSpeed;
        public string MoveSpeedParameter => moveSpeedParameter;
        public string AttackTrigger => attackTrigger;
        public string HitTrigger => hitTrigger;
        public string DeathTrigger => deathTrigger;
    }
}
