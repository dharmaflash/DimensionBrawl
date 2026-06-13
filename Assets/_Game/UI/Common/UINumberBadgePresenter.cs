using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UINumberBadgePresenter : MonoBehaviour
    {
        [SerializeField] private Text valueText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(1)] private int maxVisibleValue = 99;
        [SerializeField] private bool hideWhenZero = true;
        [SerializeField] private string cappedSuffix = "+";

        private int value;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            valueText = GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            Apply();
        }

        public void SetValue(int newValue)
        {
            value = Mathf.Max(0, newValue);
            Apply();
        }

        private void Apply()
        {
            if (valueText != null)
            {
                valueText.text = value > maxVisibleValue ? $"{maxVisibleValue}{cappedSuffix}" : value.ToString();
            }

            bool visible = !hideWhenZero || value > 0;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
