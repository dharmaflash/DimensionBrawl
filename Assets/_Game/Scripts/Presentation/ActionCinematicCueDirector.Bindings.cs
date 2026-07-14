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

            movement?.SetCinematicMoveInputLocked(PlayerInputLockSource.CinematicCue, true);
            movementLockActive = movement != null;
        }

        private void ReleaseMovementLock()
        {
            if (!movementLockActive)
            {
                return;
            }

            movement?.SetCinematicMoveInputLocked(PlayerInputLockSource.CinematicCue, false);
            movementLockActive = false;
        }

        private void ApplyInputLock()
        {
            if (inputLockActive)
            {
                return;
            }

            actionController?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, true);
            skill1Action?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, true);
            summonSlot1Action?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, true);
            rangedBasicAttackAction?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, true);
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

            actionController?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, false);
            skill1Action?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, false);
            summonSlot1Action?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, false);
            rangedBasicAttackAction?.SetCinematicInputLocked(PlayerInputLockSource.CinematicCue, false);
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
    }
}
