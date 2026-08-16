using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudPresenter : MonoBehaviour, ICombatBossHudStatus
    {
        private static readonly string[] BossHudGraphicNames =
        {
            "BossSymbol",
            "BossNameArea",
            "BossNameText",
            "BossHpBackground",
            "BossHpFill",
            "BossCostBackground",
            "BossCostFill"
        };

        private static readonly Color HealthReadoutColor = new Color(1f, 0.92f, 0.68f, 1f);
        private static readonly Color ResourceReadoutColor = new Color(0.56f, 1f, 1f, 1f);
        private static readonly Color InputModeReadoutColor = new Color(0.9f, 0.98f, 1f, 1f);
        private static readonly Color AmmoReadoutColor = new Color(1f, 0.86f, 0.38f, 1f);
        private static readonly Color TargetIvoryReadoutColor = new Color32(0xF7, 0xF5, 0xEE, 0xFF);
        private static readonly Color TargetReloadReadoutColor = new Color32(0x8D, 0xD4, 0xDF, 0xFF);
        private static readonly Color ReadoutOutlineColor = new Color(0f, 0.025f, 0.035f, 0.95f);
        private static readonly Color SummonChargingFillColor = new Color(0.08f, 0.86f, 1f, 0.94f);
        private static readonly Color SummonReadyIconColor = new Color(1f, 1f, 1f, 0.98f);
        private static readonly Color SummonUnavailableIconColor = new Color(0.26f, 0.28f, 0.31f, 0.96f);
        private static readonly Color SummonReadyGlowColor = new Color(1f, 0.94f, 0.08f, 1f);
        private static readonly Color SummonReadyRingColor = new Color(1f, 1f, 0.18f, 1f);
        private static readonly Color SummonReadySparkColor = new Color(0.1f, 1f, 1f, 1f);
        private const float DimensionHudDesignWidth = 2560f;
        private const float DimensionHudDesignHeight = 1440f;

        private Rect lastAppliedSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private Vector2Int lastAppliedScreenSize = new Vector2Int(-1, -1);
        private ScreenSafeAreaInsets safeAreaInsets;

        private enum ResponsiveHudAnchor
        {
            LeftTop,
            LeftBottom,
            RightTop,
            RightBottom,
            CenterBottom,
            CenterScreen
        }

        [Serializable]
        public sealed class ActionSlotBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private Text labelText;
            [SerializeField] private Text cooldownText;
            [SerializeField] private Image cooldownFill;
            [SerializeField] private Image readyProgressFill;
            [SerializeField] private Image readyGlowImage;
            [SerializeField] private CanvasGroup canvasGroup;
            [NonSerialized] private float readyGlowVisibility;
            [NonSerialized] private int lastCooldownTenths = int.MinValue;
            [NonSerialized] private float lastNormalizedRemaining;
            [NonSerialized] private bool inputAvailable = true;

            public CombatHudActionId ActionId => actionId;

            public void SetCooldown(float normalizedRemaining, string label, float secondsRemaining = -1f)
            {
                if (labelText != null && !string.IsNullOrWhiteSpace(label))
                {
                    SetText(labelText, label);
                }

                float clamped = Mathf.Clamp01(normalizedRemaining);
                lastNormalizedRemaining = clamped;
                if (cooldownFill != null)
                {
                    cooldownFill.fillAmount = clamped;
                }

                ApplyReadyProgress(clamped);
                ApplyReadyGlow(clamped);

                if (cooldownText != null)
                {
                    float displaySeconds = secondsRemaining >= 0f ? secondsRemaining : Mathf.CeilToInt(clamped * 10f) / 10f;
                    int displayTenths = clamped > 0f ? Mathf.Max(0, Mathf.RoundToInt(displaySeconds * 10f)) : -1;
                    if (displayTenths != lastCooldownTenths)
                    {
                        lastCooldownTenths = displayTenths;
                        SetText(
                            cooldownText,
                            displayTenths >= 0 ? $"{displayTenths / 10f:0.0}s" : string.Empty);
                    }

                    bool showCooldown = displayTenths >= 0;
                    if (cooldownText.gameObject.activeSelf != showCooldown)
                    {
                        cooldownText.gameObject.SetActive(showCooldown);
                    }
                }

                ApplyInputAvailability();
            }

            public void SetInputAvailable(bool available)
            {
                if (inputAvailable == available)
                {
                    ApplyInputAvailability();
                    return;
                }

                inputAvailable = available;
                ApplyReadyProgress(lastNormalizedRemaining);
                ApplyReadyGlow(lastNormalizedRemaining);
                ApplyInputAvailability();
            }

            private void ApplyReadyProgress(float normalizedRemaining)
            {
                if (readyProgressFill == null)
                {
                    return;
                }

                float readyProgress = inputAvailable
                    ? Mathf.Clamp01(1f - normalizedRemaining)
                    : 0f;
                readyProgressFill.raycastTarget = false;
                readyProgressFill.type = Image.Type.Filled;
                readyProgressFill.fillMethod = Image.FillMethod.Radial360;
                readyProgressFill.fillOrigin = (int)Image.Origin360.Top;
                readyProgressFill.fillClockwise = true;
                readyProgressFill.fillAmount = readyProgress;

                bool ready = inputAvailable && normalizedRemaining <= 0.001f;
                float easedProgress = Mathf.SmoothStep(0f, 1f, readyProgress);
                float readyPulse = ready ? SmoothPulse(2.4f) : 0f;
                bool highPriorityReady = ready
                    && (actionId == CombatHudActionId.Dodge || actionId == CombatHudActionId.Skill1);
                readyProgressFill.color = new Color(
                    highPriorityReady ? 0.44f : 0.92f,
                    highPriorityReady ? 1f : 0.98f,
                    1f,
                    ready
                        ? (highPriorityReady ? 0.98f : 0.86f + readyPulse * 0.14f)
                        : Mathf.Lerp(0.12f, 0.46f, easedProgress));
                readyProgressFill.gameObject.SetActive(readyProgress > 0.001f);
            }

            private void ApplyReadyGlow(float normalizedRemaining)
            {
                if (readyGlowImage == null)
                {
                    return;
                }

                bool ready = inputAvailable && normalizedRemaining <= 0.001f;
                readyGlowImage.raycastTarget = false;
                readyGlowImage.type = Image.Type.Simple;
                float targetVisibility = ready ? 1f : 0f;
                float fadeSpeed = ready ? 3.6f : 7.5f;
                readyGlowVisibility = Mathf.MoveTowards(
                    readyGlowVisibility,
                    targetVisibility,
                    GetUiDeltaTime() * fadeSpeed);

                if (readyGlowVisibility <= 0.001f && !ready)
                {
                    readyGlowImage.color = Color.clear;
                    readyGlowImage.rectTransform.localScale = Vector3.one;
                    readyGlowImage.gameObject.SetActive(false);
                    return;
                }

                readyGlowImage.gameObject.SetActive(true);
                float pulse = SmoothPulse(actionId == CombatHudActionId.Skill1 ? 1.9f : 2.15f);
                bool highPriorityReady = actionId == CombatHudActionId.Dodge || actionId == CombatHudActionId.Skill1;
                Color color = actionId == CombatHudActionId.Skill1
                    ? new Color(1f, 0.98f, 0.12f, readyGlowVisibility * (0.92f + pulse * 0.08f))
                    : actionId == CombatHudActionId.Dodge
                        ? new Color(0.08f, 1f, 1f, readyGlowVisibility * (0.94f + pulse * 0.06f))
                        : new Color(0.86f, 0.96f, 1f, readyGlowVisibility * (0.46f + pulse * 0.16f));
                readyGlowImage.color = color;
                readyGlowImage.rectTransform.localScale = Vector3.one * (1f
                    + readyGlowVisibility * (highPriorityReady ? 0.26f + pulse * 0.16f : 0.08f + pulse * 0.05f));
            }

            private static float SmoothPulse(float speed)
            {
                float raw = 0.5f + Mathf.Sin(Time.unscaledTime * speed) * 0.5f;
                return Mathf.SmoothStep(0f, 1f, raw);
            }

            private static float GetUiDeltaTime()
            {
                float deltaTime = Time.unscaledDeltaTime;
                return deltaTime > 0f ? deltaTime : 1f / 60f;
            }

            private void ApplyInputAvailability()
            {
                if (canvasGroup == null)
                {
                    return;
                }

                canvasGroup.alpha = inputAvailable
                    ? lastNormalizedRemaining > 0f ? 0.65f : 1f
                    : 0.45f;
                canvasGroup.interactable = inputAvailable;
                canvasGroup.blocksRaycasts = inputAvailable;
            }

        }

        [Serializable]
        public sealed class SummonSlotBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private Text labelText;
            [SerializeField] private Text stateText;
            [SerializeField] private Image cooldownFill;
            [SerializeField] private Image iconImage;
            [SerializeField] private Image unavailableIconImage;
            [SerializeField] private Image readyGlowImage;
            [SerializeField] private Image readyRingImage;
            [SerializeField] private Image readySparkImage;
            [SerializeField] private CanvasGroup canvasGroup;
            [NonSerialized] private bool visibilityInitialized;
            [NonSerialized] private bool isVisible;
            [NonSerialized] private bool staticVisualsApplied;
            [NonSerialized] private bool visualOrderApplied;

            public CombatHudActionId ActionId => actionId;

            public void SetVisible(bool visible)
            {
                ResolveStateVisuals();
                if (visibilityInitialized && isVisible == visible)
                {
                    return;
                }

                visibilityInitialized = true;
                isVisible = visible;
                if (labelText != null)
                {
                    SetGameObjectActive(labelText.gameObject, visible);
                }

                if (stateText != null)
                {
                    SetGameObjectActive(stateText.gameObject, visible);
                }

                if (cooldownFill != null)
                {
                    SetGameObjectActive(cooldownFill.gameObject, visible);
                }

                SetImageObjectVisible(iconImage, visible);
                SetImageObjectVisible(unavailableIconImage, visible);
                SetImageObjectVisible(readyGlowImage, false);
                SetImageObjectVisible(readyRingImage, false);
                SetImageObjectVisible(readySparkImage, false);

                if (canvasGroup != null)
                {
                    if (!visible)
                    {
                        canvasGroup.alpha = 0f;
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;
                    }
                }
            }

            public void SetState(string label, string state, bool enabled)
            {
                SetState(label, state, enabled, enabled ? 1f : 0f);
            }

            public void SetState(string label, string state, bool enabled, float availabilityFill01)
            {
                SetVisible(true);
                ResolveStateVisuals();
                ApplyStaticVisuals();
                bool compactV22 = IsCompactV22Readout();
                string compactStatus = compactV22 ? ResolveCompactCooldown(state) : string.Empty;
                if (labelText != null)
                {
                    SetText(labelText, compactV22 ? ResolveCompactCost(label, state) : label);
                    if (compactV22)
                    {
                        bool showCost = string.IsNullOrEmpty(compactStatus);
                        SetGameObjectActive(labelText.gameObject, showCost);
                        Transform unit = labelText.transform.parent != null
                            ? labelText.transform.parent.Find("CostUnitText")
                            : null;
                        if (unit != null)
                        {
                            SetGameObjectActive(
                                unit.gameObject,
                                showCost && HasCompactEnergyCost(state));
                        }
                    }
                }

                if (stateText != null)
                {
                    string displayedState = compactV22 ? compactStatus : state;
                    SetText(stateText, displayedState);
                    SetGameObjectActive(stateText.gameObject, !string.IsNullOrEmpty(displayedState));
                    Color stateColor = enabled ? HealthReadoutColor : InputModeReadoutColor;
                    if (stateText.color != stateColor)
                    {
                        stateText.color = stateColor;
                    }
                }

                if (cooldownFill != null)
                {
                    ConfigureClockwiseSummonFill(cooldownFill, enabled, availabilityFill01);
                }

                ApplyIconReadiness(enabled, availabilityFill01);
                ApplyReadyEffect(enabled);
                ApplyVisualOrder();

                if (canvasGroup != null)
                {
                    float alpha = enabled ? 1f : 0.88f;
                    if (!Mathf.Approximately(canvasGroup.alpha, alpha))
                    {
                        canvasGroup.alpha = alpha;
                    }

                    if (canvasGroup.interactable != enabled)
                    {
                        canvasGroup.interactable = enabled;
                    }

                    if (canvasGroup.blocksRaycasts != enabled)
                    {
                        canvasGroup.blocksRaycasts = enabled;
                    }
                }
            }

            private bool IsCompactV22Readout()
            {
                return labelText != null
                    && stateText != null
                    && string.Equals(labelText.name, "CostText", StringComparison.Ordinal)
                    && string.Equals(stateText.name, "StatusText", StringComparison.Ordinal);
            }

            private static string ResolveCompactCost(string label, string state)
            {
                if (!string.IsNullOrEmpty(state))
                {
                    int energySuffix = state.IndexOf("EN", StringComparison.OrdinalIgnoreCase);
                    if (energySuffix > 0)
                    {
                        int start = 0;
                        while (start < energySuffix && char.IsWhiteSpace(state[start]))
                        {
                            start++;
                        }

                        int end = start;
                        while (end < energySuffix && char.IsDigit(state[end]))
                        {
                            end++;
                        }

                        if (end > start)
                        {
                            return state.Substring(start, end - start);
                        }
                    }
                }

                return label ?? string.Empty;
            }

            private static bool HasCompactEnergyCost(string state)
            {
                if (string.IsNullOrEmpty(state))
                {
                    return false;
                }

                int energySuffix = state.IndexOf("EN", StringComparison.OrdinalIgnoreCase);
                if (energySuffix <= 0)
                {
                    return false;
                }

                int index = 0;
                while (index < energySuffix && char.IsWhiteSpace(state[index]))
                {
                    index++;
                }

                int digitStart = index;
                while (index < energySuffix && char.IsDigit(state[index]))
                {
                    index++;
                }

                return index > digitStart;
            }

            private static string ResolveCompactCooldown(string state)
            {
                if (string.IsNullOrEmpty(state))
                {
                    return string.Empty;
                }

                int marker = state.IndexOf("CD ", StringComparison.OrdinalIgnoreCase);
                if (marker < 0)
                {
                    return string.Empty;
                }

                int start = marker + 3;
                int end = start;
                while (end < state.Length
                    && (char.IsDigit(state[end]) || state[end] == '.'))
                {
                    end++;
                }

                return end > start ? state.Substring(start, end - start) : string.Empty;
            }

            private void ApplyStaticVisuals()
            {
                if (staticVisualsApplied)
                {
                    return;
                }

                if (labelText != null)
                {
                    labelText.fontStyle = FontStyle.Bold;
                    labelText.color = HealthReadoutColor;
                    ApplySlotTextOutline(labelText);
                }

                if (stateText != null)
                {
                    stateText.fontSize = Mathf.Max(stateText.fontSize, 20);
                    stateText.fontStyle = FontStyle.Bold;
                    stateText.alignment = TextAnchor.MiddleCenter;
                    stateText.lineSpacing = 0.86f;
                    stateText.resizeTextForBestFit = true;
                    stateText.resizeTextMinSize = 14;
                    stateText.resizeTextMaxSize = stateText.fontSize;
                    stateText.horizontalOverflow = HorizontalWrapMode.Wrap;
                    stateText.verticalOverflow = VerticalWrapMode.Overflow;
                    ApplySlotTextOutline(stateText);
                }

                staticVisualsApplied = true;
            }

            private void ResolveStateVisuals()
            {
                Transform root = ResolveSlotRoot();
                if (root == null)
                {
                    return;
                }

                cooldownFill ??= FindChildImage(root, "CooldownFill");
                iconImage ??= FindChildImage(root, "Icon");
                unavailableIconImage ??= FindChildImage(root, "IconDisabled");
                if (IsCompactV22Readout())
                {
                    cooldownFill ??= FindChildImage(root, "StateArc");
                    // The componentized rail deliberately has no breathing glow, enlarged
                    // ready ring, or rotating spark. Do not rediscover the inactive V19
                    // children after the assembler clears their serialized references.
                    readyGlowImage = null;
                    readyRingImage = null;
                    readySparkImage = null;
                    return;
                }

                readyGlowImage ??= FindChildImage(root, "ReadyGlow");
                readyRingImage ??= FindChildImage(root, "ReadyRing");
                readySparkImage ??= FindChildImage(root, "ReadySparkRing");
            }

            private void ApplyVisualOrder()
            {
                if (visualOrderApplied)
                {
                    return;
                }

                if (readyGlowImage != null)
                {
                    readyGlowImage.transform.SetAsFirstSibling();
                }

                if (cooldownFill != null)
                {
                    cooldownFill.transform.SetAsLastSibling();
                }

                if (unavailableIconImage != null)
                {
                    unavailableIconImage.transform.SetAsLastSibling();
                }

                if (readyRingImage != null)
                {
                    readyRingImage.transform.SetAsLastSibling();
                }

                if (readySparkImage != null)
                {
                    readySparkImage.transform.SetAsLastSibling();
                }

                if (labelText != null)
                {
                    labelText.transform.SetAsLastSibling();
                }

                if (stateText != null)
                {
                    stateText.transform.SetAsLastSibling();
                }

                visualOrderApplied = true;
            }

            private Transform ResolveSlotRoot()
            {
                if (canvasGroup != null)
                {
                    return canvasGroup.transform;
                }

                if (labelText != null && labelText.transform.parent != null)
                {
                    return labelText.transform.parent;
                }

                if (stateText != null && stateText.transform.parent != null)
                {
                    return stateText.transform.parent;
                }

                return cooldownFill != null && cooldownFill.transform.parent != null
                    ? cooldownFill.transform.parent
                    : null;
            }

            private void ApplyIconReadiness(bool ready, float availabilityFill01)
            {
                float fill = Mathf.Clamp01(availabilityFill01);
                if (iconImage != null)
                {
                    SetGameObjectActive(iconImage.gameObject, true);
                    Color iconColor = ready ? SummonReadyIconColor : new Color(0.94f, 0.97f, 1f, 0.96f);
                    if (iconImage.color != iconColor)
                    {
                        iconImage.color = iconColor;
                    }
                }

                if (unavailableIconImage != null)
                {
                    bool showWipe = !ready && fill < 0.999f;
                    SetGameObjectActive(unavailableIconImage.gameObject, showWipe);
                    unavailableIconImage.raycastTarget = false;
                    unavailableIconImage.preserveAspect = true;
                    unavailableIconImage.type = Image.Type.Filled;
                    unavailableIconImage.fillMethod = Image.FillMethod.Radial360;
                    unavailableIconImage.fillOrigin = (int)Image.Origin360.Top;
                    unavailableIconImage.fillClockwise = false;
                    float wipeFill = showWipe ? 1f - fill : 0f;
                    if (!Mathf.Approximately(unavailableIconImage.fillAmount, wipeFill))
                    {
                        unavailableIconImage.fillAmount = wipeFill;
                    }
                    Color color = SummonUnavailableIconColor;
                    color.a = showWipe ? Mathf.Lerp(0.92f, 0.78f, fill) : 0f;
                    if (unavailableIconImage.color != color)
                    {
                        unavailableIconImage.color = color;
                    }
                }
            }

            private void ApplyReadyEffect(bool ready)
            {
                float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 7.2f) * 0.5f;
                ApplyReadyImage(readyGlowImage, ready, SummonReadyGlowColor, 0.96f + pulse * 0.04f);
                ApplyReadyImage(readyRingImage, ready, SummonReadyRingColor, 0.98f + pulse * 0.02f);
                ApplyReadyImage(readySparkImage, ready, SummonReadySparkColor, 0.94f + pulse * 0.06f);

                if (readyRingImage != null)
                {
                    readyRingImage.rectTransform.localScale = Vector3.one * (1.16f + pulse * 0.22f);
                }

                if (readySparkImage != null)
                {
                    readySparkImage.rectTransform.localRotation =
                        Quaternion.Euler(0f, 0f, -Time.unscaledTime * 148f);
                    readySparkImage.rectTransform.localScale = Vector3.one * (1.14f + pulse * 0.2f);
                }
            }

            private static void ApplyReadyImage(Image image, bool visible, Color baseColor, float alpha)
            {
                if (image == null)
                {
                    return;
                }

                SetGameObjectActive(image.gameObject, visible);
                Color color = baseColor;
                color.a = visible ? Mathf.Clamp01(alpha) : 0f;
                if (image.color != color)
                {
                    image.color = color;
                }
                image.raycastTarget = false;
            }

            private static void SetImageObjectVisible(Image image, bool visible)
            {
                if (image != null)
                {
                    SetGameObjectActive(image.gameObject, visible);
                }
            }

            private static void SetGameObjectActive(GameObject gameObject, bool active)
            {
                if (gameObject != null && gameObject.activeSelf != active)
                {
                    gameObject.SetActive(active);
                }
            }

            private static Image FindChildImage(Transform root, string objectName)
            {
                Transform found = FindDeepChild(root, objectName);
                return found != null ? found.GetComponent<Image>() : null;
            }

            private static void ConfigureClockwiseSummonFill(Image image, bool enabled, float availabilityFill01)
            {
                float fill = Mathf.Clamp01(availabilityFill01);
                bool compactV22 = string.Equals(image.name, "StateArc", StringComparison.Ordinal);
                bool targetAtomicAccent =
                    image.GetComponentInParent<CombatHudCelestialTargetLayoutProfile>() != null;
                bool showFill = compactV22
                    ? fill > 0.001f
                    : !enabled && fill > 0.001f;
                image.enabled = true;
                image.raycastTarget = false;
                image.preserveAspect = false;
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Radial360;
                image.fillOrigin = (int)Image.Origin360.Top;
                image.fillClockwise = true;
                float displayedFill = showFill ? enabled && compactV22 ? 1f : fill : 0f;
                if (!Mathf.Approximately(image.fillAmount, displayedFill))
                {
                    image.fillAmount = displayedFill;
                }
                Color color = targetAtomicAccent
                    ? Color.white
                    : compactV22
                    ? enabled
                        ? new Color(0.26f, 0.92f, 1f, 1f)
                        : new Color(1f, 0.80f, 0.48f, 1f)
                    : SummonChargingFillColor;
                color.a = showFill ? Mathf.Lerp(0.70f, 0.96f, fill) : 0f;
                if (image.color != color)
                {
                    image.color = color;
                }

                SetGameObjectActive(image.gameObject, showFill);
            }

            private static void ApplySlotTextOutline(Text text)
            {
                Outline outline = text.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = text.gameObject.AddComponent<Outline>();
                }

                outline.effectColor = ReadoutOutlineColor;
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = true;
            }
        }

        [SerializeField] private Text objectiveText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text resourceText;
        [SerializeField] private Text inputModeText;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text actionFeedbackText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image resourceFill;
        [SerializeField] private RectTransform bossHudRoot;
        [SerializeField] private Text bossNameText;
        [SerializeField] private Text bossHealthText;
        [SerializeField] private Text bossResourceText;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private Image bossResourceFill;
        [SerializeField] private RectTransform aimReticleRoot;
        [SerializeField] private Image[] aimReticleSegments = Array.Empty<Image>();
        [SerializeField] private Color aimReticleColor = new Color(0.82f, 0.96f, 1f, 0.88f);
        [SerializeField] private Color aimReticleActiveColor = new Color(0.42f, 0.95f, 1f, 0.96f);
        [SerializeField] private Image playerDamageOverlayImage;
        [SerializeField] private Color playerDamageOverlayColor = new Color(1f, 0.02f, 0.01f, 0.58f);
        [SerializeField, Min(0.05f)] private float playerDamageOverlaySeconds = 0.56f;
        [SerializeField] private Color playerHealthDecreaseFlashColor = new Color(1f, 0.18f, 0.08f, 1f);
        [SerializeField] private Color playerResourceDecreaseFlashColor = new Color(0.16f, 1f, 1f, 1f);
        [SerializeField] private Color bossHealthDecreaseFlashColor = new Color(1f, 0.78f, 0.08f, 1f);
        [SerializeField] private Color bossResourceDecreaseFlashColor = new Color(0.35f, 0.86f, 1f, 1f);
        [SerializeField, Min(0.05f)] private float meterDecreaseFlashSeconds = 0.24f;
        [SerializeField] private CombatHudActionCatalog actionCatalog;
        [SerializeField] private ActionSlotBinding[] actionSlots = Array.Empty<ActionSlotBinding>();
        [SerializeField] private SummonSlotBinding[] summonSlots = Array.Empty<SummonSlotBinding>();
        [SerializeField, Tooltip("Target v23 only. Keeps the optional timer hidden unless explicitly enabled.")]
        private bool celestialTargetTimerVisible;

        private float bossHealthFillBaseWidth = -1f;
        private float bossResourceFillBaseWidth = -1f;
        private float playerDamageOverlayTimer;
        private float lastObservedPlayerHealth = -1f;
        private float lastPlayerHealthRatio = -1f;
        private float lastPlayerResourceRatio = -1f;
        private float lastBossHealthRatio = -1f;
        private float lastBossResourceRatio = -1f;
        private float playerHealthDecreaseFlashTimer;
        private float playerResourceDecreaseFlashTimer;
        private float bossHealthDecreaseFlashTimer;
        private float bossResourceDecreaseFlashTimer;
        private bool hasActiveMeterDecreaseFlash;
        private Coroutine feedbackRoutine;
        private Color playerHealthBaseColor = Color.white;
        private Color playerResourceBaseColor = Color.white;
        private Color bossHealthBaseColor = Color.white;
        private Color bossResourceBaseColor = Color.white;
        private bool playerHealthBaseColorCaptured;
        private bool playerResourceBaseColorCaptured;
        private bool bossHealthBaseColorCaptured;
        private bool bossResourceBaseColorCaptured;
        private int lastTimerSecond = int.MinValue;
        private int lastHealthCurrent = int.MinValue;
        private int lastHealthMax = int.MinValue;
        private int lastResourceCurrent = int.MinValue;
        private int lastResourceMax = int.MinValue;
        private bool ammoStyleInitialized;
        private bool lastAmmoReloading;
        private bool aimReticleStateInitialized;
        private bool lastAimReticleVisible;
        private bool lastAimReticleActive;
        private bool bossHudVisibilityInitialized;
        private bool bossHudVisible;
        private CombatHudCelestialV2LayoutProfile celestialV22Layout;
        private CombatHudCelestialTargetLayoutProfile celestialTargetLayout;
        private bool celestialV22TimerRequested;
        private bool celestialV22TimerFits = true;
        private int lastBossHealthCurrent = int.MinValue;
        private int lastBossHealthMax = int.MinValue;
        private int lastBossResourceCurrent = int.MinValue;
        private int lastBossResourceMax = int.MinValue;

        public float BossHealthFillAmount => bossHealthFill != null ? bossHealthFill.fillAmount : 0f;
        public float BossResourceFillAmount => bossResourceFill != null ? bossResourceFill.fillAmount : 0f;
        public bool BossHudVisible => bossHudRoot != null
            ? bossHudRoot.gameObject.activeInHierarchy
            : bossHealthFill != null && bossHealthFill.gameObject.activeInHierarchy;
        public bool AimReticleVisible => aimReticleRoot != null && aimReticleRoot.gameObject.activeInHierarchy;

        private void Awake()
        {
            celestialV22Layout = GetComponent<CombatHudCelestialV2LayoutProfile>();
            celestialTargetLayout = GetComponent<CombatHudCelestialTargetLayoutProfile>();
            ResolveOptionalRuntimeReferences();
            DisableDuplicateHudText("AmmoText", ammoText);
            ApplyPlayerReadoutStyles();
            ammoStyleInitialized = ammoText != null;
            lastAmmoReloading = false;
            ApplyResponsiveSideLayout();
            ApplyBossHeaderSpacing();
            EnsureAimReticle();
            EnsurePlayerDamageOverlay();
            CaptureMeterBaseColors();
        }

        private void OnEnable()
        {
            RefreshSafeAreaLayoutIfNeeded(true);
            StartFeedbackRoutineIfNeeded();
        }

        private void OnDisable()
        {
            StopFeedbackRoutine();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            ApplyResponsiveSideLayout();
            ApplyBossHeaderSpacing();
        }

        private void LateUpdate()
        {
            RefreshSafeAreaLayoutIfNeeded(false);
        }

        private IEnumerator RefreshFeedbackUntilSettled()
        {
            yield return null;

            while (isActiveAndEnabled && HasActiveFeedbackTimer())
            {
                UpdatePlayerDamageOverlay();
                UpdateMeterDecreaseFlashes();
                if (!HasActiveFeedbackTimer())
                {
                    break;
                }

                yield return null;
            }

            feedbackRoutine = null;
        }

        public void SetObjective(string objective)
        {
            SetText(objectiveText, objective);
        }

        public void SetTimer(float secondsRemaining)
        {
            if (UsesCelestialTargetLayout)
            {
                SetCelestialV22TimerVisible(
                    celestialTargetTimerVisible && celestialV22TimerFits);
            }
            else if (UsesCelestialV22Layout && !celestialV22TimerRequested)
            {
                celestialV22TimerRequested = true;
                SetCelestialV22TimerVisible(celestialV22TimerFits);
            }

            float clamped = Mathf.Max(0f, secondsRemaining);
            int wholeSeconds = Mathf.FloorToInt(clamped);
            if (wholeSeconds == lastTimerSecond)
            {
                return;
            }

            lastTimerSecond = wholeSeconds;
            int minutes = wholeSeconds / 60;
            int seconds = wholeSeconds % 60;
            SetText(timerText, $"{minutes:00}:{seconds:00}");
        }

        public void SetCelestialTargetTimerVisible(bool visible)
        {
            if (!UsesCelestialTargetLayout)
            {
                return;
            }

            celestialTargetTimerVisible = visible;
            SetCelestialV22TimerVisible(visible && celestialV22TimerFits);
        }

        public void SetHealth(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (lastObservedPlayerHealth >= 0f && current < lastObservedPlayerHealth - 0.01f)
            {
                TriggerPlayerDamageOverlay();
            }

            TriggerDecreaseFlashIfNeeded(ratio, ref lastPlayerHealthRatio, ref playerHealthDecreaseFlashTimer);
            lastObservedPlayerHealth = Mathf.Max(0f, current);
            if (healthFill != null)
            {
                healthFill.fillAmount = ratio;
            }

            int displayedCurrent = Mathf.CeilToInt(Mathf.Max(0f, current));
            int displayedMax = Mathf.CeilToInt(Mathf.Max(0f, max));
            if (displayedCurrent != lastHealthCurrent || displayedMax != lastHealthMax)
            {
                lastHealthCurrent = displayedCurrent;
                lastHealthMax = displayedMax;
                SetText(healthText, $"{displayedCurrent}/{displayedMax}");
            }
        }

        public void ShowPlayerDamageOverlay()
        {
            TriggerPlayerDamageOverlay();
        }

        public void SetBossHealth(float current, float max)
        {
            SetBossHudVisible(max > 0f);
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            TriggerDecreaseFlashIfNeeded(ratio, ref lastBossHealthRatio, ref bossHealthDecreaseFlashTimer);
            if (bossHealthFill != null)
            {
                ApplyHorizontalMeter(bossHealthFill, ratio, ref bossHealthFillBaseWidth);
            }

            if (bossHealthText != null && bossHealthText != actionFeedbackText)
            {
                if (UsesReviewedCelestialLayout)
                {
                    int displayedCurrent = Mathf.CeilToInt(Mathf.Max(0f, current));
                    int displayedMax = Mathf.CeilToInt(Mathf.Max(0f, max));
                    if (displayedCurrent != lastBossHealthCurrent || displayedMax != lastBossHealthMax)
                    {
                        lastBossHealthCurrent = displayedCurrent;
                        lastBossHealthMax = displayedMax;
                        SetText(bossHealthText, $"{displayedCurrent}/{displayedMax}");
                    }

                    bossHealthText.gameObject.SetActive(max > 0f);
                }
                else
                {
                    SetText(bossHealthText, string.Empty);
                    bossHealthText.gameObject.SetActive(false);
                }
            }
        }

        public void SetBossName(string displayName)
        {
            ResolveOptionalRuntimeReferences();
            SetText(bossNameText, displayName);
        }

        public void SetBossHudVisible(bool visible)
        {
            ResolveOptionalRuntimeReferences();
            bool currentVisibility = bossHealthFill != null
                ? bossHealthFill.gameObject.activeInHierarchy
                : bossHudRoot != null && bossHudRoot.gameObject.activeInHierarchy;
            if (bossHudVisibilityInitialized
                && bossHudVisible == visible
                && currentVisibility == visible)
            {
                return;
            }

            bossHudVisibilityInitialized = true;
            bossHudVisible = visible;
            if (bossHudRoot != null)
            {
                SetGameObjectActive(bossHudRoot.gameObject, visible);
                return;
            }

            // Compatibility for scenes authored before the dedicated boss HUD root existed.
            for (int i = 0; i < BossHudGraphicNames.Length; i++)
            {
                SetNamedHudObjectActive(BossHudGraphicNames[i], visible);
            }
        }

        public void SetBossResource(float current, float max)
        {
            ResolveOptionalRuntimeReferences();
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            TriggerDecreaseFlashIfNeeded(ratio, ref lastBossResourceRatio, ref bossResourceDecreaseFlashTimer);
            if (bossResourceFill != null)
            {
                ApplyHorizontalMeter(bossResourceFill, ratio, ref bossResourceFillBaseWidth);
            }

            if (UsesReviewedCelestialLayout && bossResourceText != null)
            {
                int displayedCurrent = Mathf.CeilToInt(Mathf.Max(0f, current));
                int displayedMax = Mathf.CeilToInt(Mathf.Max(0f, max));
                if (displayedCurrent != lastBossResourceCurrent || displayedMax != lastBossResourceMax)
                {
                    lastBossResourceCurrent = displayedCurrent;
                    lastBossResourceMax = displayedMax;
                    SetText(bossResourceText, $"{displayedCurrent}/{displayedMax}");
                }

                bossResourceText.gameObject.SetActive(max > 0f);
            }
        }

        private void EnsurePlayerDamageOverlay()
        {
            if (playerDamageOverlayImage == null)
            {
                playerDamageOverlayImage = FindImage("PlayerDamageOverlay");
            }

            if (playerDamageOverlayImage == null)
            {
                GameObject overlay = new GameObject(
                    "PlayerDamageOverlay",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                overlay.transform.SetParent(transform, worldPositionStays: false);
                playerDamageOverlayImage = overlay.GetComponent<Image>();
            }

            RectTransform rectTransform = playerDamageOverlayImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            playerDamageOverlayImage.raycastTarget = false;
            playerDamageOverlayImage.color = Color.clear;
            playerDamageOverlayImage.gameObject.SetActive(false);
            playerDamageOverlayImage.transform.SetAsLastSibling();
        }

        private void TriggerPlayerDamageOverlay()
        {
            EnsurePlayerDamageOverlay();
            playerDamageOverlayTimer = Mathf.Max(playerDamageOverlayTimer, playerDamageOverlaySeconds);
            if (playerDamageOverlayImage != null)
            {
                playerDamageOverlayImage.gameObject.SetActive(true);
                playerDamageOverlayImage.transform.SetAsLastSibling();
                ApplyPlayerDamageOverlayVisual();
            }

            StartFeedbackRoutineIfNeeded();
        }

        private void UpdatePlayerDamageOverlay()
        {
            if (playerDamageOverlayImage == null)
            {
                return;
            }

            if (playerDamageOverlayTimer <= 0f)
            {
                if (playerDamageOverlayImage.gameObject.activeSelf)
                {
                    playerDamageOverlayImage.color = Color.clear;
                    playerDamageOverlayImage.gameObject.SetActive(false);
                }

                return;
            }

            playerDamageOverlayTimer = Mathf.Max(0f, playerDamageOverlayTimer - Time.unscaledDeltaTime);
            ApplyPlayerDamageOverlayVisual();
        }

        private void ApplyPlayerDamageOverlayVisual()
        {
            if (playerDamageOverlayImage == null)
            {
                return;
            }

            float duration = Mathf.Max(0.05f, playerDamageOverlaySeconds);
            float normalized = Mathf.Clamp01(playerDamageOverlayTimer / duration);
            float alpha = playerDamageOverlayColor.a * Mathf.SmoothStep(0f, 1f, normalized);
            Color color = playerDamageOverlayColor;
            color.a = alpha;
            playerDamageOverlayImage.color = color;
            playerDamageOverlayImage.gameObject.SetActive(alpha > 0.001f);
        }

        public void SetAimReticleVisible(bool visible, bool active)
        {
            EnsureAimReticle();
            if (aimReticleRoot == null)
            {
                return;
            }

            if (aimReticleStateInitialized
                && lastAimReticleVisible == visible
                && lastAimReticleActive == active)
            {
                return;
            }

            aimReticleStateInitialized = true;
            lastAimReticleVisible = visible;
            lastAimReticleActive = active;

            aimReticleRoot.gameObject.SetActive(visible);
            Color color = active
                ? aimReticleActiveColor
                : UsesCelestialTargetLayout
                    ? Color.white
                    : aimReticleColor;
            for (int i = 0; i < aimReticleSegments.Length; i++)
            {
                if (aimReticleSegments[i] != null)
                {
                    aimReticleSegments[i].color = color;
                }
            }
        }

        public void SetResource(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            TriggerDecreaseFlashIfNeeded(ratio, ref lastPlayerResourceRatio, ref playerResourceDecreaseFlashTimer);
            if (resourceFill != null)
            {
                resourceFill.fillAmount = ratio;
            }

            int displayedCurrent = Mathf.CeilToInt(Mathf.Max(0f, current));
            int displayedMax = Mathf.CeilToInt(Mathf.Max(0f, max));
            if (displayedCurrent != lastResourceCurrent || displayedMax != lastResourceMax)
            {
                lastResourceCurrent = displayedCurrent;
                lastResourceMax = displayedMax;
                SetText(resourceText, $"{displayedCurrent}/{displayedMax}");
            }
        }

        public void SetInputMode(string label)
        {
            if (UsesCelestialTargetLayout)
            {
                SetText(inputModeText, string.Empty);
                if (inputModeText != null)
                {
                    SetGameObjectActive(inputModeText.gameObject, false);
                }

                return;
            }

            if (UsesCelestialV22Layout)
            {
                string compact = CompactCelestialV22ModeLabel(label);
                SetText(inputModeText, compact);
                if (inputModeText != null)
                {
                    SetGameObjectActive(inputModeText.gameObject, !string.IsNullOrWhiteSpace(compact));
                }

                return;
            }

            bool isRedundantWeaponMode = string.Equals(label, "MELEE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "RANGED", StringComparison.OrdinalIgnoreCase);
            string displayedLabel = isRedundantWeaponMode ? string.Empty : label;
            SetText(inputModeText, displayedLabel);
            if (inputModeText != null)
            {
                SetGameObjectActive(inputModeText.gameObject, !string.IsNullOrWhiteSpace(displayedLabel));
            }
        }

        public void SetAmmo(string label, bool reloading)
        {
            ResolveOptionalRuntimeReferences();
            if (ammoText == null)
            {
                return;
            }

            string displayedLabel = UsesCelestialTargetLayout
                ? CompactCelestialTargetAmmoLabel(label, reloading)
                : label;
            bool visible = !string.IsNullOrWhiteSpace(displayedLabel);
            if (UsesCelestialTargetLayout)
            {
                SetNamedHudObjectActive("PlayerAmmoChip", visible);
                SetNamedHudObjectActive("PlayerModeCell", visible);
            }

            ammoText.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            int fontSize = 32;
            if (!ammoStyleInitialized || lastAmmoReloading != reloading)
            {
                if (UsesCelestialTargetLayout)
                {
                    ApplyExactPlayerReadoutStyle(
                        ammoText,
                        25,
                        reloading ? TargetReloadReadoutColor : TargetIvoryReadoutColor,
                        TextAnchor.MiddleRight);
                }
                else if (UsesCelestialV22Layout)
                {
                    ApplyExactPlayerReadoutStyle(
                        ammoText,
                        30,
                        reloading ? InputModeReadoutColor : AmmoReadoutColor,
                        TextAnchor.MiddleRight);
                }
                else
                {
                    ApplyPlayerReadoutStyle(
                        ammoText,
                        fontSize,
                        reloading ? InputModeReadoutColor : AmmoReadoutColor);
                }
                ammoStyleInitialized = true;
                lastAmmoReloading = reloading;
            }

            SetText(ammoText, displayedLabel);
        }

        private static string CompactCelestialTargetAmmoLabel(string label, bool reloading)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            string compact = label.Trim();
            if (reloading)
            {
                int reloadMarker = compact.IndexOf("RLD", StringComparison.OrdinalIgnoreCase);
                return reloadMarker >= 0
                    ? compact.Substring(reloadMarker).Trim()
                    : compact;
            }

            int slash = compact.IndexOf('/');
            if (slash <= 0 || slash >= compact.Length - 1)
            {
                return compact;
            }

            string current = compact.Substring(0, slash).Trim();
            string capacity = compact.Substring(slash + 1).Trim();
            return $"{current} / {capacity}";
        }

        private static string CompactCelestialV22ModeLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            if (string.Equals(label, "MELEE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "RANGED", StringComparison.OrdinalIgnoreCase))
            {
                return label.ToUpperInvariant();
            }

            return label
                .Replace(" READY ", " ")
                .Replace(" READY", string.Empty)
                .Replace(" x", " ×")
                .Trim();
        }

        public void SetSkillCooldown(CombatHudActionId actionId, float normalizedRemaining, string label, float secondsRemaining = -1f)
        {
            ActionSlotBinding slot = FindActionSlot(actionId);
            slot?.SetCooldown(normalizedRemaining, label, secondsRemaining);
        }

        public void SetActionInputAvailable(CombatHudActionId actionId, bool available)
        {
            ActionSlotBinding slot = FindActionSlot(actionId);
            slot?.SetInputAvailable(available);
        }

        public void SetSummonSlotState(CombatHudActionId actionId, string label, string state, bool enabled)
        {
            SummonSlotBinding slot = FindSummonSlot(actionId);
            slot?.SetState(label, state, enabled);
        }

        public void SetSummonSlotState(
            CombatHudActionId actionId,
            string label,
            string state,
            bool enabled,
            float availabilityFill01)
        {
            SummonSlotBinding slot = FindSummonSlot(actionId);
            slot?.SetState(label, state, enabled, availabilityFill01);
        }

        public void SetSummonSlotVisible(CombatHudActionId actionId, bool visible)
        {
            SummonSlotBinding slot = FindSummonSlot(actionId);
            slot?.SetVisible(visible);
        }

        public void SetActionFeedback(CombatHudActionId actionId)
        {
            if (actionId == CombatHudActionId.None)
            {
                SetActionFeedbackText(string.Empty);
                return;
            }

            if (actionCatalog != null && actionCatalog.TryGetAction(actionId, out CombatHudActionCatalog.ActionEntry action))
            {
                SetActionFeedbackText(action.DisplayName);
                return;
            }

            SetActionFeedbackText(string.Empty);
        }

        public void SetActionFeedbackText(string feedback)
        {
            SetText(actionFeedbackText, feedback);
        }

        private void ResolveOptionalRuntimeReferences()
        {
            if (bossHudRoot == null)
            {
                Transform root = FindDeepChild(transform, "BossHudRoot");
                bossHudRoot = root != null ? root.GetComponent<RectTransform>() : null;
            }

            if (bossHealthFill == null)
            {
                bossHealthFill = FindImage("BossHpFill");
            }

            if (bossResourceFill == null)
            {
                bossResourceFill = FindImage("BossCostFill");
            }

            if (bossHealthText == null)
            {
                bossHealthText = FindText("BossHpText");
            }

            if (bossNameText == null)
            {
                bossNameText = FindText("BossNameText");
            }

            if (bossResourceText == null)
            {
                bossResourceText = FindText("BossCostText");
            }

            if (ammoText == null)
            {
                ammoText = FindText("AmmoText");
            }

            CaptureMeterBaseColors();
        }

        private void CaptureMeterBaseColors()
        {
            CaptureMeterBaseColor(healthFill, ref playerHealthBaseColor, ref playerHealthBaseColorCaptured);
            CaptureMeterBaseColor(resourceFill, ref playerResourceBaseColor, ref playerResourceBaseColorCaptured);
            CaptureMeterBaseColor(bossHealthFill, ref bossHealthBaseColor, ref bossHealthBaseColorCaptured);
            CaptureMeterBaseColor(bossResourceFill, ref bossResourceBaseColor, ref bossResourceBaseColorCaptured);
        }

        private void UpdateMeterDecreaseFlashes()
        {
            if (!hasActiveMeterDecreaseFlash)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : 1f / 60f;
            playerHealthDecreaseFlashTimer = Mathf.Max(0f, playerHealthDecreaseFlashTimer - deltaTime);
            playerResourceDecreaseFlashTimer = Mathf.Max(0f, playerResourceDecreaseFlashTimer - deltaTime);
            bossHealthDecreaseFlashTimer = Mathf.Max(0f, bossHealthDecreaseFlashTimer - deltaTime);
            bossResourceDecreaseFlashTimer = Mathf.Max(0f, bossResourceDecreaseFlashTimer - deltaTime);

            ApplyMeterDecreaseFlash(
                healthFill,
                playerHealthBaseColor,
                playerHealthDecreaseFlashColor,
                playerHealthDecreaseFlashTimer);
            ApplyMeterDecreaseFlash(
                resourceFill,
                playerResourceBaseColor,
                playerResourceDecreaseFlashColor,
                playerResourceDecreaseFlashTimer);
            ApplyMeterDecreaseFlash(
                bossHealthFill,
                bossHealthBaseColor,
                bossHealthDecreaseFlashColor,
                bossHealthDecreaseFlashTimer);
            ApplyMeterDecreaseFlash(
                bossResourceFill,
                bossResourceBaseColor,
                bossResourceDecreaseFlashColor,
                bossResourceDecreaseFlashTimer);

            hasActiveMeterDecreaseFlash = playerHealthDecreaseFlashTimer > 0f
                || playerResourceDecreaseFlashTimer > 0f
                || bossHealthDecreaseFlashTimer > 0f
                || bossResourceDecreaseFlashTimer > 0f;
        }

        private void TriggerDecreaseFlashIfNeeded(
            float ratio,
            ref float lastRatio,
            ref float flashTimer)
        {
            if (lastRatio >= 0f && ratio < lastRatio - 0.001f)
            {
                flashTimer = Mathf.Max(flashTimer, meterDecreaseFlashSeconds);
                hasActiveMeterDecreaseFlash = true;
                StartFeedbackRoutineIfNeeded();
            }

            lastRatio = ratio;
        }

        private bool HasActiveFeedbackTimer()
        {
            return playerDamageOverlayTimer > 0f || hasActiveMeterDecreaseFlash;
        }

        private void StartFeedbackRoutineIfNeeded()
        {
            if (feedbackRoutine == null && Application.isPlaying && isActiveAndEnabled && HasActiveFeedbackTimer())
            {
                feedbackRoutine = StartCoroutine(RefreshFeedbackUntilSettled());
            }
        }

        private void StopFeedbackRoutine()
        {
            if (feedbackRoutine == null)
            {
                return;
            }

            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        private void ApplyMeterDecreaseFlash(Image image, Color baseColor, Color flashColor, float timer)
        {
            if (image == null)
            {
                return;
            }

            float duration = Mathf.Max(0.05f, meterDecreaseFlashSeconds);
            float normalized = Mathf.Clamp01(timer / duration);
            float weight = Mathf.SmoothStep(0f, 1f, normalized);
            image.color = Color.Lerp(baseColor, flashColor, weight);
        }

        private static void CaptureMeterBaseColor(Image image, ref Color color, ref bool captured)
        {
            if (captured || image == null)
            {
                return;
            }

            color = image.color;
            captured = true;
        }

        private void ApplyPlayerReadoutStyles()
        {
            if (UsesCelestialTargetLayout)
            {
                ApplyExactPlayerReadoutStyle(
                    healthText,
                    30,
                    TargetIvoryReadoutColor,
                    TextAnchor.MiddleLeft);
                ApplyExactPlayerReadoutStyle(resourceText, 23, ResourceReadoutColor, TextAnchor.MiddleRight);
                ApplyExactPlayerReadoutStyle(inputModeText, 20, InputModeReadoutColor, TextAnchor.MiddleCenter);
                ApplyExactPlayerReadoutStyle(
                    ammoText,
                    25,
                    TargetIvoryReadoutColor,
                    TextAnchor.MiddleRight);
                return;
            }

            if (UsesCelestialV22Layout)
            {
                ApplyExactPlayerReadoutStyle(healthText, 28, HealthReadoutColor, TextAnchor.MiddleLeft);
                ApplyExactPlayerReadoutStyle(resourceText, 23, ResourceReadoutColor, TextAnchor.MiddleRight);
                ApplyExactPlayerReadoutStyle(inputModeText, 24, InputModeReadoutColor, TextAnchor.MiddleCenter);
                ApplyExactPlayerReadoutStyle(ammoText, 30, AmmoReadoutColor, TextAnchor.MiddleRight);
                return;
            }

            ApplyPlayerReadoutStyle(healthText, 29, HealthReadoutColor);
            ApplyPlayerReadoutStyle(resourceText, 24, ResourceReadoutColor);
            ApplyPlayerReadoutStyle(inputModeText, 20, InputModeReadoutColor);
            ApplyPlayerReadoutStyle(ammoText, 27, AmmoReadoutColor);
        }

        private void ApplyResponsiveSideLayout()
        {
            lastAppliedSafeArea = Screen.safeArea;
            lastAppliedScreenSize = new Vector2Int(Screen.width, Screen.height);
            RectTransform root = transform as RectTransform;
            Vector2 canvasSize = root != null && root.rect.width > 1f && root.rect.height > 1f
                ? root.rect.size
                : new Vector2(DimensionHudDesignWidth, DimensionHudDesignHeight);
            safeAreaInsets = ScreenSafeAreaUtility.ResolveCanvasInsets(
                lastAppliedSafeArea,
                new Vector2(Screen.width, Screen.height),
                canvasSize);

            if (UsesCelestialTargetLayout)
            {
                ApplyCelestialTargetResponsiveLayout(canvasSize);
                return;
            }

            if (UsesCelestialV22Layout)
            {
                ApplyCelestialV22ResponsiveLayout(canvasSize);
                return;
            }

            ApplyLegacyResponsiveLayout(canvasSize);
        }

        private void ApplyLegacyResponsiveLayout(Vector2 canvasSize)
        {

            // Keep objective, timer, and system controls as separate information groups.
            // This mirrors the reviewed PGR-style hierarchy without changing bindings.
            ApplyResponsiveDesignRect("TopLeftPanel", new Rect(24f, 316f, 760f, 160f), ResponsiveHudAnchor.LeftTop);
            ApplyResponsiveDesignRect("Objective", new Rect(88f, 329f, 620f, 126f), ResponsiveHudAnchor.LeftTop);
            ApplyResponsiveMissionTimerLayout(canvasSize);
            ApplyResponsiveDesignRect("SettingsButton", new Rect(2250f, 47f, 100f, 95f), ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect("PauseButton", new Rect(2404f, 44f, 89f, 89f), ResponsiveHudAnchor.RightTop);

            ApplyResponsiveDesignRect("MoveJoystickRing", new Rect(201f, 979f, 269f, 269f), ResponsiveHudAnchor.LeftBottom);
            ApplyResponsiveDesignRect("MoveJoystickKnob", new Rect(285f, 1063f, 101f, 101f), ResponsiveHudAnchor.LeftBottom);

            // Preserve the existing action IDs: Ultimate is the weapon swap at upper-left,
            // Skill1 is the high-priority skill at upper-right, then Dodge and BasicAttack.
            ApplyResponsiveDesignRect("UltimateButton", new Rect(2059f, 967f, 171f, 171f), ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect("Skill1Button", new Rect(2261f, 938f, 187f, 187f), ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect("DodgeButton", new Rect(2046f, 1177f, 184f, 184f), ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect("BasicAttackButton", new Rect(2248f, 1131f, 273f, 272f), ResponsiveHudAnchor.RightBottom);

            ApplyResponsiveDesignRect("SummonSlot1Button", new Rect(2263f, 171f, 211f, 226f), ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect("SummonSlot2Button", new Rect(2275f, 413f, 193f, 211f), ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect("SummonSlot3Button", new Rect(2275f, 640f, 193f, 211f), ResponsiveHudAnchor.RightTop);

            ApplyResponsiveDesignRect("PlayerPortraitFrame", new Rect(686f, 1262f, 153f, 153f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("HealthBar_Track", new Rect(731f, 1287f, 944f, 49f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("HealthBar", new Rect(818f, 1302f, 766f, 15f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("HealthText", new Rect(1390f, 1246f, 214f, 45f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("ResourceBar_Track", new Rect(780f, 1333f, 846f, 40f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("ResourceBar", new Rect(818f, 1347f, 766f, 12f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("ResourceText", new Rect(1415f, 1324f, 180f, 43f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("InputMode", new Rect(805f, 1380f, 500f, 32f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("PlayerAmmoChip", new Rect(1614f, 1284f, 144f, 77f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("AmmoText", new Rect(1623f, 1294f, 125f, 56f), ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect("CenterAimReticle", new Rect(1232.5f, 672.5f, 95f, 95f), ResponsiveHudAnchor.CenterScreen);
            RefreshVirtualJoystickRestPosition();
        }

        private void ApplyCelestialV22ResponsiveLayout(Vector2 canvasSize)
        {
            ResetCelestialV22ResponsiveGroups();

            ApplyCelestialV22ObjectiveFrameRect();
            ApplyCelestialV22ObjectiveTextRect();
            ApplyResponsiveMissionTimerLayout(canvasSize);
            ApplyResponsiveDesignRect(
                "PauseButton",
                CombatHudCelestialV2LayoutProfile.PauseHit,
                ResponsiveHudAnchor.RightTop);

            ApplyResponsiveDesignRect(
                "MoveJoystickRing",
                CombatHudCelestialV2LayoutProfile.JoystickVisual,
                ResponsiveHudAnchor.LeftBottom);
            ApplyResponsiveDesignRect(
                "MoveJoystickKnob",
                CombatHudCelestialV2LayoutProfile.JoystickKnob,
                ResponsiveHudAnchor.LeftBottom);

            ApplyResponsiveDesignRect(
                "UltimateButton",
                CombatHudCelestialV2LayoutProfile.WeaponSwap,
                ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect(
                "Skill1Button",
                CombatHudCelestialV2LayoutProfile.Skill,
                ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect(
                "DodgeButton",
                CombatHudCelestialV2LayoutProfile.Dodge,
                ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect(
                "BasicAttackButton",
                CombatHudCelestialV2LayoutProfile.BasicAttack,
                ResponsiveHudAnchor.RightBottom);

            ApplyResponsiveDesignRect(
                "SummonSlot1Button",
                CombatHudCelestialV2LayoutProfile.SummonSlot1,
                ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect(
                "SummonSlot2Button",
                CombatHudCelestialV2LayoutProfile.SummonSlot2,
                ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect(
                "SummonSlot3Button",
                CombatHudCelestialV2LayoutProfile.SummonSlot3,
                ResponsiveHudAnchor.RightTop);

            ApplyResponsiveDesignRect(
                "PlayerPortraitFrame",
                CombatHudCelestialV2LayoutProfile.PlayerPortrait,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "HealthText",
                CombatHudCelestialV2LayoutProfile.PlayerHpText,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "HealthBar_Track",
                CombatHudCelestialV2LayoutProfile.PlayerHpTrack,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "HealthBar",
                CombatHudCelestialV2LayoutProfile.PlayerHpFill,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "ResourceBar_Track",
                CombatHudCelestialV2LayoutProfile.PlayerEnTrack,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "ResourceBar",
                CombatHudCelestialV2LayoutProfile.PlayerEnFill,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "ResourceText",
                CombatHudCelestialV2LayoutProfile.PlayerEnText,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "PlayerModeCell",
                CombatHudCelestialV2LayoutProfile.PlayerMode,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "InputMode",
                CombatHudCelestialV2LayoutProfile.PlayerMode,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "PlayerAmmoChip",
                CombatHudCelestialV2LayoutProfile.PlayerAmmo,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "AmmoText",
                CombatHudCelestialV2LayoutProfile.PlayerAmmo,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "CenterAimReticle",
                CombatHudCelestialV2LayoutProfile.Reticle,
                ResponsiveHudAnchor.CenterScreen);

            ApplyCelestialV22CollisionGuards(canvasSize);
            RefreshVirtualJoystickRestPosition();
        }

        private void ApplyCelestialTargetResponsiveLayout(Vector2 canvasSize)
        {
            ResetResponsiveGroup("PlayerHudTargetRoot");
            ResetResponsiveGroup("SummonRailTargetRoot");

            ApplyCelestialTargetObjectiveFrameRect();
            ApplyCelestialTargetObjectiveTextRect();
            ApplyResponsiveMissionTimerLayout(canvasSize);
            ApplyResponsiveDesignRect(
                "PauseButton",
                CombatHudCelestialTargetLayoutProfile.PauseHit,
                ResponsiveHudAnchor.RightTop);

            ApplyResponsiveDesignRect(
                "MoveJoystickRing",
                CombatHudCelestialTargetLayoutProfile.JoystickVisual,
                ResponsiveHudAnchor.LeftBottom);
            ApplyResponsiveDesignRect(
                "MoveJoystickKnob",
                CombatHudCelestialTargetLayoutProfile.JoystickKnob,
                ResponsiveHudAnchor.LeftBottom);

            ApplyResponsiveDesignRect(
                "UltimateButton",
                CombatHudCelestialTargetLayoutProfile.WeaponSwap,
                ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect(
                "Skill1Button",
                CombatHudCelestialTargetLayoutProfile.Ultimate,
                ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect(
                "DodgeButton",
                CombatHudCelestialTargetLayoutProfile.Dash,
                ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect(
                "BasicAttackButton",
                CombatHudCelestialTargetLayoutProfile.BasicAttack,
                ResponsiveHudAnchor.RightBottom);

            ApplyResponsiveDesignRect(
                "SummonSlot1Button",
                CombatHudCelestialTargetLayoutProfile.SummonSlot1,
                ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect(
                "SummonSlot2Button",
                CombatHudCelestialTargetLayoutProfile.SummonSlot2,
                ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect(
                "SummonSlot3Button",
                CombatHudCelestialTargetLayoutProfile.SummonSlot3,
                ResponsiveHudAnchor.RightTop);

            ApplyResponsiveDesignRect(
                "PlayerTargetChassis",
                CombatHudCelestialTargetLayoutProfile.PlayerComposite,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "PlayerPortraitFrame",
                CombatHudCelestialTargetLayoutProfile.PlayerPortrait,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "HealthText",
                CombatHudCelestialTargetLayoutProfile.PlayerHpText,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "HealthBar_Track",
                CombatHudCelestialTargetLayoutProfile.PlayerHpTrack,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "HealthBar",
                CombatHudCelestialTargetLayoutProfile.PlayerHpFill,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "ResourceBar_Track",
                CombatHudCelestialTargetLayoutProfile.PlayerCostTrack,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "ResourceBar",
                CombatHudCelestialTargetLayoutProfile.PlayerCostFill,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "PlayerModeCell",
                CombatHudCelestialTargetLayoutProfile.PlayerModeGlyph,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "PlayerAmmoChip",
                CombatHudCelestialTargetLayoutProfile.PlayerAmmo,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "AmmoText",
                CombatHudCelestialTargetLayoutProfile.PlayerAmmoText,
                ResponsiveHudAnchor.CenterBottom);
            ApplyResponsiveDesignRect(
                "CenterAimReticle",
                CombatHudCelestialTargetLayoutProfile.Reticle,
                ResponsiveHudAnchor.CenterScreen);

            ApplyCelestialTargetCollisionGuards(canvasSize);
            RefreshVirtualJoystickRestPosition();
        }

        private void ApplyCelestialTargetObjectiveFrameRect()
        {
            Transform found = FindDeepChild(transform, "TopLeftPanel");
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return;
            }

            Rect designRect = CombatHudCelestialTargetLayoutProfile.ObjectiveFrame;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(
                0f,
                -(designRect.yMin + safeAreaInsets.Top));
            rectTransform.sizeDelta = designRect.size;
            rectTransform.localScale = Vector3.one;
        }

        private void ApplyCelestialTargetObjectiveTextRect()
        {
            Transform found = FindDeepChild(transform, "Objective");
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return;
            }

            Rect resolvedRect = CombatHudCelestialTargetLayoutProfile.ResolveObjectiveText(
                safeAreaInsets.Left);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(
                resolvedRect.xMin,
                -(resolvedRect.yMin + safeAreaInsets.Top));
            rectTransform.sizeDelta = resolvedRect.size;
            rectTransform.localScale = Vector3.one;
        }

        private void ApplyCelestialTargetCollisionGuards(Vector2 canvasSize)
        {
            float actionLeft = canvasSize.x
                - (DimensionHudDesignWidth - CombatHudCelestialTargetLayoutProfile.Dash.xMin)
                - safeAreaInsets.Right;
            float playerCenter = canvasSize.x * 0.5f
                + CombatHudCelestialTargetLayoutProfile.PlayerComposite.center.x
                - DimensionHudDesignWidth * 0.5f;
            float playerHalfWidth =
                CombatHudCelestialTargetLayoutProfile.PlayerComposite.width * 0.5f;
            float missingGap = Mathf.Max(
                0f,
                CombatHudCelestialTargetLayoutProfile.MinimumPlayerActionGap
                    - (actionLeft - (playerCenter + playerHalfWidth)));
            float playerShift = Mathf.Min(
                CombatHudCelestialTargetLayoutProfile.MaximumPlayerLeftShift,
                missingGap);
            float remainingRight = actionLeft
                - CombatHudCelestialTargetLayoutProfile.MinimumPlayerActionGap
                - (playerCenter - playerShift);
            float playerScale = Mathf.Clamp(
                remainingRight / Mathf.Max(1f, playerHalfWidth),
                CombatHudCelestialTargetLayoutProfile.MinimumPlayerScale,
                1f);

            Transform playerGroup = FindDeepChild(transform, "PlayerHudTargetRoot");
            RectTransform playerGroupRect = playerGroup != null
                ? playerGroup.GetComponent<RectTransform>()
                : null;
            if (playerGroupRect != null)
            {
                playerGroupRect.anchoredPosition = new Vector2(-playerShift, 0f);
                playerGroupRect.localScale = new Vector3(playerScale, playerScale, 1f);
            }

            float playerLeft = playerCenter - playerShift - playerHalfWidth * playerScale;
            float joystickCenter = safeAreaInsets.Left
                + CombatHudCelestialTargetLayoutProfile.JoystickVisual.center.x;
            float availableActivationHalfWidth = playerLeft
                - CombatHudCelestialTargetLayoutProfile.MinimumPlayerActionGap
                - joystickCenter;
            float activationSize = Mathf.Clamp(
                availableActivationHalfWidth * 2f,
                CombatHudCelestialTargetLayoutProfile.MinimumJoystickActivationSize,
                CombatHudCelestialTargetLayoutProfile.JoystickActivation.width);
            RectTransform activationHit = FindDeepChild(transform, "JoystickActivationHit")
                as RectTransform;
            if (activationHit != null)
            {
                activationHit.sizeDelta = new Vector2(activationSize, activationSize);
            }

            float summonActionGap = CombatHudCelestialTargetLayoutProfile.Ultimate.yMin
                - safeAreaInsets.Bottom
                - (CombatHudCelestialTargetLayoutProfile.SummonSlot3.yMax
                    + safeAreaInsets.Top);
            float summonScale = summonActionGap >= 24f ? 1f : 0.96f;
            Transform summonGroup = FindDeepChild(transform, "SummonRailTargetRoot");
            RectTransform summonGroupRect = summonGroup != null
                ? summonGroup.GetComponent<RectTransform>()
                : null;
            if (summonGroupRect != null)
            {
                summonGroupRect.localScale = new Vector3(summonScale, summonScale, 1f);
            }
        }

        private void ApplyCelestialV22ObjectiveFrameRect()
        {
            Transform found = FindDeepChild(transform, "TopLeftPanel");
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return;
            }

            Rect designRect = CombatHudCelestialV2LayoutProfile.ObjectiveFrame;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            // The measured PGR-style facet strip intentionally bleeds to the physical
            // left screen edge. Only its top edge follows the safe area.
            rectTransform.anchoredPosition = new Vector2(0f, -(designRect.yMin + safeAreaInsets.Top));
            rectTransform.sizeDelta = designRect.size;
            rectTransform.localScale = Vector3.one;
        }

        private void ApplyCelestialV22ObjectiveTextRect()
        {
            Transform found = FindDeepChild(transform, "Objective");
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return;
            }

            Rect resolvedRect = CombatHudCelestialV2LayoutProfile.ResolveObjectiveText(
                safeAreaInsets.Left);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(
                resolvedRect.xMin,
                -(resolvedRect.yMin + safeAreaInsets.Top));
            rectTransform.sizeDelta = resolvedRect.size;
            rectTransform.localScale = Vector3.one;
        }

        private void ResetCelestialV22ResponsiveGroups()
        {
            ResetResponsiveGroup("PlayerHudV22Root");
            ResetResponsiveGroup("SummonRailV22Root");
        }

        private void ResetResponsiveGroup(string objectName)
        {
            Transform found = FindDeepChild(transform, objectName);
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private void ApplyCelestialV22CollisionGuards(Vector2 canvasSize)
        {
            float actionLeft = canvasSize.x
                - (DimensionHudDesignWidth - CombatHudCelestialV2LayoutProfile.Dodge.xMin)
                - safeAreaInsets.Right;
            float playerCenter = canvasSize.x * 0.5f
                + CombatHudCelestialV2LayoutProfile.PlayerComposite.center.x
                - DimensionHudDesignWidth * 0.5f;
            float playerHalfWidth = CombatHudCelestialV2LayoutProfile.PlayerComposite.width * 0.5f;
            float unguardedPlayerRight = playerCenter + playerHalfWidth;
            float missingGap = Mathf.Max(
                0f,
                CombatHudCelestialV2LayoutProfile.MinimumPlayerActionGap
                    - (actionLeft - unguardedPlayerRight));
            float playerShift = Mathf.Min(
                CombatHudCelestialV2LayoutProfile.MaximumPlayerLeftShift,
                missingGap);
            float remainingRight = actionLeft
                - CombatHudCelestialV2LayoutProfile.MinimumPlayerActionGap
                - (playerCenter - playerShift);
            float playerScale = Mathf.Clamp(
                remainingRight / Mathf.Max(1f, playerHalfWidth),
                CombatHudCelestialV2LayoutProfile.MinimumPlayerScale,
                1f);

            Transform playerGroup = FindDeepChild(transform, "PlayerHudV22Root");
            RectTransform playerGroupRect = playerGroup != null
                ? playerGroup.GetComponent<RectTransform>()
                : null;
            if (playerGroupRect != null)
            {
                playerGroupRect.anchoredPosition = new Vector2(-playerShift, 0f);
                playerGroupRect.localScale = new Vector3(playerScale, playerScale, 1f);
            }

            float playerLeft = playerCenter - playerShift - playerHalfWidth * playerScale;
            float joystickCenter = safeAreaInsets.Left
                + CombatHudCelestialV2LayoutProfile.JoystickVisual.center.x;
            float availableActivationHalfWidth = playerLeft
                - CombatHudCelestialV2LayoutProfile.MinimumPlayerActionGap
                - joystickCenter;
            float activationSize = Mathf.Clamp(
                availableActivationHalfWidth * 2f,
                CombatHudCelestialV2LayoutProfile.MinimumJoystickActivationSize,
                CombatHudCelestialV2LayoutProfile.JoystickActivation.width);
            RectTransform activationHit = FindDeepChild(transform, "JoystickActivationHit")
                as RectTransform;
            if (activationHit != null)
            {
                activationHit.sizeDelta = new Vector2(activationSize, activationSize);
            }

            float summonActionGap = CombatHudCelestialV2LayoutProfile.Skill.yMin
                - safeAreaInsets.Bottom
                - (CombatHudCelestialV2LayoutProfile.SummonSlot3.yMax + safeAreaInsets.Top);
            float summonScale = summonActionGap >= 24f ? 1f : 0.96f;
            Transform summonGroup = FindDeepChild(transform, "SummonRailV22Root");
            RectTransform summonGroupRect = summonGroup != null
                ? summonGroup.GetComponent<RectTransform>()
                : null;
            if (summonGroupRect != null)
            {
                summonGroupRect.localScale = new Vector3(summonScale, summonScale, 1f);
            }
        }

        private void RefreshSafeAreaLayoutIfNeeded(bool force)
        {
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (!force && Screen.safeArea == lastAppliedSafeArea && screenSize == lastAppliedScreenSize)
            {
                return;
            }

            ApplyResponsiveSideLayout();
        }

        private void ApplyBossHeaderSpacing()
        {
            if (UsesCelestialTargetLayout)
            {
                ApplyCenterDesignRect("BossTargetChassis", CombatHudCelestialTargetLayoutProfile.BossChassis);
                ApplyCenterDesignRect("BossNameArea", CombatHudCelestialTargetLayoutProfile.BossName);
                ApplyCenterDesignRect("BossNameText", CombatHudCelestialTargetLayoutProfile.BossName);
                ApplyCenterDesignRect("BossHpBackground", CombatHudCelestialTargetLayoutProfile.BossHpTrack);
                ApplyCenterDesignRect("BossHpFill", CombatHudCelestialTargetLayoutProfile.BossHpFill);
                ApplyCenterDesignRect("BossHpText", CombatHudCelestialTargetLayoutProfile.BossHpValue);
                ApplyCenterDesignRect("BossCostBackground", CombatHudCelestialTargetLayoutProfile.BossCostTrack);
                ApplyCenterDesignRect("BossCostFill", CombatHudCelestialTargetLayoutProfile.BossCostFill);
                ApplyCenterDesignRect("BossCostText", CombatHudCelestialTargetLayoutProfile.BossCostValue);
                return;
            }

            if (UsesCelestialV22Layout)
            {
                ApplyCenterDesignRect("BossNameArea", CombatHudCelestialV2LayoutProfile.BossName);
                ApplyCenterDesignRect("BossNameText", CombatHudCelestialV2LayoutProfile.BossName);
                ApplyCenterDesignRect("BossHpBackground", CombatHudCelestialV2LayoutProfile.BossHpTrack);
                ApplyCenterDesignRect("BossHpFill", CombatHudCelestialV2LayoutProfile.BossHpFill);
                ApplyCenterDesignRect("BossHpText", CombatHudCelestialV2LayoutProfile.BossHpValue);
                ApplyCenterDesignRect("BossCostBackground", CombatHudCelestialV2LayoutProfile.BossCostTrack);
                ApplyCenterDesignRect("BossCostFill", CombatHudCelestialV2LayoutProfile.BossCostFill);
                ApplyCenterDesignRect("BossCostText", CombatHudCelestialV2LayoutProfile.BossCostValue);
                return;
            }

            ApplyCenterDesignRect("BossNameArea", new Rect(796f, 52f, 1056f, 132f));
            ApplyCenterDesignRect("ActionFeedback", new Rect(850f, 57f, 500f, 46f));
            ApplyCenterDesignRect("BossHpBackground", new Rect(839f, 104f, 913f, 18f));
            ApplyCenterDesignRect("BossHpFill", new Rect(842f, 103f, 741f, 29f));
            ApplyCenterDesignRect("BossCostBackground", new Rect(839f, 147f, 913f, 14f));
            // 821 is the full cost width. At the v19 sample's 64/100 value it renders
            // as 525 design pixels, matching the 343-pixel source fill.
            ApplyCenterDesignRect("BossCostFill", new Rect(842f, 138f, 821f, 13f));
        }

        private void ApplyCenterDesignRect(string objectName, Rect designRect)
        {
            Transform found = FindDeepChild(transform, objectName);
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(
                designRect.xMin + designRect.width * 0.5f - DimensionHudDesignWidth * 0.5f,
                DimensionHudDesignHeight * 0.5f
                    - designRect.yMin
                    - designRect.height * 0.5f
                    - safeAreaInsets.Top);
            rectTransform.sizeDelta = new Vector2(designRect.width, designRect.height);
            rectTransform.localScale = Vector3.one;
        }

        private void DisableDuplicateHudText(string objectName, Text activeText)
        {
            Text[] texts = GetComponentsInChildren<Text>(includeInactive: true);
            for (int i = 0; i < texts.Length; i++)
            {
                Text text = texts[i];
                if (text == null || text == activeText || !string.Equals(text.name, objectName, StringComparison.Ordinal))
                {
                    continue;
                }

                text.text = string.Empty;
                text.gameObject.SetActive(false);
            }
        }

        private void RefreshVirtualJoystickRestPosition()
        {
            Transform found = FindDeepChild(transform, "MoveJoystickRing");
            CombatHudVirtualJoystick joystick = found != null
                ? found.GetComponent<CombatHudVirtualJoystick>()
                : null;
            joystick?.RefreshRestPosition();
        }

        private void ApplyResponsiveDesignRect(string objectName, Rect designRect, ResponsiveHudAnchor anchor)
        {
            Transform found = FindDeepChild(transform, objectName);
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return;
            }

            ApplyResponsiveDesignRect(rectTransform, designRect, anchor);
        }

        private void ApplyResponsiveMissionTimerLayout(Vector2 canvasSize)
        {
            Rect backingDesignRect = UsesCelestialTargetLayout
                ? CombatHudCelestialTargetLayoutProfile.MissionTimerBacking
                : UsesCelestialV22Layout
                    ? CombatHudCelestialV2LayoutProfile.MissionTimerBacking
                    : new Rect(2014f, 47f, 184f, 86f);
            Rect timerDesignRect = UsesCelestialTargetLayout
                ? CombatHudCelestialTargetLayoutProfile.MissionTimerText
                : UsesCelestialV22Layout
                    ? CombatHudCelestialV2LayoutProfile.MissionTimerText
                    : new Rect(2026f, 47f, 160f, 86f);

            const float bossDesignRight = 1852f;
            const float settingsDesignLeft = 2250f;
            const float bossSeparation = 24f;
            const float settingsSeparation = 52f;

            float bossCanvasRight = (canvasSize.x - DimensionHudDesignWidth) * 0.5f + bossDesignRight;
            float settingsCanvasLeft = canvasSize.x
                - (DimensionHudDesignWidth - settingsDesignLeft)
                - safeAreaInsets.Right;
            float groupRight = settingsCanvasLeft - settingsSeparation;
            float availableWidth = Mathf.Max(1f, groupRight - (bossCanvasRight + bossSeparation));
            float fitScale = Mathf.Min(1f, availableWidth / backingDesignRect.width);
            float minimumTimerScale = UsesCelestialTargetLayout
                ? CombatHudCelestialTargetLayoutProfile.MinimumTimerScale
                : CombatHudCelestialV2LayoutProfile.MinimumTimerScale;
            if (UsesReviewedCelestialLayout && fitScale < minimumTimerScale)
            {
                celestialV22TimerFits = false;
                SetCelestialV22TimerVisible(false);
                return;
            }

            if (UsesReviewedCelestialLayout)
            {
                celestialV22TimerFits = true;
                bool timerRequested = UsesCelestialTargetLayout
                    ? celestialTargetTimerVisible
                    : celestialV22TimerRequested;
                SetCelestialV22TimerVisible(timerRequested);
            }

            float groupLeft = groupRight - backingDesignRect.width * fitScale;
            float groupTop = backingDesignRect.yMin + safeAreaInsets.Top;

            ApplyResolvedRightTopRect(
                "MissionTimerBacking",
                ResolveScaledGroupRect(
                    backingDesignRect,
                    backingDesignRect,
                    groupLeft,
                    groupTop,
                    fitScale),
                canvasSize.x);
            RectTransform timerRect = ApplyResolvedRightTopRect(
                "Timer",
                ResolveScaledGroupRect(
                    timerDesignRect,
                    backingDesignRect,
                    groupLeft,
                    groupTop,
                    fitScale),
                canvasSize.x);

            Text timer = timerRect != null ? timerRect.GetComponent<Text>() : null;
            if (timer != null)
            {
                timer.fontSize = Mathf.Max(1, Mathf.RoundToInt(46f * fitScale));
            }
        }

        private void SetCelestialV22TimerVisible(bool visible)
        {
            SetNamedHudObjectActive("MissionTimerBacking", visible);
            SetNamedHudObjectActive("Timer", visible);
        }

        private static Rect ResolveScaledGroupRect(
            Rect designRect,
            Rect groupDesignRect,
            float groupLeft,
            float groupTop,
            float fitScale)
        {
            return new Rect(
                groupLeft + (designRect.xMin - groupDesignRect.xMin) * fitScale,
                groupTop + (designRect.yMin - groupDesignRect.yMin) * fitScale,
                designRect.width * fitScale,
                designRect.height * fitScale);
        }

        private RectTransform ApplyResolvedRightTopRect(
            string objectName,
            Rect resolvedRect,
            float canvasWidth)
        {
            Transform found = FindDeepChild(transform, objectName);
            RectTransform rectTransform = found != null ? found.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                return null;
            }

            rectTransform.anchorMin = Vector2.one;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = Vector2.one;
            rectTransform.anchoredPosition = new Vector2(
                -(canvasWidth - resolvedRect.xMax),
                -resolvedRect.yMin);
            rectTransform.sizeDelta = resolvedRect.size;
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }

        private void ApplyResponsiveDesignRect(
            RectTransform rectTransform,
            Rect designRect,
            ResponsiveHudAnchor anchor)
        {
            float rightInset = DimensionHudDesignWidth - designRect.xMax;
            float bottomInset = DimensionHudDesignHeight - designRect.yMax;
            switch (anchor)
            {
                case ResponsiveHudAnchor.LeftTop:
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    rectTransform.anchoredPosition = new Vector2(
                        designRect.xMin + safeAreaInsets.Left,
                        -(designRect.yMin + safeAreaInsets.Top));
                    break;
                case ResponsiveHudAnchor.LeftBottom:
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);
                    rectTransform.anchoredPosition = new Vector2(
                        designRect.xMin + safeAreaInsets.Left,
                        bottomInset + safeAreaInsets.Bottom);
                    break;
                case ResponsiveHudAnchor.RightTop:
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.pivot = new Vector2(1f, 1f);
                    rectTransform.anchoredPosition = new Vector2(
                        -(rightInset + safeAreaInsets.Right),
                        -(designRect.yMin + safeAreaInsets.Top));
                    break;
                case ResponsiveHudAnchor.RightBottom:
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    rectTransform.anchoredPosition = new Vector2(
                        -(rightInset + safeAreaInsets.Right),
                        bottomInset + safeAreaInsets.Bottom);
                    break;
                case ResponsiveHudAnchor.CenterBottom:
                    rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    rectTransform.anchoredPosition = new Vector2(
                        designRect.center.x - DimensionHudDesignWidth * 0.5f,
                        bottomInset + safeAreaInsets.Bottom);
                    break;
                case ResponsiveHudAnchor.CenterScreen:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = new Vector2(
                        designRect.center.x - DimensionHudDesignWidth * 0.5f,
                        DimensionHudDesignHeight * 0.5f - designRect.center.y);
                    break;
            }

            rectTransform.sizeDelta = new Vector2(designRect.width, designRect.height);
            rectTransform.localScale = Vector3.one;
        }

        private static void ApplyPlayerReadoutStyle(Text text, int fontSize, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = Mathf.Max(text.fontSize, fontSize);
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = false;

            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = ReadoutOutlineColor;
            outline.effectDistance = new Vector2(1.25f, -1.25f);
            outline.useGraphicAlpha = true;
        }

        private static void ApplyExactPlayerReadoutStyle(
            Text text,
            int fontSize,
            Color color,
            TextAnchor alignment)
        {
            ApplyPlayerReadoutStyle(text, fontSize, color);
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.alignment = alignment;
        }

        private bool UsesCelestialV22Layout
        {
            get
            {
                if (celestialV22Layout == null)
                {
                    celestialV22Layout = GetComponent<CombatHudCelestialV2LayoutProfile>();
                }

                return celestialV22Layout != null
                    && celestialV22Layout.Version == CombatHudCelestialV2LayoutProfile.LayoutVersion;
            }
        }

        private bool UsesCelestialTargetLayout
        {
            get
            {
                if (celestialTargetLayout == null)
                {
                    celestialTargetLayout = GetComponent<CombatHudCelestialTargetLayoutProfile>();
                }

                return celestialTargetLayout != null
                    && celestialTargetLayout.Version
                        == CombatHudCelestialTargetLayoutProfile.LayoutVersion;
            }
        }

        private bool UsesReviewedCelestialLayout =>
            UsesCelestialTargetLayout || UsesCelestialV22Layout;

        private static void ApplyHorizontalMeter(Image image, float ratio, ref float baseWidth)
        {
            RectTransform rectTransform = image.rectTransform;
            if (rectTransform != null)
            {
                float currentWidth = Mathf.Abs(rectTransform.rect.width) > 0.01f
                    ? rectTransform.rect.width
                    : rectTransform.sizeDelta.x;
                if (baseWidth <= 0f)
                {
                    baseWidth = Mathf.Max(1f, Mathf.Abs(currentWidth));
                }

                // The Image fill already clips horizontally. Keep the authored full-width
                // rectangle so applying both width scaling and fillAmount cannot square the
                // presented ratio (for example, 50% accidentally becoming 25%).
                if (!Mathf.Approximately(Mathf.Abs(currentWidth), baseWidth))
                {
                    rectTransform.SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Horizontal,
                        baseWidth);
                }
            }

            if (image.type != Image.Type.Filled)
            {
                image.type = Image.Type.Filled;
            }

            if (image.fillMethod != Image.FillMethod.Horizontal)
            {
                image.fillMethod = Image.FillMethod.Horizontal;
            }

            if (image.fillOrigin != (int)Image.OriginHorizontal.Left)
            {
                image.fillOrigin = (int)Image.OriginHorizontal.Left;
            }

            if (!Mathf.Approximately(image.fillAmount, ratio))
            {
                image.fillAmount = ratio;
            }
        }

        private void EnsureAimReticle()
        {
            if (aimReticleRoot == null)
            {
                Transform existing = FindDeepChild(transform, "CenterAimReticle");
                aimReticleRoot = existing != null ? existing.GetComponent<RectTransform>() : null;
            }

            if (aimReticleRoot == null)
            {
                GameObject root = new GameObject("CenterAimReticle", typeof(RectTransform));
                root.transform.SetParent(transform, worldPositionStays: false);
                aimReticleRoot = root.GetComponent<RectTransform>();
                aimReticleRoot.anchorMin = new Vector2(0.5f, 0.5f);
                aimReticleRoot.anchorMax = new Vector2(0.5f, 0.5f);
                aimReticleRoot.pivot = new Vector2(0.5f, 0.5f);
                aimReticleRoot.sizeDelta = new Vector2(96f, 96f);
                aimReticleRoot.anchoredPosition = Vector2.zero;
                aimReticleRoot.SetAsLastSibling();
            }

            Image authoredReticle = aimReticleRoot.GetComponent<Image>();
            if (authoredReticle != null && authoredReticle.sprite != null)
            {
                authoredReticle.raycastTarget = false;
                authoredReticle.preserveAspect = true;
                aimReticleSegments = new[] { authoredReticle };
                return;
            }

            if (aimReticleSegments == null || aimReticleSegments.Length < 4)
            {
                aimReticleSegments = new[]
                {
                    EnsureReticleSegment("Left", new Vector2(-23f, 0f), new Vector2(18f, 2f)),
                    EnsureReticleSegment("Right", new Vector2(23f, 0f), new Vector2(18f, 2f)),
                    EnsureReticleSegment("Top", new Vector2(0f, 23f), new Vector2(2f, 18f)),
                    EnsureReticleSegment("Bottom", new Vector2(0f, -23f), new Vector2(2f, 18f))
                };
            }
        }

        private Image EnsureReticleSegment(string name, Vector2 anchoredPosition, Vector2 size)
        {
            Transform child = aimReticleRoot.Find(name);
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
                child.SetParent(aimReticleRoot, worldPositionStays: false);
            }

            RectTransform rectTransform = child.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = child.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = aimReticleColor;
            return image;
        }

        private ActionSlotBinding FindActionSlot(CombatHudActionId actionId)
        {
            for (int i = 0; i < actionSlots.Length; i++)
            {
                if (actionSlots[i] != null && actionSlots[i].ActionId == actionId)
                {
                    return actionSlots[i];
                }
            }

            return null;
        }

        private SummonSlotBinding FindSummonSlot(CombatHudActionId actionId)
        {
            for (int i = 0; i < summonSlots.Length; i++)
            {
                if (summonSlots[i] != null && summonSlots[i].ActionId == actionId)
                {
                    return summonSlots[i];
                }
            }

            return null;
        }

        private Image FindImage(string objectName)
        {
            Transform found = FindDeepChild(transform, objectName);
            return found != null ? found.GetComponent<Image>() : null;
        }

        private Text FindText(string objectName)
        {
            Transform found = FindDeepChild(transform, objectName);
            return found != null ? found.GetComponent<Text>() : null;
        }

        private static Transform FindDeepChild(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChild(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void SetNamedHudObjectActive(string objectName, bool active)
        {
            Transform found = FindDeepChild(transform, objectName);
            if (found != null)
            {
                SetGameObjectActive(found.gameObject, active);
            }
        }

        private static void SetGameObjectActive(GameObject gameObject, bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        private static void SetText(Text target, string value)
        {
            if (target == null)
            {
                return;
            }

            value ??= string.Empty;
            if (!string.Equals(target.text, value, StringComparison.Ordinal))
            {
                target.text = value;
            }
        }
    }
}
