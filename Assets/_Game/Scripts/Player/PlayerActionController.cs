using System;
using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    public sealed class PlayerActionController : MonoBehaviour
    {
        [System.Serializable]
        private struct AttackStep
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

        private enum PlayerActionState
        {
            Free,
            Attacking,
            Dodging
        }

        [Header("Input")]
        [SerializeField] private InputActionReference basicAttackAction;
        [SerializeField] private InputActionReference dodgeAction;
        [Tooltip("Editor test fallback only. Serialized InputActionReference still takes priority when assigned.")]
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;

        [Header("Combat References")]
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private CombatHealth health;
        [SerializeField] private LayerMask hittableLayers = ~0;
        [SerializeField] private Animator animator;

        [Header("Basic Attack")]
        [Tooltip("Defaults use documented startup/active/recovery/hit-stop ranges from Combat Feel Frame Reference.")]
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
                startupSeconds = 0.18f,
                activeSeconds = 0.10f,
                recoverySeconds = 0.42f,
                inputBufferSeconds = 0.10f,
                dodgeCancelAfterSeconds = 0.10f,
                damage = 34f,
                hitRadius = 0.7f,
                hitDistance = 1.55f,
                hitStopSeconds = 0.05f
            }
        };

        [SerializeField] private float comboResetSeconds = 0.75f;

        [Header("Dodge")]
        [Tooltip("Uses player_dodge_default totalDuration from collected combat feel data.")]
        [SerializeField] private float dodgeDurationSeconds = 0.62f;
        [SerializeField] private float dodgeInvulnerableFromSeconds = 0.05f;
        [SerializeField] private float dodgeInvulnerableToSeconds = 0.40f;
        [SerializeField] private float dodgeRecoverySeconds = 0.18f;
        [Tooltip("First-pass deviation: no collected dodge distance/speed value exists yet, so expose it.")]
        [SerializeField] private float dodgeSpeed = 8.5f;
        [SerializeField] private string dodgeTrigger = "Dodge";
        [SerializeField] private string dodgingParameter = "IsDodging";

        private readonly Collider[] hitBuffer = new Collider[16];
        private PlayerActionState state;
        private int comboIndex;
        private float actionTimer;
        private float attackBufferTimer;
        private float comboResetTimer;
        private bool attackHasHit;
        private bool mobileAttackQueued;
        private bool mobileDodgeQueued;
        private bool enabledAttackAction;
        private bool enabledDodgeAction;
        private bool dodgeFeedbackActive;

        public bool IsDodging => state == PlayerActionState.Dodging && actionTimer < dodgeDurationSeconds;

        public event Action DodgeStarted;
        public event Action DodgeEnded;

        public void QueueBasicAttack()
        {
            mobileAttackQueued = true;
        }

        public void QueueDodge()
        {
            mobileDodgeQueued = true;
        }

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<PlayerMovementController>();
            }

            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }
        }

        private void OnEnable()
        {
            enabledAttackAction = EnableActionIfNeeded(basicAttackAction);
            enabledDodgeAction = EnableActionIfNeeded(dodgeAction);
        }

        private void OnDisable()
        {
            EndDodgeFeedbackIfNeeded();
            DisableActionIfOwned(basicAttackAction, enabledAttackAction);
            DisableActionIfOwned(dodgeAction, enabledDodgeAction);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            bool attackPressed = ReadAttackPressed();
            bool dodgePressed = ReadDodgePressed();

            if (attackBufferTimer > 0f)
            {
                attackBufferTimer = Mathf.Max(0f, attackBufferTimer - deltaTime);
            }

            if (comboResetTimer > 0f)
            {
                comboResetTimer = Mathf.Max(0f, comboResetTimer - deltaTime);
            }
            else if (state == PlayerActionState.Free)
            {
                comboIndex = 0;
            }

            if (attackPressed)
            {
                attackBufferTimer = CurrentAttackStep().inputBufferSeconds;
            }

            if (dodgePressed && CanStartDodge())
            {
                StartDodge();
                return;
            }

            switch (state)
            {
                case PlayerActionState.Free:
                    if (attackBufferTimer > 0f)
                    {
                        StartAttack(comboIndex);
                    }
                    break;
                case PlayerActionState.Attacking:
                    UpdateAttack(deltaTime, dodgePressed);
                    break;
                case PlayerActionState.Dodging:
                    UpdateDodge(deltaTime);
                    break;
            }
        }

        private void StartAttack(int index)
        {
            state = PlayerActionState.Attacking;
            comboIndex = Mathf.Clamp(index, 0, Mathf.Max(0, basicCombo.Length - 1));
            actionTimer = 0f;
            attackHasHit = false;
            attackBufferTimer = 0f;
            TriggerAnimator(CurrentAttackStep().animationTrigger);
        }

        private void UpdateAttack(float deltaTime, bool dodgePressed)
        {
            AttackStep step = CurrentAttackStep();
            actionTimer += deltaTime;

            if (dodgePressed && actionTimer >= step.dodgeCancelAfterSeconds)
            {
                StartDodge();
                return;
            }

            if (!attackHasHit && actionTimer >= step.startupSeconds)
            {
                attackHasHit = true;
                TryApplyAttackHit(step);
            }

            float attackEnd = step.startupSeconds + step.activeSeconds + step.recoverySeconds;
            if (actionTimer < attackEnd)
            {
                return;
            }

            bool canContinue = attackBufferTimer > 0f && comboIndex < basicCombo.Length - 1;
            if (canContinue)
            {
                StartAttack(comboIndex + 1);
                return;
            }

            comboResetTimer = comboResetSeconds;
            state = PlayerActionState.Free;
        }

        private void TryApplyAttackHit(AttackStep step)
        {
            Vector3 direction = movement != null ? movement.FacingDirection : transform.forward;
            Vector3 hitCenter = transform.position + Vector3.up * 1f + direction.normalized * step.hitDistance;
            int hitCount = Physics.OverlapSphereNonAlloc(hitCenter, step.hitRadius, hitBuffer, hittableLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (!hitBuffer[i].TryGetComponent(out CombatHealth targetHealth) || targetHealth == health)
                {
                    continue;
                }

                Vector3 hitDirection = Vector3.ProjectOnPlane(targetHealth.transform.position - transform.position, Vector3.up).normalized;
                DamageInfo damageInfo = new DamageInfo(
                    health,
                    health != null ? health.Team : DamageTeam.Player,
                    step.damage,
                    hitCenter,
                    hitDirection.sqrMagnitude > 0f ? hitDirection : direction,
                    step.hitStopSeconds);

                targetHealth.TryApplyDamage(damageInfo);
                return;
            }
        }

        private bool CanStartDodge()
        {
            if (state == PlayerActionState.Dodging)
            {
                return false;
            }

            if (state != PlayerActionState.Attacking)
            {
                return true;
            }

            return actionTimer >= CurrentAttackStep().dodgeCancelAfterSeconds;
        }

        private void StartDodge()
        {
            state = PlayerActionState.Dodging;
            actionTimer = 0f;
            attackBufferTimer = 0f;
            comboResetTimer = 0f;
            comboIndex = 0;

            Vector3 dodgeDirection = ResolveDodgeDirection();
            if (movement != null)
            {
                movement.BeginExternalPlanarBurst(dodgeDirection.normalized * dodgeSpeed, dodgeDurationSeconds);
            }

            TriggerAnimator(dodgeTrigger);
            SetAnimatorBool(dodgingParameter, true);
            dodgeFeedbackActive = true;
            DodgeStarted?.Invoke();
        }

        private void UpdateDodge(float deltaTime)
        {
            actionTimer += deltaTime;

            if (health != null && actionTimer >= dodgeInvulnerableFromSeconds && actionTimer <= dodgeInvulnerableToSeconds)
            {
                health.SetInvulnerableUntil(Time.time + Mathf.Max(0f, dodgeInvulnerableToSeconds - actionTimer));
            }

            if (actionTimer >= dodgeDurationSeconds)
            {
                EndDodgeFeedbackIfNeeded();
            }

            if (actionTimer < dodgeDurationSeconds + dodgeRecoverySeconds)
            {
                return;
            }

            SetAnimatorBool(dodgingParameter, false);
            state = PlayerActionState.Free;
        }

        private Vector3 ResolveDodgeDirection()
        {
            if (movement == null)
            {
                return transform.forward;
            }

            Vector3 intentDirection = movement.MoveIntentDirection;
            if (intentDirection.sqrMagnitude > 0f)
            {
                return intentDirection;
            }

            Vector3 planarVelocity = Vector3.ProjectOnPlane(movement.PlanarVelocity, Vector3.up);
            if (planarVelocity.sqrMagnitude > 0.01f)
            {
                return planarVelocity.normalized;
            }

            return movement.FacingDirection;
        }

        private void EndDodgeFeedbackIfNeeded()
        {
            if (!dodgeFeedbackActive)
            {
                return;
            }

            dodgeFeedbackActive = false;
            DodgeEnded?.Invoke();
        }

        private AttackStep CurrentAttackStep()
        {
            if (basicCombo == null || basicCombo.Length == 0)
            {
                return new AttackStep
                {
                    startupSeconds = 0.12f,
                    activeSeconds = 0.08f,
                    recoverySeconds = 0.28f,
                    inputBufferSeconds = 0.10f,
                    dodgeCancelAfterSeconds = 0.06f,
                    damage = 10f,
                    hitRadius = 0.5f,
                    hitDistance = 1.2f,
                    hitStopSeconds = 0.03f
                };
            }

            return basicCombo[Mathf.Clamp(comboIndex, 0, basicCombo.Length - 1)];
        }

        private static bool EnableActionIfNeeded(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null || actionReference.action.enabled)
            {
                return false;
            }

            actionReference.action.Enable();
            return true;
        }

        private static void DisableActionIfOwned(InputActionReference actionReference, bool enabledHere)
        {
            if (enabledHere && actionReference != null && actionReference.action != null)
            {
                actionReference.action.Disable();
            }
        }

        private static bool ReadButtonDown(InputActionReference actionReference, ref bool mobileQueued)
        {
            bool pressed = mobileQueued;
            mobileQueued = false;

            if (actionReference != null && actionReference.action != null)
            {
                pressed |= actionReference.action.WasPressedThisFrame();
            }

            return pressed;
        }

        private bool ReadAttackPressed()
        {
            bool pressed = ReadButtonDown(basicAttackAction, ref mobileAttackQueued);
            if (pressed || !useDeviceFallbackWhenActionMissing || !IsActionMissing(basicAttackAction))
            {
                return pressed;
            }

            return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
        }

        private bool ReadDodgePressed()
        {
            bool pressed = ReadButtonDown(dodgeAction, ref mobileDodgeQueued);
            if (pressed || !useDeviceFallbackWhenActionMissing || !IsActionMissing(dodgeAction))
            {
                return pressed;
            }

            return (Keyboard.current != null
                    && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.leftShiftKey.wasPressedThisFrame))
                || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        }

        private static bool IsActionMissing(InputActionReference actionReference)
        {
            return actionReference == null || actionReference.action == null;
        }

        private void TriggerAnimator(string triggerName)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
            {
                animator.SetTrigger(triggerName);
            }
        }

        private void SetAnimatorBool(string parameterName, bool value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }
    }
}
