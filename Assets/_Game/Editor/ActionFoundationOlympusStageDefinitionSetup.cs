using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationOlympusStageDefinitionSetup
    {
        private const string DefinitionRoot = "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions";
        private const string DefinitionPath = DefinitionRoot + "/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string StageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string SourceScenePath = "Assets/_Game/Scenes/Lookdev/OlympusCorridorInvasionLookdev.unity";
        private const string IntroGatePodReviewScenePath = "Assets/_Game/Scenes/IntroGatePodCutsceneReview.unity";
        private const string StageRootName = "OlympusCorridorStageRoot";
        private const string StageMapRootName = "OlympusCorridorStageMap";
        private const string StageAnchorsName = "OlympusCorridorStageAnchors";
        private const string CombatAnchorsName = "CombatSpawnAnchors";
        private const string CutsceneAnchorsName = "CutsceneHandoffAnchors";
        private const string RuntimeAnchorsName = "RuntimeStateAnchors";
        private const string CutscenePortsName = "OlympusCorridorCutscenePorts";
        private const string PortPayloadRootName = "Payload";
        private const string IntroGatePodPortRootName = "IntroGatePodAwakeningPort";
        private const string IntroGatePodGeneratedPayloadRootName = "IntroGatePodPortPayload_Visuals";
        private const string IntroGatePodRuntimePayloadRootName = "IntroGatePodPortPayload_CutsceneRuntime";
        private const string CinematicProfileRoot = "Assets/_Game/DesignData/Profiles/Cinematics";
        private const string IntroGatePodProfilePath = CinematicProfileRoot + "/DB_Cinematic_IntroGatePodAwakening.asset";
        private const string BossIntroProfilePath = CinematicProfileRoot + "/DB_Cinematic_BossIntro.asset";
        private const string GameplayHandoffProfilePath = CinematicProfileRoot + "/DB_Cinematic_GameplayHandoff.asset";
        private const float StageScale = 1.5f;
        private const float OlympusCorridorGameplayYawDegrees = 90f;

        private static readonly string[] IntroGatePodPayloadSourceNames =
        {
            "IntroGatePodReview_StageRoot",
            "IntroGatePodReview_GatePods",
            "IntroGatePodReview_InvasionBridge",
            "IntroGatePodReview_CommandoRunGroup",
            "IntroGatePodReview_HeavenBackgroundExplosion",
            "IntroGatePodReview_InoriPlacement",
            "IntroGatePodReview_FloorRifle",
            "IntroGatePodReview_FloorSword"
        };

        private static readonly string[] IntroGatePodRuntimeSourceNames =
        {
            "Main Camera",
            "IntroGatePodReview_CinemachineShots",
            "IntroGatePodReview_CinemachineShotPlayer",
            "IntroGatePodReview_CueDirector",
            "IntroGatePodReview_TimelineDirector",
            "IntroGatePodReview_TimelineAudio",
            "IntroGatePodReview_TimelineFadeOverlay",
            "IntroGatePodReview_FirstPersonRendererMask",
            "IntroGatePodReview_Runner",
            "IntroGatePodReview_InvasionScreenEffects",
            "IntroGatePodReview_FirstPersonViewMarker"
        };

        private static readonly AnchorSpec[] AnchorSpecs =
        {
            new("Player_LeftShoulderCameraAnchor", CombatAnchorsName, new Vector3(-51.402f, 9.785f, -0.411f), new Vector3(0f, OlympusCorridorGameplayYawDegrees, 0f), "Player camera/start read for intro handoff and combat entry."),
            new("Boss_CenterLaneAnchor", CombatAnchorsName, new Vector3(15.3f, 0f, 0f), Vector3.zero, "Boss center spawn and reveal focus."),
            new("Add_LeftLaneAnchor", CombatAnchorsName, new Vector3(13.35f, 0f, -1.875f), Vector3.zero, "Left add spawn lane."),
            new("Add_RightLaneAnchor", CombatAnchorsName, new Vector3(13.35f, 0f, 1.875f), Vector3.zero, "Right add spawn lane."),
            new("Rift_BackdropAnchor", CombatAnchorsName, new Vector3(22.2f, 3.975f, 0f), Vector3.zero, "Far rift/backdrop spatial reference."),
            new("IntroCutscene_End_PlayerHandoffAnchor", CutsceneAnchorsName, new Vector3(-51.402f, 9.785f, -0.411f), new Vector3(0f, OlympusCorridorGameplayYawDegrees, 0f), "Intro cutscene exits into this player-side view."),
            new("BossEntrance_BossRevealAnchor", CutsceneAnchorsName, new Vector3(15.3f, 1.6f, 0f), Vector3.zero, "Boss entrance reveal look/actor anchor."),
            new("Gameplay_CombatStartAnchor", CutsceneAnchorsName, new Vector3(-51.402f, 7.985f, -0.411f), new Vector3(0f, OlympusCorridorGameplayYawDegrees, 0f), "Gameplay camera/input unlock handoff."),
            new("StageSpawner_PlayerStart", RuntimeAnchorsName, new Vector3(-51.402f, 7.985f, -0.411f), new Vector3(0f, OlympusCorridorGameplayYawDegrees, 0f), "Runtime PositionId for player start."),
            new("StageSpawner_BossCenter", RuntimeAnchorsName, new Vector3(15.3f, 0f, 0f), Vector3.zero, "Runtime PositionId for boss center."),
            new("StageClear_CorridorExit", RuntimeAnchorsName, new Vector3(27f, 0f, 0f), Vector3.zero, "Runtime clear/exit state hook.")
        };

        private static readonly SpawnSpec[] SpawnSpecs =
        {
            new("player-start", StageSpawnKind.Player, 1001, "StageSpawner_PlayerStart", "PlayerParty", 1, 0f, "Player spawn is a stage position, not a balance value."),
            new("boss-center", StageSpawnKind.Boss, 2001, "StageSpawner_BossCenter", "BossEntranceCandidate", 1, 0f, "Boss actor spawns through boss entrance cutscene."),
            new("add-left", StageSpawnKind.Add, 2101, "Add_LeftLaneAnchor", "OlympusAdd.Left", 1, 0f, "Add payload is placeholder identity only."),
            new("add-right", StageSpawnKind.Add, 2102, "Add_RightLaneAnchor", "OlympusAdd.Right", 1, 0f, "Add payload is placeholder identity only."),
            new("rift-backdrop", StageSpawnKind.Rift, 3001, "Rift_BackdropAnchor", "OlympusRiftBackdrop", 1, 0f, "Far rift visual context anchor.")
        };

        private static readonly CutsceneHandoffSpec[] HandoffSpecs =
        {
            new("intro-to-stage", "IntroCutscene_End_PlayerHandoffAnchor", "DB_Cinematic_IntroGatePodAwakening", "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening.playable", "boss-entrance-ready", "Intro cutscene should end in the same shared corridor stage."),
            new("boss-entrance", "BossEntrance_BossRevealAnchor", "DB_Cinematic_BossIntro", string.Empty, "combat-start-ready", "Boss entrance reveal uses the shared stage anchor."),
            new("combat-start", "Gameplay_CombatStartAnchor", "DB_Cinematic_GameplayHandoff", string.Empty, "unlock-input", "Gameplay handoff happens without changing map scene.")
        };

        private static readonly RuntimeStateSpec[] RuntimeStateSpecs =
        {
            new("position-player-start", StageRuntimeStateKind.StageSpawner, 1001, "StageSpawner_PlayerStart", "stage-open", "PGR/NIKKE-style lightweight stage position record."),
            new("position-boss-center", StageRuntimeStateKind.StageSpawner, 2001, "StageSpawner_BossCenter", "boss-entrance-ready", "Boss PositionId remains separate from combat tuning."),
            new("state-intro-handoff", StageRuntimeStateKind.CutsceneHandoff, 4001, "IntroCutscene_End_PlayerHandoffAnchor", "intro-complete", "Intro completion state feeds boss entrance staging."),
            new("state-stage-clear", StageRuntimeStateKind.StageClear, 9001, "StageClear_CorridorExit", "boss-defeated", "Clear state hook; reward payout remains out of scope.")
        };

        private static readonly CutscenePortSpec[] CutscenePortSpecs =
        {
            new(
                "intro-gatepod-port",
                "IntroGatePodAwakeningPort",
                StageCutscenePortKind.Intro,
                "intro-to-stage",
                "IntroCutscene_End_PlayerHandoffAnchor",
                "state-intro-handoff",
                new Vector3(-16.5f, 1.8f, -4.65f),
                new Vector3(0f, 82f, 0f),
                "Placement port for original GatePod, invasion bridge, Timeline, fade, camera, and runner presentation. Temporary Olympus fill roots in the intro review scene are not stage-map content."),
            new(
                "boss-entrance-port",
                "BossEntrancePort",
                StageCutscenePortKind.BossEntrance,
                "boss-entrance",
                "BossEntrance_BossRevealAnchor",
                "position-boss-center",
                new Vector3(15.3f, 1.6f, 0f),
                Vector3.zero,
                "Boss reveal presentation port. It should read the shared boss center anchor before gameplay unlock."),
            new(
                "gameplay-handoff-port",
                "GameplayHandoffPort",
                StageCutscenePortKind.GameplayHandoff,
                "combat-start",
                "Gameplay_CombatStartAnchor",
                "position-player-start",
                new Vector3(-51.402f, 7.985f, -0.411f),
                new Vector3(0f, 82f, 0f),
                "Input and camera unlock port after cutscenes. Combat balance and spawn pacing stay outside this port.")
        };

        private static readonly SourceReferenceSpec[] SourceReferenceSpecs =
        {
            new("ark-pgr-stage-layout", "arkdata:PGR_Tutorial_Stage_Data_2026-06-19", "Use shared scene/layout definitions and lightweight stage records instead of duplicating full scenes per logical stage."),
            new("ark-nikke-stagespawner", "arkdata:NIKKE_LostSectorStageRuntime_ApplyData_2026-06-26", "Use StageSpawner/PositionId/ClearedStages naming as the runtime state vocabulary."),
            new("ark-hi3-stage-context", "arkdata:HI3_CombatCutscene_ApplyData_2026-06-26", "Use HI3 for camera/stage-context and combat presentation vocabulary, not geometry recovery."),
            new("local-olympus-lookdev", SourceScenePath, "Use the current promoted Olympus corridor lookdev as the real geometry source.")
        };

        private static readonly CinematicStageBindingSpec[] CinematicStageBindingSpecs =
        {
            new(
                IntroGatePodProfilePath,
                "DB_Cinematic_IntroGatePodAwakening",
                "intro-to-stage",
                "IntroCutscene_End_PlayerHandoffAnchor",
                "state-intro-handoff",
                "Intro ends in the shared Olympus corridor and feeds the boss-entrance-ready stage state."),
            new(
                BossIntroProfilePath,
                "DB_Cinematic_BossIntro",
                "boss-entrance",
                "BossEntrance_BossRevealAnchor",
                "position-boss-center",
                "Boss reveal reads the shared corridor boss anchor before combat unlock."),
            new(
                GameplayHandoffProfilePath,
                "DB_Cinematic_GameplayHandoff",
                "combat-start",
                "Gameplay_CombatStartAnchor",
                "position-player-start",
                "Gameplay handoff returns to the same corridor map without changing stage scene.")
        };

        [MenuItem("DimensionBrawl/Rebuild Olympus Corridor Stage Definition")]
        public static void RebuildOlympusCorridorStageDefinitionMenu()
        {
            RebuildOlympusCorridorStageDefinition();
            Debug.Log("Rebuilt Olympus corridor stage definition.");
        }

        public static void RebuildOlympusCorridorStageDefinition()
        {
            EnsureFolder(DefinitionRoot);
            StageDefinitionProfile profile = LoadOrCreate<StageDefinitionProfile>(DefinitionPath);

            SerializedObject serialized = new SerializedObject(profile);
            SetString(serialized, "stageId", "OLYMPUS-CORRIDOR-INTRO-COMBAT-01");
            SetString(serialized, "displayName", "Olympus Corridor Invasion");
            SetString(serialized, "chapterId", "OLYMPUS-INVASION");
            SetString(serialized, "previousStageId", string.Empty);
            SetString(serialized, "nextStageId", "OLYMPUS-CORRIDOR-BOSS-CLEAR-01");
            SetString(serialized, "mapScenePath", StageScenePath);
            SetString(serialized, "mapRootName", StageRootName);
            SetString(serialized, "mapContentRootName", StageMapRootName);
            SetVector3(serialized, "mapScale", new Vector3(StageScale, StageScale, StageScale));
            SetString(serialized, "layoutId", "OLYMPUS_CORRIDOR_LONG_01");
            SetString(serialized, "scenePrefabSource", SourceScenePath);
            SetString(serialized, "objective", "Intro cutscene, boss entrance, and combat start share the Olympus corridor map, anchor set, and runtime state vocabulary.");
            SetString(serialized, "clearCondition", "Boss defeated or corridor exit state reached; reward payout and combat balance are intentionally out of scope.");
            SetString(serialized, "excludedScope", "No enemy damage tuning, skill balance, reward payout, generated fake geometry, or final NavMesh bake in this definition.");
            SetAnchors(serialized.FindProperty("anchors"), AnchorSpecs);
            SetSpawns(serialized.FindProperty("spawns"), SpawnSpecs);
            SetHandoffs(serialized.FindProperty("cutsceneHandoffs"), HandoffSpecs);
            SetRuntimeStates(serialized.FindProperty("runtimeStates"), RuntimeStateSpecs);
            SetSourceReferences(serialized.FindProperty("sourceReferences"), SourceReferenceSpecs);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(profile);
            ApplySceneBinding(profile);
            ApplyCinematicStageBindings(profile);
            ValidateOlympusCorridorStageDefinition(profile);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("DimensionBrawl/Validate Olympus Corridor Stage Definition")]
        public static void ValidateOlympusCorridorStageDefinitionMenu()
        {
            ValidateOlympusCorridorStageDefinition();
            Debug.Log("Olympus corridor stage definition validation passed.");
        }

        public static void ValidateOlympusCorridorStageDefinition()
        {
            StageDefinitionProfile profile = LoadRequired<StageDefinitionProfile>(DefinitionPath);
            ValidateOlympusCorridorStageDefinition(profile);
        }

        [MenuItem("DimensionBrawl/Apply Intro GatePod Payload To Olympus Stage")]
        public static void ApplyIntroGatePodPayloadToOlympusStageMenu()
        {
            ApplyIntroGatePodPayloadToOlympusStage();
            Debug.Log("Applied intro GatePod visual payload to the Olympus corridor stage port.");
        }

        public static void ApplyIntroGatePodPayloadToOlympusStage()
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(IntroGatePodReviewScenePath))
            {
                throw new InvalidOperationException($"Missing intro GatePod review scene: {IntroGatePodReviewScenePath}");
            }

            StageDefinitionProfile profile = LoadRequired<StageDefinitionProfile>(DefinitionPath);
            Scene stageScene = EditorSceneManager.OpenScene(profile.MapScenePath, OpenSceneMode.Single);
            GameObject stageRoot = RequireRoot(stageScene, profile.MapRootName);
            Transform introPayloadRoot = RequireIntroPortPayloadRoot(stageRoot.transform);

            RemoveChild(introPayloadRoot, IntroGatePodGeneratedPayloadRootName);
            GameObject generatedRoot = CreateChild(
                introPayloadRoot,
                IntroGatePodGeneratedPayloadRootName,
                Vector3.zero,
                Quaternion.identity,
                Vector3.one);

            Scene sourceScene = default;
            bool sourceSceneOpened = false;
            try
            {
                sourceScene = EditorSceneManager.OpenScene(IntroGatePodReviewScenePath, OpenSceneMode.Additive);
                sourceSceneOpened = true;
                GameObject[] sourceObjects = ResolveTopLevelPayloadSources(sourceScene);

                for (int i = 0; i < sourceObjects.Length; i++)
                {
                    GameObject sourceObject = sourceObjects[i];
                    GameObject copy = UnityEngine.Object.Instantiate(sourceObject);
                    copy.name = sourceObject.name;
                    SceneManager.MoveGameObjectToScene(copy, stageScene);
                    copy.transform.SetParent(generatedRoot.transform, worldPositionStays: false);
                    DisableCopiedIntroPayloadDrivers(copy);
                    EditorUtility.SetDirty(copy);
                }
            }
            finally
            {
                if (sourceSceneOpened && sourceScene.IsValid())
                {
                    EditorSceneManager.CloseScene(sourceScene, removeScene: true);
                }
            }

            int rendererCount = generatedRoot.GetComponentsInChildren<Renderer>(includeInactive: true).Length;
            if (rendererCount == 0)
            {
                throw new InvalidOperationException("Intro GatePod stage payload copied no visible renderers.");
            }

            EditorUtility.SetDirty(generatedRoot);
            EditorSceneManager.MarkSceneDirty(stageScene);
            EditorSceneManager.SaveScene(stageScene);
            AssetDatabase.SaveAssets();

            ValidateOlympusCorridorStageDefinition(profile);
        }

        [MenuItem("DimensionBrawl/Apply Intro GatePod Cutscene Runtime To Olympus Stage")]
        public static void ApplyIntroGatePodCutsceneRuntimeToOlympusStageMenu()
        {
            ApplyIntroGatePodCutsceneRuntimeToOlympusStage();
            Debug.Log("Applied intro GatePod cutscene runtime to the Olympus corridor stage port.");
        }

        public static void ApplyIntroGatePodCutsceneRuntimeToOlympusStage()
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(IntroGatePodReviewScenePath))
            {
                throw new InvalidOperationException($"Missing intro GatePod review scene: {IntroGatePodReviewScenePath}");
            }

            StageDefinitionProfile profile = LoadRequired<StageDefinitionProfile>(DefinitionPath);
            Scene stageScene = EditorSceneManager.OpenScene(profile.MapScenePath, OpenSceneMode.Single);
            GameObject stageRoot = RequireRoot(stageScene, profile.MapRootName);
            Transform introPayloadRoot = RequireIntroPortPayloadRoot(stageRoot.transform);
            Transform visualRoot = RequireChild(introPayloadRoot, IntroGatePodGeneratedPayloadRootName);

            RemoveChild(introPayloadRoot, IntroGatePodRuntimePayloadRootName);
            GameObject runtimeRoot = CreateChild(
                introPayloadRoot,
                IntroGatePodRuntimePayloadRootName,
                visualRoot.localPosition,
                visualRoot.localRotation,
                visualRoot.localScale);

            Scene sourceScene = default;
            bool sourceSceneOpened = false;
            try
            {
                sourceScene = EditorSceneManager.OpenScene(IntroGatePodReviewScenePath, OpenSceneMode.Additive);
                sourceSceneOpened = true;
                CopyIntroRuntimeSources(sourceScene, stageScene, runtimeRoot.transform);
                RebindIntroRuntimeLayer(sourceScene, runtimeRoot.transform, visualRoot);
            }
            finally
            {
                if (sourceSceneOpened && sourceScene.IsValid())
                {
                    EditorSceneManager.CloseScene(sourceScene, removeScene: true);
                }
            }

            ValidateIntroRuntimeLayer(runtimeRoot.transform, visualRoot);
            EditorUtility.SetDirty(runtimeRoot);
            EditorSceneManager.MarkSceneDirty(stageScene);
            EditorSceneManager.SaveScene(stageScene);
            AssetDatabase.SaveAssets();

            ValidateOlympusCorridorStageDefinition(profile);
        }

        private static void ValidateOlympusCorridorStageDefinition(StageDefinitionProfile profile)
        {
            ValidateProfileFields(profile);

            Scene scene = EditorSceneManager.OpenScene(profile.MapScenePath, OpenSceneMode.Single);
            GameObject stageRoot = RequireRoot(scene, profile.MapRootName);
            Transform mapRoot = RequireChild(stageRoot.transform, profile.MapContentRootName);
            if (Vector3.Distance(mapRoot.localScale, profile.MapScale) > 0.001f)
            {
                throw new InvalidOperationException($"{profile.StageId} map scale does not match scene root scale.");
            }

            ValidateSceneBinding(profile, stageRoot, mapRoot);

            Dictionary<string, StageDefinitionProfile.AnchorRef> anchorsById = new Dictionary<string, StageDefinitionProfile.AnchorRef>(StringComparer.Ordinal);
            for (int i = 0; i < profile.AnchorCount; i++)
            {
                StageDefinitionProfile.AnchorRef anchor = profile.GetAnchor(i);
                ValidateAnchor(profile, stageRoot.transform, anchor, anchorsById);
            }

            ValidateSpawns(profile, anchorsById);
            ValidateCutsceneHandoffs(profile, anchorsById);
            ValidateRuntimeStates(profile, anchorsById);
            ValidateCutscenePorts(profile, stageRoot.transform, anchorsById);
            ValidateCinematicStageBindings(profile, anchorsById);
            ValidateSourceReferences(profile);
        }

        private static void ApplySceneBinding(StageDefinitionProfile profile)
        {
            Scene scene = EditorSceneManager.OpenScene(profile.MapScenePath, OpenSceneMode.Single);
            GameObject stageRoot = RequireRoot(scene, profile.MapRootName);
            Transform mapRoot = RequireChild(stageRoot.transform, profile.MapContentRootName);
            Transform stageAnchors = RequireChild(stageRoot.transform, StageAnchorsName);

            StageDefinitionSceneBinding binding = GetOrAddComponent<StageDefinitionSceneBinding>(stageRoot);
            StageAnchorPoint[] points = new StageAnchorPoint[AnchorSpecs.Length];
            StageCutscenePort[] ports = ConfigureCutscenePorts(stageRoot.transform);

            for (int i = 0; i < AnchorSpecs.Length; i++)
            {
                AnchorSpec spec = AnchorSpecs[i];
                Transform group = RequireChild(stageAnchors, spec.GroupId);
                Transform anchor = RequireChild(group, spec.AnchorId);
                ConfigureAnchorTransform(anchor, spec);
                StageAnchorPoint point = GetOrAddComponent<StageAnchorPoint>(anchor.gameObject);
                ConfigureAnchorPoint(point, spec);
                points[i] = point;
            }

            binding.Configure(profile, mapRoot, points, ports);
            if (binding.StageDefinition == null)
            {
                throw new InvalidOperationException($"{profile.StageId} scene binding failed to retain the StageDefinition profile reference.");
            }

            EditorUtility.SetDirty(binding);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static StageCutscenePort[] ConfigureCutscenePorts(Transform stageRoot)
        {
            Transform portsRoot = FindDirectChild(stageRoot, CutscenePortsName);
            if (portsRoot == null)
            {
                portsRoot = CreateChild(stageRoot, CutscenePortsName, Vector3.zero, Quaternion.identity, Vector3.one).transform;
            }

            StageCutscenePort[] ports = new StageCutscenePort[CutscenePortSpecs.Length];
            for (int i = 0; i < CutscenePortSpecs.Length; i++)
            {
                CutscenePortSpec spec = CutscenePortSpecs[i];
                Transform portRoot = FindDirectChild(portsRoot, spec.RootName);
                if (portRoot == null)
                {
                    portRoot = CreateChild(portsRoot, spec.RootName, spec.Position, Quaternion.Euler(spec.Euler), Vector3.one).transform;
                }
                else
                {
                    portRoot.localPosition = spec.Position;
                    portRoot.localRotation = Quaternion.Euler(spec.Euler);
                    portRoot.localScale = Vector3.one;
                }

                Transform payloadRoot = FindDirectChild(portRoot, PortPayloadRootName);
                if (payloadRoot == null)
                {
                    payloadRoot = CreateChild(portRoot, PortPayloadRootName, Vector3.zero, Quaternion.identity, Vector3.one).transform;
                }

                StageCutscenePort port = GetOrAddComponent<StageCutscenePort>(portRoot.gameObject);
                port.Configure(
                    spec.PortId,
                    spec.PortKind,
                    spec.HandoffId,
                    spec.AnchorId,
                    spec.RuntimeStateId,
                    payloadRoot,
                    spec.Purpose);
                EditorUtility.SetDirty(port);
                ports[i] = port;
            }

            EditorUtility.SetDirty(portsRoot);
            return ports;
        }

        private static Transform RequireIntroPortPayloadRoot(Transform stageRoot)
        {
            Transform portsRoot = RequireChild(stageRoot, CutscenePortsName);
            Transform introPortRoot = RequireChild(portsRoot, IntroGatePodPortRootName);
            return RequireChild(introPortRoot, PortPayloadRootName);
        }

        private static GameObject[] ResolveTopLevelPayloadSources(Scene sourceScene)
        {
            List<GameObject> candidates = new List<GameObject>();
            for (int i = 0; i < IntroGatePodPayloadSourceNames.Length; i++)
            {
                string sourceName = IntroGatePodPayloadSourceNames[i];
                GameObject sourceObject = FindRootOrDescendant(sourceScene, sourceName)
                    ?? throw new InvalidOperationException($"Missing intro GatePod payload source `{sourceName}`.");
                candidates.Add(sourceObject);
            }

            List<GameObject> topLevelSources = new List<GameObject>();
            for (int i = 0; i < candidates.Count; i++)
            {
                GameObject candidate = candidates[i];
                if (!HasPayloadAncestor(candidate.transform, candidates))
                {
                    topLevelSources.Add(candidate);
                }
            }

            return topLevelSources.ToArray();
        }

        private static bool HasPayloadAncestor(Transform candidate, List<GameObject> sources)
        {
            Transform parent = candidate.parent;
            while (parent != null)
            {
                for (int i = 0; i < sources.Count; i++)
                {
                    if (sources[i] != null && sources[i].transform == parent)
                    {
                        return true;
                    }
                }

                parent = parent.parent;
            }

            return false;
        }

        private static void DisableCopiedIntroPayloadDrivers(GameObject root)
        {
            foreach (Camera camera in root.GetComponentsInChildren<Camera>(includeInactive: true))
            {
                camera.enabled = false;
                EditorUtility.SetDirty(camera);
            }

            foreach (AudioListener audioListener in root.GetComponentsInChildren<AudioListener>(includeInactive: true))
            {
                audioListener.enabled = false;
                EditorUtility.SetDirty(audioListener);
            }

            foreach (PlayableDirector director in root.GetComponentsInChildren<PlayableDirector>(includeInactive: true))
            {
                director.enabled = false;
                EditorUtility.SetDirty(director);
            }

            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.enabled = false;
                EditorUtility.SetDirty(behaviour);
            }
        }

        private static void CopyIntroRuntimeSources(Scene sourceScene, Scene stageScene, Transform runtimeRoot)
        {
            GameObject[] sourceObjects = ResolveTopLevelRuntimeSources(sourceScene);
            for (int i = 0; i < sourceObjects.Length; i++)
            {
                GameObject sourceObject = sourceObjects[i];
                GameObject copy = UnityEngine.Object.Instantiate(sourceObject);
                copy.name = sourceObject.name;
                SceneManager.MoveGameObjectToScene(copy, stageScene);
                copy.transform.SetParent(runtimeRoot, worldPositionStays: false);
                EditorUtility.SetDirty(copy);
            }
        }

        private static GameObject[] ResolveTopLevelRuntimeSources(Scene sourceScene)
        {
            List<GameObject> candidates = new List<GameObject>();
            for (int i = 0; i < IntroGatePodRuntimeSourceNames.Length; i++)
            {
                string sourceName = IntroGatePodRuntimeSourceNames[i];
                GameObject sourceObject = FindRootOrDescendant(sourceScene, sourceName)
                    ?? throw new InvalidOperationException($"Missing intro GatePod runtime source `{sourceName}`.");
                candidates.Add(sourceObject);
            }

            List<GameObject> topLevelSources = new List<GameObject>();
            for (int i = 0; i < candidates.Count; i++)
            {
                GameObject candidate = candidates[i];
                if (!HasPayloadAncestor(candidate.transform, candidates))
                {
                    topLevelSources.Add(candidate);
                }
            }

            return topLevelSources.ToArray();
        }

        private static void RebindIntroRuntimeLayer(Scene sourceScene, Transform runtimeRoot, Transform visualRoot)
        {
            Camera runtimeCamera = RequireComponentByObjectName<Camera>(runtimeRoot, "Main Camera");
            runtimeCamera.enabled = true;
            runtimeCamera.tag = "MainCamera";
            AudioListener audioListener = runtimeCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
                EditorUtility.SetDirty(audioListener);
            }

            CinemachineBrain brain = RequireComponent<CinemachineBrain>(runtimeCamera.gameObject);
            IntroGatePodCinemachineShotPlayer sourceShotPlayer =
                FindComponentInScene<IntroGatePodCinemachineShotPlayer>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodCinemachineShotPlayer.");
            IntroGatePodCinemachineShotPlayer targetShotPlayer =
                RequireComponentByObjectName<IntroGatePodCinemachineShotPlayer>(
                    runtimeRoot,
                    "IntroGatePodReview_CinemachineShotPlayer");
            IntroGatePodCinemachineShotPlayer.Shot[] targetShots =
                RebindIntroCinemachineShotPlayer(sourceShotPlayer, targetShotPlayer, runtimeRoot, brain);

            PlayableDirector director = RequireComponentByObjectName<PlayableDirector>(
                runtimeRoot,
                "IntroGatePodReview_TimelineDirector");
            RebindIntroTimelineDirector(director, brain, targetShots, runtimeRoot, visualRoot);
            RebindIntroCueDirector(sourceScene, runtimeRoot, targetShots);
            RebindIntroFirstPersonRendererMask(sourceScene, runtimeRoot, visualRoot, director);
            RebindIntroRunner(runtimeRoot, visualRoot, runtimeCamera);
            RebindIntroInvasionBridge(sourceScene, runtimeRoot, visualRoot, director, runtimeCamera);
            EnableIntroVisualPresentationDrivers(visualRoot);

            EditorUtility.SetDirty(runtimeCamera);
            EditorUtility.SetDirty(brain);
        }

        private static IntroGatePodCinemachineShotPlayer.Shot[] RebindIntroCinemachineShotPlayer(
            IntroGatePodCinemachineShotPlayer sourceShotPlayer,
            IntroGatePodCinemachineShotPlayer targetShotPlayer,
            Transform runtimeRoot,
            CinemachineBrain brain)
        {
            IntroGatePodCinemachineShotPlayer.Shot[] sourceShots = sourceShotPlayer.Shots;
            IntroGatePodCinemachineShotPlayer.Shot[] targetShots =
                new IntroGatePodCinemachineShotPlayer.Shot[sourceShots.Length];
            for (int i = 0; i < sourceShots.Length; i++)
            {
                IntroGatePodCinemachineShotPlayer.Shot sourceShot = sourceShots[i];
                if (sourceShot.Camera == null)
                {
                    throw new InvalidOperationException($"Intro GatePod shot {sourceShot.ShotId} has no source camera.");
                }

                CinemachineCamera targetCamera =
                    RequireComponentByObjectName<CinemachineCamera>(runtimeRoot, sourceShot.Camera.name);
                targetCamera.gameObject.SetActive(true);
                targetCamera.enabled = true;
                targetShots[i] = new IntroGatePodCinemachineShotPlayer.Shot(
                    sourceShot.ShotId,
                    sourceShot.StartSeconds,
                    targetCamera,
                    sourceShot.BlendStyle,
                    sourceShot.BlendSeconds);
                EditorUtility.SetDirty(targetCamera);
            }

            targetShotPlayer.Configure(brain, targetShots, false, true);
            targetShotPlayer.enabled = false;
            EditorUtility.SetDirty(targetShotPlayer);
            return targetShots;
        }

        private static void RebindIntroTimelineDirector(
            PlayableDirector director,
            CinemachineBrain brain,
            IntroGatePodCinemachineShotPlayer.Shot[] shots,
            Transform runtimeRoot,
            Transform visualRoot)
        {
            TimelineAsset timeline = director.playableAsset as TimelineAsset
                ?? throw new InvalidOperationException("Intro GatePod runtime PlayableDirector has no TimelineAsset.");
            director.playOnAwake = true;
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.extrapolationMode = DirectorWrapMode.Hold;

            CinemachineTrack cameraTrack = FindTimelineTrack<CinemachineTrack>(timeline, "Cinemachine Shots")
                ?? throw new InvalidOperationException("Intro GatePod Timeline is missing the Cinemachine Shots track.");
            director.SetGenericBinding(cameraTrack, brain);
            BindCinemachineTimelineClips(director, cameraTrack, shots);

            AnimationTrack openingDollyTrack = FindTimelineTrack<AnimationTrack>(timeline, "Opening Dolly")
                ?? throw new InvalidOperationException("Intro GatePod Timeline is missing the Opening Dolly track.");
            CinemachineSplineDolly openingDolly = ResolveOpeningDolly(shots);
            Animator openingAnimator = openingDolly.GetComponent<Animator>()
                ?? throw new InvalidOperationException("Intro GatePod opening dolly camera is missing an Animator.");
            director.SetGenericBinding(openingDollyTrack, openingAnimator);

            AnimationTrack inoriBodyTrack = FindTimelineTrack<AnimationTrack>(timeline, "Inori Body")
                ?? throw new InvalidOperationException("Intro GatePod Timeline is missing the Inori Body track.");
            Animator inoriAnimator = RequireVisualInoriAnimator(visualRoot);
            director.SetGenericBinding(inoriBodyTrack, inoriAnimator);

            AudioTrack voiceTrack = FindTimelineTrack<AudioTrack>(timeline, "Voice")
                ?? throw new InvalidOperationException("Intro GatePod Timeline is missing the Voice track.");
            director.SetGenericBinding(
                voiceTrack,
                RequireComponentByObjectName<AudioSource>(runtimeRoot, "IntroGatePodReview_VoiceTimelineAudio"));

            AudioTrack bgmTrack = FindTimelineTrack<AudioTrack>(timeline, "BGM")
                ?? throw new InvalidOperationException("Intro GatePod Timeline is missing the BGM track.");
            director.SetGenericBinding(
                bgmTrack,
                RequireComponentByObjectName<AudioSource>(runtimeRoot, "IntroGatePodReview_BgmTimelineAudio"));

            IntroGatePodFadeTrack fadeTrack = FindTimelineTrack<IntroGatePodFadeTrack>(timeline, "Fade")
                ?? throw new InvalidOperationException("Intro GatePod Timeline is missing the Fade track.");
            IntroGatePodTimelineFadeOverlay fadeOverlay =
                RequireComponentByObjectName<IntroGatePodTimelineFadeOverlay>(
                    runtimeRoot,
                    "IntroGatePodReview_TimelineFadeOverlay");
            director.SetGenericBinding(fadeTrack, fadeOverlay);
            fadeOverlay.enabled = true;

            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(fadeOverlay);
        }

        private static void BindCinemachineTimelineClips(
            PlayableDirector director,
            CinemachineTrack track,
            IntroGatePodCinemachineShotPlayer.Shot[] shots)
        {
            int clipIndex = 0;
            foreach (TimelineClip clip in track.GetClips())
            {
                CinemachineShot shotAsset = clip.asset as CinemachineShot;
                if (shotAsset == null)
                {
                    continue;
                }

                CinemachineCamera camera = FindShotCamera(shots, clip.displayName);
                if (camera == null && clipIndex < shots.Length)
                {
                    camera = shots[clipIndex].Camera;
                }

                if (camera == null)
                {
                    throw new InvalidOperationException($"Timeline Cinemachine clip `{clip.displayName}` has no copied camera binding.");
                }

                PropertyName exposedName = shotAsset.VirtualCamera.exposedName;
                if (string.IsNullOrWhiteSpace(exposedName.ToString()))
                {
                    exposedName = new PropertyName($"cm_stage_{clipIndex + 1:00}_{SanitizeExposedReferenceId(clip.displayName)}");
                    shotAsset.VirtualCamera.exposedName = exposedName;
                    EditorUtility.SetDirty(shotAsset);
                }

                director.SetReferenceValue(exposedName, camera);
                clipIndex++;
            }
        }

        private static void RebindIntroCueDirector(
            Scene sourceScene,
            Transform runtimeRoot,
            IntroGatePodCinemachineShotPlayer.Shot[] targetShots)
        {
            IntroGatePodCutsceneCueDirector sourceCueDirector =
                FindComponentInScene<IntroGatePodCutsceneCueDirector>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodCutsceneCueDirector.");
            IntroGatePodCutsceneCueDirector targetCueDirector =
                RequireComponentByObjectName<IntroGatePodCutsceneCueDirector>(
                    runtimeRoot,
                    "IntroGatePodReview_CueDirector");

            IntroGatePodCutsceneCueDirector.DollyCue[] sourceDollyCues = sourceCueDirector.DollyCues;
            IntroGatePodCutsceneCueDirector.DollyCue[] targetDollyCues =
                new IntroGatePodCutsceneCueDirector.DollyCue[sourceDollyCues.Length];
            for (int i = 0; i < sourceDollyCues.Length; i++)
            {
                IntroGatePodCutsceneCueDirector.DollyCue sourceCue = sourceDollyCues[i];
                CinemachineSplineDolly targetDolly = sourceCue.Dolly != null
                    ? RequireComponentByObjectName<CinemachineSplineDolly>(runtimeRoot, sourceCue.Dolly.gameObject.name)
                    : ResolveOpeningDolly(targetShots);
                targetDollyCues[i] = new IntroGatePodCutsceneCueDirector.DollyCue(
                    sourceCue.CueId,
                    sourceCue.StartSeconds,
                    sourceCue.DurationSeconds,
                    targetDolly,
                    sourceCue.FromPosition,
                    sourceCue.ToPosition);
            }

            IntroGatePodCutsceneCueDirector.VoiceCue[] sourceVoiceCues = sourceCueDirector.VoiceCues;
            IntroGatePodCutsceneCueDirector.VoiceCue[] targetVoiceCues =
                new IntroGatePodCutsceneCueDirector.VoiceCue[sourceVoiceCues.Length];
            for (int i = 0; i < sourceVoiceCues.Length; i++)
            {
                IntroGatePodCutsceneCueDirector.VoiceCue sourceCue = sourceVoiceCues[i];
                AudioSource targetSource = sourceCue.AudioSource != null
                    ? RequireComponentByObjectName<AudioSource>(runtimeRoot, sourceCue.AudioSource.name)
                    : null;
                targetVoiceCues[i] = new IntroGatePodCutsceneCueDirector.VoiceCue(
                    sourceCue.CueId,
                    sourceCue.StartSeconds,
                    targetSource);
            }

            targetCueDirector.Configure(
                targetDollyCues,
                targetVoiceCues,
                sourceCueDirector.FadeCues,
                false,
                true);
            targetCueDirector.enabled = true;
            EditorUtility.SetDirty(targetCueDirector);
        }

        private static void RebindIntroFirstPersonRendererMask(
            Scene sourceScene,
            Transform runtimeRoot,
            Transform visualRoot,
            PlayableDirector director)
        {
            IntroGatePodFirstPersonRendererMask sourceMask =
                FindComponentInScene<IntroGatePodFirstPersonRendererMask>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodFirstPersonRendererMask.");
            IntroGatePodFirstPersonRendererMask targetMask =
                RequireComponentByObjectName<IntroGatePodFirstPersonRendererMask>(
                    runtimeRoot,
                    "IntroGatePodReview_FirstPersonRendererMask");
            GameObject visualInori = RequireVisualInori(visualRoot);
            Renderer[] hiddenRenderers = ResolveFirstPersonHiddenRenderers(visualInori);
            if (hiddenRenderers.Length == 0)
            {
                throw new InvalidOperationException("Intro GatePod first-person mask could not find head or hair renderers on the copied Inori visual.");
            }

            SerializedObject sourceSerialized = new SerializedObject(sourceMask);
            targetMask.Configure(
                director,
                hiddenRenderers,
                GetFloat(sourceSerialized, "hideStartSeconds"),
                GetFloat(sourceSerialized, "hideEndSeconds"));
            targetMask.enabled = true;
            EditorUtility.SetDirty(targetMask);
        }

        private static void RebindIntroRunner(Transform runtimeRoot, Transform visualRoot, Camera runtimeCamera)
        {
            CinematicSequenceRunner runner =
                RequireComponentByObjectName<CinematicSequenceRunner>(runtimeRoot, "IntroGatePodReview_Runner");
            GameObject visualInori = RequireVisualInori(visualRoot);
            Animator inoriAnimator = RequireVisualInoriAnimator(visualRoot);
            CinematicBlendShapeExpressionPlayer expressionPlayer =
                visualInori.GetComponentInChildren<CinematicBlendShapeExpressionPlayer>(includeInactive: true);
            if (expressionPlayer != null)
            {
                expressionPlayer.enabled = true;
                EditorUtility.SetDirty(expressionPlayer);
            }

            ActionCameraController cameraController = runtimeCamera.GetComponent<ActionCameraController>();
            if (cameraController != null)
            {
                cameraController.enabled = false;
                EditorUtility.SetDirty(cameraController);
            }

            SerializedObject serializedRunner = new SerializedObject(runner);
            SetObjectReference(serializedRunner, "sequenceProfile", LoadRequired<CinematicSequenceProfile>(IntroGatePodProfilePath));
            SetObjectReference(
                serializedRunner,
                "bodyControllerOverride",
                LoadRequired<RuntimeAnimatorController>(BuildResubmissionCinematicAnimationSetup.CinematicControllerPath));
            SetObjectReference(serializedRunner, "cameraController", cameraController);
            SetObjectReference(serializedRunner, "cinematicCamera", runtimeCamera);
            SetObjectReference(serializedRunner, "cueSpace", visualInori.transform);
            RequireProperty(serializedRunner, "driveCameraTransformFromProfile").boolValue = false;
            RequireProperty(serializedRunner, "disableActionCameraControllerDuringPoseDrive").boolValue = true;

            SerializedProperty bindings = RequireProperty(serializedRunner, "actorBindings");
            bindings.arraySize = 1;
            SerializedProperty binding = bindings.GetArrayElementAtIndex(0);
            SetRelativeEnum(binding, "role", (int)CinematicSequenceProfile.ActorRole.Inori);
            SetRelativeObjectReference(binding, "bodyAnimator", inoriAnimator);
            SetRelativeObjectReference(binding, "faceAnimator", null);
            SetRelativeObjectReference(binding, "expressionPlayer", expressionPlayer);
            SetRelativeObjectReference(binding, "anchor", visualInori.transform);
            serializedRunner.ApplyModifiedPropertiesWithoutUndo();

            runner.enabled = true;
            CinematicSequenceAutoPlay autoPlay = runner.GetComponent<CinematicSequenceAutoPlay>();
            if (autoPlay != null)
            {
                SetObjectReference(autoPlay, "runner", runner);
                SetBool(autoPlay, "playOnStart", false);
            }

            EditorUtility.SetDirty(runner);
        }

        private static void RebindIntroInvasionBridge(
            Scene sourceScene,
            Transform runtimeRoot,
            Transform visualRoot,
            PlayableDirector director,
            Camera runtimeCamera)
        {
            IntroGatePodInvasionBridgeCue sourceBridge =
                FindComponentInScene<IntroGatePodInvasionBridgeCue>(sourceScene)
                ?? throw new InvalidOperationException("Missing source IntroGatePodInvasionBridgeCue.");
            IntroGatePodInvasionBridgeCue targetBridge =
                FindComponentByObjectName<IntroGatePodInvasionBridgeCue>(visualRoot, "IntroGatePodReview_InvasionBridge")
                ?? throw new InvalidOperationException("Missing copied IntroGatePodReview_InvasionBridge cue under the visual payload.");

            IntroGatePodInvasionBridgeCue.CommandoCue[] sourceCommandos = sourceBridge.Commandos;
            IntroGatePodInvasionBridgeCue.CommandoCue[] targetCommandos =
                new IntroGatePodInvasionBridgeCue.CommandoCue[sourceCommandos.Length];
            for (int i = 0; i < sourceCommandos.Length; i++)
            {
                IntroGatePodInvasionBridgeCue.CommandoCue sourceCue = sourceCommandos[i];
                if (sourceCue.Root == null)
                {
                    throw new InvalidOperationException("Intro GatePod source Commando cue has no root.");
                }

                Transform targetRoot = RequireDescendantOrSelf(visualRoot, sourceCue.Root.name);
                Animator targetAnimator = targetRoot.GetComponentInChildren<Animator>(includeInactive: true);
                targetCommandos[i] = new IntroGatePodInvasionBridgeCue.CommandoCue(
                    targetRoot,
                    targetAnimator,
                    sourceCue.RunStateName,
                    sourceCue.StartSeconds,
                    sourceCue.EndSeconds,
                    sourceCue.StartLocalPosition,
                    sourceCue.EndLocalPosition,
                    sourceCue.LocalEulerAngles,
                    sourceCue.NormalizedTimeOffset);
            }

            GameObject targetExplosionRoot = sourceBridge.ExplosionRoot != null
                ? RequireDescendantOrSelf(visualRoot, sourceBridge.ExplosionRoot.name).gameObject
                : null;
            Light targetExplosionLight = targetExplosionRoot != null
                ? targetExplosionRoot.GetComponentInChildren<Light>(includeInactive: true)
                : null;
            SerializedObject sourceSerialized = new SerializedObject(sourceBridge);
            targetBridge.Configure(
                director,
                targetCommandos,
                targetExplosionRoot,
                targetExplosionLight,
                GetFloat(sourceSerialized, "explosionStartSeconds"),
                GetFloat(sourceSerialized, "explosionDurationSeconds"),
                GetVector3(sourceSerialized, "explosionRestScale"),
                GetVector3(sourceSerialized, "explosionPeakScale"),
                GetFloat(sourceSerialized, "explosionPeakLightIntensity"));
            targetBridge.ConfigurePresentation(
                runtimeCamera,
                RequireComponentByObjectName<CanvasGroup>(runtimeRoot, "IntroGatePodReview_InvasionImpactFlash"),
                RequireComponentByObjectName<CanvasGroup>(runtimeRoot, "IntroGatePodReview_InvasionWarningSweep"),
                GetFloat(sourceSerialized, "explosionAfterSmokeSeconds"),
                GetFloat(sourceSerialized, "warningSweepLeadSeconds"),
                GetFloat(sourceSerialized, "warningSweepDurationSeconds"),
                GetFloat(sourceSerialized, "impactFlashPeakAlpha"),
                GetVector3(sourceSerialized, "cameraShakePositionAmplitude"),
                GetVector3(sourceSerialized, "cameraShakeEulerAmplitude"),
                GetFloat(sourceSerialized, "cameraShakeDurationSeconds"));
            targetBridge.enabled = true;
            targetBridge.Sample(0f);
            EditorUtility.SetDirty(targetBridge);
        }

        private static void EnableIntroVisualPresentationDrivers(Transform visualRoot)
        {
            foreach (CinematicBlendShapeExpressionPlayer expressionPlayer in
                visualRoot.GetComponentsInChildren<CinematicBlendShapeExpressionPlayer>(includeInactive: true))
            {
                expressionPlayer.enabled = true;
                EditorUtility.SetDirty(expressionPlayer);
            }

            foreach (RifleGirlWeaponSocketDriver socketDriver in
                visualRoot.GetComponentsInChildren<RifleGirlWeaponSocketDriver>(includeInactive: true))
            {
                socketDriver.enabled = true;
                EditorUtility.SetDirty(socketDriver);
            }
        }

        private static void ValidateIntroRuntimeLayer(Transform runtimeRoot, Transform visualRoot)
        {
            Camera camera = RequireComponentByObjectName<Camera>(runtimeRoot, "Main Camera");
            RequireComponent<CinemachineBrain>(camera.gameObject);

            PlayableDirector director = RequireComponentByObjectName<PlayableDirector>(
                runtimeRoot,
                "IntroGatePodReview_TimelineDirector");
            TimelineAsset timeline = director.playableAsset as TimelineAsset
                ?? throw new InvalidOperationException("Intro GatePod runtime director has no TimelineAsset.");

            CinemachineTrack cameraTrack = FindTimelineTrack<CinemachineTrack>(timeline, "Cinemachine Shots")
                ?? throw new InvalidOperationException("Intro GatePod runtime Timeline has no Cinemachine Shots track.");
            if (director.GetGenericBinding(cameraTrack) == null)
            {
                throw new InvalidOperationException("Intro GatePod runtime Timeline has no CinemachineBrain binding.");
            }

            IntroGatePodCinemachineShotPlayer shotPlayer =
                RequireComponentByObjectName<IntroGatePodCinemachineShotPlayer>(
                    runtimeRoot,
                    "IntroGatePodReview_CinemachineShotPlayer");
            if (shotPlayer.Shots.Length == 0)
            {
                throw new InvalidOperationException("Intro GatePod runtime shot player has no copied Cinemachine shots.");
            }

            IntroGatePodTimelineFadeOverlay fadeOverlay =
                RequireComponentByObjectName<IntroGatePodTimelineFadeOverlay>(
                    runtimeRoot,
                    "IntroGatePodReview_TimelineFadeOverlay");
            if (!fadeOverlay.HasCanvasGroup)
            {
                throw new InvalidOperationException("Intro GatePod runtime fade overlay has no CanvasGroup.");
            }

            IntroGatePodFirstPersonRendererMask rendererMask =
                RequireComponentByObjectName<IntroGatePodFirstPersonRendererMask>(
                    runtimeRoot,
                    "IntroGatePodReview_FirstPersonRendererMask");
            if (rendererMask.HiddenRendererCount == 0)
            {
                throw new InvalidOperationException("Intro GatePod runtime first-person renderer mask has no hidden renderers.");
            }

            CinematicSequenceRunner runner =
                RequireComponentByObjectName<CinematicSequenceRunner>(runtimeRoot, "IntroGatePodReview_Runner");
            if (runner.SequenceProfile == null || runner.CinematicCamera != camera)
            {
                throw new InvalidOperationException("Intro GatePod runtime runner is not bound to the profile and runtime camera.");
            }

            IntroGatePodInvasionBridgeCue invasionBridge =
                FindComponentByObjectName<IntroGatePodInvasionBridgeCue>(visualRoot, "IntroGatePodReview_InvasionBridge")
                ?? throw new InvalidOperationException("Intro GatePod visual bridge cue is missing.");
            if (!invasionBridge.enabled
                || invasionBridge.Commandos.Length < 3
                || invasionBridge.ExplosionRoot == null)
            {
                throw new InvalidOperationException("Intro GatePod visual bridge cue is not runtime-ready.");
            }
        }

        private static void ApplyCinematicStageBindings(StageDefinitionProfile profile)
        {
            for (int i = 0; i < CinematicStageBindingSpecs.Length; i++)
            {
                CinematicStageBindingSpec spec = CinematicStageBindingSpecs[i];
                CinematicSequenceProfile cinematicProfile =
                    LoadRequired<CinematicSequenceProfile>(spec.CinematicProfilePath);
                cinematicProfile.ConfigureStageContext(
                    profile,
                    spec.HandoffId,
                    spec.AnchorId,
                    spec.RuntimeStateId,
                    spec.Note);
                EditorUtility.SetDirty(cinematicProfile);
            }
        }

        private static void ConfigureAnchorPoint(StageAnchorPoint point, AnchorSpec spec)
        {
            SerializedObject serialized = new SerializedObject(point);
            SetString(serialized, "anchorId", spec.AnchorId);
            SetString(serialized, "groupId", spec.GroupId);
            serialized.FindProperty("usageKind").enumValueIndex = (int)ResolveUsageKind(spec);
            serialized.FindProperty("positionId").intValue = ResolvePositionId(spec.AnchorId);
            serialized.FindProperty("spawnKind").enumValueIndex = (int)ResolveSpawnKind(spec.AnchorId);
            serialized.FindProperty("runtimeStateKind").enumValueIndex = (int)ResolveRuntimeStateKind(spec.AnchorId);
            SetString(serialized, "purpose", spec.Purpose);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(point);
        }

        private static void ConfigureAnchorTransform(Transform anchor, AnchorSpec spec)
        {
            anchor.localPosition = spec.Position;
            anchor.localRotation = Quaternion.Euler(spec.Euler);
            anchor.localScale = Vector3.one;
            EditorUtility.SetDirty(anchor);
        }

        private static StageAnchorUsageKind ResolveUsageKind(AnchorSpec spec)
        {
            if (string.Equals(spec.GroupId, CombatAnchorsName, StringComparison.Ordinal))
            {
                return StageAnchorUsageKind.CombatSpawn;
            }

            if (string.Equals(spec.GroupId, CutsceneAnchorsName, StringComparison.Ordinal))
            {
                return StageAnchorUsageKind.CutsceneHandoff;
            }

            if (string.Equals(spec.GroupId, RuntimeAnchorsName, StringComparison.Ordinal))
            {
                return StageAnchorUsageKind.RuntimeState;
            }

            return StageAnchorUsageKind.Generic;
        }

        private static int ResolvePositionId(string anchorId)
        {
            for (int i = 0; i < RuntimeStateSpecs.Length; i++)
            {
                if (string.Equals(RuntimeStateSpecs[i].AnchorId, anchorId, StringComparison.Ordinal))
                {
                    return RuntimeStateSpecs[i].PositionId;
                }
            }

            for (int i = 0; i < SpawnSpecs.Length; i++)
            {
                if (string.Equals(SpawnSpecs[i].AnchorId, anchorId, StringComparison.Ordinal))
                {
                    return SpawnSpecs[i].PositionId;
                }
            }

            return 0;
        }

        private static StageSpawnKind ResolveSpawnKind(string anchorId)
        {
            for (int i = 0; i < SpawnSpecs.Length; i++)
            {
                if (string.Equals(SpawnSpecs[i].AnchorId, anchorId, StringComparison.Ordinal))
                {
                    return SpawnSpecs[i].SpawnKind;
                }
            }

            return StageSpawnKind.Objective;
        }

        private static StageRuntimeStateKind ResolveRuntimeStateKind(string anchorId)
        {
            for (int i = 0; i < RuntimeStateSpecs.Length; i++)
            {
                if (string.Equals(RuntimeStateSpecs[i].AnchorId, anchorId, StringComparison.Ordinal))
                {
                    return RuntimeStateSpecs[i].StateKind;
                }
            }

            return StageRuntimeStateKind.CutsceneHandoff;
        }

        private static void ValidateSceneBinding(StageDefinitionProfile profile, GameObject stageRoot, Transform mapRoot)
        {
            StageDefinitionSceneBinding binding = stageRoot.GetComponent<StageDefinitionSceneBinding>();
            if (binding == null)
            {
                throw new InvalidOperationException($"{profile.StageId} scene root has no StageDefinitionSceneBinding.");
            }

            string boundProfilePath = AssetDatabase.GetAssetPath(binding.StageDefinition);
            if (!string.Equals(boundProfilePath, DefinitionPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.StageId} scene binding references the wrong StageDefinition profile: {boundProfilePath}");
            }

            if (binding.MapRoot != mapRoot)
            {
                throw new InvalidOperationException($"{profile.StageId} scene binding does not reference the map root.");
            }

            if (binding.AnchorPointCount < AnchorSpecs.Length)
            {
                throw new InvalidOperationException($"{profile.StageId} scene binding does not expose all anchor points.");
            }

            if (binding.CutscenePortCount < CutscenePortSpecs.Length)
            {
                throw new InvalidOperationException($"{profile.StageId} scene binding does not expose all cutscene ports.");
            }
        }

        private static void ValidateProfileFields(StageDefinitionProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.StageId))
            {
                throw new InvalidOperationException("Olympus corridor stage definition has no stage id.");
            }

            if (!string.Equals(profile.MapScenePath, StageScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.StageId} should reference the shared Olympus corridor stage scene.");
            }

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(profile.MapScenePath))
            {
                throw new InvalidOperationException($"{profile.StageId} references a missing map scene: {profile.MapScenePath}");
            }

            if (profile.AnchorCount < AnchorSpecs.Length)
            {
                throw new InvalidOperationException($"{profile.StageId} does not preserve all required Olympus stage anchors.");
            }

            if (profile.SpawnCount < SpawnSpecs.Length)
            {
                throw new InvalidOperationException($"{profile.StageId} does not preserve all required spawn records.");
            }

            if (profile.CutsceneHandoffCount < 3)
            {
                throw new InvalidOperationException($"{profile.StageId} needs intro, boss, and gameplay handoff records.");
            }

            if (profile.RuntimeStateCount < 4)
            {
                throw new InvalidOperationException($"{profile.StageId} needs stage position and clear state records.");
            }
        }

        private static void ValidateAnchor(
            StageDefinitionProfile profile,
            Transform stageRoot,
            StageDefinitionProfile.AnchorRef anchor,
            Dictionary<string, StageDefinitionProfile.AnchorRef> anchorsById)
        {
            if (string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                throw new InvalidOperationException($"{profile.StageId} has an anchor without an id.");
            }

            if (anchorsById.ContainsKey(anchor.AnchorId))
            {
                throw new InvalidOperationException($"{profile.StageId} has duplicate anchor id {anchor.AnchorId}.");
            }

            anchorsById.Add(anchor.AnchorId, anchor);

            Transform stageAnchors = RequireChild(stageRoot, StageAnchorsName);
            Transform group = RequireChild(stageAnchors, anchor.GroupId);
            Transform sceneAnchor = RequireChild(group, anchor.AnchorId);
            if (Vector3.Distance(sceneAnchor.localPosition, anchor.ExpectedPosition) > 0.05f)
            {
                throw new InvalidOperationException($"{profile.StageId}/{anchor.AnchorId} position does not match scene anchor.");
            }

            if (Quaternion.Angle(sceneAnchor.localRotation, Quaternion.Euler(anchor.ExpectedEuler)) > 0.25f)
            {
                throw new InvalidOperationException($"{profile.StageId}/{anchor.AnchorId} rotation does not match scene anchor.");
            }

            StageAnchorPoint point = sceneAnchor.GetComponent<StageAnchorPoint>();
            if (point == null)
            {
                throw new InvalidOperationException($"{profile.StageId}/{anchor.AnchorId} has no StageAnchorPoint component.");
            }

            if (!string.Equals(point.AnchorId, anchor.AnchorId, StringComparison.Ordinal)
                || !string.Equals(point.GroupId, anchor.GroupId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{profile.StageId}/{anchor.AnchorId} StageAnchorPoint metadata does not match the StageDefinition anchor.");
            }
        }

        private static void ValidateSpawns(
            StageDefinitionProfile profile,
            Dictionary<string, StageDefinitionProfile.AnchorRef> anchorsById)
        {
            HashSet<int> positionIds = new HashSet<int>();
            for (int i = 0; i < profile.SpawnCount; i++)
            {
                StageDefinitionProfile.SpawnRef spawn = profile.GetSpawn(i);
                if (string.IsNullOrWhiteSpace(spawn.SpawnId))
                {
                    throw new InvalidOperationException($"{profile.StageId} has a spawn without an id.");
                }

                if (spawn.PositionId <= 0 || !positionIds.Add(spawn.PositionId))
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spawn.SpawnId} has an invalid or duplicate PositionId.");
                }

                if (!anchorsById.ContainsKey(spawn.AnchorId))
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spawn.SpawnId} references missing anchor {spawn.AnchorId}.");
                }

                if (spawn.Count <= 0)
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spawn.SpawnId} has no spawn count.");
                }
            }
        }

        private static void ValidateCutsceneHandoffs(
            StageDefinitionProfile profile,
            Dictionary<string, StageDefinitionProfile.AnchorRef> anchorsById)
        {
            HashSet<string> required = new HashSet<string>(StringComparer.Ordinal)
            {
                "intro-to-stage",
                "boss-entrance",
                "combat-start"
            };

            for (int i = 0; i < profile.CutsceneHandoffCount; i++)
            {
                StageDefinitionProfile.CutsceneHandoffRef handoff = profile.GetCutsceneHandoff(i);
                required.Remove(handoff.HandoffId);
                if (string.IsNullOrWhiteSpace(handoff.HandoffId))
                {
                    throw new InvalidOperationException($"{profile.StageId} has a cutscene handoff without an id.");
                }

                if (!anchorsById.ContainsKey(handoff.AnchorId))
                {
                    throw new InvalidOperationException($"{profile.StageId}/{handoff.HandoffId} references missing anchor {handoff.AnchorId}.");
                }
            }

            if (required.Count > 0)
            {
                throw new InvalidOperationException($"{profile.StageId} is missing required cutscene handoff records.");
            }
        }

        private static void ValidateRuntimeStates(
            StageDefinitionProfile profile,
            Dictionary<string, StageDefinitionProfile.AnchorRef> anchorsById)
        {
            bool hasStageClear = false;
            for (int i = 0; i < profile.RuntimeStateCount; i++)
            {
                StageDefinitionProfile.RuntimeStateRef state = profile.GetRuntimeState(i);
                if (string.IsNullOrWhiteSpace(state.StateId))
                {
                    throw new InvalidOperationException($"{profile.StageId} has a runtime state without an id.");
                }

                if (!anchorsById.ContainsKey(state.AnchorId))
                {
                    throw new InvalidOperationException($"{profile.StageId}/{state.StateId} references missing anchor {state.AnchorId}.");
                }

                hasStageClear |= state.StateKind == StageRuntimeStateKind.StageClear;
            }

            if (!hasStageClear)
            {
                throw new InvalidOperationException($"{profile.StageId} needs a StageClear runtime state.");
            }
        }

        private static void ValidateCutscenePorts(
            StageDefinitionProfile profile,
            Transform stageRoot,
            Dictionary<string, StageDefinitionProfile.AnchorRef> anchorsById)
        {
            Transform portsRoot = RequireChild(stageRoot, CutscenePortsName);
            HashSet<string> portIds = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> handoffIds = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < CutscenePortSpecs.Length; i++)
            {
                CutscenePortSpec spec = CutscenePortSpecs[i];
                Transform portRoot = RequireChild(portsRoot, spec.RootName);
                StageCutscenePort port = portRoot.GetComponent<StageCutscenePort>();
                if (port == null)
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spec.RootName} has no StageCutscenePort component.");
                }

                if (Vector3.Distance(portRoot.localPosition, spec.Position) > 0.05f)
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spec.RootName} position does not match its cutscene port spec.");
                }

                if (!portIds.Add(port.PortId) || !handoffIds.Add(port.HandoffId))
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spec.RootName} has duplicate cutscene port metadata.");
                }

                if (!string.Equals(port.PortId, spec.PortId, StringComparison.Ordinal)
                    || port.PortKind != spec.PortKind
                    || !string.Equals(port.HandoffId, spec.HandoffId, StringComparison.Ordinal)
                    || !string.Equals(port.AnchorId, spec.AnchorId, StringComparison.Ordinal)
                    || !string.Equals(port.RuntimeStateId, spec.RuntimeStateId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spec.RootName} metadata does not match the shared stage handoff contract.");
                }

                if (!anchorsById.ContainsKey(port.AnchorId))
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spec.RootName} references missing anchor {port.AnchorId}.");
                }

                RequireHandoff(profile, port.HandoffId);
                RequireRuntimeState(profile, port.RuntimeStateId);

                if (!port.HasPayloadRoot || port.PayloadRoot.parent != portRoot)
                {
                    throw new InvalidOperationException($"{profile.StageId}/{spec.RootName} needs a local payload root for cutscene-only props.");
                }
            }
        }

        private static void ValidateCinematicStageBindings(
            StageDefinitionProfile profile,
            Dictionary<string, StageDefinitionProfile.AnchorRef> anchorsById)
        {
            for (int i = 0; i < CinematicStageBindingSpecs.Length; i++)
            {
                CinematicStageBindingSpec spec = CinematicStageBindingSpecs[i];
                CinematicSequenceProfile cinematicProfile =
                    LoadRequired<CinematicSequenceProfile>(spec.CinematicProfilePath);
                string boundDefinitionPath = AssetDatabase.GetAssetPath(cinematicProfile.StageDefinition);
                if (!string.Equals(boundDefinitionPath, DefinitionPath, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"{profile.StageId}/{cinematicProfile.name} does not reference the shared Olympus stage definition.");
                }

                if (!cinematicProfile.RequiresStageDefinition || !cinematicProfile.HasStageContext)
                {
                    throw new InvalidOperationException(
                        $"{profile.StageId}/{cinematicProfile.name} has no complete required stage context.");
                }

                if (!string.Equals(cinematicProfile.StageHandoffId, spec.HandoffId, StringComparison.Ordinal)
                    || !string.Equals(cinematicProfile.StageAnchorId, spec.AnchorId, StringComparison.Ordinal)
                    || !string.Equals(cinematicProfile.StageRuntimeStateId, spec.RuntimeStateId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"{profile.StageId}/{cinematicProfile.name} stage context ids do not match the expected Olympus handoff contract.");
                }

                if (!anchorsById.ContainsKey(spec.AnchorId))
                {
                    throw new InvalidOperationException(
                        $"{profile.StageId}/{cinematicProfile.name} references missing stage anchor {spec.AnchorId}.");
                }

                StageDefinitionProfile.CutsceneHandoffRef handoff =
                    RequireHandoff(profile, spec.HandoffId);
                if (!string.Equals(handoff.AnchorId, spec.AnchorId, StringComparison.Ordinal)
                    || !string.Equals(handoff.CinematicProfileId, spec.CinematicProfileId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"{profile.StageId}/{cinematicProfile.name} does not match its StageDefinition handoff record.");
                }

                RequireRuntimeState(profile, spec.RuntimeStateId);
            }
        }

        private static StageDefinitionProfile.CutsceneHandoffRef RequireHandoff(
            StageDefinitionProfile profile,
            string handoffId)
        {
            for (int i = 0; i < profile.CutsceneHandoffCount; i++)
            {
                StageDefinitionProfile.CutsceneHandoffRef handoff = profile.GetCutsceneHandoff(i);
                if (string.Equals(handoff.HandoffId, handoffId, StringComparison.Ordinal))
                {
                    return handoff;
                }
            }

            throw new InvalidOperationException($"{profile.StageId} is missing cutscene handoff {handoffId}.");
        }

        private static StageDefinitionProfile.RuntimeStateRef RequireRuntimeState(
            StageDefinitionProfile profile,
            string runtimeStateId)
        {
            for (int i = 0; i < profile.RuntimeStateCount; i++)
            {
                StageDefinitionProfile.RuntimeStateRef state = profile.GetRuntimeState(i);
                if (string.Equals(state.StateId, runtimeStateId, StringComparison.Ordinal))
                {
                    return state;
                }
            }

            throw new InvalidOperationException($"{profile.StageId} is missing runtime state {runtimeStateId}.");
        }

        private static void ValidateSourceReferences(StageDefinitionProfile profile)
        {
            bool hasPgr = false;
            bool hasNikke = false;
            bool hasHi3 = false;

            for (int i = 0; i < profile.SourceReferenceCount; i++)
            {
                StageDefinitionProfile.SourceReference source = profile.GetSourceReference(i);
                hasPgr |= source.SourceId.Contains("pgr", StringComparison.OrdinalIgnoreCase);
                hasNikke |= source.SourceId.Contains("nikke", StringComparison.OrdinalIgnoreCase);
                hasHi3 |= source.SourceId.Contains("hi3", StringComparison.OrdinalIgnoreCase);
            }

            if (!hasPgr || !hasNikke || !hasHi3)
            {
                throw new InvalidOperationException($"{profile.StageId} should preserve PGR, NIKKE, and HI3 source takeaways.");
            }
        }

        private static void SetAnchors(SerializedProperty property, AnchorSpec[] specs)
        {
            property.arraySize = specs.Length;
            for (int i = 0; i < specs.Length; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SetString(element, "anchorId", specs[i].AnchorId);
                SetString(element, "groupId", specs[i].GroupId);
                SetVector3(element, "expectedPosition", specs[i].Position);
                SetVector3(element, "expectedEuler", specs[i].Euler);
                SetString(element, "purpose", specs[i].Purpose);
            }
        }

        private static void SetSpawns(SerializedProperty property, SpawnSpec[] specs)
        {
            property.arraySize = specs.Length;
            for (int i = 0; i < specs.Length; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SetString(element, "spawnId", specs[i].SpawnId);
                element.FindPropertyRelative("spawnKind").enumValueIndex = (int)specs[i].SpawnKind;
                element.FindPropertyRelative("positionId").intValue = specs[i].PositionId;
                SetString(element, "anchorId", specs[i].AnchorId);
                SetString(element, "payloadId", specs[i].PayloadId);
                element.FindPropertyRelative("count").intValue = specs[i].Count;
                element.FindPropertyRelative("delaySeconds").floatValue = specs[i].DelaySeconds;
                SetString(element, "note", specs[i].Note);
            }
        }

        private static void SetHandoffs(SerializedProperty property, CutsceneHandoffSpec[] specs)
        {
            property.arraySize = specs.Length;
            for (int i = 0; i < specs.Length; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SetString(element, "handoffId", specs[i].HandoffId);
                SetString(element, "anchorId", specs[i].AnchorId);
                SetString(element, "cinematicProfileId", specs[i].CinematicProfileId);
                SetString(element, "timelineAssetPath", specs[i].TimelineAssetPath);
                SetString(element, "nextEventId", specs[i].NextEventId);
                SetString(element, "purpose", specs[i].Purpose);
            }
        }

        private static void SetRuntimeStates(SerializedProperty property, RuntimeStateSpec[] specs)
        {
            property.arraySize = specs.Length;
            for (int i = 0; i < specs.Length; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SetString(element, "stateId", specs[i].StateId);
                element.FindPropertyRelative("stateKind").enumValueIndex = (int)specs[i].StateKind;
                element.FindPropertyRelative("positionId").intValue = specs[i].PositionId;
                SetString(element, "anchorId", specs[i].AnchorId);
                SetString(element, "conditionId", specs[i].ConditionId);
                SetString(element, "note", specs[i].Note);
            }
        }

        private static void SetSourceReferences(SerializedProperty property, SourceReferenceSpec[] specs)
        {
            property.arraySize = specs.Length;
            for (int i = 0; i < specs.Length; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                SetString(element, "sourceId", specs[i].SourceId);
                SetString(element, "sourcePath", specs[i].SourcePath);
                SetString(element, "localTakeaway", specs[i].LocalTakeaway);
            }
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            serialized.FindProperty(propertyName).stringValue = value;
        }

        private static void SetVector3(SerializedObject serialized, string propertyName, Vector3 value)
        {
            serialized.FindProperty(propertyName).vector3Value = value;
        }

        private static void SetString(SerializedProperty parent, string propertyName, string value)
        {
            parent.FindPropertyRelative(propertyName).stringValue = value;
        }

        private static void SetVector3(SerializedProperty parent, string propertyName, Vector3 value)
        {
            parent.FindPropertyRelative(propertyName).vector3Value = value;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SetObjectReference(serializedObject, propertyName, value);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            RequireProperty(serializedObject, propertyName).objectReferenceValue = value;
        }

        private static void SetRelativeObjectReference(SerializedProperty property, string propertyName, UnityEngine.Object value)
        {
            property.FindPropertyRelative(propertyName).objectReferenceValue = value;
        }

        private static void SetRelativeEnum(SerializedProperty property, string propertyName, int value)
        {
            property.FindPropertyRelative(propertyName).enumValueIndex = value;
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            RequireProperty(serializedObject, propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static float GetFloat(SerializedObject serializedObject, string propertyName)
        {
            return RequireProperty(serializedObject, propertyName).floatValue;
        }

        private static Vector3 GetVector3(SerializedObject serializedObject, string propertyName)
        {
            return RequireProperty(serializedObject, propertyName).vector3Value;
        }

        private static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"{serializedObject.targetObject.GetType().Name} is missing serialized property {propertyName}.");
            }

            return property;
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if ((UnityEngine.Object)asset != null)
            {
                return asset;
            }

            string parent = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static T RequireComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"{gameObject.name} is missing required component {typeof(T).Name}.");
            }

            return component;
        }

        private static T RequireComponentByObjectName<T>(Transform root, string objectName) where T : Component
        {
            T component = FindComponentByObjectName<T>(root, objectName);
            if (component == null)
            {
                throw new InvalidOperationException($"{root.name} is missing {typeof(T).Name} on `{objectName}`.");
            }

            return component;
        }

        private static T FindComponentByObjectName<T>(Transform root, string objectName) where T : Component
        {
            Transform target = FindDescendantOrSelf(root, objectName);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static T LoadRequired<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if ((UnityEngine.Object)asset == null)
            {
                throw new InvalidOperationException($"Missing required asset at {assetPath}.");
            }

            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folder = System.IO.Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(folder))
            {
                throw new InvalidOperationException($"Invalid folder path: {path}");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static GameObject RequireRoot(Scene scene, string rootName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == rootName)
                {
                    return root;
                }
            }

            throw new InvalidOperationException($"Missing root object {rootName} in {scene.path}.");
        }

        private static Transform RequireChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException($"Missing child {childName} under {parent.name}.");
            }

            return child;
        }

        private static GameObject FindRootOrDescendant(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, objectName, StringComparison.Ordinal))
                {
                    return root;
                }

                Transform child = FindDescendant(root.transform, objectName);
                if (child != null)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static Transform RequireDescendantOrSelf(Transform root, string objectName)
        {
            Transform target = FindDescendantOrSelf(root, objectName);
            if (target == null)
            {
                throw new InvalidOperationException($"{root.name} is missing descendant `{objectName}`.");
            }

            return target;
        }

        private static Transform FindDescendantOrSelf(Transform root, string objectName)
        {
            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            return FindDescendant(root, objectName);
        }

        private static Transform FindDescendant(Transform parent, string objectName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (string.Equals(child.name, objectName, StringComparison.Ordinal))
                {
                    return child;
                }

                Transform match = FindDescendant(child, objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static GameObject RequireVisualInori(Transform visualRoot)
        {
            return RequireDescendantOrSelf(visualRoot, "IntroGatePodReview_Inori").gameObject;
        }

        private static Animator RequireVisualInoriAnimator(Transform visualRoot)
        {
            GameObject inori = RequireVisualInori(visualRoot);
            Animator animator = inori.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                throw new InvalidOperationException("Intro GatePod visual Inori has no Animator.");
            }

            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);
            return animator;
        }

        private static Renderer[] ResolveFirstPersonHiddenRenderers(GameObject inori)
        {
            Renderer[] renderers = inori.GetComponentsInChildren<Renderer>(includeInactive: true);
            List<Renderer> hiddenRenderers = new List<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && IsFirstPersonHeadOccluder(renderer.transform, inori.transform))
                {
                    hiddenRenderers.Add(renderer);
                }
            }

            return hiddenRenderers.ToArray();
        }

        private static bool IsFirstPersonHeadOccluder(Transform target, Transform inoriRoot)
        {
            for (Transform current = target; current != null; current = current.parent)
            {
                string name = current.name;
                if (name.IndexOf("Hair", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Head", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Face", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Brow", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Mouth", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (current == inoriRoot)
                {
                    break;
                }
            }

            return false;
        }

        private static CinemachineSplineDolly ResolveOpeningDolly(
            IntroGatePodCinemachineShotPlayer.Shot[] shots)
        {
            for (int i = 0; i < shots.Length; i++)
            {
                CinemachineCamera camera = shots[i].Camera;
                if (camera == null)
                {
                    continue;
                }

                CinemachineSplineDolly dolly = camera.GetComponent<CinemachineSplineDolly>();
                if (dolly != null)
                {
                    return dolly;
                }
            }

            throw new InvalidOperationException("Intro GatePod copied shots have no opening CinemachineSplineDolly.");
        }

        private static CinemachineCamera FindShotCamera(
            IntroGatePodCinemachineShotPlayer.Shot[] shots,
            string shotId)
        {
            for (int i = 0; i < shots.Length; i++)
            {
                if (string.Equals(shots[i].ShotId, shotId, StringComparison.Ordinal))
                {
                    return shots[i].Camera;
                }
            }

            return null;
        }

        private static T FindTimelineTrack<T>(TimelineAsset timeline, string trackName) where T : TrackAsset
        {
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is T typedTrack && string.Equals(track.name, trackName, StringComparison.Ordinal))
                {
                    return typedTrack;
                }
            }

            return null;
        }

        private static string SanitizeExposedReferenceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "shot";
            }

            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private static Transform FindDirectChild(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (string.Equals(child.name, childName, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static GameObject CreateChild(
            Transform parent,
            string childName,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = localRotation;
            child.transform.localScale = localScale;
            return child;
        }

        private static void RemoveChild(Transform parent, string childName)
        {
            Transform child = FindDirectChild(parent, childName);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private readonly struct AnchorSpec
        {
            public AnchorSpec(string anchorId, string groupId, Vector3 position, Vector3 euler, string purpose)
            {
                AnchorId = anchorId;
                GroupId = groupId;
                Position = position;
                Euler = euler;
                Purpose = purpose;
            }

            public readonly string AnchorId;
            public readonly string GroupId;
            public readonly Vector3 Position;
            public readonly Vector3 Euler;
            public readonly string Purpose;
        }

        private readonly struct SpawnSpec
        {
            public SpawnSpec(string spawnId, StageSpawnKind spawnKind, int positionId, string anchorId, string payloadId, int count, float delaySeconds, string note)
            {
                SpawnId = spawnId;
                SpawnKind = spawnKind;
                PositionId = positionId;
                AnchorId = anchorId;
                PayloadId = payloadId;
                Count = count;
                DelaySeconds = delaySeconds;
                Note = note;
            }

            public readonly string SpawnId;
            public readonly StageSpawnKind SpawnKind;
            public readonly int PositionId;
            public readonly string AnchorId;
            public readonly string PayloadId;
            public readonly int Count;
            public readonly float DelaySeconds;
            public readonly string Note;
        }

        private readonly struct CutsceneHandoffSpec
        {
            public CutsceneHandoffSpec(string handoffId, string anchorId, string cinematicProfileId, string timelineAssetPath, string nextEventId, string purpose)
            {
                HandoffId = handoffId;
                AnchorId = anchorId;
                CinematicProfileId = cinematicProfileId;
                TimelineAssetPath = timelineAssetPath;
                NextEventId = nextEventId;
                Purpose = purpose;
            }

            public readonly string HandoffId;
            public readonly string AnchorId;
            public readonly string CinematicProfileId;
            public readonly string TimelineAssetPath;
            public readonly string NextEventId;
            public readonly string Purpose;
        }

        private readonly struct RuntimeStateSpec
        {
            public RuntimeStateSpec(string stateId, StageRuntimeStateKind stateKind, int positionId, string anchorId, string conditionId, string note)
            {
                StateId = stateId;
                StateKind = stateKind;
                PositionId = positionId;
                AnchorId = anchorId;
                ConditionId = conditionId;
                Note = note;
            }

            public readonly string StateId;
            public readonly StageRuntimeStateKind StateKind;
            public readonly int PositionId;
            public readonly string AnchorId;
            public readonly string ConditionId;
            public readonly string Note;
        }

        private readonly struct CutscenePortSpec
        {
            public CutscenePortSpec(
                string portId,
                string rootName,
                StageCutscenePortKind portKind,
                string handoffId,
                string anchorId,
                string runtimeStateId,
                Vector3 position,
                Vector3 euler,
                string purpose)
            {
                PortId = portId;
                RootName = rootName;
                PortKind = portKind;
                HandoffId = handoffId;
                AnchorId = anchorId;
                RuntimeStateId = runtimeStateId;
                Position = position;
                Euler = euler;
                Purpose = purpose;
            }

            public readonly string PortId;
            public readonly string RootName;
            public readonly StageCutscenePortKind PortKind;
            public readonly string HandoffId;
            public readonly string AnchorId;
            public readonly string RuntimeStateId;
            public readonly Vector3 Position;
            public readonly Vector3 Euler;
            public readonly string Purpose;
        }

        private readonly struct SourceReferenceSpec
        {
            public SourceReferenceSpec(string sourceId, string sourcePath, string localTakeaway)
            {
                SourceId = sourceId;
                SourcePath = sourcePath;
                LocalTakeaway = localTakeaway;
            }

            public readonly string SourceId;
            public readonly string SourcePath;
            public readonly string LocalTakeaway;
        }

        private readonly struct CinematicStageBindingSpec
        {
            public CinematicStageBindingSpec(
                string cinematicProfilePath,
                string cinematicProfileId,
                string handoffId,
                string anchorId,
                string runtimeStateId,
                string note)
            {
                CinematicProfilePath = cinematicProfilePath;
                CinematicProfileId = cinematicProfileId;
                HandoffId = handoffId;
                AnchorId = anchorId;
                RuntimeStateId = runtimeStateId;
                Note = note;
            }

            public readonly string CinematicProfilePath;
            public readonly string CinematicProfileId;
            public readonly string HandoffId;
            public readonly string AnchorId;
            public readonly string RuntimeStateId;
            public readonly string Note;
        }
    }
}
