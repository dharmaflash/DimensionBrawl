using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace DimensionBrawl.Presentation
{
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(Camera))]
    public sealed class ActionCameraController : MonoBehaviour
    {
        private static readonly List<ActionCameraController> ActiveControllers = new(2);

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

        [Header("Micro Shake")]
        [Tooltip("Short deterministic impact shake layered after the main camera cue. Rifle fire is kept modest so it reads as rhythm instead of heavy impact shake.")]
        [SerializeField] private bool enableMicroShake = true;
        [SerializeField, Min(0f)] private float maxMicroShakePosition = 0.085f;
        [SerializeField, Min(0f)] private float maxMicroShakeEuler = 0.9f;
        [SerializeField, Min(0f)] private float microShakeFrequency = 34f;
        [SerializeField] private Vector3 microShakePositionAxes = new Vector3(1f, 0.58f, 0.05f);
        [SerializeField] private Vector3 microShakeEulerAxes = new Vector3(0.34f, 0.28f, 1f);

        [Header("Combat Feedback")]
        [SerializeField, Min(0f)] private float rifleFireFeedbackCooldownSeconds = 0.035f;
        [SerializeField, Min(0f)] private float sustainedFireFeedbackIntervalSeconds = 0.135f;
        [SerializeField, Min(0f)] private float heavyShotFeedbackCooldownSeconds = 0.08f;
        [SerializeField, Min(0f)] private float hitFeedbackCooldownSeconds = 0.12f;
        [SerializeField, Min(0f)] private float explosionFeedbackCooldownSeconds = 0.14f;
        [SerializeField, Min(0f)] private float laserSustainFeedbackIntervalSeconds = 0.14f;
        [SerializeField, Min(0f)] private float shieldBlockFeedbackCooldownSeconds = 0.07f;
        [SerializeField, Range(0f, 1f)] private float liveFireFeedbackScale = 1f;

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
        [Tooltip("Preserves the authored world-space look offset when a rotated scene root uses the linked aim rig.")]
        [SerializeField] private bool aimRigUsesWorldLookOffset;
        [SerializeField, Range(0f, 90f)] private float aimOrbitYawLimitDegrees = 45f;
        [SerializeField] private bool aimOrbitUsesPitchInput = true;
        [SerializeField, Range(0f, 45f)] private float aimOrbitPitchLimitDegrees = 16f;
        [SerializeField, Min(0f)] private float aimOrbitYawSpeedDegrees = 360f;
        [SerializeField, Min(0f)] private float aimOrbitReturnSpeedDegrees = 420f;

        [Header("Look Peek")]
        [Tooltip("Screen-space camera pan used by mobile free-look drags outside fire. This never changes aim yaw or player facing.")]
        [SerializeField, Min(0f)] private float lookPeekHorizontalOffset = 1.15f;
        [SerializeField, Min(0f)] private float lookPeekVerticalOffset = 0.55f;

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
        private Vector2 lookPeekInput;
        private float lookPeekTargetWeight;
        private float lookPeekWeight;
        private float aimYawOffsetDegrees;
        private float aimPitchOffsetDegrees;
        private bool hasAimAssistYawTarget;
        private float requestedAimAssistYawTargetDegrees;
        private float requestedAimAssistStrength01;
        private float aimAssistYawOffsetDegrees;
        private bool wasAimFollowActive;
        private float microShakeTimer;
        private float microShakeDuration;
        private float microShakePositionAmplitude;
        private float microShakeEulerAmplitude;
        private float microShakeSeed;
        private float microShakeActiveFrequency;
        private Vector2 microShakeDirectionBias;
        private Vector3 lastMicroShakeLocalOffset;
        private Vector3 lastMicroShakeEulerOffset;
        private int microShakeRequestCount;
        private float nextRifleFireFeedbackTime;
        private float nextHeavyShotFeedbackTime;
        private float nextHitFeedbackTime;
        private float nextExplosionFeedbackTime;
        private float nextLaserSustainFeedbackTime;
        private float nextShieldBlockFeedbackTime;
        private int rifleFireFeedbackRequestCount;
        private float lastRifleFireFeedbackTime = float.NegativeInfinity;
        private float lastManualViewIntentTime = float.NegativeInfinity;
        private bool hasBaseFieldOfView;

        public bool HasActiveCue => cueTimer > 0f;
        public bool HasActiveMicroShake => microShakeTimer > 0f;
        public bool IsAimModifierActive => aimTargetWeight > 0.5f;
        public float AimWeight => aimWeight;
        public Vector2 AimOrbitInput => aimOrbitInput;
        public Vector2 LookPeekInput => lookPeekInput;
        public float AimYawOffsetDegrees => aimYawOffsetDegrees;
        public float AimPitchOffsetDegrees => aimPitchOffsetDegrees;
        public float AimAssistYawOffsetDegrees => aimAssistYawOffsetDegrees;
        public float TotalAimYawOffsetDegrees => ResolveTotalAimYawOffset();
        public float TotalAimPitchOffsetDegrees => ResolveTotalAimPitchOffset();
        public float OrbitYawDegrees => orbitYawDegrees;
        public Transform Target => target;
        public Transform Threat => threat;
        public int MicroShakeRequestCount => microShakeRequestCount;
        public int RifleFireFeedbackRequestCount => rifleFireFeedbackRequestCount;
        public float LastRifleFireFeedbackTime => lastRifleFireFeedbackTime;
        public Vector3 LastMicroShakeLocalOffset => lastMicroShakeLocalOffset;
        public Vector3 LastMicroShakeEulerOffset => lastMicroShakeEulerOffset;
        public float LastManualViewIntentTime => lastManualViewIntentTime;
        public static ActionCameraController ActiveInstance
        {
            get
            {
                for (int index = ActiveControllers.Count - 1; index >= 0; index--)
                {
                    ActionCameraController controller = ActiveControllers[index];
                    if (controller != null && controller.isActiveAndEnabled)
                    {
                        return controller;
                    }

                    ActiveControllers.RemoveAt(index);
                }

                return null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            ActiveControllers.Clear();
        }

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
            RecordManualViewIntentIfNeeded(mobileOrbitInput);
        }

        public void SetAimOrbitInput(Vector2 input)
        {
            aimOrbitInput = Vector2.ClampMagnitude(input, 1f);
            RecordManualViewIntentIfNeeded(aimOrbitInput);
        }

        public void SetLookPeekInput(Vector2 input)
        {
            lookPeekInput = Vector2.ClampMagnitude(input, 1f);
            lookPeekTargetWeight = lookPeekInput.sqrMagnitude > 0.0001f ? 1f : 0f;
            RecordManualViewIntentIfNeeded(lookPeekInput);
        }

        public float ResolveManualViewIntentStrength(float strongSeconds, float fadeSeconds)
        {
            if (float.IsNegativeInfinity(lastManualViewIntentTime))
            {
                return 0f;
            }

            float age = Time.time - lastManualViewIntentTime;
            if (age < 0f)
            {
                return 1f;
            }

            float strongDuration = Mathf.Max(0f, strongSeconds);
            if (age <= strongDuration)
            {
                return 1f;
            }

            float fadeDuration = Mathf.Max(0.001f, fadeSeconds);
            return 1f - Mathf.Clamp01((age - strongDuration) / fadeDuration);
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

        public void CaptureBaseFieldOfViewFromControlledCamera()
        {
            if (hasBaseFieldOfView)
            {
                return;
            }

            Camera camera = ResolveControlledCamera();
            if (camera == null)
            {
                return;
            }

            baseFieldOfView = camera.fieldOfView;
            hasBaseFieldOfView = true;
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
                    if (!hasBaseFieldOfView)
                    {
                        baseFieldOfView = handoffFieldOfView.Value;
                        hasBaseFieldOfView = true;
                    }
                }
            }

            followVelocity = Vector3.zero;
            cueOffset = Vector3.zero;
            cueFieldOfViewDelta = 0f;
            cueCameraDistanceDelta = 0f;
            cueFocusHeightDelta = 0f;
            cueTimer = 0f;
            cueDuration = 0f;
            microShakeTimer = 0f;
            microShakeDuration = 0f;
            microShakePositionAmplitude = 0f;
            microShakeEulerAmplitude = 0f;
            microShakeActiveFrequency = 0f;
            microShakeDirectionBias = Vector2.zero;
            lastMicroShakeLocalOffset = Vector3.zero;
            lastMicroShakeEulerOffset = Vector3.zero;
            orbitInitialized = false;
            aimTargetWeight = 0f;
            aimWeight = 0f;
            aimOrbitInput = Vector2.zero;
            aimYawOffsetDegrees = 0f;
            aimPitchOffsetDegrees = 0f;
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

        public void RequestMicroShake(
            float durationSeconds,
            float positionAmplitude,
            float eulerAmplitude,
            Vector3 planarDirection)
        {
            RequestMicroShake(
                durationSeconds,
                positionAmplitude,
                eulerAmplitude,
                planarDirection,
                microShakeFrequency);
        }

        public void RequestMicroShake(
            float durationSeconds,
            float positionAmplitude,
            float eulerAmplitude,
            Vector3 planarDirection,
            float frequency)
        {
            if (!enableMicroShake || durationSeconds <= 0f || (positionAmplitude <= 0f && eulerAmplitude <= 0f))
            {
                return;
            }

            microShakeDuration = Mathf.Max(microShakeDuration, Mathf.Max(0.01f, durationSeconds));
            microShakeTimer = Mathf.Max(microShakeTimer, Mathf.Max(0.01f, durationSeconds));
            microShakePositionAmplitude = Mathf.Max(
                microShakePositionAmplitude,
                Mathf.Clamp(positionAmplitude, 0f, maxMicroShakePosition));
            microShakeEulerAmplitude = Mathf.Max(
                microShakeEulerAmplitude,
                Mathf.Clamp(eulerAmplitude, 0f, maxMicroShakeEuler));
            microShakeActiveFrequency = Mathf.Max(1f, frequency);
            microShakeDirectionBias = ResolveMicroShakeDirectionBias(planarDirection);
            microShakeSeed = Mathf.Repeat(microShakeSeed + 0.371f, 100f);
            microShakeRequestCount++;
        }

        public void RequestRifleFireFeedback(Vector3 shotDirection, bool sustainedFire)
        {
            float cooldown = sustainedFire
                ? sustainedFireFeedbackIntervalSeconds
                : rifleFireFeedbackCooldownSeconds;
            if (!TryReserveFeedback(ref nextRifleFireFeedbackTime, cooldown))
            {
                return;
            }

            float scale = Mathf.Clamp(liveFireFeedbackScale, 0.85f, 1f);
            Vector3 direction = ResolvePlanarFeedbackDirection(shotDirection);
            float duration = sustainedFire ? 0.1025f : 0.085f;
            float positionAmplitude = sustainedFire ? 0.0335f : 0.0315f;
            float eulerAmplitude = sustainedFire ? 0.49f : 0.46f;
            float frequency = sustainedFire ? 11f : 14.5f;
            Vector3 additiveOffset = transform.TransformDirection(new Vector3(0f, 0.012f, -0.0365f)) * scale
                - direction * (0.017f * scale);

            rifleFireFeedbackRequestCount++;
            lastRifleFireFeedbackTime = Time.unscaledTime;
            RequestCue(additiveOffset, duration, 0.345f * scale, -0.035f * scale, 0f);
            RequestMicroShake(
                duration,
                positionAmplitude * scale,
                eulerAmplitude * scale,
                direction,
                frequency);
        }

        public void RequestHeavyShotFeedback(Vector3 shotDirection, float strength01 = 1f)
        {
            if (!TryReserveFeedback(ref nextHeavyShotFeedbackTime, heavyShotFeedbackCooldownSeconds))
            {
                return;
            }

            float weight = Mathf.Clamp01(strength01) * Mathf.Clamp01(liveFireFeedbackScale + 0.25f);
            Vector3 direction = ResolvePlanarFeedbackDirection(shotDirection);
            float duration = Mathf.Lerp(0.08f, 0.13f, weight);
            RequestCue(
                -direction * (0.035f * weight) + Vector3.up * (0.016f * weight),
                duration,
                0.52f * weight,
                -0.04f * weight,
                0.01f * weight);
            RequestMicroShake(
                duration,
                0.04f * weight,
                0.24f * weight,
                direction,
                20f);
        }

        public void RequestDamageHitFeedback(Vector3 incomingDirection, float strength01 = 0.5f)
        {
            if (!TryReserveFeedback(ref nextHitFeedbackTime, hitFeedbackCooldownSeconds))
            {
                return;
            }

            float weight = Mathf.Clamp01(strength01);
            Vector3 direction = ResolvePlanarFeedbackDirection(incomingDirection);
            float duration = Mathf.Lerp(0.10f, 0.16f, weight);
            RequestCue(
                -direction * Mathf.Lerp(0.018f, 0.04f, weight) + Vector3.up * Mathf.Lerp(0.006f, 0.016f, weight),
                duration,
                -Mathf.Lerp(0.05f, 0.16f, weight),
                Mathf.Lerp(0.01f, 0.03f, weight),
                0f);
            RequestMicroShake(
                duration,
                Mathf.Lerp(0.018f, 0.04f, weight),
                Mathf.Lerp(0.10f, 0.20f, weight),
                direction,
                20f);
        }

        public void RequestExplosionFeedback(Vector3 worldPoint, float radius = 9f, float strength01 = 1f)
        {
            Vector3 referencePoint = target != null ? target.position : transform.position;
            float safeRadius = Mathf.Max(0.1f, radius);
            float distance = Vector3.Distance(referencePoint, worldPoint);
            float falloff = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1f - distance / safeRadius));
            float weight = falloff * Mathf.Clamp01(strength01);
            if (weight <= 0.05f || !TryReserveFeedback(ref nextExplosionFeedbackTime, explosionFeedbackCooldownSeconds))
            {
                return;
            }

            Vector3 direction = ResolvePlanarFeedbackDirection(referencePoint - worldPoint);
            float duration = Mathf.Lerp(0.18f, 0.30f, weight);
            RequestCue(
                direction * Mathf.Lerp(0.025f, 0.14f, weight) + Vector3.up * Mathf.Lerp(0.004f, 0.035f, weight),
                duration,
                Mathf.Lerp(0.10f, 0.55f, weight),
                -Mathf.Lerp(0.02f, 0.12f, weight),
                Mathf.Lerp(0.005f, 0.04f, weight));
            RequestMicroShake(
                duration,
                Mathf.Lerp(0.025f, 0.085f, weight),
                Mathf.Lerp(0.10f, 0.55f, weight),
                direction,
                Mathf.Lerp(18f, 28f, weight));
        }

        public void RequestLaserFireFeedback(Vector3 laserDirection)
        {
            Vector3 direction = ResolvePlanarFeedbackDirection(laserDirection);
            const float duration = 0.085f;
            RequestCue(
                -direction * 0.018f + Vector3.up * 0.006f,
                duration,
                0.22f,
                -0.018f,
                0f);
            RequestMicroShake(duration, 0.014f, 0.09f, direction, 18f);
        }

        public void RequestLaserSustainFeedback(Vector3 laserDirection, float strength01 = 1f)
        {
            if (!TryReserveFeedback(ref nextLaserSustainFeedbackTime, laserSustainFeedbackIntervalSeconds))
            {
                return;
            }

            float weight = Mathf.Clamp01(strength01);
            Vector3 direction = ResolvePlanarFeedbackDirection(laserDirection);
            RequestMicroShake(
                0.075f,
                Mathf.Lerp(0.006f, 0.014f, weight),
                Mathf.Lerp(0.035f, 0.075f, weight),
                direction,
                10f);
        }

        public void RequestPerfectDodgeFeedback(Vector3 dodgeDirection)
        {
            Vector3 direction = ResolvePlanarFeedbackDirection(dodgeDirection);
            RequestCue(
                -direction * 0.04f + Vector3.up * 0.018f,
                0.12f,
                0.45f,
                -0.06f,
                0.015f);
            RequestMicroShake(0.07f, 0.008f, 0.045f, direction, 14f);
        }

        public void RequestShieldBlockFeedback(Vector3 incomingDirection, float strength01 = 1f)
        {
            if (!TryReserveFeedback(ref nextShieldBlockFeedbackTime, shieldBlockFeedbackCooldownSeconds))
            {
                return;
            }

            float weight = Mathf.Clamp01(strength01);
            Vector3 direction = ResolvePlanarFeedbackDirection(incomingDirection);
            RequestCue(
                -direction * Mathf.Lerp(0.02f, 0.035f, weight) + Vector3.up * Mathf.Lerp(0.006f, 0.014f, weight),
                0.08f,
                Mathf.Lerp(0.10f, 0.18f, weight),
                -Mathf.Lerp(0.012f, 0.025f, weight),
                0f);
            RequestMicroShake(
                0.08f,
                Mathf.Lerp(0.012f, 0.022f, weight),
                Mathf.Lerp(0.07f, 0.12f, weight),
                direction,
                22f);
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
            UpdateLookPeekWeight(deltaTime);
            UpdateAimOrbitOffsets(deltaTime);
            UpdateAimAssistYawOffset(deltaTime);
            float totalAimYawOffsetDegrees = ResolveTotalAimYawOffset();
            float totalAimPitchOffsetDegrees = ResolveTotalAimPitchOffset();
            Quaternion baseRotation = Quaternion.Euler(
                0f,
                NormalizeYaw(orbitYawDegrees),
                0f);
            Quaternion aimRotation = Quaternion.Euler(
                0f,
                NormalizeYaw(orbitYawDegrees + totalAimYawOffsetDegrees),
                0f);
            Quaternion cameraPositionRotation = aimOrbitRotatesCameraPosition ? aimRotation : baseRotation;
            Vector3 baseFocus = BuildFocusPoint()
                + Vector3.up * (cueFocusHeightDelta * cueWeight)
                + ResolveLookPeekOffset(baseRotation);
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
                    baseFocus,
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

            focus = RotateFocusPitchAroundAnchor(desiredPosition, focus, totalAimPitchOffsetDegrees * aimWeight);
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
            ApplyMicroShake(Time.unscaledDeltaTime);
        }

        private void Awake()
        {
            RegisterActiveController();
            controlledCamera = ResolveControlledCamera();
            CaptureBaseFieldOfViewFromControlledCamera();
            if (!hasBaseFieldOfView)
            {
                baseFieldOfView = 50f;
                hasBaseFieldOfView = true;
            }
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
            RegisterActiveController();
            enabledOrbitAction = EnableActionIfNeeded(orbitAction);
        }

        private void OnDisable()
        {
            ActiveControllers.Remove(this);
            DisableActionIfOwned(orbitAction, enabledOrbitAction);
            aimOrbitInput = Vector2.zero;
            lookPeekInput = Vector2.zero;
            lookPeekTargetWeight = 0f;
            lookPeekWeight = 0f;
            aimYawOffsetDegrees = 0f;
            aimPitchOffsetDegrees = 0f;
            hasAimAssistYawTarget = false;
            requestedAimAssistYawTargetDegrees = 0f;
            requestedAimAssistStrength01 = 0f;
            aimAssistYawOffsetDegrees = 0f;
            microShakeTimer = 0f;
            microShakeDuration = 0f;
            microShakePositionAmplitude = 0f;
            microShakeEulerAmplitude = 0f;
            microShakeActiveFrequency = 0f;
            microShakeDirectionBias = Vector2.zero;
            lastMicroShakeLocalOffset = Vector3.zero;
            lastMicroShakeEulerOffset = Vector3.zero;
        }

        private void OnDestroy()
        {
            ActiveControllers.Remove(this);
        }

        private void RegisterActiveController()
        {
            if (!ActiveControllers.Contains(this))
            {
                ActiveControllers.Add(this);
            }
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
            Vector3 baseFocus,
            out Vector3 position,
            out Vector3 focus)
        {
            Vector3 rigOrigin = target.position;
            position = rigOrigin
                + aimRotation * (cameraOffset + cueCameraOffset + aimCameraOffset)
                + cueOffset * cueWeight;
            if (aimRigUsesWorldLookOffset)
            {
                focus = baseFocus + aimRotation * aimFocusOffset;
                return;
            }

            focus = rigOrigin
                + aimRotation * (lookOffset + aimFocusOffset)
                + Vector3.up * (activeCueFocusHeightDelta * cueWeight);
        }

        private Vector3 ResolveLookPeekOffset(Quaternion baseRotation)
        {
            float weight = aimWeight <= 0.001f ? lookPeekWeight : 0f;
            if (weight <= 0.001f)
            {
                return Vector3.zero;
            }

            Vector2 input = ApplyDeadZone(lookPeekInput);
            if (input.sqrMagnitude <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 right = baseRotation * Vector3.right;
            return (right * (input.x * lookPeekHorizontalOffset)
                + Vector3.up * (input.y * lookPeekVerticalOffset)) * weight;
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
            else
            {
                lastManualViewIntentTime = Time.time;
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

        private void RecordManualViewIntentIfNeeded(Vector2 input)
        {
            if (input.sqrMagnitude >= orbitInputDeadZone * orbitInputDeadZone)
            {
                lastManualViewIntentTime = Time.time;
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

        private static Vector3 RotateFocusPitchAroundAnchor(Vector3 anchor, Vector3 focus, float pitchDegrees)
        {
            if (Mathf.Approximately(pitchDegrees, 0f))
            {
                return focus;
            }

            Vector3 localFocus = focus - anchor;
            Vector3 planarFocus = Vector3.ProjectOnPlane(localFocus, Vector3.up);
            if (planarFocus.sqrMagnitude <= 0.0001f)
            {
                return focus;
            }

            Vector3 rightAxis = Vector3.Cross(Vector3.up, planarFocus.normalized);
            if (rightAxis.sqrMagnitude <= 0.0001f)
            {
                return focus;
            }

            Vector3 pitchedFocus = Quaternion.AngleAxis(-pitchDegrees, rightAxis.normalized) * localFocus;
            return anchor + pitchedFocus;
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

        private void UpdateLookPeekWeight(float deltaTime)
        {
            float speed = lookPeekTargetWeight > lookPeekWeight ? aimBlendInSpeed : aimBlendOutSpeed;
            if (speed <= 0f)
            {
                lookPeekWeight = lookPeekTargetWeight;
                return;
            }

            float step = 1f - Mathf.Exp(-speed * deltaTime);
            lookPeekWeight = Mathf.Lerp(lookPeekWeight, lookPeekTargetWeight, step);
        }

        private void UpdateAimOrbitOffsets(float deltaTime)
        {
            float targetYawOffset = 0f;
            float targetPitchOffset = 0f;
            Vector2 aimInput = ApplyDeadZone(aimOrbitInput);
            bool holdsCurrentOffset = false;
            if (aimInput.sqrMagnitude > 0f)
            {
                lastManualViewIntentTime = Time.time;
            }

            if (aimOrbitUsesInput && aimTargetWeight > 0f && aimOrbitYawLimitDegrees > 0f)
            {
                holdsCurrentOffset = aimOrbitHoldsYawUntilAimEnds && aimInput.sqrMagnitude <= 0f;
                targetYawOffset = holdsCurrentOffset ? aimYawOffsetDegrees : aimInput.x * aimOrbitYawLimitDegrees;
            }

            if (aimOrbitUsesInput && aimOrbitUsesPitchInput && aimTargetWeight > 0f && aimOrbitPitchLimitDegrees > 0f)
            {
                targetPitchOffset = holdsCurrentOffset ? aimPitchOffsetDegrees : aimInput.y * aimOrbitPitchLimitDegrees;
            }

            bool shouldReturnToCenter =
                Mathf.Approximately(targetYawOffset, 0f)
                && Mathf.Approximately(targetPitchOffset, 0f)
                && !holdsCurrentOffset;
            float speed = shouldReturnToCenter ? aimOrbitReturnSpeedDegrees : aimOrbitYawSpeedDegrees;
            if (speed <= 0f)
            {
                aimYawOffsetDegrees = targetYawOffset;
                aimPitchOffsetDegrees = targetPitchOffset;
                return;
            }

            aimYawOffsetDegrees = Mathf.MoveTowards(
                aimYawOffsetDegrees,
                targetYawOffset,
                speed * deltaTime);
            aimPitchOffsetDegrees = Mathf.MoveTowards(
                aimPitchOffsetDegrees,
                targetPitchOffset,
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

        private float ResolveTotalAimPitchOffset()
        {
            return Mathf.Clamp(
                aimPitchOffsetDegrees,
                -aimOrbitPitchLimitDegrees,
                aimOrbitPitchLimitDegrees);
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

        private void ApplyMicroShake(float deltaTime)
        {
            if (microShakeTimer <= 0f)
            {
                lastMicroShakeLocalOffset = Vector3.zero;
                lastMicroShakeEulerOffset = Vector3.zero;
                return;
            }

            float duration = Mathf.Max(0.01f, microShakeDuration);
            float normalized = Mathf.Clamp01(microShakeTimer / duration);
            float envelope = normalized * normalized * (3f - 2f * normalized);
            float activeFrequency = microShakeActiveFrequency > 0f ? microShakeActiveFrequency : microShakeFrequency;
            float phase = (Time.unscaledTime + microShakeSeed) * Mathf.Max(1f, activeFrequency);
            float x = Mathf.Sin(phase * 6.283185f);
            float y = Mathf.Sin((phase * 1.37f + 0.23f + microShakeSeed) * 6.283185f);
            float z = Mathf.Sin((phase * 1.91f + 0.41f + microShakeSeed) * 6.283185f);

            x = Mathf.Lerp(x, microShakeDirectionBias.x, 0.22f);
            y = Mathf.Lerp(y, microShakeDirectionBias.y, 0.16f);

            lastMicroShakeLocalOffset = Vector3.Scale(
                new Vector3(x, y, z),
                microShakePositionAxes) * (microShakePositionAmplitude * envelope);
            lastMicroShakeEulerOffset = Vector3.Scale(
                new Vector3(y, -x, x - y * 0.35f),
                microShakeEulerAxes) * (microShakeEulerAmplitude * envelope);

            transform.position += transform.right * lastMicroShakeLocalOffset.x
                + transform.up * lastMicroShakeLocalOffset.y
                + transform.forward * lastMicroShakeLocalOffset.z;
            transform.rotation *= Quaternion.Euler(lastMicroShakeEulerOffset);

            microShakeTimer = Mathf.Max(0f, microShakeTimer - Mathf.Max(0f, deltaTime));
            if (microShakeTimer <= 0f)
            {
                microShakePositionAmplitude = 0f;
                microShakeEulerAmplitude = 0f;
                microShakeActiveFrequency = 0f;
                microShakeDirectionBias = Vector2.zero;
            }
        }

        private bool TryReserveFeedback(ref float nextAllowedTime, float cooldownSeconds)
        {
            float now = Time.unscaledTime;
            if (now < nextAllowedTime)
            {
                return false;
            }

            nextAllowedTime = now + Mathf.Max(0f, cooldownSeconds);
            return true;
        }

        private Vector3 ResolvePlanarFeedbackDirection(Vector3 direction)
        {
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude > 0.0001f)
            {
                return planar.normalized;
            }

            if (target != null)
            {
                planar = Vector3.ProjectOnPlane(target.forward, Vector3.up);
                if (planar.sqrMagnitude > 0.0001f)
                {
                    return planar.normalized;
                }
            }

            planar = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            return planar.sqrMagnitude > 0.0001f ? planar.normalized : Vector3.forward;
        }

        private Vector2 ResolveMicroShakeDirectionBias(Vector3 planarDirection)
        {
            Vector3 direction = Vector3.ProjectOnPlane(planarDirection, Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            direction.Normalize();
            return Vector2.ClampMagnitude(
                new Vector2(
                    Vector3.Dot(direction, transform.right),
                    Vector3.Dot(direction, transform.forward)),
                1f);
        }
    }
}
