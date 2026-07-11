using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerRangedBasicVfxCueDriver : MonoBehaviour
    {
        private sealed class PhysicalImpactVfxInstance
        {
            public GameObject Root;
            public ParticleSystem[] Particles;
            public Vector3 BaseScale;
            public float ReleaseTime;
            public bool Active;
        }

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
        [SerializeField, Range(1, 16)] private int physicalImpactVfxPrewarmCount = 6;

        private readonly HashSet<LaneActionProjectile> watchedProjectiles = new HashSet<LaneActionProjectile>();
        private readonly List<PhysicalImpactVfxInstance> physicalImpactVfxPool =
            new List<PhysicalImpactVfxInstance>(8);
        private bool subscribed;
        private int activePhysicalImpactVfxCount;
        private Coroutine physicalImpactReleaseRoutine;
        private float lastCameraFireCueTime = float.NegativeInfinity;
        private int cameraFireCueRequestCount;

        public bool PlayImpactVfx => playImpactVfx;
        public bool PlayImpactAudio => playImpactAudio;
        public int CameraFireCueRequestCount => cameraFireCueRequestCount;
        public int PhysicalImpactVfxPoolSize => physicalImpactVfxPool.Count;
        public int ActivePhysicalImpactVfxCount => activePhysicalImpactVfxCount;

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
                cameraController = ActionCameraController.ActiveInstance;
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

        private void Start()
        {
            PrewarmPhysicalImpactVfx();
        }

        private void OnDisable()
        {
            StopPhysicalImpactReleaseRoutine();
            Unsubscribe();
            ReleaseAllPhysicalImpactVfx();
        }

        private void OnDestroy()
        {
            StopPhysicalImpactReleaseRoutine();
            for (int i = 0; i < physicalImpactVfxPool.Count; i++)
            {
                PhysicalImpactVfxInstance entry = physicalImpactVfxPool[i];
                if (entry?.Root != null)
                {
                    Destroy(entry.Root);
                }
            }

            physicalImpactVfxPool.Clear();
            activePhysicalImpactVfxCount = 0;
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
                cameraController = ActionCameraController.ActiveInstance;
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
            PhysicalImpactVfxInstance entry = AcquirePhysicalImpactVfx();
            if (entry == null || entry.Root == null)
            {
                return;
            }

            Transform instanceTransform = entry.Root.transform;
            instanceTransform.SetParent(null, worldPositionStays: false);
            instanceTransform.SetPositionAndRotation(impactPoint, rotation);
            instanceTransform.localScale = entry.BaseScale * Mathf.Max(0f, physicalImpactVfxScale);
            entry.Root.SetActive(true);
            RestartParticles(entry.Particles);
            entry.Active = true;
            activePhysicalImpactVfxCount++;
            entry.ReleaseTime = Time.time + Mathf.Max(0.05f, physicalImpactVfxLifetimeSeconds);
            StartPhysicalImpactReleaseRoutineIfNeeded();
        }

        private IEnumerator ReleasePhysicalImpactsUntilIdle()
        {
            yield return null;

            while (isActiveAndEnabled && activePhysicalImpactVfxCount > 0)
            {
                ReleaseExpiredPhysicalImpactVfx();
                if (activePhysicalImpactVfxCount <= 0)
                {
                    break;
                }

                yield return null;
            }

            physicalImpactReleaseRoutine = null;
        }

        private void StartPhysicalImpactReleaseRoutineIfNeeded()
        {
            if (physicalImpactReleaseRoutine == null
                && Application.isPlaying
                && isActiveAndEnabled
                && activePhysicalImpactVfxCount > 0)
            {
                physicalImpactReleaseRoutine = StartCoroutine(ReleasePhysicalImpactsUntilIdle());
            }
        }

        private void StopPhysicalImpactReleaseRoutine()
        {
            if (physicalImpactReleaseRoutine == null)
            {
                return;
            }

            StopCoroutine(physicalImpactReleaseRoutine);
            physicalImpactReleaseRoutine = null;
        }

        private void PrewarmPhysicalImpactVfx()
        {
            if (!playPhysicalImpactVfx || physicalImpactVfxPrefab == null)
            {
                return;
            }

            int targetCount = Mathf.Max(1, physicalImpactVfxPrewarmCount);
            while (physicalImpactVfxPool.Count < targetCount)
            {
                CreatePhysicalImpactVfxInstance();
            }
        }

        private PhysicalImpactVfxInstance AcquirePhysicalImpactVfx()
        {
            ReleaseExpiredPhysicalImpactVfx();
            for (int i = 0; i < physicalImpactVfxPool.Count; i++)
            {
                PhysicalImpactVfxInstance entry = physicalImpactVfxPool[i];
                if (entry != null && entry.Root != null && !entry.Active)
                {
                    return entry;
                }
            }

            return CreatePhysicalImpactVfxInstance();
        }

        private PhysicalImpactVfxInstance CreatePhysicalImpactVfxInstance()
        {
            if (physicalImpactVfxPrefab == null)
            {
                return null;
            }

            GameObject instance = Instantiate(physicalImpactVfxPrefab, transform);
            instance.name = $"{physicalImpactVfxPrefab.name}_Pooled_{physicalImpactVfxPool.Count:00}";
            PhysicalImpactVfxInstance entry = new PhysicalImpactVfxInstance
            {
                Root = instance,
                Particles = instance.GetComponentsInChildren<ParticleSystem>(includeInactive: true),
                BaseScale = instance.transform.localScale
            };
            instance.SetActive(false);
            physicalImpactVfxPool.Add(entry);
            return entry;
        }

        private void ReleaseExpiredPhysicalImpactVfx()
        {
            if (activePhysicalImpactVfxCount <= 0)
            {
                return;
            }

            float now = Time.time;
            for (int i = 0; i < physicalImpactVfxPool.Count; i++)
            {
                PhysicalImpactVfxInstance entry = physicalImpactVfxPool[i];
                if (entry != null && entry.Active && now >= entry.ReleaseTime)
                {
                    ReleasePhysicalImpactVfx(entry);
                }
            }
        }

        private void ReleaseAllPhysicalImpactVfx()
        {
            if (activePhysicalImpactVfxCount <= 0)
            {
                return;
            }

            for (int i = 0; i < physicalImpactVfxPool.Count; i++)
            {
                PhysicalImpactVfxInstance entry = physicalImpactVfxPool[i];
                if (entry != null && entry.Active)
                {
                    ReleasePhysicalImpactVfx(entry);
                }
            }
        }

        private void ReleasePhysicalImpactVfx(PhysicalImpactVfxInstance entry)
        {
            if (entry == null || !entry.Active)
            {
                return;
            }

            entry.Active = false;
            activePhysicalImpactVfxCount = Mathf.Max(0, activePhysicalImpactVfxCount - 1);
            entry.ReleaseTime = 0f;
            if (entry.Root == null)
            {
                return;
            }

            StopParticles(entry.Particles);
            entry.Root.SetActive(false);
            if (isActiveAndEnabled && gameObject.activeInHierarchy)
            {
                entry.Root.transform.SetParent(transform, worldPositionStays: false);
            }

            entry.Root.transform.localScale = entry.BaseScale;
        }

        private static void RestartParticles(ParticleSystem[] particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                ParticleSystem particle = particles[i];
                if (particle == null)
                {
                    continue;
                }

                particle.Clear(withChildren: true);
                particle.Play(withChildren: true);
            }
        }

        private static void StopParticles(ParticleSystem[] particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                ParticleSystem particle = particles[i];
                if (particle != null)
                {
                    particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
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
