using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    public static class PlayableStageDefinitionValidator
    {
        private const string RouteAssetPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";
        private const string CorridorDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string StationDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusStationCombat.asset";
        private const string CorridorScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StageSelectScenePath = "Assets/_Game/Scenes/UI/UI_StageSelect.unity";
        private const string StageClearScenePath = "Assets/_Game/Scenes/UI/UI_StageClear.unity";
        private const string UiRouteTablePath = "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string StageResultPresentationCatalogPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog.asset";
        private const string StageResultDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusInvasion.asset";
        private const string StageProgressionNodePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusInvasion.asset";
        private const string StageProgressionGraphPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusInvasion.asset";
        private const string StageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string StationMeleeAddArchetypePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/DB_Archetype_SciFiSoldier_Melee.asset";
        private const string StationRangedAddArchetypePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/DB_Archetype_SciFiSoldier_Ranged.asset";
        private const string StationMeleeAddPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Melee_HeavyWindup.prefab";
        private const string StationRangedAddPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Ranged_RifleCrossfire.prefab";
        private const string StationMeleeAddPatternPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_HeavyWindup.asset";
        private const string StationRangedAddPatternPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RifleCrossfire.asset";
        private const string StationRangedAddDeckPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BasicSoldier_RifleCrossfireDeck.asset";
        private const string StationRangedProjectilePrefabPath =
            "Assets/_Game/Prefabs/Combat/PF_EnemyProjectile_RifleCrossfire.prefab";
        private const string StageTemplatePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusInvasionTutorialStationRun.asset";
        private const string StageSelectPrefabPath =
            "Assets/_Game/UI/StageSelect/PF_UI_StageSelectScreen.prefab";
        private const string IntroPresentationProfilePath =
            "Assets/_Game/DesignData/Profiles/Cinematics/DB_Cinematic_IntroGatePodAwakening_OlympusBombingPrelude.asset";
        private const string IntroPresentationPlayablePath =
            "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_IntroGatePodAwakening_OlympusBombingPrelude.playable";
        private const string CinematicProfilesDirectory =
            "Assets/_Game/DesignData/Profiles/Cinematics";
        private const string StageClearPresenterPath =
            "Assets/_Game/Scripts/UI/StageClear/StageClearScreenPresenter.cs";
        private const string StageSelectPresenterPath =
            "Assets/_Game/UI/StageSelect/StageSelectScreenPresenter.cs";
        private const string StageCatalogSourcePath =
            "Assets/_Game/UI/StageSelect/UIStageCatalog.cs";

        private const string PlayableStageId = "OLYMPUS-INVASION-01";
        private const string CorridorStageId = "OLYMPUS-CORRIDOR-INTRO-COMBAT-01";
        private const string StationStageId = "OLYMPUS-STATION-COMBAT-01";
        private const string RunEntryConditionId = "run.entry.admitted";
        private const string StationEntryReachedConditionId = "corridor.station-entry.reached";
        private const string StationTerminalConditionId = "station.encounter.terminal";
        private const string StationAddSpawnId = "add-left";
        private const string StationAddAnchorId = "Add_LeftLaneAnchor";
        private const string StationRightAddSpawnId = "add-right";
        private const string StationRightAddAnchorId = "Add_RightLaneAnchor";
        private const string StationAddAnchorGroupId = "CombatSpawnAnchors";
        private const string StationMeleeAddPayloadId = "SciFiSoldier.Melee";
        private const string StationRangedAddPayloadId = "SciFiSoldier.Ranged";
        private const int StationAddPositionId = 2101;
        private const int StationRightAddPositionId = 2102;
        private const string IntroHandoffId = "intro-to-stage";
        private const string IntroPortId = "intro-gatepod-port";
        private const string IntroCompletionConditionId = "corridor.tutorial.ready";
        private const string CanonicalCatalogEntryId = "story_v1_training_route";
        private const string CanonicalLoadingCardId = "stage_to_combat_mood_bridge";
        private const string CanonicalProjectionDigest =
            "571b79d2fb47619383be714f88870752c4f8e1ce4d2864d6dc846307aecb6f1d";
        private const string CanonicalTemplateDigest =
            "3eec8a5f94c4dfd47ae9255a49ff3b5961d5130cf386f2c6ba96b0525c502e55";
        private const string CanonicalReferenceDigest =
            "eada6124fe3bed295bddaf3caeb0b53ff1510a2f790c2b76b8454410834a21ea";
        private const string CanonicalBriefingDigest =
            "e334d6bb63dc42d921e6d85bcca42cc628064d0d1b8fee1cc303d4ca223fab70";
        private const string CanonicalResultEvaluationDigest =
            "ab16e4e051c053d57b7ce7a4c841fe42ee1a730ca0123f62684cf7c3decdc5da";
        private const string CanonicalResultPresentationBindingDigest =
            "095c545df089d7670daedb20b3603c180ca5c4ecf7a67c75a6a351d690dd4d0f";
        private const string CanonicalResultPresentationSourceDigest =
            "e94f5290000b043b5e96c496a67cac6a0df716c77fb36994a34f568ea829f5bc";
        private const string CanonicalProgressionNodeContentDigest =
            "87e684b5a7b0eac8fceaae168693d84132504bbca9a52bee0deb4187b28f9ac4";
        private const string CanonicalProgressionNodeBindingDigest =
            "cf1f0d21d34d1553b3aaadd6e8dbb8ace24b235245f8301b35959ee662a3dc37";
        private const string CanonicalProgressionGraphDigest =
            "7132faaa2607da7ad62380d2d3301ed2bda50cf81ad26e0dd6cdd08e46154221";
        private const string CanonicalResultProgressionJoinDigest =
            "d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71";
        private const string CanonicalStageTitle = "기억의 회랑";
        private const string CanonicalStageObjective =
            "하층 세계에서 발생한 차원의 미세한 균열.\n그 징후의 진원지를 조사하라.";
        private const string CanonicalCombatLesson =
            "회랑에서 근접 공격, 이동, 원거리 전환과 사격, 회피, 표적 정리를 차례로 익힌다. 정거장에서는 레플리카 지급과 소환 안내를 확인한 뒤 보스 격파를 목표로 한다.";

        private static readonly Dictionary<string, int> ExpectedDamageProducers =
            new(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/Combat/BossBarrageProjectile.cs"] = 1,
                ["Assets/_Game/Scripts/Combat/BossLaserSummonPattern.cs"] = 1,
                ["Assets/_Game/Scripts/Combat/LaneActionProjectile.cs"] = 1,
                ["Assets/_Game/Scripts/Combat/SummonFrontlineClash.cs"] = 1,
                ["Assets/_Game/Scripts/Enemies/BasicSoldierEnemy.cs"] = 1,
                ["Assets/_Game/Scripts/Player/PlayerActionController.cs"] = 1,
                ["Assets/_Game/Scripts/Player/PlayerController.cs"] = 1,
                ["Assets/_Game/Scripts/Player/PlayerSkill1LaserSweepAction.cs"] = 1
            };

        private static readonly Dictionary<string, int> ExpectedResetCallers =
            new(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/Combat/SummonFrontlineProxy.cs"] = 1,
                ["Assets/_Game/Scripts/Core/MobilePerformanceBenchmarkRunner.cs"] = 1,
                ["Assets/_Game/Scripts/LevelDesign/OlympusCorridorTutorialDirector.cs"] = 1,
                ["Assets/_Game/Scripts/Player/PlayerController.cs"] = 1
            };

        private static readonly Dictionary<string, int> ExpectedConfigureMaxCallers =
            new(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/Combat/SummonFrontlineProxy.cs"] = 1
            };

        [MenuItem("DimensionBrawl/Validate Playable Stage Route Shell")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
        }

        public static void RunBatchVerification()
        {
            try
            {
                ValidateOrThrow();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateOrThrow()
        {
            PlayableStageDefinition route = LoadRequiredAsset<PlayableStageDefinition>(RouteAssetPath);
            StageDefinitionProfile corridor = LoadRequiredAsset<StageDefinitionProfile>(CorridorDefinitionPath);
            StageDefinitionProfile station = LoadRequiredAsset<StageDefinitionProfile>(StationDefinitionPath);
            CinematicSequenceProfile introProfile =
                LoadRequiredAsset<CinematicSequenceProfile>(IntroPresentationProfilePath);
            PlayableAsset introPlayable =
                LoadRequiredAsset<PlayableAsset>(IntroPresentationPlayablePath);
            UIStageCatalog stageCatalog = LoadRequiredAsset<UIStageCatalog>(StageCatalogPath);
            LinearStageTemplateProfile stageTemplate =
                LoadRequiredAsset<LinearStageTemplateProfile>(StageTemplatePath);

            ValidateStageCatalogInventory(stageCatalog);
            ValidateRouteContract(route, corridor, station, introProfile, introPlayable);
            ValidateTruthfulReferenceContract(route, stageTemplate, introProfile);
            ValidateResultProgressionJoin(route);
            ValidateStageCatalogSelection(stageCatalog, route, corridor);
            ValidateStageDefinitions(corridor, station);
            ValidateBuildSettingsRoute();
            ValidateSceneContracts(route, corridor, station, introProfile, introPlayable);
            ValidateCinematicProfileStageContexts();
            ValidateTerminalMutationInventory();

            Debug.Log(
                "[PlayableStageDefinitionValidator] PASS "
                + $"playableStageId={route.PlayableStageId}, routeRevision={route.RouteRevision}, "
                + $"policyRevision={route.TerminalResolutionPolicy.SemanticRevision}, "
                + $"policyDigest={route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest}, "
                + $"routeDigest={route.CanonicalRouteDigest}, "
                + $"resultProgressionJoinDigest={route.ResultProgressionJoin.CanonicalDigest}");
        }

        private static void ValidateRouteContract(
            PlayableStageDefinition route,
            StageDefinitionProfile corridor,
            StageDefinitionProfile station,
            CinematicSequenceProfile introProfile,
            PlayableAsset introPlayable)
        {
            Require(
                StageRunRouteSnapshot.TryCreate(
                    route,
                    out StageRunRouteSnapshot genericSnapshot,
                    out string genericRouteError),
                "Route must pass the bounded generic admission contract: " + genericRouteError);
            Require(route.SchemaVersion == 1, "Route schemaVersion must be 1.");
            RequireEqual(route.PlayableStageId, PlayableStageId, "playableStageId");
            Require(route.RouteRevision == 2, "Route revision must be 2.");
            Require(
                route.SceneSegmentCount == 2,
                "Route must contain exactly two ordered logical segments in the shared host scene.");
            Require(route.TerminalActionCount == 3, "Route must contain exactly Replay, Retry, and Lobby actions.");
            Require(
                (int)StageUiRouteId.Lobby == (int)UIRouteId.Lobby,
                "StageUiRouteId.Lobby must remain numerically identical to UIRouteId.Lobby.");

            StageSceneSegmentRef corridorSegment = route.GetSceneSegment(0);
            StageSceneSegmentRef stationSegment = route.GetSceneSegment(1);
            Require(
                genericSnapshot.GetSegmentRoles(0) == StageRunSegmentRole.Entry,
                "Olympus Corridor must remain the non-terminal entry role.");
            Require(
                genericSnapshot.GetSegmentRoles(1) == StageRunSegmentRole.Terminal,
                "Olympus Station must remain the final ReturnToOwner role.");

            ValidateSegmentIdentity(
                corridorSegment,
                "corridor_intro_tutorial",
                0,
                corridor,
                RunEntryConditionId,
                StageSegmentConditionKind.RunEntrySnapshotValidatedAndFirstSegmentActivated,
                StationEntryReachedConditionId,
                StageSegmentConditionKind.CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance);
            Require(
                corridorSegment.HandoffPolicy == StageSceneHandoffPolicy.InSceneAdvance,
                "Corridor must use InSceneAdvance.");
            Require(
                corridorSegment.SuccessorKind == StageSegmentSuccessorKind.NextOrderedSegment,
                "Corridor InSceneAdvance must target the next ordered segment.");
            Require(
                corridorSegment.DestinationSceneKind
                    == StageSegmentDestinationSceneKind.SuccessorStageDefinitionScene,
                "Corridor InSceneAdvance destination must be the successor StageDefinition scene.");
            Require(
                corridorSegment.TransitionTokenKind
                    == StageSegmentTransitionTokenKind.SealedCurrentRunSegmentHandoff,
                "Corridor InSceneAdvance must require the sealed current-run handoff token.");
            Require(
                corridorSegment.LoaderGenerationKind == StageSegmentLoaderGenerationKind.None,
                "Corridor InSceneAdvance must carry typed absence of a route-loader generation.");
            Require(
                corridorSegment.NavigationAuthorityKind
                    == StageSegmentNavigationAuthorityKind.P1AStageRunRouteOwner,
                "Corridor InSceneAdvance must be owned by the P1-A run/route owner.");
            Require(
                corridorSegment.ReturnOwnerKind == StageSegmentReturnOwnerKind.None
                    && corridorSegment.ReturnOwnerReceiptPolicy == StageReturnOwnerReceiptPolicy.None,
                "Corridor InSceneAdvance must carry typed absence of ReturnToOwner fields.");

            ValidateSegmentIdentity(
                stationSegment,
                "station_entry_combat",
                1,
                station,
                StationEntryReachedConditionId,
                StageSegmentConditionKind.CorridorTutorialFactsSealedAndStationEntryReachedForInSceneAdvance,
                StationTerminalConditionId,
                StageSegmentConditionKind.StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched);
            Require(
                stationSegment.HandoffPolicy == StageSceneHandoffPolicy.ReturnToOwner,
                "Final Station segment must use ReturnToOwner.");
            Require(
                stationSegment.SuccessorKind == StageSegmentSuccessorKind.None
                    && stationSegment.DestinationSceneKind == StageSegmentDestinationSceneKind.None
                    && stationSegment.TransitionTokenKind == StageSegmentTransitionTokenKind.None
                    && stationSegment.LoaderGenerationKind == StageSegmentLoaderGenerationKind.None
                    && stationSegment.NavigationAuthorityKind == StageSegmentNavigationAuthorityKind.None,
                "ReturnToOwner must carry typed absence of successor, destination scene, transition token, loader generation, and navigation authority.");
            Require(
                stationSegment.ReturnOwnerKind == StageSegmentReturnOwnerKind.P1AStageRunRouteOwner,
                "ReturnToOwner owner must be the P1-A stage run/route owner.");
            Require(
                stationSegment.ReturnOwnerReceiptPolicy
                    == StageReturnOwnerReceiptPolicy.ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented,
                "ReturnToOwner must deliver the exact terminal record exactly once through TerminalFinalizing -> Committed -> Presented.");
            Require(
                string.Equals(
                    corridorSegment.ExitConditionId,
                    stationSegment.EntryConditionId,
                    StringComparison.Ordinal)
                && corridorSegment.ExitConditionKind == stationSegment.EntryConditionKind,
                "Corridor exit and Station entry must share the same immutable in-scene boundary condition.");
            Require(
                new HashSet<string>(StringComparer.Ordinal)
                {
                    corridorSegment.EntryConditionId,
                    corridorSegment.ExitConditionId,
                    stationSegment.ExitConditionId
                }.SetEquals(new[]
                {
                    RunEntryConditionId,
                    StationEntryReachedConditionId,
                    StationTerminalConditionId
                }),
                "The three route revision 2 condition IDs are immutable; semantic changes require a new condition ID and route revision/digest bump.");
            Require(
                string.Equals(
                    corridorSegment.StageDefinition.MapScenePath,
                    stationSegment.StageDefinition.MapScenePath,
                    StringComparison.Ordinal),
                "Revision 2 logical segments must resolve to the same physical host scene.");

            ValidateIntroPresentationContract(
                corridorSegment,
                corridor,
                introProfile,
                introPlayable);
            Require(
                !IsPresent(corridorSegment.ExitPresentation)
                    && !IsPresent(stationSegment.EntryPresentation)
                    && !IsPresent(stationSegment.ExitPresentation),
                "Revision 2 owns only the Corridor intro entry presentation; other presentation arms must remain typed absent.");

            ValidateAction(
                route,
                "olympus-invasion.replay",
                StageRouteActionKind.Replay,
                PlayableStageId,
                StageUiRouteId.None,
                StageRouteOutcome.Clear);
            ValidateAction(
                route,
                "olympus-invasion.retry",
                StageRouteActionKind.Retry,
                PlayableStageId,
                StageUiRouteId.None,
                StageRouteOutcome.Fail);
            ValidateAction(
                route,
                "olympus-invasion.to-lobby",
                StageRouteActionKind.UIRoute,
                string.Empty,
                StageUiRouteId.Lobby,
                StageRouteOutcome.Clear | StageRouteOutcome.Fail);

            ValidateTerminalPolicy(route.TerminalResolutionPolicy);

            string expectedPolicyDigest = route.TerminalResolutionPolicy.ComputeCanonicalDigest();
            if (!string.Equals(
                route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                expectedPolicyDigest,
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Terminal policy digest mismatch. "
                    + $"expectedPolicyDigest={expectedPolicyDigest}, "
                    + $"storedPolicyDigest={route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest}");
            }

            string expectedRouteDigest = route.ComputeCanonicalRouteDigest();
            if (!string.Equals(route.CanonicalRouteDigest, expectedRouteDigest, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Canonical route digest mismatch. "
                    + $"expectedRouteDigest={expectedRouteDigest}, storedRouteDigest={route.CanonicalRouteDigest}");
            }
        }

        private static void ValidateTruthfulReferenceContract(
            PlayableStageDefinition route,
            LinearStageTemplateProfile template,
            CinematicSequenceProfile introProfile)
        {
            StageReferenceBlock block = route.ReferenceBlock;
            Require(block != null && block.IsPresent, "Canonical route reference block is missing.");
            Require(block.SchemaVersion == 1 && block.Revision == 2, "Reference block must be revision 2.");
            Require(
                ReferenceEquals(block.StageTemplate, template),
                "Reference block must retain the exact canonical stage-template object.");
            RequireEqual(block.CanonicalReferenceDigest, CanonicalReferenceDigest, "Stored reference digest");
            RequireEqual(
                route.ComputeCanonicalReferenceDigest(),
                CanonicalReferenceDigest,
                "Recomputed reference digest");

            Require(template.TemplateSchemaVersion == 1, "Stage-template schema must be 1.");
            Require(template.TemplateRevision == 1, "Stage-template revision must be 1.");
            RequireEqual(
                template.StageTemplateId,
                "olympus-invasion.tutorial-station-run",
                "Stage-template ID");
            Require(template.TemplateKind == LinearStageTemplateKind.TutorialRun, "Stage-template kind is invalid.");
            RequireEqual(template.CanonicalTemplateDigest, CanonicalTemplateDigest, "Stored template digest");
            RequireEqual(
                template.ComputeCanonicalTemplateDigest(),
                CanonicalTemplateDigest,
                "Recomputed template digest");
            RequireEqual(template.DisplayName, CanonicalStageTitle, "Template display-name mirror");
            Require(
                template.TitleDisposition == StageBriefingValueDisposition.Present,
                "Template title must be present.");
            RequireEqual(template.Title, CanonicalStageTitle, "Template title");
            Require(
                template.TitleLocalizationKeyDisposition
                    == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.TitleLocalizationKey),
                "Template title localization key must remain typed NoVerifiedSource.");
            Require(
                template.ObjectiveDisposition == StageBriefingValueDisposition.Present,
                "Template objective must be present.");
            RequireEqual(template.Objective, CanonicalStageObjective, "Template objective");
            Require(
                template.CombatLessonDisposition == StageBriefingValueDisposition.Present,
                "Template combat lesson must be present.");
            RequireEqual(template.CombatLesson, CanonicalCombatLesson, "Template combat lesson");
            Require(
                template.RecommendedPowerDisposition == StageBriefingValueDisposition.NoVerifiedSource
                && template.RecommendedPowerTier == 0
                && template.RecommendedLoadoutDisposition == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.RecommendedLoadout)
                && template.TargetRunDurationDisposition == StageBriefingValueDisposition.NoVerifiedSource
                && template.TargetRunDurationMilliseconds == 0
                && template.FeaturedThreatDisposition == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.FeaturedThreat)
                && template.FeaturedSummonNeedDisposition == StageBriefingValueDisposition.NoVerifiedSource
                && template.FeaturedSummonNeed == StageSummonNeed.None,
                "Unverified template recommendations must remain typed absent with zero payloads.");
            Require(
                template.RestrictionsDisposition == StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                && template.RestrictionCount == 0
                && template.MasteryPreviewDisposition
                    == StageBriefingValueDisposition.NotAuthoredForCurrentSchema
                && string.IsNullOrEmpty(template.MasteryPreview)
                && template.EnemyPreviewDisposition
                    == StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                && template.EnemyPreviewCount == 0
                && template.RewardPreviewDisposition == StageBriefingValueDisposition.NoVerifiedSource
                && string.IsNullOrEmpty(template.RewardPreview)
                && template.CourseSummaryDisposition
                    == StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                && string.IsNullOrEmpty(template.CourseSummary),
                "Optional template arms must retain their frozen typed absence.");
            Require(
                template.SegmentCount == 0,
                "Canonical current-route template must not reuse legacy S1 segment profiles.");
            Require(template.CanonicalRouteSegmentCount == 2, "Template must contain exactly two route segments.");

            StageTemplateRouteSegmentRef corridor = template.GetCanonicalRouteSegment(0);
            ValidateTemplateSegment(
                corridor,
                "olympus-invasion.corridor-tutorial",
                "corridor_intro_tutorial",
                0,
                1);
            ValidateTemplatePocket(
                corridor.GetPocket(0),
                "olympus-invasion.corridor.core-tutorial",
                0,
                StageTemplatePocketObjectiveKind.CompleteTutorialPlan,
                StageTemplateSourceDisposition.CanonicalSemanticDigest,
                StageRunFactVocabulary.OlympusCorridorTutorialPlanId,
                StageRunFactVocabulary.OlympusCorridorTutorialPlanRevision,
                StageRunFactVocabulary.OlympusCorridorTutorialPlanSemanticDigest);

            StageTemplateRouteSegmentRef station = template.GetCanonicalRouteSegment(1);
            ValidateTemplateSegment(
                station,
                "olympus-invasion.station-guide-combat",
                "station_entry_combat",
                1,
                2);
            Require((int)CombatEntryGuideState.Released == 2, "CombatEntryGuideState.Released ordinal changed.");
            ValidateTemplatePocket(
                station.GetPocket(0),
                "olympus-invasion.station.replica-summon-guide",
                0,
                StageTemplatePocketObjectiveKind.CompleteEntryGuide,
                StageTemplateSourceDisposition.RuntimeStateBoundary,
                "CombatEntryGuideState.Released",
                0,
                string.Empty);
            ValidateTemplatePocket(
                station.GetPocket(1),
                "olympus-invasion.station.boss-encounter",
                1,
                StageTemplatePocketObjectiveKind.DefeatBoss,
                StageTemplateSourceDisposition.RouteConditionBoundary,
                StationTerminalConditionId,
                1,
                string.Empty);

            Require(block.BriefingSchemaVersion == 1 && block.BriefingRevision == 2, "Briefing must be revision 2.");
            RequireEqual(block.CanonicalBriefingDigest, CanonicalBriefingDigest, "Stored briefing digest");
            Require(
                block.StoryEntryDisposition == StageReferenceDisposition.Present
                && block.StoryExitDisposition
                    == StageReferenceDisposition.NoFinalSegmentExitPresentationAuthored,
                "Story reference dispositions are invalid.");
            Require(
                block.ResultDefinitionDisposition
                    == StageReferenceDisposition.NotAuthoredForCurrentSchema
                && block.ProgressionNodeDisposition
                    == StageReferenceDisposition.NotAuthoredForCurrentSchema
                && block.RuleSetDisposition == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.ModifierDisposition == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.EnemyVariantDisposition == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.TutorialCourseDisposition == StageReferenceDisposition.NotAdmittedByCurrentSchema
                && block.RewardPlanDisposition == StageReferenceDisposition.NoVerifiedSource,
                "Optional route references must retain their frozen typed absence.");
            Require(
                block.ActiveRunRestartPolicyDisposition
                    == StageBriefingValueDisposition.NotAdmittedByCurrentSchema
                && string.IsNullOrEmpty(block.ActiveRunRestartPolicyDigest),
                "Pre-result restart must remain typed absent and separate from terminal actions.");
            Require(
                ReferenceEquals(route.GetSceneSegment(0).EntryPresentation.CinematicProfile, introProfile),
                "Reference story source must retain the exact canonical cinematic profile.");

            Require(
                route.TryComputeCanonicalBriefingDigest(
                    out string recomputedBriefingDigest,
                    out StageBriefingBuildRejectReason computeRejectReason),
                $"Canonical briefing digest could not be recomputed: {computeRejectReason}.");
            RequireEqual(recomputedBriefingDigest, CanonicalBriefingDigest, "Recomputed briefing digest");
            Require(
                route.TryCreateBriefingReadModel(
                    out StageBriefingReadModel briefing,
                    out StageBriefingBuildRejectReason briefingRejectReason),
                $"Canonical briefing read model is invalid: {briefingRejectReason}.");
            RequireEqual(briefing.CanonicalBriefingDigest, CanonicalBriefingDigest, "Briefing read-model digest");
            RequireEqual(briefing.Title, CanonicalStageTitle, "Briefing title");
            RequireEqual(briefing.Objective, CanonicalStageObjective, "Briefing objective");
            RequireEqual(briefing.CombatLesson, CanonicalCombatLesson, "Briefing combat lesson");
            Require(briefing.SegmentCount == 2, "Briefing must expose two exact route segments.");
            Require(briefing.ActionCount == 3, "Briefing must expose three terminal actions.");
            RequireEqual(briefing.GetAction(0).ActionId, "olympus-invasion.replay", "Briefing action[0]");
            RequireEqual(briefing.GetAction(1).ActionId, "olympus-invasion.retry", "Briefing action[1]");
            RequireEqual(briefing.GetAction(2).ActionId, "olympus-invasion.to-lobby", "Briefing action[2]");
            RequireNoUnityObjectFields(
                typeof(StageBriefingReadModel),
                new HashSet<Type>(),
                nameof(StageBriefingReadModel));
        }

        private static void ValidateTemplateSegment(
            StageTemplateRouteSegmentRef segment,
            string templateSegmentId,
            string routeSegmentId,
            int routeSequenceIndex,
            int pocketCount)
        {
            Require(segment != null, $"Missing template segment {templateSegmentId}.");
            RequireEqual(segment.TemplateSegmentId, templateSegmentId, $"{templateSegmentId}.templateSegmentId");
            RequireEqual(segment.RouteSegmentId, routeSegmentId, $"{templateSegmentId}.routeSegmentId");
            Require(
                segment.RouteSequenceIndex == routeSequenceIndex,
                $"{templateSegmentId}.routeSequenceIndex is invalid.");
            Require(segment.PocketCount == pocketCount, $"{templateSegmentId}.pocketCount is invalid.");
        }

        private static void ValidateTemplatePocket(
            StageTemplatePocketRef pocket,
            string pocketId,
            int sequenceIndex,
            StageTemplatePocketObjectiveKind objectiveKind,
            StageTemplateSourceDisposition sourceDisposition,
            string sourceSemanticId,
            int sourceRevision,
            string sourceSemanticDigest)
        {
            Require(pocket != null, $"Missing template pocket {pocketId}.");
            RequireEqual(pocket.PocketId, pocketId, $"{pocketId}.pocketId");
            Require(pocket.SequenceIndex == sequenceIndex, $"{pocketId}.sequenceIndex is invalid.");
            Require(pocket.ObjectiveKind == objectiveKind, $"{pocketId}.objectiveKind is invalid.");
            Require(
                pocket.CurrentExecutionOwnerDisposition
                    == StageTemplateCurrentExecutionOwnerDisposition.ExistingSceneOwner,
                $"{pocketId} must retain ExistingSceneOwner.");
            Require(
                pocket.P1CAdmissionDisposition == StageTemplateP1CAdmissionDisposition.NotAdmitted,
                $"{pocketId} must remain NotAdmitted for P1-C.");
            Require(pocket.SourceDisposition == sourceDisposition, $"{pocketId}.sourceDisposition is invalid.");
            RequireEqual(pocket.SourceSemanticId, sourceSemanticId, $"{pocketId}.sourceSemanticId");
            Require(pocket.SourceRevision == sourceRevision, $"{pocketId}.sourceRevision is invalid.");
            RequireEqual(
                pocket.SourceSemanticDigest,
                sourceSemanticDigest,
                $"{pocketId}.sourceSemanticDigest");
            Require(pocket.EnemyRoleCount == 0, $"{pocketId} must not invent enemy-role data.");
        }

        private static void ValidateResultProgressionJoin(PlayableStageDefinition route)
        {
            StageResultDefinition result =
                LoadRequiredAsset<StageResultDefinition>(StageResultDefinitionPath);
            StageProgressionNode node =
                LoadRequiredAsset<StageProgressionNode>(StageProgressionNodePath);
            StageProgressionGraph graph =
                LoadRequiredAsset<StageProgressionGraph>(StageProgressionGraphPath);
            StageResultPresentationCatalog presentationCatalog =
                LoadRequiredAsset<StageResultPresentationCatalog>(StageResultPresentationCatalogPath);
            StageResultProgressionJoinBlock block = route.ResultProgressionJoin;

            Require(block != null && block.Present, "Result/progression sibling sidecar is missing.");
            Require(
                block.SchemaVersion == 1 && block.Revision == 2,
                "Result/progression sibling sidecar must be revision 2.");
            Require(
                ReferenceEquals(block.ResultDefinition, result)
                    && ReferenceEquals(block.CanonicalPresentationCatalog, presentationCatalog)
                    && ReferenceEquals(block.ProgressionNode, node)
                    && ReferenceEquals(block.ProgressionGraph, graph),
                "Result/progression sidecar must retain the exact canonical asset references.");
            Require(
                graph.NodeCount == 1 && ReferenceEquals(graph.GetNode(0), node),
                "Progression graph must contain the canonical node exactly once.");
            Require(node.Revision == 1, "Progression node content identity must remain revision 1.");
            Require(node.BindingRevision == 2, "Progression node route binding must be revision 2.");
            Require(graph.Revision == 2, "Progression graph must be revision 2.");
            bool exactPresentationSources = presentationCatalog.TryValidateExactSources(
                route.PlayableStageId,
                result.PresentationProfile,
                result.LocalizationTable,
                out string exactPresentationSourceError);
            Require(
                ReferenceEquals(result.CanonicalPresentationCatalog, presentationCatalog)
                    && exactPresentationSources,
                "Result definition must retain the exact canonical catalog/profile/localization references: "
                    + exactPresentationSourceError);
            RequireEqual(
                result.EvaluationContentDigest,
                CanonicalResultEvaluationDigest,
                "Result evaluation digest");
            RequireEqual(
                result.PresentationBindingDigest,
                CanonicalResultPresentationBindingDigest,
                "Result presentation binding digest");
            RequireEqual(
                result.PresentationSourceDigest,
                CanonicalResultPresentationSourceDigest,
                "Result presentation source digest");
            RequireEqual(
                node.ContentDigest,
                CanonicalProgressionNodeContentDigest,
                "Progression node content digest");
            RequireEqual(
                node.BindingDigest,
                CanonicalProgressionNodeBindingDigest,
                "Progression node binding digest");
            RequireEqual(
                graph.CanonicalDigest,
                CanonicalProgressionGraphDigest,
                "Progression graph digest");
            RequireEqual(
                block.CanonicalDigest,
                CanonicalResultProgressionJoinDigest,
                "Result/progression sidecar digest");
            Require(
                result.TryComputeCanonicalDigests(
                    out string computedEvaluationDigest,
                    out string computedPresentationBindingDigest,
                    out string computedPresentationSourceDigest,
                    out string resultComputeError),
                $"Result-definition authoring digests are not computable: {resultComputeError}");
            RequireEqual(
                computedEvaluationDigest,
                CanonicalResultEvaluationDigest,
                "Computed result evaluation digest");
            RequireEqual(
                computedPresentationBindingDigest,
                CanonicalResultPresentationBindingDigest,
                "Computed result presentation binding digest");
            RequireEqual(
                computedPresentationSourceDigest,
                CanonicalResultPresentationSourceDigest,
                "Computed result presentation source digest");
            Require(
                node.TryComputeCanonicalDigests(
                    out string computedNodeContentDigest,
                    out string computedNodeBindingDigest,
                    out string nodeComputeError),
                $"Progression-node authoring digests are not computable: {nodeComputeError}");
            RequireEqual(
                computedNodeContentDigest,
                CanonicalProgressionNodeContentDigest,
                "Computed progression node content digest");
            RequireEqual(
                computedNodeBindingDigest,
                CanonicalProgressionNodeBindingDigest,
                "Computed progression node binding digest");
            Require(
                graph.TryComputeCanonicalDigest(
                    out string computedGraphDigest,
                    out string graphComputeError),
                $"Progression-graph authoring digest is not computable: {graphComputeError}");
            RequireEqual(
                computedGraphDigest,
                CanonicalProgressionGraphDigest,
                "Computed progression graph digest");
            Require(
                route.TryComputeResultProgressionJoinDigest(
                    out string computedJoinDigest,
                    out string joinComputeError),
                $"Result/progression join authoring digest is not computable: {joinComputeError}");
            RequireEqual(
                computedJoinDigest,
                CanonicalResultProgressionJoinDigest,
                "Computed result/progression join digest");

            Require(
                StageRunResultProgressionJoinSnapshot.TryCreate(
                    route,
                    out StageRunResultProgressionJoinSnapshot snapshot,
                    out string snapshotError),
                $"Result/progression admission snapshot is invalid: {snapshotError}");
            RequireEqual(
                snapshot.CanonicalDigest,
                CanonicalResultProgressionJoinDigest,
                "Recomputed admission join snapshot digest");
            RequireEqual(
                snapshot.ResultDefinition.EvaluationContentDigest,
                CanonicalResultEvaluationDigest,
                "Recomputed result evaluation digest");
            RequireEqual(
                snapshot.PresentationSource.PresentationBindingDigest,
                CanonicalResultPresentationBindingDigest,
                "Recomputed presentation binding digest");
            RequireEqual(
                snapshot.PresentationSource.CanonicalDigest,
                CanonicalResultPresentationSourceDigest,
                "Recomputed presentation source digest");
            RequireEqual(
                snapshot.ProgressionNode.ContentDigest,
                CanonicalProgressionNodeContentDigest,
                "Recomputed progression node content digest");
            RequireEqual(
                snapshot.ProgressionNode.BindingDigest,
                CanonicalProgressionNodeBindingDigest,
                "Recomputed progression node binding digest");
            RequireEqual(
                snapshot.ProgressionGraph.CanonicalDigest,
                CanonicalProgressionGraphDigest,
                "Recomputed progression graph digest");
            RequireNoUnityObjectFields(
                typeof(StageRunResultProgressionJoinSnapshot),
                new HashSet<Type>(),
                nameof(StageRunResultProgressionJoinSnapshot));

            string presenterSource = File.ReadAllText(
                ToAbsoluteProjectPath(StageClearPresenterPath));
            Require(
                presenterSource.Contains(
                    "StageRunRuntime.TryPrepareResultPresentation(",
                    StringComparison.Ordinal),
                "Stage-clear presenter must consume the exact run-owned presentation snapshot.");
            Require(
                !presenterSource.Contains(
                    "presentationCatalog.TryCreateSnapshot(",
                    StringComparison.Ordinal),
                "Stage-clear presenter must not reread the live presentation catalog at result time.");
        }

        private static void RequireNoUnityObjectFields(
            Type type,
            HashSet<Type> visited,
            string path)
        {
            Require(
                !typeof(UnityEngine.Object).IsAssignableFrom(type),
                $"Runtime DTO field {path} must not reference UnityEngine.Object ({type.FullName}).");
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
            {
                return;
            }

            if (type.IsArray)
            {
                RequireNoUnityObjectFields(type.GetElementType(), visited, path + "[]");
                return;
            }

            if (!visited.Add(type))
            {
                return;
            }

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly);
            for (int i = 0; i < fields.Length; i++)
            {
                RequireNoUnityObjectFields(
                    fields[i].FieldType,
                    visited,
                    path + "." + fields[i].Name);
            }
        }

        private static void ValidateSegmentIdentity(
            StageSceneSegmentRef segment,
            string segmentId,
            int sequenceIndex,
            StageDefinitionProfile stageDefinition,
            string entryConditionId,
            StageSegmentConditionKind entryConditionKind,
            string exitConditionId,
            StageSegmentConditionKind exitConditionKind)
        {
            Require(segment != null, $"Segment {segmentId} is missing.");
            RequireEqual(segment.SegmentId, segmentId, $"{segmentId}.segmentId");
            Require(segment.SequenceIndex == sequenceIndex, $"{segmentId}.sequenceIndex is invalid.");
            Require(
                ReferenceEquals(segment.StageDefinition, stageDefinition),
                $"{segmentId}.stageDefinition must be the canonical profile reference.");
            RequireEqual(segment.EntryConditionId, entryConditionId, $"{segmentId}.entryConditionId");
            Require(
                segment.EntryConditionKind == entryConditionKind,
                $"{segmentId}.entryConditionKind is invalid.");
            RequireEqual(segment.ExitConditionId, exitConditionId, $"{segmentId}.exitConditionId");
            Require(
                segment.ExitConditionKind == exitConditionKind,
                $"{segmentId}.exitConditionKind is invalid.");
        }

        private static void ValidateIntroPresentationContract(
            StageSceneSegmentRef corridorSegment,
            StageDefinitionProfile corridor,
            CinematicSequenceProfile introProfile,
            PlayableAsset introPlayable)
        {
            StagePresentationHandoffRef presentation = corridorSegment.EntryPresentation;
            Require(IsPresent(presentation), "Corridor entry presentation is missing.");
            Require(
                ReferenceEquals(presentation.StageDefinition, corridor),
                "Corridor entry presentation must directly reference the canonical Corridor definition.");
            RequireEqual(presentation.HandoffId, IntroHandoffId, "Corridor entry presentation handoffId");
            Require(
                ReferenceEquals(presentation.CinematicProfile, introProfile),
                "Corridor entry presentation must directly reference the combined cinematic profile.");
            RequireEqual(presentation.ExpectedPortId, IntroPortId, "Corridor entry presentation portId");
            Require(
                ReferenceEquals(presentation.ExpectedPlayableAsset, introPlayable),
                "Corridor entry presentation must directly reference the combined Timeline asset.");
            RequireEqual(
                presentation.TriggerConditionId,
                RunEntryConditionId,
                "Corridor entry presentation triggerConditionId");
            RequireEqual(
                presentation.CompletionConditionId,
                IntroCompletionConditionId,
                "Corridor entry presentation completionConditionId");

            int handoffCount = 0;
            StageDefinitionProfile.CutsceneHandoffRef handoff = default;
            for (int i = 0; i < corridor.CutsceneHandoffCount; i++)
            {
                StageDefinitionProfile.CutsceneHandoffRef candidate = corridor.GetCutsceneHandoff(i);
                if (string.Equals(candidate.HandoffId, presentation.HandoffId, StringComparison.Ordinal))
                {
                    handoff = candidate;
                    handoffCount++;
                }
            }

            Require(handoffCount == 1, "Corridor definition must own intro-to-stage exactly once.");
            RequireEqual(handoff.AnchorId, introProfile.StageAnchorId, "intro-to-stage anchorId");
            RequireEqual(
                handoff.CinematicProfileId,
                introProfile.SequenceId,
                "intro-to-stage cinematicProfileId alias");
            RequireEqual(
                handoff.TimelineAssetPath,
                AssetDatabase.GetAssetPath(introPlayable),
                "intro-to-stage Timeline path alias");
            RequireEqual(
                handoff.NextEventId,
                presentation.CompletionConditionId,
                "intro-to-stage nextEventId");

            RequireEqual(introProfile.SequenceId, introProfile.name, "Combined cinematic profile asset-name alias");
            Require(
                ReferenceEquals(introProfile.StageDefinition, corridor),
                "Combined cinematic profile must reference the canonical Corridor definition.");
            RequireEqual(introProfile.StageHandoffId, presentation.HandoffId, "Combined profile handoffId");

            int anchorCount = 0;
            for (int i = 0; i < corridor.AnchorCount; i++)
            {
                if (string.Equals(
                    corridor.GetAnchor(i).AnchorId,
                    introProfile.StageAnchorId,
                    StringComparison.Ordinal))
                {
                    anchorCount++;
                }
            }

            Require(anchorCount == 1, "Combined profile stage anchor must resolve exactly once.");

            int runtimeStateCount = 0;
            StageDefinitionProfile.RuntimeStateRef runtimeState = default;
            for (int i = 0; i < corridor.RuntimeStateCount; i++)
            {
                StageDefinitionProfile.RuntimeStateRef candidate = corridor.GetRuntimeState(i);
                if (string.Equals(
                    candidate.StateId,
                    introProfile.StageRuntimeStateId,
                    StringComparison.Ordinal))
                {
                    runtimeState = candidate;
                    runtimeStateCount++;
                }
            }

            Require(runtimeStateCount == 1, "Combined profile runtime state must resolve exactly once.");
            Require(
                runtimeState.StateKind == StageRuntimeStateKind.CutsceneHandoff,
                "Combined profile runtime state must be a CutsceneHandoff.");
            RequireEqual(runtimeState.AnchorId, introProfile.StageAnchorId, "Combined profile runtime-state anchorId");
            RequireEqual(
                runtimeState.ConditionId,
                presentation.CompletionConditionId,
                "Combined profile runtime-state conditionId");
        }

        private static void ValidateStageCatalogInventory(UIStageCatalog catalog)
        {
            Require(catalog != null, "Stage catalog is missing.");
            Require(
                catalog.TryValidateEntryIdentities(
                    out UIStageRouteProjectionRejectReason identityRejectReason),
                $"Stage catalog entry identities are invalid: {identityRejectReason}.");

            var playableStageIds = new HashSet<string>(StringComparer.Ordinal);
            var resultDefinitionIds = new HashSet<string>(StringComparer.Ordinal);
            var progressionNodeIds = new HashSet<string>(StringComparer.Ordinal);
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

            for (int catalogIndex = 0; catalogIndex < catalog.StageCount; catalogIndex++)
            {
                UIStageCatalog.StageEntry entry = catalog.GetStage(catalogIndex);
                string rowLabel = $"Stage catalog row {catalogIndex} ('{entry.Id}')";
                Require(
                    catalog.TryCreateRouteProjection(
                        entry.Id,
                        UIRouteId.Combat,
                        out UIStageRouteProjection namedProjection,
                        out UIStageRouteProjectionRejectReason namedRejectReason),
                    $"{rowLabel} named projection is invalid: {namedRejectReason}.");
                Require(
                    catalog.TryCreateRouteProjection(
                        catalogIndex,
                        UIRouteId.Combat,
                        out UIStageRouteProjection indexedProjection,
                        out UIStageRouteProjectionRejectReason indexedRejectReason),
                    $"{rowLabel} indexed projection is invalid: {indexedRejectReason}.");
                Require(
                    namedProjection != null
                        && indexedProjection != null
                        && ReferenceEquals(namedProjection.PlayableStage, entry.PlayableStage)
                        && ReferenceEquals(indexedProjection.PlayableStage, entry.PlayableStage)
                        && string.Equals(
                            namedProjection.CatalogEntryId,
                            indexedProjection.CatalogEntryId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            namedProjection.CanonicalProjectionDigest,
                            indexedProjection.CanonicalProjectionDigest,
                            StringComparison.Ordinal)
                        && string.Equals(
                            namedProjection.EntryScenePath,
                            indexedProjection.EntryScenePath,
                            StringComparison.Ordinal)
                        && namedProjection.ResultProgressionJoinPreflight != null
                        && indexedProjection.ResultProgressionJoinPreflight != null
                        && string.Equals(
                            namedProjection.ResultProgressionJoinPreflight.CanonicalDigest,
                            indexedProjection.ResultProgressionJoinPreflight.CanonicalDigest,
                            StringComparison.Ordinal),
                    $"{rowLabel} named and indexed projections must resolve the same immutable route bundle.");
                Require(
                    catalog.IsProjectionCurrent(
                        namedProjection,
                        UIRouteId.Combat,
                        out UIStageRouteProjectionRejectReason namedCurrentRejectReason),
                    $"{rowLabel} named projection is stale: {namedCurrentRejectReason}.");
                Require(
                    catalog.IsProjectionCurrent(
                        indexedProjection,
                        UIRouteId.Combat,
                        out UIStageRouteProjectionRejectReason indexedCurrentRejectReason),
                    $"{rowLabel} indexed projection is stale: {indexedCurrentRejectReason}.");

                PlayableStageDefinition playableStage = entry.PlayableStage;
                Require(
                    StageRunRouteSnapshot.TryCreate(
                        playableStage,
                        out StageRunRouteSnapshot routeSnapshot,
                        out string routeSnapshotError),
                    $"{rowLabel} route snapshot is invalid: {routeSnapshotError}");
                Require(
                    string.Equals(
                        routeSnapshot.PlayableStageId,
                        namedProjection.PlayableStageId,
                        StringComparison.Ordinal),
                    $"{rowLabel} route snapshot identity disagrees with its projection.");

                Require(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        playableStage,
                        out StageRunResultProgressionJoinSnapshot joinSnapshot,
                        out string joinSnapshotError),
                    $"{rowLabel} result/progression join snapshot is invalid: {joinSnapshotError}");
                Require(
                    joinSnapshot.TryValidateIntegrity(out string joinIntegrityError),
                    $"{rowLabel} result/progression join integrity is invalid: {joinIntegrityError}");
                Require(
                    string.Equals(
                        joinSnapshot.CanonicalDigest,
                        namedProjection.ResultProgressionJoinPreflight.CanonicalDigest,
                        StringComparison.Ordinal),
                    $"{rowLabel} projection result/progression preflight is stale.");

                Require(
                    playableStageIds.Add(routeSnapshot.PlayableStageId),
                    $"Reachable playableStageId '{routeSnapshot.PlayableStageId}' must be unique across the stage catalog.");
                Require(
                    resultDefinitionIds.Add(joinSnapshot.ResultDefinition.ResultDefinitionId),
                    $"Reachable resultDefinitionId '{joinSnapshot.ResultDefinition.ResultDefinitionId}' must be unique across the stage catalog.");
                Require(
                    progressionNodeIds.Add(joinSnapshot.ProgressionNode.ProgressionNodeId),
                    $"Reachable progressionNodeId '{joinSnapshot.ProgressionNode.ProgressionNodeId}' must be unique across the stage catalog.");

                for (int segmentIndex = 0; segmentIndex < routeSnapshot.SegmentCount; segmentIndex++)
                {
                    StageRunSegmentSnapshot segment = routeSnapshot.GetSegment(segmentIndex);
                    Require(
                        AssetDatabase.LoadAssetAtPath<SceneAsset>(segment.ScenePath) != null,
                        $"{rowLabel} segment {segmentIndex} ('{segment.SegmentId}') scene asset is missing: {segment.ScenePath}.");
                    Require(
                        FindEnabledSceneIndex(buildScenes, segment.ScenePath) >= 0,
                        $"{rowLabel} segment {segmentIndex} ('{segment.SegmentId}') scene must be enabled in Build Settings: {segment.ScenePath}.");
                }
            }
        }

        private static void ValidateStageCatalogSelection(
            UIStageCatalog catalog,
            PlayableStageDefinition route,
            StageDefinitionProfile corridor)
        {
            Require(
                catalog.ProjectionSchemaVersion == UIStageCatalog.SupportedProjectionSchemaVersion,
                "Stage catalog projection schema must be revision 1.");
            Require(
                catalog.CatalogProjectionGeneration == 2,
                "Stage catalog projection generation must remain 2 for the single-stage product cohort.");
            Require(
                catalog.StageCount == 1,
                "Stage catalog must contain exactly the accepted Olympus training route while Courtyard remains quarantined.");
            RequireEqual(
                catalog.GetStage(0).Id,
                CanonicalCatalogEntryId,
                "Stage catalog row 0 entry ID");
            Require(
                catalog.TryGetStage(CanonicalCatalogEntryId, out UIStageCatalog.StageEntry entry),
                "Stage catalog must expose the canonical Olympus product entry exactly once.");
            RequireEqual(entry.Id, CanonicalCatalogEntryId, "Canonical stage catalog entry ID");
            RequireEqual(entry.DisplayName, CanonicalStageTitle, "Legacy stage-title mirror");
            RequireEqual(entry.Summary, CanonicalStageObjective, "Legacy stage-objective mirror");
            Require(
                entry.PresentationProvenance
                    == UIStagePresentationProvenance.LegacyPresentationOnly,
                "Canonical stage catalog copy must retain explicit legacy-only provenance.");
            Require(
                string.IsNullOrWhiteSpace(entry.MockRewardPreview),
                "Stage catalog reward preview must remain empty until a verified progression source exists.");
            Require(
                ReferenceEquals(entry.PlayableStage, route),
                "Canonical stage catalog entry must directly reference OLYMPUS-INVASION-01.");
            RequireEqual(
                entry.CanonicalProjectionDigest,
                CanonicalProjectionDigest,
                "Stored canonical stage-selection projection digest");
            Require(
                catalog.TryCreateRouteProjection(
                    CanonicalCatalogEntryId,
                    UIRouteId.Combat,
                    out UIStageRouteProjection projection,
                    out UIStageRouteProjectionRejectReason rejectReason),
                $"Canonical stage catalog projection is invalid: {rejectReason}.");
            Require(
                projection.ProjectionSchemaVersion == 1,
                "Catalog projection schema version must be 1.");
            Require(
                projection.CatalogProjectionGeneration == 2,
                "Catalog projection generation must be 2.");
            RequireEqual(
                projection.CatalogEntryId,
                CanonicalCatalogEntryId,
                "Catalog projection entry ID");
            Require(
                ReferenceEquals(projection.PlayableStage, route),
                "Stage catalog projection must retain the exact playable-stage object.");
            Require(projection.RouteSchemaVersion == route.SchemaVersion, "Catalog projection schema is stale.");
            RequireEqual(projection.PlayableStageId, route.PlayableStageId, "Catalog projection playableStageId");
            Require(projection.RouteRevision == route.RouteRevision, "Catalog projection route revision is stale.");
            RequireEqual(
                projection.StoredCanonicalRouteDigest,
                route.CanonicalRouteDigest,
                "Catalog projection stored route digest");
            RequireEqual(
                projection.RecomputedCanonicalRouteDigest,
                route.ComputeCanonicalRouteDigest(),
                "Catalog projection recomputed route digest");
            RequireEqual(
                projection.CanonicalProjectionDigest,
                CanonicalProjectionDigest,
                "Catalog projection digest");
            Require(
                ReferenceEquals(projection.StageTemplate, route.ReferenceBlock.StageTemplate),
                "Catalog projection must retain the exact canonical stage-template object.");
            RequireEqual(
                projection.CanonicalReferenceDigest,
                CanonicalReferenceDigest,
                "Catalog projection reference digest");
            RequireEqual(
                projection.CanonicalTemplateDigest,
                CanonicalTemplateDigest,
                "Catalog projection template digest");
            RequireEqual(
                projection.CanonicalBriefingDigest,
                CanonicalBriefingDigest,
                "Catalog projection briefing digest");
            Require(projection.Briefing != null, "Catalog projection must own an immutable briefing instance.");
            RequireEqual(
                projection.Briefing.CanonicalBriefingDigest,
                CanonicalBriefingDigest,
                "Catalog projection briefing-instance digest");
            RequireEqual(projection.DisplayName, CanonicalStageTitle, "Catalog projection title");
            RequireEqual(projection.Summary, CanonicalStageObjective, "Catalog projection objective");
            Require(
                string.IsNullOrEmpty(projection.ThreatTags)
                && string.IsNullOrEmpty(projection.RecommendedSummonRole)
                && string.IsNullOrEmpty(projection.RewardPreview),
                "Unverified optional presentation rows must not fall back to legacy catalog copy.");

            StageSceneSegmentRef entrySegment = route.GetSceneSegment(0);
            RequireEqual(projection.EntrySegmentId, entrySegment.SegmentId, "Catalog projection entry segmentId");
            Require(projection.EntrySequenceIndex == 0, "Catalog projection entry sequence must be zero.");
            Require(
                ReferenceEquals(projection.EntryStageDefinition, corridor),
                "Catalog projection must retain the exact Corridor stage-definition object.");
            RequireEqual(
                projection.EntryStageDefinitionId,
                corridor.StageId,
                "Catalog projection entry stageDefinitionId");
            RequireEqual(projection.EntryScenePath, corridor.MapScenePath, "Catalog projection entry scene path");
            RequireEqual(
                projection.EntrySceneName,
                Path.GetFileNameWithoutExtension(corridor.MapScenePath),
                "Catalog projection entry scene name");
            RequireEqual(projection.LoadingCardId, CanonicalLoadingCardId, "Catalog projection loadingCardId");
            Require(
                projection.UiRouteId == UIRouteId.Combat,
                "Catalog projection must dispatch only UIRouteId.Combat.");
            Require(
                projection.PresentationProvenance
                    == UIStagePresentationProvenance.LegacyPresentationOnly,
                "Catalog projection copy must retain legacy-only provenance.");
            Require(
                string.IsNullOrWhiteSpace(projection.RewardPreview),
                "Catalog projection must not expose an unverified reward preview.");
            Require(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(projection.EntryScenePath) != null,
                "Catalog projection entry scene path must resolve an exact scene asset.");
            Require(
                FindEnabledSceneIndex(EditorBuildSettings.scenes, projection.EntryScenePath) >= 0,
                "Catalog projection entry scene must be enabled in Build Settings.");
            Require(
                !catalog.TryGetStage("story_v1_retry_route", out _),
                "Retry must remain a terminal action, not a second selectable stage entry.");

            UIScreenRouteTable routeTable = LoadRequiredAsset<UIScreenRouteTable>(UiRouteTablePath);
            Require(
                routeTable.TryGetRoute(UIRouteId.Combat, out UIScreenRouteTable.Route combatRoute),
                "DB_UIRouteTable must resolve UIRouteId.Combat.");
            RequireEqual(combatRoute.ScenePath, projection.EntryScenePath, "Combat UI route scene path");
            RequireEqual(combatRoute.SceneName, projection.EntrySceneName, "Combat UI route scene name");
            RequireEqual(combatRoute.LoadingCardId, projection.LoadingCardId, "Combat UI route loading card");

            GameObject prefab = LoadRequiredAsset<GameObject>(StageSelectPrefabPath);
            StageSelectScreenPresenter[] presenters =
                prefab.GetComponentsInChildren<StageSelectScreenPresenter>(true);
            Require(presenters.Length == 1, "Stage-select prefab must contain exactly one presenter.");
            SerializedObject serializedPresenter = new(presenters[0]);
            Require(
                ReferenceEquals(
                    serializedPresenter.FindProperty("stageCatalog")?.objectReferenceValue,
                    catalog),
                "Stage-select presenter must consume the canonical stage catalog.");
            RequireEqual(
                serializedPresenter.FindProperty("selectedStageId")?.stringValue,
                catalog.GetStage(0).Id,
                "Stage-select selectedStageId");
            Require(
                serializedPresenter.FindProperty("startRoute")?.intValue == (int)UIRouteId.Combat,
                "Stage-select prefab start route must be UIRouteId.Combat.");
            Text stageNameText =
                serializedPresenter.FindProperty("stageNameText")?.objectReferenceValue as Text;
            Text summaryText =
                serializedPresenter.FindProperty("summaryText")?.objectReferenceValue as Text;
            Require(
                stageNameText != null
                    && string.Equals(
                        stageNameText.gameObject.name,
                        "CurrentChapterTitleText",
                        StringComparison.Ordinal),
                "Stage-select presenter must bind CurrentChapterTitleText as its stage-name row.");
            Require(
                summaryText != null
                    && string.Equals(
                        summaryText.gameObject.name,
                        "CurrentChapterBodyText",
                        StringComparison.Ordinal),
                "Stage-select presenter must bind CurrentChapterBodyText as its objective row.");
            Text combatLessonText =
                serializedPresenter.FindProperty("combatLessonText")?.objectReferenceValue as Text;
            Require(
                combatLessonText != null,
                "Stage-select presenter must bind its canonical combat-lesson Text component.");
            RequireEqual(
                combatLessonText.gameObject.name,
                "CurrentChapterLessonText",
                "Stage-select combat-lesson object name");
            Require(
                string.IsNullOrEmpty(combatLessonText.text) && combatLessonText.gameObject.activeSelf,
                "Stage-select combat-lesson row must be authored empty, active, and ready for briefing rendering.");
            Text rewardPreviewText =
                serializedPresenter.FindProperty("rewardPreviewText")?.objectReferenceValue as Text;
            Require(
                rewardPreviewText != null,
                "Stage-select presenter must bind its optional reward-row Text component.");
            RequireEqual(
                rewardPreviewText.gameObject.name,
                "CurrentChapterRewardText",
                "Stage-select reward-row object name");
            Require(
                string.IsNullOrEmpty(rewardPreviewText.text) && !rewardPreviewText.gameObject.activeSelf,
                "Stage-select unverified reward row must be authored empty and inactive.");
            ValidateStageSelectDetailLayout(
                prefab.transform,
                stageNameText,
                summaryText,
                combatLessonText,
                rewardPreviewText);
            Button startButton =
                serializedPresenter.FindProperty("startButton")?.objectReferenceValue as Button;
            ValidateVisibleStageSelectStartButton(prefab.transform, startButton);
            ValidateTruthfulStageSelectChapterInventory(prefab.transform);
            ValidateHiddenStageSelectDetailObject(
                prefab.transform,
                "ChapterProgressLabel",
                requireEmptyText: true);
            ValidateHiddenStageSelectDetailObject(
                prefab.transform,
                "ChapterPercentText",
                requireEmptyText: true);
            ValidateHiddenStageSelectDetailObject(
                prefab.transform,
                "ChapterProgress",
                requireEmptyText: false);
            ValidateHiddenStageSelectDetailObject(
                prefab.transform,
                "ChapterProgressBackground",
                requireEmptyText: false);
            ValidateHiddenStageSelectDetailObject(
                prefab.transform,
                "SummaryFrame",
                requireEmptyText: false);
            ValidateHiddenStageSelectDetailObject(
                prefab.transform,
                "SummaryText",
                requireEmptyText: true);
            SerializedProperty startRequested =
                serializedPresenter.FindProperty("startRequested")
                    ?.FindPropertyRelative("m_PersistentCalls")
                    ?.FindPropertyRelative("m_Calls");
            Require(
                startRequested != null && startRequested.arraySize == 0,
                "Stage-select startRequested must have no admission or navigation listener.");
            ValidateStageSelectCardBindings(prefab, serializedPresenter, catalog);
            ValidateStageSelectRouteInteractableGate(prefab, serializedPresenter, catalog);

            WithLoadedScene(
                StageSelectScenePath,
                scene =>
                {
                    StageSelectScreenPresenter[] scenePresenters =
                        CollectSceneComponents<StageSelectScreenPresenter>(scene);
                    UISceneFlowRouter[] routers = CollectSceneComponents<UISceneFlowRouter>(scene);
                    UISceneRouteLoader[] loaders = CollectSceneComponents<UISceneRouteLoader>(scene);
                    Require(
                        scenePresenters.Length == 1,
                        "UI_StageSelect must contain exactly one stage-select presenter.");
                    Require(
                        routers.Length == 1,
                        "UI_StageSelect must contain exactly one UI scene-flow router.");
                    Require(
                        loaders.Length == 1,
                        "UI_StageSelect must contain exactly one UI scene-route loader.");

                    SerializedObject scenePresenter = new(scenePresenters[0]);
                    Require(
                        ReferenceEquals(
                            scenePresenter.FindProperty("stageCatalog")?.objectReferenceValue,
                            catalog),
                        "UI_StageSelect presenter must consume DB_UIStageCatalog.");
                    Require(
                        ReferenceEquals(
                            scenePresenter.FindProperty("router")?.objectReferenceValue,
                            routers[0]),
                        "UI_StageSelect presenter must bind the exact scene-flow router.");
                    Require(
                        scenePresenter.FindProperty("startRoute")?.intValue
                            == (int)projection.UiRouteId,
                        "UI_StageSelect presenter route must match the canonical projection.");

                    SerializedObject serializedRouter = new(routers[0]);
                    Require(
                        ReferenceEquals(
                            serializedRouter.FindProperty("routeTable")?.objectReferenceValue,
                            routeTable),
                        "UI_StageSelect router must reference DB_UIRouteTable.");
                    Require(
                        ReferenceEquals(
                            serializedRouter.FindProperty("routeLoader")?.objectReferenceValue,
                            loaders[0]),
                        "UI_StageSelect router must bind the exact scene route-loader object.");
                });

            WithLoadedScene(
                CorridorScenePath,
                scene =>
                {
                    OlympusCorridorCombatFlowController[] flows =
                        CollectSceneComponents<OlympusCorridorCombatFlowController>(scene);
                    Require(
                        flows.Length == 1,
                        "Catalog destination Corridor must contain exactly one canonical flow.");
                    SerializedObject serializedFlow = new(flows[0]);
                    Require(
                        ReferenceEquals(
                            serializedFlow.FindProperty("playableStageDefinition")
                                ?.objectReferenceValue,
                            projection.PlayableStage),
                        "Catalog destination flow must bind the exact projected playable-stage object.");
                });

            string presenterSource = File.ReadAllText(ToAbsoluteProjectPath(StageSelectPresenterPath));
            string catalogSource = File.ReadAllText(ToAbsoluteProjectPath(StageCatalogSourcePath));
            Require(
                !presenterSource.Contains("TryAdmitFirstSegment", StringComparison.Ordinal)
                    && !presenterSource.Contains("StageRunRuntime", StringComparison.Ordinal)
                    && !catalogSource.Contains("TryAdmitFirstSegment", StringComparison.Ordinal)
                    && !catalogSource.Contains("StageRunRuntime", StringComparison.Ordinal),
                "Stage selection must not become a second route admission owner.");
        }

        private static void ValidateStageSelectDetailLayout(
            Transform prefabRoot,
            Text titleText,
            Text objectiveText,
            Text lessonText,
            Text rewardText)
        {
            RectTransform titleRect = titleText.rectTransform;
            RectTransform objectiveRect = objectiveText.rectTransform;
            RectTransform lessonRect = lessonText.rectTransform;
            RectTransform rewardRect = rewardText.rectTransform;
            RectTransform numberRect = FindUniqueDescendant(
                    prefabRoot,
                    "CurrentChapterNumberText",
                    "Stage-select truthful detail panel") as RectTransform;
            RectTransform startRect = FindUniqueDescendant(
                    prefabRoot,
                    "StartButton",
                    "Stage-select truthful detail panel") as RectTransform;
            const float MinimumVerticalGap = 0.005f;
            Require(
                numberRect != null && startRect != null,
                "Stage-select detail layout requires RectTransforms for its number and Start button.");
            Require(
                titleText.gameObject.activeSelf
                    && objectiveText.gameObject.activeSelf
                    && lessonText.gameObject.activeSelf,
                "Stage-select title, objective, and combat-lesson rows must be authored active.");
            Require(
                titleRect.parent == numberRect.parent
                    && titleRect.parent == objectiveRect.parent
                    && titleRect.parent == lessonRect.parent
                    && titleRect.parent == rewardRect.parent
                    && titleRect.parent == startRect.parent
                    && numberRect.anchorMin.y >= titleRect.anchorMax.y + MinimumVerticalGap
                    && titleRect.anchorMin.y >= objectiveRect.anchorMax.y + MinimumVerticalGap
                    && objectiveRect.anchorMin.y >= lessonRect.anchorMax.y + MinimumVerticalGap
                    && lessonRect.anchorMin.y >= startRect.anchorMax.y + MinimumVerticalGap
                    && lessonRect.anchorMin.y >= rewardRect.anchorMax.y + MinimumVerticalGap
                    && rewardRect.anchorMax.x <= startRect.anchorMin.x - MinimumVerticalGap,
                "Stage-select number, title, objective, lesson, optional reward, and Start control must have non-overlapping bounds.");
            Require(
                titleText.resizeTextForBestFit
                    && objectiveText.resizeTextForBestFit
                    && lessonText.resizeTextForBestFit
                    && titleText.fontStyle == FontStyle.Bold
                    && objectiveText.fontStyle == FontStyle.Normal
                    && lessonText.fontStyle == FontStyle.Normal
                    && !titleText.raycastTarget
                    && !objectiveText.raycastTarget
                    && !lessonText.raycastTarget
                    && titleText.horizontalOverflow == HorizontalWrapMode.Wrap
                    && objectiveText.horizontalOverflow == HorizontalWrapMode.Wrap
                    && lessonText.horizontalOverflow == HorizontalWrapMode.Wrap
                    && titleText.verticalOverflow == VerticalWrapMode.Truncate
                    && objectiveText.verticalOverflow == VerticalWrapMode.Truncate
                    && lessonText.verticalOverflow == VerticalWrapMode.Truncate,
                "Stage-select title, objective, and lesson must use wrapped best-fit text inside bounded rows.");
        }

        private static void ValidateHiddenStageSelectDetailObject(
            Transform prefabRoot,
            string objectName,
            bool requireEmptyText)
        {
            Transform target = FindUniqueDescendant(
                prefabRoot,
                objectName,
                "Stage-select truthful detail panel");
            Text text = target.GetComponent<Text>();
            Require(
                !target.gameObject.activeSelf
                    && (!requireEmptyText
                        || (text != null && string.IsNullOrEmpty(text.text))),
                $"Stage-select '{objectName}' must be inactive"
                + (requireEmptyText ? " and have empty text." : "."));
        }

        private static void ValidateVisibleStageSelectStartButton(
            Transform prefabRoot,
            Button startButton)
        {
            Transform exactStart = FindUniqueDescendant(
                prefabRoot,
                "StartButton",
                "Stage-select truthful detail panel");
            CanvasGroup canvasGroup = exactStart.GetComponent<CanvasGroup>();
            Transform frame = FindUniqueDescendant(
                exactStart,
                "Frame",
                "Stage-select Start button");
            Graphic frameGraphic = frame.GetComponent<Graphic>();
            Transform labelObject = FindUniqueDescendant(
                exactStart,
                "StageStartText",
                "Stage-select Start button");
            Text label = labelObject.GetComponent<Text>();
            Require(
                startButton != null
                    && ReferenceEquals(startButton.transform, exactStart)
                    && exactStart.gameObject.activeSelf
                    && startButton.enabled
                    && startButton.interactable
                    && startButton.targetGraphic != null
                    && startButton.targetGraphic.raycastTarget
                    && canvasGroup != null
                    && canvasGroup.alpha >= 0.99f
                    && canvasGroup.interactable
                    && canvasGroup.blocksRaycasts,
                "Stage-select presenter must bind one active, interactable, raycastable Start button.");
            Require(
                frame.gameObject.activeSelf
                    && frameGraphic != null
                    && frameGraphic.color.a > 0.01f
                    && label != null
                    && label.gameObject.activeSelf
                    && string.Equals(label.text, "작전 시작", StringComparison.Ordinal)
                    && label.resizeTextForBestFit
                    && label.resizeTextMinSize == 20
                    && label.resizeTextMaxSize == 30
                    && label.fontStyle == FontStyle.Bold
                    && label.alignment == TextAnchor.MiddleCenter
                    && !label.raycastTarget,
                "Stage-select Start button must contain one visible frame and exact readable '작전 시작' label.");
            RectTransform labelRect = label.rectTransform;
            Require(
                Mathf.Approximately(labelRect.anchorMin.x, 0.1f)
                    && Mathf.Approximately(labelRect.anchorMin.y, 0.15f)
                    && Mathf.Approximately(labelRect.anchorMax.x, 0.9f)
                    && Mathf.Approximately(labelRect.anchorMax.y, 0.85f)
                    && labelRect.anchoredPosition == Vector2.zero
                    && labelRect.sizeDelta == Vector2.zero,
                "Stage-select Start label must retain its full-width bounded layout.");
        }

        private static void ValidateTruthfulStageSelectChapterInventory(Transform prefabRoot)
        {
            Transform selected = FindUniqueDescendant(
                prefabRoot,
                "EP 01_SelectedChapterCard",
                "Stage-select chapter inventory");
            Button selectedButton = selected.GetComponent<Button>();
            Text episode = FindUniqueDescendant(
                    selected,
                    "EpisodeText",
                    "Stage-select selected chapter")
                .GetComponent<Text>();
            Text title = FindUniqueDescendant(
                    selected,
                    "TitleText",
                    "Stage-select selected chapter")
                .GetComponent<Text>();
            Text percent = FindUniqueDescendant(
                    selected,
                    "PercentText",
                    "Stage-select selected chapter")
                .GetComponent<Text>();
            Require(
                selected.gameObject.activeSelf
                    && selectedButton != null
                    && selectedButton.enabled
                    && !selectedButton.interactable
                    && selectedButton.targetGraphic != null
                    && !selectedButton.targetGraphic.raycastTarget
                    && episode != null
                    && episode.gameObject.activeSelf
                    && string.Equals(episode.text, "EP 01", StringComparison.Ordinal)
                    && title != null
                    && title.gameObject.activeSelf
                    && string.Equals(title.text, "차원 안정화", StringComparison.Ordinal)
                    && percent != null
                    && !percent.gameObject.activeSelf
                    && string.IsNullOrEmpty(percent.text),
                "Stage-select must expose only one truthful EP 01 / 차원 안정화 chapter card without false progress.");

            string[] placeholders =
            {
                "EP 02_ChapterCard",
                "EP 03_ChapterCard",
                "EP 04_ChapterCard"
            };
            for (int i = 0; i < placeholders.Length; i++)
            {
                Transform placeholder = FindUniqueDescendant(
                    prefabRoot,
                    placeholders[i],
                    "Stage-select chapter inventory");
                Button button = placeholder.GetComponent<Button>();
                CanvasGroup canvasGroup = placeholder.GetComponent<CanvasGroup>();
                Require(
                    !placeholder.gameObject.activeSelf
                        && button != null
                        && !button.interactable
                        && (button.targetGraphic == null
                            || !button.targetGraphic.raycastTarget)
                        && (canvasGroup == null
                            || (!canvasGroup.interactable
                                && !canvasGroup.blocksRaycasts)),
                    $"Unadmitted chapter placeholder '{placeholders[i]}' must be inactive and reject interaction.");
            }
        }

        private static void ValidateStageSelectRouteInteractableGate(
            GameObject prefab,
            SerializedObject serializedPresenter,
            UIStageCatalog catalog)
        {
            UIRouteInteractableGate[] gates =
                prefab.GetComponentsInChildren<UIRouteInteractableGate>(true);
            Require(
                gates.Length == 1,
                $"Stage-select prefab must contain exactly one route interactable gate, but found {gates.Length}.");

            var expected = new HashSet<Selectable>();
            Button backButton =
                serializedPresenter.FindProperty("backButton")?.objectReferenceValue as Button;
            Button startButton =
                serializedPresenter.FindProperty("startButton")?.objectReferenceValue as Button;
            Require(
                backButton != null
                    && startButton != null
                    && expected.Add(backButton)
                    && expected.Add(startButton),
                "Stage-select route gate contract requires unique Back and Start buttons.");

            SerializedProperty focusEntries = serializedPresenter.FindProperty("stageFocusEntries");
            Require(
                focusEntries != null
                    && focusEntries.isArray
                    && focusEntries.arraySize == catalog.StageCount,
                "Stage-select route gate contract requires one focus entry per catalog row.");
            for (int i = 0; i < focusEntries.arraySize; i++)
            {
                Selectable selectionButton = focusEntries.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("selectionButton")
                    .objectReferenceValue as Selectable;
                Require(
                    selectionButton != null && expected.Add(selectionButton),
                    $"Stage-select route gate contract has a missing or duplicate stage button at row {i}.");
            }

            SerializedObject serializedGate = new(gates[0]);
            SerializedProperty selectables = serializedGate.FindProperty("selectables");
            Require(
                selectables != null
                    && selectables.isArray
                    && selectables.arraySize == expected.Count,
                "Stage-select route gate must bind exactly Back, Start, and every admitted stage button.");
            for (int i = 0; i < selectables.arraySize; i++)
            {
                Selectable selectable =
                    selectables.GetArrayElementAtIndex(i).objectReferenceValue as Selectable;
                Require(
                    selectable != null && expected.Remove(selectable),
                    "Stage-select route gate contains a missing, duplicate, or non-product control.");
            }

            Require(
                expected.Count == 0,
                "Stage-select route gate is missing an admitted product control.");
        }

        private static void ValidateStageSelectCardBindings(
            GameObject prefab,
            SerializedObject serializedPresenter,
            UIStageCatalog catalog)
        {
            SerializedProperty exactBindings =
                serializedPresenter.FindProperty("requireExactStageCardBindings");
            Require(
                exactBindings != null && exactBindings.boolValue,
                "Stage-select prefab must require exact stage-card bindings.");

            SerializedProperty focusEntries = serializedPresenter.FindProperty("stageFocusEntries");
            Require(
                focusEntries != null
                    && focusEntries.isArray
                    && focusEntries.arraySize == catalog.StageCount,
                "Stage-select focus entries must map one-to-one to every catalog entry.");

            var boundStageIds = new HashSet<string>(StringComparer.Ordinal);
            var boundButtons = new HashSet<Button>();
            var boundTargets = new HashSet<RectTransform>();
            for (int focusIndex = 0; focusIndex < focusEntries.arraySize; focusIndex++)
            {
                SerializedProperty focusEntry = focusEntries.GetArrayElementAtIndex(focusIndex);
                string stageId = focusEntry.FindPropertyRelative("stageId")?.stringValue;
                UIStageCatalog.StageEntry catalogEntry = catalog.GetStage(focusIndex);
                Button selectionButton =
                    focusEntry.FindPropertyRelative("selectionButton")?.objectReferenceValue as Button;
                RectTransform stageTarget =
                    focusEntry.FindPropertyRelative("stageTarget")?.objectReferenceValue as RectTransform;

                RequireEqual(
                    stageId,
                    catalogEntry.Id,
                    $"Stage-select focus entry {focusIndex} catalog ID");
                Require(
                    !string.IsNullOrWhiteSpace(stageId)
                        && boundStageIds.Add(stageId)
                        && catalog.TryGetStage(stageId, out _),
                    $"Stage-select focus entry {focusIndex} must reference one unique catalog ID.");
                Require(
                    selectionButton != null
                        && stageTarget != null
                        && ReferenceEquals(selectionButton.transform, stageTarget)
                        && boundButtons.Add(selectionButton)
                        && boundTargets.Add(stageTarget),
                    $"Stage-select focus entry {focusIndex} must own one unique Button on its exact stage target.");
                Require(
                    IsStageCardShellName(stageTarget.name),
                    $"Stage-select focus entry {focusIndex} target '{stageTarget.name}' is not a ??-?_StageCard shell.");
                RequireEqual(
                    stageTarget.name,
                    $"01-{focusIndex + 1}_StageCard",
                    $"Stage-select focus entry {focusIndex} shell name");
                Require(
                    stageTarget.gameObject.activeSelf
                        && selectionButton.enabled
                        && selectionButton.interactable,
                    $"Bound stage-card shell '{stageTarget.name}' must be active and interactable.");
                CanvasGroup boundCanvasGroup = stageTarget.GetComponent<CanvasGroup>();
                Require(
                    boundCanvasGroup == null
                        || (boundCanvasGroup.interactable && boundCanvasGroup.blocksRaycasts),
                    $"Bound stage-card shell '{stageTarget.name}' must accept canvas interaction and raycasts.");
                Require(
                    selectionButton.onClick.GetPersistentEventCount() == 0,
                    $"Bound stage-card shell '{stageTarget.name}' must not serialize persistent onClick listeners.");
                ValidateBoundStageCardPresentation(stageTarget, catalogEntry, focusIndex);
            }

            for (int catalogIndex = 0; catalogIndex < catalog.StageCount; catalogIndex++)
            {
                string catalogEntryId = catalog.GetStage(catalogIndex).Id;
                Require(
                    boundStageIds.Contains(catalogEntryId),
                    $"Stage-select prefab is missing an exact card binding for catalog entry '{catalogEntryId}'.");
            }

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            int stageCardShellCount = 0;
            int boundShellCount = 0;
            for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
            {
                Transform shell = transforms[transformIndex];
                if (!IsStageCardShellName(shell.name))
                {
                    continue;
                }

                stageCardShellCount++;
                Button shellButton = shell.GetComponent<Button>();
                Require(
                    shellButton != null,
                    $"Stage-card shell '{shell.name}' must own a Button.");
                Require(
                    shellButton.onClick.GetPersistentEventCount() == 0,
                    $"Stage-card shell '{shell.name}' must not serialize persistent onClick listeners.");

                CanvasGroup shellCanvasGroup = shell.GetComponent<CanvasGroup>();
                bool bound = boundButtons.Contains(shellButton);
                if (bound)
                {
                    boundShellCount++;
                    Require(
                        boundTargets.Contains(shell as RectTransform)
                            && shell.gameObject.activeSelf
                            && shellButton.enabled
                            && shellButton.interactable,
                        $"Bound stage-card shell '{shell.name}' must be the exact active, interactable target.");
                    Require(
                        shellCanvasGroup == null
                            || (shellCanvasGroup.interactable && shellCanvasGroup.blocksRaycasts),
                        $"Bound stage-card shell '{shell.name}' must accept canvas interaction and raycasts.");
                    continue;
                }

                Require(
                    !shell.gameObject.activeSelf && !shellButton.interactable,
                    $"Unbound stage-card shell '{shell.name}' must be inactive and non-interactable.");
                Require(
                    shellCanvasGroup == null
                        || (!shellCanvasGroup.interactable && !shellCanvasGroup.blocksRaycasts),
                    $"Unbound stage-card shell '{shell.name}' must reject canvas interaction and raycasts.");
            }

            Require(
                stageCardShellCount >= catalog.StageCount
                    && boundShellCount == boundButtons.Count,
                "Every exact stage binding must resolve to one authored ??-?_StageCard shell.");
        }

        private static void ValidateBoundStageCardPresentation(
            RectTransform stageTarget,
            UIStageCatalog.StageEntry catalogEntry,
            int catalogIndex)
        {
            Text stageNumberText = FindUniqueDescendant(
                    stageTarget,
                    "StageNumberText",
                    $"Stage-select card {catalogIndex}")
                .GetComponent<Text>();
            Text stageTitleText = FindUniqueDescendant(
                    stageTarget,
                    "StageTitleText",
                    $"Stage-select card {catalogIndex}")
                .GetComponent<Text>();
            Require(
                stageNumberText != null && stageNumberText.gameObject.activeSelf,
                $"Bound stage-card shell '{stageTarget.name}' must expose one active StageNumberText.");
            Require(
                stageTitleText != null && stageTitleText.gameObject.activeSelf,
                $"Bound stage-card shell '{stageTarget.name}' must expose one active StageTitleText.");
            Require(
                stageNumberText.enabled
                    && stageTitleText.enabled
                    && stageNumberText.color.a > 0.01f
                    && stageTitleText.color.a > 0.01f
                    && stageNumberText.resizeTextForBestFit
                    && stageTitleText.resizeTextForBestFit
                    && stageNumberText.resizeTextMinSize == 16
                    && stageNumberText.resizeTextMaxSize == 21
                    && stageTitleText.resizeTextMinSize == 12
                    && stageTitleText.resizeTextMaxSize == 22
                    && stageNumberText.fontStyle == FontStyle.Bold
                    && stageTitleText.fontStyle == FontStyle.Bold
                    && stageNumberText.alignment == TextAnchor.MiddleLeft
                    && stageTitleText.alignment == TextAnchor.MiddleLeft
                    && !stageNumberText.raycastTarget
                    && !stageTitleText.raycastTarget
                    && stageNumberText.GetComponent<Outline>() != null
                    && stageTitleText.GetComponent<Outline>() != null,
                $"Bound stage-card shell '{stageTarget.name}' must expose readable outlined number and title labels.");
            RequireEqual(
                stageNumberText.text,
                $"01-{catalogIndex + 1}",
                $"Bound stage-card shell '{stageTarget.name}' number text");
            RequireEqual(
                stageTitleText.text,
                catalogEntry.DisplayName,
                $"Bound stage-card shell '{stageTarget.name}' title text");

            string[] inactivePresentationObjects =
            {
                "StagePercentText",
                "Star1",
                "Star2",
                "Star3"
            };
            for (int objectIndex = 0;
                 objectIndex < inactivePresentationObjects.Length;
                 objectIndex++)
            {
                string objectName = inactivePresentationObjects[objectIndex];
                Transform presentationObject = FindUniqueDescendant(
                    stageTarget,
                    objectName,
                    $"Stage-select card {catalogIndex}");
                Text presentationText = presentationObject.GetComponent<Text>();
                Require(
                    !presentationObject.gameObject.activeSelf
                        && (!string.Equals(
                                objectName,
                                "StagePercentText",
                                StringComparison.Ordinal)
                            || (presentationText != null
                                && string.IsNullOrEmpty(presentationText.text))),
                    $"Bound stage-card shell '{stageTarget.name}' must keep '{objectName}' inactive until verified progression data exists.");
            }

            Transform lockIcon = FindOptionalUniqueDescendant(
                stageTarget,
                "LockIcon",
                $"Stage-select card {catalogIndex}");
            Require(
                lockIcon == null || !lockIcon.gameObject.activeSelf,
                $"Bound stage-card shell '{stageTarget.name}' must not present a lock without an authoritative eligibility source.");
        }

        private static Transform FindUniqueDescendant(
            Transform root,
            string objectName,
            string ownerLabel)
        {
            Transform match = FindOptionalUniqueDescendant(root, objectName, ownerLabel);
            Require(
                match != null,
                $"{ownerLabel} must contain exactly one descendant named '{objectName}', actual=0.");
            return match;
        }

        private static Transform FindOptionalUniqueDescendant(
            Transform root,
            string objectName,
            string ownerLabel)
        {
            Transform match = null;
            int matchCount = 0;
            Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
            for (int descendantIndex = 0;
                 descendantIndex < descendants.Length;
                 descendantIndex++)
            {
                Transform candidate = descendants[descendantIndex];
                if (!string.Equals(candidate.name, objectName, StringComparison.Ordinal))
                {
                    continue;
                }

                match = candidate;
                matchCount++;
            }

            Require(
                matchCount <= 1,
                $"{ownerLabel} must contain at most one descendant named '{objectName}', actual={matchCount}.");
            return match;
        }

        private static bool IsStageCardShellName(string objectName)
        {
            const string suffix = "_StageCard";
            return !string.IsNullOrEmpty(objectName)
                && objectName.Length == 4 + suffix.Length
                && objectName[2] == '-'
                && objectName.EndsWith(suffix, StringComparison.Ordinal);
        }

        private static void ValidateAction(
            PlayableStageDefinition route,
            string actionId,
            StageRouteActionKind actionKind,
            string targetPlayableStageId,
            StageUiRouteId targetUiRouteId,
            StageRouteOutcome allowedOutcomes)
        {
            Require(route.TryGetTerminalAction(actionId, out StageRouteActionRef action), $"Missing action {actionId}.");
            Require(action.ActionKind == actionKind, $"{actionId}.actionKind is invalid.");
            RequireEqual(action.TargetPlayableStageId, targetPlayableStageId, $"{actionId}.targetPlayableStageId");
            Require(action.TargetUiRouteId == targetUiRouteId, $"{actionId}.targetUiRouteId is invalid.");
            Require(action.AllowedOutcomes == allowedOutcomes, $"{actionId}.allowedOutcomes is invalid.");
        }

        private static void ValidateTerminalPolicy(StageTerminalResolutionPolicy policy)
        {
            Require(policy != null, "Terminal resolution policy is missing.");
            RequireEqual(
                policy.TerminalResolutionPolicyId,
                "olympus-invasion.same-terminal-epoch",
                "terminalResolutionPolicyId");
            Require(policy.SemanticRevision == 1, "Terminal policy semantic revision must be 1.");
            Require(policy.WindowKind == StageTerminalWindowKind.SameTerminalResolutionEpoch, "windowKind is invalid.");
            Require(
                policy.BatchOwnerKind == StageTerminalBatchOwnerKind.EncounterTerminalResolutionCoordinator,
                "batchOwnerKind is invalid.");
            Require(
                policy.RootAdmissionKind == StageTerminalRootAdmissionKind.CanonicalCombatRootAdmission,
                "rootAdmissionKind is invalid.");
            Require(policy.RootOrderKind == StageTerminalRootOrderKind.RootAdmissionSequence, "rootOrderKind is invalid.");
            Require(
                policy.RootIssuePoint == StageTerminalRootIssuePoint.BeforeTerminalStateMutationAndCallbacks,
                "rootIssuePoint is invalid.");
            Require(
                policy.BatchBoundaryKind == StageTerminalBatchBoundaryKind.RootResolutionToken,
                "batchBoundaryKind is invalid.");
            Require(
                policy.TerminalSubjectRoles == (StageTerminalSubjectRole.Player | StageTerminalSubjectRole.Boss),
                "terminalSubjectRoles must be exactly Player | Boss.");
            Require(
                policy.CoveragePolicy
                    == StageTerminalCoveragePolicy.ExclusiveQueuedTerminalStateMutationForBoundSubjects,
                "coveragePolicy is invalid.");
            Require(
                policy.WorkExecutionKind == StageTerminalWorkExecutionKind.SynchronousNonYieldingResolution,
                "workExecutionKind is invalid.");
            Require(
                policy.NestedRequestPolicy == StageTerminalNestedRequestPolicy.SameRootSameEpoch,
                "nestedRequestPolicy is invalid.");
            Require(
                policy.IndependentRequestPolicy
                    == StageTerminalIndependentRequestPolicy.LowerAdmissionSequenceThenNextEpoch,
                "independentRequestPolicy is invalid.");
            Require(
                policy.EpochStampKind == StageTerminalEpochStampKind.EncounterTerminalEpoch,
                "epochStampKind is invalid.");
            Require(
                policy.CoordinatorLifecycleKind
                    == StageTerminalCoordinatorLifecycleKind.IdleOpenDrainingFinalizingEpochClosedTerminalClosedFaultedCancelled,
                "coordinatorLifecycleKind is invalid.");
            Require(
                policy.SubjectFinalizationKind == StageTerminalSubjectFinalizationKind.SynchronousTwoSubjectSnapshot,
                "subjectFinalizationKind is invalid.");
            Require(
                policy.TokenStatePolicy
                    == StageTerminalTokenStatePolicy.ExplicitIdleActiveDeferredClosedWrongRunPostTerminal,
                "tokenStatePolicy is invalid.");
            Require(
                policy.FlushBarrier == StageTerminalFlushBarrierKind.QueueDrainedAndSubjectsFinalized,
                "flushBarrier is invalid.");
            Require(
                policy.SimultaneousOutcome == StageTerminalSimultaneousOutcome.Clear,
                "simultaneousOutcome is invalid.");
            Require(policy.RequiresBossCandidateAndFinalDead, "Boss candidate/final match must be required.");
            Require(policy.RequiresPlayerCandidateAndFinalDown, "Player candidate/final match must be required.");
        }

        private static void ValidateStageDefinitions(
            StageDefinitionProfile corridor,
            StageDefinitionProfile station)
        {
            RequireEqual(corridor.StageId, CorridorStageId, "Corridor stageId");
            RequireEqual(corridor.ChapterId, "OLYMPUS-INVASION", "Corridor chapterId");
            RequireEqual(corridor.MapScenePath, CorridorScenePath, "Corridor mapScenePath");
            Require(string.IsNullOrEmpty(corridor.PreviousStageId), "Corridor previousStageId must be absent.");
            Require(string.IsNullOrEmpty(corridor.NextStageId), "Corridor nextStageId must be absent; the route owns succession.");
            for (int i = 0; i < corridor.SpawnCount; i++)
            {
                StageSpawnKind kind = corridor.GetSpawn(i).SpawnKind;
                Require(
                    kind != StageSpawnKind.Boss && kind != StageSpawnKind.Add && kind != StageSpawnKind.Rift,
                    "Corridor P1-0 definition must not claim Station boss/add/rift spawns.");
            }

            for (int i = 0; i < corridor.RuntimeStateCount; i++)
            {
                Require(
                    corridor.GetRuntimeState(i).StateKind != StageRuntimeStateKind.StageClear,
                    "Corridor physical definition must not own StageClear.");
            }

            RequireEqual(station.StageId, StationStageId, "Station stageId");
            RequireEqual(station.ChapterId, "OLYMPUS-INVASION", "Station chapterId");
            RequireEqual(station.PreviousStageId, CorridorStageId, "Station previousStageId");
            Require(string.IsNullOrEmpty(station.NextStageId), "Station final segment must not declare a successor.");
            RequireEqual(station.MapScenePath, CorridorScenePath, "Station shared-host mapScenePath");
            RequireEqual(
                station.MapScenePath,
                corridor.MapScenePath,
                "Corridor/Station shared-host mapScenePath");
            ValidateStationAddRows(station);
            Require(
                station.SpawnCount == 2 && station.AnchorCount == 2,
                "A2 Station fixture must contain exactly two ordered Add rows and two anchors.");
            CombatEnemyArchetypeProfile reviewedMeleeArchetype =
                LoadRequiredAsset<CombatEnemyArchetypeProfile>(StationMeleeAddArchetypePath);
            CombatEnemyArchetypeProfile reviewedRangedArchetype =
                LoadRequiredAsset<CombatEnemyArchetypeProfile>(StationRangedAddArchetypePath);
            ValidateExactStationAddFixtureRow(
                station.GetSpawn(0),
                station.GetAnchor(0),
                StationAddSpawnId,
                StationAddAnchorId,
                StationAddPositionId,
                new Vector3(8.9f, 0f, -1.25f),
                StationMeleeAddPayloadId,
                reviewedMeleeArchetype,
                "left");
            ValidateExactStationAddFixtureRow(
                station.GetSpawn(1),
                station.GetAnchor(1),
                StationRightAddSpawnId,
                StationRightAddAnchorId,
                StationRightAddPositionId,
                new Vector3(8.9f, 0f, 1.25f),
                StationRangedAddPayloadId,
                reviewedRangedArchetype,
                "right");

            GameObject reviewedPrefab = LoadRequiredAsset<GameObject>(StationMeleeAddPrefabPath);
            CombatAiPatternProfile reviewedPattern =
                LoadRequiredAsset<CombatAiPatternProfile>(StationMeleeAddPatternPath);
            Require(
                ReferenceEquals(reviewedMeleeArchetype.GameplayPrefab, reviewedPrefab),
                "A1 Station Add archetype must retain the reviewed HeavyWindup gameplay prefab.");
            BasicSoldierEnemy reviewedAgent = reviewedPrefab.GetComponent<BasicSoldierEnemy>();
            CombatTargetSensor reviewedSensor = reviewedPrefab.GetComponent<CombatTargetSensor>();
            Require(
                reviewedAgent != null
                && reviewedSensor != null
                && ReferenceEquals(reviewedAgent.PatternProfile, reviewedPattern)
                && reviewedAgent.PatternDeck == null
                && reviewedPattern.AttackShape == CombatAiAttackShape.MeleeArc
                && reviewedPattern.AttackRange <= reviewedSensor.SearchRadius
                && reviewedPrefab.GetComponentsInChildren<BasicSoldierProjectileAttackDriver>(true).Length == 0,
                "A1 Station Add fixture drifted from the reviewed HeavyWindup melee contract.");

            GameObject rangedPrefab = LoadRequiredAsset<GameObject>(StationRangedAddPrefabPath);
            CombatAiPatternProfile rangedPattern =
                LoadRequiredAsset<CombatAiPatternProfile>(StationRangedAddPatternPath);
            CombatAiPatternDeck rangedDeck =
                LoadRequiredAsset<CombatAiPatternDeck>(StationRangedAddDeckPath);
            LaneActionProjectile rangedProjectile =
                LoadRequiredAsset<GameObject>(StationRangedProjectilePrefabPath)
                    .GetComponent<LaneActionProjectile>();
            BasicSoldierEnemy rangedAgent = rangedPrefab.GetComponent<BasicSoldierEnemy>();
            CombatTargetSensor rangedSensor = rangedPrefab.GetComponent<CombatTargetSensor>();
            BasicSoldierProjectileAttackDriver[] rangedDrivers =
                rangedPrefab.GetComponentsInChildren<BasicSoldierProjectileAttackDriver>(true);
            Require(
                ReferenceEquals(reviewedRangedArchetype.GameplayPrefab, rangedPrefab)
                && rangedAgent != null
                && rangedSensor != null
                && ReferenceEquals(rangedAgent.PatternProfile, rangedPattern)
                && ReferenceEquals(rangedAgent.PatternDeck, rangedDeck)
                && rangedDeck.EntryCount == 1
                && ReferenceEquals(rangedDeck.GetEntry(0).Profile, rangedPattern)
                && rangedPattern.AttackShape == CombatAiAttackShape.ProjectileLine
                && string.Equals(rangedPattern.PatternId, "RifleCrossfire", StringComparison.Ordinal)
                && rangedPattern.AttackRange <= rangedSensor.SearchRadius
                && rangedDrivers.Length == 1
                && rangedDrivers[0].IsConfiguredFor(rangedAgent, rangedAgent.SelfHealth, rangedSensor)
                && ReferenceEquals(rangedDrivers[0].ProjectilePrefab, rangedProjectile)
                && rangedDrivers[0].MaxOwnedProjectileCount == 3,
                "A2 Station right Add drifted from the reviewed RifleCrossfire projectile contract.");
            Require(station.RuntimeStateCount == 1, "Station definition must contain one terminal runtime state.");
            StageDefinitionProfile.RuntimeStateRef terminal = station.GetRuntimeState(0);
            Require(terminal.StateKind == StageRuntimeStateKind.StageClear, "Station terminal runtime state kind is invalid.");
            RequireEqual(terminal.ConditionId, StationTerminalConditionId, "Station terminal conditionId");
        }

        private static void ValidateExactStationAddFixtureRow(
            StageDefinitionProfile.SpawnRef spawn,
            StageDefinitionProfile.AnchorRef anchor,
            string expectedSpawnId,
            string expectedAnchorId,
            int expectedPositionId,
            Vector3 expectedPosition,
            string expectedPayloadId,
            CombatEnemyArchetypeProfile expectedArchetype,
            string laneName)
        {
            RequireEqual(spawn.SpawnId, expectedSpawnId, $"Station {laneName} Add spawnId");
            Require(spawn.SpawnKind == StageSpawnKind.Add, $"Station {laneName} row must be an Add.");
            Require(spawn.PositionId == expectedPositionId, $"Station {laneName} Add positionId is invalid.");
            RequireEqual(spawn.AnchorId, expectedAnchorId, $"Station {laneName} Add spawn anchorId");
            RequireEqual(spawn.PayloadId, expectedPayloadId, $"Station {laneName} Add payloadId");
            Require(
                ReferenceEquals(spawn.PayloadArchetype, expectedArchetype),
                $"Station {laneName} Add must directly reference its reviewed archetype.");
            Require(
                spawn.AuthoredCount == 1 && spawn.AuthoredDelaySeconds == 0f,
                $"Station {laneName} Add fixture must retain raw count one and zero delay.");
            RequireEqual(anchor.AnchorId, expectedAnchorId, $"Station {laneName} Add anchorId");
            RequireEqual(anchor.GroupId, StationAddAnchorGroupId, $"Station {laneName} Add anchor groupId");
            Require(
                Approximately(anchor.ExpectedPosition, expectedPosition),
                $"Station {laneName} Add anchor position is invalid.");
            Require(
                ApproximatelyEuler(anchor.ExpectedEuler, Vector3.zero),
                $"Station {laneName} Add anchor rotation is invalid.");
        }

        private static void ValidateStationAddRows(StageDefinitionProfile station)
        {
            var spawnIds = new HashSet<string>(StringComparer.Ordinal);
            var anchorIds = new HashSet<string>(StringComparer.Ordinal);
            var positionIds = new HashSet<int>();
            var referencedAnchorIds = new HashSet<string>(StringComparer.Ordinal);
            var serializedStation = new SerializedObject(station);
            SerializedProperty serializedSpawns = serializedStation.FindProperty("spawns");
            Require(serializedSpawns != null, "Station definition has no serialized spawn array.");

            int addCount = 0;
            float priorDelay = -1f;
            for (int sourceOrdinal = 0; sourceOrdinal < station.SpawnCount; sourceOrdinal++)
            {
                StageDefinitionProfile.SpawnRef spawn = station.GetSpawn(sourceOrdinal);
                if (spawn.SpawnKind != StageSpawnKind.Add)
                {
                    continue;
                }

                addCount++;
                SerializedProperty serializedSpawn =
                    serializedSpawns.GetArrayElementAtIndex(sourceOrdinal);
                int rawCount = serializedSpawn.FindPropertyRelative("count").intValue;
                float rawDelay = serializedSpawn.FindPropertyRelative("delaySeconds").floatValue;
                Require(
                    !string.IsNullOrWhiteSpace(spawn.SpawnId)
                    && spawnIds.Add(spawn.SpawnId),
                    $"Station Add row {sourceOrdinal} has an empty or duplicate spawnId.");
                Require(
                    !string.IsNullOrWhiteSpace(spawn.AnchorId)
                    && anchorIds.Add(spawn.AnchorId),
                    $"Station Add row '{spawn.SpawnId}' has an empty or duplicate anchorId.");
                Require(
                    spawn.PositionId > 0 && positionIds.Add(spawn.PositionId),
                    $"Station Add row '{spawn.SpawnId}' has a non-positive or duplicate positionId.");
                Require(
                    rawCount == 1 && spawn.AuthoredCount == rawCount,
                    $"Station Add row '{spawn.SpawnId}' must author exactly count 1 before runtime clamping.");
                Require(
                    float.IsFinite(rawDelay)
                    && rawDelay >= 0f
                    && Mathf.Approximately(spawn.AuthoredDelaySeconds, rawDelay),
                    $"Station Add row '{spawn.SpawnId}' has an invalid raw delay.");
                Require(
                    rawDelay >= priorDelay,
                    "Station Add delays must be nondecreasing in serialized source order.");
                priorDelay = rawDelay;

                int definitionAnchorCount = 0;
                StageDefinitionProfile.AnchorRef definitionAnchor = default;
                for (int anchorIndex = 0; anchorIndex < station.AnchorCount; anchorIndex++)
                {
                    StageDefinitionProfile.AnchorRef candidate = station.GetAnchor(anchorIndex);
                    if (string.Equals(candidate.AnchorId, spawn.AnchorId, StringComparison.Ordinal))
                    {
                        definitionAnchor = candidate;
                        definitionAnchorCount++;
                    }
                }

                Require(
                    definitionAnchorCount == 1,
                    $"Station Add row '{spawn.SpawnId}' must resolve exactly one definition anchor.");
                Require(
                    !string.IsNullOrWhiteSpace(definitionAnchor.GroupId),
                    $"Station Add anchor '{spawn.AnchorId}' has no groupId.");
                Require(
                    IsFinite(definitionAnchor.ExpectedPosition)
                    && IsFinite(definitionAnchor.ExpectedEuler),
                    $"Station Add anchor '{spawn.AnchorId}' has a non-finite authored pose.");
                referencedAnchorIds.Add(spawn.AnchorId);

                CombatEnemyArchetypeProfile archetype = spawn.PayloadArchetype;
                Require(archetype != null, $"Station Add row '{spawn.SpawnId}' has no direct archetype reference.");
                RequireEqual(
                    spawn.PayloadId,
                    archetype.ArchetypeId,
                    $"Station Add row '{spawn.SpawnId}' payload/archetype identity");
                Require(
                    !archetype.RequiresDedicatedPrefabPromotion,
                    $"Station Add row '{spawn.SpawnId}' archetype still requires prefab promotion.");
                GameObject prefab = archetype.GameplayPrefab;
                Require(prefab != null, $"Station Add row '{spawn.SpawnId}' archetype has no gameplay prefab.");
                Require(
                    AssetDatabase.GetAssetPath(prefab).StartsWith("Assets/_Game/", StringComparison.Ordinal),
                    $"Station Add row '{spawn.SpawnId}' must use a game-owned gameplay prefab.");

                CombatHealth[] health = prefab.GetComponentsInChildren<CombatHealth>(true);
                Require(
                    health.Length == 1
                    && health[0].Team == DamageTeam.Enemy
                    && health[0].enabled
                    && health[0].gameObject.activeSelf,
                    $"Station Add row '{spawn.SpawnId}' prefab must own exactly one enabled, active-self Enemy health.");
                Require(
                    prefab.GetComponentsInChildren<SummonFrontlineProxy>(true).Length == 0,
                    $"Station Add row '{spawn.SpawnId}' prefab must not carry a summon-frontline proxy.");

                ICombatAiAgent agent = null;
                int agentCount = 0;
                MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    if (behaviours[behaviourIndex] is ICombatAiAgent candidate)
                    {
                        agent = candidate;
                        agentCount++;
                    }
                }

                CombatTargetSensor[] sensors =
                    prefab.GetComponentsInChildren<CombatTargetSensor>(true);
                BasicSoldierProjectileAttackDriver[] projectileDrivers =
                    prefab.GetComponentsInChildren<BasicSoldierProjectileAttackDriver>(true);
                MonoBehaviour agentBehaviour = agent as MonoBehaviour;
                Require(
                    agentCount == 1
                    && agent != null
                    && agentBehaviour != null
                    && agentBehaviour.enabled
                    && agentBehaviour.gameObject.activeSelf
                    && ReferenceEquals(agent.SelfHealth, health[0])
                    && agent.PatternProfile != null
                    && agent.TargetSensor != null
                    && agent.TargetSensor.enabled
                    && agent.TargetSensor.gameObject.activeSelf
                    && sensors.Length == 1
                    && ReferenceEquals(sensors[0], agent.TargetSensor)
                    && ReferenceEquals(agent.TargetSensor.SelfHealth, health[0])
                    && agent.PatternProfile.AttackRange <= agent.TargetSensor.SearchRadius
                    && agent.TargetSensor.TargetCandidateCount == 0,
                    $"Station Add row '{spawn.SpawnId}' prefab violates the A0 agent/sensor participation contract.");

                if (agent.PatternProfile.AttackShape == CombatAiAttackShape.ProjectileLine)
                {
                    Require(
                        agent is BasicSoldierEnemy projectileSoldier
                        && projectileDrivers.Length == 1
                        && projectileDrivers[0].enabled
                        && projectileDrivers[0].gameObject.activeSelf
                        && projectileDrivers[0].IsConfiguredFor(
                            projectileSoldier,
                            health[0],
                            agent.TargetSensor),
                        $"Station Add row '{spawn.SpawnId}' ProjectileLine prefab requires one coherent bounded projectile driver.");
                }
                else
                {
                    Require(
                        projectileDrivers.Length == 0,
                        $"Station Add row '{spawn.SpawnId}' non-projectile prefab must not carry a projectile driver.");
                }
            }

            Require(addCount == 2, "A2 Station definition must author exactly two count-one Add rows.");
            Require(
                station.AnchorCount == referencedAnchorIds.Count,
                "Station definition anchors must map one-to-one to its current ordered Add rows.");
        }

        private static void ValidateBuildSettingsRoute()
        {
            UIV1BuildSettingsReadinessReporter.ValidateCurrentReadinessOrThrow();
        }

        private static int FindEnabledSceneIndex(EditorBuildSettingsScene[] scenes, string path)
        {
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && string.Equals(scenes[i].path, path, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void ValidateSceneContracts(
            PlayableStageDefinition route,
            StageDefinitionProfile corridor,
            StageDefinitionProfile station,
            CinematicSequenceProfile introProfile,
            PlayableAsset introPlayable)
        {
            UIScreenRouteTable uiRouteTable = LoadRequiredAsset<UIScreenRouteTable>(UiRouteTablePath);
            StageResultPresentationCatalog presentationCatalog =
                LoadRequiredAsset<StageResultPresentationCatalog>(StageResultPresentationCatalogPath);
            Require(
                presentationCatalog.TryValidate(out string presentationCatalogError),
                $"Stage-result presentation catalog is invalid: {presentationCatalogError}");
            Require(
                presentationCatalog.ProfileCount == 1
                    && string.Equals(
                        presentationCatalog.GetProfile(0).PlayableStageId,
                        route.PlayableStageId,
                        StringComparison.Ordinal),
                "Stage-result presentation catalog must contain the canonical Olympus stage profile once.");

            WithLoadedScene(
                CorridorScenePath,
                scene =>
                {
                    StageDefinitionSceneBinding corridorBinding = ValidateSceneBinding(scene, corridor);
                    StageDefinitionSceneBinding stationBinding = ValidateSceneBinding(scene, station);
                    StageDefinitionSceneBinding[] bindings =
                        CollectSceneComponents<StageDefinitionSceneBinding>(scene);
                    Require(
                        bindings.Length == 2,
                        "Shared Olympus host must contain exactly the Corridor and Station scene bindings.");
                    Require(
                        ReferenceEquals(corridorBinding.MapRoot, stationBinding.MapRoot),
                        "Corridor and Station scene bindings must share the exact physical map root.");
                    OlympusCorridorCombatFlowController[] flows =
                        CollectSceneComponents<OlympusCorridorCombatFlowController>(scene);
                    Require(flows.Length == 1, "Corridor must contain exactly one canonical flow controller.");
                    ValidateIntroPresentationSceneContract(
                        scene,
                        corridorBinding,
                        route.GetSceneSegment(0).EntryPresentation,
                        introProfile,
                        introPlayable,
                        flows[0]);
                    SerializedObject serializedFlow = new(flows[0]);
                    Require(
                        ReferenceEquals(
                            serializedFlow.FindProperty("playableStageDefinition")?.objectReferenceValue,
                            route),
                        "Corridor flow must directly reference the canonical PlayableStageDefinition.");
                    ValidateStationAddSceneAuthoring(scene, stationBinding);
                    CombatEncounterController[] controllers = CollectSceneComponents<CombatEncounterController>(scene);
                    Require(
                        controllers.Length == 1,
                        "Shared Olympus host must contain exactly one CombatEncounterController.");
                    CombatEncounterController controller = controllers[0];
                    Require(
                        controller.UsesCoordinatedTerminalResolution,
                        "Station CombatEncounterController must use coordinated terminal resolution.");

                    SerializedObject serializedController = new(controller);
                    Require(
                        serializedController.FindProperty("playerHealth")?.objectReferenceValue != null,
                        "Station coordinated encounter must bind playerHealth.");
                    Require(
                        serializedController.FindProperty("enemyHealth")?.objectReferenceValue != null,
                        "Station coordinated encounter must bind enemyHealth.");

                    OlympusStationRunFactCollector[] collectors =
                        CollectSceneComponents<OlympusStationRunFactCollector>(scene);
                    Require(collectors.Length == 1, "Station must contain exactly one run fact collector.");
                    SerializedObject serializedCollector = new(collectors[0]);
                    Require(
                        ReferenceEquals(
                            serializedCollector.FindProperty("encounter")?.objectReferenceValue,
                            controller),
                        "Station run fact collector must reference the canonical encounter.");
                    string[] requiredCollectorReferences =
                    {
                        "playerHealth",
                        "playerActionController",
                        "summonEnergyLadder",
                        "summonSlot1Action",
                        "bossEncounter",
                        "resultSurfaceBehaviour"
                    };
                    for (int i = 0; i < requiredCollectorReferences.Length; i++)
                    {
                        string propertyName = requiredCollectorReferences[i];
                        Require(
                            serializedCollector.FindProperty(propertyName)?.objectReferenceValue != null,
                            $"Station run fact collector must bind {propertyName}.");
                    }

                    SerializedProperty supportSources =
                        serializedCollector.FindProperty("supportSummonActions");
                    Require(
                        supportSources != null && supportSources.arraySize == 2,
                        "Station run fact collector must bind the two authored support summon sources.");
                    for (int i = 0; i < supportSources.arraySize; i++)
                    {
                        Require(
                            supportSources.GetArrayElementAtIndex(i).objectReferenceValue != null,
                            "Station run fact collector contains a missing support summon source.");
                    }

                    OlympusStationCombatResultPresenter[] resultPresenters =
                        CollectSceneComponents<OlympusStationCombatResultPresenter>(scene);
                    Require(
                        resultPresenters.Length == 1,
                        "Station must contain exactly one canonical combat result presenter.");
                    SerializedObject serializedResultPresenter = new(resultPresenters[0]);
                    Require(
                        ReferenceEquals(
                            serializedResultPresenter.FindProperty("factCollector")?.objectReferenceValue,
                            collectors[0]),
                        "Station result presenter must directly reference the run fact collector.");

                    int entryGuideCount = 0;
                    MonoBehaviour[] stationBehaviours = CollectSceneComponents<MonoBehaviour>(scene);
                    for (int i = 0; i < stationBehaviours.Length; i++)
                    {
                        if (stationBehaviours[i] is ICombatEntryGuideGate)
                        {
                            entryGuideCount++;
                        }
                    }

                    Require(
                        entryGuideCount == 1,
                        "Station must contain exactly one explicit combat-entry guide lifecycle gate.");
                    ValidateNoLegacyTerminalProducerComponents(scene);
                });

            WithLoadedScene(
                StageClearScenePath,
                scene =>
                {
                    StageClearScreenPresenter[] presenters =
                        CollectSceneComponents<StageClearScreenPresenter>(scene);
                    Require(presenters.Length == 1, "UI_StageClear must contain exactly one result presenter.");

                    StageRunUiRouteResolver[] resolvers =
                        CollectSceneComponents<StageRunUiRouteResolver>(scene);
                    Require(resolvers.Length == 1, "UI_StageClear must contain exactly one canonical UI route resolver.");

                    SerializedObject serializedPresenter = new(presenters[0]);
                    Require(
                        ReferenceEquals(
                            serializedPresenter.FindProperty("uiRouteResolverBehaviour")?.objectReferenceValue,
                            resolvers[0]),
                        "Stage-clear presenter must directly reference the canonical UI route resolver.");
                    Require(
                        ReferenceEquals(
                            serializedPresenter.FindProperty("presentationCatalog")?.objectReferenceValue,
                            presentationCatalog),
                        "Stage-clear presenter must directly reference the validated presentation catalog.");
                    string localeId = serializedPresenter.FindProperty("localeId")?.stringValue;
                    Require(
                        presentationCatalog.LocalizationTable.TryResolveLocale(
                            localeId,
                            out string resolvedLocaleId,
                            out string localeError)
                            && string.Equals(localeId, resolvedLocaleId, StringComparison.Ordinal),
                        $"Stage-clear presenter locale is not explicitly authored: {localeError}");

                    string[] requiredTextFields =
                    {
                        "primaryActionText",
                        "lobbyActionText",
                        "stageNameText",
                        "stageNumberText",
                        "totalActiveTimeLabelText",
                        "totalActiveTimeValueText",
                        "combatActiveTimeLabelText",
                        "combatActiveTimeValueText",
                        "recordsCategoryText"
                    };
                    for (int fieldIndex = 0; fieldIndex < requiredTextFields.Length; fieldIndex++)
                    {
                        Require(
                            serializedPresenter.FindProperty(requiredTextFields[fieldIndex])?.objectReferenceValue
                                != null,
                            $"Stage-clear presenter is missing {requiredTextFields[fieldIndex]}.");
                    }

                    SerializedProperty proofRows = serializedPresenter.FindProperty("proofRowTexts");
                    Require(
                        proofRows != null && proofRows.arraySize == 3,
                        "Stage-clear presenter must bind exactly three authored proof rows.");
                    for (int proofIndex = 0; proofIndex < proofRows.arraySize; proofIndex++)
                    {
                        Require(
                            proofRows.GetArrayElementAtIndex(proofIndex).objectReferenceValue != null,
                            $"Stage-clear presenter proof row {proofIndex} is unbound.");
                    }

                    SerializedObject serializedResolver = new(resolvers[0]);
                    Require(
                        ReferenceEquals(
                            serializedResolver.FindProperty("routeTable")?.objectReferenceValue,
                            uiRouteTable),
                        "Stage-clear UI route resolver must reference DB_UIRouteTable.");
                });

            string sharedHostYaml = File.ReadAllText(ToAbsoluteProjectPath(CorridorScenePath));
            Require(
                !sharedHostYaml.Contains("m_MethodName: ResetHealthToFull", StringComparison.Ordinal)
                    && !sharedHostYaml.Contains("m_MethodName: ConfigureMaxHealth", StringComparison.Ordinal),
                "Shared-host UnityEvents must not bypass coordinated terminal ownership through health reset/configuration.");

            string stageClearYaml = File.ReadAllText(ToAbsoluteProjectPath(StageClearScenePath));
            Require(
                !stageClearYaml.Contains("retrySceneName:", StringComparison.Ordinal)
                    && !stageClearYaml.Contains("retryScenePath:", StringComparison.Ordinal)
                    && !stageClearYaml.Contains("lobbySceneName:", StringComparison.Ordinal)
                    && !stageClearYaml.Contains("lobbyScenePath:", StringComparison.Ordinal),
                "UI_StageClear must not serialize legacy direct-navigation scene strings.");

            string presenterSource = File.ReadAllText(ToAbsoluteProjectPath(StageClearPresenterPath));
            Require(
                !presenterSource.Contains("LoadSingleScene(", StringComparison.Ordinal)
                    && !presenterSource.Contains("SceneManager.LoadScene", StringComparison.Ordinal)
                    && !presenterSource.Contains("EditorSceneManager.LoadSceneInPlayMode", StringComparison.Ordinal),
                "Stage-clear presenter must delegate all navigation to the P1-A route/run owner.");
            Require(
                !presenterSource.Contains("OLYMPUS INVASION / CLEAR", StringComparison.Ordinal)
                    && !presenterSource.Contains("OLYMPUS INVASION / FAILED", StringComparison.Ordinal)
                    && !presenterSource.Contains("clear ? \"REPLAY\" : \"RETRY\"", StringComparison.Ordinal),
                "Stage-clear player-facing copy must come from the validated localization profile.");
        }

        private static void ValidateStationAddSceneAuthoring(
            Scene scene,
            StageDefinitionSceneBinding binding)
        {
            Require(binding != null, "Station Add authoring requires the exact Station scene binding.");
            Require(binding.MapRoot != null, "Station Add authoring requires a binding MapRoot.");
            StageDefinitionProfile definition = binding.StageDefinition;
            Require(definition != null, "Station Add authoring requires its exact stage definition.");
            StageAnchorPoint[] authoredAnchors = CollectSceneComponents<StageAnchorPoint>(scene);
            Require(
                authoredAnchors.Length > 0,
                "Shared Olympus host must contain authored StageAnchorPoints.");
            int addRowCount = 0;
            for (int sourceOrdinal = 0; sourceOrdinal < definition.SpawnCount; sourceOrdinal++)
            {
                StageDefinitionProfile.SpawnRef spawn = definition.GetSpawn(sourceOrdinal);
                if (spawn.SpawnKind != StageSpawnKind.Add)
                {
                    continue;
                }

                addRowCount++;
                Require(
                    binding.TryGetAnchorPoint(spawn.AnchorId, out StageAnchorPoint addAnchor),
                    $"Station scene binding is missing Add anchor '{spawn.AnchorId}'.");
                int matchingAnchorCount = 0;
                for (int anchorIndex = 0; anchorIndex < authoredAnchors.Length; anchorIndex++)
                {
                    StageAnchorPoint candidate = authoredAnchors[anchorIndex];
                    if (candidate != null
                        && string.Equals(candidate.AnchorId, spawn.AnchorId, StringComparison.Ordinal))
                    {
                        matchingAnchorCount++;
                        Require(
                            ReferenceEquals(candidate, addAnchor),
                            $"Station Add anchor '{spawn.AnchorId}' must resolve only to the exact binding row.");
                    }
                }

                StageDefinitionProfile.AnchorRef expectedAnchor = default;
                int expectedAnchorCount = 0;
                for (int anchorIndex = 0; anchorIndex < definition.AnchorCount; anchorIndex++)
                {
                    StageDefinitionProfile.AnchorRef candidate = definition.GetAnchor(anchorIndex);
                    if (string.Equals(candidate.AnchorId, spawn.AnchorId, StringComparison.Ordinal))
                    {
                        expectedAnchor = candidate;
                        expectedAnchorCount++;
                    }
                }

                Require(
                    matchingAnchorCount == 1 && expectedAnchorCount == 1,
                    $"Shared Olympus host must contain exactly one Add anchor '{spawn.AnchorId}'.");
                RequireEqual(addAnchor.name, spawn.AnchorId, "Station Add anchor GameObject name");
                RequireEqual(
                    addAnchor.GroupId,
                    expectedAnchor.GroupId,
                    $"Station Add '{spawn.SpawnId}' live anchor groupId");
                Require(
                    addAnchor.UsageKind == StageAnchorUsageKind.CombatSpawn,
                    $"Station Add '{spawn.SpawnId}' live anchor must use CombatSpawn semantics.");
                Require(
                    addAnchor.PositionId == spawn.PositionId,
                    $"Station Add '{spawn.SpawnId}' live anchor positionId is invalid.");
                Require(
                    addAnchor.SpawnKind == StageSpawnKind.Add,
                    $"Station Add '{spawn.SpawnId}' live anchor must use SpawnKind.Add.");
                Require(
                    addAnchor.transform.IsChildOf(binding.MapRoot),
                    $"Station Add '{spawn.SpawnId}' live anchor must descend from the binding MapRoot.");
                ResolveBindingRootLocalPose(
                    binding.transform,
                    addAnchor.transform,
                    out Vector3 bindingLocalPosition,
                    out Quaternion bindingLocalRotation);
                Require(
                    Approximately(bindingLocalPosition, expectedAnchor.ExpectedPosition),
                    $"Station Add '{spawn.SpawnId}' binding-root-local position is invalid.");
                Require(
                    ApproximatelyEuler(
                        bindingLocalRotation.eulerAngles,
                        expectedAnchor.ExpectedEuler),
                    $"Station Add '{spawn.SpawnId}' binding-root-local rotation is invalid.");
            }

            Require(addRowCount > 0, "Station scene requires at least one bound Add row.");
            Require(
                binding.AnchorPointCount == addRowCount,
                "Station scene binding must expose exactly one live anchor per ordered Add row.");

            StageCountOneEncounterExecutor[] executors =
                CollectSceneComponents<StageCountOneEncounterExecutor>(scene);
            Require(executors.Length == 1, "Station must contain exactly one ordered Add encounter executor.");
            StageCountOneEncounterExecutor executor = executors[0];
            Require(
                ReferenceEquals(executor.SceneBinding, binding),
                "Station ordered Add executor must consume the canonical scene binding.");
            Require(
                executor.ActivationKind == StageEncounterActivationKind.CombatEntryGuideReleased,
                "Station ordered Add executor must activate after the entry guide releases gameplay.");
            Require(
                executor.RequiresActiveStageRun,
                "Station ordered Add executor must require the exact active canonical run.");
            Require(
                executor.CancelsOnTerminalEncounter,
                "Station ordered Add executor must cancel unfinished tickets on the authoritative terminal outcome.");

            CombatEncounterController[] terminalEncounters =
                CollectSceneComponents<CombatEncounterController>(scene);
            Require(
                terminalEncounters.Length == 1,
                "Station Add participation requires exactly one terminal encounter owner.");
            CombatEncounterController terminalEncounter = terminalEncounters[0];
            Require(
                terminalEncounter.PlayerHealth != null
                && terminalEncounter.EnemyHealth != null
                && terminalEncounter.PlayerHealth.gameObject.scene == scene
                && terminalEncounter.EnemyHealth.gameObject.scene == scene
                && terminalEncounter.PlayerHealth.Team == DamageTeam.Player
                && terminalEncounter.EnemyHealth.Team == DamageTeam.Enemy
                && terminalEncounter.PlayerHealth.enabled
                && terminalEncounter.EnemyHealth.enabled,
                "Station terminal encounter must expose the exact scene-local, enabled Player/Enemy health pair.");
            PlayerCombatTargetSelector[] playerSelectors =
                terminalEncounter.PlayerHealth.GetComponents<PlayerCombatTargetSelector>();
            Require(
                playerSelectors.Length == 1
                && ReferenceEquals(playerSelectors[0].SelfHealth, terminalEncounter.PlayerHealth)
                && playerSelectors[0].enabled
                && playerSelectors[0].ContainsAuthoredTargetCandidate(terminalEncounter.EnemyHealth),
                "Station player selector must retain the authored boss while admitting runtime Add candidates separately.");
        }

        private static void ValidateIntroPresentationSceneContract(
            Scene scene,
            StageDefinitionSceneBinding binding,
            StagePresentationHandoffRef presentation,
            CinematicSequenceProfile introProfile,
            PlayableAsset introPlayable,
            OlympusCorridorCombatFlowController flow)
        {
            Require(binding != null, "Corridor presentation requires the exact Corridor scene binding.");
            Require(
                ReferenceEquals(binding.StageDefinition, presentation.StageDefinition),
                "Corridor presentation must use the binding owned by its exact stage definition.");

            int matchingHandoffCount = 0;
            StageCutscenePort introPort = null;
            for (int i = 0; i < binding.CutscenePortCount; i++)
            {
                StageCutscenePort port = binding.GetCutscenePort(i);
                if (port != null
                    && string.Equals(port.HandoffId, presentation.HandoffId, StringComparison.Ordinal))
                {
                    introPort = port;
                    matchingHandoffCount++;
                }
            }

            Require(matchingHandoffCount == 1, "Corridor scene must expose intro-to-stage on exactly one port.");
            RequireEqual(introPort.PortId, presentation.ExpectedPortId, "Corridor intro portId");
            RequireEqual(introPort.AnchorId, introProfile.StageAnchorId, "Corridor intro port anchorId");
            RequireEqual(
                introPort.RuntimeStateId,
                introProfile.StageRuntimeStateId,
                "Corridor intro port runtimeStateId");
            Require(
                ReferenceEquals(introPort.PresentationProfile, presentation.CinematicProfile),
                "Corridor intro port must directly reference the combined cinematic profile.");
            Require(introPort.RuntimeDirector != null, "Corridor intro port has no runtime PlayableDirector.");

            PlayableDirector[] directors = CollectSceneComponents<PlayableDirector>(scene);
            Require(directors.Length == 1, "Corridor must contain exactly one authored PlayableDirector.");
            Require(
                ReferenceEquals(introPort.RuntimeDirector, directors[0]),
                "Corridor intro port must bind the sole authored PlayableDirector.");
            Require(
                ReferenceEquals(directors[0].playableAsset, introPlayable)
                    && ReferenceEquals(directors[0].playableAsset, presentation.ExpectedPlayableAsset),
                "Corridor runtime PlayableDirector must consume the combined Timeline referenced by the spine.");

            ValidatePlayableDirectorBindings(directors[0], introPlayable);
            Require(
                binding.CutscenePortCount == 1,
                "Corridor scene binding must expose exactly the one presentation port owned by its definition.");

            SerializedObject serializedFlow = new(flow);
            Require(
                ReferenceEquals(
                    serializedFlow.FindProperty("introDirector")?.objectReferenceValue,
                    directors[0]),
                "Corridor flow must consume the same PlayableDirector bound by the intro port.");
        }

        private static void ValidatePlayableDirectorBindings(
            PlayableDirector director,
            PlayableAsset expectedPlayable)
        {
            Dictionary<UnityEngine.Object, UnityEngine.Object> runtimeBindings = new();
            List<string> unboundOutputs = new();
            foreach (PlayableBinding output in expectedPlayable.outputs)
            {
                UnityEngine.Object source = output.sourceObject;
                Require(
                    source != null,
                    $"Corridor Timeline output '{output.streamName}' has no source object.");
                Require(
                    !runtimeBindings.ContainsKey(source),
                    $"Corridor Timeline output source '{source.name}' is duplicated.");

                UnityEngine.Object target = director.GetGenericBinding(source);
                runtimeBindings.Add(source, target);
                if (target == null)
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out _, out long localId);
                    unboundOutputs.Add($"{source.name} [{localId}]");
                }
            }

            Require(runtimeBindings.Count > 0, "Corridor Timeline exposes no bindable outputs.");
            Require(
                unboundOutputs.Count == 0,
                "Corridor Timeline has unbound outputs: " + string.Join(", ", unboundOutputs));

            SerializedObject serializedDirector = new(director);
            SerializedProperty sceneBindings = serializedDirector.FindProperty("m_SceneBindings");
            Require(
                sceneBindings != null && sceneBindings.isArray,
                "Corridor PlayableDirector has no serialized scene-binding table.");
            Require(
                sceneBindings.arraySize == runtimeBindings.Count,
                $"Corridor PlayableDirector serializes {sceneBindings.arraySize} bindings for "
                    + $"{runtimeBindings.Count} current Timeline outputs.");

            HashSet<UnityEngine.Object> serializedSources = new();
            for (int i = 0; i < sceneBindings.arraySize; i++)
            {
                SerializedProperty row = sceneBindings.GetArrayElementAtIndex(i);
                UnityEngine.Object source = row.FindPropertyRelative("key")?.objectReferenceValue;
                UnityEngine.Object target = row.FindPropertyRelative("value")?.objectReferenceValue;

                Require(source != null, $"Corridor PlayableDirector binding row {i} has no source.");
                Require(target != null, $"Corridor PlayableDirector binding row {i} has no target.");
                RequireEqual(
                    AssetDatabase.GetAssetPath(source),
                    IntroPresentationPlayablePath,
                    $"Corridor PlayableDirector binding row {i} source asset");
                Require(
                    serializedSources.Add(source),
                    $"Corridor PlayableDirector serializes source '{source.name}' more than once.");
                Require(
                    runtimeBindings.TryGetValue(source, out UnityEngine.Object runtimeTarget),
                    $"Corridor PlayableDirector binding source '{source.name}' is not an output of the current Timeline.");
                Require(
                    ReferenceEquals(runtimeTarget, target),
                    $"Corridor PlayableDirector binding target for '{source.name}' disagrees with its runtime binding.");
            }
        }

        private static bool IsPresent(StagePresentationHandoffRef presentation)
        {
            return presentation != null && presentation.IsPresent;
        }

        private static StageDefinitionSceneBinding ValidateSceneBinding(
            Scene scene,
            StageDefinitionProfile expectedDefinition)
        {
            StageDefinitionSceneBinding[] bindings = CollectSceneComponents<StageDefinitionSceneBinding>(scene);
            int matchingBindingCount = 0;
            StageDefinitionSceneBinding binding = null;
            for (int i = 0; i < bindings.Length; i++)
            {
                StageDefinitionSceneBinding candidate = bindings[i];
                if (candidate != null && ReferenceEquals(candidate.StageDefinition, expectedDefinition))
                {
                    binding = candidate;
                    matchingBindingCount++;
                }
            }

            Require(
                matchingBindingCount == 1,
                $"{scene.path} must contain exactly one scene binding for {expectedDefinition.name}.");
            Require(binding.MapRoot != null, $"{scene.path} StageDefinitionSceneBinding has no map root.");
            RequireEqual(binding.MapRoot.name, expectedDefinition.MapContentRootName, $"{scene.path} map root name");
            Require(
                Approximately(binding.MapRoot.localScale, expectedDefinition.MapScale),
                $"{scene.path} map root scale disagrees with its StageDefinitionProfile.");

            Require(
                binding.AnchorPointCount == expectedDefinition.AnchorCount,
                $"{scene.path} scene binding exposes {binding.AnchorPointCount} anchors for "
                    + $"{expectedDefinition.AnchorCount} definition-owned anchors.");

            HashSet<StageAnchorPoint> sceneAnchors = new();
            HashSet<string> sceneAnchorIds = new(StringComparer.Ordinal);
            for (int i = 0; i < binding.AnchorPointCount; i++)
            {
                StageAnchorPoint sceneAnchor = binding.GetAnchorPoint(i);
                Require(sceneAnchor != null, $"{scene.path} scene anchor row {i} is null.");
                Require(
                    sceneAnchors.Add(sceneAnchor),
                    $"{scene.path} scene anchor '{sceneAnchor.name}' is bound more than once.");
                Require(
                    !string.IsNullOrWhiteSpace(sceneAnchor.AnchorId),
                    $"{scene.path} scene anchor '{sceneAnchor.name}' has no anchorId.");
                Require(
                    sceneAnchorIds.Add(sceneAnchor.AnchorId),
                    $"{scene.path} scene anchorId '{sceneAnchor.AnchorId}' is bound more than once.");

                int definitionMatchCount = 0;
                StageDefinitionProfile.AnchorRef definitionAnchor = default;
                for (int definitionIndex = 0;
                     definitionIndex < expectedDefinition.AnchorCount;
                     definitionIndex++)
                {
                    StageDefinitionProfile.AnchorRef candidate = expectedDefinition.GetAnchor(definitionIndex);
                    if (string.Equals(candidate.AnchorId, sceneAnchor.AnchorId, StringComparison.Ordinal))
                    {
                        definitionAnchor = candidate;
                        definitionMatchCount++;
                    }
                }

                Require(
                    definitionMatchCount == 1,
                    $"{scene.path} scene anchorId '{sceneAnchor.AnchorId}' must resolve exactly once in "
                        + $"{expectedDefinition.name}.");
                RequireEqual(
                    sceneAnchor.GroupId,
                    definitionAnchor.GroupId,
                    $"{scene.path} anchor '{sceneAnchor.AnchorId}' groupId");
                Transform bindingRoot = binding.transform;
                Require(
                    sceneAnchor.transform.IsChildOf(bindingRoot),
                    $"{scene.path} anchor '{sceneAnchor.AnchorId}' is outside its scene-binding hierarchy.");
                ResolveBindingRootLocalPose(
                    bindingRoot,
                    sceneAnchor.transform,
                    out Vector3 bindingLocalPosition,
                    out Quaternion bindingLocalRotation);
                Require(
                    Approximately(bindingLocalPosition, definitionAnchor.ExpectedPosition),
                    $"{scene.path} anchor '{sceneAnchor.AnchorId}' binding-root-local position disagrees with its definition.");
                Require(
                    ApproximatelyEuler(bindingLocalRotation.eulerAngles, definitionAnchor.ExpectedEuler),
                    $"{scene.path} anchor '{sceneAnchor.AnchorId}' binding-root-local rotation disagrees with its definition.");
            }

            return binding;
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

        private static void ValidateCinematicProfileStageContexts()
        {
            string[] profileGuids = AssetDatabase.FindAssets(
                "t:CinematicSequenceProfile",
                new[] { CinematicProfilesDirectory });
            List<string> profilePaths = new(profileGuids.Length);
            for (int i = 0; i < profileGuids.Length; i++)
            {
                profilePaths.Add(AssetDatabase.GUIDToAssetPath(profileGuids[i]));
            }

            profilePaths.Sort(StringComparer.Ordinal);
            for (int i = 0; i < profilePaths.Count; i++)
            {
                CinematicSequenceProfile profile =
                    LoadRequiredAsset<CinematicSequenceProfile>(profilePaths[i]);
                if (!profile.RequiresStageDefinition)
                {
                    continue;
                }

                StageDefinitionProfile definition = profile.StageDefinition;
                Require(
                    definition != null,
                    $"Cinematic profile {profile.name} requires a stage definition but has none.");

                int handoffMatches = 0;
                StageDefinitionProfile.CutsceneHandoffRef resolvedHandoff = default;
                for (int handoffIndex = 0; handoffIndex < definition.CutsceneHandoffCount; handoffIndex++)
                {
                    StageDefinitionProfile.CutsceneHandoffRef candidate =
                        definition.GetCutsceneHandoff(handoffIndex);
                    if (string.Equals(candidate.HandoffId, profile.StageHandoffId, StringComparison.Ordinal))
                    {
                        resolvedHandoff = candidate;
                        handoffMatches++;
                    }
                }

                Require(
                    handoffMatches == 1,
                    $"Cinematic profile {profile.name} stage handoff '{profile.StageHandoffId}' "
                        + $"must resolve exactly once in {definition.name}.");

                int anchorMatches = 0;
                for (int anchorIndex = 0; anchorIndex < definition.AnchorCount; anchorIndex++)
                {
                    if (string.Equals(
                        definition.GetAnchor(anchorIndex).AnchorId,
                        profile.StageAnchorId,
                        StringComparison.Ordinal))
                    {
                        anchorMatches++;
                    }
                }

                Require(
                    anchorMatches == 1,
                    $"Cinematic profile {profile.name} stage anchor '{profile.StageAnchorId}' "
                        + $"must resolve exactly once in {definition.name}.");
                RequireEqual(
                    resolvedHandoff.AnchorId,
                    profile.StageAnchorId,
                    $"Cinematic profile {profile.name} handoff anchorId");

                int runtimeStateMatches = 0;
                for (int stateIndex = 0; stateIndex < definition.RuntimeStateCount; stateIndex++)
                {
                    if (string.Equals(
                        definition.GetRuntimeState(stateIndex).StateId,
                        profile.StageRuntimeStateId,
                        StringComparison.Ordinal))
                    {
                        runtimeStateMatches++;
                    }
                }

                Require(
                    runtimeStateMatches == 1,
                    $"Cinematic profile {profile.name} stage runtime state '{profile.StageRuntimeStateId}' "
                        + $"must resolve exactly once in {definition.name}.");
            }
        }

        private static void ValidateNoLegacyTerminalProducerComponents(Scene scene)
        {
            HashSet<string> bannedTypeNames = new(StringComparer.Ordinal)
            {
                "BattleManager",
                "EnemyAI",
                "EnemyProjectile",
                "MobilePerformanceBenchmarkRunner"
            };
            MonoBehaviour[] behaviours = CollectSceneComponents<MonoBehaviour>(scene);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && bannedTypeNames.Contains(behaviour.GetType().Name))
                {
                    throw new InvalidOperationException(
                        $"Station contains banned legacy terminal producer {behaviour.GetType().FullName}.");
                }
            }
        }

        private static void ValidateTerminalMutationInventory()
        {
            string scriptsRoot = Path.Combine(Application.dataPath, "_Game", "Scripts");
            string combatHealthPath = "Assets/_Game/Scripts/Combat/CombatHealth.cs";
            string coordinatorPath = "Assets/_Game/Scripts/Combat/EncounterTerminalResolutionCoordinator.cs";
            Dictionary<string, int> damageProducers = new(StringComparer.Ordinal);
            Dictionary<string, int> resetCallers = new(StringComparer.Ordinal);
            Dictionary<string, int> configureMaxCallers = new(StringComparer.Ordinal);
            Dictionary<string, int> authorizedCoreCallers = new(StringComparer.Ordinal);

            string[] files = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string assetPath = ToAssetPath(files[i]);
                string source = File.ReadAllText(files[i]);
                if (!string.Equals(assetPath, combatHealthPath, StringComparison.Ordinal)
                    && !string.Equals(assetPath, coordinatorPath, StringComparison.Ordinal))
                {
                    AddOccurrences(damageProducers, assetPath, source, ".TryApplyDamage(");
                }

                if (!string.Equals(assetPath, combatHealthPath, StringComparison.Ordinal))
                {
                    AddOccurrences(resetCallers, assetPath, source, ".ResetHealthToFull(");
                    AddOccurrences(configureMaxCallers, assetPath, source, ".ConfigureMaxHealth(");
                }

                AddOccurrences(authorizedCoreCallers, assetPath, source, ".TryApplyDamageAuthorized(");
            }

            RequireExactInventory("damage producers", damageProducers, ExpectedDamageProducers);
            RequireExactInventory("reset callers", resetCallers, ExpectedResetCallers);
            RequireExactInventory("ConfigureMaxHealth callers", configureMaxCallers, ExpectedConfigureMaxCallers);
            RequireExactInventory(
                "authorized core callers",
                authorizedCoreCallers,
                new Dictionary<string, int>(StringComparer.Ordinal)
                {
                    [coordinatorPath] = 1
                });

            string combatHealthSource = File.ReadAllText(ToAbsoluteProjectPath(combatHealthPath));
            Require(
                combatHealthSource.Contains("authority.TryApplyDamage(this, damageInfo)", StringComparison.Ordinal),
                "CombatHealth.TryApplyDamage no longer delegates bound mutation to its authority.");
            Require(
                combatHealthSource.Contains("TryAuthorizeBoundMutation(BoundHealthMutationKind.ConfigureMaxHealth)", StringComparison.Ordinal)
                    && combatHealthSource.Contains("TryAuthorizeBoundMutation(BoundHealthMutationKind.ResetHealthToFull)", StringComparison.Ordinal),
                "CombatHealth reset/configuration guards are missing.");

            Debug.Log(
                "[PlayableStageDefinitionValidator] Terminal mutation inventory PASS: "
                + "damageProducers=8, resetCallers=4, configureMaxCallers=1, authorizedCoreCallers=1, bypass=0.");
        }

        private static void RequireExactInventory(
            string label,
            Dictionary<string, int> actual,
            Dictionary<string, int> expected)
        {
            Require(
                actual.Count == expected.Count,
                $"Unexpected {label} inventory. expected={FormatInventory(expected)}, actual={FormatInventory(actual)}");
            foreach (KeyValuePair<string, int> pair in expected)
            {
                Require(
                    actual.TryGetValue(pair.Key, out int actualCount) && actualCount == pair.Value,
                    $"Unexpected {label} inventory. expected={FormatInventory(expected)}, actual={FormatInventory(actual)}");
            }
        }

        private static void AddOccurrences(
            Dictionary<string, int> inventory,
            string assetPath,
            string source,
            string token)
        {
            int count = CountOccurrences(source, token);
            if (count > 0)
            {
                inventory[assetPath] = count;
            }
        }

        private static int CountOccurrences(string source, string token)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(token, offset, StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }

            return count;
        }

        private static string FormatInventory(Dictionary<string, int> inventory)
        {
            List<string> entries = new(inventory.Count);
            foreach (KeyValuePair<string, int> pair in inventory)
            {
                entries.Add($"{pair.Key}:{pair.Value}");
            }

            entries.Sort(StringComparer.Ordinal);
            return string.Join(",", entries);
        }

        private static void WithLoadedScene(string scenePath, Action<Scene> validator)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
            if (openedForValidation)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                validator(scene);
            }
            finally
            {
                if (openedForValidation && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static T[] CollectSceneComponents<T>(Scene scene)
            where T : Component
        {
            List<T> components = new();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                components.AddRange(roots[i].GetComponentsInChildren<T>(true));
            }

            return components.ToArray();
        }

        private static T LoadRequiredAsset<T>(string assetPath)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Require(asset != null, $"Missing required asset {assetPath}.");
            return asset;
        }

        private static string ToAssetPath(string absolutePath)
        {
            string normalized = absolutePath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            Require(
                normalized.StartsWith(dataPath + "/", StringComparison.OrdinalIgnoreCase),
                $"Path is outside Assets: {absolutePath}");
            return "Assets/" + normalized.Substring(dataPath.Length + 1);
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Cannot resolve project root.");
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return Mathf.Abs(left.x - right.x) <= 0.0001f
                && Mathf.Abs(left.y - right.y) <= 0.0001f
                && Mathf.Abs(left.z - right.z) <= 0.0001f;
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y)
                && float.IsFinite(value.z);
        }

        private static bool ApproximatelyEuler(Vector3 left, Vector3 right)
        {
            return Mathf.Abs(Mathf.DeltaAngle(left.x, right.x)) <= 0.001f
                && Mathf.Abs(Mathf.DeltaAngle(left.y, right.y)) <= 0.001f
                && Mathf.Abs(Mathf.DeltaAngle(left.z, right.z)) <= 0.001f;
        }

        private static void RequireEqual(string actual, string expected, string label)
        {
            Require(
                string.Equals(actual ?? string.Empty, expected ?? string.Empty, StringComparison.Ordinal),
                $"{label} mismatch. expected='{expected}', actual='{actual}'.");
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
