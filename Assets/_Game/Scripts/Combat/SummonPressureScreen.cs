using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SummonPressureScreen : MonoBehaviour
    {
        [SerializeField] private DamageTeam ownerTeam = DamageTeam.AllySummon;
        [SerializeField, Min(0)] private int defaultMaxIntercepts = 2;
        [SerializeField, Min(0.05f)] private float defaultLifetimeSeconds = 1.2f;
        [SerializeField, Min(0.05f)] private float defaultRadius = 1.35f;

        private SphereCollider screenCollider;
        private Rigidbody screenRigidbody;
        private float remainingLifetime;
        private int maxIntercepts;
        private int interceptedProjectiles;
        private bool active;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public int InterceptedProjectiles => interceptedProjectiles;
        public int RemainingIntercepts => Mathf.Max(0, maxIntercepts - interceptedProjectiles);
        public DamageTeam OwnerTeam => ownerTeam;

        private void Awake()
        {
            EnsurePhysicsComponents();
            Deactivate();
        }

        public void Activate(
            DamageTeam newOwnerTeam,
            int newMaxIntercepts,
            float radius,
            float lifetimeSeconds)
        {
            EnsurePhysicsComponents();
            ownerTeam = newOwnerTeam;
            maxIntercepts = Mathf.Max(0, newMaxIntercepts);
            interceptedProjectiles = 0;
            remainingLifetime = Mathf.Max(0.05f, lifetimeSeconds);
            active = maxIntercepts > 0;

            screenCollider.radius = Mathf.Max(0.05f, radius);
            screenCollider.enabled = active;
        }

        public void ActivateDefault()
        {
            Activate(ownerTeam, defaultMaxIntercepts, defaultRadius, defaultLifetimeSeconds);
        }

        public void Tick(float deltaTime)
        {
            if (!active || deltaTime <= 0f)
            {
                return;
            }

            remainingLifetime -= deltaTime;
            if (remainingLifetime <= 0f)
            {
                Deactivate();
            }
        }

        public bool TryIntercept(BossBarrageProjectile projectile)
        {
            if (!active
                || projectile == null
                || !projectile.IsActive
                || !CombatTeamUtility.AreHostile(ownerTeam, projectile.SourceTeam))
            {
                return false;
            }

            projectile.Deactivate();
            interceptedProjectiles++;
            if (interceptedProjectiles >= maxIntercepts)
            {
                Deactivate();
            }

            return true;
        }

        public void Deactivate()
        {
            active = false;
            remainingLifetime = 0f;
            if (screenCollider != null)
            {
                screenCollider.enabled = false;
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return;
            }

            TryIntercept(other.GetComponentInParent<BossBarrageProjectile>());
        }

        private void EnsurePhysicsComponents()
        {
            if (screenCollider == null)
            {
                screenCollider = GetComponent<SphereCollider>();
            }

            if (screenCollider != null)
            {
                screenCollider.isTrigger = true;
            }

            if (screenRigidbody == null)
            {
                screenRigidbody = GetComponent<Rigidbody>();
            }

            if (screenRigidbody != null)
            {
                screenRigidbody.useGravity = false;
                screenRigidbody.isKinematic = true;
            }
        }
    }
}
