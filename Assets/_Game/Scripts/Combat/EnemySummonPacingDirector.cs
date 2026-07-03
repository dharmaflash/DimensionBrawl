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
        [SerializeField, Min(1)] private int maxActiveSummonActors = 1;

        private float nextReleaseTimer;
        private float lastReleaseAgeSeconds = float.PositiveInfinity;
        private int totalPacingReleaseCount;
        private int lastPacingReleasedTier;

        public bool PacingEnabled => pacingEnabled;
        public int SummonTier => summonTier;
        public float InitialDelaySeconds => initialDelaySeconds;
        public float RespawnIntervalSeconds => respawnIntervalSeconds;
        public float RetryIntervalSeconds => retryIntervalSeconds;
        public int MaxActiveSummonActors => maxActiveSummonActors;
        public float NextReleaseRemainingSeconds => nextReleaseTimer;
        public float LastReleaseAgeSeconds => lastReleaseAgeSeconds;
        public int TotalPacingReleaseCount => totalPacingReleaseCount;
        public int LastPacingReleasedTier => lastPacingReleasedTier;

        private void OnEnable()
        {
            ResetPacingTimer();
        }

        private void OnValidate()
        {
            summonTier = Mathf.Clamp(summonTier, 1, 3);
            initialDelaySeconds = Mathf.Max(0f, initialDelaySeconds);
            respawnIntervalSeconds = Mathf.Max(0.1f, respawnIntervalSeconds);
            retryIntervalSeconds = Mathf.Max(0.05f, retryIntervalSeconds);
            maxActiveSummonActors = Mathf.Max(1, maxActiveSummonActors);
        }

        public void ConfigureReferences(BossSummonPressureAction newSummonPressureAction)
        {
            summonPressureAction = newSummonPressureAction;
        }

        public void ConfigurePacing(
            float newInitialDelaySeconds,
            float newRespawnIntervalSeconds,
            int newSummonTier,
            int newMaxActiveSummonActors,
            float newRetryIntervalSeconds = 0.35f)
        {
            initialDelaySeconds = Mathf.Max(0f, newInitialDelaySeconds);
            respawnIntervalSeconds = Mathf.Max(0.1f, newRespawnIntervalSeconds);
            summonTier = Mathf.Clamp(newSummonTier, 1, 3);
            maxActiveSummonActors = Mathf.Max(1, newMaxActiveSummonActors);
            retryIntervalSeconds = Mathf.Max(0.05f, newRetryIntervalSeconds);
            ResetPacingTimer();
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

            int releasedTier = Mathf.Clamp(summonTier, 1, 3);
            if (!summonPressureAction.TryReleasePressureSummon(releasedTier))
            {
                nextReleaseTimer = retryIntervalSeconds;
                return;
            }

            totalPacingReleaseCount++;
            lastPacingReleasedTier = releasedTier;
            lastReleaseAgeSeconds = 0f;
            nextReleaseTimer = respawnIntervalSeconds;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private bool CanReleasePacedSummon()
        {
            return summonPressureAction != null
                && summonPressureAction.CanRelease
                && summonPressureAction.ActiveSummonActorCount < maxActiveSummonActors;
        }
    }
}
