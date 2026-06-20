using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIButtonStatePresenter : MonoBehaviour
    {
        [SerializeField] private UIButtonStateCatalog catalog;
        [SerializeField] private UIButtonVisualState stateId = UIButtonVisualState.Normal;
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;
        [SerializeField] private string labelOverride;

        private void Reset()
        {
            button = GetComponent<Button>();
            backgroundImage = GetComponent<Image>();
            labelText = GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            Apply();
        }

        public void SetState(UIButtonVisualState state)
        {
            stateId = state;
            Apply();
        }

        private void Apply()
        {
            if (catalog == null || !catalog.TryGetState(stateId, out UIButtonStateCatalog.ButtonStateEntry state))
            {
                return;
            }

            if (button != null)
            {
                button.interactable = state.Interactable;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = state.BackgroundColor;
            }

            if (labelText != null)
            {
                labelText.color = state.TextColor;
                if (!string.IsNullOrEmpty(labelOverride))
                {
                    labelText.text = labelOverride;
                }
            }
        }
    }
}
