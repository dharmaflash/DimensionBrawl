using UnityEngine;

namespace DimensionBrawl.Combat
{
    [CreateAssetMenu(
        fileName = "DB_BossBasicFire",
        menuName = "DimensionBrawl/Combat/Boss Basic Fire")]
    public sealed class BossBasicFireProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string fireId = "LanePoke";
        [SerializeField] private string readoutLabel = "Basic Fire";

        [Header("Timing")]
        [SerializeField, Min(0f)] private float initialDelaySeconds = 1.1f;
        [SerializeField, Min(0.15f)] private float fireIntervalSeconds = 2.2f;

        [Header("Projectile")]
        [SerializeField, Range(1, 3)] private int projectilesPerVolley = 2;
        [SerializeField, Min(0f)] private float damage = 5f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 11.5f;
        [SerializeField, Min(0.1f)] private float projectileLifetimeSeconds = 5.2f;
        [SerializeField, Min(0f)] private float projectileRadius = 0.22f;
        [SerializeField] private DamageResponsePolicy damageResponsePolicy = DamageResponsePolicy.FlashOnly;
        [SerializeField] private CombatControlLockPolicy controlLockPolicy = CombatControlLockPolicy.None;

        [Header("Forward Risk Shape")]
        [SerializeField, Min(0f)] private float backlineHalfSpread = 1.45f;
        [SerializeField, Min(0f)] private float forwardHalfSpread = 0.48f;
        [SerializeField, Range(0f, 1f)] private float spawnLateralFollowRatio = 0.2f;
        [SerializeField, Min(0f)] private float spawnHeight = 1.28f;
        [SerializeField, Min(0f)] private float targetHeight = 1.02f;

        [Header("Projectile Read")]
        [SerializeField] private Color projectileColor = new Color(0.7f, 0.95f, 1f, 1f);
        [SerializeField] private Vector3 projectileVisualScale = new Vector3(0.62f, 0.62f, 0.62f);
        [SerializeField] private Material projectileMaterial;

        public string FireId => string.IsNullOrWhiteSpace(fireId) ? name : fireId;
        public string ReadoutLabel => string.IsNullOrWhiteSpace(readoutLabel) ? FireId : readoutLabel;
        public float InitialDelaySeconds => initialDelaySeconds;
        public float FireIntervalSeconds => fireIntervalSeconds;
        public int ProjectilesPerVolley => projectilesPerVolley;
        public float Damage => damage;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetimeSeconds => projectileLifetimeSeconds;
        public float ProjectileRadius => projectileRadius;
        public DamageResponsePolicy DamageResponsePolicy =>
            damageResponsePolicy == DamageResponsePolicy.Default ? DamageResponsePolicy.FlashOnly : damageResponsePolicy;
        public CombatControlLockPolicy ControlLockPolicy => controlLockPolicy;
        public float SpawnLateralFollowRatio => spawnLateralFollowRatio;
        public float SpawnHeight => spawnHeight;
        public float TargetHeight => targetHeight;
        public Color ProjectileColor => projectileColor;
        public Vector3 ProjectileVisualScale => SanitizeVisualScale(projectileVisualScale);
        public Material ProjectileMaterial => projectileMaterial;

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
            float normalizedIndex = Mathf.Clamp01((float)Mathf.Clamp(projectileIndex, 0, safeCount - 1) / (safeCount - 1));
            return Mathf.Lerp(-halfSpread, halfSpread, normalizedIndex);
        }

        private static Vector3 SanitizeVisualScale(Vector3 scale)
        {
            return new Vector3(
                Mathf.Max(0.05f, scale.x),
                Mathf.Max(0.05f, scale.y),
                Mathf.Max(0.05f, scale.z));
        }
    }
}
