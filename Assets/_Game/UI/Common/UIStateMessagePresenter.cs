using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIStateMessagePresenter : MonoBehaviour
    {
        [SerializeField] private UIStateMessageCatalog catalog;
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private string stateId;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private UIIconPresenter iconPresenter;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button actionButton;
        [SerializeField] private Text actionText;
        [SerializeField] private bool visibleOnEnable = true;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            SetVisible(visibleOnEnable);
            Apply();
        }

        public void SetState(string id)
        {
            stateId = id;
            Apply();
        }

        public void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(visible);
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void Apply()
        {
            if (catalog == null || !catalog.TryGetState(stateId, out UIStateMessageCatalog.StateMessageEntry entry))
            {
                return;
            }

            SetText(titleText, entry.TitleTextKey);
            SetText(bodyText, entry.BodyTextKey);

            if (iconPresenter != null)
            {
                iconPresenter.SetIcon(entry.IconKey);
            }

            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(entry.ActionVisible);
                actionButton.interactable = entry.ActionInteractable;
            }

            SetText(actionText, entry.ActionTextKey);
        }

        private void SetText(Text text, string key)
        {
            if (text == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            text.text = textCatalog != null && textCatalog.TryGetText(key, out string value) ? value : key;
        }
    }
}
