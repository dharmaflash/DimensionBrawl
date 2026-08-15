using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace DimensionBrawl.LevelDesign
{
    /// <summary>
    /// Owns the one-way, single-health handoff from the authored Station boss into
    /// Akaza phase two. The cinematic is presentation-only: the canonical boss
    /// health and encounter/result owners never change.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OlympusStationAkazaPhase2FlowController : MonoBehaviour
    {
        public enum Phase
        {
            Phase1,
            Transitioning,
            Phase2
        }

        private enum HandoffPresentationStage
        {
            None,
            CoveringTerminalPose,
            RevealingGameplay
        }

        [Header("Threshold")]
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField, Range(0.05f, 0.95f)] private float phaseThreshold01 = 0.5f;

        [Header("Canonical Encounter")]
        [SerializeField] private BossBarrageEncounterController bossBarrageEncounterController;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossBasicFireEmitter bossBasicFireEmitter;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private BossSummonPressureAction bossSummonPressureAction;
        [SerializeField] private EnemySummonPacingDirector enemySummonPacingDirector;
        [SerializeField] private BossPressurePositionController bossPressurePositionController;
        [SerializeField] private BossBarrageVisualCueDriver bossVisualCueDriver;

        [Header("Phase Two Loadout")]
        [SerializeField] private BossBarragePatternProfile[] phaseTwoPatternSequence =
            Array.Empty<BossBarragePatternProfile>();
        [SerializeField] private BossBarragePatternProfile phaseTwoOpeningPattern;
        [SerializeField] private BossBasicFireProfile phaseTwoBasicFireProfile;
        [SerializeField] private BossPressureActionDeckProfile phaseTwoActionDeckProfile;
        [SerializeField] private BossSummonPressureProfile phaseTwoSummonPressureProfile;
        [SerializeField] private BossBarrageProjectile phaseTwoProjectilePrefab;
        [SerializeField] private Transform phaseTwoBasicFireOrigin;
        [SerializeField] private Transform[] phaseTwoBarrageSpawnOrigins = Array.Empty<Transform>();
        [SerializeField, Min(1)] private int phaseTwoWavesPerPattern = 1;
        [SerializeField, Min(0)] private int phaseTwoBarragePrewarmCount = 36;
        [SerializeField, Min(0)] private int phaseTwoBasicPrewarmCount = 12;

        [Header("Phase Two Presentation")]
        [SerializeField] private GameObject phaseOneVisualRoot;
        [SerializeField] private GameObject phaseTwoVisualRoot;
        [SerializeField] private Animator phaseTwoAnimator;
        [SerializeField] private Transform phaseTwoPulseRoot;
        [SerializeField] private Renderer[] phaseTwoPulseRenderers = Array.Empty<Renderer>();
        [SerializeField] private BossBarrageVisualCueDriver.PatternAnimationCue[] phaseTwoPatternCues =
            Array.Empty<BossBarrageVisualCueDriver.PatternAnimationCue>();
        [SerializeField] private BossBarrageVisualCueDriver.PressureActionCue[] phaseTwoPressureActionCues =
            Array.Empty<BossBarrageVisualCueDriver.PressureActionCue>();

        [Header("Transition Timeline")]
        [SerializeField] private GameObject transitionRoot;
        [SerializeField] private PlayableDirector transitionDirector;
        [SerializeField] private Camera eyeOpenCamera;
        [SerializeField] private Camera wingDeployCamera;
        [SerializeField] private CanvasGroup transitionCurtain;
        [SerializeField] private bool wingDeployFirst = true;
        [SerializeField, Min(0f)] private float cinematicCameraSwitchSeconds = 2.45f;
        [SerializeField, Min(0f)] private float curtainFadeInStartSeconds = 2.22f;
        [SerializeField, Min(0f)] private float curtainFadeOutEndSeconds = 2.68f;
        [SerializeField, Min(0.1f)] private float transitionDurationSeconds = 4.7f;
        [SerializeField, Min(0.1f)] private float transitionTimeoutSeconds = 6.25f;
        [SerializeField, Min(0.01f)] private float handoffCoverSeconds = 0.10f;
        [SerializeField, Min(0.01f)] private float handoffRevealSeconds = 0.18f;
        [SerializeField] private bool allowEscapeSkip = true;

        [Header("Gameplay Camera / HUD")]
        [SerializeField] private ActionCameraController gameplayCameraController;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private CanvasGroup combatHudCanvasGroup;

        [Header("Player Input Owners")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private PlayerActionController playerActionController;
        [SerializeField] private PlayerSkill1Action playerSkill1Action;
        [SerializeField] private PlayerSummonSlot1Action playerSummonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction playerSummonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction playerSummonSlot3Action;
        [SerializeField] private PlayerRangedBasicAttackAction playerRangedBasicAttackAction;
        [SerializeField] private PlayerCombatModeController playerCombatModeController;

        private Phase currentPhase;
        private bool phaseTwoApplied;
        private bool transitionGameplayCommitted;
        private bool transitionFaultedOpen;
        private bool transitionCompletionSignaled;
        private bool transitionCompletionSignalPending;
        private bool bossTerminalized;
        private bool pocketFailureTerminalized;
        private bool subscribed;
        private bool subscribedEncounterFailure;
        private bool subscribedPlayerDeath;
        private bool transitionCompletionInProgress;
        private float transitionElapsedSeconds;
        private HandoffPresentationStage handoffPresentationStage;
        private float handoffPresentationElapsedSeconds;
        private Camera pendingHandoffCamera;
        private int transitionStartCount;
        private int transitionCompletionCount;
        private bool presentationLeaseActive;
        private bool inputLeaseActive;
        private bool playerDamageLeaseActive;
        private bool summonPacingLeaseActive;
        private bool savedSummonPacingEnabled;
        private bool savedGameplayCameraEnabled;
        private bool savedEyeOpenCameraEnabled;
        private bool savedWingDeployCameraEnabled;
        private bool savedPhaseOneVisibilityState;
        private bool savedPhaseOneActive;
        private bool savedHudState;
        private float savedHudAlpha;
        private bool savedHudInteractable;
        private bool savedHudBlocksRaycasts;
        private bool transitionAnchorCaptured;
        private Vector3 transitionRootBossLocalPosition;
        private Quaternion transitionRootBossLocalRotation = Quaternion.identity;

        public event Action TransitionStarted;
        public event Action TransitionCompleted;

        public Phase CurrentPhase => currentPhase;
        public int TransitionStartCount => transitionStartCount;
        public int TransitionCompletionCount => transitionCompletionCount;
        public CombatHealth BossHealth => bossHealth;
        public BossBarrageEncounterController EncounterController =>
            bossBarrageEncounterController;
        public BossBarrageEmitter BarrageEmitter => bossBarrageEmitter;
        public BossPressureActionDirector PressureActionDirector =>
            bossPressureActionDirector;
        public BossPressurePositionController PressurePositionController =>
            bossPressurePositionController;
        public CanvasGroup CombatHudCanvasGroup => combatHudCanvasGroup;
        public CombatHealth PlayerHealth => playerHealth;
        public PlayerMovementController PlayerMovement => playerMovement;
        public PlayerActionController PlayerActionController => playerActionController;
        public PlayerRangedBasicAttackAction PlayerRangedBasicAttackAction =>
            playerRangedBasicAttackAction;
        public bool PhaseTwoApplied => phaseTwoApplied;
        public bool TransitionFaultedOpen => transitionFaultedOpen;
        public bool BossTerminalized => bossTerminalized;
        public float PhaseThreshold01 => phaseThreshold01;
        public float TransitionElapsedSeconds => transitionElapsedSeconds;
        public bool PlayerDamageLeaseActive => playerDamageLeaseActive;

        private void Awake()
        {
            ResolveOwnedReferences();
            CaptureTransitionAnchor();
            PrewarmPhaseTwoProjectilePools();
            transitionTimeoutSeconds = Mathf.Max(transitionDurationSeconds + 0.25f, transitionTimeoutSeconds);
            if (transitionDirector != null)
            {
                transitionDirector.playOnAwake = false;
                transitionDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
                transitionDirector.extrapolationMode = DirectorWrapMode.None;
            }

            ApplyAuthoredPhaseVisibility();
        }

        private void OnEnable()
        {
            SubscribeBossHealth();
            SubscribeEncounterFailure();
            SubscribePlayerDeath();
            if (pocketFailureTerminalized || bossTerminalized)
            {
                currentPhase = phaseTwoApplied ? Phase.Phase2 : Phase.Phase1;
                ApplyAuthoredPhaseVisibility();
                return;
            }

            if (transitionFaultedOpen)
            {
                currentPhase = phaseTwoApplied ? Phase.Phase2 : Phase.Phase1;
                ApplyAuthoredPhaseVisibility();
                return;
            }

            if (transitionGameplayCommitted || phaseTwoApplied)
            {
                currentPhase = Phase.Phase2;
                if (transitionCompletionSignalPending)
                {
                    transitionCompletionSignalPending = false;
                    SignalTransitionCompletedOnce();
                }

                return;
            }

            currentPhase = Phase.Phase1;
            if (HasReachedPhaseThreshold())
            {
                BeginTransition();
            }
        }

        private void OnDisable()
        {
            UnsubscribeBossHealth();
            UnsubscribeEncounterFailure();
            UnsubscribePlayerDeath();
            if (currentPhase == Phase.Transitioning
                || handoffPresentationStage != HandoffPresentationStage.None)
            {
                AbortTransitionForDisable();
            }
            else
            {
                ReleaseEnemySummonPacingLease();
                ReleasePlayerDamageLease();
                ReleaseTransitionPresentation();
                SetPlayerInputLocked(false);
            }
        }

        private void Update()
        {
            if (pocketFailureTerminalized || bossTerminalized)
            {
                return;
            }

            // Gameplay is atomically committed while the curtain is opaque. The
            // reveal is presentation-only and must keep ticking after Phase2 is set.
            if (handoffPresentationStage != HandoffPresentationStage.None)
            {
                TickHandoffPresentation();
                return;
            }

            if (currentPhase == Phase.Phase1)
            {
                if (!transitionFaultedOpen && HasReachedPhaseThreshold())
                {
                    BeginTransition();
                }

                return;
            }

            if (currentPhase != Phase.Transitioning)
            {
                return;
            }

            transitionElapsedSeconds += Mathf.Max(0f, Time.unscaledDeltaTime);
            UpdateTransitionPresentation();

            if (allowEscapeSkip
                && Keyboard.current != null
                && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TrySkipTransition();
                return;
            }

            if (IsDirectorComplete() || transitionElapsedSeconds >= transitionTimeoutSeconds)
            {
                CompleteTransition();
            }
        }

        public bool TrySkipTransition()
        {
            if (pocketFailureTerminalized
                || bossTerminalized
                || currentPhase != Phase.Transitioning
                || transitionCompletionInProgress)
            {
                return false;
            }

            CompleteTransition();
            return true;
        }

        private void ResolveOwnedReferences()
        {
            bossHealth ??= GetComponent<CombatHealth>();
            bossBarrageEmitter ??= GetComponent<BossBarrageEmitter>();
            bossBasicFireEmitter ??= GetComponent<BossBasicFireEmitter>();
            bossPressureActionDirector ??= GetComponent<BossPressureActionDirector>();
            bossSummonPressureAction ??= GetComponent<BossSummonPressureAction>();
            enemySummonPacingDirector ??= GetComponent<EnemySummonPacingDirector>();
            bossPressurePositionController ??= GetComponent<BossPressurePositionController>();
            bossVisualCueDriver ??= GetComponent<BossBarrageVisualCueDriver>();

            if (playerHealth == null && playerMovement != null)
            {
                playerHealth = playerMovement.GetComponent<CombatHealth>()
                    ?? playerMovement.GetComponentInParent<CombatHealth>();
            }

            if (phaseTwoAnimator == null && phaseTwoVisualRoot != null)
            {
                phaseTwoAnimator = phaseTwoVisualRoot.GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (gameplayCamera == null && gameplayCameraController != null)
            {
                gameplayCamera = gameplayCameraController.GetComponent<Camera>();
            }
        }

        private void PrewarmPhaseTwoProjectilePools()
        {
            if (phaseTwoProjectilePrefab == null)
            {
                return;
            }

            bossBarrageEmitter?.PrewarmProjectilePrefab(
                phaseTwoProjectilePrefab,
                phaseTwoBarragePrewarmCount);
            bossBasicFireEmitter?.PrewarmProjectilePrefab(
                phaseTwoProjectilePrefab,
                phaseTwoBasicPrewarmCount);
        }

        private void SubscribeBossHealth()
        {
            if (subscribed || bossHealth == null)
            {
                return;
            }

            bossHealth.DamageModifying += HandleBossDamageModifying;
            bossHealth.Damaged += HandleBossDamaged;
            bossHealth.Died += HandleBossDied;
            subscribed = true;
        }

        private void UnsubscribeBossHealth()
        {
            if (!subscribed || bossHealth == null)
            {
                subscribed = false;
                return;
            }

            bossHealth.DamageModifying -= HandleBossDamageModifying;
            bossHealth.Damaged -= HandleBossDamaged;
            bossHealth.Died -= HandleBossDied;
            subscribed = false;
        }

        private void SubscribeEncounterFailure()
        {
            if (subscribedEncounterFailure || bossBarrageEncounterController == null)
            {
                return;
            }

            bossBarrageEncounterController.PocketFailed += HandlePocketFailed;
            subscribedEncounterFailure = true;
        }

        private void UnsubscribeEncounterFailure()
        {
            if (!subscribedEncounterFailure || bossBarrageEncounterController == null)
            {
                subscribedEncounterFailure = false;
                return;
            }

            bossBarrageEncounterController.PocketFailed -= HandlePocketFailed;
            subscribedEncounterFailure = false;
        }

        private void SubscribePlayerDeath()
        {
            if (subscribedPlayerDeath || playerHealth == null)
            {
                return;
            }

            playerHealth.Died += HandlePlayerDied;
            subscribedPlayerDeath = true;
        }

        private void UnsubscribePlayerDeath()
        {
            if (!subscribedPlayerDeath || playerHealth == null)
            {
                subscribedPlayerDeath = false;
                return;
            }

            playerHealth.Died -= HandlePlayerDied;
            subscribedPlayerDeath = false;
        }

        private void HandleBossDamageModifying(DamageModificationContext context)
        {
            if (context == null || bossHealth == null)
            {
                return;
            }

            if (transitionFaultedOpen)
            {
                return;
            }

            if (currentPhase == Phase.Transitioning
                || handoffPresentationStage != HandoffPresentationStage.None
                || transitionCompletionInProgress)
            {
                context.SetAmount(0f);
                return;
            }

            if (currentPhase != Phase.Phase1)
            {
                return;
            }

            float thresholdHealth = ResolveThresholdHealth();
            float availableDamage = Mathf.Max(0f, bossHealth.CurrentHealth - thresholdHealth);
            context.SetAmount(Mathf.Min(context.ModifiedAmount, availableDamage));
        }

        private void HandleBossDamaged(DamageInfo damageInfo)
        {
            if (!transitionFaultedOpen
                && currentPhase == Phase.Phase1
                && HasReachedPhaseThreshold())
            {
                BeginTransition();
            }
        }

        private void HandleBossDied()
        {
            if (bossTerminalized)
            {
                return;
            }

            bossTerminalized = true;
            transitionCompletionSignalPending = false;
            TryInvokeTransitionAction(
                () => transitionDirector?.Stop(),
                "stop the defeated boss transition director");
            TryInvokeTransitionAction(
                () => bossBarrageEncounterController?.SetExternalCombatSuspended(true),
                "suspend the defeated boss encounter");
            TryInvokeTransitionAction(
                () => bossBarrageEmitter?.SetFiringEnabled(false),
                "stop defeated boss barrage fire");
            TryInvokeTransitionAction(
                () => bossBasicFireEmitter?.SetFiringEnabled(false),
                "stop defeated boss basic fire");
            TryInvokeTransitionAction(
                () => bossPressureActionDirector?.SetActionsEnabled(false),
                "stop defeated boss pressure actions");
            TryInvokeTransitionAction(
                () => enemySummonPacingDirector?.SetPacingEnabled(false),
                "stop defeated boss summon pacing");
            TryInvokeTransitionAction(
                () => bossSummonPressureAction?.DismissActivePressureSummons(),
                "dismiss defeated boss pressure summons");
            TryInvokeTransitionAction(
                () => bossPressurePositionController?.SetMovementEnabled(false),
                "stop defeated boss movement");
            ReleaseEnemySummonPacingLease();
            FinishTransitionPresentationAndInput();
        }

        private void HandlePocketFailed()
        {
            if (pocketFailureTerminalized)
            {
                return;
            }

            pocketFailureTerminalized = true;
            transitionCompletionSignalPending = false;
            TryInvokeTransitionAction(
                () => transitionDirector?.Stop(),
                "stop failed-pocket transition director");
            TryInvokeTransitionAction(
                () => bossBarrageEmitter?.SetFiringEnabled(false),
                "stop failed-pocket barrage fire");
            TryInvokeTransitionAction(
                () => bossBasicFireEmitter?.SetFiringEnabled(false),
                "stop failed-pocket basic fire");
            TryInvokeTransitionAction(
                () => bossPressureActionDirector?.SetActionsEnabled(false),
                "stop failed-pocket pressure actions");
            TryInvokeTransitionAction(
                () => enemySummonPacingDirector?.SetPacingEnabled(false),
                "stop failed-pocket summon pacing");
            TryInvokeTransitionAction(
                () => bossSummonPressureAction?.DismissActivePressureSummons(),
                "dismiss failed-pocket pressure summons");
            TryInvokeTransitionAction(
                () => bossPressurePositionController?.SetMovementEnabled(false),
                "stop failed-pocket boss movement");
            ReleaseEnemySummonPacingLease();
            currentPhase = transitionGameplayCommitted || phaseTwoApplied
                ? Phase.Phase2
                : Phase.Phase1;
            FinishTransitionPresentationAndInput();
        }

        private void HandlePlayerDied()
        {
            TryInvokeTransitionAction(
                () => bossBarrageEncounterController?.Tick(0f),
                "publish the player-down pocket failure");
            HandlePocketFailed();
        }

        private bool HasReachedPhaseThreshold()
        {
            return bossHealth != null
                && bossHealth.IsAlive
                && bossHealth.CurrentHealth <= ResolveThresholdHealth() + 0.001f;
        }

        private float ResolveThresholdHealth()
        {
            return bossHealth != null
                ? bossHealth.MaxHealth * Mathf.Clamp(phaseThreshold01, 0.05f, 0.95f)
                : 0f;
        }

        private void BeginTransition()
        {
            if (transitionFaultedOpen
                || bossTerminalized
                || pocketFailureTerminalized
                || currentPhase != Phase.Phase1
                || phaseTwoApplied
                || bossHealth == null
                || !bossHealth.IsAlive)
            {
                return;
            }

            currentPhase = Phase.Transitioning;
            transitionElapsedSeconds = 0f;
            transitionStartCount++;

            try
            {
                bossBarrageEncounterController?.SetExternalCombatSuspended(true);
                bossBarrageEmitter?.SetFiringEnabled(false);
                bossBasicFireEmitter?.SetFiringEnabled(false);
                bossPressureActionDirector?.SetActionsEnabled(false);
                AcquireEnemySummonPacingLease();
                bossSummonPressureAction?.DismissActivePressureSummons();
                bossPressurePositionController?.SetMovementEnabled(false);
                SetPlayerInputLocked(true);
                AcquirePlayerDamageLease();
                AnchorTransitionRootToCurrentBossPose();
                AcquireTransitionPresentation();
                InvokeTransitionEventSafely(TransitionStarted, "started");
                if (!isActiveAndEnabled
                    || currentPhase != Phase.Transitioning
                    || transitionCompletionInProgress
                    || handoffPresentationStage != HandoffPresentationStage.None)
                {
                    return;
                }

                if (transitionDirector == null || transitionDirector.playableAsset == null)
                {
                    CompleteTransition();
                    return;
                }

                transitionDirector.time = 0d;
                transitionDirector.RebuildGraph();
                transitionDirector.Evaluate();
                transitionDirector.Play();
                UpdateTransitionPresentation();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                ForceFailOpenAfterTransitionError();
            }
        }

        private bool IsDirectorComplete()
        {
            if (transitionDirector == null || transitionDirector.playableAsset == null)
            {
                return true;
            }

            double duration = transitionDirector.duration;
            if (duration <= 0d)
            {
                return true;
            }

            // Some custom PlayableAssets keep the director in Playing state at
            // their terminal sample. Time reaching the authored duration is the
            // deterministic completion contract; CommitTerminalCinematicState
            // owns the final Evaluate before the handoff begins.
            return transitionDirector.time >= duration - 0.001d;
        }

        private void CompleteTransition()
        {
            if (pocketFailureTerminalized
                || bossTerminalized
                || currentPhase != Phase.Transitioning
                || transitionCompletionInProgress)
            {
                return;
            }

            transitionCompletionInProgress = true;
            try
            {
                CommitTerminalCinematicState();
                pendingHandoffCamera = ResolveActiveCinematicCamera();
                handoffPresentationElapsedSeconds = 0f;
                handoffPresentationStage = HandoffPresentationStage.CoveringTerminalPose;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                ForceFailOpenAfterTransitionError();
            }
        }

        private void TickHandoffPresentation()
        {
            float deltaTime = Mathf.Max(0f, Time.unscaledDeltaTime);
            handoffPresentationElapsedSeconds += deltaTime;

            if (handoffPresentationStage == HandoffPresentationStage.CoveringTerminalPose)
            {
                float duration = Mathf.Max(0.01f, handoffCoverSeconds);
                SetCurtainAlpha(Mathf.Clamp01(handoffPresentationElapsedSeconds / duration));
                if (handoffPresentationElapsedSeconds + 0.0001f < duration)
                {
                    return;
                }

                try
                {
                    SetCurtainAlpha(1f);
                    SwapToGameplayBehindCurtain();
                    handoffPresentationStage = HandoffPresentationStage.RevealingGameplay;
                    handoffPresentationElapsedSeconds = 0f;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                    ForceFailOpenAfterTransitionError();
                }

                return;
            }

            if (handoffPresentationStage != HandoffPresentationStage.RevealingGameplay)
            {
                return;
            }

            float revealDuration = Mathf.Max(0.01f, handoffRevealSeconds);
            SetCurtainAlpha(1f - Mathf.Clamp01(handoffPresentationElapsedSeconds / revealDuration));
            if (handoffPresentationElapsedSeconds + 0.0001f < revealDuration)
            {
                return;
            }

            try
            {
                FinalizeGameplayHandoff();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                ForceFailOpenAfterTransitionError();
            }
        }

        private void SwapToGameplayBehindCurtain()
        {
            CommitPhaseTwoGameplayState();
            if (gameplayCameraController != null && pendingHandoffCamera != null)
            {
                gameplayCameraController.PrimeFromHandoffCamera(pendingHandoffCamera);
            }

            if (transitionDirector != null)
            {
                transitionDirector.Stop();
            }

            if (eyeOpenCamera != null)
            {
                eyeOpenCamera.enabled = false;
            }

            if (wingDeployCamera != null)
            {
                wingDeployCamera.enabled = false;
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled = savedGameplayCameraEnabled;
            }
        }

        private void FinalizeGameplayHandoff()
        {
            if (!transitionGameplayCommitted)
            {
                CommitPhaseTwoGameplayState();
            }

            ResumeGameplayAfterTransition();
            FinishTransitionPresentationAndInput();
            SignalTransitionCompletedOnce();
        }

        private void CommitPhaseTwoGameplayState()
        {
            if (transitionGameplayCommitted)
            {
                return;
            }

            if (pocketFailureTerminalized || bossTerminalized)
            {
                throw new InvalidOperationException(
                    $"{name} cannot commit Phase 2 after terminal combat cleanup.");
            }

            ApplyPhaseTwoLoadout();
            if (bossBarrageEncounterController != null
                && !bossBarrageEncounterController.BeginPhaseTwoAtSummonBlock())
            {
                throw new InvalidOperationException(
                    $"{name} could not re-arm the canonical Phase 2 summon-block cycle.");
            }

            if (bossBarrageEmitter != null && phaseTwoOpeningPattern != null)
            {
                bool queued = bossBarrageEmitter.QueuePriorityPatternForNextFiringWindow(
                    phaseTwoOpeningPattern,
                    1);
                if (!queued)
                {
                    throw new InvalidOperationException(
                        $"{name} could not reserve the authored Phase 2 opening pattern "
                        + $"{phaseTwoOpeningPattern.name}.");
                }
            }

            currentPhase = Phase.Phase2;
            transitionGameplayCommitted = true;
        }

        private void ResumeGameplayAfterTransition()
        {
            TryInvokeTransitionAction(
                () => bossBarrageEncounterController?.SetExternalCombatSuspended(false),
                "release encounter suspension");
            TryInvokeTransitionAction(
                () => bossPressurePositionController?.SetMovementEnabled(true),
                "release boss movement suspension");
            if (bossBarrageEncounterController == null)
            {
                TryInvokeTransitionAction(
                    () => bossBarrageEmitter?.SetFiringEnabled(true),
                    "resume barrage emitter");
                TryInvokeTransitionAction(
                    () => bossBasicFireEmitter?.SetFiringEnabled(true),
                    "resume basic-fire emitter");
                TryInvokeTransitionAction(
                    () => bossPressureActionDirector?.SetActionsEnabled(true),
                    "resume pressure actions");
            }

            ReleaseEnemySummonPacingLease();
        }

        private void FinishTransitionPresentationAndInput()
        {
            handoffPresentationStage = HandoffPresentationStage.None;
            handoffPresentationElapsedSeconds = 0f;
            pendingHandoffCamera = null;
            transitionCompletionInProgress = false;
            TryInvokeTransitionAction(
                ReleaseTransitionPresentation,
                "release transition presentation");
            TryInvokeTransitionAction(
                () => SetPlayerInputLocked(false),
                "release transition input lease");
            ReleasePlayerDamageLease();
        }

        private void ForceFailOpenAfterTransitionError()
        {
            if (!TryCommitPhaseTwoGameplayState())
            {
                transitionFaultedOpen = true;
                currentPhase = phaseTwoApplied ? Phase.Phase2 : Phase.Phase1;
                ResumeDegradedGameplayAfterTransition();
                FinishTransitionPresentationAndInput();
                ApplyAuthoredPhaseVisibility();
                Debug.LogError(
                    $"{name} could not commit Phase 2. Combat was restored without "
                    + "publishing a false transition-completed signal.",
                    this);
                return;
            }

            try
            {
                ResumeGameplayAfterTransition();
            }
            finally
            {
                FinishTransitionPresentationAndInput();
                SignalTransitionCompletedOnce();
            }
        }

        private bool TryCommitPhaseTwoGameplayState()
        {
            if (transitionGameplayCommitted)
            {
                return true;
            }

            try
            {
                CommitPhaseTwoGameplayState();
                return transitionGameplayCommitted;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{name} could not commit the fail-open Phase 2 state: {exception}",
                    this);
                return false;
            }
        }

        private void ResumeDegradedGameplayAfterTransition()
        {
            TryInvokeTransitionAction(
                () => bossBarrageEncounterController?.SetExternalCombatSuspended(false),
                "release the degraded encounter suspension");
            TryInvokeTransitionAction(
                () => bossBarrageEmitter?.SetFiringEnabled(true),
                "resume degraded barrage fire");
            TryInvokeTransitionAction(
                () => bossBasicFireEmitter?.SetFiringEnabled(true),
                "resume degraded basic fire");
            TryInvokeTransitionAction(
                () => bossPressureActionDirector?.SetActionsEnabled(true),
                "resume degraded pressure actions");
            TryInvokeTransitionAction(
                () => bossPressurePositionController?.SetMovementEnabled(true),
                "resume degraded boss movement");
            ReleaseEnemySummonPacingLease();
        }

        private void AbortTransitionForDisable()
        {
            TryInvokeTransitionAction(
                () => transitionDirector?.Stop(),
                "stop disabled transition director");
            ResumeGameplayAfterTransition();
            FinishTransitionPresentationAndInput();
            if (transitionGameplayCommitted)
            {
                currentPhase = Phase.Phase2;
                transitionCompletionSignalPending = !transitionCompletionSignaled;
            }
            else if (transitionFaultedOpen)
            {
                currentPhase = phaseTwoApplied ? Phase.Phase2 : Phase.Phase1;
                transitionCompletionSignalPending = false;
            }
            else
            {
                currentPhase = Phase.Phase1;
            }
        }

        private void CommitTerminalCinematicState()
        {
            if (transitionDirector == null || transitionDirector.playableAsset == null)
            {
                return;
            }

            double terminalTime = Math.Max(0d, transitionDirector.duration);
            transitionDirector.time = terminalTime;
            transitionDirector.Evaluate();
            transitionElapsedSeconds = Mathf.Max(transitionElapsedSeconds, (float)terminalTime);
            UpdateTransitionPresentation();
        }

        private void ApplyPhaseTwoLoadout()
        {
            if (phaseTwoApplied)
            {
                return;
            }

            if (phaseOneVisualRoot != null)
            {
                phaseOneVisualRoot.SetActive(false);
            }

            if (phaseTwoVisualRoot != null)
            {
                phaseTwoVisualRoot.SetActive(true);
            }

            if (phaseTwoAnimator == null && phaseTwoVisualRoot != null)
            {
                phaseTwoAnimator = phaseTwoVisualRoot.GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (phaseTwoAnimator != null
                && phaseTwoAnimator.isActiveAndEnabled
                && phaseTwoAnimator.runtimeAnimatorController != null)
            {
                phaseTwoAnimator.Rebind();
                phaseTwoAnimator.Play("Hover", 0, 0f);
                phaseTwoAnimator.Update(0f);
            }

            BossBarragePatternProfile firstPattern = phaseTwoOpeningPattern;
            if (firstPattern == null && phaseTwoPatternSequence != null && phaseTwoPatternSequence.Length > 0)
            {
                firstPattern = phaseTwoPatternSequence[0];
            }

            if (bossBarrageEmitter != null)
            {
                bossBarrageEmitter.ConfigureSpawnOrigins(phaseTwoBarrageSpawnOrigins);
                bossBarrageEmitter.ConfigurePattern(
                    firstPattern,
                    phaseTwoProjectilePrefab,
                    phaseTwoBarragePrewarmCount);
                bossBarrageEmitter.ConfigurePatternSequence(
                    phaseTwoPatternSequence,
                    phaseTwoWavesPerPattern);
            }

            bossBasicFireEmitter?.ConfigureProfile(
                phaseTwoBasicFireProfile,
                phaseTwoProjectilePrefab,
                phaseTwoBasicPrewarmCount);
            bossBasicFireEmitter?.ConfigureFireOrigin(phaseTwoBasicFireOrigin);
            bossPressureActionDirector?.ConfigureActionDeck(phaseTwoActionDeckProfile);
            bossSummonPressureAction?.ConfigurePressureProfile(phaseTwoSummonPressureProfile);
            bossPressurePositionController?.ConfigureMovementAnimator(phaseTwoAnimator);

            if (bossVisualCueDriver != null)
            {
                Transform pulseRoot = phaseTwoPulseRoot != null
                    ? phaseTwoPulseRoot
                    : bossVisualCueDriver.PulseRoot;
                Renderer[] pulseRenderers = phaseTwoPulseRenderers != null
                    ? phaseTwoPulseRenderers
                    : Array.Empty<Renderer>();
                bossVisualCueDriver.ConfigurePresentation(
                    bossBarrageEmitter,
                    phaseTwoAnimator,
                    pulseRoot,
                    pulseRenderers);
                bossVisualCueDriver.ConfigurePressureActionSource(bossPressureActionDirector);
                bossVisualCueDriver.ConfigureAnimationCues(
                    phaseTwoPatternCues,
                    phaseTwoPressureActionCues);
            }

            phaseTwoApplied = true;
        }

        private void ApplyAuthoredPhaseVisibility()
        {
            if (phaseOneVisualRoot != null)
            {
                phaseOneVisualRoot.SetActive(!phaseTwoApplied);
            }

            if (phaseTwoVisualRoot != null)
            {
                phaseTwoVisualRoot.SetActive(phaseTwoApplied);
            }

            if (transitionRoot != null)
            {
                transitionRoot.SetActive(false);
            }
        }

        private void AcquireTransitionPresentation()
        {
            if (presentationLeaseActive)
            {
                return;
            }

            presentationLeaseActive = true;
            savedGameplayCameraEnabled = gameplayCamera != null && gameplayCamera.enabled;
            savedEyeOpenCameraEnabled = eyeOpenCamera != null && eyeOpenCamera.enabled;
            savedWingDeployCameraEnabled = wingDeployCamera != null && wingDeployCamera.enabled;
            savedPhaseOneVisibilityState = phaseOneVisualRoot != null;
            savedPhaseOneActive = phaseOneVisualRoot != null && phaseOneVisualRoot.activeSelf;
            if (phaseOneVisualRoot != null)
            {
                phaseOneVisualRoot.SetActive(false);
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled = false;
            }

            if (transitionRoot != null)
            {
                transitionRoot.SetActive(true);
            }

            SetCinematicCameraState(showWingDeploy: wingDeployFirst);
            SetCurtainAlpha(0f);

            if (combatHudCanvasGroup != null)
            {
                savedHudState = true;
                savedHudAlpha = combatHudCanvasGroup.alpha;
                savedHudInteractable = combatHudCanvasGroup.interactable;
                savedHudBlocksRaycasts = combatHudCanvasGroup.blocksRaycasts;
                combatHudCanvasGroup.alpha = 0f;
                combatHudCanvasGroup.interactable = false;
                combatHudCanvasGroup.blocksRaycasts = false;
            }
        }

        private void CaptureTransitionAnchor()
        {
            if (transitionRoot == null)
            {
                transitionAnchorCaptured = false;
                return;
            }

            transitionRootBossLocalPosition = transform.InverseTransformPoint(
                transitionRoot.transform.position);
            transitionRootBossLocalRotation = Quaternion.Inverse(transform.rotation)
                * transitionRoot.transform.rotation;
            transitionAnchorCaptured = true;
        }

        private void AnchorTransitionRootToCurrentBossPose()
        {
            if (transitionRoot == null)
            {
                return;
            }

            if (!transitionAnchorCaptured)
            {
                CaptureTransitionAnchor();
            }

            transitionRoot.transform.SetPositionAndRotation(
                transform.TransformPoint(transitionRootBossLocalPosition),
                transform.rotation * transitionRootBossLocalRotation);
            Physics.SyncTransforms();
        }

        private void ReleaseTransitionPresentation()
        {
            if (!presentationLeaseActive)
            {
                return;
            }

            try
            {
                TryInvokeTransitionAction(
                    () => transitionDirector?.Stop(),
                    "stop transition presentation director");
                TryInvokeTransitionAction(
                    () => SetCurtainAlpha(0f),
                    "clear transition curtain");
                TryInvokeTransitionAction(
                    () =>
                    {
                        if (eyeOpenCamera != null)
                        {
                            eyeOpenCamera.enabled = savedEyeOpenCameraEnabled;
                        }
                    },
                    "restore eye-open camera state");
                TryInvokeTransitionAction(
                    () =>
                    {
                        if (wingDeployCamera != null)
                        {
                            wingDeployCamera.enabled = savedWingDeployCameraEnabled;
                        }
                    },
                    "restore wing-deploy camera state");
                TryInvokeTransitionAction(
                    () => transitionRoot?.SetActive(false),
                    "hide transition root");
                TryInvokeTransitionAction(
                    () =>
                    {
                        if (savedPhaseOneVisibilityState && phaseOneVisualRoot != null)
                        {
                            phaseOneVisualRoot.SetActive(
                                transitionGameplayCommitted ? false : savedPhaseOneActive);
                        }
                    },
                    "restore phase-one presentation state");
                TryInvokeTransitionAction(
                    () =>
                    {
                        if (gameplayCamera != null)
                        {
                            gameplayCamera.enabled = savedGameplayCameraEnabled;
                        }
                    },
                    "restore gameplay camera state");
                TryInvokeTransitionAction(
                    () =>
                    {
                        if (savedHudState && combatHudCanvasGroup != null)
                        {
                            combatHudCanvasGroup.alpha = savedHudAlpha;
                            combatHudCanvasGroup.interactable = savedHudInteractable;
                            combatHudCanvasGroup.blocksRaycasts = savedHudBlocksRaycasts;
                        }
                    },
                    "restore combat HUD state");
            }
            finally
            {
                savedPhaseOneVisibilityState = false;
                savedHudState = false;
                presentationLeaseActive = false;
            }
        }

        private void UpdateTransitionPresentation()
        {
            if (!presentationLeaseActive)
            {
                return;
            }

            float time = transitionDirector != null
                ? (float)transitionDirector.time
                : transitionElapsedSeconds;
            bool secondShot = time >= cinematicCameraSwitchSeconds;
            SetCinematicCameraState(showWingDeploy: wingDeployFirst ? !secondShot : secondShot);
            SetCurtainAlpha(EvaluateCurtainAlpha(time));
        }

        private float EvaluateCurtainAlpha(float time)
        {
            float fadeInStart = Mathf.Min(curtainFadeInStartSeconds, cinematicCameraSwitchSeconds);
            float fadeOutEnd = Mathf.Max(curtainFadeOutEndSeconds, cinematicCameraSwitchSeconds);
            if (time <= fadeInStart || time >= fadeOutEnd)
            {
                return 0f;
            }

            if (time < cinematicCameraSwitchSeconds)
            {
                return Mathf.InverseLerp(fadeInStart, cinematicCameraSwitchSeconds, time);
            }

            return 1f - Mathf.InverseLerp(cinematicCameraSwitchSeconds, fadeOutEnd, time);
        }

        private void SetCurtainAlpha(float alpha)
        {
            if (transitionCurtain == null)
            {
                return;
            }

            transitionCurtain.alpha = Mathf.Clamp01(alpha);
            transitionCurtain.interactable = false;
            transitionCurtain.blocksRaycasts = false;
        }

        private void SetCinematicCameraState(bool showWingDeploy)
        {
            if (eyeOpenCamera != null)
            {
                eyeOpenCamera.enabled = !showWingDeploy;
            }

            if (wingDeployCamera != null)
            {
                wingDeployCamera.enabled = showWingDeploy;
            }
        }

        private Camera ResolveActiveCinematicCamera()
        {
            if (wingDeployCamera != null && wingDeployCamera.enabled)
            {
                return wingDeployCamera;
            }

            return eyeOpenCamera != null && eyeOpenCamera.enabled ? eyeOpenCamera : null;
        }

        private void SetPlayerInputLocked(bool locked)
        {
            if (inputLeaseActive == locked)
            {
                return;
            }

            const PlayerInputLockSource source = PlayerInputLockSource.BossPhaseTransition;
            TryInvokeTransitionAction(
                () => playerMovement?.SetCinematicMoveInputLocked(source, locked),
                $"set movement input lock={locked}");
            TryInvokeTransitionAction(
                () => playerActionController?.SetCinematicInputLocked(source, locked),
                $"set action input lock={locked}");
            TryInvokeTransitionAction(
                () => playerSkill1Action?.SetCinematicInputLocked(source, locked),
                $"set Skill1 input lock={locked}");
            TryInvokeTransitionAction(
                () => playerSummonSlot1Action?.SetCinematicInputLocked(source, locked),
                $"set Summon1 input lock={locked}");
            TryInvokeTransitionAction(
                () => playerSummonSlot2Action?.SetCinematicInputLocked(source, locked),
                $"set Summon2 input lock={locked}");
            TryInvokeTransitionAction(
                () => playerSummonSlot3Action?.SetCinematicInputLocked(source, locked),
                $"set Summon3 input lock={locked}");
            TryInvokeTransitionAction(
                () => playerRangedBasicAttackAction?.SetCinematicInputLocked(source, locked),
                $"set ranged input lock={locked}");
            TryInvokeTransitionAction(
                () => playerCombatModeController?.SetCinematicInputLocked(source, locked),
                $"set combat-mode input lock={locked}");
            inputLeaseActive = locked;
        }

        private void AcquirePlayerDamageLease()
        {
            if (playerDamageLeaseActive || playerHealth == null)
            {
                return;
            }

            playerHealth.DamageModifying += BlockPlayerDamageDuringTransition;
            playerDamageLeaseActive = true;
        }

        private void ReleasePlayerDamageLease()
        {
            if (!playerDamageLeaseActive)
            {
                return;
            }

            if (playerHealth != null)
            {
                playerHealth.DamageModifying -= BlockPlayerDamageDuringTransition;
            }

            playerDamageLeaseActive = false;
        }

        private void BlockPlayerDamageDuringTransition(DamageModificationContext context)
        {
            context?.SetAmount(0f);
        }

        private void AcquireEnemySummonPacingLease()
        {
            if (summonPacingLeaseActive || enemySummonPacingDirector == null)
            {
                return;
            }

            summonPacingLeaseActive = true;
            savedSummonPacingEnabled = enemySummonPacingDirector.PacingEnabled;
            enemySummonPacingDirector.SetPacingEnabled(false);
        }

        private void ReleaseEnemySummonPacingLease()
        {
            if (!summonPacingLeaseActive)
            {
                return;
            }

            try
            {
                if (enemySummonPacingDirector != null)
                {
                    enemySummonPacingDirector.SetPacingEnabled(
                        bossTerminalized || pocketFailureTerminalized
                            ? false
                            : savedSummonPacingEnabled);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{name} failed to restore enemy summon pacing: {exception.Message}",
                    this);
            }
            finally
            {
                savedSummonPacingEnabled = false;
                summonPacingLeaseActive = false;
            }
        }

        private void SignalTransitionCompletedOnce()
        {
            if (transitionCompletionSignaled || pocketFailureTerminalized || bossTerminalized)
            {
                return;
            }

            transitionCompletionSignaled = true;
            transitionCompletionCount++;
            InvokeTransitionEventSafely(TransitionCompleted, "completed");
        }

        private void InvokeTransitionEventSafely(Action transitionEvent, string eventLabel)
        {
            if (transitionEvent == null)
            {
                return;
            }

            foreach (Delegate subscriber in transitionEvent.GetInvocationList())
            {
                if (subscriber is not Action action)
                {
                    continue;
                }

                TryInvokeTransitionAction(action, $"invoke transition-{eventLabel} subscriber");
            }
        }

        private void TryInvokeTransitionAction(Action action, string actionLabel)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{name} could not {actionLabel}: {exception}",
                    this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            phaseThreshold01 = Mathf.Clamp(phaseThreshold01, 0.05f, 0.95f);
            phaseTwoWavesPerPattern = Mathf.Max(1, phaseTwoWavesPerPattern);
            phaseTwoBarragePrewarmCount = Mathf.Max(0, phaseTwoBarragePrewarmCount);
            phaseTwoBasicPrewarmCount = Mathf.Max(0, phaseTwoBasicPrewarmCount);
            transitionDurationSeconds = Mathf.Max(0.1f, transitionDurationSeconds);
            transitionTimeoutSeconds = Mathf.Max(transitionDurationSeconds + 0.25f, transitionTimeoutSeconds);
            handoffCoverSeconds = Mathf.Max(0.01f, handoffCoverSeconds);
            handoffRevealSeconds = Mathf.Max(0.01f, handoffRevealSeconds);
            cinematicCameraSwitchSeconds = Mathf.Clamp(
                cinematicCameraSwitchSeconds,
                0f,
                transitionDurationSeconds);
            curtainFadeInStartSeconds = Mathf.Clamp(
                curtainFadeInStartSeconds,
                0f,
                cinematicCameraSwitchSeconds);
            curtainFadeOutEndSeconds = Mathf.Clamp(
                curtainFadeOutEndSeconds,
                cinematicCameraSwitchSeconds,
                transitionDurationSeconds);
        }
#endif
    }
}
