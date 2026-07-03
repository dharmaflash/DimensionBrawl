using System;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum BossPressureRiskBand
    {
        BackSafety,
        MidPressure,
        ForwardCommit
    }

    [DisallowMultipleComponent]
    public sealed class BossPressureCostLadder : MonoBehaviour
    {
        private const int MaxTier = 3;

        [Header("References")]
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedBoss;

        [Header("Cost Targets")]
        [SerializeField, Min(1f)] private float levelOneCost = 100f;
        [SerializeField, Min(1f)] private float levelTwoCost = 100f;
        [SerializeField, Min(1f)] private float levelThreeCost = 100f;

        [Header("Boss Position Bands")]
        [SerializeField, Range(0f, 1f)] private float backSafetyMaxBossForwardRisk01 = 1f / 3f;
        [SerializeField, Range(0f, 1f)] private float forwardCommitStartBossForwardRisk01 = 2f / 3f;

        [Header("Gain")]
        [Tooltip("At the middle risk band this reaches LV1 in about eight seconds with the default 100 cost target.")]
        [SerializeField, Min(0f)] private float baseCostPerSecond = 12.5f;
        [SerializeField, Range(0f, 1f)] private float fallbackBossForwardRisk01 = 0.25f;
        [SerializeField] private AnimationCurve bossForwardGainCurve = AnimationCurve.EaseInOut(0f, 0.55f, 1f, 1.55f);
        [SerializeField] private bool gainEnabled = true;

        private int chargingTier = 1;
        private int availableTier;
        private float currentTierCost;
        private float currentBossForwardRisk01 = 0.25f;
        private float currentGainMultiplier = 1f;

        public event Action CostChanged;
        public event Action<int> TierAvailable;
        public event Action<int> CostSpent;

        public int ChargingTier => chargingTier;
        public int AvailableTier => availableTier;
        public float CurrentTierCost => currentTierCost;
        public float CurrentTierTarget => GetTierTarget(chargingTier);
        public float CurrentTierFillRatio => CurrentTierTarget > 0f ? Mathf.Clamp01(currentTierCost / CurrentTierTarget) : 0f;
        public float CurrentBossForwardRisk01 => currentBossForwardRisk01;
        public float CurrentGainMultiplier => currentGainMultiplier;
        public BossPressureRiskBand CurrentRiskBand => EvaluateRiskBand(currentBossForwardRisk01);
        public bool CanSpend => availableTier > 0;
        public bool IsCapped => availableTier >= MaxTier && chargingTier >= MaxTier && CurrentTierFillRatio >= 1f;

        private void OnValidate()
        {
            backSafetyMaxBossForwardRisk01 = Mathf.Clamp01(backSafetyMaxBossForwardRisk01);
            forwardCommitStartBossForwardRisk01 = Mathf.Clamp01(forwardCommitStartBossForwardRisk01);
            if (forwardCommitStartBossForwardRisk01 < backSafetyMaxBossForwardRisk01)
            {
                forwardCommitStartBossForwardRisk01 = backSafetyMaxBossForwardRisk01;
            }
        }

        public void ConfigureReferences(SummonLaneSpace newLaneSpace, Transform newTrackedBoss)
        {
            laneSpace = newLaneSpace;
            trackedBoss = newTrackedBoss;
        }

        public bool TrySpend(out int spentTier)
        {
            spentTier = availableTier;
            if (spentTier <= 0)
            {
                return false;
            }

            ResetLadder();
            CostSpent?.Invoke(spentTier);
            return true;
        }

        public void GrantCurrentTierCost(float costAmount)
        {
            if (costAmount <= 0f || IsCapped)
            {
                return;
            }

            ApplyCostAmount(costAmount);
        }

        public void ResetLadder()
        {
            chargingTier = 1;
            availableTier = 0;
            currentTierCost = 0f;
            currentBossForwardRisk01 = Mathf.Clamp01(fallbackBossForwardRisk01);
            currentGainMultiplier = Mathf.Max(0f, bossForwardGainCurve.Evaluate(currentBossForwardRisk01));
            CostChanged?.Invoke();
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
            ApplyCostAmount(baseCostPerSecond * currentGainMultiplier * deltaTime);
        }

        public float EvaluateBossForwardRisk01(Vector3 worldPosition)
        {
            if (laneSpace == null)
            {
                return Mathf.Clamp01(fallbackBossForwardRisk01);
            }

            float laneZ = laneSpace.GetLaneCoordinates(worldPosition).y;
            float clampedZ = Mathf.Clamp(laneZ, laneSpace.ForwardBoundaryZ, laneSpace.BossProxyZ);
            return Mathf.Clamp01(Mathf.InverseLerp(laneSpace.BossProxyZ, laneSpace.ForwardBoundaryZ, clampedZ));
        }

        private void Update()
        {
            Tick(Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this));
        }

        private void ApplyCostAmount(float costAmount)
        {
            if (costAmount <= 0f || IsCapped)
            {
                return;
            }

            float remainingCost = costAmount;
            while (remainingCost > 0f && !IsCapped)
            {
                float target = CurrentTierTarget;
                float missingCost = Mathf.Max(0f, target - currentTierCost);
                if (remainingCost < missingCost)
                {
                    currentTierCost += remainingCost;
                    remainingCost = 0f;
                    break;
                }

                currentTierCost = target;
                remainingCost -= missingCost;
                availableTier = chargingTier;
                TierAvailable?.Invoke(availableTier);

                if (chargingTier >= MaxTier)
                {
                    break;
                }

                chargingTier++;
                currentTierCost = 0f;
            }

            CostChanged?.Invoke();
        }

        private float EvaluateGainMultiplier()
        {
            currentBossForwardRisk01 = Mathf.Clamp01(fallbackBossForwardRisk01);
            if (trackedBoss != null)
            {
                currentBossForwardRisk01 = EvaluateBossForwardRisk01(trackedBoss.position);
            }

            return Mathf.Max(0f, bossForwardGainCurve.Evaluate(currentBossForwardRisk01));
        }

        private BossPressureRiskBand EvaluateRiskBand(float bossForwardRisk01)
        {
            if (bossForwardRisk01 >= forwardCommitStartBossForwardRisk01)
            {
                return BossPressureRiskBand.ForwardCommit;
            }

            if (bossForwardRisk01 >= backSafetyMaxBossForwardRisk01)
            {
                return BossPressureRiskBand.MidPressure;
            }

            return BossPressureRiskBand.BackSafety;
        }

        private float GetTierTarget(int tier)
        {
            return tier switch
            {
                1 => levelOneCost,
                2 => levelTwoCost,
                _ => levelThreeCost
            };
        }
    }
}
