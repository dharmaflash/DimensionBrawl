using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UITransitionPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text progressText;
        [SerializeField] private Image progressFill;
        [SerializeField] private UILoadingCardDeck loadingCardDeck;
        [SerializeField] private UILoadingCardPresenter loadingCardPresenter;
        [SerializeField] private Graphic[] loadingDetailGraphics;
        [SerializeField, Min(0f)] private float defaultFadeSeconds = 0.45f;

        private bool showsLoadingDetails;

        private void Reset()
        {
            fadeGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            HideImmediate();
        }

        public IEnumerator PlayOut(UIScreenRouteTable.Route route)
        {
            PrepareLoadingDetails(route);
            SetProgress(0f, route.HasLoadingCard ? "Preparing" : string.Empty);
            yield return FadeTo(1f, defaultFadeSeconds);
        }

        public IEnumerator PlayIn()
        {
            yield return FadeTo(0f, defaultFadeSeconds);
            showsLoadingDetails = false;
            SetLoadingDetailsVisible(false);
        }

        public void HideImmediate()
        {
            showsLoadingDetails = false;
            SetProgress(0f, string.Empty);
            SetLoadingDetailsVisible(false);
            SetAlpha(0f);
        }

        public void SetProgress(float normalizedProgress, string label)
        {
            if (!showsLoadingDetails)
            {
                SetProgressText(string.Empty);
                SetProgressFill(0f);
                return;
            }

            float clamped = Mathf.Clamp01(normalizedProgress);
            SetProgressFill(clamped);
            string progressLabel = string.IsNullOrWhiteSpace(label) ? "Loading" : label;
            SetProgressText(clamped > 0f ? $"{progressLabel} {Mathf.RoundToInt(clamped * 100f)}%" : progressLabel);
        }

        private void PrepareLoadingDetails(UIScreenRouteTable.Route route)
        {
            if (!route.HasLoadingCard)
            {
                showsLoadingDetails = false;
                ClearLoadingText();
                SetLoadingDetailsVisible(false);
                return;
            }

            bool applied = false;
            if (loadingCardPresenter != null)
            {
                applied = loadingCardPresenter.TryShowCard(route.LoadingCardId);
            }
            else if (loadingCardDeck != null && loadingCardDeck.TryGetCard(route.LoadingCardId, out UILoadingCardDeck.LoadingCard card))
            {
                SetText(titleText, card.Title);
                SetText(descriptionText, card.Description);
                applied = true;
            }

            showsLoadingDetails = applied;
            SetLoadingDetailsVisible(applied);
            if (!applied)
            {
                ClearLoadingText();
                Debug.LogWarning($"UI loading card '{route.LoadingCardId}' was not found for route {route.RouteId}.", this);
            }
        }

        private IEnumerator FadeTo(float targetAlpha, float seconds)
        {
            if (fadeGroup == null)
            {
                yield break;
            }

            fadeGroup.blocksRaycasts = true;
            float startAlpha = fadeGroup.alpha;
            if (seconds <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
            {
                SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / seconds));
                yield return null;
            }

            SetAlpha(targetAlpha);
        }

        private void SetAlpha(float alpha)
        {
            if (fadeGroup == null)
            {
                return;
            }

            fadeGroup.alpha = alpha;
            fadeGroup.interactable = alpha > 0.95f;
            fadeGroup.blocksRaycasts = alpha > 0.01f;
        }

        private void SetLoadingDetailsVisible(bool visible)
        {
            if (loadingDetailGraphics == null)
            {
                return;
            }

            for (int i = 0; i < loadingDetailGraphics.Length; i++)
            {
                if (loadingDetailGraphics[i] != null)
                {
                    loadingDetailGraphics[i].enabled = visible;
                }
            }
        }

        private void ClearLoadingText()
        {
            SetText(titleText, string.Empty);
            SetText(descriptionText, string.Empty);
            SetProgressText(string.Empty);
            SetProgressFill(0f);
        }

        private void SetProgressText(string value)
        {
            SetText(progressText, value);
        }

        private void SetProgressFill(float value)
        {
            if (progressFill != null)
            {
                progressFill.fillAmount = value;
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
