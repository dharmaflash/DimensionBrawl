using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRangedBasicAttackAction : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private bool fireContinuouslyWhileHeld = true;
        [SerializeField] private bool holdFireActivatesAim = true;
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;
        [SerializeField] private bool allowMouseFireFallback;
        [SerializeField] private Key keyboardTestKey = Key.F;

        [Header("References")]
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedAimController aimController;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Animator animator;

        [Header("Projectile")]
        [SerializeField] private LaneActionProjectile projectilePrefab;
        [SerializeField] private GameObject projectilePrefabObject;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private Transform fireOrigin;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.Player;
        [SerializeField, Min(0f)] private float damage = 28f;
        [SerializeField, Min(0.01f)] private float projectileSpeed = 24f;
        [SerializeField, Min(0.01f)] private float projectileLifetimeSeconds = 1.75f;
        [SerializeField, Min(0.01f)] private float projectileRadius = 0.31f;
        [SerializeField, Min(0f)] private int prewarmCount = 8;

        [Header("Fire Feel")]
        [SerializeField, Min(0.01f)] private float fireIntervalSeconds = 0.24f;
        [SerializeField, Min(0f)] private float spawnForwardOffset = 0.85f;
        [SerializeField, Min(0f)] private float spawnHeight = 1.12f;
        [SerializeField, Min(0f)] private float targetHeight = 1.0f;
        [SerializeField, Min(0f)] private float facingHoldSeconds = 0.16f;
        [SerializeField] private bool requestFacingOnFire;
        [SerializeField] private bool snapFacingOnFire = true;
        [SerializeField] private bool suppressFacingOnFireWhileMoving = true;
        [SerializeField, Min(0f)] private float movingFacingSuppressSpeed = 0.08f;
        [SerializeField] private bool requireAimToFire;
        [SerializeField] private string fireTrigger;

        [Header("Manual Aim")]
        [SerializeField, Range(0f, 1f)] private float aimInputDeadZone = 0.18f;
        [SerializeField, Range(0f, 85f)] private float aimInputYawDegrees = 34f;
        [SerializeField] private bool aimFromCameraViewport = true;
        [SerializeField] private bool useFixedCenterAimViewport = true;
        [SerializeField] private bool preserveVerticalAim = true;
        [SerializeField, Min(1f)] private float cameraAimFallbackDistance = 32f;
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

        [Header("Camera Feedback")]
        [SerializeField] private Vector3 fireCameraCueOffset = new Vector3(0.025f, 0.01f, -0.045f);
        [SerializeField, Min(0.01f)] private float fireCameraCueSeconds = 0.10f;
        [SerializeField] private float fireFieldOfViewDelta = -0.8f;
        [SerializeField] private float fireCameraDistanceDelta = -0.04f;
        [SerializeField] private float fireFocusHeightDelta;

        private readonly List<LaneActionProjectile> projectiles = new List<LaneActionProjectile>(12);
        private bool actionEnabledHere;
        private bool queuedFire;
        private bool mobileFireHeld;
        private bool currentFireHeld;
        private bool suppressDeviceFallbackThisFrame;
        private float nextFireTime;
        private float blockedHintUntil;
        private Vector2 aimInput;

        public float FireCooldownRemaining => Mathf.Max(0f, nextFireTime - Time.time);
        public bool IsFireReady => FireCooldownRemaining <= 0f;
        public bool IsFireHeld => currentFireHeld;
        public bool IsAimPreviewActive => IsRangedModeActive()
            && (currentFireHeld || (aimController != null && aimController.IsAiming));
        public Vector2 AimInput => aimInput;
        public Transform FireOrigin => fireOrigin;
        public Vector3 LastResolvedFireDirection { get; private set; } = Vector3.forward;
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
                int count = 0;
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (projectiles[i] != null && projectiles[i].IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public event Action RangedFireStarted;
        public event Action<LaneActionProjectile> RangedProjectileFired;

        public void QueueFire()
        {
            queuedFire = true;
        }

        public void SetFireHeld(bool active)
        {
            mobileFireHeld = active;
        }

        public void SetAimInput(Vector2 input)
        {
            aimInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void ClearAimInput()
        {
            aimInput = Vector2.zero;
        }

        public void SuppressDeviceFallbackThisFrame()
        {
            suppressDeviceFallbackThisFrame = true;
        }

        public bool TryFire()
        {
            if (!CanFire(out string blockedReason))
            {
                SetBlockedHint(blockedReason);
                return false;
            }

            LaneActionProjectile projectile = GetProjectile();
            if (projectile == null)
            {
                SetBlockedHint("Ranged projectile prefab is missing.");
                return false;
            }

            Vector3 direction = ResolveFireDirection(out Vector3 spawnPosition);
            LastResolvedFireDirection = direction;
            projectile.transform.SetParent(projectileRoot, worldPositionStays: true);
            projectile.transform.position = spawnPosition;
            DamageTeam resolvedSourceTeam = sourceHealth != null && sourceHealth.Team != DamageTeam.Neutral
                ? sourceHealth.Team
                : sourceTeam;
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
            RangedProjectileFired?.Invoke(projectile);
            RangedFireStarted?.Invoke();
            return true;
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
            combatModeController = newCombatModeController;
            aimController = newAimController;
            movement = newMovement;
            targetSelector = newTargetSelector;
            sourceHealth = newSourceHealth;
            cameraController = newCameraController;
            animator = newAnimator;
        }

        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }

        public void SetFireOrigin(Transform newFireOrigin)
        {
            fireOrigin = newFireOrigin;
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

            if (sourceHealth == null)
            {
                sourceHealth = GetComponent<CombatHealth>();
            }
        }

        private void OnEnable()
        {
            actionEnabledHere = EnableActionIfNeeded(fireAction);
            PrewarmProjectiles();
        }

        private void OnDisable()
        {
            SetFireAimHold(false);
            DisableActionIfOwned(fireAction, actionEnabledHere);
            actionEnabledHere = false;
            queuedFire = false;
            mobileFireHeld = false;
            currentFireHeld = false;
            suppressDeviceFallbackThisFrame = false;
            ClearAimInput();
        }

        private void Update()
        {
            if (!IsRangedModeActive())
            {
                SetFireAimHold(false);
                currentFireHeld = false;
                suppressDeviceFallbackThisFrame = false;
                return;
            }

            bool pressed = ReadFirePressed();
            bool held = fireContinuouslyWhileHeld && ReadFireHeld();
            currentFireHeld = held || pressed;
            SetFireAimHold(currentFireHeld);
            UpdateCameraAimAssistIfNeeded();
            if (pressed || held)
            {
                TryFire();
            }

            suppressDeviceFallbackThisFrame = false;
        }

        private bool CanFire(out string blockedReason)
        {
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

        private Vector3 ResolveFireDirection(out Vector3 spawnPosition)
        {
            Vector3 fallbackDirection = movement != null ? movement.FacingDirection : transform.forward;
            fallbackDirection = ResolveBaseFireDirection(fallbackDirection);
            spawnPosition = ResolveSpawnPosition(fallbackDirection);

            Vector3 aimOriginPosition = ResolveAimOriginPosition(spawnPosition, fallbackDirection);
            Vector3 rawAimDirection = ResolveRawAimDirection(aimOriginPosition, fallbackDirection);
            return ResolveAssistedAimDirection(aimOriginPosition, spawnPosition, rawAimDirection);
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
                ? cameraController.transform.forward
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
            return aimFromCameraViewport && IsRangedModeActive();
        }

        public bool TryGetAimPreviewDirection(out Vector3 direction)
        {
            if (!IsRangedModeActive())
            {
                direction = LastResolvedFireDirection;
                return false;
            }

            direction = ResolveFireDirection(out _);
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

            Vector3 fallbackDirection = movement != null ? movement.FacingDirection : transform.forward;
            fallbackDirection = ResolveBaseFireDirection(fallbackDirection);
            Vector3 spawnPosition = ResolveSpawnPosition(fallbackDirection);
            Vector3 aimPoint = spawnPosition + LastAimAssistDirection * cameraAimFallbackDistance;
            if (!cameraController.TryWorldToViewportPoint(aimPoint, out Vector3 projectedPoint))
            {
                return false;
            }

            viewportPoint = new Vector2(
                Mathf.Clamp01(projectedPoint.x),
                Mathf.Clamp01(projectedPoint.y));
            return true;
        }

        private void UpdateCameraAimAssistIfNeeded()
        {
            if (!driveCameraAimAssist
                || cameraController == null
                || !currentFireHeld
                || !IsAimPreviewActive
                || !TryGetAimPreviewDirection(out _)
                || !HasAimAssistTarget
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

        private Vector3 ResolveRawAimDirection(Vector3 spawnPosition, Vector3 fallbackDirection)
        {
            if (aimFromCameraViewport
                && TryResolveCameraViewportDirection(spawnPosition, fallbackDirection, out Vector3 viewportDirection))
            {
                return viewportDirection;
            }

            return ResolveManualAimDirection(fallbackDirection);
        }

        private bool TryResolveCameraViewportDirection(
            Vector3 spawnPosition,
            Vector3 fallbackDirection,
            out Vector3 direction)
        {
            if (cameraController == null
                || !cameraController.TryGetViewportAimRay(ResolveAimViewportPoint(), out Ray ray))
            {
                direction = fallbackDirection;
                return false;
            }

            Vector3 aimPoint = ResolveCameraAimPoint(ray, spawnPosition, fallbackDirection);
            direction = ResolveFireTravelDirection(aimPoint - spawnPosition, fallbackDirection);
            return true;
        }

        private Vector3 ResolveCameraAimPoint(Ray ray, Vector3 spawnPosition, Vector3 fallbackDirection)
        {
            Plane aimPlane = new Plane(fallbackDirection.normalized, spawnPosition + fallbackDirection * cameraAimFallbackDistance);
            if (aimPlane.Raycast(ray, out float enter) && enter > 0f)
            {
                return ray.GetPoint(enter);
            }

            return spawnPosition + fallbackDirection * cameraAimFallbackDistance;
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
            Vector3 rawAimDirection)
        {
            LastRawAimDirection = ResolveFireTravelDirection(rawAimDirection, LastResolvedFireDirection);
            if (!useAimAssist
                || targetSelector == null
                || (disableAimAssistWithManualInput && HasManualAimInput()))
            {
                SetAimAssistState(null, 0f, LastRawAimDirection);
                return rawAimDirection;
            }

            float assistAngle = aimController != null && aimController.IsAiming
                ? aimedAimAssistAngleDegrees
                : hipAimAssistAngleDegrees;
            if (!targetSelector.TryGetAimAssistDirection(
                aimOriginPosition,
                rawAimDirection,
                aimAssistDistance,
                assistAngle,
                out Vector3 selectorAssistDirection,
                out CombatHealth assistTargetHealth))
            {
                SetAimAssistState(null, 0f, LastRawAimDirection);
                return rawAimDirection;
            }

            Vector3 assistDirection = assistTargetHealth != null
                ? ResolveFireTravelDirection(
                    assistTargetHealth.transform.position + Vector3.up * targetHeight - projectileSpawnPosition,
                    selectorAssistDirection)
                : selectorAssistDirection;
            Vector3 assistedDirection = Vector3.RotateTowards(
                rawAimDirection,
                assistDirection,
                aimAssistMaxTurnDegrees * Mathf.Deg2Rad,
                0f);
            Vector3 resolvedDirection = ResolveFireTravelDirection(assistedDirection, rawAimDirection);
            float assistTargetAngle = Vector3.Angle(LastRawAimDirection, assistDirection);
            float assistStrength = assistAngle > 0f
                ? 1f - Mathf.Clamp01(assistTargetAngle / assistAngle)
                : 0f;
            SetAimAssistState(assistTargetHealth, assistStrength, resolvedDirection);
            return resolvedDirection;
        }

        private bool HasManualAimInput()
        {
            return aimInput.sqrMagnitude > aimInputDeadZone * aimInputDeadZone;
        }

        private void SetAimAssistState(CombatHealth targetHealth, float strength01, Vector3 assistedDirection)
        {
            AimAssistTargetHealth = targetHealth;
            HasAimAssistTarget = targetHealth != null && targetHealth.IsAlive && strength01 > 0f;
            AimAssistStrength01 = HasAimAssistTarget ? Mathf.Clamp01(strength01) : 0f;
            LastAimAssistDirection = ResolveFireTravelDirection(assistedDirection, LastRawAimDirection);
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

        private LaneActionProjectile GetProjectile()
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                LaneActionProjectile projectile = projectiles[i];
                if (projectile != null && !projectile.IsActive)
                {
                    return projectile;
                }
            }

            LaneActionProjectile prefab = ResolveProjectilePrefab();
            if (prefab == null)
            {
                return null;
            }

            LaneActionProjectile instance = Instantiate(prefab, projectileRoot);
            instance.gameObject.SetActive(false);
            projectiles.Add(instance);
            return instance;
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

        private void PrewarmProjectiles()
        {
            LaneActionProjectile prefab = ResolveProjectilePrefab();
            if (prefab == null)
            {
                return;
            }

            int targetCount = Mathf.Max(0, prewarmCount);
            while (projectiles.Count < targetCount)
            {
                LaneActionProjectile instance = Instantiate(prefab, projectileRoot);
                instance.gameObject.SetActive(false);
                projectiles.Add(instance);
            }
        }

        private void RequestCameraFireCue()
        {
            if (cameraController == null)
            {
                return;
            }

            cameraController.RequestCue(
                fireCameraCueOffset,
                fireCameraCueSeconds,
                fireFieldOfViewDelta,
                fireCameraDistanceDelta,
                fireFocusHeightDelta);
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

        private void SetFireAimHold(bool active)
        {
            if (holdFireActivatesAim)
            {
                aimController?.SetFireAimHeld(active && IsRangedModeActive());
            }
        }

        private bool IsRangedModeActive()
        {
            return combatModeController == null || combatModeController.IsRangedMode;
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

            return !suppressDeviceFallbackThisFrame
                && (IsKeyboardPressed()
                || (allowMouseFireFallback
                    && Mouse.current != null
                    && Mouse.current.leftButton.wasPressedThisFrame)
                || (Gamepad.current != null
                    && (Gamepad.current.rightTrigger.wasPressedThisFrame
                        || Gamepad.current.buttonWest.wasPressedThisFrame)));
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

            return !suppressDeviceFallbackThisFrame
                && (IsKeyboardHeld()
                || (allowMouseFireFallback
                    && Mouse.current != null
                    && Mouse.current.leftButton.isPressed)
                || (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.5f));
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
