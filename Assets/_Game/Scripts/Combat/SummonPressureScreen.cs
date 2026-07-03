using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SummonPressureScreen : MonoBehaviour
    {
        private static readonly List<SummonPressureScreen> ActiveScreens = new List<SummonPressureScreen>(16);

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
        private int activeTier = 1;
        private bool active;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public int InterceptedProjectiles => interceptedProjectiles;
        public int RemainingIntercepts => Mathf.Max(0, maxIntercepts - interceptedProjectiles);
        public int MaxIntercepts => maxIntercepts;
        public int ActiveTier => activeTier;
        public float RemainingLifetimeSeconds => remainingLifetime;
        public float ActiveRadius => activeRadius > 0f ? activeRadius : defaultRadius;
        public DamageTeam OwnerTeam => ownerTeam;

        public event Action<SummonPressureScreen> Activated;
        public event Action<SummonPressureScreen, BossBarrageProjectile> Intercepted;
        public event Action<SummonPressureScreen, LaneActionProjectile> ActionProjectileIntercepted;
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
            Activate(newOwnerTeam, newMaxIntercepts, radius, lifetimeSeconds, 1);
        }

        public void Activate(
            DamageTeam newOwnerTeam,
            int newMaxIntercepts,
            float radius,
            float lifetimeSeconds,
            int tier)
        {
            EnsurePhysicsComponents();
            ownerTeam = newOwnerTeam;
            maxIntercepts = Mathf.Max(0, newMaxIntercepts);
            interceptedProjectiles = 0;
            remainingLifetime = Mathf.Max(0.05f, lifetimeSeconds);
            overlapScanTimer = 0f;
            activeRadius = Mathf.Max(0.05f, radius);
            activeTier = Mathf.Clamp(tier, 1, 3);
            active = maxIntercepts > 0;

            screenCollider.radius = activeRadius;
            screenCollider.enabled = active;
            if (active)
            {
                RegisterActiveScreen();
                Activated?.Invoke(this);
                ScanForOverlappingProjectiles();
            }
            else
            {
                UnregisterActiveScreen();
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

        public bool TryIntercept(LaneActionProjectile projectile)
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
            ActionProjectileIntercepted?.Invoke(this, projectile);
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
            UnregisterActiveScreen();
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
            Tick(Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this));
        }

        private void OnDisable()
        {
            UnregisterActiveScreen();
        }

        private void OnDestroy()
        {
            UnregisterActiveScreen();
        }

        public static bool TryInterceptAnyOverlapping(
            BossBarrageProjectile projectile,
            Vector3 impactPoint,
            float extraRadius = 0f)
        {
            if (projectile == null || !projectile.IsActive)
            {
                return false;
            }

            if (TryInterceptAnyOverlapping(
                ActiveScreens,
                projectile,
                impactPoint,
                extraRadius,
                pruneInactive: true))
            {
                return true;
            }

            SummonPressureScreen[] sceneScreens = FindObjectsByType<SummonPressureScreen>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            return TryInterceptAnyOverlapping(
                sceneScreens,
                projectile,
                impactPoint,
                extraRadius,
                pruneInactive: false);
        }

        private static bool TryInterceptAnyOverlapping(
            IList<SummonPressureScreen> screens,
            BossBarrageProjectile projectile,
            Vector3 impactPoint,
            float extraRadius,
            bool pruneInactive)
        {
            for (int i = screens.Count - 1; i >= 0; i--)
            {
                SummonPressureScreen screen = screens[i];
                if (screen == null || !screen.IsActive)
                {
                    if (pruneInactive)
                    {
                        screens.RemoveAt(i);
                    }

                    continue;
                }

                if (!CombatTeamUtility.AreHostile(screen.OwnerTeam, projectile.SourceTeam))
                {
                    continue;
                }

                float distance = Vector3.Distance(screen.transform.position, impactPoint);
                float radius = screen.ActiveRadius + Mathf.Max(0f, extraRadius);
                bool inRange = distance * distance <= radius * radius;
                if (!inRange)
                {
                    continue;
                }

                if (screen.TryIntercept(projectile))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return;
            }

            if (TryIntercept(other.GetComponentInParent<BossBarrageProjectile>()))
            {
                return;
            }

            TryIntercept(other.GetComponentInParent<LaneActionProjectile>());
        }

        private void OnTriggerStay(Collider other)
        {
            if (other == null)
            {
                return;
            }

            if (TryIntercept(other.GetComponentInParent<BossBarrageProjectile>()))
            {
                return;
            }

            TryIntercept(other.GetComponentInParent<LaneActionProjectile>());
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

                if (TryIntercept(candidate.GetComponentInParent<BossBarrageProjectile>()))
                {
                    continue;
                }

                TryIntercept(candidate.GetComponentInParent<LaneActionProjectile>());
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

        private void RegisterActiveScreen()
        {
            if (!ActiveScreens.Contains(this))
            {
                ActiveScreens.Add(this);
            }
        }

        private void UnregisterActiveScreen()
        {
            ActiveScreens.Remove(this);
        }
    }
}
