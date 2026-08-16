using System;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    /// <summary>
    /// Adds lightweight combat motion on top of Akaza's authored animator pose.
    /// The driver intentionally owns presentation transforms only; it never moves
    /// the canonical combat root, changes time scale, or touches UI state.
    /// </summary>
    [DisallowMultipleComponent]
    // ActionFoundationArenaAnimationScheduler owns its authored bob at 10000.
    // This additive reaction layer must sample and decorate that final base pose.
    [DefaultExecutionOrder(10100)]
    public sealed class AkazaPhase2CombatMotionDriver : MonoBehaviour
    {
        private const float TwoPi = Mathf.PI * 2f;

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private Transform motionRoot;
        [SerializeField] private Transform[] wingRoots = Array.Empty<Transform>();
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossBasicFireEmitter bossBasicFireEmitter;

        [Header("Animator Triggers")]
        [SerializeField] private string heavyReleaseTrigger = "AttackHeavy";
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deathTrigger = "Death";

        [Header("Hover")]
        [SerializeField, Min(0.1f)] private float hoverPeriodSeconds = 2.4f;
        [SerializeField, Min(0f)] private float hoverHeight = 0.022f;
        [SerializeField, Min(0f)] private float hoverPitchDegrees = 0.65f;
        [SerializeField, Min(0f)] private float wingSwayDegrees = 3.6f;

        [Header("Heavy Release")]
        [SerializeField, Min(0.01f)] private float heavyReleaseSeconds = 0.34f;
        [SerializeField, Min(0f)] private float heavyRootRecoilDistance = 0.055f;
        [SerializeField, Min(0f)] private float heavyWingFlareDegrees = 3.5f;

        [Header("Hit Reaction")]
        [SerializeField, Min(0.01f)] private float hitReactionSeconds = 0.16f;
        [SerializeField, Min(0f)] private float hitRootRecoilDistance = 0.075f;
        [SerializeField, Min(0f)] private float hitPitchDegrees = 3.5f;
        [SerializeField, Min(0f)] private float hitWingKickDegrees = 7f;

        [Header("Lethal Settle")]
        [SerializeField, Min(0.01f)] private float deathSettleSeconds = 0.9f;
        [SerializeField, Min(0f)] private float deathDropDistance = 0.32f;
        [SerializeField, Min(0f)] private float deathBackDistance = 0.1f;
        [SerializeField] private float deathPitchDegrees = 18f;
        [SerializeField, Min(0f)] private float deathWingFoldDegrees = 32f;
        [SerializeField, Min(0f)] private float deathWingYawDegrees = 12f;

        private Quaternion[] originalWingLocalRotations = Array.Empty<Quaternion>();
        private Quaternion[] lastBaseWingLocalRotations = Array.Empty<Quaternion>();
        private Quaternion[] lastAppliedWingLocalRotations = Array.Empty<Quaternion>();
        private Vector3 originalRootLocalPosition;
        private Quaternion originalRootLocalRotation = Quaternion.identity;
        private Vector3 lastBaseRootLocalPosition;
        private Quaternion lastBaseRootLocalRotation = Quaternion.identity;
        private Vector3 lastAppliedRootLocalPosition;
        private Quaternion lastAppliedRootLocalRotation = Quaternion.identity;
        private float hoverClock;
        private float heavyReleaseRemaining;
        private float hitReactionRemaining;
        private float deathElapsed;
        private float lastWingOffsetDegrees;
        private bool poseCaptured;
        private bool poseApplied;
        private bool subscribed;
        private bool emitterSubscribed;
        private bool dead;
        private bool attacksStopped;
        private bool lastAnimatorTriggerAccepted;
        private int capturedWingCount;
        private int heavyReleaseRequestCount;
        private int hitReactionRequestCount;
        private int deathRequestCount;
        private int animatorTriggerAcceptedCount;
        private string lastAnimatorTrigger = string.Empty;

        public Animator Animator => animator;
        public CombatHealth BossHealth => bossHealth;
        public Transform MotionRoot => motionRoot;
        public BossBarrageEmitter BossBarrageEmitter => bossBarrageEmitter;
        public BossBasicFireEmitter BossBasicFireEmitter => bossBasicFireEmitter;
        public string HeavyReleaseTrigger => heavyReleaseTrigger;
        public bool OriginalPoseCaptured => poseCaptured;
        public int ConfiguredWingCount => wingRoots != null ? wingRoots.Length : 0;
        public int CapturedWingCount => capturedWingCount;
        public bool IsHeavyReleaseActive => !dead && heavyReleaseRemaining > 0f;
        public bool IsHitReactionActive => !dead && hitReactionRemaining > 0f;
        public bool IsDead => dead;
        public bool AttacksStopped => attacksStopped;
        public float DeathProgress01 => dead
            ? Mathf.Clamp01(deathElapsed / Mathf.Max(0.01f, deathSettleSeconds))
            : 0f;
        public float DeathSettleDurationSeconds => deathSettleSeconds;
        public int HeavyReleaseRequestCount => heavyReleaseRequestCount;
        public int HitReactionRequestCount => hitReactionRequestCount;
        public int DeathRequestCount => deathRequestCount;
        public int AnimatorTriggerAcceptedCount => animatorTriggerAcceptedCount;
        public string LastAnimatorTrigger => lastAnimatorTrigger;
        public bool LastAnimatorTriggerAccepted => lastAnimatorTriggerAccepted;
        public Vector3 LastAppliedRootOffset => lastAppliedRootLocalPosition - lastBaseRootLocalPosition;
        public float LastWingOffsetDegrees => lastWingOffsetDegrees;

        public void Configure(
            Animator newAnimator,
            CombatHealth newBossHealth,
            Transform newMotionRoot,
            Transform[] newWingRoots,
            BossBarrageEmitter newBossBarrageEmitter,
            BossBasicFireEmitter newBossBasicFireEmitter)
        {
            UnsubscribeEvents();
            RestoreOriginalPose();

            animator = newAnimator;
            bossHealth = newBossHealth;
            motionRoot = newMotionRoot;
            wingRoots = newWingRoots != null
                ? (Transform[])newWingRoots.Clone()
                : Array.Empty<Transform>();
            bossBarrageEmitter = newBossBarrageEmitter;
            bossBasicFireEmitter = newBossBasicFireEmitter;

            ResolveReferences();
            CaptureOriginalPose();
            ResetTransientMotion();

            if (isActiveAndEnabled)
            {
                SubscribeEvents();
                if (bossHealth != null && !bossHealth.IsAlive)
                {
                    PlayDeath();
                }
            }
        }

        /// <summary>
        /// Requests the C27-backed heavy state through the canonical AttackHeavy
        /// parameter. A missing controller or trigger is a safe, observable miss;
        /// procedural release recoil still plays so the gameplay cue is readable.
        /// </summary>
        public bool PlayHeavyRelease()
        {
            if (dead)
            {
                return false;
            }

            BeginHeavyReleasePresentation();
            lastAnimatorTriggerAccepted = TryTriggerAnimator(heavyReleaseTrigger);
            return lastAnimatorTriggerAccepted;
        }

        public bool PlayHitReaction()
        {
            return PlayHitReaction(triggerFullBodyAnimator: true);
        }

        public void PlayDeath()
        {
            if (dead)
            {
                StopAttacks();
                return;
            }

            dead = true;
            deathRequestCount++;
            deathElapsed = 0f;
            heavyReleaseRemaining = 0f;
            hitReactionRemaining = 0f;
            StopAttacks();
            lastAnimatorTriggerAccepted = TryTriggerAnimator(deathTrigger);
        }

        /// <summary>
        /// Advances only presentation state. LateUpdate supplies the shared presentation
        /// delta time; the public entry point also keeps focused tests and custom schedulers
        /// deterministic.
        /// </summary>
        public void TickPresentation(float unscaledDeltaTime)
        {
            if (!poseCaptured)
            {
                return;
            }

            float deltaTime = Mathf.Max(0f, unscaledDeltaTime);
            if (dead)
            {
                deathElapsed = Mathf.Min(
                    Mathf.Max(0.01f, deathSettleSeconds),
                    deathElapsed + deltaTime);
            }
            else
            {
                hoverClock = Mathf.Repeat(
                    hoverClock + deltaTime,
                    Mathf.Max(0.1f, hoverPeriodSeconds));
                heavyReleaseRemaining = Mathf.Max(0f, heavyReleaseRemaining - deltaTime);
                hitReactionRemaining = Mathf.Max(0f, hitReactionRemaining - deltaTime);
            }

            ApplyPresentationPose();
        }

        /// <summary>
        /// Re-captures the currently evaluated hold pose. This allocates only when
        /// wiring changes, never from LateUpdate.
        /// </summary>
        public void CaptureOriginalPose()
        {
            RemoveLastAppliedOffsets();
            ResolveReferences();
            if (motionRoot == null)
            {
                poseCaptured = false;
                capturedWingCount = 0;
                return;
            }

            originalRootLocalPosition = motionRoot.localPosition;
            originalRootLocalRotation = motionRoot.localRotation;
            lastBaseRootLocalPosition = originalRootLocalPosition;
            lastBaseRootLocalRotation = originalRootLocalRotation;
            lastAppliedRootLocalPosition = originalRootLocalPosition;
            lastAppliedRootLocalRotation = originalRootLocalRotation;

            int wingCount = wingRoots != null ? wingRoots.Length : 0;
            originalWingLocalRotations = new Quaternion[wingCount];
            lastBaseWingLocalRotations = new Quaternion[wingCount];
            lastAppliedWingLocalRotations = new Quaternion[wingCount];
            capturedWingCount = 0;
            for (int i = 0; i < wingCount; i++)
            {
                Transform wingRoot = wingRoots[i];
                Quaternion localRotation = wingRoot != null
                    ? wingRoot.localRotation
                    : Quaternion.identity;
                originalWingLocalRotations[i] = localRotation;
                lastBaseWingLocalRotations[i] = localRotation;
                lastAppliedWingLocalRotations[i] = localRotation;
                if (wingRoot != null)
                {
                    capturedWingCount++;
                }
            }

            poseCaptured = true;
            poseApplied = false;
        }

        public void RestoreOriginalPose()
        {
            if (!poseCaptured)
            {
                return;
            }

            if (motionRoot != null)
            {
                motionRoot.localPosition = originalRootLocalPosition;
                motionRoot.localRotation = originalRootLocalRotation;
            }

            int count = Mathf.Min(
                wingRoots != null ? wingRoots.Length : 0,
                originalWingLocalRotations.Length);
            for (int i = 0; i < count; i++)
            {
                if (wingRoots[i] != null)
                {
                    wingRoots[i].localRotation = originalWingLocalRotations[i];
                }

                lastBaseWingLocalRotations[i] = originalWingLocalRotations[i];
                lastAppliedWingLocalRotations[i] = originalWingLocalRotations[i];
            }

            lastBaseRootLocalPosition = originalRootLocalPosition;
            lastBaseRootLocalRotation = originalRootLocalRotation;
            lastAppliedRootLocalPosition = originalRootLocalPosition;
            lastAppliedRootLocalRotation = originalRootLocalRotation;
            lastWingOffsetDegrees = 0f;
            poseApplied = false;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CaptureOriginalPose();
            ResetTransientMotion();
            SubscribeEvents();

            if (bossHealth != null && !bossHealth.IsAlive)
            {
                PlayDeath();
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            RestoreOriginalPose();
            ResetTransientMotion();
        }

        private void LateUpdate()
        {
            TickPresentation(PresentationClock.UnscaledDeltaTime);
        }

        private void OnValidate()
        {
            wingRoots ??= Array.Empty<Transform>();
            hoverPeriodSeconds = Mathf.Max(0.1f, hoverPeriodSeconds);
            hoverHeight = Mathf.Max(0f, hoverHeight);
            hoverPitchDegrees = Mathf.Max(0f, hoverPitchDegrees);
            wingSwayDegrees = Mathf.Max(0f, wingSwayDegrees);
            heavyReleaseSeconds = Mathf.Max(0.01f, heavyReleaseSeconds);
            heavyRootRecoilDistance = Mathf.Max(0f, heavyRootRecoilDistance);
            heavyWingFlareDegrees = Mathf.Max(0f, heavyWingFlareDegrees);
            hitReactionSeconds = Mathf.Max(0.01f, hitReactionSeconds);
            hitRootRecoilDistance = Mathf.Max(0f, hitRootRecoilDistance);
            hitPitchDegrees = Mathf.Max(0f, hitPitchDegrees);
            hitWingKickDegrees = Mathf.Max(0f, hitWingKickDegrees);
            deathSettleSeconds = Mathf.Max(0.01f, deathSettleSeconds);
            deathDropDistance = Mathf.Max(0f, deathDropDistance);
            deathBackDistance = Mathf.Max(0f, deathBackDistance);
            deathWingFoldDegrees = Mathf.Max(0f, deathWingFoldDegrees);
            deathWingYawDegrees = Mathf.Max(0f, deathWingYawDegrees);
        }

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(includeInactive: true);
            }

            if (bossHealth == null)
            {
                bossHealth = GetComponentInParent<CombatHealth>();
            }

            if (motionRoot == null)
            {
                motionRoot = animator != null ? animator.transform : transform;
            }

            if (bossBarrageEmitter == null)
            {
                bossBarrageEmitter = GetComponentInParent<BossBarrageEmitter>();
            }

            if (bossBasicFireEmitter == null)
            {
                bossBasicFireEmitter = GetComponentInParent<BossBasicFireEmitter>();
            }
        }

        private void SubscribeEvents()
        {
            if (!subscribed && bossHealth != null)
            {
                bossHealth.Damaged += HandleDamaged;
                bossHealth.Died += HandleDied;
                subscribed = true;
            }

            if (!emitterSubscribed && bossBarrageEmitter != null)
            {
                bossBarrageEmitter.WindupStarted += HandleBarrageWindupStarted;
                emitterSubscribed = true;
            }
        }

        private void UnsubscribeEvents()
        {
            if (!subscribed && !emitterSubscribed)
            {
                return;
            }

            if (bossHealth != null)
            {
                bossHealth.Damaged -= HandleDamaged;
                bossHealth.Died -= HandleDied;
            }

            subscribed = false;
            if (emitterSubscribed && bossBarrageEmitter != null)
            {
                bossBarrageEmitter.WindupStarted -= HandleBarrageWindupStarted;
            }

            emitterSubscribed = false;
        }

        private void HandleBarrageWindupStarted(
            BossBarrageEmitter source,
            BossBarragePatternProfile pattern)
        {
            if (!dead)
            {
                BeginHeavyReleasePresentation();
            }
        }

        private void BeginHeavyReleasePresentation()
        {
            heavyReleaseRequestCount++;
            heavyReleaseRemaining = Mathf.Max(0.01f, heavyReleaseSeconds);
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (bossHealth != null && bossHealth.CurrentHealth <= 0f)
            {
                return;
            }

            PlayHitReaction(DamageResponsePolicyUtility.PlaysFullBodyHitAnimation(damageInfo));
        }

        private void HandleDied()
        {
            PlayDeath();
        }

        private bool PlayHitReaction(bool triggerFullBodyAnimator)
        {
            if (dead)
            {
                return false;
            }

            hitReactionRequestCount++;
            hitReactionRemaining = Mathf.Max(0.01f, hitReactionSeconds);
            if (triggerFullBodyAnimator)
            {
                lastAnimatorTriggerAccepted = TryTriggerAnimator(hitTrigger);
            }

            return true;
        }

        private void StopAttacks()
        {
            if (bossBarrageEmitter != null)
            {
                bossBarrageEmitter.SetFiringEnabled(false);
            }

            if (bossBasicFireEmitter != null)
            {
                bossBasicFireEmitter.SetFiringEnabled(false);
            }

            attacksStopped = (bossBarrageEmitter == null || !bossBarrageEmitter.IsFiringEnabled)
                && (bossBasicFireEmitter == null || !bossBasicFireEmitter.IsFiringEnabled);
        }

        private bool TryTriggerAnimator(string triggerName)
        {
            lastAnimatorTrigger = triggerName ?? string.Empty;
            if (animator == null
                || animator.runtimeAnimatorController == null
                || string.IsNullOrWhiteSpace(triggerName))
            {
                return false;
            }

            int triggerHash = Animator.StringToHash(triggerName);
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type != AnimatorControllerParameterType.Trigger
                    || parameter.nameHash != triggerHash)
                {
                    continue;
                }

                animator.ResetTrigger(triggerHash);
                animator.SetTrigger(triggerHash);
                animatorTriggerAcceptedCount++;
                return true;
            }

            return false;
        }

        private void ResetTransientMotion()
        {
            hoverClock = 0f;
            heavyReleaseRemaining = 0f;
            hitReactionRemaining = 0f;
            deathElapsed = 0f;
            dead = false;
            attacksStopped = false;
            lastWingOffsetDegrees = 0f;
        }

        private void ApplyPresentationPose()
        {
            Vector3 baseRootPosition = motionRoot.localPosition;
            Quaternion baseRootRotation = motionRoot.localRotation;
            if (poseApplied
                && Approximately(baseRootPosition, lastAppliedRootLocalPosition)
                && Approximately(baseRootRotation, lastAppliedRootLocalRotation))
            {
                baseRootPosition = lastBaseRootLocalPosition;
                baseRootRotation = lastBaseRootLocalRotation;
            }

            float period = Mathf.Max(0.1f, hoverPeriodSeconds);
            float hoverAngle = hoverClock / period * TwoPi;
            float hoverWave = Mathf.Sin(hoverAngle);
            float heavyEnvelope = ResolvePulseEnvelope(heavyReleaseRemaining, heavyReleaseSeconds);
            float hitEnvelope = ResolvePulseEnvelope(hitReactionRemaining, hitReactionSeconds);
            float deathProgress = DeathProgress01;
            float deathEase = deathProgress * deathProgress * (3f - 2f * deathProgress);
            float hoverWeight = dead ? 0f : 1f - Mathf.Clamp01(Mathf.Max(heavyEnvelope, hitEnvelope));

            Vector3 rootOffset;
            Quaternion rootOffsetRotation;
            if (dead)
            {
                rootOffset = Vector3.down * (deathDropDistance * deathEase)
                    + Vector3.back * (deathBackDistance * deathEase);
                rootOffsetRotation = Quaternion.Euler(deathPitchDegrees * deathEase, 0f, 0f);
            }
            else
            {
                rootOffset = Vector3.up * (hoverWave * hoverHeight * hoverWeight)
                    + Vector3.back * (
                        heavyRootRecoilDistance * heavyEnvelope
                        + hitRootRecoilDistance * hitEnvelope);
                rootOffsetRotation = Quaternion.Euler(
                    hoverWave * hoverPitchDegrees * hoverWeight + hitPitchDegrees * hitEnvelope,
                    0f,
                    0f);
            }

            Vector3 appliedRootPosition = baseRootPosition + rootOffset;
            Quaternion appliedRootRotation = baseRootRotation * rootOffsetRotation;
            motionRoot.SetLocalPositionAndRotation(appliedRootPosition, appliedRootRotation);
            lastBaseRootLocalPosition = baseRootPosition;
            lastBaseRootLocalRotation = baseRootRotation;
            lastAppliedRootLocalPosition = appliedRootPosition;
            lastAppliedRootLocalRotation = appliedRootRotation;

            int count = Mathf.Min(
                wingRoots != null ? wingRoots.Length : 0,
                lastAppliedWingLocalRotations.Length);
            lastWingOffsetDegrees = 0f;
            for (int i = 0; i < count; i++)
            {
                Transform wingRoot = wingRoots[i];
                if (wingRoot == null)
                {
                    continue;
                }

                Quaternion baseWingRotation = wingRoot.localRotation;
                if (poseApplied
                    && Approximately(baseWingRotation, lastAppliedWingLocalRotations[i]))
                {
                    baseWingRotation = lastBaseWingLocalRotations[i];
                }

                float side = (i & 1) == 0 ? -1f : 1f;
                float phaseOffset = count > 0 ? TwoPi * i / count : 0f;
                float sway = Mathf.Sin(hoverAngle + phaseOffset)
                    * wingSwayDegrees
                    * hoverWeight;
                float pitch;
                float yaw;
                float roll;
                if (dead)
                {
                    pitch = deathWingFoldDegrees * deathEase;
                    yaw = side * deathWingYawDegrees * deathEase;
                    roll = side * deathWingFoldDegrees * 0.35f * deathEase;
                }
                else
                {
                    pitch = hitWingKickDegrees * hitEnvelope;
                    yaw = side * (heavyWingFlareDegrees * heavyEnvelope);
                    roll = sway + side * hitWingKickDegrees * 0.45f * hitEnvelope;
                }

                Quaternion wingOffset = Quaternion.Euler(pitch, yaw, roll);
                Quaternion appliedWingRotation = baseWingRotation * wingOffset;
                wingRoot.localRotation = appliedWingRotation;
                lastBaseWingLocalRotations[i] = baseWingRotation;
                lastAppliedWingLocalRotations[i] = appliedWingRotation;
                lastWingOffsetDegrees = Mathf.Max(
                    lastWingOffsetDegrees,
                    Mathf.Max(Mathf.Abs(pitch), Mathf.Max(Mathf.Abs(yaw), Mathf.Abs(roll))));
            }

            poseApplied = true;
        }

        private void RemoveLastAppliedOffsets()
        {
            if (!poseApplied)
            {
                return;
            }

            if (motionRoot != null
                && Approximately(motionRoot.localPosition, lastAppliedRootLocalPosition)
                && Approximately(motionRoot.localRotation, lastAppliedRootLocalRotation))
            {
                motionRoot.SetLocalPositionAndRotation(
                    lastBaseRootLocalPosition,
                    lastBaseRootLocalRotation);
            }

            int count = Mathf.Min(
                wingRoots != null ? wingRoots.Length : 0,
                lastAppliedWingLocalRotations.Length);
            for (int i = 0; i < count; i++)
            {
                Transform wingRoot = wingRoots[i];
                if (wingRoot != null
                    && Approximately(wingRoot.localRotation, lastAppliedWingLocalRotations[i]))
                {
                    wingRoot.localRotation = lastBaseWingLocalRotations[i];
                }
            }

            poseApplied = false;
        }

        private static float ResolvePulseEnvelope(float remainingSeconds, float durationSeconds)
        {
            if (remainingSeconds <= 0f)
            {
                return 0f;
            }

            float progress = 1f - Mathf.Clamp01(remainingSeconds / Mathf.Max(0.01f, durationSeconds));
            return Mathf.Sin(progress * Mathf.PI);
        }

        private static bool Approximately(Vector3 first, Vector3 second)
        {
            return (first - second).sqrMagnitude <= 0.0000001f;
        }

        private static bool Approximately(Quaternion first, Quaternion second)
        {
            return Mathf.Abs(Quaternion.Dot(first, second)) >= 0.99999f;
        }
    }
}
