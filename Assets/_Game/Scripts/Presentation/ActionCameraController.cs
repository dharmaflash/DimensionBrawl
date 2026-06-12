using UnityEngine;

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
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 4.4f, -6.4f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);
        [Tooltip("First-pass deviation: no collected follow damping value exists yet, so this remains Inspector-tunable.")]
        [SerializeField, Min(0f)] private float followSmoothTime = 0.12f;
        [SerializeField, Min(0f)] private float rotationSmooth = 16f;

        [Header("Threat Bias")]
        [Tooltip("Keeps current threat readable without becoming hard lock-on. This is intentionally inspectable for tuning.")]
        [SerializeField, Range(0f, 1f)] private float threatBias = 0.35f;
        [SerializeField, Min(0f)] private float maxThreatFocusOffset = 1.8f;
        [SerializeField, Min(0f)] private float maxLeadFromPlayerSpeed = 1f;

        [Header("Cue")]
        [Tooltip("Uses the collected perfect-dodge/camera-cue range around 0.20-0.32 seconds.")]
        [SerializeField, Min(0f)] private float defaultCueSeconds = 0.24f;

        private Vector3 followVelocity;
        private Vector3 cueOffset;
        private float cueTimer;
        private float cueDuration;

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
            cueOffset = additiveOffset;
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
            Vector3 focus = BuildFocusPoint();
            Vector3 desiredPosition = focus + target.rotation * cameraOffset + UpdateCueOffset(deltaTime);

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

        private Vector3 UpdateCueOffset(float deltaTime)
        {
            if (cueTimer <= 0f)
            {
                return Vector3.zero;
            }

            cueTimer = Mathf.Max(0f, cueTimer - deltaTime);
            float normalizedTime = cueDuration > 0f ? cueTimer / cueDuration : 0f;
            return cueOffset * normalizedTime;
        }
    }
}
