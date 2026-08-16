using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationBossDeathAftermathGoldenTests
    {
        private const string ShaA =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string ShaB =
            "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        [Test]
        public void Contract_IsExactSixSecondG08WithThreeByteExactBaselines()
        {
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ShotId,
                Is.EqualTo("g08"));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.FirstFrame,
                Is.Zero);
            Assert.That(AuditionPvStationBossDeathAftermathCapture.LastFrame,
                Is.EqualTo(359));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount,
                Is.EqualTo(360));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.FireFrame,
                Is.EqualTo(1));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ImpactFrame,
                Is.EqualTo(62));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.AftermathHeroFrame,
                Is.EqualTo(116));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.ResultRequestFrame,
                Is.EqualTo(218));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.InteractiveResultFrame,
                Is.EqualTo(246));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.PreparedBossHealth,
                Is.EqualTo(12f));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileDamage,
                Is.EqualTo(12f));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.AuthoredProjectileSpeed,
                Is.EqualTo(24f));
            Assert.That(AuditionPvStationBossDeathAftermathCapture.FrameTimeSeconds(359),
                Is.EqualTo(359f / 60f).Within(0.000001f));
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.RawWarmupFrame,
                Is.Zero);
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.RawFirstShotFrame,
                Is.EqualTo(1));
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.RawLastShotFrame,
                Is.EqualTo(360));
            Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner.ExpectedRawFrameCount,
                Is.EqualTo(361));

            AuditionPvShotManifestEntry shot =
                AuditionPvStationBossDeathAftermathCapture.CreateShotManifestEntry();
            Assert.That(shot.scenePath,
                Is.EqualTo(AuditionPvStationBossDeathAftermathCapture.StationScenePath));
            Assert.That(shot.hudMode, Is.EqualTo("hud-on-to-result"));
            Assert.That(shot.notes, Does.Contain("natural"));
            Assert.That(shot.notes, Does.Contain("f62"));
            Assert.That(shot.notes, Does.Contain("f218"));
            Assert.That(shot.notes, Does.Contain("f246"));

            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationBossDeathAftermathCapture
                    .CreateBaselineManifestEntries();
            Assert.That(baselines.Select(value => value.id),
                Is.EqualTo(new[] { "bl10", "bl11", "bl12" }));
            Assert.That(baselines.Select(value => value.sourceFrame),
                Is.EqualTo(new[] { 62, 116, 246 }));
            Assert.That(baselines.Select(value => value.hudMode),
                Is.EqualTo(new[] { "hud-on", "hud-on", "authored-result" }));
            Assert.That(baselines.All(value => value.status == "captured"), Is.True);
        }

        [Test]
        public void RecordingRegion_HasOneTryFireAndNoGameplayPresentationInjection()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            const string Begin = "// RECORDING CONTRACT BEGIN";
            const string End = "// RECORDING CONTRACT END";
            int begin = source.IndexOf(Begin, StringComparison.Ordinal);
            int end = source.IndexOf(End, StringComparison.Ordinal);
            Assert.That(begin, Is.GreaterThanOrEqualTo(0));
            Assert.That(end, Is.GreaterThan(begin));
            string executable = string.Join("\n",
                source.Substring(begin, end - begin)
                    .Split('\n')
                    .Where(line => !line.TrimStart().StartsWith(
                        "//", StringComparison.Ordinal)));
            Assert.That(Count(executable, "ranged.TryFire()"), Is.EqualTo(1));
            foreach (string forbidden in new[]
                     {
                         "TryApplyDamage", "TryApplyImpact", "PlayDeath",
                         "PlayWorldVfx", "RequestBossDeath", ".TryShow(",
                         ".Show(", "PublishCommitted", "RecordTerminal",
                         "transform.position =", ".velocity ="
                     })
            {
                Assert.That(executable, Does.Not.Contain(forbidden), forbidden);
            }
        }

        [Test]
        public void Preparation_UsesPublicCorridorHandoffAndStrictlyNonLethalSetupOnly()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            Assert.That(source, Does.Not.Contain("SceneManager.LoadScene("));
            Assert.That(source, Does.Not.Contain("SceneManager.LoadSceneAsync("));
            Assert.That(source, Does.Not.Contain("TrySealCurrentSegmentForSingleLoad"));
            Assert.That(source, Does.Not.Contain("SetExternalAimPreviewHeld"));
            Assert.That(source, Does.Contain("SetFireHeld(true)"));
            Assert.That(source, Does.Contain("PendingHandoffToken"));
            Assert.That(source, Does.Contain("TrySkipTransition()"));
            Assert.That(source, Does.Contain("ApplyStrictlyNonlethalSetupDamage"));
            Assert.That(Count(source, "bossHealth.TryApplyDamage(damage)"),
                Is.EqualTo(1));
            Assert.That(source, Does.Not.Contain("projectile.transform.position ="));
            Assert.That(source, Does.Not.Contain("firedProjectile.transform.position ="));
            Assert.That(source, Does.Not.Contain("projectile.Velocity ="));
            Assert.That(source, Does.Not.Contain("firedProjectile.Velocity ="));
            Assert.That(source, Does.Not.Contain(".linearVelocity ="));
            Assert.That(source, Does.Not.Contain(".MovePosition("));
            Assert.That(source, Does.Not.Contain(".SetPositionAndRotation("));
            Assert.That(source, Does.Not.Contain("TryApplyImpact("));
            Assert.That(source, Does.Not.Contain("ResolveImpact("));
        }

        [Test]
        public void RawWarmupRemap_IsExactAndRejectsAnyExtraOrMissingFrame()
        {
            string root = NewTempRoot("g08-remap");
            string frames = Path.Combine(root, "frames", "g08");
            string evidence = Path.Combine(root, "evidence");
            Directory.CreateDirectory(frames);
            try
            {
                for (int raw = 0;
                    raw < AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedRawFrameCount;
                    raw++)
                {
                    File.WriteAllText(
                        Path.Combine(
                            frames,
                            AuditionPvStationBossDeathAftermathGoldenRunner
                                .RawFrameFileName(raw)),
                        "raw-" + raw);
                }

                string warmup = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RemapRawFrames(frames, evidence);
                Assert.That(File.ReadAllText(warmup), Is.EqualTo("raw-0"));
                Assert.That(File.ReadAllText(Path.Combine(
                    frames,
                    AuditionPvStationBossDeathAftermathCapture.FrameFileName(0))),
                    Is.EqualTo("raw-1"));
                Assert.That(File.ReadAllText(Path.Combine(
                    frames,
                    AuditionPvStationBossDeathAftermathCapture.FrameFileName(359))),
                    Is.EqualTo("raw-360"));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateLogicalFrameSequence(frames));
                File.WriteAllText(Path.Combine(frames, "frame_0360.png"), "extra");
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateLogicalFrameSequence(frames));
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void PixelCalibration_FirstHonestTakeIsFailClosedAfterTelemetryValidation()
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            Assert.That(
                AuditionPvStationBossDeathAftermathGoldenRunner.PixelCalibrationLocked,
                Is.False);
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
            AuditionPvStationBossDeathAftermathGoldenRunner
                .G08PixelCalibrationRequiredException failure = Assert.Throws<
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .G08PixelCalibrationRequiredException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateRuntimeProof(proof));
            Assert.That(failure.Message, Does.Contain("calibration"));
        }

        [Test]
        public void PixelTelemetry_RejectsNonFiniteNegativeAndOutOfDomainValues()
        {
            AssertRuntimeMutation(proof => proof.sequenceBlackRatio = double.PositiveInfinity);
            AssertRuntimeMutation(proof => proof.sequenceMagentaRatio = -0.0001d);
            AssertRuntimeMutation(proof => proof.maximumFrameMagentaRatio = double.NaN);
            AssertRuntimeMutation(proof => proof.healthyFramePercent = 100.0001d);
            AssertRuntimeMutation(proof => proof.impactMeanAbsoluteRgb = -1d);
            AssertRuntimeMutation(proof => proof.impactChangedRatio = 1.0001d);
            AssertRuntimeMutation(proof => proof.aftermathEvolutionChangedRatio = -1d);
            AssertRuntimeMutation(proof => proof.resultEntranceChangedRatio = double.NaN);
            AssertRuntimeMutation(proof => proof.resultBrightSamples = -1);
            AssertRuntimeMutation(proof => proof.cleanupFailure = "cleanup leaked");
            AssertRuntimeMutation(proof => proof.aftermathElapsedSeconds = 2.599f);
            AssertRuntimeMutation(proof => proof.overlayPresentationSucceededCount = 0);
            AssertRuntimeMutation(proof => proof.outcomeFactDigest = string.Empty);
            AssertRuntimeMutation(proof => proof.projectileImpactDirection = Vector3.zero);
        }

        [Test]
        public void LockedPixelThresholds_AcceptEveryBoundaryAndRejectEveryCrossing()
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof Boundary()
            {
                AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                    CreateValidProof();
                proof.sequenceBlackRatio = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumSequenceBlackRatio;
                proof.sequenceMagentaRatio = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MaximumSequenceMagentaRatio;
                proof.maximumFrameMagentaRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MaximumFrameMagentaRatio;
                proof.healthyFramePercent = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumHealthyFramePercent;
                proof.impactMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumImpactMeanAbsoluteRgb;
                proof.impactChangedRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumImpactChangedRatio;
                proof.aftermathEvolutionMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumAftermathEvolutionMeanAbsoluteRgb;
                proof.resultCutMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumResultCutMeanAbsoluteRgb;
                proof.resultCutChangedRatio =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumResultCutChangedRatio;
                proof.resultEntranceMeanAbsoluteRgb =
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .MinimumResultEntranceMeanAbsoluteRgb;
                proof.resultBrightSamples = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultBrightSamples;
                proof.resultDarkSamples = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultDarkSamples;
                proof.resultCyanSamples = AuditionPvStationBossDeathAftermathGoldenRunner
                    .MinimumResultCyanSamples;
                return proof;
            }

            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateLockedPixelThresholdsForTests(Boundary()));
            void Reject(Action<AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof>
                mutate)
            {
                AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                    Boundary();
                mutate(proof);
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateLockedPixelThresholdsForTests(proof));
            }

            Reject(value => value.sequenceBlackRatio += 0.000001d);
            Reject(value => value.sequenceMagentaRatio += 0.000001d);
            Reject(value => value.maximumFrameMagentaRatio += 0.000001d);
            Reject(value => value.healthyFramePercent -= 0.000001d);
            Reject(value => value.impactMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.impactChangedRatio -= 0.000001d);
            Reject(value => value.aftermathEvolutionMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.resultCutMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.resultCutChangedRatio -= 0.000001d);
            Reject(value => value.resultEntranceMeanAbsoluteRgb -= 0.000001d);
            Reject(value => value.resultBrightSamples--);
            Reject(value => value.resultDarkSamples--);
            Reject(value => value.resultCyanSamples--);
        }

        [Test]
        public void CalibrationFailure_WritesTelemetryAndLeavesNoSuccessArtifacts()
        {
            string root = NewTempRoot("g08-calibration");
            const string CaptureId = "g08-calibration-first-take";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            string baselines = Path.Combine(output,
                AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName);
            Directory.CreateDirectory(evidence);
            Directory.CreateDirectory(baselines);
            string[] successArtifacts = SuccessArtifactPaths(output);
            try
            {
                foreach (string path in successArtifacts)
                {
                    File.WriteAllText(path, "must-be-removed");
                }

                var state = CreateState(root, CaptureId);
                var proof = CreateValidProof();
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .G08PixelCalibrationRequiredException exception = Assert.Throws<
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .G08PixelCalibrationRequiredException>(() =>
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .ValidateRuntimeProof(proof));
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .WriteFailureArtifactForRoot(
                        output,
                        "AwaitingEditMode",
                        exception,
                        proof,
                        state,
                        root);

                string failurePath = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FailureFileName);
                CalibrationFailureProbe artifact = JsonUtility.FromJson<
                    CalibrationFailureProbe>(File.ReadAllText(failurePath));
                Assert.That(artifact.pixelCalibrationLocked, Is.False);
                Assert.That(artifact.calibrationRequired, Is.True);
                Assert.That(artifact.exception, Does.Contain("CalibrationRequired"));
                Assert.That(artifact.runtime, Is.Not.Null);
                Assert.That(artifact.runtime.impactMeanAbsoluteRgb,
                    Is.EqualTo(proof.impactMeanAbsoluteRgb));
                Assert.That(successArtifacts.All(path => !File.Exists(path)), Is.True);
                Assert.That(File.Exists(failurePath), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void FailureCleanup_IsExhaustiveAndReportsInjectedDeleteFault()
        {
            string root = NewTempRoot("g08-delete-fault");
            const string CaptureId = "g08-delete-fault-fixture";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string[] artifacts = SuccessArtifactPaths(output);
            foreach (string path in artifacts)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "owned");
            }

            try
            {
                int calls = 0;
                string diagnostic = AuditionPvStationBossDeathAftermathGoldenRunner
                    .DeleteUncommittedSuccessArtifactsForRoot(
                        output,
                        CaptureId,
                        root,
                        path =>
                        {
                            calls++;
                            if (calls == 1)
                            {
                                throw new IOException("injected first delete fault");
                            }

                            File.Delete(path);
                        });
                Assert.That(diagnostic, Does.Contain("injected first delete fault"));
                Assert.That(calls, Is.EqualTo(artifacts.Length));
                Assert.That(File.Exists(artifacts[0]), Is.True);
                Assert.That(artifacts.Skip(1).All(path => !File.Exists(path)), Is.True);
                Assert.That(
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .DeleteUncommittedSuccessArtifactsForRoot(
                            output,
                            CaptureId,
                            root),
                    Is.Empty);
                Assert.That(artifacts.All(path => !File.Exists(path)), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void FailureArtifact_RecordsCleanupFaultWithoutSkippingLaterTargets()
        {
            string root = NewTempRoot("g08-failure-cleanup-artifact");
            const string CaptureId = "g08-failure-cleanup-artifact";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            string[] artifacts = SuccessArtifactPaths(output);
            foreach (string path in artifacts)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "owned");
            }

            try
            {
                int calls = 0;
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .WriteFailureArtifactForRoot(
                        output,
                        "injected-cleanup",
                        new InvalidOperationException("primary capture failure"),
                        CreateValidProof(),
                        CreateState(root, CaptureId),
                        root,
                        path =>
                        {
                            calls++;
                            if (calls == 1)
                            {
                                throw new IOException("injected cleanup persistence fault");
                            }

                            File.Delete(path);
                        });
                string failurePath = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FailureFileName);
                CalibrationFailureProbe failure = JsonUtility.FromJson<
                    CalibrationFailureProbe>(File.ReadAllText(failurePath));
                Assert.That(failure.successArtifactCleanupFailure,
                    Does.Contain("injected cleanup persistence fault"));
                Assert.That(calls, Is.EqualTo(artifacts.Length));
                Assert.That(artifacts.Skip(1).All(path => !File.Exists(path)), Is.True);
                Assert.That(File.Exists(failurePath), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void PersistedState_RejectsBaselineOutputRootAndSessionAuthorityEscapes()
        {
            string root = NewTempRoot("g08-state-root");
            string outside = NewTempRoot("g08-state-outside");
            const string CaptureId = "g08-state-fixture";
            var state = CreateState(root, CaptureId);
            string statePath = Path.Combine(
                state.outputDirectory,
                AuditionPvStationBossDeathAftermathGoldenRunner.StateFileName);
            try
            {
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidatePersistedStateLocationForRoot(
                            statePath,
                            state,
                            root));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateSessionStateAuthority(
                            state.outputDirectory,
                            state.captureId,
                            state.batchMode,
                            state));

                state.baselineDirectory = outside;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.baselineDirectory = Path.Combine(
                    state.outputDirectory,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName);
                state.outputRoot = outside;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidatePersistedStateLocationForRoot(statePath, state, root));
                state.outputRoot = root;
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateSessionStateAuthority(
                            outside,
                            CaptureId,
                            false,
                            state));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateSessionRecoveryLocationForRoot(
                            Path.Combine(outside, "state.json"),
                            state.outputDirectory,
                            CaptureId,
                            root));
            }
            finally
            {
                DeleteTempRoot(root);
                DeleteTempRoot(outside);
            }
        }

        [Test]
        public void TerminalPersistenceRecovery_ClearsSessionWritesFailureAndRequestsExitOne()
        {
            string root = NewTempRoot("g08-terminal-fault");
            const string CaptureId = "g08-terminal-fault-fixture";
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, CaptureId);
            Directory.CreateDirectory(output);
            bool cleared = false;
            int exitCode = -1;
            try
            {
                Exception failure = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RecoverTerminalPersistenceFaultForRoot(
                        output,
                        CaptureId,
                        root,
                        "injected SaveState fault",
                        () => cleared = true,
                        code => exitCode = code);
                Assert.That(failure, Is.Null);
                Assert.That(cleared, Is.True);
                Assert.That(exitCode, Is.EqualTo(1));
                string failurePath = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FailureFileName);
                Assert.That(File.Exists(failurePath), Is.True);
                Assert.That(File.ReadAllText(failurePath),
                    Does.Contain("stale Recording state was not resumed"));
                Assert.That(SuccessArtifactPaths(output).All(path => !File.Exists(path)),
                    Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void Manifest_IsExactAndBoundToCaptureStartStateAndRuntimeProof()
        {
            string root = NewTempRoot("g08-manifest");
            DateTime started = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            const string CaptureId =
                "20260102t030405z_g08-station-boss-death-aftermath_fixture";
            var state = CreateState(root, CaptureId, started);
            string output = state.outputDirectory;
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            var proof = CreateValidProof();
            proof.frameHashLedgerPath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .FrameHashLedgerFileName).Replace('\\', '/');
            proof.warmupEvidencePath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .WarmupEvidenceFileName).Replace('\\', '/');
            string[] paths = AuditionPvStationBossDeathAftermathGoldenRunner
                .CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencies = paths.Select(path =>
                new AuditionPvDependencyHash
                {
                    path = path,
                    exists = true,
                    byteLength = 1,
                    sha256 = ShaA
                }).ToArray();
            state.dependencyPaths = paths;
            state.dependencyHashesAtStart = dependencies;
            proof.dependencyHashCount = dependencies.Length;
            proof.captureStartProvenanceSha256 =
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ComputeCaptureStartProvenanceSha256(state);
            AuditionPvTestResult[] tests =
                AuditionPvStationBossDeathAftermathGoldenRunner.CreateTestResults(
                    state,
                    proof,
                    Path.Combine(
                        evidence,
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .RuntimeProofFileName),
                    started);
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    CaptureId,
                    root,
                    output,
                    new[]
                    {
                        AuditionPvStationBossDeathAftermathCapture
                            .CreateShotManifestEntry()
                    },
                    AuditionPvStationBossDeathAftermathCapture
                        .CreateBaselineManifestEntries(),
                    tests,
                    createdAtUtc: started,
                    gitSnapshot: new AuditionPvGitSnapshot
                    {
                        probeSucceeded = true,
                        commitSha = state.gitCommitSha,
                        branch = state.gitBranch,
                        isDirty = false,
                        dirtyStateHashSha256 = state.gitDirtyHashSha256
                    },
                    engineSnapshot: state.engine,
                    dependencyHashSnapshot: dependencies);
            try
            {
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestInMemory(manifest, CaptureId));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestMatchesRecordedState(state, manifest));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestProofProvenance(manifest, proof));

                string notes = manifest.shots[0].notes;
                manifest.shots[0].notes = "semantic substitution";
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestInMemory(manifest, CaptureId));
                manifest.shots[0].notes = notes;
                string commit = manifest.gitCommitSha;
                manifest.gitCommitSha = new string('b', 40);
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestMatchesRecordedState(state, manifest));
                manifest.gitCommitSha = commit;
                proof.captureStartProvenanceSha256 = ShaB;
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateManifestProofProvenance(manifest, proof));
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void CommittedPackageValidator_AcceptsExactPackageRejectsCorruptionAndWinsStaleFault()
        {
            string root = NewTempRoot("g08-committed-package");
            DateTime started = new(2026, 1, 2, 3, 4, 6, DateTimeKind.Utc);
            const string CaptureId =
                "20260102t030406z_g08-station-boss-death-aftermath_fixture";
            try
            {
                AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState state =
                    WriteCommittedPackageFixture(root, CaptureId, started);
                bool valid = AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state);
                Assert.That(valid, Is.True);
                Assert.That(
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .DetermineSessionRecoveryDecision(
                            editorPlaying: false,
                            committedManifestIsValid: valid,
                            terminalFault: "stale injected terminal marker"),
                    Is.EqualTo(AuditionPvStationBossDeathAftermathGoldenRunner
                        .SessionRecoveryDecision.CommittedManifest));

                string baseline = Path.Combine(
                    state.baselineDirectory,
                    AuditionPvStationBossDeathAftermathCapture.Bl10FileName);
                byte[] baselineBytes = File.ReadAllBytes(baseline);
                File.AppendAllText(baseline, "tamper");
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.False);
                File.WriteAllBytes(baseline, baselineBytes);

                string manifestPath = Path.Combine(
                    state.outputDirectory,
                    AuditionPvCaptureContract.ManifestFileName);
                string manifestJson = File.ReadAllText(manifestPath);
                AuditionPvCaptureManifest manifest = JsonUtility.FromJson<
                    AuditionPvCaptureManifest>(manifestJson);
                manifest.shots[0].notes = "semantic substitution";
                File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.False);
                File.WriteAllText(manifestPath, manifestJson);

                string originalCommit = state.gitCommitSha;
                state.gitCommitSha = new string('b', 40);
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.False);
                state.gitCommitSha = originalCommit;
                Assert.That(AuditionPvStationBossDeathAftermathGoldenRunner
                    .IsValidCommittedManifestAtForTests(
                        state.outputDirectory,
                        CaptureId,
                        root,
                        state), Is.True);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void DependenciesAndBatchContract_AreHeadfulAndPinCaptureClosure()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity", "-noaudio" }));
            foreach (string forbidden in new[] { "-batchmode", "-quit", "-nographics" })
            {
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationBossDeathAftermathGoldenRunner
                        .ValidateBatchCommandLine(
                            new[] { "Unity", "-noaudio", forbidden }));
            }

            string[] dependencies = AuditionPvStationBossDeathAftermathGoldenRunner
                .CollectCaptureDependencyPaths();
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedBasicProjectile_AimBolt.prefab"));
            Assert.That(dependencies, Does.Not.Contain(
                "Assets/_Game/Prefabs/Combat/PF_PlayerRangedProjectile_ActionFoundation.prefab"));
            Assert.That(dependencies, Does.Contain(
                "Assets/_Game/Art/Characters/Bosses/Akaza/Animations/DB_Akaza_Phase2Boss.controller"));
            Assert.That(dependencies, Does.Not.Contain(
                "Assets/_Game/Animations/Controllers/DB_Akaza_Phase2Boss.controller"));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerScriptPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerTestPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner.ReadmePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedRenderPipelineAssetPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ExpectedRenderPipelineAssetPath + ".meta"));
            foreach (string partial in new[]
                     {
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Timing.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Camera.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Signals.cs",
                         "Assets/_Game/Scripts/Presentation/ActionCinematicCueDirector.Bindings.cs"
                     })
            {
                Assert.That(dependencies, Does.Contain(partial));
                Assert.That(dependencies, Does.Contain(partial + ".meta"));
            }

            Assert.That(dependencies.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                Is.EqualTo(dependencies.Length));
            AuditionPvDependencyHash[] hashes =
                AuditionPvEnvironmentProbe.HashDependencies(dependencies);
            Assert.That(hashes.Length, Is.EqualTo(dependencies.Length));
            Assert.That(
                hashes.Select(value => value.path).ToArray(),
                Is.EqualTo(dependencies),
                "Collected dependency paths must already be the exact canonical "
                + "project/package/meta path set emitted by HashDependencies.");
            Assert.That(hashes.All(value => value != null
                && value.exists
                && value.byteLength >= 0
                && AuditionPvSha256.IsSha256(value.sha256)), Is.True);
        }

        [Test]
        public void CaptureSubscriptionsAndLateRenderOrdering_PinPhysicalF62EvidenceOwners()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathCapture.CaptureScriptPath);
            Assert.That(source, Does.Contain(
                "ranged.RangedProjectileFired += HandleProjectileFired"));
            Assert.That(source, Does.Contain(
                "projectile.DamageApplied += HandleProjectileDamageApplied"));
            Assert.That(source, Does.Contain("bossHealth.Died += HandleBossDied"));
            Assert.That(source, Does.Contain(
                "projectile != firedProjectile"));
            Assert.That(source, Does.Contain(
                "projectile.GetInstanceID() != projectileInstanceId"));
            Assert.That(source, Does.Contain(
                "projectilePositionAtFrame61 = firedProjectile.transform.position"));
            Assert.That(source, Does.Contain(
                "Vector3.Distance(projectileSpawnPosition, projectilePositionAtFrame61)"));
            Assert.That(source, Does.Contain(
                "projectileImpactSequence <= bossDiedSequence"));

            var runnerOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationBossDeathAftermathGoldenRunnerBehaviour),
                typeof(DefaultExecutionOrder));
            var directorOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationBossDeathAftermathDirector),
                typeof(DefaultExecutionOrder));
            var probeOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(AuditionPvStationBossDeathAftermathRenderProbe),
                typeof(DefaultExecutionOrder));
            var cameraOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(DimensionBrawl.Presentation.ActionCameraController),
                typeof(DefaultExecutionOrder));
            var motionOrder = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(DimensionBrawl.Presentation.AkazaPhase2CombatMotionDriver),
                typeof(DefaultExecutionOrder));
            Assert.That(runnerOrder.order, Is.EqualTo(-32500));
            Assert.That(directorOrder.order, Is.EqualTo(-32000));
            Assert.That(probeOrder.order, Is.EqualTo(32000));
            Assert.That(probeOrder.order, Is.GreaterThan(cameraOrder.order));
            Assert.That(probeOrder.order, Is.GreaterThan(motionOrder.order));
        }

        [Test]
        public void TransactionSource_IsCalibrationFirstManifestLastAndManifestWinsRecovery()
        {
            string source = ReadProjectFile(
                AuditionPvStationBossDeathAftermathGoldenRunner.RunnerScriptPath);
            int finalize = source.IndexOf(
                "private static void FinalizeSuccessfulCapture",
                StringComparison.Ordinal);
            int calibration = source.IndexOf(
                "if (!PixelCalibrationLocked)",
                finalize,
                StringComparison.Ordinal);
            int baselineWrite = source.IndexOf(
                "CopyBaselines(state, frames, proof)",
                finalize,
                StringComparison.Ordinal);
            int manifestWrite = source.IndexOf(
                "AuditionPvCaptureManifestWriter.WriteNew(manifest);",
                finalize,
                StringComparison.Ordinal);
            int nextMethod = source.IndexOf(
                "private static void AnalyzeFrames",
                manifestWrite,
                StringComparison.Ordinal);
            Assert.That(finalize, Is.GreaterThanOrEqualTo(0));
            Assert.That(calibration, Is.GreaterThan(finalize));
            Assert.That(baselineWrite, Is.GreaterThan(calibration));
            Assert.That(manifestWrite, Is.GreaterThan(baselineWrite));
            Assert.That(nextMethod, Is.GreaterThan(manifestWrite));
            string afterCommit = source.Substring(
                manifestWrite
                    + "AuditionPvCaptureManifestWriter.WriteNew(manifest);".Length,
                nextMethod - manifestWrite
                    - "AuditionPvCaptureManifestWriter.WriteNew(manifest);".Length);
            Assert.That(afterCommit, Does.Not.Contain("Write"));
            Assert.That(afterCommit, Does.Not.Contain("SaveState"));

            int resume = source.IndexOf(
                "private static void ResumeOwnedSession",
                StringComparison.Ordinal);
            int manifestRecovery = source.IndexOf(
                "IsValidCommittedManifestAt(",
                resume,
                StringComparison.Ordinal);
            int terminalFault = source.IndexOf(
                "SessionTerminalFaultKey",
                resume,
                StringComparison.Ordinal);
            Assert.That(manifestRecovery, Is.GreaterThan(resume));
            Assert.That(terminalFault, Is.GreaterThan(manifestRecovery));
        }

        [Test]
        public void RuntimeProof_RejectsEveryCanonicalTimelineAndLifecycleSubstitution()
        {
            AssertRuntimeMutation(value => value.fireFrame = 2);
            AssertRuntimeMutation(value => value.projectileImpactFrame = 61);
            AssertRuntimeMutation(value => value.bossDiedFrame = 63);
            AssertRuntimeMutation(value => value.aftermathCompletedFrame = 217);
            AssertRuntimeMutation(value => value.inputLeaseReleasedFrame = 219);
            AssertRuntimeMutation(value => value.firstFreezeFrame = 217);
            AssertRuntimeMutation(value => value.firstResultSceneFrame = 219);
            AssertRuntimeMutation(value => value.firstInteractiveFrame = 245);
            AssertRuntimeMutation(value => value.allEightLocksObservedAtImpact = false);
            AssertRuntimeMutation(value => value.allEightLocksReleasedAtResult = false);
            AssertRuntimeMutation(value => value.bossDeathCameraInterrupted = true);
            AssertRuntimeMutation(value => value.bossDeathVfxRequestCount = 2);
            AssertRuntimeMutation(value => value.bossDeathAudioSourceDelta = 0);
            AssertRuntimeMutation(value => value.bossDeathUsesPhaseTwoAnchor = false);
            AssertRuntimeMutation(value => value.deathMotionRequestCount = 2);
            AssertRuntimeMutation(value => value.resultSummarySameInstance = false);
            AssertRuntimeMutation(value => value.presentedSummarySameInstance = false);
            AssertRuntimeMutation(value => value.eventsReleased = false);
            AssertRuntimeMutation(value => value.editModeGlobalCleanupExact = false);
            AssertRuntimeMutation(value => value.renderEvidence[0].bossPixelExtent =
                new Vector2(7f, 20f));
        }

        private static void AssertRuntimeMutation(
            Action<AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof> mutate)
        {
            AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof proof =
                CreateValidProof();
            mutate(proof);
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ValidateRuntimeProofBeforePixelCalibration(proof));
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof
            CreateValidProof()
        {
            return new AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof
            {
                directorCompleted = true,
                lastLogicalFrame = 359,
                presentedFrameCount = 360,
                presentedFramesExact = true,
                presentationClockExact = true,
                recorderWarmupEndOfFrameCount = 2,
                recorderAutoStoppedAfterLastFrame = true,
                runId = "run",
                playableStageId = "olympus-station",
                routeRevision = 1,
                routeDigest = "route-digest",
                transitionTokenId = "token",
                transitionTokenDigest = "token-digest",
                loaderGeneration = 1,
                segmentEntryReceiptId = "entry",
                segmentEntryReceiptDigest = "entry-digest",
                handoffTerminalReceiptId = "terminal",
                handoffTerminalReceiptDigest = "terminal-digest",
                enteredFromHandoffPending = true,
                exactHandoffReceiptChain = true,
                productTransitionProviderObserved = true,
                productTransitionDestinationArrived = true,
                productTransitionHandoffCompleted = true,
                productTransitionGeneration = 1,
                entryGuideObservedPlaying = true,
                entryGuideReleased = true,
                phaseTransitionStartCount = 1,
                phaseTransitionCompletionCount = 1,
                phaseTwoApplied = true,
                preparedHealth = 12f,
                bossHealthBeforeShot = 12f,
                fireFrame = 1,
                projectileFiredFrame = 1,
                bossDiedFrame = 62,
                projectileImpactFrame = 62,
                terminalResolvedFrame = 62,
                firstFreezeFrame = 218,
                firstResultSceneFrame = 218,
                firstResultConfiguredFrame = 218,
                firstInteractiveFrame = 246,
                aftermathCompletedFrame = 218,
                inputLeaseReleasedFrame = 218,
                deathStateHeldFrame = 129,
                rangedFireStartedCount = 1,
                projectileFiredCount = 1,
                projectileDamageAppliedCount = 1,
                bossDamagedDuringShotCount = 1,
                bossDiedCount = 1,
                encounterTerminalResolvedCount = 1,
                overlayPresentationSucceededCount = 1,
                aftermathStartedCount = 1,
                aftermathCompletedCount = 1,
                projectileInstanceId = 42,
                projectileFiredSequence = 1,
                bossDiedSequence = 2,
                projectileImpactSequence = 3,
                terminalResolvedSequence = 4,
                projectileSpawnPosition = Vector3.zero,
                projectilePositionAtFrame61 = new Vector3(11f, 0f, 0f),
                projectileImpactPoint = new Vector3(12f, 1f, 0f),
                projectileImpactDirection = Vector3.right,
                physicalProjectileObservedActiveBeforeImpact = true,
                projectileMovedBeforeImpact = true,
                noEarlyFreeze = true,
                resultAbsentBeforeRequest = true,
                allEightLocksObservedAtImpact = true,
                allEightLocksReleasedAtResult = true,
                deathStateAtAftermathHero = true,
                aftermathCompletedSuccessfully = true,
                aftermathScaleOneObserved = true,
                aftermathScaleOneViolated = false,
                aftermathBeginCount = 1,
                aftermathCompleteCount = 1,
                aftermathElapsedSeconds = 2.6f,
                bossDeathCameraRequestCount = 1,
                bossDeathCameraVersion = 4,
                bossDeathCameraInterrupted = false,
                bossDeathCameraComplete = true,
                bossDeathVfxRequestCount = 1,
                bossDeathAudioSourceDelta = 1,
                bossDeathUsesPhaseTwoAnchor = true,
                deathMotionRequestCount = 1,
                motionIsDead = true,
                motionAttacksStopped = true,
                animatorInDeathState = true,
                overlayShown = true,
                overlayFrozen = true,
                resultSummarySameInstance = true,
                presentedSummarySameInstance = true,
                committedSummaryDigest = "summary-digest",
                presentedSummaryDigest = "summary-digest",
                outcomeFactDigest = "fact-digest",
                rootAdmissionSequence = 1,
                terminalEpoch = 1,
                terminalEpochEvidenceDigest = "epoch-digest",
                terminalClosureDigest = "closure-digest",
                terminalRecordReceiptCount = 1,
                terminalFactsExact = true,
                hudWasActiveAtFire = true,
                hudWasActiveAtImpact = true,
                hudYieldedAtResult = true,
                resultInteractiveAt246 = true,
                stateRestored = true,
                eventsReleased = true,
                presentationClockReleased = true,
                cadenceReleased = true,
                transitionCaptureStateReleased = true,
                globalCaptureStateRestored = true,
                editModeSceneCleanupExact = true,
                editModeGlobalCleanupExact = true,
                cleanupFailure = string.Empty,
                renderEvidence = new[]
                {
                    GameplayEvidence(62),
                    GameplayEvidence(116),
                    new AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
                    {
                        frame = 246,
                        resultCanvasVisible = true,
                        resultInteractive = true
                    }
                },
                pixelSampleStride = 8,
                pixelSampleCount = 1,
                sequenceBlackRatio = 0.1d,
                sequenceMagentaRatio = 0d,
                maximumFrameMagentaRatio = 0d,
                healthyFramePercent = 100d,
                impactMeanAbsoluteRgb = 10d,
                impactChangedRatio = 0.5d,
                aftermathEvolutionMeanAbsoluteRgb = 5d,
                aftermathEvolutionChangedRatio = 0.5d,
                resultCutMeanAbsoluteRgb = 10d,
                resultCutChangedRatio = 0.5d,
                resultEntranceMeanAbsoluteRgb = 5d,
                resultEntranceChangedRatio = 0.5d,
                resultBrightSamples = 1000,
                resultDarkSamples = 1000,
                resultCyanSamples = 100,
                frameHashLedgerSha256 = ShaA,
                warmupEvidenceSha256 = ShaA,
                bl10Sha256 = ShaA,
                bl11Sha256 = ShaA,
                bl12Sha256 = ShaA,
                dependencyHashCount = 1,
                captureStartProvenanceSha256 = ShaA
            };
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            GameplayEvidence(int frame)
        {
            return new AuditionPvStationBossDeathAftermathGoldenRunner.RenderEvidence
            {
                frame = frame,
                gameplayCameraExact = true,
                playerSafeViewport = true,
                bossSafeViewport = true,
                playerViewport = new Vector3(0.25f, 0.5f, 10f),
                bossViewport = new Vector3(0.75f, 0.5f, 10f),
                playerPixelExtent = new Vector2(100f, 200f),
                bossPixelExtent = new Vector2(150f, 250f)
            };
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
            CreateState(string root, string captureId, DateTime? startedAt = null)
        {
            DateTime time = (startedAt ?? DateTime.UtcNow).ToUniversalTime();
            string output = AuditionPvOutputPaths.ResolveOutputDirectory(root, captureId);
            return new AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
            {
                schema = "dimension-brawl.audition-pv.g08-runner-state.v1",
                phase = AuditionPvStationBossDeathAftermathGoldenRunner.RunnerPhase
                    .AwaitingEditMode.ToString(),
                startedAtUtc = time.ToString("O"),
                captureId = captureId,
                outputRoot = root,
                outputDirectory = output,
                baselineDirectory = Path.Combine(
                    output,
                    AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName),
                gitCommitSha = new string('a', 40),
                gitBranch = "main",
                gitWorktreeDirty = false,
                gitDirtyHashSha256 = ShaA,
                engine = new AuditionPvEngineSnapshot
                {
                    unityVersion = AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedUnityVersion,
                    unityVersionWithRevision =
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .ExpectedUnityVersionWithRevision,
                    recorderPackageVersion = AuditionPvCaptureContract
                        .RecorderPackageVersion,
                    urpPackageVersion = AuditionPvStationBossDeathAftermathGoldenRunner
                        .ExpectedUrpPackageVersion,
                    activeRenderPipelineAssetPath =
                        AuditionPvStationBossDeathAftermathGoldenRunner
                            .ExpectedRenderPipelineAssetPath
                },
                dependencyPaths = Array.Empty<string>(),
                dependencyHashesAtStart = Array.Empty<AuditionPvDependencyHash>()
            };
        }

        private static AuditionPvStationBossDeathAftermathGoldenRunner.PersistedRunnerState
            WriteCommittedPackageFixture(
                string root,
                string captureId,
                DateTime started)
        {
            var state = CreateState(root, captureId, started);
            string output = state.outputDirectory;
            string frames = Path.Combine(output, "frames", "g08");
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            Directory.CreateDirectory(frames);
            Directory.CreateDirectory(evidence);
            Directory.CreateDirectory(state.baselineDirectory);
            byte[] frameBytes = FakeQhdPngHeader();
            for (int frame = 0;
                frame < AuditionPvStationBossDeathAftermathCapture.ExpectedFrameCount;
                frame++)
            {
                File.WriteAllBytes(
                    Path.Combine(
                        frames,
                        AuditionPvStationBossDeathAftermathCapture.FrameFileName(frame)),
                    frameBytes);
            }

            string warmup = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner.WarmupEvidenceFileName);
            var texture = new Texture2D(
                AuditionPvCaptureContract.Width,
                AuditionPvCaptureContract.Height,
                TextureFormat.RGBA32,
                false,
                true);
            try
            {
                File.WriteAllBytes(warmup, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }

            foreach (AuditionPvBaselineManifestEntry baseline in
                     AuditionPvStationBossDeathAftermathCapture
                         .CreateBaselineManifestEntries())
            {
                File.Copy(
                    Path.Combine(
                        frames,
                        AuditionPvStationBossDeathAftermathCapture.FrameFileName(
                            baseline.sourceFrame)),
                    Path.Combine(state.baselineDirectory, baseline.fileName));
            }

            var proof = CreateValidProof();
            string ledger = AuditionPvStationBossDeathAftermathGoldenRunner
                .BuildFrameHashLedger(frames);
            proof.frameHashLedgerPath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .FrameHashLedgerFileName).Replace('\\', '/');
            File.WriteAllText(proof.frameHashLedgerPath, ledger);
            proof.frameHashLedgerSha256 = AuditionPvSha256.TextHash(ledger);
            proof.warmupEvidencePath = warmup.Replace('\\', '/');
            proof.warmupEvidenceSha256 = AuditionPvSha256.FileHash(warmup);
            proof.bl10Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName(62)));
            proof.bl11Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName(116)));
            proof.bl12Sha256 = AuditionPvSha256.FileHash(Path.Combine(
                frames,
                AuditionPvStationBossDeathAftermathCapture.FrameFileName(246)));
            string[] paths = AuditionPvStationBossDeathAftermathGoldenRunner
                .CollectCaptureDependencyPaths();
            AuditionPvDependencyHash[] dependencies = paths.Select(path =>
                new AuditionPvDependencyHash
                {
                    path = path,
                    exists = true,
                    byteLength = 1,
                    sha256 = ShaA
                }).ToArray();
            state.dependencyPaths = paths;
            state.dependencyHashesAtStart = dependencies;
            proof.dependencyHashCount = dependencies.Length;
            proof.captureStartProvenanceSha256 =
                AuditionPvStationBossDeathAftermathGoldenRunner
                    .ComputeCaptureStartProvenanceSha256(state);
            string proofPath = Path.Combine(
                evidence,
                AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProofFileName);
            File.WriteAllText(proofPath, JsonUtility.ToJson(new RuntimeProofFileProbe
            {
                schema = AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProofSchema,
                captureId = captureId,
                mapping = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RuntimeMappingDescription,
                gameplay = AuditionPvStationBossDeathAftermathGoldenRunner
                    .RuntimeGameplayDescription,
                runtime = proof
            }, true));
            AuditionPvTestResult[] results =
                AuditionPvStationBossDeathAftermathGoldenRunner.CreateTestResults(
                    state,
                    proof,
                    proofPath,
                    started);
            AuditionPvCaptureManifest manifest =
                AuditionPvCaptureManifestFactory.CreateForRoot(
                    captureId,
                    root,
                    output,
                    new[]
                    {
                        AuditionPvStationBossDeathAftermathCapture
                            .CreateShotManifestEntry()
                    },
                    AuditionPvStationBossDeathAftermathCapture
                        .CreateBaselineManifestEntries(),
                    results,
                    createdAtUtc: started,
                    gitSnapshot: new AuditionPvGitSnapshot
                    {
                        probeSucceeded = true,
                        commitSha = state.gitCommitSha,
                        branch = state.gitBranch,
                        isDirty = false,
                        dirtyStateHashSha256 = state.gitDirtyHashSha256
                    },
                    engineSnapshot: state.engine,
                    dependencyHashSnapshot: dependencies);
            AuditionPvCaptureManifestWriter.WriteNew(manifest);
            return state;
        }

        private static byte[] FakeQhdPngHeader()
        {
            byte[] bytes = new byte[24];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Array.Copy(signature, bytes, signature.Length);
            bytes[12] = (byte)'I';
            bytes[13] = (byte)'H';
            bytes[14] = (byte)'D';
            bytes[15] = (byte)'R';
            WriteBigEndian(bytes, 16, AuditionPvCaptureContract.Width);
            WriteBigEndian(bytes, 20, AuditionPvCaptureContract.Height);
            return bytes;
        }

        private static void WriteBigEndian(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private static string[] SuccessArtifactPaths(string output)
        {
            string evidence = Path.Combine(output,
                AuditionPvStationBossDeathAftermathGoldenRunner.EvidenceFolderName);
            string baselines = Path.Combine(output,
                AuditionPvStationBossDeathAftermathCapture.BaselinesFolderName);
            return new[]
            {
                Path.Combine(output, AuditionPvCaptureContract.ManifestFileName),
                Path.Combine(evidence,
                    AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProofFileName),
                Path.Combine(evidence,
                    AuditionPvStationBossDeathAftermathGoldenRunner.FrameHashLedgerFileName),
                Path.Combine(baselines,
                    AuditionPvStationBossDeathAftermathCapture.Bl10FileName),
                Path.Combine(baselines,
                    AuditionPvStationBossDeathAftermathCapture.Bl11FileName),
                Path.Combine(baselines,
                    AuditionPvStationBossDeathAftermathCapture.Bl12FileName)
            };
        }

        private static string ReadProjectFile(string projectRelativePath)
        {
            string project = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project root missing.");
            return File.ReadAllText(Path.Combine(project, projectRelativePath));
        }

        private static int Count(string text, string token)
        {
            int count = 0;
            int cursor = 0;
            while ((cursor = text.IndexOf(token, cursor, StringComparison.Ordinal)) >= 0)
            {
                count++;
                cursor += token.Length;
            }

            return count;
        }

        private static string NewTempRoot(string label)
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-" + label + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        private static void DeleteTempRoot(string root)
        {
            string full = Path.GetFullPath(root);
            string temp = Path.GetFullPath(Path.GetTempPath()).TrimEnd('\\', '/');
            if (!full.StartsWith(temp + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Refused test cleanup outside the OS temp root.");
            }

            if (Directory.Exists(full))
            {
                Directory.Delete(full, recursive: true);
            }
        }

        [Serializable]
        private sealed class CalibrationFailureProbe
        {
            public string exception;
            public bool pixelCalibrationLocked;
            public bool calibrationRequired;
            public string successArtifactCleanupFailure;
            public AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof runtime;
        }

        [Serializable]
        private sealed class RuntimeProofFileProbe
        {
            public string schema;
            public string captureId;
            public string mapping;
            public string gameplay;
            public AuditionPvStationBossDeathAftermathGoldenRunner.RuntimeProof runtime;
        }
    }
}
