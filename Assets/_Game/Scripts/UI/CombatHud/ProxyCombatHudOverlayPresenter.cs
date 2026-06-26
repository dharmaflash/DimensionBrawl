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
        [SerializeField] private Color pulseColor = new Color(1f, 1f, 1f, 0.42f);
        [SerializeField] private Color guideBoxColor = new Color(0.02f, 0.025f, 0.035f, 0.88f);
        [SerializeField] private Color guideTextColor = Color.white;
        [SerializeField] private Color promptTextColor = Color.white;
        [SerializeField, Min(0f)] private float spotlightPadding = 14f;
        [SerializeField, Min(0f)] private float pulsePadding = 20f;
        [SerializeField, Min(0f)] private float pulseSpeed = 4.8f;
        [SerializeField, Min(1f)] private float outlineThickness = 3f;

        private readonly List<RectTransform> activeTargets = new List<RectTransform>();
        private Texture2D whiteTexture;
        private GUIStyle guideStyle;
        private GUIStyle promptStyle;
        private GUIStyle guideTitleStyle;
        private bool visible;
        private string lastMappingId;
        private string lastProxyHudObject;
        private string lastGuideText;
        private string lastFocusPolicy;
        private string lastCueProfileId;
        private string lastPromptLabel;
        private Color lastAccentColor;
        private bool lastTextOnlyFallback;
        private float visibleStartTime;

        public bool Visible => visible;
        public string LastMappingId => lastMappingId;
        public string LastProxyHudObject => lastProxyHudObject;
        public string LastGuideText => lastGuideText;
        public string LastFocusPolicy => lastFocusPolicy;
        public string LastCueProfileId => lastCueProfileId;
        public string LastPromptLabel => lastPromptLabel;
        public Color LastAccentColor => lastAccentColor;
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
            lastCueProfileId = ResolveCueProfileId(mapping);
            lastPromptLabel = ResolvePromptLabel(mapping);
            lastAccentColor = ResolveAccentColor(mapping);
            lastTextOnlyFallback = textOnlyFallback;
            visibleStartTime = Time.unscaledTime;

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
            lastCueProfileId = string.Empty;
            lastPromptLabel = string.Empty;
            lastAccentColor = default;
            lastTextOnlyFallback = false;
            visibleStartTime = 0f;

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

            float elapsed = Time.unscaledTime - visibleStartTime;
            float pulse01 = 0.5f + Mathf.Sin(elapsed * pulseSpeed) * 0.5f;
            Rect unionRect = default;
            bool hasUnion = false;
            for (int i = 0; i < activeTargets.Count; i++)
            {
                if (!TryGetGuiRect(activeTargets[i], out Rect rect))
                {
                    continue;
                }

                rect = PadRect(rect, spotlightPadding);
                DrawTargetFocus(rect, pulse01);
                DrawPromptChip(rect);
                unionRect = hasUnion ? Union(unionRect, rect) : rect;
                hasUnion = true;
            }

            Rect anchorRect = hasUnion
                ? unionRect
                : new Rect(Screen.width * 0.5f, Screen.height * 0.55f, 0f, 0f);
            Rect guideRect = ResolveGuideRect(anchorRect);
            DrawGuideBox(guideRect);

            if (!hasUnion)
            {
                Rect fallbackPulseRect = new Rect(
                    anchorRect.x - 52f - pulse01 * 12f,
                    anchorRect.y - 52f - pulse01 * 12f,
                    104f + pulse01 * 24f,
                    104f + pulse01 * 24f);
                DrawOutline(fallbackPulseRect, WithAlpha(lastAccentColor, 0.34f), outlineThickness);
            }
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

            promptStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.02f), 13, 20),
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                richText = false,
                padding = new RectOffset(10, 10, 3, 3)
            };

            guideTitleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.016f), 11, 16),
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                richText = false,
                padding = new RectOffset(18, 18, 6, 0)
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
            float height = 106f;
            float x = Mathf.Clamp(targetRect.center.x - width * 0.5f, 24f, Screen.width - width - 24f);
            float y = targetRect.yMin - height - 22f;
            if (y < 24f)
            {
                y = Mathf.Min(Screen.height - height - 24f, targetRect.yMax + 22f);
            }

            return new Rect(x, y, width, height);
        }

        private void DrawTargetFocus(Rect rect, float pulse01)
        {
            Color accent = lastAccentColor.a > 0f ? lastAccentColor : spotlightColor;
            Rect outerPulse = PadRect(rect, pulsePadding * (0.35f + pulse01));
            Rect innerGlow = PadRect(rect, Mathf.Lerp(6f, 14f, pulse01));

            DrawOutline(outerPulse, WithAlpha(accent, 0.16f + pulse01 * 0.16f), outlineThickness);
            DrawOutline(innerGlow, WithAlpha(pulseColor, 0.14f + pulse01 * 0.18f), outlineThickness);
            DrawOutline(rect, WithAlpha(accent, 0.9f), outlineThickness);
            DrawCornerBrackets(rect, accent, pulse01);
            DrawSweep(rect, accent, pulse01);
        }

        private void DrawPromptChip(Rect targetRect)
        {
            string prompt = string.IsNullOrWhiteSpace(lastPromptLabel) ? "FOCUS" : lastPromptLabel;
            float width = Mathf.Clamp(prompt.Length * 10.5f + 28f, 96f, 220f);
            Rect chipRect = new Rect(
                Mathf.Clamp(targetRect.center.x - width * 0.5f, 18f, Screen.width - width - 18f),
                Mathf.Max(18f, targetRect.yMin - 34f),
                width,
                25f);

            DrawRect(chipRect, WithAlpha(lastAccentColor, 0.82f));
            DrawOutline(chipRect, WithAlpha(Color.white, 0.28f), 1f);
            GUI.color = promptTextColor;
            GUI.Label(chipRect, prompt, promptStyle);
            GUI.color = Color.white;
        }

        private void DrawGuideBox(Rect guideRect)
        {
            DrawRect(guideRect, guideBoxColor);
            DrawRect(new Rect(guideRect.xMin, guideRect.yMin, 5f, guideRect.height), WithAlpha(lastAccentColor, 0.95f));
            DrawRect(new Rect(guideRect.xMin + 5f, guideRect.yMin, guideRect.width - 5f, 2f), WithAlpha(lastAccentColor, 0.38f));

            Rect titleRect = new Rect(guideRect.xMin + 6f, guideRect.yMin + 4f, guideRect.width - 12f, 24f);
            Rect bodyRect = new Rect(guideRect.xMin, guideRect.yMin + 28f, guideRect.width, guideRect.height - 28f);
            GUI.color = WithAlpha(lastAccentColor, 0.98f);
            GUI.Label(titleRect, ResolveGuideTitle(), guideTitleStyle);
            GUI.color = guideTextColor;
            GUI.Label(bodyRect, string.IsNullOrWhiteSpace(lastGuideText) ? lastMappingId : lastGuideText, guideStyle);
            GUI.color = Color.white;
        }

        private void DrawCornerBrackets(Rect rect, Color color, float pulse01)
        {
            float length = Mathf.Min(rect.width, rect.height) * 0.28f + pulse01 * 4f;
            float thickness = outlineThickness + 1f;
            Color bracketColor = WithAlpha(color, 0.95f);

            DrawRect(new Rect(rect.xMin, rect.yMin, length, thickness), bracketColor);
            DrawRect(new Rect(rect.xMin, rect.yMin, thickness, length), bracketColor);
            DrawRect(new Rect(rect.xMax - length, rect.yMin, length, thickness), bracketColor);
            DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, length), bracketColor);
            DrawRect(new Rect(rect.xMin, rect.yMax - thickness, length, thickness), bracketColor);
            DrawRect(new Rect(rect.xMin, rect.yMax - length, thickness, length), bracketColor);
            DrawRect(new Rect(rect.xMax - length, rect.yMax - thickness, length, thickness), bracketColor);
            DrawRect(new Rect(rect.xMax - thickness, rect.yMax - length, thickness, length), bracketColor);
        }

        private void DrawSweep(Rect rect, Color color, float pulse01)
        {
            float sweepWidth = Mathf.Clamp(rect.width * 0.18f, 10f, 34f);
            float x = Mathf.Lerp(rect.xMin - sweepWidth, rect.xMax, pulse01);
            Rect sweepRect = new Rect(x, rect.yMin, sweepWidth, rect.height);
            DrawRect(sweepRect, WithAlpha(color, 0.08f));
            DrawRect(new Rect(x + sweepWidth * 0.5f, rect.yMin, 2f, rect.height), WithAlpha(Color.white, 0.22f));
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

        private static string ResolveCueProfileId(PgrCombatHudProxyMapping mapping)
        {
            if (!string.IsNullOrWhiteSpace(mapping.SemanticLabel))
            {
                return mapping.SemanticLabel.Trim();
            }

            return mapping.ProxyInputEvent.Kind.ToString();
        }

        private static string ResolvePromptLabel(PgrCombatHudProxyMapping mapping)
        {
            switch (mapping.ProxyInputEvent.Kind)
            {
                case ProxyCombatHudInputKind.BasicAttackPressed:
                    return "TAP ATTACK";
                case ProxyCombatHudInputKind.SignalOrbPressed:
                    return "PING ORB";
                case ProxyCombatHudInputKind.SignalOrbSequencePressed:
                    return "3-PING";
                case ProxyCombatHudInputKind.DodgePressed:
                    return "DODGE NOW";
                case ProxyCombatHudInputKind.SignatureSkillPressed:
                    return "CAST SKILL";
                case ProxyCombatHudInputKind.SwitchOrQtePressed:
                    return "QTE READY";
                case ProxyCombatHudInputKind.PartnerSkillPressed:
                    return "CALL SUPPORT";
                default:
                    return mapping.ProxyCompletionKind == ProxyCombatHudCompletionKind.DurationOrReadAck
                        ? "READ"
                        : "FOCUS";
            }
        }

        private static Color ResolveAccentColor(PgrCombatHudProxyMapping mapping)
        {
            switch (mapping.ProxyInputEvent.Kind)
            {
                case ProxyCombatHudInputKind.BasicAttackPressed:
                    return new Color(0.34f, 0.92f, 1f, 0.96f);
                case ProxyCombatHudInputKind.SignalOrbPressed:
                case ProxyCombatHudInputKind.SignalOrbSequencePressed:
                    return new Color(0.88f, 0.58f, 1f, 0.96f);
                case ProxyCombatHudInputKind.DodgePressed:
                    return new Color(1f, 0.84f, 0.36f, 0.96f);
                case ProxyCombatHudInputKind.SignatureSkillPressed:
                    return new Color(0.56f, 0.72f, 1f, 0.96f);
                case ProxyCombatHudInputKind.SwitchOrQtePressed:
                    return new Color(1f, 0.54f, 0.86f, 0.96f);
                case ProxyCombatHudInputKind.PartnerSkillPressed:
                    return new Color(0.44f, 1f, 0.72f, 0.96f);
                default:
                    return mapping.ProxyCompletionKind == ProxyCombatHudCompletionKind.DurationOrReadAck
                        ? new Color(1f, 0.62f, 0.38f, 0.96f)
                        : new Color(0.38f, 0.92f, 1f, 0.96f);
            }
        }

        private string ResolveGuideTitle()
        {
            if (lastTextOnlyFallback)
            {
                return "GUIDE TARGET LOST";
            }

            if (!string.IsNullOrWhiteSpace(lastPromptLabel))
            {
                return lastPromptLabel;
            }

            return "COMBAT GUIDE";
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
