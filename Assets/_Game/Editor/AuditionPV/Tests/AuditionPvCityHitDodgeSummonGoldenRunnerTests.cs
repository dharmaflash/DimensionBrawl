using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvCityHitDodgeSummonGoldenRunnerTests
    {
        [Test]
        public void RecorderMapping_UsesOnePaddingFrameAnd720SourceFrames()
        {
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawPaddingFrame,
                Is.Zero);
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawFirstSourceFrame,
                Is.EqualTo(1));
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawLastSourceFrame,
                Is.EqualTo(720));
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.ExpectedRawFrameCount,
                Is.EqualTo(721));
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawToSourceFrame(1),
                Is.Zero);
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawToSourceFrame(720),
                Is.EqualTo(719));
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawFrameFileName(0),
                Is.EqualTo("frame_0000.png"));
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner.RawFrameFileName(720),
                Is.EqualTo("frame_0720.png"));
        }

        [Test]
        public void BatchCommandLine_RequiresHeadfulNoAudioInvocation()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity", "-noaudio" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ValidateBatchCommandLine(
                        new[] { "Unity", "-noaudio", "-batchmode" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ValidateBatchCommandLine(
                        new[] { "Unity", "-noaudio", "-nographics" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ValidateBatchCommandLine(
                        new[] { "Unity", "-noaudio", "-quit" }));
        }

        [Test]
        public void ApprovedEvidenceFlag_IsExplicitAndCaseInsensitive()
        {
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ResolveApprovedEvidenceRequest(Array.Empty<string>()),
                Is.False);
            Assert.That(
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ResolveApprovedEvidenceRequest(
                        new[] { "Unity", "-Pv60ApprovedEvidence" }),
                Is.True);
        }

        [Test]
        public void ApprovedEvidencePipeline_SealsCanonicalRangeBeforeManifest()
        {
            string source = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvCityHitDodgeSummonCapture.RunnerScriptPath));

            Assert.That(source, Does.Contain(
                "AuditionPvRuntimeWorkloadCaptureSession.Open("));
            Assert.That(source, Does.Contain("sourceRangeStartFrame ="));
            Assert.That(source, Does.Contain(
                "AuditionPvCityHitDodgeSummonCapture.FirstSourceFrame"));
            Assert.That(source, Does.Contain("sourceRangeEndFrame ="));
            Assert.That(source, Does.Contain(
                "AuditionPvCityHitDodgeSummonCapture.LastSourceFrame"));
            Assert.That(source, Does.Contain(
                "runtimeWorkloadCapture?.CapturePresentedFrame(sourceFrame);"));
            Assert.That(source, Does.Contain(
                "state.runtimeWorkloadSealPath = runtimeWorkloadCapture.Complete();"));
            Assert.That(source, Does.Contain(
                "runtimeWorkloadCapture?.Dispose();"));
            Assert.That(source, Does.Contain(
                "approvedSourceRange = true"));

            int produce = source.IndexOf(
                "AuditionPvSixtySecondEvidenceProducer.Produce(",
                StringComparison.Ordinal);
            int merge = source.IndexOf(
                ".MergeCaptureTestResults(tests, evidence);",
                StringComparison.Ordinal);
            int write = source.IndexOf(
                "AuditionPvCaptureManifestWriter.WriteNew(manifest);",
                StringComparison.Ordinal);
            Assert.That(produce, Is.GreaterThanOrEqualTo(0));
            Assert.That(merge, Is.GreaterThan(produce));
            Assert.That(write, Is.GreaterThan(merge));
        }

        [Test]
        public void GateEvidence_WritesExactSuiteNamesAndPinnedArtifacts()
        {
            AuditionPvCityHitDodgeSummonRuntimeProof proof =
                AuditionPvCityHitDodgeSummonCaptureTests.CreateValidProof();
            string temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_S030_Gate_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
            try
            {
                string proofPath = Path.Combine(
                    temporaryDirectory,
                    "s030_runtime_proof.json");
                string ledgerPath = Path.Combine(
                    temporaryDirectory,
                    "s030_source_ledger.json");
                File.WriteAllText(proofPath, "{\"proof\":\"s030\"}");
                File.WriteAllText(ledgerPath, "{\"ledger\":\"s030\"}");
                var state = new AuditionPvCityHitDodgeSummonGoldenRunner
                    .PersistedRunnerState
                {
                    captureId = "s030-test-capture",
                    engine = new AuditionPvCityHitDodgeSummonGoldenRunner
                        .EngineState
                    {
                        recorderPackageVersion = "5.1.6"
                    }
                };

                AuditionPvTestResult[] results =
                    AuditionPvCityHitDodgeSummonGoldenRunner
                        .WriteGateEvidenceArtifacts(
                            state,
                            proof,
                            proofPath,
                            ledgerPath,
                            temporaryDirectory,
                            new string('a', 64),
                            DateTime.UtcNow);

                Assert.That(results, Has.Length.EqualTo(5));
                Assert.That(
                    results.All(result =>
                        result.suite
                            == AuditionPvCityHitDodgeSummonCapture
                                .GateEvidenceTestSuite
                        && result.status == "passed"
                        && result.details.Contains("artifact-sha256=")
                        && File.Exists(result.artifactPath)),
                    Is.True);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "shot-authorship/s030",
                        "shot-authorship-runtime/s030",
                        "semantic-beat/player-hit",
                        "semantic-beat/perfect-dodge",
                        "semantic-beat/summon-chain"
                    },
                    results.Select(result => result.name).ToArray());

                string authorship = File.ReadAllText(
                    results.Single(result =>
                        result.name == "shot-authorship/s030")
                        .artifactPath);
                Assert.That(
                    authorship,
                    Does.Contain("dimension-brawl.audition-pv.shot-authorship.v1"));
                Assert.That(authorship, Does.Contain(new string('a', 64)));
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }

        [Test]
        public void RunnerSource_EnforcesFreshSceneRestoreAndCanonicalRemap()
        {
            string source = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvCityHitDodgeSummonCapture.RunnerScriptPath));

            Assert.That(
                source,
                Does.Contain("SetRecordModeToFrameInterval("));
            Assert.That(
                source,
                Does.Contain("RawPaddingFrame,"));
            Assert.That(
                source,
                Does.Contain("RawLastSourceFrame"));
            Assert.That(
                source,
                Does.Contain(".ReopenProductSceneAfterPlayMode();"));
            Assert.That(source, Does.Contain("director.RestoreShotState();"));
            Assert.That(
                source,
                Does.Contain("freshSceneReopened = true"));
            Assert.That(
                source,
                Does.Contain("recorder_padding_raw_frame_0000.png"));
            Assert.That(
                source,
                Does.Contain("raw1..raw720 map"));
            Assert.That(
                source,
                Does.Contain("select f180..f539"));
        }

        [Test]
        public void DependencyStability_RejectsAnyByteOrHashChange()
        {
            var before = new[]
            {
                Dependency("Assets/a", 10, new string('a', 64)),
                Dependency("Assets/b", 20, new string('b', 64))
            };
            var same = new[]
            {
                Dependency("Assets/a", 10, new string('a', 64)),
                Dependency("Assets/b", 20, new string('b', 64))
            };

            Assert.DoesNotThrow(() =>
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ValidateStableDependencies(before, same));
            same[1].sha256 = new string('c', 64);
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHitDodgeSummonGoldenRunner
                    .ValidateStableDependencies(before, same));
        }

        [Test]
        public void DedicatedFiles_AreAllDependencyPinned()
        {
            string[] paths = AuditionPvCityHitDodgeSummonCapture
                .ExplicitProductDependencyPaths();
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Editor/AuditionPV/AuditionPvCityHitDodgeSummonCapture.cs");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Editor/AuditionPV/AuditionPvCityHitDodgeSummonGoldenRunner.cs");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHitDodgeSummonCaptureTests.cs");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Editor/AuditionPV/Tests/AuditionPvCityHitDodgeSummonGoldenRunnerTests.cs");
            CollectionAssert.Contains(
                paths,
                "Assets/_Game/Editor/AuditionPV/AuditionPvSixtySecondGateManifest.cs");
        }

        private static AuditionPvDependencyHash Dependency(
            string path,
            long length,
            string hash)
        {
            return new AuditionPvDependencyHash
            {
                path = path,
                exists = true,
                byteLength = length,
                sha256 = hash
            };
        }

        private static string ProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(
                    UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Unity project root is missing.");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }
    }
}
