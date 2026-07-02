using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using DimensionBrawl.Player;
using DimensionBrawl.Test;
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
    public static partial class ActionFoundationBossBarrageLaneReviewSetup
    {
        private const string CombatHudPrefabPath = "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const string DimensionHudArtRoot = "Assets/_Game/UI/CombatHud/Art/DimensionHud";
        private const string CombatHudCanvasRootName = ReviewRootPrefix + "CombatHudCanvas";
        private const string CombatHudEventSystemRootName = ReviewRootPrefix + "CombatHudEventSystem";
        private const string DimensionHudSkinRootName = "DimensionHudSkinRoot";
        private static readonly Vector2 DimensionHudDesignResolution = new Vector2(2560f, 1440f);
        private static readonly Color CombatHudHealthReadoutColor = new Color(1f, 0.92f, 0.68f, 1f);
        private static readonly Color CombatHudResourceReadoutColor = new Color(0.56f, 1f, 1f, 1f);
        private static readonly Color CombatHudInputReadoutColor = new Color(0.9f, 0.98f, 1f, 1f);
        private static readonly Color CombatHudReadoutOutlineColor = new Color(0f, 0.025f, 0.035f, 0.95f);
        private static readonly Color CombatHudSummonStateColor = new Color(0.9f, 0.98f, 1f, 1f);
        private static readonly Color CombatHudSummonLabelColor = new Color(1f, 0.92f, 0.68f, 1f);
        private static readonly Color CombatHudSummonIconColor = new Color(1f, 1f, 1f, 0.94f);

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Barrage Combat HUD UI")]
        public static void ReapplyBossBarrageCombatHudUiMenu()
        {
            EnsureDimensionHudSpriteImporters();
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            EnsureExistingReviewHudVisualPolicy(scene);
            EnsureCombatHudCanvasForOpenScene(scene);
            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException($"Failed to save {ReviewScenePath}.");
            }

            AssetDatabase.SaveAssets();
        }

        private static void CreateCombatHudCanvas(
            Scene scene,
            CombatHealth playerHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarragePocketReviewOwner pocketOwner,
            BossBarrageLaneReviewOverlayHud overlayHud)
        {
            EnsureDimensionHudSpriteImporters();
            EnsureExistingReviewHudVisualPolicy(scene);
            GameObject canvasRoot = EnsureCombatHudCanvasForOpenScene(scene);
            ConfigureCombatHudBinder(
                canvasRoot,
                playerHealth,
                bossHealth,
                energyLadder,
                actionController,
                combatModeController,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                summonSlot2Action,
                summonSlot3Action,
                pocketOwner,
                overlayHud);
        }

        private static void EnsureExistingReviewHudVisualPolicy(Scene scene)
        {
            GameObject hudRoot = FindRoot(scene, HudRootName);
            if (hudRoot == null)
            {
                return;
            }

            BossBarrageLaneReviewHud reviewHud = hudRoot.GetComponent<BossBarrageLaneReviewHud>();
            if (reviewHud != null)
            {
                SetBool(reviewHud, "showHud", false);
                SetBool(reviewHud, "showCenterReticle", true);
            }

            BossBarrageLaneReviewMobileHud mobileHud = hudRoot.GetComponent<BossBarrageLaneReviewMobileHud>();
            if (mobileHud != null)
            {
                SetBool(mobileHud, "showHud", false);
                SetBool(mobileHud, "drawHudVisuals", false);
                SetBehaviourEnabled(mobileHud, false);
            }

            BossBarrageLaneReviewOverlayHud overlayHud = hudRoot.GetComponent<BossBarrageLaneReviewOverlayHud>();
            if (overlayHud != null)
            {
                SetBool(overlayHud, "showOverlay", true);
                SetBool(overlayHud, "drawIdleButton", false);
            }
        }

        private static GameObject EnsureCombatHudCanvasForOpenScene(Scene scene)
        {
            GameObject canvasRoot = FindRoot(scene, CombatHudCanvasRootName);
            if (canvasRoot == null)
            {
                canvasRoot = new GameObject(
                    CombatHudCanvasRootName,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster),
                    typeof(BossBarrageLaneReviewCombatHudBinder));
                SceneManager.MoveGameObjectToScene(canvasRoot, scene);
            }

            RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
            Stretch(canvasRect);

            Canvas canvas = EnsureComponent<Canvas>(canvasRoot);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasRoot);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = DimensionHudDesignResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            EnsureComponent<GraphicRaycaster>(canvasRoot);
            EnsureCombatHudEventSystem(scene);

            GameObject hudInstance = EnsureCombatHudInstance(canvasRoot.transform);
            ApplyDimensionHudSkin(hudInstance);
            ConfigureCombatHudBinderFromScene(canvasRoot, scene);
            EditorUtility.SetDirty(canvasRoot);
            return canvasRoot;
        }

        private static void ConfigureCombatHudBinderFromScene(GameObject canvasRoot, Scene scene)
        {
            CombatHealth[] healthComponents = CollectComponents<CombatHealth>(scene);
            CombatHealth playerHealth = null;
            CombatHealth bossHealth = null;
            for (int i = 0; i < healthComponents.Length; i++)
            {
                CombatHealth candidate = healthComponents[i];
                if (candidate == null)
                {
                    continue;
                }

                if (playerHealth == null && candidate.GetComponent<PlayerActionController>() != null)
                {
                    playerHealth = candidate;
                }

                if (bossHealth == null && candidate.name.Contains("Boss", StringComparison.Ordinal))
                {
                    bossHealth = candidate;
                }
            }

            PlayerActionController actionController = playerHealth != null
                ? playerHealth.GetComponent<PlayerActionController>()
                : null;
            PlayerCombatModeController combatModeController = playerHealth != null
                ? playerHealth.GetComponent<PlayerCombatModeController>()
                : null;
            ConfigureCombatHudBinder(
                canvasRoot,
                playerHealth,
                bossHealth,
                playerHealth != null ? playerHealth.GetComponent<SummonEnergyLadder>() : null,
                actionController,
                combatModeController,
                playerHealth != null ? playerHealth.GetComponent<PlayerRangedBasicAttackAction>() : null,
                playerHealth != null ? playerHealth.GetComponent<PlayerSkill1Action>() : null,
                playerHealth != null ? playerHealth.GetComponent<PlayerSummonSlot1Action>() : null,
                FindSupportSummonAction(playerHealth, "SummonSlot2"),
                FindSupportSummonAction(playerHealth, "SummonSlot3"),
                FirstSceneComponent<BossBarragePocketReviewOwner>(scene),
                FirstSceneComponent<BossBarrageLaneReviewOverlayHud>(scene));
        }

        private static PlayerSupportSummonSlotAction FindSupportSummonAction(CombatHealth playerHealth, string actionName)
        {
            if (playerHealth == null)
            {
                return null;
            }

            PlayerSupportSummonSlotAction[] actions =
                playerHealth.GetComponents<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                PlayerSupportSummonSlotAction action = actions[i];
                if (action != null && string.Equals(action.SlotActionName, actionName, StringComparison.Ordinal))
                {
                    return action;
                }
            }

            return null;
        }

        private static T FirstSceneComponent<T>(Scene scene) where T : Component
        {
            T[] components = CollectComponents<T>(scene);
            return components.Length > 0 ? components[0] : null;
        }

        private static void ConfigureCombatHudBinder(
            GameObject canvasRoot,
            CombatHealth playerHealth,
            CombatHealth bossHealth,
            SummonEnergyLadder energyLadder,
            PlayerActionController actionController,
            PlayerCombatModeController combatModeController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction,
            PlayerSkill1Action skill1Action,
            PlayerSummonSlot1Action summonSlot1Action,
            PlayerSupportSummonSlotAction summonSlot2Action,
            PlayerSupportSummonSlotAction summonSlot3Action,
            BossBarragePocketReviewOwner pocketOwner,
            BossBarrageLaneReviewOverlayHud overlayHud)
        {
            BossBarrageLaneReviewCombatHudBinder binder =
                EnsureComponent<BossBarrageLaneReviewCombatHudBinder>(canvasRoot);
            CombatHudPresenter presenter = canvasRoot.GetComponentInChildren<CombatHudPresenter>(includeInactive: true);
            CombatHudInputBridge inputBridge = canvasRoot.GetComponentInChildren<CombatHudInputBridge>(includeInactive: true);
            ConfigureCombatHudPresenterRuntimeReferences(canvasRoot, presenter);
            SetObjectReference(binder, "hudPresenter", presenter);
            SetObjectReference(binder, "inputBridge", inputBridge);
            SetObjectReference(binder, "overlayHud", overlayHud);
            SetObjectReference(binder, "pocketReviewOwner", pocketOwner);
            SetObjectReference(binder, "playerHealth", playerHealth);
            SetObjectReference(binder, "bossHealth", bossHealth);
            SetObjectReference(binder, "energyLadder", energyLadder);
            SetObjectReference(binder, "actionController", actionController);
            SetObjectReference(binder, "combatModeController", combatModeController);
            SetObjectReference(binder, "rangedBasicAttackAction", rangedBasicAttackAction);
            SetObjectReference(binder, "skill1Action", skill1Action);
            SetObjectReference(binder, "summonSlot1Action", summonSlot1Action);
            SetObjectReference(binder, "summonSlot2Action", summonSlot2Action);
            SetObjectReference(binder, "summonSlot3Action", summonSlot3Action);
            SetBool(binder, "useSingleSummonPresentation", false);
            CombatHudMockFlowPresenter mockFlow =
                canvasRoot.GetComponentInChildren<CombatHudMockFlowPresenter>(includeInactive: true);
            if (mockFlow != null)
            {
                SetBehaviourEnabled(mockFlow, false);
            }

            ConfigureCombatHudInputComponents(
                canvasRoot,
                inputBridge,
                playerHealth != null ? playerHealth.GetComponent<PlayerMovementController>() : null,
                combatModeController,
                playerHealth != null ? playerHealth.GetComponent<PlayerRangedAimController>() : null,
                rangedBasicAttackAction);
            ConfigureCombatHudOverlayInputLocks(canvasRoot, overlayHud);
        }

        private static void ConfigureCombatHudInputComponents(
            GameObject canvasRoot,
            CombatHudInputBridge inputBridge,
            PlayerMovementController movementController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController aimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            ConfigureCombatHudAimDragInput(
                canvasRoot,
                movementController,
                combatModeController,
                aimController,
                rangedBasicAttackAction);
            ConfigureCombatHudVirtualJoystick(canvasRoot, movementController);
            ConfigureCombatHudPointerAction(
                canvasRoot,
                inputBridge,
                "BasicAttackButton",
                CombatHudActionId.BasicAttack,
                sendHoldState: true);
            ConfigureCombatHudPointerAction(
                canvasRoot,
                inputBridge,
                "DodgeButton",
                CombatHudActionId.Dodge,
                sendHoldState: false);
            ConfigureCombatHudPointerAction(
                canvasRoot,
                inputBridge,
                "Skill1Button",
                CombatHudActionId.Skill1,
                sendHoldState: false);
            ConfigureCombatHudPointerAction(
                canvasRoot,
                inputBridge,
                "UltimateButton",
                CombatHudActionId.Ultimate,
                sendHoldState: false);
            ConfigureCombatHudPointerAction(
                canvasRoot,
                inputBridge,
                "SummonSlot1Button",
                CombatHudActionId.SummonSlot1,
                sendHoldState: false);
            ConfigureCombatHudPointerAction(
                canvasRoot,
                inputBridge,
                "SummonSlot2Button",
                CombatHudActionId.SummonSlot2,
                sendHoldState: false);
            ConfigureCombatHudPointerAction(
                canvasRoot,
                inputBridge,
                "SummonSlot3Button",
                CombatHudActionId.SummonSlot3,
                sendHoldState: false);
        }

        private static void ConfigureCombatHudPresenterRuntimeReferences(
            GameObject canvasRoot,
            CombatHudPresenter presenter)
        {
            if (presenter == null)
            {
                return;
            }

            Image bossHpFill = FindHudDescendant(canvasRoot.transform, "BossHpFill")?.GetComponent<Image>();
            if (bossHpFill != null)
            {
                bossHpFill.type = Image.Type.Filled;
                bossHpFill.fillMethod = Image.FillMethod.Horizontal;
                bossHpFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                bossHpFill.fillAmount = 1f;
                MarkComponentDirty(bossHpFill);
            }

            SetObjectReference(presenter, "bossHealthFill", bossHpFill);
            SetObjectReference(presenter, "bossHealthText", FindHudDescendant(canvasRoot.transform, "ActionFeedback")?.GetComponent<Text>());
            MarkComponentDirty(presenter);
        }

        private static void ConfigureCombatHudAimDragInput(
            GameObject canvasRoot,
            PlayerMovementController movementController,
            PlayerCombatModeController combatModeController,
            PlayerRangedAimController aimController,
            PlayerRangedBasicAttackAction rangedBasicAttackAction)
        {
            Transform hudInstance = canvasRoot.transform.Find("PF_UI_CombatHud");
            Transform parent = hudInstance != null ? hudInstance : canvasRoot.transform;
            Transform aimArea = parent.Find("AimDragArea");
            if (aimArea == null)
            {
                aimArea = new GameObject("AimDragArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
                aimArea.SetParent(parent, worldPositionStays: false);
            }

            RectTransform rectTransform = aimArea.GetComponent<RectTransform>();
            Stretch(rectTransform);
            aimArea.SetAsFirstSibling();

            Image image = aimArea.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            MarkComponentDirty(image);

            CombatHudAimDragInput aimDragInput = EnsureComponent<CombatHudAimDragInput>(aimArea.gameObject);
            aimDragInput.Configure(
                movementController,
                combatModeController,
                aimController,
                rangedBasicAttackAction);
            SetBehaviourEnabled(aimDragInput, true);
            MarkComponentDirty(aimDragInput);
            EditorUtility.SetDirty(aimArea.gameObject);
        }

        private static void ConfigureCombatHudOverlayInputLocks(
            GameObject canvasRoot,
            BossBarrageLaneReviewOverlayHud overlayHud)
        {
            if (overlayHud == null)
            {
                return;
            }

            List<Behaviour> inputLocks = new List<Behaviour>();
            AddUniqueInputLock(inputLocks, canvasRoot.GetComponentInChildren<CombatHudInputBridge>(includeInactive: true));
            AddUniqueInputLocks(inputLocks, canvasRoot.GetComponentsInChildren<CombatHudAimDragInput>(includeInactive: true));
            AddUniqueInputLocks(inputLocks, canvasRoot.GetComponentsInChildren<CombatHudVirtualJoystick>(includeInactive: true));
            AddUniqueInputLocks(inputLocks, canvasRoot.GetComponentsInChildren<CombatHudPointerActionInput>(includeInactive: true));
            SetObjectReferenceArray(overlayHud, "inputLockBehaviours", inputLocks.ToArray());
        }

        private static void AddUniqueInputLocks<T>(List<Behaviour> inputLocks, T[] behaviours) where T : Behaviour
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                AddUniqueInputLock(inputLocks, behaviours[i]);
            }
        }

        private static void AddUniqueInputLock(List<Behaviour> inputLocks, Behaviour behaviour)
        {
            if (behaviour == null || inputLocks.Contains(behaviour))
            {
                return;
            }

            inputLocks.Add(behaviour);
        }

        private static void ConfigureCombatHudVirtualJoystick(
            GameObject canvasRoot,
            PlayerMovementController movementController)
        {
            Transform ring = FindHudDescendant(canvasRoot.transform, "MoveJoystickRing");
            Transform knob = FindHudDescendant(canvasRoot.transform, "MoveJoystickKnob");
            if (ring == null)
            {
                return;
            }

            CombatHudVirtualJoystick joystick =
                EnsureComponent<CombatHudVirtualJoystick>(ring.gameObject);
            joystick.Configure(movementController, knob != null ? knob.GetComponent<RectTransform>() : null);
            SetBehaviourEnabled(joystick, true);

            Image ringImage = ring.GetComponent<Image>();
            if (ringImage != null)
            {
                ringImage.raycastTarget = true;
                MarkComponentDirty(ringImage);
            }

            MarkComponentDirty(joystick);
            EditorUtility.SetDirty(ring.gameObject);
        }

        private static void ConfigureCombatHudPointerAction(
            GameObject canvasRoot,
            CombatHudInputBridge inputBridge,
            string objectName,
            CombatHudActionId actionId,
            bool sendHoldState)
        {
            Transform target = FindHudDescendant(canvasRoot.transform, objectName);
            if (target == null)
            {
                return;
            }

            CombatHudPointerActionInput pointerAction =
                EnsureComponent<CombatHudPointerActionInput>(target.gameObject);
            pointerAction.Configure(inputBridge, actionId, sendHoldState);
            SetBehaviourEnabled(pointerAction, true);
            MarkComponentDirty(pointerAction);
        }

        private static void EnsureCombatHudEventSystem(Scene scene)
        {
            GameObject eventSystemRoot = FindRoot(scene, CombatHudEventSystemRootName);
            if (eventSystemRoot == null)
            {
                eventSystemRoot = new GameObject(
                    CombatHudEventSystemRootName,
                    typeof(EventSystem),
                    typeof(InputSystemUIInputModule));
                SceneManager.MoveGameObjectToScene(eventSystemRoot, scene);
            }

            EnsureComponent<EventSystem>(eventSystemRoot);
            InputSystemUIInputModule inputModule = EnsureComponent<InputSystemUIInputModule>(eventSystemRoot);
            EnsureInputModuleActions(inputModule);
            EditorUtility.SetDirty(eventSystemRoot);
        }

        private static void EnsureInputModuleActions(InputSystemUIInputModule inputModule)
        {
            if (inputModule == null)
            {
                return;
            }

            if (inputModule.point == null || inputModule.leftClick == null)
            {
                inputModule.AssignDefaultActions();
            }

            MarkComponentDirty(inputModule);
        }

        private static GameObject EnsureCombatHudInstance(Transform canvasRoot)
        {
            Transform existing = canvasRoot.Find("PF_UI_CombatHud");
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject prefab = LoadAsset<GameObject>(CombatHudPrefabPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasRoot);
            instance.name = "PF_UI_CombatHud";
            Stretch(instance.GetComponent<RectTransform>());
            return instance;
        }

        private static void ApplyDimensionHudSkin(GameObject hudRoot)
        {
            Dictionary<string, Sprite> sprites = LoadDimensionHudSprites();
            Image rootImage = hudRoot.GetComponent<Image>();
            if (rootImage != null)
            {
                rootImage.color = Color.clear;
                rootImage.raycastTarget = false;
            }

            Transform skinRoot = EnsureDimensionSkinRoot(hudRoot.transform);
            AddOrUpdateSkinImage(skinRoot, "TopLeftPanel", sprites["Hud_TopLeftPanel"], new Rect(45f, 36f, 571f, 165f));
            AddOrUpdateSkinImage(skinRoot, "BossSymbol", sprites["Hud_BossSymbol"], new Rect(850f, 39f, 59f, 71f));
            AddOrUpdateSkinImage(skinRoot, "BossNameArea", sprites["Hud_BossNameArea"], new Rect(916f, 51f, 759f, 48f), visible: false);
            AddOrUpdateSkinImage(skinRoot, "BossHpBackground", sprites["Hud_BossHpBackground"], new Rect(851f, 109f, 856f, 31f));
            AddOrUpdateSkinImage(skinRoot, "BossHpFill", sprites["Hud_BossHpFill"], new Rect(867f, 113f, 823f, 24f));
            AddOrUpdateSkinImage(skinRoot, "BossCostBackground", sprites["Hud_BossCostBackground"], new Rect(854f, 142f, 849f, 34f));
            AddOrUpdateSkinImage(skinRoot, "BossCostFill", sprites["Hud_BossCostFill"], new Rect(870f, 144f, 819f, 25f));
            AddOrUpdateSkinImage(skinRoot, "PlayerSymbol", sprites["Hud_PlayerSymbol"], new Rect(814f, 1195f, 85f, 142f));
            AddOrUpdateSkinImage(skinRoot, "PlayerNameArea", sprites["Hud_PlayerNameArea"], new Rect(911f, 1212f, 215f, 43f), visible: false);
            AddOrUpdateSkinImage(skinRoot, "PlayerHpAmountArea", sprites["Hud_PlayerHpAmountArea"], new Rect(1326f, 1220f, 201f, 32f), visible: false);
            AddOrUpdateSkinImage(skinRoot, "PlayerMpAmountArea", sprites["Hud_PlayerMpAmountArea"], new Rect(1359f, 1327f, 147f, 26f), visible: false);
            AddOrUpdateSkinImage(skinRoot, "SettingsButton", sprites["Hud_ButtonSettings"], new Rect(2250f, 47f, 100f, 95f));

            ConfigureText(hudRoot, "Timer", new Rect(178f, 55f, 409f, 48f), Color.black, 18);
            ConfigureText(hudRoot, "Objective", new Rect(180f, 117f, 409f, 64f), Color.black, 18);
            ConfigureText(hudRoot, "ActionFeedback", new Rect(916f, 51f, 759f, 48f), Color.black, 18);
            ConfigureReadoutText(
                hudRoot,
                "InputMode",
                new Rect(911f, 1212f, 215f, 43f),
                CombatHudInputReadoutColor,
                17);
            ConfigureReadoutText(
                hudRoot,
                "HealthText",
                new Rect(1326f, 1220f, 201f, 32f),
                CombatHudHealthReadoutColor,
                20);
            ConfigureReadoutText(
                hudRoot,
                "ResourceText",
                new Rect(1359f, 1327f, 147f, 26f),
                CombatHudResourceReadoutColor,
                20);

            ConfigureImage(hudRoot, "PauseButton", sprites["Hud_ButtonPause"], new Rect(2396f, 47f, 100f, 95f), preserveAspect: false);
            ConfigureImage(hudRoot, "MoveJoystickRing", sprites["Hud_JoystickPanel"], new Rect(155f, 853f, 421f, 415f), preserveAspect: false);
            ConfigureImage(hudRoot, "MoveJoystickKnob", sprites["Hud_JoystickKnob"], new Rect(303f, 1004f, 122f, 121f), preserveAspect: false);
            ConfigureImage(hudRoot, "BasicAttackButton", sprites["Hud_ButtonAttack"], new Rect(2239f, 1156f, 230f, 248f), preserveAspect: false);
            ConfigureImage(hudRoot, "DodgeButton", sprites["Hud_ButtonDodge"], new Rect(1975f, 1172f, 256f, 218f), preserveAspect: false);
            ConfigureImage(hudRoot, "Skill1Button", sprites["Hud_ButtonSkill"], new Rect(2217f, 868f, 236f, 286f), preserveAspect: false);
            ConfigureImage(hudRoot, "UltimateButton", sprites["Hud_ButtonSwap"], new Rect(1975f, 896f, 248f, 226f), preserveAspect: false);
            ConfigureImage(hudRoot, "SummonSlot1Button", sprites["Hud_SummonSlot1Frame"], new Rect(2293f, 235f, 211f, 216f), preserveAspect: false);
            ConfigureImage(hudRoot, "SummonSlot2Button", sprites["Hud_SummonSlot2Frame"], new Rect(2308f, 472f, 182f, 186f), preserveAspect: false);
            ConfigureImage(hudRoot, "SummonSlot3Button", sprites["Hud_SummonSlot3Frame"], new Rect(2312f, 683f, 179f, 183f), preserveAspect: false);
            ConfigureImage(hudRoot, "HealthBar_Track", sprites["Hud_PlayerHpBackground"], new Rect(901f, 1263f, 640f, 21f), preserveAspect: false);
            ConfigureFillImage(hudRoot, "HealthBar", sprites["Hud_PlayerHpFill"], new Rect(904f, 1263f, 633f, 20f));
            ConfigureImage(hudRoot, "ResourceBar_Track", sprites["Hud_PlayerMpBackground"], new Rect(909f, 1296f, 616f, 28f), preserveAspect: false);
            ConfigureFillImage(hudRoot, "ResourceBar", sprites["Hud_PlayerMpFill"], new Rect(915f, 1299f, 605f, 21f));
            HideLegacyHudLabels(hudRoot);
            HideActionButtonTexts(hudRoot);
            ConfigureSummonSlotPresentation(
                hudRoot,
                "SummonSlot1Button",
                new Vector2(211f, 216f),
                16,
                sprites["Hud_SummonSlot1Icon"]);
            ConfigureSummonSlotPresentation(
                hudRoot,
                "SummonSlot2Button",
                new Vector2(182f, 186f),
                15,
                sprites["Hud_SummonSlot2Icon"]);
            ConfigureSummonSlotPresentation(
                hudRoot,
                "SummonSlot3Button",
                new Vector2(179f, 183f),
                15,
                sprites["Hud_SummonSlot3Icon"]);
            EditorUtility.SetDirty(hudRoot);
        }

        private static Transform EnsureDimensionSkinRoot(Transform hudRoot)
        {
            Transform existing = hudRoot.Find(DimensionHudSkinRootName);
            if (existing != null)
            {
                existing.SetAsFirstSibling();
                return existing;
            }

            GameObject root = new GameObject(DimensionHudSkinRootName, typeof(RectTransform));
            root.transform.SetParent(hudRoot, worldPositionStays: false);
            RectTransform rectTransform = root.GetComponent<RectTransform>();
            Stretch(rectTransform);
            root.transform.SetAsFirstSibling();
            return root.transform;
        }

        private static void AddOrUpdateSkinImage(Transform parent, string name, Sprite sprite, Rect designRect, bool visible = true)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
                child.SetParent(parent, worldPositionStays: false);
            }

            RectTransform rectTransform = child.GetComponent<RectTransform>();
            ApplyDesignRect(rectTransform, designRect);
            Image image = child.GetComponent<Image>();
            image.sprite = sprite;
            image.color = visible ? Color.white : Color.clear;
            image.raycastTarget = false;
            image.preserveAspect = false;
            MarkComponentDirty(rectTransform);
            MarkComponentDirty(image);
            EditorUtility.SetDirty(child.gameObject);
        }

        private static void ConfigureImage(
            GameObject hudRoot,
            string objectName,
            Sprite sprite,
            Rect designRect,
            bool preserveAspect)
        {
            Transform target = FindHudDescendant(hudRoot.transform, objectName);
            if (target == null)
            {
                return;
            }

            ApplyDesignRect(target.GetComponent<RectTransform>(), designRect);
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.raycastTarget = target.GetComponent<Button>() != null;
                image.preserveAspect = preserveAspect;
                image.type = Image.Type.Simple;
                MarkComponentDirty(image);
            }

            Button button = target.GetComponent<Button>();
            if (button != null)
            {
                button.targetGraphic = image;
                HideNonRootButtonGraphics(target, image);
                MarkComponentDirty(button);
            }

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                MarkComponentDirty(canvasGroup);
            }

            EditorUtility.SetDirty(target.gameObject);
        }

        private static void ConfigureFillImage(GameObject hudRoot, string objectName, Sprite sprite, Rect designRect)
        {
            Transform target = FindHudDescendant(hudRoot.transform, objectName);
            if (target == null)
            {
                return;
            }

            ApplyDesignRect(target.GetComponent<RectTransform>(), designRect);
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.raycastTarget = false;
                image.preserveAspect = false;
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Horizontal;
                image.fillOrigin = (int)Image.OriginHorizontal.Left;
                image.fillAmount = 1f;
                MarkComponentDirty(image);
            }

            EditorUtility.SetDirty(target.gameObject);
        }

        private static void ConfigureText(GameObject hudRoot, string objectName, Rect designRect, Color color, int fontSize)
        {
            Transform target = FindHudDescendant(hudRoot.transform, objectName);
            if (target == null)
            {
                return;
            }

            target.gameObject.SetActive(true);
            ApplyDesignRect(target.GetComponent<RectTransform>(), designRect);
            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.color = color;
                text.fontSize = fontSize;
                text.alignment = TextAnchor.MiddleCenter;
                text.raycastTarget = false;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 8;
                text.resizeTextMaxSize = fontSize;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
                MarkComponentDirty(text);
            }

            EditorUtility.SetDirty(target.gameObject);
        }

        private static void HideLegacyHudLabels(GameObject hudRoot)
        {
            string[] hiddenNames =
            {
                "CombatFieldReadback",
                "CombatFieldLabel",
                "MockState",
                "MockStartButton",
                "MockWinButton",
                "MockFailButton",
                "MockResetButton",
                "RetryHudButton",
                "ReturnLobbyButton",
                "LobbyButton",
                "ResultPreviewOverlay",
                "CombatToast"
            };

            for (int i = 0; i < hiddenNames.Length; i++)
            {
                Transform target = FindHudDescendant(hudRoot.transform, hiddenNames[i]);
                if (target != null)
                {
                    target.gameObject.SetActive(false);
                    EditorUtility.SetDirty(target.gameObject);
                }
            }
        }

        private static void HideActionButtonTexts(GameObject hudRoot)
        {
            string[] actionButtonNames =
            {
                "BasicAttackButton",
                "DodgeButton",
                "Skill1Button",
                "UltimateButton"
            };

            for (int i = 0; i < actionButtonNames.Length; i++)
            {
                Transform button = FindHudDescendant(hudRoot.transform, actionButtonNames[i]);
                if (button == null)
                {
                    continue;
                }

                Text[] texts = button.GetComponentsInChildren<Text>(includeInactive: true);
                for (int j = 0; j < texts.Length; j++)
                {
                    SetTextVisible(texts[j], false, Color.black, texts[j].fontSize);
                }
            }
        }

        private static void ConfigureSummonSlotPresentation(
            GameObject hudRoot,
            string buttonName,
            Vector2 buttonSize,
            int fontSize,
            Sprite iconSprite)
        {
            Transform button = FindHudDescendant(hudRoot.transform, buttonName);
            if (button == null)
            {
                return;
            }

            Rect barRect = ResolveSummonTextBarRect(buttonSize);
            Rect labelRect = new Rect(
                barRect.xMin + 4f,
                barRect.yMin,
                Mathf.Min(34f, barRect.width * 0.28f),
                barRect.height);
            Rect stateRect = new Rect(
                labelRect.xMax,
                barRect.yMin,
                Mathf.Max(24f, barRect.xMax - labelRect.xMax - 4f),
                barRect.height);

            ConfigureLocalImage(
                button,
                "Icon",
                ResolveSummonIconRect(buttonSize),
                buttonSize,
                iconSprite,
                CombatHudSummonIconColor);
            ConfigureLocalText(
                button,
                "Label",
                labelRect,
                buttonSize,
                CombatHudSummonLabelColor,
                Mathf.Max(13, fontSize - 1));
            ConfigureLocalText(
                button,
                "State",
                stateRect,
                buttonSize,
                CombatHudSummonStateColor,
                Mathf.Max(14, fontSize));
            HideSummonSlotProgressFill(button);
        }

        private static Rect ResolveSummonTextBarRect(Vector2 buttonSize)
        {
            float x = buttonSize.x * 0.17f;
            float y = buttonSize.y * 0.74f;
            float width = buttonSize.x * 0.66f;
            float height = buttonSize.y * 0.17f;
            return new Rect(x, y, width, height);
        }

        private static Rect ResolveSummonIconRect(Vector2 buttonSize)
        {
            float width = buttonSize.x * 0.58f;
            float height = buttonSize.y * 0.58f;
            float x = (buttonSize.x - width) * 0.5f;
            float y = buttonSize.y * 0.13f;
            return new Rect(x, y, width, height);
        }

        private static void ConfigureLocalImage(
            Transform root,
            string objectName,
            Rect localRect,
            Vector2 parentSize,
            Sprite sprite,
            Color color)
        {
            Transform target = root.Find(objectName);
            if (target == null)
            {
                target = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
                target.SetParent(root, worldPositionStays: false);
            }

            target.gameObject.SetActive(true);
            target.SetAsFirstSibling();
            ApplyLocalRect(target.GetComponent<RectTransform>(), localRect, parentSize);
            Image image = target.GetComponent<Image>();
            image.sprite = sprite;
            image.color = sprite != null ? color : Color.clear;
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
            MarkComponentDirty(image);
            EditorUtility.SetDirty(target.gameObject);
        }

        private static void ConfigureLocalText(
            Transform root,
            string objectName,
            Rect localRect,
            Vector2 parentSize,
            Color color,
            int fontSize)
        {
            Transform target = FindHudDescendant(root, objectName);
            if (target == null)
            {
                return;
            }

            target.gameObject.SetActive(true);
            ApplyLocalRect(target.GetComponent<RectTransform>(), localRect, parentSize);
            Text text = target.GetComponent<Text>();
            SetTextVisible(text, true, color, fontSize);
            EditorUtility.SetDirty(target.gameObject);
        }

        private static void ConfigureReadoutText(
            GameObject hudRoot,
            string objectName,
            Rect designRect,
            Color color,
            int fontSize)
        {
            ConfigureText(hudRoot, objectName, designRect, color, fontSize);
            Transform target = FindHudDescendant(hudRoot.transform, objectName);
            Text text = target != null ? target.GetComponent<Text>() : null;
            if (text == null)
            {
                return;
            }

            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            ConfigureTextOutline(text);
            MarkComponentDirty(text);
            EditorUtility.SetDirty(text.gameObject);
        }

        private static void HideSummonSlotProgressFill(Transform button)
        {
            Transform fill = FindHudDescendant(button, "CooldownFill");
            Image image = fill != null ? fill.GetComponent<Image>() : null;
            if (image == null)
            {
                return;
            }

            image.sprite = null;
            image.color = Color.clear;
            image.raycastTarget = false;
            image.fillAmount = 0f;
            MarkComponentDirty(image);
            EditorUtility.SetDirty(image.gameObject);
        }

        private static void ConfigureTextOutline(Text text)
        {
            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = CombatHudReadoutOutlineColor;
            outline.effectDistance = new Vector2(1.25f, -1.25f);
            outline.useGraphicAlpha = true;
            MarkComponentDirty(outline);
        }

        private static void SetTextVisible(Text text, bool visible, Color color, int fontSize)
        {
            if (text == null)
            {
                return;
            }

            text.gameObject.SetActive(visible);
            Color resolvedColor = color;
            resolvedColor.a = visible ? 1f : 0f;
            text.color = resolvedColor;
            text.fontSize = fontSize;
            text.fontStyle = visible ? FontStyle.Bold : FontStyle.Normal;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.resizeTextForBestFit = false;
            text.resizeTextMinSize = 8;
            text.resizeTextMaxSize = fontSize;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            if (visible)
            {
                ConfigureTextOutline(text);
            }

            MarkComponentDirty(text);
            EditorUtility.SetDirty(text.gameObject);
        }

        private static void HideNonRootButtonGraphics(Transform buttonRoot, Image rootImage)
        {
            Image[] images = buttonRoot.GetComponentsInChildren<Image>(includeInactive: true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null || image == rootImage)
                {
                    continue;
                }

                Color color = image.color;
                color.a = 0f;
                image.color = color;
                image.raycastTarget = false;
                MarkComponentDirty(image);
            }
        }

        private static void ApplyLocalRect(RectTransform rectTransform, Rect localRect, Vector2 parentSize)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(localRect.width, localRect.height);
            rectTransform.anchoredPosition = new Vector2(
                localRect.xMin + localRect.width * 0.5f - parentSize.x * 0.5f,
                parentSize.y * 0.5f - localRect.yMin - localRect.height * 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            MarkComponentDirty(rectTransform);
        }

        private static Dictionary<string, Sprite> LoadDimensionHudSprites()
        {
            var sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            string[] names =
            {
                "Hud_BossCostBackground",
                "Hud_BossCostFill",
                "Hud_BossHpBackground",
                "Hud_BossHpFill",
                "Hud_BossNameArea",
                "Hud_BossSymbol",
                "Hud_ButtonAttack",
                "Hud_ButtonDodge",
                "Hud_ButtonPause",
                "Hud_ButtonSettings",
                "Hud_ButtonSkill",
                "Hud_ButtonSwap",
                "Hud_JoystickKnob",
                "Hud_JoystickPanel",
                "Hud_PlayerHpAmountArea",
                "Hud_PlayerHpBackground",
                "Hud_PlayerHpFill",
                "Hud_PlayerMpAmountArea",
                "Hud_PlayerMpBackground",
                "Hud_PlayerMpFill",
                "Hud_PlayerNameArea",
                "Hud_PlayerSymbol",
                "Hud_SummonSlot1Icon",
                "Hud_SummonSlot1Frame",
                "Hud_SummonSlot2Icon",
                "Hud_SummonSlot2Frame",
                "Hud_SummonSlot3Icon",
                "Hud_SummonSlot3Frame",
                "Hud_TopLeftPanel"
            };

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                string path = $"{DimensionHudArtRoot}/{name}.png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    throw new InvalidOperationException($"Missing Dimension HUD sprite at {path}.");
                }

                sprites.Add(name, sprite);
            }

            return sprites;
        }

        private static void EnsureDimensionHudSpriteImporters()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { DimensionHudArtRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void ApplyDesignRect(RectTransform rectTransform, Rect designRect)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(
                designRect.xMin + designRect.width * 0.5f - DimensionHudDesignResolution.x * 0.5f,
                DimensionHudDesignResolution.y * 0.5f - designRect.yMin - designRect.height * 0.5f);
            rectTransform.sizeDelta = new Vector2(designRect.width, designRect.height);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            MarkComponentDirty(rectTransform);
        }

        private static void Stretch(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            MarkComponentDirty(rectTransform);
        }

        private static void MarkComponentDirty(Component component)
        {
            if (component == null)
            {
                return;
            }

            EditorUtility.SetDirty(component);
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }

        private static Transform FindHudDescendant(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, name, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindHudDescendant(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
