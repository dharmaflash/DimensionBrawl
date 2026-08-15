using System;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    /// <summary>
    /// Supplies presentation-only unscaled time. Runtime uses Unity time by default;
    /// deterministic capture can temporarily own a manually sampled clock.
    /// </summary>
    public static class PresentationClock
    {
        private static object manualOwner;
        private static int manualGeneration;
        private static float manualUnscaledTime;
        private static float manualUnscaledDeltaTime;

        public static bool IsManuallyDriven => manualOwner != null;
        public static float UnscaledTime => IsManuallyDriven
            ? manualUnscaledTime
            : Time.unscaledTime;
        public static float UnscaledDeltaTime => IsManuallyDriven
            ? manualUnscaledDeltaTime
            : Time.unscaledDeltaTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            manualOwner = null;
            manualGeneration++;
            manualUnscaledTime = 0f;
            manualUnscaledDeltaTime = 0f;
        }

        public static ManualLease AcquireManual(object owner, int framesPerSecond)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (framesPerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(framesPerSecond));
            }

            if (manualOwner != null)
            {
                throw new InvalidOperationException(
                    "The presentation clock already has a manual owner. Dispose its lease before acquiring another one.");
            }

            manualOwner = owner;
            manualGeneration++;
            manualUnscaledTime = 0f;
            manualUnscaledDeltaTime = 1f / framesPerSecond;
            return new ManualLease(owner, manualGeneration, framesPerSecond);
        }

        private static bool TrySetManualTime(
            object owner,
            int generation,
            float unscaledTime,
            float unscaledDeltaTime)
        {
            if (!ReferenceEquals(manualOwner, owner) || manualGeneration != generation)
            {
                return false;
            }

            ValidateFiniteNonNegative(unscaledTime, nameof(unscaledTime));
            ValidateFiniteNonNegative(unscaledDeltaTime, nameof(unscaledDeltaTime));
            manualUnscaledTime = unscaledTime;
            manualUnscaledDeltaTime = unscaledDeltaTime;
            return true;
        }

        private static void ReleaseManual(object owner, int generation)
        {
            if (!ReferenceEquals(manualOwner, owner) || manualGeneration != generation)
            {
                return;
            }

            manualOwner = null;
            manualGeneration++;
            manualUnscaledTime = 0f;
            manualUnscaledDeltaTime = 0f;
        }

        private static void ValidateFiniteNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public sealed class ManualLease : IDisposable
        {
            private object owner;
            private readonly int generation;
            private readonly int framesPerSecond;

            internal ManualLease(object owner, int generation, int framesPerSecond)
            {
                this.owner = owner;
                this.generation = generation;
                this.framesPerSecond = framesPerSecond;
            }

            public bool IsValid => owner != null
                && ReferenceEquals(manualOwner, owner)
                && manualGeneration == generation;
            public int FramesPerSecond => framesPerSecond;

            public void SetFrame(int frameIndex)
            {
                if (frameIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(frameIndex));
                }

                EnsureValid();
                float frameDuration = 1f / framesPerSecond;
                TrySetManualTime(owner, generation, frameIndex * frameDuration, frameDuration);
            }

            public void SetTime(float unscaledTime, float unscaledDeltaTime)
            {
                EnsureValid();
                TrySetManualTime(owner, generation, unscaledTime, unscaledDeltaTime);
            }

            public void Dispose()
            {
                object releaseOwner = owner;
                owner = null;
                if (releaseOwner != null)
                {
                    ReleaseManual(releaseOwner, generation);
                }
            }

            private void EnsureValid()
            {
                if (!IsValid)
                {
                    throw new ObjectDisposedException(nameof(ManualLease));
                }
            }
        }
    }
}
