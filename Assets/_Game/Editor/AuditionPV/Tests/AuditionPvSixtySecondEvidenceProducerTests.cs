using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvSixtySecondEvidenceProducerTests
    {
        private string root;

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_PV60_Evidence_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        [Test]
        public void MemoryContract_PermitsOnlyOneDecodedSourceFrameAndStaysBounded()
        {
            Assert.That(
                AuditionPvEvidenceMemoryContract.MaxSimultaneousDecodedSourcePngs,
                Is.EqualTo(1));
            long peak = AuditionPvEvidenceMemoryContract.ConservativePeakBytes(
                AuditionPvSixtySecondEvidenceProducer.MaxPngBytes,
                AuditionPvSixtySecondEvidenceProducer.MaxPngBytes,
                2560L * 1440L * 4L);
            Assert.That(peak,
                Is.LessThanOrEqualTo(
                    AuditionPvEvidenceMemoryContract.MaxTransientWorkingSetBytes));

            using IDisposable lease =
                AuditionPvEvidenceMemoryContract.AcquireDecodedSourcePng();
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvEvidenceMemoryContract.AcquireDecodedSourcePng());
        }

        [Test]
        public void RuntimeSpool_InterruptedOrIncompleteCaptureNeverEmitsSeal()
        {
            string capture = Path.Combine(root, "capture-a");
            Directory.CreateDirectory(capture);
            var config = new AuditionPvRuntimeWorkloadCaptureConfig
            {
                captureId = "capture-a",
                captureOutputDirectory = capture,
                sourceShotId = "shot-a",
                sourceRangeStartFrame = 0,
                sourceRangeEndFrame = 1,
                captureHudEvidence = false
            };
            string seal;
            using (AuditionPvRuntimeWorkloadCaptureSession session =
                   AuditionPvRuntimeWorkloadCaptureSession.Open(config))
            {
                seal = session.SealPath;
                Assert.Throws<InvalidDataException>(() => session.Complete());
            }
            Assert.That(File.Exists(seal), Is.False,
                "An incomplete capture may leave an orphan spool but can never leave a seal.");
        }

        [Test]
        public void RuntimeSpool_RejectsRangeBeyondFixedMemoryCardinality()
        {
            string capture = Path.Combine(root, "capture-a");
            Directory.CreateDirectory(capture);
            var config = new AuditionPvRuntimeWorkloadCaptureConfig
            {
                captureId = "capture-a",
                captureOutputDirectory = capture,
                sourceShotId = "shot-a",
                sourceRangeStartFrame = 0,
                sourceRangeEndFrame =
                    AuditionPvRuntimeWorkloadCaptureSession.MaxRangeFrames,
                captureHudEvidence = false
            };
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvRuntimeWorkloadCaptureSession.Open(config));
        }

        [Test]
        public void RuntimeMaterialIdentity_UsesDeterministicShaderSignatureWithoutInstanceId()
        {
            Shader shader = Shader.Find("Sprites/Default") ??
                Shader.Find("Universal Render Pipeline/Unlit");
            Assert.That(shader, Is.Not.Null);
            var first = new Material(shader) { name = "Runtime Probe (Instance)" };
            var second = new Material(shader) { name = "Runtime Probe" };
            try
            {
                string firstId = AuditionPvRuntimeWorkloadProbe
                    .StableMaterialIdForTest(first);
                string secondId = AuditionPvRuntimeWorkloadProbe
                    .StableMaterialIdForTest(second);
                Assert.That(firstId, Is.EqualTo(secondId));
                Assert.That(firstId, Does.StartWith("runtime-material-signature/"));

                second.renderQueue = second.renderQueue + 1;
                Assert.That(AuditionPvRuntimeWorkloadProbe
                    .StableMaterialIdForTest(second), Is.Not.EqualTo(firstId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void RuntimeSeal_FullShotCanFeedMultipleAtomicRangesButPartialSealCannot()
        {
            var shot = new AuditionPvShotManifestEntry
            {
                id = "g06",
                startFrame = 0,
                endFrame = 719,
                expectedFrameCount = 720
            };
            var seal = new AuditionPvRuntimeWorkloadCaptureSeal
            {
                sourceRangeStartFrame = 0,
                sourceRangeEndFrame = 719,
                frameCount = 720
            };
            var first = new AuditionPvSixtySecondEvidenceRequest
            {
                sourceRangeStartFrame = 0,
                sourceRangeEndFrame = 659,
                selectStartFrame = 180,
                selectEndFrame = 479
            };
            var second = new AuditionPvSixtySecondEvidenceRequest
            {
                sourceRangeStartFrame = 60,
                sourceRangeEndFrame = 719,
                selectStartFrame = 240,
                selectEndFrame = 539
            };

            Assert.That(AuditionPvSixtySecondEvidenceProducer
                .RuntimeSealCoversRange(seal, shot, first), Is.True);
            Assert.That(AuditionPvSixtySecondEvidenceProducer
                .RuntimeSealCoversRange(seal, shot, second), Is.True);

            seal.sourceRangeEndFrame = 659;
            seal.frameCount = 660;
            Assert.That(AuditionPvSixtySecondEvidenceProducer
                .RuntimeSealCoversRange(seal, shot, first), Is.False,
                "Only a complete source-shot seal may derive Gate range evidence.");
        }

        [Test]
        public void Producer_RejectsUnapprovedHighCostRangeBeforeWritingAnything()
        {
            string captureDirectory = Path.Combine(root, "capture-a");
            Directory.CreateDirectory(captureDirectory);
            AuditionPvCaptureManifest capture = CaptureCore(captureDirectory);
            string graphics = Path.Combine(root, "graphics");
            string reviews = Path.Combine(root, "reviews");
            var request = new AuditionPvSixtySecondEvidenceRequest
            {
                captureCoreManifest = capture,
                sourceShotId = "shot-a",
                sourceRangeStartFrame = 0,
                sourceRangeEndFrame = 0,
                selectStartFrame = 0,
                selectEndFrame = 0,
                runtimeWorkloadSealPath = Path.Combine(root, "missing-seal.json"),
                graphicsRootDirectory = graphics,
                reviewRootDirectory = reviews,
                approvedSourceRange = false
            };

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvSixtySecondEvidenceProducer.Produce(request));
            Assert.That(Directory.Exists(graphics), Is.False);
            Assert.That(Directory.Exists(reviews), Is.False);
        }

        [Test]
        public void MergeCaptureTests_AllowsSameGateNameForDistinctRangeArtifacts()
        {
            string firstPath = WriteArtifact("first.json", "first");
            string secondPath = WriteArtifact("second.json", "second");
            AuditionPvTestResult first = Passed("resolution", firstPath);
            AuditionPvTestResult second = Passed("resolution", secondPath);
            var bundle = new AuditionPvSixtySecondEvidenceBundle
                { testResults = new[] { second } };

            AuditionPvTestResult[] merged = AuditionPvSixtySecondEvidenceProducer
                .MergeCaptureTestResults(new[] { first }, bundle);

            Assert.That(merged.Length, Is.EqualTo(2));
            Assert.That(merged.Select(value => value.name),
                Is.EqualTo(new[] { "resolution", "resolution" }));
            Assert.Throws<InvalidDataException>(() =>
                AuditionPvSixtySecondEvidenceProducer.MergeCaptureTestResults(
                    new[] { second }, bundle));
        }

        [Test]
        public void S050ApprovedEvidenceFlag_IsExplicitAndCaseInsensitive()
        {
            Assert.That(AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                .ResolveApprovedEvidenceRequest(Array.Empty<string>()), Is.False);
            Assert.That(AuditionPvStationPhaseOneBossLowAngleGoldenRunner
                .ResolveApprovedEvidenceRequest(new[] { "-PV60APPROVEDEVIDENCE" }), Is.True);
        }

        [Test]
        public void ReviewSkeleton_DefaultCanNeverMasqueradeAsHumanApproval()
        {
            var skeleton = new AuditionPvTakeReviewSkeletonArtifact();
            string json = JsonUtility.ToJson(skeleton);
            Assert.That(skeleton.approved, Is.False);
            Assert.That(json, Does.Contain("\"approved\":false"));
            Assert.That(skeleton.reviewedBy, Is.Empty);
            Assert.That(skeleton.reviewedAtUtc, Is.Empty);
        }

        private AuditionPvCaptureManifest CaptureCore(string outputDirectory) => new()
        {
            schemaVersion = AuditionPvCaptureContract.SchemaVersion,
            captureId = "capture-a",
            createdAtUtc = "2026-08-17T00:00:00.0000000Z",
            outputRoot = root.Replace('\\', '/'),
            outputDirectory = outputDirectory.Replace('\\', '/'),
            sourceFormat = AuditionPvCaptureContract.SourceFormat,
            width = 2560,
            height = 1440,
            fps = 60,
            gitCommitSha = new string('a', 40),
            gitBranch = "main",
            worktreeDirtyHashSha256 = new string('0', 64),
            unityVersion = Application.unityVersion,
            unityVersionWithRevision = Application.unityVersion + " (test)",
            recorderPackageVersion = AuditionPvCaptureContract.RecorderPackageVersion,
            urpPackageVersion = "17.3.0",
            activeRenderPipelineAssetPath = "Assets/Settings/URP.asset",
            shots = new[]
            {
                new AuditionPvShotManifestEntry
                {
                    id = "shot-a",
                    scenePath = "Assets/Test.unity",
                    startFrame = 0,
                    endFrame = 0,
                    expectedFrameCount = 1,
                    hudMode = "hud-on"
                }
            },
            dependencyHashes = new[]
            {
                new AuditionPvDependencyHash
                {
                    path = "Assets/Settings/URP.asset",
                    exists = true,
                    byteLength = 1,
                    sha256 = new string('b', 64)
                }
            }
        };

        private string WriteArtifact(string name, string content)
        {
            string path = Path.Combine(root, name);
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path;
        }

        private static AuditionPvTestResult Passed(string name, string path)
        {
            string hash = AuditionPvSha256.FileHash(path);
            return new AuditionPvTestResult
            {
                suite = AuditionPvSixtySecondEvidenceProducer.AutomatedTestSuite,
                name = name,
                status = "passed",
                artifactPath = path,
                details = "artifact-sha256=" + hash
            };
        }
    }
}
