using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageCameraCueDriver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
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

        [Tooltip("Boss costed skill read. Short additive cue before the queued skill pattern windup takes over.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue pressureSkillCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.03f, -0.12f),
            planarDirectionOffset = 0.07f,
            fieldOfViewDelta = 1.1f,
            cameraDistanceDelta = -0.12f,
            focusHeightDelta = 0.02f,
            durationSeconds = 0.20f,
            finisherScale = 1.18f
        };

        [Tooltip("Boss summon-pressure read. Slightly widens the field so the proxy/screen exchange can read.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue pressureSummonCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.04f, -0.06f),
            planarDirectionOffset = 0.04f,
            fieldOfViewDelta = 1.4f,
            cameraDistanceDelta = 0.10f,
            focusHeightDelta = 0.04f,
            durationSeconds = 0.28f,
            finisherScale = 1.25f
        };

        [Tooltip("Boss overextend-punish read. Stronger but still bounded so dodge control remains readable.")]
        [SerializeField] private ActionCameraCueProfile.CameraCue pressurePunishCue = new ActionCameraCueProfile.CameraCue
        {
            enabled = true,
            localOffset = new Vector3(0f, 0.05f, -0.20f),
            planarDirectionOffset = 0.11f,
            fieldOfViewDelta = 2.0f,
            cameraDistanceDelta = -0.22f,
            focusHeightDelta = 0.03f,
            durationSeconds = 0.26f,
            finisherScale = 1.35f
        };

        private bool subscribed;
        private bool pressureActionSubscribed;
        private int windupCueRequestCount;
        private int fireCueRequestCount;
        private int pressureActionCueRequestCount;
        private BossPressureActionKind lastPressureActionKind;
        private int lastPressureActionTier;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public BossPressureActionDirector BossPressureActionDirector => bossPressureActionDirector;
        public ActionCameraController CameraController => cameraController;
        public Transform CueSpace => cueSpace;
        public int WindupCueRequestCount => windupCueRequestCount;
        public int FireCueRequestCount => fireCueRequestCount;
        public int PressureActionCueRequestCount => pressureActionCueRequestCount;
        public BossPressureActionKind LastPressureActionKind => lastPressureActionKind;
        public int LastPressureActionTier => lastPressureActionTier;

        public void Configure(
            BossBarrageEmitter newEmitter,
            ActionCameraController newCameraController,
            Transform newCueSpace,
            BossPressureActionDirector newBossPressureActionDirector = null)
        {
            Unsubscribe();
            bossBarrageEmitter = newEmitter;
            bossPressureActionDirector = newBossPressureActionDirector;
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

        private void HandlePressureActionQueued(
            BossPressureActionDirector director,
            BossPressureActionKind actionKind,
            BossBarragePatternProfile pattern,
            int spentTier)
        {
            ActionCameraCueProfile.CameraCue cue = ResolvePressureActionCue(actionKind);
            if (RequestCue(cue, ResolveBossDirection(bossBarrageEmitter), ResolvePressureActionScale(spentTier, cue)))
            {
                pressureActionCueRequestCount++;
                lastPressureActionKind = actionKind;
                lastPressureActionTier = Mathf.Clamp(spentTier, 1, 3);
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
            SubscribeBarrageEmitter();
            SubscribePressureActionSource();
        }

        private void SubscribeBarrageEmitter()
        {
            if (subscribed || bossBarrageEmitter == null)
            {
                return;
            }

            bossBarrageEmitter.WindupStarted += HandleWindupStarted;
            bossBarrageEmitter.WaveFired += HandleWaveFired;
            subscribed = true;
        }

        private void SubscribePressureActionSource()
        {
            if (pressureActionSubscribed || bossPressureActionDirector == null)
            {
                return;
            }

            bossPressureActionDirector.ActionQueued += HandlePressureActionQueued;
            pressureActionSubscribed = true;
        }

        private void Unsubscribe()
        {
            UnsubscribeBarrageEmitter();
            UnsubscribePressureActionSource();
        }

        private void UnsubscribeBarrageEmitter()
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

        private void UnsubscribePressureActionSource()
        {
            if (!pressureActionSubscribed || bossPressureActionDirector == null)
            {
                pressureActionSubscribed = false;
                return;
            }

            bossPressureActionDirector.ActionQueued -= HandlePressureActionQueued;
            pressureActionSubscribed = false;
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

        private ActionCameraCueProfile.CameraCue ResolvePressureActionCue(BossPressureActionKind actionKind)
        {
            return actionKind switch
            {
                BossPressureActionKind.SummonPressure => pressureSummonCue,
                BossPressureActionKind.PunishOverextend => pressurePunishCue,
                _ => pressureSkillCue
            };
        }

        private static float ResolvePressureActionScale(int spentTier, ActionCameraCueProfile.CameraCue cue)
        {
            float tierWeight = Mathf.Clamp01((Mathf.Clamp(spentTier, 1, 3) - 1) / 2f);
            return Mathf.Lerp(1f, cue.finisherScale, tierWeight);
        }

        private static float ResolveProjectileScale(int projectileCount, ActionCameraCueProfile.CameraCue cue)
        {
            float pressureWeight = Mathf.Clamp01((Mathf.Max(1, projectileCount) - 1) / 6f);
            return Mathf.Lerp(1f, cue.finisherScale, pressureWeight);
        }
    }
}
