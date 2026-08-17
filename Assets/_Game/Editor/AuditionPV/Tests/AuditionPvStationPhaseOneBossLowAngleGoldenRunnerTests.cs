using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationPhaseOneBossLowAngleGoldenRunnerTests
    {
        [Test]
        public void RecorderMapping_PreservesRawZeroAndMapsOneThroughSixHundred()
        {
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner.ExpectedRawFrameCount,
                Is.EqualTo(601));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .RawFrameFileName(0),
                Is.EqualTo("frame_0000.png"));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .RawFrameFileName(600),
                Is.EqualTo("frame_0600.png"));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .RawToSourceFrame(1),
                Is.Zero);
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .RawToSourceFrame(600),
                Is.EqualTo(599));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .RawToSourceFrame(0));
        }

        [Test]
        public void RuntimeProof_RequiresExactCaptureAndRestoreContract()
        {
            AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RuntimeProof proof =
                CreatePassingProof(2);
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateRuntimeProof(proof, 2));

            proof.transitionStartedEventCount = 1;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateRuntimeProof(proof, 2));
            proof.transitionStartedEventCount = 0;
            proof.stateRestored = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateRuntimeProof(proof, 2));
        }

        [Test]
        public void SourceLedger_BindsEveryPhysicalPngToRawSourceAndSelection()
        {
            const string sha =
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var frames = Enumerable.Range(0, 600)
                .Select(source =>
                    new AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                        .SourceFrameLedgerEntry
                    {
                        sourceFrame = source,
                        recorderRawFrame = source + 1,
                        selectedLogicalFrame =
                            AuditionPvStationPhaseOneBossLowAngleCapture
                                .SourceToSelectedLogicalFrame(source),
                        role = AuditionPvStationPhaseOneBossLowAngleCapture
                            .SourceFrameRole(source),
                        relativePngPath =
                            "frames/s050/"
                            + AuditionPvStationPhaseOneBossLowAngleCapture
                                .FrameFileName(source),
                        byteLength = 25,
                        pngSha256 = sha,
                        width = 2560,
                        height = 1440,
                        fps = 60
                    })
                .ToArray();
            var ledger = new AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                .SourceLedger
            {
                schema =
                    "dimension-brawl.audition-pv.canonical-source-ledger.v1",
                captureId =
                    "20260817t120000z_s050-t01_gaaaaaaaaaaaa_clean",
                segmentId = "PV_S050",
                shotId = "s050",
                takeOrdinal = 1,
                railPresetId = "rail-ltr-032",
                scenePath =
                    AuditionPvStationPhaseOneBossLowAngleCapture.StationScenePath,
                sceneSha256 = sha,
                phaseOneVisualPrefabPath =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabPath,
                phaseOneVisualPrefabGuid =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .PhaseOneVisualPrefabGuid,
                phaseOneVisualPrefabSha256 = sha,
                sourceCaptureCoreSha256 = sha,
                sourceFirstFrame = 0,
                sourceLastFrame = 599,
                sourceFrameCount = 600,
                selectedFirstSourceFrame = 180,
                selectedLastSourceFrame = 419,
                selectedFrameCount = 240,
                recorderPaddingRawFrame = 0,
                recorderFirstMappedRawFrame = 1,
                recorderLastMappedRawFrame = 600,
                width = 2560,
                height = 1440,
                fps = 60,
                sourceFormat = AuditionPvCaptureContract.SourceFormat,
                frames = frames
            };

            Assert.DoesNotThrow(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateSourceLedger(ledger));
            Assert.That(frames.Count(value => value.role == "prehandle"),
                Is.EqualTo(180));
            Assert.That(frames.Count(value => value.role == "selected"),
                Is.EqualTo(240));
            Assert.That(frames.Count(value => value.role == "posthandle"),
                Is.EqualTo(180));

            frames[419].recorderRawFrame = 419;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateSourceLedger(ledger));
        }

        [Test]
        public void PngValidator_RequiresExactQhdIhdr()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "dimension-brawl-s050-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "frame.png");
            try
            {
                File.WriteAllBytes(path, PngHeader(2560, 1440));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                        .ValidatePngFile(path));

                File.WriteAllBytes(path, PngHeader(1920, 1080));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                        .ValidatePngFile(path));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: false);
                }
            }
        }

        [Test]
        public void BatchArguments_RequireHeadfulAsynchronousEditor()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity.exe", "-noaudio" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity.exe" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateBatchCommandLine(
                        new[] { "Unity.exe", "-noaudio", "-batchmode" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateBatchCommandLine(
                        new[] { "Unity.exe", "-noaudio", "-nographics" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ValidateBatchCommandLine(
                        new[] { "Unity.exe", "-noaudio", "-quit" }));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ResolveRequestedTakeOrdinal(new[] { "Unity.exe", "-noaudio" }),
                Is.EqualTo(1));
            Assert.That(
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ResolveRequestedTakeOrdinal(
                        new[] { "Unity.exe", "-noaudio", "-s050TakeOrdinal=3" }),
                Is.EqualTo(3));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .ResolveRequestedTakeOrdinal(
                        new[] { "Unity.exe", "-noaudio", "-s050TakeOrdinal=4" }));
        }

        [Test]
        public void GateBindings_PinAuthorshipRuntimeCoreAndEveryS050SemanticFact()
        {
            const string sha =
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var take = new AuditionPvStationPhaseOneBossLowAngleGoldenRunner.TakeState
            {
                takeOrdinal = 2,
                railPresetId = AuditionPvStationPhaseOneBossLowAngleCapture
                    .GetRailPreset(2).Id,
                captureId =
                    "20260817t120000z_s050-station-t02_gaaaaaaaaaaaa_clean",
                outputDirectory = "C:/capture",
                runtimeProof = CreatePassingProof(2)
            };
            AuditionPvShotAuthorshipArtifact authorship =
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .CreateShotAuthorship(
                        take,
                        sha,
                        "C:/capture/evidence/s050_runtime_proof.json",
                        sha,
                        new DateTime(
                            2026, 8, 17, 12, 0, 0, DateTimeKind.Utc));
            var shot = new AuditionPvSixtySecondAtomicShot
            {
                cameraId = AuditionPvStationPhaseOneBossLowAngleCapture.CameraId(2),
                gameplayState =
                    AuditionPvStationPhaseOneBossLowAngleCapture.GameplayState,
                deterministicSeed =
                    AuditionPvStationPhaseOneBossLowAngleCapture
                        .DeterministicSeed(2),
                timelineId = AuditionPvStationPhaseOneBossLowAngleCapture.TimelineId
            };
            var candidate = new AuditionPvSixtySecondTakeCandidate
            {
                sourceCaptureCoreSha256 = sha,
                sourceCaptureId = take.captureId,
                sourceShotId = "s050",
                cameraId = shot.cameraId,
                gameplayState = shot.gameplayState,
                deterministicSeed = shot.deterministicSeed,
                timelineId = shot.timelineId
            };
            Assert.That(
                AuditionPvSixtySecondGateManifestValidator
                    .ShotAuthorshipIdentityValid(
                        authorship,
                        shot,
                        candidate,
                        sha),
                Is.True);

            AuditionPvTestResult[] results =
                AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                    .CreateTestResults(
                        take,
                        "C:/capture/evidence/s050_runtime_proof.json",
                        sha,
                        "C:/capture/evidence/s050_shot_authorship.json",
                        sha,
                        "C:/capture/evidence/s050_source_ledger.json",
                        sha,
                        "C:/capture/evidence/s050_source_frames.sha256",
                        sha,
                        sha,
                        new[]
                        {
                            new AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                                .SemanticArtifactBinding
                            {
                                factKey = "boss-low-angle",
                                path = "C:/capture/evidence/semantic_beats/boss-low-angle.json",
                                sha256 = sha
                            },
                            new AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                                .SemanticArtifactBinding
                            {
                                factKey = "boss-silhouette",
                                path = "C:/capture/evidence/semantic_beats/boss-silhouette.json",
                                sha256 = sha
                            },
                            new AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                                .SemanticArtifactBinding
                            {
                                factKey = "boss-low-angle-silhouette",
                                path = "C:/capture/evidence/semantic_beats/boss-low-angle-silhouette.json",
                                sha256 = sha
                            }
                        },
                        DateTime.UtcNow);
            string[] requiredNames =
            {
                "shot-authorship/s050",
                "shot-authorship-runtime/s050",
                "semantic-beat/boss-low-angle",
                "semantic-beat/boss-silhouette",
                "semantic-beat/boss-low-angle-silhouette"
            };
            foreach (string name in requiredNames)
            {
                AuditionPvTestResult result = results.Single(value =>
                    value.suite
                        == AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                            .GateEvidenceTestSuite
                    && value.name == name);
                Assert.That(result.status, Is.EqualTo("passed"));
                Assert.That(result.details, Does.Contain("artifact-sha256=" + sha));
                Assert.That(result.details, Does.Contain("capture-core-sha256=" + sha));
            }

            Assert.That(
                results.Single(value =>
                        value.name == "semantic-beat/boss-low-angle")
                    .details,
                Does.Contain("semantic-fact=boss-low-angle"));
            Assert.That(
                results.Single(value =>
                        value.name == "semantic-beat/boss-silhouette")
                    .details,
                Does.Contain("semantic-fact=boss-silhouette"));
        }

        private static AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RuntimeProof
            CreatePassingProof(int takeOrdinal)
        {
            return new AuditionPvStationPhaseOneBossLowAngleGoldenRunner.RuntimeProof
            {
                freshSceneValidated = true,
                directorCompleted = true,
                takeOrdinal = takeOrdinal,
                railPresetId = AuditionPvStationPhaseOneBossLowAngleCapture
                    .GetRailPreset(takeOrdinal).Id,
                lastSourceFrame = 599,
                presentedFrameCount = 600,
                selectedPresentedFrameCount = 240,
                presentedFramesExact = true,
                selectedMappingExact = true,
                presentationClockExact = true,
                hudOffEveryFrame = true,
                phaseOneEveryFrame = true,
                cameraTakeoverObserved = true,
                allSelectedFramesInFront = true,
                allSelectedFramesLowAngle = true,
                allSelectedFramesInCoverage = true,
                minimumProjectedHeight = 0.29f,
                maximumProjectedHeight = 0.36f,
                maximumEyeHeightRatio = 0.14f,
                minimumCornerDepth = 3f,
                bossFullAliveAndUnchanged = true,
                transitionStateUnchanged = true,
                transitionStartedEventCount = 0,
                transitionCompletedEventCount = 0,
                recorderWarmupEndOfFrameCount = 2,
                recorderCaptureDeltaTimeAtSourceFrameZero = 1f / 60f + 0.0001f,
                recorderPaddingActiveAtSourceFrameZero = true,
                recorderAutoStoppedAfterLastFrame = true,
                stateRestored = true,
                presentationClockReleased = true,
                cadenceSuspensionCountAfterRestore = 0
            };
        }

        private static byte[] PngHeader(int width, int height)
        {
            var bytes = new byte[24];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Array.Copy(signature, bytes, signature.Length);
            bytes[12] = (byte)'I';
            bytes[13] = (byte)'H';
            bytes[14] = (byte)'D';
            bytes[15] = (byte)'R';
            WriteBigEndian(bytes, 16, width);
            WriteBigEndian(bytes, 20, height);
            return bytes;
        }

        private static void WriteBigEndian(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }
    }
}
