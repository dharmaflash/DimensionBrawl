using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Stage Catalog")]
    public sealed class UIStageCatalog : ScriptableObject
    {
        [Serializable]
        public struct StageEntry
        {
            [SerializeField] private string id;
            [SerializeField] private string displayName;
            [SerializeField, TextArea] private string summary;
            [SerializeField] private string threatTags;
            [SerializeField] private string recommendedSummonRole;
            [SerializeField] private string mockRewardPreview;
            [SerializeField] private string loadingCardId;

            public string Id => id;
            public string DisplayName => displayName;
            public string Summary => summary;
            public string ThreatTags => threatTags;
            public string RecommendedSummonRole => recommendedSummonRole;
            public string MockRewardPreview => mockRewardPreview;
            public string LoadingCardId => loadingCardId;
        }

        [SerializeField] private StageEntry[] stages = Array.Empty<StageEntry>();

        public bool TryGetStage(string id, out StageEntry stage)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                for (int i = 0; i < stages.Length; i++)
                {
                    if (string.Equals(stages[i].Id, id, StringComparison.Ordinal))
                    {
                        stage = stages[i];
                        return true;
                    }
                }
            }

            if (stages.Length > 0)
            {
                stage = stages[0];
                return true;
            }

            stage = default;
            return false;
        }
    }
}
