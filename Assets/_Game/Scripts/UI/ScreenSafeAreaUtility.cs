using UnityEngine;

namespace DimensionBrawl.UI
{
    public readonly struct ScreenSafeAreaInsets
    {
        public ScreenSafeAreaInsets(float left, float right, float top, float bottom)
        {
            Left = Mathf.Max(0f, left);
            Right = Mathf.Max(0f, right);
            Top = Mathf.Max(0f, top);
            Bottom = Mathf.Max(0f, bottom);
        }

        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public float Bottom { get; }
    }

    public static class ScreenSafeAreaUtility
    {
        public static Rect ResolveGuiSafeArea(Rect rawSafeArea, Vector2 screenSize)
        {
            float width = Mathf.Max(1f, screenSize.x);
            float height = Mathf.Max(1f, screenSize.y);
            Rect safeArea = ClampToScreen(rawSafeArea, width, height);
            return new Rect(
                safeArea.xMin,
                height - safeArea.yMax,
                safeArea.width,
                safeArea.height);
        }

        public static ScreenSafeAreaInsets ResolveCanvasInsets(
            Rect rawSafeArea,
            Vector2 screenSize,
            Vector2 canvasSize)
        {
            float width = Mathf.Max(1f, screenSize.x);
            float height = Mathf.Max(1f, screenSize.y);
            Rect safeArea = ClampToScreen(rawSafeArea, width, height);
            float canvasWidth = Mathf.Max(1f, canvasSize.x);
            float canvasHeight = Mathf.Max(1f, canvasSize.y);
            return new ScreenSafeAreaInsets(
                safeArea.xMin / width * canvasWidth,
                (width - safeArea.xMax) / width * canvasWidth,
                (height - safeArea.yMax) / height * canvasHeight,
                safeArea.yMin / height * canvasHeight);
        }

        public static Vector2 ResolveNormalizedAnchorFromGuiPoint(
            Vector2 guiPoint,
            Rect rawSafeArea,
            Vector2 screenSize)
        {
            Rect safeArea = ResolveGuiSafeArea(rawSafeArea, screenSize);
            return new Vector2(
                Mathf.Clamp01((guiPoint.x - safeArea.xMin) / Mathf.Max(1f, safeArea.width)),
                Mathf.Clamp01(1f
                    - (guiPoint.y - safeArea.yMin) / Mathf.Max(1f, safeArea.height)));
        }

        public static Vector2 ResolveGuiPointFromNormalizedAnchor(
            Vector2 anchor,
            Rect rawSafeArea,
            Vector2 screenSize)
        {
            Rect safeArea = ResolveGuiSafeArea(rawSafeArea, screenSize);
            return new Vector2(
                safeArea.xMin + Mathf.Clamp01(anchor.x) * safeArea.width,
                safeArea.yMin + (1f - Mathf.Clamp01(anchor.y)) * safeArea.height);
        }

        private static Rect ClampToScreen(Rect rawSafeArea, float width, float height)
        {
            if (rawSafeArea.width <= 0f || rawSafeArea.height <= 0f)
            {
                return new Rect(0f, 0f, width, height);
            }

            float xMin = Mathf.Clamp(rawSafeArea.xMin, 0f, width);
            float yMin = Mathf.Clamp(rawSafeArea.yMin, 0f, height);
            float xMax = Mathf.Clamp(rawSafeArea.xMax, xMin, width);
            float yMax = Mathf.Clamp(rawSafeArea.yMax, yMin, height);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}
