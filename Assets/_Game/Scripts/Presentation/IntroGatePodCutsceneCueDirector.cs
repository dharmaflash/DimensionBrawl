using System;
using Unity.Cinemachine;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class IntroGatePodCutsceneCueDirector : MonoBehaviour
    {
        [Serializable]
        public struct DollyCue
        {
            [SerializeField] private string cueId;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0.01f)] private float durationSeconds;
            [SerializeField] private CinemachineSplineDolly dolly;
            [SerializeField] private float fromPosition;
            [SerializeField] private float toPosition;

            public DollyCue(
                string cueId,
                float startSeconds,
                float durationSeconds,
                CinemachineSplineDolly dolly,
                float fromPosition,
                float toPosition)
            {
                this.cueId = cueId;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
                this.dolly = dolly;
                this.fromPosition = fromPosition;
                this.toPosition = toPosition;
            }

            public string CueId => cueId;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public CinemachineSplineDolly Dolly => dolly;
            public float FromPosition => fromPosition;
            public float ToPosition => toPosition;

            public void Apply(float elapsedSeconds)
            {
                if (dolly == null)
                {
                    return;
                }

                float t = Mathf.InverseLerp(StartSeconds, EndSeconds, Mathf.Max(0f, elapsedSeconds));
                t = t * t * (3f - (2f * t));
                dolly.CameraPosition = Mathf.Lerp(fromPosition, toPosition, t);
            }
        }

        [Serializable]
        public struct VoiceCue
        {
            [SerializeField] private string cueId;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField] private AudioSource audioSource;
            [SerializeField, HideInInspector] private bool played;

            public VoiceCue(string cueId, float startSeconds, AudioSource audioSource)
            {
                this.cueId = cueId;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.audioSource = audioSource;
                played = false;
            }

            public string CueId => cueId;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public AudioSource AudioSource => audioSource;
            public bool Played => played;

            public void ResetPlayback()
            {
                played = false;
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.time = 0f;
                }
            }

            public void Apply(float elapsedSeconds, bool allowPlayback)
            {
                if (!allowPlayback || played || audioSource == null || elapsedSeconds < StartSeconds)
                {
                    return;
                }

                audioSource.Play();
                played = true;
            }
        }

        [Serializable]
        public struct FadeCue
        {
            [SerializeField] private string cueId;
            [SerializeField, Min(0f)] private float startSeconds;
            [SerializeField, Min(0.01f)] private float durationSeconds;
            [SerializeField, Range(0f, 1f)] private float fromAlpha;
            [SerializeField, Range(0f, 1f)] private float toAlpha;

            public FadeCue(string cueId, float startSeconds, float durationSeconds, float fromAlpha, float toAlpha)
            {
                this.cueId = cueId;
                this.startSeconds = Mathf.Max(0f, startSeconds);
                this.durationSeconds = Mathf.Max(0.01f, durationSeconds);
                this.fromAlpha = Mathf.Clamp01(fromAlpha);
                this.toAlpha = Mathf.Clamp01(toAlpha);
            }

            public string CueId => cueId;
            public float StartSeconds => Mathf.Max(0f, startSeconds);
            public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
            public float EndSeconds => StartSeconds + DurationSeconds;
            public float FromAlpha => Mathf.Clamp01(fromAlpha);
            public float ToAlpha => Mathf.Clamp01(toAlpha);

            public float Evaluate(float elapsedSeconds)
            {
                if (elapsedSeconds < StartSeconds || elapsedSeconds > EndSeconds)
                {
                    return 0f;
                }

                float t = Mathf.InverseLerp(StartSeconds, EndSeconds, elapsedSeconds);
                t = t * t * (3f - (2f * t));
                return Mathf.Lerp(FromAlpha, ToAlpha, t);
            }
        }

        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool useUnscaledClock = true;
        [SerializeField] private DollyCue[] dollyCues = Array.Empty<DollyCue>();
        [SerializeField] private VoiceCue[] voiceCues = Array.Empty<VoiceCue>();
        [SerializeField] private FadeCue[] fadeCues = Array.Empty<FadeCue>();

        private float elapsedSeconds;
        private bool playing;
        private float currentFadeAlpha;

        public DollyCue[] DollyCues => dollyCues ?? Array.Empty<DollyCue>();
        public VoiceCue[] VoiceCues => voiceCues ?? Array.Empty<VoiceCue>();
        public FadeCue[] FadeCues => fadeCues ?? Array.Empty<FadeCue>();
        public float CurrentFadeAlpha => currentFadeAlpha;
        public float ElapsedSeconds => elapsedSeconds;

        public void Configure(
            DollyCue[] newDollyCues,
            VoiceCue[] newVoiceCues,
            FadeCue[] newFadeCues,
            bool newPlayOnStart,
            bool newUseUnscaledClock)
        {
            dollyCues = newDollyCues ?? Array.Empty<DollyCue>();
            voiceCues = newVoiceCues ?? Array.Empty<VoiceCue>();
            fadeCues = newFadeCues ?? Array.Empty<FadeCue>();
            playOnStart = newPlayOnStart;
            useUnscaledClock = newUseUnscaledClock;
            ResetVoicePlayback();
            ApplySampleForReview(0f);
        }

        private void Awake()
        {
            ResetVoicePlayback();
            elapsedSeconds = 0f;
            playing = playOnStart;
            currentFadeAlpha = 0f;
            if (playing)
            {
                ApplyCues(0f, false);
            }
        }

        private void Update()
        {
            if (!playing)
            {
                return;
            }

            elapsedSeconds += useUnscaledClock ? Time.unscaledDeltaTime : Time.deltaTime;
            ApplyCues(elapsedSeconds, true);
        }

        public void Play()
        {
            elapsedSeconds = 0f;
            playing = true;
            ResetVoicePlayback();
            ApplyCues(0f, false);
        }

        public void Stop()
        {
            playing = false;
        }

        public void ApplySampleForReview(float sampleSeconds)
        {
            elapsedSeconds = Mathf.Max(0f, sampleSeconds);
            ApplyCues(elapsedSeconds, false);
        }

        private void ApplyCues(float sampleSeconds, bool allowVoicePlayback)
        {
            DollyCue[] resolvedDollyCues = DollyCues;
            for (int i = 0; i < resolvedDollyCues.Length; i++)
            {
                resolvedDollyCues[i].Apply(sampleSeconds);
            }

            VoiceCue[] resolvedVoiceCues = VoiceCues;
            for (int i = 0; i < resolvedVoiceCues.Length; i++)
            {
                VoiceCue cue = resolvedVoiceCues[i];
                cue.Apply(sampleSeconds, allowVoicePlayback);
                resolvedVoiceCues[i] = cue;
            }

            FadeCue[] resolvedFadeCues = FadeCues;
            float alpha = 0f;
            for (int i = 0; i < resolvedFadeCues.Length; i++)
            {
                alpha = Mathf.Max(alpha, resolvedFadeCues[i].Evaluate(sampleSeconds));
            }

            currentFadeAlpha = alpha;
        }

        private void ResetVoicePlayback()
        {
            VoiceCue[] resolvedVoiceCues = VoiceCues;
            for (int i = 0; i < resolvedVoiceCues.Length; i++)
            {
                VoiceCue cue = resolvedVoiceCues[i];
                cue.ResetPlayback();
                resolvedVoiceCues[i] = cue;
            }
        }
    }
}
