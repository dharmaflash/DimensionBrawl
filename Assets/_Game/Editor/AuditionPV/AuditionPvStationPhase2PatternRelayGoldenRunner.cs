using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Recorder, evidence, provenance, and lifecycle owner for the independent
    /// G07 product-state capture. Gameplay mutations remain in the G07 director;
    /// this runner observes final rendered geometry after the product camera and
    /// publishes a manifest only after every other fallible gate has passed.
    /// </summary>
    [InitializeOnLoad]
    public static class AuditionPvStationPhase2PatternRelayGoldenRunner
    {
        internal const string RunnerScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationPhase2PatternRelayGoldenRunner.cs";
        internal const string RunnerTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvStationPhase2PatternRelayGoldenTests.cs";
        internal const string MenuPath =
            "DimensionBrawl/Audition PV/Capture G07 Station Phase 2 Pattern Relay Golden Source";
        internal const string StateFileName = "g07_runner_state.json";
        internal const string RuntimeProofFileName = "g07_runtime_proof.json";
        internal const string FrameHashLedgerFileName = "frame_hashes.sha256";
        internal const string GateShotAuthorshipFileName =
            "g07_shot_authorship.json";
        internal const string GateSemanticEvidenceFolderName =
            "semantic_beats";
        internal const string FailureFileName = "g07_capture_failure.json";
        internal const string EvidenceFolderName = "evidence";
        internal const string WarmupEvidenceFileName =
            "recorder_warmup_raw_frame_0000.png";
        internal const int RawWarmupFrame = 0;
        internal const int RawFirstShotFrame = 1;
        internal const int RawLastShotFrame =
            AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount;
        internal const int ExpectedRawFrameCount = RawLastShotFrame + 1;
        internal const int S070SourceRangeStartFrame = 0;
        internal const int S070SourceRangeEndFrame = 779;
        internal const int S070SelectStartFrame = 180;
        internal const int S070SelectEndFrame = 599;
        internal const string ExpectedUnityVersion = "6000.3.5f2";
        internal const string ExpectedUnityVersionWithRevision =
            "6000.3.5f2 (3fa8bc678cb0)";
        internal const string ExpectedUrpPackageVersion = "17.3.0";
        internal const string ExpectedRenderPipelineAssetPath =
            "Assets/Settings/PC_RPAsset.asset";
        internal const string ExpectedCurtainProfileGuid =
            "031a4022a43b0d94da2839f6c10ba846";
        internal const string ExpectedHoverProfileGuid =
            "d6ddf85506fc3a64593c1f3627179c8d";
        internal const string G06HudCalibrationManifestSha256 =
            "2f6d7ccf4a87b98055a6557674aa7c1487748e70093c20e15b2c9bac21b0053d";

        internal const double MaximumSequenceBlackRatio = 0.90d;
        internal const double MaximumSequenceMagentaRatio = 0.005d;
        internal const double MaximumFrameMagentaRatio = 0.02d;
        internal const int MinimumHealthyFramePercent = 90;
        internal const double MinimumWindupMeanAbsoluteRgb = 2d;
        internal const double MinimumWindupChangedRatio = 0.08d;
        internal const int MinimumHudPinkSamples = 540;
        internal const int MinimumHudDarkSamples = 210;
        internal const int MinimumHudBrightSamples = 830;
        internal const double MinimumHudMeanLuma = 140d;
        internal const double MaximumHudMeanLuma = 170d;
        // Locked from two independent clean 2ad27978 takes:
        // 20260816t013248z: Curtain green 72, Hover cyan 31, fire means
        // 4.853380/8.683188 and fire-over-quiet margins 3.888452/4.285788.
        // 20260816t014116z: Curtain green 80, Hover cyan 31, fire means
        // 4.891451/6.589592 and margins 3.911660/2.083720. The floors below
        // retain explicit headroom beneath every observed same-commit minimum.
        internal static readonly bool PatternPixelCalibrationLocked = true;
        internal const long MinimumCurtainGreenSamples = 60;
        internal const long MinimumHoverCyanSamples = 24;
        internal const double MinimumCurtainLocalizedFireMeanAbsoluteRgb = 4.4d;
        internal const double MinimumHoverLocalizedFireMeanAbsoluteRgb = 6d;
        internal const double MinimumCurtainFireOverQuietMeanMargin = 3.5d;
        internal const double MinimumHoverFireOverQuietMeanMargin = 1.8d;
        internal const int PatternWindupColorRoiPadding = 24;
        // Independent clean G06 QHD HUD-on capture (manifest SHA prefix
        // 2f6d7c) measured this raw-bottom ROI across all 360 frames:
        // pink 569..622, dark 227..228, bright 877..966, luma 149.1..159.7.
        // The published bounds below retain deliberate review margins.
        internal static readonly RectInt HudRawBottomLeftRoi =
            new(688, 8, 176, 176);
        internal static readonly RectInt PatternRawBottomLeftRoi =
            new(320, 480, 1920, 520);

        private const string SessionActiveKey =
            "DimensionBrawl.AuditionPV.G07GoldenRunner.Active";
        private const string SessionStatePathKey =
            "DimensionBrawl.AuditionPV.G07GoldenRunner.StatePath";
        private const string SessionOwnerKey =
            "DimensionBrawl.AuditionPV.G07GoldenRunner.Owner";
        private const string SessionBatchKey =
            "DimensionBrawl.AuditionPV.G07GoldenRunner.Batch";
        private const string SessionOutputDirectoryKey =
            "DimensionBrawl.AuditionPV.G07GoldenRunner.OutputDirectory";
        private const string SessionCaptureIdKey =
            "DimensionBrawl.AuditionPV.G07GoldenRunner.CaptureId";
        private const string SessionTerminalFaultKey =
            "DimensionBrawl.AuditionPV.G07GoldenRunner.TerminalFault";
        private const string SessionOwnerValue =
            "dimension-brawl.g07-station-phase2-pattern-relay.v1";
        private const string RunnerSchema =
            "dimension-brawl.audition-pv.g07-runner-state.v1";
        internal const string RuntimeProofSchema =
            "dimension-brawl.audition-pv.g07-runtime-proof.v1";
        private const string FailureSchema =
            "dimension-brawl.audition-pv.capture-failure.v1";

        private static bool resumeScheduled;
        private static bool finalizing;
        private static AuditionPvStationPhase2PatternRelayGoldenRunnerBehaviour
            activeBehaviour;

        static AuditionPvStationPhase2PatternRelayGoldenRunner()
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
                    "G07 golden capture did not start",
                    exception.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Headful asynchronous entry point. Use -executeMethod and -noaudio;
        /// -batchmode, -quit, and -nographics are rejected because the source is
        /// the rendered Game View.
        /// </summary>
        public static void RunBatchCapture()
        {
            try
            {
                ValidateBatchCommandLine(Environment.GetCommandLineArgs());
                BeginCapture(batchMode: true);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        internal static string RawFrameFileName(int rawFrameIndex)
        {
            if (rawFrameIndex < RawWarmupFrame
                || rawFrameIndex > RawLastShotFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(rawFrameIndex));
            }

            return $"frame_{rawFrameIndex:0000}.png";
        }

        internal static void ValidateBatchCommandLine(IEnumerable<string> arguments)
        {
            string[] args = (arguments ?? Array.Empty<string>()).ToArray();
            bool Has(string expected) => args.Any(value =>
                string.Equals(value, expected, StringComparison.OrdinalIgnoreCase));
            if (!Has("-noaudio"))
            {
                throw new InvalidOperationException(
                    "G07 RunBatchCapture requires -noaudio.");
            }

            if (Has("-batchmode") || Has("-quit") || Has("-nographics"))
            {
                throw new InvalidOperationException(
                    "G07 requires a headful asynchronous Editor: remove -batchmode, -quit, and -nographics.");
            }
        }

        internal static void ValidateExactEngineProvenance(
            string unityVersion,
            string unityVersionWithRevision,
            string recorderPackageVersion,
            string urpPackageVersion,
            string activeRenderPipelineAssetPath)
        {
            if (!string.Equals(
                    unityVersion,
                    ExpectedUnityVersion,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(unityVersionWithRevision)
                || !string.Equals(
                    unityVersionWithRevision,
                    ExpectedUnityVersionWithRevision,
                    StringComparison.Ordinal)
                || !string.Equals(
                    recorderPackageVersion,
                    AuditionPvCaptureContract.RecorderPackageVersion,
                    StringComparison.Ordinal)
                || !string.Equals(
                    urpPackageVersion,
                    ExpectedUrpPackageVersion,
                    StringComparison.Ordinal)
                || !string.Equals(
                    activeRenderPipelineAssetPath,
                    ExpectedRenderPipelineAssetPath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G07 requires the exact authored Unity, Recorder, URP, and render-pipeline provenance.");
            }
        }

        internal static void EnsureNoDirtyOpenScenes()
        {
            var dirty = new List<string>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.IsValid() && scene.isLoaded && scene.isDirty)
                {
                    dirty.Add(string.IsNullOrWhiteSpace(scene.path)
                        ? "<untitled:" + scene.name + ">"
                        : scene.path);
                }
            }

            if (dirty.Count > 0)
            {
                throw new InvalidOperationException(
                    "G07 refuses to replace dirty open scenes: "
                    + string.Join(", ", dirty));
            }
        }

        internal static string[] CollectCaptureDependencyPaths()
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void AddWithMeta(string path)
            {
                string normalized = path.Replace('\\', '/');
                dependencies.Add(normalized);
                string absolute = ProjectAbsolutePath(normalized);
                if (File.Exists(absolute + ".meta"))
                {
                    dependencies.Add(normalized + ".meta");
                }
            }

            AddWithMeta(RunnerScriptPath);
            AddWithMeta(RunnerTestPath);
            foreach (string explicitPath in
                     AuditionPvStationPhase2PatternRelayCapture
                         .ExplicitProductDependencyPaths())
            {
                if (AssetDatabase.LoadMainAssetAtPath(explicitPath) == null
                    && !File.Exists(ProjectAbsolutePath(explicitPath)))
                {
                    throw new FileNotFoundException(
                        "G07 explicit product dependency is missing.",
                        explicitPath);
                }

                AddWithMeta(explicitPath);
                foreach (string nested in AssetDatabase.GetDependencies(explicitPath, true))
                {
                    AddWithMeta(nested);
                }
            }

            return AuditionPvEnvironmentProbe.CollectCaptureDependencyPaths(
                dependencies);
        }

        internal static void ValidateStableGitSnapshot(
            AuditionPvGitSnapshot start,
            AuditionPvGitSnapshot end)
        {
            if (start == null
                || end == null
                || !start.probeSucceeded
                || !end.probeSucceeded
                || start.isDirty
                || end.isDirty
                || !string.Equals(start.commitSha, end.commitSha, StringComparison.Ordinal)
                || !string.Equals(start.branch, end.branch, StringComparison.Ordinal)
                || !string.Equals(
                    start.dirtyStateHashSha256,
                    end.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Git source state changed while G07 was recording.");
            }
        }

        internal static void ValidateStableDependencies(
            AuditionPvDependencyHash[] start,
            AuditionPvDependencyHash[] end)
        {
            AuditionPvDependencyHash[] initial = start
                ?? Array.Empty<AuditionPvDependencyHash>();
            AuditionPvDependencyHash[] current = end
                ?? Array.Empty<AuditionPvDependencyHash>();
            var currentByPath = current.ToDictionary(
                value => value.path,
                StringComparer.OrdinalIgnoreCase);
            if (initial.Length != current.Length)
            {
                throw new InvalidOperationException(
                    "G07 dependency set changed while recording.");
            }

            foreach (AuditionPvDependencyHash dependency in initial)
            {
                if (dependency == null
                    || !currentByPath.TryGetValue(
                        dependency.path,
                        out AuditionPvDependencyHash candidate)
                    || dependency.exists != candidate.exists
                    || dependency.byteLength != candidate.byteLength
                    || !string.Equals(
                        dependency.sha256,
                        candidate.sha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G07 dependency changed while recording: "
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
                throw new FileNotFoundException("Expected G07 PNG is missing.", path);
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length
                    || stream.Read(header, 0, header.Length) != header.Length)
                {
                    throw new InvalidDataException("G07 PNG is truncated: " + path);
                }
            }

            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (!signature.Select((value, index) => header[index] == value).All(value => value)
                || header[12] != (byte)'I'
                || header[13] != (byte)'H'
                || header[14] != (byte)'D'
                || header[15] != (byte)'R')
            {
                throw new InvalidDataException("G07 PNG signature/IHDR mismatch: " + path);
            }

            int width = ReadBigEndianInt32(header, 16);
            int height = ReadBigEndianInt32(header, 20);
            if (width != expectedWidth || height != expectedHeight)
            {
                throw new InvalidDataException(
                    $"G07 PNG is {width}x{height}; expected {expectedWidth}x{expectedHeight}: {path}");
            }
        }

        internal static void ValidateDecodablePngFile(
            string path,
            int expectedWidth,
            int expectedHeight)
        {
            ValidatePngFile(path, expectedWidth, expectedHeight);
            Texture2D texture = LoadPng(path, expectedWidth, expectedHeight);
            try
            {
                if (texture.width != expectedWidth
                    || texture.height != expectedHeight
                    || texture.GetPixels32().Length
                        != expectedWidth * expectedHeight)
                {
                    throw new InvalidDataException(
                        "G07 PNG decoded dimensions/pixels are not exact: " + path);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        internal static string RemapRawFrames(
            string frameDirectory,
            string evidenceDirectory)
        {
            string frames = RequireDirectory(frameDirectory);
            string evidence = Path.GetFullPath(evidenceDirectory);
            Directory.CreateDirectory(evidence);
            ValidateRawFrameSequence(frames);
            string staging = Path.Combine(
                Path.GetDirectoryName(frames)
                    ?? throw new InvalidOperationException("G07 frame directory has no parent."),
                ".g07-remap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            bool complete = false;
            try
            {
                for (int raw = RawFirstShotFrame; raw <= RawLastShotFrame; raw++)
                {
                    MoveNew(
                        Path.Combine(frames, RawFrameFileName(raw)),
                        Path.Combine(
                            staging,
                            AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                                raw - RawFirstShotFrame)));
                }

                string warmup = Path.Combine(evidence, WarmupEvidenceFileName);
                MoveNew(
                    Path.Combine(frames, RawFrameFileName(RawWarmupFrame)),
                    warmup);
                for (int sourceFrame =
                         AuditionPvStationPhase2PatternRelayCapture.FirstFrame;
                    sourceFrame <=
                         AuditionPvStationPhase2PatternRelayCapture.LastFrame;
                    sourceFrame++)
                {
                    string name =
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                            sourceFrame);
                    MoveNew(Path.Combine(staging, name), Path.Combine(frames, name));
                }

                Directory.Delete(staging, recursive: false);
                complete = true;
                return warmup.Replace('\\', '/');
            }
            finally
            {
                if (complete && Directory.Exists(staging))
                {
                    Directory.Delete(staging, recursive: false);
                }
            }
        }

        internal static void ValidateRawFrameSequence(string frameDirectory)
        {
            ValidateExactNamedSequence(
                frameDirectory,
                ExpectedRawFrameCount,
                RawFrameFileName);
        }

        internal static void ValidateLogicalFrameSequence(string frameDirectory)
        {
            ValidateExactNamedSequence(
                frameDirectory,
                AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName);
        }

        internal static FrameDeltaMetrics EvaluateFrameDelta(
            Color32[] before,
            Color32[] after,
            int width,
            int height,
            RectInt? roi = null,
            int sampleStride = 1)
        {
            if (before == null
                || after == null
                || before.Length != after.Length
                || before.Length != width * height
                || width <= 0
                || height <= 0
                || sampleStride <= 0)
            {
                throw new ArgumentException(
                    "G07 frame-delta buffers/dimensions/stride are invalid.");
            }

            RectInt area = roi ?? new RectInt(0, 0, width, height);
            ValidateRoi(area, width, height);
            long samples = 0;
            long changed = 0;
            double absoluteRgb = 0d;
            for (int y = area.yMin; y < area.yMax; y += sampleStride)
            {
                for (int x = area.xMin; x < area.xMax; x += sampleStride)
                {
                    int index = y * width + x;
                    Color32 left = before[index];
                    Color32 right = after[index];
                    int red = Math.Abs(left.r - right.r);
                    int green = Math.Abs(left.g - right.g);
                    int blue = Math.Abs(left.b - right.b);
                    absoluteRgb += (red + green + blue) / 3d;
                    if (Math.Max(red, Math.Max(green, blue)) >= 8)
                    {
                        changed++;
                    }

                    samples++;
                }
            }

            return new FrameDeltaMetrics
            {
                sampleCount = samples,
                changedSampleCount = changed,
                meanAbsoluteRgb = absoluteRgb / Math.Max(1L, samples),
                changedSampleRatio = changed / (double)Math.Max(1L, samples),
                roiX = area.x,
                roiY = area.y,
                roiWidth = area.width,
                roiHeight = area.height,
                sampleStride = sampleStride
            };
        }

        internal static PatternColorMetrics EvaluatePatternColors(
            Color32[] pixels,
            int width,
            int height,
            RectInt roi,
            int sampleStride)
        {
            if (pixels == null
                || pixels.Length != width * height
                || sampleStride <= 0)
            {
                throw new ArgumentException("G07 pattern-color buffer is invalid.");
            }

            ValidateRoi(roi, width, height);
            var metrics = new PatternColorMetrics
            {
                roiX = roi.x,
                roiY = roi.y,
                roiWidth = roi.width,
                roiHeight = roi.height,
                sampleStride = sampleStride
            };
            for (int y = roi.yMin; y < roi.yMax; y += sampleStride)
            {
                for (int x = roi.xMin; x < roi.xMax; x += sampleStride)
                {
                    Color32 pixel = pixels[y * width + x];
                    if (IsCurtainGreen(pixel))
                    {
                        metrics.curtainGreenSampleCount++;
                    }

                    if (IsHoverCyan(pixel))
                    {
                        metrics.hoverCyanSampleCount++;
                    }

                    metrics.sampleCount++;
                }
            }

            return metrics;
        }

        internal static bool IsCurtainGreen(Color32 pixel)
        {
            return pixel.g >= 170
                && pixel.g - pixel.r >= 24
                && pixel.g - pixel.b >= 8;
        }

        internal static bool IsHoverCyan(Color32 pixel)
        {
            return pixel.g >= 170
                && pixel.b >= 180
                && Math.Min(pixel.g, pixel.b) - pixel.r >= 45
                && Math.Abs(pixel.g - pixel.b) <= 50;
        }

        internal static HudVisualMetrics EvaluateHudRoi(
            Texture2D texture,
            RectInt roi,
            int sampleStride)
        {
            if (texture == null
                || !texture.isReadable
                || texture.width <= 0
                || texture.height <= 0
                || sampleStride <= 0)
            {
                throw new ArgumentException("G07 HUD texture is invalid.");
            }

            ValidateRoi(roi, texture.width, texture.height);
            var metrics = new HudVisualMetrics
            {
                minimumFramePinkSamples = int.MaxValue,
                minimumFrameDarkSamples = int.MaxValue,
                minimumFrameBrightSamples = int.MaxValue,
                minimumFrameMeanLuma = double.PositiveInfinity,
                maximumFrameMeanLuma = double.NegativeInfinity,
                roiX = roi.x,
                roiY = roi.y,
                roiWidth = roi.width,
                roiHeight = roi.height,
                sampleStride = sampleStride,
                frameCount = 1
            };
            EvaluateHudFrame(
                texture,
                roi,
                sampleStride,
                out int pink,
                out int dark,
                out int bright,
                out double meanLuma);
            metrics.minimumFramePinkSamples = pink;
            metrics.maximumFramePinkSamples = pink;
            metrics.minimumFrameDarkSamples = dark;
            metrics.maximumFrameDarkSamples = dark;
            metrics.minimumFrameBrightSamples = bright;
            metrics.maximumFrameBrightSamples = bright;
            metrics.minimumFrameMeanLuma = meanLuma;
            metrics.maximumFrameMeanLuma = meanLuma;
            return metrics;
        }

        internal static void ValidateVisualMetrics(SequenceVisualMetrics sequence)
        {
            int minimumHealthy =
                AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount
                * MinimumHealthyFramePercent / 100;
            if (sequence == null
                || sequence.sampleCount <= 0
                || sequence.blackRatio >= MaximumSequenceBlackRatio
                || sequence.healthyFrameCount < minimumHealthy
                || sequence.maximumSampledLuma - sequence.minimumSampledLuma < 32
                || sequence.magentaRatio >= MaximumSequenceMagentaRatio
                || sequence.maximumFrameMagentaRatio >= MaximumFrameMagentaRatio)
            {
                throw new InvalidOperationException(
                    "G07 black/flat/magenta sequence sanity failed.");
            }
        }

        internal static void ValidateHudMetrics(HudVisualMetrics metrics)
        {
            if (metrics == null
                || metrics.frameCount
                    != AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount
                || metrics.minimumFramePinkSamples < MinimumHudPinkSamples
                || metrics.minimumFrameDarkSamples < MinimumHudDarkSamples
                || metrics.minimumFrameBrightSamples < MinimumHudBrightSamples
                || metrics.minimumFrameMeanLuma < MinimumHudMeanLuma
                || metrics.maximumFrameMeanLuma > MaximumHudMeanLuma
                || metrics.roiX != HudRawBottomLeftRoi.x
                || metrics.roiY != HudRawBottomLeftRoi.y
                || metrics.roiWidth != HudRawBottomLeftRoi.width
                || metrics.roiHeight != HudRawBottomLeftRoi.height
                || metrics.sampleStride != 4)
            {
                throw new InvalidOperationException(
                    "G07 stable HUD portrait ROI gate failed.");
            }
        }

        internal static void ValidateWindupDelta(FrameDeltaMetrics metrics)
        {
            if (metrics == null
                || metrics.sampleCount <= 0
                || metrics.meanAbsoluteRgb < MinimumWindupMeanAbsoluteRgb
                || metrics.changedSampleRatio < MinimumWindupChangedRatio)
            {
                throw new InvalidOperationException(
                    "G07 authored windup pixel delta is missing or too small.");
            }
        }

        internal static void ValidateRuntimeProof(RuntimeProof proof)
        {
            ValidateRuntimeProofCore(proof, requirePixelCalibration: true);
        }

        internal static void ValidateRuntimeProofBeforePixelCalibration(RuntimeProof proof)
        {
            ValidateRuntimeProofCore(proof, requirePixelCalibration: false);
        }

        internal static int[] ExpectedPixelSampleSourceFrames()
        {
            int Source(int logicalFrame) =>
                AuditionPvStationPhase2PatternRelayCapture
                    .LogicalToSourceFrame(logicalFrame);
            return new[]
            {
                Source(9), Source(10), Source(66), Source(67), Source(68),
                Source(367), Source(368), Source(416), Source(417),
                Source(418), Source(419)
            };
        }

        private static void ValidateRuntimeProofCore(
            RuntimeProof proof,
            bool requirePixelCalibration)
        {
            // Canonical authored GUIDs are literal recovery identity. The live
            // finalization proof is populated from AssetDatabase and must match
            // these values; committed recovery never consults the future
            // workspace and therefore cannot invalidate an immutable take after
            // an unrelated asset edit.
            const string curtainGuid = ExpectedCurtainProfileGuid;
            const string hoverGuid = ExpectedHoverProfileGuid;
            if (proof == null
                || !proof.directorCompleted
                || proof.lastLogicalFrame
                    != AuditionPvStationPhase2PatternRelayCapture
                        .LogicalLastFrame
                || proof.presentedFrameCount
                    != AuditionPvStationPhase2PatternRelayCapture
                        .LogicalExpectedFrameCount
                || !proof.presentedFramesExact
                || !proof.presentationClockExact
                || proof.transitionCompletedEventCount != 1
                || proof.windupEventCount != 2
                || proof.waveEventCount != 2
                || proof.curtainWindupFrame != 10
                || proof.curtainFireFrame != 68
                || proof.curtainSpawnedCount != 7
                || !proof.curtainWasPriority
                || proof.hoverWindupFrame != 368
                || proof.hoverFireFrame != 418
                || proof.hoverSpawnedCount != 4
                || proof.hoverWasPriority
                || proof.hoverSequenceIndexAfterFire != 1
                || !string.Equals(proof.curtainWindupPatternId, "AkazaSummonCurtain", StringComparison.Ordinal)
                || !string.Equals(proof.curtainFirePatternId, "AkazaSummonCurtain", StringComparison.Ordinal)
                || !string.Equals(proof.hoverWindupPatternId, "AkazaHoverLance", StringComparison.Ordinal)
                || !string.Equals(proof.hoverFirePatternId, "AkazaHoverLance", StringComparison.Ordinal)
                || proof.emitterTickCount != 420
                || proof.minimumEmitterTimeScale != 1f
                || proof.maximumEmitterTimeScale != 1f
                || proof.runStartedCount != 2
                || proof.stopSettleCount != 2
                || proof.curtainMoveFirstAppliedFrame != 17
                || proof.curtainMoveLastAppliedFrame != 46
                || proof.curtainZeroAppliedFrame != 47
                || proof.hoverMoveFirstAppliedFrame != 374
                || proof.hoverMoveLastAppliedFrame != 406
                || proof.hoverZeroAppliedFrame != 407
                || proof.curtainRiskBefore - proof.curtainRiskAfter
                    < AuditionPvStationPhase2PatternRelayCapture.MinimumCurtainRiskDecrease
                || !proof.stayedInsideForwardBoundary
                || proof.hoverPreviewCount != 4
                || proof.hoverLateralDisplacement < 1.5f
                || proof.hoverDirectionDot <= 0.98f
                || proof.visualWindupDelta != 2
                || proof.visualReleaseDelta != 2
                || proof.telegraphWindupDelta != 2
                || proof.telegraphReleaseDelta != 2
                || proof.cameraWindupDelta != 2
                || proof.cameraFireDelta != 2
                || proof.motionReleaseDelta != 2
                || proof.curtainWindupVisibleMarkerCount != 7
                || proof.curtainFireVisibleMarkerCount != 7
                || proof.hoverWindupVisibleMarkerCount != 4
                || proof.hoverFireVisibleMarkerCount != 4
                || proof.curtainWindupVisibleRendererCount != 7
                || proof.curtainFireVisibleRendererCount != 7
                || proof.hoverWindupVisibleRendererCount != 4
                || proof.hoverFireVisibleRendererCount != 4
                || !proof.telegraphMarkerCollidersNonBlocking
                || proof.basicVolleyEventCount != 0
                || proof.pressureActionEventCount != 0
                || proof.enemySummonReleaseCountDelta != 0
                || proof.playerDamageEventCount != 0
                || proof.bossDamageEventCount != 0
                || proof.playerBasicStartedCount != 0
                || proof.playerBasicHitCount != 0
                || proof.dodgeStartedCount != 0
                || proof.dodgeEndedCount != 0
                || proof.perfectDodgeCount != 0
                || proof.summonUsedCount != 0
                || proof.summonBlockedCount != 0
                || proof.summonUseBlockedCount != 0
                || !proof.playerHealthUnchanged
                || !proof.bossHealthUnchanged
                || !proof.resourcesUnchanged
                || !proof.exactHudAndBindings
                || !proof.exactProjectileAndVfxBindings
                || proof.lifecycleEmergencyResetUsed
                || !string.IsNullOrEmpty(proof.cleanupFailure)
                || !proof.recorderAutoStoppedAfterLastFrame
                || proof.recorderWarmupEndOfFrameCount != 2
                || proof.recorderPreHandleEndOfFrameCount
                    != AuditionPvStationPhase2PatternRelayCapture
                        .HandleFrameCount
                || proof.canonicalSourceFrameCount
                    != AuditionPvStationPhase2PatternRelayCapture
                        .ExpectedFrameCount
                || proof.logicalFirstSourceFrame
                    != AuditionPvStationPhase2PatternRelayCapture
                        .SelectStartFrame
                || proof.logicalLastSourceFrame
                    != AuditionPvStationPhase2PatternRelayCapture
                        .SelectEndFrame
                || proof.recordedPreHandleFrameCount
                    != AuditionPvStationPhase2PatternRelayCapture
                        .HandleFrameCount
                || proof.recordedPostHandleFrameCount
                    != AuditionPvStationPhase2PatternRelayCapture
                        .HandleFrameCount
                || !proof.recorderPaddingActiveAtLogicalFrameZero
                || !proof.stateRestored
                || !proof.eventsReleased
                || !proof.presentationClockReleased
                || !proof.cadenceReleased
                || !proof.emitterRestored
                || !proof.spawnOriginOrderRestored
                || !proof.playerStateRestored
                || !proof.bossStateRestored
                || !proof.cameraStateRestored
                || !proof.hudStateRestored
                || !proof.globalStateRestored
                || proof.postRecordingSettleFrames < 0
                || proof.postRecordingSettleFrames
                    > AuditionPvStationPhase2PatternRelayCapture.PostRecordingSettleFrameBudget
                || proof.postRecordingSettleSeconds < 0f
                || proof.postRecordingSettleSeconds
                    > AuditionPvStationPhase2PatternRelayCapture.PostRecordingSettleFrameBudget
                        / (float)AuditionPvCaptureContract.Fps
                || Mathf.Abs(
                    proof.postRecordingSettleSeconds
                    - proof.postRecordingSettleFrames
                        / (float)AuditionPvCaptureContract.Fps) > 0.000001f
                || !string.Equals(
                    proof.stationScenePath,
                    AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    StringComparison.Ordinal)
                || !string.Equals(
                    proof.curtainProfilePath,
                    AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath,
                    StringComparison.Ordinal)
                || !string.Equals(proof.curtainProfileGuid, curtainGuid, StringComparison.Ordinal)
                || !string.Equals(
                    proof.hoverProfilePath,
                    AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath,
                    StringComparison.Ordinal)
                || !string.Equals(proof.hoverProfileGuid, hoverGuid, StringComparison.Ordinal)
                || proof.dependencyHashCount <= 0
                || !AuditionPvSha256.IsSha256(
                    proof.captureStartProvenanceSha256)
                || !AuditionPvSha256.IsSha256(proof.stationSceneSha256)
                || !AuditionPvSha256.IsSha256(proof.warmupEvidenceSha256)
                || !AuditionPvSha256.IsSha256(proof.frame67Sha256)
                || !AuditionPvSha256.IsSha256(proof.frame66Sha256)
                || !AuditionPvSha256.IsSha256(proof.frame417Sha256)
                || !AuditionPvSha256.IsSha256(proof.frame416Sha256)
                || !AuditionPvSha256.IsSha256(proof.bl08Sha256)
                || !AuditionPvSha256.IsSha256(proof.bl09Sha256)
                || string.Equals(proof.frame67Sha256, proof.bl08Sha256, StringComparison.Ordinal)
                || string.Equals(proof.frame417Sha256, proof.bl09Sha256, StringComparison.Ordinal)
                || proof.renderEvents == null
                || !AuditionPvSha256.IsSha256(proof.frame419Sha256)
                || !AuditionPvSha256.IsSha256(proof.frameHashLedgerSha256)
                || proof.frameHashLedgerEntryCount
                    != AuditionPvStationPhase2PatternRelayCapture
                        .ExpectedFrameCount
                || !proof.frameHashLedgerPath.EndsWith(
                    "/" + FrameHashLedgerFileName,
                    StringComparison.Ordinal)
                || !(proof.pixelSampleSourceFrames ?? Array.Empty<int>())
                    .SequenceEqual(ExpectedPixelSampleSourceFrames())
                || proof.renderEvents.Length != 5)
            {
                throw new InvalidOperationException(
                    "G07 runtime proof failed its exact schedule, product response, presentation, no-extra-action, source identity, Recorder, or cleanup contract.");
            }

            ValidateAuthoredMarkerColors(proof);

            int[] expectedFrames = { 10, 68, 368, 418 };
            int[] expectedMarkers = { 7, 7, 4, 4 };
            for (int index = 0; index < expectedFrames.Length; index++)
            {
                ValidateRenderEvent(
                    proof.renderEvents[index],
                    expectedFrames[index],
                    expectedMarkers[index]);
            }

            ValidateFinalHeroRender(proof.renderEvents[4]);

            ValidateVisualMetrics(proof.visualMetrics);
            ValidateHudMetrics(proof.hudMetrics);
            ValidateWindupDelta(proof.curtainWindupDelta);
            ValidateWindupDelta(proof.hoverWindupDelta);
            if (proof.curtainFireDelta == null
                || proof.curtainFireDelta.sampleCount <= 0
                || proof.hoverFireDelta == null
                || proof.hoverFireDelta.sampleCount <= 0
                || proof.curtainQuietDelta == null
                || proof.curtainQuietDelta.sampleCount <= 0
                || proof.hoverQuietDelta == null
                || proof.hoverQuietDelta.sampleCount <= 0
                || proof.curtainWindupColors == null
                || proof.hoverWindupColors == null)
            {
                throw new InvalidOperationException(
                    "G07 fire telemetry or pattern-specific windup color evidence is missing.");
            }

            ValidatePatternColorRoi(
                proof.curtainWindupColors,
                proof.renderEvents[0].boss,
                "Curtain");
            ValidatePatternColorRoi(
                proof.hoverWindupColors,
                proof.renderEvents[2].boss,
                "Hover");

            if (requirePixelCalibration)
            {
                ValidateCalibratedPatternPixelEvidence(proof);
            }
        }

        internal static void ValidateCalibratedPatternPixelEvidence(RuntimeProof proof)
        {
            ValidatePatternPixelEvidence(
                proof,
                PatternPixelCalibrationLocked,
                MinimumCurtainGreenSamples,
                MinimumHoverCyanSamples,
                MinimumCurtainLocalizedFireMeanAbsoluteRgb,
                MinimumHoverLocalizedFireMeanAbsoluteRgb,
                MinimumCurtainFireOverQuietMeanMargin,
                MinimumHoverFireOverQuietMeanMargin);
        }

        internal static void ValidatePatternPixelEvidence(
            RuntimeProof proof,
            bool calibrationLocked,
            long minimumCurtainGreenSamples,
            long minimumHoverCyanSamples,
            double minimumCurtainFireMean,
            double minimumHoverFireMean,
            double minimumCurtainFireOverQuietMargin,
            double minimumHoverFireOverQuietMargin)
        {
            if (!calibrationLocked
                || minimumCurtainGreenSamples <= 0
                || minimumHoverCyanSamples <= 0
                || minimumCurtainFireMean <= 0d
                || minimumHoverFireMean <= 0d
                || minimumCurtainFireOverQuietMargin <= 0d
                || minimumHoverFireOverQuietMargin <= 0d)
            {
                throw new InvalidOperationException(
                    "G07 CalibrationRequired: first honest take retained dynamic Curtain/Hover color and localized fire metrics; freeze measured fail-closed minima and negative fixtures before publishing a manifest.");
            }

            if (proof?.curtainWindupColors == null
                || proof.curtainWindupColors.curtainGreenSampleCount
                    < minimumCurtainGreenSamples
                || proof.hoverWindupColors == null
                || proof.hoverWindupColors.hoverCyanSampleCount
                    < minimumHoverCyanSamples
                || proof.curtainFireDelta == null
                || proof.curtainFireDelta.meanAbsoluteRgb
                    < minimumCurtainFireMean
                || proof.hoverFireDelta == null
                || proof.hoverFireDelta.meanAbsoluteRgb
                    < minimumHoverFireMean
                || proof.curtainQuietDelta == null
                || proof.curtainFireDelta.meanAbsoluteRgb
                    - proof.curtainQuietDelta.meanAbsoluteRgb
                    < minimumCurtainFireOverQuietMargin
                || proof.hoverQuietDelta == null
                || proof.hoverFireDelta.meanAbsoluteRgb
                    - proof.hoverQuietDelta.meanAbsoluteRgb
                    < minimumHoverFireOverQuietMargin)
            {
                throw new InvalidOperationException(
                    "G07 calibrated dynamic-ROI color or localized fire boundary gate failed.");
            }
        }

        private static void ValidatePatternColorRoi(
            PatternColorMetrics metrics,
            SubjectViewportEvidence boss,
            string label)
        {
            RectInt expected = ExpandAndClamp(
                RectFromSubject(boss),
                PatternWindupColorRoiPadding,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            long expectedSamples = ((expected.width + 3L) / 4L)
                * ((expected.height + 3L) / 4L);
            if (metrics == null
                || metrics.roiX != expected.x
                || metrics.roiY != expected.y
                || metrics.roiWidth != expected.width
                || metrics.roiHeight != expected.height
                || metrics.sampleStride != 4
                || metrics.sampleCount != expectedSamples)
            {
                throw new InvalidOperationException(
                    "G07 " + label
                    + " pattern-color evidence is not bound to the exact late-rendered boss windup ROI.");
            }
        }

        private static void ValidateAuthoredMarkerColors(RuntimeProof proof)
        {
            bool Rgb(Color actual, float r, float g, float b) =>
                Mathf.Abs(actual.r - r) <= 0.002f
                && Mathf.Abs(actual.g - g) <= 0.002f
                && Mathf.Abs(actual.b - b) <= 0.002f;
            if (!Rgb(proof.curtainWindupMarkerColor, 0.16f, 1f, 0.66f)
                || !Rgb(proof.curtainFireMarkerColor, 0.75f, 1f, 0.9f)
                || !Rgb(proof.hoverWindupMarkerColor, 0.2f, 0.9f, 1f)
                || !Rgb(proof.hoverFireMarkerColor, 0.72f, 1f, 1f))
            {
                throw new InvalidOperationException(
                    "G07 runtime telegraph marker colors do not match the authored Curtain/Hover windup/release identities.");
            }
        }

        private static void ValidateRenderEvent(
            RenderEventEvidence evidence,
            int expectedFrame,
            int expectedMarkers)
        {
            bool markerArrayExact = evidence?.markers != null
                && evidence.markers.Length == expectedMarkers;
            bool allMarkerBoundsFoundAndInFront = markerArrayExact
                && evidence.markers.All(marker => marker != null
                    && marker.rendererBoundsFound
                    && marker.centerInFront);
            bool anyMarkerIntersects = markerArrayExact
                && evidence.markers.Any(marker => marker != null
                    && marker.rendererBoundsFound
                    && marker.frustumIntersects
                    && marker.pixelWidth > 0
                    && marker.pixelHeight > 0);
            bool allMarkersIntersect = markerArrayExact
                && evidence.markers.All(marker => marker != null
                    && marker.rendererBoundsFound
                    && marker.frustumIntersects);
            bool allIntersectionFlagExact = markerArrayExact
                && evidence.allMarkerRenderersIntersectFrustum
                    == allMarkersIntersect;
            if (evidence == null
                || evidence.logicalFrame != expectedFrame
                || !evidence.cameraActiveAndEnabled
                || !evidence.cameraPerspective
                || !evidence.cameraFullRect
                || !evidence.cameraTargetTextureNull
                || evidence.player == null
                || !evidence.player.safeViewport
                || evidence.boss == null
                || !evidence.boss.safeViewport
                || evidence.visibleMarkerCount != expectedMarkers
                || evidence.visibleMarkerRendererCount != expectedMarkers
                || !evidence.markerBoundsIntersectFrustum
                || !markerArrayExact
                || !allMarkerBoundsFoundAndInFront
                || !anyMarkerIntersects
                || !allIntersectionFlagExact
                || evidence.markerPixelWidth <= 0
                || evidence.markerPixelHeight <= 0)
            {
                throw new InvalidOperationException(
                    "G07 safe-frame render evidence failed at f" + expectedFrame
                    + ": actualFrame=" + (evidence?.logicalFrame ?? -1)
                    + ", camera=" + (evidence != null
                        && evidence.cameraActiveAndEnabled
                        && evidence.cameraPerspective
                        && evidence.cameraFullRect
                        && evidence.cameraTargetTextureNull)
                    + ", playerSafe=" + (evidence?.player?.safeViewport ?? false)
                    + ", bossSafe=" + (evidence?.boss?.safeViewport ?? false)
                    + ", visible=" + (evidence?.visibleMarkerCount ?? -1)
                    + "/" + expectedMarkers
                    + ", renderers=" + (evidence?.visibleMarkerRendererCount ?? -1)
                    + "/" + expectedMarkers
                    + ", aggregateIntersects="
                    + (evidence?.markerBoundsIntersectFrustum ?? false)
                    + ", anyMarkerIntersects=" + anyMarkerIntersects
                    + ", allBoundsFoundAndInFront="
                    + allMarkerBoundsFoundAndInFront
                    + ", allFlagExact=" + allIntersectionFlagExact
                    + ".");
            }
        }

        private static void ValidateFinalHeroRender(RenderEventEvidence evidence)
        {
            if (evidence == null
                || evidence.logicalFrame != 419
                || !evidence.cameraActiveAndEnabled
                || !evidence.cameraPerspective
                || !evidence.cameraFullRect
                || !evidence.cameraTargetTextureNull
                || evidence.player == null
                || !evidence.player.safeViewport
                || evidence.boss == null
                || !evidence.boss.safeViewport
                || !evidence.finalHeroComposition)
            {
                throw new InvalidOperationException(
                    "G07 final logical f419 hero composition is not safely framed.");
            }
        }

        private static void BeginCapture(bool batchMode)
        {
            if (SessionState.GetBool(SessionActiveKey, false)
                || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                throw new InvalidOperationException(
                    "A G07 capture cannot start during another capture, Play Mode, compilation, or asset update.");
            }

            EnsureNoDirtyOpenScenes();
            DateTime startedAtUtc = DateTime.UtcNow;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded || git.isDirty)
            {
                throw new InvalidOperationException(
                    "G07 golden capture requires a successful clean Git provenance probe: "
                    + git.probeError);
            }

            AuditionPvEngineSnapshot engine = AuditionPvEnvironmentProbe.ReadEngineSnapshot();
            ValidateExactEngineProvenance(
                engine.unityVersion,
                engine.unityVersionWithRevision,
                engine.recorderPackageVersion,
                engine.urpPackageVersion,
                engine.activeRenderPipelineAssetPath);

            string[] dependencyPaths = CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] hashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            AuditionPvDependencyHash station = hashes.SingleOrDefault(value =>
                string.Equals(
                    value.path,
                    AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    StringComparison.OrdinalIgnoreCase));
            if (station == null
                || !station.exists
                || !AuditionPvSha256.IsSha256(station.sha256))
            {
                throw new InvalidOperationException(
                    "G07 could not hash the Station scene.");
            }

            AuditionPvStationPhase2PatternRelayOutput output = null;
            PersistedRunnerState state = null;
            try
            {
                output = AuditionPvStationPhase2PatternRelayCapture.ReserveNewOutput(
                    startedAtUtc,
                    git);
                state = new PersistedRunnerState
                {
                    schema = RunnerSchema,
                    phase = RunnerPhase.AwaitingPlayMode.ToString(),
                    batchMode = batchMode,
                    produceApprovedSixtySecondEvidence = batchMode &&
                        Environment.GetCommandLineArgs().Any(value => string.Equals(
                            value,
                            "-pv60ApprovedEvidence",
                            StringComparison.OrdinalIgnoreCase)),
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
                    dependencyHashesAtStart = hashes,
                    stationSceneSha256AtStart = station.sha256,
                    runtimeProof = new RuntimeProof()
                };
                string statePath = Path.Combine(output.outputDirectory, StateFileName);
                SaveState(statePath, state);
                SessionState.SetString(SessionOwnerKey, SessionOwnerValue);
                SessionState.SetString(SessionStatePathKey, statePath);
                SessionState.SetString(
                    SessionOutputDirectoryKey,
                    output.outputDirectory);
                SessionState.SetString(SessionCaptureIdKey, output.captureId);
                SessionState.SetBool(SessionBatchKey, batchMode);
                SessionState.SetBool(SessionActiveKey, true);
                EditorSceneManager.OpenScene(
                    AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    OpenSceneMode.Single);
                if (SceneManager.GetActiveScene().isDirty)
                {
                    throw new InvalidOperationException(
                        "Fresh Station scene became dirty before G07 Play Mode.");
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
                        state?.runtimeProof,
                        state);
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

            string statePath = SessionState.GetString(
                SessionStatePathKey,
                string.Empty);
            string sessionOutput = SessionState.GetString(
                SessionOutputDirectoryKey,
                string.Empty);
            string sessionCaptureId = SessionState.GetString(
                SessionCaptureIdKey,
                string.Empty);
            bool sessionBatch = SessionState.GetBool(SessionBatchKey, false);
            try
            {
                ValidateSessionRecoveryLocationForRoot(
                    statePath,
                    sessionOutput,
                    sessionCaptureId,
                    AuditionPvCaptureContract.OutputRoot);
            }
            catch (Exception exception)
            {
                // SessionState is not trusted. Never read, delete, or write an
                // artifact until all three independent tokens resolve to the one
                // configured direct-child capture directory.
                ClearSession();
                Debug.LogException(exception);
                if (sessionBatch)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            string terminalFault = SessionState.GetString(
                SessionTerminalFaultKey,
                string.Empty);
            if (!string.IsNullOrWhiteSpace(terminalFault))
            {
                Exception recoveryFailure = RecoverTerminalPersistenceFaultForRoot(
                    sessionOutput,
                    sessionCaptureId,
                    AuditionPvCaptureContract.OutputRoot,
                    terminalFault,
                    ClearSession,
                    sessionBatch ? code => EditorApplication.Exit(code) : null);
                if (recoveryFailure != null)
                {
                    Debug.LogException(recoveryFailure);
                }

                return;
            }

            // The manifest is the terminal commit record. Validate it from the
            // independent canonical SessionState identity before trusting or
            // even parsing the mutable runner-state JSON, so a torn/corrupt
            // state file cannot cause a valid immutable package to be re-run or
            // deleted.
            if (!EditorApplication.isPlaying
                && IsValidCommittedManifestAt(
                    sessionOutput,
                    sessionCaptureId,
                    null,
                    AuditionPvCaptureContract.OutputRoot))
            {
                ClearSession();
                Debug.Log(
                    "[AuditionPV] Recovered committed G07 manifest from canonical session identity: "
                    + sessionOutput);
                if (sessionBatch)
                {
                    EditorApplication.Exit(0);
                }

                return;
            }

            PersistedRunnerState state;
            try
            {
                state = LoadState(statePath);
                ValidateSessionBatchAuthority(sessionBatch, state);
            }
            catch (Exception exception)
            {
                if (IsValidCommittedManifestAt(
                    sessionOutput,
                    sessionCaptureId,
                    null,
                    AuditionPvCaptureContract.OutputRoot))
                {
                    ClearSession();
                    Debug.Log(
                        "[AuditionPV] Recovered committed G07 manifest without runner state: "
                        + sessionOutput);
                    if (sessionBatch)
                    {
                        EditorApplication.Exit(0);
                    }

                    return;
                }

                var recoveryState = new PersistedRunnerState
                {
                    schema = RunnerSchema,
                    phase = RunnerPhase.FailedInPlayMode.ToString(),
                    batchMode = sessionBatch,
                    captureId = sessionCaptureId,
                    outputRoot = AuditionPvCaptureContract.OutputRoot,
                    outputDirectory = sessionOutput,
                    baselineDirectory = Path.Combine(
                        sessionOutput,
                        AuditionPvStationPhase2PatternRelayCapture.BaselinesFolderName)
                };
                TryWriteFailureArtifact(
                    sessionOutput,
                    "state-load",
                    exception,
                    null,
                    recoveryState);
                ClearSession();
                Debug.LogException(exception);
                if (sessionBatch)
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
            if (!EditorApplication.isPlaying && IsValidCommittedManifest(state))
            {
                bool batch = state.batchMode;
                string output = state.outputDirectory;
                ClearSession();
                Debug.Log("[AuditionPV] Recovered committed G07 manifest: " + output);
                if (batch)
                {
                    EditorApplication.Exit(0);
                }

                return;
            }
            if (EditorApplication.isPlaying)
            {
                if (phase == RunnerPhase.AwaitingPlayMode)
                {
                    LaunchPlayModeRunner(statePath, state);
                }
                else if (phase == RunnerPhase.Recording && activeBehaviour == null)
                {
                    NotifyPlayModeFinished(
                        statePath,
                        state,
                        state.runtimeProof,
                        new InvalidOperationException(
                            "A domain reload interrupted G07 Recorder."));
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
                    "Play Mode exited before G07 Recorder reported completion.";
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
                || !string.Equals(
                    scene.path,
                    AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    StringComparison.Ordinal))
            {
                NotifyPlayModeFinished(
                    statePath,
                    state,
                    null,
                    new InvalidOperationException(
                        "G07 entered Play Mode without the fresh Station scene."));
                return;
            }

            state.phase = RunnerPhase.Recording.ToString();
            SaveState(statePath, state);
            var root = new GameObject("[AuditionPV_G07_GoldenRunner]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            activeBehaviour = root.AddComponent<
                AuditionPvStationPhase2PatternRelayGoldenRunnerBehaviour>();
            activeBehaviour.Begin(statePath, state.outputDirectory, state);
        }

        internal static void NotifyPlayModeFinished(
            string statePath,
            PersistedRunnerState state,
            RuntimeProof proof,
            Exception failure)
        {
            state.runtimeProof = proof ?? state.runtimeProof ?? new RuntimeProof();
            state.failure = failure?.ToString() ?? string.Empty;
            state.phase = failure == null
                ? RunnerPhase.AwaitingEditMode.ToString()
                : RunnerPhase.FailedInPlayMode.ToString();
            Exception terminalFailure = failure;
            Exception handoffFailure = ExecuteTerminalHandoff(
                () => SaveState(statePath, state),
                persistenceFailure =>
                {
                    terminalFailure = terminalFailure == null
                        ? persistenceFailure
                        : new AggregateException(terminalFailure, persistenceFailure);
                    SessionState.SetString(
                        SessionTerminalFaultKey,
                        terminalFailure.ToString());
                    state.failure = terminalFailure.ToString();
                    state.phase = RunnerPhase.FailedInPlayMode.ToString();
                    TryWriteFailureArtifact(
                        state.outputDirectory,
                        "playmode-terminal-persistence",
                        terminalFailure,
                        state.runtimeProof,
                        state);
                },
                () =>
                {
                    activeBehaviour = null;
                    EditorApplication.isPlaying = false;
                });
            if (handoffFailure != null)
            {
                throw new InvalidOperationException(
                    "G07 could not persist its PlayMode terminal handoff.",
                    terminalFailure == null
                        ? handoffFailure
                        : new AggregateException(terminalFailure, handoffFailure));
            }
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
                AuditionPvStationPhase2PatternRelayCapture
                    .ReopenProductSceneAfterPlayMode();
                Scene reopened = SceneManager.GetActiveScene();
                if (!reopened.IsValid()
                    || !reopened.isLoaded
                    || reopened.isDirty
                    || !string.Equals(
                        reopened.path,
                        AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G07 did not reopen an unmodified Station scene.");
                }

                if (!string.IsNullOrWhiteSpace(state.failure))
                {
                    throw new InvalidOperationException(
                        "G07 PlayMode recording failed.\n" + state.failure);
                }

                FinalizeSuccessfulCapture(state);
                success = true;
            }
            catch (Exception exception)
            {
                if (IsValidCommittedManifest(state))
                {
                    success = true;
                    failure = null;
                }
                else
                {
                    failure = exception;
                    TryWriteFailureArtifact(
                        state.outputDirectory,
                        state.phase,
                        exception,
                        state.runtimeProof,
                        state);
                    Debug.LogException(exception);
                }
            }
            finally
            {
                bool batch = state.batchMode;
                string output = state.outputDirectory;
                ClearSession();
                finalizing = false;
                if (success)
                {
                    Debug.Log("[AuditionPV] G07 pattern relay passed: " + output);
                    if (batch)
                    {
                        EditorApplication.Exit(0);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(output);
                    }
                }
                else if (batch)
                {
                    EditorApplication.Exit(1);
                }
                else if (failure != null)
                {
                    EditorUtility.DisplayDialog(
                        "G07 golden capture failed",
                        failure.Message,
                        "OK");
                }
            }
        }

        private static void FinalizeSuccessfulCapture(PersistedRunnerState state)
        {
            RuntimeProof proof = state.runtimeProof
                ?? throw new InvalidOperationException("G07 runtime proof is missing.");
            string frames = Path.Combine(
                state.outputDirectory,
                "frames",
                AuditionPvStationPhase2PatternRelayCapture.ShotId);
            string evidence = Path.Combine(
                state.outputDirectory,
                EvidenceFolderName);
            string warmup = RemapRawFrames(frames, evidence);
            ValidateDecodablePngFile(
                warmup,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            ValidateLogicalFrameSequence(frames);
            for (int frame = 0;
                frame < AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount;
                frame++)
            {
                ValidatePngFile(
                    Path.Combine(
                        frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(frame)),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
            }

            AnalyzeFrames(frames, proof);
            string frameHashLedger = BuildFrameHashLedger(frames);
            proof.frameHashLedgerPath = Path.Combine(
                    evidence,
                    FrameHashLedgerFileName)
                .Replace('\\', '/');
            proof.frameHashLedgerSha256 =
                AuditionPvSha256.TextHash(frameHashLedger);
            proof.frameHashLedgerEntryCount =
                AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount;
            proof.pixelSampleSourceFrames = ExpectedPixelSampleSourceFrames();
            proof.warmupEvidencePath = warmup.Replace('\\', '/');
            proof.warmupEvidenceSha256 = AuditionPvSha256.FileHash(warmup);
            proof.frame67Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .LogicalToSourceFrame(67))));
            proof.frame66Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .LogicalToSourceFrame(66))));
            proof.frame417Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .LogicalToSourceFrame(417))));
            proof.frame416Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .LogicalToSourceFrame(416))));
            proof.frame419Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .LogicalToSourceFrame(419))));
            // Source hashes are populated before the calibration gate, but the
            // canonical BL08/BL09 files remain absent on a first-take failure.
            proof.bl08Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .Bl08SourceFrame)));
            proof.bl09Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .Bl09SourceFrame)));
            AuditionPvGitSnapshot gitAtEnd =
                AuditionPvEnvironmentProbe.ReadGitSnapshot();
            ValidateStableGitSnapshot(CreateGitSnapshot(state), gitAtEnd);
            string[] dependenciesAtEnd = CollectCaptureDependencyPaths();
            if (!state.dependencyPaths.SequenceEqual(
                    dependenciesAtEnd,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "G07 dependency path set changed while recording.");
            }

            AuditionPvDependencyHash[] hashesAtEnd =
                AuditionPvEnvironmentProbe.HashDependencies(dependenciesAtEnd);
            ValidateStableDependencies(state.dependencyHashesAtStart, hashesAtEnd);
            proof.dependencyHashCount = state.dependencyHashesAtStart.Length;
            proof.stationScenePath =
                AuditionPvStationPhase2PatternRelayCapture.StationScenePath;
            proof.stationSceneSha256 = state.stationSceneSha256AtStart;
            proof.curtainProfilePath =
                AuditionPvStationPhase2PatternRelayCapture.CurtainProfilePath;
            proof.curtainProfileGuid = AssetDatabase.AssetPathToGUID(
                proof.curtainProfilePath);
            proof.hoverProfilePath =
                AuditionPvStationPhase2PatternRelayCapture.HoverProfilePath;
            proof.hoverProfileGuid = AssetDatabase.AssetPathToGUID(
                proof.hoverProfilePath);
            proof.captureStartProvenanceSha256 =
                ComputeCaptureStartProvenanceSha256(
                    state.captureId,
                    state.startedAtUtc,
                    state.outputRoot,
                    state.outputDirectory,
                    state.gitCommitSha,
                    state.gitBranch,
                    state.gitWorktreeDirty,
                    state.gitDirtyHashSha256,
                    AuditionPvGitSnapshot.DirtyHashAlgorithm,
                    state.engine,
                    state.dependencyHashesAtStart);

            ValidateRuntimeProof(proof);
            string failurePath = Path.Combine(state.outputDirectory, FailureFileName);
            if (File.Exists(failurePath))
            {
                throw new InvalidOperationException(
                    "G07 success cannot coexist with a failure artifact.");
            }

            CopyBaselines(state, frames, proof);
            WriteTextNew(proof.frameHashLedgerPath, frameHashLedger);
            ValidateFrameHashLedger(
                frames,
                proof.frameHashLedgerPath,
                proof.frameHashLedgerSha256);

            string proofPath = Path.Combine(evidence, RuntimeProofFileName);
            WriteJsonNew(proofPath, new RuntimeProofArtifact
            {
                schema = RuntimeProofSchema,
                captureId = state.captureId,
                mapping =
                    "Recorder raw0 is preserved warm-up evidence; raw1..raw780 map to canonical source f0..f779; logical f0..f419 map to source f180..f599.",
                cadence =
                    "Exactly one public BossBarrageEmitter.Tick((1/60)*exactScale1) per logical frame; observed events f10/f68/f368/f418.",
                playerResponse =
                    "Product movement input only: lane-back f17..f46/zero f47; opposite Hover preview f374..f406/zero f407.",
                runtime = proof
            });
            DateTime started = DateTime.Parse(
                state.startedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
            AuditionPvShotManifestEntry[] shots =
            {
                AuditionPvStationPhase2PatternRelayCapture
                    .CreateShotManifestEntry()
            };
            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationPhase2PatternRelayCapture
                    .CreateBaselineManifestEntries();
            AuditionPvCaptureManifest captureCoreManifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    shots,
                    baselines,
                    Array.Empty<AuditionPvTestResult>(),
                    createdAtUtc: started,
                    gitSnapshot: CreateGitSnapshot(state),
                    engineSnapshot: RestoreEngine(state.engine),
                    dependencyHashSnapshot: state.dependencyHashesAtStart);
            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator
                    .CaptureCoreSha256(captureCoreManifest);
            if (!AuditionPvSha256.IsSha256(captureCoreSha256))
            {
                throw new InvalidDataException(
                    "G07 could not create its immutable Gate capture-core identity.");
            }

            AuditionPvTestResult[] ordinaryResults = CreateTestResults(
                state,
                proof,
                proofPath,
                started);
            AuditionPvTestResult[] gateResults = WriteGateEvidenceArtifacts(
                state,
                proof,
                proofPath,
                proof.frameHashLedgerPath,
                evidence,
                captureCoreSha256,
                started);
            AuditionPvTestResult[] results = ordinaryResults
                .Concat(gateResults)
                .ToArray();
            if (state.produceApprovedSixtySecondEvidence)
            {
                AuditionPvSixtySecondEvidenceBundle sixtySecondEvidence =
                    AuditionPvSixtySecondEvidenceProducer.Produce(
                        new AuditionPvSixtySecondEvidenceRequest
                        {
                            captureCoreManifest = captureCoreManifest,
                            expectedCaptureCoreSha256 = captureCoreSha256,
                            sourceShotId =
                                AuditionPvStationPhase2PatternRelayCapture.ShotId,
                            sourceRangeStartFrame = S070SourceRangeStartFrame,
                            sourceRangeEndFrame = S070SourceRangeEndFrame,
                            selectStartFrame = S070SelectStartFrame,
                            selectEndFrame = S070SelectEndFrame,
                            runtimeWorkloadSealPath =
                                state.s070RuntimeWorkloadSealPath,
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
                    .MergeCaptureTestResults(results, sixtySecondEvidence);
            }
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    shots,
                    baselines,
                    results,
                    createdAtUtc: started,
                    gitSnapshot: CreateGitSnapshot(state),
                    engineSnapshot: RestoreEngine(state.engine),
                    dependencyHashSnapshot: state.dependencyHashesAtStart);
            if (!string.Equals(
                    captureCoreSha256,
                    AuditionPvSixtySecondGateManifestValidator
                        .CaptureCoreSha256(manifest),
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "G07 Gate evidence changed its immutable capture-core identity.");
            }

            ValidateManifestInMemory(manifest, state.captureId);

            // This is deliberately the final fallible success operation. No
            // validation or state write follows it, so a failed take cannot
            // retain a published manifest.
            AuditionPvCaptureManifestWriter.WriteNew(manifest);
        }

        private static void AnalyzeFrames(string frameDirectory, RuntimeProof proof)
        {
            int Source(int logicalFrame) =>
                AuditionPvStationPhase2PatternRelayCapture
                    .LogicalToSourceFrame(logicalFrame);
            var sequence = new SequenceVisualMetrics
            {
                minimumSampledLuma = 255
            };
            var hud = new HudVisualMetrics
            {
                minimumFramePinkSamples = int.MaxValue,
                minimumFrameDarkSamples = int.MaxValue,
                minimumFrameBrightSamples = int.MaxValue,
                minimumFrameMeanLuma = double.PositiveInfinity,
                maximumFrameMeanLuma = double.NegativeInfinity,
                roiX = HudRawBottomLeftRoi.x,
                roiY = HudRawBottomLeftRoi.y,
                roiWidth = HudRawBottomLeftRoi.width,
                roiHeight = HudRawBottomLeftRoi.height,
                sampleStride = 4
            };
            var selected = new Dictionary<int, Color32[]>();
            int[] selectedFrames = new[]
                { 9, 10, 66, 67, 68, 367, 368, 416, 417, 418 }
                .Select(Source)
                .ToArray();
            var selectedSet = new HashSet<int>(selectedFrames);
            for (int frame = 0;
                frame < AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount;
                frame++)
            {
                Texture2D texture = LoadPng(Path.Combine(
                    frameDirectory,
                    AuditionPvStationPhase2PatternRelayCapture.FrameFileName(frame)));
                try
                {
                    AnalyzeSequenceFrame(texture, sequence);
                    AnalyzeHudFrame(
                        texture,
                        HudRawBottomLeftRoi,
                        hud);
                    if (selectedSet.Contains(frame))
                    {
                        selected[frame] = texture.GetPixels32();
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }

            sequence.blackRatio = sequence.blackSampleCount
                / (double)Math.Max(1L, sequence.sampleCount);
            sequence.magentaRatio = sequence.magentaSampleCount
                / (double)Math.Max(1L, sequence.sampleCount);
            proof.visualMetrics = sequence;
            proof.hudMetrics = hud;
            proof.curtainWindupDelta = EvaluateFrameDelta(
                selected[Source(9)], selected[Source(10)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                sampleStride: 16);
            RenderEventEvidence curtainFireRender = RequireRenderEvidence(proof, 68);
            RenderEventEvidence hoverFireRender = RequireRenderEvidence(proof, 418);
            RectInt curtainMarkerRoi = ExpandAndClamp(
                RectFromMarkerEvidence(curtainFireRender),
                8,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            RectInt hoverMarkerRoi = ExpandAndClamp(
                RectFromMarkerEvidence(hoverFireRender),
                8,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            RectInt curtainFireRoi = ExpandAndClamp(
                Union(curtainMarkerRoi, RectFromSubject(curtainFireRender.boss)),
                24,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            RectInt hoverFireRoi = ExpandAndClamp(
                Union(hoverMarkerRoi, RectFromSubject(hoverFireRender.boss)),
                24,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            proof.curtainFireDelta = EvaluateFrameDelta(
                selected[Source(67)], selected[Source(68)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                curtainFireRoi,
                sampleStride: 4);
            proof.curtainQuietDelta = EvaluateFrameDelta(
                selected[Source(66)], selected[Source(67)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                curtainFireRoi,
                sampleStride: 4);
            proof.hoverWindupDelta = EvaluateFrameDelta(
                selected[Source(367)], selected[Source(368)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                sampleStride: 16);
            proof.hoverFireDelta = EvaluateFrameDelta(
                selected[Source(417)], selected[Source(418)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                hoverFireRoi,
                sampleStride: 4);
            proof.hoverQuietDelta = EvaluateFrameDelta(
                selected[Source(416)], selected[Source(417)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                hoverFireRoi,
                sampleStride: 4);
            RenderEventEvidence curtainWindupRender = RequireRenderEvidence(proof, 10);
            RenderEventEvidence hoverWindupRender = RequireRenderEvidence(proof, 368);
            RectInt curtainWindupColorRoi = ExpandAndClamp(
                RectFromSubject(curtainWindupRender.boss),
                PatternWindupColorRoiPadding,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            RectInt hoverWindupColorRoi = ExpandAndClamp(
                RectFromSubject(hoverWindupRender.boss),
                PatternWindupColorRoiPadding,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            proof.curtainWindupColors = EvaluatePatternColors(
                selected[Source(10)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                curtainWindupColorRoi,
                4);
            proof.hoverWindupColors = EvaluatePatternColors(
                selected[Source(368)],
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                hoverWindupColorRoi,
                4);
        }

        private static RenderEventEvidence RequireRenderEvidence(
            RuntimeProof proof,
            int logicalFrame)
        {
            RenderEventEvidence evidence = proof?.renderEvents?.SingleOrDefault(value =>
                value != null && value.logicalFrame == logicalFrame);
            return evidence ?? throw new InvalidOperationException(
                "G07 dynamic pixel analysis is missing late render evidence for f"
                + logicalFrame + ".");
        }

        private static RectInt RectFromMarkerEvidence(RenderEventEvidence evidence)
        {
            return new RectInt(
                evidence.markerPixelX,
                evidence.markerPixelY,
                evidence.markerPixelWidth,
                evidence.markerPixelHeight);
        }

        private static RectInt RectFromSubject(SubjectViewportEvidence evidence)
        {
            if (evidence == null)
            {
                throw new InvalidOperationException(
                    "G07 dynamic fire ROI is missing subject render evidence.");
            }

            return new RectInt(
                evidence.pixelX,
                evidence.pixelY,
                evidence.pixelWidth,
                evidence.pixelHeight);
        }

        internal static RectInt Union(RectInt left, RectInt right)
        {
            int xMin = Math.Min(left.xMin, right.xMin);
            int yMin = Math.Min(left.yMin, right.yMin);
            int xMax = Math.Max(left.xMax, right.xMax);
            int yMax = Math.Max(left.yMax, right.yMax);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        internal static RectInt ExpandAndClamp(
            RectInt roi,
            int padding,
            int width,
            int height)
        {
            if (padding < 0 || width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(padding));
            }

            int xMin = Mathf.Clamp(roi.xMin - padding, 0, width - 1);
            int yMin = Mathf.Clamp(roi.yMin - padding, 0, height - 1);
            int xMax = Mathf.Clamp(roi.xMax + padding, xMin + 1, width);
            int yMax = Mathf.Clamp(roi.yMax + padding, yMin + 1, height);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static void AnalyzeSequenceFrame(
            Texture2D texture,
            SequenceVisualMetrics metrics)
        {
            int width = texture.width;
            int height = texture.height;
            long samples = 0;
            long black = 0;
            long magenta = 0;
            int minimumLuma = 255;
            int maximumLuma = 0;
            const int Step = 32;
            for (int y = Step / 2; y < height; y += Step)
            {
                for (int x = Step / 2; x < width; x += Step)
                {
                    Color32 pixel = texture.GetPixel(x, y);
                    int luma = Luma(pixel);
                    minimumLuma = Math.Min(minimumLuma, luma);
                    maximumLuma = Math.Max(maximumLuma, luma);
                    if (pixel.r <= 10 && pixel.g <= 10 && pixel.b <= 10)
                    {
                        black++;
                    }

                    if (pixel.r >= 245 && pixel.g <= 12 && pixel.b >= 245)
                    {
                        magenta++;
                    }

                    samples++;
                }
            }

            metrics.sampleCount += samples;
            metrics.blackSampleCount += black;
            metrics.magentaSampleCount += magenta;
            double frameBlack = black / (double)Math.Max(1L, samples);
            double frameMagenta = magenta / (double)Math.Max(1L, samples);
            if (frameBlack < MaximumSequenceBlackRatio)
            {
                metrics.healthyFrameCount++;
            }

            if (frameMagenta > 0d)
            {
                metrics.magentaAffectedFrameCount++;
            }

            metrics.maximumFrameMagentaRatio = Math.Max(
                metrics.maximumFrameMagentaRatio,
                frameMagenta);
            metrics.minimumSampledLuma = Math.Min(
                metrics.minimumSampledLuma,
                minimumLuma);
            metrics.maximumSampledLuma = Math.Max(
                metrics.maximumSampledLuma,
                maximumLuma);
        }

        private static void AnalyzeHudFrame(
            Texture2D texture,
            RectInt roi,
            HudVisualMetrics aggregate)
        {
            int samples = 0;
            int pink = 0;
            int dark = 0;
            int bright = 0;
            long lumaTotal = 0;
            for (int y = roi.yMin; y < roi.yMax; y += 4)
            {
                for (int x = roi.xMin; x < roi.xMax; x += 4)
                {
                    Color32 pixel = texture.GetPixel(x, y);
                    int luma = Luma(pixel);
                    if (pixel.r >= 140
                        && pixel.r - pixel.g >= 15
                        && pixel.r - pixel.b >= 5)
                    {
                        pink++;
                    }

                    if (Math.Max(pixel.r, Math.Max(pixel.g, pixel.b)) <= 65)
                    {
                        dark++;
                    }

                    if (luma >= 180)
                    {
                        bright++;
                    }

                    lumaTotal += luma;
                    samples++;
                }
            }

            double mean = lumaTotal / (double)Math.Max(1, samples);
            aggregate.frameCount++;
            aggregate.minimumFramePinkSamples = Math.Min(
                aggregate.minimumFramePinkSamples,
                pink);
            aggregate.maximumFramePinkSamples = Math.Max(
                aggregate.maximumFramePinkSamples,
                pink);
            aggregate.minimumFrameDarkSamples = Math.Min(
                aggregate.minimumFrameDarkSamples,
                dark);
            aggregate.maximumFrameDarkSamples = Math.Max(
                aggregate.maximumFrameDarkSamples,
                dark);
            aggregate.minimumFrameBrightSamples = Math.Min(
                aggregate.minimumFrameBrightSamples,
                bright);
            aggregate.maximumFrameBrightSamples = Math.Max(
                aggregate.maximumFrameBrightSamples,
                bright);
            aggregate.minimumFrameMeanLuma = Math.Min(
                aggregate.minimumFrameMeanLuma,
                mean);
            aggregate.maximumFrameMeanLuma = Math.Max(
                aggregate.maximumFrameMeanLuma,
                mean);
        }

        private static void EvaluateHudFrame(
            Texture2D texture,
            RectInt roi,
            int stride,
            out int pink,
            out int dark,
            out int bright,
            out double meanLuma)
        {
            pink = 0;
            dark = 0;
            bright = 0;
            long total = 0;
            int samples = 0;
            for (int y = roi.yMin; y < roi.yMax; y += stride)
            {
                for (int x = roi.xMin; x < roi.xMax; x += stride)
                {
                    Color32 pixel = texture.GetPixel(x, y);
                    int luma = Luma(pixel);
                    if (pixel.r >= 140
                        && pixel.r - pixel.g >= 15
                        && pixel.r - pixel.b >= 5)
                    {
                        pink++;
                    }

                    if (Math.Max(pixel.r, Math.Max(pixel.g, pixel.b)) <= 65)
                    {
                        dark++;
                    }

                    if (luma >= 180)
                    {
                        bright++;
                    }

                    total += luma;
                    samples++;
                }
            }

            meanLuma = total / (double)Math.Max(1, samples);
        }

        private static int Luma(Color32 pixel)
        {
            return (pixel.r * 54 + pixel.g * 183 + pixel.b * 19) >> 8;
        }

        private static void ValidateRoi(RectInt roi, int width, int height)
        {
            if (roi.width <= 0
                || roi.height <= 0
                || roi.xMin < 0
                || roi.yMin < 0
                || roi.xMax > width
                || roi.yMax > height)
            {
                throw new ArgumentOutOfRangeException(nameof(roi));
            }
        }

        private static Texture2D LoadPng(
            string path,
            int expectedWidth = AuditionPvCaptureContract.Width,
            int expectedHeight = AuditionPvCaptureContract.Height)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true)
            {
                name = "G07Validation_" + Path.GetFileNameWithoutExtension(path)
            };
            if (!ImageConversion.LoadImage(
                    texture,
                    File.ReadAllBytes(path),
                    markNonReadable: false)
                || texture.width != expectedWidth
                || texture.height != expectedHeight)
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidDataException(
                    $"Unity could not decode the exact {expectedWidth}x{expectedHeight} G07 PNG: "
                    + path);
            }

            return texture;
        }

        private static void CopyBaselines(
            PersistedRunnerState state,
            string frameDirectory,
            RuntimeProof proof)
        {
            foreach (AuditionPvBaselineManifestEntry baseline in
                     AuditionPvStationPhase2PatternRelayCapture
                         .CreateBaselineManifestEntries())
            {
                string source = Path.Combine(
                    frameDirectory,
                    AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                        baseline.sourceFrame));
                string destination = Path.Combine(
                    state.baselineDirectory,
                    baseline.fileName);
                CopyNew(source, destination);
                string sourceHash = AuditionPvSha256.FileHash(source);
                string destinationHash = AuditionPvSha256.FileHash(destination);
                if (!string.Equals(sourceHash, destinationHash, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G07 baseline is not a byte-exact event-frame copy: "
                        + baseline.id);
                }

                if (baseline.id == "bl08")
                {
                    proof.bl08Sha256 = destinationHash;
                }
                else if (baseline.id == "bl09")
                {
                    proof.bl09Sha256 = destinationHash;
                }
            }
        }

        private static AuditionPvTestResult[] CreateTestResults(
            PersistedRunnerState state,
            RuntimeProof proof,
            string proofPath,
            DateTime startedAtUtc)
        {
            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            AuditionPvTestResult Passed(
                string suite,
                string name,
                string details,
                string artifact) => new()
            {
                suite = suite,
                name = name,
                status = "passed",
                durationMilliseconds = duration,
                details = details,
                artifactPath = artifact?.Replace('\\', '/') ?? string.Empty
            };
            return new[]
            {
                Passed(
                    "recorder",
                    "raw-warmup-and-logical-remap",
                    "Recorder 5.1.6 QHD60 raw0..780 exact; raw0 evidence; raw1..780 -> source f0..f779; logical f0..f419 occupies source f180..f599 with real 180/180 handles.",
                    proof.warmupEvidencePath),
                Passed(
                    "product-state",
                    "phase2-curtain-hover-relay",
                    "Actual event path: Curtain f10/f68 x7 priority; Hover f368/f418 x4 sequence; exactly 420 Tick calls.",
                    proofPath),
                Passed(
                    "player-response",
                    "movement-api-counterplay",
                    $"Risk {proof.curtainRiskBefore:F3}->{proof.curtainRiskAfter:F3}; Hover {proof.hoverLateralDisplacement:F3}m dot={proof.hoverDirectionDot:F4}; run/stop=2/2.",
                    proofPath),
                Passed(
                    "render",
                    "qhd-hud-windup-pattern-telemetry",
                    $"780 QHD PNGs; black={proof.visualMetrics.blackRatio:P3}; magenta={proof.visualMetrics.magentaRatio:P3}; windup deltas={proof.curtainWindupDelta.meanAbsoluteRgb:F2}/{proof.hoverWindupDelta.meanAbsoluteRgb:F2}.",
                    Path.Combine(state.outputDirectory, "frames", "g07")),
                Passed(
                    "render",
                    "late-camera-frustum-evidence",
                    "Four +32000 LateUpdate probes prove exact gameplay camera, player/boss safe viewport, and 7/7/4/4 marker visibility.",
                    proofPath),
                Passed(
                    "provenance",
                    "clean-git-dependencies-and-byte-baselines",
                    $"{proof.dependencyHashCount} hashes stable; 780-entry source ledger; BL08/BL09 byte-exact at source f248/f598; logical f67/f417 neighbor hashes retained at source f247/f597.",
                    proofPath),
                Passed(
                    "lifecycle",
                    "exhaustive-public-cleanup",
                    $"Unrecorded cue settle {proof.postRecordingSettleSeconds:F3}s; emitter/index/queue, poses, camera, HUD, globals, events, cadence and clock restored.",
                    proofPath)
            };
        }

        private static AuditionPvTestResult[] WriteGateEvidenceArtifacts(
            PersistedRunnerState state,
            RuntimeProof proof,
            string runtimeProofPath,
            string frameHashLedgerPath,
            string evidenceDirectory,
            string captureCoreSha256,
            DateTime startedAtUtc)
        {
            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            string createdAtUtc = startedAtUtc.ToString(
                "O",
                CultureInfo.InvariantCulture);
            string normalizedRuntimeProofPath =
                Path.GetFullPath(runtimeProofPath).Replace('\\', '/');
            string runtimeProofSha256 =
                AuditionPvSha256.FileHash(runtimeProofPath);
            string normalizedFrameLedgerPath =
                Path.GetFullPath(frameHashLedgerPath).Replace('\\', '/');
            string frameLedgerSha256 =
                AuditionPvSha256.FileHash(frameHashLedgerPath);
            var runtimePin = new AuditionPvPinnedArtifact
            {
                path = normalizedRuntimeProofPath,
                sha256 = runtimeProofSha256
            };
            var ledgerPin = new AuditionPvPinnedArtifact
            {
                path = normalizedFrameLedgerPath,
                sha256 = frameLedgerSha256
            };

            AuditionPvTestResult Passed(
                string name,
                string details,
                string artifactPath) => new()
            {
                suite = AuditionPvStationPhase2PatternRelayCapture
                    .GateEvidenceTestSuite,
                name = name,
                status = "passed",
                durationMilliseconds = duration,
                details = details,
                artifactPath = artifactPath.Replace('\\', '/')
            };

            string authorshipPath = Path.Combine(
                evidenceDirectory,
                GateShotAuthorshipFileName);
            var authorship = new AuditionPvShotAuthorshipArtifact
            {
                schemaVersion =
                    AuditionPvSixtySecondGateManifestValidator
                        .ShotAuthorshipSchema,
                sourceCaptureCoreSha256 = captureCoreSha256,
                captureId = state.captureId,
                sourceShotId =
                    AuditionPvStationPhase2PatternRelayCapture.ShotId,
                cameraId =
                    AuditionPvStationPhase2PatternRelayCapture.GateCameraId,
                gameplayState =
                    AuditionPvStationPhase2PatternRelayCapture
                        .GateGameplayState,
                deterministicSeed =
                    AuditionPvStationPhase2PatternRelayCapture
                        .DeterministicRandomSeed,
                timelineId =
                    AuditionPvStationPhase2PatternRelayCapture.GateTimelineId,
                runtimeProof = runtimePin,
                tool = "G07GoldenRunner",
                toolVersion = string.IsNullOrWhiteSpace(
                    state.engine?.recorderPackageVersion)
                    ? "1"
                    : state.engine.recorderPackageVersion,
                createdAtUtc = createdAtUtc
            };
            WriteJsonNew(authorshipPath, authorship);
            string authorshipSha256 =
                AuditionPvSha256.FileHash(authorshipPath);
            var results = new List<AuditionPvTestResult>
            {
                Passed(
                    "shot-authorship/"
                        + AuditionPvStationPhase2PatternRelayCapture.ShotId,
                    $"artifact-sha256={authorshipSha256}; capture-core-sha256={captureCoreSha256}; exact-camera-state-seed-timeline=true",
                    authorshipPath),
                Passed(
                    "shot-authorship-runtime/"
                        + AuditionPvStationPhase2PatternRelayCapture.ShotId,
                    $"artifact-sha256={runtimeProofSha256}; capture-core-sha256={captureCoreSha256}; exact-runtime=true",
                    runtimeProofPath)
            };

            string semanticDirectory = Path.Combine(
                evidenceDirectory,
                GateSemanticEvidenceFolderName);
            foreach (GateSemanticBeatSpec spec in
                     CreateGateSemanticBeatSpecs(proof))
            {
                string artifactPath = Path.Combine(
                    semanticDirectory,
                    spec.beatId + ".json");
                var artifact = new GateSemanticBeatRuntimeArtifact
                {
                    schemaVersion =
                        "dimension-brawl.audition-pv.g07-semantic-beat-runtime.v1",
                    sourceCaptureCoreSha256 = captureCoreSha256,
                    captureId = state.captureId,
                    sourceShotId =
                        AuditionPvStationPhase2PatternRelayCapture.ShotId,
                    beatId = spec.beatId,
                    runtimeFactKey = spec.beatId,
                    sourceRangeStartFrame =
                        AuditionPvStationPhase2PatternRelayCapture.FirstFrame,
                    sourceRangeEndFrame =
                        AuditionPvStationPhase2PatternRelayCapture.LastFrame,
                    logicalFactStartFrame = spec.logicalStartFrame,
                    logicalFactEndFrame = spec.logicalEndFrame,
                    sourceFactStartFrame =
                        AuditionPvStationPhase2PatternRelayCapture
                            .LogicalToSourceFrame(spec.logicalStartFrame),
                    sourceFactEndFrame =
                        AuditionPvStationPhase2PatternRelayCapture
                            .LogicalToSourceFrame(spec.logicalEndFrame),
                    exactFacts = spec.exactFacts,
                    runtimeProof = runtimePin,
                    sourceFrameLedger = ledgerPin,
                    producer = "G07GoldenRunner",
                    createdAtUtc = createdAtUtc
                };
                WriteJsonNew(artifactPath, artifact);
                string artifactSha256 =
                    AuditionPvSha256.FileHash(artifactPath);
                results.Add(Passed(
                    "semantic-beat/" + spec.beatId,
                    $"artifact-sha256={artifactSha256}; semantic-fact={spec.beatId}; capture-core-sha256={captureCoreSha256}; exact-runtime=true",
                    artifactPath));
            }

            string[] actualBeatIds = results
                .Where(result => result.name.StartsWith(
                    "semantic-beat/",
                    StringComparison.Ordinal))
                .Select(result => result.name.Substring(
                    "semantic-beat/".Length))
                .ToArray();
            if (!actualBeatIds.SequenceEqual(
                    AuditionPvStationPhase2PatternRelayCapture
                        .GateSemanticBeatIds(),
                    StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "G07 Gate semantic-beat artifacts are incomplete or reordered.");
            }

            return results.ToArray();
        }

        private static GateSemanticBeatSpec[] CreateGateSemanticBeatSpecs(
            RuntimeProof proof)
        {
            return new[]
            {
                new GateSemanticBeatSpec(
                    "boss-pattern-2",
                    AuditionPvStationPhase2PatternRelayCapture
                        .CurtainWindupFrame,
                    AuditionPvStationPhase2PatternRelayCapture
                        .CurtainFireFrame,
                    $"pattern={proof.curtainFirePatternId}",
                    $"priority={proof.curtainWasPriority}",
                    $"projectiles={proof.curtainSpawnedCount}",
                    $"windup-fire={proof.curtainWindupFrame}>{proof.curtainFireFrame}"),
                new GateSemanticBeatSpec(
                    "boss-pattern-3",
                    AuditionPvStationPhase2PatternRelayCapture
                        .HoverWindupFrame,
                    AuditionPvStationPhase2PatternRelayCapture
                        .HoverFireFrame,
                    $"pattern={proof.hoverFirePatternId}",
                    $"priority={proof.hoverWasPriority}",
                    $"sequence-index-after={proof.hoverSequenceIndexAfterFire}",
                    $"projectiles={proof.hoverSpawnedCount}",
                    $"windup-fire={proof.hoverWindupFrame}>{proof.hoverFireFrame}")
            };
        }

        private static void ValidateManifestInMemory(
            AuditionPvCaptureManifest manifest,
            string captureId)
        {
            AuditionPvCaptureManifestWriter.Validate(manifest);
            string json = JsonUtility.ToJson(manifest, true);
            AuditionPvCaptureManifest roundTrip =
                JsonUtility.FromJson<AuditionPvCaptureManifest>(json);
            AuditionPvCaptureManifestWriter.Validate(roundTrip);
            ValidateExactEngineProvenance(
                roundTrip.unityVersion,
                roundTrip.unityVersionWithRevision,
                roundTrip.recorderPackageVersion,
                roundTrip.urpPackageVersion,
                roundTrip.activeRenderPipelineAssetPath);
            AuditionPvShotManifestEntry shot = roundTrip.shots.Single();
            AuditionPvBaselineManifestEntry bl08 = roundTrip.baselines.Single(value =>
                value.id == "bl08");
            AuditionPvBaselineManifestEntry bl09 = roundTrip.baselines.Single(value =>
                value.id == "bl09");
            if (!string.Equals(roundTrip.captureId, captureId, StringComparison.Ordinal)
                || roundTrip.shots.Length != 1
                || !string.Equals(shot.id, "g07", StringComparison.Ordinal)
                || !string.Equals(
                    shot.scenePath,
                    AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    StringComparison.Ordinal)
                || shot.startFrame != 0
                || shot.endFrame != 779
                || shot.expectedFrameCount != 780
                || !string.Equals(shot.hudMode, "hud-on", StringComparison.Ordinal)
                || roundTrip.baselines.Length != 2
                || bl08.sourceFrame != 248
                || !string.Equals(bl08.shotId, "g07", StringComparison.Ordinal)
                || !string.Equals(bl08.hudMode, "hud-on", StringComparison.Ordinal)
                || !string.Equals(bl08.status, "captured", StringComparison.Ordinal)
                || !string.Equals(bl08.fileName,
                    AuditionPvStationPhase2PatternRelayCapture.Bl08FileName,
                    StringComparison.Ordinal)
                || bl09.sourceFrame != 598
                || !string.Equals(bl09.shotId, "g07", StringComparison.Ordinal)
                || !string.Equals(bl09.hudMode, "hud-on", StringComparison.Ordinal)
                || !string.Equals(bl09.status, "captured", StringComparison.Ordinal)
                || !string.Equals(bl09.fileName,
                    AuditionPvStationPhase2PatternRelayCapture.Bl09FileName,
                    StringComparison.Ordinal)
                || !shot.notes.Contains("windup f10/fire f68", StringComparison.Ordinal)
                || !shot.notes.Contains("windup f368/fire f418", StringComparison.Ordinal)
                || !shot.notes.Contains("f419", StringComparison.Ordinal)
                || !shot.notes.Contains("source f180..f599", StringComparison.Ordinal)
                || !shot.notes.Contains("SetMoveInput only", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G07 manifest failed its in-memory exact content round-trip.");
            }

            string proofPath = Path.Combine(
                    roundTrip.outputDirectory,
                    EvidenceFolderName,
                    RuntimeProofFileName)
                .Replace('\\', '/');
            string warmupPath = Path.Combine(
                    roundTrip.outputDirectory,
                    EvidenceFolderName,
                    WarmupEvidenceFileName)
                .Replace('\\', '/');
            string framesPath = Path.Combine(
                    roundTrip.outputDirectory,
                    "frames",
                    AuditionPvStationPhase2PatternRelayCapture.ShotId)
                .Replace('\\', '/');
            (string suite, string name, string artifact)[] expectedResults =
            {
                ("recorder", "raw-warmup-and-logical-remap", warmupPath),
                ("product-state", "phase2-curtain-hover-relay", proofPath),
                ("player-response", "movement-api-counterplay", proofPath),
                ("render", "qhd-hud-windup-pattern-telemetry", framesPath),
                ("render", "late-camera-frustum-evidence", proofPath),
                ("provenance", "clean-git-dependencies-and-byte-baselines", proofPath),
                ("lifecycle", "exhaustive-public-cleanup", proofPath)
            };
            string[] expectedGateTestNames = new[]
                {
                    "shot-authorship/"
                        + AuditionPvStationPhase2PatternRelayCapture.ShotId,
                    "shot-authorship-runtime/"
                        + AuditionPvStationPhase2PatternRelayCapture.ShotId
                }
                .Concat(
                    AuditionPvStationPhase2PatternRelayCapture
                        .GateSemanticBeatIds()
                        .Select(beatId => "semantic-beat/" + beatId))
                .ToArray();
            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator
                    .CaptureCoreSha256(roundTrip);
            int fixedResultCount = expectedResults.Length
                + expectedGateTestNames.Length;
            AuditionPvTestResult[] generatedEvidenceResults =
                GeneratedSixtySecondEvidenceResults(roundTrip);
            if (roundTrip.testResults.Length
                    != fixedResultCount + generatedEvidenceResults.Length
                || generatedEvidenceResults.Length != 0
                    && generatedEvidenceResults.Length != 7)
            {
                throw new InvalidOperationException(
                    "G07 manifest must contain the exact ordinary/Gate records and either zero or one complete generated 60-second evidence set.");
            }

            foreach ((string suite, string name, string artifact) expected in expectedResults)
            {
                AuditionPvTestResult result = roundTrip.testResults.SingleOrDefault(value =>
                    value != null
                    && string.Equals(value.name, expected.name, StringComparison.Ordinal));
                if (result == null
                    || !string.Equals(result.suite, expected.suite, StringComparison.Ordinal)
                    || !string.Equals(result.status, "passed", StringComparison.Ordinal)
                    || !string.Equals(
                        result.artifactPath?.Replace('\\', '/'),
                        expected.artifact,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "G07 manifest test result is missing, failed, or points at the wrong artifact: "
                    + expected.name);
                }
            }

            foreach (string expectedName in expectedGateTestNames)
            {
                AuditionPvTestResult[] matches = roundTrip.testResults
                    .Where(result => result != null
                        && string.Equals(
                            result.suite,
                            AuditionPvStationPhase2PatternRelayCapture
                                .GateEvidenceTestSuite,
                            StringComparison.Ordinal)
                        && string.Equals(
                            result.name,
                            expectedName,
                            StringComparison.Ordinal)
                        && string.Equals(
                            result.status,
                            "passed",
                            StringComparison.Ordinal))
                    .ToArray();
                if (matches.Length != 1
                    || string.IsNullOrWhiteSpace(matches[0].artifactPath)
                    || !File.Exists(matches[0].artifactPath)
                    || !matches[0].details.Contains(
                        "artifact-sha256=" + AuditionPvSha256.FileHash(
                            matches[0].artifactPath),
                        StringComparison.Ordinal)
                    || !matches[0].details.Contains(
                        "capture-core-sha256=" + captureCoreSha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G07 Gate test result is missing, duplicated, or unpinned: "
                        + expectedName);
                }
            }

            ValidateGeneratedSixtySecondEvidenceResults(
                roundTrip,
                generatedEvidenceResults,
                captureCoreSha256,
                S070SourceRangeStartFrame,
                S070SourceRangeEndFrame);
        }

        private static AuditionPvTestResult[] GeneratedSixtySecondEvidenceResults(
            AuditionPvCaptureManifest manifest)
        {
            string[] names =
            {
                "contact-sheet",
                "missing-frame",
                "error-magenta",
                "resolution",
                "rec709",
                "renderer-material-scan",
                "renderer-material-scan/runtime-workload"
            };
            return (manifest?.testResults ?? Array.Empty<AuditionPvTestResult>())
                .Where(result => result != null
                    && string.Equals(
                        result.suite,
                        AuditionPvStationPhase2PatternRelayCapture
                            .GateEvidenceTestSuite,
                        StringComparison.Ordinal)
                    && names.Contains(result.name, StringComparer.Ordinal))
                .ToArray();
        }

        private static void ValidateGeneratedSixtySecondEvidenceResults(
            AuditionPvCaptureManifest manifest,
            AuditionPvTestResult[] results,
            string captureCoreSha256,
            int sourceRangeStartFrame,
            int sourceRangeEndFrame)
        {
            results ??= Array.Empty<AuditionPvTestResult>();
            if (results.Length == 0)
            {
                return;
            }

            string[] expectedNames =
            {
                "contact-sheet",
                "missing-frame",
                "error-magenta",
                "resolution",
                "rec709",
                "renderer-material-scan",
                "renderer-material-scan/runtime-workload"
            };
            string rangeToken = $"source-range={sourceRangeStartFrame}-{sourceRangeEndFrame}";
            string outputRoot = Path.GetFullPath(manifest.outputDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            foreach (string expectedName in expectedNames)
            {
                AuditionPvTestResult[] matches = results
                    .Where(result => string.Equals(
                        result.name,
                        expectedName,
                        StringComparison.Ordinal))
                    .ToArray();
                if (matches.Length != 1)
                {
                    throw new InvalidOperationException(
                        "G07 generated evidence test is missing or duplicated: "
                        + expectedName);
                }

                AuditionPvTestResult result = matches[0];
                string artifactPath = Path.GetFullPath(result.artifactPath ?? string.Empty);
                bool valid = string.Equals(result.status, "passed", StringComparison.Ordinal)
                    && result.durationMilliseconds >= 0
                    && artifactPath.StartsWith(outputRoot, StringComparison.OrdinalIgnoreCase)
                    && File.Exists(artifactPath);
                if (valid)
                {
                    string artifactSha256 = AuditionPvSha256.FileHash(artifactPath);
                    valid = result.details != null
                        && result.details.Contains(
                            "artifact-sha256=" + artifactSha256,
                            StringComparison.Ordinal)
                        && result.details.Contains(
                            "capture-core-sha256=" + captureCoreSha256,
                            StringComparison.Ordinal)
                        && result.details.Contains("source-shot=g07", StringComparison.Ordinal)
                        && result.details.Contains(rangeToken, StringComparison.Ordinal);
                }

                if (!valid)
                {
                    throw new InvalidOperationException(
                        "G07 generated evidence test is unpinned or range-mismatched: "
                        + expectedName);
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
                throw new InvalidDataException(
                    $"G07 frame sequence has {files.Length} files; expected {expectedCount}.");
            }

            for (int index = 0; index < expectedCount; index++)
            {
                if (!string.Equals(
                    Path.GetFileName(files[index]),
                    expectedName(index),
                    StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "G07 frame sequence has a gap or unexpected name at index "
                        + index + ".");
                }
            }
        }

        private static string RequireDirectory(string path)
        {
            string full = Path.GetFullPath(path);
            if (!Directory.Exists(full))
            {
                throw new DirectoryNotFoundException(full);
            }

            return full;
        }

        private static void MoveNew(string source, string destination)
        {
            if (!File.Exists(source) || File.Exists(destination))
            {
                throw new IOException(
                    "G07 immutable move requires an existing source and absent destination: "
                    + source + " -> " + destination);
            }

            File.Move(source, destination);
        }

        private static void CopyNew(string source, string destination)
        {
            if (!File.Exists(source) || File.Exists(destination))
            {
                throw new IOException(
                    "G07 immutable copy requires an existing source and absent destination: "
                    + source + " -> " + destination);
            }

            Directory.CreateDirectory(
                Path.GetDirectoryName(destination)
                    ?? throw new InvalidOperationException("Destination has no parent."));
            File.Copy(source, destination, overwrite: false);
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
            string root = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve project root.");
            return Path.GetFullPath(Path.Combine(root, projectRelativePath));
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
            SessionState.EraseString(SessionStatePathKey);
            SessionState.EraseString(SessionOwnerKey);
            SessionState.EraseBool(SessionBatchKey);
            SessionState.EraseString(SessionOutputDirectoryKey);
            SessionState.EraseString(SessionCaptureIdKey);
            SessionState.EraseString(SessionTerminalFaultKey);
        }

        private static RunnerPhase ParsePhase(string value)
        {
            if (!Enum.TryParse(value, ignoreCase: false, out RunnerPhase phase)
                || !Enum.IsDefined(typeof(RunnerPhase), phase))
            {
                throw new InvalidDataException("Unknown G07 runner phase: " + value);
            }

            return phase;
        }

        private static PersistedRunnerState LoadState(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException("G07 runner state is missing.", path);
            }

            PersistedRunnerState state = JsonUtility.FromJson<PersistedRunnerState>(
                File.ReadAllText(path, Encoding.UTF8));
            if (state == null
                || !string.Equals(state.schema, RunnerSchema, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(state.captureId)
                || string.IsNullOrWhiteSpace(state.outputDirectory))
            {
                throw new InvalidDataException("G07 runner state is invalid.");
            }

            ValidatePersistedStateLocationForRoot(
                path,
                state,
                AuditionPvCaptureContract.OutputRoot);
            return state;
        }

        internal static void ValidateSessionRecoveryLocationForRoot(
            string statePath,
            string outputDirectory,
            string captureId,
            string authorizedOutputRoot)
        {
            ValidateCanonicalCaptureLocationForRoot(
                outputDirectory,
                captureId,
                authorizedOutputRoot);
            string expectedStatePath = Path.Combine(
                Path.GetFullPath(outputDirectory),
                StateFileName);
            if (!PathsEqual(expectedStatePath, statePath))
            {
                throw new InvalidDataException(
                    "G07 SessionState path is not the canonical capture state path.");
            }
        }

        internal static void ValidatePersistedStateLocationForRoot(
            string statePath,
            PersistedRunnerState state,
            string authorizedOutputRoot)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            ParsePhase(state.phase);
            ValidateCanonicalCaptureLocationForRoot(
                state.outputDirectory,
                state.captureId,
                authorizedOutputRoot);
            if (!PathsEqual(state.outputRoot, authorizedOutputRoot)
                || !PathsEqual(
                    state.baselineDirectory,
                    Path.Combine(
                        state.outputDirectory,
                        AuditionPvStationPhase2PatternRelayCapture.BaselinesFolderName))
                || !PathsEqual(
                    statePath,
                    Path.Combine(state.outputDirectory, StateFileName)))
            {
                throw new InvalidDataException(
                    "G07 runner state paths are outside the canonical configured capture layout.");
            }
        }

        internal static void ValidateSessionBatchAuthority(
            bool sessionBatchMode,
            PersistedRunnerState state)
        {
            if (state == null || state.batchMode != sessionBatchMode)
            {
                throw new InvalidDataException(
                    "G07 runner state batch mode differs from authoritative SessionState.");
            }
        }

        private static void ValidateCanonicalCaptureLocationForRoot(
            string outputDirectory,
            string captureId,
            string authorizedOutputRoot)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory)
                || string.IsNullOrWhiteSpace(captureId)
                || string.IsNullOrWhiteSpace(authorizedOutputRoot))
            {
                throw new InvalidDataException(
                    "G07 canonical capture location tokens are incomplete.");
            }

            AuditionPvOutputPaths.ValidateOutputId(captureId);
            string root = Path.GetFullPath(authorizedOutputRoot).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string expected = Path.GetFullPath(
                    AuditionPvOutputPaths.ResolveOutputDirectory(root, captureId))
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            string actual = Path.GetFullPath(outputDirectory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            if (!PathsEqual(actual, expected)
                || !PathsEqual(Path.GetDirectoryName(actual), root))
            {
                throw new InvalidDataException(
                    "G07 capture output is not the configured root's exact direct child.");
            }
        }

        private static bool PathsEqual(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            return string.Equals(
                Path.GetFullPath(left).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }

        private static void SaveState(string path, PersistedRunnerState state)
        {
            string json = JsonUtility.ToJson(state, true) + Environment.NewLine;
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(temporary, json, new UTF8Encoding(false));
                if (File.Exists(path))
                {
                    File.Replace(temporary, path, null);
                }
                else
                {
                    File.Move(temporary, path);
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

        internal static Exception ExecuteTerminalHandoff(
            Action persist,
            Action<Exception> recordPersistenceFailure,
            Action forceTerminalExit)
        {
            Exception failure = null;
            try
            {
                persist?.Invoke();
            }
            catch (Exception persistenceFailure)
            {
                failure = persistenceFailure;
                try
                {
                    recordPersistenceFailure?.Invoke(persistenceFailure);
                }
                catch (Exception recordFailure)
                {
                    failure = new AggregateException(failure, recordFailure);
                }
            }
            finally
            {
                try
                {
                    forceTerminalExit?.Invoke();
                }
                catch (Exception exitFailure)
                {
                    failure = failure == null
                        ? exitFailure
                        : new AggregateException(failure, exitFailure);
                }
            }

            return failure;
        }

        internal static Exception RecoverTerminalPersistenceFaultForRoot(
            string outputDirectory,
            string captureId,
            string authorizedOutputRoot,
            string terminalFault,
            Action clearSession,
            Action<int> requestExit)
        {
            Exception failure = null;
            try
            {
                ValidateCanonicalCaptureLocationForRoot(
                    outputDirectory,
                    captureId,
                    authorizedOutputRoot);
                var state = new PersistedRunnerState
                {
                    schema = RunnerSchema,
                    phase = RunnerPhase.FailedInPlayMode.ToString(),
                    captureId = captureId,
                    outputRoot = authorizedOutputRoot,
                    outputDirectory = outputDirectory,
                    baselineDirectory = Path.Combine(
                        outputDirectory,
                        AuditionPvStationPhase2PatternRelayCapture.BaselinesFolderName)
                };
                string cleanupFailure =
                    DeleteUncommittedSuccessArtifactsForRoot(
                        outputDirectory,
                        state,
                        authorizedOutputRoot);
                string failurePath = Path.Combine(outputDirectory, FailureFileName);
                if (!File.Exists(failurePath))
                {
                    WriteJsonNew(failurePath, new FailureArtifact
                    {
                        schema = FailureSchema,
                        createdAtUtc = DateTime.UtcNow.ToString("O"),
                        phase = "playmode-terminal-persistence-resume",
                        exception =
                            "G07 PlayMode terminal state persistence failed; stale disk state was not resumed.\n"
                            + (terminalFault ?? string.Empty),
                        captureId = captureId,
                        outputDirectory = outputDirectory.Replace('\\', '/'),
                        retainedArtifacts =
                            "Failure-only: raw/logical frames and measurement telemetry retained; success artifacts removed.",
                        pixelCalibrationLocked = PatternPixelCalibrationLocked,
                        successArtifactCleanupFailure = cleanupFailure
                    });
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                try
                {
                    clearSession?.Invoke();
                }
                catch (Exception clearFailure)
                {
                    failure = failure == null
                        ? clearFailure
                        : new AggregateException(failure, clearFailure);
                }

                try
                {
                    requestExit?.Invoke(1);
                }
                catch (Exception exitFailure)
                {
                    failure = failure == null
                        ? exitFailure
                        : new AggregateException(failure, exitFailure);
                }
            }

            return failure;
        }

        private static void WriteJsonNew<T>(string path, T value)
        {
            if (File.Exists(path))
            {
                throw new IOException("G07 evidence is immutable: " + path);
            }

            Directory.CreateDirectory(
                Path.GetDirectoryName(path)
                    ?? throw new InvalidOperationException("Evidence path has no parent."));
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(
                    JsonUtility.ToJson(value, true) + Environment.NewLine);
                using (var stream = new FileStream(
                    temporary,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(flushToDisk: true);
                }

                File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        }

        private static void WriteTextNew(string path, string value)
        {
            if (File.Exists(path))
            {
                throw new IOException("G07 evidence is immutable: " + path);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("Evidence path has no parent."));
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(value);
                using (var stream = new FileStream(
                    temporary,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(flushToDisk: true);
                }

                File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        }

        internal static string BuildFrameHashLedger(string frameDirectory)
        {
            var builder = new StringBuilder();
            for (int frame = 0;
                frame < AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount;
                frame++)
            {
                string name = AuditionPvStationPhase2PatternRelayCapture
                    .FrameFileName(frame);
                builder.Append(AuditionPvSha256.FileHash(
                        Path.Combine(frameDirectory, name)))
                    .Append("  ")
                    .Append(
                        AuditionPvStationPhase2PatternRelayCapture
                            .FrameLedgerRelativePath(frame))
                    .Append('\n');
            }

            return builder.ToString();
        }

        internal static void ValidateFrameHashLedger(
            string frameDirectory,
            string ledgerPath,
            string expectedLedgerSha256)
        {
            string ledger = File.ReadAllText(ledgerPath);
            if (!string.Equals(
                    AuditionPvSha256.TextHash(ledger),
                    expectedLedgerSha256,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ledger,
                    BuildFrameHashLedger(frameDirectory),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G07 canonical 780-frame SHA-256 ledger changed.");
            }
        }

        private static void TryWriteFailureArtifact(
            string outputDirectory,
            string phase,
            Exception exception,
            RuntimeProof proof,
            PersistedRunnerState state)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory)
                || !Directory.Exists(outputDirectory))
            {
                return;
            }

            AuditionPvGitSnapshot failureGit;
            try
            {
                failureGit = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            }
            catch (Exception gitFailure)
            {
                failureGit = new AuditionPvGitSnapshot
                {
                    probeSucceeded = false,
                    probeError = gitFailure.ToString()
                };
            }

            try
            {
                WriteFailureArtifactForRoot(
                    outputDirectory,
                    phase,
                    exception,
                    proof,
                    state,
                    AuditionPvCaptureContract.OutputRoot,
                    failureGit,
                    PatternPixelCalibrationLocked);
            }
            catch (Exception writeFailure)
            {
                Debug.LogException(writeFailure);
            }
        }

        internal static void WriteFailureArtifactForRoot(
            string outputDirectory,
            string phase,
            Exception exception,
            RuntimeProof proof,
            PersistedRunnerState state,
            string authorizedRoot,
            AuditionPvGitSnapshot failureGit,
            bool pixelCalibrationLocked)
        {
            if (state == null
                || !PathsEqual(state.outputRoot, authorizedRoot))
            {
                throw new InvalidDataException(
                    "G07 failure artifact requires trusted canonical session identity.");
            }

            ValidateCanonicalCaptureLocationForRoot(
                outputDirectory,
                state.captureId,
                authorizedRoot);
            if (!PathsEqual(outputDirectory, state.outputDirectory))
            {
                throw new InvalidDataException(
                    "G07 failure artifact output differs from runner state.");
            }

            if (IsValidCommittedManifestAt(
                outputDirectory,
                state.captureId,
                state,
                authorizedRoot))
            {
                return;
            }

            string successArtifactCleanupFailure =
                DeleteUncommittedSuccessArtifactsForRoot(
                    outputDirectory,
                    state,
                    authorizedRoot);
            string failure = Path.Combine(outputDirectory, FailureFileName);
            if (File.Exists(failure))
            {
                return;
            }

            WriteJsonNew(failure, new FailureArtifact
            {
                schema = FailureSchema,
                createdAtUtc = DateTime.UtcNow.ToString("O"),
                phase = phase ?? string.Empty,
                exception = exception?.ToString() ?? string.Empty,
                captureId = state.captureId ?? string.Empty,
                outputDirectory = outputDirectory.Replace('\\', '/'),
                startGitCommitSha = state.gitCommitSha ?? string.Empty,
                startGitBranch = state.gitBranch ?? string.Empty,
                startGitDirty = state.gitWorktreeDirty,
                startGitDirtyHashSha256 = state.gitDirtyHashSha256
                    ?? string.Empty,
                failureGit = failureGit,
                engine = state.engine,
                dependencyHashesAtStart = state.dependencyHashesAtStart
                    ?? Array.Empty<AuditionPvDependencyHash>(),
                retainedArtifacts =
                    "Failure-only: raw/logical frames, runner state, and measurement telemetry are retained; manifest, canonical baselines, and success runtime-proof artifact are absent.",
                pixelCalibrationLocked = pixelCalibrationLocked,
                successArtifactCleanupFailure = successArtifactCleanupFailure,
                runtime = proof
            });
        }

        private static bool IsValidCommittedManifest(PersistedRunnerState state)
        {
            if (state == null
                || string.IsNullOrWhiteSpace(state.outputDirectory)
                || string.IsNullOrWhiteSpace(state.captureId))
            {
                return false;
            }

            return IsValidCommittedManifestAt(
                state.outputDirectory,
                state.captureId,
                state,
                AuditionPvCaptureContract.OutputRoot);
        }

        private static bool IsValidCommittedManifestAt(
            string outputDirectory,
            string captureId,
            PersistedRunnerState state,
            string authorizedOutputRoot)
        {
            try
            {
                ValidateCanonicalCaptureLocationForRoot(
                    outputDirectory,
                    captureId,
                    authorizedOutputRoot);
            }
            catch
            {
                return false;
            }

            string path = Path.Combine(
                outputDirectory,
                AuditionPvCaptureContract.ManifestFileName);
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                AuditionPvCaptureManifest manifest =
                    JsonUtility.FromJson<AuditionPvCaptureManifest>(
                        File.ReadAllText(path));
                ValidateManifestInMemory(manifest, captureId);
                if (!PathsEqual(manifest.outputRoot, authorizedOutputRoot)
                    || !PathsEqual(manifest.outputDirectory, outputDirectory))
                {
                    return false;
                }

                if (state != null)
                {
                    ValidateManifestMatchesRecordedState(state, manifest);
                }

                ValidateCommittedArtifacts(
                    outputDirectory,
                    captureId,
                    state,
                    manifest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ValidateCommittedArtifacts(
            string outputDirectory,
            string captureId,
            PersistedRunnerState state,
            AuditionPvCaptureManifest manifest)
        {
            string output = Path.GetFullPath(outputDirectory);
            string failure = Path.Combine(output, FailureFileName);
            if (File.Exists(failure))
            {
                throw new InvalidOperationException(
                    "A committed G07 manifest cannot coexist with failure evidence.");
            }

            string frames = Path.Combine(output, "frames", "g07");
            ValidateLogicalFrameSequence(frames);
            for (int frame =
                     AuditionPvStationPhase2PatternRelayCapture.FirstFrame;
                frame <= AuditionPvStationPhase2PatternRelayCapture.LastFrame;
                frame++)
            {
                ValidatePngFile(
                    Path.Combine(frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(frame)),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
            }

            string evidence = Path.Combine(output, EvidenceFolderName);
            string warmup = Path.Combine(evidence, WarmupEvidenceFileName);
            ValidateDecodablePngFile(
                warmup,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            string proofPath = Path.Combine(evidence, RuntimeProofFileName);
            if (!File.Exists(proofPath))
            {
                throw new FileNotFoundException(
                    "Committed G07 runtime proof is missing.",
                    proofPath);
            }

            RuntimeProofArtifact artifact = JsonUtility.FromJson<RuntimeProofArtifact>(
                File.ReadAllText(proofPath));
            if (artifact == null
                || !string.Equals(artifact.schema, RuntimeProofSchema, StringComparison.Ordinal)
                || !string.Equals(artifact.captureId, captureId, StringComparison.Ordinal)
                || artifact.runtime == null)
            {
                throw new InvalidDataException(
                    "Committed G07 runtime proof identity is invalid.");
            }

            RuntimeProof proof = artifact.runtime;
            ValidateRuntimeProof(proof);
            string ledgerPath = Path.Combine(evidence, FrameHashLedgerFileName);
            if (!string.Equals(
                    Path.GetFullPath(proof.frameHashLedgerPath),
                    Path.GetFullPath(ledgerPath),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Committed G07 frame ledger path is not canonical.");
            }

            ValidateFrameHashLedger(
                frames,
                ledgerPath,
                proof.frameHashLedgerSha256);
            string baselineRoot = Path.Combine(output,
                AuditionPvStationPhase2PatternRelayCapture.BaselinesFolderName);
            string bl08 = Path.Combine(baselineRoot,
                AuditionPvStationPhase2PatternRelayCapture.Bl08FileName);
            string bl09 = Path.Combine(baselineRoot,
                AuditionPvStationPhase2PatternRelayCapture.Bl09FileName);
            ValidatePngFile(bl08, AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            ValidatePngFile(bl09, AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            string frame68 = Path.Combine(frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .Bl08SourceFrame));
            string frame418 = Path.Combine(frames,
                AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                    AuditionPvStationPhase2PatternRelayCapture
                        .Bl09SourceFrame));
            if (!string.Equals(AuditionPvSha256.FileHash(warmup),
                    proof.warmupEvidenceSha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(frame68),
                    proof.bl08Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(frame418),
                    proof.bl09Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(bl08),
                    proof.bl08Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(bl09),
                    proof.bl09Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(Path.Combine(frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                            AuditionPvStationPhase2PatternRelayCapture
                                .LogicalToSourceFrame(66)))),
                    proof.frame66Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(Path.Combine(frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                            AuditionPvStationPhase2PatternRelayCapture
                                .LogicalToSourceFrame(67)))),
                    proof.frame67Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(Path.Combine(frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                            AuditionPvStationPhase2PatternRelayCapture
                                .LogicalToSourceFrame(416)))),
                    proof.frame416Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(Path.Combine(frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                            AuditionPvStationPhase2PatternRelayCapture
                                .LogicalToSourceFrame(417)))),
                    proof.frame417Sha256, StringComparison.Ordinal)
                || !string.Equals(AuditionPvSha256.FileHash(Path.Combine(frames,
                        AuditionPvStationPhase2PatternRelayCapture.FrameFileName(
                            AuditionPvStationPhase2PatternRelayCapture
                                .LogicalToSourceFrame(419)))),
                    proof.frame419Sha256, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Committed G07 frame, warmup, neighbor, final-hero, or baseline bytes changed.");
            }

            AuditionPvDependencyHash[] recorded = manifest.dependencyHashes
                ?? Array.Empty<AuditionPvDependencyHash>();
            if (recorded.Length != proof.dependencyHashCount
                || recorded.Length == 0
                || recorded.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.path)
                    || !value.exists
                    || value.byteLength < 0
                    || !AuditionPvSha256.IsSha256(value.sha256))
                || recorded.Select(value => value.path).Distinct(
                    StringComparer.OrdinalIgnoreCase).Count() != recorded.Length)
            {
                throw new InvalidOperationException(
                    "Committed G07 dependency snapshot is incomplete or malformed.");
            }

            ValidateRecordedCommittedProvenance(
                outputDirectory,
                captureId,
                manifest,
                proof,
                recorded);
        }

        private static void ValidateRecordedCommittedProvenance(
            string outputDirectory,
            string captureId,
            AuditionPvCaptureManifest manifest,
            RuntimeProof proof,
            AuditionPvDependencyHash[] recorded)
        {
            if (!DateTime.TryParse(
                    manifest.createdAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime createdAt)
                || !string.Equals(
                    manifest.createdAtUtc,
                    createdAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                || !captureId.StartsWith(
                    createdAt.ToUniversalTime().ToString(
                        "yyyyMMdd't'HHmmss'z'_",
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                || !string.Equals(
                    manifest.worktreeDirtyHashAlgorithm,
                    AuditionPvGitSnapshot.DirtyHashAlgorithm,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Committed G07 timestamp/hash-algorithm provenance is not canonical.");
            }

            ValidateExactEngineProvenance(
                manifest.unityVersion,
                manifest.unityVersionWithRevision,
                manifest.recorderPackageVersion,
                manifest.urpPackageVersion,
                manifest.activeRenderPipelineAssetPath);
            if (manifest.gitWorktreeDirty
                || string.IsNullOrWhiteSpace(manifest.gitBranch)
                || !string.Equals(
                    manifest.gitBranch,
                    manifest.gitBranch.Trim(),
                    StringComparison.Ordinal)
                || string.Equals(manifest.gitBranch, "HEAD", StringComparison.OrdinalIgnoreCase)
                || manifest.gitCommitSha == null
                || manifest.gitCommitSha.Length != 40
                || manifest.gitCommitSha.Any(character =>
                    !(character >= '0' && character <= '9'
                        || character >= 'a' && character <= 'f')))
            {
                throw new InvalidOperationException(
                    "Committed G07 clean Git provenance is invalid.");
            }

            var byPath = recorded.ToDictionary(
                value => value.path,
                StringComparer.OrdinalIgnoreCase);
            var required = new HashSet<string>(
                AuditionPvCaptureContract.CoreDependencyPaths,
                StringComparer.OrdinalIgnoreCase)
            {
                RunnerScriptPath,
                RunnerScriptPath + ".meta",
                RunnerTestPath,
                RunnerTestPath + ".meta"
            };
            foreach (string path in
                     AuditionPvStationPhase2PatternRelayCapture
                         .ExplicitProductDependencyPaths())
            {
                required.Add(path);
                if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    required.Add(path + ".meta");
                }
            }

            if (required.Any(path => !byPath.ContainsKey(path))
                || !byPath.ContainsKey(manifest.activeRenderPipelineAssetPath)
                || !recorded.Any(value => value.path.StartsWith(
                    "Packages/com.unity.render-pipelines.universal/",
                    StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Committed G07 dependency snapshot lacks the exact direct/core/meta/URP closure.");
            }

            AuditionPvDependencyHash station = byPath[
                AuditionPvStationPhase2PatternRelayCapture.StationScenePath];
            if (!string.Equals(
                    station.sha256,
                    proof.stationSceneSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Committed G07 Station dependency hash is not bound to runtime proof.");
            }

            var engine = new PersistedEngineSnapshot
            {
                unityVersion = manifest.unityVersion,
                unityVersionWithRevision = manifest.unityVersionWithRevision,
                recorderPackageVersion = manifest.recorderPackageVersion,
                urpPackageVersion = manifest.urpPackageVersion,
                activeRenderPipelineAssetPath = manifest.activeRenderPipelineAssetPath
            };
            string digest = ComputeCaptureStartProvenanceSha256(
                captureId,
                manifest.createdAtUtc,
                manifest.outputRoot,
                outputDirectory,
                manifest.gitCommitSha,
                manifest.gitBranch,
                manifest.gitWorktreeDirty,
                manifest.worktreeDirtyHashSha256,
                manifest.worktreeDirtyHashAlgorithm,
                engine,
                recorded);
            if (!string.Equals(
                digest,
                proof.captureStartProvenanceSha256,
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Committed G07 manifest provenance digest is not bound to runtime proof.");
            }
        }

        private static void ValidateManifestMatchesRecordedState(
            PersistedRunnerState state,
            AuditionPvCaptureManifest manifest)
        {
            if (!string.Equals(manifest.captureId, state.captureId, StringComparison.Ordinal)
                || !PathsEqual(manifest.outputRoot, state.outputRoot)
                || !PathsEqual(manifest.outputDirectory, state.outputDirectory)
                || !string.Equals(manifest.gitCommitSha, state.gitCommitSha, StringComparison.Ordinal)
                || !string.Equals(manifest.gitBranch, state.gitBranch, StringComparison.Ordinal)
                || manifest.gitWorktreeDirty != state.gitWorktreeDirty
                || !string.Equals(manifest.worktreeDirtyHashSha256,
                    state.gitDirtyHashSha256, StringComparison.Ordinal)
                || !string.Equals(
                    manifest.worktreeDirtyHashAlgorithm,
                    AuditionPvGitSnapshot.DirtyHashAlgorithm,
                    StringComparison.Ordinal)
                || !DateTime.TryParse(
                    state.startedAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime stateStarted)
                || !string.Equals(
                    manifest.createdAtUtc,
                    stateStarted.ToUniversalTime().ToString(
                        "O",
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                || state.engine == null
                || !string.Equals(manifest.unityVersion,
                    state.engine.unityVersion, StringComparison.Ordinal)
                || !string.Equals(manifest.unityVersionWithRevision,
                    state.engine.unityVersionWithRevision, StringComparison.Ordinal)
                || !string.Equals(manifest.recorderPackageVersion,
                    state.engine.recorderPackageVersion, StringComparison.Ordinal)
                || !string.Equals(manifest.urpPackageVersion,
                    state.engine.urpPackageVersion, StringComparison.Ordinal)
                || !string.Equals(manifest.activeRenderPipelineAssetPath,
                    state.engine.activeRenderPipelineAssetPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Committed G07 manifest provenance differs from its capture-start state.");
            }

            ValidateStableDependencies(
                state.dependencyHashesAtStart,
                manifest.dependencyHashes);
            string[] statePaths = state.dependencyPaths ?? Array.Empty<string>();
            string[] manifestPaths = manifest.dependencyHashes?
                .Select(value => value.path)
                .ToArray() ?? Array.Empty<string>();
            if (!statePaths.SequenceEqual(
                    manifestPaths,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Committed G07 manifest dependency paths differ from capture-start state.");
            }
        }

        private static string DeleteUncommittedSuccessArtifacts(
            string outputDirectory,
            PersistedRunnerState state)
        {
            return DeleteUncommittedSuccessArtifactsForRoot(
                outputDirectory,
                state,
                AuditionPvCaptureContract.OutputRoot);
        }

        internal static string DeleteUncommittedSuccessArtifactsForRoot(
            string outputDirectory,
            PersistedRunnerState state,
            string authorizedOutputRoot)
        {
            try
            {
                return DeleteUncommittedSuccessArtifactsForRootCore(
                    outputDirectory,
                    state,
                    authorizedOutputRoot);
            }
            catch (Exception exception)
            {
                return "Refused G07 success-artifact cleanup: " + exception;
            }
        }

        private static string DeleteUncommittedSuccessArtifactsForRootCore(
            string outputDirectory,
            PersistedRunnerState state,
            string authorizedOutputRoot)
        {
            if (state == null)
            {
                return string.Empty;
            }

            string actualOutput = Path.GetFullPath(outputDirectory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string stateOutput = Path.GetFullPath(state.outputDirectory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string authorizedRoot = Path.GetFullPath(
                    authorizedOutputRoot)
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            string stateRoot = Path.GetFullPath(state.outputRoot).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            string expectedOutput = Path.GetFullPath(
                    AuditionPvOutputPaths.ResolveOutputDirectory(
                        state.outputRoot,
                        state.captureId))
                .TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            if (!string.Equals(actualOutput, stateOutput,
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(stateRoot, authorizedRoot,
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(actualOutput, expectedOutput,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Refused success-artifact cleanup outside the canonical capture output.";
            }

            string baselineRoot = Path.GetFullPath(Path.Combine(
                actualOutput,
                AuditionPvStationPhase2PatternRelayCapture.BaselinesFolderName));
            if (!baselineRoot.StartsWith(
                    actualOutput + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Refused non-contained G07 baseline cleanup target.";
            }

            var ownedSuccessArtifacts = new List<string>
            {
                Path.Combine(
                    outputDirectory,
                    AuditionPvCaptureContract.ManifestFileName),
                Path.Combine(outputDirectory, EvidenceFolderName, RuntimeProofFileName),
                Path.Combine(outputDirectory, EvidenceFolderName, FrameHashLedgerFileName),
                Path.Combine(outputDirectory, EvidenceFolderName,
                    GateShotAuthorshipFileName),
                Path.Combine(
                    baselineRoot,
                    AuditionPvStationPhase2PatternRelayCapture.Bl08FileName),
                Path.Combine(
                    baselineRoot,
                    AuditionPvStationPhase2PatternRelayCapture.Bl09FileName)
            };
            ownedSuccessArtifacts.AddRange(
                AuditionPvStationPhase2PatternRelayCapture
                    .GateSemanticBeatIds()
                    .Select(beatId => Path.Combine(
                        outputDirectory,
                        EvidenceFolderName,
                        GateSemanticEvidenceFolderName,
                        beatId + ".json")));
            Exception failure = null;
            foreach (string path in ownedSuccessArtifacts)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception exception)
                {
                    failure = failure == null
                        ? exception
                        : new AggregateException(failure, exception);
                }
            }

            return failure?.ToString() ?? string.Empty;
        }

        private static AuditionPvGitSnapshot CreateGitSnapshot(
            PersistedRunnerState state)
        {
            return new AuditionPvGitSnapshot
            {
                probeSucceeded = true,
                commitSha = state.gitCommitSha,
                branch = state.gitBranch,
                isDirty = state.gitWorktreeDirty,
                dirtyStateHashSha256 = state.gitDirtyHashSha256
            };
        }

        internal static string ComputeCaptureStartProvenanceSha256(
            string captureId,
            string startedAtUtc,
            string outputRoot,
            string outputDirectory,
            string gitCommitSha,
            string gitBranch,
            bool gitWorktreeDirty,
            string gitDirtyHashSha256,
            string gitDirtyHashAlgorithm,
            PersistedEngineSnapshot engine,
            AuditionPvDependencyHash[] dependencies)
        {
            var canonical = new StringBuilder();
            void Append(string value)
            {
                string normalized = value ?? string.Empty;
                canonical.Append(normalized.Length.ToString(CultureInfo.InvariantCulture));
                canonical.Append(':');
                canonical.Append(normalized);
                canonical.Append('\n');
            }

            Append("dimension-brawl.audition-pv.g07-start-provenance.v1");
            Append(captureId);
            Append(startedAtUtc);
            Append(Path.GetFullPath(outputRoot).Replace('\\', '/').TrimEnd('/'));
            Append(Path.GetFullPath(outputDirectory).Replace('\\', '/').TrimEnd('/'));
            Append(gitCommitSha);
            Append(gitBranch);
            Append(gitWorktreeDirty ? "1" : "0");
            Append(gitDirtyHashSha256);
            Append(gitDirtyHashAlgorithm);
            Append(engine?.unityVersion);
            Append(engine?.unityVersionWithRevision);
            Append(engine?.recorderPackageVersion);
            Append(engine?.urpPackageVersion);
            Append(engine?.activeRenderPipelineAssetPath);
            AuditionPvDependencyHash[] values = dependencies
                ?? Array.Empty<AuditionPvDependencyHash>();
            Append(values.Length.ToString(CultureInfo.InvariantCulture));
            foreach (AuditionPvDependencyHash dependency in values)
            {
                Append(dependency?.path);
                Append(dependency != null && dependency.exists ? "1" : "0");
                Append((dependency?.byteLength ?? -1L).ToString(
                    CultureInfo.InvariantCulture));
                Append(dependency?.sha256);
            }

            return AuditionPvSha256.TextHash(canonical.ToString());
        }

        private static PersistedEngineSnapshot CopyEngine(
            AuditionPvEngineSnapshot source)
        {
            return new PersistedEngineSnapshot
            {
                unityVersion = source.unityVersion,
                unityVersionWithRevision = source.unityVersionWithRevision,
                recorderPackageVersion = source.recorderPackageVersion,
                urpPackageVersion = source.urpPackageVersion,
                activeRenderPipelineAssetPath = source.activeRenderPipelineAssetPath
            };
        }

        private static AuditionPvEngineSnapshot RestoreEngine(
            PersistedEngineSnapshot source)
        {
            return new AuditionPvEngineSnapshot
            {
                unityVersion = source.unityVersion,
                unityVersionWithRevision = source.unityVersionWithRevision,
                recorderPackageVersion = source.recorderPackageVersion,
                urpPackageVersion = source.urpPackageVersion,
                activeRenderPipelineAssetPath = source.activeRenderPipelineAssetPath
            };
        }

        private enum RunnerPhase
        {
            AwaitingPlayMode,
            Recording,
            AwaitingEditMode,
            FailedInPlayMode
        }

        [Serializable]
        internal sealed class PersistedRunnerState
        {
            public string schema = string.Empty;
            public string phase = string.Empty;
            public bool batchMode;
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
            public string stationSceneSha256AtStart = string.Empty;
            public bool produceApprovedSixtySecondEvidence;
            public string s070RuntimeWorkloadSealPath = string.Empty;
            public RuntimeProof runtimeProof = new();
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
        internal sealed class RuntimeProof
        {
            public bool directorCompleted;
            public int lastLogicalFrame = -1;
            public int presentedFrameCount;
            public bool presentedFramesExact = true;
            public bool presentationClockExact = true;
            public int transitionCompletedEventCount;
            public int windupEventCount;
            public int waveEventCount;
            public int curtainWindupFrame = -1;
            public int curtainFireFrame = -1;
            public int curtainSpawnedCount;
            public bool curtainWasPriority;
            public int hoverWindupFrame = -1;
            public int hoverFireFrame = -1;
            public int hoverSpawnedCount;
            public bool hoverWasPriority;
            public int hoverSequenceIndexAfterFire = -1;
            public string curtainWindupPatternId = string.Empty;
            public string curtainFirePatternId = string.Empty;
            public string hoverWindupPatternId = string.Empty;
            public string hoverFirePatternId = string.Empty;
            public int emitterTickCount;
            public float minimumEmitterTimeScale = -1f;
            public float maximumEmitterTimeScale = -1f;
            public int runStartedCount;
            public int stopSettleCount;
            public int curtainMoveFirstAppliedFrame = -1;
            public int curtainMoveLastAppliedFrame = -1;
            public int curtainZeroAppliedFrame = -1;
            public int hoverMoveFirstAppliedFrame = -1;
            public int hoverMoveLastAppliedFrame = -1;
            public int hoverZeroAppliedFrame = -1;
            public float curtainRiskBefore = -1f;
            public float curtainRiskAfter = -1f;
            public bool stayedInsideForwardBoundary;
            public int hoverPreviewCount;
            public float hoverPreviewAverageLateral;
            public float hoverLateralDisplacement;
            public float hoverDirectionDot = -1f;
            public int visualWindupDelta;
            public int visualReleaseDelta;
            public int telegraphWindupDelta;
            public int telegraphReleaseDelta;
            public int cameraWindupDelta;
            public int cameraFireDelta;
            public int motionReleaseDelta;
            public int curtainWindupVisibleMarkerCount;
            public int curtainFireVisibleMarkerCount;
            public int hoverWindupVisibleMarkerCount;
            public int hoverFireVisibleMarkerCount;
            public int curtainWindupVisibleRendererCount;
            public int curtainFireVisibleRendererCount;
            public int hoverWindupVisibleRendererCount;
            public int hoverFireVisibleRendererCount;
            public Color curtainWindupMarkerColor;
            public Color curtainFireMarkerColor;
            public Color hoverWindupMarkerColor;
            public Color hoverFireMarkerColor;
            public int basicVolleyEventCount;
            public int pressureActionEventCount;
            public int enemySummonReleaseCountDelta;
            public int playerDamageEventCount;
            public int bossDamageEventCount;
            public int playerBasicStartedCount;
            public int playerBasicHitCount;
            public int dodgeStartedCount;
            public int dodgeEndedCount;
            public int perfectDodgeCount;
            public int summonUsedCount;
            public int summonBlockedCount;
            public int summonUseBlockedCount;
            public bool playerHealthUnchanged;
            public bool bossHealthUnchanged;
            public bool resourcesUnchanged;
            public bool exactHudAndBindings;
            public bool exactProjectileAndVfxBindings;
            public bool lifecycleEmergencyResetUsed;
            public string cleanupFailure = string.Empty;
            public int recorderWarmupEndOfFrameCount;
            public int recorderPreHandleEndOfFrameCount;
            public int canonicalSourceFrameCount;
            public int logicalFirstSourceFrame = -1;
            public int logicalLastSourceFrame = -1;
            public int recordedPreHandleFrameCount;
            public int recordedPostHandleFrameCount;
            public bool recorderPaddingActiveAtLogicalFrameZero;
            public float recorderCaptureDeltaTimeAtLogicalFrameZero;
            public bool recorderAutoStoppedAfterLastFrame;
            public bool stateRestored;
            public bool eventsReleased;
            public bool presentationClockReleased;
            public bool cadenceReleased;
            public bool emitterRestored;
            public bool spawnOriginOrderRestored;
            public bool playerStateRestored;
            public bool bossStateRestored;
            public bool cameraStateRestored;
            public bool hudStateRestored;
            public bool globalStateRestored;
            public int postRecordingSettleFrames;
            public float postRecordingSettleSeconds;
            public RenderEventEvidence[] renderEvents =
                Array.Empty<RenderEventEvidence>();
            public string stationScenePath = string.Empty;
            public string stationSceneSha256 = string.Empty;
            public string curtainProfilePath = string.Empty;
            public string curtainProfileGuid = string.Empty;
            public string hoverProfilePath = string.Empty;
            public string hoverProfileGuid = string.Empty;
            public int dependencyHashCount;
            public string captureStartProvenanceSha256 = string.Empty;
            public string warmupEvidencePath = string.Empty;
            public string warmupEvidenceSha256 = string.Empty;
            public string frame67Sha256 = string.Empty;
            public string frame66Sha256 = string.Empty;
            public string frame417Sha256 = string.Empty;
            public string frame416Sha256 = string.Empty;
            public string frame419Sha256 = string.Empty;
            public string frameHashLedgerPath = string.Empty;
            public string frameHashLedgerSha256 = string.Empty;
            public int frameHashLedgerEntryCount;
            public int[] pixelSampleSourceFrames = Array.Empty<int>();
            public string bl08Sha256 = string.Empty;
            public string bl09Sha256 = string.Empty;
            public SequenceVisualMetrics visualMetrics;
            public HudVisualMetrics hudMetrics;
            public FrameDeltaMetrics curtainWindupDelta;
            public FrameDeltaMetrics curtainFireDelta;
            public FrameDeltaMetrics curtainQuietDelta;
            public FrameDeltaMetrics hoverWindupDelta;
            public FrameDeltaMetrics hoverFireDelta;
            public FrameDeltaMetrics hoverQuietDelta;
            public PatternColorMetrics curtainWindupColors;
            public PatternColorMetrics hoverWindupColors;
            public bool telegraphMarkerCollidersNonBlocking;
        }

        [Serializable]
        internal sealed class SequenceVisualMetrics
        {
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
        }

        [Serializable]
        internal sealed class HudVisualMetrics
        {
            public int frameCount;
            public int minimumFramePinkSamples;
            public int maximumFramePinkSamples;
            public int minimumFrameDarkSamples;
            public int maximumFrameDarkSamples;
            public int minimumFrameBrightSamples;
            public int maximumFrameBrightSamples;
            public double minimumFrameMeanLuma;
            public double maximumFrameMeanLuma;
            public int roiX;
            public int roiY;
            public int roiWidth;
            public int roiHeight;
            public int sampleStride;
        }

        [Serializable]
        internal sealed class FrameDeltaMetrics
        {
            public long sampleCount;
            public long changedSampleCount;
            public double meanAbsoluteRgb;
            public double changedSampleRatio;
            public int roiX;
            public int roiY;
            public int roiWidth;
            public int roiHeight;
            public int sampleStride;
        }

        [Serializable]
        internal sealed class PatternColorMetrics
        {
            public long sampleCount;
            public long curtainGreenSampleCount;
            public long hoverCyanSampleCount;
            public int roiX;
            public int roiY;
            public int roiWidth;
            public int roiHeight;
            public int sampleStride;
        }

        [Serializable]
        internal sealed class SubjectViewportEvidence
        {
            public bool rendererBoundsFound;
            public bool frustumIntersects;
            public bool centerInFront;
            public bool centerInsideSafeViewport;
            public bool safeViewport;
            public Vector3 viewportCenter;
            public int pixelX;
            public int pixelY;
            public int pixelWidth;
            public int pixelHeight;
        }

        [Serializable]
        internal sealed class RenderEventEvidence
        {
            public int logicalFrame = -1;
            public bool cameraActiveAndEnabled;
            public bool cameraPerspective;
            public bool cameraFullRect;
            public bool cameraTargetTextureNull;
            public bool finalHeroComposition;
            public SubjectViewportEvidence player;
            public SubjectViewportEvidence boss;
            public int visibleMarkerCount;
            public int visibleMarkerRendererCount;
            public bool markerBoundsIntersectFrustum;
            public bool allMarkerRenderersIntersectFrustum;
            public SubjectViewportEvidence[] markers =
                Array.Empty<SubjectViewportEvidence>();
            public int markerPixelX;
            public int markerPixelY;
            public int markerPixelWidth;
            public int markerPixelHeight;
        }

        private sealed class GateSemanticBeatSpec
        {
            public GateSemanticBeatSpec(
                string beatId,
                int logicalStartFrame,
                int logicalEndFrame,
                params string[] exactFacts)
            {
                this.beatId = beatId;
                this.logicalStartFrame = logicalStartFrame;
                this.logicalEndFrame = logicalEndFrame;
                this.exactFacts = exactFacts ?? Array.Empty<string>();
            }

            public readonly string beatId;
            public readonly int logicalStartFrame;
            public readonly int logicalEndFrame;
            public readonly string[] exactFacts;
        }

        [Serializable]
        private sealed class GateSemanticBeatRuntimeArtifact
        {
            public string schemaVersion = string.Empty;
            public string sourceCaptureCoreSha256 = string.Empty;
            public string captureId = string.Empty;
            public string sourceShotId = string.Empty;
            public string beatId = string.Empty;
            public string runtimeFactKey = string.Empty;
            public int sourceRangeStartFrame;
            public int sourceRangeEndFrame;
            public int logicalFactStartFrame;
            public int logicalFactEndFrame;
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
            public string cadence = string.Empty;
            public string playerResponse = string.Empty;
            public RuntimeProof runtime;
        }

        [Serializable]
        private sealed class FailureArtifact
        {
            public string schema = string.Empty;
            public string createdAtUtc = string.Empty;
            public string phase = string.Empty;
            public string exception = string.Empty;
            public string captureId = string.Empty;
            public string outputDirectory = string.Empty;
            public string startGitCommitSha = string.Empty;
            public string startGitBranch = string.Empty;
            public bool startGitDirty;
            public string startGitDirtyHashSha256 = string.Empty;
            public AuditionPvGitSnapshot failureGit;
            public PersistedEngineSnapshot engine;
            public AuditionPvDependencyHash[] dependencyHashesAtStart =
                Array.Empty<AuditionPvDependencyHash>();
            public string retainedArtifacts = string.Empty;
            public bool pixelCalibrationLocked;
            public string successArtifactCleanupFailure = string.Empty;
            public RuntimeProof runtime;
        }
    }

    /// <summary>
    /// Recorder lifecycle owner. It executes before the product director and
    /// always drives the same coroutine cleanup for successful and failed takes.
    /// </summary>
    [DefaultExecutionOrder(-32500)]
    public sealed class AuditionPvStationPhase2PatternRelayGoldenRunnerBehaviour
        : MonoBehaviour
    {
        private const double ShotTimeoutSeconds = 240d;
        private string statePath;
        private string outputDirectory;
        private AuditionPvStationPhase2PatternRelayGoldenRunner.PersistedRunnerState state;
        private AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof proof;
        private AuditionPvStationPhase2PatternRelayDirector director;
        private AuditionPvStationPhase2PatternRelayRenderProbe renderProbe;
        private AuditionPvRecorderSettingsBundle recorderSettings;
        private RecorderController recorderController;
        private AuditionPvRuntimeWorkloadCaptureSession s070RuntimeWorkload;
        private Exception updateFailure;
        private bool armLogicalFrameZero;
        private bool beganLogicalShot;
        private bool cleaningUp;
        private bool notified;
        private int nextPresentedFrame;

        internal void Begin(
            string persistedStatePath,
            string captureOutputDirectory,
            AuditionPvStationPhase2PatternRelayGoldenRunner.PersistedRunnerState
                persistedState)
        {
            statePath = persistedStatePath;
            outputDirectory = captureOutputDirectory;
            state = persistedState;
            proof = persistedState.runtimeProof
                ?? new AuditionPvStationPhase2PatternRelayGoldenRunner.RuntimeProof();
            StartCoroutine(RunGuarded());
        }

        private void Update()
        {
            if (!armLogicalFrameZero || beganLogicalShot || updateFailure != null)
            {
                return;
            }

            armLogicalFrameZero = false;
            try
            {
                if (Time.timeScale != 1f)
                {
                    throw new InvalidOperationException(
                        "G07 logical f0 requires exact global time scale one.");
                }

                float minimumDelta = 1f / AuditionPvCaptureContract.Fps;
                proof.recorderCaptureDeltaTimeAtLogicalFrameZero =
                    Time.captureDeltaTime;
                proof.recorderPaddingActiveAtLogicalFrameZero =
                    Time.captureDeltaTime >= minimumDelta
                    && Time.captureDeltaTime < minimumDelta + 0.001f;
                if (!proof.recorderPaddingActiveAtLogicalFrameZero)
                {
                    throw new InvalidOperationException(
                        "G07 Recorder padding was not active at logical f0: "
                        + Time.captureDeltaTime.ToString(
                            "F9",
                            CultureInfo.InvariantCulture));
                }

                director.BeginShotForRecorder();
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

            failure = Combine(failure, CaptureDirectorProof());
            IEnumerator cleanup = CleanupAfterRecorder();
            while (true)
            {
                bool moved;
                object yielded;
                try
                {
                    moved = cleanup.MoveNext();
                    yielded = moved ? cleanup.Current : null;
                }
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                    break;
                }

                if (!moved)
                {
                    break;
                }

                yield return yielded;
            }

            failure = Combine(failure, CaptureCleanupProof());
            if (director != null && director.CleanupFailure != null)
            {
                proof.cleanupFailure = director.CleanupFailure.ToString();
                failure = Combine(failure, director.CleanupFailure);
            }

            NotifyFinished(failure);
        }

        private IEnumerator RunCore()
        {
            director = AuditionPvStationPhase2PatternRelayCapture
                .AttachToFreshActiveScene();
            director.FramePresented += HandleFramePresented;
            renderProbe = gameObject.AddComponent<
                AuditionPvStationPhase2PatternRelayRenderProbe>();
            renderProbe.Configure(director);

            IEnumerator preparation = director.PrepareFreshProductState();
            while (preparation.MoveNext())
            {
                yield return preparation.Current;
            }

            if (!director.IsPrepared)
            {
                throw new InvalidOperationException(
                    "G07 product-state director did not finish preparation.");
            }

            s070RuntimeWorkload = AuditionPvRuntimeWorkloadCaptureSession.Open(
                new AuditionPvRuntimeWorkloadCaptureConfig
                {
                    captureId = state.captureId,
                    captureOutputDirectory = outputDirectory,
                    sourceShotId =
                        AuditionPvStationPhase2PatternRelayCapture.ShotId,
                    sourceRangeStartFrame =
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .S070SourceRangeStartFrame,
                    sourceRangeEndFrame =
                        AuditionPvStationPhase2PatternRelayGoldenRunner
                            .S070SourceRangeEndFrame,
                    captureHudEvidence = false
                });

            recorderSettings =
                AuditionPvRecorderSettingsFactory.CreateLosslessPngSequence(
                    outputDirectory,
                    AuditionPvStationPhase2PatternRelayCapture.ShotId);
            recorderSettings.controllerSettings.SetRecordModeToFrameInterval(
                AuditionPvStationPhase2PatternRelayGoldenRunner.RawWarmupFrame,
                AuditionPvStationPhase2PatternRelayGoldenRunner.RawLastShotFrame);
            AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
            recorderController = new RecorderController(
                recorderSettings.controllerSettings);
            recorderController.PrepareRecording();
            if (!recorderController.StartRecording())
            {
                throw new InvalidOperationException(
                    "Unity Recorder 5.1.6 rejected the G07 QHD60 PNG session.");
            }

            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 1;
            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 2;
            for (int handleFrame = 0;
                handleFrame
                    < AuditionPvStationPhase2PatternRelayCapture
                        .HandleFrameCount;
                handleFrame++)
            {
                yield return new WaitForEndOfFrame();
                s070RuntimeWorkload.CapturePresentedFrame(handleFrame);
                proof.recorderPreHandleEndOfFrameCount++;
            }

            proof.canonicalSourceFrameCount =
                AuditionPvStationPhase2PatternRelayCapture.ExpectedFrameCount;
            proof.logicalFirstSourceFrame =
                AuditionPvStationPhase2PatternRelayCapture.SelectStartFrame;
            proof.logicalLastSourceFrame =
                AuditionPvStationPhase2PatternRelayCapture.SelectEndFrame;
            proof.recordedPreHandleFrameCount =
                proof.recorderPreHandleEndOfFrameCount;
            if (!recorderController.IsRecording()
                || director.IsRunning
                || director.IsComplete)
            {
                throw new InvalidOperationException(
                    "G07 did not record the complete real prehandle before logical f0.");
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
                    "G07 could not arm logical f0 after Recorder warm-up.",
                    updateFailure);
            }

            if (!beganLogicalShot)
            {
                throw new TimeoutException(
                    "G07 timed out before its early-Update logical f0 arm.");
            }

            while (!director.IsComplete
                && director.Failure == null
                && renderProbe.Failure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            if (director.Failure != null)
            {
                throw new InvalidOperationException(
                    "G07 product-state director failed during recording.",
                    director.Failure);
            }

            if (renderProbe.Failure != null)
            {
                throw new InvalidOperationException(
                    "G07 late render probe failed during recording.",
                    renderProbe.Failure);
            }

            if (!director.IsComplete)
            {
                throw new TimeoutException(
                    "G07 did not complete logical frames 0..419 before timeout.");
            }

            int recordedPostHandleFrames = 0;
            for (; recordedPostHandleFrames <
                   AuditionPvStationPhase2PatternRelayCapture.HandleFrameCount;
                 recordedPostHandleFrames++)
            {
                if (!recorderController.IsRecording())
                {
                    throw new InvalidOperationException(
                        "G07 Recorder stopped before the complete runtime-evidenced posthandle.");
                }

                yield return new WaitForEndOfFrame();
                s070RuntimeWorkload.CapturePresentedFrame(
                    AuditionPvStationPhase2PatternRelayCapture.SelectEndFrame + 1
                    + recordedPostHandleFrames);
            }

            while (recorderController.IsRecording()
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            proof.recorderAutoStoppedAfterLastFrame =
                !recorderController.IsRecording();
            if (!proof.recorderAutoStoppedAfterLastFrame)
            {
                throw new InvalidOperationException(
                    "G07 Recorder did not auto-stop after raw780/canonical source f779.");
            }

            proof.recordedPostHandleFrameCount = recordedPostHandleFrames;
            state.s070RuntimeWorkloadSealPath = s070RuntimeWorkload.Complete();
            s070RuntimeWorkload = null;
        }

        private void HandleFramePresented(int frameIndex)
        {
            s070RuntimeWorkload?.CapturePresentedFrame(
                AuditionPvStationPhase2PatternRelayCapture.SelectStartFrame
                + frameIndex);
            proof.presentedFramesExact &= frameIndex == nextPresentedFrame;
            proof.presentationClockExact &= PresentationClock.IsManuallyDriven
                && Mathf.Abs(PresentationClock.UnscaledTime
                    - frameIndex / (float)AuditionPvCaptureContract.Fps) <= 0.00001f
                && Mathf.Abs(PresentationClock.UnscaledDeltaTime
                    - 1f / AuditionPvCaptureContract.Fps) <= 0.00001f;
            proof.presentedFrameCount++;
            nextPresentedFrame++;
        }

        private Exception CaptureDirectorProof()
        {
            try
            {
                if (director == null)
                {
                    return null;
                }

                proof.directorCompleted = director.IsComplete;
                proof.lastLogicalFrame = director.CurrentFrame;
                proof.transitionCompletedEventCount = director.TransitionCompletedEventCount;
                proof.windupEventCount = director.WindupEventCount;
                proof.waveEventCount = director.WaveEventCount;
                proof.curtainWindupFrame = director.CurtainWindupFrame;
                proof.curtainFireFrame = director.CurtainFireFrame;
                proof.curtainSpawnedCount = director.CurtainSpawnedCount;
                proof.curtainWasPriority = director.CurtainWasPriority;
                proof.hoverWindupFrame = director.HoverWindupFrame;
                proof.hoverFireFrame = director.HoverFireFrame;
                proof.hoverSpawnedCount = director.HoverSpawnedCount;
                proof.hoverWasPriority = director.HoverWasPriority;
                proof.hoverSequenceIndexAfterFire = director.HoverSequenceIndexAfterFire;
                proof.curtainWindupPatternId = director.CurtainWindupPatternId;
                proof.curtainFirePatternId = director.CurtainFirePatternId;
                proof.hoverWindupPatternId = director.HoverWindupPatternId;
                proof.hoverFirePatternId = director.HoverFirePatternId;
                proof.emitterTickCount = director.EmitterTickCount;
                proof.minimumEmitterTimeScale = director.MinimumEmitterTimeScale;
                proof.maximumEmitterTimeScale = director.MaximumEmitterTimeScale;
                proof.runStartedCount = director.RunStartedCount;
                proof.stopSettleCount = director.StopSettleCount;
                proof.curtainMoveFirstAppliedFrame = director.CurtainMoveFirstAppliedFrame;
                proof.curtainMoveLastAppliedFrame = director.CurtainMoveLastAppliedFrame;
                proof.curtainZeroAppliedFrame = director.CurtainZeroAppliedFrame;
                proof.hoverMoveFirstAppliedFrame = director.HoverMoveFirstAppliedFrame;
                proof.hoverMoveLastAppliedFrame = director.HoverMoveLastAppliedFrame;
                proof.hoverZeroAppliedFrame = director.HoverZeroAppliedFrame;
                proof.curtainRiskBefore = director.CurtainRiskBefore;
                proof.curtainRiskAfter = director.CurtainRiskAfter;
                proof.stayedInsideForwardBoundary = director.StayedInsideForwardBoundary;
                proof.hoverPreviewCount = director.HoverPreviewCount;
                proof.hoverPreviewAverageLateral = director.HoverPreviewAverageLateral;
                proof.hoverLateralDisplacement = director.HoverLateralDisplacement;
                proof.hoverDirectionDot = director.HoverDirectionDot;
                proof.visualWindupDelta = director.VisualWindupDelta;
                proof.visualReleaseDelta = director.VisualReleaseDelta;
                proof.telegraphWindupDelta = director.TelegraphWindupDelta;
                proof.telegraphReleaseDelta = director.TelegraphReleaseDelta;
                proof.cameraWindupDelta = director.CameraWindupDelta;
                proof.cameraFireDelta = director.CameraFireDelta;
                proof.motionReleaseDelta = director.MotionReleaseDelta;
                proof.curtainWindupVisibleMarkerCount = director.CurtainWindupVisibleMarkerCount;
                proof.curtainFireVisibleMarkerCount = director.CurtainFireVisibleMarkerCount;
                proof.hoverWindupVisibleMarkerCount = director.HoverWindupVisibleMarkerCount;
                proof.hoverFireVisibleMarkerCount = director.HoverFireVisibleMarkerCount;
                proof.curtainWindupVisibleRendererCount = director.CurtainWindupVisibleRendererCount;
                proof.curtainFireVisibleRendererCount = director.CurtainFireVisibleRendererCount;
                proof.hoverWindupVisibleRendererCount = director.HoverWindupVisibleRendererCount;
                proof.hoverFireVisibleRendererCount = director.HoverFireVisibleRendererCount;
                proof.telegraphMarkerCollidersNonBlocking =
                    director.TelegraphMarkerCollidersNonBlocking;
                proof.curtainWindupMarkerColor = director.CurtainWindupMarkerColor;
                proof.curtainFireMarkerColor = director.CurtainFireMarkerColor;
                proof.hoverWindupMarkerColor = director.HoverWindupMarkerColor;
                proof.hoverFireMarkerColor = director.HoverFireMarkerColor;
                proof.basicVolleyEventCount = director.BasicVolleyEventCount;
                proof.pressureActionEventCount = director.PressureActionEventCount;
                proof.enemySummonReleaseCountDelta = director.EnemySummonReleaseCountDelta;
                proof.playerDamageEventCount = director.PlayerDamageEventCount;
                proof.bossDamageEventCount = director.BossDamageEventCount;
                proof.playerBasicStartedCount = director.PlayerBasicStartedCount;
                proof.playerBasicHitCount = director.PlayerBasicHitCount;
                proof.dodgeStartedCount = director.DodgeStartedCount;
                proof.dodgeEndedCount = director.DodgeEndedCount;
                proof.perfectDodgeCount = director.PerfectDodgeCount;
                proof.summonUsedCount = director.SummonUsedCount;
                proof.summonBlockedCount = director.SummonBlockedCount;
                proof.summonUseBlockedCount = director.SummonUseBlockedCount;
                proof.playerHealthUnchanged = director.PlayerHealthUnchanged;
                proof.bossHealthUnchanged = director.BossHealthUnchanged;
                proof.resourcesUnchanged = director.ResourcesUnchanged;
                proof.exactHudAndBindings = director.ExactHudAndBindings;
                proof.exactProjectileAndVfxBindings = director.ExactProjectileAndVfxBindings;
                proof.lifecycleEmergencyResetUsed = director.LifecycleEmergencyResetUsed;
                proof.renderEvents = renderProbe != null
                    ? renderProbe.CopyEvidence()
                    : Array.Empty<AuditionPvStationPhase2PatternRelayGoldenRunner.RenderEventEvidence>();
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private IEnumerator CleanupAfterRecorder()
        {
            if (cleaningUp)
            {
                yield break;
            }

            cleaningUp = true;
            armLogicalFrameZero = false;
            Exception cleanupFailure = null;
            if (director != null)
            {
                director.FramePresented -= HandleFramePresented;
            }

            try
            {
                if (recorderController != null && recorderController.IsRecording())
                {
                    recorderController.StopRecording();
                }
            }
            catch (Exception exception)
            {
                cleanupFailure = Combine(cleanupFailure, exception);
            }

            recorderController = null;
            if (director != null)
            {
                IEnumerator restoration = null;
                try
                {
                    restoration = director.RestoreAfterRecording();
                }
                catch (Exception exception)
                {
                    cleanupFailure = Combine(cleanupFailure, exception);
                }

                while (restoration != null)
                {
                    bool moved;
                    object yielded;
                    try
                    {
                        moved = restoration.MoveNext();
                        yielded = moved ? restoration.Current : null;
                    }
                    catch (Exception exception)
                    {
                        cleanupFailure = Combine(cleanupFailure, exception);
                        break;
                    }

                    if (!moved)
                    {
                        break;
                    }

                    yield return yielded;
                }
            }

            try
            {
                s070RuntimeWorkload?.Dispose();
                s070RuntimeWorkload = null;
            }
            catch (Exception exception)
            {
                cleanupFailure = Combine(cleanupFailure, exception);
            }

            try
            {
                recorderSettings?.Dispose();
            }
            catch (Exception exception)
            {
                cleanupFailure = Combine(cleanupFailure, exception);
            }

            recorderSettings = null;
            if (cleanupFailure != null)
            {
                throw new InvalidOperationException(
                    "G07 Recorder/director/settings cleanup encountered an error.",
                    cleanupFailure);
            }
        }

        private Exception CaptureCleanupProof()
        {
            try
            {
                if (director != null)
                {
                    proof.stateRestored = director.StateRestored;
                    proof.eventsReleased = director.EventsReleased;
                    proof.presentationClockReleased = director.PresentationClockReleased;
                    proof.cadenceReleased = director.CadenceReleased;
                    proof.emitterRestored = director.EmitterRestored;
                    proof.spawnOriginOrderRestored =
                        director.SpawnOriginOrderRestored;
                    proof.playerStateRestored = director.PlayerStateRestored;
                    proof.bossStateRestored = director.BossStateRestored;
                    proof.cameraStateRestored = director.CameraStateRestored;
                    proof.hudStateRestored = director.HudStateRestored;
                    proof.globalStateRestored = director.GlobalStateRestored;
                    proof.lifecycleEmergencyResetUsed =
                        director.LifecycleEmergencyResetUsed;
                    proof.postRecordingSettleFrames = director.PostRecordingSettleFrames;
                    proof.postRecordingSettleSeconds = director.PostRecordingSettleSeconds;
                }

                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private void NotifyFinished(Exception failure)
        {
            if (notified)
            {
                return;
            }

            try
            {
                AuditionPvStationPhase2PatternRelayGoldenRunner.NotifyPlayModeFinished(
                    statePath,
                    state,
                    proof,
                    failure);
            }
            catch (Exception handoffFailure)
            {
                Debug.LogException(handoffFailure);
            }
            finally
            {
                // Set only after the transactional handoff has either persisted
                // or recorded its own terminal failure and forced PlayMode exit.
                notified = true;
            }
        }

        private void OnDisable()
        {
            if (notified || !Application.isPlaying)
            {
                return;
            }

            Exception failure = new InvalidOperationException(
                "G07 runner was disabled before asynchronous cleanup completed.");
            try
            {
                if (recorderController != null && recorderController.IsRecording())
                {
                    recorderController.StopRecording();
                }

                director?.RestoreFromLifecycleEmergency();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            try
            {
                s070RuntimeWorkload?.Dispose();
                s070RuntimeWorkload = null;
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            failure = Combine(failure, CaptureDirectorProof());
            failure = Combine(failure, CaptureCleanupProof());
            NotifyFinished(failure);
        }

        private static Exception Combine(Exception first, Exception next)
        {
            if (first == null)
            {
                return next;
            }

            if (next == null || ReferenceEquals(first, next))
            {
                return first;
            }

            return new AggregateException(first, next);
        }
    }

    /// <summary>
    /// Render authority after ActionCameraController (+200). FramePresented is
    /// only a schedule signal; this +32000 LateUpdate samples Recorder's final
    /// gameplay camera, actors, and every enabled telegraph renderer.
    /// </summary>
    [DefaultExecutionOrder(32000)]
    public sealed class AuditionPvStationPhase2PatternRelayRenderProbe
        : MonoBehaviour
    {
        private static readonly int[] EventFrames = { 10, 68, 368, 418, 419 };
        private readonly List<AuditionPvStationPhase2PatternRelayGoldenRunner
            .RenderEventEvidence> evidence = new();
        private readonly HashSet<int> capturedFrames = new();
        private AuditionPvStationPhase2PatternRelayDirector director;

        public Exception Failure { get; private set; }

        internal void Configure(AuditionPvStationPhase2PatternRelayDirector source)
        {
            director = source ?? throw new ArgumentNullException(nameof(source));
        }

        internal AuditionPvStationPhase2PatternRelayGoldenRunner.RenderEventEvidence[]
            CopyEvidence()
        {
            return evidence.OrderBy(value => value.logicalFrame).ToArray();
        }

        private void LateUpdate()
        {
            if (Failure != null || director == null)
            {
                return;
            }

            int frame = director.LastPresentedFrame;
            if (Array.IndexOf(EventFrames, frame) < 0 || !capturedFrames.Add(frame))
            {
                return;
            }

            try
            {
                evidence.Add(Capture(frame));
            }
            catch (Exception exception)
            {
                Failure = exception;
            }
        }

        private AuditionPvStationPhase2PatternRelayGoldenRunner.RenderEventEvidence
            Capture(int logicalFrame)
        {
            Camera camera = director.GameplayCamera;
            Transform player = director.PlayerRendererRoot;
            Transform boss = director.BossRendererRoot;
            BossBarrageLaneTelegraphPresenter telegraph = director.TelegraphPresenter;
            if (camera == null || player == null || boss == null || telegraph == null)
            {
                throw new InvalidOperationException(
                    "G07 late render probe lost an exact director binding.");
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var result = new AuditionPvStationPhase2PatternRelayGoldenRunner
                .RenderEventEvidence
            {
                logicalFrame = logicalFrame,
                cameraActiveAndEnabled = camera.isActiveAndEnabled
                    && camera.gameObject.activeInHierarchy,
                cameraPerspective = !camera.orthographic,
                cameraFullRect = Mathf.Abs(camera.rect.x) <= 0.000001f
                    && Mathf.Abs(camera.rect.y) <= 0.000001f
                    && Mathf.Abs(camera.rect.width - 1f) <= 0.000001f
                    && Mathf.Abs(camera.rect.height - 1f) <= 0.000001f,
                cameraTargetTextureNull = camera.targetTexture == null,
                player = CaptureSubject(camera, planes, player),
                boss = CaptureSubject(camera, planes, boss),
                visibleMarkerCount = telegraph.VisibleMarkerCount
            };

            // f419 is the explicit final-hero composition after Hover release.
            // Markers are event-frame evidence only and may naturally expire.
            if (logicalFrame == 419)
            {
                result.finalHeroComposition = result.player.safeViewport
                    && result.boss.safeViewport;
                result.markers = Array.Empty<AuditionPvStationPhase2PatternRelayGoldenRunner
                    .SubjectViewportEvidence>();
                return result;
            }

            Renderer[] markers = ReadActiveMarkerRenderers(camera, telegraph);
            int expected = logicalFrame < 300
                ? AuditionPvStationPhase2PatternRelayCapture.CurtainProjectileCount
                : AuditionPvStationPhase2PatternRelayCapture.HoverProjectileCount;
            if (markers.Length != expected || result.visibleMarkerCount != expected)
            {
                throw new InvalidOperationException(
                    $"G07 f{logicalFrame} late marker set was {markers.Length}/{result.visibleMarkerCount}; expected {expected}.");
            }

            result.visibleMarkerRendererCount = markers.Length;
            result.markers = markers
                .Select(marker => CaptureBounds(camera, planes, marker.bounds, false))
                .ToArray();
            result.allMarkerRenderersIntersectFrustum = result.markers.All(value =>
                value.rendererBoundsFound
                && value.frustumIntersects
                && value.pixelWidth > 0
                && value.pixelHeight > 0);
            Bounds aggregate = markers[0].bounds;
            for (int index = 1; index < markers.Length; index++)
            {
                aggregate.Encapsulate(markers[index].bounds);
            }

            AuditionPvStationPhase2PatternRelayGoldenRunner.SubjectViewportEvidence
                markerBounds = CaptureBounds(camera, planes, aggregate, false);
            result.markerBoundsIntersectFrustum = markerBounds.frustumIntersects;
            result.markerPixelX = markerBounds.pixelX;
            result.markerPixelY = markerBounds.pixelY;
            result.markerPixelWidth = markerBounds.pixelWidth;
            result.markerPixelHeight = markerBounds.pixelHeight;
            return result;
        }

        private static Renderer[] ReadActiveMarkerRenderers(
            Camera camera,
            BossBarrageLaneTelegraphPresenter telegraph)
        {
            var serialized = new SerializedObject(telegraph);
            SerializedProperty property = serialized.FindProperty("markerRenderers");
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException(
                    "G07 late render probe could not read the marker renderer list.");
            }

            var markers = new List<Renderer>(property.arraySize);
            for (int index = 0; index < property.arraySize; index++)
            {
                Renderer renderer = property.GetArrayElementAtIndex(index)
                    .objectReferenceValue as Renderer;
                if (IsRenderedByCamera(camera, renderer))
                {
                    markers.Add(renderer);
                }
            }

            return markers.ToArray();
        }

        private static AuditionPvStationPhase2PatternRelayGoldenRunner
            .SubjectViewportEvidence CaptureSubject(
                Camera camera,
                Plane[] planes,
                Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(false)
                .Where(value => IsRenderedByCamera(camera, value))
                .ToArray();
            if (renderers.Length == 0)
            {
                return new AuditionPvStationPhase2PatternRelayGoldenRunner
                    .SubjectViewportEvidence();
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return CaptureBounds(camera, planes, bounds, true);
        }

        internal static bool IsRenderedByCamera(Camera camera, Renderer renderer)
        {
            return camera != null
                && renderer != null
                && renderer.enabled
                && !renderer.forceRenderingOff
                && renderer.shadowCastingMode
                    != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly
                && renderer.gameObject.activeInHierarchy
                && (camera.cullingMask & (1 << renderer.gameObject.layer)) != 0;
        }

        private static AuditionPvStationPhase2PatternRelayGoldenRunner
            .SubjectViewportEvidence CaptureBounds(
                Camera camera,
                Plane[] planes,
                Bounds bounds,
                bool requireSafeCenter)
        {
            Vector3 center = camera.WorldToViewportPoint(bounds.center);
            bool centerInFront = center.z > 0f;
            bool centerSafe = centerInFront
                && center.x >= 0.05f && center.x <= 0.95f
                && center.y >= 0.05f && center.y <= 0.95f;
            bool intersects = GeometryUtility.TestPlanesAABB(planes, bounds);
            RectInt pixels = ProjectBounds(camera, bounds);
            return new AuditionPvStationPhase2PatternRelayGoldenRunner
                .SubjectViewportEvidence
            {
                rendererBoundsFound = true,
                frustumIntersects = intersects,
                centerInFront = centerInFront,
                centerInsideSafeViewport = centerSafe,
                safeViewport = intersects
                    && centerInFront
                    && (!requireSafeCenter || centerSafe),
                viewportCenter = center,
                pixelX = pixels.x,
                pixelY = pixels.y,
                pixelWidth = pixels.width,
                pixelHeight = pixels.height
            };
        }

        internal static RectInt ProjectBounds(Camera camera, Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3[] corners =
            {
                new(min.x, min.y, min.z), new(max.x, min.y, min.z),
                new(min.x, max.y, min.z), new(max.x, max.y, min.z),
                new(min.x, min.y, max.z), new(max.x, min.y, max.z),
                new(min.x, max.y, max.z), new(max.x, max.y, max.z)
            };
            float xMin = 1f;
            float yMin = 1f;
            float xMax = 0f;
            float yMax = 0f;
            foreach (Vector3 corner in corners)
            {
                Vector3 viewport = camera.WorldToViewportPoint(corner);
                xMin = Mathf.Min(xMin, viewport.x);
                yMin = Mathf.Min(yMin, viewport.y);
                xMax = Mathf.Max(xMax, viewport.x);
                yMax = Mathf.Max(yMax, viewport.y);
            }

            xMin = Mathf.Clamp01(xMin);
            yMin = Mathf.Clamp01(yMin);
            xMax = Mathf.Clamp01(xMax);
            yMax = Mathf.Clamp01(yMax);
            int pixelXMin = Mathf.Clamp(
                Mathf.FloorToInt(xMin * AuditionPvCaptureContract.Width),
                0,
                AuditionPvCaptureContract.Width - 1);
            int pixelYMin = Mathf.Clamp(
                Mathf.FloorToInt(yMin * AuditionPvCaptureContract.Height),
                0,
                AuditionPvCaptureContract.Height - 1);
            int pixelXMax = Mathf.Clamp(
                Mathf.CeilToInt(xMax * AuditionPvCaptureContract.Width),
                pixelXMin + 1,
                AuditionPvCaptureContract.Width);
            int pixelYMax = Mathf.Clamp(
                Mathf.CeilToInt(yMax * AuditionPvCaptureContract.Height),
                pixelYMin + 1,
                AuditionPvCaptureContract.Height);
            return new RectInt(
                pixelXMin,
                pixelYMin,
                pixelXMax - pixelXMin,
                pixelYMax - pixelYMin);
        }
    }
}
