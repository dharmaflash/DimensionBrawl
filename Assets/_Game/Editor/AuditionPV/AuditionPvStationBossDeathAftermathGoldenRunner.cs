using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Combat;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using DimensionBrawl.UI.StageClear;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Transactional G08 Recorder owner. A committed manifest is written only
    /// after the real product route, physical projectile, terminal aftermath,
    /// rendered pixels, provenance, and cleanup all pass.
    /// </summary>
    [InitializeOnLoad]
    public static class AuditionPvStationBossDeathAftermathGoldenRunner
    {
        internal const string RunnerScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationBossDeathAftermathGoldenRunner.cs";
        internal const string RunnerTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvStationBossDeathAftermathGoldenTests.cs";
        internal const string ReadmePath =
            "Assets/_Game/Editor/AuditionPV/README.md";
        internal const string MenuPath =
            "DimensionBrawl/Audition PV/Capture G08 Station Boss Death Aftermath Golden Source";
        internal const string StateFileName = "g08_runner_state.json";
        internal const string RuntimeProofFileName = "g08_runtime_proof.json";
        internal const string FrameHashLedgerFileName = "frame_hashes.sha256";
        internal const string GateShotAuthorshipFileName =
            "g08_shot_authorship.json";
        internal const string GateSemanticEvidenceFolderName =
            "semantic_beats";
        internal const string FailureFileName = "g08_capture_failure.json";
        internal const string EvidenceFolderName = "evidence";
        internal const string WarmupEvidenceFileName =
            "recorder_warmup_raw_frame_0000.png";
        internal const int RawWarmupFrame = 0;
        internal const int RawFirstShotFrame = 1;
        internal const int RawLastShotFrame =
            AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
        internal const int ExpectedRawFrameCount = RawLastShotFrame + 1;
        internal const int S090EvidenceSourceRangeStartFrame = 60;
        internal const int S090EvidenceSourceRangeEndFrame = 719;
        internal const int S090EvidenceSelectStartFrame = 240;
        internal const int S090EvidenceSelectEndFrame = 539;
        internal const string RuntimeMappingDescription =
            "Recorder raw0 is preserved warm-up evidence; raw1..raw720 map to canonical source f0..f719; logical f0..f359 map to source f180..f539; S090 selects source f240..f539.";
        internal const string RuntimeGameplayDescription =
            "Canonical Corridor product flow; logical f0..f59 excluded from S090; logical f1 one public TryFire; same physical projectile natural logical f62 impact/Died and unique authored finisher-camera hard cut; logical f218 terminal Timeline sample/result cover; logical f246 gameplay-camera restore and interactive committed SameAs result.";
        internal const string ExpectedUnityVersion = "6000.3.5f2";
        internal const string ExpectedUnityVersionWithRevision =
            "6000.3.5f2 (3fa8bc678cb0)";
        internal const string ExpectedUrpPackageVersion = "17.3.0";
        internal const string ExpectedRenderPipelineAssetPath =
            "Assets/Settings/PC_RPAsset.asset";

        internal const string PixelCalibrationCaptureId =
            "20260816t084414z_g08-station-boss-death-aftermath_g174d6862472a_clean";
        internal const string PixelCalibrationHeadSha =
            "174d6862472abf89b295749e37fdd1b280f97c49";
        internal const string PixelCalibrationFailureSha256 =
            "e44e24e74c31f9ad6b6b1e0e6ef903ee10f7181cce5fd22afca0e1eda5defa9a";
        internal const string PixelCalibrationReconstructedLedgerSha256 =
            "66577dd2934bae05f50c9812026d5e46e98f9de45de23c3c00393e1196d24de1";
        internal const string VisualCompositionAcceptanceCaptureId =
            "20260816t130338z_g08-station-boss-death-aftermath_gd4c70fbbe697_clean";
        internal const string VisualCompositionAcceptanceHeadSha =
            "d4c70fbbe697da1a16ad505533cb15ba6c7b4357";
        internal const string VisualCompositionAcceptanceFailureSha256 =
            "7367b22719da895f74dcb1ad4b18b6a0b434eabe5291af2236bfe991c958834c";
        internal const string VisualCompositionAcceptanceReconstructedLedgerSha256 =
            "83a51984a5403863a36dc09ce3bcedc9f68e211abba970a96bc5d24cab1483e9";
        internal const string VisualCompositionAcceptanceCaptureStartProvenanceSha256 =
            "df07decc27241d4e3adf305b9aa03a543452ac6bafbce5f045b540e8d36d967c";
        internal const double VisualCompositionAcceptanceF62BodyHeightRatio =
            0.30446988344192507d;
        internal const double VisualCompositionAcceptanceF116BodyMaxExtentRatio =
            0.2547542154788971d;
        internal const double VisualCompositionAcceptanceF181BodyMaxExtentRatio =
            0.25421980023384097d;
        internal const double VisualCompositionAcceptanceImpactToHeroAxisDeltaDegrees =
            61.78799404763924d;
        internal const double VisualCompositionAcceptanceImpactToHoldAxisDeltaDegrees =
            61.84057514229713d;
        internal const double VisualCompositionAcceptanceTerminalAxisHoldDriftDegrees =
            0.05258109465662268d;

        internal static readonly int ImpactDeltaFromFrame = SourceFrame(61);
        internal static readonly int ImpactDeltaToFrame = SourceFrame(62);
        internal static readonly int AftermathDeltaFromFrame = SourceFrame(62);
        internal static readonly int AftermathDeltaToFrame = SourceFrame(116);
        internal static readonly int ResultAppearanceFromFrame = SourceFrame(218);
        internal static readonly int ResultAppearanceToFrame = SourceFrame(221);
        internal static readonly int ResultEntranceFromFrame = SourceFrame(221);
        internal static readonly int ResultEntranceToFrame = SourceFrame(246);
        internal static readonly int ResultSurfaceFrame = SourceFrame(246);
        internal const int SequencePixelSampleStride = 8;
        internal const long ExpectedSequencePixelSampleCount = 41472000;
        internal const int FrameDeltaPixelSampleStride = 4;
        internal const int ExpectedFrameDeltaPixelSampleCount = 230400;
        internal const int FrameDeltaChangedRgbSumCutoff = 24;
        internal const int ResultSurfaceSampleStride = 4;
        internal const int ExpectedResultSurfaceSampleCount = 138240;
        internal const int ResultBrightMinimumChannel = 200;
        internal const int ResultNavyMaximumLuma = 75;
        internal const int ResultBlueMinimumChannel = 120;
        internal const int ResultBlueMinimumRedDelta = 25;
        internal const int ResultBlueMinimumGreenDelta = 10;
        internal static readonly RectInt ResultSurfaceRawBottomLeftRoi =
            new(256, 180, 2048, 1080);

        // Locked from the independently reviewed same-HEAD runtime/pixel-calibration
        // take above; visual composition is independently locked below.
        // Observed: black/magenta/max-magenta=0, healthy=100%; impact
        // 13.542403/.299323; death evolution 30.489848/.549518; first visible
        // result appearance f218->f221 8.468069/.328385; visible entrance
        // f221->f246 35.305295/.852348. At f246 the exact raw-bottom ROI
        // measured bright=76,646, navy-luma=630, blue=80,369 samples. Floors
        // and ceilings retain at least ~20% headroom from every reviewed value.
        internal const double MaximumSequenceBlackRatio = 0.05d;
        internal const double MaximumSequenceMagentaRatio = 0.001d;
        internal const double MaximumFrameMagentaRatio = 0.005d;
        internal const int MinimumHealthyFramePercent = 100;
        internal const double MinimumImpactMeanAbsoluteRgb = 6d;
        internal const double MinimumImpactChangedRatio = 0.12d;
        internal const double MinimumAftermathEvolutionMeanAbsoluteRgb = 12d;
        internal const double MinimumAftermathEvolutionChangedRatio = 0.20d;
        internal const double MinimumResultAppearanceMeanAbsoluteRgb = 3d;
        internal const double MinimumResultAppearanceChangedRatio = 0.08d;
        internal const double MinimumResultEntranceMeanAbsoluteRgb = 15d;
        internal const double MinimumResultEntranceChangedRatio = 0.30d;
        internal const int MinimumResultBrightSamples = 60000;
        internal const int MinimumResultNavySamples = 500;
        internal const int MinimumResultBlueSamples = 60000;
        internal static readonly bool PixelCalibrationLocked = true;

        // Locked after independent QHD review of the exact clean take pinned above.
        // Those pins are the immutable approval provenance; each later clean take
        // must independently pass the live runtime, pixel, and composition gates.
        // Observed tight body extents were f62=.304470, f116=.254754, and
        // f181=.254220. In the validator's screen-height-equivalent coordinates,
        // the projected hip-to-head axis changed 61.79 degrees into the collapse
        // and drifted only .053 degrees through the terminal hold.
        internal static readonly bool VisualCompositionAcceptanceLocked = true;
        internal const float MinimumFinisherBossBodyHeightRatio = 0.25f;
        internal const float MaximumFinisherBossBodyHeightRatio = 0.40f;
        internal const float MinimumTerminalBossBodyMaxExtentRatio = 0.25f;
        internal const float MaximumTerminalBossBodyMaxExtentRatio = 0.40f;
        internal const float MinimumVisiblePlayerBodyHeightRatio = 0.25f;
        internal const float MaximumVisiblePlayerBodyHeightRatio = 0.32f;
        internal const float MinimumBossEnvelopeReadableExtentRatio = 0.05f;
        internal const float MinimumBossCoreAxisViewportLength = 0.08f;
        internal const float MinimumTerminalBossCoreAxisOrientationDeltaDegrees = 35f;
        internal const float MaximumTerminalBossCoreAxisHoldDriftDegrees = 8f;
        internal const float ExpectedCompositionProjectionAspect = 16f / 9f;
        internal const string BossCoreAxisSource =
            "akaza-generic-hip_C-to-head_C";
        internal const float MaximumFinisherBossCenterDrift = 0.08f;
        internal const float MaximumTerminalBossBodyMaxExtentSpread = 0.05f;
        internal static readonly int[] CompositionEvidenceFrames =
        {
            SourceFrame(61),
            SourceFrame(62),
            SourceFrame(116),
            SourceFrame(181),
            SourceFrame(246)
        };

        private const string SessionActiveKey =
            "DimensionBrawl.AuditionPV.G08GoldenRunner.Active";
        private const string SessionStatePathKey =
            "DimensionBrawl.AuditionPV.G08GoldenRunner.StatePath";
        private const string SessionOwnerKey =
            "DimensionBrawl.AuditionPV.G08GoldenRunner.Owner";
        private const string SessionBatchKey =
            "DimensionBrawl.AuditionPV.G08GoldenRunner.Batch";
        private const string SessionOutputDirectoryKey =
            "DimensionBrawl.AuditionPV.G08GoldenRunner.OutputDirectory";
        private const string SessionCaptureIdKey =
            "DimensionBrawl.AuditionPV.G08GoldenRunner.CaptureId";
        private const string SessionTerminalFaultKey =
            "DimensionBrawl.AuditionPV.G08GoldenRunner.TerminalFault";
        private const string SessionOwnerValue =
            "dimension-brawl.g08-station-boss-death-aftermath.v2";
        private const string RunnerSchema =
            "dimension-brawl.audition-pv.g08-runner-state.v2";
        internal const string RuntimeProofSchema =
            "dimension-brawl.audition-pv.g08-runtime-proof.v2";
        private const string FailureSchema =
            "dimension-brawl.audition-pv.capture-failure.v1";

        private static bool resumeScheduled;
        private static bool resumeWatchdogRegistered;
        private static bool finalizing;
        private static AuditionPvStationBossDeathAftermathGoldenRunnerBehaviour
            activeBehaviour;

        static AuditionPvStationBossDeathAftermathGoldenRunner()
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
                    "G08 golden capture did not start",
                    exception.Message,
                    "OK");
            }
        }

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
            if (rawFrameIndex < RawWarmupFrame || rawFrameIndex > RawLastShotFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(rawFrameIndex));
            }

            return $"frame_{rawFrameIndex:0000}.png";
        }

        internal static int SourceFrame(int logicalFrame)
        {
            return AuditionPvStationBossDeathAftermathCapture
                .LogicalToSourceFrame(logicalFrame);
        }

        internal static void ValidateBatchCommandLine(IEnumerable<string> arguments)
        {
            string[] args = (arguments ?? Array.Empty<string>()).ToArray();
            bool Has(string expected) => args.Any(value => string.Equals(
                value,
                expected,
                StringComparison.OrdinalIgnoreCase));
            if (!Has("-noaudio"))
            {
                throw new InvalidOperationException(
                    "G08 RunBatchCapture requires -noaudio.");
            }

            if (Has("-batchmode") || Has("-quit") || Has("-nographics"))
            {
                throw new InvalidOperationException(
                    "G08 requires a headful asynchronous Editor; remove -batchmode, -quit, and -nographics.");
            }
        }

        internal static void ValidateExactEngineProvenance(
            string unityVersion,
            string unityVersionWithRevision,
            string recorderPackageVersion,
            string urpPackageVersion,
            string activeRenderPipelineAssetPath)
        {
            if (!string.Equals(unityVersion, ExpectedUnityVersion, StringComparison.Ordinal)
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
                    "G08 requires the exact authored Unity, Recorder, URP, and render-pipeline provenance.");
            }
        }

        internal static void ValidateRequiredEngineProvenance(
            AuditionPvEngineSnapshot engine)
        {
            if (engine == null)
            {
                throw new InvalidDataException(
                    "G08 engine provenance is missing.");
            }

            if (string.IsNullOrWhiteSpace(engine.unityVersion)
                || string.IsNullOrWhiteSpace(engine.unityVersionWithRevision)
                || string.IsNullOrWhiteSpace(engine.recorderPackageVersion)
                || string.IsNullOrWhiteSpace(engine.urpPackageVersion)
                || string.IsNullOrWhiteSpace(engine.activeRenderPipelineAssetPath))
            {
                throw new InvalidDataException(
                    "G08 engine provenance is incomplete.");
            }

            try
            {
                ValidateExactEngineProvenance(
                    engine.unityVersion,
                    engine.unityVersionWithRevision,
                    engine.recorderPackageVersion,
                    engine.urpPackageVersion,
                    engine.activeRenderPipelineAssetPath);
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidDataException(
                    "G08 engine provenance is invalid.",
                    exception);
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
                    "G08 refuses to replace dirty open scenes: "
                    + string.Join(", ", dirty));
            }
        }

        internal static string[] CollectCaptureDependencyPaths()
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Add(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                string normalized = path.Replace('\\', '/');
                dependencies.Add(normalized);
                string absolute = ProjectAbsolutePath(normalized);
                if (File.Exists(absolute + ".meta"))
                {
                    dependencies.Add(normalized + ".meta");
                }
            }

            foreach (string path in AuditionPvCaptureContract.CoreDependencyPaths)
            {
                Add(path);
            }

            foreach (string path in
                     AuditionPvStationBossDeathAftermathCapture
                         .ExplicitProductDependencyPaths())
            {
                Add(path);
            }

            Add(RunnerScriptPath);
            Add(RunnerTestPath);
            Add(ReadmePath);
            string[] roots =
            {
                AuditionPvStationBossDeathAftermathCapture.CorridorScenePath,
                AuditionPvStationBossDeathAftermathCapture.StationScenePath,
                AuditionPvStationBossDeathAftermathCapture.StageClearScenePath,
                AuditionPvStationBossDeathAftermathCapture.TransitionOverlayPrefabPath,
                ExpectedRenderPipelineAssetPath,
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_CombatVfxCues_ActionFoundation.asset",
                "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_FrontlineWaveStage_MotivationReview.asset",
                "Assets/_Game/DesignData/Timelines/Cinematics/DB_Timeline_OlympusStationBossTerminalFinisher.playable",
                "Assets/_Game/DesignData/Timelines/Cinematics/DB_Anim_OlympusStationBossTerminalFinisherCamera.anim",
                "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller",
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab"
            };
            foreach (string root in roots)
            {
                Add(root);
                foreach (string dependency in AssetDatabase.GetDependencies(root, true))
                {
                    Add(dependency);
                }
            }

            // HashDependencies is the single authority for resolving both project and
            // PackageCache-backed assets.  Normalize through it here as well so its
            // automatically discovered package .meta files are persisted in the exact
            // same ordered path set that will be hashed at capture start/end.
            return AuditionPvEnvironmentProbe.HashDependencies(dependencies)
                .Select(value => value.path)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        internal static void ValidateStableGitSnapshot(
            AuditionPvGitSnapshot initial,
            AuditionPvGitSnapshot current)
        {
            if (initial == null
                || current == null
                || !initial.probeSucceeded
                || !current.probeSucceeded
                || initial.isDirty
                || current.isDirty
                || !string.Equals(initial.commitSha, current.commitSha, StringComparison.Ordinal)
                || !string.Equals(initial.branch, current.branch, StringComparison.Ordinal)
                || !string.Equals(
                    initial.dirtyStateHashSha256,
                    current.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 Git HEAD/branch/clean-state changed while recording.");
            }
        }

        internal static void ValidateStableDependencies(
            AuditionPvDependencyHash[] initial,
            AuditionPvDependencyHash[] current)
        {
            var currentByPath = (current ?? Array.Empty<AuditionPvDependencyHash>())
                .ToDictionary(value => value.path, StringComparer.OrdinalIgnoreCase);
            if (initial == null || initial.Length != currentByPath.Count)
            {
                throw new InvalidOperationException(
                    "G08 dependency set changed while recording.");
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
                        "G08 dependency changed while recording: "
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
                throw new FileNotFoundException("Expected G08 PNG is missing.", path);
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length
                    || stream.Read(header, 0, header.Length) != header.Length)
                {
                    throw new InvalidDataException("G08 PNG is truncated: " + path);
                }
            }

            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (!signature.Select((value, index) => header[index] == value)
                    .All(value => value)
                || header[12] != (byte)'I'
                || header[13] != (byte)'H'
                || header[14] != (byte)'D'
                || header[15] != (byte)'R')
            {
                throw new InvalidDataException(
                    "G08 PNG signature/IHDR mismatch: " + path);
            }

            int width = ReadBigEndianInt32(header, 16);
            int height = ReadBigEndianInt32(header, 20);
            if (width != expectedWidth || height != expectedHeight)
            {
                throw new InvalidDataException(
                    $"G08 PNG is {width}x{height}; expected {expectedWidth}x{expectedHeight}: {path}");
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
                if (texture.GetPixels32().Length != expectedWidth * expectedHeight)
                {
                    throw new InvalidDataException(
                        "G08 PNG decoded pixel count is not exact: " + path);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
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
                AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName);
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
                    ?? throw new InvalidOperationException(
                        "G08 frame directory has no parent."),
                ".g08-remap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            try
            {
                for (int raw = RawFirstShotFrame; raw <= RawLastShotFrame; raw++)
                {
                    MoveNew(
                        Path.Combine(frames, RawFrameFileName(raw)),
                        Path.Combine(
                            staging,
                            AuditionPvStationBossDeathAftermathCapture.FrameFileName(
                                raw - RawFirstShotFrame)));
                }

                string warmup = Path.Combine(evidence, WarmupEvidenceFileName);
                MoveNew(
                    Path.Combine(frames, RawFrameFileName(RawWarmupFrame)),
                    warmup);
                for (int sourceFrame =
                         AuditionPvStationBossDeathAftermathCapture.FirstFrame;
                    sourceFrame <=
                         AuditionPvStationBossDeathAftermathCapture.LastFrame;
                    sourceFrame++)
                {
                    string name = AuditionPvStationBossDeathAftermathCapture
                        .FrameFileName(sourceFrame);
                    MoveNew(Path.Combine(staging, name), Path.Combine(frames, name));
                }

                Directory.Delete(staging, recursive: false);
                return warmup.Replace('\\', '/');
            }
            catch
            {
                // A torn remap remains explicit evidence. Recovery never
                // guesses which duplicate is authoritative.
                throw;
            }
        }

        internal static string BuildFrameHashLedger(string frameDirectory)
        {
            ValidateLogicalFrameSequence(frameDirectory);
            var builder = new StringBuilder(
                AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount * 84);
            for (int frame =
                     AuditionPvStationBossDeathAftermathCapture.FirstFrame;
                frame <= AuditionPvStationBossDeathAftermathCapture.LastFrame;
                frame++)
            {
                string name = AuditionPvStationBossDeathAftermathCapture
                    .FrameFileName(frame);
                builder.Append(AuditionPvSha256.FileHash(Path.Combine(frameDirectory, name)))
                    .Append("  ")
                    .Append(name)
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
                    "G08 canonical 720-frame SHA-256 ledger changed.");
            }
        }

        internal static void ValidateRuntimeProof(RuntimeProof proof)
        {
            ValidateRuntimeProofCore(proof, requirePixelCalibration: true);
        }

        internal static void ValidateRuntimeProofBeforeVisualCompositionAcceptance(
            RuntimeProof proof)
        {
            ValidateRuntimeProof(proof);
        }

        internal static void ValidateRuntimeProofForPublication(RuntimeProof proof)
        {
            ValidateRuntimeProofForPublication(
                proof,
                VisualCompositionAcceptanceLocked);
        }

        internal static void ValidateRuntimeProofForPublication(
            RuntimeProof proof,
            bool visualCompositionAcceptanceLocked)
        {
            ValidateRuntimeProofBeforeVisualCompositionAcceptance(proof);
            if (!visualCompositionAcceptanceLocked)
            {
                throw new G08VisualCompositionAcceptanceRequiredException(
                    "G08 authored finisher composition is not visually accepted. Review the "
                    + "telemetry-only finisher take, approve the exact clean composition, and "
                    + "set VisualCompositionAcceptanceLocked=true before publication.");
            }
        }

        internal static void ValidateRuntimeProofBeforePixelCalibration(
            RuntimeProof proof)
        {
            ValidateRuntimeProofCore(proof, requirePixelCalibration: false);
        }

        private static void ValidateRuntimeProofCore(
            RuntimeProof proof,
            bool requirePixelCalibration)
        {
            if (proof == null
                || !proof.directorCompleted
                || proof.lastLogicalFrame
                    != AuditionPvStationBossDeathAftermathCapture.LogicalLastFrame
                || proof.presentedFrameCount
                    != AuditionPvStationBossDeathAftermathCapture
                        .LogicalExpectedFrameCount
                || !proof.presentedFramesExact
                || !proof.presentationClockExact
                || proof.recorderWarmupEndOfFrameCount != 2
                || proof.recorderPreHandleEndOfFrameCount
                    != AuditionPvStationBossDeathAftermathCapture.HandleFrameCount
                || proof.canonicalSourceFrameCount
                    != AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount
                || proof.logicalFirstSourceFrame
                    != AuditionPvStationBossDeathAftermathCapture.SelectStartFrame
                || proof.logicalLastSourceFrame
                    != AuditionPvStationBossDeathAftermathCapture.SelectEndFrame
                || proof.s090SelectStartFrame
                    != AuditionPvStationBossDeathAftermathCapture.S090SelectStartFrame
                || proof.s090SelectEndFrame
                    != AuditionPvStationBossDeathAftermathCapture.S090SelectEndFrame
                || proof.recordedPreHandleFrameCount
                    != AuditionPvStationBossDeathAftermathCapture.HandleFrameCount
                || proof.recordedPostHandleFrameCount
                    != AuditionPvStationBossDeathAftermathCapture.HandleFrameCount
                || !proof.recorderAutoStoppedAfterLastFrame)
            {
                throw new InvalidOperationException(
                    "G08 Recorder/logical-frame proof is incomplete.");
            }

            if (string.IsNullOrWhiteSpace(proof.runId)
                || string.IsNullOrWhiteSpace(proof.playableStageId)
                || proof.routeRevision <= 0
                || string.IsNullOrWhiteSpace(proof.routeDigest)
                || string.IsNullOrWhiteSpace(proof.transitionTokenId)
                || string.IsNullOrWhiteSpace(proof.transitionTokenDigest)
                || proof.loaderGeneration <= 0
                || string.IsNullOrWhiteSpace(proof.segmentEntryReceiptId)
                || string.IsNullOrWhiteSpace(proof.segmentEntryReceiptDigest)
                || string.IsNullOrWhiteSpace(proof.handoffTerminalReceiptId)
                || string.IsNullOrWhiteSpace(proof.handoffTerminalReceiptDigest)
                || !proof.enteredFromHandoffPending
                || !proof.exactHandoffReceiptChain
                || !proof.productTransitionProviderObserved
                || !proof.productTransitionDestinationArrived
                || !proof.productTransitionHandoffCompleted
                || proof.productTransitionGeneration == 0
                || !proof.entryGuideObservedPlaying
                || !proof.entryGuideReleased)
            {
                throw new InvalidOperationException(
                    "G08 canonical Corridor/UI-handoff/Station receipt proof is incomplete.");
            }

            float recomputedProjectileWorldRadius;
            try
            {
                recomputedProjectileWorldRadius =
                    AuditionPvStationBossDeathAftermathCapture
                        .ResolveConfiguredProjectileWorldRadius(
                            proof.projectileConfiguredLocalRadius,
                            proof.projectilePrefabLocalScale,
                            proof.projectileRootLossyScale);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "G08 physical projectile geometry proof is invalid.",
                    exception);
            }

            if (proof.phaseTransitionStartCount != 1
                || proof.phaseTransitionCompletionCount != 1
                || !proof.phaseTwoApplied
                || Mathf.Abs(
                    proof.preparedHealth
                    - AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth) > 0.001f
                || Mathf.Abs(
                    proof.bossHealthBeforeShot
                    - AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth) > 0.001f
                || proof.pressureScreensBeforeDismiss < 0
                || proof.pressureSummonsDismissed < 0
                || proof.pressureScreensAfterDismiss != 0
                || (proof.pressureScreensBeforeDismiss > 0
                    && proof.pressureSummonsDismissed
                        < proof.pressureScreensBeforeDismiss)
                || !float.IsFinite(proof.predictedBossSweepDistance)
                || Mathf.Abs(
                    proof.predictedBossSweepDistance
                    - AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactTargetDistance)
                    > AuditionPvStationBossDeathAftermathCapture
                        .NaturalImpactDistanceTolerance
                || proof.predictedNaturalImpactFrame
                    != AuditionPvStationBossDeathAftermathCapture.ImpactFrame
                || !float.IsFinite(proof.preShotPlayerPlanarStepDistance)
                || proof.preShotPlayerPlanarStepDistance <= 0.25f
                || proof.preShotPlayerPlanarStepDistance
                    > AuditionPvStationBossDeathAftermathCapture
                        .MaximumNaturalImpactTotalStepMeters
                || !string.Equals(
                    proof.projectilePrefabAssetPath,
                    AuditionPvStationBossDeathAftermathCapture
                        .PlayerRangedProjectilePrefabPath,
                    StringComparison.Ordinal)
                || !string.Equals(
                    proof.projectilePrefabAssetGuid,
                    AuditionPvStationBossDeathAftermathCapture
                        .PlayerRangedProjectilePrefabGuid,
                    StringComparison.Ordinal)
                || !float.IsFinite(proof.projectileConfiguredLocalRadius)
                || Mathf.Abs(
                    proof.projectileConfiguredLocalRadius
                    - AuditionPvStationBossDeathAftermathCapture
                        .AuthoredProjectileRadius) > 0.000001f
                || !IsFinite(proof.projectilePrefabLocalScale)
                || Vector3.Distance(
                    proof.projectilePrefabLocalScale,
                    Vector3.one
                        * AuditionPvStationBossDeathAftermathCapture
                            .AuthoredProjectilePrefabScale) > 0.000001f
                || !IsFinite(proof.projectileRootLossyScale)
                || Vector3.Distance(
                    proof.projectileRootLossyScale,
                    Vector3.one
                        * AuditionPvStationBossDeathAftermathCapture
                            .AuthoredProjectileRootScale) > 0.000001f
                || !float.IsFinite(proof.projectileConfiguredWorldRadius)
                || proof.projectileConfiguredWorldRadius <= 0f
                || Mathf.Abs(
                    proof.projectileConfiguredWorldRadius
                    - recomputedProjectileWorldRadius) > 0.000001f
                || Mathf.Abs(
                    proof.projectileConfiguredWorldRadius
                    - AuditionPvStationBossDeathAftermathCapture
                        .AuthoredProjectileWorldRadius) > 0.000001f
                || !float.IsFinite(proof.projectileObservedLocalRadius)
                || Mathf.Abs(
                    proof.projectileObservedLocalRadius
                    - AuditionPvStationBossDeathAftermathCapture
                        .AuthoredProjectileRadius) > 0.000001f
                || !IsFinite(proof.projectileObservedLossyScale)
                || Vector3.Distance(
                    proof.projectileObservedLossyScale,
                    Vector3.one
                        * AuditionPvStationBossDeathAftermathCapture
                            .AuthoredProjectilePrefabScale) > 0.000001f
                || !float.IsFinite(proof.projectileObservedWorldRadius)
                || Mathf.Abs(
                    proof.projectileObservedWorldRadius
                    - AuditionPvStationBossDeathAftermathCapture
                        .AuthoredProjectileWorldRadius) > 0.000001f
                || Mathf.Abs(
                    proof.projectileObservedWorldRadius
                    - proof.projectileConfiguredWorldRadius) > 0.000001f
                || !proof.bossPressureMovementWasEnabled
                || !proof.bossPressureMovementHoldAcquired
                || !proof.bossPoseStableThroughImpact
                || !IsFinite(proof.bossPositionAtShotArm)
                || !IsFinite(proof.bossPositionAtImpact)
                || Vector3.Distance(
                    proof.bossPositionAtShotArm,
                    proof.bossPositionAtImpact) > 0.001f
                || !float.IsFinite(proof.maximumBossPositionDriftThroughImpact)
                || proof.maximumBossPositionDriftThroughImpact < 0f
                || proof.maximumBossPositionDriftThroughImpact > 0.001f
                || !float.IsFinite(proof.maximumBossRotationDriftThroughImpact)
                || proof.maximumBossRotationDriftThroughImpact < 0f
                || proof.maximumBossRotationDriftThroughImpact > 0.001f)
            {
                throw new InvalidOperationException(
                    "G08 real Phase1-to-Phase2/HP12/unobstructed stationary-boss natural-impact setup proof is incomplete: "
                    + $"pressureBefore={proof.pressureScreensBeforeDismiss}, "
                    + $"pressureDismissed={proof.pressureSummonsDismissed}, "
                    + $"pressureAfter={proof.pressureScreensAfterDismiss}.");
            }

            if (proof.fireFrame != 1
                || proof.projectileFiredFrame != 1
                || proof.bossDiedFrame != 62
                || proof.projectileImpactFrame != 62
                || proof.terminalResolvedFrame != 62
                || proof.firstFreezeFrame != 218
                || proof.firstResultSceneFrame != 218
                || proof.firstResultConfiguredFrame != 218
                || proof.firstInteractiveFrame != 246
                || proof.aftermathCompletedFrame != 218
                || proof.inputLeaseReleasedFrame != 218
                || proof.deathStateHeldFrame != 129)
            {
                throw new InvalidOperationException(
                    "G08 exact f1/f62/f218/f246 schedule proof drifted.");
            }

            if (proof.rangedFireStartedCount != 1
                || proof.projectileFiredCount != 1
                || proof.projectileDamageAppliedCount != 1
                || proof.bossDamagedDuringShotCount != 1
                || proof.bossDiedCount != 1
                || proof.encounterTerminalResolvedCount != 1
                || proof.projectileInstanceId == 0
                || !proof.physicalProjectileObservedActiveBeforeImpact
                || !proof.projectileMovedBeforeImpact
                || !IsFinite(proof.projectileImpactPoint)
                || !IsFinite(proof.projectileImpactDirection)
                || proof.projectileImpactDirection.sqrMagnitude <= 0.0001f
                || Vector3.Distance(
                    proof.projectileSpawnPosition,
                    proof.projectilePositionAtFrame61) <= 10f
                || proof.projectileFiredSequence <= 0
                || proof.bossDiedSequence <= proof.projectileFiredSequence
                || proof.projectileImpactSequence <= proof.bossDiedSequence
                || proof.terminalResolvedSequence <= proof.projectileFiredSequence)
            {
                throw new InvalidOperationException(
                    "G08 same physical projectile flight/impact/death identity proof failed.");
            }

            if (!proof.noEarlyFreeze
                || !proof.resultAbsentBeforeRequest
                || !proof.allEightLocksObservedAtImpact
                || !proof.allEightLocksReleasedAtResult
                || !proof.deathStateAtAftermathHero
                || proof.aftermathStartedCount != 1
                || proof.aftermathCompletedCount != 1
                || !proof.aftermathCompletedSuccessfully
                || proof.aftermathBeginCount != 1
                || proof.aftermathCompleteCount != 1
                || !float.IsFinite(proof.aftermathElapsedSeconds)
                || proof.aftermathElapsedSeconds < 2.6f
                || !proof.aftermathScaleOneObserved
                || proof.aftermathScaleOneViolated
                || !proof.exclusiveCameraScheduleExact
                || proof.cameraRoleTransitionCount != 2
                || proof.firstFinisherCameraFrame != 62
                || proof.firstGameplayCameraRestoreFrame != 246
                || !proof.finisherTerminalHoldExactAt218
                || !proof.finisherReleaseExactAt246
                || !proof.finisherCameraSucceeded
                || !proof.finisherCameraReleaseScheduled
                || proof.finisherCameraInterrupted
                || proof.fallbackCameraCueSucceeded
                || proof.finisherCameraRequestVersion != 1
                || proof.finisherCameraAcquireCount != 1
                || proof.finisherCameraReleaseCount != 1
                || proof.finisherCameraControllerRequestVersion
                    != proof.finisherCameraRequestVersion
                || proof.finisherCameraSampleCount
                    != AuditionPvStationBossDeathAftermathCapture
                        .ExpectedFinisherTimelineSampleCount
                || proof.finisherCameraResultCoverReleaseSampleCount
                    != AuditionPvStationBossDeathAftermathCapture
                        .ExpectedResultCoverReleaseSampleCount
                || double.IsNaN(proof.finisherCameraLastSampledSeconds)
                || double.IsInfinity(proof.finisherCameraLastSampledSeconds)
                || Math.Abs(
                    proof.finisherCameraLastSampledSeconds
                    - OlympusStationBossTerminalFinisherCameraController
                        .RequiredTimelineDurationSeconds) > 0.0001d
                || !float.IsFinite(
                    proof.finisherCameraResultCoverReleaseElapsedSeconds)
                || Mathf.Abs(
                    proof.finisherCameraResultCoverReleaseElapsedSeconds
                    - AuditionPvStationBossDeathAftermathCapture
                        .ExpectedResultCoverReleaseSampleCount
                        / (float)AuditionPvCaptureContract.Fps) > 0.0001f
                || !proof.finisherCameraReachedTerminalSample
                || !proof.finisherCameraLeaseReleased
                || !proof.finisherCameraGameplayRestored
                || !proof.finisherCameraDisabledAtResult
                || proof.bossDeathCameraRequestCount != 0
                || proof.bossDeathCameraVersion != -1
                || proof.bossDeathCameraInterrupted
                || proof.bossDeathCameraComplete
                || proof.bossDeathVfxRequestCount != 1
                || proof.bossDeathAudioSourceDelta <= 0
                || !proof.bossDeathUsesPhaseTwoAnchor
                || proof.deathMotionRequestCount != 1
                || !proof.motionIsDead
                || !proof.motionAttacksStopped
                || !proof.animatorInDeathState)
            {
                throw new InvalidOperationException(
                    "G08 2.6s input/time/camera/VFX/audio/motion proof failed: "
                    + proof.aftermathLastError + " | " + proof.aftermathQualityWarning);
            }

            if (!proof.resultInteractiveAt246
                || proof.overlayPresentationSucceededCount != 1
                || !proof.hudWasActiveAtFire
                || !proof.hudWasActiveAtImpact
                || !proof.hudYieldedAtResult
                || !proof.pocketClearMarkerReferenceUnbound
                || !proof.pocketClearMarkerInactiveAtEnd
                || !proof.terminalBoundaryVisualHiddenAtEnd
                || !proof.overlayShown
                || !proof.overlayFrozen
                || !proof.resultSummarySameInstance
                || !proof.presentedSummarySameInstance
                || string.IsNullOrWhiteSpace(proof.outcomeFactDigest)
                || !string.Equals(
                    proof.committedSummaryDigest,
                    proof.presentedSummaryDigest,
                    StringComparison.Ordinal)
                || !proof.terminalFactsExact
                || proof.terminalRecordReceiptCount != 1
                || proof.rootAdmissionSequence <= 0
                || proof.terminalEpoch <= 0
                || string.IsNullOrWhiteSpace(proof.terminalEpochEvidenceDigest)
                || string.IsNullOrWhiteSpace(proof.terminalClosureDigest))
            {
                throw new InvalidOperationException(
                    "G08 terminal facts/committed SameAs/HUD/result lifecycle proof failed.");
            }

            if (!proof.stateRestored
                || !proof.eventsReleased
                || !proof.presentationClockReleased
                || !proof.cadenceReleased
                || !proof.bossPressureMovementRestored
                || !proof.transitionCaptureStateReleased
                || !proof.globalCaptureStateRestored
                || !proof.editModeSceneCleanupExact
                || !proof.editModeGlobalCleanupExact
                || !string.IsNullOrEmpty(proof.cleanupFailure))
            {
                throw new InvalidOperationException(
                    "G08 event/input/global/scene cleanup proof failed.");
            }

            ValidateCompositionEvidence(proof);

            if (proof.pixelSampleStride != SequencePixelSampleStride
                || proof.pixelSampleCount != ExpectedSequencePixelSampleCount
                || proof.frameDeltaPixelSampleStride != FrameDeltaPixelSampleStride
                || proof.frameDeltaPixelSampleCount
                    != ExpectedFrameDeltaPixelSampleCount
                || proof.frameDeltaChangedRgbSumCutoff
                    != FrameDeltaChangedRgbSumCutoff
                || proof.impactDeltaFromFrame != ImpactDeltaFromFrame
                || proof.impactDeltaToFrame != ImpactDeltaToFrame
                || proof.aftermathDeltaFromFrame != AftermathDeltaFromFrame
                || proof.aftermathDeltaToFrame != AftermathDeltaToFrame
                || proof.resultAppearanceFromFrame != ResultAppearanceFromFrame
                || proof.resultAppearanceToFrame != ResultAppearanceToFrame
                || proof.resultEntranceFromFrame != ResultEntranceFromFrame
                || proof.resultEntranceToFrame != ResultEntranceToFrame
                || proof.resultSurfaceFrame != ResultSurfaceFrame
                || proof.resultSurfaceRoiX != ResultSurfaceRawBottomLeftRoi.x
                || proof.resultSurfaceRoiY != ResultSurfaceRawBottomLeftRoi.y
                || proof.resultSurfaceRoiWidth != ResultSurfaceRawBottomLeftRoi.width
                || proof.resultSurfaceRoiHeight != ResultSurfaceRawBottomLeftRoi.height
                || proof.resultSurfaceSampleStride != ResultSurfaceSampleStride
                || proof.resultSurfaceSampleCount != ExpectedResultSurfaceSampleCount
                || proof.resultBrightMinimumChannel != ResultBrightMinimumChannel
                || proof.resultNavyMaximumLuma != ResultNavyMaximumLuma
                || proof.resultBlueMinimumChannel != ResultBlueMinimumChannel
                || proof.resultBlueMinimumRedDelta != ResultBlueMinimumRedDelta
                || proof.resultBlueMinimumGreenDelta != ResultBlueMinimumGreenDelta
                || !IsRatio(proof.sequenceBlackRatio)
                || !IsRatio(proof.sequenceMagentaRatio)
                || !IsRatio(proof.maximumFrameMagentaRatio)
                || !IsFiniteInRange(proof.healthyFramePercent, 0d, 100d)
                || !IsMeanAbsoluteRgb(proof.impactMeanAbsoluteRgb)
                || !IsRatio(proof.impactChangedRatio)
                || !IsMeanAbsoluteRgb(proof.aftermathEvolutionMeanAbsoluteRgb)
                || !IsRatio(proof.aftermathEvolutionChangedRatio)
                || !IsMeanAbsoluteRgb(proof.resultAppearanceMeanAbsoluteRgb)
                || !IsRatio(proof.resultAppearanceChangedRatio)
                || !IsMeanAbsoluteRgb(proof.resultEntranceMeanAbsoluteRgb)
                || !IsRatio(proof.resultEntranceChangedRatio)
                || proof.resultBrightSamples < 0
                || proof.resultBrightSamples > proof.resultSurfaceSampleCount
                || proof.resultNavySamples < 0
                || proof.resultNavySamples > proof.resultSurfaceSampleCount
                || proof.resultBlueSamples < 0
                || proof.resultBlueSamples > proof.resultSurfaceSampleCount)
            {
                throw new InvalidOperationException(
                    "G08 QHD pixel telemetry is absent, out of domain, "
                    + "or algorithm metadata drifted.");
            }

            if (requirePixelCalibration && !PixelCalibrationLocked)
            {
                throw new G08PixelCalibrationRequiredException(
                    "G08 pixel calibration is not locked. Review the runtime failure telemetry, "
                    + "pin independently justified thresholds, and set PixelCalibrationLocked=true.");
            }

            if (requirePixelCalibration)
            {
                ValidateLockedPixelThresholdsForTests(proof);
            }

            if (!AuditionPvSha256.IsSha256(proof.frameHashLedgerSha256)
                || proof.frameHashLedgerEntryCount
                    != AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount
                || !AuditionPvSha256.IsSha256(proof.warmupEvidenceSha256)
                || !AuditionPvSha256.IsSha256(proof.bl10Sha256)
                || !AuditionPvSha256.IsSha256(proof.bl11Sha256)
                || !AuditionPvSha256.IsSha256(proof.bl12Sha256)
                || !AuditionPvSha256.IsSha256(
                    proof.captureStartProvenanceSha256)
                || proof.dependencyHashCount <= 0)
            {
                throw new InvalidOperationException(
                    "G08 hash/provenance proof is incomplete.");
            }
        }

        internal static void ValidateCompositionEvidence(RuntimeProof proof)
        {
            if (proof?.renderEvidence == null
                || proof.renderEvidence.Length != CompositionEvidenceFrames.Length
                || proof.renderEvidence.Any(value => value == null)
                || !proof.renderEvidence.Select(value => value.frame)
                    .SequenceEqual(CompositionEvidenceFrames))
            {
                throw new InvalidOperationException(
                    "G08 composition evidence must be the exact ordered source "
                    + "f241/f242/f296/f361/f426 set (logical f61/f62/f116/f181/f246).");
            }

            RenderEvidence gameplay = proof.renderEvidence[0];
            if (!gameplay.gameplayCameraExact
                || gameplay.finisherCameraExact
                || !gameplay.exclusiveCameraRoleExact
                || !gameplay.combatHudVisible
                || !float.IsFinite(gameplay.projectionAspect)
                || Mathf.Abs(
                    gameplay.projectionAspect
                    - ExpectedCompositionProjectionAspect) > 0.0001f
                || !gameplay.bossEnvelopeVisible
                || gameplay.bossEnvelopeFullyOutsideFrustum
                || gameplay.bossEnvelopeRendererCount < 2
                || !IsFiniteInRange(
                    gameplay.bossEnvelopeMaxExtentRatio,
                    MinimumBossEnvelopeReadableExtentRatio,
                    float.MaxValue)
                || !IsFiniteInRange(
                    gameplay.bossEnvelopeWidthRatio,
                    0d,
                    float.MaxValue)
                || !IsFiniteInRange(
                    gameplay.bossEnvelopeHeightRatio,
                    0d,
                    float.MaxValue)
                || Mathf.Abs(
                    gameplay.bossEnvelopeMaxExtentRatio
                    - Mathf.Max(
                        gameplay.bossEnvelopeWidthRatio
                            * gameplay.projectionAspect,
                        gameplay.bossEnvelopeHeightRatio)) > 0.0001f
                || !string.Equals(gameplay.cameraRole, "gameplay", StringComparison.Ordinal)
                || !string.Equals(
                    gameplay.objectiveText,
                    AuditionPvStationBossDeathAftermathCapture
                        .ExpectedPlayerFacingKoObjective,
                    StringComparison.Ordinal)
                || !gameplay.objectiveForbiddenInternalTokensAbsent
                || !string.Equals(
                    gameplay.bossLabelText,
                    AuditionPvStationBossDeathAftermathCapture.ExpectedBossDisplayName,
                    StringComparison.Ordinal)
                || !gameplay.pocketClearMarkerReferenceUnbound
                || !gameplay.pocketClearMarkerPresent
                || !gameplay.pocketClearMarkerInactive)
            {
                throw new InvalidOperationException(
                    "G08 f61 causal gameplay handle/objective/boss-label/marker truth failed.");
            }

            RenderEvidence[] finisher = proof.renderEvidence.Skip(1).Take(3).ToArray();
            foreach (RenderEvidence value in finisher)
            {
                bool playerCompositionExact = value.playerFullyOutsideFrustum
                    ? !value.playerFullyInsideFrustum
                        && !value.playerPartiallyClipped
                    : value.playerFullyInsideFrustum
                        && value.playerSafeViewport
                        && !value.playerPartiallyClipped
                        && IsFiniteInRange(
                            value.playerBodyHeightRatio,
                            MinimumVisiblePlayerBodyHeightRatio,
                            MaximumVisiblePlayerBodyHeightRatio);
                bool impactFrame = value.frame == SourceFrame(
                    AuditionPvStationBossDeathAftermathCapture.ImpactFrame);
                bool hudModeExact = value.combatHudVisible == impactFrame;
                bool hudCopyExact = !impactFrame
                    || (string.Equals(
                            value.objectiveText,
                            AuditionPvStationBossDeathAftermathCapture
                                .ExpectedPlayerFacingKoObjective,
                            StringComparison.Ordinal)
                        && value.objectiveForbiddenInternalTokensAbsent
                        && string.Equals(
                            value.bossLabelText,
                            AuditionPvStationBossDeathAftermathCapture
                                .ExpectedBossDisplayName,
                            StringComparison.Ordinal));
                bool bodyGeometryExact = string.Equals(
                        value.bossBodyRendererNames,
                        "DB_AkazaPhase2Combined_BodySilhouette|DB_AkazaPhase2Combined_FaceHairDetail",
                        StringComparison.Ordinal)
                    && value.bossBodyRendererCount == 2
                    && IsFiniteInRange(value.bossBodyWidthRatio, 0d, 1d)
                    && IsFiniteInRange(value.bossBodyHeightRatio, 0d, 1d)
                    && IsFiniteInRange(
                        value.bossBodyMaxExtentRatio,
                        0d,
                        float.MaxValue)
                    && Mathf.Abs(value.bossBodyMaxExtentRatio - Mathf.Max(
                        value.bossBodyWidthRatio * value.projectionAspect,
                        value.bossBodyHeightRatio)) <= 0.0001f
                    && IsFinite(value.bossViewport)
                    && IsFinite(value.bossPixelExtent)
                    && value.bossPixelExtent.x >= 8f
                    && value.bossPixelExtent.y >= 8f;
                bool bodySizeExact = impactFrame
                    ? IsFiniteInRange(
                        value.bossBodyHeightRatio,
                        MinimumFinisherBossBodyHeightRatio,
                        MaximumFinisherBossBodyHeightRatio)
                    : IsFiniteInRange(
                        value.bossBodyMaxExtentRatio,
                        MinimumTerminalBossBodyMaxExtentRatio,
                        MaximumTerminalBossBodyMaxExtentRatio);
                bool envelopePresenceExact = value.bossEnvelopeVisible
                    && !value.bossEnvelopeFullyOutsideFrustum
                    && value.bossEnvelopeRendererCount >= 2
                    && IsFinite(value.bossEnvelopeViewport)
                    && IsFinite(value.bossEnvelopePixelExtent)
                    && IsFiniteInRange(
                        value.bossEnvelopeWidthRatio,
                        0d,
                        float.MaxValue)
                    && IsFiniteInRange(
                        value.bossEnvelopeHeightRatio,
                        0d,
                        float.MaxValue)
                    && IsFiniteInRange(
                        value.bossEnvelopeMaxExtentRatio,
                        MinimumBossEnvelopeReadableExtentRatio,
                        float.MaxValue)
                    && Mathf.Abs(
                        value.bossEnvelopeMaxExtentRatio
                        - Mathf.Max(
                            value.bossEnvelopeWidthRatio
                                * value.projectionAspect,
                            value.bossEnvelopeHeightRatio)) <= 0.0001f;
                if (value.gameplayCameraExact
                    || !value.finisherCameraExact
                    || !value.exclusiveCameraRoleExact
                    || !float.IsFinite(value.projectionAspect)
                    || Mathf.Abs(
                        value.projectionAspect
                        - ExpectedCompositionProjectionAspect) > 0.0001f
                    || !string.Equals(value.cameraRole, "finisher", StringComparison.Ordinal)
                    || !value.bossFullyInsideFrustum
                    || value.bossFullyOutsideFrustum
                    || !value.bossSafeViewport
                    || value.bossPartiallyClipped
                    || !bodyGeometryExact
                    || !bodySizeExact
                    || !envelopePresenceExact
                    || !playerCompositionExact
                    || !hudModeExact
                    || !hudCopyExact
                    || !value.pocketClearMarkerReferenceUnbound
                    || !value.pocketClearMarkerPresent
                    || !value.pocketClearMarkerInactive
                    || !value.terminalBoundaryVisualPresent
                    || !value.terminalBoundaryVisualHidden)
                {
                    throw new InvalidOperationException(
                        $"G08 f{value.frame} authored finisher framing or visual-truth evidence failed.");
                }
            }

            RenderEvidence impact = finisher[0];
            RenderEvidence terminalHero = finisher[1];
            RenderEvidence terminalHold = finisher[2];
            if (Mathf.Abs(
                    terminalHero.bossBodyMaxExtentRatio
                    - terminalHold.bossBodyMaxExtentRatio)
                > MaximumTerminalBossBodyMaxExtentSpread)
            {
                throw new InvalidOperationException(
                    "G08 terminal boss tight max-axis stability exceeded the exact 0.05 viewport span.");
            }

            Vector2 impactCoreAxis = ValidateBossCoreAxisEvidence(impact);
            Vector2 terminalHeroCoreAxis = ValidateBossCoreAxisEvidence(terminalHero);
            Vector2 terminalHoldCoreAxis = ValidateBossCoreAxisEvidence(terminalHold);
            float heroOrientationDelta = Vector2.Angle(
                impactCoreAxis,
                terminalHeroCoreAxis);
            float heldOrientationDelta = Vector2.Angle(
                impactCoreAxis,
                terminalHoldCoreAxis);
            float terminalHoldDrift = Vector2.Angle(
                terminalHeroCoreAxis,
                terminalHoldCoreAxis);
            if (heroOrientationDelta
                    < MinimumTerminalBossCoreAxisOrientationDeltaDegrees
                || heldOrientationDelta
                    < MinimumTerminalBossCoreAxisOrientationDeltaDegrees
                || terminalHoldDrift
                    > MaximumTerminalBossCoreAxisHoldDriftDegrees)
            {
                throw new InvalidOperationException(
                    "G08 projected hips-to-head evidence did not prove a materially different terminal orientation at f116 held through f181.");
            }

            bool centerDriftExceeded = false;
            for (int left = 0; left < finisher.Length; left++)
            {
                for (int right = left + 1; right < finisher.Length; right++)
                {
                    centerDriftExceeded |= Vector2.Distance(
                        new Vector2(
                            finisher[left].bossViewport.x,
                            finisher[left].bossViewport.y),
                        new Vector2(
                            finisher[right].bossViewport.x,
                            finisher[right].bossViewport.y))
                        > MaximumFinisherBossCenterDrift;
                }
            }

            if (centerDriftExceeded)
            {
                throw new InvalidOperationException(
                    "G08 finisher boss viewport-center drift exceeded the exact 0.08 radius.");
            }

            RenderEvidence result = proof.renderEvidence[4];
            if (!result.gameplayCameraExact
                || result.finisherCameraExact
                || !result.exclusiveCameraRoleExact
                || !string.Equals(result.cameraRole, "gameplay", StringComparison.Ordinal)
                || !result.finisherLeaseReleased
                || result.combatHudVisible
                || !result.resultCanvasVisible
                || !result.resultInteractive
                || !result.redundantClearTextPresent
                || !result.redundantClearTextInactive
                || !result.realClearIconPresent
                || !result.realClearIconActive
                || !result.terminalBoundaryVisualPresent
                || !result.terminalBoundaryVisualHidden
                || !result.pocketClearMarkerReferenceUnbound
                || !result.pocketClearMarkerPresent
                || !result.pocketClearMarkerInactive)
            {
                throw new InvalidOperationException(
                    "G08 f246 gameplay-camera restore/authored-result/CLEAR visual truth failed.");
            }
        }

        private static Vector2 ValidateBossCoreAxisEvidence(RenderEvidence value)
        {
            if (value == null
                || !string.Equals(
                    value.bossCoreAxisSource,
                    BossCoreAxisSource,
                    StringComparison.Ordinal)
                || !float.IsFinite(value.projectionAspect)
                || Mathf.Abs(
                    value.projectionAspect
                    - ExpectedCompositionProjectionAspect) > 0.0001f
                || !IsFinite(value.bossCoreAxisHipsViewport)
                || !IsFinite(value.bossCoreAxisHeadViewport)
                || value.bossCoreAxisHipsViewport.z <= 0f
                || value.bossCoreAxisHeadViewport.z <= 0f
                || !IsFiniteInRange(value.bossCoreAxisHipsViewport.x, 0d, 1d)
                || !IsFiniteInRange(value.bossCoreAxisHipsViewport.y, 0d, 1d)
                || !IsFiniteInRange(value.bossCoreAxisHeadViewport.x, 0d, 1d)
                || !IsFiniteInRange(value.bossCoreAxisHeadViewport.y, 0d, 1d))
            {
                throw new InvalidOperationException(
                    $"G08 f{value?.frame ?? -1} projected authored Akaza hip/head evidence is missing or outside the capture.");
            }

            Vector2 axis = new(
                (value.bossCoreAxisHeadViewport.x
                    - value.bossCoreAxisHipsViewport.x)
                    * value.projectionAspect,
                value.bossCoreAxisHeadViewport.y
                    - value.bossCoreAxisHipsViewport.y);
            float measuredLength = axis.magnitude;
            if (!float.IsFinite(value.bossCoreAxisViewportLength)
                || measuredLength < MinimumBossCoreAxisViewportLength
                || Mathf.Abs(value.bossCoreAxisViewportLength - measuredLength)
                    > 0.0001f)
            {
                throw new InvalidOperationException(
                    $"G08 f{value.frame} projected authored Akaza hip/head axis is too short or internally inconsistent.");
            }

            return axis / measuredLength;
        }

        internal static void ValidateLockedPixelThresholdsForTests(
            RuntimeProof proof)
        {
            if (proof == null
                || proof.sequenceBlackRatio > MaximumSequenceBlackRatio
                || proof.sequenceMagentaRatio > MaximumSequenceMagentaRatio
                || proof.healthyFramePercent < MinimumHealthyFramePercent
                || proof.maximumFrameMagentaRatio > MaximumFrameMagentaRatio
                || proof.impactMeanAbsoluteRgb < MinimumImpactMeanAbsoluteRgb
                || proof.impactChangedRatio < MinimumImpactChangedRatio
                || proof.aftermathEvolutionMeanAbsoluteRgb
                    < MinimumAftermathEvolutionMeanAbsoluteRgb
                || proof.aftermathEvolutionChangedRatio
                    < MinimumAftermathEvolutionChangedRatio
                || proof.resultAppearanceMeanAbsoluteRgb
                    < MinimumResultAppearanceMeanAbsoluteRgb
                || proof.resultAppearanceChangedRatio
                    < MinimumResultAppearanceChangedRatio
                || proof.resultEntranceMeanAbsoluteRgb
                    < MinimumResultEntranceMeanAbsoluteRgb
                || proof.resultEntranceChangedRatio
                    < MinimumResultEntranceChangedRatio
                || proof.resultBrightSamples < MinimumResultBrightSamples
                || proof.resultNavySamples < MinimumResultNavySamples
                || proof.resultBlueSamples < MinimumResultBlueSamples)
            {
                throw new InvalidOperationException(
                    "G08 QHD pixel health/delta/result-surface gates failed.");
            }
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y)
                && float.IsFinite(value.z);
        }

        private static bool IsFinite(Vector2 value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y);
        }

        private static bool IsRatio(double value)
        {
            return IsFiniteInRange(value, 0d, 1d);
        }

        private static bool IsMeanAbsoluteRgb(double value)
        {
            return IsFiniteInRange(value, 0d, byte.MaxValue);
        }

        private static bool IsFiniteInRange(double value, double minimum, double maximum)
        {
            return !double.IsNaN(value)
                && !double.IsInfinity(value)
                && value >= minimum
                && value <= maximum;
        }

        private static void BeginCapture(bool batchMode)
        {
            if (SessionState.GetBool(SessionActiveKey, false)
                || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                throw new InvalidOperationException(
                    "A G08 capture cannot start during another owned capture, Play Mode, compilation, or asset update.");
            }

            EnsureNoDirtyOpenScenes();
            DateTime startedAtUtc = DateTime.UtcNow;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded || git.isDirty)
            {
                throw new InvalidOperationException(
                    "G08 golden capture requires a successful clean Git provenance probe: "
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
            AuditionPvDependencyHash[] dependencyHashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            if (dependencyHashes.Any(value => value == null
                    || !value.exists
                    || !AuditionPvSha256.IsSha256(value.sha256)))
            {
                throw new InvalidOperationException(
                    "G08 requires every declared product/capture dependency to exist and hash.");
            }

            AuditionPvStationBossDeathAftermathOutput output = null;
            PersistedRunnerState state = null;
            try
            {
                output = AuditionPvStationBossDeathAftermathCapture.ReserveNewOutput(
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
                    dependencyHashesAtStart = dependencyHashes,
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
                ScheduleResume();
                EditorSceneManager.OpenScene(
                    AuditionPvStationBossDeathAftermathCapture.CorridorScenePath,
                    OpenSceneMode.Single);
                if (SceneManager.GetActiveScene().isDirty)
                {
                    throw new InvalidOperationException(
                        "Fresh Corridor scene became dirty before G08 Play Mode.");
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
            if (IsOwnedSession()
                && (change == PlayModeStateChange.EnteredPlayMode
                    || change == PlayModeStateChange.ExitingPlayMode
                    || change == PlayModeStateChange.EnteredEditMode))
            {
                ScheduleResume();
            }
        }

        private static void ScheduleResume()
        {
            EnsureResumeWatchdog();
            if (resumeScheduled)
            {
                return;
            }

            resumeScheduled = true;
            EditorApplication.delayCall += ResumeOwnedSession;
        }

        private static void EnsureResumeWatchdog()
        {
            if (resumeWatchdogRegistered)
            {
                return;
            }

            resumeWatchdogRegistered = true;
            EditorApplication.update -= ResumeOwnedSessionWatchdog;
            EditorApplication.update += ResumeOwnedSessionWatchdog;
        }

        private static void ResumeOwnedSessionWatchdog()
        {
            ResumeWatchdogAction action = DetermineResumeWatchdogAction(
                IsOwnedSession(),
                EditorApplication.isPlayingOrWillChangePlaymode,
                EditorApplication.isCompiling,
                EditorApplication.isUpdating);
            if (action == ResumeWatchdogAction.KeepWaiting)
            {
                return;
            }

            EditorApplication.update -= ResumeOwnedSessionWatchdog;
            resumeWatchdogRegistered = false;
            if (action == ResumeWatchdogAction.Unregister)
            {
                return;
            }

            // A delayCall requeued from inside Unity's updating pass can be
            // discarded when that pass clears its current queue while leaving
            // resumeScheduled true.  The update watchdog owns the fallback and
            // cancels any still-live duplicate before running directly.
            EditorApplication.delayCall -= ResumeOwnedSession;
            resumeScheduled = false;
            ResumeOwnedSession();
        }

        internal static ResumeWatchdogAction DetermineResumeWatchdogAction(
            bool ownedSession,
            bool isPlayingOrWillChangePlaymode,
            bool isCompiling,
            bool isUpdating)
        {
            if (!ownedSession)
            {
                return ResumeWatchdogAction.Unregister;
            }

            return isPlayingOrWillChangePlaymode || isCompiling || isUpdating
                ? ResumeWatchdogAction.KeepWaiting
                : ResumeWatchdogAction.Run;
        }

        private static void ResumeOwnedSession()
        {
            resumeScheduled = false;
            if (!IsOwnedSession())
            {
                return;
            }

            string statePath = SessionState.GetString(SessionStatePathKey, string.Empty);
            string outputDirectory = SessionState.GetString(
                SessionOutputDirectoryKey,
                string.Empty);
            string captureId = SessionState.GetString(SessionCaptureIdKey, string.Empty);
            bool batchMode = SessionState.GetBool(SessionBatchKey, false);
            try
            {
                ValidateSessionRecoveryLocationForRoot(
                    statePath,
                    outputDirectory,
                    captureId,
                    AuditionPvCaptureContract.OutputRoot);
            }
            catch (Exception exception)
            {
                ClearSession();
                Debug.LogException(exception);
                if (batchMode)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            bool committedManifest = !EditorApplication.isPlaying
                && IsValidCommittedManifestAt(
                    outputDirectory,
                    captureId,
                    AuditionPvCaptureContract.OutputRoot);
            string terminalFault = SessionState.GetString(
                SessionTerminalFaultKey,
                string.Empty);
            SessionRecoveryDecision recoveryDecision =
                DetermineSessionRecoveryDecision(
                    EditorApplication.isPlaying,
                    committedManifest,
                    terminalFault);
            if (recoveryDecision == SessionRecoveryDecision.CommittedManifest)
            {
                ClearSession();
                if (batchMode)
                {
                    EditorApplication.Exit(0);
                }

                return;
            }

            if (recoveryDecision == SessionRecoveryDecision.TerminalFault)
            {
                Exception recoveryFailure = RecoverTerminalPersistenceFaultForRoot(
                    outputDirectory,
                    captureId,
                    AuditionPvCaptureContract.OutputRoot,
                    terminalFault,
                    ClearSession,
                    batchMode ? code => EditorApplication.Exit(code) : null);
                if (recoveryFailure != null)
                {
                    Debug.LogException(recoveryFailure);
                }

                return;
            }

            PersistedRunnerState state;
            try
            {
                state = LoadState(statePath);
                ValidateSessionStateAuthority(
                    outputDirectory,
                    captureId,
                    batchMode,
                    state);
            }
            catch (Exception exception)
            {
                var recoveryState = new PersistedRunnerState
                {
                    schema = RunnerSchema,
                    phase = RunnerPhase.FailedInPlayMode.ToString(),
                    batchMode = batchMode,
                    captureId = captureId,
                    outputRoot = AuditionPvCaptureContract.OutputRoot,
                    outputDirectory = outputDirectory,
                    baselineDirectory = Path.Combine(
                        outputDirectory,
                        AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName)
                };
                TryWriteFailureArtifact(
                    outputDirectory,
                    "state-load",
                    exception,
                    null,
                    recoveryState);
                ClearSession();
                Debug.LogException(exception);
                if (batchMode)
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
                }
                else if (phase == RunnerPhase.Recording && activeBehaviour == null)
                {
                    NotifyPlayModeFinished(
                        statePath,
                        state,
                        state.runtimeProof,
                        new InvalidOperationException(
                            "A domain reload interrupted G08 Recorder."));
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
                    "Play Mode exited before G08 Recorder reported completion.";
                state.phase = RunnerPhase.FailedInPlayMode.ToString();
                SaveState(statePath, state);
            }

            if (phase == RunnerPhase.Recording
                || phase == RunnerPhase.AwaitingEditMode
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
                    AuditionPvStationBossDeathAftermathCapture.CorridorScenePath,
                    StringComparison.Ordinal))
            {
                NotifyPlayModeFinished(
                    statePath,
                    state,
                    null,
                    new InvalidOperationException(
                        "G08 entered Play Mode without the fresh Corridor scene."));
                return;
            }

            state.phase = RunnerPhase.Recording.ToString();
            SaveState(statePath, state);
            var root = new GameObject("[AuditionPV_G08_GoldenRunner]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            UnityEngine.Object.DontDestroyOnLoad(root);
            activeBehaviour = root.AddComponent<
                AuditionPvStationBossDeathAftermathGoldenRunnerBehaviour>();
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
            try
            {
                SaveState(statePath, state);
            }
            catch (Exception persistenceFailure)
            {
                Exception terminalFailure = Combine(failure, persistenceFailure)
                    ?? persistenceFailure;
                state.failure = terminalFailure.ToString();
                state.phase = RunnerPhase.FailedInPlayMode.ToString();
                try
                {
                    SessionState.SetString(
                        SessionTerminalFaultKey,
                        terminalFailure.ToString());
                }
                catch (Exception markerFailure)
                {
                    terminalFailure = Combine(terminalFailure, markerFailure);
                }

                TryWriteFailureArtifact(
                    state.outputDirectory,
                    "playmode-terminal-persistence",
                    terminalFailure,
                    state.runtimeProof,
                    state);
            }
            finally
            {
                EnsureResumeWatchdog();
                activeBehaviour = null;
                EditorApplication.isPlaying = false;
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
                ValidatePersistedStateLocationForRoot(
                    statePath,
                    state,
                    AuditionPvCaptureContract.OutputRoot);
                AuditionPvStationBossDeathAftermathCapture.ReopenCorridorAfterPlayMode();
                Scene reopened = SceneManager.GetActiveScene();
                if (!reopened.IsValid()
                    || !reopened.isLoaded
                    || reopened.isDirty
                    || !string.Equals(
                        reopened.path,
                        AuditionPvStationBossDeathAftermathCapture.CorridorScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G08 did not reopen an unmodified Corridor scene.");
                }

                RuntimeProof proof = state.runtimeProof
                    ?? throw new InvalidOperationException(
                        "G08 runtime proof is missing.");
                proof.editModeSceneCleanupExact = SceneManager.sceneCount == 1
                    && !SceneManager.GetSceneByName(
                        AuditionPvStationBossDeathAftermathCapture.StageClearSceneName)
                        .isLoaded
                    && UnityEngine.Object.FindObjectsByType<
                        AuditionPvStationBossDeathAftermathDirector>(
                            FindObjectsInactive.Include,
                            FindObjectsSortMode.None).Length == 0;
                proof.editModeGlobalCleanupExact = Mathf.Abs(Time.timeScale - 1f) <= 0.0001f
                    && !PresentationClock.IsManuallyDriven
                    && BossCombatCadenceScheduler.ExternalSuspensionCount == 0
                    && UISceneTransitionHandoffOwner.CurrentOwner == null
                    && !UITransitionHandoffService.HasProvider;
                state.runtimeProof = proof;
                SaveState(statePath, state);
                if (!string.IsNullOrWhiteSpace(state.failure))
                {
                    throw new InvalidOperationException(
                        "G08 PlayMode recording failed.\n" + state.failure);
                }

                FinalizeSuccessfulCapture(state);
                success = true;
            }
            catch (Exception exception)
            {
                if (IsValidCommittedManifestAt(
                    state.outputDirectory,
                    state.captureId,
                    state.outputRoot,
                    state))
                {
                    success = true;
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
                bool batchMode = state.batchMode;
                string output = state.outputDirectory;
                ClearSession();
                finalizing = false;
                if (success)
                {
                    Debug.Log("[AuditionPV] G08 boss aftermath passed: " + output);
                    if (batchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(output);
                    }
                }
                else if (batchMode)
                {
                    EditorApplication.Exit(1);
                }
                else if (failure != null)
                {
                    EditorUtility.DisplayDialog(
                        "G08 golden capture failed",
                        failure.Message,
                        "OK");
                }
            }
        }

        private static void FinalizeSuccessfulCapture(PersistedRunnerState state)
        {
            ValidatePersistedStateLayoutForRoot(
                state,
                AuditionPvCaptureContract.OutputRoot);
            RuntimeProof proof = state.runtimeProof;
            string frames = Path.Combine(
                state.outputDirectory,
                "frames",
                AuditionPvStationBossDeathAftermathCapture.ShotId);
            string evidence = Path.Combine(state.outputDirectory, EvidenceFolderName);
            string warmup = RemapRawFrames(frames, evidence);
            ValidateDecodablePngFile(
                warmup,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            ValidateLogicalFrameSequence(frames);
            for (int frame = 0;
                frame < AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
                frame++)
            {
                ValidatePngFile(
                    Path.Combine(
                        frames,
                        AuditionPvStationBossDeathAftermathCapture.FrameFileName(frame)),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
            }

            AnalyzeFrames(frames, proof);
            string ledger = BuildFrameHashLedger(frames);
            proof.frameHashLedgerPath = Path.Combine(evidence, FrameHashLedgerFileName)
                .Replace('\\', '/');
            proof.frameHashLedgerSha256 = AuditionPvSha256.TextHash(ledger);
            proof.frameHashLedgerEntryCount =
                AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
            proof.warmupEvidencePath = warmup;
            proof.warmupEvidenceSha256 = AuditionPvSha256.FileHash(warmup);
            proof.bl10Sha256 = FrameHash(
                frames,
                SourceFrame(AuditionPvStationBossDeathAftermathCapture.ImpactFrame));
            proof.bl11Sha256 = FrameHash(
                frames,
                SourceFrame(
                    AuditionPvStationBossDeathAftermathCapture.AftermathHeroFrame));
            proof.bl12Sha256 = FrameHash(
                frames,
                SourceFrame(
                    AuditionPvStationBossDeathAftermathCapture.InteractiveResultFrame));

            AuditionPvGitSnapshot gitAtEnd = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            ValidateStableGitSnapshot(CreateGitSnapshot(state), gitAtEnd);
            string[] dependencyPathsAtEnd = CollectCaptureDependencyPaths();
            if (!state.dependencyPaths.SequenceEqual(
                dependencyPathsAtEnd,
                StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "G08 dependency path set changed while recording.");
            }

            AuditionPvDependencyHash[] hashesAtEnd =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPathsAtEnd);
            ValidateStableDependencies(state.dependencyHashesAtStart, hashesAtEnd);
            proof.dependencyHashCount = state.dependencyHashesAtStart.Length;
            proof.captureStartProvenanceSha256 =
                ComputeCaptureStartProvenanceSha256(state);
            ValidateRuntimeProofBeforePixelCalibration(proof);
            if (!PixelCalibrationLocked)
            {
                throw new G08PixelCalibrationRequiredException(
                    "G08 clean telemetry take completed, but pixel calibration is not locked. "
                    + "No success artifacts may be published from this first take.");
            }

            ValidateRuntimeProofForPublication(proof);

            string failurePath = Path.Combine(state.outputDirectory, FailureFileName);
            if (File.Exists(failurePath))
            {
                throw new InvalidOperationException(
                    "G08 success cannot coexist with a failure artifact.");
            }

            CopyBaselines(state, frames, proof);
            WriteTextNew(proof.frameHashLedgerPath, ledger);
            ValidateFrameHashLedger(
                frames,
                proof.frameHashLedgerPath,
                proof.frameHashLedgerSha256);
            string proofPath = Path.Combine(evidence, RuntimeProofFileName);
            DateTime startedAtUtc = DateTime.Parse(
                state.startedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
            AuditionPvShotManifestEntry[] shots =
            {
                AuditionPvStationBossDeathAftermathCapture
                    .CreateShotManifestEntry()
            };
            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationBossDeathAftermathCapture
                    .CreateBaselineManifestEntries();
            AuditionPvCaptureManifest captureCoreManifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    shots,
                    baselines,
                    Array.Empty<AuditionPvTestResult>(),
                    createdAtUtc: startedAtUtc,
                    gitSnapshot: CreateGitSnapshot(state),
                    engineSnapshot: CopyEngine(state.engine),
                    dependencyHashSnapshot: state.dependencyHashesAtStart);
            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator
                    .CaptureCoreSha256(captureCoreManifest);
            if (!AuditionPvSha256.IsSha256(captureCoreSha256))
            {
                throw new InvalidDataException(
                    "G08 could not create its immutable Gate capture-core identity.");
            }

            WriteJsonNew(proofPath, new RuntimeProofArtifact
            {
                schema = RuntimeProofSchema,
                captureId = state.captureId,
                sourceCaptureCoreSha256 = captureCoreSha256,
                sourceRangeStartFrame =
                    AuditionPvStationBossDeathAftermathCapture.FirstFrame,
                sourceRangeEndFrame =
                    AuditionPvStationBossDeathAftermathCapture.LastFrame,
                selectStartFrame =
                    AuditionPvStationBossDeathAftermathCapture.S090SelectStartFrame,
                selectEndFrame =
                    AuditionPvStationBossDeathAftermathCapture.S090SelectEndFrame,
                sourceFrameLedger = new AuditionPvPinnedArtifact
                {
                    path = Path.GetFullPath(proof.frameHashLedgerPath)
                        .Replace('\\', '/'),
                    sha256 = AuditionPvSha256.FileHash(proof.frameHashLedgerPath)
                },
                mapping = RuntimeMappingDescription,
                gameplay = RuntimeGameplayDescription,
                runtime = proof
            });

            AuditionPvTestResult[] ordinaryResults =
                CreateTestResults(state, proof, proofPath, startedAtUtc);
            AuditionPvTestResult[] gateResults = WriteGateEvidenceArtifacts(
                state,
                proof,
                proofPath,
                proof.frameHashLedgerPath,
                evidence,
                captureCoreSha256,
                startedAtUtc);
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
                                AuditionPvStationBossDeathAftermathCapture.ShotId,
                            sourceRangeStartFrame =
                                S090EvidenceSourceRangeStartFrame,
                            sourceRangeEndFrame = S090EvidenceSourceRangeEndFrame,
                            selectStartFrame = S090EvidenceSelectStartFrame,
                            selectEndFrame = S090EvidenceSelectEndFrame,
                            runtimeWorkloadSealPath =
                                state.s090RuntimeWorkloadSealPath,
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
                    createdAtUtc: startedAtUtc,
                    gitSnapshot: CreateGitSnapshot(state),
                    engineSnapshot: CopyEngine(state.engine),
                    dependencyHashSnapshot: state.dependencyHashesAtStart);
            if (!string.Equals(
                    captureCoreSha256,
                    AuditionPvSixtySecondGateManifestValidator
                        .CaptureCoreSha256(manifest),
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "G08 Gate evidence changed its immutable capture-core identity.");
            }

            ValidateManifestInMemory(manifest, state.captureId);
            ValidateManifestMatchesRecordedState(state, manifest);
            ValidateManifestProofProvenance(manifest, proof);
            // Terminal commit record: no fallible writes may follow this call.
            AuditionPvCaptureManifestWriter.WriteNew(manifest);
        }

        private static void AnalyzeFrames(string frameDirectory, RuntimeProof proof)
        {
            const int sampleStride = SequencePixelSampleStride;
            long total = 0;
            long black = 0;
            long magenta = 0;
            int healthy = 0;
            double maximumMagenta = 0d;
            for (int frame = 0;
                frame < AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
                frame++)
            {
                Texture2D texture = LoadPng(
                    Path.Combine(
                        frameDirectory,
                        AuditionPvStationBossDeathAftermathCapture.FrameFileName(frame)),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
                try
                {
                    Color32[] pixels = texture.GetPixels32();
                    long frameSamples = 0;
                    long frameBlack = 0;
                    long frameMagenta = 0;
                    for (int y = 0; y < texture.height; y += sampleStride)
                    {
                        int row = y * texture.width;
                        for (int x = 0; x < texture.width; x += sampleStride)
                        {
                            Color32 color = pixels[row + x];
                            frameSamples++;
                            if (color.r <= 8 && color.g <= 8 && color.b <= 8)
                            {
                                frameBlack++;
                            }

                            if (color.r >= 200 && color.g <= 80 && color.b >= 200)
                            {
                                frameMagenta++;
                            }
                        }
                    }

                    double blackRatio = frameBlack / (double)frameSamples;
                    double magentaRatio = frameMagenta / (double)frameSamples;
                    if (blackRatio <= MaximumSequenceBlackRatio
                        && magentaRatio <= MaximumFrameMagentaRatio)
                    {
                        healthy++;
                    }

                    total += frameSamples;
                    black += frameBlack;
                    magenta += frameMagenta;
                    maximumMagenta = Math.Max(maximumMagenta, magentaRatio);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }

            proof.pixelSampleStride = sampleStride;
            proof.pixelSampleCount = total;
            proof.sequenceBlackRatio = black / (double)total;
            proof.sequenceMagentaRatio = magenta / (double)total;
            proof.maximumFrameMagentaRatio = maximumMagenta;
            proof.healthyFramePercent = healthy * 100d
                / AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
            proof.frameDeltaPixelSampleStride = FrameDeltaPixelSampleStride;
            proof.frameDeltaChangedRgbSumCutoff = FrameDeltaChangedRgbSumCutoff;
            proof.impactDeltaFromFrame = ImpactDeltaFromFrame;
            proof.impactDeltaToFrame = ImpactDeltaToFrame;
            proof.frameDeltaPixelSampleCount = MeasureFrameDelta(
                frameDirectory,
                ImpactDeltaFromFrame,
                ImpactDeltaToFrame,
                out proof.impactMeanAbsoluteRgb,
                out proof.impactChangedRatio);
            proof.aftermathDeltaFromFrame = AftermathDeltaFromFrame;
            proof.aftermathDeltaToFrame = AftermathDeltaToFrame;
            MeasureFrameDelta(frameDirectory, AftermathDeltaFromFrame, AftermathDeltaToFrame,
                out proof.aftermathEvolutionMeanAbsoluteRgb,
                out proof.aftermathEvolutionChangedRatio);
            proof.resultAppearanceFromFrame = ResultAppearanceFromFrame;
            proof.resultAppearanceToFrame = ResultAppearanceToFrame;
            MeasureFrameDelta(frameDirectory, ResultAppearanceFromFrame,
                ResultAppearanceToFrame,
                out proof.resultAppearanceMeanAbsoluteRgb,
                out proof.resultAppearanceChangedRatio);
            proof.resultEntranceFromFrame = ResultEntranceFromFrame;
            proof.resultEntranceToFrame = ResultEntranceToFrame;
            MeasureFrameDelta(frameDirectory, ResultEntranceFromFrame, ResultEntranceToFrame,
                out proof.resultEntranceMeanAbsoluteRgb,
                out proof.resultEntranceChangedRatio);
            MeasureResultSurface(frameDirectory, ResultSurfaceFrame, proof);
        }

        private static int MeasureFrameDelta(
            string frameDirectory,
            int firstFrame,
            int secondFrame,
            out double meanAbsoluteRgb,
            out double changedRatio)
        {
            Texture2D first = LoadPng(FramePath(frameDirectory, firstFrame),
                AuditionPvCaptureContract.Width, AuditionPvCaptureContract.Height);
            Texture2D second = LoadPng(FramePath(frameDirectory, secondFrame),
                AuditionPvCaptureContract.Width, AuditionPvCaptureContract.Height);
            try
            {
                Color32[] a = first.GetPixels32();
                Color32[] b = second.GetPixels32();
                int samples = 0;
                long changed = 0;
                double sum = 0d;
                for (int y = 0; y < first.height; y += FrameDeltaPixelSampleStride)
                {
                    int row = y * first.width;
                    for (int x = 0; x < first.width;
                        x += FrameDeltaPixelSampleStride)
                    {
                        int index = row + x;
                        int delta = Math.Abs(a[index].r - b[index].r)
                            + Math.Abs(a[index].g - b[index].g)
                            + Math.Abs(a[index].b - b[index].b);
                        sum += delta / 3d;
                        if (delta >= FrameDeltaChangedRgbSumCutoff)
                        {
                            changed++;
                        }

                        samples++;
                    }
                }

                meanAbsoluteRgb = sum / samples;
                changedRatio = changed / (double)samples;
                return samples;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        private static void MeasureResultSurface(
            string frameDirectory,
            int frame,
            RuntimeProof proof)
        {
            Texture2D texture = LoadPng(FramePath(frameDirectory, frame),
                AuditionPvCaptureContract.Width, AuditionPvCaptureContract.Height);
            try
            {
                Color32[] pixels = texture.GetPixels32();
                RectInt roi = ResultSurfaceRawBottomLeftRoi;
                proof.resultSurfaceFrame = frame;
                proof.resultSurfaceRoiX = roi.x;
                proof.resultSurfaceRoiY = roi.y;
                proof.resultSurfaceRoiWidth = roi.width;
                proof.resultSurfaceRoiHeight = roi.height;
                proof.resultSurfaceSampleStride = ResultSurfaceSampleStride;
                proof.resultBrightMinimumChannel = ResultBrightMinimumChannel;
                proof.resultNavyMaximumLuma = ResultNavyMaximumLuma;
                proof.resultBlueMinimumChannel = ResultBlueMinimumChannel;
                proof.resultBlueMinimumRedDelta = ResultBlueMinimumRedDelta;
                proof.resultBlueMinimumGreenDelta = ResultBlueMinimumGreenDelta;
                proof.resultSurfaceSampleCount = 0;
                proof.resultBrightSamples = 0;
                proof.resultNavySamples = 0;
                proof.resultBlueSamples = 0;
                for (int y = roi.yMin; y < roi.yMax;
                    y += ResultSurfaceSampleStride)
                {
                    int row = y * texture.width;
                    for (int x = roi.xMin; x < roi.xMax;
                        x += ResultSurfaceSampleStride)
                    {
                        Color32 color = pixels[row + x];
                        proof.resultSurfaceSampleCount++;
                        if (color.r >= ResultBrightMinimumChannel
                            && color.g >= ResultBrightMinimumChannel
                            && color.b >= ResultBrightMinimumChannel)
                        {
                            proof.resultBrightSamples++;
                        }

                        int luma = (54 * color.r + 183 * color.g + 19 * color.b)
                            >> 8;
                        if (luma <= ResultNavyMaximumLuma)
                        {
                            proof.resultNavySamples++;
                        }

                        if (color.b >= ResultBlueMinimumChannel
                            && color.b >= color.r + ResultBlueMinimumRedDelta
                            && color.b >= color.g + ResultBlueMinimumGreenDelta)
                        {
                            proof.resultBlueSamples++;
                        }
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void CopyBaselines(
            PersistedRunnerState state,
            string frameDirectory,
            RuntimeProof proof)
        {
            foreach (AuditionPvBaselineManifestEntry baseline in
                     AuditionPvStationBossDeathAftermathCapture
                         .CreateBaselineManifestEntries())
            {
                string source = FramePath(frameDirectory, baseline.sourceFrame);
                string destination = Path.Combine(
                    state.baselineDirectory,
                    baseline.fileName);
                CopyNew(source, destination);
                string sourceHash = AuditionPvSha256.FileHash(source);
                string destinationHash = AuditionPvSha256.FileHash(destination);
                if (!string.Equals(sourceHash, destinationHash, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G08 baseline is not a byte-exact event-frame copy: "
                        + baseline.id);
                }

                string expected = baseline.sourceFrame == SourceFrame(
                    AuditionPvStationBossDeathAftermathCapture.ImpactFrame)
                    ? proof.bl10Sha256
                    : baseline.sourceFrame == SourceFrame(
                        AuditionPvStationBossDeathAftermathCapture
                            .AftermathHeroFrame)
                        ? proof.bl11Sha256
                        : proof.bl12Sha256;
                if (!string.Equals(destinationHash, expected, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G08 baseline hash differs from its canonical source frame.");
                }
            }
        }

        internal static AuditionPvTestResult[] WriteGateEvidenceArtifacts(
            PersistedRunnerState state,
            RuntimeProof proof,
            string runtimeProofPath,
            string frameHashLedgerPath,
            string evidenceDirectory,
            string captureCoreSha256,
            DateTime startedAtUtc)
        {
            if (state == null || proof == null
                || !AuditionPvSha256.IsSha256(captureCoreSha256))
            {
                throw new InvalidDataException(
                    "G08 Gate evidence identity inputs are incomplete.");
            }

            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            string createdAtUtc = startedAtUtc.ToUniversalTime().ToString(
                "O",
                CultureInfo.InvariantCulture);
            var runtimePin = new AuditionPvPinnedArtifact
            {
                path = Path.GetFullPath(runtimeProofPath).Replace('\\', '/'),
                sha256 = AuditionPvSha256.FileHash(runtimeProofPath)
            };
            var ledgerPin = new AuditionPvPinnedArtifact
            {
                path = Path.GetFullPath(frameHashLedgerPath).Replace('\\', '/'),
                sha256 = AuditionPvSha256.FileHash(frameHashLedgerPath)
            };

            AuditionPvTestResult Passed(
                string name,
                string details,
                string artifactPath) => new()
            {
                suite = AuditionPvStationBossDeathAftermathCapture
                    .GateEvidenceTestSuite,
                name = name,
                status = "passed",
                durationMilliseconds = duration,
                details = details,
                artifactPath = Path.GetFullPath(artifactPath).Replace('\\', '/')
            };

            string authorshipPath = Path.Combine(
                evidenceDirectory,
                GateShotAuthorshipFileName);
            WriteJsonNew(authorshipPath, new AuditionPvShotAuthorshipArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator
                    .ShotAuthorshipSchema,
                sourceCaptureCoreSha256 = captureCoreSha256,
                captureId = state.captureId,
                sourceShotId = AuditionPvStationBossDeathAftermathCapture.ShotId,
                cameraId = AuditionPvStationBossDeathAftermathCapture.GateCameraId,
                gameplayState = AuditionPvStationBossDeathAftermathCapture
                    .GateGameplayState,
                timelineId = AuditionPvStationBossDeathAftermathCapture
                    .GateTimelineId,
                deterministicSeed = AuditionPvStationBossDeathAftermathCapture
                    .DeterministicRandomSeed,
                runtimeProof = runtimePin,
                tool = "G08GoldenRunner",
                toolVersion = string.IsNullOrWhiteSpace(
                    state.engine?.recorderPackageVersion)
                    ? "1"
                    : state.engine.recorderPackageVersion,
                createdAtUtc = createdAtUtc
            });
            string authorshipSha256 = AuditionPvSha256.FileHash(authorshipPath);
            var results = new List<AuditionPvTestResult>
            {
                Passed(
                    "shot-authorship/"
                        + AuditionPvStationBossDeathAftermathCapture.ShotId,
                    $"artifact-sha256={authorshipSha256}; capture-core-sha256={captureCoreSha256}; exact-camera-state-seed-timeline=true",
                    authorshipPath),
                Passed(
                    "shot-authorship-runtime/"
                        + AuditionPvStationBossDeathAftermathCapture.ShotId,
                    $"artifact-sha256={runtimePin.sha256}; capture-core-sha256={captureCoreSha256}; frame-ledger-sha256={ledgerPin.sha256}; exact-runtime=true",
                    runtimeProofPath)
            };

            string semanticDirectory = Path.Combine(
                evidenceDirectory,
                GateSemanticEvidenceFolderName);
            Directory.CreateDirectory(semanticDirectory);
            foreach (GateSemanticBeatSpec spec in CreateGateSemanticBeatSpecs(proof))
            {
                string artifactPath = Path.Combine(
                    semanticDirectory,
                    spec.beatId + ".json");
                WriteJsonNew(artifactPath, new GateSemanticBeatRuntimeArtifact
                {
                    schemaVersion =
                        "dimension-brawl.audition-pv.g08-semantic-beat-runtime.v1",
                    sourceCaptureCoreSha256 = captureCoreSha256,
                    captureId = state.captureId,
                    sourceShotId = AuditionPvStationBossDeathAftermathCapture.ShotId,
                    beatId = spec.beatId,
                    runtimeFactKey = spec.beatId,
                    sourceRangeStartFrame =
                        AuditionPvStationBossDeathAftermathCapture.FirstFrame,
                    sourceRangeEndFrame =
                        AuditionPvStationBossDeathAftermathCapture.LastFrame,
                    logicalFactStartFrame = spec.logicalStartFrame,
                    logicalFactEndFrame = spec.logicalEndFrame,
                    sourceFactStartFrame = SourceFrame(spec.logicalStartFrame),
                    sourceFactEndFrame = SourceFrame(spec.logicalEndFrame),
                    exactFacts = spec.exactFacts,
                    runtimeProof = runtimePin,
                    sourceFrameLedger = ledgerPin,
                    producer = "G08GoldenRunner",
                    createdAtUtc = createdAtUtc
                });
                string artifactSha256 = AuditionPvSha256.FileHash(artifactPath);
                results.Add(Passed(
                    "semantic-beat/" + spec.beatId,
                    $"artifact-sha256={artifactSha256}; semantic-fact={spec.beatId}; capture-core-sha256={captureCoreSha256}; frame-ledger-sha256={ledgerPin.sha256}; exact-runtime=true",
                    artifactPath));
            }

            string[] expectedBeatIds =
                AuditionPvStationBossDeathAftermathCapture.GateSemanticBeatIds();
            string[] actualBeatIds = results
                .Where(result => result.name.StartsWith(
                    "semantic-beat/",
                    StringComparison.Ordinal))
                .Select(result => result.name.Substring("semantic-beat/".Length))
                .ToArray();
            if (!actualBeatIds.SequenceEqual(
                    expectedBeatIds,
                    StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "G08 Gate semantic-beat artifacts are incomplete or reordered.");
            }

            return results.ToArray();
        }

        private static GateSemanticBeatSpec[] CreateGateSemanticBeatSpecs(
            RuntimeProof proof)
        {
            return new[]
            {
                new GateSemanticBeatSpec(
                    "boss-finisher",
                    AuditionPvStationBossDeathAftermathCapture
                        .S090SelectLogicalStartFrame,
                    AuditionPvStationBossDeathAftermathCapture
                        .AftermathHeroFrame,
                    $"projectile-impact-logical-frame={proof.projectileImpactFrame}",
                    $"finisher-camera-first-logical-frame={proof.firstFinisherCameraFrame}",
                    $"physical-projectile-instance={proof.projectileInstanceId}"),
                new GateSemanticBeatSpec(
                    "boss-collapse",
                    AuditionPvStationBossDeathAftermathCapture.ImpactFrame,
                    AuditionPvStationBossDeathAftermathCapture
                        .FinisherStabilityFrame,
                    $"boss-died-count={proof.bossDiedCount}",
                    $"death-motion-request-count={proof.deathMotionRequestCount}",
                    $"animator-dead={proof.animatorInDeathState}",
                    $"terminal-hold-logical-frame={AuditionPvStationBossDeathAftermathCapture.FinisherStabilityFrame}"),
                new GateSemanticBeatSpec(
                    "aftermath",
                    AuditionPvStationBossDeathAftermathCapture.AftermathHeroFrame,
                    AuditionPvStationBossDeathAftermathCapture.LogicalLastFrame,
                    $"aftermath-elapsed-seconds={proof.aftermathElapsedSeconds.ToString("F3", CultureInfo.InvariantCulture)}",
                    $"result-request-logical-frame={proof.firstResultSceneFrame}",
                    $"interactive-result-logical-frame={proof.firstInteractiveFrame}",
                    $"committed-presented-same-instance={proof.resultSummarySameInstance && proof.presentedSummarySameInstance}")
            };
        }

        internal static AuditionPvTestResult[] CreateTestResults(
            PersistedRunnerState state,
            RuntimeProof proof,
            string proofPath,
            DateTime startedAtUtc)
        {
            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            AuditionPvTestResult Passed(string suite, string name, string details,
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
                Passed("recorder", "raw-warmup-and-logical-remap",
                    "Recorder 5.1.6 QHD60 raw0..720 exact; raw0 evidence; raw1..720 -> source f0..f719; logical f0..f359 -> source f180..f539 with real 180/180 handles.",
                    proof.warmupEvidencePath),
                Passed("canonical-route", "corridor-to-station-product-handoff",
                    "Real public Corridor tutorial/product UI handoff with exact pending token and Station entry/terminal receipts.",
                    proofPath),
                Passed("physical-combat", "one-projectile-natural-terminal",
                    "Logical f1 one public TryFire; same authored projectile naturally impacts at f62 and produces one Died/BossTerminal.",
                    proofPath),
                Passed("aftermath", "unscaled-2.6s-terminal-bridge",
                    "Eight locks, scale-one observation, camera/VFX/audio/motion, f218 release/freeze, no early result.",
                    proofPath),
                Passed("result", "committed-same-instance-interactive",
                    "Canonical fact/commit/presented summary SameAs; interactive stable result at f246.",
                    proofPath),
                Passed("pixels", "qhd-health-deltas-and-result-surface",
                    "720 QHD source frames pass black/magenta health; mapped impact/aftermath/result deltas and result-surface color gates use logical+180 source frames.",
                    proof.frameHashLedgerPath),
                Passed("cleanup", "scene-global-input-event-restore",
                    "Recorder, manual clock, cadence, transition bootstrap/events, input leases, globals, and edit-mode scene state restored.",
                    proofPath)
            };
        }

        internal static void ValidateManifestInMemory(
            AuditionPvCaptureManifest manifest,
            string captureId)
        {
            AuditionPvCaptureManifestWriter.Validate(manifest);
            AuditionPvCaptureManifest roundTrip =
                JsonUtility.FromJson<AuditionPvCaptureManifest>(
                    JsonUtility.ToJson(manifest, true));
            AuditionPvCaptureManifestWriter.Validate(roundTrip);
            ValidateExactEngineProvenance(
                roundTrip.unityVersion,
                roundTrip.unityVersionWithRevision,
                roundTrip.recorderPackageVersion,
                roundTrip.urpPackageVersion,
                roundTrip.activeRenderPipelineAssetPath);
            if (!DateTime.TryParse(
                    roundTrip.createdAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime createdAt)
                || !string.Equals(
                    roundTrip.createdAtUtc,
                    createdAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                || !roundTrip.captureId.StartsWith(
                    createdAt.ToUniversalTime().ToString(
                        "yyyyMMdd't'HHmmss'z'_",
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                || roundTrip.gitWorktreeDirty
                || roundTrip.gitCommitSha == null
                || roundTrip.gitCommitSha.Length != 40
                || roundTrip.gitCommitSha.Any(character =>
                    !(character >= '0' && character <= '9'
                        || character >= 'a' && character <= 'f'))
                || string.IsNullOrWhiteSpace(roundTrip.gitBranch)
                || string.Equals(
                    roundTrip.gitBranch,
                    "HEAD",
                    StringComparison.OrdinalIgnoreCase)
                || !AuditionPvSha256.IsSha256(
                    roundTrip.worktreeDirtyHashSha256)
                || !string.Equals(
                    roundTrip.worktreeDirtyHashAlgorithm,
                    AuditionPvGitSnapshot.DirtyHashAlgorithm,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 manifest clean HEAD/timestamp provenance is not exact.");
            }

            AuditionPvShotManifestEntry expectedShot =
                AuditionPvStationBossDeathAftermathCapture
                    .CreateShotManifestEntry();
            if (!string.Equals(roundTrip.captureId, captureId, StringComparison.Ordinal)
                || roundTrip.shots == null
                || roundTrip.shots.Length != 1
                || !ShotEquals(roundTrip.shots[0], expectedShot))
            {
                throw new InvalidOperationException(
                    "G08 manifest shot record is not exact.");
            }

            AuditionPvBaselineManifestEntry[] expectedBaselines =
                AuditionPvStationBossDeathAftermathCapture
                    .CreateBaselineManifestEntries();
            if (roundTrip.baselines == null
                || roundTrip.baselines.Length != expectedBaselines.Length)
            {
                throw new InvalidOperationException(
                    "G08 manifest baseline count is not exact.");
            }

            for (int index = 0; index < expectedBaselines.Length; index++)
            {
                if (!BaselineEquals(
                        roundTrip.baselines[index],
                        expectedBaselines[index]))
                {
                    throw new InvalidOperationException(
                        "G08 manifest baseline record is not exact: "
                        + expectedBaselines[index].id);
                }
            }

            string output = roundTrip.outputDirectory;
            string proofPath = Path.Combine(
                output,
                EvidenceFolderName,
                RuntimeProofFileName).Replace('\\', '/');
            string warmupPath = Path.Combine(
                output,
                EvidenceFolderName,
                WarmupEvidenceFileName).Replace('\\', '/');
            string ledgerPath = Path.Combine(
                output,
                EvidenceFolderName,
                FrameHashLedgerFileName).Replace('\\', '/');
            (string suite, string name, string details, string artifact)[] expectedTests =
            {
                ("recorder", "raw-warmup-and-logical-remap",
                    "Recorder 5.1.6 QHD60 raw0..720 exact; raw0 evidence; raw1..720 -> source f0..f719; logical f0..f359 -> source f180..f539 with real 180/180 handles.",
                    warmupPath),
                ("canonical-route", "corridor-to-station-product-handoff",
                    "Real public Corridor tutorial/product UI handoff with exact pending token and Station entry/terminal receipts.",
                    proofPath),
                ("physical-combat", "one-projectile-natural-terminal",
                    "Logical f1 one public TryFire; same authored projectile naturally impacts at f62 and produces one Died/BossTerminal.",
                    proofPath),
                ("aftermath", "unscaled-2.6s-terminal-bridge",
                    "Eight locks, scale-one observation, camera/VFX/audio/motion, f218 release/freeze, no early result.",
                    proofPath),
                ("result", "committed-same-instance-interactive",
                    "Canonical fact/commit/presented summary SameAs; interactive stable result at f246.",
                    proofPath),
                ("pixels", "qhd-health-deltas-and-result-surface",
                    "720 QHD source frames pass black/magenta health; mapped impact/aftermath/result deltas and result-surface color gates use logical+180 source frames.",
                    ledgerPath),
                ("cleanup", "scene-global-input-event-restore",
                    "Recorder, manual clock, cadence, transition bootstrap/events, input leases, globals, and edit-mode scene state restored.",
                    proofPath)
            };
            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator
                    .CaptureCoreSha256(roundTrip);
            string frameLedgerSha256 = File.Exists(ledgerPath)
                ? AuditionPvSha256.FileHash(ledgerPath)
                : string.Empty;
            string authorshipPath = Path.Combine(
                output,
                EvidenceFolderName,
                GateShotAuthorshipFileName).Replace('\\', '/');
            string semanticDirectory = Path.Combine(
                output,
                EvidenceFolderName,
                GateSemanticEvidenceFolderName);
            var expectedGateTests = new List<(
                string name,
                string artifact,
                string details)>
            {
                (
                    "shot-authorship/"
                        + AuditionPvStationBossDeathAftermathCapture.ShotId,
                    authorshipPath,
                    $"artifact-sha256={ArtifactHash(authorshipPath)}; capture-core-sha256={captureCoreSha256}; exact-camera-state-seed-timeline=true"),
                (
                    "shot-authorship-runtime/"
                        + AuditionPvStationBossDeathAftermathCapture.ShotId,
                    proofPath,
                    $"artifact-sha256={ArtifactHash(proofPath)}; capture-core-sha256={captureCoreSha256}; frame-ledger-sha256={frameLedgerSha256}; exact-runtime=true")
            };
            foreach (string beatId in
                     AuditionPvStationBossDeathAftermathCapture.GateSemanticBeatIds())
            {
                string artifactPath = Path.Combine(
                    semanticDirectory,
                    beatId + ".json").Replace('\\', '/');
                expectedGateTests.Add((
                    "semantic-beat/" + beatId,
                    artifactPath,
                    $"artifact-sha256={ArtifactHash(artifactPath)}; semantic-fact={beatId}; capture-core-sha256={captureCoreSha256}; frame-ledger-sha256={frameLedgerSha256}; exact-runtime=true"));
            }

            int fixedResultCount = expectedTests.Length + expectedGateTests.Count;
            AuditionPvTestResult[] generatedEvidenceResults =
                GeneratedSixtySecondEvidenceResults(roundTrip);
            if (roundTrip.testResults == null
                || roundTrip.testResults.Length
                    != fixedResultCount + generatedEvidenceResults.Length
                || generatedEvidenceResults.Length != 0
                    && generatedEvidenceResults.Length != 7)
            {
                throw new InvalidOperationException(
                    "G08 manifest must contain the exact ordinary/Gate records and either zero or one complete generated 60-second evidence set.");
            }

            for (int index = 0; index < expectedTests.Length; index++)
            {
                AuditionPvTestResult actual = roundTrip.testResults[index];
                var expected = expectedTests[index];
                if (actual == null
                    || !string.Equals(actual.suite, expected.suite, StringComparison.Ordinal)
                    || !string.Equals(actual.name, expected.name, StringComparison.Ordinal)
                    || !string.Equals(actual.status, "passed", StringComparison.Ordinal)
                    || actual.durationMilliseconds < 0
                    || !string.Equals(actual.details, expected.details, StringComparison.Ordinal)
                    || !PathsEqual(actual.artifactPath, expected.artifact))
                {
                    throw new InvalidOperationException(
                        "G08 manifest test-result record is not exact: "
                        + expected.suite + "/" + expected.name);
                }
            }

            for (int index = 0; index < expectedGateTests.Count; index++)
            {
                AuditionPvTestResult actual =
                    roundTrip.testResults[expectedTests.Length + index];
                var expected = expectedGateTests[index];
                if (actual == null
                    || !string.Equals(
                        actual.suite,
                        AuditionPvStationBossDeathAftermathCapture
                            .GateEvidenceTestSuite,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        actual.name,
                        expected.name,
                        StringComparison.Ordinal)
                    || !string.Equals(actual.status, "passed", StringComparison.Ordinal)
                    || actual.durationMilliseconds < 0
                    || !string.Equals(
                        actual.details,
                        expected.details,
                        StringComparison.Ordinal)
                    || !PathsEqual(actual.artifactPath, expected.artifact))
                {
                    throw new InvalidOperationException(
                        "G08 Gate test-result record is not exact: "
                        + expected.name);
                }
            }

            ValidateGeneratedSixtySecondEvidenceResults(
                roundTrip,
                generatedEvidenceResults,
                captureCoreSha256,
                S090EvidenceSourceRangeStartFrame,
                S090EvidenceSourceRangeEndFrame);

            AuditionPvDependencyHash[] dependencies = roundTrip.dependencyHashes
                ?? Array.Empty<AuditionPvDependencyHash>();
            if (dependencies.Length == 0
                || dependencies.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.path)
                    || !value.exists
                    || value.byteLength < 0
                    || !AuditionPvSha256.IsSha256(value.sha256))
                || dependencies.Select(value => value.path).Distinct(
                    StringComparer.OrdinalIgnoreCase).Count() != dependencies.Length)
            {
                throw new InvalidOperationException(
                    "G08 manifest dependency snapshot is incomplete or malformed.");
            }

            var dependencyPaths = new HashSet<string>(
                dependencies.Select(value => value.path),
                StringComparer.OrdinalIgnoreCase);
            var required = new HashSet<string>(
                AuditionPvCaptureContract.CoreDependencyPaths,
                StringComparer.OrdinalIgnoreCase)
            {
                RunnerScriptPath,
                RunnerScriptPath + ".meta",
                RunnerTestPath,
                RunnerTestPath + ".meta",
                ReadmePath,
                ReadmePath + ".meta"
            };
            foreach (string path in
                     AuditionPvStationBossDeathAftermathCapture
                         .ExplicitProductDependencyPaths())
            {
                required.Add(path);
                if (path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    required.Add(path + ".meta");
                }
            }

            if (required.Any(path => !dependencyPaths.Contains(path))
                || !dependencyPaths.Contains(roundTrip.activeRenderPipelineAssetPath)
                || !dependencies.Any(value => value.path.StartsWith(
                    "Packages/com.unity.render-pipelines.universal/",
                    StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "G08 manifest dependency snapshot lacks the exact direct/core/meta/URP closure.");
            }
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
                        AuditionPvStationBossDeathAftermathCapture
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
                        "G08 generated evidence test is missing or duplicated: "
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
                        && result.details.Contains("source-shot=g08", StringComparison.Ordinal)
                        && result.details.Contains(rangeToken, StringComparison.Ordinal);
                }

                if (!valid)
                {
                    throw new InvalidOperationException(
                        "G08 generated evidence test is unpinned or range-mismatched: "
                        + expectedName);
                }
            }
        }

        private static bool ShotEquals(
            AuditionPvShotManifestEntry actual,
            AuditionPvShotManifestEntry expected)
        {
            return actual != null
                && expected != null
                && string.Equals(actual.id, expected.id, StringComparison.Ordinal)
                && string.Equals(actual.scenePath, expected.scenePath, StringComparison.Ordinal)
                && actual.startFrame == expected.startFrame
                && actual.endFrame == expected.endFrame
                && actual.expectedFrameCount == expected.expectedFrameCount
                && string.Equals(actual.hudMode, expected.hudMode, StringComparison.Ordinal)
                && string.Equals(actual.notes, expected.notes, StringComparison.Ordinal);
        }

        private static bool BaselineEquals(
            AuditionPvBaselineManifestEntry actual,
            AuditionPvBaselineManifestEntry expected)
        {
            return actual != null
                && expected != null
                && string.Equals(actual.id, expected.id, StringComparison.Ordinal)
                && string.Equals(actual.shotId, expected.shotId, StringComparison.Ordinal)
                && actual.sourceFrame == expected.sourceFrame
                && string.Equals(actual.fileName, expected.fileName, StringComparison.Ordinal)
                && string.Equals(actual.hudMode, expected.hudMode, StringComparison.Ordinal)
                && string.Equals(actual.status, expected.status, StringComparison.Ordinal);
        }

        private static void TryWriteFailureArtifact(
            string outputDirectory,
            string phase,
            Exception exception,
            RuntimeProof proof,
            PersistedRunnerState state)
        {
            if (state == null
                || string.IsNullOrWhiteSpace(outputDirectory)
                || !Directory.Exists(outputDirectory))
            {
                return;
            }

            try
            {
                WriteFailureArtifactForRoot(
                    outputDirectory,
                    phase,
                    exception,
                    proof,
                    state,
                    state.outputRoot);
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
            Action<string> deleteFile = null)
        {
            WriteFailureArtifactForRoot(
                outputDirectory,
                phase,
                exception,
                proof,
                state,
                authorizedRoot,
                VisualCompositionAcceptanceLocked,
                deleteFile);
        }

        internal static void WriteFailureArtifactForRoot(
            string outputDirectory,
            string phase,
            Exception exception,
            RuntimeProof proof,
            PersistedRunnerState state,
            string authorizedRoot,
            bool visualCompositionAcceptanceLocked,
            Action<string> deleteFile)
        {
            ValidatePersistedStateLayoutForRoot(state, authorizedRoot);
            ValidateCanonicalCaptureLocationForRoot(
                outputDirectory,
                state.captureId,
                authorizedRoot);
            if (!PathsEqual(outputDirectory, state.outputDirectory))
            {
                throw new InvalidDataException(
                    "G08 failure output differs from canonical runner state.");
            }

            if (IsValidCommittedManifestAt(
                outputDirectory,
                state.captureId,
                authorizedRoot,
                state))
            {
                return;
            }

            string cleanupFailure = DeleteUncommittedSuccessArtifactsForRoot(
                outputDirectory,
                state.captureId,
                authorizedRoot,
                deleteFile);
            string path = Path.Combine(outputDirectory, FailureFileName);
            if (File.Exists(path))
            {
                return;
            }

            WriteJsonNew(path, new FailureArtifact
            {
                schema = FailureSchema,
                createdAtUtc = DateTime.UtcNow.ToString("O"),
                phase = phase ?? string.Empty,
                exception = exception?.ToString() ?? string.Empty,
                captureId = state.captureId,
                outputDirectory = outputDirectory.Replace('\\', '/'),
                startGitCommitSha = state.gitCommitSha,
                startGitBranch = state.gitBranch,
                startGitDirty = state.gitWorktreeDirty,
                startGitDirtyHashSha256 = state.gitDirtyHashSha256,
                retainedArtifacts =
                    "Failure-only: raw/logical frames, runner state, and runtime/pixel/composition telemetry may remain; manifest, baselines, success proof, and canonical ledger are absent.",
                pixelCalibrationLocked = PixelCalibrationLocked,
                calibrationRequired = exception is G08PixelCalibrationRequiredException,
                visualCompositionAcceptanceLocked =
                    visualCompositionAcceptanceLocked,
                visualCompositionAcceptanceRequired =
                    exception is G08VisualCompositionAcceptanceRequiredException,
                successArtifactCleanupFailure = cleanupFailure,
                runtime = proof
            });
        }

        internal static string DeleteUncommittedSuccessArtifactsForRoot(
            string outputDirectory,
            string captureId,
            string authorizedRoot,
            Action<string> deleteFile = null)
        {
            try
            {
                ValidateCanonicalCaptureLocationForRoot(
                    outputDirectory,
                    captureId,
                    authorizedRoot);
            }
            catch (Exception exception)
            {
                return "Refused G08 success-artifact cleanup: " + exception;
            }

            string[] paths = new[]
            {
                Path.Combine(outputDirectory, AuditionPvCaptureContract.ManifestFileName),
                Path.Combine(outputDirectory, EvidenceFolderName, RuntimeProofFileName),
                Path.Combine(outputDirectory, EvidenceFolderName, FrameHashLedgerFileName),
                Path.Combine(
                    outputDirectory,
                    EvidenceFolderName,
                    GateShotAuthorshipFileName),
                Path.Combine(outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName,
                    AuditionPvStationBossDeathAftermathCapture.Bl10FileName),
                Path.Combine(outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName,
                    AuditionPvStationBossDeathAftermathCapture.Bl11FileName),
                Path.Combine(outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName,
                    AuditionPvStationBossDeathAftermathCapture.Bl12FileName)
            }
            .Concat(
                AuditionPvStationBossDeathAftermathCapture
                    .GateSemanticBeatIds()
                    .Select(beatId => Path.Combine(
                        outputDirectory,
                        EvidenceFolderName,
                        GateSemanticEvidenceFolderName,
                        beatId + ".json")))
            .ToArray();
            Exception failure = null;
            foreach (string path in paths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        (deleteFile ?? File.Delete)(path);
                    }
                }
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                }
            }

            return failure?.ToString() ?? string.Empty;
        }

        internal static bool IsValidCommittedManifestAt(
            string outputDirectory,
            string captureId,
            string authorizedRoot,
            PersistedRunnerState expectedState = null)
        {
            return IsValidCommittedManifestAtCore(
                outputDirectory,
                captureId,
                authorizedRoot,
                expectedState,
                requireLockedPixelCalibration: true);
        }

        internal static bool IsValidCommittedManifestAtForTests(
            string outputDirectory,
            string captureId,
            string authorizedRoot,
            PersistedRunnerState expectedState = null)
        {
            return IsValidCommittedManifestAtCore(
                outputDirectory,
                captureId,
                authorizedRoot,
                expectedState,
                requireLockedPixelCalibration: false);
        }

        private static bool IsValidCommittedManifestAtCore(
            string outputDirectory,
            string captureId,
            string authorizedRoot,
            PersistedRunnerState expectedState,
            bool requireLockedPixelCalibration)
        {
            try
            {
                ValidateCanonicalCaptureLocationForRoot(
                    outputDirectory,
                    captureId,
                    authorizedRoot);
                string manifestPath = Path.Combine(
                    outputDirectory,
                    AuditionPvCaptureContract.ManifestFileName);
                if (!File.Exists(manifestPath)
                    || File.Exists(Path.Combine(outputDirectory, FailureFileName)))
                {
                    return false;
                }

                AuditionPvCaptureManifest manifest =
                    JsonUtility.FromJson<AuditionPvCaptureManifest>(
                        File.ReadAllText(manifestPath));
                ValidateManifestInMemory(manifest, captureId);
                if (!PathsEqual(manifest.outputDirectory, outputDirectory)
                    || !PathsEqual(manifest.outputRoot, authorizedRoot))
                {
                    return false;
                }

                if (expectedState != null)
                {
                    ValidatePersistedStateLayoutForRoot(
                        expectedState,
                        authorizedRoot);
                    ValidateManifestMatchesRecordedState(expectedState, manifest);
                }

                string frames = Path.Combine(outputDirectory, "frames", "g08");
                ValidateLogicalFrameSequence(frames);
                for (int frame = 0;
                    frame < AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
                    frame++)
                {
                    ValidatePngFile(
                        FramePath(frames, frame),
                        AuditionPvCaptureContract.Width,
                        AuditionPvCaptureContract.Height);
                }

                string evidence = Path.Combine(outputDirectory, EvidenceFolderName);
                string proofPath = Path.Combine(evidence, RuntimeProofFileName);
                string ledgerPath = Path.Combine(evidence, FrameHashLedgerFileName);
                string warmupPath = Path.Combine(evidence, WarmupEvidenceFileName);
                RuntimeProofArtifact artifact = JsonUtility.FromJson<RuntimeProofArtifact>(
                    File.ReadAllText(proofPath));
                if (artifact == null
                    || !string.Equals(artifact.schema, RuntimeProofSchema, StringComparison.Ordinal)
                    || !string.Equals(artifact.captureId, captureId, StringComparison.Ordinal)
                    || !string.Equals(
                        artifact.sourceCaptureCoreSha256,
                        AuditionPvSixtySecondGateManifestValidator
                            .CaptureCoreSha256(manifest),
                        StringComparison.Ordinal)
                    || artifact.sourceRangeStartFrame
                        != AuditionPvStationBossDeathAftermathCapture.FirstFrame
                    || artifact.sourceRangeEndFrame
                        != AuditionPvStationBossDeathAftermathCapture.LastFrame
                    || artifact.selectStartFrame
                        != AuditionPvStationBossDeathAftermathCapture.S090SelectStartFrame
                    || artifact.selectEndFrame
                        != AuditionPvStationBossDeathAftermathCapture.S090SelectEndFrame
                    || artifact.sourceFrameLedger == null
                    || !PathsEqual(
                        artifact.sourceFrameLedger.path,
                        ledgerPath)
                    || !string.Equals(
                        artifact.sourceFrameLedger.sha256,
                        ArtifactHash(ledgerPath),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        artifact.mapping,
                        RuntimeMappingDescription,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        artifact.gameplay,
                        RuntimeGameplayDescription,
                        StringComparison.Ordinal)
                    || artifact.runtime == null)
                {
                    return false;
                }

                if (requireLockedPixelCalibration)
                {
                    ValidateRuntimeProof(artifact.runtime);
                }
                else
                {
                    ValidateRuntimeProofBeforePixelCalibration(artifact.runtime);
                }
                ValidateManifestProofProvenance(manifest, artifact.runtime);
                if (!PathsEqual(artifact.runtime.frameHashLedgerPath, ledgerPath)
                    || !PathsEqual(artifact.runtime.warmupEvidencePath, warmupPath)
                    || artifact.runtime.dependencyHashCount
                        != manifest.dependencyHashes.Length)
                {
                    return false;
                }

                ValidateDecodablePngFile(
                    warmupPath,
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
                ValidateFrameHashLedger(
                    frames,
                    ledgerPath,
                    artifact.runtime.frameHashLedgerSha256);
                if (!string.Equals(
                        AuditionPvSha256.FileHash(warmupPath),
                        artifact.runtime.warmupEvidenceSha256,
                        StringComparison.Ordinal))
                {
                    return false;
                }

                foreach (AuditionPvBaselineManifestEntry baseline in
                         AuditionPvStationBossDeathAftermathCapture
                             .CreateBaselineManifestEntries())
                {
                    string baselinePath = Path.Combine(
                        outputDirectory,
                        AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName,
                        baseline.fileName);
                    ValidatePngFile(baselinePath,
                        AuditionPvCaptureContract.Width,
                        AuditionPvCaptureContract.Height);
                    if (!string.Equals(
                        AuditionPvSha256.FileHash(baselinePath),
                        FrameHash(frames, baseline.sourceFrame),
                        StringComparison.Ordinal))
                    {
                        return false;
                    }
                }

                if (!string.Equals(
                        artifact.runtime.bl10Sha256,
                        FrameHash(
                            frames,
                            SourceFrame(
                                AuditionPvStationBossDeathAftermathCapture
                                    .ImpactFrame)),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        artifact.runtime.bl11Sha256,
                        FrameHash(
                            frames,
                            SourceFrame(
                                AuditionPvStationBossDeathAftermathCapture
                                    .AftermathHeroFrame)),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        artifact.runtime.bl12Sha256,
                        FrameHash(
                            frames,
                            SourceFrame(
                                AuditionPvStationBossDeathAftermathCapture
                                    .InteractiveResultFrame)),
                        StringComparison.Ordinal))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void ValidateSessionRecoveryLocationForRoot(
            string statePath,
            string outputDirectory,
            string captureId,
            string authorizedRoot)
        {
            ValidateCanonicalCaptureLocationForRoot(
                outputDirectory,
                captureId,
                authorizedRoot);
            string expectedState = Path.Combine(outputDirectory, StateFileName);
            if (!PathsEqual(statePath, expectedState))
            {
                throw new InvalidDataException(
                    "G08 SessionState path is not the canonical capture state path.");
            }
        }

        internal static void ValidatePersistedStateLocationForRoot(
            string statePath,
            PersistedRunnerState state,
            string authorizedRoot)
        {
            ValidatePersistedStateLayoutForRoot(state, authorizedRoot);
            if (!PathsEqual(
                    statePath,
                    Path.Combine(state.outputDirectory, StateFileName)))
            {
                throw new InvalidDataException(
                    "G08 runner state path is not canonical.");
            }
        }

        internal static void ValidatePersistedStateLayoutForRoot(
            PersistedRunnerState state,
            string authorizedRoot)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            ValidateCanonicalCaptureLocationForRoot(
                state.outputDirectory,
                state.captureId,
                authorizedRoot);
            string expectedBaselines = Path.Combine(
                state.outputDirectory,
                AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName);
            if (!PathsEqual(state.outputRoot, authorizedRoot)
                || !PathsEqual(state.baselineDirectory, expectedBaselines))
            {
                throw new InvalidDataException(
                    "G08 persisted state paths are outside the exact canonical capture layout.");
            }
        }

        internal static void ValidateSessionStateAuthority(
            string sessionOutputDirectory,
            string sessionCaptureId,
            bool sessionBatchMode,
            PersistedRunnerState state)
        {
            if (state == null
                || !PathsEqual(sessionOutputDirectory, state.outputDirectory)
                || !string.Equals(sessionCaptureId, state.captureId, StringComparison.Ordinal)
                || state.batchMode != sessionBatchMode)
            {
                throw new InvalidDataException(
                    "G08 mutable runner state differs from authoritative SessionState identity.");
            }
        }

        internal static Exception RecoverTerminalPersistenceFaultForRoot(
            string outputDirectory,
            string captureId,
            string authorizedRoot,
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
                    authorizedRoot);
                var recoveryState = new PersistedRunnerState
                {
                    schema = RunnerSchema,
                    phase = RunnerPhase.FailedInPlayMode.ToString(),
                    captureId = captureId,
                    outputRoot = authorizedRoot,
                    outputDirectory = outputDirectory,
                    baselineDirectory = Path.Combine(
                        outputDirectory,
                        AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName)
                };
                WriteFailureArtifactForRoot(
                    outputDirectory,
                    "playmode-terminal-persistence-resume",
                    new InvalidOperationException(
                        "G08 terminal state persistence failed; stale Recording state was not resumed.\n"
                        + (terminalFault ?? string.Empty)),
                    null,
                    recoveryState,
                    authorizedRoot);
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
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                }

                try
                {
                    requestExit?.Invoke(1);
                }
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                }
            }

            return failure;
        }

        internal static SessionRecoveryDecision DetermineSessionRecoveryDecision(
            bool editorPlaying,
            bool committedManifestIsValid,
            string terminalFault)
        {
            if (!editorPlaying && committedManifestIsValid)
            {
                return SessionRecoveryDecision.CommittedManifest;
            }

            return string.IsNullOrWhiteSpace(terminalFault)
                ? SessionRecoveryDecision.Continue
                : SessionRecoveryDecision.TerminalFault;
        }

        internal static void ValidateCanonicalCaptureLocationForRoot(
            string outputDirectory,
            string captureId,
            string authorizedRoot)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory)
                || string.IsNullOrWhiteSpace(captureId)
                || string.IsNullOrWhiteSpace(authorizedRoot))
            {
                throw new InvalidDataException(
                    "G08 canonical capture location tokens are incomplete.");
            }

            AuditionPvOutputPaths.ValidateOutputId(captureId);
            string root = Path.GetFullPath(authorizedRoot).TrimEnd('\\', '/');
            string expected = AuditionPvOutputPaths.ResolveOutputDirectory(
                root,
                captureId);
            string actual = Path.GetFullPath(outputDirectory).TrimEnd('\\', '/');
            if (!PathsEqual(expected, actual)
                || !PathsEqual(Path.GetDirectoryName(actual), root))
            {
                throw new InvalidDataException(
                    "G08 output is not the authorized direct-child capture directory.");
            }
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
            EditorApplication.delayCall -= ResumeOwnedSession;
            EditorApplication.update -= ResumeOwnedSessionWatchdog;
            resumeScheduled = false;
            resumeWatchdogRegistered = false;
            SessionState.EraseBool(SessionActiveKey);
            SessionState.EraseString(SessionStatePathKey);
            SessionState.EraseString(SessionOwnerKey);
            SessionState.EraseBool(SessionBatchKey);
            SessionState.EraseString(SessionOutputDirectoryKey);
            SessionState.EraseString(SessionCaptureIdKey);
            SessionState.EraseString(SessionTerminalFaultKey);
        }

        private static void SaveState(string path, PersistedRunnerState state)
        {
            if (state == null || !string.Equals(state.schema, RunnerSchema, StringComparison.Ordinal))
            {
                throw new InvalidDataException("G08 runner state schema is invalid.");
            }

            ValidatePersistedStateLocationForRoot(
                path,
                state,
                AuditionPvCaptureContract.OutputRoot);

            Directory.CreateDirectory(state.outputDirectory);
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(
                    temporary,
                    JsonUtility.ToJson(state, true) + Environment.NewLine,
                    new UTF8Encoding(false));
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

        private static PersistedRunnerState LoadState(string path)
        {
            PersistedRunnerState state = JsonUtility.FromJson<PersistedRunnerState>(
                File.ReadAllText(path));
            if (state == null || !string.Equals(state.schema, RunnerSchema, StringComparison.Ordinal))
            {
                throw new InvalidDataException("G08 runner state is missing or corrupt.");
            }

            ValidatePersistedStateLocationForRoot(
                path,
                state,
                AuditionPvCaptureContract.OutputRoot);
            ValidateRequiredEngineProvenance(state.engine);
            return state;
        }

        private static RunnerPhase ParsePhase(string value)
        {
            if (!Enum.TryParse(value, out RunnerPhase phase))
            {
                throw new InvalidDataException("Unknown G08 runner phase: " + value);
            }

            return phase;
        }

        private static void ValidateExactNamedSequence(
            string directory,
            int expectedCount,
            Func<int, string> fileName)
        {
            string root = RequireDirectory(directory);
            string[] actual = Directory.GetFiles(root, "*.png", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] expected = Enumerable.Range(0, expectedCount)
                .Select(fileName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    $"G08 frame sequence mismatch: expected={expectedCount}, actual={actual.Length}.");
            }
        }

        private static Texture2D LoadPng(
            string path,
            int expectedWidth,
            int expectedHeight)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true)
            {
                name = "G08Validation_" + Path.GetFileNameWithoutExtension(path)
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
                    $"Unity could not decode exact {expectedWidth}x{expectedHeight} G08 PNG: {path}");
            }

            return texture;
        }

        private static string FramePath(string frameDirectory, int frame)
        {
            return Path.Combine(
                frameDirectory,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName(frame));
        }

        private static string FrameHash(string frameDirectory, int frame)
        {
            return AuditionPvSha256.FileHash(FramePath(frameDirectory, frame));
        }

        private static string ArtifactHash(string path)
        {
            return File.Exists(path)
                ? AuditionPvSha256.FileHash(path)
                : string.Empty;
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset)
        {
            return bytes[offset] << 24
                | bytes[offset + 1] << 16
                | bytes[offset + 2] << 8
                | bytes[offset + 3];
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
                    $"G08 move requires one new destination. source={source}, destination={destination}");
            }

            File.Move(source, destination);
        }

        private static void CopyNew(string source, string destination)
        {
            if (!File.Exists(source) || File.Exists(destination))
            {
                throw new IOException(
                    $"G08 copy requires one new destination. source={source}, destination={destination}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)
                ?? throw new InvalidOperationException("G08 destination has no parent."));
            File.Copy(source, destination, overwrite: false);
        }

        private static void WriteTextNew(string path, string value)
        {
            if (File.Exists(path))
            {
                throw new IOException("G08 immutable artifact already exists: " + path);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("G08 artifact has no parent."));
            string temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(
                    temporary,
                    value ?? string.Empty,
                    new UTF8Encoding(false));
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

        private static void WriteJsonNew<T>(string path, T value)
        {
            WriteTextNew(path, JsonUtility.ToJson(value, true) + Environment.NewLine);
        }

        private static bool PathsEqual(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
            {
                return false;
            }

            return string.Equals(
                Path.GetFullPath(first).TrimEnd('\\', '/'),
                Path.GetFullPath(second).TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string ProjectAbsolutePath(string projectRelativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Could not resolve G08 project root.");
            return Path.GetFullPath(Path.Combine(root, projectRelativePath));
        }

        private static AuditionPvGitSnapshot CreateGitSnapshot(PersistedRunnerState state)
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

        internal static string ComputeCaptureStartProvenanceSha256(
            PersistedRunnerState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return ComputeCaptureStartProvenanceSha256(
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
            AuditionPvEngineSnapshot engine,
            AuditionPvDependencyHash[] dependencies)
        {
            ValidateRequiredEngineProvenance(engine);

            var canonical = new StringBuilder();
            void Append(string value)
            {
                string normalized = value ?? string.Empty;
                canonical.Append(normalized.Length.ToString(CultureInfo.InvariantCulture));
                canonical.Append(':');
                canonical.Append(normalized);
                canonical.Append('\n');
            }

            Append("dimension-brawl.audition-pv.g08-start-provenance.v1");
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

        internal static void ValidateManifestMatchesRecordedState(
            PersistedRunnerState state,
            AuditionPvCaptureManifest manifest)
        {
            if (state == null || manifest == null)
            {
                throw new ArgumentNullException(
                    state == null ? nameof(state) : nameof(manifest));
            }

            if (!DateTime.TryParse(
                    state.startedAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime startedAt)
                || !string.Equals(manifest.captureId, state.captureId, StringComparison.Ordinal)
                || !PathsEqual(manifest.outputRoot, state.outputRoot)
                || !PathsEqual(manifest.outputDirectory, state.outputDirectory)
                || !string.Equals(
                    manifest.createdAtUtc,
                    startedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                || !string.Equals(manifest.gitCommitSha, state.gitCommitSha, StringComparison.Ordinal)
                || !string.Equals(manifest.gitBranch, state.gitBranch, StringComparison.Ordinal)
                || manifest.gitWorktreeDirty != state.gitWorktreeDirty
                || !string.Equals(
                    manifest.worktreeDirtyHashSha256,
                    state.gitDirtyHashSha256,
                    StringComparison.Ordinal)
                || !string.Equals(
                    manifest.worktreeDirtyHashAlgorithm,
                    AuditionPvGitSnapshot.DirtyHashAlgorithm,
                    StringComparison.Ordinal)
                || state.engine == null
                || !string.Equals(manifest.unityVersion, state.engine.unityVersion, StringComparison.Ordinal)
                || !string.Equals(
                    manifest.unityVersionWithRevision,
                    state.engine.unityVersionWithRevision,
                    StringComparison.Ordinal)
                || !string.Equals(
                    manifest.recorderPackageVersion,
                    state.engine.recorderPackageVersion,
                    StringComparison.Ordinal)
                || !string.Equals(
                    manifest.urpPackageVersion,
                    state.engine.urpPackageVersion,
                    StringComparison.Ordinal)
                || !string.Equals(
                    manifest.activeRenderPipelineAssetPath,
                    state.engine.activeRenderPipelineAssetPath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 committed manifest provenance differs from capture-start state.");
            }

            ValidateStableDependencies(
                state.dependencyHashesAtStart,
                manifest.dependencyHashes);
            string[] statePaths = state.dependencyPaths ?? Array.Empty<string>();
            string[] manifestPaths = manifest.dependencyHashes?
                .Select(value => value.path).ToArray() ?? Array.Empty<string>();
            if (!statePaths.SequenceEqual(
                    manifestPaths,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "G08 manifest dependency order differs from capture-start state.");
            }
        }

        internal static void ValidateManifestProofProvenance(
            AuditionPvCaptureManifest manifest,
            RuntimeProof proof)
        {
            if (manifest == null || proof == null)
            {
                throw new ArgumentNullException(
                    manifest == null ? nameof(manifest) : nameof(proof));
            }

            var engine = new AuditionPvEngineSnapshot
            {
                unityVersion = manifest.unityVersion,
                unityVersionWithRevision = manifest.unityVersionWithRevision,
                recorderPackageVersion = manifest.recorderPackageVersion,
                urpPackageVersion = manifest.urpPackageVersion,
                activeRenderPipelineAssetPath = manifest.activeRenderPipelineAssetPath
            };
            string expected = ComputeCaptureStartProvenanceSha256(
                manifest.captureId,
                manifest.createdAtUtc,
                manifest.outputRoot,
                manifest.outputDirectory,
                manifest.gitCommitSha,
                manifest.gitBranch,
                manifest.gitWorktreeDirty,
                manifest.worktreeDirtyHashSha256,
                manifest.worktreeDirtyHashAlgorithm,
                engine,
                manifest.dependencyHashes);
            if (!string.Equals(
                    expected,
                    proof.captureStartProvenanceSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G08 manifest provenance digest is not bound to runtime proof.");
            }
        }

        private static AuditionPvEngineSnapshot CopyEngine(AuditionPvEngineSnapshot value)
        {
            return new AuditionPvEngineSnapshot
            {
                unityVersion = value?.unityVersion ?? string.Empty,
                unityVersionWithRevision = value?.unityVersionWithRevision ?? string.Empty,
                recorderPackageVersion = value?.recorderPackageVersion ?? string.Empty,
                urpPackageVersion = value?.urpPackageVersion ?? string.Empty,
                activeRenderPipelineAssetPath =
                    value?.activeRenderPipelineAssetPath ?? string.Empty
            };
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

        internal enum RunnerPhase
        {
            AwaitingPlayMode,
            Recording,
            AwaitingEditMode,
            FailedInPlayMode
        }

        internal enum SessionRecoveryDecision
        {
            Continue,
            CommittedManifest,
            TerminalFault
        }

        internal enum ResumeWatchdogAction
        {
            Unregister,
            KeepWaiting,
            Run
        }

        internal sealed class G08PixelCalibrationRequiredException
            : InvalidOperationException
        {
            public G08PixelCalibrationRequiredException(string message)
                : base(message)
            {
            }
        }

        internal sealed class G08VisualCompositionAcceptanceRequiredException
            : InvalidOperationException
        {
            public G08VisualCompositionAcceptanceRequiredException(string message)
                : base(message)
            {
            }
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
            public AuditionPvEngineSnapshot engine;
            public string[] dependencyPaths = Array.Empty<string>();
            public AuditionPvDependencyHash[] dependencyHashesAtStart =
                Array.Empty<AuditionPvDependencyHash>();
            public bool produceApprovedSixtySecondEvidence;
            public string s090RuntimeWorkloadSealPath = string.Empty;
            public RuntimeProof runtimeProof;
            public string failure = string.Empty;
        }

        [Serializable]
        internal sealed class RuntimeProof
        {
            public bool directorCompleted;
            public int lastLogicalFrame = -1;
            public int presentedFrameCount;
            public bool presentedFramesExact = true;
            public bool presentationClockExact = true;
            public int recorderWarmupEndOfFrameCount;
            public int recorderPreHandleEndOfFrameCount;
            public int canonicalSourceFrameCount;
            public int logicalFirstSourceFrame = -1;
            public int logicalLastSourceFrame = -1;
            public int s090SelectStartFrame = -1;
            public int s090SelectEndFrame = -1;
            public int recordedPreHandleFrameCount;
            public int recordedPostHandleFrameCount;
            public bool recorderAutoStoppedAfterLastFrame;

            public string runId = string.Empty;
            public string playableStageId = string.Empty;
            public int routeRevision;
            public string routeDigest = string.Empty;
            public string transitionTokenId = string.Empty;
            public string transitionTokenDigest = string.Empty;
            public long loaderGeneration;
            public string segmentEntryReceiptId = string.Empty;
            public string segmentEntryReceiptDigest = string.Empty;
            public string handoffTerminalReceiptId = string.Empty;
            public string handoffTerminalReceiptDigest = string.Empty;
            public bool enteredFromHandoffPending;
            public bool exactHandoffReceiptChain;
            public bool productTransitionProviderObserved;
            public bool productTransitionDestinationArrived;
            public bool productTransitionHandoffCompleted;
            public uint productTransitionGeneration;
            public bool entryGuideObservedPlaying;
            public bool entryGuideReleased;

            public int phaseTransitionStartCount;
            public int phaseTransitionCompletionCount;
            public bool phaseTwoApplied;
            public float preparedHealth;
            public float bossHealthBeforeShot;
            public int pressureScreensBeforeDismiss;
            public int pressureSummonsDismissed;
            public int pressureScreensAfterDismiss = -1;
            public float predictedBossSweepDistance;
            public int predictedNaturalImpactFrame = -1;
            public float preShotPlayerPlanarStepDistance;
            public float projectileConfiguredLocalRadius;
            public float projectileConfiguredWorldRadius;
            public Vector3 projectilePrefabLocalScale;
            public Vector3 projectileRootLossyScale;
            public string projectilePrefabAssetPath = string.Empty;
            public string projectilePrefabAssetGuid = string.Empty;
            public float projectileObservedLocalRadius;
            public float projectileObservedWorldRadius;
            public Vector3 projectileObservedLossyScale;
            public bool bossPressureMovementWasEnabled;
            public bool bossPressureMovementHoldAcquired;
            public bool bossPoseStableThroughImpact;
            public Vector3 bossPositionAtShotArm;
            public Vector3 bossPositionAtImpact;
            public float maximumBossPositionDriftThroughImpact;
            public float maximumBossRotationDriftThroughImpact;

            public int fireFrame = -1;
            public int projectileFiredFrame = -1;
            public int bossDiedFrame = -1;
            public int projectileImpactFrame = -1;
            public int terminalResolvedFrame = -1;
            public int firstFreezeFrame = -1;
            public int firstResultSceneFrame = -1;
            public int firstResultConfiguredFrame = -1;
            public int firstInteractiveFrame = -1;
            public int aftermathCompletedFrame = -1;
            public int inputLeaseReleasedFrame = -1;
            public int deathStateHeldFrame = -1;

            public int rangedFireStartedCount;
            public int projectileFiredCount;
            public int projectileDamageAppliedCount;
            public int bossDamagedDuringShotCount;
            public int bossDiedCount;
            public int encounterTerminalResolvedCount;
            public int overlayPresentationSucceededCount;
            public int aftermathStartedCount;
            public int aftermathCompletedCount;
            public int projectileInstanceId;
            public int projectileFiredSequence;
            public int bossDiedSequence;
            public int projectileImpactSequence;
            public int terminalResolvedSequence;
            public Vector3 projectileSpawnPosition;
            public Vector3 projectilePositionAtFrame61;
            public Vector3 projectileImpactPoint;
            public Vector3 projectileImpactDirection;
            public bool physicalProjectileObservedActiveBeforeImpact;
            public bool projectileMovedBeforeImpact;

            public bool noEarlyFreeze;
            public bool resultAbsentBeforeRequest;
            public bool allEightLocksObservedAtImpact;
            public bool allEightLocksReleasedAtResult;
            public bool deathStateAtAftermathHero;
            public bool aftermathCompletedSuccessfully;
            public string aftermathLastError = string.Empty;
            public string aftermathQualityWarning = string.Empty;
            public bool aftermathScaleOneObserved;
            public bool aftermathScaleOneViolated;
            public int aftermathBeginCount;
            public int aftermathCompleteCount;
            public float aftermathElapsedSeconds;
            public bool exclusiveCameraScheduleExact;
            public int cameraRoleTransitionCount;
            public int firstFinisherCameraFrame = -1;
            public int firstGameplayCameraRestoreFrame = -1;
            public bool finisherTerminalHoldExactAt218;
            public bool finisherReleaseExactAt246;
            public bool finisherCameraSucceeded;
            public bool finisherCameraReleaseScheduled;
            public bool finisherCameraInterrupted;
            public bool fallbackCameraCueSucceeded;
            public int finisherCameraRequestVersion = -1;
            public int finisherCameraAcquireCount;
            public int finisherCameraReleaseCount;
            public int finisherCameraControllerRequestVersion = -1;
            public int finisherCameraSampleCount;
            public int finisherCameraResultCoverReleaseSampleCount;
            public double finisherCameraLastSampledSeconds;
            public float finisherCameraResultCoverReleaseElapsedSeconds;
            public bool finisherCameraReachedTerminalSample;
            public bool finisherCameraLeaseReleased;
            public bool finisherCameraGameplayRestored;
            public bool finisherCameraDisabledAtResult;
            public int bossDeathCameraRequestCount;
            public int bossDeathCameraVersion;
            public bool bossDeathCameraInterrupted;
            public bool bossDeathCameraComplete;
            public int bossDeathVfxRequestCount;
            public int bossDeathAudioSourceDelta;
            public bool bossDeathUsesPhaseTwoAnchor;
            public int deathMotionRequestCount;
            public bool motionIsDead;
            public bool motionAttacksStopped;
            public bool animatorInDeathState;

            public bool overlayShown;
            public bool overlayFrozen;
            public bool resultSummarySameInstance;
            public bool presentedSummarySameInstance;
            public string committedSummaryDigest = string.Empty;
            public string presentedSummaryDigest = string.Empty;
            public string outcomeFactDigest = string.Empty;
            public long rootAdmissionSequence;
            public long terminalEpoch;
            public string terminalEpochEvidenceDigest = string.Empty;
            public string terminalClosureDigest = string.Empty;
            public int terminalRecordReceiptCount;
            public bool terminalFactsExact;
            public bool hudWasActiveAtFire;
            public bool hudWasActiveAtImpact;
            public bool hudYieldedAtResult;
            public bool resultInteractiveAt246;
            public bool pocketClearMarkerReferenceUnbound;
            public bool pocketClearMarkerInactiveAtEnd;
            public bool terminalBoundaryVisualHiddenAtEnd;

            public bool stateRestored;
            public bool eventsReleased;
            public bool presentationClockReleased;
            public bool cadenceReleased;
            public bool bossPressureMovementRestored;
            public bool transitionCaptureStateReleased;
            public bool globalCaptureStateRestored;
            public bool editModeSceneCleanupExact;
            public bool editModeGlobalCleanupExact;
            public string cleanupFailure = string.Empty;

            public RenderEvidence[] renderEvidence = Array.Empty<RenderEvidence>();
            public int pixelSampleStride;
            public long pixelSampleCount;
            public double sequenceBlackRatio;
            public double sequenceMagentaRatio;
            public double maximumFrameMagentaRatio;
            public double healthyFramePercent;
            public int frameDeltaPixelSampleStride;
            public int frameDeltaPixelSampleCount;
            public int frameDeltaChangedRgbSumCutoff;
            public int impactDeltaFromFrame;
            public int impactDeltaToFrame;
            public double impactMeanAbsoluteRgb;
            public double impactChangedRatio;
            public int aftermathDeltaFromFrame;
            public int aftermathDeltaToFrame;
            public double aftermathEvolutionMeanAbsoluteRgb;
            public double aftermathEvolutionChangedRatio;
            public int resultAppearanceFromFrame;
            public int resultAppearanceToFrame;
            public double resultAppearanceMeanAbsoluteRgb;
            public double resultAppearanceChangedRatio;
            public int resultEntranceFromFrame;
            public int resultEntranceToFrame;
            public double resultEntranceMeanAbsoluteRgb;
            public double resultEntranceChangedRatio;
            public int resultSurfaceFrame;
            public int resultSurfaceRoiX;
            public int resultSurfaceRoiY;
            public int resultSurfaceRoiWidth;
            public int resultSurfaceRoiHeight;
            public int resultSurfaceSampleStride;
            public int resultSurfaceSampleCount;
            public int resultBrightMinimumChannel;
            public int resultNavyMaximumLuma;
            public int resultBlueMinimumChannel;
            public int resultBlueMinimumRedDelta;
            public int resultBlueMinimumGreenDelta;
            public int resultBrightSamples;
            public int resultNavySamples;
            public int resultBlueSamples;

            public string frameHashLedgerPath = string.Empty;
            public string frameHashLedgerSha256 = string.Empty;
            public int frameHashLedgerEntryCount;
            public string warmupEvidencePath = string.Empty;
            public string warmupEvidenceSha256 = string.Empty;
            public string bl10Sha256 = string.Empty;
            public string bl11Sha256 = string.Empty;
            public string bl12Sha256 = string.Empty;
            public int dependencyHashCount;
            public string captureStartProvenanceSha256 = string.Empty;
        }

        [Serializable]
        internal sealed class RenderEvidence
        {
            public int frame;
            public string cameraRole = string.Empty;
            public bool gameplayCameraExact;
            public bool finisherCameraExact;
            public bool exclusiveCameraRoleExact;
            public bool finisherLeaseReleased;
            public bool combatHudVisible;
            public float projectionAspect;
            public bool playerSafeViewport;
            public bool bossSafeViewport;
            public bool playerFullyInsideFrustum;
            public bool playerFullyOutsideFrustum;
            public bool playerPartiallyClipped;
            public bool bossFullyInsideFrustum;
            public bool bossFullyOutsideFrustum;
            public bool bossPartiallyClipped;
            public float playerBodyHeightRatio;
            public string bossBodyRendererNames = string.Empty;
            public int bossBodyRendererCount;
            public float bossBodyWidthRatio;
            public float bossBodyHeightRatio;
            public float bossBodyMaxExtentRatio;
            public bool bossEnvelopeVisible;
            public bool bossEnvelopeFullyInsideFrustum;
            public bool bossEnvelopeFullyOutsideFrustum;
            public bool bossEnvelopePartiallyClipped;
            public int bossEnvelopeRendererCount;
            public float bossEnvelopeWidthRatio;
            public float bossEnvelopeHeightRatio;
            public float bossEnvelopeMaxExtentRatio;
            public string bossCoreAxisSource = string.Empty;
            public Vector3 bossCoreAxisHipsViewport;
            public Vector3 bossCoreAxisHeadViewport;
            public float bossCoreAxisViewportLength;
            public bool resultCanvasVisible;
            public bool resultInteractive;
            public string objectiveText = string.Empty;
            public string bossLabelText = string.Empty;
            public bool objectiveForbiddenInternalTokensAbsent;
            public bool pocketClearMarkerReferenceUnbound;
            public bool pocketClearMarkerPresent;
            public bool pocketClearMarkerInactive;
            public bool terminalBoundaryVisualPresent;
            public bool terminalBoundaryVisualHidden;
            public bool redundantClearTextPresent;
            public bool redundantClearTextInactive;
            public bool realClearIconPresent;
            public bool realClearIconActive;
            public Vector3 playerViewport;
            public Vector3 bossViewport;
            public Vector3 bossEnvelopeViewport;
            public Vector2 playerPixelExtent;
            public Vector2 bossPixelExtent;
            public Vector2 bossEnvelopePixelExtent;
        }

        [Serializable]
        private sealed class RuntimeProofArtifact
        {
            public string schema = string.Empty;
            public string captureId = string.Empty;
            public string sourceCaptureCoreSha256 = string.Empty;
            public int sourceRangeStartFrame = -1;
            public int sourceRangeEndFrame = -1;
            public int selectStartFrame = -1;
            public int selectEndFrame = -1;
            public AuditionPvPinnedArtifact sourceFrameLedger = new();
            public string mapping = string.Empty;
            public string gameplay = string.Empty;
            public RuntimeProof runtime;
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
            public int sourceRangeStartFrame = -1;
            public int sourceRangeEndFrame = -1;
            public int logicalFactStartFrame = -1;
            public int logicalFactEndFrame = -1;
            public int sourceFactStartFrame = -1;
            public int sourceFactEndFrame = -1;
            public string[] exactFacts = Array.Empty<string>();
            public AuditionPvPinnedArtifact runtimeProof = new();
            public AuditionPvPinnedArtifact sourceFrameLedger = new();
            public string producer = string.Empty;
            public string createdAtUtc = string.Empty;
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
            public string retainedArtifacts = string.Empty;
            public bool pixelCalibrationLocked;
            public bool calibrationRequired;
            public bool visualCompositionAcceptanceLocked;
            public bool visualCompositionAcceptanceRequired;
            public string successArtifactCleanupFailure = string.Empty;
            public RuntimeProof runtime;
        }
    }

    /// <summary>
    /// Flattens only managed iterator nesting so every MoveNext/Current/Dispose
    /// exception returns to the G08 transaction owner. Unity-native waits remain
    /// yielded to the engine with their original scheduling semantics.
    /// </summary>
    internal sealed class G08GuardedIteratorDriver
    {
        private readonly Stack<IEnumerator> iterators = new Stack<IEnumerator>();
        private bool terminal;

        internal G08GuardedIteratorDriver(IEnumerator root)
        {
            iterators.Push(root ?? throw new ArgumentNullException(nameof(root)));
        }

        internal int Depth => iterators.Count;

        internal bool TryMoveNext(out object yielded, out Exception failure)
        {
            yielded = null;
            failure = null;
            if (terminal)
            {
                return false;
            }

            while (iterators.Count > 0)
            {
                IEnumerator current = iterators.Peek();
                bool moved;
                object value = null;
                try
                {
                    moved = current.MoveNext();
                    if (moved)
                    {
                        value = current.Current;
                    }
                }
                catch (Exception exception)
                {
                    failure = Combine(exception, DisposeRemaining());
                    return false;
                }

                if (!moved)
                {
                    iterators.Pop();
                    Exception disposeFailure = DisposeOne(current);
                    if (disposeFailure != null)
                    {
                        failure = Combine(disposeFailure, DisposeRemaining());
                        return false;
                    }

                    continue;
                }

                if (value is IEnumerator nested
                    && !(value is CustomYieldInstruction))
                {
                    if (ContainsReference(nested))
                    {
                        failure = Combine(
                            new InvalidOperationException(
                                "G08 nested iterator graph contained a reference cycle."),
                            DisposeRemaining());
                        return false;
                    }

                    iterators.Push(nested);
                    continue;
                }

                yielded = value;
                return true;
            }

            terminal = true;
            return false;
        }

        internal Exception DisposeRemaining()
        {
            Exception failure = null;
            while (iterators.Count > 0)
            {
                failure = Combine(failure, DisposeOne(iterators.Pop()));
            }

            terminal = true;
            return failure;
        }

        private bool ContainsReference(IEnumerator candidate)
        {
            foreach (IEnumerator iterator in iterators)
            {
                if (ReferenceEquals(iterator, candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static Exception DisposeOne(IEnumerator iterator)
        {
            try
            {
                (iterator as IDisposable)?.Dispose();
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
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
    /// Owns the exact core-proof-cleanup-proof-notify transaction used by the
    /// PlayMode runner. All managed iterator faults are converted into one
    /// terminal failure value, cleanup always runs, and notification is invoked
    /// exactly once after both proof hooks.
    /// </summary>
    internal static class G08GuardedCoroutineTransaction
    {
        internal static IEnumerator Run(
            IEnumerator core,
            Func<Exception> captureCoreProof,
            Func<IEnumerator> cleanupFactory,
            Func<Exception> captureCleanupProof,
            Action<Exception> notify)
        {
            Exception failure = null;
            G08GuardedIteratorDriver coreDriver = TryCreateDriver(
                core,
                "core",
                out Exception coreCreationFailure);
            failure = Combine(failure, coreCreationFailure);
            while (failure == null && coreDriver != null)
            {
                bool moved = coreDriver.TryMoveNext(
                    out object yielded,
                    out Exception iteratorFailure);
                failure = Combine(failure, iteratorFailure);
                if (!moved)
                {
                    break;
                }

                yield return yielded;
            }

            failure = Combine(
                failure,
                coreDriver?.DisposeRemaining());
            failure = Combine(
                failure,
                InvokeProofHook(captureCoreProof, "core proof"));

            IEnumerator cleanup = InvokeCleanupFactory(
                cleanupFactory,
                out Exception cleanupCreationFailure);
            failure = Combine(failure, cleanupCreationFailure);
            G08GuardedIteratorDriver cleanupDriver = null;
            Exception cleanupDriverFailure = null;
            if (cleanupCreationFailure == null)
            {
                cleanupDriver = TryCreateDriver(
                    cleanup,
                    "cleanup",
                    out cleanupDriverFailure);
            }

            failure = Combine(failure, cleanupDriverFailure);
            while (cleanupDriver != null)
            {
                bool moved = cleanupDriver.TryMoveNext(
                    out object yielded,
                    out Exception iteratorFailure);
                failure = Combine(failure, iteratorFailure);
                if (!moved)
                {
                    break;
                }

                yield return yielded;
            }

            failure = Combine(
                failure,
                cleanupDriver?.DisposeRemaining());
            failure = Combine(
                failure,
                InvokeProofHook(captureCleanupProof, "cleanup proof"));
            InvokeNotifyOnce(notify, failure);
        }

        private static G08GuardedIteratorDriver TryCreateDriver(
            IEnumerator iterator,
            string label,
            out Exception failure)
        {
            try
            {
                if (iterator == null)
                {
                    throw new InvalidOperationException(
                        $"G08 guarded {label} iterator was null.");
                }

                failure = null;
                return new G08GuardedIteratorDriver(iterator);
            }
            catch (Exception exception)
            {
                failure = exception;
                return null;
            }
        }

        private static IEnumerator InvokeCleanupFactory(
            Func<IEnumerator> cleanupFactory,
            out Exception failure)
        {
            try
            {
                if (cleanupFactory == null)
                {
                    throw new InvalidOperationException(
                        "G08 guarded cleanup factory was null.");
                }

                IEnumerator cleanup = cleanupFactory();
                failure = null;
                return cleanup;
            }
            catch (Exception exception)
            {
                failure = exception;
                return null;
            }
        }

        private static Exception InvokeProofHook(
            Func<Exception> proofHook,
            string label)
        {
            try
            {
                if (proofHook == null)
                {
                    return new InvalidOperationException(
                        $"G08 guarded {label} hook was null.");
                }

                return proofHook();
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static void InvokeNotifyOnce(
            Action<Exception> notify,
            Exception failure)
        {
            try
            {
                if (notify == null)
                {
                    throw new InvalidOperationException(
                        "G08 guarded terminal notification callback was null.");
                }

                notify(failure);
            }
            catch (Exception exception)
            {
                Debug.LogException(Combine(failure, exception));
            }
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

    [DefaultExecutionOrder(-32500)]
    public sealed class AuditionPvStationBossDeathAftermathGoldenRunnerBehaviour
        : MonoBehaviour
    {
        private const double ShotTimeoutSeconds = 150d;

        private string statePath = string.Empty;
        private string outputDirectory = string.Empty;
        private AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState state;
        private AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof;
        private AuditionPvStationBossDeathAftermathDirector director;
        private AuditionPvStationBossDeathAftermathRenderProbe renderProbe;
        private AuditionPvRecorderSettingsBundle recorderSettings;
        private RecorderController recorderController;
        private AuditionPvRuntimeWorkloadCaptureSession s090RuntimeWorkload;
        private bool armLogicalFrameZero;
        private bool beganLogicalShot;
        private bool cleaningUp;
        private bool notified;
        private int nextPresentedFrame;
        private Exception updateFailure;

        internal void Begin(
            string newStatePath,
            string newOutputDirectory,
            AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
                newState)
        {
            statePath = newStatePath;
            outputDirectory = newOutputDirectory;
            state = newState ?? throw new ArgumentNullException(nameof(newState));
            proof = state.runtimeProof
                ?? new AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof();
            StartCoroutine(RunGuarded());
        }

        private void Update()
        {
            if (!armLogicalFrameZero || beganLogicalShot || updateFailure != null)
            {
                return;
            }

            try
            {
                if (Time.captureFramerate != AuditionPvCaptureContract.Fps
                    || recorderController == null
                    || !recorderController.IsRecording())
                {
                    throw new InvalidOperationException(
                        "G08 Recorder padding was not active at the early-Update f0 arm.");
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
            return G08GuardedCoroutineTransaction.Run(
                RunCore(),
                CaptureDirectorProof,
                CleanupAfterRecorder,
                CaptureCleanupProof,
                NotifyFinished);
        }

        private IEnumerator RunCore()
        {
            director = AuditionPvStationBossDeathAftermathCapture
                .AttachToFreshCorridorScene();
            director.FramePresented += HandleFramePresented;
            renderProbe = gameObject.AddComponent<
                AuditionPvStationBossDeathAftermathRenderProbe>();
            renderProbe.Configure(director);

            yield return director.PrepareFreshProductState();

            if (!director.IsPrepared)
            {
                throw new InvalidOperationException(
                    "G08 canonical product-state director did not finish preparation.");
            }

            s090RuntimeWorkload = AuditionPvRuntimeWorkloadCaptureSession.Open(
                new AuditionPvRuntimeWorkloadCaptureConfig
                {
                    captureId = state.captureId,
                    captureOutputDirectory = outputDirectory,
                    sourceShotId =
                        AuditionPvStationBossDeathAftermathCapture.ShotId,
                    sourceRangeStartFrame =
                        AuditionPvStationBossDeathAftermathCapture.FirstFrame,
                    sourceRangeEndFrame =
                        AuditionPvStationBossDeathAftermathCapture.LastFrame,
                    captureHudEvidence = false
                });

            recorderSettings = AuditionPvRecorderSettingsFactory
                .CreateLosslessPngSequence(
                    outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.ShotId);
            recorderSettings.controllerSettings.SetRecordModeToFrameInterval(
                AuditionPvStationBossDeathAftermathGoldenRunner.RawWarmupFrame,
                AuditionPvStationBossDeathAftermathGoldenRunner.RawLastShotFrame);
            AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
            recorderController = new RecorderController(
                recorderSettings.controllerSettings);
            recorderController.PrepareRecording();
            if (!recorderController.StartRecording())
            {
                throw new InvalidOperationException(
                    "Unity Recorder 5.1.6 rejected the G08 QHD60 PNG session.");
            }

            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 1;
            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 2;
            for (int handleFrame = 0;
                handleFrame
                    < AuditionPvStationBossDeathAftermathCapture.HandleFrameCount;
                handleFrame++)
            {
                yield return new WaitForEndOfFrame();
                CaptureRuntimeWorkload(handleFrame);
                proof.recorderPreHandleEndOfFrameCount++;
            }

            proof.canonicalSourceFrameCount =
                AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
            proof.logicalFirstSourceFrame =
                AuditionPvStationBossDeathAftermathCapture.SelectStartFrame;
            proof.logicalLastSourceFrame =
                AuditionPvStationBossDeathAftermathCapture.SelectEndFrame;
            proof.s090SelectStartFrame =
                AuditionPvStationBossDeathAftermathCapture.S090SelectStartFrame;
            proof.s090SelectEndFrame =
                AuditionPvStationBossDeathAftermathCapture.S090SelectEndFrame;
            proof.recordedPreHandleFrameCount =
                proof.recorderPreHandleEndOfFrameCount;
            if (!recorderController.IsRecording()
                || director.IsRunning
                || director.IsComplete)
            {
                throw new InvalidOperationException(
                    "G08 did not record the complete prehandle before arming logical f0.");
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
                    "G08 could not arm logical f0 after Recorder warm-up.",
                    updateFailure);
            }

            if (!beganLogicalShot)
            {
                throw new TimeoutException(
                    "G08 timed out before its early-Update logical f0 arm.");
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
                    "G08 product director failed during recording.",
                    director.Failure);
            }

            if (renderProbe.Failure != null)
            {
                throw new InvalidOperationException(
                    "G08 render probe failed during recording.",
                    renderProbe.Failure);
            }

            if (!director.IsComplete)
            {
                throw new TimeoutException(
                    "G08 did not complete logical frames 0..359 before timeout.");
            }

            // Logical f359 occupies canonical source f539. The committed result
            // remains product-owned while Recorder captures the complete suffix.
            int recordedPostHandleFrames = 0;
            for (; recordedPostHandleFrames <
                   AuditionPvStationBossDeathAftermathCapture.HandleFrameCount;
                 recordedPostHandleFrames++)
            {
                if (!recorderController.IsRecording())
                {
                    throw new InvalidOperationException(
                        "G08 Recorder stopped before the complete runtime-evidenced posthandle.");
                }

                yield return new WaitForEndOfFrame();
                CaptureRuntimeWorkload(
                    AuditionPvStationBossDeathAftermathCapture.SelectEndFrame + 1
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
                    "G08 Recorder did not auto-stop after raw720/canonical source f719.");
            }

            proof.recordedPostHandleFrameCount = recordedPostHandleFrames;
            state.s090RuntimeWorkloadSealPath = s090RuntimeWorkload.Complete();
            s090RuntimeWorkload = null;
        }

        private void HandleFramePresented(int frameIndex)
        {
            CaptureRuntimeWorkload(
                AuditionPvStationBossDeathAftermathCapture.SelectStartFrame
                + frameIndex);
            proof.presentedFramesExact &= frameIndex == nextPresentedFrame;
            proof.presentationClockExact &= PresentationClock.IsManuallyDriven
                && Mathf.Abs(
                    PresentationClock.UnscaledTime
                    - frameIndex / (float)AuditionPvCaptureContract.Fps) <= 0.00001f
                && Mathf.Abs(
                    PresentationClock.UnscaledDeltaTime
                    - 1f / AuditionPvCaptureContract.Fps) <= 0.00001f;
            proof.presentedFrameCount++;
            nextPresentedFrame++;
        }

        private void CaptureRuntimeWorkload(int sourceFrame)
        {
            s090RuntimeWorkload?.CapturePresentedFrame(sourceFrame);
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
                proof.runId = director.RunId;
                proof.playableStageId = director.PlayableStageId;
                proof.routeRevision = director.RouteRevision;
                proof.routeDigest = director.RouteDigest;
                proof.transitionTokenId = director.TransitionTokenId;
                proof.transitionTokenDigest = director.TransitionTokenDigest;
                proof.loaderGeneration = director.LoaderGeneration;
                proof.segmentEntryReceiptId = director.SegmentEntryReceiptId;
                proof.segmentEntryReceiptDigest = director.SegmentEntryReceiptDigest;
                proof.handoffTerminalReceiptId = director.HandoffTerminalReceiptId;
                proof.handoffTerminalReceiptDigest =
                    director.HandoffTerminalReceiptDigest;
                proof.enteredFromHandoffPending = director.EnteredFromHandoffPending;
                proof.exactHandoffReceiptChain = director.ExactHandoffReceiptChain;
                proof.productTransitionProviderObserved =
                    director.ProductTransitionProviderObserved;
                proof.productTransitionDestinationArrived =
                    director.ProductTransitionDestinationArrived;
                proof.productTransitionHandoffCompleted =
                    director.ProductTransitionHandoffCompleted;
                proof.productTransitionGeneration =
                    director.ProductTransitionGeneration;
                proof.entryGuideObservedPlaying = director.EntryGuideObservedPlaying;
                proof.entryGuideReleased = director.EntryGuideReleased;
                proof.phaseTransitionStartCount = director.PhaseTransitionStartCount;
                proof.phaseTransitionCompletionCount =
                    director.PhaseTransitionCompletionCount;
                proof.phaseTwoApplied = director.PhaseTwoApplied;
                proof.preparedHealth = director.PreparedHealthObserved;
                proof.bossHealthBeforeShot = director.BossHealthBeforeShot;
                proof.pressureScreensBeforeDismiss =
                    director.PressureScreensBeforeDismiss;
                proof.pressureSummonsDismissed = director.PressureSummonsDismissed;
                proof.pressureScreensAfterDismiss =
                    director.PressureScreensAfterDismiss;
                proof.predictedBossSweepDistance =
                    director.PredictedBossSweepDistance;
                proof.predictedNaturalImpactFrame =
                    director.PredictedNaturalImpactFrame;
                proof.preShotPlayerPlanarStepDistance =
                    director.PreShotPlayerPlanarStepDistance;
                proof.projectileConfiguredLocalRadius =
                    director.ProjectileConfiguredLocalRadius;
                proof.projectileConfiguredWorldRadius =
                    director.ProjectileConfiguredWorldRadius;
                proof.projectilePrefabLocalScale =
                    director.ProjectilePrefabLocalScale;
                proof.projectileRootLossyScale =
                    director.ProjectileRootLossyScale;
                proof.projectilePrefabAssetPath =
                    director.ProjectilePrefabAssetPath;
                proof.projectilePrefabAssetGuid =
                    director.ProjectilePrefabAssetGuid;
                proof.projectileObservedLocalRadius =
                    director.ProjectileObservedLocalRadius;
                proof.projectileObservedWorldRadius =
                    director.ProjectileObservedWorldRadius;
                proof.projectileObservedLossyScale =
                    director.ProjectileObservedLossyScale;
                proof.bossPressureMovementWasEnabled =
                    director.BossPressureMovementWasEnabled;
                proof.bossPressureMovementHoldAcquired =
                    director.BossPressureMovementHoldAcquired;
                proof.bossPoseStableThroughImpact =
                    director.BossPoseStableThroughImpact;
                proof.bossPositionAtShotArm = director.BossPositionAtShotArm;
                proof.bossPositionAtImpact = director.BossPositionAtImpact;
                proof.maximumBossPositionDriftThroughImpact =
                    director.MaximumBossPositionDriftThroughImpact;
                proof.maximumBossRotationDriftThroughImpact =
                    director.MaximumBossRotationDriftThroughImpact;

                proof.fireFrame = director.FireFrame;
                proof.projectileFiredFrame = director.ProjectileFiredFrame;
                proof.bossDiedFrame = director.BossDiedFrame;
                proof.projectileImpactFrame = director.ProjectileImpactFrame;
                proof.terminalResolvedFrame = director.TerminalResolvedFrame;
                proof.firstFreezeFrame = director.FirstFreezeFrame;
                proof.firstResultSceneFrame = director.FirstResultSceneFrame;
                proof.firstResultConfiguredFrame = director.FirstResultConfiguredFrame;
                proof.firstInteractiveFrame = director.FirstInteractiveFrame;
                proof.aftermathCompletedFrame = director.AftermathCompletedFrame;
                proof.inputLeaseReleasedFrame = director.InputLeaseReleasedFrame;
                proof.deathStateHeldFrame = director.DeathStateHeldFrame;

                proof.rangedFireStartedCount = director.RangedFireStartedCount;
                proof.projectileFiredCount = director.ProjectileFiredCount;
                proof.projectileDamageAppliedCount =
                    director.ProjectileDamageAppliedCount;
                proof.bossDamagedDuringShotCount = director.BossDamagedDuringShotCount;
                proof.bossDiedCount = director.BossDiedCount;
                proof.encounterTerminalResolvedCount =
                    director.EncounterTerminalResolvedCount;
                proof.overlayPresentationSucceededCount =
                    director.OverlayPresentationSucceededCount;
                proof.aftermathStartedCount = director.AftermathStartedCount;
                proof.aftermathCompletedCount = director.AftermathCompletedCount;
                proof.projectileInstanceId = director.ProjectileInstanceId;
                proof.projectileFiredSequence = director.ProjectileFiredSequence;
                proof.bossDiedSequence = director.BossDiedSequence;
                proof.projectileImpactSequence = director.ProjectileImpactSequence;
                proof.terminalResolvedSequence = director.TerminalResolvedSequence;
                proof.projectileSpawnPosition = director.ProjectileSpawnPosition;
                proof.projectilePositionAtFrame61 = director.ProjectilePositionAtFrame61;
                proof.projectileImpactPoint = director.ProjectileImpactPoint;
                proof.projectileImpactDirection = director.ProjectileImpactDirection;
                proof.physicalProjectileObservedActiveBeforeImpact =
                    director.PhysicalProjectileObservedActiveBeforeImpact;
                proof.projectileMovedBeforeImpact = director.ProjectileMovedBeforeImpact;

                proof.noEarlyFreeze = director.NoEarlyFreeze;
                proof.resultAbsentBeforeRequest = director.ResultAbsentBeforeRequest;
                proof.allEightLocksObservedAtImpact =
                    director.AllEightLocksObservedAtImpact;
                proof.allEightLocksReleasedAtResult =
                    director.AllEightLocksReleasedAtResult;
                proof.deathStateAtAftermathHero = director.DeathStateAtAftermathHero;
                proof.aftermathCompletedSuccessfully =
                    director.AftermathCompletedSuccessfully;
                proof.aftermathLastError = director.AftermathLastError;
                proof.aftermathQualityWarning = director.AftermathQualityWarning;
                proof.aftermathScaleOneObserved = director.AftermathScaleOneObserved;
                proof.aftermathScaleOneViolated = director.AftermathScaleOneViolated;
                proof.aftermathBeginCount = director.AftermathBeginCount;
                proof.aftermathCompleteCount = director.AftermathCompleteCount;
                proof.aftermathElapsedSeconds = director.AftermathElapsedSeconds;
                proof.exclusiveCameraScheduleExact =
                    director.ExclusiveCameraScheduleExact;
                proof.cameraRoleTransitionCount = director.CameraRoleTransitionCount;
                proof.firstFinisherCameraFrame = director.FirstFinisherCameraFrame;
                proof.firstGameplayCameraRestoreFrame =
                    director.FirstGameplayCameraRestoreFrame;
                proof.finisherTerminalHoldExactAt218 =
                    director.FinisherTerminalHoldExactAt218;
                proof.finisherReleaseExactAt246 =
                    director.FinisherReleaseExactAt246;
                proof.finisherCameraSucceeded = director.FinisherCameraSucceeded;
                proof.finisherCameraReleaseScheduled =
                    director.FinisherCameraReleaseScheduled;
                proof.finisherCameraInterrupted = director.FinisherCameraInterrupted;
                proof.fallbackCameraCueSucceeded =
                    director.FallbackCameraCueSucceeded;
                proof.finisherCameraRequestVersion =
                    director.FinisherCameraRequestVersion;
                proof.finisherCameraAcquireCount =
                    director.FinisherCameraAcquireCount;
                proof.finisherCameraReleaseCount =
                    director.FinisherCameraReleaseCount;
                proof.finisherCameraControllerRequestVersion =
                    director.FinisherCameraControllerRequestVersion;
                proof.finisherCameraSampleCount =
                    director.FinisherCameraSampleCount;
                proof.finisherCameraResultCoverReleaseSampleCount =
                    director.FinisherCameraResultCoverReleaseSampleCount;
                proof.finisherCameraLastSampledSeconds =
                    director.FinisherCameraLastSampledSeconds;
                proof.finisherCameraResultCoverReleaseElapsedSeconds =
                    director.FinisherCameraResultCoverReleaseElapsedSeconds;
                proof.finisherCameraReachedTerminalSample =
                    director.FinisherCameraReachedTerminalSample;
                proof.finisherCameraLeaseReleased =
                    director.FinisherCameraLeaseReleased;
                proof.finisherCameraGameplayRestored =
                    director.FinisherCameraGameplayRestored;
                proof.finisherCameraDisabledAtResult =
                    director.FinisherCameraDisabledAtResult;
                proof.bossDeathCameraRequestCount =
                    director.BossDeathCameraRequestCount;
                proof.bossDeathCameraVersion = director.BossDeathCameraVersion;
                proof.bossDeathCameraInterrupted = director.BossDeathCameraInterrupted;
                proof.bossDeathCameraComplete = director.BossDeathCameraComplete;
                proof.bossDeathVfxRequestCount = director.BossDeathVfxRequestCount;
                proof.bossDeathAudioSourceDelta = director.BossDeathAudioSourceDelta;
                proof.bossDeathUsesPhaseTwoAnchor =
                    director.BossDeathUsesPhaseTwoAnchor;
                proof.deathMotionRequestCount = director.DeathMotionRequestCount;
                proof.motionIsDead = director.MotionIsDead;
                proof.motionAttacksStopped = director.MotionAttacksStopped;
                proof.animatorInDeathState = director.AnimatorInDeathState;

                proof.overlayShown = director.OverlayShown;
                proof.overlayFrozen = director.OverlayFrozen;
                proof.resultSummarySameInstance = director.ResultSummarySameInstance;
                proof.presentedSummarySameInstance =
                    director.PresentedSummarySameInstance;
                proof.committedSummaryDigest = director.CommittedSummaryDigest;
                proof.presentedSummaryDigest = director.PresentedSummaryDigest;
                proof.outcomeFactDigest = director.OutcomeFactDigest;
                proof.rootAdmissionSequence = director.RootAdmissionSequence;
                proof.terminalEpoch = director.TerminalEpoch;
                proof.terminalEpochEvidenceDigest =
                    director.TerminalEpochEvidenceDigest;
                proof.terminalClosureDigest = director.TerminalClosureDigest;
                proof.terminalRecordReceiptCount = director.TerminalRecordReceiptCount;
                proof.terminalFactsExact = director.TerminalFactsExact;
                proof.hudWasActiveAtFire = director.HudWasActiveAtFire;
                proof.hudWasActiveAtImpact = director.HudWasActiveAtImpact;
                proof.hudYieldedAtResult = director.HudYieldedAtResult;
                proof.resultInteractiveAt246 = director.ResultInteractiveAt246;
                proof.pocketClearMarkerReferenceUnbound =
                    director.PocketClearMarkerReferenceUnbound;
                proof.pocketClearMarkerInactiveAtEnd =
                    director.PocketClearMarker != null
                    && !director.PocketClearMarker.activeSelf;
                proof.terminalBoundaryVisualHiddenAtEnd =
                    director.TerminalBoundaryVisualRoot != null
                    && !director.TerminalBoundaryVisualRoot.activeSelf;
                proof.renderEvidence = renderProbe != null
                    ? renderProbe.CopyEvidence()
                    : Array.Empty<
                        AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence>();
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
            Exception failure = null;
            try
            {
                try
                {
                    if (director != null)
                    {
                        director.FramePresented -= HandleFramePresented;
                    }
                }
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                }

                try
                {
                    if (recorderController != null
                        && recorderController.IsRecording())
                    {
                        recorderController.StopRecording();
                    }
                }
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                }

                recorderController = null;
                if (director != null)
                {
                    yield return director.RestoreAfterRecording();
                }
            }
            finally
            {
                try
                {
                    s090RuntimeWorkload?.Dispose();
                    s090RuntimeWorkload = null;
                }
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                }

                try
                {
                    recorderSettings?.Dispose();
                }
                catch (Exception exception)
                {
                    failure = Combine(failure, exception);
                }

                recorderSettings = null;
                recorderController = null;
                if (failure != null)
                {
                    throw new InvalidOperationException(
                        "G08 Recorder/director/settings cleanup encountered an error.",
                        failure);
                }
            }
        }

        private Exception CaptureCleanupProof()
        {
            Exception failure = null;
            try
            {
                if (director != null)
                {
                    proof.stateRestored = director.StateRestored;
                    proof.eventsReleased = director.EventsReleased;
                    proof.presentationClockReleased =
                        director.PresentationClockReleased;
                    proof.cadenceReleased = director.CadenceReleased;
                    proof.bossPressureMovementRestored =
                        director.BossPressureMovementRestored;
                    proof.transitionCaptureStateReleased =
                        director.TransitionCaptureStateReleased;
                    proof.globalCaptureStateRestored =
                        director.GlobalCaptureStateRestored;
                }
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            try
            {
                if (director != null && director.CleanupFailure != null)
                {
                    proof.cleanupFailure = director.CleanupFailure.ToString();
                    failure = Combine(failure, director.CleanupFailure);
                }
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            return failure;
        }

        private void NotifyFinished(Exception failure)
        {
            if (notified)
            {
                return;
            }

            try
            {
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .NotifyPlayModeFinished(statePath, state, proof, failure);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
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
                "G08 runner was disabled before asynchronous cleanup completed.");
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
                failure = Combine(failure, exception);
            }

            try
            {
                director?.RestoreCaptureOwnedState();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            try
            {
                s090RuntimeWorkload?.Dispose();
                s090RuntimeWorkload = null;
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            try
            {
                recorderSettings?.Dispose();
                recorderSettings = null;
                recorderController = null;
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
    /// Late geometry evidence from the exact camera/result objects actually
    /// presented to Recorder. It never drives gameplay or presentation.
    /// </summary>
    [DefaultExecutionOrder(32000)]
    public sealed class AuditionPvStationBossDeathAftermathRenderProbe
        : MonoBehaviour
    {
        private static readonly int[] EvidenceFrames = { 61, 62, 116, 181, 246 };
        private readonly List<
            AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence>
            evidence = new();
        private readonly List<Vector3> bakedCoreVertices = new();
        private AuditionPvStationBossDeathAftermathDirector director;
        private Mesh bakedCoreMesh;
        private int lastSampledFrame = -1;

        public Exception Failure { get; private set; }

        public void Configure(AuditionPvStationBossDeathAftermathDirector value)
        {
            director = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence[]
            CopyEvidence()
        {
            return evidence.Select(value => new
                AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
                {
                    frame = value.frame,
                    cameraRole = value.cameraRole,
                    gameplayCameraExact = value.gameplayCameraExact,
                    finisherCameraExact = value.finisherCameraExact,
                    exclusiveCameraRoleExact = value.exclusiveCameraRoleExact,
                    finisherLeaseReleased = value.finisherLeaseReleased,
                    combatHudVisible = value.combatHudVisible,
                    projectionAspect = value.projectionAspect,
                    playerSafeViewport = value.playerSafeViewport,
                    bossSafeViewport = value.bossSafeViewport,
                    playerFullyInsideFrustum = value.playerFullyInsideFrustum,
                    playerFullyOutsideFrustum = value.playerFullyOutsideFrustum,
                    playerPartiallyClipped = value.playerPartiallyClipped,
                    bossFullyInsideFrustum = value.bossFullyInsideFrustum,
                    bossFullyOutsideFrustum = value.bossFullyOutsideFrustum,
                    bossPartiallyClipped = value.bossPartiallyClipped,
                    playerBodyHeightRatio = value.playerBodyHeightRatio,
                    bossBodyRendererNames = value.bossBodyRendererNames,
                    bossBodyRendererCount = value.bossBodyRendererCount,
                    bossBodyWidthRatio = value.bossBodyWidthRatio,
                    bossBodyHeightRatio = value.bossBodyHeightRatio,
                    bossBodyMaxExtentRatio = value.bossBodyMaxExtentRatio,
                    bossEnvelopeVisible = value.bossEnvelopeVisible,
                    bossEnvelopeFullyInsideFrustum =
                        value.bossEnvelopeFullyInsideFrustum,
                    bossEnvelopeFullyOutsideFrustum =
                        value.bossEnvelopeFullyOutsideFrustum,
                    bossEnvelopePartiallyClipped =
                        value.bossEnvelopePartiallyClipped,
                    bossEnvelopeRendererCount = value.bossEnvelopeRendererCount,
                    bossEnvelopeWidthRatio = value.bossEnvelopeWidthRatio,
                    bossEnvelopeHeightRatio = value.bossEnvelopeHeightRatio,
                    bossEnvelopeMaxExtentRatio =
                        value.bossEnvelopeMaxExtentRatio,
                    bossCoreAxisSource = value.bossCoreAxisSource,
                    bossCoreAxisHipsViewport = value.bossCoreAxisHipsViewport,
                    bossCoreAxisHeadViewport = value.bossCoreAxisHeadViewport,
                    bossCoreAxisViewportLength =
                        value.bossCoreAxisViewportLength,
                    resultCanvasVisible = value.resultCanvasVisible,
                    resultInteractive = value.resultInteractive,
                    objectiveText = value.objectiveText,
                    bossLabelText = value.bossLabelText,
                    objectiveForbiddenInternalTokensAbsent =
                        value.objectiveForbiddenInternalTokensAbsent,
                    pocketClearMarkerReferenceUnbound =
                        value.pocketClearMarkerReferenceUnbound,
                    pocketClearMarkerPresent = value.pocketClearMarkerPresent,
                    pocketClearMarkerInactive = value.pocketClearMarkerInactive,
                    terminalBoundaryVisualPresent =
                        value.terminalBoundaryVisualPresent,
                    terminalBoundaryVisualHidden =
                        value.terminalBoundaryVisualHidden,
                    redundantClearTextPresent = value.redundantClearTextPresent,
                    redundantClearTextInactive = value.redundantClearTextInactive,
                    realClearIconPresent = value.realClearIconPresent,
                    realClearIconActive = value.realClearIconActive,
                    playerViewport = value.playerViewport,
                    bossViewport = value.bossViewport,
                    bossEnvelopeViewport = value.bossEnvelopeViewport,
                    playerPixelExtent = value.playerPixelExtent,
                    bossPixelExtent = value.bossPixelExtent,
                    bossEnvelopePixelExtent = value.bossEnvelopePixelExtent
                })
                .ToArray();
        }

        private void OnDisable()
        {
            ReleaseBakedCoreMesh();
        }

        private void OnDestroy()
        {
            ReleaseBakedCoreMesh();
        }

        private void LateUpdate()
        {
            if (director == null || Failure != null)
            {
                return;
            }

            int frame = director.LastPresentedFrame;
            if (frame == lastSampledFrame || !EvidenceFrames.Contains(frame))
            {
                return;
            }

            lastSampledFrame = frame;
            try
            {
                evidence.Add(Capture(frame));
            }
            catch (Exception exception)
            {
                Failure = exception;
            }
        }

        private AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            Capture(int frame)
        {
            var result = new
                AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
                {
                    frame = AuditionPvStationBossDeathAftermathGoldenRunner
                        .SourceFrame(frame)
                };

            Camera gameplayCamera = director.GameplayCamera;
            Camera finisherCamera = director.FinisherCamera;
            bool gameplayActive = IsExactCaptureCamera(gameplayCamera);
            bool finisherActive = IsExactCaptureCamera(finisherCamera);
            result.gameplayCameraExact = gameplayActive && !finisherActive;
            result.finisherCameraExact = finisherActive && !gameplayActive;
            result.exclusiveCameraRoleExact = gameplayActive != finisherActive;
            result.cameraRole = result.finisherCameraExact
                ? "finisher"
                : result.gameplayCameraExact ? "gameplay" : "invalid";
            result.finisherLeaseReleased = director.FinisherCameraLeaseReleased;
            result.combatHudVisible = director.CombatHudVisible;
            result.pocketClearMarkerReferenceUnbound =
                director.PocketClearMarkerReferenceUnbound;
            result.pocketClearMarkerPresent = director.PocketClearMarker != null;
            result.pocketClearMarkerInactive = director.PocketClearMarker != null
                && !director.PocketClearMarker.activeSelf
                && !director.PocketClearMarker.activeInHierarchy;
            result.terminalBoundaryVisualPresent =
                director.TerminalBoundaryVisualRoot != null;
            result.terminalBoundaryVisualHidden =
                director.TerminalBoundaryVisualRoot != null
                && !director.TerminalBoundaryVisualRoot.activeSelf
                && !director.TerminalBoundaryVisualRoot.activeInHierarchy;

            if (frame == 246)
            {
                StageClearScreenPresenter presenter = director.ClearPresenter;
                CanvasGroup group = presenter != null
                    ? presenter.GetComponent<CanvasGroup>()
                    : null;
                Graphic[] graphics = presenter != null
                    ? presenter.GetComponentsInChildren<Graphic>(true)
                    : Array.Empty<Graphic>();
                result.resultCanvasVisible = presenter != null
                    && presenter.gameObject.activeInHierarchy
                    && group != null
                    && group.alpha > 0.99f
                    && graphics.Any(graphic => graphic != null
                        && graphic.enabled
                        && graphic.gameObject.activeInHierarchy
                        && graphic.canvasRenderer.GetAlpha() > 0.01f);
                result.resultInteractive = group != null
                    && group.interactable
                    && group.blocksRaycasts
                    && presenter.EntranceCompleted
                    && !presenter.IsEntrancePlaying;
                GameObject redundantClearText = FindNamedGameObject(
                    presenter != null ? presenter.transform : null,
                    AuditionPvStationBossDeathAftermathCapture
                        .RedundantClearTextObjectName);
                GameObject realClearIcon = FindNamedGameObject(
                    presenter != null ? presenter.transform : null,
                    AuditionPvStationBossDeathAftermathCapture
                        .RealClearIconObjectName);
                result.redundantClearTextPresent = redundantClearText != null;
                result.redundantClearTextInactive = redundantClearText != null
                    && !redundantClearText.activeSelf
                    && !redundantClearText.activeInHierarchy;
                result.realClearIconPresent = realClearIcon != null;
                result.realClearIconActive = realClearIcon != null
                    && realClearIcon.activeSelf
                    && realClearIcon.activeInHierarchy;
                return result;
            }

            Camera camera = frame == 61 ? gameplayCamera : finisherCamera;
            if (!IsExactCaptureCamera(camera))
            {
                return result;
            }

            result.projectionAspect = camera.aspect;

            ProjectedBodyEvidence player = ResolveProjectedRendererBounds(
                camera,
                director.PlayerRendererRoot);
            SkinnedMeshRenderer[] coreRenderers =
                director.BossCoreBodyRenderers
                ?? Array.Empty<SkinnedMeshRenderer>();
            ProjectedBodyEvidence boss = ResolveProjectedSkinnedGeometry(
                camera,
                coreRenderers);
            ProjectedBodyEvidence envelope = ResolveProjectedRendererBounds(
                camera,
                director.BossRendererRoot);
            result.playerSafeViewport = player.safeViewport;
            result.bossSafeViewport = boss.safeViewport;
            result.playerFullyInsideFrustum = player.fullyInsideFrustum;
            result.playerFullyOutsideFrustum = player.fullyOutsideFrustum;
            result.playerPartiallyClipped = player.partiallyClipped;
            result.bossFullyInsideFrustum = boss.fullyInsideFrustum;
            result.bossFullyOutsideFrustum = boss.fullyOutsideFrustum;
            result.bossPartiallyClipped = boss.partiallyClipped;
            result.playerBodyHeightRatio = player.bodyHeightRatio;
            result.bossBodyRendererNames = string.Join(
                "|",
                coreRenderers
                    .Where(value => value != null)
                    .Select(value => value.gameObject.name));
            result.bossBodyRendererCount = boss.rendererCount;
            result.bossBodyWidthRatio = boss.bodyWidthRatio;
            result.bossBodyHeightRatio = boss.bodyHeightRatio;
            result.bossBodyMaxExtentRatio = boss.bodyMaxExtentRatio;
            result.bossEnvelopeVisible = envelope.visibleInFrustum;
            result.bossEnvelopeFullyInsideFrustum =
                envelope.fullyInsideFrustum;
            result.bossEnvelopeFullyOutsideFrustum =
                envelope.fullyOutsideFrustum;
            result.bossEnvelopePartiallyClipped = envelope.partiallyClipped;
            result.bossEnvelopeRendererCount = envelope.rendererCount;
            result.bossEnvelopeWidthRatio = envelope.bodyWidthRatio;
            result.bossEnvelopeHeightRatio = envelope.bodyHeightRatio;
            result.bossEnvelopeMaxExtentRatio = envelope.bodyMaxExtentRatio;
            result.playerViewport = player.viewport;
            result.bossViewport = boss.viewport;
            result.bossEnvelopeViewport = envelope.viewport;
            result.playerPixelExtent = player.pixelExtent;
            result.bossPixelExtent = boss.pixelExtent;
            result.bossEnvelopePixelExtent = envelope.pixelExtent;

            Transform hips = director.BossCoreAxisHips;
            Transform head = director.BossCoreAxisHead;
            if (hips != null && head != null && hips != head)
            {
                result.bossCoreAxisSource =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .BossCoreAxisSource;
                result.bossCoreAxisHipsViewport =
                    camera.WorldToViewportPoint(hips.position);
                result.bossCoreAxisHeadViewport =
                    camera.WorldToViewportPoint(head.position);
                result.bossCoreAxisViewportLength = new Vector2(
                    (result.bossCoreAxisHeadViewport.x
                        - result.bossCoreAxisHipsViewport.x) * camera.aspect,
                    result.bossCoreAxisHeadViewport.y
                        - result.bossCoreAxisHipsViewport.y).magnitude;
            }

            if (frame == 61 || frame == 62)
            {
                result.objectiveText = director.ObjectiveTextValue;
                result.bossLabelText = director.BossNameTextValue;
                result.objectiveForbiddenInternalTokensAbsent =
                    !ContainsForbiddenInternalObjectiveToken(result.objectiveText);
            }

            return result;
        }

        private static bool IsExactCaptureCamera(Camera camera)
        {
            return camera != null
                && camera.gameObject.activeInHierarchy
                && camera.isActiveAndEnabled
                && camera.targetTexture == null
                && camera.rect == new Rect(0f, 0f, 1f, 1f)
                && camera.pixelWidth > 0
                && camera.pixelHeight > 0;
        }

        private static ProjectedBodyEvidence ResolveProjectedRendererBounds(
            Camera camera,
            Transform root)
        {
            var result = new ProjectedBodyEvidence();
            if (camera == null || root == null)
            {
                return result;
            }

            Renderer[] activeRenderers = root.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer != null
                    && (renderer is SkinnedMeshRenderer
                        || renderer is MeshRenderer)
                    && renderer.enabled
                    && !renderer.forceRenderingOff
                    && renderer.shadowCastingMode
                        != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly
                    && renderer.gameObject.activeInHierarchy
                    && (camera.cullingMask & (1 << renderer.gameObject.layer)) != 0)
                .ToArray();
            Renderer[] skinnedBodyRenderers = activeRenderers
                .Where(renderer => renderer is SkinnedMeshRenderer)
                .ToArray();
            Renderer[] renderers = skinnedBodyRenderers.Length > 0
                ? skinnedBodyRenderers
                : activeRenderers
                    .Where(renderer => renderer is MeshRenderer
                        && IsExplicitBodyMeshName(renderer.gameObject.name))
                    .ToArray();
            if (renderers.Length == 0)
            {
                return result;
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            bool anyRendererIntersects = renderers.Any(renderer =>
                GeometryUtility.TestPlanesAABB(planes, renderer.bounds));

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            result.viewport = camera.WorldToViewportPoint(bounds.center);
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3[] corners =
            {
                new(min.x, min.y, min.z), new(min.x, min.y, max.z),
                new(min.x, max.y, min.z), new(min.x, max.y, max.z),
                new(max.x, min.y, min.z), new(max.x, min.y, max.z),
                new(max.x, max.y, min.z), new(max.x, max.y, max.z)
            };
            Vector3[] projected = corners.Select(camera.WorldToViewportPoint)
                .ToArray();
            bool[] projectedCornerInside = projected.Select(value => value.z > 0f
                    && value.x >= 0f
                    && value.x <= 1f
                    && value.y >= 0f
                    && value.y <= 1f)
                .ToArray();
            result.fullyInsideFrustum = projectedCornerInside.All(value => value);
            result.fullyOutsideFrustum = !anyRendererIntersects
                && projectedCornerInside.All(value => !value);
            result.partiallyClipped = !result.fullyInsideFrustum
                && !result.fullyOutsideFrustum;

            Vector3[] inFront = projected.Where(value => value.z > 0f).ToArray();
            if (inFront.Length == 0)
            {
                return result;
            }

            float minimumX = inFront.Min(value => value.x);
            float maximumX = inFront.Max(value => value.x);
            float minimumY = inFront.Min(value => value.y);
            float maximumY = inFront.Max(value => value.y);
            result.bodyWidthRatio = Mathf.Max(0f, maximumX - minimumX);
            result.bodyHeightRatio = Mathf.Max(0f, maximumY - minimumY);
            result.bodyMaxExtentRatio = Mathf.Max(
                result.bodyWidthRatio * camera.aspect,
                result.bodyHeightRatio);
            result.pixelExtent = new Vector2(
                result.bodyWidthRatio * camera.pixelWidth,
                result.bodyHeightRatio * camera.pixelHeight);
            result.visibleInFrustum = anyRendererIntersects;
            result.rendererCount = renderers.Length;
            result.safeViewport = result.fullyInsideFrustum
                && result.viewport.z > 0f
                && result.viewport.x >= 0.03f
                && result.viewport.x <= 0.97f
                && result.viewport.y >= 0.03f
                && result.viewport.y <= 0.97f
                && result.pixelExtent.x >= 8f
                && result.pixelExtent.y >= 8f;
            return result;
        }

        private ProjectedBodyEvidence ResolveProjectedSkinnedGeometry(
            Camera camera,
            SkinnedMeshRenderer[] renderers)
        {
            var result = new ProjectedBodyEvidence();
            if (camera == null || renderers == null || renderers.Length == 0)
            {
                return result;
            }

            SkinnedMeshRenderer[] active = renderers
                .Where(renderer => renderer != null
                    && renderer.enabled
                    && !renderer.forceRenderingOff
                    && renderer.shadowCastingMode
                        != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly
                    && renderer.gameObject.activeInHierarchy
                    && (camera.cullingMask & (1 << renderer.gameObject.layer)) != 0)
                .ToArray();
            result.rendererCount = active.Length;
            if (active.Length != renderers.Length)
            {
                return result;
            }

            bakedCoreMesh ??= new Mesh
            {
                name = "G08_TemporaryBossCoreGeometry",
                hideFlags = HideFlags.HideAndDontSave
            };

            bool anyVertex = false;
            bool anyVertexInside = false;
            bool allVerticesInside = true;
            bool anyBoundsIntersects = false;
            float minimumX = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumY = float.NegativeInfinity;
            float depthSum = 0f;
            int inFrontCount = 0;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

            foreach (SkinnedMeshRenderer renderer in active)
            {
                anyBoundsIntersects |= GeometryUtility.TestPlanesAABB(
                    planes,
                    renderer.bounds);
                bakedCoreMesh.Clear(keepVertexLayout: false);
                renderer.BakeMesh(bakedCoreMesh);
                bakedCoreVertices.Clear();
                bakedCoreMesh.GetVertices(bakedCoreVertices);
                Matrix4x4 localToWorld = renderer.localToWorldMatrix;
                foreach (Vector3 localVertex in bakedCoreVertices)
                {
                    anyVertex = true;
                    Vector3 viewport = camera.WorldToViewportPoint(
                        localToWorld.MultiplyPoint3x4(localVertex));
                    bool inside = viewport.z > 0f
                        && viewport.x >= 0f
                        && viewport.x <= 1f
                        && viewport.y >= 0f
                        && viewport.y <= 1f;
                    anyVertexInside |= inside;
                    allVerticesInside &= inside;
                    if (viewport.z <= 0f)
                    {
                        continue;
                    }

                    minimumX = Mathf.Min(minimumX, viewport.x);
                    maximumX = Mathf.Max(maximumX, viewport.x);
                    minimumY = Mathf.Min(minimumY, viewport.y);
                    maximumY = Mathf.Max(maximumY, viewport.y);
                    depthSum += viewport.z;
                    inFrontCount++;
                }
            }

            if (!anyVertex || inFrontCount == 0)
            {
                return result;
            }

            result.fullyInsideFrustum = allVerticesInside;
            result.fullyOutsideFrustum = !anyBoundsIntersects
                && !anyVertexInside;
            result.partiallyClipped = !result.fullyInsideFrustum
                && !result.fullyOutsideFrustum;
            result.visibleInFrustum = anyBoundsIntersects || anyVertexInside;
            result.bodyWidthRatio = Mathf.Max(0f, maximumX - minimumX);
            result.bodyHeightRatio = Mathf.Max(0f, maximumY - minimumY);
            result.bodyMaxExtentRatio = Mathf.Max(
                result.bodyWidthRatio * camera.aspect,
                result.bodyHeightRatio);
            result.viewport = new Vector3(
                (minimumX + maximumX) * 0.5f,
                (minimumY + maximumY) * 0.5f,
                depthSum / inFrontCount);
            result.pixelExtent = new Vector2(
                result.bodyWidthRatio * camera.pixelWidth,
                result.bodyHeightRatio * camera.pixelHeight);
            result.safeViewport = result.fullyInsideFrustum
                && result.viewport.z > 0f
                && minimumX >= 0.03f
                && maximumX <= 0.97f
                && minimumY >= 0.03f
                && maximumY <= 0.97f
                && result.pixelExtent.x >= 8f
                && result.pixelExtent.y >= 8f;
            return result;
        }

        private void ReleaseBakedCoreMesh()
        {
            if (bakedCoreMesh == null)
            {
                return;
            }

            Mesh value = bakedCoreMesh;
            bakedCoreMesh = null;
            bakedCoreVertices.Clear();
            if (Application.isPlaying)
            {
                Destroy(value);
            }
            else
            {
                DestroyImmediate(value);
            }
        }

        private static bool IsExplicitBodyMeshName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return new[] { "Body", "Character", "Model", "Mesh", "Weapon", "Wing" }
                .Any(token => value.IndexOf(
                    token,
                    StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool ContainsForbiddenInternalObjectiveToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return new[]
                {
                    "SummonSlot", "boss curtain", "Build EN", "ARCHON PROXY", "LV."
                }
                .Any(token => value.IndexOf(
                    token,
                    StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static GameObject FindNamedGameObject(Transform root, string objectName)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            Transform[] values = root.GetComponentsInChildren<Transform>(true)
                .Where(value => value != null
                    && string.Equals(value.name, objectName, StringComparison.Ordinal))
                .ToArray();
            return values.Length == 1 ? values[0].gameObject : null;
        }

        private struct ProjectedBodyEvidence
        {
            public bool safeViewport;
            public bool visibleInFrustum;
            public bool fullyInsideFrustum;
            public bool fullyOutsideFrustum;
            public bool partiallyClipped;
            public int rendererCount;
            public float bodyWidthRatio;
            public float bodyHeightRatio;
            public float bodyMaxExtentRatio;
            public Vector3 viewport;
            public Vector2 pixelExtent;
        }
    }
}
