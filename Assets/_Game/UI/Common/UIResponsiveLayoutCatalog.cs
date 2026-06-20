using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIResponsiveBreakpoint
    {
        Compact = 0,
        Medium = 10,
        Wide = 20
    }

    public enum UISafeAreaMode
    {
        None = 0,
        InsetsOnly = 10,
        FullCanvas = 20
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Responsive Layout Catalog")]
    public sealed class UIResponsiveLayoutCatalog : ScriptableObject
    {
        [Serializable]
        public struct BreakpointEntry
        {
            [SerializeField] private string id;
            [SerializeField] private UIResponsiveBreakpoint breakpoint;
            [SerializeField] private Vector2 minimumResolution;
            [SerializeField] private Vector2 referenceResolution;
            [SerializeField, Range(0f, 1f)] private float matchWidthOrHeight;
            [SerializeField, Min(0f)] private float edgeInset;
            [SerializeField] private UISafeAreaMode safeAreaMode;

            public string Id => id;
            public UIResponsiveBreakpoint Breakpoint => breakpoint;
            public Vector2 MinimumResolution => minimumResolution;
            public Vector2 ReferenceResolution => referenceResolution;
            public float MatchWidthOrHeight => matchWidthOrHeight;
            public float EdgeInset => edgeInset;
            public UISafeAreaMode SafeAreaMode => safeAreaMode;

            public bool Matches(Vector2 resolution)
            {
                return resolution.x >= minimumResolution.x && resolution.y >= minimumResolution.y;
            }
        }

        [SerializeField] private BreakpointEntry[] breakpoints = Array.Empty<BreakpointEntry>();

        public bool TryResolve(Vector2 resolution, out BreakpointEntry entry)
        {
            int bestIndex = -1;
            float bestScore = -1f;
            for (int i = 0; i < breakpoints.Length; i++)
            {
                if (!breakpoints[i].Matches(resolution))
                {
                    continue;
                }

                Vector2 minimum = breakpoints[i].MinimumResolution;
                float score = minimum.x * minimum.y;
                if (score > bestScore)
                {
                    bestIndex = i;
                    bestScore = score;
                }
            }

            if (bestIndex >= 0)
            {
                entry = breakpoints[bestIndex];
                return true;
            }

            if (breakpoints.Length > 0)
            {
                entry = breakpoints[0];
                return true;
            }

            entry = default;
            return false;
        }
    }
}
