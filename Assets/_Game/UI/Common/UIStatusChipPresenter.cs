using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIStatusChipPresenter : MonoBehaviour
    {
        [SerializeField] private Text labelText;
        [SerializeField] private Image swatchImage;
        [SerializeField] private string label;
        [SerializeField] private Color swatchColor = Color.white;

        private void OnEnable()
        {
            Apply();
        }

        public void SetState(string newLabel, Color newColor)
        {
            label = newLabel;
            swatchColor = newColor;
            Apply();
        }

        private void Apply()
        {
            if (labelText != null)
            {
                labelText.text = label;
            }

            if (swatchImage != null)
            {
                swatchImage.color = swatchColor;
            }
        }
    }
}
