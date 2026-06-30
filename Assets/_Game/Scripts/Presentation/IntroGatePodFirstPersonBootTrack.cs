using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [TrackClipType(typeof(IntroGatePodFirstPersonBootClip))]
    [TrackBindingType(typeof(IntroGatePodFirstPersonBootOverlay))]
    [TrackColor(0.15f, 0.82f, 0.92f)]
    public sealed class IntroGatePodFirstPersonBootTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<IntroGatePodFirstPersonBootMixer>.Create(graph, inputCount);
        }
    }

    public sealed class IntroGatePodFirstPersonBootMixer : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            IntroGatePodFirstPersonBootOverlay overlay = playerData as IntroGatePodFirstPersonBootOverlay;
            if (overlay == null)
            {
                return;
            }

            bool hasFrame = false;
            IntroGatePodFirstPersonBootFrame bestFrame = default;
            float bestAlpha = 0f;
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0.001f)
                {
                    continue;
                }

                ScriptPlayable<IntroGatePodFirstPersonBootBehaviour> input =
                    (ScriptPlayable<IntroGatePodFirstPersonBootBehaviour>)playable.GetInput(i);
                IntroGatePodFirstPersonBootFrame frame = input.GetBehaviour().Evaluate(input);
                float alpha = Mathf.Max(frame.GlitchAlpha, frame.HudAlpha) * weight;
                if (alpha >= bestAlpha)
                {
                    bestAlpha = alpha;
                    bestFrame = new IntroGatePodFirstPersonBootFrame(
                        frame.GlitchAlpha * weight,
                        frame.GlitchStrength * weight,
                        frame.HudAlpha * weight,
                        frame.HudOpenAmount,
                        frame.Phase,
                        frame.StatusBarMaxWidth,
                        frame.StatusBarThickness);
                    hasFrame = true;
                }
            }

            if (!hasFrame || bestAlpha <= 0.001f)
            {
                overlay.Clear();
                return;
            }

            overlay.Apply(bestFrame);
        }
    }
}
