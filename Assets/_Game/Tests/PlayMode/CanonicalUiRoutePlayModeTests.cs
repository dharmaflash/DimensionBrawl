using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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

        private static T LoadRequired<T>(string assetPath)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.NotNull(asset, $"Missing required asset at {assetPath}.");
            return asset;
        }
    }
}
