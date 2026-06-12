using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.AI
{
    public sealed class CombatTargetSensor : MonoBehaviour
    {
        [Header("Owner")]
        [SerializeField] private CombatHealth selfHealth;

        [Header("Target Search")]
        [Tooltip("Shared enemy/summon sensor range. Kept serialized so enemy types and future summon roles can tune it per prefab.")]
        [SerializeField, Min(0f)] private float searchRadius = 12f;

        [Tooltip("Small retarget cadence keeps one-enemy tests responsive without turning sensing into per-frame global management.")]
        [SerializeField, Min(0f)] private float retargetIntervalSeconds = 0.2f;

        [Tooltip("Authored or encounter-provided candidates. Avoids scene-wide searches and lets enemies/summons share the same hostile-selection rules.")]
        [SerializeField] private CombatHealth[] targetCandidates = new CombatHealth[0];

        private CombatHealth currentTargetHealth;
        private Transform currentTarget;
        private float nextRetargetTime;

        public CombatHealth SelfHealth => selfHealth;
        public CombatHealth CurrentTargetHealth => currentTargetHealth;
        public Transform CurrentTarget => currentTarget;
        public float SearchRadius => searchRadius;
        public int TargetCandidateCount => targetCandidates != null ? targetCandidates.Length : 0;

        public bool TryGetCurrentTarget(out Transform target, out CombatHealth targetHealth)
        {
            if (ShouldRefreshTarget())
            {
                RefreshTarget();
            }

            target = currentTarget;
            targetHealth = currentTargetHealth;
            return target != null && targetHealth != null && targetHealth.IsAlive;
        }

        public bool RefreshTarget()
        {
            currentTargetHealth = FindBestTarget();
            currentTarget = currentTargetHealth != null ? currentTargetHealth.transform : null;
            nextRetargetTime = Time.time + retargetIntervalSeconds;
            return currentTargetHealth != null;
        }

        private void Awake()
        {
            if (selfHealth == null)
            {
                selfHealth = GetComponent<CombatHealth>();
            }
        }

        private void OnEnable()
        {
            nextRetargetTime = 0f;
            RefreshTarget();
        }

        private bool ShouldRefreshTarget()
        {
            if (!IsValidTarget(currentTargetHealth))
            {
                return true;
            }

            return retargetIntervalSeconds <= 0f || Time.time >= nextRetargetTime;
        }

        private CombatHealth FindBestTarget()
        {
            if (targetCandidates == null)
            {
                return null;
            }

            CombatHealth bestTarget = null;
            float bestDistanceSqr = float.PositiveInfinity;
            float searchRadiusSqr = searchRadius * searchRadius;

            for (int i = 0; i < targetCandidates.Length; i++)
            {
                CombatHealth candidate = targetCandidates[i];
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                Vector3 offset = Vector3.ProjectOnPlane(candidate.transform.position - transform.position, Vector3.up);
                float distanceSqr = offset.sqrMagnitude;
                if (searchRadius > 0f && distanceSqr > searchRadiusSqr)
                {
                    continue;
                }

                if (distanceSqr < bestDistanceSqr)
                {
                    bestTarget = candidate;
                    bestDistanceSqr = distanceSqr;
                }
            }

            return bestTarget;
        }

        private bool IsValidTarget(CombatHealth candidate)
        {
            if (candidate == null || candidate == selfHealth || !candidate.IsAlive)
            {
                return false;
            }

            if (selfHealth == null)
            {
                return candidate.Team != DamageTeam.Neutral;
            }

            return CombatTeamUtility.AreHostile(selfHealth.Team, candidate.Team);
        }
    }
}
