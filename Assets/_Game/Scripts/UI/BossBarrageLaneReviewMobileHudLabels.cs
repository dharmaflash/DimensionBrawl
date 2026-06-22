using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.UI
{
    internal static class BossBarrageLaneReviewMobileHudLabels
    {
        public static string BuildPrimarySummonLabel(
            string slotLabel,
            SummonEnergyLadder energyLadder,
            PlayerSummonSlot1Action summonSlot1Action)
        {
            int availableTier = energyLadder != null ? energyLadder.AvailableTier : 0;
            if (availableTier > 0)
            {
                string tierName = TryGetPrimarySummonTierShortName(summonSlot1Action, availableTier);
                return $"{slotLabel}\nREADY LV{availableTier} {tierName}".TrimEnd();
            }

            if (energyLadder == null)
            {
                return $"{slotLabel}\nREADY?";
            }

            return BuildChargingLabel(slotLabel, energyLadder);
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

            int availableTier = energyLadder != null ? energyLadder.AvailableTier : 0;
            if (availableTier > 0)
            {
                string tierName = TryGetSupportSummonTierShortName(supportAction, availableTier);
                return $"{slotLabel}\nREADY LV{availableTier} {tierName}".TrimEnd();
            }

            if (energyLadder == null)
            {
                return $"{slotLabel}\nREADY?";
            }

            return BuildChargingLabel(slotLabel, energyLadder);
        }

        private static string BuildChargingLabel(string slotLabel, SummonEnergyLadder energyLadder)
        {
            int chargingTier = Mathf.Clamp(energyLadder.ChargingTier, 1, 3);
            int fillPercent = Mathf.RoundToInt(energyLadder.CurrentTierFillRatio * 100f);
            return $"{slotLabel}\nLV{chargingTier} {fillPercent}%";
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
