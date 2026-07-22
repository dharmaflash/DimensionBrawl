using System.Collections.Generic;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BasicSoldierEnemy))]
    [RequireComponent(typeof(CombatHealth))]
    [RequireComponent(typeof(CombatTargetSensor))]
    public sealed class BasicSoldierProjectileAttackDriver : MonoBehaviour
    {
        [SerializeField] private BasicSoldierEnemy soldier;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private CombatTargetSensor targetSensor;
        [SerializeField] private Transform projectileOrigin;
        [SerializeField] private LaneActionProjectile projectilePrefab;
        [SerializeField] private Transform projectileRoot;
        [SerializeField, Min(0f)] private float projectileDamageOverride;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 18f;
        [SerializeField, Min(0.05f)] private float projectileLifetimeSeconds = 1.25f;
        [SerializeField, Min(0.01f)] private float projectileRadius = 0.2f;
        [SerializeField, Min(1)] private int maxOwnedProjectiles = 3;
        [SerializeField, Min(0f)] private float originHeight = 1.25f;
        [SerializeField, Min(0f)] private float targetHeightOffset = 0.9f;
        [SerializeField] private DamageResponsePolicy responsePolicy = DamageResponsePolicy.FlashOnly;
        [SerializeField] private CombatControlLockPolicy controlLockPolicy = CombatControlLockPolicy.None;

        private readonly List<LaneActionProjectile> ownedProjectiles = new();
        private Transform runtimeProjectileRoot;
        private bool firedForCurrentAttack;
        private bool hasPreparedFireDirection;
        private Vector3 preparedFireDirection = Vector3.forward;
        private int firedCount;
        private LaneActionProjectile lastFiredProjectile;

        public int FiredCount => firedCount;
        public int OwnedProjectileCount => CountOwnedProjectiles(activeOnly: false);
        public int ActiveProjectileCount => CountOwnedProjectiles(activeOnly: true);
        public int MaxOwnedProjectileCount => Mathf.Max(1, maxOwnedProjectiles);
        public LaneActionProjectile LastFiredProjectile => lastFiredProjectile;
        public CombatHealth SourceHealth => sourceHealth;
        public CombatTargetSensor TargetSensor => targetSensor;
        public Transform ProjectileOrigin => projectileOrigin;
        public LaneActionProjectile ProjectilePrefab => projectilePrefab;
        public Transform ProjectilePoolRoot => projectileRoot;
        public Transform RuntimeProjectileRoot => runtimeProjectileRoot;
        public bool HasIndependentRuntimeProjectileRoot => runtimeProjectileRoot != null
            && runtimeProjectileRoot != transform
            && !runtimeProjectileRoot.IsChildOf(transform);

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (soldier != null)
            {
                soldier.PatternStateChanged += HandlePatternStateChanged;
            }

            if (sourceHealth != null)
            {
                sourceHealth.Died += HandleSourceDied;
            }

            firedForCurrentAttack = false;
            hasPreparedFireDirection = false;
        }

        private void OnDisable()
        {
            if (soldier != null)
            {
                soldier.PatternStateChanged -= HandlePatternStateChanged;
            }

            if (sourceHealth != null)
            {
                sourceHealth.Died -= HandleSourceDied;
            }

            firedForCurrentAttack = false;
            hasPreparedFireDirection = false;
            DeactivateAndParkOwnedProjectiles();
        }

        private void OnDestroy()
        {
            DeactivateAndParkOwnedProjectiles();
        }

        private void HandleSourceDied()
        {
            firedForCurrentAttack = false;
            hasPreparedFireDirection = false;
            DeactivateAndParkOwnedProjectiles();
        }

        private void HandlePatternStateChanged(CombatAiPatternState state, CombatAiPatternProfile profile)
        {
            if (state == CombatAiPatternState.Windup
                && profile != null
                && profile.AttackShape == CombatAiAttackShape.ProjectileLine)
            {
                firedForCurrentAttack = false;
                Vector3 origin = projectileOrigin != null
                    ? projectileOrigin.position
                    : transform.position + Vector3.up * originHeight;
                preparedFireDirection = ResolveFireDirection(origin);
                hasPreparedFireDirection = preparedFireDirection.sqrMagnitude > 0.0001f;
                return;
            }

            if (state != CombatAiPatternState.AttackActive)
            {
                firedForCurrentAttack = false;
                hasPreparedFireDirection = false;
                return;
            }

            if (profile == null || profile.AttackShape != CombatAiAttackShape.ProjectileLine)
            {
                hasPreparedFireDirection = false;
                return;
            }

            if (firedForCurrentAttack)
            {
                return;
            }

            firedForCurrentAttack = true;
            FireProjectile(profile);
        }

        private void FireProjectile(CombatAiPatternProfile profile)
        {
            ResolveReferences();
            Vector3 origin = projectileOrigin != null
                ? projectileOrigin.position
                : transform.position + Vector3.up * originHeight;
            Vector3 direction = hasPreparedFireDirection
                ? preparedFireDirection
                : ResolveFireDirection(origin);
            hasPreparedFireDirection = false;
            lastFiredProjectile = null;
            LaneActionProjectile projectile = InstantiateProjectile(origin, direction);
            if (projectile == null)
            {
                return;
            }

            DamageTeam team = sourceHealth != null ? sourceHealth.Team : DamageTeam.Enemy;
            projectile.Configure(
                sourceHealth,
                team,
                ResolveProjectileDamage(profile),
                direction,
                projectileSpeed,
                projectileLifetimeSeconds,
                projectileRadius,
                profile != null ? profile.DamageResponsePolicy : responsePolicy,
                profile != null ? profile.ControlLockPolicy : controlLockPolicy,
                profile != null ? profile.HitStopSeconds : 0f);
            lastFiredProjectile = projectile;
            firedCount++;
        }

        private float ResolveProjectileDamage(CombatAiPatternProfile profile)
        {
            if (projectileDamageOverride > 0f)
            {
                return projectileDamageOverride;
            }

            return profile != null
                ? profile.Damage
                : soldier != null && soldier.PatternProfile != null
                    ? soldier.PatternProfile.Damage
                : 12f;
        }

        private LaneActionProjectile InstantiateProjectile(Vector3 origin, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            RemoveDestroyedProjectileReferences();
            for (int i = 0; i < ownedProjectiles.Count; i++)
            {
                LaneActionProjectile pooledProjectile = ownedProjectiles[i];
                if (pooledProjectile == null || pooledProjectile.IsActive)
                {
                    continue;
                }

                PrepareProjectileForWorldFlight(pooledProjectile, origin, rotation);
                return pooledProjectile;
            }

            if (ownedProjectiles.Count >= MaxOwnedProjectileCount)
            {
                return null;
            }

            LaneActionProjectile projectile;
            if (projectilePrefab != null)
            {
                projectile = Instantiate(projectilePrefab, origin, rotation);
                projectile.name = $"{projectilePrefab.name}_Owned_{ownedProjectiles.Count + 1:00}";
                ownedProjectiles.Add(projectile);
                PrepareProjectileForWorldFlight(projectile, origin, rotation);
                return projectile;
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = $"BasicSoldierProjectile_Runtime_Owned_{ownedProjectiles.Count + 1:00}";
            projectileObject.transform.SetPositionAndRotation(origin, rotation);
            projectileObject.transform.localScale = Vector3.one * Mathf.Max(0.08f, projectileRadius * 2f);

            SphereCollider sphereCollider = projectileObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.isTrigger = true;
            }

            Rigidbody projectileRigidbody = projectileObject.AddComponent<Rigidbody>();
            projectileRigidbody.useGravity = false;
            projectileRigidbody.isKinematic = true;
            projectile = projectileObject.AddComponent<LaneActionProjectile>();
            ownedProjectiles.Add(projectile);
            PrepareProjectileForWorldFlight(projectile, origin, rotation);
            return projectile;
        }

        public bool IsConfiguredFor(
            BasicSoldierEnemy expectedSoldier,
            CombatHealth expectedSourceHealth,
            CombatTargetSensor expectedTargetSensor)
        {
            return expectedSoldier != null
                && expectedSourceHealth != null
                && expectedTargetSensor != null
                && ReferenceEquals(soldier, expectedSoldier)
                && ReferenceEquals(sourceHealth, expectedSourceHealth)
                && ReferenceEquals(targetSensor, expectedTargetSensor)
                && projectileOrigin != null
                && projectilePrefab != null
                && projectileRoot != null
                && projectileRoot != projectileOrigin
                && projectileRoot != expectedSoldier.transform
                && projectileOrigin != expectedSoldier.transform
                && projectileRoot.transform.IsChildOf(expectedSoldier.transform)
                && projectileOrigin.transform.IsChildOf(expectedSoldier.transform)
                && MaxOwnedProjectileCount > 0;
        }

        public void ConfigureRuntimeProjectileRoot(Transform newRuntimeProjectileRoot)
        {
            if (newRuntimeProjectileRoot != null
                && (newRuntimeProjectileRoot == transform
                    || newRuntimeProjectileRoot.IsChildOf(transform)
                    || newRuntimeProjectileRoot.gameObject.scene != gameObject.scene))
            {
                throw new System.ArgumentException(
                    "The runtime projectile root must be a scene-local transform outside the moving soldier hierarchy.",
                    nameof(newRuntimeProjectileRoot));
            }

            DeactivateAndParkOwnedProjectiles();
            runtimeProjectileRoot = newRuntimeProjectileRoot;
        }

        private void PrepareProjectileForWorldFlight(
            LaneActionProjectile projectile,
            Vector3 origin,
            Quaternion rotation)
        {
            projectile.transform.SetParent(runtimeProjectileRoot, worldPositionStays: true);
            projectile.transform.SetPositionAndRotation(origin, rotation);
        }

        private void DeactivateAndParkOwnedProjectiles()
        {
            RemoveDestroyedProjectileReferences();
            for (int i = 0; i < ownedProjectiles.Count; i++)
            {
                LaneActionProjectile projectile = ownedProjectiles[i];
                if (projectile == null)
                {
                    continue;
                }

                if (projectile.IsActive || projectile.gameObject.activeSelf)
                {
                    projectile.Deactivate();
                }

                Transform poolRoot = runtimeProjectileRoot != null
                    ? runtimeProjectileRoot
                    : projectileRoot;
                if (poolRoot != null)
                {
                    projectile.transform.SetParent(poolRoot, worldPositionStays: false);
                    projectile.transform.localPosition = Vector3.zero;
                    projectile.transform.localRotation = Quaternion.identity;
                }
            }

            lastFiredProjectile = null;
        }

        private int CountOwnedProjectiles(bool activeOnly)
        {
            int count = 0;
            for (int i = 0; i < ownedProjectiles.Count; i++)
            {
                LaneActionProjectile projectile = ownedProjectiles[i];
                if (projectile != null && (!activeOnly || projectile.IsActive))
                {
                    count++;
                }
            }

            return count;
        }

        private void RemoveDestroyedProjectileReferences()
        {
            for (int i = ownedProjectiles.Count - 1; i >= 0; i--)
            {
                if (ownedProjectiles[i] == null)
                {
                    ownedProjectiles.RemoveAt(i);
                }
            }
        }

        private Vector3 ResolveFireDirection(Vector3 origin)
        {
            Vector3 warnedDirection = Vector3.zero;
            if (soldier != null)
            {
                warnedDirection = Vector3.ProjectOnPlane(
                    soldier.ResolvedAttackDirection,
                    Vector3.up);
                warnedDirection = warnedDirection.sqrMagnitude > 0.0001f
                    ? warnedDirection.normalized
                    : Vector3.zero;
            }

            if (targetSensor != null
                && targetSensor.TryGetCurrentTarget(out Transform currentTarget, out CombatHealth _)
                && currentTarget != null)
            {
                Vector3 toTarget = currentTarget.position + Vector3.up * targetHeightOffset - origin;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    if (warnedDirection.sqrMagnitude > 0.0001f)
                    {
                        float warnedDistance = Vector3.Dot(
                            Vector3.ProjectOnPlane(toTarget, Vector3.up),
                            warnedDirection);
                        if (warnedDistance > 0.0001f)
                        {
                            Vector3 warnedTargetDirection = warnedDirection * warnedDistance
                                + Vector3.up * toTarget.y;
                            if (warnedTargetDirection.sqrMagnitude > 0.0001f)
                            {
                                return warnedTargetDirection.normalized;
                            }
                        }
                    }

                    return toTarget.normalized;
                }
            }

            if (warnedDirection.sqrMagnitude > 0.0001f)
            {
                return warnedDirection;
            }

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private void ResolveReferences()
        {
            if (soldier == null)
            {
                soldier = GetComponent<BasicSoldierEnemy>();
            }

            if (sourceHealth == null)
            {
                sourceHealth = GetComponent<CombatHealth>();
            }

            if (targetSensor == null)
            {
                targetSensor = GetComponent<CombatTargetSensor>();
            }
        }
    }
}
