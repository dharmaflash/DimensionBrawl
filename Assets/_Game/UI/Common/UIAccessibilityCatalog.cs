using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIContrastClass
    {
        Decorative = 0,
        Informational = 10,
        Essential = 20,
        Critical = 30
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Accessibility Catalog")]
    public sealed class UIAccessibilityCatalog : ScriptableObject
    {
        [Serializable]
        public struct AccessibilityEntry
        {
            [SerializeField] private string key;
            [SerializeField] private string readableLabelKey;
            [SerializeField] private string narrationKey;
            [SerializeField] private int focusOrder;
            [SerializeField, Min(1)] private int minimumFontSize;
            [SerializeField] private UIContrastClass contrastClass;
            [SerializeField] private bool requiresInputPrompt;

            public string Key => key;
            public string ReadableLabelKey => readableLabelKey;
            public string NarrationKey => narrationKey;
            public int FocusOrder => focusOrder;
            public int MinimumFontSize => minimumFontSize;
            public UIContrastClass ContrastClass => contrastClass;
            public bool RequiresInputPrompt => requiresInputPrompt;
        }

        [SerializeField] private AccessibilityEntry[] entries = Array.Empty<AccessibilityEntry>();

        public bool TryGetEntry(string key, out AccessibilityEntry entry)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (string.Equals(entries[i].Key, key, StringComparison.Ordinal))
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
