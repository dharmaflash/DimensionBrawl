using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public sealed partial class ActionCinematicCueDirector
    {
        private void ResolveCueSpaceReferences()
        {
            if (cueSpace == null)
            {
                return;
            }

            if (movement == null)
            {
                movement = cueSpace.GetComponent<PlayerMovementController>();
            }

            if (actionController == null)
            {
                actionController = cueSpace.GetComponent<PlayerActionController>();
            }

            if (skill1Action == null)
            {
                skill1Action = cueSpace.GetComponent<PlayerSkill1Action>();
            }

            if (summonSlot1Action == null)
            {
                summonSlot1Action = cueSpace.GetComponent<PlayerSummonSlot1Action>();
            }

            if (rangedBasicAttackAction == null)
            {
                rangedBasicAttackAction = cueSpace.GetComponent<PlayerRangedBasicAttackAction>();
            }

            if (cuePlayer == null)
            {
                cuePlayer = cueSpace.GetComponent<CombatVfxCuePlayer>();
            }

            if (cueAnimator == null)
            {
                cueAnimator = cueSpace.GetComponentInChildren<Animator>(includeInactive: true);
            }
        }

        private void ApplyMovementLock()
        {
            if (movementLockActive)
            {
                return;
            }

            movement?.SetCinematicMoveInputSpeedScale(0f);
            movementLockActive = movement != null;
        }

        private void ReleaseMovementLock()
        {
            if (!movementLockActive)
            {
                return;
            }

            movement?.ClearCinematicMoveInputSpeedScale();
            movementLockActive = false;
        }

        private void ApplyInputLock()
        {
            if (inputLockActive)
            {
                return;
            }

            actionController?.SetCinematicInputLocked(true);
            skill1Action?.SetCinematicInputLocked(true);
            summonSlot1Action?.SetCinematicInputLocked(true);
            rangedBasicAttackAction?.SetCinematicInputLocked(true);
            inputLockActive = actionController != null
                || skill1Action != null
                || summonSlot1Action != null
                || rangedBasicAttackAction != null;
        }

        private void ReleaseInputLock()
        {
            if (!inputLockActive)
            {
                return;
            }

            actionController?.SetCinematicInputLocked(false);
            skill1Action?.SetCinematicInputLocked(false);
            summonSlot1Action?.SetCinematicInputLocked(false);
            rangedBasicAttackAction?.SetCinematicInputLocked(false);
            inputLockActive = false;
        }

        private float TickMovementLockTimer(float timer, float deltaTime)
        {
            if (!movementLockActive || timer <= 0f)
            {
                return timer;
            }

            timer = Mathf.Max(0f, timer - Mathf.Max(0f, deltaTime));
            if (timer <= 0f)
            {
                ReleaseMovementLock();
            }

            return timer;
        }

        private float TickInputLockTimer(float timer, float deltaTime)
        {
            if (!inputLockActive || timer <= 0f)
            {
                return timer;
            }

            timer = Mathf.Max(0f, timer - Mathf.Max(0f, deltaTime));
            if (timer <= 0f)
            {
                ReleaseInputLock();
            }

            return timer;
        }

        private void RestoreCinematicState()
        {
            ReleaseMovementLock();
            ReleaseInputLock();
            RestoreTimeScale();
        }

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

            if (!signal.playVfx || cuePlayer == null)
            {
                return;
            }

            Transform anchor = vfxAnchor != null ? vfxAnchor : (cueSpace != null ? cueSpace : transform);
            float tierWeight = Mathf.Clamp01((Mathf.Max(1, tier) - 1) / 2f);
            float intensity = Mathf.Max(0.01f, signal.vfxIntensity) * Mathf.Lerp(1f, 1.18f, tierWeight);
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
