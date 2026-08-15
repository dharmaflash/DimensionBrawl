using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    /// <summary>
    /// Owns the direct-load City Hero Pocket exit beat. The transition can only be
    /// armed by the configured encounter's real Won event and can only be started
    /// by the configured player CharacterController entering the authored trigger.
    /// It deliberately stops at an opaque cover; route loading belongs to a future
    /// product-owned handoff.
    /// </summary>
    [DefaultExecutionOrder(12000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CityHeroPocketExitTransitionController : MonoBehaviour
    {
        public const int HudFadeFrameCount = 18;
        public const int PortalGrowFrameCount = 42;
        public const int CoverFadeStartFrame = 234;
        public const int ExitReadyFrame = 294;
        public const float InitialPortalScaleFactor = 0.08f;
        public const uint FirstParticleRandomSeed = 240815u;

        [Header("Won-gated Trigger")]
        [SerializeField] private CombatEncounterController encounter;
        [SerializeField] private CharacterController playerController;
        [SerializeField] private BoxCollider exitTrigger;

        [Header("Presentation")]
        [SerializeField] private Transform transitionFocus;
        [SerializeField] private Transform portalRoot;
        [SerializeField] private Vector3 portalAuthoredScale = Vector3.one;
        [SerializeField] private CanvasGroup hudCanvasGroup;
        [SerializeField] private CanvasGroup coverCanvasGroup;

        [Header("Product Control Ownership")]
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private PlayerActionController playerAction;
        [SerializeField] private PlayerCombatModeController playerCombatMode;
        [SerializeField] private PlayerRangedBasicAttackAction playerRangedAttack;
        [SerializeField] private BasicSoldierEnemy enemyAi;
        [SerializeField] private BasicSoldierProjectileAttackDriver enemyProjectileDriver;

        private CombatEncounterController subscribedEncounter;
        private bool initialStateCaptured;
        private float initialHudAlpha;
        private bool initialHudInteractable;
        private bool initialHudBlocksRaycasts;
        private float initialCoverAlpha;
        private bool initialCoverInteractable;
        private bool initialCoverBlocksRaycasts;
        private bool initialEnemyAiEnabled;
        private bool initialEnemyProjectileDriverEnabled;
        private bool isArmed;
        private bool isTransitionRunning;
        private bool isHudHidden;
        private bool isFullCover;
        private bool isExitReady;
        private bool isInputLocked;
        private bool isAiLocked;
        private int presentationFrame;
        private int ignoredLaneActionProjectileTriggerEnterCount;
        private int rejectedTriggerEnterCount;
        private int triggerAcceptedCount;
        private int transitionStartedCount;
        private int hudHiddenCount;
        private int fullCoverCount;
        private int exitReadyCount;

        public CombatEncounterController Encounter => encounter;
        public CharacterController PlayerController => playerController;
        public BoxCollider ExitTrigger => exitTrigger;
        public Transform TransitionFocus => transitionFocus;
        public Transform PortalRoot => portalRoot;
        public Vector3 PortalAuthoredScale => portalAuthoredScale;
        public CanvasGroup HudCanvasGroup => hudCanvasGroup;
        public CanvasGroup CoverCanvasGroup => coverCanvasGroup;
        public PlayerMovementController PlayerMovement => playerMovement;
        public PlayerActionController PlayerAction => playerAction;
        public PlayerCombatModeController PlayerCombatMode => playerCombatMode;
        public PlayerRangedBasicAttackAction PlayerRangedAttack => playerRangedAttack;
        public BasicSoldierEnemy EnemyAi => enemyAi;
        public BasicSoldierProjectileAttackDriver EnemyProjectileDriver =>
            enemyProjectileDriver;

        public bool IsConfigured => encounter != null
            && playerController != null
            && exitTrigger != null
            && transitionFocus != null
            && portalRoot != null
            && hudCanvasGroup != null
            && coverCanvasGroup != null
            && playerMovement != null
            && playerAction != null
            && playerCombatMode != null
            && playerRangedAttack != null
            && enemyAi != null
            && enemyProjectileDriver != null;
        public bool IsArmed => isArmed;
        public bool IsTransitionRunning => isTransitionRunning;
        public bool IsHudHidden => isHudHidden;
        public bool IsFullCover => isFullCover;
        public bool IsExitReady => isExitReady;
        public bool IsInputLocked => isInputLocked;
        public bool IsAiLocked => isAiLocked;
        public int PresentationFrame => presentationFrame;
        public int IgnoredLaneActionProjectileTriggerEnterCount =>
            ignoredLaneActionProjectileTriggerEnterCount;
        public int RejectedTriggerEnterCount => rejectedTriggerEnterCount;
        public int TriggerAcceptedCount => triggerAcceptedCount;
        public int TransitionStartedCount => transitionStartedCount;
        public int HudHiddenCount => hudHiddenCount;
        public int FullCoverCount => fullCoverCount;
        public int ExitReadyCount => exitReadyCount;
        public float HudFadeProgress01 => Mathf.Clamp01(
            presentationFrame / (float)HudFadeFrameCount);
        public float PortalGrowProgress01 => Mathf.Clamp01(
            presentationFrame / (float)PortalGrowFrameCount);
        public float CoverProgress01 => Mathf.Clamp01(
            (presentationFrame - CoverFadeStartFrame)
            / (float)(ExitReadyFrame - CoverFadeStartFrame));
        public float TransitionProgress01 => Mathf.Clamp01(
            presentationFrame / (float)ExitReadyFrame);

        public event Action TriggerAccepted;
        public event Action<Collider, LaneActionProjectile>
            LaneActionProjectileTriggerEnterIgnored;
        public event Action<Collider> TriggerEnterRejected;
        public event Action TransitionStarted;
        public event Action HudHidden;
        public event Action FullCover;
        public event Action ExitReady;

        public void Configure(
            CombatEncounterController newEncounter,
            CharacterController newPlayerController,
            BoxCollider newExitTrigger,
            Transform newTransitionFocus,
            Transform newPortalRoot,
            Vector3 newPortalAuthoredScale,
            CanvasGroup newHudCanvasGroup,
            CanvasGroup newCoverCanvasGroup,
            PlayerMovementController newPlayerMovement,
            PlayerActionController newPlayerAction,
            PlayerCombatModeController newPlayerCombatMode,
            PlayerRangedBasicAttackAction newPlayerRangedAttack,
            BasicSoldierEnemy newEnemyAi,
            BasicSoldierProjectileAttackDriver newEnemyProjectileDriver)
        {
            UnbindEncounter();
            encounter = newEncounter;
            playerController = newPlayerController;
            exitTrigger = newExitTrigger;
            transitionFocus = newTransitionFocus;
            portalRoot = newPortalRoot;
            portalAuthoredScale = newPortalAuthoredScale;
            hudCanvasGroup = newHudCanvasGroup;
            coverCanvasGroup = newCoverCanvasGroup;
            playerMovement = newPlayerMovement;
            playerAction = newPlayerAction;
            playerCombatMode = newPlayerCombatMode;
            playerRangedAttack = newPlayerRangedAttack;
            enemyAi = newEnemyAi;
            enemyProjectileDriver = newEnemyProjectileDriver;

            if (exitTrigger != null)
            {
                exitTrigger.isTrigger = true;
            }

            ApplyDeterministicParticleSeeds();
            ApplyAuthoredInitialVisualState();
            initialStateCaptured = false;
            if (isActiveAndEnabled)
            {
                CaptureInitialState();
                BindEncounter();
            }
        }

        /// <summary>
        /// Restores every state owned by this transition. It intentionally does not
        /// infer a win from Encounter.IsWon; a subsequent run must emit Won again.
        /// </summary>
        public void ResetForRestart()
        {
            CaptureInitialState();
            SetPlayerInputLocked(false);
            SetAiLocked(false);

            isArmed = false;
            isTransitionRunning = false;
            isHudHidden = false;
            isFullCover = false;
            isExitReady = false;
            presentationFrame = 0;
            ignoredLaneActionProjectileTriggerEnterCount = 0;
            rejectedTriggerEnterCount = 0;
            triggerAcceptedCount = 0;
            transitionStartedCount = 0;
            hudHiddenCount = 0;
            fullCoverCount = 0;
            exitReadyCount = 0;

            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = initialHudAlpha;
                hudCanvasGroup.interactable = initialHudInteractable;
                hudCanvasGroup.blocksRaycasts = initialHudBlocksRaycasts;
            }
            if (coverCanvasGroup != null)
            {
                coverCanvasGroup.alpha = initialCoverAlpha;
                coverCanvasGroup.interactable = initialCoverInteractable;
                coverCanvasGroup.blocksRaycasts = initialCoverBlocksRaycasts;
            }

            ResetPortalPresentation();
        }

        private void Awake()
        {
            if (exitTrigger == null)
            {
                exitTrigger = GetComponent<BoxCollider>();
            }
            CaptureInitialState();
            ResetForRestart();
        }

        private void OnEnable()
        {
            BindEncounter();
        }

        private void OnDisable()
        {
            UnbindEncounter();
            RestoreOwnedStateForTeardown();
        }

        private void OnDestroy()
        {
            UnbindEncounter();
            RestoreOwnedStateForTeardown();
        }

        private void OnValidate()
        {
            if (exitTrigger == null)
            {
                exitTrigger = GetComponent<BoxCollider>();
            }
            if (exitTrigger != null)
            {
                exitTrigger.isTrigger = true;
            }
            portalAuthoredScale.x = Mathf.Max(0.0001f, portalAuthoredScale.x);
            portalAuthoredScale.y = Mathf.Max(0.0001f, portalAuthoredScale.y);
            portalAuthoredScale.z = Mathf.Max(0.0001f, portalAuthoredScale.z);
        }

        private void LateUpdate()
        {
            if (!isTransitionRunning || isExitReady)
            {
                return;
            }

            presentationFrame = Mathf.Min(presentationFrame + 1, ExitReadyFrame);
            ApplyPresentationFrame();
        }

        private void OnTriggerEnter(Collider other)
        {
            LaneActionProjectile laneProjectile = other != null
                ? other.GetComponentInParent<LaneActionProjectile>()
                : null;
            if (laneProjectile != null && laneProjectile.IsActive)
            {
                ignoredLaneActionProjectileTriggerEnterCount++;
                LaneActionProjectileTriggerEnterIgnored?.Invoke(
                    other,
                    laneProjectile);
                return;
            }

            if (playerController != null
                && !ReferenceEquals(other, playerController))
            {
                rejectedTriggerEnterCount++;
                TriggerEnterRejected?.Invoke(other);
                return;
            }

            if (playerController == null
                || !isArmed
                || isTransitionRunning
                || isExitReady
                || !ReferenceEquals(other, playerController))
            {
                return;
            }

            triggerAcceptedCount++;
            TriggerAccepted?.Invoke();
            StartTransition();
        }

        private void BindEncounter()
        {
            if (subscribedEncounter == encounter)
            {
                return;
            }

            UnbindEncounter();
            subscribedEncounter = encounter;
            if (subscribedEncounter != null)
            {
                subscribedEncounter.Won += HandleEncounterWon;
            }
        }

        private void UnbindEncounter()
        {
            if (subscribedEncounter != null)
            {
                subscribedEncounter.Won -= HandleEncounterWon;
                subscribedEncounter = null;
            }
        }

        private void HandleEncounterWon()
        {
            if (encounter == null
                || !ReferenceEquals(subscribedEncounter, encounter)
                || !encounter.IsWon
                || isTransitionRunning
                || isExitReady)
            {
                return;
            }

            isArmed = true;
        }

        private void StartTransition()
        {
            if (isTransitionRunning || isExitReady)
            {
                return;
            }

            isTransitionRunning = true;
            presentationFrame = 0;
            transitionStartedCount++;
            SetPlayerInputLocked(true);
            SetAiLocked(true);

            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = initialHudAlpha;
                hudCanvasGroup.interactable = false;
                hudCanvasGroup.blocksRaycasts = false;
            }
            if (coverCanvasGroup != null)
            {
                coverCanvasGroup.alpha = 0f;
                coverCanvasGroup.interactable = false;
                coverCanvasGroup.blocksRaycasts = false;
            }

            StartPortalPresentation();
            TransitionStarted?.Invoke();
        }

        private void ApplyPresentationFrame()
        {
            float hudProgress = HudFadeProgress01;
            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = Mathf.Lerp(initialHudAlpha, 0f, hudProgress);
            }
            if (!isHudHidden && presentationFrame >= HudFadeFrameCount)
            {
                isHudHidden = true;
                hudHiddenCount++;
                HudHidden?.Invoke();
            }

            if (portalRoot != null)
            {
                float easedPortalProgress = 1f
                    - Mathf.Pow(1f - PortalGrowProgress01, 3f);
                portalRoot.localScale = Vector3.LerpUnclamped(
                    portalAuthoredScale * InitialPortalScaleFactor,
                    portalAuthoredScale,
                    easedPortalProgress);
            }

            if (coverCanvasGroup != null)
            {
                coverCanvasGroup.alpha = CoverProgress01;
            }

            if (!isFullCover && presentationFrame >= ExitReadyFrame)
            {
                isFullCover = true;
                fullCoverCount++;
                FullCover?.Invoke();
            }
            if (!isExitReady && presentationFrame >= ExitReadyFrame)
            {
                isExitReady = true;
                isTransitionRunning = false;
                exitReadyCount++;
                ExitReady?.Invoke();
            }
        }

        private void CaptureInitialState()
        {
            if (initialStateCaptured)
            {
                return;
            }

            initialStateCaptured = true;
            if (hudCanvasGroup != null)
            {
                initialHudAlpha = hudCanvasGroup.alpha;
                initialHudInteractable = hudCanvasGroup.interactable;
                initialHudBlocksRaycasts = hudCanvasGroup.blocksRaycasts;
            }
            else
            {
                initialHudAlpha = 1f;
                initialHudInteractable = true;
                initialHudBlocksRaycasts = true;
            }
            if (coverCanvasGroup != null)
            {
                initialCoverAlpha = coverCanvasGroup.alpha;
                initialCoverInteractable = coverCanvasGroup.interactable;
                initialCoverBlocksRaycasts = coverCanvasGroup.blocksRaycasts;
            }
            initialEnemyAiEnabled = enemyAi != null && enemyAi.enabled;
            initialEnemyProjectileDriverEnabled =
                enemyProjectileDriver != null && enemyProjectileDriver.enabled;
        }

        private void ApplyAuthoredInitialVisualState()
        {
            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = 1f;
                hudCanvasGroup.interactable = true;
                hudCanvasGroup.blocksRaycasts = true;
            }
            if (coverCanvasGroup != null)
            {
                coverCanvasGroup.alpha = 0f;
                coverCanvasGroup.interactable = false;
                coverCanvasGroup.blocksRaycasts = false;
            }
            if (portalRoot != null)
            {
                portalRoot.localScale = portalAuthoredScale * InitialPortalScaleFactor;
                portalRoot.gameObject.SetActive(false);
            }
        }

        private void StartPortalPresentation()
        {
            if (portalRoot == null)
            {
                return;
            }

            ApplyDeterministicParticleSeeds();
            portalRoot.localScale = portalAuthoredScale * InitialPortalScaleFactor;
            portalRoot.gameObject.SetActive(true);
            ParticleSystem[] particles = portalRoot.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(
                    withChildren: false,
                    stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Play(withChildren: false);
            }
        }

        private void ResetPortalPresentation()
        {
            if (portalRoot == null)
            {
                return;
            }

            if (Application.isPlaying && portalRoot.gameObject.activeSelf)
            {
                ParticleSystem[] particles =
                    portalRoot.GetComponentsInChildren<ParticleSystem>(true);
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Stop(
                        withChildren: false,
                        ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            portalRoot.localScale = portalAuthoredScale * InitialPortalScaleFactor;
            portalRoot.gameObject.SetActive(false);
        }

        private void ApplyDeterministicParticleSeeds()
        {
            if (portalRoot == null)
            {
                return;
            }

            ParticleSystem[] particles = portalRoot.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].useAutoRandomSeed = false;
                particles[i].randomSeed = FirstParticleRandomSeed + (uint)i;
            }
        }

        private void SetPlayerInputLocked(bool locked)
        {
            playerMovement?.SetCinematicMoveInputLocked(
                PlayerInputLockSource.CityHeroPocketExitTransition,
                locked);
            playerAction?.SetCinematicInputLocked(
                PlayerInputLockSource.CityHeroPocketExitTransition,
                locked);
            playerCombatMode?.SetCinematicInputLocked(
                PlayerInputLockSource.CityHeroPocketExitTransition,
                locked);
            playerRangedAttack?.SetCinematicInputLocked(
                PlayerInputLockSource.CityHeroPocketExitTransition,
                locked);
            isInputLocked = locked;
        }

        private void RestoreOwnedStateForTeardown()
        {
            if (!Application.isPlaying || !initialStateCaptured)
            {
                return;
            }

            // OnDisable is the normal path and OnDestroy is a deliberate second
            // safety net. ResetForRestart is idempotent, so either or both callbacks
            // leave no City-owned lock, disabled AI, hidden HUD, cover or live portal.
            ResetForRestart();
        }

        private void SetAiLocked(bool locked)
        {
            if (enemyAi != null)
            {
                enemyAi.enabled = locked ? false : initialEnemyAiEnabled;
            }
            if (enemyProjectileDriver != null)
            {
                enemyProjectileDriver.enabled = locked
                    ? false
                    : initialEnemyProjectileDriverEnabled;
            }
            isAiLocked = locked;
        }
    }
}
