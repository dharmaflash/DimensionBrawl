using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIIconPresenter : MonoBehaviour
    {
        [SerializeField] private UIIconCatalog catalog;
        [SerializeField] private string iconKey;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text fallbackText;

        private void Reset()
        {
            iconImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            Apply();
        }

        public void SetIcon(string key)
        {
            iconKey = key;
            Apply();
        }

        private void Apply()
        {
            if (catalog == null || !catalog.TryGetIcon(iconKey, out UIIconCatalog.IconEntry icon))
            {
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = icon.Sprite;
                iconImage.color = icon.Sprite == null ? icon.FallbackColor : Color.white;
            }

            if (fallbackText != null)
            {
                fallbackText.text = icon.TextFallback;
            }
        }
    }
}
