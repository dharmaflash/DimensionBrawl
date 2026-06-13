using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LoginScreenPresenter : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text promptText;
        [SerializeField] private Text versionText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button startButton;
        [SerializeField] private bool startOnScreenTap = true;
        [SerializeField] private UISceneFlowRouter router;
        [SerializeField] private UIRouteId startRoute = UIRouteId.Lobby;
        [SerializeField] private string title = "Dimension Brawl";
        [SerializeField] private string prompt = "Start";
        [SerializeField] private string version = "UI V1 Test";
        [SerializeField] private string status = "Local UI shell";
        [SerializeField] private UnityEvent startRequested = new UnityEvent();

        private void OnEnable()
        {
            Apply();

            if (startButton != null)
            {
                startButton.onClick.AddListener(HandleStartClicked);
            }
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartClicked);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!startOnScreenTap || !CanRequestStart())
            {
                return;
            }

            if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            HandleStartClicked();
        }

        public void Apply()
        {
            SetText(titleText, title);
            SetText(promptText, prompt);
            SetText(versionText, version);
            SetText(statusText, status);
        }

        public void HandleStartClicked()
        {
            startRequested.Invoke();

            if (router != null)
            {
                router.RequestRoute(startRoute);
            }
        }

        private bool CanRequestStart()
        {
            return startButton == null || startButton.IsInteractable();
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
