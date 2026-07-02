using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class CombatHudVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private RectTransform knob;
        [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.08f;
        [SerializeField, Range(0.05f, 1f)] private float knobTravelRatio = 0.34f;

        private RectTransform rectTransform;
        private Vector2 knobRestPosition;
        private bool hasKnobRestPosition;
        private bool pointerHeld;

        public Vector2 CurrentInput { get; private set; }

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
            ClearInput();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerHeld = true;
            UpdateInput(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!pointerHeld)
            {
                return;
            }

            UpdateInput(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ClearInput();
        }

        private void UpdateInput(PointerEventData eventData)
        {
            RectTransform joystickRect = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
            if (joystickRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    joystickRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            float radius = Mathf.Max(1f, Mathf.Min(joystickRect.rect.width, joystickRect.rect.height) * 0.5f);
            Vector2 input = Vector2.ClampMagnitude(localPoint / radius, 1f);
            if (input.sqrMagnitude < deadZone * deadZone)
            {
                input = Vector2.zero;
            }

            CurrentInput = input;
            movementController?.SetMoveInput(input);
            MoveKnob(input, radius);
        }

        private void MoveKnob(Vector2 input, float radius)
        {
            if (knob == null)
            {
                return;
            }

            CaptureKnobRestPosition();
            knob.anchoredPosition = knobRestPosition + input * radius * knobTravelRatio;
        }

        private void ClearInput()
        {
            pointerHeld = false;
            CurrentInput = Vector2.zero;
            movementController?.SetMoveInput(Vector2.zero);
            ResetKnob();
        }

        private void ResetKnob()
        {
            if (knob == null || !hasKnobRestPosition)
            {
                return;
            }

            knob.anchoredPosition = knobRestPosition;
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
