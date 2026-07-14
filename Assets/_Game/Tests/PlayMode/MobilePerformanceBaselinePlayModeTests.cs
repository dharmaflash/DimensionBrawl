using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class MobilePerformanceBaselinePlayModeTests
    {
        private const string MarkdownReportPath = "C:/tmp/DimensionBrawl-MobilePerformancePlayModeBaseline.md";
        private const string JsonReportPath = "C:/tmp/DimensionBrawl-MobilePerformancePlayModeBaseline.json";
        private const string MobilePipelinePath = "Assets/Settings/Mobile_RPAsset.asset";
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 720;
        private const int TargetFrameRate = 60;
        private const int WarmupFrameLimit = 600;
        private const int SampleFrameCount = 360;
        private const float WarmupSeconds = 2f;
        private const float MinimumSampleSeconds = 5f;

        private static readonly ProfileTarget[] Targets =
        {
            new(
                "Olympus Station Combat",
                "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
                "Direct combat scene",
                4f,
                12f),
            new(
                "Olympus Corridor Startup",
                "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity",
                "Natural intro/startup path",
                4f,
                8f)
        };

        private static readonly MetricSpec[] RecorderMetrics =
        {
            new("CPU Main Thread Work", ProfilerCategory.Render, "CPU Main Thread Frame Time", MetricValueKind.Nanoseconds, false, true),
            new("CPU Render Thread Work", ProfilerCategory.Render, "CPU Render Thread Frame Time", MetricValueKind.Nanoseconds, false, true),
            new("GPU Frame", ProfilerCategory.Render, "GPU Frame Time", MetricValueKind.Nanoseconds, false, true),
            new("Editor Process GC Allocated", ProfilerCategory.Memory, "GC Allocated In Frame", MetricValueKind.Kibibytes, true, false),
            new("Total Used Memory", ProfilerCategory.Memory, "Total Used Memory", MetricValueKind.Mebibytes, false, true),
            new("Gfx Used Memory", ProfilerCategory.Memory, "Gfx Used Memory", MetricValueKind.Mebibytes, false, true)
        };

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator CanonicalRuntimeStagesWriteMeasuredPlayModeBaseline()
        {
            BaselineReport report = new()
            {
                GeneratedUtc = DateTime.UtcNow.ToString("O"),
                UnityVersion = Application.unityVersion,
                Platform = Application.platform.ToString(),
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                GraphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                Processor = SystemInfo.processorType,
                OperatingSystem = SystemInfo.operatingSystem
            };
            int previousQualityLevel = QualitySettings.GetQualityLevel();
            int previousTargetFrameRate = Application.targetFrameRate;
            int previousVSyncCount = QualitySettings.vSyncCount;
            int mobileQualityLevel = Array.IndexOf(QualitySettings.names, "Mobile");
            Assert.That(mobileQualityLevel, Is.GreaterThanOrEqualTo(0));

            QualitySettings.SetQualityLevel(mobileQualityLevel, applyExpensiveChanges: true);
            Screen.SetResolution(CaptureWidth, CaptureHeight, FullScreenMode.Windowed);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
            yield return null;
            yield return null;
            Assert.That(
                AssetDatabase.GetAssetPath(QualitySettings.renderPipeline),
                Is.EqualTo(MobilePipelinePath),
                "The mobile performance baseline must exercise the shipping mobile pipeline asset.");
            report.Width = Screen.width;
            report.Height = Screen.height;

            try
            {
                for (int i = 0; i < Targets.Length; i++)
                {
                    ProfileTarget target = Targets[i];
                    EditorSceneManager.LoadSceneInPlayMode(
                        target.ScenePath,
                        new LoadSceneParameters(LoadSceneMode.Single));
                    yield return null;
                    yield return null;
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = TargetFrameRate;

                    Scene activeScene = SceneManager.GetActiveScene();
                    Assert.That(activeScene.path, Is.EqualTo(target.ScenePath));

                    float warmupStartedAt = Time.realtimeSinceStartup;
                    float warmupDeadline = warmupStartedAt + WarmupSeconds;
                    int warmupFrames = 0;
                    while (Time.realtimeSinceStartup < warmupDeadline && warmupFrames < WarmupFrameLimit)
                    {
                        warmupFrames++;
                        yield return null;
                    }

                    float warmupElapsedSeconds = Time.realtimeSinceStartup - warmupStartedAt;
                    using ProfileSession session = new(target, warmupFrames, warmupElapsedSeconds);
                    for (int frame = 0; frame < SampleFrameCount; frame++)
                    {
                        yield return null;
                        session.Sample();
                    }

                    SceneProfileResult sceneResult = session.BuildResult();
                    string capturePath = $"C:/tmp/DimensionBrawl-MobilePerformance-{i:00}-Baseline.png";
                    if (!TryCaptureActiveCamera(capturePath, out string captureSkipReason))
                    {
                        capturePath = $"Skipped: {captureSkipReason}";
                    }

                    sceneResult.CapturePath = capturePath;
                    report.Scenes.Add(sceneResult);
                }
            }
            finally
            {
                Application.targetFrameRate = previousTargetFrameRate;
                QualitySettings.vSyncCount = previousVSyncCount;
                QualitySettings.SetQualityLevel(previousQualityLevel, applyExpensiveChanges: true);
                WriteReports(report);
            }

            Assert.That(report.Scenes.Count, Is.EqualTo(Targets.Length));
            for (int i = 0; i < report.Scenes.Count; i++)
            {
                Assert.That(report.Scenes[i].SampleFrames, Is.EqualTo(SampleFrameCount));
                Assert.That(
                    report.Scenes[i].ElapsedSeconds,
                    Is.GreaterThanOrEqualTo(MinimumSampleSeconds),
                    $"{report.Scenes[i].Label} did not honor the {TargetFrameRate} fps measurement pace.");
                Assert.That(report.Scenes[i].Metrics.Count, Is.GreaterThan(0));
                AssertMeasurementIntegrity(report.Scenes[i]);
            }
        }

        private static void AssertMeasurementIntegrity(SceneProfileResult scene)
        {
            MetricSummary frameDelta = FindMetric(scene, "Player Frame Delta");
            Assert.That(frameDelta, Is.Not.Null);
            Assert.That(frameDelta.Valid, Is.True);
            Assert.That(frameDelta.P50, Is.InRange(14d, 20d));
            Assert.That(scene.ActiveRendererCount, Is.GreaterThan(0));
            Assert.That(scene.FrustumRendererCount, Is.GreaterThan(0));
            Assert.That(scene.FrustumTriangleCount, Is.GreaterThan(0));

            MetricSummary mainThread = FindMetric(scene, "CPU Main Thread Work");
            Assert.That(mainThread, Is.Not.Null);
            if (mainThread.Valid)
            {
                Assert.That(mainThread.P95, Is.GreaterThan(0d));
            }
        }

        private static MetricSummary FindMetric(SceneProfileResult scene, string label)
        {
            for (int i = 0; i < scene.Metrics.Count; i++)
            {
                MetricSummary metric = scene.Metrics[i];
                if (string.Equals(metric.Label, label, StringComparison.Ordinal))
                {
                    return metric;
                }
            }

            return null;
        }

        private static void WriteReports(BaselineReport report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MarkdownReportPath) ?? "C:/tmp");
            File.WriteAllText(JsonReportPath, JsonUtility.ToJson(report, true), Encoding.UTF8);
            File.WriteAllText(MarkdownReportPath, BuildMarkdown(report), Encoding.UTF8);
        }

        private static string BuildMarkdown(BaselineReport report)
        {
            StringBuilder builder = new();
            builder.AppendLine("# DimensionBrawl Mobile Performance PlayMode Baseline");
            builder.AppendLine();
            builder.AppendLine($"- Generated UTC: {report.GeneratedUtc}");
            builder.AppendLine($"- Unity: {report.UnityVersion}");
            builder.AppendLine($"- Platform: {report.Platform}");
            builder.AppendLine($"- Graphics: {report.GraphicsDevice} ({report.GraphicsApi})");
            builder.AppendLine($"- Processor: {report.Processor}");
            builder.AppendLine($"- OS: {report.OperatingSystem}");
            builder.AppendLine($"- Requested capture: {CaptureWidth}x{CaptureHeight}");
            builder.AppendLine($"- Actual capture: {report.Width}x{report.Height}");
            builder.AppendLine($"- Measurement pace: {TargetFrameRate} fps target, {SampleFrameCount:N0} frames after {WarmupSeconds:0.0}s warmup");
            builder.AppendLine($"- Validity gate: each sample window must span at least {MinimumSampleSeconds:0.0}s");
            builder.AppendLine("- Scope: Editor PlayMode cadence, memory trend, and runtime scene inventory baseline.");
            builder.AppendLine("- CPU/GPU counters may be unavailable in batch-mode Editor; Android development-player captures are the authoritative timing and GC source.");
            builder.AppendLine();

            for (int i = 0; i < report.Scenes.Count; i++)
            {
                SceneProfileResult scene = report.Scenes[i];
                builder.AppendLine($"## {scene.Label}");
                builder.AppendLine();
                builder.AppendLine($"- Scene: `{scene.ScenePath}`");
                builder.AppendLine($"- Capture role: {scene.CaptureRole}");
                builder.AppendLine($"- Warmup frames: {scene.WarmupFrames:N0}");
                builder.AppendLine($"- Warmup elapsed: {scene.WarmupElapsedSeconds:0.000}s");
                builder.AppendLine($"- Sample frames: {scene.SampleFrames:N0}");
                builder.AppendLine($"- Capture elapsed: {scene.ElapsedSeconds:0.000}s");
                builder.AppendLine($"- Effective sample rate: {scene.EffectiveFramesPerSecond:0.0} fps");
                builder.AppendLine($"- World capture: `{scene.CapturePath}`");
                builder.AppendLine($"- Runtime render inventory: {scene.ActiveRendererCount:N0} active renderers, {scene.ShadowCasterCount:N0} shadow casters, {scene.ActiveLightCount:N0} active lights");
                builder.AppendLine($"- Camera frustum estimate: {scene.FrustumRendererCount:N0} renderers, {scene.FrustumTriangleCount:N0} triangles");
                builder.AppendLine(
                    $"- Frustum distance bands: <=30m {scene.NearRendererCount:N0}/{scene.NearTriangleCount:N0}, " +
                    $"30-60m {scene.MidRendererCount:N0}/{scene.MidTriangleCount:N0}, " +
                    $"60-120m {scene.FarRendererCount:N0}/{scene.FarTriangleCount:N0}, " +
                    $">120m {scene.VeryFarRendererCount:N0}/{scene.VeryFarTriangleCount:N0} (renderers/triangles)");
                builder.AppendLine($"- Mobile-player targets: Main Thread P95 <= {scene.MainThreadP95BudgetMilliseconds:0.0} ms, GC average <= {scene.GcAllocatedAverageBudgetKibibytes:0.0} KiB/frame (not enforced against the Editor process)");
                builder.AppendLine();
                builder.AppendLine("### Top Frustum Meshes By Distant Triangle Inventory");
                builder.AppendLine();
                builder.AppendLine("| Mesh | Visible instances | Visible triangles | 60-120m | 120m+ | Max bounds |");
                builder.AppendLine("|---|---:|---:|---:|---:|---:|");
                int meshUsageCount = Math.Min(15, scene.FrustumMeshUsages.Count);
                for (int meshIndex = 0; meshIndex < meshUsageCount; meshIndex++)
                {
                    FrustumMeshUsage usage = scene.FrustumMeshUsages[meshIndex];
                    builder.AppendLine(
                        $"| {usage.MeshName} | {usage.VisibleRendererCount:N0} | {usage.VisibleTriangleCount:N0} | " +
                        $"{usage.FarRendererCount:N0}/{usage.FarTriangleCount:N0} | " +
                        $"{usage.VeryFarRendererCount:N0}/{usage.VeryFarTriangleCount:N0} | " +
                        $"{usage.MaxWorldBoundsSize:0.0}m |");
                }

                builder.AppendLine();
                builder.AppendLine("| Metric | Samples | Average | P50 | P95 | P99 | Max | Total |");
                builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|");

                for (int metricIndex = 0; metricIndex < scene.Metrics.Count; metricIndex++)
                {
                    MetricSummary metric = scene.Metrics[metricIndex];
                    if (!metric.Valid)
                    {
                        builder.AppendLine($"| {metric.Label} | unavailable | - | - | - | - | - | - |");
                        continue;
                    }

                    string total = metric.IncludeTotal
                        ? FormatTotal(metric)
                        : "-";
                    builder.AppendLine(
                        $"| {metric.Label} | {metric.SampleCount:N0} | {FormatMetric(metric.Average, metric.Unit)} | " +
                        $"{FormatMetric(metric.P50, metric.Unit)} | {FormatMetric(metric.P95, metric.Unit)} | " +
                        $"{FormatMetric(metric.P99, metric.Unit)} | {FormatMetric(metric.Max, metric.Unit)} | {total} |");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static bool TryCaptureActiveCamera(string path, out string skipReason)
        {
            skipReason = string.Empty;
            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            Camera camera = null;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].enabled && cameras[i].gameObject.activeInHierarchy)
                {
                    camera = cameras[i];
                    break;
                }
            }

            Assert.That(camera, Is.Not.Null, "Performance baseline needs an active camera for visual comparison.");
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                skipReason = "null graphics device";
                return false;
            }

            if (Application.isBatchMode
                && SystemInfo.graphicsDeviceType
                    == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11
                && HasLoadedVisualEffects())
            {
                skipReason = "D3D11 batch readback is disabled while VFX Graph content is loaded";
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "C:/tmp");
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(
                CaptureWidth,
                CaptureHeight,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D image = new(CaptureWidth, CaptureHeight, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                image.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
                image.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.Destroy(image);
                RenderTexture.ReleaseTemporary(target);
            }

            return true;
        }

        private static bool HasLoadedVisualEffects()
        {
            Behaviour[] behaviours = UnityEngine.Object.FindObjectsByType<Behaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().FullName == "UnityEngine.VFX.VisualEffect")
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatMetric(double value, string unit)
        {
            if (unit == "count")
            {
                return $"{value:N0}";
            }

            return $"{value:N3} {unit}";
        }

        private static string FormatTotal(MetricSummary metric)
        {
            if (metric.Unit == "KiB/frame")
            {
                return $"{metric.Total / 1024d:N3} MiB";
            }

            return FormatMetric(metric.Total, metric.Unit);
        }

        private sealed class ProfileSession : IDisposable
        {
            private readonly ProfileTarget target;
            private readonly int warmupFrames;
            private readonly List<RecorderSampler> samplers = new();
            private readonly List<double> frameDeltaMilliseconds = new(SampleFrameCount);
            private readonly float startedAt;
            private readonly float warmupElapsedSeconds;
            private bool disposed;

            public ProfileSession(ProfileTarget target, int warmupFrames, float warmupElapsedSeconds)
            {
                this.target = target;
                this.warmupFrames = warmupFrames;
                this.warmupElapsedSeconds = warmupElapsedSeconds;
                startedAt = Time.realtimeSinceStartup;

                for (int i = 0; i < RecorderMetrics.Length; i++)
                {
                    samplers.Add(new RecorderSampler(RecorderMetrics[i]));
                }
            }

            public void Sample()
            {
                frameDeltaMilliseconds.Add(Time.unscaledDeltaTime * 1000d);
                for (int i = 0; i < samplers.Count; i++)
                {
                    samplers[i].Sample();
                }
            }

            public SceneProfileResult BuildResult()
            {
                SceneProfileResult result = new()
                {
                    Label = target.Label,
                    ScenePath = target.ScenePath,
                    CaptureRole = target.CaptureRole,
                    MainThreadP95BudgetMilliseconds = target.MainThreadP95BudgetMilliseconds,
                    GcAllocatedAverageBudgetKibibytes = target.GcAllocatedAverageBudgetKibibytes,
                    WarmupFrames = warmupFrames,
                    WarmupElapsedSeconds = warmupElapsedSeconds,
                    SampleFrames = frameDeltaMilliseconds.Count,
                    ElapsedSeconds = Time.realtimeSinceStartup - startedAt
                };
                result.EffectiveFramesPerSecond = result.ElapsedSeconds > 0d
                    ? result.SampleFrames / result.ElapsedSeconds
                    : 0d;
                CaptureRuntimeInventory(result);

                result.Metrics.Add(BuildSummary(
                    "Player Frame Delta",
                    "ms",
                    false,
                    frameDeltaMilliseconds));

                for (int i = 0; i < samplers.Count; i++)
                {
                    result.Metrics.Add(samplers[i].BuildSummary());
                }

                return result;
            }

            private static void CaptureRuntimeInventory(SceneProfileResult result)
            {
                Camera camera = FindActiveCamera();
                Plane[] frustumPlanes = camera != null
                    ? GeometryUtility.CalculateFrustumPlanes(camera)
                    : null;
                Dictionary<int, FrustumMeshUsage> meshUsages = new();
                Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Include,
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

                    if (frustumPlanes == null || !GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds))
                    {
                        continue;
                    }

                    Mesh mesh = GetRendererMesh(renderer);
                    long triangles = GetRendererTriangleCount(renderer, mesh);
                    float distance = Vector3.Distance(
                        camera.transform.position,
                        renderer.bounds.ClosestPoint(camera.transform.position));
                    result.FrustumRendererCount++;
                    result.FrustumTriangleCount += triangles;
                    if (distance <= 30f)
                    {
                        result.NearRendererCount++;
                        result.NearTriangleCount += triangles;
                    }
                    else if (distance <= 60f)
                    {
                        result.MidRendererCount++;
                        result.MidTriangleCount += triangles;
                    }
                    else if (distance <= 120f)
                    {
                        result.FarRendererCount++;
                        result.FarTriangleCount += triangles;
                    }
                    else
                    {
                        result.VeryFarRendererCount++;
                        result.VeryFarTriangleCount += triangles;
                    }

                    if (mesh == null)
                    {
                        continue;
                    }

                    int meshId = mesh.GetInstanceID();
                    if (!meshUsages.TryGetValue(meshId, out FrustumMeshUsage usage))
                    {
                        usage = new FrustumMeshUsage
                        {
                            MeshName = mesh.name
                        };
                        meshUsages.Add(meshId, usage);
                    }

                    usage.VisibleRendererCount++;
                    usage.VisibleTriangleCount += triangles;
                    usage.MaxWorldBoundsSize = Mathf.Max(
                        usage.MaxWorldBoundsSize,
                        renderer.bounds.size.magnitude);
                    if (distance > 120f)
                    {
                        usage.VeryFarRendererCount++;
                        usage.VeryFarTriangleCount += triangles;
                    }
                    else if (distance > 60f)
                    {
                        usage.FarRendererCount++;
                        usage.FarTriangleCount += triangles;
                    }
                }

                foreach (FrustumMeshUsage usage in meshUsages.Values)
                {
                    result.FrustumMeshUsages.Add(usage);
                }

                result.FrustumMeshUsages.Sort(CompareFrustumMeshUsage);

                Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i].enabled && lights[i].gameObject.activeInHierarchy)
                    {
                        result.ActiveLightCount++;
                    }
                }
            }

            private static Camera FindActiveCamera()
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null && mainCamera.enabled && mainCamera.gameObject.activeInHierarchy)
                {
                    return mainCamera;
                }

                Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
                for (int i = 0; i < cameras.Length; i++)
                {
                    Camera camera = cameras[i];
                    if (camera.enabled && camera.gameObject.activeInHierarchy)
                    {
                        return camera;
                    }
                }

                return null;
            }

            private static Mesh GetRendererMesh(Renderer renderer)
            {
                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    return skinnedMeshRenderer.sharedMesh;
                }

                if (renderer is MeshRenderer)
                {
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    return meshFilter != null ? meshFilter.sharedMesh : null;
                }

                return null;
            }

            private static long GetRendererTriangleCount(Renderer renderer, Mesh mesh)
            {
                if (mesh == null)
                {
                    return 0L;
                }

                int firstSubMesh = renderer is MeshRenderer meshRenderer
                    ? meshRenderer.subMeshStartIndex
                    : 0;
                int renderedSubMeshCount = renderer.sharedMaterials.Length;
                if (renderedSubMeshCount <= 0 || firstSubMesh + renderedSubMeshCount > mesh.subMeshCount)
                {
                    firstSubMesh = 0;
                    renderedSubMeshCount = mesh.subMeshCount;
                }

                long triangleCount = 0L;
                int lastSubMesh = firstSubMesh + renderedSubMeshCount;
                for (int subMeshIndex = firstSubMesh; subMeshIndex < lastSubMesh; subMeshIndex++)
                {
                    if (mesh.GetTopology(subMeshIndex) == MeshTopology.Triangles)
                    {
                        triangleCount += (long)mesh.GetIndexCount(subMeshIndex) / 3L;
                    }
                }

                return triangleCount;
            }

            private static int CompareFrustumMeshUsage(
                FrustumMeshUsage left,
                FrustumMeshUsage right)
            {
                int comparison = right.VeryFarTriangleCount.CompareTo(left.VeryFarTriangleCount);
                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = right.FarTriangleCount.CompareTo(left.FarTriangleCount);
                return comparison != 0
                    ? comparison
                    : right.VisibleTriangleCount.CompareTo(left.VisibleTriangleCount);
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                for (int i = 0; i < samplers.Count; i++)
                {
                    samplers[i].Dispose();
                }
            }
        }

        private sealed class RecorderSampler : IDisposable
        {
            private readonly MetricSpec spec;
            private readonly List<double> samples = new(SampleFrameCount);
            private ProfilerRecorder recorder;
            private bool disposed;

            public RecorderSampler(MetricSpec spec)
            {
                this.spec = spec;
                try
                {
                    recorder = ProfilerRecorder.StartNew(spec.Category, spec.StatName, 1);
                }
                catch (Exception)
                {
                    recorder = default;
                }
            }

            public void Sample()
            {
                if (!recorder.Valid)
                {
                    return;
                }

                samples.Add(ConvertValue(recorder.LastValue, spec.ValueKind));
            }

            public MetricSummary BuildSummary()
            {
                MetricSummary summary = MobilePerformanceBaselinePlayModeTests.BuildSummary(
                    spec.Label,
                    GetUnit(spec.ValueKind),
                    spec.IncludeTotal,
                    samples);
                summary.Valid = recorder.Valid && samples.Count > 0;
                if (summary.Valid && spec.RequiresPositive && summary.Max <= 0d)
                {
                    summary.Valid = false;
                }

                return summary;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                recorder.Dispose();
            }
        }

        private static MetricSummary BuildSummary(
            string label,
            string unit,
            bool includeTotal,
            List<double> samples)
        {
            MetricSummary summary = new()
            {
                Label = label,
                Unit = unit,
                IncludeTotal = includeTotal,
                Valid = samples.Count > 0,
                SampleCount = samples.Count
            };

            if (samples.Count == 0)
            {
                return summary;
            }

            double total = 0d;
            double max = double.MinValue;
            for (int i = 0; i < samples.Count; i++)
            {
                double sample = samples[i];
                total += sample;
                max = Math.Max(max, sample);
            }

            double[] sorted = samples.ToArray();
            Array.Sort(sorted);
            summary.Average = total / samples.Count;
            summary.P50 = Percentile(sorted, 0.50d);
            summary.P95 = Percentile(sorted, 0.95d);
            summary.P99 = Percentile(sorted, 0.99d);
            summary.Max = max;
            summary.Total = total;
            return summary;
        }

        private static double Percentile(double[] sorted, double percentile)
        {
            if (sorted.Length == 0)
            {
                return 0d;
            }

            int index = Mathf.Clamp(
                Mathf.CeilToInt((float)((sorted.Length - 1) * percentile)),
                0,
                sorted.Length - 1);
            return sorted[index];
        }

        private static double ConvertValue(long value, MetricValueKind valueKind)
        {
            switch (valueKind)
            {
                case MetricValueKind.Nanoseconds:
                    return value / 1_000_000d;
                case MetricValueKind.Kibibytes:
                    return value / 1024d;
                case MetricValueKind.Mebibytes:
                    return value / (1024d * 1024d);
                default:
                    return value;
            }
        }

        private static string GetUnit(MetricValueKind valueKind)
        {
            switch (valueKind)
            {
                case MetricValueKind.Nanoseconds:
                    return "ms";
                case MetricValueKind.Kibibytes:
                    return "KiB/frame";
                case MetricValueKind.Mebibytes:
                    return "MiB";
                default:
                    return "count";
            }
        }

        private readonly struct ProfileTarget
        {
            public ProfileTarget(
                string label,
                string scenePath,
                string captureRole,
                float mainThreadP95BudgetMilliseconds,
                float gcAllocatedAverageBudgetKibibytes)
            {
                Label = label;
                ScenePath = scenePath;
                CaptureRole = captureRole;
                MainThreadP95BudgetMilliseconds = mainThreadP95BudgetMilliseconds;
                GcAllocatedAverageBudgetKibibytes = gcAllocatedAverageBudgetKibibytes;
            }

            public string Label { get; }
            public string ScenePath { get; }
            public string CaptureRole { get; }
            public float MainThreadP95BudgetMilliseconds { get; }
            public float GcAllocatedAverageBudgetKibibytes { get; }
        }

        private readonly struct MetricSpec
        {
            public MetricSpec(
                string label,
                ProfilerCategory category,
                string statName,
                MetricValueKind valueKind,
                bool includeTotal,
                bool requiresPositive)
            {
                Label = label;
                Category = category;
                StatName = statName;
                ValueKind = valueKind;
                IncludeTotal = includeTotal;
                RequiresPositive = requiresPositive;
            }

            public string Label { get; }
            public ProfilerCategory Category { get; }
            public string StatName { get; }
            public MetricValueKind ValueKind { get; }
            public bool IncludeTotal { get; }
            public bool RequiresPositive { get; }
        }

        private enum MetricValueKind
        {
            Count,
            Nanoseconds,
            Kibibytes,
            Mebibytes
        }

        [Serializable]
        private sealed class BaselineReport
        {
            public string GeneratedUtc;
            public string UnityVersion;
            public string Platform;
            public string GraphicsDevice;
            public string GraphicsApi;
            public string Processor;
            public string OperatingSystem;
            public int Width;
            public int Height;
            public List<SceneProfileResult> Scenes = new();
        }

        [Serializable]
        private sealed class SceneProfileResult
        {
            public string Label;
            public string ScenePath;
            public string CaptureRole;
            public float MainThreadP95BudgetMilliseconds;
            public float GcAllocatedAverageBudgetKibibytes;
            public int WarmupFrames;
            public double WarmupElapsedSeconds;
            public int SampleFrames;
            public int ActiveRendererCount;
            public int ShadowCasterCount;
            public int ActiveLightCount;
            public int FrustumRendererCount;
            public long FrustumTriangleCount;
            public int NearRendererCount;
            public long NearTriangleCount;
            public int MidRendererCount;
            public long MidTriangleCount;
            public int FarRendererCount;
            public long FarTriangleCount;
            public int VeryFarRendererCount;
            public long VeryFarTriangleCount;
            public double ElapsedSeconds;
            public double EffectiveFramesPerSecond;
            public string CapturePath;
            public List<FrustumMeshUsage> FrustumMeshUsages = new();
            public List<MetricSummary> Metrics = new();
        }

        [Serializable]
        private sealed class FrustumMeshUsage
        {
            public string MeshName;
            public int VisibleRendererCount;
            public long VisibleTriangleCount;
            public int FarRendererCount;
            public long FarTriangleCount;
            public int VeryFarRendererCount;
            public long VeryFarTriangleCount;
            public float MaxWorldBoundsSize;
        }

        [Serializable]
        private sealed class MetricSummary
        {
            public string Label;
            public string Unit;
            public bool Valid;
            public bool IncludeTotal;
            public int SampleCount;
            public double Average;
            public double P50;
            public double P95;
            public double P99;
            public double Max;
            public double Total;
        }
    }
}
