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
        [Tooltip("Active summon proxies register themselves; this lets player fire answer the frontline without scene-scanning.")]
        [SerializeField] private bool includeActiveHostileSummons = true;
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
        [Tooltip("Keeps an enemy summon body in front of the boss from being ignored while it owns the frontline.")]
        [SerializeField, Min(0f)] private float activeSummonTargetBonus = 0.55f;
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
        public bool IncludesActiveHostileSummons => includeActiveHostileSummons;

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

        public bool TryGetRangedAimAssistDirection(
            Vector3 originPosition,
            Vector3 rawAimDirection,
            float maxDistance,
            float maxAngleDegrees,
            out Vector3 direction,
            out CombatHealth targetHealth)
        {
            return TryGetRangedAimAssistDirection(
                originPosition,
                rawAimDirection,
                maxDistance,
                maxAngleDegrees,
                out direction,
                out _,
                out targetHealth);
        }

        public bool TryGetRangedAimAssistDirection(
            Vector3 originPosition,
            Vector3 rawAimDirection,
            float maxDistance,
            float maxAngleDegrees,
            out Vector3 direction,
            out Vector3 aimPoint,
            out CombatHealth targetHealth)
        {
            Vector3 rawPlanarDirection = ResolvePlanarDirection(rawAimDirection, ResolvePlanarForward(SelectionOrigin));
            targetHealth = FindBestAimAssistTarget(originPosition, rawPlanarDirection, maxDistance, maxAngleDegrees);
            if (targetHealth == null)
            {
                aimPoint = default;
                direction = rawPlanarDirection;
                return false;
            }

            if (!TryResolveAimAssistPoint(targetHealth, originPosition, rawPlanarDirection, out aimPoint, out _))
            {
                aimPoint = targetHealth.transform.position;
            }

            Vector3 offset = aimPoint - originPosition;
            direction = offset.sqrMagnitude > 0.0001f ? offset.normalized : rawPlanarDirection;
            return true;
        }

        public bool TryGetBestLockTarget(
            Vector3 originPosition,
            Vector3 viewDirection,
            float maxDistance,
            float maxAngleDegrees,
            CombatHealth preferredTarget,
            float preferredTargetBonus,
            out CombatHealth targetHealth,
            out Vector3 lockPoint,
            out float strength01)
        {
            targetHealth = null;
            lockPoint = default;
            strength01 = 0f;
            if (maxDistance <= 0f || maxAngleDegrees <= 0f)
            {
                return false;
            }

            Vector3 planarViewDirection = ResolvePlanarDirection(viewDirection, ResolvePlanarForward(SelectionOrigin));
            CombatHealth bestTarget = null;
            float bestScore = float.NegativeInfinity;
            if (targetCandidates != null)
            {
                for (int i = 0; i < targetCandidates.Length; i++)
                {
                    ConsiderTargetCandidate(
                        targetCandidates[i],
                        candidate => ScoreLockTargetCandidate(
                            candidate,
                            originPosition,
                            planarViewDirection,
                            maxDistance,
                            maxAngleDegrees,
                            candidate == preferredTarget ? preferredTargetBonus : 0f),
                        0f,
                        ref bestTarget,
                        ref bestScore);
                }
            }

            ConsiderActiveSummonTargets(
                candidate => ScoreLockTargetCandidate(
                    candidate,
                    originPosition,
                    planarViewDirection,
                    maxDistance,
                    maxAngleDegrees,
                    candidate == preferredTarget ? preferredTargetBonus : 0f),
                activeSummonTargetBonus,
                ref bestTarget,
                ref bestScore);

            if (bestTarget == null
                || !TryResolveAimAssistPoint(bestTarget, originPosition, planarViewDirection, out lockPoint, out _))
            {
                return false;
            }

            Vector3 offset = Vector3.ProjectOnPlane(lockPoint - originPosition, Vector3.up);
            float distance = offset.magnitude;
            Vector3 direction = distance > 0.0001f ? offset / distance : planarViewDirection;
            float angle = Vector3.Angle(planarViewDirection, direction);
            float angleScore = 1f - Mathf.Clamp01(angle / Mathf.Max(0.01f, maxAngleDegrees));
            float distanceScore = 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, maxDistance));

            targetHealth = bestTarget;
            strength01 = Mathf.Clamp01(angleScore * 0.72f + distanceScore * 0.28f);
            return true;
        }

        public bool RefreshTarget()
        {
            if (IsValidTarget(currentTargetHealth) && Time.time < nextRetargetTime)
            {
                currentTarget = currentTargetHealth.transform;
                return true;
            }

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

        public void ConfigureTargetCandidates(CombatHealth[] candidates, bool refreshNow = true)
        {
            targetCandidates = candidates ?? Array.Empty<CombatHealth>();
            SetCurrentTarget(null);
            nextRetargetTime = 0f;
            if (refreshNow && isActiveAndEnabled)
            {
                RefreshTarget();
            }
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
            CombatHealth bestTarget = null;
            float bestScore = float.NegativeInfinity;

            if (targetCandidates != null)
            {
                for (int i = 0; i < targetCandidates.Length; i++)
                {
                    ConsiderTargetCandidate(targetCandidates[i], ScoreCandidate, 0f, ref bestTarget, ref bestScore);
                }
            }

            ConsiderActiveSummonTargets(ScoreCandidate, activeSummonTargetBonus, ref bestTarget, ref bestScore);
            return bestTarget;
        }

        private CombatHealth FindBestAttackAimTarget(Vector3 fallbackDirection, float preferredContactDistance)
        {
            CombatHealth bestTarget = null;
            float bestScore = float.NegativeInfinity;

            if (targetCandidates != null)
            {
                for (int i = 0; i < targetCandidates.Length; i++)
                {
                    ConsiderTargetCandidate(
                        targetCandidates[i],
                        candidate => ScoreAttackAimCandidate(candidate, fallbackDirection, preferredContactDistance),
                        0f,
                        ref bestTarget,
                        ref bestScore);
                }
            }

            ConsiderActiveSummonTargets(
                candidate => ScoreAttackAimCandidate(candidate, fallbackDirection, preferredContactDistance),
                activeSummonTargetBonus,
                ref bestTarget,
                ref bestScore);
            return bestTarget;
        }

        private CombatHealth FindBestAimAssistTarget(
            Vector3 originPosition,
            Vector3 rawAimDirection,
            float maxDistance,
            float maxAngleDegrees)
        {
            if (maxDistance <= 0f || maxAngleDegrees <= 0f)
            {
                return null;
            }

            CombatHealth bestTarget = null;
            float bestScore = float.NegativeInfinity;
            if (targetCandidates != null)
            {
                for (int i = 0; i < targetCandidates.Length; i++)
                {
                    ConsiderTargetCandidate(
                        targetCandidates[i],
                        candidate => ScoreAimAssistCandidate(
                            candidate,
                            originPosition,
                            rawAimDirection,
                            maxDistance,
                            maxAngleDegrees),
                        0f,
                        ref bestTarget,
                        ref bestScore);
                }
            }

            ConsiderActiveSummonTargets(
                candidate => ScoreAimAssistCandidate(candidate, originPosition, rawAimDirection, maxDistance, maxAngleDegrees),
                activeSummonTargetBonus,
                ref bestTarget,
                ref bestScore);
            return bestTarget;
        }

        private void ConsiderActiveSummonTargets(
            Func<CombatHealth, float> scorer,
            float scoreBonus,
            ref CombatHealth bestTarget,
            ref float bestScore)
        {
            if (!includeActiveHostileSummons)
            {
                return;
            }

            int count = SummonFrontlineProxy.ActiveRegisteredProxyCount;
            for (int i = 0; i < count; i++)
            {
                if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy))
                {
                    continue;
                }

                ConsiderTargetCandidate(proxy.Health, scorer, scoreBonus, ref bestTarget, ref bestScore);
            }
        }

        private void ConsiderTargetCandidate(
            CombatHealth candidate,
            Func<CombatHealth, float> scorer,
            float scoreBonus,
            ref CombatHealth bestTarget,
            ref float bestScore)
        {
            if (!IsValidTarget(candidate))
            {
                return;
            }

            float score = scorer(candidate);
            if (float.IsNegativeInfinity(score))
            {
                return;
            }

            score += scoreBonus;
            if (score > bestScore)
            {
                bestTarget = candidate;
                bestScore = score;
            }
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

        private float ScoreAimAssistCandidate(
            CombatHealth candidate,
            Vector3 originPosition,
            Vector3 rawAimDirection,
            float maxDistance,
            float maxAngleDegrees)
        {
            if (!TryResolveAimAssistPoint(candidate, originPosition, rawAimDirection, out Vector3 aimPoint, out Bounds bounds))
            {
                return float.NegativeInfinity;
            }

            Vector3 offset = Vector3.ProjectOnPlane(aimPoint - originPosition, Vector3.up);
            float distance = offset.magnitude;
            if (distance > maxDistance)
            {
                return float.NegativeInfinity;
            }

            Vector3 direction = distance > 0.0001f ? offset / distance : rawAimDirection;
            float angle = Vector3.Angle(rawAimDirection.normalized, direction.normalized);
            if (angle > maxAngleDegrees)
            {
                return float.NegativeInfinity;
            }

            float angleScore = 1f - Mathf.Clamp01(angle / maxAngleDegrees);
            float distanceScore = 1f - Mathf.Clamp01(distance / maxDistance);
            float nearBodyScore = distanceScore * distanceScore;
            float bodyMissDistance = DistanceFromRayToBounds(originPosition, rawAimDirection, bounds);
            float bodyRadius = Mathf.Max(0.05f, Mathf.Max(bounds.extents.x, bounds.extents.z));
            float bodyRayScore = 1f - Mathf.Clamp01(bodyMissDistance / Mathf.Max(0.05f, bodyRadius));
            float threatScore = ResolveThreatScore(candidate);
            return angleScore * 0.75f
                + nearBodyScore * 1.5f
                + bodyRayScore * 0.15f
                + threatScore * 0.2f;
        }

        private float ScoreLockTargetCandidate(
            CombatHealth candidate,
            Vector3 originPosition,
            Vector3 viewDirection,
            float maxDistance,
            float maxAngleDegrees,
            float preferredBonus)
        {
            if (!TryResolveAimAssistPoint(candidate, originPosition, viewDirection, out Vector3 aimPoint, out Bounds bounds))
            {
                return float.NegativeInfinity;
            }

            Vector3 offset = Vector3.ProjectOnPlane(aimPoint - originPosition, Vector3.up);
            float distance = offset.magnitude;
            if (distance > maxDistance)
            {
                return float.NegativeInfinity;
            }

            Vector3 direction = distance > 0.0001f ? offset / distance : viewDirection;
            float angle = Vector3.Angle(viewDirection.normalized, direction.normalized);
            if (angle > maxAngleDegrees)
            {
                return float.NegativeInfinity;
            }

            float angleScore = 1f - Mathf.Clamp01(angle / maxAngleDegrees);
            float distanceScore = 1f - Mathf.Clamp01(distance / maxDistance);
            float bodyMissDistance = DistanceFromRayToBounds(originPosition, viewDirection, bounds);
            float bodyRadius = Mathf.Max(0.05f, Mathf.Max(bounds.extents.x, bounds.extents.z));
            float bodyRayScore = 1f - Mathf.Clamp01(bodyMissDistance / Mathf.Max(0.05f, bodyRadius * 1.75f));
            float threatScore = ResolveThreatScore(candidate);
            return angleScore * 1.35f
                + distanceScore * 0.45f
                + bodyRayScore * 0.35f
                + threatScore * 0.25f
                + Mathf.Max(0f, preferredBonus);
        }

        private static bool TryResolveAimAssistPoint(
            CombatHealth candidate,
            Vector3 originPosition,
            Vector3 rawPlanarDirection,
            out Vector3 aimPoint,
            out Bounds bounds)
        {
            aimPoint = default;
            bounds = default;
            if (candidate == null)
            {
                return false;
            }

            if (!TryResolveCombatBounds(candidate, originPosition, rawPlanarDirection, out bounds))
            {
                aimPoint = candidate.transform.position;
                bounds = new Bounds(aimPoint, Vector3.one * 0.1f);
                return true;
            }

            Vector3 direction = ResolvePlanarDirection(rawPlanarDirection, ResolvePlanarForward(candidate.transform));
            aimPoint = bounds.center;
            if ((aimPoint - originPosition).sqrMagnitude <= 0.0001f)
            {
                aimPoint = originPosition + direction;
            }

            return true;
        }

        private static bool TryResolveCombatBounds(
            CombatHealth candidate,
            Vector3 originPosition,
            Vector3 rawPlanarDirection,
            out Bounds bounds)
        {
            bounds = default;
            if (candidate == null)
            {
                return false;
            }

            Collider[] colliders = candidate.GetComponentsInChildren<Collider>();
            float bestMissDistance = float.PositiveInfinity;
            float bestCenterDistance = float.PositiveInfinity;
            bool hasBounds = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (!IsUsableAimAssistCollider(collider, candidate))
                {
                    continue;
                }

                Bounds candidateBounds = collider.bounds;
                float missDistance = DistanceFromRayToBounds(originPosition, rawPlanarDirection, candidateBounds);
                float centerDistance = Vector3.ProjectOnPlane(candidateBounds.center - originPosition, Vector3.up).magnitude;
                if (!hasBounds
                    || missDistance < bestMissDistance - 0.001f
                    || (Mathf.Abs(missDistance - bestMissDistance) <= 0.001f && centerDistance < bestCenterDistance))
                {
                    bounds = candidateBounds;
                    bestMissDistance = missDistance;
                    bestCenterDistance = centerDistance;
                    hasBounds = true;
                }
            }

            return hasBounds;
        }

        private static bool IsUsableAimAssistCollider(Collider collider, CombatHealth candidate)
        {
            return collider != null
                && collider.enabled
                && collider.gameObject.activeInHierarchy
                && collider.GetComponentInParent<SummonPressureScreen>() == null
                && ResolveColliderCombatHealth(collider) == candidate;
        }

        private static CombatHealth ResolveColliderCombatHealth(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            SummonFrontlineProxy proxy = collider.GetComponentInParent<SummonFrontlineProxy>();
            if (proxy != null)
            {
                return proxy.Health ?? collider.GetComponentInParent<CombatHealth>();
            }

            return collider.GetComponentInParent<CombatHealth>();
        }

        private static float DistanceFromRayToBounds(Vector3 rayOrigin, Vector3 rayDirection, Bounds bounds)
        {
            Vector3 direction = ResolvePlanarDirection(rayDirection, Vector3.forward);
            Vector3 toCenter = bounds.center - rayOrigin;
            float projectedDistance = Mathf.Max(0f, Vector3.Dot(toCenter, direction));
            Vector3 closestPointOnRay = rayOrigin + direction * projectedDistance;
            return Mathf.Sqrt(bounds.SqrDistance(closestPointOnRay));
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
