using System;
using System.Collections.Generic;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SummonPressureScreen : MonoBehaviour
    {
        private readonly struct ColliderScreenBinding
        {
            public ColliderScreenBinding(Collider collider, SummonPressureScreen screen, int version)
            {
                Collider = collider;
                Screen = screen;
                Version = version;
            }

            public Collider Collider { get; }
            public SummonPressureScreen Screen { get; }
            public int Version { get; }
        }

        private static readonly List<SummonPressureScreen> ActiveScreens = new List<SummonPressureScreen>(16);
        private static readonly Dictionary<int, ColliderScreenBinding> ColliderScreenBindings = new(128);
        private static int colliderScreenBindingVersion;

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
        public static int CachedColliderBindingCount => ColliderScreenBindings.Count;

        public event Action<SummonPressureScreen> Activated;
        public event Action<SummonPressureScreen, BossBarrageProjectile> Intercepted;
        public event Action<SummonPressureScreen, LaneActionProjectile> ActionProjectileIntercepted;
        public event Action<SummonPressureScreen> SkillBeamIntercepted;
        public event Action<SummonPressureScreen> Deactivated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            ActiveScreens.Clear();
            ColliderScreenBindings.Clear();
            colliderScreenBindingVersion = 0;
        }

        public static SummonPressureScreen ResolveFromCollider(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            int id = collider.GetInstanceID();
            if (ColliderScreenBindings.TryGetValue(id, out ColliderScreenBinding binding)
                && binding.Collider == collider
                && binding.Version == colliderScreenBindingVersion)
            {
                return binding.Screen;
            }

            SummonPressureScreen screen = collider.GetComponentInParent<SummonPressureScreen>();
            ColliderScreenBindings[id] = new ColliderScreenBinding(
                collider,
                screen,
                colliderScreenBindingVersion);
            return screen;
        }

        private void Awake()
        {
            InvalidateColliderBindings();
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
            InvalidateColliderBindings();
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

            Vector3 incomingDirection = transform.position - projectile.transform.position;
            projectile.Deactivate();
            interceptedProjectiles++;
            RequestScreenBlockCamera(incomingDirection);
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

            Vector3 incomingDirection = transform.position - projectile.transform.position;
            projectile.Deactivate();
            interceptedProjectiles++;
            RequestScreenBlockCamera(incomingDirection);
            ActionProjectileIntercepted?.Invoke(this, projectile);
            if (interceptedProjectiles >= maxIntercepts)
            {
                Deactivate();
            }

            return true;
        }

        public bool TryInterceptSkillBeam(DamageTeam sourceTeam, Vector3 sourcePosition)
        {
            if (!active || !CombatTeamUtility.AreHostile(ownerTeam, sourceTeam))
            {
                return false;
            }

            interceptedProjectiles++;
            RequestScreenBlockCamera(transform.position - sourcePosition);
            SkillBeamIntercepted?.Invoke(this);
            if (interceptedProjectiles >= maxIntercepts)
            {
                Deactivate();
            }

            return true;
        }

        private void RequestScreenBlockCamera(Vector3 incomingDirection)
        {
            if (!CombatTeamUtility.IsPlayerSide(ownerTeam))
            {
                return;
            }

            ActionCameraController cameraController = ActionCameraController.ActiveInstance;
            cameraController?.RequestShieldBlockFeedback(incomingDirection, Mathf.Clamp01(activeTier / 3f + 0.35f));
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
            InvalidateColliderBindings();
            UnregisterActiveScreen();
        }

        private static void InvalidateColliderBindings()
        {
            unchecked
            {
                colliderScreenBindingVersion++;
            }
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

            return TryInterceptAnyOverlapping(
                ActiveScreens,
                projectile,
                impactPoint,
                extraRadius,
                pruneInactive: true);
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

                float radius = screen.ActiveRadius + Mathf.Max(0f, extraRadius);
                bool inRange = (screen.transform.position - impactPoint).sqrMagnitude <= radius * radius;
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

        public static bool TryInterceptAnySkillBeam(
            DamageTeam sourceTeam,
            Vector3 origin,
            Vector3 forward,
            Vector3 right,
            float maxDistance,
            float halfWidth,
            out int blockedBeamIndex,
            out float blockedBeamDistance)
        {
            blockedBeamIndex = -1;
            blockedBeamDistance = float.PositiveInfinity;
            Vector3 forwardDirection = ResolvePlanarDirection(forward, Vector3.forward);
            Vector3 rightDirection = ResolvePlanarDirection(right, Vector3.right);

            SummonPressureScreen closestScreen = null;
            for (int screenIndex = ActiveScreens.Count - 1; screenIndex >= 0; screenIndex--)
            {
                SummonPressureScreen screen = ActiveScreens[screenIndex];
                if (screen == null || !screen.IsActive)
                {
                    ActiveScreens.RemoveAt(screenIndex);
                    continue;
                }

                if (!CombatTeamUtility.AreHostile(screen.OwnerTeam, sourceTeam))
                {
                    continue;
                }

                Vector3 offset = Vector3.ProjectOnPlane(screen.transform.position - origin, Vector3.up);
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    Vector3 direction = directionIndex switch
                    {
                        0 => forwardDirection,
                        1 => rightDirection,
                        2 => -forwardDirection,
                        _ => -rightDirection
                    };
                    float forwardDistance = Vector3.Dot(offset, direction);
                    if (forwardDistance < 0f || forwardDistance > Mathf.Max(0f, maxDistance))
                    {
                        continue;
                    }

                    float interceptWidth = Mathf.Max(0f, halfWidth) + screen.ActiveRadius;
                    Vector3 lateralOffset = offset - direction * forwardDistance;
                    float blockDistance = Mathf.Max(0f, forwardDistance - screen.ActiveRadius);
                    if (lateralOffset.sqrMagnitude > interceptWidth * interceptWidth
                        || blockDistance >= blockedBeamDistance)
                    {
                        continue;
                    }

                    closestScreen = screen;
                    blockedBeamDistance = blockDistance;
                    blockedBeamIndex = directionIndex;
                }
            }

            if (closestScreen == null
                || !closestScreen.TryInterceptSkillBeam(sourceTeam, origin))
            {
                blockedBeamIndex = -1;
                blockedBeamDistance = float.PositiveInfinity;
                return false;
            }

            return true;
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction, Vector3 fallback)
        {
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            return planar.sqrMagnitude > 0.0001f ? planar.normalized : fallback;
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
