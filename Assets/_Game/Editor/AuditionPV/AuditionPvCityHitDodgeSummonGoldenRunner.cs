using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// One-take, fresh-scene S030 Recorder runner. Recorder raw0 remains explicit
    /// padding evidence; raw1..raw720 map to canonical source f0..f719.
    /// </summary>
    [InitializeOnLoad]
    public static class AuditionPvCityHitDodgeSummonGoldenRunner
    {
        internal const string MenuPath =
            "DimensionBrawl/Audition PV/Capture S030 City Hit Dodge Summon";
        internal const string StateFileName = "s030_runner_state.json";
        internal const string RuntimeProofFileName = "s030_runtime_proof.json";
        internal const string SourceLedgerFileName = "s030_source_ledger.json";
        internal const string FailureFileName = "s030_capture_failure.json";
        internal const string EvidenceFolderName = "evidence";
        internal const string BaselinesFolderName = "baselines";
        internal const string PaddingEvidenceFileName =
            "recorder_padding_raw_frame_0000.png";
        internal const int RawPaddingFrame = 0;
        internal const int RawFirstSourceFrame = 1;
        internal const int RawLastSourceFrame = 720;
        internal const int ExpectedRawFrameCount = 721;

        private const string SessionActiveKey =
            "DimensionBrawl.AuditionPV.S030GoldenRunner.Active";
        private const string SessionStatePathKey =
            "DimensionBrawl.AuditionPV.S030GoldenRunner.StatePath";
        private const string SessionBatchKey =
            "DimensionBrawl.AuditionPV.S030GoldenRunner.Batch";
        private const string SessionOwnerKey =
            "DimensionBrawl.AuditionPV.S030GoldenRunner.Owner";
        private const string SessionOwnerValue =
            "dimension-brawl.s030-city-hit-dodge-summon.v1";
        private const string StateSchema =
            "dimension-brawl.audition-pv.s030-runner-state.v1";
        private const string ProofSchema =
            "dimension-brawl.audition-pv.s030-runtime-proof.v1";
        private const string LedgerSchema =
            "dimension-brawl.audition-pv.canonical-source-ledger.v1";
        private const string FailureSchema =
            "dimension-brawl.audition-pv.capture-failure.v1";

        private static bool resumeScheduled;
        private static bool finalizing;
        private static AuditionPvCityHitDodgeSummonGoldenRunnerBehaviour
            activeBehaviour;

        static AuditionPvCityHitDodgeSummonGoldenRunner()
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
                    "S030 golden capture did not start",
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
                    "S030 batch capture requires -noaudio.");
            }

            if (Has("-batchmode") || Has("-nographics") || Has("-quit"))
            {
                throw new InvalidOperationException(
                    "S030 GameView capture is headful/asynchronous; remove "
                    + "-batchmode, -nographics, and -quit.");
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
                        ? $"<untitled:{scene.name}>"
                        : scene.path);
                }
            }

            if (dirty.Count > 0)
            {
                throw new InvalidOperationException(
                    "S030 refuses to replace dirty scenes: "
                    + string.Join(", ", dirty));
            }
        }

        internal static string[] CollectCaptureDependencyPaths()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string explicitPath in
                     AuditionPvCityHitDodgeSummonCapture
                         .ExplicitProductDependencyPaths())
            {
                string normalized = explicitPath.Replace('\\', '/');
                if (AssetDatabase.LoadMainAssetAtPath(normalized) == null
                    && !File.Exists(ProjectAbsolutePath(normalized)))
                {
                    throw new FileNotFoundException(
                        "S030 dependency is missing.",
                        normalized);
                }

                paths.Add(normalized);
                if (string.Equals(
                        normalized,
                        AuditionPvCityHitDodgeSummonCapture.CityScenePath,
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        normalized,
                        AuditionPvCityHitDodgeSummonCapture
                            .ChargeBruiserProfilePath,
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        normalized,
                        AuditionPvCityHitDodgeSummonCapture
                            .RifleCrossfirePrefabPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string dependency in
                             AssetDatabase.GetDependencies(normalized, true))
                    {
                        paths.Add(dependency.Replace('\\', '/'));
                    }
                }
            }

            return AuditionPvEnvironmentProbe
                .CollectCaptureDependencyPaths(paths);
        }

        internal static void ValidateStableDependencies(
            AuditionPvDependencyHash[] before,
            AuditionPvDependencyHash[] after)
        {
            AuditionPvDependencyHash[] initial =
                before ?? Array.Empty<AuditionPvDependencyHash>();
            AuditionPvDependencyHash[] current =
                after ?? Array.Empty<AuditionPvDependencyHash>();
            if (initial.Length != current.Length)
            {
                throw new InvalidOperationException(
                    "S030 dependency set changed during capture.");
            }

            var currentByPath = current.ToDictionary(
                entry => entry.path,
                StringComparer.OrdinalIgnoreCase);
            foreach (AuditionPvDependencyHash entry in initial)
            {
                if (entry == null
                    || !currentByPath.TryGetValue(
                        entry.path,
                        out AuditionPvDependencyHash found)
                    || entry.exists != found.exists
                    || entry.byteLength != found.byteLength
                    || !string.Equals(
                        entry.sha256,
                        found.sha256,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "S030 dependency changed during capture: "
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
                || !string.Equals(
                    before.commitSha,
                    after.commitSha,
                    StringComparison.Ordinal)
                || !string.Equals(
                    before.branch,
                    after.branch,
                    StringComparison.Ordinal)
                || before.isDirty != after.isDirty
                || !string.Equals(
                    before.dirtyStateHashSha256,
                    after.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Git source state changed during S030 capture.");
            }
        }

        internal static void ValidatePngFile(
            string path,
            int expectedWidth = AuditionPvCaptureContract.Width,
            int expectedHeight = AuditionPvCaptureContract.Height)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("S030 PNG is missing.", path);
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length
                    || stream.Read(header, 0, header.Length) != header.Length)
                {
                    throw new InvalidDataException(
                        "S030 PNG is truncated: " + path);
                }
            }

            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (!signature.SequenceEqual(header.Take(signature.Length))
                || header[12] != (byte)'I'
                || header[13] != (byte)'H'
                || header[14] != (byte)'D'
                || header[15] != (byte)'R')
            {
                throw new InvalidDataException(
                    "S030 PNG signature/IHDR mismatch: " + path);
            }

            int width = ReadBigEndianInt32(header, 16);
            int height = ReadBigEndianInt32(header, 20);
            if (width != expectedWidth || height != expectedHeight)
            {
                throw new InvalidDataException(
                    $"S030 PNG is {width}x{height}; expected "
                    + $"{expectedWidth}x{expectedHeight}: {path}");
            }
        }

        internal static string RemapRawFrames(
            string frameDirectory,
            string evidenceDirectory)
        {
            string frames = RequireDirectory(frameDirectory);
            ValidateExactSequence(
                frames,
                ExpectedRawFrameCount,
                RawFrameFileName);
            string evidence = Path.GetFullPath(evidenceDirectory);
            Directory.CreateDirectory(evidence);
            string staging = Path.Combine(
                Path.GetDirectoryName(frames)
                    ?? throw new InvalidOperationException(
                        "S030 frame directory has no parent."),
                ".s030-remap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            bool completed = false;
            try
            {
                for (int raw = RawFirstSourceFrame;
                    raw <= RawLastSourceFrame;
                    raw++)
                {
                    int source = RawToSourceFrame(raw);
                    MoveNew(
                        Path.Combine(frames, RawFrameFileName(raw)),
                        Path.Combine(
                            staging,
                            AuditionPvCityHitDodgeSummonCapture
                                .FrameFileName(source)));
                }

                string paddingEvidence = Path.Combine(
                    evidence,
                    PaddingEvidenceFileName);
                MoveNew(
                    Path.Combine(frames, RawFrameFileName(RawPaddingFrame)),
                    paddingEvidence);
                for (int source =
                        AuditionPvCityHitDodgeSummonCapture.FirstSourceFrame;
                    source <=
                        AuditionPvCityHitDodgeSummonCapture.LastSourceFrame;
                    source++)
                {
                    string fileName =
                        AuditionPvCityHitDodgeSummonCapture
                            .FrameFileName(source);
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

        internal static void ValidateCanonicalFrameSequence(
            string frameDirectory)
        {
            ValidateExactSequence(
                frameDirectory,
                AuditionPvCityHitDodgeSummonCapture.SourceFrameCount,
                AuditionPvCityHitDodgeSummonCapture.FrameFileName);
        }

        internal static SourceLedger CreateSourceLedger(
            PersistedRunnerState state,
            string frameDirectory,
            string citySceneSha256,
            string chargeBruiserSha256)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            string frames = RequireDirectory(frameDirectory);
            var entries = new SourceFrameLedgerEntry[
                AuditionPvCityHitDodgeSummonCapture.SourceFrameCount];
            for (int source = 0; source < entries.Length; source++)
            {
                string fileName =
                    AuditionPvCityHitDodgeSummonCapture.FrameFileName(source);
                string path = Path.Combine(frames, fileName);
                ValidatePngFile(path);
                var info = new FileInfo(path);
                entries[source] = new SourceFrameLedgerEntry
                {
                    sourceFrame = source,
                    recorderRawFrame = source + RawFirstSourceFrame,
                    selectedLogicalFrame =
                        AuditionPvCityHitDodgeSummonCapture
                            .SourceToSelectedLogicalFrame(source),
                    role = AuditionPvCityHitDodgeSummonCapture
                        .SourceFrameRole(source),
                    relativePngPath =
                        $"frames/{AuditionPvCityHitDodgeSummonCapture.ShotId}/{fileName}",
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
                captureId = state.captureId,
                segmentId = AuditionPvCityHitDodgeSummonCapture.SegmentId,
                shotId = AuditionPvCityHitDodgeSummonCapture.ShotId,
                scenePath = AuditionPvCityHitDodgeSummonCapture.CityScenePath,
                sceneSha256 = citySceneSha256,
                chargeBruiserProfilePath =
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfilePath,
                chargeBruiserProfileGuid =
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfileGuid,
                chargeBruiserProfileSha256 = chargeBruiserSha256,
                sourceFirstFrame =
                    AuditionPvCityHitDodgeSummonCapture.FirstSourceFrame,
                sourceLastFrame =
                    AuditionPvCityHitDodgeSummonCapture.LastSourceFrame,
                sourceFrameCount =
                    AuditionPvCityHitDodgeSummonCapture.SourceFrameCount,
                selectedFirstSourceFrame =
                    AuditionPvCityHitDodgeSummonCapture
                        .SelectedFirstSourceFrame,
                selectedLastSourceFrame =
                    AuditionPvCityHitDodgeSummonCapture
                        .SelectedLastSourceFrame,
                selectedFrameCount =
                    AuditionPvCityHitDodgeSummonCapture.SelectedFrameCount,
                preHandleFrameCount =
                    AuditionPvCityHitDodgeSummonCapture.PreHandleFrameCount,
                postHandleFrameCount =
                    AuditionPvCityHitDodgeSummonCapture.PostHandleFrameCount,
                recorderPaddingRawFrame = RawPaddingFrame,
                recorderFirstMappedRawFrame = RawFirstSourceFrame,
                recorderLastMappedRawFrame = RawLastSourceFrame,
                width = AuditionPvCaptureContract.Width,
                height = AuditionPvCaptureContract.Height,
                fps = AuditionPvCaptureContract.Fps,
                sourceFormat = AuditionPvCaptureContract.SourceFormat,
                semanticEvents = state.runtimeProof.events
                    ?? Array.Empty<AuditionPvCityHitDodgeSummonEvent>(),
                frames = entries
            };
            ValidateSourceLedger(ledger);
            return ledger;
        }

        internal static void ValidateSourceLedger(SourceLedger ledger)
        {
            if (ledger == null
                || !string.Equals(
                    ledger.schema,
                    LedgerSchema,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(ledger.captureId)
                || !string.Equals(
                    ledger.segmentId,
                    AuditionPvCityHitDodgeSummonCapture.SegmentId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ledger.shotId,
                    AuditionPvCityHitDodgeSummonCapture.ShotId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ledger.scenePath,
                    AuditionPvCityHitDodgeSummonCapture.CityScenePath,
                    StringComparison.Ordinal)
                || !AuditionPvSha256.IsSha256(ledger.sceneSha256)
                || !string.Equals(
                    ledger.chargeBruiserProfilePath,
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfilePath,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ledger.chargeBruiserProfileGuid,
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfileGuid,
                    StringComparison.Ordinal)
                || !AuditionPvSha256.IsSha256(
                    ledger.chargeBruiserProfileSha256)
                || ledger.sourceFirstFrame != 0
                || ledger.sourceLastFrame != 719
                || ledger.sourceFrameCount != 720
                || ledger.selectedFirstSourceFrame != 180
                || ledger.selectedLastSourceFrame != 539
                || ledger.selectedFrameCount != 360
                || ledger.preHandleFrameCount != 180
                || ledger.postHandleFrameCount != 180
                || ledger.recorderPaddingRawFrame != 0
                || ledger.recorderFirstMappedRawFrame != 1
                || ledger.recorderLastMappedRawFrame != 720
                || ledger.width != AuditionPvCaptureContract.Width
                || ledger.height != AuditionPvCaptureContract.Height
                || ledger.fps != AuditionPvCaptureContract.Fps
                || !string.Equals(
                    ledger.sourceFormat,
                    AuditionPvCaptureContract.SourceFormat,
                    StringComparison.Ordinal)
                || ledger.semanticEvents == null
                || ledger.semanticEvents.Length < 8
                || ledger.frames == null
                || ledger.frames.Length != 720)
            {
                throw new InvalidOperationException(
                    "S030 source ledger header is not canonical.");
            }

            for (int source = 0; source < ledger.frames.Length; source++)
            {
                SourceFrameLedgerEntry entry = ledger.frames[source];
                if (entry == null
                    || entry.sourceFrame != source
                    || entry.recorderRawFrame != source + 1
                    || entry.selectedLogicalFrame !=
                        AuditionPvCityHitDodgeSummonCapture
                            .SourceToSelectedLogicalFrame(source)
                    || !string.Equals(
                        entry.role,
                        AuditionPvCityHitDodgeSummonCapture
                            .SourceFrameRole(source),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        entry.relativePngPath,
                        $"frames/{AuditionPvCityHitDodgeSummonCapture.ShotId}/"
                            + AuditionPvCityHitDodgeSummonCapture
                                .FrameFileName(source),
                        StringComparison.Ordinal)
                    || entry.byteLength <= 24
                    || !AuditionPvSha256.IsSha256(entry.pngSha256)
                    || entry.width != AuditionPvCaptureContract.Width
                    || entry.height != AuditionPvCaptureContract.Height
                    || entry.fps != AuditionPvCaptureContract.Fps)
                {
                    throw new InvalidOperationException(
                        $"S030 source ledger frame f{source} is invalid.");
                }
            }
        }

        internal static AuditionPvTestResult[] CreateTestResults(
            AuditionPvCityHitDodgeSummonRuntimeProof proof,
            string proofPath,
            string ledgerPath,
            DateTime startedAtUtc)
        {
            AuditionPvCityHitDodgeSummonCapture.ValidateRuntimeProof(proof);
            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            return new[]
            {
                Passed(
                    "source",
                    "physical-qhd60-png-hash-ledger/s030",
                    "720 physical 2560x1440 PNGs plus separately hashed raw0 Recorder padding evidence.",
                    ledgerPath,
                    duration),
                Passed(
                    "lifecycle",
                    "fresh-scene-restore-reopen/s030",
                    "Capture leases/artifacts were released and an unmodified City product scene was reopened after Play Mode.",
                    proofPath,
                    duration)
            };
        }

        internal static AuditionPvTestResult[] WriteGateEvidenceArtifacts(
            PersistedRunnerState state,
            AuditionPvCityHitDodgeSummonRuntimeProof proof,
            string runtimeProofPath,
            string sourceLedgerPath,
            string evidenceDirectory,
            string captureCoreSha256,
            DateTime startedAtUtc)
        {
            AuditionPvCityHitDodgeSummonCapture.ValidateRuntimeProof(proof);
            if (state == null
                || !AuditionPvSha256.IsSha256(captureCoreSha256))
            {
                throw new InvalidOperationException(
                    "S030 Gate evidence requires an immutable capture-core SHA-256.");
            }

            long duration = Math.Max(
                0L,
                (long)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            string createdAtUtc = startedAtUtc.ToString(
                "O",
                CultureInfo.InvariantCulture);
            string normalizedRuntimeProofPath = Path.GetFullPath(
                runtimeProofPath).Replace('\\', '/');
            string runtimeProofSha256 =
                AuditionPvSha256.FileHash(runtimeProofPath);
            string normalizedSourceLedgerPath = Path.GetFullPath(
                sourceLedgerPath).Replace('\\', '/');
            string sourceLedgerSha256 =
                AuditionPvSha256.FileHash(sourceLedgerPath);
            var runtimePin = new AuditionPvPinnedArtifact
            {
                path = normalizedRuntimeProofPath,
                sha256 = runtimeProofSha256
            };
            var ledgerPin = new AuditionPvPinnedArtifact
            {
                path = normalizedSourceLedgerPath,
                sha256 = sourceLedgerSha256
            };

            string authorshipPath = Path.Combine(
                evidenceDirectory,
                "s030_shot_authorship.json");
            var authorship = new AuditionPvShotAuthorshipArtifact
            {
                schemaVersion =
                    AuditionPvSixtySecondGateManifestValidator
                        .ShotAuthorshipSchema,
                sourceCaptureCoreSha256 = captureCoreSha256,
                captureId = state.captureId,
                sourceShotId =
                    AuditionPvCityHitDodgeSummonCapture.ShotId,
                cameraId = AuditionPvCityHitDodgeSummonCapture.GateCameraId,
                gameplayState =
                    AuditionPvCityHitDodgeSummonCapture.GateGameplayState,
                deterministicSeed =
                    AuditionPvCityHitDodgeSummonCapture
                        .DeterministicRandomSeed,
                timelineId =
                    AuditionPvCityHitDodgeSummonCapture.GateTimelineId,
                runtimeProof = runtimePin,
                tool = "S030GoldenRunner",
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
                    AuditionPvCityHitDodgeSummonCapture
                        .GateEvidenceTestSuite,
                    "shot-authorship/"
                        + AuditionPvCityHitDodgeSummonCapture.ShotId,
                    $"artifact-sha256={authorshipSha256}; capture-core-sha256={captureCoreSha256}; exact-camera-state-seed-timeline=true",
                    authorshipPath,
                    duration),
                Passed(
                    AuditionPvCityHitDodgeSummonCapture
                        .GateEvidenceTestSuite,
                    "shot-authorship-runtime/"
                        + AuditionPvCityHitDodgeSummonCapture.ShotId,
                    $"artifact-sha256={runtimeProofSha256}; capture-core-sha256={captureCoreSha256}; exact-runtime=true",
                    runtimeProofPath,
                    duration)
            };

            string semanticDirectory = Path.Combine(
                evidenceDirectory,
                "semantic_beats");
            Directory.CreateDirectory(semanticDirectory);
            foreach (GateSemanticBeatSpec spec in
                     CreateGateSemanticBeatSpecs(proof))
            {
                string artifactPath = Path.Combine(
                    semanticDirectory,
                    spec.beatId + ".json");
                var artifact = new GateSemanticBeatRuntimeArtifact
                {
                    schemaVersion =
                        "dimension-brawl.audition-pv.s030-semantic-beat-runtime.v1",
                    sourceCaptureCoreSha256 = captureCoreSha256,
                    captureId = state.captureId,
                    sourceShotId =
                        AuditionPvCityHitDodgeSummonCapture.ShotId,
                    beatId = spec.beatId,
                    runtimeFactKey = spec.beatId,
                    sourceRangeStartFrame =
                        AuditionPvCityHitDodgeSummonCapture.FirstSourceFrame,
                    sourceRangeEndFrame =
                        AuditionPvCityHitDodgeSummonCapture.LastSourceFrame,
                    selectStartFrame =
                        AuditionPvCityHitDodgeSummonCapture
                            .SelectedFirstSourceFrame,
                    selectEndFrame =
                        AuditionPvCityHitDodgeSummonCapture
                            .SelectedLastSourceFrame,
                    logicalFactStartFrame = spec.sourceStartFrame
                        - AuditionPvCityHitDodgeSummonCapture
                            .SelectedFirstSourceFrame,
                    logicalFactEndFrame = spec.sourceEndFrame
                        - AuditionPvCityHitDodgeSummonCapture
                            .SelectedFirstSourceFrame,
                    sourceFactStartFrame = spec.sourceStartFrame,
                    sourceFactEndFrame = spec.sourceEndFrame,
                    exactFacts = spec.exactFacts,
                    runtimeProof = runtimePin,
                    sourceFrameLedger = ledgerPin,
                    producer = "S030GoldenRunner",
                    createdAtUtc = createdAtUtc
                };
                WriteJsonNew(artifactPath, artifact);
                string artifactSha256 =
                    AuditionPvSha256.FileHash(artifactPath);
                results.Add(Passed(
                    AuditionPvCityHitDodgeSummonCapture
                        .GateEvidenceTestSuite,
                    "semantic-beat/" + spec.beatId,
                    $"artifact-sha256={artifactSha256}; semantic-fact={spec.beatId}; capture-core-sha256={captureCoreSha256}; exact-runtime=true",
                    artifactPath,
                    duration));
            }

            string[] actualBeatIds = results
                .Where(result => result.name.StartsWith(
                    "semantic-beat/",
                    StringComparison.Ordinal))
                .Select(result => result.name.Substring(
                    "semantic-beat/".Length))
                .ToArray();
            if (!actualBeatIds.SequenceEqual(
                    AuditionPvCityHitDodgeSummonCapture
                        .GateSemanticBeatIds(),
                    StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "S030 Gate semantic-beat artifacts are incomplete or reordered.");
            }

            return results.ToArray();
        }

        private static GateSemanticBeatSpec[] CreateGateSemanticBeatSpecs(
            AuditionPvCityHitDodgeSummonRuntimeProof proof)
        {
            return new[]
            {
                new GateSemanticBeatSpec(
                    "player-hit",
                    proof.firstHostileHitFrame,
                    proof.firstHostileHitFrame,
                    $"source-team=Enemy",
                    $"hostile-projectiles={proof.hostileProjectileFiredCount}",
                    $"player-hp={proof.playerHealthAtStart.ToString("F3", CultureInfo.InvariantCulture)}>{proof.playerHealthAfterHostileHit.ToString("F3", CultureInfo.InvariantCulture)}"),
                new GateSemanticBeatSpec(
                    "perfect-dodge",
                    proof.dodgeRequestFrame,
                    proof.perfectDodgeFrame,
                    $"hud-request-source-frame={proof.dodgeRequestFrame}",
                    $"perfect-dodge-source-frame={proof.perfectDodgeFrame}",
                    $"player-hp-unchanged={proof.perfectDodgePreservedHealth}"),
                new GateSemanticBeatSpec(
                    "summon-chain",
                    proof.summonRequestFrame,
                    proof.summonDamageFrame,
                    $"hud-s1-source-frame={proof.summonRequestFrame}",
                    $"spent-tier={proof.summonSpentTier}",
                    $"ally-summon-damage-source-frame={proof.summonDamageFrame}")
            };
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

        private static void BeginCapture(bool batchMode)
        {
            if (SessionState.GetBool(SessionActiveKey, false)
                || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                throw new InvalidOperationException(
                    "S030 cannot start during another run, Play Mode, compilation, or asset update.");
            }

            EnsureNoDirtyOpenScenes();
            DateTime startedAtUtc = DateTime.UtcNow;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "S030 requires a successful Git provenance probe: "
                    + git.probeError);
            }

            AuditionPvEngineSnapshot engine =
                AuditionPvEnvironmentProbe.ReadEngineSnapshot();
            if (!string.Equals(
                    engine.recorderPackageVersion,
                    AuditionPvCaptureContract.RecorderPackageVersion,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S030 requires Unity Recorder "
                    + AuditionPvCaptureContract.RecorderPackageVersion + ".");
            }

            string[] dependencyPaths = CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencyHashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            string sceneSha = RequireDependencyHash(
                dependencyHashes,
                AuditionPvCityHitDodgeSummonCapture.CityScenePath);
            string profileSha = RequireDependencyHash(
                dependencyHashes,
                AuditionPvCityHitDodgeSummonCapture
                    .ChargeBruiserProfilePath);
            if (!string.Equals(
                    AssetDatabase.AssetPathToGUID(
                        AuditionPvCityHitDodgeSummonCapture
                            .ChargeBruiserProfilePath),
                    AuditionPvCityHitDodgeSummonCapture
                        .ChargeBruiserProfileGuid,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S030 ChargeBruiser profile GUID changed.");
            }

            string outputDirectory = string.Empty;
            string statePath = string.Empty;
            try
            {
                string captureId = AuditionPvOutputPaths.CreateOutputId(
                    "s030-city-hit-dodge-summon",
                    startedAtUtc,
                    git.commitSha,
                    git.isDirty,
                    git.dirtyStateHashSha256);
                outputDirectory =
                    AuditionPvOutputPaths.CreateUniqueGoldenOutputDirectory(
                        captureId);
                Directory.CreateDirectory(Path.Combine(
                    outputDirectory,
                    BaselinesFolderName));
                statePath = Path.Combine(outputDirectory, StateFileName);
                var state = new PersistedRunnerState
                {
                    schema = StateSchema,
                    phase = RunnerPhase.AwaitingPlayMode.ToString(),
                    batchMode = batchMode,
                    startedAtUtc = startedAtUtc.ToString("O"),
                    outputRoot = AuditionPvCaptureContract.OutputRoot,
                    captureId = new DirectoryInfo(outputDirectory).Name,
                    outputDirectory = outputDirectory,
                    gitCommitSha = git.commitSha,
                    gitBranch = git.branch,
                    gitWorktreeDirty = git.isDirty,
                    gitDirtyHashSha256 = git.dirtyStateHashSha256,
                    engine = CopyEngine(engine),
                    dependencyPaths = dependencyPaths,
                    dependencyHashesAtStart = dependencyHashes,
                    citySceneSha256AtStart = sceneSha,
                    chargeBruiserSha256AtStart = profileSha,
                    runtimeProof = new AuditionPvCityHitDodgeSummonRuntimeProof()
                };
                SaveState(statePath, state);
                SessionState.SetString(SessionOwnerKey, SessionOwnerValue);
                SessionState.SetString(SessionStatePathKey, statePath);
                SessionState.SetBool(SessionBatchKey, batchMode);
                SessionState.SetBool(SessionActiveKey, true);
                OpenFreshCityScene();
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                TryWriteFailureArtifact(
                    outputDirectory,
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

            string statePath = SessionState.GetString(
                SessionStatePathKey,
                string.Empty);
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
                else if (phase == RunnerPhase.Recording
                    && activeBehaviour == null)
                {
                    NotifyPlayModeFinished(
                        statePath,
                        state,
                        state.runtimeProof,
                        new InvalidOperationException(
                            "A domain reload interrupted S030 Recorder."));
                }

                return;
            }

            if (phase == RunnerPhase.AwaitingPlayMode)
            {
                OpenFreshCityScene();
                EditorApplication.isPlaying = true;
                return;
            }

            if (phase == RunnerPhase.Recording)
            {
                state.failure =
                    "Play Mode exited before S030 reported completion.";
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
                    AuditionPvCityHitDodgeSummonCapture.CityScenePath,
                    StringComparison.Ordinal))
            {
                NotifyPlayModeFinished(
                    statePath,
                    state,
                    state.runtimeProof,
                    new InvalidOperationException(
                        "S030 entered Play Mode without the fresh City scene."));
                return;
            }

            state.phase = RunnerPhase.Recording.ToString();
            SaveState(statePath, state);
            var root = new GameObject("[AuditionPV_S030_GoldenRunner]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            activeBehaviour = root.AddComponent<
                AuditionPvCityHitDodgeSummonGoldenRunnerBehaviour>();
            activeBehaviour.Begin(statePath, state);
        }

        internal static void NotifyPlayModeFinished(
            string statePath,
            PersistedRunnerState state,
            AuditionPvCityHitDodgeSummonRuntimeProof proof,
            Exception failure)
        {
            activeBehaviour = null;
            state.runtimeProof = proof
                ?? state.runtimeProof
                ?? new AuditionPvCityHitDodgeSummonRuntimeProof();
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
            try
            {
                AuditionPvCityHitDodgeSummonCapture
                    .ReopenProductSceneAfterPlayMode();
                Scene scene = SceneManager.GetActiveScene();
                if (!scene.IsValid()
                    || !scene.isLoaded
                    || scene.isDirty
                    || !string.Equals(
                        scene.path,
                        AuditionPvCityHitDodgeSummonCapture.CityScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "S030 did not reopen an unmodified City scene.");
                }

                state.runtimeProof.freshSceneReopened = true;
                if (!string.IsNullOrWhiteSpace(state.failure))
                {
                    throw new InvalidOperationException(
                        "S030 failed in Play Mode.\n" + state.failure);
                }

                FinalizeSuccessfulCapture(state);
                state.phase = RunnerPhase.Complete.ToString();
                SaveState(statePath, state);
                complete = true;
            }
            catch (Exception exception)
            {
                failure = exception;
                TryWriteFailureArtifact(
                    state.outputDirectory,
                    state.phase,
                    exception,
                    state.runtimeProof);
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
                        "[AuditionPV] S030 fresh City source passed: "
                        + state.outputDirectory);
                    if (batch)
                    {
                        EditorApplication.Exit(0);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(state.outputDirectory);
                    }
                }
                else if (batch)
                {
                    EditorApplication.Exit(1);
                }
                else if (failure != null)
                {
                    EditorUtility.DisplayDialog(
                        "S030 golden capture failed",
                        failure.Message,
                        "OK");
                }
            }
        }

        private static void FinalizeSuccessfulCapture(
            PersistedRunnerState state)
        {
            DateTime startedAtUtc = DateTime.Parse(
                state.startedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
            AuditionPvCityHitDodgeSummonCapture.ValidateRuntimeProof(
                state.runtimeProof);

            string frameDirectory = Path.Combine(
                state.outputDirectory,
                "frames",
                AuditionPvCityHitDodgeSummonCapture.ShotId);
            string evidenceDirectory = Path.Combine(
                state.outputDirectory,
                EvidenceFolderName);
            string paddingEvidence = RemapRawFrames(
                frameDirectory,
                evidenceDirectory);
            ValidatePngFile(paddingEvidence);
            ValidateCanonicalFrameSequence(frameDirectory);

            AuditionPvGitSnapshot gitAtEnd =
                AuditionPvEnvironmentProbe.ReadGitSnapshot();
            ValidateStableGitSnapshot(CreateGitSnapshot(state), gitAtEnd);
            string[] dependencyPathsAtEnd = CollectCaptureDependencyPaths();
            if (!state.dependencyPaths.SequenceEqual(
                    dependencyPathsAtEnd,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "S030 dependency path set changed during capture.");
            }

            AuditionPvDependencyHash[] dependenciesAtEnd =
                AuditionPvEnvironmentProbe.HashDependencies(
                    dependencyPathsAtEnd);
            ValidateStableDependencies(
                state.dependencyHashesAtStart,
                dependenciesAtEnd);
            string sceneSha = RequireDependencyHash(
                dependenciesAtEnd,
                AuditionPvCityHitDodgeSummonCapture.CityScenePath);
            string profileSha = RequireDependencyHash(
                dependenciesAtEnd,
                AuditionPvCityHitDodgeSummonCapture
                    .ChargeBruiserProfilePath);
            if (!string.Equals(
                    sceneSha,
                    state.citySceneSha256AtStart,
                    StringComparison.Ordinal)
                || !string.Equals(
                    profileSha,
                    state.chargeBruiserSha256AtStart,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "S030 City scene or ChargeBruiser profile hash changed.");
            }

            SourceLedger ledger = CreateSourceLedger(
                state,
                frameDirectory,
                sceneSha,
                profileSha);
            string ledgerPath = Path.Combine(
                evidenceDirectory,
                SourceLedgerFileName);
            WriteJsonNew(ledgerPath, ledger);
            string proofPath = Path.Combine(
                evidenceDirectory,
                RuntimeProofFileName);
            WriteJsonNew(proofPath, new RuntimeProofArtifact
            {
                schema = ProofSchema,
                captureId = state.captureId,
                mapping =
                    "Recorder raw0 is padding evidence only; raw1..raw720 map "
                    + "one-to-one to canonical source f0..f719; select f180..f539 "
                    + "maps to PV_S030 logical f0..f359 with actual 180f handles.",
                sourceLedgerPath = ledgerPath.Replace('\\', '/'),
                sourceLedgerSha256 = AuditionPvSha256.FileHash(ledgerPath),
                paddingEvidencePath = paddingEvidence,
                paddingEvidenceSha256 =
                    AuditionPvSha256.FileHash(paddingEvidence),
                citySceneSha256 = sceneSha,
                chargeBruiserProfileSha256 = profileSha,
                runtime = state.runtimeProof
            });

            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvCityHitDodgeSummonCapture
                    .CreateBaselineManifestEntries(state.runtimeProof);
            CopyBaselines(frameDirectory, state.outputDirectory, baselines);
            AuditionPvShotManifestEntry[] shots =
            {
                AuditionPvCityHitDodgeSummonCapture
                    .CreateShotManifestEntry()
            };
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
                    engineSnapshot: RestoreEngine(state.engine),
                    dependencyHashSnapshot: state.dependencyHashesAtStart);
            string captureCoreSha256 =
                AuditionPvSixtySecondGateManifestValidator
                    .CaptureCoreSha256(captureCoreManifest);
            if (!AuditionPvSha256.IsSha256(captureCoreSha256))
            {
                throw new InvalidDataException(
                    "S030 could not create its immutable Gate capture-core identity.");
            }

            AuditionPvTestResult[] ordinaryTests = CreateTestResults(
                state.runtimeProof,
                proofPath,
                ledgerPath,
                startedAtUtc);
            AuditionPvTestResult[] gateTests = WriteGateEvidenceArtifacts(
                state,
                state.runtimeProof,
                proofPath,
                ledgerPath,
                evidenceDirectory,
                captureCoreSha256,
                startedAtUtc);
            AuditionPvTestResult[] tests = ordinaryTests
                .Concat(gateTests)
                .ToArray();
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    shots,
                    baselines,
                    tests,
                    createdAtUtc: startedAtUtc,
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
                    "S030 Gate evidence changed its immutable capture-core identity.");
            }

            AuditionPvCaptureManifestWriter.WriteNew(manifest);
        }

        private static void CopyBaselines(
            string frameDirectory,
            string outputDirectory,
            IEnumerable<AuditionPvBaselineManifestEntry> baselines)
        {
            string destinationDirectory = Path.Combine(
                outputDirectory,
                BaselinesFolderName);
            Directory.CreateDirectory(destinationDirectory);
            foreach (AuditionPvBaselineManifestEntry baseline in baselines)
            {
                string source = Path.Combine(
                    frameDirectory,
                    AuditionPvCityHitDodgeSummonCapture
                        .FrameFileName(baseline.sourceFrame));
                string destination = Path.Combine(
                    destinationDirectory,
                    baseline.fileName);
                if (File.Exists(destination))
                {
                    throw new IOException(
                        "S030 baseline refuses to overwrite: " + destination);
                }

                File.Copy(source, destination, overwrite: false);
                ValidatePngFile(destination);
            }
        }

        private static void OpenFreshCityScene()
        {
            EditorSceneManager.OpenScene(
                AuditionPvCityHitDodgeSummonCapture.CityScenePath,
                OpenSceneMode.Single);
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || scene.isDirty)
            {
                throw new InvalidOperationException(
                    "S030 could not open a clean fresh City scene.");
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
                throw new InvalidDataException(
                    "Unknown S030 runner phase: " + value);
            }

            return phase;
        }

        private static void ValidateState(PersistedRunnerState state)
        {
            if (state == null
                || !string.Equals(
                    state.schema,
                    StateSchema,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(state.captureId)
                || string.IsNullOrWhiteSpace(state.outputDirectory)
                || state.engine == null
                || state.dependencyPaths == null
                || state.dependencyHashesAtStart == null
                || !AuditionPvSha256.IsSha256(
                    state.citySceneSha256AtStart)
                || !AuditionPvSha256.IsSha256(
                    state.chargeBruiserSha256AtStart)
                || state.runtimeProof == null)
            {
                throw new InvalidDataException(
                    "S030 persisted runner state is invalid.");
            }

            ParsePhase(state.phase);
        }

        private static string RequireDependencyHash(
            IEnumerable<AuditionPvDependencyHash> dependencies,
            string path)
        {
            AuditionPvDependencyHash dependency = dependencies?.FirstOrDefault(
                entry => entry != null
                    && string.Equals(
                        entry.path,
                        path,
                        StringComparison.OrdinalIgnoreCase));
            if (dependency == null
                || !dependency.exists
                || !AuditionPvSha256.IsSha256(dependency.sha256))
            {
                throw new InvalidOperationException(
                    "S030 dependency lacks a physical SHA-256: " + path);
            }

            return dependency.sha256;
        }

        private static string ProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Unity project root is missing.");
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
                    $"S030 frame sequence contains {actual.Length} PNGs; "
                    + $"expected exact {expectedCount}-file canonical set.");
            }
        }

        private static void MoveNew(string source, string destination)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException(
                    "S030 source frame is missing.",
                    source);
            }

            if (File.Exists(destination))
            {
                throw new IOException(
                    "S030 remap refuses to overwrite: " + destination);
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

        private static void SaveState(
            string path,
            PersistedRunnerState state)
        {
            string normalized = Path.GetFullPath(path);
            string parent = Path.GetDirectoryName(normalized)
                ?? throw new InvalidOperationException(
                    "S030 state path has no parent.");
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
                throw new FileNotFoundException(
                    "S030 persisted state is missing.",
                    path);
            }

            return JsonUtility.FromJson<PersistedRunnerState>(
                    File.ReadAllText(path))
                ?? throw new InvalidDataException(
                    "S030 persisted state JSON is empty.");
        }

        private static void WriteJsonNew<T>(string path, T value)
        {
            string normalized = Path.GetFullPath(path);
            string parent = Path.GetDirectoryName(normalized)
                ?? throw new InvalidOperationException(
                    "S030 artifact path has no parent.");
            Directory.CreateDirectory(parent);
            using var stream = new FileStream(
                normalized,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
            using var writer = new StreamWriter(
                stream,
                new UTF8Encoding(false));
            writer.Write(JsonUtility.ToJson(value, true));
            writer.WriteLine();
        }

        private static void TryWriteFailureArtifact(
            string outputDirectory,
            string phase,
            Exception exception,
            AuditionPvCityHitDodgeSummonRuntimeProof proof)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            try
            {
                string evidence = Path.Combine(
                    outputDirectory,
                    EvidenceFolderName);
                Directory.CreateDirectory(evidence);
                string path = Path.Combine(evidence, FailureFileName);
                if (File.Exists(path))
                {
                    path = Path.Combine(
                        evidence,
                        Path.GetFileNameWithoutExtension(FailureFileName)
                            + "_"
                            + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
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
                activeRenderPipelineAssetPath =
                    value.activeRenderPipelineAssetPath
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
                activeRenderPipelineAssetPath =
                    value.activeRenderPipelineAssetPath
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
            public string startedAtUtc = string.Empty;
            public string outputRoot = string.Empty;
            public string captureId = string.Empty;
            public string outputDirectory = string.Empty;
            public string gitCommitSha = string.Empty;
            public string gitBranch = string.Empty;
            public bool gitWorktreeDirty;
            public string gitDirtyHashSha256 = string.Empty;
            public EngineState engine;
            public string[] dependencyPaths = Array.Empty<string>();
            public AuditionPvDependencyHash[] dependencyHashesAtStart =
                Array.Empty<AuditionPvDependencyHash>();
            public string citySceneSha256AtStart = string.Empty;
            public string chargeBruiserSha256AtStart = string.Empty;
            public AuditionPvCityHitDodgeSummonRuntimeProof runtimeProof;
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
        internal sealed class SourceLedger
        {
            public string schema = string.Empty;
            public string captureId = string.Empty;
            public string segmentId = string.Empty;
            public string shotId = string.Empty;
            public string scenePath = string.Empty;
            public string sceneSha256 = string.Empty;
            public string chargeBruiserProfilePath = string.Empty;
            public string chargeBruiserProfileGuid = string.Empty;
            public string chargeBruiserProfileSha256 = string.Empty;
            public int sourceFirstFrame;
            public int sourceLastFrame;
            public int sourceFrameCount;
            public int selectedFirstSourceFrame;
            public int selectedLastSourceFrame;
            public int selectedFrameCount;
            public int preHandleFrameCount;
            public int postHandleFrameCount;
            public int recorderPaddingRawFrame;
            public int recorderFirstMappedRawFrame;
            public int recorderLastMappedRawFrame;
            public int width;
            public int height;
            public int fps;
            public string sourceFormat = string.Empty;
            public AuditionPvCityHitDodgeSummonEvent[] semanticEvents =
                Array.Empty<AuditionPvCityHitDodgeSummonEvent>();
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

        private sealed class GateSemanticBeatSpec
        {
            public GateSemanticBeatSpec(
                string beatId,
                int sourceStartFrame,
                int sourceEndFrame,
                params string[] exactFacts)
            {
                this.beatId = beatId;
                this.sourceStartFrame = sourceStartFrame;
                this.sourceEndFrame = sourceEndFrame;
                this.exactFacts = exactFacts ?? Array.Empty<string>();
            }

            public readonly string beatId;
            public readonly int sourceStartFrame;
            public readonly int sourceEndFrame;
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
            public int selectStartFrame;
            public int selectEndFrame;
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
            public string sourceLedgerPath = string.Empty;
            public string sourceLedgerSha256 = string.Empty;
            public string paddingEvidencePath = string.Empty;
            public string paddingEvidenceSha256 = string.Empty;
            public string citySceneSha256 = string.Empty;
            public string chargeBruiserProfileSha256 = string.Empty;
            public AuditionPvCityHitDodgeSummonRuntimeProof runtime;
        }

        [Serializable]
        private sealed class FailureArtifact
        {
            public string schema = string.Empty;
            public string createdAtUtc = string.Empty;
            public string phase = string.Empty;
            public string exception = string.Empty;
            public AuditionPvCityHitDodgeSummonRuntimeProof runtime;
        }
    }

    [DefaultExecutionOrder(-32500)]
    public sealed class AuditionPvCityHitDodgeSummonGoldenRunnerBehaviour
        : MonoBehaviour
    {
        private const double ShotTimeoutSeconds = 300d;

        private string statePath;
        private AuditionPvCityHitDodgeSummonGoldenRunner.PersistedRunnerState
            state;
        private AuditionPvCityHitDodgeSummonRuntimeProof proof;
        private AuditionPvCityHitDodgeSummonDirector director;
        private AuditionPvRecorderSettingsBundle recorderSettings;
        private RecorderController recorderController;
        private Exception updateFailure;
        private bool armSourceFrameZero;
        private bool beganShot;
        private bool cleaningUp;
        private bool notified;
        private int nextSourceFrame;
        private int nextSelectedLogicalFrame;

        internal void Begin(
            string persistedStatePath,
            AuditionPvCityHitDodgeSummonGoldenRunner.PersistedRunnerState
                persistedState)
        {
            statePath = persistedStatePath;
            state = persistedState;
            proof = state.runtimeProof
                ?? new AuditionPvCityHitDodgeSummonRuntimeProof();
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
                    Time.captureDeltaTime > minimumDelta
                    && Time.captureDeltaTime < minimumDelta + 0.001f;
                if (!proof.recorderPaddingActiveAtSourceFrameZero)
                {
                    throw new InvalidOperationException(
                        "Recorder padding cadence was not active at S030 f0.");
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
            director = AuditionPvCityHitDodgeSummonCapture
                .AttachToFreshActiveScene();
            proof.freshSceneValidated = director.IsPrepared;
            director.FramePresented += HandleFramePresented;
            recorderSettings = AuditionPvRecorderSettingsFactory
                .CreateLosslessPngSequence(
                    state.outputDirectory,
                    AuditionPvCityHitDodgeSummonCapture.ShotId);
            recorderSettings.controllerSettings.SetRecordModeToFrameInterval(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawPaddingFrame,
                AuditionPvCityHitDodgeSummonGoldenRunner.RawLastSourceFrame);
            AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
            recorderController = new RecorderController(
                recorderSettings.controllerSettings);
            recorderController.PrepareRecording();
            if (!recorderController.StartRecording())
            {
                throw new InvalidOperationException(
                    "Unity Recorder rejected the S030 QHD60 PNG session.");
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
                    "S030 could not arm source f0.",
                    updateFailure);
            }

            if (!beganShot)
            {
                throw new TimeoutException(
                    "S030 timed out before source f0.");
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
                    "S030 director failed during recording.",
                    director.Failure);
            }

            if (!director.IsComplete)
            {
                throw new TimeoutException(
                    "S030 did not complete source f0..f719.");
            }

            yield return null;
            proof.recorderAutoStoppedAfterLastFrame =
                !recorderController.IsRecording();
            if (!proof.recorderAutoStoppedAfterLastFrame)
            {
                throw new InvalidOperationException(
                    "Recorder did not stop after raw720/source f719.");
            }
        }

        private void HandleFramePresented(int sourceFrame)
        {
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
            int logical = AuditionPvCityHitDodgeSummonCapture
                .SourceToSelectedLogicalFrame(sourceFrame);
            if (logical >= 0)
            {
                proof.selectedMappingExact &=
                    logical == nextSelectedLogicalFrame;
                proof.selectedPresentedFrameCount++;
                nextSelectedLogicalFrame++;
            }
            else if (sourceFrame <
                AuditionPvCityHitDodgeSummonCapture.SelectedFirstSourceFrame)
            {
                proof.preHandlePresentedFrameCount++;
            }
            else
            {
                proof.postHandlePresentedFrameCount++;
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
                director?.PopulateRuntimeProof(proof));
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
                    director.FramePresented -= HandleFramePresented;
                    director.PopulateRuntimeProof(proof);
                }
            });
            CaptureFailure(ref firstFailure, () =>
            {
                proof.presentationClockReleased =
                    !PresentationClock.IsManuallyDriven;
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
            AuditionPvCityHitDodgeSummonGoldenRunner
                .NotifyPlayModeFinished(
                    statePath,
                    state,
                    proof,
                    failure);
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
                    "S030 runner was disabled before finalization."));
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
