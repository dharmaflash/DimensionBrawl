using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class CombatHudVirtualJoystick : MonoBehaviour, ICombatMoveInputGate, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointerId = int.MinValue;

        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private RectTransform knob;
        [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.08f;
        [SerializeField, Range(0.25f, 1f)] private float inputRadiusRatio = 0.62f;
        [SerializeField, Range(0.05f, 1f)] private float knobTravelRatio = 0.72f;

        private const float MaximumResponsiveDeadZone = 0.05f;
        private const float MinimumResponsiveKnobTravelRatio = 0.68f;

        private RectTransform rectTransform;
        private Vector2 knobRestPosition;
        private bool hasKnobRestPosition;
        private bool pointerHeld;
        private PlayerInputLockSource inputBlockSources;
        private int activePointerId = NoPointerId;
        private int deferredPointerId = NoPointerId;
        private Vector2 deferredPointerPosition;
        private Camera deferredEventCamera;

        public Vector2 CurrentInput { get; private set; }
        public bool IsPointerHeld => pointerHeld;
        public bool IsInputBlocked => inputBlockSources != PlayerInputLockSource.None;

        public void Configure(PlayerMovementController newMovementController, RectTransform newKnob)
        {
            movementController = newMovementController;
            knob = newKnob;
            if (knob == null)
            {
                hasKnobRestPosition = false;
                return;
            }

            knobRestPosition = knob.anchoredPosition;
            hasKnobRestPosition = true;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            CaptureKnobRestPosition();
        }

        private void OnEnable()
        {
            CaptureKnobRestPosition();
        }

        private void OnDisable()
        {
            CancelPointerInput();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerHeld
                || eventData == null
                || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (IsInputBlocked)
            {
                CaptureDeferredPointer(eventData);
                return;
            }

            BeginPointerInput(eventData.pointerId, eventData.position, eventData.pressEventCamera);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            if (IsInputBlocked)
            {
                if (eventData.pointerId == deferredPointerId)
                {
                    deferredPointerPosition = eventData.position;
                    deferredEventCamera = eventData.pressEventCamera;
                }

                return;
            }

            if (IsActivePointer(eventData))
            {
                UpdateInput(eventData.position, eventData.pressEventCamera);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData != null && eventData.pointerId == deferredPointerId)
            {
                ClearDeferredPointer();
                return;
            }

            if (!IsActivePointer(eventData))
            {
                return;
            }

            ClearInput();
        }

        public void RefreshRestPosition()
        {
            if (knob == null)
            {
                hasKnobRestPosition = false;
                return;
            }

            knobRestPosition = knob.anchoredPosition;
            hasKnobRestPosition = true;
        }

        public void SetInputBlocked(PlayerInputLockSource source, bool blocked)
        {
            bool wasBlocked = IsInputBlocked;
            inputBlockSources = PlayerInputLockMask.WithState(inputBlockSources, source, blocked);
            if (wasBlocked == IsInputBlocked)
            {
                return;
            }

            if (IsInputBlocked)
            {
                CancelPointerInput();
                return;
            }

            ActivateDeferredPointer();
        }

        private void UpdateInput(Vector2 screenPosition, Camera eventCamera)
        {
            if (IsInputBlocked)
            {
                return;
            }

            RectTransform joystickRect = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
            if (joystickRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    joystickRect,
                    screenPosition,
                    eventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            float visualRadius = Mathf.Max(1f, Mathf.Min(joystickRect.rect.width, joystickRect.rect.height) * 0.5f);
            float inputRadius = Mathf.Max(1f, visualRadius * Mathf.Clamp(inputRadiusRatio, 0.25f, 1f));
            Vector2 centeredPoint = localPoint - joystickRect.rect.center;
            Vector2 input = Vector2.ClampMagnitude(centeredPoint / inputRadius, 1f);
            float responsiveDeadZone = Mathf.Min(deadZone, MaximumResponsiveDeadZone);
            if (input.sqrMagnitude < responsiveDeadZone * responsiveDeadZone)
            {
                input = Vector2.zero;
            }

            CurrentInput = input;
            if (movementController != null)
            {
                movementController.SetMoveInput(input);
            }
            MoveKnob(input, visualRadius);
        }

        private void MoveKnob(Vector2 input, float radius)
        {
            if (knob == null)
            {
                return;
            }

            CaptureKnobRestPosition();
            float responsiveTravel = Mathf.Max(knobTravelRatio, MinimumResponsiveKnobTravelRatio);
            knob.anchoredPosition = knobRestPosition + input * radius * responsiveTravel;
        }

        private void ClearInput()
        {
            pointerHeld = false;
            activePointerId = NoPointerId;
            CurrentInput = Vector2.zero;
            if (movementController != null)
            {
                movementController.SetMoveInput(Vector2.zero);
            }
            ResetKnob();
        }

        private void BeginPointerInput(int pointerId, Vector2 screenPosition, Camera eventCamera)
        {
            pointerHeld = true;
            activePointerId = pointerId;
            UpdateInput(screenPosition, eventCamera);
        }

        private void CaptureDeferredPointer(PointerEventData eventData)
        {
            if (deferredPointerId != NoPointerId && deferredPointerId != eventData.pointerId)
            {
                return;
            }

            deferredPointerId = eventData.pointerId;
            deferredPointerPosition = eventData.position;
            deferredEventCamera = eventData.pressEventCamera;
        }

        private void ActivateDeferredPointer()
        {
            if (deferredPointerId == NoPointerId || pointerHeld)
            {
                return;
            }

            int pointerId = deferredPointerId;
            Vector2 screenPosition = deferredPointerPosition;
            Camera eventCamera = deferredEventCamera;
            ClearDeferredPointer();
            BeginPointerInput(pointerId, screenPosition, eventCamera);
        }

        private void CancelPointerInput()
        {
            ClearDeferredPointer();
            ClearInput();
        }

        private void ClearDeferredPointer()
        {
            deferredPointerId = NoPointerId;
            deferredPointerPosition = Vector2.zero;
            deferredEventCamera = null;
        }

        private void ResetKnob()
        {
            if (knob == null || !hasKnobRestPosition)
            {
                return;
            }

            knob.anchoredPosition = knobRestPosition;
        }

        private bool IsActivePointer(PointerEventData eventData)
        {
            return pointerHeld
                && eventData != null
                && eventData.pointerId == activePointerId;
        }

        private void CaptureKnobRestPosition()
        {
            if (knob == null || hasKnobRestPosition)
            {
                return;
            }

            knobRestPosition = knob.anchoredPosition;
            hasKnobRestPosition = true;
        }
    }
}
