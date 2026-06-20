using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIProgressPresenter : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Text valueText;
        [SerializeField] private string label = "Progress";
        [SerializeField] private bool showPercent = true;

        public void SetProgress(float normalizedValue)
        {
            float clamped = Mathf.Clamp01(normalizedValue);
            if (fillImage != null)
            {
                fillImage.fillAmount = clamped;
            }

            if (valueText != null)
            {
                valueText.text = showPercent ? $"{label} {Mathf.RoundToInt(clamped * 100f)}%" : label;
            }
        }
    }
}
