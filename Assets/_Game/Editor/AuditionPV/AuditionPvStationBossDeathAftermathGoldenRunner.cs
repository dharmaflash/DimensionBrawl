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
        internal const string FailureFileName = "g08_capture_failure.json";
        internal const string EvidenceFolderName = "evidence";
        internal const string WarmupEvidenceFileName =
            "recorder_warmup_raw_frame_0000.png";
        internal const int RawWarmupFrame = 0;
        internal const int RawFirstShotFrame = 1;
        internal const int RawLastShotFrame = 360;
        internal const int ExpectedRawFrameCount = 361;
        internal const string RuntimeMappingDescription =
            "Recorder raw0 is preserved warm-up evidence; raw1..raw360 map to logical f0..f359.";
        internal const string RuntimeGameplayDescription =
            "Canonical Corridor product flow; logical f1 one public TryFire; same physical projectile natural f62 impact/Died; f218 product freeze/result; f246 interactive committed SameAs result.";
        internal const string ExpectedUnityVersion = "6000.3.5f2";
        internal const string ExpectedUnityVersionWithRevision =
            "6000.3.5f2 (3fa8bc678cb0)";
        internal const string ExpectedUrpPackageVersion = "17.3.0";
        internal const string ExpectedRenderPipelineAssetPath =
            "Assets/Settings/PC_RPAsset.asset";

        internal const double MaximumSequenceBlackRatio = 0.90d;
        internal const double MaximumSequenceMagentaRatio = 0.005d;
        internal const double MaximumFrameMagentaRatio = 0.02d;
        internal const int MinimumHealthyFramePercent = 90;
        internal const double MinimumImpactMeanAbsoluteRgb = 0.75d;
        internal const double MinimumImpactChangedRatio = 0.01d;
        internal const double MinimumAftermathEvolutionMeanAbsoluteRgb = 0.35d;
        internal const double MinimumResultCutMeanAbsoluteRgb = 3.0d;
        internal const double MinimumResultCutChangedRatio = 0.08d;
        internal const double MinimumResultEntranceMeanAbsoluteRgb = 0.20d;
        internal const int MinimumResultBrightSamples = 500;
        internal const int MinimumResultDarkSamples = 500;
        internal const int MinimumResultCyanSamples = 40;
        // This remains false until an independent reviewer locks thresholds
        // from at least one clean same-HEAD telemetry take. A false sentinel is
        // intentionally unable to publish baselines, proof, ledger, or manifest.
        internal static readonly bool PixelCalibrationLocked = false;

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
            "dimension-brawl.g08-station-boss-death-aftermath.v1";
        private const string RunnerSchema =
            "dimension-brawl.audition-pv.g08-runner-state.v1";
        internal const string RuntimeProofSchema =
            "dimension-brawl.audition-pv.g08-runtime-proof.v1";
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
                for (int logical = 0;
                    logical < AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
                    logical++)
                {
                    string name = AuditionPvStationBossDeathAftermathCapture
                        .FrameFileName(logical);
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
            var builder = new StringBuilder(360 * 84);
            for (int frame = 0;
                frame < AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
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
                    "G08 canonical 360-frame SHA-256 ledger changed.");
            }
        }

        internal static void ValidateRuntimeProof(RuntimeProof proof)
        {
            ValidateRuntimeProofCore(proof, requirePixelCalibration: true);
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
                    != AuditionPvStationBossDeathAftermathCapture.LastFrame
                || proof.presentedFrameCount
                    != AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount
                || !proof.presentedFramesExact
                || !proof.presentationClockExact
                || proof.recorderWarmupEndOfFrameCount != 2
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
                || proof.preShotPlayerPlanarStepDistance > 3f
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
                || proof.bossDeathCameraRequestCount != 1
                || proof.bossDeathCameraVersion <= 0
                || proof.bossDeathCameraInterrupted
                || !proof.bossDeathCameraComplete
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

            if (proof.renderEvidence == null
                || proof.renderEvidence.Length != 3
                || !proof.renderEvidence.Any(value =>
                    value.frame == 62 && value.gameplayCameraExact
                    && value.playerSafeViewport && value.bossSafeViewport
                    && value.playerPixelExtent.x >= 8f
                    && value.playerPixelExtent.y >= 8f
                    && value.bossPixelExtent.x >= 8f
                    && value.bossPixelExtent.y >= 8f)
                || !proof.renderEvidence.Any(value =>
                    value.frame == 116 && value.gameplayCameraExact
                    && value.playerSafeViewport && value.bossSafeViewport
                    && value.playerPixelExtent.x >= 8f
                    && value.playerPixelExtent.y >= 8f
                    && value.bossPixelExtent.x >= 8f
                    && value.bossPixelExtent.y >= 8f)
                || !proof.renderEvidence.Any(value =>
                    value.frame == 246 && value.resultCanvasVisible
                    && value.resultInteractive))
            {
                throw new InvalidOperationException(
                    "G08 final rendered impact/aftermath/result composition proof failed.");
            }

            if (proof.pixelSampleStride <= 0
                || proof.pixelSampleCount <= 0
                || !IsRatio(proof.sequenceBlackRatio)
                || !IsRatio(proof.sequenceMagentaRatio)
                || !IsRatio(proof.maximumFrameMagentaRatio)
                || !IsFiniteInRange(proof.healthyFramePercent, 0d, 100d)
                || !IsNonNegativeFinite(proof.impactMeanAbsoluteRgb)
                || !IsRatio(proof.impactChangedRatio)
                || !IsNonNegativeFinite(proof.aftermathEvolutionMeanAbsoluteRgb)
                || !IsRatio(proof.aftermathEvolutionChangedRatio)
                || !IsNonNegativeFinite(proof.resultCutMeanAbsoluteRgb)
                || !IsRatio(proof.resultCutChangedRatio)
                || !IsNonNegativeFinite(proof.resultEntranceMeanAbsoluteRgb)
                || !IsRatio(proof.resultEntranceChangedRatio)
                || proof.resultBrightSamples < 0
                || proof.resultDarkSamples < 0
                || proof.resultCyanSamples < 0)
            {
                throw new InvalidOperationException(
                    "G08 QHD pixel telemetry is absent or non-finite.");
            }

            if (requirePixelCalibration && !PixelCalibrationLocked)
            {
                throw new G08PixelCalibrationRequiredException(
                    "G08 pixel calibration is not locked. Review the clean failure telemetry, "
                    + "pin independently justified thresholds, and set PixelCalibrationLocked=true.");
            }

            if (requirePixelCalibration)
            {
                ValidateLockedPixelThresholdsForTests(proof);
            }

            if (!AuditionPvSha256.IsSha256(proof.frameHashLedgerSha256)
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
                || proof.resultCutMeanAbsoluteRgb < MinimumResultCutMeanAbsoluteRgb
                || proof.resultCutChangedRatio < MinimumResultCutChangedRatio
                || proof.resultEntranceMeanAbsoluteRgb
                    < MinimumResultEntranceMeanAbsoluteRgb
                || proof.resultBrightSamples < MinimumResultBrightSamples
                || proof.resultDarkSamples < MinimumResultDarkSamples
                || proof.resultCyanSamples < MinimumResultCyanSamples)
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

        private static bool IsRatio(double value)
        {
            return IsFiniteInRange(value, 0d, 1d);
        }

        private static bool IsNonNegativeFinite(double value)
        {
            return IsFiniteInRange(value, 0d, double.MaxValue);
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
            proof.warmupEvidencePath = warmup;
            proof.warmupEvidenceSha256 = AuditionPvSha256.FileHash(warmup);
            proof.bl10Sha256 = FrameHash(frames, 62);
            proof.bl11Sha256 = FrameHash(frames, 116);
            proof.bl12Sha256 = FrameHash(frames, 246);

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

            ValidateRuntimeProof(proof);

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
            WriteJsonNew(proofPath, new RuntimeProofArtifact
            {
                schema = RuntimeProofSchema,
                captureId = state.captureId,
                mapping = RuntimeMappingDescription,
                gameplay = RuntimeGameplayDescription,
                runtime = proof
            });

            DateTime startedAtUtc = DateTime.Parse(
                state.startedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    state.captureId,
                    state.outputRoot,
                    state.outputDirectory,
                    new[]
                    {
                        AuditionPvStationBossDeathAftermathCapture
                            .CreateShotManifestEntry()
                    },
                    AuditionPvStationBossDeathAftermathCapture
                        .CreateBaselineManifestEntries(),
                    CreateTestResults(state, proof, proofPath, startedAtUtc),
                    createdAtUtc: startedAtUtc,
                    gitSnapshot: CreateGitSnapshot(state),
                    engineSnapshot: CopyEngine(state.engine),
                    dependencyHashSnapshot: state.dependencyHashesAtStart);
            ValidateManifestInMemory(manifest, state.captureId);
            ValidateManifestMatchesRecordedState(state, manifest);
            ValidateManifestProofProvenance(manifest, proof);
            // Terminal commit record: no fallible writes may follow this call.
            AuditionPvCaptureManifestWriter.WriteNew(manifest);
        }

        private static void AnalyzeFrames(string frameDirectory, RuntimeProof proof)
        {
            const int sampleStride = 8;
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
            MeasureFrameDelta(frameDirectory, 61, 62,
                out proof.impactMeanAbsoluteRgb,
                out proof.impactChangedRatio);
            MeasureFrameDelta(frameDirectory, 62, 116,
                out proof.aftermathEvolutionMeanAbsoluteRgb,
                out proof.aftermathEvolutionChangedRatio);
            MeasureFrameDelta(frameDirectory, 217, 218,
                out proof.resultCutMeanAbsoluteRgb,
                out proof.resultCutChangedRatio);
            MeasureFrameDelta(frameDirectory, 218, 246,
                out proof.resultEntranceMeanAbsoluteRgb,
                out proof.resultEntranceChangedRatio);
            MeasureResultSurface(frameDirectory, 246, proof);
        }

        private static void MeasureFrameDelta(
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
                long samples = 0;
                long changed = 0;
                double sum = 0d;
                const int stride = 4;
                for (int y = 0; y < first.height; y += stride)
                {
                    int row = y * first.width;
                    for (int x = 0; x < first.width; x += stride)
                    {
                        int index = row + x;
                        int delta = Math.Abs(a[index].r - b[index].r)
                            + Math.Abs(a[index].g - b[index].g)
                            + Math.Abs(a[index].b - b[index].b);
                        sum += delta / 3d;
                        if (delta >= 24)
                        {
                            changed++;
                        }

                        samples++;
                    }
                }

                meanAbsoluteRgb = sum / samples;
                changedRatio = changed / (double)samples;
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
                for (int y = 180; y < 1260; y += 4)
                {
                    int row = y * texture.width;
                    for (int x = 256; x < 2304; x += 4)
                    {
                        Color32 color = pixels[row + x];
                        if (color.r >= 200 && color.g >= 200 && color.b >= 200)
                        {
                            proof.resultBrightSamples++;
                        }

                        if (color.r <= 55 && color.g <= 55 && color.b <= 55)
                        {
                            proof.resultDarkSamples++;
                        }

                        if (color.r <= 120 && color.g >= 100 && color.b >= 140)
                        {
                            proof.resultCyanSamples++;
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

                string expected = baseline.sourceFrame == 62
                    ? proof.bl10Sha256
                    : baseline.sourceFrame == 116
                        ? proof.bl11Sha256
                        : proof.bl12Sha256;
                if (!string.Equals(destinationHash, expected, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "G08 baseline hash differs from its canonical source frame.");
                }
            }
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
                    "Recorder 5.1.6 QHD60 raw0..360 exact; raw0 evidence; raw1..360 -> logical f0..f359.",
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
                    "360 QHD frames pass black/magenta health, impact/aftermath/result deltas, and result-surface color gates.",
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
                    "Recorder 5.1.6 QHD60 raw0..360 exact; raw0 evidence; raw1..360 -> logical f0..f359.",
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
                    "360 QHD frames pass black/magenta health, impact/aftermath/result deltas, and result-surface color gates.",
                    ledgerPath),
                ("cleanup", "scene-global-input-event-restore",
                    "Recorder, manual clock, cadence, transition bootstrap/events, input leases, globals, and edit-mode scene state restored.",
                    proofPath)
            };
            if (roundTrip.testResults == null
                || roundTrip.testResults.Length != expectedTests.Length)
            {
                throw new InvalidOperationException(
                    "G08 manifest test-result count is not exact.");
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
                    "Failure-only: raw/logical frames, runner state, and telemetry may remain; manifest, baselines, success proof, and canonical ledger are absent.",
                pixelCalibrationLocked = PixelCalibrationLocked,
                calibrationRequired = exception is G08PixelCalibrationRequiredException,
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

            string[] paths =
            {
                Path.Combine(outputDirectory, AuditionPvCaptureContract.ManifestFileName),
                Path.Combine(outputDirectory, EvidenceFolderName, RuntimeProofFileName),
                Path.Combine(outputDirectory, EvidenceFolderName, FrameHashLedgerFileName),
                Path.Combine(outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName,
                    AuditionPvStationBossDeathAftermathCapture.Bl10FileName),
                Path.Combine(outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName,
                    AuditionPvStationBossDeathAftermathCapture.Bl11FileName),
                Path.Combine(outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName,
                    AuditionPvStationBossDeathAftermathCapture.Bl12FileName)
            };
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
                        FrameHash(frames, 62),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        artifact.runtime.bl11Sha256,
                        FrameHash(frames, 116),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        artifact.runtime.bl12Sha256,
                        FrameHash(frames, 246),
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
            public double impactMeanAbsoluteRgb;
            public double impactChangedRatio;
            public double aftermathEvolutionMeanAbsoluteRgb;
            public double aftermathEvolutionChangedRatio;
            public double resultCutMeanAbsoluteRgb;
            public double resultCutChangedRatio;
            public double resultEntranceMeanAbsoluteRgb;
            public double resultEntranceChangedRatio;
            public int resultBrightSamples;
            public int resultDarkSamples;
            public int resultCyanSamples;

            public string frameHashLedgerPath = string.Empty;
            public string frameHashLedgerSha256 = string.Empty;
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
            public bool gameplayCameraExact;
            public bool playerSafeViewport;
            public bool bossSafeViewport;
            public bool resultCanvasVisible;
            public bool resultInteractive;
            public Vector3 playerViewport;
            public Vector3 bossViewport;
            public Vector2 playerPixelExtent;
            public Vector2 bossPixelExtent;
        }

        [Serializable]
        private sealed class RuntimeProofArtifact
        {
            public string schema = string.Empty;
            public string captureId = string.Empty;
            public string mapping = string.Empty;
            public string gameplay = string.Empty;
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
            public string retainedArtifacts = string.Empty;
            public bool pixelCalibrationLocked;
            public bool calibrationRequired;
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

            yield return null;
            proof.recorderAutoStoppedAfterLastFrame =
                !recorderController.IsRecording();
            if (!proof.recorderAutoStoppedAfterLastFrame)
            {
                throw new InvalidOperationException(
                    "G08 Recorder did not auto-stop after raw360/logical f359.");
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
        private static readonly int[] EvidenceFrames = { 62, 116, 246 };
        private readonly List<
            AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence>
            evidence = new();
        private AuditionPvStationBossDeathAftermathDirector director;
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
                    gameplayCameraExact = value.gameplayCameraExact,
                    playerSafeViewport = value.playerSafeViewport,
                    bossSafeViewport = value.bossSafeViewport,
                    resultCanvasVisible = value.resultCanvasVisible,
                    resultInteractive = value.resultInteractive,
                    playerViewport = value.playerViewport,
                    bossViewport = value.bossViewport,
                    playerPixelExtent = value.playerPixelExtent,
                    bossPixelExtent = value.bossPixelExtent
                })
                .ToArray();
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
                    frame = frame
                };
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
                return result;
            }

            Camera camera = director.GameplayCamera;
            result.gameplayCameraExact = camera != null
                && camera.isActiveAndEnabled
                && camera.targetTexture == null
                && camera.rect == new Rect(0f, 0f, 1f, 1f)
                && camera.pixelWidth > 0
                && camera.pixelHeight > 0;
            if (!result.gameplayCameraExact)
            {
                return result;
            }

            result.playerSafeViewport = TryResolveSafeViewport(
                camera,
                director.PlayerRendererRoot,
                out Vector3 playerViewport,
                out Vector2 playerPixelExtent);
            result.bossSafeViewport = TryResolveSafeViewport(
                camera,
                director.BossRendererRoot,
                out Vector3 bossViewport,
                out Vector2 bossPixelExtent);
            result.playerViewport = playerViewport;
            result.bossViewport = bossViewport;
            result.playerPixelExtent = playerPixelExtent;
            result.bossPixelExtent = bossPixelExtent;
            return result;
        }

        private static bool TryResolveSafeViewport(
            Camera camera,
            Transform root,
            out Vector3 viewport,
            out Vector2 pixelExtent)
        {
            viewport = default;
            pixelExtent = default;
            if (camera == null || root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer != null
                    && renderer.enabled
                    && !renderer.forceRenderingOff
                    && renderer.shadowCastingMode
                        != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly
                    && renderer.gameObject.activeInHierarchy
                    && (camera.cullingMask & (1 << renderer.gameObject.layer)) != 0)
                .ToArray();
            if (renderers.Length == 0)
            {
                return false;
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (!renderers.Any(renderer =>
                    GeometryUtility.TestPlanesAABB(planes, renderer.bounds)))
            {
                return false;
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            viewport = camera.WorldToViewportPoint(bounds.center);
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3[] corners =
            {
                new(min.x, min.y, min.z), new(min.x, min.y, max.z),
                new(min.x, max.y, min.z), new(min.x, max.y, max.z),
                new(max.x, min.y, min.z), new(max.x, min.y, max.z),
                new(max.x, max.y, min.z), new(max.x, max.y, max.z)
            };
            Vector3[] visible = corners.Select(camera.WorldToViewportPoint)
                .Where(value => value.z > 0f)
                .ToArray();
            if (visible.Length == 0)
            {
                return false;
            }

            float minimumX = visible.Min(value => value.x);
            float maximumX = visible.Max(value => value.x);
            float minimumY = visible.Min(value => value.y);
            float maximumY = visible.Max(value => value.y);
            pixelExtent = new Vector2(
                Mathf.Max(0f, maximumX - minimumX) * camera.pixelWidth,
                Mathf.Max(0f, maximumY - minimumY) * camera.pixelHeight);
            return viewport.z > 0f
                && viewport.x >= 0.03f
                && viewport.x <= 0.97f
                && viewport.y >= 0.03f
                && viewport.y <= 0.97f
                && maximumX > 0f
                && minimumX < 1f
                && maximumY > 0f
                && minimumY < 1f
                && pixelExtent.x >= 8f
                && pixelExtent.y >= 8f;
        }
    }
}
