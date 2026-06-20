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
                SetText(screenIdText, routeId == UIRouteId.None ? string.Empty : routeId.ToString());
                SetText(soundContextText, string.Empty);
                SetText(cachePolicyText, string.Empty);
                SetAccent(fallbackAccentColor);
                return;
            }

            SetText(screenIdText, screen.ScreenId);
            SetText(cachePolicyText, screen.CachePolicy.ToString());

            if (soundContextCatalog != null
                && soundContextCatalog.TryGetContext(screen.BgmContextId, out UISoundContextCatalog.SoundContext context))
            {
                SetText(soundContextText, $"{context.Id} | {context.Loop} | {context.FadeCrossSeconds:0.00}");
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
