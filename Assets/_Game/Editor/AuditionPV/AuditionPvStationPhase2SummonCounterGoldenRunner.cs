using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.Combat;
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
    /// Editor/PlayMode orchestration for the G06 golden source. The product-state
    /// director owns gameplay; this runner owns Recorder, evidence, validation,
    /// provenance, and editor lifecycle only.
    /// </summary>
    [InitializeOnLoad]
    public static class AuditionPvStationPhase2SummonCounterGoldenRunner
    {
        internal const string RunnerScriptPath =
            "Assets/_Game/Editor/AuditionPV/AuditionPvStationPhase2SummonCounterGoldenRunner.cs";
        internal const string RunnerTestPath =
            "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvStationPhase2SummonCounterGoldenTests.cs";
        internal const string MenuPath =
            "DimensionBrawl/Audition PV/Capture G06 Station Phase 2 Summon Counter Golden Source";
        internal const string StateFileName = "g06_runner_state.json";
        internal const string RuntimeProofFileName = "g06_runtime_proof.json";
        internal const string FailureFileName = "g06_capture_failure.json";
        internal const string EvidenceFolderName = "evidence";
        internal const string WarmupEvidenceFileName =
            "recorder_warmup_raw_frame_0000.png";
        internal const int RawWarmupFrame = 0;
        internal const int RawFirstShotFrame = 1;
        internal const int RawLastShotFrame = 360;
        internal const int ExpectedRawFrameCount = 361;

        private const string SessionActiveKey =
            "DimensionBrawl.AuditionPV.G06GoldenRunner.Active";
        private const string SessionStatePathKey =
            "DimensionBrawl.AuditionPV.G06GoldenRunner.StatePath";
        private const string SessionOwnerKey =
            "DimensionBrawl.AuditionPV.G06GoldenRunner.Owner";
        private const string SessionBatchKey =
            "DimensionBrawl.AuditionPV.G06GoldenRunner.Batch";
        private const string SessionOwnerValue =
            "dimension-brawl.g06-station-phase2-summon-counter.v1";
        private const string RunnerSchema =
            "dimension-brawl.audition-pv.g06-runner-state.v1";
        private const string RuntimeProofSchema =
            "dimension-brawl.audition-pv.g06-runtime-proof.v1";
        private const string FailureSchema =
            "dimension-brawl.audition-pv.capture-failure.v1";
        private const double MinimumScreenDeltaMeanRgb = 2d;
        private const double MinimumScreenChangedRatio = 0.08d;
        private const double MinimumCounterDeltaMeanRgb = 1.25d;
        private const double MinimumCounterChangedRatio = 0.04d;
        private const double MaximumSequenceBlackRatio = 0.90d;
        private const double MaximumSequenceMagentaRatio = 0.005d;
        private const double MaximumFrameMagentaRatio = 0.02d;
        private const int MinimumHealthyFramePercent = 90;
        private const int MinimumHudAccentSamples = 12;

        private static bool resumeScheduled;
        private static bool finalizing;
        private static AuditionPvStationPhase2SummonCounterGoldenRunnerBehaviour activeBehaviour;

        static AuditionPvStationPhase2SummonCounterGoldenRunner()
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
                    "G06 golden capture did not start",
                    exception.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Asynchronous unattended entry point. Invoke a graphics-capable Editor
        /// with -executeMethod and -noaudio, without -batchmode, -quit, or
        /// -nographics. This method exits only after PlayMode recording,
        /// validation, and manifest writing have completed.
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
                    "G06 RunBatchCapture requires -noaudio for unattended capture.");
            }

            if (Has("-quit"))
            {
                throw new InvalidOperationException(
                    "Do not pass -quit to the asynchronous G06 capture; the runner exits after finalization.");
            }

            if (Has("-nographics"))
            {
                throw new InvalidOperationException(
                    "G06 Game View PNG capture requires graphics; remove -nographics.");
            }

            if (Has("-batchmode"))
            {
                throw new InvalidOperationException(
                    "G06 GameViewInput capture requires a headful Editor; remove -batchmode.");
            }
        }

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
                    "G06 refuses to replace dirty open scenes. Save or discard them explicitly first: "
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

            foreach (string explicitPath in
                     AuditionPvStationPhase2SummonCounterCapture
                         .ExplicitProductDependencyPaths())
            {
                if (AssetDatabase.LoadMainAssetAtPath(explicitPath) == null
                    && !File.Exists(ProjectAbsolutePath(explicitPath)))
                {
                    throw new FileNotFoundException(
                        "G06 explicit product dependency is missing.",
                        explicitPath);
                }

                dependencies.Add(explicitPath.Replace('\\', '/'));
                foreach (string nested in AssetDatabase.GetDependencies(explicitPath, true))
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
                || start.isDirty
                || end.isDirty
                || !string.Equals(start.commitSha, end.commitSha, StringComparison.Ordinal)
                || !string.Equals(start.branch, end.branch, StringComparison.Ordinal)
                || start.isDirty != end.isDirty
                || !string.Equals(
                    start.dirtyStateHashSha256,
                    end.dirtyStateHashSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Git source state changed while G06 was recording; discard this take.");
            }
        }

        internal static void ValidateStableDependencies(
            AuditionPvDependencyHash[] start,
            AuditionPvDependencyHash[] end)
        {
            AuditionPvDependencyHash[] initial = start ?? Array.Empty<AuditionPvDependencyHash>();
            AuditionPvDependencyHash[] current = end ?? Array.Empty<AuditionPvDependencyHash>();
            var currentByPath = current.ToDictionary(
                dependency => dependency.path,
                StringComparer.OrdinalIgnoreCase);

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
                        "G06 dependency changed while recording: "
                        + (dependency?.path ?? "<null>"));
                }
            }

            if (initial.Length != current.Length)
            {
                throw new InvalidOperationException(
                    "G06 dependency set changed while recording; discard this take.");
            }
        }

        internal static void ValidatePngFile(
            string path,
            int expectedWidth,
            int expectedHeight)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Expected G06 PNG is missing.", path);
            }

            byte[] header = new byte[24];
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length < header.Length
                    || stream.Read(header, 0, header.Length) != header.Length)
                {
                    throw new InvalidDataException($"PNG is truncated: {path}");
                }
            }

            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            for (int index = 0; index < signature.Length; index++)
            {
                if (header[index] != signature[index])
                {
                    throw new InvalidDataException($"PNG signature mismatch: {path}");
                }
            }

            if (header[12] != (byte)'I'
                || header[13] != (byte)'H'
                || header[14] != (byte)'D'
                || header[15] != (byte)'R')
            {
                throw new InvalidDataException($"PNG does not begin with IHDR: {path}");
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
        /// Preserves Recorder's resolution warm-up frame as evidence and maps
        /// raw 1..360 to logical 0..359 through a collision-free staging folder.
        /// </summary>
        internal static string RemapRawFrames(
            string frameDirectory,
            string evidenceDirectory)
        {
            string normalizedFrameDirectory = RequireDirectory(frameDirectory);
            string normalizedEvidenceDirectory = Path.GetFullPath(evidenceDirectory);
            Directory.CreateDirectory(normalizedEvidenceDirectory);
            ValidateRawFrameSequence(normalizedFrameDirectory);

            string stagingDirectory = Path.Combine(
                Path.GetDirectoryName(normalizedFrameDirectory)
                    ?? throw new InvalidOperationException("Frame directory has no parent."),
                ".g06-remap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stagingDirectory);
            bool completed = false;
            try
            {
                for (int rawFrame = RawFirstShotFrame;
                    rawFrame <= RawLastShotFrame;
                    rawFrame++)
                {
                    string source = Path.Combine(
                        normalizedFrameDirectory,
                        RawFrameFileName(rawFrame));
                    string destination = Path.Combine(
                        stagingDirectory,
                        AuditionPvStationPhase2SummonCounterCapture.FrameFileName(
                            rawFrame - RawFirstShotFrame));
                    MoveNew(source, destination);
                }

                string warmupSource = Path.Combine(
                    normalizedFrameDirectory,
                    RawFrameFileName(RawWarmupFrame));
                string warmupEvidence = Path.Combine(
                    normalizedEvidenceDirectory,
                    WarmupEvidenceFileName);
                MoveNew(warmupSource, warmupEvidence);

                for (int logicalFrame =
                         AuditionPvStationPhase2SummonCounterCapture.FirstFrame;
                    logicalFrame <=
                         AuditionPvStationPhase2SummonCounterCapture.LastFrame;
                    logicalFrame++)
                {
                    string fileName =
                        AuditionPvStationPhase2SummonCounterCapture.FrameFileName(
                            logicalFrame);
                    MoveNew(
                        Path.Combine(stagingDirectory, fileName),
                        Path.Combine(normalizedFrameDirectory, fileName));
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
                AuditionPvStationPhase2SummonCounterCapture.ExpectedFrameCount,
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName);
        }

        internal static ScreenDeltaMetrics EvaluateScreenDelta(
            Color32[] before,
            Color32[] after,
            int sampleStride = 1)
        {
            if (before == null || after == null || before.Length != after.Length)
            {
                throw new ArgumentException(
                    "Screen-delta frames must be non-null and have equal pixel counts.");
            }

            if (before.Length == 0 || sampleStride <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleStride));
            }

            long samples = 0;
            long changed = 0;
            double absoluteRgb = 0d;
            for (int index = 0; index < before.Length; index += sampleStride)
            {
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

            return new ScreenDeltaMetrics
            {
                sampleCount = samples,
                changedSampleCount = changed,
                meanAbsoluteRgb = absoluteRgb / samples,
                changedSampleRatio = changed / (double)samples
            };
        }

        internal static void ValidateScreenDelta(ScreenDeltaMetrics metrics)
        {
            if (metrics == null
                || metrics.sampleCount <= 0
                || metrics.meanAbsoluteRgb < MinimumScreenDeltaMeanRgb
                || metrics.changedSampleRatio < MinimumScreenChangedRatio)
            {
                throw new InvalidOperationException(
                    "G06 f188->f189 screen-domain pixel delta is missing or too small: "
                    + $"mean={metrics?.meanAbsoluteRgb ?? 0d:F3}, "
                    + $"changed={metrics?.changedSampleRatio ?? 0d:P2}.");
            }
        }

        internal static void ValidateCounterDelta(ScreenDeltaMetrics metrics)
        {
            if (metrics == null
                || metrics.sampleCount <= 0
                || metrics.meanAbsoluteRgb < MinimumCounterDeltaMeanRgb
                || metrics.changedSampleRatio < MinimumCounterChangedRatio)
            {
                throw new InvalidOperationException(
                    "G06 f249->f251 retained-projectile intercept/counter pixel delta is missing or too small: "
                    + $"mean={metrics?.meanAbsoluteRgb ?? 0d:F3}, "
                    + $"changed={metrics?.changedSampleRatio ?? 0d:P2}.");
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
                    "A G06 capture cannot start while another capture, Play Mode, compilation, or asset update is active.");
            }

            EnsureNoDirtyOpenScenes();
            DateTime startedAtUtc = DateTime.UtcNow;
            AuditionPvGitSnapshot git = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            if (!git.probeSucceeded)
            {
                throw new InvalidOperationException(
                    "G06 requires a successful Git provenance probe: "
                    + git.probeError);
            }

            if (git.isDirty)
            {
                throw new InvalidOperationException(
                    "G06 golden capture requires a clean Git worktree.");
            }

            AuditionPvEngineSnapshot engine = AuditionPvEnvironmentProbe.ReadEngineSnapshot();
            if (!string.Equals(
                    engine.recorderPackageVersion,
                    AuditionPvCaptureContract.RecorderPackageVersion,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G06 requires Unity Recorder "
                    + AuditionPvCaptureContract.RecorderPackageVersion
                    + $"; found {engine.recorderPackageVersion}.");
            }

            string[] dependencyPaths = CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencyHashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPaths);
            AuditionPvDependencyHash stationSceneHash = dependencyHashes.FirstOrDefault(
                dependency => string.Equals(
                    dependency.path,
                    AuditionPvStationPhase2SummonCounterCapture.StationScenePath,
                    StringComparison.OrdinalIgnoreCase));
            if (stationSceneHash == null
                || !stationSceneHash.exists
                || !AuditionPvSha256.IsSha256(stationSceneHash.sha256))
            {
                throw new InvalidOperationException(
                    "G06 could not hash the Station product scene before capture.");
            }

            AuditionPvStationPhase2SummonCounterOutput output = null;
            try
            {
                output = AuditionPvStationPhase2SummonCounterCapture.ReserveNewOutput(
                    startedAtUtc,
                    git);
                var state = new PersistedRunnerState
                {
                    schema = RunnerSchema,
                    phase = RunnerPhase.AwaitingPlayMode.ToString(),
                    batchMode = batchMode,
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
                    stationSceneSha256AtStart = stationSceneHash.sha256,
                    runtimeProof = new RuntimeProof()
                };
                string statePath = Path.Combine(output.outputDirectory, StateFileName);
                SaveState(statePath, state);
                SessionState.SetString(SessionOwnerKey, SessionOwnerValue);
                SessionState.SetString(SessionStatePathKey, statePath);
                SessionState.SetBool(SessionBatchKey, batchMode);
                SessionState.SetBool(SessionActiveKey, true);

                EditorSceneManager.OpenScene(
                    AuditionPvStationPhase2SummonCounterCapture.StationScenePath,
                    OpenSceneMode.Single);
                if (SceneManager.GetActiveScene().isDirty)
                {
                    throw new InvalidOperationException(
                        "Fresh Station scene became dirty before entering Play Mode.");
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

            switch (change)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    ScheduleResume();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    ScheduleResume();
                    break;
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
                string recoveryOutputDirectory = string.IsNullOrWhiteSpace(statePath)
                    ? string.Empty
                    : Path.GetDirectoryName(statePath) ?? string.Empty;
                TryWriteFailureArtifact(
                    recoveryOutputDirectory,
                    "state-load",
                    exception,
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

                if (phase == RunnerPhase.Recording
                    && activeBehaviour == null)
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
                    "Play Mode exited before the G06 Recorder session reported completion.";
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
                    AuditionPvStationPhase2SummonCounterCapture.StationScenePath,
                    StringComparison.Ordinal))
            {
                NotifyPlayModeFinished(
                    statePath,
                    state,
                    null,
                    new InvalidOperationException(
                        "G06 entered Play Mode without the fresh Station scene."));
                return;
            }

            state.phase = RunnerPhase.Recording.ToString();
            SaveState(statePath, state);
            var root = new GameObject("[AuditionPV_G06_GoldenRunner]")
            {
                hideFlags = HideFlags.DontSave
            };
            SceneManager.MoveGameObjectToScene(root, scene);
            activeBehaviour = root.AddComponent<
                AuditionPvStationPhase2SummonCounterGoldenRunnerBehaviour>();
            activeBehaviour.Begin(statePath, state.outputDirectory, state);
        }

        private static void FailAfterUnexpectedDomainReload(
            string statePath,
            PersistedRunnerState state)
        {
            var failure = new InvalidOperationException(
                "A script/domain reload interrupted G06 while Recorder was active; discard this take.");
            NotifyPlayModeFinished(statePath, state, state.runtimeProof, failure);
        }

        internal static void NotifyPlayModeFinished(
            string statePath,
            PersistedRunnerState state,
            RuntimeProof runtimeProof,
            Exception failure)
        {
            activeBehaviour = null;
            state.runtimeProof = runtimeProof ?? state.runtimeProof ?? new RuntimeProof();
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
            bool success = false;
            Exception failure = null;
            try
            {
                AuditionPvStationPhase2SummonCounterCapture
                    .ReopenProductSceneAfterPlayMode();
                Scene reopened = SceneManager.GetActiveScene();
                if (!reopened.IsValid()
                    || !reopened.isLoaded
                    || reopened.isDirty
                    || !string.Equals(
                        reopened.path,
                        AuditionPvStationPhase2SummonCounterCapture.StationScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G06 did not reopen an unmodified Station product scene after Play Mode.");
                }

                if (!string.IsNullOrWhiteSpace(state.failure))
                {
                    throw new InvalidOperationException(
                        "G06 PlayMode recording failed. See the failure artifact.\n"
                        + state.failure);
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
                    state.runtimeProof);
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
                        "[AuditionPV] G06 Station summon-counter golden source passed: "
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
                        "G06 golden capture failed",
                        failure.Message,
                        "OK");
                }
            }
        }

        private static void FinalizeSuccessfulCapture(
            string statePath,
            PersistedRunnerState state)
        {
            RuntimeProof proof = state.runtimeProof
                ?? throw new InvalidOperationException("G06 runtime proof is missing.");
            ValidateRuntimeProof(proof);

            string frameDirectory = Path.Combine(
                state.outputDirectory,
                "frames",
                AuditionPvStationPhase2SummonCounterCapture.ShotId);
            string evidenceDirectory = Path.Combine(
                state.outputDirectory,
                EvidenceFolderName);
            string warmupPath = RemapRawFrames(frameDirectory, evidenceDirectory);
            ValidatePngFile(
                warmupPath,
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height);
            ValidateLogicalFrameSequence(frameDirectory);
            for (int frame =
                     AuditionPvStationPhase2SummonCounterCapture.FirstFrame;
                frame <= AuditionPvStationPhase2SummonCounterCapture.LastFrame;
                frame++)
            {
                ValidatePngFile(
                    Path.Combine(
                        frameDirectory,
                        AuditionPvStationPhase2SummonCounterCapture.FrameFileName(frame)),
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height);
            }

            SequenceVisualMetrics visualMetrics = AnalyzeVisualSequence(frameDirectory);
            ValidateVisualSequence(visualMetrics);
            ScreenDeltaMetrics screenDelta = AnalyzeScreenDelta(frameDirectory);
            ValidateScreenDelta(screenDelta);
            ScreenDeltaMetrics counterDelta = AnalyzeCounterDelta(frameDirectory);
            ValidateCounterDelta(counterDelta);
            proof.visualMetrics = visualMetrics;
            proof.screenDelta = screenDelta;
            proof.counterDelta = counterDelta;
            proof.warmupEvidencePath = warmupPath.Replace('\\', '/');
            proof.warmupEvidenceSha256 = AuditionPvSha256.FileHash(warmupPath);

            CopyBaselines(state, frameDirectory);
            AuditionPvGitSnapshot gitAtEnd = AuditionPvEnvironmentProbe.ReadGitSnapshot();
            ValidateStableGitSnapshot(CreateGitSnapshot(state), gitAtEnd);
            string[] dependencyPathsAtEnd = CollectCaptureDependencyPaths();
            if (!state.dependencyPaths.SequenceEqual(
                    dependencyPathsAtEnd,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "G06 dependency path set changed while recording.");
            }

            AuditionPvDependencyHash[] dependenciesAtEnd =
                AuditionPvEnvironmentProbe.HashDependencies(dependencyPathsAtEnd);
            ValidateStableDependencies(state.dependencyHashesAtStart, dependenciesAtEnd);
            AuditionPvDependencyHash stationSceneHash = dependenciesAtEnd.FirstOrDefault(
                dependency => string.Equals(
                    dependency.path,
                    AuditionPvStationPhase2SummonCounterCapture.StationScenePath,
                    StringComparison.OrdinalIgnoreCase));
            if (stationSceneHash == null
                || !string.Equals(
                    stationSceneHash.sha256,
                    state.stationSceneSha256AtStart,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The Station scene hash changed while G06 was recording.");
            }

            string proofPath = Path.Combine(evidenceDirectory, RuntimeProofFileName);
            WriteJsonNew(proofPath, new RuntimeProofArtifact
            {
                schema = RuntimeProofSchema,
                captureId = state.captureId,
                mapping =
                    "Recorder raw0 is preserved warm-up evidence; raw1..raw360 map to logical f0..f359.",
                productScreenProfile =
                    "authored product profile used unchanged: enabled=true, domain=.14, "
                    + "invert=.015, edge=.18, glitch=.03, duration=.42s.",
                summonCounterContract =
                    "authored Slot1 cost=200, full EN 300->100, tier=2, "
                    + "screen intercept=1, automatic counter damage=29.44.",
                runtime = proof
            });

            DateTime startedAtUtc = DateTime.Parse(
                state.startedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
            AuditionPvTestResult[] results = CreateTestResults(
                state,
                proof,
                proofPath,
                startedAtUtc);
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    new[]
                    {
                        AuditionPvStationPhase2SummonCounterCapture
                            .CreateShotManifestEntry()
                    },
                    AuditionPvStationPhase2SummonCounterCapture
                        .CreateBaselineManifestEntries(),
                    results,
                    createdAtUtc: startedAtUtc,
                    gitSnapshot: CreateGitSnapshot(state),
                    engineSnapshot: RestoreEngine(state.engine),
                    dependencyHashSnapshot: state.dependencyHashesAtStart);
            string manifestPath = AuditionPvCaptureManifestWriter.WriteNew(manifest);
            ValidateManifestRoundTrip(manifestPath, state.captureId);

            state.phase = RunnerPhase.Complete.ToString();
            state.runtimeProof = proof;
            SaveState(statePath, state);
        }

        internal static void ValidateRuntimeProof(RuntimeProof proof)
        {
            if (!proof.directorCompleted
                || proof.lastLogicalFrame
                    != AuditionPvStationPhase2SummonCounterCapture.LastFrame
                || proof.presentedFrameCount
                    != AuditionPvStationPhase2SummonCounterCapture.ExpectedFrameCount
                || !proof.presentedFramesExact
                || !proof.presentationClockExact
                || proof.perfectDodgeCount != 1
                || proof.firedProjectileCount <= 0
                || !proof.usedActualCrushNetPattern
                || !proof.impactAppliedOrBlocked
                || !proof.impactProjectileInactive
                || proof.damageBlockedObservationCount != 1
                || proof.damageModifyingObservationCount != 0
                || !proof.playerHealthUnchanged
                || !proof.productScreenProfileActive
                || proof.bossRiskAtFirstFrame < 0.58f
                || proof.bossRiskAtFireFrame < 0.86f
                || proof.bossRiskAtImpactFrame < 0.88f
                || !proof.screenCueRequested
                || !proof.screenCueActiveAtBaselineFrame
                || !proof.cameraCueRequested
                || !proof.exactHudRenderable
                || !proof.exactHudResources
                || !proof.exactEnergyBinding
                || proof.hudMagazineSize <= 0
                || proof.hudAmmo != proof.hudMagazineSize
                || proof.hudEnergyMaxMana <= 0f
                || Mathf.Abs(
                    proof.hudEnergyMana
                        - AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredEnergyAfterUse) > 0.001f
                || Mathf.Abs(
                    proof.summonEnergyBeforeUse - proof.hudEnergyMaxMana)
                    > 0.001f
                || Mathf.Abs(
                    proof.summonEnergyAfterUse
                        - AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredEnergyAfterUse) > 0.001f
                || proof.summonSpentTier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier
                || proof.summonUseCountDelta != 1
                || proof.summonInterceptCountDelta != 1
                || proof.summonUsedEventCount != 1
                || proof.summonBlockedEventCount != 1
                || proof.screenInterceptEventCount != 1
                || !proof.uniqueSummonPressureScreenObserved
                || proof.screenFirstObservedFrame
                    < AuditionPvStationPhase2SummonCounterCapture
                        .ScreenObservationFirstFrame
                || proof.screenFirstObservedFrame
                    > AuditionPvStationPhase2SummonCounterCapture
                        .ScreenObservationLastFrame
                || proof.summonPressureScreenTier
                    != AuditionPvStationPhase2SummonCounterCapture
                        .AuthoredSummonTier
                || proof.summonPressureScreenRemainingIntercepts != 1
                || proof.retainedProjectileCountBeforeIntercept != 6
                || !proof.retainedProjectileIdentitySetExact
                || !proof.retainedProjectileImpactApplied
                || !proof.retainedProjectileInactive
                || proof.activeCounterProjectileCountAfterIntercept != 1
                || proof.bossDamageEventCount < 1
                || proof.bossAllyDamageEventCount < 1
                || proof.bossCounterDamageEventCount != 1
                || proof.counterProjectileDamageAppliedCount != 1
                || proof.bossCounterDamageFrame
                    != proof.counterProjectileDamageAppliedFrame
                || proof.bossCounterDamageFrame
                    < AuditionPvStationPhase2SummonCounterCapture
                        .CounterHitFirstFrame
                || proof.bossCounterDamageFrame
                    > AuditionPvStationPhase2SummonCounterCapture
                        .CounterHitLastFrame
                || Mathf.Abs(
                    proof.authoredCounterDamage
                        - AuditionPvStationPhase2SummonCounterCapture
                            .AuthoredCounterDamage) > 0.001f
                || Mathf.Abs(
                    proof.bossCounterDamageAmount
                        - proof.authoredCounterDamage) > 0.001f
                || Mathf.Abs(
                    proof.bossCounterHealthDelta
                        - proof.authoredCounterDamage) > 0.001f
                || !proof.fixedDeltaTimeExact
                || !proof.recorderAutoStoppedAfterLastFrame
                || proof.recorderWarmupEndOfFrameCount != 2
                || !proof.recorderPaddingActiveAtLogicalFrameZero
                || !proof.stateRestored
                || !proof.screenProfileRestored
                || !proof.fixedDeltaTimeRestored
                || !proof.captureInputLocksReleased
                || !proof.captureHudStateRestored
                || !proof.captureEventsReleased
                || !proof.captureSummonArtifactsReleased
                || !proof.bossCompositionRestored
                || !proof.presentationClockReleased
                || proof.cadenceSuspensionCountAfterRestore != 0)
            {
                throw new InvalidOperationException(
                    "G06 runtime proof does not satisfy the exact gameplay, real Slot1 "
                    + "intercept/counter, Recorder, HUD, boss composition, screen, or "
                    + "restoration contract.");
            }
        }

        private static void CopyBaselines(
            PersistedRunnerState state,
            string frameDirectory)
        {
            foreach (AuditionPvBaselineManifestEntry baseline in
                     AuditionPvStationPhase2SummonCounterCapture
                         .CreateBaselineManifestEntries())
            {
                string source = Path.Combine(
                    frameDirectory,
                    AuditionPvStationPhase2SummonCounterCapture.FrameFileName(
                        baseline.sourceFrame));
                string destination = Path.Combine(
                    state.baselineDirectory,
                    baseline.fileName);
                CopyNew(source, destination);
                if (!string.Equals(
                        AuditionPvSha256.FileHash(source),
                        AuditionPvSha256.FileHash(destination),
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"G06 baseline '{baseline.id}' is not a byte-exact frame copy.");
                }
            }
        }

        private static SequenceVisualMetrics AnalyzeVisualSequence(
            string frameDirectory)
        {
            var metrics = new SequenceVisualMetrics();
            for (int frame =
                     AuditionPvStationPhase2SummonCounterCapture.FirstFrame;
                frame <= AuditionPvStationPhase2SummonCounterCapture.LastFrame;
                frame++)
            {
                string path = Path.Combine(
                    frameDirectory,
                    AuditionPvStationPhase2SummonCounterCapture.FrameFileName(frame));
                Texture2D texture = LoadPng(path);
                try
                {
                    NativeArray<Color32> pixels =
                        texture.GetRawTextureData<Color32>();
                    long frameSamples = 0;
                    long frameBlack = 0;
                    long frameMagenta = 0;
                    int minimumLuma = 255;
                    int maximumLuma = 0;
                    int hudAccents = 0;
                    const int Step = 32;
                    for (int y = Step / 2; y < texture.height; y += Step)
                    {
                        for (int x = Step / 2; x < texture.width; x += Step)
                        {
                            Color32 pixel = pixels[y * texture.width + x];
                            int luma = (pixel.r * 54 + pixel.g * 183 + pixel.b * 19) >> 8;
                            minimumLuma = Math.Min(minimumLuma, luma);
                            maximumLuma = Math.Max(maximumLuma, luma);
                            bool black = pixel.r <= 10
                                && pixel.g <= 10
                                && pixel.b <= 10;
                            bool missingShaderMagenta = pixel.r >= 245
                                && pixel.g <= 12
                                && pixel.b >= 245;
                            if (black)
                            {
                                frameBlack++;
                            }

                            if (missingShaderMagenta)
                            {
                                frameMagenta++;
                            }

                            bool hudBand = y < texture.height * 0.20f
                                || y > texture.height * 0.72f;
                            bool brightOrCyan = luma >= 178
                                || pixel.g >= 150
                                    && pixel.b >= 160
                                    && pixel.r <= 170;
                            if (frame == 0 && hudBand && brightOrCyan)
                            {
                                hudAccents++;
                            }

                            frameSamples++;
                        }
                    }

                    metrics.sampleCount += frameSamples;
                    metrics.blackSampleCount += frameBlack;
                    metrics.magentaSampleCount += frameMagenta;
                    double blackRatio = frameBlack / (double)frameSamples;
                    double magentaRatio = frameMagenta / (double)frameSamples;
                    if (blackRatio < MaximumSequenceBlackRatio)
                    {
                        metrics.healthyFrameCount++;
                    }

                    if (magentaRatio > 0d)
                    {
                        metrics.magentaAffectedFrameCount++;
                    }

                    metrics.maximumFrameMagentaRatio = Math.Max(
                        metrics.maximumFrameMagentaRatio,
                        magentaRatio);
                    metrics.minimumSampledLuma = Math.Min(
                        metrics.minimumSampledLuma,
                        minimumLuma);
                    metrics.maximumSampledLuma = Math.Max(
                        metrics.maximumSampledLuma,
                        maximumLuma);
                    if (frame == 0)
                    {
                        metrics.frameZeroHudAccentSamples = hudAccents;
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }

            metrics.blackRatio = metrics.blackSampleCount
                / (double)Math.Max(1L, metrics.sampleCount);
            metrics.magentaRatio = metrics.magentaSampleCount
                / (double)Math.Max(1L, metrics.sampleCount);
            return metrics;
        }

        internal static void ValidateVisualSequence(SequenceVisualMetrics metrics)
        {
            int minimumHealthyFrames =
                AuditionPvStationPhase2SummonCounterCapture.ExpectedFrameCount
                * MinimumHealthyFramePercent / 100;
            if (metrics == null
                || metrics.sampleCount <= 0
                || metrics.blackRatio >= MaximumSequenceBlackRatio
                || metrics.healthyFrameCount < minimumHealthyFrames
                || metrics.maximumSampledLuma - metrics.minimumSampledLuma < 32)
            {
                throw new InvalidOperationException(
                    "G06 black/flat-frame sanity failed: "
                    + $"black={metrics?.blackRatio ?? 1d:P2}, "
                    + $"healthy={metrics?.healthyFrameCount ?? 0}/"
                    + AuditionPvStationPhase2SummonCounterCapture.ExpectedFrameCount
                    + $", luma={metrics?.minimumSampledLuma ?? 0}..{metrics?.maximumSampledLuma ?? 0}.");
            }

            if (metrics.magentaRatio >= MaximumSequenceMagentaRatio
                || metrics.maximumFrameMagentaRatio >= MaximumFrameMagentaRatio)
            {
                throw new InvalidOperationException(
                    "G06 missing-shader magenta sanity failed: "
                    + $"global={metrics.magentaRatio:P3}, "
                    + $"maxFrame={metrics.maximumFrameMagentaRatio:P3}.");
            }

            if (metrics.frameZeroHudAccentSamples < MinimumHudAccentSamples)
            {
                throw new InvalidOperationException(
                    "G06 f0 lacks enough rendered HUD-edge luminance/chroma evidence: "
                    + metrics.frameZeroHudAccentSamples);
            }
        }

        private static ScreenDeltaMetrics AnalyzeScreenDelta(string frameDirectory)
        {
            string beforePath = Path.Combine(
                frameDirectory,
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(
                    AuditionPvStationPhase2SummonCounterCapture.ImpactFrame));
            string afterPath = Path.Combine(
                frameDirectory,
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(
                    AuditionPvStationPhase2SummonCounterCapture.Bl06SourceFrame));
            Texture2D before = LoadPng(beforePath);
            Texture2D after = LoadPng(afterPath);
            try
            {
                return EvaluateScreenDelta(
                    before.GetPixels32(),
                    after.GetPixels32(),
                    sampleStride: 32);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(before);
                UnityEngine.Object.DestroyImmediate(after);
            }
        }

        private static ScreenDeltaMetrics AnalyzeCounterDelta(
            string frameDirectory)
        {
            string beforePath = Path.Combine(
                frameDirectory,
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(
                    AuditionPvStationPhase2SummonCounterCapture
                        .ScreenObservationLastFrame));
            string afterPath = Path.Combine(
                frameDirectory,
                AuditionPvStationPhase2SummonCounterCapture.FrameFileName(
                    AuditionPvStationPhase2SummonCounterCapture
                        .Bl07SourceFrame));
            Texture2D before = LoadPng(beforePath);
            Texture2D after = LoadPng(afterPath);
            try
            {
                return EvaluateScreenDelta(
                    before.GetPixels32(),
                    after.GetPixels32(),
                    sampleStride: 32);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(before);
                UnityEngine.Object.DestroyImmediate(after);
            }
        }

        private static Texture2D LoadPng(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true)
            {
                name = "G06Validation_" + Path.GetFileNameWithoutExtension(path)
            };
            if (!ImageConversion.LoadImage(texture, bytes, markNonReadable: false)
                || texture.width != AuditionPvCaptureContract.Width
                || texture.height != AuditionPvCaptureContract.Height)
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidDataException(
                    "Unity could not decode the exact QHD G06 PNG: " + path);
            }

            return texture;
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
            return new[]
            {
                Passed(
                    "recorder",
                    "raw-warmup-and-logical-frame-mapping",
                    duration,
                    "Recorder 5.1.6 QHD60 raw 0..360 complete; raw0 preserved; raw1..360 mapped to logical 0..359; f359 flushed before stop.",
                    proof.warmupEvidencePath),
                Passed(
                    "product-state",
                    "real-station-phase2-perfect-dodge-slot1-counter",
                    duration,
                    $"threshold->skip->Phase2; Begin f1/fire f71/dodge f186/impact f188; "
                    + $"Slot1 release/queue/relock f221/f222/f223; retained intercept f250; "
                    + $"counter hit f{proof.bossCounterDamageFrame}; "
                    + $"boss risk={proof.bossRiskAtFirstFrame:F3}/{proof.bossRiskAtFireFrame:F3}/{proof.bossRiskAtImpactFrame:F3}; "
                    + $"perfect={proof.perfectDodgeCount}; blocked={proof.damageBlockedObservationCount}; "
                    + $"summon tier={proof.summonSpentTier}, EN={proof.summonEnergyBeforeUse:F0}->{proof.summonEnergyAfterUse:F0}, "
                    + $"counter={proof.bossCounterDamageAmount:F2}, HP unchanged={proof.playerHealthUnchanged}.",
                    proofPath),
                Passed(
                    "render",
                    "png-hud-and-visual-sanity",
                    duration,
                    $"360 exact 2560x1440 PNGs; black={proof.visualMetrics.blackRatio:P3}; magenta={proof.visualMetrics.magentaRatio:P3}; HUD f0 accents={proof.visualMetrics.frameZeroHudAccentSamples}.",
                    Path.Combine(state.outputDirectory, "frames", AuditionPvStationPhase2SummonCounterCapture.ShotId).Replace('\\', '/')),
                Passed(
                    "render",
                    "perfect-dodge-screen-domain-f189",
                    duration,
                    $"Authored product profile enabled=.14/.015/.18/.03, duration=.42s was unchanged; "
                    + $"f188->f189 mean RGB delta={proof.screenDelta.meanAbsoluteRgb:F3}, "
                    + $"changed={proof.screenDelta.changedSampleRatio:P2}.",
                    Path.Combine(state.baselineDirectory, AuditionPvStationPhase2SummonCounterCapture.Bl06FileName).Replace('\\', '/')),
                Passed(
                    "render",
                    "slot1-screen-intercept-counter-f251",
                    duration,
                    $"f249->f251 mean RGB delta={proof.counterDelta.meanAbsoluteRgb:F3}, "
                    + $"changed={proof.counterDelta.changedSampleRatio:P2}; BL07 is a byte-exact f251 copy.",
                    Path.Combine(state.baselineDirectory, AuditionPvStationPhase2SummonCounterCapture.Bl07FileName).Replace('\\', '/')),
                Passed(
                    "provenance",
                    "git-dependencies-and-station-scene-stable",
                    duration,
                    $"Clean Git HEAD and {state.dependencyHashesAtStart.Length} dependency hashes remained stable; Station SHA-256={state.stationSceneSha256AtStart}.",
                    proofPath),
                Passed(
                    "lifecycle",
                    "state-restored-and-product-scene-reopened",
                    duration,
                    "Recorder stopped; fixedDelta, PresentationClock, input/events, summon artifacts, cadence, screen profile, and boss composition restored; "
                    + "Play Mode exited, and the unsaved product scene was reopened clean.",
                    proofPath)
            };
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
            AuditionPvShotManifestEntry shot = manifest.shots.Single();
            AuditionPvBaselineManifestEntry bl03 = manifest.baselines.Single(value =>
                value.id == "bl03");
            AuditionPvBaselineManifestEntry bl06 = manifest.baselines.Single(value =>
                value.id == "bl06");
            AuditionPvBaselineManifestEntry bl07 = manifest.baselines.Single(value =>
                value.id == "bl07");
            if (!string.Equals(manifest.captureId, captureId, StringComparison.Ordinal)
                || shot.startFrame != 0
                || shot.endFrame != 359
                || shot.expectedFrameCount != 360
                || !string.Equals(shot.hudMode, "hud-on", StringComparison.Ordinal)
                || bl03.sourceFrame != 0
                || bl06.sourceFrame != 189
                || bl07.sourceFrame != 251
                || !shot.notes.Contains("QueueSummonSlot1 f222", StringComparison.Ordinal)
                || !shot.notes.Contains("automatic 29.44 counter", StringComparison.Ordinal)
                || !shot.notes.Contains(".14/.015/.18/.03", StringComparison.Ordinal)
                || !shot.notes.Contains(
                    "without a capture-time visual override",
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "G06 manifest did not round-trip its exact logical-frame, HUD, baseline, and product screen contract.");
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
                    $"G06 requires {expectedCount} exact PNGs in '{directory}'; found {files.Length}.");
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
                        $"G06 frame sequence is not contiguous at index {index}.");
                }
            }
        }

        private static void MoveNew(string source, string destination)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("G06 remap source is missing.", source);
            }

            if (File.Exists(destination) || Directory.Exists(destination))
            {
                throw new IOException(
                    "G06 never overwrites a remap destination: " + destination);
            }

            string parent = Path.GetDirectoryName(destination)
                ?? throw new InvalidOperationException("G06 remap destination has no parent.");
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
                throw new FileNotFoundException("G06 copy source is missing.", source);
            }

            if (File.Exists(destination) || Directory.Exists(destination))
            {
                throw new IOException(
                    "G06 never overwrites a baseline/evidence destination: "
                    + destination);
            }

            string parent = Path.GetDirectoryName(destination)
                ?? throw new InvalidOperationException("G06 copy destination has no parent.");
            Directory.CreateDirectory(parent);
            File.Copy(source, destination, overwrite: false);
        }

        private static string RequireDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("G06 directory must not be empty.", nameof(path));
            }

            string normalized = Path.GetFullPath(path);
            if (!Directory.Exists(normalized))
            {
                throw new DirectoryNotFoundException(normalized);
            }

            return normalized;
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
                ?? throw new InvalidOperationException("Could not resolve Unity project root.");
            return Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath));
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
                    "G06 runner state contains an unknown phase: " + value);
            }

            return phase;
        }

        private static PersistedRunnerState LoadState(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException(
                    "G06 SessionState does not point to a persisted runner state.",
                    path);
            }

            PersistedRunnerState state =
                JsonUtility.FromJson<PersistedRunnerState>(File.ReadAllText(path));
            if (state == null
                || !string.Equals(state.schema, RunnerSchema, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(state.outputDirectory)
                || string.IsNullOrWhiteSpace(state.captureId))
            {
                throw new InvalidDataException("G06 persisted runner state is invalid.");
            }

            state.dependencyPaths ??= Array.Empty<string>();
            state.dependencyHashesAtStart ??= Array.Empty<AuditionPvDependencyHash>();
            state.runtimeProof ??= new RuntimeProof();
            return state;
        }

        private static void SaveState(string path, PersistedRunnerState state)
        {
            string normalizedPath = Path.GetFullPath(path);
            string parent = Path.GetDirectoryName(normalizedPath)
                ?? throw new InvalidOperationException("G06 state path has no parent.");
            Directory.CreateDirectory(parent);
            string temporaryPath = normalizedPath
                + ".tmp-"
                + Guid.NewGuid().ToString("N");
            File.WriteAllText(
                temporaryPath,
                JsonUtility.ToJson(state, true) + Environment.NewLine,
                new UTF8Encoding(false));
            try
            {
                if (File.Exists(normalizedPath))
                {
                    File.Replace(temporaryPath, normalizedPath, null);
                }
                else
                {
                    File.Move(temporaryPath, normalizedPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        private static void WriteJsonNew<T>(string path, T value)
        {
            string parent = Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("G06 JSON artifact has no parent.");
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
            RuntimeProof runtimeProof)
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
                        "g06_capture_failure_"
                        + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture)
                        + ".json");
                }

                WriteJsonNew(path, new FailureArtifact
                {
                    schema = FailureSchema,
                    createdAtUtc = DateTime.UtcNow.ToString("O"),
                    phase = phase ?? string.Empty,
                    exception = exception?.ToString() ?? "unknown failure",
                    runtime = runtimeProof
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
                throw new InvalidDataException("G06 engine provenance is missing.");
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
            public int perfectDodgeCount;
            public int firedProjectileCount;
            public bool usedActualCrushNetPattern;
            public bool impactAppliedOrBlocked;
            public bool impactProjectileInactive;
            public int damageBlockedObservationCount;
            public int damageModifyingObservationCount;
            public bool playerHealthUnchanged;
            public bool cameraCueRequested;
            public bool screenCueRequested;
            public bool screenCueActiveAtBaselineFrame;
            public bool productScreenProfileActive;
            public float bossRiskAtFirstFrame = -1f;
            public float bossRiskAtFireFrame = -1f;
            public float bossRiskAtImpactFrame = -1f;
            public bool exactHudRenderable;
            public bool exactHudResources;
            public bool exactEnergyBinding;
            public int hudAmmo = -1;
            public int hudMagazineSize = -1;
            public float hudEnergyMana = -1f;
            public float hudEnergyMaxMana = -1f;
            public float summonEnergyBeforeUse = -1f;
            public float summonEnergyAfterUse = -1f;
            public int summonSpentTier;
            public int summonUseCountDelta;
            public int summonInterceptCountDelta;
            public int summonUsedEventCount;
            public int summonBlockedEventCount;
            public int screenInterceptEventCount;
            public int screenFirstObservedFrame = -1;
            public int summonPressureScreenTier;
            public int summonPressureScreenRemainingIntercepts;
            public bool uniqueSummonPressureScreenObserved;
            public int retainedProjectileCountBeforeIntercept;
            public bool retainedProjectileIdentitySetExact;
            public bool retainedProjectileImpactApplied;
            public bool retainedProjectileInactive;
            public int activeCounterProjectileCountAfterIntercept;
            public int bossDamageEventCount;
            public int bossAllyDamageEventCount;
            public int bossCounterDamageEventCount;
            public int bossCounterDamageFrame = -1;
            public int counterProjectileDamageAppliedCount;
            public int counterProjectileDamageAppliedFrame = -1;
            public float authoredCounterDamage = -1f;
            public float bossCounterDamageAmount = -1f;
            public float bossCounterHealthDelta = -1f;
            public bool fixedDeltaTimeExact;
            public int recorderWarmupEndOfFrameCount;
            public bool recorderPaddingActiveAtLogicalFrameZero;
            public float recorderCaptureDeltaTimeAtLogicalFrameZero;
            public bool recorderAutoStoppedAfterLastFrame;
            public bool stateRestored;
            public bool screenProfileRestored;
            public bool fixedDeltaTimeRestored;
            public bool captureInputLocksReleased;
            public bool captureHudStateRestored;
            public bool captureEventsReleased;
            public bool captureSummonArtifactsReleased;
            public bool bossCompositionRestored;
            public bool presentationClockReleased;
            public int cadenceSuspensionCountAfterRestore = -1;
            public string warmupEvidencePath = string.Empty;
            public string warmupEvidenceSha256 = string.Empty;
            public SequenceVisualMetrics visualMetrics;
            public ScreenDeltaMetrics screenDelta;
            public ScreenDeltaMetrics counterDelta;
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
            public int frameZeroHudAccentSamples;
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
        private sealed class RuntimeProofArtifact
        {
            public string schema = string.Empty;
            public string captureId = string.Empty;
            public string mapping = string.Empty;
            public string productScreenProfile = string.Empty;
            public string summonCounterContract = string.Empty;
            public RuntimeProof runtime;
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
    /// Executes before the product director so logical f0 is armed in early
    /// Update after Recorder has completed both resolution warm-up end frames.
    /// </summary>
    [DefaultExecutionOrder(-32500)]
    public sealed class AuditionPvStationPhase2SummonCounterGoldenRunnerBehaviour
        : MonoBehaviour
    {
        private const double ShotTimeoutSeconds = 60d;

        private string statePath;
        private string outputDirectory;
        private AuditionPvStationPhase2SummonCounterGoldenRunner.PersistedRunnerState state;
        private AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof proof;
        private AuditionPvStationPhase2SummonCounterDirector director;
        private AuditionPvRecorderSettingsBundle recorderSettings;
        private RecorderController recorderController;
        private Exception updateFailure;
        private bool armLogicalFrameZero;
        private bool beganLogicalShot;
        private bool notified;
        private bool cleaningUp;
        private int nextPresentedFrame;

        internal void Begin(
            string persistedStatePath,
            string captureOutputDirectory,
            AuditionPvStationPhase2SummonCounterGoldenRunner.PersistedRunnerState
                persistedState)
        {
            statePath = persistedStatePath;
            outputDirectory = captureOutputDirectory;
            state = persistedState;
            proof = persistedState.runtimeProof
                ?? new AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof();
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
                if (Time.timeScale <= 0f)
                {
                    throw new InvalidOperationException(
                        "Recorder did not restore gameplay time before the armed logical f0 Update.");
                }

                float minimumDelta = 1f / AuditionPvCaptureContract.Fps;
                proof.recorderCaptureDeltaTimeAtLogicalFrameZero =
                    Time.captureDeltaTime;
                proof.recorderPaddingActiveAtLogicalFrameZero =
                    Time.captureDeltaTime > minimumDelta
                    && Time.captureDeltaTime < minimumDelta + 0.001f;
                if (!proof.recorderPaddingActiveAtLogicalFrameZero)
                {
                    throw new InvalidOperationException(
                        "Recorder cadence padding was not active when logical f0 was armed: "
                        + Time.captureDeltaTime.ToString("F9", CultureInfo.InvariantCulture));
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

            Exception cleanupFailure = CleanupAndCaptureProof();
            failure ??= cleanupFailure;
            NotifyFinished(failure);
        }

        private IEnumerator RunCore()
        {
            director = AuditionPvStationPhase2SummonCounterCapture
                .AttachToFreshActiveScene();
            director.FramePresented += HandleFramePresented;

            IEnumerator preparation = director.PrepareFreshProductState();
            while (true)
            {
                bool moved = preparation.MoveNext();
                if (!moved)
                {
                    break;
                }

                yield return preparation.Current;
            }

            if (!director.IsPrepared)
            {
                throw new InvalidOperationException(
                    "G06 product-state director did not finish preparation.");
            }

            recorderSettings =
                AuditionPvRecorderSettingsFactory.CreateLosslessPngSequence(
                    outputDirectory,
                    AuditionPvStationPhase2SummonCounterCapture.ShotId);
            recorderSettings.controllerSettings.SetRecordModeToFrameInterval(
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawWarmupFrame,
                AuditionPvStationPhase2SummonCounterGoldenRunner.RawLastShotFrame);
            AuditionPvRecorderSettingsFactory.Validate(recorderSettings);
            recorderController = new RecorderController(
                recorderSettings.controllerSettings);
            recorderController.PrepareRecording();
            if (!recorderController.StartRecording())
            {
                throw new InvalidOperationException(
                    "Unity Recorder 5.1.6 rejected the G06 QHD60 image session.");
            }

            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 1;
            yield return new WaitForEndOfFrame();
            proof.recorderWarmupEndOfFrameCount = 2;
            armLogicalFrameZero = true;

            double deadline = Time.realtimeSinceStartupAsDouble + ShotTimeoutSeconds;
            while (!beganLogicalShot && updateFailure == null
                && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
            }

            if (updateFailure != null)
            {
                throw new InvalidOperationException(
                    "G06 could not arm logical frame zero after Recorder warm-up.",
                    updateFailure);
            }

            if (!beganLogicalShot)
            {
                throw new TimeoutException(
                    "G06 timed out before its early-Update logical f0 arm.");
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
                    "G06 product-state director failed during recording.",
                    director.Failure);
            }

            if (!director.IsComplete)
            {
                throw new TimeoutException(
                    "G06 did not complete logical frames 0..359 before timeout.");
            }

            // FramePresented is emitted in the director's LateUpdate, before the
            // Recorder component requests the same rendered frame. One Update
            // boundary lets raw360 (logical f359) finish at end-of-frame without
            // permitting an additional Recorder LateUpdate request.
            yield return null;
            proof.recorderAutoStoppedAfterLastFrame =
                !recorderController.IsRecording();
            if (!proof.recorderAutoStoppedAfterLastFrame)
            {
                throw new InvalidOperationException(
                    "Recorder did not auto-stop after inclusive raw frame 360 / logical f359.");
            }
        }

        private void HandleFramePresented(int frameIndex)
        {
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
                if (director != null)
                {
                    proof.directorCompleted = director.IsComplete;
                    proof.lastLogicalFrame = director.CurrentFrame;
                    proof.perfectDodgeCount = director.PerfectDodgeCount;
                    proof.firedProjectileCount = director.FiredProjectileCount;
                    proof.usedActualCrushNetPattern = director.UsedActualCrushNetPattern;
                    proof.impactAppliedOrBlocked = director.ImpactAppliedOrBlocked;
                    proof.impactProjectileInactive = director.ImpactProjectileInactive;
                    proof.damageBlockedObservationCount =
                        director.DamageBlockedObservationCount;
                    proof.damageModifyingObservationCount =
                        director.DamageModifyingObservationCount;
                    proof.playerHealthUnchanged = director.PlayerHealthUnchanged;
                    proof.cameraCueRequested = director.CameraCueRequested;
                    proof.screenCueRequested = director.ScreenCueRequested;
                    proof.screenCueActiveAtBaselineFrame =
                        director.ScreenCueActiveAtBaselineFrame;
                    proof.productScreenProfileActive =
                        director.ProductScreenProfileActive;
                    proof.bossRiskAtFirstFrame = director.BossRiskAtFirstFrame;
                    proof.bossRiskAtFireFrame = director.BossRiskAtFireFrame;
                    proof.bossRiskAtImpactFrame = director.BossRiskAtImpactFrame;
                    proof.exactHudRenderable = director.IsExactHudRenderable;
                    proof.exactHudResources = director.IsHudResourceStateExact;
                    proof.exactEnergyBinding = director.UsesExactEnergyLadderBinding;
                    proof.hudAmmo = director.HudAmmo;
                    proof.hudMagazineSize = director.HudMagazineSize;
                    proof.hudEnergyMana = director.HudEnergyMana;
                    proof.hudEnergyMaxMana = director.HudEnergyMaxMana;
                    proof.summonEnergyBeforeUse =
                        director.SummonEnergyBeforeUse;
                    proof.summonEnergyAfterUse =
                        director.SummonEnergyAfterUse;
                    proof.summonSpentTier = director.SummonSpentTier;
                    proof.summonUseCountDelta = director.SummonUseCountDelta;
                    proof.summonInterceptCountDelta =
                        director.SummonInterceptCountDelta;
                    proof.summonUsedEventCount =
                        director.SummonUsedEventCount;
                    proof.summonBlockedEventCount =
                        director.SummonBlockedEventCount;
                    proof.screenInterceptEventCount =
                        director.ScreenInterceptEventCount;
                    proof.screenFirstObservedFrame =
                        director.ScreenFirstObservedFrame;
                    proof.summonPressureScreenTier =
                        director.SummonPressureScreenTier;
                    proof.summonPressureScreenRemainingIntercepts =
                        director.SummonPressureScreenRemainingIntercepts;
                    proof.uniqueSummonPressureScreenObserved =
                        director.UniqueSummonPressureScreenObserved;
                    proof.retainedProjectileCountBeforeIntercept =
                        director.RetainedProjectileCountBeforeIntercept;
                    proof.retainedProjectileIdentitySetExact =
                        director.RetainedProjectileIdentitySetExact;
                    proof.retainedProjectileImpactApplied =
                        director.RetainedProjectileImpactApplied;
                    proof.retainedProjectileInactive =
                        director.RetainedProjectileInactive;
                    proof.activeCounterProjectileCountAfterIntercept =
                        director.ActiveCounterProjectileCountAfterIntercept;
                    proof.bossDamageEventCount = director.BossDamageEventCount;
                    proof.bossAllyDamageEventCount =
                        director.BossAllyDamageEventCount;
                    proof.bossCounterDamageEventCount =
                        director.BossCounterDamageEventCount;
                    proof.bossCounterDamageFrame =
                        director.BossCounterDamageFrame;
                    proof.counterProjectileDamageAppliedCount =
                        director.CounterProjectileDamageAppliedCount;
                    proof.counterProjectileDamageAppliedFrame =
                        director.CounterProjectileDamageAppliedFrame;
                    proof.authoredCounterDamage =
                        director.AuthoredCounterDamage;
                    proof.bossCounterDamageAmount =
                        director.BossCounterDamageAmount;
                    proof.bossCounterHealthDelta =
                        director.BossCounterHealthDelta;
                    proof.fixedDeltaTimeExact = director.FixedDeltaTimeExact;
                }
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
                    proof.screenProfileRestored = director.ScreenProfileRestored;
                    proof.fixedDeltaTimeRestored =
                        director.FixedDeltaTimeRestored;
                    proof.captureInputLocksReleased =
                        director.CaptureInputLocksReleased;
                    proof.captureHudStateRestored =
                        director.CaptureHudStateRestored;
                    proof.captureEventsReleased =
                        director.CaptureEventsReleased;
                    proof.captureSummonArtifactsReleased =
                        director.CaptureSummonArtifactsReleased;
                    proof.bossCompositionRestored = director.BossCompositionRestored;
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
            AuditionPvStationPhase2SummonCounterGoldenRunner.NotifyPlayModeFinished(
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
                    "G06 runner was disabled before capture finalization."));
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
