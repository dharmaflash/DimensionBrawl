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
        [SerializeField] private Text idText;
        [SerializeField] private Text weightText;
        [SerializeField] private string defaultCardId;
        [SerializeField] private bool useWeightedFallback;
        [SerializeField] private int weightedSeed;
        [SerializeField] private bool applyOnEnable = true;

        private void OnEnable()
        {
            if (applyOnEnable)
            {
                ShowCard(defaultCardId);
            }
        }

        public void ShowCard(string cardId)
        {
            if (deck == null)
            {
                return;
            }

            if (deck.TryGetCard(cardId, out UILoadingCardDeck.LoadingCard card)
                || (useWeightedFallback && deck.TryGetWeightedCard(weightedSeed, out card)))
            {
                Apply(card);
            }
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
            SetText(titleText, card.Title);
            SetText(descriptionText, card.Description);
            SetText(idText, card.Id);
            SetText(weightText, $"Weight {card.Weight}");
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
