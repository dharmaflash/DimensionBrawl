using System;
using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerSummonSlot1Action : MonoBehaviour
    {
        [Serializable]
        private struct SummonTierSettings
        {
            [Min(0f)] public float Damage;
            [Min(0f)] public float ProjectileSpeed;
            [Min(0.01f)] public float LifetimeSeconds;
            [Min(0.01f)] public float Radius;
            [Min(1)] public int ProjectileCount;
            [Min(0f)] public float LateralReach;
            [Min(0f)] public float EntryHeight;
            [Min(0f)] public float TargetHeight;
            [Min(0f)] public float CueScale;
            [Min(0f)] public float CueLifetimeSeconds;
            [Min(0.05f)] public float ActorLifetimeSeconds;
            [Min(0.01f)] public float ActorScale;
            [Min(0f)] public float ActorAdvanceDistance;
            [Min(0.01f)] public float ActorAdvanceSeconds;
            [Min(0)] public int ScreenIntercepts;
            [Min(0.05f)] public float ScreenRadius;
            [Min(0.05f)] public float ScreenLifetimeSeconds;
            [Min(0f)] public float CounterDamage;
            [Min(0f)] public float CounterProjectileSpeed;
            [Min(0.01f)] public float CounterLifetimeSeconds;
            [Min(0.01f)] public float CounterRadius;
            [Min(0f)] public float CounterTargetHeight;
        }

        [Header("Input")]
        [SerializeField] private InputActionReference summonAction;
        [SerializeField] private bool useKeyboardWhenActionMissing = true;
        [SerializeField] private Key keyboardTestKey = Key.Digit1;

        [Header("References")]
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [Tooltip("Preferred far/frontline target for summon exchanges. Local target selection is reserved for close defense and is only a fallback here.")]
        [SerializeField] private CombatHealth frontlineTargetHealth;
        [Tooltip("Summon actions use battlefield coordinates, not player clamping, because summons may cross lane rails and the player forward boundary.")]
        [SerializeField] private SummonLaneSpace laneSpace;

        [Header("Summon Action")]
        [SerializeField] private LaneActionProjectile projectilePrefab;
        [SerializeField] private GameObject projectilePrefabObject;
        [SerializeField] private GameObject entryCuePrefab;
        [SerializeField] private SummonFrontlineProxy summonActorPrefab;
        [SerializeField] private GameObject summonActorPrefabObject;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private Transform cueRoot;
        [SerializeField] private Transform summonActorRoot;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.AllySummon;
        [SerializeField, Min(0)] private int prewarmCount = 6;
        [SerializeField, Min(0)] private int actorPrewarmCount = 2;

        [Header("Tier Tuning")]
        [SerializeField] private SummonTierSettings[] tierSettings = CreateDefaultTierSettings();

        private readonly List<LaneActionProjectile> projectiles = new List<LaneActionProjectile>();
        private readonly Queue<LaneActionProjectile> projectilePool = new Queue<LaneActionProjectile>();
        private readonly List<GameObject> entryCues = new List<GameObject>();
        private readonly Queue<GameObject> entryCuePool = new Queue<GameObject>();
        private readonly List<SummonFrontlineProxy> summonActors = new List<SummonFrontlineProxy>();
        private readonly Queue<SummonFrontlineProxy> summonActorPool = new Queue<SummonFrontlineProxy>();
        private bool actionEnabledHere;
        private bool queued;
        private int lastSpentTier;
        private Vector3 lastEntryPosition;
        private Vector3 lastSummonActorPosition;

        public int LastSpentTier => lastSpentTier;
        public Vector3 LastEntryPosition => lastEntryPosition;
        public Vector3 LastSummonActorPosition => lastSummonActorPosition;
        public int ActiveProjectileCount => CountActiveProjectiles();
        public int ActiveCueCount => CountActiveCues();
        public int ActiveSummonActorCount => CountActiveSummonActors();
        public int ActivePressureScreenCount => CountActivePressureScreens();
        public int ActivePressureScreenRemainingIntercepts => CountActivePressureScreenRemainingIntercepts();

        private void Awake()
        {
            if (energyLadder == null)
            {
                energyLadder = GetComponent<SummonEnergyLadder>();
            }

            if (sourceHealth == null)
            {
                sourceHealth = GetComponent<CombatHealth>();
            }

            if (targetSelector == null)
            {
                targetSelector = GetComponent<PlayerCombatTargetSelector>();
            }
        }

        private void OnEnable()
        {
            actionEnabledHere = EnableActionIfNeeded(summonAction);
            PrewarmProjectiles();
            PrewarmEntryCues();
            PrewarmSummonActors();
        }

        private void OnDisable()
        {
            UnsubscribePressureScreens();
            DisableActionIfOwned(summonAction, actionEnabledHere);
            actionEnabledHere = false;
        }

        private void Update()
        {
            if (ReadSummonPressed())
            {
                TryUseSummonSlot1();
            }
        }

        public void ConfigureReferences(
            SummonEnergyLadder newEnergyLadder,
            CombatHealth newSourceHealth,
            PlayerCombatTargetSelector newTargetSelector,
            CombatHealth newFrontlineTargetHealth,
            SummonLaneSpace newLaneSpace,
            LaneActionProjectile newProjectilePrefab,
            GameObject newEntryCuePrefab,
            Transform newProjectileRoot,
            Transform newCueRoot)
        {
            energyLadder = newEnergyLadder;
            sourceHealth = newSourceHealth;
            targetSelector = newTargetSelector;
            frontlineTargetHealth = newFrontlineTargetHealth;
            laneSpace = newLaneSpace;
            projectilePrefab = newProjectilePrefab;
            projectilePrefabObject = newProjectilePrefab != null ? newProjectilePrefab.gameObject : null;
            entryCuePrefab = newEntryCuePrefab;
            projectileRoot = newProjectileRoot;
            cueRoot = newCueRoot;
        }

        public void ResetToDefaultTierSettings()
        {
            tierSettings = CreateDefaultTierSettings();
        }

        public void QueueSummonSlot1()
        {
            queued = true;
        }

        public bool TryUseSummonSlot1()
        {
            if (energyLadder == null || !energyLadder.TrySpend(out int spentTier))
            {
                return false;
            }

            lastSpentTier = Mathf.Clamp(spentTier, 1, 3);
            FireTier(lastSpentTier);
            return true;
        }

        private void FireTier(int tier)
        {
            SummonTierSettings settings = ResolveTierSettings(tier);
            Vector2 playerLane = laneSpace != null ? laneSpace.GetLaneCoordinates(transform.position) : Vector2.zero;
            float entryZ = laneSpace != null ? laneSpace.SummonEntryZ : playerLane.y + 2f;
            float targetZ = ResolveTargetLaneZ();
            int count = Mathf.Max(1, settings.ProjectileCount);

            Vector3 entryPosition = ResolveBattlefieldPoint(playerLane.x, entryZ, settings.EntryHeight);
            lastEntryPosition = entryPosition;
            SpawnEntryCue(entryPosition, settings);
            Vector3 firstTargetPosition = ResolveBattlefieldPoint(playerLane.x, targetZ, settings.TargetHeight);
            Vector3 actorFacing = ResolvePlanarDirection(firstTargetPosition - entryPosition);
            SummonFrontlineProxy actor = SpawnSummonActor(entryPosition, actorFacing, tier, settings);
            Vector3 projectileSpawnBase = actor != null
                ? actor.ProjectileOrigin.position
                : ResolveBattlefieldPoint(playerLane.x, entryZ, settings.EntryHeight + 0.7f);
            Vector3 right = ResolveRight(actorFacing);

            for (int i = 0; i < count; i++)
            {
                float targetOffset = ResolveOffset(i, count, settings.LateralReach);
                float targetX = playerLane.x + targetOffset;
                Vector3 spawnPosition = projectileSpawnBase + right * (targetOffset * 0.22f);
                Vector3 targetPosition = ResolveBattlefieldPoint(targetX, targetZ, settings.TargetHeight);
                Vector3 direction = ResolvePlanarDirection(targetPosition - spawnPosition);

                LaneActionProjectile projectile = GetProjectile();
                projectile.transform.position = spawnPosition;
                projectile.Configure(
                    sourceHealth,
                    sourceTeam,
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
                lastSummonActorPosition = position;
                return null;
            }

            actor.transform.SetParent(summonActorRoot != null ? summonActorRoot : transform, worldPositionStays: true);
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
                actor.PressureScreen.Activate(
                    sourceTeam,
                    settings.ScreenIntercepts,
                    settings.ScreenRadius,
                    settings.ScreenLifetimeSeconds);
            }

            lastSummonActorPosition = actor.transform.position;
            return actor;
        }

        private void OnPressureScreenIntercepted(SummonPressureScreen screen, BossBarrageProjectile projectile)
        {
            SummonFrontlineProxy actor = FindActorForPressureScreen(screen);
            if (actor == null || !actor.IsActive)
            {
                return;
            }

            FireCounterProjectile(actor, ResolveTierSettings(actor.ActiveTier));
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
                sourceHealth,
                sourceTeam,
                counterDamage,
                direction,
                counterSpeed,
                counterLifetime,
                counterRadius);
        }

        private Vector3 ResolveCounterTargetPosition(float targetHeight)
        {
            if (laneSpace != null)
            {
                if (frontlineTargetHealth != null && frontlineTargetHealth.IsAlive)
                {
                    Vector2 targetLane = laneSpace.GetLaneCoordinates(frontlineTargetHealth.transform.position);
                    return laneSpace.GetBattlefieldWorldPoint(targetLane.x, targetLane.y, targetHeight);
                }

                return laneSpace.GetBattlefieldWorldPoint(0f, laneSpace.BossProxyZ, targetHeight);
            }

            return transform.position + ResolvePlanarDirection(transform.forward) * 10f + Vector3.up * targetHeight;
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
            if (laneSpace == null)
            {
                return 8f;
            }

            if (frontlineTargetHealth != null && frontlineTargetHealth.IsAlive)
            {
                return laneSpace.GetLaneCoordinates(frontlineTargetHealth.transform.position).y;
            }

            if (targetSelector != null
                && targetSelector.TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth)
                && target != null
                && targetHealth != null
                && targetHealth.IsAlive)
            {
                return laneSpace.GetLaneCoordinates(target.position).y;
            }

            return laneSpace.BossProxyZ;
        }

        private Vector3 ResolveBattlefieldPoint(float lateralX, float laneZ, float worldY)
        {
            if (laneSpace != null)
            {
                return laneSpace.GetBattlefieldWorldPoint(lateralX, laneZ, worldY);
            }

            Vector3 right = Vector3.Cross(Vector3.up, ResolvePlanarDirection(transform.forward));
            return transform.position + ResolvePlanarDirection(transform.forward) * laneZ + right * lateralX + Vector3.up * worldY;
        }

        private void SpawnEntryCue(Vector3 position, SummonTierSettings settings)
        {
            GameObject cue = GetEntryCue();
            if (cue == null)
            {
                return;
            }

            cue.transform.SetParent(cueRoot != null ? cueRoot : transform, worldPositionStays: true);
            cue.transform.SetPositionAndRotation(position, Quaternion.identity);
            float scale = Mathf.Max(0.01f, settings.CueScale);
            cue.transform.localScale = new Vector3(scale, 0.04f, scale);
            cue.SetActive(true);

            if (settings.CueLifetimeSeconds > 0f)
            {
                StartCoroutine(ReleaseCueAfterSeconds(cue, settings.CueLifetimeSeconds));
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
                throw new InvalidOperationException($"{name} is missing a LaneActionProjectile prefab.");
            }

            Transform parent = projectileRoot != null ? projectileRoot : transform;
            LaneActionProjectile instance = Instantiate(prefab, parent);
            instance.name = prefab.name;
            projectiles.Add(instance);
            return instance;
        }

        private GameObject GetEntryCue()
        {
            if (entryCuePrefab == null)
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

            GameObject instance = Instantiate(entryCuePrefab, cueRoot != null ? cueRoot : transform);
            instance.name = entryCuePrefab.name;
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

            Transform parent = summonActorRoot != null ? summonActorRoot : transform;
            SummonFrontlineProxy instance = Instantiate(prefab, parent);
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
                cue.transform.SetParent(cueRoot != null ? cueRoot : transform, worldPositionStays: false);
                entryCuePool.Enqueue(cue);
            }
        }

        private LaneActionProjectile ResolveProjectilePrefab()
        {
            if (projectilePrefab != null)
            {
                return projectilePrefab;
            }

            if (projectilePrefabObject != null)
            {
                projectilePrefab = projectilePrefabObject.GetComponent<LaneActionProjectile>();
            }

            return projectilePrefab;
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

        private void PrewarmProjectiles()
        {
            LaneActionProjectile prefab = ResolveProjectilePrefab();
            if (prefab == null || prewarmCount <= 0)
            {
                return;
            }

            for (int i = projectiles.Count; i < prewarmCount; i++)
            {
                LaneActionProjectile projectile = Instantiate(prefab, projectileRoot != null ? projectileRoot : transform);
                projectile.name = prefab.name;
                projectile.Deactivate();
                projectiles.Add(projectile);
                projectilePool.Enqueue(projectile);
            }
        }

        private void PrewarmEntryCues()
        {
            if (entryCuePrefab == null)
            {
                return;
            }

            for (int i = entryCues.Count; i < 2; i++)
            {
                GameObject cue = Instantiate(entryCuePrefab, cueRoot != null ? cueRoot : transform);
                cue.name = entryCuePrefab.name;
                cue.SetActive(false);
                entryCues.Add(cue);
                entryCuePool.Enqueue(cue);
            }
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
                SummonFrontlineProxy actor = Instantiate(prefab, summonActorRoot != null ? summonActorRoot : transform);
                actor.name = prefab.name;
                actor.Deactivate();
                summonActors.Add(actor);
                summonActorPool.Enqueue(actor);
            }
        }

        private SummonTierSettings ResolveTierSettings(int tier)
        {
            if (tierSettings == null || tierSettings.Length == 0)
            {
                return new SummonTierSettings
                {
                    Damage = 55f,
                    ProjectileSpeed = 17f,
                    LifetimeSeconds = 2.4f,
                    Radius = 0.34f,
                    ProjectileCount = 1,
                    LateralReach = 1f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.3f,
                    CueScale = 1.5f,
                    CueLifetimeSeconds = 0.85f,
                    ActorLifetimeSeconds = 1.25f,
                    ActorScale = 1f,
                    ActorAdvanceDistance = 1f,
                    ActorAdvanceSeconds = 0.24f,
                    ScreenIntercepts = 2,
                    ScreenRadius = 1.25f,
                    ScreenLifetimeSeconds = 1.15f,
                    CounterDamage = 16f,
                    CounterProjectileSpeed = 20f,
                    CounterLifetimeSeconds = 1.65f,
                    CounterRadius = 0.24f,
                    CounterTargetHeight = 1.35f
                };
            }

            return tierSettings[Mathf.Clamp(tier - 1, 0, tierSettings.Length - 1)];
        }

        private static SummonTierSettings[] CreateDefaultTierSettings()
        {
            return new[]
            {
                new SummonTierSettings
                {
                    Damage = 58f,
                    ProjectileSpeed = 17f,
                    LifetimeSeconds = 2.4f,
                    Radius = 0.34f,
                    ProjectileCount = 1,
                    LateralReach = 1.2f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.35f,
                    CueScale = 1.45f,
                    CueLifetimeSeconds = 0.85f,
                    ActorLifetimeSeconds = 1.25f,
                    ActorScale = 0.9f,
                    ActorAdvanceDistance = 1.05f,
                    ActorAdvanceSeconds = 0.24f,
                    ScreenIntercepts = 2,
                    ScreenRadius = 1.25f,
                    ScreenLifetimeSeconds = 1.15f,
                    CounterDamage = 16f,
                    CounterProjectileSpeed = 20f,
                    CounterLifetimeSeconds = 1.65f,
                    CounterRadius = 0.24f,
                    CounterTargetHeight = 1.35f
                },
                new SummonTierSettings
                {
                    Damage = 66f,
                    ProjectileSpeed = 18.5f,
                    LifetimeSeconds = 2.65f,
                    Radius = 0.38f,
                    ProjectileCount = 2,
                    LateralReach = 4.2f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.35f,
                    CueScale = 1.85f,
                    CueLifetimeSeconds = 1f,
                    ActorLifetimeSeconds = 1.55f,
                    ActorScale = 1.08f,
                    ActorAdvanceDistance = 1.65f,
                    ActorAdvanceSeconds = 0.3f,
                    ScreenIntercepts = 4,
                    ScreenRadius = 1.55f,
                    ScreenLifetimeSeconds = 1.4f,
                    CounterDamage = 22f,
                    CounterProjectileSpeed = 21.5f,
                    CounterLifetimeSeconds = 1.8f,
                    CounterRadius = 0.27f,
                    CounterTargetHeight = 1.4f
                },
                new SummonTierSettings
                {
                    Damage = 78f,
                    ProjectileSpeed = 20f,
                    LifetimeSeconds = 2.9f,
                    Radius = 0.42f,
                    ProjectileCount = 3,
                    LateralReach = 6.8f,
                    EntryHeight = 0.18f,
                    TargetHeight = 1.45f,
                    CueScale = 2.25f,
                    CueLifetimeSeconds = 1.15f,
                    ActorLifetimeSeconds = 1.85f,
                    ActorScale = 1.28f,
                    ActorAdvanceDistance = 2.35f,
                    ActorAdvanceSeconds = 0.36f,
                    ScreenIntercepts = 7,
                    ScreenRadius = 1.9f,
                    ScreenLifetimeSeconds = 1.7f,
                    CounterDamage = 30f,
                    CounterProjectileSpeed = 23f,
                    CounterLifetimeSeconds = 2f,
                    CounterRadius = 0.3f,
                    CounterTargetHeight = 1.45f
                }
            };
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

        private bool ReadSummonPressed()
        {
            bool pressed = queued;
            queued = false;

            if (summonAction != null && summonAction.action != null)
            {
                pressed |= summonAction.action.WasPressedThisFrame();
            }

            if (pressed || !useKeyboardWhenActionMissing || !IsActionMissing(summonAction))
            {
                return pressed;
            }

            return Keyboard.current != null
                && Keyboard.current[keyboardTestKey] != null
                && Keyboard.current[keyboardTestKey].wasPressedThisFrame;
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

        private static bool EnableActionIfNeeded(InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null || actionReference.action.enabled)
            {
                return false;
            }

            actionReference.action.Enable();
            return true;
        }

        private static void DisableActionIfOwned(InputActionReference actionReference, bool enabledHere)
        {
            if (enabledHere && actionReference != null && actionReference.action != null)
            {
                actionReference.action.Disable();
            }
        }

        private static bool IsActionMissing(InputActionReference actionReference)
        {
            return actionReference == null || actionReference.action == null;
        }
    }
}
