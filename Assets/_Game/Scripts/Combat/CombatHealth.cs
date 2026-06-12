using System;
using UnityEngine;
using UnityEngine.Events;

namespace DimensionBrawl.Combat
{
    public enum DamageTeam
    {
        Neutral = 0,
        Player = 1,
        Enemy = 2
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
        public event Action Died;

        public DamageTeam Team => team;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthRatio => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsAlive => !isDead;
        public bool IsInvulnerable => Time.time < invulnerableUntilTime;

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

            if (damageInfo.SourceTeam != DamageTeam.Neutral && damageInfo.SourceTeam == team)
            {
                return false;
            }

            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            Damaged?.Invoke(damageInfo);
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
