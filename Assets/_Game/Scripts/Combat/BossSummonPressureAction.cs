using System;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossSummonPressureAction : MonoBehaviour
    {
        [Serializable]
        public struct BossSummonTierSettings
        {
            [Range(0f, 1f)] public float EntryForwardBlend01;
            public float LateralOffset;
            [Min(0f)] public float EntryHeight;
            [Min(0f)] public float ActorLifetimeSeconds;
            [Min(0.01f)] public float ActorScale;
            public string ActorRoleId;
            [Min(0f)] public float ActorMaxHealth;
            [Min(0f)] public float ActorMoveSpeed;
            [Min(0f)] public float ActorAdvanceDistance;
            [Min(0.01f)] public float ActorAdvanceSeconds;
            [Min(0.05f)] public float ActorEngageRadius;
            [Min(0f)] public float ActorAttackDamagePerSecond;
            [Min(0.05f)] public float ActorAttackIntervalSeconds;
            [Min(0)] public int ScreenIntercepts;
            [Min(0.05f)] public float ScreenRadius;
            [Min(0.05f)] public float ScreenLifetimeSeconds;

            public void Normalize()
            {
                EntryForwardBlend01 = Mathf.Clamp01(EntryForwardBlend01);
                EntryHeight = Mathf.Max(0f, EntryHeight);
                ActorLifetimeSeconds = Mathf.Max(0f, ActorLifetimeSeconds);
                ActorScale = Mathf.Max(0.01f, ActorScale);
                ActorRoleId = string.IsNullOrWhiteSpace(ActorRoleId) ? "BossPressure" : ActorRoleId.Trim();
                ActorMaxHealth = Mathf.Max(0f, ActorMaxHealth);
                ActorMoveSpeed = Mathf.Max(0f, ActorMoveSpeed);
                ActorAdvanceDistance = Mathf.Max(0f, ActorAdvanceDistance);
                ActorAdvanceSeconds = Mathf.Max(0.01f, ActorAdvanceSeconds);
                ActorEngageRadius = Mathf.Max(0.05f, ActorEngageRadius);
                ActorAttackDamagePerSecond = Mathf.Max(0f, ActorAttackDamagePerSecond);
                ActorAttackIntervalSeconds = Mathf.Max(0.05f, ActorAttackIntervalSeconds);
                ScreenIntercepts = Mathf.Max(0, ScreenIntercepts);
                ScreenRadius = Mathf.Max(0.05f, ScreenRadius);
                ScreenLifetimeSeconds = Mathf.Max(0.05f, ScreenLifetimeSeconds);
            }
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
        [SerializeField, Min(1)] private int maxActiveSummonActors = 1;
        [Tooltip("Extra travel time per meter after the authored advance distance. Keep this high enough that summons march instead of snapping to the target.")]
        [SerializeField, Min(0f)] private float actorEntryCatchupSecondsPerMeter = 0.55f;
        [Tooltip("Minimum distance boss pressure summons push past the player forward boundary, even when the player stands on that boundary.")]
        [SerializeField, Min(0f)] private float minimumPlayerSideTargetDepth = 1.2f;
        [SerializeField] private BossSummonPressureProfile pressureProfile;
        [SerializeField] private BossSummonTierSettings[] tierSettings = CreateDefaultTierSettings();

        private readonly SummonFrontlineProxyPool summonActorPool = new SummonFrontlineProxyPool();

        private int lastReleasedTier;
        private int totalReleaseCount;
        private int lastPressureScreenMaxIntercepts;
        private int lastPressureScreenInterceptCount;
        private int lastPressureScreenInterceptTier;
        private int totalPressureScreenInterceptCount;
        private int totalSummonActorDefeatCount;
        private Vector3 lastSummonActorPosition;
        private SummonFrontlineProxy lastSummonActor;

        public event Action<BossSummonPressureAction, int> PressureSummonReleased;
        public event Action<BossSummonPressureAction, int> PressureSummonIntercepted;

        public int LastReleasedTier => lastReleasedTier;
        public int TotalReleaseCount => totalReleaseCount;
        public int LastPressureScreenMaxIntercepts => lastPressureScreenMaxIntercepts;
        public int LastPressureScreenInterceptCount => lastPressureScreenInterceptCount;
        public int LastPressureScreenInterceptTier => lastPressureScreenInterceptTier;
        public int TotalPressureScreenInterceptCount => totalPressureScreenInterceptCount;
        public int TotalSummonActorDefeatCount => totalSummonActorDefeatCount;
        public Vector3 LastSummonActorPosition => lastSummonActorPosition;
        public SummonFrontlineProxy LastSummonActor => lastSummonActor;
        public int ActiveSummonActorCount => summonActorPool.CountActive();
        public int ActivePressureScreenCount => summonActorPool.CountActivePressureScreens();
        public int ActivePressureScreenRemainingIntercepts => summonActorPool.CountActivePressureScreenRemainingIntercepts();
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
        public bool CanRelease => laneSpace != null && ResolveSummonActorPrefab() != null;
        public BossSummonPressureProfile PressureProfile => pressureProfile;
        public bool HasPressureProfile => pressureProfile != null;
        private int MaxActiveSummonActors => Mathf.Max(1, maxActiveSummonActors);

        private void OnEnable()
        {
            ApplyPressureProfile();
            PrewarmSummonActors();
        }

        private void OnDisable()
        {
            UnsubscribePressureScreens();
            summonActorPool.ForEach(actor => actor.Exited -= HandleSummonActorExited);
        }

        private void OnValidate()
        {
            ApplyPressureProfile();
            if (tierSettings == null || tierSettings.Length == 0)
            {
                tierSettings = CreateDefaultTierSettings();
            }

            actorEntryCatchupSecondsPerMeter = Mathf.Max(0f, actorEntryCatchupSecondsPerMeter);
            minimumPlayerSideTargetDepth = Mathf.Max(0f, minimumPlayerSideTargetDepth);
            maxActiveSummonActors = Mathf.Max(1, maxActiveSummonActors);
            for (int i = 0; i < tierSettings.Length; i++)
            {
                BossSummonTierSettings settings = tierSettings[i];
                settings.Normalize();
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

        public void ConfigurePressureProfile(BossSummonPressureProfile newPressureProfile)
        {
            pressureProfile = newPressureProfile;
            ApplyPressureProfile();
        }

        public bool TryGetTierReadout(int tier, out BossSummonPressureProfile.BossSummonTierReadout readout)
        {
            if (pressureProfile == null)
            {
                readout = default;
                return false;
            }

            return pressureProfile.TryGetTierReadout(tier, out readout);
        }

        public bool TryReleasePressureSummon(int tier)
        {
            if (!CanRelease)
            {
                return false;
            }

            int resolvedTier = Mathf.Clamp(tier, 1, 3);
            BossSummonTierSettings settings = ResolveTierSettings(resolvedTier);
            summonActorPool.TrimActiveCountBeforeSpawn(MaxActiveSummonActors);
            SummonFrontlineProxy actor = GetSummonActor();
            if (actor == null)
            {
                return false;
            }

            actor.Exited -= HandleSummonActorExited;
            actor.Exited += HandleSummonActorExited;
            Vector3 entryPosition = ResolveEntryPosition(settings);
            Vector3 targetPosition = ResolvePressureTargetPosition(entryPosition, settings);
            Vector3 facingDirection = ResolveFacingDirection(entryPosition, targetPosition);
            float actorAdvanceDistance = Vector3.Distance(
                Vector3.ProjectOnPlane(targetPosition - entryPosition, Vector3.up),
                Vector3.zero);
            float actorAdvanceSeconds = ResolveActorAdvanceSeconds(actorAdvanceDistance, settings);
            actor.transform.SetParent(summonActorRoot != null ? summonActorRoot : transform, worldPositionStays: true);
            ConfigureActorCombat(actor, settings);
            actor.Activate(
                entryPosition,
                facingDirection,
                resolvedTier,
                settings.ActorLifetimeSeconds,
                settings.ActorScale,
                targetPosition,
                actorAdvanceSeconds,
                settings.ActorMaxHealth,
                settings.ActorMoveSpeed);

            lastPressureScreenMaxIntercepts = 0;
            lastPressureScreenInterceptCount = 0;
            lastPressureScreenInterceptTier = 0;
            if (actor.PressureScreen != null)
            {
                actor.PressureScreen.Intercepted -= HandlePressureScreenIntercepted;
                actor.PressureScreen.ActionProjectileIntercepted -= HandlePressureScreenActionProjectileIntercepted;
                actor.PressureScreen.Intercepted += HandlePressureScreenIntercepted;
                actor.PressureScreen.ActionProjectileIntercepted += HandlePressureScreenActionProjectileIntercepted;
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
            lastSummonActor = actor;
            PressureSummonReleased?.Invoke(this, resolvedTier);
            return true;
        }

        private void HandleSummonActorExited(
            SummonFrontlineProxy actor,
            SummonFrontlineProxyExitReason reason)
        {
            if (reason == SummonFrontlineProxyExitReason.Defeated)
            {
                totalSummonActorDefeatCount++;
            }
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

        private Vector3 ResolvePressureTargetPosition(Vector3 entryPosition, BossSummonTierSettings settings)
        {
            if (laneSpace == null)
            {
                return entryPosition + Vector3.back * Mathf.Max(0f, settings.ActorAdvanceDistance);
            }

            float targetX = 0f;
            float targetZ = laneSpace.ForwardBoundaryZ - minimumPlayerSideTargetDepth;
            if (trackedPlayer != null)
            {
                Vector2 playerLane = laneSpace.GetLaneCoordinates(trackedPlayer.position);
                targetX = Mathf.Clamp(playerLane.x, -laneSpace.HalfWidth, laneSpace.HalfWidth);
                targetZ = Mathf.Min(playerLane.y, targetZ);
            }

            targetZ = Mathf.Clamp(targetZ, laneSpace.BackLimitZ, laneSpace.ForwardBoundaryZ);
            return laneSpace.GetBattlefieldWorldPoint(targetX, targetZ, settings.EntryHeight);
        }

        private float ResolveActorAdvanceSeconds(float resolvedAdvanceDistance, BossSummonTierSettings settings)
        {
            float extraDistance = Mathf.Max(0f, resolvedAdvanceDistance - settings.ActorAdvanceDistance);
            return settings.ActorAdvanceSeconds + extraDistance * actorEntryCatchupSecondsPerMeter;
        }

        private static void ConfigureActorCombat(SummonFrontlineProxy actor, BossSummonTierSettings settings)
        {
            SummonFrontlineClash clash = actor != null ? actor.GetComponent<SummonFrontlineClash>() : null;
            if (clash == null)
            {
                return;
            }

            float damagePerSecond = settings.ActorAttackDamagePerSecond > 0f
                ? settings.ActorAttackDamagePerSecond
                : 34f;
            clash.ConfigureTuning(
                damagePerSecond,
                settings.ActorAttackIntervalSeconds,
                0.16f,
                0.24f,
                settings.ActorEngageRadius);
        }

        private Vector3 ResolveFacingDirection(Vector3 entryPosition, Vector3 targetPosition)
        {
            Vector3 facingDirection = targetPosition - entryPosition;
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
            lastPressureScreenInterceptTier = actor.ActiveTier;
            totalPressureScreenInterceptCount++;
            PressureSummonIntercepted?.Invoke(this, actor.ActiveTier);
        }

        private void HandlePressureScreenActionProjectileIntercepted(SummonPressureScreen screen, LaneActionProjectile projectile)
        {
            SummonFrontlineProxy actor = FindActorForPressureScreen(screen);
            if (actor == null || !actor.IsActive)
            {
                return;
            }

            lastPressureScreenInterceptCount++;
            lastPressureScreenInterceptTier = actor.ActiveTier;
            totalPressureScreenInterceptCount++;
            PressureSummonIntercepted?.Invoke(this, actor.ActiveTier);
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
                    actor.PressureScreen.Intercepted -= HandlePressureScreenIntercepted;
                    actor.PressureScreen.ActionProjectileIntercepted -= HandlePressureScreenActionProjectileIntercepted;
                }
            });
        }

        private SummonFrontlineProxy GetSummonActor()
        {
            SummonFrontlineProxy prefab = ResolveSummonActorPrefab();
            if (prefab == null)
            {
                return null;
            }

            Transform parent = summonActorRoot != null ? summonActorRoot : transform;
            return summonActorPool.Get(prefab, parent);
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

        private void ApplyPressureProfile()
        {
            if (pressureProfile == null)
            {
                return;
            }

            tierSettings = pressureProfile.CopyTierSettings();
        }

        private void PrewarmSummonActors()
        {
            SummonFrontlineProxy prefab = ResolveSummonActorPrefab();
            if (prefab == null || actorPrewarmCount <= 0)
            {
                return;
            }

            Transform parent = summonActorRoot != null ? summonActorRoot : transform;
            summonActorPool.Prewarm(prefab, parent, actorPrewarmCount);
        }

        private BossSummonTierSettings ResolveTierSettings(int tier)
        {
            if (tierSettings == null || tierSettings.Length == 0)
            {
                tierSettings = CreateDefaultTierSettings();
            }

            BossSummonTierSettings settings = tierSettings[Mathf.Clamp(tier - 1, 0, tierSettings.Length - 1)];
            settings.Normalize();
            return settings;
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

        private static BossSummonTierSettings[] CreateDefaultTierSettings()
        {
            return new[]
            {
                new BossSummonTierSettings
                {
                    EntryForwardBlend01 = 0.28f,
                    LateralOffset = 0.9f,
                    EntryHeight = 0.2f,
                    ActorLifetimeSeconds = 0f,
                    ActorScale = 0.92f,
                    ActorRoleId = "EscortProbe",
                    ActorMaxHealth = 220f,
                    ActorMoveSpeed = 1.35f,
                    ActorAdvanceDistance = 2.4f,
                    ActorAdvanceSeconds = 1.4f,
                    ActorEngageRadius = 0.95f,
                    ActorAttackDamagePerSecond = 32f,
                    ActorAttackIntervalSeconds = 0.35f,
                    ScreenIntercepts = 2,
                    ScreenRadius = 1.2f,
                    ScreenLifetimeSeconds = 2.6f
                },
                new BossSummonTierSettings
                {
                    EntryForwardBlend01 = 0.38f,
                    LateralOffset = 1.4f,
                    EntryHeight = 0.2f,
                    ActorLifetimeSeconds = 0f,
                    ActorScale = 1.12f,
                    ActorRoleId = "PressureScreen",
                    ActorMaxHealth = 320f,
                    ActorMoveSpeed = 1.42f,
                    ActorAdvanceDistance = 3.8f,
                    ActorAdvanceSeconds = 1.85f,
                    ActorEngageRadius = 1.05f,
                    ActorAttackDamagePerSecond = 44f,
                    ActorAttackIntervalSeconds = 0.35f,
                    ScreenIntercepts = 4,
                    ScreenRadius = 1.55f,
                    ScreenLifetimeSeconds = 3.4f
                },
                new BossSummonTierSettings
                {
                    EntryForwardBlend01 = 0.5f,
                    LateralOffset = 2.0f,
                    EntryHeight = 0.2f,
                    ActorLifetimeSeconds = 0f,
                    ActorScale = 1.36f,
                    ActorRoleId = "ClampGuard",
                    ActorMaxHealth = 460f,
                    ActorMoveSpeed = 1.48f,
                    ActorAdvanceDistance = 5.2f,
                    ActorAdvanceSeconds = 2.35f,
                    ActorEngageRadius = 1.18f,
                    ActorAttackDamagePerSecond = 58f,
                    ActorAttackIntervalSeconds = 0.35f,
                    ScreenIntercepts = 7,
                    ScreenRadius = 1.95f,
                    ScreenLifetimeSeconds = 4.2f
                }
            };
        }
    }
}
