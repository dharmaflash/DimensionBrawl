using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace DimensionBrawl.Tests
{
    public sealed class LoginVideoBackgroundPresenterPlayModeTests
    {
        private const string CanonicalProfilePath =
            "Assets/_Game/DesignData/UI/DB_LoginBackgroundVideo.asset";
        private const string PresenterTypeName =
            "DimensionBrawl.UI.LoginVideoBackgroundPresenter";
        private const string ProfileTypeName =
            "DimensionBrawl.UI.LoginVideoBackgroundProfile";

        [Test]
        public void PreparedCallbackKeepsFallbackUntilFirstFrameAndSignalsOnce()
        {
            LoginVideoRig rig = CreateRig(muteAudio: true, hideVideoUntilPrepared: false);
            try
            {
                int preparedCount = 0;
                SubscribeToVisualPrepared(rig.Presenter, () => preparedCount++);
                int generation = ArmValidGeneration(rig);

                InvokeMethod(rig.Presenter, "HandlePrepared", rig.VideoPlayer, generation);

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.FallbackTexture));
                Assert.That(rig.RawImage.color, Is.EqualTo(rig.FallbackColor));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.False);
                Assert.That(preparedCount, Is.Zero);
                Assert.That(rig.VideoPlayer.sendFrameReadyEvents, Is.True);
                Assert.That(rig.VideoPlayer.audioOutputMode, Is.EqualTo(VideoAudioOutputMode.None));

                InvokeMethod(rig.Presenter, "HandleFrameReady", rig.VideoPlayer, 0L, generation);

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.TargetTexture));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.True);
                Assert.That(preparedCount, Is.EqualTo(1));
                Assert.That(rig.VideoPlayer.sendFrameReadyEvents, Is.False);

                InvokeMethod(rig.Presenter, "HandleFrameReady", rig.VideoPlayer, 1L, generation);
                Assert.That(preparedCount, Is.EqualTo(1), "A generation must publish VisualPrepared only once.");
            }
            finally
            {
                rig.Dispose();
            }
        }

        [TestCase(MissingVideoDependency.Clip)]
        [TestCase(MissingVideoDependency.RenderTexture)]
        public void MissingVideoDependencyImmediatelyCompletesWithFallback(MissingVideoDependency missingDependency)
        {
            LoginVideoRig rig = CreateRig(muteAudio: true, hideVideoUntilPrepared: true);
            try
            {
                if (missingDependency == MissingVideoDependency.Clip)
                {
                    SetPrivateField(rig.Profile, "backgroundClip", null);
                }
                else
                {
                    SetPrivateField(rig.Profile, "targetTexture", null);
                }

                int preparedCount = 0;
                SubscribeToVisualPrepared(rig.Presenter, () => preparedCount++);

                InvokeMethod(rig.Presenter, "Play");

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.FallbackTexture));
                Assert.That(rig.RawImage.color, Is.EqualTo(rig.FallbackColor));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.True);
                Assert.That(preparedCount, Is.EqualTo(1));

                int generation = ReadPrivateField<int>(rig.Presenter, "playbackGeneration");
                InvokeMethod(rig.Presenter, "CompleteWithFallback", generation, rig.VideoPlayer);
                Assert.That(preparedCount, Is.EqualTo(1), "Fallback completion must be idempotent per generation.");
            }
            finally
            {
                rig.Dispose();
            }
        }

        [Test]
        public void CurrentGenerationErrorRestoresFallbackWithoutRepublishingReadiness()
        {
            LoginVideoRig rig = CreateRig(muteAudio: true, hideVideoUntilPrepared: true);
            try
            {
                int preparedCount = 0;
                SubscribeToVisualPrepared(rig.Presenter, () => preparedCount++);
                int generation = ArmValidGeneration(rig);
                rig.RawImage.texture = rig.TargetTexture;
                rig.RawImage.color = Color.white;

                InvokeMethod(rig.Presenter, "HandleErrorReceived", rig.VideoPlayer, "test error", generation);

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.FallbackTexture));
                Assert.That(rig.RawImage.color, Is.EqualTo(rig.FallbackColor));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.True);
                Assert.That(preparedCount, Is.EqualTo(1));

                InvokeMethod(rig.Presenter, "HandleErrorReceived", rig.VideoPlayer, "late duplicate", generation);
                Assert.That(preparedCount, Is.EqualTo(1));
            }
            finally
            {
                rig.Dispose();
            }
        }

        [Test]
        public void ReplacedGenerationRejectsStalePreparedFrameAndErrorCallbacks()
        {
            LoginVideoRig rig = CreateRig(muteAudio: true, hideVideoUntilPrepared: false);
            try
            {
                int preparedCount = 0;
                SubscribeToVisualPrepared(rig.Presenter, () => preparedCount++);
                int staleGeneration = ArmValidGeneration(rig);
                InvokeMethod(rig.Presenter, "HandlePrepared", rig.VideoPlayer, staleGeneration);

                int currentGeneration = ArmValidGeneration(rig);
                InvokeMethod(rig.Presenter, "HandleFrameReady", rig.VideoPlayer, 0L, staleGeneration);
                InvokeMethod(rig.Presenter, "HandleErrorReceived", rig.VideoPlayer, "stale error", staleGeneration);

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.FallbackTexture));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.False);
                Assert.That(preparedCount, Is.Zero);

                InvokeMethod(rig.Presenter, "HandlePrepared", rig.VideoPlayer, currentGeneration);
                Assert.That(rig.RawImage.texture, Is.SameAs(rig.FallbackTexture));
                InvokeMethod(rig.Presenter, "HandleFrameReady", rig.VideoPlayer, 0L, currentGeneration);

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.TargetTexture));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.True);
                Assert.That(preparedCount, Is.EqualTo(1));
            }
            finally
            {
                rig.Dispose();
            }
        }

        [Test]
        public void SetClipAndReenableInvalidateEarlierGenerationCallbacks()
        {
            LoginVideoRig rig = CreateRig(muteAudio: true, hideVideoUntilPrepared: false);
            try
            {
                SetPrivateField(rig.Presenter, "playOnEnable", false);
                int preparedCount = 0;
                SubscribeToVisualPrepared(rig.Presenter, () => preparedCount++);
                int staleGeneration = ArmValidGeneration(rig);
                InvokeMethod(rig.Presenter, "HandlePrepared", rig.VideoPlayer, staleGeneration);

                InvokeMethod(rig.Presenter, "SetClip", rig.Clip, false);
                InvokeMethod(rig.Presenter, "HandleFrameReady", rig.VideoPlayer, 0L, staleGeneration);

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.FallbackTexture));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.True);
                Assert.That(preparedCount, Is.EqualTo(1));

                rig.Root.SetActive(false);
                rig.Root.SetActive(true);
                InvokeMethod(rig.Presenter, "HandleFrameReady", rig.VideoPlayer, 0L, staleGeneration);

                Assert.That(rig.RawImage.texture, Is.SameAs(rig.FallbackTexture));
                Assert.That(IsVisualPrepared(rig.Presenter), Is.True);
                Assert.That(preparedCount, Is.EqualTo(2), "Re-enable publishes readiness for its new fallback generation once.");
            }
            finally
            {
                rig.Dispose();
            }
        }

        [TestCase(true, VideoAudioOutputMode.None)]
        [TestCase(false, VideoAudioOutputMode.Direct)]
        public void ConfigurePreservesProfileMutePolicy(bool muteAudio, VideoAudioOutputMode expectedMode)
        {
            LoginVideoRig rig = CreateRig(muteAudio, hideVideoUntilPrepared: true);
            try
            {
                InvokeMethod(rig.Presenter, "BeginPlaybackGeneration");
                InvokeMethod(rig.Presenter, "ConfigureVideoPlayer");

                Assert.That(rig.VideoPlayer.audioOutputMode, Is.EqualTo(expectedMode));
            }
            finally
            {
                rig.Dispose();
            }
        }

        private static LoginVideoRig CreateRig(bool muteAudio, bool hideVideoUntilPrepared)
        {
            ScriptableObject canonicalProfile =
                AssetDatabase.LoadAssetAtPath<ScriptableObject>(CanonicalProfilePath);
            Assert.That(canonicalProfile, Is.Not.Null, $"Missing canonical login video profile: {CanonicalProfilePath}");
            VideoClip clip = ReadProperty(canonicalProfile, "BackgroundClip") as VideoClip;
            Assert.That(clip, Is.Not.Null, "Canonical login profile needs a testable clip.");

            return new LoginVideoRig(clip, muteAudio, hideVideoUntilPrepared);
        }

        private static int ArmValidGeneration(LoginVideoRig rig)
        {
            int generation = (int)InvokeMethod(rig.Presenter, "BeginPlaybackGeneration");
            InvokeMethod(rig.Presenter, "RestoreFallback");
            InvokeMethod(rig.Presenter, "ConfigureVideoPlayer");
            SetPrivateField(rig.Presenter, "expectedClip", rig.Clip);
            SetPrivateField(rig.Presenter, "expectedTargetTexture", rig.TargetTexture);
            InvokeMethod(rig.Presenter, "BindVideoPlayer", rig.VideoPlayer, generation);
            return generation;
        }

        private static object InvokeMethod(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing method {target.GetType().Name}.{methodName}.");
            return method.Invoke(target, arguments);
        }

        private static bool IsVisualPrepared(Component presenter)
        {
            return Convert.ToBoolean(ReadProperty(presenter, "IsVisualPrepared"));
        }

        private static void SubscribeToVisualPrepared(Component presenter, Action handler)
        {
            EventInfo eventInfo = presenter.GetType().GetEvent(
                "VisualPrepared",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(eventInfo, Is.Not.Null, $"Missing event {presenter.GetType().Name}.VisualPrepared.");
            eventInfo.AddEventHandler(presenter, handler);
        }

        private static object ReadProperty(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"Missing property {target.GetType().Name}.{propertyName}.");
            return property.GetValue(target);
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing private field {target.GetType().Name}.{fieldName}.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing private field {target.GetType().Name}.{fieldName}.");
            field.SetValue(target, value);
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        public enum MissingVideoDependency
        {
            Clip,
            RenderTexture
        }

        private sealed class LoginVideoRig : IDisposable
        {
            public LoginVideoRig(VideoClip clip, bool muteAudio, bool hideVideoUntilPrepared)
            {
                Clip = clip;
                FallbackTexture = new Texture2D(2, 2)
                {
                    name = "LoginFallback_Test",
                    hideFlags = HideFlags.HideAndDontSave
                };
                TargetTexture = new RenderTexture(64, 36, 0)
                {
                    name = "LoginVideoTarget_Test",
                    hideFlags = HideFlags.HideAndDontSave
                };
                FallbackColor = new Color(0.36f, 0.48f, 0.62f, 0.9f);

                Root = new GameObject(
                    "LoginVideoBackgroundPresenter_Test",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage),
                    typeof(AspectRatioFitter),
                    typeof(VideoPlayer));
                Root.hideFlags = HideFlags.HideAndDontSave;
                RawImage = Root.GetComponent<RawImage>();
                RawImage.texture = FallbackTexture;
                RawImage.color = FallbackColor;
                VideoPlayer = Root.GetComponent<VideoPlayer>();
                Presenter = Root.AddComponent(RequireProductType(PresenterTypeName));

                Profile = ScriptableObject.CreateInstance(RequireProductType(ProfileTypeName));
                Profile.hideFlags = HideFlags.HideAndDontSave;
                SetPrivateField(Profile, "backgroundClip", Clip);
                SetPrivateField(Profile, "targetTexture", TargetTexture);
                SetPrivateField(Profile, "loop", true);
                SetPrivateField(Profile, "muteAudio", muteAudio);
                SetPrivateField(Profile, "hideVideoUntilPrepared", hideVideoUntilPrepared);
                SetPrivateField(Profile, "fallbackWidth", 64);
                SetPrivateField(Profile, "fallbackHeight", 36);
                SetPrivateField(Presenter, "profile", Profile);
            }

            public GameObject Root { get; }
            public RawImage RawImage { get; }
            public VideoPlayer VideoPlayer { get; }
            public Component Presenter { get; }
            public ScriptableObject Profile { get; }
            public VideoClip Clip { get; }
            public Texture2D FallbackTexture { get; }
            public RenderTexture TargetTexture { get; }
            public Color FallbackColor { get; }

            public void Dispose()
            {
                if (VideoPlayer != null)
                {
                    VideoPlayer.Stop();
                }

                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root);
                }

                if (Profile != null)
                {
                    UnityEngine.Object.DestroyImmediate(Profile);
                }

                if (TargetTexture != null)
                {
                    if (TargetTexture.IsCreated())
                    {
                        TargetTexture.Release();
                    }

                    UnityEngine.Object.DestroyImmediate(TargetTexture);
                }

                if (FallbackTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(FallbackTexture);
                }
            }
        }
    }
}
