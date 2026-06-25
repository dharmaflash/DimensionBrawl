using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class IntroGatePodTimelineFadeOverlay : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float alpha;
        [SerializeField] private CanvasGroup canvasGroup;

        public float Alpha
        {
            get => Mathf.Clamp01(alpha);
            set
            {
                alpha = Mathf.Clamp01(value);
                ApplyAlpha();
            }
        }

        public bool HasCanvasGroup => canvasGroup != null;

        public void Configure(CanvasGroup newCanvasGroup)
        {
            canvasGroup = newCanvasGroup;
            ApplyAlpha();
        }

        private void Awake()
        {
            ApplyAlpha();
        }

        private void OnValidate()
        {
            ApplyAlpha();
        }

        private void ApplyAlpha()
        {
            if (canvasGroup == null)
            {
                return;
            }

            float resolvedAlpha = Alpha;
            canvasGroup.alpha = resolvedAlpha;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
