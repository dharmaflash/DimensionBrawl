using System;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRangedAimController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference aimAction;
        [SerializeField] private bool holdToAim = true;
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;
        [SerializeField] private bool allowMouseAimFallback;

        [Header("References")]
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private Animator animator;

        [Header("Animation")]
        [SerializeField] private string aimingParameter = "IsAiming";

        [Header("Facing")]
        [SerializeField] private bool faceCameraForwardWhileAiming = true;
        [SerializeField, Min(0f)] private float aimingFacingHoldSeconds = 0.08f;
        [SerializeField] private bool snapAimingFacing;

        private bool actionEnabledHere;
        private bool mobileAimHeld;
        private bool fireAimHeld;
        private bool queuedAimToggle;
        private bool isAiming;
        private Vector2 aimInput;

        public bool IsAiming => isAiming;
        public Vector2 AimInput => aimInput;
        public bool CanAim => combatModeController == null || combatModeController.IsRangedMode;

        public event Action<bool> AimModeChanged;

        private void Awake()
        {
            if (combatModeController == null)
            {
                combatModeController = GetComponent<PlayerCombatModeController>();
            }

            if (movement == null)
            {
                movement = GetComponent<PlayerMovementController>();
            }
        }

        private void OnEnable()
        {
            actionEnabledHere = EnableActionIfNeeded(aimAction);
            if (combatModeController != null)
            {
                combatModeController.CombatModeChanged += HandleCombatModeChanged;
            }
        }

        private void OnDisable()
        {
            fireAimHeld = false;
            SetAimInput(Vector2.zero);
            SetAimMode(false);
            if (combatModeController != null)
            {
                combatModeController.CombatModeChanged -= HandleCombatModeChanged;
            }

            DisableActionIfOwned(aimAction, actionEnabledHere);
            actionEnabledHere = false;
        }

        private void Update()
        {
            if (!CanAim)
            {
                SetAimInput(Vector2.zero);
                SetAimMode(false);
                return;
            }

            if (holdToAim)
            {
                SetAimMode(ReadAimHeld());
                RequestAimFacingIfNeeded();
                return;
            }

            if (ReadAimPressed())
            {
                SetAimMode(!isAiming);
            }

            RequestAimFacingIfNeeded();
        }

        public void SetAimHeld(bool active)
        {
            mobileAimHeld = active;
            if (holdToAim)
            {
                SetAimMode(HasHeldAimInput() && CanAim);
            }
        }

        public void SetFireAimHeld(bool active)
        {
            fireAimHeld = active;
            if (holdToAim)
            {
                SetAimMode(HasHeldAimInput() && CanAim);
            }
        }

        public void SetAimInput(Vector2 input)
        {
            aimInput = CanAim ? Vector2.ClampMagnitude(input, 1f) : Vector2.zero;
            cameraController?.SetAimOrbitInput(aimInput);
        }

        public void QueueAimToggle()
        {
            queuedAimToggle = true;
        }

        public void SetAimMode(bool active)
        {
            bool resolvedActive = active && CanAim;
            if (isAiming == resolvedActive)
            {
                cameraController?.SetAimModifierActive(resolvedActive);
                if (!resolvedActive)
                {
                    SetAimInput(Vector2.zero);
                }

                return;
            }

            isAiming = resolvedActive;
            cameraController?.SetAimModifierActive(isAiming);
            if (!isAiming)
            {
                SetAimInput(Vector2.zero);
            }

            SetAnimatorBool(aimingParameter, isAiming);
            AimModeChanged?.Invoke(isAiming);
        }

        public void ConfigureReferences(
            PlayerCombatModeController newCombatModeController,
            ActionCameraController newCameraController,
            Animator newAnimator,
            PlayerMovementController newMovement = null)
        {
            combatModeController = newCombatModeController;
            cameraController = newCameraController;
            animator = newAnimator;
            if (newMovement != null)
            {
                movement = newMovement;
            }
        }

        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
            SetAnimatorBool(aimingParameter, isAiming);
        }

        private void HandleCombatModeChanged(PlayerCombatMode combatMode)
        {
            if (combatMode != PlayerCombatMode.Ranged)
            {
                SetAimMode(false);
            }
        }

        private bool ReadAimHeld()
        {
            bool held = HasHeldAimInput();
            if (aimAction != null && aimAction.action != null)
            {
                held |= aimAction.action.IsPressed();
            }

            if (held || !useDeviceFallbackWhenActionMissing || !IsActionMissing(aimAction))
            {
                return held;
            }

            return (allowMouseAimFallback
                    && Mouse.current != null
                    && Mouse.current.rightButton.isPressed)
                || (Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.5f);
        }

        private bool HasHeldAimInput()
        {
            return mobileAimHeld || fireAimHeld;
        }

        private bool ReadAimPressed()
        {
            bool pressed = queuedAimToggle;
            queuedAimToggle = false;

            if (aimAction != null && aimAction.action != null)
            {
                pressed |= aimAction.action.WasPressedThisFrame();
            }

            if (pressed || !useDeviceFallbackWhenActionMissing || !IsActionMissing(aimAction))
            {
                return pressed;
            }

            return (allowMouseAimFallback
                    && Mouse.current != null
                    && Mouse.current.rightButton.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.leftTrigger.wasPressedThisFrame);
        }

        private void RequestAimFacingIfNeeded()
        {
            if (!isAiming || !faceCameraForwardWhileAiming || movement == null || cameraController == null)
            {
                return;
            }

            Vector3 facingDirection = Vector3.ProjectOnPlane(cameraController.transform.forward, Vector3.up);
            if (facingDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            movement.RequestFacingDirection(
                facingDirection.normalized,
                aimingFacingHoldSeconds,
                snapAimingFacing);
        }

        private void SetAnimatorBool(string parameterName, bool value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
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
