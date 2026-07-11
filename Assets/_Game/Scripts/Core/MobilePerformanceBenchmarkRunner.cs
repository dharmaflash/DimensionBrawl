using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Core;
using DimensionBrawl.Player;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Debugging
{
    public static class MobilePerformanceBenchmarkBootstrap
    {
#if DEVELOPMENT_BUILD && DIMENSIONBRAWL_MOBILE_PERF && (UNITY_ANDROID || UNITY_IOS)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateRunner()
        {
            if (UnityEngine.Object.FindAnyObjectByType<MobilePerformanceBenchmarkRunner>() != null)
            {
                return;
            }

            GameObject root = new("[MobilePerformanceBenchmark]");
            UnityEngine.Object.DontDestroyOnLoad(root);
            root.AddComponent<MobilePerformanceBenchmarkRunner>();
        }
#endif
    }

    [DisallowMultipleComponent]
    public sealed class MobilePerformanceBenchmarkRunner : MonoBehaviour
    {
        private const string ReportFileName = "DimensionBrawl-MobilePerformance.json";
        private const float WarmupSeconds = 10f;
        private const float SampleSeconds = 60f;
        private const int MaximumSamples = 7200;
        private const string LogPrefix = "[MOBILE_PERF]";

        private static readonly BenchmarkScene[] Scenes =
        {
            new("Olympus Station Combat", "Assets/_Game/Scenes/OlympusStationCombatStage.unity"),
            new("Olympus Corridor", "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity"),
            new("Boss Barrage", "Assets/_Game/Scenes/ActionFoundationBossBarrageLaneReview.unity"),
            new("Frontline Motivation", "Assets/_Game/Scenes/ActionFoundationFrontlineMotivationReview.unity")
        };

        private readonly double[] frameMilliseconds = new double[MaximumSamples];
        private readonly double[] mainThreadMilliseconds = new double[MaximumSamples];
        private readonly double[] renderThreadMilliseconds = new double[MaximumSamples];
        private readonly double[] gcAllocatedKibibytes = new double[MaximumSamples];
        private readonly double[] totalUsedMemoryMebibytes = new double[MaximumSamples];
        private readonly double[] gfxUsedMemoryMebibytes = new double[MaximumSamples];
        private readonly double[] drawCalls = new double[MaximumSamples];
        private readonly double[] setPassCalls = new double[MaximumSamples];
        private readonly double[] triangles = new double[MaximumSamples];
        private readonly double[] renderScales = new double[MaximumSamples];
        private readonly double[] textureCurrentMemoryMebibytes = new double[MaximumSamples];
        private readonly double[] textureDesiredMemoryMebibytes = new double[MaximumSamples];
        private readonly double[] textureTargetMemoryMebibytes = new double[MaximumSamples];
        private readonly double[] textureTotalMemoryMebibytes = new double[MaximumSamples];
        private readonly double[] nonStreamingTextureMemoryMebibytes = new double[MaximumSamples];
        private readonly double[] streamingTexturePendingLoads = new double[MaximumSamples];
        private readonly double[] streamingTextureLoading = new double[MaximumSamples];
        private readonly double[] streamingMipmapUploads = new double[MaximumSamples];
        private readonly double[] globalTextureMipmapLimits = new double[MaximumSamples];
        private readonly double[] streamingMemoryBudgets = new double[MaximumSamples];
        private readonly double[] lodBiases = new double[MaximumSamples];
        private readonly double[] shadowDistances = new double[MaximumSamples];
        private readonly int[] thermalStatusSampleCounts = new int[7];

        private ProfilerRecorder mainThreadRecorder;
        private ProfilerRecorder renderThreadRecorder;
        private ProfilerRecorder gcAllocatedRecorder;
        private ProfilerRecorder totalUsedMemoryRecorder;
        private ProfilerRecorder gfxUsedMemoryRecorder;
        private ProfilerRecorder drawCallsRecorder;
        private ProfilerRecorder setPassCallsRecorder;
        private ProfilerRecorder trianglesRecorder;
        private MobilePerformanceBenchmarkReport report;
        private PlayerMovementController movement;
        private PlayerActionController actionController;
        private PlayerCombatModeController combatModeController;
        private PlayerRangedBasicAttackAction rangedAttack;
        private PlayerSkill1Action skill1;
        private PlayerSummonSlot1Action summonSlot1;
        private MobilePerformanceGovernor performanceGovernor;
        private CombatHealth[] combatHealth = Array.Empty<CombatHealth>();
        private int sampleCount;
        private int frameBudgetMissCount;
        private int framesOver50Milliseconds;
        private int framesOver100Milliseconds;
        private int tierChangeCount;
        private int minimumTargetFrameRate;
        private int maximumTargetFrameRate;
        private int maximumThermalStatus = -1;
        private int thermalStatusSampleCount;
        private int lowMemoryEventCount;
        private float minimumBatteryLevel = -1f;
        private bool hasPerformanceGovernor;
        private MobilePerformanceTier initialPerformanceTier;
        private MobilePerformanceTier lastPerformanceTier;
        private float nextDeviceStatusSampleTime;
        private float nextDodgeTime;
        private float nextSkillTime;
        private float nextSummonTime;
        private float nextSwapTime;
        private float nextHealthResetTime;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject powerManager;
#endif

        public static IReadOnlyList<string> CanonicalScenePaths
        {
            get
            {
                string[] paths = new string[Scenes.Length];
                for (int i = 0; i < Scenes.Length; i++)
                {
                    paths[i] = Scenes[i].Path;
                }

                return paths;
            }
        }

        private void Awake()
        {
            report = CreateReport();
            StartRecorders();
        }

        private IEnumerator Start()
        {
            yield return null;
            for (int sceneIndex = 0; sceneIndex < Scenes.Length; sceneIndex++)
            {
                BenchmarkScene target = Scenes[sceneIndex];
                float sceneLoadStartedAt = Time.realtimeSinceStartup;
                yield return LoadBenchmarkScene(target.Path);
                float sceneLoadSeconds = Time.realtimeSinceStartup - sceneLoadStartedAt;
                ResolveCombatDriver();
                Debug.Log($"{LogPrefix} warmup {target.Label} for {WarmupSeconds:0}s");

                float warmupStartedAt = Time.realtimeSinceStartup;
                float warmupDeadline = warmupStartedAt + WarmupSeconds;
                while (Time.realtimeSinceStartup < warmupDeadline)
                {
                    DriveCombatLoad(Time.realtimeSinceStartup - warmupStartedAt);
                    yield return null;
                }

                ResetCombatSchedule();
                ResetSamples();
                Debug.Log($"{LogPrefix} sampling {target.Label} for {SampleSeconds:0}s");
                float sampleStartedAt = Time.realtimeSinceStartup;
                float sampleDeadline = sampleStartedAt + SampleSeconds;
                while (Time.realtimeSinceStartup < sampleDeadline && sampleCount < MaximumSamples)
                {
                    DriveCombatLoad(Time.realtimeSinceStartup - sampleStartedAt);
                    SampleFrame();
                    yield return null;
                }

                ReleaseCombatInputs();
                MobilePerformanceSceneResult result = BuildSceneResult(
                    target,
                    Time.realtimeSinceStartup - sampleStartedAt,
                    sceneLoadSeconds);
                report.Scenes.Add(result);
                report.CurrentScene = target.Label;
                WriteReport(completed: false);
                Debug.Log(
                    $"{LogPrefix} {target.Label} complete: " +
                    $"frame p95={result.FrameMilliseconds.P95:0.00}ms, " +
                    $"main p95={result.MainThreadMilliseconds.P95:0.00}ms, " +
                    $"GC={result.GcAllocatedKibibytes.Average:0.00}KiB/frame");
                yield return null;
            }

            report.CurrentScene = string.Empty;
            WriteReport(completed: true);
            Debug.Log($"{LogPrefix} COMPLETE path={GetReportPath()}");
#if !UNITY_EDITOR
            yield return new WaitForSecondsRealtime(2f);
            Application.Quit(0);
#endif
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && report != null)
            {
                WriteReport(completed: false);
            }
        }

        private void OnEnable()
        {
            Application.lowMemory += HandleLowMemory;
        }

        private void OnDisable()
        {
            Application.lowMemory -= HandleLowMemory;
        }

        private void OnDestroy()
        {
            ReleaseCombatInputs();
            DisposeRecorders();
#if UNITY_ANDROID && !UNITY_EDITOR
            powerManager?.Dispose();
            powerManager = null;
#endif
        }

        private void HandleLowMemory()
        {
            lowMemoryEventCount++;
        }

        private static MobilePerformanceBenchmarkReport CreateReport()
        {
            return new MobilePerformanceBenchmarkReport
            {
                GeneratedUtc = DateTime.UtcNow.ToString("O"),
                UnityVersion = Application.unityVersion,
                Platform = Application.platform.ToString(),
                DeviceModel = SystemInfo.deviceModel,
                DeviceName = SystemInfo.deviceName,
                OperatingSystem = SystemInfo.operatingSystem,
                Processor = SystemInfo.processorType,
                ProcessorCount = SystemInfo.processorCount,
                ProcessorFrequencyMegahertz = SystemInfo.processorFrequency,
                SystemMemoryMegabytes = SystemInfo.systemMemorySize,
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                GraphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                GraphicsMemoryMegabytes = SystemInfo.graphicsMemorySize,
                GraphicsShaderLevel = SystemInfo.graphicsShaderLevel,
                ScreenWidth = Screen.width,
                ScreenHeight = Screen.height,
                WarmupSeconds = WarmupSeconds,
                SampleSecondsPerScene = SampleSeconds
            };
        }

        private IEnumerator LoadBenchmarkScene(string scenePath)
        {
            if (string.Equals(SceneManager.GetActiveScene().path, scenePath, StringComparison.Ordinal))
            {
                yield return null;
                yield return null;
                yield break;
            }

            AsyncOperation load = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
            if (load == null)
            {
                throw new InvalidOperationException($"Failed to start benchmark scene load: {scenePath}");
            }

            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return null;
        }

        private void ResolveCombatDriver()
        {
            movement = FindFirstObjectByType<PlayerMovementController>();
            actionController = FindFirstObjectByType<PlayerActionController>();
            combatModeController = FindFirstObjectByType<PlayerCombatModeController>();
            rangedAttack = FindFirstObjectByType<PlayerRangedBasicAttackAction>();
            skill1 = FindFirstObjectByType<PlayerSkill1Action>();
            summonSlot1 = FindFirstObjectByType<PlayerSummonSlot1Action>();
            performanceGovernor = FindAnyObjectByType<MobilePerformanceGovernor>();
            combatHealth = FindObjectsByType<CombatHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            ResetCombatSchedule();
        }

        private void ResetCombatSchedule()
        {
            nextDodgeTime = 0f;
            nextSkillTime = 0f;
            nextSummonTime = 0f;
            nextSwapTime = 0f;
            nextHealthResetTime = 0f;
        }

        private void DriveCombatLoad(float elapsedSeconds)
        {
            if (movement != null)
            {
                Vector2 move = new(
                    Mathf.Sin(elapsedSeconds * 0.83f),
                    Mathf.Cos(elapsedSeconds * 0.57f));
                movement.SetMoveInput(Vector2.ClampMagnitude(move, 0.72f));
            }

            if (rangedAttack != null)
            {
                rangedAttack.SetFireHeld(elapsedSeconds % 6f < 4.8f);
            }

            if (elapsedSeconds >= nextDodgeTime)
            {
                actionController?.QueueDodge();
                nextDodgeTime = elapsedSeconds + 3.1f;
            }

            if (elapsedSeconds >= nextSkillTime)
            {
                skill1?.QueueSkill1();
                nextSkillTime = elapsedSeconds + 5.3f;
            }

            if (elapsedSeconds >= nextSummonTime)
            {
                summonSlot1?.QueueSummonSlot1();
                nextSummonTime = elapsedSeconds + 8.7f;
            }

            if (elapsedSeconds >= nextSwapTime)
            {
                combatModeController?.QueueCombatModeSwap();
                nextSwapTime = elapsedSeconds + 17.9f;
            }

            if (elapsedSeconds >= nextHealthResetTime)
            {
                for (int i = 0; i < combatHealth.Length; i++)
                {
                    CombatHealth health = combatHealth[i];
                    if (health != null && (!health.IsAlive || health.HealthRatio < 0.3f))
                    {
                        health.ResetHealthToFull();
                    }
                }

                nextHealthResetTime = elapsedSeconds + 1f;
            }
        }

        private void ReleaseCombatInputs()
        {
            movement?.SetMoveInput(Vector2.zero);
            rangedAttack?.SetFireHeld(false);
        }

        private void StartRecorders()
        {
            mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            renderThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread", 1);
            gcAllocatedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            totalUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1);
            gfxUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory", 1);
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1);
            setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 1);
            trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", 1);
        }

        private void DisposeRecorders()
        {
            mainThreadRecorder.Dispose();
            renderThreadRecorder.Dispose();
            gcAllocatedRecorder.Dispose();
            totalUsedMemoryRecorder.Dispose();
            gfxUsedMemoryRecorder.Dispose();
            drawCallsRecorder.Dispose();
            setPassCallsRecorder.Dispose();
            trianglesRecorder.Dispose();
        }

        private void ResetSamples()
        {
            sampleCount = 0;
            frameBudgetMissCount = 0;
            framesOver50Milliseconds = 0;
            framesOver100Milliseconds = 0;
            tierChangeCount = 0;
            minimumTargetFrameRate = int.MaxValue;
            maximumTargetFrameRate = 0;
            maximumThermalStatus = -1;
            thermalStatusSampleCount = 0;
            Array.Clear(thermalStatusSampleCounts, 0, thermalStatusSampleCounts.Length);
            lowMemoryEventCount = 0;
            minimumBatteryLevel = -1f;
            nextDeviceStatusSampleTime = 0f;
            hasPerformanceGovernor = performanceGovernor != null;
            if (hasPerformanceGovernor)
            {
                initialPerformanceTier = performanceGovernor.CurrentTier;
                lastPerformanceTier = initialPerformanceTier;
            }
        }

        private void SampleFrame()
        {
            if (sampleCount >= MaximumSamples)
            {
                return;
            }

            int index = sampleCount++;
            double frameDurationMilliseconds = Time.unscaledDeltaTime * 1000d;
            frameMilliseconds[index] = frameDurationMilliseconds;
            mainThreadMilliseconds[index] = RecorderNanosecondsToMilliseconds(mainThreadRecorder);
            renderThreadMilliseconds[index] = RecorderNanosecondsToMilliseconds(renderThreadRecorder);
            gcAllocatedKibibytes[index] = RecorderBytesToKibibytes(gcAllocatedRecorder);
            totalUsedMemoryMebibytes[index] = RecorderBytesToMebibytes(totalUsedMemoryRecorder);
            gfxUsedMemoryMebibytes[index] = RecorderBytesToMebibytes(gfxUsedMemoryRecorder);
            drawCalls[index] = RecorderValue(drawCallsRecorder);
            setPassCalls[index] = RecorderValue(setPassCallsRecorder);
            triangles[index] = RecorderValue(trianglesRecorder);
            renderScales[index] = performanceGovernor != null
                ? performanceGovernor.CurrentRenderScale
                : 1d;
            textureCurrentMemoryMebibytes[index] = BytesToMebibytes(Texture.currentTextureMemory);
            textureDesiredMemoryMebibytes[index] = BytesToMebibytes(Texture.desiredTextureMemory);
            textureTargetMemoryMebibytes[index] = BytesToMebibytes(Texture.targetTextureMemory);
            textureTotalMemoryMebibytes[index] = BytesToMebibytes(Texture.totalTextureMemory);
            nonStreamingTextureMemoryMebibytes[index] = BytesToMebibytes(Texture.nonStreamingTextureMemory);
            streamingTexturePendingLoads[index] = Texture.streamingTexturePendingLoadCount;
            streamingTextureLoading[index] = Texture.streamingTextureLoadingCount;
            streamingMipmapUploads[index] = Texture.streamingMipmapUploadCount;
            globalTextureMipmapLimits[index] = QualitySettings.globalTextureMipmapLimit;
            streamingMemoryBudgets[index] = QualitySettings.streamingMipmapsMemoryBudget;
            lodBiases[index] = QualitySettings.lodBias;
            shadowDistances[index] = QualitySettings.shadowDistance;

            int targetFrameRate = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
            minimumTargetFrameRate = Math.Min(minimumTargetFrameRate, targetFrameRate);
            maximumTargetFrameRate = Math.Max(maximumTargetFrameRate, targetFrameRate);
            if (frameDurationMilliseconds > (1000d / targetFrameRate) * 1.05d)
            {
                frameBudgetMissCount++;
            }

            if (frameDurationMilliseconds > 50d)
            {
                framesOver50Milliseconds++;
            }

            if (frameDurationMilliseconds > 100d)
            {
                framesOver100Milliseconds++;
            }

            if (performanceGovernor != null && performanceGovernor.CurrentTier != lastPerformanceTier)
            {
                tierChangeCount++;
                lastPerformanceTier = performanceGovernor.CurrentTier;
            }

            if (Time.realtimeSinceStartup >= nextDeviceStatusSampleTime)
            {
                int thermalStatus = ReadThermalStatus();
                maximumThermalStatus = Mathf.Max(maximumThermalStatus, thermalStatus);
                if (thermalStatus >= 0 && thermalStatus < thermalStatusSampleCounts.Length)
                {
                    thermalStatusSampleCounts[thermalStatus]++;
                    thermalStatusSampleCount++;
                }
                float batteryLevel = SystemInfo.batteryLevel;
                if (batteryLevel >= 0f)
                {
                    minimumBatteryLevel = minimumBatteryLevel < 0f
                        ? batteryLevel
                        : Mathf.Min(minimumBatteryLevel, batteryLevel);
                }

                nextDeviceStatusSampleTime = Time.realtimeSinceStartup + 1f;
            }
        }

        private MobilePerformanceSceneResult BuildSceneResult(
            BenchmarkScene scene,
            float elapsedSeconds,
            float sceneLoadSeconds)
        {
            MobilePerformanceMetricSummary frameSummary = MobilePerformanceStatistics.Summarize(
                frameMilliseconds,
                sampleCount,
                "Frame",
                "ms",
                valid: true);
            MobilePerformanceSceneResult result = new()
            {
                Label = scene.Label,
                ScenePath = scene.Path,
                SceneLoadSeconds = sceneLoadSeconds,
                SampleCount = sampleCount,
                ElapsedSeconds = elapsedSeconds,
                PerformanceTier = performanceGovernor != null
                    ? performanceGovernor.CurrentTier.ToString()
                    : "Unavailable",
                InitialPerformanceTier = hasPerformanceGovernor
                    ? initialPerformanceTier.ToString()
                    : "Unavailable",
                TierChangeCount = tierChangeCount,
                RenderScale = performanceGovernor != null ? performanceGovernor.CurrentRenderScale : 1f,
                TargetFrameRate = Application.targetFrameRate,
                MinimumTargetFrameRate = minimumTargetFrameRate == int.MaxValue ? 0 : minimumTargetFrameRate,
                MaximumTargetFrameRate = maximumTargetFrameRate,
                FrameBudgetMissCount = frameBudgetMissCount,
                FrameBudgetMissPercent = sampleCount > 0
                    ? frameBudgetMissCount * 100d / sampleCount
                    : 0d,
                FramesOver50Milliseconds = framesOver50Milliseconds,
                FramesOver100Milliseconds = framesOver100Milliseconds,
                AverageFramesPerSecond = MobilePerformanceStatistics.ToFramesPerSecond(frameSummary.Average),
                OnePercentLowFramesPerSecond = MobilePerformanceStatistics.ToFramesPerSecond(frameSummary.P99),
                MaximumThermalStatus = maximumThermalStatus,
                ThermalStatusSampleCount = thermalStatusSampleCount,
                ThermalStatusSampleCounts = CopyThermalStatusCounts(),
                LowMemoryEventCount = lowMemoryEventCount,
                MinimumBatteryLevel = minimumBatteryLevel,
                TextureStreamingActive = QualitySettings.streamingMipmapsActive,
                StreamingTextureCount = Texture.streamingTextureCount,
                NonStreamingTextureCount = Texture.nonStreamingTextureCount,
                StreamingRendererCount = Texture.streamingRendererCount,
                FrameMilliseconds = frameSummary,
                MainThreadMilliseconds = MobilePerformanceStatistics.Summarize(
                    mainThreadMilliseconds,
                    sampleCount,
                    "Main Thread",
                    "ms",
                    mainThreadRecorder.Valid),
                RenderThreadMilliseconds = MobilePerformanceStatistics.Summarize(
                    renderThreadMilliseconds,
                    sampleCount,
                    "Render Thread",
                    "ms",
                    renderThreadRecorder.Valid),
                GcAllocatedKibibytes = MobilePerformanceStatistics.Summarize(
                    gcAllocatedKibibytes,
                    sampleCount,
                    "GC Allocated",
                    "KiB/frame",
                    gcAllocatedRecorder.Valid),
                TotalUsedMemoryMebibytes = MobilePerformanceStatistics.Summarize(
                    totalUsedMemoryMebibytes,
                    sampleCount,
                    "Total Used Memory",
                    "MiB",
                    totalUsedMemoryRecorder.Valid),
                GfxUsedMemoryMebibytes = MobilePerformanceStatistics.Summarize(
                    gfxUsedMemoryMebibytes,
                    sampleCount,
                    "Gfx Used Memory",
                    "MiB",
                    gfxUsedMemoryRecorder.Valid),
                DrawCalls = MobilePerformanceStatistics.Summarize(
                    drawCalls,
                    sampleCount,
                    "Draw Calls",
                    "count",
                    drawCallsRecorder.Valid),
                SetPassCalls = MobilePerformanceStatistics.Summarize(
                    setPassCalls,
                    sampleCount,
                    "SetPass Calls",
                    "count",
                    setPassCallsRecorder.Valid),
                Triangles = MobilePerformanceStatistics.Summarize(
                    triangles,
                    sampleCount,
                    "Triangles",
                    "count",
                    trianglesRecorder.Valid),
                RenderScaleSummary = MobilePerformanceStatistics.Summarize(
                    renderScales,
                    sampleCount,
                    "Render Scale",
                    "scale",
                    valid: true),
                TextureCurrentMemoryMebibytes = MobilePerformanceStatistics.Summarize(
                    textureCurrentMemoryMebibytes,
                    sampleCount,
                    "Texture Current Memory",
                    "MiB",
                    valid: true),
                TextureDesiredMemoryMebibytes = MobilePerformanceStatistics.Summarize(
                    textureDesiredMemoryMebibytes,
                    sampleCount,
                    "Texture Desired Memory",
                    "MiB",
                    valid: true),
                TextureTargetMemoryMebibytes = MobilePerformanceStatistics.Summarize(
                    textureTargetMemoryMebibytes,
                    sampleCount,
                    "Texture Target Memory",
                    "MiB",
                    valid: true),
                TextureTotalMemoryMebibytes = MobilePerformanceStatistics.Summarize(
                    textureTotalMemoryMebibytes,
                    sampleCount,
                    "Texture Total Memory",
                    "MiB",
                    valid: true),
                NonStreamingTextureMemoryMebibytes = MobilePerformanceStatistics.Summarize(
                    nonStreamingTextureMemoryMebibytes,
                    sampleCount,
                    "Non-streaming Texture Memory",
                    "MiB",
                    valid: true),
                StreamingTexturePendingLoads = MobilePerformanceStatistics.Summarize(
                    streamingTexturePendingLoads,
                    sampleCount,
                    "Streaming Texture Pending Loads",
                    "count",
                    valid: true),
                StreamingTextureLoading = MobilePerformanceStatistics.Summarize(
                    streamingTextureLoading,
                    sampleCount,
                    "Streaming Textures Loading",
                    "count",
                    valid: true),
                StreamingMipmapUploads = MobilePerformanceStatistics.Summarize(
                    streamingMipmapUploads,
                    sampleCount,
                    "Streaming Mipmap Uploads",
                    "count",
                    valid: true),
                GlobalTextureMipmapLimit = MobilePerformanceStatistics.Summarize(
                    globalTextureMipmapLimits,
                    sampleCount,
                    "Global Texture Mipmap Limit",
                    "level",
                    valid: true),
                StreamingMemoryBudgetMebibytes = MobilePerformanceStatistics.Summarize(
                    streamingMemoryBudgets,
                    sampleCount,
                    "Streaming Memory Budget",
                    "MiB",
                    valid: true),
                LodBias = MobilePerformanceStatistics.Summarize(
                    lodBiases,
                    sampleCount,
                    "LOD Bias",
                    "scale",
                    valid: true),
                ShadowDistance = MobilePerformanceStatistics.Summarize(
                    shadowDistances,
                    sampleCount,
                    "Shadow Distance",
                    "m",
                    valid: true)
            };

            CaptureRuntimeInventory(result);
            return result;
        }

        public static void CaptureRuntimeInventory(MobilePerformanceSceneResult result)
        {
            if (result == null)
            {
                return;
            }

            ResetRuntimeInventory(result);

            Renderer[] renderers = FindObjectsByType<Renderer>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                result.ActiveRendererCount++;
                if (renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
                {
                    result.ShadowCasterCount++;
                }
            }

            Light[] lights = FindObjectsByType<Light>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].enabled && lights[i].gameObject.activeInHierarchy)
                {
                    result.ActiveLightCount++;
                }
            }

            Collider[] colliders = FindObjectsByType<Collider>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].enabled && colliders[i].gameObject.activeInHierarchy)
                {
                    result.ActiveColliderCount++;
                }
            }

            OlympusMobileEnvironmentDetailCuller[] detailCullers =
                FindObjectsByType<OlympusMobileEnvironmentDetailCuller>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
            for (int i = 0; i < detailCullers.Length; i++)
            {
                OlympusMobileEnvironmentDetailCuller detailCuller = detailCullers[i];
                result.EnvironmentDetailCandidateRendererCount += detailCuller.CandidateCount;
                result.EnvironmentDetailCulledRendererCount += detailCuller.CulledRendererCount;
                result.EnvironmentDetailCandidateColliderCount += detailCuller.CandidateColliderCount;
                result.EnvironmentDetailCulledColliderCount += detailCuller.CulledColliderCount;
            }

            CaptureRuntimeFrameLoops(result);
            result.ConsolidatedFootstepPresenterCount =
                DimensionBrawl.Presentation.MovementFootstepAudioScheduler.RegisteredPresenterCount;
        }

        private static void CaptureRuntimeFrameLoops(MobilePerformanceSceneResult result)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            Behaviour[] behaviours = FindObjectsByType<Behaviour>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            var messageMasks = new Dictionary<Type, byte>();
            var loopInventory = new Dictionary<Type, MobilePerformanceFrameLoopInventory>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour == null
                    || !behaviour.isActiveAndEnabled
                    || !ShouldCountRuntimeLoop(behaviour, activeScene))
                {
                    continue;
                }

                Type type = behaviour.GetType();
                if (!messageMasks.TryGetValue(type, out byte mask))
                {
                    mask = 0;
                    if (HasUnityMessage(type, "Update"))
                    {
                        mask |= 1;
                    }

                    if (HasUnityMessage(type, "LateUpdate"))
                    {
                        mask |= 2;
                    }

                    if (HasUnityMessage(type, "FixedUpdate"))
                    {
                        mask |= 4;
                    }

                    messageMasks.Add(type, mask);
                }

                if (mask == 0)
                {
                    continue;
                }

                if (!loopInventory.TryGetValue(type, out MobilePerformanceFrameLoopInventory inventory))
                {
                    inventory = new MobilePerformanceFrameLoopInventory
                    {
                        TypeName = type.FullName ?? type.Name
                    };
                    loopInventory.Add(type, inventory);
                    result.FrameLoops.Add(inventory);
                }

                result.ActiveFrameLoopBehaviourCount++;
                if ((mask & 1) != 0)
                {
                    result.ActiveUpdateBehaviourCount++;
                    inventory.UpdateInstances++;
                }

                if ((mask & 2) != 0)
                {
                    result.ActiveLateUpdateBehaviourCount++;
                    inventory.LateUpdateInstances++;
                }

                if ((mask & 4) != 0)
                {
                    result.ActiveFixedUpdateBehaviourCount++;
                    inventory.FixedUpdateInstances++;
                }
            }

            result.FrameLoops.Sort((left, right) =>
                string.CompareOrdinal(left.TypeName, right.TypeName));
        }

        private static void ResetRuntimeInventory(MobilePerformanceSceneResult result)
        {
            result.ActiveRendererCount = 0;
            result.ShadowCasterCount = 0;
            result.ActiveLightCount = 0;
            result.ActiveColliderCount = 0;
            result.EnvironmentDetailCandidateRendererCount = 0;
            result.EnvironmentDetailCulledRendererCount = 0;
            result.EnvironmentDetailCandidateColliderCount = 0;
            result.EnvironmentDetailCulledColliderCount = 0;
            result.ActiveFrameLoopBehaviourCount = 0;
            result.ActiveUpdateBehaviourCount = 0;
            result.ActiveLateUpdateBehaviourCount = 0;
            result.ActiveFixedUpdateBehaviourCount = 0;
            result.ConsolidatedFootstepPresenterCount = 0;
            result.FrameLoops ??= new List<MobilePerformanceFrameLoopInventory>();
            result.FrameLoops.Clear();
        }

        private static bool ShouldCountRuntimeLoop(Behaviour behaviour, Scene activeScene)
        {
            if (behaviour.gameObject.scene == activeScene)
            {
                return true;
            }

            return behaviour is DimensionBrawl.Presentation.MovementFootstepAudioScheduler
                || behaviour is MobilePerformanceGovernor;
        }

        private static bool HasUnityMessage(Type type, string methodName)
        {
            while (type != null && type != typeof(MonoBehaviour))
            {
                MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                    null,
                    Type.EmptyTypes,
                    null);
                if (method != null)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        private int[] CopyThermalStatusCounts()
        {
            int[] copy = new int[thermalStatusSampleCounts.Length];
            Array.Copy(thermalStatusSampleCounts, copy, copy.Length);
            return copy;
        }

        private void WriteReport(bool completed)
        {
            report.Completed = completed;
            report.UpdatedUtc = DateTime.UtcNow.ToString("O");
            string path = GetReportPath();
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static string GetReportPath()
        {
            return Path.Combine(Application.persistentDataPath, ReportFileName);
        }

        private static double RecorderNanosecondsToMilliseconds(ProfilerRecorder recorder)
        {
            return RecorderValue(recorder) / 1_000_000d;
        }

        private static double RecorderBytesToKibibytes(ProfilerRecorder recorder)
        {
            return RecorderValue(recorder) / 1024d;
        }

        private static double RecorderBytesToMebibytes(ProfilerRecorder recorder)
        {
            return RecorderValue(recorder) / (1024d * 1024d);
        }

        private static double BytesToMebibytes(ulong bytes)
        {
            return bytes / (1024d * 1024d);
        }

        private static long RecorderValue(ProfilerRecorder recorder)
        {
            return recorder.Valid && recorder.Count > 0 ? recorder.LastValue : 0L;
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
            catch (Exception)
            {
                return -1;
            }
#else
            return -1;
#endif
        }

        private readonly struct BenchmarkScene
        {
            public BenchmarkScene(string label, string path)
            {
                Label = label;
                Path = path;
            }

            public string Label { get; }
            public string Path { get; }
        }
    }

    public static class MobilePerformanceStatistics
    {
        public static double ToFramesPerSecond(double frameMilliseconds)
        {
            if (frameMilliseconds <= 0d
                || double.IsNaN(frameMilliseconds)
                || double.IsInfinity(frameMilliseconds))
            {
                return 0d;
            }

            return 1000d / frameMilliseconds;
        }

        public static MobilePerformanceMetricSummary Summarize(
            double[] samples,
            int count,
            string label,
            string unit,
            bool valid)
        {
            int clampedCount = Mathf.Clamp(count, 0, samples?.Length ?? 0);
            MobilePerformanceMetricSummary summary = new()
            {
                Label = label,
                Unit = unit,
                Valid = valid && clampedCount > 0,
                SampleCount = clampedCount
            };
            if (!summary.Valid)
            {
                return summary;
            }

            double total = 0d;
            double maximum = double.MinValue;
            double[] sorted = new double[clampedCount];
            for (int i = 0; i < clampedCount; i++)
            {
                double value = samples[i];
                sorted[i] = value;
                total += value;
                maximum = Math.Max(maximum, value);
            }

            Array.Sort(sorted);
            summary.Average = total / clampedCount;
            summary.P50 = Percentile(sorted, 0.50d);
            summary.P95 = Percentile(sorted, 0.95d);
            summary.P99 = Percentile(sorted, 0.99d);
            summary.Maximum = maximum;
            return summary;
        }

        private static double Percentile(double[] sorted, double percentile)
        {
            int index = Mathf.Clamp(
                Mathf.CeilToInt((float)((sorted.Length - 1) * percentile)),
                0,
                sorted.Length - 1);
            return sorted[index];
        }
    }

    [Serializable]
    public sealed class MobilePerformanceBenchmarkReport
    {
        public string GeneratedUtc;
        public string UpdatedUtc;
        public bool Completed;
        public string CurrentScene;
        public string UnityVersion;
        public string Platform;
        public string DeviceModel;
        public string DeviceName;
        public string OperatingSystem;
        public string Processor;
        public int ProcessorCount;
        public int ProcessorFrequencyMegahertz;
        public int SystemMemoryMegabytes;
        public string GraphicsDevice;
        public string GraphicsApi;
        public int GraphicsMemoryMegabytes;
        public int GraphicsShaderLevel;
        public int ScreenWidth;
        public int ScreenHeight;
        public float WarmupSeconds;
        public float SampleSecondsPerScene;
        public List<MobilePerformanceSceneResult> Scenes = new();
    }

    [Serializable]
    public sealed class MobilePerformanceFrameLoopInventory
    {
        public string TypeName;
        public int UpdateInstances;
        public int LateUpdateInstances;
        public int FixedUpdateInstances;
    }

    [Serializable]
    public sealed class MobilePerformanceSceneResult
    {
        public string Label;
        public string ScenePath;
        public float SceneLoadSeconds;
        public int SampleCount;
        public float ElapsedSeconds;
        public string PerformanceTier;
        public string InitialPerformanceTier;
        public int TierChangeCount;
        public float RenderScale;
        public int TargetFrameRate;
        public int MinimumTargetFrameRate;
        public int MaximumTargetFrameRate;
        public int FrameBudgetMissCount;
        public double FrameBudgetMissPercent;
        public int FramesOver50Milliseconds;
        public int FramesOver100Milliseconds;
        public double AverageFramesPerSecond;
        public double OnePercentLowFramesPerSecond;
        public int MaximumThermalStatus;
        public int ThermalStatusSampleCount;
        public int[] ThermalStatusSampleCounts;
        public int LowMemoryEventCount;
        public float MinimumBatteryLevel;
        public bool TextureStreamingActive;
        public ulong StreamingTextureCount;
        public ulong NonStreamingTextureCount;
        public ulong StreamingRendererCount;
        public int ActiveRendererCount;
        public int ShadowCasterCount;
        public int ActiveLightCount;
        public int ActiveColliderCount;
        public int EnvironmentDetailCandidateRendererCount;
        public int EnvironmentDetailCulledRendererCount;
        public int EnvironmentDetailCandidateColliderCount;
        public int EnvironmentDetailCulledColliderCount;
        public int ActiveFrameLoopBehaviourCount;
        public int ActiveUpdateBehaviourCount;
        public int ActiveLateUpdateBehaviourCount;
        public int ActiveFixedUpdateBehaviourCount;
        public int ConsolidatedFootstepPresenterCount;
        public List<MobilePerformanceFrameLoopInventory> FrameLoops = new();
        public MobilePerformanceMetricSummary FrameMilliseconds;
        public MobilePerformanceMetricSummary MainThreadMilliseconds;
        public MobilePerformanceMetricSummary RenderThreadMilliseconds;
        public MobilePerformanceMetricSummary GcAllocatedKibibytes;
        public MobilePerformanceMetricSummary TotalUsedMemoryMebibytes;
        public MobilePerformanceMetricSummary GfxUsedMemoryMebibytes;
        public MobilePerformanceMetricSummary DrawCalls;
        public MobilePerformanceMetricSummary SetPassCalls;
        public MobilePerformanceMetricSummary Triangles;
        public MobilePerformanceMetricSummary RenderScaleSummary;
        public MobilePerformanceMetricSummary TextureCurrentMemoryMebibytes;
        public MobilePerformanceMetricSummary TextureDesiredMemoryMebibytes;
        public MobilePerformanceMetricSummary TextureTargetMemoryMebibytes;
        public MobilePerformanceMetricSummary TextureTotalMemoryMebibytes;
        public MobilePerformanceMetricSummary NonStreamingTextureMemoryMebibytes;
        public MobilePerformanceMetricSummary StreamingTexturePendingLoads;
        public MobilePerformanceMetricSummary StreamingTextureLoading;
        public MobilePerformanceMetricSummary StreamingMipmapUploads;
        public MobilePerformanceMetricSummary GlobalTextureMipmapLimit;
        public MobilePerformanceMetricSummary StreamingMemoryBudgetMebibytes;
        public MobilePerformanceMetricSummary LodBias;
        public MobilePerformanceMetricSummary ShadowDistance;
    }

    [Serializable]
    public sealed class MobilePerformanceMetricSummary
    {
        public string Label;
        public string Unit;
        public bool Valid;
        public int SampleCount;
        public double Average;
        public double P50;
        public double P95;
        public double P99;
        public double Maximum;
    }
}
