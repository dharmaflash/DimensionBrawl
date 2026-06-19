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

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(StageSelectPrefabPath);
            try
            {
                List<SequenceEntrySpec> entries = new List<SequenceEntrySpec>();

                AddMotion(prefabRoot, "StageSelectArtRoot", "stage_screen_enter", 0f, motionCatalog, entries);
                AddMotion(prefabRoot, "BackButton", "stage_back_button_enter", 0.04f, motionCatalog, entries);
                AddMotion(prefabRoot, "ChapterScrollArea", "stage_chapter_list_enter", 0.08f, motionCatalog, entries);
                AddMotion(prefabRoot, "EP 01_ChapterCard", "stage_card_enter", 0.12f, motionCatalog, entries);
                AddMotion(prefabRoot, "EP 02_ChapterCard", "stage_card_enter", 0.15f, motionCatalog, entries);
                AddMotion(prefabRoot, "EP 03_SelectedChapterCard", "stage_card_enter", 0.18f, motionCatalog, entries);
                AddMotion(prefabRoot, "EP 04_ChapterCard", "stage_card_enter", 0.21f, motionCatalog, entries);
                AddMotion(prefabRoot, "SelectedPanel", "stage_feature_panel_enter", 0.12f, motionCatalog, entries);
                AddMotion(prefabRoot, "CurrentChapterFrame", "stage_feature_panel_enter", 0.16f, motionCatalog, entries);
                AddMotion(prefabRoot, "StageScrollArea", "stage_stage_grid_enter", 0.22f, motionCatalog, entries);
                AddMotion(prefabRoot, "03-1_StageCard", "stage_card_enter", 0.27f, motionCatalog, entries);
                AddMotion(prefabRoot, "03-2_StageCard", "stage_card_enter", 0.31f, motionCatalog, entries);
                AddMotion(prefabRoot, "03-3_StageCard", "stage_card_enter", 0.35f, motionCatalog, entries);
                AddMotion(prefabRoot, "03-4_StageCard", "stage_card_enter", 0.39f, motionCatalog, entries);
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
                ConfigureStageSelectPresenterFocus(prefabRoot, stageScrollMotion, chapterScrollMotion);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, StageSelectPrefabPath);
                AssetDatabase.SaveAssets();
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
            UIScrollRectMotionPresenter chapterScrollMotion)
        {
            StageSelectScreenPresenter presenter = prefabRoot.GetComponent<StageSelectScreenPresenter>();
            if (presenter == null)
            {
                throw new InvalidOperationException("Stage select scroll setup could not find StageSelectScreenPresenter on the prefab root.");
            }

            RectTransform selectedChapterTarget = RequireRectTransform(prefabRoot, "EP 03_SelectedChapterCard");
            SerializedObject serializedObject = new SerializedObject(presenter);
            serializedObject.FindProperty("stageScrollMotion").objectReferenceValue = stageScrollMotion;
            serializedObject.FindProperty("chapterScrollMotion").objectReferenceValue = chapterScrollMotion;
            serializedObject.FindProperty("focusSelectedStageOnEnable").boolValue = true;
            serializedObject.FindProperty("focusDelaySeconds").floatValue = 0.02f;
            serializedObject.FindProperty("initialFocusDurationSeconds").floatValue = 0.18f;
            serializedObject.FindProperty("selectedFocusDurationSeconds").floatValue = 0.3f;

            SerializedProperty entriesProperty = serializedObject.FindProperty("stageFocusEntries");
            entriesProperty.arraySize = 2;
            ConfigureStageFocusEntry(
                entriesProperty.GetArrayElementAtIndex(0),
                "story_v1_training_route",
                RequireRectTransform(prefabRoot, "03-3_StageCard"),
                selectedChapterTarget);
            ConfigureStageFocusEntry(
                entriesProperty.GetArrayElementAtIndex(1),
                "story_v1_retry_route",
                RequireRectTransform(prefabRoot, "03-2_StageCard"),
                selectedChapterTarget);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureStageFocusEntry(
            SerializedProperty entryProperty,
            string stageId,
            RectTransform stageTarget,
            RectTransform chapterTarget)
        {
            entryProperty.FindPropertyRelative("stageId").stringValue = stageId;
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
