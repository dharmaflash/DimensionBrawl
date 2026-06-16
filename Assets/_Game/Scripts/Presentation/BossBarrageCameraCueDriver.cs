using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageCameraCueDriver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Transform cueSpace;

        [Header("Cues")]
        [Tooltip("Short boss windup read. Keeps the fixed rear lane readable without a cinematic lock.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue windupCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.04f, -0.10f),
            planarDirectionOffset = 0.06f,
            fieldOfViewDelta = 1.0f,
            cameraDistanceDelta = -0.12f,
            focusHeightDelta = 0.03f,
            durationSeconds = 0.26f,
            finisherScale = 1.18f
        };

        [Tooltip("Short boss fire read. Emphasizes release while preserving projectile dodging control.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue fireCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.02f, -0.16f),
            planarDirectionOffset = 0.08f,
            fieldOfViewDelta = 1.6f,
            cameraDistanceDelta = -0.18f,
            focusHeightDelta = 0.02f,
            durationSeconds = 0.20f,
            finisherScale = 1.25f
        };

        private bool subscribed;
        private int windupCueRequestCount;
        private int fireCueRequestCount;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public ActionCameraController CameraController => cameraController;
        public Transform CueSpace => cueSpace;
        public int WindupCueRequestCount => windupCueRequestCount;
        public int FireCueRequestCount => fireCueRequestCount;

        public void Configure(BossBarrageEmitter newEmitter, ActionCameraController newCameraController, Transform newCueSpace)
        {
            Unsubscribe();
            bossBarrageEmitter = newEmitter;
            cameraController = newCameraController;
            cueSpace = newCueSpace;
            Subscribe();
        }

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<ActionCameraController>();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void HandleWindupStarted(BossBarrageEmitter emitter, BossBarragePatternProfile pattern)
        {
            if (RequestCue(windupCue, ResolveBossDirection(emitter), ResolvePatternScale(pattern, windupCue)))
            {
                windupCueRequestCount++;
            }
        }

        private void HandleWaveFired(BossBarrageEmitter emitter, BossBarragePatternProfile pattern, int spawnedCount)
        {
            if (RequestCue(fireCue, ResolveBossDirection(emitter), ResolveFireScale(pattern, spawnedCount, fireCue)))
            {
                fireCueRequestCount++;
            }
        }

        private bool RequestCue(ActionCameraCueProfile.CameraCue cue, Vector3 planarDirection, float scale)
        {
            if (!cue.enabled || cameraController == null)
            {
                return false;
            }

            Transform space = cueSpace != null ? cueSpace : transform;
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
            return true;
        }

        private Vector3 ResolveBossDirection(BossBarrageEmitter emitter)
        {
            Transform space = cueSpace != null ? cueSpace : transform;
            Transform bossTransform = emitter != null ? emitter.transform : null;
            if (bossTransform != null && TryResolvePlanarDirection(space.position, bossTransform.position, out Vector3 bossDirection))
            {
                return bossDirection;
            }

            if (cameraController != null
                && cameraController.Threat != null
                && TryResolvePlanarDirection(space.position, cameraController.Threat.position, out Vector3 threatDirection))
            {
                return threatDirection;
            }

            return space.forward;
        }

        private void Subscribe()
        {
            if (subscribed || bossBarrageEmitter == null)
            {
                return;
            }

            bossBarrageEmitter.WindupStarted += HandleWindupStarted;
            bossBarrageEmitter.WaveFired += HandleWaveFired;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || bossBarrageEmitter == null)
            {
                subscribed = false;
                return;
            }

            bossBarrageEmitter.WindupStarted -= HandleWindupStarted;
            bossBarrageEmitter.WaveFired -= HandleWaveFired;
            subscribed = false;
        }

        private static bool TryResolvePlanarDirection(Vector3 from, Vector3 to, out Vector3 direction)
        {
            direction = Vector3.ProjectOnPlane(to - from, Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            return true;
        }

        private static float ResolvePatternScale(
            BossBarragePatternProfile pattern,
            ActionCameraCueProfile.CameraCue cue)
        {
            int projectileCount = pattern != null ? pattern.ProjectilesPerWave : 1;
            return ResolveProjectileScale(projectileCount, cue);
        }

        private static float ResolveFireScale(
            BossBarragePatternProfile pattern,
            int spawnedCount,
            ActionCameraCueProfile.CameraCue cue)
        {
            int projectileCount = Mathf.Max(spawnedCount, pattern != null ? pattern.ProjectilesPerWave : 1);
            return ResolveProjectileScale(projectileCount, cue);
        }

        private static float ResolveProjectileScale(int projectileCount, ActionCameraCueProfile.CameraCue cue)
        {
            float pressureWeight = Mathf.Clamp01((Mathf.Max(1, projectileCount) - 1) / 6f);
            return Mathf.Lerp(1f, cue.finisherScale, pressureWeight);
        }
    }
}
