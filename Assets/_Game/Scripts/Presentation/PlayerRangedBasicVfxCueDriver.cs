using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerRangedBasicVfxCueDriver : MonoBehaviour
    {
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform muzzleAnchor;
        [SerializeField] private CombatVfxCueId muzzleFlashCueId = CombatVfxCueId.PlayerRangedMuzzleFlash;
        [SerializeField, Min(0f)] private float muzzleFlashIntensity = 1f;
        [SerializeField] private CombatVfxCueId impactCueId = CombatVfxCueId.PlayerRangedProjectileImpact;
        [SerializeField, Min(0f)] private float impactIntensity = 1f;

        private readonly HashSet<LaneActionProjectile> watchedProjectiles = new HashSet<LaneActionProjectile>();
        private bool subscribed;

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
                muzzleFlashIntensity);
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

            cuePlayer.PlayCue(
                impactCueId,
                projectile.transform,
                impactDirection,
                impactIntensity);
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
