using System;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerCombatTargetSelector : MonoBehaviour
    {
        [Header("Owner")]
        [SerializeField] private CombatHealth selfHealth;
        [SerializeField] private Transform selectionOrigin;
        [SerializeField] private Transform viewReference;

        [Header("Candidate Search")]
        [Tooltip("Authored or encounter-provided hostile candidates. Do not scene-scan for default ARPG targeting.")]
        [SerializeField] private CombatHealth[] targetCandidates = new CombatHealth[0];
        [SerializeField, Min(0f)] private float selectionRadius = 12f;
        [SerializeField, Min(0f)] private float retargetIntervalSeconds = 0.12f;
        [SerializeField, Min(0f)] private float contactStickinessSeconds = 0.35f;

        [Header("Priority Weights")]
        [Tooltip("Close targets read better for manual ARPG attacks, but should not beat all front/threat cues by itself.")]
        [SerializeField, Min(0f)] private float distanceWeight = 0.35f;
        [Tooltip("Reference direction: keep current forward danger readable before adding hard lock-on UI.")]
        [SerializeField, Min(0f)] private float ownerForwardWeight = 0.3f;
        [Tooltip("Camera-facing threats get a small bonus so the selected target matches what the player can read.")]
        [SerializeField, Min(0f)] private float viewForwardWeight = 0.2f;
        [Tooltip("Windup/active enemy states can beat pure distance, matching collected threat-priority notes.")]
        [SerializeField, Min(0f)] private float threatStateWeight = 0.35f;
        [Tooltip("Prevents jitter when two readable enemies have similar scores.")]
        [SerializeField, Min(0f)] private float currentTargetStickiness = 0.18f;
        [SerializeField, Range(-1f, 1f)] private float minimumReadableForwardDot = -0.35f;

        [Header("Attack Assist")]
        [Tooltip("Soft-lock pocket used only for basic-attack facing. A target outside melee range still receives no damage.")]
        [SerializeField, Min(0f)] private float attackAimRadius = 9f;
        [Tooltip("Default allows any candidate inside the local pocket. Raise this only if behind-the-back attack turns become unreadable.")]
        [SerializeField, Range(-1f, 1f)] private float minimumAttackAimDot = -1f;
        [Tooltip("Keeps basic attacks from ignoring an enemy already inside the current melee reach.")]
        [SerializeField, Min(0f)] private float attackReachPriorityWeight = 0.8f;

        private CombatHealth currentTargetHealth;
        private Transform currentTarget;
        private float nextRetargetTime;

        public CombatHealth SelfHealth => selfHealth;
        public Transform SelectionOrigin => selectionOrigin != null ? selectionOrigin : transform;
        public Transform ViewReference => viewReference;
        public CombatHealth CurrentTargetHealth => currentTargetHealth;
        public Transform CurrentTarget => currentTarget;
        public float SelectionRadius => selectionRadius;
        public float AttackAimRadius => attackAimRadius;
        public int TargetCandidateCount => targetCandidates != null ? targetCandidates.Length : 0;

        public event Action<CombatHealth> TargetChanged;

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

        public bool TryGetAttackAimDirection(
            Vector3 fallbackDirection,
            out Vector3 direction,
            out CombatHealth targetHealth)
        {
            return TryGetAttackAimDirection(fallbackDirection, 0f, out direction, out targetHealth);
        }

        public bool TryGetAttackAimDirection(
            Vector3 fallbackDirection,
            float preferredContactDistance,
            out Vector3 direction,
            out CombatHealth targetHealth)
        {
            Vector3 fallbackPlanarDirection = ResolvePlanarDirection(fallbackDirection, ResolvePlanarForward(SelectionOrigin));

            if (ShouldRefreshTarget())
            {
                RefreshTarget();
            }

            targetHealth = FindBestAttackAimTarget(fallbackPlanarDirection, preferredContactDistance);
            if (targetHealth == null)
            {
                direction = fallbackPlanarDirection;
                return false;
            }

            SetCurrentTarget(targetHealth);
            Vector3 offset = Vector3.ProjectOnPlane(targetHealth.transform.position - SelectionOrigin.position, Vector3.up);
            direction = offset.sqrMagnitude > 0.0001f ? offset.normalized : fallbackPlanarDirection;
            return true;
        }

        public bool RefreshTarget()
        {
            CombatHealth bestTarget = FindBestTarget();
            SetCurrentTarget(bestTarget);
            nextRetargetTime = Time.time + retargetIntervalSeconds;
            return currentTargetHealth != null;
        }

        public void NotifyTargetContact(CombatHealth contactedTarget)
        {
            if (!IsValidTarget(contactedTarget))
            {
                return;
            }

            SetCurrentTarget(contactedTarget);
            nextRetargetTime = Time.time + Mathf.Max(retargetIntervalSeconds, contactStickinessSeconds);
        }

        private void Awake()
        {
            if (selfHealth == null)
            {
                selfHealth = GetComponent<CombatHealth>();
            }

            if (selectionOrigin == null)
            {
                selectionOrigin = transform;
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
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < targetCandidates.Length; i++)
            {
                CombatHealth candidate = targetCandidates[i];
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                float score = ScoreCandidate(candidate);
                if (score > bestScore)
                {
                    bestTarget = candidate;
                    bestScore = score;
                }
            }

            return bestTarget;
        }

        private CombatHealth FindBestAttackAimTarget(Vector3 fallbackDirection, float preferredContactDistance)
        {
            if (targetCandidates == null)
            {
                return null;
            }

            CombatHealth bestTarget = null;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < targetCandidates.Length; i++)
            {
                CombatHealth candidate = targetCandidates[i];
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                float score = ScoreAttackAimCandidate(candidate, fallbackDirection, preferredContactDistance);
                if (score > bestScore)
                {
                    bestTarget = candidate;
                    bestScore = score;
                }
            }

            return bestTarget;
        }

        private float ScoreCandidate(CombatHealth candidate)
        {
            Transform origin = SelectionOrigin;
            Vector3 offset = Vector3.ProjectOnPlane(candidate.transform.position - origin.position, Vector3.up);
            float distance = offset.magnitude;
            if (selectionRadius > 0f && distance > selectionRadius)
            {
                return float.NegativeInfinity;
            }

            Vector3 direction = distance > 0.0001f ? offset / distance : ResolvePlanarForward(origin);
            float radius = selectionRadius > 0f ? selectionRadius : Mathf.Max(1f, distance);
            float distanceScore = 1f - Mathf.Clamp01(distance / radius);
            float ownerForwardScore = ResolveForwardScore(ResolvePlanarForward(origin), direction);
            float viewForwardScore = viewReference != null
                ? ResolveForwardScore(ResolvePlanarForward(viewReference), direction)
                : 0f;
            float threatScore = ResolveThreatScore(candidate);

            float score = distanceScore * distanceWeight
                + ownerForwardScore * ownerForwardWeight
                + viewForwardScore * viewForwardWeight
                + threatScore * threatStateWeight;

            if (candidate == currentTargetHealth)
            {
                score += currentTargetStickiness;
            }

            return score;
        }

        private float ScoreAttackAimCandidate(
            CombatHealth candidate,
            Vector3 fallbackDirection,
            float preferredContactDistance)
        {
            Transform origin = SelectionOrigin;
            Vector3 offset = Vector3.ProjectOnPlane(candidate.transform.position - origin.position, Vector3.up);
            float distance = offset.magnitude;
            if (attackAimRadius > 0f && distance > attackAimRadius)
            {
                return float.NegativeInfinity;
            }

            Vector3 direction = distance > 0.0001f ? offset / distance : fallbackDirection;
            float fallbackDot = Vector3.Dot(fallbackDirection.normalized, direction.normalized);
            if (fallbackDot < minimumAttackAimDot)
            {
                return float.NegativeInfinity;
            }

            float radius = attackAimRadius > 0f ? attackAimRadius : Mathf.Max(1f, distance);
            float distanceScore = 1f - Mathf.Clamp01(distance / radius);
            float contactScore = preferredContactDistance > 0f && distance <= preferredContactDistance ? 1f : 0f;
            float ownerForwardScore = ResolveForwardScore(ResolvePlanarForward(origin), direction);
            float viewForwardScore = viewReference != null
                ? ResolveForwardScore(ResolvePlanarForward(viewReference), direction)
                : 0f;
            float threatScore = ResolveThreatScore(candidate);

            float score = distanceScore * distanceWeight
                + contactScore * attackReachPriorityWeight
                + ownerForwardScore * ownerForwardWeight
                + viewForwardScore * viewForwardWeight
                + threatScore * threatStateWeight;

            if (candidate == currentTargetHealth)
            {
                score += currentTargetStickiness;
            }

            return score;
        }

        private float ResolveForwardScore(Vector3 forward, Vector3 direction)
        {
            if (forward.sqrMagnitude <= 0.0001f || direction.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            float dot = Vector3.Dot(forward.normalized, direction.normalized);
            return Mathf.Clamp01(Mathf.InverseLerp(minimumReadableForwardDot, 1f, dot));
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction, Vector3 fallbackDirection)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planarDirection.sqrMagnitude > 0.0001f)
            {
                return planarDirection.normalized;
            }

            Vector3 planarFallback = Vector3.ProjectOnPlane(fallbackDirection, Vector3.up);
            return planarFallback.sqrMagnitude > 0.0001f ? planarFallback.normalized : Vector3.forward;
        }

        private static Vector3 ResolvePlanarForward(Transform source)
        {
            Vector3 forward = source != null ? source.forward : Vector3.forward;
            Vector3 planarForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            return planarForward.sqrMagnitude > 0.0001f ? planarForward.normalized : Vector3.forward;
        }

        private static float ResolveThreatScore(CombatHealth candidate)
        {
            ICombatAiAgent agent = ResolveAgent(candidate);
            if (agent == null)
            {
                return 0f;
            }

            CombatAiPatternProfile profile = agent.PatternProfile;
            return agent.CurrentPatternState switch
            {
                CombatAiPatternState.AttackActive => Mathf.Max(1f, profile != null ? profile.ActiveCameraCueStrength : 1f),
                CombatAiPatternState.Windup => Mathf.Max(0.75f, profile != null ? profile.WindupThreatLevel : 1f),
                CombatAiPatternState.Recovery => 0.12f,
                CombatAiPatternState.Stagger => 0.05f,
                _ => 0f
            };
        }

        private static ICombatAiAgent ResolveAgent(CombatHealth candidate)
        {
            MonoBehaviour[] behaviours = candidate.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ICombatAiAgent agent)
                {
                    return agent;
                }
            }

            return null;
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

        private void SetCurrentTarget(CombatHealth nextTarget)
        {
            if (currentTargetHealth == nextTarget)
            {
                currentTarget = currentTargetHealth != null ? currentTargetHealth.transform : null;
                return;
            }

            currentTargetHealth = nextTarget;
            currentTarget = currentTargetHealth != null ? currentTargetHealth.transform : null;
            TargetChanged?.Invoke(currentTargetHealth);
        }
    }
}
