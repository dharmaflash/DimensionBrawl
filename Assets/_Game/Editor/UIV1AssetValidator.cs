using System;
using System.Collections.Generic;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class UIV1AssetValidator
    {
        private const string RouteTablePath = "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string ScreenCatalogPath = "Assets/_Game/DesignData/UI/DB_UIScreenCatalog.asset";
        private const string ComponentCatalogPath = "Assets/_Game/DesignData/UI/DB_UIComponentCatalog.asset";
        private const string LoadingDeckPath = "Assets/_Game/DesignData/UI/DB_UILoadingCards.asset";
        private const string StageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string SoundContextCatalogPath = "Assets/_Game/DesignData/UI/DB_UISoundContexts.asset";
        private const string CueBundleCatalogPath = "Assets/_Game/DesignData/UI/DB_UICueBundles.asset";
        private const string LobbyFeedbackCatalogPath = "Assets/_Game/DesignData/UI/DB_LobbyGuideFeedback.asset";
        private const string CombatHudActionCatalogPath = "Assets/_Game/DesignData/UI/DB_CombatHudActions.asset";
        private const string ThemeCatalogPath = "Assets/_Game/DesignData/UI/DB_UIThemeCatalog.asset";
        private const string TextCatalogPath = "Assets/_Game/DesignData/UI/DB_UITextCatalog.asset";
        private const string InputPromptCatalogPath = "Assets/_Game/DesignData/UI/DB_UIInputPrompts.asset";
        private const string PanelCatalogPath = "Assets/_Game/DesignData/UI/DB_UIPanelCatalog.asset";
        private const string ResponsiveLayoutCatalogPath = "Assets/_Game/DesignData/UI/DB_UIResponsiveLayouts.asset";
        private const string AccessibilityCatalogPath = "Assets/_Game/DesignData/UI/DB_UIAccessibility.asset";
        private const string IconCatalogPath = "Assets/_Game/DesignData/UI/DB_UIIconCatalog.asset";
        private const string MotionCatalogPath = "Assets/_Game/DesignData/UI/DB_UIMotionCatalog.asset";
        private const string LayerCatalogPath = "Assets/_Game/DesignData/UI/DB_UILayers.asset";
        private const string ButtonStateCatalogPath = "Assets/_Game/DesignData/UI/DB_UIButtonStates.asset";
        private const string StateMessageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStateMessages.asset";
        private const string ToastCatalogPath = "Assets/_Game/DesignData/UI/DB_UIToasts.asset";
        private const string TooltipCatalogPath = "Assets/_Game/DesignData/UI/DB_UITooltips.asset";
        private const string DialogCatalogPath = "Assets/_Game/DesignData/UI/DB_UIDialogs.asset";
        private const string ResultPreviewCatalogPath = "Assets/_Game/DesignData/UI/DB_UIResultPreviews.asset";
        private const string LoginScreenPrefabPath = "Assets/_Game/UI/Login/PF_UI_LoginScreen.prefab";
        private const string LoginAccountServerToastId = "Login.AccountServerNotice";
        private const string UiSceneRoot = "Assets/_Game/Scenes/UI/";
        private const string ImportedRoot = "Assets/_Imported/";
        private const string ActionFoundationScenePath = "Assets/_Game/Scenes/ActionFoundationTest.unity";

        [MenuItem("DimensionBrawl/UI V1/Validate UI Test Assets")]
        public static void ValidateMenu()
        {
            ValidateUiAssets();
        }

        public static void ValidateUiAssets()
        {
            RequireAsset<UIScreenRouteTable>(RouteTablePath);
            RequireAsset<UIScreenCatalog>(ScreenCatalogPath);
            RequireAsset<UIComponentCatalog>(ComponentCatalogPath);
            RequireAsset<UILoadingCardDeck>(LoadingDeckPath);
            RequireAsset<UIStageCatalog>(StageCatalogPath);
            RequireAsset<UISoundContextCatalog>(SoundContextCatalogPath);
            RequireAsset<UICueBundleCatalog>(CueBundleCatalogPath);
            RequireAsset<LobbyGuideFeedbackCatalog>(LobbyFeedbackCatalogPath);
            RequireAsset<CombatHudActionCatalog>(CombatHudActionCatalogPath);
            RequireAsset<UIThemeCatalog>(ThemeCatalogPath);
            RequireAsset<UITextCatalog>(TextCatalogPath);
            RequireAsset<UIInputPromptCatalog>(InputPromptCatalogPath);
            RequireAsset<UIPanelCatalog>(PanelCatalogPath);
            RequireAsset<UIResponsiveLayoutCatalog>(ResponsiveLayoutCatalogPath);
            RequireAsset<UIAccessibilityCatalog>(AccessibilityCatalogPath);
            RequireAsset<UIIconCatalog>(IconCatalogPath);
            RequireAsset<UIMotionCatalog>(MotionCatalogPath);
            RequireAsset<UILayerCatalog>(LayerCatalogPath);
            RequireAsset<UIButtonStateCatalog>(ButtonStateCatalogPath);
            RequireAsset<UIStateMessageCatalog>(StateMessageCatalogPath);
            RequireAsset<UIToastCatalog>(ToastCatalogPath);
            RequireAsset<UITooltipCatalog>(TooltipCatalogPath);
            RequireAsset<UIDialogCatalog>(DialogCatalogPath);
            RequireAsset<UIResultPreviewCatalog>(ResultPreviewCatalogPath);

            HashSet<string> loadingCardIds = CollectStringKeys(LoadingDeckPath, "cards", "id");
            HashSet<UIRouteId> routeIds = ValidateRouteTable(loadingCardIds);
            ValidateObjectReferenceArray(ScreenCatalogPath, "screens", "screenPrefab");
            ValidateObjectReferenceArray(ComponentCatalogPath, "components", "prefab");
            ValidateObjectReferenceArray(PanelCatalogPath, "panels", "prefab");
            ValidateOptionalObjectReferenceArray(IconCatalogPath, "icons", "sprite");
            ValidateLoadingCards();
            HashSet<string> soundContextIds = CollectStringKeys(SoundContextCatalogPath, "contexts", "id");
            ValidateScreenCatalog(routeIds, soundContextIds);
            ValidateComponentCatalog();
            CollectStringKeys(DialogCatalogPath, "dialogs", "id");
            ValidateDialogs();
            ValidateResponsiveLayoutCatalog();
            ValidateCombatHudActionCatalog();
            ValidateInputPromptCatalog();
            HashSet<string> themeColorKeys = CollectStringKeys(ThemeCatalogPath, "colors", "key");
            HashSet<string> themeTextStyleKeys = CollectStringKeys(ThemeCatalogPath, "textStyles", "key");
            CollectStringKeys(ThemeCatalogPath, "spacing", "key");
            ValidateNonEmptyArrays(
                LoadingDeckPath,
                StageCatalogPath,
                SoundContextCatalogPath,
                CueBundleCatalogPath,
                LobbyFeedbackCatalogPath,
                CombatHudActionCatalogPath,
                ThemeCatalogPath,
                TextCatalogPath,
                InputPromptCatalogPath,
                PanelCatalogPath,
                ResponsiveLayoutCatalogPath,
                AccessibilityCatalogPath,
                IconCatalogPath,
                MotionCatalogPath,
                LayerCatalogPath,
                ButtonStateCatalogPath,
                StateMessageCatalogPath,
                ToastCatalogPath,
                TooltipCatalogPath,
                DialogCatalogPath,
                ResultPreviewCatalogPath);
            HashSet<string> textKeys = CollectStringKeys(TextCatalogPath, "entries", "key");
            ValidateTextKeyReferences(textKeys, AccessibilityCatalogPath, "entries", "readableLabelKey", "narrationKey");
            ValidateTextKeyReferences(textKeys, ButtonStateCatalogPath, "states", "labelKey", "tooltipTextKey");
            ValidateTextKeyReferences(textKeys, StateMessageCatalogPath, "states", "titleTextKey", "bodyTextKey", "actionTextKey");
            ValidateTextKeyReferences(textKeys, ToastCatalogPath, "toasts", "messageTextKey");
            ValidateTextKeyReferences(textKeys, TooltipCatalogPath, "tooltips", "titleTextKey", "bodyTextKey");
            ValidateTextKeyReferences(textKeys, DialogCatalogPath, "dialogs", "titleTextKey", "bodyTextKey", "confirmTextKey", "cancelTextKey");
            ValidateTextKeyReferences(textKeys, ResultPreviewCatalogPath, "results", "titleTextKey", "summaryTextKey", "primaryActionTextKey", "secondaryActionTextKey");
            HashSet<string> iconKeys = CollectStringKeys(IconCatalogPath, "icons", "key");
            ValidateKnownStringReferences(iconKeys, InputPromptCatalogPath, "prompts", "iconKey");
            ValidateKnownStringReferences(iconKeys, StateMessageCatalogPath, "states", "iconKey");
            ValidateKnownStringReferences(iconKeys, ToastCatalogPath, "toasts", "iconKey");
            ValidateKnownStringReferences(iconKeys, TooltipCatalogPath, "tooltips", "iconKey");
            ValidateKnownStringReferences(iconKeys, DialogCatalogPath, "dialogs", "iconKey");
            HashSet<string> motionIds = CollectStringKeys(MotionCatalogPath, "motions", "id");
            HashSet<string> stageIds = CollectStringKeys(StageCatalogPath, "stages", "id");
            ValidateRouteMotionReferences(motionIds);
            ValidateCueMotionReferences(motionIds);
            HashSet<string> cueIds = CollectStringKeys(CueBundleCatalogPath, "cueBundles", "id");
            ValidateKnownStringReferences(cueIds, ResultPreviewCatalogPath, "results", "cueId");
            HashSet<string> toastIds = CollectStringKeys(ToastCatalogPath, "toasts", "id");
            HashSet<string> resultIds = CollectStringKeys(ResultPreviewCatalogPath, "results", "id");
            HashSet<string> panelIds = CollectStringKeys(PanelCatalogPath, "panels", "panelId");
            ValidatePanelRoots(panelIds);
            ValidatePanelRouters(panelIds);
            ValidatePanelRequestButtons(panelIds);
            ValidateKnownPrefabs(themeColorKeys, themeTextStyleKeys, motionIds, cueIds, loadingCardIds, toastIds, resultIds, routeIds, stageIds);
            ValidateUiScenes(routeIds, toastIds, stageIds);
            UIV1BuildSettingsReadinessReporter.ReportCurrentReadiness();
            Debug.Log("UI V1 asset validation passed.");
        }

        private static HashSet<UIRouteId> ValidateRouteTable(HashSet<string> loadingCardIds)
        {
            UnityEngine.Object routeTable = RequireAsset<UIScreenRouteTable>(RouteTablePath);
            SerializedObject serializedObject = new SerializedObject(routeTable);
            SerializedProperty routes = serializedObject.FindProperty("routes");
            if (routes == null || !routes.isArray || routes.arraySize < 4)
            {
                throw new InvalidOperationException("UI route table must contain Login, Lobby, StageSelect, and CombatHud routes.");
            }

            HashSet<UIRouteId> foundRoutes = new HashSet<UIRouteId>();
            for (int i = 0; i < routes.arraySize; i++)
            {
                SerializedProperty route = routes.GetArrayElementAtIndex(i);
                UIRouteId routeId = (UIRouteId)route.FindPropertyRelative("routeId").intValue;
                string sceneName = route.FindPropertyRelative("sceneName").stringValue;
                string scenePath = route.FindPropertyRelative("scenePath").stringValue;
                string loadingCardId = route.FindPropertyRelative("loadingCardId").stringValue;
                bool useAsyncLoading = route.FindPropertyRelative("useAsyncLoading").boolValue;
                float minimumLoadingSeconds = route.FindPropertyRelative("minimumLoadingSeconds").floatValue;

                if (routeId == UIRouteId.None)
                {
                    throw new InvalidOperationException($"{RouteTablePath}.routes[{i}].routeId must not be None.");
                }

                if (!foundRoutes.Add(routeId))
                {
                    throw new InvalidOperationException($"{RouteTablePath}.routes[{i}].routeId duplicates {routeId}.");
                }

                RequireNonEmpty(sceneName, $"{RouteTablePath}.routes[{i}].sceneName");
                RequireNonEmpty(loadingCardId, $"{RouteTablePath}.routes[{i}].loadingCardId");
                RequireScenePath(scenePath, $"{RouteTablePath}.routes[{i}].scenePath");
                RequireSceneNameMatchesPath(sceneName, scenePath, $"{RouteTablePath}.routes[{i}]");
                RequireKnownKey(loadingCardIds, loadingCardId, $"{RouteTablePath}.routes[{i}].loadingCardId");
                RequireTrue(useAsyncLoading, $"{RouteTablePath}.routes[{i}].useAsyncLoading");
                RequireSceneFadeDuration(minimumLoadingSeconds, $"{RouteTablePath}.routes[{i}].minimumLoadingSeconds");
            }

            RequireRoute(foundRoutes, UIRouteId.Login);
            RequireRoute(foundRoutes, UIRouteId.Lobby);
            RequireRoute(foundRoutes, UIRouteId.StageSelect);
            RequireRoute(foundRoutes, UIRouteId.CombatHud);
            return foundRoutes;
        }

        private static void ValidateLoadingCards()
        {
            UnityEngine.Object deck = RequireAsset<UILoadingCardDeck>(LoadingDeckPath);
            SerializedObject serializedObject = new SerializedObject(deck);
            SerializedProperty cards = serializedObject.FindProperty("cards");
            if (cards == null || !cards.isArray || cards.arraySize == 0)
            {
                throw new InvalidOperationException($"{LoadingDeckPath}.cards must not be empty.");
            }

            int totalPositiveWeight = 0;
            for (int i = 0; i < cards.arraySize; i++)
            {
                SerializedProperty card = cards.GetArrayElementAtIndex(i);
                RequireNonEmpty(card.FindPropertyRelative("title").stringValue, $"{LoadingDeckPath}.cards[{i}].title");
                RequireNonEmpty(card.FindPropertyRelative("description").stringValue, $"{LoadingDeckPath}.cards[{i}].description");
                totalPositiveWeight += Mathf.Max(0, card.FindPropertyRelative("weight").intValue);
            }

            if (totalPositiveWeight <= 0)
            {
                throw new InvalidOperationException($"{LoadingDeckPath}.cards must contain at least one positively weighted card.");
            }
        }

        private static void ValidateScreenCatalog(HashSet<UIRouteId> routeIds, HashSet<string> soundContextIds)
        {
            UnityEngine.Object catalog = RequireAsset<UIScreenCatalog>(ScreenCatalogPath);
            SerializedObject serializedObject = new SerializedObject(catalog);
            SerializedProperty screens = serializedObject.FindProperty("screens");
            if (screens == null || !screens.isArray || screens.arraySize == 0)
            {
                throw new InvalidOperationException($"{ScreenCatalogPath}.screens must not be empty.");
            }

            HashSet<UIRouteId> foundScreenRoutes = new HashSet<UIRouteId>();
            HashSet<string> screenIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < screens.arraySize; i++)
            {
                SerializedProperty screen = screens.GetArrayElementAtIndex(i);
                UIRouteId routeId = (UIRouteId)screen.FindPropertyRelative("routeId").intValue;
                string screenId = screen.FindPropertyRelative("screenId").stringValue;
                string bgmContextId = screen.FindPropertyRelative("bgmContextId").stringValue;
                SerializedProperty presentationPrefab = screen.FindPropertyRelative("presentationPrefab");

                if (routeId == UIRouteId.None)
                {
                    throw new InvalidOperationException($"{ScreenCatalogPath}.screens[{i}].routeId must not be None.");
                }

                if (!foundScreenRoutes.Add(routeId))
                {
                    throw new InvalidOperationException($"{ScreenCatalogPath}.screens[{i}].routeId duplicates {routeId}.");
                }

                if (!routeIds.Contains(routeId))
                {
                    throw new InvalidOperationException($"{ScreenCatalogPath}.screens[{i}].routeId references missing route {routeId}.");
                }

                RequireNonEmpty(screenId, $"{ScreenCatalogPath}.screens[{i}].screenId");
                if (!screenIds.Add(screenId))
                {
                    throw new InvalidOperationException($"{ScreenCatalogPath}.screens[{i}].screenId duplicates {screenId}.");
                }

                RequireKnownKey(soundContextIds, bgmContextId, $"{ScreenCatalogPath}.screens[{i}].bgmContextId");
                if (presentationPrefab == null || presentationPrefab.objectReferenceValue == null)
                {
                    throw new InvalidOperationException($"{ScreenCatalogPath}.screens[{i}].presentationPrefab must be assigned.");
                }

                string presentationPath = AssetDatabase.GetAssetPath(presentationPrefab.objectReferenceValue);
                RequireGameOwnedPath(presentationPath, $"{ScreenCatalogPath}.screens[{i}].presentationPrefab");
            }

            foreach (UIRouteId routeId in routeIds)
            {
                if (!foundScreenRoutes.Contains(routeId))
                {
                    throw new InvalidOperationException($"{ScreenCatalogPath}.screens is missing screen data for {routeId}.");
                }
            }
        }

        private static void ValidateComponentCatalog()
        {
            UnityEngine.Object catalog = RequireAsset<UIComponentCatalog>(ComponentCatalogPath);
            SerializedObject serializedObject = new SerializedObject(catalog);
            SerializedProperty components = serializedObject.FindProperty("components");
            if (components == null || !components.isArray || components.arraySize == 0)
            {
                throw new InvalidOperationException($"{ComponentCatalogPath}.components must not be empty.");
            }

            HashSet<string> componentKeys = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < components.arraySize; i++)
            {
                SerializedProperty component = components.GetArrayElementAtIndex(i);
                string key = component.FindPropertyRelative("key").stringValue;
                string category = component.FindPropertyRelative("category").stringValue;

                RequireNonEmpty(key, $"{ComponentCatalogPath}.components[{i}].key");
                RequireNonEmpty(category, $"{ComponentCatalogPath}.components[{i}].category");
                if (!componentKeys.Add(key))
                {
                    throw new InvalidOperationException($"{ComponentCatalogPath}.components[{i}].key duplicates {key}.");
                }
            }
        }

        private static void ValidateDialogs()
        {
            UnityEngine.Object catalog = RequireAsset<UIDialogCatalog>(DialogCatalogPath);
            SerializedObject serializedObject = new SerializedObject(catalog);
            SerializedProperty dialogs = serializedObject.FindProperty("dialogs");
            if (dialogs == null || !dialogs.isArray || dialogs.arraySize == 0)
            {
                throw new InvalidOperationException($"{DialogCatalogPath}.dialogs must not be empty.");
            }

            for (int i = 0; i < dialogs.arraySize; i++)
            {
                SerializedProperty dialog = dialogs.GetArrayElementAtIndex(i);
                RequireNonEmpty(dialog.FindPropertyRelative("titleTextKey").stringValue, $"{DialogCatalogPath}.dialogs[{i}].titleTextKey");
                RequireNonEmpty(dialog.FindPropertyRelative("bodyTextKey").stringValue, $"{DialogCatalogPath}.dialogs[{i}].bodyTextKey");
                RequireNonEmpty(dialog.FindPropertyRelative("confirmTextKey").stringValue, $"{DialogCatalogPath}.dialogs[{i}].confirmTextKey");
                RequireNonEmpty(dialog.FindPropertyRelative("iconKey").stringValue, $"{DialogCatalogPath}.dialogs[{i}].iconKey");

                bool cancelVisible = dialog.FindPropertyRelative("cancelVisible").boolValue;
                string cancelTextKey = dialog.FindPropertyRelative("cancelTextKey").stringValue;
                if (cancelVisible && string.IsNullOrWhiteSpace(cancelTextKey))
                {
                    throw new InvalidOperationException($"{DialogCatalogPath}.dialogs[{i}].cancelTextKey is required when cancelVisible is true.");
                }
            }
        }

        private static void ValidateResponsiveLayoutCatalog()
        {
            UnityEngine.Object catalog = RequireAsset<UIResponsiveLayoutCatalog>(ResponsiveLayoutCatalogPath);
            SerializedObject serializedObject = new SerializedObject(catalog);
            SerializedProperty breakpoints = serializedObject.FindProperty("breakpoints");
            if (breakpoints == null || !breakpoints.isArray || breakpoints.arraySize == 0)
            {
                throw new InvalidOperationException($"{ResponsiveLayoutCatalogPath}.breakpoints must not be empty.");
            }

            bool foundAndroidLandscape = false;
            for (int i = 0; i < breakpoints.arraySize; i++)
            {
                SerializedProperty breakpoint = breakpoints.GetArrayElementAtIndex(i);
                string id = breakpoint.FindPropertyRelative("id").stringValue;
                Vector2 referenceResolution = breakpoint.FindPropertyRelative("referenceResolution").vector2Value;
                float matchWidthOrHeight = breakpoint.FindPropertyRelative("matchWidthOrHeight").floatValue;
                UISafeAreaMode safeAreaMode = (UISafeAreaMode)breakpoint.FindPropertyRelative("safeAreaMode").intValue;

                RequireNonEmpty(id, $"{ResponsiveLayoutCatalogPath}.breakpoints[{i}].id");
                if (referenceResolution.x <= referenceResolution.y)
                {
                    throw new InvalidOperationException($"{ResponsiveLayoutCatalogPath}.breakpoints[{i}] must use a landscape reference resolution.");
                }

                if (referenceResolution.x < 1280f || referenceResolution.y < 720f)
                {
                    throw new InvalidOperationException($"{ResponsiveLayoutCatalogPath}.breakpoints[{i}] reference resolution is below the mobile landscape floor.");
                }

                if (matchWidthOrHeight < 0.35f || matchWidthOrHeight > 0.75f)
                {
                    throw new InvalidOperationException($"{ResponsiveLayoutCatalogPath}.breakpoints[{i}].matchWidthOrHeight must stay mobile-landscape friendly.");
                }

                if (safeAreaMode == UISafeAreaMode.None)
                {
                    throw new InvalidOperationException($"{ResponsiveLayoutCatalogPath}.breakpoints[{i}] must opt into a Safe Area mode.");
                }

                foundAndroidLandscape |= string.Equals(id, "AndroidLandscape", StringComparison.Ordinal);
            }

            if (!foundAndroidLandscape)
            {
                throw new InvalidOperationException($"{ResponsiveLayoutCatalogPath} must include an AndroidLandscape breakpoint.");
            }
        }

        private static void ValidateCombatHudActionCatalog()
        {
            UnityEngine.Object catalog = RequireAsset<CombatHudActionCatalog>(CombatHudActionCatalogPath);
            SerializedObject serializedObject = new SerializedObject(catalog);
            SerializedProperty actions = serializedObject.FindProperty("actions");
            if (actions == null || !actions.isArray || actions.arraySize == 0)
            {
                throw new InvalidOperationException($"{CombatHudActionCatalogPath}.actions must not be empty.");
            }

            HashSet<CombatHudActionId> foundActions = new HashSet<CombatHudActionId>();
            for (int i = 0; i < actions.arraySize; i++)
            {
                SerializedProperty action = actions.GetArrayElementAtIndex(i);
                CombatHudActionId actionId = (CombatHudActionId)action.FindPropertyRelative("actionId").intValue;
                string displayName = action.FindPropertyRelative("displayName").stringValue;
                string canonicalName = action.FindPropertyRelative("canonicalName").stringValue;
                string placeholderState = action.FindPropertyRelative("placeholderState").stringValue;

                if (actionId == CombatHudActionId.None)
                {
                    throw new InvalidOperationException($"{CombatHudActionCatalogPath}.actions[{i}].actionId must not be None.");
                }

                if (!foundActions.Add(actionId))
                {
                    throw new InvalidOperationException($"{CombatHudActionCatalogPath}.actions[{i}] duplicates {actionId}.");
                }

                RequireNonEmpty(displayName, $"{CombatHudActionCatalogPath}.actions[{i}].displayName");
                RequireNonEmpty(placeholderState, $"{CombatHudActionCatalogPath}.actions[{i}].placeholderState");
                if (!string.Equals(canonicalName, actionId.ToString(), StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{CombatHudActionCatalogPath}.actions[{i}].canonicalName must match {actionId}.");
                }
            }

            RequireCombatHudAction(foundActions, CombatHudActionId.Move);
            RequireCombatHudAction(foundActions, CombatHudActionId.Look);
            RequireCombatHudAction(foundActions, CombatHudActionId.TargetBias);
            RequireCombatHudAction(foundActions, CombatHudActionId.BasicAttack);
            RequireCombatHudAction(foundActions, CombatHudActionId.Dodge);
            RequireCombatHudAction(foundActions, CombatHudActionId.Skill1);
            RequireCombatHudAction(foundActions, CombatHudActionId.Ultimate);
            RequireCombatHudAction(foundActions, CombatHudActionId.SummonSlot1);
            RequireCombatHudAction(foundActions, CombatHudActionId.SummonSlot2);
            RequireCombatHudAction(foundActions, CombatHudActionId.SummonSlot3);
            RequireCombatHudAction(foundActions, CombatHudActionId.Pause);
        }

        private static void ValidateInputPromptCatalog()
        {
            UnityEngine.Object catalog = RequireAsset<UIInputPromptCatalog>(InputPromptCatalogPath);
            SerializedObject serializedObject = new SerializedObject(catalog);
            SerializedProperty prompts = serializedObject.FindProperty("prompts");
            if (prompts == null || !prompts.isArray || prompts.arraySize == 0)
            {
                throw new InvalidOperationException($"{InputPromptCatalogPath}.prompts must not be empty.");
            }

            HashSet<string> promptPairs = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> actionNames = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < prompts.arraySize; i++)
            {
                SerializedProperty prompt = prompts.GetArrayElementAtIndex(i);
                string actionName = prompt.FindPropertyRelative("actionName").stringValue;
                UIInputPromptDevice device = (UIInputPromptDevice)prompt.FindPropertyRelative("device").intValue;
                string promptLabel = prompt.FindPropertyRelative("promptLabel").stringValue;
                string iconKey = prompt.FindPropertyRelative("iconKey").stringValue;
                string pairKey = $"{actionName}:{device}";

                RequireNonEmpty(actionName, $"{InputPromptCatalogPath}.prompts[{i}].actionName");
                RequireNonEmpty(promptLabel, $"{InputPromptCatalogPath}.prompts[{i}].promptLabel");
                RequireNonEmpty(iconKey, $"{InputPromptCatalogPath}.prompts[{i}].iconKey");
                if (!promptPairs.Add(pairKey))
                {
                    throw new InvalidOperationException($"{InputPromptCatalogPath}.prompts[{i}] duplicates {pairKey}.");
                }

                actionNames.Add(actionName);
            }

            RequireActionPrompt(actionNames, "Confirm");
            RequireActionPrompt(actionNames, "Back");
            RequireActionPrompt(actionNames, "Move");
            RequireActionPrompt(actionNames, "Look");
            RequireActionPrompt(actionNames, "TargetBias");
            RequireActionPrompt(actionNames, "BasicAttack");
            RequireActionPrompt(actionNames, "Dodge");
            RequireActionPrompt(actionNames, "Skill1");
            RequireActionPrompt(actionNames, "Ultimate");
            RequireActionPrompt(actionNames, "SummonSlot1");
            RequireActionPrompt(actionNames, "SummonSlot2");
            RequireActionPrompt(actionNames, "SummonSlot3");
            RequireActionPrompt(actionNames, "Pause");
        }

        private static void ValidateObjectReferenceArray(string assetPath, string arrayPropertyName, string referencePropertyName)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            SerializedObject serializedObject = new SerializedObject(asset);
            SerializedProperty array = serializedObject.FindProperty(arrayPropertyName);
            if (array == null || !array.isArray || array.arraySize == 0)
            {
                throw new InvalidOperationException($"{assetPath}.{arrayPropertyName} must not be empty.");
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty reference = array.GetArrayElementAtIndex(i).FindPropertyRelative(referencePropertyName);
                if (reference == null || reference.objectReferenceValue == null)
                {
                    throw new InvalidOperationException($"{assetPath}.{arrayPropertyName}[{i}].{referencePropertyName} is missing.");
                }

                string referencePath = AssetDatabase.GetAssetPath(reference.objectReferenceValue);
                RequireGameOwnedPath(referencePath, $"{assetPath}.{arrayPropertyName}[{i}].{referencePropertyName}");
            }
        }

        private static void ValidateOptionalObjectReferenceArray(string assetPath, string arrayPropertyName, string referencePropertyName)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            SerializedObject serializedObject = new SerializedObject(asset);
            SerializedProperty array = serializedObject.FindProperty(arrayPropertyName);
            if (array == null || !array.isArray || array.arraySize == 0)
            {
                throw new InvalidOperationException($"{assetPath}.{arrayPropertyName} must not be empty.");
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty reference = array.GetArrayElementAtIndex(i).FindPropertyRelative(referencePropertyName);
                if (reference == null || reference.objectReferenceValue == null)
                {
                    continue;
                }

                string referencePath = AssetDatabase.GetAssetPath(reference.objectReferenceValue);
                RequireGameOwnedPath(referencePath, $"{assetPath}.{arrayPropertyName}[{i}].{referencePropertyName}");
            }
        }

        private static HashSet<string> CollectStringKeys(string assetPath, string arrayPropertyName, string keyPropertyName)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            SerializedObject serializedObject = new SerializedObject(asset);
            SerializedProperty array = serializedObject.FindProperty(arrayPropertyName);
            if (array == null || !array.isArray || array.arraySize == 0)
            {
                throw new InvalidOperationException($"{assetPath}.{arrayPropertyName} must not be empty.");
            }

            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty keyProperty = array.GetArrayElementAtIndex(i).FindPropertyRelative(keyPropertyName);
                if (keyProperty == null)
                {
                    throw new InvalidOperationException($"{assetPath}.{arrayPropertyName}[{i}].{keyPropertyName} is missing.");
                }

                string key = keyProperty.stringValue;
                RequireNonEmpty(key, $"{assetPath}.{arrayPropertyName}[{i}].{keyPropertyName}");
                if (!keys.Add(key))
                {
                    throw new InvalidOperationException($"{assetPath}.{arrayPropertyName}[{i}].{keyPropertyName} duplicates {key}.");
                }
            }

            return keys;
        }

        private static void ValidateTextKeyReferences(
            HashSet<string> textKeys,
            string assetPath,
            string arrayPropertyName,
            params string[] keyPropertyNames)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            SerializedObject serializedObject = new SerializedObject(asset);
            SerializedProperty array = serializedObject.FindProperty(arrayPropertyName);
            if (array == null || !array.isArray || array.arraySize == 0)
            {
                throw new InvalidOperationException($"{assetPath}.{arrayPropertyName} must not be empty.");
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty entry = array.GetArrayElementAtIndex(i);
                for (int keyIndex = 0; keyIndex < keyPropertyNames.Length; keyIndex++)
                {
                    string propertyName = keyPropertyNames[keyIndex];
                    string key = entry.FindPropertyRelative(propertyName).stringValue;
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    if (!textKeys.Contains(key))
                    {
                        throw new InvalidOperationException($"{assetPath}.{arrayPropertyName}[{i}].{propertyName} references missing text key {key}.");
                    }
                }
            }
        }

        private static void ValidateKnownStringReferences(
            HashSet<string> knownKeys,
            string assetPath,
            string arrayPropertyName,
            params string[] keyPropertyNames)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            SerializedObject serializedObject = new SerializedObject(asset);
            SerializedProperty array = serializedObject.FindProperty(arrayPropertyName);
            if (array == null || !array.isArray || array.arraySize == 0)
            {
                throw new InvalidOperationException($"{assetPath}.{arrayPropertyName} must not be empty.");
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty entry = array.GetArrayElementAtIndex(i);
                for (int keyIndex = 0; keyIndex < keyPropertyNames.Length; keyIndex++)
                {
                    string propertyName = keyPropertyNames[keyIndex];
                    string key = entry.FindPropertyRelative(propertyName).stringValue;
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    if (!knownKeys.Contains(key))
                    {
                        throw new InvalidOperationException($"{assetPath}.{arrayPropertyName}[{i}].{propertyName} references unknown key {key}.");
                    }
                }
            }
        }

        private static void ValidateRouteMotionReferences(HashSet<string> motionIds)
        {
            UnityEngine.Object routeTable = RequireAsset<UIScreenRouteTable>(RouteTablePath);
            SerializedObject serializedObject = new SerializedObject(routeTable);
            SerializedProperty routes = serializedObject.FindProperty("routes");
            for (int i = 0; i < routes.arraySize; i++)
            {
                string transitionId = routes.GetArrayElementAtIndex(i).FindPropertyRelative("transitionId").stringValue;
                RequireKnownKey(motionIds, transitionId, $"{RouteTablePath}.routes[{i}].transitionId");
            }
        }

        private static void ValidateCueMotionReferences(HashSet<string> motionIds)
        {
            UnityEngine.Object cueCatalog = RequireAsset<UICueBundleCatalog>(CueBundleCatalogPath);
            SerializedObject serializedObject = new SerializedObject(cueCatalog);
            SerializedProperty cues = serializedObject.FindProperty("cueBundles");
            for (int i = 0; i < cues.arraySize; i++)
            {
                string motionId = cues.GetArrayElementAtIndex(i).FindPropertyRelative("uiMotionId").stringValue;
                RequireKnownKey(motionIds, motionId, $"{CueBundleCatalogPath}.cueBundles[{i}].uiMotionId");
            }
        }

        private static void ValidatePanelRoots(HashSet<string> panelIds)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/UI" });
            for (int prefabIndex = 0; prefabIndex < prefabGuids.Length; prefabIndex++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[prefabIndex]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                UIPanelRoot[] panelRoots = prefab.GetComponentsInChildren<UIPanelRoot>(true);
                for (int i = 0; i < panelRoots.Length; i++)
                {
                    SerializedObject serializedObject = new SerializedObject(panelRoots[i]);
                    string panelId = serializedObject.FindProperty("panelId").stringValue;
                    if (string.IsNullOrWhiteSpace(panelId))
                    {
                        continue;
                    }

                    RequireKnownKey(panelIds, panelId, $"{path}.UIPanelRoot[{i}].panelId");
                    SerializedProperty canvasGroup = serializedObject.FindProperty("canvasGroup");
                    if (canvasGroup == null || canvasGroup.objectReferenceValue == null)
                    {
                        throw new InvalidOperationException($"{path}.UIPanelRoot[{i}].canvasGroup is missing.");
                    }
                }
            }
        }

        private static void ValidatePanelRouters(HashSet<string> panelIds)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/UI" });
            for (int prefabIndex = 0; prefabIndex < prefabGuids.Length; prefabIndex++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[prefabIndex]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                UIPanelRouter[] routers = prefab.GetComponentsInChildren<UIPanelRouter>(true);
                for (int routerIndex = 0; routerIndex < routers.Length; routerIndex++)
                {
                    SerializedObject serializedObject = new SerializedObject(routers[routerIndex]);
                    SerializedProperty panels = serializedObject.FindProperty("panels");
                    if (panels == null || !panels.isArray || panels.arraySize == 0)
                    {
                        throw new InvalidOperationException($"{path}.UIPanelRouter[{routerIndex}] must bind at least one authored panel.");
                    }

                    for (int panelIndex = 0; panelIndex < panels.arraySize; panelIndex++)
                    {
                        SerializedProperty panel = panels.GetArrayElementAtIndex(panelIndex);
                        string panelId = panel.FindPropertyRelative("panelId").stringValue;
                        RequireKnownKey(panelIds, panelId, $"{path}.UIPanelRouter[{routerIndex}].panels[{panelIndex}].panelId");
                        SerializedProperty panelRoot = panel.FindPropertyRelative("panelRoot");
                        if (panelRoot == null || panelRoot.objectReferenceValue == null)
                        {
                            throw new InvalidOperationException($"{path}.UIPanelRouter[{routerIndex}].panels[{panelIndex}].panelRoot is missing.");
                        }
                    }
                }
            }
        }

        private static void ValidatePanelRequestButtons(HashSet<string> panelIds)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/UI" });
            for (int prefabIndex = 0; prefabIndex < prefabGuids.Length; prefabIndex++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[prefabIndex]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                UIPanelRequestButton[] buttons = prefab.GetComponentsInChildren<UIPanelRequestButton>(true);
                for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
                {
                    SerializedObject serializedObject = new SerializedObject(buttons[buttonIndex]);
                    SerializedProperty button = serializedObject.FindProperty("button");
                    SerializedProperty router = serializedObject.FindProperty("router");
                    string panelId = serializedObject.FindProperty("panelId").stringValue;

                    if (button == null || button.objectReferenceValue == null)
                    {
                        throw new InvalidOperationException($"{path}.UIPanelRequestButton[{buttonIndex}].button is missing.");
                    }

                    if (router == null || router.objectReferenceValue == null)
                    {
                        throw new InvalidOperationException($"{path}.UIPanelRequestButton[{buttonIndex}].router is missing.");
                    }

                    RequireKnownKey(panelIds, panelId, $"{path}.UIPanelRequestButton[{buttonIndex}].panelId");
                    UIPanelRouter panelRouter = router.objectReferenceValue as UIPanelRouter;
                    if (panelRouter == null || !PanelRouterContains(panelRouter, panelId))
                    {
                        throw new InvalidOperationException($"{path}.UIPanelRequestButton[{buttonIndex}] targets panel {panelId}, but its router does not bind that panel.");
                    }
                }
            }
        }

        private static bool PanelRouterContains(UIPanelRouter router, string panelId)
        {
            SerializedObject serializedObject = new SerializedObject(router);
            SerializedProperty panels = serializedObject.FindProperty("panels");
            if (panels == null || !panels.isArray)
            {
                return false;
            }

            for (int i = 0; i < panels.arraySize; i++)
            {
                string candidate = panels.GetArrayElementAtIndex(i).FindPropertyRelative("panelId").stringValue;
                if (string.Equals(candidate, panelId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateUiScenes(HashSet<UIRouteId> routeIds, HashSet<string> toastIds, HashSet<string> stageIds)
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { UiSceneRoot });
            if (sceneGuids.Length < 4)
            {
                throw new InvalidOperationException($"{UiSceneRoot} must contain the Login, Lobby, StageSelect, and CombatHud UI test scenes.");
            }

            string[] scenePaths = new string[sceneGuids.Length];
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                scenePaths[i] = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            }

            Array.Sort(scenePaths, StringComparer.Ordinal);
            for (int i = 0; i < scenePaths.Length; i++)
            {
                string scenePath = scenePaths[i];
                RequireScenePath(scenePath, scenePath);
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    ValidateLoadedUiScene(scenePath, scene, routeIds, toastIds, stageIds);
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void ValidateLoadedUiScene(
            string scenePath,
            Scene scene,
            HashSet<UIRouteId> routeIds,
            HashSet<string> toastIds,
            HashSet<string> stageIds)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException($"{scenePath} could not be opened for validation.");
            }

            GameObject[] roots = scene.GetRootGameObjects();
            if (roots.Length == 0)
            {
                throw new InvalidOperationException($"{scenePath} must contain authored UI scene roots.");
            }

            int missingScriptCount = CountMissingScripts(roots);
            if (missingScriptCount > 0)
            {
                throw new InvalidOperationException($"{scenePath} has {missingScriptCount} missing script reference(s).");
            }

            Canvas canvas = RequireSingleSceneComponent<Canvas>(scenePath, roots);
            EventSystem eventSystem = RequireSingleSceneComponent<EventSystem>(scenePath, roots);
            ValidateMobileFirstSceneFrame(scenePath, roots, canvas, eventSystem);

            UISceneFlowRouter router = RequireSingleSceneComponent<UISceneFlowRouter>(scenePath, roots);
            SerializedObject routerObject = new SerializedObject(router);
            RequireObjectReference(routerObject, "routeTable", $"{scenePath} scene router route table");
            RequireObjectReference(routerObject, "routeLoader", $"{scenePath} scene router route loader");
            RequireKnownRoute(routeIds, (UIRouteId)routerObject.FindProperty("defaultRoute").intValue, $"{scenePath}.UISceneFlowRouter.defaultRoute");
            UISceneRouteLoader routeLoader = RequireSingleSceneComponent<UISceneRouteLoader>(scenePath, roots);
            if (routerObject.FindProperty("routeLoader").objectReferenceValue != routeLoader)
            {
                throw new InvalidOperationException($"{scenePath} scene router must reference the scene UISceneRouteLoader.");
            }

            UITransitionPresenter transition = RequireSingleSceneComponent<UITransitionPresenter>(scenePath, roots);
            SerializedObject transitionObject = new SerializedObject(transition);
            RequireObjectReference(transitionObject, "fadeGroup", $"{scenePath} transition fade group");
            RequireObjectReference(transitionObject, "titleText", $"{scenePath} transition title text");
            RequireObjectReference(transitionObject, "descriptionText", $"{scenePath} transition description text");
            RequireObjectReference(transitionObject, "progressText", $"{scenePath} transition progress text");
            RequireObjectReference(transitionObject, "progressFill", $"{scenePath} transition progress fill");
            RequireObjectReference(transitionObject, "loadingCardDeck", $"{scenePath} transition loading deck");
            RequireObjectReference(transitionObject, "loadingCardPresenter", $"{scenePath} transition loading card presenter");
            ValidateSceneRouteLoader(scenePath, routeLoader, transition);
            ValidateSceneFlowStatus(scenePath, roots, router);
            ValidateSceneFlowNotice(scenePath, roots, router, toastIds);

            ValidateScenePresentation(scenePath, roots, routeIds);
            ValidateSceneRouteButtons(scenePath, roots, routeIds);
            ValidateSceneRouteInteractableGates(scenePath, roots, router);
            ValidateSceneSpecificPresenter(scenePath, roots, routeIds, stageIds);
        }

        private static void ValidateMobileFirstSceneFrame(
            string scenePath,
            GameObject[] roots,
            Canvas canvas,
            EventSystem eventSystem)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                throw new InvalidOperationException($"{scenePath} Canvas must include a CanvasScaler.");
            }

            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                throw new InvalidOperationException($"{scenePath} CanvasScaler must scale with screen size.");
            }

            if (scaler.referenceResolution.x <= scaler.referenceResolution.y)
            {
                throw new InvalidOperationException($"{scenePath} CanvasScaler must use a landscape reference resolution.");
            }

            if (scaler.referenceResolution.x < 1600f || scaler.referenceResolution.y < 720f)
            {
                throw new InvalidOperationException($"{scenePath} CanvasScaler reference resolution is below the mobile landscape floor.");
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                throw new InvalidOperationException($"{scenePath} EventSystem must use InputSystemUIInputModule for mobile/touch UI input.");
            }

            if (eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                throw new InvalidOperationException($"{scenePath} EventSystem must not keep StandaloneInputModule.");
            }

            UIResponsiveRoot responsiveRoot = RequireSingleSceneComponent<UIResponsiveRoot>(scenePath, roots);
            UISafeAreaRoot safeAreaRoot = RequireSingleSceneComponent<UISafeAreaRoot>(scenePath, roots);
            SerializedObject responsiveObject = new SerializedObject(responsiveRoot);
            RequireObjectReference(responsiveObject, "catalog", $"{scenePath} responsive layout catalog");
            RequireObjectReference(responsiveObject, "canvasScaler", $"{scenePath} responsive canvas scaler");
            RequireObjectReference(responsiveObject, "safeAreaRoot", $"{scenePath} responsive Safe Area root");

            if (responsiveObject.FindProperty("canvasScaler").objectReferenceValue != scaler)
            {
                throw new InvalidOperationException($"{scenePath} UIResponsiveRoot must reference the scene CanvasScaler.");
            }

            if (responsiveObject.FindProperty("safeAreaRoot").objectReferenceValue != safeAreaRoot)
            {
                throw new InvalidOperationException($"{scenePath} UIResponsiveRoot must reference the scene UISafeAreaRoot.");
            }
        }

        private static void ValidateSceneRouteLoader(
            string scenePath,
            UISceneRouteLoader routeLoader,
            UITransitionPresenter transitionPresenter)
        {
            SerializedObject serializedObject = new SerializedObject(routeLoader);
            RequireObjectReference(serializedObject, "transitionPresenter", $"{scenePath} scene route loader transition presenter");
            if (serializedObject.FindProperty("transitionPresenter").objectReferenceValue != transitionPresenter)
            {
                throw new InvalidOperationException($"{scenePath} scene route loader must reference the scene UITransitionPresenter.");
            }
        }

        private static void ValidateSceneFlowStatus(string scenePath, GameObject[] roots, UISceneFlowRouter router)
        {
            UISceneFlowStatusPresenter presenter = RequireSingleSceneComponent<UISceneFlowStatusPresenter>(scenePath, roots);
            SerializedObject serializedObject = new SerializedObject(presenter);
            RequireObjectReference(serializedObject, "router", $"{scenePath} flow status router");
            RequireObjectReference(serializedObject, "canvasGroup", $"{scenePath} flow status canvas group");
            RequireObjectReference(serializedObject, "routeText", $"{scenePath} flow status route text");
            RequireObjectReference(serializedObject, "phaseText", $"{scenePath} flow status phase text");
            RequireObjectReference(serializedObject, "progressText", $"{scenePath} flow status progress text");
            RequireObjectReference(serializedObject, "progressFill", $"{scenePath} flow status progress fill");

            if (serializedObject.FindProperty("router").objectReferenceValue != router)
            {
                throw new InvalidOperationException($"{scenePath} flow status presenter must reference the scene UISceneFlowRouter.");
            }
        }

        private static void ValidateSceneFlowNotice(
            string scenePath,
            GameObject[] roots,
            UISceneFlowRouter router,
            HashSet<string> toastIds)
        {
            UISceneFlowNoticePresenter presenter = RequireSingleSceneComponent<UISceneFlowNoticePresenter>(scenePath, roots);
            SerializedObject serializedObject = new SerializedObject(presenter);
            RequireObjectReference(serializedObject, "router", $"{scenePath} flow notice router");
            RequireObjectReference(serializedObject, "toastPresenter", $"{scenePath} flow notice toast presenter");
            RequireKnownKey(toastIds, serializedObject.FindProperty("busyToastId").stringValue, $"{scenePath}.UISceneFlowNoticePresenter.busyToastId");
            RequireKnownKey(toastIds, serializedObject.FindProperty("failedToastId").stringValue, $"{scenePath}.UISceneFlowNoticePresenter.failedToastId");
            RequireTrue(serializedObject.FindProperty("showRejectedRequests").boolValue, $"{scenePath}.UISceneFlowNoticePresenter.showRejectedRequests");
            RequireTrue(serializedObject.FindProperty("showFailedRoutes").boolValue, $"{scenePath}.UISceneFlowNoticePresenter.showFailedRoutes");

            if (serializedObject.FindProperty("router").objectReferenceValue != router)
            {
                throw new InvalidOperationException($"{scenePath} flow notice presenter must reference the scene UISceneFlowRouter.");
            }
        }

        private static void ValidateScenePresentation(string scenePath, GameObject[] roots, HashSet<UIRouteId> routeIds)
        {
            UIScreenPresentationPresenter presenter = RequireSingleSceneComponent<UIScreenPresentationPresenter>(scenePath, roots);
            SerializedObject serializedObject = new SerializedObject(presenter);
            RequireObjectReference(serializedObject, "screenCatalog", $"{scenePath} presentation screen catalog");
            RequireObjectReference(serializedObject, "soundContextCatalog", $"{scenePath} presentation sound context catalog");
            RequireObjectReference(serializedObject, "screenIdText", $"{scenePath} presentation screen id text");
            RequireObjectReference(serializedObject, "soundContextText", $"{scenePath} presentation sound context text");
            RequireObjectReference(serializedObject, "cachePolicyText", $"{scenePath} presentation cache policy text");
            RequireObjectReference(serializedObject, "accentImage", $"{scenePath} presentation accent image");
            UIRouteId routeId = (UIRouteId)serializedObject.FindProperty("routeId").intValue;
            RequireKnownRoute(routeIds, routeId, $"{scenePath}.UIScreenPresentationPresenter.routeId");
            if (TryGetExpectedRouteForScene(scenePath, out UIRouteId expectedRoute) && routeId != expectedRoute)
            {
                throw new InvalidOperationException($"{scenePath} presentation route must be {expectedRoute}, found {routeId}.");
            }
        }

        private static bool TryGetExpectedRouteForScene(string scenePath, out UIRouteId routeId)
        {
            if (scenePath.EndsWith("UI_LoginTest.unity", StringComparison.Ordinal))
            {
                routeId = UIRouteId.Login;
                return true;
            }

            if (scenePath.EndsWith("UI_LobbyTest.unity", StringComparison.Ordinal))
            {
                routeId = UIRouteId.Lobby;
                return true;
            }

            if (scenePath.EndsWith("UI_StageSelectTest.unity", StringComparison.Ordinal))
            {
                routeId = UIRouteId.StageSelect;
                return true;
            }

            if (scenePath.EndsWith("UI_CombatHudTest.unity", StringComparison.Ordinal))
            {
                routeId = UIRouteId.CombatHud;
                return true;
            }

            routeId = UIRouteId.None;
            return false;
        }

        private static void ValidateSceneSpecificPresenter(
            string scenePath,
            GameObject[] roots,
            HashSet<UIRouteId> routeIds,
            HashSet<string> stageIds)
        {
            if (scenePath.EndsWith("UI_LoginTest.unity", StringComparison.Ordinal))
            {
                LoginScreenPresenter presenter = RequireSingleSceneComponent<LoginScreenPresenter>(scenePath, roots);
                SerializedObject serializedObject = new SerializedObject(presenter);
                RequireObjectReference(serializedObject, "startButton", $"{scenePath} login start button");
                RequireObjectReference(serializedObject, "router", $"{scenePath} login router");
                RequireKnownRoute(routeIds, (UIRouteId)serializedObject.FindProperty("startRoute").intValue, $"{scenePath}.LoginScreenPresenter.startRoute");
                return;
            }

            if (scenePath.EndsWith("UI_LobbyTest.unity", StringComparison.Ordinal))
            {
                LobbyScreenPresenter presenter = RequireSingleSceneComponent<LobbyScreenPresenter>(scenePath, roots);
                SerializedObject serializedObject = new SerializedObject(presenter);
                RequireObjectReference(serializedObject, "primaryCtaButton", $"{scenePath} lobby primary CTA button");
                RequireObjectReference(serializedObject, "router", $"{scenePath} lobby router");
                RequireKnownRoute(routeIds, (UIRouteId)serializedObject.FindProperty("primaryRoute").intValue, $"{scenePath}.LobbyScreenPresenter.primaryRoute");
                ValidateSceneLobbyMockStateControls(scenePath, roots, presenter);
                return;
            }

            if (scenePath.EndsWith("UI_StageSelectTest.unity", StringComparison.Ordinal))
            {
                StageSelectScreenPresenter presenter = RequireSingleSceneComponent<StageSelectScreenPresenter>(scenePath, roots);
                SerializedObject serializedObject = new SerializedObject(presenter);
                RequireObjectReference(serializedObject, "stageCatalog", $"{scenePath} stage catalog");
                RequireObjectReference(serializedObject, "startButton", $"{scenePath} stage start button");
                RequireObjectReference(serializedObject, "backButton", $"{scenePath} stage back button");
                RequireObjectReference(serializedObject, "router", $"{scenePath} stage router");
                RequireKnownRoute(routeIds, (UIRouteId)serializedObject.FindProperty("startRoute").intValue, $"{scenePath}.StageSelectScreenPresenter.startRoute");
                RequireKnownRoute(routeIds, (UIRouteId)serializedObject.FindProperty("backRoute").intValue, $"{scenePath}.StageSelectScreenPresenter.backRoute");
                ValidateSceneStageSelectMockControls(scenePath, roots, presenter, stageIds);
                return;
            }

            if (scenePath.EndsWith("UI_CombatHudTest.unity", StringComparison.Ordinal))
            {
                CombatHudPresenter presenter = RequireSingleSceneComponent<CombatHudPresenter>(scenePath, roots);
                CombatHudInputBridge inputBridge = RequireSingleSceneComponent<CombatHudInputBridge>(scenePath, roots);
                RequireSingleSceneComponent<CombatHudMockFlowPresenter>(scenePath, roots);
                SerializedObject bridgeObject = new SerializedObject(inputBridge);
                RequireObjectReference(bridgeObject, "presenter", $"{scenePath} combat HUD input bridge presenter");
                if (bridgeObject.FindProperty("presenter").objectReferenceValue != presenter)
                {
                    throw new InvalidOperationException($"{scenePath} combat HUD input bridge must reference the scene CombatHudPresenter.");
                }
            }
        }

        private static void ValidateSceneLobbyMockStateControls(
            string scenePath,
            GameObject[] roots,
            LobbyScreenPresenter presenter)
        {
            LobbyMockStateControls controls = RequireSingleSceneComponent<LobbyMockStateControls>(scenePath, roots);
            ValidateLobbyMockStateControls(scenePath, controls, presenter);
        }

        private static void ValidateSceneStageSelectMockControls(
            string scenePath,
            GameObject[] roots,
            StageSelectScreenPresenter presenter,
            HashSet<string> stageIds)
        {
            StageSelectMockStageControls controls = RequireSingleSceneComponent<StageSelectMockStageControls>(scenePath, roots);
            ValidateStageSelectMockStageControls(scenePath, controls, presenter, stageIds);
        }

        private static void ValidateSceneRouteButtons(string scenePath, GameObject[] roots, HashSet<UIRouteId> routeIds)
        {
            UIRouteRequestButton[] routeButtons = GetComponentsInScene<UIRouteRequestButton>(roots);
            for (int i = 0; i < routeButtons.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(routeButtons[i]);
                RequireObjectReference(serializedObject, "button", $"{scenePath}.UIRouteRequestButton[{i}].button");
                RequireObjectReference(serializedObject, "router", $"{scenePath}.UIRouteRequestButton[{i}].router");
                RequireKnownRoute(routeIds, (UIRouteId)serializedObject.FindProperty("routeId").intValue, $"{scenePath}.UIRouteRequestButton[{i}].routeId");
                RequireTrue(serializedObject.FindProperty("disableWhileRouting").boolValue, $"{scenePath}.UIRouteRequestButton[{i}].disableWhileRouting");
            }
        }

        private static void ValidateSceneRouteInteractableGates(string scenePath, GameObject[] roots, UISceneFlowRouter router)
        {
            UIRouteInteractableGate[] gates = GetComponentsInScene<UIRouteInteractableGate>(roots);
            if (gates.Length == 0)
            {
                throw new InvalidOperationException($"{scenePath} must contain at least one UIRouteInteractableGate.");
            }

            for (int i = 0; i < gates.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(gates[i]);
                RequireObjectReference(serializedObject, "router", $"{scenePath}.UIRouteInteractableGate[{i}].router");
                if (serializedObject.FindProperty("router").objectReferenceValue != router)
                {
                    throw new InvalidOperationException($"{scenePath}.UIRouteInteractableGate[{i}] must reference the scene UISceneFlowRouter.");
                }

                ValidateRouteInteractableGateArrays(serializedObject, $"{scenePath}.UIRouteInteractableGate[{i}]");
            }
        }

        private static T RequireSingleSceneComponent<T>(string scenePath, GameObject[] roots)
            where T : Component
        {
            T[] components = GetComponentsInScene<T>(roots);
            if (components.Length != 1)
            {
                throw new InvalidOperationException($"{scenePath} must contain exactly one {typeof(T).Name}; found {components.Length}.");
            }

            return components[0];
        }

        private static T[] GetComponentsInScene<T>(GameObject[] roots)
            where T : Component
        {
            List<T> components = new List<T>();
            for (int i = 0; i < roots.Length; i++)
            {
                components.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }

            return components.ToArray();
        }

        private static int CountMissingScripts(GameObject[] roots)
        {
            int missingScriptCount = 0;
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    missingScriptCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transforms[transformIndex].gameObject);
                }
            }

            return missingScriptCount;
        }

        private static void ValidateNonEmptyArrays(params string[] assetPaths)
        {
            for (int i = 0; i < assetPaths.Length; i++)
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPaths[i]);
                SerializedObject serializedObject = new SerializedObject(asset);
                SerializedProperty iterator = serializedObject.GetIterator();
                bool foundNonEmptyArray = false;
                while (iterator.NextVisible(true))
                {
                    if (iterator.isArray && iterator.propertyType == SerializedPropertyType.Generic && iterator.arraySize > 0)
                    {
                        foundNonEmptyArray = true;
                        break;
                    }
                }

                if (!foundNonEmptyArray)
                {
                    throw new InvalidOperationException($"{assetPaths[i]} must contain at least one data row.");
                }
            }
        }

        private static void ValidateKnownPrefabs(
            HashSet<string> themeColorKeys,
            HashSet<string> themeTextStyleKeys,
            HashSet<string> motionIds,
            HashSet<string> cueIds,
            HashSet<string> loadingCardIds,
            HashSet<string> toastIds,
            HashSet<string> resultIds,
            HashSet<UIRouteId> routeIds,
            HashSet<string> stageIds)
        {
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/UI" });
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabPaths[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    throw new InvalidOperationException($"Could not load UI prefab at {path}.");
                }

                int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
                if (missingScriptCount > 0)
                {
                    throw new InvalidOperationException($"{path} has {missingScriptCount} missing script reference(s).");
                }

                ValidateResultPreviewPresenters(path, prefab);
                ValidateCombatHudMockFlowPresenters(path, prefab, toastIds, resultIds);
                ValidateSceneFlowRouters(path, prefab);
                ValidateFlowStatusPresenters(path, prefab);
                ValidateFlowNoticePresenters(path, prefab, toastIds);
                ValidateRouteRequestButtons(path, prefab, routeIds);
                ValidateRouteInteractableGates(path, prefab);
                ValidateLobbyMockStateControls(path, prefab);
                ValidateStageSelectMockStageControls(path, prefab, stageIds);
                ValidateScreenPresentationPresenters(path, prefab, routeIds);
                ValidateToastPresenters(path, prefab);
                ValidateDialogPresenters(path, prefab);
                ValidateLoginAccountServerNotice(path, prefab);
                ValidateLoadingCardPresenters(path, prefab, loadingCardIds);
                ValidateThemePresenters(path, prefab, themeColorKeys, themeTextStyleKeys);
                ValidateMotionPresenters(path, prefab, motionIds);
                ValidateCuePresenters(path, prefab, cueIds);
                ValidateLoginStartPromptPresenters(path, prefab);
            }
        }

        private static void ValidateRouteRequestButtons(
            string prefabPath,
            GameObject prefab,
            HashSet<UIRouteId> routeIds)
        {
            UIRouteRequestButton[] routeButtons = prefab.GetComponentsInChildren<UIRouteRequestButton>(true);
            for (int i = 0; i < routeButtons.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(routeButtons[i]);
                RequireObjectReference(serializedObject, "button", $"{prefabPath}.UIRouteRequestButton[{i}].button");
                RequireKnownRoute(routeIds, (UIRouteId)serializedObject.FindProperty("routeId").intValue, $"{prefabPath}.UIRouteRequestButton[{i}].routeId");
                RequireTrue(serializedObject.FindProperty("disableWhileRouting").boolValue, $"{prefabPath}.UIRouteRequestButton[{i}].disableWhileRouting");
            }
        }

        private static void ValidateRouteInteractableGates(string prefabPath, GameObject prefab)
        {
            UIRouteInteractableGate[] gates = prefab.GetComponentsInChildren<UIRouteInteractableGate>(true);
            for (int i = 0; i < gates.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(gates[i]);
                ValidateRouteInteractableGateArrays(serializedObject, $"{prefabPath}.UIRouteInteractableGate[{i}]");
            }
        }

        private static void ValidateRouteInteractableGateArrays(SerializedObject serializedObject, string label)
        {
            SerializedProperty selectables = serializedObject.FindProperty("selectables");
            if (selectables == null || !selectables.isArray || selectables.arraySize == 0)
            {
                throw new InvalidOperationException($"{label}.selectables must bind at least one Selectable.");
            }

            for (int i = 0; i < selectables.arraySize; i++)
            {
                if (selectables.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    throw new InvalidOperationException($"{label}.selectables[{i}] is missing.");
                }
            }

            SerializedProperty dimGroups = serializedObject.FindProperty("dimGroups");
            if (dimGroups == null || !dimGroups.isArray || dimGroups.arraySize == 0)
            {
                throw new InvalidOperationException($"{label}.dimGroups must bind at least one CanvasGroup.");
            }

            for (int i = 0; i < dimGroups.arraySize; i++)
            {
                if (dimGroups.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    throw new InvalidOperationException($"{label}.dimGroups[{i}] is missing.");
                }
            }

            RequireTrue(serializedObject.FindProperty("disableWhileRouting").boolValue, $"{label}.disableWhileRouting");
        }

        private static void ValidateLobbyMockStateControls(string prefabPath, GameObject prefab)
        {
            LobbyMockStateControls[] controls = prefab.GetComponentsInChildren<LobbyMockStateControls>(true);
            for (int i = 0; i < controls.Length; i++)
            {
                ValidateLobbyMockStateControls($"{prefabPath}.LobbyMockStateControls[{i}]", controls[i], null);
            }
        }

        private static void ValidateLobbyMockStateControls(
            string label,
            LobbyMockStateControls controls,
            LobbyScreenPresenter expectedPresenter)
        {
            SerializedObject serializedObject = new SerializedObject(controls);
            RequireObjectReference(serializedObject, "presenter", $"{label}.presenter");
            RequireObjectReference(serializedObject, "normalButton", $"{label}.normalButton");
            RequireObjectReference(serializedObject, "returnedButton", $"{label}.returnedButton");
            RequireObjectReference(serializedObject, "rewardButton", $"{label}.rewardButton");
            RequireObjectReference(serializedObject, "summonReadyButton", $"{label}.summonReadyButton");

            if (expectedPresenter != null && serializedObject.FindProperty("presenter").objectReferenceValue != expectedPresenter)
            {
                throw new InvalidOperationException($"{label}.presenter must reference the scene LobbyScreenPresenter.");
            }
        }

        private static void ValidateStageSelectMockStageControls(
            string prefabPath,
            GameObject prefab,
            HashSet<string> stageIds)
        {
            StageSelectMockStageControls[] controls = prefab.GetComponentsInChildren<StageSelectMockStageControls>(true);
            for (int i = 0; i < controls.Length; i++)
            {
                ValidateStageSelectMockStageControls($"{prefabPath}.StageSelectMockStageControls[{i}]", controls[i], null, stageIds);
            }
        }

        private static void ValidateStageSelectMockStageControls(
            string label,
            StageSelectMockStageControls controls,
            StageSelectScreenPresenter expectedPresenter,
            HashSet<string> stageIds)
        {
            SerializedObject serializedObject = new SerializedObject(controls);
            RequireObjectReference(serializedObject, "presenter", $"{label}.presenter");
            RequireObjectReference(serializedObject, "primaryStageButton", $"{label}.primaryStageButton");
            RequireObjectReference(serializedObject, "alternateStageButton", $"{label}.alternateStageButton");
            RequireKnownKey(stageIds, serializedObject.FindProperty("primaryStageId").stringValue, $"{label}.primaryStageId");
            RequireKnownKey(stageIds, serializedObject.FindProperty("alternateStageId").stringValue, $"{label}.alternateStageId");

            if (expectedPresenter != null && serializedObject.FindProperty("presenter").objectReferenceValue != expectedPresenter)
            {
                throw new InvalidOperationException($"{label}.presenter must reference the scene StageSelectScreenPresenter.");
            }
        }

        private static void ValidateSceneFlowRouters(string prefabPath, GameObject prefab)
        {
            UISceneFlowRouter[] routers = prefab.GetComponentsInChildren<UISceneFlowRouter>(true);
            for (int i = 0; i < routers.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(routers[i]);
                RequireObjectReference(serializedObject, "routeTable", $"{prefabPath}.UISceneFlowRouter[{i}].routeTable");
                RequireObjectReference(serializedObject, "routeLoader", $"{prefabPath}.UISceneFlowRouter[{i}].routeLoader");
            }
        }

        private static void ValidateFlowStatusPresenters(string prefabPath, GameObject prefab)
        {
            UISceneFlowStatusPresenter[] presenters = prefab.GetComponentsInChildren<UISceneFlowStatusPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "canvasGroup", $"{prefabPath} flow status canvas group");
                RequireObjectReference(serializedObject, "routeText", $"{prefabPath} flow status route text");
                RequireObjectReference(serializedObject, "phaseText", $"{prefabPath} flow status phase text");
                RequireObjectReference(serializedObject, "progressText", $"{prefabPath} flow status progress text");
                RequireObjectReference(serializedObject, "progressFill", $"{prefabPath} flow status progress fill");
            }
        }

        private static void ValidateFlowNoticePresenters(
            string prefabPath,
            GameObject prefab,
            HashSet<string> toastIds)
        {
            UISceneFlowNoticePresenter[] presenters = prefab.GetComponentsInChildren<UISceneFlowNoticePresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "router", $"{prefabPath} flow notice router");
                RequireObjectReference(serializedObject, "toastPresenter", $"{prefabPath} flow notice toast presenter");
                RequireKnownKey(toastIds, serializedObject.FindProperty("busyToastId").stringValue, $"{prefabPath}.UISceneFlowNoticePresenter[{i}].busyToastId");
                RequireKnownKey(toastIds, serializedObject.FindProperty("failedToastId").stringValue, $"{prefabPath}.UISceneFlowNoticePresenter[{i}].failedToastId");
                RequireTrue(serializedObject.FindProperty("showRejectedRequests").boolValue, $"{prefabPath}.UISceneFlowNoticePresenter[{i}].showRejectedRequests");
                RequireTrue(serializedObject.FindProperty("showFailedRoutes").boolValue, $"{prefabPath}.UISceneFlowNoticePresenter[{i}].showFailedRoutes");
            }
        }

        private static void ValidateScreenPresentationPresenters(
            string prefabPath,
            GameObject prefab,
            HashSet<UIRouteId> routeIds)
        {
            UIScreenPresentationPresenter[] presenters = prefab.GetComponentsInChildren<UIScreenPresentationPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "screenCatalog", $"{prefabPath} presentation screen catalog");
                RequireObjectReference(serializedObject, "soundContextCatalog", $"{prefabPath} presentation sound context catalog");
                RequireObjectReference(serializedObject, "screenIdText", $"{prefabPath} presentation screen id text");
                RequireObjectReference(serializedObject, "soundContextText", $"{prefabPath} presentation sound context text");
                RequireObjectReference(serializedObject, "cachePolicyText", $"{prefabPath} presentation cache policy text");
                RequireObjectReference(serializedObject, "accentImage", $"{prefabPath} presentation accent image");
                RequireKnownRoute(routeIds, (UIRouteId)serializedObject.FindProperty("routeId").intValue, $"{prefabPath}.UIScreenPresentationPresenter[{i}].routeId");
            }
        }

        private static void ValidateResultPreviewPresenters(string prefabPath, GameObject prefab)
        {
            UIResultPreviewPresenter[] presenters = prefab.GetComponentsInChildren<UIResultPreviewPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "catalog", $"{prefabPath} result preview catalog");
                RequireObjectReference(serializedObject, "textCatalog", $"{prefabPath} result preview text catalog");
                RequireObjectReference(serializedObject, "canvasGroup", $"{prefabPath} result preview canvas group");
                RequireObjectReference(serializedObject, "accentImage", $"{prefabPath} result preview accent image");
                RequireObjectReference(serializedObject, "titleText", $"{prefabPath} result preview title text");
                RequireObjectReference(serializedObject, "summaryText", $"{prefabPath} result preview summary text");
                RequireObjectReference(serializedObject, "primaryActionText", $"{prefabPath} result preview primary action text");
                RequireObjectReference(serializedObject, "secondaryActionText", $"{prefabPath} result preview secondary action text");
            }
        }

        private static void ValidateCombatHudMockFlowPresenters(
            string prefabPath,
            GameObject prefab,
            HashSet<string> toastIds,
            HashSet<string> resultIds)
        {
            CombatHudMockFlowPresenter[] presenters = prefab.GetComponentsInChildren<CombatHudMockFlowPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "hudPresenter", $"{prefabPath} mock flow HUD presenter");
                RequireObjectReference(serializedObject, "resultPreviewPresenter", $"{prefabPath} mock flow result presenter");
                RequireObjectReference(serializedObject, "toastPresenter", $"{prefabPath} mock flow toast presenter");
                RequireObjectReference(serializedObject, "stateText", $"{prefabPath} mock flow state text");
                RequireObjectReference(serializedObject, "startButton", $"{prefabPath} mock flow start button");
                RequireObjectReference(serializedObject, "winButton", $"{prefabPath} mock flow win button");
                RequireObjectReference(serializedObject, "failButton", $"{prefabPath} mock flow fail button");
                RequireObjectReference(serializedObject, "resetButton", $"{prefabPath} mock flow reset button");
                RequireKnownKey(resultIds, serializedObject.FindProperty("winResultId").stringValue, $"{prefabPath}.CombatHudMockFlowPresenter[{i}].winResultId");
                RequireKnownKey(resultIds, serializedObject.FindProperty("failResultId").stringValue, $"{prefabPath}.CombatHudMockFlowPresenter[{i}].failResultId");
                RequireKnownKey(toastIds, serializedObject.FindProperty("startToastId").stringValue, $"{prefabPath}.CombatHudMockFlowPresenter[{i}].startToastId");
                RequireKnownKey(toastIds, serializedObject.FindProperty("winToastId").stringValue, $"{prefabPath}.CombatHudMockFlowPresenter[{i}].winToastId");
                RequireKnownKey(toastIds, serializedObject.FindProperty("failToastId").stringValue, $"{prefabPath}.CombatHudMockFlowPresenter[{i}].failToastId");
                RequireKnownKey(toastIds, serializedObject.FindProperty("resetToastId").stringValue, $"{prefabPath}.CombatHudMockFlowPresenter[{i}].resetToastId");
            }
        }

        private static void ValidateToastPresenters(string prefabPath, GameObject prefab)
        {
            UIToastPresenter[] presenters = prefab.GetComponentsInChildren<UIToastPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "toastCatalog", $"{prefabPath} toast catalog");
                RequireObjectReference(serializedObject, "textCatalog", $"{prefabPath} toast text catalog");
                RequireObjectReference(serializedObject, "canvasGroup", $"{prefabPath} toast canvas group");
                RequireObjectReference(serializedObject, "messageText", $"{prefabPath} toast message text");
                RequireObjectReference(serializedObject, "accentImage", $"{prefabPath} toast accent image");
                RequireObjectReference(serializedObject, "iconPresenter", $"{prefabPath} toast icon presenter");
            }
        }

        private static void ValidateDialogPresenters(string prefabPath, GameObject prefab)
        {
            UIDialogPresenter[] presenters = prefab.GetComponentsInChildren<UIDialogPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "catalog", $"{prefabPath} dialog catalog");
                RequireObjectReference(serializedObject, "textCatalog", $"{prefabPath} dialog text catalog");
                RequireObjectReference(serializedObject, "canvasGroup", $"{prefabPath} dialog canvas group");
                RequireObjectReference(serializedObject, "iconPresenter", $"{prefabPath} dialog icon presenter");
                RequireObjectReference(serializedObject, "accentImage", $"{prefabPath} dialog accent image");
                RequireObjectReference(serializedObject, "titleText", $"{prefabPath} dialog title text");
                RequireObjectReference(serializedObject, "bodyText", $"{prefabPath} dialog body text");
                RequireObjectReference(serializedObject, "confirmButton", $"{prefabPath} dialog confirm button");
                RequireObjectReference(serializedObject, "confirmText", $"{prefabPath} dialog confirm text");
                RequireObjectReference(serializedObject, "cancelButton", $"{prefabPath} dialog cancel button");
                RequireObjectReference(serializedObject, "cancelText", $"{prefabPath} dialog cancel text");
            }
        }

        private static void ValidateLoginAccountServerNotice(string prefabPath, GameObject prefab)
        {
            if (!string.Equals(prefabPath, LoginScreenPrefabPath, StringComparison.Ordinal))
            {
                return;
            }

            Transform accountServer = FindChildByName(prefab.transform, "AccountServer");
            if (accountServer == null)
            {
                throw new InvalidOperationException($"{prefabPath} must include AccountServer.");
            }

            Button button = accountServer.GetComponent<Button>();
            if (button == null)
            {
                throw new InvalidOperationException($"{prefabPath}.AccountServer must include a Button.");
            }

            RequireTrue(button.interactable, $"{prefabPath}.AccountServer.interactable");
            if (button.targetGraphic == null)
            {
                throw new InvalidOperationException($"{prefabPath}.AccountServer.targetGraphic must be assigned.");
            }

            RequireTrue(button.targetGraphic.raycastTarget, $"{prefabPath}.AccountServer.targetGraphic.raycastTarget");

            Transform toastRoot = FindChildByName(prefab.transform, "LoginAccountServerToast");
            if (toastRoot == null)
            {
                throw new InvalidOperationException($"{prefabPath} must include LoginAccountServerToast.");
            }

            UIToastPresenter toastPresenter = toastRoot.GetComponent<UIToastPresenter>();
            if (toastPresenter == null)
            {
                throw new InvalidOperationException($"{prefabPath}.LoginAccountServerToast must include a UIToastPresenter.");
            }

            RequireButtonPersistentStringCall(
                button,
                toastPresenter,
                nameof(UIToastPresenter.ShowToast),
                LoginAccountServerToastId,
                $"{prefabPath}.AccountServer.onClick");
        }

        private static void RequireButtonPersistentStringCall(Button button, UnityEngine.Object target, string methodName, string argument, string label)
        {
            SerializedObject serializedButton = new SerializedObject(button);
            SerializedProperty calls = serializedButton.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            if (calls == null || !calls.isArray)
            {
                throw new InvalidOperationException($"{label} must have persistent calls.");
            }

            for (int i = 0; i < calls.arraySize; i++)
            {
                SerializedProperty call = calls.GetArrayElementAtIndex(i);
                UnityEngine.Object persistentTarget = call.FindPropertyRelative("m_Target").objectReferenceValue;
                string persistentMethod = call.FindPropertyRelative("m_MethodName").stringValue;
                int persistentMode = call.FindPropertyRelative("m_Mode").intValue;
                string persistentString = call.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue;

                if (persistentTarget == target &&
                    string.Equals(persistentMethod, methodName, StringComparison.Ordinal) &&
                    persistentMode == 5 &&
                    string.Equals(persistentString, argument, StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"{label} must call {target.GetType().Name}.{methodName} with {argument}.");
        }

        private static Transform FindChildByName(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (string.Equals(transforms[i].name, name, StringComparison.Ordinal))
                {
                    return transforms[i];
                }
            }

            return null;
        }

        private static void ValidateLoadingCardPresenters(
            string prefabPath,
            GameObject prefab,
            HashSet<string> loadingCardIds)
        {
            UILoadingCardPresenter[] presenters = prefab.GetComponentsInChildren<UILoadingCardPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "deck", $"{prefabPath} loading card deck");
                RequireObjectReference(serializedObject, "titleText", $"{prefabPath} loading card title text");
                RequireObjectReference(serializedObject, "descriptionText", $"{prefabPath} loading card description text");
                string defaultCardId = serializedObject.FindProperty("defaultCardId").stringValue;
                if (!string.IsNullOrWhiteSpace(defaultCardId))
                {
                    RequireKnownKey(loadingCardIds, defaultCardId, $"{prefabPath}.UILoadingCardPresenter[{i}].defaultCardId");
                }
            }
        }

        private static void ValidateThemePresenters(
            string prefabPath,
            GameObject prefab,
            HashSet<string> themeColorKeys,
            HashSet<string> themeTextStyleKeys)
        {
            UIThemeGraphicPresenter[] graphicPresenters = prefab.GetComponentsInChildren<UIThemeGraphicPresenter>(true);
            for (int i = 0; i < graphicPresenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(graphicPresenters[i]);
                RequireObjectReference(serializedObject, "catalog", $"{prefabPath} theme graphic catalog");
                RequireObjectReference(serializedObject, "targetGraphic", $"{prefabPath} theme graphic target");
                string colorKey = serializedObject.FindProperty("colorKey").stringValue;
                RequireKnownKey(themeColorKeys, colorKey, $"{prefabPath}.UIThemeGraphicPresenter[{i}].colorKey");
            }

            UIThemeTextStylePresenter[] textStylePresenters = prefab.GetComponentsInChildren<UIThemeTextStylePresenter>(true);
            for (int i = 0; i < textStylePresenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(textStylePresenters[i]);
                RequireObjectReference(serializedObject, "catalog", $"{prefabPath} theme text style catalog");
                RequireObjectReference(serializedObject, "targetText", $"{prefabPath} theme text style target");
                string textStyleKey = serializedObject.FindProperty("textStyleKey").stringValue;
                RequireKnownKey(themeTextStyleKeys, textStyleKey, $"{prefabPath}.UIThemeTextStylePresenter[{i}].textStyleKey");
            }
        }

        private static void ValidateMotionPresenters(string prefabPath, GameObject prefab, HashSet<string> motionIds)
        {
            UIMotionPresenter[] presenters = prefab.GetComponentsInChildren<UIMotionPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "catalog", $"{prefabPath} motion catalog");
                RequireObjectReference(serializedObject, "targetRect", $"{prefabPath} motion target rect");
                RequireObjectReference(serializedObject, "canvasGroup", $"{prefabPath} motion canvas group");
                string defaultMotionId = serializedObject.FindProperty("defaultMotionId").stringValue;
                RequireKnownKey(motionIds, defaultMotionId, $"{prefabPath}.UIMotionPresenter[{i}].defaultMotionId");
            }
        }

        private static void ValidateCuePresenters(string prefabPath, GameObject prefab, HashSet<string> cueIds)
        {
            UICuePresenter[] presenters = prefab.GetComponentsInChildren<UICuePresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "catalog", $"{prefabPath} cue catalog");
                RequireObjectReference(serializedObject, "motionPresenter", $"{prefabPath} cue motion presenter");
                RequireObjectReference(serializedObject, "cueIdText", $"{prefabPath} cue id text");
                RequireObjectReference(serializedObject, "detailText", $"{prefabPath} cue detail text");
                string defaultCueId = serializedObject.FindProperty("defaultCueId").stringValue;
                RequireKnownKey(cueIds, defaultCueId, $"{prefabPath}.UICuePresenter[{i}].defaultCueId");
            }
        }

        private static void ValidateLoginStartPromptPresenters(string prefabPath, GameObject prefab)
        {
            LoginStartPromptPresenter[] presenters = prefab.GetComponentsInChildren<LoginStartPromptPresenter>(true);
            for (int i = 0; i < presenters.Length; i++)
            {
                SerializedObject serializedObject = new SerializedObject(presenters[i]);
                RequireObjectReference(serializedObject, "promptGraphic", $"{prefabPath} login start prompt graphic");
                RequireObjectReference(serializedObject, "glowGraphic", $"{prefabPath} login start prompt glow");

                SerializedProperty scaleTargets = serializedObject.FindProperty("scaleTargets");
                if (scaleTargets == null || !scaleTargets.isArray || scaleTargets.arraySize == 0)
                {
                    throw new InvalidOperationException($"{prefabPath}.LoginStartPromptPresenter[{i}].scaleTargets must not be empty.");
                }

                for (int targetIndex = 0; targetIndex < scaleTargets.arraySize; targetIndex++)
                {
                    if (scaleTargets.GetArrayElementAtIndex(targetIndex).objectReferenceValue == null)
                    {
                        throw new InvalidOperationException($"{prefabPath}.LoginStartPromptPresenter[{i}].scaleTargets[{targetIndex}] must be assigned.");
                    }
                }
            }
        }

        private static T RequireAsset<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required UI asset: {path}");
            }

            return asset;
        }

        private static void RequireRoute(HashSet<UIRouteId> foundRoutes, UIRouteId routeId)
        {
            if (!foundRoutes.Contains(routeId))
            {
                throw new InvalidOperationException($"UI route table is missing {routeId}.");
            }
        }

        private static void RequireCombatHudAction(HashSet<CombatHudActionId> foundActions, CombatHudActionId actionId)
        {
            if (!foundActions.Contains(actionId))
            {
                throw new InvalidOperationException($"{CombatHudActionCatalogPath} is missing {actionId}.");
            }
        }

        private static void RequireActionPrompt(HashSet<string> actionNames, string actionName)
        {
            if (!actionNames.Contains(actionName))
            {
                throw new InvalidOperationException($"{InputPromptCatalogPath} is missing prompts for {actionName}.");
            }
        }

        private static void RequireKnownRoute(HashSet<UIRouteId> routeIds, UIRouteId routeId, string label)
        {
            if (routeId == UIRouteId.None || !routeIds.Contains(routeId))
            {
                throw new InvalidOperationException($"{label} references unknown UI route {routeId}.");
            }
        }

        private static void RequireTrue(bool value, string label)
        {
            if (!value)
            {
                throw new InvalidOperationException($"{label} must be true.");
            }
        }

        private static void RequireScenePath(string scenePath, string label)
        {
            RequireNonEmpty(scenePath, label);
            string normalized = scenePath.Replace('\\', '/');
            if (string.Equals(normalized, ActionFoundationScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} must not route to ActionFoundationTest.");
            }

            if (!normalized.StartsWith(UiSceneRoot, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} must stay under {UiSceneRoot}, found {scenePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(normalized) == null)
            {
                throw new InvalidOperationException($"{label} points to a missing scene: {scenePath}.");
            }
        }

        private static void RequireSceneNameMatchesPath(string sceneName, string scenePath, string label)
        {
            string normalized = scenePath.Replace('\\', '/');
            int lastSlash = normalized.LastIndexOf('/');
            string fileName = lastSlash >= 0 ? normalized.Substring(lastSlash + 1) : normalized;
            const string extension = ".unity";
            if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - extension.Length);
            }

            if (!string.Equals(sceneName, fileName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label}.sceneName must match scenePath file name. Found {sceneName} for {scenePath}.");
            }
        }

        private static void RequireSceneFadeDuration(float seconds, string label)
        {
            if (seconds < 0.35f || seconds > 0.8f)
            {
                throw new InvalidOperationException($"{label} must stay within UI V1 scene fade range 0.35-0.80 seconds.");
            }
        }

        private static void RequireGameOwnedPath(string assetPath, string label)
        {
            string normalized = assetPath.Replace('\\', '/');
            if (normalized.StartsWith(ImportedRoot, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{label} must not reference {ImportedRoot}.");
            }
        }

        private static void RequireNonEmpty(string value, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{label} must not be empty.");
            }
        }

        private static void RequireObjectReference(SerializedObject serializedObject, string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
            {
                throw new InvalidOperationException($"{label} must be assigned.");
            }
        }

        private static void RequireKnownKey(HashSet<string> knownKeys, string key, string label)
        {
            RequireNonEmpty(key, label);
            if (!knownKeys.Contains(key))
            {
                throw new InvalidOperationException($"{label} references unknown key {key}.");
            }
        }
    }
}
