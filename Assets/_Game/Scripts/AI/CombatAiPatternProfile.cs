using UnityEngine;

namespace DimensionBrawl.AI
{
    public abstract class CombatAiPatternProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string actorTypeId = "SciFiSoldier.Basic";
        [SerializeField] private string patternId = "ClosePunish";

        [Header("Movement")]
        [SerializeField, Min(0f)] private float approachSpeed = 2.7f;
        [SerializeField, Min(0f)] private float turnRateDegrees = 540f;
        [SerializeField] private float gravity = -24f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float attackRange = 1.65f;
        [SerializeField, Range(-1f, 1f)] private float attackFacingDotThreshold = -0.15f;
        [SerializeField, Min(0f)] private float telegraphSeconds = 0.65f;
        [SerializeField, Min(0f)] private float activeSeconds = 0.14f;
        [SerializeField, Min(0f)] private float activeLungeSpeed = 0f;
        [SerializeField, Min(0f)] private float recoverySeconds = 0.45f;
        [SerializeField, Min(0f)] private float damage = 15f;
        [SerializeField, Min(0f)] private float hitStopSeconds = 0.03f;

        [Header("Hit Reaction")]
        [SerializeField, Min(0f)] private float hitReactionSeconds = 0.24f;
        [SerializeField, Min(0f)] private float knockbackSpeed = 2f;

        [Header("Recovery Movement")]
        [SerializeField, Min(0f)] private float recoveryRetreatSpeed = 0f;
        [SerializeField, Min(0f)] private float recoveryRetreatSeconds = 0f;

        [Header("Readability")]
        [SerializeField] private CombatAiCameraCueKind cameraCueKind = CombatAiCameraCueKind.ClosePunish;
        [SerializeField, Min(0f)] private float windupThreatLevel = 1f;
        [SerializeField, Min(0f)] private float activeCameraCueStrength = 1f;
        [SerializeField, Min(0f)] private float deathCameraCueStrength = 0.6f;

        [Header("Telegraph Presentation")]
        [SerializeField] private Vector3 telegraphWindupStartScale = new Vector3(0.35f, 0.02f, 0.65f);
        [SerializeField] private Vector3 telegraphWindupEndScale = new Vector3(1.05f, 0.02f, 1.55f);
        [SerializeField] private Vector3 telegraphActiveScale = new Vector3(1.25f, 0.025f, 1.8f);
        [SerializeField] private Vector3 windupPoseOffset = new Vector3(0f, 0f, -0.08f);
        [SerializeField] private Vector3 activePoseOffset = new Vector3(0f, 0f, 0.12f);
        [SerializeField] private Color windupStartColor = new Color(1f, 0.45f, 0.08f, 1f);
        [SerializeField] private Color windupEndColor = new Color(1f, 0.08f, 0.02f, 1f);
        [SerializeField] private Color activeColor = Color.white;

        [Header("Animation Requests")]
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deathTrigger = "Death";

        public string ActorTypeId => actorTypeId;
        public string PatternId => patternId;
        public float ApproachSpeed => approachSpeed;
        public float TurnRateDegrees => turnRateDegrees;
        public float Gravity => gravity;
        public float AttackRange => attackRange;
        public float AttackFacingDotThreshold => attackFacingDotThreshold;
        public float TelegraphSeconds => telegraphSeconds;
        public float ActiveSeconds => activeSeconds;
        public float ActiveLungeSpeed => activeLungeSpeed;
        public float RecoverySeconds => recoverySeconds;
        public float Damage => damage;
        public float HitStopSeconds => hitStopSeconds;
        public float HitReactionSeconds => hitReactionSeconds;
        public float KnockbackSpeed => knockbackSpeed;
        public float RecoveryRetreatSpeed => recoveryRetreatSpeed;
        public float RecoveryRetreatSeconds => recoveryRetreatSeconds;
        public CombatAiCameraCueKind CameraCueKind => cameraCueKind;
        public float WindupThreatLevel => windupThreatLevel;
        public float ActiveCameraCueStrength => activeCameraCueStrength;
        public float DeathCameraCueStrength => deathCameraCueStrength;
        public Vector3 TelegraphWindupStartScale => telegraphWindupStartScale;
        public Vector3 TelegraphWindupEndScale => telegraphWindupEndScale;
        public Vector3 TelegraphActiveScale => telegraphActiveScale;
        public Vector3 WindupPoseOffset => windupPoseOffset;
        public Vector3 ActivePoseOffset => activePoseOffset;
        public Color WindupStartColor => windupStartColor;
        public Color WindupEndColor => windupEndColor;
        public Color ActiveColor => activeColor;
        public string MoveSpeedParameter => moveSpeedParameter;
        public string AttackTrigger => attackTrigger;
        public string HitTrigger => hitTrigger;
        public string DeathTrigger => deathTrigger;
    }
}
