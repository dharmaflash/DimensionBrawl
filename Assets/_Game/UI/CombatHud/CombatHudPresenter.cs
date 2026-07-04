using System;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudPresenter : MonoBehaviour
    {
        private static readonly Color HealthReadoutColor = new Color(1f, 0.92f, 0.68f, 1f);
        private static readonly Color ResourceReadoutColor = new Color(0.56f, 1f, 1f, 1f);
        private static readonly Color InputModeReadoutColor = new Color(0.9f, 0.98f, 1f, 1f);
        private static readonly Color AmmoReadoutColor = new Color(1f, 0.86f, 0.38f, 1f);
        private static readonly Color ReadoutOutlineColor = new Color(0f, 0.025f, 0.035f, 0.95f);
        private static readonly Color SummonChargingFillColor = new Color(0.08f, 0.86f, 1f, 0.94f);
        private static readonly Color SummonReadyIconColor = new Color(1f, 1f, 1f, 0.98f);
        private static readonly Color SummonUnavailableIconColor = new Color(0.26f, 0.28f, 0.31f, 0.96f);
        private static readonly Color SummonReadyGlowColor = new Color(1f, 0.82f, 0.28f, 0.42f);
        private static readonly Color SummonReadyRingColor = new Color(1f, 0.92f, 0.44f, 0.78f);
        private static readonly Color SummonReadySparkColor = new Color(0.44f, 0.98f, 1f, 0.64f);
        private const float DimensionHudDesignWidth = 2560f;
        private const float DimensionHudDesignHeight = 1440f;

        private enum ResponsiveHudAnchor
        {
            LeftTop,
            LeftBottom,
            RightTop,
            RightBottom
        }

        [Serializable]
        public sealed class ActionSlotBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private Text labelText;
            [SerializeField] private Text cooldownText;
            [SerializeField] private Image cooldownFill;
            [SerializeField] private CanvasGroup canvasGroup;

            public CombatHudActionId ActionId => actionId;

            public void SetCooldown(float normalizedRemaining, string label, float secondsRemaining = -1f)
            {
                if (labelText != null && !string.IsNullOrWhiteSpace(label))
                {
                    labelText.text = label;
                }

                float clamped = Mathf.Clamp01(normalizedRemaining);
                if (cooldownFill != null)
                {
                    cooldownFill.fillAmount = clamped;
                }

                if (cooldownText != null)
                {
                    float displaySeconds = secondsRemaining >= 0f ? secondsRemaining : Mathf.CeilToInt(clamped * 10f) / 10f;
                    cooldownText.text = clamped > 0f ? $"{displaySeconds:0.0}s" : string.Empty;
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = clamped > 0f ? 0.65f : 1f;
                }
            }

            public void ApplyGuideFocus(bool focused, bool dimUnfocused)
            {
                if (canvasGroup == null)
                {
                    return;
                }

                if (focused)
                {
                    canvasGroup.alpha = Mathf.Max(canvasGroup.alpha, 1f);
                }
                else if (dimUnfocused)
                {
                    canvasGroup.alpha = Mathf.Min(canvasGroup.alpha, 0.42f);
                }
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

            public CombatHudActionId ActionId => actionId;

            public void SetVisible(bool visible)
            {
                ResolveStateVisuals();
                if (labelText != null)
                {
                    labelText.gameObject.SetActive(visible);
                }

                if (stateText != null)
                {
                    stateText.gameObject.SetActive(visible);
                }

                if (cooldownFill != null)
                {
                    cooldownFill.gameObject.SetActive(visible);
                }

                SetImageObjectVisible(iconImage, visible);
                SetImageObjectVisible(unavailableIconImage, visible);
                SetImageObjectVisible(readyGlowImage, false);
                SetImageObjectVisible(readyRingImage, false);
                SetImageObjectVisible(readySparkImage, false);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = visible ? canvasGroup.alpha : 0f;
                    canvasGroup.interactable = visible && canvasGroup.interactable;
                    canvasGroup.blocksRaycasts = visible && canvasGroup.blocksRaycasts;
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
                if (labelText != null)
                {
                    labelText.text = label;
                    labelText.fontStyle = FontStyle.Bold;
                    labelText.color = HealthReadoutColor;
                    ApplySlotTextOutline(labelText);
                }

                if (stateText != null)
                {
                    stateText.text = state;
                    stateText.fontSize = Mathf.Max(stateText.fontSize, 20);
                    stateText.fontStyle = FontStyle.Bold;
                    stateText.color = enabled ? HealthReadoutColor : InputModeReadoutColor;
                    stateText.alignment = TextAnchor.MiddleCenter;
                    stateText.lineSpacing = 0.86f;
                    stateText.resizeTextForBestFit = true;
                    stateText.resizeTextMinSize = 14;
                    stateText.resizeTextMaxSize = stateText.fontSize;
                    stateText.horizontalOverflow = HorizontalWrapMode.Wrap;
                    stateText.verticalOverflow = VerticalWrapMode.Overflow;
                    ApplySlotTextOutline(stateText);
                }

                if (cooldownFill != null)
                {
                    ConfigureClockwiseSummonFill(cooldownFill, enabled, availabilityFill01);
                }

                ApplyIconReadiness(enabled, availabilityFill01);
                ApplyReadyEffect(enabled);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = enabled ? 1f : 0.88f;
                    canvasGroup.interactable = enabled;
                    canvasGroup.blocksRaycasts = enabled;
                }
            }

            private void ResolveStateVisuals()
            {
                Transform root = ResolveSlotRoot();
                if (root == null)
                {
                    return;
                }

                iconImage ??= FindChildImage(root, "Icon");
                unavailableIconImage ??= FindChildImage(root, "IconDisabled");
                readyGlowImage ??= FindChildImage(root, "ReadyGlow");
                readyRingImage ??= FindChildImage(root, "ReadyRing");
                readySparkImage ??= FindChildImage(root, "ReadySparkRing");
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
                    iconImage.gameObject.SetActive(true);
                    iconImage.color = ready ? SummonReadyIconColor : new Color(0.94f, 0.97f, 1f, 0.96f);
                }

                if (unavailableIconImage != null)
                {
                    bool showWipe = !ready && fill < 0.999f;
                    unavailableIconImage.gameObject.SetActive(showWipe);
                    unavailableIconImage.raycastTarget = false;
                    unavailableIconImage.preserveAspect = true;
                    unavailableIconImage.type = Image.Type.Filled;
                    unavailableIconImage.fillMethod = Image.FillMethod.Radial360;
                    unavailableIconImage.fillOrigin = (int)Image.Origin360.Top;
                    unavailableIconImage.fillClockwise = false;
                    unavailableIconImage.fillAmount = showWipe ? 1f - fill : 0f;
                    Color color = SummonUnavailableIconColor;
                    color.a = showWipe ? Mathf.Lerp(0.92f, 0.78f, fill) : 0f;
                    unavailableIconImage.color = color;
                }
            }

            private void ApplyReadyEffect(bool ready)
            {
                float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 5.8f) * 0.5f;
                ApplyReadyImage(readyGlowImage, ready, SummonReadyGlowColor, 0.18f + pulse * 0.18f);
                ApplyReadyImage(readyRingImage, ready, SummonReadyRingColor, 0.56f + pulse * 0.24f);
                ApplyReadyImage(readySparkImage, ready, SummonReadySparkColor, 0.34f + pulse * 0.34f);

                if (readyRingImage != null)
                {
                    readyRingImage.rectTransform.localScale = Vector3.one * (1f + pulse * 0.045f);
                }

                if (readySparkImage != null)
                {
                    readySparkImage.rectTransform.localRotation =
                        Quaternion.Euler(0f, 0f, -Time.unscaledTime * 82f);
                    readySparkImage.rectTransform.localScale = Vector3.one * (0.98f + pulse * 0.035f);
                }
            }

            private static void ApplyReadyImage(Image image, bool visible, Color baseColor, float alpha)
            {
                if (image == null)
                {
                    return;
                }

                image.gameObject.SetActive(visible);
                Color color = baseColor;
                color.a = visible ? Mathf.Clamp01(alpha) : 0f;
                image.color = color;
                image.raycastTarget = false;
            }

            private static void SetImageObjectVisible(Image image, bool visible)
            {
                if (image != null)
                {
                    image.gameObject.SetActive(visible);
                }
            }

            private static Image FindChildImage(Transform root, string objectName)
            {
                Transform found = FindDeepChild(root, objectName);
                return found != null ? found.GetComponent<Image>() : null;
            }

            public void ApplyGuideFocus(bool focused, bool dimUnfocused)
            {
                if (canvasGroup == null)
                {
                    return;
                }

                if (focused)
                {
                    canvasGroup.alpha = Mathf.Max(canvasGroup.alpha, 1f);
                }
                else if (dimUnfocused)
                {
                    canvasGroup.alpha = Mathf.Min(canvasGroup.alpha, 0.48f);
                }
            }

            private static void ConfigureClockwiseSummonFill(Image image, bool enabled, float availabilityFill01)
            {
                float fill = Mathf.Clamp01(availabilityFill01);
                bool showFill = !enabled && fill > 0.001f;
                image.raycastTarget = false;
                image.preserveAspect = false;
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Radial360;
                image.fillOrigin = (int)Image.Origin360.Top;
                image.fillClockwise = true;
                image.fillAmount = showFill ? fill : 0f;
                Color color = SummonChargingFillColor;
                color.a = showFill ? Mathf.Lerp(0.70f, 0.96f, fill) : 0f;
                image.color = color;
                image.gameObject.SetActive(showFill);
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
        [SerializeField] private Text bossHealthText;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private Image bossResourceFill;
        [SerializeField] private RectTransform aimReticleRoot;
        [SerializeField] private Image[] aimReticleSegments = Array.Empty<Image>();
        [SerializeField] private Color aimReticleColor = new Color(0.82f, 0.96f, 1f, 0.88f);
        [SerializeField] private Color aimReticleActiveColor = new Color(0.42f, 0.95f, 1f, 0.96f);
        [SerializeField] private CombatHudActionCatalog actionCatalog;
        [SerializeField] private ActionSlotBinding[] actionSlots = Array.Empty<ActionSlotBinding>();
        [SerializeField] private SummonSlotBinding[] summonSlots = Array.Empty<SummonSlotBinding>();

        public float BossHealthFillAmount => bossHealthFill != null ? bossHealthFill.fillAmount : 0f;
        public float BossResourceFillAmount => bossResourceFill != null ? bossResourceFill.fillAmount : 0f;
        public bool AimReticleVisible => aimReticleRoot != null && aimReticleRoot.gameObject.activeInHierarchy;

        private void Awake()
        {
            ResolveOptionalRuntimeReferences();
            ApplyPlayerReadoutStyles();
            ApplyResponsiveSideLayout();
            EnsureAimReticle();
        }

        public void SetObjective(string objective)
        {
            SetText(objectiveText, objective);
        }

        public void SetTimer(float secondsRemaining)
        {
            float clamped = Mathf.Max(0f, secondsRemaining);
            int minutes = Mathf.FloorToInt(clamped / 60f);
            int seconds = Mathf.FloorToInt(clamped % 60f);
            SetText(timerText, $"{minutes:00}:{seconds:00}");
        }

        public void SetHealth(float current, float max)
        {
            ApplyPlayerReadoutStyle(healthText, 19, HealthReadoutColor);
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (healthFill != null)
            {
                healthFill.fillAmount = ratio;
            }

            SetText(healthText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetBossHealth(float current, float max)
        {
            ResolveOptionalRuntimeReferences();
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (bossHealthFill != null)
            {
                bossHealthFill.fillAmount = ratio;
            }

            SetText(bossHealthText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetBossResource(float current, float max)
        {
            ResolveOptionalRuntimeReferences();
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (bossResourceFill != null)
            {
                bossResourceFill.fillAmount = ratio;
            }
        }

        public void SetAimReticleVisible(bool visible, bool active)
        {
            EnsureAimReticle();
            if (aimReticleRoot == null)
            {
                return;
            }

            aimReticleRoot.gameObject.SetActive(visible);
            Color color = active ? aimReticleActiveColor : aimReticleColor;
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
            ApplyPlayerReadoutStyle(resourceText, 19, ResourceReadoutColor);
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (resourceFill != null)
            {
                resourceFill.fillAmount = ratio;
            }

            SetText(resourceText, $"{Mathf.CeilToInt(Mathf.Max(0f, current))}/{Mathf.CeilToInt(Mathf.Max(0f, max))}");
        }

        public void SetInputMode(string label)
        {
            ApplyPlayerReadoutStyle(inputModeText, 15, InputModeReadoutColor);
            SetText(inputModeText, label);
        }

        public void SetAmmo(string label, bool reloading)
        {
            ResolveOptionalRuntimeReferences();
            if (ammoText == null)
            {
                return;
            }

            bool visible = !string.IsNullOrWhiteSpace(label);
            ammoText.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            int fontSize = reloading ? 16 : 18;
            ApplyPlayerReadoutStyle(ammoText, fontSize, reloading ? InputModeReadoutColor : AmmoReadoutColor);
            ammoText.fontSize = fontSize;
            SetText(ammoText, label);
        }

        public void SetSkillCooldown(CombatHudActionId actionId, float normalizedRemaining, string label, float secondsRemaining = -1f)
        {
            ActionSlotBinding slot = FindActionSlot(actionId);
            slot?.SetCooldown(normalizedRemaining, label, secondsRemaining);
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

        public void SetGuideFocus(CombatHudActionId focusAction, bool dimUnfocused)
        {
            bool shouldDim = dimUnfocused && focusAction != CombatHudActionId.None;
            for (int i = 0; i < actionSlots.Length; i++)
            {
                ActionSlotBinding slot = actionSlots[i];
                slot?.ApplyGuideFocus(slot.ActionId == focusAction, shouldDim);
            }

            for (int i = 0; i < summonSlots.Length; i++)
            {
                SummonSlotBinding slot = summonSlots[i];
                slot?.ApplyGuideFocus(slot.ActionId == focusAction, shouldDim);
            }
        }

        private void ResolveOptionalRuntimeReferences()
        {
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

            if (ammoText == null)
            {
                ammoText = FindText("AmmoText");
            }
        }

        private void ApplyPlayerReadoutStyles()
        {
            ApplyPlayerReadoutStyle(healthText, 19, HealthReadoutColor);
            ApplyPlayerReadoutStyle(resourceText, 19, ResourceReadoutColor);
            ApplyPlayerReadoutStyle(inputModeText, 15, InputModeReadoutColor);
            ApplyPlayerReadoutStyle(ammoText, 18, AmmoReadoutColor);
        }

        private void ApplyResponsiveSideLayout()
        {
            ApplyResponsiveDesignRect("TopLeftPanel", new Rect(45f, 36f, 571f, 165f), ResponsiveHudAnchor.LeftTop);
            ApplyResponsiveDesignRect("Timer", new Rect(178f, 55f, 409f, 48f), ResponsiveHudAnchor.LeftTop);
            ApplyResponsiveDesignRect("Objective", new Rect(180f, 117f, 409f, 64f), ResponsiveHudAnchor.LeftTop);
            ApplyResponsiveDesignRect("SettingsButton", new Rect(2250f, 47f, 100f, 95f), ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect("PauseButton", new Rect(2396f, 47f, 100f, 95f), ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect("MoveJoystickRing", new Rect(155f, 853f, 421f, 415f), ResponsiveHudAnchor.LeftBottom);
            ApplyResponsiveDesignRect("MoveJoystickKnob", new Rect(303f, 1004f, 122f, 121f), ResponsiveHudAnchor.LeftBottom);
            ApplyResponsiveDesignRect("BasicAttackButton", new Rect(2239f, 1156f, 230f, 248f), ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect("DodgeButton", new Rect(1975f, 1172f, 256f, 218f), ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect("Skill1Button", new Rect(2217f, 868f, 236f, 286f), ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect("UltimateButton", new Rect(1975f, 896f, 248f, 226f), ResponsiveHudAnchor.RightBottom);
            ApplyResponsiveDesignRect("SummonSlot1Button", new Rect(2293f, 235f, 211f, 216f), ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect("SummonSlot2Button", new Rect(2308f, 472f, 182f, 186f), ResponsiveHudAnchor.RightTop);
            ApplyResponsiveDesignRect("SummonSlot3Button", new Rect(2312f, 683f, 179f, 183f), ResponsiveHudAnchor.RightTop);
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

        private static void ApplyResponsiveDesignRect(
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
                    rectTransform.anchoredPosition = new Vector2(designRect.xMin, -designRect.yMin);
                    break;
                case ResponsiveHudAnchor.LeftBottom:
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);
                    rectTransform.anchoredPosition = new Vector2(designRect.xMin, bottomInset);
                    break;
                case ResponsiveHudAnchor.RightTop:
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.pivot = new Vector2(1f, 1f);
                    rectTransform.anchoredPosition = new Vector2(-rightInset, -designRect.yMin);
                    break;
                case ResponsiveHudAnchor.RightBottom:
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    rectTransform.anchoredPosition = new Vector2(-rightInset, bottomInset);
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

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
