using UnityEngine;
using UnityEngine.UI;

namespace IsekaiBrawl.Gameplay
{
    internal static class RuntimeCanvasLayoutUtility
    {
        private static readonly Vector2 DefaultReferenceResolution = new(720f, 1280f);

        public static float ResolveScale(RectTransform context)
        {
            if (context == null)
            {
                return 1f;
            }

            Vector2 referenceResolution = ResolveReferenceResolution(context);
            Vector2 canvasSize = ResolveCanvasSize(context, referenceResolution);

            float widthScale = canvasSize.x / Mathf.Max(1f, referenceResolution.x);
            float heightScale = canvasSize.y / Mathf.Max(1f, referenceResolution.y);

            CanvasScaler scaler = context.GetComponentInParent<CanvasScaler>();
            float match = scaler != null ? scaler.matchWidthOrHeight : 0.5f;
            float resolved = Mathf.Lerp(widthScale, heightScale, match);
            if (!float.IsFinite(resolved) || resolved <= 0.01f)
            {
                return 1f;
            }

            return resolved;
        }

        public static float ResolveSoftScale(RectTransform context, float influence = 0.42f, float maxScale = 1.56f)
        {
            float raw = ResolveScale(context);
            float softened = Mathf.Lerp(1f, raw, Mathf.Clamp01(influence));
            if (!float.IsFinite(softened) || softened <= 0.01f)
            {
                return 1f;
            }

            return Mathf.Min(maxScale, softened);
        }

        private static Vector2 ResolveReferenceResolution(RectTransform context)
        {
            CanvasScaler scaler = context.GetComponentInParent<CanvasScaler>();
            if (scaler != null &&
                scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                scaler.referenceResolution.x > 0f &&
                scaler.referenceResolution.y > 0f)
            {
                return scaler.referenceResolution;
            }

            return DefaultReferenceResolution;
        }

        private static Vector2 ResolveCanvasSize(RectTransform context, Vector2 fallback)
        {
            Canvas canvas = context.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;

            float width = canvasRect != null && canvasRect.rect.width > 1f
                ? canvasRect.rect.width
                : context.rect.width > 1f ? context.rect.width : (Screen.width > 0 ? Screen.width : fallback.x);

            float height = canvasRect != null && canvasRect.rect.height > 1f
                ? canvasRect.rect.height
                : context.rect.height > 1f ? context.rect.height : (Screen.height > 0 ? Screen.height : fallback.y);

            return new Vector2(width, height);
        }
    }
}
