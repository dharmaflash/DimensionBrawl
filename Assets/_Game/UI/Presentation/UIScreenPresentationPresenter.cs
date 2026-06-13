using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIScreenPresentationPresenter : MonoBehaviour
    {
        [SerializeField] private UIScreenCatalog screenCatalog;
        [SerializeField] private UISoundContextCatalog soundContextCatalog;
        [SerializeField] private UIRouteId routeId = UIRouteId.None;
        [SerializeField] private Text screenIdText;
        [SerializeField] private Text soundContextText;
        [SerializeField] private Text cachePolicyText;
        [SerializeField] private Image accentImage;
        [SerializeField] private Color fallbackAccentColor = Color.white;
        [SerializeField] private bool applyOnEnable = true;

        private void OnEnable()
        {
            if (applyOnEnable)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if (screenCatalog == null || !screenCatalog.TryGetScreen(routeId, out UIScreenCatalog.ScreenEntry screen))
            {
                SetText(screenIdText, routeId == UIRouteId.None ? "Screen" : routeId.ToString());
                SetText(soundContextText, "Sound context unavailable");
                SetText(cachePolicyText, "Cache unknown");
                SetAccent(fallbackAccentColor);
                return;
            }

            SetText(screenIdText, screen.ScreenId);
            SetText(cachePolicyText, $"Cache {screen.CachePolicy}");

            if (soundContextCatalog != null
                && soundContextCatalog.TryGetContext(screen.BgmContextId, out UISoundContextCatalog.SoundContext context))
            {
                string loopLabel = context.Loop ? "Loop" : "One-shot";
                SetText(soundContextText, $"{context.Id} | {loopLabel} | Fade {context.FadeCrossSeconds:0.00}s");
            }
            else
            {
                SetText(soundContextText, screen.BgmContextId);
            }

            SetAccent(fallbackAccentColor);
        }

        private void SetAccent(Color color)
        {
            if (accentImage != null)
            {
                accentImage.color = color;
            }
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
