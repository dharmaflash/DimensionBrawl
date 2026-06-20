using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UITooltipAnchor
    {
        Auto = 0,
        Top = 10,
        Right = 20,
        Bottom = 30,
        Left = 40
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Tooltip Catalog")]
    public sealed class UITooltipCatalog : ScriptableObject
    {
        [Serializable]
        public struct TooltipEntry
        {
            [SerializeField] private string id;
            [SerializeField] private string titleTextKey;
            [SerializeField] private string bodyTextKey;
            [SerializeField] private string iconKey;
            [SerializeField] private UITooltipAnchor preferredAnchor;
            [SerializeField, Min(0f)] private float holdSeconds;

            public string Id => id;
            public string TitleTextKey => titleTextKey;
            public string BodyTextKey => bodyTextKey;
            public string IconKey => iconKey;
            public UITooltipAnchor PreferredAnchor => preferredAnchor;
            public float HoldSeconds => holdSeconds;
        }

        [SerializeField] private TooltipEntry[] tooltips = Array.Empty<TooltipEntry>();

        public bool TryGetTooltip(string id, out TooltipEntry tooltip)
        {
            for (int i = 0; i < tooltips.Length; i++)
            {
                if (string.Equals(tooltips[i].Id, id, StringComparison.Ordinal))
                {
                    tooltip = tooltips[i];
                    return true;
                }
            }

            tooltip = default;
            return false;
        }
    }
}
