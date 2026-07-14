using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PlayerRangedBasicAttackAction : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private bool fireContinuouslyWhileHeld = true;
        [SerializeField] private bool holdFireActivatesAim = true;
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;
        [SerializeField] private bool allowMouseFireFallback;
        [SerializeField] private bool allowDesktopMouseFireFallbackWhenActionMissing = true;
        [SerializeField] private bool blockMouseFireFallbackOverUi = true;
        [SerializeField] private Key keyboardTestKey = Key.F;

        [Header("References")]
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedAimController aimController;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private PlayerLockTargetController lockTargetController;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Animator animator;

        [Header("Projectile")]
        [SerializeField] private LaneActionProjectile projectilePrefab;
        [SerializeField] private GameObject projectilePrefabObject;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private Transform fireOrigin;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.Player;
        [SerializeField, Min(0f)] private float damage = 30f;
        [SerializeField, Min(0.01f)] private float projectileSpeed = 24f;
        [SerializeField, Min(0.01f)] private float projectileLifetimeSeconds = 1.75f;
        [SerializeField, Min(0.01f)] private float projectileRadius = 0.31f;
        [SerializeField, Min(0f)] private int prewarmCount = 16;

        [Header("Fire Feel")]
        [SerializeField, Min(0.01f)] private float fireIntervalSeconds = 0.12f;
        [SerializeField, Min(0f)] private float spawnForwardOffset = 0.85f;
        [SerializeField, Min(0f)] private float spawnHeight = 1.12f;
        [SerializeField, Min(0f)] private float targetHeight = 1.0f;
        [SerializeField] private bool stabilizeDirectTargetAimHeight = true;
        [SerializeField, Min(0f)] private float directTargetAimHeightTolerance = 0.35f;
        [SerializeField, Min(0f)] private float facingHoldSeconds = 0.16f;
        [SerializeField] private bool requestFacingOnFire;
        [SerializeField] private bool snapFacingOnFire = true;
        [SerializeField] private bool suppressFacingOnFireWhileMoving = true;
        [SerializeField, Min(0f)] private float movingFacingSuppressSpeed = 0.08f;
        [SerializeField] private bool requireAimToFire;
        [SerializeField] private string fireTrigger;

        [Header("Reload")]
        [SerializeField] private bool useMagazineReload = true;
        [SerializeField, Min(1)] private int magazineSize = 24;
        [SerializeField, Min(0.01f)] private float reloadSeconds = 2f;
        [SerializeField] private bool autoReloadWhenEmpty = true;
        [SerializeField] private bool reloadWhenAimReleased = true;
        [SerializeField] private bool cancelAimReleaseReloadOnAimResume = true;

        [Header("Manual Aim")]
        [SerializeField, Range(0f, 1f)] private float aimInputDeadZone = 0.18f;
        [SerializeField, Range(0f, 85f)] private float aimInputYawDegrees = 34f;
        [SerializeField] private bool aimFromCameraViewport = true;
        [SerializeField] private bool useFixedCenterAimViewport = true;
        [SerializeField] private bool preserveVerticalAim = true;
        [SerializeField, Min(1f)] private float cameraAimFallbackDistance = 32f;
        [SerializeField, Min(1f)] private float cameraAimRaycastDistance = 96f;
        [SerializeField] private bool cameraAimIgnoresNonTargetHits;
        [SerializeField, Range(0f, 0.49f)] private float aimInputViewportOffsetX = 0.39f;
        [SerializeField, Range(0f, 0.49f)] private float aimInputViewportOffsetY = 0.20f;
        [SerializeField] private bool useStableAimOrigin = true;
        [SerializeField] private bool useAimAssist = true;
        [SerializeField] private bool disableAimAssistWithManualInput;
        [SerializeField, Min(0f)] private float aimAssistDistance = 30f;
        [SerializeField, Range(0f, 45f)] private float hipAimAssistAngleDegrees = 14f;
        [SerializeField, Range(0f, 45f)] private float aimedAimAssistAngleDegrees = 14f;
        [SerializeField, Range(0f, 45f)] private float aimAssistMaxTurnDegrees = 14f;

        [Header("Camera Aim Assist")]
        [SerializeField] private bool driveCameraAimAssist = true;
        [SerializeField, Range(0f, 1f)] private float cameraAimAssistStrengthScale = 1f;
        [SerializeField, Range(0f, 1f)] private float cameraAimAssistMinStrength = 0.05f;

        private readonly PlayerRangedProjectilePool projectilePool = new PlayerRangedProjectilePool();
        private readonly RaycastHit[] cameraAimHits = new RaycastHit[16];
        private readonly List<Collider> stableAimColliders = new List<Collider>(8);
        private CombatHealth stableAimColliderTarget;
        private Collider stableAimRootCollider;
        private bool actionEnabledHere;
        private bool queuedFire;
        private bool mobileFireHeld;
        private bool currentFireHeld;
        private bool externalAimPreviewHeld;
        private bool pendingFireThisFrame;
        private bool suppressDeviceFallbackThisFrame;
        private PlayerInputLockSource cinematicInputLockSources;
        private PlayerInputLockSource heldAimPreservationLockSources;
        private bool preserveHeldAimWhileCinematicLocked;
        private float nextFireTime;
        private float blockedHintUntil;
        private bool ammoInitialized;
        private int currentAmmo;
        private bool isReloading;
        private bool reloadStartedByAimRelease;
        private float reloadFinishTime;
        private float lastCameraFireCueTime = float.NegativeInfinity;
        private PlayerRangedAimController subscribedAimController;
        private Vector2 aimInput;
        private int firePreviewFrame = -1;
        private bool hasCachedFirePreview;
        private Vector3 cachedFirePreviewDirection = Vector3.forward;
        private Vector3 cachedFirePreviewSpawnPosition;
        private Vector3 cachedFirePreviewAimPoint;
        private bool hasAimAssistPreviewPoint;
        private Vector3 aimAssistPreviewPoint;
        private bool aimAssistMayDriveCamera;
        private bool aimAssistSuppressesViewportReprojection;

        public float FireCooldownRemaining => Mathf.Max(0f, nextFireTime - Time.time);
        public bool UsesMagazineReload => useMagazineReload;
        public int MagazineSize => Mathf.Max(1, magazineSize);
        public int CurrentAmmo
        {
            get
            {
                EnsureAmmoInitialized();
                return useMagazineReload ? currentAmmo : MagazineSize;
            }
        }

        public bool HasAmmo
        {
            get
            {
                EnsureAmmoInitialized();
                return !useMagazineReload || currentAmmo > 0;
            }
        }

        public bool IsReloading => isReloading;
        public float ReloadRemaining => isReloading ? Mathf.Max(0f, reloadFinishTime - Time.time) : 0f;
        public float ReloadProgress01 => isReloading
            ? 1f - Mathf.Clamp01(ReloadRemaining / Mathf.Max(0.01f, reloadSeconds))
            : 1f;
        public bool IsFireReady
        {
            get
            {
                EnsureAmmoInitialized();
                return FireCooldownRemaining <= 0f
                    && !isReloading
                    && (!useMagazineReload || currentAmmo > 0);
            }
        }

        public bool IsFireHeld => currentFireHeld;
        public bool HasExternalFireHeldInput => mobileFireHeld;
        public bool IsCinematicInputLocked => cinematicInputLockSources != PlayerInputLockSource.None;
        public bool IsAimPreviewActive => IsRangedModeActive()
            && (currentFireHeld
                || externalAimPreviewHeld
                || (aimController != null && aimController.IsAiming));
        public Vector2 AimInput => aimInput;
        public Transform FireOrigin => fireOrigin;
        public Transform ProjectileRoot => projectileRoot;
        public Vector3 LastResolvedFireDirection { get; private set; } = Vector3.forward;
        public bool HasLockTarget => lockTargetController != null && lockTargetController.HasLockTarget;
        public CombatHealth LockTargetHealth => lockTargetController != null ? lockTargetController.CurrentTargetHealth : null;
        public bool HasAimAssistTarget { get; private set; }
        public float AimAssistStrength01 { get; private set; }
        public CombatHealth AimAssistTargetHealth { get; private set; }
        public Vector3 LastRawAimDirection { get; private set; } = Vector3.forward;
        public Vector3 LastAimAssistDirection { get; private set; } = Vector3.forward;
        public string LastUseBlockedReason { get; private set; } = string.Empty;
        public bool ShowUseBlockedHint => Time.time < blockedHintUntil && !string.IsNullOrWhiteSpace(LastUseBlockedReason);
        public int ActiveProjectileCount
        {
            get
            {
                return projectilePool.ActiveCount;
            }
        }

        public event Action RangedFireStarted;
        public event Action RangedFireInputStarted;
        public event Action RangedReloadStarted;
        public event Action RangedReloadCompleted;
        public event Action RangedReloadCanceled;
        public event Action<LaneActionProjectile> RangedProjectileFired;
        public event Action AimPreviewStateChanged;

        public void QueueFire()
        {
            if (!CanAcceptQueuedFireInput())
            {
                queuedFire = false;
                return;
            }

            queuedFire = true;
            RangedFireInputStarted?.Invoke();
        }

        public void SetFireHeld(bool active)
        {
            if (!CanAcceptContinuousFireInput())
            {
                if (CanPreserveHeldAimWhileLocked())
                {
                    mobileFireHeld = active;
                    SetCurrentFireHeldState(active);
                    if (!active)
                    {
                        preserveHeldAimWhileCinematicLocked = false;
                    }

                    SetFireAimHold(currentFireHeld);
                    InvalidateFirePreviewCache();
                    return;
                }

                mobileFireHeld = false;
                SetCurrentFireHeldState(false);
                SetFireAimHold(false);
                InvalidateFirePreviewCache();
                return;
            }

            mobileFireHeld = active;
            SetCurrentFireHeldState(active && !IsCinematicInputLocked && IsRangedModeActive());
            SetFireAimHold(currentFireHeld);
            InvalidateFirePreviewCache();
        }

        public void SetExternalAimPreviewHeld(bool active)
        {
            if (active && !CanAcceptExternalAimPreview())
            {
                SetExternalAimPreviewHeldState(false);
                SetFireAimHold(false);
                InvalidateFirePreviewCache();
                return;
            }

            SetExternalAimPreviewHeldState(active);
            InvalidateFirePreviewCache();
            SetFireAimHold(currentFireHeld);
        }

        public void SetAimInput(Vector2 input)
        {
            if (!CanAcceptContinuousFireInput())
            {
                aimInput = Vector2.zero;
                InvalidateFirePreviewCache();
                return;
            }

            aimInput = Vector2.ClampMagnitude(input, 1f);
            InvalidateFirePreviewCache();
        }

        public void ClearAimInput()
        {
            aimInput = Vector2.zero;
            InvalidateFirePreviewCache();
        }

        public void SuppressDeviceFallbackThisFrame()
        {
            suppressDeviceFallbackThisFrame = true;
        }

        public void SetCinematicInputLocked(PlayerInputLockSource source, bool locked)
        {
            SetCinematicInputLocked(source, locked, false);
        }

        public void SetCinematicInputLocked(
            PlayerInputLockSource source,
            bool locked,
            bool preserveHeldAim)
        {
            bool wasLocked = IsCinematicInputLocked;
            bool wasPreservingHeldAim = preserveHeldAimWhileCinematicLocked;
            cinematicInputLockSources = PlayerInputLockMask.WithState(
                cinematicInputLockSources,
                source,
                locked);
            heldAimPreservationLockSources = PlayerInputLockMask.WithState(
                heldAimPreservationLockSources,
                source,
                locked && preserveHeldAim);

            if (!IsCinematicInputLocked)
            {
                preserveHeldAimWhileCinematicLocked = false;
                return;
            }

            bool everyOwnerPreservesHeldAim =
                heldAimPreservationLockSources == cinematicInputLockSources;
            if (wasLocked && wasPreservingHeldAim == everyOwnerPreservesHeldAim)
            {
                return;
            }

            queuedFire = false;
            pendingFireThisFrame = false;
            suppressDeviceFallbackThisFrame = true;
            InvalidateFirePreviewCache();
            preserveHeldAimWhileCinematicLocked = everyOwnerPreservesHeldAim
                && IsRangedModeActive()
                && (currentFireHeld || mobileFireHeld);
            if (preserveHeldAimWhileCinematicLocked)
            {
                mobileFireHeld = true;
                SetCurrentFireHeldState(true);
                SetFireAimHold(true);
                return;
            }

            mobileFireHeld = false;
            SetCurrentFireHeldState(false);
            SetFireAimHold(false);
        }

        public bool TryFire()
        {
            EnsureAmmoInitialized();
            UpdateReloadState();
            TryCancelAimReleaseReloadForImmediateUse();
            if (useMagazineReload && currentAmmo <= 0 && autoReloadWhenEmpty)
            {
                BeginReload();
            }

            if (!CanFire(out string blockedReason))
            {
                SetBlockedHint(blockedReason);
                return false;
            }

            LaneActionProjectile projectile = projectilePool.Get(ResolveProjectilePrefab(), projectileRoot);
            if (projectile == null)
            {
                SetBlockedHint("Ranged projectile prefab is missing.");
                return false;
            }

            InvalidateFirePreviewCache();
            ResolveFirePreview(out Vector3 direction, out Vector3 spawnPosition, out _);
            LastResolvedFireDirection = direction;
            lockTargetController?.NotifyAttackTarget(AimAssistTargetHealth);
            projectile.transform.SetParent(projectileRoot, worldPositionStays: true);
            projectile.transform.position = spawnPosition;
            DamageTeam resolvedSourceTeam = ResolveSourceTeam();
            projectile.Configure(
                sourceHealth,
                resolvedSourceTeam,
                damage,
                direction,
                projectileSpeed,
                projectileLifetimeSeconds,
                projectileRadius);

            RequestFacingOnFire(direction);
            TriggerAnimator(fireTrigger);
            RequestCameraFireCue();
            nextFireTime = Time.time + fireIntervalSeconds;
            LastUseBlockedReason = string.Empty;
            blockedHintUntil = 0f;
            ConsumeAmmoAfterFire();
            RangedProjectileFired?.Invoke(projectile);
            RangedFireStarted?.Invoke();
            TryBeginAutoReloadIfEmpty();
            return true;
        }

        public bool TryStartReload()
        {
            EnsureAmmoInitialized();
            UpdateReloadState();
            return BeginReload();
        }

        public void ConfigureReferences(
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newAimController,
            PlayerMovementController newMovement,
            PlayerCombatTargetSelector newTargetSelector,
            CombatHealth newSourceHealth,
            ActionCameraController newCameraController,
            Animator newAnimator)
        {
            bool resubscribeAim = isActiveAndEnabled;
            if (resubscribeAim)
            {
                UnsubscribeAimController();
            }

            combatModeController = newCombatModeController;
            aimController = newAimController;
            movement = newMovement;
            targetSelector = newTargetSelector;
            sourceHealth = newSourceHealth;
            cameraController = newCameraController;
            animator = newAnimator;

            if (resubscribeAim)
            {
                SubscribeAimController();
            }
        }

        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }

        public void SetFireOrigin(Transform newFireOrigin)
        {
            fireOrigin = newFireOrigin;
        }

        public void SetLockTargetController(PlayerLockTargetController newLockTargetController)
        {
            lockTargetController = newLockTargetController;
            InvalidateFirePreviewCache();
        }

        private void Awake()
        {
            if (combatModeController == null)
            {
                combatModeController = GetComponent<PlayerCombatModeController>();
            }

            if (aimController == null)
            {
                aimController = GetComponent<PlayerRangedAimController>();
            }

            if (movement == null)
            {
                movement = GetComponent<PlayerMovementController>();
            }

            if (targetSelector == null)
            {
                targetSelector = GetComponent<PlayerCombatTargetSelector>();
            }

            if (lockTargetController == null)
            {
                lockTargetController = GetComponent<PlayerLockTargetController>();
            }

            if (sourceHealth == null)
            {
                sourceHealth = GetComponent<CombatHealth>();
            }

            EnsureAmmoInitialized();
        }

        private void OnEnable()
        {
            EnsureAmmoInitialized();
            SubscribeAimController();
            actionEnabledHere = EnableActionIfNeeded(fireAction);
            projectilePool.Prewarm(ResolveProjectilePrefab(), projectileRoot, prewarmCount);
        }

        private void OnDisable()
        {
            UnsubscribeAimController();
            SetExternalAimPreviewHeldState(false);
            SetFireAimHold(false);
            DisableActionIfOwned(fireAction, actionEnabledHere);
            actionEnabledHere = false;
            queuedFire = false;
            mobileFireHeld = false;
            SetCurrentFireHeldState(false);
            lastCameraFireCueTime = float.NegativeInfinity;
            pendingFireThisFrame = false;
            suppressDeviceFallbackThisFrame = false;
            hasCachedFirePreview = false;
            firePreviewFrame = -1;
            ClearAimInput();
        }

        private void Update()
        {
            EnsureAmmoInitialized();
            UpdateReloadState();

            if (!IsRangedModeActive())
            {
                SetFireAimHold(false);
                SetCurrentFireHeldState(false);
                pendingFireThisFrame = false;
                suppressDeviceFallbackThisFrame = false;
                return;
            }

            if (IsCinematicInputLocked)
            {
                queuedFire = false;
                pendingFireThisFrame = false;
                suppressDeviceFallbackThisFrame = false;
                if (CanPreserveHeldAimWhileLocked())
                {
                    SetCurrentFireHeldState(mobileFireHeld);
                    SetFireAimHold(currentFireHeld);
                    return;
                }

                preserveHeldAimWhileCinematicLocked = false;
                SetFireAimHold(false);
                mobileFireHeld = false;
                SetCurrentFireHeldState(false);
                return;
            }

            bool pressed = ReadFirePressed();
            bool held = fireContinuouslyWhileHeld && ReadFireHeld();
            bool wasFireHeld = currentFireHeld;
            SetCurrentFireHeldState(held || pressed);
            SetFireAimHold(currentFireHeld);
            if (pressed || (currentFireHeld && !wasFireHeld))
            {
                RangedFireInputStarted?.Invoke();
            }

            if (pressed || held)
            {
                pendingFireThisFrame = true;
            }

            if (driveCameraAimAssist)
            {
                ResolveFirePreview(out _, out _, out _);
                UpdateCameraAimAssistIfNeeded();
            }

            suppressDeviceFallbackThisFrame = false;
        }

        private void LateUpdate()
        {
            if (!pendingFireThisFrame)
            {
                return;
            }

            pendingFireThisFrame = false;
            if (IsCinematicInputLocked || !IsRangedModeActive())
            {
                return;
            }

            InvalidateFirePreviewCache();
            TryFire();
        }

        private bool CanFire(out string blockedReason)
        {
            EnsureAmmoInitialized();
            UpdateReloadState();

            if (!IsRangedModeActive())
            {
                blockedReason = "Switch to ranged mode.";
                return false;
            }

            if (requireAimToFire && (aimController == null || !aimController.IsAiming))
            {
                blockedReason = "Hold aim before firing.";
                return false;
            }

            if (sourceHealth != null && !sourceHealth.IsAlive)
            {
                blockedReason = "Player is down.";
                return false;
            }

            if (isReloading)
            {
                blockedReason = "Reloading.";
                return false;
            }

            if (useMagazineReload && currentAmmo <= 0)
            {
                blockedReason = "Magazine empty.";
                return false;
            }

            if (Time.time < nextFireTime)
            {
                blockedReason = string.Empty;
                return false;
            }

            if (ResolveProjectilePrefab() == null)
            {
                blockedReason = "Ranged projectile prefab is missing.";
                return false;
            }

            blockedReason = string.Empty;
            return true;
        }

        private void EnsureAmmoInitialized()
        {
            ClampReloadSettings();
            if (!ammoInitialized)
            {
                currentAmmo = magazineSize;
                ammoInitialized = true;
            }

            if (!useMagazineReload)
            {
                currentAmmo = magazineSize;
                isReloading = false;
                reloadStartedByAimRelease = false;
                reloadFinishTime = 0f;
                return;
            }

            currentAmmo = Mathf.Clamp(currentAmmo, 0, magazineSize);
        }

        private void ClampReloadSettings()
        {
            magazineSize = Mathf.Max(1, magazineSize);
            reloadSeconds = Mathf.Max(0.01f, reloadSeconds);
            if (currentAmmo > magazineSize)
            {
                currentAmmo = magazineSize;
            }
        }

        private void UpdateReloadState()
        {
            if (!isReloading || Time.time < reloadFinishTime)
            {
                return;
            }

            isReloading = false;
            reloadStartedByAimRelease = false;
            reloadFinishTime = 0f;
            currentAmmo = magazineSize;
            LastUseBlockedReason = string.Empty;
            blockedHintUntil = 0f;
            RangedReloadCompleted?.Invoke();
            RequestContinuousFireAfterReloadIfHeld();
        }

        private void RequestContinuousFireAfterReloadIfHeld()
        {
            if (!fireContinuouslyWhileHeld || !CanAcceptContinuousFireInput() || !ReadFireHeld())
            {
                return;
            }

            SetCurrentFireHeldState(true);
            pendingFireThisFrame = true;
            SetFireAimHold(true);
            InvalidateFirePreviewCache();
        }

        private void ConsumeAmmoAfterFire()
        {
            if (!useMagazineReload)
            {
                return;
            }

            currentAmmo = Mathf.Max(0, currentAmmo - 1);
        }

        private void TryBeginAutoReloadIfEmpty()
        {
            if (useMagazineReload && autoReloadWhenEmpty && currentAmmo <= 0)
            {
                BeginReload();
            }
        }

        private bool BeginReload(bool startedByAimRelease = false)
        {
            if (!useMagazineReload || isReloading || currentAmmo >= magazineSize)
            {
                return false;
            }

            isReloading = true;
            reloadStartedByAimRelease = startedByAimRelease;
            reloadFinishTime = Time.time + reloadSeconds;
            pendingFireThisFrame = false;
            RangedReloadStarted?.Invoke();
            return true;
        }

        private bool TryCancelAimReleaseReloadForImmediateUse()
        {
            if (!cancelAimReleaseReloadOnAimResume
                || !isReloading
                || !reloadStartedByAimRelease
                || currentAmmo <= 0)
            {
                return false;
            }

            isReloading = false;
            reloadStartedByAimRelease = false;
            reloadFinishTime = 0f;
            LastUseBlockedReason = string.Empty;
            blockedHintUntil = 0f;
            RangedReloadCanceled?.Invoke();
            return true;
        }

        private void SubscribeAimController()
        {
            if (subscribedAimController == aimController)
            {
                return;
            }

            UnsubscribeAimController();
            if (aimController == null)
            {
                return;
            }

            subscribedAimController = aimController;
            subscribedAimController.AimModeChanged += HandleAimModeChanged;
        }

        private void UnsubscribeAimController()
        {
            if (subscribedAimController == null)
            {
                return;
            }

            subscribedAimController.AimModeChanged -= HandleAimModeChanged;
            subscribedAimController = null;
        }

        private void HandleAimModeChanged(bool isAiming)
        {
            if (isAiming)
            {
                TryCancelAimReleaseReloadForImmediateUse();
                return;
            }

            if (!reloadWhenAimReleased || IsCinematicInputLocked || !IsRangedModeActive())
            {
                return;
            }

            EnsureAmmoInitialized();
            UpdateReloadState();
            if (useMagazineReload && currentAmmo < magazineSize)
            {
                BeginReload(startedByAimRelease: true);
            }
        }

        private Vector3 ResolveFireDirection(out Vector3 spawnPosition)
        {
            ResolveFirePreview(out Vector3 direction, out spawnPosition, out _);
            return direction;
        }

        private Vector3 ResolveSpawnPosition(Vector3 fallbackDirection)
        {
            return fireOrigin != null
                ? fireOrigin.position
                : ResolveDefaultFireOriginPosition(fallbackDirection);
        }

        private Vector3 ResolveAimOriginPosition(Vector3 spawnPosition, Vector3 fallbackDirection)
        {
            return useStableAimOrigin ? ResolveDefaultFireOriginPosition(fallbackDirection) : spawnPosition;
        }

        private Vector3 ResolveDefaultFireOriginPosition(Vector3 fallbackDirection)
        {
            Vector3 planarDirection = ResolvePlanarDirection(fallbackDirection, transform.forward);
            return transform.position + planarDirection * spawnForwardOffset + Vector3.up * spawnHeight;
        }

        private Vector3 ResolveBaseFireDirection(Vector3 fallbackDirection)
        {
            Vector3 baseForward = cameraController != null
                ? cameraController.GetAimPlanarForward()
                : fireOrigin != null
                    ? fireOrigin.forward
                    : fallbackDirection;
            return ResolvePlanarDirection(baseForward, fallbackDirection);
        }

        private Vector3 ResolveManualAimDirection(Vector3 fallbackDirection)
        {
            if (aimInput.sqrMagnitude <= aimInputDeadZone * aimInputDeadZone)
            {
                return fallbackDirection;
            }

            Vector2 resolvedAimInput = Vector2.ClampMagnitude(aimInput, 1f);
            Quaternion yaw = Quaternion.AngleAxis(resolvedAimInput.x * aimInputYawDegrees, Vector3.up);
            return ResolvePlanarDirection(yaw * fallbackDirection, fallbackDirection);
        }

        public bool TryGetAimPreviewViewportPoint(out Vector2 viewportPoint)
        {
            viewportPoint = ResolveAimViewportPoint();
            if (!aimFromCameraViewport
                || !IsRangedModeActive()
                || cameraController == null)
            {
                return false;
            }

            if (!TryGetAimPreviewWorldPoint(out Vector3 aimPoint)
                || !cameraController.TryWorldToViewportPoint(aimPoint, out Vector3 projectedPoint))
            {
                return false;
            }

            viewportPoint = new Vector2(
                Mathf.Clamp01(projectedPoint.x),
                Mathf.Clamp01(projectedPoint.y));
            return true;
        }

        public bool TryGetAimPreviewWorldPoint(out Vector3 aimPoint)
        {
            aimPoint = default;
            if (!IsRangedModeActive())
            {
                return false;
            }

            ResolveFirePreview(out _, out _, out aimPoint);
            return true;
        }

        public bool TryGetAimPreviewDirection(out Vector3 direction)
        {
            if (!IsRangedModeActive())
            {
                direction = LastResolvedFireDirection;
                return false;
            }

            ResolveFirePreview(out direction, out _, out _);
            return true;
        }

        public bool TryGetAimAssistPreviewViewportPoint(out Vector2 viewportPoint)
        {
            viewportPoint = new Vector2(0.5f, 0.5f);
            if (cameraController == null
                || !TryGetAimPreviewDirection(out _)
                || !HasAimAssistTarget)
            {
                return false;
            }

            ResolveFirePreview(out _, out _, out Vector3 aimPoint);
            if (!cameraController.TryWorldToViewportPoint(aimPoint, out Vector3 projectedPoint))
            {
                return false;
            }

            viewportPoint = new Vector2(
                Mathf.Clamp01(projectedPoint.x),
                Mathf.Clamp01(projectedPoint.y));
            return true;
        }

        public bool TryGetTargetLockedAimViewportPoint(out Vector2 viewportPoint)
        {
            viewportPoint = new Vector2(0.5f, 0.5f);
            if (cameraController == null
                || !TryGetAimPreviewDirection(out _)
                || !HasAimAssistTarget
                || AimAssistTargetHealth == null
                || !AimAssistTargetHealth.IsAlive)
            {
                return false;
            }

            Vector3 aimPoint = ResolveTargetLockedAimPoint(AimAssistTargetHealth);
            if (!cameraController.TryWorldToViewportPoint(aimPoint, out Vector3 projectedPoint))
            {
                return false;
            }

            viewportPoint = new Vector2(
                Mathf.Clamp01(projectedPoint.x),
                Mathf.Clamp01(projectedPoint.y));
            return true;
        }

        private void ResolveFirePreview(
            out Vector3 direction,
            out Vector3 spawnPosition,
            out Vector3 aimPoint)
        {
            if (firePreviewFrame != Time.frameCount || !hasCachedFirePreview)
            {
                RefreshFirePreview();
            }

            direction = cachedFirePreviewDirection;
            spawnPosition = cachedFirePreviewSpawnPosition;
            aimPoint = cachedFirePreviewAimPoint;
        }

        private void InvalidateFirePreviewCache()
        {
            hasCachedFirePreview = false;
            firePreviewFrame = -1;
        }

        private void RefreshFirePreview()
        {
            firePreviewFrame = Time.frameCount;
            hasCachedFirePreview = true;

            Vector3 fallbackDirection = movement != null ? movement.FacingDirection : transform.forward;
            fallbackDirection = ResolveBaseFireDirection(fallbackDirection);
            Vector3 spawnPosition = ResolveSpawnPosition(fallbackDirection);

            Vector3 aimOriginPosition = ResolveAimOriginPosition(spawnPosition, fallbackDirection);
            bool hasViewportAimPoint = TryResolveCameraAimPoint(
                aimOriginPosition,
                fallbackDirection,
                out Vector3 rawViewportAimPoint,
                out CombatHealth directViewportTargetHealth);
            Vector3 rawAimDirection = hasViewportAimPoint
                ? ResolveFireTravelDirection(rawViewportAimPoint - aimOriginPosition, fallbackDirection)
                : ResolveManualAimDirection(fallbackDirection);
            Vector3 resolvedDirection = ResolveAssistedAimDirection(
                aimOriginPosition,
                spawnPosition,
                rawAimDirection,
                directViewportTargetHealth);
            bool hasResolvedAimAssistTarget = AimAssistTargetHealth != null && AimAssistTargetHealth.IsAlive;
            bool hasSoftAimAssist = aimAssistSuppressesViewportReprojection
                || (hasResolvedAimAssistTarget && directViewportTargetHealth == null);
            Vector3 assistPreviewPoint = default;
            bool hasAimAssistPreviewPoint = hasResolvedAimAssistTarget
                && TryResolveAimAssistPreviewPoint(out assistPreviewPoint);
            if (hasAimAssistPreviewPoint)
            {
                resolvedDirection = ResolveFireTravelDirection(assistPreviewPoint - spawnPosition, resolvedDirection);
            }
            else if (hasViewportAimPoint && !hasSoftAimAssist)
            {
                resolvedDirection = ResolveFireTravelDirection(rawViewportAimPoint - spawnPosition, resolvedDirection);
            }

            cachedFirePreviewDirection = resolvedDirection;
            cachedFirePreviewSpawnPosition = spawnPosition;
            if (hasAimAssistPreviewPoint)
            {
                cachedFirePreviewAimPoint = assistPreviewPoint;
            }
            else if (hasViewportAimPoint && !hasSoftAimAssist)
            {
                cachedFirePreviewAimPoint = rawViewportAimPoint;
            }
            else if (hasSoftAimAssist && TryResolveAimAssistPreviewPoint(out assistPreviewPoint))
            {
                cachedFirePreviewAimPoint = assistPreviewPoint;
            }
            else
            {
                cachedFirePreviewAimPoint = ResolveFirePreviewAimPoint(spawnPosition, resolvedDirection);
            }
        }

        private bool TryResolveAimAssistPreviewPoint(out Vector3 aimPoint)
        {
            CombatHealth targetHealth = AimAssistTargetHealth;
            if (targetHealth == null || !targetHealth.IsAlive)
            {
                aimPoint = default;
                return false;
            }

            aimPoint = hasAimAssistPreviewPoint
                ? aimAssistPreviewPoint
                : targetHealth.transform.position + Vector3.up * targetHeight;
            return true;
        }

        private Vector3 ResolveFirePreviewAimPoint(Vector3 spawnPosition, Vector3 direction)
        {
            float previewDistance = Mathf.Max(
                1f,
                Mathf.Max(cameraAimFallbackDistance, cameraAimRaycastDistance),
                aimAssistDistance);
            if (TryResolveShotPreviewHit(spawnPosition, direction, previewDistance, out Vector3 hitPoint))
            {
                return hitPoint;
            }

            return spawnPosition + ResolveFireTravelDirection(direction, LastResolvedFireDirection) * previewDistance;
        }

        private Vector3 ResolveTargetLockedAimPoint(CombatHealth targetHealth)
        {
            if (TryResolveStableTargetAimPoint(targetHealth, out Vector3 aimPoint))
            {
                return aimPoint;
            }

            return targetHealth.transform.position + Vector3.up * targetHeight;
        }

        private bool TryResolveShotPreviewHit(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            out Vector3 hitPoint)
        {
            hitPoint = default;
            Vector3 resolvedDirection = ResolveFireTravelDirection(direction, LastResolvedFireDirection);
            if (resolvedDirection.sqrMagnitude <= 0.0001f || maxDistance <= 0.01f)
            {
                return false;
            }

            int hitCount = Physics.RaycastNonAlloc(
                new Ray(origin, resolvedDirection),
                cameraAimHits,
                maxDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);
            if (hitCount <= 0)
            {
                return false;
            }

            int nearestHitIndex = -1;
            float nearestHitDistance = float.PositiveInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = cameraAimHits[i];
                if (!IsValidCameraAimHit(hit, out _))
                {
                    continue;
                }

                if (hit.distance < nearestHitDistance)
                {
                    nearestHitDistance = hit.distance;
                    nearestHitIndex = i;
                }
            }

            if (nearestHitIndex < 0)
            {
                return false;
            }

            hitPoint = cameraAimHits[nearestHitIndex].point;
            return true;
        }

        private void UpdateCameraAimAssistIfNeeded()
        {
            if (!driveCameraAimAssist
                || cameraController == null
                || (!currentFireHeld && !externalAimPreviewHeld)
                || !IsAimPreviewActive
                || !TryGetAimPreviewDirection(out _)
                || !HasAimAssistTarget
                || !aimAssistMayDriveCamera
                || AimAssistTargetHealth == null)
            {
                return;
            }

            float strength = AimAssistStrength01 * cameraAimAssistStrengthScale;
            if (strength < cameraAimAssistMinStrength)
            {
                return;
            }

            Vector3 targetOffset = Vector3.ProjectOnPlane(
                AimAssistTargetHealth.transform.position + Vector3.up * targetHeight - cameraController.transform.position,
                Vector3.up);
            if (targetOffset.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 baseForward = Quaternion.Euler(0f, cameraController.OrbitYawDegrees, 0f) * Vector3.forward;
            float targetYawOffset = Vector3.SignedAngle(
                baseForward,
                targetOffset.normalized,
                Vector3.up);
            cameraController.RequestAimAssistYawTarget(targetYawOffset, strength);
        }

        private bool TryResolveCameraAimPoint(
            Vector3 aimOriginPosition,
            Vector3 fallbackDirection,
            out Vector3 aimPoint,
            out CombatHealth directTargetHealth)
        {
            if (cameraController == null
                || !cameraController.TryGetViewportAimRay(ResolveAimViewportPoint(), out Ray ray))
            {
                aimPoint = default;
                directTargetHealth = null;
                return false;
            }

            aimPoint = ResolveCameraAimPoint(ray, aimOriginPosition, fallbackDirection, out directTargetHealth);
            return true;
        }

        private Vector3 ResolveCameraAimPoint(
            Ray ray,
            Vector3 aimOriginPosition,
            Vector3 fallbackDirection,
            out CombatHealth directTargetHealth)
        {
            directTargetHealth = null;
            if (TryResolveCameraAimHit(ray, out Vector3 hitPoint, out CombatHealth hitTargetHealth))
            {
                if (hitTargetHealth != null)
                {
                    directTargetHealth = hitTargetHealth;
                    return ResolveDirectTargetAimPoint(hitTargetHealth, hitPoint);
                }

                if (!cameraAimIgnoresNonTargetHits)
                {
                    return hitPoint;
                }
            }

            float fallbackDistance = Mathf.Max(cameraAimFallbackDistance, cameraAimRaycastDistance);
            if (fallbackDistance > 0.01f)
            {
                return ray.GetPoint(fallbackDistance);
            }

            return aimOriginPosition + fallbackDirection * cameraAimFallbackDistance;
        }

        private bool TryResolveCameraAimHit(Ray ray, out Vector3 hitPoint, out CombatHealth directTargetHealth)
        {
            directTargetHealth = null;
            hitPoint = default;

            int hitCount = Physics.RaycastNonAlloc(
                ray,
                cameraAimHits,
                Mathf.Max(cameraAimRaycastDistance, cameraAimFallbackDistance),
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);
            if (hitCount <= 0)
            {
                return false;
            }

            int nearestHitIndex = -1;
            float nearestHitDistance = float.PositiveInfinity;
            int nearestTargetHitIndex = -1;
            float nearestTargetHitDistance = float.PositiveInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = cameraAimHits[i];
                if (!IsValidCameraAimHit(hit, out CombatHealth hitTargetHealth))
                {
                    continue;
                }

                if (hitTargetHealth != null && hit.distance < nearestTargetHitDistance)
                {
                    nearestTargetHitDistance = hit.distance;
                    nearestTargetHitIndex = i;
                }

                if (hit.distance < nearestHitDistance)
                {
                    nearestHitDistance = hit.distance;
                    nearestHitIndex = i;
                }
            }

            if (nearestTargetHitIndex >= 0)
            {
                RaycastHit targetHit = cameraAimHits[nearestTargetHitIndex];
                directTargetHealth = ResolveHitCombatHealth(targetHit.collider);
                hitPoint = targetHit.point;
                return true;
            }

            if (nearestHitIndex >= 0)
            {
                hitPoint = cameraAimHits[nearestHitIndex].point;
                return true;
            }

            return false;
        }

        private Vector3 ResolveDirectTargetAimPoint(CombatHealth targetHealth, Vector3 hitPoint)
        {
            if (!stabilizeDirectTargetAimHeight || targetHealth == null)
            {
                return hitPoint;
            }

            if (!TryResolveStableTargetAimY(targetHealth, out float stableY))
            {
                return hitPoint;
            }

            if (Mathf.Abs(hitPoint.y - stableY) <= directTargetAimHeightTolerance)
            {
                return hitPoint;
            }

            return new Vector3(hitPoint.x, stableY, hitPoint.z);
        }

        private bool TryResolveStableTargetAimY(CombatHealth targetHealth, out float stableY)
        {
            stableY = default;
            if (!TryResolveStableTargetAimPoint(targetHealth, out Vector3 stableAimPoint))
            {
                return false;
            }

            stableY = stableAimPoint.y;
            return true;
        }

        private bool TryResolveStableTargetAimPoint(CombatHealth targetHealth, out Vector3 stableAimPoint)
        {
            stableAimPoint = default;
            if (targetHealth == null)
            {
                return false;
            }

            if (stableAimColliderTarget != targetHealth)
            {
                CacheStableAimColliders(targetHealth);
            }

            if (IsCachedAimHeightColliderActive(stableAimRootCollider))
            {
                stableAimPoint = stableAimRootCollider.bounds.center;
                return true;
            }

            Bounds bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < stableAimColliders.Count; i++)
            {
                Collider collider = stableAimColliders[i];
                if (!IsCachedAimHeightColliderActive(collider))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = collider.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            if (!hasBounds)
            {
                return false;
            }

            stableAimPoint = bounds.center;
            return true;
        }

        private void CacheStableAimColliders(CombatHealth targetHealth)
        {
            stableAimColliderTarget = targetHealth;
            stableAimRootCollider = null;
            stableAimColliders.Clear();
            if (targetHealth == null)
            {
                return;
            }

            Collider rootCollider = targetHealth.GetComponent<Collider>();
            if (IsAimHeightColliderOwnedByTarget(rootCollider, targetHealth))
            {
                stableAimRootCollider = rootCollider;
            }

            targetHealth.GetComponentsInChildren(includeInactive: false, stableAimColliders);
            for (int i = stableAimColliders.Count - 1; i >= 0; i--)
            {
                Collider collider = stableAimColliders[i];
                if (collider == stableAimRootCollider || !IsAimHeightColliderOwnedByTarget(collider, targetHealth))
                {
                    stableAimColliders.RemoveAt(i);
                }
            }
        }

        private static bool IsCachedAimHeightColliderActive(Collider collider)
        {
            return collider != null && collider.enabled && collider.gameObject.activeInHierarchy;
        }

        private static bool IsAimHeightColliderOwnedByTarget(Collider collider, CombatHealth targetHealth)
        {
            return collider != null
                && SummonPressureScreen.ResolveFromCollider(collider) == null
                && ResolveHitCombatHealth(collider) == targetHealth;
        }

        private Vector2 ResolveAimViewportPoint()
        {
            if (useFixedCenterAimViewport)
            {
                return new Vector2(0.5f, 0.5f);
            }

            Vector2 resolvedAimInput = aimInput.sqrMagnitude > aimInputDeadZone * aimInputDeadZone
                ? Vector2.ClampMagnitude(aimInput, 1f)
                : Vector2.zero;
            return new Vector2(
                Mathf.Clamp01(0.5f + resolvedAimInput.x * aimInputViewportOffsetX),
                Mathf.Clamp01(0.5f + resolvedAimInput.y * aimInputViewportOffsetY));
        }

        private Vector3 ResolveAssistedAimDirection(
            Vector3 aimOriginPosition,
            Vector3 projectileSpawnPosition,
            Vector3 rawAimDirection,
            CombatHealth directViewportTargetHealth)
        {
            LastRawAimDirection = ResolveFireTravelDirection(rawAimDirection, LastResolvedFireDirection);
            float assistAngle = aimController != null && aimController.IsAiming
                ? aimedAimAssistAngleDegrees
                : hipAimAssistAngleDegrees;
            float assistSelectionAngle = aimAssistMaxTurnDegrees > 0f
                ? Mathf.Min(assistAngle, aimAssistMaxTurnDegrees)
                : assistAngle;
            if (IsValidDirectViewportTarget(directViewportTargetHealth))
            {
                SetAimAssistState(directViewportTargetHealth, 1f, LastRawAimDirection);
                SetAimAssistPreviewPoint(
                    directViewportTargetHealth,
                    ResolveTargetLockedAimPoint(directViewportTargetHealth));
                return LastRawAimDirection;
            }

            if (TryResolveLockTargetAimDirection(
                projectileSpawnPosition,
                rawAimDirection,
                out Vector3 lockTargetDirection,
                out Vector3 lockTargetAimPoint,
                out CombatHealth lockTargetHealth,
                out float lockStrength))
            {
                SetAimAssistState(
                    lockTargetHealth,
                    lockStrength,
                    lockTargetDirection,
                    allowCameraAimAssist: true,
                    suppressViewportReprojection: true);
                SetAimAssistPreviewPoint(lockTargetHealth, lockTargetAimPoint);
                return lockTargetDirection;
            }

            bool canUseSoftAimAssist = useAimAssist
                && targetSelector != null
                && (!disableAimAssistWithManualInput || !HasManualAimInput());
            if (canUseSoftAimAssist && targetSelector.TryGetRangedAimAssistDirection(
                projectileSpawnPosition,
                rawAimDirection,
                aimAssistDistance,
                assistSelectionAngle,
                out Vector3 selectorAssistDirection,
                out Vector3 selectorAssistAimPoint,
                out CombatHealth assistTargetHealth))
            {
                Vector3 assistDirection = ResolveFireTravelDirection(
                    selectorAssistAimPoint - projectileSpawnPosition,
                    selectorAssistDirection);
                Vector3 resolvedDirection = ResolveFireTravelDirection(assistDirection, rawAimDirection);
                float assistTargetAngle = Vector3.Angle(LastRawAimDirection, assistDirection);
                float assistStrength = assistSelectionAngle > 0f
                    ? 1f - Mathf.Clamp01(assistTargetAngle / assistSelectionAngle)
                    : 0f;
                SetAimAssistState(assistTargetHealth, assistStrength, resolvedDirection);
                SetAimAssistPreviewPoint(assistTargetHealth, selectorAssistAimPoint);
                return resolvedDirection;
            }

            SetAimAssistState(null, 0f, LastRawAimDirection);
            return rawAimDirection;
        }

        private bool TryResolveLockTargetAimDirection(
            Vector3 projectileSpawnPosition,
            Vector3 rawAimDirection,
            out Vector3 resolvedDirection,
            out Vector3 aimPoint,
            out CombatHealth targetHealth,
            out float strength01)
        {
            resolvedDirection = rawAimDirection;
            aimPoint = default;
            targetHealth = null;
            strength01 = 0f;
            if (lockTargetController == null
                || !lockTargetController.TryGetLockDirection(
                    projectileSpawnPosition,
                    rawAimDirection,
                    out Vector3 lockDirection,
                    out aimPoint,
                    out targetHealth,
                    out strength01))
            {
                return false;
            }

            if (!IsValidAimTarget(targetHealth))
            {
                return false;
            }

            resolvedDirection = ResolveFireTravelDirection(lockDirection, rawAimDirection);
            strength01 = Mathf.Clamp01(strength01);
            return strength01 > 0f;
        }

        private bool HasManualAimInput()
        {
            return aimInput.sqrMagnitude > aimInputDeadZone * aimInputDeadZone;
        }

        private void SetAimAssistState(
            CombatHealth targetHealth,
            float strength01,
            Vector3 assistedDirection,
            bool allowCameraAimAssist = true,
            bool suppressViewportReprojection = false)
        {
            if (!IsValidAimTarget(targetHealth))
            {
                targetHealth = null;
                strength01 = 0f;
            }

            AimAssistTargetHealth = targetHealth;
            HasAimAssistTarget = targetHealth != null && targetHealth.IsAlive && strength01 > 0f;
            AimAssistStrength01 = HasAimAssistTarget ? Mathf.Clamp01(strength01) : 0f;
            aimAssistMayDriveCamera = HasAimAssistTarget && allowCameraAimAssist;
            aimAssistSuppressesViewportReprojection = HasAimAssistTarget && suppressViewportReprojection;
            LastAimAssistDirection = ResolveFireTravelDirection(assistedDirection, LastRawAimDirection);
            hasAimAssistPreviewPoint = false;
            aimAssistPreviewPoint = default;
        }

        private void SetAimAssistPreviewPoint(CombatHealth targetHealth, Vector3 previewPoint)
        {
            hasAimAssistPreviewPoint = HasAimAssistTarget && targetHealth != null;
            aimAssistPreviewPoint = hasAimAssistPreviewPoint ? previewPoint : default;
        }

        private bool IsValidDirectViewportTarget(CombatHealth targetHealth)
        {
            return IsValidAimTarget(targetHealth);
        }

        private bool IsValidAimTarget(CombatHealth targetHealth)
        {
            return targetHealth != null
                && targetHealth.IsAlive
                && targetHealth != sourceHealth
                && CombatTeamUtility.AreHostile(ResolveSourceTeam(), targetHealth.Team);
        }

        private DamageTeam ResolveSourceTeam()
        {
            return sourceHealth != null && sourceHealth.Team != DamageTeam.Neutral
                ? sourceHealth.Team
                : sourceTeam;
        }

        private void RequestFacingOnFire(Vector3 direction)
        {
            if (!requestFacingOnFire || movement == null)
            {
                return;
            }

            if (suppressFacingOnFireWhileMoving && IsMovingForFacingSuppression())
            {
                return;
            }

            movement.RequestFacingDirection(direction, facingHoldSeconds, snapFacingOnFire);
        }

        private bool IsMovingForFacingSuppression()
        {
            if (movement == null)
            {
                return false;
            }

            float threshold = movingFacingSuppressSpeed * movingFacingSuppressSpeed;
            Vector3 planarVelocity = Vector3.ProjectOnPlane(movement.PlanarVelocity, Vector3.up);
            return planarVelocity.sqrMagnitude > threshold;
        }

        private LaneActionProjectile ResolveProjectilePrefab()
        {
            if (projectilePrefab != null)
            {
                return projectilePrefab;
            }

            return projectilePrefabObject != null
                ? projectilePrefabObject.GetComponent<LaneActionProjectile>()
                : null;
        }

        private void RequestCameraFireCue()
        {
            if (cameraController == null)
            {
                cameraController = ActionCameraController.ActiveInstance;
                if (cameraController == null)
                {
                    return;
                }
            }

            bool sustainedFire = Time.time - lastCameraFireCueTime <= fireIntervalSeconds * 1.35f;
            lastCameraFireCueTime = Time.time;
            cameraController.RequestRifleFireFeedback(LastResolvedFireDirection, sustainedFire);
        }

        private void SetBlockedHint(string blockedReason)
        {
            if (string.IsNullOrWhiteSpace(blockedReason))
            {
                return;
            }

            LastUseBlockedReason = blockedReason;
            blockedHintUntil = Time.time + 1.1f;
        }

        private void SetCurrentFireHeldState(bool active)
        {
            if (currentFireHeld == active)
            {
                return;
            }

            currentFireHeld = active;
            AimPreviewStateChanged?.Invoke();
        }

        private void SetExternalAimPreviewHeldState(bool active)
        {
            if (externalAimPreviewHeld == active)
            {
                return;
            }

            externalAimPreviewHeld = active;
            AimPreviewStateChanged?.Invoke();
        }

        private void SetFireAimHold(bool active)
        {
            if (holdFireActivatesAim)
            {
                aimController?.SetFireAimHeld((active || externalAimPreviewHeld) && IsRangedModeActive());
            }
        }

        private bool IsRangedModeActive()
        {
            return combatModeController == null || combatModeController.IsRangedMode;
        }

        private bool CanAcceptQueuedFireInput()
        {
            return isActiveAndEnabled
                && !IsCinematicInputLocked
                && IsRangedModeActive();
        }

        private bool CanAcceptContinuousFireInput()
        {
            return isActiveAndEnabled
                && !IsCinematicInputLocked
                && IsRangedModeActive();
        }

        private bool CanPreserveHeldAimWhileLocked()
        {
            return isActiveAndEnabled
                && IsCinematicInputLocked
                && preserveHeldAimWhileCinematicLocked
                && IsRangedModeActive();
        }

        private bool CanAcceptExternalAimPreview()
        {
            return isActiveAndEnabled && IsRangedModeActive();
        }

        private bool ReadFirePressed()
        {
            bool pressed = queuedFire;
            queuedFire = false;

            if (fireAction != null && fireAction.action != null)
            {
                pressed |= fireAction.action.WasPressedThisFrame();
            }

            if (pressed || !useDeviceFallbackWhenActionMissing || !IsActionMissing(fireAction))
            {
                return pressed;
            }

            bool keyboardPressed = IsKeyboardPressed();
            if (suppressDeviceFallbackThisFrame)
            {
                return keyboardPressed;
            }

            return keyboardPressed
                || (ShouldReadMouseFireFallback()
                && Mouse.current.leftButton.wasPressedThisFrame)
                || (Gamepad.current != null
                && (Gamepad.current.rightTrigger.wasPressedThisFrame
                    || Gamepad.current.buttonWest.wasPressedThisFrame));
        }

        private bool ReadFireHeld()
        {
            bool held = mobileFireHeld;
            if (fireAction != null && fireAction.action != null)
            {
                held |= fireAction.action.IsPressed();
            }

            if (held || !useDeviceFallbackWhenActionMissing || !IsActionMissing(fireAction))
            {
                return held;
            }

            bool keyboardHeld = IsKeyboardHeld();
            if (suppressDeviceFallbackThisFrame)
            {
                return keyboardHeld;
            }

            return keyboardHeld
                || (ShouldReadMouseFireFallback()
                && Mouse.current.leftButton.isPressed)
                || (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.5f);
        }

        private bool ShouldReadMouseFireFallback()
        {
            if (Mouse.current == null)
            {
                return false;
            }

            bool canUseMouseFallback = allowMouseFireFallback
                || (allowDesktopMouseFireFallbackWhenActionMissing && !Application.isMobilePlatform);
            if (!canUseMouseFallback)
            {
                return false;
            }

            return !blockMouseFireFallbackOverUi
                || EventSystem.current == null
                || !EventSystem.current.IsPointerOverGameObject();
        }

        private bool IsKeyboardHeld()
        {
            return Keyboard.current != null
                && Keyboard.current[keyboardTestKey] != null
                && Keyboard.current[keyboardTestKey].isPressed;
        }

        private bool IsKeyboardPressed()
        {
            return Keyboard.current != null
                && Keyboard.current[keyboardTestKey] != null
                && Keyboard.current[keyboardTestKey].wasPressedThisFrame;
        }

        private void TriggerAnimator(string triggerName)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
            {
                animator.SetTrigger(triggerName);
            }
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction, Vector3 fallbackDirection)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            Vector3 planarFallback = Vector3.ProjectOnPlane(fallbackDirection, Vector3.up);
            return planarFallback.sqrMagnitude > 0.0001f ? planarFallback.normalized : Vector3.forward;
        }

        private Vector3 ResolveFireTravelDirection(Vector3 direction, Vector3 fallbackDirection)
        {
            if (preserveVerticalAim && direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }

            return ResolvePlanarDirection(direction, fallbackDirection);
        }

        private bool IsValidCameraAimHit(RaycastHit hit, out CombatHealth hitTargetHealth)
        {
            hitTargetHealth = null;
            Collider hitCollider = hit.collider;
            if (hitCollider == null)
            {
                return false;
            }

            Transform hitTransform = hitCollider.transform;
            if (hitTransform == null || hitTransform.IsChildOf(transform))
            {
                return false;
            }

            if (SummonPressureScreen.ResolveFromCollider(hitCollider) != null)
            {
                return false;
            }

            hitTargetHealth = ResolveHitCombatHealth(hitCollider);
            if (hitTargetHealth != null && !IsValidAimTarget(hitTargetHealth))
            {
                return false;
            }

            return true;
        }

        private static CombatHealth ResolveHitCombatHealth(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return null;
            }

            SummonFrontlineProxy targetProxy = SummonFrontlineProxy.ResolveFromCollider(hitCollider);
            if (targetProxy != null)
            {
                return targetProxy.Health ?? CombatHealth.ResolveFromCollider(hitCollider);
            }

            return CombatHealth.ResolveFromCollider(hitCollider);
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

        private static bool IsActionMissing(InputActionReference actionReference)
        {
            return actionReference == null || actionReference.action == null;
        }
    }

}
