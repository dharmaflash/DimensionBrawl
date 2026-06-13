using System;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudPresenter : MonoBehaviour
    {
        [Serializable]
        public sealed class ActionSlotBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private Text labelText;
            [SerializeField] private Text cooldownText;
            [SerializeField] private Image cooldownFill;
            [SerializeField] private CanvasGroup canvasGroup;

            public CombatHudActionId ActionId => actionId;

            public void SetCooldown(float normalizedRemaining, string label)
            {
                if (labelText != null && !string.IsNullOrWhiteSpace(label))
                {
                    labelText.text = label;
                }

                float clamped = Mathf.Clamp01(normalizedRemaining);
                if (cooldownFill != null)
                {
                    cooldownFill.fillAmount = clamped;
                }

                if (cooldownText != null)
                {
                    cooldownText.text = clamped > 0f ? $"{Mathf.CeilToInt(clamped * 10f) / 10f:0.0}s" : string.Empty;
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = clamped > 0f ? 0.65f : 1f;
                }
            }
        }

        [Serializable]
        public sealed class SummonSlotBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private Text labelText;
            [SerializeField] private Text stateText;
            [SerializeField] private Image cooldownFill;
            [SerializeField] private CanvasGroup canvasGroup;

            public CombatHudActionId ActionId => actionId;

            public void SetState(string label, string state, bool enabled)
            {
                if (labelText != null)
                {
                    labelText.text = label;
                }

                if (stateText != null)
                {
                    stateText.text = state;
                }

                if (cooldownFill != null)
                {
                    cooldownFill.fillAmount = enabled ? 0f : 1f;
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = enabled ? 1f : 0.55f;
                    canvasGroup.interactable = enabled;
                    canvasGroup.blocksRaycasts = enabled;
                }
            }
        }

        [SerializeField] private Text objectiveText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text resourceText;
        [SerializeField] private Text inputModeText;
        [SerializeField] private Text actionFeedbackText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image resourceFill;
        [SerializeField] private ActionSlotBinding[] actionSlots = Array.Empty<ActionSlotBinding>();
        [SerializeField] private SummonSlotBinding[] summonSlots = Array.Empty<SummonSlotBinding>();

        private void Start()
        {
            SetObjective("Reach the encounter marker");
            SetTimer(180f);
            SetHealth(840f, 1000f);
            SetResource(56f, 100f);
            SetInputMode("Mobile HUD placeholder");
            SetSummonSlotState(CombatHudActionId.SummonSlot1, "Break", "Visual only", false);
            SetSummonSlotState(CombatHudActionId.SummonSlot2, "Tank", "Locked", false);
            SetSummonSlotState(CombatHudActionId.SummonSlot3, "Heal", "Locked", false);
            SetSkillCooldown(CombatHudActionId.Skill1, 0.35f, "Skill1");
            SetSkillCooldown(CombatHudActionId.Ultimate, 0f, "Ultimate");
        }

        public void SetObjective(string objective)
        {
            SetText(objectiveText, objective);
        }

        public void SetTimer(float secondsRemaining)
        {
            float clamped = Mathf.Max(0f, secondsRemaining);
            int minutes = Mathf.FloorToInt(clamped / 60f);
            int seconds = Mathf.FloorToInt(clamped % 60f);
            SetText(timerText, $"{minutes:00}:{seconds:00}");
        }

        public void SetHealth(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (healthFill != null)
            {
                healthFill.fillAmount = ratio;
            }

            SetText(healthText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetResource(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (resourceFill != null)
            {
                resourceFill.fillAmount = ratio;
            }

            SetText(resourceText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetInputMode(string label)
        {
            SetText(inputModeText, label);
        }

        public void SetSkillCooldown(CombatHudActionId actionId, float normalizedRemaining, string label)
        {
            ActionSlotBinding slot = FindActionSlot(actionId);
            slot?.SetCooldown(normalizedRemaining, label);
        }

        public void SetSummonSlotState(CombatHudActionId actionId, string label, string state, bool enabled)
        {
            SummonSlotBinding slot = FindSummonSlot(actionId);
            slot?.SetState(label, state, enabled);
        }

        public void SetActionFeedback(CombatHudActionId actionId)
        {
            SetActionFeedbackText(actionId == CombatHudActionId.None ? string.Empty : actionId.ToString());
        }

        public void SetActionFeedbackText(string feedback)
        {
            SetText(actionFeedbackText, feedback);
        }

        private ActionSlotBinding FindActionSlot(CombatHudActionId actionId)
        {
            for (int i = 0; i < actionSlots.Length; i++)
            {
                if (actionSlots[i] != null && actionSlots[i].ActionId == actionId)
                {
                    return actionSlots[i];
                }
            }

            return null;
        }

        private SummonSlotBinding FindSummonSlot(CombatHudActionId actionId)
        {
            for (int i = 0; i < summonSlots.Length; i++)
            {
                if (summonSlots[i] != null && summonSlots[i].ActionId == actionId)
                {
                    return summonSlots[i];
                }
            }

            return null;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
