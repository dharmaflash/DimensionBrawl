using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DimensionBrawl.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerSkill1Action : MonoBehaviour
    {
        [Serializable]
        private struct SkillTierSettings
        {
            [Min(0f)] public float Damage;
            [Min(0f)] public float ProjectileSpeed;
            [Min(0.01f)] public float LifetimeSeconds;
            [Min(0.01f)] public float Radius;
            [Min(1)] public int ProjectileCount;
            [Min(0f)] public float LateralSpread;
            [Min(0f)] public float SpawnForwardOffset;
            [Min(0f)] public float SpawnHeight;
            [Min(0f)] public float TargetHeight;
        }

        [Header("Input")]
        [SerializeField] private InputActionReference skillAction;
        [SerializeField] private bool useKeyboardWhenActionMissing = true;
        [SerializeField] private Key keyboardTestKey = Key.E;

        [Header("References")]
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;

        [Header("Projectile")]
        [SerializeField] private LaneActionProjectile projectilePrefab;
        [SerializeField] private GameObject projectilePrefabObject;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.Player;
        [SerializeField, Min(0)] private int prewarmCount = 4;
        [Header("Failure Feedback")]
        [SerializeField, Min(0f)] private float useBlockedHintSeconds = 0.75f;

        [Header("Tier Tuning")]
        [SerializeField] private SkillTierSettings[] tierSettings =
        {
            new SkillTierSettings
            {
                Damage = 38f,
                ProjectileSpeed = 24f,
                LifetimeSeconds = 1.7f,
                Radius = 0.28f,
                ProjectileCount = 1,
                LateralSpread = 0f,
                SpawnForwardOffset = 0.85f,
                SpawnHeight = 1.15f,
                TargetHeight = 1.25f
            },
            new SkillTierSettings
            {
                Damage = 44f,
                ProjectileSpeed = 25.5f,
                LifetimeSeconds = 1.85f,
                Radius = 0.3f,
                ProjectileCount = 2,
                LateralSpread = 0.55f,
                SpawnForwardOffset = 0.9f,
                SpawnHeight = 1.2f,
                TargetHeight = 1.25f
            },
            new SkillTierSettings
            {
                Damage = 52f,
                ProjectileSpeed = 27f,
                LifetimeSeconds = 2f,
                Radius = 0.32f,
                ProjectileCount = 3,
                LateralSpread = 0.9f,
                SpawnForwardOffset = 0.95f,
                SpawnHeight = 1.25f,
                TargetHeight = 1.3f
            }
        };

        private readonly List<LaneActionProjectile> projectiles = new List<LaneActionProjectile>();
        private readonly Queue<LaneActionProjectile> projectilePool = new Queue<LaneActionProjectile>();
        private bool actionEnabledHere;
        private bool queued;
        private int lastSpentTier;
        private int lastFiredProjectileCount;
        private int totalUseCount;
        private float blockedHintTimer;
        private string lastBlockedReason;

        public int LastSpentTier => lastSpentTier;
        public int LastFiredProjectileCount => lastFiredProjectileCount;
        public int TotalUseCount => totalUseCount;
        public int ActiveProjectileCount => CountActiveProjectiles();
        public bool ShowUseBlockedHint => blockedHintTimer > 0f;
        public string LastUseBlockedReason => lastBlockedReason;

        public event Action<int> Skill1Used;
        public event Action Skill1UseBlocked;

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
            actionEnabledHere = EnableActionIfNeeded(skillAction);
            PrewarmProjectiles();
        }

        private void OnDisable()
        {
            DisableActionIfOwned(skillAction, actionEnabledHere);
            actionEnabledHere = false;
        }

        private void Update()
        {
            TickFeedback(Time.deltaTime);
            if (ReadSkillPressed())
            {
                TryUseSkill1();
            }
        }

        public void ConfigureReferences(
            SummonEnergyLadder newEnergyLadder,
            CombatHealth newSourceHealth,
            PlayerCombatTargetSelector newTargetSelector,
            LaneActionProjectile newProjectilePrefab,
            Transform newProjectileRoot)
        {
            energyLadder = newEnergyLadder;
            sourceHealth = newSourceHealth;
            targetSelector = newTargetSelector;
            projectilePrefab = newProjectilePrefab;
            projectilePrefabObject = newProjectilePrefab != null ? newProjectilePrefab.gameObject : null;
            projectileRoot = newProjectileRoot;
        }

        public void QueueSkill1()
        {
            queued = true;
        }

        public bool TryUseSkill1()
        {
            if (energyLadder == null)
            {
                SetUseBlocked("Energy system missing");
                return false;
            }

            if (!energyLadder.TrySpend(out int spentTier))
            {
                SetUseBlocked("EN not ready");
                return false;
            }

            lastSpentTier = Mathf.Clamp(spentTier, 1, 3);
            totalUseCount++;
            blockedHintTimer = 0f;
            lastBlockedReason = null;
            FireTier(lastSpentTier);
            Skill1Used?.Invoke(lastSpentTier);
            return true;
        }

        private void SetUseBlocked(string reason)
        {
            lastBlockedReason = string.IsNullOrWhiteSpace(reason) ? "Unavailable" : reason;
            blockedHintTimer = useBlockedHintSeconds;
            Skill1UseBlocked?.Invoke();
        }

        private void TickFeedback(float deltaTime)
        {
            if (blockedHintTimer <= 0f)
            {
                return;
            }

            blockedHintTimer = Mathf.Max(0f, blockedHintTimer - deltaTime);
            if (blockedHintTimer <= 0f)
            {
                lastBlockedReason = null;
            }
        }

        private void FireTier(int tier)
        {
            SkillTierSettings settings = ResolveTierSettings(tier);
            Vector3 spawnBase = transform.position;
            Vector3 direction = ResolveAimDirection(spawnBase + Vector3.up * settings.SpawnHeight, settings.TargetHeight);
            Vector3 right = ResolveRight(direction);
            int count = Mathf.Max(1, settings.ProjectileCount);
            lastFiredProjectileCount = count;

            for (int i = 0; i < count; i++)
            {
                float lateralOffset = ResolveOffset(i, count, settings.LateralSpread);
                Vector3 spawnPosition = spawnBase
                    + direction * settings.SpawnForwardOffset
                    + right * lateralOffset
                    + Vector3.up * settings.SpawnHeight;

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

        private Vector3 ResolveAimDirection(Vector3 spawnPosition, float targetHeight)
        {
            if (targetSelector != null
                && targetSelector.TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth)
                && target != null
                && targetHealth != null
                && targetHealth.IsAlive)
            {
                Vector3 targetPosition = target.position + Vector3.up * targetHeight;
                Vector3 targetDirection = Vector3.ProjectOnPlane(targetPosition - spawnPosition, Vector3.up);
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    return targetDirection.normalized;
                }
            }

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude > 0.0001f)
            {
                return forward.normalized;
            }

            return Vector3.forward;
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

        private SkillTierSettings ResolveTierSettings(int tier)
        {
            if (tierSettings == null || tierSettings.Length == 0)
            {
                return new SkillTierSettings
                {
                    Damage = 30f,
                    ProjectileSpeed = 22f,
                    LifetimeSeconds = 1.5f,
                    Radius = 0.28f,
                    ProjectileCount = 1,
                    SpawnForwardOffset = 0.8f,
                    SpawnHeight = 1.1f,
                    TargetHeight = 1.2f
                };
            }

            return tierSettings[Mathf.Clamp(tier - 1, 0, tierSettings.Length - 1)];
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

        private bool ReadSkillPressed()
        {
            bool pressed = queued;
            queued = false;

            if (skillAction != null && skillAction.action != null)
            {
                pressed |= skillAction.action.WasPressedThisFrame();
            }

            if (pressed || !useKeyboardWhenActionMissing || !IsActionMissing(skillAction))
            {
                return pressed;
            }

            return Keyboard.current != null
                && Keyboard.current[keyboardTestKey] != null
                && Keyboard.current[keyboardTestKey].wasPressedThisFrame;
        }

        private static Vector3 ResolveRight(Vector3 direction)
        {
            Vector3 right = Vector3.Cross(Vector3.up, direction);
            return right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
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
