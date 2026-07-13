using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IsekaiBrawl.Gameplay
{
    public class CardSlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const int NoPointerId = int.MinValue;

        [SerializeField] private Image cardImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Image glowImage;
        [SerializeField] private Button buttonComponent;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private Color emptyTint = new(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color filledTint = Color.white;
        [SerializeField] private Color readyTextColor = Color.white;
        [SerializeField] private Color blockedTextColor = new(1f, 0.78f, 0.78f, 1f);
        [SerializeField] private Color readyGlowColor = new(0.4f, 1f, 0.78f, 0.32f);
        [SerializeField] private Color blockedGlowColor = new(0.82f, 0.32f, 0.32f, 0.16f);
        [SerializeField] private Color hintTextColor = new(0.82f, 0.9f, 1f, 0.9f);
        [SerializeField] private Color recommendedHintColor = new(0.96f, 0.92f, 0.54f, 1f);

        private Action onClick;
        private Action<int> onLanePlay;
        private SummonData summonData;
        private bool isInteractableVisual;
        private bool isRecommendedChoice;
        private Vector3 baseScale = Vector3.one;
        private float pulseOffset;
        private float displayedEnergy;
        private string tacticalHint = string.Empty;
        private bool usesMobileLayout;
        private bool usesCompactMobileLayout;
        private bool isDraggingForLanePlacement;
        private int activePointerId = NoPointerId;
        private float layoutFitScale = 1f;
        private Vector2 currentBaseSize = new(126f, 138f);

        public static CardSlotUI CreateRuntimeInstance(Transform parent)
        {
            GameObject root = new("CardSlotUI", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(CardSlotUI));
            root.transform.SetParent(parent, false);

            RectTransform rootRect = root.GetComponent<RectTransform>();
            float canvasScale = RuntimeCanvasLayoutUtility.ResolveSoftScale(rootRect, 0.4f, 1.46f);
            rootRect.sizeDelta = new Vector2(126f, 138f) * canvasScale;

            Sprite builtinSprite = RuntimeUISpriteUtility.GetPanelSprite();

            Image backgroundImage = root.GetComponent<Image>();
            backgroundImage.sprite = builtinSprite;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = new Color(0.14f, 0.15f, 0.18f, 0.74f);

            CardSlotUI slotUi = root.GetComponent<CardSlotUI>();
            slotUi.buttonComponent = root.GetComponent<Button>();
            slotUi.canvasGroup = root.GetComponent<CanvasGroup>();
            slotUi.fallbackSprite = builtinSprite;
            slotUi.EnsureVisualChildren();
            return slotUi;
        }

        private void Awake()
        {
            EnsureVisualChildren();
            baseScale = (transform as RectTransform) != null ? transform.localScale : Vector3.one;
            pulseOffset = UnityEngine.Random.Range(0f, 10f);
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            CancelActiveDrag();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveListener(HandleClick);
            }
        }

        private void OnDisable()
        {
            CancelActiveDrag();
        }

        private void Update()
        {
            if (glowImage == null)
            {
                return;
            }

            if (summonData == null)
            {
                glowImage.enabled = false;
                transform.localScale = Vector3.Lerp(transform.localScale, baseScale, 12f * Time.deltaTime);
                return;
            }

            glowImage.enabled = true;
            Color targetColor = isInteractableVisual ? readyGlowColor : blockedGlowColor;
            if (isRecommendedChoice)
            {
                targetColor = Color.Lerp(targetColor, recommendedHintColor, 0.68f);
            }

            float alphaPulse = isInteractableVisual
                ? 0.72f + (Mathf.Sin((Time.unscaledTime * 5.2f) + pulseOffset) * (isRecommendedChoice ? 0.24f : 0.18f))
                : isRecommendedChoice ? 0.72f : 0.55f;

            targetColor.a *= alphaPulse;
            glowImage.color = Color.Lerp(glowImage.color, targetColor, 12f * Time.deltaTime);

            Vector3 targetScale = isInteractableVisual
                ? baseScale * (1f + (Mathf.Sin((Time.unscaledTime * 4.2f) + pulseOffset) * (isRecommendedChoice ? 0.03f : 0.018f)))
                : isRecommendedChoice ? baseScale * 1.02f : baseScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 10f * Time.deltaTime);
        }

        public void Init(SummonData data, Action onClickCallback, Action<int> onLanePlayCallback = null)
        {
            CancelActiveDrag();
            EnsureVisualChildren();
            summonData = data;
            onClick = onClickCallback;
            onLanePlay = onLanePlayCallback;
            tacticalHint = string.Empty;
            isRecommendedChoice = false;
            isDraggingForLanePlacement = false;

            if (cardImage != null)
            {
                cardImage.sprite = data != null && data.cardSprite != null ? data.cardSprite : fallbackSprite;
                cardImage.color = ResolveCardColor(data);
            }

            if (nameText != null)
            {
                nameText.text = data != null ? SummonPresentationUtility.GetShortLabel(data) : string.Empty;
            }

            if (costText != null)
            {
                costText.text = data != null ? Mathf.RoundToInt(data.energyCost).ToString() : string.Empty;
            }

            if (glowImage != null)
            {
                glowImage.enabled = data != null;
                glowImage.color = Color.clear;
            }

            RefreshTextState();
        }

        public void SetEmpty()
        {
            Init(null, null);
            SetInteractable(false, 0f);
        }

        public void SetInteractable(bool canAfford, float currentEnergy)
        {
            bool isUsable = summonData != null && canAfford;
            displayedEnergy = currentEnergy;

            if (buttonComponent != null)
            {
                buttonComponent.interactable = isUsable;
            }

            isInteractableVisual = isUsable;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = isUsable ? 1f : summonData == null ? 0.2f : 0.74f;
            }

            RefreshTextState();
        }

        public void SetTacticalHint(string hint, bool isRecommended)
        {
            tacticalHint = hint ?? string.Empty;
            isRecommendedChoice = summonData != null && isRecommended;
            RefreshTextState();
        }

        public Vector2 GetCurrentBaseSize()
        {
            return currentBaseSize;
        }

        public void SetLayoutFitScale(float scale)
        {
            float clampedScale = Mathf.Clamp(scale, 0.64f, 1f);
            if (Mathf.Abs(layoutFitScale - clampedScale) <= 0.01f)
            {
                return;
            }

            layoutFitScale = clampedScale;
            ApplyResponsiveLayout(usesMobileLayout, usesCompactMobileLayout, true);
        }

        public void ApplyResponsiveLayout(bool useMobileLayout, bool useCompactMobileLayout, bool forceRefresh = false)
        {
            EnsureVisualChildren();
            if (!forceRefresh && usesMobileLayout == useMobileLayout && usesCompactMobileLayout == useCompactMobileLayout)
            {
                return;
            }

            usesMobileLayout = useMobileLayout;
            usesCompactMobileLayout = useCompactMobileLayout;

            RectTransform rect = transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            float mobileWidth = Screen.width > 0 ? Screen.width : rect.rect.width;
            float widthBlend = useMobileLayout ? Mathf.Clamp01((mobileWidth - 360f) / 280f) : 1f;
            float canvasScale = RuntimeCanvasLayoutUtility.ResolveSoftScale(rect, 0.4f, 1.46f);
            Vector2 rootSize = useMobileLayout
                ? Vector2.Lerp(new Vector2(118f, 132f), new Vector2(132f, 146f), widthBlend)
                : new Vector2(126f, 138f);
            rootSize *= canvasScale;
            currentBaseSize = rootSize;
            rootSize *= layoutFitScale;
            rect.sizeDelta = rootSize;

            Vector2 artSize = useMobileLayout
                ? Vector2.Lerp(new Vector2(86f, 52f), new Vector2(98f, 62f), widthBlend)
                : new Vector2(94f, 58f);
            Vector2 artOffset = useMobileLayout
                ? Vector2.Lerp(new Vector2(0f, 6f), new Vector2(0f, 8f), widthBlend)
                : new Vector2(0f, 7f);
            artSize *= canvasScale * layoutFitScale;
            artOffset *= canvasScale * layoutFitScale;
            ApplyImageLayout(cardImage, artSize, artOffset);
            ApplyImageLayout(glowImage, useMobileLayout ? new Vector2(rootSize.x - (10f * canvasScale), rootSize.y - (10f * canvasScale)) : new Vector2(102f, 106f) * canvasScale, Vector2.zero);

            float textScale = canvasScale * Mathf.Lerp(0.98f, 1.04f, layoutFitScale);
            float nameFont = (useMobileLayout ? Mathf.Lerp(15.8f, 17.6f, widthBlend) : 16.2f) * textScale;
            float costFont = (useMobileLayout ? Mathf.Lerp(20.5f, 23.5f, widthBlend) : 21.5f) * textScale;
            float detailFont = (useMobileLayout ? Mathf.Lerp(11.6f, 12.6f, widthBlend) : 12.2f) * textScale;
            float hintFont = (useMobileLayout ? Mathf.Lerp(10.7f, 11.6f, widthBlend) : 11.2f) * textScale;
            float titleY = (useMobileLayout ? Mathf.Lerp(40f, 46f, widthBlend) : 42f) * canvasScale * layoutFitScale;
            float costX = (useMobileLayout ? Mathf.Lerp(-23f, -27f, widthBlend) : -24f) * canvasScale * layoutFitScale;
            float costY = (useMobileLayout ? Mathf.Lerp(-39f, -44f, widthBlend) : -40f) * canvasScale * layoutFitScale;
            float detailY = (useMobileLayout ? Mathf.Lerp(-34f, -39f, widthBlend) : -38f) * canvasScale * layoutFitScale;
            float hintY = (useMobileLayout ? Mathf.Lerp(-49f, -54f, widthBlend) : -56f) * canvasScale * layoutFitScale;
            float hintHeight = (useMobileLayout ? Mathf.Lerp(20f, 24f, widthBlend) : 26f) * canvasScale * layoutFitScale;
            ApplyTextLayout(nameText, new Vector2(rootSize.x - (16f * canvasScale), 22f * canvasScale), new Vector2(0f, titleY), nameFont, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
            ApplyTextLayout(costText, new Vector2(52f, 24f) * canvasScale, new Vector2(costX, costY), costFont, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
            ApplyTextLayout(detailText, new Vector2(rootSize.x - (14f * canvasScale), 18f * canvasScale), new Vector2(0f, detailY), detailFont, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
            ApplyTextLayout(hintText, new Vector2(rootSize.x - (12f * canvasScale), hintHeight), new Vector2(0f, hintY), hintFont, TextAlignmentOptions.Center, TextWrappingModes.Normal);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        private void HandleClick()
        {
            if (onClick == null)
            {
                return;
            }

            if (CardHandTouchCoordinator.ShouldSuppressClicks())
            {
                return;
            }

            transform.localScale = baseScale * 0.93f;
            onClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null
                || eventData.button != PointerEventData.InputButton.Left
                || activePointerId != NoPointerId)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            isDraggingForLanePlacement = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsActivePointer(eventData) && !isDraggingForLanePlacement)
            {
                activePointerId = NoPointerId;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsActivePointer(eventData) || !CanBeginLanePlacement())
            {
                return;
            }

            isDraggingForLanePlacement = MobileBattleControls.BeginSummonPlacement(eventData.position);
            if (isDraggingForLanePlacement)
            {
                CardHandTouchCoordinator.SuppressClicks(0.18f);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsActivePointer(eventData) || !isDraggingForLanePlacement)
            {
                return;
            }

            MobileBattleControls.UpdateSummonPlacement(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsActivePointer(eventData))
            {
                return;
            }

            activePointerId = NoPointerId;
            if (!isDraggingForLanePlacement)
            {
                return;
            }

            isDraggingForLanePlacement = false;
            CardHandTouchCoordinator.SuppressClicks(0.18f);
            if (MobileBattleControls.TryCompleteSummonPlacement(eventData.position, out int laneIndex))
            {
                transform.localScale = baseScale * 0.93f;
                onLanePlay?.Invoke(laneIndex);
                return;
            }

            MobileBattleControls.CancelSummonPlacement();
        }

        private bool CanBeginLanePlacement()
        {
            return summonData != null && isInteractableVisual && onLanePlay != null;
        }

        private bool IsActivePointer(PointerEventData eventData)
        {
            return eventData != null && eventData.pointerId == activePointerId;
        }

        private void CancelActiveDrag()
        {
            activePointerId = NoPointerId;
            if (!isDraggingForLanePlacement)
            {
                return;
            }

            isDraggingForLanePlacement = false;
            MobileBattleControls.CancelSummonPlacement();
        }

        private void EnsureVisualChildren()
        {
            if (buttonComponent == null)
            {
                buttonComponent = GetComponent<Button>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            RectTransform rect = transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            float canvasScale = RuntimeCanvasLayoutUtility.ResolveSoftScale(rect, 0.4f, 1.46f);

            if (cardImage == null)
            {
                cardImage = EnsureImageChild(rect, "CardArt", new Vector2(94f, 58f) * canvasScale, new Vector2(0f, 7f) * canvasScale);
            }

            if (glowImage == null)
            {
                glowImage = EnsureImageChild(rect, "GlowImage", new Vector2(108f, 112f) * canvasScale, Vector2.zero);
                glowImage.transform.SetAsFirstSibling();
                glowImage.color = Color.clear;
            }

            if (nameText == null)
            {
                nameText = EnsureTextChild(rect, "NameText", new Vector2(106f, 22f) * canvasScale, new Vector2(0f, 42f) * canvasScale, 15.5f * canvasScale);
            }

            if (costText == null)
            {
                costText = EnsureTextChild(rect, "CostText", new Vector2(52f, 24f) * canvasScale, new Vector2(-24f, -40f) * canvasScale, 20.5f * canvasScale);
            }

            if (detailText == null)
            {
                detailText = EnsureTextChild(rect, "DetailText", new Vector2(110f, 18f) * canvasScale, new Vector2(0f, -38f) * canvasScale, 11.75f * canvasScale);
                detailText.alignment = TextAlignmentOptions.Center;
            }

            if (hintText == null)
            {
                hintText = EnsureTextChild(rect, "HintText", new Vector2(114f, 26f) * canvasScale, new Vector2(0f, -56f) * canvasScale, 10.75f * canvasScale);
                hintText.alignment = TextAlignmentOptions.Center;
                hintText.textWrappingMode = TextWrappingModes.Normal;
            }

            RuntimeUIFontUtility.ApplyToText(nameText);
            RuntimeUIFontUtility.ApplyToText(costText);
            RuntimeUIFontUtility.ApplyToText(detailText);
            RuntimeUIFontUtility.ApplyToText(hintText);
        }

        private static Image EnsureImageChild(RectTransform parent, string name, Vector2 size, Vector2 anchoredPosition)
        {
            Transform existing = parent.Find(name);
            GameObject childObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            childObject.transform.SetParent(parent, false);

            RectTransform rect = childObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = childObject.GetComponent<Image>();
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text EnsureTextChild(RectTransform parent, string name, Vector2 size, Vector2 anchoredPosition, float fontSize)
        {
            Transform existing = parent.Find(name);
            GameObject childObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            childObject.transform.SetParent(parent, false);

            RectTransform rect = childObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI text = childObject.GetComponent<TextMeshProUGUI>();
            text.font = RuntimeUIFontUtility.EnsureKoreanFallback() ?? TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            RuntimeUIFontUtility.ApplyToText(text);
            return text;
        }

        private static void ApplyImageLayout(Image image, Vector2 size, Vector2 anchoredPosition)
        {
            if (image == null)
            {
                return;
            }

            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void ApplyTextLayout(TMP_Text text, Vector2 size, Vector2 anchoredPosition, float fontSize, TextAlignmentOptions alignment, TextWrappingModes wrappingMode)
        {
            if (text == null)
            {
                return;
            }

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.textWrappingMode = wrappingMode;
        }

        private Color ResolveCardColor(SummonData data)
        {
            if (data == null)
            {
                return emptyTint;
            }

            return data.cardSprite != null ? filledTint : SummonPresentationUtility.GetCardColor(data);
        }

        private void RefreshTextState()
        {
            if (detailText != null)
            {
                detailText.text = summonData != null ? SummonPresentationUtility.GetRoleLabel(summonData) : string.Empty;
                detailText.color = summonData == null
                    ? readyTextColor
                    : isRecommendedChoice ? recommendedHintColor : readyTextColor;
            }

            if (hintText != null)
            {
                hintText.text = usesMobileLayout ? ResolveCompactHintText() : ResolveHintText();
                hintText.color = summonData == null
                    ? readyTextColor
                    : isRecommendedChoice ? recommendedHintColor : isInteractableVisual ? hintTextColor : blockedTextColor;
            }

            if (nameText != null)
            {
                nameText.color = summonData == null
                    ? readyTextColor
                    : isRecommendedChoice ? recommendedHintColor : readyTextColor;
            }
        }

        private string ResolveHintText()
        {
            if (summonData == null)
            {
                return string.Empty;
            }

            if (!isInteractableVisual)
            {
                float shortage = Mathf.Max(0f, summonData.energyCost - displayedEnergy);
                if (isRecommendedChoice)
                {
                    return shortage > 0.01f ? $"\uCD94\uCC9C +{Mathf.CeilToInt(shortage)}E" : "\uCD94\uCC9C";
                }

                return shortage > 0.01f ? $"+{Mathf.CeilToInt(shortage)}E" : "\uC800\uC7A5";
            }

            if (string.IsNullOrWhiteSpace(tacticalHint))
            {
                return isRecommendedChoice ? "\uCD94\uCC9C" : string.Empty;
            }

            return isRecommendedChoice ? $"\uCD94\uCC9C {tacticalHint}" : tacticalHint;
        }

        private string ResolveCompactHintText()
        {
            if (summonData == null)
            {
                return string.Empty;
            }

            if (!isInteractableVisual)
            {
                float shortage = Mathf.Max(0f, summonData.energyCost - displayedEnergy);
                if (shortage > 0.01f)
                {
                    return $"+{Mathf.CeilToInt(shortage)}E";
                }

                return isRecommendedChoice ? "\uCD94\uCC9C" : string.Empty;
            }

            if (isRecommendedChoice)
            {
                return string.IsNullOrWhiteSpace(tacticalHint) ? "\uCD94\uCC9C" : tacticalHint;
            }

            return string.Empty;
        }
    }
}
