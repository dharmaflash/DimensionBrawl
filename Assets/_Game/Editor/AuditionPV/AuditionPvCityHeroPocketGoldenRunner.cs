using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DimensionBrawl.Editor.CityHeroPocket;
using DimensionBrawl.Presentation;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Headful Editor orchestration for the City Hero Pocket G01-G03 golden
    /// sources. Product gameplay and presentation remain owned by the City
    /// capture director; this type owns only Recorder, evidence, validation,
    /// provenance, and Editor lifecycle.
    /// </summary>
    [InitializeOnLoad]
    public static class AuditionPvCityHeroPocketGoldenRunner
    {
        internal const string RunnerScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvCityHeroPocketGoldenRunner.cs";
        internal const string RunnerTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHeroPocketGoldenRunnerTests.cs";
        internal const string MenuPath =
            "DimensionBrawl/Audition PV/Capture City Hero Pocket G01-G03 Golden Sources";
        internal const string StateFileName = "city_g01_g03_runner_state.json";
        internal const string RuntimeProofFileName =
            "city_g01_g03_runtime_proof.json";
        internal const string FrameHashFileName = "frame_hashes.sha256";
        internal const string FailureFileName =
            "city_g01_g03_capture_failure.json";
        internal const string EvidenceFolderName = "evidence";
        internal const int RawWarmupFrame = 0;
        internal const int RawFirstLogicalFrame = 1;

        private const string SessionActiveKey =
            "DimensionBrawl.AuditionPV.CityG01G03GoldenRunner.Active";
        private const string SessionStatePathKey =
            "DimensionBrawl.AuditionPV.CityG01G03GoldenRunner.StatePath";
        private const string SessionOwnerKey =
            "DimensionBrawl.AuditionPV.CityG01G03GoldenRunner.Owner";
        private const string SessionBatchKey =
            "DimensionBrawl.AuditionPV.CityG01G03GoldenRunner.Batch";
        private const string SessionOwnerValue =
            "dimension-brawl.city-hero-pocket-g01-g03.v1";
        private const string RunnerSchema =
            "dimension-brawl.audition-pv.city-g01-g03-runner-state.v1";
        private const string RuntimeProofSchema =
            "dimension-brawl.audition-pv.city-g01-g03-runtime-proof.v2";
        private const string FailureSchema =
            "dimension-brawl.audition-pv.capture-failure.v1";
        private const double MaximumSequenceBlackRatio = 0.90d;
        private const double MaximumSequenceMagentaRatio = 0.005d;
        private const double MaximumFrameMagentaRatio = 0.02d;
        private const int MinimumHealthyFramePercent = 90;
        private const int MinimumHudAccentSamples = 12;
        internal const int G02DodgeVisualBeforeFrame = 239;
        internal const int G02DodgeVisualAfterFrame = 242;

        internal static readonly AuditionPvCityShot[] ShotOrder =
        {
            AuditionPvCityShot.G01,
            AuditionPvCityShot.G02,
            AuditionPvCityShot.G03
        };

        internal static readonly SixtySecondEvidenceRange[]
            ApprovedSixtySecondEvidenceRanges =
        {
            new(AuditionPvCityShot.G01, 0, 539, 180, 359),
            new(AuditionPvCityShot.G02, 60, 779, 240, 599),
            new(AuditionPvCityShot.G03, 0, 419, 180, 239),
            new(AuditionPvCityShot.G03, 60, 659, 240, 479)
        };

        private static bool resumeScheduled;
        private static bool finalizing;
        private static AuditionPvCityHeroPocketGoldenRunnerBehaviour activeBehaviour;

        static AuditionPvCityHeroPocketGoldenRunner()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            ScheduleResume();
        }

        [MenuItem(MenuPath)]
        public static void CaptureMenu()
        {
            try
            {
                BeginCapture(batchMode: false);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "City G01-G03 golden capture did not start",
                    exception.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Invoke from a graphics-capable GUI Editor with -executeMethod and
        /// -noaudio. The asynchronous runner exits the Editor after finalization.
        /// </summary>
        public static void RunBatchCapture()
        {
            try
            {
                ValidateBatchCommandLine(Environment.GetCommandLineArgs());
                BeginCapture(
                    batchMode: true,
                    ResolveApprovedEvidenceRequest(Environment.GetCommandLineArgs()));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        internal static int ExpectedRawFrameCount(AuditionPvCityShot shot)
        {
            return AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot) + 1;
        }

        internal static int RawLastFrame(AuditionPvCityShot shot)
        {
            return ExpectedRawFrameCount(shot) - 1;
        }

        internal static string RawFrameFileName(
            AuditionPvCityShot shot,
            int rawFrameIndex)
        {
            if (rawFrameIndex < RawWarmupFrame
                || rawFrameIndex > RawLastFrame(shot))
            {
                throw new ArgumentOutOfRangeException(nameof(rawFrameIndex));
            }

            return $"frame_{rawFrameIndex:0000}.png";
        }

        internal static string WarmupEvidenceFileName(AuditionPvCityShot shot)
        {
            return $"recorder_warmup_{ShotId(shot)}_raw_frame_0000.png";
        }

        internal static void ValidateBatchCommandLine(IEnumerable<string> arguments)
        {
            string[] args = (arguments ?? Array.Empty<string>()).ToArray();
            bool Has(string expected) => args.Any(value =>
                string.Equals(value, expected, StringComparison.OrdinalIgnoreCase));

            if (!Has("-noaudio"))
            {
                throw new InvalidOperationException(
                    "City G01-G03 RunBatchCapture requires -noaudio.");
            }

            if (!Has("-executeMethod"))
            {
                throw new InvalidOperationException(
                    "City G01-G03 RunBatchCapture requires -executeMethod.");
            }

            if (Has("-quit"))
            {
                throw new InvalidOperationException(
                    "Do not pass -quit; the asynchronous runner exits after finalization.");
            }

            if (Has("-nographics"))
            {
                throw new InvalidOperationException(
                    "QHD Game View PNG capture requires graphics; remove -nographics.");
            }

            if (Has("-batchmode"))
            {
                throw new InvalidOperationException(
                    "City GameViewInput capture requires a headful Editor; remove -batchmode.");
            }
        }

        internal static bool ResolveApprovedEvidenceRequest(
            IEnumerable<string> arguments) => (arguments ?? Array.Empty<string>()).Any(value =>
            string.Equals(value, "-pv60ApprovedEvidence", StringComparison.OrdinalIgnoreCase));

        internal static void EnsureNoDirtyOpenScenes()
        {
            var dirtyScenes = new List<string>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.IsValid() && scene.isLoaded && scene.isDirty)
                {
                    dirtyScenes.Add(string.IsNullOrWhiteSpace(scene.path)
                        ? $"<untitled:{scene.name}>"
                        : scene.path);
                }
            }

            if (dirtyScenes.Count > 0)
            {
                throw new InvalidOperationException(
                    "City G01-G03 refuses to replace dirty open scenes: "
                    + string.Join(", ", dirtyScenes));
            }
        }

        internal static string[] CollectCaptureDependencyPaths()
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                RunnerScriptPath,
                RunnerTestPath
            };

            foreach (string direct in
                     AuditionPvCityHeroPocketCapture.ExplicitProductDependencyPaths())
            {
                if (AssetDatabase.LoadMainAssetAtPath(direct) == null
                    && !File.Exists(ProjectAbsolutePath(direct)))
                {
                    throw new FileNotFoundException(
                        "City golden capture dependency is missing.",
                        direct);
                }

                dependencies.Add(direct.Replace('\\', '/'));
                foreach (string nested in AssetDatabase.GetDependencies(direct, true))
                {
                    dependencies.Add(nested.Replace('\\', '/'));
                }
            }

            return AuditionPvEnvironmentProbe.CollectCaptureDependencyPaths(dependencies);
        }

        internal static void ValidateStableGitSnapshot(
            AuditionPvGitSnapshot start,
            AuditionPvGitSnapshot end)
        {
            if (start == null
                || end == null
                || !start.probeSucceeded
                || !end.probeSucceeded
                || !string.Equals(start.commitSha, end.commitSha, StringComparison.Ordinal)
                || !string.Equals(start.branch, end.branch, StringComparison.Ordinal)
                || start.isDirty != end.isDirty
                || !string.Equals(
                    start.dirtyStateHashSha256,
                    end.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Git source state changed during City G01-G03 capture.");
            }
        }

        internal static void ValidateStableDependencies(
            AuditionPvDependencyHash[] start,
            AuditionPvDependencyHash[] end)
        {
            AuditionPvDependencyHash[] initial =
                start ?? Array.Empty<AuditionPvDependencyHash>();
            AuditionPvDependencyHash[] current =
                end ?? Array.Empty<AuditionPvDependencyHash>();
            var currentByPath = current.ToDictionary(
                dependency => dependency.path,
                StringComparer.OrdinalIgnoreCase);

            if (initial.Length != current.Length)
            {
                throw new InvalidOperationException(
                    "City capture dependency set changed while recording.");
            }

            foreach (AuditionPvDependencyHash dependency in initial)
            {
                if (dependency == null
                    || !currentByPath.TryGetValue(
                        dependency.path,
                        out AuditionPvDependencyHash currentDependency)
                    || dependency.exists != currentDependency.exists
                    || dependency.byteLength != currentDependency.byteLength
                    || !string.Equals(
                        dependency.sha256,
                        currentDependency.sha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "City capture dependency changed: "
                        + (dependency?.path ?? "<null>"));
                }
            }
        }

        internal static void ValidatePngFile(
            string path,
            int expectedWidth,
            int expectedHeight)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Expected City PNG is missing.", path);
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length
                    || stream.Read(header, 0, header.Length) != header.Length)
                {
                    throw new InvalidDataException("PNG is truncated: " + path);
                }
            }

            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            for (int index = 0; index < signature.Length; index++)
            {
                if (header[index] != signature[index])
                {
                    throw new InvalidDataException("PNG signature mismatch: " + path);
                }
            }

            if (header[12] != (byte)'I'
                || header[13] != (byte)'H'
                || header[14] != (byte)'D'
                || header[15] != (byte)'R')
            {
                throw new InvalidDataException("PNG does not begin with IHDR: " + path);
            }

            int width = ReadBigEndianInt32(header, 16);
            int height = ReadBigEndianInt32(header, 20);
            if (width != expectedWidth || height != expectedHeight)
            {
                throw new InvalidDataException(
                    $"PNG dimensions are {width}x{height}; expected "
                    + $"{expectedWidth}x{expectedHeight}: {path}");
            }
        }

        /// <summary>
        /// Moves raw0 to evidence and maps raw1..N to canonical source f0..f(N-1)
        /// through a unique sibling staging directory. Existing destinations
        /// are always a hard failure.
        /// </summary>
        internal static string RemapRawFrames(
            AuditionPvCityShot shot,
            string frameDirectory,
            string evidenceDirectory)
        {
            string normalizedFrames = RequireDirectory(frameDirectory);
            string normalizedEvidence = Path.GetFullPath(evidenceDirectory);
            Directory.CreateDirectory(normalizedEvidence);
            ValidateRawFrameSequence(shot, normalizedFrames);

            string stagingDirectory = Path.Combine(
                Path.GetDirectoryName(normalizedFrames)
                    ?? throw new InvalidOperationException(
                        "City frame directory has no parent."),
                ".city-" + ShotId(shot) + "-remap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stagingDirectory);
            bool completed = false;
            try
            {
                int rawLast = RawLastFrame(shot);
                for (int raw = RawFirstLogicalFrame; raw <= rawLast; raw++)
                {
                    MoveNew(
                        Path.Combine(normalizedFrames, RawFrameFileName(shot, raw)),
                        Path.Combine(
                            stagingDirectory,
                            AuditionPvCityHeroPocketCapture.SourceFrameFileName(
                                shot,
                                raw - RawFirstLogicalFrame)));
                }

                string warmupEvidence = Path.Combine(
                    normalizedEvidence,
                    WarmupEvidenceFileName(shot));
                MoveNew(
                    Path.Combine(
                        normalizedFrames,
                        RawFrameFileName(shot, RawWarmupFrame)),
                    warmupEvidence);

                for (int frame = AuditionPvCityHeroPocketCapture.GetSourceFirstFrame(shot);
                    frame <= AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot);
                    frame++)
                {
                    string fileName =
                        AuditionPvCityHeroPocketCapture.SourceFrameFileName(shot, frame);
                    MoveNew(
                        Path.Combine(stagingDirectory, fileName),
                        Path.Combine(normalizedFrames, fileName));
                }

                Directory.Delete(stagingDirectory, recursive: false);
                completed = true;
                return warmupEvidence.Replace('\\', '/');
            }
            finally
            {
                if (completed && Directory.Exists(stagingDirectory))
                {
                    Directory.Delete(stagingDirectory, recursive: false);
                }
            }
        }

        internal static void ValidateRawFrameSequence(
            AuditionPvCityShot shot,
            string frameDirectory)
        {
            ValidateExactNamedSequence(
                frameDirectory,
                ExpectedRawFrameCount(shot),
                index => RawFrameFileName(shot, index));
        }

        internal static void ValidateSourceFrameSequence(
            AuditionPvCityShot shot,
            string frameDirectory)
        {
            ValidateExactNamedSequence(
                frameDirectory,
                AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot),
                index => AuditionPvCityHeroPocketCapture.SourceFrameFileName(shot, index));
        }

        internal static SequenceVisualMetrics EvaluatePixels(
            Color32[] pixels,
            int width,
            int height,
            bool measureHud)
        {
            if (pixels == null || width <= 0 || height <= 0
                || pixels.Length != width * height)
            {
                throw new ArgumentException("City pixel buffer dimensions are invalid.");
            }

            SequenceVisualMetrics metrics = EvaluateSampledPixels(
                index => pixels[index],
                width,
                height);
            if (measureHud)
            {
                // Synthetic test buffers use presentation (top-left) row order.
                metrics.frameZeroHudAccentSamples = CountHudCyanAccentSamples(
                    (x, topY) => pixels[topY * width + x],
                    width,
                    height);
            }

            return metrics;
        }

        internal static SequenceVisualMetrics EvaluateTexturePixels(
            Texture2D texture,
            bool measureHud)
        {
            if (texture == null || !texture.isReadable
                || texture.width <= 0 || texture.height <= 0)
            {
                throw new ArgumentException(
                    "City texture pixel buffer is null, unreadable, or empty.");
            }

            NativeArray<Color32> native = texture.GetRawTextureData<Color32>();
            SequenceVisualMetrics metrics = EvaluateSampledPixels(
                index => native[index],
                texture.width,
                texture.height);
            if (measureHud)
            {
                // GetPixel provides semantic RGBA values with a bottom-left
                // origin, independent of raw texture byte order on the active
                // graphics backend.
                metrics.frameZeroHudAccentSamples = CountHudCyanAccentSamples(
                    (x, topY) => (Color32)texture.GetPixel(
                        x,
                        texture.height - 1 - topY),
                    texture.width,
                    texture.height);
            }

            return metrics;
        }

        private static SequenceVisualMetrics EvaluateSampledPixels(
            Func<int, Color32> readPixel,
            int width,
            int height)
        {
            var metrics = new SequenceVisualMetrics
            {
                minimumSampledLuma = 255
            };
            const int Step = 32;
            for (int y = Step / 2; y < height; y += Step)
            {
                for (int x = Step / 2; x < width; x += Step)
                {
                    Color32 pixel = readPixel(y * width + x);
                    int luma = (pixel.r * 54 + pixel.g * 183 + pixel.b * 19) >> 8;
                    metrics.minimumSampledLuma = Math.Min(
                        metrics.minimumSampledLuma,
                        luma);
                    metrics.maximumSampledLuma = Math.Max(
                        metrics.maximumSampledLuma,
                        luma);
                    if (pixel.r <= 10 && pixel.g <= 10 && pixel.b <= 10)
                    {
                        metrics.blackSampleCount++;
                    }

                    if (pixel.r >= 245 && pixel.g <= 12 && pixel.b >= 245)
                    {
                        metrics.magentaSampleCount++;
                    }

                    metrics.sampleCount++;
                }
            }

            metrics.blackRatio = metrics.blackSampleCount
                / (double)Math.Max(1L, metrics.sampleCount);
            metrics.magentaRatio = metrics.magentaSampleCount
                / (double)Math.Max(1L, metrics.sampleCount);
            metrics.maximumFrameMagentaRatio = metrics.magentaRatio;
            metrics.healthyFrameCount =
                metrics.blackRatio < MaximumSequenceBlackRatio ? 1 : 0;
            return metrics;
        }

        private static int CountHudCyanAccentSamples(
            Func<int, int, Color32> readPresentationPixel,
            int width,
            int height)
        {
            // Keep this sparse probe in presentation (top-left) coordinates.
            // The caller owns any Texture2D storage-orientation/channel mapping.
            const int Step = 32;
            int count = 0;
            for (int topY = Step / 2; topY < height; topY += Step)
            {
                if (topY < height * 0.84f || topY > height * 0.92f)
                {
                    continue;
                }

                for (int x = Step / 2; x < width; x += Step)
                {
                    if (x < width * 0.27f || x > width * 0.53f)
                    {
                        continue;
                    }

                    Color32 pixel = readPresentationPixel(x, topY);
                    bool cyanHudAccent = pixel.g >= 145
                        && pixel.b >= 165
                        && pixel.r <= 125
                        && pixel.b - pixel.r >= 45;
                    if (cyanHudAccent)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        internal static ScreenDeltaMetrics EvaluateScreenDelta(
            Color32[] before,
            Color32[] after,
            int sampleStride = 32)
        {
            if (before == null
                || after == null
                || before.Length == 0
                || before.Length != after.Length
                || sampleStride <= 0)
            {
                throw new ArgumentException(
                    "City screen-delta buffers must be non-empty and equal.");
            }

            return EvaluateScreenDelta(
                before.Length,
                index => before[index],
                index => after[index],
                sampleStride);
        }

        private static ScreenDeltaMetrics EvaluateScreenDelta(
            int pixelCount,
            Func<int, Color32> readBefore,
            Func<int, Color32> readAfter,
            int sampleStride)
        {
            var metrics = new ScreenDeltaMetrics();
            double absoluteRgb = 0d;
            for (int index = 0; index < pixelCount; index += sampleStride)
            {
                Color32 left = readBefore(index);
                Color32 right = readAfter(index);
                int red = Math.Abs(left.r - right.r);
                int green = Math.Abs(left.g - right.g);
                int blue = Math.Abs(left.b - right.b);
                absoluteRgb += (red + green + blue) / 3d;
                if (Math.Max(red, Math.Max(green, blue)) >= 8)
                {
                    metrics.changedSampleCount++;
                }

                metrics.sampleCount++;
            }

            metrics.meanAbsoluteRgb = absoluteRgb
                / Math.Max(1L, metrics.sampleCount);
            metrics.changedSampleRatio = metrics.changedSampleCount
                / (double)Math.Max(1L, metrics.sampleCount);
            return metrics;
        }

        internal static void ValidateScreenDelta(
            string label,
            ScreenDeltaMetrics metrics,
            double minimumMeanAbsoluteRgb,
            double minimumChangedRatio)
        {
            if (metrics == null
                || metrics.sampleCount <= 0
                || metrics.meanAbsoluteRgb < minimumMeanAbsoluteRgb
                || metrics.changedSampleRatio < minimumChangedRatio)
            {
                throw new InvalidOperationException(
                    label + " pixel delta is too small: mean="
                    + (metrics?.meanAbsoluteRgb ?? 0d).ToString(
                        "F3",
                        CultureInfo.InvariantCulture)
                    + ", changed="
                    + (metrics?.changedSampleRatio ?? 0d).ToString(
                        "P2",
                        CultureInfo.InvariantCulture)
                    + ".");
            }
        }

        internal static void ValidateVisualSequence(
            AuditionPvCityShot shot,
            SequenceVisualMetrics metrics)
        {
            int frameCount =
                AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot);
            int minimumHealthy = frameCount * MinimumHealthyFramePercent / 100;
            if (metrics == null
                || metrics.sampleCount <= 0
                || metrics.blackRatio >= MaximumSequenceBlackRatio
                || metrics.healthyFrameCount < minimumHealthy
                || metrics.maximumSampledLuma - metrics.minimumSampledLuma < 24)
            {
                throw new InvalidOperationException(
                    $"City {ShotId(shot)} black/flat-frame sanity failed: "
                    + $"black={metrics?.blackRatio ?? 1d:P2}, "
                    + $"healthy={metrics?.healthyFrameCount ?? 0}/{frameCount}.");
            }

            if (metrics.magentaRatio >= MaximumSequenceMagentaRatio
                || metrics.maximumFrameMagentaRatio >= MaximumFrameMagentaRatio)
            {
                throw new InvalidOperationException(
                    $"City {ShotId(shot)} missing-shader magenta sanity failed.");
            }

            switch (shot)
            {
                case AuditionPvCityShot.G01:
                    RequireHudOff(metrics, new[]
                    {
                        AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            AuditionPvCityShot.G01,
                            0)
                    });
                    ValidateScreenDelta(
                        "City G01 f0->f239",
                        metrics.primaryDelta,
                        1.5d,
                        0.05d);
                    break;
                case AuditionPvCityShot.G02:
                    RequireHudOn(metrics, new[] { 0, 120, 240, 419 }
                        .Select(frame =>
                            AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                                AuditionPvCityShot.G02,
                                frame))
                        .ToArray());
                    ValidateScreenDelta(
                        "City G02 f0->f419",
                        metrics.primaryDelta,
                        3d,
                        0.12d);
                    ValidateScreenDelta(
                        $"City G02 dodge f{G02DodgeVisualBeforeFrame}"
                        + $"->f{G02DodgeVisualAfterFrame}",
                        metrics.dodgeDelta,
                        2d,
                        0.05d);
                    break;
                case AuditionPvCityShot.G03:
                    RequireHudOff(metrics, new[]
                    {
                        AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            AuditionPvCityShot.G03,
                            0)
                    });
                    ValidateScreenDelta(
                        "City G03 f0->f276",
                        metrics.primaryDelta,
                        60d,
                        0.70d);
                    if (metrics.fullCoverFrameCount != 24
                        || metrics.minimumFullCoverSampledLuma < 220
                        || metrics.maximumFullCoverSpatialChannelRange > 4
                        || !metrics.fullCoverDecodedPixelHashesExact)
                    {
                        throw new InvalidOperationException(
                            "City G03 f276..f299 is not an exact uniform bright "
                            + "24-frame full-cover hold.");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shot), shot, null);
            }
        }

        private static void RequireHudOn(
            SequenceVisualMetrics metrics,
            int[] expectedFrames)
        {
            HudProbeMetrics[] probes = metrics.hudProbes
                ?? Array.Empty<HudProbeMetrics>();
            if (probes.Length != expectedFrames.Length
                || probes.Where((value, index) => value == null
                        || value.frame != expectedFrames[index]
                        || value.cyanAccentSamples < MinimumHudAccentSamples)
                    .Any())
            {
                string observed = string.Join(
                    ",",
                    probes.Select(value => value == null
                        ? "<null>"
                        : $"f{value.frame}:{value.cyanAccentSamples}"));
                throw new InvalidOperationException(
                    "City G02 HUD-on cyan pixel evidence is incomplete; expected "
                    + string.Join(",", expectedFrames.Select(frame =>
                        $"f{frame}>={MinimumHudAccentSamples}"))
                    + ", observed "
                    + observed
                    + ".");
            }
        }

        private static void RequireHudOff(
            SequenceVisualMetrics metrics,
            int[] expectedFrames)
        {
            HudProbeMetrics[] probes = metrics.hudProbes
                ?? Array.Empty<HudProbeMetrics>();
            if (probes.Length != expectedFrames.Length
                || probes.Where((value, index) => value == null
                        || value.frame != expectedFrames[index]
                        || value.cyanAccentSamples != 0)
                    .Any())
            {
                throw new InvalidOperationException(
                    "City HUD-off shot contains HUD cyan pixel evidence.");
            }
        }

        internal static void ValidateRecorderProof(ShotRecorderProof proof)
        {
            if (proof == null
                || !TryParseShot(proof.shotId, out AuditionPvCityShot shot)
                || proof.expectedRawFrameCount != ExpectedRawFrameCount(shot)
                || proof.recorderWarmupEndOfFrameCount != 2
                || proof.canonicalSourceFrameCount
                    != AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot)
                || proof.logicalFirstSourceFrame
                    != AuditionPvCityHeroPocketCapture.GetSelectStartFrame(shot)
                || proof.logicalLastSourceFrame
                    != AuditionPvCityHeroPocketCapture.GetSelectEndFrame(shot)
                || proof.recordedPreHandleFrameCount
                    != AuditionPvCityHeroPocketCapture.HandleFrameCount
                || proof.recordedPostHandleFrameCount
                    != AuditionPvCityHeroPocketCapture.HandleFrameCount
                || !proof.recorderPaddingActiveAtLogicalFrameZero
                || !proof.recorderAutoStoppedAfterLastFrame
                || proof.presentedFrameCount
                    != AuditionPvCityHeroPocketCapture.GetExpectedFrameCount(shot)
                || !proof.presentedFramesExact
                || !proof.presentationClockExact
                || !proof.directorStateRestored)
            {
                throw new InvalidOperationException(
                    "City Recorder proof does not satisfy raw-warmup, logical-frame, "
                    + "clock, auto-stop, and restoration contracts for "
                    + (proof?.shotId ?? "<null>") + ".");
            }
        }

        private static void BeginCapture(
            bool batchMode,
            bool produceApprovedSixtySecondEvidence = false)
        {
            if (SessionState.GetBool(SessionActiveKey, false)
                || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                throw new InvalidOperationException(
                    "City G01-G03 capture cannot start during another capture, "
                    + "Play Mode, compilation, or asset update.");
            }

            EnsureNoDirtyOpenScenes();
            ValidateAuthoredPreflight(
                CityHeroPocketAuthoredPackValidator.ValidateAuthoredOutputs);
            DateTime startedAtUtc = DateTime.UtcNow;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "City capture requires a successful Git provenance probe: "
                    + git.probeError);
            }
            if (git.isDirty)
            {
                throw new InvalidOperationException(
                    "City capture requires a clean Git worktree so every take "
                    + "has an immutable product and evidence identity.");
            }

            AuditionPvEngineSnapshot engine = AuditionPvEnvironmentProbe.ReadEngineSnapshot();
            if (!string.Equals(
                    engine.recorderPackageVersion,
                    AuditionPvCaptureContract.RecorderPackageVersion,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "City capture requires Unity Recorder "
                    + AuditionPvCaptureContract.RecorderPackageVersion
                    + "; found " + engine.recorderPackageVersion + ".");
            }

            string[] dependencyPaths = CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencyHashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            AuditionPvDependencyHash citySceneHash = dependencyHashes.FirstOrDefault(
                dependency => string.Equals(
                    dependency.path,
                    AuditionPvCityHeroPocketCapture.CityScenePath,
                    StringComparison.OrdinalIgnoreCase));
            if (citySceneHash == null
                || !citySceneHash.exists
                || !AuditionPvSha256.IsSha256(citySceneHash.sha256))
            {
                throw new InvalidOperationException(
                    "City product scene could not be hashed before capture.");
            }

            AuditionPvCityHeroPocketOutput output = null;
            try
            {
                output = AuditionPvCityHeroPocketCapture.ReserveNewOutput(
                    startedAtUtc,
                    git);
                var state = new PersistedRunnerState
                {
                    schema = RunnerSchema,
                    phase = RunnerPhase.AwaitingPlayMode.ToString(),
                    batchMode = batchMode,
                    produceApprovedSixtySecondEvidence =
                        produceApprovedSixtySecondEvidence,
                    startedAtUtc = startedAtUtc.ToString("O"),
                    captureId = output.captureId,
                    outputRoot = output.outputRoot,
                    outputDirectory = output.outputDirectory,
                    baselineDirectory = output.baselineDirectory,
                    gitCommitSha = git.commitSha,
                    gitBranch = git.branch,
                    gitWorktreeDirty = git.isDirty,
                    gitDirtyHashSha256 = git.dirtyStateHashSha256,
                    engine = CopyEngine(engine),
                    dependencyPaths = dependencyPaths,
                    dependencyHashesAtStart = dependencyHashes,
                    citySceneSha256AtStart = citySceneHash.sha256,
                    recorderProofs = Array.Empty<ShotRecorderProof>(),
                    runtimeProofs = Array.Empty<AuditionPvCityHeroPocketRuntimeProof>()
                };
                string statePath = Path.Combine(output.outputDirectory, StateFileName);
                SaveState(statePath, state);
                SessionState.SetString(SessionOwnerKey, SessionOwnerValue);
                SessionState.SetString(SessionStatePathKey, statePath);
                SessionState.SetBool(SessionBatchKey, batchMode);
                SessionState.SetBool(SessionActiveKey, true);

                EditorSceneManager.OpenScene(
                    AuditionPvCityHeroPocketCapture.CityScenePath,
                    OpenSceneMode.Single);
                Scene activeScene = SceneManager.GetActiveScene();
                if (!activeScene.IsValid()
                    || !activeScene.isLoaded
                    || activeScene.isDirty
                    || !string.Equals(
                        activeScene.path,
                        AuditionPvCityHeroPocketCapture.CityScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "City capture did not open a fresh clean single product scene.");
                }

                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                if (output != null)
                {
                    TryWriteFailureArtifact(
                        output.outputDirectory,
                        RunnerPhase.AwaitingPlayMode.ToString(),
                        exception,
                        null,
                        null);
                }

                ClearSession();
                throw;
            }
            finally
            {
                output?.Dispose();
            }
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange change)
        {
            if (!IsOwnedSession())
            {
                return;
            }

            if (change == PlayModeStateChange.EnteredPlayMode
                || change == PlayModeStateChange.EnteredEditMode)
            {
                ScheduleResume();
            }
        }

        private static void ScheduleResume()
        {
            if (resumeScheduled)
            {
                return;
            }

            resumeScheduled = true;
            EditorApplication.delayCall += ResumeOwnedSession;
        }

        private static void ResumeOwnedSession()
        {
            resumeScheduled = false;
            if (!IsOwnedSession())
            {
                return;
            }

            string statePath = SessionState.GetString(SessionStatePathKey, string.Empty);
            PersistedRunnerState state;
            try
            {
                state = LoadState(statePath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                bool batchSession = SessionState.GetBool(SessionBatchKey, false);
                string outputDirectory = string.IsNullOrWhiteSpace(statePath)
                    ? string.Empty
                    : Path.GetDirectoryName(statePath) ?? string.Empty;
                TryWriteFailureArtifact(
                    outputDirectory,
                    "state-load",
                    exception,
                    null,
                    null);
                ClearSession();
                if (batchSession)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                ScheduleResume();
                return;
            }

            RunnerPhase phase = ParsePhase(state.phase);
            if (EditorApplication.isPlaying)
            {
                if (phase == RunnerPhase.AwaitingPlayMode)
                {
                    LaunchPlayModeRunner(statePath, state);
                    return;
                }

                if (phase == RunnerPhase.Recording && activeBehaviour == null)
                {
                    FailAfterUnexpectedDomainReload(statePath, state);
                }

                return;
            }

            if (phase == RunnerPhase.AwaitingPlayMode)
            {
                EditorApplication.isPlaying = true;
                return;
            }

            if (phase == RunnerPhase.Recording)
            {
                state.failure =
                    "Play Mode exited before all three City Recorder sessions completed.";
                state.phase = RunnerPhase.FailedInPlayMode.ToString();
                SaveState(statePath, state);
                FinalizeAfterPlayMode(statePath, state);
                return;
            }

            if (phase == RunnerPhase.AwaitingEditMode
                || phase == RunnerPhase.FailedInPlayMode)
            {
                FinalizeAfterPlayMode(statePath, state);
            }
        }

        private static void LaunchPlayModeRunner(
            string statePath,
            PersistedRunnerState state)
        {
            if (activeBehaviour != null)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !scene.isLoaded
                || SceneManager.sceneCount != 1
                || !string.Equals(
                    scene.path,
                    AuditionPvCityHeroPocketCapture.CityScenePath,
                    StringComparison.Ordinal))
            {
                NotifyPlayModeFinished(
                    statePath,
                    state,
                    null,
                    null,
                    new InvalidOperationException(
                        "City capture entered Play Mode without one fresh product scene."));
                return;
            }

            state.phase = RunnerPhase.Recording.ToString();
            SaveState(statePath, state);
            var root = new GameObject("[AuditionPV_City_G01_G03_GoldenRunner]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            activeBehaviour = root.AddComponent<
                AuditionPvCityHeroPocketGoldenRunnerBehaviour>();
            activeBehaviour.Begin(statePath, state.outputDirectory, state);
        }

        private static void FailAfterUnexpectedDomainReload(
            string statePath,
            PersistedRunnerState state)
        {
            NotifyPlayModeFinished(
                statePath,
                state,
                state.recorderProofs,
                state.runtimeProofs,
                new InvalidOperationException(
                    "A domain reload interrupted an active City Recorder session; "
                    + "the take is invalid."));
        }

        internal static void NotifyPlayModeFinished(
            string statePath,
            PersistedRunnerState state,
            ShotRecorderProof[] recorderProofs,
            AuditionPvCityHeroPocketRuntimeProof[] runtimeProofs,
            Exception failure)
        {
            activeBehaviour = null;
            state.recorderProofs = recorderProofs
                ?? state.recorderProofs
                ?? Array.Empty<ShotRecorderProof>();
            state.runtimeProofs = runtimeProofs
                ?? state.runtimeProofs
                ?? Array.Empty<AuditionPvCityHeroPocketRuntimeProof>();
            state.failure = failure?.ToString() ?? string.Empty;
            state.phase = failure == null
                ? RunnerPhase.AwaitingEditMode.ToString()
                : RunnerPhase.FailedInPlayMode.ToString();
            try
            {
                SaveState(statePath, state);
            }
            finally
            {
                EditorApplication.isPlaying = false;
            }
        }

        internal static void ValidateAuthoredPreflight(Action validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            validator();
        }

        private static void FinalizeAfterPlayMode(
            string statePath,
            PersistedRunnerState state)
        {
            if (finalizing)
            {
                return;
            }

            finalizing = true;
            bool success = false;
            Exception failure = null;
            try
            {
                AuditionPvCityHeroPocketCapture.ReopenProductSceneAfterPlayMode();
                Scene reopened = SceneManager.GetActiveScene();
                if (!reopened.IsValid()
                    || !reopened.isLoaded
                    || reopened.isDirty
                    || SceneManager.sceneCount != 1
                    || !string.Equals(
                        reopened.path,
                        AuditionPvCityHeroPocketCapture.CityScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "City product scene lifecycle was not restored cleanly.");
                }

                if (!string.IsNullOrWhiteSpace(state.failure))
                {
                    throw new InvalidOperationException(
                        "City PlayMode recording failed.\n" + state.failure);
                }

                FinalizeSuccessfulCapture(statePath, state);
                success = true;
            }
            catch (Exception exception)
            {
                failure = exception;
                TryWriteFailureArtifact(
                    state.outputDirectory,
                    state.phase,
                    exception,
                    state.recorderProofs,
                    state.runtimeProofs);
                Debug.LogException(exception);
            }
            finally
            {
                bool batchMode = state.batchMode;
                string outputDirectory = state.outputDirectory;
                ClearSession();
                finalizing = false;
                if (success)
                {
                    Debug.Log(
                        "[AuditionPV] City G01-G03 golden sources passed: "
                        + outputDirectory);
                    if (batchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(outputDirectory);
                    }
                }
                else if (batchMode)
                {
                    EditorApplication.Exit(1);
                }
                else if (failure != null)
                {
                    EditorUtility.DisplayDialog(
                        "City G01-G03 golden capture failed",
                        failure.Message,
                        "OK");
                }
            }
        }

        private static void FinalizeSuccessfulCapture(
            string statePath,
            PersistedRunnerState state)
        {
            ValidateRuntimeProofSet(state.recorderProofs, state.runtimeProofs);
            string evidenceDirectory = Path.Combine(
                state.outputDirectory,
                EvidenceFolderName);
            Directory.CreateDirectory(evidenceDirectory);

            var visualMetrics = new List<SequenceVisualMetrics>(ShotOrder.Length);
            var hashes = new List<FrameHashEntry>(1024);
            foreach (AuditionPvCityShot shot in ShotOrder)
            {
                string frameDirectory = FrameDirectory(state.outputDirectory, shot);
                string warmupPath = RemapRawFrames(
                    shot,
                    frameDirectory,
                    evidenceDirectory);
                ValidatePngFile(
                    warmupPath,
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
                hashes.Add(CreateHashEntry(
                    "warmup/" + ShotId(shot),
                    warmupPath,
                    state.outputDirectory));

                ValidateSourceFrameSequence(shot, frameDirectory);
                for (int frame = AuditionPvCityHeroPocketCapture.GetSourceFirstFrame(shot);
                    frame <= AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot);
                    frame++)
                {
                    string path = Path.Combine(
                        frameDirectory,
                        AuditionPvCityHeroPocketCapture.SourceFrameFileName(shot, frame));
                    ValidatePngFile(
                        path,
                        AuditionPvCaptureContract.Width,
                        AuditionPvCaptureContract.Height);
                    hashes.Add(CreateHashEntry(
                        ShotId(shot) + "/f" + frame.ToString("0000", CultureInfo.InvariantCulture),
                        path,
                        state.outputDirectory));
                }

                SequenceVisualMetrics metrics =
                    AnalyzeVisualSequence(shot, frameDirectory);
                ValidateVisualSequence(shot, metrics);
                visualMetrics.Add(metrics);
            }

            CopyBaselines(state, hashes);
            int expectedHashEntryCount = ShotOrder.Length
                + ShotOrder.Sum(
                    AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount)
                + AuditionPvCityHeroPocketCapture
                    .CreateBaselineManifestEntries().Length;
            if (hashes.Count != expectedHashEntryCount)
            {
                throw new InvalidOperationException(
                    $"City hash evidence expected {expectedHashEntryCount} entries; "
                    + $"found {hashes.Count}.");
            }

            ValidateHashEntries(hashes);
            string hashPath = Path.Combine(evidenceDirectory, FrameHashFileName);
            WriteCanonicalHashLedgerNew(hashPath, hashes);
            ValidateCanonicalHashLedger(hashPath, hashes);

            AuditionPvGitSnapshot gitAtEnd =
                AuditionPvEnvironmentProbe.ReadGitSnapshot();
            ValidateStableGitSnapshot(CreateGitSnapshot(state), gitAtEnd);
            string[] dependencyPathsAtEnd = CollectCaptureDependencyPaths();
            if (!state.dependencyPaths.SequenceEqual(
                    dependencyPathsAtEnd,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "City capture dependency path set changed while recording.");
            }

            AuditionPvDependencyHash[] dependenciesAtEnd =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPathsAtEnd);
            ValidateStableDependencies(
                state.dependencyHashesAtStart,
                dependenciesAtEnd);
            AuditionPvDependencyHash citySceneHash = dependenciesAtEnd.FirstOrDefault(
                dependency => string.Equals(
                    dependency.path,
                    AuditionPvCityHeroPocketCapture.CityScenePath,
                    StringComparison.OrdinalIgnoreCase));
            if (citySceneHash == null
                || !string.Equals(
                    citySceneHash.sha256,
                    state.citySceneSha256AtStart,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "City product scene hash changed while recording.");
            }

            string proofPath = Path.Combine(
                evidenceDirectory,
                RuntimeProofFileName);
            WriteJsonNew(proofPath, new RuntimeProofArtifact
            {
                schema = RuntimeProofSchema,
                captureId = state.captureId,
                mapping =
                    "Each shot preserves Recorder raw0 as warm-up evidence; "
                    + "raw1..N map collision-free to canonical source f0..f(N-1); "
                    + "each logical shot is enclosed by recorded 180-frame pre/post handles.",
                gameplay =
                    "The City director uses product inputs and damage only; G03 starts "
                    + "after natural Won and enters the authored trigger without a runner "
                    + "transition-start call.",
                recorder = state.recorderProofs,
                runtime = state.runtimeProofs,
                visual = visualMetrics.ToArray(),
                frameHashArtifactPath = RelativePath(
                    state.outputDirectory,
                    hashPath),
                frameHashArtifactSha256 = AuditionPvSha256.FileHash(hashPath)
            });

            DateTime startedAtUtc = DateTime.Parse(
                state.startedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
            AuditionPvTestResult[] ordinaryResults = CreateTestResults(
                state,
                visualMetrics.ToArray(),
                proofPath,
                hashPath,
                startedAtUtc);
            AuditionPvCaptureManifest captureCoreManifest =
                AuditionPvCityHeroPocketCapture
                .CreateFinalManifestForExistingOutput(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    startedAtUtc,
                    ordinaryResults,
                    CreateGitSnapshot(state),
                    RestoreEngine(state.engine),
                    state.dependencyHashesAtStart);
            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(
                    captureCoreManifest);
            if (!AuditionPvSha256.IsSha256(captureCoreSha256))
            {
                throw new InvalidDataException(
                    "City capture could not create its immutable Gate capture-core identity.");
            }

            AuditionPvTestResult[] gateResults = WriteGateEvidenceArtifacts(
                state,
                evidenceDirectory,
                proofPath,
                hashPath,
                visualMetrics.ToArray(),
                captureCoreSha256,
                startedAtUtc);
            AuditionPvTestResult[] results = ordinaryResults
                .Concat(gateResults)
                .ToArray();
            if (state.produceApprovedSixtySecondEvidence)
            {
                foreach (SixtySecondEvidenceRange range in
                         ApprovedSixtySecondEvidenceRanges)
                {
                    ShotRecorderProof recorderProof = state.recorderProofs.Single(value =>
                        value != null && string.Equals(
                            value.shotId,
                            ShotId(range.shot),
                            StringComparison.Ordinal));
                    AuditionPvSixtySecondEvidenceBundle evidence =
                        AuditionPvSixtySecondEvidenceProducer.Produce(
                            new AuditionPvSixtySecondEvidenceRequest
                            {
                                captureCoreManifest = captureCoreManifest,
                                expectedCaptureCoreSha256 = captureCoreSha256,
                                sourceShotId = ShotId(range.shot),
                                sourceRangeStartFrame = range.sourceStartFrame,
                                sourceRangeEndFrame = range.sourceEndFrame,
                                selectStartFrame = range.selectStartFrame,
                                selectEndFrame = range.selectEndFrame,
                                runtimeWorkloadSealPath =
                                    recorderProof.runtimeWorkloadSealPath,
                                graphicsRootDirectory =
                                    AuditionPvSixtySecondGateManifestValidator
                                        .ProductionGraphicsRoot,
                                reviewRootDirectory =
                                    AuditionPvSixtySecondGateManifestValidator
                                        .ProductionReviewRoot,
                                approvedSourceRange = true,
                                cleanPlate = false,
                                linkedCleanPlateConfirmed = false
                            });
                    results = AuditionPvSixtySecondEvidenceProducer
                        .MergeCaptureTestResults(results, evidence);
                }
            }
            AuditionPvCaptureManifest manifest =
                AuditionPvCityHeroPocketCapture
                .CreateFinalManifestForExistingOutput(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    startedAtUtc,
                    results,
                    CreateGitSnapshot(state),
                    RestoreEngine(state.engine),
                    state.dependencyHashesAtStart);
            if (!string.Equals(
                    captureCoreSha256,
                    AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(
                        manifest),
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "City Gate evidence changed its immutable capture-core identity.");
            }
            string manifestPath = AuditionPvCaptureManifestWriter.WriteNew(manifest);
            ValidateManifestRoundTrip(manifestPath, state.captureId);

            state.phase = RunnerPhase.Complete.ToString();
            SaveState(statePath, state);
        }

        internal static void ValidateRuntimeProofSet(
            ShotRecorderProof[] recorderProofs,
            AuditionPvCityHeroPocketRuntimeProof[] runtimeProofs)
        {
            ShotRecorderProof[] recorder =
                recorderProofs ?? Array.Empty<ShotRecorderProof>();
            AuditionPvCityHeroPocketRuntimeProof[] runtime =
                runtimeProofs ?? Array.Empty<AuditionPvCityHeroPocketRuntimeProof>();
            if (recorder.Length != ShotOrder.Length
                || runtime.Length != ShotOrder.Length)
            {
                throw new InvalidOperationException(
                    "City capture requires exactly one Recorder and runtime proof "
                    + "for each of G01, G02, and G03.");
            }

            foreach (AuditionPvCityShot shot in ShotOrder)
            {
                string id = ShotId(shot);
                ShotRecorderProof recorderProof = recorder.SingleOrDefault(value =>
                    value != null
                    && string.Equals(value.shotId, id, StringComparison.Ordinal));
                AuditionPvCityHeroPocketRuntimeProof runtimeProof =
                    runtime.SingleOrDefault(value =>
                        value != null
                        && string.Equals(value.shotId, id, StringComparison.Ordinal));
                ValidateRecorderProof(recorderProof);
                if (runtimeProof == null)
                {
                    throw new InvalidOperationException(
                        "Missing City runtime proof for " + id + ".");
                }

                AuditionPvCityHeroPocketCapture.ValidateRuntimeProof(runtimeProof);
            }

            ValidateFinalDirectorLifecycle(recorder);

            AuditionPvCityHeroPocketRuntimeProof g02 = runtime.Single(value =>
                string.Equals(
                    value.shotId,
                    AuditionPvCityHeroPocketCapture.G02ShotId,
                    StringComparison.Ordinal));
            AuditionPvCityHeroPocketRuntimeProof g03 = runtime.Single(value =>
                string.Equals(
                    value.shotId,
                    AuditionPvCityHeroPocketCapture.G03ShotId,
                    StringComparison.Ordinal));
            ShotRecorderProof g02Recorder = recorder.Single(value =>
                string.Equals(
                    value.shotId,
                    AuditionPvCityHeroPocketCapture.G02ShotId,
                    StringComparison.Ordinal));
            ValidateG02G03Continuity(g02Recorder, g02, g03);
        }

        internal static void ValidateFinalDirectorLifecycle(
            ShotRecorderProof[] recorderProofs)
        {
            ShotRecorderProof[] recorder =
                recorderProofs ?? Array.Empty<ShotRecorderProof>();
            if (recorder.Length != ShotOrder.Length
                || recorder.Any(value => value == null
                    || value.directorRestoreCallCountAtSequenceEnd != 1
                    || value.directorDestroyCallCountAtSequenceEnd != 1))
            {
                throw new InvalidOperationException(
                    "City sequence must observe exactly one final director restore "
                    + "and one final director destroy in every Recorder proof.");
            }

            foreach (AuditionPvCityShot continuedShot in new[]
                     {
                         AuditionPvCityShot.G01,
                         AuditionPvCityShot.G02
                     })
            {
                string id = ShotId(continuedShot);
                ShotRecorderProof value = recorder.SingleOrDefault(proof =>
                    proof != null
                    && string.Equals(proof.shotId, id, StringComparison.Ordinal));
                if (value == null
                    || value.directorRestoreCallCountBeforeNextShot != 0
                    || value.directorDestroyCallCountBeforeNextShot != 0)
                {
                    throw new InvalidOperationException(
                        "City continuation observed premature director cleanup for "
                        + id + ".");
                }
            }
        }

        internal static void AppendValidatedSealedRuntimeProof(
            IList<AuditionPvCityHeroPocketRuntimeProof> destination,
            AuditionPvCityHeroPocketRuntimeProof candidate,
            AuditionPvCityShot expectedShot,
            Action<AuditionPvCityHeroPocketRuntimeProof> validator)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            int expectedIndex = Array.IndexOf(ShotOrder, expectedShot);
            string expectedId = ShotId(expectedShot);
            if (expectedIndex < 0
                || destination.Count != expectedIndex
                || !string.Equals(
                    candidate.shotId,
                    expectedId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "City sealed runtime proof order or shot ID drifted for "
                    + expectedId + ".");
            }

            for (int index = 0; index < destination.Count; index++)
            {
                AuditionPvCityHeroPocketRuntimeProof prior = destination[index];
                string requiredPriorId = ShotId(ShotOrder[index]);
                if (prior == null
                    || !string.Equals(
                        prior.shotId,
                        requiredPriorId,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "City sealed runtime proof history is duplicated or out of order.");
                }
            }

            validator(candidate);
            destination.Add(candidate);
        }

        internal static void ValidateG02G03Continuity(
            ShotRecorderProof g02Recorder,
            AuditionPvCityHeroPocketRuntimeProof g02,
            AuditionPvCityHeroPocketRuntimeProof g03)
        {
            if (g02Recorder == null
                || g02 == null
                || g03 == null
                || g02Recorder.directorRestoreCallCountBeforeNextShot != 0
                || g02Recorder.directorDestroyCallCountBeforeNextShot != 0
                || g02.encounterInstanceId <= 0
                || g02.playerInstanceId <= 0
                || g02.enemyInstanceId <= 0
                || g03.encounterInstanceId != g02.encounterInstanceId
                || g03.playerInstanceId != g02.playerInstanceId
                || g03.enemyInstanceId != g02.enemyInstanceId
                || g02.enemyDiedCount != 1
                || g02.encounterWonCount != 1
                || !g02.naturalEnemyDeathObserved
                || !g02.naturalWonObserved
                || !g02.g02EndedOutsideExitTrigger
                || !g03.continuityFromPreviousShot
                || !g03.g03StartedAlreadyWon
                || !g03.g03StartedTransitionArmed
                || g03.g03NewDamageEventCount != 0
                || g03.g03NewDeathEventCount != 0
                || g03.g03NewWonEventCount != 0
                || g03.captureTransitionStartCallCount != 0)
            {
                throw new InvalidOperationException(
                    "G03 must continue the exact G02 scene identities and terminal Won "
                    + "without replaying damage/death/Won or calling transition start.");
            }
        }

        private static void CopyBaselines(
            PersistedRunnerState state,
            ICollection<FrameHashEntry> hashes)
        {
            foreach (AuditionPvBaselineManifestEntry baseline in
                     AuditionPvCityHeroPocketCapture.CreateBaselineManifestEntries())
            {
                if (!TryParseShot(
                        baseline.shotId,
                        out AuditionPvCityShot shot))
                {
                    throw new InvalidOperationException(
                        "City baseline has an unknown shot ID: " + baseline.shotId);
                }

                string source = Path.Combine(
                    FrameDirectory(state.outputDirectory, shot),
                    AuditionPvCityHeroPocketCapture.SourceFrameFileName(
                        shot,
                        baseline.sourceFrame));
                string destination = Path.Combine(
                    state.baselineDirectory,
                    baseline.fileName);
                CopyNew(source, destination);
                string sourceHash = AuditionPvSha256.FileHash(source);
                string baselineHash = AuditionPvSha256.FileHash(destination);
                if (!string.Equals(sourceHash, baselineHash, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "City baseline is not a byte-exact source-frame copy: "
                        + baseline.id);
                }

                hashes.Add(CreateHashEntry(
                    "baseline/" + baseline.id,
                    destination,
                    state.outputDirectory));
            }
        }

        private static SequenceVisualMetrics AnalyzeVisualSequence(
            AuditionPvCityShot shot,
            string frameDirectory)
        {
            var aggregate = new SequenceVisualMetrics
            {
                shotId = ShotId(shot),
                minimumSampledLuma = 255
            };
            int first = AuditionPvCityHeroPocketCapture.GetSourceFirstFrame(shot);
            int last = AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot);
            var decodedHashBuffer = new byte[64 * 1024];
            int[] hudProbeFrames = shot switch
            {
                AuditionPvCityShot.G01 => new[]
                {
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 0)
                },
                AuditionPvCityShot.G02 => new[] { 0, 120, 240, 419 }
                    .Select(frame =>
                        AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            shot,
                            frame))
                    .ToArray(),
                AuditionPvCityShot.G03 => new[]
                {
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 0)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(shot), shot, null)
            };
            var hudProbes = new List<HudProbeMetrics>(hudProbeFrames.Length);
            var fullCoverHashes = new List<string>(24);
            for (int frame = first; frame <= last; frame++)
            {
                string path = Path.Combine(
                    frameDirectory,
                    AuditionPvCityHeroPocketCapture.SourceFrameFileName(shot, frame));
                Texture2D texture = LoadPng(path);
                try
                {
                    bool measureHud = Array.IndexOf(hudProbeFrames, frame) >= 0;
                    SequenceVisualMetrics current = EvaluateTexturePixels(
                        texture,
                        measureHud);
                    NativeArray<Color32> native =
                        texture.GetRawTextureData<Color32>();
                    aggregate.sampleCount += current.sampleCount;
                    aggregate.blackSampleCount += current.blackSampleCount;
                    aggregate.magentaSampleCount += current.magentaSampleCount;
                    aggregate.healthyFrameCount += current.healthyFrameCount;
                    aggregate.magentaAffectedFrameCount +=
                        current.magentaSampleCount > 0 ? 1 : 0;
                    aggregate.maximumFrameMagentaRatio = Math.Max(
                        aggregate.maximumFrameMagentaRatio,
                        current.magentaRatio);
                    aggregate.minimumSampledLuma = Math.Min(
                        aggregate.minimumSampledLuma,
                        current.minimumSampledLuma);
                    aggregate.maximumSampledLuma = Math.Max(
                        aggregate.maximumSampledLuma,
                        current.maximumSampledLuma);
                    if (measureHud)
                    {
                        hudProbes.Add(new HudProbeMetrics
                        {
                            frame = frame,
                            cyanAccentSamples = current.frameZeroHudAccentSamples
                        });
                    }

                    int logicalFrame =
                        AuditionPvCityHeroPocketCapture.SourceToLogicalFrame(
                            shot,
                            frame);
                    if (shot == AuditionPvCityShot.G03 && logicalFrame >= 276)
                    {
                        aggregate.fullCoverFrameCount++;
                        aggregate.minimumFullCoverSampledLuma = Math.Min(
                            aggregate.minimumFullCoverSampledLuma,
                            current.minimumSampledLuma);
                        aggregate.maximumFullCoverSpatialChannelRange = Math.Max(
                            aggregate.maximumFullCoverSpatialChannelRange,
                            EvaluateMaximumSpatialChannelRange(native));
                        fullCoverHashes.Add(HashDecodedPixels(
                            texture,
                            decodedHashBuffer));
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }

            aggregate.blackRatio = aggregate.blackSampleCount
                / (double)Math.Max(1L, aggregate.sampleCount);
            aggregate.magentaRatio = aggregate.magentaSampleCount
                / (double)Math.Max(1L, aggregate.sampleCount);
            aggregate.hudProbes = hudProbes
                .OrderBy(value => value.frame)
                .ToArray();
            aggregate.fullCoverDecodedPixelHashesExact =
                fullCoverHashes.Count == 24
                && fullCoverHashes.Distinct(StringComparer.Ordinal).Count() == 1;
            aggregate.primaryDelta = shot switch
            {
                AuditionPvCityShot.G01 => AnalyzeScreenDelta(
                    frameDirectory,
                    shot,
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 0),
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 239)),
                AuditionPvCityShot.G02 => AnalyzeScreenDelta(
                    frameDirectory,
                    shot,
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 0),
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 419)),
                AuditionPvCityShot.G03 => AnalyzeScreenDelta(
                    frameDirectory,
                    shot,
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 0),
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(shot, 276)),
                _ => throw new ArgumentOutOfRangeException(nameof(shot), shot, null)
            };
            if (shot == AuditionPvCityShot.G02)
            {
                aggregate.dodgeDelta = AnalyzeScreenDelta(
                    frameDirectory,
                    shot,
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                        shot,
                        G02DodgeVisualBeforeFrame),
                    AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                        shot,
                        G02DodgeVisualAfterFrame));
            }

            return aggregate;
        }

        private static ScreenDeltaMetrics AnalyzeScreenDelta(
            string frameDirectory,
            AuditionPvCityShot shot,
            int beforeFrame,
            int afterFrame)
        {
            Texture2D before = LoadPng(Path.Combine(
                frameDirectory,
                AuditionPvCityHeroPocketCapture.SourceFrameFileName(shot, beforeFrame)));
            Texture2D after = LoadPng(Path.Combine(
                frameDirectory,
                AuditionPvCityHeroPocketCapture.SourceFrameFileName(shot, afterFrame)));
            try
            {
                NativeArray<Color32> beforePixels =
                    before.GetRawTextureData<Color32>();
                NativeArray<Color32> afterPixels =
                    after.GetRawTextureData<Color32>();
                return EvaluateScreenDelta(
                    beforePixels.Length,
                    index => beforePixels[index],
                    index => afterPixels[index],
                    sampleStride: 32);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(before);
                UnityEngine.Object.DestroyImmediate(after);
            }
        }

        private static int EvaluateMaximumSpatialChannelRange(
            NativeArray<Color32> pixels)
        {
            int minimumRed = 255;
            int minimumGreen = 255;
            int minimumBlue = 255;
            int maximumRed = 0;
            int maximumGreen = 0;
            int maximumBlue = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                minimumRed = Math.Min(minimumRed, pixel.r);
                minimumGreen = Math.Min(minimumGreen, pixel.g);
                minimumBlue = Math.Min(minimumBlue, pixel.b);
                maximumRed = Math.Max(maximumRed, pixel.r);
                maximumGreen = Math.Max(maximumGreen, pixel.g);
                maximumBlue = Math.Max(maximumBlue, pixel.b);
            }

            return Math.Max(
                maximumRed - minimumRed,
                Math.Max(
                    maximumGreen - minimumGreen,
                    maximumBlue - minimumBlue));
        }

        private static string HashDecodedPixels(
            Texture2D texture,
            byte[] chunkBuffer)
        {
            if (chunkBuffer == null || chunkBuffer.Length == 0)
            {
                throw new ArgumentException(
                    "Decoded-pixel hash chunk buffer is required.",
                    nameof(chunkBuffer));
            }

            NativeArray<byte> bytes = texture.GetRawTextureData<byte>();
            return HashDecodedBytesChunked(
                bytes.Length,
                chunkBuffer,
                (offset, target, count) => NativeArray<byte>.Copy(
                    bytes,
                    offset,
                    target,
                    0,
                    count));
        }

        internal static string HashDecodedPixelBytes(
            byte[] decodedPixels,
            int chunkSize = 64 * 1024)
        {
            if (decodedPixels == null || decodedPixels.Length == 0 || chunkSize <= 0)
            {
                throw new ArgumentException(
                    "Decoded pixel bytes and a positive chunk size are required.");
            }

            var chunkBuffer = new byte[Math.Min(chunkSize, decodedPixels.Length)];
            return HashDecodedBytesChunked(
                decodedPixels.Length,
                chunkBuffer,
                (offset, target, count) => Array.Copy(
                    decodedPixels,
                    offset,
                    target,
                    0,
                    count));
        }

        private static string HashDecodedBytesChunked(
            int byteLength,
            byte[] chunkBuffer,
            Action<int, byte[], int> fillChunk)
        {
            using SHA256 sha256 = SHA256.Create();
            for (int offset = 0; offset < byteLength; offset += chunkBuffer.Length)
            {
                int count = Math.Min(chunkBuffer.Length, byteLength - offset);
                fillChunk(offset, chunkBuffer, count);
                sha256.TransformBlock(
                    chunkBuffer,
                    0,
                    count,
                    chunkBuffer,
                    0);
            }

            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            byte[] hash = sha256.Hash
                ?? throw new InvalidOperationException(
                    "Decoded-pixel SHA-256 did not finalize.");
            var builder = new StringBuilder(hash.Length * 2);
            foreach (byte value in hash)
            {
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static Texture2D LoadPng(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true)
            {
                name = "CityGoldenValidation_" + Path.GetFileNameWithoutExtension(path)
            };
            if (!ImageConversion.LoadImage(texture, bytes, markNonReadable: false)
                || texture.width != AuditionPvCaptureContract.Width
                || texture.height != AuditionPvCaptureContract.Height)
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidDataException(
                    "Unity could not decode an exact QHD City PNG: " + path);
            }

            return texture;
        }

        private static FrameHashEntry CreateHashEntry(
            string id,
            string path,
            string outputDirectory)
        {
            var info = new FileInfo(path);
            if (!info.Exists || info.Length <= 24)
            {
                throw new InvalidDataException(
                    "City hash source is absent or truncated: " + path);
            }

            return new FrameHashEntry
            {
                id = id,
                relativePath = RelativePath(outputDirectory, path),
                byteLength = info.Length,
                sha256 = AuditionPvSha256.FileHash(path)
            };
        }

        internal static void ValidateHashEntries(IEnumerable<FrameHashEntry> values)
        {
            FrameHashEntry[] entries =
                (values ?? Array.Empty<FrameHashEntry>()).ToArray();
            if (entries.Length == 0
                || entries.Select(value => value.id)
                    .Distinct(StringComparer.Ordinal)
                    .Count() != entries.Length
                || entries.Select(value => value.relativePath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count() != entries.Length
                || entries.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.id)
                    || string.IsNullOrWhiteSpace(value.relativePath)
                    || value.byteLength <= 24
                    || !AuditionPvSha256.IsSha256(value.sha256)))
            {
                throw new InvalidOperationException(
                    "City frame/baseline hash evidence is incomplete or duplicated.");
            }
        }

        internal static void WriteCanonicalHashLedgerNew(
            string path,
            IEnumerable<FrameHashEntry> values)
        {
            FrameHashEntry[] entries =
                (values ?? Array.Empty<FrameHashEntry>())
                .OrderBy(value => value.relativePath, StringComparer.Ordinal)
                .ToArray();
            ValidateHashEntries(entries);
            string parent = Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException(
                    "City frame hash ledger has no parent.");
            Directory.CreateDirectory(parent);
            using var stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            foreach (FrameHashEntry entry in entries)
            {
                if (entry.relativePath.Contains('\n')
                    || entry.relativePath.Contains('\r')
                    || entry.relativePath.StartsWith("/", StringComparison.Ordinal)
                    || entry.relativePath.Contains("..", StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Unsafe relative path in City frame hash ledger: "
                        + entry.relativePath);
                }

                writer.Write(entry.sha256);
                writer.Write("  ");
                writer.Write(entry.relativePath.Replace('\\', '/'));
                writer.Write('\n');
            }

            writer.Flush();
            stream.Flush(flushToDisk: true);
        }

        internal static void ValidateCanonicalHashLedger(
            string path,
            IEnumerable<FrameHashEntry> expectedValues)
        {
            FrameHashEntry[] expected =
                (expectedValues ?? Array.Empty<FrameHashEntry>())
                .OrderBy(value => value.relativePath, StringComparer.Ordinal)
                .ToArray();
            ValidateHashEntries(expected);
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length != expected.Length)
            {
                throw new InvalidDataException(
                    "City frame hash ledger line count drifted.");
            }

            for (int index = 0; index < lines.Length; index++)
            {
                string exact = expected[index].sha256
                    + "  "
                    + expected[index].relativePath.Replace('\\', '/');
                if (!string.Equals(lines[index], exact, StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "City frame hash ledger is not canonical at line "
                        + (index + 1) + ".");
                }
            }
        }

        private static AuditionPvTestResult[] CreateTestResults(
            PersistedRunnerState state,
            SequenceVisualMetrics[] visual,
            string proofPath,
            string hashPath,
            DateTime startedAtUtc)
        {
            string frameLedgerSha256 = AuditionPvSha256.FileHash(hashPath);
            if (!AuditionPvSha256.IsSha256(frameLedgerSha256))
            {
                throw new InvalidDataException(
                    "City frame_hashes.sha256 artifact could not be hashed.");
            }

            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            int logicalFrames = ShotOrder.Sum(
                AuditionPvCityHeroPocketCapture.GetExpectedFrameCount);
            int sourceFrames = ShotOrder.Sum(
                AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount);
            int rawFrames = ShotOrder.Sum(ExpectedRawFrameCount);
            return new[]
            {
                Passed(
                    "recorder",
                    "three-raw-warmups-and-logical-frame-maps",
                    duration,
                    $"Recorder 5.1.6 QHD60 captured {rawFrames} raw PNGs; "
                    + $"three raw0 warm-ups were preserved, {sourceFrames} canonical "
                    + $"source frames include 180/180 handles, and {logicalFrames} "
                    + "logical frames were mapped without overwrite.",
                    hashPath),
                Passed(
                    "product-state",
                    "g01-g02-g03-real-gameplay-and-transition",
                    duration,
                    "Core runtime proofs passed real City inputs, natural damage/death/Won, "
                    + "and G03 trigger/transition exact-once milestones.",
                    proofPath),
                Passed(
                    "render",
                    "png-pixels-hud-and-shader-sanity",
                    duration,
                    string.Join("; ", visual.Select(metrics =>
                        $"{metrics.shotId}: black={metrics.blackRatio:P2}, "
                        + $"magenta={metrics.magentaRatio:P3}, "
                        + "HUD="
                        + string.Join(",", (metrics.hudProbes
                                ?? Array.Empty<HudProbeMetrics>())
                            .Select(value =>
                                $"f{value.frame}:{value.cyanAccentSamples}")))),
                    hashPath),
                Passed(
                    "provenance",
                    "git-dependencies-scene-and-frame-hashes",
                    duration,
                    $"Git dirty-state and {state.dependencyHashesAtStart.Length} "
                    + "dependency hashes remained stable; City scene SHA-256="
                    + state.citySceneSha256AtStart
                    + "; frame_hashes.sha256 artifact SHA-256="
                    + frameLedgerSha256 + ".",
                    hashPath),
                Passed(
                    "lifecycle",
                    "shot-state-restored-and-city-scene-reopened",
                    duration,
                    "All directors restored product state and PresentationClock; Play Mode "
                    + "exited and the clean single City product scene reopened.",
                    proofPath)
            };
        }

        private static AuditionPvTestResult[] WriteGateEvidenceArtifacts(
            PersistedRunnerState state,
            string evidenceDirectory,
            string runtimeProofPath,
            string frameHashLedgerPath,
            SequenceVisualMetrics[] visualMetrics,
            string captureCoreSha256,
            DateTime startedAtUtc)
        {
            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            string createdAtUtc = startedAtUtc.ToString(
                "O",
                CultureInfo.InvariantCulture);
            AuditionPvPinnedArtifact runtimePin = Pin(runtimeProofPath);
            AuditionPvPinnedArtifact ledgerPin = Pin(frameHashLedgerPath);
            var results = new List<AuditionPvTestResult>();
            string semanticDirectory = Path.Combine(
                evidenceDirectory,
                "semantic_beats");
            Directory.CreateDirectory(semanticDirectory);

            foreach (AuditionPvCityShot shot in ShotOrder)
            {
                string sourceShotId = ShotId(shot);
                string authorshipPath = Path.Combine(
                    evidenceDirectory,
                    sourceShotId + "_shot_authorship.json");
                WriteJsonNew(authorshipPath, new AuditionPvShotAuthorshipArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator
                        .ShotAuthorshipSchema,
                    sourceCaptureCoreSha256 = captureCoreSha256,
                    captureId = state.captureId,
                    sourceShotId = sourceShotId,
                    cameraId = AuditionPvCityHeroPocketCapture.GetGateCameraId(shot),
                    gameplayState = AuditionPvCityHeroPocketCapture
                        .GetGateGameplayState(shot),
                    timelineId = AuditionPvCityHeroPocketCapture.GetGateTimelineId(shot),
                    deterministicSeed = AuditionPvCityHeroPocketCapture
                        .DeterministicRandomSeed,
                    runtimeProof = runtimePin,
                    tool = nameof(AuditionPvCityHeroPocketGoldenRunner),
                    toolVersion = "2",
                    createdAtUtc = createdAtUtc
                });
                string authorshipSha256 = AuditionPvSha256.FileHash(authorshipPath);
                results.Add(GatePassed(
                    "shot-authorship/" + sourceShotId,
                    duration,
                    $"artifact-sha256={authorshipSha256}; capture-core-sha256={captureCoreSha256}; exact-camera-state-seed-timeline=true",
                    authorshipPath));
                results.Add(GatePassed(
                    "shot-authorship-runtime/" + sourceShotId,
                    duration,
                    $"artifact-sha256={runtimePin.sha256}; capture-core-sha256={captureCoreSha256}; exact-runtime=true",
                    runtimeProofPath));

                AuditionPvCityHeroPocketRuntimeProof runtime =
                    (state.runtimeProofs ?? Array.Empty<AuditionPvCityHeroPocketRuntimeProof>())
                    .Single(value => value != null
                        && string.Equals(
                            value.shotId,
                            sourceShotId,
                            StringComparison.Ordinal));
                SequenceVisualMetrics visual =
                    (visualMetrics ?? Array.Empty<SequenceVisualMetrics>())
                    .Single(value => value != null
                        && string.Equals(
                            value.shotId,
                            sourceShotId,
                            StringComparison.Ordinal));
                foreach (CitySemanticBeatSpec beat in
                         CreateCitySemanticBeatSpecs(shot, runtime, visual))
                {
                    string artifactPath = Path.Combine(
                        semanticDirectory,
                        sourceShotId + "_" + beat.beatId + ".json");
                    WriteJsonNew(artifactPath, new CitySemanticBeatRuntimeArtifact
                    {
                        schemaVersion =
                            "dimension-brawl.audition-pv.city-semantic-beat-runtime.v1",
                        sourceCaptureCoreSha256 = captureCoreSha256,
                        captureId = state.captureId,
                        sourceShotId = sourceShotId,
                        beatId = beat.beatId,
                        runtimeFactKey = beat.beatId,
                        sourceRangeStartFrame =
                            AuditionPvCityHeroPocketCapture.GetSourceFirstFrame(shot),
                        sourceRangeEndFrame =
                            AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot),
                        sourceFactStartFrame =
                            AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                                shot,
                                beat.logicalStartFrame),
                        sourceFactEndFrame =
                            AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                                shot,
                                beat.logicalEndFrame),
                        exactFacts = beat.exactFacts,
                        runtimeProof = runtimePin,
                        sourceFrameLedger = ledgerPin,
                        producer = nameof(AuditionPvCityHeroPocketGoldenRunner),
                        createdAtUtc = createdAtUtc
                    });
                    string artifactSha256 = AuditionPvSha256.FileHash(artifactPath);
                    results.Add(GatePassed(
                        "semantic-beat/" + beat.beatId,
                        duration,
                        $"artifact-sha256={artifactSha256}; semantic-fact={beat.beatId}; capture-core-sha256={captureCoreSha256}; exact-runtime=true",
                        artifactPath));
                }
            }

            return results.ToArray();
        }

        private static CitySemanticBeatSpec[] CreateCitySemanticBeatSpecs(
            AuditionPvCityShot shot,
            AuditionPvCityHeroPocketRuntimeProof runtime,
            SequenceVisualMetrics visual)
        {
            if (runtime == null || visual == null)
            {
                throw new InvalidDataException(
                    "City semantic evidence requires runtime and visual proof.");
            }

            return shot switch
            {
                AuditionPvCityShot.G01 => new[]
                {
                    new CitySemanticBeatSpec(
                        "city-alert",
                        0,
                        120,
                        new[]
                        {
                            $"live-actors={runtime.g01MidgroundActorsObserved}",
                            $"line-of-sight={runtime.g01PlayerEnemyLineOfSightClear}",
                            "hud=off"
                        }),
                    new CitySemanticBeatSpec(
                        "city-skyline",
                        0,
                        AuditionPvCityHeroPocketCapture.G01LastFrame,
                        new[]
                        {
                            $"foreground={runtime.g01ForegroundDepthObserved}",
                            $"background={runtime.g01BackgroundDepthObserved}",
                            $"three-depth={runtime.g01ThreeDepthCompositionObserved}",
                            $"screen-delta={visual.primaryDelta.changedSampleRatio.ToString("F6", CultureInfo.InvariantCulture)}"
                        })
                },
                AuditionPvCityShot.G02 => new[]
                {
                    new CitySemanticBeatSpec(
                        "city-movement",
                        AuditionPvCityHeroPocketCapture.G02FirstMoveDownFrame,
                        AuditionPvCityHeroPocketCapture.G02ThirdMoveUpFrame,
                        new[]
                        {
                            $"path-length={runtime.g02PlayerPathLength.ToString("F4", CultureInfo.InvariantCulture)}",
                            $"net-displacement={runtime.g02PlayerNetDisplacement.ToString("F4", CultureInfo.InvariantCulture)}",
                            $"in-bounds={runtime.g02PlayerStayedInBounds}"
                        }),
                    new CitySemanticBeatSpec(
                        "city-fire",
                        AuditionPvCityHeroPocketCapture.G02FirstAttackDownFrame,
                        AuditionPvCityHeroPocketCapture.G02AttackHoldUpFrame,
                        new[]
                        {
                            $"projectiles={runtime.rangedProjectileFiredCount}",
                            $"enemy-damage-events={runtime.enemyDamagedCount}",
                            $"natural-won={runtime.naturalWonObserved}"
                        }),
                    new CitySemanticBeatSpec(
                        "city-hud-gameplay",
                        0,
                        AuditionPvCityHeroPocketCapture.G02LastFrame,
                        new[]
                        {
                            $"hud-probes={visual.hudProbes?.Length ?? 0}",
                            $"pointer-schedule={runtime.g02PointerScheduleExact}",
                            $"player-framing={runtime.g02PlayerFramingPassCount}/{runtime.g02PlayerFramingSampleCount}"
                        })
                },
                AuditionPvCityShot.G03 => new[]
                {
                    new CitySemanticBeatSpec(
                        "dimensional-anomaly",
                        0,
                        42,
                        new[]
                        {
                            $"portal-frame={runtime.transitionPortalAuthoredLogicalFrame}",
                            $"trigger-accepted={runtime.transitionTriggerAcceptedCount}",
                            $"hud-hidden={runtime.hudHiddenBeforeLogicalFrameZero}"
                        }),
                    new CitySemanticBeatSpec(
                        "dimension-rift-transition",
                        0,
                        AuditionPvCityHeroPocketCapture.G03LastFrame,
                        new[]
                        {
                            $"transition-started={runtime.transitionStartedCount}",
                            $"full-cover={runtime.transitionFullCoverCount}",
                            $"exit-ready={runtime.transitionExitReadyCount}",
                            $"clean-cover-frames={runtime.cleanCoverFrameCount}"
                        })
                },
                _ => throw new ArgumentOutOfRangeException(nameof(shot), shot, null)
            };
        }

        private static AuditionPvPinnedArtifact Pin(string path)
        {
            string fullPath = Path.GetFullPath(path).Replace('\\', '/');
            return new AuditionPvPinnedArtifact
            {
                path = fullPath,
                sha256 = AuditionPvSha256.FileHash(fullPath)
            };
        }

        private static AuditionPvTestResult GatePassed(
            string name,
            long duration,
            string details,
            string artifactPath)
        {
            return Passed(
                AuditionPvCityHeroPocketCapture.GateEvidenceTestSuite,
                name,
                duration,
                details,
                Path.GetFullPath(artifactPath));
        }

        private static AuditionPvTestResult Passed(
            string suite,
            string name,
            long duration,
            string details,
            string artifactPath)
        {
            return new AuditionPvTestResult
            {
                suite = suite,
                name = name,
                status = "passed",
                durationMilliseconds = duration,
                details = details,
                artifactPath = artifactPath?.Replace('\\', '/') ?? string.Empty
            };
        }

        private static void ValidateManifestRoundTrip(
            string manifestPath,
            string captureId)
        {
            AuditionPvCaptureManifest manifest =
                JsonUtility.FromJson<AuditionPvCaptureManifest>(
                    File.ReadAllText(manifestPath));
            AuditionPvCaptureManifestWriter.Validate(manifest);
            if (!string.Equals(manifest.captureId, captureId, StringComparison.Ordinal)
                || manifest.shots.Length != ShotOrder.Length)
            {
                throw new InvalidOperationException(
                    "City manifest did not preserve the three-shot contract.");
            }

            foreach (AuditionPvCityShot shot in ShotOrder)
            {
                string id = ShotId(shot);
                AuditionPvShotManifestEntry entry = manifest.shots.SingleOrDefault(
                    value => string.Equals(value.id, id, StringComparison.Ordinal));
                if (entry == null
                    || entry.startFrame
                        != AuditionPvCityHeroPocketCapture.GetSourceFirstFrame(shot)
                    || entry.endFrame
                        != AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot)
                    || entry.expectedFrameCount
                        != AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot))
                {
                    throw new InvalidOperationException(
                        "City manifest frame contract drifted for " + id + ".");
                }
            }

            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(
                    manifest);
            var requiredGateResults = new List<string>();
            foreach (AuditionPvCityShot shot in ShotOrder)
            {
                string id = ShotId(shot);
                requiredGateResults.Add("shot-authorship/" + id);
                requiredGateResults.Add("shot-authorship-runtime/" + id);
                requiredGateResults.AddRange(
                    AuditionPvCityHeroPocketCapture.GetGateSemanticBeatIds(shot)
                        .Select(beat => "semantic-beat/" + beat));
            }

            foreach (string name in requiredGateResults)
            {
                AuditionPvTestResult result = manifest.testResults.SingleOrDefault(value =>
                    value != null
                    && value.suite
                        == AuditionPvCityHeroPocketCapture.GateEvidenceTestSuite
                    && value.name == name);
                if (result == null
                    || result.status != "passed"
                    || string.IsNullOrWhiteSpace(result.artifactPath)
                    || !File.Exists(result.artifactPath)
                    || !result.details.Contains(
                        "artifact-sha256="
                        + AuditionPvSha256.FileHash(result.artifactPath),
                        StringComparison.Ordinal)
                    || !result.details.Contains(
                        "capture-core-sha256=" + captureCoreSha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "City manifest lost an exact Gate evidence result: " + name);
                }
            }
        }

        private static void ValidateExactNamedSequence(
            string frameDirectory,
            int expectedCount,
            Func<int, string> expectedName)
        {
            string directory = RequireDirectory(frameDirectory);
            string[] files = Directory.GetFiles(
                    directory,
                    "frame_*.png",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            if (files.Length != expectedCount)
            {
                throw new InvalidOperationException(
                    $"City capture requires {expectedCount} exact PNGs in "
                    + $"'{directory}'; found {files.Length}.");
            }

            for (int index = 0; index < expectedCount; index++)
            {
                string expected = Path.Combine(directory, expectedName(index));
                if (!string.Equals(
                        Path.GetFullPath(files[index]),
                        Path.GetFullPath(expected),
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "City frame sequence is not contiguous at index " + index + ".");
                }
            }
        }

        private static void MoveNew(string source, string destination)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("City remap source is missing.", source);
            }

            if (File.Exists(destination) || Directory.Exists(destination))
            {
                throw new IOException(
                    "City capture never overwrites a remap destination: "
                    + destination);
            }

            string parent = Path.GetDirectoryName(destination)
                ?? throw new InvalidOperationException(
                    "City remap destination has no parent.");
            if (!Directory.Exists(parent))
            {
                throw new DirectoryNotFoundException(parent);
            }

            File.Move(source, destination);
        }

        private static void CopyNew(string source, string destination)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("City baseline source is missing.", source);
            }

            if (File.Exists(destination) || Directory.Exists(destination))
            {
                throw new IOException(
                    "City capture never overwrites a baseline destination: "
                    + destination);
            }

            string parent = Path.GetDirectoryName(destination)
                ?? throw new InvalidOperationException(
                    "City baseline destination has no parent.");
            Directory.CreateDirectory(parent);
            File.Copy(source, destination, overwrite: false);
        }

        private static string RequireDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "City capture directory must not be empty.",
                    nameof(path));
            }

            string normalized = Path.GetFullPath(path);
            if (!Directory.Exists(normalized))
            {
                throw new DirectoryNotFoundException(normalized);
            }

            return normalized;
        }

        private static string FrameDirectory(
            string outputDirectory,
            AuditionPvCityShot shot)
        {
            return Path.Combine(outputDirectory, "frames", ShotId(shot));
        }

        internal static string ShotId(AuditionPvCityShot shot)
        {
            return AuditionPvCityHeroPocketCapture.GetShotId(shot);
        }

        private static bool TryParseShot(string value, out AuditionPvCityShot shot)
        {
            return Enum.TryParse(value, ignoreCase: true, out shot)
                && ShotOrder.Contains(shot);
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset)
        {
            return bytes[offset] << 24
                | bytes[offset + 1] << 16
                | bytes[offset + 2] << 8
                | bytes[offset + 3];
        }

        private static string ProjectAbsolutePath(string projectRelativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Could not resolve Unity project root.");
            return Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath));
        }

        private static string RelativePath(string root, string path)
        {
            return Path.GetRelativePath(
                    Path.GetFullPath(root),
                    Path.GetFullPath(path))
                .Replace('\\', '/');
        }

        private static bool IsOwnedSession()
        {
            return SessionState.GetBool(SessionActiveKey, false)
                && string.Equals(
                    SessionState.GetString(SessionOwnerKey, string.Empty),
                    SessionOwnerValue,
                    StringComparison.Ordinal);
        }

        private static void ClearSession()
        {
            SessionState.EraseBool(SessionActiveKey);
            SessionState.EraseBool(SessionBatchKey);
            SessionState.EraseString(SessionStatePathKey);
            SessionState.EraseString(SessionOwnerKey);
            activeBehaviour = null;
        }

        private static RunnerPhase ParsePhase(string value)
        {
            if (!Enum.TryParse(value, ignoreCase: false, out RunnerPhase phase))
            {
                throw new InvalidDataException(
                    "City runner state contains an unknown phase: " + value);
            }

            return phase;
        }

        private static PersistedRunnerState LoadState(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException(
                    "City SessionState does not point to a persisted runner state.",
                    path);
            }

            PersistedRunnerState state =
                JsonUtility.FromJson<PersistedRunnerState>(File.ReadAllText(path));
            if (state == null
                || !string.Equals(state.schema, RunnerSchema, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(state.outputDirectory)
                || string.IsNullOrWhiteSpace(state.captureId))
            {
                throw new InvalidDataException("City persisted runner state is invalid.");
            }

            state.dependencyPaths ??= Array.Empty<string>();
            state.dependencyHashesAtStart ??= Array.Empty<AuditionPvDependencyHash>();
            state.recorderProofs ??= Array.Empty<ShotRecorderProof>();
            state.runtimeProofs ??= Array.Empty<AuditionPvCityHeroPocketRuntimeProof>();
            return state;
        }

        private static void SaveState(string path, PersistedRunnerState state)
        {
            string normalizedPath = Path.GetFullPath(path);
            string parent = Path.GetDirectoryName(normalizedPath)
                ?? throw new InvalidOperationException(
                    "City state path has no parent.");
            Directory.CreateDirectory(parent);
            string temporary = normalizedPath + ".tmp-" + Guid.NewGuid().ToString("N");
            File.WriteAllText(
                temporary,
                JsonUtility.ToJson(state, true) + Environment.NewLine,
                new UTF8Encoding(false));
            try
            {
                if (File.Exists(normalizedPath))
                {
                    File.Replace(temporary, normalizedPath, null);
                }
                else
                {
                    File.Move(temporary, normalizedPath);
                }
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        }

        private static void WriteJsonNew<T>(string path, T value)
        {
            string parent = Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException(
                    "City JSON artifact has no parent.");
            Directory.CreateDirectory(parent);
            using var stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(JsonUtility.ToJson(value, true));
            writer.WriteLine();
            writer.Flush();
            stream.Flush(flushToDisk: true);
        }

        private static void TryWriteFailureArtifact(
            string outputDirectory,
            string phase,
            Exception exception,
            ShotRecorderProof[] recorderProofs,
            AuditionPvCityHeroPocketRuntimeProof[] runtimeProofs)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    return;
                }

                Directory.CreateDirectory(outputDirectory);
                string path = Path.Combine(outputDirectory, FailureFileName);
                if (File.Exists(path))
                {
                    path = Path.Combine(
                        outputDirectory,
                        "city_g01_g03_capture_failure_"
                        + DateTime.UtcNow.ToString(
                            "yyyyMMddTHHmmssfffZ",
                            CultureInfo.InvariantCulture)
                        + ".json");
                }

                WriteJsonNew(path, new FailureArtifact
                {
                    schema = FailureSchema,
                    createdAtUtc = DateTime.UtcNow.ToString("O"),
                    phase = phase ?? string.Empty,
                    exception = exception?.ToString() ?? "unknown failure",
                    recorder = recorderProofs,
                    runtime = runtimeProofs
                });
            }
            catch (Exception artifactFailure)
            {
                Debug.LogException(artifactFailure);
            }
        }

        private static AuditionPvGitSnapshot CreateGitSnapshot(
            PersistedRunnerState state)
        {
            return new AuditionPvGitSnapshot
            {
                commitSha = state.gitCommitSha,
                branch = state.gitBranch,
                isDirty = state.gitWorktreeDirty,
                dirtyStateHashSha256 = state.gitDirtyHashSha256,
                probeSucceeded = true
            };
        }

        private static PersistedEngineSnapshot CopyEngine(
            AuditionPvEngineSnapshot engine)
        {
            return new PersistedEngineSnapshot
            {
                unityVersion = engine.unityVersion,
                unityVersionWithRevision = engine.unityVersionWithRevision,
                recorderPackageVersion = engine.recorderPackageVersion,
                urpPackageVersion = engine.urpPackageVersion,
                activeRenderPipelineAssetPath = engine.activeRenderPipelineAssetPath
            };
        }

        private static AuditionPvEngineSnapshot RestoreEngine(
            PersistedEngineSnapshot engine)
        {
            if (engine == null)
            {
                throw new InvalidDataException(
                    "City engine provenance is missing.");
            }

            return new AuditionPvEngineSnapshot
            {
                unityVersion = engine.unityVersion,
                unityVersionWithRevision = engine.unityVersionWithRevision,
                recorderPackageVersion = engine.recorderPackageVersion,
                urpPackageVersion = engine.urpPackageVersion,
                activeRenderPipelineAssetPath = engine.activeRenderPipelineAssetPath
            };
        }

        private enum RunnerPhase
        {
            AwaitingPlayMode,
            Recording,
            AwaitingEditMode,
            FailedInPlayMode,
            Complete
        }

        internal readonly struct SixtySecondEvidenceRange
        {
            public readonly AuditionPvCityShot shot;
            public readonly int sourceStartFrame;
            public readonly int sourceEndFrame;
            public readonly int selectStartFrame;
            public readonly int selectEndFrame;

            public SixtySecondEvidenceRange(
                AuditionPvCityShot shot,
                int sourceStartFrame,
                int sourceEndFrame,
                int selectStartFrame,
                int selectEndFrame)
            {
                this.shot = shot;
                this.sourceStartFrame = sourceStartFrame;
                this.sourceEndFrame = sourceEndFrame;
                this.selectStartFrame = selectStartFrame;
                this.selectEndFrame = selectEndFrame;
            }
        }

        [Serializable]
        internal sealed class PersistedRunnerState
        {
            public string schema = string.Empty;
            public string phase = string.Empty;
            public bool batchMode;
            public bool produceApprovedSixtySecondEvidence;
            public string startedAtUtc = string.Empty;
            public string captureId = string.Empty;
            public string outputRoot = string.Empty;
            public string outputDirectory = string.Empty;
            public string baselineDirectory = string.Empty;
            public string gitCommitSha = string.Empty;
            public string gitBranch = string.Empty;
            public bool gitWorktreeDirty;
            public string gitDirtyHashSha256 = string.Empty;
            public PersistedEngineSnapshot engine = new();
            public string[] dependencyPaths = Array.Empty<string>();
            public AuditionPvDependencyHash[] dependencyHashesAtStart =
                Array.Empty<AuditionPvDependencyHash>();
            public string citySceneSha256AtStart = string.Empty;
            public ShotRecorderProof[] recorderProofs =
                Array.Empty<ShotRecorderProof>();
            public AuditionPvCityHeroPocketRuntimeProof[] runtimeProofs =
                Array.Empty<AuditionPvCityHeroPocketRuntimeProof>();
            public string failure = string.Empty;
        }

        [Serializable]
        internal sealed class PersistedEngineSnapshot
        {
            public string unityVersion = string.Empty;
            public string unityVersionWithRevision = string.Empty;
            public string recorderPackageVersion = string.Empty;
            public string urpPackageVersion = string.Empty;
            public string activeRenderPipelineAssetPath = string.Empty;
        }

        [Serializable]
        internal sealed class ShotRecorderProof
        {
            public string shotId = string.Empty;
            public string runtimeWorkloadSealPath = string.Empty;
            public int expectedRawFrameCount;
            public int recorderWarmupEndOfFrameCount;
            public int canonicalSourceFrameCount;
            public int logicalFirstSourceFrame;
            public int logicalLastSourceFrame;
            public int recordedPreHandleFrameCount;
            public int recordedPostHandleFrameCount;
            public bool recorderPaddingActiveAtLogicalFrameZero;
            public float recorderCaptureDeltaTimeAtLogicalFrameZero;
            public bool recorderAutoStoppedAfterLastFrame;
            public int presentedFrameCount;
            public bool presentedFramesExact = true;
            public bool presentationClockExact = true;
            public bool directorStateRestored;
            public int directorRestoreCallCountBeforeNextShot;
            public int directorDestroyCallCountBeforeNextShot;
            public int directorRestoreCallCountAtSequenceEnd;
            public int directorDestroyCallCountAtSequenceEnd;
        }

        [Serializable]
        internal sealed class SequenceVisualMetrics
        {
            public string shotId = string.Empty;
            public long sampleCount;
            public long blackSampleCount;
            public long magentaSampleCount;
            public int healthyFrameCount;
            public int magentaAffectedFrameCount;
            public double blackRatio;
            public double magentaRatio;
            public double maximumFrameMagentaRatio;
            public int minimumSampledLuma = 255;
            public int maximumSampledLuma;
            public int frameZeroHudAccentSamples;
            public HudProbeMetrics[] hudProbes = Array.Empty<HudProbeMetrics>();
            public ScreenDeltaMetrics primaryDelta;
            public ScreenDeltaMetrics dodgeDelta;
            public int fullCoverFrameCount;
            public int minimumFullCoverSampledLuma = 255;
            public int maximumFullCoverSpatialChannelRange;
            public bool fullCoverDecodedPixelHashesExact;
        }

        [Serializable]
        internal sealed class HudProbeMetrics
        {
            public int frame;
            public int cyanAccentSamples;
        }

        [Serializable]
        internal sealed class ScreenDeltaMetrics
        {
            public long sampleCount;
            public long changedSampleCount;
            public double meanAbsoluteRgb;
            public double changedSampleRatio;
        }

        [Serializable]
        internal sealed class FrameHashEntry
        {
            public string id = string.Empty;
            public string relativePath = string.Empty;
            public long byteLength;
            public string sha256 = string.Empty;
        }

        private sealed class CitySemanticBeatSpec
        {
            public readonly string beatId;
            public readonly int logicalStartFrame;
            public readonly int logicalEndFrame;
            public readonly string[] exactFacts;

            public CitySemanticBeatSpec(
                string newBeatId,
                int newLogicalStartFrame,
                int newLogicalEndFrame,
                string[] newExactFacts)
            {
                beatId = newBeatId;
                logicalStartFrame = newLogicalStartFrame;
                logicalEndFrame = newLogicalEndFrame;
                exactFacts = newExactFacts ?? Array.Empty<string>();
            }
        }

        [Serializable]
        private sealed class CitySemanticBeatRuntimeArtifact
        {
            public string schemaVersion = string.Empty;
            public string sourceCaptureCoreSha256 = string.Empty;
            public string captureId = string.Empty;
            public string sourceShotId = string.Empty;
            public string beatId = string.Empty;
            public string runtimeFactKey = string.Empty;
            public int sourceRangeStartFrame;
            public int sourceRangeEndFrame;
            public int sourceFactStartFrame;
            public int sourceFactEndFrame;
            public string[] exactFacts = Array.Empty<string>();
            public AuditionPvPinnedArtifact runtimeProof = new();
            public AuditionPvPinnedArtifact sourceFrameLedger = new();
            public string producer = string.Empty;
            public string createdAtUtc = string.Empty;
        }

        [Serializable]
        private sealed class RuntimeProofArtifact
        {
            public string schema = string.Empty;
            public string captureId = string.Empty;
            public string mapping = string.Empty;
            public string gameplay = string.Empty;
            public ShotRecorderProof[] recorder = Array.Empty<ShotRecorderProof>();
            public AuditionPvCityHeroPocketRuntimeProof[] runtime =
                Array.Empty<AuditionPvCityHeroPocketRuntimeProof>();
            public SequenceVisualMetrics[] visual =
                Array.Empty<SequenceVisualMetrics>();
            public string frameHashArtifactPath = string.Empty;
            public string frameHashArtifactSha256 = string.Empty;
        }

        [Serializable]
        private sealed class FailureArtifact
        {
            public string schema = string.Empty;
            public string createdAtUtc = string.Empty;
            public string phase = string.Empty;
            public string exception = string.Empty;
            public ShotRecorderProof[] recorder;
            public AuditionPvCityHeroPocketRuntimeProof[] runtime;
        }
    }

    /// <summary>
    /// Runs before the product director so each logical f0 is armed in early
    /// Update after Recorder completes its two resolution warm-up end frames.
    /// </summary>
    [DefaultExecutionOrder(-32600)]
    public sealed class AuditionPvCityHeroPocketGoldenRunnerBehaviour : MonoBehaviour
    {
        private const double ShotTimeoutSeconds = 180d;
        private static readonly WaitForEndOfFrame EndOfFrameYield = new();

        private string statePath;
        private string outputDirectory;
        private AuditionPvCityHeroPocketGoldenRunner.PersistedRunnerState state;
        private readonly List<AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof>
            recorderProofs = new(3);
        private readonly List<AuditionPvCityHeroPocketRuntimeProof> runtimeProofs =
            new(3);
        private AuditionPvCityHeroPocketDirector director;
        private AuditionPvRecorderSettingsBundle recorderSettings;
        private RecorderController recorderController;
        private AuditionPvRuntimeWorkloadCaptureSession runtimeWorkloadCapture;
        private AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof activeProof;
        private AuditionPvCityShot activeShot;
        private Exception updateFailure;
        private bool armLogicalFrameZero;
        private bool beganLogicalShot;
        private bool notified;
        private bool recorderCleaned;
        private bool finalizingSequence;
        private readonly AuditionPvRecordedPostHandleTimeFreeze
            recordedPostHandleTimeFreeze = new();
        private int directorRestoreCallCount;
        private int directorDestroyCallCount;
        private int nextPresentedFrame;

        internal void Begin(
            string persistedStatePath,
            string captureOutputDirectory,
            AuditionPvCityHeroPocketGoldenRunner.PersistedRunnerState persistedState)
        {
            statePath = persistedStatePath;
            outputDirectory = captureOutputDirectory;
            state = persistedState;
            recorderProofs.AddRange(
                persistedState.recorderProofs
                ?? Array.Empty<AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof>());
            runtimeProofs.AddRange(
                persistedState.runtimeProofs
                ?? Array.Empty<AuditionPvCityHeroPocketRuntimeProof>());
            StartCoroutine(RunGuarded());
        }

        private void Update()
        {
            if (updateFailure != null)
            {
                return;
            }

            try
            {
                if (recordedPostHandleTimeFreeze.IsOwned)
                {
                    recordedPostHandleTimeFreeze.AssertHeld();
                }
                if (!armLogicalFrameZero || beganLogicalShot)
                {
                    return;
                }

                armLogicalFrameZero = false;
                if (Time.timeScale <= 0f)
                {
                    throw new InvalidOperationException(
                        "Recorder did not restore gameplay time before City logical f0.");
                }

                float minimumDelta = 1f / AuditionPvCaptureContract.Fps;
                activeProof.recorderCaptureDeltaTimeAtLogicalFrameZero =
                    Time.captureDeltaTime;
                activeProof.recorderPaddingActiveAtLogicalFrameZero =
                    Time.captureDeltaTime >= minimumDelta
                    && Time.captureDeltaTime < minimumDelta + 0.001f;
                if (!activeProof.recorderPaddingActiveAtLogicalFrameZero)
                {
                    throw new InvalidOperationException(
                        "Recorder cadence padding was not active at City logical f0: "
                        + Time.captureDeltaTime.ToString(
                            "F9",
                            CultureInfo.InvariantCulture));
                }

                director.BeginShotForRecorder(
                    activeProof.recorderWarmupEndOfFrameCount);
                beganLogicalShot = true;
            }
            catch (Exception exception)
            {
                updateFailure = exception;
            }
        }

        private IEnumerator RunGuarded()
        {
            Exception failure = null;
            IEnumerator core = RunCore();
            while (failure == null)
            {
                bool moved;
                object yielded;
                try
                {
                    moved = core.MoveNext();
                    yielded = moved ? core.Current : null;
                }
                catch (Exception exception)
                {
                    failure = exception;
                    break;
                }

                if (!moved)
                {
                    break;
                }

                yield return yielded;
            }

            Exception cleanupFailure = FinalizeSequence();
            failure ??= cleanupFailure;
            NotifyFinished(failure);
        }

        private IEnumerator RunCore()
        {
            director = AuditionPvCityHeroPocketCapture.AttachToFreshActiveScene(
                AuditionPvCityShot.G01);
            director.FramePresented += HandleFramePresented;

            IEnumerator freshPreparation = director.PrepareFreshProductState();
            while (freshPreparation.MoveNext())
            {
                yield return freshPreparation.Current;
            }

            if (!director.IsPrepared || director.Shot != AuditionPvCityShot.G01)
            {
                throw new InvalidOperationException(
                    "The same-session City director did not prepare G01.");
            }

            for (int index = 0;
                index < AuditionPvCityHeroPocketGoldenRunner.ShotOrder.Length;
                index++)
            {
                AuditionPvCityShot shot =
                    AuditionPvCityHeroPocketGoldenRunner.ShotOrder[index];
                if (index > 0)
                {
                    AuditionPvCityShot previous =
                        AuditionPvCityHeroPocketGoldenRunner.ShotOrder[index - 1];
                    ReleaseRecordedPostHandleFreeze();
                    MarkNoDirectorCleanupBeforeContinuation(previous);
                    IEnumerator continuation = director.PrepareContinuationShot(shot);
                    while (true)
                    {
                        bool moved;
                        object yielded;
                        try
                        {
                            moved = continuation.MoveNext();
                            yielded = moved ? continuation.Current : null;
                        }
                        catch (Exception continuationFailure)
                        {
                            try
                            {
                                CaptureSealedRuntimeProof(previous);
                            }
                            catch (Exception proofFailure)
                            {
                                throw new AggregateException(
                                    "City continuation failed and its prior sealed "
                                    + "runtime proof could not be preserved.",
                                    continuationFailure,
                                    proofFailure);
                            }

                            throw new InvalidOperationException(
                                "City continuation failed after preserving the exact "
                                + AuditionPvCityHeroPocketGoldenRunner.ShotId(previous)
                                + " sealed runtime proof.",
                                continuationFailure);
                        }

                        if (!moved)
                        {
                            break;
                        }

                        yield return yielded;
                    }

                    CaptureSealedRuntimeProof(previous);
                }

                ResetActiveShot(shot);
                IEnumerator recorderShot = RecordPreparedShot(shot);
                while (recorderShot.MoveNext())
                {
                    yield return recorderShot.Current;
                }
            }
        }

        private IEnumerator RecordPreparedShot(AuditionPvCityShot shot)
        {
            if (director == null
                || !director.IsPrepared
                || director.Shot != shot)
            {
                throw new InvalidOperationException(
                    "The same-session City director is not prepared for "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
            }

            runtimeWorkloadCapture = AuditionPvRuntimeWorkloadCaptureSession.Open(
                new AuditionPvRuntimeWorkloadCaptureConfig
                {
                    captureId = state.captureId,
                    captureOutputDirectory = outputDirectory,
                    sourceShotId =
                        AuditionPvCityHeroPocketGoldenRunner.ShotId(shot),
                    sourceRangeStartFrame =
                        AuditionPvCityHeroPocketCapture.GetSourceFirstFrame(shot),
                    sourceRangeEndFrame =
                        AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot),
                    captureHudEvidence = false
                });

            recorderSettings = AuditionPvCityHeroPocketCapture
                .CreateRecorderSettingsForExistingOutput(outputDirectory, shot);
            recorderSettings.controllerSettings.SetRecordModeToFrameInterval(
                AuditionPvCityHeroPocketGoldenRunner.RawWarmupFrame,
                AuditionPvCityHeroPocketGoldenRunner.RawLastFrame(shot));
            AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
            if (shot == AuditionPvCityShot.G02)
            {
                director.ArmG02RecorderWarmupSuspension();
            }
            recorderController = new RecorderController(
                recorderSettings.controllerSettings);
            recorderController.PrepareRecording();
            if (!recorderController.StartRecording())
            {
                throw new InvalidOperationException(
                    "Unity Recorder 5.1.6 rejected City "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
            }

            yield return new WaitForEndOfFrame();
            activeProof.recorderWarmupEndOfFrameCount = 1;
            yield return new WaitForEndOfFrame();
            activeProof.recorderWarmupEndOfFrameCount = 2;
            for (int handleFrame = 0;
                handleFrame < AuditionPvCityHeroPocketCapture.HandleFrameCount;
                handleFrame++)
            {
                yield return new WaitForEndOfFrame();
                runtimeWorkloadCapture.CapturePresentedFrame(
                    AuditionPvCityHeroPocketCapture.GetSourceFirstFrame(shot)
                    + handleFrame);
                activeProof.recordedPreHandleFrameCount++;
            }

            activeProof.canonicalSourceFrameCount =
                AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot);
            activeProof.logicalFirstSourceFrame =
                AuditionPvCityHeroPocketCapture.GetSelectStartFrame(shot);
            activeProof.logicalLastSourceFrame =
                AuditionPvCityHeroPocketCapture.GetSelectEndFrame(shot);
            if (!recorderController.IsRecording()
                || director.IsRunning
                || director.IsComplete)
            {
                throw new InvalidOperationException(
                    "City did not record the complete prehandle before logical f0 for "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
            }
            armLogicalFrameZero = true;

            double deadline = Time.realtimeSinceStartupAsDouble + ShotTimeoutSeconds;
            while (!beganLogicalShot
                && updateFailure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            if (updateFailure != null)
            {
                throw new InvalidOperationException(
                    "City logical f0 arm failed.",
                    updateFailure);
            }

            if (!beganLogicalShot)
            {
                throw new TimeoutException("City logical f0 arm timed out.");
            }

            while (!director.IsComplete
                && director.Failure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                // Resume after the terminal rail LateUpdate so the posthandle can
                // freeze before the next frame's FixedUpdate or product Update.
                yield return EndOfFrameYield;
            }

            if (director.Failure != null)
            {
                throw new InvalidOperationException(
                    "City product director failed for "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".",
                    director.Failure);
            }

            if (!director.IsComplete)
            {
                throw new TimeoutException(
                    "City product director did not complete "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
            }

            AcquireRecordedPostHandleFreeze();
            if (updateFailure != null)
            {
                throw new InvalidOperationException(
                    "City recorded posthandle freeze failed.",
                    updateFailure);
            }
            if (!recordedPostHandleTimeFreeze.IsOwned)
            {
                throw new InvalidOperationException(
                    "City terminal product state was not frozen before the recorded "
                    + "posthandle began.");
            }

            // Keep the completed product/camera state alive while Recorder writes
            // the full 180-frame posthandle. Cleanup and same-session continuation
            // cannot begin until the inclusive raw interval auto-stops.
            int nextPostHandleSourceFrame =
                AuditionPvCityHeroPocketCapture.GetSelectEndFrame(shot) + 1;
            int lastSourceFrame =
                AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot);
            for (;
                nextPostHandleSourceFrame <= lastSourceFrame;
                nextPostHandleSourceFrame++)
            {
                if (updateFailure != null)
                {
                    throw new InvalidOperationException(
                        "City recorded posthandle freeze was lost.",
                        updateFailure);
                }
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                {
                    throw new TimeoutException(
                        "City timed out while recording the canonical posthandle for "
                        + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
                }
                if (!recorderController.IsRecording())
                {
                    throw new InvalidOperationException(
                        "Recorder stopped before the canonical City posthandle was complete for "
                        + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
                }

                yield return EndOfFrameYield;
                runtimeWorkloadCapture.CapturePresentedFrame(
                    nextPostHandleSourceFrame);
            }

            // Give Recorder one Update boundary to publish its inclusive interval stop.
            yield return null;

            if (updateFailure != null)
            {
                throw new InvalidOperationException(
                    "City recorded posthandle freeze was lost.",
                    updateFailure);
            }
            activeProof.recorderAutoStoppedAfterLastFrame =
                !recorderController.IsRecording();
            if (!activeProof.recorderAutoStoppedAfterLastFrame)
            {
                throw new InvalidOperationException(
                    "Recorder did not auto-stop after City "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
            }
            activeProof.recordedPostHandleFrameCount =
                AuditionPvCityHeroPocketCapture.HandleFrameCount;
            if (nextPostHandleSourceFrame != lastSourceFrame + 1)
            {
                throw new InvalidOperationException(
                    "City runtime workload did not observe every recorded posthandle "
                    + "frame for "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(shot) + ".");
            }
            activeProof.runtimeWorkloadSealPath =
                runtimeWorkloadCapture.Complete();
            runtimeWorkloadCapture = null;

            Exception cleanupFailure = CleanupRecorderSession();
            if (cleanupFailure != null)
            {
                throw cleanupFailure;
            }

            yield return null;
        }

        private void ResetActiveShot(AuditionPvCityShot shot)
        {
            if (recordedPostHandleTimeFreeze.IsOwned)
            {
                throw new InvalidOperationException(
                    "A City shot cannot reset while the recorded posthandle freeze "
                    + "is owned.");
            }

            activeProof = new AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof
            {
                shotId = AuditionPvCityHeroPocketGoldenRunner.ShotId(shot),
                expectedRawFrameCount =
                    AuditionPvCityHeroPocketGoldenRunner.ExpectedRawFrameCount(shot),
                presentedFramesExact = true,
                presentationClockExact = true
            };
            activeShot = shot;
            updateFailure = null;
            armLogicalFrameZero = false;
            beganLogicalShot = false;
            recorderCleaned = false;
            nextPresentedFrame = 0;
        }

        private void HandleFramePresented(int frameIndex)
        {
            runtimeWorkloadCapture?.CapturePresentedFrame(
                AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                    activeShot,
                    frameIndex));
            activeProof.presentedFramesExact &= frameIndex == nextPresentedFrame;
            activeProof.presentationClockExact &= PresentationClock.IsManuallyDriven
                && Mathf.Abs(
                    PresentationClock.UnscaledTime
                    - frameIndex / (float)AuditionPvCaptureContract.Fps) <= 0.00001f
                && Mathf.Abs(
                    PresentationClock.UnscaledDeltaTime
                    - 1f / AuditionPvCaptureContract.Fps) <= 0.00001f;
            activeProof.presentedFrameCount++;
            nextPresentedFrame++;
        }

        private void AcquireRecordedPostHandleFreeze()
        {
            if (recordedPostHandleTimeFreeze.IsOwned)
            {
                recordedPostHandleTimeFreeze.AssertHeld();
                return;
            }
            if (director == null || !director.IsComplete)
            {
                throw new InvalidOperationException(
                    "The City recorded posthandle freeze requires a completed "
                    + "product shot.");
            }
            if (recorderController == null || !recorderController.IsRecording())
            {
                throw new InvalidOperationException(
                    "Recorder stopped before the City terminal posthandle freeze "
                    + "could be acquired.");
            }

            // The logical wait resumes at EndOfFrame after the rail seals its final
            // LateUpdate. Acquiring here prevents any next-frame physics, reload,
            // encounter, camera, or HUD product tick.
            recordedPostHandleTimeFreeze.Acquire();
        }

        private void ReleaseRecordedPostHandleFreeze()
        {
            recordedPostHandleTimeFreeze.Release();
        }

        private Exception CleanupRecorderSession()
        {
            if (recorderCleaned)
            {
                return null;
            }

            recorderCleaned = true;
            Exception firstFailure = null;
            CaptureFailure(ref firstFailure, () =>
            {
                recorderController?.StopRecording();
                recorderController = null;
            });
            CaptureFailure(ref firstFailure, () =>
            {
                if (activeProof != null && !recorderProofs.Contains(activeProof))
                {
                    recorderProofs.Add(activeProof);
                }
            });
            CaptureFailure(ref firstFailure, () =>
            {
                runtimeWorkloadCapture?.Dispose();
                runtimeWorkloadCapture = null;
            });
            CaptureFailure(ref firstFailure, () =>
            {
                recorderSettings?.Dispose();
                recorderSettings = null;
            });
            return firstFailure;
        }

        private void MarkNoDirectorCleanupBeforeContinuation(
            AuditionPvCityShot previousShot)
        {
            AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof recorder =
                recorderProofs.SingleOrDefault(value => value != null
                    && string.Equals(
                        value.shotId,
                        AuditionPvCityHeroPocketGoldenRunner.ShotId(previousShot),
                        StringComparison.Ordinal));
            if (recorder == null)
            {
                throw new InvalidOperationException(
                    "City continuation is missing prior Recorder proof for "
                    + AuditionPvCityHeroPocketGoldenRunner.ShotId(previousShot) + ".");
            }

            recorder.directorRestoreCallCountBeforeNextShot =
                directorRestoreCallCount;
            recorder.directorDestroyCallCountBeforeNextShot =
                directorDestroyCallCount;
            if (recorder.directorRestoreCallCountBeforeNextShot != 0
                || recorder.directorDestroyCallCountBeforeNextShot != 0)
            {
                throw new InvalidOperationException(
                    "City continuation observed a premature director restore or destroy.");
            }
        }

        private void CaptureSealedRuntimeProof(AuditionPvCityShot sealedShot)
        {
            AuditionPvCityHeroPocketRuntimeProof proof =
                director?.LastSealedRuntimeProof
                ?? throw new InvalidOperationException(
                    "City continuation did not publish a sealed runtime proof.");
            if (!string.Equals(
                    proof.shotId,
                    AuditionPvCityHeroPocketGoldenRunner.ShotId(sealedShot),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "City continuation sealed the wrong shot proof.");
            }

            AuditionPvCityHeroPocketGoldenRunner.AppendValidatedSealedRuntimeProof(
                runtimeProofs,
                proof,
                sealedShot,
                AuditionPvCityHeroPocketCapture.ValidateRuntimeProof);
            AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof recorder =
                recorderProofs.Single(value => string.Equals(
                    value.shotId,
                    proof.shotId,
                    StringComparison.Ordinal));
            recorder.directorStateRestored = proof.stateRestored;
            AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(recorder);
        }

        private Exception FinalizeSequence()
        {
            if (finalizingSequence)
            {
                return null;
            }

            finalizingSequence = true;
            bool directorStateRestoredAtSequenceEnd = false;
            Exception firstFailure = null;
            CaptureFailure(ref firstFailure, ReleaseRecordedPostHandleFreeze);
            Exception recorderCleanupFailure = CleanupRecorderSession();
            firstFailure ??= recorderCleanupFailure;
            CaptureFailure(ref firstFailure, () =>
            {
                if (director == null)
                {
                    return;
                }

                directorRestoreCallCount++;
                director.RestoreShotState();
                directorStateRestoredAtSequenceEnd = director.StateRestored;
                AuditionPvCityHeroPocketRuntimeProof finalProof =
                    director.SnapshotRuntimeProof();
                AuditionPvCityHeroPocketCapture.ValidateRuntimeProof(finalProof);
                if (!runtimeProofs.Any(value => value != null
                    && string.Equals(
                        value.shotId,
                        finalProof.shotId,
                        StringComparison.Ordinal)))
                {
                    runtimeProofs.Add(finalProof);
                }
            });
            CaptureFailure(ref firstFailure, () =>
            {
                if (director != null)
                {
                    director.FramePresented -= HandleFramePresented;
                }
            });
            CaptureFailure(ref firstFailure, () =>
            {
                if (director != null)
                {
                    directorDestroyCallCount++;
                    UnityEngine.Object.Destroy(director.gameObject);
                    director = null;
                }
            });
            CaptureFailure(ref firstFailure, () =>
            {
                foreach (AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof value
                         in recorderProofs)
                {
                    value.directorStateRestored =
                        directorStateRestoredAtSequenceEnd;
                    value.directorRestoreCallCountAtSequenceEnd =
                        directorRestoreCallCount;
                    value.directorDestroyCallCountAtSequenceEnd =
                        directorDestroyCallCount;
                    AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(value);
                }

                AuditionPvCityHeroPocketGoldenRunner.ValidateFinalDirectorLifecycle(
                    recorderProofs.ToArray());
            });
            return firstFailure;
        }

        private void NotifyFinished(Exception failure)
        {
            if (notified)
            {
                return;
            }

            notified = true;
            AuditionPvCityHeroPocketGoldenRunner.NotifyPlayModeFinished(
                statePath,
                state,
                recorderProofs.ToArray(),
                runtimeProofs.ToArray(),
                failure);
        }

        private void OnDisable()
        {
            if (notified || !Application.isPlaying)
            {
                return;
            }

            Exception cleanupFailure = FinalizeSequence();
            NotifyFinished(
                cleanupFailure
                ?? new InvalidOperationException(
                    "City golden runner was disabled before finalization."));
        }

        private static void CaptureFailure(
            ref Exception firstFailure,
            Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception exception)
            {
                firstFailure ??= exception;
            }
        }
    }

    /// <summary>
    /// Owns the isolated capture runner's temporary global-time freeze. Recorder
    /// frame-interval capture and coroutines continue to advance while scaled
    /// gameplay, camera, and UI product ticks remain at the sealed terminal frame.
    /// </summary>
    internal sealed class AuditionPvRecordedPostHandleTimeFreeze
    {
        private float restoreTimeScale;

        internal bool IsOwned { get; private set; }

        internal void Acquire()
        {
            if (IsOwned)
            {
                AssertHeld();
                return;
            }
            if (Time.timeScale <= 0f)
            {
                throw new InvalidOperationException(
                    "The City recorded posthandle cannot acquire over paused time.");
            }

            restoreTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            IsOwned = true;
            try
            {
                AssertHeld();
            }
            catch
            {
                IsOwned = false;
                Time.timeScale = restoreTimeScale;
                restoreTimeScale = 0f;
                throw;
            }
        }

        internal void AssertHeld()
        {
            if (!IsOwned || !Mathf.Approximately(Time.timeScale, 0f))
            {
                throw new InvalidOperationException(
                    "The City recorded posthandle lost its global-time freeze.");
            }
        }

        internal void Release()
        {
            if (!IsOwned)
            {
                return;
            }

            float capturedTimeScale = restoreTimeScale;
            bool stillHeld = Mathf.Approximately(Time.timeScale, 0f);
            IsOwned = false;
            restoreTimeScale = 0f;
            Time.timeScale = capturedTimeScale;
            if (!stillHeld)
            {
                throw new InvalidOperationException(
                    "The City recorded posthandle freeze was externally replaced "
                    + "before release. The captured time scale was restored.");
            }
        }
    }
}
