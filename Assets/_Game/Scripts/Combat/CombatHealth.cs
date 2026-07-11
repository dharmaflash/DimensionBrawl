using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DimensionBrawl.Combat
{
    public enum DamageTeam
    {
        Neutral = 0,
        Player = 1,
        Enemy = 2,
        AllySummon = 3
    }

    public enum DamageResponsePolicy
    {
        Default = 0,
        DamageOnly = 1,
        FlashOnly = 2,
        Flinch = 3,
        Stagger = 4,
        Break = 5,
        Knockdown = 6
    }

    public enum CombatControlLockPolicy
    {
        None = 0,
        InterruptAction = 1,
        HardLock = 2
    }

    public static class DamageResponsePolicyUtility
    {
        public static bool PlaysDamagePresentation(DamageResponsePolicy responsePolicy)
        {
            return responsePolicy != DamageResponsePolicy.DamageOnly;
        }

        public static bool PlaysFullBodyHitAnimation(DamageInfo damageInfo)
        {
            return PlaysFullBodyHitAnimation(damageInfo.ResponsePolicy, damageInfo.ControlLockPolicy);
        }

        public static bool PlaysFullBodyHitAnimation(
            DamageResponsePolicy responsePolicy,
            CombatControlLockPolicy controlLockPolicy)
        {
            return InterruptsAction(controlLockPolicy)
                && (responsePolicy == DamageResponsePolicy.Default
                || responsePolicy == DamageResponsePolicy.Stagger
                || responsePolicy == DamageResponsePolicy.Break
                || responsePolicy == DamageResponsePolicy.Knockdown);
        }

        public static bool InterruptsAction(CombatControlLockPolicy controlLockPolicy)
        {
            return controlLockPolicy != CombatControlLockPolicy.None;
        }
    }

    public static class CombatTeamUtility
    {
        public static bool AreAllied(DamageTeam first, DamageTeam second)
        {
            if (first == DamageTeam.Neutral || second == DamageTeam.Neutral)
            {
                return false;
            }

            if (first == second)
            {
                return true;
            }

            return IsPlayerSide(first) && IsPlayerSide(second);
        }

        public static bool AreHostile(DamageTeam first, DamageTeam second)
        {
            return first != DamageTeam.Neutral
                && second != DamageTeam.Neutral
                && !AreAllied(first, second);
        }

        public static bool IsPlayerSide(DamageTeam team)
        {
            return team == DamageTeam.Player || team == DamageTeam.AllySummon;
        }
    }

    public readonly struct DamageInfo
    {
        public DamageInfo(
            CombatHealth source,
            DamageTeam sourceTeam,
            float amount,
            Vector3 point,
            Vector3 direction,
            float hitStopSeconds,
            DamageResponsePolicy responsePolicy = DamageResponsePolicy.Default,
            CombatControlLockPolicy controlLockPolicy = CombatControlLockPolicy.InterruptAction)
        {
            Source = source;
            SourceTeam = sourceTeam;
            Amount = amount;
            Point = point;
            Direction = direction;
            HitStopSeconds = hitStopSeconds;
            ResponsePolicy = responsePolicy;
            ControlLockPolicy = controlLockPolicy;
        }

        public CombatHealth Source { get; }
        public DamageTeam SourceTeam { get; }
        public float Amount { get; }
        public Vector3 Point { get; }
        public Vector3 Direction { get; }
        public float HitStopSeconds { get; }
        public DamageResponsePolicy ResponsePolicy { get; }
        public CombatControlLockPolicy ControlLockPolicy { get; }
    }

    public sealed class DamageModificationContext
    {
        public DamageModificationContext(DamageInfo damageInfo)
        {
            Reset(damageInfo);
        }

        public DamageInfo DamageInfo { get; private set; }
        public float ModifiedAmount { get; private set; }

        internal void Reset(DamageInfo damageInfo)
        {
            DamageInfo = damageInfo;
            ModifiedAmount = damageInfo.Amount;
        }

        public void ScaleAmount(float multiplier)
        {
            ModifiedAmount = Mathf.Max(0f, ModifiedAmount * Mathf.Max(0f, multiplier));
        }

        public void SetAmount(float amount)
        {
            ModifiedAmount = Mathf.Max(0f, amount);
        }

        public DamageInfo ToResolvedDamageInfo()
        {
            return new DamageInfo(
                DamageInfo.Source,
                DamageInfo.SourceTeam,
                ModifiedAmount,
                DamageInfo.Point,
                DamageInfo.Direction,
                DamageInfo.HitStopSeconds,
                DamageInfo.ResponsePolicy,
                DamageInfo.ControlLockPolicy);
        }
    }

    public sealed class CombatHealth : MonoBehaviour
    {
        private readonly struct ColliderHealthBinding
        {
            public ColliderHealthBinding(Collider collider, CombatHealth health)
            {
                Collider = collider;
                Health = health;
            }

            public Collider Collider { get; }
            public CombatHealth Health { get; }
        }

        private static readonly List<CombatHealth> ActiveHealth = new();
        private static readonly Dictionary<int, ColliderHealthBinding> ColliderHealthBindings = new(128);

        [SerializeField] private DamageTeam team = DamageTeam.Neutral;
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private bool startAtFullHealth = true;
        [SerializeField] private UnityEvent onDamaged = new UnityEvent();
        [SerializeField] private UnityEvent onDied = new UnityEvent();

        private float currentHealth;
        private float invulnerableUntilTime;
        private bool isDead;
        private DamageModificationContext reusableDamageModificationContext;
        private bool damageModificationInProgress;

        public event Action<DamageInfo> Damaged;
        public event Action<DamageModificationContext> DamageModifying;
        public event Action<DamageInfo> DamageBlockedByInvulnerability;
        public event Action Died;
        public static event Action<CombatHealth> BecameActive;
        public static event Action<CombatHealth> BecameInactive;

        public DamageTeam Team => team;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthRatio => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsAlive => !isDead;
        public bool IsInvulnerable => Time.time < invulnerableUntilTime;
        public static IReadOnlyList<CombatHealth> ActiveInstances => ActiveHealth;
        public static int CachedColliderBindingCount => ColliderHealthBindings.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            ActiveHealth.Clear();
            ColliderHealthBindings.Clear();
            BecameActive = null;
            BecameInactive = null;
        }

        public static CombatHealth ResolveFromCollider(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            int id = collider.GetInstanceID();
            if (ColliderHealthBindings.TryGetValue(id, out ColliderHealthBinding binding)
                && binding.Collider == collider
                && binding.Health != null)
            {
                return binding.Health;
            }

            CombatHealth health = collider.GetComponentInParent<CombatHealth>();
            if (health != null)
            {
                ColliderHealthBindings[id] = new ColliderHealthBinding(collider, health);
            }
            else
            {
                ColliderHealthBindings.Remove(id);
            }

            return health;
        }

        public void ConfigureTeam(DamageTeam newTeam)
        {
            team = newTeam;
        }

        public void ConfigureMaxHealth(float newMaxHealth, bool resetToFull = true)
        {
            maxHealth = Mathf.Max(1f, newMaxHealth);
            if (resetToFull)
            {
                ResetHealthToFull();
                return;
            }

            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            isDead = currentHealth <= 0f;
        }

        public void ResetHealthToFull()
        {
            currentHealth = maxHealth;
            invulnerableUntilTime = 0f;
            isDead = false;
        }

        private void Awake()
        {
            currentHealth = startAtFullHealth ? maxHealth : Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        private void OnEnable()
        {
            CombatTimeDilationReceiver.Ensure(gameObject);
            if (!ActiveHealth.Contains(this))
            {
                ActiveHealth.Add(this);
                BecameActive?.Invoke(this);
            }
        }

        private void OnDisable()
        {
            if (ActiveHealth.Remove(this))
            {
                BecameInactive?.Invoke(this);
            }
        }

        private void OnDestroy()
        {
            if (ActiveHealth.Remove(this))
            {
                BecameInactive?.Invoke(this);
            }
        }

        public void SetTemporaryInvulnerability(float seconds)
        {
            invulnerableUntilTime = Mathf.Max(invulnerableUntilTime, Time.time + Mathf.Max(0f, seconds));
        }

        public void SetInvulnerableUntil(float worldTime)
        {
            invulnerableUntilTime = Mathf.Max(invulnerableUntilTime, worldTime);
        }

        public bool TryApplyDamage(DamageInfo damageInfo)
        {
            if (!IsAlive || damageInfo.Amount <= 0f)
            {
                return false;
            }

            if (CombatTeamUtility.AreAllied(damageInfo.SourceTeam, team))
            {
                return false;
            }

            if (IsInvulnerable)
            {
                DamageBlockedByInvulnerability?.Invoke(damageInfo);
                return false;
            }

            DamageInfo resolvedDamageInfo = damageInfo;
            Action<DamageModificationContext> damageModifiers = DamageModifying;
            if (damageModifiers != null)
            {
                bool reuseContext = !damageModificationInProgress;
                DamageModificationContext modificationContext;
                if (reuseContext)
                {
                    reusableDamageModificationContext ??= new DamageModificationContext(damageInfo);
                    reusableDamageModificationContext.Reset(damageInfo);
                    modificationContext = reusableDamageModificationContext;
                    damageModificationInProgress = true;
                }
                else
                {
                    modificationContext = new DamageModificationContext(damageInfo);
                }

                try
                {
                    damageModifiers.Invoke(modificationContext);
                    if (modificationContext.ModifiedAmount <= 0f)
                    {
                        return false;
                    }

                    resolvedDamageInfo = modificationContext.ToResolvedDamageInfo();
                }
                finally
                {
                    if (reuseContext)
                    {
                        damageModificationInProgress = false;
                    }
                }
            }

            currentHealth = Mathf.Max(0f, currentHealth - resolvedDamageInfo.Amount);
            Damaged?.Invoke(resolvedDamageInfo);
            onDamaged.Invoke();

            if (currentHealth <= 0f)
            {
                isDead = true;
                Died?.Invoke();
                onDied.Invoke();
            }

            return true;
        }
    }
}
