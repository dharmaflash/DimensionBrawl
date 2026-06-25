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
        [SerializeField] private Vector3 explosionRestScale = Vector3.one * 0.1f;
        [SerializeField] private Vector3 explosionPeakScale = Vector3.one * 1.8f;
        [SerializeField, Min(0f)] private float explosionPeakLightIntensity = 5.5f;

        private bool explosionWasActive;

        public CommandoCue[] Commandos => commandos;
        public GameObject ExplosionRoot => explosionRoot;

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
                root.localPosition = Vector3.Lerp(cue.StartLocalPosition, cue.EndLocalPosition, normalized);
                root.localRotation = Quaternion.Euler(cue.LocalEulerAngles);

                Animator animator = cue.Animator;
                if (active && animator != null && !string.IsNullOrWhiteSpace(cue.RunStateName))
                {
                    float runTime = Mathf.Repeat((elapsedSeconds - cue.StartSeconds) * 1.35f + cue.NormalizedTimeOffset, 1f);
                    animator.Play(cue.RunStateName, 0, runTime);
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

            float endSeconds = explosionStartSeconds + explosionDurationSeconds;
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

            float normalized = Mathf.InverseLerp(explosionStartSeconds, endSeconds, elapsedSeconds);
            float pulse = Mathf.Sin(Mathf.Clamp01(normalized) * Mathf.PI);
            explosionRoot.transform.localScale = Vector3.Lerp(explosionRestScale, explosionPeakScale, pulse);

            if (explosionLight != null)
            {
                explosionLight.intensity = explosionPeakLightIntensity * pulse;
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
    }
}
