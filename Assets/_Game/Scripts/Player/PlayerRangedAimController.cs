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
        [SerializeField] private Key keyboardTestKey = Key.Q;

        [Header("References")]
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Animator animator;

        [Header("Animation")]
        [SerializeField] private string aimingParameter = "IsAiming";

        private bool actionEnabledHere;
        private bool mobileAimHeld;
        private bool queuedAimToggle;
        private bool isAiming;

        public bool IsAiming => isAiming;
        public bool CanAim => combatModeController == null || combatModeController.IsRangedMode;

        public event Action<bool> AimModeChanged;

        private void Awake()
        {
            if (combatModeController == null)
            {
                combatModeController = GetComponent<PlayerCombatModeController>();
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
                SetAimMode(false);
                return;
            }

            if (holdToAim)
            {
                SetAimMode(ReadAimHeld());
                return;
            }

            if (ReadAimPressed())
            {
                SetAimMode(!isAiming);
            }
        }

        public void SetAimHeld(bool active)
        {
            mobileAimHeld = active;
            if (holdToAim)
            {
                SetAimMode(active && CanAim);
            }
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
                return;
            }

            isAiming = resolvedActive;
            cameraController?.SetAimModifierActive(isAiming);
            SetAnimatorBool(aimingParameter, isAiming);
            AimModeChanged?.Invoke(isAiming);
        }

        public void ConfigureReferences(
            PlayerCombatModeController newCombatModeController,
            ActionCameraController newCameraController,
            Animator newAnimator)
        {
            combatModeController = newCombatModeController;
            cameraController = newCameraController;
            animator = newAnimator;
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
            bool held = mobileAimHeld;
            if (aimAction != null && aimAction.action != null)
            {
                held |= aimAction.action.IsPressed();
            }

            if (held || !useDeviceFallbackWhenActionMissing || !IsActionMissing(aimAction))
            {
                return held;
            }

            return IsKeyboardHeld()
                || (Mouse.current != null && Mouse.current.rightButton.isPressed)
                || (Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.5f);
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

            return IsKeyboardPressed()
                || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.leftTrigger.wasPressedThisFrame);
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
