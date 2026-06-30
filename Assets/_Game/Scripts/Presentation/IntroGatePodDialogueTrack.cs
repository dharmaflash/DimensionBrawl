using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DimensionBrawl.Presentation
{
    [TrackClipType(typeof(IntroGatePodDialogueClip))]
    [TrackBindingType(typeof(IntroGatePodDialogueOverlay))]
    [TrackColor(0.16f, 0.43f, 0.82f)]
    public sealed class IntroGatePodDialogueTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<IntroGatePodDialogueMixer>.Create(graph, inputCount);
        }
    }

    public sealed class IntroGatePodDialogueMixer : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            IntroGatePodDialogueOverlay overlay = playerData as IntroGatePodDialogueOverlay;
            if (overlay == null)
            {
                return;
            }

            float bestWeight = 0f;
            float bestAlpha = 0f;
            string speakerName = string.Empty;
            string dialogueText = string.Empty;
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0.001f)
                {
                    continue;
                }

                ScriptPlayable<IntroGatePodDialogueBehaviour> input =
                    (ScriptPlayable<IntroGatePodDialogueBehaviour>)playable.GetInput(i);
                IntroGatePodDialogueBehaviour behaviour = input.GetBehaviour();
                float alpha = behaviour.EvaluateAlpha(input) * weight;
                if (alpha >= bestWeight)
                {
                    bestWeight = alpha;
                    bestAlpha = alpha;
                    speakerName = behaviour.SpeakerName;
                    dialogueText = behaviour.DialogueText;
                }
            }

            if (bestWeight <= 0.001f)
            {
                overlay.Clear();
                return;
            }

            overlay.Apply(speakerName, dialogueText, bestAlpha);
        }
    }
}
