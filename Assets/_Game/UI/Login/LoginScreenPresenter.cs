using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class LoginScreenPresenter : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text promptText;
        [SerializeField] private Text versionText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button startButton;
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

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
