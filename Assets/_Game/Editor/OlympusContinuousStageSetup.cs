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
using UnityEngine.SceneManagement;

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
