using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class IntroGatePodLetterboxOverlay : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float amount;
        [SerializeField, Min(0f)] private float barHeight = 39.333332f;
        [SerializeField, Range(0f, 1f)] private float alpha = 1f;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform bottomBar;

        public bool HasBindings => canvasGroup != null && topBar != null && bottomBar != null;

        public void Configure(CanvasGroup newCanvasGroup, RectTransform newTopBar, RectTransform newBottomBar, float newBarHeight)
        {
            canvasGroup = newCanvasGroup;
            topBar = newTopBar;
            bottomBar = newBottomBar;
            barHeight = Mathf.Max(0f, newBarHeight);
            Apply(amount, alpha, barHeight);
        }

        public void Apply(float normalizedAmount, float normalizedAlpha, float targetBarHeight)
        {
            amount = Mathf.Clamp01(normalizedAmount);
            alpha = Mathf.Clamp01(normalizedAlpha);
            barHeight = Mathf.Max(0f, targetBarHeight);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            float resolvedHeight = barHeight * amount;
            ApplyBarHeight(topBar, resolvedHeight);
            ApplyBarHeight(bottomBar, resolvedHeight);
        }

        public void Clear()
        {
            Apply(0f, 0f, barHeight);
        }

        private static void ApplyBarHeight(RectTransform bar, float height)
        {
            if (bar == null)
            {
                return;
            }

            bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void Awake()
        {
            Apply(amount, alpha, barHeight);
        }

        private void OnValidate()
        {
            Apply(amount, alpha, barHeight);
        }
    }
}
