using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BossBarrageProjectile : MonoBehaviour
    {
        [SerializeField] private bool deactivateOnHit = true;

        private Collider triggerCollider;
        private Rigidbody projectileRigidbody;
        private CombatHealth sourceHealth;
        private DamageTeam sourceTeam = DamageTeam.Enemy;
        private Vector3 travelDirection = Vector3.back;
        private float damage;
        private float speed;
        private float remainingLifetime;
        private bool active;
        private ProjectileImpactResult lastImpactResult = ProjectileImpactResult.None;
        private CombatHealth lastImpactTargetHealth;
        private SummonFrontlineProxy lastImpactTargetProxy;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public DamageTeam SourceTeam => sourceTeam;
        public ProjectileImpactResult LastImpactResult => lastImpactResult;
        public CombatHealth LastImpactTargetHealth => lastImpactTargetHealth;
        public SummonFrontlineProxy LastImpactTargetProxy => lastImpactTargetProxy;

        private void Awake()
        {
            EnsurePhysicsComponents();
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
                sphereCollider.radius = radius;
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
