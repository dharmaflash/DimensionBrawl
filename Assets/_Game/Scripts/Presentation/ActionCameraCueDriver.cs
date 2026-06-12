using System;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed class ActionCameraCueDriver : MonoBehaviour
    {
        [Serializable]
        private struct CameraCueProfile
        {
            public bool enabled;
            public Vector3 localOffset;
            public float planarDirectionOffset;
            public float fieldOfViewDelta;
            public float cameraDistanceDelta;
            public float focusHeightDelta;
            public float durationSeconds;
            public float finisherScale;
        }

        [Header("References")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerMovementController movement;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Transform cueSpace;

        [Header("Cue Profiles")]
        [Tooltip("Run-start cue. Uses the low end of short action cue timing so movement start feels deliberate without camera lock.")]
        [SerializeField] private CameraCueProfile runStartCue = new CameraCueProfile
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.02f, -0.10f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = 0.8f,
            cameraDistanceDelta = -0.08f,
            focusHeightDelta = 0.01f,
            durationSeconds = 0.20f,
            finisherScale = 1f
        };

        [Tooltip("Short stop-settle cue. Uses the 0.15-0.35s dodge/hit emphasis range conservatively.")]
        [SerializeField] private CameraCueProfile stopSettleCue = new CameraCueProfile
        {
            enabled = true,
            localOffset = new Vector3(0f, -0.02f, -0.06f),
            planarDirectionOffset = -0.02f,
            fieldOfViewDelta = -0.8f,
            cameraDistanceDelta = -0.12f,
            focusHeightDelta = -0.02f,
            durationSeconds = 0.22f,
            finisherScale = 1f
        };

        [Tooltip("Sharp movement turn cue. Keeps 90-degree direction changes readable without a full lock-on camera.")]
        [SerializeField] private CameraCueProfile sharpTurnCue = new CameraCueProfile
        {
            enabled = true,
            localOffset = new Vector3(0.08f, 0f, -0.10f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = 0.6f,
            cameraDistanceDelta = -0.06f,
            focusHeightDelta = 0f,
            durationSeconds = 0.24f,
            finisherScale = 1f
        };

        [Tooltip("Dodge read cue. Uses the collected short camera cue range around 0.20-0.32s.")]
        [SerializeField] private CameraCueProfile dodgeCue = new CameraCueProfile
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.04f, -0.20f),
            planarDirectionOffset = -0.18f,
            fieldOfViewDelta = 2.2f,
            cameraDistanceDelta = -0.20f,
            focusHeightDelta = 0.03f,
            durationSeconds = 0.28f,
            finisherScale = 1f
        };

        [Tooltip("Basic attack entry cue. Small by default so normal attacks do not become cinematic locks.")]
        [SerializeField] private CameraCueProfile attackStartCue = new CameraCueProfile
        {
            enabled = true,
            localOffset = new Vector3(0f, -0.03f, 0.14f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = -1.2f,
            cameraDistanceDelta = 0.12f,
            focusHeightDelta = -0.02f,
            durationSeconds = 0.22f,
            finisherScale = 1.2f
        };

        [Tooltip("Successful hit cue. Kept shorter than attack state emphasis; hit-stop already carries impact.")]
        [SerializeField] private CameraCueProfile attackHitCue = new CameraCueProfile
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.03f, 0.12f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = -1.8f,
            cameraDistanceDelta = 0.16f,
            focusHeightDelta = 0.01f,
            durationSeconds = 0.18f,
            finisherScale = 1.3f
        };

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<ActionCameraController>();
            }
        }

        private void OnEnable()
        {
            if (movement != null)
            {
                movement.RunStarted += HandleRunStarted;
                movement.StopSettleStarted += HandleStopSettleStarted;
                movement.SharpTurnStarted += HandleSharpTurnStarted;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted += HandleDodgeStarted;
                actionController.BasicAttackStarted += HandleBasicAttackStarted;
                actionController.BasicAttackHit += HandleBasicAttackHit;
            }
        }

        private void OnDisable()
        {
            if (movement != null)
            {
                movement.RunStarted -= HandleRunStarted;
                movement.StopSettleStarted -= HandleStopSettleStarted;
                movement.SharpTurnStarted -= HandleSharpTurnStarted;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted -= HandleDodgeStarted;
                actionController.BasicAttackStarted -= HandleBasicAttackStarted;
                actionController.BasicAttackHit -= HandleBasicAttackHit;
            }
        }

        private void HandleRunStarted()
        {
            RequestCue(runStartCue, ResolvePlanarDirection(), 1f);
        }

        private void HandleStopSettleStarted()
        {
            RequestCue(stopSettleCue, -ResolvePlanarDirection(), 1f);
        }

        private void HandleSharpTurnStarted(float signedAngle)
        {
            float side = signedAngle < 0f ? -1f : 1f;
            Vector3 turnDirection = Quaternion.AngleAxis(35f * side, Vector3.up) * ResolvePlanarDirection();
            RequestCue(sharpTurnCue, turnDirection, 1f);
        }

        private void HandleDodgeStarted()
        {
            Vector3 dodgeDirection = actionController != null ? actionController.LastDodgeDirection : ResolvePlanarDirection();
            RequestCue(dodgeCue, dodgeDirection, 1f);
        }

        private void HandleBasicAttackStarted(int comboIndex)
        {
            RequestCue(attackStartCue, ResolvePlanarDirection(), ResolveComboScale(comboIndex, attackStartCue));
        }

        private void HandleBasicAttackHit(int comboIndex)
        {
            RequestCue(attackHitCue, ResolvePlanarDirection(), ResolveComboScale(comboIndex, attackHitCue));
        }

        private void RequestCue(CameraCueProfile cue, Vector3 planarDirection, float scale)
        {
            if (!cue.enabled || cameraController == null)
            {
                return;
            }

            Transform space = cueSpace != null ? cueSpace : (movement != null ? movement.transform : transform);
            Vector3 offset = space.TransformDirection(cue.localOffset);
            Vector3 direction = Vector3.ProjectOnPlane(planarDirection, Vector3.up);
            if (direction.sqrMagnitude > 0.0001f)
            {
                offset += direction.normalized * cue.planarDirectionOffset;
            }

            float clampedScale = Mathf.Max(0f, scale);
            cameraController.RequestCue(
                offset * clampedScale,
                cue.durationSeconds,
                cue.fieldOfViewDelta * clampedScale,
                cue.cameraDistanceDelta * clampedScale,
                cue.focusHeightDelta * clampedScale);
        }

        private Vector3 ResolvePlanarDirection()
        {
            if (movement == null)
            {
                return transform.forward;
            }

            Vector3 intent = movement.MoveIntentDirection;
            if (intent.sqrMagnitude > 0.0001f)
            {
                return intent.normalized;
            }

            Vector3 velocity = Vector3.ProjectOnPlane(movement.PlanarVelocity, Vector3.up);
            if (velocity.sqrMagnitude > 0.0001f)
            {
                return velocity.normalized;
            }

            return movement.FacingDirection;
        }

        private static float ResolveComboScale(int comboIndex, CameraCueProfile cue)
        {
            if (comboIndex <= 0)
            {
                return 1f;
            }

            float comboWeight = Mathf.Clamp01(comboIndex / 4f);
            return Mathf.Lerp(1f, cue.finisherScale, comboWeight);
        }
    }
}
