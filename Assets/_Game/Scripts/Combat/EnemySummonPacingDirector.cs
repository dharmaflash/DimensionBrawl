using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class EnemySummonPacingDirector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossSummonPressureAction summonPressureAction;

        [Header("Pacing")]
        [SerializeField] private bool pacingEnabled = true;
        [SerializeField, Range(1, 3)] private int summonTier = 1;
        [SerializeField, Min(0f)] private float initialDelaySeconds = 2f;
        [SerializeField, Min(0.1f)] private float respawnIntervalSeconds = 6.5f;
        [SerializeField, Min(0.05f)] private float retryIntervalSeconds = 0.35f;
        [SerializeField] private int[] summonTierSequence = { 1, 2, 1, 3 };

        private float nextReleaseTimer;
        private float lastReleaseAgeSeconds = float.PositiveInfinity;
        private int totalPacingReleaseCount;
        private int lastPacingReleasedTier;
        private int summonTierSequenceCursor;

        public bool PacingEnabled => pacingEnabled;
        public int SummonTier => summonTier;
        public float InitialDelaySeconds => initialDelaySeconds;
        public float RespawnIntervalSeconds => respawnIntervalSeconds;
        public float RetryIntervalSeconds => retryIntervalSeconds;
        public int SummonTierSequenceCount => summonTierSequence != null ? summonTierSequence.Length : 0;
        public int NextPacingTier => ResolveNextSummonTier();
        public float NextReleaseRemainingSeconds => nextReleaseTimer;
        public float LastReleaseAgeSeconds => lastReleaseAgeSeconds;
        public int TotalPacingReleaseCount => totalPacingReleaseCount;
        public int LastPacingReleasedTier => lastPacingReleasedTier;

        private void OnEnable()
        {
            CombatTimeDilationReceiver.Ensure(gameObject);
            ResetPacingTimer();
        }

        private void OnValidate()
        {
            summonTier = Mathf.Clamp(summonTier, 1, 3);
            initialDelaySeconds = Mathf.Max(0f, initialDelaySeconds);
            respawnIntervalSeconds = Mathf.Max(0.1f, respawnIntervalSeconds);
            retryIntervalSeconds = Mathf.Max(0.05f, retryIntervalSeconds);
            summonTierSequence = NormalizeTierSequence(summonTierSequence, summonTier);
        }

        public void ConfigureReferences(BossSummonPressureAction newSummonPressureAction)
        {
            summonPressureAction = newSummonPressureAction;
        }

        public void ConfigurePacing(
            float newInitialDelaySeconds,
            float newRespawnIntervalSeconds,
            int newSummonTier,
            float newRetryIntervalSeconds = 0.35f,
            int[] newSummonTierSequence = null)
        {
            initialDelaySeconds = Mathf.Max(0f, newInitialDelaySeconds);
            respawnIntervalSeconds = Mathf.Max(0.1f, newRespawnIntervalSeconds);
            summonTier = Mathf.Clamp(newSummonTier, 1, 3);
            retryIntervalSeconds = Mathf.Max(0.05f, newRetryIntervalSeconds);
            summonTierSequence = NormalizeTierSequence(newSummonTierSequence, summonTier);
            summonTierSequenceCursor = 0;
            ResetPacingTimer();
        }

        public bool TryGetSummonTierSequenceValue(int index, out int tier)
        {
            if (summonTierSequence == null || index < 0 || index >= summonTierSequence.Length)
            {
                tier = 0;
                return false;
            }

            tier = Mathf.Clamp(summonTierSequence[index], 1, 3);
            return true;
        }

        public void SetPacingEnabled(bool enabled)
        {
            if (pacingEnabled == enabled)
            {
                return;
            }

            pacingEnabled = enabled;
            if (pacingEnabled)
            {
                ResetPacingTimer();
            }
        }

        public void ResetPacingTimer()
        {
            nextReleaseTimer = Mathf.Max(0f, initialDelaySeconds);
        }

        public void Tick(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            if (!float.IsPositiveInfinity(lastReleaseAgeSeconds))
            {
                lastReleaseAgeSeconds += safeDeltaTime;
            }

            if (!pacingEnabled || safeDeltaTime <= 0f)
            {
                return;
            }

            nextReleaseTimer = Mathf.Max(0f, nextReleaseTimer - safeDeltaTime);
            if (nextReleaseTimer > 0f)
            {
                return;
            }

            if (!CanReleasePacedSummon())
            {
                nextReleaseTimer = retryIntervalSeconds;
                return;
            }

            int releasedTier = ResolveNextSummonTier();
            if (!summonPressureAction.TryReleasePressureSummon(releasedTier))
            {
                nextReleaseTimer = retryIntervalSeconds;
                return;
            }

            totalPacingReleaseCount++;
            lastPacingReleasedTier = releasedTier;
            AdvanceSummonTierSequence();
            lastReleaseAgeSeconds = 0f;
            nextReleaseTimer = respawnIntervalSeconds;
        }

        private void Update()
        {
            Tick(Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this));
        }

        private bool CanReleasePacedSummon()
        {
            return summonPressureAction != null
                && summonPressureAction.CanRelease;
        }

        private int ResolveNextSummonTier()
        {
            if (summonTierSequence == null || summonTierSequence.Length == 0)
            {
                return Mathf.Clamp(summonTier, 1, 3);
            }

            int index = Mathf.Abs(summonTierSequenceCursor) % summonTierSequence.Length;
            return Mathf.Clamp(summonTierSequence[index], 1, 3);
        }

        private void AdvanceSummonTierSequence()
        {
            if (summonTierSequence == null || summonTierSequence.Length <= 0)
            {
                return;
            }

            summonTierSequenceCursor = (summonTierSequenceCursor + 1) % summonTierSequence.Length;
        }

        private static int[] NormalizeTierSequence(int[] sequence, int fallbackTier)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return new[] { Mathf.Clamp(fallbackTier, 1, 3) };
            }

            int[] normalized = new int[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
            {
                normalized[i] = Mathf.Clamp(sequence[i], 1, 3);
            }

            return normalized;
        }
    }
}
