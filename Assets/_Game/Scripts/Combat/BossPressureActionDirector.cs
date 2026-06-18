using System;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum BossPressureActionKind
    {
        SkillPattern,
        SummonPressure,
        PunishOverextend
    }

    [DisallowMultipleComponent]
    public sealed class BossPressureActionDirector : MonoBehaviour
    {
        [Serializable]
        public struct BossPressureActionSlot
        {
            public BossBarragePatternProfile Pattern;
            public BossPressureActionKind ActionKind;
            [Range(1, 3)] public int MinimumTier;
            [Min(1)] public int QueuedWaves;
            [Min(0f)] public float MinimumIntervalSeconds;

            public BossPressureActionSlot(
                BossBarragePatternProfile pattern,
                BossPressureActionKind actionKind,
                int minimumTier,
                int queuedWaves,
                float minimumIntervalSeconds)
            {
                Pattern = pattern;
                ActionKind = actionKind;
                MinimumTier = Mathf.Clamp(minimumTier, 1, 3);
                QueuedWaves = Mathf.Max(1, queuedWaves);
                MinimumIntervalSeconds = Mathf.Max(0f, minimumIntervalSeconds);
            }
        }

        [Header("References")]
        [SerializeField] private BossPressureCostLadder costLadder;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossSummonPressureAction summonPressureAction;

        [Header("Action Selection")]
        [SerializeField] private BossPressureActionSlot[] actionSlots = Array.Empty<BossPressureActionSlot>();
        [SerializeField, Min(0f)] private float globalRecoverySeconds = 0.35f;
        [SerializeField] private bool actionsEnabled = true;

        private float globalRecoveryTimer;
        private float[] perSlotTimers = Array.Empty<float>();
        private int selectionCursor;
        private int totalActionCount;
        private int lastSpentTier;
        private BossPressureActionKind lastActionKind;
        private BossBarragePatternProfile lastQueuedPattern;

        public event Action<BossPressureActionDirector, BossPressureActionKind, BossBarragePatternProfile, int> ActionQueued;

        public bool ActionsEnabled => actionsEnabled;
        public int TotalActionCount => totalActionCount;
        public int LastSpentTier => lastSpentTier;
        public BossPressureActionKind LastActionKind => lastActionKind;
        public BossBarragePatternProfile LastQueuedPattern => lastQueuedPattern;
        public float GlobalRecoveryRemainingSeconds => globalRecoveryTimer;
        public int ActionSlotCount => actionSlots != null ? actionSlots.Length : 0;

        private void OnValidate()
        {
            EnsurePerSlotTimers();
            if (actionSlots == null)
            {
                return;
            }

            for (int i = 0; i < actionSlots.Length; i++)
            {
                BossPressureActionSlot slot = actionSlots[i];
                slot.MinimumTier = Mathf.Clamp(slot.MinimumTier, 1, 3);
                slot.QueuedWaves = Mathf.Max(1, slot.QueuedWaves);
                slot.MinimumIntervalSeconds = Mathf.Max(0f, slot.MinimumIntervalSeconds);
                actionSlots[i] = slot;
            }
        }

        public void ConfigureReferences(
            BossPressureCostLadder newCostLadder,
            BossBarrageEmitter newBossBarrageEmitter,
            BossSummonPressureAction newSummonPressureAction = null)
        {
            costLadder = newCostLadder;
            bossBarrageEmitter = newBossBarrageEmitter;
            summonPressureAction = newSummonPressureAction;
        }

        public void ConfigureActionSlots(BossPressureActionSlot[] newActionSlots)
        {
            actionSlots = newActionSlots != null
                ? (BossPressureActionSlot[])newActionSlots.Clone()
                : Array.Empty<BossPressureActionSlot>();
            EnsurePerSlotTimers(reset: true);
        }

        public bool TryGetActionSlot(int index, out BossPressureActionSlot slot)
        {
            if (actionSlots == null || index < 0 || index >= actionSlots.Length)
            {
                slot = default;
                return false;
            }

            slot = actionSlots[index];
            return true;
        }

        public void SetActionsEnabled(bool enabled)
        {
            actionsEnabled = enabled;
        }

        public void Tick(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            TickTimers(safeDeltaTime);
            if (!actionsEnabled || safeDeltaTime <= 0f)
            {
                return;
            }

            TryQueueBestAvailableAction();
        }

        public bool TryQueueBestAvailableAction()
        {
            if (!CanAttemptAction())
            {
                return false;
            }

            int slotIndex = ResolveBestSlotIndex(costLadder.AvailableTier);
            if (slotIndex < 0)
            {
                return false;
            }

            BossPressureActionSlot slot = actionSlots[slotIndex];
            if (!bossBarrageEmitter.CanQueuePriorityPattern(slot.Pattern))
            {
                return false;
            }

            if (!bossBarrageEmitter.QueuePriorityPattern(slot.Pattern, slot.QueuedWaves))
            {
                return false;
            }

            if (!costLadder.TrySpend(out int spentTier))
            {
                bossBarrageEmitter.CancelQueuedPriorityPattern(slot.Pattern);
                return false;
            }

            if (slot.ActionKind == BossPressureActionKind.SummonPressure)
            {
                summonPressureAction?.TryReleasePressureSummon(spentTier);
            }

            totalActionCount++;
            lastSpentTier = Mathf.Clamp(spentTier, 1, 3);
            lastActionKind = slot.ActionKind;
            lastQueuedPattern = slot.Pattern;
            selectionCursor = (slotIndex + 1) % Mathf.Max(1, actionSlots.Length);
            globalRecoveryTimer = Mathf.Max(globalRecoveryTimer, globalRecoverySeconds);
            EnsurePerSlotTimers();
            perSlotTimers[slotIndex] = Mathf.Max(perSlotTimers[slotIndex], slot.MinimumIntervalSeconds);
            ActionQueued?.Invoke(this, lastActionKind, lastQueuedPattern, lastSpentTier);
            return true;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private bool CanAttemptAction()
        {
            return actionsEnabled
                && costLadder != null
                && costLadder.CanSpend
                && bossBarrageEmitter != null
                && bossBarrageEmitter.IsFiringEnabled
                && !bossBarrageEmitter.IsWindupActive
                && !bossBarrageEmitter.HasQueuedPriorityPattern
                && globalRecoveryTimer <= 0f
                && actionSlots != null
                && actionSlots.Length > 0;
        }

        private int ResolveBestSlotIndex(int availableTier)
        {
            int bestIndex = -1;
            int bestTier = 0;
            int slotCount = actionSlots != null ? actionSlots.Length : 0;
            if (slotCount <= 0)
            {
                return -1;
            }

            EnsurePerSlotTimers();
            int startIndex = Mathf.Clamp(selectionCursor, 0, slotCount - 1);
            for (int step = 0; step < slotCount; step++)
            {
                int index = (startIndex + step) % slotCount;
                BossPressureActionSlot slot = actionSlots[index];
                if (slot.Pattern == null
                    || slot.MinimumTier > availableTier
                    || perSlotTimers[index] > 0f
                    || !CanRunActionKind(slot.ActionKind)
                    || slot.MinimumTier < bestTier)
                {
                    continue;
                }

                bestIndex = index;
                bestTier = slot.MinimumTier;
            }

            return bestIndex;
        }

        private bool CanRunActionKind(BossPressureActionKind actionKind)
        {
            return actionKind != BossPressureActionKind.SummonPressure
                || summonPressureAction == null
                || summonPressureAction.CanRelease;
        }

        private void TickTimers(float deltaTime)
        {
            if (globalRecoveryTimer > 0f)
            {
                globalRecoveryTimer = Mathf.Max(0f, globalRecoveryTimer - deltaTime);
            }

            EnsurePerSlotTimers();
            for (int i = 0; i < perSlotTimers.Length; i++)
            {
                if (perSlotTimers[i] > 0f)
                {
                    perSlotTimers[i] = Mathf.Max(0f, perSlotTimers[i] - deltaTime);
                }
            }
        }

        private void EnsurePerSlotTimers(bool reset = false)
        {
            int slotCount = actionSlots != null ? actionSlots.Length : 0;
            if (perSlotTimers == null || perSlotTimers.Length != slotCount || reset)
            {
                perSlotTimers = new float[slotCount];
            }

            if (selectionCursor >= slotCount)
            {
                selectionCursor = 0;
            }
        }
    }
}
