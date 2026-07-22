using System;
using System.Collections.Generic;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class StageSelectMotionPrefabSetup
    {
        private const string StageSelectPrefabPath = "Assets/_Game/UI/StageSelect/PF_UI_StageSelectScreen.prefab";
        private const string MotionCatalogPath = "Assets/_Game/DesignData/UI/DB_UIMotionCatalog.asset";
        private const string StageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private static readonly string[] StageCardNames =
        {
            "01-1_StageCard",
            "01-2_StageCard",
            "01-3_StageCard",
            "01-4_StageCard"
        };
        private static readonly string[] PlaceholderChapterCardNames =
        {
            "EP 02_ChapterCard",
            "EP 03_ChapterCard",
            "EP 04_ChapterCard"
        };

        [MenuItem("DimensionBrawl/UI V1/Reapply Stage Select Motion")]
        public static void ApplyMenu()
        {
            ApplyStageSelectMotion();
        }

        public static void ApplyStageSelectMotion()
        {
            UIMotionCatalog motionCatalog = AssetDatabase.LoadAssetAtPath<UIMotionCatalog>(MotionCatalogPath);
            if (motionCatalog == null)
            {
                throw new InvalidOperationException($"Stage select motion setup could not find {MotionCatalogPath}.");
            }

            UIStageCatalog stageCatalog = AssetDatabase.LoadAssetAtPath<UIStageCatalog>(StageCatalogPath);
            if (stageCatalog == null)
            {
                throw new InvalidOperationException($"Stage select motion setup could not find {StageCatalogPath}.");
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(StageSelectPrefabPath);
            try
            {
                List<SequenceEntrySpec> entries = new List<SequenceEntrySpec>();

                AddMotion(prefabRoot, "StageSelectArtRoot", "stage_screen_enter", 0f, motionCatalog, entries);
                AddMotion(prefabRoot, "BackButton", "stage_back_button_enter", 0.04f, motionCatalog, entries);
                AddMotion(prefabRoot, "ChapterScrollArea", "stage_chapter_list_enter", 0.08f, motionCatalog, entries);
                AddMotion(prefabRoot, "EP 01_SelectedChapterCard", "stage_card_enter", 0.12f, motionCatalog, entries);
                AddMotion(prefabRoot, "SelectedPanel", "stage_feature_panel_enter", 0.12f, motionCatalog, entries);
                AddMotion(prefabRoot, "CurrentChapterFrame", "stage_feature_panel_enter", 0.16f, motionCatalog, entries);
                AddMotion(prefabRoot, "StageScrollArea", "stage_stage_grid_enter", 0.22f, motionCatalog, entries);
                AddMotion(prefabRoot, "01-1_StageCard", "stage_card_enter", 0.27f, motionCatalog, entries);
                AddMotion(prefabRoot, "01-2_StageCard", "stage_card_enter", 0.31f, motionCatalog, entries);
                AddMotion(prefabRoot, "01-3_StageCard", "stage_card_enter", 0.35f, motionCatalog, entries);
                AddMotion(prefabRoot, "01-4_StageCard", "stage_card_enter", 0.39f, motionCatalog, entries);
                AddMotion(prefabRoot, "BottomLeftButton", "stage_bottom_nav_enter", 0.42f, motionCatalog, entries);
                AddMotion(prefabRoot, "BottomCenterSelectedButton", "stage_bottom_nav_enter", 0.46f, motionCatalog, entries);
                AddMotion(prefabRoot, "BottomRightButton", "stage_bottom_nav_enter", 0.5f, motionCatalog, entries);
                AddMotion(prefabRoot, "StartButton", "stage_start_button_enter", 0.48f, motionCatalog, entries);

                UIMotionSequencePresenter sequence = prefabRoot.GetComponent<UIMotionSequencePresenter>();
                if (sequence == null)
                {
                    sequence = prefabRoot.AddComponent<UIMotionSequencePresenter>();
                }

                ConfigureSequence(sequence, entries);
                UIScrollRectMotionPresenter stageScrollMotion = ConfigureScrollMotion(
                    prefabRoot,
                    "StageScrollArea",
                    snapOnEndDrag: false,
                    focusDurationSeconds: 0.3f);
                UIScrollRectMotionPresenter chapterScrollMotion = ConfigureScrollMotion(
                    prefabRoot,
                    "ChapterScrollArea",
                    snapOnEndDrag: false,
                    focusDurationSeconds: 0.28f);
                ConfigureStageSelectPresenterFocus(prefabRoot, stageScrollMotion, chapterScrollMotion, stageCatalog);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, StageSelectPrefabPath);
                Debug.Log($"Stage select motion setup applied with {entries.Count} sequence entr{(entries.Count == 1 ? "y" : "ies")}.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void AddMotion(
            GameObject prefabRoot,
            string targetName,
            string motionId,
            float startDelaySeconds,
            UIMotionCatalog motionCatalog,
            List<SequenceEntrySpec> entries)
        {
            GameObject target = FindChild(prefabRoot, targetName);
            if (target == null)
            {
                throw new InvalidOperationException($"Stage select motion setup could not find child '{targetName}'.");
            }

            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                throw new InvalidOperationException($"Stage select motion target '{targetName}' must have a RectTransform.");
            }

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            UIMotionPresenter motionPresenter = target.GetComponent<UIMotionPresenter>();
            if (motionPresenter == null)
            {
                motionPresenter = target.AddComponent<UIMotionPresenter>();
            }

            ConfigureMotionPresenter(motionPresenter, motionCatalog, targetRect, canvasGroup, motionId);
            entries.Add(new SequenceEntrySpec(motionPresenter, motionId, startDelaySeconds));
        }

        private static void ConfigureMotionPresenter(
            UIMotionPresenter motionPresenter,
            UIMotionCatalog motionCatalog,
            RectTransform targetRect,
            CanvasGroup canvasGroup,
            string motionId)
        {
            SerializedObject serializedObject = new SerializedObject(motionPresenter);
            serializedObject.FindProperty("catalog").objectReferenceValue = motionCatalog;
            serializedObject.FindProperty("targetRect").objectReferenceValue = targetRect;
            serializedObject.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedObject.FindProperty("defaultMotionId").stringValue = motionId;
            serializedObject.FindProperty("playOnEnable").boolValue = false;
            serializedObject.FindProperty("useUnscaledTime").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureSequence(UIMotionSequencePresenter sequence, List<SequenceEntrySpec> entries)
        {
            SerializedObject serializedObject = new SerializedObject(sequence);
            SerializedProperty entriesProperty = serializedObject.FindProperty("entries");
            entriesProperty.arraySize = entries.Count;

            for (int i = 0; i < entries.Count; i++)
            {
                SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(i);
                entryProperty.FindPropertyRelative("motionPresenter").objectReferenceValue = entries[i].MotionPresenter;
                entryProperty.FindPropertyRelative("motionId").stringValue = entries[i].MotionId;
                entryProperty.FindPropertyRelative("startDelaySeconds").floatValue = entries[i].StartDelaySeconds;
            }

            serializedObject.FindProperty("initialDelaySeconds").floatValue = 0.06f;
            serializedObject.FindProperty("playOnEnable").boolValue = true;
            serializedObject.FindProperty("replayOnEnable").boolValue = false;
            serializedObject.FindProperty("snapToEndOnDisable").boolValue = true;
            serializedObject.FindProperty("useUnscaledTime").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UIScrollRectMotionPresenter ConfigureScrollMotion(
            GameObject prefabRoot,
            string targetName,
            bool snapOnEndDrag,
            float focusDurationSeconds)
        {
            GameObject target = FindChild(prefabRoot, targetName);
            if (target == null)
            {
                throw new InvalidOperationException($"Stage select scroll setup could not find child '{targetName}'.");
            }

            ScrollRect scrollRect = target.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                throw new InvalidOperationException($"Stage select scroll target '{targetName}' must have a ScrollRect.");
            }

            UIScrollRectMotionPresenter motionPresenter = target.GetComponent<UIScrollRectMotionPresenter>();
            if (motionPresenter == null)
            {
                motionPresenter = target.AddComponent<UIScrollRectMotionPresenter>();
            }

            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.inertia = true;
            scrollRect.elasticity = 0.14f;
            scrollRect.decelerationRate = 0.18f;
            scrollRect.scrollSensitivity = 32f;

            SerializedObject serializedObject = new SerializedObject(motionPresenter);
            serializedObject.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            serializedObject.FindProperty("content").objectReferenceValue = scrollRect.content;
            serializedObject.FindProperty("viewport").objectReferenceValue = scrollRect.viewport;
            serializedObject.FindProperty("configurePhysics").boolValue = true;
            serializedObject.FindProperty("useUnscaledTime").boolValue = true;
            serializedObject.FindProperty("elasticity").floatValue = 0.14f;
            serializedObject.FindProperty("decelerationRate").floatValue = 0.18f;
            serializedObject.FindProperty("scrollSensitivity").floatValue = 32f;
            serializedObject.FindProperty("snapOnEndDrag").boolValue = snapOnEndDrag;
            serializedObject.FindProperty("viewportFocus").floatValue = 0.5f;
            serializedObject.FindProperty("focusDurationSeconds").floatValue = focusDurationSeconds;
            serializedObject.FindProperty("snapDelaySeconds").floatValue = 0.08f;
            serializedObject.FindProperty("snapVelocityThreshold").floatValue = 80f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return motionPresenter;
        }

        private static void ConfigureStageSelectPresenterFocus(
            GameObject prefabRoot,
            UIScrollRectMotionPresenter stageScrollMotion,
            UIScrollRectMotionPresenter chapterScrollMotion,
            UIStageCatalog stageCatalog)
        {
            StageSelectScreenPresenter presenter = prefabRoot.GetComponent<StageSelectScreenPresenter>();
            if (presenter == null)
            {
                throw new InvalidOperationException("Stage select scroll setup could not find StageSelectScreenPresenter on the prefab root.");
            }

            RectTransform selectedChapterTarget = RequireRectTransform(prefabRoot, "EP 01_SelectedChapterCard");
            ConfigureTruthfulChapterInventory(prefabRoot, selectedChapterTarget);
            Text combatLessonText = EnsureCombatLessonText(prefabRoot);
            Text rewardPreviewText = EnsureRewardPreviewText(prefabRoot);
            ConfigureTruthfulStageDetailLayout(prefabRoot, combatLessonText);
            ConfigureVisibleStartButton(prefabRoot);
            SerializedObject serializedObject = new SerializedObject(presenter);
            serializedObject.FindProperty("stageScrollMotion").objectReferenceValue = stageScrollMotion;
            serializedObject.FindProperty("chapterScrollMotion").objectReferenceValue = chapterScrollMotion;
            serializedObject.FindProperty("combatLessonText").objectReferenceValue = combatLessonText;
            serializedObject.FindProperty("rewardPreviewText").objectReferenceValue = rewardPreviewText;
            serializedObject.FindProperty("requireExactStageCardBindings").boolValue = true;
            serializedObject.FindProperty("focusSelectedStageOnEnable").boolValue = true;
            serializedObject.FindProperty("focusDelaySeconds").floatValue = 0.02f;
            serializedObject.FindProperty("initialFocusDurationSeconds").floatValue = 0.18f;
            serializedObject.FindProperty("selectedFocusDurationSeconds").floatValue = 0.3f;
            serializedObject.FindProperty("startRoute").intValue = (int)UIRouteId.Combat;

            if (!stageCatalog.TryValidateEntryIdentities(out UIStageRouteProjectionRejectReason rejectReason))
            {
                throw new InvalidOperationException(
                    $"{StageCatalogPath} entry identities are invalid: {rejectReason}.");
            }

            if (stageCatalog.StageCount > StageCardNames.Length)
            {
                throw new InvalidOperationException(
                    $"Stage select prefab exposes {StageCardNames.Length} card shells but the catalog has {stageCatalog.StageCount} entries.");
            }

            string firstStageId = RequireStageId(stageCatalog, 0);
            serializedObject.FindProperty("selectedStageId").stringValue = firstStageId;

            SerializedProperty entriesProperty = serializedObject.FindProperty("stageFocusEntries");
            entriesProperty.arraySize = stageCatalog.StageCount;
            for (int i = 0; i < StageCardNames.Length; i++)
            {
                RectTransform stageTarget = RequireRectTransform(prefabRoot, StageCardNames[i]);
                Button selectionButton = stageTarget.GetComponent<Button>();
                if (selectionButton == null)
                {
                    throw new InvalidOperationException(
                        $"Stage select card '{StageCardNames[i]}' must own a Button.");
                }

                bool bound = i < stageCatalog.StageCount;
                stageTarget.gameObject.SetActive(bound);
                selectionButton.enabled = true;
                selectionButton.interactable = bound;
                Graphic targetGraphic = selectionButton.targetGraphic
                    ?? stageTarget.GetComponent<Graphic>();
                if (targetGraphic == null)
                {
                    throw new InvalidOperationException(
                        $"Stage select card '{StageCardNames[i]}' requires one target Graphic.");
                }

                selectionButton.targetGraphic = targetGraphic;
                targetGraphic.raycastTarget = bound;
                CanvasGroup canvasGroup = stageTarget.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = bound;
                    canvasGroup.blocksRaycasts = bound;
                }

                if (!bound)
                {
                    continue;
                }

                ConfigureBoundStageCardTruth(
                    stageTarget,
                    GetStageNumberLabel(i),
                    stageCatalog.GetStage(i).DisplayName);

                ConfigureStageFocusEntry(
                    entriesProperty.GetArrayElementAtIndex(i),
                    RequireStageId(stageCatalog, i),
                    selectionButton,
                    stageTarget,
                    selectedChapterTarget);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            ConfigureExactRouteInteractableGate(prefabRoot, stageCatalog);
            ValidateStageCardTruth(prefabRoot, stageCatalog, firstStageId);
        }

        private static void ConfigureExactRouteInteractableGate(
            GameObject prefabRoot,
            UIStageCatalog stageCatalog)
        {
            UIRouteInteractableGate[] gates =
                prefabRoot.GetComponentsInChildren<UIRouteInteractableGate>(true);
            if (gates.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Stage select prefab requires exactly one route interactable gate, but found {gates.Length}.");
            }

            var expectedSelectables = new List<Selectable>
            {
                RequireButton(prefabRoot, "BackButton"),
                RequireButton(prefabRoot, "StartButton")
            };
            for (int i = 0; i < stageCatalog.StageCount; i++)
            {
                expectedSelectables.Add(RequireButton(prefabRoot, StageCardNames[i]));
            }

            SerializedObject serializedGate = new SerializedObject(gates[0]);
            SerializedProperty selectables = serializedGate.FindProperty("selectables");
            if (selectables == null || !selectables.isArray)
            {
                throw new InvalidOperationException(
                    "Stage select route interactable gate has no serialized Selectable array.");
            }

            selectables.arraySize = expectedSelectables.Count;
            for (int i = 0; i < expectedSelectables.Count; i++)
            {
                selectables.GetArrayElementAtIndex(i).objectReferenceValue = expectedSelectables[i];
            }

            serializedGate.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureBoundStageCardTruth(
            RectTransform stageCard,
            string stageNumber,
            string stageTitle)
        {
            if (string.IsNullOrWhiteSpace(stageTitle))
            {
                throw new InvalidOperationException(
                    $"Stage select bound card '{stageCard.name}' requires a non-empty catalog display name.");
            }

            Text numberText = RequireDescendantComponent<Text>(stageCard, "StageNumberText");
            Text titleText = RequireDescendantComponent<Text>(stageCard, "StageTitleText");
            ConfigureReadableStageCardText(numberText, stageNumber, isTitle: false);
            ConfigureReadableStageCardText(titleText, stageTitle, isTitle: true);

            HideUnauthoritativeText(stageCard, "StagePercentText");
            SetDescendantActive(stageCard, "Star1", false);
            SetDescendantActive(stageCard, "Star2", false);
            SetDescendantActive(stageCard, "Star3", false);
            SetOptionalDescendantActive(stageCard, "LockIcon", false);
        }

        private static void ConfigureReadableStageCardText(
            Text text,
            string value,
            bool isTitle)
        {
            RectTransform rect = text.rectTransform;
            rect.anchorMin = isTitle
                ? new Vector2(0.065f, 0.31f)
                : new Vector2(0.065f, 0.55f);
            rect.anchorMax = isTitle
                ? new Vector2(0.935f, 0.49f)
                : new Vector2(0.42f, 0.73f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            text.gameObject.SetActive(true);
            text.enabled = true;
            text.text = value;
            text.fontSize = isTitle ? 22 : 21;
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = isTitle ? 12 : 16;
            text.resizeTextMaxSize = isTitle ? 22 : 21;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.lineSpacing = 0.9f;
            text.color = Color.white;
            text.raycastTarget = false;

            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
            text.transform.SetAsLastSibling();
        }

        private static void ConfigureTruthfulChapterInventory(
            GameObject prefabRoot,
            RectTransform selectedChapterCard)
        {
            selectedChapterCard.gameObject.SetActive(true);
            Button selectedButton = selectedChapterCard.GetComponent<Button>();
            Graphic selectedTarget = selectedButton != null
                ? selectedButton.targetGraphic ?? selectedChapterCard.GetComponent<Graphic>()
                : null;
            if (selectedButton == null || selectedTarget == null)
            {
                throw new InvalidOperationException(
                    "Stage select selected EP 01 chapter card requires a Button and target Graphic.");
            }

            selectedButton.enabled = true;
            selectedButton.interactable = false;
            selectedButton.targetGraphic = selectedTarget;
            selectedTarget.raycastTarget = false;

            Text episode = RequireDescendantComponent<Text>(selectedChapterCard, "EpisodeText");
            Text title = RequireDescendantComponent<Text>(selectedChapterCard, "TitleText");
            Text percent = RequireDescendantComponent<Text>(selectedChapterCard, "PercentText");
            ConfigureReadableChapterText(episode, "EP 01", isTitle: false);
            ConfigureReadableChapterText(title, "차원 안정화", isTitle: true);
            percent.text = string.Empty;
            percent.gameObject.SetActive(false);

            for (int i = 0; i < PlaceholderChapterCardNames.Length; i++)
            {
                RectTransform placeholder = RequireRectTransform(
                    prefabRoot,
                    PlaceholderChapterCardNames[i]);
                Button button = placeholder.GetComponent<Button>();
                CanvasGroup canvasGroup = placeholder.GetComponent<CanvasGroup>();
                if (button == null)
                {
                    throw new InvalidOperationException(
                        $"Stage select placeholder chapter '{placeholder.name}' requires a Button.");
                }

                placeholder.gameObject.SetActive(false);
                button.interactable = false;
                if (button.targetGraphic != null)
                {
                    button.targetGraphic.raycastTarget = false;
                }

                if (canvasGroup != null)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }

        private static void ConfigureReadableChapterText(
            Text text,
            string value,
            bool isTitle)
        {
            text.gameObject.SetActive(true);
            text.enabled = true;
            text.text = value;
            text.fontSize = isTitle ? 25 : 18;
            text.fontStyle = isTitle ? FontStyle.Bold : FontStyle.Normal;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = isTitle ? 15 : 13;
            text.resizeTextMaxSize = isTitle ? 25 : 18;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = Color.white;
            text.raycastTarget = false;

            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
            text.transform.SetAsLastSibling();
        }

        private static void ValidateStageCardTruth(
            GameObject prefabRoot,
            UIStageCatalog stageCatalog,
            string expectedSelectedStageId)
        {
            StageSelectScreenPresenter presenter = prefabRoot.GetComponent<StageSelectScreenPresenter>();
            SerializedObject serializedPresenter = new SerializedObject(presenter);
            string selectedStageId = serializedPresenter.FindProperty("selectedStageId").stringValue;
            if (!string.Equals(selectedStageId, expectedSelectedStageId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Stage select selectedStageId must be the first catalog row '{expectedSelectedStageId}', but was '{selectedStageId}'.");
            }

            for (int i = 0; i < StageCardNames.Length; i++)
            {
                RectTransform stageCard = RequireRectTransform(prefabRoot, StageCardNames[i]);
                Button selectionButton = stageCard.GetComponent<Button>();
                CanvasGroup canvasGroup = stageCard.GetComponent<CanvasGroup>();
                bool bound = i < stageCatalog.StageCount;
                if (stageCard.gameObject.activeSelf != bound
                    || selectionButton == null
                    || selectionButton.interactable != bound
                    || (canvasGroup != null
                        && (canvasGroup.interactable != bound || canvasGroup.blocksRaycasts != bound)))
                {
                    throw new InvalidOperationException(
                        $"Stage select card '{StageCardNames[i]}' activation or interaction state does not match bound={bound}.");
                }

                if (!bound)
                {
                    continue;
                }

                string expectedNumber = GetStageNumberLabel(i);
                string expectedTitle = stageCatalog.GetStage(i).DisplayName;
                RequireExactText(stageCard, "StageNumberText", expectedNumber);
                RequireExactText(stageCard, "StageTitleText", expectedTitle);
                RequireReadableStageCardText(stageCard, "StageNumberText", isTitle: false);
                RequireReadableStageCardText(stageCard, "StageTitleText", isTitle: true);
                RequireInactiveDescendant(stageCard, "StagePercentText", requireEmptyText: true);
                RequireInactiveDescendant(stageCard, "Star1", requireEmptyText: false);
                RequireInactiveDescendant(stageCard, "Star2", requireEmptyText: false);
                RequireInactiveDescendant(stageCard, "Star3", requireEmptyText: false);
                RequireMissingOrInactiveDescendant(stageCard, "LockIcon");
            }

            GameObject rewardPreview = FindChild(prefabRoot, "CurrentChapterRewardText");
            Text rewardPreviewText = rewardPreview != null ? rewardPreview.GetComponent<Text>() : null;
            if (rewardPreview == null
                || rewardPreview.activeSelf
                || rewardPreviewText == null
                || !string.IsNullOrEmpty(rewardPreviewText.text))
            {
                throw new InvalidOperationException(
                    "Stage select CurrentChapterRewardText must remain inactive and empty until an authoritative reward preview exists.");
            }

            RequireHiddenUnauthoritativeDetailObject(
                prefabRoot,
                "ChapterProgressLabel",
                requireEmptyText: true);
            RequireHiddenUnauthoritativeDetailObject(
                prefabRoot,
                "ChapterPercentText",
                requireEmptyText: true);
            RequireHiddenUnauthoritativeDetailObject(
                prefabRoot,
                "ChapterProgress",
                requireEmptyText: false);
            RequireHiddenUnauthoritativeDetailObject(
                prefabRoot,
                "ChapterProgressBackground",
                requireEmptyText: false);
            RequireHiddenUnauthoritativeDetailObject(
                prefabRoot,
                "SummaryFrame",
                requireEmptyText: false);
            RequireHiddenUnauthoritativeDetailObject(
                prefabRoot,
                "SummaryText",
                requireEmptyText: true);
            ValidateTruthfulStageDetailLayout(prefabRoot);
            ValidateVisibleStartButton(prefabRoot);
            ValidateTruthfulChapterInventory(prefabRoot);
            ValidateExactRouteInteractableGate(prefabRoot, stageCatalog);
        }

        private static void ValidateExactRouteInteractableGate(
            GameObject prefabRoot,
            UIStageCatalog stageCatalog)
        {
            UIRouteInteractableGate[] gates =
                prefabRoot.GetComponentsInChildren<UIRouteInteractableGate>(true);
            if (gates.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Stage select prefab requires exactly one route interactable gate, but found {gates.Length}.");
            }

            var expected = new HashSet<Selectable>
            {
                RequireButton(prefabRoot, "BackButton"),
                RequireButton(prefabRoot, "StartButton")
            };
            for (int i = 0; i < stageCatalog.StageCount; i++)
            {
                expected.Add(RequireButton(prefabRoot, StageCardNames[i]));
            }

            SerializedObject serializedGate = new SerializedObject(gates[0]);
            SerializedProperty selectables = serializedGate.FindProperty("selectables");
            if (selectables == null
                || !selectables.isArray
                || selectables.arraySize != expected.Count)
            {
                throw new InvalidOperationException(
                    "Stage select route gate must bind exactly Back, Start, and every admitted stage card.");
            }

            for (int i = 0; i < selectables.arraySize; i++)
            {
                Selectable selectable =
                    selectables.GetArrayElementAtIndex(i).objectReferenceValue as Selectable;
                if (selectable == null || !expected.Remove(selectable))
                {
                    throw new InvalidOperationException(
                        "Stage select route gate contains a missing, duplicate, or non-product Selectable.");
                }
            }

            if (expected.Count != 0)
            {
                throw new InvalidOperationException(
                    "Stage select route gate is missing an admitted product control.");
            }
        }

        private static void RequireReadableStageCardText(
            RectTransform stageCard,
            string childName,
            bool isTitle)
        {
            Text text = RequireDescendantComponent<Text>(stageCard, childName);
            Outline outline = text.GetComponent<Outline>();
            if (!text.gameObject.activeSelf
                || !text.enabled
                || text.color.a <= 0.01f
                || !text.resizeTextForBestFit
                || text.resizeTextMinSize != (isTitle ? 12 : 16)
                || text.resizeTextMaxSize != (isTitle ? 22 : 21)
                || text.fontStyle != FontStyle.Bold
                || text.alignment != TextAnchor.MiddleLeft
                || text.horizontalOverflow != HorizontalWrapMode.Wrap
                || text.verticalOverflow != VerticalWrapMode.Truncate
                || text.raycastTarget
                || outline == null
                || outline.effectColor.a <= 0.01f)
            {
                throw new InvalidOperationException(
                    $"Stage select card '{stageCard.name}' child '{childName}' must be an active, outlined, wrapped best-fit label.");
            }
        }

        private static void ValidateTruthfulChapterInventory(GameObject prefabRoot)
        {
            RectTransform selected = RequireRectTransform(
                prefabRoot,
                "EP 01_SelectedChapterCard");
            Button selectedButton = selected.GetComponent<Button>();
            Text episode = RequireDescendantComponent<Text>(selected, "EpisodeText");
            Text title = RequireDescendantComponent<Text>(selected, "TitleText");
            Text percent = RequireDescendantComponent<Text>(selected, "PercentText");
            if (!selected.gameObject.activeSelf
                || selectedButton == null
                || !selectedButton.enabled
                || selectedButton.interactable
                || selectedButton.targetGraphic == null
                || selectedButton.targetGraphic.raycastTarget
                || !episode.gameObject.activeSelf
                || !string.Equals(episode.text, "EP 01", StringComparison.Ordinal)
                || !title.gameObject.activeSelf
                || !string.Equals(title.text, "차원 안정화", StringComparison.Ordinal)
                || percent.gameObject.activeSelf
                || !string.IsNullOrEmpty(percent.text))
            {
                throw new InvalidOperationException(
                    "Stage select must expose one non-clickable EP 01 / 차원 안정화 chapter card without false progress.");
            }

            for (int i = 0; i < PlaceholderChapterCardNames.Length; i++)
            {
                RectTransform placeholder = RequireRectTransform(
                    prefabRoot,
                    PlaceholderChapterCardNames[i]);
                Button button = placeholder.GetComponent<Button>();
                CanvasGroup canvasGroup = placeholder.GetComponent<CanvasGroup>();
                if (placeholder.gameObject.activeSelf
                    || button == null
                    || button.interactable
                    || (button.targetGraphic != null && button.targetGraphic.raycastTarget)
                    || (canvasGroup != null
                        && (canvasGroup.interactable || canvasGroup.blocksRaycasts)))
                {
                    throw new InvalidOperationException(
                        $"Stage select unadmitted chapter placeholder '{placeholder.name}' must be inactive and reject interaction.");
                }
            }
        }

        private static void ConfigureVisibleStartButton(GameObject prefabRoot)
        {
            GameObject startObject = RequireUniqueObjectInHierarchy(prefabRoot, "StartButton");
            Button startButton = startObject.GetComponent<Button>();
            CanvasGroup canvasGroup = startObject.GetComponent<CanvasGroup>();
            Graphic targetGraphic = startButton != null
                ? startButton.targetGraphic ?? startObject.GetComponent<Graphic>()
                : null;
            RectTransform startRect = startObject.GetComponent<RectTransform>();
            GameObject frameObject = startRect != null
                ? FindUniqueDescendantOrNull(startRect, "Frame")
                : null;
            Graphic frameGraphic = frameObject != null
                ? frameObject.GetComponent<Graphic>()
                : null;
            Text label = startRect != null
                ? RequireDescendantComponent<Text>(startRect, "StageStartText")
                : null;
            if (startButton == null
                || canvasGroup == null
                || targetGraphic == null
                || startRect == null
                || frameObject == null
                || frameGraphic == null
                || label == null)
            {
                throw new InvalidOperationException(
                    "Stage select Start button requires Button, CanvasGroup, target Graphic, frame Graphic, and label Text components.");
            }

            startObject.SetActive(true);
            startButton.enabled = true;
            startButton.interactable = true;
            startButton.targetGraphic = targetGraphic;
            targetGraphic.raycastTarget = true;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            frameObject.SetActive(true);
            frameGraphic.raycastTarget = false;
            Color frameColor = frameGraphic.color;
            frameColor.a = 1f;
            frameGraphic.color = frameColor;

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.1f, 0.15f);
            labelRect.anchorMax = new Vector2(0.9f, 0.85f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            label.gameObject.SetActive(true);
            label.text = "작전 시작";
            label.fontSize = 30;
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 20;
            label.resizeTextMaxSize = 30;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.lineSpacing = 1f;
            label.raycastTarget = false;
        }

        private static void ValidateVisibleStartButton(GameObject prefabRoot)
        {
            GameObject startObject = RequireUniqueObjectInHierarchy(prefabRoot, "StartButton");
            Button startButton = startObject.GetComponent<Button>();
            CanvasGroup canvasGroup = startObject.GetComponent<CanvasGroup>();
            RectTransform startRect = startObject.GetComponent<RectTransform>();
            GameObject frameObject = startRect != null
                ? FindUniqueDescendantOrNull(startRect, "Frame")
                : null;
            Graphic frameGraphic = frameObject != null
                ? frameObject.GetComponent<Graphic>()
                : null;
            Text label = startRect != null
                ? RequireDescendantComponent<Text>(startRect, "StageStartText")
                : null;
            if (!startObject.activeSelf
                || startButton == null
                || !startButton.enabled
                || !startButton.interactable
                || startButton.targetGraphic == null
                || !startButton.targetGraphic.raycastTarget
                || canvasGroup == null
                || canvasGroup.alpha < 0.99f
                || !canvasGroup.interactable
                || !canvasGroup.blocksRaycasts
                || frameObject == null
                || !frameObject.activeSelf
                || frameGraphic == null
                || frameGraphic.color.a <= 0.01f
                || label == null
                || !label.gameObject.activeSelf
                || !string.Equals(label.text, "작전 시작", StringComparison.Ordinal)
                || !label.resizeTextForBestFit
                || label.resizeTextMinSize != 20
                || label.resizeTextMaxSize != 30
                || label.fontStyle != FontStyle.Bold
                || label.alignment != TextAnchor.MiddleCenter
                || label.raycastTarget)
            {
                throw new InvalidOperationException(
                    "Stage select Start button must be an active, interactable, visibly framed control with one exact readable '작전 시작' label.");
            }
        }

        private static void RequireHiddenUnauthoritativeDetailObject(
            GameObject prefabRoot,
            string objectName,
            bool requireEmptyText)
        {
            GameObject target = RequireUniqueObjectInHierarchy(prefabRoot, objectName);
            Text text = target.GetComponent<Text>();
            if (target.activeSelf
                || (requireEmptyText && (text == null || !string.IsNullOrEmpty(text.text))))
            {
                throw new InvalidOperationException(
                    $"Stage select '{objectName}' must be inactive"
                    + (requireEmptyText ? " and have empty text." : "."));
            }
        }

        private static void ValidateTruthfulStageDetailLayout(GameObject prefabRoot)
        {
            Text titleText = RequireUniqueObjectInHierarchy(
                    prefabRoot,
                    "CurrentChapterTitleText")
                .GetComponent<Text>();
            Text objectiveText = RequireUniqueObjectInHierarchy(
                    prefabRoot,
                    "CurrentChapterBodyText")
                .GetComponent<Text>();
            Text lessonText = RequireUniqueObjectInHierarchy(
                    prefabRoot,
                    "CurrentChapterLessonText")
                .GetComponent<Text>();
            if (titleText == null || objectiveText == null || lessonText == null)
            {
                throw new InvalidOperationException(
                    "Stage select truthful detail layout requires title, objective, and lesson Text components.");
            }

            RectTransform titleRect = titleText.rectTransform;
            RectTransform objectiveRect = objectiveText.rectTransform;
            RectTransform lessonRect = lessonText.rectTransform;
            RectTransform numberRect = RequireRectTransform(
                prefabRoot,
                "CurrentChapterNumberText");
            RectTransform startRect = RequireRectTransform(prefabRoot, "StartButton");
            const float MinimumVerticalGap = 0.005f;
            if (!titleText.gameObject.activeSelf
                || !objectiveText.gameObject.activeSelf
                || !lessonText.gameObject.activeSelf
                || titleRect.parent != numberRect.parent
                || titleRect.parent != objectiveRect.parent
                || titleRect.parent != lessonRect.parent
                || titleRect.parent != startRect.parent
                || numberRect.anchorMin.y < titleRect.anchorMax.y + MinimumVerticalGap
                || titleRect.anchorMin.y < objectiveRect.anchorMax.y + MinimumVerticalGap
                || objectiveRect.anchorMin.y < lessonRect.anchorMax.y + MinimumVerticalGap
                || lessonRect.anchorMin.y < startRect.anchorMax.y + MinimumVerticalGap
                || !titleText.resizeTextForBestFit
                || !objectiveText.resizeTextForBestFit
                || !lessonText.resizeTextForBestFit
                || titleText.fontStyle != FontStyle.Bold
                || objectiveText.fontStyle != FontStyle.Normal
                || lessonText.fontStyle != FontStyle.Normal
                || titleText.raycastTarget
                || objectiveText.raycastTarget
                || lessonText.raycastTarget
                || titleText.horizontalOverflow != HorizontalWrapMode.Wrap
                || objectiveText.horizontalOverflow != HorizontalWrapMode.Wrap
                || lessonText.horizontalOverflow != HorizontalWrapMode.Wrap
                || titleText.verticalOverflow != VerticalWrapMode.Truncate
                || objectiveText.verticalOverflow != VerticalWrapMode.Truncate
                || lessonText.verticalOverflow != VerticalWrapMode.Truncate)
            {
                throw new InvalidOperationException(
                    "Stage select title, objective, and lesson must be active, wrapped, best-fit rows with non-overlapping vertical bounds.");
            }
        }

        private static void HideUnauthoritativeText(RectTransform stageCard, string childName)
        {
            Text text = RequireDescendantComponent<Text>(stageCard, childName);
            text.text = string.Empty;
            text.gameObject.SetActive(false);
        }

        private static void SetDescendantActive(
            RectTransform stageCard,
            string childName,
            bool active)
        {
            GameObject child = RequireUniqueDescendant(stageCard, childName);
            child.SetActive(active);
        }

        private static void SetOptionalDescendantActive(
            RectTransform stageCard,
            string childName,
            bool active)
        {
            GameObject child = FindUniqueDescendantOrNull(stageCard, childName);
            if (child != null)
            {
                child.SetActive(active);
            }
        }

        private static void RequireExactText(
            RectTransform stageCard,
            string childName,
            string expectedText)
        {
            Text text = RequireDescendantComponent<Text>(stageCard, childName);
            if (!string.Equals(text.text, expectedText, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Stage select card '{stageCard.name}' child '{childName}' must read '{expectedText}', but read '{text.text}'.");
            }
        }

        private static void RequireInactiveDescendant(
            RectTransform stageCard,
            string childName,
            bool requireEmptyText)
        {
            GameObject child = RequireUniqueDescendant(stageCard, childName);
            Text text = child.GetComponent<Text>();
            if (child.activeSelf || (requireEmptyText && (text == null || !string.IsNullOrEmpty(text.text))))
            {
                throw new InvalidOperationException(
                    $"Stage select card '{stageCard.name}' child '{childName}' must be inactive"
                    + (requireEmptyText ? " and have empty text." : "."));
            }
        }

        private static void RequireMissingOrInactiveDescendant(
            RectTransform stageCard,
            string childName)
        {
            GameObject child = FindUniqueDescendantOrNull(stageCard, childName);
            if (child != null && child.activeSelf)
            {
                throw new InvalidOperationException(
                    $"Stage select card '{stageCard.name}' child '{childName}' must be absent or inactive.");
            }
        }

        private static T RequireDescendantComponent<T>(
            RectTransform stageCard,
            string childName)
            where T : Component
        {
            GameObject child = RequireUniqueDescendant(stageCard, childName);
            T component = child.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Stage select card '{stageCard.name}' child '{childName}' must own {typeof(T).Name}.");
            }

            return component;
        }

        private static GameObject RequireUniqueDescendant(
            RectTransform stageCard,
            string childName)
        {
            GameObject match = FindUniqueDescendantOrNull(stageCard, childName);
            if (match == null)
            {
                throw new InvalidOperationException(
                    $"Stage select card '{stageCard.name}' could not find descendant '{childName}'.");
            }

            return match;
        }

        private static GameObject FindUniqueDescendantOrNull(
            RectTransform stageCard,
            string childName)
        {
            Transform[] descendants = stageCard.GetComponentsInChildren<Transform>(true);
            GameObject match = null;
            for (int i = 0; i < descendants.Length; i++)
            {
                if (!string.Equals(descendants[i].name, childName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (match != null)
                {
                    throw new InvalidOperationException(
                        $"Stage select card '{stageCard.name}' contains more than one '{childName}' descendant.");
                }

                match = descendants[i].gameObject;
            }
            return match;
        }

        private static string GetStageNumberLabel(int index)
        {
            const string suffix = "_StageCard";
            string shellName = StageCardNames[index];
            if (!shellName.EndsWith(suffix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Stage select card shell '{shellName}' must end with '{suffix}'.");
            }

            return shellName.Substring(0, shellName.Length - suffix.Length);
        }

        private static Text EnsureCombatLessonText(GameObject prefabRoot)
        {
            const string objectName = "CurrentChapterLessonText";
            GameObject lessonObject = FindChild(prefabRoot, objectName);
            if (lessonObject == null)
            {
                RectTransform parent = RequireRectTransform(prefabRoot, "StageSelectArtRoot");
                lessonObject = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                lessonObject.transform.SetParent(parent, false);
            }

            RectTransform lessonRect = lessonObject.GetComponent<RectTransform>();
            Text lessonText = lessonObject.GetComponent<Text>();
            Text bodyText = FindChild(prefabRoot, "CurrentChapterBodyText")?.GetComponent<Text>();
            if (lessonRect == null || lessonText == null || bodyText == null)
            {
                throw new InvalidOperationException(
                    "Stage select combat lesson requires a RectTransform, Text, and body-text style source.");
            }

            lessonRect.anchorMin = new Vector2(0.2421875f, 0.49f);
            lessonRect.anchorMax = new Vector2(0.45273438f, 0.58f);
            lessonRect.anchoredPosition = Vector2.zero;
            lessonRect.sizeDelta = Vector2.zero;
            lessonRect.pivot = new Vector2(0.5f, 0.5f);
            lessonText.font = bodyText.font;
            lessonText.fontSize = 17;
            lessonText.fontStyle = FontStyle.Normal;
            lessonText.resizeTextForBestFit = true;
            lessonText.resizeTextMinSize = 13;
            lessonText.resizeTextMaxSize = 17;
            lessonText.alignment = TextAnchor.UpperLeft;
            lessonText.horizontalOverflow = HorizontalWrapMode.Wrap;
            lessonText.verticalOverflow = VerticalWrapMode.Truncate;
            lessonText.lineSpacing = 0.95f;
            lessonText.color = new Color(0.78f, 0.84f, 0.92f, 1f);
            lessonText.raycastTarget = false;
            lessonText.text = string.Empty;
            lessonObject.SetActive(true);
            return lessonText;
        }

        private static void ConfigureTruthfulStageDetailLayout(
            GameObject prefabRoot,
            Text combatLessonText)
        {
            Text titleText = RequireRectTransform(prefabRoot, "CurrentChapterTitleText")
                .GetComponent<Text>();
            Text objectiveText = RequireRectTransform(prefabRoot, "CurrentChapterBodyText")
                .GetComponent<Text>();
            if (titleText == null || objectiveText == null || combatLessonText == null)
            {
                throw new InvalidOperationException(
                    "Stage select detail layout requires title, objective, and combat-lesson Text components.");
            }

            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.2421875f, 0.715f);
            titleRect.anchorMax = new Vector2(0.45273438f, 0.78f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = Vector2.zero;
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleText.gameObject.SetActive(true);
            titleText.fontSize = 34;
            titleText.fontStyle = FontStyle.Bold;
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 22;
            titleText.resizeTextMaxSize = 34;
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            titleText.verticalOverflow = VerticalWrapMode.Truncate;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.lineSpacing = 0.9f;
            titleText.raycastTarget = false;

            RectTransform objectiveRect = objectiveText.rectTransform;
            objectiveRect.anchorMin = new Vector2(0.2421875f, 0.59f);
            objectiveRect.anchorMax = new Vector2(0.45273438f, 0.705f);
            objectiveRect.anchoredPosition = Vector2.zero;
            objectiveRect.sizeDelta = Vector2.zero;
            objectiveRect.pivot = new Vector2(0.5f, 0.5f);
            objectiveText.gameObject.SetActive(true);
            objectiveText.fontSize = 20;
            objectiveText.fontStyle = FontStyle.Normal;
            objectiveText.resizeTextForBestFit = true;
            objectiveText.resizeTextMinSize = 15;
            objectiveText.resizeTextMaxSize = 20;
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;
            objectiveText.alignment = TextAnchor.UpperLeft;
            objectiveText.lineSpacing = 0.92f;
            objectiveText.raycastTarget = false;

            combatLessonText.gameObject.SetActive(true);
            HideUnauthoritativeDetailObject(prefabRoot, "ChapterProgressLabel");
            HideUnauthoritativeDetailObject(prefabRoot, "ChapterPercentText");
            HideUnauthoritativeDetailObject(prefabRoot, "ChapterProgress");
            HideUnauthoritativeDetailObject(prefabRoot, "ChapterProgressBackground");
            HideUnauthoritativeDetailObject(prefabRoot, "SummaryFrame");
            HideUnauthoritativeDetailObject(prefabRoot, "SummaryText");
        }

        private static void HideUnauthoritativeDetailObject(
            GameObject prefabRoot,
            string objectName)
        {
            GameObject target = RequireUniqueObjectInHierarchy(prefabRoot, objectName);

            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.text = string.Empty;
            }

            target.SetActive(false);
        }

        private static Text EnsureRewardPreviewText(GameObject prefabRoot)
        {
            const string objectName = "CurrentChapterRewardText";
            GameObject rewardObject = FindChild(prefabRoot, objectName);
            if (rewardObject == null)
            {
                RectTransform parent = RequireRectTransform(prefabRoot, "StageSelectArtRoot");
                rewardObject = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                rewardObject.transform.SetParent(parent, false);
            }

            RectTransform rewardRect = rewardObject.GetComponent<RectTransform>();
            Text rewardText = rewardObject.GetComponent<Text>();
            Text bodyText = FindChild(prefabRoot, "CurrentChapterBodyText")?.GetComponent<Text>();
            if (rewardRect == null || rewardText == null || bodyText == null)
            {
                throw new InvalidOperationException(
                    "Stage select reward preview requires a RectTransform, Text, and body-text style source.");
            }

            rewardRect.anchorMin = new Vector2(0.2421875f, 0.415f);
            rewardRect.anchorMax = new Vector2(0.385f, 0.452f);
            rewardRect.anchoredPosition = Vector2.zero;
            rewardRect.sizeDelta = Vector2.zero;
            rewardRect.pivot = new Vector2(0.5f, 0.5f);
            rewardText.font = bodyText.font;
            rewardText.fontSize = 18;
            rewardText.fontStyle = FontStyle.Bold;
            rewardText.alignment = TextAnchor.MiddleLeft;
            rewardText.color = new Color(0.72f, 0.82f, 0.95f, 1f);
            rewardText.raycastTarget = false;
            rewardText.text = string.Empty;
            rewardObject.SetActive(false);
            return rewardText;
        }

        private static GameObject RequireUniqueObjectInHierarchy(
            GameObject prefabRoot,
            string objectName)
        {
            Transform[] descendants = prefabRoot.GetComponentsInChildren<Transform>(true);
            GameObject match = null;
            int matchCount = 0;
            for (int i = 0; i < descendants.Length; i++)
            {
                Transform candidate = descendants[i];
                if (!string.Equals(candidate.name, objectName, StringComparison.Ordinal))
                {
                    continue;
                }

                match = candidate.gameObject;
                matchCount++;
            }

            if (matchCount != 1)
            {
                throw new InvalidOperationException(
                    $"Stage select requires exactly one '{objectName}' object, but found {matchCount}.");
            }

            return match;
        }

        private static string RequireStageId(UIStageCatalog stageCatalog, int index)
        {
            SerializedObject serializedObject = new SerializedObject(stageCatalog);
            SerializedProperty stages = serializedObject.FindProperty("stages");
            if (stages == null || !stages.isArray || stages.arraySize <= index)
            {
                throw new InvalidOperationException($"{StageCatalogPath} must include stage entry #{index} for stage select focus setup.");
            }

            string stageId = stages.GetArrayElementAtIndex(index).FindPropertyRelative("id").stringValue;
            if (string.IsNullOrWhiteSpace(stageId))
            {
                throw new InvalidOperationException($"{StageCatalogPath} stage entry #{index} must have an id.");
            }

            return stageId;
        }

        private static void ConfigureStageFocusEntry(
            SerializedProperty entryProperty,
            string stageId,
            Button selectionButton,
            RectTransform stageTarget,
            RectTransform chapterTarget)
        {
            entryProperty.FindPropertyRelative("stageId").stringValue = stageId;
            entryProperty.FindPropertyRelative("selectionButton").objectReferenceValue =
                selectionButton;
            entryProperty.FindPropertyRelative("stageTarget").objectReferenceValue = stageTarget;
            entryProperty.FindPropertyRelative("chapterTarget").objectReferenceValue = chapterTarget;
        }

        private static RectTransform RequireRectTransform(GameObject prefabRoot, string targetName)
        {
            GameObject target = FindChild(prefabRoot, targetName);
            if (target == null)
            {
                throw new InvalidOperationException($"Stage select setup could not find child '{targetName}'.");
            }

            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                throw new InvalidOperationException($"Stage select setup target '{targetName}' must have a RectTransform.");
            }

            return rectTransform;
        }

        private static Button RequireButton(GameObject prefabRoot, string targetName)
        {
            GameObject target = RequireUniqueObjectInHierarchy(prefabRoot, targetName);
            Button button = target.GetComponent<Button>();
            if (button == null)
            {
                throw new InvalidOperationException(
                    $"Stage select setup target '{targetName}' must have a Button.");
            }

            return button;
        }

        private static GameObject FindChild(GameObject root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (string.Equals(transforms[i].name, name, StringComparison.Ordinal))
                {
                    return transforms[i].gameObject;
                }
            }

            return null;
        }

        private readonly struct SequenceEntrySpec
        {
            public SequenceEntrySpec(UIMotionPresenter motionPresenter, string motionId, float startDelaySeconds)
            {
                MotionPresenter = motionPresenter;
                MotionId = motionId;
                StartDelaySeconds = startDelaySeconds;
            }

            public UIMotionPresenter MotionPresenter { get; }
            public string MotionId { get; }
            public float StartDelaySeconds { get; }
        }
    }
}
