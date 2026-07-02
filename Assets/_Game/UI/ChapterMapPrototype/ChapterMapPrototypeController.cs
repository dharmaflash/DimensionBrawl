using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class ChapterMapPrototypeController : MonoBehaviour
    {
        [Serializable]
        private sealed class RegionEntry
        {
            [SerializeField] private string regionId;
            [SerializeField] private string title;
            [SerializeField] private string subtitle;
            [SerializeField] private string guideText;
            [SerializeField] private Button button;
            [SerializeField] private CanvasGroup labelGroup;
            [SerializeField] private CanvasGroup stageGroup;
            [SerializeField] private Vector2 focusedMapPosition;
            [SerializeField, Min(1f)] private float focusedMapScale = 1.8f;

            public string RegionId => regionId;
            public string Title => title;
            public string Subtitle => subtitle;
            public string GuideText => guideText;
            public Button Button => button;
            public CanvasGroup LabelGroup => labelGroup;
            public CanvasGroup StageGroup => stageGroup;
            public Vector2 FocusedMapPosition => focusedMapPosition;
            public float FocusedMapScale => focusedMapScale;
        }

        [SerializeField] private RectTransform mapContent;
        [SerializeField] private CanvasGroup overviewGroup;
        [SerializeField] private RectTransform detailPanel;
        [SerializeField] private CanvasGroup detailGroup;
        [SerializeField] private Button backButton;
        [SerializeField] private Button overviewButton;
        [SerializeField] private TMP_Text chapterTitleText;
        [SerializeField] private TMP_Text chapterSubtitleText;
        [SerializeField] private TMP_Text guideText;
        [SerializeField] private TMP_Text detailTitleText;
        [SerializeField] private TMP_Text detailSubtitleText;
        [SerializeField] private TMP_Text detailObjectivesText;
        [SerializeField] private TMP_Text detailRewardsText;
        [SerializeField] private TMP_Text detailCostText;
        [SerializeField] private RegionEntry[] regions = Array.Empty<RegionEntry>();
        [SerializeField] private ChapterMapPrototypeStageNode[] stageNodes = Array.Empty<ChapterMapPrototypeStageNode>();
        [SerializeField] private Vector2 overviewMapPosition;
        [SerializeField, Min(0.1f)] private float overviewMapScale = 1f;
        [SerializeField, Min(1f)] private float selectedStageScale = 2.3f;
        [SerializeField] private Vector2 selectedStageScreenAnchor = new Vector2(-300f, -20f);
        [SerializeField] private Vector2 detailPanelShownPosition;
        [SerializeField] private Vector2 detailPanelHiddenPosition = new Vector2(520f, 0f);
        [SerializeField, Min(0f)] private float regionTransitionSeconds = 0.85f;
        [SerializeField, Min(0f)] private float stageTransitionSeconds = 0.55f;
        [SerializeField, Min(0f)] private float detailTransitionSeconds = 0.28f;

        private Coroutine transitionRoutine;
        private int currentRegionIndex = -1;
        private ChapterMapPrototypeStageNode selectedStageNode;
        private UnityAction[] regionButtonActions = Array.Empty<UnityAction>();

        private void Awake()
        {
            ApplyOverviewImmediate();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && HasBackTarget())
            {
                HandleBackClicked();
            }
        }

        private bool HasBackTarget()
        {
            return selectedStageNode != null || currentRegionIndex >= 0;
        }

        private void OnEnable()
        {
            regionButtonActions = new UnityAction[regions.Length];
            for (int i = 0; i < regions.Length; i++)
            {
                int regionIndex = i;
                regionButtonActions[i] = () => SelectRegion(regionIndex);
                if (regions[i].Button != null)
                {
                    regions[i].Button.onClick.AddListener(regionButtonActions[i]);
                }
            }

            for (int i = 0; i < stageNodes.Length; i++)
            {
                if (stageNodes[i] != null)
                {
                    stageNodes[i].Clicked += HandleStageClicked;
                }
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }

            if (overviewButton != null)
            {
                overviewButton.onClick.AddListener(ReturnToOverview);
            }
        }

        private void OnDisable()
        {
            StopTransition();

            for (int i = 0; i < regions.Length; i++)
            {
                if (regions[i].Button != null)
                {
                    UnityAction action = i < regionButtonActions.Length ? regionButtonActions[i] : null;
                    if (action != null)
                    {
                        regions[i].Button.onClick.RemoveListener(action);
                    }
                }
            }

            for (int i = 0; i < stageNodes.Length; i++)
            {
                if (stageNodes[i] != null)
                {
                    stageNodes[i].Clicked -= HandleStageClicked;
                }
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackClicked);
            }

            if (overviewButton != null)
            {
                overviewButton.onClick.RemoveListener(ReturnToOverview);
            }
        }

        public void ReturnToOverview()
        {
            StopTransition();
            transitionRoutine = StartCoroutine(MoveToOverviewRoutine());
        }

        private void SelectRegion(int regionIndex)
        {
            if (regionIndex < 0 || regionIndex >= regions.Length)
            {
                return;
            }

            StopTransition();
            transitionRoutine = StartCoroutine(MoveToRegionRoutine(regionIndex));
        }

        private void HandleBackClicked()
        {
            if (selectedStageNode != null && currentRegionIndex >= 0)
            {
                StopTransition();
                transitionRoutine = StartCoroutine(MoveToRegionRoutine(currentRegionIndex));
                return;
            }

            ReturnToOverview();
        }

        private void HandleStageClicked(ChapterMapPrototypeStageNode stageNode)
        {
            if (stageNode == null || currentRegionIndex < 0)
            {
                return;
            }

            RegionEntry activeRegion = regions[currentRegionIndex];
            if (!string.Equals(stageNode.RegionId, activeRegion.RegionId, StringComparison.Ordinal))
            {
                return;
            }

            StopTransition();
            selectedStageNode = stageNode;
            ApplySelectedStageVisuals(stageNode);
            ApplyDetail(stageNode);
            transitionRoutine = StartCoroutine(MoveToStageRoutine(stageNode));
        }

        private IEnumerator MoveToOverviewRoutine()
        {
            selectedStageNode = null;
            ApplySelectedStageVisuals(null);
            SetStageNodesActive(null);

            Vector2 startPosition = GetMapPosition();
            float startScale = GetMapScale();
            Vector2 startPanelPosition = GetDetailPanelPosition();
            float startDetailAlpha = detailGroup != null ? detailGroup.alpha : 0f;

            for (float elapsed = 0f; elapsed < regionTransitionSeconds; elapsed += Time.unscaledDeltaTime)
            {
                float t = Ease(elapsed / regionTransitionSeconds);
                SetMap(Vector2.LerpUnclamped(startPosition, overviewMapPosition, t), Mathf.LerpUnclamped(startScale, overviewMapScale, t));
                SetCanvasGroup(overviewGroup, t, true);
                SetAllRegionLabelsAlpha(t);
                SetOnlyStageGroupAlpha(currentRegionIndex, 1f - t);
                SetDetail(t: Mathf.LerpUnclamped(startDetailAlpha, 0f, t), Vector2.LerpUnclamped(startPanelPosition, detailPanelHiddenPosition, t));
                yield return null;
            }

            currentRegionIndex = -1;
            SetMap(overviewMapPosition, overviewMapScale);
            SetCanvasGroup(overviewGroup, 1f, true);
            SetAllRegionLabelsAlpha(1f);
            SetAllStageGroupsAlpha(0f);
            SetDetail(0f, detailPanelHiddenPosition);
            ApplyOverviewText();
            transitionRoutine = null;
        }

        private IEnumerator MoveToRegionRoutine(int regionIndex)
        {
            selectedStageNode = null;
            ApplySelectedStageVisuals(null);
            currentRegionIndex = regionIndex;
            RegionEntry region = regions[regionIndex];
            ApplyRegionText(region);
            SetStageNodesActive(region.RegionId);

            Vector2 startPosition = GetMapPosition();
            float startScale = GetMapScale();
            Vector2 startPanelPosition = GetDetailPanelPosition();
            float startDetailAlpha = detailGroup != null ? detailGroup.alpha : 0f;

            for (float elapsed = 0f; elapsed < regionTransitionSeconds; elapsed += Time.unscaledDeltaTime)
            {
                float t = Ease(elapsed / regionTransitionSeconds);
                SetMap(
                    Vector2.LerpUnclamped(startPosition, region.FocusedMapPosition, t),
                    Mathf.LerpUnclamped(startScale, region.FocusedMapScale, t));
                SetCanvasGroup(overviewGroup, 1f - t, false);
                SetRegionLabelAlpha(regionIndex, 1f - t * 0.75f);
                SetOnlyStageGroupAlpha(regionIndex, t);
                SetDetail(Mathf.LerpUnclamped(startDetailAlpha, 0f, t), Vector2.LerpUnclamped(startPanelPosition, detailPanelHiddenPosition, t));
                yield return null;
            }

            SetMap(region.FocusedMapPosition, region.FocusedMapScale);
            SetCanvasGroup(overviewGroup, 0f, false);
            SetRegionLabelAlpha(regionIndex, 0.25f);
            SetOnlyStageGroupAlpha(regionIndex, 1f);
            SetDetail(0f, detailPanelHiddenPosition);
            transitionRoutine = null;
        }

        private IEnumerator MoveToStageRoutine(ChapterMapPrototypeStageNode stageNode)
        {
            Vector2 targetPosition = CalculateStageFocusPosition(stageNode);
            Vector2 startPosition = GetMapPosition();
            float startScale = GetMapScale();
            Vector2 startPanelPosition = GetDetailPanelPosition();
            float startDetailAlpha = detailGroup != null ? detailGroup.alpha : 0f;
            float duration = Mathf.Max(stageTransitionSeconds, detailTransitionSeconds);

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float mapT = Ease(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, stageTransitionSeconds)));
                float detailT = Ease(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, detailTransitionSeconds)));
                SetMap(Vector2.LerpUnclamped(startPosition, targetPosition, mapT), Mathf.LerpUnclamped(startScale, selectedStageScale, mapT));
                SetDetail(Mathf.LerpUnclamped(startDetailAlpha, 1f, detailT), Vector2.LerpUnclamped(startPanelPosition, detailPanelShownPosition, detailT));
                yield return null;
            }

            SetMap(targetPosition, selectedStageScale);
            SetDetail(1f, detailPanelShownPosition);
            transitionRoutine = null;
        }

        private void ApplyOverviewImmediate()
        {
            currentRegionIndex = -1;
            selectedStageNode = null;
            SetMap(overviewMapPosition, overviewMapScale);
            SetCanvasGroup(overviewGroup, 1f, true);
            SetAllRegionLabelsAlpha(1f);
            SetAllStageGroupsAlpha(0f);
            SetDetail(0f, detailPanelHiddenPosition);
            SetStageNodesActive(null);
            ApplySelectedStageVisuals(null);
            ApplyOverviewText();
        }

        private Vector2 CalculateStageFocusPosition(ChapterMapPrototypeStageNode stageNode)
        {
            RectTransform rectTransform = stageNode.RectTransform;
            Vector2 localPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
            return selectedStageScreenAnchor - localPosition * selectedStageScale;
        }

        private void ApplyOverviewText()
        {
            SetText(chapterTitleText, "Annihilation Run");
            SetText(chapterSubtitleText, "Chapter 17");
            SetText(guideText, "Select a region to move the map camera to that front.");
        }

        private void ApplyRegionText(RegionEntry region)
        {
            SetText(chapterTitleText, region.Title);
            SetText(chapterSubtitleText, region.Subtitle);
            SetText(guideText, region.GuideText);
        }

        private void ApplyDetail(ChapterMapPrototypeStageNode stageNode)
        {
            string lockPrefix = stageNode.IsLocked ? "[LOCKED] " : string.Empty;
            SetText(detailTitleText, $"{lockPrefix}{stageNode.StageCode} \"{stageNode.StageTitle}\"");
            SetText(detailSubtitleText, stageNode.StageSubtitle);
            SetText(detailObjectivesText, stageNode.ObjectiveText);
            SetText(detailRewardsText, stageNode.RewardText);
            SetText(detailCostText, stageNode.IsLocked ? "-" : stageNode.EnergyCostText);
        }

        private void ApplySelectedStageVisuals(ChapterMapPrototypeStageNode selected)
        {
            for (int i = 0; i < stageNodes.Length; i++)
            {
                if (stageNodes[i] != null)
                {
                    stageNodes[i].SetSelected(stageNodes[i] == selected);
                }
            }
        }

        private void SetStageNodesActive(string regionId)
        {
            for (int i = 0; i < stageNodes.Length; i++)
            {
                ChapterMapPrototypeStageNode node = stageNodes[i];
                if (node != null)
                {
                    node.SetRegionActive(!string.IsNullOrEmpty(regionId) && string.Equals(node.RegionId, regionId, StringComparison.Ordinal));
                }
            }
        }

        private void StopTransition()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }
        }

        private Vector2 GetMapPosition()
        {
            return mapContent != null ? mapContent.anchoredPosition : Vector2.zero;
        }

        private float GetMapScale()
        {
            return mapContent != null ? mapContent.localScale.x : overviewMapScale;
        }

        private void SetMap(Vector2 anchoredPosition, float scale)
        {
            if (mapContent == null)
            {
                return;
            }

            mapContent.anchoredPosition = anchoredPosition;
            mapContent.localScale = new Vector3(scale, scale, 1f);
        }

        private Vector2 GetDetailPanelPosition()
        {
            return detailPanel != null ? detailPanel.anchoredPosition : detailPanelHiddenPosition;
        }

        private void SetDetail(float t, Vector2 anchoredPosition)
        {
            SetCanvasGroup(detailGroup, Mathf.Clamp01(t), t > 0.99f);

            if (detailPanel != null)
            {
                detailPanel.anchoredPosition = anchoredPosition;
            }
        }

        private void SetAllRegionLabelsAlpha(float alpha)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                SetCanvasGroup(regions[i].LabelGroup, alpha, alpha > 0.5f);
            }
        }

        private void SetRegionLabelAlpha(int selectedIndex, float selectedAlpha)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                float alpha = i == selectedIndex ? selectedAlpha : 0f;
                SetCanvasGroup(regions[i].LabelGroup, alpha, alpha > 0.5f);
            }
        }

        private void SetAllStageGroupsAlpha(float alpha)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                SetCanvasGroup(regions[i].StageGroup, alpha, false);
            }
        }

        private void SetOnlyStageGroupAlpha(int selectedIndex, float selectedAlpha)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                float alpha = i == selectedIndex ? selectedAlpha : 0f;
                SetCanvasGroup(regions[i].StageGroup, alpha, i == selectedIndex && alpha > 0.98f);
            }
        }

        private static void SetCanvasGroup(CanvasGroup canvasGroup, float alpha, bool interactive)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = Mathf.Clamp01(alpha);
            canvasGroup.interactable = interactive;
            canvasGroup.blocksRaycasts = interactive;
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static float Ease(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
        }
    }
}
