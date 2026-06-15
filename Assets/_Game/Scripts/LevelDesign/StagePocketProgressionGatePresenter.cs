using System;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    public sealed class StagePocketProgressionGatePresenter : MonoBehaviour
    {
        [SerializeField] private StageEncounterReviewOwner owner;
        [SerializeField] private StagePocketProgressionGateBinding[] gates = Array.Empty<StagePocketProgressionGateBinding>();

        private bool[] lockedStates = Array.Empty<bool>();

        public StageEncounterReviewOwner Owner => owner;
        public int GateCount => gates != null ? gates.Length : 0;

        public void Configure(
            StageEncounterReviewOwner newOwner,
            StagePocketProgressionGateBinding[] newGates)
        {
            owner = newOwner;
            gates = newGates ?? Array.Empty<StagePocketProgressionGateBinding>();
            EnsureStateBuffer();
            RefreshNow();
        }

        public bool IsGateLocked(int index)
        {
            if (index < 0 || index >= GateCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            EnsureStateBuffer();
            return lockedStates[index];
        }

        public int GetGateColliderCount(int index)
        {
            if (index < 0 || index >= GateCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return gates[index].ColliderCount;
        }

        public void RefreshNow()
        {
            EnsureStateBuffer();
            for (int i = 0; i < GateCount; i++)
            {
                bool locked = owner == null || !owner.IsPocketCompleted(gates[i].UnlockAfterPocketIndex);
                lockedStates[i] = locked;
                gates[i].ApplyLockedState(locked);
            }
        }

        private void OnEnable()
        {
            RefreshNow();
        }

        private void Update()
        {
            RefreshNow();
        }

        private void EnsureStateBuffer()
        {
            int gateCount = GateCount;
            if (lockedStates == null || lockedStates.Length != gateCount)
            {
                lockedStates = new bool[gateCount];
            }
        }
    }

    [Serializable]
    public struct StagePocketProgressionGateBinding
    {
        [SerializeField] private string label;
        [SerializeField, Min(0)] private int unlockAfterPocketIndex;
        [SerializeField] private GameObject gateRoot;
        [SerializeField] private Collider[] blockers;
        [SerializeField] private Renderer[] visuals;

        public StagePocketProgressionGateBinding(
            string label,
            int unlockAfterPocketIndex,
            GameObject gateRoot,
            Collider[] blockers,
            Renderer[] visuals)
        {
            this.label = label;
            this.unlockAfterPocketIndex = unlockAfterPocketIndex;
            this.gateRoot = gateRoot;
            this.blockers = blockers ?? Array.Empty<Collider>();
            this.visuals = visuals ?? Array.Empty<Renderer>();
        }

        public string Label => label;
        public int UnlockAfterPocketIndex => unlockAfterPocketIndex;
        public int ColliderCount => blockers != null ? blockers.Length : 0;

        public void ApplyLockedState(bool locked)
        {
            if (gateRoot != null && !gateRoot.activeSelf)
            {
                gateRoot.SetActive(true);
            }

            if (blockers != null)
            {
                for (int i = 0; i < blockers.Length; i++)
                {
                    if (blockers[i] != null)
                    {
                        blockers[i].enabled = locked;
                    }
                }
            }

            if (visuals != null)
            {
                for (int i = 0; i < visuals.Length; i++)
                {
                    if (visuals[i] != null)
                    {
                        visuals[i].enabled = locked;
                    }
                }
            }
        }
    }
}
