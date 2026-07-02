using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UILoadingCardPresenter : MonoBehaviour
    {
        [SerializeField] private UILoadingCardDeck deck;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private bool hideBackgroundWhenMissing = true;
        [SerializeField] private Text idText;
        [SerializeField] private Text weightText;
        [SerializeField] private string defaultCardId;
        [SerializeField] private bool useWeightedFallback;
        [SerializeField] private int weightedSeed;
        [SerializeField] private bool applyOnEnable = true;

        public bool LastShownCardHasBackground { get; private set; }

        private void OnEnable()
        {
            if (applyOnEnable)
            {
                TryShowCard(defaultCardId);
            }
        }

        public void ShowCard(string cardId)
        {
            TryShowCard(cardId);
        }

        public bool TryShowCard(string cardId)
        {
            if (deck == null)
            {
                LastShownCardHasBackground = false;
                return false;
            }

            if (deck.TryGetCard(cardId, out UILoadingCardDeck.LoadingCard card)
                || (string.IsNullOrWhiteSpace(cardId) && useWeightedFallback && deck.TryGetWeightedCard(weightedSeed, out card)))
            {
                Apply(card);
                return true;
            }

            LastShownCardHasBackground = false;
            return false;
        }

        public void ShowWeightedPreview()
        {
            ShowWeighted(weightedSeed);
        }

        public void ShowWeighted(int seed)
        {
            if (deck != null && deck.TryGetWeightedCard(seed, out UILoadingCardDeck.LoadingCard card))
            {
                Apply(card);
            }
        }

        private void Apply(UILoadingCardDeck.LoadingCard card)
        {
            LastShownCardHasBackground = card.BackgroundSprite != null;
            SetText(titleText, card.Title);
            SetText(descriptionText, card.Description);
            SetBackground(card.BackgroundSprite);
            SetText(idText, card.Id);
            SetText(weightText, card.Weight.ToString());
        }

        private void SetBackground(Sprite sprite)
        {
            if (backgroundImage == null)
            {
                return;
            }

            backgroundImage.sprite = sprite;
            if (sprite != null)
            {
                backgroundImage.color = Color.white;
                backgroundImage.enabled = true;
                return;
            }

            if (hideBackgroundWhenMissing)
            {
                backgroundImage.enabled = false;
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
