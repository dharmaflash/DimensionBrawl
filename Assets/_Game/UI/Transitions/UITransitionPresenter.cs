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
        [SerializeField, Min(0f)] private float defaultFadeSeconds = 0.45f;

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
            SetLoadingCard(route.LoadingCardId);
            SetProgress(0f, "Preparing");
            yield return FadeTo(1f, defaultFadeSeconds);
        }

        public IEnumerator PlayIn()
        {
            yield return FadeTo(0f, defaultFadeSeconds);
        }

        public void HideImmediate()
        {
            SetProgress(0f, string.Empty);
            SetAlpha(0f);
        }

        public void SetProgress(float normalizedProgress, string label)
        {
            float clamped = Mathf.Clamp01(normalizedProgress);
            if (progressFill != null)
            {
                progressFill.fillAmount = clamped;
            }

            if (progressText != null)
            {
                string progressLabel = string.IsNullOrWhiteSpace(label) ? "Loading" : label;
                progressText.text = clamped > 0f ? $"{progressLabel} {Mathf.RoundToInt(clamped * 100f)}%" : progressLabel;
            }
        }

        private void SetLoadingCard(string loadingCardId)
        {
            if (loadingCardPresenter != null)
            {
                loadingCardPresenter.ShowCard(loadingCardId);
                return;
            }

            if (loadingCardDeck == null || !loadingCardDeck.TryGetCard(loadingCardId, out UILoadingCardDeck.LoadingCard card))
            {
                SetText(titleText, string.Empty);
                SetText(descriptionText, string.Empty);
                return;
            }

            SetText(titleText, card.Title);
            SetText(descriptionText, card.Description);
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

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
