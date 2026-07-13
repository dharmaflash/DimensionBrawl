using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsekaiBrawl.Gameplay
{
    public class BattleCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 6.4f, -10.8f);
        [SerializeField] private Vector3 lookAhead = new(0f, 1.25f, 9.8f);
        [SerializeField] private Vector3 overviewOffset = new(0f, 20f, -6f);
        [SerializeField] private Vector3 overviewLookAhead = new(0f, 0.6f, 18f);
        [SerializeField] private float smoothSpeed = 6.5f;
        [SerializeField] private float lookAtSmoothSpeed = 10f;
        [SerializeField] private float minX = -3.8f;
        [SerializeField] private float maxX = 3.8f;
        [SerializeField] private float overviewPanSpeed = 18f;
        [SerializeField] private float overviewDragSensitivity = 0.045f;
        [SerializeField] private float overviewZoomSpeed = 2.6f;
        [SerializeField] private float mobileOverviewZoomStep = 1.8f;

        private bool isOverviewMode;
        private Vector3 overviewFocusPosition;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public bool IsOverviewMode => isOverviewMode;
        public Vector3 CurrentFocusPosition { get; private set; }
        public float EstimatedVisibleLength { get; private set; }
        public float EstimatedVisibleWidth { get; private set; }

        public void ConfigureHorizontalBounds(float halfWidth)
        {
            minX = -Mathf.Max(2.8f, halfWidth * 0.68f);
            maxX = Mathf.Max(2.8f, halfWidth * 0.68f);
        }

        public void ConfigureOffset(Vector3 desiredOffset)
        {
            offset = desiredOffset;
        }

        public void ConfigureLookAhead(Vector3 desiredLookAhead)
        {
            lookAhead = desiredLookAhead;
        }

        private void Update()
        {
            HandleOverviewInput();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float laneLength = BattleManager.Instance != null ? Mathf.Max(1f, BattleManager.Instance.LaneLength) : 48f;
            float laneHalfWidth = BattleManager.Instance != null ? BattleManager.Instance.LaneHalfWidth : Mathf.Max(4f, maxX);
            Vector3 focusPosition;
            Vector3 desiredPosition;
            Vector3 lookTarget;

            if (isOverviewMode)
            {
                overviewFocusPosition = ClampOverviewFocus(overviewFocusPosition, laneHalfWidth, laneLength);
                focusPosition = overviewFocusPosition;
                desiredPosition = overviewFocusPosition + overviewOffset;
                lookTarget = overviewFocusPosition + overviewLookAhead;
                EstimatedVisibleLength = Mathf.Clamp(laneLength * 0.42f, 18f, laneLength);
                EstimatedVisibleWidth = Mathf.Clamp(laneHalfWidth * 1.6f, 6f, laneHalfWidth * 2f);
            }
            else
            {
                float forwardProgress = Mathf.Clamp01(target.position.z / laneLength);
                Vector3 dynamicOffset = offset + new Vector3(0f, forwardProgress * 1.35f, -forwardProgress * 2.65f);
                Vector3 dynamicLookAhead = lookAhead + new Vector3(0f, forwardProgress * 0.4f, forwardProgress * 5.4f);
                focusPosition = target.position;
                desiredPosition = target.position + dynamicOffset;
                lookTarget = target.position + dynamicLookAhead;
                EstimatedVisibleLength = Mathf.Lerp(18f, 26f, forwardProgress);
                EstimatedVisibleWidth = Mathf.Clamp(laneHalfWidth * 1.05f, 5.5f, laneHalfWidth * 1.45f);
            }

            CurrentFocusPosition = focusPosition;
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX - 1.2f, maxX + 1.2f);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, -14f, laneLength + 6f);
            desiredPosition += CameraShake.Instance != null ? CameraShake.Instance.CurrentOffset : Vector3.zero;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSmoothSpeed * Time.deltaTime);
        }

        private void HandleOverviewInput()
        {
            if (target == null)
            {
                return;
            }

            if (MobileBattleControls.ConsumeMapTogglePressed())
            {
                ToggleOverviewMode();
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                ToggleOverviewMode();
            }

            if (!isOverviewMode)
            {
                return;
            }

            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            Vector2 panInput = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.jKey.isPressed)
                {
                    panInput.x -= 1f;
                }

                if (Keyboard.current.lKey.isPressed)
                {
                    panInput.x += 1f;
                }

                if (Keyboard.current.kKey.isPressed)
                {
                    panInput.y -= 1f;
                }

                if (Keyboard.current.iKey.isPressed)
                {
                    panInput.y += 1f;
                }
            }

            if (panInput.sqrMagnitude > 0.001f)
            {
                Vector2 clamped = Vector2.ClampMagnitude(panInput, 1f);
                overviewFocusPosition += new Vector3(clamped.x, 0f, clamped.y) * (overviewPanSpeed * deltaTime);
            }

            if (Mouse.current != null)
            {
                bool pointerOverUi = EventSystem.current != null
                    && EventSystem.current.IsPointerOverGameObject();
                if (!pointerOverUi
                    && (Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed))
                {
                    Vector2 dragDelta = Mouse.current.delta.ReadValue();
                    overviewFocusPosition += new Vector3(-dragDelta.x, 0f, -dragDelta.y) * overviewDragSensitivity;
                }

                float scrollValue = Mouse.current.scroll.ReadValue().y;
                if (!pointerOverUi && Mathf.Abs(scrollValue) > 0.01f)
                {
                    ApplyOverviewZoom(scrollValue * 0.01f);
                }
            }

            if (MobileBattleControls.TryConsumeOverviewDrag(out Vector2 overviewDragDelta))
            {
                overviewFocusPosition += new Vector3(-overviewDragDelta.x, 0f, -overviewDragDelta.y) * overviewDragSensitivity;
            }

            if (MobileBattleControls.TryConsumeOverviewZoomStep(out float zoomStep))
            {
                ApplyOverviewZoom(zoomStep * mobileOverviewZoomStep);
            }

            if (MobileBattleControls.ConsumeOverviewCenterPressed())
            {
                RecenterOverviewOnTarget();
            }
