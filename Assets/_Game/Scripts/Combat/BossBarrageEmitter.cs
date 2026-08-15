using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public sealed class BossBarrageEmitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedPlayer;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private Transform[] projectileSpawnOrigins = Array.Empty<Transform>();

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
        private readonly List<BossBarrageProjectile> projectilesInPoolOrder =
            new List<BossBarrageProjectile>(16);
        private readonly Dictionary<BossBarrageProjectile, List<BossBarrageProjectile>> standbyPools =
            new Dictionary<BossBarrageProjectile, List<BossBarrageProjectile>>();
        private BossBarrageProjectile pooledProjectilePrefab;
        private SpatialOneShotAudioPool impactAudioPool;
        private Collider[] sourceBodyColliders = Array.Empty<Collider>();
        private CombatHealth sourceBodyColliderOwner;
        private bool sourceBodyCollidersCaptured;
        private float cooldownTimer;
        private float windupTimer;
        private bool windupActive;
        private Vector2 pendingTargetLanePoint;
        private float pendingForwardRisk01;
        private int patternSequenceIndex;
        private int wavesFiredInCurrentPattern;
        private BossBarragePatternProfile queuedPriorityPattern;
        private int queuedPriorityWavesRemaining;
        private bool lastFiredWaveWasPriority;
        private int spawnOriginWaveCursor;

        public event Action<BossBarrageEmitter, BossBarragePatternProfile> WindupStarted;
        public event Action<BossBarrageEmitter, BossBarragePatternProfile, int> WaveFired;

        public bool IsWindupActive => windupActive;
        public bool IsFiringEnabled => firingEnabled;
        public bool HasQueuedPriorityPattern => queuedPriorityPattern != null;
        public BossBarragePatternProfile QueuedPriorityPattern => queuedPriorityPattern;
        public bool CurrentPatternIsPriority => queuedPriorityPattern != null;
        public bool LastFiredWaveWasPriority => lastFiredWaveWasPriority;
        public int QueuedPriorityWavesRemaining => queuedPriorityWavesRemaining;
        public int PooledProjectileCount => pool.Count;
        public BossBarrageProjectile PooledProjectilePrefab => pooledProjectilePrefab;
        public float PendingForwardRisk01 => pendingForwardRisk01;
        public BossBarragePatternProfile CurrentPattern => ActivePattern;
        public int CurrentPatternSequenceIndex => patternSequenceIndex;
        public int ConfiguredSpawnOriginCount => projectileSpawnOrigins != null
            ? projectileSpawnOrigins.Length
            : 0;
        public int ActiveProjectileCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < projectilesInPoolOrder.Count; i++)
                {
                    BossBarrageProjectile projectile = projectilesInPoolOrder[i];
                    if (projectile != null && projectile.IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Copies every live projectile owned by this emitter in deterministic
        /// creation order. Callers can reuse the destination list without this
        /// method creating a temporary collection.
        /// </summary>
        public int CopyActiveProjectiles(List<BossBarrageProjectile> destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.Clear();
            for (int i = 0; i < projectilesInPoolOrder.Count; i++)
            {
                BossBarrageProjectile projectile = projectilesInPoolOrder[i];
                if (projectile != null && projectile.IsActive)
                {
                    destination.Add(projectile);
                }
            }

            return destination.Count;
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
            if (sourceHealth != newSourceHealth)
            {
                InvalidateSourceBodyColliderCache();
            }

            sourceHealth = newSourceHealth;
        }

        public void ConfigureSpawnOrigins(Transform[] newSpawnOrigins)
        {
            projectileSpawnOrigins = newSpawnOrigins != null
                ? Array.FindAll(newSpawnOrigins, origin => origin != null)
                : Array.Empty<Transform>();
            spawnOriginWaveCursor = 0;
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
            EnsureImpactAudioPool();
            PrewarmPool();
            ResetPatternSequence();
            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
        }

        public void PrewarmProjectilePrefab(
            BossBarrageProjectile targetProjectilePrefab,
            int targetCount)
        {
            int resolvedCount = Mathf.Max(0, targetCount);
            if (targetProjectilePrefab == null || resolvedCount <= 0)
            {
                return;
            }

            if (pooledProjectilePrefab == targetProjectilePrefab)
            {
                EnsurePoolSize(pool, targetProjectilePrefab, resolvedCount, "Pooled");
                return;
            }

            if (!standbyPools.TryGetValue(targetProjectilePrefab, out List<BossBarrageProjectile> standby))
            {
                standby = new List<BossBarrageProjectile>(resolvedCount);
                standbyPools.Add(targetProjectilePrefab, standby);
            }

            EnsurePoolSize(standby, targetProjectilePrefab, resolvedCount, "Standby");
        }

        public int GetProjectilePoolCountForPrefab(BossBarrageProjectile targetProjectilePrefab)
        {
            if (targetProjectilePrefab == null)
            {
                return 0;
            }

            if (pooledProjectilePrefab == targetProjectilePrefab)
            {
                return pool.Count;
            }

            return standbyPools.TryGetValue(targetProjectilePrefab, out List<BossBarrageProjectile> standby)
                ? standby.Count
                : 0;
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

            return QueuePriorityPatternInternal(priorityPattern, waveCount);
        }

        /// <summary>
        /// Reserves the next authored pattern while encounter pacing deliberately
        /// pauses firing. Unlike QueuePriorityPattern, this explicit handoff API
        /// does not require firing to be enabled; the queued pattern remains
        /// dormant until the next firing window opens.
        /// </summary>
        public bool QueuePriorityPatternForNextFiringWindow(
            BossBarragePatternProfile priorityPattern,
            int waveCount = 1)
        {
            if (priorityPattern == null || windupActive)
            {
                return false;
            }

            if (queuedPriorityPattern != null)
            {
                return queuedPriorityPattern == priorityPattern
                    && queuedPriorityWavesRemaining >= Mathf.Max(1, waveCount);
            }

            return QueuePriorityPatternInternal(priorityPattern, waveCount);
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

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                BossCombatCadenceScheduler.Register(this);
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                BossCombatCadenceScheduler.Unregister(this);
            }
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
            lastFiredWaveWasPriority = queuedPriorityPattern != null;
            int spawnedCount = 0;
            int projectileCount = activePattern.ProjectilesPerWave;
            int waveOriginCursor = spawnOriginWaveCursor;
            for (int i = 0; i < projectileCount; i++)
            {
                float offset = activePattern.GetLateralOffset(i, projectileCount, pendingForwardRisk01);
                float depthOffset = activePattern.GetTargetDepthOffset(i, projectileCount, pendingForwardRisk01);
                if (TryFireProjectile(
                    activePattern,
                    pendingTargetLanePoint.x + offset,
                    pendingTargetLanePoint.y + depthOffset,
                    waveOriginCursor + i))
                {
                    spawnedCount++;
                }
            }

            if (ConfiguredSpawnOriginCount > 0)
            {
                // The setup orders six Akaza muzzles as A,F,B,E,C,D. Advancing by
                // one mirrored pair yields three deterministic, readable waves.
                spawnOriginWaveCursor = (spawnOriginWaveCursor + 2) % ConfiguredSpawnOriginCount;
            }

            cooldownTimer = activePattern.WaveIntervalSeconds;
            WaveFired?.Invoke(this, activePattern, spawnedCount);
            AdvancePatternSequenceAfterWave();
            return spawnedCount;
        }

        private void Awake()
        {
            CombatTimeDilationReceiver.Ensure(gameObject);
            if (projectileRoot == null)
            {
                projectileRoot = transform;
            }

            EnsureImpactAudioPool();
            PrewarmPool();
            ResetPatternSequence();
            cooldownTimer = ActivePattern != null ? ActivePattern.InitialDelaySeconds : 0f;
        }

        private bool TryFireProjectile(
            BossBarragePatternProfile activePattern,
            float targetLateralX,
            float targetLaneZ,
            int spawnOriginIndex)
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
            Vector3 spawnPoint;
            Transform authoredOrigin = ResolveConfiguredSpawnOrigin(spawnOriginIndex);
            if (authoredOrigin != null)
            {
                spawnPoint = authoredOrigin.position;
            }
            else
            {
                spawnPoint = laneSpace.GetLaneWorldPoint(
                    Mathf.Lerp(pendingTargetLanePoint.x, targetLateralX, 0.35f),
                    laneSpace.BossProxyZ,
                    activePattern.SpawnHeight);
            }
            projectile.RecordAuthoredSpawnOrigin(
                spawnPoint,
                authoredOrigin != null,
                targetPoint);
            spawnPoint = ResolveSourceClearedSpawnPoint(
                spawnPoint,
                targetPoint,
                activePattern.ProjectileRadius,
                applySourceRootClearance: authoredOrigin == null);
            Vector3 direction = targetPoint - spawnPoint;
            projectile.transform.SetPositionAndRotation(
                spawnPoint,
                Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, Vector3.up), Vector3.up));
            DamageTeam resolvedSourceTeam = sourceHealth != null && sourceHealth.Team != DamageTeam.Neutral
                ? sourceHealth.Team
                : sourceTeam;
            projectile.ApplyPresentation(
                activePattern.ProjectileColor,
                activePattern.ProjectileVisualScale,
                activePattern.ProjectileMaterial);
            projectile.Configure(
                sourceHealth,
                resolvedSourceTeam,
                activePattern.Damage,
                direction,
                activePattern.ProjectileSpeed,
                activePattern.ProjectileLifetimeSeconds,
                activePattern.ProjectileRadius,
                activePattern.DamageResponsePolicy,
                activePattern.ControlLockPolicy,
                impactAudioPool);
            return true;
        }

        private Transform ResolveConfiguredSpawnOrigin(int index)
        {
            int count = ConfiguredSpawnOriginCount;
            if (count <= 0)
            {
                return null;
            }

            int safeIndex = ((index % count) + count) % count;
            return projectileSpawnOrigins[safeIndex];
        }

        private Vector3 ResolveSourceClearedSpawnPoint(
            Vector3 spawnPoint,
            Vector3 targetPoint,
            float projectileRadius,
            bool applySourceRootClearance)
        {
            Collider[] sourceColliders = ResolveSourceBodyColliders();
            if (sourceColliders.Length == 0)
            {
                return spawnPoint;
            }

            Vector3 direction = targetPoint - spawnPoint;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return spawnPoint;
            }

            Vector3 forward = applySourceRootClearance
                ? direction.normalized
                : Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                return spawnPoint;
            }
            float radius = Mathf.Max(0.05f, projectileRadius);
            Vector3 resolvedSpawnPoint = spawnPoint;
            float sourceRadius = ResolveSourceColliderMaxExtent(sourceColliders);
            // The legacy lane-space origin is synthesized around sourceHealth and
            // needs a root-radius clearance. An authored muzzle is already the
            // authoritative body-exit point; applying the root-relative distance
            // again can move absolute-coordinate rigs many metres off the weapon.
            if (applySourceRootClearance && sourceHealth != null && sourceRadius > 0f)
            {
                float minimumForwardDistance = sourceRadius + radius + 0.05f;
                float currentForwardDistance = Vector3.Dot(
                    resolvedSpawnPoint - sourceHealth.transform.position,
                    forward);
                if (currentForwardDistance < minimumForwardDistance)
                {
                    resolvedSpawnPoint += forward * (minimumForwardDistance - currentForwardDistance);
                }
            }

            float stepDistance = Mathf.Max(0.05f, radius * 0.5f);
            float maxDistance = Mathf.Max(2.5f, radius * 6f);
            for (float distance = 0f; distance <= maxDistance; distance += stepDistance)
            {
                Vector3 candidate = resolvedSpawnPoint + forward * distance;
                if (!OverlapsAnySourceCollider(candidate, radius, sourceColliders))
                {
                    return candidate;
                }
            }

            return resolvedSpawnPoint + forward * maxDistance;
        }

        private static float ResolveSourceColliderMaxExtent(IReadOnlyList<Collider> sourceColliders)
        {
            float maxExtent = 0f;
            for (int i = 0; i < sourceColliders.Count; i++)
            {
                Collider sourceCollider = sourceColliders[i];
                if (sourceCollider == null)
                {
                    continue;
                }

                Vector3 extents = sourceCollider.bounds.extents;
                maxExtent = Mathf.Max(maxExtent, Mathf.Abs(extents.x), Mathf.Abs(extents.y), Mathf.Abs(extents.z));
            }

            return maxExtent;
        }

        private static bool OverlapsAnySourceCollider(
            Vector3 position,
            float radius,
            IReadOnlyList<Collider> sourceColliders)
        {
            float radiusSquared = radius * radius;
            for (int i = 0; i < sourceColliders.Count; i++)
            {
                Collider sourceCollider = sourceColliders[i];
                if (sourceCollider == null || !sourceCollider.enabled)
                {
                    continue;
                }

                if (sourceCollider.bounds.SqrDistance(position) <= radiusSquared)
                {
                    return true;
                }
            }

            return false;
        }

        private Collider[] ResolveSourceBodyColliders()
        {
            if (sourceHealth == null)
            {
                InvalidateSourceBodyColliderCache();
                return Array.Empty<Collider>();
            }

            if (sourceBodyCollidersCaptured && sourceBodyColliderOwner == sourceHealth)
            {
                return sourceBodyColliders;
            }

            sourceBodyColliderOwner = sourceHealth;
            sourceBodyColliders = sourceHealth.GetComponents<Collider>();
            if (sourceBodyColliders.Length == 0)
            {
                sourceBodyColliders = sourceHealth.GetComponentsInChildren<Collider>();
            }

            sourceBodyCollidersCaptured = true;
            return sourceBodyColliders;
        }

        private void InvalidateSourceBodyColliderCache()
        {
            sourceBodyColliders = Array.Empty<Collider>();
            sourceBodyColliderOwner = null;
            sourceBodyCollidersCaptured = false;
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

        private bool QueuePriorityPatternInternal(
            BossBarragePatternProfile priorityPattern,
            int waveCount)
        {
            queuedPriorityPattern = priorityPattern;
            queuedPriorityWavesRemaining = Mathf.Max(1, waveCount);
            cooldownTimer = Mathf.Min(cooldownTimer, priorityPattern.InitialDelaySeconds);
            return true;
        }

        private void PrewarmPool()
        {
            BossBarrageProjectile activeProjectilePrefab = ActiveProjectilePrefab;
            if (activeProjectilePrefab == null)
            {
                return;
            }

            SwitchActiveProjectilePool(activeProjectilePrefab);
            EnsurePoolSize(pool, activeProjectilePrefab, prewarmCount, "Pooled");
        }

        private void SwitchActiveProjectilePool(BossBarrageProjectile activeProjectilePrefab)
        {
            if (pooledProjectilePrefab == activeProjectilePrefab)
            {
                return;
            }

            if (pooledProjectilePrefab != null && pool.Count > 0)
            {
                if (!standbyPools.TryGetValue(
                    pooledProjectilePrefab,
                    out List<BossBarrageProjectile> previousPool))
                {
                    previousPool = new List<BossBarrageProjectile>(pool.Count);
                    standbyPools.Add(pooledProjectilePrefab, previousPool);
                }

                previousPool.AddRange(pool);
            }

            pool.Clear();
            if (standbyPools.TryGetValue(
                activeProjectilePrefab,
                out List<BossBarrageProjectile> nextPool))
            {
                pool.AddRange(nextPool);
                standbyPools.Remove(activeProjectilePrefab);
            }

            pooledProjectilePrefab = activeProjectilePrefab;
        }

        private void EnsurePoolSize(
            List<BossBarrageProjectile> targetPool,
            BossBarrageProjectile targetProjectilePrefab,
            int targetCount,
            string nameSuffix)
        {
            Transform root = projectileRoot != null ? projectileRoot : transform;
            while (targetPool.Count < targetCount)
            {
                BossBarrageProjectile projectile = Instantiate(targetProjectilePrefab, root);
                projectile.name =
                    $"{targetProjectilePrefab.name}_{nameSuffix}_{targetPool.Count:00}";
                projectile.Deactivate();
                targetPool.Add(projectile);
                projectilesInPoolOrder.Add(projectile);
            }
        }

        private void EnsureImpactAudioPool()
        {
            if (impactAudioPool == null)
            {
                impactAudioPool = GetComponent<SpatialOneShotAudioPool>();
                if (impactAudioPool == null)
                {
                    impactAudioPool = gameObject.AddComponent<SpatialOneShotAudioPool>();
                }
            }

            impactAudioPool.ConfigurePrewarmCount(Mathf.Clamp(prewarmCount, 4, 16));
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
            for (int i = 0; i < projectilesInPoolOrder.Count; i++)
            {
                BossBarrageProjectile projectile = projectilesInPoolOrder[i];
                if (projectile != null && projectile.IsActive)
                {
                    projectile.Deactivate();
                }
            }
        }
    }
}
