using System;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum ProjectileImpactResult
    {
        None = 0,
        IgnoredInactive = 1,
        IgnoredPressureScreen = 2,
        IgnoredMissingHealth = 3,
        IgnoredSelf = 4,
        IgnoredInactiveSummon = 5,
        IgnoredDeadTarget = 6,
        IgnoredNonHostile = 7,
        AppliedDamage = 8,
        IgnoredDamageRejected = 9
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class LaneActionProjectile : MonoBehaviour
    {
        [SerializeField] private bool deactivateOnHit = true;
        [SerializeField] private bool alignVisualToDirection = true;
        [SerializeField] private bool allowVerticalTravel;

        private readonly RaycastHit[] sweepHits = new RaycastHit[16];
        private readonly Collider[] overlapHits = new Collider[16];
        private Collider triggerCollider;
        private Rigidbody projectileRigidbody;
        private CombatHealth sourceHealth;
        private DamageTeam sourceTeam = DamageTeam.Player;
        private Vector3 travelDirection = Vector3.forward;
        private DamageResponsePolicy responsePolicy = DamageResponsePolicy.FlashOnly;
        private CombatControlLockPolicy controlLockPolicy = CombatControlLockPolicy.None;
        private float damage;
        private float hitStopSeconds;
        private float speed;
        private float remainingLifetime;
        private bool active;
        private ProjectileImpactResult lastImpactResult = ProjectileImpactResult.None;
        private CombatHealth lastImpactTargetHealth;
        private SummonFrontlineProxy lastImpactTargetProxy;
        private AudioSource[] audioSources = Array.Empty<AudioSource>();
        private bool audioSourcesResolved;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public CombatHealth SourceHealth => sourceHealth;
        public DamageTeam SourceTeam => sourceTeam;
        public float Damage => damage;
        public float HitStopSeconds => hitStopSeconds;
        public float RemainingLifetimeSeconds => remainingLifetime;
        public Vector3 TravelDirection => travelDirection;
        public bool AllowsVerticalTravel => allowVerticalTravel;
        public DamageResponsePolicy ResponsePolicy => responsePolicy;
        public CombatControlLockPolicy ControlLockPolicy => controlLockPolicy;
        public ProjectileImpactResult LastImpactResult => lastImpactResult;
        public CombatHealth LastImpactTargetHealth => lastImpactTargetHealth;
        public SummonFrontlineProxy LastImpactTargetProxy => lastImpactTargetProxy;

        public event Action<LaneActionProjectile, CombatHealth, Vector3, Vector3> DamageApplied;

        private void Awake()
        {
            CombatTimeDilationReceiver.Ensure(gameObject);
            EnsurePhysicsComponents();
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
            CombatControlLockPolicy newControlLockPolicy = CombatControlLockPolicy.None,
            float newHitStopSeconds = 0f)
        {
            EnsurePhysicsComponents();
            sourceHealth = newSourceHealth;
            sourceTeam = newSourceTeam;
            ConfigureDamagePolicy(newResponsePolicy, newControlLockPolicy);
            damage = Mathf.Max(0f, newDamage);
            hitStopSeconds = Mathf.Max(0f, newHitStopSeconds);
            travelDirection = ResolveDirection(newTravelDirection);
            speed = Mathf.Max(0f, newSpeed);
            remainingLifetime = Mathf.Max(0.01f, lifetimeSeconds);
            active = true;
            SetLastImpact(ProjectileImpactResult.None, null, null);

            if (triggerCollider is SphereCollider sphereCollider && radius > 0f)
            {
                sphereCollider.radius = radius;
            }

            if (alignVisualToDirection)
            {
                transform.rotation = Quaternion.LookRotation(travelDirection, Vector3.up);
            }

            ResetTrailRenderers();
            gameObject.SetActive(true);
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

            Vector3 startPosition = transform.position;
            Vector3 travelDelta = travelDirection * speed * deltaTime;
            if (TryApplyTravelImpacts(startPosition, travelDelta))
            {
                return;
            }

            transform.position = startPosition + travelDelta;
            remainingLifetime -= deltaTime;
            if (remainingLifetime <= 0f)
            {
                Deactivate();
            }
        }

        private bool TryApplyTravelImpacts(Vector3 startPosition, Vector3 travelDelta)
        {
            float radius = ResolveCurrentRadius();
            float distance = travelDelta.magnitude;
            if (distance > 0.0001f)
            {
                Vector3 direction = travelDelta / distance;
                int hitCount = Physics.SphereCastNonAlloc(
                    startPosition,
                    radius,
                    direction,
                    sweepHits,
                    distance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Collide);
                SortSweepHitsByDistance(hitCount);
                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = sweepHits[i];
                    if (hit.collider == null
                        || hit.collider.transform == transform
                        || hit.collider.transform.IsChildOf(transform))
                    {
                        continue;
                    }

                    transform.position = hit.point;
                    if (TryApplyImpact(hit.collider, hit.point) && !active)
                    {
                        return true;
                    }
                }
            }

            Vector3 endPosition = startPosition + travelDelta;
            transform.position = endPosition;
            if (SummonPressureScreen.TryInterceptAnyOverlapping(this, endPosition, radius))
            {
                return true;
            }

            int overlapCount = Physics.OverlapSphereNonAlloc(
                endPosition,
                radius,
                overlapHits,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);
            for (int i = 0; i < overlapCount; i++)
            {
                Collider hitCollider = overlapHits[i];
                if (hitCollider != null && TryApplyImpact(hitCollider, endPosition) && !active)
                {
                    return true;
                }
            }

            return !active;
        }

        private void SortSweepHitsByDistance(int hitCount)
        {
            for (int i = 0; i < hitCount - 1; i++)
            {
                int nearestIndex = i;
                for (int j = i + 1; j < hitCount; j++)
                {
                    if (sweepHits[j].distance < sweepHits[nearestIndex].distance)
                    {
                        nearestIndex = j;
                    }
                }

                if (nearestIndex == i)
                {
                    continue;
                }

                RaycastHit swap = sweepHits[i];
                sweepHits[i] = sweepHits[nearestIndex];
                sweepHits[nearestIndex] = swap;
            }
        }

        private float ResolveCurrentRadius()
        {
            if (triggerCollider is SphereCollider sphereCollider)
            {
                Vector3 lossyScale = sphereCollider.transform.lossyScale;
                float maxScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
                return Mathf.Max(0.01f, sphereCollider.radius * maxScale);
            }

            return 0.1f;
        }

        public bool TryApplyImpact(Collider hitCollider, Vector3 impactPoint)
        {
            if (!active || hitCollider == null)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredInactive, null, null);
                return false;
            }

            SummonPressureScreen hitPressureScreen = SummonPressureScreen.ResolveFromCollider(hitCollider);
            if (hitPressureScreen != null)
            {
                if (hitPressureScreen.TryIntercept(this))
                {
                    return true;
                }

                SetLastImpact(ProjectileImpactResult.IgnoredPressureScreen, null, null);
                return false;
            }

            if (SummonPressureScreen.TryInterceptAnyOverlapping(
                    this,
                    impactPoint,
                    ResolveCurrentRadius()))
            {
                return true;
            }

            SummonFrontlineProxy targetProxy = SummonFrontlineProxy.ResolveFromCollider(hitCollider);
            if (targetProxy != null && !targetProxy.IsActive)
            {
                SetLastImpact(ProjectileImpactResult.IgnoredInactiveSummon, null, targetProxy);
                return false;
            }

            CombatHealth targetHealth = targetProxy != null
                ? targetProxy.Health ?? CombatHealth.ResolveFromCollider(hitCollider)
                : CombatHealth.ResolveFromCollider(hitCollider);
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
                hitStopSeconds,
                responsePolicy,
                controlLockPolicy);

            bool applied = targetHealth.TryApplyDamage(damageInfo);
            SetLastImpact(
                applied ? ProjectileImpactResult.AppliedDamage : ProjectileImpactResult.IgnoredDamageRejected,
                targetHealth,
                targetProxy);

            if (applied)
            {
                RequestPlayerSideHeavyShotCamera(targetHealth, impactPoint);
                DamageApplied?.Invoke(this, targetHealth, impactPoint, travelDirection);
                if (deactivateOnHit)
                {
                    Deactivate();
                }
            }

            return applied;
        }

        private void RequestPlayerSideHeavyShotCamera(CombatHealth targetHealth, Vector3 impactPoint)
        {
            if (!CombatTeamUtility.IsPlayerSide(sourceTeam)
                || targetHealth == null
                || CombatTeamUtility.IsPlayerSide(targetHealth.Team)
                || (!DamageResponsePolicyUtility.InterruptsAction(controlLockPolicy)
                    && responsePolicy != DamageResponsePolicy.Stagger))
            {
                return;
            }

            ActionCameraController cameraController = ActionCameraController.ActiveInstance;
            if (cameraController == null)
            {
                return;
            }

            cameraController.RequestHeavyShotFeedback(travelDirection, 0.9f);
            cameraController.RequestExplosionFeedback(impactPoint, 8f, 0.35f);
        }

        public void Deactivate()
        {
            active = false;
            remainingLifetime = 0f;
            StopAudioSources();
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

        private void ResetTrailRenderers()
        {
            TrailRenderer[] trailRenderers = GetComponentsInChildren<TrailRenderer>(includeInactive: true);
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

        private void RestartAudioSources()
        {
            EnsureAudioSources();
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
            EnsureAudioSources();
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i] != null)
                {
                    audioSources[i].Stop();
                }
            }
        }

        private void EnsureAudioSources()
        {
            if (audioSourcesResolved)
            {
                return;
            }

            audioSourcesResolved = true;
            audioSources = GetComponentsInChildren<AudioSource>(includeInactive: true);
        }

        private Vector3 ResolveDirection(Vector3 direction)
        {
            if (allowVerticalTravel && direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }

            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            return Vector3.forward;
        }
    }
}
