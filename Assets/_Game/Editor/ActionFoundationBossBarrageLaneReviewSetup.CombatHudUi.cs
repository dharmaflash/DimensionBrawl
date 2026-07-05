using System;
using System.Collections.Generic;
using System.IO;
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
        private const string CombatHudMaterialRoot = "Assets/_Game/UI/CombatHud/Materials";
        private const string CombatHudGeneratedArtRoot = "Assets/_Game/UI/CombatHud/Generated";
        private const string CombatHudDisabledSummonIconMaterialPath =
            CombatHudMaterialRoot + "/DB_UI_SummonIconDisabledGrayscale.mat";
        private const string CombatHudDisabledSummonIconShaderName = "DimensionBrawl/UI/GrayscaleTint";
        private const string CombatHudSummonProgressRingSpritePath =
            CombatHudGeneratedArtRoot + "/DB_UI_SummonProgressRing.png";
        private const string CombatHudSummonReadyGlowSpritePath =
            CombatHudGeneratedArtRoot + "/DB_UI_SummonReadyGlow.png";
        private const string CombatHudSummonReadySparkSpritePath =
            CombatHudGeneratedArtRoot + "/DB_UI_SummonReadySparkRing.png";
        private const string CombatHudActionCooldownRingSpritePath =
            CombatHudGeneratedArtRoot + "/DB_UI_ActionCooldownRing.png";
        private const string CombatHudActionReadyGlowSpritePath =
            CombatHudGeneratedArtRoot + "/DB_UI_ActionReadyGlow.png";
        private const string CombatHudCanvasRootName = ReviewRootPrefix + "CombatHudCanvas";
        private const string CombatHudEventSystemRootName = ReviewRootPrefix + "CombatHudEventSystem";
        private const string DimensionHudSkinRootName = "DimensionHudSkinRoot";
        private static readonly Vector2 DimensionHudDesignResolution = new Vector2(2560f, 1440f);
        private static readonly Color CombatHudHealthReadoutColor = new Color(1f, 0.92f, 0.68f, 1f);
        private static readonly Color CombatHudResourceReadoutColor = new Color(0.56f, 1f, 1f, 1f);
        private static readonly Color CombatHudInputReadoutColor = new Color(0.9f, 0.98f, 1f, 1f);
        private static readonly Color CombatHudAmmoReadoutColor = new Color(1f, 0.86f, 0.38f, 1f);
        private static readonly Color CombatHudReadoutOutlineColor = new Color(0f, 0.025f, 0.035f, 0.95f);
        private static readonly Color CombatHudSummonStateColor = new Color(0.9f, 0.98f, 1f, 1f);
        private static readonly Color CombatHudSummonLabelColor = new Color(1f, 0.92f, 0.68f, 1f);
        private static readonly Color CombatHudSummonIconColor = new Color(1f, 1f, 1f, 0.94f);
        private static readonly Color CombatHudSummonProgressColor = new Color(0.35f, 0.95f, 1f, 0.72f);

        [MenuItem("DimensionBrawl/Reapply Action Foundation Boss Barrage Combat HUD UI")]
        public static void ReapplyBossBarrageCombatHudUiMenu()
        {
            EnsureDimensionHudSpriteImporters();
            EnsureCombatHudUiMaterials();
            EnsureCombatHudUiGeneratedSprites();
            ApplyDimensionHudSkinToPrefabAsset();
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
            EnsureCombatHudUiMaterials();
            EnsureCombatHudUiGeneratedSprites();
            ApplyDimensionHudSkinToPrefabAsset();
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
            SetObjectReference(binder, "bossCostLadder", UnityEngine.Object.FindFirstObjectByType<BossPressureCostLadder>());
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
            Image bossCostFill = FindHudDescendant(canvasRoot.transform, "BossCostFill")?.GetComponent<Image>();
            if (bossHpFill != null)
            {
                bossHpFill.type = Image.Type.Filled;
                bossHpFill.fillMethod = Image.FillMethod.Horizontal;
                bossHpFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                bossHpFill.fillAmount = 1f;
                MarkComponentDirty(bossHpFill);
            }

            if (bossCostFill != null)
            {
                bossCostFill.type = Image.Type.Filled;
                bossCostFill.fillMethod = Image.FillMethod.Horizontal;
                bossCostFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                bossCostFill.fillAmount = 0f;
                MarkComponentDirty(bossCostFill);
            }

            SetObjectReference(presenter, "bossHealthFill", bossHpFill);
            SetObjectReference(presenter, "bossResourceFill", bossCostFill);
            SetObjectReference(presenter, "bossHealthText", FindHudDescendant(canvasRoot.transform, "ActionFeedback")?.GetComponent<Text>());
            SetObjectReference(presenter, "ammoText", FindHudDescendant(canvasRoot.transform, "AmmoText")?.GetComponent<Text>());
            BindExistingActionSlotEffectImages(canvasRoot, presenter);
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

        private static void ApplyDimensionHudSkinToPrefabAsset()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(CombatHudPrefabPath);
            try
            {
                ApplyDimensionHudSkin(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CombatHudPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void ApplyDimensionHudSkin(GameObject hudRoot)
        {
            Dictionary<string, Sprite> sprites = LoadDimensionHudSprites();
            Material disabledIconMaterial = LoadAsset<Material>(CombatHudDisabledSummonIconMaterialPath);
            Sprite progressRingSprite = LoadAsset<Sprite>(CombatHudSummonProgressRingSpritePath);
            Sprite readyGlowSprite = LoadAsset<Sprite>(CombatHudSummonReadyGlowSpritePath);
            Sprite readySparkSprite = LoadAsset<Sprite>(CombatHudSummonReadySparkSpritePath);
            Sprite actionCooldownRingSprite = LoadAsset<Sprite>(CombatHudActionCooldownRingSpritePath);
            Sprite actionReadyGlowSprite = LoadAsset<Sprite>(CombatHudActionReadyGlowSpritePath);
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
            Image bossHpFill = AddOrUpdateSkinImage(skinRoot, "BossHpFill", sprites["Hud_BossHpFill"], new Rect(867f, 113f, 823f, 24f));
            ConfigureHorizontalFillImage(bossHpFill, 1f);
            AddOrUpdateSkinImage(skinRoot, "BossCostBackground", sprites["Hud_BossCostBackground"], new Rect(854f, 142f, 849f, 34f));
            Image bossCostFill = AddOrUpdateSkinImage(skinRoot, "BossCostFill", sprites["Hud_BossCostFill"], new Rect(870f, 144f, 819f, 25f));
            ConfigureHorizontalFillImage(bossCostFill, 1f);
            AddOrUpdateSkinImage(skinRoot, "PlayerSymbol", sprites["Hud_PlayerSymbol"], new Rect(916.5f, 1195f, 85f, 142f));
            AddOrUpdateSkinImage(skinRoot, "PlayerNameArea", sprites["Hud_PlayerNameArea"], new Rect(1013.5f, 1212f, 215f, 43f), visible: false);
            AddOrUpdateSkinImage(skinRoot, "PlayerHpAmountArea", sprites["Hud_PlayerHpAmountArea"], new Rect(1428.5f, 1220f, 201f, 32f), visible: false);
            AddOrUpdateSkinImage(skinRoot, "PlayerMpAmountArea", sprites["Hud_PlayerMpAmountArea"], new Rect(1461.5f, 1327f, 147f, 26f), visible: false);
            AddOrUpdateSkinImage(skinRoot, "SettingsButton", sprites["Hud_ButtonSettings"], new Rect(2250f, 47f, 100f, 95f));

            ConfigureText(hudRoot, "Timer", new Rect(178f, 55f, 409f, 48f), Color.black, 18);
            ConfigureText(hudRoot, "Objective", new Rect(180f, 117f, 409f, 64f), Color.black, 18);
            ConfigureText(hudRoot, "ActionFeedback", new Rect(916f, 51f, 759f, 48f), Color.black, 18);
            ConfigureReadoutText(
                hudRoot,
                "InputMode",
                new Rect(1013.5f, 1212f, 215f, 43f),
                CombatHudInputReadoutColor,
                17);
            ConfigureReadoutText(
                hudRoot,
                "HealthText",
                new Rect(1428.5f, 1220f, 201f, 32f),
                CombatHudHealthReadoutColor,
                20);
            ConfigureReadoutText(
                hudRoot,
                "ResourceText",
                new Rect(1461.5f, 1327f, 147f, 26f),
                CombatHudResourceReadoutColor,
                20);
            EnsureHudText(hudRoot, "AmmoText", "24/24");
            ConfigureReadoutText(
                hudRoot,
                "AmmoText",
                new Rect(1650f, 1268f, 178f, 42f),
                CombatHudAmmoReadoutColor,
                18);

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
            ConfigureImage(hudRoot, "HealthBar_Track", sprites["Hud_PlayerHpBackground"], new Rect(1003.5f, 1263f, 640f, 21f), preserveAspect: false);
            ConfigureFillImage(hudRoot, "HealthBar", sprites["Hud_PlayerHpFill"], new Rect(1006.5f, 1263f, 633f, 20f));
            ConfigureImage(hudRoot, "ResourceBar_Track", sprites["Hud_PlayerMpBackground"], new Rect(1011.5f, 1296f, 616f, 28f), preserveAspect: false);
            ConfigureFillImage(hudRoot, "ResourceBar", sprites["Hud_PlayerMpFill"], new Rect(1017.5f, 1299f, 605f, 21f));
            HideLegacyHudLabels(hudRoot);
            HideActionButtonTexts(hudRoot);
            ConfigureActionButtonAvailabilityEffects(
                hudRoot,
                "DodgeButton",
                new Vector2(256f, 218f),
                CombatHudActionId.Dodge,
                actionCooldownRingSprite,
                actionReadyGlowSprite);
            ConfigureActionButtonAvailabilityEffects(
                hudRoot,
                "Skill1Button",
                new Vector2(236f, 286f),
                CombatHudActionId.Skill1,
                null,
                actionReadyGlowSprite);
            ConfigureSummonSlotPresentation(
                hudRoot,
                "SummonSlot1Button",
                new Vector2(211f, 216f),
                16,
                sprites["Hud_SummonSlot1Icon"],
                disabledIconMaterial,
                progressRingSprite,
                readyGlowSprite,
                readySparkSprite);
            ConfigureSummonSlotPresentation(
                hudRoot,
                "SummonSlot2Button",
                new Vector2(182f, 186f),
                15,
                sprites["Hud_SummonSlot2Icon"],
                disabledIconMaterial,
                progressRingSprite,
                readyGlowSprite,
                readySparkSprite);
            ConfigureSummonSlotPresentation(
                hudRoot,
                "SummonSlot3Button",
                new Vector2(179f, 183f),
                15,
                sprites["Hud_SummonSlot3Icon"],
                disabledIconMaterial,
                progressRingSprite,
                readyGlowSprite,
                readySparkSprite);
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

        private static Image AddOrUpdateSkinImage(Transform parent, string name, Sprite sprite, Rect designRect, bool visible = true)
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
            return image;
        }

        private static void ConfigureHorizontalFillImage(Image image, float fillAmount)
        {
            if (image == null)
            {
                return;
            }

            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillAmount = Mathf.Clamp01(fillAmount);
            MarkComponentDirty(image);
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

        private static Text EnsureHudText(GameObject hudRoot, string objectName, string defaultText)
        {
            Transform target = FindHudDescendant(hudRoot.transform, objectName);
            if (target == null)
            {
                GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObject.transform.SetParent(hudRoot.transform, worldPositionStays: false);
                target = textObject.transform;
                target.SetAsLastSibling();
                EditorUtility.SetDirty(textObject);
            }

            Text text = target.GetComponent<Text>();
            if (text == null)
            {
                text = target.gameObject.AddComponent<Text>();
            }

            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.text = defaultText;
            MarkComponentDirty(text);
            return text;
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

        private static void ConfigureActionButtonAvailabilityEffects(
            GameObject hudRoot,
            string buttonName,
            Vector2 buttonSize,
            CombatHudActionId actionId,
            Sprite progressRingSprite,
            Sprite readyGlowSprite)
        {
            Transform button = FindHudDescendant(hudRoot.transform, buttonName);
            if (button == null)
            {
                return;
            }

            Vector2 effectCenterOffset = ResolveActionEffectCenterOffset(actionId);
            Image readyGlowImage = ConfigureActionEffectImage(
                button,
                "ActionReadyGlow",
                ResolveActionEffectRect(buttonSize, 1.12f, effectCenterOffset),
                buttonSize,
                readyGlowSprite,
                Image.Type.Simple);
            Image progressFill = null;
            if (progressRingSprite != null)
            {
                progressFill = ConfigureActionEffectImage(
                    button,
                    "DodgeCooldownRing",
                    ResolveActionEffectRect(buttonSize, 1.08f, effectCenterOffset),
                    buttonSize,
                    progressRingSprite,
                    Image.Type.Filled);
                progressFill.fillMethod = Image.FillMethod.Radial360;
                progressFill.fillOrigin = (int)Image.Origin360.Top;
                progressFill.fillClockwise = true;
                progressFill.fillAmount = 0f;
                MarkComponentDirty(progressFill);
                progressFill.transform.SetAsLastSibling();
            }

            readyGlowImage.transform.SetAsFirstSibling();
            BindActionSlotEffectImages(
                hudRoot.GetComponentInChildren<CombatHudPresenter>(includeInactive: true),
                actionId,
                readyGlowImage,
                progressFill);
            EditorUtility.SetDirty(button.gameObject);
        }

        private static Image ConfigureActionEffectImage(
            Transform button,
            string objectName,
            Rect localRect,
            Vector2 parentSize,
            Sprite sprite,
            Image.Type imageType)
        {
            Transform target = FindOrCreateUniqueActionEffectChild(button, objectName);
            ApplyLocalRect(target.GetComponent<RectTransform>(), localRect, parentSize);
            Image image = target.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.clear;
            image.raycastTarget = false;
            image.preserveAspect = false;
            image.type = imageType;
            MarkComponentDirty(image);
            EditorUtility.SetDirty(target.gameObject);
            target.gameObject.SetActive(false);
            return image;
        }

        private static Transform FindOrCreateUniqueActionEffectChild(Transform button, string objectName)
        {
            Transform target = null;
            List<Transform> duplicates = null;
            for (int i = 0; i < button.childCount; i++)
            {
                Transform child = button.GetChild(i);
                if (!string.Equals(child.name, objectName, StringComparison.Ordinal))
                {
                    continue;
                }

                bool childComesFromPrefabSource =
                    PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) != null;
                if (target == null || childComesFromPrefabSource)
                {
                    if (target != null)
                    {
                        duplicates ??= new List<Transform>();
                        duplicates.Add(target);
                    }

                    target = child;
                }
                else
                {
                    duplicates ??= new List<Transform>();
                    duplicates.Add(child);
                }
            }

            if (target == null)
            {
                target = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
                target.SetParent(button, worldPositionStays: false);
            }
            else
            {
                target.gameObject.SetActive(true);
            }

            if (duplicates != null)
            {
                for (int i = 0; i < duplicates.Count; i++)
                {
                    if (duplicates[i] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(duplicates[i].gameObject);
                    }
                }

                EditorUtility.SetDirty(button.gameObject);
            }

            return target;
        }

        private static Rect ResolveActionEffectRect(Vector2 buttonSize, float scale)
        {
            return ResolveActionEffectRect(buttonSize, scale, Vector2.zero);
        }

        private static Rect ResolveActionEffectRect(Vector2 buttonSize, float scale, Vector2 centerOffset)
        {
            float side = Mathf.Min(buttonSize.x, buttonSize.y) * scale;
            float x = (buttonSize.x - side) * 0.5f + centerOffset.x;
            float y = (buttonSize.y - side) * 0.5f - centerOffset.y;
            return new Rect(x, y, side, side);
        }

        private static Vector2 ResolveActionEffectCenterOffset(CombatHudActionId actionId)
        {
            return actionId switch
            {
                CombatHudActionId.Dodge => new Vector2(-9f, 8f),
                CombatHudActionId.Skill1 => new Vector2(8f, -2.5f),
                _ => Vector2.zero
            };
        }

        private static void BindActionSlotEffectImages(
            CombatHudPresenter presenter,
            CombatHudActionId actionId,
            Image readyGlowImage,
            Image readyProgressFill)
        {
            if (presenter == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(presenter);
            SerializedProperty actionSlots = serializedObject.FindProperty("actionSlots");
            if (actionSlots == null || !actionSlots.isArray)
            {
                return;
            }

            for (int i = 0; i < actionSlots.arraySize; i++)
            {
                SerializedProperty element = actionSlots.GetArrayElementAtIndex(i);
                SerializedProperty actionIdProperty = element.FindPropertyRelative("actionId");
                if (actionIdProperty == null || (CombatHudActionId)actionIdProperty.intValue != actionId)
                {
                    continue;
                }

                SerializedProperty glowProperty = element.FindPropertyRelative("readyGlowImage");
                if (glowProperty != null)
                {
                    glowProperty.objectReferenceValue = readyGlowImage;
                }

                SerializedProperty progressProperty = element.FindPropertyRelative("readyProgressFill");
                if (progressProperty != null)
                {
                    progressProperty.objectReferenceValue = readyProgressFill;
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                MarkComponentDirty(presenter);
                return;
            }
        }

        private static void BindExistingActionSlotEffectImages(GameObject canvasRoot, CombatHudPresenter presenter)
        {
            Transform dodgeButton = FindHudDescendant(canvasRoot.transform, "DodgeButton");
            Image dodgeGlow = FindDirectChildImage(dodgeButton, "ActionReadyGlow");
            Image dodgeProgress = FindDirectChildImage(dodgeButton, "DodgeCooldownRing");
            if (dodgeGlow != null || dodgeProgress != null)
            {
                BindActionSlotEffectImages(
                    presenter,
                    CombatHudActionId.Dodge,
                    dodgeGlow,
                    dodgeProgress);
            }

            Transform skill1Button = FindHudDescendant(canvasRoot.transform, "Skill1Button");
            Image skill1Glow = FindDirectChildImage(skill1Button, "ActionReadyGlow");
            if (skill1Glow != null)
            {
                BindActionSlotEffectImages(
                    presenter,
                    CombatHudActionId.Skill1,
                    skill1Glow,
                    null);
            }
        }

        private static Image FindDirectChildImage(Transform root, string objectName)
        {
            Transform target = root != null ? root.Find(objectName) : null;
            return target != null ? target.GetComponent<Image>() : null;
        }

        private static void ConfigureSummonSlotPresentation(
            GameObject hudRoot,
            string buttonName,
            Vector2 buttonSize,
            int fontSize,
            Sprite iconSprite,
            Material disabledIconMaterial,
            Sprite progressRingSprite,
            Sprite readyGlowSprite,
            Sprite readySparkSprite)
        {
            Transform button = FindHudDescendant(hudRoot.transform, buttonName);
            if (button == null)
            {
                return;
            }

            Rect labelRect = ResolveSummonLabelRect(buttonSize);
            Rect stateRect = ResolveSummonStateRect(buttonSize);

            ConfigureLocalImage(
                button,
                "ReadyGlow",
                ResolveSummonReadyEffectRect(buttonSize, 1.18f),
                buttonSize,
                readyGlowSprite,
                new Color(1f, 0.78f, 0.2f, 0f),
                preserveAspect: false);
            ConfigureSummonSlotProgressFill(button, buttonSize, progressRingSprite);
            ConfigureLocalImage(
                button,
                "IconDisabled",
                ResolveSummonIconRect(buttonSize),
                buttonSize,
                iconSprite,
                new Color(0.26f, 0.28f, 0.31f, 0f),
                disabledIconMaterial);
            ConfigureSummonSlotDisabledIcon(button);
            ConfigureLocalImage(
                button,
                "Icon",
                ResolveSummonIconRect(buttonSize),
                buttonSize,
                iconSprite,
                CombatHudSummonIconColor);
            ConfigureLocalImage(
                button,
                "ReadyRing",
                ResolveSummonReadyEffectRect(buttonSize, 1.05f),
                buttonSize,
                progressRingSprite,
                new Color(1f, 0.92f, 0.38f, 0f),
                preserveAspect: false);
            ConfigureLocalImage(
                button,
                "ReadySparkRing",
                ResolveSummonReadyEffectRect(buttonSize, 0.94f),
                buttonSize,
                readySparkSprite,
                new Color(0.42f, 0.98f, 1f, 0f),
                preserveAspect: false);
            ConfigureLocalText(
                button,
                "Label",
                labelRect,
                buttonSize,
                CombatHudSummonLabelColor,
                Mathf.Max(18, fontSize + 4));
            ConfigureLocalText(
                button,
                "State",
                stateRect,
                buttonSize,
                CombatHudSummonStateColor,
                Mathf.Max(20, fontSize + 5));
            ApplySummonSlotVisualOrder(button);
        }

        private static Rect ResolveSummonLabelRect(Vector2 buttonSize)
        {
            float width = buttonSize.x * 0.36f;
            float height = buttonSize.y * 0.18f;
            float x = buttonSize.x * 0.11f;
            float y = buttonSize.y * 0.14f;
            return new Rect(x, y, width, height);
        }

        private static Rect ResolveSummonStateRect(Vector2 buttonSize)
        {
            float width = buttonSize.x * 0.78f;
            float height = buttonSize.y * 0.28f;
            float x = (buttonSize.x - width) * 0.5f;
            float y = buttonSize.y * 0.62f;
            return new Rect(x, y, width, height);
        }

        private static Rect ResolveSummonIconRect(Vector2 buttonSize)
        {
            float width = buttonSize.x * 0.92f;
            float height = buttonSize.y * 0.92f;
            float x = (buttonSize.x - width) * 0.5f;
            float y = (buttonSize.y - height) * 0.5f;
            return new Rect(x, y, width, height);
        }

        private static Rect ResolveSummonReadyEffectRect(Vector2 buttonSize, float scale)
        {
            float width = buttonSize.x * scale;
            float height = buttonSize.y * scale;
            float x = (buttonSize.x - width) * 0.5f;
            float y = (buttonSize.y - height) * 0.5f;
            return new Rect(x, y, width, height);
        }

        private static void ConfigureLocalImage(
            Transform root,
            string objectName,
            Rect localRect,
            Vector2 parentSize,
            Sprite sprite,
            Color color,
            Material material = null,
            bool preserveAspect = true)
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
            image.material = material;
            image.raycastTarget = false;
            image.preserveAspect = preserveAspect;
            image.type = Image.Type.Simple;
            MarkComponentDirty(image);
            EditorUtility.SetDirty(target.gameObject);
        }

        private static void ApplySummonSlotVisualOrder(Transform button)
        {
            string[] orderedChildren =
            {
                "ReadyGlow",
                "Icon",
                "IconDisabled",
                "CooldownFill",
                "ReadyRing",
                "ReadySparkRing",
                "Label",
                "State"
            };

            for (int i = 0; i < orderedChildren.Length; i++)
            {
                Transform child = button.Find(orderedChildren[i]);
                if (child != null)
                {
                    child.SetAsLastSibling();
                    EditorUtility.SetDirty(child.gameObject);
                }
            }
        }

        private static void ConfigureSummonSlotDisabledIcon(Transform button)
        {
            Transform target = button.Find("IconDisabled");
            Image image = target != null ? target.GetComponent<Image>() : null;
            if (image == null)
            {
                return;
            }

            image.raycastTarget = false;
            image.preserveAspect = true;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = false;
            image.fillAmount = 1f;
            MarkComponentDirty(image);
            EditorUtility.SetDirty(image.gameObject);
        }

        private static void ConfigureSummonSlotProgressFill(Transform button, Vector2 buttonSize, Sprite progressRingSprite)
        {
            Transform fill = FindHudDescendant(button, "CooldownFill");
            if (fill == null)
            {
                fill = new GameObject("CooldownFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
                fill.SetParent(button, worldPositionStays: false);
            }

            fill.gameObject.SetActive(true);
            if (button.childCount > 1)
            {
                fill.SetSiblingIndex(1);
            }

            ApplyLocalRect(fill.GetComponent<RectTransform>(), new Rect(Vector2.zero, buttonSize), buttonSize);
            Image image = fill.GetComponent<Image>();
            image.sprite = progressRingSprite;
            image.color = Color.clear;
            image.raycastTarget = false;
            image.preserveAspect = false;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = 0f;
            MarkComponentDirty(image);
            EditorUtility.SetDirty(fill.gameObject);
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
            target.SetAsLastSibling();
            ApplyLocalRect(target.GetComponent<RectTransform>(), localRect, parentSize);
            Text text = target.GetComponent<Text>();
            SetTextVisible(text, true, color, fontSize);
            if (string.Equals(objectName, "State", StringComparison.Ordinal))
            {
                text.lineSpacing = 0.86f;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 14;
                text.resizeTextMaxSize = fontSize;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                MarkComponentDirty(text);
            }

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

        private static void EnsureCombatHudUiMaterials()
        {
            EnsureAssetFolder("Assets/_Game/UI/CombatHud", "Materials");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(CombatHudDisabledSummonIconMaterialPath);
            Shader shader = Shader.Find(CombatHudDisabledSummonIconShaderName);
            if (shader == null)
            {
                throw new InvalidOperationException(
                    $"Missing summon HUD grayscale shader '{CombatHudDisabledSummonIconShaderName}'.");
            }

            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "DB_UI_SummonIconDisabledGrayscale"
                };
                AssetDatabase.CreateAsset(material, CombatHudDisabledSummonIconMaterialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = new Color(0.88f, 0.9f, 0.92f, 1f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureCombatHudUiGeneratedSprites()
        {
            EnsureAssetFolder("Assets/_Game/UI/CombatHud", "Generated");
            EnsureGeneratedSpriteTexture(CombatHudSummonProgressRingSpritePath, CreateSummonProgressRingPixel);
            EnsureGeneratedSpriteTexture(CombatHudSummonReadyGlowSpritePath, CreateSummonReadyGlowPixel);
            EnsureGeneratedSpriteTexture(CombatHudSummonReadySparkSpritePath, CreateSummonReadySparkPixel);
            EnsureGeneratedSpriteTexture(CombatHudActionCooldownRingSpritePath, CreateActionCooldownRingPixel, forceRegenerate: true);
            EnsureGeneratedSpriteTexture(CombatHudActionReadyGlowSpritePath, CreateActionReadyGlowPixel, forceRegenerate: true);
        }

        private delegate Color32 GeneratedUiPixel(int x, int y, int size);

        private static void EnsureGeneratedSpriteTexture(
            string assetPath,
            GeneratedUiPixel pixel,
            bool forceRegenerate = false)
        {
            string absolutePath = AssetPathToAbsolutePath(assetPath);
            if (forceRegenerate || !File.Exists(absolutePath))
            {
                const int size = 192;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

                Color32[] pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        pixels[y * size + x] = pixel(x, y, size);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            }

            ConfigureGeneratedSpriteImporter(assetPath);
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Expected project asset path, got {assetPath}.");
            }

            string relativeToAssets = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relativeToAssets);
        }

        private static void ConfigureGeneratedSpriteImporter(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
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

        private static Color32 CreateSummonProgressRingPixel(int x, int y, int size)
        {
            float distance = DistanceFromCenter01(x, y, size);
            float outer = 1f - Mathf.SmoothStep(0.88f, 0.98f, distance);
            float inner = Mathf.SmoothStep(0.58f, 0.7f, distance);
            float ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.78f) / 0.052f);
            byte alpha = (byte)Mathf.RoundToInt(255f * outer * inner * ring);
            return new Color32(255, 255, 255, alpha);
        }

        private static Color32 CreateSummonReadyGlowPixel(int x, int y, int size)
        {
            float distance = DistanceFromCenter01(x, y, size);
            float glow = 1f - Mathf.SmoothStep(0.18f, 0.98f, distance);
            float halo = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.72f) / 0.24f) * 0.42f;
            float alpha01 = Mathf.Clamp01(glow * glow * 0.72f + halo);
            byte alpha = (byte)Mathf.RoundToInt(255f * alpha01);
            return new Color32(255, 255, 255, alpha);
        }

        private static Color32 CreateSummonReadySparkPixel(int x, int y, int size)
        {
            float nx = ((x + 0.5f) / size - 0.5f) * 2f;
            float ny = ((y + 0.5f) / size - 0.5f) * 2f;
            float distance = Mathf.Sqrt(nx * nx + ny * ny);
            float degrees = Mathf.Atan2(ny, nx) * Mathf.Rad2Deg;
            float nearestSpark = Mathf.Round(degrees / 30f) * 30f;
            float angular = Mathf.Clamp01(1f - Mathf.Abs(Mathf.DeltaAngle(degrees, nearestSpark)) / 4.8f);
            float radial = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.78f) / 0.075f);
            float outer = 1f - Mathf.SmoothStep(0.91f, 1f, distance);
            byte alpha = (byte)Mathf.RoundToInt(255f * angular * radial * outer);
            return new Color32(255, 255, 255, alpha);
        }

        private static Color32 CreateActionCooldownRingPixel(int x, int y, int size)
        {
            float distance = DistanceFromCenter01(x, y, size);
            float outerFade = 1f - SmoothStep01(0.985f, 1f, distance);
            float innerFade = SmoothStep01(0.84f, 0.90f, distance);
            float core = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.94f) / 0.026f);
            float softEdge = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.94f) / 0.074f) * 0.34f;
            byte alpha = (byte)Mathf.RoundToInt(215f * outerFade * innerFade * Mathf.Clamp01(core + softEdge));
            return new Color32(255, 255, 255, alpha);
        }

        private static Color32 CreateActionReadyGlowPixel(int x, int y, int size)
        {
            float distance = DistanceFromCenter01(x, y, size);
            float outerFade = 1f - SmoothStep01(0.965f, 1f, distance);
            float innerFade = SmoothStep01(0.72f, 0.84f, distance);
            float broadHalo = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.89f) / 0.12f);
            float rim = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.92f) / 0.044f);
            float alpha01 = Mathf.Clamp01((rim * 0.54f + broadHalo * 0.26f) * outerFade * innerFade);
            byte alpha = (byte)Mathf.RoundToInt(185f * alpha01);
            return new Color32(255, 255, 255, alpha);
        }

        private static float SmoothStep01(float edge0, float edge1, float value)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edge0, edge1, value));
        }

        private static float DistanceFromCenter01(int x, int y, int size)
        {
            float nx = ((x + 0.5f) / size - 0.5f) * 2f;
            float ny = ((y + 0.5f) / size - 0.5f) * 2f;
            return Mathf.Sqrt(nx * nx + ny * ny);
        }

        private static void EnsureAssetFolder(string parentPath, string folderName)
        {
            string childPath = $"{parentPath}/{folderName}";
            if (AssetDatabase.IsValidFolder(childPath))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parentPath))
            {
                throw new InvalidOperationException($"Missing asset folder {parentPath}.");
            }

            AssetDatabase.CreateFolder(parentPath, folderName);
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

            if (TryApplyResponsiveSideDesignRect(rectTransform, designRect))
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

        private static bool TryApplyResponsiveSideDesignRect(RectTransform rectTransform, Rect designRect)
        {
            bool pinLeft = designRect.xMax <= 700f;
            bool pinRight = designRect.xMin >= 1800f;
            if (!pinLeft && !pinRight)
            {
                return false;
            }

            bool pinTop = designRect.yMin < DimensionHudDesignResolution.y * 0.5f;
            float rightInset = DimensionHudDesignResolution.x - designRect.xMax;
            float bottomInset = DimensionHudDesignResolution.y - designRect.yMax;
            if (pinLeft && pinTop)
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0f, 1f);
                rectTransform.anchoredPosition = new Vector2(designRect.xMin, -designRect.yMin);
            }
            else if (pinLeft)
            {
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.anchoredPosition = new Vector2(designRect.xMin, bottomInset);
            }
            else if (pinTop)
            {
                rectTransform.anchorMin = new Vector2(1f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.pivot = new Vector2(1f, 1f);
                rectTransform.anchoredPosition = new Vector2(-rightInset, -designRect.yMin);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(1f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 0f);
                rectTransform.pivot = new Vector2(1f, 0f);
                rectTransform.anchoredPosition = new Vector2(-rightInset, bottomInset);
            }

            rectTransform.sizeDelta = new Vector2(designRect.width, designRect.height);
            rectTransform.localScale = Vector3.one;
            MarkComponentDirty(rectTransform);
            return true;
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
