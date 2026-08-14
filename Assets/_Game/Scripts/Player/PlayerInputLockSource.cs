using System;

namespace DimensionBrawl.Player
{
    [Flags]
    public enum PlayerInputLockSource
    {
        None = 0,
        CombatHudAim = 1 << 0,
        CorridorTutorial = 1 << 2,
        StationEntryGuide = 1 << 3,
        CinematicCue = 1 << 4,
        EditorVerification = 1 << 5,
        CorridorCombatFlow = 1 << 6,
        CombatMenu = 1 << 7,
        BossPhaseTransition = 1 << 8
    }

    public static class PlayerInputLockMask
    {
        public static PlayerInputLockSource WithState(
            PlayerInputLockSource current,
            PlayerInputLockSource source,
            bool locked)
        {
            int sourceValue = (int)source;
            if (sourceValue <= 0 || (sourceValue & (sourceValue - 1)) != 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(source),
                    source,
                    "An input lock operation must identify exactly one owner.");
            }

            return locked ? current | source : current & ~source;
        }
    }
}
