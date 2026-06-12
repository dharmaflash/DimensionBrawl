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
        [Tooltip("Queued combo input persists after this point so later hits do not feel like they drop buffered presses.")]
        [SerializeField, Min(0f)] private float comboQueueOpenAfterSeconds = 0.10f;
        [Tooltip("Starts the next combo hit part-way through recovery when queued. Keeps beat spacing inside the collected 0.28-0.55s range.")]
        [SerializeField, Range(0f, 1f)] private float comboChainRecoveryRatio = 0.45f;

        [Header("Dodge")]
        [Tooltip("Uses player_dodge_default totalDuration from collected combat feel data.")]
        [SerializeField] private float dodgeDurationSeconds = 0.56f;
        [SerializeField] private float dodgeInvulnerableFromSeconds = 0.05f;
        [SerializeField] private float dodgeInvulnerableToSeconds = 0.32f;
        [SerializeField] private float dodgeRecoverySeconds = 0.14f;
        [Tooltip("First-pass deviation: no collected dodge distance/speed value exists yet, so expose it.")]
        [SerializeField] private float dodgeSpeed = 10.2f;
        [SerializeField] private string dodgeTrigger = "DodgeForward";
        [SerializeField] private string dodgeBackTrigger = "DodgeBack";
        [SerializeField] private string dodgeLeftTrigger = "DodgeLeft";
        [SerializeField] private string dodgeRightTrigger = "DodgeRight";
        [SerializeField] private string dodgingParameter = "IsDodging";

        private readonly Collider[] hitBuffer = new Collider[64];
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
        private bool nextAttackQueued;

        public bool IsDodging => state == PlayerActionState.Dodging && actionTimer < dodgeDurationSeconds;
        public Vector3 LastDodgeDirection { get; private set; } = Vector3.forward;

        public event Action<int> BasicAttackStarted;
        public event Action<int> BasicAttackHit;
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
                TryQueueNextAttack();
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
            nextAttackQueued = false;
            TriggerAnimator(CurrentAttackStep().animationTrigger);
            BasicAttackStarted?.Invoke(comboIndex);
        }

        private void UpdateAttack(float deltaTime, bool dodgePressed)
        {
            AttackStep step = CurrentAttackStep();
            actionTimer += deltaTime;

            TryQueueNextAttack();

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

            float activeEnd = step.startupSeconds + step.activeSeconds;
            float chainStart = activeEnd + step.recoverySeconds * comboChainRecoveryRatio;
            bool canContinue = nextAttackQueued && comboIndex < basicCombo.Length - 1;
            if (canContinue && actionTimer >= chainStart)
            {
                StartAttack(comboIndex + 1);
                return;
            }

            float attackEnd = activeEnd + step.recoverySeconds;
            if (actionTimer < attackEnd)
            {
                return;
            }

            if (canContinue || (attackBufferTimer > 0f && comboIndex < basicCombo.Length - 1))
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
                Collider hitCollider = hitBuffer[i];
                if (hitCollider == null || hitCollider.transform.IsChildOf(transform))
                {
                    continue;
                }

                CombatHealth targetHealth = hitCollider.GetComponentInParent<CombatHealth>();
                if (targetHealth == null || targetHealth == health)
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
                BasicAttackHit?.Invoke(comboIndex);
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
            nextAttackQueued = false;

            Vector3 dodgeDirection = ResolveDodgeDirection();
            LastDodgeDirection = dodgeDirection.sqrMagnitude > 0f ? dodgeDirection.normalized : ResolvePlanarBack(transform.forward);
            if (movement != null)
            {
                movement.BeginExternalPlanarBurst(LastDodgeDirection * dodgeSpeed, dodgeDurationSeconds);
            }

            TriggerAnimator(ResolveDodgeTrigger(LastDodgeDirection));
            SetAnimatorBool(dodgingParameter, true);
            dodgeFeedbackActive = true;
            DodgeStarted?.Invoke();
        }

        private void TryQueueNextAttack()
        {
            if (state != PlayerActionState.Attacking || nextAttackQueued || comboIndex >= basicCombo.Length - 1)
            {
                return;
            }

            if (attackBufferTimer > 0f && actionTimer >= comboQueueOpenAfterSeconds)
            {
                nextAttackQueued = true;
                attackBufferTimer = 0f;
            }
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
                return ResolvePlanarBack(transform.forward);
            }

            if (movement.TryGetCurrentMoveDirection(out Vector3 currentMoveDirection))
            {
                if (currentMoveDirection.sqrMagnitude > 0.0001f)
                {
                    return currentMoveDirection.normalized;
                }

                Vector3 intentDirection = movement.MoveIntentDirection;
                if (intentDirection.sqrMagnitude > 0.0001f)
                {
                    return intentDirection.normalized;
                }
            }

            return ResolvePlanarBack(movement.FacingDirection);
        }

        private static Vector3 ResolvePlanarBack(Vector3 facingDirection)
        {
            Vector3 planarFacing = Vector3.ProjectOnPlane(facingDirection, Vector3.up);
            if (planarFacing.sqrMagnitude > 0.0001f)
            {
                return -planarFacing.normalized;
            }

            return Vector3.back;
        }

        private string ResolveDodgeTrigger(Vector3 dodgeDirection)
        {
            if (dodgeDirection.sqrMagnitude <= 0.0001f)
            {
                return dodgeTrigger;
            }

            Vector3 localDirection = transform.InverseTransformDirection(dodgeDirection.normalized);
            if (Mathf.Abs(localDirection.x) > Mathf.Abs(localDirection.z))
            {
                return localDirection.x < 0f ? dodgeLeftTrigger : dodgeRightTrigger;
            }

            return localDirection.z < 0f ? dodgeBackTrigger : dodgeTrigger;
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
