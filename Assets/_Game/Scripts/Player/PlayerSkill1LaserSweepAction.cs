using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerSkill1LaserSweepAction : MonoBehaviour
    {
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private GameObject laserPrefab;
        [SerializeField] private Transform effectRoot;
        [SerializeField] private DamageTeam sourceTeam = DamageTeam.Player;

        [Header("Sweep")]
        [SerializeField, Min(0.1f)] private float activeSeconds = 2.5f;
        [SerializeField, Min(0.1f)] private float radius = 23.5f;
        [SerializeField, Min(0.05f)] private float beamHalfWidth = 1.1f;
        [SerializeField, Min(0.01f)] private float hitIntervalSeconds = 0.08f;
        [SerializeField, Min(0f)] private float effectHeightOffset = 0.03f;
        [SerializeField, Min(0.01f)] private float effectScale = 1f;
        [SerializeField, Min(0f)] private float effectDestroyDelaySeconds = 1f;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float tierOneDamagePerTick = 18f;
        [SerializeField, Min(0f)] private float tierTwoDamagePerTick = 32f;
        [SerializeField, Min(0f)] private float tierThreeDamagePerTick = 54f;
        [SerializeField] private DamageResponsePolicy responsePolicy = DamageResponsePolicy.FlashOnly;
        [SerializeField] private CombatControlLockPolicy controlLockPolicy = CombatControlLockPolicy.None;

        private readonly HashSet<CombatHealth> damagedThisTick = new HashSet<CombatHealth>();
        private readonly List<CombatHealth> candidateBuffer = new List<CombatHealth>(32);
        private readonly Dictionary<CombatHealth, Collider> targetColliderCache =
            new Dictionary<CombatHealth, Collider>(32);
        private Coroutine activeRoutine;
        private Coroutine effectReleaseRoutine;
        private GameObject activeEffect;
        private GameObject pooledEffect;
        private Transform pooledBeamSpace;
        private Vector3 pooledBeamLocalPosition;
        private Quaternion pooledBeamLocalRotation = Quaternion.identity;
        private Vector3 pooledBeamLocalScale = Vector3.one;
        private bool hasPooledBeamLocalPose;
        private ParticleSystem[] pooledParticles = new ParticleSystem[0];
        private bool effectReleasePending;
        private float effectReleaseTime;

        public bool HasActiveSweep => activeRoutine != null;

        private void Awake()
        {
            if (sourceHealth == null)
            {
                sourceHealth = GetComponent<CombatHealth>();
            }

            if (targetSelector == null)
            {
                targetSelector = GetComponent<PlayerCombatTargetSelector>();
            }
        }

        private void OnEnable()
        {
            CombatHealth.BecameInactive += HandleHealthBecameInactive;
            EnsurePooledEffect();
        }

        private void OnDisable()
        {
            CombatHealth.BecameInactive -= HandleHealthBecameInactive;
            StopActiveSweep();
            candidateBuffer.Clear();
            targetColliderCache.Clear();
        }

        private void OnDestroy()
        {
            CombatHealth.BecameInactive -= HandleHealthBecameInactive;
            StopActiveSweep();
            if (pooledEffect != null)
            {
                Destroy(pooledEffect);
                pooledEffect = null;
            }
        }

        public bool TryCastLaserSweep(int tier)
        {
            if (!isActiveAndEnabled || laserPrefab == null)
            {
                return false;
            }

            if (sourceHealth == null)
            {
                sourceHealth = GetComponent<CombatHealth>();
            }

            StopActiveSweep();
            activeRoutine = StartCoroutine(RunSweepRoutine(Mathf.Clamp(tier, 1, 3)));
            return true;
        }

        private IEnumerator RunSweepRoutine(int tier)
        {
            Vector3 origin = ResolveOrigin();
            Quaternion baseRotation = ResolveBaseRotation();
            Transform parent = effectRoot != null ? effectRoot : transform;
            activeEffect = AcquirePooledEffect(origin, baseRotation, parent);
            if (activeEffect == null)
            {
                activeRoutine = null;
                yield break;
            }

            Transform beamSpace = pooledBeamSpace != null ? pooledBeamSpace : activeEffect.transform;

            float elapsed = 0f;
            float hitTimer = 0f;
            float damagePerTick = ResolveDamagePerTick(tier);
            Vector3 forward = ResolvePlanarDirection(beamSpace.forward, Vector3.forward);
            Vector3 right = ResolvePlanarDirection(beamSpace.right, Vector3.right);
            bool hasBlockedBeam = SummonPressureScreen.TryInterceptAnySkillBeam(
                sourceTeam,
                origin,
                forward,
                right,
                radius,
                beamHalfWidth,
                out int blockedBeamIndex,
                out float blockedBeamDistance);

            ApplyDamageForBeamSpace(
                origin,
                beamSpace,
                damagePerTick,
                hasBlockedBeam,
                blockedBeamIndex,
                blockedBeamDistance);

            while (elapsed < activeSeconds)
            {
                float deltaTime = Time.deltaTime;
                elapsed = Mathf.Min(activeSeconds, elapsed + deltaTime);

                if (activeEffect != null)
                {
                    activeEffect.transform.position = ResolveOrigin();
                }

                hitTimer += deltaTime;
                if (hitTimer >= hitIntervalSeconds || elapsed >= activeSeconds)
                {
                    hitTimer = 0f;
                    ApplyDamageForBeamSpace(
                        ResolveOrigin(),
                        beamSpace,
                        damagePerTick,
                        hasBlockedBeam,
                        blockedBeamIndex,
                        blockedBeamDistance);
                }

                yield return null;
            }

            StopParticles(pooledParticles);
            activeEffect = null;
            activeRoutine = null;
            SchedulePooledEffectRelease();
        }

        private Vector3 ResolveOrigin()
        {
            return transform.position + Vector3.up * effectHeightOffset;
        }

        private Quaternion ResolveBaseRotation()
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (targetSelector != null
                && targetSelector.TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth)
                && target != null
                && targetHealth != null
                && targetHealth.IsAlive)
            {
                Vector3 targetDirection = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
                if (targetDirection.sqrMagnitude > 0.0001f)
                {
                    forward = targetDirection.normalized;
                }
            }

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private float ResolveDamagePerTick(int tier)
        {
            return tier switch
            {
                1 => tierOneDamagePerTick,
                2 => tierTwoDamagePerTick,
                _ => tierThreeDamagePerTick
            };
        }

        private void ApplyDamageForBeamSpace(
            Vector3 origin,
            Transform beamSpace,
            float damagePerTick,
            bool hasBlockedBeam,
            int blockedBeamIndex,
            float blockedBeamDistance)
        {
            if (damagePerTick <= 0f || beamSpace == null)
            {
                return;
            }

            damagedThisTick.Clear();
            candidateBuffer.Clear();
            IReadOnlyList<CombatHealth> activeHealth = CombatHealth.ActiveInstances;
            for (int i = 0; i < activeHealth.Count; i++)
            {
                candidateBuffer.Add(activeHealth[i]);
            }

            Vector3 forward = ResolvePlanarDirection(beamSpace.forward, Vector3.forward);
            Vector3 right = ResolvePlanarDirection(beamSpace.right, Vector3.right);

            for (int candidateIndex = 0; candidateIndex < candidateBuffer.Count; candidateIndex++)
            {
                CombatHealth candidate = candidateBuffer[candidateIndex];
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                Vector3 targetPosition = ResolveTargetPosition(candidate);
                Vector3 planarOffset = Vector3.ProjectOnPlane(targetPosition - origin, Vector3.up);
                if (planarOffset.sqrMagnitude <= 0.0001f || planarOffset.sqrMagnitude > radius * radius)
                {
                    continue;
                }

                if (!IsInsideCurrentBeams(
                        planarOffset,
                        forward,
                        right,
                        out Vector3 hitDirection,
                        out int hitBeamIndex)
                    || (hasBlockedBeam
                        && hitBeamIndex == blockedBeamIndex
                        && Vector3.Dot(planarOffset, hitDirection) >= blockedBeamDistance))
                {
                    continue;
                }

                DamageInfo damageInfo = new DamageInfo(
                    sourceHealth,
                    sourceTeam,
                    damagePerTick,
                    origin + hitDirection * Mathf.Min(radius, planarOffset.magnitude),
                    hitDirection,
                    0f,
                    responsePolicy,
                    controlLockPolicy);

                if (candidate.TryApplyDamage(damageInfo))
                {
                    damagedThisTick.Add(candidate);
                }
            }
        }

        private bool IsValidTarget(CombatHealth candidate)
        {
            return candidate != null
                && candidate != sourceHealth
                && candidate.IsAlive
                && !damagedThisTick.Contains(candidate)
                && CombatTeamUtility.AreHostile(sourceTeam, candidate.Team);
        }

        private bool IsInsideCurrentBeams(
            Vector3 planarOffset,
            Vector3 forward,
            Vector3 right,
            out Vector3 hitDirection,
            out int hitBeamIndex)
        {
            if (IsInsideBeam(planarOffset, forward, out hitDirection))
            {
                hitBeamIndex = 0;
                return true;
            }

            if (IsInsideBeam(planarOffset, right, out hitDirection))
            {
                hitBeamIndex = 1;
                return true;
            }

            if (IsInsideBeam(planarOffset, -forward, out hitDirection))
            {
                hitBeamIndex = 2;
                return true;
            }

            if (IsInsideBeam(planarOffset, -right, out hitDirection))
            {
                hitBeamIndex = 3;
                return true;
            }

            hitDirection = Vector3.forward;
            hitBeamIndex = -1;
            return false;
        }

        private bool IsInsideBeam(Vector3 planarOffset, Vector3 direction, out Vector3 hitDirection)
        {
            float forwardDistance = Vector3.Dot(planarOffset, direction);
            if (forwardDistance >= 0f && forwardDistance <= radius)
            {
                Vector3 closestPoint = direction * forwardDistance;
                if ((planarOffset - closestPoint).sqrMagnitude <= beamHalfWidth * beamHalfWidth)
                {
                    hitDirection = direction;
                    return true;
                }
            }

            hitDirection = Vector3.forward;
            return false;
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction, Vector3 fallback)
        {
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude > 0.0001f)
            {
                return planar.normalized;
            }

            return fallback.normalized;
        }

        private Vector3 ResolveTargetPosition(CombatHealth target)
        {
            if (!targetColliderCache.TryGetValue(target, out Collider collider) || collider == null)
            {
                collider = target.GetComponentInChildren<Collider>();
                targetColliderCache[target] = collider;
            }

            return collider != null ? collider.bounds.center : target.transform.position;
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDescendant(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private void StopActiveSweep()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            if (effectReleaseRoutine != null)
            {
                StopCoroutine(effectReleaseRoutine);
                effectReleaseRoutine = null;
            }

            effectReleasePending = false;
            if (pooledEffect != null)
            {
                StopParticles(pooledParticles);
                pooledEffect.SetActive(false);
            }

            activeEffect = null;
        }

        private GameObject AcquirePooledEffect(Vector3 position, Quaternion rotation, Transform parent)
        {
            EnsurePooledEffect();
            if (pooledEffect == null)
            {
                return null;
            }

            effectReleasePending = false;
            Transform effectTransform = pooledEffect.transform;
            if (effectTransform.parent != parent)
            {
                effectTransform.SetParent(parent, worldPositionStays: false);
            }

            effectTransform.SetPositionAndRotation(position, rotation);
            effectTransform.localScale = Vector3.one * effectScale;
            RestorePooledBeamLocalPose();
            pooledEffect.SetActive(true);
            RestartParticles(pooledParticles);
            return pooledEffect;
        }

        private void EnsurePooledEffect()
        {
            if (pooledEffect != null || laserPrefab == null)
            {
                return;
            }

            Transform parent = effectRoot != null ? effectRoot : transform;
            pooledEffect = Instantiate(laserPrefab, parent);
            pooledEffect.name = laserPrefab.name;
            pooledBeamSpace = FindDescendant(pooledEffect.transform, "Rotator") ?? pooledEffect.transform;
            CapturePooledBeamLocalPose();
            pooledParticles = pooledEffect.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            pooledEffect.SetActive(false);
        }

        private void CapturePooledBeamLocalPose()
        {
            hasPooledBeamLocalPose = pooledEffect != null
                && pooledBeamSpace != null
                && pooledBeamSpace != pooledEffect.transform;
            if (!hasPooledBeamLocalPose)
            {
                return;
            }

            pooledBeamLocalPosition = pooledBeamSpace.localPosition;
            pooledBeamLocalRotation = pooledBeamSpace.localRotation;
            pooledBeamLocalScale = pooledBeamSpace.localScale;
        }

        private void RestorePooledBeamLocalPose()
        {
            if (!hasPooledBeamLocalPose || pooledBeamSpace == null)
            {
                return;
            }

            pooledBeamSpace.SetLocalPositionAndRotation(
                pooledBeamLocalPosition,
                pooledBeamLocalRotation);
            pooledBeamSpace.localScale = pooledBeamLocalScale;
        }

        private void SchedulePooledEffectRelease()
        {
            if (pooledEffect == null)
            {
                return;
            }

            if (effectDestroyDelaySeconds <= 0f)
            {
                ReleasePooledEffect();
                return;
            }

            effectReleasePending = true;
            effectReleaseTime = Time.time + effectDestroyDelaySeconds;
            effectReleaseRoutine = StartCoroutine(ReleasePooledEffectAfterDelay());
        }

        private IEnumerator ReleasePooledEffectAfterDelay()
        {
            while (effectReleasePending && Time.time < effectReleaseTime)
            {
                yield return null;
            }

            ReleasePooledEffect();
            effectReleaseRoutine = null;
        }

        private void ReleasePooledEffect()
        {
            effectReleasePending = false;
            if (pooledEffect != null)
            {
                pooledEffect.SetActive(false);
            }
        }

        private void HandleHealthBecameInactive(CombatHealth health)
        {
            if (health != null)
            {
                targetColliderCache.Remove(health);
            }
        }

        private static void RestartParticles(ParticleSystem[] particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null)
                {
                    continue;
                }

                particles[i].Clear(withChildren: true);
                particles[i].Play(withChildren: true);
            }
        }

        private static void StopParticles(ParticleSystem[] particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null)
                {
                    continue;
                }

                particles[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
