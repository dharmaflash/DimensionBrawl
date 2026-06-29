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

        [Header("Fixed Rear")]
        [Tooltip("Pins yaw to a stable authored rear reference for fixed-rear review scenes instead of deriving orbit from the current camera pose.")]
        [SerializeField] private bool useFixedRearYaw;
        [SerializeField] private Transform fixedRearYawReference;
        [SerializeField] private float fixedRearYawOffsetDegrees;

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

        [Header("Aim Mode")]
        [Tooltip("Persistent ranged-aim shoulder offset. This is a mode modifier, not a short combat cue.")]
        [SerializeField] private Vector3 aimCameraOffset = new Vector3(0.5f, 0.18f, 0.12f);
        [SerializeField] private Vector3 aimFocusOffset = new Vector3(0.5f, 0.06f, 1.05f);
        [SerializeField] private float aimFieldOfViewDelta = -5.5f;
        [SerializeField, Min(0f)] private float aimBlendInSpeed = 14f;
        [SerializeField, Min(0f)] private float aimBlendOutSpeed = 18f;
        [Tooltip("Tightens positional follow while aiming so the zoomed shoulder view feels attached to the player.")]
        [SerializeField, Min(0f)] private float aimFollowSmoothTime = 0.025f;
        [Tooltip("Lets Look/TargetBias peek the fixed-rear aim camera without enabling free orbit.")]
        [SerializeField] private bool aimOrbitUsesInput = true;
        [Tooltip("Keeps the last aim-camera peek while ranged aim/fire is held, then returns when aim ends.")]
        [SerializeField] private bool aimOrbitHoldsYawUntilAimEnds = true;
        [Tooltip("Moves the shoulder camera position with aim peek so the player stays anchored like a linked TPS rig.")]
        [SerializeField] private bool aimOrbitRotatesCameraPosition;
        [SerializeField, Range(0f, 90f)] private float aimOrbitYawLimitDegrees = 45f;
        [SerializeField, Min(0f)] private float aimOrbitYawSpeedDegrees = 360f;
        [SerializeField, Min(0f)] private float aimOrbitReturnSpeedDegrees = 420f;

        [Header("Aim Assist")]
        [SerializeField] private bool aimAssistUsesYawTarget = true;
        [SerializeField, Range(0f, 1f)] private float aimAssistMaxYawBlend = 0.85f;
        [SerializeField, Min(0f)] private float aimAssistYawSpeedDegrees = 420f;
        [SerializeField, Min(0f)] private float aimAssistYawReturnSpeedDegrees = 520f;

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
        private float aimTargetWeight;
        private float aimWeight;
        private Vector2 aimOrbitInput;
        private float aimYawOffsetDegrees;
        private bool hasAimAssistYawTarget;
        private float requestedAimAssistYawTargetDegrees;
        private float requestedAimAssistStrength01;
        private float aimAssistYawOffsetDegrees;
        private bool wasAimFollowActive;

        public bool HasActiveCue => cueTimer > 0f;
        public bool IsAimModifierActive => aimTargetWeight > 0.5f;
        public float AimWeight => aimWeight;
        public Vector2 AimOrbitInput => aimOrbitInput;
        public float AimYawOffsetDegrees => aimYawOffsetDegrees;
        public float AimAssistYawOffsetDegrees => aimAssistYawOffsetDegrees;
        public float TotalAimYawOffsetDegrees => ResolveTotalAimYawOffset();
        public float OrbitYawDegrees => orbitYawDegrees;
        public Transform Target => target;
        public Transform Threat => threat;

        public Vector3 GetAimPlanarForward()
        {
            float resolvedYaw = NormalizeYaw(orbitYawDegrees + ResolveTotalAimYawOffset());
            return Quaternion.Euler(0f, resolvedYaw, 0f) * Vector3.forward;
        }

        public bool TryGetViewportAimRay(Vector2 viewportPoint, out Ray ray)
        {
            Camera camera = ResolveControlledCamera();
            if (camera == null)
            {
                ray = default;
                return false;
            }

            Vector2 clampedPoint = new Vector2(
                Mathf.Clamp01(viewportPoint.x),
                Mathf.Clamp01(viewportPoint.y));
            ray = camera.ViewportPointToRay(new Vector3(clampedPoint.x, clampedPoint.y, 0f));
            return true;
        }

        public bool TryWorldToViewportPoint(Vector3 worldPoint, out Vector3 viewportPoint)
        {
            Camera camera = ResolveControlledCamera();
            if (camera == null)
            {
                viewportPoint = default;
                return false;
            }

            viewportPoint = camera.WorldToViewportPoint(worldPoint);
            return viewportPoint.z > 0f;
        }

        public void SetOrbitInput(Vector2 input)
        {
            mobileOrbitInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void SetAimOrbitInput(Vector2 input)
        {
            aimOrbitInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void RequestAimAssistYawTarget(float targetYawOffsetDegrees, float strength01)
        {
            if (!aimAssistUsesYawTarget || strength01 <= 0f)
            {
                return;
            }

            float resolvedStrength = Mathf.Clamp01(strength01);
            if (hasAimAssistYawTarget && resolvedStrength < requestedAimAssistStrength01)
            {
                return;
            }

            hasAimAssistYawTarget = true;
            requestedAimAssistYawTargetDegrees = Mathf.Clamp(
                Mathf.DeltaAngle(0f, targetYawOffsetDegrees),
                -aimOrbitYawLimitDegrees,
                aimOrbitYawLimitDegrees);
            requestedAimAssistStrength01 = resolvedStrength;
        }

        public void ConfigureTargets(Transform newTarget, Transform newThreat)
        {
            target = newTarget;
            threat = newThreat;
        }

        public void PrimeFromHandoffPose(Transform handoffPose)
        {
            if (handoffPose == null)
            {
                return;
            }

            PrimeFromHandoffPose(handoffPose.position, handoffPose.rotation, null);
        }

        public void PrimeFromHandoffCamera(Camera handoffCamera)
        {
            if (handoffCamera == null)
            {
                return;
            }

            PrimeFromHandoffPose(
                handoffCamera.transform.position,
                handoffCamera.transform.rotation,
                handoffCamera.fieldOfView);
        }

        private void PrimeFromHandoffPose(
            Vector3 handoffPosition,
            Quaternion handoffRotation,
            float? handoffFieldOfView)
        {
            transform.SetPositionAndRotation(handoffPosition, handoffRotation);
            if (handoffFieldOfView.HasValue)
            {
                Camera camera = ResolveControlledCamera();
                if (camera != null)
                {
                    camera.fieldOfView = handoffFieldOfView.Value;
                    baseFieldOfView = handoffFieldOfView.Value;
                }
            }

            followVelocity = Vector3.zero;
            cueOffset = Vector3.zero;
            cueFieldOfViewDelta = 0f;
            cueCameraDistanceDelta = 0f;
            cueFocusHeightDelta = 0f;
            cueTimer = 0f;
            cueDuration = 0f;
            orbitInitialized = false;
            aimTargetWeight = 0f;
            aimWeight = 0f;
            aimOrbitInput = Vector2.zero;
            aimYawOffsetDegrees = 0f;
            hasAimAssistYawTarget = false;
            requestedAimAssistYawTargetDegrees = 0f;
            requestedAimAssistStrength01 = 0f;
            aimAssistYawOffsetDegrees = 0f;
            wasAimFollowActive = false;
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

        public void SetAimModifierActive(bool active)
        {
            aimTargetWeight = active ? 1f : 0f;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (useFixedRearYaw)
            {
                orbitYawDegrees = ResolveFixedRearYaw();
            }
            else
            {
                InitializeOrbitIfNeeded();
                UpdateOrbit(deltaTime);
            }

            float cueWeight = UpdateCueWeight(deltaTime);
            UpdateAimWeight(deltaTime);
            UpdateAimYawOffset(deltaTime);
            UpdateAimAssistYawOffset(deltaTime);
            float totalAimYawOffsetDegrees = ResolveTotalAimYawOffset();
            Quaternion baseRotation = Quaternion.Euler(
                0f,
                NormalizeYaw(orbitYawDegrees),
                0f);
            Quaternion aimRotation = Quaternion.Euler(
                0f,
                NormalizeYaw(orbitYawDegrees + totalAimYawOffsetDegrees),
                0f);
            Quaternion cameraPositionRotation = aimOrbitRotatesCameraPosition ? aimRotation : baseRotation;
            Vector3 baseFocus = BuildFocusPoint() + Vector3.up * (cueFocusHeightDelta * cueWeight);
            Vector3 cueCameraOffset = Vector3.forward * (cueCameraDistanceDelta * cueWeight);
            Vector3 basePosition = baseFocus
                + baseRotation * (cameraOffset + cueCameraOffset)
                + cueOffset * cueWeight;
            Vector3 aimFocus = baseFocus
                + (aimOrbitRotatesCameraPosition ? aimRotation : baseRotation) * (aimFocusOffset * aimWeight);
            Vector3 desiredPosition;
            Vector3 focus;
            if (aimOrbitRotatesCameraPosition)
            {
                BuildAimRigPose(
                    aimRotation,
                    cueCameraOffset,
                    cueWeight,
                    cueFocusHeightDelta,
                    out Vector3 aimRigPosition,
                    out Vector3 aimRigFocus);
                desiredPosition = Vector3.Lerp(basePosition, aimRigPosition, aimWeight);
                focus = Vector3.Lerp(baseFocus, aimRigFocus, aimWeight);
            }
            else
            {
                desiredPosition = baseFocus
                    + cameraPositionRotation * (cameraOffset + cueCameraOffset + aimCameraOffset * aimWeight)
                    + cueOffset * cueWeight;
                focus = RotateFocusAroundAnchor(desiredPosition, aimFocus, totalAimYawOffsetDegrees * aimWeight);
            }
            UpdateFieldOfView(deltaTime, cueWeight);

            bool aimFollowActive = aimWeight > 0.5f;
            if (aimFollowActive != wasAimFollowActive)
            {
                followVelocity = Vector3.zero;
                wasAimFollowActive = aimFollowActive;
            }

            float activeFollowSmoothTime = Mathf.Lerp(followSmoothTime, aimFollowSmoothTime, aimWeight);
            if (activeFollowSmoothTime <= 0.0001f)
            {
                transform.position = desiredPosition;
                followVelocity = Vector3.zero;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref followVelocity,
                    activeFollowSmoothTime);
            }

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
            controlledCamera = ResolveControlledCamera();
            baseFieldOfView = controlledCamera != null ? controlledCamera.fieldOfView : 50f;
        }

        private Camera ResolveControlledCamera()
        {
            if (controlledCamera == null)
            {
                controlledCamera = GetComponent<Camera>();
            }

            return controlledCamera;
        }

        private void OnEnable()
        {
            enabledOrbitAction = EnableActionIfNeeded(orbitAction);
        }

        private void OnDisable()
        {
            DisableActionIfOwned(orbitAction, enabledOrbitAction);
            aimOrbitInput = Vector2.zero;
            aimYawOffsetDegrees = 0f;
            hasAimAssistYawTarget = false;
            requestedAimAssistYawTargetDegrees = 0f;
            requestedAimAssistStrength01 = 0f;
            aimAssistYawOffsetDegrees = 0f;
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

        private void BuildAimRigPose(
            Quaternion aimRotation,
            Vector3 cueCameraOffset,
            float cueWeight,
            float activeCueFocusHeightDelta,
            out Vector3 position,
            out Vector3 focus)
        {
            Vector3 rigOrigin = target.position;
            position = rigOrigin
                + aimRotation * (cameraOffset + cueCameraOffset + aimCameraOffset)
                + cueOffset * cueWeight;
            focus = rigOrigin
                + aimRotation * (lookOffset + aimFocusOffset)
                + Vector3.up * (activeCueFocusHeightDelta * cueWeight);
        }

        private float ResolveFixedRearYaw()
        {
            Transform yawReference = fixedRearYawReference != null ? fixedRearYawReference : target;
            float baseYaw = yawReference != null ? yawReference.eulerAngles.y : orbitYawDegrees;
            return NormalizeYaw(baseYaw + fixedRearYawOffsetDegrees);
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

        private static Vector3 RotateFocusAroundAnchor(Vector3 anchor, Vector3 focus, float yawDegrees)
        {
            if (Mathf.Approximately(yawDegrees, 0f))
            {
                return focus;
            }

            Vector3 localFocus = focus - anchor;
            Vector3 planarFocus = Vector3.ProjectOnPlane(localFocus, Vector3.up);
            if (planarFocus.sqrMagnitude <= 0.0001f)
            {
                return focus;
            }

            Vector3 verticalFocus = localFocus - planarFocus;
            Vector3 rotatedPlanarFocus = Quaternion.Euler(0f, yawDegrees, 0f) * planarFocus;
            return anchor + rotatedPlanarFocus + verticalFocus;
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

        private void UpdateAimWeight(float deltaTime)
        {
            float speed = aimTargetWeight > aimWeight ? aimBlendInSpeed : aimBlendOutSpeed;
            if (speed <= 0f)
            {
                aimWeight = aimTargetWeight;
                return;
            }

            float step = 1f - Mathf.Exp(-speed * deltaTime);
            aimWeight = Mathf.Lerp(aimWeight, aimTargetWeight, step);
        }

        private void UpdateAimYawOffset(float deltaTime)
        {
            float targetOffset = 0f;
            Vector2 aimInput = ApplyDeadZone(aimOrbitInput);
            bool holdsCurrentYaw = false;
            if (aimOrbitUsesInput && aimTargetWeight > 0f && aimOrbitYawLimitDegrees > 0f)
            {
                holdsCurrentYaw = aimOrbitHoldsYawUntilAimEnds && aimInput.sqrMagnitude <= 0f;
                targetOffset = holdsCurrentYaw ? aimYawOffsetDegrees : aimInput.x * aimOrbitYawLimitDegrees;
            }

            float speed = Mathf.Approximately(targetOffset, 0f) && !holdsCurrentYaw
                ? aimOrbitReturnSpeedDegrees
                : aimOrbitYawSpeedDegrees;
            if (speed <= 0f)
            {
                aimYawOffsetDegrees = targetOffset;
                return;
            }

            aimYawOffsetDegrees = Mathf.MoveTowards(
                aimYawOffsetDegrees,
                targetOffset,
                speed * deltaTime);
        }

        private void UpdateAimAssistYawOffset(float deltaTime)
        {
            float targetOffset = 0f;
            if (aimAssistUsesYawTarget && hasAimAssistYawTarget && aimTargetWeight > 0f && aimOrbitYawLimitDegrees > 0f)
            {
                float blend = Mathf.Clamp01(requestedAimAssistStrength01 * aimAssistMaxYawBlend);
                float desiredTotalOffset = Mathf.Lerp(
                    aimYawOffsetDegrees,
                    requestedAimAssistYawTargetDegrees,
                    blend);
                targetOffset = Mathf.Clamp(
                    desiredTotalOffset - aimYawOffsetDegrees,
                    -aimOrbitYawLimitDegrees,
                    aimOrbitYawLimitDegrees);
            }

            float speed = Mathf.Approximately(targetOffset, 0f)
                ? aimAssistYawReturnSpeedDegrees
                : aimAssistYawSpeedDegrees;
            if (speed <= 0f)
            {
                aimAssistYawOffsetDegrees = targetOffset;
            }
            else
            {
                aimAssistYawOffsetDegrees = Mathf.MoveTowards(
                    aimAssistYawOffsetDegrees,
                    targetOffset,
                    speed * deltaTime);
            }

            hasAimAssistYawTarget = false;
            requestedAimAssistYawTargetDegrees = 0f;
            requestedAimAssistStrength01 = 0f;
        }

        private float ResolveTotalAimYawOffset()
        {
            return Mathf.Clamp(
                aimYawOffsetDegrees + aimAssistYawOffsetDegrees,
                -aimOrbitYawLimitDegrees,
                aimOrbitYawLimitDegrees);
        }

        private void UpdateFieldOfView(float deltaTime, float cueWeight)
        {
            if (controlledCamera == null)
            {
                return;
            }

            float targetFieldOfView = baseFieldOfView
                + cueFieldOfViewDelta * cueWeight
                + aimFieldOfViewDelta * aimWeight;
            float fovStep = 1f - Mathf.Exp(-cueFieldOfViewSmooth * deltaTime);
            controlledCamera.fieldOfView = Mathf.Lerp(controlledCamera.fieldOfView, targetFieldOfView, fovStep);
        }
    }
}
