using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [Serializable]
    public sealed class IntroGatePodFirstPersonBootClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField, Min(0f)] private float glitchFadeInSeconds = 0.045f;
        [SerializeField, Min(0f)] private float glitchHoldSeconds = 0.18f;
        [SerializeField, Min(0f)] private float glitchFadeOutSeconds = 0.55f;
        [SerializeField, Range(0f, 1f)] private float glitchMaxAlpha = 0.42f;
        [SerializeField, Min(0f)] private float glitchStrength = 1f;
        [SerializeField, Min(0f)] private float hudDelaySeconds = 0.10f;
        [SerializeField, Min(0f)] private float hudOpenSeconds = 0.24f;
        [SerializeField, Min(0f)] private float hudHoldSeconds = 0.34f;
        [SerializeField, Min(0f)] private float hudFadeOutSeconds = 0.34f;
        [SerializeField, Range(0f, 1f)] private float hudMaxAlpha = 0.62f;
        [SerializeField, Min(0f)] private float statusBarMaxWidth = 430f;
        [SerializeField, Min(0f)] private float statusBarThickness = 2f;

        public float GlitchFadeInSeconds
        {
            get => Mathf.Max(0f, glitchFadeInSeconds);
            set => glitchFadeInSeconds = Mathf.Max(0f, value);
        }

        public float GlitchHoldSeconds
        {
            get => Mathf.Max(0f, glitchHoldSeconds);
            set => glitchHoldSeconds = Mathf.Max(0f, value);
        }

        public float GlitchFadeOutSeconds
        {
            get => Mathf.Max(0f, glitchFadeOutSeconds);
            set => glitchFadeOutSeconds = Mathf.Max(0f, value);
        }

        public float GlitchMaxAlpha
        {
            get => Mathf.Clamp01(glitchMaxAlpha);
            set => glitchMaxAlpha = Mathf.Clamp01(value);
        }

        public float GlitchStrength
        {
            get => Mathf.Max(0f, glitchStrength);
            set => glitchStrength = Mathf.Max(0f, value);
        }

        public float HudDelaySeconds
        {
            get => Mathf.Max(0f, hudDelaySeconds);
            set => hudDelaySeconds = Mathf.Max(0f, value);
        }

        public float HudOpenSeconds
        {
            get => Mathf.Max(0f, hudOpenSeconds);
            set => hudOpenSeconds = Mathf.Max(0f, value);
        }

        public float HudHoldSeconds
        {
            get => Mathf.Max(0f, hudHoldSeconds);
            set => hudHoldSeconds = Mathf.Max(0f, value);
        }

        public float HudFadeOutSeconds
        {
            get => Mathf.Max(0f, hudFadeOutSeconds);
            set => hudFadeOutSeconds = Mathf.Max(0f, value);
        }

        public float HudMaxAlpha
        {
            get => Mathf.Clamp01(hudMaxAlpha);
            set => hudMaxAlpha = Mathf.Clamp01(value);
        }

        public float StatusBarMaxWidth
        {
            get => Mathf.Max(0f, statusBarMaxWidth);
            set => statusBarMaxWidth = Mathf.Max(0f, value);
        }

        public float StatusBarThickness
        {
            get => Mathf.Max(0f, statusBarThickness);
            set => statusBarThickness = Mathf.Max(0f, value);
        }

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<IntroGatePodFirstPersonBootBehaviour> playable =
                ScriptPlayable<IntroGatePodFirstPersonBootBehaviour>.Create(graph);
            IntroGatePodFirstPersonBootBehaviour behaviour = playable.GetBehaviour();
            behaviour.GlitchFadeInSeconds = GlitchFadeInSeconds;
            behaviour.GlitchHoldSeconds = GlitchHoldSeconds;
            behaviour.GlitchFadeOutSeconds = GlitchFadeOutSeconds;
            behaviour.GlitchMaxAlpha = GlitchMaxAlpha;
            behaviour.GlitchStrength = GlitchStrength;
            behaviour.HudDelaySeconds = HudDelaySeconds;
            behaviour.HudOpenSeconds = HudOpenSeconds;
            behaviour.HudHoldSeconds = HudHoldSeconds;
            behaviour.HudFadeOutSeconds = HudFadeOutSeconds;
            behaviour.HudMaxAlpha = HudMaxAlpha;
            behaviour.StatusBarMaxWidth = StatusBarMaxWidth;
            behaviour.StatusBarThickness = StatusBarThickness;
            return playable;
        }
    }

    public sealed class IntroGatePodFirstPersonBootBehaviour : PlayableBehaviour
    {
        public float GlitchFadeInSeconds { get; set; }
        public float GlitchHoldSeconds { get; set; }
        public float GlitchFadeOutSeconds { get; set; }
        public float GlitchMaxAlpha { get; set; }
        public float GlitchStrength { get; set; }
        public float HudDelaySeconds { get; set; }
        public float HudOpenSeconds { get; set; }
        public float HudHoldSeconds { get; set; }
        public float HudFadeOutSeconds { get; set; }
        public float HudMaxAlpha { get; set; }
        public float StatusBarMaxWidth { get; set; }
        public float StatusBarThickness { get; set; }

        public IntroGatePodFirstPersonBootFrame Evaluate(Playable playable)
        {
            float time = Mathf.Max(0f, (float)playable.GetTime());
            float glitchAlpha = EvaluateEnvelope(
                time,
                0f,
                GlitchFadeInSeconds,
                GlitchHoldSeconds,
                GlitchFadeOutSeconds) * Mathf.Clamp01(GlitchMaxAlpha);

            float hudStart = Mathf.Max(0f, HudDelaySeconds);
            float hudAlpha = EvaluateEnvelope(
                time,
                hudStart,
                HudOpenSeconds,
                HudHoldSeconds,
                HudFadeOutSeconds) * Mathf.Clamp01(HudMaxAlpha);
            float hudOpenAmount = Smooth01(Mathf.InverseLerp(hudStart, hudStart + Mathf.Max(0.0001f, HudOpenSeconds), time));
            float phase = time * 13.73f;

            return new IntroGatePodFirstPersonBootFrame(
                glitchAlpha,
                Mathf.Max(0f, GlitchStrength) * Mathf.Clamp01(glitchAlpha * 2.4f),
                hudAlpha,
                hudOpenAmount,
                phase,
                Mathf.Max(0f, StatusBarMaxWidth),
                Mathf.Max(0f, StatusBarThickness));
        }

        private static float EvaluateEnvelope(float time, float start, float fadeIn, float hold, float fadeOut)
        {
            if (time < start)
            {
                return 0f;
            }

            float localTime = time - start;
            fadeIn = Mathf.Max(0f, fadeIn);
            hold = Mathf.Max(0f, hold);
            fadeOut = Mathf.Max(0f, fadeOut);
            float sustainStart = fadeIn;
            float sustainEnd = sustainStart + hold;
            float end = sustainEnd + fadeOut;

            if (fadeIn > 0.0001f && localTime < sustainStart)
            {
                return Smooth01(localTime / fadeIn);
            }

            if (localTime <= sustainEnd)
            {
                return 1f;
            }

            if (fadeOut > 0.0001f && localTime <= end)
            {
                return Smooth01(1f - ((localTime - sustainEnd) / fadeOut));
            }

            return 0f;
        }

        private static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - (2f * t));
        }
    }

    public readonly struct IntroGatePodFirstPersonBootFrame
    {
        public IntroGatePodFirstPersonBootFrame(
            float glitchAlpha,
            float glitchStrength,
            float hudAlpha,
            float hudOpenAmount,
            float phase,
            float statusBarMaxWidth,
            float statusBarThickness)
        {
            GlitchAlpha = glitchAlpha;
            GlitchStrength = glitchStrength;
            HudAlpha = hudAlpha;
            HudOpenAmount = hudOpenAmount;
            Phase = phase;
            StatusBarMaxWidth = statusBarMaxWidth;
            StatusBarThickness = statusBarThickness;
        }

        public float GlitchAlpha { get; }
        public float GlitchStrength { get; }
        public float HudAlpha { get; }
        public float HudOpenAmount { get; }
        public float Phase { get; }
        public float StatusBarMaxWidth { get; }
        public float StatusBarThickness { get; }
    }
}
