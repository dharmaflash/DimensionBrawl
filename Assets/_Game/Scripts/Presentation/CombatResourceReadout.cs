using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    public readonly struct CombatResourceReadout
    {
        private const float PressuredHealthRatio = 0.65f;
        private const float CriticalHealthRatio = 0.32f;

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

        public static CombatResourceReadout FromSurvivalHealth(string label, CombatHealth health, Color stableFillColor)
        {
            if (health == null)
            {
                return Missing(label);
            }

            return new CombatResourceReadout(
                label,
                $"{health.CurrentHealth:0}/{health.MaxHealth:0}",
                ResolveSurvivalStateText(health),
                health.HealthRatio,
                ResolveSurvivalFillColor(health, stableFillColor),
                false);
        }

        public static string ResolveSurvivalStateText(CombatHealth health)
        {
            if (health == null)
            {
                return "missing";
            }

            if (!health.IsAlive)
            {
                return "down";
            }

            float healthRatio = Mathf.Clamp01(health.HealthRatio);
            if (healthRatio <= CriticalHealthRatio)
            {
                return "critical";
            }

            return healthRatio <= PressuredHealthRatio ? "pressured" : "stable";
        }

        private static Color ResolveSurvivalFillColor(CombatHealth health, Color stableFillColor)
        {
            if (health == null || !health.IsAlive)
            {
                return new Color(0.36f, 0.38f, 0.42f, stableFillColor.a);
            }

            float healthRatio = Mathf.Clamp01(health.HealthRatio);
            if (healthRatio <= CriticalHealthRatio)
            {
                return new Color(1f, 0.24f, 0.18f, stableFillColor.a);
            }

            return healthRatio <= PressuredHealthRatio
                ? new Color(1f, 0.76f, 0.24f, stableFillColor.a)
                : stableFillColor;
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
                $"{energy.CurrentMana:0}/{energy.MaxMana:0} EN",
                $"{ready}{capped}",
                energy.CurrentManaFillRatio,
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