#else
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleOverviewMode();
            }

            if (!isOverviewMode)
            {
                return;
            }

            Vector3 panDirection = new(
                (Input.GetKey(KeyCode.L) ? 1f : 0f) - (Input.GetKey(KeyCode.J) ? 1f : 0f),
                0f,
                (Input.GetKey(KeyCode.I) ? 1f : 0f) - (Input.GetKey(KeyCode.K) ? 1f : 0f));
            overviewFocusPosition += Vector3.ClampMagnitude(panDirection, 1f) * (overviewPanSpeed * Time.unscaledDeltaTime);
#endif
        }

        private void ToggleOverviewMode()
        {
            isOverviewMode = !isOverviewMode;
            overviewFocusPosition = target != null ? target.position + new Vector3(0f, 0f, 10f) : overviewFocusPosition;
        }

        private void ApplyOverviewZoom(float delta)
        {
            overviewOffset.y = Mathf.Clamp(overviewOffset.y - (delta * overviewZoomSpeed), 14f, 30f);
            overviewLookAhead.z = Mathf.Clamp(overviewLookAhead.z + (delta * 2f), 14f, 24f);
        }

        private void RecenterOverviewOnTarget()
        {
            overviewFocusPosition = target != null ? target.position + new Vector3(0f, 0f, 10f) : overviewFocusPosition;
        }

        private static Vector3 ClampOverviewFocus(Vector3 focusPosition, float laneHalfWidth, float laneLength)
        {
            focusPosition.x = Mathf.Clamp(focusPosition.x, -laneHalfWidth, laneHalfWidth);
            focusPosition.z = Mathf.Clamp(focusPosition.z, 0f, laneLength);
            return focusPosition;
        }
    }
}
