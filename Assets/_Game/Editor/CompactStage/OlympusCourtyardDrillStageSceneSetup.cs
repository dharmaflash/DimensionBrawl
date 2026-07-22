using System;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Authors the B1-1 compact stage from an empty scene.  The large Olympus scenes are
    /// intentionally not used as a seed; only promoted runtime, UI, character, enemy and
    /// modular-environment assets are composed into this isolated candidate.
    /// </summary>
    public static class OlympusCourtyardDrillStageSceneSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        public const string StageDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCourtyardDrillCombat.asset";
        public const string PlayableStagePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusCourtyardDrill.asset";

        private const string HudPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";
        private const string PlayerVisualPrefabPath =
            "Assets/_Imported/AssetStore/CombatGirlsCharacterPack/CombatGirl_Shield/Prefab/CombatGirls_Sword_Shield.prefab";
        private const string PlayerAnimatorControllerPath =
            "Assets/_Game/Art/Animations/Player/CombatGirlSwordShield/DB_CombatGirl_ActionFoundation.controller";
        private const string BossVisualPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Boss_Akaza_Phase2Review.prefab";
        private const string FloorPrefabPath =
            "Assets/_Game/Art/Environment/OlympusTemple/HDRP/Art/Prefabs/SM_FloorBase_part_3_square_bevel.prefab";
        private const string ColumnPrefabPath =
            "Assets/_Game/Art/Environment/OlympusTemple/HDRP/Art/Prefabs/SM_Column_SM_Column_body.prefab";

        public const string StageRootName = "OlympusCourtyardDrillStageRoot";
        public const string MapRootName = "OlympusCourtyardDrillMap";
        public const string RuntimeRootName = "CourtyardDrill_OneRowRuntime";
        public const string PlayerRootName = "Player_CourtyardDrill";
        public const string BossRootName = "Boss_CourtyardDrillSentinel";
        public const string HudRootName = "PF_UI_CombatHud";

        public const string PlayerAnchorId = "Player_Start";
        public const string BossAnchorId = "Boss_Terminal";
        public const string AddAnchorId = "Add_RifleCrossfire";

        public static readonly Vector3 PlayerPosition = new(0f, 0f, -4.5f);
        public static readonly Vector3 BossPosition = new(0f, 0f, 3.6f);
        public static readonly Vector3 AddPosition = new(-5f, 0f, 1.5f);

        [MenuItem("DimensionBrawl/Setup/B1-1 Build Olympus Courtyard Drill Scene")]
        public static void BuildFromMenu()
        {
            Build();
            Debug.Log("[OlympusCourtyardDrillStageSceneSetup] SCENE_SETUP_PASS");
        }

        public static void RunBatchSetup()
        {
            try
            {
                Build();
                Debug.Log("[OlympusCourtyardDrillStageSceneSetup] BATCH_SETUP_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[OlympusCourtyardDrillStageSceneSetup] BATCH_SETUP_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "The compact stage cannot be authored while entering or running Play Mode.");
            RefuseDirtyOpenScenes();

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);

                // A single-scene replacement can release otherwise unreferenced loaded assets.
                // Resolve the persistent authorities only after the new scene is established.
                StageDefinitionProfile stageDefinition = LoadRequired<StageDefinitionProfile>(
                    StageDefinitionPath);
                PlayableStageDefinition playableStage = LoadRequired<PlayableStageDefinition>(
                    PlayableStagePath);
                Require(string.Equals(stageDefinition.MapScenePath, ScenePath, StringComparison.Ordinal),
                    "The B1-1 StageDefinition does not own the compact scene path.");
                Require(playableStage.SceneSegmentCount == 1
                    && ReferenceEquals(playableStage.GetSceneSegment(0).StageDefinition, stageDefinition),
                    "The B1-1 route does not reference the exact compact StageDefinition.");

                Camera camera = CreateCamera(scene);
                CreateLighting(scene);
                CreateEventSystem(scene);

                GameObject stageRoot = CreateSceneRoot(scene, StageRootName, active: false);
                GameObject mapRoot = CreateMap(stageRoot.transform, scene);
                StageAnchorPoint playerAnchor = CreateAnchor(
                    stageRoot.transform,
                    PlayerAnchorId,
                    "CombatSpawnAnchors",
                    PlayerPosition,
                    Vector3.zero,
                    1101,
                    StageSpawnKind.Player);
                StageAnchorPoint bossAnchor = CreateAnchor(
                    stageRoot.transform,
                    BossAnchorId,
                    "CombatSpawnAnchors",
                    BossPosition,
                    new Vector3(0f, 180f, 0f),
                    1201,
                    StageSpawnKind.Boss);
                StageAnchorPoint addAnchor = CreateAnchor(
                    stageRoot.transform,
                    AddAnchorId,
                    "CombatSpawnAnchors",
                    AddPosition,
                    new Vector3(0f, 135f, 0f),
                    1301,
                    StageSpawnKind.Add);

                StageDefinitionSceneBinding sceneBinding =
                    stageRoot.AddComponent<StageDefinitionSceneBinding>();
                sceneBinding.Configure(
                    stageDefinition,
                    mapRoot.transform,
                    new[] { playerAnchor, bossAnchor, addAnchor });

                PlayerPackage player = CreatePlayer(scene, playerAnchor.transform, camera);
                BossPackage boss = CreateTerminalBoss(scene, bossAnchor.transform);
                ConfigurePlayerTargets(player, boss, camera);

                HudPackage hud = CreateHud(scene);
                RuntimePackage runtime = CreateRuntime(
                    scene,
                    playableStage,
                    sceneBinding,
                    player,
                    boss,
                    hud);
                ConfigureHud(hud, runtime.Encounter, player, boss);

                stageRoot.SetActive(true);
                player.Root.SetActive(true);
                boss.Root.SetActive(true);
                hud.Root.SetActive(true);
                runtime.Root.SetActive(true);

                EditorSceneManager.MarkSceneDirty(scene);
                Require(EditorSceneManager.SaveScene(scene, ScenePath),
                    $"Failed to save compact stage scene at '{ScenePath}'.");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                if (previousSetup != null && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }
        }

        private static GameObject CreateMap(Transform stageRoot, Scene scene)
        {
            GameObject mapRoot = new(MapRootName);
            mapRoot.transform.SetParent(stageRoot, false);

            GameObject collision = new("ArenaCollision");
            collision.transform.SetParent(mapRoot.transform, false);
            BoxCollider floorCollider = collision.AddComponent<BoxCollider>();
            floorCollider.center = new Vector3(0f, -0.3f, 0f);
            floorCollider.size = new Vector3(18f, 0.6f, 14f);

            CreateBoundary(mapRoot.transform, "Boundary_North", new Vector3(0f, 1.5f, 7f), new Vector3(18f, 3f, 0.35f));
            CreateBoundary(mapRoot.transform, "Boundary_South", new Vector3(0f, 1.5f, -7f), new Vector3(18f, 3f, 0.35f));
            CreateBoundary(mapRoot.transform, "Boundary_East", new Vector3(9f, 1.5f, 0f), new Vector3(0.35f, 3f, 14f));
            CreateBoundary(mapRoot.transform, "Boundary_West", new Vector3(-9f, 1.5f, 0f), new Vector3(0.35f, 3f, 14f));

            Transform visuals = new GameObject("PromotedOlympusModules").transform;
            visuals.SetParent(mapRoot.transform, false);
            CreateFittedModule(FloorPrefabPath, visuals, "Floor_NW", new Vector3(-4.35f, 0f, 3.35f), Vector3.zero, new Vector3(8.45f, 0.65f, 6.45f));
            CreateFittedModule(FloorPrefabPath, visuals, "Floor_NE", new Vector3(4.35f, 0f, 3.35f), Vector3.zero, new Vector3(8.45f, 0.65f, 6.45f));
            CreateFittedModule(FloorPrefabPath, visuals, "Floor_SW", new Vector3(-4.35f, 0f, -3.35f), Vector3.zero, new Vector3(8.45f, 0.65f, 6.45f));
            CreateFittedModule(FloorPrefabPath, visuals, "Floor_SE", new Vector3(4.35f, 0f, -3.35f), Vector3.zero, new Vector3(8.45f, 0.65f, 6.45f));

            Vector3[] columnPositions =
            {
                new(-7.5f, 0f, -5.5f),
                new(7.5f, 0f, -5.5f),
                new(-7.5f, 0f, 5.5f),
                new(7.5f, 0f, 5.5f)
            };
            for (int i = 0; i < columnPositions.Length; i++)
            {
                CreateFittedModule(
                    ColumnPrefabPath,
                    visuals,
                    $"CourtyardColumn_{i + 1:00}",
                    columnPositions[i],
                    Vector3.zero,
                    new Vector3(1.2f, 4.8f, 1.2f));
            }

            return mapRoot;
        }

        private static void CreateBoundary(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 size)
        {
            GameObject boundary = new(name);
            boundary.transform.SetParent(parent, false);
            boundary.transform.localPosition = localPosition;
            BoxCollider collider = boundary.AddComponent<BoxCollider>();
            collider.size = size;
        }

        private static StageAnchorPoint CreateAnchor(
            Transform parent,
            string anchorId,
            string groupId,
            Vector3 localPosition,
            Vector3 localEuler,
            int positionId,
            StageSpawnKind spawnKind)
        {
            GameObject anchorObject = new(anchorId);
            anchorObject.transform.SetParent(parent, false);
            anchorObject.transform.localPosition = localPosition;
            anchorObject.transform.localEulerAngles = localEuler;
            StageAnchorPoint anchor = anchorObject.AddComponent<StageAnchorPoint>();
            var serialized = new SerializedObject(anchor);
            serialized.FindProperty("anchorId").stringValue = anchorId;
            serialized.FindProperty("groupId").stringValue = groupId;
            serialized.FindProperty("usageKind").enumValueIndex =
                (int)StageAnchorUsageKind.CombatSpawn;
            serialized.FindProperty("positionId").intValue = positionId;
            serialized.FindProperty("spawnKind").enumValueIndex = (int)spawnKind;
            serialized.FindProperty("purpose").stringValue = spawnKind switch
            {
                StageSpawnKind.Player => "Exact compact-stage player admission position.",
                StageSpawnKind.Boss => "Always-live terminal drill sentinel subject.",
                StageSpawnKind.Add => "Independent reviewed Rifle Crossfire pressure spawn.",
                _ => "Compact-stage authored anchor."
            };
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return anchor;
        }

        private static PlayerPackage CreatePlayer(Scene scene, Transform anchor, Camera camera)
        {
            GameObject root = CreateSceneRoot(scene, PlayerRootName, active: false);
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            CharacterController character = root.AddComponent<CharacterController>();
            character.center = new Vector3(0f, 0.92f, 0f);
            character.height = 1.84f;
            character.radius = 0.36f;
            character.stepOffset = 0.28f;

            CombatHealth health = root.AddComponent<CombatHealth>();
            health.ConfigureTeam(DamageTeam.Player);
            health.ConfigureMaxHealth(180f);

            PlayerMovementController movement = root.AddComponent<PlayerMovementController>();
            PlayerCombatTargetSelector selector = root.AddComponent<PlayerCombatTargetSelector>();
            PlayerActionController action = root.AddComponent<PlayerActionController>();

            Animator animator = InstantiateCharacterVisual(
                PlayerVisualPrefabPath,
                root.transform,
                "CombatGirl_SwordShield_Visual",
                1.76f);
            Require(animator != null, "The compact player visual has no Animator.");
            animator.runtimeAnimatorController =
                LoadRequired<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
            animator.applyRootMotion = false;
            movement.SetAnimator(animator);
            action.SetAnimator(animator);

            var serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("referenceCamera").objectReferenceValue = camera;
            serializedMovement.FindProperty("cameraRelativeMovement").boolValue = true;
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            var serializedAction = new SerializedObject(action);
            serializedAction.FindProperty("movement").objectReferenceValue = movement;
            serializedAction.FindProperty("health").objectReferenceValue = health;
            serializedAction.FindProperty("targetSelector").objectReferenceValue = selector;
            serializedAction.FindProperty("animator").objectReferenceValue = animator;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            return new PlayerPackage(root, health, movement, selector, action);
        }

        private static BossPackage CreateTerminalBoss(Scene scene, Transform anchor)
        {
            GameObject root = CreateSceneRoot(scene, BossRootName, active: false);
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1.25f, 0f);
            collider.height = 2.5f;
            collider.radius = 0.62f;

            CombatHealth health = root.AddComponent<CombatHealth>();
            health.ConfigureTeam(DamageTeam.Enemy);
            health.ConfigureMaxHealth(260f);

            InstantiateCharacterVisual(
                BossVisualPrefabPath,
                root.transform,
                "CourtyardSentinel_Visual",
                2.55f);
            return new BossPackage(root, health);
        }

        private static void ConfigurePlayerTargets(
            PlayerPackage player,
            BossPackage boss,
            Camera camera)
        {
            var serializedSelector = new SerializedObject(player.TargetSelector);
            serializedSelector.FindProperty("selfHealth").objectReferenceValue = player.Health;
            serializedSelector.FindProperty("selectionOrigin").objectReferenceValue =
                player.Root.transform;
            serializedSelector.FindProperty("viewReference").objectReferenceValue =
                camera.transform;
            SerializedProperty candidates = serializedSelector.FindProperty("targetCandidates");
            candidates.arraySize = 1;
            candidates.GetArrayElementAtIndex(0).objectReferenceValue = boss.Health;
            serializedSelector.FindProperty("includeActiveHostileSummons").boolValue = true;
            serializedSelector.ApplyModifiedPropertiesWithoutUndo();
        }

        private static HudPackage CreateHud(Scene scene)
        {
            GameObject prefab = LoadRequired<GameObject>(HudPrefabPath);
            GameObject root = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            Require(root != null, "Failed to instantiate the shared combat HUD prefab.");
            root.name = HudRootName;
            root.SetActive(false);

            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = root.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = root.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            if (root.GetComponent<GraphicRaycaster>() == null)
            {
                root.AddComponent<GraphicRaycaster>();
            }
            StyleHudText(root, "Objective", new Color(0.90f, 0.97f, 1f, 1f));
            StyleHudText(root, "Timer", new Color(1f, 0.91f, 0.62f, 1f));

            BossBarrageLaneReviewCombatHudBinder[] routeBinders =
                root.GetComponentsInChildren<BossBarrageLaneReviewCombatHudBinder>(true);
            for (int i = 0; i < routeBinders.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(routeBinders[i]);
            }

            CombatHudPresenter presenter = RequireSingle<CombatHudPresenter>(root);
            CombatHudInputBridge input = RequireSingle<CombatHudInputBridge>(root);
            CombatHudVirtualJoystick joystick = EnsureVirtualJoystick(root);
            CombatSessionOverlayPresenter session =
                RequireSingle<CombatSessionOverlayPresenter>(root);
            OneRowCombatHudBinder binder = root.GetComponent<OneRowCombatHudBinder>()
                ?? root.AddComponent<OneRowCombatHudBinder>();
            return new HudPackage(root, presenter, input, joystick, session, binder);
        }

        private static void StyleHudText(GameObject hudRoot, string objectName, Color color)
        {
            Transform target = FindDescendant(hudRoot.transform, objectName);
            Require(target != null, $"The shared combat HUD is missing '{objectName}'.");
            Text text = target.GetComponent<Text>();
            Require(text != null, $"The shared combat HUD '{objectName}' has no Text component.");
            text.color = color;
            text.fontStyle = FontStyle.Bold;
            Outline outline = target.GetComponent<Outline>();
            if (outline == null)
            {
                outline = target.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = new Color(0f, 0.02f, 0.04f, 0.96f);
            outline.effectDistance = new Vector2(1.25f, -1.25f);
            outline.useGraphicAlpha = true;
        }

        private static CombatHudVirtualJoystick EnsureVirtualJoystick(GameObject hudRoot)
        {
            CombatHudVirtualJoystick[] existing =
                hudRoot.GetComponentsInChildren<CombatHudVirtualJoystick>(true);
            Require(existing.Length <= 1,
                $"'{hudRoot.name}' has duplicate {nameof(CombatHudVirtualJoystick)} components.");

            Transform ringTransform = FindDescendant(hudRoot.transform, "MoveJoystickRing");
            Transform knobTransform = FindDescendant(hudRoot.transform, "MoveJoystickKnob");
            Require(ringTransform is RectTransform,
                "The shared combat HUD is missing the MoveJoystickRing RectTransform.");
            Require(knobTransform is RectTransform,
                "The shared combat HUD is missing the MoveJoystickKnob RectTransform.");

            CombatHudVirtualJoystick joystick = existing.Length == 1
                ? existing[0]
                : ringTransform.gameObject.AddComponent<CombatHudVirtualJoystick>();
            joystick.Configure(null, (RectTransform)knobTransform);
            return joystick;
        }

        private static RuntimePackage CreateRuntime(
            Scene scene,
            PlayableStageDefinition playableStage,
            StageDefinitionSceneBinding sceneBinding,
            PlayerPackage player,
            BossPackage boss,
            HudPackage hud)
        {
            GameObject root = CreateSceneRoot(scene, RuntimeRootName, active: false);
            CombatEncounterController encounter = root.AddComponent<CombatEncounterController>();
            encounter.ConfigureCombatants(player.Health, boss.Health);
            encounter.ConfigureTerminalResolutionPolicy(true);

            OlympusStageClearOverlay resultOverlay =
                root.AddComponent<OlympusStageClearOverlay>();
            OneRowStageRunBootstrap bootstrap =
                root.AddComponent<OneRowStageRunBootstrap>();
            OneRowStageRunFactAdapter factAdapter =
                root.AddComponent<OneRowStageRunFactAdapter>();
            OneRowStageRunResultPresenter resultPresenter =
                root.AddComponent<OneRowStageRunResultPresenter>();
            StageCountOneEncounterExecutor addExecutor =
                root.AddComponent<StageCountOneEncounterExecutor>();

            var serializedFact = new SerializedObject(factAdapter);
            serializedFact.FindProperty("encounter").objectReferenceValue = encounter;
            serializedFact.FindProperty("playerHealth").objectReferenceValue = player.Health;
            serializedFact.FindProperty("playerActionController").objectReferenceValue =
                player.Action;
            serializedFact.FindProperty("summonEnergyLadder").objectReferenceValue = null;
            serializedFact.FindProperty("summonSlot1Action").objectReferenceValue = null;
            serializedFact.FindProperty("supportSummonActions").arraySize = 0;
            serializedFact.FindProperty("resultSurfaceBehaviour").objectReferenceValue =
                hud.Session;
            serializedFact.ApplyModifiedPropertiesWithoutUndo();

            var serializedPresenter = new SerializedObject(resultPresenter);
            serializedPresenter.FindProperty("encounter").objectReferenceValue = encounter;
            serializedPresenter.FindProperty("resultOverlayBehaviour").objectReferenceValue =
                resultOverlay;
            serializedPresenter.FindProperty("resultSurfaceBehaviour").objectReferenceValue =
                hud.Session;
            serializedPresenter.FindProperty("factAdapter").objectReferenceValue = factAdapter;
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            var serializedBootstrap = new SerializedObject(bootstrap);
            serializedBootstrap.FindProperty("playableStageDefinition").objectReferenceValue =
                playableStage;
            serializedBootstrap.FindProperty("encounter").objectReferenceValue = encounter;
            serializedBootstrap.FindProperty("factAdapter").objectReferenceValue = factAdapter;
            serializedBootstrap.FindProperty("resultPresenter").objectReferenceValue =
                resultPresenter;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            var serializedExecutor = new SerializedObject(addExecutor);
            serializedExecutor.FindProperty("sceneBinding").objectReferenceValue = sceneBinding;
            serializedExecutor.FindProperty("activationKind").enumValueIndex =
                (int)StageEncounterActivationKind.SceneReady;
            serializedExecutor.FindProperty("requireActiveStageRun").boolValue = true;
            serializedExecutor.FindProperty("cancelOnTerminalEncounter").boolValue = true;
            serializedExecutor.ApplyModifiedPropertiesWithoutUndo();

            return new RuntimePackage(root, encounter, resultOverlay, bootstrap, factAdapter,
                resultPresenter, addExecutor);
        }

        private static void ConfigureHud(
            HudPackage hud,
            CombatEncounterController encounter,
            PlayerPackage player,
            BossPackage boss)
        {
            var serializedJoystick = new SerializedObject(hud.Joystick);
            serializedJoystick.FindProperty("movementController").objectReferenceValue =
                player.Movement;
            serializedJoystick.ApplyModifiedPropertiesWithoutUndo();

            CombatHudAimDragInput[] aimInputs =
                hud.Root.GetComponentsInChildren<CombatHudAimDragInput>(true);
            for (int i = 0; i < aimInputs.Length; i++)
            {
                aimInputs[i].Configure(player.Movement, null, null, null);
            }

            ConfigurePointerAction(
                hud.Root,
                hud.Input,
                "BasicAttackButton",
                CombatHudActionId.BasicAttack,
                sendHoldState: true);
            ConfigurePointerAction(
                hud.Root,
                hud.Input,
                "DodgeButton",
                CombatHudActionId.Dodge,
                sendHoldState: false);
            ConfigurePointerAction(
                hud.Root,
                hud.Input,
                "PauseButton",
                CombatHudActionId.Pause,
                sendHoldState: false);

            DisableUnavailableAction(hud.Root, "Skill1Button");
            DisableUnavailableAction(hud.Root, "UltimateButton");
            DisableUnavailableAction(hud.Root, "SummonSlot1Button");
            DisableUnavailableAction(hud.Root, "SummonSlot2Button");
            DisableUnavailableAction(hud.Root, "SummonSlot3Button");

            hud.Binder.Configure(
                hud.Presenter,
                hud.Input,
                hud.Joystick,
                hud.Session,
                encounter,
                player.Health,
                boss.Health,
                player.Movement,
                player.Action);
            var serializedBinder = new SerializedObject(hud.Binder);
            serializedBinder.FindProperty("objectiveText").stringValue =
                "Defeat the courtyard sentinel under rifle crossfire.";
            serializedBinder.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigurePointerAction(
            GameObject hudRoot,
            CombatHudInputBridge inputBridge,
            string buttonName,
            CombatHudActionId actionId,
            bool sendHoldState)
        {
            Transform buttonTransform = FindDescendant(hudRoot.transform, buttonName);
            Require(buttonTransform != null,
                $"Shared combat HUD is missing required action button '{buttonName}'.");
            Button button = buttonTransform.GetComponent<Button>();
            Require(button != null,
                $"Shared combat HUD action '{buttonName}' is missing its Button component.");

            CombatHudPointerActionInput[] authoredInputs =
                buttonTransform.GetComponents<CombatHudPointerActionInput>();
            CombatHudPointerActionInput pointerInput = authoredInputs.Length > 0
                ? authoredInputs[0]
                : buttonTransform.gameObject.AddComponent<CombatHudPointerActionInput>();
            for (int i = 1; i < authoredInputs.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(authoredInputs[i]);
            }

            button.interactable = true;
            pointerInput.Configure(inputBridge, actionId, sendHoldState);
        }

        private static void DisableUnavailableAction(GameObject hudRoot, string buttonName)
        {
            Transform buttonTransform = FindDescendant(hudRoot.transform, buttonName);
            if (buttonTransform == null)
            {
                return;
            }

            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }

            CombatHudPointerActionInput[] authoredInputs =
                buttonTransform.GetComponents<CombatHudPointerActionInput>();
            for (int i = 0; i < authoredInputs.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(authoredInputs[i]);
            }
        }

        private static Transform FindDescendant(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Camera CreateCamera(Scene scene)
        {
            GameObject cameraObject = CreateSceneRoot(scene, "Main Camera", active: true);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.15f;
            camera.farClipPlane = 250f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.045f, 0.07f, 1f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, 9.4f, -12.2f);
            cameraObject.transform.rotation = Quaternion.LookRotation(
                new Vector3(0f, 0.75f, 0.2f) - cameraObject.transform.position,
                Vector3.up);
            return camera;
        }

        private static void CreateLighting(Scene scene)
        {
            GameObject lightObject = CreateSceneRoot(scene, "CourtyardKeyLight", active: true);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.92f, 0.78f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.37f, 0.52f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.2f, 0.3f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.07f, 0.1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.34f, 0.42f, 0.55f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 25f;
            RenderSettings.fogEndDistance = 90f;
        }

        private static void CreateEventSystem(Scene scene)
        {
            GameObject eventObject = CreateSceneRoot(scene, "EventSystem", active: true);
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<InputSystemUIInputModule>();
        }

        private static Animator InstantiateCharacterVisual(
            string prefabPath,
            Transform parent,
            string name,
            float targetHeight)
        {
            GameObject prefab = LoadRequired<GameObject>(prefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Require(instance != null, $"Failed to instantiate character visual '{prefabPath}'.");
            instance.name = name;
            instance.transform.SetParent(parent, false);
            StripVisualRuntimeOwnership(instance);
            FitVisualHeight(instance, parent, targetHeight);
            return instance.GetComponentInChildren<Animator>(true);
        }

        private static void StripVisualRuntimeOwnership(GameObject visualRoot)
        {
            MonoBehaviour[] behaviours = visualRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(behaviours[i]);
            }

            Collider[] colliders = visualRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }

            CombatHealth[] health = visualRoot.GetComponentsInChildren<CombatHealth>(true);
            for (int i = 0; i < health.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(health[i]);
            }
        }

        private static void FitVisualHeight(
            GameObject instance,
            Transform parent,
            float targetHeight)
        {
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            Require(TryGetRendererBounds(instance, out Bounds bounds),
                $"Visual '{instance.name}' has no Renderer bounds.");
            float scale = targetHeight / Mathf.Max(0.001f, bounds.size.y);
            instance.transform.localScale = Vector3.one * scale;
            Require(TryGetRendererBounds(instance, out bounds),
                $"Visual '{instance.name}' lost its Renderer bounds after scaling.");
            Vector3 centerLocal = parent.InverseTransformPoint(bounds.center);
            Vector3 bottomLocal = parent.InverseTransformPoint(
                new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
            instance.transform.localPosition += new Vector3(
                -centerLocal.x,
                -bottomLocal.y,
                -centerLocal.z);
        }

        private static void CreateFittedModule(
            string prefabPath,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 targetSize)
        {
            GameObject prefab = LoadRequired<GameObject>(prefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Require(instance != null, $"Failed to instantiate Olympus module '{prefabPath}'.");
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(localEuler);
            instance.transform.localScale = Vector3.one;
            Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            Require(TryGetRendererBounds(instance, out Bounds bounds),
                $"Olympus module '{prefabPath}' has no Renderer bounds.");
            Vector3 sourceSize = bounds.size;
            Vector3 scale = new(
                targetSize.x / Mathf.Max(0.001f, sourceSize.x),
                targetSize.y / Mathf.Max(0.001f, sourceSize.y),
                targetSize.z / Mathf.Max(0.001f, sourceSize.z));
            instance.transform.localScale = scale;
            Require(TryGetRendererBounds(instance, out bounds),
                $"Olympus module '{prefabPath}' lost Renderer bounds after fitting.");
            Vector3 desiredWorld = parent.TransformPoint(localPosition);
            Vector3 bottomCenter = new(bounds.center.x, bounds.min.y, bounds.center.z);
            instance.transform.position += desiredWorld - bottomCenter;
        }

        private static bool TryGetRendererBounds(GameObject root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool found = false;
            bounds = default;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return found;
        }

        private static GameObject CreateSceneRoot(Scene scene, string name, bool active)
        {
            GameObject root = new(name);
            root.SetActive(active);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static T RequireSingle<T>(GameObject root)
            where T : Component
        {
            T[] components = root.GetComponentsInChildren<T>(true);
            Require(components.Length == 1,
                $"'{root.name}' requires exactly one {typeof(T).Name}, found {components.Length}.");
            return components[0];
        }

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null, $"Required asset is missing: {path}");
            return asset;
        }

        private static void RefuseDirtyOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                Require(!scene.isDirty,
                    $"Refusing to replace a dirty open scene: '{scene.path}'. Save or discard it first.");
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private readonly struct PlayerPackage
        {
            public PlayerPackage(
                GameObject root,
                CombatHealth health,
                PlayerMovementController movement,
                PlayerCombatTargetSelector targetSelector,
                PlayerActionController action)
            {
                Root = root;
                Health = health;
                Movement = movement;
                TargetSelector = targetSelector;
                Action = action;
            }

            public GameObject Root { get; }
            public CombatHealth Health { get; }
            public PlayerMovementController Movement { get; }
            public PlayerCombatTargetSelector TargetSelector { get; }
            public PlayerActionController Action { get; }
        }

        private readonly struct BossPackage
        {
            public BossPackage(GameObject root, CombatHealth health)
            {
                Root = root;
                Health = health;
            }

            public GameObject Root { get; }
            public CombatHealth Health { get; }
        }

        private readonly struct HudPackage
        {
            public HudPackage(
                GameObject root,
                CombatHudPresenter presenter,
                CombatHudInputBridge input,
                CombatHudVirtualJoystick joystick,
                CombatSessionOverlayPresenter session,
                OneRowCombatHudBinder binder)
            {
                Root = root;
                Presenter = presenter;
                Input = input;
                Joystick = joystick;
                Session = session;
                Binder = binder;
            }

            public GameObject Root { get; }
            public CombatHudPresenter Presenter { get; }
            public CombatHudInputBridge Input { get; }
            public CombatHudVirtualJoystick Joystick { get; }
            public CombatSessionOverlayPresenter Session { get; }
            public OneRowCombatHudBinder Binder { get; }
        }

        private readonly struct RuntimePackage
        {
            public RuntimePackage(
                GameObject root,
                CombatEncounterController encounter,
                OlympusStageClearOverlay resultOverlay,
                OneRowStageRunBootstrap bootstrap,
                OneRowStageRunFactAdapter factAdapter,
                OneRowStageRunResultPresenter resultPresenter,
                StageCountOneEncounterExecutor addExecutor)
            {
                Root = root;
                Encounter = encounter;
                ResultOverlay = resultOverlay;
                Bootstrap = bootstrap;
                FactAdapter = factAdapter;
                ResultPresenter = resultPresenter;
                AddExecutor = addExecutor;
            }

            public GameObject Root { get; }
            public CombatEncounterController Encounter { get; }
            public OlympusStageClearOverlay ResultOverlay { get; }
            public OneRowStageRunBootstrap Bootstrap { get; }
            public OneRowStageRunFactAdapter FactAdapter { get; }
            public OneRowStageRunResultPresenter ResultPresenter { get; }
            public StageCountOneEncounterExecutor AddExecutor { get; }
        }
    }
}
