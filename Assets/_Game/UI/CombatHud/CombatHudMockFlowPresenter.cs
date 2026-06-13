using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudMockFlowPresenter : MonoBehaviour
    {
        [SerializeField] private CombatHudPresenter hudPresenter;
        [SerializeField] private UIResultPreviewPresenter resultPreviewPresenter;
        [SerializeField] private UIToastPresenter toastPresenter;
        [SerializeField] private Text stateText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button winButton;
        [SerializeField] private Button failButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private string winResultId = "MockWin";
        [SerializeField] private string failResultId = "MockFail";
        [SerializeField] private string startToastId = "Combat.MockStart";
        [SerializeField] private string winToastId = "Combat.MockWin";
        [SerializeField] private string failToastId = "Combat.MockFail";
        [SerializeField] private string resetToastId = "Combat.MockReset";

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
            hudPresenter?.SetObjective("Mock encounter running");
            hudPresenter?.SetTimer(180f);
            hudPresenter?.SetHealth(840f, 1000f);
            hudPresenter?.SetResource(56f, 100f);
            hudPresenter?.SetInputMode("Direct-control HUD mock");
            hudPresenter?.SetActionFeedbackText("Start Combat mock");
            SetText(stateText, "Mock combat running");
            ShowToast(startToastId);
        }

        public void ShowMockWin()
        {
            hudPresenter?.SetObjective("Mock clear state");
            hudPresenter?.SetTimer(0f);
            hudPresenter?.SetHealth(840f, 1000f);
            hudPresenter?.SetResource(100f, 100f);
            hudPresenter?.SetActionFeedbackText("Mock result: clear");
            SetText(stateText, "Mock clear preview");
            resultPreviewPresenter?.ShowResult(winResultId);
            ShowToast(winToastId);
        }

        public void ShowMockFail()
        {
            hudPresenter?.SetObjective("Mock fail state");
            hudPresenter?.SetTimer(0f);
            hudPresenter?.SetHealth(0f, 1000f);
            hudPresenter?.SetResource(18f, 100f);
            hudPresenter?.SetActionFeedbackText("Mock result: fail");
            SetText(stateText, "Mock fail preview");
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
            hudPresenter?.SetObjective("Reach the encounter marker");
            hudPresenter?.SetTimer(180f);
            hudPresenter?.SetHealth(840f, 1000f);
            hudPresenter?.SetResource(56f, 100f);
            hudPresenter?.SetInputMode("Mobile HUD placeholder");
            hudPresenter?.SetActionFeedbackText("HUD ready");
            SetText(stateText, "HUD mock ready");

            if (showToast)
            {
                ShowToast(resetToastId);
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

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
