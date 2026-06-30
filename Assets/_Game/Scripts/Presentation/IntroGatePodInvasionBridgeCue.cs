using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX;

namespace DimensionBrawl.Presentation
{
    [ExecuteAlways]
    public sealed class IntroGatePodInvasionBridgeCue : MonoBehaviour
    {
        [Serializable]
        public struct CommandoCue
        {
            [SerializeField] private Transform root;
            [SerializeField] private Animator animator;
            [SerializeField] private string runStateName;
            [SerializeField] private string attackStateName;
            [SerializeField] private string hitStateName;
            [SerializeField] private float startSeconds;
            [SerializeField] private float attackStartSeconds;
            [SerializeField] private float hitStartSeconds;
            [SerializeField] private float endSeconds;
            [SerializeField] private Vector3 startLocalPosition;
            [SerializeField] private Vector3 endLocalPosition;
            [SerializeField] private Vector3 hitLocalPositionOffset;
            [SerializeField] private Vector3 localEulerAngles;
            [SerializeField] private float normalizedTimeOffset;

            public CommandoCue(
                Transform root,
                Animator animator,
                string runStateName,
                float startSeconds,
                float endSeconds,
                Vector3 startLocalPosition,
                Vector3 endLocalPosition,
                Vector3 localEulerAngles,
                float normalizedTimeOffset)
            {
                this.root = root;
                this.animator = animator;
                this.runStateName = runStateName;
                attackStateName = string.Empty;
                hitStateName = string.Empty;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                attackStartSeconds = Mathf.Max(this.startSeconds, endSeconds);
                hitStartSeconds = Mathf.Max(this.startSeconds, endSeconds);
                this.endSeconds = Mathf.Max(this.startSeconds + 0.01f, endSeconds);
                this.startLocalPosition = startLocalPosition;
                this.endLocalPosition = endLocalPosition;
                hitLocalPositionOffset = Vector3.zero;
                this.localEulerAngles = localEulerAngles;
                this.normalizedTimeOffset = normalizedTimeOffset;
            }

            public CommandoCue(
                Transform root,
                Animator animator,
                string runStateName,
                string attackStateName,
                string hitStateName,
                float startSeconds,
                float attackStartSeconds,
                float hitStartSeconds,
                float endSeconds,
                Vector3 startLocalPosition,
                Vector3 endLocalPosition,
                Vector3 hitLocalPositionOffset,
                Vector3 localEulerAngles,
                float normalizedTimeOffset)
                : this(
                    root,
                    animator,
                    runStateName,
                    startSeconds,
                    endSeconds,
                    startLocalPosition,
                    endLocalPosition,
                    localEulerAngles,
                    normalizedTimeOffset)
            {
                this.attackStateName = attackStateName ?? string.Empty;
                this.hitStateName = hitStateName ?? string.Empty;
                this.attackStartSeconds = Mathf.Clamp(attackStartSeconds, this.startSeconds, this.endSeconds);
                this.hitStartSeconds = Mathf.Clamp(hitStartSeconds, this.startSeconds, this.endSeconds);
                this.hitLocalPositionOffset = hitLocalPositionOffset;
            }

            public Transform Root => root;
            public Animator Animator => animator;
            public string RunStateName => runStateName;
            public string AttackStateName => attackStateName;
            public string HitStateName => hitStateName;
            public float StartSeconds => startSeconds;
            public float AttackStartSeconds => attackStartSeconds;
            public float HitStartSeconds => hitStartSeconds;
            public float EndSeconds => endSeconds;
            public Vector3 StartLocalPosition => startLocalPosition;
            public Vector3 EndLocalPosition => endLocalPosition;
            public Vector3 HitLocalPositionOffset => hitLocalPositionOffset;
            public Vector3 LocalEulerAngles => localEulerAngles;
            public float NormalizedTimeOffset => normalizedTimeOffset;
        }

        [Serializable]
        public struct TimedObjectCue
        {
            [SerializeField] private Transform root;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0f)] private float endSeconds;
            [SerializeField] private Vector3 startLocalPosition;
            [SerializeField] private Vector3 endLocalPosition;
            [SerializeField] private Vector3 localEulerAngles;
            [SerializeField] private Vector3 startLocalScale;
            [SerializeField] private Vector3 endLocalScale;
            [SerializeField] private bool pulseScale;
            [SerializeField, Min(0f)] private float pulseScaleAmplitude;

