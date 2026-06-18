using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossSummonPressureAction : MonoBehaviour
    {
        [Serializable]
        private struct BossSummonTierSettings
        {
            [Range(0f, 1f)] public float EntryForwardBlend01;
            public float LateralOffset;
            [Min(0f)] public float EntryHeight;
            [Min(0.05f)] public float ActorLifetimeSeconds;
            [Min(0.01f)] public float ActorScale;
            [Min(0f)] public float ActorAdvanceDistance;
            [Min(0.01f)] public float ActorAdvanceSeconds;
            [Min(0)] public int ScreenIntercepts;
            [Min(0.05f)] public float ScreenRadius;
            [Min(0.05f)] public float ScreenLifetimeSeconds;
        }

        [Header("References")]
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedPlayer;
        [SerializeField] private SummonFrontlineProxy summonActorPrefab;
        [SerializeField] private GameObject summonActorPrefabObject;
        [SerializeField] private Transform summonActorRoot;

        [Header("Boss Summon")]
        [SerializeField] private DamageTeam ownerTeam = DamageTeam.Enemy;
        [SerializeField, Min(0)] private int actorPrewarmCount = 2;
        [SerializeField] private BossSummonTierSettings[] tierSettings = CreateDefaultTierSettings();

        private readonly List<SummonFrontlineProxy> summonActors = new List<SummonFrontlineProxy>(4);
        private readonly Queue<SummonFrontlineProxy> summonActorPool = new Queue<SummonFrontlineProxy>(4);

        private int lastReleasedTier;
        private int totalReleaseCount;
        private int lastPressureScreenMaxIntercepts;
        private int lastPressureScreenInterceptCount;
        private Vector3 lastSummonActorPosition;

        public event Action<BossSummonPressureAction, int> PressureSummonReleased;
        public event Action<BossSummonPressureAction, int> PressureSummonIntercepted;

        public int LastReleasedTier => lastReleasedTier;
        public int TotalReleaseCount => totalReleaseCount;
        public int LastPressureScreenMaxIntercepts => lastPressureScreenMaxIntercepts;
        public int LastPressureScreenInterceptCount => lastPressureScreenInterceptCount;
        public Vector3 LastSummonActorPosition => lastSummonActorPosition;
        public int ActiveSummonActorCount => CountActiveSummonActors();
        public int ActivePressureScreenCount => CountActivePressureScreens();
        public int ActivePressureScreenRemainingIntercepts => CountActivePressureScreenRemainingIntercepts();
        public bool CanRelease => laneSpace != null && ResolveSummonActorPrefab() != null;

        private void OnEnable()
        {
            PrewarmSummonActors();
        }

        private void OnDisable()
        {
            UnsubscribePressureScreens();
        }

        private void OnValidate()
        {
            if (tierSettings == null || tierSettings.Length == 0)
            {
                tierSettings = CreateDefaultTierSettings();
            }

            for (int i = 0; i < tierSettings.Length; i++)
            {
                BossSummonTierSettings settings = tierSettings[i];
                settings.EntryForwardBlend01 = Mathf.Clamp01(settings.EntryForwardBlend01);
                settings.EntryHeight = Mathf.Max(0f, settings.EntryHeight);
                settings.ActorLifetimeSeconds = Mathf.Max(0.05f, settings.ActorLifetimeSeconds);
                settings.ActorScale = Mathf.Max(0.01f, settings.ActorScale);
                settings.ActorAdvanceDistance = Mathf.Max(0f, settings.ActorAdvanceDistance);
                settings.ActorAdvanceSeconds = Mathf.Max(0.01f, settings.ActorAdvanceSeconds);
                settings.ScreenIntercepts = Mathf.Max(0, settings.ScreenIntercepts);
                settings.ScreenRadius = Mathf.Max(0.05f, settings.ScreenRadius);
                settings.ScreenLifetimeSeconds = Mathf.Max(0.05f, settings.ScreenLifetimeSeconds);
                tierSettings[i] = settings;
            }
        }

        public void ConfigureReferences(
            SummonLaneSpace newLaneSpace,
            Transform newTrackedPlayer,
            SummonFrontlineProxy newSummonActorPrefab,
            Transform newSummonActorRoot)
        {
            laneSpace = newLaneSpace;
            trackedPlayer = newTrackedPlayer;
            summonActorPrefab = newSummonActorPrefab;
            summonActorPrefabObject = newSummonActorPrefab != null ? newSummonActorPrefab.gameObject : null;
            summonActorRoot = newSummonActorRoot;
        }

        public bool TryReleasePressureSummon(int tier)
        {
            if (!CanRelease)
            {
                return false;
            }

            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            BossSummonTierSettings settings = ResolveTierSettings(resolvedTier);
            SummonFrontlineProxy actor = GetSummonActor();
            if (actor == null)
            {
                return false;
            }

            Vector3 entryPosition = ResolveEntryPosition(settings);
            Vector3 facingDirection = ResolveFacingDirection(entryPosition);
            actor.transform.SetParent(summonActorRoot != null ? summonActorRoot : transform, worldPositionStays: true);
            actor.Activate(
                entryPosition,
                facingDirection,
                resolvedTier,
                settings.ActorLifetimeSeconds,
                settings.ActorScale,
                settings.ActorAdvanceDistance,
                settings.ActorAdvanceSeconds);

            lastPressureScreenMaxIntercepts = 0;
            lastPressureScreenInterceptCount = 0;
            if (actor.PressureScreen != null)
            {
                actor.PressureScreen.Intercepted -= HandlePressureScreenIntercepted;
                actor.PressureScreen.Intercepted += HandlePressureScreenIntercepted;
                lastPressureScreenMaxIntercepts = Mathf.Max(0, settings.ScreenIntercepts);
                actor.PressureScreen.Activate(
                    ownerTeam,
                    settings.ScreenIntercepts,
                    settings.ScreenRadius,
                    settings.ScreenLifetimeSeconds,
                    actor.ActiveTier);
            }

            lastReleasedTier = resolvedTier;
            totalReleaseCount++;
            lastSummonActorPosition = actor.transform.position;
            PressureSummonReleased?.Invoke(this, resolvedTier);
            return true;
        }

        private Vector3 ResolveEntryPosition(BossSummonTierSettings settings)
        {
            if (laneSpace == null)
            {
                return transform.position + Vector3.forward * 2f + Vector3.up * settings.EntryHeight;
            }

            float targetX = 0f;
            if (trackedPlayer != null)
            {
                targetX = laneSpace.GetLaneCoordinates(trackedPlayer.position).x;
            }

            float side = totalReleaseCount % 2 == 0 ? 1f : -1f;
            targetX = Mathf.Clamp(targetX + settings.LateralOffset * side, -laneSpace.HalfWidth, laneSpace.HalfWidth);
            float entryZ = Mathf.Lerp(laneSpace.SummonEntryZ, laneSpace.BossProxyZ, settings.EntryForwardBlend01);
            return laneSpace.GetBattlefieldWorldPoint(targetX, entryZ, settings.EntryHeight);
        }

        private Vector3 ResolveFacingDirection(Vector3 entryPosition)
        {
            Vector3 facingDirection = trackedPlayer != null
                ? trackedPlayer.position - entryPosition
                : Vector3.back;
            Vector3 planarDirection = Vector3.ProjectOnPlane(facingDirection, Vector3.up);
            return planarDirection.sqrMagnitude > 0.0001f ? planarDirection.normalized : Vector3.back;
        }

        private void HandlePressureScreenIntercepted(SummonPressureScreen screen, BossBarrageProjectile projectile)
        {
            SummonFrontlineProxy actor = FindActorForPressureScreen(screen);
            if (actor == null || !actor.IsActive)
            {
                return;
            }

            lastPressureScreenInterceptCount++;
            PressureSummonIntercepted?.Invoke(this, actor.ActiveTier);
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
                    actor.PressureScreen.Intercepted -= HandlePressureScreenIntercepted;
                }
            }
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

            Transform parent = summonActorRoot != null ? summonActorRoot : transform;
            SummonFrontlineProxy instance = Instantiate(prefab, parent);
            instance.name = prefab.name;
            summonActors.Add(instance);
            return instance;
        }

        private SummonFrontlineProxy ResolveSummonActorPrefab()
        {
            if (summonActorPrefab != null)
            {
                return summonActorPrefab;
            }

            if (summonActorPrefabObject != null)
            {
                summonActorPrefab = summonActorPrefabObject.GetComponent<SummonFrontlineProxy>();
            }

            return summonActorPrefab;
        }

        private void PrewarmSummonActors()
        {
            SummonFrontlineProxy prefab = ResolveSummonActorPrefab();
            if (prefab == null || actorPrewarmCount <= 0)
            {
                return;
            }

            for (int i = summonActors.Count; i < actorPrewarmCount; i++)
            {
                SummonFrontlineProxy actor = Instantiate(
                    prefab,
                    summonActorRoot != null ? summonActorRoot : transform);
                actor.name = prefab.name;
                actor.Deactivate();
                summonActors.Add(actor);
                summonActorPool.Enqueue(actor);
            }
        }

        private BossSummonTierSettings ResolveTierSettings(int tier)
        {
            if (tierSettings == null || tierSettings.Length == 0)
            {
                tierSettings = CreateDefaultTierSettings();
            }

            return tierSettings[Mathf.Clamp(tier - 1, 0, tierSettings.Length - 1)];
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

        private static BossSummonTierSettings[] CreateDefaultTierSettings()
        {
            return new[]
            {
                new BossSummonTierSettings
                {
                    EntryForwardBlend01 = 0.24f,
                    LateralOffset = 0.9f,
                    EntryHeight = 0.2f,
                    ActorLifetimeSeconds = 1.35f,
                    ActorScale = 0.92f,
                    ActorAdvanceDistance = 1.15f,
                    ActorAdvanceSeconds = 0.28f,
                    ScreenIntercepts = 2,
                    ScreenRadius = 1.2f,
                    ScreenLifetimeSeconds = 1.2f
                },
                new BossSummonTierSettings
                {
                    EntryForwardBlend01 = 0.34f,
                    LateralOffset = 1.4f,
                    EntryHeight = 0.2f,
                    ActorLifetimeSeconds = 1.65f,
                    ActorScale = 1.12f,
                    ActorAdvanceDistance = 1.9f,
                    ActorAdvanceSeconds = 0.34f,
                    ScreenIntercepts = 4,
                    ScreenRadius = 1.55f,
                    ScreenLifetimeSeconds = 1.45f
                },
                new BossSummonTierSettings
                {
                    EntryForwardBlend01 = 0.45f,
                    LateralOffset = 2.0f,
                    EntryHeight = 0.2f,
                    ActorLifetimeSeconds = 2.0f,
                    ActorScale = 1.36f,
                    ActorAdvanceDistance = 2.7f,
                    ActorAdvanceSeconds = 0.42f,
                    ScreenIntercepts = 7,
                    ScreenRadius = 1.95f,
                    ScreenLifetimeSeconds = 1.8f
                }
            };
        }
    }
}
