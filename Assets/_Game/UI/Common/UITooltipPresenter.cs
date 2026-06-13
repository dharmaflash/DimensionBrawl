using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UITooltipPresenter : MonoBehaviour
    {
        [SerializeField] private UITooltipCatalog catalog;
        [SerializeField] private UITextCatalog textCatalog;
        [SerializeField] private string tooltipId;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private UIIconPresenter iconPresenter;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private bool visibleOnEnable;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            SetVisible(visibleOnEnable);
            Apply();
        }

        public void ShowTooltip(string id)
        {
            tooltipId = id;
            Apply();
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void Apply()
        {
            if (catalog == null || !catalog.TryGetTooltip(tooltipId, out UITooltipCatalog.TooltipEntry tooltip))
            {
                return;
            }

            SetText(titleText, tooltip.TitleTextKey);
            SetText(bodyText, tooltip.BodyTextKey);

            if (iconPresenter != null)
            {
                iconPresenter.SetIcon(tooltip.IconKey);
            }
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
