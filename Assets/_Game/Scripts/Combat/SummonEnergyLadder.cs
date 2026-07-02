using System;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum SummonEnergyRiskBand
    {
        BackSafety,
        MidCharge,
        ForwardRisk
    }

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

        [Header("Risk Bands")]
        [Tooltip("Forward-risk normalized boundary where MidCharge transitions into ForwardRisk. Higher = less likely to reach ForwardRisk.")]
        [SerializeField, Range(0f, 1f)] private float forwardRiskStartForwardRisk01 = 2f / 3f;
        [Tooltip("Forward-risk normalized boundary where BackSafety transitions into MidCharge.")]
        [SerializeField, Range(0f, 1f)] private float backSafetyMaxForwardRisk01 = 1f / 3f;

        [Header("Gain")]
        [Tooltip("At the middle risk band this fills LV1 in about eight seconds with the default 100 energy target.")]
        [SerializeField, Min(0f)] private float baseEnergyPerSecond = 12.5f;
        [SerializeField, Range(0f, 1f)] private float fallbackForwardRisk01 = 0.5f;
        [SerializeField] private AnimationCurve forwardRiskGainCurve = AnimationCurve.EaseInOut(0f, 0.55f, 1f, 1.65f);
        [SerializeField, Min(0f)] private float backSafetyGainScale = 0.45f;
        [SerializeField, Min(0f)] private float midChargeGainScale = 0.9f;
        [SerializeField, Min(0f)] private float forwardRiskGainScale = 1.75f;
        [SerializeField] private bool gainEnabled = true;

        private int chargingTier = 1;
        private int availableTier;
        private float currentTierEnergy;
        private float currentMana;
        private float currentForwardRisk01 = 0.5f;
        private float currentGainMultiplier = 1f;

        public event Action EnergyChanged;
        public event Action<SummonEnergyRiskBand> RiskBandChanged;
        public event Action<int> TierAvailable;
        public event Action<int> EnergySpent;

        public int ChargingTier => chargingTier;
        public int AvailableTier => availableTier;
        public float CurrentTierEnergy => currentTierEnergy;
        public float CurrentTierTarget => GetTierTarget(chargingTier);
        public float CurrentTierFillRatio => CurrentTierTarget > 0f ? Mathf.Clamp01(currentTierEnergy / CurrentTierTarget) : 0f;
        public float CurrentMana => currentMana;
        public float MaxMana => levelOneEnergy + levelTwoEnergy + levelThreeEnergy;
        public float CurrentManaFillRatio => MaxMana > 0f ? Mathf.Clamp01(currentMana / MaxMana) : 0f;
        public float CurrentForwardRisk01 => currentForwardRisk01;
        public float CurrentGainMultiplier => currentGainMultiplier;
        public SummonEnergyRiskBand CurrentRiskBand => EvaluateRiskBand(currentForwardRisk01);
        public bool CanSpend => availableTier > 0;
        public bool IsCapped => currentMana >= MaxMana - 0.001f;

        private void OnValidate()
        {
            backSafetyMaxForwardRisk01 = Mathf.Clamp01(backSafetyMaxForwardRisk01);
            forwardRiskStartForwardRisk01 = Mathf.Clamp(forwardRiskStartForwardRisk01, 0f, 1f);
            if (forwardRiskStartForwardRisk01 < backSafetyMaxForwardRisk01)
            {
                forwardRiskStartForwardRisk01 = backSafetyMaxForwardRisk01;
            }

            backSafetyGainScale = Mathf.Max(0f, backSafetyGainScale);
            midChargeGainScale = Mathf.Max(0f, midChargeGainScale);
            forwardRiskGainScale = Mathf.Max(0f, forwardRiskGainScale);
        }

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

        public bool CanSpendMana(float requiredMana)
        {
            return currentMana + 0.001f >= Mathf.Max(1f, requiredMana);
        }

        public int ResolveTierForManaCost(float requiredMana)
        {
            float cost = Mathf.Max(1f, requiredMana);
            if (cost > levelOneEnergy + levelTwoEnergy + 0.001f)
            {
                return MaxTier;
            }

            if (cost > levelOneEnergy + 0.001f)
            {
                return 2;
            }

            return 1;
        }

        public bool TrySpend(float requiredMana, out int spentTier)
        {
            requiredMana = Mathf.Max(1f, requiredMana);
            int costTier = ResolveTierForManaCost(requiredMana);
            if (availableTier < costTier || !CanSpendMana(requiredMana))
            {
                spentTier = 0;
                return false;
            }

            spentTier = costTier;
            SetCurrentMana(currentMana - requiredMana);
            EnergyChanged?.Invoke();
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
            currentMana = 0f;
            currentForwardRisk01 = Mathf.Clamp01(fallbackForwardRisk01);
            currentGainMultiplier = EvaluateGainMultiplier(currentForwardRisk01);
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

            SummonEnergyRiskBand previousRiskBand = CurrentRiskBand;
            currentGainMultiplier = EvaluateGainMultiplier();
            SummonEnergyRiskBand currentRiskBand = CurrentRiskBand;
            if (currentRiskBand != previousRiskBand)
            {
                RiskBandChanged?.Invoke(currentRiskBand);
            }

            ApplyEnergyAmount(baseEnergyPerSecond * currentGainMultiplier * deltaTime);
        }

        private void ApplyEnergyAmount(float energyAmount)
        {
            if (energyAmount <= 0f || IsCapped)
            {
                return;
            }

            float previousMana = currentMana;
            float nextMana = Mathf.Min(MaxMana, currentMana + energyAmount);
            SetCurrentMana(nextMana);

            for (int tier = 1; tier <= MaxTier; tier++)
            {
                float threshold = GetCumulativeTierTarget(tier);
                if (previousMana < threshold && nextMana >= threshold)
                {
                    TierAvailable?.Invoke(tier);
                }
            }

            EnergyChanged?.Invoke();
        }

        private void SetCurrentMana(float nextMana)
        {
            currentMana = Mathf.Clamp(nextMana, 0f, MaxMana);
            availableTier = ResolveAvailableTier(currentMana);
            chargingTier = ResolveChargingTier(currentMana, availableTier);
            currentTierEnergy = ResolveCurrentTierEnergy(currentMana, chargingTier);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private float EvaluateGainMultiplier()
        {
            currentForwardRisk01 = Mathf.Clamp01(fallbackForwardRisk01);
            if (laneSpace != null && trackedPlayer != null)
            {
                currentForwardRisk01 = laneSpace.EvaluateForwardRisk01(trackedPlayer.position);
            }

            return EvaluateGainMultiplier(currentForwardRisk01);
        }

        private float EvaluateGainMultiplier(float forwardRisk01)
        {
            float clampedForwardRisk01 = Mathf.Clamp01(forwardRisk01);
            float curveMultiplier = forwardRiskGainCurve != null ? forwardRiskGainCurve.Evaluate(clampedForwardRisk01) : 1f;
            return Mathf.Max(0f, curveMultiplier * ResolveRiskBandGainScale(EvaluateRiskBand(clampedForwardRisk01)));
        }

        private float ResolveRiskBandGainScale(SummonEnergyRiskBand riskBand)
        {
            return riskBand switch
            {
                SummonEnergyRiskBand.ForwardRisk => forwardRiskGainScale,
                SummonEnergyRiskBand.MidCharge => midChargeGainScale,
                _ => backSafetyGainScale
            };
        }

        private SummonEnergyRiskBand EvaluateRiskBand(float forwardRisk01)
        {
            if (forwardRisk01 >= forwardRiskStartForwardRisk01)
            {
                return SummonEnergyRiskBand.ForwardRisk;
            }

            if (forwardRisk01 >= backSafetyMaxForwardRisk01)
            {
                return SummonEnergyRiskBand.MidCharge;
            }

            return SummonEnergyRiskBand.BackSafety;
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

        private float GetCumulativeTierTarget(int tier)
        {
            return tier switch
            {
                1 => levelOneEnergy,
                2 => levelOneEnergy + levelTwoEnergy,
                _ => MaxMana
            };
        }

        private int ResolveAvailableTier(float mana)
        {
            if (mana >= MaxMana - 0.001f)
            {
                return MaxTier;
            }

            if (mana >= levelOneEnergy + levelTwoEnergy - 0.001f)
            {
                return 2;
            }

            return mana >= levelOneEnergy - 0.001f ? 1 : 0;
        }

        private int ResolveChargingTier(float mana, int resolvedAvailableTier)
        {
            if (resolvedAvailableTier >= MaxTier)
            {
                return MaxTier;
            }

            return Mathf.Clamp(resolvedAvailableTier + 1, 1, MaxTier);
        }

        private float ResolveCurrentTierEnergy(float mana, int resolvedChargingTier)
        {
            float previousTierTotal = resolvedChargingTier switch
            {
                1 => 0f,
                2 => levelOneEnergy,
                _ => levelOneEnergy + levelTwoEnergy
            };

            return Mathf.Clamp(mana - previousTierTotal, 0f, GetTierTarget(resolvedChargingTier));
        }
    }
}
