using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public readonly struct CombatResourceReadout
    {
        public CombatResourceReadout(
            string label,
            string valueText,
            string stateText,
            float fill01,
            Color fillColor,
            bool isReady)
        {
            Label = string.IsNullOrWhiteSpace(label) ? "-" : label;
            ValueText = string.IsNullOrWhiteSpace(valueText) ? "-" : valueText;
            StateText = string.IsNullOrWhiteSpace(stateText) ? "-" : stateText;
            Fill01 = Mathf.Clamp01(fill01);
            FillColor = fillColor;
            IsReady = isReady;
        }

        public string Label { get; }
        public string ValueText { get; }
        public string StateText { get; }
        public float Fill01 { get; }
        public Color FillColor { get; }
        public bool IsReady { get; }
        public string Line => $"{Label} {ValueText} {StateText}";

        public static CombatResourceReadout Missing(string label)
        {
            return new CombatResourceReadout(label, "-", "missing", 0f, new Color(0.28f, 0.28f, 0.28f, 1f), false);
        }

        public static CombatResourceReadout FromHealth(string label, CombatHealth health, Color fillColor)
        {
            if (health == null)
            {
                return Missing(label);
            }

            string state = health.IsAlive ? "alive" : "down";
            return new CombatResourceReadout(
                label,
                $"{health.CurrentHealth:0}/{health.MaxHealth:0}",
                state,
                health.HealthRatio,
                fillColor,
                false);
        }

        public static CombatResourceReadout FromEnergy(string label, SummonEnergyLadder energy)
        {
            if (energy == null)
            {
                return Missing(label);
            }

            string ready = energy.CanSpend ? $"READY LV{energy.AvailableTier}" : "charging";
            string capped = energy.IsCapped ? " capped" : string.Empty;
            return new CombatResourceReadout(
                label,
                $"LV{energy.ChargingTier} {energy.CurrentTierFillRatio * 100f:0}%",
                $"{ready}{capped}",
                energy.CurrentTierFillRatio,
                new Color(0.18f, 0.92f, 1f, 1f),
                energy.CanSpend);
        }

        public static CombatResourceReadout FromBossCost(string label, BossPressureCostLadder cost)
        {
            if (cost == null)
            {
                return Missing(label);
            }

            string ready = cost.CanSpend ? $"READY LV{cost.AvailableTier}" : "charging";
            string capped = cost.IsCapped ? " capped" : string.Empty;
            return new CombatResourceReadout(
                label,
                $"LV{cost.ChargingTier} {cost.CurrentTierFillRatio * 100f:0}%",
                $"{ready}{capped}",
                cost.CurrentTierFillRatio,
                new Color(1f, 0.58f, 0.18f, 1f),
                cost.CanSpend);
        }
    }
}
