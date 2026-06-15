using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum BossBarrageLateralShape
    {
        CenterSpread = 0,
        TwinColumns = 1,
        SideClamp = 2,
    }

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
        [SerializeField] private BossBarrageLateralShape lateralShape = BossBarrageLateralShape.CenterSpread;
        [Tooltip("Backline gaps should be wider so defensive play is safer but charges EN slower.")]
        [SerializeField, Min(0f)] private float backlineHalfSpread = 2.8f;
        [Tooltip("Forward-risk gaps are tighter, making aggressive EN charging more dangerous.")]
        [SerializeField, Min(0f)] private float forwardHalfSpread = 1.05f;
        [Tooltip("Twin-column patterns leave a readable middle gap while still tightening near the forward boundary.")]
        [SerializeField, Range(0f, 0.8f)] private float twinColumnInnerSpreadRatio = 0.38f;
        [Tooltip("Negative values clamp from the left, positive values clamp from the right.")]
        [SerializeField, Range(-1f, 1f)] private float sideClampDirection = -1f;
        [Tooltip("How far the clamped side reaches across center. Higher values leave a smaller opposite-side gap.")]
        [SerializeField, Range(0f, 0.65f)] private float sideClampCrossReachRatio = 0.28f;
        [SerializeField, Min(0f)] private float spawnHeight = 1.25f;
        [SerializeField, Min(0f)] private float targetHeight = 1f;

        public string PatternId => patternId;
        public BossBarrageLateralShape LateralShape => lateralShape;
        public float InitialDelaySeconds => initialDelaySeconds;
        public float WindupSeconds => windupSeconds;
        public float WaveIntervalSeconds => waveIntervalSeconds;
        public int ProjectilesPerWave => projectilesPerWave;
        public float Damage => damage;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetimeSeconds => projectileLifetimeSeconds;
        public float ProjectileRadius => projectileRadius;
        public float SideClampDirection => sideClampDirection;
        public float SideClampCrossReachRatio => sideClampCrossReachRatio;
        public float SpawnHeight => spawnHeight;
        public float TargetHeight => targetHeight;

        public float EvaluateHalfSpread(float forwardRisk01)
        {
            return Mathf.Lerp(backlineHalfSpread, forwardHalfSpread, Mathf.Clamp01(forwardRisk01));
        }

        public float GetLateralOffset(int projectileIndex, int count, float forwardRisk01)
        {
            if (lateralShape == BossBarrageLateralShape.TwinColumns)
            {
                return GetTwinColumnOffset(projectileIndex, count, forwardRisk01);
            }

            if (lateralShape == BossBarrageLateralShape.SideClamp)
            {
                return GetSideClampOffset(projectileIndex, count, forwardRisk01);
            }

            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float normalizedIndex = Mathf.Clamp01((float)projectileIndex / (safeCount - 1));
            return Mathf.Lerp(-halfSpread, halfSpread, normalizedIndex);
        }

        private float GetTwinColumnOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float innerSpread = halfSpread * Mathf.Clamp01(twinColumnInnerSpreadRatio);
            if (safeCount <= 2)
            {
                return projectileIndex <= 0 ? -halfSpread : halfSpread;
            }

            int safeIndex = Mathf.Clamp(projectileIndex, 0, safeCount - 1);
            int leftCount = safeCount / 2;
            bool isLeft = safeIndex < leftCount;
            int sideIndex = isLeft ? safeIndex : safeIndex - leftCount;
            int sideCount = isLeft ? leftCount : safeCount - leftCount;
            float side01 = sideCount <= 1 ? 0f : Mathf.Clamp01((float)sideIndex / (sideCount - 1));
            float magnitude = isLeft
                ? Mathf.Lerp(halfSpread, innerSpread, side01)
                : Mathf.Lerp(innerSpread, halfSpread, side01);

            return isLeft ? -magnitude : magnitude;
        }

        private float GetSideClampOffset(int projectileIndex, int count, float forwardRisk01)
        {
            int safeCount = Mathf.Max(1, count);
            if (safeCount <= 1)
            {
                return 0f;
            }

            float halfSpread = EvaluateHalfSpread(forwardRisk01);
            float direction = sideClampDirection < 0f ? -1f : 1f;
            float outerEdge = direction * halfSpread;
            float crossReach = -direction * halfSpread * Mathf.Clamp01(sideClampCrossReachRatio);
            float normalizedIndex = Mathf.Clamp01((float)Mathf.Clamp(projectileIndex, 0, safeCount - 1) / (safeCount - 1));
            return Mathf.Lerp(outerEdge, crossReach, normalizedIndex);
        }
    }
}
