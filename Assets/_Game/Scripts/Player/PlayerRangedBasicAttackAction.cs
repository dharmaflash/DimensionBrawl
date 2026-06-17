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
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;
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
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.Player;
        [SerializeField, Min(0f)] private float damage = 12f;
        [SerializeField, Min(0.01f)] private float projectileSpeed = 19f;
        [SerializeField, Min(0.01f)] private float projectileLifetimeSeconds = 1.4f;
        [SerializeField, Min(0.01f)] private float projectileRadius = 0.22f;
        [SerializeField, Min(0f)] private int prewarmCount = 8;

        [Header("Fire Feel")]
        [SerializeField, Min(0.01f)] private float fireIntervalSeconds = 0.22f;
        [SerializeField, Min(0f)] private float spawnForwardOffset = 0.85f;
        [SerializeField, Min(0f)] private float spawnHeight = 1.12f;
        [SerializeField, Min(0f)] private float targetHeight = 1.0f;
        [SerializeField, Min(0f)] private float facingHoldSeconds = 0.16f;
        [SerializeField] private bool snapFacingOnFire = true;
        [SerializeField] private bool requireAimToFire;
        [SerializeField] private string fireTrigger;

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
        private bool suppressDeviceFallbackThisFrame;
        private float nextFireTime;
        private float blockedHintUntil;

        public float FireCooldownRemaining => Mathf.Max(0f, nextFireTime - Time.time);
        public bool IsFireReady => FireCooldownRemaining <= 0f;
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

        public void QueueFire()
        {
            queuedFire = true;
        }

        public void SetFireHeld(bool active)
        {
            mobileFireHeld = active;
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

            movement?.RequestFacingDirection(direction, facingHoldSeconds, snapFacingOnFire);
            TriggerAnimator(fireTrigger);
            RequestCameraFireCue();
            nextFireTime = Time.time + fireIntervalSeconds;
            LastUseBlockedReason = string.Empty;
            blockedHintUntil = 0f;
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
            DisableActionIfOwned(fireAction, actionEnabledHere);
            actionEnabledHere = false;
            queuedFire = false;
            mobileFireHeld = false;
            suppressDeviceFallbackThisFrame = false;
        }

        private void Update()
        {
            if (combatModeController != null && !combatModeController.IsRangedMode)
            {
                suppressDeviceFallbackThisFrame = false;
                return;
            }

            bool pressed = ReadFirePressed();
            bool held = fireContinuouslyWhileHeld && ReadFireHeld();
            if (pressed || held)
            {
                TryFire();
            }

            suppressDeviceFallbackThisFrame = false;
        }

        private bool CanFire(out string blockedReason)
        {
            if (combatModeController != null && !combatModeController.IsRangedMode)
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
            fallbackDirection = ResolvePlanarDirection(fallbackDirection, transform.forward);
            spawnPosition = transform.position + fallbackDirection * spawnForwardOffset + Vector3.up * spawnHeight;

            if (targetSelector != null
                && targetSelector.TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth)
                && target != null
                && targetHealth != null
                && targetHealth.IsAlive)
            {
                Vector3 targetPoint = target.position + Vector3.up * targetHeight;
                return ResolvePlanarDirection(targetPoint - spawnPosition, fallbackDirection);
            }

            return fallbackDirection;
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
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
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
                || (Mouse.current != null && Mouse.current.leftButton.isPressed)
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
