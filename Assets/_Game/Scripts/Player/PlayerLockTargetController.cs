using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [DefaultExecutionOrder(80)]
    [DisallowMultipleComponent]
    public sealed class PlayerLockTargetController : MonoBehaviour
    {
        public enum LockTargetType
        {
            None = 0,
            SoftLock = 1,
            HardLock = 2
        }

        [Header("Input")]
        [SerializeField] private InputActionReference focusAction;
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;
        [SerializeField] private Key keyboardFocusKey = Key.Tab;

        [Header("References")]
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private Transform selectionOrigin;
        [SerializeField] private ActionCameraController cameraController;

        [Header("PGR-Style Lock")]
        [SerializeField] private bool autoAcquire = true;
        [SerializeField, Min(0f)] private float softLockDistance = 34f;
        [SerializeField, Min(0f)] private float lockBreakDistance = 40f;
        [SerializeField, Range(1f, 120f)] private float softLockAngleDegrees = 58f;
        [SerializeField, Range(1f, 160f)] private float retainedLockAngleDegrees = 86f;
        [SerializeField, Min(0f)] private float currentTargetStickiness = 0.62f;
        [SerializeField, Min(0f)] private float retargetIntervalSeconds = 0.08f;
        [SerializeField, Min(0f)] private float lostTargetGraceSeconds = 0.25f;
        [SerializeField, Min(0f)] private float fallbackAimHeight = 1.05f;
        [SerializeField] private bool clearWhenPlayerDown = true;

        [Header("Camera Intent")]
        [SerializeField, Min(0f)] private float cameraIntentStrongSeconds = 0.55f;
        [SerializeField, Min(0f)] private float cameraIntentFadeSeconds = 1.35f;
        [SerializeField, Range(1f, 160f)] private float cameraIntentAngleDegrees = 64f;
        [SerializeField, Min(0f)] private float cameraIntentTargetStickiness = 1.05f;
        [SerializeField, Min(0f)] private float attackStickySeconds = 0.38f;
        [SerializeField, Min(0f)] private float attackStickyTargetStickiness = 0.42f;

        private CombatHealth currentTargetHealth;
        private Vector3 currentTargetPoint;
        private LockTargetType currentLockType = LockTargetType.None;
        private float currentStrength01;
        private float nextRetargetTime;
        private float lostTargetUntil;
        private CombatHealth requestedHardLockTarget;
        private CombatHealth cameraIntentTarget;
        private float cameraIntentTargetUntil;
        private CombatHealth attackStickyTarget;
        private float attackStickyUntil;
        private bool actionEnabledHere;

        public CombatHealth CurrentTargetHealth => currentTargetHealth;
        public Transform CurrentTarget => currentTargetHealth != null ? currentTargetHealth.transform : null;
        public Vector3 CurrentTargetPoint => currentTargetPoint;
        public LockTargetType CurrentLockType => currentLockType;
        public float LockStrength01 => currentTargetHealth != null && currentTargetHealth.IsAlive ? currentStrength01 : 0f;
        public float CameraIntentStrength01 => ResolveCameraIntentStrength();
        public bool HasLockTarget => currentTargetHealth != null && currentTargetHealth.IsAlive && currentLockType != LockTargetType.None;

        public event Action<CombatHealth, CombatHealth> LockTargetChanged;

        public void ConfigureReferences(
            PlayerCombatTargetSelector newTargetSelector,
            CombatHealth newSourceHealth,
            ActionCameraController newCameraController,
            Transform newSelectionOrigin)
        {
            targetSelector = newTargetSelector;
            sourceHealth = newSourceHealth;
            cameraController = newCameraController;
            selectionOrigin = newSelectionOrigin;
            nextRetargetTime = 0f;
            RefreshLockTarget(force: true);
        }

        public void RequestHardLock(CombatHealth targetHealth)
        {
            requestedHardLockTarget = targetHealth != null && targetHealth.IsAlive ? targetHealth : null;
            if (requestedHardLockTarget == null)
            {
                ClearHardLock();
                return;
            }

            Vector3 origin = ResolveSelectionOriginPosition();
            Vector3 direction = ResolveViewDirection();
            if (targetSelector != null
                && targetSelector.TryGetBestLockTarget(
                    origin,
                    direction,
                    lockBreakDistance,
                    retainedLockAngleDegrees,
                    requestedHardLockTarget,
                    currentTargetStickiness,
                    out CombatHealth resolvedTarget,
                    out Vector3 lockPoint,
                    out float strength)
                && resolvedTarget == requestedHardLockTarget)
            {
                SetLockTarget(resolvedTarget, lockPoint, Mathf.Max(0.85f, strength), LockTargetType.HardLock);
            }
        }

        public void ToggleHardLockOnCurrentTarget()
        {
            if (currentLockType == LockTargetType.HardLock)
            {
                ClearHardLock();
                nextRetargetTime = 0f;
                return;
            }

            if (HasLockTarget)
            {
                RequestHardLock(currentTargetHealth);
                return;
            }

            RefreshLockTarget(force: true);
            if (HasLockTarget)
            {
                RequestHardLock(currentTargetHealth);
            }
        }

        public void ClearHardLock()
        {
            requestedHardLockTarget = null;
            if (currentLockType == LockTargetType.HardLock)
            {
                SetLockTarget(null, default, 0f, LockTargetType.None);
            }
        }

        public void ClearLock()
        {
            requestedHardLockTarget = null;
            SetLockTarget(null, default, 0f, LockTargetType.None);
        }

        public void NotifyAttackTarget(CombatHealth targetHealth)
        {
            if (!IsUsableRememberedTarget(targetHealth))
            {
                return;
            }

            attackStickyTarget = targetHealth;
            attackStickyUntil = Time.time + Mathf.Max(0f, attackStickySeconds);
            if (currentLockType != LockTargetType.HardLock)
            {
                nextRetargetTime = 0f;
            }
        }

        public bool TryGetLockDirection(
            Vector3 projectileSpawnPosition,
            Vector3 fallbackDirection,
            out Vector3 direction,
            out Vector3 aimPoint,
            out CombatHealth targetHealth,
            out float strength01)
        {
            if (!HasLockTarget)
            {
                direction = fallbackDirection;
                aimPoint = default;
                targetHealth = null;
                strength01 = 0f;
                return false;
            }

            aimPoint = ResolveLiveAimPoint();
            Vector3 offset = aimPoint - projectileSpawnPosition;
            if (offset.sqrMagnitude <= 0.0001f)
            {
                direction = fallbackDirection;
                targetHealth = currentTargetHealth;
                strength01 = LockStrength01;
                return false;
            }

            direction = offset.normalized;
            targetHealth = currentTargetHealth;
            strength01 = LockStrength01;
            return true;
        }

        public bool TryGetLockViewportPoint(out Vector2 viewportPoint)
        {
            viewportPoint = default;
            if (!HasLockTarget || cameraController == null)
            {
                return false;
            }

            if (!cameraController.TryWorldToViewportPoint(ResolveLiveAimPoint(), out Vector3 projectedPoint))
            {
                return false;
            }

            viewportPoint = new Vector2(Mathf.Clamp01(projectedPoint.x), Mathf.Clamp01(projectedPoint.y));
            return true;
        }

        private void Awake()
        {
            targetSelector ??= GetComponent<PlayerCombatTargetSelector>();
            sourceHealth ??= GetComponent<CombatHealth>();
            selectionOrigin ??= transform;
            cameraController ??= FindFirstObjectByType<ActionCameraController>();
        }

        private void OnEnable()
        {
            actionEnabledHere = EnableActionIfNeeded(focusAction);
            nextRetargetTime = 0f;
            RefreshLockTarget(force: true);
        }

        private void OnDisable()
        {
            DisableActionIfOwned(focusAction, actionEnabledHere);
            actionEnabledHere = false;
        }

        private void Update()
        {
            if (ReadFocusPressed())
            {
                ToggleHardLockOnCurrentTarget();
            }

            if (Time.time < nextRetargetTime)
            {
                return;
            }

            RefreshLockTarget(force: false);
        }

        private void RefreshLockTarget(bool force)
        {
            nextRetargetTime = Time.time + Mathf.Max(0.01f, retargetIntervalSeconds);
            if (!autoAcquire || targetSelector == null || IsSourceUnavailable())
            {
                ClearLock();
                return;
            }

            Vector3 origin = ResolveSelectionOriginPosition();
            Vector3 direction = ResolveViewDirection();
            PruneRememberedTargets();
            float cameraIntentStrength = requestedHardLockTarget == null ? ResolveCameraIntentStrength() : 0f;
            bool hasCameraIntent = cameraIntentStrength > 0f;
            float distance = currentTargetHealth != null ? lockBreakDistance : softLockDistance;
            float angle = ResolveSearchAngle(hasCameraIntent);
            CombatHealth preferredTarget = ResolvePreferredTarget(hasCameraIntent);
            float preferredBonus = ResolvePreferredTargetBonus(preferredTarget, hasCameraIntent, cameraIntentStrength);
            LockTargetType requestedType = requestedHardLockTarget != null ? LockTargetType.HardLock : LockTargetType.SoftLock;

            if (targetSelector.TryGetBestLockTarget(
                origin,
                direction,
                distance,
                angle,
                preferredTarget,
                preferredBonus,
                out CombatHealth nextTarget,
                out Vector3 lockPoint,
                out float strength)
                && (requestedHardLockTarget == null || nextTarget == requestedHardLockTarget))
            {
                lostTargetUntil = Time.time + lostTargetGraceSeconds;
                if (hasCameraIntent)
                {
                    cameraIntentTarget = nextTarget;
                    cameraIntentTargetUntil = Time.time
                        + Mathf.Max(0f, cameraIntentStrongSeconds)
                        + Mathf.Max(0f, cameraIntentFadeSeconds);
                }

                SetLockTarget(nextTarget, lockPoint, strength, requestedType);
                return;
            }

            if (!force && HasLockTarget && Time.time <= lostTargetUntil && IsWithinBreakDistance(currentTargetHealth, origin))
            {
                currentTargetPoint = ResolveLiveAimPoint();
                currentStrength01 = Mathf.Max(0.3f, currentStrength01 * 0.92f);
                return;
            }

            SetLockTarget(null, default, 0f, LockTargetType.None);
        }

        private CombatHealth ResolvePreferredTarget(bool hasCameraIntent)
        {
            if (requestedHardLockTarget != null)
            {
                return requestedHardLockTarget;
            }

            if (hasCameraIntent)
            {
                return cameraIntentTarget;
            }

            if (IsUsableRememberedTarget(attackStickyTarget) && Time.time <= attackStickyUntil)
            {
                return attackStickyTarget;
            }

            return currentTargetHealth;
        }

        private float ResolvePreferredTargetBonus(
            CombatHealth preferredTarget,
            bool hasCameraIntent,
            float cameraIntentStrength)
        {
            if (preferredTarget == null)
            {
                return 0f;
            }

            if (requestedHardLockTarget != null)
            {
                return Mathf.Max(0.85f, currentTargetStickiness);
            }

            if (hasCameraIntent)
            {
                return Mathf.Lerp(0f, cameraIntentTargetStickiness, cameraIntentStrength);
            }

            if (preferredTarget == attackStickyTarget && Time.time <= attackStickyUntil)
            {
                return Mathf.Max(currentTargetStickiness, attackStickyTargetStickiness);
            }

            return currentTargetStickiness;
        }

        private float ResolveSearchAngle(bool hasCameraIntent)
        {
            if (requestedHardLockTarget != null)
            {
                return retainedLockAngleDegrees;
            }

            if (hasCameraIntent)
            {
                return Mathf.Max(1f, cameraIntentAngleDegrees);
            }

            if (currentTargetHealth != null)
            {
                return retainedLockAngleDegrees;
            }

            return softLockAngleDegrees;
        }

        private float ResolveCameraIntentStrength()
        {
            if (cameraController == null)
            {
                return 0f;
            }

            return cameraController.ResolveManualViewIntentStrength(
                cameraIntentStrongSeconds,
                cameraIntentFadeSeconds);
        }

        private void PruneRememberedTargets()
        {
            if (!IsUsableRememberedTarget(requestedHardLockTarget))
            {
                requestedHardLockTarget = null;
            }

            if (!IsUsableRememberedTarget(cameraIntentTarget) || Time.time > cameraIntentTargetUntil)
            {
                cameraIntentTarget = null;
                cameraIntentTargetUntil = 0f;
            }

            if (!IsUsableRememberedTarget(attackStickyTarget) || Time.time > attackStickyUntil)
            {
                attackStickyTarget = null;
                attackStickyUntil = 0f;
            }
        }

        private bool IsSourceUnavailable()
        {
            return clearWhenPlayerDown && sourceHealth != null && !sourceHealth.IsAlive;
        }

        private static bool IsUsableRememberedTarget(CombatHealth targetHealth)
        {
            return targetHealth != null && targetHealth.IsAlive;
        }

        private bool IsWithinBreakDistance(CombatHealth targetHealth, Vector3 origin)
        {
            if (targetHealth == null || !targetHealth.IsAlive)
            {
                return false;
            }

            Vector3 offset = Vector3.ProjectOnPlane(targetHealth.transform.position - origin, Vector3.up);
            return offset.sqrMagnitude <= lockBreakDistance * lockBreakDistance;
        }

        private Vector3 ResolveLiveAimPoint()
        {
            if (currentTargetHealth == null)
            {
                return currentTargetPoint;
            }

            if ((currentTargetPoint - currentTargetHealth.transform.position).sqrMagnitude <= 0.0001f)
            {
                return currentTargetHealth.transform.position + Vector3.up * fallbackAimHeight;
            }

            return currentTargetPoint;
        }

        private Vector3 ResolveSelectionOriginPosition()
        {
            Transform origin = selectionOrigin != null ? selectionOrigin : transform;
            return origin.position + Vector3.up * Mathf.Max(0f, fallbackAimHeight * 0.5f);
        }

        private Vector3 ResolveViewDirection()
        {
            if (cameraController != null)
            {
                Vector3 cameraForward = cameraController.GetAimPlanarForward();
                if (cameraForward.sqrMagnitude > 0.0001f)
                {
                    return cameraForward.normalized;
                }
            }

            Transform origin = selectionOrigin != null ? selectionOrigin : transform;
            Vector3 forward = Vector3.ProjectOnPlane(origin.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private void SetLockTarget(
            CombatHealth nextTarget,
            Vector3 nextTargetPoint,
            float strength01,
            LockTargetType lockType)
        {
            CombatHealth previousTarget = currentTargetHealth;
            currentTargetHealth = nextTarget != null && nextTarget.IsAlive ? nextTarget : null;
            currentTargetPoint = currentTargetHealth != null
                ? nextTargetPoint
                : default;
            currentStrength01 = currentTargetHealth != null ? Mathf.Clamp01(strength01) : 0f;
            currentLockType = currentTargetHealth != null ? lockType : LockTargetType.None;

            if (previousTarget != currentTargetHealth)
            {
                LockTargetChanged?.Invoke(previousTarget, currentTargetHealth);
            }
        }

        private bool ReadFocusPressed()
        {
            bool pressed = false;
            if (focusAction != null && focusAction.action != null)
            {
                pressed |= focusAction.action.WasPressedThisFrame();
            }

            if (pressed || !useDeviceFallbackWhenActionMissing || !IsActionMissing(focusAction))
            {
                return pressed;
            }

            return Keyboard.current != null
                && Keyboard.current[keyboardFocusKey] != null
                && Keyboard.current[keyboardFocusKey].wasPressedThisFrame;
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
