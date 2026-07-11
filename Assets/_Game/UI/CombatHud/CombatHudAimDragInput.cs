using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class CombatHudAimDragInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedAimController aimController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private bool routeAimToMovementLook;
        [SerializeField] private bool keyboardPeekControlsAim = true;
        [SerializeField] private bool keyboardPeekRequiresActiveAim = true;
        [SerializeField] private Key keyboardPeekLeftKey = Key.Q;
        [SerializeField] private Key keyboardPeekRightKey = Key.E;
        [SerializeField, Range(0f, 0.5f)] private float dragDeadZone = 0.08f;
        [SerializeField, Min(0.0001f)] private float dragSensitivity = 0.00435f;

        private bool pointerHeld;
        private bool keyboardAimActive;
        private Vector2 pointerAimInput;
        private Vector2 keyboardPeekInput;
        private InputAction keyboardPeekAction;
        private PlayerCombatModeController subscribedCombatModeController;
        private PlayerRangedAimController subscribedAimController;
        private PlayerRangedBasicAttackAction subscribedRangedBasicAttackAction;

        public Vector2 CurrentAimInput { get; private set; }
        public bool IsPointerHeld => pointerHeld;
        public bool IsKeyboardAimActive => keyboardAimActive;

        public void Configure(
            PlayerMovementController newMovementController,
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newAimController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction)
        {
            bool rebindStateEvents = isActiveAndEnabled;
            if (rebindStateEvents)
            {
                UnsubscribeStateEvents();
            }

            movementController = newMovementController;
            combatModeController = newCombatModeController;
            aimController = newAimController;
            rangedBasicAttackAction = newRangedBasicAttackAction;

            if (rebindStateEvents)
            {
                SubscribeStateEvents();
                RefreshKeyboardAim();
            }
        }

        private void OnEnable()
        {
            SubscribeStateEvents();
            EnableKeyboardPeekAction();
            RefreshKeyboardAim();
        }

        private void OnDisable()
        {
            DisableKeyboardPeekAction();
            UnsubscribeStateEvents();
            keyboardPeekInput = Vector2.zero;
            keyboardAimActive = false;
            ReleasePointerAim();
            ApplyAim(Vector2.zero, holdAim: false);
        }

        private void RefreshKeyboardAim()
        {
            if (pointerHeld)
            {
                return;
            }

            Vector2 keyboardInput = ResolveKeyboardPeekInput();
            keyboardAimActive = keyboardInput.sqrMagnitude > 0.0001f;
            if (keyboardAimActive || CurrentAimInput.sqrMagnitude > 0.0001f)
            {
                ApplyAim(keyboardInput, holdAim: false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerHeld = true;
            keyboardAimActive = false;
            pointerAimInput = Vector2.zero;
            ApplyPointerDrag(pointerAimInput);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!pointerHeld || eventData == null)
            {
                return;
            }

            pointerAimInput = Vector2.ClampMagnitude(
                pointerAimInput + new Vector2(eventData.delta.x, -eventData.delta.y) * dragSensitivity,
                1f);
            Vector2 resolvedInput = pointerAimInput.sqrMagnitude >= dragDeadZone * dragDeadZone
                ? pointerAimInput
                : Vector2.zero;
            ApplyPointerDrag(resolvedInput);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleasePointerAim();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ReleasePointerAim();
        }

        private void ReleasePointerAim()
        {
            if (!pointerHeld)
            {
                return;
            }

            pointerHeld = false;
            pointerAimInput = Vector2.zero;
            ApplyAim(Vector2.zero, holdAim: false);
            RefreshKeyboardAim();
        }

        private void ApplyPointerDrag(Vector2 input)
        {
            Vector2 resolvedInput = Vector2.ClampMagnitude(input, 1f);
            if (ShouldRoutePointerDragToAim())
            {
                ApplyAim(resolvedInput, holdAim: true);
                return;
            }

            CurrentAimInput = Vector2.zero;
            if (movementController != null)
            {
                movementController.SetLookInput(resolvedInput);
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetAimInput(Vector2.zero);
            }

            if (aimController != null)
            {
                aimController.SetAimInput(Vector2.zero);
                aimController.SetAimHeld(false);
            }
        }

        private Vector2 ResolveKeyboardPeekInput()
        {
            if (!keyboardPeekControlsAim || !CanAim())
            {
                return Vector2.zero;
            }

            if (keyboardPeekRequiresActiveAim
                && aimController != null
                && !aimController.IsAiming
                && (rangedBasicAttackAction == null || !rangedBasicAttackAction.IsAimPreviewActive))
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(keyboardPeekInput, 1f);
        }

        private void ApplyAim(Vector2 input, bool holdAim)
        {
            Vector2 resolvedInput = CanAim() ? Vector2.ClampMagnitude(input, 1f) : Vector2.zero;
            CurrentAimInput = resolvedInput;
            if (movementController != null)
            {
                movementController.SetLookInput(routeAimToMovementLook ? resolvedInput : Vector2.zero);
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetAimInput(resolvedInput);
            }

            if (aimController != null)
            {
                aimController.SetAimInput(resolvedInput);
                aimController.SetAimHeld(holdAim);
            }
        }

        private bool CanAim()
        {
            return combatModeController == null || combatModeController.IsRangedMode;
        }

        private bool ShouldRoutePointerDragToAim()
        {
            return CanAim()
                && rangedBasicAttackAction != null
                && rangedBasicAttackAction.IsFireHeld;
        }

        private void EnableKeyboardPeekAction()
        {
            if (!keyboardPeekControlsAim
                || keyboardPeekAction != null
                || Application.isMobilePlatform
                || Keyboard.current == null)
            {
                return;
            }

            string leftPath = ResolveKeyboardControlPath(keyboardPeekLeftKey);
            string rightPath = ResolveKeyboardControlPath(keyboardPeekRightKey);
            if (string.IsNullOrEmpty(leftPath) && string.IsNullOrEmpty(rightPath))
            {
                return;
            }

            keyboardPeekAction = new InputAction(
                $"{nameof(CombatHudAimDragInput)} Keyboard Peek",
                InputActionType.Value,
                expectedControlType: "Axis");
            var composite = keyboardPeekAction.AddCompositeBinding("1DAxis");
            if (!string.IsNullOrEmpty(leftPath))
            {
                composite.With("Negative", leftPath);
            }

            if (!string.IsNullOrEmpty(rightPath))
            {
                composite.With("Positive", rightPath);
            }

            keyboardPeekAction.performed += HandleKeyboardPeekChanged;
            keyboardPeekAction.canceled += HandleKeyboardPeekChanged;
            keyboardPeekAction.Enable();
        }

        private void DisableKeyboardPeekAction()
        {
            if (keyboardPeekAction == null)
            {
                return;
            }

            keyboardPeekAction.performed -= HandleKeyboardPeekChanged;
            keyboardPeekAction.canceled -= HandleKeyboardPeekChanged;
            keyboardPeekAction.Disable();
            keyboardPeekAction.Dispose();
            keyboardPeekAction = null;
        }

        private void HandleKeyboardPeekChanged(InputAction.CallbackContext context)
        {
            keyboardPeekInput = new Vector2(context.ReadValue<float>(), 0f);
            RefreshKeyboardAim();
        }

        private void SubscribeStateEvents()
        {
            UnsubscribeStateEvents();
            if (combatModeController != null)
            {
                subscribedCombatModeController = combatModeController;
                subscribedCombatModeController.CombatModeChanged += HandleCombatModeChanged;
            }

            if (aimController != null)
            {
                subscribedAimController = aimController;
                subscribedAimController.AimModeChanged += HandleAimModeChanged;
            }

            if (rangedBasicAttackAction != null)
            {
                subscribedRangedBasicAttackAction = rangedBasicAttackAction;
                subscribedRangedBasicAttackAction.AimPreviewStateChanged += HandleAimPreviewStateChanged;
            }
        }

        private void UnsubscribeStateEvents()
        {
            if (subscribedCombatModeController != null)
            {
                subscribedCombatModeController.CombatModeChanged -= HandleCombatModeChanged;
            }

            if (subscribedAimController != null)
            {
                subscribedAimController.AimModeChanged -= HandleAimModeChanged;
            }

            if (subscribedRangedBasicAttackAction != null)
            {
                subscribedRangedBasicAttackAction.AimPreviewStateChanged -= HandleAimPreviewStateChanged;
            }

            subscribedCombatModeController = null;
            subscribedAimController = null;
            subscribedRangedBasicAttackAction = null;
        }

        private void HandleCombatModeChanged(PlayerCombatMode combatMode)
        {
            RefreshKeyboardAim();
        }

        private void HandleAimModeChanged(bool isAiming)
        {
            RefreshKeyboardAim();
        }

        private void HandleAimPreviewStateChanged()
        {
            RefreshKeyboardAim();
        }

        private static string ResolveKeyboardControlPath(Key key)
        {
            if (key == Key.None || Keyboard.current == null)
            {
                return null;
            }

            return Keyboard.current[key]?.path;
        }
    }
}
