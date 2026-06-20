using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIResultPreviewOutcome
    {
        Win = 0,
        Fail = 10,
        Retry = 20
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Result Preview Catalog")]
    public sealed class UIResultPreviewCatalog : ScriptableObject
    {
        [Serializable]
        public struct ResultPreviewEntry
        {
            [SerializeField] private string id;
            [SerializeField] private UIResultPreviewOutcome outcome;
            [SerializeField] private string titleTextKey;
            [SerializeField] private string summaryTextKey;
            [SerializeField] private string primaryActionTextKey;
            [SerializeField] private string secondaryActionTextKey;
            [SerializeField] private string cueId;
            [SerializeField] private Color accentColor;

            public string Id => id;
            public UIResultPreviewOutcome Outcome => outcome;
            public string TitleTextKey => titleTextKey;
            public string SummaryTextKey => summaryTextKey;
            public string PrimaryActionTextKey => primaryActionTextKey;
            public string SecondaryActionTextKey => secondaryActionTextKey;
            public string CueId => cueId;
            public Color AccentColor => accentColor;
        }

        [SerializeField] private ResultPreviewEntry[] results = Array.Empty<ResultPreviewEntry>();

        public bool TryGetResult(string id, out ResultPreviewEntry entry)
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (string.Equals(results[i].Id, id, StringComparison.Ordinal))
                {
                    entry = results[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
