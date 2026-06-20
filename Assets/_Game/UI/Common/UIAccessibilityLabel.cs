using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIAccessibilityLabel : MonoBehaviour
    {
        [SerializeField] private UIAccessibilityCatalog catalog;
        [SerializeField] private string accessibilityKey;
        [SerializeField] private Text readableLabelPreview;
        [SerializeField] private Text fontSizeTarget;
        [SerializeField] private bool enforceMinimumFontSize;

        public string AccessibilityKey => accessibilityKey;

        private void OnEnable()
        {
            Apply();
        }

        public void Configure(string key)
        {
            accessibilityKey = key;
            Apply();
        }

        private void Apply()
        {
            if (catalog == null || !catalog.TryGetEntry(accessibilityKey, out UIAccessibilityCatalog.AccessibilityEntry entry))
            {
                return;
            }

            if (readableLabelPreview != null)
            {
                readableLabelPreview.text = entry.ReadableLabelKey;
            }

            if (enforceMinimumFontSize && fontSizeTarget != null)
            {
                fontSizeTarget.fontSize = Mathf.Max(fontSizeTarget.fontSize, entry.MinimumFontSize);
            }
        }
    }
}
