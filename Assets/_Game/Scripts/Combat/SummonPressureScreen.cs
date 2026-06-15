using System;
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
        [SerializeField, Min(0f)] private float overlapScanIntervalSeconds = 0.04f;
        [SerializeField, Min(1)] private int overlapBufferSize = 16;
        [SerializeField] private LayerMask interceptLayers = ~0;

        private SphereCollider screenCollider;
        private Rigidbody screenRigidbody;
        private Collider[] overlapBuffer;
        private float remainingLifetime;
        private float overlapScanTimer;
        private float activeRadius;
        private int maxIntercepts;
        private int interceptedProjectiles;
        private bool active;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public int InterceptedProjectiles => interceptedProjectiles;
        public int RemainingIntercepts => Mathf.Max(0, maxIntercepts - interceptedProjectiles);
        public int MaxIntercepts => maxIntercepts;
        public float RemainingLifetimeSeconds => remainingLifetime;
        public float ActiveRadius => activeRadius > 0f ? activeRadius : defaultRadius;
        public DamageTeam OwnerTeam => ownerTeam;

        public event Action<SummonPressureScreen> Activated;
        public event Action<SummonPressureScreen, BossBarrageProjectile> Intercepted;
        public event Action<SummonPressureScreen> Deactivated;

        private void Awake()
        {
            EnsurePhysicsComponents();
            EnsureOverlapBuffer();
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
            overlapScanTimer = 0f;
            activeRadius = Mathf.Max(0.05f, radius);
            active = maxIntercepts > 0;

            screenCollider.radius = activeRadius;
            screenCollider.enabled = active;
            if (active)
            {
                Activated?.Invoke(this);
                ScanForOverlappingProjectiles();
            }
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
                return;
            }

            overlapScanTimer -= deltaTime;
            if (overlapScanTimer <= 0f)
            {
                overlapScanTimer = Mathf.Max(0f, overlapScanIntervalSeconds);
                ScanForOverlappingProjectiles();
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
            Intercepted?.Invoke(this, projectile);
            if (interceptedProjectiles >= maxIntercepts)
            {
                Deactivate();
            }

            return true;
        }

        public void Deactivate()
        {
            bool wasActive = active;
            active = false;
            remainingLifetime = 0f;
            if (screenCollider != null)
            {
                screenCollider.enabled = false;
            }

            if (wasActive)
            {
                Deactivated?.Invoke(this);
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

        private void OnTriggerStay(Collider other)
        {
            if (other == null)
            {
                return;
            }

            TryIntercept(other.GetComponentInParent<BossBarrageProjectile>());
        }

        private void ScanForOverlappingProjectiles()
        {
            if (!active)
            {
                return;
            }

            EnsureOverlapBuffer();
            int overlapCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                ActiveRadius,
                overlapBuffer,
                interceptLayers,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < overlapCount && active; i++)
            {
                Collider candidate = overlapBuffer[i];
                if (candidate == null || candidate == screenCollider)
                {
                    continue;
                }

                TryIntercept(candidate.GetComponentInParent<BossBarrageProjectile>());
            }
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

        private void EnsureOverlapBuffer()
        {
            int safeSize = Mathf.Max(1, overlapBufferSize);
            if (overlapBuffer == null || overlapBuffer.Length != safeSize)
            {
                overlapBuffer = new Collider[safeSize];
            }
        }
    }
}
