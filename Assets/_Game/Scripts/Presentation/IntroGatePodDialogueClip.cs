using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [Serializable]
    public sealed class IntroGatePodDialogueClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] private string speakerName = string.Empty;
        [SerializeField, TextArea(2, 4)] private string dialogueText = string.Empty;
        [SerializeField, Min(0f)] private float fadeInSeconds = 0.12f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.12f;
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 1f;

        public string SpeakerName
        {
            get => speakerName ?? string.Empty;
            set => speakerName = value ?? string.Empty;
        }

        public string DialogueText
        {
            get => dialogueText ?? string.Empty;
            set => dialogueText = value ?? string.Empty;
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

        public float MaxAlpha
        {
            get => Mathf.Clamp01(maxAlpha);
            set => maxAlpha = Mathf.Clamp01(value);
        }

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<IntroGatePodDialogueBehaviour> playable =
                ScriptPlayable<IntroGatePodDialogueBehaviour>.Create(graph);
            IntroGatePodDialogueBehaviour behaviour = playable.GetBehaviour();
            behaviour.SpeakerName = SpeakerName;
            behaviour.DialogueText = DialogueText;
            behaviour.FadeInSeconds = FadeInSeconds;
            behaviour.FadeOutSeconds = FadeOutSeconds;
            behaviour.MaxAlpha = MaxAlpha;
            return playable;
        }
    }

    public sealed class IntroGatePodDialogueBehaviour : PlayableBehaviour
    {
        public string SpeakerName { get; set; } = string.Empty;
        public string DialogueText { get; set; } = string.Empty;
        public float FadeInSeconds { get; set; } = 0.12f;
        public float FadeOutSeconds { get; set; } = 0.12f;
        public float MaxAlpha { get; set; } = 1f;

        public float EvaluateAlpha(Playable playable)
        {
            double duration = playable.GetDuration();
            if (duration <= 0.0001d)
            {
                return Mathf.Clamp01(MaxAlpha);
            }

            float time = Mathf.Clamp((float)playable.GetTime(), 0f, (float)duration);
            float resolvedDuration = Mathf.Max(0.0001f, (float)duration);
            float fadeIn = Mathf.Min(Mathf.Max(0f, FadeInSeconds), resolvedDuration * 0.5f);
            float fadeOut = Mathf.Min(Mathf.Max(0f, FadeOutSeconds), resolvedDuration * 0.5f);
            float alpha = Mathf.Clamp01(MaxAlpha);

            if (fadeIn > 0.0001f && time < fadeIn)
            {
                alpha *= Smooth01(time / fadeIn);
            }

            float remaining = resolvedDuration - time;
            if (fadeOut > 0.0001f && remaining < fadeOut)
            {
                alpha *= Smooth01(remaining / fadeOut);
            }

            return Mathf.Clamp01(alpha);
        }

        private static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - (2f * t));
        }
    }
}
