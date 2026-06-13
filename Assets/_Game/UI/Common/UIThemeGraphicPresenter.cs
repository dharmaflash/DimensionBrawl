using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIThemeGraphicPresenter : MonoBehaviour
    {
        [SerializeField] private UIThemeCatalog catalog;
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private string colorKey;
        [SerializeField] private bool applyOnEnable = true;

        private void Reset()
        {
            targetGraphic = GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            if (applyOnEnable)
            {
                Apply();
            }
        }

        public void SetColorKey(string key)
        {
            colorKey = key;
            Apply();
        }

        public void Apply()
        {
            if (targetGraphic == null || catalog == null || string.IsNullOrWhiteSpace(colorKey))
            {
                return;
            }

            if (catalog.TryGetColor(colorKey, out Color color))
            {
                targetGraphic.color = color;
            }
        }
    }
}
