using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class ProxyCombatHudOverlayPresenter : MonoBehaviour
    {
        [Header("Runtime Canvas Overlay")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform overlayRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image fullMaskImage;
        [SerializeField] private Image topMaskImage;
        [SerializeField] private Image bottomMaskImage;
        [SerializeField] private Image leftMaskImage;
        [SerializeField] private Image rightMaskImage;
        [SerializeField] private RectTransform focusFrame;
        [SerializeField] private RectTransform pulseFrame;
        [SerializeField] private RectTransform fallbackPulseFrame;
        [SerializeField] private RectTransform cornerFrame;
        [SerializeField] private Image sweepImage;
        [SerializeField] private RectTransform promptChip;
        [SerializeField] private Text promptTextComponent;
        [SerializeField] private RectTransform guideBox;
        [SerializeField] private Image guideBoxImage;
        [SerializeField] private Image guideAccentBarImage;
        [SerializeField] private Image guideAccentLineImage;
        [SerializeField] private Text guideTitleTextComponent;
        [SerializeField] private Text guideTextComponent;

        [Header("Visual Tuning")]
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

        private const string OverlayRootName = "ProxyCombatHudCanvasOverlay";
        private const string FullMaskName = "MaskFull";
        private const string TopMaskName = "MaskTop";
        private const string BottomMaskName = "MaskBottom";
        private const string LeftMaskName = "MaskLeft";
        private const string RightMaskName = "MaskRight";
        private const string FocusFrameName = "FocusFrame";
        private const string PulseFrameName = "PulseFrame";
        private const string FallbackPulseFrameName = "FallbackPulseFrame";
        private const string CornerFrameName = "CornerFrame";
        private const string SweepName = "FocusSweep";
        private const string PromptChipName = "PromptChip";
        private const string PromptTextName = "PromptText";
        private const string GuideBoxName = "GuideBox";
        private const string GuideAccentBarName = "GuideAccentBar";
        private const string GuideAccentLineName = "GuideAccentLine";
        private const string GuideTitleName = "GuideTitle";
        private const string GuideBodyName = "GuideBody";

        private readonly List<RectTransform> activeTargets = new List<RectTransform>();
        private readonly Vector3[] targetCorners = new Vector3[4];
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
        public bool HasCanvasOverlay => overlayRoot != null && canvasGroup != null;
        public bool RuntimeMaskActive =>
            IsActive(fullMaskImage)
            || IsActive(topMaskImage)
            || IsActive(bottomMaskImage)
            || IsActive(leftMaskImage)
            || IsActive(rightMaskImage);
        public bool RuntimeFocusFrameActive => focusFrame != null && focusFrame.gameObject.activeSelf;
        public bool RuntimeFallbackPulseActive => fallbackPulseFrame != null && fallbackPulseFrame.gameObject.activeSelf;
        public bool RuntimeGuideBoxActive => guideBox != null && guideBox.gameObject.activeSelf;
        public string RuntimeGuideBodyText => guideTextComponent != null ? guideTextComponent.text : string.Empty;

        private void Reset()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            EnsureCanvasOverlay();
            ApplyVisibilityState();
        }

        private void LateUpdate()
        {
            if (visible)
            {
                ApplyCanvasOverlay();
            }
        }

        public void EnsureCanvasOverlay()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (canvas == null)
            {
                canvas = gameObject.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                }

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 100);

                CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = gameObject.AddComponent<CanvasScaler>();
                }

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform canvasTransform = canvas.transform;
            overlayRoot = EnsureRectTransform(canvasTransform, OverlayRootName);
            overlayRoot.gameObject.layer = gameObject.layer;
            overlayRoot.SetAsLastSibling();
            SetFullStretch(overlayRoot);

            canvasGroup = overlayRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = overlayRoot.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            fullMaskImage = EnsureImage(overlayRoot, FullMaskName, maskColor);
            topMaskImage = EnsureImage(overlayRoot, TopMaskName, maskColor);
            bottomMaskImage = EnsureImage(overlayRoot, BottomMaskName, maskColor);
            leftMaskImage = EnsureImage(overlayRoot, LeftMaskName, maskColor);
            rightMaskImage = EnsureImage(overlayRoot, RightMaskName, maskColor);

            focusFrame = EnsureRectTransform(overlayRoot, FocusFrameName);
            pulseFrame = EnsureRectTransform(overlayRoot, PulseFrameName);
            fallbackPulseFrame = EnsureRectTransform(overlayRoot, FallbackPulseFrameName);
            cornerFrame = EnsureRectTransform(overlayRoot, CornerFrameName);
            EnsureFrameBars(focusFrame);
            EnsureFrameBars(pulseFrame);
            EnsureFrameBars(fallbackPulseFrame);
            EnsureCornerBars(cornerFrame);

            sweepImage = EnsureImage(overlayRoot, SweepName, Color.clear);
            promptChip = EnsureRectTransform(overlayRoot, PromptChipName);
            Image promptImage = EnsureImageComponent(promptChip, Color.clear);
            promptImage.type = Image.Type.Sliced;
            promptTextComponent = EnsureText(promptChip, PromptTextName, 16, FontStyle.Bold, TextAnchor.MiddleCenter);

            guideBox = EnsureRectTransform(overlayRoot, GuideBoxName);
            guideBoxImage = EnsureImageComponent(guideBox, guideBoxColor);
            guideBoxImage.type = Image.Type.Sliced;
            guideAccentBarImage = EnsureImage(guideBox, GuideAccentBarName, spotlightColor);
            guideAccentLineImage = EnsureImage(guideBox, GuideAccentLineName, spotlightColor);
            guideTitleTextComponent = EnsureText(guideBox, GuideTitleName, 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            guideTextComponent = EnsureText(guideBox, GuideBodyName, 22, FontStyle.Normal, TextAnchor.MiddleCenter);

            SetFullStretch(fullMaskImage.rectTransform);
            focusFrame.gameObject.SetActive(false);
            pulseFrame.gameObject.SetActive(false);
            fallbackPulseFrame.gameObject.SetActive(false);
            cornerFrame.gameObject.SetActive(false);
            sweepImage.gameObject.SetActive(false);
            promptChip.gameObject.SetActive(false);
            guideBox.gameObject.SetActive(false);
            ApplyVisibilityState();
        }

        public void Show(
            PgrCombatHudProxyMapping mapping,
            IReadOnlyList<RectTransform> targets,
            string guideText,
            bool textOnlyFallback)
        {
            EnsureCanvasOverlay();

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

            if (promptTextComponent != null)
            {
                promptTextComponent.text = string.IsNullOrWhiteSpace(lastPromptLabel) ? "FOCUS" : lastPromptLabel;
            }

            if (guideTitleTextComponent != null)
            {
                guideTitleTextComponent.text = ResolveGuideTitle();
            }

            if (guideTextComponent != null)
            {
                guideTextComponent.text = string.IsNullOrWhiteSpace(guideText) ? lastMappingId : guideText;
            }

            ApplyVisibilityState();
            ApplyCanvasOverlay();
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
            ApplyVisibilityState();
        }

        private void ApplyVisibilityState()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (overlayRoot != null)
            {
                overlayRoot.gameObject.SetActive(visible);
            }
        }

        private void ApplyCanvasOverlay()
        {
            if (!HasCanvasOverlay)
            {
                EnsureCanvasOverlay();
            }

            Rect rootRect = ResolveOverlayRect();
            float elapsed = Time.unscaledTime - visibleStartTime;
            float pulse01 = 0.5f + Mathf.Sin(elapsed * pulseSpeed) * 0.5f;
            bool hasUnion = TryResolveTargetUnion(out Rect targetRect);
            Rect paddedTargetRect = hasUnion ? PadRect(targetRect, spotlightPadding) : CreateFallbackTargetRect(rootRect);
            Color accent = lastAccentColor.a > 0f ? lastAccentColor : spotlightColor;

            ApplyMask(rootRect, hasUnion, paddedTargetRect);

            if (hasUnion)
            {
                SetFrame(focusFrame, paddedTargetRect, WithAlpha(accent, 0.92f), outlineThickness);
                SetFrame(
                    pulseFrame,
                    PadRect(paddedTargetRect, pulsePadding * (0.35f + pulse01)),
                    WithAlpha(pulseColor, 0.14f + pulse01 * 0.18f),
                    outlineThickness);
                SetCornerFrame(cornerFrame, paddedTargetRect, WithAlpha(accent, 0.98f), pulse01);
                SetSweep(paddedTargetRect, accent, pulse01);
                focusFrame.gameObject.SetActive(true);
                pulseFrame.gameObject.SetActive(true);
                cornerFrame.gameObject.SetActive(true);
                sweepImage.gameObject.SetActive(true);
                fallbackPulseFrame.gameObject.SetActive(false);
            }
            else
            {
                SetFrame(
                    fallbackPulseFrame,
                    PadRect(paddedTargetRect, pulse01 * 12f),
                    WithAlpha(accent, 0.26f + pulse01 * 0.12f),
                    outlineThickness);
                focusFrame.gameObject.SetActive(false);
                pulseFrame.gameObject.SetActive(false);
                cornerFrame.gameObject.SetActive(false);
                sweepImage.gameObject.SetActive(false);
                fallbackPulseFrame.gameObject.SetActive(true);
            }

            ApplyPromptChip(ResolvePromptRect(paddedTargetRect, rootRect), accent);
            ApplyGuideBox(ResolveGuideRect(paddedTargetRect, rootRect), accent);
        }

        private bool TryResolveTargetUnion(out Rect unionRect)
        {
            unionRect = default;
            bool hasUnion = false;
            for (int i = 0; i < activeTargets.Count; i++)
            {
                if (!TryGetCanvasRect(activeTargets[i], out Rect rect))
                {
                    continue;
                }

                unionRect = hasUnion ? Union(unionRect, rect) : rect;
                hasUnion = true;
            }

            return hasUnion;
        }

        private bool TryGetCanvasRect(RectTransform target, out Rect rect)
        {
            rect = default;
            if (target == null || overlayRoot == null)
            {
                return false;
            }

            target.GetWorldCorners(targetCorners);
            bool hasPoint = false;
            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;
            for (int i = 0; i < targetCorners.Length; i++)
            {
                Vector2 localPoint = overlayRoot.InverseTransformPoint(targetCorners[i]);
                if (!hasPoint)
                {
                    min = localPoint;
                    max = localPoint;
                    hasPoint = true;
                }
                else
                {
                    min = Vector2.Min(min, localPoint);
                    max = Vector2.Max(max, localPoint);
                }
            }

            rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return rect.width > 0f && rect.height > 0f;
        }

        private Rect ResolveOverlayRect()
        {
            Rect rect = overlayRoot != null ? overlayRoot.rect : default;
            if (rect.width > 1f && rect.height > 1f)
            {
                return rect;
            }

            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);
            return new Rect(width * -0.5f, height * -0.5f, width, height);
        }

        private void ApplyMask(Rect rootRect, bool hasSpotlight, Rect spotlightRect)
        {
            SetImageColor(fullMaskImage, maskColor);
            SetImageColor(topMaskImage, maskColor);
            SetImageColor(bottomMaskImage, maskColor);
            SetImageColor(leftMaskImage, maskColor);
            SetImageColor(rightMaskImage, maskColor);

            if (!hasSpotlight)
            {
                ApplyLocalRect(fullMaskImage.rectTransform, rootRect);
                SetActive(fullMaskImage, true);
                SetActive(topMaskImage, false);
                SetActive(bottomMaskImage, false);
                SetActive(leftMaskImage, false);
                SetActive(rightMaskImage, false);
                return;
            }

            Rect hole = ClampRect(spotlightRect, rootRect);
            SetActive(fullMaskImage, false);
            ApplyMaskPanel(topMaskImage, Rect.MinMaxRect(rootRect.xMin, hole.yMax, rootRect.xMax, rootRect.yMax));
            ApplyMaskPanel(bottomMaskImage, Rect.MinMaxRect(rootRect.xMin, rootRect.yMin, rootRect.xMax, hole.yMin));
            ApplyMaskPanel(leftMaskImage, Rect.MinMaxRect(rootRect.xMin, hole.yMin, hole.xMin, hole.yMax));
            ApplyMaskPanel(rightMaskImage, Rect.MinMaxRect(hole.xMax, hole.yMin, rootRect.xMax, hole.yMax));
        }

        private static void ApplyMaskPanel(Image image, Rect rect)
        {
            bool active = rect.width > 0.5f && rect.height > 0.5f;
            SetActive(image, active);
            if (active)
            {
                ApplyLocalRect(image.rectTransform, rect);
            }
        }

        private void ApplyPromptChip(Rect rect, Color accent)
        {
            ApplyLocalRect(promptChip, rect);
            promptChip.gameObject.SetActive(true);
            Image promptImage = promptChip.GetComponent<Image>();
            if (promptImage != null)
            {
                promptImage.color = WithAlpha(accent, 0.82f);
                promptImage.raycastTarget = false;
            }

            if (promptTextComponent != null)
            {
                SetFullStretch(promptTextComponent.rectTransform, new Vector2(10f, 2f));
                promptTextComponent.color = promptTextColor;
                promptTextComponent.text = string.IsNullOrWhiteSpace(lastPromptLabel) ? "FOCUS" : lastPromptLabel;
            }
        }

        private void ApplyGuideBox(Rect rect, Color accent)
        {
            ApplyLocalRect(guideBox, rect);
            guideBox.gameObject.SetActive(true);
            SetImageColor(guideBoxImage, guideBoxColor);
            SetImageColor(guideAccentBarImage, WithAlpha(accent, 0.95f));
            SetImageColor(guideAccentLineImage, WithAlpha(accent, 0.38f));

            ApplyLocalRect(guideAccentBarImage.rectTransform, new Rect(rect.width * -0.5f, rect.height * -0.5f, 5f, rect.height));
            ApplyLocalRect(guideAccentLineImage.rectTransform, new Rect(rect.width * -0.5f + 5f, rect.height * 0.5f - 2f, rect.width - 5f, 2f));

            if (guideTitleTextComponent != null)
            {
                ApplyLocalRect(guideTitleTextComponent.rectTransform, new Rect(rect.width * -0.5f + 18f, rect.height * 0.5f - 28f, rect.width - 36f, 24f));
                guideTitleTextComponent.color = WithAlpha(accent, 0.98f);
                guideTitleTextComponent.text = ResolveGuideTitle();
            }

            if (guideTextComponent != null)
            {
                ApplyLocalRect(guideTextComponent.rectTransform, new Rect(rect.width * -0.5f + 18f, rect.height * -0.5f + 10f, rect.width - 36f, rect.height - 42f));
                guideTextComponent.color = guideTextColor;
                guideTextComponent.text = string.IsNullOrWhiteSpace(lastGuideText) ? lastMappingId : lastGuideText;
            }
        }

        private static Rect CreateFallbackTargetRect(Rect rootRect)
        {
            Vector2 center = new Vector2(rootRect.center.x, Mathf.Lerp(rootRect.yMin, rootRect.yMax, 0.55f));
            return new Rect(center.x - 52f, center.y - 52f, 104f, 104f);
        }

        private Rect ResolvePromptRect(Rect targetRect, Rect rootRect)
        {
            const float Height = 25f;
            string prompt = string.IsNullOrWhiteSpace(lastPromptLabel) ? "FOCUS" : lastPromptLabel;
            float width = Mathf.Clamp(prompt.Length * 10.5f + 92f, 96f, 220f);
            float x = Mathf.Clamp(targetRect.center.x - width * 0.5f, rootRect.xMin + 18f, rootRect.xMax - width - 18f);
            float y = targetRect.yMax + 9f;
            if (y + Height > rootRect.yMax - 18f)
            {
                y = targetRect.yMin - Height - 9f;
            }

            return new Rect(x, Mathf.Clamp(y, rootRect.yMin + 18f, rootRect.yMax - Height - 18f), width, Height);
        }

        private static Rect ResolveGuideRect(Rect targetRect, Rect rootRect)
        {
            float width = Mathf.Min(rootRect.width * 0.62f, 520f);
            float height = 106f;
            float x = Mathf.Clamp(targetRect.center.x - width * 0.5f, rootRect.xMin + 24f, rootRect.xMax - width - 24f);
            float y = targetRect.yMax + 22f;
            if (y + height > rootRect.yMax - 24f)
            {
                y = targetRect.yMin - height - 22f;
            }

            return new Rect(x, Mathf.Clamp(y, rootRect.yMin + 24f, rootRect.yMax - height - 24f), width, height);
        }

        private void SetFrame(RectTransform frame, Rect rect, Color color, float thickness)
        {
            ApplyLocalRect(frame, rect);
            SetFrameBar(frame, "Top", new Rect(rect.width * -0.5f, rect.height * 0.5f - thickness, rect.width, thickness), color);
            SetFrameBar(frame, "Bottom", new Rect(rect.width * -0.5f, rect.height * -0.5f, rect.width, thickness), color);
            SetFrameBar(frame, "Left", new Rect(rect.width * -0.5f, rect.height * -0.5f, thickness, rect.height), color);
            SetFrameBar(frame, "Right", new Rect(rect.width * 0.5f - thickness, rect.height * -0.5f, thickness, rect.height), color);
        }

        private void SetCornerFrame(RectTransform frame, Rect rect, Color color, float pulse01)
        {
            ApplyLocalRect(frame, rect);
            frame.gameObject.SetActive(true);

            float length = Mathf.Min(rect.width, rect.height) * 0.28f + pulse01 * 4f;
            float thickness = outlineThickness + 1f;
            float left = rect.width * -0.5f;
            float right = rect.width * 0.5f;
            float bottom = rect.height * -0.5f;
            float top = rect.height * 0.5f;

            SetFrameBar(frame, "TopLeftH", new Rect(left, top - thickness, length, thickness), color);
            SetFrameBar(frame, "TopLeftV", new Rect(left, top - length, thickness, length), color);
            SetFrameBar(frame, "TopRightH", new Rect(right - length, top - thickness, length, thickness), color);
            SetFrameBar(frame, "TopRightV", new Rect(right - thickness, top - length, thickness, length), color);
            SetFrameBar(frame, "BottomLeftH", new Rect(left, bottom, length, thickness), color);
            SetFrameBar(frame, "BottomLeftV", new Rect(left, bottom, thickness, length), color);
            SetFrameBar(frame, "BottomRightH", new Rect(right - length, bottom, length, thickness), color);
            SetFrameBar(frame, "BottomRightV", new Rect(right - thickness, bottom, thickness, length), color);
        }

        private void SetSweep(Rect targetRect, Color accent, float pulse01)
        {
            float sweepWidth = Mathf.Clamp(targetRect.width * 0.18f, 10f, 34f);
            float x = Mathf.Lerp(targetRect.xMin - sweepWidth, targetRect.xMax, pulse01);
            ApplyLocalRect(sweepImage.rectTransform, new Rect(x, targetRect.yMin, sweepWidth, targetRect.height));
            sweepImage.color = WithAlpha(accent, 0.1f);
            sweepImage.raycastTarget = false;
        }

        private static void SetFrameBar(RectTransform frame, string childName, Rect rect, Color color)
        {
            Transform child = frame.Find(childName);
            if (child == null)
            {
                return;
            }

            RectTransform rectTransform = child as RectTransform;
            Image image = child.GetComponent<Image>();
            if (rectTransform == null || image == null)
            {
                return;
            }

            ApplyLocalRect(rectTransform, rect);
            image.color = color;
            image.raycastTarget = false;
            child.gameObject.SetActive(rect.width > 0.5f && rect.height > 0.5f);
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

        private static Rect ClampRect(Rect rect, Rect bounds)
        {
            float xMin = Mathf.Clamp(rect.xMin, bounds.xMin, bounds.xMax);
            float xMax = Mathf.Clamp(rect.xMax, bounds.xMin, bounds.xMax);
            float yMin = Mathf.Clamp(rect.yMin, bounds.yMin, bounds.yMax);
            float yMax = Mathf.Clamp(rect.yMax, bounds.yMin, bounds.yMax);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static RectTransform EnsureRectTransform(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                gameObject.transform.SetParent(parent, worldPositionStays: false);
            }

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            return rectTransform;
        }

        private static Image EnsureImage(RectTransform parent, string name, Color color)
        {
            RectTransform rectTransform = EnsureRectTransform(parent, name);
            return EnsureImageComponent(rectTransform, color);
        }

        private static Image EnsureImageComponent(RectTransform rectTransform, Color color)
        {
            Image image = rectTransform.GetComponent<Image>();
            if (image == null)
            {
                image = rectTransform.gameObject.AddComponent<Image>();
            }

            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text EnsureText(RectTransform parent, string name, int fontSize, FontStyle fontStyle, TextAnchor alignment)
        {
            RectTransform rectTransform = EnsureRectTransform(parent, name);
            Text text = rectTransform.GetComponent<Text>();
            if (text == null)
            {
                text = rectTransform.gameObject.AddComponent<Text>();
            }

            Font font = ResolveRuntimeFont();
            if (font != null)
            {
                text.font = font;
            }

            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static Font ResolveRuntimeFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void EnsureFrameBars(RectTransform frame)
        {
            EnsureImage(frame, "Top", Color.clear);
            EnsureImage(frame, "Bottom", Color.clear);
            EnsureImage(frame, "Left", Color.clear);
            EnsureImage(frame, "Right", Color.clear);
        }

        private static void EnsureCornerBars(RectTransform frame)
        {
            EnsureImage(frame, "TopLeftH", Color.clear);
            EnsureImage(frame, "TopLeftV", Color.clear);
            EnsureImage(frame, "TopRightH", Color.clear);
            EnsureImage(frame, "TopRightV", Color.clear);
            EnsureImage(frame, "BottomLeftH", Color.clear);
            EnsureImage(frame, "BottomLeftV", Color.clear);
            EnsureImage(frame, "BottomRightH", Color.clear);
            EnsureImage(frame, "BottomRightV", Color.clear);
        }

        private static void SetFullStretch(RectTransform rectTransform)
        {
            SetFullStretch(rectTransform, Vector2.zero);
        }

        private static void SetFullStretch(RectTransform rectTransform, Vector2 padding)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(-padding.x * 2f, -padding.y * 2f);
        }

        private static void ApplyLocalRect(RectTransform rectTransform, Rect rect)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = rect.center;
            rectTransform.sizeDelta = rect.size;
        }

        private static void SetImageColor(Image image, Color color)
        {
            if (image != null)
            {
                image.color = color;
                image.raycastTarget = false;
            }
        }

        private static void SetActive(Graphic graphic, bool active)
        {
            if (graphic != null)
            {
                graphic.gameObject.SetActive(active);
            }
        }

        private static bool IsActive(Graphic graphic)
        {
            return graphic != null && graphic.gameObject.activeInHierarchy;
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
