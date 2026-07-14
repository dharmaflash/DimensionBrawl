namespace DimensionBrawl.UI
{
    public interface ICombatBossHudStatus
    {
        bool BossHudVisible { get; }
        float BossHealthFillAmount { get; }
    }
}
