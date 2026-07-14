using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Core
{
    public enum MobilePerformanceTier
    {
        Low,
        Balanced,
        High
    }

    public readonly struct MobilePerformanceProfile
    {
        public MobilePerformanceProfile(
            int targetFrameRate,
            float renderScale,
            float minimumRenderScale,
            int globalTextureMipmapLimit,
            float lodBias,
            float shadowDistance,
            int maxAdditionalLights,
            float streamingMipmapsMemoryBudget,
            int streamingMipmapsRenderersPerFrame,
            int streamingMipmapsMaxLevelReduction,
            int streamingMipmapsMaxFileIoRequests)
        {
            TargetFrameRate = targetFrameRate;
            RenderScale = renderScale;
            MinimumRenderScale = minimumRenderScale;
            GlobalTextureMipmapLimit = globalTextureMipmapLimit;
            LodBias = lodBias;
            ShadowDistance = shadowDistance;
            MaxAdditionalLights = maxAdditionalLights;
            StreamingMipmapsMemoryBudget = streamingMipmapsMemoryBudget;
            StreamingMipmapsRenderersPerFrame = streamingMipmapsRenderersPerFrame;
            StreamingMipmapsMaxLevelReduction = streamingMipmapsMaxLevelReduction;
            StreamingMipmapsMaxFileIoRequests = streamingMipmapsMaxFileIoRequests;
        }

        public int TargetFrameRate { get; }
        public float RenderScale { get; }
        public float MinimumRenderScale { get; }
        public int GlobalTextureMipmapLimit { get; }
        public float LodBias { get; }
        public float ShadowDistance { get; }
        public int MaxAdditionalLights { get; }
        public float StreamingMipmapsMemoryBudget { get; }
        public int StreamingMipmapsRenderersPerFrame { get; }
        public int StreamingMipmapsMaxLevelReduction { get; }
        public int StreamingMipmapsMaxFileIoRequests { get; }
    }

    public static class MobileFrameRateBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
#if UNITY_EDITOR
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            return;
#else
            if (!Application.isMobilePlatform
                || Object.FindAnyObjectByType<MobilePerformanceGovernor>() != null)
            {
                return;
            }

            GameObject root = new("[MobilePerformanceGovernor]");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<MobilePerformanceGovernor>();
#endif
        }
    }

    [DisallowMultipleComponent]
    public sealed class MobilePerformanceGovernor : MonoBehaviour
    {
        private const float SampleWindowSeconds = 2f;
        private const float AdjustmentCooldownSeconds = 12f;
        private const int SlowWindowsBeforeAdjustment = 3;
        private const int StableWindowsBeforeRecovery = 10;
        private const float RenderScaleStep = 0.05f;
        private const int ModerateThermalStatus = 2;
        private const int SevereThermalStatus = 3;

        private UniversalRenderPipelineAsset pipelineAsset;
        private MobilePerformanceTier currentTier;
        private MobilePerformanceProfile currentProfile;
        private float sampledSeconds;
        private float sampledFrameSeconds;
        private int sampledFrames;
        private int slowWindowCount;
        private int stableWindowCount;
        private float lastAdjustmentTime = float.NegativeInfinity;
        private int currentThermalStatus = -1;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject powerManager;
#endif

        public static MobilePerformanceGovernor ActiveInstance { get; private set; }
        public MobilePerformanceTier CurrentTier => currentTier;
        public float CurrentRenderScale => pipelineAsset != null ? pipelineAsset.renderScale : 1f;
        public int CurrentThermalStatus => currentThermalStatus;

        private void Awake()
        {
            ActiveInstance = this;
            pipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            currentTier = SelectInitialTier(
                SystemInfo.systemMemorySize,
                SystemInfo.graphicsMemorySize,
                SystemInfo.processorCount,
                SystemInfo.processorFrequency,
                SystemInfo.graphicsShaderLevel);
            ApplyTier(currentTier);
        }

        private void OnDestroy()
        {
            if (ActiveInstance == this)
            {
                ActiveInstance = null;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            powerManager?.Dispose();
            powerManager = null;
#endif
        }

        private void OnEnable()
        {
            Application.lowMemory += HandleLowMemory;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        private void OnDisable()
        {
            Application.lowMemory -= HandleLowMemory;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void Update()
        {
            if (!IsMonitoredCombatScene(SceneManager.GetActiveScene().name))
            {
                ResetSamples();
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            if (deltaTime <= 0.001f || deltaTime >= 0.1f)
            {
                return;
            }

            sampledSeconds += deltaTime;
            sampledFrameSeconds += deltaTime;
            sampledFrames++;
            if (sampledSeconds < SampleWindowSeconds || sampledFrames <= 0)
            {
                return;
            }

            float averageFrameSeconds = sampledFrameSeconds / sampledFrames;
            currentThermalStatus = ReadThermalStatus();
            MobilePerformanceTier thermallyConstrainedTier = GetThermallyConstrainedTier(
                currentTier,
                currentThermalStatus);
            if (thermallyConstrainedTier < currentTier)
            {
                ApplyTier(thermallyConstrainedTier);
                slowWindowCount = 0;
                stableWindowCount = 0;
                lastAdjustmentTime = Time.unscaledTime;
            }
            else
            {
                EvaluateFrameWindow(averageFrameSeconds);
            }

            ResetSamples();
        }

        private void EvaluateFrameWindow(float averageFrameSeconds)
        {
            float targetFrameSeconds = 1f / Mathf.Max(1, currentProfile.TargetFrameRate);
            bool slow = averageFrameSeconds > targetFrameSeconds * 1.2f;
            bool stable = averageFrameSeconds <= targetFrameSeconds * 1.05f;
            slowWindowCount = slow ? slowWindowCount + 1 : 0;
            stableWindowCount = stable ? stableWindowCount + 1 : 0;

            if (Time.unscaledTime - lastAdjustmentTime < AdjustmentCooldownSeconds)
            {
                return;
            }

            if (slowWindowCount >= SlowWindowsBeforeAdjustment)
            {
                ReduceSustainedLoad();
                slowWindowCount = 0;
                stableWindowCount = 0;
                lastAdjustmentTime = Time.unscaledTime;
                return;
            }

            if (stableWindowCount >= StableWindowsBeforeRecovery)
            {
                RecoverRenderScale();
                stableWindowCount = 0;
                lastAdjustmentTime = Time.unscaledTime;
            }
        }

        private void ReduceSustainedLoad()
        {
            if (pipelineAsset != null
                && pipelineAsset.renderScale > currentProfile.MinimumRenderScale + 0.001f)
            {
                pipelineAsset.renderScale = Mathf.Max(
                    currentProfile.MinimumRenderScale,
                    pipelineAsset.renderScale - RenderScaleStep);
                return;
            }

            if (currentTier > MobilePerformanceTier.Low)
            {
                ApplyTier(currentTier - 1);
            }
        }

        private void RecoverRenderScale()
        {
            if (pipelineAsset == null || pipelineAsset.renderScale >= currentProfile.RenderScale - 0.001f)
            {
                return;
            }

            pipelineAsset.renderScale = Mathf.Min(
                currentProfile.RenderScale,
                pipelineAsset.renderScale + RenderScaleStep);
        }

        private void ApplyTier(MobilePerformanceTier tier)
        {
            currentTier = tier;
            currentProfile = GetProfile(tier);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = currentProfile.TargetFrameRate;
            QualitySettings.globalTextureMipmapLimit = currentProfile.GlobalTextureMipmapLimit;
            QualitySettings.lodBias = currentProfile.LodBias;
            QualitySettings.shadowDistance = currentProfile.ShadowDistance;
            QualitySettings.pixelLightCount = currentProfile.MaxAdditionalLights;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = currentProfile.StreamingMipmapsMemoryBudget;
            QualitySettings.streamingMipmapsRenderersPerFrame = currentProfile.StreamingMipmapsRenderersPerFrame;
            QualitySettings.streamingMipmapsMaxLevelReduction = currentProfile.StreamingMipmapsMaxLevelReduction;
            QualitySettings.streamingMipmapsMaxFileIORequests = currentProfile.StreamingMipmapsMaxFileIoRequests;

            if (pipelineAsset != null)
            {
                pipelineAsset.renderScale = currentProfile.RenderScale;
                pipelineAsset.shadowDistance = currentProfile.ShadowDistance;
                pipelineAsset.maxAdditionalLightsCount = currentProfile.MaxAdditionalLights;
            }

            OlympusMobileRenderBudgetBootstrap.ApplyToScene(
                SceneManager.GetActiveScene(),
                currentTier);
        }

        private void HandleLowMemory()
        {
            ApplyTier(MobilePerformanceTier.Low);
            Resources.UnloadUnusedAssets();
            lastAdjustmentTime = Time.unscaledTime;
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene nextScene)
        {
            ResetSamples();
            slowWindowCount = 0;
            stableWindowCount = 0;
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused)
            {
                ResetSamples();
                slowWindowCount = 0;
                stableWindowCount = 0;
            }
        }

        private void ResetSamples()
        {
            sampledSeconds = 0f;
            sampledFrameSeconds = 0f;
            sampledFrames = 0;
        }

        public static MobilePerformanceTier SelectInitialTier(
            int systemMemoryMegabytes,
            int graphicsMemoryMegabytes,
            int processorCount,
            int processorFrequencyMegahertz,
            int graphicsShaderLevel)
        {
            if ((systemMemoryMegabytes > 0 && systemMemoryMegabytes <= 4096)
                || (graphicsMemoryMegabytes > 0 && graphicsMemoryMegabytes <= 1024)
                || processorCount <= 4
                || graphicsShaderLevel < 45)
            {
                return MobilePerformanceTier.Low;
            }

            if (systemMemoryMegabytes >= 8192
                && processorCount >= 8
                && processorFrequencyMegahertz >= 2200
                && graphicsShaderLevel >= 45
                && (graphicsMemoryMegabytes <= 0 || graphicsMemoryMegabytes >= 3072))
            {
                return MobilePerformanceTier.High;
            }

            return MobilePerformanceTier.Balanced;
        }

        public static MobilePerformanceProfile GetProfile(MobilePerformanceTier tier)
        {
            switch (tier)
            {
                case MobilePerformanceTier.Low:
                    return new MobilePerformanceProfile(
                        30,
                        0.68f,
                        0.60f,
                        1,
                        0.8f,
                        28f,
                        2,
                        192f,
                        64,
                        3,
                        64);
                case MobilePerformanceTier.High:
                    return new MobilePerformanceProfile(
                        60,
                        0.9f,
                        0.8f,
                        0,
                        1.25f,
                        50f,
                        4,
                        384f,
                        256,
                        1,
                        256);
                default:
                    return new MobilePerformanceProfile(
                        60,
                        0.8f,
                        0.68f,
                        0,
                        1f,
                        40f,
                        3,
                        256f,
                        128,
                        2,
                        128);
            }
        }

        public static MobilePerformanceTier GetThermallyConstrainedTier(
            MobilePerformanceTier currentTier,
            int thermalStatus)
        {
            if (thermalStatus >= SevereThermalStatus)
            {
                return MobilePerformanceTier.Low;
            }

            if (thermalStatus >= ModerateThermalStatus
                && currentTier > MobilePerformanceTier.Balanced)
            {
                return MobilePerformanceTier.Balanced;
            }

            return currentTier;
        }

        private int ReadThermalStatus()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (powerManager == null)
                {
                    using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
                    using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    powerManager = activity.Call<AndroidJavaObject>("getSystemService", "power");
                }

                return powerManager != null ? powerManager.Call<int>("getCurrentThermalStatus") : -1;
            }
            catch (System.Exception)
            {
                return -1;
            }
#else
            return -1;
#endif
        }

        private static bool IsMonitoredCombatScene(string sceneName)
        {
            return sceneName == "OlympusCorridorInvasionStage"
                || sceneName == "OlympusStationCombatStage";
        }
    }
}
