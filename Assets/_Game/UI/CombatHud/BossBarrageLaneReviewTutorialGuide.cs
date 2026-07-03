using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageLaneReviewTutorialGuide : MonoBehaviour
    {
        private const int BufferedConditionCapacity = 8;

        [SerializeField] private BossBarrageLaneReviewTutorialProfile profile;
        [SerializeField] private bool startOnEnable = true;
        [SerializeField, Min(0f)] private float minimumStepReadSeconds = 0.85f;
        [SerializeField, Min(0f)] private float completionHoldSeconds = 0.85f;

        private BossBarragePocketReviewOwner pocketReviewOwner;
        private SummonEnergyLadder energyLadder;
        private PlayerActionController actionController;
        private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        private PlayerSkill1Action skill1Action;
        private PlayerSummonSlot1Action summonSlot1Action;

        private int stepIndex;
        private float stepTimer;
        private float completionTimer;
        private bool subscribed;
        private bool completed;
        private bool failed;
        private bool pocketGuideStarted;
        private bool stepCompletionPending;
        private string completionPrompt;
        private bool dodgeObserved;
        private bool basicDefenseFireObserved;
        private bool forwardRiskObserved;
        private bool summonBlockOpportunityObserved;
        private bool summonSlot1PressureBlockedObserved;
        private bool summonFollowupWindowObserved;
        private bool skill1UsedObserved;
        private bool skill1FollowupHitObserved;
        private bool energyTierAvailableObserved;
        private bool summonSlot1ReadyObserved;
        private bool pocketClearedObserved;
        private float sustainedConditionTimer;
        private readonly BossBarrageLaneReviewTutorialCondition[] bufferedConditions =
            new BossBarrageLaneReviewTutorialCondition[BufferedConditionCapacity];
        private int bufferedConditionCount;

        public bool HasReadoutOverride => IsTutorialEnabled && IsPocketReadyForGuide && (HasActiveStep || completed || failed);
        public bool HasActiveStep => IsTutorialEnabled && IsPocketReadyForGuide && !completed && !failed && CurrentStep != null;
        public CombatHudActionId CurrentFocusAction => HasActiveStep ? CurrentStep.FocusAction : CombatHudActionId.None;
        public bool CurrentFocusDimUnfocusedActions => HasActiveStep && CurrentStep.DimUnfocusedActions;

        public string CurrentObjective
        {
            get
            {
                if (!IsTutorialEnabled)
                {
                    return null;
                }

                if (failed)
                {
                    return ResolveFailedObjective();
                }

                if (completed)
                {
                    return ResolveCompletedObjective();
                }

                return CurrentStep != null ? CurrentStep.ObjectiveText : null;
            }
        }

        public string CurrentPrompt
        {
            get
            {
                if (!HasActiveStep)
                {
                    if (failed)
                    {
                        return ResolveFailedPrompt();
                    }

                    if (completed)
                    {
                        return ResolveCompletedPrompt();
                    }

                    return null;
                }

                if (stepCompletionPending)
                {
                    return completionPrompt;
                }

                return ResolveActivePrompt(CurrentStep);
            }
        }

        private bool IsTutorialEnabled => profile != null && profile.TutorialEnabled && profile.StepCount > 0;
        private bool IsPocketReadyForGuide => pocketReviewOwner != null && pocketReviewOwner.isActiveAndEnabled;
        private BossBarrageLaneReviewTutorialProfile.Step CurrentStep => profile != null ? profile.GetStep(stepIndex) : null;

        private void OnEnable()
        {
            if (startOnEnable)
            {
                RestartGuide();
            }

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void BindRuntimeContext(
            BossBarragePocketReviewOwner newPocketReviewOwner,
            SummonEnergyLadder newEnergyLadder,
            PlayerActionController newActionController,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action)
        {
            bool changed = pocketReviewOwner != newPocketReviewOwner
                || energyLadder != newEnergyLadder
                || actionController != newActionController
                || rangedBasicAttackAction != newRangedBasicAttackAction
                || skill1Action != newSkill1Action
                || summonSlot1Action != newSummonSlot1Action;
            if (!changed)
            {
                return;
            }

            Unsubscribe();
            pocketReviewOwner = newPocketReviewOwner;
            energyLadder = newEnergyLadder;
            actionController = newActionController;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            CaptureTerminalState();
            ClearBufferedObservations();
            ArmCurrentStep();
            Subscribe();
        }

        public void TickTutorial(float deltaTime)
        {
            if (!IsTutorialEnabled)
            {
                return;
            }

            if (!IsPocketReadyForGuide)
            {
                pocketGuideStarted = false;
                return;
            }

            EnsureGuideStartedForPocket();
            CaptureTerminalState();
            if (completed || failed)
            {
                return;
            }

            BossBarrageLaneReviewTutorialProfile.Step step = CurrentStep;
            if (step == null)
            {
                completed = true;
                return;
            }

            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            stepTimer += safeDeltaTime;
            if (stepCompletionPending)
            {
                completionTimer += safeDeltaTime;
                if (completionTimer >= completionHoldSeconds)
                {
                    AdvanceStep();
                }

                return;
            }

            UpdateSustainedStepObservation(step, safeDeltaTime);
            if (IsStepSatisfied(step))
            {
                ConfirmStep(step);
            }
        }

        public void RestartGuide()
        {
            stepIndex = 0;
            stepTimer = 0f;
            completionTimer = 0f;
            completed = false;
            failed = false;
            pocketGuideStarted = IsPocketReadyForGuide;
            stepCompletionPending = false;
            completionPrompt = string.Empty;
            dodgeObserved = false;
            basicDefenseFireObserved = false;
            forwardRiskObserved = false;
            summonBlockOpportunityObserved = false;
            summonSlot1PressureBlockedObserved = false;
            summonFollowupWindowObserved = false;
            skill1UsedObserved = false;
            skill1FollowupHitObserved = false;
            energyTierAvailableObserved = false;
            summonSlot1ReadyObserved = false;
            pocketClearedObserved = false;
            sustainedConditionTimer = 0f;
            ClearBufferedObservations();
            CaptureTerminalState();
            ArmCurrentStep();
        }

        private void Subscribe()
        {
            if (subscribed || !isActiveAndEnabled)
            {
                return;
            }

            if (pocketReviewOwner != null)
            {
                pocketReviewOwner.SummonBlockOpportunityOpened += HandleSummonBlockOpportunityOpened;
                pocketReviewOwner.SummonFollowupWindowOpened += HandleSummonFollowupWindowOpened;
                pocketReviewOwner.SummonFollowupHitConfirmed += HandleSummonFollowupHitConfirmed;
                pocketReviewOwner.PocketCleared += HandlePocketCleared;
                pocketReviewOwner.PocketFailed += HandlePocketFailed;
            }

            if (energyLadder != null)
            {
                energyLadder.RiskBandChanged += HandleRiskBandChanged;
                energyLadder.TierAvailable += HandleTierAvailable;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted += HandleDodgeStarted;
                actionController.BasicAttackStarted += HandleBasicAttackStarted;
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.RangedFireStarted += HandleRangedFireStarted;
                rangedBasicAttackAction.RangedProjectileFired += HandleRangedProjectileFired;
            }

            if (skill1Action != null)
            {
                skill1Action.Skill1Used += HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonPressureBlocked += HandleSummonPressureBlocked;
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (pocketReviewOwner != null)
            {
                pocketReviewOwner.SummonBlockOpportunityOpened -= HandleSummonBlockOpportunityOpened;
                pocketReviewOwner.SummonFollowupWindowOpened -= HandleSummonFollowupWindowOpened;
                pocketReviewOwner.SummonFollowupHitConfirmed -= HandleSummonFollowupHitConfirmed;
                pocketReviewOwner.PocketCleared -= HandlePocketCleared;
                pocketReviewOwner.PocketFailed -= HandlePocketFailed;
            }

            if (energyLadder != null)
            {
                energyLadder.RiskBandChanged -= HandleRiskBandChanged;
                energyLadder.TierAvailable -= HandleTierAvailable;
            }

            if (actionController != null)
            {
                actionController.DodgeStarted -= HandleDodgeStarted;
                actionController.BasicAttackStarted -= HandleBasicAttackStarted;
            }

            if (rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.RangedFireStarted -= HandleRangedFireStarted;
                rangedBasicAttackAction.RangedProjectileFired -= HandleRangedProjectileFired;
            }

            if (skill1Action != null)
            {
                skill1Action.Skill1Used -= HandleSkill1Used;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonPressureBlocked -= HandleSummonPressureBlocked;
            }

            subscribed = false;
        }

        private void CaptureTerminalState()
        {
            if (pocketReviewOwner != null)
            {
                failed |= pocketReviewOwner.IsFailed;
            }
        }

        private void ArmCurrentStep()
        {
            stepTimer = 0f;
            completionTimer = 0f;
            stepCompletionPending = false;
            completionPrompt = string.Empty;
            sustainedConditionTimer = 0f;
            ResetStepObservations();
            ApplyBufferedObservation(CurrentStep);
            ClearBufferedObservations();
        }

        private void ResetStepObservations()
        {
            dodgeObserved = false;
            basicDefenseFireObserved = false;
            forwardRiskObserved = false;
            summonBlockOpportunityObserved = false;
            summonSlot1PressureBlockedObserved = false;
            summonFollowupWindowObserved = false;
            skill1UsedObserved = false;
            skill1FollowupHitObserved = false;
            energyTierAvailableObserved = false;
            summonSlot1ReadyObserved = false;
            pocketClearedObserved = false;
        }

        private void UpdateSustainedStepObservation(
            BossBarrageLaneReviewTutorialProfile.Step step,
            float deltaTime)
        {
            if (step == null)
            {
                return;
            }

            switch (step.CompletionCondition)
            {
                case BossBarrageLaneReviewTutorialCondition.ForwardRiskEntered:
                    ObserveSustainedCondition(
                        energyLadder != null && energyLadder.CurrentRiskBand == SummonEnergyRiskBand.ForwardRisk,
                        deltaTime,
                        ref forwardRiskObserved);
                    break;
                case BossBarrageLaneReviewTutorialCondition.EnergyTierAvailable:
                    ObserveSustainedCondition(
                        energyLadder != null && energyLadder.AvailableTier >= Mathf.Max(1, step.RequiredTier),
                        deltaTime,
                        ref energyTierAvailableObserved);
                    break;
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1Ready:
                    ObserveSustainedCondition(IsSummonSlot1Ready(step), deltaTime, ref summonSlot1ReadyObserved);
                    break;
                case BossBarrageLaneReviewTutorialCondition.PocketCleared:
                    ObserveSustainedCondition(
                        pocketReviewOwner != null && pocketReviewOwner.IsCleared,
                        deltaTime,
                        ref pocketClearedObserved);
                    break;
            }
        }

        private void ObserveSustainedCondition(bool conditionIsTrue, float deltaTime, ref bool observed)
        {
            if (observed)
            {
                return;
            }

            if (!conditionIsTrue)
            {
                sustainedConditionTimer = 0f;
                return;
            }

            sustainedConditionTimer += deltaTime;
            observed = sustainedConditionTimer >= minimumStepReadSeconds;
        }

        private bool IsCurrentStepCondition(BossBarrageLaneReviewTutorialCondition condition)
        {
            BossBarrageLaneReviewTutorialProfile.Step step = CurrentStep;
            return step != null && step.CompletionCondition == condition;
        }

        private bool CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition condition)
        {
            if (!IsTutorialEnabled || !IsPocketReadyForGuide || completed || failed)
            {
                return false;
            }

            EnsureGuideStartedForPocket();
            if (stepCompletionPending)
            {
                BufferTutorialEvent(condition);
                return false;
            }

            return IsCurrentStepCondition(condition);
        }

        private bool CanRecordTutorialEvent(params BossBarrageLaneReviewTutorialCondition[] conditions)
        {
            if (!IsTutorialEnabled || !IsPocketReadyForGuide || completed || failed)
            {
                return false;
            }

            EnsureGuideStartedForPocket();
            if (stepCompletionPending)
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    BufferTutorialEvent(conditions[i]);
                }

                return false;
            }

            for (int i = 0; i < conditions.Length; i++)
            {
                if (IsCurrentStepCondition(conditions[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void BufferTutorialEvent(BossBarrageLaneReviewTutorialCondition condition)
        {
            if (!CanBufferCondition(condition))
            {
                return;
            }

            for (int i = 0; i < bufferedConditionCount; i++)
            {
                if (bufferedConditions[i] == condition)
                {
                    return;
                }
            }

            if (bufferedConditionCount >= bufferedConditions.Length)
            {
                return;
            }

            bufferedConditions[bufferedConditionCount] = condition;
            bufferedConditionCount++;
        }

        private void ApplyBufferedObservation(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            if (step == null)
            {
                return;
            }

            for (int i = 0; i < bufferedConditionCount; i++)
            {
                if (bufferedConditions[i] != step.CompletionCondition)
                {
                    continue;
                }

                RecordDiscreteObservation(step.CompletionCondition);
                return;
            }
        }

        private void ClearBufferedObservations()
        {
            for (int i = 0; i < bufferedConditionCount; i++)
            {
                bufferedConditions[i] = BossBarrageLaneReviewTutorialCondition.None;
            }

            bufferedConditionCount = 0;
        }

        private void RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition condition)
        {
            switch (condition)
            {
                case BossBarrageLaneReviewTutorialCondition.DodgeStarted:
                    dodgeObserved = true;
                    break;
                case BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed:
                    basicDefenseFireObserved = true;
                    break;
                case BossBarrageLaneReviewTutorialCondition.CloseThreatDefeated:
                case BossBarrageLaneReviewTutorialCondition.SummonBlockOpportunityOpened:
                    summonBlockOpportunityObserved = true;
                    break;
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1PressureBlocked:
                    summonSlot1PressureBlockedObserved = true;
                    break;
                case BossBarrageLaneReviewTutorialCondition.SummonFollowupWindowOpened:
                    summonFollowupWindowObserved = true;
                    break;
                case BossBarrageLaneReviewTutorialCondition.Skill1Used:
                    skill1UsedObserved = true;
                    break;
                case BossBarrageLaneReviewTutorialCondition.Skill1FollowupHit:
                    skill1FollowupHitObserved = true;
                    break;
                case BossBarrageLaneReviewTutorialCondition.PocketCleared:
                    pocketClearedObserved = true;
                    break;
            }
        }

        private static bool CanBufferCondition(BossBarrageLaneReviewTutorialCondition condition)
        {
            switch (condition)
            {
                case BossBarrageLaneReviewTutorialCondition.DodgeStarted:
                case BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed:
                case BossBarrageLaneReviewTutorialCondition.CloseThreatDefeated:
                case BossBarrageLaneReviewTutorialCondition.SummonBlockOpportunityOpened:
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1PressureBlocked:
                case BossBarrageLaneReviewTutorialCondition.SummonFollowupWindowOpened:
                case BossBarrageLaneReviewTutorialCondition.Skill1Used:
                case BossBarrageLaneReviewTutorialCondition.Skill1FollowupHit:
                case BossBarrageLaneReviewTutorialCondition.PocketCleared:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsStepSatisfied(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            if (stepTimer < Mathf.Max(step.MinimumSeconds, minimumStepReadSeconds))
            {
                return false;
            }

            switch (step.CompletionCondition)
            {
                case BossBarrageLaneReviewTutorialCondition.None:
                    return false;
                case BossBarrageLaneReviewTutorialCondition.TimeElapsed:
                    return true;
                case BossBarrageLaneReviewTutorialCondition.DodgeStarted:
                    return dodgeObserved;
                case BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed:
                    return basicDefenseFireObserved;
                case BossBarrageLaneReviewTutorialCondition.ForwardRiskEntered:
                    return forwardRiskObserved;
                case BossBarrageLaneReviewTutorialCondition.EnergyTierAvailable:
                    return energyTierAvailableObserved;
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1Ready:
                    return summonSlot1ReadyObserved;
                case BossBarrageLaneReviewTutorialCondition.CloseThreatDefeated:
                    return summonBlockOpportunityObserved;
                case BossBarrageLaneReviewTutorialCondition.SummonBlockOpportunityOpened:
                    return summonBlockOpportunityObserved;
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1PressureBlocked:
                    return summonSlot1PressureBlockedObserved;
                case BossBarrageLaneReviewTutorialCondition.SummonFollowupWindowOpened:
                    return summonFollowupWindowObserved;
                case BossBarrageLaneReviewTutorialCondition.Skill1Used:
                    return skill1UsedObserved;
                case BossBarrageLaneReviewTutorialCondition.Skill1FollowupHit:
                    return skill1FollowupHitObserved;
                case BossBarrageLaneReviewTutorialCondition.PocketCleared:
                    return pocketClearedObserved;
                default:
                    return false;
            }
        }

        private bool IsSummonSlot1Ready(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            if (energyLadder == null || summonSlot1Action == null || summonSlot1Action.IsSlotOnCooldown)
            {
                return false;
            }

            float requiredMana = step.RequiredMana > 0f ? step.RequiredMana : summonSlot1Action.RequiredSummonMana;
            return energyLadder.AvailableTier >= Mathf.Max(1, step.RequiredTier)
                && energyLadder.CanSpendMana(requiredMana);
        }

        private void ConfirmStep(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            stepCompletionPending = true;
            completionTimer = 0f;
            completionPrompt = ResolveCompletionPrompt(step);
        }

        private void AdvanceStep()
        {
            stepIndex++;
            if (profile == null || stepIndex >= profile.StepCount)
            {
                completed = true;
                stepCompletionPending = false;
                completionPrompt = string.Empty;
                return;
            }

            ArmCurrentStep();
        }

        private string ResolveActivePrompt(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            if (step == null)
            {
                return null;
            }

            return CombineReadouts(step.PromptText, ResolveStepLiveReadout(step));
        }

        private string ResolveCompletedObjective()
        {
            if (pocketReviewOwner != null && pocketReviewOwner.HasCommittedResultRecord)
            {
                return pocketReviewOwner.LastResultRecord.Title;
            }

            if (pocketReviewOwner != null && pocketReviewOwner.IsCleared)
            {
                return pocketReviewOwner.ObjectiveCue;
            }

            return profile.ClearObjective;
        }

        private string ResolveCompletedPrompt()
        {
            if (pocketReviewOwner == null)
            {
                return profile.ClearObjective;
            }

            if (pocketReviewOwner.HasCommittedResultRecord)
            {
                BossBarragePocketReviewOwner.RouteResultRecord record = pocketReviewOwner.LastResultRecord;
                return CombineReadouts(
                    record.Summary,
                    ResolveShortPocketRecordReadout());
            }

            return ResolvePocketRecordReadout();
        }

        private string ResolveFailedObjective()
        {
            if (pocketReviewOwner != null)
            {
                return pocketReviewOwner.ObjectiveCue;
            }

            return profile.FailObjective;
        }

        private string ResolveFailedPrompt()
        {
            if (pocketReviewOwner == null)
            {
                return profile.FailObjective;
            }

            if (pocketReviewOwner.HasCommittedResultRecord)
            {
                BossBarragePocketReviewOwner.RouteResultRecord record = pocketReviewOwner.LastResultRecord;
                return CombineReadouts(
                    record.Summary,
                    ResolveShortPocketRecordReadout());
            }

            return ResolvePocketRecordReadout();
        }

        private string ResolveCompletionPrompt(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            if (step == null)
            {
                return "\ud655\uc778\ub428.";
            }

            string prompt;
            switch (step.CompletionCondition)
            {
                case BossBarrageLaneReviewTutorialCondition.DodgeStarted:
                    prompt = "\ud68c\ud53c \ud655\uc778. \ub2e4\uc74c \uc555\ubc15\uc744 \uc77d\uc5b4.";
                    break;
                case BossBarrageLaneReviewTutorialCondition.ForwardRiskEntered:
                    prompt = "EN \ucda9\uc804 \uc704\uce58 \ud655\uc778.";
                    break;
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1Ready:
                case BossBarrageLaneReviewTutorialCondition.EnergyTierAvailable:
                    prompt = "EN \ub2e8\uacc4\uc640 \uc2ac\ub86f \uc900\ube44 \ud655\uc778.";
                    break;
                case BossBarrageLaneReviewTutorialCondition.CloseThreatDefeated:
                case BossBarrageLaneReviewTutorialCondition.SummonBlockOpportunityOpened:
                    prompt = "\uadfc\uc811 \uc555\ubc15 \ucc98\ub9ac \uae30\ub85d.";
                    break;
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1PressureBlocked:
                    prompt = "S1 \ucc28\ub2e8 \uae30\ub85d. \uc5f4\ub9b0 \ud2c8\uc744 \uc77d\uc5b4.";
                    break;
                case BossBarrageLaneReviewTutorialCondition.SummonFollowupWindowOpened:
                    prompt = "Skill1 \ud655\uc778 \ucc3d \uc5f4\ub9bc.";
                    break;
                case BossBarrageLaneReviewTutorialCondition.Skill1FollowupHit:
                    prompt = "Skill1 \ud788\ud2b8 \uae30\ub85d. \uc555\ubc15 \ud574\ub2f5 \uc644\ub8cc.";
                    break;
                case BossBarrageLaneReviewTutorialCondition.PocketCleared:
                    prompt = "\ud3ec\ucf13 \uae30\ub85d \uc644\ub8cc.";
                    break;
                default:
                    prompt = "\ud655\uc778\ub428.";
                    break;
            }

            return CombineReadouts(prompt, ResolveStepLiveReadout(step));
        }

        private string ResolveStepLiveReadout(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            if (step == null)
            {
                return string.Empty;
            }

            switch (step.CompletionCondition)
            {
                case BossBarrageLaneReviewTutorialCondition.ForwardRiskEntered:
                case BossBarrageLaneReviewTutorialCondition.EnergyTierAvailable:
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1Ready:
                    return ResolveEnergyReadout();
                case BossBarrageLaneReviewTutorialCondition.CloseThreatDefeated:
                case BossBarrageLaneReviewTutorialCondition.SummonBlockOpportunityOpened:
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1PressureBlocked:
                case BossBarrageLaneReviewTutorialCondition.SummonFollowupWindowOpened:
                case BossBarrageLaneReviewTutorialCondition.Skill1FollowupHit:
                case BossBarrageLaneReviewTutorialCondition.PocketCleared:
                    return ResolvePocketRecordReadout();
                default:
                    return string.Empty;
            }
        }

        private string ResolveEnergyReadout()
        {
            if (energyLadder == null)
            {
                return string.Empty;
            }

            string band = energyLadder.CurrentRiskBand switch
            {
                SummonEnergyRiskBand.ForwardRisk => "FRONT",
                SummonEnergyRiskBand.MidCharge => "MID",
                _ => "BACK"
            };
            int tier = energyLadder.CanSpend ? energyLadder.AvailableTier : energyLadder.ChargingTier;
            return $"EN {Mathf.CeilToInt(energyLadder.CurrentMana)}/{Mathf.CeilToInt(energyLadder.MaxMana)} {band} LV{tier} x{energyLadder.CurrentGainMultiplier:0.0}";
        }

        private string ResolvePocketRecordReadout()
        {
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            return CombineReadouts(pocketReviewOwner.ObjectiveCue, ResolveShortPocketRecordReadout());
        }

        private string ResolveShortPocketRecordReadout()
        {
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            return $"RECORD close:{ResolveRecordMark(pocketReviewOwner.IsCloseProbeCompletionRecorded)} "
                + $"summon:{ResolveRecordMark(pocketReviewOwner.IsSummonRouteCompletionRecorded)} "
                + $"followup:{ResolveRecordMark(pocketReviewOwner.IsFollowupCompletionRecorded)}";
        }

        private static string ResolveRecordMark(bool recorded)
        {
            return recorded ? "OK" : "--";
        }

        private static string CombineReadouts(string primary, string secondary)
        {
            if (string.IsNullOrWhiteSpace(primary))
            {
                return secondary;
            }

            if (string.IsNullOrWhiteSpace(secondary))
            {
                return primary;
            }

            return $"{primary}\n{secondary}";
        }

        private void EnsureGuideStartedForPocket()
        {
            if (pocketGuideStarted)
            {
                return;
            }

            RestartGuide();
            pocketGuideStarted = true;
        }

        private void HandleDodgeStarted()
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.DodgeStarted))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.DodgeStarted);
        }

        private void HandleBasicAttackStarted(int _)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed);
        }

        private void HandleRangedFireStarted()
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed);
        }

        private void HandleRangedProjectileFired(LaneActionProjectile _)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.BasicDefenseFireUsed);
        }

        private void HandleRiskBandChanged(SummonEnergyRiskBand riskBand)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.ForwardRiskEntered))
            {
                return;
            }

            if (riskBand != SummonEnergyRiskBand.ForwardRisk)
            {
                sustainedConditionTimer = 0f;
            }
        }

        private void HandleTierAvailable(int tier)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.EnergyTierAvailable))
            {
                return;
            }

            BossBarrageLaneReviewTutorialProfile.Step step = CurrentStep;
            energyTierAvailableObserved = step != null && tier >= Mathf.Max(1, step.RequiredTier);
        }

        private void HandleSummonBlockOpportunityOpened()
        {
            if (!CanRecordTutorialEvent(
                BossBarrageLaneReviewTutorialCondition.SummonBlockOpportunityOpened,
                BossBarrageLaneReviewTutorialCondition.CloseThreatDefeated))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.SummonBlockOpportunityOpened);
        }

        private void HandleSummonPressureBlocked(int tier)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.SummonSlot1PressureBlocked))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.SummonSlot1PressureBlocked);
        }

        private void HandleSummonFollowupWindowOpened(int tier)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.SummonFollowupWindowOpened))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.SummonFollowupWindowOpened);
        }

        private void HandleSkill1Used(int tier)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.Skill1Used))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.Skill1Used);
        }

        private void HandleSummonFollowupHitConfirmed(int tier, float _)
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.Skill1FollowupHit))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.Skill1FollowupHit);
        }

        private void HandlePocketCleared()
        {
            if (!CanRecordTutorialEvent(BossBarrageLaneReviewTutorialCondition.PocketCleared))
            {
                return;
            }

            RecordDiscreteObservation(BossBarrageLaneReviewTutorialCondition.PocketCleared);
        }

        private void HandlePocketFailed()
        {
            if (!IsTutorialEnabled || !IsPocketReadyForGuide)
            {
                return;
            }

            EnsureGuideStartedForPocket();
            failed = true;
        }
    }
}
