using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public sealed class BossBarrageEmitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedPlayer;
        [SerializeField] private CombatHealth sourceHealth;

        [Header("Pattern")]
        [SerializeField] private BossBarragePatternProfile patternProfile;
        [SerializeField] private BossBarrageProjectile projectilePrefab;
        [SerializeField] private GameObject projectilePrefabObject;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.Enemy;
        [SerializeField] private bool firingEnabled = true;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int prewarmCount = 12;
        [SerializeField] private Transform projectileRoot;

        private readonly List<BossBarrageProjectile> pool = new List<BossBarrageProjectile>(16);
        private float cooldownTimer;
        private float windupTimer;
        private bool windupActive;
        private Vector2 pendingTargetLanePoint;
        private float pendingForwardRisk01;

        public bool IsWindupActive => windupActive;
        public float PendingForwardRisk01 => pendingForwardRisk01;
        public int ActiveProjectileCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < pool.Count; i++)
                {
                    if (pool[i] != null && pool[i].IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private BossBarragePatternProfile ActivePattern => patternProfile;
        private BossBarrageProjectile ActiveProjectilePrefab =>
            projectilePrefab != null
                ? projectilePrefab
                : projectilePrefabObject != null
                    ? projectilePrefabObject.GetComponent<BossBarrageProjectile>()
                    : null;

        public void ConfigureReferences(
            SummonLaneSpace newLaneSpace,
            Transform newTrackedPlayer,
            CombatHealth newSourceHealth)
        {
            laneSpace = newLaneSpace;
            trackedPlayer = newTrackedPlayer;
            sourceHealth = newSourceHealth;
        }

        public void ConfigurePattern(
            BossBarragePatternProfile newPatternProfile,
            BossBarrageProjectile newProjectilePrefab,
            int newPrewarmCount)
        {
            patternProfile = newPatternProfile;
            projectilePrefab = newProjectilePrefab;
            projectilePrefabObject = newProjectilePrefab != null ? newProjectilePrefab.gameObject : null;
            prewarmCount = Mathf.Max(0, newPrewarmCount);
            PrewarmPool();
            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
        }

        public void SetFiringEnabled(bool enabled)
        {
            firingEnabled = enabled;
        }

        public void Tick(float deltaTime)
        {
            if (!firingEnabled || deltaTime <= 0f || ActivePattern == null || laneSpace == null || trackedPlayer == null)
            {
                return;
            }

            if (windupActive)
            {
                windupTimer -= deltaTime;
                if (windupTimer <= 0f)
                {
                    FirePendingWave();
                }

                return;
            }

            cooldownTimer -= deltaTime;
            if (cooldownTimer <= 0f)
            {
                BeginWindup();
            }
        }

        public bool BeginWindup()
        {
            if (ActivePattern == null || laneSpace == null || trackedPlayer == null)
            {
                return false;
            }

            Vector2 lanePoint = laneSpace.GetLaneCoordinates(trackedPlayer.position);
            pendingTargetLanePoint = new Vector2(
                Mathf.Clamp(lanePoint.x, -laneSpace.HalfWidth, laneSpace.HalfWidth),
                Mathf.Clamp(lanePoint.y, laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ));
            pendingForwardRisk01 = laneSpace.EvaluateForwardRisk01(trackedPlayer.position);
            windupTimer = ActivePattern.WindupSeconds;
            windupActive = true;
            return true;
        }

        public int FirePendingWave()
        {
            if (!windupActive || ActivePattern == null || laneSpace == null)
            {
                return 0;
            }

            windupActive = false;
            int spawnedCount = 0;
            int projectileCount = ActivePattern.ProjectilesPerWave;
            for (int i = 0; i < projectileCount; i++)
            {
                float offset = ActivePattern.GetLateralOffset(i, projectileCount, pendingForwardRisk01);
                if (TryFireProjectile(pendingTargetLanePoint.x + offset))
                {
                    spawnedCount++;
                }
            }

            cooldownTimer = ActivePattern.WaveIntervalSeconds;
            return spawnedCount;
        }

        private void Awake()
        {
            if (projectileRoot == null)
            {
                projectileRoot = transform;
            }

            PrewarmPool();
            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private bool TryFireProjectile(float targetLateralX)
        {
            BossBarrageProjectile projectile = GetInactiveProjectile();
            if (projectile == null)
            {
                return false;
            }

            Vector3 targetPoint = laneSpace.GetLaneWorldPoint(
                targetLateralX,
                pendingTargetLanePoint.y,
                ActivePattern.TargetHeight);
            Vector3 spawnPoint = laneSpace.GetLaneWorldPoint(
                Mathf.Lerp(pendingTargetLanePoint.x, targetLateralX, 0.35f),
                laneSpace.BossProxyZ,
                ActivePattern.SpawnHeight);
            Vector3 direction = targetPoint - spawnPoint;
            projectile.transform.SetPositionAndRotation(
                spawnPoint,
                Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, Vector3.up), Vector3.up));
            DamageTeam resolvedSourceTeam = sourceHealth != null && sourceHealth.Team != DamageTeam.Neutral
                ? sourceHealth.Team
                : sourceTeam;
            projectile.Configure(
                sourceHealth,
                resolvedSourceTeam,
                ActivePattern.Damage,
                direction,
                ActivePattern.ProjectileSpeed,
                ActivePattern.ProjectileLifetimeSeconds,
                ActivePattern.ProjectileRadius);
            return true;
        }

        private void PrewarmPool()
        {
            BossBarrageProjectile activeProjectilePrefab = ActiveProjectilePrefab;
            if (activeProjectilePrefab == null || prewarmCount <= pool.Count)
            {
                return;
            }

            Transform root = projectileRoot != null ? projectileRoot : transform;
            while (pool.Count < prewarmCount)
            {
                BossBarrageProjectile projectile = Instantiate(activeProjectilePrefab, root);
                projectile.name = $"{activeProjectilePrefab.name}_Pooled_{pool.Count:00}";
                projectile.Deactivate();
                pool.Add(projectile);
            }
        }

        private BossBarrageProjectile GetInactiveProjectile()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].IsActive)
                {
                    return pool[i];
                }
            }

            return null;
        }
    }
}
