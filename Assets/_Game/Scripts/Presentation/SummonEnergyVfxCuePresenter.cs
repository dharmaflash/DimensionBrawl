using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(910)]
    public sealed class SummonEnergyVfxCuePresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform cueAnchor;
        [SerializeField] private Transform directionTarget;

        [Header("Cue IDs")]
        [SerializeField] private CombatVfxCueId forwardRiskCueId = CombatVfxCueId.EliteAuraSignal;
        [SerializeField] private CombatVfxCueId tierReadyCueId = CombatVfxCueId.SummonFollowupWindow;
        [SerializeField] private CombatVfxCueId spendCueId = CombatVfxCueId.SummonFollowupMissed;

        [Header("Intensity")]
        [SerializeField, Min(0f)] private float forwardRiskCueIntensity = 0.58f;
        [SerializeField, Min(0f)] private float tierReadyCueIntensity = 0.82f;
        [SerializeField, Min(0f)] private float spendCueIntensity = 0.5f;
        [SerializeField, Min(0f)] private float tierIntensityStep = 0.12f;
        [SerializeField, Min(0f)] private float forwardRiskCueCooldownSeconds = 0.75f;

        private bool subscribed;
        private bool hasObservedRiskBand;
        private SummonEnergyRiskBand lastRiskBand;
        private float forwardRiskCueCooldown;
        private int forwardRiskCueRequestCount;
        private int tierReadyCueRequestCount;
        private int spendCueRequestCount;
        private int lastReadyTier;
        private int lastSpentTier;

        public SummonEnergyLadder EnergyLadder => energyLadder;
        public CombatVfxCuePlayer CuePlayer => cuePlayer;
        public Transform CueAnchor => cueAnchor != null ? cueAnchor : transform;
        public Transform DirectionTarget => directionTarget;
        public CombatVfxCueId ForwardRiskCueId => forwardRiskCueId;
        public CombatVfxCueId TierReadyCueId => tierReadyCueId;
        public CombatVfxCueId SpendCueId => spendCueId;
        public int ForwardRiskCueRequestCount => forwardRiskCueRequestCount;
        public int TierReadyCueRequestCount => tierReadyCueRequestCount;
        public int SpendCueRequestCount => spendCueRequestCount;
        public int LastReadyTier => lastReadyTier;
        public int LastSpentTier => lastSpentTier;

        public void Configure(
            SummonEnergyLadder newEnergyLadder,
            CombatVfxCuePlayer newCuePlayer,
            Transform newCueAnchor,
            Transform newDirectionTarget)
        {
            Unsubscribe();
            energyLadder = newEnergyLadder;
            cuePlayer = newCuePlayer;
            cueAnchor = newCueAnchor;
            directionTarget = newDirectionTarget;
            ObserveInitialRiskBand();
            Subscribe();
        }

        private void Awake()
        {
            ResolveReferences();
            ObserveInitialRiskBand();
        }

        private void OnEnable()
        {
            ResolveReferences();
            ObserveInitialRiskBand();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (forwardRiskCueCooldown > 0f)
            {
                forwardRiskCueCooldown = Mathf.Max(0f, forwardRiskCueCooldown - Time.deltaTime);
            }

            RefreshNow();
        }

        public void RefreshNow()
        {
            if (energyLadder == null)
            {
                return;
            }

            SummonEnergyRiskBand currentRiskBand = energyLadder.CurrentRiskBand;
            if (!hasObservedRiskBand)
            {
                lastRiskBand = currentRiskBand;
                hasObservedRiskBand = true;
                return;
            }

            if (currentRiskBand == lastRiskBand)
            {
                return;
            }

            lastRiskBand = currentRiskBand;
            if (currentRiskBand == SummonEnergyRiskBand.ForwardRisk
                && forwardRiskCueCooldown <= 0f
                && Play(forwardRiskCueId, 1, forwardRiskCueIntensity))
            {
                forwardRiskCueCooldown = forwardRiskCueCooldownSeconds;
                forwardRiskCueRequestCount++;
            }
        }

        private void HandleTierAvailable(int tier)
        {
            int safeTier = Mathf.Clamp(tier, 1, 3);
            lastReadyTier = safeTier;
            if (Play(tierReadyCueId, safeTier, tierReadyCueIntensity))
            {
                tierReadyCueRequestCount++;
            }
        }

        private void HandleEnergySpent(int tier)
        {
            int safeTier = Mathf.Clamp(tier, 1, 3);
            lastSpentTier = safeTier;
            if (Play(spendCueId, safeTier, spendCueIntensity))
            {
                spendCueRequestCount++;
            }
        }

        private bool Play(CombatVfxCueId cueId, int tier, float baseIntensity)
        {
            if (cuePlayer == null)
            {
                return false;
            }

            float intensity = Mathf.Max(0f, baseIntensity + Mathf.Max(0, tier - 1) * tierIntensityStep);
            return cuePlayer.PlayCue(cueId, CueAnchor, ResolveCueDirection(), intensity);
        }

        private Vector3 ResolveCueDirection()
        {
            Transform anchor = CueAnchor;
            if (directionTarget != null)
            {
                Vector3 targetDirection = Vector3.ProjectOnPlane(directionTarget.position - anchor.position, Vector3.up);
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    return targetDirection.normalized;
                }
            }

            Vector3 forward = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private void ResolveReferences()
        {
            if (energyLadder == null)
            {
                energyLadder = GetComponent<SummonEnergyLadder>();
            }

            if (cuePlayer == null)
            {
                cuePlayer = GetComponent<CombatVfxCuePlayer>();
            }

            if (cueAnchor == null)
            {
                cueAnchor = transform;
            }
        }

        private void ObserveInitialRiskBand()
        {
            if (energyLadder == null)
            {
                hasObservedRiskBand = false;
                return;
            }

            lastRiskBand = energyLadder.CurrentRiskBand;
            hasObservedRiskBand = true;
        }

        private void Subscribe()
        {
            if (subscribed || energyLadder == null)
            {
                return;
            }

            energyLadder.TierAvailable += HandleTierAvailable;
            energyLadder.EnergySpent += HandleEnergySpent;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || energyLadder == null)
            {
                subscribed = false;
                return;
            }

            energyLadder.TierAvailable -= HandleTierAvailable;
            energyLadder.EnergySpent -= HandleEnergySpent;
            subscribed = false;
        }
    }
}
