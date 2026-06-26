using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class ProxyCombatHudOverlayPresenter : MonoBehaviour
    {
        [Header("Optional UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform focusFrame;
        [SerializeField] private Text guideTextComponent;

        [Header("Immediate GUI Fallback")]
        [SerializeField] private bool drawImmediateGuiFallback = true;
        [SerializeField] private Color maskColor = new Color(0f, 0f, 0f, 0.58f);
        [SerializeField] private Color spotlightColor = new Color(0.38f, 0.92f, 1f, 0.95f);
        [SerializeField] private Color guideBoxColor = new Color(0.02f, 0.025f, 0.035f, 0.88f);
        [SerializeField] private Color guideTextColor = Color.white;
        [SerializeField, Min(0f)] private float spotlightPadding = 14f;

        private readonly List<RectTransform> activeTargets = new List<RectTransform>();
        private Texture2D whiteTexture;
        private GUIStyle guideStyle;
        private bool visible;
        private string lastMappingId;
        private string lastProxyHudObject;
        private string lastGuideText;
        private string lastFocusPolicy;
        private bool lastTextOnlyFallback;

        public bool Visible => visible;
        public string LastMappingId => lastMappingId;
        public string LastProxyHudObject => lastProxyHudObject;
        public string LastGuideText => lastGuideText;
        public string LastFocusPolicy => lastFocusPolicy;
        public bool LastTextOnlyFallback => lastTextOnlyFallback;
        public int LastTargetCount => activeTargets.Count;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show(
            PgrCombatHudProxyMapping mapping,
            IReadOnlyList<RectTransform> targets,
            string guideText,
            bool textOnlyFallback)
        {
            visible = true;
            lastMappingId = mapping.MappingId;
            lastProxyHudObject = mapping.ProxyHudObject;
            lastGuideText = guideText;
            lastFocusPolicy = mapping.FocusPolicy;
            lastTextOnlyFallback = textOnlyFallback;

            activeTargets.Clear();
            if (targets != null)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] != null)
                    {
                        activeTargets.Add(targets[i]);
                    }
                }
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (guideTextComponent != null)
            {
                guideTextComponent.text = guideText;
            }

            ApplyFocusFrame();
        }

        public void Hide()
        {
            visible = false;
            activeTargets.Clear();
            lastMappingId = string.Empty;
            lastProxyHudObject = string.Empty;
            lastGuideText = string.Empty;
            lastFocusPolicy = string.Empty;
            lastTextOnlyFallback = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (focusFrame != null)
            {
                focusFrame.gameObject.SetActive(false);
            }
        }

        private void OnGUI()
        {
            if (!visible || !drawImmediateGuiFallback || Event.current.type != EventType.Repaint)
            {
                return;
            }

            EnsureGuiResources();
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), maskColor);

            Rect unionRect = default;
            bool hasUnion = false;
            for (int i = 0; i < activeTargets.Count; i++)
            {
                if (!TryGetGuiRect(activeTargets[i], out Rect rect))
                {
                    continue;
                }

                rect = PadRect(rect, spotlightPadding);
                DrawOutline(rect, spotlightColor, 3f);
                unionRect = hasUnion ? Union(unionRect, rect) : rect;
                hasUnion = true;
            }

            Rect anchorRect = hasUnion
                ? unionRect
                : new Rect(Screen.width * 0.5f, Screen.height * 0.55f, 0f, 0f);
            Rect guideRect = ResolveGuideRect(anchorRect);
            DrawRect(guideRect, guideBoxColor);
            GUI.color = guideTextColor;
            GUI.Label(guideRect, string.IsNullOrWhiteSpace(lastGuideText) ? lastMappingId : lastGuideText, guideStyle);
            GUI.color = Color.white;
        }

        private void ApplyFocusFrame()
        {
            if (focusFrame == null)
            {
                return;
            }

            if (activeTargets.Count == 0)
            {
                focusFrame.gameObject.SetActive(false);
                return;
            }

            RectTransform parent = focusFrame.parent as RectTransform;
            if (parent == null)
            {
                focusFrame.gameObject.SetActive(true);
                return;
            }

            bool hasBounds = false;
            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;
            Vector3[] corners = new Vector3[4];
            for (int i = 0; i < activeTargets.Count; i++)
            {
                activeTargets[i].GetWorldCorners(corners);
                for (int j = 0; j < corners.Length; j++)
                {
                    Vector2 localPoint = parent.InverseTransformPoint(corners[j]);
                    if (!hasBounds)
                    {
                        min = localPoint;
                        max = localPoint;
                        hasBounds = true;
                    }
                    else
                    {
                        min = Vector2.Min(min, localPoint);
                        max = Vector2.Max(max, localPoint);
                    }
                }
            }

            if (!hasBounds)
            {
                focusFrame.gameObject.SetActive(false);
                return;
            }

            Vector2 size = max - min + Vector2.one * spotlightPadding * 2f;
            focusFrame.anchorMin = new Vector2(0.5f, 0.5f);
            focusFrame.anchorMax = new Vector2(0.5f, 0.5f);
            focusFrame.pivot = new Vector2(0.5f, 0.5f);
            focusFrame.anchoredPosition = (min + max) * 0.5f;
            focusFrame.sizeDelta = size;
            focusFrame.gameObject.SetActive(true);
        }

        private void EnsureGuiResources()
        {
            whiteTexture ??= Texture2D.whiteTexture;

            guideStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.028f), 18, 30),
                wordWrap = true,
                richText = false,
                padding = new RectOffset(18, 18, 10, 10)
            };
        }

        private static bool TryGetGuiRect(RectTransform target, out Rect rect)
        {
            rect = default;
            if (target == null)
            {
                return false;
            }

            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                minX = Mathf.Min(minX, screenPoint.x);
                minY = Mathf.Min(minY, screenPoint.y);
                maxX = Mathf.Max(maxX, screenPoint.x);
                maxY = Mathf.Max(maxY, screenPoint.y);
            }

            rect = new Rect(minX, Screen.height - maxY, maxX - minX, maxY - minY);
            return rect.width > 0f && rect.height > 0f;
        }

        private static Rect ResolveGuideRect(Rect targetRect)
        {
            float width = Mathf.Min(Screen.width * 0.62f, 520f);
            float height = 92f;
            float x = Mathf.Clamp(targetRect.center.x - width * 0.5f, 24f, Screen.width - width - 24f);
            float y = targetRect.yMin - height - 22f;
            if (y < 24f)
            {
                y = Mathf.Min(Screen.height - height - 24f, targetRect.yMax + 22f);
            }

            return new Rect(x, y, width, height);
        }

        private void DrawOutline(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        private static Rect PadRect(Rect rect, float padding)
        {
            return new Rect(rect.x - padding, rect.y - padding, rect.width + padding * 2f, rect.height + padding * 2f);
        }

        private static Rect Union(Rect a, Rect b)
        {
            float xMin = Mathf.Min(a.xMin, b.xMin);
            float yMin = Mathf.Min(a.yMin, b.yMin);
            float xMax = Mathf.Max(a.xMax, b.xMax);
            float yMax = Mathf.Max(a.yMax, b.yMax);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}
