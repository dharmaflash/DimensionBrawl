using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Theme Catalog")]
    public sealed class UIThemeCatalog : ScriptableObject
    {
        [Serializable]
        public struct ThemeColor
        {
            [SerializeField] private string key;
            [SerializeField] private Color color;

            public string Key => key;
            public Color Color => color;
        }

        [Serializable]
        public struct TextStyle
        {
            [SerializeField] private string key;
            [SerializeField, Min(1)] private int fontSize;
            [SerializeField] private FontStyle fontStyle;
            [SerializeField] private Color color;

            public string Key => key;
            public int FontSize => fontSize;
            public FontStyle FontStyle => fontStyle;
            public Color Color => color;
        }

        [Serializable]
        public struct SpacingToken
        {
            [SerializeField] private string key;
            [SerializeField, Min(0f)] private float value;

            public string Key => key;
            public float Value => value;
        }

        [SerializeField] private ThemeColor[] colors = Array.Empty<ThemeColor>();
        [SerializeField] private TextStyle[] textStyles = Array.Empty<TextStyle>();
        [SerializeField] private SpacingToken[] spacing = Array.Empty<SpacingToken>();

        public bool TryGetColor(string key, out Color color)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (string.Equals(colors[i].Key, key, StringComparison.Ordinal))
                {
                    color = colors[i].Color;
                    return true;
                }
            }

            color = Color.white;
            return false;
        }

        public bool TryGetTextStyle(string key, out TextStyle textStyle)
        {
            for (int i = 0; i < textStyles.Length; i++)
            {
                if (string.Equals(textStyles[i].Key, key, StringComparison.Ordinal))
                {
                    textStyle = textStyles[i];
                    return true;
                }
            }

            textStyle = default;
            return false;
        }

        public bool TryGetSpacing(string key, out float value)
        {
            for (int i = 0; i < spacing.Length; i++)
            {
                if (string.Equals(spacing[i].Key, key, StringComparison.Ordinal))
                {
                    value = spacing[i].Value;
                    return true;
                }
            }

            value = 0f;
            return false;
        }
    }
}
