using System;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Validates the persistent B1-1 content pack without treating it as a published product
    /// route. Product publication remains the separate B1-2 catalog/build-settings gate.
    /// </summary>
    public static class OlympusCourtyardDrillAuthoredPackValidator
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        public const string StageDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCourtyardDrillCombat.asset";
        public const string PlayableStagePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusCourtyardDrill.asset";
        public const string StageTemplatePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusCourtyardDrillRun.asset";
        public const string ResultPresentationProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentation_OlympusCourtyardDrill.asset";
        public const string ResultLocalizationPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultLocalization_OlympusCourtyardDrill.asset";
        public const string ResultPresentationCatalogPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog_OlympusCourtyardDrill.asset";
        public const string ResultDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusCourtyardDrill.asset";
        public const string ProgressionNodePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusCourtyardDrill.asset";
        public const string ProgressionGraphPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusCourtyardDrill.asset";

        public const string StageId = "OLYMPUS-COURTYARD-DRILL-COMBAT-01";
        public const string PlayableStageId = "OLYMPUS-COURTYARD-DRILL-01";
        public const string RouteSegmentId = "courtyard_drill_combat";
        public const string TerminalConditionId = "courtyard-drill.encounter.terminal";
        public const string TemplateId = "olympus-courtyard-drill.standard-run";
        public const string TemplateSegmentId = "olympus-courtyard-drill.combat";
        public const string TemplatePocketId = "olympus-courtyard-drill.terminal-combat";
        public const string TerminalPolicyId = "olympus-courtyard-drill.same-terminal-epoch";
        public const string ReplayActionId = "olympus-courtyard-drill.replay";
        public const string RetryActionId = "olympus-courtyard-drill.retry";
        public const string LobbyActionId = "olympus-courtyard-drill.to-lobby";
        private const string ReplayActionLabelKey = "stage_result.action.replay";
        private const string RetryActionLabelKey = "stage_result.action.retry";
        private const string LobbyActionLabelKey = "stage_result.action.lobby";
        public const string ResultDefinitionId = "olympus-courtyard-drill.result-definition";
        public const string ResultPresentationProfileId = "stage-result.olympus-courtyard-drill";
        public const string ResultPresentationCatalogId =
            "stage-result.presentation.catalog.olympus-courtyard-drill";
        public const string ResultLocalizationId =
            "stage-result.localization.olympus-courtyard-drill";
        public const string ProgressionNodeId = "olympus-courtyard-drill.progression-node";
        public const string ProgressionGraphId = "olympus-courtyard-drill.progression-graph";

        private const string RunEntryConditionId = "run.entry.admitted";
        private const string ResultStageCode = "01-2";
        private const string ChapterId = "OLYMPUS-INVASION";
        private const string MapRootName = "OlympusCourtyardDrillStageRoot";
        private const string MapContentRootName = "OlympusCourtyardDrillMap";
        private const string LayoutId = "OLYMPUS_COURTYARD_DRILL_COMPACT_01";
        private const string SpawnAnchorGroupId = "CombatSpawnAnchors";
        private const string PlayerAnchorId = "Player_Start";
        private const string BossAnchorId = "Boss_Terminal";
        private const string AddAnchorId = "Add_RifleCrossfire";
        private const string PlayerSpawnId = "player-start";
        private const string BossSpawnId = "terminal-boss";
        private const string AddSpawnId = "rifle-crossfire";
        private const string PlayerPayloadId = "PlayerParty";
        private const string BossPayloadId = "CourtyardDrill.MiniBoss";
        private const string AddPayloadId = "SciFiSoldier.Ranged";
        private const string TerminalRuntimeStateId = "state-courtyard-encounter-terminal";
        private const int PlayerPositionId = 1101;
        private const int BossPositionId = 1201;
        private const int AddPositionId = 1301;
        private const int TerminalRuntimeStatePositionId = 9001;
        private const string RangedAddArchetypePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/DB_Archetype_SciFiSoldier_Ranged.asset";

        private const string AcceptedRoutePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";
        private const string AcceptedCorridorDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string AcceptedStationDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusStationCombat.asset";
        private const string AcceptedTemplatePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusInvasionTutorialStationRun.asset";
        private const string AcceptedResultProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentation_OlympusInvasion.asset";
        private const string AcceptedLocalizationPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultLocalization_Core.asset";
        private const string AcceptedPresentationCatalogPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog.asset";
        private const string AcceptedResultDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusInvasion.asset";
        private const string AcceptedProgressionNodePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusInvasion.asset";
        private const string AcceptedProgressionGraphPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusInvasion.asset";
        private const string AcceptedIntroCinematicPath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening_OlympusBombingPrelude.asset";
        private const string AcceptedIntroTimelinePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";

        private static readonly string[] AuthoredAssetPaths =
        {
            StageDefinitionPath,
            PlayableStagePath,
            StageTemplatePath,
            ResultPresentationProfilePath,
            ResultLocalizationPath,
            ResultPresentationCatalogPath,
            ResultDefinitionPath,
            ProgressionNodePath,
            ProgressionGraphPath
        };

        private static readonly HashSet<string> ForbiddenAcceptedDependencies =
            new(StringComparer.Ordinal)
            {
                AcceptedRoutePath,
                AcceptedCorridorDefinitionPath,
                AcceptedStationDefinitionPath,
                AcceptedTemplatePath,
                AcceptedResultProfilePath,
                AcceptedLocalizationPath,
                AcceptedPresentationCatalogPath,
                AcceptedResultDefinitionPath,
                AcceptedProgressionNodePath,
                AcceptedProgressionGraphPath,
                AcceptedIntroCinematicPath,
                AcceptedIntroTimelinePath
            };

        [MenuItem("DimensionBrawl/B1-1/Validate Olympus Courtyard Drill Authored Pack")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
            Debug.Log(
                "[OlympusCourtyardDrillAuthoredPackValidator] AUTHORED_PACK_PASS "
                + $"playableStageId={PlayableStageId}, scene={ScenePath}.");
        }

        public static void RunBatchVerification()
        {
            try
            {
                ValidateOrThrow();
                Debug.Log(
                    "[OlympusCourtyardDrillAuthoredPackValidator] BATCH_AUTHORED_PACK_PASS "
                    + $"playableStageId={PlayableStageId}, scene={ScenePath}.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[OlympusCourtyardDrillAuthoredPackValidator] BATCH_AUTHORED_PACK_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateOrThrow()
        {
            Pack pack = LoadPack();
            ValidatePersistentAssetIdentity(pack);
            ValidateStageDefinition(pack.StageDefinition);
            ValidateRoute(pack);
            ValidateTemplate(pack.Template);
            ValidateResultSources(pack);
            ValidateProgression(pack);
            ValidateResultProgressionJoin(pack);
            ValidateGlobalIdentityUniqueness(pack);
            ValidateAcceptedSourceIsolation(pack);
            ValidateSceneContract(pack);
        }

        private static Pack LoadPack()
        {
            return new Pack(
                LoadRequired<SceneAsset>(ScenePath),
                LoadRequired<StageDefinitionProfile>(StageDefinitionPath),
                LoadRequired<PlayableStageDefinition>(PlayableStagePath),
                LoadRequired<LinearStageTemplateProfile>(StageTemplatePath),
                LoadRequired<StageResultPresentationProfile>(ResultPresentationProfilePath),
                LoadRequired<StageResultLocalizationTable>(ResultLocalizationPath),
                LoadRequired<StageResultPresentationCatalog>(ResultPresentationCatalogPath),
                LoadRequired<StageResultDefinition>(ResultDefinitionPath),
                LoadRequired<StageProgressionNode>(ProgressionNodePath),
                LoadRequired<StageProgressionGraph>(ProgressionGraphPath));
        }

        private static void ValidatePersistentAssetIdentity(Pack pack)
        {
            RequireExactAssetPath(pack.SceneAsset, ScenePath, "compact scene");
            RequireExactAssetPath(pack.StageDefinition, StageDefinitionPath, "stage definition");
            RequireExactAssetPath(pack.Route, PlayableStagePath, "playable stage");
            RequireExactAssetPath(pack.Template, StageTemplatePath, "stage template");
            RequireExactAssetPath(
                pack.PresentationProfile,
                ResultPresentationProfilePath,
                "result presentation profile");
            RequireExactAssetPath(pack.Localization, ResultLocalizationPath, "result localization");
            RequireExactAssetPath(
                pack.PresentationCatalog,
                ResultPresentationCatalogPath,
                "result presentation catalog");
            RequireExactAssetPath(pack.ResultDefinition, ResultDefinitionPath, "result definition");
            RequireExactAssetPath(pack.ProgressionNode, ProgressionNodePath, "progression node");
            RequireExactAssetPath(pack.ProgressionGraph, ProgressionGraphPath, "progression graph");

            UnityEngine.Object[] authoredObjects =
            {
                pack.StageDefinition,
                pack.Route,
                pack.Template,
                pack.PresentationProfile,
                pack.Localization,
                pack.PresentationCatalog,
                pack.ResultDefinition,
                pack.ProgressionNode,
                pack.ProgressionGraph
            };
            var identities = new HashSet<int>();
            for (int i = 0; i < authoredObjects.Length; i++)
            {
                UnityEngine.Object authored = authoredObjects[i];
                Require(authored != null && EditorUtility.IsPersistent(authored),
                    $"Authored pack object {i} must be a persistent asset.");
                Require(identities.Add(authored.GetInstanceID()),
                    $"Authored pack object {i} aliases another exact asset.");
            }
        }

        private static void ValidateStageDefinition(StageDefinitionProfile definition)
        {
            RequireEqual(definition.StageId, StageId, "stageDefinition.stageId");
            RequireEqual(definition.ChapterId, ChapterId, "stageDefinition.chapterId");
            RequireEqual(NormalizePath(definition.MapScenePath), ScenePath,
                "stageDefinition.mapScenePath");
            Require(string.IsNullOrWhiteSpace(definition.PreviousStageId)
                    && string.IsNullOrWhiteSpace(definition.NextStageId),
                "The isolated stage definition must not claim product succession.");
            RequireEqual(definition.MapRootName, MapRootName, "stageDefinition.mapRootName");
            RequireEqual(
                definition.MapContentRootName,
                MapContentRootName,
                "stageDefinition.mapContentRootName");
            RequireEqual(definition.LayoutId, LayoutId, "stageDefinition.layoutId");
            Require(definition.MapScale == Vector3.one,
                "The isolated stage definition map scale must be exactly one.");
            Require(definition.CutsceneHandoffCount == 0,
                "B1-1 has typed-absent story presentation and cannot author a cutscene handoff.");
            Require(definition.AnchorCount == 3,
                "The courtyard drill requires exactly player, terminal boss, and Rifle Crossfire Add anchors.");
            Require(definition.SpawnCount == 3,
                "The courtyard drill requires exactly three spawn rows.");
            Require(definition.RuntimeStateCount == 1,
                "The courtyard drill requires exactly one terminal runtime-state row.");
            Require(definition.SourceReferenceCount == 1,
                "The courtyard drill requires exactly one local provenance row.");

            var anchorIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < definition.AnchorCount; i++)
            {
                StageDefinitionProfile.AnchorRef anchor = definition.GetAnchor(i);
                Require(!string.IsNullOrWhiteSpace(anchor.AnchorId)
                        && string.Equals(anchor.GroupId, SpawnAnchorGroupId,
                            StringComparison.Ordinal)
                        && anchorIds.Add(anchor.AnchorId)
                        && IsFinite(anchor.ExpectedPosition)
                        && IsFinite(anchor.ExpectedEuler),
                    $"Stage definition anchor {i} has missing, duplicate, or non-finite authored identity.");
            }

            ValidateExpectedDefinitionAnchor(
                definition,
                PlayerAnchorId,
                PlayerPositionId,
                new Vector3(0f, 0f, -4.5f),
                Vector3.zero);
            ValidateExpectedDefinitionAnchor(
                definition,
                BossAnchorId,
                BossPositionId,
                new Vector3(0f, 0f, 3.6f),
                new Vector3(0f, 180f, 0f));
            ValidateExpectedDefinitionAnchor(
                definition,
                AddAnchorId,
                AddPositionId,
                new Vector3(-5f, 0f, 1.5f),
                new Vector3(0f, 135f, 0f));

            var spawnIds = new HashSet<string>(StringComparer.Ordinal);
            var positionIds = new HashSet<int>();
            int playerSpawnCount = 0;
            int bossSpawnCount = 0;
            int addSpawnCount = 0;
            StageDefinitionProfile.SpawnRef rangedAdd = default;
            for (int i = 0; i < definition.SpawnCount; i++)
            {
                StageDefinitionProfile.SpawnRef spawn = definition.GetSpawn(i);
                Require(!string.IsNullOrWhiteSpace(spawn.SpawnId)
                        && spawnIds.Add(spawn.SpawnId)
                        && spawn.PositionId > 0
                        && positionIds.Add(spawn.PositionId)
                        && !string.IsNullOrWhiteSpace(spawn.AnchorId)
                        && anchorIds.Contains(spawn.AnchorId)
                        && spawn.AuthoredCount > 0
                        && float.IsFinite(spawn.AuthoredDelaySeconds)
                        && spawn.AuthoredDelaySeconds >= 0f,
                    $"Stage definition spawn {i} has invalid identity, count, delay, or anchor ownership.");

                switch (spawn.SpawnKind)
                {
                    case StageSpawnKind.Player:
                        playerSpawnCount++;
                        RequireEqual(spawn.SpawnId, PlayerSpawnId, "player spawnId");
                        RequireEqual(spawn.AnchorId, PlayerAnchorId, "player spawn anchorId");
                        RequireEqual(spawn.PayloadId, PlayerPayloadId, "player spawn payloadId");
                        Require(spawn.PositionId == PlayerPositionId,
                            "Player spawn positionId is invalid.");
                        Require(spawn.PayloadArchetype == null
                                && spawn.AuthoredCount == 1
                                && Mathf.Approximately(spawn.AuthoredDelaySeconds, 0f),
                            "Player spawn archetype/count/delay contract is stale.");
                        break;
                    case StageSpawnKind.Boss:
                        bossSpawnCount++;
                        RequireEqual(spawn.SpawnId, BossSpawnId, "terminal boss spawnId");
                        RequireEqual(spawn.AnchorId, BossAnchorId, "boss spawn anchorId");
                        RequireEqual(spawn.PayloadId, BossPayloadId, "boss spawn payloadId");
                        Require(spawn.PositionId == BossPositionId,
                            "Terminal boss spawn positionId is invalid.");
                        Require(spawn.PayloadArchetype == null
                                && spawn.AuthoredCount == 1
                                && Mathf.Approximately(spawn.AuthoredDelaySeconds, 0f),
                            "Terminal boss spawn archetype/count/delay contract is stale.");
                        break;
                    case StageSpawnKind.Add:
                        addSpawnCount++;
                        rangedAdd = spawn;
                        break;
                }
            }

            Require(playerSpawnCount == 1 && bossSpawnCount == 1 && addSpawnCount == 1,
                "The courtyard drill must author exactly one Player, one terminal Boss, and one Add row.");
            RequireEqual(rangedAdd.SpawnId, AddSpawnId, "Rifle Crossfire Add spawnId");
            RequireEqual(rangedAdd.AnchorId, AddAnchorId, "Rifle Crossfire Add anchorId");
            RequireEqual(rangedAdd.PayloadId, AddPayloadId, "Rifle Crossfire Add payloadId");
            Require(rangedAdd.PositionId == AddPositionId && rangedAdd.AuthoredCount == 1,
                "Rifle Crossfire Add must own exact position 1301 and one authored instance.");
            Require(Mathf.Approximately(rangedAdd.AuthoredDelaySeconds, 0.6f),
                "Rifle Crossfire Add authored delay must remain 0.6 seconds.");
            Require(rangedAdd.PayloadArchetype != null
                    && string.Equals(
                        AssetDatabase.GetAssetPath(rangedAdd.PayloadArchetype),
                        RangedAddArchetypePath,
                        StringComparison.Ordinal),
                "Rifle Crossfire Add must use the reviewed ranged enemy archetype.");

            StageDefinitionProfile.RuntimeStateRef terminal = definition.GetRuntimeState(0);
            RequireEqual(terminal.StateId, TerminalRuntimeStateId,
                "terminal runtime-state stateId");
            Require(terminal.StateKind == StageRuntimeStateKind.StageClear
                    && terminal.PositionId == TerminalRuntimeStatePositionId
                    && string.IsNullOrWhiteSpace(terminal.AnchorId),
                "Terminal runtime-state kind/position/anchor contract is stale.");
            RequireEqual(terminal.ConditionId, TerminalConditionId,
                "terminal runtime-state conditionId");
            RequireEqual(
                NormalizePath(definition.GetSourceReference(0).SourcePath),
                ScenePath,
                "stageDefinition local source-reference path");
        }

        private static void ValidateRoute(Pack pack)
        {
            PlayableStageDefinition route = pack.Route;
            Require(route.SchemaVersion == 1, "Courtyard route schemaVersion must be 1.");
            RequireEqual(route.PlayableStageId, PlayableStageId, "route.playableStageId");
            Require(route.RouteRevision == 1, "Courtyard route revision must be 1.");
            Require(route.SceneSegmentCount == 1,
                "Courtyard route must contain exactly one Entry|Terminal segment.");
            Require(route.TerminalActionCount == 3,
                "Courtyard route must contain exactly Replay, Retry, and Lobby actions.");

            StageSceneSegmentRef segment = route.GetSceneSegment(0);
            Require(segment != null && ReferenceEquals(segment.StageDefinition, pack.StageDefinition),
                "Courtyard route must reference the exact isolated stage definition.");
            RequireEqual(segment.SegmentId, RouteSegmentId, "route segmentId");
            Require(segment.SequenceIndex == 0, "Courtyard route segment sequence must be zero.");
            RequireEqual(segment.EntryConditionId, RunEntryConditionId, "route entryConditionId");
            Require(segment.EntryConditionKind
                    == StageSegmentConditionKind.RunEntrySnapshotValidatedAndFirstSegmentActivated,
                "Courtyard route entry condition kind is invalid.");
            RequireEqual(segment.ExitConditionId, TerminalConditionId, "route exitConditionId");
            Require(segment.ExitConditionKind
                    == StageSegmentConditionKind
                        .StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched,
                "Courtyard route final condition kind is invalid.");
            Require(segment.HandoffPolicy == StageSceneHandoffPolicy.ReturnToOwner
                    && segment.SuccessorKind == StageSegmentSuccessorKind.None
                    && segment.DestinationSceneKind == StageSegmentDestinationSceneKind.None
                    && segment.TransitionTokenKind == StageSegmentTransitionTokenKind.None
                    && segment.LoaderGenerationKind == StageSegmentLoaderGenerationKind.None
                    && segment.NavigationAuthorityKind == StageSegmentNavigationAuthorityKind.None
                    && segment.ReturnOwnerKind == StageSegmentReturnOwnerKind.P1AStageRunRouteOwner
                    && segment.ReturnOwnerReceiptPolicy
                        == StageReturnOwnerReceiptPolicy
                            .ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented,
                "Courtyard route must be an exact one-row ReturnToOwner route without handoff evidence.");
            ValidateAbsentPresentation(segment.EntryPresentation, "entry");
            ValidateAbsentPresentation(segment.ExitPresentation, "exit");

            ValidateAction(route, ReplayActionId, StageRouteActionKind.Replay,
                PlayableStageId, StageUiRouteId.None, StageRouteOutcome.Clear);
            ValidateAction(route, RetryActionId, StageRouteActionKind.Retry,
                PlayableStageId, StageUiRouteId.None, StageRouteOutcome.Fail);
            ValidateAction(route, LobbyActionId, StageRouteActionKind.UIRoute,
                string.Empty, StageUiRouteId.Lobby,
                StageRouteOutcome.Clear | StageRouteOutcome.Fail);
            ValidateTerminalPolicy(route.TerminalResolutionPolicy);

            RequireCanonicalDigest(
                route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                "terminal policy stored digest");
            RequireEqual(
                route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                route.TerminalResolutionPolicy.ComputeCanonicalDigest(),
                "terminal policy recomputed digest");
            RequireCanonicalDigest(route.CanonicalRouteDigest, "route stored digest");
            RequireEqual(route.CanonicalRouteDigest, route.ComputeCanonicalRouteDigest(),
                "route recomputed digest");
            Require(StageRunRouteSnapshot.TryCreate(
                    route,
                    out StageRunRouteSnapshot snapshot,
                    out string routeError),
                "Courtyard route snapshot is invalid: " + routeError);
            Require(snapshot.SegmentCount == 1
                    && snapshot.GetSegmentRoles(0)
                        == (StageRunSegmentRole.Entry | StageRunSegmentRole.Terminal)
                    && snapshot.IsEntrySegment(0)
                    && snapshot.IsTerminalSegment(0)
                    && snapshot.TutorialFactRequirement == StageRunTutorialFactRequirement.None,
                "Courtyard route snapshot is not an exact tutorial-free Entry|Terminal route.");
            RequireEqual(snapshot.GetSegment(0).ScenePath, ScenePath,
                "route snapshot scene path");

            StageReferenceBlock references = route.ReferenceBlock;
            Require(references != null && references.IsPresent
                    && references.SchemaVersion == 1
                    && references.Revision == 1,
                "Courtyard route reference block must be present at revision 1.");
            Require(ReferenceEquals(references.StageTemplate, pack.Template),
                "Courtyard route must reference the exact isolated template.");
            Require(references.BriefingSchemaVersion == 1 && references.BriefingRevision == 1,
                "Courtyard briefing must be revision 1.");
            Require(references.StoryEntryDisposition == StageReferenceDisposition.None
                    && references.StoryExitDisposition
                        == StageReferenceDisposition.NoFinalSegmentExitPresentationAuthored,
                "Courtyard story presentation must remain typed absent.");
            Require(references.ResultDefinitionDisposition
                    == StageReferenceDisposition.NotAuthoredForCurrentSchema
                    && references.ProgressionNodeDisposition
                        == StageReferenceDisposition.NotAuthoredForCurrentSchema,
                "Result/progression sidecars must remain outside the route reference-block schema.");
            RequireCanonicalDigest(references.CanonicalReferenceDigest,
                "reference stored digest");
            RequireEqual(references.CanonicalReferenceDigest,
                route.ComputeCanonicalReferenceDigest(), "reference recomputed digest");
            RequireCanonicalDigest(references.CanonicalBriefingDigest,
                "briefing stored digest");
            Require(route.TryComputeCanonicalBriefingDigest(
                    out string briefingDigest,
                    out StageBriefingBuildRejectReason briefingReject),
                $"Courtyard briefing digest rejected {briefingReject}.");
            RequireEqual(references.CanonicalBriefingDigest, briefingDigest,
                "briefing recomputed digest");
            Require(route.TryCreateBriefingReadModel(
                    out StageBriefingReadModel briefing,
                    out StageBriefingBuildRejectReason readModelReject),
                $"Courtyard briefing read model rejected {readModelReject}.");
            Require(briefing.SegmentCount == 1 && briefing.ActionCount == 3,
                "Courtyard briefing must expose one segment and three terminal actions.");
        }

        private static void ValidateTemplate(LinearStageTemplateProfile template)
        {
            Require(template.TemplateSchemaVersion == 1 && template.TemplateRevision == 1,
                "Courtyard template must be schema/revision 1.");
            RequireEqual(template.StageTemplateId, TemplateId, "template.stageTemplateId");
            Require(template.TemplateKind == LinearStageTemplateKind.StandardStoryRun,
                "Courtyard template kind must be StandardStoryRun.");
            Require(template.TitleDisposition == StageBriefingValueDisposition.Present
                    && template.ObjectiveDisposition == StageBriefingValueDisposition.Present
                    && template.CombatLessonDisposition == StageBriefingValueDisposition.Present,
                "Courtyard template must expose truthful title, objective, and combat lesson.");
            Require(template.RewardPreviewDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                    && string.IsNullOrWhiteSpace(template.RewardPreview),
                "Courtyard template cannot invent a reward preview.");
            Require(template.SegmentCount == 0,
                "Courtyard template cannot reuse a legacy prototype segment array.");
            Require(template.CanonicalRouteSegmentCount == 1,
                "Courtyard template must describe one canonical route segment.");

            StageTemplateRouteSegmentRef segment = template.GetCanonicalRouteSegment(0);
            Require(segment != null, "Courtyard canonical template segment is missing.");
            RequireEqual(segment.TemplateSegmentId, TemplateSegmentId,
                "template segmentId");
            RequireEqual(segment.RouteSegmentId, RouteSegmentId,
                "template routeSegmentId");
            Require(segment.RouteSequenceIndex == 0 && segment.PocketCount == 1,
                "Courtyard template segment must own one sequence-zero pocket.");
            StageTemplatePocketRef pocket = segment.GetPocket(0);
            Require(pocket != null, "Courtyard terminal-combat template pocket is missing.");
            RequireEqual(pocket.PocketId, TemplatePocketId, "template pocketId");
            Require(pocket.SequenceIndex == 0
                    && pocket.ObjectiveKind == StageTemplatePocketObjectiveKind.DefeatBoss
                    && pocket.CurrentExecutionOwnerDisposition
                        == StageTemplateCurrentExecutionOwnerDisposition.ExistingSceneOwner
                    && pocket.P1CAdmissionDisposition
                        == StageTemplateP1CAdmissionDisposition.NotAdmitted
                    && pocket.SourceDisposition == StageTemplateSourceDisposition.RouteConditionBoundary
                    && string.Equals(pocket.SourceSemanticId, TerminalConditionId,
                        StringComparison.Ordinal)
                    && pocket.SourceRevision == 1
                    && string.IsNullOrWhiteSpace(pocket.SourceSemanticDigest)
                    && pocket.EnemyRoleCount == 0,
                "Courtyard terminal-combat pocket does not truthfully bind the route-owned boss terminal.");
            RequireCanonicalDigest(template.CanonicalTemplateDigest,
                "template stored digest");
            RequireEqual(template.CanonicalTemplateDigest,
                template.ComputeCanonicalTemplateDigest(), "template recomputed digest");
        }

        private static void ValidateResultSources(Pack pack)
        {
            StageResultLocalizationTable localization = pack.Localization;
            Require(localization.SchemaVersion == 1 && localization.TableRevision == 1,
                "Courtyard result localization must be schema/revision 1.");
            RequireEqual(localization.TableId, ResultLocalizationId,
                "result localization tableId");
            Require(localization.TryValidate(out string localizationError),
                "Courtyard result localization is invalid: " + localizationError);
            RequireCanonicalDigest(localization.ComputeCanonicalDigest(),
                "result localization canonical digest");

            StageResultPresentationProfile profile = pack.PresentationProfile;
            Require(profile.SchemaVersion == 1 && profile.ProfileRevision == 1
                    && profile.SupportedRunSchemaVersion == 1,
                "Courtyard result profile must be schema/revision/run-schema 1.");
            RequireEqual(profile.ProfileId, ResultPresentationProfileId,
                "result presentation profileId");
            RequireEqual(profile.PlayableStageId, PlayableStageId,
                "result presentation playableStageId");
            SerializedProperty stageCode = new SerializedObject(profile).FindProperty("stageCode");
            Require(stageCode != null && string.Equals(stageCode.stringValue, ResultStageCode,
                    StringComparison.Ordinal),
                "Courtyard result profile stageCode must be 01-2.");
            Require(profile.TryValidate(localization, out string profileError),
                "Courtyard result profile is invalid: " + profileError);
            RequireCanonicalDigest(profile.ComputeCanonicalDigest(),
                "result profile canonical digest");

            StageResultPresentationCatalog catalog = pack.PresentationCatalog;
            Require(catalog.SchemaVersion == 1 && catalog.CatalogRevision == 1,
                "Courtyard result presentation catalog must be schema/revision 1.");
            RequireEqual(catalog.CatalogId, ResultPresentationCatalogId,
                "result presentation catalogId");
            Require(ReferenceEquals(catalog.LocalizationTable, localization)
                    && catalog.ProfileCount == 1
                    && ReferenceEquals(catalog.GetProfile(0), profile),
                "Courtyard result presentation catalog must own only its exact profile/localization.");
            Require(catalog.TryValidate(out string catalogError),
                "Courtyard result presentation catalog is invalid: " + catalogError);
            Require(catalog.TryValidateExactSources(
                    PlayableStageId,
                    profile,
                    localization,
                    out string exactSourceError),
                "Courtyard result presentation sources are not exact: " + exactSourceError);

            StageResultDefinition result = pack.ResultDefinition;
            Require(result.SchemaVersion == 1 && result.Revision == 1
                    && result.EvaluationContentRevision == 1
                    && result.PresentationBindingRevision == 1
                    && result.SupportedRunSchemaVersion == 1,
                "Courtyard result definition revisions are invalid.");
            RequireEqual(result.ResultDefinitionId, ResultDefinitionId,
                "resultDefinitionId");
            RequireEqual(result.PlayableStageId, PlayableStageId,
                "result definition playableStageId");
            Require(ReferenceEquals(result.PresentationProfile, profile)
                    && ReferenceEquals(result.LocalizationTable, localization)
                    && ReferenceEquals(result.CanonicalPresentationCatalog, catalog),
                "Courtyard result definition must retain exact isolated presentation sources.");
            Require(result.ActionMappingCount == 4,
                "Courtyard result definition must own exactly four outcome-specific action mappings.");
            ValidateResultActionMapping(
                result,
                0,
                StageRouteOutcome.Clear,
                ReplayActionId,
                ReplayActionLabelKey,
                StageResultActionPresentationRole.Primary,
                0);
            ValidateResultActionMapping(
                result,
                1,
                StageRouteOutcome.Clear,
                LobbyActionId,
                LobbyActionLabelKey,
                StageResultActionPresentationRole.Secondary,
                1);
            ValidateResultActionMapping(
                result,
                2,
                StageRouteOutcome.Fail,
                RetryActionId,
                RetryActionLabelKey,
                StageResultActionPresentationRole.Primary,
                0);
            ValidateResultActionMapping(
                result,
                3,
                StageRouteOutcome.Fail,
                LobbyActionId,
                LobbyActionLabelKey,
                StageResultActionPresentationRole.Secondary,
                1);

            var mappedOutcomeActions = new HashSet<string>(StringComparer.Ordinal);
            var actionOccurrenceCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < result.ActionMappingCount; i++)
            {
                StageResultActionPresentationMapping mapping = result.GetActionMapping(i);
                Require(mapping != null
                        && mappedOutcomeActions.Add(
                            $"{(int)mapping.Outcome}:{mapping.ActionId}"),
                    $"Courtyard result action mapping {i} is missing or duplicates an outcome/action pair.");
                actionOccurrenceCounts.TryGetValue(mapping.ActionId, out int occurrenceCount);
                actionOccurrenceCounts[mapping.ActionId] = occurrenceCount + 1;
            }
            Require(actionOccurrenceCounts.Count == 3
                    && actionOccurrenceCounts.TryGetValue(ReplayActionId, out int replayCount)
                    && replayCount == 1
                    && actionOccurrenceCounts.TryGetValue(RetryActionId, out int retryCount)
                    && retryCount == 1
                    && actionOccurrenceCounts.TryGetValue(LobbyActionId, out int lobbyCount)
                    && lobbyCount == 2,
                "Only the Lobby action may repeat, once for each Clear/Fail outcome.");
            Require(result.TryComputeCanonicalDigests(
                    out string evaluationDigest,
                    out string bindingDigest,
                    out string sourceDigest,
                    out string resultDigestError),
                "Courtyard result digests cannot be computed: " + resultDigestError);
            RequireEqual(result.EvaluationContentDigest, evaluationDigest,
                "result evaluation digest");
            RequireEqual(result.PresentationBindingDigest, bindingDigest,
                "result presentation binding digest");
            RequireEqual(result.PresentationSourceDigest, sourceDigest,
                "result presentation source digest");
            RequireCanonicalDigest(evaluationDigest, "result evaluation canonical digest");
            RequireCanonicalDigest(bindingDigest, "result binding canonical digest");
            RequireCanonicalDigest(sourceDigest, "result source canonical digest");
            Require(result.TryCreateSnapshot(out _, out string resultSnapshotError),
                "Courtyard result snapshot is invalid: " + resultSnapshotError);
        }

        private static void ValidateResultActionMapping(
            StageResultDefinition result,
            int index,
            StageRouteOutcome expectedOutcome,
            string expectedActionId,
            string expectedLabelKey,
            StageResultActionPresentationRole expectedRole,
            int expectedDisplayOrder)
        {
            StageResultActionPresentationMapping mapping = result.GetActionMapping(index);
            Require(mapping != null
                    && mapping.Outcome == expectedOutcome
                    && string.Equals(mapping.ActionId, expectedActionId,
                        StringComparison.Ordinal)
                    && string.Equals(mapping.LabelKey, expectedLabelKey,
                        StringComparison.Ordinal)
                    && mapping.Role == expectedRole
                    && mapping.DisplayOrder == expectedDisplayOrder,
                $"Courtyard result action mapping {index} must be exact: "
                + $"outcome={expectedOutcome}, actionId='{expectedActionId}', "
                + $"labelKey='{expectedLabelKey}', role={expectedRole}, "
                + $"displayOrder={expectedDisplayOrder}.");
        }

        private static void ValidateProgression(Pack pack)
        {
            StageProgressionNode node = pack.ProgressionNode;
            Require(node.SchemaVersion == 1 && node.Revision == 1
                    && node.ContentRevision == 1 && node.BindingRevision == 1,
                "Courtyard progression node revisions are invalid.");
            RequireEqual(node.ProgressionNodeId, ProgressionNodeId,
                "progressionNodeId");
            Require(node.PrerequisiteCount == 0 && node.RecommendedNextCount == 0,
                "B1-1 cannot author cross-stage availability edges before persistent progression.");
            RequireEqual(node.PlayableStageId, PlayableStageId,
                "progression node playableStageId");
            Require(node.RouteRevision == pack.Route.RouteRevision,
                "Progression node routeRevision is stale.");
            RequireEqual(node.CanonicalRouteDigest, pack.Route.CanonicalRouteDigest,
                "progression node route digest");
            RequireEqual(node.ProgressionGraphId, ProgressionGraphId,
                "progression node graphId");
            Require(node.ProgressionGraphRevision == 1,
                "Courtyard progression graph binding revision must be 1.");
            Require(node.TryComputeCanonicalDigests(
                    out string contentDigest,
                    out string bindingDigest,
                    out string nodeDigestError),
                "Courtyard progression node digests cannot be computed: " + nodeDigestError);
            RequireEqual(node.ContentDigest, contentDigest,
                "progression node content digest");
            RequireEqual(node.BindingDigest, bindingDigest,
                "progression node binding digest");
            RequireCanonicalDigest(contentDigest, "progression node content canonical digest");
            RequireCanonicalDigest(bindingDigest, "progression node binding canonical digest");
            Require(node.TryCreateSnapshot(out _, out string nodeSnapshotError),
                "Courtyard progression node snapshot is invalid: " + nodeSnapshotError);

            StageProgressionGraph graph = pack.ProgressionGraph;
            Require(graph.SchemaVersion == 1 && graph.Revision == 1,
                "Courtyard progression graph must be schema/revision 1.");
            RequireEqual(graph.ProgressionGraphId, ProgressionGraphId,
                "progressionGraphId");
            Require(graph.NodeCount == 1 && ReferenceEquals(graph.GetNode(0), node),
                "Courtyard progression graph must own only its exact isolated node.");
            Require(graph.TryComputeCanonicalDigest(
                    out string graphDigest,
                    out string graphDigestError),
                "Courtyard progression graph digest cannot be computed: " + graphDigestError);
            RequireEqual(graph.CanonicalDigest, graphDigest,
                "progression graph digest");
            RequireCanonicalDigest(graphDigest, "progression graph canonical digest");
            Require(graph.TryCreateSnapshot(out _, out string graphSnapshotError),
                "Courtyard progression graph snapshot is invalid: " + graphSnapshotError);
        }

        private static void ValidateResultProgressionJoin(Pack pack)
        {
            StageResultProgressionJoinBlock join = pack.Route.ResultProgressionJoin;
            Require(join != null && join.Present && join.SchemaVersion == 1 && join.Revision == 1,
                "Courtyard result/progression join must be present at revision 1.");
            Require(join.ResultDefinitionDisposition
                    == StageResultProgressionReferenceDisposition.Present
                    && join.ProgressionNodeDisposition
                        == StageResultProgressionReferenceDisposition.Present
                    && join.ProgressionGraphDisposition
                        == StageResultProgressionReferenceDisposition.Present,
                "Courtyard join must mark all exact sidecar references Present.");
            Require(ReferenceEquals(join.ResultDefinition, pack.ResultDefinition)
                    && ReferenceEquals(join.CanonicalPresentationCatalog,
                        pack.PresentationCatalog)
                    && ReferenceEquals(join.ProgressionNode, pack.ProgressionNode)
                    && ReferenceEquals(join.ProgressionGraph, pack.ProgressionGraph),
                "Courtyard join references are not the exact isolated asset graph.");
            Require(pack.Route.TryComputeResultProgressionJoinDigest(
                    out string joinDigest,
                    out string joinDigestError),
                "Courtyard join digest cannot be computed: " + joinDigestError);
            RequireEqual(join.CanonicalDigest, joinDigest,
                "result/progression join digest");
            RequireCanonicalDigest(joinDigest, "result/progression join canonical digest");
            Require(StageRunResultProgressionJoinSnapshot.TryCreate(
                    pack.Route,
                    out StageRunResultProgressionJoinSnapshot snapshot,
                    out string snapshotError),
                "Courtyard result/progression snapshot is invalid: " + snapshotError);
            Require(snapshot.TryValidateIntegrity(out string integrityError),
                "Courtyard result/progression integrity is invalid: " + integrityError);
        }

        private static void ValidateGlobalIdentityUniqueness(Pack pack)
        {
            RequireUniqueAssetIdentity(pack.StageDefinition, StageId,
                static asset => asset.StageId);
            RequireUniqueAssetIdentity(pack.Route, PlayableStageId,
                static asset => asset.PlayableStageId);
            RequireUniqueAssetIdentity(pack.Template, TemplateId,
                static asset => asset.StageTemplateId);
            RequireUniqueAssetIdentity(pack.PresentationProfile, ResultPresentationProfileId,
                static asset => asset.ProfileId);
            RequireUniqueAssetIdentity(pack.Localization, ResultLocalizationId,
                static asset => asset.TableId);
            RequireUniqueAssetIdentity(pack.PresentationCatalog, ResultPresentationCatalogId,
                static asset => asset.CatalogId);
            RequireUniqueAssetIdentity(pack.ResultDefinition, ResultDefinitionId,
                static asset => asset.ResultDefinitionId);
            RequireUniqueAssetIdentity(pack.ProgressionNode, ProgressionNodeId,
                static asset => asset.ProgressionNodeId);
            RequireUniqueAssetIdentity(pack.ProgressionGraph, ProgressionGraphId,
                static asset => asset.ProgressionGraphId);
        }

        private static void ValidateAcceptedSourceIsolation(Pack pack)
        {
            Require(!ReferenceEquals(pack.Route, LoadRequired<PlayableStageDefinition>(AcceptedRoutePath)),
                "Courtyard route aliases the accepted Olympus route.");
            Require(!ReferenceEquals(pack.StageDefinition,
                    LoadRequired<StageDefinitionProfile>(AcceptedCorridorDefinitionPath))
                    && !ReferenceEquals(pack.StageDefinition,
                        LoadRequired<StageDefinitionProfile>(AcceptedStationDefinitionPath)),
                "Courtyard stage definition aliases an accepted Olympus definition.");
            Require(!ReferenceEquals(pack.Template,
                    LoadRequired<LinearStageTemplateProfile>(AcceptedTemplatePath)),
                "Courtyard template aliases the accepted Olympus template.");
            Require(!ReferenceEquals(pack.PresentationProfile,
                    LoadRequired<StageResultPresentationProfile>(AcceptedResultProfilePath))
                    && !ReferenceEquals(pack.Localization,
                        LoadRequired<StageResultLocalizationTable>(AcceptedLocalizationPath))
                    && !ReferenceEquals(pack.PresentationCatalog,
                        LoadRequired<StageResultPresentationCatalog>(
                            AcceptedPresentationCatalogPath))
                    && !ReferenceEquals(pack.ResultDefinition,
                        LoadRequired<StageResultDefinition>(AcceptedResultDefinitionPath))
                    && !ReferenceEquals(pack.ProgressionNode,
                        LoadRequired<StageProgressionNode>(AcceptedProgressionNodePath))
                    && !ReferenceEquals(pack.ProgressionGraph,
                        LoadRequired<StageProgressionGraph>(AcceptedProgressionGraphPath)),
                "Courtyard result/progression sources alias accepted Olympus sidecars.");

            var dependencyRoots = new string[AuthoredAssetPaths.Length + 1];
            Array.Copy(AuthoredAssetPaths, dependencyRoots, AuthoredAssetPaths.Length);
            dependencyRoots[dependencyRoots.Length - 1] = ScenePath;
            string[] dependencies = AssetDatabase.GetDependencies(dependencyRoots, true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependency = NormalizePath(dependencies[i]);
                Require(!ForbiddenAcceptedDependencies.Contains(dependency),
                    $"Courtyard authored pack retains forbidden accepted source dependency: {dependency}");
            }
        }

        private static void ValidateSceneContract(Pack pack)
        {
            RefuseDirtyOpenScenes();
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Require(scene.IsValid() && scene.isLoaded && !scene.isDirty,
                    "Courtyard scene did not open cleanly for read-only validation.");
                ValidateNoMissingScripts(scene);

                StageDefinitionSceneBinding binding = RequireSingleSceneComponent<
                    StageDefinitionSceneBinding>(scene);
                ValidateSceneBinding(scene, binding, pack.StageDefinition);

                CombatEncounterController encounter =
                    RequireSingleSceneComponent<CombatEncounterController>(scene);
                OneRowStageRunBootstrap bootstrap =
                    RequireSingleSceneComponent<OneRowStageRunBootstrap>(scene);
                OneRowStageRunFactAdapter factAdapter =
                    RequireSingleSceneComponent<OneRowStageRunFactAdapter>(scene);
                OneRowStageRunResultPresenter resultPresenter =
                    RequireSingleSceneComponent<OneRowStageRunResultPresenter>(scene);
                StageCountOneEncounterExecutor addExecutor =
                    RequireSingleSceneComponent<StageCountOneEncounterExecutor>(scene);
                OneRowCombatHudBinder hudBinder =
                    RequireSingleSceneComponent<OneRowCombatHudBinder>(scene);
                PlayerCombatTargetSelector targetSelector =
                    RequireSingleSceneComponent<PlayerCombatTargetSelector>(scene);

                Require(encounter.isActiveAndEnabled && encounter.UsesCoordinatedTerminalResolution,
                    "Courtyard terminal encounter must be active and coordinated.");
                Require(bootstrap.isActiveAndEnabled && factAdapter.isActiveAndEnabled
                        && resultPresenter.isActiveAndEnabled && addExecutor.isActiveAndEnabled
                        && hudBinder.isActiveAndEnabled,
                    "Courtyard neutral runtime owners must all be active and enabled.");

                CombatHealth playerHealth = encounter.PlayerHealth;
                CombatHealth bossHealth = encounter.EnemyHealth;
                Require(playerHealth != null && bossHealth != null
                        && !ReferenceEquals(playerHealth, bossHealth)
                        && playerHealth.Team == DamageTeam.Player
                        && bossHealth.Team == DamageTeam.Enemy
                        && playerHealth.gameObject.scene.handle == scene.handle
                        && bossHealth.gameObject.scene.handle == scene.handle
                        && playerHealth.isActiveAndEnabled
                        && bossHealth.isActiveAndEnabled,
                    "Courtyard encounter must own distinct active scene-local Player and terminal Boss health.");
                CombatHealth[] sceneHealth = CollectSceneComponents<CombatHealth>(scene);
                Require(sceneHealth.Length == 2,
                    "The Rifle Crossfire Add must be runtime-owned; the authored scene may contain only Player and terminal Boss health.");

                ValidateExactSerializedReference(bootstrap, "playableStageDefinition",
                    pack.Route, "bootstrap route");
                ValidateExactSerializedReference(bootstrap, "encounter",
                    encounter, "bootstrap encounter");
                ValidateExactSerializedReference(bootstrap, "factAdapter",
                    factAdapter, "bootstrap fact adapter");
                ValidateExactSerializedReference(bootstrap, "resultPresenter",
                    resultPresenter, "bootstrap result presenter");

                MonoBehaviour resultSurface = RequireSingleSceneInterface<ICombatSessionOverlay>(scene);
                MonoBehaviour resultOverlay = RequireSingleSceneInterface<IStageRunResultOverlay>(scene);
                ValidateExactSerializedReference(factAdapter, "encounter",
                    encounter, "fact-adapter encounter");
                ValidateExactSerializedReference(factAdapter, "playerHealth",
                    playerHealth, "fact-adapter player health");
                ValidateExactSerializedReference(factAdapter, "resultSurfaceBehaviour",
                    resultSurface, "fact-adapter result surface");
                ValidateOptionalPlayerFactSources(factAdapter, playerHealth.transform.root);

                ValidateExactSerializedReference(resultPresenter, "encounter",
                    encounter, "result-presenter encounter");
                ValidateExactSerializedReference(resultPresenter, "factAdapter",
                    factAdapter, "result-presenter fact adapter");
                ValidateExactSerializedReference(resultPresenter, "resultSurfaceBehaviour",
                    resultSurface, "result-presenter result surface");
                ValidateExactSerializedReference(resultPresenter, "resultOverlayBehaviour",
                    resultOverlay, "result-presenter result overlay");

                Require(ReferenceEquals(addExecutor.SceneBinding, binding)
                        && addExecutor.ActivationKind == StageEncounterActivationKind.SceneReady
                        && addExecutor.RequiresActiveStageRun
                        && addExecutor.CancelsOnTerminalEncounter,
                    "Courtyard Add executor must use exact SceneReady/run-required/terminal-cancel ownership.");
                Require(ReferenceEquals(targetSelector.SelfHealth, playerHealth)
                        && targetSelector.TargetCandidateCount == 1
                        && targetSelector.ContainsAuthoredTargetCandidate(bossHealth)
                        && targetSelector.IncludesActiveHostileSummons,
                    "Player targeting must preserve the authored terminal Boss and admit runtime Add registration.");

                ValidateHudBinder(hudBinder, encounter, playerHealth, bossHealth, resultSurface);
                ValidateSceneServiceCounts(scene);
                Require(CollectSceneComponents<OlympusCorridorCombatFlowController>(scene).Length == 0
                        && CollectSceneComponents<OlympusStationRunFactCollector>(scene).Length == 0
                        && CollectSceneComponents<OlympusStationCombatResultPresenter>(scene).Length == 0,
                    "Courtyard scene cannot copy Olympus route/fact/result controllers.");
                Require(!scene.isDirty,
                    "Read-only courtyard scene validation dirtied the scene.");
            }
            finally
            {
                if (setup != null && setup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }
        }

        private static void ValidateSceneBinding(
            Scene scene,
            StageDefinitionSceneBinding binding,
            StageDefinitionProfile definition)
        {
            Require(binding != null && binding.gameObject.activeInHierarchy
                    && ReferenceEquals(binding.StageDefinition, definition),
                "Courtyard scene binding does not own the exact isolated stage definition.");
            RequireEqual(binding.gameObject.name, definition.MapRootName,
                "scene binding root name");
            Require(binding.MapRoot != null
                    && binding.MapRoot.gameObject.scene.handle == scene.handle
                    && string.Equals(binding.MapRoot.name, definition.MapContentRootName,
                        StringComparison.Ordinal)
                    && Approximately(binding.MapRoot.localScale, definition.MapScale),
                "Courtyard scene map root name, scene ownership, or scale is stale.");
            Require(binding.CutscenePortCount == 0,
                "Courtyard scene cannot bind a cutscene port in B1-1.");
            Require(binding.AnchorPointCount == definition.AnchorCount,
                "Courtyard scene/definition anchor counts disagree.");

            var boundAnchors = new HashSet<StageAnchorPoint>();
            var boundAnchorIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < binding.AnchorPointCount; i++)
            {
                StageAnchorPoint sceneAnchor = binding.GetAnchorPoint(i);
                Require(sceneAnchor != null
                        && sceneAnchor.gameObject.scene.handle == scene.handle
                        && sceneAnchor.gameObject.activeInHierarchy
                        && sceneAnchor.transform.IsChildOf(binding.transform)
                        && boundAnchors.Add(sceneAnchor)
                        && !string.IsNullOrWhiteSpace(sceneAnchor.AnchorId)
                        && boundAnchorIds.Add(sceneAnchor.AnchorId),
                    $"Courtyard scene anchor {i} is null, foreign, inactive, duplicated, or outside its binding.");

                int matchCount = 0;
                StageDefinitionProfile.AnchorRef definitionAnchor = default;
                for (int definitionIndex = 0;
                     definitionIndex < definition.AnchorCount;
                     definitionIndex++)
                {
                    StageDefinitionProfile.AnchorRef candidate = definition.GetAnchor(definitionIndex);
                    if (string.Equals(candidate.AnchorId, sceneAnchor.AnchorId,
                        StringComparison.Ordinal))
                    {
                        definitionAnchor = candidate;
                        matchCount++;
                    }
                }

                Require(matchCount == 1,
                    $"Scene anchor '{sceneAnchor.AnchorId}' must resolve once in its definition.");
                RequireEqual(sceneAnchor.GroupId, definitionAnchor.GroupId,
                    $"scene anchor {sceneAnchor.AnchorId} groupId");
                ResolveBindingRootLocalPose(
                    binding.transform,
                    sceneAnchor.transform,
                    out Vector3 localPosition,
                    out Quaternion localRotation);
                Require(Approximately(localPosition, definitionAnchor.ExpectedPosition)
                        && ApproximatelyEuler(localRotation.eulerAngles,
                            definitionAnchor.ExpectedEuler),
                    $"Scene anchor '{sceneAnchor.AnchorId}' pose disagrees with its definition.");
            }

            ValidateExpectedSceneAnchor(binding, PlayerAnchorId,
                StageSpawnKind.Player, PlayerPositionId);
            ValidateExpectedSceneAnchor(binding, BossAnchorId,
                StageSpawnKind.Boss, BossPositionId);
            ValidateExpectedSceneAnchor(binding, AddAnchorId,
                StageSpawnKind.Add, AddPositionId);
        }

        private static void ValidateOptionalPlayerFactSources(
            OneRowStageRunFactAdapter adapter,
            Transform playerRoot)
        {
            SerializedObject serialized = new(adapter);
            ValidateOptionalSingleton<PlayerActionController>(
                serialized,
                "playerActionController",
                playerRoot,
                "player action");
            ValidateOptionalSingleton<SummonEnergyLadder>(
                serialized,
                "summonEnergyLadder",
                playerRoot,
                "summon energy");
            ValidateOptionalSingleton<PlayerSummonSlot1Action>(
                serialized,
                "summonSlot1Action",
                playerRoot,
                "summon slot 1");

            SerializedProperty support = serialized.FindProperty("supportSummonActions");
            Require(support != null && support.isArray,
                "Fact adapter support-summon source array is missing.");
            var configured = new HashSet<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < support.arraySize; i++)
            {
                PlayerSupportSummonSlotAction action =
                    support.GetArrayElementAtIndex(i).objectReferenceValue
                        as PlayerSupportSummonSlotAction;
                Require(action != null && action.transform.root == playerRoot
                        && action.isActiveAndEnabled && configured.Add(action),
                    $"Fact adapter support-summon source {i} is null, foreign, inactive, or duplicated.");
            }

            PlayerSupportSummonSlotAction[] candidates =
                playerRoot.GetComponentsInChildren<PlayerSupportSummonSlotAction>(true);
            int liveCount = 0;
            for (int i = 0; i < candidates.Length; i++)
            {
                PlayerSupportSummonSlotAction candidate = candidates[i];
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    continue;
                }

                liveCount++;
                Require(configured.Contains(candidate),
                    "Fact adapter omits a live player support-summon source.");
            }

            Require(liveCount == configured.Count,
                "Fact adapter support-summon source set is not exact.");
        }

        private static void ValidateHudBinder(
            OneRowCombatHudBinder binder,
            CombatEncounterController encounter,
            CombatHealth player,
            CombatHealth boss,
            MonoBehaviour resultSurface)
        {
            SerializedObject serialized = new(binder);
            ValidateSerializedReferenceType<CombatHudPresenter>(serialized,
                "hudPresenter", "HUD presenter");
            ValidateSerializedReferenceType<CombatHudInputBridge>(serialized,
                "inputBridge", "HUD input bridge");
            ValidateSerializedReferenceType<CombatHudVirtualJoystick>(serialized,
                "moveJoystick", "HUD movement joystick");
            ValidateExactSerializedReference(serialized, "sessionOverlayBehaviour",
                resultSurface, "HUD session surface");
            ValidateExactSerializedReference(serialized, "encounterController",
                encounter, "HUD encounter");
            ValidateExactSerializedReference(serialized, "playerHealth",
                player, "HUD player health");
            ValidateExactSerializedReference(serialized, "bossHealth",
                boss, "HUD terminal boss health");
            PlayerMovementController movement = ValidateSerializedReferenceType<
                PlayerMovementController>(serialized, "movementController", "HUD movement");
            PlayerActionController action = ValidateSerializedReferenceType<
                PlayerActionController>(serialized, "actionController", "HUD action");
            Require(movement.transform.root == player.transform.root
                    && action.transform.root == player.transform.root,
                "HUD player controls must belong to the exact terminal encounter player root.");
        }

        private static void ValidateSceneServiceCounts(Scene scene)
        {
            Require(CountLiveSceneBehaviours<Camera>(scene) == 1,
                "Courtyard scene must own exactly one active camera.");
            Require(CountLiveSceneBehaviours<AudioListener>(scene) == 1,
                "Courtyard scene must own exactly one active AudioListener.");
            Require(CountLiveSceneBehaviours<EventSystem>(scene) == 1,
                "Courtyard scene must own exactly one active EventSystem.");
        }

        private static void ValidateNoMissingScripts(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            int missing = 0;
            for (int i = 0; i < roots.Length; i++)
            {
                missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(roots[i]);
            }

            Require(missing == 0,
                $"Courtyard scene contains {missing} missing MonoBehaviour script slot(s).");
        }

        private static void ValidateExpectedDefinitionAnchor(
            StageDefinitionProfile definition,
            string anchorId,
            int expectedPositionId,
            Vector3 expectedPosition,
            Vector3 expectedEuler)
        {
            int anchorCount = 0;
            for (int i = 0; i < definition.AnchorCount; i++)
            {
                StageDefinitionProfile.AnchorRef anchor = definition.GetAnchor(i);
                if (string.Equals(anchor.AnchorId, anchorId,
                    StringComparison.Ordinal))
                {
                    RequireEqual(anchor.GroupId, SpawnAnchorGroupId,
                        $"definition anchor {anchorId} groupId");
                    Require(Approximately(anchor.ExpectedPosition, expectedPosition)
                            && ApproximatelyEuler(anchor.ExpectedEuler, expectedEuler),
                        $"Definition anchor '{anchorId}' has stale reviewed pose.");
                    anchorCount++;
                }
            }

            Require(anchorCount == 1,
                $"Stage definition requires exactly one anchor '{anchorId}'.");
            int spawnCount = 0;
            for (int i = 0; i < definition.SpawnCount; i++)
            {
                StageDefinitionProfile.SpawnRef spawn = definition.GetSpawn(i);
                if (string.Equals(spawn.AnchorId, anchorId, StringComparison.Ordinal))
                {
                    Require(spawn.PositionId == expectedPositionId,
                        $"Spawn bound to '{anchorId}' has stale positionId.");
                    spawnCount++;
                }
            }

            Require(spawnCount == 1,
                $"Stage definition requires exactly one spawn bound to '{anchorId}'.");
        }

        private static void ValidateExpectedSceneAnchor(
            StageDefinitionSceneBinding binding,
            string anchorId,
            StageSpawnKind spawnKind,
            int positionId)
        {
            Require(binding.TryGetAnchorPoint(anchorId, out StageAnchorPoint anchor)
                    && anchor != null
                    && anchor.UsageKind == StageAnchorUsageKind.CombatSpawn
                    && anchor.SpawnKind == spawnKind
                    && anchor.PositionId == positionId,
                $"Scene anchor '{anchorId}' has invalid combat-spawn kind or position identity.");
        }

        private static void ValidateAbsentPresentation(
            StagePresentationHandoffRef presentation,
            string label)
        {
            if (presentation == null)
            {
                return;
            }

            Require(!presentation.IsPresent
                    && presentation.StageDefinition == null
                    && presentation.CinematicProfile == null
                    && presentation.ExpectedPlayableAsset == null
                    && string.IsNullOrWhiteSpace(presentation.HandoffId)
                    && string.IsNullOrWhiteSpace(presentation.ExpectedPortId)
                    && string.IsNullOrWhiteSpace(presentation.TriggerConditionId)
                    && string.IsNullOrWhiteSpace(presentation.CompletionConditionId),
                $"Courtyard {label} presentation must be fully typed absent without retained Olympus references.");
        }

        private static void ValidateTerminalPolicy(StageTerminalResolutionPolicy policy)
        {
            Require(policy != null, "Courtyard terminal resolution policy is missing.");
            RequireEqual(policy.TerminalResolutionPolicyId, TerminalPolicyId,
                "terminalResolutionPolicyId");
            Require(policy.SemanticRevision == 1
                    && policy.WindowKind == StageTerminalWindowKind.SameTerminalResolutionEpoch
                    && policy.BatchOwnerKind
                        == StageTerminalBatchOwnerKind.EncounterTerminalResolutionCoordinator
                    && policy.RootAdmissionKind
                        == StageTerminalRootAdmissionKind.CanonicalCombatRootAdmission
                    && policy.RootOrderKind == StageTerminalRootOrderKind.RootAdmissionSequence
                    && policy.RootIssuePoint
                        == StageTerminalRootIssuePoint.BeforeTerminalStateMutationAndCallbacks
                    && policy.BatchBoundaryKind
                        == StageTerminalBatchBoundaryKind.RootResolutionToken
                    && policy.TerminalSubjectRoles
                        == (StageTerminalSubjectRole.Player | StageTerminalSubjectRole.Boss)
                    && policy.CoveragePolicy
                        == StageTerminalCoveragePolicy
                            .ExclusiveQueuedTerminalStateMutationForBoundSubjects
                    && policy.WorkExecutionKind
                        == StageTerminalWorkExecutionKind.SynchronousNonYieldingResolution
                    && policy.NestedRequestPolicy
                        == StageTerminalNestedRequestPolicy.SameRootSameEpoch
                    && policy.IndependentRequestPolicy
                        == StageTerminalIndependentRequestPolicy
                            .LowerAdmissionSequenceThenNextEpoch
                    && policy.EpochStampKind == StageTerminalEpochStampKind.EncounterTerminalEpoch
                    && policy.CoordinatorLifecycleKind
                        == StageTerminalCoordinatorLifecycleKind
                            .IdleOpenDrainingFinalizingEpochClosedTerminalClosedFaultedCancelled
                    && policy.SubjectFinalizationKind
                        == StageTerminalSubjectFinalizationKind.SynchronousTwoSubjectSnapshot
                    && policy.TokenStatePolicy
                        == StageTerminalTokenStatePolicy
                            .ExplicitIdleActiveDeferredClosedWrongRunPostTerminal
                    && policy.FlushBarrier
                        == StageTerminalFlushBarrierKind.QueueDrainedAndSubjectsFinalized
                    && policy.SimultaneousOutcome == StageTerminalSimultaneousOutcome.Clear
                    && policy.RequiresBossCandidateAndFinalDead
                    && policy.RequiresPlayerCandidateAndFinalDown,
                "Courtyard terminal policy does not preserve the exact coordinated two-subject semantics.");
        }

        private static void ValidateAction(
            PlayableStageDefinition route,
            string actionId,
            StageRouteActionKind kind,
            string targetPlayableStageId,
            StageUiRouteId targetUiRouteId,
            StageRouteOutcome outcomes)
        {
            Require(route.TryGetTerminalAction(actionId, out StageRouteActionRef action)
                    && action != null
                    && action.ActionKind == kind
                    && string.Equals(action.TargetPlayableStageId,
                        targetPlayableStageId, StringComparison.Ordinal)
                    && action.TargetUiRouteId == targetUiRouteId
                    && action.AllowedOutcomes == outcomes,
                $"Courtyard terminal action '{actionId}' is invalid.");
        }

        private static void ValidateOptionalSingleton<T>(
            SerializedObject serialized,
            string propertyName,
            Transform playerRoot,
            string label)
            where T : Component
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null, $"Fact adapter is missing serialized {label} property.");
            T configured = property.objectReferenceValue as T;
            T resolved = null;
            T[] candidates = playerRoot.GetComponentsInChildren<T>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                T candidate = candidates[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy
                    || candidate is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                {
                    continue;
                }

                Require(resolved == null,
                    $"Player root contains more than one live {label} source.");
                resolved = candidate;
            }

            Require(ReferenceEquals(resolved, configured),
                $"Fact adapter does not bind the exact live {label} source.");
        }

        private static void ValidateExactSerializedReference(
            UnityEngine.Object owner,
            string propertyName,
            UnityEngine.Object expected,
            string label)
        {
            ValidateExactSerializedReference(
                new SerializedObject(owner), propertyName, expected, label);
        }

        private static void ValidateExactSerializedReference(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object expected,
            string label)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null && ReferenceEquals(property.objectReferenceValue, expected),
                $"Courtyard {label} does not reference the exact authored object.");
        }

        private static T ValidateSerializedReferenceType<T>(
            SerializedObject serialized,
            string propertyName,
            string label)
            where T : UnityEngine.Object
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            T value = property?.objectReferenceValue as T;
            Require(value != null, $"Courtyard {label} reference is missing or has the wrong type.");
            return value;
        }

        private static void RequireUniqueAssetIdentity<T>(
            T expectedObject,
            string expectedId,
            Func<T, string> readIdentity)
            where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets(
                $"t:{typeof(T).Name}",
                new[] { "Assets/_Game" });
            int matchCount = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                T candidate = AssetDatabase.LoadAssetAtPath<T>(path);
                if (candidate != null
                    && string.Equals(readIdentity(candidate), expectedId,
                        StringComparison.Ordinal))
                {
                    Require(ReferenceEquals(candidate, expectedObject),
                        $"Identity '{expectedId}' is owned by unexpected asset '{path}'.");
                    matchCount++;
                }
            }

            Require(matchCount == 1,
                $"Identity '{expectedId}' must resolve to exactly one persistent {typeof(T).Name}; found {matchCount}.");
        }

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null,
                $"B1-1 required {typeof(T).Name} is missing at exact path '{path}'.");
            return asset;
        }

        private static void RequireExactAssetPath(
            UnityEngine.Object asset,
            string expectedPath,
            string label)
        {
            Require(asset != null && EditorUtility.IsPersistent(asset),
                $"B1-1 {label} must be a persistent asset.");
            RequireEqual(NormalizePath(AssetDatabase.GetAssetPath(asset)), expectedPath,
                $"{label} asset path");
        }

        private static T RequireSingleSceneComponent<T>(Scene scene)
            where T : Component
        {
            T[] components = CollectSceneComponents<T>(scene);
            Require(components.Length == 1,
                $"Scene '{scene.path}' requires exactly one {typeof(T).Name}; found {components.Length}.");
            return components[0];
        }

        private static MonoBehaviour RequireSingleSceneInterface<T>(Scene scene)
            where T : class
        {
            MonoBehaviour found = null;
            MonoBehaviour[] behaviours = CollectSceneComponents<MonoBehaviour>(scene);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour is not T)
                {
                    continue;
                }

                Require(found == null,
                    $"Scene '{scene.path}' contains more than one {typeof(T).Name} implementation.");
                found = behaviour;
            }

            Require(found != null && found.isActiveAndEnabled,
                $"Scene '{scene.path}' requires one active {typeof(T).Name} implementation.");
            return found;
        }

        private static T[] CollectSceneComponents<T>(Scene scene)
            where T : Component
        {
            var rows = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                T[] candidates = roots[i].GetComponentsInChildren<T>(true);
                for (int candidateIndex = 0;
                     candidateIndex < candidates.Length;
                     candidateIndex++)
                {
                    T candidate = candidates[candidateIndex];
                    if (candidate != null && candidate.gameObject.scene.handle == scene.handle)
                    {
                        rows.Add(candidate);
                    }
                }
            }

            return rows.ToArray();
        }

        private static int CountLiveSceneBehaviours<T>(Scene scene)
            where T : Behaviour
        {
            int count = 0;
            T[] rows = CollectSceneComponents<T>(scene);
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null && rows[i].isActiveAndEnabled)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ResolveBindingRootLocalPose(
            Transform bindingRoot,
            Transform anchor,
            out Vector3 localPosition,
            out Quaternion localRotation)
        {
            localPosition = bindingRoot.InverseTransformPoint(anchor.position);
            localRotation = Quaternion.Inverse(bindingRoot.rotation) * anchor.rotation;
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return Mathf.Abs(left.x - right.x) <= 0.0001f
                && Mathf.Abs(left.y - right.y) <= 0.0001f
                && Mathf.Abs(left.z - right.z) <= 0.0001f;
        }

        private static bool ApproximatelyEuler(Vector3 left, Vector3 right)
        {
            return Mathf.Abs(Mathf.DeltaAngle(left.x, right.x)) <= 0.001f
                && Mathf.Abs(Mathf.DeltaAngle(left.y, right.y)) <= 0.001f
                && Mathf.Abs(Mathf.DeltaAngle(left.z, right.z)) <= 0.001f;
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y)
                && float.IsFinite(value.z);
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/');
        }

        private static void RefuseDirtyOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                Require(!scene.isDirty,
                    $"Refusing read-only B1-1 validation while scene '{scene.path}' is dirty.");
            }
        }

        private static void RequireCanonicalDigest(string digest, string label)
        {
            bool valid = digest != null && digest.Length == 64;
            for (int i = 0; valid && i < digest.Length; i++)
            {
                char value = digest[i];
                valid = value is >= '0' and <= '9' or >= 'a' and <= 'f';
            }

            Require(valid, $"{label} must be a lowercase 64-character SHA-256 digest.");
        }

        private static void RequireEqual(string actual, string expected, string label)
        {
            Require(string.Equals(actual, expected, StringComparison.Ordinal),
                $"{label} mismatch. actual='{actual}', expected='{expected}'.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class Pack
        {
            public Pack(
                SceneAsset sceneAsset,
                StageDefinitionProfile stageDefinition,
                PlayableStageDefinition route,
                LinearStageTemplateProfile template,
                StageResultPresentationProfile presentationProfile,
                StageResultLocalizationTable localization,
                StageResultPresentationCatalog presentationCatalog,
                StageResultDefinition resultDefinition,
                StageProgressionNode progressionNode,
                StageProgressionGraph progressionGraph)
            {
                SceneAsset = sceneAsset;
                StageDefinition = stageDefinition;
                Route = route;
                Template = template;
                PresentationProfile = presentationProfile;
                Localization = localization;
                PresentationCatalog = presentationCatalog;
                ResultDefinition = resultDefinition;
                ProgressionNode = progressionNode;
                ProgressionGraph = progressionGraph;
            }

            public SceneAsset SceneAsset { get; }
            public StageDefinitionProfile StageDefinition { get; }
            public PlayableStageDefinition Route { get; }
            public LinearStageTemplateProfile Template { get; }
            public StageResultPresentationProfile PresentationProfile { get; }
            public StageResultLocalizationTable Localization { get; }
            public StageResultPresentationCatalog PresentationCatalog { get; }
            public StageResultDefinition ResultDefinition { get; }
            public StageProgressionNode ProgressionNode { get; }
            public StageProgressionGraph ProgressionGraph { get; }
        }
    }

    /// <summary>
    /// B1-1 publication-state gate. A valid authored stage must remain unreachable from the
    /// product catalog and from every enabled or disabled Build Settings row while quarantined.
    /// </summary>
    public static class OlympusCourtyardDrillB11QuarantineGate
    {
        private const string RouteTablePath =
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string StageCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string AcceptedRoutePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";
        private const string AcceptedCatalogEntryId = "story_v1_training_route";
        private const string AcceptedPlayableStageId = "OLYMPUS-INVASION-01";
        private const string AcceptedProjectionDigest =
            "571b79d2fb47619383be714f88870752c4f8e1ce4d2864d6dc846307aecb6f1d";
        private const string AcceptedTerminalPolicyDigest =
            "f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2";
        private const string AcceptedRouteDigest =
            "878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0";
        private const string AcceptedJoinDigest =
            "d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71";
        private const string AcceptedProductManifestDigest =
            "b0f1a128548f8f77aae5a0670586a2ac39c504d967ef722cf9681f56cd788d6b";

        [MenuItem("DimensionBrawl/B1-1/Validate Olympus Courtyard Drill Quarantine Gate")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
            Debug.Log(
                "[OlympusCourtyardDrillB11QuarantineGate] QUARANTINE_PASS "
                + "authoredReady=true, shippedReachable=false.");
        }

        public static void RunBatchVerification()
        {
            try
            {
                ValidateOrThrow();
                Debug.Log(
                    "[OlympusCourtyardDrillB11QuarantineGate] BATCH_QUARANTINE_PASS "
                    + "authoredReady=true, shippedReachable=false, "
                    + $"productManifest={AcceptedProductManifestDigest}.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[OlympusCourtyardDrillB11QuarantineGate] BATCH_QUARANTINE_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateOrThrow()
        {
            OlympusCourtyardDrillAuthoredPackValidator.ValidateOrThrow();

            UIStageCatalog catalog = LoadRequired<UIStageCatalog>(StageCatalogPath);
            PlayableStageDefinition candidate = LoadRequired<PlayableStageDefinition>(
                OlympusCourtyardDrillAuthoredPackValidator.PlayableStagePath);
            PlayableStageDefinition accepted = LoadRequired<PlayableStageDefinition>(
                AcceptedRoutePath);
            Require(catalog.TryValidateEntryIdentities(out UIStageRouteProjectionRejectReason reason),
                $"Product stage catalog identity is invalid: {reason}.");
            Require(catalog.StageCount == 1 && catalog.CatalogProjectionGeneration == 2,
                "B1-1 product catalog must remain one row at generation 2.");
            UIStageCatalog.StageEntry acceptedEntry = catalog.GetStage(0);
            Require(string.Equals(acceptedEntry.Id, AcceptedCatalogEntryId,
                        StringComparison.Ordinal)
                    && ReferenceEquals(acceptedEntry.PlayableStage, accepted)
                    && string.Equals(acceptedEntry.CanonicalProjectionDigest,
                        AcceptedProjectionDigest, StringComparison.Ordinal),
                "Accepted Olympus catalog entry identity/projection changed during B1-1.");

            for (int i = 0; i < catalog.StageCount; i++)
            {
                UIStageCatalog.StageEntry entry = catalog.GetStage(i);
                Require(!ReferenceEquals(entry.PlayableStage, candidate)
                        && !string.Equals(entry.PlayableStage?.PlayableStageId,
                            OlympusCourtyardDrillAuthoredPackValidator.PlayableStageId,
                            StringComparison.Ordinal),
                    "Courtyard candidate became catalog-reachable while quarantined.");
                if (entry.PlayableStage == null)
                {
                    continue;
                }

                for (int segmentIndex = 0;
                     segmentIndex < entry.PlayableStage.SceneSegmentCount;
                     segmentIndex++)
                {
                    string scenePath = entry.PlayableStage.GetSceneSegment(segmentIndex)
                        ?.StageDefinition?.MapScenePath;
                    Require(!string.Equals(NormalizePath(scenePath),
                            OlympusCourtyardDrillAuthoredPackValidator.ScenePath,
                            StringComparison.OrdinalIgnoreCase),
                        "Courtyard candidate scene became catalog-reachable while quarantined.");
                }
            }

            EditorBuildSettingsScene[] buildScenes =
                EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            for (int i = 0; i < buildScenes.Length; i++)
            {
                Require(!string.Equals(
                        NormalizePath(buildScenes[i].path),
                        OlympusCourtyardDrillAuthoredPackValidator.ScenePath,
                        StringComparison.OrdinalIgnoreCase),
                    "Courtyard candidate scene must be absent from every enabled or disabled Build Settings row while quarantined.");
            }

            Require(string.Equals(accepted.PlayableStageId, AcceptedPlayableStageId,
                        StringComparison.Ordinal)
                    && string.Equals(accepted.CanonicalRouteDigest, AcceptedRouteDigest,
                        StringComparison.Ordinal)
                    && accepted.TerminalResolutionPolicy != null
                    && string.Equals(
                        accepted.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                        AcceptedTerminalPolicyDigest,
                        StringComparison.Ordinal)
                    && accepted.ResultProgressionJoin != null
                    && string.Equals(accepted.ResultProgressionJoin.CanonicalDigest,
                        AcceptedJoinDigest, StringComparison.Ordinal),
                "Accepted Olympus route/policy/join identity changed during B1-1.");

            UIScreenRouteTable routeTable = LoadRequired<UIScreenRouteTable>(RouteTablePath);
            Require(UIProductBuildRouteManifest.TryCreate(
                    routeTable,
                    catalog,
                    CanonicalUiBuildSettings.StageClearScenePath,
                    out UIProductBuildRouteManifest manifest,
                    out UIProductBuildManifestRejectReason manifestReject,
                    out string manifestError),
                $"Accepted product manifest rejected {manifestReject}: {manifestError}");
            Require(manifest.CatalogEntryCount == 1
                    && manifest.RouteSegmentCount == 2
                    && manifest.SceneCount == 5
                    && string.Equals(manifest.CanonicalDigest,
                        AcceptedProductManifestDigest, StringComparison.Ordinal),
                "Accepted product manifest changed while the Courtyard candidate is quarantined.");
            for (int i = 0; i < manifest.SceneCount; i++)
            {
                Require(!string.Equals(manifest.GetScene(i).ScenePath,
                        OlympusCourtyardDrillAuthoredPackValidator.ScenePath,
                        StringComparison.OrdinalIgnoreCase),
                    "Courtyard candidate leaked into the accepted product manifest.");
            }

            UIV1BuildSettingsReadinessReporter.ValidateCurrentReadinessOrThrow();
            PlayableStageDefinitionValidator.ValidateOrThrow();
        }

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null,
                $"Quarantine gate requires {typeof(T).Name} at exact path '{path}'.");
            return asset;
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/');
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
