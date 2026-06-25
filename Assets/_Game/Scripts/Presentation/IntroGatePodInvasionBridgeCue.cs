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
            [SerializeField] private float startSeconds;
            [SerializeField] private float endSeconds;
            [SerializeField] private Vector3 startLocalPosition;
            [SerializeField] private Vector3 endLocalPosition;
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
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.endSeconds = Mathf.Max(this.startSeconds + 0.01f, endSeconds);
                this.startLocalPosition = startLocalPosition;
                this.endLocalPosition = endLocalPosition;
                this.localEulerAngles = localEulerAngles;
                this.normalizedTimeOffset = normalizedTimeOffset;
            }

            public Transform Root => root;
            public Animator Animator => animator;
            public string RunStateName => runStateName;
            public float StartSeconds => startSeconds;
            public float EndSeconds => endSeconds;
            public Vector3 StartLocalPosition => startLocalPosition;
            public Vector3 EndLocalPosition => endLocalPosition;
            public Vector3 LocalEulerAngles => localEulerAngles;
            public float NormalizedTimeOffset => normalizedTimeOffset;
        }

        [Header("Timeline")]
        [SerializeField] private PlayableDirector director;

        [Header("Commandos")]
        [SerializeField] private CommandoCue[] commandos = Array.Empty<CommandoCue>();

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
            SampleCommandos(elapsedSeconds);
            SampleExplosion(elapsedSeconds);
            SampleScreenImpact(elapsedSeconds);
            SampleCameraImpact(elapsedSeconds);
        }

        private float ResolveTimelineTime()
        {
            return director != null ? (float)director.time : 0f;
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

                float normalized = Mathf.InverseLerp(cue.StartSeconds, cue.EndSeconds, elapsedSeconds);
                float runCycle = Mathf.Repeat((elapsedSeconds - cue.StartSeconds) * 1.35f + cue.NormalizedTimeOffset, 1f);
                float strideBob = active ? Mathf.Abs(Mathf.Sin(runCycle * Mathf.PI * 2f)) * 0.026f : 0f;
                root.localPosition = Vector3.Lerp(cue.StartLocalPosition, cue.EndLocalPosition, normalized)
                    + (Vector3.up * strideBob);
                root.localRotation = Quaternion.Euler(cue.LocalEulerAngles);

                Animator animator = cue.Animator;
                if (active && animator != null && !string.IsNullOrWhiteSpace(cue.RunStateName))
                {
                    animator.Play(cue.RunStateName, 0, runCycle);
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
            float startSeconds = explosionStartSeconds - warningSweepLeadSeconds;
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
            float leadSeconds = 0.055f;
            float recoverSeconds = 0.46f;
            float startSeconds = explosionStartSeconds - leadSeconds;
            float endSeconds = explosionStartSeconds + recoverSeconds;
            if (elapsedSeconds < startSeconds || elapsedSeconds > endSeconds)
            {
                return 0f;
            }

            if (elapsedSeconds <= explosionStartSeconds)
            {
                float windup = Mathf.InverseLerp(startSeconds, explosionStartSeconds, elapsedSeconds);
                return Mathf.Lerp(0.18f, impactFlashPeakAlpha, windup);
            }

            float recovery = Mathf.InverseLerp(explosionStartSeconds, endSeconds, elapsedSeconds);
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

            float shake = ResolveCameraShakeWeight(elapsedSeconds);
            if (!cameraBaseCaptured || !Application.isPlaying)
            {
                cameraBasePosition = impactCamera.transform.position;
                cameraBaseRotation = impactCamera.transform.rotation;
                cameraBaseCaptured = true;
            }

            if (shake <= 0.0001f)
            {
                if (Application.isPlaying)
                {
                    impactCamera.transform.SetPositionAndRotation(cameraBasePosition, cameraBaseRotation);
                }

                return;
            }

            float localTime = Mathf.Max(0f, elapsedSeconds - explosionStartSeconds);
            float x = (Mathf.PerlinNoise(localTime * 18.0f, 0.17f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0.37f, localTime * 21.0f) - 0.5f) * 2f;
            float z = (Mathf.PerlinNoise(localTime * 11.0f, 0.73f) - 0.5f) * 2f;
            Vector3 localOffset = Vector3.Scale(cameraShakePositionAmplitude, new Vector3(x, y, z)) * shake;
            Vector3 eulerOffset = Vector3.Scale(cameraShakeEulerAmplitude, new Vector3(y, x, z)) * shake;
            Quaternion shakeRotation = Quaternion.Euler(eulerOffset);
            impactCamera.transform.SetPositionAndRotation(
                cameraBasePosition + (cameraBaseRotation * localOffset),
                shakeRotation * cameraBaseRotation);
        }

        private float ResolveCameraShakeWeight(float elapsedSeconds)
        {
            if (elapsedSeconds < explosionStartSeconds)
            {
                return 0f;
            }

            float t = Mathf.InverseLerp(
                explosionStartSeconds,
                explosionStartSeconds + cameraShakeDurationSeconds,
                elapsedSeconds);
            if (t <= 0f || t >= 1f)
            {
                return 0f;
            }

            float decay = 1f - (t * t * (3f - (2f * t)));
            return Mathf.Sin(t * Mathf.PI * 7.5f) * decay;
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
