using DimensionBrawl.Player;

namespace DimensionBrawl.UI
{
    public interface ICombatMoveInputGate
    {
        bool IsInputBlocked { get; }
        bool IsPointerHeld { get; }

        void SetInputBlocked(PlayerInputLockSource source, bool blocked);
    }
}
