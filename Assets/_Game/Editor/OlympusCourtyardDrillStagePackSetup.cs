using System;
using System.Collections.Generic;
using System.IO;
using DimensionBrawl.AI;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Authors the isolated B1-1 Courtyard Drill data pack. Product publication is owned
    /// by the separate B1-2 admission setup, so reauthoring this pack remains valid both
    /// before and after catalog admission. Existing Olympus assets are creation seeds only;
    /// every route-owned authority is persisted as a distinct asset and resealed.
    /// </summary>
    public static class OlympusCourtyardDrillStagePackSetup
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        public const string StageDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCourtyardDrillCombat.asset";
        public const string PlayableStagePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusCourtyardDrill.asset";
        public const string StageTemplatePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusCourtyardDrillRun.asset";
        public const string LocalizationPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultLocalization_OlympusCourtyardDrill.asset";
        public const string PresentationProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentation_OlympusCourtyardDrill.asset";
        public const string PresentationCatalogPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog_OlympusCourtyardDrill.asset";
        public const string ResultDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusCourtyardDrill.asset";
        public const string ProgressionNodePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusCourtyardDrill.asset";
        public const string ProgressionGraphPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusCourtyardDrill.asset";

        public const string PlayableStageId = "OLYMPUS-COURTYARD-DRILL-01";
        public const string StageDefinitionId = "OLYMPUS-COURTYARD-DRILL-COMBAT-01";
        public const string RouteSegmentId = "courtyard_drill_combat";
        public const string TerminalConditionId = "courtyard-drill.encounter.terminal";
        public const string TemplateId = "olympus-courtyard-drill.standard-run";
        public const string TemplateSegmentId = "olympus-courtyard-drill.combat";
        public const string TemplatePocketId = "olympus-courtyard-drill.terminal-combat";
        public const string TerminalPolicyId = "olympus-courtyard-drill.same-terminal-epoch";
        public const string ReplayActionId = "olympus-courtyard-drill.replay";
        public const string RetryActionId = "olympus-courtyard-drill.retry";
        public const string LobbyActionId = "olympus-courtyard-drill.to-lobby";
        public const string ResultDefinitionId = "olympus-courtyard-drill.result-definition";
        public const string PresentationProfileId = "stage-result.olympus-courtyard-drill";
        public const string PresentationCatalogId =
            "stage-result.presentation.catalog.olympus-courtyard-drill";
        public const string LocalizationTableId =
            "stage-result.localization.olympus-courtyard-drill";
        public const string ProgressionNodeId = "olympus-courtyard-drill.progression-node";
        public const string ProgressionGraphId = "olympus-courtyard-drill.progression-graph";

        public const string MapRootName = "OlympusCourtyardDrillStageRoot";
        public const string MapContentRootName = "OlympusCourtyardDrillMap";
        public const string LayoutId = "OLYMPUS_COURTYARD_DRILL_COMPACT_01";
        public const string PlayerAnchorId = "Player_Start";
        public const string BossAnchorId = "Boss_Terminal";
        public const string RangedAnchorId = "Add_RifleCrossfire";
        public const string PlayerSpawnId = "player-start";
        public const string BossSpawnId = "terminal-boss";
        public const string RangedSpawnId = "rifle-crossfire";
        public const int PlayerPositionId = 1101;
        public const int BossPositionId = 1201;
        public const int RangedPositionId = 1301;

        private const string SourceStageDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusCorridorIntroCombat.asset";
        private const string SourcePlayableStagePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";
        private const string SourceStageTemplatePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDesign/Templates/DB_StageTemplate_OlympusInvasionTutorialStationRun.asset";
        private const string SourceLocalizationPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultLocalization_Core.asset";
        private const string SourcePresentationProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentation_OlympusInvasion.asset";
        private const string SourcePresentationCatalogPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog.asset";
        private const string SourceResultDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultDefinition_OlympusInvasion.asset";
        private const string SourceProgressionNodePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionNode_OlympusInvasion.asset";
        private const string SourceProgressionGraphPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageProgressionGraph_OlympusInvasion.asset";
        private const string RangedArchetypePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/EnemyArchetypes/DB_Archetype_SciFiSoldier_Ranged.asset";
        private const string AcceptedOlympusPolicyDigest =
            "f18fc51e2b65ae7e11b7e26866adc29f1f994c95be3591f2806bb846cd0bcaf2";
        private const string AcceptedOlympusRouteDigest =
            "878dac821103cdca2d2ad29a3fab8bce27109e9a5c1d551b14eccb736fd252d0";
        private const string AcceptedOlympusTemplateDigest =
            "3eec8a5f94c4dfd47ae9255a49ff3b5961d5130cf386f2c6ba96b0525c502e55";
        private const string AcceptedOlympusReferenceDigest =
            "eada6124fe3bed295bddaf3caeb0b53ff1510a2f790c2b76b8454410834a21ea";
        private const string AcceptedOlympusBriefingDigest =
            "e334d6bb63dc42d921e6d85bcca42cc628064d0d1b8fee1cc303d4ca223fab70";
        private const string AcceptedOlympusResultEvaluationDigest =
            "ab16e4e051c053d57b7ce7a4c841fe42ee1a730ca0123f62684cf7c3decdc5da";
        private const string AcceptedOlympusResultBindingDigest =
            "095c545df089d7670daedb20b3603c180ca5c4ecf7a67c75a6a351d690dd4d0f";
        private const string AcceptedOlympusResultSourceDigest =
            "e94f5290000b043b5e96c496a67cac6a0df716c77fb36994a34f568ea829f5bc";
        private const string AcceptedOlympusNodeContentDigest =
            "87e684b5a7b0eac8fceaae168693d84132504bbca9a52bee0deb4187b28f9ac4";
        private const string AcceptedOlympusNodeBindingDigest =
            "cf1f0d21d34d1553b3aaadd6e8dbb8ace24b235245f8301b35959ee662a3dc37";
        private const string AcceptedOlympusGraphDigest =
            "7132faaa2607da7ad62380d2d3301ed2bda50cf81ad26e0dd6cdd08e46154221";
        private const string AcceptedOlympusJoinDigest =
            "d389c587a17c29cb8e1df60222442ff4339f32fa5435b3586e8f49aa43461d71";

        private const string TitleFormatKey = "stage_result.title.format";
        private const string StageNameKey = "stage_result.courtyard_drill.stage_name";
        private const string ClearStatusKey = "stage_result.status.clear";
        private const string FailStatusKey = "stage_result.status.fail";
        private const string TotalTimeKey = "stage_result.label.total_time";
        private const string CombatTimeKey = "stage_result.label.combat_time";
        private const string RecordsKey = "stage_result.label.records";
        private const string ReplayLabelKey = "stage_result.action.replay";
        private const string RetryLabelKey = "stage_result.action.retry";
        private const string LobbyLabelKey = "stage_result.action.lobby";
        private const string SurvivalProofTextKey =
            "stage_result.courtyard_drill.proof.survival";
        private const string ForwardRiskProofTextKey =
            "stage_result.courtyard_drill.proof.forward_risk_time";

        private static readonly string[] TargetPaths =
        {
            StageDefinitionPath,
            PlayableStagePath,
            StageTemplatePath,
            LocalizationPath,
            PresentationProfilePath,
            PresentationCatalogPath,
            ResultDefinitionPath,
            ProgressionNodePath,
            ProgressionGraphPath
        };

        private static readonly string[] SourcePaths =
        {
            SourceStageDefinitionPath,
            SourcePlayableStagePath,
            SourceStageTemplatePath,
            SourceLocalizationPath,
            SourcePresentationProfilePath,
            SourcePresentationCatalogPath,
            SourceResultDefinitionPath,
            SourceProgressionNodePath,
            SourceProgressionGraphPath
        };

        [MenuItem("DimensionBrawl/Setup/B1-1 Build Isolated Olympus Courtyard Drill Data Pack")]
        public static void BuildFromMenu()
        {
            PackAssets pack = BuildOrUpdate();
            ValidatePackOrThrow(pack);
            Debug.Log(FormatPassMessage(pack, "SETUP_PASS"));
        }

        [MenuItem("DimensionBrawl/Validate/B1-1 Isolated Olympus Courtyard Drill Data Pack")]
        public static void ValidateFromMenu()
        {
            PackAssets pack = LoadRequiredPack();
            ValidatePackOrThrow(pack);
            Debug.Log(FormatPassMessage(pack, "VALIDATION_PASS"));
        }

        public static void RunBatchSetup()
        {
            RunBatch(true);
        }

        public static void RunBatchValidation()
        {
            RunBatch(false);
        }

        private static void RunBatch(bool build)
        {
            try
            {
                PackAssets pack = build ? BuildOrUpdate() : LoadRequiredPack();
                ValidatePackOrThrow(pack);
                Debug.Log(FormatPassMessage(pack, build ? "BATCH_SETUP_PASS" : "BATCH_VALIDATION_PASS"));
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    build
                        ? "[OlympusCourtyardDrillStagePackSetup] BATCH_SETUP_FAIL"
                        : "[OlympusCourtyardDrillStagePackSetup] BATCH_VALIDATION_FAIL");
                EditorApplication.Exit(1);
            }
        }

        private static PackAssets BuildOrUpdate()
        {
            string sourceFingerprint = CaptureAcceptedOlympusSourceFingerprint();
            PackAssets pack = LoadOrCreatePack();
            ConfigureStageDefinition(pack.StageDefinition, pack.RangedArchetype);
            ConfigureTemplate(pack.Template);
            ConfigureRouteCore(pack.Route, pack.StageDefinition, pack.Template);
            ConfigureLocalization(pack.Localization);
            ConfigurePresentationProfile(pack.PresentationProfile);
            ConfigurePresentationCatalog(
                pack.PresentationCatalog,
                pack.Localization,
                pack.PresentationProfile);
            ConfigureResultDefinition(
                pack.ResultDefinition,
                pack.PresentationCatalog,
                pack.PresentationProfile,
                pack.Localization);
            ConfigureProgressionNode(pack.ProgressionNode, pack.Route);
            ConfigureProgressionGraph(pack.ProgressionGraph, pack.ProgressionNode);
            ConfigureAndSealJoin(pack);

            for (int i = 0; i < pack.PersistentAssets.Length; i++)
            {
                EditorUtility.SetDirty(pack.PersistentAssets[i]);
            }

            AssetDatabase.SaveAssets();
            ValidatePackOrThrow(pack);
            RequireEqual(
                CaptureAcceptedOlympusSourceFingerprint(),
                sourceFingerprint,
                "accepted Olympus source fingerprint after B1-1 setup");
            return pack;
        }

        private static PackAssets LoadOrCreatePack()
        {
            var pack = new PackAssets
            {
                StageDefinition = GetOrCreateClone<StageDefinitionProfile>(
                    StageDefinitionPath,
                    SourceStageDefinitionPath),
                Route = GetOrCreateClone<PlayableStageDefinition>(
                    PlayableStagePath,
                    SourcePlayableStagePath),
                Template = GetOrCreateClone<LinearStageTemplateProfile>(
                    StageTemplatePath,
                    SourceStageTemplatePath),
                Localization = GetOrCreateClone<StageResultLocalizationTable>(
                    LocalizationPath,
                    SourceLocalizationPath),
                PresentationProfile = GetOrCreateClone<StageResultPresentationProfile>(
                    PresentationProfilePath,
                    SourcePresentationProfilePath),
                PresentationCatalog = GetOrCreateClone<StageResultPresentationCatalog>(
                    PresentationCatalogPath,
                    SourcePresentationCatalogPath),
                ResultDefinition = GetOrCreateClone<StageResultDefinition>(
                    ResultDefinitionPath,
                    SourceResultDefinitionPath),
                ProgressionNode = GetOrCreateClone<StageProgressionNode>(
                    ProgressionNodePath,
                    SourceProgressionNodePath),
                ProgressionGraph = GetOrCreateClone<StageProgressionGraph>(
                    ProgressionGraphPath,
                    SourceProgressionGraphPath),
                RangedArchetype = LoadRequired<CombatEnemyArchetypeProfile>(RangedArchetypePath)
            };
            return pack;
        }

        private static PackAssets LoadRequiredPack()
        {
            return new PackAssets
            {
                StageDefinition = LoadRequired<StageDefinitionProfile>(StageDefinitionPath),
                Route = LoadRequired<PlayableStageDefinition>(PlayableStagePath),
                Template = LoadRequired<LinearStageTemplateProfile>(StageTemplatePath),
                Localization = LoadRequired<StageResultLocalizationTable>(LocalizationPath),
                PresentationProfile =
                    LoadRequired<StageResultPresentationProfile>(PresentationProfilePath),
                PresentationCatalog =
                    LoadRequired<StageResultPresentationCatalog>(PresentationCatalogPath),
                ResultDefinition = LoadRequired<StageResultDefinition>(ResultDefinitionPath),
                ProgressionNode = LoadRequired<StageProgressionNode>(ProgressionNodePath),
                ProgressionGraph = LoadRequired<StageProgressionGraph>(ProgressionGraphPath),
                RangedArchetype = LoadRequired<CombatEnemyArchetypeProfile>(RangedArchetypePath)
            };
        }

        private static void ConfigureStageDefinition(
            StageDefinitionProfile definition,
            CombatEnemyArchetypeProfile rangedArchetype)
        {
            var serialized = new SerializedObject(definition);
            SetString(serialized, "stageId", StageDefinitionId);
            SetString(serialized, "displayName", "Olympus Courtyard Drill");
            SetString(serialized, "chapterId", "OLYMPUS-INVASION");
            SetString(serialized, "previousStageId", string.Empty);
            SetString(serialized, "nextStageId", string.Empty);
            SetString(serialized, "mapScenePath", ScenePath);
            SetString(serialized, "mapRootName", MapRootName);
            SetString(serialized, "mapContentRootName", MapContentRootName);
            RequireProperty(serialized, "mapScale").vector3Value = Vector3.one;
            SetString(serialized, "layoutId", LayoutId);
            SetString(serialized, "scenePrefabSource", string.Empty);
            SetString(
                serialized,
                "objective",
                "Defeat the Courtyard terminal boss while handling the reviewed Rifle Crossfire pressure.");
            SetString(
                serialized,
                "clearCondition",
                "The authoritative Courtyard encounter resolves the bound boss terminal state through the coordinated terminal policy.");
            SetString(
                serialized,
                "excludedScope",
                "No cinematic, tutorial course, reward payout, persistent unlock, broad wave graph, or live-service economy is authored in B1-1.");

            SerializedProperty anchors = RequireProperty(serialized, "anchors");
            anchors.arraySize = 3;
            ConfigureAnchor(
                anchors.GetArrayElementAtIndex(0),
                PlayerAnchorId,
                new Vector3(0f, 0f, -4.5f),
                Vector3.zero,
                "Exact player entry anchor for the one-row Courtyard run.");
            ConfigureAnchor(
                anchors.GetArrayElementAtIndex(1),
                BossAnchorId,
                new Vector3(0f, 0f, 3.6f),
                new Vector3(0f, 180f, 0f),
                "Exact terminal-boss authoring anchor.");
            ConfigureAnchor(
                anchors.GetArrayElementAtIndex(2),
                RangedAnchorId,
                new Vector3(-5f, 0f, 1.5f),
                new Vector3(0f, 135f, 0f),
                "Exact reviewed Rifle Crossfire Add anchor.");

            SerializedProperty spawns = RequireProperty(serialized, "spawns");
            spawns.arraySize = 3;
            ConfigureSpawn(
                spawns.GetArrayElementAtIndex(0),
                PlayerSpawnId,
                StageSpawnKind.Player,
                PlayerPositionId,
                PlayerAnchorId,
                "PlayerParty",
                null,
                1,
                0f,
                "Scene-owned player entry identity; no balance or roster expansion.");
            ConfigureSpawn(
                spawns.GetArrayElementAtIndex(1),
                BossSpawnId,
                StageSpawnKind.Boss,
                BossPositionId,
                BossAnchorId,
                "CourtyardDrill.MiniBoss",
                null,
                1,
                0f,
                "Scene-owned actual boss terminal subject; ordinary Adds never impersonate this role.");
            ConfigureSpawn(
                spawns.GetArrayElementAtIndex(2),
                RangedSpawnId,
                StageSpawnKind.Add,
                RangedPositionId,
                RangedAnchorId,
                "SciFiSoldier.Ranged",
                rangedArchetype,
                1,
                0.6f,
                "One reviewed RifleCrossfire pressure participant activated by the neutral scene-ready executor.");

            RequireProperty(serialized, "cutsceneHandoffs").arraySize = 0;

            SerializedProperty states = RequireProperty(serialized, "runtimeStates");
            states.arraySize = 1;
            SerializedProperty terminalState = states.GetArrayElementAtIndex(0);
            SetRelativeString(terminalState, "stateId", "state-courtyard-encounter-terminal");
            SetRelativeInt(terminalState, "stateKind", (int)StageRuntimeStateKind.StageClear);
            SetRelativeInt(terminalState, "positionId", 9001);
            SetRelativeString(terminalState, "anchorId", string.Empty);
            SetRelativeString(terminalState, "conditionId", TerminalConditionId);
            SetRelativeString(
                terminalState,
                "note",
                "The coordinated terminal record closes the only logical segment; result UI remains presentation.");

            SerializedProperty sources = RequireProperty(serialized, "sourceReferences");
            sources.arraySize = 1;
            SerializedProperty source = sources.GetArrayElementAtIndex(0);
            SetRelativeString(source, "sourceId", "local-olympus-courtyard-drill");
            SetRelativeString(source, "sourcePath", ScenePath);
            SetRelativeString(
                source,
                "localTakeaway",
                "A compact local arena reuses promoted modular art and reviewed payloads; Ark data supports only the separated stage/map/spawn structure, not copied values.");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureTemplate(LinearStageTemplateProfile template)
        {
            var serialized = new SerializedObject(template);
            SetString(serialized, "stageTemplateId", TemplateId);
            SetString(serialized, "displayName", "Olympus Courtyard Drill");
            SetInt(serialized, "templateKind", (int)LinearStageTemplateKind.StandardStoryRun);
            SetInt(serialized, "templateSchemaVersion", 1);
            SetInt(serialized, "templateRevision", 1);
            SetString(serialized, "canonicalTemplateDigest", string.Empty);
            SetInt(serialized, "titleDisposition", (int)StageBriefingValueDisposition.Present);
            SetString(serialized, "title", "Olympus Courtyard Drill");
            SetInt(
                serialized,
                "titleLocalizationKeyDisposition",
                (int)StageBriefingValueDisposition.NoVerifiedSource);
            SetString(serialized, "titleLocalizationKey", string.Empty);
            SetInt(serialized, "objectiveDisposition", (int)StageBriefingValueDisposition.Present);
            SetString(
                serialized,
                "objective",
                "Defeat the Courtyard terminal boss under Rifle Crossfire pressure.");
            SetInt(
                serialized,
                "combatLessonDisposition",
                (int)StageBriefingValueDisposition.Present);
            SetString(
                serialized,
                "combatLesson",
                "Read the visible mid-range projectile lane while maintaining pressure on the terminal boss.");
            SetInt(
                serialized,
                "recommendedPowerDisposition",
                (int)StageBriefingValueDisposition.NoVerifiedSource);
            SetInt(serialized, "recommendedPowerTier", 0);
            SetInt(
                serialized,
                "recommendedLoadoutDisposition",
                (int)StageBriefingValueDisposition.NoVerifiedSource);
            SetString(serialized, "recommendedLoadout", string.Empty);
            SetInt(
                serialized,
                "targetRunDurationDisposition",
                (int)StageBriefingValueDisposition.NoVerifiedSource);
            SetInt(serialized, "targetRunDurationMilliseconds", 0);
            RequireProperty(serialized, "targetRunDurationSeconds").floatValue = 0f;
            SetInt(
                serialized,
                "featuredThreatDisposition",
                (int)StageBriefingValueDisposition.NoVerifiedSource);
            SetString(serialized, "featuredThreat", string.Empty);
            SetInt(
                serialized,
                "featuredSummonNeedDisposition",
                (int)StageBriefingValueDisposition.NoVerifiedSource);
            SetInt(serialized, "featuredSummonNeed", (int)StageSummonNeed.None);
            SetInt(
                serialized,
                "restrictionsDisposition",
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema);
            SetInt(serialized, "restrictionCount", 0);
            SetInt(
                serialized,
                "masteryPreviewDisposition",
                (int)StageBriefingValueDisposition.NotAuthoredForCurrentSchema);
            SetString(serialized, "masteryPreview", string.Empty);
            SetInt(
                serialized,
                "enemyPreviewDisposition",
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema);
            SetInt(serialized, "enemyPreviewCount", 0);
            SetInt(
                serialized,
                "rewardPreviewDisposition",
                (int)StageBriefingValueDisposition.NoVerifiedSource);
            SetString(serialized, "rewardPreview", string.Empty);
            SetInt(
                serialized,
                "courseSummaryDisposition",
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema);
            SetString(serialized, "courseSummary", string.Empty);
            SetString(serialized, "masteryObjective", string.Empty);
            SetString(serialized, "rewardHook", string.Empty);
            RequireProperty(serialized, "segments").arraySize = 0;
            SetString(
                serialized,
                "excludedScope",
                "One existing-scene boss terminal pocket only; no reward, unlock, tutorial, cinematic, branching encounter graph, or foreign gameplay value.");

            SerializedProperty routeSegments =
                RequireProperty(serialized, "canonicalRouteSegments");
            routeSegments.arraySize = 1;
            SerializedProperty segment = routeSegments.GetArrayElementAtIndex(0);
            SetRelativeString(segment, "templateSegmentId", TemplateSegmentId);
            SetRelativeString(segment, "routeSegmentId", RouteSegmentId);
            SetRelativeInt(segment, "routeSequenceIndex", 0);
            SerializedProperty pockets = RequireRelative(segment, "pockets");
            pockets.arraySize = 1;
            SerializedProperty pocket = pockets.GetArrayElementAtIndex(0);
            SetRelativeString(pocket, "pocketId", TemplatePocketId);
            SetRelativeInt(pocket, "sequenceIndex", 0);
            SetRelativeInt(
                pocket,
                "objectiveKind",
                (int)StageTemplatePocketObjectiveKind.DefeatBoss);
            SetRelativeInt(
                pocket,
                "currentExecutionOwnerDisposition",
                (int)StageTemplateCurrentExecutionOwnerDisposition.ExistingSceneOwner);
            SetRelativeInt(
                pocket,
                "p1cAdmissionDisposition",
                (int)StageTemplateP1CAdmissionDisposition.NotAdmitted);
            SetRelativeInt(
                pocket,
                "sourceDisposition",
                (int)StageTemplateSourceDisposition.RouteConditionBoundary);
            SetRelativeString(pocket, "sourceSemanticId", TerminalConditionId);
            SetRelativeInt(pocket, "sourceRevision", 1);
            SetRelativeString(pocket, "sourceSemanticDigest", string.Empty);
            SetRelativeInt(pocket, "enemyRoleCount", 0);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SetString(
                serialized,
                "canonicalTemplateDigest",
                template.ComputeCanonicalTemplateDigest());
        }

        private static void ConfigureRouteCore(
            PlayableStageDefinition route,
            StageDefinitionProfile stageDefinition,
            LinearStageTemplateProfile template)
        {
            var serialized = new SerializedObject(route);
            SetInt(serialized, "schemaVersion", 1);
            SetString(serialized, "playableStageId", PlayableStageId);
            SetInt(serialized, "routeRevision", 1);
            SetString(serialized, "canonicalRouteDigest", string.Empty);

            SerializedProperty segments = RequireProperty(serialized, "sceneSegments");
            segments.arraySize = 1;
            SerializedProperty segment = segments.GetArrayElementAtIndex(0);
            SetRelativeString(segment, "segmentId", RouteSegmentId);
            SetRelativeInt(segment, "sequenceIndex", 0);
            SetRelativeObject(segment, "stageDefinition", stageDefinition);
            SetRelativeString(segment, "entryConditionId", "run.entry.admitted");
            SetRelativeInt(
                segment,
                "entryConditionKind",
                (int)StageSegmentConditionKind
                    .RunEntrySnapshotValidatedAndFirstSegmentActivated);
            SetRelativeString(segment, "exitConditionId", TerminalConditionId);
            SetRelativeInt(
                segment,
                "exitConditionKind",
                (int)StageSegmentConditionKind
                    .StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched);
            SetRelativeInt(
                segment,
                "handoffPolicy",
                (int)StageSceneHandoffPolicy.ReturnToOwner);
            SetRelativeInt(segment, "successorKind", (int)StageSegmentSuccessorKind.None);
            SetRelativeInt(
                segment,
                "destinationSceneKind",
                (int)StageSegmentDestinationSceneKind.None);
            SetRelativeInt(
                segment,
                "transitionTokenKind",
                (int)StageSegmentTransitionTokenKind.None);
            SetRelativeInt(
                segment,
                "loaderGenerationKind",
                (int)StageSegmentLoaderGenerationKind.None);
            SetRelativeInt(
                segment,
                "navigationAuthorityKind",
                (int)StageSegmentNavigationAuthorityKind.None);
            SetRelativeInt(
                segment,
                "returnOwnerKind",
                (int)StageSegmentReturnOwnerKind.P1AStageRunRouteOwner);
            SetRelativeInt(
                segment,
                "returnOwnerReceiptPolicy",
                (int)StageReturnOwnerReceiptPolicy
                    .ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented);
            ConfigureAbsentPresentation(RequireRelative(segment, "entryPresentation"));
            ConfigureAbsentPresentation(RequireRelative(segment, "exitPresentation"));

            SerializedProperty actions = RequireProperty(serialized, "terminalActions");
            actions.arraySize = 3;
            ConfigureAction(
                actions.GetArrayElementAtIndex(0),
                ReplayActionId,
                StageRouteActionKind.Replay,
                PlayableStageId,
                StageUiRouteId.None,
                StageRouteOutcome.Clear);
            ConfigureAction(
                actions.GetArrayElementAtIndex(1),
                RetryActionId,
                StageRouteActionKind.Retry,
                PlayableStageId,
                StageUiRouteId.None,
                StageRouteOutcome.Fail);
            ConfigureAction(
                actions.GetArrayElementAtIndex(2),
                LobbyActionId,
                StageRouteActionKind.UIRoute,
                string.Empty,
                StageUiRouteId.Lobby,
                StageRouteOutcome.Clear | StageRouteOutcome.Fail);

            SerializedProperty policy = RequireProperty(serialized, "terminalResolutionPolicy");
            SetRelativeString(policy, "terminalResolutionPolicyId", TerminalPolicyId);
            SetRelativeInt(policy, "semanticRevision", 1);
            SetRelativeString(policy, "terminalResolutionPolicyDigest", string.Empty);
            SetRelativeInt(policy, "windowKind", (int)StageTerminalWindowKind.SameTerminalResolutionEpoch);
            SetRelativeInt(
                policy,
                "batchOwnerKind",
                (int)StageTerminalBatchOwnerKind.EncounterTerminalResolutionCoordinator);
            SetRelativeInt(
                policy,
                "rootAdmissionKind",
                (int)StageTerminalRootAdmissionKind.CanonicalCombatRootAdmission);
            SetRelativeInt(policy, "rootOrderKind", (int)StageTerminalRootOrderKind.RootAdmissionSequence);
            SetRelativeInt(
                policy,
                "rootIssuePoint",
                (int)StageTerminalRootIssuePoint.BeforeTerminalStateMutationAndCallbacks);
            SetRelativeInt(
                policy,
                "batchBoundaryKind",
                (int)StageTerminalBatchBoundaryKind.RootResolutionToken);
            SetRelativeInt(
                policy,
                "terminalSubjectRoles",
                (int)(StageTerminalSubjectRole.Player | StageTerminalSubjectRole.Boss));
            SetRelativeInt(
                policy,
                "coveragePolicy",
                (int)StageTerminalCoveragePolicy.ExclusiveQueuedTerminalStateMutationForBoundSubjects);
            SetRelativeInt(
                policy,
                "workExecutionKind",
                (int)StageTerminalWorkExecutionKind.SynchronousNonYieldingResolution);
            SetRelativeInt(
                policy,
                "nestedRequestPolicy",
                (int)StageTerminalNestedRequestPolicy.SameRootSameEpoch);
            SetRelativeInt(
                policy,
                "independentRequestPolicy",
                (int)StageTerminalIndependentRequestPolicy.LowerAdmissionSequenceThenNextEpoch);
            SetRelativeInt(
                policy,
                "epochStampKind",
                (int)StageTerminalEpochStampKind.EncounterTerminalEpoch);
            SetRelativeInt(
                policy,
                "coordinatorLifecycleKind",
                (int)StageTerminalCoordinatorLifecycleKind
                    .IdleOpenDrainingFinalizingEpochClosedTerminalClosedFaultedCancelled);
            SetRelativeInt(
                policy,
                "subjectFinalizationKind",
                (int)StageTerminalSubjectFinalizationKind.SynchronousTwoSubjectSnapshot);
            SetRelativeInt(
                policy,
                "tokenStatePolicy",
                (int)StageTerminalTokenStatePolicy
                    .ExplicitIdleActiveDeferredClosedWrongRunPostTerminal);
            SetRelativeInt(
                policy,
                "flushBarrier",
                (int)StageTerminalFlushBarrierKind.QueueDrainedAndSubjectsFinalized);
            SetRelativeInt(
                policy,
                "simultaneousOutcome",
                (int)StageTerminalSimultaneousOutcome.Clear);
            SetRelativeBool(policy, "requiresBossCandidateAndFinalDead", true);
            SetRelativeBool(policy, "requiresPlayerCandidateAndFinalDown", true);

            SerializedProperty reference = RequireProperty(serialized, "referenceBlock");
            SetRelativeBool(reference, "enabled", true);
            SetRelativeInt(reference, "schemaVersion", 1);
            SetRelativeInt(reference, "revision", 1);
            SetRelativeString(reference, "canonicalReferenceDigest", string.Empty);
            SetRelativeObject(reference, "stageTemplate", template);
            SetRelativeInt(reference, "briefingSchemaVersion", 1);
            SetRelativeInt(reference, "briefingRevision", 1);
            SetRelativeString(reference, "canonicalBriefingDigest", string.Empty);
            SetRelativeInt(reference, "storyEntryDisposition", (int)StageReferenceDisposition.None);
            SetRelativeInt(
                reference,
                "storyExitDisposition",
                (int)StageReferenceDisposition.NoFinalSegmentExitPresentationAuthored);
            SetRelativeInt(
                reference,
                "resultDefinitionDisposition",
                (int)StageReferenceDisposition.NotAuthoredForCurrentSchema);
            SetRelativeInt(
                reference,
                "progressionNodeDisposition",
                (int)StageReferenceDisposition.NotAuthoredForCurrentSchema);
            SetRelativeInt(
                reference,
                "ruleSetDisposition",
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema);
            SetRelativeInt(
                reference,
                "modifierDisposition",
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema);
            SetRelativeInt(
                reference,
                "enemyVariantDisposition",
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema);
            SetRelativeInt(
                reference,
                "tutorialCourseDisposition",
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema);
            SetRelativeInt(
                reference,
                "rewardPlanDisposition",
                (int)StageReferenceDisposition.NoVerifiedSource);
            SetRelativeInt(
                reference,
                "activeRunRestartPolicyDisposition",
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema);
            SetRelativeString(reference, "activeRunRestartPolicyDigest", string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            serialized.Update();
            SetRelativeString(
                RequireProperty(serialized, "terminalResolutionPolicy"),
                "terminalResolutionPolicyDigest",
                route.TerminalResolutionPolicy.ComputeCanonicalDigest());
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SetString(serialized, "canonicalRouteDigest", route.ComputeCanonicalRouteDigest());
            serialized.Update();
            SetRelativeString(
                RequireProperty(serialized, "referenceBlock"),
                "canonicalReferenceDigest",
                route.ComputeCanonicalReferenceDigest());
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Require(
                route.TryComputeCanonicalBriefingDigest(
                    out string briefingDigest,
                    out StageBriefingBuildRejectReason briefingReject),
                $"Courtyard briefing digest cannot be computed: {briefingReject}.");
            serialized.Update();
            SetRelativeString(
                RequireProperty(serialized, "referenceBlock"),
                "canonicalBriefingDigest",
                briefingDigest);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureLocalization(StageResultLocalizationTable localization)
        {
            var serialized = new SerializedObject(localization);
            SetInt(serialized, "schemaVersion", 1);
            SetString(serialized, "tableId", LocalizationTableId);
            SetInt(serialized, "tableRevision", 1);
            SetString(serialized, "defaultLocaleId", "ko-KR");

            SerializedProperty locales = RequireProperty(serialized, "locales");
            locales.arraySize = 2;
            ConfigureLocale(
                locales.GetArrayElementAtIndex(0),
                "en-US",
                new[]
                {
                    Pair(LobbyLabelKey, "LOBBY"),
                    Pair(ReplayLabelKey, "REPLAY"),
                    Pair(RetryLabelKey, "RETRY"),
                    Pair(ForwardRiskProofTextKey, "Forward pressure {0}"),
                    Pair(SurvivalProofTextKey, "No combat incapacitation"),
                    Pair(StageNameKey, "OLYMPUS COURTYARD DRILL"),
                    Pair(CombatTimeKey, "COMBAT TIME\nCOMBAT"),
                    Pair(RecordsKey, "COMBAT RECORD"),
                    Pair(TotalTimeKey, "OPERATION TIME\nTOTAL"),
                    Pair(ClearStatusKey, "CLEAR"),
                    Pair(FailStatusKey, "FAILED"),
                    Pair(TitleFormatKey, "{0} / {1}")
                });
            ConfigureLocale(
                locales.GetArrayElementAtIndex(1),
                "ko-KR",
                new[]
                {
                    Pair(LobbyLabelKey, "로비로"),
                    Pair(ReplayLabelKey, "다시 보기"),
                    Pair(RetryLabelKey, "재도전"),
                    Pair(ForwardRiskProofTextKey, "전진 압박 유지 {0}"),
                    Pair(SurvivalProofTextKey, "전투 불능 없이 생존"),
                    Pair(StageNameKey, "올림푸스 중정 훈련"),
                    Pair(CombatTimeKey, "전투 시간\nCOMBAT"),
                    Pair(RecordsKey, "전투 기록"),
                    Pair(TotalTimeKey, "작전 시간\nTOTAL"),
                    Pair(ClearStatusKey, "클리어"),
                    Pair(FailStatusKey, "실패"),
                    Pair(TitleFormatKey, "{0} / {1}")
                });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Require(
                localization.TryValidate(out string error),
                "Courtyard localization is invalid: " + error);
        }

        private static void ConfigurePresentationProfile(StageResultPresentationProfile profile)
        {
            var serialized = new SerializedObject(profile);
            SetInt(serialized, "schemaVersion", 1);
            SetString(serialized, "profileId", PresentationProfileId);
            SetInt(serialized, "profileRevision", 1);
            SetString(serialized, "playableStageId", PlayableStageId);
            SetInt(serialized, "supportedRunSchemaVersion", 1);
            SetString(serialized, "stageCode", "01-2");
            SetString(serialized, "titleFormatKey", TitleFormatKey);
            SetString(serialized, "stageNameKey", StageNameKey);
            SetString(serialized, "clearStatusKey", ClearStatusKey);
            SetString(serialized, "failStatusKey", FailStatusKey);
            SetString(serialized, "totalActiveTimeLabelKey", TotalTimeKey);
            SetString(serialized, "combatActiveTimeLabelKey", CombatTimeKey);
            SetString(serialized, "recordsCategoryKey", RecordsKey);
            SetString(serialized, "replayActionKey", ReplayLabelKey);
            SetString(serialized, "retryActionKey", RetryLabelKey);
            SetString(serialized, "lobbyActionKey", LobbyLabelKey);
            SetInt(serialized, "proofRowLimit", 2);
            SerializedProperty rules = RequireProperty(serialized, "proofRules");
            rules.arraySize = 2;
            ConfigureProofRule(
                rules.GetArrayElementAtIndex(0),
                StageRunFactVocabulary.SurvivalNoPlayerDownProofId,
                SurvivalProofTextKey,
                StageResultProofValueFormat.Literal);
            ConfigureProofRule(
                rules.GetArrayElementAtIndex(1),
                StageRunFactVocabulary.MovementForwardRiskTimeProofId,
                ForwardRiskProofTextKey,
                StageResultProofValueFormat.ActualDuration);
            RequireProperty(serialized, "clearTitleColor").colorValue =
                new Color(0.06666667f, 0.14901961f, 0.34509805f, 1f);
            RequireProperty(serialized, "failTitleColor").colorValue =
                new Color(0.62f, 0.08f, 0.12f, 1f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigurePresentationCatalog(
            StageResultPresentationCatalog catalog,
            StageResultLocalizationTable localization,
            StageResultPresentationProfile profile)
        {
            var serialized = new SerializedObject(catalog);
            SetInt(serialized, "schemaVersion", 1);
            SetString(serialized, "catalogId", PresentationCatalogId);
            SetInt(serialized, "catalogRevision", 1);
            SetObject(serialized, "localizationTable", localization);
            SerializedProperty profiles = RequireProperty(serialized, "profiles");
            profiles.arraySize = 1;
            profiles.GetArrayElementAtIndex(0).objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Require(catalog.TryValidate(out string error), "Courtyard presentation catalog is invalid: " + error);
        }

        private static void ConfigureResultDefinition(
            StageResultDefinition result,
            StageResultPresentationCatalog catalog,
            StageResultPresentationProfile profile,
            StageResultLocalizationTable localization)
        {
            var serialized = new SerializedObject(result);
            SetInt(serialized, "schemaVersion", 1);
            SetString(serialized, "resultDefinitionId", ResultDefinitionId);
            SetInt(serialized, "revision", 1);
            SetInt(serialized, "evaluationContentRevision", 1);
            SetString(serialized, "playableStageId", PlayableStageId);
            SetInt(serialized, "supportedRunSchemaVersion", 1);
            SetInt(
                serialized,
                "masterySetDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema);
            SetString(serialized, "masterySetId", string.Empty);
            SetInt(serialized, "masterySetRevision", 0);
            SetString(serialized, "masterySetSemanticDigest", string.Empty);
            SetInt(
                serialized,
                "requiredFactCapabilitiesDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema);
            SetInt(serialized, "requiredFactCapabilityCount", 0);
            SetString(serialized, "requiredFactCapabilitiesDigest", string.Empty);
            SetInt(
                serialized,
                "allowedSemanticProofsDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema);
            SetInt(serialized, "allowedSemanticProofCount", 0);
            SetString(serialized, "allowedSemanticProofsDigest", string.Empty);
            SetInt(serialized, "presentationBindingRevision", 1);
            SetObject(serialized, "canonicalPresentationCatalog", catalog);
            SetObject(serialized, "presentationProfile", profile);
            SetObject(serialized, "localizationTable", localization);
            SetInt(
                serialized,
                "localeResolutionPolicy",
                (int)StageResultLocaleResolutionPolicy
                    .ExactThenLanguageThenDefaultOrdinalIgnoreCase);

            SerializedProperty mappings = RequireProperty(serialized, "actionMappings");
            mappings.arraySize = 4;
            ConfigureResultMapping(
                mappings.GetArrayElementAtIndex(0),
                StageRouteOutcome.Clear,
                ReplayActionId,
                ReplayLabelKey,
                StageResultActionPresentationRole.Primary,
                0);
            ConfigureResultMapping(
                mappings.GetArrayElementAtIndex(1),
                StageRouteOutcome.Clear,
                LobbyActionId,
                LobbyLabelKey,
                StageResultActionPresentationRole.Secondary,
                1);
            ConfigureResultMapping(
                mappings.GetArrayElementAtIndex(2),
                StageRouteOutcome.Fail,
                RetryActionId,
                RetryLabelKey,
                StageResultActionPresentationRole.Primary,
                0);
            ConfigureResultMapping(
                mappings.GetArrayElementAtIndex(3),
                StageRouteOutcome.Fail,
                LobbyActionId,
                LobbyLabelKey,
                StageResultActionPresentationRole.Secondary,
                1);
            SetString(serialized, "evaluationContentDigest", string.Empty);
            SetString(serialized, "presentationBindingDigest", string.Empty);
            SetString(serialized, "presentationSourceDigest", string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Require(
                result.TryComputeCanonicalDigests(
                    out string evaluationDigest,
                    out string bindingDigest,
                    out string sourceDigest,
                    out string error),
                "Courtyard result digests cannot be computed: " + error);
            SetString(serialized, "evaluationContentDigest", evaluationDigest);
            SetString(serialized, "presentationBindingDigest", bindingDigest);
            SetString(serialized, "presentationSourceDigest", sourceDigest);
        }

        private static void ConfigureProgressionNode(
            StageProgressionNode node,
            PlayableStageDefinition route)
        {
            var serialized = new SerializedObject(node);
            SetInt(serialized, "schemaVersion", 1);
            SetString(serialized, "progressionNodeId", ProgressionNodeId);
            SetInt(serialized, "revision", 1);
            SetInt(serialized, "contentRevision", 1);
            SetInt(
                serialized,
                "battleStageDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema);
            SetString(serialized, "battleStageId", string.Empty);
            RequireProperty(serialized, "prerequisites").arraySize = 0;
            RequireProperty(serialized, "recommendedNext").arraySize = 0;
            SetInt(
                serialized,
                "preBattleStoryDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema);
            SetString(serialized, "preBattleStoryId", string.Empty);
            SetInt(
                serialized,
                "postBattleStoryDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema);
            SetString(serialized, "postBattleStoryId", string.Empty);
            SetInt(
                serialized,
                "afterClearScriptDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAuthoredForCurrentSchema);
            SetString(serialized, "afterClearScriptId", string.Empty);
            SetInt(
                serialized,
                "rewardPlanDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema);
            SetString(serialized, "rewardPlanId", string.Empty);
            SetInt(serialized, "rewardPlanRevision", 0);
            SetString(serialized, "rewardPlanDigest", string.Empty);
            SetInt(serialized, "bindingRevision", 1);
            SetString(serialized, "playableStageId", PlayableStageId);
            SetInt(serialized, "routeRevision", 1);
            SetString(serialized, "canonicalRouteDigest", route.CanonicalRouteDigest);
            SetString(serialized, "progressionGraphId", ProgressionGraphId);
            SetInt(serialized, "progressionGraphRevision", 1);
            SetString(serialized, "contentDigest", string.Empty);
            SetString(serialized, "bindingDigest", string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Require(
                node.TryComputeCanonicalDigests(
                    out string contentDigest,
                    out string bindingDigest,
                    out string error),
                "Courtyard progression-node digests cannot be computed: " + error);
            SetString(serialized, "contentDigest", contentDigest);
            SetString(serialized, "bindingDigest", bindingDigest);
        }

        private static void ConfigureProgressionGraph(
            StageProgressionGraph graph,
            StageProgressionNode node)
        {
            var serialized = new SerializedObject(graph);
            SetInt(serialized, "schemaVersion", 1);
            SetString(serialized, "progressionGraphId", ProgressionGraphId);
            SetInt(serialized, "revision", 1);
            SetInt(
                serialized,
                "cyclePolicy",
                (int)StageProgressionCyclePolicy.DisallowCyclesWithinEachRelation);
            SerializedProperty nodes = RequireProperty(serialized, "nodes");
            nodes.arraySize = 1;
            nodes.GetArrayElementAtIndex(0).objectReferenceValue = node;
            SetString(serialized, "canonicalDigest", string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Require(
                graph.TryComputeCanonicalDigest(
                    out string digest,
                    out string error),
                "Courtyard progression-graph digest cannot be computed: " + error);
            SetString(serialized, "canonicalDigest", digest);
        }

        private static void ConfigureAndSealJoin(PackAssets pack)
        {
            var serialized = new SerializedObject(pack.Route);
            SerializedProperty join = RequireProperty(serialized, "resultProgressionJoin");
            SetRelativeBool(join, "present", true);
            SetRelativeInt(join, "schemaVersion", 1);
            SetRelativeInt(join, "revision", 1);
            SetRelativeInt(
                join,
                "semanticCoupling",
                (int)StageResultJoinSemanticCoupling
                    .PresentationAuditSidecarOutsideP1ASemanticResult);
            SetRelativeInt(
                join,
                "resultDefinitionDisposition",
                (int)StageResultProgressionReferenceDisposition.Present);
            SetRelativeObject(join, "resultDefinition", pack.ResultDefinition);
            SetRelativeObject(join, "canonicalPresentationCatalog", pack.PresentationCatalog);
            SetRelativeInt(
                join,
                "progressionNodeDisposition",
                (int)StageResultProgressionReferenceDisposition.Present);
            SetRelativeObject(join, "progressionNode", pack.ProgressionNode);
            SetRelativeInt(
                join,
                "progressionGraphDisposition",
                (int)StageResultProgressionReferenceDisposition.Present);
            SetRelativeObject(join, "progressionGraph", pack.ProgressionGraph);
            SetRelativeInt(
                join,
                "rewardPlanDisposition",
                (int)StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema);
            SetRelativeString(join, "rewardPlanDigest", string.Empty);
            SetRelativeString(join, "canonicalDigest", string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Require(
                pack.Route.TryComputeResultProgressionJoinDigest(
                    out string digest,
                    out string error),
                "Courtyard result/progression join digest cannot be computed: " + error);
            serialized.Update();
            SetRelativeString(
                RequireProperty(serialized, "resultProgressionJoin"),
                "canonicalDigest",
                digest);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void ValidatePackOrThrow()
        {
            ValidatePackOrThrow(LoadRequiredPack());
        }

        private static void ValidatePackOrThrow(PackAssets pack)
        {
            CaptureAcceptedOlympusSourceFingerprint();
            ValidatePersistentIdentityIsolation(pack);
            ValidateStageDefinition(pack);
            ValidateRouteAndTemplate(pack);
            ValidatePresentationAndResult(pack);
            ValidateProgressionAndJoin(pack);
        }

        private static string CaptureAcceptedOlympusSourceFingerprint()
        {
            PlayableStageDefinition route =
                LoadRequired<PlayableStageDefinition>(SourcePlayableStagePath);
            LinearStageTemplateProfile template =
                LoadRequired<LinearStageTemplateProfile>(SourceStageTemplatePath);
            StageResultDefinition result =
                LoadRequired<StageResultDefinition>(SourceResultDefinitionPath);
            StageProgressionNode node =
                LoadRequired<StageProgressionNode>(SourceProgressionNodePath);
            StageProgressionGraph graph =
                LoadRequired<StageProgressionGraph>(SourceProgressionGraphPath);
            StageResultLocalizationTable localization =
                LoadRequired<StageResultLocalizationTable>(SourceLocalizationPath);
            StageResultPresentationProfile profile =
                LoadRequired<StageResultPresentationProfile>(SourcePresentationProfilePath);
            StageResultPresentationCatalog catalog =
                LoadRequired<StageResultPresentationCatalog>(SourcePresentationCatalogPath);

            RequireEqual(
                route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                AcceptedOlympusPolicyDigest,
                "accepted Olympus terminal policy digest");
            RequireEqual(route.CanonicalRouteDigest, AcceptedOlympusRouteDigest, "accepted Olympus route digest");
            RequireEqual(template.CanonicalTemplateDigest, AcceptedOlympusTemplateDigest, "accepted Olympus template digest");
            RequireEqual(
                route.ReferenceBlock.CanonicalReferenceDigest,
                AcceptedOlympusReferenceDigest,
                "accepted Olympus reference digest");
            RequireEqual(
                route.ReferenceBlock.CanonicalBriefingDigest,
                AcceptedOlympusBriefingDigest,
                "accepted Olympus briefing digest");
            RequireEqual(
                result.EvaluationContentDigest,
                AcceptedOlympusResultEvaluationDigest,
                "accepted Olympus result evaluation digest");
            RequireEqual(
                result.PresentationBindingDigest,
                AcceptedOlympusResultBindingDigest,
                "accepted Olympus result binding digest");
            RequireEqual(
                result.PresentationSourceDigest,
                AcceptedOlympusResultSourceDigest,
                "accepted Olympus result source digest");
            RequireEqual(node.ContentDigest, AcceptedOlympusNodeContentDigest, "accepted Olympus node content digest");
            RequireEqual(node.BindingDigest, AcceptedOlympusNodeBindingDigest, "accepted Olympus node binding digest");
            RequireEqual(graph.CanonicalDigest, AcceptedOlympusGraphDigest, "accepted Olympus graph digest");
            RequireEqual(
                route.ResultProgressionJoin.CanonicalDigest,
                AcceptedOlympusJoinDigest,
                "accepted Olympus result/progression join digest");

            RequireEqual(route.ComputeCanonicalRouteDigest(), AcceptedOlympusRouteDigest, "computed accepted Olympus route digest");
            RequireEqual(template.ComputeCanonicalTemplateDigest(), AcceptedOlympusTemplateDigest, "computed accepted Olympus template digest");
            RequireEqual(route.ComputeCanonicalReferenceDigest(), AcceptedOlympusReferenceDigest, "computed accepted Olympus reference digest");
            Require(
                route.TryComputeCanonicalBriefingDigest(
                    out string briefingDigest,
                    out StageBriefingBuildRejectReason briefingReject),
                $"Accepted Olympus briefing cannot be recomputed: {briefingReject}.");
            RequireEqual(briefingDigest, AcceptedOlympusBriefingDigest, "computed accepted Olympus briefing digest");
            Require(
                result.TryComputeCanonicalDigests(
                    out string evaluation,
                    out string binding,
                    out string source,
                    out string resultError),
                "Accepted Olympus result digests cannot be recomputed: " + resultError);
            RequireEqual(evaluation, AcceptedOlympusResultEvaluationDigest, "computed accepted Olympus result evaluation digest");
            RequireEqual(binding, AcceptedOlympusResultBindingDigest, "computed accepted Olympus result binding digest");
            RequireEqual(source, AcceptedOlympusResultSourceDigest, "computed accepted Olympus result source digest");
            Require(
                node.TryComputeCanonicalDigests(
                    out string nodeContent,
                    out string nodeBinding,
                    out string nodeError),
                "Accepted Olympus node digests cannot be recomputed: " + nodeError);
            RequireEqual(nodeContent, AcceptedOlympusNodeContentDigest, "computed accepted Olympus node content digest");
            RequireEqual(nodeBinding, AcceptedOlympusNodeBindingDigest, "computed accepted Olympus node binding digest");
            Require(
                graph.TryComputeCanonicalDigest(out string graphDigest, out string graphError),
                "Accepted Olympus graph digest cannot be recomputed: " + graphError);
            RequireEqual(graphDigest, AcceptedOlympusGraphDigest, "computed accepted Olympus graph digest");
            Require(
                route.TryComputeResultProgressionJoinDigest(out string joinDigest, out string joinError),
                "Accepted Olympus join digest cannot be recomputed: " + joinError);
            RequireEqual(joinDigest, AcceptedOlympusJoinDigest, "computed accepted Olympus join digest");
            Require(localization.TryValidate(out string localizationError), "Accepted Olympus localization is invalid: " + localizationError);
            Require(profile.TryValidate(localization, out string profileError), "Accepted Olympus presentation profile is invalid: " + profileError);
            Require(catalog.TryValidate(out string catalogError), "Accepted Olympus presentation catalog is invalid: " + catalogError);

            return string.Join(
                "|",
                route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                route.CanonicalRouteDigest,
                template.CanonicalTemplateDigest,
                route.ReferenceBlock.CanonicalReferenceDigest,
                route.ReferenceBlock.CanonicalBriefingDigest,
                result.EvaluationContentDigest,
                result.PresentationBindingDigest,
                result.PresentationSourceDigest,
                node.ContentDigest,
                node.BindingDigest,
                graph.CanonicalDigest,
                route.ResultProgressionJoin.CanonicalDigest,
                localization.ComputeCanonicalDigest(),
                profile.ComputeCanonicalDigest(),
                catalog.CatalogId,
                catalog.CatalogRevision.ToString());
        }

        private static void ValidatePersistentIdentityIsolation(PackAssets pack)
        {
            var targetGuids = new HashSet<string>(StringComparer.Ordinal);
            var sourceGuids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < SourcePaths.Length; i++)
            {
                string sourceGuid = AssetDatabase.AssetPathToGUID(SourcePaths[i]);
                Require(!string.IsNullOrWhiteSpace(sourceGuid), "Missing seed asset GUID: " + SourcePaths[i]);
                sourceGuids.Add(sourceGuid);
            }

            for (int i = 0; i < TargetPaths.Length; i++)
            {
                string targetGuid = AssetDatabase.AssetPathToGUID(TargetPaths[i]);
                Require(!string.IsNullOrWhiteSpace(targetGuid), "Missing target asset GUID: " + TargetPaths[i]);
                Require(targetGuids.Add(targetGuid), "B1-1 target assets must have distinct GUIDs.");
                Require(!sourceGuids.Contains(targetGuid), "A B1-1 target reuses an Olympus seed GUID: " + TargetPaths[i]);
            }

            UnityEngine.Object[] persistent = pack.PersistentAssets;
            for (int i = 0; i < persistent.Length; i++)
            {
                Require(persistent[i] != null, "B1-1 persistent asset is missing.");
                Require(
                    string.Equals(
                        AssetDatabase.GetAssetPath(persistent[i]),
                        TargetPaths[i],
                        StringComparison.Ordinal),
                    $"B1-1 asset {i} is not stored at its exact target path.");
            }
        }

        private static void ValidateStageDefinition(PackAssets pack)
        {
            StageDefinitionProfile definition = pack.StageDefinition;
            RequireEqual(definition.StageId, StageDefinitionId, "stage definition ID");
            RequireEqual(definition.MapScenePath, ScenePath, "stage scene path");
            RequireEqual(definition.MapRootName, MapRootName, "stage map root");
            RequireEqual(definition.MapContentRootName, MapContentRootName, "stage content root");
            Require(definition.MapScale == Vector3.one, "Courtyard map scale must be one.");
            RequireEqual(definition.LayoutId, LayoutId, "stage layout ID");
            Require(definition.AnchorCount == 3, "Courtyard definition must contain three exact anchors.");
            Require(definition.SpawnCount == 3, "Courtyard definition must contain three exact spawn rows.");
            Require(definition.CutsceneHandoffCount == 0, "Courtyard definition must have no cutscene handoff.");
            Require(definition.RuntimeStateCount == 1, "Courtyard definition must have one terminal state.");
            Require(definition.SourceReferenceCount == 1, "Courtyard definition must have one local provenance row.");

            ValidateDefinitionAnchor(
                definition.GetAnchor(0),
                PlayerAnchorId,
                new Vector3(0f, 0f, -4.5f),
                Vector3.zero);
            ValidateDefinitionAnchor(
                definition.GetAnchor(1),
                BossAnchorId,
                new Vector3(0f, 0f, 3.6f),
                new Vector3(0f, 180f, 0f));
            ValidateDefinitionAnchor(
                definition.GetAnchor(2),
                RangedAnchorId,
                new Vector3(-5f, 0f, 1.5f),
                new Vector3(0f, 135f, 0f));
            ValidateDefinitionSpawn(
                definition.GetSpawn(0),
                PlayerSpawnId,
                StageSpawnKind.Player,
                PlayerPositionId,
                PlayerAnchorId,
                "PlayerParty",
                null,
                0f);
            ValidateDefinitionSpawn(
                definition.GetSpawn(1),
                BossSpawnId,
                StageSpawnKind.Boss,
                BossPositionId,
                BossAnchorId,
                "CourtyardDrill.MiniBoss",
                null,
                0f);
            ValidateDefinitionSpawn(
                definition.GetSpawn(2),
                RangedSpawnId,
                StageSpawnKind.Add,
                RangedPositionId,
                RangedAnchorId,
                "SciFiSoldier.Ranged",
                pack.RangedArchetype,
                0.6f);
            RequireEqual(
                definition.GetRuntimeState(0).ConditionId,
                TerminalConditionId,
                "terminal runtime-state condition");
            Require(
                definition.GetRuntimeState(0).StateKind == StageRuntimeStateKind.StageClear
                    && definition.GetRuntimeState(0).PositionId == 9001
                    && string.IsNullOrEmpty(definition.GetRuntimeState(0).AnchorId),
                "Courtyard terminal runtime-state row is stale.");
            RequireEqual(
                definition.GetSourceReference(0).SourcePath,
                ScenePath,
                "local source-reference path");
        }

        private static void ValidateRouteAndTemplate(PackAssets pack)
        {
            PlayableStageDefinition route = pack.Route;
            Require(route.SchemaVersion == 1 && route.RouteRevision == 1, "Courtyard route schema/revision must be 1/1.");
            RequireEqual(route.PlayableStageId, PlayableStageId, "playable-stage ID");
            Require(route.SceneSegmentCount == 1, "Courtyard route must contain exactly one segment.");
            Require(route.TerminalActionCount == 3, "Courtyard route must contain three terminal actions.");
            RequireEqual(route.CanonicalRouteDigest, route.ComputeCanonicalRouteDigest(), "route digest");
            RequireEqual(
                route.TerminalResolutionPolicy.TerminalResolutionPolicyId,
                TerminalPolicyId,
                "terminal policy ID");
            RequireEqual(
                route.TerminalResolutionPolicy.TerminalResolutionPolicyDigest,
                route.TerminalResolutionPolicy.ComputeCanonicalDigest(),
                "terminal policy digest");

            StageSceneSegmentRef segment = route.GetSceneSegment(0);
            RequireEqual(segment.SegmentId, RouteSegmentId, "route segment ID");
            Require(ReferenceEquals(segment.StageDefinition, pack.StageDefinition), "Route must reference the exact Courtyard definition.");
            RequireEqual(segment.ExitConditionId, TerminalConditionId, "route terminal condition");
            Require(segment.EntryPresentation != null && !segment.EntryPresentation.IsPresent, "Courtyard story entry must be typed absent.");
            Require(segment.ExitPresentation != null && !segment.ExitPresentation.IsPresent, "Courtyard story exit must be typed absent.");
            Require(
                StageRunRouteSnapshot.TryCreate(route, out StageRunRouteSnapshot snapshot, out string routeError),
                "Courtyard route snapshot is invalid: " + routeError);
            Require(snapshot.SegmentCount == 1 && snapshot.IsEntrySegment(0) && snapshot.IsTerminalSegment(0), "Courtyard route row must own Entry|Terminal.");
            ValidateRouteAction(
                route,
                ReplayActionId,
                StageRouteActionKind.Replay,
                PlayableStageId,
                StageUiRouteId.None,
                StageRouteOutcome.Clear);
            ValidateRouteAction(
                route,
                RetryActionId,
                StageRouteActionKind.Retry,
                PlayableStageId,
                StageUiRouteId.None,
                StageRouteOutcome.Fail);
            ValidateRouteAction(
                route,
                LobbyActionId,
                StageRouteActionKind.UIRoute,
                string.Empty,
                StageUiRouteId.Lobby,
                StageRouteOutcome.Clear | StageRouteOutcome.Fail);

            LinearStageTemplateProfile template = pack.Template;
            RequireEqual(template.StageTemplateId, TemplateId, "template ID");
            Require(template.TemplateSchemaVersion == 1 && template.TemplateRevision == 1, "Template schema/revision must be 1/1.");
            RequireEqual(template.CanonicalTemplateDigest, template.ComputeCanonicalTemplateDigest(), "template digest");
            Require(template.CanonicalRouteSegmentCount == 1, "Template must contain one route segment.");
            StageTemplateRouteSegmentRef templateSegment = template.GetCanonicalRouteSegment(0);
            RequireEqual(templateSegment.TemplateSegmentId, TemplateSegmentId, "template segment ID");
            RequireEqual(templateSegment.RouteSegmentId, RouteSegmentId, "template route-segment binding");
            Require(templateSegment.PocketCount == 1, "Template must contain one truthful terminal pocket.");
            RequireEqual(templateSegment.GetPocket(0).PocketId, TemplatePocketId, "template pocket ID");
            Require(ReferenceEquals(route.ReferenceBlock.StageTemplate, template), "Route must reference the exact Courtyard template.");
            Require(route.ReferenceBlock.Revision == 1 && route.ReferenceBlock.BriefingRevision == 1, "Reference/briefing revisions must be 1/1.");
            Require(
                route.ReferenceBlock.StoryEntryDisposition == StageReferenceDisposition.None
                    && route.ReferenceBlock.StoryExitDisposition
                        == StageReferenceDisposition.NoFinalSegmentExitPresentationAuthored
                    && route.ReferenceBlock.ResultDefinitionDisposition
                        == StageReferenceDisposition.NotAuthoredForCurrentSchema
                    && route.ReferenceBlock.ProgressionNodeDisposition
                        == StageReferenceDisposition.NotAuthoredForCurrentSchema,
                "Courtyard reference typed absences are stale.");
            RequireEqual(route.ReferenceBlock.CanonicalReferenceDigest, route.ComputeCanonicalReferenceDigest(), "reference digest");
            Require(
                route.TryComputeCanonicalBriefingDigest(out string briefingDigest, out StageBriefingBuildRejectReason briefingReject),
                $"Courtyard briefing is invalid: {briefingReject}.");
            RequireEqual(route.ReferenceBlock.CanonicalBriefingDigest, briefingDigest, "briefing digest");
            Require(
                route.TryCreateBriefingReadModel(out _, out StageBriefingBuildRejectReason briefingCreateReject),
                $"Courtyard briefing snapshot is invalid: {briefingCreateReject}.");
        }

        private static void ValidatePresentationAndResult(PackAssets pack)
        {
            Require(pack.Localization.TryValidate(out string localizationError), "Courtyard localization is invalid: " + localizationError);
            Require(
                pack.PresentationProfile.TryValidate(pack.Localization, out string profileError),
                "Courtyard presentation profile is invalid: " + profileError);
            Require(pack.PresentationCatalog.TryValidate(out string catalogError), "Courtyard presentation catalog is invalid: " + catalogError);
            Require(
                pack.PresentationCatalog.TryValidateExactSources(
                    PlayableStageId,
                    pack.PresentationProfile,
                    pack.Localization,
                    out string exactSourceError),
                "Courtyard presentation sources are not exact: " + exactSourceError);
            RequireEqual(pack.Localization.TableId, LocalizationTableId, "localization table ID");
            RequireEqual(pack.PresentationProfile.ProfileId, PresentationProfileId, "presentation profile ID");
            RequireEqual(pack.PresentationCatalog.CatalogId, PresentationCatalogId, "presentation catalog ID");

            StageResultDefinition result = pack.ResultDefinition;
            RequireEqual(result.ResultDefinitionId, ResultDefinitionId, "result-definition ID");
            RequireEqual(result.PlayableStageId, PlayableStageId, "result playable-stage ID");
            Require(
                ReferenceEquals(result.CanonicalPresentationCatalog, pack.PresentationCatalog)
                    && ReferenceEquals(result.PresentationProfile, pack.PresentationProfile)
                    && ReferenceEquals(result.LocalizationTable, pack.Localization),
                "Result definition must retain the exact isolated presentation source triad.");
            Require(
                result.TryComputeCanonicalDigests(
                    out string evaluation,
                    out string binding,
                    out string source,
                    out string resultError),
                "Courtyard result digests are invalid: " + resultError);
            RequireEqual(result.EvaluationContentDigest, evaluation, "result evaluation digest");
            RequireEqual(result.PresentationBindingDigest, binding, "result binding digest");
            RequireEqual(result.PresentationSourceDigest, source, "result source digest");
            Require(result.TryCreateSnapshot(out _, out string snapshotError), "Courtyard result snapshot is invalid: " + snapshotError);
        }

        private static void ValidateProgressionAndJoin(PackAssets pack)
        {
            StageProgressionNode node = pack.ProgressionNode;
            RequireEqual(node.ProgressionNodeId, ProgressionNodeId, "progression-node ID");
            Require(node.Revision == 1 && node.ContentRevision == 1 && node.BindingRevision == 1, "Progression node revisions must be 1/1/1.");
            Require(node.PrerequisiteCount == 0 && node.RecommendedNextCount == 0, "B1-1 route-owned progression node must have no availability edges.");
            RequireEqual(node.PlayableStageId, PlayableStageId, "progression-node playable-stage ID");
            RequireEqual(node.CanonicalRouteDigest, pack.Route.CanonicalRouteDigest, "progression-node route digest");
            Require(
                node.TryComputeCanonicalDigests(out string content, out string binding, out string nodeError),
                "Courtyard progression-node digests are invalid: " + nodeError);
            RequireEqual(node.ContentDigest, content, "progression-node content digest");
            RequireEqual(node.BindingDigest, binding, "progression-node binding digest");

            StageProgressionGraph graph = pack.ProgressionGraph;
            RequireEqual(graph.ProgressionGraphId, ProgressionGraphId, "progression-graph ID");
            Require(graph.Revision == 1 && graph.NodeCount == 1, "Progression graph must be revision 1 with one node.");
            Require(ReferenceEquals(graph.GetNode(0), node), "Progression graph must reference the exact Courtyard node.");
            Require(
                graph.TryComputeCanonicalDigest(out string graphDigest, out string graphError),
                "Courtyard progression-graph digest is invalid: " + graphError);
            RequireEqual(graph.CanonicalDigest, graphDigest, "progression-graph digest");

            StageResultProgressionJoinBlock join = pack.Route.ResultProgressionJoin;
            Require(join != null && join.Present && join.SchemaVersion == 1 && join.Revision == 1, "Courtyard join must be present at schema/revision 1/1.");
            Require(
                ReferenceEquals(join.ResultDefinition, pack.ResultDefinition)
                    && ReferenceEquals(join.CanonicalPresentationCatalog, pack.PresentationCatalog)
                    && ReferenceEquals(join.ProgressionNode, node)
                    && ReferenceEquals(join.ProgressionGraph, graph),
                "Courtyard join must retain only the exact isolated sidecars.");
            Require(
                pack.Route.TryComputeResultProgressionJoinDigest(out string joinDigest, out string joinError),
                "Courtyard join digest is invalid: " + joinError);
            RequireEqual(join.CanonicalDigest, joinDigest, "result/progression join digest");
            Require(
                StageRunResultProgressionJoinSnapshot.TryCreate(
                    pack.Route,
                    out StageRunResultProgressionJoinSnapshot snapshot,
                    out string snapshotError),
                "Courtyard admission join snapshot is invalid: " + snapshotError);
            Require(snapshot.TryValidateIntegrity(out string integrityError), "Courtyard join integrity is invalid: " + integrityError);
        }

        private static void ConfigureAnchor(
            SerializedProperty anchor,
            string anchorId,
            Vector3 position,
            Vector3 euler,
            string purpose)
        {
            SetRelativeString(anchor, "anchorId", anchorId);
            SetRelativeString(anchor, "groupId", "CombatSpawnAnchors");
            RequireRelative(anchor, "expectedPosition").vector3Value = position;
            RequireRelative(anchor, "expectedEuler").vector3Value = euler;
            SetRelativeString(anchor, "purpose", purpose);
        }

        private static void ValidateDefinitionAnchor(
            StageDefinitionProfile.AnchorRef anchor,
            string anchorId,
            Vector3 position,
            Vector3 euler)
        {
            RequireEqual(anchor.AnchorId, anchorId, "stage-definition anchor ID");
            RequireEqual(anchor.GroupId, "CombatSpawnAnchors", $"{anchorId} group ID");
            Require(anchor.ExpectedPosition == position, $"{anchorId} expected position is stale.");
            Require(anchor.ExpectedEuler == euler, $"{anchorId} expected Euler is stale.");
        }

        private static void ValidateDefinitionSpawn(
            StageDefinitionProfile.SpawnRef spawn,
            string spawnId,
            StageSpawnKind kind,
            int positionId,
            string anchorId,
            string payloadId,
            CombatEnemyArchetypeProfile archetype,
            float delaySeconds)
        {
            RequireEqual(spawn.SpawnId, spawnId, "stage-definition spawn ID");
            Require(spawn.SpawnKind == kind, $"{spawnId} spawn kind is stale.");
            Require(spawn.PositionId == positionId, $"{spawnId} position ID is stale.");
            RequireEqual(spawn.AnchorId, anchorId, $"{spawnId} anchor ID");
            RequireEqual(spawn.PayloadId, payloadId, $"{spawnId} payload ID");
            Require(ReferenceEquals(spawn.PayloadArchetype, archetype), $"{spawnId} payload archetype is stale.");
            Require(
                spawn.AuthoredCount == 1
                    && Mathf.Approximately(spawn.AuthoredDelaySeconds, delaySeconds),
                $"{spawnId} count/delay is stale.");
        }

        private static void ConfigureSpawn(
            SerializedProperty spawn,
            string spawnId,
            StageSpawnKind kind,
            int positionId,
            string anchorId,
            string payloadId,
            CombatEnemyArchetypeProfile archetype,
            int count,
            float delaySeconds,
            string note)
        {
            SetRelativeString(spawn, "spawnId", spawnId);
            SetRelativeInt(spawn, "spawnKind", (int)kind);
            SetRelativeInt(spawn, "positionId", positionId);
            SetRelativeString(spawn, "anchorId", anchorId);
            SetRelativeString(spawn, "payloadId", payloadId);
            SetRelativeObject(spawn, "payloadArchetype", archetype);
            SetRelativeInt(spawn, "count", count);
            RequireRelative(spawn, "delaySeconds").floatValue = delaySeconds;
            SetRelativeString(spawn, "note", note);
        }

        private static void ConfigureAbsentPresentation(SerializedProperty presentation)
        {
            SetRelativeBool(presentation, "enabled", false);
            SetRelativeObject(presentation, "stageDefinition", null);
            SetRelativeString(presentation, "handoffId", string.Empty);
            SetRelativeObject(presentation, "cinematicProfile", null);
            SetRelativeString(presentation, "expectedPortId", string.Empty);
            SetRelativeObject(presentation, "expectedPlayableAsset", null);
            SetRelativeString(presentation, "triggerConditionId", string.Empty);
            SetRelativeString(presentation, "completionConditionId", string.Empty);
        }

        private static void ConfigureAction(
            SerializedProperty action,
            string actionId,
            StageRouteActionKind kind,
            string targetPlayableStageId,
            StageUiRouteId targetUiRouteId,
            StageRouteOutcome outcomes)
        {
            SetRelativeString(action, "actionId", actionId);
            SetRelativeInt(action, "actionKind", (int)kind);
            SetRelativeString(action, "targetPlayableStageId", targetPlayableStageId);
            SetRelativeInt(action, "targetUiRouteId", (int)targetUiRouteId);
            SetRelativeInt(action, "allowedOutcomes", (int)outcomes);
        }

        private static void ValidateRouteAction(
            PlayableStageDefinition route,
            string actionId,
            StageRouteActionKind kind,
            string targetPlayableStageId,
            StageUiRouteId targetUiRouteId,
            StageRouteOutcome outcomes)
        {
            Require(
                route.TryGetTerminalAction(actionId, out StageRouteActionRef action),
                $"Courtyard route is missing action '{actionId}'.");
            Require(action.ActionKind == kind, $"{actionId} action kind is stale.");
            RequireEqual(action.TargetPlayableStageId, targetPlayableStageId, $"{actionId} playable target");
            Require(action.TargetUiRouteId == targetUiRouteId, $"{actionId} UI route target is stale.");
            Require(action.AllowedOutcomes == outcomes, $"{actionId} allowed outcomes are stale.");
        }

        private static void ConfigureLocale(
            SerializedProperty locale,
            string localeId,
            KeyValuePair<string, string>[] values)
        {
            SetRelativeString(locale, "localeId", localeId);
            SerializedProperty entries = RequireRelative(locale, "entries");
            entries.arraySize = values.Length;
            string previous = string.Empty;
            for (int i = 0; i < values.Length; i++)
            {
                KeyValuePair<string, string> value = values[i];
                Require(
                    i == 0 || string.Compare(previous, value.Key, StringComparison.Ordinal) < 0,
                    $"Localization keys for {localeId} must be authored in ordinal order.");
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                SetRelativeString(entry, "key", value.Key);
                SetRelativeString(entry, "value", value.Value);
                previous = value.Key;
            }
        }

        private static KeyValuePair<string, string> Pair(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }

        private static void ConfigureProofRule(
            SerializedProperty rule,
            string proofId,
            string textKey,
            StageResultProofValueFormat valueFormat)
        {
            SetRelativeString(rule, "proofId", proofId);
            SetRelativeString(rule, "textKey", textKey);
            SetRelativeBool(rule, "requireQualified", true);
            SetRelativeInt(rule, "valueFormat", (int)valueFormat);
        }

        private static void ConfigureResultMapping(
            SerializedProperty mapping,
            StageRouteOutcome outcome,
            string actionId,
            string labelKey,
            StageResultActionPresentationRole role,
            int displayOrder)
        {
            SetRelativeInt(mapping, "outcome", (int)outcome);
            SetRelativeString(mapping, "actionId", actionId);
            SetRelativeString(mapping, "labelKey", labelKey);
            SetRelativeInt(mapping, "role", (int)role);
            SetRelativeInt(mapping, "displayOrder", displayOrder);
        }

        private static T GetOrCreateClone<T>(string targetPath, string sourcePath)
            where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(targetPath);
            if (existing != null)
            {
                return existing;
            }

            UnityEngine.Object wrongType = AssetDatabase.LoadMainAssetAtPath(targetPath);
            Require(wrongType == null, $"Target path contains an incompatible asset: {targetPath}.");
            T source = LoadRequired<T>(sourcePath);
            T clone = UnityEngine.Object.Instantiate(source);
            clone.name = Path.GetFileNameWithoutExtension(targetPath);
            clone.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(clone, targetPath);
            return clone;
        }

        private static T LoadRequired<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null, $"Required asset is missing or has the wrong type: {path}.");
            return asset;
        }

        private static SerializedProperty RequireProperty(
            SerializedObject serialized,
            string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null, $"Serialized property '{propertyName}' is missing on {serialized.targetObject.name}.");
            return property;
        }

        private static SerializedProperty RequireRelative(
            SerializedProperty owner,
            string propertyName)
        {
            SerializedProperty property = owner.FindPropertyRelative(propertyName);
            Require(property != null, $"Relative serialized property '{propertyName}' is missing below {owner.propertyPath}.");
            return property;
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            RequireProperty(serialized, propertyName).stringValue = value ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(SerializedObject serialized, string propertyName, int value)
        {
            RequireProperty(serialized, propertyName).intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObject(
            SerializedObject serialized,
            string propertyName,
            UnityEngine.Object value)
        {
            RequireProperty(serialized, propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRelativeString(
            SerializedProperty owner,
            string propertyName,
            string value)
        {
            RequireRelative(owner, propertyName).stringValue = value ?? string.Empty;
        }

        private static void SetRelativeInt(
            SerializedProperty owner,
            string propertyName,
            int value)
        {
            RequireRelative(owner, propertyName).intValue = value;
        }

        private static void SetRelativeBool(
            SerializedProperty owner,
            string propertyName,
            bool value)
        {
            RequireRelative(owner, propertyName).boolValue = value;
        }

        private static void SetRelativeObject(
            SerializedProperty owner,
            string propertyName,
            UnityEngine.Object value)
        {
            RequireRelative(owner, propertyName).objectReferenceValue = value;
        }

        private static string FormatPassMessage(PackAssets pack, string status)
        {
            return "[OlympusCourtyardDrillStagePackSetup] " + status
                + $" playableStageId={pack.Route.PlayableStageId}"
                + $", routeDigest={pack.Route.CanonicalRouteDigest}"
                + $", templateDigest={pack.Template.CanonicalTemplateDigest}"
                + $", resultEvaluationDigest={pack.ResultDefinition.EvaluationContentDigest}"
                + $", nodeBindingDigest={pack.ProgressionNode.BindingDigest}"
                + $", graphDigest={pack.ProgressionGraph.CanonicalDigest}"
                + $", joinDigest={pack.Route.ResultProgressionJoin.CanonicalDigest}";
        }

        private static void RequireEqual(string actual, string expected, string label)
        {
            Require(
                string.Equals(actual, expected, StringComparison.Ordinal),
                $"{label} mismatch. expected='{expected}', actual='{actual}'.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class PackAssets
        {
            public StageDefinitionProfile StageDefinition;
            public PlayableStageDefinition Route;
            public LinearStageTemplateProfile Template;
            public StageResultLocalizationTable Localization;
            public StageResultPresentationProfile PresentationProfile;
            public StageResultPresentationCatalog PresentationCatalog;
            public StageResultDefinition ResultDefinition;
            public StageProgressionNode ProgressionNode;
            public StageProgressionGraph ProgressionGraph;
            public CombatEnemyArchetypeProfile RangedArchetype;

            public UnityEngine.Object[] PersistentAssets => new UnityEngine.Object[]
            {
                StageDefinition,
                Route,
                Template,
                Localization,
                PresentationProfile,
                PresentationCatalog,
                ResultDefinition,
                ProgressionNode,
                ProgressionGraph
            };
        }
    }
}
