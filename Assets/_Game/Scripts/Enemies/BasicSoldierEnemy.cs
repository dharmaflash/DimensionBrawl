using DimensionBrawl.Combat;
using System;
using DimensionBrawl.AI;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatHealth))]
    [RequireComponent(typeof(CombatTargetSensor))]
    public sealed class BasicSoldierEnemy : MonoBehaviour, ICombatAiAgent
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

        [Header("Enemy Type")]
        [Tooltip("Prefab-level enemy identity. Visual model, Animator controller, and animation trigger names stay swappable per enemy type.")]
        [SerializeField] private string enemyTypeId = "SciFiSoldier.Basic";

        [Tooltip("Reference-backed pattern sample: ClosePunish = Track -> Windup -> MeleeBurst -> Recover.")]
        [SerializeField] private string patternId = "ClosePunish";

        [Header("Profile")]
        [SerializeField] private CombatAiPatternProfile patternProfile;

        [Header("References")]
        [SerializeField] private CombatTargetSensor targetSensor;
        [SerializeField] private Transform target;
        [SerializeField] private CombatHealth targetHealth;
        [SerializeField] private CombatHealth selfHealth;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject telegraphIndicator;
        [SerializeField] private EnemyAttackTelegraphPresenter telegraphPresenter;
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
        [SerializeField, Range(-1f, 1f)] private float attackFacingDotThreshold = -0.15f;
        [SerializeField, Min(0f)] private float activeLungeSpeed = 0f;
        [SerializeField] private CombatAiAttackShape attackShape = CombatAiAttackShape.MeleeArc;
        [SerializeField, Min(0f)] private float attackHalfWidth = 0.65f;
        [SerializeField] private bool lockAttackDirectionOnWindup;

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
        [SerializeField] private bool usePrototypeBodyColors = true;
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
        private bool hasLockedAttackDirection;
        private Vector3 lockedAttackDirection = Vector3.forward;
        private CombatAiPatternState currentPatternState = CombatAiPatternState.Tracking;

        public CombatAiPatternProfile PatternProfile => patternProfile;
        public CombatHealth SelfHealth => selfHealth;
        public CombatAiPatternState CurrentPatternState => currentPatternState;
        public string ActorTypeId => patternProfile != null ? patternProfile.ActorTypeId : enemyTypeId;
        public string EnemyTypeId => ActorTypeId;
        public string PatternId => patternProfile != null ? patternProfile.PatternId : patternId;
        public CombatTargetSensor TargetSensor => targetSensor;
        public string AttackAnimationTrigger => ActiveAttackTrigger;
        public string HitAnimationTrigger => ActiveHitTrigger;
        public string DeathAnimationTrigger => ActiveDeathTrigger;

        public event Action<CombatAiPatternState, CombatAiPatternProfile> PatternStateChanged;

        private float ActiveApproachSpeed => patternProfile != null ? patternProfile.ApproachSpeed : approachSpeed;
        private float ActiveTurnRateDegrees => patternProfile != null ? patternProfile.TurnRateDegrees : turnRateDegrees;
        private float ActiveGravity => patternProfile != null ? patternProfile.Gravity : gravity;
        private float ActiveAttackRange => patternProfile != null ? patternProfile.AttackRange : attackRange;
        private float ActiveAttackFacingDotThreshold => patternProfile != null ? patternProfile.AttackFacingDotThreshold : attackFacingDotThreshold;
        private float ActiveTelegraphSeconds => patternProfile != null ? patternProfile.TelegraphSeconds : telegraphSeconds;
        private float ActiveActiveSeconds => patternProfile != null ? patternProfile.ActiveSeconds : activeSeconds;
        private float ActiveActiveLungeSpeed => patternProfile != null ? patternProfile.ActiveLungeSpeed : activeLungeSpeed;
        private float ActiveRecoverySeconds => patternProfile != null ? patternProfile.RecoverySeconds : recoverySeconds;
        private float ActiveDamage => patternProfile != null ? patternProfile.Damage : damage;
        private float ActiveHitStopSeconds => patternProfile != null ? patternProfile.HitStopSeconds : hitStopSeconds;
        private CombatAiAttackShape ActiveAttackShape => patternProfile != null ? patternProfile.AttackShape : attackShape;
        private float ActiveAttackHalfWidth => patternProfile != null ? patternProfile.AttackHalfWidth : attackHalfWidth;
        private bool ActiveLockAttackDirectionOnWindup => patternProfile != null ? patternProfile.LockAttackDirectionOnWindup : lockAttackDirectionOnWindup;
        private float ActiveHitReactionSeconds => patternProfile != null ? patternProfile.HitReactionSeconds : hitReactionSeconds;
        private float ActiveKnockbackSpeed => patternProfile != null ? patternProfile.KnockbackSpeed : knockbackSpeed;
        private float ActiveRecoveryRetreatSpeed => patternProfile != null ? patternProfile.RecoveryRetreatSpeed : 0f;
        private float ActiveRecoveryRetreatSeconds => patternProfile != null ? patternProfile.RecoveryRetreatSeconds : 0f;
        private string ActiveMoveSpeedParameter => patternProfile != null ? patternProfile.MoveSpeedParameter : moveSpeedParameter;
        private string ActiveAttackTrigger => patternProfile != null ? patternProfile.AttackTrigger : attackTrigger;
        private string ActiveHitTrigger => patternProfile != null ? patternProfile.HitTrigger : hitTrigger;
        private string ActiveDeathTrigger => patternProfile != null ? patternProfile.DeathTrigger : deathTrigger;

        public void ConfigureTarget(Transform newTarget, CombatHealth newTargetHealth)
        {
            target = newTarget;
            targetHealth = newTargetHealth;
        }

        public void ConfigurePattern(CombatAiPatternProfile profile)
        {
            patternProfile = profile;
            PatternStateChanged?.Invoke(currentPatternState, patternProfile);
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

            if (targetSensor == null)
            {
                targetSensor = GetComponent<CombatTargetSensor>();
            }

            if (telegraphPresenter == null)
            {
                telegraphPresenter = GetComponent<EnemyAttackTelegraphPresenter>();
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
            ResolveCurrentTarget();

            if (state == SoldierState.Dead || target == null || targetHealth == null || !targetHealth.IsAlive)
            {
                if (state != SoldierState.Dead)
                {
                    SetPatternState(CombatAiPatternState.Tracking);
                }

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

        private void ResolveCurrentTarget()
        {
            if (targetSensor == null)
            {
                return;
            }

            if (targetSensor.TryGetCurrentTarget(out Transform sensedTarget, out CombatHealth sensedHealth))
            {
                target = sensedTarget;
                targetHealth = sensedHealth;
                return;
            }

            if (targetHealth == null || !targetHealth.IsAlive)
            {
                target = null;
                targetHealth = null;
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
            Move(direction * ActiveApproachSpeed, deltaTime);
            UpdateAnimation(ActiveApproachSpeed);
            SetBodyColor(normalColor);
        }

        private void BeginTelegraph()
        {
            EnterState(SoldierState.Telegraph, CombatAiPatternState.Windup);
            stateTimer = 0f;
            dealtDamageThisSwing = false;
            hasLockedAttackDirection = ActiveLockAttackDirectionOnWindup;
            lockedAttackDirection = DirectionToTarget();
            if (hasLockedAttackDirection)
            {
                FaceDirection(lockedAttackDirection, 0f);
            }

            ApplyTelegraphStyle();
            ShowTelegraphWindup(0f);
            SetBodyColor(telegraphColor);
        }

        private void UpdateTelegraph(float deltaTime)
        {
            stateTimer += deltaTime;
            FaceCurrentAttackDirection(deltaTime);
            Move(Vector3.zero, deltaTime);
            UpdateAnimation(0f);
            ShowTelegraphWindup(ActiveTelegraphSeconds > 0f ? stateTimer / ActiveTelegraphSeconds : 1f);

            if (stateTimer < ActiveTelegraphSeconds)
            {
                return;
            }

            EnterState(SoldierState.Active, CombatAiPatternState.AttackActive);
            stateTimer = 0f;
            ShowTelegraphActive(0f);
            TriggerAnimator(ActiveAttackTrigger);
        }

        private void UpdateActive(float deltaTime)
        {
            stateTimer += deltaTime;
            FaceCurrentAttackDirection(deltaTime);
            Vector3 lungeVelocity = ActiveActiveLungeSpeed > 0f ? CurrentAttackDirection() * ActiveActiveLungeSpeed : Vector3.zero;
            Move(lungeVelocity, deltaTime);
            ShowTelegraphActive(ActiveActiveSeconds > 0f ? stateTimer / ActiveActiveSeconds : 1f);

            if (!dealtDamageThisSwing && IsTargetInsideActiveHitShape())
            {
                dealtDamageThisSwing = true;
                ApplyDamageToTarget();
            }

            if (stateTimer < ActiveActiveSeconds)
            {
                return;
            }

            EnterState(SoldierState.Recovery, CombatAiPatternState.Recovery);
            stateTimer = 0f;
            hasLockedAttackDirection = false;
            HideTelegraph();
            SetBodyColor(normalColor);
        }

        private void UpdateRecovery(float deltaTime)
        {
            stateTimer += deltaTime;
            FaceTarget(deltaTime);
            Vector3 retreatVelocity = stateTimer < ActiveRecoveryRetreatSeconds
                ? -DirectionToTarget() * ActiveRecoveryRetreatSpeed
                : Vector3.zero;
            Move(retreatVelocity, deltaTime);

            if (stateTimer < ActiveRecoverySeconds)
            {
                return;
            }

            EnterState(SoldierState.Approach, CombatAiPatternState.Tracking);
            stateTimer = 0f;
        }

        private void UpdateStagger(float deltaTime)
        {
            stateTimer += deltaTime;
            Move(knockbackVelocity, deltaTime);
            knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, ActiveKnockbackSpeed * deltaTime);
            UpdateAnimation(0f);

            if (stateTimer < ActiveHitReactionSeconds)
            {
                return;
            }

            EnterState(SoldierState.Approach, CombatAiPatternState.Tracking);
            stateTimer = 0f;
            SetBodyColor(normalColor);
        }

        private void ApplyDamageToTarget()
        {
            Vector3 direction = CurrentAttackDirection();
            DamageInfo damageInfo = new DamageInfo(
                selfHealth,
                selfHealth != null ? selfHealth.Team : DamageTeam.Enemy,
                ActiveDamage,
                target.position,
                direction,
                ActiveHitStopSeconds);

            targetHealth.TryApplyDamage(damageInfo);
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (state == SoldierState.Dead)
            {
                return;
            }

            if (selfHealth != null && selfHealth.CurrentHealth <= 0f)
            {
                return;
            }

            EnterState(SoldierState.Stagger, CombatAiPatternState.Stagger);
            stateTimer = 0f;
            hasLockedAttackDirection = false;
            knockbackVelocity = Vector3.ProjectOnPlane(damageInfo.Direction, Vector3.up).normalized * ActiveKnockbackSpeed;
            HideTelegraph();
            SetBodyColor(staggerColor);
            TriggerAnimator(ActiveHitTrigger);
        }

        private void HandleDied()
        {
            EnterState(SoldierState.Dead, CombatAiPatternState.Death);
            hasLockedAttackDirection = false;
            HideTelegraph();
            SetBodyColor(deadColor);
            ResetAnimatorTrigger(ActiveAttackTrigger);
            ResetAnimatorTrigger(ActiveHitTrigger);
            UpdateAnimation(0f);
            TriggerAnimator(ActiveDeathTrigger);
        }

        private void EnterState(SoldierState nextState, CombatAiPatternState nextPatternState)
        {
            state = nextState;
            SetPatternState(nextPatternState);
        }

        private void SetPatternState(CombatAiPatternState nextPatternState)
        {
            if (currentPatternState == nextPatternState)
            {
                return;
            }

            currentPatternState = nextPatternState;
            PatternStateChanged?.Invoke(currentPatternState, patternProfile);
        }

        private bool IsTargetInAttackRange()
        {
            if (target == null)
            {
                return false;
            }

            Vector3 toTarget = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
            if (toTarget.magnitude > ActiveAttackRange)
            {
                return false;
            }

            return Vector3.Dot(transform.forward, toTarget.normalized) > ActiveAttackFacingDotThreshold;
        }

        private bool IsTargetInsideActiveHitShape()
        {
            if (target == null)
            {
                return false;
            }

            if (ActiveAttackShape == CombatAiAttackShape.ForwardLine)
            {
                Vector3 localTarget = transform.InverseTransformPoint(target.position);
                return localTarget.z >= 0f
                    && localTarget.z <= ActiveAttackRange
                    && Mathf.Abs(localTarget.x) <= ActiveAttackHalfWidth;
            }

            return IsTargetInAttackRange();
        }

        private Vector3 CurrentAttackDirection()
        {
            return hasLockedAttackDirection ? lockedAttackDirection : DirectionToTarget();
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
            FaceDirection(DirectionToTarget(), deltaTime);
        }

        private void FaceCurrentAttackDirection(float deltaTime)
        {
            FaceDirection(CurrentAttackDirection(), deltaTime);
        }

        private void FaceDirection(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = deltaTime > 0f
                ? Quaternion.RotateTowards(transform.rotation, targetRotation, ActiveTurnRateDegrees * deltaTime)
                : targetRotation;
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

            verticalVelocity += ActiveGravity * deltaTime;
            characterController.Move((planarVelocity + Vector3.up * verticalVelocity) * deltaTime);
        }

        private void UpdateAnimation(float planarSpeed)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(ActiveMoveSpeedParameter))
            {
                animator.SetFloat(ActiveMoveSpeedParameter, planarSpeed);
            }
        }

        private void SetTelegraphVisible(bool visible)
        {
            if (telegraphIndicator != null)
            {
                telegraphIndicator.SetActive(visible);
            }
        }

        private void ApplyTelegraphStyle()
        {
            if (telegraphPresenter == null || patternProfile == null)
            {
                return;
            }

            telegraphPresenter.ConfigureStyle(
                patternProfile.TelegraphWindupStartScale,
                patternProfile.TelegraphWindupEndScale,
                patternProfile.TelegraphActiveScale,
                patternProfile.WindupPoseOffset,
                patternProfile.ActivePoseOffset,
                patternProfile.WindupStartColor,
                patternProfile.WindupEndColor,
                patternProfile.ActiveColor);
        }

        private void ShowTelegraphWindup(float normalizedProgress)
        {
            if (telegraphPresenter != null)
            {
                telegraphPresenter.ShowWindup(normalizedProgress);
                return;
            }

            SetTelegraphVisible(true);
        }

        private void ShowTelegraphActive(float normalizedProgress)
        {
            if (telegraphPresenter != null)
            {
                telegraphPresenter.ShowActive(normalizedProgress);
                return;
            }

            SetTelegraphVisible(true);
        }

        private void HideTelegraph()
        {
            if (telegraphPresenter != null)
            {
                telegraphPresenter.Hide();
                return;
            }

            SetTelegraphVisible(false);
        }

        private void SetBodyColor(Color color)
        {
            if (!usePrototypeBodyColors)
            {
                return;
            }

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

        private void ResetAnimatorTrigger(string triggerName)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
            {
                animator.ResetTrigger(triggerName);
            }
        }
    }
}