            public TimedObjectCue(
                Transform root,
                float startSeconds,
                float endSeconds,
                Vector3 startLocalPosition,
                Vector3 endLocalPosition,
                Vector3 localEulerAngles,
                Vector3 startLocalScale,
                Vector3 endLocalScale,
                bool pulseScale = false,
                float pulseScaleAmplitude = 0f)
            {
                this.root = root;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.endSeconds = Mathf.Max(this.startSeconds + 0.01f, endSeconds);
                this.startLocalPosition = startLocalPosition;
                this.endLocalPosition = endLocalPosition;
                this.localEulerAngles = localEulerAngles;
                this.startLocalScale = startLocalScale;
                this.endLocalScale = endLocalScale;
                this.pulseScale = pulseScale;
                this.pulseScaleAmplitude = Mathf.Max(0f, pulseScaleAmplitude);
            }

            public Transform Root => root;
            public float StartSeconds => startSeconds;
            public float EndSeconds => endSeconds;
            public Vector3 StartLocalPosition => startLocalPosition;
            public Vector3 EndLocalPosition => endLocalPosition;
            public Vector3 LocalEulerAngles => localEulerAngles;
            public Vector3 StartLocalScale => startLocalScale;
            public Vector3 EndLocalScale => endLocalScale;
            public bool PulseScale => pulseScale;
            public float PulseScaleAmplitude => pulseScaleAmplitude;
        }

        [Header("Timeline")]
        [SerializeField] private PlayableDirector director;

        [Header("Commandos")]
        [SerializeField] private CommandoCue[] commandos = Array.Empty<CommandoCue>();
        [SerializeField, Min(0f)] private float commandoStrideBobHeight = 0f;

        [Header("Timed Objects")]
        [SerializeField] private TimedObjectCue[] timedObjects = Array.Empty<TimedObjectCue>();

        [Header("Background Explosion")]
        [SerializeField] private GameObject explosionRoot;
        [SerializeField] private Light explosionLight;
        [SerializeField, Min(0f)] private float explosionStartSeconds;
        [SerializeField, Min(0.01f)] private float explosionDurationSeconds = 1.2f;
        [SerializeField, Min(0f)] private float explosionAfterSmokeSeconds = 2.4f;
        [SerializeField] private Vector3 explosionRestScale = Vector3.one * 0.1f;
        [SerializeField] private Vector3 explosionPeakScale = Vector3.one * 1.8f;
        [SerializeField, Min(0f)] private float explosionPeakLightIntensity = 5.5f;

        [Header("Screen Impact")]
        [SerializeField] private CanvasGroup impactFlashGroup;
        [SerializeField] private CanvasGroup warningSweepGroup;
        [SerializeField] private float[] impactCueSeconds = Array.Empty<float>();
        [SerializeField, Min(0f)] private float warningSweepLeadSeconds = 0.42f;
        [SerializeField, Min(0f)] private float warningSweepDurationSeconds = 0.56f;
        [SerializeField, Range(0f, 1f)] private float impactFlashPeakAlpha = 0.72f;

        [Header("Camera Impact")]
        [SerializeField] private Camera impactCamera;
        [SerializeField] private Vector3 cameraShakePositionAmplitude = new Vector3(0.045f, 0.036f, 0f);
        [SerializeField] private Vector3 cameraShakeEulerAmplitude = new Vector3(1.35f, 1.80f, 0.55f);
        [SerializeField, Min(0.01f)] private float cameraShakeDurationSeconds = 0.72f;

        private bool explosionWasActive;
        private float currentImpactFlashAlpha;
        private float currentWarningSweepAlpha;
        private bool cameraBaseCaptured;
        private Vector3 cameraBasePosition;
        private Quaternion cameraBaseRotation;

        public CommandoCue[] Commandos => commandos;
        public TimedObjectCue[] TimedObjects => timedObjects ?? Array.Empty<TimedObjectCue>();
        public GameObject ExplosionRoot => explosionRoot;
        public float CurrentImpactFlashAlpha => currentImpactFlashAlpha;
        public float CurrentWarningSweepAlpha => currentWarningSweepAlpha;

        public void Configure(
            PlayableDirector newDirector,
            CommandoCue[] newCommandos,
            GameObject newExplosionRoot,
            Light newExplosionLight,
            float newExplosionStartSeconds,
            float newExplosionDurationSeconds,
            Vector3 newExplosionRestScale,
            Vector3 newExplosionPeakScale,
            float newExplosionPeakLightIntensity)
        {
            director = newDirector;
            commandos = newCommandos ?? Array.Empty<CommandoCue>();
            explosionRoot = newExplosionRoot;
            explosionLight = newExplosionLight;
            explosionStartSeconds = Mathf.Max(0f, newExplosionStartSeconds);
            explosionDurationSeconds = Mathf.Max(0.01f, newExplosionDurationSeconds);
            explosionRestScale = newExplosionRestScale;
            explosionPeakScale = newExplosionPeakScale;
            explosionPeakLightIntensity = Mathf.Max(0f, newExplosionPeakLightIntensity);
            Sample(0f);
        }

        public void ConfigureTimedObjects(TimedObjectCue[] newTimedObjects, float[] newImpactCueSeconds)
        {
            timedObjects = newTimedObjects ?? Array.Empty<TimedObjectCue>();
            impactCueSeconds = newImpactCueSeconds ?? Array.Empty<float>();
            Sample(0f);
        }

        public void ConfigurePresentation(
            Camera newImpactCamera,
            CanvasGroup newImpactFlashGroup,
            CanvasGroup newWarningSweepGroup,
            float newExplosionAfterSmokeSeconds,
            float newWarningSweepLeadSeconds,
            float newWarningSweepDurationSeconds,
            float newImpactFlashPeakAlpha,
            Vector3 newCameraShakePositionAmplitude,
            Vector3 newCameraShakeEulerAmplitude,
            float newCameraShakeDurationSeconds)
        {
            impactCamera = newImpactCamera;
            impactFlashGroup = newImpactFlashGroup;
            warningSweepGroup = newWarningSweepGroup;
            explosionAfterSmokeSeconds = Mathf.Max(0f, newExplosionAfterSmokeSeconds);
            warningSweepLeadSeconds = Mathf.Max(0f, newWarningSweepLeadSeconds);
            warningSweepDurationSeconds = Mathf.Max(0.01f, newWarningSweepDurationSeconds);
            impactFlashPeakAlpha = Mathf.Clamp01(newImpactFlashPeakAlpha);
            cameraShakePositionAmplitude = newCameraShakePositionAmplitude;
            cameraShakeEulerAmplitude = newCameraShakeEulerAmplitude;
            cameraShakeDurationSeconds = Mathf.Max(0.01f, newCameraShakeDurationSeconds);
            cameraBaseCaptured = false;
            Sample(0f);
        }

        private void OnEnable()
        {
            Sample(ResolveTimelineTime());
        }

        private void Update()
        {
            Sample(ResolveTimelineTime());
        }

        public void Sample(float elapsedSeconds)
        {
            SampleTimedObjects(elapsedSeconds);
            SampleCommandos(elapsedSeconds);
            SampleExplosion(elapsedSeconds);
            SampleScreenImpact(elapsedSeconds);
            SampleCameraImpact(elapsedSeconds);
        }

        private float ResolveTimelineTime()
        {
            return director != null ? (float)director.time : 0f;
        }

        private void SampleTimedObjects(float elapsedSeconds)
        {
            TimedObjectCue[] cues = TimedObjects;
            for (int i = 0; i < cues.Length; i++)
            {
                TimedObjectCue cue = cues[i];
                Transform root = cue.Root;
                if (root == null)
                {
                    continue;
                }

                bool active = elapsedSeconds >= cue.StartSeconds && elapsedSeconds <= cue.EndSeconds;
                if (root.gameObject.activeSelf != active)
                {
                    root.gameObject.SetActive(active);
                }

                float normalized = Mathf.InverseLerp(cue.StartSeconds, cue.EndSeconds, elapsedSeconds);
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalized));
                root.localPosition = Vector3.LerpUnclamped(cue.StartLocalPosition, cue.EndLocalPosition, eased);
                root.localRotation = Quaternion.Euler(cue.LocalEulerAngles);

                Vector3 scale = Vector3.Lerp(cue.StartLocalScale, cue.EndLocalScale, eased);
                if (active && cue.PulseScale && cue.PulseScaleAmplitude > 0f)
                {
                    float pulse = Mathf.Sin(Mathf.Clamp01(normalized) * Mathf.PI * 2.5f) * cue.PulseScaleAmplitude;
                    scale += Vector3.one * pulse;
                }

                root.localScale = MaxScale(Vector3.zero, scale);
            }
        }

        private void SampleCommandos(float elapsedSeconds)
        {
            for (int i = 0; i < commandos.Length; i++)
            {
                CommandoCue cue = commandos[i];
                Transform root = cue.Root;
                if (root == null)
                {
                    continue;
                }

                bool active = elapsedSeconds >= cue.StartSeconds && elapsedSeconds <= cue.EndSeconds;
                if (root.gameObject.activeSelf != active)
                {
                    root.gameObject.SetActive(active);
                }

                bool hasAttackState = !string.IsNullOrWhiteSpace(cue.AttackStateName);
                float moveEndSeconds = hasAttackState && cue.AttackStartSeconds > cue.StartSeconds
                    ? cue.AttackStartSeconds
                    : cue.EndSeconds;
                float normalized = Mathf.InverseLerp(cue.StartSeconds, moveEndSeconds, elapsedSeconds);
                float runCycle = Mathf.Repeat((elapsedSeconds - cue.StartSeconds) * 1.35f + cue.NormalizedTimeOffset, 1f);
                bool hitActive = active
                    && !string.IsNullOrWhiteSpace(cue.HitStateName)
                    && elapsedSeconds >= cue.HitStartSeconds;
                float strideBob = active && !hitActive && commandoStrideBobHeight > 0f
                    ? Mathf.Abs(Mathf.Sin(runCycle * Mathf.PI * 2f)) * commandoStrideBobHeight
                    : 0f;
                Vector3 hitOffset = hitActive
                    ? cue.HitLocalPositionOffset * Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(cue.HitStartSeconds, cue.EndSeconds, elapsedSeconds))
                    : Vector3.zero;
                root.localPosition = Vector3.Lerp(cue.StartLocalPosition, cue.EndLocalPosition, normalized)
                    + (Vector3.up * strideBob)
                    + hitOffset;
                root.localRotation = Quaternion.Euler(cue.LocalEulerAngles);

                Animator animator = cue.Animator;
                if (active && animator != null)
                {
                    string stateName = ResolveCommandoStateName(cue, elapsedSeconds, out float stateNormalizedTime, runCycle);
                    if (!string.IsNullOrWhiteSpace(stateName))
                    {
                        animator.Play(stateName, 0, stateNormalizedTime);
                    }

                    animator.Update(0f);
                }
            }
        }

        private void SampleExplosion(float elapsedSeconds)
        {
            if (explosionRoot == null)
            {
                return;
            }

            float burstEndSeconds = explosionStartSeconds + explosionDurationSeconds;
            float endSeconds = burstEndSeconds + explosionAfterSmokeSeconds;
            bool active = elapsedSeconds >= explosionStartSeconds && elapsedSeconds <= endSeconds;
            if (explosionRoot.activeSelf != active)
            {
                explosionRoot.SetActive(active);
            }

            ParticleSystem[] particleSystems = explosionRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            VisualEffect[] visualEffects = explosionRoot.GetComponentsInChildren<VisualEffect>(includeInactive: true);
            if (!active)
            {
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    particleSystems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                for (int i = 0; i < visualEffects.Length; i++)
                {
                    visualEffects[i].Stop();
                }

                explosionWasActive = false;
                return;
            }

            float burstNormalized = Mathf.InverseLerp(explosionStartSeconds, burstEndSeconds, elapsedSeconds);
            float smokeNormalized = Mathf.InverseLerp(burstEndSeconds, endSeconds, elapsedSeconds);
            float pulse = Mathf.Sin(Mathf.Clamp01(burstNormalized) * Mathf.PI);
            float afterSmokeScale = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(smokeNormalized));
            explosionRoot.transform.localScale = Vector3.Lerp(
                explosionRestScale,
                explosionPeakScale + (Vector3.one * 0.42f * afterSmokeScale),
                Mathf.Max(pulse, afterSmokeScale * 0.38f));

            if (explosionLight != null)
            {
                float afterglow = Mathf.Exp(-Mathf.Clamp01(smokeNormalized) * 3.4f) * 0.22f;
                explosionLight.intensity = explosionPeakLightIntensity * Mathf.Max(pulse, afterglow);
            }

            float localTime = Mathf.Max(0f, elapsedSeconds - explosionStartSeconds);
            bool deterministicSample = !Application.isPlaying;
            if (!explosionWasActive || deterministicSample)
            {
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    particleSystems[i].Clear(false);
                    particleSystems[i].Play(false);
                }

                for (int i = 0; i < visualEffects.Length; i++)
                {
                    visualEffects[i].Reinit();
                    visualEffects[i].Play();
                }
            }

            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Simulate(localTime, false, true, true);
            }

            if (deterministicSample)
            {
                uint stepCount = (uint)Mathf.Max(1, Mathf.CeilToInt(localTime * 60f));
                for (int i = 0; i < visualEffects.Length; i++)
                {
                    visualEffects[i].Simulate(1f / 60f, stepCount);
                }
            }

            explosionWasActive = true;
        }

        private void SampleScreenImpact(float elapsedSeconds)
        {
            currentWarningSweepAlpha = ResolveWarningSweepAlpha(elapsedSeconds);
            currentImpactFlashAlpha = ResolveImpactFlashAlpha(elapsedSeconds);
            ApplyCanvasGroupAlpha(warningSweepGroup, currentWarningSweepAlpha);
            ApplyCanvasGroupAlpha(impactFlashGroup, currentImpactFlashAlpha);
        }

        private float ResolveWarningSweepAlpha(float elapsedSeconds)
        {
            float alpha = ResolveWarningSweepAlphaAt(elapsedSeconds, explosionStartSeconds);
            float[] cues = impactCueSeconds ?? Array.Empty<float>();
            for (int i = 0; i < cues.Length; i++)
            {
                alpha = Mathf.Max(alpha, ResolveWarningSweepAlphaAt(elapsedSeconds, cues[i]));
            }

            return alpha;
        }

        private float ResolveWarningSweepAlphaAt(float elapsedSeconds, float impactSeconds)
        {
            float startSeconds = impactSeconds - warningSweepLeadSeconds;
            float endSeconds = startSeconds + warningSweepDurationSeconds;
            if (elapsedSeconds < startSeconds || elapsedSeconds > endSeconds)
            {
                return 0f;
            }

            float t = Mathf.InverseLerp(startSeconds, endSeconds, elapsedSeconds);
            return Mathf.Sin(t * Mathf.PI) * 0.42f;
        }

        private float ResolveImpactFlashAlpha(float elapsedSeconds)
        {
            float alpha = ResolveImpactFlashAlphaAt(elapsedSeconds, explosionStartSeconds);
            float[] cues = impactCueSeconds ?? Array.Empty<float>();
            for (int i = 0; i < cues.Length; i++)
            {
                alpha = Mathf.Max(alpha, ResolveImpactFlashAlphaAt(elapsedSeconds, cues[i]));
            }

            return alpha;
        }

        private float ResolveImpactFlashAlphaAt(float elapsedSeconds, float impactSeconds)
        {
            float leadSeconds = 0.055f;
            float recoverSeconds = 0.46f;
            float startSeconds = impactSeconds - leadSeconds;
            float endSeconds = impactSeconds + recoverSeconds;
            if (elapsedSeconds < startSeconds || elapsedSeconds > endSeconds)
            {
                return 0f;
            }

            if (elapsedSeconds <= impactSeconds)
            {
                float windup = Mathf.InverseLerp(startSeconds, impactSeconds, elapsedSeconds);
                return Mathf.Lerp(0.18f, impactFlashPeakAlpha, windup);
            }

            float recovery = Mathf.InverseLerp(impactSeconds, endSeconds, elapsedSeconds);
            recovery = recovery * recovery;
            return Mathf.Lerp(impactFlashPeakAlpha, 0f, recovery);
        }

        private void SampleCameraImpact(float elapsedSeconds)
        {
            if (impactCamera == null)
            {
                cameraBaseCaptured = false;
                return;
            }

            if (!cameraBaseCaptured || !Application.isPlaying)
            {
                cameraBasePosition = impactCamera.transform.position;
                cameraBaseRotation = impactCamera.transform.rotation;
                cameraBaseCaptured = true;
            }

            Vector3 localOffset = Vector3.zero;
            Vector3 eulerOffset = Vector3.zero;
            float totalShake = AccumulateCameraShake(elapsedSeconds, explosionStartSeconds, ref localOffset, ref eulerOffset);
            float[] cues = impactCueSeconds ?? Array.Empty<float>();
            for (int i = 0; i < cues.Length; i++)
            {
                totalShake += AccumulateCameraShake(elapsedSeconds, cues[i], ref localOffset, ref eulerOffset);
            }

            if (totalShake <= 0.0001f)
            {
                if (Application.isPlaying)
                {
                    impactCamera.transform.SetPositionAndRotation(cameraBasePosition, cameraBaseRotation);
                }

                return;
            }

            impactCamera.transform.SetPositionAndRotation(
                cameraBasePosition + (cameraBaseRotation * localOffset),
                Quaternion.Euler(eulerOffset) * cameraBaseRotation);
        }

        private static string ResolveCommandoStateName(
            CommandoCue cue,
            float elapsedSeconds,
            out float stateNormalizedTime,
            float runCycle)
        {
            if (!string.IsNullOrWhiteSpace(cue.HitStateName) && elapsedSeconds >= cue.HitStartSeconds)
            {
                stateNormalizedTime = Mathf.Clamp01(
                    Mathf.InverseLerp(cue.HitStartSeconds, cue.EndSeconds, elapsedSeconds));
                return cue.HitStateName;
            }

            if (!string.IsNullOrWhiteSpace(cue.AttackStateName) && elapsedSeconds >= cue.AttackStartSeconds)
            {
                stateNormalizedTime = Mathf.Repeat(
                    Mathf.InverseLerp(cue.AttackStartSeconds, cue.EndSeconds, elapsedSeconds),
                    1f);
                return cue.AttackStateName;
            }

            stateNormalizedTime = runCycle;
            return cue.RunStateName;
        }

        private float AccumulateCameraShake(
            float elapsedSeconds,
            float impactSeconds,
            ref Vector3 localOffset,
            ref Vector3 eulerOffset)
        {
            float shake = ResolveCameraShakeWeight(elapsedSeconds, impactSeconds);
            if (shake <= 0.0001f)
            {
                return 0f;
            }

            float localTime = Mathf.Max(0f, elapsedSeconds - impactSeconds);
            float x = (Mathf.PerlinNoise(localTime * 18.0f, 0.17f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0.37f, localTime * 21.0f) - 0.5f) * 2f;
            float z = (Mathf.PerlinNoise(localTime * 11.0f, 0.73f) - 0.5f) * 2f;
            localOffset += Vector3.Scale(cameraShakePositionAmplitude, new Vector3(x, y, z)) * shake;
            eulerOffset += Vector3.Scale(cameraShakeEulerAmplitude, new Vector3(y, x, z)) * shake;
            return shake;
        }

        private float ResolveCameraShakeWeight(float elapsedSeconds, float impactSeconds)
        {
            if (elapsedSeconds < impactSeconds)
            {
                return 0f;
            }

            float t = Mathf.InverseLerp(
                impactSeconds,
                impactSeconds + cameraShakeDurationSeconds,
                elapsedSeconds);
            if (t <= 0f || t >= 1f)
            {
                return 0f;
            }

            float decay = 1f - (t * t * (3f - (2f * t)));
            return Mathf.Sin(t * Mathf.PI * 7.5f) * decay;
        }

        private static Vector3 MaxScale(Vector3 min, Vector3 value)
        {
            return new Vector3(
                Mathf.Max(min.x, value.x),
                Mathf.Max(min.y, value.y),
                Mathf.Max(min.z, value.z));
        }

        private static void ApplyCanvasGroupAlpha(CanvasGroup group, float alpha)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = Mathf.Clamp01(alpha);
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}
