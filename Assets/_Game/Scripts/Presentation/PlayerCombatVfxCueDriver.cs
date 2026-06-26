using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerCombatVfxCueDriver : MonoBehaviour
    {
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform attackAnchor;
        [SerializeField] private Transform dodgeAnchor;
        [SerializeField] private Transform damageAnchor;
        [SerializeField] private CombatVfxCueId damagedCueId = CombatVfxCueId.PlayerDamaged;
        [SerializeField] private CombatVfxCueId criticalCueId = CombatVfxCueId.PlayerCritical;
        [SerializeField, Min(0f)] private float damagedCueIntensity = 1f;
        [SerializeField, Range(0.05f, 0.95f)] private float criticalHealthRatio = 0.35f;
        [SerializeField, Min(0f)] private float criticalCueIntensity = 1.18f;

        private bool actionSubscribed;
        private bool healthSubscribed;
        private bool criticalCuePlayed;
        private int damageVfxCueRequestCount;
        private int criticalVfxCueRequestCount;

        public CombatHealth PlayerHealth => playerHealth;
        public Transform DamageAnchor => damageAnchor != null ? damageAnchor : attackAnchor;
        public CombatVfxCueId DamagedCueId => damagedCueId;
        public CombatVfxCueId CriticalCueId => criticalCueId;
        public int DamageVfxCueRequestCount => damageVfxCueRequestCount;
        public int CriticalVfxCueRequestCount => criticalVfxCueRequestCount;

        public void ConfigureDamageFeedback(CombatHealth newPlayerHealth, Transform newDamageAnchor)
        {
            UnsubscribeHealth();
            playerHealth = newPlayerHealth;
            damageAnchor = newDamageAnchor;
            SubscribeHealth();
        }

        private void Awake()
        {
            if (actionController == null)
            {
                actionController = GetComponent<PlayerActionController>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<CombatHealth>();
            }

            if (cuePlayer == null)
            {
                cuePlayer = GetComponent<CombatVfxCuePlayer>();
            }
        }

        private void OnEnable()
        {
            SubscribeActions();
            SubscribeHealth();
        }

        private void OnDisable()
        {
            UnsubscribeActions();
            UnsubscribeHealth();
        }

        private void SubscribeActions()
        {
            if (actionSubscribed || actionController == null)
            {
                return;
            }

            actionController.BasicAttackStarted += HandleBasicAttackStarted;
            actionController.BasicAttackHit += HandleBasicAttackHit;
            actionController.DodgeStarted += HandleDodgeStarted;
            actionSubscribed = true;
        }

        private void UnsubscribeActions()
        {
            if (!actionSubscribed || actionController == null)
            {
                actionSubscribed = false;
                return;
            }

            actionController.BasicAttackStarted -= HandleBasicAttackStarted;
            actionController.BasicAttackHit -= HandleBasicAttackHit;
            actionController.DodgeStarted -= HandleDodgeStarted;
            actionSubscribed = false;
        }

        private void SubscribeHealth()
        {
            if (healthSubscribed || playerHealth == null)
            {
                return;
            }

            playerHealth.Damaged += HandlePlayerDamaged;
            playerHealth.Died += HandlePlayerDied;
            healthSubscribed = true;
        }

        private void UnsubscribeHealth()
        {
            if (!healthSubscribed || playerHealth == null)
            {
                healthSubscribed = false;
                return;
            }

            playerHealth.Damaged -= HandlePlayerDamaged;
            playerHealth.Died -= HandlePlayerDied;
            healthSubscribed = false;
        }

        private void HandleBasicAttackStarted(int comboIndex)
        {
            Play(CombatVfxCueId.PlayerBasicAttackStart, attackAnchor, actionController.LastAttackDirection, ResolveComboIntensity(comboIndex));
        }

        private void HandleBasicAttackHit(int comboIndex)
        {
            Play(CombatVfxCueId.PlayerBasicAttackHit, attackAnchor, actionController.LastAttackDirection, ResolveComboIntensity(comboIndex));
        }

        private void HandleDodgeStarted()
        {
            Play(CombatVfxCueId.PlayerDodgeStart, dodgeAnchor, actionController.LastDodgeDirection, 1f);
        }

        private void HandlePlayerDamaged(DamageInfo damageInfo)
        {
            if (!DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy))
            {
                return;
            }

            float damageScale = playerHealth != null && playerHealth.MaxHealth > 0f
                ? Mathf.Clamp01(damageInfo.Amount / playerHealth.MaxHealth)
                : 0f;

            if (Play(damagedCueId, DamageAnchor, damageInfo.Direction, damagedCueIntensity + damageScale * 0.35f))
            {
                damageVfxCueRequestCount++;
            }

            if (playerHealth == null || playerHealth.HealthRatio > criticalHealthRatio)
            {
                criticalCuePlayed = false;
                return;
            }

            if (criticalCuePlayed)
            {
                return;
            }

            if (Play(criticalCueId, DamageAnchor, damageInfo.Direction, criticalCueIntensity))
            {
                criticalVfxCueRequestCount++;
                criticalCuePlayed = true;
            }
        }

        private void HandlePlayerDied()
        {
            if (criticalCuePlayed)
            {
                return;
            }

            if (Play(criticalCueId, DamageAnchor, Vector3.back, criticalCueIntensity + 0.22f))
            {
                criticalVfxCueRequestCount++;
                criticalCuePlayed = true;
            }
        }

        private bool Play(CombatVfxCueId cueId, Transform anchor, Vector3 direction, float intensity)
        {
            if (cuePlayer == null)
            {
                return false;
            }

            return cuePlayer.PlayCue(cueId, anchor != null ? anchor : transform, direction, intensity);
        }

        private static float ResolveComboIntensity(int comboIndex)
        {
            return Mathf.Lerp(1f, 1.35f, Mathf.Clamp01(comboIndex / 4f));
        }
    }
}
