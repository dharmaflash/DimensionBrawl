using System;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudMockFlowPresenter : MonoBehaviour
    {
        [Serializable]
        private struct HudSnapshot
        {
            [SerializeField] private string objectiveTextKey;
            [SerializeField] private string inputModeTextKey;
            [SerializeField] private string actionFeedbackTextKey;
            [SerializeField] private string stateTextKey;
            [SerializeField, Min(0f)] private float timerSeconds;
            [SerializeField, Min(0f)] private float healthCurrent;
            [SerializeField, Min(0f)] private float healthMax;
            [SerializeField, Min(0f)] private float resourceCurrent;
            [SerializeField, Min(0f)] private float resourceMax;

            public string ObjectiveTextKey => objectiveTextKey;
            public string InputModeTextKey => inputModeTextKey;
            public string ActionFeedbackTextKey => actionFeedbackTextKey;
            public string StateTextKey => stateTextKey;
            public float TimerSeconds => timerSeconds;
            public float HealthCurrent => healthCurrent;
            public float HealthMax => healthMax;
            public float ResourceCurrent => resourceCurrent;
            public float ResourceMax => resourceMax;
        }

        [Serializable]
        private struct SkillCooldownBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField, Range(0f, 1f)] private float normalizedRemaining;
            [SerializeField] private string labelTextKey;

            public CombatHudActionId ActionId => actionId;
            public float NormalizedRemaining => normalizedRemaining;
            public string LabelTextKey => labelTextKey;
        }

        [Serializable]
        private struct SummonSlotStateBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private string labelTextKey;
            [SerializeField] private string stateTextKey;
            [SerializeField] private bool enabled;

            public CombatHudActionId ActionId => actionId;
            public string LabelTextKey => labelTextKey;
            public string StateTextKey => stateTextKey;
            public bool Enabled => enabled;
        }

        [SerializeField] private CombatHudPresenter hudPresenter;
        [SerializeField] private UIResultPreviewPresenter resultPreviewPresenter;
        [SerializeField] private UIToastPresenter toastPresenter;
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private Text stateText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button winButton;
        [SerializeField] private Button failButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private HudSnapshot readySnapshot;
        [SerializeField] private HudSnapshot runningSnapshot;
        [SerializeField] private HudSnapshot winSnapshot;
        [SerializeField] private HudSnapshot failSnapshot;
        [SerializeField] private SkillCooldownBinding[] skillCooldowns = Array.Empty<SkillCooldownBinding>();
        [SerializeField] private SummonSlotStateBinding[] summonSlotStates = Array.Empty<SummonSlotStateBinding>();
        [SerializeField] private string winResultId;
        [SerializeField] private string failResultId;
        [SerializeField] private string startToastId;
        [SerializeField] private string winToastId;
        [SerializeField] private string failToastId;
        [SerializeField] private string resetToastId;

        private void OnEnable()
        {
            AddListeners();
        }

        private void Start()
        {
            ApplyReadyState(false);
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        public void StartMockCombat()
        {
            resultPreviewPresenter?.Hide();
            ApplySnapshot(runningSnapshot);
            ApplyStaticHudBindings();
            ShowToast(startToastId);
        }

        public void ShowMockWin()
        {
            ApplySnapshot(winSnapshot);
            resultPreviewPresenter?.ShowResult(winResultId);
            ShowToast(winToastId);
        }

        public void ShowMockFail()
        {
            ApplySnapshot(failSnapshot);
            resultPreviewPresenter?.ShowResult(failResultId);
            ShowToast(failToastId);
        }

        public void ResetMockCombat()
        {
            ApplyReadyState(true);
        }

        private void ApplyReadyState(bool showToast)
        {
            resultPreviewPresenter?.Hide();
            ApplySnapshot(readySnapshot);
            ApplyStaticHudBindings();

            if (showToast)
            {
                ShowToast(resetToastId);
            }
        }

        private void ApplySnapshot(HudSnapshot snapshot)
        {
            if (hudPresenter != null)
            {
                hudPresenter.SetObjective(ResolveText(snapshot.ObjectiveTextKey));
                hudPresenter.SetTimer(snapshot.TimerSeconds);
                hudPresenter.SetHealth(snapshot.HealthCurrent, snapshot.HealthMax);
                hudPresenter.SetResource(snapshot.ResourceCurrent, snapshot.ResourceMax);
                hudPresenter.SetInputMode(ResolveText(snapshot.InputModeTextKey));
                hudPresenter.SetActionFeedbackText(ResolveText(snapshot.ActionFeedbackTextKey));
            }

            SetText(stateText, ResolveText(snapshot.StateTextKey));
        }

        private void ApplyStaticHudBindings()
        {
            if (hudPresenter == null)
            {
                return;
            }

            for (int i = 0; i < skillCooldowns.Length; i++)
            {
                SkillCooldownBinding binding = skillCooldowns[i];
                hudPresenter.SetSkillCooldown(
                    binding.ActionId,
                    binding.NormalizedRemaining,
                    ResolveText(binding.LabelTextKey));
            }

            for (int i = 0; i < summonSlotStates.Length; i++)
            {
                SummonSlotStateBinding binding = summonSlotStates[i];
                hudPresenter.SetSummonSlotState(
                    binding.ActionId,
                    ResolveText(binding.LabelTextKey),
                    ResolveText(binding.StateTextKey),
                    binding.Enabled);
            }
        }

        private void AddListeners()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartMockCombat);
            }

            if (winButton != null)
            {
                winButton.onClick.AddListener(ShowMockWin);
            }

            if (failButton != null)
            {
                failButton.onClick.AddListener(ShowMockFail);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetMockCombat);
            }
        }

        private void RemoveListeners()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartMockCombat);
            }

            if (winButton != null)
            {
                winButton.onClick.RemoveListener(ShowMockWin);
            }

            if (failButton != null)
            {
                failButton.onClick.RemoveListener(ShowMockFail);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(ResetMockCombat);
            }
        }

        private void ShowToast(string toastId)
        {
            if (toastPresenter != null)
            {
                toastPresenter.ShowToast(toastId);
            }
        }

        private string ResolveText(string key)
        {
            if (textCatalog != null &&
                !string.IsNullOrWhiteSpace(key) &&
                textCatalog.TryGetText(key, out string value))
            {
                return value;
            }

            return string.Empty;
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
