using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private static readonly string[] ExpectedBuildScenePaths =
        {
            LoginScenePath,
            LobbyScenePath,
            StageSelectScenePath,
            CorridorScenePath,
            StationScenePath,
            StageClearScenePath
        };

        [TearDown]
        public void ReleaseUiSceneServiceOwnership()
        {
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

        [Test]
        public void BuildSettingsContainTheCanonicalProductRouteInOrder()
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

            ScriptableObject stageCatalog = LoadRequired<ScriptableObject>(StageCatalogPath);
            PropertyInfo stageCountProperty = RequireProperty(stageCatalog.GetType(), "StageCount");
            MethodInfo getStageMethod = RequireMethod(stageCatalog.GetType(), "GetStage");
            int stageCount = (int)stageCountProperty.GetValue(stageCatalog);
            Assert.Greater(stageCount, 0, "The product stage catalog must expose at least one route.");

            for (int i = 0; i < stageCount; i++)
            {
                object stage = getStageMethod.Invoke(stageCatalog, new object[] { i });
                string stageId = (string)ReadProperty(stage, "Id");
                string scenePath = (string)ReadProperty(stage, "ScenePath");
                string sceneName = (string)ReadProperty(stage, "SceneName");
                bool hasSceneRoute = (bool)ReadProperty(stage, "HasSceneRoute");

                Assert.IsTrue(hasSceneRoute, $"Stage '{stageId}' has no authored scene route.");
                Assert.AreEqual(
                    Path.GetFileNameWithoutExtension(scenePath.Replace('\\', '/')),
                    sceneName,
                    $"Stage '{stageId}' resolves an inconsistent scene name.");
                Assert.NotNull(
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath),
                    $"Stage '{stageId}' references a missing scene at {scenePath}.");
            }
        }

        [UnityTest]
        public IEnumerator SelectedStageStartUsesTheSelectedCatalogDefinition()
        {
            Type routerType = RequireProductType("DimensionBrawl.UI.UISceneFlowRouter");
            Type presenterType = RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter");
            Type catalogType = RequireProductType("DimensionBrawl.UI.UIStageCatalog");
            ScriptableObject routeTable = LoadRequired<ScriptableObject>(RouteTablePath);
            StageDefinitionProfile primaryDefinition = CreateStageDefinition(
                "Assets/Virtual/PrimaryCombat.unity");
            StageDefinitionProfile secondaryDefinition = CreateStageDefinition(
                "Assets/Virtual/SecondaryCombat.unity");
            ScriptableObject stageCatalog = CreateStageCatalog(
                catalogType,
                primaryDefinition,
                secondaryDefinition);
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
                root.SetActive(true);

                LogAssert.Expect(LogType.Warning, "UI scene route loader is not configured.");
                RequireMethod(presenterType, "HandleStartClicked").Invoke(presenter, null);
                yield return null;

                object state = RequireProperty(routerType, "CurrentState").GetValue(router);
                Assert.AreEqual(CombatRouteId, Convert.ToInt32(ReadProperty(state, "RouteId")));
                Assert.AreEqual("SecondaryCombat", ReadProperty(state, "SceneName"));
                Assert.AreEqual("Failed", ReadProperty(state, "Phase").ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(stageCatalog);
                UnityEngine.Object.DestroyImmediate(primaryDefinition);
                UnityEngine.Object.DestroyImmediate(secondaryDefinition);
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
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StationVictoryRetryButtonLoadsFreshCorridorRun()
        {
            yield return LoadStationVictoryAndWaitForClearSurface();

            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            AssertPresenterRoutes(presenter);
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
            Assert.That(playerRoot.activeInHierarchy, Is.False, "Retry must start a fresh intro run.");
        }

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator StationVictoryLobbyButtonLoadsCanonicalLobby()
        {
            yield return LoadStationVictoryAndWaitForClearSurface();

            Scene clearScene = SceneManager.GetSceneByName(StageClearSceneName);
            StageClearScreenPresenter presenter = RequireSingleSceneComponent<StageClearScreenPresenter>(clearScene);
            AssertPresenterRoutes(presenter);
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

        private static IEnumerator LoadStationVictoryAndWaitForClearSurface()
        {
            Time.timeScale = 1f;
            EditorSceneManager.LoadSceneInPlayMode(
                StationScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Scene stationScene = SceneManager.GetActiveScene();
            Assert.AreEqual(StationScenePath, stationScene.path.Replace('\\', '/'));
            Assert.That(CountSceneComponents<OlympusStationCombatResultPresenter>(stationScene), Is.EqualTo(1));
            Assert.That(CountSceneComponents<OlympusStageClearOverlay>(stationScene), Is.EqualTo(1));
            Assert.That(CountSceneComponents<CombatEncounterController>(stationScene), Is.EqualTo(1));

            Behaviour entryGuide = RequireSingleSceneBehaviour(
                stationScene,
                RequireProductType("DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge"));
            Behaviour entryNotice = RequireSingleSceneBehaviour(
                stationScene,
                RequireProductType("DimensionBrawl.UI.SceneEntryNoticeOverlay"));
            entryGuide.enabled = false;
            entryNotice.enabled = false;
            yield return null;
            Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f));

            CombatEncounterController encounter = RequireSingleSceneComponent<CombatEncounterController>(stationScene);
            CombatHealth enemyHealth = ReadPrivateField<CombatHealth>(encounter, "enemyHealth");
            enemyHealth.ResetHealthToFull();
            Assert.That(
                enemyHealth.TryApplyDamage(new DamageInfo(
                    null,
                    DamageTeam.Player,
                    enemyHealth.MaxHealth + 1f,
                    enemyHealth.transform.position,
                    Vector3.forward,
                    0f,
                    DamageResponsePolicy.DamageOnly,
                    CombatControlLockPolicy.None)),
                Is.True);
            Assert.That(encounter.IsWon, Is.True);

            float deadline = Time.realtimeSinceStartup + 8f;
            StageClearScreenPresenter presenter = null;
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
                }

                yield return null;
            }

            Scene loadedClearScene = SceneManager.GetSceneByName(StageClearSceneName);
            Assert.That(loadedClearScene.isLoaded, Is.True);
            Assert.That(CountSceneComponents<StageClearScreenPresenter>(loadedClearScene), Is.EqualTo(1));
            Assert.That(CountCombatSessionSurfaces(stationScene, visibleOnly: false), Is.EqualTo(1));
            Assert.That(CountCombatSessionSurfaces(stationScene, visibleOnly: true), Is.Zero);
            Assert.That(Time.timeScale, Is.Zero);

            GameObject combatHud = FindSceneObject(stationScene, "BossBarrageLaneReview_CombatHudCanvas");
            Assert.That(combatHud, Is.Not.Null);
            Assert.That(combatHud.activeSelf, Is.False, "The combat HUD must yield to the single clear surface.");
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

        private static void AssertPresenterRoutes(StageClearScreenPresenter presenter)
        {
            Assert.AreEqual("OlympusCorridorInvasionStage", ReadPrivateField<string>(presenter, "retrySceneName"));
            Assert.AreEqual(CorridorScenePath, ReadPrivateField<string>(presenter, "retryScenePath"));
            Assert.AreEqual("UI_Lobby", ReadPrivateField<string>(presenter, "lobbySceneName"));
            Assert.AreEqual(LobbyScenePath, ReadPrivateField<string>(presenter, "lobbyScenePath"));
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

        private static StageDefinitionProfile CreateStageDefinition(string scenePath)
        {
            StageDefinitionProfile definition = ScriptableObject.CreateInstance<StageDefinitionProfile>();
            var serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("mapScenePath").stringValue = scenePath;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static ScriptableObject CreateStageCatalog(
            Type catalogType,
            StageDefinitionProfile primaryDefinition,
            StageDefinitionProfile secondaryDefinition)
        {
            ScriptableObject catalog = ScriptableObject.CreateInstance(catalogType);
            var serializedCatalog = new SerializedObject(catalog);
            SerializedProperty stages = serializedCatalog.FindProperty("stages");
            stages.arraySize = 2;
            ConfigureStageEntry(stages.GetArrayElementAtIndex(0), "primary", primaryDefinition);
            ConfigureStageEntry(stages.GetArrayElementAtIndex(1), "secondary", secondaryDefinition);
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static void ConfigureStageEntry(
            SerializedProperty entry,
            string stageId,
            StageDefinitionProfile definition)
        {
            entry.FindPropertyRelative("id").stringValue = stageId;
            entry.FindPropertyRelative("stageDefinition").objectReferenceValue = definition;
            entry.FindPropertyRelative("loadingCardId").stringValue = stageId + "_loading";
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.NotNull(type, $"Missing product type {fullName}.");
            return type;
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Missing method {type.Name}.{methodName}.");
            return method;
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

        private static T LoadRequired<T>(string assetPath)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.NotNull(asset, $"Missing required asset at {assetPath}.");
            return asset;
        }
    }
}
