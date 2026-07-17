using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
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
        private const string StationAddArchetypePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/DB_Archetype_SciFiSoldier_Melee.asset";
        private const string StationAddPrefabPath =
            "Assets/_Game/Prefabs/Enemies/ActionFoundation/PF_Enemy_SciFiSoldier_Melee_ClosePunish.prefab";
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
        private const string StationAddAnchorGroupId = "CombatSpawnAnchors";
        private const string StationAddPayloadId = "SciFiSoldier.Melee";
        private const int StationAddPositionId = 2101;
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

            Require(
                AssetDatabase.FindAssets("t:StageResultDefinition").Length == 1,
                "Exactly one StageResultDefinition must be authored for the current project slice.");
            Require(
                AssetDatabase.FindAssets("t:StageProgressionNode").Length == 1,
                "Exactly one StageProgressionNode must be authored for the current project slice.");
            Require(
                AssetDatabase.FindAssets("t:StageProgressionGraph").Length == 1,
                "Exactly one StageProgressionGraph must be authored for the current project slice.");
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
                "Stage catalog projection generation must be 2 for the revision-2 route.");
            Require(catalog.StageCount == 1, "Stage catalog must expose exactly one canonical product entry.");
            UIStageCatalog.StageEntry entry = catalog.GetStage(0);
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
                    0,
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
                CanonicalCatalogEntryId,
                "Stage-select selectedStageId");
            Require(
                serializedPresenter.FindProperty("startRoute")?.intValue == (int)UIRouteId.Combat,
                "Stage-select prefab start route must be UIRouteId.Combat.");
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
            RectTransform lessonRect = combatLessonText.rectTransform;
            RectTransform rewardRect = rewardPreviewText.rectTransform;
            Require(
                lessonRect.anchorMin.y >= rewardRect.anchorMax.y,
                "Stage-select combat lesson and optional reward rows must not overlap.");
            SerializedProperty startRequested =
                serializedPresenter.FindProperty("startRequested")
                    ?.FindPropertyRelative("m_PersistentCalls")
                    ?.FindPropertyRelative("m_Calls");
            Require(
                startRequested != null && startRequested.arraySize == 0,
                "Stage-select startRequested must have no admission or navigation listener.");
            SerializedProperty focusEntries = serializedPresenter.FindProperty("stageFocusEntries");
            Require(
                focusEntries != null && focusEntries.isArray && focusEntries.arraySize == 1,
                "Stage-select presenter must expose exactly one canonical focus entry.");
            RequireEqual(
                focusEntries.GetArrayElementAtIndex(0).FindPropertyRelative("stageId")?.stringValue,
                CanonicalCatalogEntryId,
                "Stage-select focus entry ID");

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            int retiredCardCount = 0;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (string.Equals(transforms[i].name, "01-2_StageCard", StringComparison.Ordinal))
                {
                    retiredCardCount++;
                    Require(
                        !transforms[i].gameObject.activeSelf,
                        "The retired retry-route stage card must remain inactive.");
                }
            }

            Require(retiredCardCount == 1, "Stage-select prefab must retain one inactive retired card shell.");

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
            Require(station.AnchorCount == 1, "Station definition must contain exactly one P1-B Add anchor.");
            StageDefinitionProfile.AnchorRef addAnchor = station.GetAnchor(0);
            RequireEqual(addAnchor.AnchorId, StationAddAnchorId, "Station Add anchorId");
            RequireEqual(addAnchor.GroupId, StationAddAnchorGroupId, "Station Add anchor groupId");
            Require(
                Approximately(addAnchor.ExpectedPosition, new Vector3(8.9f, 0f, -1.25f)),
                "Station Add anchor position is invalid.");
            Require(
                ApproximatelyEuler(addAnchor.ExpectedEuler, Vector3.zero),
                "Station Add anchor rotation is invalid.");

            Require(station.SpawnCount == 1, "Station definition must contain exactly one P1-B Add spawn.");
            StageDefinitionProfile.SpawnRef addSpawn = station.GetSpawn(0);
            RequireEqual(addSpawn.SpawnId, StationAddSpawnId, "Station Add spawnId");
            Require(addSpawn.SpawnKind == StageSpawnKind.Add, "Station first fixture must use SpawnKind.Add.");
            Require(addSpawn.PositionId == StationAddPositionId, "Station Add positionId is invalid.");
            RequireEqual(addSpawn.AnchorId, StationAddAnchorId, "Station Add spawn anchorId");
            RequireEqual(addSpawn.PayloadId, StationAddPayloadId, "Station Add payloadId");
            Require(addSpawn.Count == 1, "Station first Add fixture must have count 1.");
            Require(
                float.IsFinite(addSpawn.DelaySeconds) && Mathf.Approximately(addSpawn.DelaySeconds, 0f),
                "Station first Add fixture must have a finite zero delay.");

            CombatEnemyArchetypeProfile addArchetype =
                LoadRequiredAsset<CombatEnemyArchetypeProfile>(StationAddArchetypePath);
            GameObject addPrefab = LoadRequiredAsset<GameObject>(StationAddPrefabPath);
            RequireEqual(addArchetype.ArchetypeId, StationAddPayloadId, "Station Add archetypeId");
            Require(
                ReferenceEquals(addArchetype.GameplayPrefab, addPrefab),
                "Station Add archetype must reference the exact reviewed gameplay prefab.");
            Require(
                !addArchetype.RequiresDedicatedPrefabPromotion,
                "Station Add archetype still requires dedicated prefab promotion.");
            Require(station.RuntimeStateCount == 1, "Station definition must contain one terminal runtime state.");
            StageDefinitionProfile.RuntimeStateRef terminal = station.GetRuntimeState(0);
            Require(terminal.StateKind == StageRuntimeStateKind.StageClear, "Station terminal runtime state kind is invalid.");
            RequireEqual(terminal.ConditionId, StationTerminalConditionId, "Station terminal conditionId");
        }

        private static void ValidateBuildSettingsRoute()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            int corridorIndex = FindEnabledSceneIndex(scenes, CorridorScenePath);
            int stageClearIndex = FindEnabledSceneIndex(scenes, StageClearScenePath);
            Require(corridorIndex >= 0, "Corridor scene is missing or disabled in Build Settings.");
            Require(
                stageClearIndex >= 0 && stageClearIndex != corridorIndex,
                "UI_StageClear must remain an enabled, separate presentation scene.");
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
            Require(
                binding.TryGetAnchorPoint(StationAddAnchorId, out StageAnchorPoint addAnchor),
                "Station scene binding is missing the canonical Add anchor.");

            StageAnchorPoint[] authoredAnchors = CollectSceneComponents<StageAnchorPoint>(scene);
            int matchingAddAnchorCount = 0;
            Require(
                authoredAnchors.Length > 0,
                "Shared Olympus host must contain authored StageAnchorPoints.");
            for (int i = 0; i < authoredAnchors.Length; i++)
            {
                StageAnchorPoint candidate = authoredAnchors[i];
                if (candidate != null
                    && string.Equals(candidate.AnchorId, StationAddAnchorId, StringComparison.Ordinal))
                {
                    matchingAddAnchorCount++;
                    Require(
                        ReferenceEquals(candidate, addAnchor),
                        "Station Add anchor ID must resolve only to the exact Station binding row.");
                }
            }

            Require(
                matchingAddAnchorCount == 1,
                "Shared Olympus host must contain exactly one canonical Station Add anchor ID.");
            RequireEqual(addAnchor.name, StationAddAnchorId, "Station Add anchor GameObject name");
            RequireEqual(addAnchor.GroupId, StationAddAnchorGroupId, "Station Add live anchor groupId");
            Require(
                addAnchor.UsageKind == StageAnchorUsageKind.CombatSpawn,
                "Station Add live anchor must use CombatSpawn semantics.");
            Require(addAnchor.PositionId == StationAddPositionId, "Station Add live anchor positionId is invalid.");
            Require(
                addAnchor.SpawnKind == StageSpawnKind.Add,
                "Station Add live anchor must use SpawnKind.Add.");
            Require(
                addAnchor.transform.IsChildOf(binding.MapRoot),
                "Station Add live anchor must be a descendant of the binding MapRoot.");
            ResolveBindingRootLocalPose(
                binding.transform,
                addAnchor.transform,
                out Vector3 bindingLocalPosition,
                out Quaternion bindingLocalRotation);
            Require(
                Approximately(bindingLocalPosition, new Vector3(8.9f, 0f, -1.25f)),
                "Station Add live anchor binding-root-local position is invalid.");
            Require(
                ApproximatelyEuler(bindingLocalRotation.eulerAngles, Vector3.zero),
                "Station Add live anchor binding-root-local rotation is invalid.");

            StageCountOneEncounterExecutor[] executors =
                CollectSceneComponents<StageCountOneEncounterExecutor>(scene);
            Require(executors.Length == 1, "Station must contain exactly one count-one encounter executor.");
            StageCountOneEncounterExecutor executor = executors[0];
            Require(
                ReferenceEquals(executor.SceneBinding, binding),
                "Station count-one executor must consume the canonical scene binding.");
            RequireEqual(executor.SpawnId, StationAddSpawnId, "Station count-one executor spawnId");
            Require(
                executor.ActivationKind == StageEncounterActivationKind.CombatEntryGuideReleased,
                "Station count-one executor must activate after the entry guide releases gameplay.");
            Require(
                executor.RequiresActiveStageRun,
                "Station count-one executor must require the exact active canonical run.");
            Require(
                executor.CancelsOnTerminalEncounter,
                "Station count-one executor must cancel its Add on the authoritative terminal outcome.");
            Require(executor.PayloadMappingCount == 1, "Station count-one executor requires one payload mapping.");
            CombatEnemyArchetypeProfile addArchetype =
                LoadRequiredAsset<CombatEnemyArchetypeProfile>(StationAddArchetypePath);
            Require(
                ReferenceEquals(executor.GetPayloadMapping(0), addArchetype),
                "Station count-one executor must reference the canonical melee soldier archetype.");
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
