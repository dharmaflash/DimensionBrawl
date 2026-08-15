using System;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageCameraCueDriver : MonoBehaviour
    {
        [Serializable]
        public struct PatternWindupCueOverride
        {
            [SerializeField] private string patternId;
            [SerializeField] private ActionCameraCueProfile.CameraCue cue;

            public PatternWindupCueOverride(
                string patternId,
                ActionCameraCueProfile.CameraCue cue)
            {
                this.patternId = patternId;
                this.cue = cue;
            }

            public string PatternId => patternId;
            public ActionCameraCueProfile.CameraCue Cue => cue;
        }

        [Header("References")]
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossPressureActionDirector bossPressureActionDirector;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Transform cueSpace;

        [Header("Pattern Windup Overrides")]
        [Tooltip("Full-strength hold before a short release. Pattern overrides use this sustained product-camera envelope; generic cues keep their original decay.")]
        [SerializeField, Min(0.01f)] private float patternWindupCueReleaseSeconds = 0.18f;
        [Tooltip("Optional exact patternId matches. A matching windup composition remains authoritative over that pattern's ordinary fire cue while the camera request is still active.")]
        [SerializeField] private PatternWindupCueOverride[] patternWindupCueOverrides =
            Array.Empty<PatternWindupCueOverride>();

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
        private int patternWindupOverrideRequestCount;
        private int preservedPatternFireCueCount;
        private BossPressureActionKind lastPressureActionKind;
        private int lastPressureActionTier;
        private string activePatternWindupOverrideId;
        private int activePatternWindupCameraCueVersion = -1;

        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public BossPressureActionDirector BossPressureActionDirector => bossPressureActionDirector;
        public ActionCameraController CameraController => cameraController;
        public Transform CueSpace => cueSpace;
        public int WindupCueRequestCount => windupCueRequestCount;
        public int FireCueRequestCount => fireCueRequestCount;
        public int PressureActionCueRequestCount => pressureActionCueRequestCount;
        public int PatternWindupOverrideRequestCount => patternWindupOverrideRequestCount;
        public int PreservedPatternFireCueCount => preservedPatternFireCueCount;
        public string ActivePatternWindupOverrideId => activePatternWindupOverrideId;
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

        public void ConfigurePatternWindupCueOverrides(
            float releaseSeconds,
            params PatternWindupCueOverride[] overrides)
        {
            patternWindupCueReleaseSeconds = Mathf.Max(0.01f, releaseSeconds);
            patternWindupCueOverrides = overrides != null
                ? (PatternWindupCueOverride[])overrides.Clone()
                : Array.Empty<PatternWindupCueOverride>();
            ClearActivePatternWindupOverride();
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
            ClearActivePatternWindupOverride();
        }

        private void HandleWindupStarted(BossBarrageEmitter emitter, BossBarragePatternProfile pattern)
        {
            ClearActivePatternWindupOverride();
            bool usesPatternOverride = TryResolvePatternWindupCue(pattern, out ActionCameraCueProfile.CameraCue cue);
            if (!usesPatternOverride)
            {
                cue = windupCue;
            }

            if (RequestCue(
                cue,
                ResolveBossDirection(emitter),
                ResolvePatternScale(pattern, cue),
                usesPatternOverride))
            {
                windupCueRequestCount++;
                if (usesPatternOverride)
                {
                    patternWindupOverrideRequestCount++;
                    activePatternWindupOverrideId = pattern.PatternId;
                    activePatternWindupCameraCueVersion = cameraController.CueRequestVersion;
                }
            }
        }

        private void HandleWaveFired(BossBarrageEmitter emitter, BossBarragePatternProfile pattern, int spawnedCount)
        {
            if (ShouldPreservePatternWindupOverride(pattern))
            {
                preservedPatternFireCueCount++;
                return;
            }

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

        private bool RequestCue(
            ActionCameraCueProfile.CameraCue cue,
            Vector3 planarDirection,
            float scale,
            bool sustainAtFullWeight = false)
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
            if (sustainAtFullWeight)
            {
                cameraController.RequestSustainedCue(
                    offset * clampedScale,
                    cue.durationSeconds,
                    patternWindupCueReleaseSeconds,
                    cue.fieldOfViewDelta * clampedScale,
                    cue.cameraDistanceDelta * clampedScale,
                    cue.focusHeightDelta * clampedScale);
            }
            else
            {
                cameraController.RequestCue(
                    offset * clampedScale,
                    cue.durationSeconds,
                    cue.fieldOfViewDelta * clampedScale,
                    cue.cameraDistanceDelta * clampedScale,
                    cue.focusHeightDelta * clampedScale);
            }

            return true;
        }

        private bool TryResolvePatternWindupCue(
            BossBarragePatternProfile pattern,
            out ActionCameraCueProfile.CameraCue cue)
        {
            cue = default;
            string patternId = pattern != null ? pattern.PatternId : null;
            if (string.IsNullOrWhiteSpace(patternId) || patternWindupCueOverrides == null)
            {
                return false;
            }

            for (int index = 0; index < patternWindupCueOverrides.Length; index++)
            {
                PatternWindupCueOverride candidate = patternWindupCueOverrides[index];
                if (!string.Equals(candidate.PatternId, patternId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                cue = candidate.Cue;
                return true;
            }

            return false;
        }

        private bool ShouldPreservePatternWindupOverride(BossBarragePatternProfile pattern)
        {
            if (pattern == null
                || string.IsNullOrEmpty(activePatternWindupOverrideId)
                || !string.Equals(
                    activePatternWindupOverrideId,
                    pattern.PatternId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            bool remainsActive = cameraController != null
                && cameraController.HasActiveCue
                && cameraController.CueRequestVersion == activePatternWindupCameraCueVersion;
            if (!remainsActive)
            {
                ClearActivePatternWindupOverride();
            }

            return remainsActive;
        }

        private void ClearActivePatternWindupOverride()
        {
            activePatternWindupOverrideId = null;
            activePatternWindupCameraCueVersion = -1;
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
