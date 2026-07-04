using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BossBarrageProjectile : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private bool deactivateOnHit = true;
        [SerializeField] private Renderer[] visualRenderers = new Renderer[0];
        [SerializeField] private TrailRenderer[] trailRenderers = new TrailRenderer[0];
        [SerializeField] private AudioClip impactSfx;
        [SerializeField, Range(0f, 1f)] private float impactSfxVolume = 0.42f;
        [SerializeField] private Vector2 impactSfxPitchRange = new Vector2(0.96f, 1.04f);

        private Collider triggerCollider;
        private Rigidbody projectileRigidbody;
        private MaterialPropertyBlock materialPropertyBlock;
        private Material[][] baseSharedMaterials = new Material[0][];
        private ParticleSystem[] visualParticleSystems = System.Array.Empty<ParticleSystem>();
        private AudioSource[] audioSources = System.Array.Empty<AudioSource>();
        private CombatHealth sourceHealth;
        private DamageTeam sourceTeam = DamageTeam.Enemy;
        private Vector3 travelDirection = Vector3.back;
        private DamageResponsePolicy responsePolicy = DamageResponsePolicy.FlashOnly;
        private CombatControlLockPolicy controlLockPolicy = CombatControlLockPolicy.None;
        private Vector3 baseLocalScale = Vector3.one;
        private Vector3 lastPresentationScale = Vector3.one;
        private Color lastPresentationColor = Color.white;
        private Material lastPresentationMaterial;
        private float damage;
        private float speed;
        private float remainingLifetime;
        private bool active;
        private bool presentationInitialized;
        private ProjectileImpactResult lastImpactResult = ProjectileImpactResult.None;
        private CombatHealth lastImpactTargetHealth;
        private SummonFrontlineProxy lastImpactTargetProxy;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public DamageTeam SourceTeam => sourceTeam;
        public DamageResponsePolicy ResponsePolicy => responsePolicy;
        public CombatControlLockPolicy ControlLockPolicy => controlLockPolicy;
        public ProjectileImpactResult LastImpactResult => lastImpactResult;
        public CombatHealth LastImpactTargetHealth => lastImpactTargetHealth;
        public SummonFrontlineProxy LastImpactTargetProxy => lastImpactTargetProxy;
        public Vector3 LastPresentationScale => lastPresentationScale;
        public Color LastPresentationColor => lastPresentationColor;
        public Material LastPresentationMaterial => lastPresentationMaterial;

        private void Awake()
        {
            EnsurePhysicsComponents();
            EnsurePresentationComponents();
        }

        public void ApplyPresentation(Color color, Vector3 visualScale, Material visualMaterial)
        {
            EnsurePresentationComponents();
            lastPresentationColor = color;
            lastPresentationScale = SanitizeVisualScale(visualScale);
            lastPresentationMaterial = visualMaterial;
            transform.localScale = Vector3.Scale(baseLocalScale, lastPresentationScale);
            ApplySharedMaterial(visualMaterial);
            ApplyColor(lastPresentationColor);
            ApplyTrailPresentation(lastPresentationColor);
        }

        public void Configure(
            CombatHealth newSourceHealth,
            DamageTeam newSourceTeam,
            float newDamage,
            Vector3 newTravelDirection,
            float newSpeed,
            float lifetimeSeconds,
            float radius,
            DamageResponsePolicy newResponsePolicy = DamageResponsePolicy.FlashOnly,
            CombatControlLockPolicy newControlLockPolicy = CombatControlLockPolicy.None)
        {
            EnsurePhysicsComponents();
            sourceHealth = newSourceHealth;
            sourceTeam = newSourceTeam;
            ConfigureDamagePolicy(newResponsePolicy, newControlLockPolicy);
            damage = Mathf.Max(0f, newDamage);
            travelDirection = ResolveDirection(newTravelDirection);
            if (travelDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(travelDirection, Vector3.up);
            }

            speed = Mathf.Max(0f, newSpeed);
            remainingLifetime = Mathf.Max(0.01f, lifetimeSeconds);
            active = true;
            SetLastImpact(ProjectileImpactResult.None, null, null);

            if (triggerCollider is SphereCollider sphereCollider && radius > 0f)
            {
                sphereCollider.radius = radius / ResolvePresentationColliderScale();
            }

            gameObject.SetActive(true);
            ResetTrailRenderers();
            RestartVisualParticleSystems();
            RestartAudioSources();
        }

        public void ConfigureDamagePolicy(
            DamageResponsePolicy newResponsePolicy,
            CombatControlLockPolicy newControlLockPolicy)
        {
            responsePolicy = newResponsePolicy;
            controlLockPolicy = newControlLockPolicy;
        }

        public void Tick(float deltaTime)
        {
            if (!active || deltaTime <= 0f)
            {
                return;
            }

            if (sourceHealth != null && sourceHealth.IsAlive == false)
            {
                Deactivate();
                return;
            }

            transform.position += travelDirection * speed * deltaTime;
            if (SummonPressureScreen.TryInterceptAnyOverlapping(this, transform.position, ResolveWorldColliderRadius()))
            {
                return;
            }

            remainingLifetime -= deltaTime;
            if (remainingLifetime <= 0f)
            {
                Deactivate();
            }
        }

        public bool TryApplyImpact(Collider hitCollider, Vector3 impactPoint)
        {
            if (!active || hitCollider == null)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredInactive, null, null);
                return false;
            }

            if (hitCollider.GetComponentInParent<SummonPressureScreen>() != null)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredPressureScreen, null, null);
                return false;
            }

            if (SummonPressureScreen.TryInterceptAnyOverlapping(this, impactPoint, ResolveWorldColliderRadius()))
            {
                return true;
            }

            SummonFrontlineProxy targetProxy = hitCollider.GetComponentInParent<SummonFrontlineProxy>();
            if (targetProxy != null && !targetProxy.IsActive)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredInactiveSummon, null, targetProxy);
                return false;
            }

            CombatHealth targetHealth = targetProxy != null
                ? targetProxy.Health ?? hitCollider.GetComponentInParent<CombatHealth>()
                : hitCollider.GetComponentInParent<CombatHealth>();
            if (targetHealth == null)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredMissingHealth, null, targetProxy);
                return false;
            }

            if (targetHealth == sourceHealth)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredSelf, targetHealth, targetProxy);
                return false;
            }

            if (!targetHealth.IsAlive)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredDeadTarget, targetHealth, targetProxy);
                return false;
            }

            if (!CombatTeamUtility.AreHostile(sourceTeam, targetHealth.Team))
            {
                SetLastImpact(ProjectileImpactResult.IgnoredNonHostile, targetHealth, targetProxy);
                return false;
            }

            DamageInfo damageInfo = new DamageInfo(
                sourceHealth,
                sourceTeam,
                damage,
                impactPoint,
                travelDirection,
                0f,
                responsePolicy,
                controlLockPolicy);

            bool blockedByInvulnerability = targetHealth.IsInvulnerable;
            bool applied = targetHealth.TryApplyDamage(damageInfo);
            if (applied && deactivateOnHit)
            {
                PlayImpactSfx(impactPoint);
                Deactivate();
            }
            else if (blockedByInvulnerability && deactivateOnHit)
            {
                Deactivate();
            }

            SetLastImpact(
                applied ? ProjectileImpactResult.AppliedDamage : ProjectileImpactResult.IgnoredDamageRejected,
                targetHealth,
                targetProxy);
            return applied || blockedByInvulnerability;
        }

        public void Deactivate()
        {
            ResetPresentation();
            StopTrailRenderers();
            StopVisualParticleSystems();
            StopAudioSources();
            active = false;
            remainingLifetime = 0f;
            gameObject.SetActive(false);
        }

        private void SetLastImpact(
            ProjectileImpactResult result,
            CombatHealth targetHealth,
            SummonFrontlineProxy targetProxy)
        {
            lastImpactResult = result;
            lastImpactTargetHealth = targetHealth;
            lastImpactTargetProxy = targetProxy;
        }

        private void Update()
        {
            Tick(Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this));
        }

        private void OnTriggerEnter(Collider other)
        {
            TryApplyImpact(other, transform.position);
        }

        private float ResolveWorldColliderRadius()
        {
            if (triggerCollider is SphereCollider sphereCollider)
            {
                Vector3 scale = sphereCollider.transform.lossyScale;
                float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                return sphereCollider.radius * Mathf.Max(0.05f, maxScale);
            }

            return 0f;
        }

        private void EnsurePhysicsComponents()
        {
            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<Collider>();
            }

            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }

            if (projectileRigidbody == null)
            {
                projectileRigidbody = GetComponent<Rigidbody>();
            }

            if (projectileRigidbody != null)
            {
                projectileRigidbody.useGravity = false;
                projectileRigidbody.isKinematic = true;
            }
        }

        private void EnsurePresentationComponents()
        {
            if (presentationInitialized)
            {
                return;
            }

            presentationInitialized = true;
            baseLocalScale = transform.localScale;
            if (visualRenderers == null || visualRenderers.Length == 0)
            {
                visualRenderers = ResolveNonTrailRenderers();
            }

            if (trailRenderers == null || trailRenderers.Length == 0)
            {
                trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
            }

            visualParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
            audioSources = GetComponentsInChildren<AudioSource>(true);

            materialPropertyBlock = new MaterialPropertyBlock();
            baseSharedMaterials = new Material[visualRenderers.Length][];
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                baseSharedMaterials[i] = visualRenderers[i] != null
                    ? (Material[])visualRenderers[i].sharedMaterials.Clone()
                    : new Material[0];
            }
        }

        private void ApplySharedMaterial(Material visualMaterial)
        {
            if (visualRenderers == null)
            {
                return;
            }

            if (visualMaterial == null)
            {
                RestoreBaseSharedMaterials();
                return;
            }

            for (int i = 0; i < visualRenderers.Length; i++)
            {
                Renderer visualRenderer = visualRenderers[i];
                if (visualRenderer == null)
                {
                    continue;
                }

                Material[] materials = visualRenderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = visualMaterial;
                }

                visualRenderer.sharedMaterials = materials;
            }
        }

        private void RestoreBaseSharedMaterials()
        {
            if (visualRenderers == null || baseSharedMaterials == null)
            {
                return;
            }

            int count = Mathf.Min(visualRenderers.Length, baseSharedMaterials.Length);
            for (int i = 0; i < count; i++)
            {
                if (visualRenderers[i] != null && baseSharedMaterials[i] != null && baseSharedMaterials[i].Length > 0)
                {
                    visualRenderers[i].sharedMaterials = (Material[])baseSharedMaterials[i].Clone();
                }
            }
        }

        private void ApplyColor(Color color)
        {
            if (visualRenderers == null)
            {
                return;
            }

            for (int i = 0; i < visualRenderers.Length; i++)
            {
                Renderer visualRenderer = visualRenderers[i];
                if (visualRenderer == null)
                {
                    continue;
                }

                visualRenderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor(BaseColorId, color);
                materialPropertyBlock.SetColor(ColorId, color);
                materialPropertyBlock.SetColor(EmissionColorId, color * 1.35f);
                visualRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        private void ResetPresentation()
        {
            EnsurePresentationComponents();
            lastPresentationScale = Vector3.one;
            lastPresentationColor = Color.white;
            lastPresentationMaterial = null;
            transform.localScale = baseLocalScale;
            RestoreBaseSharedMaterials();
            ClearColorOverrides();
        }

        private void ApplyTrailPresentation(Color color)
        {
            if (trailRenderers == null)
            {
                return;
            }

            Color head = color;
            head.a = Mathf.Max(head.a, 0.75f);
            Color tail = color;
            tail.a = 0f;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(head, 0f),
                    new GradientColorKey(tail, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(head.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                });

            for (int i = 0; i < trailRenderers.Length; i++)
            {
                TrailRenderer trail = trailRenderers[i];
                if (trail == null)
                {
                    continue;
                }

                trail.colorGradient = gradient;
            }
        }

        private void ResetTrailRenderers()
        {
            if (trailRenderers == null)
            {
                return;
            }

            for (int i = 0; i < trailRenderers.Length; i++)
            {
                TrailRenderer trail = trailRenderers[i];
                if (trail == null)
                {
                    continue;
                }

                trail.emitting = true;
                trail.Clear();
            }
        }

        private void StopTrailRenderers()
        {
            if (trailRenderers == null)
            {
                return;
            }

            for (int i = 0; i < trailRenderers.Length; i++)
            {
                TrailRenderer trail = trailRenderers[i];
                if (trail == null)
                {
                    continue;
                }

                trail.emitting = false;
                trail.Clear();
            }
        }

        private void RestartVisualParticleSystems()
        {
            EnsurePresentationComponents();
            if (visualParticleSystems == null)
            {
                return;
            }

            for (int i = 0; i < visualParticleSystems.Length; i++)
            {
                ParticleSystem particleSystem = visualParticleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Clear(withChildren: true);
                particleSystem.Play(withChildren: true);
            }
        }

        private void StopVisualParticleSystems()
        {
            EnsurePresentationComponents();
            if (visualParticleSystems == null)
            {
                return;
            }

            for (int i = 0; i < visualParticleSystems.Length; i++)
            {
                ParticleSystem particleSystem = visualParticleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void RestartAudioSources()
        {
            EnsurePresentationComponents();
            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];
                if (audioSource == null || audioSource.clip == null || !audioSource.enabled)
                {
                    continue;
                }

                audioSource.Stop();
                audioSource.Play();
            }
        }

        private void StopAudioSources()
        {
            EnsurePresentationComponents();
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i] != null)
                {
                    audioSources[i].Stop();
                }
            }
        }

        private void PlayImpactSfx(Vector3 impactPoint)
        {
            if (impactSfx == null || impactSfxVolume <= 0f)
            {
                return;
            }

            GameObject audioObject = new GameObject("BossBarrageProjectileImpactAudio");
            audioObject.transform.position = impactPoint;
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = impactSfx;
            source.playOnAwake = false;
            source.loop = false;
            source.volume = Mathf.Clamp01(impactSfxVolume);
            source.pitch = Random.Range(
                Mathf.Min(impactSfxPitchRange.x, impactSfxPitchRange.y),
                Mathf.Max(impactSfxPitchRange.x, impactSfxPitchRange.y));
            source.spatialBlend = 0.62f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 3f;
            source.maxDistance = 28f;
            source.priority = 136;
            source.Play();
            Destroy(audioObject, impactSfx.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)) + 0.1f);
        }

        private void ClearColorOverrides()
        {
            if (visualRenderers == null)
            {
                return;
            }

            for (int i = 0; i < visualRenderers.Length; i++)
            {
                if (visualRenderers[i] != null)
                {
                    visualRenderers[i].SetPropertyBlock(null);
                }
            }
        }

        private float ResolvePresentationColliderScale()
        {
            return Mathf.Max(
                0.05f,
                Mathf.Max(lastPresentationScale.x, Mathf.Max(lastPresentationScale.y, lastPresentationScale.z)));
        }

        private static Vector3 SanitizeVisualScale(Vector3 scale)
        {
            return new Vector3(
                Mathf.Max(0.05f, scale.x),
                Mathf.Max(0.05f, scale.y),
                Mathf.Max(0.05f, scale.z));
        }

        private Renderer[] ResolveNonTrailRenderers()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            int count = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled && renderers[i] is not TrailRenderer)
                {
                    count++;
                }
            }

            Renderer[] filtered = new Renderer[count];
            int writeIndex = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || renderer is TrailRenderer)
                {
                    continue;
                }

                filtered[writeIndex] = renderer;
                writeIndex++;
            }

            return filtered;
        }

        private static Vector3 ResolveDirection(Vector3 direction)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            return Vector3.back;
        }
    }
}
