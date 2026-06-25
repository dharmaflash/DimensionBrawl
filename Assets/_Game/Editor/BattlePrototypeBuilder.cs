using System.Collections.Generic;
using DimensionBrawl.Editor;
using IsekaiBrawl.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IsekaiBrawl.EditorTools
{
    public static class BattlePrototypeBuilder
    {
        private const string RootPath = "Assets/_Game";
        private const string ScenePath = RootPath + "/Scenes/Battle.unity";
        private const string MetaScenePath = RootPath + "/Scenes/Meta.unity";
        private const string PlayerPrefabPath = RootPath + "/Prefabs/Player/Player.prefab";
        private const string ProjectilePrefabPath = RootPath + "/Prefabs/Enemies/EnemyProjectile.prefab";
        private const string SlotPrefabPath = RootPath + "/Prefabs/UI/CardSlotUI.prefab";
        private const string PlayerAnimatorControllerPath = RootPath + "/Animations/BattlePrototypePlayer.controller";
        private const string EnemyAnimatorControllerPath = RootPath + "/Animations/BattlePrototypeEnemy.controller";
        private const string PlayerVisualPrefabPath = ActionFoundationInoriPlayerVisualAssetSetup.ModelPath;
        private const string IdleAnimationPath =
            RootPath + "/Art/Animations/Cinematics/Inori/KawaiiP0/CIN_CombatReady.fbx";
        private const string WalkAnimationPath =
            RootPath + "/Art/Animations/Cinematics/Inori/KawaiiP0/CIN_BackViewProjectileAim.fbx";
        private const string CastAnimationPath =
            RootPath + "/Art/Animations/Cinematics/Inori/KawaiiP0/CIN_QTEMagicShot.fbx";

        [MenuItem("Tools/IsekaiBrawl/Build Battle Prototype")]
        public static void BuildBattlePrototype()
        {
            EnsureFolders();
            EnsureTagsAndLayers();

            PrototypeAssets assets = CreatePrototypeAssets();
            BuildMetaScene(assets.FontAsset);
            BuildScene(assets);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Battle prototype scene and assets were generated at Assets/_Game.");
        }

        [MenuItem("Tools/IsekaiBrawl/Repair Open Battle Scene References")]
        public static void RepairOpenBattleSceneReferencesMenu()
        {
            RepairOpenBattleSceneReferences();
            Debug.Log("Open Battle scene references were repaired where possible.");
        }

        public static void BuildBattlePrototypeBatch()
        {
            BuildBattlePrototype();
            EditorApplication.Exit(0);
        }

        private static void RepairOpenBattleSceneReferences()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            bool changed = false;
            PrototypeAssets assets = CreatePrototypeAssets();

            CardHandUI cardHandUi = Object.FindFirstObjectByType<CardHandUI>();
            if (cardHandUi != null)
            {
                SerializedObject cardHandSerialized = new(cardHandUi);
                SerializedProperty slotPrefabProperty = cardHandSerialized.FindProperty("slotPrefab");
                if (slotPrefabProperty != null && slotPrefabProperty.objectReferenceValue == null && assets.CardSlotPrefab != null)
                {
                    slotPrefabProperty.objectReferenceValue = assets.CardSlotPrefab;
                    cardHandSerialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(cardHandUi);
                    changed = true;
                }
            }

            EnemyAI enemyAi = Object.FindFirstObjectByType<EnemyAI>();
            if (enemyAi != null)
            {
                SerializedObject enemyAiSerialized = new(enemyAi);
                SerializedProperty projectilePrefabProperty = enemyAiSerialized.FindProperty("projectilePrefab");
                if (projectilePrefabProperty != null && projectilePrefabProperty.objectReferenceValue == null && assets.ProjectilePrefab != null)
                {
                    projectilePrefabProperty.objectReferenceValue = assets.ProjectilePrefab;
                    enemyAiSerialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(enemyAi);
                    changed = true;
                }
            }

            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                if (playerController.GetComponent<PlayerSkillController>() == null)
                {
                    playerController.gameObject.AddComponent<PlayerSkillController>();
                    EditorUtility.SetDirty(playerController.gameObject);
                    changed = true;
                }

                changed |= EnsureCharacterAnimationBinding(
                    playerController.transform,
                    assets.PlayerVisualPrefab,
                    assets.PlayerAnimatorController,
                    assets.CharacterAvatar,
                    out Animator playerAnimator);
                changed |= TrySetObjectReference(playerController, "characterAnimator", playerAnimator);
            }

            if (enemyAi != null)
            {
                changed |= EnsureCharacterAnimationBinding(
                    enemyAi.transform,
                    assets.PlayerVisualPrefab,
                    assets.EnemyAnimatorController,
                    assets.CharacterAvatar,
                    out Animator enemyAnimator);
                changed |= TrySetObjectReference(enemyAi, "characterAnimator", enemyAnimator);
            }

            if (changed)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(activeScene);
                    EditorSceneManager.SaveScene(activeScene);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                RootPath + "/Animations",
                RootPath + "/Materials",
                RootPath + "/Prefabs/Player",
                RootPath + "/Prefabs/Summons",
                RootPath + "/Prefabs/Enemies",
                RootPath + "/Prefabs/UI",
                RootPath + "/Scenes",
                RootPath + "/ScriptableObjects/SummonData",
                RootPath + "/ScriptableObjects/EnemyData"
            };

            foreach (string folder in folders)
            {
                EnsureFolder(folder);
            }
        }

        private static PrototypeAssets CreatePrototypeAssets()
        {
            ActionFoundationInoriPlayerVisualAssetSetup.EnsureInoriPlayerVisualAssets();

            PrototypeAssets assets = new();
            assets.UiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            assets.UiKnobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            assets.FontAsset = TMP_Settings.defaultFontAsset;
            assets.PlayerVisualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerVisualPrefabPath);
            assets.CharacterAvatar = LoadCharacterAvatar(assets.PlayerVisualPrefab);
            assets.PlayerAnimatorController = LoadOrCreatePlayerAnimatorController();
            assets.EnemyAnimatorController = LoadOrCreateEnemyAnimatorController();
            assets.PlayerPrefab = LoadOrCreatePlayerPrefab(assets.PlayerVisualPrefab, assets.PlayerAnimatorController, assets.CharacterAvatar);
            assets.ProjectilePrefab = LoadOrCreateProjectilePrefab();
            assets.CardSlotPrefab = LoadOrCreateCardSlotPrefab(assets.FontAsset, assets.UiSprite);
            assets.SummonDataAssets = LoadOrCreateSummonAssets();
            return assets;
        }

        private static void BuildScene(PrototypeAssets assets)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateDirectionalLight();
            GameObject corridor = CreatePrimitiveObject("Corridor", PrimitiveType.Plane, new Vector3(0f, 0f, 42f), Vector3.zero, new Vector3(1.52f, 1f, 8.4f), CreateMaterial("CorridorFloor", new Color(0.2f, 0.22f, 0.28f)));
            CreatePrimitiveObject("LeftWall", PrimitiveType.Cube, new Vector3(-6.6f, 1.5f, 42f), Vector3.zero, new Vector3(0.5f, 3f, 85.5f), CreateMaterial("CorridorWall", new Color(0.14f, 0.16f, 0.2f)));
            CreatePrimitiveObject("RightWall", PrimitiveType.Cube, new Vector3(6.6f, 1.5f, 42f), Vector3.zero, new Vector3(0.5f, 3f, 85.5f), CreateMaterial("CorridorWall", new Color(0.14f, 0.16f, 0.2f)));

            GameObject playerBase = CreatePrimitiveObject("PlayerBase", PrimitiveType.Cube, new Vector3(0f, 0f, 0f), Vector3.zero, new Vector3(4.8f, 0.55f, 1.2f), CreateMaterial("PlayerBase", new Color(0.22f, 0.45f, 0.85f)));
            SetTagIfExists(playerBase, "PlayerBase");
            SetLayerByName(playerBase, "PlayerBase");

            GameObject enemyBase = CreatePrimitiveObject("EnemyBase", PrimitiveType.Cube, new Vector3(0f, 0f, 84f), Vector3.zero, new Vector3(4.8f, 0.55f, 1.2f), CreateMaterial("EnemyBase", new Color(0.85f, 0.25f, 0.2f)));
            SetTagIfExists(enemyBase, "EnemyBase");
            SetLayerByName(enemyBase, "EnemyBase");

            Transform playerSpawn = CreateMarker("PlayerSpawn", new Vector3(0f, 0f, 4.2f));
            Transform enemySpawn = CreateMarker("EnemySpawn", new Vector3(0f, 0f, 79.8f));
            Transform summonSpawnPoint = CreateMarker("SummonSpawnPoint", new Vector3(0f, 0f, 10.4f));
            Transform enemySummonSpawnPoint = CreateMarker("EnemySummonSpawnPoint", new Vector3(0f, 0f, 73.6f));
            Transform battlefieldLayoutRoot = CreateBattlefieldLayoutRoot();
            Transform laneAnchorRoot = CreateLaneAnchorSetsRoot();

            GameObject playerObject = PrefabUtility.InstantiatePrefab(assets.PlayerPrefab) as GameObject;
            playerObject.transform.position = playerSpawn.position;
            playerObject.transform.rotation = Quaternion.identity;
            if (playerObject.GetComponent<PlayerSkillController>() == null)
            {
                playerObject.AddComponent<PlayerSkillController>();
            }

            PlayerController playerController = playerObject.GetComponent<PlayerController>();

            GameObject enemyRoot = new("EnemyController");
            enemyRoot.transform.position = enemySpawn.position;
            SetTagIfExists(enemyRoot, "Enemy");
            Animator enemyAnimator = AttachCharacterVisual(
                enemyRoot.transform,
                assets.PlayerVisualPrefab,
                assets.EnemyAnimatorController,
                assets.CharacterAvatar,
                Vector3.one);

            Transform projectileSpawn = CreateMarker("ProjectileSpawn", enemySpawn.position + new Vector3(0f, 1.5f, -1.1f));
            projectileSpawn.SetParent(enemyRoot.transform, true);

            GameObject systemsRoot = new("BattleSystems");
            GameManager gameManager = systemsRoot.AddComponent<GameManager>();
            BattleManager battleManager = systemsRoot.AddComponent<BattleManager>();
            BattleEnergySystem energySystem = systemsRoot.AddComponent<BattleEnergySystem>();
            SummonSpawner summonSpawner = systemsRoot.AddComponent<SummonSpawner>();
            EnemyAI enemyAI = enemyRoot.AddComponent<EnemyAI>();

            SetObjectReference(battleManager, "playerSpawn", playerSpawn);
            SetObjectReference(battleManager, "enemySpawn", enemySpawn);
            SetObjectReference(battleManager, "summonSpawnPoint", summonSpawnPoint);
            SetObjectReference(battleManager, "enemySummonSpawnPoint", enemySummonSpawnPoint);
            SetObjectReference(battleManager, "playerBaseTransform", playerBase.transform);
            SetObjectReference(battleManager, "enemyBaseTransform", enemyBase.transform);
            SetObjectReference(battleManager, "playerController", playerController);
            SetObjectReference(battleManager, "battlefieldLayoutRoot", battlefieldLayoutRoot);
            SetObjectReference(battleManager, "laneAnchorRoot", laneAnchorRoot);

            SetObjectReference(summonSpawner, "summonSpawnPoint", summonSpawnPoint);
            SetObjectList(summonSpawner, "availableSummons", assets.SummonDataAssets);

            SetObjectReference(enemyAI, "summonSpawnPoint", enemySummonSpawnPoint);
            SetObjectReference(enemyAI, "projectileSpawnPoint", projectileSpawn);
            SetObjectReference(enemyAI, "projectilePrefab", assets.ProjectilePrefab);
            SetObjectList(enemyAI, "enemyDeck", assets.SummonDataAssets);
            SetObjectReference(enemyAI, "characterAnimator", enemyAnimator);

            Camera mainCamera = CreateCamera(playerController.transform);
            BattleCamera battleCamera = mainCamera.GetComponent<BattleCamera>();
            battleCamera.Target = playerController.transform;

            Canvas canvas = CreateCanvas();
            CreateEventSystem();
            CreateHud(canvas.transform, assets, summonSpawner, enemyAI, playerController, battleManager, energySystem);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void BuildMetaScene(TMP_FontAsset fontAsset)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateDirectionalLight();

            GameObject cameraObject = new("Main Camera");
            SetTagIfExists(cameraObject, "MainCamera");
            Camera cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Canvas canvas = CreateCanvas();
            CreateEventSystem();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Stretch(canvasRect);

            GameObject root = new("MetaRoot", typeof(RectTransform), typeof(Image), typeof(MetaMenuController));
            root.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(680f, 440f);
            rootRect.anchoredPosition = Vector2.zero;

            Image rootImage = root.GetComponent<Image>();
            rootImage.color = new Color(0f, 0f, 0f, 0.35f);

            CreateText(
                "Title",
                rootRect,
                "\uCC28\uC6D0 \uB300\uB09C\uD22C",
                42f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(520f, 60f),
                new Vector2(0f, -40f),
                fontAsset);

            CreateText(
                "Subtitle",
                rootRect,
                "Select the prototype battle flow",
                22f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(620f, 40f),
                new Vector2(0f, -84f),
                fontAsset);

            Button storyButton = CreateButton("StoryButton", rootRect, "Story PvE", new Vector2(260f, 56f), new Vector2(0f, 238f), new PrototypeAssets { FontAsset = fontAsset, UiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd") });
            Button asyncPvpButton = CreateButton("AsyncPvpButton", rootRect, "Async PvP", new Vector2(260f, 56f), new Vector2(0f, 166f), new PrototypeAssets { FontAsset = fontAsset, UiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd") });
            Button sandboxButton = CreateButton("SandboxButton", rootRect, "Battle Test", new Vector2(260f, 56f), new Vector2(0f, 94f), new PrototypeAssets { FontAsset = fontAsset, UiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd") });

            CreateText("StoryHint", rootRect, "Story chapter enemy deck flow", 16f, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(360f, 24f), new Vector2(0f, 214f), fontAsset);
            CreateText("AsyncHint", rootRect, "Fight an imported rival deck asynchronously", 16f, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420f, 24f), new Vector2(0f, 142f), fontAsset);
            CreateText("SandboxHint", rootRect, "Jump straight into the prototype battle scene", 16f, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420f, 24f), new Vector2(0f, 70f), fontAsset);

            TMP_Text descriptionText = CreateText(
                "DescriptionText",
                rootRect,
                "Directly control the hero, summon units, and break the enemy base.",
                18f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(600f, 40f),
                new Vector2(0f, 26f),
                fontAsset);

            MetaMenuController metaMenuController = root.GetComponent<MetaMenuController>();
            SetObjectReference(metaMenuController, "storyButton", storyButton);
            SetObjectReference(metaMenuController, "asyncPvpButton", asyncPvpButton);
            SetObjectReference(metaMenuController, "sandboxButton", sandboxButton);
            SetObjectReference(metaMenuController, "descriptionText", descriptionText);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MetaScenePath);
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightObject = new("Directional Light");
            Light lightComponent = lightObject.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static Camera CreateCamera(Transform target)
        {
            GameObject cameraObject = new("Main Camera");
            SetTagIfExists(cameraObject, "MainCamera");

            Camera cameraComponent = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            BattleCamera battleCamera = cameraObject.AddComponent<BattleCamera>();
            cameraObject.AddComponent<CameraShake>();

            cameraObject.transform.position = target.position + new Vector3(0f, 7.2f, -11.8f);
            cameraObject.transform.LookAt(target.position + Vector3.up);
            battleCamera.Target = target;
            battleCamera.ConfigureOffset(new Vector3(0f, 7.2f, -11.8f));
            battleCamera.ConfigureLookAhead(new Vector3(0f, 1.45f, 11.6f));
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
            return cameraComponent;
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new("BattleCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static void CreateHud(
            Transform canvasRoot,
            PrototypeAssets assets,
            SummonSpawner summonSpawner,
            EnemyAI enemyAI,
            PlayerController playerController,
            BattleManager battleManager,
            BattleEnergySystem energySystem)
        {
            BattleHUD battleHud = new GameObject("BattleHUD", typeof(RectTransform)).AddComponent<BattleHUD>();
            battleHud.transform.SetParent(canvasRoot, false);
            RectTransform hudRoot = battleHud.GetComponent<RectTransform>();
            hudRoot.anchorMin = Vector2.zero;
            hudRoot.anchorMax = Vector2.one;
            hudRoot.offsetMin = Vector2.zero;
            hudRoot.offsetMax = Vector2.zero;

            Slider enemyBaseSlider = CreateSlider("EnemyBaseSlider", hudRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 24f), new Vector2(0f, -28f), assets);
            TMP_Text enemyBaseText = CreateText("EnemyBaseText", hudRoot, "Enemy Base", 20, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 30f), new Vector2(0f, -4f), assets.FontAsset);
            TMP_Text timerText = CreateText("TimerText", hudRoot, "03:00", 28, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(180f, 40f), new Vector2(0f, -62f), assets.FontAsset);

            Slider playerHpSlider = CreateSlider("PlayerHpSlider", hudRoot, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 220f), new Vector2(42f, 0f), assets, true);
            TMP_Text playerHpText = CreateText("PlayerHpText", hudRoot, "Player HP", 18, TextAlignmentOptions.Center, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(110f, 30f), new Vector2(70f, 132f), assets.FontAsset);

            Slider energySlider = CreateSlider("EnergySlider", hudRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420f, 24f), new Vector2(0f, 150f), assets);
            TMP_Text energyText = CreateText("EnergyText", hudRoot, "Energy", 22, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(420f, 30f), new Vector2(0f, 176f), assets.FontAsset);

            GameObject handRoot = new("CardHandUI", typeof(RectTransform), typeof(Image), typeof(CardHandUI));
            handRoot.transform.SetParent(hudRoot, false);
            Image handImage = handRoot.GetComponent<Image>();
            handImage.sprite = assets.UiSprite;
            handImage.type = Image.Type.Sliced;
            handImage.color = new Color(0f, 0f, 0f, 0.35f);
            RectTransform handRect = handRoot.GetComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.5f, 0f);
            handRect.anchorMax = new Vector2(0.5f, 0f);
            handRect.sizeDelta = new Vector2(820f, 140f);
            handRect.anchoredPosition = new Vector2(0f, 60f);

            GameObject slotContainer = new("Slots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            slotContainer.transform.SetParent(handRoot.transform, false);
            RectTransform slotRect = slotContainer.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(760f, 110f);
            slotRect.anchoredPosition = Vector2.zero;
            HorizontalLayoutGroup layoutGroup = slotContainer.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;

            CardHandUI cardHandUi = handRoot.GetComponent<CardHandUI>();
            SetObjectReference(cardHandUi, "slotPrefab", assets.CardSlotPrefab);
            SetObjectReference(cardHandUi, "slotContainer", slotContainer.transform);
            SetObjectReference(cardHandUi, "summonSpawner", summonSpawner);
            SetObjectList(cardHandUi, "currentHand", assets.SummonDataAssets);

            GameObject enemyIntentRoot = new("EnemyCardStackUI", typeof(RectTransform), typeof(Image), typeof(EnemyCardStackUI));
            enemyIntentRoot.transform.SetParent(hudRoot, false);
            Image enemyIntentImage = enemyIntentRoot.GetComponent<Image>();
            enemyIntentImage.sprite = assets.UiSprite;
            enemyIntentImage.type = Image.Type.Sliced;
            enemyIntentImage.color = new Color(0f, 0f, 0f, 0.35f);
            RectTransform enemyIntentRect = enemyIntentRoot.GetComponent<RectTransform>();
            enemyIntentRect.anchorMin = new Vector2(1f, 1f);
            enemyIntentRect.anchorMax = new Vector2(1f, 1f);
            enemyIntentRect.sizeDelta = new Vector2(180f, 180f);
            enemyIntentRect.anchoredPosition = new Vector2(-110f, -80f);

            Image nextCardImage = CreateImage("NextCardImage", enemyIntentRect, assets.UiSprite, new Vector2(100f, 100f), new Vector2(0f, 20f));
            TMP_Text countdownText = CreateText("CountdownText", enemyIntentRect, "5.0s", 22, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(140f, 28f), new Vector2(0f, 18f), assets.FontAsset);
            EnemyCardStackUI enemyCardStackUi = enemyIntentRoot.GetComponent<EnemyCardStackUI>();
            SetObjectReference(enemyCardStackUi, "nextCardImage", nextCardImage);
            SetObjectReference(enemyCardStackUi, "countdownText", countdownText);
            SetObjectReference(enemyCardStackUi, "enemyAI", enemyAI);
            SetObjectReference(enemyCardStackUi, "fallbackSprite", assets.UiSprite);

            GameObject presentationRoot = new("BattlePresentationOverlay", typeof(RectTransform), typeof(BattlePresentationController));
            presentationRoot.transform.SetParent(hudRoot, false);
            RectTransform presentationRect = presentationRoot.GetComponent<RectTransform>();
            presentationRect.anchorMin = Vector2.zero;
            presentationRect.anchorMax = Vector2.one;
            presentationRect.offsetMin = Vector2.zero;
            presentationRect.offsetMax = Vector2.zero;

            GameObject resultRoot = new("BattleResultUI", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(BattleResultUI));
            resultRoot.transform.SetParent(hudRoot, false);
            Image resultBackground = resultRoot.GetComponent<Image>();
            resultBackground.sprite = assets.UiSprite;
            resultBackground.type = Image.Type.Sliced;
            resultBackground.color = new Color(0f, 0f, 0f, 0.6f);
            RectTransform resultRect = resultRoot.GetComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.sizeDelta = new Vector2(360f, 220f);
            resultRect.anchoredPosition = Vector2.zero;

            TMP_Text resultText = CreateText("ResultText", resultRect, "\uC2B9\uB9AC", 36, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(260f, 40f), new Vector2(0f, -44f), assets.FontAsset);
            Button restartButton = CreateButton("RestartButton", resultRect, "\uB2E4\uC2DC \uC2DC\uC791", new Vector2(180f, 48f), new Vector2(0f, 42f), assets);
            BattleResultUI battleResultUi = resultRoot.GetComponent<BattleResultUI>();
            SetObjectReference(battleResultUi, "canvasGroup", resultRoot.GetComponent<CanvasGroup>());
            SetObjectReference(battleResultUi, "resultText", resultText);
            SetObjectReference(battleResultUi, "restartButton", restartButton);

            SetObjectReference(battleHud, "enemyBaseHpSlider", enemyBaseSlider);
            SetObjectReference(battleHud, "enemyBaseHpText", enemyBaseText);
            SetObjectReference(battleHud, "timerText", timerText);
            SetObjectReference(battleHud, "energySlider", energySlider);
            SetObjectReference(battleHud, "energyText", energyText);
            SetObjectReference(battleHud, "playerHpSlider", playerHpSlider);
            SetObjectReference(battleHud, "playerHpText", playerHpText);
        }

        private static void CreateBattlefieldStructures()
        {
            CreateBattleStructure("BattleStructure_1", new Vector3(-2.3f, 0.8f, 15.2f), new Color(0.95f, 0.78f, 0.38f, 1f), 150f, 20f);
            CreateBattleStructure("BattleStructure_2", new Vector3(2.35f, 0.8f, 26f), new Color(0.58f, 0.9f, 1f, 1f), 150f, 20f);
            CreateBattleStructure("BattleStructure_3", new Vector3(-1.85f, 0.8f, 37.8f), new Color(0.95f, 0.78f, 0.38f, 1f), 150f, 20f);
            CreateBattleStructure("BattleStructure_4", new Vector3(1.95f, 0.8f, 48.6f), new Color(0.58f, 0.9f, 1f, 1f), 150f, 20f);
            CreateBattleStructure("BattleStructure_5", new Vector3(-2f, 0.8f, 60.4f), new Color(0.95f, 0.78f, 0.38f, 1f), 150f, 20f);
            CreateBattleStructure("BattleStructure_6", new Vector3(2.15f, 0.8f, 72.2f), new Color(0.58f, 0.9f, 1f, 1f), 150f, 20f);
        }

        private static void CreateBattleStructure(string name, Vector3 position, Color color, float maxHp, float energyReward)
        {
            GameObject structureObject = CreatePrimitiveObject(
                name,
                PrimitiveType.Cylinder,
                position,
                Vector3.zero,
                new Vector3(0.9f, 0.75f, 0.9f),
                CreateMaterial(name, color));

            BattleStructure structure = structureObject.AddComponent<BattleStructure>();
            structure.Configure(maxHp, energyReward);
        }

        private static Transform CreateBattlefieldLayoutRoot()
        {
            return new GameObject("BattlefieldLayout").transform;
        }

        private static Transform CreateLaneAnchorSetsRoot()
        {
            Transform root = new GameObject("LaneAnchorSets").transform;
            float laneHalfWidth = 6.25f;
            for (int laneIndex = 0; laneIndex < BattleLaneUtility.DefaultLaneCount; laneIndex++)
            {
                float laneX = BattleLaneUtility.GetLaneCenterX(laneIndex, laneHalfWidth);
                GameObject laneObject = new($"LaneAnchorSet_{laneIndex + 1}");
                laneObject.transform.SetParent(root, false);

                Transform rear = CreateLaneAnchor(laneObject.transform, "Rear", laneX, 0f, 6.2f);
                Transform support = CreateLaneAnchor(laneObject.transform, "SupportCover", laneX, 0f, 9.6f);
                Transform peek = CreateLaneAnchor(laneObject.transform, "Peek", laneX, 0f, 16.8f);
                Transform advance = CreateLaneAnchor(laneObject.transform, "AdvanceBase", laneX, 0f, 24.4f);

                LaneAnchorSet anchorSet = laneObject.AddComponent<LaneAnchorSet>();
                anchorSet.Configure(laneIndex, rear, support, peek, advance);
            }

            return root;
        }

        private static Transform CreateLaneAnchor(Transform parent, string name, float x, float y, float z)
        {
            Transform anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            anchor.position = new Vector3(x, y, z);
            return anchor;
        }

        private static Slider CreateSlider(
            string name,
            RectTransform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 anchoredPosition,
            PrototypeAssets assets,
            bool vertical = false)
        {
            GameObject sliderRoot = new(name, typeof(RectTransform), typeof(Slider));
            sliderRoot.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderRoot.GetComponent<RectTransform>();
            sliderRect.anchorMin = anchorMin;
            sliderRect.anchorMax = anchorMax;
            sliderRect.sizeDelta = size;
            sliderRect.anchoredPosition = anchoredPosition;

            GameObject background = new("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(sliderRoot.transform, false);
            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = assets.UiSprite;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            Stretch(background.GetComponent<RectTransform>());

            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderRoot.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(4f, 4f);
            fillAreaRect.offsetMax = new Vector2(-4f, -4f);

            GameObject fill = new("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.GetComponent<Image>();
            fillImage.sprite = assets.UiSprite;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = vertical ? new Color(0.24f, 0.8f, 0.95f, 1f) : new Color(0.2f, 0.85f, 0.55f, 1f);
            Stretch(fill.GetComponent<RectTransform>());

            Slider slider = sliderRoot.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = backgroundImage;
            slider.direction = vertical ? Slider.Direction.BottomToTop : Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Vector2 size, Vector2 anchoredPosition)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            return image;
        }

        private static TMP_Text CreateText(
            string name,
            RectTransform parent,
            string textValue,
            float fontSize,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 anchoredPosition,
            TMP_FontAsset font)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = textValue;
            return text;
        }

        private static Button CreateButton(string name, RectTransform parent, string label, Vector2 size, Vector2 anchoredPosition, PrototypeAssets assets)
        {
            GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = assets.UiSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.2f, 0.45f, 0.92f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            CreateText("Label", rect, label, 24f, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero, assets.FontAsset);
            return button;
        }

        private static CardSlotUI LoadOrCreateCardSlotPrefab(TMP_FontAsset font, Sprite uiSprite)
        {
            GameObject existingPrefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);
            CardSlotUI existingPrefab = existingPrefabRoot != null ? existingPrefabRoot.GetComponent<CardSlotUI>() : null;
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            GameObject root = new("CardSlotUI", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(IsekaiBrawl.Gameplay.CardSlotUI));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 110f);

            Image background = root.GetComponent<Image>();
            background.sprite = uiSprite;
            background.type = Image.Type.Sliced;
            background.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);

            Image artImage = CreateImage("CardArt", rect, uiSprite, new Vector2(84f, 64f), new Vector2(0f, 12f));
            TMP_Text costText = CreateText("CostText", rect, "0", 24f, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(80f, 24f), new Vector2(0f, 14f), font);

            IsekaiBrawl.Gameplay.CardSlotUI slotUi = root.GetComponent<IsekaiBrawl.Gameplay.CardSlotUI>();
            SetObjectReference(slotUi, "cardImage", artImage);
            SetObjectReference(slotUi, "costText", costText);
            SetObjectReference(slotUi, "buttonComponent", root.GetComponent<Button>());
            SetObjectReference(slotUi, "canvasGroup", root.GetComponent<CanvasGroup>());
            SetObjectReference(slotUi, "fallbackSprite", uiSprite);

            PrefabUtility.SaveAsPrefabAsset(root, SlotPrefabPath);
            Object.DestroyImmediate(root);
            GameObject savedRoot = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);
            return savedRoot != null ? savedRoot.GetComponent<CardSlotUI>() : null;
        }

        private static EnemyProjectile LoadOrCreateProjectilePrefab()
        {
            GameObject existingPrefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            EnemyProjectile existingPrefab = existingPrefabRoot != null ? existingPrefabRoot.GetComponent<EnemyProjectile>() : null;
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "EnemyProjectile";
            projectile.transform.localScale = Vector3.one * 0.35f;
            Object.DestroyImmediate(projectile.GetComponent<SphereCollider>());
            SphereCollider sphereCollider = projectile.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            Rigidbody rigidbodyComponent = projectile.AddComponent<Rigidbody>();
            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.useGravity = false;
            projectile.AddComponent<IsekaiBrawl.Gameplay.EnemyProjectile>();
            SetLayerByName(projectile, "Projectile");

            Renderer renderer = projectile.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateMaterial("EnemyProjectile", new Color(1f, 0.45f, 0.22f));

            PrefabUtility.SaveAsPrefabAsset(projectile, ProjectilePrefabPath);
            Object.DestroyImmediate(projectile);
            GameObject savedRoot = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            return savedRoot != null ? savedRoot.GetComponent<EnemyProjectile>() : null;
        }

        private static GameObject LoadOrCreatePlayerPrefab(GameObject visualPrefab, AnimatorController animatorController, Avatar avatar)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            bool prefabAlreadyExists = existingPrefab != null;
            GameObject player = prefabAlreadyExists ? PrefabUtility.LoadPrefabContents(PlayerPrefabPath) : new GameObject("Player");

            SetTagIfExists(player, "Player");

            CapsuleCollider collider = player.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = player.AddComponent<CapsuleCollider>();
            }

            collider.height = 2f;
            collider.radius = 0.35f;
            collider.center = new Vector3(0f, 1f, 0f);

            Rigidbody rigidbodyComponent = player.GetComponent<Rigidbody>();
            if (rigidbodyComponent == null)
            {
                rigidbodyComponent = player.AddComponent<Rigidbody>();
            }

            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.constraints = RigidbodyConstraints.FreezeRotation;

            PlayerController playerController = player.GetComponent<IsekaiBrawl.Gameplay.PlayerController>();
            if (playerController == null)
            {
                playerController = player.AddComponent<IsekaiBrawl.Gameplay.PlayerController>();
            }

            if (player.GetComponent<IsekaiBrawl.Gameplay.JustDodgeDetector>() == null)
            {
                player.AddComponent<IsekaiBrawl.Gameplay.JustDodgeDetector>();
            }

            if (player.GetComponent<IsekaiBrawl.Gameplay.PlayerSkillController>() == null)
            {
                player.AddComponent<IsekaiBrawl.Gameplay.PlayerSkillController>();
            }

            Animator playerAnimator = AttachCharacterVisual(player.transform, visualPrefab, animatorController, avatar, Vector3.one);
            SetObjectReference(playerController, "characterAnimator", playerAnimator);

            if (prefabAlreadyExists)
            {
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
                PrefabUtility.UnloadPrefabContents(player);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
                Object.DestroyImmediate(player);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        }

        private static Animator AttachCharacterVisual(
            Transform parent,
            GameObject visualPrefab,
            RuntimeAnimatorController animatorController,
            Avatar avatar,
            Vector3 localScale)
        {
            _ = EnsureCharacterAnimationBinding(parent, visualPrefab, animatorController, avatar, out Animator animator, localScale);
            return animator;
        }

        private static AnimatorController LoadOrCreatePlayerAnimatorController()
        {
            EnsurePrimaryClipLoop(IdleAnimationPath, true);
            EnsurePrimaryClipLoop(WalkAnimationPath, true);

            AnimatorController controller = LoadOrRebuildAnimatorController(PlayerAnimatorControllerPath);

            ConfigurePlayerAnimatorController(
                controller,
                LoadPrimaryAnimationClip(IdleAnimationPath),
                LoadPrimaryAnimationClip(WalkAnimationPath));
            return controller;
        }

        private static AnimatorController LoadOrCreateEnemyAnimatorController()
        {
            EnsurePrimaryClipLoop(IdleAnimationPath, true);
            EnsurePrimaryClipLoop(CastAnimationPath, false);

            AnimatorController controller = LoadOrRebuildAnimatorController(EnemyAnimatorControllerPath);

            ConfigureEnemyAnimatorController(
                controller,
                LoadPrimaryAnimationClip(IdleAnimationPath),
                LoadPrimaryAnimationClip(CastAnimationPath));
            return controller;
        }

        private static AnimatorController LoadOrRebuildAnimatorController(string assetPath)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (controller != null && controller.layers != null && controller.layers.Length > 0)
            {
                return controller;
            }

            if (controller != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            return AnimatorController.CreateAnimatorControllerAtPath(assetPath);
        }

        private static void ConfigurePlayerAnimatorController(AnimatorController controller, AnimationClip idleClip, AnimationClip walkClip)
        {
            if (controller == null || idleClip == null || walkClip == null)
            {
                return;
            }

            EnsureAnimatorParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            ResetStateMachine(stateMachine);

            AnimatorState idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;
            idleState.writeDefaultValues = true;

            AnimatorState walkState = stateMachine.AddState("Walk");
            walkState.motion = walkClip;
            walkState.writeDefaultValues = true;

            AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.08f;
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.08f;
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

            stateMachine.defaultState = idleState;
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureEnemyAnimatorController(AnimatorController controller, AnimationClip idleClip, AnimationClip castClip)
        {
            if (controller == null || idleClip == null || castClip == null)
            {
                return;
            }

            EnsureAnimatorParameter(controller, "Cast", AnimatorControllerParameterType.Trigger);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            ResetStateMachine(stateMachine);

            AnimatorState idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;
            idleState.writeDefaultValues = true;

            AnimatorState castState = stateMachine.AddState("Cast");
            castState.motion = castClip;
            castState.writeDefaultValues = true;

            AnimatorStateTransition anyToCast = stateMachine.AddAnyStateTransition(castState);
            anyToCast.hasExitTime = false;
            anyToCast.duration = 0.03f;
            anyToCast.canTransitionToSelf = false;
            anyToCast.AddCondition(AnimatorConditionMode.If, 0f, "Cast");

            AnimatorStateTransition castToIdle = castState.AddTransition(idleState);
            castToIdle.hasExitTime = true;
            castToIdle.exitTime = 0.92f;
            castToIdle.duration = 0.05f;

            stateMachine.defaultState = idleState;
            EditorUtility.SetDirty(controller);
        }

        private static void ResetStateMachine(AnimatorStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                return;
            }

            ChildAnimatorState[] states = stateMachine.states;
            for (int index = states.Length - 1; index >= 0; index--)
            {
                stateMachine.RemoveState(states[index].state);
            }

            AnimatorStateTransition[] anyStateTransitions = stateMachine.anyStateTransitions;
            for (int index = anyStateTransitions.Length - 1; index >= 0; index--)
            {
                stateMachine.RemoveAnyStateTransition(anyStateTransitions[index]);
            }
        }

        private static void EnsureAnimatorParameter(AnimatorController controller, string parameterName, AnimatorControllerParameterType parameterType)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == parameterName && parameter.type == parameterType)
                {
                    return;
                }
            }

            controller.AddParameter(parameterName, parameterType);
        }

        private static void EnsurePrimaryClipLoop(string assetPath, bool loopTime)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            ModelImporterClipAnimation[] sourceClips = importer.clipAnimations;
            if (sourceClips == null || sourceClips.Length == 0)
            {
                sourceClips = importer.defaultClipAnimations;
            }

            if (sourceClips == null || sourceClips.Length == 0)
            {
                return;
            }

            bool changed = false;
            for (int index = 0; index < sourceClips.Length; index++)
            {
                if (sourceClips[index].loopTime == loopTime)
                {
                    continue;
                }

                sourceClips[index].loopTime = loopTime;
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            importer.clipAnimations = sourceClips;
            importer.SaveAndReimport();
        }

        private static AnimationClip LoadPrimaryAnimationClip(string assetPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int index = 0; index < assets.Length; index++)
            {
                if (assets[index] is not AnimationClip clip)
                {
                    continue;
                }

                if (clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                {
                    continue;
                }

                return clip;
            }

            return null;
        }

        private static Avatar LoadCharacterAvatar(GameObject visualPrefab)
        {
            if (visualPrefab != null)
            {
                Animator visualAnimator = visualPrefab.GetComponent<Animator>();
                if (visualAnimator != null && visualAnimator.avatar != null && visualAnimator.avatar.isValid)
                {
                    return visualAnimator.avatar;
                }

                string visualPrefabPath = AssetDatabase.GetAssetPath(visualPrefab);
                Avatar dependencyAvatar = FindFirstValidAvatar(AssetDatabase.GetDependencies(visualPrefabPath, true));
                if (dependencyAvatar != null)
                {
                    return dependencyAvatar;
                }
            }

            string[] modelGuids = AssetDatabase.FindAssets(
                "t:Model",
                new[] { RootPath + "/Art/Characters/Player/Inori/Models" });
            List<string> modelPaths = new(modelGuids.Length);
            for (int index = 0; index < modelGuids.Length; index++)
            {
                modelPaths.Add(AssetDatabase.GUIDToAssetPath(modelGuids[index]));
            }

            return FindFirstValidAvatar(modelPaths);
        }

        private static Avatar FindFirstValidAvatar(IEnumerable<string> assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                for (int index = 0; index < assets.Length; index++)
                {
                    if (assets[index] is not Avatar avatar)
                    {
                        continue;
                    }

                    if (avatar.isValid && avatar.isHuman)
                    {
                        return avatar;
                    }
                }
            }

            return null;
        }

        private static bool EnsureCharacterAnimationBinding(
            Transform parent,
            GameObject visualPrefab,
            RuntimeAnimatorController animatorController,
            Avatar avatar,
            out Animator animator,
            Vector3? localScaleOverride = null)
        {
            animator = null;
            if (parent == null || visualPrefab == null)
            {
                return false;
            }

            bool changed = false;
            GameObject visualInstance = FindOrCreateCharacterVisual(parent, visualPrefab, ref changed);
            if (visualInstance == null)
            {
                return changed;
            }

            if (visualInstance.name != visualPrefab.name)
            {
                visualInstance.name = visualPrefab.name;
                changed = true;
            }

            Vector3 targetScale = localScaleOverride ?? visualInstance.transform.localScale;
            if (visualInstance.transform.localPosition != Vector3.zero)
            {
                visualInstance.transform.localPosition = Vector3.zero;
                changed = true;
            }

            if (visualInstance.transform.localRotation != Quaternion.identity)
            {
                visualInstance.transform.localRotation = Quaternion.identity;
                changed = true;
            }

            if (visualInstance.transform.localScale != targetScale)
            {
                visualInstance.transform.localScale = targetScale;
                changed = true;
            }

            changed |= AssignInoriPromotedMaterials(visualInstance);

            animator = visualInstance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = visualInstance.AddComponent<Animator>();
                changed = true;
            }

            if (animator.runtimeAnimatorController != animatorController)
            {
                animator.runtimeAnimatorController = animatorController;
                changed = true;
            }

            if (avatar != null && animator.avatar != avatar)
            {
                animator.avatar = avatar;
                changed = true;
            }

            if (animator.applyRootMotion)
            {
                animator.applyRootMotion = false;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(visualInstance);
                EditorUtility.SetDirty(animator);
            }

            return changed;
        }

        private static bool AssignInoriPromotedMaterials(GameObject visualRoot)
        {
            bool changed = false;
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] materials = renderer.sharedMaterials;
                bool rendererChanged = false;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    string hint = materials[materialIndex] != null ? materials[materialIndex].name : string.Empty;
                    Material promotedMaterial =
                        ActionFoundationInoriPlayerVisualAssetSetup.ResolvePromotedMaterial(hint, materialIndex);
                    if (materials[materialIndex] == promotedMaterial)
                    {
                        continue;
                    }

                    materials[materialIndex] = promotedMaterial;
                    rendererChanged = true;
                    changed = true;
                }

                if (rendererChanged)
                {
                    renderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(renderer);
                }
            }

            return changed;
        }

        private static GameObject FindOrCreateCharacterVisual(Transform parent, GameObject visualPrefab, ref bool changed)
        {
            Transform existingVisual = FindExistingCharacterVisual(parent, visualPrefab.name);
            if (existingVisual != null)
            {
                return existingVisual.gameObject;
            }

            GameObject visualInstance = PrefabUtility.InstantiatePrefab(visualPrefab, parent) as GameObject;
            if (visualInstance != null)
            {
                changed = true;
            }

            return visualInstance;
        }

        private static Transform FindExistingCharacterVisual(Transform parent, string preferredName)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == preferredName)
                {
                    return child;
                }
            }

            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
                {
                    return child;
                }
            }

            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.GetComponentInChildren<Renderer>(true) != null)
                {
                    return child;
                }
            }

            return null;
        }

        private static List<IsekaiBrawl.Gameplay.SummonData> LoadOrCreateSummonAssets()
        {
            List<IsekaiBrawl.Gameplay.SummonData> assets = new();
            assets.Add(LoadOrCreateSummonAsset("BasicWarrior", PrimitiveType.Capsule, new Color(0.85f, 0.35f, 0.35f), 30f, 100f, 15f, 1.5f, 0.9f, 2f, IsekaiBrawl.Gameplay.SummonType.Melee));
            assets.Add(LoadOrCreateSummonAsset("IceGolem", PrimitiveType.Cube, new Color(0.45f, 0.82f, 1f), 60f, 300f, 25f, 1.2f, 1.25f, 1f, IsekaiBrawl.Gameplay.SummonType.Tank));
            assets.Add(LoadOrCreateSummonAsset("ShadowArcher", PrimitiveType.Capsule, new Color(0.32f, 0.28f, 0.42f), 45f, 80f, 30f, 5f, 1.15f, 2.5f, IsekaiBrawl.Gameplay.SummonType.Ranged));
            return assets;
        }

        private static IsekaiBrawl.Gameplay.SummonData LoadOrCreateSummonAsset(
            string summonName,
            PrimitiveType primitiveType,
            Color color,
            float energyCost,
            float maxHp,
            float attackDamage,
            float attackRange,
            float attackCooldown,
            float moveSpeed,
            IsekaiBrawl.Gameplay.SummonType summonType)
        {
            string prefabPath = $"{RootPath}/Prefabs/Summons/{summonName}.prefab";
            GameObject summonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (summonPrefab == null)
            {
                GameObject summonObject = GameObject.CreatePrimitive(primitiveType);
                summonObject.name = summonName;
                summonObject.transform.localScale = primitiveType == PrimitiveType.Cube ? new Vector3(1.1f, 1.4f, 1.1f) : new Vector3(0.8f, 1.2f, 0.8f);
                summonObject.GetComponent<Renderer>().sharedMaterial = CreateMaterial(summonName, color);
                summonObject.AddComponent<IsekaiBrawl.Gameplay.SummonUnit>();
                PrefabUtility.SaveAsPrefabAsset(summonObject, prefabPath);
                Object.DestroyImmediate(summonObject);
                summonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            string assetPath = $"{RootPath}/ScriptableObjects/SummonData/{summonName}.asset";
            IsekaiBrawl.Gameplay.SummonData summonData = AssetDatabase.LoadAssetAtPath<IsekaiBrawl.Gameplay.SummonData>(assetPath);
            if (summonData == null)
            {
                summonData = ScriptableObject.CreateInstance<IsekaiBrawl.Gameplay.SummonData>();
                AssetDatabase.CreateAsset(summonData, assetPath);
            }

            summonData.summonName = summonName;
            summonData.prefab = summonPrefab;
            summonData.energyCost = energyCost;
            summonData.maxHP = maxHp;
            summonData.attackDamage = attackDamage;
            summonData.attackRange = attackRange;
            summonData.attackCooldown = attackCooldown;
            summonData.moveSpeed = moveSpeed;
            summonData.summonType = summonType;
            EditorUtility.SetDirty(summonData);
            return summonData;
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            string materialPath = $"{RootPath}/Materials/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreatePrimitiveObject(string name, PrimitiveType primitiveType, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.position = position;
            primitive.transform.rotation = Quaternion.Euler(rotation);
            primitive.transform.localScale = scale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            return primitive;
        }

        private static Transform CreateMarker(string name, Vector3 position)
        {
            GameObject marker = new(name);
            marker.transform.position = position;
            return marker.transform;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void EnsureTagsAndLayers()
        {
            SerializedObject tagManager = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProperty = tagManager.FindProperty("tags");
            EnsureTag(tagsProperty, "PlayerBase");
            EnsureTag(tagsProperty, "EnemyBase");
            EnsureTag(tagsProperty, "Player");
            EnsureTag(tagsProperty, "Enemy");

            SerializedProperty layersProperty = tagManager.FindProperty("layers");
            SetLayerName(layersProperty, 8, "PlayerSummon");
            SetLayerName(layersProperty, 9, "EnemySummon");
            SetLayerName(layersProperty, 10, "Projectile");
            SetLayerName(layersProperty, 11, "PlayerBase");
            SetLayerName(layersProperty, 12, "EnemyBase");

            tagManager.ApplyModifiedProperties();
        }

        private static void EnsureTag(SerializedProperty tagsProperty, string tagName)
        {
            for (int index = 0; index < tagsProperty.arraySize; index++)
            {
                if (tagsProperty.GetArrayElementAtIndex(index).stringValue == tagName)
                {
                    return;
                }
            }

            int newIndex = tagsProperty.arraySize;
            tagsProperty.InsertArrayElementAtIndex(newIndex);
            tagsProperty.GetArrayElementAtIndex(newIndex).stringValue = tagName;
        }

        private static void SetLayerName(SerializedProperty layersProperty, int layerIndex, string layerName)
        {
            if (layerIndex < 0 || layerIndex >= layersProperty.arraySize)
            {
                return;
            }

            layersProperty.GetArrayElementAtIndex(layerIndex).stringValue = layerName;
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string currentPath = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string nextPath = currentPath + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[index]);
                }

                currentPath = nextPath;
            }
        }

        private static void SetTagIfExists(GameObject gameObject, string tagName)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags == null)
            {
                return;
            }

            foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (existingTag == tagName)
                {
                    gameObject.tag = tagName;
                    return;
                }
            }
        }

        private static void SetLayerByName(GameObject gameObject, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                gameObject.layer = layer;
            }
        }

        private static void SetObjectReference(Object target, string propertyName, Object referenceValue)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = referenceValue;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool TrySetObjectReference(Object target, string propertyName, Object referenceValue)
        {
            if (target == null)
            {
                return false;
            }

            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == referenceValue)
            {
                return false;
            }

            property.objectReferenceValue = referenceValue;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            return true;
        }

        private static void SetObjectList(Object target, string propertyName, IList<IsekaiBrawl.Gameplay.SummonData> values)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class PrototypeAssets
        {
            public TMP_FontAsset FontAsset;
            public Sprite UiSprite;
            public Sprite UiKnobSprite;
            public GameObject PlayerVisualPrefab;
            public Avatar CharacterAvatar;
            public AnimatorController PlayerAnimatorController;
            public AnimatorController EnemyAnimatorController;
            public GameObject PlayerPrefab;
            public EnemyProjectile ProjectilePrefab;
            public CardSlotUI CardSlotPrefab;
            public List<IsekaiBrawl.Gameplay.SummonData> SummonDataAssets;
        }
    }
}
