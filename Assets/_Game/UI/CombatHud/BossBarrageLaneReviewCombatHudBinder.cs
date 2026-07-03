using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class BossBarrageLaneReviewCombatHudBinder : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private CombatHudPresenter hudPresenter;
        [SerializeField] private CombatHudInputBridge inputBridge;
        [SerializeField] private BossBarrageLaneReviewOverlayHud overlayHud;
        [SerializeField] private BossBarrageLaneReviewTutorialGuide tutorialGuide;
        [SerializeField] private bool useSingleSummonPresentation;

        [Header("Combat State")]
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth bossHealth;
        [SerializeField] private SummonEnergyLadder energyLadder;

        [Header("Player Actions")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;

        private void Awake()
        {
            if (hudPresenter == null)
            {
                hudPresenter = GetComponentInChildren<CombatHudPresenter>(includeInactive: true);
            }

            if (inputBridge == null)
            {
                inputBridge = GetComponentInChildren<CombatHudInputBridge>(includeInactive: true);
            }

            if (tutorialGuide == null)
            {
                tutorialGuide = GetComponent<BossBarrageLaneReviewTutorialGuide>();
            }

            BindTutorialGuide();
        }

        private void OnEnable()
        {
            BindTutorialGuide();

            if (inputBridge != null)
            {
                inputBridge.ActionRequested += HandleActionRequested;
                inputBridge.ActionHoldChanged += HandleActionHoldChanged;
            }
        }

        private void OnDisable()
        {
            if (inputBridge != null)
            {
                inputBridge.ActionRequested -= HandleActionRequested;
                inputBridge.ActionHoldChanged -= HandleActionHoldChanged;
            }

            rangedBasicAttackAction?.SetFireHeld(false);
        }

        private void Update()
        {
            if (hudPresenter == null)
            {
                return;
            }

            tutorialGuide?.TickTutorial(Time.deltaTime);
            UpdatePrimaryReadouts();
            UpdateActionReadouts();
            UpdateSummonReadouts();
            UpdateTutorialGuideReadouts();
        }

        private void UpdatePrimaryReadouts()
        {
            string objective = tutorialGuide != null && tutorialGuide.HasReadoutOverride
                ? tutorialGuide.CurrentObjective
                : pocketReviewOwner != null
                    ? pocketReviewOwner.ObjectiveCue
                    : "Survive the boss lane.";
            hudPresenter.SetObjective(objective);
            hudPresenter.SetTimer(ResolveRemainingSeconds());
            if (playerHealth != null)
            {
                hudPresenter.SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (bossHealth != null)
            {
                hudPresenter.SetBossHealth(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            }

            bool rangedMode = combatModeController == null || combatModeController.IsRangedMode;
            bool aimActive = rangedBasicAttackAction != null && rangedBasicAttackAction.IsAimPreviewActive;
            hudPresenter.SetAimReticleVisible(rangedMode, aimActive);

            if (energyLadder != null)
            {
                hudPresenter.SetResource(energyLadder.CurrentMana, Mathf.Max(1f, energyLadder.MaxMana));
                hudPresenter.SetInputMode(ResolveEnergyInputModeLabel());
            }

            hudPresenter.SetAmmo(ResolveAmmoReadout(), rangedBasicAttackAction != null && rangedBasicAttackAction.IsReloading);

            string feedback = tutorialGuide != null && tutorialGuide.HasReadoutOverride
                ? tutorialGuide.CurrentPrompt
                : ResolveCombatModeLabel();
            hudPresenter.SetActionFeedbackText(feedback);
        }

        private void UpdateActionReadouts()
        {
            bool canSpend = energyLadder != null && energyLadder.CanSpend;
            int tier = canSpend ? energyLadder.AvailableTier : energyLadder != null ? energyLadder.ChargingTier : 0;
            hudPresenter.SetSkillCooldown(CombatHudActionId.BasicAttack, 0f, ResolveBasicAttackLabel());
            hudPresenter.SetSkillCooldown(CombatHudActionId.Dodge, 0f, "DODGE");
            hudPresenter.SetSkillCooldown(CombatHudActionId.Skill1, canSpend ? 0f : 1f, tier > 0 ? $"SKILL LV{tier}" : "SKILL");
            hudPresenter.SetSkillCooldown(CombatHudActionId.Ultimate, 0f, "SWAP");
        }

        private void UpdateSummonReadouts()
        {
            hudPresenter.SetSummonSlotVisible(CombatHudActionId.SummonSlot1, true);
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot1,
                "S1",
                ResolvePrimarySummonState(),
                IsPrimarySummonReady(),
                ResolvePrimarySummonAvailabilityFill01());
            bool showSupportSummonSlots = !useSingleSummonPresentation;
            hudPresenter.SetSummonSlotVisible(CombatHudActionId.SummonSlot2, showSupportSummonSlots);
            hudPresenter.SetSummonSlotVisible(CombatHudActionId.SummonSlot3, showSupportSummonSlots);
            if (!showSupportSummonSlots)
            {
                return;
            }

            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot2,
                "S2",
                ResolveSupportSummonState(summonSlot2Action),
                IsSupportSummonReady(summonSlot2Action),
                ResolveSupportSummonAvailabilityFill01(summonSlot2Action));
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot3,
                "S3",
                ResolveSupportSummonState(summonSlot3Action),
                IsSupportSummonReady(summonSlot3Action),
                ResolveSupportSummonAvailabilityFill01(summonSlot3Action));
        }

        private void UpdateTutorialGuideReadouts()
        {
            if (tutorialGuide == null || !tutorialGuide.HasActiveStep)
            {
                hudPresenter.SetGuideFocus(CombatHudActionId.None, dimUnfocused: false);
                return;
            }

            hudPresenter.SetGuideFocus(
                tutorialGuide.CurrentFocusAction,
                tutorialGuide.CurrentFocusDimUnfocusedActions);
        }

        private void HandleActionRequested(CombatHudActionId actionId)
        {
            switch (actionId)
            {
                case CombatHudActionId.BasicAttack:
                    QueueBasicAttack();
                    break;
                case CombatHudActionId.Dodge:
                    actionController?.QueueDodge();
                    break;
                case CombatHudActionId.Skill1:
                    skill1Action?.QueueSkill1();
                    break;
                case CombatHudActionId.Ultimate:
                    combatModeController?.QueueCombatModeSwap();
                    break;
                case CombatHudActionId.SummonSlot1:
                    summonSlot1Action?.QueueSummonSlot1();
                    break;
                case CombatHudActionId.SummonSlot2:
                    summonSlot2Action?.QueueSummon();
                    break;
                case CombatHudActionId.SummonSlot3:
                    summonSlot3Action?.QueueSummon();
                    break;
                case CombatHudActionId.Pause:
                    overlayHud?.OpenPauseMenu();
                    break;
            }
        }

        private void HandleActionHoldChanged(CombatHudActionId actionId, bool held)
        {
            if (actionId == CombatHudActionId.BasicAttack)
            {
                SetBasicAttackHeld(held);
            }
        }

        private void QueueBasicAttack()
        {
            if (combatModeController == null || combatModeController.IsRangedMode)
            {
                rangedBasicAttackAction?.QueueFire();
                return;
            }

            actionController?.QueueBasicAttack();
        }

        private void SetBasicAttackHeld(bool held)
        {
            if (combatModeController == null || combatModeController.IsRangedMode)
            {
                rangedBasicAttackAction?.SetFireHeld(held);
                return;
            }

            if (!held)
            {
                rangedBasicAttackAction?.SetFireHeld(false);
            }
        }

        private void BindTutorialGuide()
        {
            if (tutorialGuide == null)
            {
                return;
            }

            tutorialGuide.BindRuntimeContext(
                pocketReviewOwner,
                energyLadder,
                actionController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action);
        }

        private float ResolveRemainingSeconds()
        {
            if (pocketReviewOwner == null)
            {
                return 0f;
            }

            float target = pocketReviewOwner.StageProfile != null
                ? pocketReviewOwner.StageProfile.TargetDurationSeconds
                : 90f;
            return Mathf.Max(0f, target - pocketReviewOwner.ElapsedSeconds);
        }

        private string ResolveBasicAttackLabel()
        {
            return combatModeController != null && combatModeController.IsMeleeMode ? "SLASH" : "FIRE";
        }

        private string ResolveAmmoReadout()
        {
            if (combatModeController != null && combatModeController.IsMeleeMode)
            {
                return string.Empty;
            }

            if (rangedBasicAttackAction == null || !rangedBasicAttackAction.UsesMagazineReload)
            {
                return string.Empty;
            }

            string ammo = $"{rangedBasicAttackAction.CurrentAmmo}/{rangedBasicAttackAction.MagazineSize}";
            return rangedBasicAttackAction.IsReloading
                ? $"{ammo} RLD {rangedBasicAttackAction.ReloadRemaining:0.0}"
                : ammo;
        }

        private string ResolveCombatModeLabel()
        {
            if (bossHealth != null)
            {
                return $"Boss {Mathf.CeilToInt(Mathf.Max(0f, bossHealth.CurrentHealth))}/{Mathf.CeilToInt(Mathf.Max(0f, bossHealth.MaxHealth))}";
            }

            return combatModeController != null && combatModeController.IsMeleeMode ? "Melee" : "Ranged";
        }

        private string ResolveEnergyInputModeLabel()
        {
            if (energyLadder == null)
            {
                return "EN";
            }

            string band = energyLadder.CurrentRiskBand switch
            {
                SummonEnergyRiskBand.ForwardRisk => "FRONT",
                SummonEnergyRiskBand.MidCharge => "MID",
                _ => "BACK"
            };
            string tier = energyLadder.CanSpend ? $"READY LV{energyLadder.AvailableTier}" : $"EN LV{energyLadder.ChargingTier}";
            return $"{band} {tier} x{energyLadder.CurrentGainMultiplier:0.0}";
        }

        private bool IsPrimarySummonReady()
        {
            return summonSlot1Action != null
                && energyLadder != null
                && !summonSlot1Action.IsSlotOnCooldown
                && energyLadder.CanSpendMana(summonSlot1Action.RequiredSummonMana);
        }

        private string ResolvePrimarySummonState()
        {
            if (summonSlot1Action == null || energyLadder == null)
            {
                return "LOCKED";
            }

            if (summonSlot1Action.IsSlotOnCooldown)
            {
                return BuildSummonState(
                    summonSlot1Action.RequiredSummonMana,
                    $"CD {Mathf.Max(0f, summonSlot1Action.SlotCooldownRemaining):0.0}s");
            }

            return IsPrimarySummonReady()
                ? BuildSummonState(
                    summonSlot1Action.RequiredSummonMana,
                    $"READY LV{energyLadder.AvailableTier}")
                : BuildSummonState(
                    summonSlot1Action.RequiredSummonMana,
                    BuildManaWaitText(summonSlot1Action.RequiredSummonMana));
        }

        private bool IsSupportSummonReady(PlayerSupportSummonSlotAction action)
        {
            return action != null
                && energyLadder != null
                && !action.IsSlotOnCooldown
                && energyLadder.AvailableTier >= action.MinimumSummonTier
                && energyLadder.CanSpendMana(action.RequiredSummonMana);
        }

        private string ResolveSupportSummonState(PlayerSupportSummonSlotAction action)
        {
            if (action == null || energyLadder == null)
            {
                return "LOCKED";
            }

            if (action.IsSlotOnCooldown)
            {
                return BuildSummonState(
                    action.RequiredSummonMana,
                    $"CD {Mathf.Max(0f, action.SlotCooldownRemaining):0.0}s");
            }

            return IsSupportSummonReady(action)
                ? BuildSummonState(
                    action.RequiredSummonMana,
                    $"READY LV{energyLadder.AvailableTier}")
                : BuildSummonState(
                    action.RequiredSummonMana,
                    BuildManaWaitText(ResolveSupportGateMana(action)));
        }

        private float ResolvePrimarySummonAvailabilityFill01()
        {
            if (summonSlot1Action == null || energyLadder == null)
            {
                return 0f;
            }

            if (summonSlot1Action.IsSlotOnCooldown)
            {
                return ResolveCooldownProgress01(
                    summonSlot1Action.SlotCooldownRemaining,
                    summonSlot1Action.SlotCooldownSeconds);
            }

            return ResolveManaProgress01(summonSlot1Action.RequiredSummonMana);
        }

        private float ResolveSupportSummonAvailabilityFill01(PlayerSupportSummonSlotAction action)
        {
            if (action == null || energyLadder == null)
            {
                return 0f;
            }

            if (action.IsSlotOnCooldown)
            {
                return ResolveCooldownProgress01(action.SlotCooldownRemaining, action.SlotCooldownSeconds);
            }

            return ResolveManaProgress01(ResolveSupportGateMana(action));
        }

        private float ResolveSupportGateMana(PlayerSupportSummonSlotAction action)
        {
            if (action == null)
            {
                return 1f;
            }

            float minimumTierMana = energyLadder != null
                ? energyLadder.GetMinimumManaForTier(action.MinimumSummonTier)
                : 1f;
            return Mathf.Max(action.RequiredSummonMana, minimumTierMana);
        }

        private string BuildManaWaitText(float requiredMana)
        {
            if (energyLadder == null)
            {
                return "WAIT";
            }

            float shortage = energyLadder.GetManaShortage(requiredMana);
            if (shortage <= 0.001f)
            {
                return $"LV{energyLadder.ChargingTier}";
            }

            float seconds = energyLadder.EstimateSecondsToMana(requiredMana);
            string eta = seconds >= 0f ? $" / {Mathf.CeilToInt(seconds)}s" : string.Empty;
            return $"+{Mathf.CeilToInt(shortage)}{eta}";
        }

        private float ResolveManaProgress01(float requiredMana)
        {
            if (energyLadder == null)
            {
                return 0f;
            }

            return Mathf.Clamp01(energyLadder.CurrentMana / Mathf.Max(1f, requiredMana));
        }

        private static float ResolveCooldownProgress01(float cooldownRemaining, float cooldownSeconds)
        {
            if (cooldownSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - Mathf.Max(0f, cooldownRemaining) / cooldownSeconds);
        }

        private static string BuildSummonState(float requiredMana, string status)
        {
            return $"{Mathf.CeilToInt(Mathf.Max(1f, requiredMana))}EN\n{status}";
        }
    }
}
