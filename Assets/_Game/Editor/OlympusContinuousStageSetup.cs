using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.Editor
{
    public static class OlympusContinuousStageSetup
    {
        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StationScenePath =
            "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string RoutePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";
        private const string CorridorDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string StationDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusStationCombat.asset";
        private const string ProgressionNodePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusInvasion.asset";
        private const string ProgressionGraphPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusInvasion.asset";
        private const string ResultDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusInvasion.asset";
        private const string StageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string GuidePrefabPath =
            "Assets/_Game/UI/Transitions/PF_UI_SceneEntryNoticeOverlay.prefab";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string PlayerInputActionMapId = "df70fa95-8a34-4494-b137-73ab6b9c7d37";
        internal const string StationBossTerminalFinisherTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_OlympusStationBossTerminalFinisher.playable";
        internal const string StationBossTerminalFinisherCameraClipPath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Anim_OlympusStationBossTerminalFinisherCamera.anim";
        private const string StationBossTerminalFinisherCameraTrackName =
            "Boss Terminal Finisher Camera";

        private const string SharedMapName = "OlympusCorridorStageMap";
        private const string FlowRootName = "OlympusCorridor_CombatFlowRoot";
        private const string CombatPackageRootName = "OlympusCorridor_BossBarrageCombatPackage";
        private const string LowerCombatPlacementName = "OlympusCorridor_CombatPocketPlacement";
        private const string LowerCombatRuntimeRootName = "OlympusStation_LowerCombatRuntimeRoot";
        private const string LowerPlayerStartAnchorName = "PlayerStartAnchor";
        private const string AddLeftAnchorPath =
            "OlympusCorridorInvasionLookdev_RuntimeFreePass/"
            + "OlympusCorridor_CombatReadAnchors/Add_LeftLaneAnchor";
        private const string AddRightAnchorPath =
            "OlympusCorridorInvasionLookdev_RuntimeFreePass/"
            + "OlympusCorridor_CombatReadAnchors/Add_RightLaneAnchor";
        private const string ResultAdaptersName = "OlympusStation_ResultAdapters";
        private const string EntryGuideName = "OlympusStation_EntryGuide";
        internal const string StationBossTerminalFinisherCameraRigName =
            "OlympusStation_BossTerminalFinisherCameraRig";
        internal const string StationTerminalBoundaryVisualName =
            "OlympusStation_NoCrossCenterLine";
        internal const string StationPocketClearMarkerName =
            "BossBarrageLaneReview_PocketClearMarker";
        internal const string StationBossDisplayName = "AKAZA";
        internal const float StationBossTerminalFinisherDurationSeconds = 2.6f;
        internal const float StationBossTerminalFinisherSettleSeconds = 0.14f;
        internal const float StationBossTerminalFinisherFieldOfView = 44f;
        internal static readonly Vector3 StationBossTerminalFinisherStartLocalPosition =
            new(0f, 1.45f, 5.35f);
        internal static readonly Vector3 StationBossTerminalFinisherSettleLocalPosition =
            new(0f, 1.40f, 5.60f);
        internal static readonly Vector3 StationBossTerminalFinisherStartLookTarget =
            new(0f, -0.40f, 0f);
        internal static readonly Vector3 StationBossTerminalFinisherSettleLookTarget =
            new(0f, -0.78f, 0f);
        private const string StairTraversalSupportName =
            "OlympusCorridor_IntroStairTraversalSupport";
        private const string UpperLandingTraversalSupportName =
            "OlympusCorridor_IntroUpperLandingTraversalSupport";
        private const string StairEntryAnchorName = "OlympusCorridor_StairEntryAnchor";
        private const string StairTriggerName = "OlympusCorridor_StairToCorridorCombatTrigger";
        private const float TraversalSupportWidth = 7.25f;
        private const float TraversalSupportThickness = 0.18f;
        private const float StairCrestDistanceFromEntry = 24.25f;
        private static readonly Vector3 SourceStationPlayerStartPosition = new Vector3(0f, 0f, -8.5f);

        [MenuItem("DimensionBrawl/Setup/Build Olympus Tutorial To Station Scene Route")]
        public static void BuildFromMenu()
        {
            Build();
            Debug.Log("[OlympusContinuousStageSetup] Tutorial-to-Station scene route setup complete.");
        }

        public static void RunBatchSetup()
        {
            try
            {
                Build();
                Debug.Log("[OlympusContinuousStageSetup] BATCH_SETUP_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[OlympusContinuousStageSetup] BATCH_SETUP_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void RunBatchSpatialAudit()
        {
            try
            {
                SpatialSnapshot[] stationSnapshots = CaptureSpatialSnapshots(StationScenePath);
                SpatialSnapshot[] corridorSnapshots = CaptureSpatialSnapshots(CorridorScenePath);
                var corridorByName = new Dictionary<string, SpatialSnapshot>(StringComparer.Ordinal);
                for (int i = 0; i < corridorSnapshots.Length; i++)
                {
                    corridorByName[corridorSnapshots[i].Name] = corridorSnapshots[i];
                }

                Debug.Log("[OlympusContinuousStageSetup] SPATIAL_AUDIT_BEGIN");
                for (int i = 0; i < stationSnapshots.Length; i++)
                {
                    SpatialSnapshot station = stationSnapshots[i];
                    if (!corridorByName.TryGetValue(station.Name, out SpatialSnapshot corridor))
                    {
                        Debug.Log($"[OlympusContinuousStageSetup] {station.Name}: missing in Corridor.");
                        continue;
                    }

                    Debug.Log(
                        $"[OlympusContinuousStageSetup] {station.Name}: "
                        + $"stationWorld={FormatVector(station.WorldPosition)}, "
                        + $"stationMapLocal={FormatVector(station.MapLocalPosition)}, "
                        + $"mappedCorridorWorld={FormatVector(corridor.MapRootLocalToWorld.MultiplyPoint3x4(station.MapLocalPosition))}, "
                        + $"corridorWorld={FormatVector(corridor.WorldPosition)}, "
                        + $"corridorMapLocal={FormatVector(corridor.MapLocalPosition)}, "
                        + $"worldDelta={Vector3.Distance(corridor.WorldPosition, corridor.MapRootLocalToWorld.MultiplyPoint3x4(station.MapLocalPosition)):0.###}, "
                        + $"stationRotation={FormatQuaternion(station.WorldRotation)}, "
                        + $"corridorRotation={FormatQuaternion(corridor.WorldRotation)}.");
                }

                Debug.Log("[OlympusContinuousStageSetup] SPATIAL_AUDIT_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[OlympusContinuousStageSetup] SPATIAL_AUDIT_FAIL");
                EditorApplication.Exit(1);
            }
        }

        private readonly struct SpatialSnapshot
        {
            public SpatialSnapshot(
                string name,
                Vector3 worldPosition,
                Quaternion worldRotation,
                Vector3 mapLocalPosition,
                Matrix4x4 mapRootLocalToWorld)
            {
                Name = name;
                WorldPosition = worldPosition;
                WorldRotation = worldRotation;
                MapLocalPosition = mapLocalPosition;
                MapRootLocalToWorld = mapRootLocalToWorld;
            }

            public string Name { get; }
            public Vector3 WorldPosition { get; }
            public Quaternion WorldRotation { get; }
            public Vector3 MapLocalPosition { get; }
            public Matrix4x4 MapRootLocalToWorld { get; }
        }

        private static readonly string[] SpatialAuditObjectNames =
        {
            SharedMapName,
            "Player_CombatGirl_ActionFoundation",
            "CombatEncounter",
            "BossBarrageLaneReview_PocketOwner",
            "BossBarrageLaneReview_BossProxy_NeedleLock",
            "BossBarrageLaneReview_SummonLaneSpace",
            "BossBarrageLaneReview_Markers",
            "Boss_CenterLaneAnchor",
            "StageSpawner_PlayerStart"
        };

        private static SpatialSnapshot[] CaptureSpatialSnapshots(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Transform mapRoot = FindRequiredSceneObject(scene, SharedMapName).transform;
            var snapshots = new List<SpatialSnapshot>(SpatialAuditObjectNames.Length);
            for (int i = 0; i < SpatialAuditObjectNames.Length; i++)
            {
                GameObject candidate = FindSceneObject(scene, SpatialAuditObjectNames[i]);
                if (candidate == null)
                {
                    continue;
                }

                Transform transform = candidate.transform;
                snapshots.Add(new SpatialSnapshot(
                    SpatialAuditObjectNames[i],
                    transform.position,
                    transform.rotation,
                    mapRoot.InverseTransformPoint(transform.position),
                    mapRoot.localToWorldMatrix));
            }

            return snapshots.ToArray();
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x:0.###},{value.y:0.###},{value.z:0.###})";
        }

        private static string FormatQuaternion(Quaternion value)
        {
            Vector3 euler = value.eulerAngles;
            return $"({euler.x:0.###},{euler.y:0.###},{euler.z:0.###})";
        }

        private static void Build()
        {
            PlayableStageDefinition route = LoadRequired<PlayableStageDefinition>(RoutePath);
            StageDefinitionProfile corridorDefinition =
                LoadRequired<StageDefinitionProfile>(CorridorDefinitionPath);
            StageDefinitionProfile stationDefinition =
                LoadRequired<StageDefinitionProfile>(StationDefinitionPath);
            StageProgressionNode progressionNode =
                LoadRequired<StageProgressionNode>(ProgressionNodePath);
            StageProgressionGraph progressionGraph =
                LoadRequired<StageProgressionGraph>(ProgressionGraphPath);
            StageResultDefinition resultDefinition =
                LoadRequired<StageResultDefinition>(ResultDefinitionPath);
            UIStageCatalog stageCatalog = LoadRequired<UIStageCatalog>(StageCatalogPath);

            ConfigureCanonicalAssets(
                route,
                corridorDefinition,
                stationDefinition,
                progressionNode,
                progressionGraph,
                resultDefinition,
                stageCatalog);
            string routeDigest = route.CanonicalRouteDigest;
            string referenceDigest = route.ReferenceBlock.CanonicalReferenceDigest;
            string briefingDigest = route.ReferenceBlock.CanonicalBriefingDigest;
            string nodeBindingDigest = progressionNode.BindingDigest;
            string graphDigest = progressionGraph.CanonicalDigest;
            string joinDigest = route.ResultProgressionJoin.CanonicalDigest;

            SaveAsset(route);
            SaveAsset(corridorDefinition);
            SaveAsset(stationDefinition);
            SaveAsset(progressionNode);
            SaveAsset(progressionGraph);
            SaveAsset(stageCatalog);
            ConfigureSeparatedRouteScenes(route, stationDefinition);

            Debug.Log(
                "[OlympusContinuousStageSetup] "
                + $"route={routeDigest}, "
                + $"reference={referenceDigest}, "
                + $"briefing={briefingDigest}, "
                + $"nodeBinding={nodeBindingDigest}, "
                + $"graph={graphDigest}, "
                + $"join={joinDigest}.");
        }

        private static void ConfigureCanonicalAssets(
            PlayableStageDefinition route,
            StageDefinitionProfile corridorDefinition,
            StageDefinitionProfile stationDefinition,
            StageProgressionNode progressionNode,
            StageProgressionGraph progressionGraph,
            StageResultDefinition resultDefinition,
            UIStageCatalog stageCatalog)
        {
            var station = new SerializedObject(stationDefinition);
            station.FindProperty("mapScenePath").stringValue = StationScenePath;
            station.FindProperty("mapRootName").stringValue = SharedMapName;
            station.FindProperty("mapContentRootName").stringValue = SharedMapName;
            station.FindProperty("mapScale").vector3Value = new Vector3(1.8f, 1.8f, 2.3f);
            ConfigureStationBossOnlyDefinition(station);
            SerializedProperty stationSources = station.FindProperty("sourceReferences");
            Require(stationSources != null && stationSources.arraySize == 1,
                "Station definition must have one provenance row.");
            stationSources.GetArrayElementAtIndex(0)
                .FindPropertyRelative("sourcePath").stringValue = StationScenePath;
            stationSources.GetArrayElementAtIndex(0)
                .FindPropertyRelative("localTakeaway").stringValue =
                "This terminal combat segment is authored and loaded from the dedicated Olympus Station scene.";
            station.ApplyModifiedPropertiesWithoutUndo();

            var corridor = new SerializedObject(corridorDefinition);
            corridor.FindProperty("clearCondition").stringValue =
                "Completing the final tutorial step seals the run handoff and immediately loads the dedicated Station combat scene.";
            corridor.FindProperty("excludedScope").stringValue =
                "Station guide, boss terminal authority, result UI, enemy balance, reward payout, and persistent progress remain separately owned.";
            corridor.ApplyModifiedPropertiesWithoutUndo();

            var serializedRoute = new SerializedObject(route);
            serializedRoute.FindProperty("routeRevision").intValue = 1;
            SerializedProperty segments = serializedRoute.FindProperty("sceneSegments");
            Require(segments != null && segments.arraySize == 2,
                "Olympus route must retain exactly two ordered logical segments.");
            SerializedProperty corridorSegment = segments.GetArrayElementAtIndex(0);
            corridorSegment.FindPropertyRelative("exitConditionId").stringValue =
                "corridor.tutorial.completed";
            corridorSegment.FindPropertyRelative("exitConditionKind").intValue =
                (int)StageSegmentConditionKind
                    .CorridorTutorialFactsAndClosureSealedForSingleLoad;
            corridorSegment.FindPropertyRelative("handoffPolicy").intValue =
                (int)StageSceneHandoffPolicy.SingleLoad;
            corridorSegment.FindPropertyRelative("successorKind").intValue =
                (int)StageSegmentSuccessorKind.NextOrderedSegment;
            corridorSegment.FindPropertyRelative("destinationSceneKind").intValue =
                (int)StageSegmentDestinationSceneKind.SuccessorStageDefinitionScene;
            corridorSegment.FindPropertyRelative("transitionTokenKind").intValue =
                (int)StageSegmentTransitionTokenKind.SealedCurrentRunSegmentHandoff;
            corridorSegment.FindPropertyRelative("loaderGenerationKind").intValue =
                (int)StageSegmentLoaderGenerationKind.ActiveRunRouteLoaderGeneration;
            corridorSegment.FindPropertyRelative("navigationAuthorityKind").intValue =
                (int)StageSegmentNavigationAuthorityKind.P1AStageRunRouteOwner;
            corridorSegment.FindPropertyRelative("returnOwnerKind").intValue =
                (int)StageSegmentReturnOwnerKind.None;
            corridorSegment.FindPropertyRelative("returnOwnerReceiptPolicy").intValue =
                (int)StageReturnOwnerReceiptPolicy.None;

            SerializedProperty stationSegment = segments.GetArrayElementAtIndex(1);
            stationSegment.FindPropertyRelative("entryConditionId").stringValue =
                "corridor.tutorial.completed";
            stationSegment.FindPropertyRelative("entryConditionKind").intValue =
                (int)StageSegmentConditionKind
                    .CorridorTutorialFactsAndClosureSealedForSingleLoad;

            SerializedProperty reference = serializedRoute.FindProperty("referenceBlock");
            reference.FindPropertyRelative("revision").intValue = 1;
            reference.FindPropertyRelative("briefingRevision").intValue = 1;
            SerializedProperty join = serializedRoute.FindProperty("resultProgressionJoin");
            join.FindPropertyRelative("revision").intValue = 1;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            SetString(serializedRoute, "canonicalRouteDigest", route.ComputeCanonicalRouteDigest());
            SetRelativeString(
                serializedRoute,
                "referenceBlock",
                "canonicalReferenceDigest",
                route.ComputeCanonicalReferenceDigest());
            Require(
                route.TryComputeCanonicalBriefingDigest(
                    out string briefingDigest,
                    out StageBriefingBuildRejectReason briefingReject),
                $"Briefing digest computation failed: {briefingReject}.");
            SetRelativeString(
                serializedRoute,
                "referenceBlock",
                "canonicalBriefingDigest",
                briefingDigest);

            var serializedGraph = new SerializedObject(progressionGraph);
            serializedGraph.FindProperty("revision").intValue = 1;
            serializedGraph.ApplyModifiedPropertiesWithoutUndo();

            var serializedNode = new SerializedObject(progressionNode);
            serializedNode.FindProperty("bindingRevision").intValue = 1;
            serializedNode.FindProperty("routeRevision").intValue = route.RouteRevision;
            serializedNode.FindProperty("canonicalRouteDigest").stringValue =
                route.CanonicalRouteDigest;
            serializedNode.FindProperty("progressionGraphRevision").intValue = 1;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();
            Require(
                progressionNode.TryComputeCanonicalDigests(
                    out string nodeContentDigest,
                    out string nodeBindingDigest,
                    out string nodeError),
                $"Progression node digest computation failed: {nodeError}");
            serializedNode.Update();
            serializedNode.FindProperty("contentDigest").stringValue = nodeContentDigest;
            serializedNode.FindProperty("bindingDigest").stringValue = nodeBindingDigest;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();

            Require(
                progressionGraph.TryComputeCanonicalDigest(
                    out string graphDigest,
                    out string graphError),
                $"Progression graph digest computation failed: {graphError}");
            SetString(serializedGraph, "canonicalDigest", graphDigest);

            Require(
                resultDefinition.TryComputeCanonicalDigests(
                    out string resultEvaluationDigest,
                    out string resultBindingDigest,
                    out string resultSourceDigest,
                    out string resultError),
                $"Result definition digest computation failed: {resultError}");
            Require(
                string.Equals(resultDefinition.EvaluationContentDigest, resultEvaluationDigest,
                    StringComparison.Ordinal)
                && string.Equals(resultDefinition.PresentationBindingDigest, resultBindingDigest,
                    StringComparison.Ordinal)
                && string.Equals(resultDefinition.PresentationSourceDigest, resultSourceDigest,
                    StringComparison.Ordinal),
                "Route migration must not mutate the result-definition cohort.");

            Require(
                route.TryComputeResultProgressionJoinDigest(
                    out string joinDigest,
                    out string joinError),
                $"Result/progression join digest computation failed: {joinError}");
            SetRelativeString(
                serializedRoute,
                "resultProgressionJoin",
                "canonicalDigest",
                joinDigest);

            var serializedCatalog = new SerializedObject(stageCatalog);
            SerializedProperty projectionGeneration =
                serializedCatalog.FindProperty("catalogProjectionGeneration");
            Require(
                projectionGeneration != null
                    && projectionGeneration.intValue
                        >= UIStageCatalog.InitialCatalogProjectionGeneration,
                "Stage catalog projection generation must remain valid.");
            projectionGeneration.intValue = UIStageCatalog.InitialCatalogProjectionGeneration;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            serializedCatalog.Update();
            SerializedProperty catalogStages = serializedCatalog.FindProperty("stages");
            Require(
                catalogStages != null
                    && catalogStages.arraySize == stageCatalog.StageCount
                    && stageCatalog.TryValidateEntryIdentities(out _),
                "Stage catalog must retain one or more uniquely identified entries.");
            for (int i = 0; i < stageCatalog.StageCount; i++)
            {
                Require(
                    stageCatalog.TryComputeCanonicalProjectionDigest(
                        i,
                        UIRouteId.Combat,
                        out string projectionDigest,
                        out UIStageRouteProjectionRejectReason projectionReject),
                    $"Stage catalog projection computation failed at row {i}: {projectionReject}.");
                catalogStages.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("canonicalProjectionDigest").stringValue = projectionDigest;
            }
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

            Require(StageRunRouteSnapshot.TryCreate(route, out _, out string routeError), routeError);
            Require(
                StageRunResultProgressionJoinSnapshot.TryCreate(route, out _, out string joinBuildError),
                joinBuildError);
            Require(
                stageCatalog.TryCreateRouteProjection(
                    0,
                    UIRouteId.Combat,
                    out _,
                    out UIStageRouteProjectionRejectReason projectionBuildReject),
                $"Stage catalog projection validation failed: {projectionBuildReject}.");
        }

        private static void ConfigureStationBossOnlyDefinition(SerializedObject station)
        {
            SerializedProperty anchors = station.FindProperty("anchors");
            Require(anchors != null, "Station definition is missing its anchor array.");
            anchors.arraySize = 0;

            SerializedProperty spawns = station.FindProperty("spawns");
            Require(spawns != null, "Station definition is missing its spawn array.");
            spawns.arraySize = 0;
        }

        private static void ConfigureSeparatedRouteScenes(
            PlayableStageDefinition route,
            StageDefinitionProfile stationDefinition)
        {
            Scene corridorScene = EditorSceneManager.OpenScene(
                CorridorScenePath,
                OpenSceneMode.Single);
            StageCountOneEncounterExecutor[] retiredCorridorExecutors =
                FindSceneComponents<StageCountOneEncounterExecutor>(corridorScene);
            for (int i = 0; i < retiredCorridorExecutors.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(retiredCorridorExecutors[i]);
            }

            OlympusCorridorCombatFlowController flow =
                FindSingleSceneComponent<OlympusCorridorCombatFlowController>(corridorScene);
            var serializedFlow = new SerializedObject(flow);
            serializedFlow.FindProperty("playableStageDefinition").objectReferenceValue = route;
            serializedFlow.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(corridorScene);
            Require(
                EditorSceneManager.SaveScene(corridorScene),
                "Failed to save the Corridor source scene.");

            Scene stationScene = EditorSceneManager.OpenScene(
                StationScenePath,
                OpenSceneMode.Single);
            GameObject stationMap = FindRequiredSceneObject(stationScene, SharedMapName);
            Transform addLeftTransform = stationMap.transform.Find(AddLeftAnchorPath);
            Transform addRightTransform = stationMap.transform.Find(AddRightAnchorPath);
            Require(addLeftTransform != null,
                $"Missing Station left Add anchor: {AddLeftAnchorPath}");
            Require(addRightTransform != null,
                $"Missing Station right Add anchor: {AddRightAnchorPath}");

            StageCountOneEncounterExecutor[] executors =
                FindSceneComponents<StageCountOneEncounterExecutor>(stationScene);
            for (int i = 0; i < executors.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(executors[i]);
            }

            StageDefinitionSceneBinding[] stationBindings =
                FindSceneComponents<StageDefinitionSceneBinding>(stationScene);
            StageDefinitionSceneBinding stationBinding =
                stationMap.GetComponent<StageDefinitionSceneBinding>();
            for (int i = 0; i < stationBindings.Length; i++)
            {
                if (!ReferenceEquals(stationBindings[i], stationBinding))
                {
                    UnityEngine.Object.DestroyImmediate(stationBindings[i]);
                }
            }

            stationBinding ??= stationMap.AddComponent<StageDefinitionSceneBinding>();
            stationBinding.Configure(
                stationDefinition,
                stationMap.transform,
                Array.Empty<StageAnchorPoint>(),
                Array.Empty<StageCutscenePort>());

            StageAnchorPoint[] stationAnchors =
                FindSceneComponents<StageAnchorPoint>(stationScene);
            for (int i = 0; i < stationAnchors.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(stationAnchors[i]);
            }

            GameObject entryGuide = FindSceneObject(stationScene, "SceneEntryNoticeOverlay")
                ?? FindSceneObject(stationScene, EntryGuideName);
            if (entryGuide == null)
            {
                GameObject guidePrefab = LoadRequired<GameObject>(GuidePrefabPath);
                entryGuide = (GameObject)PrefabUtility.InstantiatePrefab(guidePrefab, stationScene);
            }

            OlympusStationCombatIntroTutorialBridge[] guideBridges =
                FindSceneComponents<OlympusStationCombatIntroTutorialBridge>(stationScene);
            for (int i = 0; i < guideBridges.Length; i++)
            {
                GameObject guideRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(
                    guideBridges[i].gameObject) ?? guideBridges[i].gameObject;
                if (guideRoot != entryGuide)
                {
                    UnityEngine.Object.DestroyImmediate(guideRoot);
                }
            }

            entryGuide.name = EntryGuideName;
            Require(
                entryGuide.GetComponent<OlympusStationCombatIntroTutorialBridge>() != null,
                "Station entry guide prefab is missing its runtime bridge.");
            entryGuide.SetActive(true);

            ConfigureStationBossTerminalAftermath(stationScene);

            Require(
                FindSingleSceneComponent<OlympusStationCombatIntroTutorialBridge>(stationScene) != null,
                "Station scene must retain exactly one canonical entry guide.");
            Require(
                FindSingleSceneComponent<BossBarrageEncounterController>(stationScene) != null,
                "Station scene must retain exactly one authored boss encounter.");
            Require(
                FindSingleSceneComponent<OlympusStationCombatResultPresenter>(stationScene) != null,
                "Station scene is missing its canonical result presenter.");
            Require(
                FindSingleSceneComponent<OlympusStationRunFactCollector>(stationScene) != null,
                "Station scene is missing its run fact collector.");
            Require(
                FindSingleSceneComponent<OlympusStationBossTerminalAftermathPresenter>(stationScene) != null,
                "Station scene is missing its boss-terminal aftermath gate.");

            EditorSceneManager.MarkSceneDirty(stationScene);
            Require(
                EditorSceneManager.SaveScene(stationScene),
                "Failed to save the dedicated Station combat scene.");
        }

        private static void ConfigureCorridorScene(
            PlayableStageDefinition route,
            StageDefinitionProfile stationDefinition)
        {
            Scene scene = EditorSceneManager.OpenScene(CorridorScenePath, OpenSceneMode.Single);
            GameObject sharedMap = FindRequiredSceneObject(scene, SharedMapName);
            OlympusCorridorCombatFlowController flow =
                FindSingleSceneComponent<OlympusCorridorCombatFlowController>(scene);
            Transform combatPackageRoot =
                FindRequiredSceneObject(scene, CombatPackageRootName).transform;
            Transform lowerCombatPlacement =
                FindRequiredSceneObject(scene, LowerCombatPlacementName).transform;
            Transform lowerPlayerStartAnchor =
                FindRequiredSceneObject(scene, LowerPlayerStartAnchorName).transform;
            GameObject stairTraversalSupport =
                FindRequiredSceneObject(scene, StairTraversalSupportName);
            Transform stairEntryAnchor =
                FindRequiredSceneObject(scene, StairEntryAnchorName).transform;
            Transform stairTrigger =
                FindRequiredSceneObject(scene, StairTriggerName).transform;
            stairTrigger.SetPositionAndRotation(
                lowerPlayerStartAnchor.position,
                lowerPlayerStartAnchor.rotation);
            Vector3 routeDirection = Vector3.ProjectOnPlane(
                stairTrigger.position - stairEntryAnchor.position,
                Vector3.up).normalized;
            Require(routeDirection.sqrMagnitude > 0.99f,
                "Continuous traversal route requires distinct stair entry and trigger positions.");

            Transform supportParent = stairTraversalSupport.transform.parent;
            Require(supportParent != null, "Continuous stair traversal support requires an authored parent.");
            GameObject upperLandingSupport = FindSceneObject(scene, UpperLandingTraversalSupportName);
            if (upperLandingSupport == null)
            {
                upperLandingSupport = new GameObject(UpperLandingTraversalSupportName);
                upperLandingSupport.transform.SetParent(supportParent, worldPositionStays: true);
                upperLandingSupport.AddComponent<BoxCollider>();
            }

            Vector3 stairCrest = stairEntryAnchor.position
                + routeDirection * StairCrestDistanceFromEntry;
            stairCrest.y = combatPackageRoot.position.y;
            Vector3 upperBridgeStart = stairEntryAnchor.position - routeDirection * 2f;
            upperBridgeStart.y = combatPackageRoot.position.y;
            Vector3 upperBridgeEnd = stairCrest + routeDirection * 0.4f;
            upperBridgeEnd.y = combatPackageRoot.position.y;
            ConfigureTraversalSupport(upperLandingSupport, upperBridgeStart, upperBridgeEnd);

            Vector3 lowerRampStart = stairCrest - routeDirection * 0.4f;
            lowerRampStart.y = combatPackageRoot.position.y;
            Vector3 lowerRampEnd = stairTrigger.position + routeDirection;
            lowerRampEnd.y = lowerCombatPlacement.position.y;
            ConfigureTraversalSupport(stairTraversalSupport, lowerRampStart, lowerRampEnd);
            Transform addLeftAnchorTransform = sharedMap.transform.Find(AddLeftAnchorPath);
            Transform addRightAnchorTransform = sharedMap.transform.Find(AddRightAnchorPath);
            Require(addLeftAnchorTransform != null,
                $"Missing canonical left Station Add anchor path under {SharedMapName}: {AddLeftAnchorPath}");
            Require(addRightAnchorTransform != null,
                $"Missing canonical right Station Add anchor path under {SharedMapName}: {AddRightAnchorPath}");

            StageAnchorPoint addLeftAnchor =
                addLeftAnchorTransform.GetComponent<StageAnchorPoint>()
                ?? addLeftAnchorTransform.gameObject.AddComponent<StageAnchorPoint>();
            var serializedLeftAnchor = new SerializedObject(addLeftAnchor);
            serializedLeftAnchor.FindProperty("anchorId").stringValue = "Add_LeftLaneAnchor";
            serializedLeftAnchor.FindProperty("groupId").stringValue = "CombatSpawnAnchors";
            serializedLeftAnchor.FindProperty("usageKind").intValue =
                (int)StageAnchorUsageKind.CombatSpawn;
            serializedLeftAnchor.FindProperty("positionId").intValue = 2101;
            serializedLeftAnchor.FindProperty("spawnKind").intValue = (int)StageSpawnKind.Add;
            serializedLeftAnchor.FindProperty("runtimeStateKind").intValue = 0;
            serializedLeftAnchor.FindProperty("purpose").stringValue =
                "First source-ordered Station Add ticket in the continuous Corridor host.";
            serializedLeftAnchor.ApplyModifiedPropertiesWithoutUndo();

            StageAnchorPoint addRightAnchor =
                addRightAnchorTransform.GetComponent<StageAnchorPoint>()
                ?? addRightAnchorTransform.gameObject.AddComponent<StageAnchorPoint>();
            var serializedRightAnchor = new SerializedObject(addRightAnchor);
            serializedRightAnchor.FindProperty("anchorId").stringValue = "Add_RightLaneAnchor";
            serializedRightAnchor.FindProperty("groupId").stringValue = "CombatSpawnAnchors";
            serializedRightAnchor.FindProperty("usageKind").intValue =
                (int)StageAnchorUsageKind.CombatSpawn;
            serializedRightAnchor.FindProperty("positionId").intValue = 2102;
            serializedRightAnchor.FindProperty("spawnKind").intValue = (int)StageSpawnKind.Add;
            serializedRightAnchor.FindProperty("runtimeStateKind").intValue = 0;
            serializedRightAnchor.FindProperty("purpose").stringValue =
                "Second mirrored Station Add ticket with source-order activation.";
            serializedRightAnchor.ApplyModifiedPropertiesWithoutUndo();

            StageDefinitionSceneBinding stationBinding = null;
            StageDefinitionSceneBinding[] bindings = FindSceneComponents<StageDefinitionSceneBinding>(scene);
            for (int i = 0; i < bindings.Length; i++)
            {
                if (ReferenceEquals(bindings[i].StageDefinition, stationDefinition))
                {
                    Require(stationBinding == null,
                        "Continuous Corridor scene contains duplicate Station definition bindings.");
                    stationBinding = bindings[i];
                }
            }

            stationBinding ??= sharedMap.AddComponent<StageDefinitionSceneBinding>();
            Require(stationBinding.gameObject == sharedMap,
                "Station definition binding must live on the shared Corridor map root.");
            stationBinding.Configure(
                stationDefinition,
                sharedMap.transform,
                new[] { addLeftAnchor, addRightAnchor },
                Array.Empty<StageCutscenePort>());

            GameObject entryGuide = FindSceneObject(scene, EntryGuideName);
            if (entryGuide == null)
            {
                GameObject guidePrefab = LoadRequired<GameObject>(GuidePrefabPath);
                entryGuide = (GameObject)PrefabUtility.InstantiatePrefab(guidePrefab, scene);
                entryGuide.name = EntryGuideName;
            }

            Require(
                entryGuide.GetComponent<OlympusStationCombatIntroTutorialBridge>() != null,
                "Station entry guide prefab is missing its runtime bridge.");

            GameObject resultAdapters = FindSceneObject(scene, ResultAdaptersName);
            if (resultAdapters == null)
            {
                resultAdapters = new GameObject(ResultAdaptersName);
                SceneManager.MoveGameObjectToScene(resultAdapters, scene);
            }

            OlympusStationRunFactCollector factCollector =
                resultAdapters.GetComponent<OlympusStationRunFactCollector>()
                ?? resultAdapters.AddComponent<OlympusStationRunFactCollector>();
            OlympusStationCombatResultPresenter resultPresenter =
                resultAdapters.GetComponent<OlympusStationCombatResultPresenter>()
                ?? resultAdapters.AddComponent<OlympusStationCombatResultPresenter>();

            PlayerMovementController movement = FindSingleSceneComponent<PlayerMovementController>(scene);
            PlayerActionController playerAction = movement.GetComponent<PlayerActionController>();
            PlayerRangedBasicAttackAction rangedBasicAttack =
                movement.GetComponent<PlayerRangedBasicAttackAction>();
            PlayerSkill1Action skill1Action = movement.GetComponent<PlayerSkill1Action>();
            PlayerLockTargetController lockTarget = movement.GetComponent<PlayerLockTargetController>();
            CombatHealth playerHealth = movement.GetComponent<CombatHealth>();
            SummonEnergyLadder energy = movement.GetComponent<SummonEnergyLadder>();
            PlayerSummonSlot1Action slot1 = movement.GetComponent<PlayerSummonSlot1Action>();
            PlayerSupportSummonSlotAction[] support =
                movement.GetComponents<PlayerSupportSummonSlotAction>();
            BossBarrageEncounterController bossEncounter =
                FindSingleSceneComponent<BossBarrageEncounterController>(scene);
            BossBarragePocketVfxCueBridge pocketVfxBridge =
                bossEncounter.GetComponent<BossBarragePocketVfxCueBridge>();
            CombatEncounterController encounter =
                FindSingleSceneComponent<CombatEncounterController>(scene);
            OlympusStageClearOverlay stageClearOverlay =
                FindSingleSceneComponent<OlympusStageClearOverlay>(scene);
            CombatSessionOverlayPresenter resultSurface =
                FindSingleSceneComponent<CombatSessionOverlayPresenter>(scene);

            var serializedBoss = new SerializedObject(bossEncounter);
            CombatHealth bossHealth = serializedBoss.FindProperty("bossHealth")
                .objectReferenceValue as CombatHealth;
            BossBarrageVisualCueDriver bossVisualCueDriver =
                bossHealth != null ? bossHealth.GetComponent<BossBarrageVisualCueDriver>() : null;
            Require(
                playerAction != null
                && rangedBasicAttack != null
                && skill1Action != null
                && lockTarget != null
                && playerHealth != null
                && energy != null
                && slot1 != null,
                "Continuous player package is missing a Station fact source.");
            Require(support.Length == 2,
                "Continuous player package must expose exactly two support summon sources.");
            Require(bossHealth != null,
                "Continuous boss package has no authored boss health subject.");
            Require(
                pocketVfxBridge != null && bossVisualCueDriver != null,
                "Continuous boss package is missing its follow-up VFX bridge or visual reaction driver.");

            var serializedPocketVfxBridge = new SerializedObject(pocketVfxBridge);
            serializedPocketVfxBridge.FindProperty("bossVisualCueDriver").objectReferenceValue =
                bossVisualCueDriver;
            serializedPocketVfxBridge.ApplyModifiedPropertiesWithoutUndo();

            ConfigureDesktopInput(
                movement,
                playerAction,
                rangedBasicAttack,
                skill1Action,
                lockTarget);

            var serializedEncounter = new SerializedObject(encounter);
            serializedEncounter.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            serializedEncounter.FindProperty("enemyHealth").objectReferenceValue = bossHealth;
            serializedEncounter.FindProperty("useCoordinatedTerminalResolution").boolValue = true;
            serializedEncounter.ApplyModifiedPropertiesWithoutUndo();

            var serializedCollector = new SerializedObject(factCollector);
            serializedCollector.FindProperty("encounter").objectReferenceValue = encounter;
            serializedCollector.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            serializedCollector.FindProperty("playerActionController").objectReferenceValue = playerAction;
            serializedCollector.FindProperty("summonEnergyLadder").objectReferenceValue = energy;
            serializedCollector.FindProperty("summonSlot1Action").objectReferenceValue = slot1;
            SerializedProperty supportRows = serializedCollector.FindProperty("supportSummonActions");
            supportRows.arraySize = support.Length;
            for (int i = 0; i < support.Length; i++)
            {
                supportRows.GetArrayElementAtIndex(i).objectReferenceValue = support[i];
            }
            serializedCollector.FindProperty("bossEncounter").objectReferenceValue = bossEncounter;
            serializedCollector.FindProperty("resultSurfaceBehaviour").objectReferenceValue = resultSurface;
            serializedCollector.ApplyModifiedPropertiesWithoutUndo();

            var serializedPresenter = new SerializedObject(resultPresenter);
            serializedPresenter.FindProperty("encounter").objectReferenceValue = encounter;
            serializedPresenter.FindProperty("stageClearOverlay").objectReferenceValue = stageClearOverlay;
            serializedPresenter.FindProperty("resultSurfaceBehaviour").objectReferenceValue = resultSurface;
            serializedPresenter.FindProperty("factCollector").objectReferenceValue = factCollector;
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            var serializedFlow = new SerializedObject(flow);
            serializedFlow.FindProperty("playableStageDefinition").objectReferenceValue = route;
            AppendUniqueObject(serializedFlow.FindProperty("corridorCombatRoots"), entryGuide);
            AppendUniqueObject(serializedFlow.FindProperty("corridorCombatRoots"), resultAdapters);
            ConfigureLowerStationCombatRuntime(
                scene,
                serializedFlow,
                combatPackageRoot,
                lowerCombatPlacement);
            serializedFlow.ApplyModifiedPropertiesWithoutUndo();

            entryGuide.SetActive(false);
            resultAdapters.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Failed to save continuous Corridor scene.");
        }

        private static void ConfigureDesktopInput(
            PlayerMovementController movement,
            PlayerActionController playerAction,
            PlayerRangedBasicAttackAction rangedBasicAttack,
            PlayerSkill1Action skill1Action,
            PlayerLockTargetController lockTarget)
        {
            InputActionReference move = LoadInputActionReference(
                "Move",
                "351f2ccd-1f9f-44bf-9bec-d62ac5c5f408",
                InputActionType.Value,
                "Vector2");
            InputActionReference look = LoadInputActionReference(
                "Look",
                "6b444451-8a00-4d00-a97e-f47457f736a8",
                InputActionType.Value,
                "Vector2");
            InputActionReference attack = LoadInputActionReference(
                "Attack",
                "6c2ab1b8-8984-453a-af3d-a3c78ae1679a",
                InputActionType.Button,
                "Button");
            InputActionReference dodge = LoadInputActionReference(
                "Dodge",
                "edaa7bad-e0f4-4b5f-946c-42b9f6c1ae8e",
                InputActionType.Button,
                "Button");
            InputActionReference skill1 = LoadInputActionReference(
                "Skill1",
                "0fbb03a8-9df1-46f7-9cd5-630ac2e738d8",
                InputActionType.Button,
                "Button");

            var serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("moveAction").objectReferenceValue = move;
            serializedMovement.FindProperty("lookAction").objectReferenceValue = look;
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            var serializedPlayerAction = new SerializedObject(playerAction);
            serializedPlayerAction.FindProperty("basicAttackAction").objectReferenceValue = attack;
            serializedPlayerAction.FindProperty("dodgeAction").objectReferenceValue = dodge;
            serializedPlayerAction.ApplyModifiedPropertiesWithoutUndo();

            var serializedRangedAttack = new SerializedObject(rangedBasicAttack);
            serializedRangedAttack.FindProperty("fireAction").objectReferenceValue = attack;
            serializedRangedAttack.FindProperty("manageFireActionLifecycle").boolValue = false;
            serializedRangedAttack.FindProperty("keyboardTestKey").intValue = (int)Key.F;
            serializedRangedAttack.ApplyModifiedPropertiesWithoutUndo();

            var serializedSkill = new SerializedObject(skill1Action);
            serializedSkill.FindProperty("skillAction").objectReferenceValue = skill1;
            serializedSkill.FindProperty("useKeyboardWhenActionMissing").boolValue = true;
            serializedSkill.FindProperty("keyboardTestKey").intValue = (int)Key.R;
            serializedSkill.ApplyModifiedPropertiesWithoutUndo();

            var serializedLockTarget = new SerializedObject(lockTarget);
            serializedLockTarget.FindProperty("keyboardFocusKey").intValue = (int)Key.T;
            serializedLockTarget.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureStationBossTerminalAftermath(Scene scene)
        {
            OlympusStationCombatResultPresenter resultPresenter =
                FindSingleSceneComponent<OlympusStationCombatResultPresenter>(scene);
            OlympusStageClearOverlay stageClearOverlay =
                FindSingleSceneComponent<OlympusStageClearOverlay>(scene);
            BossBarrageEncounterController bossEncounter =
                FindSingleSceneComponent<BossBarrageEncounterController>(scene);
            PlayerMovementController movement =
                FindSingleSceneComponent<PlayerMovementController>(scene);
            BossBarrageCameraCueDriver cameraCueDriver =
                FindSingleSceneComponent<BossBarrageCameraCueDriver>(scene);
            ActionCinematicCueDirector actionCinematicCueDirector =
                FindSingleSceneComponent<ActionCinematicCueDirector>(scene);

            var serializedBoss = new SerializedObject(bossEncounter);
            CombatHealth bossHealth = serializedBoss.FindProperty("bossHealth")
                .objectReferenceValue as CombatHealth;
            Require(bossHealth != null,
                "Station aftermath authoring requires the canonical boss health subject.");
            GameObject clearMarker = FindRequiredSceneObject(
                scene,
                StationPocketClearMarkerName);
            clearMarker.SetActive(false);
            SerializedProperty clearMarkerReference = serializedBoss.FindProperty("clearMarker");
            Require(clearMarkerReference != null,
                "Station boss encounter is missing its clearMarker authoring field.");
            clearMarkerReference.objectReferenceValue = null;
            serializedBoss.ApplyModifiedPropertiesWithoutUndo();

            GameObject terminalBoundaryVisual = FindRequiredSceneObject(
                scene,
                StationTerminalBoundaryVisualName);
            terminalBoundaryVisual.SetActive(true);

            BossBarrageLaneReviewCombatHudBinder hudBinder =
                FindSingleSceneComponent<BossBarrageLaneReviewCombatHudBinder>(scene);
            var serializedHudBinder = new SerializedObject(hudBinder);
            SerializedProperty bossDisplayName = serializedHudBinder.FindProperty("bossDisplayName");
            Require(bossDisplayName != null,
                "Station combat HUD binder is missing its bossDisplayName authoring field.");
            bossDisplayName.stringValue = StationBossDisplayName;
            serializedHudBinder.ApplyModifiedPropertiesWithoutUndo();

            Require(cameraCueDriver.CameraController != null,
                "Station finisher authoring requires the canonical action camera controller.");
            Camera gameplayCamera = cameraCueDriver.CameraController.GetComponent<Camera>();
            Require(gameplayCamera != null,
                "Station finisher authoring requires the canonical gameplay Camera component.");
            FinisherCameraTimelineBindings finisherTimeline =
                EnsureStationBossTerminalFinisherTimeline();
            OlympusStationBossTerminalFinisherCameraController finisherCameraController =
                ConfigureStationBossTerminalFinisherCamera(
                    scene,
                    bossHealth.transform,
                    gameplayCamera,
                    finisherTimeline);
            BossBarrageVisualCueDriver visualCueDriver =
                bossHealth.GetComponent<BossBarrageVisualCueDriver>();
            PlayerActionController action = movement.GetComponent<PlayerActionController>();
            PlayerSkill1Action skill1 = movement.GetComponent<PlayerSkill1Action>();
            PlayerSummonSlot1Action summon1 = movement.GetComponent<PlayerSummonSlot1Action>();
            PlayerRangedBasicAttackAction ranged =
                movement.GetComponent<PlayerRangedBasicAttackAction>();
            PlayerCombatModeController combatMode =
                movement.GetComponent<PlayerCombatModeController>();
            PlayerSupportSummonSlotAction[] supports =
                movement.GetComponents<PlayerSupportSummonSlotAction>();
            PlayerSupportSummonSlotAction summon2 = Array.Find(
                supports,
                candidate => candidate != null
                    && string.Equals(candidate.SlotActionName, "SummonSlot2", StringComparison.Ordinal));
            PlayerSupportSummonSlotAction summon3 = Array.Find(
                supports,
                candidate => candidate != null
                    && string.Equals(candidate.SlotActionName, "SummonSlot3", StringComparison.Ordinal));
            Require(
                cameraCueDriver != null
                    && actionCinematicCueDirector != null
                    && visualCueDriver != null
                    && action != null
                    && skill1 != null
                    && summon1 != null
                    && summon2 != null
                    && summon3 != null
                    && ranged != null
                    && combatMode != null,
                "Station aftermath authoring requires its action/death camera owners, VFX, and exact eight player input owners.");

            OlympusStationBossTerminalAftermathPresenter aftermath =
                resultPresenter.GetComponent<OlympusStationBossTerminalAftermathPresenter>()
                ?? resultPresenter.gameObject.AddComponent<OlympusStationBossTerminalAftermathPresenter>();
            var serializedAftermath = new SerializedObject(aftermath);
            serializedAftermath.FindProperty("bossHealth").objectReferenceValue = bossHealth;
            serializedAftermath.FindProperty("cameraCueDriver").objectReferenceValue = cameraCueDriver;
            serializedAftermath.FindProperty("finisherCameraController").objectReferenceValue =
                finisherCameraController;
            serializedAftermath.FindProperty("actionCinematicCueDirector").objectReferenceValue =
                actionCinematicCueDirector;
            serializedAftermath.FindProperty("visualCueDriver").objectReferenceValue = visualCueDriver;
            SerializedProperty terminalBoundaryVisualRoot =
                serializedAftermath.FindProperty("terminalBoundaryVisualRoot");
            Require(terminalBoundaryVisualRoot != null,
                "Station aftermath presenter is missing its terminalBoundaryVisualRoot authoring field.");
            terminalBoundaryVisualRoot.objectReferenceValue = terminalBoundaryVisual;
            serializedAftermath.FindProperty("aftermathDurationSeconds").floatValue =
                StationBossTerminalFinisherDurationSeconds;
            serializedAftermath.FindProperty("unattachedResultLeaseTimeoutSeconds").floatValue = 2f;
            serializedAftermath.FindProperty("initialHitStopRecoveryGraceSeconds").floatValue = 0.35f;
            serializedAftermath.FindProperty("playerMovement").objectReferenceValue = movement;
            serializedAftermath.FindProperty("playerActionController").objectReferenceValue = action;
            serializedAftermath.FindProperty("playerSkill1Action").objectReferenceValue = skill1;
            serializedAftermath.FindProperty("playerSummonSlot1Action").objectReferenceValue = summon1;
            serializedAftermath.FindProperty("playerSummonSlot2Action").objectReferenceValue = summon2;
            serializedAftermath.FindProperty("playerSummonSlot3Action").objectReferenceValue = summon3;
            serializedAftermath.FindProperty("playerRangedBasicAttackAction").objectReferenceValue = ranged;
            serializedAftermath.FindProperty("playerCombatModeController").objectReferenceValue = combatMode;
            serializedAftermath.ApplyModifiedPropertiesWithoutUndo();

            var serializedOverlay = new SerializedObject(stageClearOverlay);
            serializedOverlay.FindProperty("bossTerminalAftermath").objectReferenceValue = aftermath;
            serializedOverlay.FindProperty("bossTerminalAftermathWaitSlackSeconds").floatValue = 0.5f;
            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();

            var serializedResultPresenter = new SerializedObject(resultPresenter);
            serializedResultPresenter.FindProperty("bossTerminalAftermath").objectReferenceValue = aftermath;
            serializedResultPresenter.ApplyModifiedPropertiesWithoutUndo();

            var serializedCamera = new SerializedObject(cameraCueDriver);
            SerializedProperty bossDeathCue = serializedCamera.FindProperty("bossDeathCue");
            Require(bossDeathCue != null,
                "Boss camera driver is missing its authored bossDeathCue field.");
            bossDeathCue.FindPropertyRelative("enabled").boolValue = true;
            bossDeathCue.FindPropertyRelative("localOffset").vector3Value =
                new Vector3(0f, 0.08f, 0.18f);
            bossDeathCue.FindPropertyRelative("planarDirectionOffset").floatValue = 0.08f;
            bossDeathCue.FindPropertyRelative("fieldOfViewDelta").floatValue = -3.5f;
            bossDeathCue.FindPropertyRelative("cameraDistanceDelta").floatValue = -0.35f;
            bossDeathCue.FindPropertyRelative("focusHeightDelta").floatValue = 0.18f;
            bossDeathCue.FindPropertyRelative("durationSeconds").floatValue = 1.65f;
            bossDeathCue.FindPropertyRelative("finisherScale").floatValue = 1f;
            serializedCamera.FindProperty("bossDeathCueReleaseSeconds").floatValue = 0.35f;
            serializedCamera.FindProperty("bossDeathImpactShakeSeconds").floatValue = 0.14f;
            serializedCamera.FindProperty("bossDeathImpactPositionAmplitude").floatValue = 0.018f;
            serializedCamera.FindProperty("bossDeathImpactEulerAmplitude").floatValue = 0.11f;
            serializedCamera.ApplyModifiedPropertiesWithoutUndo();

            var serializedVisual = new SerializedObject(visualCueDriver);
            serializedVisual.FindProperty("bossDeathCueId").intValue =
                (int)CombatVfxCueId.EnemyDeath;
            serializedVisual.FindProperty("bossDeathCueIntensity").floatValue = 1.15f;
            serializedVisual.FindProperty("bossDeathAudioIntensity").floatValue = 1f;
            serializedVisual.FindProperty("bossDeathFlashColor").colorValue = Color.white;
            serializedVisual.FindProperty("bossDeathFlashSeconds").floatValue = 0.35f;
            SerializedProperty bossDeathPulseScale =
                serializedVisual.FindProperty("bossDeathPulseScale");
            Require(bossDeathPulseScale != null,
                "Boss visual driver is missing its authored bossDeathPulseScale field.");
            bossDeathPulseScale.floatValue = 0.42f;
            serializedVisual.ApplyModifiedPropertiesWithoutUndo();
        }

        private static FinisherCameraTimelineBindings
            EnsureStationBossTerminalFinisherTimeline()
        {
            Require(
                AssetDatabase.IsValidFolder("Assets/_Game/DesignData/Timelines/Cinematics"),
                "Station finisher authoring folder is missing.");

            AnimationClip cameraClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    StationBossTerminalFinisherCameraClipPath);
            if (cameraClip == null)
            {
                cameraClip = new AnimationClip();
                AssetDatabase.CreateAsset(
                    cameraClip,
                    StationBossTerminalFinisherCameraClipPath);
            }

            cameraClip.name = "DB_Anim_OlympusStationBossTerminalFinisherCamera";
            cameraClip.frameRate = 60f;
            cameraClip.wrapMode = WrapMode.Once;
            cameraClip.ClearCurves();

            Quaternion startRotation = Quaternion.LookRotation(
                StationBossTerminalFinisherStartLookTarget
                    - StationBossTerminalFinisherStartLocalPosition,
                Vector3.up);
            Quaternion settleRotation = Quaternion.LookRotation(
                StationBossTerminalFinisherSettleLookTarget
                    - StationBossTerminalFinisherSettleLocalPosition,
                Vector3.up);
            SetLocalTransformCurve(
                cameraClip,
                "m_LocalPosition.x",
                0f,
                StationBossTerminalFinisherStartLocalPosition.x,
                StationBossTerminalFinisherSettleSeconds,
                StationBossTerminalFinisherSettleLocalPosition.x,
                StationBossTerminalFinisherDurationSeconds,
                StationBossTerminalFinisherSettleLocalPosition.x);
            SetLocalTransformCurve(
                cameraClip,
                "m_LocalPosition.y",
                0f,
                StationBossTerminalFinisherStartLocalPosition.y,
                StationBossTerminalFinisherSettleSeconds,
                StationBossTerminalFinisherSettleLocalPosition.y,
                StationBossTerminalFinisherDurationSeconds,
                StationBossTerminalFinisherSettleLocalPosition.y);
            SetLocalTransformCurve(
                cameraClip,
                "m_LocalPosition.z",
                0f,
                StationBossTerminalFinisherStartLocalPosition.z,
                StationBossTerminalFinisherSettleSeconds,
                StationBossTerminalFinisherSettleLocalPosition.z,
                StationBossTerminalFinisherDurationSeconds,
                StationBossTerminalFinisherSettleLocalPosition.z);
            SetLocalTransformCurve(
                cameraClip,
                "m_LocalRotation.x",
                0f,
                startRotation.x,
                StationBossTerminalFinisherSettleSeconds,
                settleRotation.x,
                StationBossTerminalFinisherDurationSeconds,
                settleRotation.x);
            SetLocalTransformCurve(
                cameraClip,
                "m_LocalRotation.y",
                0f,
                startRotation.y,
                StationBossTerminalFinisherSettleSeconds,
                settleRotation.y,
                StationBossTerminalFinisherDurationSeconds,
                settleRotation.y);
            SetLocalTransformCurve(
                cameraClip,
                "m_LocalRotation.z",
                0f,
                startRotation.z,
                StationBossTerminalFinisherSettleSeconds,
                settleRotation.z,
                StationBossTerminalFinisherDurationSeconds,
                settleRotation.z);
            SetLocalTransformCurve(
                cameraClip,
                "m_LocalRotation.w",
                0f,
                startRotation.w,
                StationBossTerminalFinisherSettleSeconds,
                settleRotation.w,
                StationBossTerminalFinisherDurationSeconds,
                settleRotation.w);
            cameraClip.EnsureQuaternionContinuity();
            SaveAsset(cameraClip);

            TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(
                StationBossTerminalFinisherTimelinePath);
            if (timeline != null
                && TryGetExactStationBossTerminalFinisherTimeline(
                    timeline,
                    cameraClip,
                    out AnimationTrack exactCameraTrack))
            {
                return new FinisherCameraTimelineBindings(
                    timeline,
                    exactCameraTrack);
            }

            if (timeline == null)
            {
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(
                    timeline,
                    StationBossTerminalFinisherTimelinePath);
            }

            timeline.name = "DB_Timeline_OlympusStationBossTerminalFinisher";
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = StationBossTerminalFinisherDurationSeconds;
            timeline.editorSettings.frameRate = 60f;

            AnimationTrack cameraTrack = null;
            var existingTracks = new List<TrackAsset>(timeline.GetRootTracks());
            for (int i = 0; i < existingTracks.Count; i++)
            {
                TrackAsset existingTrack = existingTracks[i];
                if (cameraTrack == null
                    && existingTrack is AnimationTrack existingAnimationTrack
                    && string.Equals(
                        existingAnimationTrack.name,
                        StationBossTerminalFinisherCameraTrackName,
                        StringComparison.Ordinal))
                {
                    cameraTrack = existingAnimationTrack;
                    continue;
                }

                timeline.DeleteTrack(existingTrack);
            }

            if (cameraTrack == null)
            {
                cameraTrack = timeline.CreateTrack<AnimationTrack>(
                    StationBossTerminalFinisherCameraTrackName);
            }

            var childTracks = new List<TrackAsset>(cameraTrack.GetChildTracks());
            for (int i = 0; i < childTracks.Count; i++)
            {
                timeline.DeleteTrack(childTracks[i]);
            }

            cameraTrack.name = StationBossTerminalFinisherCameraTrackName;
            cameraTrack.locked = false;
            cameraTrack.muted = false;
            cameraTrack.trackOffset = TrackOffset.Auto;
            var serializedTrack = new SerializedObject(cameraTrack);
            SerializedProperty applyOffsets = serializedTrack.FindProperty("m_ApplyOffsets");
            if (applyOffsets != null)
            {
                applyOffsets.boolValue = false;
                serializedTrack.ApplyModifiedPropertiesWithoutUndo();
            }

            TimelineClip timelineClip = null;
            var existingClips = new List<TimelineClip>(cameraTrack.GetClips());
            for (int i = 0; i < existingClips.Count; i++)
            {
                TimelineClip existingClip = existingClips[i];
                if (timelineClip == null
                    && existingClip.asset is AnimationPlayableAsset)
                {
                    timelineClip = existingClip;
                    continue;
                }

                timeline.DeleteClip(existingClip);
            }

            if (timelineClip == null)
            {
                timelineClip = cameraTrack.CreateClip(cameraClip);
            }

            timelineClip.displayName = "Boss Terminal Camera 0.00-2.60";
            timelineClip.start = 0d;
            timelineClip.clipIn = 0d;
            timelineClip.duration = StationBossTerminalFinisherDurationSeconds;
            timelineClip.timeScale = 1d;
            timelineClip.easeInDuration = 0d;
            timelineClip.easeOutDuration = 0d;
            SetTimelineClipExtrapolation(
                timelineClip,
                TimelineClip.ClipExtrapolation.None,
                TimelineClip.ClipExtrapolation.Hold);
            Require(
                timelineClip.asset is AnimationPlayableAsset,
                "Station finisher Timeline did not create an AnimationPlayableAsset.");
            var playable = (AnimationPlayableAsset)timelineClip.asset;
            playable.name = $"AnimationPlayableAsset of {cameraClip.name}";
            playable.clip = cameraClip;
            playable.position = Vector3.zero;
            playable.rotation = Quaternion.identity;
            playable.removeStartOffset = false;
            playable.applyFootIK = false;
            playable.loop = AnimationPlayableAsset.LoopMode.Off;
            playable.useTrackMatchFields = false;

            EditorUtility.SetDirty(playable);
            EditorUtility.SetDirty(cameraTrack);
            SaveAsset(timeline);
            return new FinisherCameraTimelineBindings(
                timeline,
                cameraTrack);
        }

        private static bool TryGetExactStationBossTerminalFinisherTimeline(
            TimelineAsset timeline,
            AnimationClip cameraClip,
            out AnimationTrack cameraTrack)
        {
            cameraTrack = null;
            if (timeline == null
                || cameraClip == null
                || !string.Equals(
                    timeline.name,
                    "DB_Timeline_OlympusStationBossTerminalFinisher",
                    StringComparison.Ordinal)
                || timeline.durationMode != TimelineAsset.DurationMode.FixedLength
                || !ApproximatelyEqual(
                    timeline.fixedDuration,
                    StationBossTerminalFinisherDurationSeconds)
                || !ApproximatelyEqual(timeline.editorSettings.frameRate, 60d))
            {
                return false;
            }

            var rootTracks = new List<TrackAsset>(timeline.GetRootTracks());
            if (rootTracks.Count != 1
                || rootTracks[0] is not AnimationTrack exactTrack
                || !string.Equals(
                    exactTrack.name,
                    StationBossTerminalFinisherCameraTrackName,
                    StringComparison.Ordinal)
                || exactTrack.locked
                || exactTrack.muted
                || exactTrack.trackOffset != TrackOffset.Auto)
            {
                return false;
            }

            var childTracks = new List<TrackAsset>(exactTrack.GetChildTracks());
            if (childTracks.Count != 0)
            {
                return false;
            }

            var serializedTrack = new SerializedObject(exactTrack);
            SerializedProperty applyOffsets =
                serializedTrack.FindProperty("m_ApplyOffsets");
            if (applyOffsets == null || applyOffsets.boolValue)
            {
                return false;
            }

            var clips = new List<TimelineClip>(exactTrack.GetClips());
            if (clips.Count != 1)
            {
                return false;
            }

            TimelineClip timelineClip = clips[0];
            if (timelineClip.asset is not AnimationPlayableAsset playable
                || !string.Equals(
                    timelineClip.displayName,
                    "Boss Terminal Camera 0.00-2.60",
                    StringComparison.Ordinal)
                || !ApproximatelyEqual(timelineClip.start, 0d)
                || !ApproximatelyEqual(timelineClip.clipIn, 0d)
                || !ApproximatelyEqual(
                    timelineClip.duration,
                    StationBossTerminalFinisherDurationSeconds)
                || !ApproximatelyEqual(timelineClip.timeScale, 1d)
                || !ApproximatelyEqual(timelineClip.easeInDuration, 0d)
                || !ApproximatelyEqual(timelineClip.easeOutDuration, 0d)
                || timelineClip.preExtrapolationMode
                    != TimelineClip.ClipExtrapolation.None
                || timelineClip.postExtrapolationMode
                    != TimelineClip.ClipExtrapolation.Hold
                || !string.Equals(
                    playable.name,
                    $"AnimationPlayableAsset of {cameraClip.name}",
                    StringComparison.Ordinal)
                || !ReferenceEquals(playable.clip, cameraClip)
                || playable.position != Vector3.zero
                || playable.rotation != Quaternion.identity
                || playable.removeStartOffset
                || playable.applyFootIK
                || playable.loop != AnimationPlayableAsset.LoopMode.Off
                || playable.useTrackMatchFields)
            {
                return false;
            }

            cameraTrack = exactTrack;
            return true;
        }

        private static bool ApproximatelyEqual(double left, double right)
        {
            return Math.Abs(left - right) <= 0.0001d;
        }

        private static OlympusStationBossTerminalFinisherCameraController
            ConfigureStationBossTerminalFinisherCamera(
                Scene scene,
                Transform stableBossRoot,
                Camera gameplayCamera,
                FinisherCameraTimelineBindings timeline)
        {
            Require(stableBossRoot != null,
                "Station finisher camera requires the stable canonical boss root.");
            Require(gameplayCamera != null,
                "Station finisher camera requires the canonical gameplay Camera.");

            GameObject rig = FindSceneObject(
                scene,
                StationBossTerminalFinisherCameraRigName);
            if (rig == null)
            {
                rig = new GameObject(StationBossTerminalFinisherCameraRigName);
                SceneManager.MoveGameObjectToScene(rig, scene);
            }

            rig.name = StationBossTerminalFinisherCameraRigName;
            rig.tag = "Untagged";
            rig.SetActive(true);
            rig.transform.SetParent(stableBossRoot, worldPositionStays: false);
            rig.transform.localScale = Vector3.one;
            rig.transform.localPosition = StationBossTerminalFinisherStartLocalPosition;
            rig.transform.localRotation = Quaternion.LookRotation(
                StationBossTerminalFinisherStartLookTarget
                    - StationBossTerminalFinisherStartLocalPosition,
                Vector3.up);

            AudioListener[] listeners = rig.GetComponentsInChildren<AudioListener>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(listeners[i]);
            }

            Animator animator = rig.GetComponent<Animator>() ?? rig.AddComponent<Animator>();
            animator.runtimeAnimatorController = null;
            animator.avatar = null;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.enabled = true;

            Camera finisherCamera = rig.GetComponent<Camera>() ?? rig.AddComponent<Camera>();
            finisherCamera.CopyFrom(gameplayCamera);
            finisherCamera.fieldOfView = StationBossTerminalFinisherFieldOfView;
            finisherCamera.enabled = false;

            UniversalAdditionalCameraData finisherCameraData =
                rig.GetComponent<UniversalAdditionalCameraData>()
                ?? rig.AddComponent<UniversalAdditionalCameraData>();
            UniversalAdditionalCameraData gameplayCameraData =
                gameplayCamera.GetComponent<UniversalAdditionalCameraData>();
            if (gameplayCameraData != null)
            {
                EditorUtility.CopySerialized(gameplayCameraData, finisherCameraData);
            }

            finisherCameraData.renderPostProcessing = true;
            finisherCameraData.antialiasing =
                AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            finisherCameraData.antialiasingQuality = AntialiasingQuality.High;

            PlayableDirector director =
                rig.GetComponent<PlayableDirector>() ?? rig.AddComponent<PlayableDirector>();
            director.playableAsset = timeline.Asset;
            director.playOnAwake = false;
            director.extrapolationMode = DirectorWrapMode.Hold;
            director.timeUpdateMode = DirectorUpdateMode.Manual;
            EnsureSingleDirectorGenericBinding(
                director,
                timeline.CameraTrack,
                animator);
            director.RebuildGraph();
            director.time = 0d;
            director.Evaluate();
            Physics.SyncTransforms();

            OlympusStationBossTerminalFinisherCameraController[] controllers =
                FindSceneComponents<OlympusStationBossTerminalFinisherCameraController>(scene);
            OlympusStationBossTerminalFinisherCameraController controller =
                rig.GetComponent<OlympusStationBossTerminalFinisherCameraController>()
                ?? rig.AddComponent<OlympusStationBossTerminalFinisherCameraController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i] != controller)
                {
                    UnityEngine.Object.DestroyImmediate(controllers[i]);
                }
            }

            Require(
                controller.Configure(
                    director,
                    timeline.Asset,
                    gameplayCamera,
                    finisherCamera),
                "Station finisher camera controller rejected its authored references.");
            Require(controller.ValidateConfiguration(out string validationError),
                validationError);

            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(finisherCamera);
            EditorUtility.SetDirty(finisherCameraData);
            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void EnsureSingleDirectorGenericBinding(
            PlayableDirector director,
            TrackAsset source,
            UnityEngine.Object target)
        {
            Require(director != null,
                "Station finisher binding requires a PlayableDirector.");
            Require(source != null,
                "Station finisher binding requires the current Timeline output track.");
            Require(target != null,
                "Station finisher binding requires a non-null target.");

            var serializedDirector = new SerializedObject(director);
            SerializedProperty sceneBindings =
                serializedDirector.FindProperty("m_SceneBindings");
            Require(
                sceneBindings != null && sceneBindings.isArray,
                "Station finisher PlayableDirector has no serialized scene-binding table.");

            bool alreadyExact = sceneBindings.arraySize == 1;
            if (alreadyExact)
            {
                SerializedProperty row = sceneBindings.GetArrayElementAtIndex(0);
                UnityEngine.Object serializedSource =
                    row.FindPropertyRelative("key")?.objectReferenceValue;
                UnityEngine.Object serializedTarget =
                    row.FindPropertyRelative("value")?.objectReferenceValue;
                alreadyExact = ReferenceEquals(serializedSource, source)
                    && ReferenceEquals(serializedTarget, target);
            }

            if (!alreadyExact)
            {
                sceneBindings.arraySize = 0;
                serializedDirector.ApplyModifiedPropertiesWithoutUndo();
            }

            director.SetGenericBinding(source, target);
            serializedDirector.Update();
            sceneBindings = serializedDirector.FindProperty("m_SceneBindings");
            Require(
                sceneBindings != null && sceneBindings.isArray
                    && sceneBindings.arraySize == 1,
                "Station finisher PlayableDirector must serialize exactly one current output binding.");

            SerializedProperty exactRow = sceneBindings.GetArrayElementAtIndex(0);
            UnityEngine.Object exactSource =
                exactRow.FindPropertyRelative("key")?.objectReferenceValue;
            UnityEngine.Object exactTarget =
                exactRow.FindPropertyRelative("value")?.objectReferenceValue;
            Require(
                ReferenceEquals(exactSource, source)
                    && ReferenceEquals(exactTarget, target),
                "Station finisher PlayableDirector binding must target the current Timeline output and Animator.");
        }

        private static void SetLocalTransformCurve(
            AnimationClip clip,
            string propertyName,
            float startTime,
            float startValue,
            float settleTime,
            float settleValue,
            float endTime,
            float endValue)
        {
            var curve = new AnimationCurve(
                new Keyframe(startTime, startValue),
                new Keyframe(settleTime, settleValue),
                new Keyframe(endTime, endValue));
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(
                    curve,
                    i,
                    AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(
                    curve,
                    i,
                    AnimationUtility.TangentMode.Linear);
            }

            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), propertyName),
                curve);
        }

        private static void SetTimelineClipExtrapolation(
            TimelineClip clip,
            TimelineClip.ClipExtrapolation preExtrapolation,
            TimelineClip.ClipExtrapolation postExtrapolation)
        {
            const System.Reflection.BindingFlags Flags =
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic;
            typeof(TimelineClip).GetField("m_PreExtrapolationMode", Flags)
                ?.SetValue(clip, preExtrapolation);
            typeof(TimelineClip).GetField("m_PostExtrapolationMode", Flags)
                ?.SetValue(clip, postExtrapolation);
        }

        private static InputActionReference LoadInputActionReference(
            string actionName,
            string actionId,
            InputActionType expectedType,
            string expectedControlType)
        {
            InputActionAsset inputActions = LoadRequired<InputActionAsset>(InputActionsPath);
            Guid expectedMapId = new Guid(PlayerInputActionMapId);
            Guid expectedActionId = new Guid(actionId);
            InputAction action = inputActions.FindAction(expectedActionId);
            Require(action != null, $"Missing Player/{actionName} ({actionId}) in {InputActionsPath}.");
            Require(
                action.actionMap != null
                && action.actionMap.id == expectedMapId
                && string.Equals(action.actionMap.name, "Player", StringComparison.Ordinal)
                && string.Equals(action.name, actionName, StringComparison.Ordinal)
                && action.type == expectedType
                && string.Equals(action.expectedControlType, expectedControlType, StringComparison.Ordinal),
                $"Player/{actionName} no longer matches the authored desktop input identity.");

            InputActionReference found = null;
            UnityEngine.Object[] importedObjects = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath);
            for (int i = 0; i < importedObjects.Length; i++)
            {
                if (!(importedObjects[i] is InputActionReference candidate)
                    || (candidate.hideFlags & HideFlags.HideInHierarchy) != 0
                    || !ReferenceEquals(candidate.asset, inputActions)
                    || candidate.action == null
                    || candidate.action.id != expectedActionId)
                {
                    continue;
                }

                Require(found == null, $"Duplicate imported InputActionReference for Player/{actionName}.");
                found = candidate;
            }

            Require(found != null, $"Missing imported InputActionReference for Player/{actionName}.");
            return found;
        }

        private static void ConfigureLowerStationCombatRuntime(
            Scene scene,
            SerializedObject serializedFlow,
            Transform combatPackageRoot,
            Transform lowerCombatPlacement)
        {
            Require(combatPackageRoot != null, "Continuous combat package root is missing.");
            Require(lowerCombatPlacement != null, "Authored lower combat placement is missing.");
            Require(
                Approximately(combatPackageRoot.lossyScale, Vector3.one),
                "Continuous combat package root must retain unit scale before the Station split.");

            GameObject lowerRuntimeObject = FindSceneObject(scene, LowerCombatRuntimeRootName);
            if (lowerRuntimeObject == null)
            {
                lowerRuntimeObject = new GameObject(LowerCombatRuntimeRootName);
                SceneManager.MoveGameObjectToScene(lowerRuntimeObject, scene);
            }

            Transform lowerRuntimeRoot = lowerRuntimeObject.transform;
            lowerRuntimeRoot.SetPositionAndRotation(
                lowerCombatPlacement.position,
                lowerCombatPlacement.rotation);
            lowerRuntimeRoot.localScale = Vector3.one;

            RebaseStationRoots(
                serializedFlow.FindProperty("corridorCombatRoots"),
                combatPackageRoot,
                lowerRuntimeRoot);
            RebaseStationRoots(
                serializedFlow.FindProperty("corridorBoundsRoots"),
                combatPackageRoot,
                lowerRuntimeRoot);
        }

        private static void RebaseStationRoots(
            SerializedProperty roots,
            Transform combatPackageRoot,
            Transform lowerRuntimeRoot)
        {
            Require(roots != null && roots.isArray, "Expected a serialized Station root array.");
            for (int i = 0; i < roots.arraySize; i++)
            {
                UnityEngine.Object value = roots.GetArrayElementAtIndex(i).objectReferenceValue;
                GameObject rootObject = value as GameObject;
                if (rootObject == null && value is Component component)
                {
                    rootObject = component.gameObject;
                }

                if (rootObject == null)
                {
                    continue;
                }

                Transform root = rootObject.transform;
                if (root.parent == lowerRuntimeRoot)
                {
                    continue;
                }

                if (root.parent != combatPackageRoot)
                {
                    continue;
                }

                Vector3 sourcePosition = root.localPosition + SourceStationPlayerStartPosition;
                Quaternion sourceRotation = root.localRotation;
                Vector3 sourceScale = root.localScale;
                root.SetParent(lowerRuntimeRoot, worldPositionStays: false);
                root.localPosition = sourcePosition;
                root.localRotation = sourceRotation;
                root.localScale = sourceScale;
            }
        }

        private static bool Approximately(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude <= 0.000001f;
        }

        private static void ConfigureTraversalSupport(
            GameObject support,
            Vector3 topSurfaceStart,
            Vector3 topSurfaceEnd)
        {
            Require(support != null, "Traversal support object is missing.");
            Vector3 span = topSurfaceEnd - topSurfaceStart;
            Require(span.sqrMagnitude > 0.01f, $"Traversal support {support.name} has no span.");
            Quaternion rotation = Quaternion.LookRotation(span.normalized, Vector3.up);
            Vector3 surfaceNormal = rotation * Vector3.up;
            support.transform.SetPositionAndRotation(
                (topSurfaceStart + topSurfaceEnd) * 0.5f
                - surfaceNormal * (TraversalSupportThickness * 0.5f),
                rotation);
            support.transform.localScale = Vector3.one;

            BoxCollider collider = support.GetComponent<BoxCollider>()
                ?? support.AddComponent<BoxCollider>();
            collider.enabled = true;
            collider.isTrigger = false;
            collider.center = Vector3.zero;
            collider.size = new Vector3(
                TraversalSupportWidth,
                TraversalSupportThickness,
                span.magnitude);
        }

        private static void AppendUniqueObject(SerializedProperty array, UnityEngine.Object value)
        {
            Require(array != null && array.isArray, "Expected a serialized object array.");
            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == value)
                {
                    return;
                }
            }

            int index = array.arraySize;
            array.InsertArrayElementAtIndex(index);
            array.GetArrayElementAtIndex(index).objectReferenceValue = value;
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            serialized.Update();
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null, $"Missing serialized property {propertyName}.");
            property.stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRelativeString(
            SerializedObject serialized,
            string ownerName,
            string propertyName,
            string value)
        {
            serialized.Update();
            SerializedProperty owner = serialized.FindProperty(ownerName);
            SerializedProperty property = owner?.FindPropertyRelative(propertyName);
            Require(property != null, $"Missing serialized property {ownerName}.{propertyName}.");
            property.stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null, $"Missing required asset: {path}");
            return asset;
        }

        private static GameObject FindRequiredSceneObject(Scene scene, string name)
        {
            GameObject result = FindSceneObject(scene, name);
            Require(result != null, $"Missing scene object {name} in {scene.path}.");
            return result;
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    if (string.Equals(transforms[i].name, name, StringComparison.Ordinal))
                    {
                        return transforms[i].gameObject;
                    }
                }
            }

            return null;
        }

        private static T FindSingleSceneComponent<T>(Scene scene) where T : Component
        {
            T[] components = FindSceneComponents<T>(scene);
            Require(components.Length == 1,
                $"Expected one {typeof(T).Name} in {scene.path}; found {components.Length}.");
            return components[0];
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                results.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }

            return results.ToArray();
        }

        private readonly struct FinisherCameraTimelineBindings
        {
            public FinisherCameraTimelineBindings(
                TimelineAsset asset,
                AnimationTrack cameraTrack)
            {
                Asset = asset;
                CameraTrack = cameraTrack;
            }

            public TimelineAsset Asset { get; }
            public AnimationTrack CameraTrack { get; }
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
