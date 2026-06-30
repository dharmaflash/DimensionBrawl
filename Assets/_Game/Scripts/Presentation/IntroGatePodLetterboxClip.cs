using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [Serializable]
    public sealed class IntroGatePodLetterboxClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField, Min(0f)] private float barHeight = 39.333332f;
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.96f;
        [SerializeField, Min(0f)] private float fadeInSeconds = 0.55f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.45f;
        [SerializeField] private bool fadeOutAtClipEnd = true;

        public float BarHeight
        {
            get => Mathf.Max(0f, barHeight);
            set => barHeight = Mathf.Max(0f, value);
        }

        public float MaxAlpha
        {
            get => Mathf.Clamp01(maxAlpha);
            set => maxAlpha = Mathf.Clamp01(value);
        }

        public float FadeInSeconds
        {
            get => Mathf.Max(0f, fadeInSeconds);
            set => fadeInSeconds = Mathf.Max(0f, value);
        }

        public float FadeOutSeconds
        {
            get => Mathf.Max(0f, fadeOutSeconds);
            set => fadeOutSeconds = Mathf.Max(0f, value);
        }

        public bool FadeOutAtClipEnd
        {
            get => fadeOutAtClipEnd;
            set => fadeOutAtClipEnd = value;
        }

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<IntroGatePodLetterboxBehaviour> playable =
                ScriptPlayable<IntroGatePodLetterboxBehaviour>.Create(graph);
            IntroGatePodLetterboxBehaviour behaviour = playable.GetBehaviour();
            behaviour.BarHeight = BarHeight;
            behaviour.MaxAlpha = MaxAlpha;
            behaviour.FadeInSeconds = FadeInSeconds;
            behaviour.FadeOutSeconds = FadeOutSeconds;
            behaviour.FadeOutAtClipEnd = FadeOutAtClipEnd;
            return playable;
        }
    }

    public sealed class IntroGatePodLetterboxBehaviour : PlayableBehaviour
    {
        public float BarHeight { get; set; }
        public float MaxAlpha { get; set; }
        public float FadeInSeconds { get; set; }
        public float FadeOutSeconds { get; set; }
        public bool FadeOutAtClipEnd { get; set; }

        public IntroGatePodLetterboxFrame Evaluate(Playable playable)
        {
            double duration = playable.GetDuration();
            double time = playable.GetTime();
            float amount = 1f;

            if (FadeInSeconds > 0.0001f)
            {
                amount *= Smooth01((float)(time / FadeInSeconds));
            }

            if (FadeOutAtClipEnd && FadeOutSeconds > 0.0001f && duration > 0.0001d)
            {
                amount *= Smooth01((float)((duration - time) / FadeOutSeconds));
            }

            amount = Mathf.Clamp01(amount);
            return new IntroGatePodLetterboxFrame(amount, Mathf.Clamp01(MaxAlpha) * amount, Mathf.Max(0f, BarHeight));
        }

        private static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - (2f * t));
        }
    }

    public readonly struct IntroGatePodLetterboxFrame
    {
        public IntroGatePodLetterboxFrame(float amount, float alpha, float barHeight)
        {
            Amount = amount;
            Alpha = alpha;
            BarHeight = barHeight;
        }

        public float Amount { get; }
        public float Alpha { get; }
        public float BarHeight { get; }
    }
}
