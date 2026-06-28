using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [TrackClipType(typeof(IntroGatePodLetterboxClip))]
    [TrackBindingType(typeof(IntroGatePodLetterboxOverlay))]
    [TrackColor(0.02f, 0.02f, 0.02f)]
    public sealed class IntroGatePodLetterboxTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<IntroGatePodLetterboxMixer>.Create(graph, inputCount);
        }
    }

    public sealed class IntroGatePodLetterboxMixer : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            IntroGatePodLetterboxOverlay overlay = playerData as IntroGatePodLetterboxOverlay;
            if (overlay == null)
            {
                return;
            }

            float totalWeight = 0f;
            float amount = 0f;
            float alpha = 0f;
            float barHeight = 0f;

            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0.001f)
                {
                    continue;
                }

                ScriptPlayable<IntroGatePodLetterboxBehaviour> input =
                    (ScriptPlayable<IntroGatePodLetterboxBehaviour>)playable.GetInput(i);
                IntroGatePodLetterboxFrame frame = input.GetBehaviour().Evaluate(input);
                amount += frame.Amount * weight;
                alpha += frame.Alpha * weight;
                barHeight += frame.BarHeight * weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0.001f)
            {
                overlay.Clear();
                return;
            }

            overlay.Apply(amount / totalWeight, alpha / totalWeight, barHeight / totalWeight);
        }
    }
}
