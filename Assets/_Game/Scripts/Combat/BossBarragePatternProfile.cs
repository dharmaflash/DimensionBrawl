using UnityEngine;

namespace DimensionBrawl.Combat
{
    [CreateAssetMenu(
        fileName = "DB_BossBarragePattern",
        menuName = "DimensionBrawl/Combat/Boss Barrage Pattern")]
    public sealed class BossBarragePatternProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string patternId = "NeedleLock";

        [Header("Timing")]
        [SerializeField, Min(0f)] private float initialDelaySeconds = 1.2f;
        [SerializeField, Range(0.45f, 1.2f)] private float windupSeconds = 0.9f;
        [SerializeField, Min(0.5f)] private float waveIntervalSeconds = 5.6f;

        [Header("Projectile")]
        [SerializeField, Range(1, 9)] private int projectilesPerWave = 3;
        [SerializeField, Min(0f)] private float damage = 18f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 13f;
        [SerializeField, Min(0.1f)] private float projectileLifetimeSeconds = 4.5f;
        [SerializeField, Min(0f)] private float projectileRadius = 0.32f;

        [Header("Forward Risk Shape")]
        [Tooltip("Backline gaps should be wider so defensive play is safer but charges EN slower.")]
        [SerializeField, Min(0f)] private float backlineHalfSpread = 2.8f;
        [Tooltip("Forward-risk gaps are tighter, making aggressive EN charging more dangerous.")]
        [SerializeField, Min(0f)] private float forwardHalfSpread = 1.05f;
        [SerializeField, Min(0f)] private float spawnHeight = 1.25f;
        [SerializeField, Min(0f)] private float targetHeight = 1f;

        public string PatternId => patternId;
        public float InitialDelaySeconds => initialDelaySeconds;
        public float WindupSeconds => windupSeconds;
        public float WaveIntervalSeconds => waveIntervalSeconds;
        public int ProjectilesPerWave => projectilesPerWave;
        public float Damage => damage;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetimeSeconds => projectileLifetimeSeconds;
        public float ProjectileRadius => projectileRadius;
        public float SpawnHeight => spawnHeight;
        public float TargetHeight => targetHeight;

        public float EvaluateHalfSpread(float forwardRisk01)
        {
            return Mathf.Lerp(backlineHalfSpread, forwardHalfSpread, Mathf.Clamp01(forwardRisk01));
        }

        public float GetLateralOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float normalizedIndex = Mathf.Clamp01((float)projectileIndex / (safeCount - 1));
            return Mathf.Lerp(-halfSpread, halfSpread, normalizedIndex);
        }
    }
}
