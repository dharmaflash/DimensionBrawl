using System;
using DimensionBrawl.Combat;
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
            public float forwardAdvanceDistance;
            public float forwardAdvanceDurationSeconds;
            public float damage;
            public float hitRadius;
            public float hitDistance;
            public float hitStopSeconds;
            public DamageResponsePolicy responsePolicy;
            public CombatControlLockPolicy controlLockPolicy;
        }

        public static DamageResponsePolicy ResolveResponsePolicy(AttackStep step, int stepIndex, int comboLength)
        {
            if (step.responsePolicy != DamageResponsePolicy.Default)
            {
                return step.responsePolicy;
            }

            return IsFinisherStep(stepIndex, comboLength)
                ? DamageResponsePolicy.Stagger
                : DamageResponsePolicy.FlashOnly;
        }

        public static CombatControlLockPolicy ResolveControlLockPolicy(AttackStep step, int stepIndex, int comboLength)
        {
            if (step.controlLockPolicy != CombatControlLockPolicy.None)
            {
                return step.controlLockPolicy;
            }

            return step.responsePolicy == DamageResponsePolicy.Default && IsFinisherStep(stepIndex, comboLength)
                ? CombatControlLockPolicy.InterruptAction
                : CombatControlLockPolicy.None;
        }

        private static bool IsFinisherStep(int stepIndex, int comboLength)
        {
            return comboLength <= 1 || stepIndex >= Mathf.Max(0, comboLength - 1);
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
                forwardAdvanceDistance = 0.36f,
                forwardAdvanceDurationSeconds = 0.14f,
                damage = 20f,
                hitRadius = 0.55f,
                hitDistance = 1.35f,
                hitStopSeconds = 0.03f,
                responsePolicy = DamageResponsePolicy.FlashOnly,
                controlLockPolicy = CombatControlLockPolicy.None
            },
            new AttackStep
            {
                animationTrigger = "Attack2",
                startupSeconds = 0.14f,
                activeSeconds = 0.09f,
                recoverySeconds = 0.32f,
                inputBufferSeconds = 0.10f,
                dodgeCancelAfterSeconds = 0.08f,
                forwardAdvanceDistance = 0.44f,
                forwardAdvanceDurationSeconds = 0.15f,
                damage = 24f,
                hitRadius = 0.6f,
                hitDistance = 1.45f,
                hitStopSeconds = 0.03f,
                responsePolicy = DamageResponsePolicy.FlashOnly,
                controlLockPolicy = CombatControlLockPolicy.None
            },
            new AttackStep
            {
                animationTrigger = "Attack3",
                startupSeconds = 0.16f,
                activeSeconds = 0.10f,
                recoverySeconds = 0.30f,
                inputBufferSeconds = 0.12f,
                dodgeCancelAfterSeconds = 0.10f,
                forwardAdvanceDistance = 0.52f,
                forwardAdvanceDurationSeconds = 0.16f,
                damage = 34f,
                hitRadius = 0.7f,
                hitDistance = 1.55f,
                hitStopSeconds = 0.04f,
                responsePolicy = DamageResponsePolicy.FlashOnly,
                controlLockPolicy = CombatControlLockPolicy.None
            },
            new AttackStep
            {
                animationTrigger = "Attack4",
                startupSeconds = 0.17f,
                activeSeconds = 0.10f,
                recoverySeconds = 0.34f,
                inputBufferSeconds = 0.12f,
                dodgeCancelAfterSeconds = 0.12f,
                forwardAdvanceDistance = 0.60f,
                forwardAdvanceDurationSeconds = 0.17f,
                damage = 40f,
                hitRadius = 0.72f,
                hitDistance = 1.62f,
                hitStopSeconds = 0.05f,
                responsePolicy = DamageResponsePolicy.FlashOnly,
                controlLockPolicy = CombatControlLockPolicy.None
            },
            new AttackStep
            {
                animationTrigger = "Attack5",
                startupSeconds = 0.20f,
                activeSeconds = 0.12f,
                recoverySeconds = 0.46f,
                inputBufferSeconds = 0.12f,
                dodgeCancelAfterSeconds = 0.14f,
                forwardAdvanceDistance = 0.74f,
                forwardAdvanceDurationSeconds = 0.20f,
                damage = 56f,
                hitRadius = 0.82f,
                hitDistance = 1.75f,
                hitStopSeconds = 0.05f,
                responsePolicy = DamageResponsePolicy.Stagger,
                controlLockPolicy = CombatControlLockPolicy.InterruptAction
            }
        };

        [SerializeField] private float comboResetSeconds = 0.75f;
        [SerializeField, Min(0f)] private float comboQueueOpenAfterSeconds = 0.10f;
        [SerializeField, Range(0f, 1f)] private float comboChainRecoveryRatio = 0.45f;

        [Header("Attack Aim")]
        [SerializeField, Min(0f)] private float attackFacingHoldPaddingSeconds = 0.06f;
        [SerializeField] private bool snapBasicAttackFacing = true;
        [Tooltip("Normal attacks keep stick intent for dodge/facing decisions, but should not let free locomotion slide the combo.")]
        [SerializeField, Range(0f, 1f)] private float basicAttackMoveInputSpeedScale = 0f;

        [Header("Dodge")]
        [SerializeField] private float dodgeDurationSeconds = 0.56f;
        [SerializeField] private float dodgeInvulnerableFromSeconds = 0.02f;
        [SerializeField] private float dodgeInvulnerableToSeconds = 0.40f;
        [SerializeField] private float dodgeRecoverySeconds = 0.14f;
        [SerializeField, Min(0f)] private float dodgeCooldownSeconds = 1.15f;
        [Tooltip("PGR-style armor after a successful perfect dodge, so overlapping follow-up hits do not punish the success.")]
        [SerializeField, Min(0f)] private float perfectDodgeProtectionSeconds = 0.65f;
        [Tooltip("Small timing grace around the authored invulnerability window to keep perfect dodge from feeling frame-tight.")]
        [SerializeField, Min(0f)] private float perfectDodgeTimingGraceSeconds = 0.08f;
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
        public float BasicAttackMoveInputSpeedScale => basicAttackMoveInputSpeedScale;
        public float DodgeDurationSeconds => dodgeDurationSeconds;
        public float DodgeInvulnerableFromSeconds => dodgeInvulnerableFromSeconds;
        public float DodgeInvulnerableToSeconds => dodgeInvulnerableToSeconds;
        public float DodgeRecoverySeconds => dodgeRecoverySeconds;
        public float DodgeCooldownSeconds => dodgeCooldownSeconds;
        public float PerfectDodgeProtectionSeconds => perfectDodgeProtectionSeconds;
        public float PerfectDodgeTimingGraceSeconds => perfectDodgeTimingGraceSeconds;
        public float DodgeSpeed => dodgeSpeed;
        public string DodgeTrigger => dodgeTrigger;
        public string DodgeBackTrigger => dodgeBackTrigger;
        public string DodgeLeftTrigger => dodgeLeftTrigger;
        public string DodgeRightTrigger => dodgeRightTrigger;
        public string DodgingParameter => dodgingParameter;
    }
}
