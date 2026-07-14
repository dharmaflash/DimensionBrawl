using System;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor
{
    public static class UISceneBgmSetup
    {
        private const string ScreenCatalogPath = "Assets/_Game/DesignData/UI/DB_UIScreenCatalog.asset";
        private const string SoundContextCatalogPath = "Assets/_Game/DesignData/UI/DB_UISoundContexts.asset";
        private const string LoginScenePath = "Assets/_Game/Scenes/UI/UI_Login.unity";
        private const string LobbyScenePath = "Assets/_Game/Scenes/UI/UI_Lobby.unity";
        private const string BgmPlayerRootName = "UI Scene BGM";
        private const string UiCameraName = "UI Camera";

        [MenuItem("DimensionBrawl/UI V1/Reapply Login And Lobby BGM")]
        public static void ReapplyLoginAndLobbyBgmMenu()
        {
            ReapplyLoginAndLobbyBgm();
        }

        public static void ReapplyLoginAndLobbyBgm()
        {
            ConfigureScene(LoginScenePath, UIRouteId.Login);
            ConfigureScene(LobbyScenePath, UIRouteId.Lobby);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureScene(string scenePath, UIRouteId routeId)
        {
            UIScreenCatalog screenCatalog = LoadAsset<UIScreenCatalog>(ScreenCatalogPath);
            UISoundContextCatalog soundContextCatalog = LoadAsset<UISoundContextCatalog>(SoundContextCatalogPath);

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject root = FindRoot(scene, BgmPlayerRootName);
            if (root == null)
            {
                root = new GameObject(BgmPlayerRootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            AudioSource source = EnsureComponent<AudioSource>(root);
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = false;

            UISceneBgmPlayer player = EnsureComponent<UISceneBgmPlayer>(root);
            SerializedObject playerObject = new SerializedObject(player);
            playerObject.FindProperty("screenCatalog").objectReferenceValue = screenCatalog;
            playerObject.FindProperty("soundContextCatalog").objectReferenceValue = soundContextCatalog;
            playerObject.FindProperty("routeId").intValue = (int)routeId;
            playerObject.FindProperty("source").objectReferenceValue = source;
            playerObject.FindProperty("playOnEnable").boolValue = true;
            playerObject.FindProperty("stopOnDisable").boolValue = true;
            playerObject.FindProperty("masterVolume").floatValue = 1f;
            playerObject.ApplyModifiedPropertiesWithoutUndo();

            EnsureAudioListener(scenePath, scene);
            EditorUtility.SetDirty(root);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException($"Failed to save UI BGM scene setup: {scenePath}");
            }
        }

        private static void EnsureAudioListener(string scenePath, Scene scene)
        {
            GameObject cameraObject = FindRoot(scene, UiCameraName);
            if (cameraObject == null)
            {
                throw new InvalidOperationException($"{scenePath} is missing {UiCameraName}.");
            }

            EnsureComponent<AudioListener>(cameraObject);
            EditorUtility.SetDirty(cameraObject);
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, rootName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static T EnsureComponent<T>(GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();
            return component != null ? component : owner.AddComponent<T>();
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing required asset: {path}");
            }

            return asset;
        }
    }
}
