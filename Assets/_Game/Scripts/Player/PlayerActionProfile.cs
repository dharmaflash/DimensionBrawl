using System;
using UnityEngine;

namespace DimensionBrawl.Player
{
    [CreateAssetMenu(menuName = "DimensionBrawl/Profiles/Player Action Profile")]
    public sealed class PlayerActionProfile : ScriptableObject
    {
        [Serializable]
        public struct AttackStep
        {
            public string animationTrigger;
            public float startupSeconds;
            public float activeSeconds;
            public float recoverySeconds;
            public float inputBufferSeconds;
            public float dodgeCancelAfterSeconds;
            public float damage;
            public float hitRadius;
            public float hitDistance;
            public float hitStopSeconds;
        }

        [Header("Basic Attack")]
        [SerializeField] private AttackStep[] basicCombo =
        {
            new AttackStep
            {
                animationTrigger = "Attack1",
                startupSeconds = 0.12f,
                activeSeconds = 0.08f,
                recoverySeconds = 0.28f,
                inputBufferSeconds = 0.10f,
                dodgeCancelAfterSeconds = 0.06f,
                damage = 20f,
                hitRadius = 0.55f,
                hitDistance = 1.35f,
                hitStopSeconds = 0.03f
            },
            new AttackStep
            {
                animationTrigger = "Attack2",
                startupSeconds = 0.14f,
                activeSeconds = 0.09f,
                recoverySeconds = 0.32f,
                inputBufferSeconds = 0.10f,
                dodgeCancelAfterSeconds = 0.08f,
                damage = 24f,
                hitRadius = 0.6f,
                hitDistance = 1.45f,
                hitStopSeconds = 0.03f
            },
            new AttackStep
            {
                animationTrigger = "Attack3",
                startupSeconds = 0.16f,
                activeSeconds = 0.10f,
                recoverySeconds = 0.30f,
                inputBufferSeconds = 0.12f,
                dodgeCancelAfterSeconds = 0.10f,
                damage = 34f,
                hitRadius = 0.7f,
                hitDistance = 1.55f,
                hitStopSeconds = 0.04f
            },
            new AttackStep
            {
                animationTrigger = "Attack4",
                startupSeconds = 0.17f,
                activeSeconds = 0.10f,
                recoverySeconds = 0.34f,
                inputBufferSeconds = 0.12f,
                dodgeCancelAfterSeconds = 0.12f,
                damage = 40f,
                hitRadius = 0.72f,
                hitDistance = 1.62f,
                hitStopSeconds = 0.05f
            },
            new AttackStep
            {
                animationTrigger = "Attack5",
                startupSeconds = 0.20f,
                activeSeconds = 0.12f,
                recoverySeconds = 0.46f,
                inputBufferSeconds = 0.12f,
                dodgeCancelAfterSeconds = 0.14f,
                damage = 56f,
                hitRadius = 0.82f,
                hitDistance = 1.75f,
                hitStopSeconds = 0.05f
            }
        };

        [SerializeField] private float comboResetSeconds = 0.75f;
        [SerializeField, Min(0f)] private float comboQueueOpenAfterSeconds = 0.10f;
        [SerializeField, Range(0f, 1f)] private float comboChainRecoveryRatio = 0.45f;

        [Header("Attack Aim")]
        [SerializeField, Min(0f)] private float attackFacingHoldPaddingSeconds = 0.06f;
        [SerializeField] private bool snapBasicAttackFacing = true;

        [Header("Dodge")]
        [SerializeField] private float dodgeDurationSeconds = 0.56f;
        [SerializeField] private float dodgeInvulnerableFromSeconds = 0.05f;
        [SerializeField] private float dodgeInvulnerableToSeconds = 0.32f;
        [SerializeField] private float dodgeRecoverySeconds = 0.14f;
        [SerializeField] private float dodgeSpeed = 10.2f;
        [SerializeField] private string dodgeTrigger = "DodgeForward";
        [SerializeField] private string dodgeBackTrigger = "DodgeBack";
        [SerializeField] private string dodgeLeftTrigger = "DodgeLeft";
        [SerializeField] private string dodgeRightTrigger = "DodgeRight";
        [SerializeField] private string dodgingParameter = "IsDodging";

        public AttackStep[] BasicCombo => basicCombo;
        public float ComboResetSeconds => comboResetSeconds;
        public float ComboQueueOpenAfterSeconds => comboQueueOpenAfterSeconds;
        public float ComboChainRecoveryRatio => comboChainRecoveryRatio;
        public float AttackFacingHoldPaddingSeconds => attackFacingHoldPaddingSeconds;
        public bool SnapBasicAttackFacing => snapBasicAttackFacing;
        public float DodgeDurationSeconds => dodgeDurationSeconds;
        public float DodgeInvulnerableFromSeconds => dodgeInvulnerableFromSeconds;
        public float DodgeInvulnerableToSeconds => dodgeInvulnerableToSeconds;
        public float DodgeRecoverySeconds => dodgeRecoverySeconds;
        public float DodgeSpeed => dodgeSpeed;
        public string DodgeTrigger => dodgeTrigger;
        public string DodgeBackTrigger => dodgeBackTrigger;
        public string DodgeLeftTrigger => dodgeLeftTrigger;
        public string DodgeRightTrigger => dodgeRightTrigger;
        public string DodgingParameter => dodgingParameter;
    }
}
