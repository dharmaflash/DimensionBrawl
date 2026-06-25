namespace IsekaiBrawl.Gameplay
{
    public enum BattleMode
    {
        StoryPve,
        AsyncPvp,
        Sandbox
    }

    public static class BattleModeContext
    {
        public static BattleMode CurrentMode { get; private set; } = BattleMode.StoryPve;

        public static void SetMode(BattleMode mode)
        {
            CurrentMode = mode;
        }

        public static string GetDisplayName()
        {
            return CurrentMode switch
            {
                BattleMode.StoryPve => "\uC2A4\uD1A0\uB9AC \uC804\uD22C",
                BattleMode.AsyncPvp => "\uBE44\uB3D9\uAE30 \uB300\uC804",
                BattleMode.Sandbox => "\uC804\uD22C \uD14C\uC2A4\uD2B8",
                _ => "\uC804\uD22C"
            };
        }
    }
}
