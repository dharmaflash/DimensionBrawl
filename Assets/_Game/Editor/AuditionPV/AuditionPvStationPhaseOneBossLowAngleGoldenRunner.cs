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
    /// Captures one caller-selected S050 rail ordinal per invocation and writes
    /// its immutable source ledgers. Product-state capture remains entirely
    /// inside the director; the non-core Gate contract never requires all three
    /// optional rail ordinals to exist.
    /// </summary>
    [InitializeOnLoad]
    public static class AuditionPvStationPhaseOneBossLowAngleGoldenRunner
    {
        internal const string RunnerScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationPhaseOneBossLowAngleGoldenRunner.cs";
        internal const string CaptureTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvStationPhaseOneBossLowAngleCaptureTests.cs";
        internal const string RunnerTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvStationPhaseOneBossLowAngleGoldenRunnerTests.cs";
        internal const string MenuPath =
            "DimensionBrawl/Audition PV/Capture S050 Station Phase 1 Boss Low Angle/Take 01";
        internal const string MenuPathTake02 =
            "DimensionBrawl/Audition PV/Capture S050 Station Phase 1 Boss Low Angle/Take 02";
        internal const string MenuPathTake03 =
            "DimensionBrawl/Audition PV/Capture S050 Station Phase 1 Boss Low Angle/Take 03";
        internal const string StateFileName = "s050_batch_state.json";
        internal const string RuntimeProofFileName = "s050_runtime_proof.json";
        internal const string DetailedSourceLedgerFileName =
            "s050_source_ledger.json";
        internal const string FrameLedgerFileName =
            "s050_source_frames.sha256";
        internal const string ShotAuthorshipFileName =
            "s050_shot_authorship.json";
        internal const string FailureFileName = "s050_capture_failure.json";
        internal const string EvidenceFolderName = "evidence";
        internal const string BaselinesFolderName = "baselines";
        internal const string PaddingEvidenceFileName =
            "recorder_padding_raw_frame_0000.png";
        internal const int RawPaddingFrame = 0;
        internal const int RawFirstSourceFrame = 1;
        internal const int RawLastSourceFrame = 600;
        internal const int ExpectedRawFrameCount = 601;

        private const string SessionActiveKey =
            "DimensionBrawl.AuditionPV.S050GoldenRunner.Active";
        private const string SessionStatePathKey =
            "DimensionBrawl.AuditionPV.S050GoldenRunner.StatePath";
        private const string SessionBatchKey =
            "DimensionBrawl.AuditionPV.S050GoldenRunner.Batch";
        private const string SessionOwnerKey =
            "DimensionBrawl.AuditionPV.S050GoldenRunner.Owner";
        private const string SessionOwnerValue =
            "dimension-brawl.s050-station-phase1-low-angle.v1";
        private const string StateSchema =
            "dimension-brawl.audition-pv.s050-single-take-state.v1";
        private const string ProofSchema =
            "dimension-brawl.audition-pv.s050-runtime-proof.v1";
        private const string LedgerSchema =
            "dimension-brawl.audition-pv.canonical-source-ledger.v1";
        private const string FailureSchema =
            "dimension-brawl.audition-pv.capture-failure.v1";
        internal const string GateEvidenceTestSuite =
            "AuditionPvSixtySecondEvidence";
        internal const string CombinedSemanticFact =
            "boss-low-angle-silhouette";
        internal static readonly string[] RequiredSemanticBeatFacts =
            { "boss-low-angle", "boss-silhouette" };

        private static bool resumeScheduled;
        private static bool finalizing;
        private static AuditionPvStationPhaseOneBossLowAngleGoldenRunnerBehaviour
            activeBehaviour;

        static AuditionPvStationPhaseOneBossLowAngleGoldenRunner()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            ScheduleResume();
        }

        [MenuItem(MenuPath)]
        public static void CaptureMenu()
        {
            CaptureMenuOrdinal(1);
        }

        [MenuItem(MenuPathTake02)]
        public static void CaptureMenuTake02()
        {
            CaptureMenuOrdinal(2);
        }

        [MenuItem(MenuPathTake03)]
        public static void CaptureMenuTake03()
        {
            CaptureMenuOrdinal(3);
        }

        private static void CaptureMenuOrdinal(int takeOrdinal)
        {
            try
            {
                BeginCapture(batchMode: false, takeOrdinal);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "S050 golden capture did not start",
                    exception.Message,
                    "OK");
            }
        }

        public static void RunBatchCapture()
        {
            try
            {
                ValidateBatchCommandLine(Environment.GetCommandLineArgs());
                BeginCapture(
                    batchMode: true,
                    ResolveRequestedTakeOrdinal(Environment.GetCommandLineArgs()),
                    ResolveApprovedEvidenceRequest(Environment.GetCommandLineArgs()));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        internal static string RawFrameFileName(int rawFrame)
        {
            if (rawFrame < RawPaddingFrame || rawFrame > RawLastSourceFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(rawFrame));
            }

            return $"frame_{rawFrame:0000}.png";
        }

        internal static int RawToSourceFrame(int rawFrame)
        {
            if (rawFrame < RawFirstSourceFrame || rawFrame > RawLastSourceFrame)
            {
                throw new ArgumentOutOfRangeException(nameof(rawFrame));
            }

            return rawFrame - RawFirstSourceFrame;
        }

        internal static void ValidateBatchCommandLine(IEnumerable<string> arguments)
        {
            string[] values = (arguments ?? Array.Empty<string>()).ToArray();
            bool Has(string value) => values.Any(argument =>
                string.Equals(argument, value, StringComparison.OrdinalIgnoreCase));
            if (!Has("-noaudio"))
            {
                throw new InvalidOperationException(
                    "S050 batch capture requires -noaudio.");
            }

            if (Has("-batchmode") || Has("-nographics") || Has("-quit"))
            {
                throw new InvalidOperationException(
                    "S050 GameView capture is asynchronous and headful; remove "
                    + "-batchmode, -nographics, and -quit.");
            }
        }

        internal static int ResolveRequestedTakeOrdinal(
            IEnumerable<string> arguments)
        {
            const string prefix = "-s050TakeOrdinal=";
            string[] matches = (arguments ?? Array.Empty<string>())
                .Where(value => value != null
                    && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length == 0)
            {
                return 1;
            }

            if (matches.Length != 1
                || !int.TryParse(
                    matches[0].Substring(prefix.Length),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int ordinal))
            {
                throw new InvalidOperationException(
                    "S050 accepts at most one -s050TakeOrdinal=1|2|3 argument.");
            }

            AuditionPvStationPhaseOneBossLowAngleCapture.GetRailPreset(ordinal);
            return ordinal;
        }

        internal static bool ResolveApprovedEvidenceRequest(
            IEnumerable<string> arguments) => (arguments ?? Array.Empty<string>()).Any(value =>
            string.Equals(value, "-pv60ApprovedEvidence", StringComparison.OrdinalIgnoreCase));

        internal static void EnsureNoDirtyOpenScenes()
        {
            var dirty = new List<string>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.IsValid() && scene.isLoaded && scene.isDirty)
                {
                    dirty.Add(string.IsNullOrWhiteSpace(scene.path)
                        ? $"<untitled:{scene.name}>"
                        : scene.path);
                }
            }

            if (dirty.Count > 0)
            {
                throw new InvalidOperationException(
                    "S050 refuses to replace dirty scenes: " + string.Join(", ", dirty));
            }
        }

        internal static string[] CollectCaptureDependencyPaths()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                RunnerScriptPath,
                CaptureTestPath,
                RunnerTestPath
            };

            foreach (string explicitPath in
                     AuditionPvStationPhaseOneBossLowAngleCapture
                         .ExplicitDependencyPaths())
            {
                if (AssetDatabase.LoadMainAssetAtPath(explicitPath) == null
                    && !File.Exists(ProjectAbsolutePath(explicitPath)))
                {
                    throw new FileNotFoundException(
                        "S050 dependency is missing.", explicitPath);
                }

                paths.Add(explicitPath.Replace('\\', '/'));
                if (string.Equals(
                        explicitPath,
                        AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        explicitPath,
                        AuditionPvStationPhaseOneBossLowAngleCapture
                            .PhaseOneVisualPrefabPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string dependency in
                             AssetDatabase.GetDependencies(explicitPath, true))
                    {
                        paths.Add(dependency.Replace('\\', '/'));
                    }
                }
            }

            return AuditionPvEnvironmentProbe.CollectCaptureDependencyPaths(paths);
        }

        internal static void ValidateStableDependencies(
            AuditionPvDependencyHash[] before,
            AuditionPvDependencyHash[] after)
        {
            AuditionPvDependencyHash[] initial = before ?? Array.Empty<AuditionPvDependencyHash>();
            AuditionPvDependencyHash[] current = after ?? Array.Empty<AuditionPvDependencyHash>();
            if (initial.Length != current.Length)
            {
                throw new InvalidOperationException(
                    "S050 dependency set changed during capture.");
            }

            var currentByPath = current.ToDictionary(
                entry => entry.path,
                StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvDependencyHash entry in initial)
            {
                if (entry == null
                    || !currentByPath.TryGetValue(entry.path, out AuditionPvDependencyHash found)
                    || entry.exists != found.exists
                    || entry.byteLength != found.byteLength
                    || !string.Equals(entry.sha256, found.sha256, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "S050 dependency changed during capture: "
                        + (entry?.path ?? "<null>"));
                }
            }
        }

        internal static void ValidateStableGitSnapshot(
            AuditionPvGitSnapshot before,
            AuditionPvGitSnapshot after)
        {
            if (before == null
                || after == null
                || !before.probeSucceeded
                || !after.probeSucceeded
                || !string.Equals(before.commitSha, after.commitSha, StringComparison.Ordinal)
                || !string.Equals(before.branch, after.branch, StringComparison.Ordinal)
                || before.isDirty != after.isDirty
                || !string.Equals(
                    before.dirtyStateHashSha256,
                    after.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Git source state changed during the S050 take.");
            }
        }

        internal static void ValidatePngFile(
            string path,
            int expectedWidth = AuditionPvCaptureContract.Width,
            int expectedHeight = AuditionPvCaptureContract.Height)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("S050 PNG is missing.", path);
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length
                    || stream.Read(header, 0, header.Length) != header.Length)
                {
                    throw new InvalidDataException("S050 PNG is truncated: " + path);
                }
            }

            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (!signature.SequenceEqual(header.Take(signature.Length))
                || header[12] != (byte)'I'
                || header[13] != (byte)'H'
                || header[14] != (byte)'D'
                || header[15] != (byte)'R')
            {
                throw new InvalidDataException("S050 PNG signature/IHDR mismatch: " + path);
            }

            int width = ReadBigEndianInt32(header, 16);
            int height = ReadBigEndianInt32(header, 20);
            if (width != expectedWidth || height != expectedHeight)
            {
                throw new InvalidDataException(
                    $"S050 PNG is {width}x{height}; expected "
                    + $"{expectedWidth}x{expectedHeight}: {path}");
            }
        }

        internal static string RemapRawFrames(
            string frameDirectory,
            string evidenceDirectory)
        {
            string frames = RequireDirectory(frameDirectory);
            ValidateExactSequence(frames, ExpectedRawFrameCount, RawFrameFileName);
            string evidence = Path.GetFullPath(evidenceDirectory);
            Directory.CreateDirectory(evidence);
            string staging = Path.Combine(
                Path.GetDirectoryName(frames)
                    ?? throw new InvalidOperationException("Frame directory has no parent."),
                ".s050-remap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            bool completed = false;
            try
            {
                for (int raw = RawFirstSourceFrame; raw <= RawLastSourceFrame; raw++)
                {
                    int source = RawToSourceFrame(raw);
                    MoveNew(
                        Path.Combine(frames, RawFrameFileName(raw)),
                        Path.Combine(
                            staging,
                            AuditionPvStationPhaseOneBossLowAngleCapture
                                .FrameFileName(source)));
                }

                string paddingEvidence = Path.Combine(
                    evidence,
                    PaddingEvidenceFileName);
                MoveNew(
                    Path.Combine(frames, RawFrameFileName(RawPaddingFrame)),
                    paddingEvidence);
                for (int source =
                         AuditionPvStationPhaseOneBossLowAngleCapture.FirstSourceFrame;
                    source <=
                         AuditionPvStationPhaseOneBossLowAngleCapture.LastSourceFrame;
                    source++)
                {
                    string fileName =
                        AuditionPvStationPhaseOneBossLowAngleCapture.FrameFileName(source);
                    MoveNew(
                        Path.Combine(staging, fileName),
                        Path.Combine(frames, fileName));
                }

                Directory.Delete(staging, recursive: false);
                completed = true;
                return paddingEvidence.Replace('\\', '/');
            }
            finally
            {
                if (completed && Directory.Exists(staging))
                {
                    Directory.Delete(staging, recursive: false);
                }
            }
        }

        internal static void ValidateCanonicalFrameSequence(string frameDirectory)
        {
            ValidateExactSequence(
                frameDirectory,
                AuditionPvStationPhaseOneBossLowAngleCapture.SourceFrameCount,
                AuditionPvStationPhaseOneBossLowAngleCapture.FrameFileName);
        }

        internal static SourceLedger CreateSourceLedger(
            TakeState take,
            string frameDirectory,
            string stationSceneSha256,
            string phaseOnePrefabSha256,
            string captureCoreSha256)
        {
            if (take == null)
            {
                throw new ArgumentNullException(nameof(take));
            }

            string frames = RequireDirectory(frameDirectory);
            var entries = new SourceFrameLedgerEntry[
                AuditionPvStationPhaseOneBossLowAngleCapture.SourceFrameCount];
            for (int source = 0; source < entries.Length; source++)
            {
                string fileName =
                    AuditionPvStationPhaseOneBossLowAngleCapture.FrameFileName(source);
                string path = Path.Combine(frames, fileName);
                ValidatePngFile(path);
                var info = new FileInfo(path);
                entries[source] = new SourceFrameLedgerEntry
                {
                    sourceFrame = source,
                    recorderRawFrame = source + RawFirstSourceFrame,
                    selectedLogicalFrame =
                        AuditionPvStationPhaseOneBossLowAngleCapture
                            .SourceToSelectedLogicalFrame(source),
                    role = AuditionPvStationPhaseOneBossLowAngleCapture
                        .SourceFrameRole(source),
                    relativePngPath =
                        $"frames/{AuditionPvStationPhaseOneBossLowAngleCapture.ShotId}/{fileName}",
                    byteLength = info.Length,
                    pngSha256 = AuditionPvSha256.FileHash(path),
                    width = AuditionPvCaptureContract.Width,
                    height = AuditionPvCaptureContract.Height,
                    fps = AuditionPvCaptureContract.Fps
                };
            }

            var ledger = new SourceLedger
            {
                schema = LedgerSchema,
                captureId = take.captureId,
                segmentId = AuditionPvStationPhaseOneBossLowAngleCapture.SegmentId,
                shotId = AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                takeOrdinal = take.takeOrdinal,
                railPresetId = take.railPresetId,
                scenePath = AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                sceneSha256 = stationSceneSha256,
                phaseOneVisualPrefabPath =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabPath,
                phaseOneVisualPrefabGuid =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabGuid,
                phaseOneVisualPrefabSha256 = phaseOnePrefabSha256,
                sourceCaptureCoreSha256 = captureCoreSha256,
                sourceFirstFrame =
                    AuditionPvStationPhaseOneBossLowAngleCapture.FirstSourceFrame,
                sourceLastFrame =
                    AuditionPvStationPhaseOneBossLowAngleCapture.LastSourceFrame,
                sourceFrameCount =
                    AuditionPvStationPhaseOneBossLowAngleCapture.SourceFrameCount,
                selectedFirstSourceFrame =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .SelectedFirstSourceFrame,
                selectedLastSourceFrame =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .SelectedLastSourceFrame,
                selectedFrameCount =
                    AuditionPvStationPhaseOneBossLowAngleCapture.SelectedFrameCount,
                recorderPaddingRawFrame = RawPaddingFrame,
                recorderFirstMappedRawFrame = RawFirstSourceFrame,
                recorderLastMappedRawFrame = RawLastSourceFrame,
                width = AuditionPvCaptureContract.Width,
                height = AuditionPvCaptureContract.Height,
                fps = AuditionPvCaptureContract.Fps,
                sourceFormat = AuditionPvCaptureContract.SourceFormat,
                frames = entries
            };
            ValidateSourceLedger(ledger);
            return ledger;
        }

        internal static void ValidateSourceLedger(SourceLedger ledger)
        {
            if (ledger == null
                || !string.Equals(ledger.schema, LedgerSchema, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(ledger.captureId)
                || !string.Equals(
                    ledger.segmentId,
                    AuditionPvStationPhaseOneBossLowAngleCapture.SegmentId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ledger.shotId,
                    AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                    StringComparison.Ordinal)
                || ledger.takeOrdinal < 1
                || ledger.takeOrdinal
                    > AuditionPvStationPhaseOneBossLowAngleCapture.RailPresetCount
                || !string.Equals(
                    ledger.railPresetId,
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .GetRailPreset(ledger.takeOrdinal).Id,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ledger.scenePath,
                    AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                    StringComparison.Ordinal)
                || !AuditionPvSha256.IsSha256(ledger.sceneSha256)
                || !string.Equals(
                    ledger.phaseOneVisualPrefabPath,
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabPath,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ledger.phaseOneVisualPrefabGuid,
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabGuid,
                    StringComparison.Ordinal)
                || !AuditionPvSha256.IsSha256(ledger.phaseOneVisualPrefabSha256)
                || !AuditionPvSha256.IsSha256(ledger.sourceCaptureCoreSha256)
                || ledger.sourceFirstFrame != 0
                || ledger.sourceLastFrame != 599
                || ledger.sourceFrameCount != 600
                || ledger.selectedFirstSourceFrame != 180
                || ledger.selectedLastSourceFrame != 419
                || ledger.selectedFrameCount != 240
                || ledger.recorderPaddingRawFrame != 0
                || ledger.recorderFirstMappedRawFrame != 1
                || ledger.recorderLastMappedRawFrame != 600
                || ledger.width != AuditionPvCaptureContract.Width
                || ledger.height != AuditionPvCaptureContract.Height
                || ledger.fps != AuditionPvCaptureContract.Fps
                || !string.Equals(
                    ledger.sourceFormat,
                    AuditionPvCaptureContract.SourceFormat,
                    StringComparison.Ordinal)
                || ledger.frames == null
                || ledger.frames.Length != 600)
            {
                throw new InvalidOperationException(
                    "S050 source ledger header is not canonical.");
            }

            for (int source = 0; source < ledger.frames.Length; source++)
            {
                SourceFrameLedgerEntry entry = ledger.frames[source];
                if (entry == null
                    || entry.sourceFrame != source
                    || entry.recorderRawFrame != source + 1
                    || entry.selectedLogicalFrame
                        != AuditionPvStationPhaseOneBossLowAngleCapture
                            .SourceToSelectedLogicalFrame(source)
                    || !string.Equals(
                        entry.role,
                        AuditionPvStationPhaseOneBossLowAngleCapture
                            .SourceFrameRole(source),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        entry.relativePngPath,
                        $"frames/{AuditionPvStationPhaseOneBossLowAngleCapture.ShotId}/"
                            + AuditionPvStationPhaseOneBossLowAngleCapture
                                .FrameFileName(source),
                        StringComparison.Ordinal)
                    || entry.byteLength <= 24
                    || !AuditionPvSha256.IsSha256(entry.pngSha256)
                    || entry.width != AuditionPvCaptureContract.Width
                    || entry.height != AuditionPvCaptureContract.Height
                    || entry.fps != AuditionPvCaptureContract.Fps)
                {
                    throw new InvalidOperationException(
                        $"S050 source ledger frame f{source} is invalid.");
                }
            }
        }

        internal static void ValidateRuntimeProof(RuntimeProof proof, int takeOrdinal)
        {
            string presetId = AuditionPvStationPhaseOneBossLowAngleCapture
                .GetRailPreset(takeOrdinal).Id;
            if (proof == null
                || !proof.freshSceneValidated
                || !proof.directorCompleted
                || proof.takeOrdinal != takeOrdinal
                || !string.Equals(proof.railPresetId, presetId, StringComparison.Ordinal)
                || proof.lastSourceFrame != 599
                || proof.presentedFrameCount != 600
                || proof.selectedPresentedFrameCount != 240
                || !proof.presentedFramesExact
                || !proof.selectedMappingExact
                || !proof.presentationClockExact
                || !proof.hudOffEveryFrame
                || !proof.phaseOneEveryFrame
                || !proof.cameraTakeoverObserved
                || !proof.allSelectedFramesInFront
                || !proof.allSelectedFramesLowAngle
                || !proof.allSelectedFramesInCoverage
                || proof.minimumProjectedHeight
                    < AuditionPvStationPhaseOneBossLowAngleCapture
                        .MinimumSelectedProjectedHeight
                || proof.maximumProjectedHeight
                    > AuditionPvStationPhaseOneBossLowAngleCapture
                        .MaximumSelectedProjectedHeight
                || proof.maximumEyeHeightRatio
                    > AuditionPvStationPhaseOneBossLowAngleCapture
                        .MaximumLowAngleEyeRatio
                || proof.minimumCornerDepth <= 0f
                || !proof.bossFullAliveAndUnchanged
                || !proof.transitionStateUnchanged
                || proof.transitionStartedEventCount != 0
                || proof.transitionCompletedEventCount != 0
                || proof.recorderWarmupEndOfFrameCount != 2
                || !proof.recorderPaddingActiveAtSourceFrameZero
                || !proof.recorderAutoStoppedAfterLastFrame
                || !proof.stateRestored
                || !proof.presentationClockReleased
                || proof.cadenceSuspensionCountAfterRestore != 0)
            {
                throw new InvalidOperationException(
                    "S050 runtime proof does not satisfy the exact fresh-Phase1, "
                    + "600-frame, HUD-off, low-angle, no-transition, and restore contract.");
            }
        }

        private static void BeginCapture(
            bool batchMode,
            int takeOrdinal,
            bool produceApprovedSixtySecondEvidence = false)
        {
            AuditionPvStationPhaseOneBossLowAngleCapture.GetRailPreset(takeOrdinal);
            if (SessionState.GetBool(SessionActiveKey, false)
                || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                throw new InvalidOperationException(
                    "S050 cannot start during another run, Play Mode, compilation, or asset update.");
            }

            EnsureNoDirtyOpenScenes();
            DateTime startedAtUtc = DateTime.UtcNow;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded || git.isDirty)
            {
                throw new InvalidOperationException(
                    !git.probeSucceeded
                        ? "S050 requires a successful Git provenance probe: "
                            + git.probeError
                        : "S050 Gate source capture requires a clean Git worktree.");
            }

            AuditionPvEngineSnapshot engine = AuditionPvEnvironmentProbe.ReadEngineSnapshot();
            if (!string.Equals(
                    engine.recorderPackageVersion,
                    AuditionPvCaptureContract.RecorderPackageVersion,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S050 requires Unity Recorder "
                    + AuditionPvCaptureContract.RecorderPackageVersion + ".");
            }

            string[] dependencyPaths = CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencyHashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            string stationSha = RequireDependencyHash(
                dependencyHashes,
                AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath);
            string prefabSha = RequireDependencyHash(
                dependencyHashes,
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .PhaseOneVisualPrefabPath);
            string actualPrefabGuid = AssetDatabase.AssetPathToGUID(
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .PhaseOneVisualPrefabPath);
            if (!string.Equals(
                    actualPrefabGuid,
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabGuid,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S050 Phase 1 visual prefab GUID changed.");
            }

            var takes = new TakeState[1];
            string statePath = string.Empty;
            try
            {
                string captureId = AuditionPvOutputPaths.CreateOutputId(
                    $"s050-station-phase1-boss-low-angle-t{takeOrdinal:00}",
                    startedAtUtc,
                    git.commitSha,
                    git.isDirty,
                    git.dirtyStateHashSha256);
                string outputDirectory =
                    AuditionPvOutputPaths.CreateUniqueGoldenOutputDirectory(captureId);
                Directory.CreateDirectory(Path.Combine(
                    outputDirectory,
                    BaselinesFolderName));
                takes[0] = new TakeState
                {
                    takeOrdinal = takeOrdinal,
                    railPresetId =
                        AuditionPvStationPhaseOneBossLowAngleCapture
                            .GetRailPreset(takeOrdinal).Id,
                    captureId = new DirectoryInfo(outputDirectory).Name,
                    outputDirectory = outputDirectory,
                    complete = false,
                    runtimeProof = RuntimeProof.Create(takeOrdinal)
                };

                statePath = Path.Combine(takes[0].outputDirectory, StateFileName);
                var state = new PersistedRunnerState
                {
                    schema = StateSchema,
                    phase = RunnerPhase.AwaitingPlayMode.ToString(),
                    batchMode = batchMode,
                    produceApprovedSixtySecondEvidence =
                        produceApprovedSixtySecondEvidence,
                    startedAtUtc = startedAtUtc.ToString("O"),
                    outputRoot = AuditionPvCaptureContract.OutputRoot,
                    currentTakeIndex = 0,
                    gitCommitSha = git.commitSha,
                    gitBranch = git.branch,
                    gitWorktreeDirty = git.isDirty,
                    gitDirtyHashSha256 = git.dirtyStateHashSha256,
                    engine = CopyEngine(engine),
                    dependencyPaths = dependencyPaths,
                    dependencyHashesAtStart = dependencyHashes,
                    stationSceneSha256AtStart = stationSha,
                    phaseOnePrefabSha256AtStart = prefabSha,
                    takes = takes
                };
                SaveState(statePath, state);
                SessionState.SetString(SessionOwnerKey, SessionOwnerValue);
                SessionState.SetString(SessionStatePathKey, statePath);
                SessionState.SetBool(SessionBatchKey, batchMode);
                SessionState.SetBool(SessionActiveKey, true);
                OpenFreshStationScene();
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                string recoveryDirectory = takes.FirstOrDefault(take => take != null)
                    ?.outputDirectory;
                TryWriteFailureArtifact(
                    recoveryDirectory,
                    "begin",
                    exception,
                    null);
                ClearSession();
                throw;
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
                ValidateState(state);
            }
            catch (Exception exception)
            {
                bool batch = SessionState.GetBool(SessionBatchKey, false);
                TryWriteFailureArtifact(
                    Path.GetDirectoryName(statePath),
                    "state-load",
                    exception,
                    null);
                ClearSession();
                Debug.LogException(exception);
                if (batch)
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
                        state.takes[state.currentTakeIndex].runtimeProof,
                        new InvalidOperationException(
                            "A domain reload interrupted the active S050 Recorder take."));
                }

                return;
            }

            if (phase == RunnerPhase.AwaitingPlayMode)
            {
                OpenFreshStationScene();
                EditorApplication.isPlaying = true;
                return;
            }

            if (phase == RunnerPhase.Complete)
            {
                bool batch = state.batchMode;
                ClearSession();
                if (batch)
                {
                    EditorApplication.Exit(0);
                }

                return;
            }

            if (phase == RunnerPhase.Recording)
            {
                state.failure = "Play Mode exited before S050 reported completion.";
                state.phase = RunnerPhase.FailedInPlayMode.ToString();
                SaveState(statePath, state);
            }

            if (phase == RunnerPhase.AwaitingEditMode
                || phase == RunnerPhase.Recording
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
                    AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                    StringComparison.Ordinal))
            {
                NotifyPlayModeFinished(
                    statePath,
                    state,
                    state.takes[state.currentTakeIndex].runtimeProof,
                    new InvalidOperationException(
                        "S050 entered Play Mode without the fresh Station scene."));
                return;
            }

            state.phase = RunnerPhase.Recording.ToString();
            SaveState(statePath, state);
            int ordinal = state.takes[state.currentTakeIndex].takeOrdinal;
            var root = new GameObject($"[AuditionPV_S050_T{ordinal:00}_GoldenRunner]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            activeBehaviour = root.AddComponent<
                AuditionPvStationPhaseOneBossLowAngleGoldenRunnerBehaviour>();
            activeBehaviour.Begin(statePath, state);
        }

        internal static void NotifyPlayModeFinished(
            string statePath,
            PersistedRunnerState state,
            RuntimeProof proof,
            Exception failure)
        {
            activeBehaviour = null;
            TakeState take = state.takes[state.currentTakeIndex];
            take.runtimeProof = proof ?? take.runtimeProof ?? RuntimeProof.Create(take.takeOrdinal);
            state.failure = failure?.ToString() ?? string.Empty;
            state.phase = failure == null
                ? RunnerPhase.AwaitingEditMode.ToString()
                : RunnerPhase.FailedInPlayMode.ToString();
            SaveState(statePath, state);
            EditorApplication.isPlaying = false;
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
            bool complete = false;
            Exception failure = null;
            TakeState take = state.takes[state.currentTakeIndex];
            try
            {
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .ReopenProductSceneAfterPlayMode();
                Scene scene = SceneManager.GetActiveScene();
                if (!scene.IsValid()
                    || !scene.isLoaded
                    || scene.isDirty
                    || !string.Equals(
                        scene.path,
                        AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "S050 did not reopen an unmodified Station scene after Play Mode.");
                }

                if (!string.IsNullOrWhiteSpace(state.failure))
                {
                    throw new InvalidOperationException(
                        $"S050 take {take.takeOrdinal} failed in Play Mode.\n{state.failure}");
                }

                FinalizeSuccessfulTake(state, take);
                take.complete = true;
                state.failure = string.Empty;
                state.phase = RunnerPhase.Complete.ToString();
                SaveState(statePath, state);
                complete = true;
            }
            catch (Exception exception)
            {
                failure = exception;
                TryWriteFailureArtifact(
                    take.outputDirectory,
                    state.phase,
                    exception,
                    take.runtimeProof);
                Debug.LogException(exception);
            }
            finally
            {
                bool batch = state.batchMode;
                finalizing = false;
                ClearSession();
                if (complete)
                {
                    Debug.Log(
                        $"[AuditionPV] S050 take {take.takeOrdinal} passed: "
                        + take.outputDirectory);
                    if (batch)
                    {
                        EditorApplication.Exit(0);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(take.outputDirectory);
                    }
                }
                else if (batch)
                {
                    EditorApplication.Exit(1);
                }
                else if (failure != null)
                {
                    EditorUtility.DisplayDialog(
                        "S050 golden capture failed",
                        failure.Message,
                        "OK");
                }
            }
        }

        private static void FinalizeSuccessfulTake(
            PersistedRunnerState state,
            TakeState take)
        {
            DateTime startedAtUtc = DateTime.Parse(
                state.startedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
            ValidateRuntimeProof(take.runtimeProof, take.takeOrdinal);
            string frameDirectory = Path.Combine(
                take.outputDirectory,
                "frames",
                AuditionPvStationPhaseOneBossLowAngleCapture.ShotId);
            string evidenceDirectory = Path.Combine(
                take.outputDirectory,
                EvidenceFolderName);
            string paddingEvidence = RemapRawFrames(
                frameDirectory,
                evidenceDirectory);
            ValidatePngFile(paddingEvidence);
            ValidateCanonicalFrameSequence(frameDirectory);

            AuditionPvGitSnapshot gitAtEnd = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            ValidateStableGitSnapshot(CreateGitSnapshot(state), gitAtEnd);
            string[] dependencyPathsAtEnd = CollectCaptureDependencyPaths();
            if (!state.dependencyPaths.SequenceEqual(
                    dependencyPathsAtEnd,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "S050 dependency path set changed during capture.");
            }

            AuditionPvDependencyHash[] dependenciesAtEnd =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPathsAtEnd);
            ValidateStableDependencies(state.dependencyHashesAtStart, dependenciesAtEnd);
            string stationSha = RequireDependencyHash(
                dependenciesAtEnd,
                AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath);
            string prefabSha = RequireDependencyHash(
                dependenciesAtEnd,
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .PhaseOneVisualPrefabPath);
            if (!string.Equals(
                    stationSha,
                    state.stationSceneSha256AtStart,
                    StringComparison.Ordinal)
                || !string.Equals(
                    prefabSha,
                    state.phaseOnePrefabSha256AtStart,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S050 Station scene or canonical Phase 1 visual prefab hash changed.");
            }

            AuditionPvCaptureManifest coreManifest = CreateCaptureManifest(
                state,
                take,
                startedAtUtc,
                Array.Empty<AuditionPvTestResult>());
            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator
                    .CaptureCoreSha256(coreManifest);
            if (!AuditionPvSha256.IsSha256(captureCoreSha256))
            {
                throw new InvalidOperationException(
                    "S050 could not compute its immutable capture-core SHA-256.");
            }

            SourceLedger ledger = CreateSourceLedger(
                take,
                frameDirectory,
                stationSha,
                prefabSha,
                captureCoreSha256);
            string detailedLedgerPath = Path.Combine(
                evidenceDirectory,
                DetailedSourceLedgerFileName);
            WriteJsonNew(detailedLedgerPath, ledger);
            string detailedLedgerSha = AuditionPvSha256.FileHash(detailedLedgerPath);
            string frameLedgerPath = Path.Combine(
                evidenceDirectory,
                FrameLedgerFileName);
            WriteFrameLedgerNew(frameLedgerPath, ledger.frames);
            string frameLedgerSha = AuditionPvSha256.FileHash(frameLedgerPath);
            string paddingSha = AuditionPvSha256.FileHash(paddingEvidence);
            string proofPath = Path.Combine(evidenceDirectory, RuntimeProofFileName);
            WriteJsonNew(proofPath, new RuntimeProofArtifact
            {
                schema = ProofSchema,
                captureId = take.captureId,
                takeOrdinal = take.takeOrdinal,
                railPresetId = take.railPresetId,
                mapping =
                    "Recorder raw0 is padding evidence only; raw1..raw600 map "
                    + "one-to-one to canonical source f0..f599; select f180..f419 "
                    + "maps to PV_S050 logical f0..f239.",
                sourceCaptureCoreSha256 = captureCoreSha256,
                detailedSourceLedgerPath = detailedLedgerPath.Replace('\\', '/'),
                detailedSourceLedgerSha256 = detailedLedgerSha,
                sourceFrameLedgerPath = frameLedgerPath.Replace('\\', '/'),
                sourceFrameLedgerSha256 = frameLedgerSha,
                paddingEvidencePath = paddingEvidence,
                paddingEvidenceSha256 = paddingSha,
                stationSceneSha256 = stationSha,
                phaseOneVisualPrefabSha256 = prefabSha,
                semanticFacts = CreateSemanticRuntimeFacts(),
                runtime = take.runtimeProof
            });
            string proofSha = AuditionPvSha256.FileHash(proofPath);

            AuditionPvShotAuthorshipArtifact authorship = CreateShotAuthorship(
                take,
                captureCoreSha256,
                proofPath,
                proofSha,
                DateTime.UtcNow);
            string authorshipPath = Path.Combine(
                evidenceDirectory,
                ShotAuthorshipFileName);
            WriteJsonNew(authorshipPath, authorship);
            string authorshipSha = AuditionPvSha256.FileHash(authorshipPath);
            SemanticArtifactBinding[] semanticArtifacts =
                WriteSemanticBeatArtifacts(
                    take,
                    evidenceDirectory,
                    captureCoreSha256,
                    proofPath,
                    proofSha,
                    frameLedgerPath,
                    frameLedgerSha,
                    startedAtUtc);

            CopyBaselines(take, frameDirectory);
            AuditionPvTestResult[] tests = CreateTestResults(
                take,
                proofPath,
                proofSha,
                authorshipPath,
                authorshipSha,
                detailedLedgerPath,
                detailedLedgerSha,
                frameLedgerPath,
                frameLedgerSha,
                captureCoreSha256,
                semanticArtifacts,
                startedAtUtc);
            if (state.produceApprovedSixtySecondEvidence)
            {
                AuditionPvSixtySecondEvidenceBundle evidence =
                    AuditionPvSixtySecondEvidenceProducer.Produce(
                        new AuditionPvSixtySecondEvidenceRequest
                        {
                            captureCoreManifest = coreManifest,
                            expectedCaptureCoreSha256 = captureCoreSha256,
                            sourceShotId = AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                            sourceRangeStartFrame =
                                AuditionPvStationPhaseOneBossLowAngleCapture.FirstSourceFrame,
                            sourceRangeEndFrame =
                                AuditionPvStationPhaseOneBossLowAngleCapture.LastSourceFrame,
                            selectStartFrame =
                                AuditionPvStationPhaseOneBossLowAngleCapture.SelectedFirstSourceFrame,
                            selectEndFrame =
                                AuditionPvStationPhaseOneBossLowAngleCapture.SelectedLastSourceFrame,
                            runtimeWorkloadSealPath = take.runtimeWorkloadSealPath,
                            graphicsRootDirectory =
                                AuditionPvSixtySecondGateManifestValidator.ProductionGraphicsRoot,
                            reviewRootDirectory =
                                AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot,
                            approvedSourceRange = true,
                            cleanPlate = false,
                            linkedCleanPlateConfirmed = false
                        });
                tests = AuditionPvSixtySecondEvidenceProducer.MergeCaptureTestResults(
                    tests,
                    evidence);
            }
            AuditionPvCaptureManifest manifest = CreateCaptureManifest(
                state,
                take,
                startedAtUtc,
                tests);
            if (!string.Equals(
                    AuditionPvSixtySecondGateManifestValidator
                        .CaptureCoreSha256(manifest),
                    captureCoreSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S050 capture core changed while binding Gate result artifacts.");
            }

            AuditionPvCaptureManifestWriter.WriteNew(manifest);
        }

        private static AuditionPvCaptureManifest CreateCaptureManifest(
            PersistedRunnerState state,
            TakeState take,
            DateTime startedAtUtc,
            IEnumerable<AuditionPvTestResult> tests)
        {
            return AuditionPvCaptureManifestFactory.CreateForRoot(
                take.captureId,
                state.outputRoot,
                take.outputDirectory,
                new[]
                {
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .CreateShotManifestEntry(take.takeOrdinal)
                },
                AuditionPvStationPhaseOneBossLowAngleCapture
                    .CreateBaselineManifestEntries(take.takeOrdinal),
                tests,
                createdAtUtc: startedAtUtc,
                gitSnapshot: CreateGitSnapshot(state),
                engineSnapshot: RestoreEngine(state.engine),
                dependencyHashSnapshot: state.dependencyHashesAtStart);
        }

        internal static AuditionPvShotAuthorshipArtifact CreateShotAuthorship(
            TakeState take,
            string captureCoreSha256,
            string runtimeProofPath,
            string runtimeProofSha256,
            DateTime createdAtUtc)
        {
            if (take == null
                || !AuditionPvSha256.IsSha256(captureCoreSha256)
                || string.IsNullOrWhiteSpace(runtimeProofPath)
                || !AuditionPvSha256.IsSha256(runtimeProofSha256))
            {
                throw new ArgumentException(
                    "S050 shot-authorship inputs are incomplete.");
            }

            return new AuditionPvShotAuthorshipArtifact
            {
                schemaVersion =
                    AuditionPvSixtySecondGateManifestValidator
                        .ShotAuthorshipSchema,
                sourceCaptureCoreSha256 = captureCoreSha256,
                captureId = take.captureId,
                sourceShotId =
                    AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                cameraId = AuditionPvStationPhaseOneBossLowAngleCapture
                    .CameraId(take.takeOrdinal),
                gameplayState =
                    AuditionPvStationPhaseOneBossLowAngleCapture.GameplayState,
                deterministicSeed =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .DeterministicSeed(take.takeOrdinal),
                timelineId =
                    AuditionPvStationPhaseOneBossLowAngleCapture.TimelineId,
                runtimeProof = new AuditionPvPinnedArtifact
                {
                    path = Path.GetFullPath(runtimeProofPath).Replace('\\', '/'),
                    sha256 = runtimeProofSha256
                },
                tool = nameof(
                    AuditionPvStationPhaseOneBossLowAngleGoldenRunner),
                toolVersion = "1",
                createdAtUtc = createdAtUtc.ToUniversalTime().ToString("O")
            };
        }

        private static SemanticRuntimeFact[] CreateSemanticRuntimeFacts()
        {
            return new[]
            {
                new SemanticRuntimeFact
                {
                    factKey = CombinedSemanticFact,
                    passed = true,
                    details =
                        "Fresh full-health Station Phase 1 boss is held in a HUD-off "
                        + "low-angle silhouette rail for selected source f180..f419."
                },
                new SemanticRuntimeFact
                {
                    factKey = RequiredSemanticBeatFacts[0],
                    passed = true,
                    details =
                        "Every selected frame satisfies the bounded low-angle eye "
                        + "height, upward pitch, and projected boss coverage contract."
                },
                new SemanticRuntimeFact
                {
                    factKey = RequiredSemanticBeatFacts[1],
                    passed = true,
                    details =
                        "Every selected frame retains the canonical Phase 1 visual, "
                        + "all bounds corners in front, and HUD fully suppressed."
                }
            };
        }

        private static SemanticArtifactBinding[] WriteSemanticBeatArtifacts(
            TakeState take,
            string evidenceDirectory,
            string captureCoreSha256,
            string runtimeProofPath,
            string runtimeProofSha256,
            string frameLedgerPath,
            string frameLedgerSha256,
            DateTime createdAtUtc)
        {
            string semanticDirectory = Path.Combine(
                evidenceDirectory,
                "semantic_beats");
            Directory.CreateDirectory(semanticDirectory);
            var runtimePin = new AuditionPvPinnedArtifact
            {
                path = Path.GetFullPath(runtimeProofPath).Replace('\\', '/'),
                sha256 = runtimeProofSha256
            };
            var ledgerPin = new AuditionPvPinnedArtifact
            {
                path = Path.GetFullPath(frameLedgerPath).Replace('\\', '/'),
                sha256 = frameLedgerSha256
            };
            RuntimeProof proof = take.runtimeProof;
            string[] facts = RequiredSemanticBeatFacts
                .Concat(new[] { CombinedSemanticFact })
                .ToArray();
            var bindings = new SemanticArtifactBinding[facts.Length];
            for (int index = 0; index < facts.Length; index++)
            {
                string fact = facts[index];
                string path = Path.Combine(semanticDirectory, fact + ".json");
                var artifact = new GateSemanticBeatRuntimeArtifact
                {
                    schemaVersion =
                        "dimension-brawl.audition-pv.s050-semantic-beat-runtime.v1",
                    sourceCaptureCoreSha256 = captureCoreSha256,
                    captureId = take.captureId,
                    sourceShotId =
                        AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                    beatId = fact,
                    runtimeFactKey = fact,
                    sourceRangeStartFrame = 0,
                    sourceRangeEndFrame = 599,
                    selectStartFrame = 180,
                    selectEndFrame = 419,
                    logicalFactStartFrame = 0,
                    logicalFactEndFrame = 239,
                    sourceFactStartFrame = 180,
                    sourceFactEndFrame = 419,
                    exactFacts = SemanticExactFacts(fact, proof),
                    runtimeProof = runtimePin,
                    sourceFrameLedger = ledgerPin,
                    producer = nameof(
                        AuditionPvStationPhaseOneBossLowAngleGoldenRunner),
                    createdAtUtc = createdAtUtc.ToUniversalTime().ToString("O")
                };
                WriteJsonNew(path, artifact);
                bindings[index] = new SemanticArtifactBinding
                {
                    factKey = fact,
                    path = Path.GetFullPath(path).Replace('\\', '/'),
                    sha256 = AuditionPvSha256.FileHash(path)
                };
            }

            return bindings;
        }

        private static string[] SemanticExactFacts(
            string fact,
            RuntimeProof proof)
        {
            if (string.Equals(
                    fact,
                    RequiredSemanticBeatFacts[0],
                    StringComparison.Ordinal))
            {
                return new[]
                {
                    "selected-source-range=180..419",
                    "selected-logical-range=0..239",
                    "camera-eye-ratio-max="
                        + proof.maximumEyeHeightRatio.ToString(
                            "F6", CultureInfo.InvariantCulture),
                    "boss-projected-height-min="
                        + proof.minimumProjectedHeight.ToString(
                            "F6", CultureInfo.InvariantCulture),
                    "boss-projected-height-max="
                        + proof.maximumProjectedHeight.ToString(
                            "F6", CultureInfo.InvariantCulture),
                    "upward-pitch-every-selected-frame=true"
                };
            }

            if (string.Equals(
                    fact,
                    RequiredSemanticBeatFacts[1],
                    StringComparison.Ordinal))
            {
                return new[]
                {
                    "canonical-phase1-visual-every-frame=true",
                    "hud-off-every-frame=true",
                    "all-boss-bounds-corners-in-front=true",
                    "boss-full-alive-unchanged=true",
                    "transition-events=0"
                };
            }

            return new[]
            {
                "required-beats=boss-low-angle,boss-silhouette",
                "source=station-phase1",
                "hud-mode=hud-off",
                "capture-only-camera-takeover=true",
                "select=180..419"
            };
        }

        private static void CopyBaselines(TakeState take, string frameDirectory)
        {
            string destinationDirectory = Path.Combine(
                take.outputDirectory,
                BaselinesFolderName);
            Directory.CreateDirectory(destinationDirectory);
            foreach (AuditionPvBaselineManifestEntry baseline in
                     AuditionPvStationPhaseOneBossLowAngleCapture
                         .CreateBaselineManifestEntries(take.takeOrdinal))
            {
                string source = Path.Combine(
                    frameDirectory,
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .FrameFileName(baseline.sourceFrame));
                string destination = Path.Combine(
                    destinationDirectory,
                    baseline.fileName);
                if (File.Exists(destination))
                {
                    throw new IOException(
                        "S050 baseline will not overwrite an existing file: "
                        + destination);
                }

                File.Copy(source, destination, overwrite: false);
                ValidatePngFile(destination);
            }
        }

        internal static AuditionPvTestResult[] CreateTestResults(
            TakeState take,
            string proofPath,
            string proofSha256,
            string authorshipPath,
            string authorshipSha256,
            string detailedLedgerPath,
            string detailedLedgerSha256,
            string frameLedgerPath,
            string frameLedgerSha256,
            string captureCoreSha256,
            IEnumerable<SemanticArtifactBinding> semanticArtifacts,
            DateTime startedAtUtc)
        {
            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            RuntimeProof proof = take.runtimeProof;
            var results = new List<AuditionPvTestResult>
            {
                Passed(
                    "runtime",
                    "fresh-phase1-no-transition-or-hp-change",
                    $"take={take.takeOrdinal}; preset={take.railPresetId}; "
                        + "600 exact source frames; boss full/alive/unchanged; transitions=0.",
                    proofPath,
                    duration),
                Passed(
                    "composition",
                    "hud-off-low-angle-selected-range",
                    $"select=180..419; projected={proof.minimumProjectedHeight:F4}.."
                        + $"{proof.maximumProjectedHeight:F4}; eyeRatio<="
                        + $"{proof.maximumEyeHeightRatio:F4}; all corners in front.",
                    proofPath,
                    duration),
                Passed(
                    "source",
                    "physical-qhd60-png-hash-ledger",
                    "600 physical 2560x1440 PNGs, each with SHA-256 and exact raw/source/logical mapping. "
                        + $"artifact-sha256={detailedLedgerSha256} "
                        + $"capture-core-sha256={captureCoreSha256}",
                    detailedLedgerPath,
                    duration),
                Passed(
                    "provenance",
                    "station-scene-and-phase1-prefab-stable",
                    "Station scene, Phase 1 prefab, dependency set, and Git snapshot remained byte-stable. "
                        + $"artifact-sha256={detailedLedgerSha256} "
                        + $"capture-core-sha256={captureCoreSha256}",
                    detailedLedgerPath,
                    duration),
                Passed(
                    "lifecycle",
                    "capture-only-state-restored",
                    "HUD, gameplay camera/controller, cinematic cameras, input, cadence, movement, clock, and frame rate restored.",
                    proofPath,
                    duration)
            };

            results.Add(Passed(
                GateEvidenceTestSuite,
                "shot-authorship/"
                    + AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                $"artifact-sha256={authorshipSha256} "
                    + $"capture-core-sha256={captureCoreSha256}",
                authorshipPath,
                duration));
            results.Add(Passed(
                GateEvidenceTestSuite,
                "shot-authorship-runtime/"
                    + AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                $"artifact-sha256={proofSha256} "
                    + $"capture-core-sha256={captureCoreSha256}",
                proofPath,
                duration));
            results.Add(Passed(
                GateEvidenceTestSuite,
                "source-frame-ledger/"
                    + AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                $"artifact-sha256={frameLedgerSha256} "
                    + $"capture-core-sha256={captureCoreSha256}",
                frameLedgerPath,
                duration));
            results.Add(Passed(
                GateEvidenceTestSuite,
                "capture-core/"
                    + AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                $"artifact-sha256={authorshipSha256} "
                    + $"capture-core-sha256={captureCoreSha256}",
                authorshipPath,
                duration));
            SemanticArtifactBinding[] semantic =
                (semanticArtifacts ?? Array.Empty<SemanticArtifactBinding>())
                .ToArray();
            string[] expectedFacts = RequiredSemanticBeatFacts
                .Concat(new[] { CombinedSemanticFact })
                .ToArray();
            if (!semantic.Select(value => value?.factKey)
                    .SequenceEqual(expectedFacts, StringComparer.Ordinal)
                || semantic.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.path)
                    || !AuditionPvSha256.IsSha256(value.sha256)))
            {
                throw new InvalidOperationException(
                    "S050 semantic artifact bindings are incomplete or reordered.");
            }

            foreach (SemanticArtifactBinding binding in semantic)
            {
                results.Add(Passed(
                    GateEvidenceTestSuite,
                    "semantic-beat/" + binding.factKey,
                    $"artifact-sha256={binding.sha256} "
                        + $"semantic-fact={binding.factKey} "
                        + $"capture-core-sha256={captureCoreSha256}",
                    binding.path,
                    duration));
            }

            return results.ToArray();
        }

        private static AuditionPvTestResult Passed(
            string suite,
            string name,
            string details,
            string artifactPath,
            long duration)
        {
            return new AuditionPvTestResult
            {
                suite = suite,
                name = name,
                status = "passed",
                durationMilliseconds = duration,
                details = details,
                artifactPath = artifactPath.Replace('\\', '/')
            };
        }

        private static void OpenFreshStationScene()
        {
            EditorSceneManager.OpenScene(
                AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                OpenSceneMode.Single);
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || scene.isDirty)
            {
                throw new InvalidOperationException(
                    "S050 could not open a clean fresh Station scene.");
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
            SessionState.EraseBool(SessionActiveKey);
            SessionState.EraseString(SessionStatePathKey);
            SessionState.EraseBool(SessionBatchKey);
            SessionState.EraseString(SessionOwnerKey);
            activeBehaviour = null;
        }

        private static RunnerPhase ParsePhase(string value)
        {
            if (!Enum.TryParse(value, ignoreCase: false, out RunnerPhase phase))
            {
                throw new InvalidDataException("Unknown S050 runner phase: " + value);
            }

            return phase;
        }

        private static void ValidateState(PersistedRunnerState state)
        {
            if (state == null
                || !string.Equals(state.schema, StateSchema, StringComparison.Ordinal)
                || state.takes == null
                || state.takes.Length != 1
                || state.currentTakeIndex != 0
                || state.engine == null
                || state.dependencyPaths == null
                || state.dependencyHashesAtStart == null
                || !AuditionPvSha256.IsSha256(state.stationSceneSha256AtStart)
                || !AuditionPvSha256.IsSha256(state.phaseOnePrefabSha256AtStart))
            {
                throw new InvalidDataException("S050 persisted state is invalid.");
            }

            TakeState take = state.takes[0];
            if (take == null
                || take.takeOrdinal < 1
                || take.takeOrdinal
                    > AuditionPvStationPhaseOneBossLowAngleCapture.RailPresetCount
                || !string.Equals(
                    take.railPresetId,
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .GetRailPreset(take.takeOrdinal).Id,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(take.captureId)
                || string.IsNullOrWhiteSpace(take.outputDirectory)
                || !string.Equals(
                    AuditionPvOutputPaths.ResolveOutputDirectory(
                        state.outputRoot,
                        take.captureId).TrimEnd('/'),
                    Path.GetFullPath(take.outputDirectory)
                        .Replace('\\', '/')
                        .TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "S050 persisted take identity/path is invalid.");
            }

            ParsePhase(state.phase);
        }

        private static string RequireDependencyHash(
            IEnumerable<AuditionPvDependencyHash> dependencies,
            string path)
        {
            AuditionPvDependencyHash dependency = dependencies?.FirstOrDefault(entry =>
                entry != null
                && string.Equals(entry.path, path, StringComparison.OrdinalIgnoreCase));
            if (dependency == null
                || !dependency.exists
                || !AuditionPvSha256.IsSha256(dependency.sha256))
            {
                throw new InvalidOperationException(
                    "S050 dependency lacks a physical SHA-256: " + path);
            }

            return dependency.sha256;
        }

        private static string ProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root is missing.");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }

        private static string RequireDirectory(string path)
        {
            string normalized = Path.GetFullPath(path);
            if (!Directory.Exists(normalized))
            {
                throw new DirectoryNotFoundException(normalized);
            }

            return normalized;
        }

        private static void ValidateExactSequence(
            string directory,
            int expectedCount,
            Func<int, string> expectedName)
        {
            string normalized = RequireDirectory(directory);
            string[] actual = Directory.EnumerateFiles(normalized, "*.png")
                .Select(Path.GetFileName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] expected = Enumerable.Range(0, expectedCount)
                .Select(expectedName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    $"S050 frame sequence contains {actual.Length} PNGs; expected "
                    + $"the exact {expectedCount}-file canonical name set.");
            }
        }

        private static void MoveNew(string source, string destination)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("S050 source frame is missing.", source);
            }

            if (File.Exists(destination))
            {
                throw new IOException(
                    "S050 remap refuses to overwrite: " + destination);
            }

            File.Move(source, destination);
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset)
        {
            return bytes[offset] << 24
                | bytes[offset + 1] << 16
                | bytes[offset + 2] << 8
                | bytes[offset + 3];
        }

        private static void SaveState(string path, PersistedRunnerState state)
        {
            string normalized = Path.GetFullPath(path);
            string parent = Path.GetDirectoryName(normalized)
                ?? throw new InvalidOperationException("S050 state path has no parent.");
            Directory.CreateDirectory(parent);
            string temporary = normalized + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(
                    temporary,
                    JsonUtility.ToJson(state, true) + Environment.NewLine,
                    new UTF8Encoding(false));
                if (File.Exists(normalized))
                {
                    File.Replace(temporary, normalized, null);
                }
                else
                {
                    File.Move(temporary, normalized);
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
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException("S050 persisted state is missing.", path);
            }

            return JsonUtility.FromJson<PersistedRunnerState>(File.ReadAllText(path))
                ?? throw new InvalidDataException("S050 persisted state JSON is empty.");
        }

        private static void WriteJsonNew<T>(string path, T value)
        {
            string normalized = Path.GetFullPath(path);
            string parent = Path.GetDirectoryName(normalized)
                ?? throw new InvalidOperationException("Artifact path has no parent.");
            Directory.CreateDirectory(parent);
            using var stream = new FileStream(
                normalized,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(JsonUtility.ToJson(value, true));
            writer.WriteLine();
        }

        private static void WriteFrameLedgerNew(
            string path,
            IEnumerable<SourceFrameLedgerEntry> frames)
        {
            SourceFrameLedgerEntry[] entries =
                (frames ?? Array.Empty<SourceFrameLedgerEntry>()).ToArray();
            if (entries.Length
                != AuditionPvStationPhaseOneBossLowAngleCapture.SourceFrameCount)
            {
                throw new InvalidOperationException(
                    "S050 Gate frame ledger requires exactly 600 entries.");
            }

            string normalized = Path.GetFullPath(path);
            string parent = Path.GetDirectoryName(normalized)
                ?? throw new InvalidOperationException(
                    "Frame-ledger path has no parent.");
            Directory.CreateDirectory(parent);
            using var stream = new FileStream(
                normalized,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            for (int sourceFrame = 0; sourceFrame < entries.Length; sourceFrame++)
            {
                SourceFrameLedgerEntry entry = entries[sourceFrame];
                if (entry == null
                    || entry.sourceFrame != sourceFrame
                    || !AuditionPvSha256.IsSha256(entry.pngSha256)
                    || string.IsNullOrWhiteSpace(entry.relativePngPath))
                {
                    throw new InvalidOperationException(
                        $"S050 Gate frame ledger entry f{sourceFrame} is invalid.");
                }

                writer.Write(entry.pngSha256);
                writer.Write("  ");
                writer.Write(entry.relativePngPath.Replace('\\', '/'));
                writer.WriteLine();
            }
        }

        private static void TryWriteFailureArtifact(
            string outputDirectory,
            string phase,
            Exception exception,
            RuntimeProof proof)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            try
            {
                string evidence = Path.Combine(outputDirectory, EvidenceFolderName);
                Directory.CreateDirectory(evidence);
                string path = Path.Combine(evidence, FailureFileName);
                if (File.Exists(path))
                {
                    path = Path.Combine(
                        evidence,
                        Path.GetFileNameWithoutExtension(FailureFileName)
                            + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
                            + ".json");
                }

                WriteJsonNew(path, new FailureArtifact
                {
                    schema = FailureSchema,
                    createdAtUtc = DateTime.UtcNow.ToString("O"),
                    phase = phase ?? string.Empty,
                    exception = exception?.ToString() ?? string.Empty,
                    runtime = proof
                });
            }
            catch (Exception artifactFailure)
            {
                Debug.LogException(artifactFailure);
            }
        }

        private static EngineState CopyEngine(AuditionPvEngineSnapshot value)
        {
            return new EngineState
            {
                unityVersion = value.unityVersion,
                unityVersionWithRevision = value.unityVersionWithRevision,
                recorderPackageVersion = value.recorderPackageVersion,
                urpPackageVersion = value.urpPackageVersion,
                activeRenderPipelineAssetPath = value.activeRenderPipelineAssetPath
            };
        }

        private static AuditionPvEngineSnapshot RestoreEngine(EngineState value)
        {
            return new AuditionPvEngineSnapshot
            {
                unityVersion = value.unityVersion,
                unityVersionWithRevision = value.unityVersionWithRevision,
                recorderPackageVersion = value.recorderPackageVersion,
                urpPackageVersion = value.urpPackageVersion,
                activeRenderPipelineAssetPath = value.activeRenderPipelineAssetPath
            };
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

        private enum RunnerPhase
        {
            AwaitingPlayMode,
            Recording,
            AwaitingEditMode,
            FailedInPlayMode,
            Complete
        }

        [Serializable]
        internal sealed class PersistedRunnerState
        {
            public string schema = string.Empty;
            public string phase = string.Empty;
            public bool batchMode;
            public bool produceApprovedSixtySecondEvidence;
            public string startedAtUtc = string.Empty;
            public string outputRoot = string.Empty;
            public int currentTakeIndex;
            public string gitCommitSha = string.Empty;
            public string gitBranch = string.Empty;
            public bool gitWorktreeDirty;
            public string gitDirtyHashSha256 = string.Empty;
            public EngineState engine;
            public string[] dependencyPaths = Array.Empty<string>();
            public AuditionPvDependencyHash[] dependencyHashesAtStart =
                Array.Empty<AuditionPvDependencyHash>();
            public string stationSceneSha256AtStart = string.Empty;
            public string phaseOnePrefabSha256AtStart = string.Empty;
            public TakeState[] takes = Array.Empty<TakeState>();
            public string failure = string.Empty;
        }

        [Serializable]
        internal sealed class EngineState
        {
            public string unityVersion = string.Empty;
            public string unityVersionWithRevision = string.Empty;
            public string recorderPackageVersion = string.Empty;
            public string urpPackageVersion = string.Empty;
            public string activeRenderPipelineAssetPath = string.Empty;
        }

        [Serializable]
        internal sealed class TakeState
        {
            public int takeOrdinal;
            public string railPresetId = string.Empty;
            public string captureId = string.Empty;
            public string outputDirectory = string.Empty;
            public bool complete;
            public string runtimeWorkloadSealPath = string.Empty;
            public RuntimeProof runtimeProof;
        }

        [Serializable]
        internal sealed class RuntimeProof
        {
            public bool freshSceneValidated;
            public bool directorCompleted;
            public int takeOrdinal;
            public string railPresetId = string.Empty;
            public int lastSourceFrame = -1;
            public int presentedFrameCount;
            public int selectedPresentedFrameCount;
            public bool presentedFramesExact;
            public bool selectedMappingExact;
            public bool presentationClockExact;
            public bool hudOffEveryFrame;
            public bool phaseOneEveryFrame;
            public bool cameraTakeoverObserved;
            public bool allSelectedFramesInFront;
            public bool allSelectedFramesLowAngle;
            public bool allSelectedFramesInCoverage;
            public float minimumProjectedHeight;
            public float maximumProjectedHeight;
            public float maximumEyeHeightRatio;
            public float minimumCornerDepth;
            public bool bossFullAliveAndUnchanged;
            public bool transitionStateUnchanged;
            public int transitionStartedEventCount;
            public int transitionCompletedEventCount;
            public int recorderWarmupEndOfFrameCount;
            public float recorderCaptureDeltaTimeAtSourceFrameZero;
            public bool recorderPaddingActiveAtSourceFrameZero;
            public bool recorderAutoStoppedAfterLastFrame;
            public bool stateRestored;
            public bool presentationClockReleased;
            public int cadenceSuspensionCountAfterRestore;

            internal static RuntimeProof Create(int takeOrdinal)
            {
                return new RuntimeProof
                {
                    takeOrdinal = takeOrdinal,
                    railPresetId =
                        AuditionPvStationPhaseOneBossLowAngleCapture
                            .GetRailPreset(takeOrdinal).Id,
                    presentedFramesExact = true,
                    selectedMappingExact = true,
                    presentationClockExact = true
                };
            }
        }

        [Serializable]
        internal sealed class SourceLedger
        {
            public string schema = string.Empty;
            public string captureId = string.Empty;
            public string segmentId = string.Empty;
            public string shotId = string.Empty;
            public int takeOrdinal;
            public string railPresetId = string.Empty;
            public string scenePath = string.Empty;
            public string sceneSha256 = string.Empty;
            public string phaseOneVisualPrefabPath = string.Empty;
            public string phaseOneVisualPrefabGuid = string.Empty;
            public string phaseOneVisualPrefabSha256 = string.Empty;
            public string sourceCaptureCoreSha256 = string.Empty;
            public int sourceFirstFrame;
            public int sourceLastFrame;
            public int sourceFrameCount;
            public int selectedFirstSourceFrame;
            public int selectedLastSourceFrame;
            public int selectedFrameCount;
            public int recorderPaddingRawFrame;
            public int recorderFirstMappedRawFrame;
            public int recorderLastMappedRawFrame;
            public int width;
            public int height;
            public int fps;
            public string sourceFormat = string.Empty;
            public SourceFrameLedgerEntry[] frames =
                Array.Empty<SourceFrameLedgerEntry>();
        }

        [Serializable]
        internal sealed class SourceFrameLedgerEntry
        {
            public int sourceFrame;
            public int recorderRawFrame;
            public int selectedLogicalFrame;
            public string role = string.Empty;
            public string relativePngPath = string.Empty;
            public long byteLength;
            public string pngSha256 = string.Empty;
            public int width;
            public int height;
            public int fps;
        }

        [Serializable]
        internal sealed class SemanticArtifactBinding
        {
            public string factKey = string.Empty;
            public string path = string.Empty;
            public string sha256 = string.Empty;
        }

        [Serializable]
        private sealed class GateSemanticBeatRuntimeArtifact
            : AuditionPvRangeBoundArtifact
        {
            public string schemaVersion = string.Empty;
            public string sourceCaptureCoreSha256 = string.Empty;
            public string captureId = string.Empty;
            public string sourceShotId = string.Empty;
            public string beatId = string.Empty;
            public string runtimeFactKey = string.Empty;
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
            public int takeOrdinal;
            public string railPresetId = string.Empty;
            public string mapping = string.Empty;
            public string sourceCaptureCoreSha256 = string.Empty;
            public string detailedSourceLedgerPath = string.Empty;
            public string detailedSourceLedgerSha256 = string.Empty;
            public string sourceFrameLedgerPath = string.Empty;
            public string sourceFrameLedgerSha256 = string.Empty;
            public string paddingEvidencePath = string.Empty;
            public string paddingEvidenceSha256 = string.Empty;
            public string stationSceneSha256 = string.Empty;
            public string phaseOneVisualPrefabSha256 = string.Empty;
            public SemanticRuntimeFact[] semanticFacts =
                Array.Empty<SemanticRuntimeFact>();
            public RuntimeProof runtime;
        }

        [Serializable]
        private sealed class SemanticRuntimeFact
        {
            public string factKey = string.Empty;
            public bool passed;
            public string details = string.Empty;
        }

        [Serializable]
        private sealed class FailureArtifact
        {
            public string schema = string.Empty;
            public string createdAtUtc = string.Empty;
            public string phase = string.Empty;
            public string exception = string.Empty;
            public RuntimeProof runtime;
        }
    }

    /// <summary>
    /// PlayMode bridge that arms logical source f0 after Recorder's one physical
    /// padding frame and always unwinds the director from failure/disable paths.
    /// </summary>
    [DefaultExecutionOrder(-32500)]
    public sealed class AuditionPvStationPhaseOneBossLowAngleGoldenRunnerBehaviour
        : MonoBehaviour
    {
        private const double ShotTimeoutSeconds = 300d;

        private string statePath;
        private AuditionPvStationPhaseOneBossLowAngleGoldenRunner
            .PersistedRunnerState state;
        private AuditionPvStationPhaseOneBossLowAngleGoldenRunner.TakeState take;
        private AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RuntimeProof proof;
        private AuditionPvStationPhaseOneBossLowAngleDirector director;
        private AuditionPvRecorderSettingsBundle recorderSettings;
        private RecorderController recorderController;
        private AuditionPvRuntimeWorkloadCaptureSession runtimeWorkloadCapture;
        private Exception updateFailure;
        private bool armSourceFrameZero;
        private bool beganShot;
        private bool cleaningUp;
        private bool notified;
        private int nextSourceFrame;
        private int nextSelectedLogicalFrame;

        internal void Begin(
            string persistedStatePath,
            AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                .PersistedRunnerState persistedState)
        {
            statePath = persistedStatePath;
            state = persistedState;
            take = state.takes[state.currentTakeIndex];
            proof = take.runtimeProof
                ?? AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .RuntimeProof.Create(take.takeOrdinal);
            StartCoroutine(RunGuarded());
        }

        private void Update()
        {
            if (!armSourceFrameZero || beganShot || updateFailure != null)
            {
                return;
            }

            armSourceFrameZero = false;
            try
            {
                float minimumDelta = 1f / AuditionPvCaptureContract.Fps;
                proof.recorderCaptureDeltaTimeAtSourceFrameZero =
                    Time.captureDeltaTime;
                proof.recorderPaddingActiveAtSourceFrameZero =
                    Time.captureDeltaTime >= minimumDelta
                    && Time.captureDeltaTime < minimumDelta + 0.001f;
                if (!proof.recorderPaddingActiveAtSourceFrameZero)
                {
                    throw new InvalidOperationException(
                        "Recorder padding cadence was not active at S050 source f0.");
                }

                director.BeginShotForRecorder();
                beganShot = true;
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

            Exception cleanupFailure = CleanupAndCaptureProof();
            failure ??= cleanupFailure;
            NotifyFinished(failure);
        }

        private IEnumerator RunCore()
        {
            director = AuditionPvStationPhaseOneBossLowAngleCapture
                .AttachToFreshActiveScene(take.takeOrdinal);
            proof.freshSceneValidated = director.IsPrepared;
            director.FramePresented += HandleFramePresented;
            runtimeWorkloadCapture = AuditionPvRuntimeWorkloadCaptureSession.Open(
                new AuditionPvRuntimeWorkloadCaptureConfig
                {
                    captureId = take.captureId,
                    captureOutputDirectory = take.outputDirectory,
                    sourceShotId =
                        AuditionPvStationPhaseOneBossLowAngleCapture.ShotId,
                    sourceRangeStartFrame =
                        AuditionPvStationPhaseOneBossLowAngleCapture.FirstSourceFrame,
                    sourceRangeEndFrame =
                        AuditionPvStationPhaseOneBossLowAngleCapture.LastSourceFrame,
                    captureHudEvidence = false
                });
            recorderSettings = AuditionPvRecorderSettingsFactory
                .CreateLosslessPngSequence(
                    take.outputDirectory,
                    AuditionPvStationPhaseOneBossLowAngleCapture.ShotId);
            recorderSettings.controllerSettings.SetRecordModeToFrameInterval(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RawPaddingFrame,
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RawLastSourceFrame);
            AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
            recorderController = new RecorderController(
                recorderSettings.controllerSettings);
            recorderController.PrepareRecording();
            if (!recorderController.StartRecording())
            {
                throw new InvalidOperationException(
                    "Unity Recorder rejected the S050 QHD60 PNG session.");
            }

            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 1;
            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 2;
            armSourceFrameZero = true;

            double deadline = Time.realtimeSinceStartupAsDouble
                + ShotTimeoutSeconds;
            while (!beganShot
                && updateFailure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            if (updateFailure != null)
            {
                throw new InvalidOperationException(
                    "S050 could not arm source f0.", updateFailure);
            }

            if (!beganShot)
            {
                throw new TimeoutException("S050 timed out before source f0.");
            }

            while (!director.IsComplete
                && director.Failure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            if (director.Failure != null)
            {
                throw new InvalidOperationException(
                    "S050 director failed during recording.", director.Failure);
            }

            if (!director.IsComplete)
            {
                throw new TimeoutException(
                    "S050 did not complete source f0..f599 before timeout.");
            }

            yield return null;
            proof.recorderAutoStoppedAfterLastFrame =
                !recorderController.IsRecording();
            if (!proof.recorderAutoStoppedAfterLastFrame)
            {
                throw new InvalidOperationException(
                    "Recorder did not stop after raw600/source f599.");
            }
            take.runtimeWorkloadSealPath = runtimeWorkloadCapture.Complete();
            runtimeWorkloadCapture = null;
        }

        private void HandleFramePresented(int sourceFrame)
        {
            runtimeWorkloadCapture?.CapturePresentedFrame(sourceFrame);
            proof.presentedFramesExact &= sourceFrame == nextSourceFrame;
            proof.presentationClockExact &= PresentationClock.IsManuallyDriven
                && Mathf.Abs(
                    PresentationClock.UnscaledTime
                    - sourceFrame / (float)AuditionPvCaptureContract.Fps)
                    <= 0.00001f
                && Mathf.Abs(
                    PresentationClock.UnscaledDeltaTime
                    - 1f / AuditionPvCaptureContract.Fps)
                    <= 0.00001f;
            int logical = AuditionPvStationPhaseOneBossLowAngleCapture
                .SourceToSelectedLogicalFrame(sourceFrame);
            if (logical >= 0)
            {
                proof.selectedMappingExact &= logical == nextSelectedLogicalFrame;
                proof.selectedPresentedFrameCount++;
                nextSelectedLogicalFrame++;
            }

            proof.presentedFrameCount++;
            nextSourceFrame++;
        }

        private Exception CleanupAndCaptureProof()
        {
            if (cleaningUp)
            {
                return null;
            }

            cleaningUp = true;
            Exception firstFailure = null;
            CaptureFailure(ref firstFailure, () =>
            {
                if (director == null)
                {
                    return;
                }

                proof.directorCompleted = director.IsComplete;
                proof.lastSourceFrame = director.CurrentSourceFrame;
                proof.hudOffEveryFrame = director.AllFramesHudOff;
                proof.phaseOneEveryFrame = director.AllFramesPhaseOne;
                proof.cameraTakeoverObserved = director.CameraTakeoverObserved;
                proof.allSelectedFramesInFront = director.AllSelectedFramesInFront;
                proof.allSelectedFramesLowAngle = director.AllSelectedFramesLowAngle;
                proof.allSelectedFramesInCoverage = director.AllSelectedFramesInCoverage;
                proof.minimumProjectedHeight = director.MinimumProjectedHeight;
                proof.maximumProjectedHeight = director.MaximumProjectedHeight;
                proof.maximumEyeHeightRatio = director.MaximumEyeHeightRatio;
                proof.minimumCornerDepth = director.MinimumCornerDepth;
                proof.bossFullAliveAndUnchanged =
                    director.BossWasAndRemainsFullAndAlive;
                proof.transitionStateUnchanged = director.TransitionStateUnchanged;
                proof.transitionStartedEventCount =
                    director.ObservedTransitionStartedEvents;
                proof.transitionCompletedEventCount =
                    director.ObservedTransitionCompletedEvents;
            });
            CaptureFailure(ref firstFailure, () =>
            {
                recorderController?.StopRecording();
                recorderController = null;
            });
            CaptureFailure(ref firstFailure, () =>
            {
                if (director != null)
                {
                    director.RestoreShotState();
                    proof.stateRestored = director.StateRestored;
                    director.FramePresented -= HandleFramePresented;
                }
            });
            CaptureFailure(ref firstFailure, () =>
            {
                proof.presentationClockReleased = !PresentationClock.IsManuallyDriven;
                proof.cadenceSuspensionCountAfterRestore =
                    BossCombatCadenceScheduler.ExternalSuspensionCount;
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

        private void NotifyFinished(Exception failure)
        {
            if (notified)
            {
                return;
            }

            notified = true;
            AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                .NotifyPlayModeFinished(statePath, state, proof, failure);
        }

        private void OnDisable()
        {
            if (notified || !Application.isPlaying)
            {
                return;
            }

            Exception cleanupFailure = CleanupAndCaptureProof();
            NotifyFinished(
                cleanupFailure
                ?? new InvalidOperationException(
                    "S050 runner was disabled before finalization."));
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
}
