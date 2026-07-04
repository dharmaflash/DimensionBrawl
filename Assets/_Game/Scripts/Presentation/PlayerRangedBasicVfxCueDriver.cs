using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerRangedBasicVfxCueDriver : MonoBehaviour
    {
        public const bool DefaultPlayImpactVfx = false;
        public const bool DefaultPlayImpactAudio = true;
        public const CombatVfxCueId DefaultImpactCueId = CombatVfxCueId.PlayerRangedProjectileImpact;
        public const float DefaultImpactIntensity = 1f;
        public const float DefaultImpactAudioIntensity = 0.36f;

        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform muzzleAnchor;
        [SerializeField] private CombatVfxCueId muzzleFlashCueId = CombatVfxCueId.PlayerRangedMuzzleFlash;
        [SerializeField, Min(0f)] private float muzzleFlashIntensity = 1f;
        [SerializeField, Min(0f)] private float muzzleFlashAudioIntensity = 1f;
        [SerializeField] private bool playImpactVfx = DefaultPlayImpactVfx;
        [SerializeField] private bool playImpactAudio = DefaultPlayImpactAudio;
        [SerializeField] private CombatVfxCueId impactCueId = DefaultImpactCueId;
        [SerializeField, Min(0f)] private float impactIntensity = DefaultImpactIntensity;
        [SerializeField, Min(0f)] private float impactAudioIntensity = DefaultImpactAudioIntensity;

        private readonly HashSet<LaneActionProjectile> watchedProjectiles = new HashSet<LaneActionProjectile>();
        private bool subscribed;

        public bool PlayImpactVfx => playImpactVfx;
        public bool PlayImpactAudio => playImpactAudio;

        private void Awake()
        {
            if (rangedBasicAttackAction == null)
            {
                rangedBasicAttackAction = GetComponent<PlayerRangedBasicAttackAction>();
            }

            if (cuePlayer == null)
            {
                cuePlayer = GetComponent<CombatVfxCuePlayer>();
            }

            if (muzzleAnchor == null && rangedBasicAttackAction != null)
            {
                muzzleAnchor = rangedBasicAttackAction.FireOrigin;
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            CombatVfxCuePlayer newCuePlayer,
            Transform newMuzzleAnchor)
        {
            Unsubscribe();
            rangedBasicAttackAction = newRangedBasicAttackAction;
            cuePlayer = newCuePlayer;
            muzzleAnchor = newMuzzleAnchor;

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            if (subscribed || rangedBasicAttackAction == null)
            {
                return;
            }

            rangedBasicAttackAction.RangedFireStarted += HandleRangedFireStarted;
            rangedBasicAttackAction.RangedProjectileFired += HandleRangedProjectileFired;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || rangedBasicAttackAction == null)
            {
                subscribed = false;
                return;
            }

            rangedBasicAttackAction.RangedFireStarted -= HandleRangedFireStarted;
            rangedBasicAttackAction.RangedProjectileFired -= HandleRangedProjectileFired;
            UnsubscribeProjectiles();
            subscribed = false;
        }

        private void HandleRangedFireStarted()
        {
            if (cuePlayer == null || rangedBasicAttackAction == null)
            {
                return;
            }

            Transform anchor = muzzleAnchor != null ? muzzleAnchor : rangedBasicAttackAction.transform;
            cuePlayer.PlayCue(
                muzzleFlashCueId,
                anchor,
                rangedBasicAttackAction.LastResolvedFireDirection,
                muzzleFlashIntensity,
                muzzleFlashAudioIntensity);
        }

        private void HandleRangedProjectileFired(LaneActionProjectile projectile)
        {
            if (projectile == null || watchedProjectiles.Contains(projectile))
            {
                return;
            }

            watchedProjectiles.Add(projectile);
            projectile.DamageApplied += HandleProjectileDamageApplied;
        }

        private void HandleProjectileDamageApplied(
            LaneActionProjectile projectile,
            CombatHealth targetHealth,
            Vector3 impactPoint,
            Vector3 impactDirection)
        {
            if (cuePlayer == null || projectile == null)
            {
                return;
            }

            if (!playImpactVfx && (!playImpactAudio || impactAudioIntensity <= 0f))
            {
                return;
            }

            cuePlayer.PlayCue(
                impactCueId,
                projectile.transform,
                impactDirection,
                playImpactVfx ? impactIntensity : 0f,
                playImpactAudio ? impactAudioIntensity : 0f);
        }

        private void UnsubscribeProjectiles()
        {
            foreach (LaneActionProjectile projectile in watchedProjectiles)
            {
                if (projectile != null)
                {
                    projectile.DamageApplied -= HandleProjectileDamageApplied;
                }
            }

            watchedProjectiles.Clear();
        }
    }
}
