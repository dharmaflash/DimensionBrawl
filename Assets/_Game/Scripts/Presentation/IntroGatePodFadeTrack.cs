using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [TrackClipType(typeof(IntroGatePodFadeClip))]
    [TrackBindingType(typeof(IntroGatePodTimelineFadeOverlay))]
    [TrackColor(0.05f, 0.05f, 0.05f)]
    public sealed class IntroGatePodFadeTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<IntroGatePodFadeMixer>.Create(graph, inputCount);
        }
    }

    public sealed class IntroGatePodFadeMixer : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            IntroGatePodTimelineFadeOverlay overlay = playerData as IntroGatePodTimelineFadeOverlay;
            if (overlay == null)
            {
                return;
            }

            float totalWeight = 0f;
            float alpha = 0f;
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0.001f)
                {
                    continue;
                }

                ScriptPlayable<IntroGatePodFadeBehaviour> input =
                    (ScriptPlayable<IntroGatePodFadeBehaviour>)playable.GetInput(i);
                alpha += input.GetBehaviour().Evaluate(input) * weight;
                totalWeight += weight;
            }

            overlay.Alpha = totalWeight > 0.001f ? alpha / totalWeight : 0f;
        }
    }
}
