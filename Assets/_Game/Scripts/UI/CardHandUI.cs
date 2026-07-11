using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IsekaiBrawl.Gameplay
{
    public class CardHandUI : MonoBehaviour
    {
        private readonly struct CardAdvice
        {
            public CardAdvice(float score, string hint)
            {
                Score = score;
                Hint = hint;
            }

            public float Score { get; }
            public string Hint { get; }
        }

        [SerializeField] private List<SummonData> currentHand = new();
        [SerializeField] private CardSlotUI slotPrefab;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private int maxSlots = 7;
        [SerializeField] private int activeHandSize = 5;
        [SerializeField] private bool useProgressiveUnlock = false;
        [SerializeField] private int openingUnlockedSlots = 3;
        [SerializeField] private float unlockInterval = 28f;
        [SerializeField] private SummonSpawner summonSpawner;
        [SerializeField] private ScrollRect handScrollRect;
        [SerializeField] private RectTransform handViewport;
        [SerializeField] private HorizontalLayoutGroup slotLayoutGroup;
        [SerializeField] private float mobileScrollClickSuppressionDuration = 0.16f;
        [SerializeField, Range(2f, 30f)] private float tacticalAdviceRefreshRate = 10f;

        private readonly List<CardSlotUI> activeSlots = new();
        private readonly List<SummonData> curatedDrawDeck = new();
        private CardAdvice[] adviceBuffer = System.Array.Empty<CardAdvice>();
        private float nextTacticalAdviceRefreshTime;
        private bool isSubscribedToEnergy;
        private int nextDeckIndex;
        private int currentUnlockedSlots;
        private float unlockElapsedTime;
        private EnemyAI enemyAI;
        private BattleManager battleManager;
        private PlayerSkillController playerSkillController;
        private PlayerController playerController;
        private bool isMobileLayoutActive;
        private bool isCompactMobileLayout;
        private bool mobileScrollAlignedToRight;

        private void OnEnable()
        {
            nextTacticalAdviceRefreshTime = 0f;
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (BattleEnergySystem.Instance != null && isSubscribedToEnergy)
            {
                BattleEnergySystem.Instance.OnEnergyChanged -= HandleEnergyChanged;
                isSubscribedToEnergy = false;
            }
        }

        private void Start()
        {
            ResolveReferences();
            BuildOpeningHand();
            RebuildSlots();
            EnsureResponsiveContainer();
            UpdateResponsiveLayout(forceRefresh: true);
            TrySubscribe();
            if (BattleEnergySystem.Instance != null)
            {
                HandleEnergyChanged(BattleEnergySystem.Instance.CurrentEnergy, BattleEnergySystem.Instance.MaxEnergy);
            }
        }

        private void Update()
        {
            ResolveReferences();
            EnsureOpeningHandReady();
            if (!isSubscribedToEnergy)
            {
                TrySubscribe();
            }

            EnsureResponsiveContainer();
            UpdateUnlockProgression();
            UpdateSlotInteractivity();
            if (Time.unscaledTime >= nextTacticalAdviceRefreshTime)
            {
                nextTacticalAdviceRefreshTime = Time.unscaledTime + 1f / Mathf.Max(1f, tacticalAdviceRefreshRate);
                UpdateTacticalAdvice();
            }
            UpdateResponsiveLayout(forceRefresh: false);
        }

        private void EnsureOpeningHandReady()
        {
            RectTransform slotRect = slotContainer as RectTransform;
            bool needsRebuild = currentHand.Count <= 0 ||
                activeSlots.Count <= 0 ||
                slotRect == null ||
                slotRect.childCount <= 0 ||
                activeSlots.Count < Mathf.Min(maxSlots, Mathf.Max(currentHand.Count, activeHandSize));

            if (!needsRebuild)
            {
                return;
            }

            BuildOpeningHand();
            RebuildSlots();

            if (BattleEnergySystem.Instance != null)
            {
                HandleEnergyChanged(BattleEnergySystem.Instance.CurrentEnergy, BattleEnergySystem.Instance.MaxEnergy);
            }
        }

        private void BuildOpeningHand()
        {
            currentHand.Clear();
            nextDeckIndex = 0;
            unlockElapsedTime = 0f;
            RebuildCuratedDrawDeck();
            currentUnlockedSlots = useProgressiveUnlock
                ? Mathf.Clamp(openingUnlockedSlots, 1, activeHandSize)
                : Mathf.Clamp(activeHandSize, 1, maxSlots);

            IReadOnlyList<SummonData> deck = curatedDrawDeck.Count > 0
                ? curatedDrawDeck
                : summonSpawner != null ? summonSpawner.AvailableSummons : null;
            if (deck == null || deck.Count == 0)
            {
                return;
            }

            int desiredCards = Mathf.Clamp(currentUnlockedSlots, 1, maxSlots);
            for (int index = 0; index < desiredCards; index++)
            {
                currentHand.Add(DrawNextCard(deck));
            }
        }

        private SummonData DrawNextCard(IReadOnlyList<SummonData> deck)
        {
            if (deck == null || deck.Count == 0)
            {
                return null;
            }

            SummonData fallback = null;
            for (int attempt = 0; attempt < deck.Count; attempt++)
            {
                SummonData card = deck[nextDeckIndex % deck.Count];
                nextDeckIndex++;
                fallback ??= card;

                bool duplicateInHand = currentHand.Contains(card) && deck.Count > currentHand.Count;
                if (duplicateInHand)
                {
                    continue;
                }

                return card;
            }

            return fallback;
        }

        private void RebuildSlots()
        {
            if (slotContainer == null)
            {
                return;
            }

            for (int index = 0; index < activeSlots.Count; index++)
            {
                if (activeSlots[index] != null)
                {
                    Destroy(activeSlots[index].gameObject);
                }
            }

            activeSlots.Clear();
            int visibleSlotCount = ResolveVisibleSlotCount();

            for (int index = 0; index < maxSlots; index++)
            {
                int slotIndex = index;
                CardSlotUI slot = slotPrefab != null
                    ? Instantiate(slotPrefab, slotContainer)
                    : CardSlotUI.CreateRuntimeInstance(slotContainer);
                activeSlots.Add(slot);
                bool shouldShow = index < visibleSlotCount;
                slot.gameObject.SetActive(shouldShow);
                if (!shouldShow)
                {
                    slot.SetEmpty();
                    continue;
                }

                if (index < currentHand.Count && currentHand[index] != null)
                {
                    SummonData summonData = currentHand[index];
                    slot.Init(
                        summonData,
                        () => TryPlayCard(slotIndex, ResolveQuickPlayLane()),
                        laneIndex => TryPlayCard(slotIndex, laneIndex));
                    continue;
                }

                slot.SetEmpty();
            }

            UpdateResponsiveLayout(forceRefresh: true);
        }

        private void TryPlayCard(int handIndex, int laneIndex)
        {
            if (summonSpawner == null || handIndex < 0 || handIndex >= currentHand.Count)
            {
                return;
            }

            SummonData summonData = currentHand[handIndex];
            if (summonData == null)
            {
                return;
            }

            if (!summonSpawner.TrySpawnSummon(summonData, laneIndex, out SummonPlacementResult placementResult))
            {
                ShowSpawnFailureFeedback(placementResult);
                return;
            }

            IReadOnlyList<SummonData> deck = curatedDrawDeck.Count > 0 ? curatedDrawDeck : summonSpawner.AvailableSummons;
            currentHand.RemoveAt(handIndex);
            currentHand.Add(DrawNextCard(deck));
            RebuildSlots();

            if (BattleEnergySystem.Instance != null)
            {
                HandleEnergyChanged(BattleEnergySystem.Instance.CurrentEnergy, BattleEnergySystem.Instance.MaxEnergy);
            }

            UpdateTacticalAdvice();
        }

        private void ShowSpawnFailureFeedback(SummonSpawnFailureReason failureReason)
        {
            string message = failureReason switch
            {
                SummonSpawnFailureReason.NotEnoughEnergy => "에너지 부족",
                _ => "배치 불가"
            };

            Vector3 feedbackPosition = playerController != null
                ? playerController.transform.position + new Vector3(0f, 2.2f, 0f)
                : Vector3.zero;
            BattlePresentationController.Instance?.ShowWorldText(
                feedbackPosition,
                message,
                failureReason == SummonSpawnFailureReason.NotEnoughEnergy
                    ? new Color(1f, 0.82f, 0.42f, 1f)
                    : new Color(1f, 0.56f, 0.44f, 1f),
                3.8f,
                0.7f);
        }

        private void ShowSpawnFailureFeedback(SummonPlacementResult placementResult)
        {
            string message = string.IsNullOrWhiteSpace(placementResult.FailureReason)
                ? "배치 불가"
                : placementResult.FailureReason;

            Vector3 feedbackPosition = playerController != null
                ? playerController.transform.position + new Vector3(0f, 2.2f, 0f)
                : Vector3.zero;
            BattlePresentationController.Instance?.ShowWorldText(
                feedbackPosition,
                message,
                placementResult.FailureCode == SummonSpawnFailureReason.NotEnoughEnergy
                    ? new Color(1f, 0.82f, 0.42f, 1f)
                    : new Color(1f, 0.56f, 0.44f, 1f),
                3.8f,
                0.7f);
        }

        private int ResolveQuickPlayLane()
        {
            if (summonSpawner != null)
            {
                return summonSpawner.SelectedLaneIndex;
            }

            if (playerController != null)
            {
                return playerController.EscortLaneIndex;
            }

            return BattleLaneUtility.DefaultLaneCount / 2;
        }

        private void HandleEnergyChanged(float currentEnergy, float maxEnergy)
        {
            for (int index = 0; index < activeSlots.Count; index++)
            {
                SummonData summonData = index < currentHand.Count ? currentHand[index] : null;
                bool cooldownReady = summonSpawner == null || summonSpawner.RemainingSpawnCooldown <= 0.01f;
                bool canAfford = summonData != null && currentEnergy >= summonData.energyCost && cooldownReady;
                activeSlots[index].SetInteractable(canAfford, currentEnergy);
            }
        }

        private void UpdateUnlockProgression()
        {
            if (!useProgressiveUnlock || currentUnlockedSlots >= activeHandSize || summonSpawner == null || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            unlockElapsedTime += Time.deltaTime;
            if (unlockElapsedTime < unlockInterval)
            {
                return;
            }

            unlockElapsedTime = 0f;
            currentUnlockedSlots = Mathf.Min(activeHandSize, currentUnlockedSlots + 1);
            IReadOnlyList<SummonData> deck = curatedDrawDeck.Count > 0 ? curatedDrawDeck : summonSpawner.AvailableSummons;
            currentHand.Add(DrawNextCard(deck));
            RebuildSlots();
            if (BattleEnergySystem.Instance != null)
            {
                HandleEnergyChanged(BattleEnergySystem.Instance.CurrentEnergy, BattleEnergySystem.Instance.MaxEnergy);
            }
        }

        private void UpdateSlotInteractivity()
        {
            if (BattleEnergySystem.Instance == null)
            {
                return;
            }

            HandleEnergyChanged(BattleEnergySystem.Instance.CurrentEnergy, BattleEnergySystem.Instance.MaxEnergy);
        }

        private void TrySubscribe()
        {
            if (BattleEnergySystem.Instance == null || isSubscribedToEnergy)
            {
                return;
            }

            BattleEnergySystem.Instance.OnEnergyChanged += HandleEnergyChanged;
            isSubscribedToEnergy = true;
        }

        private void ResolveReferences()
        {
            if (summonSpawner == null)
            {
                summonSpawner = FindFirstObjectByType<SummonSpawner>();
            }

            battleManager = battleManager != null ? battleManager : BattleManager.Instance;
            enemyAI = enemyAI != null ? enemyAI : FindFirstObjectByType<EnemyAI>();

            if (battleManager != null)
            {
                playerController = battleManager.PlayerController != null
                    ? battleManager.PlayerController
                    : playerController;
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (playerController != null)
            {
                playerSkillController = playerController.GetComponent<PlayerSkillController>();
            }
        }

        private void EnsureResponsiveContainer()
        {
            RectTransform handRect = transform as RectTransform;
            if (handRect == null || slotContainer == null)
            {
                return;
            }

            Image handBackground = GetComponent<Image>();
            if (handBackground != null)
            {
                handBackground.raycastTarget = false;
            }

            RectTransform slotRect = slotContainer as RectTransform;
            if (slotRect == null)
            {
                return;
            }

            slotLayoutGroup ??= slotContainer.GetComponent<HorizontalLayoutGroup>();
            handScrollRect ??= GetComponent<ScrollRect>();
            if (handScrollRect == null)
            {
                handScrollRect = gameObject.AddComponent<ScrollRect>();
            }

            if (handViewport == null)
            {
                Transform existingViewport = transform.Find("Viewport");
                if (existingViewport is RectTransform existingViewportRect)
                {
                    handViewport = existingViewportRect;
                }
                else
                {
                    GameObject viewportObject = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
                    viewportObject.transform.SetParent(transform, false);
                    handViewport = viewportObject.GetComponent<RectTransform>();
                    Image viewportImage = viewportObject.GetComponent<Image>();
                    viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
                    viewportImage.raycastTarget = true;
                    Mask viewportMask = viewportObject.GetComponent<Mask>();
                    viewportMask.showMaskGraphic = false;
                }
            }

            if (slotRect.parent != handViewport)
            {
                slotRect.SetParent(handViewport, false);
            }

            CardHandScrollGuard scrollGuard = handViewport.GetComponent<CardHandScrollGuard>();
            if (scrollGuard == null)
            {
                scrollGuard = handViewport.gameObject.AddComponent<CardHandScrollGuard>();
            }

            scrollGuard.Configure(mobileScrollClickSuppressionDuration);

            handScrollRect.viewport = handViewport;
            handScrollRect.content = slotRect;
            handScrollRect.horizontal = true;
            handScrollRect.vertical = false;
            handScrollRect.movementType = ScrollRect.MovementType.Clamped;
            handScrollRect.inertia = true;
            handScrollRect.scrollSensitivity = 24f;
            handScrollRect.elasticity = 0.08f;
            handScrollRect.decelerationRate = 0.12f;
        }

        private void UpdateResponsiveLayout(bool forceRefresh)
        {
            RectTransform handRect = transform as RectTransform;
            RectTransform slotRect = slotContainer as RectTransform;
            if (handRect == null || slotRect == null)
            {
                return;
            }

            bool useMobileLayout = MobileBattleControls.ShouldUseMobileLayout(handRect);
            float effectiveWidth = Screen.width > 0 ? Screen.width : handRect.rect.width;
            bool compactLayout = useMobileLayout && effectiveWidth <= 620f;

            if (handViewport != null)
            {
                handViewport.anchorMin = new Vector2(0f, 0f);
                handViewport.anchorMax = new Vector2(1f, 1f);
                handViewport.pivot = new Vector2(0.5f, 0.5f);
                handViewport.offsetMin = useMobileLayout ? new Vector2(8f, 8f) : new Vector2(12f, 10f);
                handViewport.offsetMax = useMobileLayout ? new Vector2(-8f, -8f) : new Vector2(-12f, -10f);
            }

            if (!forceRefresh && useMobileLayout == isMobileLayoutActive && compactLayout == isCompactMobileLayout)
            {
                UpdateSlotContainerSize(useMobileLayout, compactLayout);
                return;
            }

            isMobileLayoutActive = useMobileLayout;
            isCompactMobileLayout = compactLayout;
            if (!useMobileLayout)
            {
                mobileScrollAlignedToRight = false;
            }

            if (slotLayoutGroup != null)
            {
                slotLayoutGroup.spacing = useMobileLayout ? (compactLayout ? 8f : 12f) : 12f;
                slotLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                slotLayoutGroup.padding = new RectOffset(useMobileLayout ? 4 : 0, useMobileLayout ? 4 : 0, 0, 0);
            }

            if (handScrollRect != null)
            {
                handScrollRect.enabled = useMobileLayout;
                handScrollRect.horizontal = useMobileLayout;
                if (!useMobileLayout)
                {
                    handScrollRect.horizontalNormalizedPosition = 0.5f;
                }
            }

            for (int index = 0; index < activeSlots.Count; index++)
            {
                if (activeSlots[index] != null)
                {
                    activeSlots[index].ApplyResponsiveLayout(useMobileLayout, compactLayout);
                }
            }

            UpdateSlotContainerSize(useMobileLayout, compactLayout);
        }

        private void UpdateSlotContainerSize(bool useMobileLayout, bool compactLayout)
        {
            RectTransform slotRect = slotContainer as RectTransform;
            if (slotRect == null)
            {
                return;
            }

            RectTransform handRect = transform as RectTransform;
            if (handRect == null)
            {
                return;
            }

            float widthBlend = useMobileLayout ? Mathf.Clamp01(((handViewport != null ? handViewport.rect.width : handRect.rect.width) - 340f) / 260f) : 1f;
            float slotWidth = compactLayout ? 132f : useMobileLayout ? 150f : 138f;
            float slotHeight = compactLayout ? 144f : useMobileLayout ? 160f : 150f;
            int visibleSlotCount = ResolveVisibleSlotCount();
            for (int index = 0; index < activeSlots.Count; index++)
            {
                if (activeSlots[index] != null)
                {
                    activeSlots[index].gameObject.SetActive(index < visibleSlotCount);
                }
            }

            CardSlotUI firstVisibleSlot = null;
            for (int index = 0; index < visibleSlotCount && index < activeSlots.Count; index++)
            {
                if (activeSlots[index] != null)
                {
                    firstVisibleSlot = activeSlots[index];
                    break;
                }
            }

            if (firstVisibleSlot != null)
            {
                Vector2 baseSize = firstVisibleSlot.GetCurrentBaseSize();
                slotWidth = baseSize.x > 0.1f ? baseSize.x : slotWidth;
                slotHeight = baseSize.y > 0.1f ? baseSize.y : slotHeight;
            }

            float spacing = slotLayoutGroup != null ? slotLayoutGroup.spacing : 10f;
            int leftPadding = slotLayoutGroup != null ? slotLayoutGroup.padding.left : 0;
            int rightPadding = slotLayoutGroup != null ? slotLayoutGroup.padding.right : 0;
            float viewportWidth = handViewport != null ? handViewport.rect.width : handRect.rect.width;
            float fitScale = 1f;
            if (useMobileLayout && slotLayoutGroup != null && visibleSlotCount > 1)
            {
                float minSpacing = compactLayout ? 4f : 6f;
                float availableWidth = Mathf.Max(0f, viewportWidth - leftPadding - rightPadding);
                float fillSpacing = (availableWidth - (visibleSlotCount * slotWidth)) / (visibleSlotCount - 1f);
                float maxSpacing = Mathf.Lerp(6f, 11f, widthBlend);
                spacing = Mathf.Clamp(fillSpacing, minSpacing, maxSpacing);
                float minimumContentWidth = (visibleSlotCount * slotWidth) + ((visibleSlotCount - 1f) * minSpacing);
                if (minimumContentWidth > availableWidth)
                {
                    fitScale = Mathf.Clamp((availableWidth - ((visibleSlotCount - 1f) * minSpacing)) / (visibleSlotCount * Mathf.Max(1f, slotWidth)), compactLayout ? 0.68f : 0.76f, 1f);
                    spacing = minSpacing;
                }

                for (int index = 0; index < activeSlots.Count; index++)
                {
                    if (activeSlots[index] != null)
                    {
                        activeSlots[index].SetLayoutFitScale(index < visibleSlotCount ? fitScale : 1f);
                    }
                }

                if (firstVisibleSlot != null && firstVisibleSlot.transform is RectTransform activeSlotRect)
                {
                    slotWidth = activeSlotRect.rect.width > 0.1f ? activeSlotRect.rect.width : slotWidth;
                    slotHeight = activeSlotRect.rect.height > 0.1f ? activeSlotRect.rect.height : slotHeight;
                }

                slotLayoutGroup.spacing = spacing;
            }

            int cardCount = visibleSlotCount;
            float contentWidth = leftPadding + rightPadding + (cardCount * slotWidth) + (Mathf.Max(0, cardCount - 1) * spacing);
            float finalWidth = useMobileLayout ? Mathf.Max(contentWidth, viewportWidth) : Mathf.Max(760f, contentWidth);
            bool contentFitsViewport = contentWidth < viewportWidth - 12f;

            slotRect.anchorMin = useMobileLayout ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = useMobileLayout ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            slotRect.pivot = useMobileLayout ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            slotRect.anchoredPosition = Vector2.zero;
            slotRect.localScale = Vector3.one;
            slotRect.sizeDelta = new Vector2(finalWidth, slotHeight + 6f);

            if (slotLayoutGroup != null)
            {
                slotLayoutGroup.childAlignment = useMobileLayout
                    ? (contentFitsViewport ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft)
                    : (contentFitsViewport ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft);
            }

            if (useMobileLayout && handScrollRect != null && !mobileScrollAlignedToRight)
            {
                Canvas.ForceUpdateCanvases();
                handScrollRect.horizontalNormalizedPosition = contentFitsViewport ? 0.5f : 0f;
                mobileScrollAlignedToRight = true;
            }
        }

        private int ResolveVisibleSlotCount()
        {
            int desiredVisibleCount = currentUnlockedSlots > 0
                ? currentUnlockedSlots
                : Mathf.Max(currentHand.Count, activeHandSize);
            return Mathf.Clamp(desiredVisibleCount, 1, maxSlots);
        }

        private void RebuildCuratedDrawDeck()
        {
            curatedDrawDeck.Clear();
            IReadOnlyList<SummonData> deck = summonSpawner != null ? summonSpawner.AvailableSummons : null;
            if (deck == null || deck.Count == 0)
            {
                return;
            }

            AddFirstMatching(deck, data => IsCheapOpener(data));
            AddFirstMatching(deck, data => data != null && data.summonType == SummonType.Tank);
            AddFirstMatching(deck, data => IsLaneStabilizer(data));
            AddFirstMatching(deck, data => data != null && data.summonType == SummonType.Support);
            AddFirstMatching(deck, data => IsBreaker(data) || IsSplash(data));

            for (int index = 0; index < deck.Count; index++)
            {
                SummonData summonData = deck[index];
                if (summonData != null && !curatedDrawDeck.Contains(summonData))
                {
                    curatedDrawDeck.Add(summonData);
                }
            }
        }

        private void AddFirstMatching(IReadOnlyList<SummonData> deck, System.Predicate<SummonData> predicate)
        {
            if (deck == null || predicate == null)
            {
                return;
            }

            for (int index = 0; index < deck.Count; index++)
            {
                SummonData summonData = deck[index];
                if (summonData == null || curatedDrawDeck.Contains(summonData) || !predicate(summonData))
                {
                    continue;
                }

                curatedDrawDeck.Add(summonData);
                return;
            }
        }

        private void UpdateTacticalAdvice()
        {
            if (activeSlots.Count == 0 || currentHand.Count == 0)
            {
                return;
            }

            float currentEnergy = BattleEnergySystem.Instance != null ? BattleEnergySystem.Instance.CurrentEnergy : 0f;
            bool cooldownReady = summonSpawner == null || summonSpawner.RemainingSpawnCooldown <= 0.01f;
            BattleManager.FrontlineState frontlineState = default;
            BattleManager.PlayerTerritoryState territoryState = default;
            bool hasFrontline = battleManager != null && battleManager.TryGetFrontlineState(out frontlineState);
            bool hasTerritory = battleManager != null && battleManager.TryGetPlayerTerritoryState(out territoryState);
            int activeStructures = CountActiveStructures();

            int bestIndex = -1;
            float bestScore = float.MinValue;
            string bestHint = string.Empty;
            if (adviceBuffer.Length < currentHand.Count)
            {
                adviceBuffer = new CardAdvice[currentHand.Count];
            }

            for (int index = 0; index < currentHand.Count; index++)
            {
                SummonData summonData = currentHand[index];
                CardAdvice advice = EvaluateCardAdvice(
                    summonData,
                    currentEnergy,
                    cooldownReady,
                    hasFrontline,
                    frontlineState,
                    hasTerritory,
                    territoryState,
                    activeStructures);
                adviceBuffer[index] = advice;
                if (advice.Score > bestScore)
                {
                    bestScore = advice.Score;
                    bestIndex = index;
                    bestHint = advice.Hint;
                }
            }

            if (bestScore < 1.6f)
            {
                bestIndex = -1;
                bestHint = string.Empty;
            }

            for (int index = 0; index < activeSlots.Count; index++)
            {
                CardSlotUI slot = activeSlots[index];
                if (slot == null)
                {
                    continue;
                }

                if (index >= currentHand.Count || currentHand[index] == null)
                {
                    slot.SetTacticalHint(string.Empty, false);
                    continue;
                }

                string hint = adviceBuffer[index].Hint;
                bool isRecommended = index == bestIndex && !string.IsNullOrWhiteSpace(bestHint);
                slot.SetTacticalHint(hint, isRecommended);
            }
        }

        private CardAdvice EvaluateCardAdvice(
            SummonData summonData,
            float currentEnergy,
            bool cooldownReady,
            bool hasFrontline,
            BattleManager.FrontlineState frontlineState,
            bool hasTerritory,
            BattleManager.PlayerTerritoryState territoryState,
            int activeStructures)
        {
            if (summonData == null)
            {
                return new CardAdvice(float.MinValue, string.Empty);
            }

            int primaryLaneIndex = summonSpawner != null
                ? summonSpawner.SelectedLaneIndex
                : battleManager != null && battleManager.PlayerController != null
                    ? battleManager.PlayerController.EscortLaneIndex
                    : BattleLaneUtility.DefaultLaneCount / 2;
            BattleManager.SummonLanePreview lanePreview = default;
            bool hasLanePreview = battleManager != null && battleManager.TryGetSummonLanePreview(primaryLaneIndex, out lanePreview);
            bool isTank = summonData.summonType == SummonType.Tank;
            bool isSupport = summonData.summonType == SummonType.Support;
            bool isBreaker = IsBreaker(summonData);
            bool isRush = IsCheapOpener(summonData);
            bool isSplash = IsSplash(summonData);
            bool isLane = IsLaneStabilizer(summonData);
            bool canAffordNow = cooldownReady && currentEnergy >= summonData.energyCost;
            bool frontlineLosing = hasFrontline && (frontlineState.Balance <= -0.22f || frontlineState.EnemyUnitCount >= frontlineState.PlayerUnitCount + 2);
            bool lowEnergy = currentEnergy <= 38f;
            bool rewardLane = hasLanePreview && lanePreview.PreviewState == BattleManager.SummonLanePreviewState.Reward;
            bool blockerLane = hasLanePreview && lanePreview.HasBlocker;
            bool overextended = hasTerritory && (territoryState.OverextendDistance >= 1.15f || territoryState.IsInEnemyBaseZone);

            float score = 0f;
            float bestHintWeight = 0.25f;
            string hint = "\uC800\uC7A5";

            if (canAffordNow)
            {
                score += 0.8f;
            }
            else
            {
                score -= Mathf.Clamp((summonData.energyCost - currentEnergy) / 10f, 0f, 4.5f);
            }

            if (lowEnergy)
            {
                ApplyAdviceWeight(isRush ? 2.2f : summonData.energyCost <= 40f ? 1.2f : -1.2f, "\uC800\uC7A5", ref score, ref bestHintWeight, ref hint);
            }

            if (frontlineLosing)
            {
                ApplyAdviceWeight(
                    isTank ? 5.4f : isLane ? 4.6f : isSplash ? 3.6f : isSupport ? 2.2f : 1.4f,
                    isSupport ? "\uC9C0\uC6D0" : "\uC720\uC9C0",
                    ref score,
                    ref bestHintWeight,
                    ref hint);
            }

            if (blockerLane)
            {
                ApplyAdviceWeight(
                    isBreaker ? 5.8f : isTank ? 4.4f : isRush ? 3.2f : isSupport ? -0.4f : 1.4f,
                    "\uB3CC\uD30C",
                    ref score,
                    ref bestHintWeight,
                    ref hint);
            }

            if (rewardLane)
            {
                ApplyAdviceWeight(
                    isSupport ? 5.2f : isLane ? 4.8f : isBreaker ? 3.8f : isTank ? 0.8f : 1.8f,
                    "\uBCF4\uC0C1",
                    ref score,
                    ref bestHintWeight,
                    ref hint);
            }

            if (overextended)
            {
                ApplyAdviceWeight(
                    isTank ? 3.4f : isLane ? 2.6f : -0.6f,
                    "\uC720\uC9C0",
                    ref score,
                    ref bestHintWeight,
                    ref hint);
            }

            if (isSupport && hasFrontline && frontlineState.PlayerUnitCount < 2)
            {
                score -= 2.1f;
            }

            if (isBreaker && activeStructures >= 4 && !blockerLane)
            {
                score -= 1.1f;
            }

            if (!canAffordNow && hint == "\uC800\uC7A5")
            {
                hint = "\uC800\uC7A5";
            }

            return new CardAdvice(score, hint);
        }

        private static void ApplyAdviceWeight(float weight, string hint, ref float totalScore, ref float bestHintWeight, ref string bestHint)
        {
            totalScore += weight;
            if (weight > bestHintWeight)
            {
                bestHintWeight = weight;
                bestHint = hint;
            }
        }

        private static bool IsCheapOpener(SummonData summonData)
        {
            return summonData != null &&
                summonData.summonType == SummonType.Melee &&
                summonData.energyCost <= 24f &&
                summonData.moveSpeed >= 3f;
        }

        private static bool IsBreaker(SummonData summonData)
        {
            return summonData != null &&
                summonData.summonType == SummonType.Melee &&
                summonData.structureDamageMultiplier >= 1.8f;
        }

        private static bool IsSplash(SummonData summonData)
        {
            return summonData != null &&
                summonData.summonType == SummonType.Ranged &&
                summonData.splashRadius > 0.1f;
        }

        private static bool IsLaneStabilizer(SummonData summonData)
        {
            return summonData != null &&
                summonData.summonType == SummonType.Ranged &&
                summonData.attackRange >= 7f;
        }

        private static int CountActiveStructures()
        {
            return BattleStructure.ActiveCount;
        }
    }

    public static class CardHandTouchCoordinator
    {
        private static float suppressClicksUntil;

        public static void SuppressClicks(float duration)
        {
            suppressClicksUntil = Mathf.Max(suppressClicksUntil, Time.unscaledTime + Mathf.Max(0.01f, duration));
        }

        public static bool ShouldSuppressClicks()
        {
            return Time.unscaledTime <= suppressClicksUntil;
        }
    }

    public class CardHandScrollGuard : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float DragThresholdPixels = 14f;

        private Vector2 pointerDownPosition;
        private bool dragSuppressedThisGesture;
        private float suppressionDuration = 0.16f;

        public void Configure(float duration)
        {
            suppressionDuration = Mathf.Max(0.05f, duration);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownPosition = eventData.position;
            dragSuppressedThisGesture = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SuppressIfNeeded(eventData.position, force: true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SuppressIfNeeded(eventData.position, force: false);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragSuppressedThisGesture)
            {
                CardHandTouchCoordinator.SuppressClicks(suppressionDuration);
            }
        }

        private void SuppressIfNeeded(Vector2 currentPosition, bool force)
        {
            if (dragSuppressedThisGesture)
            {
                CardHandTouchCoordinator.SuppressClicks(suppressionDuration);
                return;
            }

            if (!force && (currentPosition - pointerDownPosition).sqrMagnitude < DragThresholdPixels * DragThresholdPixels)
            {
                return;
            }

            dragSuppressedThisGesture = true;
            CardHandTouchCoordinator.SuppressClicks(suppressionDuration);
        }
    }
}
