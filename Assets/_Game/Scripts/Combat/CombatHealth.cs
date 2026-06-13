using System;
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
            float hitStopSeconds)
        {
            Source = source;
            SourceTeam = sourceTeam;
            Amount = amount;
            Point = point;
            Direction = direction;
            HitStopSeconds = hitStopSeconds;
        }

        public CombatHealth Source { get; }
        public DamageTeam SourceTeam { get; }
        public float Amount { get; }
        public Vector3 Point { get; }
        public Vector3 Direction { get; }
        public float HitStopSeconds { get; }
    }

    public sealed class DamageModificationContext
    {
        public DamageModificationContext(DamageInfo damageInfo)
        {
            DamageInfo = damageInfo;
            ModifiedAmount = damageInfo.Amount;
        }

        public DamageInfo DamageInfo { get; }
        public float ModifiedAmount { get; private set; }

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
                DamageInfo.HitStopSeconds);
        }
    }

    public sealed class CombatHealth : MonoBehaviour
    {
        [SerializeField] private DamageTeam team = DamageTeam.Neutral;
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private bool startAtFullHealth = true;
        [SerializeField] private UnityEvent onDamaged = new UnityEvent();
        [SerializeField] private UnityEvent onDied = new UnityEvent();

        private float currentHealth;
        private float invulnerableUntilTime;
        private bool isDead;

        public event Action<DamageInfo> Damaged;
        public event Action<DamageModificationContext> DamageModifying;
        public event Action Died;

        public DamageTeam Team => team;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthRatio => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsAlive => !isDead;
        public bool IsInvulnerable => Time.time < invulnerableUntilTime;

        public void ConfigureTeam(DamageTeam newTeam)
        {
            team = newTeam;
        }

        private void Awake()
        {
            currentHealth = startAtFullHealth ? maxHealth : Mathf.Clamp(currentHealth, 0f, maxHealth);
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
            if (!IsAlive || IsInvulnerable || damageInfo.Amount <= 0f)
            {
                return false;
            }

            if (CombatTeamUtility.AreAllied(damageInfo.SourceTeam, team))
            {
                return false;
            }

            DamageModificationContext modificationContext = new DamageModificationContext(damageInfo);
            DamageModifying?.Invoke(modificationContext);
            if (modificationContext.ModifiedAmount <= 0f)
            {
                return false;
            }

            DamageInfo resolvedDamageInfo = modificationContext.ToResolvedDamageInfo();
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
