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
        private Coroutine activeRoutine;
        private GameObject activeEffect;

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

        private void OnDisable()
        {
            StopActiveSweep();
        }

        private void OnDestroy()
        {
            StopActiveSweep();
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
            activeEffect = Instantiate(laserPrefab, origin, baseRotation, parent);
            activeEffect.name = laserPrefab.name;
            activeEffect.transform.localScale = Vector3.one * effectScale;
            Transform beamSpace = FindDescendant(activeEffect.transform, "Rotator") ?? activeEffect.transform;
            RestartParticles(activeEffect);

            float elapsed = 0f;
            float hitTimer = 0f;
            float damagePerTick = ResolveDamagePerTick(tier);

            ApplyDamageForBeamSpace(origin, beamSpace, damagePerTick);

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
                    ApplyDamageForBeamSpace(ResolveOrigin(), beamSpace, damagePerTick);
                }

                yield return null;
            }

            StopParticles(activeEffect);
            if (activeEffect != null)
            {
                Destroy(activeEffect, effectDestroyDelaySeconds);
            }

            activeEffect = null;
            activeRoutine = null;
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
            float damagePerTick)
        {
            if (damagePerTick <= 0f || beamSpace == null)
            {
                return;
            }

            damagedThisTick.Clear();
            CombatHealth[] candidates =
                FindObjectsByType<CombatHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Vector3 forward = ResolvePlanarDirection(beamSpace.forward, Vector3.forward);
            Vector3 right = ResolvePlanarDirection(beamSpace.right, Vector3.right);

            for (int candidateIndex = 0; candidateIndex < candidates.Length; candidateIndex++)
            {
                CombatHealth candidate = candidates[candidateIndex];
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

                if (!IsInsideCurrentBeams(planarOffset, forward, right, out Vector3 hitDirection))
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
            out Vector3 hitDirection)
        {
            return IsInsideBeam(planarOffset, forward, out hitDirection)
                || IsInsideBeam(planarOffset, right, out hitDirection)
                || IsInsideBeam(planarOffset, -forward, out hitDirection)
                || IsInsideBeam(planarOffset, -right, out hitDirection);
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

        private static Vector3 ResolveTargetPosition(CombatHealth target)
        {
            Collider collider = target.GetComponentInChildren<Collider>();
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

            if (activeEffect != null)
            {
                Destroy(activeEffect);
                activeEffect = null;
            }
        }

        private static void RestartParticles(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Clear(withChildren: true);
                particles[i].Play(withChildren: true);
            }
        }

        private static void StopParticles(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
