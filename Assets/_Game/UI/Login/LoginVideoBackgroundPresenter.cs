using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(VideoPlayer))]
    public sealed class LoginVideoBackgroundPresenter : MonoBehaviour, IUISceneTransitionReadinessSource
    {
        [SerializeField] private RawImage targetImage;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private LoginVideoBackgroundProfile profile;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private bool playOnEnable = true;

        private Texture fallbackTexture;
        private Color fallbackColor;
        private bool fallbackCached;
        private VideoClip runtimeClipOverride;
        private VideoPlayer boundVideoPlayer;
        private VideoPlayer.EventHandler prepareCompletedHandler;
        private VideoPlayer.ErrorEventHandler errorReceivedHandler;
        private VideoPlayer.FrameReadyEventHandler frameReadyHandler;
        private VideoClip expectedClip;
        private RenderTexture expectedTargetTexture;
        private int playbackGeneration;
        private int awaitingFirstFrameGeneration = -1;
        private int visualPreparedGeneration = -1;

        private VideoClip ActiveClip => runtimeClipOverride != null ? runtimeClipOverride : profile != null ? profile.BackgroundClip : null;
        private RenderTexture TargetTexture => profile != null ? profile.TargetTexture : null;
        private bool Loop => profile == null || profile.Loop;
        private bool MuteAudio => profile == null || profile.MuteAudio;
        private int FallbackWidth => profile != null ? profile.FallbackWidth : 2560;
        private int FallbackHeight => profile != null ? profile.FallbackHeight : 1440;

        public bool IsVisualPrepared { get; private set; }
        public bool IsSceneTransitionReady => IsVisualPrepared;

        public event Action VisualPrepared;

        private void Reset()
        {
            targetImage = GetComponent<RawImage>();
            aspectRatioFitter = GetComponent<AspectRatioFitter>();
            videoPlayer = GetComponent<VideoPlayer>();
        }

        private void Awake()
        {
            ResolveReferences();
            CacheFallback();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CacheFallback();

            if (playOnEnable)
            {
                Play();
                return;
            }

            int generation = BeginPlaybackGeneration();
            ConfigureVideoPlayer();
            RestoreFallback();
            CompleteVisualPreparation(generation);
        }

        private void OnDisable()
        {
            BeginPlaybackGeneration();
            RestoreFallback();
        }

        private void OnDestroy()
        {
            UnbindVideoPlayer();
        }

        public void SetClip(VideoClip clip, bool playImmediately = true)
        {
            runtimeClipOverride = clip;

            if (playImmediately && isActiveAndEnabled)
            {
                Play();
                return;
            }

            int generation = BeginPlaybackGeneration();
            ConfigureVideoPlayer();
            RestoreFallback();
            if (isActiveAndEnabled)
            {
                CompleteVisualPreparation(generation);
            }
        }

        public void Play()
        {
            ResolveReferences();
            CacheFallback();
            int generation = BeginPlaybackGeneration();
            RestoreFallback();

            VideoClip clip = ActiveClip;
            RenderTexture targetTexture = TargetTexture;
            if (clip == null || videoPlayer == null || targetImage == null || targetTexture == null)
            {
                ConfigureVideoPlayer();
                CompleteWithFallback(generation, videoPlayer);
                return;
            }

            expectedClip = clip;
            expectedTargetTexture = targetTexture;
            ConfigureVideoPlayer();
            videoPlayer.targetTexture = targetTexture;
            videoPlayer.clip = clip;
            BindVideoPlayer(videoPlayer, generation);
            videoPlayer.Prepare();
        }

        private void CacheFallback()
        {
            if (fallbackCached || targetImage == null)
            {
                return;
            }

            fallbackTexture = targetImage.texture;
            fallbackColor = targetImage.color;
            fallbackCached = true;
        }

        private void ResolveReferences()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<RawImage>();
            }

            if (aspectRatioFitter == null)
            {
                aspectRatioFitter = GetComponent<AspectRatioFitter>();
            }

            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }
        }

        private int BeginPlaybackGeneration()
        {
            ResolveReferences();
            UnbindVideoPlayer();

            if (videoPlayer != null)
            {
                videoPlayer.sendFrameReadyEvents = false;
                videoPlayer.Stop();
            }

            playbackGeneration = playbackGeneration == int.MaxValue ? 1 : playbackGeneration + 1;
            awaitingFirstFrameGeneration = -1;
            visualPreparedGeneration = -1;
            expectedClip = null;
            expectedTargetTexture = null;
            IsVisualPrepared = false;
            return playbackGeneration;
        }

        private void BindVideoPlayer(VideoPlayer source, int generation)
        {
            if (source == null)
            {
                return;
            }

            UnbindVideoPlayer();
            prepareCompletedHandler = callbackSource => HandlePrepared(callbackSource, generation);
            errorReceivedHandler = (callbackSource, message) => HandleErrorReceived(callbackSource, message, generation);
            frameReadyHandler = (callbackSource, frameIndex) => HandleFrameReady(callbackSource, frameIndex, generation);
            source.prepareCompleted += prepareCompletedHandler;
            source.errorReceived += errorReceivedHandler;
            source.frameReady += frameReadyHandler;
            source.sendFrameReadyEvents = false;
            boundVideoPlayer = source;
        }

        private void UnbindVideoPlayer()
        {
            VideoPlayer source = boundVideoPlayer;
            if (source != null)
            {
                source.prepareCompleted -= prepareCompletedHandler;
                source.errorReceived -= errorReceivedHandler;
                source.frameReady -= frameReadyHandler;
                source.sendFrameReadyEvents = false;
            }

            boundVideoPlayer = null;
            prepareCompletedHandler = null;
            errorReceivedHandler = null;
            frameReadyHandler = null;
        }

        private void ConfigureVideoPlayer()
        {
            if (videoPlayer == null)
            {
                return;
            }

            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.isLooping = Loop;
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = MuteAudio ? VideoAudioOutputMode.None : VideoAudioOutputMode.Direct;
            videoPlayer.clip = ActiveClip;
            videoPlayer.targetTexture = TargetTexture;
        }

        private void HandlePrepared(VideoPlayer source, int generation)
        {
            if (!IsCurrentGeneration(source, generation) || visualPreparedGeneration == generation)
            {
                return;
            }

            RenderTexture targetTexture = expectedTargetTexture;
            if (expectedClip == null || targetTexture == null || targetImage == null)
            {
                CompleteWithFallback(generation, source);
                return;
            }

            if (source.clip != expectedClip || source.targetTexture != targetTexture || TargetTexture != targetTexture)
            {
                return;
            }

            source.targetTexture = targetTexture;
            awaitingFirstFrameGeneration = generation;
            source.sendFrameReadyEvents = true;
            source.Play();
        }

        private void HandleFrameReady(VideoPlayer source, long frameIndex, int generation)
        {
            if (!IsCurrentGeneration(source, generation)
                || visualPreparedGeneration == generation
                || awaitingFirstFrameGeneration != generation
                || frameIndex < 0)
            {
                return;
            }

            RenderTexture targetTexture = expectedTargetTexture;
            if (expectedClip == null
                || targetTexture == null
                || targetImage == null
                || source.clip != expectedClip
                || source.targetTexture != targetTexture
                || TargetTexture != targetTexture)
            {
                return;
            }

            awaitingFirstFrameGeneration = -1;
            source.sendFrameReadyEvents = false;
            int width = source.width > 0 ? (int)source.width : FallbackWidth;
            int height = source.height > 0 ? (int)source.height : FallbackHeight;
            ApplyAspect(width, height);
            ShowVideoTexture(targetTexture);
            CompleteVisualPreparation(generation);
        }

        private void HandleErrorReceived(VideoPlayer source, string message, int generation)
        {
            if (!IsCurrentGeneration(source, generation))
            {
                return;
            }

            CompleteWithFallback(generation, source);
        }

        private bool IsCurrentGeneration(VideoPlayer source, int generation)
        {
            return generation == playbackGeneration
                && source != null
                && source == videoPlayer
                && source == boundVideoPlayer;
        }

        private void CompleteWithFallback(int generation, VideoPlayer source)
        {
            if (generation != playbackGeneration)
            {
                return;
            }

            awaitingFirstFrameGeneration = -1;
            if (source != null && source == boundVideoPlayer)
            {
                source.sendFrameReadyEvents = false;
            }

            RestoreFallback();
            CompleteVisualPreparation(generation);
        }

        private void CompleteVisualPreparation(int generation)
        {
            if (generation != playbackGeneration || visualPreparedGeneration == generation)
            {
                return;
            }

            visualPreparedGeneration = generation;
            IsVisualPrepared = true;
            VisualPrepared?.Invoke();
        }

        private void ShowVideoTexture(RenderTexture targetTexture)
        {
            if (targetImage == null || targetTexture == null)
            {
                return;
            }

            targetImage.texture = targetTexture;
            targetImage.color = Color.white;
        }

        private void RestoreFallback()
        {
            if (targetImage == null || !fallbackCached)
            {
                return;
            }

            targetImage.texture = fallbackTexture;
            targetImage.color = fallbackColor;
            ApplyAspect(FallbackWidth, FallbackHeight);
        }

        private void ApplyAspect(int width, int height)
        {
            if (aspectRatioFitter == null || height <= 0)
            {
                return;
            }

            aspectRatioFitter.aspectRatio = (float)width / height;
        }
    }
}
