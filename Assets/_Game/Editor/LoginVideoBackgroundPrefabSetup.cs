using System;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace DimensionBrawl.Editor
{
    public static class LoginVideoBackgroundPrefabSetup
    {
        private const string LoginScreenPrefabPath = "Assets/_Game/UI/Login/PF_UI_LoginScreen.prefab";

        [MenuItem("DimensionBrawl/UI V1/Reapply Login Video Background")]
        public static void ApplyMenu()
        {
            ApplyLoginVideoBackground();
        }

        public static void ApplyLoginVideoBackground()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(LoginScreenPrefabPath);
            try
            {
                LoginVideoBackgroundPresenter presenter = prefabRoot.GetComponentInChildren<LoginVideoBackgroundPresenter>(true);
                if (presenter == null)
                {
                    throw new InvalidOperationException($"Login video setup could not find LoginVideoBackgroundPresenter in {LoginScreenPrefabPath}.");
                }

                VideoPlayer videoPlayer = presenter.GetComponent<VideoPlayer>();
                if (videoPlayer == null)
                {
                    videoPlayer = presenter.gameObject.AddComponent<VideoPlayer>();
                }

                videoPlayer.playOnAwake = false;
                videoPlayer.waitForFirstFrame = true;
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

                SerializedObject presenterObject = new SerializedObject(presenter);
                presenterObject.FindProperty("videoPlayer").objectReferenceValue = videoPlayer;
                presenterObject.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, LoginScreenPrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Login video background setup applied.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
