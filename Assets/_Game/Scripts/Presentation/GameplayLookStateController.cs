using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Presentation
{
    public enum GameplayLookState
    {
        GameplayBase = 0,
        CharacterFocus = 100,
        Phase2Cinematic = 200,
        CombatImpact = 300,
        Finisher = 400,
    }

    /// <summary>
    /// Owns global post-process overlays without mutating shared VolumeProfiles.
    /// GameplayBase is the authored environment Volume and is never modified;
    /// temporary looks are exclusive, owner-scoped leases that only drive weight.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-180)]
    public sealed class GameplayLookStateController : MonoBehaviour
    {
        [Serializable]
        public struct OverlayBinding
        {
            [SerializeField] private GameplayLookState state;
            [SerializeField] private Volume volume;
            [SerializeField, Min(0f)] private float blendInSeconds;
            [SerializeField, Min(0f)] private float blendOutSeconds;

            public OverlayBinding(
                GameplayLookState state,
                Volume volume,
                float blendInSeconds,
                float blendOutSeconds)
            {
                this.state = state;
                this.volume = volume;
                this.blendInSeconds = Mathf.Max(0f, blendInSeconds);
                this.blendOutSeconds = Mathf.Max(0f, blendOutSeconds);
            }

            public GameplayLookState State => state;
            public Volume Volume => volume;
            public float BlendInSeconds => Mathf.Max(0f, blendInSeconds);
            public float BlendOutSeconds => Mathf.Max(0f, blendOutSeconds);
        }

        public sealed class LookLease : IDisposable
        {
            private GameplayLookStateController controller;
            private readonly int leaseId;
            private readonly uint generation;
            private readonly GameplayLookState state;

            internal LookLease(
                GameplayLookStateController sourceController,
                int sourceLeaseId,
                uint sourceGeneration,
                GameplayLookState sourceState)
            {
                controller = sourceController;
                leaseId = sourceLeaseId;
                generation = sourceGeneration;
                state = sourceState;
            }

            public bool IsValid => controller != null
                && controller.IsLeaseActive(leaseId, generation);
            public GameplayLookState State => state;

            public void Dispose()
            {
                GameplayLookStateController source = controller;
                controller = null;
                source?.ReleaseLease(leaseId, generation);
            }
        }

        [SerializeField] private Volume gameplayBaseVolume;
        [SerializeField] private OverlayBinding[] overlayBindings =
            Array.Empty<OverlayBinding>();

        private readonly List<LeaseRecord> activeLeases = new List<LeaseRecord>(4);
        private GameplayLookState currentState = GameplayLookState.GameplayBase;
        private int nextLeaseId = 1;
        private uint leaseGeneration = 1;

        public Volume GameplayBaseVolume => gameplayBaseVolume;
        public GameplayLookState CurrentState => currentState;
        public int ActiveLeaseCount => activeLeases.Count;
        public int OverlayBindingCount => overlayBindings?.Length ?? 0;

        public void Configure(
            Volume sourceGameplayBaseVolume,
            OverlayBinding[] sourceOverlayBindings)
        {
            ResetImmediate();
            gameplayBaseVolume = sourceGameplayBaseVolume;
            overlayBindings = sourceOverlayBindings != null
                ? (OverlayBinding[])sourceOverlayBindings.Clone()
                : Array.Empty<OverlayBinding>();
            ResetOverlayWeightsImmediate();
        }

        public bool TryAcquire(
            GameplayLookState state,
            UnityEngine.Object owner,
            out LookLease lease)
        {
            lease = null;
            if (!isActiveAndEnabled
                || owner == null
                || state == GameplayLookState.GameplayBase
                || !IsConfigurationValid()
                || !TryGetBinding(state, out _))
            {
                return false;
            }

            int leaseId = nextLeaseId++;
            if (nextLeaseId <= 0)
            {
                nextLeaseId = 1;
            }

            activeLeases.Add(new LeaseRecord(leaseId, state, owner));
            RecalculateWinner();
            ApplyOverlayWeights(0f);
            lease = new LookLease(this, leaseId, leaseGeneration, state);
            return true;
        }

        public int ReleaseAllOwnedBy(UnityEngine.Object owner)
        {
            if (owner == null)
            {
                return 0;
            }

            int removed = 0;
            for (int index = activeLeases.Count - 1; index >= 0; index--)
            {
                if (activeLeases[index].Owner == owner)
                {
                    activeLeases.RemoveAt(index);
                    removed++;
                }
            }

            if (removed > 0)
            {
                RecalculateWinner();
                ApplyOverlayWeights(0f);
            }

            return removed;
        }

        public bool HasBinding(GameplayLookState state)
        {
            return IsConfigurationValid() && TryGetBinding(state, out _);
        }

        public Volume GetOverlayVolume(GameplayLookState state)
        {
            return IsConfigurationValid()
                && TryGetBinding(state, out OverlayBinding binding)
                ? binding.Volume
                : null;
        }

        public void ResetImmediate()
        {
            activeLeases.Clear();
            currentState = GameplayLookState.GameplayBase;
            leaseGeneration++;
            if (leaseGeneration == 0)
            {
                leaseGeneration = 1;
            }

            ResetOverlayWeightsImmediate();
        }

        private void Awake()
        {
            ResetImmediate();
        }

        private void OnEnable()
        {
            ResetImmediate();
        }

        private void Update()
        {
            RemoveDestroyedOwners();
            ApplyOverlayWeights(Time.unscaledDeltaTime);
        }

        private void OnDisable()
        {
            ResetImmediate();
        }

        private void OnDestroy()
        {
            ResetImmediate();
        }

        private void RemoveDestroyedOwners()
        {
            bool removed = false;
            for (int index = activeLeases.Count - 1; index >= 0; index--)
            {
                if (activeLeases[index].Owner == null)
                {
                    activeLeases.RemoveAt(index);
                    removed = true;
                }
            }

            if (removed)
            {
                RecalculateWinner();
            }
        }

        private void RecalculateWinner()
        {
            GameplayLookState winner = GameplayLookState.GameplayBase;
            for (int index = 0; index < activeLeases.Count; index++)
            {
                GameplayLookState candidate = activeLeases[index].State;
                if ((int)candidate > (int)winner)
                {
                    winner = candidate;
                }
            }

            currentState = winner;
        }

        private void ApplyOverlayWeights(float unscaledDeltaTime)
        {
            if (overlayBindings == null)
            {
                return;
            }

            for (int index = 0; index < overlayBindings.Length; index++)
            {
                OverlayBinding binding = overlayBindings[index];
                Volume volume = binding.Volume;
                if (volume == null)
                {
                    continue;
                }

                float target = binding.State == currentState ? 1f : 0f;
                float duration = target > volume.weight
                    ? binding.BlendInSeconds
                    : binding.BlendOutSeconds;
                if (duration <= 0.0001f)
                {
                    volume.weight = target;
                    continue;
                }

                float step = Mathf.Max(0f, unscaledDeltaTime) / duration;
                volume.weight = Mathf.MoveTowards(volume.weight, target, step);
            }
        }

        private void ResetOverlayWeightsImmediate()
        {
            if (overlayBindings == null)
            {
                return;
            }

            for (int index = 0; index < overlayBindings.Length; index++)
            {
                Volume volume = overlayBindings[index].Volume;
                if (volume != null)
                {
                    volume.weight = 0f;
                }
            }
        }

        private bool TryGetBinding(
            GameplayLookState state,
            out OverlayBinding result)
        {
            result = default;
            if (state == GameplayLookState.GameplayBase || overlayBindings == null)
            {
                return false;
            }

            bool found = false;
            for (int index = 0; index < overlayBindings.Length; index++)
            {
                OverlayBinding candidate = overlayBindings[index];
                if (candidate.State != state
                    || candidate.Volume == null
                    || candidate.Volume.sharedProfile == null)
                {
                    continue;
                }

                if (found)
                {
                    return false;
                }

                result = candidate;
                found = true;
            }

            return found;
        }

        private bool IsConfigurationValid()
        {
            if (!IsValidVolume(gameplayBaseVolume)
                || overlayBindings == null)
            {
                return false;
            }

            for (int index = 0; index < overlayBindings.Length; index++)
            {
                OverlayBinding candidate = overlayBindings[index];
                Volume candidateVolume = candidate.Volume;
                if (candidate.State == GameplayLookState.GameplayBase
                    || !IsValidVolume(candidateVolume)
                    || candidateVolume == gameplayBaseVolume
                    || candidateVolume.priority <= gameplayBaseVolume.priority)
                {
                    return false;
                }

                for (int previousIndex = 0; previousIndex < index; previousIndex++)
                {
                    OverlayBinding previous = overlayBindings[previousIndex];
                    if (previous.State == candidate.State
                        || previous.Volume == candidateVolume)
                    {
                        return false;
                    }

                    int stateComparison = ((int)candidate.State).CompareTo((int)previous.State);
                    int priorityComparison = candidateVolume.priority.CompareTo(previous.Volume.priority);
                    if (stateComparison == 0
                        || priorityComparison == 0
                        || Math.Sign(stateComparison) != Math.Sign(priorityComparison))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsValidVolume(Volume volume)
        {
            return volume != null
                && volume.isGlobal
                && volume.sharedProfile != null
                && !volume.HasInstantiatedProfile();
        }

        private bool IsLeaseActive(int leaseId, uint generation)
        {
            if (generation != leaseGeneration)
            {
                return false;
            }

            for (int index = 0; index < activeLeases.Count; index++)
            {
                if (activeLeases[index].LeaseId == leaseId)
                {
                    return true;
                }
            }

            return false;
        }

        private void ReleaseLease(int leaseId, uint generation)
        {
            if (generation != leaseGeneration)
            {
                return;
            }

            for (int index = activeLeases.Count - 1; index >= 0; index--)
            {
                if (activeLeases[index].LeaseId != leaseId)
                {
                    continue;
                }

                activeLeases.RemoveAt(index);
                RecalculateWinner();
                ApplyOverlayWeights(0f);
                return;
            }
        }

        private readonly struct LeaseRecord
        {
            public LeaseRecord(
                int leaseId,
                GameplayLookState state,
                UnityEngine.Object owner)
            {
                LeaseId = leaseId;
                State = state;
                Owner = owner;
            }

            public int LeaseId { get; }
            public GameplayLookState State { get; }
            public UnityEngine.Object Owner { get; }
        }
    }
}
