using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(VideoPlayer))]
    public sealed class LoginVideoBackgroundPresenter : MonoBehaviour
    {
        [SerializeField] private RawImage targetImage;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private LoginVideoBackgroundProfile profile;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private bool playOnEnable = true;

        private Texture fallbackTexture;
        private Color fallbackColor;
        private bool fallbackCached;
        private bool videoEventsBound;
        private VideoClip runtimeClipOverride;

        private VideoClip ActiveClip => runtimeClipOverride != null ? runtimeClipOverride : profile != null ? profile.BackgroundClip : null;
        private RenderTexture TargetTexture => profile != null ? profile.TargetTexture : null;
        private bool Loop => profile == null || profile.Loop;
        private bool MuteAudio => profile == null || profile.MuteAudio;
        private bool HideVideoUntilPrepared => profile == null || profile.HideVideoUntilPrepared;
        private int FallbackWidth => profile != null ? profile.FallbackWidth : 2560;
        private int FallbackHeight => profile != null ? profile.FallbackHeight : 1440;

        private void Reset()
        {
            targetImage = GetComponent<RawImage>();
            aspectRatioFitter = GetComponent<AspectRatioFitter>();
            videoPlayer = GetComponent<VideoPlayer>();
        }

        private void Awake()
        {
            CacheFallback();
            BindVideoPlayer();
        }

        private void OnEnable()
        {
            CacheFallback();
            BindVideoPlayer();
            ConfigureVideoPlayer();

            if (playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }

            RestoreFallback();
        }

        private void OnDestroy()
        {
            UnbindVideoPlayer();
        }

        public void SetClip(VideoClip clip, bool playImmediately = true)
        {
            runtimeClipOverride = clip;
            ConfigureVideoPlayer();

            if (playImmediately && isActiveAndEnabled)
            {
                Play();
            }
        }

        public void Play()
        {
            VideoClip clip = ActiveClip;
            RenderTexture targetTexture = TargetTexture;
            if (clip == null || videoPlayer == null || targetImage == null || targetTexture == null)
            {
                RestoreFallback();
                return;
            }

            videoPlayer.targetTexture = targetTexture;
            videoPlayer.clip = clip;

            if (!HideVideoUntilPrepared)
            {
                ShowVideoTexture(targetTexture);
            }

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

        private void BindVideoPlayer()
        {
            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }

            if (videoPlayer == null || videoEventsBound)
            {
                return;
            }

            videoPlayer.prepareCompleted += HandlePrepared;
            videoPlayer.errorReceived += HandleErrorReceived;
            videoEventsBound = true;
        }

        private void UnbindVideoPlayer()
        {
            if (videoPlayer == null || !videoEventsBound)
            {
                return;
            }

            videoPlayer.prepareCompleted -= HandlePrepared;
            videoPlayer.errorReceived -= HandleErrorReceived;
            videoEventsBound = false;
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
        }

        private void HandlePrepared(VideoPlayer source)
        {
            int width = source.width > 0 ? (int)source.width : FallbackWidth;
            int height = source.height > 0 ? (int)source.height : FallbackHeight;
            RenderTexture targetTexture = TargetTexture;
            if (targetTexture == null)
            {
                RestoreFallback();
                return;
            }

            source.targetTexture = targetTexture;
            ApplyAspect(width, height);
            ShowVideoTexture(targetTexture);
            source.Play();
        }

        private void HandleErrorReceived(VideoPlayer source, string message)
        {
            RestoreFallback();
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
