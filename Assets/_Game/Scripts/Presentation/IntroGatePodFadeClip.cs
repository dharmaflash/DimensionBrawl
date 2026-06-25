using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [Serializable]
    public sealed class IntroGatePodFadeClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField, Range(0f, 1f)] private float fromAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float toAlpha;

        public float FromAlpha
        {
            get => Mathf.Clamp01(fromAlpha);
            set => fromAlpha = Mathf.Clamp01(value);
        }

        public float ToAlpha
        {
            get => Mathf.Clamp01(toAlpha);
            set => toAlpha = Mathf.Clamp01(value);
        }

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<IntroGatePodFadeBehaviour> playable =
                ScriptPlayable<IntroGatePodFadeBehaviour>.Create(graph);
            IntroGatePodFadeBehaviour behaviour = playable.GetBehaviour();
            behaviour.FromAlpha = FromAlpha;
            behaviour.ToAlpha = ToAlpha;
            return playable;
        }
    }

    public sealed class IntroGatePodFadeBehaviour : PlayableBehaviour
    {
        public float FromAlpha { get; set; }
        public float ToAlpha { get; set; }

        public float Evaluate(Playable playable)
        {
            double duration = playable.GetDuration();
            float t = duration > 0.0001 ? (float)(playable.GetTime() / duration) : 1f;
            t = Mathf.Clamp01(t);
            t = t * t * (3f - (2f * t));
            return Mathf.Lerp(Mathf.Clamp01(FromAlpha), Mathf.Clamp01(ToAlpha), t);
        }
    }
}
