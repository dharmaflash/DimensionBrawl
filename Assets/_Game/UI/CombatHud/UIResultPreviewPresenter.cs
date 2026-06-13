using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIResultPreviewPresenter : MonoBehaviour
    {
        [SerializeField] private UIResultPreviewCatalog catalog;
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image accentImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Text primaryActionText;
        [SerializeField] private Text secondaryActionText;
        [SerializeField] private bool visibleOnAwake;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            SetVisible(visibleOnAwake);
        }

        public void ShowResult(string resultId)
        {
            if (catalog == null || !catalog.TryGetResult(resultId, out UIResultPreviewCatalog.ResultPreviewEntry entry))
            {
                return;
            }

            SetText(titleText, entry.TitleTextKey);
            SetText(summaryText, entry.SummaryTextKey);
            SetText(primaryActionText, entry.PrimaryActionTextKey);
            SetText(secondaryActionText, entry.SecondaryActionTextKey);

            if (accentImage != null)
            {
                accentImage.color = entry.AccentColor;
            }

            SetVisible(true);
        }

        public void ShowWin()
        {
            ShowResult("MockWin");
        }

        public void ShowFail()
        {
            ShowResult("MockFail");
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
