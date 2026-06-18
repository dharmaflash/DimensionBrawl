using System;
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
        [SerializeField] private BossBarragePatternProfile[] patternSequence = new BossBarragePatternProfile[0];
        [SerializeField, Min(1)] private int wavesPerPattern = 1;
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
        private int patternSequenceIndex;
        private int wavesFiredInCurrentPattern;
        private BossBarragePatternProfile queuedPriorityPattern;
        private int queuedPriorityWavesRemaining;

        public event Action<BossBarrageEmitter, BossBarragePatternProfile> WindupStarted;
        public event Action<BossBarrageEmitter, BossBarragePatternProfile, int> WaveFired;

        public bool IsWindupActive => windupActive;
        public bool IsFiringEnabled => firingEnabled;
        public bool HasQueuedPriorityPattern => queuedPriorityPattern != null;
        public BossBarragePatternProfile QueuedPriorityPattern => queuedPriorityPattern;
        public float PendingForwardRisk01 => pendingForwardRisk01;
        public BossBarragePatternProfile CurrentPattern => ActivePattern;
        public int CurrentPatternSequenceIndex => patternSequenceIndex;
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

        private BossBarragePatternProfile ActivePattern => ResolveActivePattern();
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
            ClearQueuedPriorityPattern();
            PrewarmPool();
            ResetPatternSequence();
            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
        }

        public void ConfigurePatternSequence(BossBarragePatternProfile[] newPatternSequence, int newWavesPerPattern)
        {
            patternSequence = newPatternSequence != null
                ? (BossBarragePatternProfile[])newPatternSequence.Clone()
                : new BossBarragePatternProfile[0];
            wavesPerPattern = Mathf.Max(1, newWavesPerPattern);
            ClearQueuedPriorityPattern();
            ResetPatternSequence();
            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
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
                windupActive = false;
                ClearQueuedPriorityPattern();
                cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
                DeactivateActiveProjectiles();
                return;
            }

            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : cooldownTimer;
        }

        public bool QueuePriorityPattern(BossBarragePatternProfile priorityPattern, int waveCount = 1)
        {
            if (!CanQueuePriorityPattern(priorityPattern))
            {
                return false;
            }

            queuedPriorityPattern = priorityPattern;
            queuedPriorityWavesRemaining = Mathf.Max(1, waveCount);
            cooldownTimer = Mathf.Min(cooldownTimer, priorityPattern.InitialDelaySeconds);
            return true;
        }

        public bool CancelQueuedPriorityPattern(BossBarragePatternProfile priorityPattern)
        {
            if (queuedPriorityPattern == null
                || queuedPriorityPattern != priorityPattern
                || windupActive)
            {
                return false;
            }

            ClearQueuedPriorityPattern();
            return true;
        }

        public bool CanQueuePriorityPattern(BossBarragePatternProfile priorityPattern)
        {
            return priorityPattern != null
                && firingEnabled
                && !windupActive
                && queuedPriorityPattern == null;
        }

        public void Tick(float deltaTime)
        {
            if (sourceHealth != null && sourceHealth.IsAlive == false)
            {
                windupActive = false;
                DeactivateActiveProjectiles();
                return;
            }

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
            BossBarragePatternProfile activePattern = ActivePattern;
            if (activePattern == null || laneSpace == null || trackedPlayer == null)
            {
                return false;
            }

            Vector2 lanePoint = laneSpace.GetLaneCoordinates(trackedPlayer.position);
            float targetLateralX = activePattern.ResolveTargetLateralX(lanePoint.x, laneSpace.HalfWidth);
            pendingTargetLanePoint = new Vector2(
                Mathf.Clamp(targetLateralX, -laneSpace.HalfWidth, laneSpace.HalfWidth),
                Mathf.Clamp(lanePoint.y, laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ));
            pendingForwardRisk01 = laneSpace.EvaluateForwardRisk01(trackedPlayer.position);
            windupTimer = activePattern.WindupSeconds;
            windupActive = true;
            WindupStarted?.Invoke(this, activePattern);
            return true;
        }

        public int BuildPendingLaneTargetPreview(Vector2[] results)
        {
            BossBarragePatternProfile activePattern = ActivePattern;
            if (!windupActive || activePattern == null || laneSpace == null || results == null || results.Length == 0)
            {
                return 0;
            }

            int projectileCount = Mathf.Min(activePattern.ProjectilesPerWave, results.Length);
            for (int i = 0; i < projectileCount; i++)
            {
                float offset = activePattern.GetLateralOffset(i, activePattern.ProjectilesPerWave, pendingForwardRisk01);
                float depthOffset = activePattern.GetTargetDepthOffset(i, activePattern.ProjectilesPerWave, pendingForwardRisk01);
                results[i] = new Vector2(
                    Mathf.Clamp(pendingTargetLanePoint.x + offset, -laneSpace.HalfWidth, laneSpace.HalfWidth),
                    Mathf.Clamp(pendingTargetLanePoint.y + depthOffset, laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ));
            }

            return projectileCount;
        }

        public int FirePendingWave()
        {
            BossBarragePatternProfile activePattern = ActivePattern;
            if (!windupActive || activePattern == null || laneSpace == null)
            {
                return 0;
            }

            windupActive = false;
            int spawnedCount = 0;
            int projectileCount = activePattern.ProjectilesPerWave;
            for (int i = 0; i < projectileCount; i++)
            {
                float offset = activePattern.GetLateralOffset(i, projectileCount, pendingForwardRisk01);
                float depthOffset = activePattern.GetTargetDepthOffset(i, projectileCount, pendingForwardRisk01);
                if (TryFireProjectile(
                    activePattern,
                    pendingTargetLanePoint.x + offset,
                    pendingTargetLanePoint.y + depthOffset))
                {
                    spawnedCount++;
                }
            }

            cooldownTimer = activePattern.WaveIntervalSeconds;
            WaveFired?.Invoke(this, activePattern, spawnedCount);
            AdvancePatternSequenceAfterWave();
            return spawnedCount;
        }

        private void Awake()
        {
            if (projectileRoot == null)
            {
                projectileRoot = transform;
            }

            PrewarmPool();
            ResetPatternSequence();
            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private bool TryFireProjectile(BossBarragePatternProfile activePattern, float targetLateralX, float targetLaneZ)
        {
            BossBarrageProjectile projectile = GetInactiveProjectile();
            if (projectile == null)
            {
                return false;
            }

            Vector3 targetPoint = laneSpace.GetLaneWorldPoint(
                targetLateralX,
                Mathf.Clamp(targetLaneZ, laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ),
                activePattern.TargetHeight);
            Vector3 spawnPoint = laneSpace.GetLaneWorldPoint(
                Mathf.Lerp(pendingTargetLanePoint.x, targetLateralX, 0.35f),
                laneSpace.BossProxyZ,
                activePattern.SpawnHeight);
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
                activePattern.Damage,
                direction,
                activePattern.ProjectileSpeed,
                activePattern.ProjectileLifetimeSeconds,
                activePattern.ProjectileRadius);
            return true;
        }

        private BossBarragePatternProfile ResolveActivePattern()
        {
            if (queuedPriorityPattern != null)
            {
                return queuedPriorityPattern;
            }

            if (patternSequence != null && patternSequence.Length > 0)
            {
                int safeIndex = Mathf.Clamp(patternSequenceIndex, 0, patternSequence.Length - 1);
                BossBarragePatternProfile sequencedPattern = patternSequence[safeIndex];
                if (sequencedPattern != null)
                {
                    return sequencedPattern;
                }

                for (int i = 0; i < patternSequence.Length; i++)
                {
                    if (patternSequence[i] != null)
                    {
                        return patternSequence[i];
                    }
                }
            }

            return patternProfile;
        }

        private void ResetPatternSequence()
        {
            patternSequenceIndex = FindFirstValidPatternIndex();
            wavesFiredInCurrentPattern = 0;
        }

        private int FindFirstValidPatternIndex()
        {
            if (patternSequence == null || patternSequence.Length == 0)
            {
                return 0;
            }

            for (int i = 0; i < patternSequence.Length; i++)
            {
                if (patternSequence[i] != null)
                {
                    return i;
                }
            }

            return 0;
        }

        private void AdvancePatternSequenceAfterWave()
        {
            if (queuedPriorityPattern != null)
            {
                queuedPriorityWavesRemaining--;
                if (queuedPriorityWavesRemaining <= 0)
                {
                    ClearQueuedPriorityPattern();
                }

                return;
            }

            if (patternSequence == null || patternSequence.Length <= 1)
            {
                return;
            }

            wavesFiredInCurrentPattern++;
            if (wavesFiredInCurrentPattern < Mathf.Max(1, wavesPerPattern))
            {
                return;
            }

            wavesFiredInCurrentPattern = 0;
            int startIndex = Mathf.Clamp(patternSequenceIndex, 0, patternSequence.Length - 1);
            for (int step = 1; step <= patternSequence.Length; step++)
            {
                int candidateIndex = (startIndex + step) % patternSequence.Length;
                if (patternSequence[candidateIndex] != null)
                {
                    patternSequenceIndex = candidateIndex;
                    return;
                }
            }
        }

        private void ClearQueuedPriorityPattern()
        {
            queuedPriorityPattern = null;
            queuedPriorityWavesRemaining = 0;
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
