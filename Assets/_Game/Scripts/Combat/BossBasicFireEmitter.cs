using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossBasicFireEmitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedPlayer;
        [SerializeField] private CombatHealth sourceHealth;

        [Header("Fire")]
        [SerializeField] private BossBasicFireProfile fireProfile;
        [SerializeField] private BossBarrageProjectile projectilePrefab;
        [SerializeField] private GameObject projectilePrefabObject;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.Enemy;
        [SerializeField] private bool firingEnabled = true;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int prewarmCount = 10;
        [SerializeField] private Transform projectileRoot;

        private readonly List<BossBarrageProjectile> pool = new List<BossBarrageProjectile>(12);
        private float cooldownTimer;
        private float lastForwardRisk01;
        private int totalVolleysFired;
        private int lastVolleyProjectileCount;
        private Vector2 lastTargetLanePoint;

        public BossBasicFireProfile FireProfile => fireProfile;
        public bool IsFiringEnabled => firingEnabled;
        public float LastForwardRisk01 => lastForwardRisk01;
        public int TotalVolleysFired => totalVolleysFired;
        public int LastVolleyProjectileCount => lastVolleyProjectileCount;
        public Vector2 LastTargetLanePoint => lastTargetLanePoint;
        public float CooldownRemaining => cooldownTimer;
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

        public void ConfigureProfile(
            BossBasicFireProfile newFireProfile,
            BossBarrageProjectile newProjectilePrefab,
            int newPrewarmCount)
        {
            fireProfile = newFireProfile;
            projectilePrefab = newProjectilePrefab;
            projectilePrefabObject = newProjectilePrefab != null ? newProjectilePrefab.gameObject : null;
            prewarmCount = Mathf.Max(0, newPrewarmCount);
            PrewarmPool();
            cooldownTimer = fireProfile != null ? fireProfile.InitialDelaySeconds : 0f;
        }

        public void SetFiringEnabled(bool enabled)
        {
            if (firingEnabled == enabled)
            {
                if (!enabled)
                {
                    DeactivateActiveProjectiles();
                }

                return;
            }

            firingEnabled = enabled;
            if (!enabled)
            {
                cooldownTimer = fireProfile != null ? fireProfile.InitialDelaySeconds : 0f;
                DeactivateActiveProjectiles();
                return;
            }

            cooldownTimer = fireProfile != null ? fireProfile.InitialDelaySeconds : cooldownTimer;
        }

        public void Tick(float deltaTime)
        {
            if (sourceHealth != null && !sourceHealth.IsAlive)
            {
                DeactivateActiveProjectiles();
                return;
            }

            if (!firingEnabled || deltaTime <= 0f || fireProfile == null || laneSpace == null || trackedPlayer == null)
            {
                return;
            }

            cooldownTimer -= deltaTime;
            if (cooldownTimer <= 0f)
            {
                FireVolley();
                cooldownTimer = fireProfile.FireIntervalSeconds;
            }
        }

        public int FireVolley()
        {
            if (!firingEnabled || fireProfile == null || laneSpace == null || trackedPlayer == null)
            {
                return 0;
            }

            Vector2 trackedLanePoint = laneSpace.GetLaneCoordinates(trackedPlayer.position);
            float targetLaneZ = Mathf.Clamp(trackedLanePoint.y, laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ);
            lastForwardRisk01 = laneSpace.EvaluateForwardRisk01(trackedPlayer.position);
            lastTargetLanePoint = new Vector2(
                Mathf.Clamp(trackedLanePoint.x, -laneSpace.HalfWidth, laneSpace.HalfWidth),
                targetLaneZ);

            int projectileCount = fireProfile.ProjectilesPerVolley;
            int spawnedCount = 0;
            for (int i = 0; i < projectileCount; i++)
            {
                float offset = fireProfile.GetLateralOffset(i, projectileCount, lastForwardRisk01);
                float targetLateralX = Mathf.Clamp(lastTargetLanePoint.x + offset, -laneSpace.HalfWidth, laneSpace.HalfWidth);
                if (TryFireProjectile(targetLateralX, targetLaneZ))
                {
                    spawnedCount++;
                }
            }

            totalVolleysFired++;
            lastVolleyProjectileCount = spawnedCount;
            return spawnedCount;
        }

        private void Awake()
        {
            if (projectileRoot == null)
            {
                projectileRoot = transform;
            }

            PrewarmPool();
            cooldownTimer = fireProfile != null ? fireProfile.InitialDelaySeconds : 0f;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private bool TryFireProjectile(float targetLateralX, float targetLaneZ)
        {
            BossBarrageProjectile projectile = GetInactiveProjectile();
            if (projectile == null)
            {
                return false;
            }

            Vector2 sourceLanePoint = laneSpace.GetLaneCoordinates(transform.position);
            float spawnLaneZ = Mathf.Clamp(sourceLanePoint.y, laneSpace.ForwardBoundaryZ, laneSpace.BossProxyZ);
            float spawnLateralX = Mathf.Lerp(
                Mathf.Clamp(sourceLanePoint.x, -laneSpace.HalfWidth, laneSpace.HalfWidth),
                targetLateralX,
                fireProfile.SpawnLateralFollowRatio);
            Vector3 spawnPoint = laneSpace.GetLaneWorldPoint(
                spawnLateralX,
                spawnLaneZ,
                fireProfile.SpawnHeight);
            Vector3 targetPoint = laneSpace.GetLaneWorldPoint(
                targetLateralX,
                targetLaneZ,
                fireProfile.TargetHeight);
            Vector3 direction = targetPoint - spawnPoint;
            projectile.transform.SetPositionAndRotation(
                spawnPoint,
                Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, Vector3.up), Vector3.up));
            DamageTeam resolvedSourceTeam = sourceHealth != null && sourceHealth.Team != DamageTeam.Neutral
                ? sourceHealth.Team
                : sourceTeam;
            projectile.ApplyPresentation(
                fireProfile.ProjectileColor,
                fireProfile.ProjectileVisualScale,
                fireProfile.ProjectileMaterial);
            projectile.Configure(
                sourceHealth,
                resolvedSourceTeam,
                fireProfile.Damage,
                direction,
                fireProfile.ProjectileSpeed,
                fireProfile.ProjectileLifetimeSeconds,
                fireProfile.ProjectileRadius,
                fireProfile.DamageResponsePolicy,
                fireProfile.ControlLockPolicy);
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
                projectile.name = $"{activeProjectilePrefab.name}_BasicPooled_{pool.Count:00}";
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

        private void DeactivateActiveProjectiles()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                BossBarrageProjectile projectile = pool[i];
                if (projectile != null && projectile.IsActive)
                {
                    projectile.Deactivate();
                }
            }
        }
    }
}
