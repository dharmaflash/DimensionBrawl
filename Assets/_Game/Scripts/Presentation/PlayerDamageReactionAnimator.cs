using System.Collections;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerDamageReactionAnimator : MonoBehaviour
    {
        private const float MinimumRecoilReturnSeconds = 0.035f;

        [Header("References")]
        [SerializeField] private CombatHealth health;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform recoilRoot;

        [Header("Animator")]
        [SerializeField] private string hitUpperTrigger = "HIT UPPER";
        [SerializeField] private string hitLowTrigger = "HIT LOW";
        [SerializeField] private bool crossFadeHitStates = true;
        [SerializeField] private string hitUpperStateName = "R_Hit_Upper";
        [SerializeField] private string hitLowStateName = "R_Hit_Low";
        [SerializeField, Min(0f)] private float hitCrossFadeSeconds = 0.035f;
        [SerializeField, Min(0)] private int hitAnimationLayer = 0;
        [SerializeField] private string deathFrontTrigger = "DIE F";
        [SerializeField] private string deathBackTrigger = "DIE B";
        [SerializeField, Min(0f)] private float hitReactionCooldownSeconds = 0.12f;
        [SerializeField] private bool heavyHitBypassesCooldown = true;

        [Header("Recoil")]
        [SerializeField] private bool playLocalRecoil = true;
        [SerializeField, Min(0f)] private float lightRecoilDistance = 0.07f;
        [SerializeField, Min(0f)] private float heavyRecoilDistance = 0.13f;
        [SerializeField, Min(0f)] private float lightRecoilDegrees = 4.2f;
        [SerializeField, Min(0f)] private float heavyRecoilDegrees = 8.4f;
        [SerializeField, Min(0f)] private float recoilReturnSeconds = 0.14f;

        private bool subscribed;
        private float nextHitReactionTime;
        private Coroutine recoilRoutine;
        private Transform activeRecoilRoot;
        private Vector3 recoilBaseLocalPosition;
        private Quaternion recoilBaseLocalRotation;
        private bool recoilBaseCaptured;
        private DamageInfo lastDamageInfo;
        private bool hasLastDamageInfo;
        private int hitReactionTriggerCount;
        private int deathReactionTriggerCount;
        private int localRecoilRequestCount;

        public int HitReactionTriggerCount => hitReactionTriggerCount;
        public int DeathReactionTriggerCount => deathReactionTriggerCount;
        public int LocalRecoilRequestCount => localRecoilRequestCount;

        public void Configure(CombatHealth newHealth, Animator newAnimator, Transform newRecoilRoot)
        {
            Unsubscribe();
            health = newHealth;
            animator = newAnimator;
            recoilRoot = newRecoilRoot;
            Subscribe();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
                recoilRoutine = null;
            }

            RestoreRecoilRoot();
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                health = GetComponent<CombatHealth>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (recoilRoot == null && animator != null)
            {
                recoilRoot = animator.transform;
            }
        }

        private void Subscribe()
        {
            if (subscribed || !isActiveAndEnabled || health == null)
            {
                return;
            }

            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || health == null)
            {
                subscribed = false;
                return;
            }

            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
            subscribed = false;
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (!ShouldPlayDamageReaction(damageInfo))
            {
                return;
            }

            lastDamageInfo = damageInfo;
            hasLastDamageInfo = true;
            bool heavyReaction = IsHeavyReaction(damageInfo);
            TryTriggerHitReaction(damageInfo, heavyReaction);
            RequestLocalRecoil(damageInfo, heavyReaction);
        }

        private void HandleDied()
        {
            string trigger = ResolveDeathTrigger();
            if (TriggerAnimator(trigger))
            {
                deathReactionTriggerCount++;
            }
        }

        private void TryTriggerHitReaction(DamageInfo damageInfo, bool heavyReaction)
        {
            bool bypassCooldown = heavyHitBypassesCooldown && heavyReaction;
            float now = Time.unscaledTime;
            if (!bypassCooldown && now < nextHitReactionTime)
            {
                return;
            }

            string trigger = ResolveHitTrigger(damageInfo, heavyReaction);
            bool triggered = TriggerAnimator(trigger);
            bool playedState = TryCrossFadeHitState(trigger);
            if (!triggered && !playedState)
            {
                return;
            }

            hitReactionTriggerCount++;
            nextHitReactionTime = now + Mathf.Max(0f, hitReactionCooldownSeconds);
        }

        private string ResolveHitTrigger(DamageInfo damageInfo, bool heavyReaction)
        {
            if (heavyReaction)
            {
                return hitUpperTrigger;
            }

            Transform basis = animator != null ? animator.transform : transform;
            Vector3 localHitPoint = basis.InverseTransformPoint(damageInfo.Point);
            return localHitPoint.y < 0.78f ? hitLowTrigger : hitUpperTrigger;
        }

        private string ResolveDeathTrigger()
        {
            if (!hasLastDamageInfo)
            {
                return deathFrontTrigger;
            }

            Transform basis = animator != null ? animator.transform : transform;
            Vector3 incoming = ResolvePlanarDirection(lastDamageInfo.Direction);
            float facingDot = Vector3.Dot(incoming, basis.forward);
            return facingDot > 0.2f ? deathBackTrigger : deathFrontTrigger;
        }

        private bool TriggerAnimator(string trigger)
        {
            if (animator == null || string.IsNullOrEmpty(trigger) || !HasAnimatorTrigger(trigger))
            {
                return false;
            }

            animator.ResetTrigger(trigger);
            animator.SetTrigger(trigger);
            return true;
        }

        private bool TryCrossFadeHitState(string trigger)
        {
            if (!crossFadeHitStates || animator == null)
            {
                return false;
            }

            string stateName = string.Equals(trigger, hitLowTrigger, System.StringComparison.Ordinal)
                ? hitLowStateName
                : hitUpperStateName;
            if (string.IsNullOrEmpty(stateName) || animator.layerCount <= 0)
            {
                return false;
            }

            int layer = Mathf.Clamp(hitAnimationLayer, 0, animator.layerCount - 1);
            int stateHash = Animator.StringToHash(stateName);
            if (!animator.HasState(layer, stateHash))
            {
                return false;
            }

            animator.CrossFadeInFixedTime(stateHash, Mathf.Max(0f, hitCrossFadeSeconds), layer, 0f);
            return true;
        }

        private bool HasAnimatorTrigger(string parameterName)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, parameterName, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void RequestLocalRecoil(DamageInfo damageInfo, bool heavyReaction)
        {
            if (!playLocalRecoil)
            {
                return;
            }

            Transform targetRoot = recoilRoot != null ? recoilRoot : (animator != null ? animator.transform : null);
            if (targetRoot == null)
            {
                return;
            }

            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
                RestoreRecoilRoot();
            }

            CaptureRecoilRoot(targetRoot);
            Vector3 localDirection = ResolveLocalRecoilDirection(targetRoot, damageInfo.Direction);
            float distance = heavyReaction ? heavyRecoilDistance : lightRecoilDistance;
            float degrees = heavyReaction ? heavyRecoilDegrees : lightRecoilDegrees;
            targetRoot.localPosition = recoilBaseLocalPosition + localDirection * distance;
            targetRoot.localRotation = recoilBaseLocalRotation * Quaternion.Euler(
                -Mathf.Abs(localDirection.z) * degrees * 0.45f,
                localDirection.x * degrees,
                -localDirection.x * degrees * 0.35f);
            recoilRoutine = StartCoroutine(ReturnLocalRecoil(targetRoot, recoilBaseLocalPosition, recoilBaseLocalRotation));
            localRecoilRequestCount++;
        }

        private void CaptureRecoilRoot(Transform targetRoot)
        {
            if (activeRecoilRoot == targetRoot && recoilBaseCaptured)
            {
                return;
            }

            activeRecoilRoot = targetRoot;
            recoilBaseLocalPosition = targetRoot.localPosition;
            recoilBaseLocalRotation = targetRoot.localRotation;
            recoilBaseCaptured = true;
        }

        private IEnumerator ReturnLocalRecoil(Transform targetRoot, Vector3 basePosition, Quaternion baseRotation)
        {
            float duration = Mathf.Max(MinimumRecoilReturnSeconds, recoilReturnSeconds);
            float elapsed = 0f;
            Vector3 startPosition = targetRoot.localPosition;
            Quaternion startRotation = targetRoot.localRotation;

            while (elapsed < duration && targetRoot != null)
            {
                float weight = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                targetRoot.localPosition = Vector3.Lerp(startPosition, basePosition, weight);
                targetRoot.localRotation = Quaternion.Slerp(startRotation, baseRotation, weight);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (targetRoot != null)
            {
                targetRoot.localPosition = basePosition;
                targetRoot.localRotation = baseRotation;
            }

            recoilRoutine = null;
        }

        private void RestoreRecoilRoot()
        {
            if (!recoilBaseCaptured || activeRecoilRoot == null)
            {
                return;
            }

            activeRecoilRoot.localPosition = recoilBaseLocalPosition;
            activeRecoilRoot.localRotation = recoilBaseLocalRotation;
        }

        private Vector3 ResolveLocalRecoilDirection(Transform targetRoot, Vector3 damageDirection)
        {
            Vector3 worldDirection = -ResolvePlanarDirection(damageDirection);
            if (targetRoot.parent != null)
            {
                worldDirection = targetRoot.parent.InverseTransformDirection(worldDirection);
            }

            worldDirection.y = 0f;
            return worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : Vector3.back;
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private static bool IsHeavyReaction(DamageInfo damageInfo)
        {
            return damageInfo.ResponsePolicy == DamageResponsePolicy.Stagger
                || damageInfo.ResponsePolicy == DamageResponsePolicy.Break
                || damageInfo.ResponsePolicy == DamageResponsePolicy.Knockdown
                || DamageResponsePolicyUtility.PlaysFullBodyHitAnimation(damageInfo);
        }

        private static bool ShouldPlayDamageReaction(DamageInfo damageInfo)
        {
            return damageInfo.Amount > 0f
                && DamageResponsePolicyUtility.PlaysDamagePresentation(damageInfo.ResponsePolicy);
        }
    }
}
