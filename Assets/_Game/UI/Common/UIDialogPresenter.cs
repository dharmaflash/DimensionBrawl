using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIDialogPresenter : MonoBehaviour
    {
        [SerializeField] private UIDialogCatalog catalog;
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private UIIconPresenter iconPresenter;
        [SerializeField] private Image accentImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text confirmText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text cancelText;
        [SerializeField] private string defaultDialogId;
        [SerializeField] private bool visibleOnAwake;
        [SerializeField] private UnityEvent<string> confirmed = new UnityEvent<string>();
        [SerializeField] private UnityEvent<string> canceled = new UnityEvent<string>();

        private string currentDialogId;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            SetVisible(visibleOnAwake);
        }

        private void OnEnable()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(Confirm);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(Cancel);
            }
        }

        private void OnDisable()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(Confirm);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(Cancel);
            }
        }

        public void ShowDefault()
        {
            ShowDialog(defaultDialogId);
        }

        public void ShowDialog(string dialogId)
        {
            if (catalog == null || !catalog.TryGetDialog(dialogId, out UIDialogCatalog.DialogEntry entry))
            {
                return;
            }

            currentDialogId = dialogId;
            SetText(titleText, entry.TitleTextKey);
            SetText(bodyText, entry.BodyTextKey);
            SetText(confirmText, entry.ConfirmTextKey);
            SetText(cancelText, entry.CancelTextKey);

            if (iconPresenter != null)
            {
                iconPresenter.SetIcon(entry.IconKey);
            }

            if (accentImage != null)
            {
                accentImage.color = entry.AccentColor;
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(entry.CancelVisible);
            }

            SetVisible(true);
        }

        public void Confirm()
        {
            confirmed.Invoke(currentDialogId);
            Hide();
        }

        public void Cancel()
        {
            canceled.Invoke(currentDialogId);
            Hide();
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
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

        private void SetText(Text target, string key)
        {
            if (target == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            target.text = textCatalog != null && textCatalog.TryGetText(key, out string value) ? value : key;
        }
    }
}
