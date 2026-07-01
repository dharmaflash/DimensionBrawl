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

        public Vector2 CurrentAimInput { get; private set; }
        public bool IsPointerHeld => pointerHeld;
        public bool IsKeyboardAimActive => keyboardAimActive;

        public void Configure(
            PlayerMovementController newMovementController,
            PlayerCombatModeController newCombatModeController,
            PlayerRangedAimController newAimController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction)
        {
            movementController = newMovementController;
            combatModeController = newCombatModeController;
            aimController = newAimController;
            rangedBasicAttackAction = newRangedBasicAttackAction;
        }

        private void OnDisable()
        {
            ReleasePointerAim();
            ApplyAim(Vector2.zero, holdAim: false);
        }

        private void Update()
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
            if (!CanAim())
            {
                return;
            }

            pointerHeld = true;
            keyboardAimActive = false;
            pointerAimInput = Vector2.zero;
            ApplyAim(pointerAimInput, holdAim: true);
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
            ApplyAim(resolvedInput, holdAim: true);
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
        }

        private Vector2 ResolveKeyboardPeekInput()
        {
            if (!keyboardPeekControlsAim || Keyboard.current == null || !CanAim())
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

            float x = 0f;
            if (Keyboard.current[keyboardPeekLeftKey] != null && Keyboard.current[keyboardPeekLeftKey].isPressed)
            {
                x -= 1f;
            }

            if (Keyboard.current[keyboardPeekRightKey] != null && Keyboard.current[keyboardPeekRightKey].isPressed)
            {
                x += 1f;
            }

            return new Vector2(x, 0f);
        }

        private void ApplyAim(Vector2 input, bool holdAim)
        {
            Vector2 resolvedInput = CanAim() ? Vector2.ClampMagnitude(input, 1f) : Vector2.zero;
            CurrentAimInput = resolvedInput;
            movementController?.SetLookInput(routeAimToMovementLook ? resolvedInput : Vector2.zero);
            rangedBasicAttackAction?.SetAimInput(resolvedInput);
            aimController?.SetAimInput(resolvedInput);
            aimController?.SetAimHeld(holdAim);
        }

        private bool CanAim()
        {
            return combatModeController == null || combatModeController.IsRangedMode;
        }
    }
}
