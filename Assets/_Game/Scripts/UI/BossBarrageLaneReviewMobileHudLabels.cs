using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public static class BossBarrageLaneReviewMobileHudLabels
    {
        public static string BuildPrimarySummonLabel(
            string slotLabel,
            SummonEnergyLadder energyLadder,
            PlayerSummonSlot1Action summonSlot1Action)
        {
            if (summonSlot1Action != null && summonSlot1Action.IsSlotOnCooldown)
            {
                return BuildCooldownLabel(slotLabel, summonSlot1Action.SlotCooldownRemaining);
            }

            if (energyLadder == null)
            {
                return $"{slotLabel}\nREADY?";
            }

            int availableTier = energyLadder.AvailableTier;
            float requiredMana = summonSlot1Action != null ? summonSlot1Action.RequiredSummonMana : 1f;
            if (availableTier > 0 && energyLadder.CanSpendMana(requiredMana))
            {
                string tierName = TryGetPrimarySummonTierShortName(summonSlot1Action, availableTier);
                return $"{slotLabel}\nREADY LV{availableTier} {tierName}".TrimEnd();
            }

            return BuildMissingManaLabel(slotLabel, energyLadder, requiredMana);
        }

        public static string BuildSupportSummonLabel(
            PlayerSupportSummonSlotAction supportAction,
            string slotLabel,
            string lockedSummonLabel,
            SummonEnergyLadder energyLadder)
        {
            if (supportAction == null)
            {
                return $"{slotLabel}\n{lockedSummonLabel}";
            }

            if (supportAction.IsSlotOnCooldown)
            {
                return BuildCooldownLabel(slotLabel, supportAction.SlotCooldownRemaining);
            }

            if (energyLadder == null)
            {
                return $"{slotLabel}\nREADY?";
            }

            int availableTier = energyLadder.AvailableTier;
            if (availableTier >= supportAction.MinimumSummonTier
                && energyLadder.CanSpendMana(supportAction.RequiredSummonMana))
            {
                string tierName = TryGetSupportSummonTierShortName(supportAction, availableTier);
                return $"{slotLabel}\nREADY LV{availableTier} {tierName}".TrimEnd();
            }

            return BuildMissingManaLabel(slotLabel, energyLadder, supportAction.RequiredSummonMana);
        }

        public static float ResolvePrimarySummonFill01(
            SummonEnergyLadder energyLadder,
            PlayerSummonSlot1Action summonSlot1Action)
        {
            if (summonSlot1Action == null || energyLadder == null)
            {
                return 0f;
            }

            if (summonSlot1Action.IsSlotOnCooldown)
            {
                return ResolveCooldownFill01(
                    summonSlot1Action.SlotCooldownRemaining,
                    summonSlot1Action.SlotCooldownSeconds);
            }

            if (energyLadder.CanSpendMana(summonSlot1Action.RequiredSummonMana))
            {
                return 1f;
            }

            return ResolveManaFill01(energyLadder, summonSlot1Action.RequiredSummonMana);
        }

        public static float ResolveSupportSummonFill01(
            SummonEnergyLadder energyLadder,
            PlayerSupportSummonSlotAction supportAction)
        {
            if (supportAction == null || energyLadder == null)
            {
                return 0f;
            }

            if (supportAction.IsSlotOnCooldown)
            {
                return ResolveCooldownFill01(
                    supportAction.SlotCooldownRemaining,
                    supportAction.SlotCooldownSeconds);
            }

            if (energyLadder.AvailableTier >= supportAction.MinimumSummonTier
                && energyLadder.CanSpendMana(supportAction.RequiredSummonMana))
            {
                return 1f;
            }

            return ResolveManaFill01(energyLadder, supportAction.RequiredSummonMana);
        }

        private static string BuildCooldownLabel(string slotLabel, float cooldownRemaining)
        {
            return $"{slotLabel}\nCD {Mathf.Max(0f, cooldownRemaining):0.0}s";
        }

        private static string BuildMissingManaLabel(
            string slotLabel,
            SummonEnergyLadder energyLadder,
            float requiredMana)
        {
            float shortage = energyLadder != null
                ? energyLadder.GetManaShortage(requiredMana)
                : Mathf.Max(1f, requiredMana);
            return $"{slotLabel}\nNEED +{Mathf.CeilToInt(shortage)} EN";
        }

        private static float ResolveManaFill01(SummonEnergyLadder energyLadder, float requiredMana)
        {
            if (energyLadder == null)
            {
                return 0f;
            }

            return Mathf.Clamp01(energyLadder.CurrentMana / Mathf.Max(1f, requiredMana));
        }

        private static float ResolveCooldownFill01(float cooldownRemaining, float cooldownSeconds)
        {
            if (cooldownSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - Mathf.Max(0f, cooldownRemaining) / cooldownSeconds);
        }

        private static string TryGetPrimarySummonTierShortName(
            PlayerSummonSlot1Action summonSlot1Action,
            int tier)
        {
            if (summonSlot1Action == null
                || !summonSlot1Action.TryGetTierReadout(
                    tier,
                    out SummonSlotActionProfile.SummonTierReadout readout)
                || string.IsNullOrWhiteSpace(readout.TierLabel))
            {
                return string.Empty;
            }

            return ShortenTierLabel(readout.TierLabel);
        }

        private static string TryGetSupportSummonTierShortName(
            PlayerSupportSummonSlotAction supportAction,
            int tier)
        {
            if (supportAction == null
                || !supportAction.TryGetTierReadout(
                    tier,
                    out SummonSlotActionProfile.SummonTierReadout readout)
                || string.IsNullOrWhiteSpace(readout.TierLabel))
            {
                return string.Empty;
            }

            return ShortenTierLabel(readout.TierLabel);
        }

        private static string ShortenTierLabel(string tierLabel)
        {
            string displayName = tierLabel.Trim();
            int firstSpaceIndex = displayName.IndexOf(' ');
            if (firstSpaceIndex <= 0 || firstSpaceIndex >= displayName.Length - 1)
            {
                return displayName.Length <= 10 ? displayName : string.Empty;
            }

            string trailingName = displayName.Substring(firstSpaceIndex + 1).Trim();
            return trailingName.Length <= 10 ? trailingName : string.Empty;
        }
    }
}
