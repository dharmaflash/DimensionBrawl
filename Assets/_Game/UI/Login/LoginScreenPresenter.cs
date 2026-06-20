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
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private string titleTextKey;
        [SerializeField] private string promptTextKey;
        [SerializeField] private string versionTextKey;
        [SerializeField] private string statusTextKey;
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
            SetCatalogText(titleText, titleTextKey);
            SetCatalogText(promptText, promptTextKey);
            SetCatalogText(versionText, versionTextKey);
            SetCatalogText(statusText, statusTextKey);
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

        private void SetCatalogText(Text target, string key)
        {
            if (target == null)
            {
                return;
            }

            if (textCatalog != null &&
                !string.IsNullOrWhiteSpace(key) &&
                textCatalog.TryGetText(key, out string value))
            {
                target.text = value;
                return;
            }

            target.text = string.Empty;
        }

    }
}
