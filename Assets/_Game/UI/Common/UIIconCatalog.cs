using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIIconCategory
    {
        System = 0,
        Navigation = 10,
        Input = 20,
        CombatAction = 30,
        Status = 40,
        Placeholder = 50
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Icon Catalog")]
    public sealed class UIIconCatalog : ScriptableObject
    {
        [Serializable]
        public struct IconEntry
        {
            [SerializeField] private string key;
            [SerializeField] private UIIconCategory category;
            [SerializeField] private Sprite sprite;
            [SerializeField] private Color fallbackColor;
            [SerializeField] private string textFallback;
            [SerializeField] private string accessibilityKey;

            public string Key => key;
            public UIIconCategory Category => category;
            public Sprite Sprite => sprite;
            public Color FallbackColor => fallbackColor;
            public string TextFallback => textFallback;
            public string AccessibilityKey => accessibilityKey;
        }

        [SerializeField] private IconEntry[] icons = Array.Empty<IconEntry>();

        public bool TryGetIcon(string key, out IconEntry icon)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                if (string.Equals(icons[i].Key, key, StringComparison.Ordinal))
                {
                    icon = icons[i];
                    return true;
                }
            }

            icon = default;
            return false;
        }
    }
}
