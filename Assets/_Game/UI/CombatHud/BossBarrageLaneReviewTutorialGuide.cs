using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageLaneReviewTutorialGuide : MonoBehaviour
    {
        [SerializeField] private BossBarrageLaneReviewTutorialProfile profile;
        [SerializeField] private bool startOnEnable = true;

        private BossBarragePocketReviewOwner pocketReviewOwner;
        private SummonEnergyLadder energyLadder;
        private PlayerActionController actionController;
        private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        private PlayerSkill1Action skill1Action;
        private PlayerSummonSlot1Action summonSlot1Action;

        private int stepIndex;
        private float stepTimer;
        private bool subscribed;
        private bool completed;
        private bool failed;
        private bool pocketGuideStarted;
        private bool dodgeObserved;
        private bool basicDefenseFireObserved;
        private bool forwardRiskObserved;
        private bool summonBlockOpportunityObserved;
        private bool summonSlot1PressureBlockedObserved;
        private bool summonFollowupWindowObserved;
        private bool skill1UsedObserved;
        private bool skill1FollowupHitObserved;
        private int highestTierObserved;

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
                    return profile.FailObjective;
                }

                if (completed)
                {
                    return profile.ClearObjective;
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
                    return completed || failed ? CurrentObjective : null;
                }

                return CurrentStep.PromptText;
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
            CaptureCurrentState();
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
            CaptureCurrentState();
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

            stepTimer += Mathf.Max(0f, deltaTime);
            if (IsStepSatisfied(step))
            {
                AdvanceStep();
            }
        }

        public void RestartGuide()
        {
            stepIndex = 0;
            stepTimer = 0f;
            completed = false;
            failed = false;
            pocketGuideStarted = IsPocketReadyForGuide;
            dodgeObserved = false;
            basicDefenseFireObserved = false;
            forwardRiskObserved = false;
            summonBlockOpportunityObserved = false;
            summonSlot1PressureBlockedObserved = false;
            summonFollowupWindowObserved = false;
            skill1UsedObserved = false;
            skill1FollowupHitObserved = false;
            highestTierObserved = 0;
            CaptureCurrentState();
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

        private void CaptureCurrentState()
        {
            if (pocketReviewOwner != null)
            {
                failed |= pocketReviewOwner.IsFailed;
                completed |= pocketReviewOwner.IsCleared;
                summonBlockOpportunityObserved |= pocketReviewOwner.CloseThreatDefeated;
                summonSlot1PressureBlockedObserved |= pocketReviewOwner.BlockedBossPressureWithSummon;
                skill1FollowupHitObserved |= pocketReviewOwner.Skill1FollowupHitConfirmed;
            }

            if (energyLadder != null)
            {
                highestTierObserved = Mathf.Max(highestTierObserved, energyLadder.AvailableTier);
                forwardRiskObserved |= energyLadder.CurrentRiskBand == SummonEnergyRiskBand.ForwardRisk;
            }
        }

        private bool IsStepSatisfied(BossBarrageLaneReviewTutorialProfile.Step step)
        {
            if (stepTimer < step.MinimumSeconds)
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
                    return highestTierObserved >= Mathf.Max(1, step.RequiredTier);
                case BossBarrageLaneReviewTutorialCondition.SummonSlot1Ready:
                    return IsSummonSlot1Ready(step);
                case BossBarrageLaneReviewTutorialCondition.CloseThreatDefeated:
                    return pocketReviewOwner != null && pocketReviewOwner.CloseThreatDefeated;
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
                    return pocketReviewOwner != null && pocketReviewOwner.IsCleared;
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

        private void AdvanceStep()
        {
            stepIndex++;
            stepTimer = 0f;
            if (profile == null || stepIndex >= profile.StepCount)
            {
                completed = true;
            }
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

        private bool CanRecordTutorialEvent()
        {
            if (!IsTutorialEnabled || !IsPocketReadyForGuide)
            {
                return false;
            }

            EnsureGuideStartedForPocket();
            return true;
        }

        private void HandleDodgeStarted()
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            dodgeObserved = true;
        }

        private void HandleBasicAttackStarted(int _)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            basicDefenseFireObserved = true;
        }

        private void HandleRangedFireStarted()
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            basicDefenseFireObserved = true;
        }

        private void HandleRangedProjectileFired(LaneActionProjectile _)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            basicDefenseFireObserved = true;
        }

        private void HandleRiskBandChanged(SummonEnergyRiskBand riskBand)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            forwardRiskObserved |= riskBand == SummonEnergyRiskBand.ForwardRisk;
        }

        private void HandleTierAvailable(int tier)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            highestTierObserved = Mathf.Max(highestTierObserved, tier);
        }

        private void HandleSummonBlockOpportunityOpened()
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            summonBlockOpportunityObserved = true;
        }

        private void HandleSummonPressureBlocked(int tier)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            summonSlot1PressureBlockedObserved = true;
            highestTierObserved = Mathf.Max(highestTierObserved, tier);
        }

        private void HandleSummonFollowupWindowOpened(int tier)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            summonFollowupWindowObserved = true;
            highestTierObserved = Mathf.Max(highestTierObserved, tier);
        }

        private void HandleSkill1Used(int tier)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            skill1UsedObserved = true;
            highestTierObserved = Mathf.Max(highestTierObserved, tier);
        }

        private void HandleSummonFollowupHitConfirmed(int tier, float _)
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            skill1FollowupHitObserved = true;
            highestTierObserved = Mathf.Max(highestTierObserved, tier);
        }

        private void HandlePocketCleared()
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            completed = true;
        }

        private void HandlePocketFailed()
        {
            if (!CanRecordTutorialEvent())
            {
                return;
            }

            failed = true;
        }
    }
}
