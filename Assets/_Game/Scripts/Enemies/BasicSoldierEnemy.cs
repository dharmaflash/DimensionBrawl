using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatHealth))]
    public sealed class BasicSoldierEnemy : MonoBehaviour
    {
        private enum SoldierState
        {
            Approach,
            Telegraph,
            Active,
            Recovery,
            Stagger,
            Dead
        }

        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private CombatHealth targetHealth;
        [SerializeField] private CombatHealth selfHealth;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject telegraphIndicator;
        [SerializeField] private Renderer bodyRenderer;

        [Header("Movement")]
        [Tooltip("First-pass deviation: no collected soldier approach speed exists yet, so this stays Inspector-visible.")]
        [SerializeField, Min(0f)] private float approachSpeed = 2.7f;
        [SerializeField, Min(0f)] private float turnRateDegrees = 540f;
        [SerializeField] private float gravity = -24f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float attackRange = 1.65f;
        [Tooltip("Uses the collected minor projectile/readable enemy telegraph range of 0.45-0.9 seconds.")]
        [SerializeField, Min(0f)] private float telegraphSeconds = 0.65f;
        [Tooltip("Uses the collected active-window range of 0.04-0.45 seconds.")]
        [SerializeField, Min(0f)] private float activeSeconds = 0.14f;
        [Tooltip("Uses the collected enemy pattern recovery range of 0.35-1.0 seconds.")]
        [SerializeField, Min(0f)] private float recoverySeconds = 0.45f;
        [SerializeField, Min(0f)] private float damage = 15f;
        [SerializeField, Min(0f)] private float hitStopSeconds = 0.03f;

        [Header("Hit Reaction")]
        [Tooltip("Uses the collected light enemy stagger range of 0.18-0.35 seconds.")]
        [SerializeField, Min(0f)] private float hitReactionSeconds = 0.24f;
        [Tooltip("First-pass deviation: no collected soldier knockback speed exists yet, so this stays Inspector-visible.")]
        [SerializeField, Min(0f)] private float knockbackSpeed = 2f;

        [Header("Animation Requests")]
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deathTrigger = "Death";

        [Header("Readable Prototype Colors")]
        [SerializeField] private string colorProperty = "_BaseColor";
        [SerializeField] private Color normalColor = new Color(0.55f, 0.7f, 0.9f);
        [SerializeField] private Color telegraphColor = new Color(1f, 0.65f, 0.2f);
        [SerializeField] private Color staggerColor = new Color(1f, 0.25f, 0.2f);
        [SerializeField] private Color deadColor = new Color(0.2f, 0.2f, 0.2f);

        private MaterialPropertyBlock propertyBlock;
        private SoldierState state;
        private Vector3 knockbackVelocity;
        private float stateTimer;
        private float verticalVelocity;
        private bool dealtDamageThisSwing;

        public void ConfigureTarget(Transform newTarget, CombatHealth newTargetHealth)
        {
            target = newTarget;
            targetHealth = newTargetHealth;
        }

        private void Awake()
        {
            if (selfHealth == null)
            {
                selfHealth = GetComponent<CombatHealth>();
            }

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (selfHealth != null)
            {
                selfHealth.Damaged += HandleDamaged;
                selfHealth.Died += HandleDied;
            }

            SetTelegraphVisible(false);
            SetBodyColor(normalColor);
        }

        private void OnDisable()
        {
            if (selfHealth != null)
            {
                selfHealth.Damaged -= HandleDamaged;
                selfHealth.Died -= HandleDied;
            }
        }

        private void Update()
        {
            if (state == SoldierState.Dead || target == null || targetHealth == null || !targetHealth.IsAlive)
            {
                UpdateAnimation(0f);
                return;
            }

            float deltaTime = Time.deltaTime;

            switch (state)
            {
                case SoldierState.Approach:
                    UpdateApproach(deltaTime);
                    break;
                case SoldierState.Telegraph:
                    UpdateTelegraph(deltaTime);
                    break;
                case SoldierState.Active:
                    UpdateActive(deltaTime);
                    break;
                case SoldierState.Recovery:
                    UpdateRecovery(deltaTime);
                    break;
                case SoldierState.Stagger:
                    UpdateStagger(deltaTime);
                    break;
            }
        }

        private void UpdateApproach(float deltaTime)
        {
            FaceTarget(deltaTime);

            if (IsTargetInAttackRange())
            {
                BeginTelegraph();
                return;
            }

            Vector3 direction = DirectionToTarget();
            Move(direction * approachSpeed, deltaTime);
            UpdateAnimation(approachSpeed);
            SetBodyColor(normalColor);
        }

        private void BeginTelegraph()
        {
            state = SoldierState.Telegraph;
            stateTimer = 0f;
            dealtDamageThisSwing = false;
            SetTelegraphVisible(true);
            SetBodyColor(telegraphColor);
        }

        private void UpdateTelegraph(float deltaTime)
        {
            stateTimer += deltaTime;
            FaceTarget(deltaTime);
            Move(Vector3.zero, deltaTime);
            UpdateAnimation(0f);

            if (stateTimer < telegraphSeconds)
            {
                return;
            }

            state = SoldierState.Active;
            stateTimer = 0f;
            SetTelegraphVisible(false);
            TriggerAnimator(attackTrigger);
        }

        private void UpdateActive(float deltaTime)
        {
            stateTimer += deltaTime;
            FaceTarget(deltaTime);
            Move(Vector3.zero, deltaTime);

            if (!dealtDamageThisSwing && IsTargetInAttackRange())
            {
                dealtDamageThisSwing = true;
                ApplyDamageToTarget();
            }

            if (stateTimer < activeSeconds)
            {
                return;
            }

            state = SoldierState.Recovery;
            stateTimer = 0f;
            SetBodyColor(normalColor);
        }

        private void UpdateRecovery(float deltaTime)
        {
            stateTimer += deltaTime;
            FaceTarget(deltaTime);
            Move(Vector3.zero, deltaTime);

            if (stateTimer < recoverySeconds)
            {
                return;
            }

            state = SoldierState.Approach;
            stateTimer = 0f;
        }

        private void UpdateStagger(float deltaTime)
        {
            stateTimer += deltaTime;
            Move(knockbackVelocity, deltaTime);
            knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockbackSpeed * deltaTime);
            UpdateAnimation(0f);

            if (stateTimer < hitReactionSeconds)
            {
                return;
            }

            state = SoldierState.Approach;
            stateTimer = 0f;
            SetBodyColor(normalColor);
        }

        private void ApplyDamageToTarget()
        {
            Vector3 direction = DirectionToTarget();
            DamageInfo damageInfo = new DamageInfo(
                selfHealth,
                selfHealth != null ? selfHealth.Team : DamageTeam.Enemy,
                damage,
                target.position,
                direction,
                hitStopSeconds);

            targetHealth.TryApplyDamage(damageInfo);
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (state == SoldierState.Dead)
            {
                return;
            }

            state = SoldierState.Stagger;
            stateTimer = 0f;
            knockbackVelocity = Vector3.ProjectOnPlane(damageInfo.Direction, Vector3.up).normalized * knockbackSpeed;
            SetTelegraphVisible(false);
            SetBodyColor(staggerColor);
            TriggerAnimator(hitTrigger);
        }

        private void HandleDied()
        {
            state = SoldierState.Dead;
            SetTelegraphVisible(false);
            SetBodyColor(deadColor);
            TriggerAnimator(deathTrigger);
        }

        private bool IsTargetInAttackRange()
        {
            if (target == null)
            {
                return false;
            }

            Vector3 toTarget = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
            if (toTarget.magnitude > attackRange)
            {
                return false;
            }

            return Vector3.Dot(transform.forward, toTarget.normalized) > -0.15f;
        }

        private Vector3 DirectionToTarget()
        {
            if (target == null)
            {
                return transform.forward;
            }

            Vector3 direction = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
            return direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        }

        private void FaceTarget(float deltaTime)
        {
            Vector3 direction = DirectionToTarget();
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRateDegrees * deltaTime);
        }

        private void Move(Vector3 planarVelocity, float deltaTime)
        {
            if (characterController == null)
            {
                transform.position += planarVelocity * deltaTime;
                return;
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * deltaTime;
            characterController.Move((planarVelocity + Vector3.up * verticalVelocity) * deltaTime);
        }

        private void UpdateAnimation(float planarSpeed)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(moveSpeedParameter))
            {
                animator.SetFloat(moveSpeedParameter, planarSpeed);
            }
        }

        private void SetTelegraphVisible(bool visible)
        {
            if (telegraphIndicator != null)
            {
                telegraphIndicator.SetActive(visible);
            }
        }

        private void SetBodyColor(Color color)
        {
            if (bodyRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            bodyRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorProperty, color);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            bodyRenderer.SetPropertyBlock(propertyBlock);
        }

        private void TriggerAnimator(string triggerName)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}
