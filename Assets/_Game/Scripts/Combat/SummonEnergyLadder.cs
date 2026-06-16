using System;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public sealed class SummonEnergyLadder : MonoBehaviour
    {
        private const int MaxTier = 3;

        [Header("References")]
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedPlayer;

        [Header("Energy Targets")]
        [SerializeField, Min(1f)] private float levelOneEnergy = 100f;
        [SerializeField, Min(1f)] private float levelTwoEnergy = 100f;
        [SerializeField, Min(1f)] private float levelThreeEnergy = 100f;

        [Header("Gain")]
        [Tooltip("At the middle risk band this fills LV1 in about eight seconds with the default 100 energy target.")]
        [SerializeField, Min(0f)] private float baseEnergyPerSecond = 12.5f;
        [SerializeField, Range(0f, 1f)] private float fallbackForwardRisk01 = 0.5f;
        [SerializeField] private AnimationCurve forwardRiskGainCurve = AnimationCurve.EaseInOut(0f, 0.55f, 1f, 1.65f);
        [SerializeField] private bool gainEnabled = true;

        private int chargingTier = 1;
        private int availableTier;
        private float currentTierEnergy;
        private float currentGainMultiplier = 1f;

        public event Action EnergyChanged;
        public event Action<int> TierAvailable;
        public event Action<int> EnergySpent;

        public int ChargingTier => chargingTier;
        public int AvailableTier => availableTier;
        public float CurrentTierEnergy => currentTierEnergy;
        public float CurrentTierTarget => GetTierTarget(chargingTier);
        public float CurrentTierFillRatio => CurrentTierTarget > 0f ? Mathf.Clamp01(currentTierEnergy / CurrentTierTarget) : 0f;
        public float CurrentGainMultiplier => currentGainMultiplier;
        public bool CanSpend => availableTier > 0;
        public bool IsCapped => availableTier >= MaxTier && chargingTier >= MaxTier && CurrentTierFillRatio >= 1f;

        public void ConfigureReferences(SummonLaneSpace newLaneSpace, Transform newTrackedPlayer)
        {
            laneSpace = newLaneSpace;
            trackedPlayer = newTrackedPlayer;
        }

        public bool TrySpend(out int spentTier)
        {
            spentTier = availableTier;
            if (spentTier <= 0)
            {
                return false;
            }

            ResetLadder();
            EnergySpent?.Invoke(spentTier);
            return true;
        }

        public void GrantCurrentTierEnergy(float energyAmount)
        {
            if (energyAmount <= 0f || IsCapped)
            {
                return;
            }

            ApplyEnergyAmount(energyAmount);
        }

        public void ResetLadder()
        {
            chargingTier = 1;
            availableTier = 0;
            currentTierEnergy = 0f;
            EnergyChanged?.Invoke();
        }

        public void SetGainEnabled(bool enabled)
        {
            gainEnabled = enabled;
        }

        public void Tick(float deltaTime)
        {
            if (!gainEnabled || deltaTime <= 0f || IsCapped)
            {
                return;
            }

            currentGainMultiplier = EvaluateGainMultiplier();
            ApplyEnergyAmount(baseEnergyPerSecond * currentGainMultiplier * deltaTime);
        }

        private void ApplyEnergyAmount(float energyAmount)
        {
            if (energyAmount <= 0f || IsCapped)
            {
                return;
            }

            currentTierEnergy += energyAmount;
            float target = CurrentTierTarget;

            if (currentTierEnergy >= target)
            {
                currentTierEnergy = target;
                availableTier = chargingTier;
                TierAvailable?.Invoke(availableTier);

                if (chargingTier < MaxTier)
                {
                    chargingTier++;
                    currentTierEnergy = 0f;
                }
            }

            EnergyChanged?.Invoke();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private float EvaluateGainMultiplier()
        {
            float forwardRisk = fallbackForwardRisk01;
            if (laneSpace != null && trackedPlayer != null)
            {
                forwardRisk = laneSpace.EvaluateForwardRisk01(trackedPlayer.position);
            }

            return Mathf.Max(0f, forwardRiskGainCurve.Evaluate(Mathf.Clamp01(forwardRisk)));
        }

        private float GetTierTarget(int tier)
        {
            return tier switch
            {
                1 => levelOneEnergy,
                2 => levelTwoEnergy,
                _ => levelThreeEnergy
            };
        }
    }
}
