using System;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;

namespace DimensionBrawl.UI
{
    public enum CombatSessionOverlayMode
    {
        Hidden,
        Pause,
        Settings,
        Failure,
        Routing
    }

    public interface ICombatSessionOverlay
    {
        CombatSessionOverlayMode Mode { get; }
        bool IsVisible { get; }

        event Action<bool> CombatInputBlockChanged;

        void Configure(
            BossBarrageEncounterController resultSource,
            ActionScreenCuePresenter screenCuePresenter);

        void ShowPause();
        void ShowSettings();
        void ShowFailure();
        void Resume();
        void DismissForStageClear();
    }
}
