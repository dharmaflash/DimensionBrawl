using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Player
{
    internal sealed class SupportSummonSlotExecutor
    {
        private readonly PlayerSupportSummonSlotAction owner;
        private readonly List<LaneActionProjectile> projectiles = new List<LaneActionProjectile>();
        private readonly Queue<LaneActionProjectile> projectilePool = new Queue<LaneActionProjectile>();
        private readonly List<GameObject> entryCues = new List<GameObject>();
        private readonly Queue<GameObject> entryCuePool = new Queue<GameObject>();
        private readonly SummonFrontlineProxyPool summonActorPool = new SummonFrontlineProxyPool();
        private SummonFrontlineProxy lastSummonActor;
        private string lastSummonActorRoleId;
        private int lastVolleyWaveCount;
        private int totalVolleyWaveCount;

        public SupportSummonSlotExecutor(PlayerSupportSummonSlotAction owner)
        {
            this.owner = owner;
        }

        public int ActiveProjectileCount => CountActiveProjectiles();
        public int ActiveSummonActorCount => summonActorPool.CountActive();
        public string LastSummonActorRoleId => lastSummonActorRoleId;
        public int LastVolleyWaveCount => lastVolleyWaveCount;
        public int TotalVolleyWaveCount => totalVolleyWaveCount;
        public bool LastSummonActorHasHealth => lastSummonActor != null && lastSummonActor.HasHealth;
        public float LastSummonActorHealthRatio => lastSummonActor != null ? lastSummonActor.HealthRatio : 0f;
        public float LastSummonActorRemainingLifetimeSeconds => lastSummonActor != null
            ? lastSummonActor.RemainingLifetimeSeconds
            : 0f;
        public float ActiveSummonActorAdvanceProgress01
        {
            get
            {
                SummonFrontlineProxy actor = ResolveActiveSummonActor();
                return actor != null ? actor.AdvanceProgress01 : 0f;
            }
        }
        public bool LastSummonActorIsClashing
        {
            get
            {
                SummonFrontlineClash clash = ResolveLastSummonActorClash();
                return clash != null && clash.IsClashing;
            }
        }
        public int LastSummonActorClashCount
        {
            get
            {
                SummonFrontlineClash clash = ResolveLastSummonActorClash();
                return clash != null ? clash.TotalClashCount : 0;
            }
        }

        public SummonFrontlineProxyExitReason LastSummonActorExitReason => lastSummonActor != null
            ? lastSummonActor.LastExitReason
            : SummonFrontlineProxyExitReason.None;
        public bool HasRequiredPresentation =>
            owner.ResolveProjectilePrefab() != null
            && owner.ResolveSummonActorPrefab() != null;

        public void Prewarm()
        {
            PrewarmProjectiles(4);
            PrewarmEntryCues(2);
            PrewarmSummonActors(1);
        }

        public void Detach()
        {
            UnsubscribePressureScreens();
        }

        public void FireTier(int tier, PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            Vector2 playerLane = owner.LaneSpace != null
                ? owner.LaneSpace.GetLaneCoordinates(owner.transform.position)
                : Vector2.zero;
            float entryX = playerLane.x + owner.LaneOffset.x;
            float entryZ = owner.ResolveEntryLaneZ(playerLane.y);
            Vector2 targetLane = ResolveTargetLaneCoordinates(new Vector2(entryX, ResolveFallbackTargetLaneZ()));
            Vector3 entryPosition = ResolveBattlefieldPoint(entryX, entryZ, settings.EntryHeight);
            float actorTargetX = Mathf.Lerp(entryX, targetLane.x, 0.45f);
            Vector3 actorTargetPosition = ResolveBattlefieldPoint(actorTargetX, targetLane.y, settings.EntryHeight);
            Vector3 projectileTargetPosition = ResolveBattlefieldPoint(targetLane.x, targetLane.y, settings.TargetHeight);
            Vector3 facingDirection = ResolvePlanarDirection(actorTargetPosition - entryPosition);
            float actorAdvanceDistance = Vector3.Distance(
                Vector3.ProjectOnPlane(actorTargetPosition - entryPosition, Vector3.up),
                Vector3.zero);
            float actorAdvanceSeconds = owner.ResolveActorAdvanceSeconds(actorAdvanceDistance, settings);

            SpawnEntryCue(entryPosition, settings);
            SummonFrontlineProxy actor = SpawnSummonActor(
                entryPosition,
                facingDirection,
                actorTargetPosition,
                tier,
                settings,
                actorAdvanceSeconds);
            if (actor != null)
            {
                lastSummonActorRoleId = settings.ActorRoleId;
                lastVolleyWaveCount = 0;
                owner.RunRoutine(RunPersistentVolley(actor, projectileTargetPosition, settings));
            }
        }

        private void FireProjectiles(
            Vector3 spawnBase,
            float entryX,
            float targetZ,
            Vector3 facingDirection,
            PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            int projectileCount = Mathf.Max(1, settings.ProjectileCount);
            Vector3 right = ResolveRight(facingDirection);
            for (int i = 0; i < projectileCount; i++)
            {
                float offset = ResolveOffset(i, projectileCount, settings.LateralReach);
                Vector3 spawnPosition = spawnBase + right * (offset * 0.2f);
                Vector3 targetPosition = ResolveBattlefieldPoint(entryX + offset, targetZ, settings.TargetHeight);
                SpawnProjectile(spawnPosition, targetPosition, settings);
            }
        }

        private Vector2 ResolveTargetLaneCoordinates(Vector2 fallback)
        {
            if (owner.LaneSpace == null)
            {
                return fallback;
            }

            if (owner.FrontlineTargetHealth != null && owner.FrontlineTargetHealth.IsAlive)
            {
                return owner.LaneSpace.GetLaneCoordinates(owner.FrontlineTargetHealth.transform.position);
            }

            if (owner.TargetSelector != null
                && owner.TargetSelector.TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth)
                && target != null
                && targetHealth != null
                && targetHealth.IsAlive)
            {
                return owner.LaneSpace.GetLaneCoordinates(target.position);
            }

            return fallback;
        }

        private float ResolveFallbackTargetLaneZ()
        {
            return owner.LaneSpace != null ? owner.LaneSpace.BossProxyZ : 8f;
        }

        private Vector3 ResolveBattlefieldPoint(float lateralX, float laneZ, float worldY)
        {
            if (owner.LaneSpace != null)
            {
                return owner.LaneSpace.GetBattlefieldWorldPoint(lateralX, laneZ, worldY);
            }

            Vector3 forward = ResolvePlanarDirection(owner.transform.forward);
            Vector3 right = ResolveRight(forward);
            return owner.transform.position + forward * laneZ + right * lateralX + Vector3.up * worldY;
        }

        private void SpawnEntryCue(Vector3 position, PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            if (owner.EntryCuePrefab == null)
            {
                return;
            }

            GameObject cue = GetEntryCue();
            if (cue == null)
            {
                return;
            }

            cue.transform.SetParent(owner.CueRoot != null ? owner.CueRoot : owner.transform, worldPositionStays: true);
            cue.transform.SetPositionAndRotation(position, Quaternion.identity);
            float scale = Mathf.Max(0.01f, settings.CueScale);
            cue.transform.localScale = new Vector3(scale, 0.04f, scale);
            cue.SetActive(true);
            owner.RunRoutine(ReleaseCueAfterSeconds(cue, Mathf.Max(0.05f, settings.CueLifetimeSeconds)));
        }

        private SummonFrontlineProxy SpawnSummonActor(
            Vector3 position,
            Vector3 facingDirection,
            Vector3 targetPosition,
            int tier,
            PlayerSummonSlot1Action.SummonTierSettings settings,
            float actorAdvanceSeconds)
        {
            SummonFrontlineProxy prefab = owner.ResolveSummonActorPrefab();
            if (prefab == null)
            {
                return null;
            }

            summonActorPool.TrimActiveCountBeforeSpawn(owner.MaxActiveSummonActors);
            SummonFrontlineProxy actor = GetSummonActor(prefab);
            actor.transform.SetParent(owner.SummonActorRoot != null ? owner.SummonActorRoot : owner.transform, worldPositionStays: true);
            ConfigureActorVfx(actor);
            ConfigureActorCombat(actor, settings);
            actor.Activate(
                position,
                facingDirection,
                tier,
                settings.ActorLifetimeSeconds,
                settings.ActorScale,
                targetPosition,
                actorAdvanceSeconds,
                settings.ActorMaxHealth,
                settings.ActorMoveSpeed);
            lastSummonActor = actor;

            if (actor.PressureScreen != null && settings.ScreenIntercepts > 0)
            {
                actor.PressureScreen.Intercepted -= OnPressureScreenIntercepted;
                actor.PressureScreen.ActionProjectileIntercepted -= OnPressureScreenActionProjectileIntercepted;
                actor.PressureScreen.Intercepted += OnPressureScreenIntercepted;
                actor.PressureScreen.ActionProjectileIntercepted += OnPressureScreenActionProjectileIntercepted;
                actor.PressureScreen.Activate(
                    owner.SourceTeam,
                    settings.ScreenIntercepts,
                    settings.ScreenRadius,
                    settings.ScreenLifetimeSeconds,
                    actor.ActiveTier);
            }

            return actor;
        }

        private void ConfigureActorVfx(SummonFrontlineProxy actor)
        {
            if (actor == null)
            {
                return;
            }

            DimensionBrawl.Presentation.SummonFrontlineProxyPresenter presenter =
                actor.GetComponent<DimensionBrawl.Presentation.SummonFrontlineProxyPresenter>();
            if (presenter == null)
            {
                return;
            }

            Transform directionTarget = owner.FrontlineTargetHealth != null
                ? owner.FrontlineTargetHealth.transform
                : null;
            presenter.ConfigureVfxCuePlayer(owner.CombatVfxCuePlayer, actor.transform, directionTarget);
        }

        private static void ConfigureActorCombat(
            SummonFrontlineProxy actor,
            PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            SummonFrontlineClash clash = actor != null ? actor.GetComponent<SummonFrontlineClash>() : null;
            if (clash == null)
            {
                return;
            }

            float damagePerSecond = settings.ActorAttackDamagePerSecond > 0f
                ? settings.ActorAttackDamagePerSecond
                : settings.Damage * 0.55f;
            clash.ConfigureTuning(
                damagePerSecond,
                settings.ActorAttackIntervalSeconds,
                0.16f,
                0.24f,
                settings.ActorEngageRadius);
        }

        private void OnPressureScreenIntercepted(SummonPressureScreen screen, BossBarrageProjectile projectile)
        {
            NotifyPressureScreenBlocked(screen);
        }

        private void OnPressureScreenActionProjectileIntercepted(SummonPressureScreen screen, LaneActionProjectile projectile)
        {
            NotifyPressureScreenBlocked(screen);
        }

        private void NotifyPressureScreenBlocked(SummonPressureScreen screen)
        {
            SummonFrontlineProxy actor = FindActorForPressureScreen(screen);
            if (actor == null || !actor.IsActive)
            {
                return;
            }

            owner.NotifySummonPressureBlocked(actor.ActiveTier);
        }

        private SummonFrontlineProxy FindActorForPressureScreen(SummonPressureScreen screen)
        {
            return summonActorPool.FindForPressureScreen(screen);
        }

        private void UnsubscribePressureScreens()
        {
            summonActorPool.ForEach(actor =>
            {
                if (actor.PressureScreen != null)
                {
                    actor.PressureScreen.Intercepted -= OnPressureScreenIntercepted;
                    actor.PressureScreen.ActionProjectileIntercepted -= OnPressureScreenActionProjectileIntercepted;
                }
            });
        }

        private void SpawnProjectile(
            Vector3 spawnPosition,
            Vector3 targetPosition,
            PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            LaneActionProjectile prefab = owner.ResolveProjectilePrefab();
            if (prefab == null)
            {
                return;
            }

            LaneActionProjectile projectile = GetProjectile(prefab);
            projectile.transform.SetParent(owner.ProjectileRoot != null ? owner.ProjectileRoot : owner.transform, worldPositionStays: true);
            projectile.transform.position = spawnPosition;
            projectile.Configure(
                owner.SourceHealth,
                owner.SourceTeam,
                settings.Damage,
                ResolvePlanarDirection(targetPosition - spawnPosition),
                settings.ProjectileSpeed,
                settings.LifetimeSeconds,
                settings.Radius);
        }

        private IEnumerator RunPersistentVolley(
            SummonFrontlineProxy actor,
            Vector3 initialTargetPosition,
            PlayerSummonSlot1Action.SummonTierSettings settings)
        {
            if (owner.FirstVolleyDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(owner.FirstVolleyDelaySeconds);
            }

            int firedCount = 0;
            while (actor != null && actor.IsActive && firedCount < owner.MaxVolleyCount)
            {
                Vector2 targetLane = ResolveTargetLaneCoordinates(
                    owner.LaneSpace != null
                        ? owner.LaneSpace.GetLaneCoordinates(initialTargetPosition)
                        : new Vector2(0f, 8f));
                Vector3 spawnBase = actor.ProjectileOrigin.position;
                Vector3 targetPosition = ResolveBattlefieldPoint(targetLane.x, targetLane.y, settings.TargetHeight);
                Vector3 facingDirection = ResolvePlanarDirection(targetPosition - spawnBase);
                FireProjectiles(spawnBase, targetLane.x, targetLane.y, facingDirection, settings);
                firedCount++;
                lastVolleyWaveCount = firedCount;
                totalVolleyWaveCount++;

                if (firedCount >= owner.MaxVolleyCount)
                {
                    break;
                }

                yield return new WaitForSeconds(owner.VolleyIntervalSeconds);
            }
        }

        private IEnumerator ReleaseCueAfterSeconds(GameObject instance, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (instance != null)
            {
                instance.SetActive(false);
                instance.transform.SetParent(owner.CueRoot != null ? owner.CueRoot : owner.transform, worldPositionStays: false);
                entryCuePool.Enqueue(instance);
            }
        }

        private LaneActionProjectile GetProjectile(LaneActionProjectile prefab)
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

            LaneActionProjectile instance = Object.Instantiate(
                prefab,
                owner.ProjectileRoot != null ? owner.ProjectileRoot : owner.transform);
            instance.name = prefab.name;
            projectiles.Add(instance);
            return instance;
        }

        private GameObject GetEntryCue()
        {
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

            if (owner.EntryCuePrefab == null)
            {
                return null;
            }

            GameObject instance = Object.Instantiate(
                owner.EntryCuePrefab,
                owner.CueRoot != null ? owner.CueRoot : owner.transform);
            instance.name = owner.EntryCuePrefab.name;
            entryCues.Add(instance);
            return instance;
        }

        private SummonFrontlineProxy GetSummonActor(SummonFrontlineProxy prefab)
        {
            Transform parent = owner.SummonActorRoot != null ? owner.SummonActorRoot : owner.transform;
            return summonActorPool.Get(prefab, parent);
        }

        private void PrewarmProjectiles(int count)
        {
            LaneActionProjectile prefab = owner.ResolveProjectilePrefab();
            if (prefab == null)
            {
                return;
            }

            while (projectiles.Count < count)
            {
                LaneActionProjectile projectile = Object.Instantiate(
                    prefab,
                    owner.ProjectileRoot != null ? owner.ProjectileRoot : owner.transform);
                projectile.name = prefab.name;
                projectile.Deactivate();
                projectiles.Add(projectile);
                projectilePool.Enqueue(projectile);
            }
        }

        private void PrewarmEntryCues(int count)
        {
            if (owner.EntryCuePrefab == null)
            {
                return;
            }

            while (entryCues.Count < count)
            {
                GameObject cue = Object.Instantiate(
                    owner.EntryCuePrefab,
                    owner.CueRoot != null ? owner.CueRoot : owner.transform);
                cue.name = owner.EntryCuePrefab.name;
                cue.SetActive(false);
                entryCues.Add(cue);
                entryCuePool.Enqueue(cue);
            }
        }

        private void PrewarmSummonActors(int count)
        {
            SummonFrontlineProxy prefab = owner.ResolveSummonActorPrefab();
            if (prefab == null)
            {
                return;
            }

            Transform parent = owner.SummonActorRoot != null ? owner.SummonActorRoot : owner.transform;
            summonActorPool.Prewarm(prefab, parent, count);
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

        private SummonFrontlineProxy ResolveActiveSummonActor()
        {
            return summonActorPool.ResolveActive(lastSummonActor);
        }

        private SummonFrontlineClash ResolveLastSummonActorClash()
        {
            SummonFrontlineProxy actor = ResolveActiveSummonActor() ?? lastSummonActor;
            return actor != null ? actor.GetComponent<SummonFrontlineClash>() : null;
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

        private static Vector3 ResolveRight(Vector3 direction)
        {
            Vector3 right = Vector3.Cross(Vector3.up, ResolvePlanarDirection(direction));
            if (right.sqrMagnitude > 0.0001f)
            {
                return right.normalized;
            }

            return Vector3.right;
        }

        private static float ResolveOffset(int index, int count, float spread)
        {
            if (count <= 1 || spread <= 0f)
            {
                return 0f;
            }

            float t = index / (float)(count - 1);
            return Mathf.Lerp(-spread, spread, t);
        }
    }
}
