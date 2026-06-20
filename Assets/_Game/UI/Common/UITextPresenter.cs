using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UITextPresenter : MonoBehaviour
    {
        [SerializeField] private UITextCatalog catalog;
        [SerializeField] private Text targetText;
        [SerializeField] private string textKey;
        [SerializeField, TextArea] private string fallbackText;
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

        public void SetTextKey(string key)
        {
            textKey = key;
            Apply();
        }

        public void SetLiteral(string value)
        {
            fallbackText = value;
            textKey = string.Empty;
            Apply();
        }

        public void Apply()
        {
            if (targetText == null)
            {
                return;
            }

            if (catalog != null && !string.IsNullOrWhiteSpace(textKey) && catalog.TryGetText(textKey, out string value))
            {
                targetText.text = value;
                return;
            }

            targetText.text = fallbackText;
        }
    }
}
