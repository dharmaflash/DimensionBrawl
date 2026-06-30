using System;
using System.IO;
using DimensionBrawl.LevelDesign;
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
            [SerializeField] private StageDefinitionProfile stageDefinition;
            [SerializeField] private string loadingCardId;

            public string Id => id;
            public string DisplayName => displayName;
            public string Summary => summary;
            public string ThreatTags => threatTags;
            public string RecommendedSummonRole => recommendedSummonRole;
            public string MockRewardPreview => mockRewardPreview;
            public StageDefinitionProfile StageDefinition => stageDefinition;
            public string ScenePath => stageDefinition != null ? stageDefinition.MapScenePath : string.Empty;
            public string SceneName => ResolveSceneName(ScenePath);
            public bool HasSceneRoute => !string.IsNullOrWhiteSpace(ScenePath);
            public string LoadingCardId => loadingCardId;
        }

        [SerializeField] private StageEntry[] stages = Array.Empty<StageEntry>();

        public int StageCount => stages != null ? stages.Length : 0;

        public StageEntry GetStage(int index)
        {
            if (stages == null || index < 0 || index >= stages.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return stages[index];
        }

        public bool TryGetStage(string id, out StageEntry stage)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                stage = default;
                return false;
            }

            for (int i = 0; i < stages.Length; i++)
            {
                if (string.Equals(stages[i].Id, id, StringComparison.Ordinal))
                {
                    stage = stages[i];
                    return true;
                }
            }

            stage = default;
            return false;
        }

        public bool TryGetFirstStage(out StageEntry stage)
        {
            if (stages != null && stages.Length > 0)
            {
                stage = stages[0];
                return true;
            }

            stage = default;
            return false;
        }

        private static string ResolveSceneName(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return string.Empty;
            }

            return Path.GetFileNameWithoutExtension(scenePath.Replace('\\', '/'));
        }
    }
}
