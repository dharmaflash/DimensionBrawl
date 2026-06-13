using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIPanelRoot : MonoBehaviour
    {
        [SerializeField] private string panelId;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool visibleOnAwake;

        public string PanelId => panelId;
        public bool IsVisible => canvasGroup == null ? gameObject.activeSelf : canvasGroup.alpha > 0.5f;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            SetVisible(visibleOnAwake);
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
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
    }
}
