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
        public const bool DefaultPlayPhysicalImpactVfx = true;
        public const CombatVfxCueId DefaultImpactCueId = CombatVfxCueId.PlayerRangedProjectileImpact;
        public const float DefaultImpactIntensity = 1f;
        public const float DefaultImpactAudioIntensity = 0.36f;
        public const float DefaultPhysicalImpactVfxScale = 0.42f;
        public const float DefaultPhysicalImpactVfxLifetimeSeconds = 1.05f;

        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private Transform muzzleAnchor;
        [SerializeField] private CombatVfxCueId muzzleFlashCueId = CombatVfxCueId.PlayerRangedMuzzleFlash;
        [SerializeField, Min(0f)] private float muzzleFlashIntensity = 1f;
        [SerializeField, Min(0f)] private float muzzleFlashAudioIntensity = 1f;
        [SerializeField] private bool playImpactVfx = DefaultPlayImpactVfx;
        [SerializeField] private bool playImpactAudio = DefaultPlayImpactAudio;
        [SerializeField] private CombatVfxCueId impactCueId = DefaultImpactCueId;
        [SerializeField, Min(0f)] private float impactIntensity = DefaultImpactIntensity;
        [SerializeField, Min(0f)] private float impactAudioIntensity = DefaultImpactAudioIntensity;
        [SerializeField] private bool playPhysicalImpactVfx = DefaultPlayPhysicalImpactVfx;
        [SerializeField] private GameObject physicalImpactVfxPrefab;
        [SerializeField, Min(0f)] private float physicalImpactVfxScale = DefaultPhysicalImpactVfxScale;
        [SerializeField, Min(0f)] private float physicalImpactVfxLifetimeSeconds = DefaultPhysicalImpactVfxLifetimeSeconds;

        private readonly HashSet<LaneActionProjectile> watchedProjectiles = new HashSet<LaneActionProjectile>();
        private bool subscribed;
        private float lastCameraFireCueTime = float.NegativeInfinity;
        private int cameraFireCueRequestCount;

        public bool PlayImpactVfx => playImpactVfx;
        public bool PlayImpactAudio => playImpactAudio;
        public int CameraFireCueRequestCount => cameraFireCueRequestCount;

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

            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<ActionCameraController>();
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

            RequestCameraFireCue();

            Transform anchor = muzzleAnchor != null ? muzzleAnchor : rangedBasicAttackAction.transform;
            cuePlayer.PlayCue(
                muzzleFlashCueId,
                anchor,
                rangedBasicAttackAction.LastResolvedFireDirection,
                muzzleFlashIntensity,
                muzzleFlashAudioIntensity);
        }

        private void RequestCameraFireCue()
        {
            if (rangedBasicAttackAction == null)
            {
                return;
            }

            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<ActionCameraController>();
                if (cameraController == null)
                {
                    return;
                }
            }

            bool sustainedFire = Time.time - lastCameraFireCueTime
                <= Mathf.Max(0.01f, rangedBasicAttackAction.FireCooldownRemaining + 0.18f);
            lastCameraFireCueTime = Time.time;
            cameraFireCueRequestCount++;
            cameraController.RequestRifleFireFeedback(
                rangedBasicAttackAction.LastResolvedFireDirection,
                sustainedFire);
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
            if (projectile == null)
            {
                return;
            }

            PlayPhysicalImpactVfx(impactPoint, impactDirection);

            if (cuePlayer == null || (!playImpactVfx && (!playImpactAudio || impactAudioIntensity <= 0f)))
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

        private void PlayPhysicalImpactVfx(Vector3 impactPoint, Vector3 impactDirection)
        {
            if (!playPhysicalImpactVfx || physicalImpactVfxPrefab == null)
            {
                return;
            }

            Vector3 direction = impactDirection.sqrMagnitude > 0.0001f
                ? impactDirection.normalized
                : Vector3.forward;
            Quaternion rotation = Quaternion.LookRotation(-direction, Vector3.up);
            GameObject instance = Instantiate(physicalImpactVfxPrefab, impactPoint, rotation);
            instance.transform.localScale *= Mathf.Max(0f, physicalImpactVfxScale);

            ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Clear(withChildren: true);
                particleSystem.Play(withChildren: true);
            }

            Destroy(instance, Mathf.Max(0.05f, physicalImpactVfxLifetimeSeconds));
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
