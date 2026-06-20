using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIThemeTextStylePresenter : MonoBehaviour
    {
        [SerializeField] private UIThemeCatalog catalog;
        [SerializeField] private Text targetText;
        [SerializeField] private string textStyleKey;
        [SerializeField] private bool applyOnEnable = true;

        private void Reset()
        {
            targetText = GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (applyOnEnable)
            {
                Apply();
            }
        }

        public void SetTextStyleKey(string key)
        {
            textStyleKey = key;
            Apply();
        }

        public void Apply()
        {
            if (targetText == null || catalog == null || string.IsNullOrWhiteSpace(textStyleKey))
            {
                return;
            }

            if (!catalog.TryGetTextStyle(textStyleKey, out UIThemeCatalog.TextStyle textStyle))
            {
                return;
            }

            targetText.fontSize = textStyle.FontSize;
            targetText.fontStyle = textStyle.FontStyle;
            targetText.color = textStyle.Color;
        }
    }
}
