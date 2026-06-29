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
        }

        private void OnEnable()
        {
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

            UpdatePrimaryReadouts();
            UpdateActionReadouts();
            UpdateSummonReadouts();
        }

        private void UpdatePrimaryReadouts()
        {
            hudPresenter.SetObjective(pocketReviewOwner != null ? pocketReviewOwner.ObjectiveCue : "Survive the boss lane.");
            hudPresenter.SetTimer(ResolveRemainingSeconds());
            if (playerHealth != null)
            {
                hudPresenter.SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (energyLadder != null)
            {
                float target = Mathf.Max(1f, energyLadder.CurrentTierTarget);
                float current = energyLadder.CanSpend ? target : energyLadder.CurrentTierEnergy;
                hudPresenter.SetResource(current, target);
                hudPresenter.SetInputMode(energyLadder.CanSpend ? $"EN LV{energyLadder.AvailableTier}" : $"EN LV{energyLadder.ChargingTier}");
            }

            hudPresenter.SetActionFeedbackText(ResolveCombatModeLabel());
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
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot1,
                "S1",
                ResolvePrimarySummonState(),
                IsPrimarySummonReady());
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot2,
                "S2",
                ResolveSupportSummonState(summonSlot2Action),
                IsSupportSummonReady(summonSlot2Action));
            hudPresenter.SetSummonSlotState(
                CombatHudActionId.SummonSlot3,
                "S3",
                ResolveSupportSummonState(summonSlot3Action),
                IsSupportSummonReady(summonSlot3Action));
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

        private string ResolveCombatModeLabel()
        {
            if (bossHealth != null)
            {
                return $"Boss {Mathf.CeilToInt(Mathf.Max(0f, bossHealth.CurrentHealth))}/{Mathf.CeilToInt(Mathf.Max(0f, bossHealth.MaxHealth))}";
            }

            return combatModeController != null && combatModeController.IsMeleeMode ? "Melee" : "Ranged";
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
                return $"{summonSlot1Action.SlotCooldownRemaining:0.0}s";
            }

            return IsPrimarySummonReady() ? $"READY LV{energyLadder.AvailableTier}" : "CHARGE";
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
                return $"{action.SlotCooldownRemaining:0.0}s";
            }

            return IsSupportSummonReady(action) ? $"READY LV{energyLadder.AvailableTier}" : $"LV{action.MinimumSummonTier}";
        }
    }
}
