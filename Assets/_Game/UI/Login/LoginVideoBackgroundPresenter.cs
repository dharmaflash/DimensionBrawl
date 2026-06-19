using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    public sealed class LoginVideoBackgroundPresenter : MonoBehaviour
    {
        [SerializeField] private RawImage targetImage;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private LoginVideoBackgroundProfile profile;
        [SerializeField] private bool playOnEnable = true;

        private VideoPlayer videoPlayer;
        private RenderTexture renderTexture;
        private Texture fallbackTexture;
        private Color fallbackColor;
        private bool fallbackCached;
        private VideoClip runtimeClipOverride;

        private VideoClip ActiveClip => runtimeClipOverride != null ? runtimeClipOverride : profile != null ? profile.BackgroundClip : null;
        private bool Loop => profile == null || profile.Loop;
        private bool MuteAudio => profile == null || profile.MuteAudio;
        private bool HideVideoUntilPrepared => profile == null || profile.HideVideoUntilPrepared;
        private int FallbackWidth => profile != null ? profile.FallbackWidth : 2560;
        private int FallbackHeight => profile != null ? profile.FallbackHeight : 1440;

        private void Reset()
        {
            targetImage = GetComponent<RawImage>();
            aspectRatioFitter = GetComponent<AspectRatioFitter>();
        }

        private void Awake()
        {
            CacheFallback();
            EnsureVideoPlayer();
        }

        private void OnEnable()
        {
            CacheFallback();
            EnsureVideoPlayer();
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
            ReleaseRenderTexture();
        }

        private void OnDestroy()
        {
            ReleaseRenderTexture();
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
            if (clip == null || videoPlayer == null || targetImage == null)
            {
                RestoreFallback();
                return;
            }

            EnsureRenderTexture(FallbackWidth, FallbackHeight);
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.clip = clip;

            if (!HideVideoUntilPrepared)
            {
                ShowVideoTexture();
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

        private void EnsureVideoPlayer()
        {
            if (videoPlayer != null)
            {
                return;
            }

            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            videoPlayer.prepareCompleted += HandlePrepared;
            videoPlayer.errorReceived += HandleErrorReceived;
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
            EnsureRenderTexture(width, height);
            source.targetTexture = renderTexture;
            ApplyAspect(width, height);
            ShowVideoTexture();
            source.Play();
        }

        private void HandleErrorReceived(VideoPlayer source, string message)
        {
            RestoreFallback();
        }

        private void ShowVideoTexture()
        {
            if (targetImage == null || renderTexture == null)
            {
                return;
            }

            targetImage.texture = renderTexture;
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

        private void EnsureRenderTexture(int width, int height)
        {
            width = Mathf.Max(16, width);
            height = Mathf.Max(16, height);

            if (renderTexture != null && renderTexture.width == width && renderTexture.height == height)
            {
                return;
            }

            ReleaseRenderTexture();
            renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                name = "LoginVideoBackground"
            };
            renderTexture.Create();
        }

        private void ReleaseRenderTexture()
        {
            if (renderTexture == null)
            {
                return;
            }

            if (targetImage != null && targetImage.texture == renderTexture)
            {
                targetImage.texture = null;
            }

            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
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
