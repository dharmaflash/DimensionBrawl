using System;
using DimensionBrawl.LevelDesign;
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
            public string ResponseId;
            [TextArea] public string StageLoopRole;
            [TextArea] public string PlayerAnswer;
            [TextArea] public string SummonAnswer;
            public bool UsePlayerForwardRiskGate;
            [Range(0f, 1f)] public float MinimumPlayerForwardRisk01;
            [Range(0f, 1f)] public float MaximumPlayerForwardRisk01;

            public bool HasResponsePlan =>
                !string.IsNullOrWhiteSpace(ResponseId)
                && !string.IsNullOrWhiteSpace(StageLoopRole)
                && (!string.IsNullOrWhiteSpace(PlayerAnswer) || !string.IsNullOrWhiteSpace(SummonAnswer));

            public BossPressureActionSlot(
                BossBarragePatternProfile pattern,
                BossPressureActionKind actionKind,
                int minimumTier,
                int queuedWaves,
                float minimumIntervalSeconds,
                bool usePlayerForwardRiskGate = false,
                float minimumPlayerForwardRisk01 = 0f,
                float maximumPlayerForwardRisk01 = 1f,
                string responseId = "",
                string stageLoopRole = "",
                string playerAnswer = "",
                string summonAnswer = "")
            {
                Pattern = pattern;
                ActionKind = actionKind;
                MinimumTier = Mathf.Clamp(minimumTier, 1, 3);
                QueuedWaves = Mathf.Max(1, queuedWaves);
                MinimumIntervalSeconds = Mathf.Max(0f, minimumIntervalSeconds);
                ResponseId = responseId ?? string.Empty;
                StageLoopRole = stageLoopRole ?? string.Empty;
                PlayerAnswer = playerAnswer ?? string.Empty;
                SummonAnswer = summonAnswer ?? string.Empty;
                UsePlayerForwardRiskGate = usePlayerForwardRiskGate;
                MinimumPlayerForwardRisk01 = Mathf.Clamp01(minimumPlayerForwardRisk01);
                MaximumPlayerForwardRisk01 = Mathf.Clamp01(maximumPlayerForwardRisk01);
                if (MaximumPlayerForwardRisk01 < MinimumPlayerForwardRisk01)
                {
                    MaximumPlayerForwardRisk01 = MinimumPlayerForwardRisk01;
                }
            }
        }

        [Header("References")]
        [SerializeField] private BossPressureCostLadder costLadder;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossSummonPressureAction summonPressureAction;
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedPlayer;

        [Header("Action Selection")]
        [SerializeField] private BossPressureActionDeckProfile actionDeckProfile;
        [SerializeField] private BossPressureActionSlot[] actionSlots = Array.Empty<BossPressureActionSlot>();
        [SerializeField, Min(0f)] private float globalRecoverySeconds = 0.35f;
        [SerializeField] private bool holdForNextTierActionWhenGateAllows;
        [SerializeField] private bool actionsEnabled = true;

        [Header("Player Summon Response")]
        [SerializeField, Min(0f)] private float playerSummonResponseWindowSeconds = 4f;

        private float globalRecoveryTimer;
        private float playerSummonResponseTimer;
        private float[] perSlotTimers = Array.Empty<float>();
        private int selectionCursor;
        private int totalActionCount;
        private int totalPlayerSummonResponseCount;
        private int lastSpentTier;
        private int lastObservedPlayerSummonTier;
        private int lastPlayerSummonResponseTier;
        private BossPressureActionKind lastActionKind;
        private BossPressureActionKind lastPlayerSummonResponseKind;
        private BossBarragePatternProfile lastQueuedPattern;
        private BossPressureActionSlot lastQueuedActionSlot;
        private bool lastActionRespondedToPlayerSummon;

        public event Action<BossPressureActionDirector, BossPressureActionKind, BossBarragePatternProfile, int> ActionQueued;

        public bool ActionsEnabled => actionsEnabled;
        public int TotalActionCount => totalActionCount;
        public int LastSpentTier => lastSpentTier;
        public BossPressureActionKind LastActionKind => lastActionKind;
        public BossBarragePatternProfile LastQueuedPattern => lastQueuedPattern;
        public BossPressureActionSlot LastQueuedActionSlot => lastQueuedActionSlot;
        public bool HasLastQueuedActionSlot => totalActionCount > 0 && lastQueuedActionSlot.Pattern != null;
        public BossSummonPressureAction SummonPressureAction => summonPressureAction;
        public BossPressureActionDeckProfile ActionDeckProfile => actionDeckProfile;
        public bool HasActionDeckProfile => actionDeckProfile != null;
        public bool HoldForNextTierActionWhenGateAllows => holdForNextTierActionWhenGateAllows;
        public float GlobalRecoveryRemainingSeconds => globalRecoveryTimer;
        public int ActionSlotCount => actionSlots != null ? actionSlots.Length : 0;
        public float CurrentPlayerForwardRisk01 => ResolvePlayerForwardRisk01();
        public bool IsPlayerSummonResponseWindowActive => playerSummonResponseTimer > 0f;
        public float PlayerSummonResponseRemainingSeconds => playerSummonResponseTimer;
        public int LastObservedPlayerSummonTier => lastObservedPlayerSummonTier;
        public bool LastActionRespondedToPlayerSummon => lastActionRespondedToPlayerSummon;
        public int TotalPlayerSummonResponseCount => totalPlayerSummonResponseCount;
        public BossPressureActionKind LastPlayerSummonResponseKind => lastPlayerSummonResponseKind;
        public int LastPlayerSummonResponseTier => lastPlayerSummonResponseTier;

        private void Awake()
        {
            ApplyActionDeckProfile();
        }

        private void OnValidate()
        {
            ApplyActionDeckProfile();
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
                slot.MinimumPlayerForwardRisk01 = Mathf.Clamp01(slot.MinimumPlayerForwardRisk01);
                slot.MaximumPlayerForwardRisk01 = Mathf.Clamp01(slot.MaximumPlayerForwardRisk01);
                if (slot.MaximumPlayerForwardRisk01 < slot.MinimumPlayerForwardRisk01)
                {
                    slot.MaximumPlayerForwardRisk01 = slot.MinimumPlayerForwardRisk01;
                }

                actionSlots[i] = slot;
            }

            playerSummonResponseWindowSeconds = Mathf.Max(0f, playerSummonResponseWindowSeconds);
        }

        public void ConfigureReferences(
            BossPressureCostLadder newCostLadder,
            BossBarrageEmitter newBossBarrageEmitter,
            BossSummonPressureAction newSummonPressureAction = null,
            SummonLaneSpace newLaneSpace = null,
            Transform newTrackedPlayer = null)
        {
            costLadder = newCostLadder;
            bossBarrageEmitter = newBossBarrageEmitter;
            summonPressureAction = newSummonPressureAction;
            laneSpace = newLaneSpace;
            trackedPlayer = newTrackedPlayer;
            ApplyActionDeckProfile();
        }

        public void ConfigureActionSlots(BossPressureActionSlot[] newActionSlots)
        {
            actionDeckProfile = null;
            actionSlots = newActionSlots != null
                ? (BossPressureActionSlot[])newActionSlots.Clone()
                : Array.Empty<BossPressureActionSlot>();
            EnsurePerSlotTimers(reset: true);
        }

        public void ConfigureActionDeck(BossPressureActionDeckProfile newActionDeckProfile)
        {
            actionDeckProfile = newActionDeckProfile;
            ApplyActionDeckProfile();
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

        public void SetHoldForNextTierActionWhenGateAllows(bool enabled)
        {
            holdForNextTierActionWhenGateAllows = enabled;
        }

        public void NotifyPlayerSummonFrontlineCreated(int summonTier)
        {
            if (playerSummonResponseWindowSeconds <= 0f)
            {
                return;
            }

            lastObservedPlayerSummonTier = Mathf.Clamp(summonTier, 0, 3);
            playerSummonResponseTimer = Mathf.Max(
                playerSummonResponseTimer,
                playerSummonResponseWindowSeconds);
        }

        public bool TryGetHeldNextTierAction(out BossPressureActionSlot slot, out int nextTier)
        {
            slot = default;
            nextTier = 0;
            if (!CanAttemptAction())
            {
                return false;
            }

            return TryGetNextTierHoldCandidate(costLadder.AvailableTier, out slot, out nextTier);
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
            lastActionRespondedToPlayerSummon = false;
            if (!CanAttemptAction())
            {
                return false;
            }

            int availableTier = costLadder.AvailableTier;
            if (ShouldHoldForNextTierAction(availableTier))
            {
                return false;
            }

            int slotIndex = ResolveBestSlotIndex(availableTier);
            if (slotIndex < 0)
            {
                return false;
            }

            BossPressureActionSlot slot = actionSlots[slotIndex];
            bool respondsToPlayerSummon = IsPlayerSummonResponseWindowActive;
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
            lastQueuedActionSlot = slot;
            lastActionRespondedToPlayerSummon = respondsToPlayerSummon;
            if (respondsToPlayerSummon)
            {
                totalPlayerSummonResponseCount++;
                lastPlayerSummonResponseKind = slot.ActionKind;
                lastPlayerSummonResponseTier = lastSpentTier;
                playerSummonResponseTimer = 0f;
            }

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

        private void ApplyActionDeckProfile()
        {
            if (actionDeckProfile == null)
            {
                return;
            }

            actionSlots = actionDeckProfile.CopyActionSlots();
            globalRecoverySeconds = actionDeckProfile.GlobalRecoverySeconds;
            EnsurePerSlotTimers(reset: true);
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
            int bestScore = int.MinValue;
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
                    || !IsPlayerRiskAllowed(slot))
                {
                    continue;
                }

                int score = ResolveSlotSelectionScore(slot);
                if (score < bestScore)
                {
                    continue;
                }

                bestIndex = index;
                bestScore = score;
            }

            return bestIndex;
        }

        private int ResolveSlotSelectionScore(BossPressureActionSlot slot)
        {
            int score = slot.MinimumTier * 100;
            if (!IsPlayerSummonResponseWindowActive)
            {
                return score;
            }

            switch (slot.ActionKind)
            {
                case BossPressureActionKind.SummonPressure:
                    return score + 160;
                case BossPressureActionKind.SkillPattern:
                    return score + 40;
                case BossPressureActionKind.PunishOverextend:
                    return score + 20;
                default:
                    return score;
            }
        }

        private bool CanRunActionKind(BossPressureActionKind actionKind)
        {
            return actionKind != BossPressureActionKind.SummonPressure
                || (summonPressureAction != null && summonPressureAction.CanRelease);
        }

        private bool ShouldHoldForNextTierAction(int availableTier)
        {
            return TryGetNextTierHoldCandidate(availableTier, out _, out _);
        }

        private bool TryGetNextTierHoldCandidate(
            int availableTier,
            out BossPressureActionSlot holdSlot,
            out int nextTier)
        {
            holdSlot = default;
            nextTier = 0;
            if (!holdForNextTierActionWhenGateAllows || availableTier <= 0 || availableTier >= 3)
            {
                return false;
            }

            nextTier = availableTier + 1;
            int slotCount = actionSlots != null ? actionSlots.Length : 0;
            if (slotCount <= 0)
            {
                nextTier = 0;
                return false;
            }

            EnsurePerSlotTimers();
            for (int i = 0; i < slotCount; i++)
            {
                BossPressureActionSlot slot = actionSlots[i];
                if (slot.Pattern == null
                    || slot.MinimumTier != nextTier
                    || perSlotTimers[i] > 0f
                    || !CanRunActionKind(slot.ActionKind)
                    || !IsPlayerRiskAllowed(slot)
                    || bossBarrageEmitter == null
                    || !bossBarrageEmitter.CanQueuePriorityPattern(slot.Pattern))
                {
                    continue;
                }

                holdSlot = slot;
                return true;
            }

            nextTier = 0;
            return false;
        }

        private bool IsPlayerRiskAllowed(BossPressureActionSlot slot)
        {
            if (!slot.UsePlayerForwardRiskGate)
            {
                return true;
            }

            float risk01 = ResolvePlayerForwardRisk01();
            return risk01 >= slot.MinimumPlayerForwardRisk01
                && risk01 <= slot.MaximumPlayerForwardRisk01;
        }

        private float ResolvePlayerForwardRisk01()
        {
            return laneSpace != null && trackedPlayer != null
                ? laneSpace.EvaluateForwardRisk01(trackedPlayer.position)
                : 0f;
        }

        private void TickTimers(float deltaTime)
        {
            if (globalRecoveryTimer > 0f)
            {
                globalRecoveryTimer = Mathf.Max(0f, globalRecoveryTimer - deltaTime);
            }

            if (playerSummonResponseTimer > 0f)
            {
                playerSummonResponseTimer = Mathf.Max(0f, playerSummonResponseTimer - deltaTime);
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
