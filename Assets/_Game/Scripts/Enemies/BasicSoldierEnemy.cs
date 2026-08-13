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
            Prepare,
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
        [SerializeField] private CombatAiPatternDeck patternDeck;

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

        [Header("Approach Motion")]
        [SerializeField, Min(0f)] private float approachAcceleration = 10f;
        [SerializeField, Min(0f)] private float approachDeceleration = 16f;
        [SerializeField, Min(0f)] private float attackRangeSlowdownDistance = 0.75f;
        [SerializeField, Range(0f, 1f)] private float minimumAttackRangeSpeedScale = 0.38f;
        [SerializeField, Range(0f, 1f)] private float turnAlignmentSpeedFloor = 0.42f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float prepareSeconds;
        [SerializeField, Min(0f)] private float prepareRetreatSpeed;
        [SerializeField] private bool lockAttackDirectionAfterPrepare = true;
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
        [SerializeField, Range(0f, 90f)] private float attackHalfAngleDegrees = 28f;
        [SerializeField] private bool lockAttackDirectionOnWindup;

        [Header("Contact Damage VFX")]
        [SerializeField] private GameObject contactDamageVfxPrefab;
        [SerializeField, Min(0.01f)] private float contactDamageVfxScale = 0.46f;
        [SerializeField, Min(0f)] private float contactDamageVfxHeightOffset = 0.58f;
        [SerializeField, Min(0.05f)] private float contactDamageVfxLifetimeSeconds = 0.72f;

        [Header("Hit Reaction")]
        [Tooltip("Uses the collected light enemy stagger range of 0.18-0.35 seconds.")]
        [SerializeField, Min(0f)] private float hitReactionSeconds = 0.24f;
        [Tooltip("First-pass deviation: no collected soldier knockback speed exists yet, so this stays Inspector-visible.")]
        [SerializeField, Min(0f)] private float knockbackSpeed = 2f;

        [Header("Animation Requests")]
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";
        [SerializeField] private string prepareTrigger = string.Empty;
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
        private Vector3 approachPlanarVelocity;
        private Vector3 knockbackVelocity;
        private float stateTimer;
        private float verticalVelocity;
        private bool dealtDamageThisSwing;
        private bool hasLockedAttackDirection;
        private bool gameplaySuspended;
        private bool healthEventsSubscribed;
        private Vector3 lockedAttackDirection = Vector3.forward;
        private float[] patternDeckLastUseTimes = Array.Empty<float>();
        private int activePatternDeckIndex = -1;
        private CombatAiPatternState currentPatternState = CombatAiPatternState.Tracking;

        public CombatAiPatternProfile PatternProfile => patternProfile;
        public CombatAiPatternDeck PatternDeck => patternDeck;
        public bool HasPatternDeck => patternDeck != null;
        public int ActivePatternDeckIndex => activePatternDeckIndex;
        public CombatHealth SelfHealth => selfHealth;
        public CombatAiPatternState CurrentPatternState => currentPatternState;
        public string ActorTypeId => patternProfile != null ? patternProfile.ActorTypeId : enemyTypeId;
        public string EnemyTypeId => ActorTypeId;
        public string PatternId => patternProfile != null ? patternProfile.PatternId : patternId;
        public CombatTargetSensor TargetSensor => targetSensor;
        public Vector3 ResolvedAttackDirection => CurrentAttackDirection();
        public string AttackAnimationTrigger => ActiveAttackTrigger;
        public string HitAnimationTrigger => ActiveHitTrigger;
        public string DeathAnimationTrigger => ActiveDeathTrigger;
        public bool IsGameplaySuspended => gameplaySuspended;

        public event Action<CombatAiPatternState, CombatAiPatternProfile> PatternStateChanged;

        public void SetGameplaySuspended(bool suspended)
        {
            gameplaySuspended = suspended;
            if (!suspended)
            {
                if (!isActiveAndEnabled)
                {
                    UnsubscribeHealthEvents();
                }

                if (selfHealth != null && !selfHealth.IsAlive && state != SoldierState.Dead)
                {
                    HandleDied();
                }

                return;
            }

            SubscribeHealthEvents();
            dealtDamageThisSwing = false;
            hasLockedAttackDirection = false;
            ResetApproachVelocity();
            knockbackVelocity = Vector3.zero;
            HideTelegraph();
            UpdateAnimation(0f);
        }

        private float ActiveApproachSpeed => patternProfile != null ? patternProfile.ApproachSpeed : approachSpeed;
        private float ActiveTurnRateDegrees => patternProfile != null ? patternProfile.TurnRateDegrees : turnRateDegrees;
        private float ActiveGravity => patternProfile != null ? patternProfile.Gravity : gravity;
        private float ActiveApproachAcceleration => patternProfile != null ? patternProfile.ApproachAcceleration : approachAcceleration;
        private float ActiveApproachDeceleration => patternProfile != null ? patternProfile.ApproachDeceleration : approachDeceleration;
        private float ActiveAttackRangeSlowdownDistance => patternProfile != null ? patternProfile.AttackRangeSlowdownDistance : attackRangeSlowdownDistance;
        private float ActiveMinimumAttackRangeSpeedScale => patternProfile != null ? patternProfile.MinimumAttackRangeSpeedScale : minimumAttackRangeSpeedScale;
        private float ActiveTurnAlignmentSpeedFloor => patternProfile != null ? patternProfile.TurnAlignmentSpeedFloor : turnAlignmentSpeedFloor;
        private float ActivePrepareSeconds => patternProfile != null ? patternProfile.PrepareSeconds : prepareSeconds;
        private float ActivePrepareRetreatSpeed => patternProfile != null ? patternProfile.PrepareRetreatSpeed : prepareRetreatSpeed;
        private bool ActiveLockAttackDirectionAfterPrepare => patternProfile != null ? patternProfile.LockAttackDirectionAfterPrepare : lockAttackDirectionAfterPrepare;
        private float ActiveAttackRange => patternProfile != null ? patternProfile.AttackRange : attackRange;
        private float ActiveAttackFacingDotThreshold => patternProfile != null ? patternProfile.AttackFacingDotThreshold : attackFacingDotThreshold;
        private float ActiveTelegraphSeconds => patternProfile != null ? patternProfile.TelegraphSeconds : telegraphSeconds;
        private float ActiveActiveSeconds => patternProfile != null ? patternProfile.ActiveSeconds : activeSeconds;
        private float ActiveActiveLungeSpeed => patternProfile != null ? patternProfile.ActiveLungeSpeed : activeLungeSpeed;
        private float ActiveRecoverySeconds => patternProfile != null ? patternProfile.RecoverySeconds : recoverySeconds;
        private float ActiveDamage => patternProfile != null ? patternProfile.Damage : damage;
        private float ActiveHitStopSeconds => patternProfile != null ? patternProfile.HitStopSeconds : hitStopSeconds;
        private DamageResponsePolicy ActiveDamageResponsePolicy => patternProfile != null
            ? patternProfile.DamageResponsePolicy
            : ResolveLocalDamageResponsePolicy();
        private CombatControlLockPolicy ActiveControlLockPolicy => patternProfile != null
            ? patternProfile.ControlLockPolicy
            : ResolveLocalControlLockPolicy();
        private CombatAiAttackShape ActiveAttackShape => patternProfile != null ? patternProfile.AttackShape : attackShape;
        private float ActiveAttackHalfWidth => patternProfile != null ? patternProfile.AttackHalfWidth : attackHalfWidth;
        private float ActiveAttackHalfAngleDegrees => patternProfile != null ? patternProfile.AttackHalfAngleDegrees : attackHalfAngleDegrees;
        private bool ActiveLockAttackDirectionOnWindup => patternProfile != null ? patternProfile.LockAttackDirectionOnWindup : lockAttackDirectionOnWindup;
        private float ActiveHitReactionSeconds => patternProfile != null ? patternProfile.HitReactionSeconds : hitReactionSeconds;
        private float ActiveKnockbackSpeed => patternProfile != null ? patternProfile.KnockbackSpeed : knockbackSpeed;
        private float ActiveRecoveryRetreatSpeed => patternProfile != null ? patternProfile.RecoveryRetreatSpeed : 0f;
        private float ActiveRecoveryRetreatSeconds => patternProfile != null ? patternProfile.RecoveryRetreatSeconds : 0f;
        private string ActiveMoveSpeedParameter => patternProfile != null ? patternProfile.MoveSpeedParameter : moveSpeedParameter;
        private string ActivePrepareTrigger => patternProfile != null ? patternProfile.PrepareTrigger : prepareTrigger;
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
            activePatternDeckIndex = -1;
            lockedAttackDirection = DirectionToTarget();
            hasLockedAttackDirection = state == SoldierState.Prepare
                ? ActiveLockAttackDirectionAfterPrepare
                : (state == SoldierState.Telegraph || state == SoldierState.Active)
                    && ActiveLockAttackDirectionOnWindup;
            PatternStateChanged?.Invoke(currentPatternState, patternProfile);
        }

        public void ConfigurePatternDeck(CombatAiPatternDeck deck)
        {
            patternDeck = deck;
            patternDeckLastUseTimes = Array.Empty<float>();
            activePatternDeckIndex = -1;
        }

        public bool TryGetActivePatternDeckEntry(out CombatAiPatternDeckEntry entry)
        {
            if (patternDeck == null || activePatternDeckIndex < 0 || activePatternDeckIndex >= patternDeck.EntryCount)
            {
                entry = default;
                return false;
            }

            entry = patternDeck.GetEntry(activePatternDeckIndex);
            return true;
        }

        private void Awake()
        {
            CombatTimeDilationReceiver.Ensure(gameObject);
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
            SubscribeHealthEvents();

            SetTelegraphVisible(false);
            if (selfHealth != null && !selfHealth.IsAlive)
            {
                HandleDied();
                return;
            }

            SetBodyColor(normalColor);
        }

        private void OnDisable()
        {
            if (!gameplaySuspended)
            {
                UnsubscribeHealthEvents();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeHealthEvents();
        }

        private void SubscribeHealthEvents()
        {
            if (healthEventsSubscribed || selfHealth == null)
            {
                return;
            }

            selfHealth.Damaged += HandleDamaged;
            selfHealth.Died += HandleDied;
            healthEventsSubscribed = true;
        }

        private void UnsubscribeHealthEvents()
        {
            if (!healthEventsSubscribed)
            {
                return;
            }

            if (selfHealth != null)
            {
                selfHealth.Damaged -= HandleDamaged;
                selfHealth.Died -= HandleDied;
            }

            healthEventsSubscribed = false;
        }

        private void Update()
        {
            if (gameplaySuspended)
            {
                UpdateAnimation(0f);
                return;
            }

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

            float deltaTime = Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this);

            switch (state)
            {
                case SoldierState.Approach:
                    UpdateApproach(deltaTime);
                    break;
                case SoldierState.Prepare:
                    UpdatePrepare(deltaTime);
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
            bool hasReadyPattern = SelectPatternFromDeck();
            FaceTarget(deltaTime);

            if (hasReadyPattern && IsTargetInAttackRange())
            {
                ResetApproachVelocity();
                if (ActivePrepareSeconds > 0f)
                {
                    BeginPrepare();
                }
                else
                {
                    BeginTelegraph();
                }

                return;
            }

            Vector3 velocity = ResolveApproachVelocity(deltaTime);
            Move(velocity, deltaTime);
            UpdateAnimation(velocity.magnitude);
            SetBodyColor(normalColor);
        }

        private void BeginPrepare()
        {
            ResetApproachVelocity();
            EnterState(SoldierState.Prepare, CombatAiPatternState.Repositioning);
            stateTimer = 0f;
            dealtDamageThisSwing = false;
            hasLockedAttackDirection = ActiveLockAttackDirectionAfterPrepare;
            lockedAttackDirection = DirectionToTarget();
            HideTelegraph();
            SetBodyColor(normalColor);
            TriggerAnimator(ActivePrepareTrigger);
        }

        private void UpdatePrepare(float deltaTime)
        {
            stateTimer += deltaTime;
            FaceTarget(deltaTime);
            Vector3 retreatVelocity = -DirectionToTarget() * ActivePrepareRetreatSpeed;
            Move(retreatVelocity, deltaTime);
            UpdateAnimation(ActivePrepareRetreatSpeed);

            if (stateTimer < ActivePrepareSeconds)
            {
                return;
            }

            BeginTelegraph();
        }

        private void BeginTelegraph()
        {
            ResetApproachVelocity();
            EnterState(SoldierState.Telegraph, CombatAiPatternState.Windup);
            stateTimer = 0f;
            dealtDamageThisSwing = false;
            if (!hasLockedAttackDirection)
            {
                hasLockedAttackDirection = ActiveLockAttackDirectionOnWindup;
                lockedAttackDirection = DirectionToTarget();
            }

            if (hasLockedAttackDirection)
            {
                FaceDirection(lockedAttackDirection, 0f);
            }

            ApplyTelegraphStyle();
            RecordActiveDeckPatternUse();
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
            UpdateAnimation(lungeVelocity.magnitude);
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
            UpdateAnimation(retreatVelocity.magnitude);

            if (stateTimer < ActiveRecoverySeconds)
            {
                return;
            }

            activePatternDeckIndex = -1;
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

            activePatternDeckIndex = -1;
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
                ActiveHitStopSeconds,
                ActiveDamageResponsePolicy,
                ActiveControlLockPolicy);

            if (targetHealth.TryApplyDamage(damageInfo))
            {
                SpawnContactDamageVfx(damageInfo.Point, direction);
            }
        }

        private void SpawnContactDamageVfx(Vector3 hitPoint, Vector3 direction)
        {
            if (contactDamageVfxPrefab == null)
            {
                return;
            }

            Vector3 spawnPoint = hitPoint + Vector3.up * contactDamageVfxHeightOffset;
            Quaternion rotation = ResolveContactDamageVfxRotation(direction);
            GameObject instance = Instantiate(contactDamageVfxPrefab, spawnPoint, rotation);
            instance.transform.localScale *= Mathf.Max(0.01f, contactDamageVfxScale);
            Destroy(instance, Mathf.Max(0.05f, contactDamageVfxLifetimeSeconds));
        }

        private Quaternion ResolveContactDamageVfxRotation(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude <= 0.0001f)
            {
                planarDirection = transform.forward;
            }

            return Quaternion.LookRotation(planarDirection.normalized, Vector3.up);
        }

        private DamageResponsePolicy ResolveLocalDamageResponsePolicy()
        {
            return damage >= 22f || activeLungeSpeed > 0.5f
                ? DamageResponsePolicy.Stagger
                : DamageResponsePolicy.FlashOnly;
        }

        private CombatControlLockPolicy ResolveLocalControlLockPolicy()
        {
            return damage >= 22f || activeLungeSpeed > 0.5f
                ? CombatControlLockPolicy.InterruptAction
                : CombatControlLockPolicy.None;
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

            if (!DamageResponsePolicyUtility.InterruptsAction(damageInfo.ControlLockPolicy))
            {
                return;
            }

            EnterState(SoldierState.Stagger, CombatAiPatternState.Stagger);
            stateTimer = 0f;
            hasLockedAttackDirection = false;
            ResetApproachVelocity();
            knockbackVelocity = Vector3.ProjectOnPlane(damageInfo.Direction, Vector3.up).normalized * ActiveKnockbackSpeed;
            HideTelegraph();
            SetBodyColor(staggerColor);
            TriggerAnimator(ActiveHitTrigger);
        }

        private void HandleDied()
        {
            EnterState(SoldierState.Dead, CombatAiPatternState.Death);
            hasLockedAttackDirection = false;
            ResetApproachVelocity();
            HideTelegraph();
            SetBodyColor(deadColor);
            ResetAnimatorTrigger(ActivePrepareTrigger);
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

            if (ActiveAttackShape == CombatAiAttackShape.ProjectileLine)
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

            if (ActiveAttackShape == CombatAiAttackShape.ForwardFan)
            {
                Vector3 localTarget = transform.InverseTransformPoint(target.position);
                Vector2 planarTarget = new Vector2(localTarget.x, localTarget.z);
                if (localTarget.z < 0f || planarTarget.magnitude > ActiveAttackRange)
                {
                    return false;
                }

                float angle = Mathf.Abs(Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg);
                return angle <= ActiveAttackHalfAngleDegrees;
            }

            return IsTargetInAttackRange();
        }

        private bool SelectPatternFromDeck()
        {
            if (patternDeck == null)
            {
                return true;
            }

            if (target == null)
            {
                return false;
            }

            EnsurePatternDeckState();
            float targetDistance = HorizontalDistanceToTarget();
            if (ShouldKeepActiveDeckSelection(targetDistance))
            {
                return true;
            }

            if (!patternDeck.TrySelectPattern(
                    targetDistance,
                    patternProfile,
                    Time.time,
                    patternDeckLastUseTimes,
                    out CombatAiPatternProfile selectedProfile,
                    out int selectedIndex))
            {
                activePatternDeckIndex = -1;
                return false;
            }

            activePatternDeckIndex = selectedIndex;
            if (selectedProfile == patternProfile)
            {
                return true;
            }

            patternProfile = selectedProfile;
            PatternStateChanged?.Invoke(currentPatternState, patternProfile);
            return true;
        }

        private bool ShouldKeepActiveDeckSelection(float targetDistance)
        {
            if (activePatternDeckIndex < 0 || patternDeck == null || activePatternDeckIndex >= patternDeck.EntryCount)
            {
                return false;
            }

            CombatAiPatternDeckEntry entry = patternDeck.GetEntry(activePatternDeckIndex);
            return entry.Profile == patternProfile && entry.IsDistanceInRange(targetDistance);
        }

        private void EnsurePatternDeckState()
        {
            int entryCount = patternDeck != null ? patternDeck.EntryCount : 0;
            if (entryCount <= 0)
            {
                patternDeckLastUseTimes = Array.Empty<float>();
                activePatternDeckIndex = -1;
                return;
            }

            if (patternDeckLastUseTimes != null && patternDeckLastUseTimes.Length == entryCount)
            {
                return;
            }

            patternDeckLastUseTimes = new float[entryCount];
            for (int i = 0; i < patternDeckLastUseTimes.Length; i++)
            {
                patternDeckLastUseTimes[i] = -1f;
            }

            activePatternDeckIndex = -1;
        }

        private void RecordActiveDeckPatternUse()
        {
            if (patternDeck == null || activePatternDeckIndex < 0)
            {
                return;
            }

            EnsurePatternDeckState();
            if (activePatternDeckIndex >= patternDeckLastUseTimes.Length)
            {
                return;
            }

            patternDeckLastUseTimes[activePatternDeckIndex] = Time.time;
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

        private float HorizontalDistanceToTarget()
        {
            if (target == null)
            {
                return float.PositiveInfinity;
            }

            return Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up).magnitude;
        }

        private Vector3 ResolveApproachVelocity(float deltaTime)
        {
            Vector3 direction = DirectionToTarget();
            float targetSpeed = ActiveApproachSpeed * ResolveApproachSpeedScale(direction);
            Vector3 desiredVelocity = direction * targetSpeed;
            float acceleration = desiredVelocity.sqrMagnitude > approachPlanarVelocity.sqrMagnitude
                ? ActiveApproachAcceleration
                : ActiveApproachDeceleration;

            if (acceleration <= 0f || deltaTime <= 0f)
            {
                approachPlanarVelocity = desiredVelocity;
            }
            else
            {
                approachPlanarVelocity = Vector3.MoveTowards(
                    approachPlanarVelocity,
                    desiredVelocity,
                    acceleration * deltaTime);
            }

            return approachPlanarVelocity;
        }

        private float ResolveApproachSpeedScale(Vector3 direction)
        {
            float distancePastAttackRange = HorizontalDistanceToTarget() - ActiveAttackRange;
            float slowdownDistance = ActiveAttackRangeSlowdownDistance;
            float rangeScale = 1f;
            if (slowdownDistance > 0f)
            {
                float progress = Mathf.Clamp01(distancePastAttackRange / slowdownDistance);
                rangeScale = Mathf.Lerp(
                    Mathf.Clamp01(ActiveMinimumAttackRangeSpeedScale),
                    1f,
                    Mathf.SmoothStep(0f, 1f, progress));
            }

            float facingDot = Vector3.Dot(transform.forward, direction);
            float facingScale = Mathf.Lerp(
                Mathf.Clamp01(ActiveTurnAlignmentSpeedFloor),
                1f,
                Mathf.Clamp01((facingDot + 0.2f) / 1.2f));
            return Mathf.Clamp01(rangeScale * facingScale);
        }

        private void ResetApproachVelocity()
        {
            approachPlanarVelocity = Vector3.zero;
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
