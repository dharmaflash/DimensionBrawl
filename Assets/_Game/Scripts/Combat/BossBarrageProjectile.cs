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

        private Collider triggerCollider;
        private Rigidbody projectileRigidbody;
        private MaterialPropertyBlock materialPropertyBlock;
        private Material[][] baseSharedMaterials = new Material[0][];
        private CombatHealth sourceHealth;
        private DamageTeam sourceTeam = DamageTeam.Enemy;
        private Vector3 travelDirection = Vector3.back;
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
        }

        public void Configure(
            CombatHealth newSourceHealth,
            DamageTeam newSourceTeam,
            float newDamage,
            Vector3 newTravelDirection,
            float newSpeed,
            float lifetimeSeconds,
            float radius)
        {
            EnsurePhysicsComponents();
            sourceHealth = newSourceHealth;
            sourceTeam = newSourceTeam;
            damage = Mathf.Max(0f, newDamage);
            travelDirection = ResolveDirection(newTravelDirection);
            speed = Mathf.Max(0f, newSpeed);
            remainingLifetime = Mathf.Max(0.01f, lifetimeSeconds);
            active = true;
            SetLastImpact(ProjectileImpactResult.None, null, null);

            if (triggerCollider is SphereCollider sphereCollider && radius > 0f)
            {
                sphereCollider.radius = radius / ResolvePresentationColliderScale();
            }

            gameObject.SetActive(true);
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
                0f);

            bool applied = targetHealth.TryApplyDamage(damageInfo);
            if (applied && deactivateOnHit)
            {
                Deactivate();
            }

            SetLastImpact(
                applied ? ProjectileImpactResult.AppliedDamage : ProjectileImpactResult.IgnoredDamageRejected,
                targetHealth,
                targetProxy);
            return applied;
        }

        public void Deactivate()
        {
            ResetPresentation();
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
            Tick(Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryApplyImpact(other, transform.position);
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
                visualRenderers = GetComponentsInChildren<Renderer>(true);
            }

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
