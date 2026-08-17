using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public void RuntimeSpool_G04FullCardinalityCarryForwardFitsBothBoundedSpools()
        {
            string[] rendererIds = Enumerable.Range(
                    0,
                    AuditionPvRuntimeWorkloadCaptureSession.MaxStableIdsPerInventory)
                .Select(index => "UnityEngine.MeshRenderer/global/" +
                                 index.ToString("D4") + "/" + new string('r', 96))
                .ToArray();
            string[] materialIds = Enumerable.Range(
                    0,
                    AuditionPvRuntimeWorkloadCaptureSession.MaxStableIdsPerInventory)
                .Select(index => "UnityEngine.MeshRenderer/global/" +
                                 index.ToString("D4") + "/slot/0/material/" +
                                 new string('a', 32) + "/" + index.ToString("D4") + "/" +
                                 new string('m', 96))
                .ToArray();
            string rendererHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("renderers", rendererIds);
            string materialHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("material-slots", materialIds);
            string[] canvasIds = { "UnityEngine.Canvas/global/canvas-001" };
            string[] hudIds = { "UnityEngine.CanvasRenderer/global/hud-001" };
            string canvasHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("canvases", canvasIds);
            string hudHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("hud-renderers", hudIds);
            var generalEncoder = new AuditionPvRuntimeWorkloadCarryForwardEncoder();
            var cleanEncoder = new AuditionPvRuntimeWorkloadCarryForwardEncoder();
            long generalBytes = 0;
            long cleanBytes = 0;
            long firstGeneralLineBytes = 0;
            long maximumGeneralRepeatBytes = 0;
            long maximumCleanRepeatBytes = 0;
            const int G04FrameCount = 598;

            AuditionPvRuntimeFrameWorkload Frame(int sourceFrame) => new()
            {
                sourceFrame = sourceFrame,
                inspectedRendererCount = rendererIds.LongLength,
                inspectedMaterialSlotCount = materialIds.LongLength,
                rendererStableIds = rendererIds,
                materialSlotStableIds = materialIds,
                rendererInventorySha256 = rendererHash,
                materialInventorySha256 = materialHash,
                inspectedCanvasCount = canvasIds.LongLength,
                inspectedHudRendererCount = hudIds.LongLength,
                inspectedDrawCommandCount = rendererIds.LongLength + hudIds.LongLength,
                canvasStableIds = canvasIds,
                hudRendererStableIds = hudIds,
                canvasInventorySha256 = canvasHash,
                hudInventorySha256 = hudHash
            };

            for (int sourceFrame = 0; sourceFrame < G04FrameCount; sourceFrame++)
            {
                AuditionPvRuntimeFrameWorkload general = Frame(sourceFrame);
                generalEncoder.Compress(general, false);
                long generalLineBytes = Encoding.UTF8.GetByteCount(
                    JsonUtility.ToJson(general, false)) + 1L;
                AuditionPvRuntimeWorkloadCaptureSession.RequireWithinBudget(
                    generalLineBytes,
                    generalBytes,
                    "g04",
                    sourceFrame,
                    sourceFrame);
                generalBytes += generalLineBytes;

                AuditionPvRuntimeFrameWorkload clean = Frame(sourceFrame);
                cleanEncoder.Compress(clean, true);
                long cleanLineBytes = Encoding.UTF8.GetByteCount(
                    JsonUtility.ToJson(clean, false)) + 1L;
                AuditionPvRuntimeWorkloadCaptureSession.RequireWithinBudget(
                    cleanLineBytes,
                    cleanBytes,
                    "g04-clean",
                    sourceFrame,
                    sourceFrame);
                cleanBytes += cleanLineBytes;

                if (sourceFrame == 0)
                {
                    firstGeneralLineBytes = generalLineBytes;
                    Assert.That(general.rendererStableIds.Length,
                        Is.EqualTo(rendererIds.Length));
                    Assert.That(clean.rendererStableIds.Length,
                        Is.EqualTo(rendererIds.Length));
                }
                else
                {
                    maximumGeneralRepeatBytes = Math.Max(
                        maximumGeneralRepeatBytes,
                        generalLineBytes);
                    maximumCleanRepeatBytes = Math.Max(
                        maximumCleanRepeatBytes,
                        cleanLineBytes);
                    Assert.That(general.rendererStableIds, Is.Empty);
                    Assert.That(general.materialSlotStableIds, Is.Empty);
                    Assert.That(clean.rendererStableIds, Is.Empty);
                    Assert.That(clean.materialSlotStableIds, Is.Empty);
                    Assert.That(clean.canvasStableIds, Is.Empty);
                    Assert.That(clean.hudRendererStableIds, Is.Empty);
                }
            }

            Assert.That(firstGeneralLineBytes, Is.GreaterThan(512L * 1024L),
                "The regression must exercise the G04 failure's former 512 KiB row ceiling.");
            Assert.That(firstGeneralLineBytes,
                Is.LessThanOrEqualTo(
                    AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes));
            Assert.That(generalBytes,
                Is.LessThanOrEqualTo(
                    AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes));
            Assert.That(cleanBytes,
                Is.LessThanOrEqualTo(
                    AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes));

            int[] familyFrameCounts = { 240, 420, 300, 598, 720, 600, 720, 780, 720 };
            long maximumRepeatBytes = Math.Max(
                maximumGeneralRepeatBytes,
                maximumCleanRepeatBytes);
            foreach (int frameCount in familyFrameCounts)
            {
                long constantInventorySpoolBytes = checked(
                    firstGeneralLineBytes + (frameCount - 1L) * maximumRepeatBytes);
                Assert.That(frameCount,
                    Is.LessThanOrEqualTo(
                        AuditionPvRuntimeWorkloadCaptureSession.MaxRangeFrames));
                Assert.That(constantInventorySpoolBytes,
                    Is.LessThanOrEqualTo(
                        AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes));
            }
        }

        [Test]
        public void RuntimeSpool_FixedLineAndAggregateUpperBoundsRemainFailClosed()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvRuntimeWorkloadCaptureSession.RequireWithinBudget(
                    AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes,
                    AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes -
                    AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes,
                    "g04",
                    597,
                    597));
            InvalidDataException line = Assert.Throws<InvalidDataException>(() =>
                AuditionPvRuntimeWorkloadCaptureSession.RequireWithinBudget(
                    AuditionPvRuntimeWorkloadCaptureSession.MaxFrameLineUtf8Bytes + 1L,
                    0,
                    "g04",
                    0,
                    0));
            Assert.That(line.Message, Does.Contain("shot=g04"));
            Assert.That(line.Message, Does.Contain("lineBytes="));

            InvalidDataException spool = Assert.Throws<InvalidDataException>(() =>
                AuditionPvRuntimeWorkloadCaptureSession.RequireWithinBudget(
                    1,
                    AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes,
                    "g04-clean",
                    597,
                    597));
            Assert.That(spool.Message, Does.Contain("shot=g04-clean"));
            Assert.That(spool.Message, Does.Contain("maxSpoolBytes="));
        }

        [Test]
        public void RuntimeSpoolReader_RejectsNoNewlineInputBeforeUnboundedReadLineAllocation()
        {
            const int TestLineLimit = 1024;
            byte[] exactBytes = Encoding.UTF8.GetBytes(
                new string('x', TestLineLimit - 1) + "\n");
            using (var exactStream = new MemoryStream(exactBytes))
            using (var exactReader = new StreamReader(
                       exactStream,
                       new UTF8Encoding(false, true),
                       false,
                       128,
                       false))
            {
                Assert.That(AuditionPvSixtySecondEvidenceProducer
                    .ReadRuntimeWorkloadLineCapped(exactReader, TestLineLimit)?.Length,
                    Is.EqualTo(TestLineLimit - 1));
            }

            byte[] oversizedBytes = Encoding.UTF8.GetBytes(
                new string('x', TestLineLimit + 1));
            using var oversizedStream = new MemoryStream(oversizedBytes);
            using var oversizedReader = new StreamReader(
                oversizedStream,
                new UTF8Encoding(false, true),
                false,
                128,
                false);
            Assert.Throws<InvalidDataException>(() =>
                AuditionPvSixtySecondEvidenceProducer.ReadRuntimeWorkloadLineCapped(
                    oversizedReader,
                    TestLineLimit));
        }

        [Test]
        public void RuntimeSpool_OneIdCombatChurnUsesBoundedDeltasAcrossLongestFamily()
        {
            const int InventoryCount = 4096;
            const int LongestFamilyFrameCount = 780;
            string[] baseRenderers = Enumerable.Range(0, InventoryCount)
                .Select(index => "renderer/global/" + index.ToString("D4") + "/" +
                                 new string('r', 96))
                .ToArray();
            string[] alternateRenderers = baseRenderers.Take(InventoryCount - 1)
                .Concat(new[] { "renderer/global/zzzz/" + new string('z', 96) })
                .ToArray();
            string[] materials = { "material/guid-a/1" };
            string baseHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("renderers", baseRenderers);
            string alternateHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("renderers", alternateRenderers);
            string materialHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("material-slots", materials);
            var encoder = new AuditionPvRuntimeWorkloadCarryForwardEncoder();
            var validationState = new AuditionPvSixtySecondGateManifestValidator
                .RuntimeWorkloadValidationState();
            long artifactFrameBytes = 0;

            for (int sourceFrame = 0;
                 sourceFrame < LongestFamilyFrameCount;
                 sourceFrame++)
            {
                bool alternate = (sourceFrame & 1) != 0;
                string[] currentRenderers = alternate
                    ? alternateRenderers
                    : baseRenderers;
                string currentHash = alternate ? alternateHash : baseHash;
                var frame = new AuditionPvRuntimeFrameWorkload
                {
                    sourceFrame = sourceFrame,
                    inspectedRendererCount = currentRenderers.LongLength,
                    inspectedMaterialSlotCount = materials.LongLength,
                    rendererStableIds = currentRenderers,
                    materialSlotStableIds = materials,
                    rendererInventorySha256 = currentHash,
                    materialInventorySha256 = materialHash,
                    inspectedDrawCommandCount = currentRenderers.LongLength
                };
                encoder.Compress(frame, false);
                string line = JsonUtility.ToJson(frame, false);
                long lineBytes = Encoding.UTF8.GetByteCount(line) + 1L;
                artifactFrameBytes = checked(artifactFrameBytes + lineBytes);
                var entry = new AuditionPvSelectedFrameScanEntry
                {
                    sourceFrame = sourceFrame,
                    inspectedRendererCount = currentRenderers.LongLength,
                    inspectedMaterialSlotCount = materials.LongLength,
                    rendererInventorySha256 = currentHash,
                    materialInventorySha256 = materialHash
                };
                Assert.That(AuditionPvSixtySecondGateManifestValidator
                    .RuntimeWorkloadFrameMatches(
                        "renderer-material-scan",
                        frame,
                        entry,
                        string.Empty,
                        validationState), Is.True);

                if (sourceFrame == 0)
                {
                    Assert.That(frame.rendererStableIds.Length,
                        Is.EqualTo(InventoryCount));
                }
                else
                {
                    Assert.That(frame.rendererStableIds, Is.Empty);
                    Assert.That(frame.rendererAddedStableIds.Length, Is.EqualTo(1));
                    Assert.That(frame.rendererRemovedStableIds.Length, Is.EqualTo(1));
                    Assert.That(frame.materialSlotStableIds, Is.Empty);
                    Assert.That(encoder.LastFrameIncludedDelta, Is.True);
                }
            }

            Assert.That(artifactFrameBytes,
                Is.LessThan(AuditionPvSixtySecondEvidenceProducer.MaxJsonBytes),
                "Realistic one-renderer spawn/despawn churn must stay below the pinned JSON cap.");
            Assert.That(artifactFrameBytes,
                Is.LessThanOrEqualTo(
                    AuditionPvRuntimeWorkloadCaptureSession.MaxSpoolUtf8Bytes));
        }

        [Test]
        public void RuntimeRangeWriter_MidRangeDeltaRehydratesAnchorAndMovesClosedFile()
        {
            string[] renderersA = { "renderer/global/a", "renderer/global/b" };
            string[] renderersB = { "renderer/global/a", "renderer/global/c" };
            string[] renderersC = { "renderer/global/a", "renderer/global/d" };
            string[] materials = { "material/guid-a/1" };
            string materialHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("material-slots", materials);
            var encoder = new AuditionPvRuntimeWorkloadCarryForwardEncoder();
            var rows = new string[3];
            string[][] inventories = { renderersA, renderersB, renderersC };
            string[] rendererHashes = inventories.Select(ids =>
                    AuditionPvSixtySecondGateManifestValidator
                        .StableInventorySha256("renderers", ids))
                .ToArray();
            long maximumLineBytes = 0;
            int snapshotFrames = 0;
            int deltaFrames = 0;
            for (int sourceFrame = 0; sourceFrame < rows.Length; sourceFrame++)
            {
                var frame = new AuditionPvRuntimeFrameWorkload
                {
                    sourceFrame = sourceFrame,
                    inspectedRendererCount = inventories[sourceFrame].LongLength,
                    inspectedMaterialSlotCount = materials.LongLength,
                    rendererStableIds = inventories[sourceFrame],
                    materialSlotStableIds = materials,
                    rendererInventorySha256 = rendererHashes[sourceFrame],
                    materialInventorySha256 = materialHash,
                    inspectedDrawCommandCount = inventories[sourceFrame].LongLength
                };
                encoder.Compress(frame, false);
                if (encoder.LastFrameIncludedFullSnapshot) snapshotFrames++;
                if (encoder.LastFrameIncludedDelta) deltaFrames++;
                rows[sourceFrame] = JsonUtility.ToJson(frame, false);
                maximumLineBytes = Math.Max(
                    maximumLineBytes,
                    Encoding.UTF8.GetByteCount(rows[sourceFrame]) + 1L);
            }
            string spoolPath = Path.Combine(root, "frames.ndjson");
            File.WriteAllText(
                spoolPath,
                string.Join("\n", rows) + "\n",
                new UTF8Encoding(false));
            var seal = new AuditionPvRuntimeWorkloadCaptureSeal
            {
                schemaVersion = AuditionPvRuntimeWorkloadCaptureSession.SealSchema,
                captureId = "capture-a",
                sourceShotId = "g04",
                sourceRangeStartFrame = 0,
                sourceRangeEndFrame = 2,
                frameCount = 3,
                framesPath = spoolPath.Replace('\\', '/'),
                framesSha256 = AuditionPvSha256.FileHash(spoolPath),
                framesUtf8Bytes = new FileInfo(spoolPath).Length,
                maxFrameLineUtf8Bytes = maximumLineBytes,
                inventorySnapshotFrameCount = snapshotFrames,
                inventoryDeltaFrameCount = deltaFrames,
                tool = nameof(AuditionPvRuntimeWorkloadCaptureSession),
                toolVersion = AuditionPvRuntimeWorkloadCaptureSession.ToolVersion,
                completedAtUtc = "2026-08-17T00:00:00.0000000Z"
            };
            var request = new AuditionPvSixtySecondEvidenceRequest
            {
                sourceRangeStartFrame = 1,
                sourceRangeEndFrame = 2,
                selectStartFrame = 1,
                selectEndFrame = 2
            };
            var capture = new AuditionPvCaptureManifest { captureId = "capture-a" };
            var shot = new AuditionPvShotManifestEntry { id = "g04" };
            Type producer = typeof(AuditionPvSixtySecondEvidenceProducer);
            Type validatedType = producer.GetNestedType(
                "ValidatedRequest",
                BindingFlags.NonPublic);
            Type runtimeType = producer.GetNestedType("RuntimeFacts", BindingFlags.NonPublic);
            Assert.That(validatedType, Is.Not.Null);
            Assert.That(runtimeType, Is.Not.Null);
            object validated = Activator.CreateInstance(validatedType, true);
            object runtime = Activator.CreateInstance(runtimeType, true);
            void SetField(object target, string name, object value) => target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.SetValue(target, value);
            SetField(validated, "request", request);
            SetField(validated, "capture", capture);
            SetField(validated, "shot", shot);
            SetField(validated, "captureCoreSha256", new string('a', 64));
            SetField(runtime, "seal", seal);
            SetField(runtime, "framesPath", spoolPath);
            MethodInfo writer = producer.GetMethod(
                "WriteRuntimeWorkloadArtifact",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(writer, Is.Not.Null);
            string outputPath = Path.Combine(root, "midrange_runtime_workload.json");
            try
            {
                writer.Invoke(null, new object[]
                {
                    outputPath,
                    validated,
                    runtime,
                    new string('b', 64),
                    new string('c', 64),
                    "renderer-material-scan",
                    new AuditionPvPinnedArtifact(),
                    "2026-08-17T00:00:00.0000000Z"
                });
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }

            Assert.That(File.Exists(outputPath), Is.True,
                "The FileShare.None stream must be closed before the Windows atomic move.");
            AuditionPvRuntimeWorkloadArtifact artifact = JsonUtility.FromJson<
                AuditionPvRuntimeWorkloadArtifact>(File.ReadAllText(outputPath));
            Assert.That(artifact.frames.Length, Is.EqualTo(2));
            Assert.That(artifact.frames[0].sourceFrame, Is.EqualTo(1));
            Assert.That(artifact.frames[0].rendererStableIds,
                Is.EqualTo(renderersB),
                "A mid-range artifact must begin with a self-contained full anchor.");
            Assert.That(artifact.frames[0].rendererAddedStableIds, Is.Empty);
            Assert.That(artifact.frames[0].rendererRemovedStableIds, Is.Empty);
            Assert.That(artifact.frames[1].sourceFrame, Is.EqualTo(2));
            Assert.That(artifact.frames[1].rendererStableIds, Is.Empty);
            Assert.That(artifact.frames[1].rendererAddedStableIds,
                Is.EqualTo(new[] { "renderer/global/d" }));
            Assert.That(artifact.frames[1].rendererRemovedStableIds,
                Is.EqualTo(new[] { "renderer/global/c" }));
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
