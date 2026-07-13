namespace DimensionBrawl.Combat
{
    public readonly struct BossBarrageSummonSlotBalance
    {
        public BossBarrageSummonSlotBalance(
            string actionName,
            string hudLabel,
            int minimumTier,
            float requiredMana,
            float cooldownSeconds)
        {
            ActionName = actionName;
            HudLabel = hudLabel;
            MinimumTier = minimumTier;
            RequiredMana = requiredMana;
            CooldownSeconds = cooldownSeconds;
        }

        public string ActionName { get; }
        public string HudLabel { get; }
        public int MinimumTier { get; }
        public float RequiredMana { get; }
        public float CooldownSeconds { get; }
    }

    public static class BossBarrageSummonBalance
    {
        public const bool UseSingleSummonButton = false;
        public const string LockedSummonLabel = "NEXT";

        public const string Slot1ActionName = "SummonSlot1";
        public const string Slot1HudLabel = "SUMMON";
        public const int Slot1MinimumTier = 2;
        public const float Slot1RequiredMana = 200f;
        public const float Slot1CooldownSeconds = 9.5f;

        public const string Slot2ActionName = "SummonSlot2";
        public const string Slot2HudLabel = "S2 LASER";
        public const int Slot2MinimumTier = 1;
        public const float Slot2RequiredMana = 100f;
        public const float Slot2CooldownSeconds = 4.8f;

        public const string Slot3ActionName = "SummonSlot3";
        public const string Slot3HudLabel = "S3 DRAGON";
        public const int Slot3MinimumTier = 3;
        public const float Slot3RequiredMana = 300f;
        public const float Slot3CooldownSeconds = 15f;

        public static BossBarrageSummonSlotBalance Slot1 =>
            new BossBarrageSummonSlotBalance(
                Slot1ActionName,
                Slot1HudLabel,
                Slot1MinimumTier,
                Slot1RequiredMana,
                Slot1CooldownSeconds);

        public static BossBarrageSummonSlotBalance Slot2 =>
            new BossBarrageSummonSlotBalance(
                Slot2ActionName,
                Slot2HudLabel,
                Slot2MinimumTier,
                Slot2RequiredMana,
                Slot2CooldownSeconds);

        public static BossBarrageSummonSlotBalance Slot3 =>
            new BossBarrageSummonSlotBalance(
                Slot3ActionName,
                Slot3HudLabel,
                Slot3MinimumTier,
                Slot3RequiredMana,
                Slot3CooldownSeconds);
    }
}
