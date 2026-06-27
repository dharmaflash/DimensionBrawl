using System;
using System.Collections.Generic;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class ActionFoundationOlympusStageDefinitionSetup
    {
        private const string DefinitionRoot = "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions";
        private const string DefinitionPath = DefinitionRoot + "/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string StageScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string SourceScenePath = "Assets/_Game/Scenes/Lookdev/OlympusCorridorInvasionLookdev.unity";
        private const string StageRootName = "OlympusCorridorStageRoot";
        private const string StageMapRootName = "OlympusCorridorStageMap";
        private const string StageAnchorsName = "OlympusCorridorStageAnchors";
        private const string CombatAnchorsName = "CombatSpawnAnchors";
        private const string CutsceneAnchorsName = "CutsceneHandoffAnchors";
        private const string RuntimeAnchorsName = "RuntimeStateAnchors";
        private const string CinematicProfileRoot = "Assets/_Game/DesignData/Profiles/Cinematics";
        private const string IntroGatePodProfilePath = CinematicProfileRoot + "/DB_Cinematic_IntroGatePodAwakening.asset";
        private const string BossIntroProfilePath = CinematicProfileRoot + "/DB_Cinematic_BossIntro.asset";
        private const string GameplayHandoffProfilePath = CinematicProfileRoot + "/DB_Cinematic_GameplayHandoff.asset";
        private const float StageScale = 1.5f;

        private static readonly AnchorSpec[] AnchorSpecs =
        {
            new("Player_LeftShoulderCameraAnchor", CombatAnchorsName, new Vector3(-16.5f, 1.8f, -4.65f), new Vector3(0f, 82f, 0f), "Player camera/start read for intro handoff and combat entry."),
            new("Boss_CenterLaneAnchor", CombatAnchorsName, new Vector3(15.3f, 0f, 0f), Vector3.zero, "Boss center spawn and reveal focus."),
            new("Add_LeftLaneAnchor", CombatAnchorsName, new Vector3(13.35f, 0f, -1.875f), Vector3.zero, "Left add spawn lane."),
            new("Add_RightLaneAnchor", CombatAnchorsName, new Vector3(13.35f, 0f, 1.875f), Vector3.zero, "Right add spawn lane."),
            new("Rift_BackdropAnchor", CombatAnchorsName, new Vector3(22.2f, 3.975f, 0f), Vector3.zero, "Far rift/backdrop spatial reference."),
            new("IntroCutscene_End_PlayerHandoffAnchor", CutsceneAnchorsName, new Vector3(-16.5f, 1.8f, -4.65f), new Vector3(0f, 82f, 0f), "Intro cutscene exits into this player-side view."),
            new("BossEntrance_BossRevealAnchor", CutsceneAnchorsName, new Vector3(15.3f, 1.6f, 0f), Vector3.zero, "Boss entrance reveal look/actor anchor."),
            new("Gameplay_CombatStartAnchor", CutsceneAnchorsName, new Vector3(-16.5f, 0f, -4.65f), new Vector3(0f, 82f, 0f), "Gameplay camera/input unlock handoff."),
            new("StageSpawner_PlayerStart", RuntimeAnchorsName, new Vector3(-16.5f, 0f, -4.65f), new Vector3(0f, 82f, 0f), "Runtime PositionId for player start."),
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

            for (int i = 0; i < AnchorSpecs.Length; i++)
            {
                AnchorSpec spec = AnchorSpecs[i];
                Transform group = RequireChild(stageAnchors, spec.GroupId);
                Transform anchor = RequireChild(group, spec.AnchorId);
                StageAnchorPoint point = GetOrAddComponent<StageAnchorPoint>(anchor.gameObject);
                ConfigureAnchorPoint(point, spec);
                points[i] = point;
            }

            binding.Configure(profile, mapRoot, points);
            if (binding.StageDefinition == null)
            {
                throw new InvalidOperationException($"{profile.StageId} scene binding failed to retain the StageDefinition profile reference.");
            }

            EditorUtility.SetDirty(binding);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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
