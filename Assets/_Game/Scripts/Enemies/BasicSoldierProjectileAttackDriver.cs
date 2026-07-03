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
        [SerializeField, Min(0f)] private float originHeight = 1.25f;
        [SerializeField, Min(0f)] private float targetHeightOffset = 0.9f;
        [SerializeField] private DamageResponsePolicy responsePolicy = DamageResponsePolicy.FlashOnly;
        [SerializeField] private CombatControlLockPolicy controlLockPolicy = CombatControlLockPolicy.None;

        private bool firedForCurrentAttack;
        private int firedCount;

        public int FiredCount => firedCount;

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

            firedForCurrentAttack = false;
        }

        private void OnDisable()
        {
            if (soldier != null)
            {
                soldier.PatternStateChanged -= HandlePatternStateChanged;
            }

            firedForCurrentAttack = false;
        }

        private void HandlePatternStateChanged(CombatAiPatternState state, CombatAiPatternProfile profile)
        {
            if (state != CombatAiPatternState.AttackActive)
            {
                firedForCurrentAttack = false;
                return;
            }

            if (profile == null || profile.AttackShape != CombatAiAttackShape.ProjectileLine)
            {
                return;
            }

            if (firedForCurrentAttack)
            {
                return;
            }

            firedForCurrentAttack = true;
            FireProjectile();
        }

        private void FireProjectile()
        {
            ResolveReferences();
            Vector3 origin = projectileOrigin != null
                ? projectileOrigin.position
                : transform.position + Vector3.up * originHeight;
            Vector3 direction = ResolveFireDirection(origin);
            LaneActionProjectile projectile = InstantiateProjectile(origin, direction);
            if (projectile == null)
            {
                return;
            }

            DamageTeam team = sourceHealth != null ? sourceHealth.Team : DamageTeam.Enemy;
            projectile.Configure(
                sourceHealth,
                team,
                ResolveProjectileDamage(),
                direction,
                projectileSpeed,
                projectileLifetimeSeconds,
                projectileRadius,
                responsePolicy,
                controlLockPolicy);
            firedCount++;
        }

        private float ResolveProjectileDamage()
        {
            if (projectileDamageOverride > 0f)
            {
                return projectileDamageOverride;
            }

            return soldier != null && soldier.PatternProfile != null
                ? soldier.PatternProfile.Damage
                : 12f;
        }

        private LaneActionProjectile InstantiateProjectile(Vector3 origin, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            if (projectilePrefab != null)
            {
                return Instantiate(projectilePrefab, origin, rotation, projectileRoot);
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "BasicSoldierProjectile_Runtime";
            projectileObject.transform.SetPositionAndRotation(origin, rotation);
            projectileObject.transform.localScale = Vector3.one * Mathf.Max(0.08f, projectileRadius * 2f);
            if (projectileRoot != null)
            {
                projectileObject.transform.SetParent(projectileRoot, worldPositionStays: true);
            }

            SphereCollider sphereCollider = projectileObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.isTrigger = true;
            }

            Rigidbody projectileRigidbody = projectileObject.AddComponent<Rigidbody>();
            projectileRigidbody.useGravity = false;
            projectileRigidbody.isKinematic = true;
            return projectileObject.AddComponent<LaneActionProjectile>();
        }

        private Vector3 ResolveFireDirection(Vector3 origin)
        {
            if (targetSensor != null
                && targetSensor.TryGetCurrentTarget(out Transform currentTarget, out CombatHealth _)
                && currentTarget != null)
            {
                Vector3 toTarget = currentTarget.position + Vector3.up * targetHeightOffset - origin;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    return toTarget.normalized;
                }
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
