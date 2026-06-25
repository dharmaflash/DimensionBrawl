using System;
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsekaiBrawl.Gameplay
{
    public enum ManualTargetLockKind
    {
        None = 0,
        NormalEnemy = 1,
        Boss = 2,
        Structure = 3
    }

    public enum PlayerMotionState
    {
        SlotFollow = 0,
        Dodging = 1,
        Recovering = 2,
        Retreating = 3
    }

    public enum FocusLaneReason
    {
        Hold = 0,
        Threat = 1,
        Preferred = 2,
        Retreat = 3
    }

    public enum HeroInterventionReason
    {
        Escort = 0,
        AssistWave = 1,
        BreakBlocker = 2,
        CashReward = 3,
        BossPressure = 4
    }

    public enum PlayerRetreatReason
    {
        None = 0,
        NoAlliedFrontline = 1,
        LaneCollapse = 2,
        Overextended = 3
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int IdleStateHash = Animator.StringToHash("Base Layer.Idle");
        private static readonly int WalkStateHash = Animator.StringToHash("Base Layer.Walk");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        public event Action OnDeath;
        public event Action<float, float> OnHPChanged;
        public event Action<float> OnJustDodgeRewarded;

        [SerializeField] private float moveSpeed = 5.6f;
        [SerializeField] private float minX = -4.3f;
        [SerializeField] private float maxX = 4.3f;
        [SerializeField] private float minZ = 0.6f;
        [SerializeField] private float maxZ = 12.5f;
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float justDodgeEnergyReward = 26f;
        [SerializeField] private float postureAnchorRefreshInterval = 0.12f;
        [SerializeField] private float directDodgeDistance = 1.55f;
        [SerializeField] private float directDodgeDuration = 0.18f;
        [SerializeField] private float directDodgeCooldown = 0.24f;
        [SerializeField] private float postDodgeAnchorLockDuration = 0.22f;
        [SerializeField] private float retreatStateMinimumDuration = 0.3f;
        [SerializeField] private float postRetreatRecoverDuration = 0.24f;
        [SerializeField] private float slotStateStabilityDuration = 0.2f;
        [SerializeField] private float autoSummonFocusHoldDuration = 0.85f;
        [SerializeField] private float allyLossLingerDuration = 1.1f;
        [SerializeField] private float laneDecisionWindow = 0.45f;
        [SerializeField] private float laneSwitchScoreThreshold = 1.15f;
        [SerializeField] private float laneSwitchCooldownDuration = 0.35f;
        [SerializeField] private float supportAnchorDecisionInterval = 0.30f;
        [SerializeField] private float supportAnchorLineOfFireWeight = 2.2f;
        [SerializeField] private float supportAnchorCoverWeight = 0.9f;
        [SerializeField] private float supportAnchorTravelPenaltyWeight = 0.18f;
        [SerializeField] private float supportAnchorCrowdPenaltyWeight = 0.45f;
        [SerializeField] private float supportAnchorExposurePenaltyWeight = 0.42f;
        [SerializeField] private float supportAnchorStabilityBonus = 0.34f;
        [SerializeField] private float supportAnchorSwitchMargin = 0.35f;
        [SerializeField] private float preferredLaneHintDuration = 1.2f;
        [SerializeField] private float preferredLaneInputCooldown = 0.65f;
        [SerializeField] private float directDodgeRearBias = 0.45f;
        [SerializeField] private float sameLaneDirectDodgeRearBias = 0.8f;
        [SerializeField] private float respawnDelay = 5f;
        [SerializeField] private float respawnInvulnerabilityDuration = 1.2f;
        [SerializeField] private float hitShakeDuration = 0.15f;
        [SerializeField] private float hitShakeMagnitude = 0.2f;
        [SerializeField] private float softCoverDamageMultiplier = 0.82f;
        [SerializeField] private float exposedDamageMultiplier = 1.08f;
        [SerializeField] private JustDodgeDetector justDodgeDetector;
        [SerializeField] private ParticleSystem justDodgeEffect;
        [SerializeField] private Animator characterAnimator;

        private Rigidbody cachedRigidbody;
        private CapsuleCollider cachedCapsuleCollider;
        private bool isWalkAnimationActive;
        private bool isRespawning;
        private float respawnEndsAt;
        private float respawnInvulnerableUntil;
        private Coroutine respawnRoutine;
        private bool isDirectDodging;
        private float nextDirectDodgeAllowedTime;
        private float directDodgeStartedAt;
        private float directDodgeEndsAt;
        private Vector3 directDodgeStartPosition;
        private Vector3 directDodgeTargetPosition;
        private Vector2 directDodgeInputVector;
        private Vector3 autoCombatAnchor;
        private float nextAutoCombatAnchorRefreshTime;
        private bool hasAutoCombatAnchor;
        private int focusLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private int escortLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private int preferredLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private float nextLaneDecisionTime;
        private float laneSwitchCooldownUntil;
        private int pendingLaneSwitchTarget = -1;
        private float nextSupportAnchorDecisionTime;
        private int selectedSupportAnchorIndex = 1;
        private int selectedSupportAnchorLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private Vector3 selectedSupportAnchor;
        private readonly float[] currentSupportAnchorScores = new float[3];
        private float currentSupportAnchorScore;
        private string currentSupportAnchorLabel = "CENTER";
        private FocusLaneReason currentFocusLaneReason = FocusLaneReason.Hold;
        private PlayerMotionState currentMotionState = PlayerMotionState.SlotFollow;
        private BattleManager.PlayerLaneSlot currentPlayerLaneSlot = BattleManager.PlayerLaneSlot.SupportCover;
        private BattleManager.PlayerLaneSlot desiredPlayerLaneSlot = BattleManager.PlayerLaneSlot.SupportCover;
        private BattleManager.PlayerLaneSlot stableFollowSlot = BattleManager.PlayerLaneSlot.SupportCover;
        private BattleManager.PlayerLaneSlot slotTransitionCandidate = BattleManager.PlayerLaneSlot.SupportCover;
        private BattleManager.CoverState currentCoverState = BattleManager.CoverState.SoftCover;
        private BattleManager.LanePressureState currentLanePressureState = BattleManager.LanePressureState.Empty;
        private string currentMovementReasonLabel = "COVER";
        private HeroInterventionReason currentInterventionReason = HeroInterventionReason.Escort;
        private BattleManager.EscortPhase currentEscortPhase = BattleManager.EscortPhase.Ready;
        private BattleManager.HeroLaneDepthBand currentLeashDepthBand = BattleManager.HeroLaneDepthBand.Approach;
        private float slotTransitionCandidateSince;
        private float motionStateUntil;
        private bool recoveryForcesSupportCover;
        private float lastFriendlySummonTime = float.NegativeInfinity;
        private int lastFriendlySummonLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private float preferredLaneExpiresAt = float.NegativeInfinity;
        private float preferredLaneCooldownUntil = float.NegativeInfinity;
        private float allyLossLingerUntil = float.NegativeInfinity;
        private int allyLossLingerLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private float currentDesiredAnchorZ = float.NaN;
        private float currentFrontlineObjectiveZ = float.NaN;
        private float currentLeashMaxForwardZ = float.NaN;
        private bool currentLaneHasLiveAllies;
        private bool currentLaneIsPrimed;
        private bool currentMovementIsLeashed;
        private PlayerRetreatReason currentRetreatReason = PlayerRetreatReason.None;
        private bool isResolvingAutoCombatAnchor;
        private bool recoveryHoldPosition;
        private int recoveringFocusLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private BattleManager.LanePressureState recoveringPressureState = BattleManager.LanePressureState.Empty;
        private ManualTargetLockKind manualTargetLockKind;
        private SummonUnit lockedEnemySummon;
        private EnemyAI lockedBoss;
        private BattleStructure lockedStructure;
        private SummonSpawner summonSpawner;
        private EnemyAI enemyAI;

        public float CurrentHP { get; private set; }
        public float MaxHP => maxHP;
        public Vector2 CurrentMoveInput { get; private set; }
        public bool HasMovementInput => CurrentMoveInput.sqrMagnitude > 0.01f;
        public bool IsMovingForward { get; private set; }
        public bool IsRespawning => isRespawning;
        public bool IsDirectDodging => isDirectDodging;
        public int CurrentLaneIndex => focusLaneIndex;
        public int FocusLaneIndex => focusLaneIndex;
        public int EscortLaneIndex => escortLaneIndex;
        public int PreferredLaneIndex => preferredLaneIndex;
        public float LaneSwitchCooldownRemaining => Mathf.Max(0f, laneSwitchCooldownUntil - Time.time);
        public FocusLaneReason CurrentFocusLaneReason => currentFocusLaneReason;
        public PlayerMotionState CurrentMotionState => currentMotionState;
        public float PreferredLaneCooldownRemaining => Mathf.Max(0f, preferredLaneCooldownUntil - Time.time);
        public BattleManager.PlayerLaneSlot CurrentPlayerLaneSlot => currentPlayerLaneSlot;
        public BattleManager.PlayerLaneSlot DesiredPlayerLaneSlot => desiredPlayerLaneSlot;
        public BattleManager.CoverState CurrentCoverState => currentCoverState;
        public BattleManager.LanePressureState CurrentLanePressureState => currentLanePressureState;
        public string CurrentMovementReasonLabel => currentMovementReasonLabel;
        public HeroInterventionReason CurrentInterventionReason => currentInterventionReason;
        public BattleManager.EscortPhase CurrentEscortPhase => currentEscortPhase;
        public BattleManager.HeroLaneDepthBand CurrentLeashDepthBand => currentLeashDepthBand;
        public float DesiredAnchorZ => currentDesiredAnchorZ;
        public float FrontlineObjectiveZ => currentFrontlineObjectiveZ;
        public float LeashMaxForwardZ => currentLeashMaxForwardZ;
        public bool CurrentLaneHasLiveAllies => currentLaneHasLiveAllies;
        public bool CurrentLaneIsPrimed => currentLaneIsPrimed;
        public bool IsMovementLeashed => currentMovementIsLeashed;
        public PlayerRetreatReason CurrentRetreatReason => currentRetreatReason;
        public string CurrentSupportAnchorLabel => currentSupportAnchorLabel;
        public float SelectedSupportAnchorZ => selectedSupportAnchor.z;
        public int CurrentSupportAnchorIndex => selectedSupportAnchorIndex;
        public float CurrentSupportAnchorScore => currentSupportAnchorScore;
        public string CurrentSupportAnchorScoresSummary => $"{currentSupportAnchorScores[0]:0.00} / {currentSupportAnchorScores[1]:0.00} / {currentSupportAnchorScores[2]:0.00}";
        public int PendingLaneSwitchTarget => pendingLaneSwitchTarget;
        public float RemainingRespawnTime => isRespawning ? Mathf.Max(0f, respawnEndsAt - Time.time) : 0f;
        public bool CanAct => !isRespawning && CurrentHP > 0.001f;
        public ManualTargetLockKind CurrentManualTargetLockKind
        {
            get
            {
                return TryGetManualTargetLock(out _, out _, out ManualTargetLockKind kind)
                    ? kind
                    : ManualTargetLockKind.None;
            }
        }

        public bool HasManualTargetLock => TryGetManualTargetLock(out _, out _, out _);
        public bool HasHardManualTargetLock =>
            CurrentManualTargetLockKind == ManualTargetLockKind.Boss ||
            CurrentManualTargetLockKind == ManualTargetLockKind.Structure;

        public Transform LockedTargetTransform
        {
            get
            {
                return TryGetManualTargetLock(out Transform targetTransform, out _, out _)
                    ? targetTransform
                    : null;
            }
        }

        public int LockedTargetLaneIndex
        {
            get
            {
                return TryGetManualTargetLock(out _, out int laneIndex, out _)
                    ? laneIndex
                    : -1;
            }
        }

        private void Awake()
        {
            EnsureCollisionBody();
            cachedRigidbody = GetComponent<Rigidbody>();
            cachedRigidbody.useGravity = false;
            cachedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            if (justDodgeDetector == null)
            {
                justDodgeDetector = GetComponent<JustDodgeDetector>();
            }

            if (justDodgeEffect == null)
            {
                justDodgeEffect = CreateJustDodgeEffect();
            }

            if (characterAnimator == null)
            {
                characterAnimator = GetComponentInChildren<Animator>();
            }

            if (characterAnimator != null)
            {
                characterAnimator.applyRootMotion = false;
            }

            if (GetComponent<PlayerSkillController>() == null)
            {
                gameObject.AddComponent<PlayerSkillController>();
            }

            if (GetComponent<PlayerCombatController>() == null)
            {
                gameObject.AddComponent<PlayerCombatController>();
            }

            CurrentHP = maxHP;
        }

        private void OnEnable()
        {
            if (justDodgeDetector != null)
            {
                justDodgeDetector.OnJustDodge += HandleJustDodge;
            }
        }

        private void OnDisable()
        {
            if (justDodgeDetector != null)
            {
                justDodgeDetector.OnJustDodge -= HandleJustDodge;
            }

            if (summonSpawner != null)
            {
                summonSpawner.OnSummonSpawned -= HandleFriendlySummonSpawned;
            }

            if (enemyAI != null)
            {
                enemyAI.OnSummonSpawned -= HandleEnemySummonSpawned;
            }

        }

        private void Start()
        {
            BattleManager.Instance?.RegisterPlayer(this);
            if (BattleManager.Instance != null)
            {
                focusLaneIndex = BattleManager.Instance.GetNearestLaneIndex(transform.position.x);
                escortLaneIndex = focusLaneIndex;
                preferredLaneIndex = focusLaneIndex;
            }

            summonSpawner = FindFirstObjectByType<SummonSpawner>();
            if (summonSpawner != null)
            {
                summonSpawner.OnSummonSpawned += HandleFriendlySummonSpawned;
            }

            enemyAI = FindFirstObjectByType<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.OnSummonSpawned += HandleEnemySummonSpawned;
            }

            RefreshIgnoredCollisions();

            AlignVisualToGround();
            UpdateAnimatorState(forceRefresh: true);
            OnHPChanged?.Invoke(CurrentHP, maxHP);
        }

        private void Update()
        {
            if (isRespawning)
            {
                CurrentMoveInput = Vector2.zero;
                IsMovingForward = false;
                UpdateAnimatorParameters();
                UpdateAnimatorState();
                return;
            }

            if (MobileBattleControls.TryConsumeDirectDodge(out float directDodgeDirection))
            {
                TryPerformDirectDodge(directDodgeDirection);
            }

            if (isDirectDodging)
            {
                CurrentMoveInput = directDodgeInputVector;
                IsMovingForward = false;
                UpdateAnimatorParameters();
                UpdateAnimatorState();
                UpdateFacingRotation();
                return;
            }

            if (IsAutoCombatMovementActive())
            {
                UpdateAutoCombatMovement();
                UpdateAnimatorParameters();
                UpdateAnimatorState();
                UpdateFacingRotation();
                return;
            }

            CurrentMoveInput = ReadMoveInput();
            IsMovingForward = CurrentMoveInput.y > 0.01f;

            UpdateAnimatorParameters();
            UpdateAnimatorState();
            UpdateFacingRotation();
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            if (!CanAct)
            {
                return;
            }

            if (isDirectDodging)
            {
                float progress = directDodgeEndsAt <= directDodgeStartedAt
                    ? 1f
                    : Mathf.Clamp01((Time.time - directDodgeStartedAt) / (directDodgeEndsAt - directDodgeStartedAt));
                Vector3 dodgePosition = Vector3.Lerp(
                    directDodgeStartPosition,
                    directDodgeTargetPosition,
                    Mathf.SmoothStep(0f, 1f, progress));
                cachedRigidbody.MovePosition(dodgePosition);
                if (progress >= 0.999f)
                {
                    isDirectDodging = false;
                    CurrentMoveInput = Vector2.zero;
                    directDodgeInputVector = Vector2.zero;
                    autoCombatAnchor = dodgePosition;
                    nextAutoCombatAnchorRefreshTime = Time.time;
                    hasAutoCombatAnchor = true;
                    EnterMotionState(PlayerMotionState.Recovering, postDodgeAnchorLockDuration, holdPosition: true);
                }

                return;
            }

            if (IsAutoCombatMovementActive())
            {
                if (!hasAutoCombatAnchor)
                {
                    RefreshAutoCombatAnchor(forceRefresh: true);
                }

                Vector3 autoTargetPosition = autoCombatAnchor;
                autoTargetPosition.y = cachedRigidbody.position.y;
                Vector3 anchorDelta = autoTargetPosition - cachedRigidbody.position;
                anchorDelta.y = 0f;
                float maxStep = moveSpeed * Time.fixedDeltaTime;
                Vector3 nextPosition = anchorDelta.sqrMagnitude <= maxStep * maxStep
                    ? autoTargetPosition
                    : cachedRigidbody.position + (anchorDelta.normalized * maxStep);
                nextPosition = ClampToMovementBounds(nextPosition);
                cachedRigidbody.MovePosition(nextPosition);
                return;
            }

            Vector3 moveVector = new(CurrentMoveInput.x, 0f, CurrentMoveInput.y);
            Vector3 normalizedMove = moveVector.sqrMagnitude > 1f ? moveVector.normalized : moveVector;
            Vector3 targetPosition = cachedRigidbody.position + (normalizedMove * moveSpeed * Time.fixedDeltaTime);
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
            cachedRigidbody.MovePosition(targetPosition);
        }

        private void LateUpdate()
        {
            AlignVisualToGround();
        }

        public void ConfigureMovementBounds(float newMinX, float newMaxX, float newMinZ, float newMaxZ)
        {
            minX = newMinX;
            maxX = newMaxX;
            minZ = newMinZ;
            maxZ = newMaxZ;

            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
            transform.position = clampedPosition;
        }

        public Vector3 ClampToMovementBounds(Vector3 worldPosition)
        {
            worldPosition.x = Mathf.Clamp(worldPosition.x, minX, maxX);
            worldPosition.z = Mathf.Clamp(worldPosition.z, minZ, maxZ);
            return worldPosition;
        }

        public void ConfigureJustDodgeReward(float energyReward)
        {
            justDodgeEnergyReward = Mathf.Max(0f, energyReward);
        }

        public void ApplyExternalDisplacement(Vector3 worldOffset)
        {
            Vector3 targetPosition = transform.position + worldOffset;
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

            if (isDirectDodging)
            {
                isDirectDodging = false;
                directDodgeInputVector = Vector2.zero;
                CurrentMoveInput = Vector2.zero;
            }

            if (cachedRigidbody != null)
            {
                cachedRigidbody.position = targetPosition;
                return;
            }

            transform.position = targetPosition;
        }

        public bool TryPerformDirectDodge(float directionSign)
        {
            if (!CanAct || isDirectDodging || Time.time < nextDirectDodgeAllowedTime)
            {
                return false;
            }

            float resolvedDirection = Mathf.Sign(directionSign);
            if (Mathf.Abs(resolvedDirection) <= 0.001f)
            {
                return false;
            }

            Vector3 startPosition = cachedRigidbody != null ? cachedRigidbody.position : transform.position;
            BattleManager battleManager = BattleManager.Instance;
            int originLaneIndex = battleManager != null
                ? BattleLaneUtility.ClampLaneIndex(focusLaneIndex, battleManager.LaneCount)
                : focusLaneIndex;
            int targetLaneIndex = BattleLaneUtility.ClampLaneIndex(originLaneIndex + (resolvedDirection > 0f ? 1 : -1));
            bool canShiftToAdjacentLane = battleManager != null && targetLaneIndex != originLaneIndex;
            float targetX = battleManager != null
                ? battleManager.GetLaneCenterX(canShiftToAdjacentLane ? targetLaneIndex : originLaneIndex)
                : startPosition.x + (resolvedDirection * directDodgeDistance);
            float rearBias = canShiftToAdjacentLane ? directDodgeRearBias : sameLaneDirectDodgeRearBias;
            Vector3 targetPosition = ClampToMovementBounds(new Vector3(
                targetX,
                startPosition.y,
                startPosition.z - rearBias));
            if (Mathf.Abs(targetPosition.x - startPosition.x) <= 0.04f && Mathf.Abs(targetPosition.z - startPosition.z) <= 0.12f)
            {
                return false;
            }

            isDirectDodging = true;
            currentMotionState = PlayerMotionState.Dodging;
            nextDirectDodgeAllowedTime = Time.time + Mathf.Max(0.05f, directDodgeCooldown);
            directDodgeStartedAt = Time.time;
            directDodgeEndsAt = Time.time + Mathf.Max(0.04f, directDodgeDuration);
            directDodgeStartPosition = startPosition;
            directDodgeTargetPosition = targetPosition;
            directDodgeInputVector = new Vector2(resolvedDirection, 0f);
            CurrentMoveInput = directDodgeInputVector;
            IsMovingForward = false;
            return true;
        }

        public bool SetFocusLane(int laneIndex, bool markManualSelection = true)
        {
            if (BattleManager.Instance == null)
            {
                return false;
            }

            int nextLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, BattleManager.Instance.LaneCount);
            bool changed = nextLaneIndex != escortLaneIndex;
            escortLaneIndex = nextLaneIndex;
            preferredLaneIndex = nextLaneIndex;
            lastFriendlySummonLaneIndex = nextLaneIndex;
            lastFriendlySummonTime = Time.time;
            allyLossLingerLaneIndex = nextLaneIndex;
            allyLossLingerUntil = float.NegativeInfinity;
            if (markManualSelection)
            {
                preferredLaneExpiresAt = Time.time + Mathf.Max(0.1f, preferredLaneHintDuration);
                preferredLaneCooldownUntil = Time.time + Mathf.Max(0.05f, preferredLaneInputCooldown);
            }

            if (!isResolvingAutoCombatAnchor)
            {
                RefreshAutoCombatAnchor(forceRefresh: true);
            }

            return changed;
        }

        public bool ToggleManualTarget(SummonUnit target)
        {
            if (target == null || !target.IsAlive || target.IsPlayerTeam)
            {
                return false;
            }

            if (manualTargetLockKind == ManualTargetLockKind.NormalEnemy && lockedEnemySummon == target)
            {
                ClearManualTargetLock();
                return true;
            }

            lockedEnemySummon = target;
            lockedBoss = null;
            lockedStructure = null;
            manualTargetLockKind = ManualTargetLockKind.NormalEnemy;
            EnterMotionState(PlayerMotionState.SlotFollow);
            RefreshAutoCombatAnchor(forceRefresh: true);
            return true;
        }

        public bool ToggleManualTarget(EnemyAI target)
        {
            if (target == null || !target.isActiveAndEnabled || !target.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (manualTargetLockKind == ManualTargetLockKind.Boss && lockedBoss == target)
            {
                ClearManualTargetLock();
                return true;
            }

            lockedEnemySummon = null;
            lockedBoss = target;
            lockedStructure = null;
            manualTargetLockKind = ManualTargetLockKind.Boss;
            EnterMotionState(PlayerMotionState.SlotFollow);
            RefreshAutoCombatAnchor(forceRefresh: true);
            return true;
        }

        public bool ToggleManualTarget(BattleStructure target)
        {
            if (target == null || target.IsDestroyed || !target.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (manualTargetLockKind == ManualTargetLockKind.Structure && lockedStructure == target)
            {
                ClearManualTargetLock();
                return true;
            }

            lockedEnemySummon = null;
            lockedBoss = null;
            lockedStructure = target;
            manualTargetLockKind = ManualTargetLockKind.Structure;
            EnterMotionState(PlayerMotionState.SlotFollow);
            RefreshAutoCombatAnchor(forceRefresh: true);
            return true;
        }

        public void ClearManualTargetLock()
        {
            ClearManualTargetLockInternal(refreshAnchor: true);
        }

        public bool TryGetManualTargetLock(out Transform targetTransform, out int laneIndex, out ManualTargetLockKind kind)
        {
            return TryResolveManualTargetLockState(
                BattleManager.Instance,
                out targetTransform,
                out laneIndex,
                out kind);
        }

        private bool IsAutoCombatMovementActive()
        {
            // The prototype no longer supports free movement as a primary loop.
            // Player positioning is always resolved through the focus-lane slot rules.
            return true;
        }

        private void UpdateAutoCombatMovement()
        {
            RefreshAutoCombatAnchor(forceRefresh: false);

            Vector3 planarDelta = autoCombatAnchor - transform.position;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= 0.01f)
            {
                CurrentMoveInput = Vector2.zero;
                IsMovingForward = false;
                return;
            }

            Vector2 autoMoveInput = Vector2.ClampMagnitude(new Vector2(planarDelta.x, planarDelta.z), 1f);
            CurrentMoveInput = autoMoveInput;
            IsMovingForward = autoMoveInput.y > 0.01f;
        }

        private void RefreshAutoCombatAnchor(bool forceRefresh)
        {
            if (!forceRefresh && Time.time < nextAutoCombatAnchorRefreshTime && hasAutoCombatAnchor)
            {
                return;
            }

            nextAutoCombatAnchorRefreshTime = Time.time + Mathf.Max(0.05f, postureAnchorRefreshInterval);
            autoCombatAnchor = ResolveAutoCombatAnchor();
            hasAutoCombatAnchor = true;
        }

        private Vector3 ResolveAutoCombatAnchor()
        {
            Vector3 currentPosition = cachedRigidbody != null ? cachedRigidbody.position : transform.position;
            BattleManager battleManager = BattleManager.Instance;
            isResolvingAutoCombatAnchor = true;
            try
            {
                if (battleManager == null)
                {
                    currentDesiredAnchorZ = currentPosition.z;
                    currentFocusLaneReason = FocusLaneReason.Hold;
                    currentInterventionReason = HeroInterventionReason.Escort;
                    currentEscortPhase = BattleManager.EscortPhase.Ready;
                    currentLeashDepthBand = BattleManager.HeroLaneDepthBand.Approach;
                    currentLeashMaxForwardZ = float.NaN;
                    currentLaneHasLiveAllies = false;
                    currentLaneIsPrimed = false;
                    currentMovementIsLeashed = false;
                    currentRetreatReason = PlayerRetreatReason.None;
                    return ClampToMovementBounds(currentPosition);
                }

                bool hasManualTargetLock = TryResolveManualTargetLockState(
                    battleManager,
                    out Transform manualTargetTransform,
                    out int manualTargetLaneIndex,
                    out ManualTargetLockKind manualTargetKind);
                int previousFocusLane = focusLaneIndex;
                bool previousLaneHadLiveAllies = currentLaneHasLiveAllies;
                int resolvedEscortLane = ResolveEscortLaneIndex(battleManager);
                bool manualLockAffectsMovement = hasManualTargetLock &&
                    BattleLaneUtility.ClampLaneIndex(manualTargetLaneIndex, battleManager.LaneCount) == resolvedEscortLane;
                escortLaneIndex = resolvedEscortLane;
                preferredLaneIndex = resolvedEscortLane;
                focusLaneIndex = resolvedEscortLane;
                currentFocusLaneReason = manualLockAffectsMovement
                    ? FocusLaneReason.Threat
                    : resolvedEscortLane != previousFocusLane
                        ? FocusLaneReason.Preferred
                        : FocusLaneReason.Hold;
                BattleManager.EscortPhase previousEscortPhase = currentEscortPhase;

                if (battleManager.TryGetLaneCombatState(focusLaneIndex, out BattleManager.LaneCombatState laneState))
                {
                    bool laneJustLostAllies = previousFocusLane == focusLaneIndex &&
                        previousLaneHadLiveAllies &&
                        !laneState.HasLiveAllies;
                    if (laneState.HasLiveAllies)
                    {
                        allyLossLingerLaneIndex = focusLaneIndex;
                        allyLossLingerUntil = float.NegativeInfinity;
                    }
                    else if (laneJustLostAllies)
                    {
                        allyLossLingerLaneIndex = focusLaneIndex;
                        allyLossLingerUntil = Time.time + Mathf.Max(0.1f, allyLossLingerDuration);
                    }

                    currentLanePressureState = laneState.PressureState;
                    desiredPlayerLaneSlot = laneState.SuggestedPlayerSlot;
                    currentEscortPhase = laneState.EscortPhase;
                    currentLeashDepthBand = laneState.MaxDepthBand;
                    currentLeashMaxForwardZ = laneState.MaxForwardZ;
                    currentLaneHasLiveAllies = laneState.HasLiveAllies;
                    currentLaneIsPrimed = laneState.HasRecentPrime;
                    currentMovementIsLeashed = false;
                    currentFrontlineObjectiveZ = laneState.HasFrontlineObjective ? laneState.FrontlineObjectiveZ : float.NaN;
                    if (battleManager.TryGetHeroLaneLeashState(
                        focusLaneIndex,
                        hasManualTargetLock ? manualTargetKind : ManualTargetLockKind.None,
                        out BattleManager.HeroLaneLeashState leashState))
                    {
                        currentEscortPhase = leashState.EscortPhase;
                        currentLeashDepthBand = leashState.MaxDepthBand;
                        currentLeashMaxForwardZ = leashState.MaxForwardZ;
                        currentLaneHasLiveAllies = leashState.HasLiveAllies;
                        currentLaneIsPrimed = leashState.HasRecentPrime;
                        currentInterventionReason = leashState.InterventionReason;
                    }
                    else
                    {
                        currentInterventionReason = laneState.InterventionReason;
                    }

                    currentRetreatReason = ResolveCurrentRetreatReason(battleManager, laneState, currentPosition.z, previousEscortPhase);

                    if (currentMotionState == PlayerMotionState.Dodging)
                    {
                        currentMovementReasonLabel = "DODGE";
                        currentDesiredAnchorZ = currentPosition.z;
                        currentMovementIsLeashed = false;
                        return currentPosition;
                    }

                    if (currentRetreatReason != PlayerRetreatReason.None &&
                        currentMotionState != PlayerMotionState.Retreating)
                    {
                        EnterMotionState(PlayerMotionState.Retreating, retreatStateMinimumDuration);
                    }

                    switch (currentMotionState)
                    {
                        case PlayerMotionState.Retreating:
                        {
                            currentPlayerLaneSlot = BattleManager.PlayerLaneSlot.Rear;
                            currentCoverState = ResolveCoverStateForSlot(currentPlayerLaneSlot);
                            Vector3 retreatAnchor = ResolveFallbackPhaseAnchor(laneState, currentPosition.y);
                            currentDesiredAnchorZ = retreatAnchor.z;
                            currentMovementReasonLabel = "FALL BACK";
                            currentMovementIsLeashed = false;

                            if (currentRetreatReason == PlayerRetreatReason.None &&
                                Time.time >= motionStateUntil &&
                                IsNearAnchor(currentPosition, retreatAnchor))
                            {
                                EnterMotionState(PlayerMotionState.Recovering, postRetreatRecoverDuration, forceSupportCover: true);
                            }

                            return retreatAnchor;
                        }

                        case PlayerMotionState.Recovering:
                        {
                            if (currentRetreatReason != PlayerRetreatReason.None)
                            {
                                EnterMotionState(PlayerMotionState.Retreating, retreatStateMinimumDuration);
                                goto case PlayerMotionState.Retreating;
                            }

                            if (recoveryHoldPosition)
                            {
                                if (Time.time < motionStateUntil)
                                {
                                    currentFocusLaneReason = FocusLaneReason.Hold;
                                    currentPlayerLaneSlot = BattleManager.PlayerLaneSlot.SupportCover;
                                    currentCoverState = ResolveCoverStateForSlot(BattleManager.PlayerLaneSlot.SupportCover);
                                    currentDesiredAnchorZ = currentPosition.z;
                                    currentMovementReasonLabel = "RESET";
                                    UpdateLeashFlag(currentDesiredAnchorZ);
                                    return currentPosition;
                                }

                                recoveryHoldPosition = false;
                            }

                            ResolvePhaseMovementDirective(
                                battleManager,
                                laneState,
                                currentPosition,
                                out BattleManager.PlayerLaneSlot resolvedSlot,
                                out Vector3 recoveryAnchor,
                                out string recoveryLabel,
                                out BattleManager.EscortPhase recoveryPhase);
                            currentPlayerLaneSlot = recoveryForcesSupportCover
                                ? BattleManager.PlayerLaneSlot.SupportCover
                                : resolvedSlot;
                            desiredPlayerLaneSlot = currentPlayerLaneSlot;
                            currentCoverState = ResolveCoverStateForSlot(currentPlayerLaneSlot);
                            currentEscortPhase = recoveryPhase;
                            currentDesiredAnchorZ = recoveryAnchor.z;
                            UpdateLeashFlag(currentDesiredAnchorZ);
                            currentMovementReasonLabel = recoveryForcesSupportCover ? "RESET" : recoveryLabel;

                            if (!MobileBattleControls.IsDirectDodgeModeActive &&
                                Time.time >= motionStateUntil &&
                                IsNearAnchor(currentPosition, recoveryAnchor))
                            {
                                recoveryForcesSupportCover = false;
                                EnterMotionState(PlayerMotionState.SlotFollow);
                            }

                            return recoveryAnchor;
                        }

                        case PlayerMotionState.SlotFollow:
                        default:
                        {
                            ResolvePhaseMovementDirective(
                                battleManager,
                                laneState,
                                currentPosition,
                                out currentPlayerLaneSlot,
                                out Vector3 followAnchor,
                                out string followLabel,
                                out BattleManager.EscortPhase followPhase);
                            desiredPlayerLaneSlot = currentPlayerLaneSlot;
                            currentCoverState = ResolveCoverStateForSlot(currentPlayerLaneSlot);
                            currentEscortPhase = followPhase;
                            currentMovementReasonLabel = followLabel;
                            currentDesiredAnchorZ = followAnchor.z;
                            UpdateLeashFlag(currentDesiredAnchorZ);
                            return followAnchor;
                        }
                    }
                }

                currentLanePressureState = BattleManager.LanePressureState.Empty;
                desiredPlayerLaneSlot = BattleManager.PlayerLaneSlot.SupportCover;
                currentPlayerLaneSlot = BattleManager.PlayerLaneSlot.SupportCover;
                currentCoverState = BattleManager.CoverState.SoftCover;
                currentMovementReasonLabel = "COVER";
                currentInterventionReason = HeroInterventionReason.Escort;
                currentEscortPhase = BattleManager.EscortPhase.Ready;
                currentLeashDepthBand = BattleManager.HeroLaneDepthBand.Approach;
                currentFrontlineObjectiveZ = float.NaN;
                currentLeashMaxForwardZ = float.NaN;
                currentLaneHasLiveAllies = false;
                currentLaneIsPrimed = false;
                currentMovementIsLeashed = false;
                currentRetreatReason = PlayerRetreatReason.None;
                if (battleManager != null)
                {
                    Vector3 fallbackAnchor = ClampToMovementBounds(battleManager.ResolvePlayerSlotAnchor(focusLaneIndex, currentPlayerLaneSlot, currentPosition.y));
                    currentDesiredAnchorZ = fallbackAnchor.z;
                    return fallbackAnchor;
                }

                currentDesiredAnchorZ = currentPosition.z;
                return ClampToMovementBounds(currentPosition);
            }
            finally
            {
                isResolvingAutoCombatAnchor = false;
            }
        }

        private bool TryResolveOptimizedFocusLane(out int laneIndex, out FocusLaneReason reason)
        {
            laneIndex = BattleLaneUtility.ClampLaneIndex(focusLaneIndex, BattleManager.Instance != null ? BattleManager.Instance.LaneCount : BattleLaneUtility.DefaultLaneCount);
            reason = FocusLaneReason.Hold;
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                return false;
            }

            if (TryResolveManualTargetLockState(battleManager, out _, out int lockedLaneIndex, out _))
            {
                laneIndex = lockedLaneIndex;
                return true;
            }

            laneIndex = ResolveEscortLaneIndex(battleManager);
            if (battleManager.TryGetLaneCombatState(laneIndex, out BattleManager.LaneCombatState escortLaneState) &&
                escortLaneState.PressureState == BattleManager.LanePressureState.Collapse)
            {
                reason = FocusLaneReason.Retreat;
            }

            return true;
        }

        private void ClearManualTargetLockInternal(bool refreshAnchor)
        {
            manualTargetLockKind = ManualTargetLockKind.None;
            lockedEnemySummon = null;
            lockedBoss = null;
            lockedStructure = null;

            if (refreshAnchor && !isResolvingAutoCombatAnchor)
            {
                RefreshAutoCombatAnchor(forceRefresh: true);
            }
        }

        private bool TryResolveManualTargetLockState(
            BattleManager battleManager,
            out Transform targetTransform,
            out int laneIndex,
            out ManualTargetLockKind kind)
        {
            targetTransform = null;
            laneIndex = BattleLaneUtility.ClampLaneIndex(escortLaneIndex, battleManager != null ? battleManager.LaneCount : BattleLaneUtility.DefaultLaneCount);
            kind = ManualTargetLockKind.None;

            switch (manualTargetLockKind)
            {
                case ManualTargetLockKind.NormalEnemy:
                    if (lockedEnemySummon != null && lockedEnemySummon.IsAlive && !lockedEnemySummon.IsPlayerTeam)
                    {
                        targetTransform = lockedEnemySummon.transform;
                        laneIndex = battleManager != null
                            ? BattleLaneUtility.ClampLaneIndex(lockedEnemySummon.AssignedLaneIndex, battleManager.LaneCount)
                            : BattleLaneUtility.ClampLaneIndex(lockedEnemySummon.AssignedLaneIndex);
                        kind = ManualTargetLockKind.NormalEnemy;
                        return true;
                    }
                    break;

                case ManualTargetLockKind.Boss:
                    if (lockedBoss != null && lockedBoss.isActiveAndEnabled && lockedBoss.gameObject.activeInHierarchy)
                    {
                        targetTransform = lockedBoss.transform;
                        laneIndex = battleManager != null
                            ? battleManager.GetNearestLaneIndex(lockedBoss.transform.position.x)
                            : BattleLaneUtility.ClampLaneIndex(focusLaneIndex);
                        kind = ManualTargetLockKind.Boss;
                        return true;
                    }
                    break;

                case ManualTargetLockKind.Structure:
                    if (lockedStructure != null && !lockedStructure.IsDestroyed && lockedStructure.gameObject.activeInHierarchy)
                    {
                        targetTransform = lockedStructure.transform;
                        laneIndex = battleManager != null
                            ? battleManager.GetNearestLaneIndex(lockedStructure.transform.position.x)
                            : BattleLaneUtility.ClampLaneIndex(focusLaneIndex);
                        kind = ManualTargetLockKind.Structure;
                        return true;
                    }
                    break;
            }

            ClearManualTargetLockInternal(refreshAnchor: false);
            return false;
        }

        private int ResolveEscortLaneIndex(BattleManager battleManager)
        {
            if (battleManager == null)
            {
                return BattleLaneUtility.ClampLaneIndex(escortLaneIndex);
            }

            int currentEscortLane = BattleLaneUtility.ClampLaneIndex(escortLaneIndex, battleManager.LaneCount);
            bool storyPveMode = BattleModeContext.CurrentMode == BattleMode.StoryPve;
            if (!storyPveMode && Time.time <= lastFriendlySummonTime + Mathf.Max(0.2f, autoSummonFocusHoldDuration))
            {
                int recentSummonLane = BattleLaneUtility.ClampLaneIndex(lastFriendlySummonLaneIndex, battleManager.LaneCount);
                pendingLaneSwitchTarget = -1;
                escortLaneIndex = recentSummonLane;
                preferredLaneIndex = recentSummonLane;
                return recentSummonLane;
            }

            if (Time.time < allyLossLingerUntil)
            {
                int lingeringLane = BattleLaneUtility.ClampLaneIndex(allyLossLingerLaneIndex, battleManager.LaneCount);
                escortLaneIndex = lingeringLane;
                preferredLaneIndex = lingeringLane;
                return lingeringLane;
            }

            bool hasCurrentLaneContext = battleManager.TryGetHeroLaneContext(currentEscortLane, out BattleManager.HeroLaneContext currentLaneContext);
            bool currentLaneHasLiveAllies = hasCurrentLaneContext && currentLaneContext.AllyCount > 0;
            bool currentLaneLocked = currentLaneHasLiveAllies &&
                currentLaneContext.PressureState != BattleManager.LanePressureState.Collapse &&
                (currentLaneContext.HasBlocker || currentLaneContext.HasObjective);

            if (currentLaneLocked)
            {
                escortLaneIndex = currentEscortLane;
                preferredLaneIndex = currentEscortLane;
                return currentEscortLane;
            }

            if (pendingLaneSwitchTarget >= 0)
            {
                int resolvedPendingLane = BattleLaneUtility.ClampLaneIndex(pendingLaneSwitchTarget, battleManager.LaneCount);
                if (!currentLaneHasLiveAllies)
                {
                    pendingLaneSwitchTarget = -1;
                    escortLaneIndex = resolvedPendingLane;
                    preferredLaneIndex = resolvedPendingLane;
                    return resolvedPendingLane;
                }

                escortLaneIndex = currentEscortLane;
                preferredLaneIndex = currentEscortLane;
                return currentEscortLane;
            }

            if (Time.time < nextLaneDecisionTime || Time.time < laneSwitchCooldownUntil)
            {
                int stickyLane = currentLaneHasLiveAllies
                    ? currentEscortLane
                    : storyPveMode
                        ? currentEscortLane
                        : FindNearestActivePlayerLane(battleManager, currentEscortLane);
                escortLaneIndex = stickyLane;
                preferredLaneIndex = stickyLane;
                return stickyLane;
            }

            nextLaneDecisionTime = Time.time + Mathf.Max(0.12f, laneDecisionWindow);
            float currentLaneScore = EvaluateLaneDecisionScore(currentLaneContext, currentLaneHasLiveAllies, isCurrentLane: true);
            int bestLaneIndex = currentEscortLane;
            float bestLaneScore = currentLaneScore;

            BattleManager.HeroLaneContext[] laneContexts = battleManager.BuildHeroLaneContexts();
            for (int laneIndex = 0; laneIndex < laneContexts.Length; laneIndex++)
            {
                BattleManager.HeroLaneContext candidate = laneContexts[laneIndex];
                bool hasLiveAllies = candidate.AllyCount > 0;
                if (!hasLiveAllies && !(storyPveMode && IsStrategicallyRelevantLane(candidate)))
                {
                    continue;
                }

                float candidateScore = EvaluateLaneDecisionScore(candidate, hasLiveAllies, laneIndex == currentEscortLane);
                if (candidateScore <= bestLaneScore)
                {
                    continue;
                }

                bestLaneScore = candidateScore;
                bestLaneIndex = laneIndex;
            }

            bool shouldSwitchLane =
                bestLaneIndex != currentEscortLane &&
                (!currentLaneHasLiveAllies || currentLaneContext.PressureState == BattleManager.LanePressureState.Collapse || !currentLaneLocked) &&
                bestLaneScore - currentLaneScore >= (storyPveMode ? Mathf.Min(laneSwitchScoreThreshold, 0.9f) : laneSwitchScoreThreshold);

            if (shouldSwitchLane)
            {
                laneSwitchCooldownUntil = Time.time + Mathf.Max(0.15f, laneSwitchCooldownDuration);
                if (currentLaneHasLiveAllies)
                {
                    pendingLaneSwitchTarget = bestLaneIndex;
                    escortLaneIndex = currentEscortLane;
                    preferredLaneIndex = currentEscortLane;
                    return currentEscortLane;
                }

                pendingLaneSwitchTarget = -1;
                escortLaneIndex = bestLaneIndex;
                preferredLaneIndex = bestLaneIndex;
                return bestLaneIndex;
            }

            if (currentLaneHasLiveAllies)
            {
                pendingLaneSwitchTarget = -1;
                escortLaneIndex = currentEscortLane;
                preferredLaneIndex = currentEscortLane;
                return currentEscortLane;
            }

            pendingLaneSwitchTarget = -1;
            int resolvedLane = bestLaneScore > float.NegativeInfinity
                ? bestLaneIndex
                : FindNearestActivePlayerLane(battleManager, currentEscortLane);
            escortLaneIndex = resolvedLane;
            preferredLaneIndex = resolvedLane;
            return resolvedLane;
        }

        private static bool LaneHasEscortContext(BattleManager battleManager, int laneIndex)
        {
            return battleManager != null &&
                battleManager.TryGetHeroLaneContext(laneIndex, out BattleManager.HeroLaneContext laneContext) &&
                laneContext.AllyCount > 0;
        }

        private static int FindNearestActivePlayerLane(BattleManager battleManager, int originLaneIndex)
        {
            if (battleManager == null)
            {
                return BattleLaneUtility.ClampLaneIndex(originLaneIndex);
            }

            int bestLaneIndex = BattleLaneUtility.ClampLaneIndex(BattleLaneUtility.DefaultLaneCount / 2, battleManager.LaneCount);
            int bestDistance = int.MaxValue;

            for (int laneIndex = 0; laneIndex < battleManager.LaneCount; laneIndex++)
            {
                if (!battleManager.TryGetHeroLaneContext(laneIndex, out BattleManager.HeroLaneContext laneContext) || laneContext.AllyCount <= 0)
                {
                    continue;
                }

                int laneDistance = Mathf.Abs(laneIndex - originLaneIndex);
                if (laneDistance >= bestDistance)
                {
                    continue;
                }

                bestDistance = laneDistance;
                bestLaneIndex = laneIndex;
            }

            return bestLaneIndex;
        }

        private float EvaluateLaneDecisionScore(
            BattleManager.HeroLaneContext laneContext,
            bool hasLiveAllies,
            bool isCurrentLane)
        {
            bool storyPveMode = BattleModeContext.CurrentMode == BattleMode.StoryPve;
            if (!hasLiveAllies && !storyPveMode)
            {
                return float.NegativeInfinity;
            }

            if (storyPveMode)
            {
                float storyScore = laneContext.LaneValueScore;
                storyScore -= laneContext.LaneThreatScore * 0.18f;
                storyScore += hasLiveAllies ? 0.85f : 0f;
                storyScore += laneContext.HasBlocker ? 0.35f : 0f;
                storyScore += laneContext.HasObjective ? 0.55f : 0f;
                storyScore += laneContext.HasRecentPrime ? 0.24f : 0f;
                storyScore += Mathf.Min(0.85f, laneContext.EnemyCount * 0.18f);
                storyScore += laneContext.PressureState switch
                {
                    BattleManager.LanePressureState.Collapse => hasLiveAllies ? 0.18f : -0.15f,
                    BattleManager.LanePressureState.Contest => 0.12f,
                    BattleManager.LanePressureState.Push => 0.06f,
                    _ => 0f
                };
                storyScore += isCurrentLane ? 0.30f : 0f;
                if (!hasLiveAllies && !IsStrategicallyRelevantLane(laneContext))
                {
                    storyScore -= 1.2f;
                }

                return storyScore;
            }

            float score = laneContext.LaneValueScore;
            score -= laneContext.LaneThreatScore * 0.24f;
            score += laneContext.HasBlocker ? 0.22f : 0f;
            score += laneContext.HasObjective ? 0.28f : 0f;
            score += laneContext.PressureState switch
            {
                BattleManager.LanePressureState.Push => 0.18f,
                BattleManager.LanePressureState.Contest => 0.06f,
                BattleManager.LanePressureState.Collapse => -0.42f,
                _ => 0f
            };
            score += isCurrentLane ? 0.24f : 0f;
            return score;
        }

        private static bool IsStrategicallyRelevantLane(BattleManager.HeroLaneContext laneContext)
        {
            return laneContext.HasBlocker ||
                laneContext.HasObjective ||
                laneContext.EnemyCount > 0 ||
                laneContext.HasRecentPrime;
        }

        private bool TryResolveManualTargetAnchor(
            BattleManager battleManager,
            BattleManager.LaneCombatState laneState,
            Vector3 currentPosition,
            Transform targetTransform,
            ManualTargetLockKind targetKind,
            out BattleManager.PlayerLaneSlot resolvedSlot,
            out Vector3 resolvedAnchor,
            out string movementLabel)
        {
            resolvedSlot = targetKind switch
            {
                ManualTargetLockKind.Boss => laneState.PressureState == BattleManager.LanePressureState.Collapse
                    ? BattleManager.PlayerLaneSlot.Rear
                    : BattleManager.PlayerLaneSlot.SupportCover,
                ManualTargetLockKind.Structure => laneState.PressureState == BattleManager.LanePressureState.Collapse
                    ? BattleManager.PlayerLaneSlot.Rear
                    : BattleManager.PlayerLaneSlot.SupportCover,
                _ => laneState.PressureState == BattleManager.LanePressureState.Collapse
                    ? BattleManager.PlayerLaneSlot.Rear
                    : BattleManager.PlayerLaneSlot.SupportCover
            };

            resolvedAnchor = ClampToMovementBounds(battleManager.ResolvePlayerSlotAnchor(laneState, resolvedSlot, currentPosition.y));
            movementLabel = targetKind switch
            {
                ManualTargetLockKind.Boss => "LOCK BOSS",
                ManualTargetLockKind.Structure => "LOCK OBJ",
                _ => "LOCK"
            };

            if (targetTransform == null)
            {
                return true;
            }

            PlayerCombatController combatController = GetComponent<PlayerCombatController>();
            float fireRange = combatController != null ? combatController.AutoAttackRange : 9.5f;
            float effectiveRange = Mathf.Max(2.8f, fireRange - 0.6f);
            float horizontalOffset = Mathf.Abs(targetTransform.position.x - resolvedAnchor.x);
            float maxForwardLimit = targetTransform.position.z - (targetKind == ManualTargetLockKind.NormalEnemy ? 1.25f : 0.95f);
            if (horizontalOffset >= effectiveRange || maxForwardLimit <= resolvedAnchor.z + 0.1f)
            {
                return true;
            }

            float depthAllowance = Mathf.Sqrt(Mathf.Max(0.16f, (effectiveRange * effectiveRange) - (horizontalOffset * horizontalOffset)));
            float engagementZ = targetTransform.position.z - depthAllowance;
            float leashForwardCap = laneState.MaxForwardZ;
            float envelopeForwardCap = Mathf.Max(
                laneState.SupportEnvelopeMinZ + 0.2f,
                laneState.SupportEnvelopeMaxZ + 0.2f);
            float forwardCap = Mathf.Min(maxForwardLimit, Mathf.Min(leashForwardCap, envelopeForwardCap));

            resolvedAnchor.z = Mathf.Clamp(
                Mathf.Max(resolvedAnchor.z, engagementZ),
                resolvedAnchor.z,
                Mathf.Min(maxForwardLimit, forwardCap));
            resolvedAnchor = ClampToMovementBounds(resolvedAnchor);
            return true;
        }

        private Vector3 ResolvePhasePrimaryAnchor(BattleManager.LaneCombatState laneState, float worldY)
        {
            Vector3 anchor = laneState.PrimaryAnchor;
            anchor.y = worldY;
            anchor.z = Mathf.Min(anchor.z, laneState.MaxForwardZ);
            return ClampToMovementBounds(anchor);
        }

        private Vector3 ResolveFallbackPhaseAnchor(BattleManager.LaneCombatState laneState, float worldY)
        {
            Vector3 anchor = laneState.FallbackAnchor;
            anchor.y = worldY;
            anchor.z = Mathf.Min(anchor.z, laneState.MaxForwardZ);
            return ClampToMovementBounds(anchor);
        }

        private void ResolvePhaseMovementDirective(
            BattleManager battleManager,
            BattleManager.LaneCombatState laneState,
            Vector3 currentPosition,
            out BattleManager.PlayerLaneSlot resolvedSlot,
            out Vector3 resolvedAnchor,
            out string movementLabel,
            out BattleManager.EscortPhase resolvedPhase)
        {
            resolvedSlot = laneState.EscortPhase == BattleManager.EscortPhase.Fallback
                ? BattleManager.PlayerLaneSlot.Rear
                : BattleManager.PlayerLaneSlot.SupportCover;
            resolvedAnchor = laneState.EscortPhase == BattleManager.EscortPhase.Fallback
                ? ResolveFallbackPhaseAnchor(laneState, currentPosition.y)
                : ResolvePhasePrimaryAnchor(laneState, currentPosition.y);
            movementLabel = ResolveMovementReasonLabel(laneState, resolvedSlot, currentPosition.z);
            resolvedPhase = laneState.EscortPhase;

            if (laneState.EscortPhase == BattleManager.EscortPhase.Ready ||
                laneState.EscortPhase == BattleManager.EscortPhase.Join ||
                laneState.EscortPhase == BattleManager.EscortPhase.Fallback ||
                battleManager == null ||
                !battleManager.TryGetHeroLaneContext(laneState.LaneIndex, out BattleManager.HeroLaneContext laneContext))
            {
                SetSelectedSupportAnchor(1, resolvedAnchor, laneState.LaneIndex);
                SetCurrentSupportAnchorScores(float.NegativeInfinity, 0f, float.NegativeInfinity, 0f);
                return;
            }

            int supportAnchorIndex = ResolveBestSupportAnchorIndex(
                battleManager,
                laneContext,
                currentPosition,
                out Vector3 supportAnchor,
                out HeroFireDirective fireDirective,
                out float[] supportScores,
                out float bestSupportScore);
            SetSelectedSupportAnchor(supportAnchorIndex, supportAnchor, laneState.LaneIndex);
            SetCurrentSupportAnchorScores(
                supportScores.Length > 0 ? supportScores[0] : float.NegativeInfinity,
                supportScores.Length > 1 ? supportScores[1] : float.NegativeInfinity,
                supportScores.Length > 2 ? supportScores[2] : float.NegativeInfinity,
                bestSupportScore);
            resolvedAnchor = supportAnchor;

            bool shouldPeek = laneState.CanOpenPeek &&
                laneContext.CanOpenPeek &&
                laneState.HasLiveAllies &&
                fireDirective.HasTarget &&
                !fireDirective.CanShootFromCurrentAnchor;
            if (shouldPeek)
            {
                resolvedSlot = BattleManager.PlayerLaneSlot.Peek;
                resolvedAnchor = ResolvePeekSupportAnchor(laneState, currentPosition.y, supportAnchor.x);
                resolvedPhase = BattleManager.EscortPhase.Breach;
                movementLabel = laneState.EscortPhase == BattleManager.EscortPhase.Objective ? "OBJECTIVE" : "PEEK";
                return;
            }

            resolvedSlot = BattleManager.PlayerLaneSlot.SupportCover;
            movementLabel = ResolveMovementReasonLabel(laneState, resolvedSlot, currentPosition.z);
        }

        private int ResolveBestSupportAnchorIndex(
            BattleManager battleManager,
            BattleManager.HeroLaneContext laneContext,
            Vector3 currentPosition,
            out Vector3 bestAnchor,
            out HeroFireDirective bestDirective,
            out float[] supportScores,
            out float bestScore)
        {
            Vector3[] supportAnchors = laneContext.SupportAnchors;
            supportScores = new float[3] { float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity };
            if (supportAnchors == null || supportAnchors.Length == 0)
            {
                bestAnchor = ClampToMovementBounds(new Vector3(
                    laneContext.JoinAnchor.x,
                    currentPosition.y,
                    currentLeashMaxForwardZ > 0f ? Mathf.Min(laneContext.JoinAnchor.z, currentLeashMaxForwardZ) : laneContext.JoinAnchor.z));
                bestDirective = GetCombatController()?.BuildFireDirective(bestAnchor, laneContext.LaneIndex) ?? default;
                bestScore = 0f;
                return 1;
            }

            if (Time.time < nextSupportAnchorDecisionTime &&
                selectedSupportAnchorLaneIndex == laneContext.LaneIndex &&
                selectedSupportAnchorIndex >= 0 &&
                selectedSupportAnchorIndex < supportAnchors.Length)
            {
                bestAnchor = supportAnchors[selectedSupportAnchorIndex];
                bestDirective = GetCombatController()?.BuildFireDirective(bestAnchor, laneContext.LaneIndex) ?? default;
                for (int index = 0; index < supportScores.Length && index < currentSupportAnchorScores.Length; index++)
                {
                    supportScores[index] = currentSupportAnchorScores[index];
                }
                bestScore = currentSupportAnchorScore;
                return selectedSupportAnchorIndex;
            }

            nextSupportAnchorDecisionTime = Time.time + Mathf.Max(0.08f, supportAnchorDecisionInterval);
            bestScore = float.NegativeInfinity;
            int bestIndex = 1;
            bestAnchor = supportAnchors[Mathf.Clamp(bestIndex, 0, supportAnchors.Length - 1)];
            bestDirective = default;
            float currentAnchorScore = float.NegativeInfinity;
            bool hasCurrentAnchorScore =
                selectedSupportAnchorLaneIndex == laneContext.LaneIndex &&
                selectedSupportAnchorIndex >= 0 &&
                selectedSupportAnchorIndex < supportAnchors.Length;

            for (int index = 0; index < supportAnchors.Length; index++)
            {
                Vector3 candidateAnchor = ClampToMovementBounds(new Vector3(
                    supportAnchors[index].x,
                    currentPosition.y,
                    Mathf.Min(supportAnchors[index].z, currentLeashMaxForwardZ > 0f ? currentLeashMaxForwardZ : supportAnchors[index].z)));
                HeroFireDirective fireDirective = GetCombatController()?.BuildFireDirective(candidateAnchor, laneContext.LaneIndex) ?? default;
                float score = ScoreSupportAnchor(laneContext, currentPosition, candidateAnchor, fireDirective, index);
                if (index < supportScores.Length)
                {
                    supportScores[index] = score;
                }

                if (hasCurrentAnchorScore && index == selectedSupportAnchorIndex)
                {
                    currentAnchorScore = score;
                }

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestIndex = index;
                bestAnchor = candidateAnchor;
                bestDirective = fireDirective;
            }

            if (hasCurrentAnchorScore &&
                bestIndex != selectedSupportAnchorIndex &&
                currentAnchorScore > float.NegativeInfinity &&
                bestScore < currentAnchorScore + Mathf.Max(0.05f, supportAnchorSwitchMargin))
            {
                bestIndex = selectedSupportAnchorIndex;
                bestAnchor = ClampToMovementBounds(new Vector3(
                    supportAnchors[bestIndex].x,
                    currentPosition.y,
                    Mathf.Min(
                        supportAnchors[bestIndex].z,
                        currentLeashMaxForwardZ > 0f ? currentLeashMaxForwardZ : supportAnchors[bestIndex].z)));
                bestDirective = GetCombatController()?.BuildFireDirective(bestAnchor, laneContext.LaneIndex) ?? default;
                bestScore = currentAnchorScore;
            }

            return bestIndex;
        }

        private float ScoreSupportAnchor(
            BattleManager.HeroLaneContext laneContext,
            Vector3 currentPosition,
            Vector3 candidateAnchor,
            HeroFireDirective fireDirective,
            int anchorIndex)
        {
            float lineOfFireScore = fireDirective.CanShootFromCurrentAnchor
                ? supportAnchorLineOfFireWeight
                : supportAnchorLineOfFireWeight * 0.12f;
            float coverScore = anchorIndex == 1
                ? supportAnchorCoverWeight
                : supportAnchorCoverWeight * 0.8f;
            float travelPenalty = Vector3.Distance(currentPosition, candidateAnchor) * supportAnchorTravelPenaltyWeight;
            float crowdPenalty = CountNearbyAllies(laneContext.LaneIndex, candidateAnchor, 1.1f) * supportAnchorCrowdPenaltyWeight;
            float pressurePenalty = laneContext.PressureState == BattleManager.LanePressureState.Collapse ? 0.85f : 0f;
            float forwardExposure = Mathf.Clamp01(
                (candidateAnchor.z - laneContext.SupportEnvelopeMinZ) /
                Mathf.Max(0.45f, laneContext.SupportEnvelopeMaxZ - laneContext.SupportEnvelopeMinZ + 0.2f)) * supportAnchorExposurePenaltyWeight;
            float stabilityBonus = anchorIndex == selectedSupportAnchorIndex && selectedSupportAnchorLaneIndex == laneContext.LaneIndex
                ? supportAnchorStabilityBonus
                : 0f;
            return lineOfFireScore + coverScore + stabilityBonus - travelPenalty - crowdPenalty - pressurePenalty - forwardExposure;
        }

        private int CountNearbyAllies(int laneIndex, Vector3 anchor, float radius)
        {
            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            float radiusSquared = radius * radius;
            int nearbyCount = 0;
            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null || !summonUnit.IsAlive || !summonUnit.IsPlayerTeam || summonUnit.AssignedLaneIndex != laneIndex)
                {
                    continue;
                }

                Vector3 delta = summonUnit.transform.position - anchor;
                delta.y = 0f;
                if (delta.sqrMagnitude <= radiusSquared)
                {
                    nearbyCount++;
                }
            }

            return nearbyCount;
        }

        private Vector3 ResolvePeekSupportAnchor(BattleManager.LaneCombatState laneState, float worldY, float anchorX)
        {
            Vector3 anchor = laneState.PeekAnchor;
            anchor.x = anchorX;
            anchor.y = worldY;
            anchor.z = Mathf.Min(anchor.z, laneState.MaxForwardZ);
            return ClampToMovementBounds(anchor);
        }

        private void SetSelectedSupportAnchor(int anchorIndex, Vector3 anchor, int laneIndex)
        {
            selectedSupportAnchorIndex = anchorIndex;
            selectedSupportAnchor = anchor;
            selectedSupportAnchorLaneIndex = laneIndex;
            currentSupportAnchorLabel = anchorIndex switch
            {
                0 => "LEFT",
                2 => "RIGHT",
                _ => "CENTER"
            };
        }

        private void SetCurrentSupportAnchorScores(float leftScore, float centerScore, float rightScore, float bestScore)
        {
            currentSupportAnchorScores[0] = leftScore;
            currentSupportAnchorScores[1] = centerScore;
            currentSupportAnchorScores[2] = rightScore;
            currentSupportAnchorScore = bestScore;
        }

        private PlayerCombatController GetCombatController()
        {
            return GetComponent<PlayerCombatController>();
        }

        private static string ResolveMovementReasonLabel(BattleManager.LaneCombatState laneState, BattleManager.PlayerLaneSlot slot, float currentZ)
        {
            return laneState.EscortPhase switch
            {
                BattleManager.EscortPhase.Ready => "READY",
                BattleManager.EscortPhase.Join => "JOIN",
                BattleManager.EscortPhase.BlockerHold => "SUPPORT",
                BattleManager.EscortPhase.Breach => "PEEK",
                BattleManager.EscortPhase.Objective => "OBJECTIVE",
                BattleManager.EscortPhase.Fallback => "FALL BACK",
                _ => "ESCORT"
            };
        }

        private PlayerRetreatReason ResolveCurrentRetreatReason(
            BattleManager battleManager,
            BattleManager.LaneCombatState laneState,
            float currentZ,
            BattleManager.EscortPhase previousEscortPhase)
        {
            bool isLingeringWithoutAllies = !laneState.HasLiveAllies &&
                Time.time < allyLossLingerUntil &&
                laneState.LaneIndex == BattleLaneUtility.ClampLaneIndex(
                    allyLossLingerLaneIndex,
                    battleManager != null ? battleManager.LaneCount : BattleLaneUtility.DefaultLaneCount);
            bool progressedPastJoin = previousEscortPhase is
                BattleManager.EscortPhase.BlockerHold or
                BattleManager.EscortPhase.Breach or
                BattleManager.EscortPhase.Objective;
            bool recentJoinWindow = laneState.EscortPhase == BattleManager.EscortPhase.Join &&
                !progressedPastJoin &&
                Time.time <= lastFriendlySummonTime + Mathf.Max(0.2f, autoSummonFocusHoldDuration) &&
                laneState.LaneIndex == BattleLaneUtility.ClampLaneIndex(
                    lastFriendlySummonLaneIndex,
                    battleManager != null ? battleManager.LaneCount : BattleLaneUtility.DefaultLaneCount);

            if (isLingeringWithoutAllies)
            {
                return PlayerRetreatReason.NoAlliedFrontline;
            }

            if (!laneState.HasLiveAllies &&
                laneState.EscortPhase != BattleManager.EscortPhase.Ready &&
                !recentJoinWindow)
            {
                return PlayerRetreatReason.NoAlliedFrontline;
            }

            if (laneState.PressureState == BattleManager.LanePressureState.Collapse)
            {
                return PlayerRetreatReason.LaneCollapse;
            }

            if (battleManager != null &&
                battleManager.TryGetPlayerTerritoryState(out BattleManager.PlayerTerritoryState territoryState) &&
                territoryState.OverextendDistance > 0.35f)
            {
                return PlayerRetreatReason.Overextended;
            }

            return PlayerRetreatReason.None;
        }

        private bool ApplyResolvedFocusLane(int laneIndex, FocusLaneReason reason)
        {
            BattleManager battleManager = BattleManager.Instance;
            int nextLaneIndex = BattleLaneUtility.ClampLaneIndex(laneIndex, battleManager != null ? battleManager.LaneCount : BattleLaneUtility.DefaultLaneCount);
            bool changed = nextLaneIndex != focusLaneIndex || reason != currentFocusLaneReason;
            focusLaneIndex = nextLaneIndex;
            currentFocusLaneReason = reason;

            if (!isResolvingAutoCombatAnchor)
            {
                RefreshAutoCombatAnchor(forceRefresh: true);
            }

            return changed;
        }

        private void EnterMotionState(PlayerMotionState nextState, float minimumDuration = 0f, bool forceSupportCover = false, bool holdPosition = false)
        {
            currentMotionState = nextState;
            motionStateUntil = Time.time + Mathf.Max(0f, minimumDuration);
            recoveryForcesSupportCover = nextState == PlayerMotionState.Recovering && forceSupportCover;
            recoveryHoldPosition = nextState == PlayerMotionState.Recovering && holdPosition;
            if (nextState == PlayerMotionState.Recovering)
            {
                recoveringFocusLaneIndex = focusLaneIndex;
                recoveringPressureState = currentLanePressureState;
            }
            if (nextState == PlayerMotionState.Recovering && forceSupportCover)
            {
                stableFollowSlot = BattleManager.PlayerLaneSlot.SupportCover;
                slotTransitionCandidate = BattleManager.PlayerLaneSlot.SupportCover;
                slotTransitionCandidateSince = Time.time;
                desiredPlayerLaneSlot = BattleManager.PlayerLaneSlot.SupportCover;
            }
        }

        private BattleManager.PlayerLaneSlot ResolveStableFollowSlot(BattleManager.LaneCombatState laneState)
        {
            BattleManager.PlayerLaneSlot desiredSlot = laneState.SuggestedPlayerSlot;
            desiredPlayerLaneSlot = desiredSlot;

            if (desiredSlot == BattleManager.PlayerLaneSlot.SupportCover)
            {
                if (slotTransitionCandidate != desiredSlot)
                {
                    slotTransitionCandidate = desiredSlot;
                    slotTransitionCandidateSince = Time.time;
                    return stableFollowSlot;
                }

                if (stableFollowSlot != desiredSlot &&
                    Time.time < slotTransitionCandidateSince + Mathf.Max(0.05f, slotStateStabilityDuration * 0.5f))
                {
                    return stableFollowSlot;
                }

                stableFollowSlot = desiredSlot;
                return stableFollowSlot;
            }

            if (stableFollowSlot == desiredSlot)
            {
                slotTransitionCandidate = desiredSlot;
                slotTransitionCandidateSince = Time.time;
                return stableFollowSlot;
            }

            if (slotTransitionCandidate != desiredSlot)
            {
                slotTransitionCandidate = desiredSlot;
                slotTransitionCandidateSince = Time.time;
                return stableFollowSlot;
            }

            if (Time.time < slotTransitionCandidateSince + Mathf.Max(0.05f, slotStateStabilityDuration))
            {
                return stableFollowSlot;
            }

            stableFollowSlot = desiredSlot;
            return stableFollowSlot;
        }

        private static BattleManager.CoverState ResolveCoverStateForSlot(BattleManager.PlayerLaneSlot slot)
        {
            return slot == BattleManager.PlayerLaneSlot.SupportCover || slot == BattleManager.PlayerLaneSlot.Rear
                ? BattleManager.CoverState.SoftCover
                : BattleManager.CoverState.Exposed;
        }

        private void UpdateLeashFlag(float anchorZ)
        {
            currentMovementIsLeashed =
                currentMotionState != PlayerMotionState.Retreating &&
                !float.IsNaN(currentLeashMaxForwardZ) &&
                currentLeashDepthBand < BattleManager.HeroLaneDepthBand.Advance &&
                Mathf.Abs(anchorZ - currentLeashMaxForwardZ) <= 0.12f;
        }

        private static bool IsNearAnchor(Vector3 currentPosition, Vector3 targetPosition)
        {
            Vector3 planarDelta = targetPosition - currentPosition;
            planarDelta.y = 0f;
            return planarDelta.sqrMagnitude <= 0.04f;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || CurrentHP <= 0f || isRespawning || Time.time < respawnInvulnerableUntil)
            {
                return;
            }

            float resolvedDamage = amount;
            if (currentCoverState == BattleManager.CoverState.SoftCover)
            {
                resolvedDamage *= softCoverDamageMultiplier;
            }
            else if (currentCoverState == BattleManager.CoverState.Exposed)
            {
                resolvedDamage *= exposedDamageMultiplier;
            }

            CurrentHP = Mathf.Max(0f, CurrentHP - resolvedDamage);
            OnHPChanged?.Invoke(CurrentHP, maxHP);

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.PlayShake(hitShakeDuration, hitShakeMagnitude);
            }

            BattlePresentationController.Instance?.ShowWorldText(transform.position + new Vector3(0f, 1.9f, 0f), $"-{Mathf.CeilToInt(resolvedDamage)}", new Color(1f, 0.48f, 0.48f, 1f), 3.8f, 0.7f);
            BattlePresentationController.Instance?.SpawnBurst(transform.position + Vector3.up, new Color(1f, 0.58f, 0.52f, 1f), 14, 0.16f, 2.6f, 0.08f, 0.35f);

            if (CurrentHP > 0f)
            {
                return;
            }

            OnDeath?.Invoke();
            BattlePresentationController.Instance?.ShowWorldText(
                transform.position + new Vector3(0f, 2.25f, 0f),
                "KO",
                new Color(1f, 0.54f, 0.46f, 1f),
                4.1f,
                0.9f);
            BeginRespawn();
        }

        private void HandleJustDodge()
        {
            BattleEnergySystem.Instance?.AddEnergy(justDodgeEnergyReward);
            OnJustDodgeRewarded?.Invoke(justDodgeEnergyReward);
            if (justDodgeEffect != null)
            {
                justDodgeEffect.Play();
            }

            BattlePresentationController.Instance?.ShowWorldText(transform.position + new Vector3(0f, 2f, 0f), "JUST!", new Color(0.4f, 0.95f, 1f, 1f), 4.2f, 0.8f);
            BattlePresentationController.Instance?.ShowScreenFlash(new Color(0.4f, 0.95f, 1f, 1f), 0.08f, 0.18f);
        }

        private void HandleFriendlySummonSpawned(SummonData _, Vector3 spawnPosition, bool isPlayerTeam)
        {
            if (!isPlayerTeam || BattleManager.Instance == null)
            {
                return;
            }

            RefreshIgnoredCollisions();
            int suggestedLaneIndex = BattleManager.Instance.GetNearestLaneIndex(spawnPosition.x);
            escortLaneIndex = suggestedLaneIndex;
            preferredLaneIndex = suggestedLaneIndex;
            lastFriendlySummonLaneIndex = suggestedLaneIndex;
            lastFriendlySummonTime = Time.time;
            allyLossLingerLaneIndex = suggestedLaneIndex;
            allyLossLingerUntil = float.NegativeInfinity;
            if (manualTargetLockKind == ManualTargetLockKind.NormalEnemy &&
                TryGetManualTargetLock(out Transform ignoredLockedTarget, out int lockedLaneIndex, out ManualTargetLockKind lockedKind) &&
                lockedKind == ManualTargetLockKind.NormalEnemy &&
                lockedLaneIndex != suggestedLaneIndex)
            {
                ClearManualTargetLockInternal(refreshAnchor: false);
            }

            if (!isResolvingAutoCombatAnchor && currentMotionState == PlayerMotionState.SlotFollow)
            {
                RefreshAutoCombatAnchor(forceRefresh: true);
            }
        }

        private void HandleEnemySummonSpawned(SummonData _, Vector3 __, bool ___)
        {
            RefreshIgnoredCollisions();
        }

        private void UpdateAnimatorParameters()
        {
            if (characterAnimator == null)
            {
                return;
            }

            characterAnimator.SetFloat(SpeedHash, HasMovementInput ? 1f : 0f);
        }

        private void UpdateAnimatorState(bool forceRefresh = false)
        {
            if (characterAnimator == null)
            {
                return;
            }

            bool shouldWalk = HasMovementInput;
            if (!forceRefresh && shouldWalk == isWalkAnimationActive)
            {
                return;
            }

            int targetStateHash = shouldWalk ? WalkStateHash : IdleStateHash;
            if (forceRefresh)
            {
                characterAnimator.Play(targetStateHash, 0, 0f);
            }
            else
            {
                characterAnimator.CrossFade(targetStateHash, 0.05f, 0, 0f);
            }

            isWalkAnimationActive = shouldWalk;
        }

        private void UpdateFacingRotation()
        {
            if (!HasMovementInput)
            {
                return;
            }

            Vector3 lookDirection = new(CurrentMoveInput.x, 0f, CurrentMoveInput.y);
            if (lookDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }

        private static Vector2 ReadMoveInput()
        {
            Vector2 moveInput = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                {
                    moveInput.x -= 1f;
                }

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                {
                    moveInput.x += 1f;
                }

                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                {
                    moveInput.y -= 1f;
                }

                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                {
                    moveInput.y += 1f;
                }
            }

            if (moveInput.sqrMagnitude > 0.001f)
            {
                return Vector2.ClampMagnitude(moveInput, 1f);
            }

            if (Gamepad.current != null)
            {
                Vector2 gamepadInput = Gamepad.current.leftStick.ReadValue();
                if (gamepadInput.sqrMagnitude >= 0.09f)
                {
                    return Vector2.ClampMagnitude(gamepadInput, 1f);
                }
            }

            if (MobileBattleControls.TryGetMoveInput(out Vector2 touchMoveInput))
            {
                return touchMoveInput;
            }

#else
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (moveInput.sqrMagnitude <= 0.0004f)
            {
                if (MobileBattleControls.TryGetMoveInput(out Vector2 touchMoveInput))
                {
                    return touchMoveInput;
                }

                return Vector2.zero;
            }
#endif

            return Vector2.ClampMagnitude(moveInput, 1f);
        }

        private int ReadLaneShiftInputThisFrame()
        {
            return 0;
        }

        private int ResolveCurrentLaneFrontline(ref float frontlineZ)
        {
            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            int alliedUnitCount = 0;
            float laneFrontZ = frontlineZ;
            int laneUnitCount = 0;

            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null || !summonUnit.IsAlive || !summonUnit.IsPlayerTeam)
                {
                    continue;
                }

                alliedUnitCount++;
                if (summonUnit.AssignedLaneIndex != focusLaneIndex)
                {
                    continue;
                }

                laneUnitCount++;
                laneFrontZ = Mathf.Max(laneFrontZ, summonUnit.transform.position.z);
            }

            if (laneUnitCount > 0)
            {
                frontlineZ = laneFrontZ;
            }
            else if (BattleManager.Instance.TryGetFrontlineState(out BattleManager.FrontlineState frontlineState))
            {
                frontlineZ = Mathf.Max(frontlineZ, frontlineState.PlayerFrontZ);
                alliedUnitCount = frontlineState.PlayerUnitCount;
            }

            return alliedUnitCount;
        }

        private ParticleSystem CreateJustDodgeEffect()
        {
            GameObject effectObject = new("JustDodgeEffect");
            effectObject.transform.SetParent(transform, false);
            effectObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.duration = 0.35f;
            main.loop = false;
            main.startLifetime = 0.25f;
            main.startSpeed = 2.5f;
            main.startSize = 0.35f;
            main.startColor = new Color(0.4f, 0.95f, 1f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.9f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.35f, 0.8f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particleSystem;
        }

        private void AlignVisualToGround()
        {
            if (isRespawning)
            {
                return;
            }

            Transform visualRoot = GetVisualRoot();
            if (visualRoot == null)
            {
                return;
            }

            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            float lowestPoint = float.MaxValue;
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] == null)
                {
                    continue;
                }

                lowestPoint = Mathf.Min(lowestPoint, renderers[index].bounds.min.y);
            }

            if (lowestPoint == float.MaxValue)
            {
                return;
            }

            float desiredGroundY = transform.position.y;
            float offset = desiredGroundY - lowestPoint;
            if (Mathf.Abs(offset) <= 0.001f)
            {
                return;
            }

            visualRoot.position += Vector3.up * offset;
        }

        private Transform GetVisualRoot()
        {
            if (characterAnimator != null)
            {
                return characterAnimator.transform;
            }

            return transform.childCount > 0 ? transform.GetChild(0) : null;
        }

        private void EnsureCollisionBody()
        {
            cachedCapsuleCollider = GetComponent<CapsuleCollider>();
            if (cachedCapsuleCollider == null)
            {
                cachedCapsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            cachedCapsuleCollider.isTrigger = false;
            cachedCapsuleCollider.height = 2f;
            cachedCapsuleCollider.radius = 0.38f;
            cachedCapsuleCollider.center = new Vector3(0f, 1f, 0f);
        }

        private void RefreshIgnoredCollisions()
        {
            if (cachedCapsuleCollider == null)
            {
                return;
            }

            SummonUnit[] summonUnits = FindObjectsByType<SummonUnit>(FindObjectsSortMode.None);
            for (int index = 0; index < summonUnits.Length; index++)
            {
                SummonUnit summonUnit = summonUnits[index];
                if (summonUnit == null)
                {
                    continue;
                }

                Collider summonCollider = summonUnit.GetComponent<Collider>();
                if (summonCollider != null)
                {
                    Physics.IgnoreCollision(cachedCapsuleCollider, summonCollider, true);
                }
            }

            BattleStructure[] structures = FindObjectsByType<BattleStructure>(FindObjectsSortMode.None);
            for (int index = 0; index < structures.Length; index++)
            {
                BattleStructure structure = structures[index];
                if (structure == null)
                {
                    continue;
                }

                Collider structureCollider = structure.GetComponent<Collider>();
                if (structureCollider != null)
                {
                    Physics.IgnoreCollision(cachedCapsuleCollider, structureCollider, true);
                }
            }
        }

        private void BeginRespawn()
        {
            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
            }

            isRespawning = true;
            respawnEndsAt = Time.time + Mathf.Max(0.5f, respawnDelay);
            CurrentMoveInput = Vector2.zero;
            IsMovingForward = false;
            SetRespawnPresentation(active: true);
            MoveToRespawnPoint();
            respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            while (GameManager.Instance != null
                && GameManager.Instance.CurrentState == GameState.Battle
                && Time.time < respawnEndsAt)
            {
                yield return null;
            }

            respawnRoutine = null;
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                yield break;
            }

            CurrentHP = maxHP;
            isRespawning = false;
            respawnInvulnerableUntil = Time.time + Mathf.Max(0f, respawnInvulnerabilityDuration);
            MoveToRespawnPoint();
            SetRespawnPresentation(active: false);
            UpdateAnimatorState(forceRefresh: true);
            OnHPChanged?.Invoke(CurrentHP, maxHP);
            BattlePresentationController.Instance?.ShowWorldText(
                transform.position + new Vector3(0f, 2.15f, 0f),
                "RESPAWN",
                new Color(0.62f, 0.95f, 1f, 1f),
                4.2f,
                0.92f);
            BattlePresentationController.Instance?.ShowScreenFlash(new Color(0.62f, 0.95f, 1f, 1f), 0.08f, 0.16f);
        }

        private void MoveToRespawnPoint()
        {
            Transform playerSpawn = BattleManager.Instance != null ? BattleManager.Instance.PlayerSpawn : null;
            if (playerSpawn == null)
            {
                return;
            }

            Vector3 spawnPosition = playerSpawn.position;
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, minX, maxX);
            spawnPosition.z = Mathf.Clamp(spawnPosition.z, minZ, maxZ);
            if (cachedRigidbody != null)
            {
                cachedRigidbody.position = spawnPosition;
            }

            transform.position = spawnPosition;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        private void SetRespawnPresentation(bool active)
        {
            if (cachedCapsuleCollider != null)
            {
                cachedCapsuleCollider.enabled = !active;
            }

            if (justDodgeDetector != null)
            {
                justDodgeDetector.enabled = !active;
            }

            Transform visualRoot = GetVisualRoot();
            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(!active);
            }

            if (active && justDodgeEffect != null)
            {
                justDodgeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
