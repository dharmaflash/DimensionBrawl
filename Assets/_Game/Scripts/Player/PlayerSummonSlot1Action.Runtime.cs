using System;
using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Player
{
    public sealed partial class PlayerSummonSlot1Action
    {
        private sealed class SummonExecutionRuntime
        {
            private readonly PlayerSummonSlot1Action owner;
            private readonly List<LaneActionProjectile> projectiles = new List<LaneActionProjectile>();
            private readonly Queue<LaneActionProjectile> projectilePool = new Queue<LaneActionProjectile>();
            private readonly List<GameObject> entryCues = new List<GameObject>();
            private readonly Queue<GameObject> entryCuePool = new Queue<GameObject>();
            private readonly List<SummonFrontlineProxy> summonActors = new List<SummonFrontlineProxy>();
            private readonly Queue<SummonFrontlineProxy> summonActorPool = new Queue<SummonFrontlineProxy>();

            public SummonExecutionRuntime(PlayerSummonSlot1Action owner)
            {
                this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public int LastFiredProjectileCount { get; private set; }
            public int LastPressureScreenMaxIntercepts { get; private set; }
            public int LastPressureScreenInterceptCount { get; private set; }
            public int LastPressureScreenInterceptTier { get; private set; }
            public int TotalPressureScreenInterceptCount { get; private set; }
            public Vector3 LastEntryPosition { get; private set; }
            public Vector3 LastSummonActorPosition { get; private set; }
            public int ActiveProjectileCount => CountActiveProjectiles();
            public int ActiveCueCount => CountActiveCues();
            public int ActiveSummonActorCount => CountActiveSummonActors();
            public int ActivePressureScreenCount => CountActivePressureScreens();
            public int ActivePressureScreenRemainingIntercepts => CountActivePressureScreenRemainingIntercepts();

            public void Prewarm()
            {
                PrewarmProjectiles();
                PrewarmEntryCues();
                PrewarmSummonActors();
            }

            public void Detach()
            {
                UnsubscribePressureScreens();
            }

            public void FireTier(int tier)
            {
                SummonTierSettings settings = owner.ResolveTierSettings(tier);
                LastPressureScreenMaxIntercepts = 0;
                LastPressureScreenInterceptCount = 0;
                LastPressureScreenInterceptTier = 0;

                Vector2 playerLane = owner.laneSpace != null
                    ? owner.laneSpace.GetLaneCoordinates(owner.transform.position)
                    : Vector2.zero;
                float entryZ = owner.laneSpace != null ? owner.laneSpace.SummonEntryZ : playerLane.y + 2f;
                float targetZ = ResolveTargetLaneZ();
                int projectileCount = Mathf.Max(1, settings.ProjectileCount);
                LastFiredProjectileCount = projectileCount;

                Vector3 entryPosition = ResolveBattlefieldPoint(playerLane.x, entryZ, settings.EntryHeight);
                LastEntryPosition = entryPosition;
                SpawnEntryCue(entryPosition, settings);

                Vector3 firstTargetPosition = ResolveBattlefieldPoint(playerLane.x, targetZ, settings.TargetHeight);
                Vector3 actorFacing = ResolvePlanarDirection(firstTargetPosition - entryPosition);
                SummonFrontlineProxy actor = SpawnSummonActor(entryPosition, actorFacing, tier, settings);
                Vector3 projectileSpawnBase = actor != null
                    ? actor.ProjectileOrigin.position
                    : ResolveBattlefieldPoint(playerLane.x, entryZ, settings.EntryHeight + 0.7f);
                Vector3 right = ResolveRight(actorFacing);

                for (int i = 0; i < projectileCount; i++)
                {
                    float targetOffset = ResolveOffset(i, projectileCount, settings.LateralReach);
                    float targetX = playerLane.x + targetOffset;
                    Vector3 spawnPosition = projectileSpawnBase + right * (targetOffset * 0.22f);
                    Vector3 targetPosition = ResolveBattlefieldPoint(targetX, targetZ, settings.TargetHeight);
                    Vector3 direction = ResolvePlanarDirection(targetPosition - spawnPosition);

                    LaneActionProjectile projectile = GetProjectile();
                    projectile.transform.position = spawnPosition;
                    projectile.Configure(
                        owner.sourceHealth,
                        owner.sourceTeam,
                        settings.Damage,
                        direction,
                        settings.ProjectileSpeed,
                        settings.LifetimeSeconds,
                        settings.Radius);
                }
            }

            private SummonFrontlineProxy SpawnSummonActor(
                Vector3 position,
                Vector3 facingDirection,
                int tier,
                SummonTierSettings settings)
            {
                SummonFrontlineProxy actor = GetSummonActor();
                if (actor == null)
                {
                    LastSummonActorPosition = position;
                    return null;
                }

                actor.transform.SetParent(owner.summonActorRoot != null ? owner.summonActorRoot : owner.transform, worldPositionStays: true);
                actor.Activate(
                    position,
                    facingDirection,
                    tier,
                    settings.ActorLifetimeSeconds,
                    settings.ActorScale,
                    settings.ActorAdvanceDistance,
                    settings.ActorAdvanceSeconds);

                if (actor.PressureScreen != null)
                {
                    actor.PressureScreen.Intercepted -= OnPressureScreenIntercepted;
                    actor.PressureScreen.Intercepted += OnPressureScreenIntercepted;
                    LastPressureScreenMaxIntercepts = Mathf.Max(0, settings.ScreenIntercepts);
                    actor.PressureScreen.Activate(
                        owner.sourceTeam,
                        settings.ScreenIntercepts,
                        settings.ScreenRadius,
                        settings.ScreenLifetimeSeconds,
                        actor.ActiveTier);
                }

                LastSummonActorPosition = actor.transform.position;
                return actor;
            }

            private void OnPressureScreenIntercepted(SummonPressureScreen screen, BossBarrageProjectile projectile)
            {
                SummonFrontlineProxy actor = FindActorForPressureScreen(screen);
                if (actor == null || !actor.IsActive)
                {
                    return;
                }

                LastPressureScreenInterceptCount++;
                TotalPressureScreenInterceptCount++;
                LastPressureScreenInterceptTier = actor.ActiveTier;
                owner.NotifySummonPressureBlocked(LastPressureScreenInterceptTier);
                FireCounterProjectile(actor, owner.ResolveTierSettings(actor.ActiveTier));
            }

            private void FireCounterProjectile(SummonFrontlineProxy actor, SummonTierSettings settings)
            {
                Vector3 spawnPosition = actor.ProjectileOrigin.position;
                Vector3 targetPosition = ResolveCounterTargetPosition(settings.CounterTargetHeight);
                Vector3 direction = ResolvePlanarDirection(targetPosition - spawnPosition);
                float counterDamage = settings.CounterDamage > 0f ? settings.CounterDamage : settings.Damage * 0.35f;
                float counterSpeed = settings.CounterProjectileSpeed > 0f
                    ? settings.CounterProjectileSpeed
                    : Mathf.Max(12f, settings.ProjectileSpeed);
                float counterLifetime = settings.CounterLifetimeSeconds > 0f
                    ? settings.CounterLifetimeSeconds
                    : Mathf.Max(0.8f, settings.LifetimeSeconds * 0.65f);
                float counterRadius = settings.CounterRadius > 0f
                    ? settings.CounterRadius
                    : Mathf.Max(0.18f, settings.Radius * 0.7f);

                LaneActionProjectile projectile = GetProjectile();
                projectile.transform.position = spawnPosition;
                projectile.Configure(
                    owner.sourceHealth,
                    owner.sourceTeam,
                    counterDamage,
                    direction,
                    counterSpeed,
                    counterLifetime,
                    counterRadius);
            }

            private Vector3 ResolveCounterTargetPosition(float targetHeight)
            {
                if (owner.laneSpace != null)
                {
                    if (owner.frontlineTargetHealth != null && owner.frontlineTargetHealth.IsAlive)
                    {
                        Vector2 targetLane = owner.laneSpace.GetLaneCoordinates(owner.frontlineTargetHealth.transform.position);
                        return owner.laneSpace.GetBattlefieldWorldPoint(targetLane.x, targetLane.y, targetHeight);
                    }

                    return owner.laneSpace.GetBattlefieldWorldPoint(0f, owner.laneSpace.BossProxyZ, targetHeight);
                }

                return owner.transform.position + ResolvePlanarDirection(owner.transform.forward) * 10f + Vector3.up * targetHeight;
            }

            private SummonFrontlineProxy FindActorForPressureScreen(SummonPressureScreen screen)
            {
                if (screen == null)
                {
                    return null;
                }

                for (int i = 0; i < summonActors.Count; i++)
                {
                    SummonFrontlineProxy actor = summonActors[i];
                    if (actor != null && actor.PressureScreen == screen)
                    {
                        return actor;
                    }
                }

                return null;
            }

            private void UnsubscribePressureScreens()
            {
                for (int i = 0; i < summonActors.Count; i++)
                {
                    SummonFrontlineProxy actor = summonActors[i];
                    if (actor != null && actor.PressureScreen != null)
                    {
                        actor.PressureScreen.Intercepted -= OnPressureScreenIntercepted;
                    }
                }
            }

            private float ResolveTargetLaneZ()
            {
                if (owner.laneSpace == null)
                {
                    return 8f;
                }

                if (owner.frontlineTargetHealth != null && owner.frontlineTargetHealth.IsAlive)
                {
                    return owner.laneSpace.GetLaneCoordinates(owner.frontlineTargetHealth.transform.position).y;
                }

                if (owner.targetSelector != null
                    && owner.targetSelector.TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth)
                    && target != null
                    && targetHealth != null
                    && targetHealth.IsAlive)
                {
                    return owner.laneSpace.GetLaneCoordinates(target.position).y;
                }

                return owner.laneSpace.BossProxyZ;
            }

            private Vector3 ResolveBattlefieldPoint(float lateralX, float laneZ, float worldY)
            {
                if (owner.laneSpace != null)
                {
                    return owner.laneSpace.GetBattlefieldWorldPoint(lateralX, laneZ, worldY);
                }

                Vector3 right = Vector3.Cross(Vector3.up, ResolvePlanarDirection(owner.transform.forward));
                return owner.transform.position
                    + ResolvePlanarDirection(owner.transform.forward) * laneZ
                    + right * lateralX
                    + Vector3.up * worldY;
            }

            private void SpawnEntryCue(Vector3 position, SummonTierSettings settings)
            {
                GameObject cue = GetEntryCue();
                if (cue == null)
                {
                    return;
                }

                cue.transform.SetParent(owner.cueRoot != null ? owner.cueRoot : owner.transform, worldPositionStays: true);
                cue.transform.SetPositionAndRotation(position, Quaternion.identity);
                float scale = Mathf.Max(0.01f, settings.CueScale);
                cue.transform.localScale = new Vector3(scale, 0.04f, scale);
                cue.SetActive(true);

                if (settings.CueLifetimeSeconds > 0f)
                {
                    owner.StartCoroutine(ReleaseCueAfterSeconds(cue, settings.CueLifetimeSeconds));
                }
            }

            private LaneActionProjectile GetProjectile()
            {
                while (projectilePool.Count > 0)
                {
                    LaneActionProjectile pooled = projectilePool.Dequeue();
                    if (pooled != null)
                    {
                        pooled.gameObject.SetActive(true);
                        return pooled;
                    }
                }

                for (int i = 0; i < projectiles.Count; i++)
                {
                    LaneActionProjectile reusable = projectiles[i];
                    if (reusable != null && !reusable.IsActive)
                    {
                        reusable.gameObject.SetActive(true);
                        return reusable;
                    }
                }

                LaneActionProjectile prefab = ResolveProjectilePrefab();
                if (prefab == null)
                {
                    throw new InvalidOperationException($"{owner.name} is missing a LaneActionProjectile prefab.");
                }

                Transform parent = owner.projectileRoot != null ? owner.projectileRoot : owner.transform;
                LaneActionProjectile instance = UnityEngine.Object.Instantiate(prefab, parent);
                instance.name = prefab.name;
                projectiles.Add(instance);
                return instance;
            }

            private GameObject GetEntryCue()
            {
                if (owner.entryCuePrefab == null)
                {
                    return null;
                }

                while (entryCuePool.Count > 0)
                {
                    GameObject pooled = entryCuePool.Dequeue();
                    if (pooled != null)
                    {
                        return pooled;
                    }
                }

                for (int i = 0; i < entryCues.Count; i++)
                {
                    GameObject reusable = entryCues[i];
                    if (reusable != null && !reusable.activeInHierarchy)
                    {
                        return reusable;
                    }
                }

                GameObject instance = UnityEngine.Object.Instantiate(
                    owner.entryCuePrefab,
                    owner.cueRoot != null ? owner.cueRoot : owner.transform);
                instance.name = owner.entryCuePrefab.name;
                entryCues.Add(instance);
                return instance;
            }

            private SummonFrontlineProxy GetSummonActor()
            {
                SummonFrontlineProxy prefab = ResolveSummonActorPrefab();
                if (prefab == null)
                {
                    return null;
                }

                while (summonActorPool.Count > 0)
                {
                    SummonFrontlineProxy pooled = summonActorPool.Dequeue();
                    if (pooled != null)
                    {
                        pooled.gameObject.SetActive(true);
                        return pooled;
                    }
                }

                for (int i = 0; i < summonActors.Count; i++)
                {
                    SummonFrontlineProxy reusable = summonActors[i];
                    if (reusable != null && !reusable.IsActive)
                    {
                        reusable.gameObject.SetActive(true);
                        return reusable;
                    }
                }

                Transform parent = owner.summonActorRoot != null ? owner.summonActorRoot : owner.transform;
                SummonFrontlineProxy instance = UnityEngine.Object.Instantiate(prefab, parent);
                instance.name = prefab.name;
                summonActors.Add(instance);
                return instance;
            }

            private IEnumerator ReleaseCueAfterSeconds(GameObject cue, float seconds)
            {
                yield return new WaitForSeconds(seconds);
                if (cue != null)
                {
                    cue.SetActive(false);
                    cue.transform.SetParent(owner.cueRoot != null ? owner.cueRoot : owner.transform, worldPositionStays: false);
                    entryCuePool.Enqueue(cue);
                }
            }

            private LaneActionProjectile ResolveProjectilePrefab()
            {
                if (owner.projectilePrefab != null)
                {
                    return owner.projectilePrefab;
                }

                if (owner.projectilePrefabObject != null)
                {
                    owner.projectilePrefab = owner.projectilePrefabObject.GetComponent<LaneActionProjectile>();
                }

                return owner.projectilePrefab;
            }

            private SummonFrontlineProxy ResolveSummonActorPrefab()
            {
                if (owner.summonActorPrefab != null)
                {
                    return owner.summonActorPrefab;
                }

                if (owner.summonActorPrefabObject != null)
                {
                    owner.summonActorPrefab = owner.summonActorPrefabObject.GetComponent<SummonFrontlineProxy>();
                }

                return owner.summonActorPrefab;
            }

            private void PrewarmProjectiles()
            {
                LaneActionProjectile prefab = ResolveProjectilePrefab();
                if (prefab == null || owner.prewarmCount <= 0)
                {
                    return;
                }

                for (int i = projectiles.Count; i < owner.prewarmCount; i++)
                {
                    LaneActionProjectile projectile = UnityEngine.Object.Instantiate(
                        prefab,
                        owner.projectileRoot != null ? owner.projectileRoot : owner.transform);
                    projectile.name = prefab.name;
                    projectile.Deactivate();
                    projectiles.Add(projectile);
                    projectilePool.Enqueue(projectile);
                }
            }

            private void PrewarmEntryCues()
            {
                if (owner.entryCuePrefab == null)
                {
                    return;
                }

                for (int i = entryCues.Count; i < 2; i++)
                {
                    GameObject cue = UnityEngine.Object.Instantiate(
                        owner.entryCuePrefab,
                        owner.cueRoot != null ? owner.cueRoot : owner.transform);
                    cue.name = owner.entryCuePrefab.name;
                    cue.SetActive(false);
                    entryCues.Add(cue);
                    entryCuePool.Enqueue(cue);
                }
            }

            private void PrewarmSummonActors()
            {
                SummonFrontlineProxy prefab = ResolveSummonActorPrefab();
                if (prefab == null || owner.actorPrewarmCount <= 0)
                {
                    return;
                }

                for (int i = summonActors.Count; i < owner.actorPrewarmCount; i++)
                {
                    SummonFrontlineProxy actor = UnityEngine.Object.Instantiate(
                        prefab,
                        owner.summonActorRoot != null ? owner.summonActorRoot : owner.transform);
                    actor.name = prefab.name;
                    actor.Deactivate();
                    summonActors.Add(actor);
                    summonActorPool.Enqueue(actor);
                }
            }

            private int CountActiveProjectiles()
            {
                int count = 0;
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (projectiles[i] != null && projectiles[i].IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }

            private int CountActiveSummonActors()
            {
                int count = 0;
                for (int i = 0; i < summonActors.Count; i++)
                {
                    if (summonActors[i] != null && summonActors[i].IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }

            private int CountActivePressureScreens()
            {
                int count = 0;
                for (int i = 0; i < summonActors.Count; i++)
                {
                    SummonFrontlineProxy actor = summonActors[i];
                    if (actor != null
                        && actor.PressureScreen != null
                        && actor.PressureScreen.IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }

            private int CountActivePressureScreenRemainingIntercepts()
            {
                int count = 0;
                for (int i = 0; i < summonActors.Count; i++)
                {
                    SummonFrontlineProxy actor = summonActors[i];
                    if (actor != null
                        && actor.PressureScreen != null
                        && actor.PressureScreen.IsActive)
                    {
                        count += actor.PressureScreen.RemainingIntercepts;
                    }
                }

                return count;
            }

            private int CountActiveCues()
            {
                int count = 0;
                for (int i = 0; i < entryCues.Count; i++)
                {
                    if (entryCues[i] != null && entryCues[i].activeInHierarchy)
                    {
                        count++;
                    }
                }

                return count;
            }

            private static Vector3 ResolvePlanarDirection(Vector3 direction)
            {
                Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
                if (planarDirection.sqrMagnitude > 0.0001f)
                {
                    return planarDirection.normalized;
                }

                return Vector3.forward;
            }

            private static float ResolveOffset(int index, int count, float spread)
            {
                if (count <= 1 || spread <= 0f)
                {
                    return 0f;
                }

                float t = count > 1 ? index / (float)(count - 1) : 0.5f;
                return Mathf.Lerp(-spread, spread, t);
            }

            private static Vector3 ResolveRight(Vector3 direction)
            {
                Vector3 right = Vector3.Cross(Vector3.up, ResolvePlanarDirection(direction));
                if (right.sqrMagnitude > 0.0001f)
                {
                    return right.normalized;
                }

                return Vector3.right;
            }
        }
    }
}
