using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed partial class ActionCinematicCueDirector
    {
        private void DispatchDueSignals(
            ActionCinematicCueProfile.CueSequence sequence,
            bool[] signalPlayed,
            float sequenceElapsed,
            int tier,
            Vector3 planarDirection)
        {
            if (sequence.signals == null || sequence.signals.Length == 0 || signalPlayed == null)
            {
                return;
            }

            for (int i = 0; i < sequence.signals.Length; i++)
            {
                if (signalPlayed[i])
                {
                    continue;
                }

                ActionCinematicCueProfile.CueSignal signal = sequence.signals[i];
                if (!signal.enabled || sequenceElapsed < Mathf.Max(0f, signal.delaySeconds))
                {
                    continue;
                }

                signalPlayed[i] = true;
                DispatchSignal(signal, tier, planarDirection);
            }
        }

        private void DispatchSignal(ActionCinematicCueProfile.CueSignal signal, int tier, Vector3 planarDirection)
        {
            totalSignalCount++;
            lastSignalId = signal.signalId;

            if (TriggerAnimator(signal.animatorTrigger))
            {
                animatorTriggerRequestCount++;
                lastAnimatorTrigger = signal.animatorTrigger;
            }
            else if (signal.requireAnimatorTrigger)
            {
                return;
            }

            if (!signal.playVfx || cuePlayer == null)
            {
                return;
            }

            Transform anchor = vfxAnchor != null ? vfxAnchor : (cueSpace != null ? cueSpace : transform);
            float tierWeight = Mathf.Clamp01((Mathf.Max(1, tier) - 1) / 2f);
            float tierIntensityScale = signal.tierIntensityScale > 0f ? signal.tierIntensityScale : 1f;
            float intensity = Mathf.Max(0.01f, signal.vfxIntensity) * Mathf.Lerp(1f, tierIntensityScale, tierWeight);
            if (cuePlayer.PlayCue(signal.vfxCueId, anchor, planarDirection, intensity))
            {
                vfxCueRequestCount++;
                lastVfxCueId = signal.vfxCueId;
            }
        }

        private bool TriggerAnimator(string triggerName)
        {
            if (cueAnimator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return false;
            }

            if (!HasAnimatorTrigger(triggerName))
            {
                return false;
            }

            cueAnimator.SetTrigger(triggerName);
            return true;
        }

        private bool HasAnimatorTrigger(string triggerName)
        {
            if (cueAnimator.runtimeAnimatorController == null)
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = cueAnimator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, triggerName, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
