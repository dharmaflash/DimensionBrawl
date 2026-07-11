using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class CombatTimeDilationReceiver : MonoBehaviour
    {
        private static readonly List<CombatTimeDilationReceiver> ActiveReceivers = new();

        [SerializeField, Range(0.02f, 1f)] private float currentTimeScale = 1f;

        private Animator[] animators = Array.Empty<Animator>();
        private float[] authoredAnimatorSpeeds = Array.Empty<float>();
        private float targetTimeScale = 1f;
        private float holdTimer;
        private float blendOutTimer;
        private float blendOutDuration;
        private bool active;
        private bool animatorsCaptured;
        private Coroutine dilationRoutine;

        public bool IsDilationActive => active;
        public float CurrentTimeScale => active ? currentTimeScale : 1f;
        public static IReadOnlyList<CombatTimeDilationReceiver> ActiveInstances => ActiveReceivers;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            ActiveReceivers.Clear();
        }

        public static float ResolveTimeScale(Component owner)
        {
            if (owner == null)
            {
                return 1f;
            }

            if (!owner.TryGetComponent(out CombatTimeDilationReceiver receiver))
            {
                receiver = owner.GetComponentInParent<CombatTimeDilationReceiver>();
            }

            if (receiver == null)
            {
                return 1f;
            }

            return receiver.CurrentTimeScale;
        }

        public static CombatTimeDilationReceiver Ensure(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            return owner.TryGetComponent(out CombatTimeDilationReceiver receiver)
                ? receiver
                : owner.AddComponent<CombatTimeDilationReceiver>();
        }

        public void ApplyTimeDilation(
            float requestedTimeScale,
            float holdSeconds,
            float requestedBlendOutSeconds,
            float intensity01 = 1f)
        {
            float intensity = Mathf.Clamp01(intensity01);
            float resolvedTargetScale = Mathf.Lerp(
                1f,
                Mathf.Clamp(requestedTimeScale, 0.02f, 1f),
                intensity);
            if (resolvedTargetScale >= 0.999f || holdSeconds <= 0f)
            {
                return;
            }

            EnsureAnimatorsCaptured();
            targetTimeScale = active ? Mathf.Min(targetTimeScale, resolvedTargetScale) : resolvedTargetScale;
            currentTimeScale = Mathf.Min(currentTimeScale, targetTimeScale);
            holdTimer = Mathf.Max(holdTimer, holdSeconds);
            blendOutDuration = Mathf.Max(blendOutDuration, requestedBlendOutSeconds);
            blendOutTimer = blendOutDuration;
            active = true;
            ApplyAnimatorSpeed(currentTimeScale);
            EnsureDilationRoutine();
        }

        private void OnEnable()
        {
            if (!ActiveReceivers.Contains(this))
            {
                ActiveReceivers.Add(this);
            }
        }

        private void EnsureDilationRoutine()
        {
            if (!isActiveAndEnabled || !active || dilationRoutine != null)
            {
                return;
            }

            dilationRoutine = StartCoroutine(RunDilation());
        }

        private IEnumerator RunDilation()
        {
            while (active)
            {
                yield return null;
                TickDilation();
            }

            dilationRoutine = null;
        }

        private void TickDilation()
        {
            float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
            if (holdTimer > 0f)
            {
                holdTimer = Mathf.Max(0f, holdTimer - deltaTime);
                currentTimeScale = targetTimeScale;
                ApplyAnimatorSpeed(currentTimeScale);
                return;
            }

            if (blendOutTimer > 0f && blendOutDuration > 0f)
            {
                blendOutTimer = Mathf.Max(0f, blendOutTimer - deltaTime);
                float t = 1f - Mathf.Clamp01(blendOutTimer / blendOutDuration);
                currentTimeScale = Mathf.Lerp(targetTimeScale, 1f, Mathf.SmoothStep(0f, 1f, t));
                ApplyAnimatorSpeed(currentTimeScale);
                return;
            }

            Restore();
        }

        private void OnDisable()
        {
            ActiveReceivers.Remove(this);
            dilationRoutine = null;
            Restore();
        }

        private void OnDestroy()
        {
            ActiveReceivers.Remove(this);
        }

        private void EnsureAnimatorsCaptured()
        {
            if (animatorsCaptured)
            {
                return;
            }

            animatorsCaptured = true;
            animators = GetComponentsInChildren<Animator>(includeInactive: true);
            authoredAnimatorSpeeds = new float[animators.Length];
            for (int i = 0; i < animators.Length; i++)
            {
                authoredAnimatorSpeeds[i] = animators[i] != null ? animators[i].speed : 1f;
            }
        }

        private void ApplyAnimatorSpeed(float scale)
        {
            EnsureAnimatorsCaptured();
            int count = Mathf.Min(animators.Length, authoredAnimatorSpeeds.Length);
            for (int i = 0; i < count; i++)
            {
                Animator animator = animators[i];
                if (animator != null)
                {
                    animator.speed = authoredAnimatorSpeeds[i] * Mathf.Clamp(scale, 0.02f, 1f);
                }
            }
        }

        private void Restore()
        {
            if (!active && Mathf.Approximately(currentTimeScale, 1f))
            {
                return;
            }

            active = false;
            currentTimeScale = 1f;
            targetTimeScale = 1f;
            holdTimer = 0f;
            blendOutTimer = 0f;
            blendOutDuration = 0f;
            int count = Mathf.Min(animators.Length, authoredAnimatorSpeeds.Length);
            for (int i = 0; i < count; i++)
            {
                Animator animator = animators[i];
                if (animator != null)
                {
                    animator.speed = authoredAnimatorSpeeds[i];
                }
            }
        }
    }
}
