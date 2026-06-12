using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class ActionCameraController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform threat;

        [Header("Follow")]
        [Tooltip("First-pass deviation: no collected camera distance default exists yet, so this remains Inspector-tunable.")]
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.05f, -4.2f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.2f, 0.55f);
        [Tooltip("First-pass deviation: no collected follow damping value exists yet, so this remains Inspector-tunable.")]
        [SerializeField, Min(0f)] private float followSmoothTime = 0.12f;
        [SerializeField, Min(0f)] private float rotationSmooth = 16f;

        [Header("Orbit")]
        [SerializeField] private InputActionReference orbitAction;
        [Tooltip("Shared mobile HUD drag hook for camera orbit. Mouse/right-stick fallback still works when no action is assigned.")]
        [SerializeField] private Vector2 mobileOrbitInput;
        [SerializeField] private bool useDeviceFallbackWhenActionMissing = true;
        [Tooltip("Base combat-camera yaw. Initialized from the authored scene camera position on first update.")]
        [SerializeField] private float orbitYawDegrees;
        [Tooltip("Manual orbit speed for stick or mobile drag input.")]
        [SerializeField, Min(0f)] private float manualYawSpeedDegrees = 150f;
        [Tooltip("Mouse drag sensitivity in degrees per pixel. Uses right mouse drag so left click can remain attack.")]
        [SerializeField, Min(0f)] private float mouseYawDegreesPerPixel = 0.12f;
        [Tooltip("Slowly recenters toward player facing when the player is not manually orbiting. Keeps ARPG readability without hard lock-on.")]
        [SerializeField, Range(0f, 1f)] private float targetYawAssist = 0.18f;
        [SerializeField, Min(0f)] private float targetYawAssistSpeed = 2.2f;
        [SerializeField, Range(0f, 0.5f)] private float orbitInputDeadZone = 0.08f;

        [Header("Threat Bias")]
        [Tooltip("Keeps current threat readable without becoming hard lock-on. This is intentionally inspectable for tuning.")]
        [SerializeField, Range(0f, 1f)] private float threatBias = 0.25f;
        [SerializeField, Min(0f)] private float maxThreatFocusOffset = 1.8f;
        [SerializeField, Min(0f)] private float maxLeadFromPlayerSpeed = 0.35f;

        [Header("Cue")]
        [Tooltip("Uses the collected perfect-dodge/camera-cue range around 0.20-0.32 seconds.")]
        [SerializeField, Min(0f)] private float defaultCueSeconds = 0.24f;
        [Tooltip("Keeps additive action cues bounded so camera emphasis cannot become a sticky cinematic lock.")]
        [SerializeField, Min(0f)] private float maxCueOffset = 0.55f;
        [SerializeField, Min(0f)] private float maxCueFieldOfViewDelta = 4f;
        [SerializeField, Min(0f)] private float maxCueCameraDistanceDelta = 0.45f;
        [SerializeField, Min(0f)] private float maxCueFocusHeightDelta = 0.25f;
        [SerializeField, Min(0f)] private float cueFieldOfViewSmooth = 18f;

        private Camera controlledCamera;
        private Vector3 followVelocity;
        private Vector3 cueOffset;
        private float cueFieldOfViewDelta;
        private float cueCameraDistanceDelta;
        private float cueFocusHeightDelta;
        private float cueTimer;
        private float cueDuration;
        private float baseFieldOfView;
        private bool orbitInitialized;
        private bool enabledOrbitAction;

        public bool HasActiveCue => cueTimer > 0f;
        public float OrbitYawDegrees => orbitYawDegrees;
        public Transform Target => target;
        public Transform Threat => threat;

        public void SetOrbitInput(Vector2 input)
        {
            mobileOrbitInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void ConfigureTargets(Transform newTarget, Transform newThreat)
        {
            target = newTarget;
            threat = newThreat;
        }

        public void RequestCue(Vector3 additiveOffset)
        {
            RequestCue(additiveOffset, defaultCueSeconds);
        }

        public void RequestCue(Vector3 additiveOffset, float durationSeconds)
        {
            RequestCue(additiveOffset, durationSeconds, 0f, 0f, 0f);
        }

        public void RequestCue(
            Vector3 additiveOffset,
            float durationSeconds,
            float fieldOfViewDelta,
            float cameraDistanceDelta,
            float focusHeightDelta)
        {
            cueOffset = Vector3.ClampMagnitude(additiveOffset, maxCueOffset);
            cueFieldOfViewDelta = Mathf.Clamp(fieldOfViewDelta, -maxCueFieldOfViewDelta, maxCueFieldOfViewDelta);
            cueCameraDistanceDelta = Mathf.Clamp(cameraDistanceDelta, -maxCueCameraDistanceDelta, maxCueCameraDistanceDelta);
            cueFocusHeightDelta = Mathf.Clamp(focusHeightDelta, -maxCueFocusHeightDelta, maxCueFocusHeightDelta);
            cueDuration = Mathf.Max(0.01f, durationSeconds);
            cueTimer = cueDuration;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            InitializeOrbitIfNeeded();
            UpdateOrbit(deltaTime);

            Quaternion orbitRotation = Quaternion.Euler(0f, orbitYawDegrees, 0f);
            float cueWeight = UpdateCueWeight(deltaTime);
            Vector3 focus = BuildFocusPoint() + Vector3.up * (cueFocusHeightDelta * cueWeight);
            Vector3 cueCameraOffset = Vector3.forward * (cueCameraDistanceDelta * cueWeight);
            Vector3 desiredPosition = focus + orbitRotation * (cameraOffset + cueCameraOffset) + cueOffset * cueWeight;
            UpdateFieldOfView(deltaTime, cueWeight);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref followVelocity,
                followSmoothTime);

            Vector3 lookDirection = focus - transform.position;
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            float rotationStep = 1f - Mathf.Exp(-rotationSmooth * deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationStep);
        }

        private void Awake()
        {
            controlledCamera = GetComponent<Camera>();
            baseFieldOfView = controlledCamera != null ? controlledCamera.fieldOfView : 50f;
        }

        private void OnEnable()
        {
            enabledOrbitAction = EnableActionIfNeeded(orbitAction);
        }

        private void OnDisable()
        {
            DisableActionIfOwned(orbitAction, enabledOrbitAction);
        }

        private Vector3 BuildFocusPoint()
        {
            Vector3 focus = target.position + lookOffset;

            if (threat != null)
            {
                Vector3 threatOffset = Vector3.ProjectOnPlane(threat.position - target.position, Vector3.up) * threatBias;
                focus += Vector3.ClampMagnitude(threatOffset, maxThreatFocusOffset);
            }

            Vector3 lead = Vector3.ProjectOnPlane(target.forward, Vector3.up) * maxLeadFromPlayerSpeed;
            return focus + lead;
        }

        private void InitializeOrbitIfNeeded()
        {
            if (orbitInitialized)
            {
                return;
            }

            orbitInitialized = true;
            Vector3 toCamera = Vector3.ProjectOnPlane(transform.position - target.position, Vector3.up);
            if (toCamera.sqrMagnitude <= 0.0001f)
            {
                orbitYawDegrees = target.eulerAngles.y;
                return;
            }

            Vector3 orbitForward = -toCamera.normalized;
            orbitYawDegrees = Mathf.Atan2(orbitForward.x, orbitForward.z) * Mathf.Rad2Deg;
        }

        private void UpdateOrbit(float deltaTime)
        {
            bool hasManualInput = false;
            Vector2 orbitInput = ApplyDeadZone(ReadOrbitInput());
            if (orbitInput.sqrMagnitude > 0f)
            {
                orbitYawDegrees += orbitInput.x * manualYawSpeedDegrees * deltaTime;
                hasManualInput = true;
            }

            Vector2 mouseDelta = ReadMouseOrbitDelta();
            if (mouseDelta.sqrMagnitude > 0f)
            {
                orbitYawDegrees += mouseDelta.x * mouseYawDegreesPerPixel;
                hasManualInput = true;
            }

            if (!hasManualInput)
            {
                ApplyTargetYawAssist(deltaTime);
            }

            orbitYawDegrees = NormalizeYaw(orbitYawDegrees);
        }

        private void ApplyTargetYawAssist(float deltaTime)
        {
            if (targetYawAssist <= 0f || targetYawAssistSpeed <= 0f)
            {
                return;
            }

            float assistedYaw = Mathf.LerpAngle(orbitYawDegrees, target.eulerAngles.y, targetYawAssist);
            float assistStep = 1f - Mathf.Exp(-targetYawAssistSpeed * deltaTime);
            orbitYawDegrees = Mathf.LerpAngle(orbitYawDegrees, assistedYaw, assistStep);
        }

        private Vector2 ReadOrbitInput()
        {
            Vector2 actionInput = Vector2.zero;
            if (orbitAction != null && orbitAction.action != null)
            {
                actionInput = orbitAction.action.ReadValue<Vector2>();
            }

            Vector2 input = mobileOrbitInput.sqrMagnitude > actionInput.sqrMagnitude ? mobileOrbitInput : actionInput;
            if (input.sqrMagnitude > 0f || !useDeviceFallbackWhenActionMissing || !IsActionMissing(orbitAction))
            {
                return Vector2.ClampMagnitude(input, 1f);
            }

            if (Gamepad.current == null)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(Gamepad.current.rightStick.ReadValue(), 1f);
        }

        private static Vector2 ReadMouseOrbitDelta()
        {
            if (Mouse.current == null || !Mouse.current.rightButton.isPressed)
            {
                return Vector2.zero;
            }

            return Mouse.current.delta.ReadValue();
        }

        private Vector2 ApplyDeadZone(Vector2 input)
        {
            if (input.sqrMagnitude < orbitInputDeadZone * orbitInputDeadZone)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(input, 1f);
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

        private static float NormalizeYaw(float yaw)
        {
            yaw %= 360f;
            return yaw < 0f ? yaw + 360f : yaw;
        }

        private float UpdateCueWeight(float deltaTime)
        {
            if (cueTimer <= 0f)
            {
                return 0f;
            }

            cueTimer = Mathf.Max(0f, cueTimer - deltaTime);
            float normalizedTime = cueDuration > 0f ? cueTimer / cueDuration : 0f;
            return Mathf.SmoothStep(0f, 1f, normalizedTime);
        }

        private void UpdateFieldOfView(float deltaTime, float cueWeight)
        {
            if (controlledCamera == null)
            {
                return;
            }

            float targetFieldOfView = baseFieldOfView + cueFieldOfViewDelta * cueWeight;
            float fovStep = 1f - Mathf.Exp(-cueFieldOfViewSmooth * deltaTime);
            controlledCamera.fieldOfView = Mathf.Lerp(controlledCamera.fieldOfView, targetFieldOfView, fovStep);
        }
    }
}
