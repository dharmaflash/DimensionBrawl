using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DimensionBrawl.AI;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class CanonicalUiRoutePlayModeTests
    {
        private const int LoginRouteId = 10;
        private const int LobbyRouteId = 20;
        private const int StageSelectRouteId = 30;
        private const int CombatRouteId = 40;
        private const string LoginScenePath = "Assets/_Game/Scenes/UI/UI_Login.unity";
        private const string LobbyScenePath = "Assets/_Game/Scenes/UI/UI_Lobby.unity";
        private const string StageSelectScenePath = "Assets/_Game/Scenes/UI/UI_StageSelect.unity";
        private const string CorridorScenePath = "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StationScenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity";
        private const string StageClearScenePath = "Assets/_Game/Scenes/UI/UI_StageClear.unity";
        private const string StageClearSceneName = "UI_StageClear";
        private const string RouteTablePath = "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string StageCatalogPath = "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string StationDefinitionPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_Stage_OlympusStationCombat.asset";
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
        private const string PlayableStagePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";
        private const string StageSelectPrefabPath =
            "Assets/_Game/UI/StageSelect/PF_UI_StageSelectScreen.prefab";
        private const string TrainingCanonicalProjectionDigest =
            "571b79d2fb47619383be714f88870752c4f8e1ce4d2864d6dc846307aecb6f1d";
        private const string ProductBuildManifestDigest =
            "b0f1a128548f8f77aae5a0670586a2ac39c504d967ef722cf9681f56cd788d6b";
        private const string CanonicalTemplateDigest =
            "3eec8a5f94c4dfd47ae9255a49ff3b5961d5130cf386f2c6ba96b0525c502e55";
        private const string CanonicalReferenceDigest =
            "eada6124fe3bed295bddaf3caeb0b53ff1510a2f790c2b76b8454410834a21ea";
        private const string CanonicalBriefingDigest =
            "e334d6bb63dc42d921e6d85bcca42cc628064d0d1b8fee1cc303d4ca223fab70";
        private const string ResultPresentationCatalogPath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageResults/DB_StageResultPresentationCatalog.asset";

        private static readonly string[] ExpectedBuildScenePaths =
        {
            LoginScenePath,
            LobbyScenePath,
            StageSelectScenePath,
            CorridorScenePath,
            StageClearScenePath
        };

        [TearDown]
        public void ReleaseUiSceneServiceOwnership()
        {
            StageRunRuntime.ResetForTests();
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UISceneTransitionArrivalReceiver");
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UISceneTransitionHandoffOwner");
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UITransitionHandoffService");
            Time.timeScale = 1f;
            EventSystem[] eventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < eventSystems.Length; i++)
            {
                if (eventSystems[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(eventSystems[i].gameObject);
                }
            }

            AudioListener[] listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] != null && listeners[i].isActiveAndEnabled)
                {
                    return;
                }
            }

            _ = new GameObject(
                "Canonical UI Route Test Camera",
                typeof(Camera),
                typeof(AudioListener));
        }

        private static void TryResetUiTransitionRuntime(string typeName)
        {
            Type type = Type.GetType(typeName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(typeName + ", Assembly-CSharp");
            MethodInfo reset = type?.GetMethod(
                "ResetForTests",
                BindingFlags.Public | BindingFlags.Static);
            reset?.Invoke(null, null);
        }

        [Test]
        public void BuildSettingsAndManifestContainTheCanonicalProductRouteInOrder()
        {
            var enabledScenePaths = new List<string>();
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                {
                    enabledScenePaths.Add(scenes[i].path.Replace('\\', '/'));
                }
            }

            CollectionAssert.AreEqual(ExpectedBuildScenePaths, enabledScenePaths);

            Assert.That(
                TryCreateProductBuildManifest(
                    LoadRequired<ScriptableObject>(RouteTablePath),
                    LoadRequired<ScriptableObject>(StageCatalogPath),
                    StageClearScenePath,
                    out object manifest,
                    out string rejectReason,
                    out string error),
                Is.True,
                $"{rejectReason}: {error}");
            Assert.That(Convert.ToInt32(ReadProperty(manifest, "CatalogEntryCount")),
                Is.EqualTo(1));
            Assert.That(Convert.ToInt32(ReadProperty(manifest, "RouteSegmentCount")),
                Is.EqualTo(2));
            Assert.That(Convert.ToInt32(ReadProperty(manifest, "SceneCount")),
                Is.EqualTo(ExpectedBuildScenePaths.Length));
            Assert.That(ReadProperty(manifest, "CanonicalDigest"),
                Is.EqualTo(ProductBuildManifestDigest));
        }

        [Test]
        public void RouteTableAndStageCatalogResolveAuthoredProductScenes()
        {
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            AssertSerializedRoute(routeTable, LoginRouteId, "UI_Login", LoginScenePath);
            AssertSerializedRoute(routeTable, LobbyRouteId, "UI_Lobby", LobbyScenePath);
            AssertSerializedRoute(routeTable, StageSelectRouteId, "UI_StageSelect", StageSelectScenePath);
            AssertSerializedRoute(
                routeTable,
                CombatRouteId,
                "OlympusCorridorInvasionStage",
                CorridorScenePath);

            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject stageCatalog = LoadRequired<ScriptableObject>(StageCatalogPath);
            Assert.That(catalogType.IsInstanceOfType(stageCatalog), Is.True);
            Assert.That(
                Convert.ToInt32(ReadProperty(stageCatalog, "ProjectionSchemaVersion")),
                Is.EqualTo(1));
            Assert.That(
                Convert.ToInt32(ReadProperty(stageCatalog, "CatalogProjectionGeneration")),
                Is.EqualTo(2));
            Assert.That(Convert.ToInt32(ReadProperty(stageCatalog, "StageCount")), Is.EqualTo(1));
            object stage = RequireMethod(catalogType, "GetStage").Invoke(stageCatalog, new object[] { 0 });
            Assert.That(ReadProperty(stage, "Id"), Is.EqualTo("story_v1_training_route"));
            Assert.That(
                ReadProperty(stage, "PresentationProvenance").ToString(),
                Is.EqualTo("LegacyPresentationOnly"));
            Assert.That(ReadProperty(stage, "MockRewardPreview"), Is.Empty);
            Assert.That(
                ReadProperty(stage, "CanonicalProjectionDigest"),
                Is.EqualTo(TrainingCanonicalProjectionDigest));
            PlayableStageDefinition playableStage =
                (PlayableStageDefinition)ReadProperty(stage, "PlayableStage");
            Assert.That(playableStage, Is.Not.Null);
            Assert.That(playableStage.PlayableStageId, Is.EqualTo("OLYMPUS-INVASION-01"));
            object[] projectionArguments = { 0, ResolveUiRouteId(CombatRouteId), null, null };
            Assert.That(
                (bool)RequireMethod(
                    catalogType,
                    "TryCreateRouteProjection",
                    4,
                    typeof(int)).Invoke(stageCatalog, projectionArguments),
                Is.True,
                projectionArguments[3].ToString());
            object projection = projectionArguments[2];
            Assert.That(ReadProperty(projection, "PlayableStage"), Is.SameAs(playableStage));
            Assert.That(
                ReadProperty(projection, "EntryStageDefinition"),
                Is.SameAs(playableStage.GetSceneSegment(0).StageDefinition));
            Assert.That(
                Convert.ToInt32(ReadProperty(projection, "RouteRevision")),
                Is.EqualTo(playableStage.RouteRevision));
            Assert.That(
                ReadProperty(projection, "StoredCanonicalRouteDigest"),
                Is.EqualTo(playableStage.CanonicalRouteDigest));
            Assert.That(
                ReadProperty(projection, "RecomputedCanonicalRouteDigest"),
                Is.EqualTo(playableStage.ComputeCanonicalRouteDigest()));
            Assert.That(
                ReadProperty(projection, "CanonicalProjectionDigest"),
                Is.EqualTo(TrainingCanonicalProjectionDigest));
            Assert.That(
                ReadProperty(projection, "StageTemplate"),
                Is.SameAs(playableStage.ReferenceBlock.StageTemplate));
            Assert.That(
                ReadProperty(projection, "CanonicalReferenceDigest"),
                Is.EqualTo(CanonicalReferenceDigest));
            Assert.That(
                ReadProperty(projection, "CanonicalTemplateDigest"),
                Is.EqualTo(CanonicalTemplateDigest));
            Assert.That(
                ReadProperty(projection, "CanonicalBriefingDigest"),
                Is.EqualTo(CanonicalBriefingDigest));
            StageBriefingReadModel briefing =
                (StageBriefingReadModel)ReadProperty(projection, "Briefing");
            Assert.That(briefing, Is.Not.Null);
            Assert.That(briefing.CanonicalBriefingDigest, Is.EqualTo(CanonicalBriefingDigest));
            Assert.That(briefing.Title, Is.EqualTo("기억의 회랑"));
            Assert.That(
                briefing.Objective,
                Is.EqualTo("하층 세계에서 발생한 차원의 미세한 균열.\n그 징후의 진원지를 조사하라."));
            Assert.That(
                briefing.CombatLesson,
                Is.EqualTo("회랑에서 근접 공격, 이동, 원거리 전환과 사격, 회피, 표적 정리를 차례로 익힌다. 정거장에서는 레플리카 지급과 소환 안내를 확인한 뒤 보스 격파를 목표로 한다."));
            Assert.That(ReadProperty(projection, "ThreatTags"), Is.Empty);
            Assert.That(ReadProperty(projection, "RecommendedSummonRole"), Is.Empty);
            Assert.That(ReadProperty(projection, "RewardPreview"), Is.Empty);
            Assert.That(
                Convert.ToInt32(ReadProperty(projection, "UiRouteId")),
                Is.EqualTo(CombatRouteId));
            Assert.That(Convert.ToInt32(ReadProperty(projection, "EntrySequenceIndex")), Is.Zero);
            string entryScenePath = (string)ReadProperty(projection, "EntryScenePath");
            Assert.That(entryScenePath, Is.EqualTo(CorridorScenePath));
            Assert.That(
                ReadProperty(projection, "EntrySceneName"),
                Is.EqualTo("OlympusCorridorInvasionStage"));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(entryScenePath));

            object[] retiredStageArguments = { "story_v1_retry_route", null };
            Assert.That(
                (bool)RequireMethod(catalogType, "TryGetStage").Invoke(
                    stageCatalog,
                    retiredStageArguments),
                Is.False);
            Assert.That(ReadProperty(retiredStageArguments[1], "Id"), Is.Null);
        }

        [UnityTest]
        public IEnumerator SelectedStageStartUsesTheSelectedCatalogDefinition()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            StageDefinitionProfile secondaryDefinition = CreateStageDefinition(
                "SECONDARY-SEGMENT",
                StageClearScenePath);
            PlayableStageDefinition secondaryRoute = CreatePlayableStageDefinition(
                "SECONDARY-ROUTE",
                "secondary-entry",
                secondaryDefinition);
            ScriptableObject stageCatalog =
                CreateStageCatalog(catalogType, "secondary", secondaryRoute);
            AudioClip startClip = AudioClip.Create("AcceptedStageStart", 64, 1, 44100, false);
            var root = new GameObject("Selected Stage Route Test");
            root.SetActive(false);

            try
            {
                Component router = root.AddComponent(routerType);
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(router, "routeTable", routeTable);
                SetPrivateField(presenter, "stageCatalog", stageCatalog);
                SetPrivateField(presenter, "selectedStageId", "secondary");
                SetPrivateField(presenter, "router", router);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "startButtonSfx", startClip);
                UnityEvent startRequested = ReadPrivateField<UnityEvent>(presenter, "startRequested");
                int requestCount = 0;
                startRequested.AddListener(() => requestCount++);
                root.SetActive(true);

                Assert.That(root.GetComponent<AudioSource>(), Is.Null);
                LogAssert.Expect(LogType.Warning, "UI scene route loader is not configured.");
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;

                object projection = ReadProperty(presenter, "SelectedRouteProjection");
                Assert.That(projection, Is.Not.Null);
                Assert.That(ReadProperty(projection, "PlayableStage"), Is.SameAs(secondaryRoute));
                Assert.That(
                    ReadProperty(projection, "EntryScenePath"),
                    Is.EqualTo(StageClearScenePath));
                Assert.That(requestCount, Is.EqualTo(1));
                Assert.That(root.GetComponent<AudioSource>(), Is.Not.Null);
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;
                Assert.That(requestCount, Is.EqualTo(1));
                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasAcceptedStartRequest")),
                    Is.True);
                object state = RequireProperty(routerType, "CurrentState").GetValue(router);
                Assert.AreEqual(CombatRouteId, Convert.ToInt32(ReadProperty(state, "RouteId")));
                Assert.AreEqual(StageClearSceneName, ReadProperty(state, "SceneName"));
                Assert.AreEqual("Failed", ReadProperty(state, "Phase").ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(stageCatalog);
                DestroyPlayableStageDefinition(secondaryRoute);
                UnityEngine.Object.DestroyImmediate(secondaryDefinition);
                UnityEngine.Object.DestroyImmediate(startClip);
            }
        }

        [Test]
        public void MultiEntryCatalogProjectsEveryValidRowAcrossPublicProjectionPaths()
        {
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            StageDefinitionProfile definitionA = CreateStageDefinition(
                "MULTI-ROW-A-SEGMENT",
                CorridorScenePath);
            StageDefinitionProfile definitionB = CreateStageDefinition(
                "MULTI-ROW-B-SEGMENT",
                StageClearScenePath);
            PlayableStageDefinition routeA = CreatePlayableStageDefinition(
                "MULTI-ROW-A-ROUTE",
                "multi-row-a-entry",
                definitionA);
            PlayableStageDefinition routeB = CreatePlayableStageDefinition(
                "MULTI-ROW-B-ROUTE",
                "multi-row-b-entry",
                definitionB);
            ScriptableObject catalog = CreateStageCatalog(
                catalogType,
                ("multi-row-a", routeA),
                ("multi-row-b", routeB));

            try
            {
                Assert.That(Convert.ToInt32(ReadProperty(catalog, "StageCount")), Is.EqualTo(2));
                object combatRouteId = ResolveUiRouteId(CombatRouteId);
                MethodInfo createNamed = RequireMethod(
                    catalogType,
                    "TryCreateRouteProjection",
                    4,
                    typeof(string));
                MethodInfo createIndexed = RequireMethod(
                    catalogType,
                    "TryCreateRouteProjection",
                    4,
                    typeof(int));
                MethodInfo computeDigest = RequireMethod(
                    catalogType,
                    "TryComputeCanonicalProjectionDigest");
                MethodInfo isCurrent = RequireMethod(catalogType, "IsProjectionCurrent");

                string[] ids = { "multi-row-a", "multi-row-b" };
                PlayableStageDefinition[] routes = { routeA, routeB };
                string[] scenePaths = { CorridorScenePath, StageClearScenePath };
                string[] sceneNames = { "OlympusCorridorInvasionStage", StageClearSceneName };
                for (int i = 0; i < ids.Length; i++)
                {
                    object[] namedArguments = { ids[i], combatRouteId, null, null };
                    Assert.That(
                        (bool)createNamed.Invoke(catalog, namedArguments),
                        Is.True,
                        namedArguments[3]?.ToString());
                    AssertCatalogProjectionTargets(
                        namedArguments[2],
                        ids[i],
                        routes[i],
                        scenePaths[i],
                        sceneNames[i]);

                    object[] indexedArguments = { i, combatRouteId, null, null };
                    Assert.That(
                        (bool)createIndexed.Invoke(catalog, indexedArguments),
                        Is.True,
                        indexedArguments[3]?.ToString());
                    AssertCatalogProjectionTargets(
                        indexedArguments[2],
                        ids[i],
                        routes[i],
                        scenePaths[i],
                        sceneNames[i]);

                    object[] digestArguments = { i, combatRouteId, null, null };
                    Assert.That(
                        (bool)computeDigest.Invoke(catalog, digestArguments),
                        Is.True,
                        digestArguments[3]?.ToString());
                    object stageEntry = RequireMethod(catalogType, "GetStage").Invoke(
                        catalog,
                        new object[] { i });
                    Assert.That(
                        digestArguments[2],
                        Is.EqualTo(ReadProperty(stageEntry, "CanonicalProjectionDigest")));
                    Assert.That(
                        digestArguments[2],
                        Is.EqualTo(ReadProperty(namedArguments[2], "CanonicalProjectionDigest")));

                    object[] currentArguments = { namedArguments[2], combatRouteId, null };
                    Assert.That(
                        (bool)isCurrent.Invoke(catalog, currentArguments),
                        Is.True,
                        currentArguments[2]?.ToString());

                    object[] getStageArguments = { ids[i], null };
                    Assert.That(
                        (bool)RequireMethod(catalogType, "TryGetStage").Invoke(
                            catalog,
                            getStageArguments),
                        Is.True);
                    Assert.That(ReadProperty(getStageArguments[1], "PlayableStage"), Is.SameAs(routes[i]));
                }

                object[] firstProjectionArguments = { combatRouteId, null, null };
                Assert.That(
                    (bool)RequireMethod(catalogType, "TryCreateFirstRouteProjection").Invoke(
                        catalog,
                        firstProjectionArguments),
                    Is.True,
                    firstProjectionArguments[2]?.ToString());
                AssertCatalogProjectionTargets(
                    firstProjectionArguments[1],
                    "multi-row-a",
                    routeA,
                    CorridorScenePath,
                    "OlympusCorridorInvasionStage");

                object[] firstStageArguments = { null };
                Assert.That(
                    (bool)RequireMethod(catalogType, "TryGetFirstStage").Invoke(
                        catalog,
                        firstStageArguments),
                    Is.True);
                Assert.That(ReadProperty(firstStageArguments[0], "Id"), Is.EqualTo("multi-row-a"));
                Assert.That(ReadProperty(firstStageArguments[0], "PlayableStage"), Is.SameAs(routeA));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(routeB);
                DestroyPlayableStageDefinition(routeA);
                UnityEngine.Object.DestroyImmediate(definitionB);
                UnityEngine.Object.DestroyImmediate(definitionA);
            }
        }

        [Test]
        public void InvalidIdentityAnywhereFailClosesEveryCatalogLookupAndProjectionPath()
        {
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            StageDefinitionProfile definitionA = CreateStageDefinition(
                "IDENTITY-A-SEGMENT",
                CorridorScenePath);
            StageDefinitionProfile definitionB = CreateStageDefinition(
                "IDENTITY-B-SEGMENT",
                StageClearScenePath);
            PlayableStageDefinition routeA = CreatePlayableStageDefinition(
                "IDENTITY-A-ROUTE",
                "identity-a-entry",
                definitionA);
            PlayableStageDefinition routeB = CreatePlayableStageDefinition(
                "IDENTITY-B-ROUTE",
                "identity-b-entry",
                definitionB);
            var catalogs = new List<ScriptableObject>();

            try
            {
                catalogs.Add(CreateUnsealedStageCatalog(
                    catalogType,
                    (string.Empty, routeA),
                    ("B", routeB)));
                catalogs.Add(CreateUnsealedStageCatalog(
                    catalogType,
                    ("A", routeA),
                    (" ", routeB)));
                catalogs.Add(CreateUnsealedStageCatalog(
                    catalogType,
                    ("A", routeA),
                    ("B", routeB),
                    (string.Empty, routeA)));
                catalogs.Add(CreateUnsealedStageCatalog(
                    catalogType,
                    ("A", routeA),
                    ("B", routeB),
                    ("B", routeA)));

                for (int i = 0; i < catalogs.Count; i++)
                {
                    string expectedReason = i < 3
                        ? "MissingCatalogEntryId"
                        : "DuplicateCatalogEntryId";
                    AssertCatalogIdentityFailureAcrossPublicPaths(
                        catalogType,
                        catalogs[i],
                        expectedReason);
                }
            }
            finally
            {
                for (int i = catalogs.Count - 1; i >= 0; i--)
                {
                    UnityEngine.Object.DestroyImmediate(catalogs[i]);
                }

                DestroyPlayableStageDefinition(routeB);
                DestroyPlayableStageDefinition(routeA);
                UnityEngine.Object.DestroyImmediate(definitionB);
                UnityEngine.Object.DestroyImmediate(definitionA);
            }
        }

        [Test]
        public void ProductBuildManifestPreservesAuthoredEvidenceOrderAndDedupesPhysicalScenes()
        {
            ScriptableObject productionCatalog = LoadRequired<ScriptableObject>(StageCatalogPath);
            PlayableStageDefinition productionRoute = LoadRequired<PlayableStageDefinition>(
                PlayableStagePath);
            ProductionOlympusIdentityGuard productionGuard =
                ProductionOlympusIdentityGuard.Capture(productionCatalog, productionRoute);
            ScriptableObject dynamicRouteTable = null;
            IndependentManifestRouteFixture synthetic = null;
            ScriptableObject catalogWithAdditionalRow = null;

            try
            {
                dynamicRouteTable = CreateDynamicRouteTable(
                    LoadRequired<ScriptableObject>(RouteTablePath));
                synthetic = CreateIndependentManifestRouteFixture(
                    productionRoute,
                    "deterministic",
                    "B0-4-MANIFEST-DETERMINISTIC-01",
                    "b0-4.manifest.deterministic.result",
                    "b0-4.manifest.deterministic.node",
                    StageSelectScenePath);
                int productionCatalogCount = Convert.ToInt32(
                    ReadProperty(productionCatalog, "StageCount"));
                int syntheticCatalogIndex = productionCatalogCount;
                catalogWithAdditionalRow = CreateCatalogWithIndependentAdditionalRow(
                    productionCatalog,
                    "b0-4-manifest-deterministic",
                    synthetic.Route);

                Assert.That(productionCatalogCount, Is.EqualTo(1));
                Assert.That(
                    Convert.ToInt32(ReadProperty(catalogWithAdditionalRow, "StageCount")),
                    Is.EqualTo(productionCatalogCount + 1));
                for (int productionCatalogIndex = 0;
                    productionCatalogIndex < productionCatalogCount;
                    productionCatalogIndex++)
                {
                    object sourceEntry = GetCatalogStage(
                        productionCatalog,
                        productionCatalogIndex);
                    object clonedEntry = GetCatalogStage(
                        catalogWithAdditionalRow,
                        productionCatalogIndex);
                    AssertCatalogEntriesEquivalent(sourceEntry, clonedEntry);
                }

                Assert.That(
                    TryCreateCatalogProjection(
                        catalogWithAdditionalRow,
                        syntheticCatalogIndex,
                        out object syntheticProjection,
                        out string projectionRejectReason),
                    Is.True,
                    projectionRejectReason);
                Assert.That(ReadProperty(syntheticProjection, "PlayableStage"),
                    Is.SameAs(synthetic.Route));
                Assert.That(ReadProperty(syntheticProjection, "EntryScenePath"),
                    Is.EqualTo(StageSelectScenePath));

                Assert.That(
                    TryCreateProductBuildManifest(
                        dynamicRouteTable,
                        catalogWithAdditionalRow,
                        StageClearScenePath,
                        out object manifest,
                        out string rejectReason,
                        out string error),
                    Is.True,
                    $"{rejectReason}: {error}");
                Assert.That(
                    TryCreateProductBuildManifest(
                        dynamicRouteTable,
                        catalogWithAdditionalRow,
                        StageClearScenePath,
                        out object repeatedManifest,
                        out string repeatedRejectReason,
                        out string repeatedError),
                    Is.True,
                    $"{repeatedRejectReason}: {repeatedError}");

                int routeCount = Convert.ToInt32(ReadProperty(dynamicRouteTable, "RouteCount"));
                Assert.That(Convert.ToInt32(ReadProperty(manifest, "UiRouteCount")),
                    Is.EqualTo(routeCount));
                for (int routeIndex = 0;
                    routeIndex < routeCount;
                    routeIndex++)
                {
                    object authored = GetIndexedValue(dynamicRouteTable, "GetRoute", routeIndex);
                    object projected = GetIndexedValue(manifest, "GetUiRoute", routeIndex);
                    Assert.That(Convert.ToInt32(ReadProperty(projected, "AuthoredIndex")),
                        Is.EqualTo(routeIndex));
                    Assert.That(ReadProperty(projected, "RouteId"),
                        Is.EqualTo(ReadProperty(authored, "RouteId")));
                    Assert.That(ReadProperty(projected, "ScenePath"),
                        Is.EqualTo(ReadProperty(authored, "ScenePath")));
                }

                Assert.That(productionRoute.SceneSegmentCount, Is.EqualTo(2));
                Assert.That(productionRoute.GetSceneSegment(0).StageDefinition.MapScenePath,
                    Is.EqualTo(CorridorScenePath));
                Assert.That(productionRoute.GetSceneSegment(1).StageDefinition.MapScenePath,
                    Is.EqualTo(CorridorScenePath));
                Assert.That(Convert.ToInt32(ReadProperty(manifest, "CatalogEntryCount")),
                    Is.EqualTo(productionCatalogCount + 1));
                int productRouteSegmentCount = 0;
                for (int catalogIndex = 0;
                    catalogIndex < productionCatalogCount;
                    catalogIndex++)
                {
                    object catalogEntry = GetCatalogStage(productionCatalog, catalogIndex);
                    PlayableStageDefinition authoredRoute =
                        (PlayableStageDefinition)ReadProperty(catalogEntry, "PlayableStage");
                    Assert.That(authoredRoute, Is.Not.Null);
                    for (int segmentIndex = 0;
                        segmentIndex < authoredRoute.SceneSegmentCount;
                        segmentIndex++)
                    {
                        object evidence = GetIndexedValue(
                            manifest,
                            "GetRouteSegment",
                            productRouteSegmentCount);
                        StageSceneSegmentRef authored = authoredRoute.GetSceneSegment(segmentIndex);
                        Assert.That(Convert.ToInt32(ReadProperty(evidence, "CatalogIndex")),
                            Is.EqualTo(catalogIndex));
                        Assert.That(Convert.ToInt32(ReadProperty(evidence, "SegmentIndex")),
                            Is.EqualTo(segmentIndex));
                        Assert.That(ReadProperty(evidence, "CatalogEntryId"),
                            Is.EqualTo(ReadProperty(catalogEntry, "Id")));
                        Assert.That(ReadProperty(evidence, "PlayableStageId"),
                            Is.EqualTo(authoredRoute.PlayableStageId));
                        Assert.That(ReadProperty(evidence, "SegmentId"),
                            Is.EqualTo(authored.SegmentId));
                        Assert.That(ReadProperty(evidence, "ScenePath"),
                            Is.EqualTo(authored.StageDefinition.MapScenePath));
                        productRouteSegmentCount++;
                    }
                }

                Assert.That(productRouteSegmentCount, Is.EqualTo(2));
                Assert.That(Convert.ToInt32(ReadProperty(manifest, "RouteSegmentCount")),
                    Is.EqualTo(productRouteSegmentCount + 1));
                object syntheticEvidence = GetIndexedValue(
                    manifest,
                    "GetRouteSegment",
                    productRouteSegmentCount);
                Assert.That(Convert.ToInt32(ReadProperty(syntheticEvidence, "CatalogIndex")),
                    Is.EqualTo(syntheticCatalogIndex));
                Assert.That(Convert.ToInt32(ReadProperty(syntheticEvidence, "SegmentIndex")),
                    Is.Zero);
                Assert.That(ReadProperty(syntheticEvidence, "CatalogEntryId"),
                    Is.EqualTo("b0-4-manifest-deterministic"));
                Assert.That(ReadProperty(syntheticEvidence, "PlayableStageId"),
                    Is.EqualTo(synthetic.Route.PlayableStageId));
                Assert.That(ReadProperty(syntheticEvidence, "ResultDefinitionId"),
                    Is.EqualTo(synthetic.ResultDefinition.ResultDefinitionId));
                Assert.That(ReadProperty(syntheticEvidence, "ProgressionNodeId"),
                    Is.EqualTo(synthetic.ProgressionNode.ProgressionNodeId));
                Assert.That(ReadProperty(syntheticEvidence, "ScenePath"),
                    Is.EqualTo(StageSelectScenePath));

                Assert.That(Convert.ToInt32(ReadProperty(manifest, "SceneCount")),
                    Is.EqualTo(ExpectedBuildScenePaths.Length));
                for (int sceneIndex = 0;
                    sceneIndex < ExpectedBuildScenePaths.Length;
                    sceneIndex++)
                {
                    object scene = GetIndexedValue(manifest, "GetScene", sceneIndex);
                    Assert.That(Convert.ToInt32(ReadProperty(scene, "BuildIndex")),
                        Is.EqualTo(sceneIndex));
                    Assert.That(ReadProperty(scene, "ScenePath"),
                        Is.EqualTo(ExpectedBuildScenePaths[sceneIndex]));
                }

                Assert.That(
                    CountManifestPhysicalScene(manifest, CorridorScenePath),
                    Is.EqualTo(1));
                Assert.That(
                    CountManifestPhysicalScene(manifest, StageSelectScenePath),
                    Is.EqualTo(1));
                Assert.That(
                    ReadProperty(
                        GetIndexedValue(
                            manifest,
                            "GetScene",
                            Convert.ToInt32(ReadProperty(manifest, "SceneCount")) - 1),
                        "SourceKind").ToString(),
                    Is.EqualTo("StageClear"));
                Assert.That(
                    ReadProperty(
                        GetIndexedValue(
                            manifest,
                            "GetScene",
                            Convert.ToInt32(ReadProperty(manifest, "SceneCount")) - 1),
                        "ScenePath"),
                    Is.EqualTo(StageClearScenePath));
                Assert.That(ReadProperty(manifest, "CanonicalDigest"),
                    Does.Match("^[0-9a-f]{64}$"));
                Assert.That(ReadProperty(repeatedManifest, "CanonicalDigest"),
                    Is.EqualTo(ReadProperty(manifest, "CanonicalDigest")));
                Assert.That(ReadProperty(repeatedManifest, "SceneCount"),
                    Is.EqualTo(ReadProperty(manifest, "SceneCount")));
                Assert.That(
                    ReadProperty(repeatedManifest, "RouteSegmentCount"),
                    Is.EqualTo(ReadProperty(manifest, "RouteSegmentCount")));
            }
            finally
            {
                if (catalogWithAdditionalRow != null)
                {
                    UnityEngine.Object.DestroyImmediate(catalogWithAdditionalRow);
                }

                synthetic?.Destroy();
                if (dynamicRouteTable != null)
                {
                    UnityEngine.Object.DestroyImmediate(dynamicRouteTable);
                }

                productionGuard.AssertUnchanged();
            }
        }

        [Test]
        public void ProductBuildManifestRejectsDuplicateStrictSidecarIdentitiesWithoutPartialResult()
        {
            ScriptableObject productionCatalog = LoadRequired<ScriptableObject>(StageCatalogPath);
            PlayableStageDefinition productionRoute = LoadRequired<PlayableStageDefinition>(
                PlayableStagePath);
            StageResultProgressionJoinBlock productionJoin =
                productionRoute.ResultProgressionJoin;
            ProductionOlympusIdentityGuard productionGuard =
                ProductionOlympusIdentityGuard.Capture(productionCatalog, productionRoute);
            ScriptableObject dynamicRouteTable = CreateDynamicRouteTable(
                LoadRequired<ScriptableObject>(RouteTablePath));

            try
            {
                AssertManifestRejectsDuplicateIndependentIdentity(
                    dynamicRouteTable,
                    productionCatalog,
                    productionRoute,
                    "duplicate-playable-stage",
                    productionRoute.PlayableStageId,
                    "b0-4.manifest.duplicate-playable.result",
                    "b0-4.manifest.duplicate-playable.node",
                    "DuplicatePlayableStageId");
                AssertManifestRejectsDuplicateIndependentIdentity(
                    dynamicRouteTable,
                    productionCatalog,
                    productionRoute,
                    "duplicate-result-definition",
                    "B0-4-MANIFEST-DUPLICATE-RESULT-01",
                    productionJoin.ResultDefinition.ResultDefinitionId,
                    "b0-4.manifest.duplicate-result.node",
                    "DuplicateResultDefinitionId");
                AssertManifestRejectsDuplicateIndependentIdentity(
                    dynamicRouteTable,
                    productionCatalog,
                    productionRoute,
                    "duplicate-progression-node",
                    "B0-4-MANIFEST-DUPLICATE-NODE-01",
                    "b0-4.manifest.duplicate-node.result",
                    productionJoin.ProgressionNode.ProgressionNodeId,
                    "DuplicateProgressionNodeId");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(dynamicRouteTable);
                productionGuard.AssertUnchanged();
            }
        }

        [Test]
        public void CatalogProjectionRejectsCanonicalDigestMismatch()
        {
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            StageDefinitionProfile definition = CreateStageDefinition(
                "DIGEST-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "DIGEST-ROUTE",
                "digest-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(catalogType, "digest", route);

            try
            {
                SetPrivateField(route, "canonicalRouteDigest", "stale-digest");
                object[] projectionArguments =
                    { 0, ResolveUiRouteId(CombatRouteId), null, null };
                Assert.That(
                    (bool)RequireMethod(
                        catalogType,
                        "TryCreateRouteProjection",
                        4,
                        typeof(int)).Invoke(catalog, projectionArguments),
                    Is.False);
                Assert.That(projectionArguments[2], Is.Null);
                Assert.That(
                    projectionArguments[3].ToString(),
                    Is.EqualTo("CanonicalRouteDigestMismatch"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void CatalogProjectionRejectsStoredProjectionDigestMismatch()
        {
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            StageDefinitionProfile definition = CreateStageDefinition(
                "PROJECTION-DIGEST-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "PROJECTION-DIGEST-ROUTE",
                "projection-digest-entry",
                definition);
            ScriptableObject catalog =
                CreateStageCatalog(catalogType, "projection-digest", route);

            try
            {
                var serializedCatalog = new SerializedObject(catalog);
                serializedCatalog.FindProperty("stages")
                    .GetArrayElementAtIndex(0)
                    .FindPropertyRelative("canonicalProjectionDigest")
                    .stringValue = "stale-projection-digest";
                serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

                object[] projectionArguments =
                    { 0, ResolveUiRouteId(CombatRouteId), null, null };
                Assert.That(
                    (bool)RequireMethod(
                        catalogType,
                        "TryCreateRouteProjection",
                        4,
                        typeof(int)).Invoke(catalog, projectionArguments),
                    Is.False);
                Assert.That(projectionArguments[2], Is.Null);
                Assert.That(
                    projectionArguments[3].ToString(),
                    Is.EqualTo("CanonicalProjectionDigestMismatch"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void CatalogProjectionRejectsTruthfulJoinMutationMatrix()
        {
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            StageDefinitionProfile definition = CreateStageDefinition(
                "TRUTHFUL-JOIN-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "TRUTHFUL-JOIN-ROUTE",
                "truthful-join-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(
                catalogType,
                "truthful-join",
                route);
            StageReferenceBlock reference = route.ReferenceBlock;
            LinearStageTemplateProfile template = reference.StageTemplate;

            try
            {
                AssertProjectionAccepted(catalogType, catalog);

                SetPrivateField(reference, "enabled", false);
                AssertProjectionRejected(
                    catalogType,
                    catalog,
                    "MissingStageReferenceBlock");
                SetPrivateField(reference, "enabled", true);

                string templateDigest = template.CanonicalTemplateDigest;
                SetPrivateField(template, "canonicalTemplateDigest", "stale-template-digest");
                AssertProjectionRejected(
                    catalogType,
                    catalog,
                    "CanonicalStageTemplateDigestMismatch");
                SetPrivateField(template, "canonicalTemplateDigest", templateDigest);

                string referenceDigest = reference.CanonicalReferenceDigest;
                SetPrivateField(reference, "canonicalReferenceDigest", "stale-reference-digest");
                AssertProjectionRejected(
                    catalogType,
                    catalog,
                    "CanonicalStageReferenceDigestMismatch");
                SetPrivateField(reference, "canonicalReferenceDigest", referenceDigest);

                string briefingDigest = reference.CanonicalBriefingDigest;
                SetPrivateField(reference, "canonicalBriefingDigest", "stale-briefing-digest");
                AssertProjectionRejected(
                    catalogType,
                    catalog,
                    "CanonicalStageBriefingDigestMismatch");
                SetPrivateField(reference, "canonicalBriefingDigest", briefingDigest);

                SetPrivateField(reference, "activeRunRestartPolicyDigest", "unexpected-policy");
                AssertProjectionRejected(
                    catalogType,
                    catalog,
                    "InvalidActiveRunRestartPolicy");
                SetPrivateField(reference, "activeRunRestartPolicyDigest", string.Empty);

                SetPrivateField(
                    reference,
                    "activeRunRestartPolicyDisposition",
                    StageBriefingValueDisposition.Present);
                AssertProjectionRejected(
                    catalogType,
                    catalog,
                    "InvalidActiveRunRestartPolicy");
                SetPrivateField(
                    reference,
                    "activeRunRestartPolicyDisposition",
                    StageBriefingValueDisposition.NotAdmittedByCurrentSchema);

                AssertProjectionAccepted(catalogType, catalog);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void TruthfulJoinRejectsSegmentAndPocketPermutation()
        {
            PlayableStageDefinition source = LoadRequired<PlayableStageDefinition>(PlayableStagePath);
            for (int permutation = 0; permutation < 2; permutation++)
            {
                PlayableStageDefinition route = UnityEngine.Object.Instantiate(source);
                LinearStageTemplateProfile template =
                    UnityEngine.Object.Instantiate(source.ReferenceBlock.StageTemplate);
                route.hideFlags = HideFlags.HideAndDontSave;
                template.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(route.ReferenceBlock, "stageTemplate", template);

                try
                {
                    var serializedTemplate = new SerializedObject(template);
                    SerializedProperty segments =
                        serializedTemplate.FindProperty("canonicalRouteSegments");
                    if (permutation == 0)
                    {
                        segments.MoveArrayElement(0, 1);
                    }
                    else
                    {
                        segments.GetArrayElementAtIndex(1)
                            .FindPropertyRelative("pockets")
                            .MoveArrayElement(0, 1);
                    }

                    serializedTemplate.ApplyModifiedPropertiesWithoutUndo();
                    serializedTemplate.Update();
                    serializedTemplate.FindProperty("canonicalTemplateDigest").stringValue =
                        template.ComputeCanonicalTemplateDigest();
                    serializedTemplate.ApplyModifiedPropertiesWithoutUndo();

                    Assert.That(
                        route.TryCreateBriefingReadModel(
                            out StageBriefingReadModel briefing,
                            out StageBriefingBuildRejectReason rejectReason),
                        Is.False,
                        permutation == 0 ? "segment permutation" : "pocket permutation");
                    Assert.That(briefing, Is.Null);
                    Assert.That(
                        rejectReason,
                        Is.EqualTo(StageBriefingBuildRejectReason.RouteTemplateMismatch));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(route);
                    UnityEngine.Object.DestroyImmediate(template);
                }
            }
        }

        [Test]
        public void StageSelectPrefabPreservesAuthoredBriefingAndHiddenRewardRows()
        {
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            GameObject prefab = LoadRequired<GameObject>(StageSelectPrefabPath);
            Component presenter = prefab.GetComponent(presenterType);
            Assert.That(presenter, Is.Not.Null);

            var serializedPresenter = new SerializedObject(presenter);
            Text combatLessonText = serializedPresenter.FindProperty("combatLessonText")
                ?.objectReferenceValue as Text;
            Assert.That(combatLessonText, Is.Not.Null);
            Assert.That(combatLessonText.gameObject.name, Is.EqualTo("CurrentChapterLessonText"));
            Assert.That(combatLessonText.text, Is.Empty);
            Assert.That(combatLessonText.gameObject.activeSelf, Is.True);
            Text rewardPreviewText = serializedPresenter.FindProperty("rewardPreviewText")
                ?.objectReferenceValue as Text;
            Assert.That(rewardPreviewText, Is.Not.Null);
            Assert.That(rewardPreviewText.gameObject.name, Is.EqualTo("CurrentChapterRewardText"));
            Assert.That(rewardPreviewText.text, Is.Empty);
            Assert.That(rewardPreviewText.gameObject.activeSelf, Is.False);
            Assert.That(
                combatLessonText.rectTransform.anchorMin.y,
                Is.GreaterThanOrEqualTo(rewardPreviewText.rectTransform.anchorMax.y));
        }

        [UnityTest]
        public IEnumerator StageSelectRendersImmutableBriefingWithoutLegacyOptionalFallback()
        {
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            StageDefinitionProfile definition = CreateStageDefinition(
                "BRIEFING-RENDER-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "BRIEFING-RENDER-ROUTE",
                "briefing-render-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(
                catalogType,
                "briefing-render",
                route);
            var root = new GameObject("Briefing Render Test");
            root.SetActive(false);

            Text CreateText(string name)
            {
                var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(root.transform, false);
                return textObject.GetComponent<Text>();
            }

            Text titleText = CreateText("Title");
            Text objectiveText = CreateText("Objective");
            Text lessonText = CreateText("Lesson");
            Text threatText = CreateText("Threat");
            Text summonText = CreateText("Summon");
            Text rewardText = CreateText("Reward");

            try
            {
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(presenter, "stageCatalog", catalog);
                SetPrivateField(presenter, "selectedStageId", "briefing-render");
                SetPrivateField(presenter, "stageNameText", titleText);
                SetPrivateField(presenter, "summaryText", objectiveText);
                SetPrivateField(presenter, "combatLessonText", lessonText);
                SetPrivateField(presenter, "threatTagsText", threatText);
                SetPrivateField(presenter, "summonHintText", summonText);
                SetPrivateField(presenter, "rewardPreviewText", rewardText);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                root.SetActive(true);
                yield return null;

                object projection = ReadProperty(presenter, "SelectedRouteProjection");
                StageBriefingReadModel briefing =
                    (StageBriefingReadModel)ReadProperty(projection, "Briefing");
                Assert.That(titleText.text, Is.EqualTo(briefing.Title));
                Assert.That(objectiveText.text, Is.EqualTo(briefing.Objective));
                Assert.That(lessonText.text, Is.EqualTo(briefing.CombatLesson));
                Assert.That(lessonText.gameObject.activeSelf, Is.True);
                Assert.That(threatText.text, Is.Empty);
                Assert.That(summonText.text, Is.Empty);
                Assert.That(rewardText.text, Is.Empty);
                Assert.That(threatText.gameObject.activeSelf, Is.False);
                Assert.That(summonText.gameObject.activeSelf, Is.False);
                Assert.That(rewardText.gameObject.activeSelf, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [UnityTest]
        public IEnumerator ReplacedBriefingInstanceRejectsStartWithoutSideEffects()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            StageDefinitionProfile definition = CreateStageDefinition(
                "BRIEFING-INSTANCE-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "BRIEFING-INSTANCE-ROUTE",
                "briefing-instance-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(
                catalogType,
                "briefing-instance",
                route);
            AudioClip startClip = AudioClip.Create("BriefingInstanceStart", 64, 1, 44100, false);
            var root = new GameObject("Briefing Instance Start Test");
            root.SetActive(false);

            try
            {
                StageRunRuntime.ResetForTests();
                Component router = root.AddComponent(routerType);
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(router, "routeTable", routeTable);
                SetPrivateField(presenter, "stageCatalog", catalog);
                SetPrivateField(presenter, "selectedStageId", "briefing-instance");
                SetPrivateField(presenter, "router", router);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "startButtonSfx", startClip);
                int eventCount = 0;
                ReadPrivateField<UnityEvent>(presenter, "startRequested")
                    .AddListener(() => eventCount++);
                root.SetActive(true);

                object projection = ReadProperty(presenter, "SelectedRouteProjection");
                Assert.That(
                    route.TryCreateBriefingReadModel(
                        out StageBriefingReadModel replacement,
                        out StageBriefingBuildRejectReason rejectReason),
                    Is.True,
                    rejectReason.ToString());
                Assert.That(replacement, Is.Not.SameAs(ReadProperty(projection, "Briefing")));
                SetPrivateField(projection, "<Briefing>k__BackingField", replacement);
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;

                AssertRejectedStartHasNoSideEffects(
                    presenter,
                    router,
                    eventCount,
                    "StaleProjectionBundle");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(startClip);
            }
        }

        [UnityTest]
        public IEnumerator ReplacedResultProgressionPreflightRejectsStartWithoutSideEffects()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            StageDefinitionProfile definition = CreateStageDefinition(
                "RESULT-PREFLIGHT-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "RESULT-PREFLIGHT-ROUTE",
                "result-preflight-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(
                catalogType,
                "result-preflight",
                route);
            AudioClip startClip = AudioClip.Create("ResultPreflightStart", 64, 1, 44100, false);
            var root = new GameObject("Result Preflight Start Test");
            root.SetActive(false);

            try
            {
                StageRunRuntime.ResetForTests();
                Component router = root.AddComponent(routerType);
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(router, "routeTable", routeTable);
                SetPrivateField(presenter, "stageCatalog", catalog);
                SetPrivateField(presenter, "selectedStageId", "result-preflight");
                SetPrivateField(presenter, "router", router);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "startButtonSfx", startClip);
                int eventCount = 0;
                ReadPrivateField<UnityEvent>(presenter, "startRequested")
                    .AddListener(() => eventCount++);
                root.SetActive(true);

                object projection = ReadProperty(presenter, "SelectedRouteProjection");
                StageRunResultProgressionJoinSnapshot original =
                    (StageRunResultProgressionJoinSnapshot)ReadProperty(
                        projection,
                        "ResultProgressionJoinPreflight");
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out StageRunResultProgressionJoinSnapshot replacement,
                        out string replacementError),
                    Is.True,
                    replacementError);
                Assert.That(replacement, Is.Not.SameAs(original));
                Assert.That(replacement.CanonicalDigest, Is.EqualTo(original.CanonicalDigest));
                SetPrivateField(
                    projection,
                    "<ResultProgressionJoinPreflight>k__BackingField",
                    replacement);
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;

                AssertRejectedStartHasNoSideEffects(
                    presenter,
                    router,
                    eventCount,
                    "StaleProjectionBundle");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(startClip);
            }
        }

        [UnityTest]
        public IEnumerator ReplacedTemplateSourceRejectsStartWithoutSideEffects()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            StageDefinitionProfile definition = CreateStageDefinition(
                "TEMPLATE-SOURCE-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "TEMPLATE-SOURCE-ROUTE",
                "template-source-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(
                catalogType,
                "template-source",
                route);
            LinearStageTemplateProfile originalTemplate = route.ReferenceBlock.StageTemplate;
            LinearStageTemplateProfile replacementTemplate =
                UnityEngine.Object.Instantiate(originalTemplate);
            replacementTemplate.hideFlags = HideFlags.HideAndDontSave;
            AudioClip startClip = AudioClip.Create("TemplateSourceStart", 64, 1, 44100, false);
            var root = new GameObject("Template Source Start Test");
            root.SetActive(false);

            try
            {
                StageRunRuntime.ResetForTests();
                Component router = root.AddComponent(routerType);
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(router, "routeTable", routeTable);
                SetPrivateField(presenter, "stageCatalog", catalog);
                SetPrivateField(presenter, "selectedStageId", "template-source");
                SetPrivateField(presenter, "router", router);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "startButtonSfx", startClip);
                int eventCount = 0;
                ReadPrivateField<UnityEvent>(presenter, "startRequested")
                    .AddListener(() => eventCount++);
                root.SetActive(true);

                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                    Is.True);
                SetPrivateField(route.ReferenceBlock, "stageTemplate", replacementTemplate);
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;

                AssertRejectedStartHasNoSideEffects(
                    presenter,
                    router,
                    eventCount,
                    "SourceObjectMismatch");
            }
            finally
            {
                SetPrivateField(route.ReferenceBlock, "stageTemplate", originalTemplate);
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(replacementTemplate);
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(startClip);
            }
        }

        [UnityTest]
        public IEnumerator StaleCatalogGenerationRejectsCachedStartAndHidesEmptyReward()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            StageDefinitionProfile definition = CreateStageDefinition(
                "STALE-GENERATION-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "STALE-GENERATION-ROUTE",
                "stale-generation-entry",
                definition);
            ScriptableObject catalog =
                CreateStageCatalog(catalogType, "primary", route);
            AudioClip startClip = AudioClip.Create("StaleStageStart", 64, 1, 44100, false);
            var root = new GameObject("Stale Stage Route Test");
            root.SetActive(false);
            var rewardObject = new GameObject("Reward Preview");
            rewardObject.transform.SetParent(root.transform, false);
            Text rewardText = rewardObject.AddComponent<Text>();

            try
            {
                Component router = root.AddComponent(routerType);
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(router, "routeTable", routeTable);
                SetPrivateField(presenter, "stageCatalog", catalog);
                SetPrivateField(presenter, "selectedStageId", "primary");
                SetPrivateField(presenter, "router", router);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "rewardPreviewText", rewardText);
                SetPrivateField(presenter, "startButtonSfx", startClip);
                UnityEvent startRequested = ReadPrivateField<UnityEvent>(presenter, "startRequested");
                int requestCount = 0;
                startRequested.AddListener(() => requestCount++);
                root.SetActive(true);

                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                    Is.True);
                Assert.That(rewardObject.activeSelf, Is.False);

                var serializedCatalog = new SerializedObject(catalog);
                serializedCatalog.FindProperty("catalogProjectionGeneration").intValue = 2;
                serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;

                Assert.That(requestCount, Is.Zero);
                Assert.That(root.GetComponent<AudioSource>(), Is.Null);
                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                    Is.False);
                Assert.That(
                    ReadProperty(presenter, "SelectedRouteRejectReason").ToString(),
                    Is.EqualTo("StaleProjectionGeneration"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(startClip);
            }
        }

        [UnityTest]
        public IEnumerator InvalidSelectionMatrixClearsCachedProjectionLatchAndSuppressesStart()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            StageDefinitionProfile definition = CreateStageDefinition(
                "EMPTY-SELECTION-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "EMPTY-SELECTION-ROUTE",
                "empty-selection-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(catalogType, "primary", route);
            AudioClip startClip = AudioClip.Create(
                "InvalidSelectionStageStart",
                64,
                1,
                44100,
                false);
            var root = new GameObject("Empty Stage Selection Test");
            root.SetActive(false);
            var buttonObject = new GameObject("Start Button", typeof(RectTransform), typeof(Button));
            buttonObject.transform.SetParent(root.transform, false);
            Button startButton = buttonObject.GetComponent<Button>();

            try
            {
                StageRunRuntime.ResetForTests();
                Component router = root.AddComponent(routerType);
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(router, "routeTable", routeTable);
                SetPrivateField(presenter, "stageCatalog", catalog);
                SetPrivateField(presenter, "selectedStageId", "primary");
                SetPrivateField(presenter, "startButton", startButton);
                SetPrivateField(presenter, "router", router);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "startButtonSfx", startClip);
                UnityEvent startRequested = ReadPrivateField<UnityEvent>(presenter, "startRequested");
                int eventCount = 0;
                startRequested.AddListener(() => eventCount++);
                root.SetActive(true);

                (string Label, string Value, string ExpectedReason)[] invalidSelections =
                {
                    ("null", null, "MissingCatalogEntryId"),
                    ("empty", string.Empty, "MissingCatalogEntryId"),
                    ("whitespace", " \t\r\n ", "MissingCatalogEntryId"),
                    ("unknown", "unknown-stage-id", "CatalogEntryNotFound")
                };

                for (int i = 0; i < invalidSelections.Length; i++)
                {
                    (string label, string value, string expectedReason) = invalidSelections[i];
                    RequireMethod(presenterType, "SelectStage").Invoke(
                        presenter,
                        new object[] { "primary" });
                    yield return null;

                    Assert.That(
                        Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                        Is.True,
                        $"{label}: expected a cached valid projection before invalidation.");
                    Assert.That(startButton.interactable, Is.True, label);
                    object cachedBundle = ReadPrivateField<object>(
                        presenter,
                        "selectedRouteBundle");
                    RequireProperty(cachedBundle.GetType(), "RequestAccepted")
                        .SetValue(cachedBundle, true);
                    Assert.That(
                        Convert.ToBoolean(ReadProperty(presenter, "HasAcceptedStartRequest")),
                        Is.True,
                        $"{label}: expected the previous one-shot latch to be armed.");

                    RequireMethod(presenterType, "SelectStage").Invoke(
                        presenter,
                        new object[] { value });
                    yield return null;

                    Assert.That(
                        Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                        Is.False,
                        label);
                    Assert.That(
                        Convert.ToBoolean(ReadProperty(presenter, "HasAcceptedStartRequest")),
                        Is.False,
                        $"{label}: invalidation must remove the old one-shot latch.");
                    Assert.That(
                        ReadProperty(presenter, "SelectedRouteRejectReason").ToString(),
                        Is.EqualTo(expectedReason),
                        label);
                    Assert.That(startButton.interactable, Is.False, label);

                    RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                    yield return null;

                    Assert.That(
                        Convert.ToInt32(ReadProperty(router, "RouteRequestCount")),
                        Is.Zero,
                        label);
                    Assert.That(eventCount, Is.Zero, label);
                    Assert.That(
                        root.GetComponent<AudioSource>(),
                        Is.Null,
                        $"{label}: non-null start SFX must not be resolved or played.");
                    Assert.That(StageRunRuntime.HasActiveContext, Is.False, label);
                    Assert.That(StageRunRuntime.ActiveContext, Is.Null, label);
                    Assert.That(StageRunRuntime.LastAbortRecord, Is.Null, label);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(startClip);
            }
        }

        [UnityTest]
        public IEnumerator RejectedStageStartPublishesNoEventOrStartSfx()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            StageDefinitionProfile definition = CreateStageDefinition(
                "REJECTED-SEGMENT",
                CorridorScenePath);
            PlayableStageDefinition route = CreatePlayableStageDefinition(
                "REJECTED-ROUTE",
                "rejected-entry",
                definition);
            ScriptableObject catalog = CreateStageCatalog(catalogType, "primary", route);
            AudioClip startClip = AudioClip.Create("RejectedStageStart", 64, 1, 44100, false);
            var root = new GameObject("Rejected Stage Route Test");
            root.SetActive(false);

            try
            {
                Component router = root.AddComponent(routerType);
                Component presenter = root.AddComponent(presenterType);
                SetPrivateField(presenter, "stageCatalog", catalog);
                SetPrivateField(presenter, "selectedStageId", "primary");
                SetPrivateField(presenter, "router", router);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "startButtonSfx", startClip);
                UnityEvent startRequested = ReadPrivateField<UnityEvent>(presenter, "startRequested");
                int requestCount = 0;
                startRequested.AddListener(() => requestCount++);
                root.SetActive(true);

                Assert.That(root.GetComponent<AudioSource>(), Is.Null);
                LogAssert.Expect(LogType.Warning, "UI route is not configured: Combat");
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;

                Assert.That(requestCount, Is.Zero);
                Assert.That(root.GetComponent<AudioSource>(), Is.Null);
                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasAcceptedStartRequest")),
                    Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(route);
                UnityEngine.Object.DestroyImmediate(definition);
                UnityEngine.Object.DestroyImmediate(startClip);
            }
        }

        [UnityTest]
        public IEnumerator CanonicalUiScenesLoadWithTheirProductPresenters()
        {
            yield return LoadSceneAndAssertPresenter(
                LoginScenePath,
                "DimensionBrawl.UI.LoginScreenPresenter",
                true);
            yield return LoadSceneAndAssertPresenter(
                LobbyScenePath,
                "DimensionBrawl.UI.LobbyScreenPresenter",
                true);
            yield return LoadSceneAndAssertPresenter(
                StageSelectScenePath,
                "DimensionBrawl.UI.StageSelectScreenPresenter",
                true);
            yield return LoadSceneAndAssertPresenter(
                StageClearScenePath,
                "DimensionBrawl.UI.StageClear.StageClearScreenPresenter",
                false);

            Scene clearScene = SceneManager.GetActiveScene();
            StageClearScreenPresenter presenter =
                RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            CanvasGroup canvasGroup = ReadPrivateField<CanvasGroup>(presenter, "canvasGroup");
            RectTransform motionRoot = ReadPrivateField<RectTransform>(presenter, "motionRoot");
            Vector2 basePosition = ReadPrivateField<Vector2>(presenter, "entranceBasePosition");
            Vector3 baseScale = ReadPrivateField<Vector3>(presenter, "entranceBaseScale");
            Vector2 offset = ReadPrivateField<Vector2>(presenter, "entranceOffset");
            float startScale = ReadPrivateField<float>(presenter, "entranceStartScale");

            Assert.That(presenter.IsConfigured, Is.False);
            Assert.That(presenter.EntranceStartStateApplied, Is.True);
            Assert.That(presenter.EntranceStarted, Is.False);
            Assert.That(presenter.IsEntrancePlaying, Is.False);
            Assert.That(presenter.EntranceCompleted, Is.False);
            Assert.That(presenter.EntrancePlayCount, Is.Zero);
            Assert.That(canvasGroup.alpha, Is.Zero.Within(0.0001f));
            Assert.That(canvasGroup.interactable, Is.False);
            Assert.That(canvasGroup.blocksRaycasts, Is.False);
            Assert.That(
                Vector2.Distance(motionRoot.anchoredPosition, basePosition + offset),
                Is.LessThan(0.001f));
            Assert.That(
                Vector3.Distance(
                    motionRoot.localScale,
                    new Vector3(
                        baseScale.x * startScale,
                        baseScale.y * startScale,
                        baseScale.z)),
                Is.LessThan(0.001f));
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StageClearEntranceIgnoresDuplicateExternalAndOnEnableRequests()
        {
            bool observedConfiguredEntrance = false;
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(
                StageRouteOutcome.Clear,
                null,
                presenter =>
                {
                    observedConfiguredEntrance = true;
                    Assert.That(presenter.IsConfigured, Is.True, presenter.LastActionError);
                    Assert.That(presenter.EntranceStartStateApplied, Is.True);
                    Assert.That(presenter.EntranceStarted, Is.True);
                    Assert.That(presenter.IsEntrancePlaying, Is.True);
                    Assert.That(presenter.EntranceCompleted, Is.False);
                    Assert.That(presenter.EntrancePlayCount, Is.EqualTo(1));

                    CanvasGroup canvasGroup = ReadPrivateField<CanvasGroup>(presenter, "canvasGroup");
                    RectTransform motionRoot = ReadPrivateField<RectTransform>(presenter, "motionRoot");
                    float alphaBeforeDuplicate = canvasGroup.alpha;
                    Vector2 positionBeforeDuplicate = motionRoot.anchoredPosition;
                    Vector3 scaleBeforeDuplicate = motionRoot.localScale;

                    presenter.PlayEntrance();
                    presenter.PlayEntrance();

                    Assert.That(presenter.EntrancePlayCount, Is.EqualTo(1));
                    Assert.That(canvasGroup.alpha, Is.EqualTo(alphaBeforeDuplicate).Within(0.0001f));
                    Assert.That(
                        Vector2.Distance(motionRoot.anchoredPosition, positionBeforeDuplicate),
                        Is.LessThan(0.001f));
                    Assert.That(
                        Vector3.Distance(motionRoot.localScale, scaleBeforeDuplicate),
                        Is.LessThan(0.001f));
                });

            Assert.That(observedConfiguredEntrance, Is.True);
            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter settledPresenter =
                RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            Assert.That(settledPresenter.EntrancePlayCount, Is.EqualTo(1));
            Assert.That(settledPresenter.ClearBgmPlayCount, Is.EqualTo(1));
            Assert.That(settledPresenter.EntranceCompleted, Is.True);
            Assert.That(settledPresenter.IsEntrancePlaying, Is.False);

            settledPresenter.enabled = false;
            settledPresenter.enabled = true;
            Assert.That(settledPresenter.EntranceStarted, Is.True);
            Assert.That(settledPresenter.IsEntrancePlaying, Is.True);
            Assert.That(settledPresenter.EntranceCompleted, Is.False);
            Assert.That(settledPresenter.EntrancePlayCount, Is.EqualTo(2));
            Assert.That(settledPresenter.ClearBgmPlayCount, Is.EqualTo(1));

            CanvasGroup restartedCanvasGroup =
                ReadPrivateField<CanvasGroup>(settledPresenter, "canvasGroup");
            RectTransform restartedMotionRoot =
                ReadPrivateField<RectTransform>(settledPresenter, "motionRoot");
            float restartedAlpha = restartedCanvasGroup.alpha;
            Vector2 restartedPosition = restartedMotionRoot.anchoredPosition;
            Vector3 restartedScale = restartedMotionRoot.localScale;

            settledPresenter.PlayEntrance();
            settledPresenter.PlayEntrance();
            Assert.That(settledPresenter.EntrancePlayCount, Is.EqualTo(2));
            Assert.That(restartedCanvasGroup.alpha, Is.EqualTo(restartedAlpha).Within(0.0001f));
            Assert.That(
                Vector2.Distance(restartedMotionRoot.anchoredPosition, restartedPosition),
                Is.LessThan(0.001f));
            Assert.That(
                Vector3.Distance(restartedMotionRoot.localScale, restartedScale),
                Is.LessThan(0.001f));

            float deadline = Time.realtimeSinceStartup + 2f;
            while (settledPresenter.IsEntrancePlaying)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    "Re-enabled StageClear entrance did not settle in unscaled time.");
                yield return null;
            }

            Assert.That(settledPresenter.EntranceCompleted, Is.True);
            Assert.That(settledPresenter.EntrancePlayCount, Is.EqualTo(2));
            Assert.That(settledPresenter.ClearBgmPlayCount, Is.EqualTo(1));
            Assert.That(restartedCanvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(restartedCanvasGroup.interactable, Is.True);
            Assert.That(restartedCanvasGroup.blocksRaycasts, Is.True);
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StationVictoryReplayButtonLoadsFreshCorridorRun()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            AssertPresenterRoutes(presenter, StageRouteOutcome.Clear);
            Button retryButton = ReadPrivateField<Button>(presenter, "retryButton");
            Assert.That(retryButton.IsInteractable(), Is.True);

            retryButton.onClick.Invoke();
            yield return WaitForActiveScenePath(CorridorScenePath, 8f);

            Scene corridorScene = SceneManager.GetActiveScene();
            Assert.That(SceneManager.GetSceneByName(StageClearSceneName).isLoaded, Is.False);
            Assert.That(SceneManager.GetSceneByName("OlympusStationCombatStage").isLoaded, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));

            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(corridorScene);
            Assert.That(flow.StageCleared, Is.False);
            Assert.That(flow.StageClearOverlayShown, Is.False);
            GameObject playerRoot = FindSceneObject(corridorScene, "Player_CombatGirl_ActionFoundation");
            Assert.That(playerRoot, Is.Not.Null);
            Assert.That(playerRoot.activeInHierarchy, Is.False, "Replay must start a fresh intro run.");
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StationVictoryLobbyButtonLoadsCanonicalLobby()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            AssertPresenterRoutes(presenter, StageRouteOutcome.Clear);
            Button lobbyButton = ReadPrivateField<Button>(presenter, "lobbyButton");
            Assert.That(lobbyButton.IsInteractable(), Is.True);

            lobbyButton.onClick.Invoke();
            yield return WaitForActiveScenePath(LobbyScenePath, 8f);

            Scene lobbyScene = SceneManager.GetActiveScene();
            Assert.That(SceneManager.GetSceneByName(StageClearSceneName).isLoaded, Is.False);
            Assert.That(SceneManager.GetSceneByName("OlympusStationCombatStage").isLoaded, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                CountSceneComponents(lobbyScene, RequireProductType("DimensionBrawl.UI.LobbyScreenPresenter")),
                Is.EqualTo(1));
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StationFailureRetryButtonLoadsFreshCorridorRun()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Fail);

            Scene retiredCorridorScene = SceneManager.GetSceneByPath(CorridorScenePath);
            Assert.That(retiredCorridorScene.IsValid(), Is.True);
            Assert.That(retiredCorridorScene.isLoaded, Is.True);
            int retiredSceneHandle = retiredCorridorScene.handle;
            StageRunContext retiredContext = StageRunRuntime.ActiveContext;
            Assert.That(retiredContext, Is.Not.Null);
            string retiredRunId = retiredContext.Identity.RunId;
            StageCountOneEncounterExecutor retiredExecutor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(retiredCorridorScene);
            Assert.That(
                retiredExecutor.State,
                Is.EqualTo(StageCountOneEncounterState.Cancelled));
            Assert.That(retiredExecutor.ActivationCount, Is.EqualTo(1));
            Assert.That(retiredExecutor.TicketCount, Is.EqualTo(2));
            Assert.That(retiredExecutor.ActivatedTicketCount, Is.EqualTo(2));
            Assert.That(retiredExecutor.CompletionCount, Is.Zero);
            Assert.That(retiredExecutor.CancellationCount, Is.EqualTo(1));
            Assert.That(retiredExecutor.OwnedObjectCount, Is.Zero);
            Assert.That(retiredExecutor.OwnedRoot, Is.Null);
            Assert.That(retiredExecutor.OwnedHealth, Is.Null);
            Assert.That(retiredExecutor.OwnedAgent, Is.Null);
            Assert.That(retiredExecutor.OwnedSensor, Is.Null);
            Assert.That(retiredExecutor.HasCombatantParticipation, Is.False);
            Assert.That(retiredExecutor.PlayerTargetSelector, Is.Not.Null);
            Assert.That(
                retiredExecutor.PlayerTargetSelector.RuntimeTargetCandidateCount,
                Is.Zero);
            Assert.That(retiredExecutor.LastReceipt, Is.Not.Null);
            Assert.That(
                retiredExecutor.LastReceipt.TryValidateIntegrity(out string retiredReceiptError),
                Is.True,
                retiredReceiptError);

            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            AssertPresenterRoutes(presenter, StageRouteOutcome.Fail);
            Button retryButton = ReadPrivateField<Button>(presenter, "retryButton");
            Assert.That(retryButton.IsInteractable(), Is.True);

            retryButton.onClick.Invoke();
            yield return WaitForActiveScenePath(CorridorScenePath, 8f);

            Scene corridorScene = SceneManager.GetActiveScene();
            Assert.That(SceneManager.GetSceneByName(StageClearSceneName).isLoaded, Is.False);
            Assert.That(retiredCorridorScene.isLoaded, Is.False);
            Assert.That(retiredExecutor == null, Is.True);
            Assert.That(corridorScene.handle, Is.Not.EqualTo(retiredSceneHandle));
            StageRunContext freshContext = StageRunRuntime.ActiveContext;
            Assert.That(freshContext, Is.Not.Null);
            Assert.That(freshContext, Is.Not.SameAs(retiredContext));
            Assert.That(freshContext.Identity.RunId, Is.Not.EqualTo(retiredRunId));
            Assert.That(
                freshContext.LifecycleState,
                Is.EqualTo(StageRunLifecycleState.CorridorActive));
            StageCountOneEncounterExecutor freshExecutor =
                RequireSingleSceneComponent<StageCountOneEncounterExecutor>(corridorScene);
            Assert.That(ReferenceEquals(freshExecutor, retiredExecutor), Is.False);
            Assert.That(
                freshExecutor.State,
                Is.EqualTo(StageCountOneEncounterState.WaitingForRun));
            Assert.That(
                freshExecutor.LastError,
                Does.Contain("does not own this exact Station segment"));
            Assert.That(freshExecutor.ActivationCount, Is.Zero);
            Assert.That(freshExecutor.TicketCount, Is.EqualTo(2));
            Assert.That(freshExecutor.ActivatedTicketCount, Is.Zero);
            Assert.That(freshExecutor.CompletionCount, Is.Zero);
            Assert.That(freshExecutor.CancellationCount, Is.Zero);
            Assert.That(freshExecutor.OwnedObjectCount, Is.Zero);
            Assert.That(freshExecutor.HasCombatantParticipation, Is.False);
            Assert.That(freshExecutor.HasSceneLease, Is.False);
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(corridorScene);
            Assert.That(flow.HasCanonicalStageRun, Is.True);
            Assert.That(flow.StageCleared, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));

            flow.SkipIntroCutscene();
            yield return null;
            yield return null;
            Assert.That(
                freshContext.TrySealTutorialRouteCompletion(out string tutorialFactError),
                Is.True,
                tutorialFactError);
            RequireMethod(
                typeof(OlympusCorridorCombatFlowController),
                "BeginWaitingForStairEntry").Invoke(flow, null);
            RequireMethod(
                typeof(OlympusCorridorCombatFlowController),
                "BeginCorridorCombat").Invoke(flow, null);
            yield return null;
            yield return null;
            yield return ReleaseStationEntryGuide(corridorScene);

            float activationDeadline = Time.realtimeSinceStartup + 2f;
            while (freshExecutor.State != StageCountOneEncounterState.Active)
            {
                Assert.Less(Time.realtimeSinceStartup, activationDeadline, freshExecutor.LastError);
                yield return null;
            }

            StageAddEncounterTicketSnapshot freshRangedTicket = freshExecutor.GetTicketSnapshot(1);
            Assert.That(freshRangedTicket.PayloadId, Is.EqualTo("SciFiSoldier.Ranged"));
            Assert.That(freshRangedTicket.ProjectileDriver, Is.Not.Null);
            Assert.That(freshRangedTicket.ProjectileDriver.FiredCount, Is.Zero);
            Assert.That(freshRangedTicket.ProjectileDriver.OwnedProjectileCount, Is.Zero);
            Assert.That(freshRangedTicket.ProjectileDriver.ActiveProjectileCount, Is.Zero);
            Assert.That(freshRangedTicket.ProjectileDriver.HasIndependentRuntimeProjectileRoot, Is.True);
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StationFailureLobbyButtonLoadsCanonicalLobby()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Fail);

            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            AssertPresenterRoutes(presenter, StageRouteOutcome.Fail);
            Button lobbyButton = ReadPrivateField<Button>(presenter, "lobbyButton");
            Assert.That(lobbyButton.IsInteractable(), Is.True);

            lobbyButton.onClick.Invoke();
            yield return WaitForActiveScenePath(LobbyScenePath, 8f);

            Scene lobbyScene = SceneManager.GetActiveScene();
            Assert.That(SceneManager.GetSceneByName(StageClearSceneName).isLoaded, Is.False);
            Assert.That(SceneManager.GetSceneByName("OlympusStationCombatStage").isLoaded, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                CountSceneComponents(lobbyScene, RequireProductType("DimensionBrawl.UI.LobbyScreenPresenter")),
                Is.EqualTo(1));
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator TerminalActionResolverLoadFailureAndCompetingInputsStayFailClosed()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            StageRunResultSummary summary = presenter.ResultSummary;
            StageRunContext context = StageRunRuntime.ActiveContext;
            var loader = new RecordingSceneLoader(shouldSucceed: false, "injected single-load failure");
            StageRunRuntime.SetSceneLoaderForTests(loader);

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    new RejectingUiRouteResolver("injected resolver rejection"),
                    out _,
                    out string resolverError),
                Is.False);
            Assert.That(resolverError, Does.Contain("injected resolver rejection"));
            Assert.That(context.SelectedTerminalAction, Is.Null);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(loader.CallCount, Is.Zero);

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.replay",
                    null,
                    out StageRunResolvedTerminalAction selectedReplay,
                    out string loadError),
                Is.False);
            Assert.That(loadError, Does.Contain("injected single-load failure"));
            Assert.That(selectedReplay, Is.Not.Null);
            Assert.That(selectedReplay.ActionKind, Is.EqualTo(StageRouteActionKind.Replay));
            Assert.That(context.SelectedTerminalAction, Is.SameAs(selectedReplay));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(context.FaultReason, Does.Contain("injected single-load failure"));
            Assert.That(context.AbortRecord, Is.Null);
            Assert.That(context.DispatchClosureFaultRecord, Is.Not.Null);
            Assert.That(
                context.DispatchClosureFaultRecord.FailedBoundary,
                Is.EqualTo(StageDispatchClosureFailedBoundary.SceneLoad));
            Assert.That(
                context.DispatchClosureFaultRecord.ResultSummaryDigest,
                Is.EqualTo(summary.ResultSummaryDigest));
            Assert.That(
                context.DispatchClosureFaultRecord.TerminalActionSelectionId,
                Is.EqualTo(selectedReplay.SelectionId));
            Assert.That(
                context.DispatchClosureFaultRecord.TerminalActionSelectionDigest,
                Is.EqualTo(selectedReplay.CanonicalDigest));
            Assert.That(context.DispatchClosureFaultRecord.ClosureBarrierCount, Is.EqualTo(5));
            Assert.That(context.DispatchClosureFaultRecord.PendingClosureOwnerCount, Is.Zero);
            Assert.That(context.DispatchClosureFaultRecord.HasValidIntegrity(), Is.True);
            Assert.That(loader.CallCount, Is.EqualTo(1));

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.replay",
                    null,
                    out _,
                    out string duplicateError),
                Is.False);
            Assert.That(duplicateError, Does.Contain("already won selection"));
            Assert.That(loader.CallCount, Is.EqualTo(1));

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    new FixedUiRouteResolver(
                        StageUiRouteId.Lobby,
                        "UI_Lobby",
                        LobbyScenePath),
                    out _,
                    out string competingError),
                Is.False);
            Assert.That(competingError, Does.Contain("already won selection"));
            Assert.That(loader.CallCount, Is.EqualTo(1));
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator TerminalActionClosureIntegrityFailurePreservesSelectionAndBlocksLoader()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(
                SceneManager.GetSceneByName(StageClearSceneName));
            StageRunContext context = StageRunRuntime.ActiveContext;
            var loader = new RecordingSceneLoader(shouldSucceed: false, "must not be called");
            StageRunRuntime.SetSceneLoaderForTests(loader);
            StageRunRuntime.InjectTerminalActionClosureIntegrityFailureForTests();

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    presenter.ResultSummary,
                    "olympus-invasion.replay",
                    null,
                    out StageRunResolvedTerminalAction selection,
                    out string error),
                Is.False);
            Assert.That(error, Does.Contain("closure integrity"));
            Assert.That(selection, Is.Not.Null);
            Assert.That(context.SelectedTerminalAction, Is.SameAs(selection));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(loader.CallCount, Is.Zero);
            Assert.That(context.DispatchClosureFaultRecord, Is.Not.Null);
            Assert.That(
                context.DispatchClosureFaultRecord.FailedBoundary,
                Is.EqualTo(StageDispatchClosureFailedBoundary.ClosureIntegrity));
            Assert.That(context.DispatchClosureFaultRecord.HasValidIntegrity(), Is.True);
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator UnexpectedTerminalActionSceneExitPreservesSelectionAndBlocksSuccess()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(
                SceneManager.GetSceneByName(StageClearSceneName));
            StageRunContext context = StageRunRuntime.ActiveContext;
            var resolver = new FixedUiRouteResolver(
                StageUiRouteId.Lobby,
                "UI_Lobby",
                LobbyScenePath);
            var loader = new UnexpectedSceneChangingLoader();
            StageRunRuntime.SetSceneLoaderForTests(loader);

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    presenter.ResultSummary,
                    "olympus-invasion.to-lobby",
                    resolver,
                    out StageRunResolvedTerminalAction selection,
                    out string error),
                Is.False);
            Assert.That(error, Does.Contain("sealed destination scene"));
            Assert.That(selection, Is.Not.Null);
            Assert.That(context.SelectedTerminalAction, Is.SameAs(selection));
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(loader.CallCount, Is.EqualTo(1));
            Assert.That(context.DispatchClosureFaultRecord, Is.Not.Null);
            Assert.That(
                context.DispatchClosureFaultRecord.FailedBoundary,
                Is.EqualTo(StageDispatchClosureFailedBoundary.UnexpectedSceneExit));
            Assert.That(context.DispatchClosureFaultRecord.HasValidIntegrity(), Is.True);
            Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(context));
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator ResultFromReplacedRunCannotDispatchAgainstFreshCorridorContext()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);
            StageRunResultSummary staleSummary =
                RequireSingleSceneComponent<StageClearScreenPresenter>(
                    SceneManager.GetSceneByName(StageClearSceneName)).ResultSummary;
            string staleRunId = staleSummary.Identity.RunId;

            StageRunRuntime.ResetForTests();
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(
                CorridorScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            StageRunContext freshContext = StageRunRuntime.ActiveContext;
            Assert.That(freshContext, Is.Not.Null);
            Assert.That(freshContext.Identity.RunId, Is.Not.EqualTo(staleRunId));
            Assert.That(freshContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
            var loader = new RecordingSceneLoader(shouldSucceed: false, "must not be called");
            StageRunRuntime.SetSceneLoaderForTests(loader);

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    staleSummary,
                    "olympus-invasion.replay",
                    null,
                    out _,
                    out string staleError),
                Is.False);
            Assert.That(staleError, Does.Contain("current presented result"));
            Assert.That(freshContext.SelectedTerminalAction, Is.Null);
            Assert.That(freshContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
            Assert.That(loader.CallCount, Is.Zero);
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator MutatedCommittedResultDigestIsRejectedBeforeResolverOrLoader()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(
                SceneManager.GetSceneByName(StageClearSceneName));
            StageRunResultSummary summary = presenter.ResultSummary;
            StageRunContext context = StageRunRuntime.ActiveContext;
            string durableDigest = context.CommitReceipt.ResultSummaryDigest;
            SetPrivateField(
                summary,
                "<ResultSummaryDigest>k__BackingField",
                new string('0', 64));
            var resolver = new FixedUiRouteResolver(
                StageUiRouteId.Lobby,
                "UI_Lobby",
                LobbyScenePath);
            var loader = new RecordingSceneLoader(shouldSucceed: false, "must not be called");
            StageRunRuntime.SetSceneLoaderForTests(loader);

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    resolver,
                    out _,
                    out string error),
                Is.False);
            Assert.That(error, Does.Contain("integrity validation failed"));
            Assert.That(resolver.CallCount, Is.Zero);
            Assert.That(loader.CallCount, Is.Zero);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(context.SelectedTerminalAction, Is.Null);
            Assert.That(context.DispatchClosureFaultRecord, Is.Null);
            Assert.That(context.CommitReceipt.ResultSummaryDigest, Is.EqualTo(durableDigest));
            Assert.That(summary.ResultSummaryDigest, Is.Not.EqualTo(durableDigest));
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator MutatedRouteDigestIsRejectedBeforeResolverOrLoader()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(
                SceneManager.GetSceneByName(StageClearSceneName));
            StageRunResultSummary summary = presenter.ResultSummary;
            StageRunContext context = StageRunRuntime.ActiveContext;
            string committedResultDigest = summary.ResultSummaryDigest;
            string durableRouteDigest = context.CommitReceipt.RouteDigest;
            SetPrivateField(
                summary.Identity,
                "<RouteSnapshotDigest>k__BackingField",
                new string('f', 64));
            var resolver = new FixedUiRouteResolver(
                StageUiRouteId.Lobby,
                "UI_Lobby",
                LobbyScenePath);
            var loader = new RecordingSceneLoader(shouldSucceed: false, "must not be called");
            StageRunRuntime.SetSceneLoaderForTests(loader);

            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    resolver,
                    out _,
                    out string error),
                Is.False);
            Assert.That(error, Does.Contain("integrity validation failed"));
            Assert.That(resolver.CallCount, Is.Zero);
            Assert.That(loader.CallCount, Is.Zero);
            Assert.That(context.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(context.SelectedTerminalAction, Is.Null);
            Assert.That(context.DispatchClosureFaultRecord, Is.Null);
            Assert.That(summary.ResultSummaryDigest, Is.EqualTo(committedResultDigest));
            Assert.That(context.CommitReceipt.RouteDigest, Is.EqualTo(durableRouteDigest));
            Assert.That(summary.Identity.RouteSnapshotDigest, Is.Not.EqualTo(durableRouteDigest));
        }

        [UnityTest]
        public IEnumerator MissingCommittedSummaryLeavesStandaloneResultSurfaceNonInteractive()
        {
            StageRunRuntime.ResetForTests();
            EditorSceneManager.LoadSceneInPlayMode(
                StageClearScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene clearScene = SceneManager.GetActiveScene();
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            presenter.ConfigureResult(null);
            Button primaryButton = ReadPrivateField<Button>(presenter, "retryButton");
            Button lobbyButton = ReadPrivateField<Button>(presenter, "lobbyButton");

            Assert.That(presenter.IsConfigured, Is.False);
            Assert.That(presenter.PresentationSnapshot, Is.Null);
            Assert.That(presenter.LastActionError, Does.Contain("summary is missing"));
            Assert.That(primaryButton.IsInteractable(), Is.False);
            Assert.That(lobbyButton.IsInteractable(), Is.False);
            Text unresolvedTime = ReadPrivateField<Text>(presenter, "totalActiveTimeValueText");
            Assert.That(unresolvedTime.text, Is.Empty);
            Assert.That(unresolvedTime.gameObject.activeSelf, Is.False);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator ResultProgressionAdmissionRequiresExactCanonicalPresentationSources()
        {
            StageRunRuntime.ResetForTests();
            EditorSceneManager.LoadSceneInPlayMode(
                CorridorScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene corridorScene = SceneManager.GetActiveScene();
            Assert.That(corridorScene.path.Replace('\\', '/'), Is.EqualTo(CorridorScenePath));
            StageRunRuntime.ResetForTests();

            PlayableStageDefinition route = LoadRequired<PlayableStageDefinition>(PlayableStagePath);
            StageResultDefinition result = route.ResultProgressionJoin.ResultDefinition;
            StageResultPresentationCatalog catalog =
                LoadRequired<StageResultPresentationCatalog>(ResultPresentationCatalogPath);
            StageResultPresentationProfile canonicalProfile = result.PresentationProfile;
            StageResultLocalizationTable canonicalLocalization = result.LocalizationTable;
            StageResultPresentationCatalog catalogClone = UnityEngine.Object.Instantiate(catalog);
            StageResultPresentationProfile profileClone = UnityEngine.Object.Instantiate(canonicalProfile);
            StageResultLocalizationTable localizationClone =
                UnityEngine.Object.Instantiate(canonicalLocalization);
            catalogClone.hideFlags = HideFlags.HideAndDontSave;
            profileClone.hideFlags = HideFlags.HideAndDontSave;
            localizationClone.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                Assert.That(
                    route.ResultProgressionJoin.CanonicalPresentationCatalog,
                    Is.SameAs(catalog));
                Assert.That(result.CanonicalPresentationCatalog, Is.SameAs(catalog));
                Assert.That(catalog.GetProfile(0), Is.SameAs(canonicalProfile));
                Assert.That(catalog.LocalizationTable, Is.SameAs(canonicalLocalization));
                Assert.That(
                    profileClone.ComputeCanonicalDigest(),
                    Is.EqualTo(canonicalProfile.ComputeCanonicalDigest()));
                Assert.That(
                    localizationClone.ComputeCanonicalDigest(),
                    Is.EqualTo(canonicalLocalization.ComputeCanonicalDigest()));

                SetPrivateField(result, "canonicalPresentationCatalog", catalogClone);
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out _,
                        out string catalogError),
                    Is.False);
                Assert.That(catalogError, Does.Contain("route-owned canonical presentation catalog"));
                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        corridorScene,
                        out StageRunContext catalogCloneContext,
                        out string catalogAdmissionError),
                    Is.False);
                Assert.That(catalogCloneContext, Is.Null);
                Assert.That(
                    catalogAdmissionError,
                    Does.Contain("route-owned canonical presentation catalog"));
                Assert.That(StageRunRuntime.HasActiveContext, Is.False);

                SetPrivateField(catalogClone, "localizationTable", localizationClone);
                SetPrivateField(
                    catalogClone,
                    "profiles",
                    new[] { profileClone });
                SetPrivateField(result, "presentationProfile", profileClone);
                SetPrivateField(result, "localizationTable", localizationClone);
                Assert.That(catalogClone.TryValidate(out string cloneError), Is.True, cloneError);
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out _,
                        out string coherentCloneError),
                    Is.False);
                Assert.That(
                    coherentCloneError,
                    Does.Contain("route-owned canonical presentation catalog"));
                Assert.That(
                    StageRunRuntime.TryAdmitFirstSegment(
                        route,
                        corridorScene,
                        out StageRunContext coherentCloneContext,
                        out string coherentCloneAdmissionError),
                    Is.False);
                Assert.That(coherentCloneContext, Is.Null);
                Assert.That(
                    coherentCloneAdmissionError,
                    Does.Contain("route-owned canonical presentation catalog"));
                Assert.That(StageRunRuntime.HasActiveContext, Is.False);

                SetPrivateField(result, "canonicalPresentationCatalog", catalog);
                SetPrivateField(result, "presentationProfile", canonicalProfile);
                SetPrivateField(result, "localizationTable", canonicalLocalization);
                SetPrivateField(result, "presentationProfile", profileClone);
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out _,
                        out string profileError),
                    Is.False);
                Assert.That(profileError, Does.Contain("exact canonical presentation profile"));

                SetPrivateField(result, "presentationProfile", canonicalProfile);
                SetPrivateField(result, "localizationTable", localizationClone);
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out _,
                        out string localizationError),
                    Is.False);
                Assert.That(localizationError, Does.Contain("exact canonical localization source"));
            }
            finally
            {
                SetPrivateField(result, "canonicalPresentationCatalog", catalog);
                SetPrivateField(result, "presentationProfile", canonicalProfile);
                SetPrivateField(result, "localizationTable", canonicalLocalization);
                UnityEngine.Object.DestroyImmediate(catalogClone);
                UnityEngine.Object.DestroyImmediate(profileClone);
                UnityEngine.Object.DestroyImmediate(localizationClone);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StationDefinitionAuthorsExactOrderedTwoAddFixture()
        {
            StageDefinitionProfile station = LoadRequired<StageDefinitionProfile>(StationDefinitionPath);
            CombatEnemyArchetypeProfile meleeArchetype =
                LoadRequired<CombatEnemyArchetypeProfile>(StationMeleeAddArchetypePath);
            CombatEnemyArchetypeProfile rangedArchetype =
                LoadRequired<CombatEnemyArchetypeProfile>(StationRangedAddArchetypePath);
            Assert.That(station.AnchorCount, Is.EqualTo(2));
            Assert.That(station.SpawnCount, Is.EqualTo(2));
            string[] expectedSpawnIds = { "add-left", "add-right" };
            string[] expectedAnchorIds = { "Add_LeftLaneAnchor", "Add_RightLaneAnchor" };
            string[] expectedPayloadIds = { "SciFiSoldier.Melee", "SciFiSoldier.Ranged" };
            CombatEnemyArchetypeProfile[] expectedArchetypes = { meleeArchetype, rangedArchetype };
            int[] expectedPositionIds = { 2101, 2102 };
            Vector3[] expectedPositions =
            {
                new(8.9f, 0f, -1.25f),
                new(8.9f, 0f, 1.25f)
            };
            for (int sourceOrdinal = 0; sourceOrdinal < 2; sourceOrdinal++)
            {
                StageDefinitionProfile.AnchorRef authoredAnchor = station.GetAnchor(sourceOrdinal);
                StageDefinitionProfile.SpawnRef authoredSpawn = station.GetSpawn(sourceOrdinal);
                Assert.That(authoredAnchor.AnchorId, Is.EqualTo(expectedAnchorIds[sourceOrdinal]));
                Assert.That(authoredAnchor.GroupId, Is.EqualTo("CombatSpawnAnchors"));
                Assert.That(authoredAnchor.ExpectedPosition, Is.EqualTo(expectedPositions[sourceOrdinal]));
                Assert.That(authoredAnchor.ExpectedEuler, Is.EqualTo(Vector3.zero));
                Assert.That(authoredSpawn.SpawnId, Is.EqualTo(expectedSpawnIds[sourceOrdinal]));
                Assert.That(authoredSpawn.SpawnKind, Is.EqualTo(StageSpawnKind.Add));
                Assert.That(authoredSpawn.PositionId, Is.EqualTo(expectedPositionIds[sourceOrdinal]));
                Assert.That(authoredSpawn.AnchorId, Is.EqualTo(authoredAnchor.AnchorId));
                Assert.That(authoredSpawn.PayloadId, Is.EqualTo(expectedPayloadIds[sourceOrdinal]));
                Assert.That(authoredSpawn.PayloadArchetype, Is.SameAs(expectedArchetypes[sourceOrdinal]));
                Assert.That(authoredSpawn.AuthoredCount, Is.EqualTo(1));
                Assert.That(authoredSpawn.AuthoredDelaySeconds, Is.Zero);
            }

            GameObject gameplayPrefab = LoadRequired<GameObject>(StationMeleeAddPrefabPath);
            CombatAiPatternProfile meleePattern =
                LoadRequired<CombatAiPatternProfile>(StationMeleeAddPatternPath);
            Assert.That(meleeArchetype.ArchetypeId, Is.EqualTo("SciFiSoldier.Melee"));
            Assert.That(meleeArchetype.GameplayPrefab, Is.SameAs(gameplayPrefab));
            Assert.That(meleeArchetype.RequiresDedicatedPrefabPromotion, Is.False);
            BasicSoldierEnemy meleeSoldier = gameplayPrefab.GetComponent<BasicSoldierEnemy>();
            CombatTargetSensor meleeSensor = gameplayPrefab.GetComponent<CombatTargetSensor>();
            Assert.That(meleeSoldier, Is.Not.Null);
            Assert.That(meleeSensor, Is.Not.Null);
            Assert.That(meleeSoldier.PatternProfile, Is.SameAs(meleePattern));
            Assert.That(meleeSoldier.PatternDeck, Is.Null);
            Assert.That(meleePattern.AttackShape, Is.EqualTo(CombatAiAttackShape.MeleeArc));
            Assert.That(meleePattern.AttackRange, Is.LessThanOrEqualTo(meleeSensor.SearchRadius));
            Assert.That(
                gameplayPrefab.GetComponentsInChildren<BasicSoldierProjectileAttackDriver>(true),
                Is.Empty);

            GameObject rangedPrefab = LoadRequired<GameObject>(StationRangedAddPrefabPath);
            CombatAiPatternProfile rangedPattern =
                LoadRequired<CombatAiPatternProfile>(StationRangedAddPatternPath);
            CombatAiPatternDeck rangedDeck =
                LoadRequired<CombatAiPatternDeck>(StationRangedAddDeckPath);
            LaneActionProjectile rangedProjectile =
                LoadRequired<GameObject>(StationRangedProjectilePrefabPath)
                    .GetComponent<LaneActionProjectile>();
            Assert.That(rangedArchetype.ArchetypeId, Is.EqualTo("SciFiSoldier.Ranged"));
            Assert.That(rangedArchetype.GameplayPrefab, Is.SameAs(rangedPrefab));
            Assert.That(rangedArchetype.RequiresDedicatedPrefabPromotion, Is.False);
            BasicSoldierEnemy rangedSoldier = rangedPrefab.GetComponent<BasicSoldierEnemy>();
            CombatTargetSensor rangedSensor = rangedPrefab.GetComponent<CombatTargetSensor>();
            BasicSoldierProjectileAttackDriver rangedDriver =
                rangedPrefab.GetComponent<BasicSoldierProjectileAttackDriver>();
            Assert.That(rangedSoldier, Is.Not.Null);
            Assert.That(rangedSensor, Is.Not.Null);
            Assert.That(rangedDriver, Is.Not.Null);
            Assert.That(rangedSoldier.PatternProfile, Is.SameAs(rangedPattern));
            Assert.That(rangedSoldier.PatternDeck, Is.SameAs(rangedDeck));
            Assert.That(rangedPattern.ActorTypeId, Is.EqualTo("SciFiSoldier.Ranged"));
            Assert.That(rangedPattern.PatternId, Is.EqualTo("RifleCrossfire"));
            Assert.That(rangedPattern.AttackShape, Is.EqualTo(CombatAiAttackShape.ProjectileLine));
            Assert.That(rangedDeck.EntryCount, Is.EqualTo(1));
            Assert.That(rangedDeck.GetEntry(0).Profile, Is.SameAs(rangedPattern));
            Assert.That(rangedDriver.ProjectilePrefab, Is.SameAs(rangedProjectile));
            Assert.That(rangedDriver.MaxOwnedProjectileCount, Is.EqualTo(3));
            Assert.That(
                rangedDriver.IsConfiguredFor(rangedSoldier, rangedSoldier.SelfHealth, rangedSensor),
                Is.True);

            PlayableStageDefinition route = LoadRequired<PlayableStageDefinition>(PlayableStagePath);
            Assert.That(route.GetSceneSegment(1).SegmentId, Is.EqualTo("station_entry_combat"));
            Assert.That(route.GetSceneSegment(1).StageDefinition, Is.SameAs(station));
            LinearStageTemplateProfile template = route.ReferenceBlock.StageTemplate;
            StageTemplateRouteSegmentRef templateSegment = template.GetCanonicalRouteSegment(1);
            Assert.That(
                templateSegment.TemplateSegmentId,
                Is.EqualTo("olympus-invasion.station-guide-combat"));
            Assert.That(templateSegment.PocketCount, Is.EqualTo(2));
            StageTemplatePocketRef pocket = templateSegment.GetPocket(1);
            Assert.That(pocket.PocketId, Is.EqualTo("olympus-invasion.station.boss-encounter"));
            Assert.That(
                pocket.P1CAdmissionDisposition,
                Is.EqualTo(StageTemplateP1CAdmissionDisposition.NotAdmitted));
            Assert.That(pocket.EnemyRoleCount, Is.Zero);

            Type validatorType = Type.GetType(
                "DimensionBrawl.Editor.PlayableStageDefinitionValidator, Assembly-CSharp-Editor");
            Assert.That(validatorType, Is.Not.Null);
            MethodInfo validateStationAdd = validatorType.GetMethod(
                "ValidateStationAddSceneAuthoring",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(validateStationAdd, Is.Not.Null);
            MethodInfo validateSceneBinding = validatorType.GetMethod(
                "ValidateSceneBinding",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(validateSceneBinding, Is.Not.Null);

            StageRunRuntime.ResetForTests();
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(
                CorridorScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return WaitForActiveScenePath(CorridorScenePath, 10f);
            yield return null;

            Scene corridorScene = SceneManager.GetActiveScene();
            StageDefinitionProfile corridor = route.GetSceneSegment(0).StageDefinition;
            StageDefinitionSceneBinding corridorBinding =
                RequireSceneBinding(corridorScene, corridor);
            Assert.That(
                corridorBinding.TryGetAnchorPoint(
                    "Player_LeftShoulderCameraAnchor",
                    out StageAnchorPoint corridorCameraAnchor),
                Is.True);
            Assert.That(corridorCameraAnchor.transform.IsChildOf(corridorBinding.transform), Is.True);
            Assert.That(corridorCameraAnchor.transform.IsChildOf(corridorBinding.MapRoot), Is.False);
            Assert.DoesNotThrow(
                () => validateSceneBinding.Invoke(null, new object[] { corridorScene, corridor }));

            Scene stationScene = corridorScene;
            StageDefinitionSceneBinding binding =
                RequireSceneBinding(stationScene, station);
            Assert.That(binding.StageDefinition, Is.SameAs(station));
            Assert.That(station.MapScenePath, Is.EqualTo(CorridorScenePath));
            Assert.That(binding.MapRoot, Is.SameAs(corridorBinding.MapRoot));
            Assert.That(binding.MapRoot, Is.Not.Null);
            Assert.That(binding.AnchorPointCount, Is.EqualTo(2));
            for (int sourceOrdinal = 0; sourceOrdinal < 2; sourceOrdinal++)
            {
                StageDefinitionProfile.AnchorRef authoredAnchor = station.GetAnchor(sourceOrdinal);
                Assert.That(
                    binding.TryGetAnchorPoint(authoredAnchor.AnchorId, out StageAnchorPoint boundAnchor),
                    Is.True);
                Assert.That(boundAnchor, Is.Not.Null);
                Assert.That(boundAnchor.transform.IsChildOf(binding.MapRoot), Is.True);
                Vector3 boundRootLocalPosition =
                    binding.transform.InverseTransformPoint(boundAnchor.transform.position);
                Quaternion boundRootLocalRotation = Quaternion.Inverse(binding.transform.rotation)
                    * boundAnchor.transform.rotation;
                Assert.That(
                    Vector3.Distance(boundRootLocalPosition, authoredAnchor.ExpectedPosition),
                    Is.LessThan(0.0001f));
                Assert.That(
                    Quaternion.Angle(
                        boundRootLocalRotation,
                        Quaternion.Euler(authoredAnchor.ExpectedEuler)),
                    Is.LessThan(0.001f));
            }

            StageDefinitionProfile.AnchorRef anchor = station.GetAnchor(0);
            Assert.That(binding.TryGetAnchorPoint(anchor.AnchorId, out StageAnchorPoint liveAnchor), Is.True);

            Vector3 rootLocalPosition = binding.transform.InverseTransformPoint(liveAnchor.transform.position);
            Quaternion rootLocalRotation = Quaternion.Inverse(binding.transform.rotation)
                * liveAnchor.transform.rotation;
            Assert.That(Vector3.Distance(rootLocalPosition, anchor.ExpectedPosition), Is.LessThan(0.0001f));
            Assert.That(
                Quaternion.Angle(rootLocalRotation, Quaternion.Euler(anchor.ExpectedEuler)),
                Is.LessThan(0.001f));
            Assert.DoesNotThrow(
                () => validateSceneBinding.Invoke(null, new object[] { stationScene, station }));
            Assert.DoesNotThrow(
                () => validateStationAdd.Invoke(null, new object[] { stationScene, binding }));

            Transform intermediate = liveAnchor.transform.parent;
            Assert.That(intermediate, Is.Not.Null);
            Assert.That(intermediate, Is.Not.SameAs(binding.MapRoot));
            Assert.That(intermediate.IsChildOf(binding.MapRoot), Is.True);
            Vector3 originalIntermediatePosition = intermediate.localPosition;
            Quaternion originalIntermediateRotation = intermediate.localRotation;
            Vector3 originalAnchorPosition = liveAnchor.transform.localPosition;
            Quaternion originalAnchorRotation = liveAnchor.transform.localRotation;
            try
            {
                intermediate.localPosition = originalIntermediatePosition + new Vector3(0.5f, 0f, 0f);
                TargetInvocationException genericPositionError = Assert.Throws<TargetInvocationException>(
                    () => validateSceneBinding.Invoke(null, new object[] { stationScene, station }));
                Assert.That(
                    genericPositionError.InnerException?.Message,
                    Does.Contain("binding-root-local position"));
                TargetInvocationException positionError = Assert.Throws<TargetInvocationException>(
                    () => validateStationAdd.Invoke(null, new object[] { stationScene, binding }));
                Assert.That(positionError.InnerException?.Message, Does.Contain("binding-root-local position"));

                intermediate.localPosition = originalIntermediatePosition;
                intermediate.localRotation = Quaternion.Euler(0f, 15f, 0f) * originalIntermediateRotation;
                liveAnchor.transform.position = binding.transform.TransformPoint(anchor.ExpectedPosition);
                TargetInvocationException genericRotationError = Assert.Throws<TargetInvocationException>(
                    () => validateSceneBinding.Invoke(null, new object[] { stationScene, station }));
                Assert.That(
                    genericRotationError.InnerException?.Message,
                    Does.Contain("binding-root-local rotation"));
                TargetInvocationException rotationError = Assert.Throws<TargetInvocationException>(
                    () => validateStationAdd.Invoke(null, new object[] { stationScene, binding }));
                Assert.That(rotationError.InnerException?.Message, Does.Contain("binding-root-local rotation"));
            }
            finally
            {
                intermediate.localPosition = originalIntermediatePosition;
                intermediate.localRotation = originalIntermediateRotation;
                liveAnchor.transform.localPosition = originalAnchorPosition;
                liveAnchor.transform.localRotation = originalAnchorRotation;
            }

            Assert.DoesNotThrow(
                () => validateSceneBinding.Invoke(null, new object[] { stationScene, station }));
            Assert.DoesNotThrow(
                () => validateStationAdd.Invoke(null, new object[] { stationScene, binding }));
        }

        [Test]
        public void ResultProgressionSnapshotDamageRejectsWithoutThrowAndLocalesResolveDeterministically()
        {
            PlayableStageDefinition route = LoadRequired<PlayableStageDefinition>(PlayableStagePath);
            Assert.That(
                StageRunResultProgressionJoinSnapshot.TryCreate(
                    route,
                    out StageRunResultProgressionJoinSnapshot join,
                    out string createError),
                Is.True,
                createError);

            StageResultLocalizationSnapshot localization = join.PresentationSource.Localization;
            MethodInfo resolveLocale = RequireMethod(
                typeof(StageResultLocalizationSnapshot),
                "TryResolveLocale");
            MethodInfo resolveText = RequireMethod(
                typeof(StageResultLocalizationSnapshot),
                "TryResolve");

            object[] exactArguments = { "en-US", null, null };
            Assert.That((bool)resolveLocale.Invoke(localization, exactArguments), Is.True);
            Assert.That(exactArguments[1], Is.EqualTo("en-US"));

            object[] languageArguments = { "en-GB", null, null };
            Assert.That((bool)resolveLocale.Invoke(localization, languageArguments), Is.True);
            Assert.That(languageArguments[1], Is.EqualTo("en-US"));

            object[] defaultArguments = { "zz-ZZ", null, null };
            Assert.That((bool)resolveLocale.Invoke(localization, defaultArguments), Is.True);
            Assert.That(defaultArguments[1], Is.EqualTo("ko-KR"));

            object[] missingTextArguments =
                { "ko-KR", "stage_result.missing.required.text", null };
            Assert.That((bool)resolveText.Invoke(localization, missingTextArguments), Is.False);
            Assert.That(missingTextArguments[2], Is.EqualTo(string.Empty));

            StageResultPresentationSourceSnapshot source = join.PresentationSource;
            Array mappings = ReadPrivateField<Array>(source, "actionMappings");
            try
            {
                SetPrivateField(source, "actionMappings", null);
                bool valid = true;
                string integrityError = string.Empty;
                Assert.DoesNotThrow(() => valid = join.TryValidateIntegrity(out integrityError));
                Assert.That(valid, Is.False);
                Assert.That(integrityError, Does.Contain("damaged nested data"));
            }
            finally
            {
                SetPrivateField(source, "actionMappings", mappings);
            }
        }

        [Test]
        public void ProductionProgressionGraphRejectsInvalidEdgesAndAcceptsAsymmetricRelations()
        {
            PlayableStageDefinition route = LoadRequired<PlayableStageDefinition>(PlayableStagePath);
            StageProgressionNode nodeTemplate = route.ResultProgressionJoin.ProgressionNode;
            StageProgressionGraph graphTemplate = route.ResultProgressionJoin.ProgressionGraph;
            var created = new List<UnityEngine.Object>();
            const string GraphId = "test.result-progression.graph";

            try
            {
                StageProgressionNode self = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.self",
                    GraphId,
                    created);
                ConfigureProgressionNodeEdges(
                    self,
                    new[] { self },
                    new[] { 1 },
                    Array.Empty<StageProgressionNode>(),
                    Array.Empty<int>());
                Assert.That(
                    self.TryComputeCanonicalDigests(out _, out _, out string selfError),
                    Is.False);
                Assert.That(selfError, Does.Contain("prerequisite").And.Contain("invalid"));

                StageProgressionNode duplicateSource = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.duplicate-source",
                    GraphId,
                    created);
                StageProgressionNode duplicateTarget = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.duplicate-target",
                    GraphId,
                    created);
                ConfigureProgressionNodeEdges(
                    duplicateSource,
                    new[] { duplicateTarget, duplicateTarget },
                    new[] { 1, 1 },
                    Array.Empty<StageProgressionNode>(),
                    Array.Empty<int>());
                Assert.That(
                    duplicateSource.TryComputeCanonicalDigests(
                        out _,
                        out _,
                        out string duplicateEdgeError),
                    Is.False);
                Assert.That(duplicateEdgeError, Does.Contain("prerequisite").And.Contain("invalid"));

                StageProgressionNode wrongRevisionSource = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.wrong-revision-source",
                    GraphId,
                    created);
                StageProgressionNode wrongRevisionTarget = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.wrong-revision-target",
                    GraphId,
                    created);
                ConfigureProgressionNodeEdges(
                    wrongRevisionSource,
                    Array.Empty<StageProgressionNode>(),
                    Array.Empty<int>(),
                    new[] { wrongRevisionTarget },
                    new[] { 2 });
                Assert.That(
                    wrongRevisionSource.TryComputeCanonicalDigests(
                        out _,
                        out _,
                        out string wrongRevisionError),
                    Is.False);
                Assert.That(wrongRevisionError, Does.Contain("recommended-next").And.Contain("invalid"));

                StageProgressionNode unresolvedA = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.unresolved-a",
                    GraphId,
                    created);
                StageProgressionNode unresolvedB = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.unresolved-b",
                    GraphId,
                    created);
                StageProgressionNode unresolvedC = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.unresolved-c",
                    GraphId,
                    created);
                ConfigureProgressionNodeEdges(
                    unresolvedA,
                    new[] { unresolvedC },
                    new[] { 1 },
                    Array.Empty<StageProgressionNode>(),
                    Array.Empty<int>());
                SealProgressionNode(unresolvedA);
                SealProgressionNode(unresolvedB);
                StageProgressionGraph unresolvedGraph = CreateProgressionGraphFixture(
                    graphTemplate,
                    GraphId,
                    new[] { unresolvedA, unresolvedB },
                    created);
                Assert.That(
                    unresolvedGraph.TryComputeCanonicalDigest(out _, out string unresolvedError),
                    Is.False);
                Assert.That(unresolvedError, Does.Contain("does not resolve exact graph identity"));

                StageProgressionNode duplicateNodeA = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.duplicate-id",
                    GraphId,
                    created);
                StageProgressionNode duplicateNodeB = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.duplicate-id",
                    GraphId,
                    created);
                SealProgressionNode(duplicateNodeA);
                SealProgressionNode(duplicateNodeB);
                StageProgressionGraph duplicateNodeGraph = CreateProgressionGraphFixture(
                    graphTemplate,
                    GraphId,
                    new[] { duplicateNodeA, duplicateNodeB },
                    created);
                Assert.That(
                    duplicateNodeGraph.TryComputeCanonicalDigest(
                        out _,
                        out string duplicateNodeError),
                    Is.False);
                Assert.That(duplicateNodeError, Does.Contain("invalid or duplicated"));

                StageProgressionNode cycleA = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.cycle-a",
                    GraphId,
                    created);
                StageProgressionNode cycleB = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.cycle-b",
                    GraphId,
                    created);
                ConfigureProgressionNodeEdges(
                    cycleA,
                    new[] { cycleB },
                    new[] { 1 },
                    Array.Empty<StageProgressionNode>(),
                    Array.Empty<int>());
                ConfigureProgressionNodeEdges(
                    cycleB,
                    new[] { cycleA },
                    new[] { 1 },
                    Array.Empty<StageProgressionNode>(),
                    Array.Empty<int>());
                SealProgressionNode(cycleA);
                SealProgressionNode(cycleB);
                StageProgressionGraph cycleGraph = CreateProgressionGraphFixture(
                    graphTemplate,
                    GraphId,
                    new[] { cycleA, cycleB },
                    created);
                Assert.That(
                    cycleGraph.TryComputeCanonicalDigest(out _, out string cycleError),
                    Is.False);
                Assert.That(cycleError, Does.Contain("cycle within one directed relation"));

                StageProgressionNode asymmetricA = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.asymmetric-a",
                    GraphId,
                    created);
                StageProgressionNode asymmetricB = CreateProgressionNodeFixture(
                    nodeTemplate,
                    "test.node.asymmetric-b",
                    GraphId,
                    created);
                ConfigureProgressionNodeEdges(
                    asymmetricA,
                    Array.Empty<StageProgressionNode>(),
                    Array.Empty<int>(),
                    new[] { asymmetricB },
                    new[] { 1 });
                SealProgressionNode(asymmetricA);
                SealProgressionNode(asymmetricB);
                StageProgressionGraph asymmetricGraph = CreateProgressionGraphFixture(
                    graphTemplate,
                    GraphId,
                    new[] { asymmetricA, asymmetricB },
                    created);
                Assert.That(
                    asymmetricGraph.TryComputeCanonicalDigest(
                        out string asymmetricDigest,
                        out string asymmetricError),
                    Is.True,
                    asymmetricError);
                SetPrivateField(asymmetricGraph, "canonicalDigest", asymmetricDigest);
                Assert.That(
                    asymmetricGraph.TryCreateSnapshot(
                        out StageProgressionGraphSnapshot asymmetricSnapshot,
                        out string asymmetricSnapshotError),
                    Is.True,
                    asymmetricSnapshotError);
                Assert.That(asymmetricSnapshot.NodeCount, Is.EqualTo(2));
                Assert.That(asymmetricSnapshot.GetNode(0).RecommendedNextCount, Is.EqualTo(1));
                Assert.That(asymmetricSnapshot.GetNode(1).RecommendedNextCount, Is.Zero);
            }
            finally
            {
                for (int i = created.Count - 1; i >= 0; i--)
                {
                    UnityEngine.Object.DestroyImmediate(created[i]);
                }
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator ResultPresentationUsesDeepCopiedAdmissionSourcesAndSealsAudit()
        {
            PlayableStageDefinition route = LoadRequired<PlayableStageDefinition>(PlayableStagePath);
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject stageCatalog = LoadRequired<ScriptableObject>(StageCatalogPath);
            object[] projectionArguments = { ResolveUiRouteId(CombatRouteId), null, null };
            Assert.That(
                (bool)RequireMethod(
                    catalogType,
                    "TryCreateFirstRouteProjection").Invoke(
                    stageCatalog,
                    projectionArguments),
                Is.True,
                projectionArguments[2]?.ToString());
            StageRunResultProgressionJoinSnapshot selectionPreflight =
                (StageRunResultProgressionJoinSnapshot)ReadProperty(
                    projectionArguments[1],
                    "ResultProgressionJoinPreflight");
            StageResultDefinition resultDefinition =
                route.ResultProgressionJoin.ResultDefinition;
            StageResultPresentationProfile profile = resultDefinition.PresentationProfile;
            StageResultLocalizationTable localization = resultDefinition.LocalizationTable;
            StageResultActionPresentationMapping replayMapping =
                resultDefinition.GetActionMapping(0);
            string originalStageCode = ReadPrivateField<string>(profile, "stageCode");
            string stageNameKey = ReadPrivateField<string>(profile, "stageNameKey");
            string clearStatusKey = ReadPrivateField<string>(profile, "clearStatusKey");
            string originalReplayLabelKey = replayMapping.LabelKey;
            Assert.That(
                localization.TryResolveLocale(
                    "ko-KR",
                    out string resolvedLocaleId,
                    out string localeError),
                Is.True,
                localeError);
            Assert.That(
                localization.TryResolve(
                    resolvedLocaleId,
                    stageNameKey,
                    out string originalStageName),
                Is.True);
            Assert.That(
                localization.TryResolve(
                    resolvedLocaleId,
                    originalReplayLabelKey,
                    out string originalReplayLabel),
                Is.True);
            StageResultLocalizedString stageNameEntry = FindLocalizationEntry(
                localization,
                resolvedLocaleId,
                stageNameKey);
            string originalStageNameValue = stageNameEntry.Value;
            const string MutatedStageCode = "MUTATED-AFTER-ADMISSION";
            const string MutatedStageName = "MUTATED LIVE SOURCE";
            StageRunContext admittedContext = null;

            try
            {
                yield return LoadCanonicalStationTerminalAndWaitForResultSurface(
                    StageRouteOutcome.Clear,
                    context =>
                    {
                        admittedContext = context;
                        Assert.That(
                            context.ResultProgressionJoinSnapshot,
                            Is.Not.SameAs(selectionPreflight));
                        Assert.That(
                            context.ResultProgressionJoinSnapshot.CanonicalDigest,
                            Is.EqualTo(selectionPreflight.CanonicalDigest));
                        Assert.That(
                            context.ResultProgressionJoinSnapshot.PresentationSource.Profile.StageCode,
                            Is.EqualTo(originalStageCode));
                        SetPrivateField(profile, "stageCode", MutatedStageCode);
                        SetPrivateField(stageNameEntry, "value", MutatedStageName);
                        SetPrivateField(replayMapping, "labelKey", clearStatusKey);
                    });

                Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
                StageClearScreenPresenter presenter =
                    RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
                StageResultPresentationSnapshot presentation = presenter.PresentationSnapshot;
                StageResultPresentationAuditEnvelope audit = presenter.PresentationAudit;

                Assert.That(admittedContext, Is.Not.Null);
                Assert.That(profile.ComputeCanonicalDigest(), Is.Not.EqualTo(
                    admittedContext.ResultProgressionJoinSnapshot.PresentationSource.Profile.CanonicalDigest));
                Assert.That(presentation, Is.Not.Null);
                Assert.That(presentation.StageCode, Is.EqualTo(originalStageCode));
                Assert.That(presentation.StageTitle, Does.Contain(originalStageName));
                Assert.That(presentation.StageTitle, Does.Not.Contain(MutatedStageName));
                Assert.That(presentation.PrimaryActionLabel, Is.EqualTo(originalReplayLabel));
                Assert.That(audit, Is.Not.Null);
                Assert.That(
                    audit.TryValidate(
                        presenter.ResultSummary,
                        admittedContext.ResultProgressionJoinSnapshot,
                        presentation,
                        out string auditError),
                    Is.True,
                    auditError);
                Assert.That(
                    audit.JoinSnapshotDigest,
                    Is.EqualTo(admittedContext.ResultProgressionJoinSnapshot.CanonicalDigest));
                Assert.That(
                    audit.PresentationSourceDigest,
                    Is.EqualTo(
                        admittedContext.ResultProgressionJoinSnapshot.PresentationSource.CanonicalDigest));
                Assert.That(
                    StageRunRuntime.TryPrepareResultPresentation(
                        presenter.ResultSummary,
                        selectionPreflight,
                        presentation.LocaleId,
                        out _,
                        out _,
                        out string foreignPreflightError),
                    Is.False);
                Assert.That(
                    foreignPreflightError,
                    Does.Contain("exact admission join snapshot"));
            }
            finally
            {
                SetPrivateField(profile, "stageCode", originalStageCode);
                SetPrivateField(stageNameEntry, "value", originalStageNameValue);
                SetPrivateField(replayMapping, "labelKey", originalReplayLabelKey);
            }
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator PresentedTerminalActionRevalidatesJoinPresentationAndAuditBeforeSideEffects()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(
                SceneManager.GetSceneByName(StageClearSceneName));
            StageRunContext context = StageRunRuntime.ActiveContext;
            StageRunResultSummary summary = presenter.ResultSummary;
            StageResultPresentationSnapshot presentation = presenter.PresentationSnapshot;
            StageResultPresentationAuditEnvelope audit = presenter.PresentationAudit;
            var resolver = new FixedUiRouteResolver(
                StageUiRouteId.Lobby,
                "UI_Lobby",
                LobbyScenePath);
            var loader = new RecordingSceneLoader(shouldSucceed: false, "must not be called");
            StageRunRuntime.SetSceneLoaderForTests(loader);

            string originalLocaleId = audit.LocaleId;
            SetPrivateField(audit, "<LocaleId>k__BackingField", "tampered-locale");
            Assert.That(
                audit.TryValidate(
                    summary,
                    context.ResultProgressionJoinSnapshot,
                    presentation,
                    out string auditIntegrityError),
                Is.False);
            Assert.That(auditIntegrityError, Does.Contain("audit envelope is damaged"));
            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    resolver,
                    out _,
                    out string damagedAuditError),
                Is.False);
            Assert.That(damagedAuditError, Does.Contain("presented result authority"));
            Assert.That(resolver.CallCount, Is.Zero);
            Assert.That(loader.CallCount, Is.Zero);
            Assert.That(context.SelectedTerminalAction, Is.Null);
            SetPrivateField(audit, "<LocaleId>k__BackingField", originalLocaleId);

            string originalStageCode = presentation.StageCode;
            SetPrivateField(
                presentation,
                "<StageCode>k__BackingField",
                "TAMPERED-PRESENTATION");
            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    resolver,
                    out _,
                    out string damagedPresentationError),
                Is.False);
            Assert.That(damagedPresentationError, Does.Contain("presented result authority"));
            Assert.That(resolver.CallCount, Is.Zero);
            Assert.That(loader.CallCount, Is.Zero);
            Assert.That(context.SelectedTerminalAction, Is.Null);
            SetPrivateField(presentation, "<StageCode>k__BackingField", originalStageCode);

            StageResultPresentationSourceSnapshot source =
                context.ResultProgressionJoinSnapshot.PresentationSource;
            Array mappings = ReadPrivateField<Array>(source, "actionMappings");
            SetPrivateField(source, "actionMappings", null);
            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    resolver,
                    out _,
                    out string damagedJoinError),
                Is.False);
            Assert.That(damagedJoinError, Does.Contain("presented result authority"));
            Assert.That(resolver.CallCount, Is.Zero);
            Assert.That(loader.CallCount, Is.Zero);
            Assert.That(context.SelectedTerminalAction, Is.Null);
            SetPrivateField(source, "actionMappings", mappings);

            Assert.That(
                context.ResultProgressionJoinSnapshot.TryValidateIntegrity(out string restoredJoinError),
                Is.True,
                restoredJoinError);
            Assert.That(
                audit.TryValidate(
                    summary,
                    context.ResultProgressionJoinSnapshot,
                    presentation,
                    out string restoredAuditError),
                Is.True,
                restoredAuditError);
        }

        [UnityTest]
        [Timeout(45000)]
        public IEnumerator ProcessLossDoesNotReconstructPresentationAuthorityOrDamageDurableDecision()
        {
            yield return LoadCanonicalStationTerminalAndWaitForResultSurface(StageRouteOutcome.Clear);

            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(
                SceneManager.GetSceneByName(StageClearSceneName));
            StageRunContext context = StageRunRuntime.ActiveContext;
            StageRunResultSummary summary = presenter.ResultSummary;
            StageRunResultProgressionJoinSnapshot join = context.ResultProgressionJoinSnapshot;
            StageRunResultCommitReceipt receipt = context.CommitReceipt;
            string runId = context.Identity.RunId;
            string decisionPath = StageRunRuntime.GetResultCommitDecisionPathForTests(runId);
            Assert.That(File.Exists(decisionPath), Is.True);
            byte[] durableDecisionBytes = File.ReadAllBytes(decisionPath);

            StageRunRuntime.SimulateProcessLossForTests();

            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(
                StageRunRuntime.TryReadCommittedResultDecision(
                    runId,
                    out StageRunResultCommitReceipt recoveredReceipt,
                    out string recoveryError),
                Is.True,
                recoveryError);
            Assert.That(
                (bool)RequireMethod(
                    typeof(StageRunResultCommitReceipt),
                    "HasValidIntegrity").Invoke(recoveredReceipt, null),
                Is.True);
            Assert.That(recoveredReceipt.RunId, Is.EqualTo(receipt.RunId));
            Assert.That(
                recoveredReceipt.ResultSummaryDigest,
                Is.EqualTo(receipt.ResultSummaryDigest));
            Assert.That(
                recoveredReceipt.TerminalFinalizationOwnerCoverageDigest,
                Is.EqualTo(receipt.TerminalFinalizationOwnerCoverageDigest));
            Assert.That(recoveredReceipt.CanonicalDigest, Is.EqualTo(receipt.CanonicalDigest));
            Assert.That(recoveredReceipt.EnvelopeChecksum, Is.EqualTo(receipt.EnvelopeChecksum));
            Assert.That(File.Exists(decisionPath), Is.True);
            CollectionAssert.AreEqual(
                durableDecisionBytes,
                File.ReadAllBytes(decisionPath),
                "Process-memory reset and durable reconciliation must not rewrite the sealed decision bytes.");
            Assert.That(
                Directory.GetFiles(
                    Path.GetDirectoryName(decisionPath),
                    Path.GetFileName(decisionPath) + ".corrupt*"),
                Is.Empty);

            Assert.That(
                StageRunRuntime.TryPrepareResultPresentation(
                    summary,
                    join,
                    "ko-KR",
                    out _,
                    out _,
                    out string presentationError),
                Is.False);
            Assert.That(presentationError, Does.Contain("No active canonical stage run"));

            var resolver = new FixedUiRouteResolver(
                StageUiRouteId.Lobby,
                "UI_Lobby",
                LobbyScenePath);
            var loader = new RecordingSceneLoader(shouldSucceed: false, "must not be called");
            StageRunRuntime.SetSceneLoaderForTests(loader);
            Assert.That(
                StageRunRuntime.TryDispatchTerminalAction(
                    summary,
                    "olympus-invasion.to-lobby",
                    resolver,
                    out _,
                    out string dispatchError),
                Is.False);
            Assert.That(dispatchError, Does.Contain("No active presented result"));
            Assert.That(resolver.CallCount, Is.Zero);
            Assert.That(loader.CallCount, Is.Zero);
            Assert.That(File.Exists(decisionPath), Is.True);
            CollectionAssert.AreEqual(durableDecisionBytes, File.ReadAllBytes(decisionPath));
        }

        private static IEnumerator LoadCanonicalStationTerminalAndWaitForResultSurface(
            StageRouteOutcome outcome,
            Action<StageRunContext> afterAdmission = null,
            Action<StageClearScreenPresenter> afterResultConfigured = null)
        {
            Time.timeScale = 1f;
            StageRunRuntime.ResetForTests();
            EditorSceneManager.LoadSceneInPlayMode(
                CorridorScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene corridorScene = SceneManager.GetActiveScene();
            Assert.AreEqual(CorridorScenePath, corridorScene.path.Replace('\\', '/'));
            StageRunContext runContext = StageRunRuntime.ActiveContext;
            Assert.That(runContext, Is.Not.Null, "Corridor must atomically admit a canonical stage run.");
            Assert.That(runContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.CorridorActive));
            afterAdmission?.Invoke(runContext);
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(corridorScene);
            flow.SkipIntroCutscene();
            yield return null;
            yield return null;
            Assert.That(flow.CanonicalStageRunId, Is.EqualTo(runContext.Identity.RunId));
            Assert.That(
                runContext.TrySealTutorialRouteCompletion(out string tutorialFactError),
                Is.True,
                tutorialFactError);
            RequireMethod(
                typeof(OlympusCorridorCombatFlowController),
                "BeginWaitingForStairEntry").Invoke(flow, null);
            RequireMethod(
                typeof(OlympusCorridorCombatFlowController),
                "BeginCorridorCombat").Invoke(flow, null);
            StageSegmentEntryReceipt entryReceipt = runContext.SegmentEntryReceipt;
            Assert.That(entryReceipt, Is.Not.Null);
            Assert.That(entryReceipt.FromHandoffPending, Is.False);
            Assert.That(entryReceipt.ActualScenePath, Is.EqualTo(CorridorScenePath));
            yield return null;
            yield return null;

            Scene stationScene = SceneManager.GetActiveScene();
            Assert.AreEqual(CorridorScenePath, stationScene.path.Replace('\\', '/'));
            Assert.IsTrue(
                stationScene.handle == corridorScene.handle,
                $"In-scene Station activation must retain Corridor scene handle {corridorScene.handle}; "
                + $"actual={stationScene.handle}.");
            Assert.That(SceneManager.GetSceneByName("OlympusStationCombatStage").isLoaded, Is.False);
            Assert.That(CountSceneComponents<OlympusStationCombatResultPresenter>(stationScene), Is.EqualTo(1));
            Assert.That(CountSceneComponents<OlympusStageClearOverlay>(stationScene), Is.EqualTo(1));
            Assert.That(CountSceneComponents<CombatEncounterController>(stationScene), Is.EqualTo(1));
            OlympusStationCombatResultPresenter resultPresenter =
                RequireSingleSceneComponent<OlympusStationCombatResultPresenter>(stationScene);
            Assert.That(resultPresenter.HasCanonicalStageRun, Is.True, resultPresenter.CanonicalStageRunEntryError);
            Assert.That(StageRunRuntime.ActiveContext, Is.SameAs(runContext));
            Assert.That(runContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.StationActive));

            yield return ReleaseStationEntryGuide(stationScene);
            yield return new WaitForSecondsRealtime(0.05f);
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));

            CombatEncounterController encounter = RequireSingleSceneComponent<CombatEncounterController>(stationScene);
            CombatHealth terminalHealth = outcome == StageRouteOutcome.Clear
                ? ReadPrivateField<CombatHealth>(encounter, "enemyHealth")
                : ReadPrivateField<CombatHealth>(encounter, "playerHealth");
            DamageTeam sourceTeam = outcome == StageRouteOutcome.Clear
                ? DamageTeam.Player
                : DamageTeam.Enemy;
            float invulnerabilityDeadline = Time.realtimeSinceStartup + 1f;
            while (terminalHealth.IsInvulnerable)
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    invulnerabilityDeadline,
                    "Tutorial invulnerability did not expire during the physical stair-entry interval.");
                yield return null;
            }
            Assert.That(encounter.UsesCoordinatedTerminalResolution, Is.True);
            Assert.That(
                encounter.IsRunning,
                Is.True,
                $"Station encounter was not running before victory injection. "
                + $"Won={encounter.IsWon}, Failed={encounter.IsFailed}, Faulted={encounter.IsFaulted}, "
                + $"Diagnostic={encounter.Diagnostic.Reason}: {encounter.Diagnostic.Message}");
            Assert.That(
                terminalHealth.TryApplyDamage(new DamageInfo(
                    null,
                    sourceTeam,
                    terminalHealth.MaxHealth + 1f,
                    terminalHealth.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly,
                    CombatControlLockPolicy.None)),
                Is.True);
            Assert.That(encounter.IsWon, Is.EqualTo(outcome == StageRouteOutcome.Clear));
            Assert.That(encounter.IsFailed, Is.EqualTo(outcome == StageRouteOutcome.Fail));

            float deadline = Time.realtimeSinceStartup + 12f;
            StageClearScreenPresenter presenter = null;
            bool resultObserverInvoked = false;
            while (presenter == null || !IsPresenterInteractive(presenter))
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    "Timed out waiting for the authored stage-clear surface.");
                Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
                if (clearScene.IsValid()
                    && clearScene.isLoaded
                    && CountSceneComponents<StageClearScreenPresenter>(clearScene) == 1)
                {
                    presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
                    if (!resultObserverInvoked
                        && afterResultConfigured != null
                        && presenter.IsConfigured)
                    {
                        afterResultConfigured(presenter);
                        resultObserverInvoked = true;
                    }
                }

                yield return null;
            }

            Assert.That(
                afterResultConfigured == null || resultObserverInvoked,
                Is.True,
                "Configured StageClear entrance observer was not invoked before settle.");

            Scene loadedClearScene = SceneManager.GetSceneByName(StageClearSceneName);
            Assert.That(loadedClearScene.isLoaded, Is.True);
            Assert.That(CountSceneComponents<StageClearScreenPresenter>(loadedClearScene), Is.EqualTo(1));
            Assert.That(CountCombatSessionSurfaces(stationScene, visibleOnly: false), Is.EqualTo(1));
            Assert.That(CountCombatSessionSurfaces(stationScene, visibleOnly: true), Is.Zero);
            Assert.That(Time.timeScale, Is.Zero);

            GameObject combatHud = FindSceneObject(stationScene, "BossBarrageLaneReview_CombatHudCanvas");
            Assert.That(combatHud, Is.Not.Null);
            Assert.That(combatHud.activeSelf, Is.False, "The combat HUD must yield to the shared result surface.");

            Assert.That(resultPresenter.CommittedSummary, Is.Not.Null);
            Assert.That(resultPresenter.CommittedSummary.Outcome, Is.EqualTo(outcome));
            StageRunResultSummary factSummary = resultPresenter.CommittedSummary;
            Assert.That(factSummary.OutcomeFact, Is.Not.Null);
            Assert.That(
                factSummary.OutcomeFact.OutcomeDisposition,
                Is.EqualTo(outcome == StageRouteOutcome.Clear
                    ? StageOutcomeDisposition.Clear
                    : StageOutcomeDisposition.Fail));
            Assert.That(
                factSummary.OutcomeFact.ClearReason,
                Is.EqualTo(outcome == StageRouteOutcome.Clear
                    ? StageClearReason.BossTerminal
                    : StageClearReason.None));
            Assert.That(
                factSummary.OutcomeFact.FailureReason,
                Is.EqualTo(outcome == StageRouteOutcome.Fail
                    ? StageFailureReason.PlayerDefeated
                    : StageFailureReason.None));
            Assert.That(factSummary.OutcomeFact.TotalActiveElapsedMilliseconds, Is.GreaterThan(0));
            Assert.That(factSummary.OutcomeFact.CombatActiveElapsedMilliseconds, Is.GreaterThan(0));
            Assert.That(
                factSummary.OutcomeFact.TotalActiveElapsedMilliseconds,
                Is.GreaterThanOrEqualTo(factSummary.OutcomeFact.CombatActiveElapsedMilliseconds));
            Assert.That(factSummary.SegmentResultCount, Is.EqualTo(2));
            Assert.That(factSummary.GetSegmentResult(0).SegmentId, Is.EqualTo("corridor_intro_tutorial"));
            Assert.That(factSummary.GetSegmentResult(1).SegmentId, Is.EqualTo("station_entry_combat"));
            Assert.That(factSummary.GetSegmentResult(0).ExitReason, Is.EqualTo(StageSceneSegmentExitReason.Completed));
            Assert.That(factSummary.GetSegmentResult(1).ExitReason, Is.EqualTo(StageSceneSegmentExitReason.Completed));
            Assert.That(factSummary.TutorialRouteSummaryFact.RouteState, Is.EqualTo(StageTutorialRouteState.Completed));
            Assert.That(
                factSummary.TutorialRouteSummaryFact.PlanSemanticDigest,
                Is.EqualTo(StageRunFactVocabulary.OlympusCorridorTutorialPlanSemanticDigest));
            Assert.That(
                factSummary.CombatFacts.PlayerDownCount,
                Is.EqualTo(outcome == StageRouteOutcome.Fail ? 1 : 0));
            Assert.That(
                factSummary.TryGetSemanticProof(
                    StageRunFactVocabulary.SurvivalNoPlayerDownProofId,
                    out StageRunSemanticProofFact survivalProof),
                Is.EqualTo(outcome == StageRouteOutcome.Clear));
            if (survivalProof != null)
            {
                Assert.That(survivalProof.Qualified, Is.True);
            }

            Assert.That(resultPresenter.CommitReceipt, Is.Not.Null);
            Assert.That(
                resultPresenter.CommitReceipt.ResultSummaryDigest,
                Is.EqualTo(factSummary.ResultSummaryDigest));
            Assert.That(runContext.TerminalEpochClosureRecord, Is.Not.Null);
            Assert.That(runContext.TerminalFinalizationAuthority, Is.Not.Null);
            Assert.That(runContext.OwnerCoverageRecord, Is.Not.Null);
            Assert.That(runContext.TerminalOrRestartLatch, Is.EqualTo(StageTerminalOrRestartLatchState.TerminalWon));
            Assert.That(runContext.TerminalEpochClosureRecord.QueueDrainedAndSubjectsFinalized, Is.True);
            Assert.That(runContext.TerminalEpochClosureRecord.ActiveTokenInvalidated, Is.True);
            Assert.That(runContext.TerminalEpochClosureRecord.SubjectSnapshotCount, Is.EqualTo(2));
            Assert.That(runContext.TerminalEpochClosureRecord.CandidateCoverageCount, Is.EqualTo(1));
            Assert.That(
                runContext.TerminalFinalizationAuthority.TerminalEpochClosureRecordId,
                Is.EqualTo(runContext.TerminalEpochClosureRecord.TerminalEpochClosureRecordId));
            Assert.That(
                runContext.TerminalFinalizationAuthority.TerminalEpochClosureDigest,
                Is.EqualTo(runContext.TerminalEpochClosureRecord.CanonicalDigest));
            Assert.That(runContext.OwnerCoverageRecord.OwnerRowCount, Is.EqualTo(4));
            Assert.That(runContext.OwnerCoverageRecord.ZeroPendingFinalizationOwners, Is.True);
            for (int ownerIndex = 0; ownerIndex < runContext.OwnerCoverageRecord.OwnerRowCount; ownerIndex++)
            {
                TerminalFinalizationOwnerCoverageRow ownerRow =
                    runContext.OwnerCoverageRecord.GetOwnerRow(ownerIndex);
                Assert.That(
                    (int)ownerRow.OwnerKind,
                    Is.EqualTo(ownerIndex + 1),
                    "Terminal-finalization owners must remain in the frozen fixed order.");
                Assert.That(
                    ownerRow.Disposition,
                    Is.EqualTo(TerminalFinalizationOwnerDisposition.NotAdmitted));
                Assert.That(ownerRow.HasTypedReceipt, Is.False);
            }

            Assert.That(
                resultPresenter.CommitReceipt.TerminalFinalizationOwnerCoverageRecordId,
                Is.EqualTo(runContext.OwnerCoverageRecord.TerminalFinalizationOwnerCoverageRecordId));
            Assert.That(
                resultPresenter.CommitReceipt.TerminalFinalizationOwnerCoverageDigest,
                Is.EqualTo(runContext.OwnerCoverageRecord.CanonicalDigest));
            Assert.That(runContext.LifecycleState, Is.EqualTo(StageRunLifecycleState.Presented));
            Assert.That(runContext.TerminalRecordReceiptCount, Is.EqualTo(1));
            Assert.That(
                StageRunRuntime.TryCommitTerminalResolution(
                    encounter,
                    encounter.TerminalResolution,
                    out StageRunResultSummary duplicateSummary,
                    out StageRunResultCommitReceipt duplicateReceipt,
                    out string duplicateError),
                Is.True,
                duplicateError);
            Assert.That(duplicateSummary, Is.SameAs(resultPresenter.CommittedSummary));
            Assert.That(duplicateReceipt, Is.SameAs(resultPresenter.CommitReceipt));
            Assert.That(runContext.TerminalRecordReceiptCount, Is.EqualTo(1));

            string decisionPath =
                StageRunRuntime.GetResultCommitDecisionPathForTests(runContext.Identity.RunId);
            Assert.That(File.Exists(decisionPath), Is.True, "The run must own one durable decision slot.");
            string receiptDigest = resultPresenter.CommitReceipt.CanonicalDigest;
            string receiptChecksum = resultPresenter.CommitReceipt.EnvelopeChecksum;
            StageRunRuntime.ClearResultCommitMemoryCacheForTests();
            Assert.That(
                StageRunRuntime.TryReadCommittedResultDecision(
                    runContext.Identity.RunId,
                    out StageRunResultCommitReceipt recoveredReceipt,
                    out string recoveryError),
                Is.True,
                recoveryError);
            Assert.That(recoveredReceipt, Is.Not.SameAs(resultPresenter.CommitReceipt));
            Assert.That(recoveredReceipt.CanonicalDigest, Is.EqualTo(receiptDigest));
            Assert.That(recoveredReceipt.EnvelopeChecksum, Is.EqualTo(receiptChecksum));
            Assert.That(
                recoveredReceipt.Preparation.Kind,
                Is.EqualTo(StageRunResultCommitPreparationKind.NotRequired));
            Assert.That(recoveredReceipt.SummaryCommittedAtSequence, Is.EqualTo(1));

            Assert.That(
                StageRunRuntime.TryCommitTerminalResolution(
                    encounter,
                    encounter.TerminalResolution,
                    out StageRunResultSummary recoveredSummary,
                    out StageRunResultCommitReceipt reconciledReceipt,
                    out string reconcileError),
                Is.True,
                reconcileError);
            Assert.That(recoveredSummary, Is.SameAs(resultPresenter.CommittedSummary));
            Assert.That(reconciledReceipt.CanonicalDigest, Is.EqualTo(receiptDigest));
            Assert.That(reconciledReceipt.EnvelopeChecksum, Is.EqualTo(receiptChecksum));
            Assert.That(runContext.TerminalRecordReceiptCount, Is.EqualTo(1));
        }

        private static IEnumerator ReleaseStationEntryGuide(Scene stationScene)
        {
            ICombatEntryGuideGate gate = RequireSingleSceneInterface<ICombatEntryGuideGate>(stationScene);
            Assert.That(
                gate.State,
                Is.Not.EqualTo(CombatEntryGuideState.Released),
                "The initial non-playing state must not be mistaken for an already released guide.");
            bool observedPlaying = gate.State == CombatEntryGuideState.Playing;
            float deadline = Time.realtimeSinceStartup + 8f;
            while (gate.State != CombatEntryGuideState.Released)
            {
                observedPlaying |= gate.State == CombatEntryGuideState.Playing;
                Assert.That(
                    gate.State,
                    Is.Not.EqualTo(CombatEntryGuideState.Interrupted),
                    "Station entry guide was interrupted before gameplay release.");
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    "Timed out waiting for the Station entry guide to release gameplay.");
                if (gate.IsAwaitingAdvance)
                {
                    gate.RequestAdvance();
                }

                yield return null;
            }

            Assert.That(observedPlaying, Is.True, "Station guide never published its explicit Playing state.");
        }

        private static void ActivateHostedStationRoots(Scene scene)
        {
            OlympusCorridorCombatFlowController flow =
                RequireSingleSceneComponent<OlympusCorridorCombatFlowController>(scene);
            GameObject[] roots = ReadPrivateField<GameObject[]>(flow, "corridorCombatRoots");
            Assert.That(roots, Is.Not.Null.And.Not.Empty);
            for (int i = 0; i < roots.Length; i++)
            {
                Assert.That(roots[i], Is.Not.Null, $"Hosted Station root {i} is missing.");
                roots[i].SetActive(true);
            }
        }

        private static bool IsPresenterInteractive(StageClearScreenPresenter presenter)
        {
            if (presenter == null || !presenter.isActiveAndEnabled)
            {
                return false;
            }

            CanvasGroup canvasGroup = ReadPrivateField<CanvasGroup>(presenter, "canvasGroup");
            return canvasGroup.interactable && canvasGroup.blocksRaycasts;
        }

        private static void AssertPresenterRoutes(
            StageClearScreenPresenter presenter,
            StageRouteOutcome outcome)
        {
            Assert.That(presenter.IsConfigured, Is.True, presenter.LastActionError);
            Assert.That(presenter.ResultSummary, Is.Not.Null);
            Assert.That(presenter.ResultSummary.Outcome, Is.EqualTo(outcome));
            Assert.That(
                presenter.PrimaryActionId,
                Is.EqualTo(outcome == StageRouteOutcome.Clear
                    ? "olympus-invasion.replay"
                    : "olympus-invasion.retry"));
            Assert.That(presenter.LobbyActionId, Is.EqualTo("olympus-invasion.to-lobby"));
            Assert.That(presenter.ResultSummary.OfferedActionCount, Is.EqualTo(2));
            StageResultPresentationSnapshot presentation = presenter.PresentationSnapshot;
            Assert.That(presentation, Is.Not.Null);
            Assert.That(presentation.SourceResultSummaryId, Is.EqualTo(presenter.ResultSummary.ResultSummaryId));
            Assert.That(
                presentation.SourceResultSummaryDigest,
                Is.EqualTo(presenter.ResultSummary.ResultSummaryDigest));
            Assert.That(presentation.PlayableStageId, Is.EqualTo("OLYMPUS-INVASION-01"));
            Assert.That(presentation.Outcome, Is.EqualTo(outcome));
            Assert.That(presentation.ProfileId, Is.EqualTo("stage-result.olympus-invasion"));
            Assert.That(presentation.ProfileRevision, Is.EqualTo(1));
            Assert.That(presentation.LocaleId, Is.EqualTo("ko-KR"));
            Assert.That(presentation.CanonicalDigest, Is.Not.Empty);
            Assert.That(
                presentation.TotalActiveElapsedMilliseconds,
                Is.EqualTo(presenter.ResultSummary.OutcomeFact.TotalActiveElapsedMilliseconds));
            Assert.That(
                presentation.CombatActiveElapsedMilliseconds,
                Is.EqualTo(presenter.ResultSummary.OutcomeFact.CombatActiveElapsedMilliseconds));
            Assert.That(
                ReadPrivateField<Text>(presenter, "stageNameText").text,
                Is.EqualTo(presentation.StageTitle));
            Assert.That(
                ReadPrivateField<Text>(presenter, "stageNumberText").text,
                Is.EqualTo(presentation.StageCode));
            Assert.That(
                ReadPrivateField<Text>(presenter, "totalActiveTimeLabelText").text,
                Is.EqualTo(presentation.TotalActiveTimeLabel));
            Assert.That(
                ReadPrivateField<Text>(presenter, "totalActiveTimeValueText").text,
                Is.EqualTo(presentation.TotalActiveTimeValue));
            Assert.That(
                ReadPrivateField<Text>(presenter, "combatActiveTimeLabelText").text,
                Is.EqualTo(presentation.CombatActiveTimeLabel));
            Assert.That(
                ReadPrivateField<Text>(presenter, "combatActiveTimeValueText").text,
                Is.EqualTo(presentation.CombatActiveTimeValue));
            Assert.That(
                ReadPrivateField<Text>(presenter, "primaryActionText").text,
                Is.EqualTo(presentation.PrimaryActionLabel));
            Assert.That(
                ReadPrivateField<Text>(presenter, "lobbyActionText").text,
                Is.EqualTo(presentation.LobbyActionLabel));

            Text[] proofRowTexts = ReadPrivateField<Text[]>(presenter, "proofRowTexts");
            Assert.That(proofRowTexts, Has.Length.EqualTo(3));
            bool foundSurvivalRow = false;
            for (int proofIndex = 0; proofIndex < proofRowTexts.Length; proofIndex++)
            {
                bool expectedActive = proofIndex < presentation.ProofRowCount;
                Assert.That(proofRowTexts[proofIndex].gameObject.activeSelf, Is.EqualTo(expectedActive));
                if (!expectedActive)
                {
                    continue;
                }

                StageResultPresentationRowSnapshot row = presentation.GetProofRow(proofIndex);
                Assert.That(proofRowTexts[proofIndex].text, Is.EqualTo(row.LocalizedText));
                if (row.ProofId == StageRunFactVocabulary.SurvivalNoPlayerDownProofId)
                {
                    foundSurvivalRow = true;
                    Assert.That(
                        presenter.ResultSummary.TryGetSemanticProof(row.ProofId, out StageRunSemanticProofFact proof),
                        Is.True);
                    Assert.That(row.ProofDigest, Is.EqualTo(proof.CanonicalDigest));
                    Assert.That(proof.Qualified, Is.True);
                }
            }

            Assert.That(foundSurvivalRow, Is.EqualTo(outcome == StageRouteOutcome.Clear));

            StageResultPresentationCatalog catalog =
                ReadPrivateField<StageResultPresentationCatalog>(presenter, "presentationCatalog");
            Assert.That(catalog, Is.SameAs(LoadRequired<StageResultPresentationCatalog>(
                ResultPresentationCatalogPath)));
            Assert.That(catalog.TryValidate(out string catalogError), Is.True, catalogError);
            string committedDigest = presenter.ResultSummary.ResultSummaryDigest;
            Assert.That(
                catalog.TryCreateSnapshot(
                    presenter.ResultSummary,
                    "en-US",
                    out StageResultPresentationSnapshot englishPresentation,
                    out string englishError),
                Is.True,
                englishError);
            Assert.That(englishPresentation.LocaleId, Is.EqualTo("en-US"));
            Assert.That(englishPresentation.SourceResultSummaryDigest, Is.EqualTo(committedDigest));
            Assert.That(englishPresentation.StageTitle, Is.Not.EqualTo(presentation.StageTitle));
            Assert.That(englishPresentation.TotalActiveElapsedMilliseconds,
                Is.EqualTo(presentation.TotalActiveElapsedMilliseconds));
            Assert.That(presenter.ResultSummary.ResultSummaryDigest, Is.EqualTo(committedDigest));
            MonoBehaviour resolver =
                ReadPrivateField<MonoBehaviour>(presenter, "uiRouteResolverBehaviour");
            Assert.That(resolver, Is.Not.Null);
            Assert.That(
                resolver.GetType().FullName,
                Is.EqualTo("DimensionBrawl.UI.StageClear.StageRunUiRouteResolver"));
        }

        private static IEnumerator WaitForActiveScenePath(string expectedPath, float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!string.Equals(
                SceneManager.GetActiveScene().path.Replace('\\', '/'),
                expectedPath,
                StringComparison.Ordinal))
            {
                Assert.Less(
                    Time.realtimeSinceStartup,
                    deadline,
                    $"Timed out waiting for active scene {expectedPath}.");
                yield return null;
            }

            yield return null;
            yield return null;
            Assert.AreEqual(expectedPath, SceneManager.GetActiveScene().path.Replace('\\', '/'));
        }

        private static IEnumerator LoadSceneAndAssertPresenter(
            string scenePath,
            string presenterTypeName,
            bool ownsEventSystem)
        {
            EditorSceneManager.LoadSceneInPlayMode(
                scenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene scene = SceneManager.GetActiveScene();
            Assert.AreEqual(scenePath, scene.path.Replace('\\', '/'));

            Type presenterType = RequireProductType(presenterTypeName);
            int presenterCount = 0;
            UnityEngine.Object[] presenters = Resources.FindObjectsOfTypeAll(presenterType);
            for (int i = 0; i < presenters.Length; i++)
            {
                if (presenters[i] is Component presenter && presenter.gameObject.scene == scene)
                {
                    presenterCount++;
                }
            }

            Assert.AreEqual(1, presenterCount, $"{scenePath} must own exactly one {presenterType.Name}.");
            Assert.AreEqual(0, CountMissingScripts(scene), $"{scenePath} contains missing scripts.");
            Assert.AreEqual(
                ownsEventSystem ? 1 : 0,
                CountSceneComponents<EventSystem>(scene),
                ownsEventSystem
                    ? $"{scenePath} must own exactly one EventSystem."
                    : $"{scenePath} is additive and must use its host scene EventSystem.");
            if (ownsEventSystem)
            {
                Assert.GreaterOrEqual(
                    CountSceneComponents<Camera>(scene),
                    1,
                    $"{scenePath} must own at least one camera.");
                Assert.AreEqual(
                    1,
                    CountSceneComponents<AudioListener>(scene),
                    $"{scenePath} must own exactly one AudioListener.");
            }
            else
            {
                Assert.AreEqual(
                    0,
                    CountSceneComponents<Camera>(scene),
                    $"{scenePath} is additive and must use its host scene camera.");
                Assert.AreEqual(
                    0,
                    CountSceneComponents<AudioListener>(scene),
                    $"{scenePath} is additive and must use its host scene AudioListener.");
            }
        }

        private static int CountMissingScripts(Scene scene)
        {
            int missingScriptCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Transform[] transforms = roots[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    missingScriptCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transforms[transformIndex].gameObject);
                }
            }

            return missingScriptCount;
        }

        private static int CountSceneComponents<T>(Scene scene)
            where T : Component
        {
            int componentCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                componentCount += roots[i].GetComponentsInChildren<T>(true).Length;
            }

            return componentCount;
        }

        private static int CountSceneComponents(Scene scene, Type componentType)
        {
            int componentCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                componentCount += roots[i].GetComponentsInChildren(componentType, true).Length;
            }

            return componentCount;
        }

        private static T RequireSingleSceneComponent<T>(Scene scene)
            where T : Component
        {
            T found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                T[] components = roots[rootIndex].GetComponentsInChildren<T>(true);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Assert.That(found, Is.Null, $"{scene.path} owns duplicate {typeof(T).Name} components.");
                    found = components[componentIndex];
                }
            }

            Assert.That(found, Is.Not.Null, $"{scene.path} is missing {typeof(T).Name}.");
            return found;
        }

        private static StageDefinitionSceneBinding RequireSceneBinding(
            Scene scene,
            StageDefinitionProfile expectedDefinition)
        {
            StageDefinitionSceneBinding found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                StageDefinitionSceneBinding[] bindings =
                    roots[rootIndex].GetComponentsInChildren<StageDefinitionSceneBinding>(true);
                for (int bindingIndex = 0; bindingIndex < bindings.Length; bindingIndex++)
                {
                    StageDefinitionSceneBinding candidate = bindings[bindingIndex];
                    if (candidate.StageDefinition != expectedDefinition)
                    {
                        continue;
                    }

                    Assert.That(
                        found,
                        Is.Null,
                        $"{scene.path} owns duplicate bindings for {expectedDefinition.StageId}.");
                    found = candidate;
                }
            }

            Assert.That(
                found,
                Is.Not.Null,
                $"{scene.path} is missing the binding for {expectedDefinition.StageId}.");
            return found;
        }

        private static T RequireSingleSceneInterface<T>(Scene scene)
            where T : class
        {
            T found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    if (behaviours[behaviourIndex] is not T candidate)
                    {
                        continue;
                    }

                    Assert.That(found, Is.Null, $"{scene.path} owns duplicate {typeof(T).Name} implementations.");
                    found = candidate;
                }
            }

            Assert.That(found, Is.Not.Null, $"{scene.path} is missing {typeof(T).Name}.");
            return found;
        }

        private static Behaviour RequireSingleSceneBehaviour(Scene scene, Type behaviourType)
        {
            Behaviour found = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Component[] components = roots[rootIndex].GetComponentsInChildren(behaviourType, true);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Assert.That(found, Is.Null, $"{scene.path} owns duplicate {behaviourType.Name} components.");
                    found = components[componentIndex] as Behaviour;
                }
            }

            Assert.That(found, Is.Not.Null, $"{scene.path} is missing {behaviourType.Name}.");
            return found;
        }

        private static int CountCombatSessionSurfaces(Scene scene, bool visibleOnly)
        {
            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                MonoBehaviour[] behaviours = roots[rootIndex].GetComponentsInChildren<MonoBehaviour>(true);
                for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
                {
                    if (behaviours[behaviourIndex] is ICombatSessionOverlay surface
                        && (!visibleOnly || surface.IsVisible))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    if (string.Equals(transforms[transformIndex].name, objectName, StringComparison.Ordinal))
                    {
                        return transforms[transformIndex].gameObject;
                    }
                }
            }

            return null;
        }

        private static void AssertSerializedRoute(
            ScriptableObject routeTable,
            int routeId,
            string expectedSceneName,
            string expectedScenePath)
        {
            var serializedRouteTable = new SerializedObject(routeTable);
            SerializedProperty routes = serializedRouteTable.FindProperty("routes");
            Assert.NotNull(routes);

            for (int i = 0; i < routes.arraySize; i++)
            {
                SerializedProperty route = routes.GetArrayElementAtIndex(i);
                if (route.FindPropertyRelative("routeId").intValue != routeId)
                {
                    continue;
                }

                string sceneName = route.FindPropertyRelative("sceneName").stringValue;
                string scenePath = route.FindPropertyRelative("scenePath").stringValue;
                Assert.AreEqual(expectedSceneName, sceneName);
                Assert.AreEqual(expectedScenePath, scenePath.Replace('\\', '/'));
                Assert.NotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
                return;
            }

            Assert.Fail($"Missing route id {routeId} in {RouteTablePath}.");
        }

        private static int CountManifestPhysicalScene(
            object manifest,
            string scenePath)
        {
            int count = 0;
            int sceneCount = Convert.ToInt32(ReadProperty(manifest, "SceneCount"));
            for (int i = 0; i < sceneCount; i++)
            {
                if (string.Equals(
                        ReadProperty(GetIndexedValue(manifest, "GetScene", i), "ScenePath")
                            as string,
                        scenePath,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static object GetIndexedValue(object target, string methodName, int index)
        {
            Assert.That(target, Is.Not.Null);
            return RequireMethod(target.GetType(), methodName).Invoke(
                target,
                new object[] { index });
        }

        private static object GetCatalogStage(ScriptableObject catalog, int index)
        {
            return GetIndexedValue(catalog, "GetStage", index);
        }

        private static void AssertCatalogEntriesEquivalent(object expected, object actual)
        {
            string[] valueProperties =
            {
                "Id",
                "DisplayName",
                "Summary",
                "ThreatTags",
                "RecommendedSummonRole",
                "MockRewardPreview",
                "PresentationProvenance",
                "LoadingCardId",
                "CanonicalProjectionDigest"
            };
            for (int i = 0; i < valueProperties.Length; i++)
            {
                string propertyName = valueProperties[i];
                Assert.That(ReadProperty(actual, propertyName),
                    Is.EqualTo(ReadProperty(expected, propertyName)),
                    propertyName);
            }

            Assert.That(ReadProperty(actual, "PlayableStage"),
                Is.SameAs(ReadProperty(expected, "PlayableStage")));
        }

        private static bool TryComputeCatalogProjectionDigest(
            ScriptableObject catalog,
            int index,
            out string projectionDigest,
            out string rejectReason)
        {
            object[] arguments =
                { index, ResolveUiRouteId(CombatRouteId), null, null };
            bool accepted = (bool)RequireMethod(
                catalog.GetType(),
                "TryComputeCanonicalProjectionDigest").Invoke(catalog, arguments);
            projectionDigest = arguments[2] as string ?? string.Empty;
            rejectReason = arguments[3]?.ToString() ?? string.Empty;
            return accepted;
        }

        private static bool TryCreateCatalogProjection(
            ScriptableObject catalog,
            int index,
            out object projection,
            out string rejectReason)
        {
            object[] arguments =
                { index, ResolveUiRouteId(CombatRouteId), null, null };
            bool accepted = (bool)RequireMethod(
                catalog.GetType(),
                "TryCreateRouteProjection",
                4,
                typeof(int)).Invoke(catalog, arguments);
            projection = arguments[2];
            rejectReason = arguments[3]?.ToString() ?? string.Empty;
            return accepted;
        }

        private static bool TryCreateProductBuildManifest(
            ScriptableObject routeTable,
            ScriptableObject stageCatalog,
            string stageClearScenePath,
            out object manifest,
            out string rejectReason,
            out string error)
        {
            Type manifestType = RequireProductType(
                "DimensionBrawl.UI.UIProductBuildRouteManifest");
            object[] arguments =
                { routeTable, stageCatalog, stageClearScenePath, null, null, null };
            bool accepted = (bool)RequireStaticMethod(
                manifestType,
                "TryCreate").Invoke(null, arguments);
            manifest = arguments[3];
            rejectReason = arguments[4]?.ToString() ?? string.Empty;
            error = arguments[5] as string ?? string.Empty;
            return accepted;
        }

        private static ScriptableObject CreateDynamicRouteTable(ScriptableObject source)
        {
            Assert.That(source, Is.Not.Null);
            ScriptableObject dynamicRouteTable = UnityEngine.Object.Instantiate(source);
            dynamicRouteTable.name = "B0_4_ProductBuildRouteTable_TestFixture";
            dynamicRouteTable.hideFlags = HideFlags.HideAndDontSave;
            return dynamicRouteTable;
        }

        private static ScriptableObject CreateCatalogWithIndependentAdditionalRow(
            ScriptableObject productionCatalog,
            string additionalEntryId,
            PlayableStageDefinition additionalRoute)
        {
            Assert.That(productionCatalog, Is.Not.Null);
            int productionStageCount = Convert.ToInt32(
                ReadProperty(productionCatalog, "StageCount"));
            Assert.That(productionStageCount, Is.GreaterThanOrEqualTo(1));
            ScriptableObject catalog = UnityEngine.Object.Instantiate(productionCatalog);
            catalog.name = "B0_4_AdditionalRowStageCatalog_TestFixture";
            catalog.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var serializedCatalog = new SerializedObject(catalog);
                SerializedProperty stages = serializedCatalog.FindProperty("stages");
                stages.arraySize = productionStageCount + 1;
                ConfigureStageEntry(
                    stages.GetArrayElementAtIndex(productionStageCount),
                    additionalEntryId,
                    additionalRoute);
                serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

                Assert.That(
                    TryComputeCatalogProjectionDigest(
                        catalog,
                        productionStageCount,
                        out string projectionDigest,
                        out string rejectReason),
                    Is.True,
                    rejectReason);
                serializedCatalog.Update();
                stages.GetArrayElementAtIndex(productionStageCount)
                    .FindPropertyRelative("canonicalProjectionDigest")
                    .stringValue = projectionDigest;
                serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
                return catalog;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(catalog);
                throw;
            }
        }

        private static void AssertManifestRejectsDuplicateIndependentIdentity(
            ScriptableObject dynamicRouteTable,
            ScriptableObject productionCatalog,
            PlayableStageDefinition productionRoute,
            string fixtureId,
            string playableStageId,
            string resultDefinitionId,
            string progressionNodeId,
            string expectedReason)
        {
            IndependentManifestRouteFixture fixture = null;
            ScriptableObject catalog = null;
            try
            {
                fixture = CreateIndependentManifestRouteFixture(
                    productionRoute,
                    fixtureId,
                    playableStageId,
                    resultDefinitionId,
                    progressionNodeId,
                    StageSelectScenePath);
                int additionalCatalogIndex = Convert.ToInt32(
                    ReadProperty(productionCatalog, "StageCount"));
                catalog = CreateCatalogWithIndependentAdditionalRow(
                    productionCatalog,
                    "b0-4-manifest-" + fixtureId,
                    fixture.Route);
                Assert.That(
                    TryCreateCatalogProjection(
                        catalog,
                        additionalCatalogIndex,
                        out object projection,
                        out string projectionRejectReason),
                    Is.True,
                    projectionRejectReason);
                Assert.That(ReadProperty(projection, "PlayableStage"),
                    Is.SameAs(fixture.Route));

                Assert.That(
                    TryCreateProductBuildManifest(
                        dynamicRouteTable,
                        catalog,
                        StageClearScenePath,
                        out object manifest,
                        out string rejectReason,
                        out string error),
                    Is.False);
                Assert.That(manifest, Is.Null);
                Assert.That(rejectReason, Is.EqualTo(expectedReason), error);
                Assert.That(error, Is.Not.Empty);
            }
            finally
            {
                if (catalog != null)
                {
                    UnityEngine.Object.DestroyImmediate(catalog);
                }

                fixture?.Destroy();
            }
        }

        private static IndependentManifestRouteFixture CreateIndependentManifestRouteFixture(
            PlayableStageDefinition source,
            string fixtureId,
            string playableStageId,
            string resultDefinitionId,
            string progressionNodeId,
            string scenePath)
        {
            Assert.That(source, Is.Not.Null);
            Assert.That(source.ReferenceBlock, Is.Not.Null);
            Assert.That(source.ReferenceBlock.StageTemplate, Is.Not.Null);
            Assert.That(source.ResultProgressionJoin, Is.Not.Null);
            Assert.That(source.ResultProgressionJoin.ResultDefinition, Is.Not.Null);
            Assert.That(source.ResultProgressionJoin.ProgressionNode, Is.Not.Null);
            Assert.That(source.ResultProgressionJoin.ProgressionGraph, Is.Not.Null);
            string prefix = "b0-4.manifest." + fixtureId;
            StageDefinitionProfile stageDefinition = null;
            PlayableStageDefinition route = null;
            LinearStageTemplateProfile template = null;
            StageResultLocalizationTable localization = null;
            StageResultPresentationProfile presentationProfile = null;
            StageResultPresentationCatalog presentationCatalog = null;
            StageResultDefinition resultDefinition = null;
            StageProgressionNode progressionNode = null;
            StageProgressionGraph progressionGraph = null;

            try
            {
                StageResultProgressionJoinBlock sourceJoin = source.ResultProgressionJoin;
                StageResultDefinition sourceResultDefinition = sourceJoin.ResultDefinition;

                stageDefinition = UnityEngine.Object.Instantiate(
                    source.GetSceneSegment(0).StageDefinition);
                stageDefinition.name = prefix + ".stage-definition";
                stageDefinition.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(stageDefinition, "stageId", prefix + ".stage");
                SetPrivateField(stageDefinition, "mapScenePath", scenePath);

                route = UnityEngine.Object.Instantiate(source);
                route.name = prefix + ".route";
                route.hideFlags = HideFlags.HideAndDontSave;
                Assert.That(route.ReferenceBlock, Is.Not.SameAs(source.ReferenceBlock));
                Assert.That(route.ResultProgressionJoin,
                    Is.Not.SameAs(source.ResultProgressionJoin));
                Assert.That(route.TerminalResolutionPolicy,
                    Is.Not.SameAs(source.TerminalResolutionPolicy));
                SetPrivateField(route, "playableStageId", playableStageId);
                SetPrivateField(route, "routeRevision", 3);

                StageSceneSegmentRef segment = route.GetSceneSegment(0);
                SetPrivateField(segment, "segmentId", prefix + ".entry-final");
                SetPrivateField(segment, "sequenceIndex", 0);
                SetPrivateField(segment, "stageDefinition", stageDefinition);
                SetPrivateField(segment, "entryConditionId", "run.entry.admitted");
                SetPrivateField(
                    segment,
                    "entryConditionKind",
                    StageSegmentConditionKind
                        .RunEntrySnapshotValidatedAndFirstSegmentActivated);
                SetPrivateField(segment, "exitConditionId", prefix + ".terminal");
                SetPrivateField(
                    segment,
                    "exitConditionKind",
                    StageSegmentConditionKind
                        .StationTerminalQueueDrainedSubjectsFinalizedAndEvidenceMatched);
                SetPrivateField(segment, "handoffPolicy", StageSceneHandoffPolicy.ReturnToOwner);
                SetPrivateField(segment, "successorKind", StageSegmentSuccessorKind.None);
                SetPrivateField(
                    segment,
                    "destinationSceneKind",
                    StageSegmentDestinationSceneKind.None);
                SetPrivateField(
                    segment,
                    "transitionTokenKind",
                    StageSegmentTransitionTokenKind.None);
                SetPrivateField(
                    segment,
                    "loaderGenerationKind",
                    StageSegmentLoaderGenerationKind.None);
                SetPrivateField(
                    segment,
                    "navigationAuthorityKind",
                    StageSegmentNavigationAuthorityKind.None);
                SetPrivateField(
                    segment,
                    "returnOwnerKind",
                    StageSegmentReturnOwnerKind.P1AStageRunRouteOwner);
                SetPrivateField(
                    segment,
                    "returnOwnerReceiptPolicy",
                    StageReturnOwnerReceiptPolicy
                        .ExactTerminalRecordExactlyOnceToTerminalFinalizingCommittedPresented);
                SetPrivateField(route, "sceneSegments", new[] { segment });

                var actionIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int actionIndex = 0;
                    actionIndex < route.TerminalActionCount;
                    actionIndex++)
                {
                    StageRouteActionRef action = route.GetTerminalAction(actionIndex);
                    string authoredActionId = action.ActionId;
                    string independentActionId =
                        prefix + "." + action.ActionKind.ToString().ToLowerInvariant();
                    actionIdMap.Add(authoredActionId, independentActionId);
                    SetPrivateField(action, "actionId", independentActionId);
                    if (action.ActionKind == StageRouteActionKind.Replay
                        || action.ActionKind == StageRouteActionKind.Retry)
                    {
                        SetPrivateField(action, "targetPlayableStageId", playableStageId);
                    }
                }

                SetPrivateField(route, "canonicalRouteDigest", string.Empty);
                SetPrivateField(
                    route,
                    "canonicalRouteDigest",
                    route.ComputeCanonicalRouteDigest());
                Assert.That(route.CanonicalRouteDigest,
                    Is.Not.EqualTo(source.CanonicalRouteDigest));

                template = UnityEngine.Object.Instantiate(
                    source.ReferenceBlock.StageTemplate);
                template.name = prefix + ".template";
                template.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(template, "stageTemplateId", prefix + ".template");
                StageTemplateRouteSegmentRef templateSegment =
                    ReadPrivateField<StageTemplateRouteSegmentRef[]>(
                        template,
                        "canonicalRouteSegments")[0];
                SetPrivateField(
                    templateSegment,
                    "templateSegmentId",
                    prefix + ".template-segment");
                SetPrivateField(
                    templateSegment,
                    "routeSegmentId",
                    segment.SegmentId);
                SetPrivateField(templateSegment, "routeSequenceIndex", 0);
                SetPrivateField(
                    template,
                    "canonicalRouteSegments",
                    new[] { templateSegment });
                SetPrivateField(template, "canonicalTemplateDigest", string.Empty);
                SetPrivateField(
                    template,
                    "canonicalTemplateDigest",
                    template.ComputeCanonicalTemplateDigest());
                Assert.That(
                    template.CanonicalTemplateDigest,
                    Is.Not.EqualTo(source.ReferenceBlock.StageTemplate.CanonicalTemplateDigest));
                SetPrivateField(route.ReferenceBlock, "stageTemplate", template);
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalReferenceDigest",
                    string.Empty);
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalReferenceDigest",
                    route.ComputeCanonicalReferenceDigest());
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalBriefingDigest",
                    string.Empty);
                Assert.That(
                    route.TryComputeCanonicalBriefingDigest(
                        out string briefingDigest,
                        out StageBriefingBuildRejectReason briefingRejectReason),
                    Is.True,
                    briefingRejectReason.ToString());
                SetPrivateField(
                    route.ReferenceBlock,
                    "canonicalBriefingDigest",
                    briefingDigest);

                localization = UnityEngine.Object.Instantiate(
                    sourceResultDefinition.LocalizationTable);
                localization.name = prefix + ".localization";
                localization.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(localization, "tableId", prefix + ".localization");
                Assert.That(
                    localization.TryValidate(out string localizationError),
                    Is.True,
                    localizationError);
                Assert.That(
                    localization.ComputeCanonicalDigest(),
                    Is.Not.EqualTo(
                        sourceResultDefinition.LocalizationTable.ComputeCanonicalDigest()));

                presentationProfile = UnityEngine.Object.Instantiate(
                    sourceResultDefinition.PresentationProfile);
                presentationProfile.name = prefix + ".presentation-profile";
                presentationProfile.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    presentationProfile,
                    "profileId",
                    prefix + ".presentation-profile");
                SetPrivateField(
                    presentationProfile,
                    "playableStageId",
                    playableStageId);
                SetPrivateField(presentationProfile, "stageCode", fixtureId.ToUpperInvariant());
                Assert.That(
                    presentationProfile.TryValidate(
                        localization,
                        out string profileError),
                    Is.True,
                    profileError);
                Assert.That(
                    presentationProfile.ComputeCanonicalDigest(),
                    Is.Not.EqualTo(
                        sourceResultDefinition.PresentationProfile
                            .ComputeCanonicalDigest()));

                presentationCatalog = UnityEngine.Object.Instantiate(
                    sourceResultDefinition.CanonicalPresentationCatalog);
                presentationCatalog.name = prefix + ".presentation-catalog";
                presentationCatalog.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    presentationCatalog,
                    "catalogId",
                    prefix + ".presentation-catalog");
                SetPrivateField(
                    presentationCatalog,
                    "localizationTable",
                    localization);
                SetPrivateField(
                    presentationCatalog,
                    "profiles",
                    new[] { presentationProfile });
                Assert.That(
                    presentationCatalog.TryValidate(out string presentationCatalogError),
                    Is.True,
                    presentationCatalogError);

                resultDefinition = UnityEngine.Object.Instantiate(sourceResultDefinition);
                resultDefinition.name = prefix + ".result-definition";
                resultDefinition.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    resultDefinition,
                    "resultDefinitionId",
                    resultDefinitionId);
                SetPrivateField(resultDefinition, "playableStageId", playableStageId);
                SetPrivateField(
                    resultDefinition,
                    "canonicalPresentationCatalog",
                    presentationCatalog);
                SetPrivateField(
                    resultDefinition,
                    "presentationProfile",
                    presentationProfile);
                SetPrivateField(resultDefinition, "localizationTable", localization);
                StageResultActionPresentationMapping[] mappings =
                    ReadPrivateField<StageResultActionPresentationMapping[]>(
                        resultDefinition,
                        "actionMappings");
                for (int mappingIndex = 0;
                    mappingIndex < mappings.Length;
                    mappingIndex++)
                {
                    StageResultActionPresentationMapping mapping = mappings[mappingIndex];
                    Assert.That(
                        actionIdMap.TryGetValue(
                            mapping.ActionId,
                            out string independentActionId),
                        Is.True,
                        mapping.ActionId);
                    SetPrivateField(mapping, "actionId", independentActionId);
                }

                SetPrivateField(resultDefinition, "evaluationContentDigest", string.Empty);
                SetPrivateField(resultDefinition, "presentationBindingDigest", string.Empty);
                SetPrivateField(resultDefinition, "presentationSourceDigest", string.Empty);
                Assert.That(
                    resultDefinition.TryComputeCanonicalDigests(
                        out string resultEvaluationDigest,
                        out string resultBindingDigest,
                        out string resultSourceDigest,
                        out string resultDigestError),
                    Is.True,
                    resultDigestError);
                SetPrivateField(
                    resultDefinition,
                    "evaluationContentDigest",
                    resultEvaluationDigest);
                SetPrivateField(
                    resultDefinition,
                    "presentationBindingDigest",
                    resultBindingDigest);
                SetPrivateField(
                    resultDefinition,
                    "presentationSourceDigest",
                    resultSourceDigest);
                Assert.That(resultEvaluationDigest,
                    Is.Not.EqualTo(sourceResultDefinition.EvaluationContentDigest));
                Assert.That(resultBindingDigest,
                    Is.Not.EqualTo(sourceResultDefinition.PresentationBindingDigest));
                Assert.That(resultSourceDigest,
                    Is.Not.EqualTo(sourceResultDefinition.PresentationSourceDigest));
                Assert.That(
                    resultDefinition.TryCreateSnapshot(
                        out _,
                        out string resultSnapshotError),
                    Is.True,
                    resultSnapshotError);

                progressionNode = UnityEngine.Object.Instantiate(sourceJoin.ProgressionNode);
                progressionNode.name = prefix + ".progression-node";
                progressionNode.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    progressionNode,
                    "progressionNodeId",
                    progressionNodeId);
                SetPrivateField(
                    progressionNode,
                    "prerequisites",
                    Array.Empty<StageProgressionPrerequisiteRef>());
                SetPrivateField(
                    progressionNode,
                    "recommendedNext",
                    Array.Empty<StageProgressionRecommendedNextRef>());
                SetPrivateField(progressionNode, "playableStageId", playableStageId);
                SetPrivateField(progressionNode, "routeRevision", route.RouteRevision);
                SetPrivateField(
                    progressionNode,
                    "canonicalRouteDigest",
                    route.CanonicalRouteDigest);
                SetPrivateField(
                    progressionNode,
                    "progressionGraphId",
                    prefix + ".progression-graph");
                SetPrivateField(progressionNode, "contentDigest", string.Empty);
                SetPrivateField(progressionNode, "bindingDigest", string.Empty);
                Assert.That(
                    progressionNode.TryComputeCanonicalDigests(
                        out string nodeContentDigest,
                        out string nodeBindingDigest,
                        out string nodeDigestError),
                    Is.True,
                    nodeDigestError);
                SetPrivateField(
                    progressionNode,
                    "contentDigest",
                    nodeContentDigest);
                SetPrivateField(
                    progressionNode,
                    "bindingDigest",
                    nodeBindingDigest);
                Assert.That(nodeBindingDigest,
                    Is.Not.EqualTo(sourceJoin.ProgressionNode.BindingDigest));

                progressionGraph = UnityEngine.Object.Instantiate(sourceJoin.ProgressionGraph);
                progressionGraph.name = prefix + ".progression-graph";
                progressionGraph.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(
                    progressionGraph,
                    "progressionGraphId",
                    prefix + ".progression-graph");
                SetPrivateField(
                    progressionGraph,
                    "nodes",
                    new[] { progressionNode });
                SetPrivateField(progressionGraph, "canonicalDigest", string.Empty);
                Assert.That(
                    progressionGraph.TryComputeCanonicalDigest(
                        out string graphDigest,
                        out string graphDigestError),
                    Is.True,
                    graphDigestError);
                SetPrivateField(progressionGraph, "canonicalDigest", graphDigest);
                Assert.That(graphDigest,
                    Is.Not.EqualTo(sourceJoin.ProgressionGraph.CanonicalDigest));

                SetPrivateField(
                    route.ResultProgressionJoin,
                    "resultDefinition",
                    resultDefinition);
                SetPrivateField(
                    route.ResultProgressionJoin,
                    "canonicalPresentationCatalog",
                    presentationCatalog);
                SetPrivateField(
                    route.ResultProgressionJoin,
                    "progressionNode",
                    progressionNode);
                SetPrivateField(
                    route.ResultProgressionJoin,
                    "progressionGraph",
                    progressionGraph);
                SetPrivateField(
                    route.ResultProgressionJoin,
                    "canonicalDigest",
                    string.Empty);
                Assert.That(
                    route.TryComputeResultProgressionJoinDigest(
                        out string joinDigest,
                        out string joinDigestError),
                    Is.True,
                    joinDigestError);
                SetPrivateField(
                    route.ResultProgressionJoin,
                    "canonicalDigest",
                    joinDigest);
                Assert.That(joinDigest,
                    Is.Not.EqualTo(sourceJoin.CanonicalDigest));

                Assert.That(
                    StageRunRouteSnapshot.TryCreate(
                        route,
                        out StageRunRouteSnapshot routeSnapshot,
                        out string routeError),
                    Is.True,
                    routeError);
                Assert.That(routeSnapshot.SegmentCount, Is.EqualTo(1));
                Assert.That(routeSnapshot.GetSegment(0).ScenePath, Is.EqualTo(scenePath));
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out StageRunResultProgressionJoinSnapshot joinSnapshot,
                        out string joinError),
                    Is.True,
                    joinError);
                Assert.That(
                    joinSnapshot.TryValidateIntegrity(out string integrityError),
                    Is.True,
                    integrityError);
                Assert.That(route.ReferenceBlock.StageTemplate, Is.SameAs(template));
                Assert.That(
                    resultDefinition.CanonicalPresentationCatalog,
                    Is.SameAs(presentationCatalog));
                Assert.That(resultDefinition.PresentationProfile,
                    Is.SameAs(presentationProfile));
                Assert.That(resultDefinition.LocalizationTable, Is.SameAs(localization));
                Assert.That(presentationCatalog.LocalizationTable, Is.SameAs(localization));
                Assert.That(route.ResultProgressionJoin.ProgressionNode,
                    Is.SameAs(progressionNode));
                Assert.That(route.ResultProgressionJoin.ProgressionGraph,
                    Is.SameAs(progressionGraph));
                Assert.That(stageDefinition,
                    Is.Not.SameAs(source.GetSceneSegment(0).StageDefinition));
                Assert.That(template,
                    Is.Not.SameAs(source.ReferenceBlock.StageTemplate));
                Assert.That(localization,
                    Is.Not.SameAs(sourceResultDefinition.LocalizationTable));
                Assert.That(presentationProfile,
                    Is.Not.SameAs(sourceResultDefinition.PresentationProfile));
                Assert.That(presentationCatalog,
                    Is.Not.SameAs(sourceResultDefinition.CanonicalPresentationCatalog));
                Assert.That(resultDefinition, Is.Not.SameAs(sourceResultDefinition));
                Assert.That(progressionNode, Is.Not.SameAs(sourceJoin.ProgressionNode));
                Assert.That(progressionGraph, Is.Not.SameAs(sourceJoin.ProgressionGraph));

                return new IndependentManifestRouteFixture(
                    route,
                    stageDefinition,
                    template,
                    localization,
                    presentationProfile,
                    presentationCatalog,
                    resultDefinition,
                    progressionNode,
                    progressionGraph);
            }
            catch
            {
                if (route != null)
                {
                    UnityEngine.Object.DestroyImmediate(route);
                }

                if (progressionGraph != null)
                {
                    UnityEngine.Object.DestroyImmediate(progressionGraph);
                }

                if (progressionNode != null)
                {
                    UnityEngine.Object.DestroyImmediate(progressionNode);
                }

                if (resultDefinition != null)
                {
                    UnityEngine.Object.DestroyImmediate(resultDefinition);
                }

                if (presentationCatalog != null)
                {
                    UnityEngine.Object.DestroyImmediate(presentationCatalog);
                }

                if (presentationProfile != null)
                {
                    UnityEngine.Object.DestroyImmediate(presentationProfile);
                }

                if (localization != null)
                {
                    UnityEngine.Object.DestroyImmediate(localization);
                }

                if (template != null)
                {
                    UnityEngine.Object.DestroyImmediate(template);
                }

                if (stageDefinition != null)
                {
                    UnityEngine.Object.DestroyImmediate(stageDefinition);
                }

                throw;
            }
        }

        private static StageDefinitionProfile CreateStageDefinition(string stageId, string scenePath)
        {
            StageDefinitionProfile definition = ScriptableObject.CreateInstance<StageDefinitionProfile>();
            var serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("stageId").stringValue = stageId;
            serializedDefinition.FindProperty("mapScenePath").stringValue = scenePath;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static StageResultLocalizedString FindLocalizationEntry(
            StageResultLocalizationTable table,
            string localeId,
            string key)
        {
            for (int localeIndex = 0; localeIndex < table.LocaleCount; localeIndex++)
            {
                StageResultLocaleDefinition locale = table.GetLocale(localeIndex);
                if (!string.Equals(locale.LocaleId, localeId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                for (int entryIndex = 0; entryIndex < locale.EntryCount; entryIndex++)
                {
                    StageResultLocalizedString entry = locale.GetEntry(entryIndex);
                    if (string.Equals(entry.Key, key, StringComparison.Ordinal))
                    {
                        return entry;
                    }
                }
            }

            Assert.Fail($"Missing localization entry {localeId}:{key}.");
            return null;
        }

        private static PlayableStageDefinition CreatePlayableStageDefinition(
            string playableStageId,
            string entrySegmentId,
            StageDefinitionProfile entryDefinition)
        {
            PlayableStageDefinition source = LoadRequired<PlayableStageDefinition>(
                PlayableStagePath);
            PlayableStageDefinition route = UnityEngine.Object.Instantiate(source);
            route.name = playableStageId;
            route.hideFlags = HideFlags.HideAndDontSave;
            SetPrivateField(route, "terminalResolutionPolicy", null);
            var serializedRoute = new SerializedObject(route);
            serializedRoute.FindProperty("schemaVersion").intValue = 1;
            serializedRoute.FindProperty("playableStageId").stringValue = source.PlayableStageId;
            serializedRoute.FindProperty("routeRevision").intValue = 1;
            SerializedProperty segments = serializedRoute.FindProperty("sceneSegments");
            segments.arraySize = 0;
            segments.arraySize = 1;
            SerializedProperty entry = segments.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("segmentId").stringValue = entrySegmentId;
            entry.FindPropertyRelative("sequenceIndex").intValue = 0;
            entry.FindPropertyRelative("stageDefinition").objectReferenceValue = entryDefinition;
            serializedRoute.FindProperty("terminalActions").arraySize = 0;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            serializedRoute.Update();
            serializedRoute.FindProperty("canonicalRouteDigest").stringValue = route.ComputeCanonicalRouteDigest();
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            LinearStageTemplateProfile template = CreateStageTemplate(
                source.PlayableStageId,
                entrySegmentId);
            serializedRoute.Update();
            SerializedProperty reference = serializedRoute.FindProperty("referenceBlock");
            reference.FindPropertyRelative("enabled").boolValue = true;
            reference.FindPropertyRelative("schemaVersion").intValue = 1;
            reference.FindPropertyRelative("revision").intValue = 1;
            reference.FindPropertyRelative("canonicalReferenceDigest").stringValue = string.Empty;
            reference.FindPropertyRelative("stageTemplate").objectReferenceValue = template;
            reference.FindPropertyRelative("briefingSchemaVersion").intValue = 1;
            reference.FindPropertyRelative("briefingRevision").intValue = 1;
            reference.FindPropertyRelative("canonicalBriefingDigest").stringValue = string.Empty;
            reference.FindPropertyRelative("storyEntryDisposition").intValue =
                (int)StageReferenceDisposition.None;
            reference.FindPropertyRelative("storyExitDisposition").intValue =
                (int)StageReferenceDisposition.NoFinalSegmentExitPresentationAuthored;
            reference.FindPropertyRelative("resultDefinitionDisposition").intValue =
                (int)StageReferenceDisposition.NotAuthoredForCurrentSchema;
            reference.FindPropertyRelative("progressionNodeDisposition").intValue =
                (int)StageReferenceDisposition.NotAuthoredForCurrentSchema;
            reference.FindPropertyRelative("ruleSetDisposition").intValue =
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema;
            reference.FindPropertyRelative("modifierDisposition").intValue =
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema;
            reference.FindPropertyRelative("enemyVariantDisposition").intValue =
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema;
            reference.FindPropertyRelative("tutorialCourseDisposition").intValue =
                (int)StageReferenceDisposition.NotAdmittedByCurrentSchema;
            reference.FindPropertyRelative("rewardPlanDisposition").intValue =
                (int)StageReferenceDisposition.NoVerifiedSource;
            reference.FindPropertyRelative("activeRunRestartPolicyDisposition").intValue =
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema;
            reference.FindPropertyRelative("activeRunRestartPolicyDigest").stringValue = string.Empty;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            serializedRoute.Update();
            serializedRoute.FindProperty("referenceBlock")
                .FindPropertyRelative("canonicalReferenceDigest")
                .stringValue = route.ComputeCanonicalReferenceDigest();
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                route.TryComputeCanonicalBriefingDigest(
                    out string briefingDigest,
                    out StageBriefingBuildRejectReason computeRejectReason),
                Is.True,
                computeRejectReason.ToString());
            serializedRoute.Update();
            serializedRoute.FindProperty("referenceBlock")
                .FindPropertyRelative("canonicalBriefingDigest")
                .stringValue = briefingDigest;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                route.TryCreateBriefingReadModel(
                    out _,
                    out StageBriefingBuildRejectReason createRejectReason),
                Is.True,
                createRejectReason.ToString());

            StageResultProgressionJoinBlock sourceJoin = source.ResultProgressionJoin;
            Assert.That(sourceJoin, Is.Not.Null);
            Assert.That(sourceJoin.ResultDefinition, Is.Not.Null);
            Assert.That(sourceJoin.ProgressionNode, Is.Not.Null);
            Assert.That(sourceJoin.ProgressionGraph, Is.Not.Null);

            StageProgressionNode node = UnityEngine.Object.Instantiate(
                sourceJoin.ProgressionNode);
            node.hideFlags = HideFlags.HideAndDontSave;
            var serializedNode = new SerializedObject(node);
            serializedNode.FindProperty("playableStageId").stringValue = route.PlayableStageId;
            serializedNode.FindProperty("routeRevision").intValue = route.RouteRevision;
            serializedNode.FindProperty("canonicalRouteDigest").stringValue =
                route.CanonicalRouteDigest;
            serializedNode.FindProperty("contentDigest").stringValue = string.Empty;
            serializedNode.FindProperty("bindingDigest").stringValue = string.Empty;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                node.TryComputeCanonicalDigests(
                    out string nodeContentDigest,
                    out string nodeBindingDigest,
                    out string nodeDigestError),
                Is.True,
                nodeDigestError);
            serializedNode.Update();
            serializedNode.FindProperty("contentDigest").stringValue = nodeContentDigest;
            serializedNode.FindProperty("bindingDigest").stringValue = nodeBindingDigest;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();

            StageProgressionGraph graph = UnityEngine.Object.Instantiate(
                sourceJoin.ProgressionGraph);
            graph.hideFlags = HideFlags.HideAndDontSave;
            var serializedGraph = new SerializedObject(graph);
            SerializedProperty graphNodes = serializedGraph.FindProperty("nodes");
            graphNodes.arraySize = 1;
            graphNodes.GetArrayElementAtIndex(0).objectReferenceValue = node;
            serializedGraph.FindProperty("canonicalDigest").stringValue = string.Empty;
            serializedGraph.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                graph.TryComputeCanonicalDigest(
                    out string graphDigest,
                    out string graphDigestError),
                Is.True,
                graphDigestError);
            serializedGraph.Update();
            serializedGraph.FindProperty("canonicalDigest").stringValue = graphDigest;
            serializedGraph.ApplyModifiedPropertiesWithoutUndo();

            serializedRoute.Update();
            SerializedProperty join = serializedRoute.FindProperty("resultProgressionJoin");
            join.FindPropertyRelative("present").boolValue = true;
            join.FindPropertyRelative("schemaVersion").intValue = 1;
            join.FindPropertyRelative("revision").intValue = 1;
            join.FindPropertyRelative("semanticCoupling").intValue =
                (int)StageResultJoinSemanticCoupling
                    .PresentationAuditSidecarOutsideP1ASemanticResult;
            join.FindPropertyRelative("resultDefinitionDisposition").intValue =
                (int)StageResultProgressionReferenceDisposition.Present;
            join.FindPropertyRelative("resultDefinition").objectReferenceValue =
                sourceJoin.ResultDefinition;
            join.FindPropertyRelative("progressionNodeDisposition").intValue =
                (int)StageResultProgressionReferenceDisposition.Present;
            join.FindPropertyRelative("progressionNode").objectReferenceValue = node;
            join.FindPropertyRelative("progressionGraphDisposition").intValue =
                (int)StageResultProgressionReferenceDisposition.Present;
            join.FindPropertyRelative("progressionGraph").objectReferenceValue = graph;
            join.FindPropertyRelative("rewardPlanDisposition").intValue =
                (int)StageResultProgressionReferenceDisposition.NotAdmittedByCurrentSchema;
            join.FindPropertyRelative("rewardPlanDigest").stringValue = string.Empty;
            join.FindPropertyRelative("canonicalDigest").stringValue = string.Empty;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                route.TryComputeResultProgressionJoinDigest(
                    out string joinDigest,
                    out string joinDigestError),
                Is.True,
                joinDigestError);
            serializedRoute.Update();
            serializedRoute.FindProperty("resultProgressionJoin")
                .FindPropertyRelative("canonicalDigest")
                .stringValue = joinDigest;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                StageRunResultProgressionJoinSnapshot.TryCreate(
                    route,
                    out _,
                    out string joinError),
                Is.True,
                joinError);
            return route;
        }

        private static StageProgressionNode CreateProgressionNodeFixture(
            StageProgressionNode template,
            string nodeId,
            string graphId,
            List<UnityEngine.Object> created)
        {
            StageProgressionNode node = UnityEngine.Object.Instantiate(template);
            node.hideFlags = HideFlags.HideAndDontSave;
            SetPrivateField(node, "progressionNodeId", nodeId);
            SetPrivateField(node, "progressionGraphId", graphId);
            SetPrivateField(
                node,
                "progressionGraphRevision",
                template.ProgressionGraphRevision);
            SetPrivateField(node, "prerequisites", Array.Empty<StageProgressionPrerequisiteRef>());
            SetPrivateField(node, "recommendedNext", Array.Empty<StageProgressionRecommendedNextRef>());
            SetPrivateField(node, "contentDigest", string.Empty);
            SetPrivateField(node, "bindingDigest", string.Empty);
            created.Add(node);
            return node;
        }

        private static void ConfigureProgressionNodeEdges(
            StageProgressionNode node,
            StageProgressionNode[] prerequisiteTargets,
            int[] prerequisiteRevisions,
            StageProgressionNode[] recommendedTargets,
            int[] recommendedRevisions)
        {
            Assert.That(prerequisiteTargets.Length, Is.EqualTo(prerequisiteRevisions.Length));
            Assert.That(recommendedTargets.Length, Is.EqualTo(recommendedRevisions.Length));

            var prerequisites = new StageProgressionPrerequisiteRef[prerequisiteTargets.Length];
            for (int i = 0; i < prerequisites.Length; i++)
            {
                var edge = new StageProgressionPrerequisiteRef();
                SetPrivateField(edge, "targetProgressionNode", prerequisiteTargets[i]);
                SetPrivateField(edge, "targetProgressionNodeRevision", prerequisiteRevisions[i]);
                SetPrivateField(edge, "requirementKind", StageProgressionRequirementKind.Cleared);
                SetPrivateField(edge, "requiredObjectiveId", string.Empty);
                prerequisites[i] = edge;
            }

            var recommended = new StageProgressionRecommendedNextRef[recommendedTargets.Length];
            for (int i = 0; i < recommended.Length; i++)
            {
                var edge = new StageProgressionRecommendedNextRef();
                SetPrivateField(edge, "targetProgressionNode", recommendedTargets[i]);
                SetPrivateField(edge, "targetProgressionNodeRevision", recommendedRevisions[i]);
                recommended[i] = edge;
            }

            SetPrivateField(node, "prerequisites", prerequisites);
            SetPrivateField(node, "recommendedNext", recommended);
            SetPrivateField(node, "contentDigest", string.Empty);
            SetPrivateField(node, "bindingDigest", string.Empty);
        }

        private static void SealProgressionNode(StageProgressionNode node)
        {
            Assert.That(
                node.TryComputeCanonicalDigests(
                    out string contentDigest,
                    out string bindingDigest,
                    out string error),
                Is.True,
                error);
            SetPrivateField(node, "contentDigest", contentDigest);
            SetPrivateField(node, "bindingDigest", bindingDigest);
        }

        private static StageProgressionGraph CreateProgressionGraphFixture(
            StageProgressionGraph template,
            string graphId,
            StageProgressionNode[] nodes,
            List<UnityEngine.Object> created)
        {
            StageProgressionGraph graph = UnityEngine.Object.Instantiate(template);
            graph.hideFlags = HideFlags.HideAndDontSave;
            SetPrivateField(graph, "progressionGraphId", graphId);
            SetPrivateField(graph, "nodes", nodes);
            SetPrivateField(graph, "canonicalDigest", string.Empty);
            created.Add(graph);
            return graph;
        }

        private static LinearStageTemplateProfile CreateStageTemplate(
            string playableStageId,
            string entrySegmentId)
        {
            LinearStageTemplateProfile template =
                ScriptableObject.CreateInstance<LinearStageTemplateProfile>();
            template.hideFlags = HideFlags.HideAndDontSave;
            var serializedTemplate = new SerializedObject(template);
            serializedTemplate.FindProperty("stageTemplateId").stringValue =
                playableStageId + ".template";
            serializedTemplate.FindProperty("displayName").stringValue = playableStageId;
            serializedTemplate.FindProperty("templateKind").intValue =
                (int)LinearStageTemplateKind.TutorialRun;
            serializedTemplate.FindProperty("templateSchemaVersion").intValue = 1;
            serializedTemplate.FindProperty("templateRevision").intValue = 1;
            serializedTemplate.FindProperty("canonicalTemplateDigest").stringValue = string.Empty;
            serializedTemplate.FindProperty("titleDisposition").intValue =
                (int)StageBriefingValueDisposition.Present;
            serializedTemplate.FindProperty("title").stringValue = playableStageId;
            serializedTemplate.FindProperty("titleLocalizationKeyDisposition").intValue =
                (int)StageBriefingValueDisposition.NoVerifiedSource;
            serializedTemplate.FindProperty("titleLocalizationKey").stringValue = string.Empty;
            serializedTemplate.FindProperty("objectiveDisposition").intValue =
                (int)StageBriefingValueDisposition.Present;
            serializedTemplate.FindProperty("objective").stringValue = playableStageId + " summary";
            serializedTemplate.FindProperty("combatLessonDisposition").intValue =
                (int)StageBriefingValueDisposition.Present;
            serializedTemplate.FindProperty("combatLesson").stringValue =
                playableStageId + " combat lesson";
            serializedTemplate.FindProperty("recommendedPowerDisposition").intValue =
                (int)StageBriefingValueDisposition.NoVerifiedSource;
            serializedTemplate.FindProperty("recommendedPowerTier").intValue = 0;
            serializedTemplate.FindProperty("recommendedLoadoutDisposition").intValue =
                (int)StageBriefingValueDisposition.NoVerifiedSource;
            serializedTemplate.FindProperty("recommendedLoadout").stringValue = string.Empty;
            serializedTemplate.FindProperty("targetRunDurationDisposition").intValue =
                (int)StageBriefingValueDisposition.NoVerifiedSource;
            serializedTemplate.FindProperty("targetRunDurationMilliseconds").intValue = 0;
            serializedTemplate.FindProperty("targetRunDurationSeconds").floatValue = 0f;
            serializedTemplate.FindProperty("featuredThreatDisposition").intValue =
                (int)StageBriefingValueDisposition.NoVerifiedSource;
            serializedTemplate.FindProperty("featuredThreat").stringValue = string.Empty;
            serializedTemplate.FindProperty("featuredSummonNeedDisposition").intValue =
                (int)StageBriefingValueDisposition.NoVerifiedSource;
            serializedTemplate.FindProperty("featuredSummonNeed").intValue = (int)StageSummonNeed.None;
            serializedTemplate.FindProperty("restrictionsDisposition").intValue =
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema;
            serializedTemplate.FindProperty("restrictionCount").intValue = 0;
            serializedTemplate.FindProperty("masteryPreviewDisposition").intValue =
                (int)StageBriefingValueDisposition.NotAuthoredForCurrentSchema;
            serializedTemplate.FindProperty("masteryPreview").stringValue = string.Empty;
            serializedTemplate.FindProperty("enemyPreviewDisposition").intValue =
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema;
            serializedTemplate.FindProperty("enemyPreviewCount").intValue = 0;
            serializedTemplate.FindProperty("rewardPreviewDisposition").intValue =
                (int)StageBriefingValueDisposition.NoVerifiedSource;
            serializedTemplate.FindProperty("rewardPreview").stringValue = string.Empty;
            serializedTemplate.FindProperty("courseSummaryDisposition").intValue =
                (int)StageBriefingValueDisposition.NotAdmittedByCurrentSchema;
            serializedTemplate.FindProperty("courseSummary").stringValue = string.Empty;
            serializedTemplate.FindProperty("masteryObjective").stringValue = string.Empty;
            serializedTemplate.FindProperty("rewardHook").stringValue = string.Empty;
            serializedTemplate.FindProperty("segments").arraySize = 0;

            SerializedProperty routeSegments =
                serializedTemplate.FindProperty("canonicalRouteSegments");
            routeSegments.arraySize = 1;
            SerializedProperty segment = routeSegments.GetArrayElementAtIndex(0);
            segment.FindPropertyRelative("templateSegmentId").stringValue =
                playableStageId + ".segment";
            segment.FindPropertyRelative("routeSegmentId").stringValue = entrySegmentId;
            segment.FindPropertyRelative("routeSequenceIndex").intValue = 0;
            SerializedProperty pockets = segment.FindPropertyRelative("pockets");
            pockets.arraySize = 1;
            SerializedProperty pocket = pockets.GetArrayElementAtIndex(0);
            pocket.FindPropertyRelative("pocketId").stringValue = playableStageId + ".pocket";
            pocket.FindPropertyRelative("sequenceIndex").intValue = 0;
            pocket.FindPropertyRelative("objectiveKind").intValue =
                (int)StageTemplatePocketObjectiveKind.CompleteTutorialPlan;
            pocket.FindPropertyRelative("currentExecutionOwnerDisposition").intValue =
                (int)StageTemplateCurrentExecutionOwnerDisposition.ExistingSceneOwner;
            pocket.FindPropertyRelative("p1cAdmissionDisposition").intValue =
                (int)StageTemplateP1CAdmissionDisposition.NotAdmitted;
            pocket.FindPropertyRelative("sourceDisposition").intValue =
                (int)StageTemplateSourceDisposition.CanonicalSemanticDigest;
            pocket.FindPropertyRelative("sourceSemanticId").stringValue =
                playableStageId + ".source";
            pocket.FindPropertyRelative("sourceRevision").intValue = 1;
            pocket.FindPropertyRelative("sourceSemanticDigest").stringValue =
                new string('a', 64);
            pocket.FindPropertyRelative("enemyRoleCount").intValue = 0;
            serializedTemplate.ApplyModifiedPropertiesWithoutUndo();

            serializedTemplate.Update();
            serializedTemplate.FindProperty("canonicalTemplateDigest").stringValue =
                template.ComputeCanonicalTemplateDigest();
            serializedTemplate.ApplyModifiedPropertiesWithoutUndo();
            return template;
        }

        private static void DestroyPlayableStageDefinition(PlayableStageDefinition route)
        {
            LinearStageTemplateProfile template = route?.ReferenceBlock?.StageTemplate;
            StageProgressionNode node = route?.ResultProgressionJoin?.ProgressionNode;
            StageProgressionGraph graph = route?.ResultProgressionJoin?.ProgressionGraph;
            UnityEngine.Object.DestroyImmediate(route);
            if (template != null && !EditorUtility.IsPersistent(template))
            {
                UnityEngine.Object.DestroyImmediate(template);
            }

            if (graph != null && !EditorUtility.IsPersistent(graph))
            {
                UnityEngine.Object.DestroyImmediate(graph);
            }

            if (node != null && !EditorUtility.IsPersistent(node))
            {
                UnityEngine.Object.DestroyImmediate(node);
            }
        }

        private static void AssertProjectionAccepted(Type catalogType, ScriptableObject catalog)
        {
            object[] arguments = { 0, ResolveUiRouteId(CombatRouteId), null, null };
            Assert.That(
                (bool)RequireMethod(
                    catalogType,
                    "TryCreateRouteProjection",
                    4,
                    typeof(int)).Invoke(catalog, arguments),
                Is.True,
                arguments[3]?.ToString());
            Assert.That(arguments[2], Is.Not.Null);
        }

        private static void AssertProjectionRejected(
            Type catalogType,
            ScriptableObject catalog,
            string expectedReason)
        {
            object[] arguments = { 0, ResolveUiRouteId(CombatRouteId), null, null };
            Assert.That(
                (bool)RequireMethod(
                    catalogType,
                    "TryCreateRouteProjection",
                    4,
                    typeof(int)).Invoke(catalog, arguments),
                Is.False);
            Assert.That(arguments[2], Is.Null);
            Assert.That(arguments[3]?.ToString(), Is.EqualTo(expectedReason));
        }

        private static void AssertCatalogProjectionTargets(
            object projection,
            string expectedCatalogEntryId,
            PlayableStageDefinition expectedRoute,
            string expectedScenePath,
            string expectedSceneName)
        {
            Assert.That(projection, Is.Not.Null);
            Assert.That(
                ReadProperty(projection, "CatalogEntryId"),
                Is.EqualTo(expectedCatalogEntryId));
            Assert.That(ReadProperty(projection, "PlayableStage"), Is.SameAs(expectedRoute));
            Assert.That(ReadProperty(projection, "EntryScenePath"), Is.EqualTo(expectedScenePath));
            Assert.That(ReadProperty(projection, "EntrySceneName"), Is.EqualTo(expectedSceneName));
        }

        private static void AssertCatalogIdentityFailureAcrossPublicPaths(
            Type catalogType,
            ScriptableObject catalog,
            string expectedReason)
        {
            object combatRouteId = ResolveUiRouteId(CombatRouteId);
            MethodInfo createNamed = RequireMethod(
                catalogType,
                "TryCreateRouteProjection",
                4,
                typeof(string));
            MethodInfo createIndexed = RequireMethod(
                catalogType,
                "TryCreateRouteProjection",
                4,
                typeof(int));

            string[] queriedIds = { "A", "B" };
            for (int i = 0; i < queriedIds.Length; i++)
            {
                object[] namedArguments = { queriedIds[i], combatRouteId, null, null };
                Assert.That((bool)createNamed.Invoke(catalog, namedArguments), Is.False);
                Assert.That(namedArguments[2], Is.Null);
                Assert.That(namedArguments[3]?.ToString(), Is.EqualTo(expectedReason));

                object[] getStageArguments = { queriedIds[i], null };
                Assert.That(
                    (bool)RequireMethod(catalogType, "TryGetStage").Invoke(
                        catalog,
                        getStageArguments),
                    Is.False);
            }

            object[] indexedArguments = { 0, combatRouteId, null, null };
            Assert.That((bool)createIndexed.Invoke(catalog, indexedArguments), Is.False);
            Assert.That(indexedArguments[2], Is.Null);
            Assert.That(indexedArguments[3]?.ToString(), Is.EqualTo(expectedReason));

            object[] firstProjectionArguments = { combatRouteId, null, null };
            Assert.That(
                (bool)RequireMethod(catalogType, "TryCreateFirstRouteProjection").Invoke(
                    catalog,
                    firstProjectionArguments),
                Is.False);
            Assert.That(firstProjectionArguments[1], Is.Null);
            Assert.That(firstProjectionArguments[2]?.ToString(), Is.EqualTo(expectedReason));

            object[] digestArguments = { 0, combatRouteId, null, null };
            Assert.That(
                (bool)RequireMethod(
                    catalogType,
                    "TryComputeCanonicalProjectionDigest").Invoke(
                        catalog,
                        digestArguments),
                Is.False);
            Assert.That(digestArguments[2], Is.EqualTo(string.Empty));
            Assert.That(digestArguments[3]?.ToString(), Is.EqualTo(expectedReason));

            object[] firstStageArguments = { null };
            Assert.That(
                (bool)RequireMethod(catalogType, "TryGetFirstStage").Invoke(
                    catalog,
                    firstStageArguments),
                Is.False);
        }

        private static void AssertRejectedStartHasNoSideEffects(
            Component presenter,
            Component router,
            int eventCount,
            string expectedReason)
        {
            Assert.That(eventCount, Is.Zero);
            Assert.That(Convert.ToInt32(ReadProperty(router, "RouteRequestCount")), Is.Zero);
            Assert.That(
                Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                Is.False);
            Assert.That(
                Convert.ToBoolean(ReadProperty(presenter, "HasAcceptedStartRequest")),
                Is.False);
            Assert.That(
                ReadProperty(presenter, "SelectedRouteRejectReason").ToString(),
                Is.EqualTo(expectedReason));
            Assert.That(presenter.GetComponent<AudioSource>(), Is.Null);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(StageRunRuntime.ActiveContext, Is.Null);
            Assert.That(StageRunRuntime.LastAbortRecord, Is.Null);
        }

        private static ScriptableObject CreateStageCatalog(
            Type catalogType,
            string stageId,
            PlayableStageDefinition route)
        {
            return CreateStageCatalog(catalogType, (stageId, route));
        }

        private static ScriptableObject CreateStageCatalog(
            Type catalogType,
            params (string StageId, PlayableStageDefinition Route)[] entries)
        {
            ScriptableObject catalog = CreateUnsealedStageCatalog(catalogType, entries);
            string[] projectionDigests = new string[entries.Length];
            for (int i = 0; i < entries.Length; i++)
            {
                object[] digestArguments = { i, ResolveUiRouteId(CombatRouteId), null, null };
                Assert.That(
                    (bool)RequireMethod(
                        catalogType,
                        "TryComputeCanonicalProjectionDigest").Invoke(catalog, digestArguments),
                    Is.True,
                    digestArguments[3]?.ToString());
                projectionDigests[i] = (string)digestArguments[2];
            }

            var serializedCatalog = new SerializedObject(catalog);
            SerializedProperty stages = serializedCatalog.FindProperty("stages");
            for (int i = 0; i < projectionDigests.Length; i++)
            {
                stages.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("canonicalProjectionDigest")
                    .stringValue = projectionDigests[i];
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static ScriptableObject CreateUnsealedStageCatalog(
            Type catalogType,
            params (string StageId, PlayableStageDefinition Route)[] entries)
        {
            Assert.That(entries, Is.Not.Null);
            Assert.That(entries.Length, Is.GreaterThan(0));
            ScriptableObject catalog = ScriptableObject.CreateInstance(catalogType);
            var serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.FindProperty("projectionSchemaVersion").intValue = 1;
            serializedCatalog.FindProperty("catalogProjectionGeneration").intValue = 1;
            SerializedProperty stages = serializedCatalog.FindProperty("stages");
            stages.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                ConfigureStageEntry(
                    stages.GetArrayElementAtIndex(i),
                    entries[i].StageId,
                    entries[i].Route);
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static void ConfigureStageEntry(
            SerializedProperty entry,
            string stageId,
            PlayableStageDefinition route)
        {
            entry.FindPropertyRelative("id").stringValue = stageId;
            entry.FindPropertyRelative("displayName").stringValue =
                route.ReferenceBlock.StageTemplate.Title;
            entry.FindPropertyRelative("summary").stringValue =
                route.ReferenceBlock.StageTemplate.Objective;
            entry.FindPropertyRelative("threatTags").stringValue = stageId + " threats";
            entry.FindPropertyRelative("recommendedSummonRole").stringValue = stageId + " support";
            entry.FindPropertyRelative("mockRewardPreview").stringValue = string.Empty;
            entry.FindPropertyRelative("presentationProvenance").intValue = 1;
            entry.FindPropertyRelative("playableStage").objectReferenceValue = route;
            entry.FindPropertyRelative("loadingCardId").stringValue = stageId + "_loading";
            entry.FindPropertyRelative("canonicalProjectionDigest").stringValue = string.Empty;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.NotNull(type, $"Missing product type {fullName}.");
            return type;
        }

        private static object ResolveUiRouteId(int rawValue)
        {
            return Enum.ToObject(
                RequireProductType("DimensionBrawl.UI.UIRouteId"),
                rawValue);
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Missing method {type.Name}.{methodName}.");
            return method;
        }

        private static MethodInfo RequireStaticMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Missing static method {type.Name}.{methodName}.");
            return method;
        }

        private static MethodInfo RequireMethod(
            Type type,
            string methodName,
            int parameterCount,
            Type firstParameterType)
        {
            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < methods.Length; i++)
            {
                ParameterInfo[] parameters = methods[i].GetParameters();
                if (string.Equals(methods[i].Name, methodName, StringComparison.Ordinal)
                    && parameters.Length == parameterCount
                    && parameters[0].ParameterType == firstParameterType)
                {
                    return methods[i];
                }
            }

            Assert.Fail(
                $"Missing method {type.Name}.{methodName} with {parameterCount} parameter(s) "
                + $"starting with {firstParameterType.Name}.");
            return null;
        }

        private static PropertyInfo RequireProperty(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(property, $"Missing property {type.Name}.{propertyName}.");
            return property;
        }

        private static object ReadProperty(object target, string propertyName)
        {
            return RequireProperty(target.GetType(), propertyName).GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing private field {target.GetType().Name}.{fieldName}.");
            field.SetValue(target, value);
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing private field {target.GetType().Name}.{fieldName}.");
            object value = field.GetValue(target);
            Assert.That(value, Is.Not.Null, $"Missing value {target.GetType().Name}.{fieldName}.");
            return (T)value;
        }

        private sealed class IndependentManifestRouteFixture
        {
            private bool destroyed;

            public IndependentManifestRouteFixture(
                PlayableStageDefinition route,
                StageDefinitionProfile stageDefinition,
                LinearStageTemplateProfile template,
                StageResultLocalizationTable localization,
                StageResultPresentationProfile presentationProfile,
                StageResultPresentationCatalog presentationCatalog,
                StageResultDefinition resultDefinition,
                StageProgressionNode progressionNode,
                StageProgressionGraph progressionGraph)
            {
                Route = route;
                StageDefinition = stageDefinition;
                Template = template;
                Localization = localization;
                PresentationProfile = presentationProfile;
                PresentationCatalog = presentationCatalog;
                ResultDefinition = resultDefinition;
                ProgressionNode = progressionNode;
                ProgressionGraph = progressionGraph;
            }

            public PlayableStageDefinition Route { get; }
            public StageDefinitionProfile StageDefinition { get; }
            public LinearStageTemplateProfile Template { get; }
            public StageResultLocalizationTable Localization { get; }
            public StageResultPresentationProfile PresentationProfile { get; }
            public StageResultPresentationCatalog PresentationCatalog { get; }
            public StageResultDefinition ResultDefinition { get; }
            public StageProgressionNode ProgressionNode { get; }
            public StageProgressionGraph ProgressionGraph { get; }

            public void Destroy()
            {
                if (destroyed)
                {
                    return;
                }

                destroyed = true;
                UnityEngine.Object.DestroyImmediate(Route);
                UnityEngine.Object.DestroyImmediate(ProgressionGraph);
                UnityEngine.Object.DestroyImmediate(ProgressionNode);
                UnityEngine.Object.DestroyImmediate(ResultDefinition);
                UnityEngine.Object.DestroyImmediate(PresentationCatalog);
                UnityEngine.Object.DestroyImmediate(PresentationProfile);
                UnityEngine.Object.DestroyImmediate(Localization);
                UnityEngine.Object.DestroyImmediate(Template);
                UnityEngine.Object.DestroyImmediate(StageDefinition);
            }
        }

        private sealed class ProductionOlympusIdentityGuard
        {
            private readonly ScriptableObject catalog;
            private readonly int catalogProjectionGeneration;
            private readonly object[] catalogEntries;
            private readonly PlayableStageDefinition route;
            private readonly StageSceneSegmentRef[] segments;
            private readonly StageDefinitionProfile[] stageDefinitions;
            private readonly StageRouteActionRef[] terminalActions;
            private readonly StageTerminalResolutionPolicy policy;
            private readonly StageReferenceBlock referenceBlock;
            private readonly LinearStageTemplateProfile template;
            private readonly StageResultProgressionJoinBlock join;
            private readonly StageResultDefinition resultDefinition;
            private readonly StageResultPresentationCatalog presentationCatalog;
            private readonly StageResultPresentationProfile presentationProfile;
            private readonly StageResultLocalizationTable localization;
            private readonly StageProgressionNode progressionNode;
            private readonly StageProgressionGraph progressionGraph;
            private readonly string routeDigest;
            private readonly string policyDigest;
            private readonly string referenceDigest;
            private readonly string briefingDigest;
            private readonly string templateDigest;
            private readonly string projectionDigest;
            private readonly string joinDigest;
            private readonly string resultEvaluationDigest;
            private readonly string resultBindingDigest;
            private readonly string resultSourceDigest;
            private readonly string localizationDigest;
            private readonly string nodeContentDigest;
            private readonly string nodeBindingDigest;
            private readonly string graphDigest;

            private ProductionOlympusIdentityGuard(
                ScriptableObject catalog,
                PlayableStageDefinition route)
            {
                this.catalog = catalog;
                catalogProjectionGeneration = Convert.ToInt32(
                    ReadProperty(catalog, "CatalogProjectionGeneration"));
                int catalogStageCount = Convert.ToInt32(ReadProperty(catalog, "StageCount"));
                catalogEntries = new object[catalogStageCount];
                for (int i = 0; i < catalogEntries.Length; i++)
                {
                    catalogEntries[i] = GetCatalogStage(catalog, i);
                }

                this.route = route;
                segments = new StageSceneSegmentRef[route.SceneSegmentCount];
                stageDefinitions = new StageDefinitionProfile[route.SceneSegmentCount];
                for (int i = 0; i < segments.Length; i++)
                {
                    segments[i] = route.GetSceneSegment(i);
                    stageDefinitions[i] = segments[i].StageDefinition;
                }

                terminalActions = new StageRouteActionRef[route.TerminalActionCount];
                for (int i = 0; i < terminalActions.Length; i++)
                {
                    terminalActions[i] = route.GetTerminalAction(i);
                }

                policy = route.TerminalResolutionPolicy;
                referenceBlock = route.ReferenceBlock;
                template = referenceBlock.StageTemplate;
                join = route.ResultProgressionJoin;
                resultDefinition = join.ResultDefinition;
                presentationCatalog = join.CanonicalPresentationCatalog;
                presentationProfile = resultDefinition.PresentationProfile;
                localization = resultDefinition.LocalizationTable;
                progressionNode = join.ProgressionNode;
                progressionGraph = join.ProgressionGraph;
                routeDigest = route.CanonicalRouteDigest;
                policyDigest = policy.TerminalResolutionPolicyDigest;
                referenceDigest = referenceBlock.CanonicalReferenceDigest;
                briefingDigest = referenceBlock.CanonicalBriefingDigest;
                templateDigest = template.CanonicalTemplateDigest;
                projectionDigest = (string)ReadProperty(
                    catalogEntries[0],
                    "CanonicalProjectionDigest");
                joinDigest = join.CanonicalDigest;
                resultEvaluationDigest = resultDefinition.EvaluationContentDigest;
                resultBindingDigest = resultDefinition.PresentationBindingDigest;
                resultSourceDigest = resultDefinition.PresentationSourceDigest;
                localizationDigest = localization.ComputeCanonicalDigest();
                nodeContentDigest = progressionNode.ContentDigest;
                nodeBindingDigest = progressionNode.BindingDigest;
                graphDigest = progressionGraph.CanonicalDigest;
            }

            public static ProductionOlympusIdentityGuard Capture(
                ScriptableObject catalog,
                PlayableStageDefinition route)
            {
                Assert.That(catalog, Is.Not.Null);
                Assert.That(Convert.ToInt32(ReadProperty(catalog, "StageCount")),
                    Is.GreaterThanOrEqualTo(1));
                Assert.That(route, Is.Not.Null);
                Assert.That(ReadProperty(GetCatalogStage(catalog, 0), "PlayableStage"),
                    Is.SameAs(route));
                var guard = new ProductionOlympusIdentityGuard(catalog, route);
                guard.AssertUnchanged();
                return guard;
            }

            public void AssertUnchanged()
            {
                Assert.That(catalog,
                    Is.SameAs(LoadRequired<ScriptableObject>(StageCatalogPath)));
                Assert.That(route, Is.SameAs(LoadRequired<PlayableStageDefinition>(PlayableStagePath)));
                Assert.That(Convert.ToInt32(ReadProperty(catalog, "StageCount")),
                    Is.EqualTo(catalogEntries.Length));
                Assert.That(
                    Convert.ToInt32(ReadProperty(catalog, "CatalogProjectionGeneration")),
                    Is.EqualTo(catalogProjectionGeneration));
                for (int catalogIndex = 0;
                    catalogIndex < catalogEntries.Length;
                    catalogIndex++)
                {
                    object currentCatalogEntry = GetCatalogStage(catalog, catalogIndex);
                    AssertCatalogEntriesEquivalent(
                        catalogEntries[catalogIndex],
                        currentCatalogEntry);
                    Assert.That(
                        TryComputeCatalogProjectionDigest(
                            catalog,
                            catalogIndex,
                            out string currentProjectionDigest,
                            out string currentProjectionRejectReason),
                        Is.True,
                        currentProjectionRejectReason);
                    Assert.That(currentProjectionDigest,
                        Is.EqualTo(ReadProperty(
                            catalogEntries[catalogIndex],
                            "CanonicalProjectionDigest")));
                    Assert.That(
                        TryCreateCatalogProjection(
                            catalog,
                            catalogIndex,
                            out object currentProjection,
                            out string createProjectionRejectReason),
                        Is.True,
                        createProjectionRejectReason);
                    Assert.That(ReadProperty(currentProjection, "PlayableStage"),
                        Is.SameAs(ReadProperty(catalogEntries[catalogIndex], "PlayableStage")));
                }

                object currentEntry = GetCatalogStage(catalog, 0);
                Assert.That(ReadProperty(currentEntry, "PlayableStage"), Is.SameAs(route));
                Assert.That(
                    ReadProperty(currentEntry, "CanonicalProjectionDigest"),
                    Is.EqualTo(projectionDigest));

                Assert.That(route.CanonicalRouteDigest, Is.EqualTo(routeDigest));
                Assert.That(route.ComputeCanonicalRouteDigest(), Is.EqualTo(routeDigest));
                Assert.That(route.TerminalResolutionPolicy, Is.SameAs(policy));
                Assert.That(
                    policy.TerminalResolutionPolicyDigest,
                    Is.EqualTo(policyDigest));
                Assert.That(policy.ComputeCanonicalDigest(), Is.EqualTo(policyDigest));
                Assert.That(route.ReferenceBlock, Is.SameAs(referenceBlock));
                Assert.That(referenceBlock.StageTemplate, Is.SameAs(template));
                Assert.That(
                    referenceBlock.CanonicalReferenceDigest,
                    Is.EqualTo(referenceDigest));
                Assert.That(
                    route.ComputeCanonicalReferenceDigest(),
                    Is.EqualTo(referenceDigest));
                Assert.That(
                    referenceBlock.CanonicalBriefingDigest,
                    Is.EqualTo(briefingDigest));
                Assert.That(
                    route.TryComputeCanonicalBriefingDigest(
                        out string recomputedBriefingDigest,
                        out StageBriefingBuildRejectReason briefingRejectReason),
                    Is.True,
                    briefingRejectReason.ToString());
                Assert.That(recomputedBriefingDigest, Is.EqualTo(briefingDigest));
                Assert.That(template.CanonicalTemplateDigest, Is.EqualTo(templateDigest));
                Assert.That(template.ComputeCanonicalTemplateDigest(), Is.EqualTo(templateDigest));

                Assert.That(route.SceneSegmentCount, Is.EqualTo(segments.Length));
                for (int i = 0; i < segments.Length; i++)
                {
                    Assert.That(route.GetSceneSegment(i), Is.SameAs(segments[i]));
                    Assert.That(
                        route.GetSceneSegment(i).StageDefinition,
                        Is.SameAs(stageDefinitions[i]));
                }

                Assert.That(route.TerminalActionCount, Is.EqualTo(terminalActions.Length));
                for (int i = 0; i < terminalActions.Length; i++)
                {
                    Assert.That(route.GetTerminalAction(i), Is.SameAs(terminalActions[i]));
                }

                Assert.That(route.ResultProgressionJoin, Is.SameAs(join));
                Assert.That(join.ResultDefinition, Is.SameAs(resultDefinition));
                Assert.That(
                    join.CanonicalPresentationCatalog,
                    Is.SameAs(presentationCatalog));
                Assert.That(join.ProgressionNode, Is.SameAs(progressionNode));
                Assert.That(join.ProgressionGraph, Is.SameAs(progressionGraph));
                Assert.That(join.CanonicalDigest, Is.EqualTo(joinDigest));
                Assert.That(
                    route.TryComputeResultProgressionJoinDigest(
                        out string recomputedJoinDigest,
                        out string joinDigestError),
                    Is.True,
                    joinDigestError);
                Assert.That(recomputedJoinDigest, Is.EqualTo(joinDigest));
                Assert.That(
                    resultDefinition.CanonicalPresentationCatalog,
                    Is.SameAs(presentationCatalog));
                Assert.That(
                    resultDefinition.PresentationProfile,
                    Is.SameAs(presentationProfile));
                Assert.That(resultDefinition.LocalizationTable, Is.SameAs(localization));
                Assert.That(presentationCatalog.LocalizationTable, Is.SameAs(localization));
                Assert.That(
                    resultDefinition.EvaluationContentDigest,
                    Is.EqualTo(resultEvaluationDigest));
                Assert.That(
                    resultDefinition.PresentationBindingDigest,
                    Is.EqualTo(resultBindingDigest));
                Assert.That(
                    resultDefinition.PresentationSourceDigest,
                    Is.EqualTo(resultSourceDigest));
                Assert.That(
                    resultDefinition.TryComputeCanonicalDigests(
                        out string recomputedEvaluationDigest,
                        out string recomputedBindingDigest,
                        out string recomputedSourceDigest,
                        out string resultDigestError),
                    Is.True,
                    resultDigestError);
                Assert.That(recomputedEvaluationDigest, Is.EqualTo(resultEvaluationDigest));
                Assert.That(recomputedBindingDigest, Is.EqualTo(resultBindingDigest));
                Assert.That(recomputedSourceDigest, Is.EqualTo(resultSourceDigest));
                Assert.That(localization.ComputeCanonicalDigest(), Is.EqualTo(localizationDigest));
                Assert.That(progressionNode.ContentDigest, Is.EqualTo(nodeContentDigest));
                Assert.That(progressionNode.BindingDigest, Is.EqualTo(nodeBindingDigest));
                Assert.That(
                    progressionNode.TryComputeCanonicalDigests(
                        out string recomputedNodeContentDigest,
                        out string recomputedNodeBindingDigest,
                        out string nodeDigestError),
                    Is.True,
                    nodeDigestError);
                Assert.That(recomputedNodeContentDigest, Is.EqualTo(nodeContentDigest));
                Assert.That(recomputedNodeBindingDigest, Is.EqualTo(nodeBindingDigest));
                Assert.That(progressionGraph.CanonicalDigest, Is.EqualTo(graphDigest));
                Assert.That(
                    progressionGraph.TryComputeCanonicalDigest(
                        out string recomputedGraphDigest,
                        out string graphDigestError),
                    Is.True,
                    graphDigestError);
                Assert.That(recomputedGraphDigest, Is.EqualTo(graphDigest));

                Assert.That(
                    TryComputeCatalogProjectionDigest(
                        catalog,
                        0,
                        out string recomputedProjectionDigest,
                        out string projectionDigestRejectReason),
                    Is.True,
                    projectionDigestRejectReason);
                Assert.That(recomputedProjectionDigest, Is.EqualTo(projectionDigest));
                Assert.That(
                    TryCreateCatalogProjection(
                        catalog,
                        0,
                        out object projection,
                        out string projectionRejectReason),
                    Is.True,
                    projectionRejectReason);
                Assert.That(ReadProperty(projection, "PlayableStage"), Is.SameAs(route));
                Assert.That(ReadProperty(projection, "EntryStageDefinition"),
                    Is.SameAs(stageDefinitions[0]));
                Assert.That(ReadProperty(projection, "StageTemplate"), Is.SameAs(template));
                Assert.That(ReadProperty(projection, "CanonicalProjectionDigest"),
                    Is.EqualTo(projectionDigest));
                Assert.That(
                    StageRunRouteSnapshot.TryCreate(
                        route,
                        out _,
                        out string routeError),
                    Is.True,
                    routeError);
                Assert.That(
                    StageRunResultProgressionJoinSnapshot.TryCreate(
                        route,
                        out StageRunResultProgressionJoinSnapshot joinSnapshot,
                        out string joinError),
                    Is.True,
                    joinError);
                Assert.That(joinSnapshot.CanonicalDigest, Is.EqualTo(joinDigest));
                Assert.That(
                    joinSnapshot.TryValidateIntegrity(out string joinIntegrityError),
                    Is.True,
                    joinIntegrityError);
            }
        }

        private sealed class RecordingSceneLoader : IStageRunSceneLoader
        {
            private readonly bool shouldSucceed;
            private readonly string failure;

            public RecordingSceneLoader(bool shouldSucceed, string failure)
            {
                this.shouldSucceed = shouldSucceed;
                this.failure = failure ?? string.Empty;
            }

            public int CallCount { get; private set; }
            public string LastSceneName { get; private set; } = string.Empty;
            public string LastScenePath { get; private set; } = string.Empty;
            public StageRunSceneLoadCompletionMode CompletionMode =>
                StageRunSceneLoadCompletionMode.DestinationActivatedSynchronously;

            public bool TryLoadSingle(string sceneName, string scenePath, out string error)
            {
                CallCount++;
                LastSceneName = sceneName ?? string.Empty;
                LastScenePath = scenePath ?? string.Empty;
                error = shouldSucceed ? string.Empty : failure;
                return shouldSucceed;
            }
        }

        private sealed class UnexpectedSceneChangingLoader : IStageRunSceneLoader
        {
            public int CallCount { get; private set; }
            public StageRunSceneLoadCompletionMode CompletionMode =>
                StageRunSceneLoadCompletionMode.DestinationActivatedSynchronously;

            public bool TryLoadSingle(string sceneName, string scenePath, out string error)
            {
                CallCount++;
                Scene unexpected = SceneManager.CreateScene(
                    "UnexpectedTerminalDispatch_" + Guid.NewGuid().ToString("N"));
                SceneManager.SetActiveScene(unexpected);
                error = string.Empty;
                return true;
            }
        }

        private sealed class RejectingUiRouteResolver : IStageRunUiRouteResolver
        {
            private readonly string failure;

            public RejectingUiRouteResolver(string failure)
            {
                this.failure = failure ?? string.Empty;
            }

            public bool TryResolve(
                StageUiRouteId routeId,
                out StageRunUiRouteTarget target,
                out string error)
            {
                target = null;
                error = failure;
                return false;
            }
        }

        private sealed class FixedUiRouteResolver : IStageRunUiRouteResolver
        {
            private readonly StageRunUiRouteTarget target;

            public FixedUiRouteResolver(StageUiRouteId routeId, string sceneName, string scenePath)
            {
                target = new StageRunUiRouteTarget(routeId, sceneName, scenePath);
            }

            public int CallCount { get; private set; }

            public bool TryResolve(
                StageUiRouteId routeId,
                out StageRunUiRouteTarget resolved,
                out string error)
            {
                CallCount++;
                resolved = routeId == target.RouteId ? target : null;
                error = resolved != null ? string.Empty : "route id mismatch";
                return resolved != null;
            }
        }

        private static T LoadRequired<T>(string assetPath)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.NotNull(asset, $"Missing required asset at {assetPath}.");
            return asset;
        }
    }
}
