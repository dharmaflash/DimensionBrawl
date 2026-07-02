using System;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum BossPressureActionKind
    {
        SkillPattern,
        SummonPressure,
        PunishOverextend,
        BasicShot,
        SpecialSkill
    }

    public enum BossPressureMovementIntent
    {
        CostPressure,
        HoldBacklineFire,
        StrafeFire,
        CommitForward,
        RetreatAndSummon
    }

    [DisallowMultipleComponent]
    public sealed class BossPressureActionDirector : MonoBehaviour
    {
        public struct BossPressureDecisionContext
        {
            public int AvailableTier;
            public float PlayerForwardRisk01;
            public bool PlayerSummonResponseWindowActive;
            public int LastObservedPlayerSummonTier;
            public float BossForwardRisk01;
            public bool CanReleaseSummonPressure;
            public int ActiveBossPressureSummonCount;
            public bool HasActiveBossPressureSummon;

            public BossPressureDecisionContext(
                int availableTier,
                float playerForwardRisk01,
                bool playerSummonResponseWindowActive,
                int lastObservedPlayerSummonTier,
                float bossForwardRisk01,
                bool canReleaseSummonPressure,
                int activeBossPressureSummonCount)
            {
                AvailableTier = Mathf.Clamp(availableTier, 0, 3);
                PlayerForwardRisk01 = Mathf.Clamp01(playerForwardRisk01);
                PlayerSummonResponseWindowActive = playerSummonResponseWindowActive;
                LastObservedPlayerSummonTier = Mathf.Clamp(lastObservedPlayerSummonTier, 0, 3);
                BossForwardRisk01 = Mathf.Clamp01(bossForwardRisk01);
                CanReleaseSummonPressure = canReleaseSummonPressure;
                ActiveBossPressureSummonCount = Mathf.Max(0, activeBossPressureSummonCount);
                HasActiveBossPressureSummon = ActiveBossPressureSummonCount > 0;
            }
        }

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
            public bool UsePlayerSummonResponseGate;
            [Range(1, 3)] public int MinimumPlayerSummonTier;
            [Min(0)] public int SelectionPriority;
            [Min(0)] public int ForwardRiskPriorityBonus;
            [Min(0)] public int SummonResponsePriorityBonus;
            public BossPressureMovementIntent MovementIntent;

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
                bool usePlayerSummonResponseGate = false,
                int minimumPlayerSummonTier = 1,
                string responseId = "",
                string stageLoopRole = "",
                string playerAnswer = "",
                string summonAnswer = "",
                int selectionPriority = 0,
                int forwardRiskPriorityBonus = 0,
                int summonResponsePriorityBonus = 0,
                BossPressureMovementIntent movementIntent = BossPressureMovementIntent.CostPressure)
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

                UsePlayerSummonResponseGate = usePlayerSummonResponseGate;
                MinimumPlayerSummonTier = Mathf.Clamp(minimumPlayerSummonTier, 1, 3);
                SelectionPriority = Mathf.Max(0, selectionPriority);
                ForwardRiskPriorityBonus = Mathf.Max(0, forwardRiskPriorityBonus);
                SummonResponsePriorityBonus = Mathf.Max(0, summonResponsePriorityBonus);
                MovementIntent = movementIntent;
            }
        }

        [Header("References")]
        [SerializeField] private BossPressureCostLadder costLadder;
        [SerializeField] private BossBarrageEmitter bossBarrageEmitter;
        [SerializeField] private BossBasicFireEmitter basicFireEmitter;
        [SerializeField] private BossSummonPressureAction summonPressureAction;
        [SerializeField] private SummonLaneSpace laneSpace;
        [SerializeField] private Transform trackedPlayer;

        [Header("Action Selection")]
        [SerializeField] private BossPressureActionDeckProfile actionDeckProfile;
        [SerializeField] private BossPressureActionSlot[] actionSlots = Array.Empty<BossPressureActionSlot>();
        [SerializeField, Min(0f)] private float globalRecoverySeconds = 0.35f;
        [SerializeField, Min(0f)] private float decisionThinkIntervalSeconds = 0.25f;
        [SerializeField, Min(0f)] private float basicFireSuppressionSecondsAfterPressureAction = 0.65f;
        [SerializeField] private bool holdForNextTierActionWhenGateAllows;
        [SerializeField] private bool actionsEnabled = true;

        [Header("Player Summon Response")]
        [SerializeField, Min(0f)] private float playerSummonResponseWindowSeconds = 4f;
        [SerializeField, Min(0f)] private float heldResponseWindowFloorSeconds = 0.6f;
        [SerializeField, Min(0f)] private float maxHeldResponseWindowExtensionSeconds = 1.5f;

        private float globalRecoveryTimer;
        private float decisionThinkTimer;
        private float playerSummonResponseTimer;
        private float heldResponseWindowExtensionRemainingSeconds;
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
        private BossPressureDecisionContext lastDecisionContext;
        private int lastQueuedActionSlotIndex = -1;
        private int lastSelectionScore;
        private float lastActionAgeSeconds = float.PositiveInfinity;
        private int totalBasicShotVolleys;
        private int lastBasicShotProjectileCount;
        private float lastBasicShotAgeSeconds = float.PositiveInfinity;
        private bool basicFireSubscribed;
        private bool lastActionRespondedToPlayerSummon;

        public event Action<BossPressureActionDirector, BossPressureActionKind, BossBarragePatternProfile, int> ActionQueued;

        public bool ActionsEnabled => actionsEnabled;
        public int TotalActionCount => totalActionCount;
        public int LastSpentTier => lastSpentTier;
        public BossPressureActionKind LastActionKind => lastActionKind;
        public BossBarragePatternProfile LastQueuedPattern => lastQueuedPattern;
        public BossPressureActionSlot LastQueuedActionSlot => lastQueuedActionSlot;
        public bool HasLastQueuedActionSlot => totalActionCount > 0 && lastQueuedActionSlot.Pattern != null;
        public BossBasicFireEmitter BasicFireEmitter => basicFireEmitter;
        public bool HasBasicFireEmitter => basicFireEmitter != null;
        public BossSummonPressureAction SummonPressureAction => summonPressureAction;
        public BossPressureActionDeckProfile ActionDeckProfile => actionDeckProfile;
        public bool HasActionDeckProfile => actionDeckProfile != null;
        public bool HoldForNextTierActionWhenGateAllows => holdForNextTierActionWhenGateAllows;
        public float GlobalRecoveryRemainingSeconds => globalRecoveryTimer;
        public float DecisionThinkIntervalSeconds => decisionThinkIntervalSeconds;
        public float DecisionThinkRemainingSeconds => decisionThinkTimer;
        public float BasicFireSuppressionSecondsAfterPressureAction => basicFireSuppressionSecondsAfterPressureAction;
        public int ActionSlotCount => actionSlots != null ? actionSlots.Length : 0;
        public float CurrentPlayerForwardRisk01 => ResolvePlayerForwardRisk01();
        public bool IsPlayerSummonResponseWindowActive => playerSummonResponseTimer > 0f;
        public float PlayerSummonResponseRemainingSeconds => playerSummonResponseTimer;
        public float HeldResponseWindowFloorSeconds => heldResponseWindowFloorSeconds;
        public float HeldResponseWindowExtensionRemainingSeconds => heldResponseWindowExtensionRemainingSeconds;
        public int LastObservedPlayerSummonTier => lastObservedPlayerSummonTier;
        public bool LastActionRespondedToPlayerSummon => lastActionRespondedToPlayerSummon;
        public int TotalPlayerSummonResponseCount => totalPlayerSummonResponseCount;
        public BossPressureActionKind LastPlayerSummonResponseKind => lastPlayerSummonResponseKind;
        public int LastPlayerSummonResponseTier => lastPlayerSummonResponseTier;
        public BossPressureDecisionContext LastDecisionContext => lastDecisionContext;
        public int LastQueuedActionSlotIndex => lastQueuedActionSlotIndex;
        public int LastSelectionScore => lastSelectionScore;
        public float LastActionAgeSeconds => lastActionAgeSeconds;
        public int TotalBasicShotVolleys => totalBasicShotVolleys;
        public int LastBasicShotProjectileCount => lastBasicShotProjectileCount;
        public float LastBasicShotAgeSeconds => lastBasicShotAgeSeconds;
        public BossPressureMovementIntent LastMovementIntent =>
            HasLastQueuedActionSlot ? lastQueuedActionSlot.MovementIntent : BossPressureMovementIntent.CostPressure;

        private void Awake()
        {
            ApplyActionDeckProfile();
        }

        private void OnEnable()
        {
            SubscribeBasicFireEmitter();
        }

        private void OnDisable()
        {
            UnsubscribeBasicFireEmitter();
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
                slot.MinimumPlayerSummonTier = Mathf.Clamp(slot.MinimumPlayerSummonTier, 1, 3);
                slot.SelectionPriority = Mathf.Max(0, slot.SelectionPriority);
                slot.ForwardRiskPriorityBonus = Mathf.Max(0, slot.ForwardRiskPriorityBonus);
                slot.SummonResponsePriorityBonus = Mathf.Max(0, slot.SummonResponsePriorityBonus);
                if (slot.MaximumPlayerForwardRisk01 < slot.MinimumPlayerForwardRisk01)
                {
                    slot.MaximumPlayerForwardRisk01 = slot.MinimumPlayerForwardRisk01;
                }

                actionSlots[i] = slot;
            }

            playerSummonResponseWindowSeconds = Mathf.Max(0f, playerSummonResponseWindowSeconds);
            heldResponseWindowFloorSeconds = Mathf.Max(0f, heldResponseWindowFloorSeconds);
            maxHeldResponseWindowExtensionSeconds = Mathf.Max(0f, maxHeldResponseWindowExtensionSeconds);
            decisionThinkIntervalSeconds = Mathf.Max(0f, decisionThinkIntervalSeconds);
            basicFireSuppressionSecondsAfterPressureAction = Mathf.Max(0f, basicFireSuppressionSecondsAfterPressureAction);
        }

        public void ConfigureReferences(
            BossPressureCostLadder newCostLadder,
            BossBarrageEmitter newBossBarrageEmitter,
            BossSummonPressureAction newSummonPressureAction = null,
            SummonLaneSpace newLaneSpace = null,
            Transform newTrackedPlayer = null,
            BossBasicFireEmitter newBasicFireEmitter = null)
        {
            UnsubscribeBasicFireEmitter();
            costLadder = newCostLadder;
            bossBarrageEmitter = newBossBarrageEmitter;
            basicFireEmitter = newBasicFireEmitter;
            summonPressureAction = newSummonPressureAction;
            laneSpace = newLaneSpace;
            trackedPlayer = newTrackedPlayer;
            ApplyActionDeckProfile();
            SubscribeBasicFireEmitter();
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
            heldResponseWindowExtensionRemainingSeconds = maxHeldResponseWindowExtensionSeconds;
        }

        public bool TryGetHeldNextTierAction(out BossPressureActionSlot slot, out int nextTier)
        {
            slot = default;
            nextTier = 0;
            if (!CanAttemptAction())
            {
                return false;
            }

            BossPressureDecisionContext context = BuildDecisionContext(costLadder.AvailableTier);
            lastDecisionContext = context;
            return TryGetNextTierHoldCandidate(context, out slot, out nextTier);
        }

        public void Tick(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            TickTimers(safeDeltaTime);
            if (!actionsEnabled || safeDeltaTime <= 0f)
            {
                return;
            }

            if (decisionThinkTimer > 0f)
            {
                return;
            }

            TryQueueBestAvailableAction();
            decisionThinkTimer = Mathf.Max(decisionThinkTimer, decisionThinkIntervalSeconds);
        }

        public bool TryQueueBestAvailableAction()
        {
            lastActionRespondedToPlayerSummon = false;
            if (!CanAttemptAction())
            {
                return false;
            }

            BossPressureDecisionContext context = BuildDecisionContext(costLadder.AvailableTier);
            lastDecisionContext = context;
            if (ShouldHoldForNextTierAction(context))
            {
                return false;
            }

            int slotIndex = ResolveBestSlotIndex(context, out int selectionScore);
            if (slotIndex < 0)
            {
                return false;
            }

            BossPressureActionSlot slot = actionSlots[slotIndex];
            bool respondsToPlayerSummon = slot.UsePlayerSummonResponseGate && IsPlayerSummonResponseWindowActive;
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

            SuppressBasicFireForPressureAction(slot);

            totalActionCount++;
            lastSpentTier = Mathf.Clamp(spentTier, 1, 3);
            lastActionKind = slot.ActionKind;
            lastQueuedPattern = slot.Pattern;
            lastQueuedActionSlot = slot;
            lastQueuedActionSlotIndex = slotIndex;
            lastSelectionScore = selectionScore;
            lastActionAgeSeconds = 0f;
            lastActionRespondedToPlayerSummon = respondsToPlayerSummon;
            if (respondsToPlayerSummon)
            {
                totalPlayerSummonResponseCount++;
                lastPlayerSummonResponseKind = slot.ActionKind;
                lastPlayerSummonResponseTier = lastSpentTier;
                playerSummonResponseTimer = 0f;
                heldResponseWindowExtensionRemainingSeconds = 0f;
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
            decisionThinkIntervalSeconds = actionDeckProfile.DecisionThinkIntervalSeconds;
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

        private BossPressureDecisionContext BuildDecisionContext(int availableTier)
        {
            return new BossPressureDecisionContext(
                availableTier,
                ResolvePlayerForwardRisk01(),
                IsPlayerSummonResponseWindowActive,
                lastObservedPlayerSummonTier,
                costLadder != null ? costLadder.EvaluateBossForwardRisk01(transform.position) : 0f,
                summonPressureAction != null && summonPressureAction.CanRelease,
                summonPressureAction != null ? summonPressureAction.ActiveSummonActorCount : 0);
        }

        private int ResolveBestSlotIndex(BossPressureDecisionContext context, out int bestScore)
        {
            int bestIndex = -1;
            bestScore = int.MinValue;
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
                    || slot.MinimumTier > context.AvailableTier
                    || perSlotTimers[index] > 0f
                    || !CanRunActionKind(slot.ActionKind)
                    || !IsBossSummonPressureAllowed(slot, context)
                    || !IsPlayerRiskAllowed(slot, context)
                    || !IsPlayerSummonResponseAllowed(slot, context))
                {
                    continue;
                }

                int score = ResolveSlotSelectionScore(slot, context, step);
                if (score < bestScore)
                {
                    continue;
                }

                bestIndex = index;
                bestScore = score;
            }

            return bestIndex;
        }

        private int ResolveSlotSelectionScore(
            BossPressureActionSlot slot,
            BossPressureDecisionContext context,
            int selectionStep)
        {
            int score = slot.MinimumTier * 100 + Mathf.Max(0, slot.SelectionPriority);
            if (slot.UsePlayerForwardRiskGate)
            {
                float riskBand01 = Mathf.InverseLerp(
                    slot.MinimumPlayerForwardRisk01,
                    slot.MaximumPlayerForwardRisk01,
                    context.PlayerForwardRisk01);
                score += Mathf.RoundToInt(Mathf.Clamp01(riskBand01) * 40f)
                    + Mathf.Max(0, slot.ForwardRiskPriorityBonus);
            }

            if (!context.PlayerSummonResponseWindowActive || !slot.UsePlayerSummonResponseGate)
            {
                return score - selectionStep;
            }

            score += slot.SummonResponsePriorityBonus > 0
                ? slot.SummonResponsePriorityBonus
                : 80;

            switch (slot.ActionKind)
            {
                case BossPressureActionKind.SummonPressure:
                    return score + 160 - selectionStep;
                case BossPressureActionKind.SpecialSkill:
                    return score + 80 - selectionStep;
                case BossPressureActionKind.SkillPattern:
                    return score + 40 - selectionStep;
                case BossPressureActionKind.PunishOverextend:
                    return score + 20 - selectionStep;
                default:
                    return score - selectionStep;
            }
        }

        private bool CanRunActionKind(BossPressureActionKind actionKind)
        {
            return actionKind != BossPressureActionKind.SummonPressure
                || (summonPressureAction != null && summonPressureAction.CanRelease);
        }

        private static bool IsBossSummonPressureAllowed(
            BossPressureActionSlot slot,
            BossPressureDecisionContext context)
        {
            return slot.ActionKind != BossPressureActionKind.SummonPressure
                || !context.HasActiveBossPressureSummon;
        }

        private bool ShouldHoldForNextTierAction(BossPressureDecisionContext context)
        {
            if (!TryGetNextTierHoldCandidate(context, out BossPressureActionSlot holdSlot, out int nextTier, out int holdScore))
            {
                return false;
            }

            int currentSlotIndex = ResolveBestSlotIndex(context, out int currentScore);
            if (currentSlotIndex < 0)
            {
                return true;
            }

            return holdSlot.Pattern != null
                && nextTier > context.AvailableTier
                && holdScore > currentScore;
        }

        private bool TryGetNextTierHoldCandidate(
            BossPressureDecisionContext context,
            out BossPressureActionSlot holdSlot,
            out int nextTier)
        {
            return TryGetNextTierHoldCandidate(context, out holdSlot, out nextTier, out _);
        }

        private bool TryGetNextTierHoldCandidate(
            BossPressureDecisionContext context,
            out BossPressureActionSlot holdSlot,
            out int nextTier,
            out int holdScore)
        {
            holdSlot = default;
            nextTier = 0;
            holdScore = int.MinValue;
            if (!holdForNextTierActionWhenGateAllows || context.AvailableTier <= 0 || context.AvailableTier >= 3)
            {
                return false;
            }

            int highestHoldTier = totalActionCount > 0
                ? 3
                : context.AvailableTier + 1;
            int slotCount = actionSlots != null ? actionSlots.Length : 0;
            if (slotCount <= 0)
            {
                nextTier = 0;
                holdScore = int.MinValue;
                return false;
            }

            EnsurePerSlotTimers();
            int bestIndex = -1;
            for (int i = 0; i < slotCount; i++)
            {
                BossPressureActionSlot slot = actionSlots[i];
                if (slot.MinimumTier <= context.AvailableTier || slot.MinimumTier > highestHoldTier)
                {
                    continue;
                }

                BossPressureDecisionContext holdContext = new BossPressureDecisionContext(
                    slot.MinimumTier,
                    context.PlayerForwardRisk01,
                    context.PlayerSummonResponseWindowActive,
                    context.LastObservedPlayerSummonTier,
                    context.BossForwardRisk01,
                    context.CanReleaseSummonPressure,
                    context.ActiveBossPressureSummonCount);
                if (slot.Pattern == null
                    || perSlotTimers[i] > 0f
                    || !CanRunActionKind(slot.ActionKind)
                    || !IsBossSummonPressureAllowed(slot, holdContext)
                    || !IsPlayerRiskAllowed(slot, holdContext)
                    || !IsPlayerSummonResponseAllowed(slot, holdContext)
                    || bossBarrageEmitter == null
                    || !bossBarrageEmitter.CanQueuePriorityPattern(slot.Pattern))
                {
                    continue;
                }

                int score = ResolveSlotSelectionScore(slot, holdContext, i);
                if (score < holdScore)
                {
                    continue;
                }

                bestIndex = i;
                nextTier = slot.MinimumTier;
                holdScore = score;
            }

            if (bestIndex >= 0)
            {
                holdSlot = actionSlots[bestIndex];
                return true;
            }

            nextTier = 0;
            holdScore = int.MinValue;
            return false;
        }

        private bool IsPlayerRiskAllowed(BossPressureActionSlot slot, BossPressureDecisionContext context)
        {
            if (!slot.UsePlayerForwardRiskGate)
            {
                return true;
            }

            float risk01 = context.PlayerForwardRisk01;
            return risk01 >= slot.MinimumPlayerForwardRisk01
                && risk01 <= slot.MaximumPlayerForwardRisk01;
        }

        private bool IsPlayerSummonResponseAllowed(BossPressureActionSlot slot, BossPressureDecisionContext context)
        {
            if (!slot.UsePlayerSummonResponseGate)
            {
                return true;
            }

            return context.PlayerSummonResponseWindowActive
                && context.LastObservedPlayerSummonTier >= Mathf.Clamp(slot.MinimumPlayerSummonTier, 1, 3);
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

            if (decisionThinkTimer > 0f)
            {
                decisionThinkTimer = Mathf.Max(0f, decisionThinkTimer - deltaTime);
            }

            if (!float.IsPositiveInfinity(lastActionAgeSeconds))
            {
                lastActionAgeSeconds += deltaTime;
            }

            if (!float.IsPositiveInfinity(lastBasicShotAgeSeconds))
            {
                lastBasicShotAgeSeconds += deltaTime;
            }

            lastDecisionContext = BuildDecisionContext(costLadder != null ? costLadder.AvailableTier : 0);

            bool preserveHeldResponseWindow = ShouldPreserveHeldPlayerSummonResponseWindow();
            if (playerSummonResponseTimer > 0f)
            {
                playerSummonResponseTimer = Mathf.Max(0f, playerSummonResponseTimer - deltaTime);
                if (preserveHeldResponseWindow)
                {
                    PreserveHeldPlayerSummonResponseWindow();
                }

                if (playerSummonResponseTimer <= 0f)
                {
                    heldResponseWindowExtensionRemainingSeconds = 0f;
                }
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

        private bool ShouldPreserveHeldPlayerSummonResponseWindow()
        {
            return heldResponseWindowFloorSeconds > 0f
                && heldResponseWindowExtensionRemainingSeconds > 0f
                && IsPlayerSummonResponseWindowActive
                && CanAttemptAction()
                && TryGetNextTierHoldCandidate(lastDecisionContext, out _, out _);
        }

        private void PreserveHeldPlayerSummonResponseWindow()
        {
            float desiredTimer = Mathf.Max(
                playerSummonResponseTimer,
                heldResponseWindowFloorSeconds);
            float extensionSeconds = Mathf.Min(
                desiredTimer - playerSummonResponseTimer,
                heldResponseWindowExtensionRemainingSeconds);
            if (extensionSeconds <= 0f)
            {
                return;
            }

            playerSummonResponseTimer += extensionSeconds;
            heldResponseWindowExtensionRemainingSeconds -= extensionSeconds;
        }

        private void SubscribeBasicFireEmitter()
        {
            if (basicFireSubscribed || basicFireEmitter == null)
            {
                return;
            }

            basicFireEmitter.VolleyFired += HandleBasicFireVolleyFired;
            basicFireSubscribed = true;
        }

        private void UnsubscribeBasicFireEmitter()
        {
            if (!basicFireSubscribed || basicFireEmitter == null)
            {
                basicFireSubscribed = false;
                return;
            }

            basicFireEmitter.VolleyFired -= HandleBasicFireVolleyFired;
            basicFireSubscribed = false;
        }

        private void HandleBasicFireVolleyFired(BossBasicFireEmitter emitter, int projectileCount)
        {
            totalBasicShotVolleys++;
            lastBasicShotProjectileCount = Mathf.Max(0, projectileCount);
            lastBasicShotAgeSeconds = 0f;
        }

        private void SuppressBasicFireForPressureAction(BossPressureActionSlot slot)
        {
            if (basicFireEmitter == null || basicFireSuppressionSecondsAfterPressureAction <= 0f)
            {
                return;
            }

            float suppressionSeconds = basicFireSuppressionSecondsAfterPressureAction;
            if (slot.ActionKind == BossPressureActionKind.PunishOverextend)
            {
                suppressionSeconds += 0.2f;
            }

            basicFireEmitter.SuppressAutoFire(suppressionSeconds);
        }
    }
}
