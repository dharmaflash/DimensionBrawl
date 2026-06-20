using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Loading Card Deck")]
    public sealed class UILoadingCardDeck : ScriptableObject
    {
        [Serializable]
        public struct LoadingCard
        {
            [SerializeField] private string id;
            [SerializeField] private string title;
            [SerializeField, TextArea] private string description;
            [SerializeField, Min(0)] private int weight;

            public string Id => id;
            public string Title => title;
            public string Description => description;
            public int Weight => weight;
        }

        [SerializeField] private LoadingCard[] cards = Array.Empty<LoadingCard>();

        public bool TryGetCard(string id, out LoadingCard card)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                card = default;
                return false;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                if (string.Equals(cards[i].Id, id, StringComparison.Ordinal))
                {
                    card = cards[i];
                    return true;
                }
            }

            card = default;
            return false;
        }

        public bool TryGetWeightedCard(int seed, out LoadingCard card)
        {
            int totalWeight = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                totalWeight += Mathf.Max(0, cards[i].Weight);
            }

            if (totalWeight <= 0)
            {
                if (cards.Length > 0)
                {
                    card = cards[0];
                    return true;
                }

                card = default;
                return false;
            }

            int positiveSeed = seed == int.MinValue ? int.MaxValue : Mathf.Abs(seed);
            int roll = positiveSeed % totalWeight;
            int cursor = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                cursor += Mathf.Max(0, cards[i].Weight);
                if (roll < cursor)
                {
                    card = cards[i];
                    return true;
                }
            }

            card = cards[cards.Length - 1];
            return true;
        }
    }
}
