using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    [TestFixture]
    internal sealed class AuditionPvTwelveSecondGoldAssemblerTests
    {
        private const string CommitSha =
            "0123456789abcdef0123456789abcdef01234567";
        private const string CleanHash =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string PipelinePath = "Assets/FixturePipeline.asset";
        private const string DependencySha =
            "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        private static readonly Dictionary<string, int> ContactSamplePayloads = new(
            StringComparer.Ordinal)
        {
            ["fixture-city/g01/90"] = 0,
            ["fixture-city/g02/150"] = 1,
            ["fixture-city/g02/210"] = 2,
            ["fixture-city/g02/270"] = 3,
            ["fixture-city/g03/225"] = 4,
            ["fixture-city/g03/285"] = 5,
            ["fixture-g04/g04/45"] = 6,
            ["fixture-g04/g04/105"] = 7,
            ["fixture-g04/g04/165"] = 8,
            ["fixture-g04/g04/225"] = 9,
            ["fixture-g06/g06/227"] = 10,
            ["fixture-g06/g06/287"] = 11
        };

        private static readonly object FixturePngLock = new();
        private static readonly Dictionary<int, byte[]>
            FixtureContactSourcePngs = new();

        private string temporaryRoot;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_Preedit12s_"
                + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }
        }

        [Test]
        public void ProductContract_RequiresExactRolesOrderG06And720Frames()
        {
            Fixture fixture = CreateFixture(writeFrames: false);

            Assert.DoesNotThrow(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateSegmentContract(
                    fixture.specification));
            Assert.That(
                fixture.specification.segments.Select(value => value.role),
                Is.EqualTo(AuditionPvTwelveSecondGoldAssembler.RequiredRoles));
            Assert.That(
                fixture.specification.segments.Sum(value =>
                    value.endFrame - value.startFrame + 1),
                Is.EqualTo(720));
            Assert.That(
                fixture.specification.segments.Sum(value =>
                    value.sourceFrameSha256.Length),
                Is.EqualTo(720));
            Assert.That(
                fixture.specification.segments.Select(value =>
                    $"{value.shotId}:{value.startFrame}..{value.endFrame}"),
                Is.EqualTo(new[]
                {
                    "g01:60..149",
                    "g02:150..299",
                    "g03:195..299",
                    "g04:0..237",
                    "g06:180..316"
                }));
            Assert.That(
                fixture.specification.segments[^1].shotId,
                Is.EqualTo("g06"));
        }

        [Test]
        public void SourceCaptureId_UsesCanonicalGoldenOutputIdLengthContract()
        {
            const string productionLengthCaptureId =
                "20260815t211339z_g06-station-phase2-summon-counter_"
                + "g99efc0173b09_clean";
            Assert.That(productionLengthCaptureId.Length, Is.GreaterThan(64));
            Assert.That(productionLengthCaptureId.Length, Is.LessThanOrEqualTo(128));
            Assert.DoesNotThrow(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateSourceCaptureId(
                    productionLengthCaptureId));

            Assert.Throws<InvalidDataException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateSourceCaptureId(
                    new string('a', 129)));
            Assert.Throws<InvalidDataException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateSourceCaptureId(
                    "capture-ID"));
            Assert.Throws<InvalidDataException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateSourceCaptureId(
                    "capture..id"));
        }

        [Test]
        public void MissingG06_FailsBeforeOutputRootReservationOrCopy()
        {
            Fixture fixture = CreateFixture(writeFrames: false);
            fixture.specification.segments[^1].shotId = "g05";

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("G06"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void TotalOtherThan720_FailsBeforeOutputRootReservation()
        {
            Fixture fixture = CreateFixture(writeFrames: false);
            fixture.specification.segments[0].endFrame--;

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("exactly 720"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void ValidFixture_InstallsContiguousMappedHashedProxyPackage()
        {
            Fixture fixture = CreateFixture(writeFrames: true);

            AuditionPvTwelveSecondAssemblyResult result = Assemble(fixture);

            Assert.That(result.frameCount, Is.EqualTo(720));
            Assert.That(Directory.Exists(result.outputDirectory), Is.True);
            Assert.That(File.Exists(result.manifestPath), Is.True);
            Assert.That(File.Exists(result.validationReportPath), Is.True);
            Assert.That(File.Exists(result.frameHashPath), Is.True);
            Assert.That(File.Exists(result.contactSheetPath), Is.True);
            Assert.That(File.Exists(result.proxyPath), Is.True);
            Assert.That(
                Directory.GetFiles(
                    Path.Combine(result.outputDirectory, "frames"),
                    "*.png"),
                Has.Length.EqualTo(720));
            Assert.That(
                File.Exists(Path.Combine(
                    result.outputDirectory,
                    "frames",
                    "frame_0000.png")),
                Is.True);
            Assert.That(
                File.Exists(Path.Combine(
                    result.outputDirectory,
                    "frames",
                    "frame_0719.png")),
                Is.True);

            AuditionPvTwelveSecondSelectManifest manifest =
                AuditionPvTwelveSecondGoldAssembler.ReadInstalledManifest(
                    result.outputDirectory);
            Assert.That(
                manifest.segments.Select(value => value.selectStartFrame),
                Is.EqualTo(new[] { 0, 90, 240, 345, 583 }));
            Assert.That(
                manifest.segments.Select(value => value.selectEndFrame),
                Is.EqualTo(new[] { 89, 239, 344, 582, 719 }));
            Assert.That(manifest.frames, Has.Length.EqualTo(720));
            Assert.That(manifest.frames[0].sourceShotId, Is.EqualTo("g01"));
            Assert.That(manifest.frames[719].sourceShotId, Is.EqualTo("g06"));
            Assert.That(
                manifest.frames.All(value =>
                    AuditionPvSha256.IsSha256(value.sha256)),
                Is.True);
            Assert.That(
                manifest.baselineReferences.Count(value =>
                    value.includedInSelect),
                Is.EqualTo(6));
            Assert.That(
                manifest.contactSheet.file,
                Is.EqualTo(
                    AuditionPvTwelveSecondGoldAssembler.ContactSheetFileName));
            Assert.That(manifest.contactSheet.width, Is.EqualTo(2560));
            Assert.That(manifest.contactSheet.height, Is.EqualTo(1080));
            Assert.That(manifest.contactSheet.cellWidth, Is.EqualTo(640));
            Assert.That(manifest.contactSheet.cellHeight, Is.EqualTo(360));
            Assert.That(manifest.contactSheet.columns, Is.EqualTo(4));
            Assert.That(manifest.contactSheet.rows, Is.EqualTo(3));
            Assert.That(
                manifest.contactSheet.downsamplePolicy,
                Is.EqualTo(
                    AuditionPvTwelveSecondGoldAssembler
                        .ContactSheetDownsamplePolicy));
            Assert.That(
                manifest.contactSheet.cells.Select(value => value.outputFrame),
                Is.EqualTo(
                    AuditionPvTwelveSecondGoldAssembler
                        .ContactSheetOutputFrames));
            Assert.That(
                manifest.contactSheet.cells.Select(value =>
                    $"{value.sourceShotId}:{value.sourceFrame}"),
                Is.EqualTo(new[]
                {
                    "g01:90",
                    "g02:150",
                    "g02:210",
                    "g02:270",
                    "g03:225",
                    "g03:285",
                    "g04:45",
                    "g04:105",
                    "g04:165",
                    "g04:225",
                    "g06:227",
                    "g06:287"
                }));
            Assert.That(
                manifest.contactSheet.cells.All(cell =>
                    cell.sourceSha256 ==
                    manifest.frames[cell.outputFrame].sha256),
                Is.True);
            AssertContactSheetPixels(result.contactSheetPath);
            AuditionPvTwelveSecondValidationReport report =
                JsonUtility.FromJson<AuditionPvTwelveSecondValidationReport>(
                    File.ReadAllText(
                        result.validationReportPath,
                        Encoding.UTF8));
            Assert.That(
                report.contactSheetFile,
                Is.EqualTo(manifest.contactSheet.file));
            Assert.That(
                report.contactSheetSha256,
                Is.EqualTo(manifest.contactSheet.sha256));
            Assert.That(
                report.contactSheetByteLength,
                Is.EqualTo(manifest.contactSheet.byteLength));
            Assert.That(manifest.proxy.codecName, Is.EqualTo("h264"));
            Assert.That(manifest.proxy.audioStreamCount, Is.Zero);
            Assert.That(manifest.proxy.frameCount, Is.EqualTo(720));
            Assert.That(manifest.proxy.durationSeconds, Is.EqualTo(12d));
            AuditionPvTwelveSecondSourceManifestIdentity g06Identity =
                manifest.sourceManifests.Single(value =>
                    value.captureId == "fixture-g06");
            Assert.That(
                g06Identity.runtimeProofPath,
                Is.EqualTo(fixture.sources["fixture-g06"].runtimeProofPath));
            Assert.That(
                g06Identity.runtimeProofSha256,
                Is.EqualTo(fixture.sources["fixture-g06"].runtimeProofSha256));
            Assert.That(
                manifest.segments[^1].sourceRuntimeProofSha256,
                Is.EqualTo(g06Identity.runtimeProofSha256));
            Assert.That(
                report.g06RuntimeProofPath,
                Is.EqualTo(g06Identity.runtimeProofPath));
            Assert.That(
                report.g06RuntimeProofSha256,
                Is.EqualTo(g06Identity.runtimeProofSha256));
            Assert.DoesNotThrow(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                    result.outputDirectory));
            Assert.That(
                Directory.GetDirectories(
                    fixture.outputRoot,
                    ".*.staging-*",
                    SearchOption.TopDirectoryOnly),
                Is.Empty);
        }

        [Test]
        public void DeterministicFixture_ProducesSameMappingAndHashLedger()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            AuditionPvTwelveSecondAssemblyResult first = Assemble(
                fixture,
                outputId: "fixture-select-a");
            string secondOutputRoot = Path.Combine(temporaryRoot, "selects-b");
            AuditionPvTwelveSecondAssemblyResult second = Assemble(
                fixture,
                outputId: "fixture-select-b",
                outputRoot: secondOutputRoot);

            Assert.That(
                File.ReadAllText(first.frameHashPath, Encoding.UTF8),
                Is.EqualTo(File.ReadAllText(
                    second.frameHashPath,
                    Encoding.UTF8)));
            Assert.That(
                File.ReadAllBytes(first.contactSheetPath),
                Is.EqualTo(File.ReadAllBytes(second.contactSheetPath)));
            AuditionPvTwelveSecondSelectManifest firstManifest =
                AuditionPvTwelveSecondGoldAssembler.ReadInstalledManifest(
                    first.outputDirectory);
            AuditionPvTwelveSecondSelectManifest secondManifest =
                AuditionPvTwelveSecondGoldAssembler.ReadInstalledManifest(
                    second.outputDirectory);
            for (int index = 0; index < 720; index++)
            {
                Assert.That(
                    JsonUtility.ToJson(firstManifest.frames[index]),
                    Is.EqualTo(JsonUtility.ToJson(secondManifest.frames[index])));
            }
        }

        [Test]
        public void SourceManifestTraversal_IsRejectedBeforeOutputRootCreation()
        {
            Fixture fixture = CreateFixture(writeFrames: false);
            string original = fixture.specification.segments[0]
                .sourceManifestPath;
            string captureDirectory = Path.GetDirectoryName(original);
            fixture.specification.segments[0].sourceManifestPath = Path.Combine(
                captureDirectory,
                "..",
                Path.GetFileName(captureDirectory),
                "capture_manifest.json");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("unsafe"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void ExistingDestination_IsNeverOverwritten()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            string destination = Path.Combine(
                fixture.outputRoot,
                "fixture-select");
            Directory.CreateDirectory(destination);
            string sentinel = Path.Combine(destination, "sentinel.txt");
            File.WriteAllText(sentinel, "keep", new UTF8Encoding(false));

            IOException exception = Assert.Throws<IOException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("will not be overwritten"));
            Assert.That(File.ReadAllText(sentinel), Is.EqualTo("keep"));
            Assert.That(
                Directory.GetDirectories(
                    fixture.outputRoot,
                    ".*.staging-*",
                    SearchOption.TopDirectoryOnly),
                Is.Empty);
        }

        [Test]
        public void DirtySourceManifest_IsRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteSourceManifest(fixture, "fixture-city", manifest =>
                manifest.gitWorktreeDirty = true);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("clean"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void WrongPinnedSourceManifestSha_IsRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            fixture.specification.segments[0].sourceManifestSha256 =
                new string('0', 64);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("manifest SHA-256"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void ChangedSourceFrame_IsRejectedAgainstPinnedSourceShaBeforeStaging()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            WriteFixturePng(
                SourceFramePath(fixture, "fixture-city", "g02", 175),
                2560,
                1440,
                "same-header-substituted-frame");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("source-frame SHA-256"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void MissingSourceFrame_IsRejectedBeforeStaging()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            File.Delete(SourceFramePath(fixture, "fixture-city", "g02", 75));

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("exact contiguous"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void WrongPngDimensions_AreRejectedBeforeStaging()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            WriteFixturePng(
                SourceFramePath(fixture, "fixture-city", "g03", 40),
                1920,
                1080,
                "wrong-size");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("2560x1440"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void SegmentOutsideSourceBounds_IsRejectedWithTotalHeldAt720()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            fixture.specification.segments[0].endFrame = 240;
            fixture.specification.segments[1].startFrame = 241;

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("outside source shot"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void SharedDependencyMismatch_IsRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteSourceManifest(fixture, "fixture-g04", manifest =>
                manifest.dependencyHashes[0].sha256 = new string('c', 64));

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("shared dependency"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void G06WithoutCounterBaselineKey_IsRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteSourceManifest(fixture, "fixture-g06", manifest =>
                manifest.baselines = manifest.baselines
                    .Where(value => value.id != "bl07")
                    .ToArray());

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("BL07"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void G06CounterBaselineMustFollowDistinctPerfectDodgeFrame()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteSourceManifest(fixture, "fixture-g06", manifest =>
                manifest.baselines.Single(value => value.id == "bl07")
                    .sourceFrame = manifest.baselines.Single(value =>
                        value.id == "bl06").sourceFrame);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("must precede"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void G06WithoutPassedCounterSemanticEvidence_IsRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteSourceManifest(fixture, "fixture-g06", manifest =>
                manifest.testResults = manifest.testResults.Where(value =>
                    value.name !=
                    "real-station-phase2-perfect-dodge-slot1-counter")
                    .ToArray());

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("semantic evidence test"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void MissingG06RuntimeProof_IsRejectedBeforeOutputReservation()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            File.Delete(fixture.sources["fixture-g06"].runtimeProofPath);

            FileNotFoundException exception = Assert.Throws<FileNotFoundException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("runtime proof"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void ChangedG06RuntimeProof_IsRejectedAgainstPinnedSha()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            File.AppendAllText(
                fixture.sources["fixture-g06"].runtimeProofPath,
                " ",
                new UTF8Encoding(false));

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("runtime-proof SHA-256"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void PinnedG06ProofWithActualProductionOneUlpLexeme_IsAccepted()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteG06RuntimeProof(fixture, runtime =>
            {
                runtime.counterDelta.changedSampleCount = 45235;
                runtime.counterDelta.changedSampleRatio = 45235d / 115200d;
            });
            Assert.That(
                File.ReadAllText(
                    fixture.sources["fixture-g06"].runtimeProofPath,
                    Encoding.UTF8),
                Does.Contain(
                    "\"changedSampleRatio\": 0.39266493055555559"),
                "The fixture must preserve the exact production writer lexeme before validation.");

            AuditionPvTwelveSecondAssemblyResult result = Assemble(fixture);

            Assert.That(result.frameCount, Is.EqualTo(720));
            Assert.That(Directory.Exists(result.outputDirectory), Is.True);
        }

        [Test]
        public void G06JsonLexicalEquivalence_AllowsOnlyDeclaredOneUlpDouble()
        {
            string[] declaredDoubleMetricPaths =
            {
                "runtime.visualMetrics.blackRatio",
                "runtime.visualMetrics.magentaRatio",
                "runtime.visualMetrics.maximumFrameMagentaRatio",
                "runtime.screenDelta.meanAbsoluteRgb",
                "runtime.screenDelta.changedSampleRatio",
                "runtime.counterDelta.meanAbsoluteRgb",
                "runtime.counterDelta.changedSampleRatio"
            };
            foreach (string path in declaredDoubleMetricPaths)
            {
                Assert.DoesNotThrow(
                    () => AuditionPvTwelveSecondGoldAssembler
                        .ValidateG06JsonLexicalEquivalenceForTests(
                            JsonNumberAtPropertyPath(
                                path,
                                "0.39266493055555559"),
                            JsonNumberAtPropertyPath(
                                path,
                                "0.3926649305555556")),
                    path);
            }

            string canonical = JsonNumberAtPropertyPath(
                "runtime.counterDelta.changedSampleRatio",
                "0.3926649305555556");

            double canonicalValue = double.Parse(
                "0.3926649305555556",
                CultureInfo.InvariantCulture);
            double twoUlpsAway = BitConverter.Int64BitsToDouble(
                BitConverter.DoubleToInt64Bits(canonicalValue) + 2L);
            string twoUlpSource = JsonNumberAtPropertyPath(
                "runtime.counterDelta.changedSampleRatio",
                twoUlpsAway.ToString("R", CultureInfo.InvariantCulture));
            InvalidDataException twoUlpException =
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvTwelveSecondGoldAssembler
                        .ValidateG06JsonLexicalEquivalenceForTests(
                            twoUlpSource,
                            canonical));
            Assert.That(twoUlpException.Message, Does.Contain("ULPs"));

            string wrongPathSource =
                "{\"runtime\":{\"hudEnergyMaxMana\":"
                + "0.39266493055555559}}";
            string wrongPathCanonical =
                "{\"runtime\":{\"hudEnergyMaxMana\":"
                + "0.3926649305555556}}";
            InvalidDataException wrongPathException =
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvTwelveSecondGoldAssembler
                        .ValidateG06JsonLexicalEquivalenceForTests(
                            wrongPathSource,
                            wrongPathCanonical));
            Assert.That(
                wrongPathException.Message,
                Does.Contain("lexical structure"));
        }

        [Test]
        public void G06JsonLexicalEquivalence_RejectsAllStructuralDrift()
        {
            const string canonical =
                "{\n"
                + "  \"runtime\": {\n"
                + "    \"counterDelta\": {\n"
                + "      \"changedSampleRatio\": 0.3926649305555556,\n"
                + "      \"changedSampleCount\": 45235\n"
                + "    }\n"
                + "  }\n"
                + "}\n";
            const string productionLexeme =
                "{\n"
                + "  \"runtime\": {\n"
                + "    \"counterDelta\": {\n"
                + "      \"changedSampleRatio\": 0.39266493055555559,\n"
                + "      \"changedSampleCount\": 45235\n"
                + "    }\n"
                + "  }\n"
                + "}\n";
            var mutations = new[]
            {
                new
                {
                    name = "unknown-key",
                    json = productionLexeme.Replace(
                        "      \"changedSampleRatio\"",
                        "      \"unknown\": true,\n"
                        + "      \"changedSampleRatio\"")
                },
                new
                {
                    name = "duplicate-key",
                    json = productionLexeme.Replace(
                        "      \"changedSampleCount\": 45235\n",
                        "      \"changedSampleCount\": 45235,\n"
                        + "      \"changedSampleCount\": 45235\n")
                },
                new
                {
                    name = "property-order",
                    json =
                        "{\n"
                        + "  \"runtime\": {\n"
                        + "    \"counterDelta\": {\n"
                        + "      \"changedSampleCount\": 45235,\n"
                        + "      \"changedSampleRatio\": 0.39266493055555559\n"
                        + "    }\n"
                        + "  }\n"
                        + "}\n"
                },
                new
                {
                    name = "whitespace",
                    json = productionLexeme.Replace(
                        "  \"runtime\"",
                        "   \"runtime\"")
                },
                new
                {
                    name = "trailing-whitespace",
                    json = productionLexeme + " "
                },
                new
                {
                    name = "integer-number",
                    json = productionLexeme.Replace("45235\n", "45236\n")
                }
            };

            foreach (var mutation in mutations)
            {
                Assert.That(
                    () => AuditionPvTwelveSecondGoldAssembler
                        .ValidateG06JsonLexicalEquivalenceForTests(
                            mutation.json,
                            canonical),
                    Throws.TypeOf<InvalidDataException>(),
                    mutation.name);
            }
        }

        [Test]
        public void G06JsonLexer_RejectsInvalidNumberAndStringStates()
        {
            const string canonical = "{\"value\":0.5}";
            string[] invalidSources =
            {
                "{\"value\":01.5}",
                "{\"value\":0.}",
                "{\"value\":0.5e+}",
                "{\"value\":\"bad\\q\"}",
                "{\"value\":\"unterminated}",
                "{\"value\":\"bad" + '\u0001' + "\"}",
                "\ufeff" + canonical
            };

            foreach (string invalid in invalidSources)
            {
                Assert.That(
                    () => AuditionPvTwelveSecondGoldAssembler
                        .ValidateG06JsonLexicalEquivalenceForTests(
                            invalid,
                            canonical),
                    Throws.TypeOf<InvalidDataException>());
            }
        }

        [Test]
        public void PinnedG06ProofWithInvalidCounterPredicate_IsRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteG06RuntimeProof(fixture, runtime =>
                runtime.bossCounterDamageEventCount = 0);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("runtime proof"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void PinnedG06ProofWithImpossibleSampleCount_IsRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteG06RuntimeProof(fixture, runtime =>
                runtime.counterDelta.sampleCount--);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("internally inconsistent"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void G06FailureArtifact_IsRejectedBeforeOutputReservation()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            File.WriteAllText(
                Path.Combine(
                    Path.GetDirectoryName(
                        fixture.sources["fixture-g06"].manifestPath),
                    "g06_capture_failure_20260816T000000000Z.json"),
                "{}\n",
                new UTF8Encoding(false));

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("capture-failure"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void G06FailureAppearingDuringAssembly_BlocksAtomicInstall()
        {
            Fixture fixture = CreateFixture(writeFrames: true);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(
                    fixture,
                    beforeFinalGitProbe: () => File.WriteAllText(
                        Path.Combine(
                            Path.GetDirectoryName(
                                fixture.sources["fixture-g06"].manifestPath),
                            "g06_capture_failure.json"),
                        "{}\n",
                        new UTF8Encoding(false))));

            Assert.That(exception.Message, Does.Contain("capture-failure"));
            Assert.That(
                Directory.Exists(Path.Combine(
                    fixture.outputRoot,
                    "fixture-select")),
                Is.False);
            Assert.That(
                Directory.GetDirectories(
                    fixture.outputRoot,
                    ".*.staging-*",
                    SearchOption.TopDirectoryOnly),
                Is.Empty);
        }

        [Test]
        public void G06SemanticResultMustLinkCanonicalRuntimeProof()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            RewriteSourceManifest(fixture, "fixture-g06", manifest =>
                manifest.testResults.Single(value =>
                        value.suite == "product-state")
                    .artifactPath = "substituted-proof.json");

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("canonically linked"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void DirtyCurrentWorktree_IsRejectedBeforeOutputRootCreation()
        {
            Fixture fixture = CreateFixture(writeFrames: false);
            fixture.git.isDirty = true;

            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() => Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("clean Git snapshot"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void WrongPinnedProxyToolSha_IsRejectedBeforeOutputRootCreation()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            fixture.specification.proxyTools.ffmpegSha256 = new string('d', 64);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("ffmpeg"));
            Assert.That(Directory.Exists(fixture.outputRoot), Is.False);
        }

        [Test]
        public void InvalidProxyProbe_LeavesNoInstalledOrStagingPackage()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            fixture.proxyEncoder = new FixtureProxyEncoder(includeAudio: true);

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                Assemble(fixture));

            Assert.That(exception.Message, Does.Contain("Silent H.264 proxy"));
            Assert.That(
                Directory.Exists(Path.Combine(
                    fixture.outputRoot,
                    "fixture-select")),
                Is.False);
            Assert.That(
                Directory.GetDirectories(
                    fixture.outputRoot,
                    ".*.staging-*",
                    SearchOption.TopDirectoryOnly),
                Is.Empty);
        }

        [Test]
        public void TamperedInstalledFrame_FailsShaValidation()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            AuditionPvTwelveSecondAssemblyResult result = Assemble(fixture);
            string frame = Path.Combine(
                result.outputDirectory,
                "frames",
                "frame_0400.png");
            using (var stream = new FileStream(frame, FileMode.Append, FileAccess.Write))
            {
                stream.WriteByte(0xff);
            }

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                    result.outputDirectory));

            Assert.That(exception.Message, Does.Contain("SHA-256 mismatch"));
        }

        [Test]
        public void TamperedInstalledSourceProvenance_FailsCrossValidation()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            AuditionPvTwelveSecondAssemblyResult result = Assemble(fixture);
            AuditionPvTwelveSecondSelectManifest manifest =
                AuditionPvTwelveSecondGoldAssembler.ReadInstalledManifest(
                    result.outputDirectory);
            manifest.frames[0].sourceRelativePath =
                "frames/g02/frame_0000.png";
            File.WriteAllText(
                result.manifestPath,
                JsonUtility.ToJson(manifest, true) + Environment.NewLine,
                new UTF8Encoding(false));

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                    result.outputDirectory));

            Assert.That(exception.Message, Does.Contain("frame mapping"));
        }

        [Test]
        public void TamperedInstalledG06ProofLinkage_FailsCrossValidation()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            AuditionPvTwelveSecondAssemblyResult result = Assemble(fixture);
            AuditionPvTwelveSecondSelectManifest manifest =
                AuditionPvTwelveSecondGoldAssembler.ReadInstalledManifest(
                    result.outputDirectory);
            manifest.segments[^1].sourceRuntimeProofSha256 = new string('0', 64);
            File.WriteAllText(
                result.manifestPath,
                JsonUtility.ToJson(manifest, true) + Environment.NewLine,
                new UTF8Encoding(false));

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                    result.outputDirectory));

            Assert.That(exception.Message, Does.Contain("runtime-proof identity"));
        }

        [Test]
        public void ContactSheetMutationMissingAndWrongDimensions_AreRejected()
        {
            Fixture fixture = CreateFixture(writeFrames: true);
            AuditionPvTwelveSecondAssemblyResult result = Assemble(fixture);
            byte[] original = File.ReadAllBytes(result.contactSheetPath);
            byte[] originalReport = File.ReadAllBytes(
                result.validationReportPath);

            using (var stream = new FileStream(
                       result.contactSheetPath,
                       FileMode.Append,
                       FileAccess.Write))
            {
                stream.WriteByte(0x5a);
            }

            InvalidDataException mutation = Assert.Throws<InvalidDataException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                    result.outputDirectory));
            Assert.That(mutation.Message, Does.Contain("contact-sheet"));

            File.WriteAllBytes(result.contactSheetPath, original);
            File.Delete(result.contactSheetPath);
            FileNotFoundException missing = Assert.Throws<FileNotFoundException>(() =>
                AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                    result.outputDirectory));
            Assert.That(missing.Message, Does.Contain("contact-sheet"));

            File.WriteAllBytes(result.contactSheetPath, original);
            WriteFixturePng(
                result.contactSheetPath,
                640,
                360,
                "wrong-contact-sheet-dimensions");
            InvalidDataException dimensions =
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                        result.outputDirectory));
            Assert.That(dimensions.Message, Does.Contain("2560x1080"));

            File.WriteAllBytes(result.contactSheetPath, original);
            AuditionPvTwelveSecondValidationReport report =
                JsonUtility.FromJson<AuditionPvTwelveSecondValidationReport>(
                    File.ReadAllText(
                        result.validationReportPath,
                        Encoding.UTF8));
            report.contactSheetSha256 = new string('0', 64);
            File.WriteAllText(
                result.validationReportPath,
                JsonUtility.ToJson(report, true) + Environment.NewLine,
                new UTF8Encoding(false));
            InvalidDataException reportMismatch =
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                        result.outputDirectory));
            Assert.That(
                reportMismatch.Message,
                Does.Contain("validation report"));

            File.WriteAllBytes(result.validationReportPath, originalReport);
            AuditionPvTwelveSecondSelectManifest manifest =
                AuditionPvTwelveSecondGoldAssembler.ReadInstalledManifest(
                    result.outputDirectory);
            manifest.contactSheet.cells[0].sourceFrame++;
            File.WriteAllText(
                result.manifestPath,
                JsonUtility.ToJson(manifest, true) + Environment.NewLine,
                new UTF8Encoding(false));
            InvalidDataException mapping =
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(
                        result.outputDirectory));
            Assert.That(mapping.Message, Does.Contain("cell mapping"));
        }

        private AuditionPvTwelveSecondAssemblyResult Assemble(
            Fixture fixture,
            string outputId = "fixture-select",
            string outputRoot = null,
            Action beforeFinalGitProbe = null)
        {
            return AuditionPvTwelveSecondGoldAssembler.Assemble(
                fixture.specification,
                fixture.sourceRoot,
                outputRoot ?? fixture.outputRoot,
                fixture.git,
                new DateTime(2026, 8, 16, 0, 0, 0, DateTimeKind.Utc),
                outputId,
                () =>
                {
                    beforeFinalGitProbe?.Invoke();
                    return CloneGit(fixture.git);
                },
                fixture.proxyEncoder);
        }

        private Fixture CreateFixture(bool writeFrames)
        {
            string sourceRoot = Path.Combine(temporaryRoot, "gold");
            string outputRoot = Path.Combine(temporaryRoot, "selects");
            string toolsRoot = Path.Combine(temporaryRoot, "tools");
            Directory.CreateDirectory(sourceRoot);
            Directory.CreateDirectory(toolsRoot);
            string ffmpegPath = Path.Combine(toolsRoot, "ffmpeg.exe");
            string ffprobePath = Path.Combine(toolsRoot, "ffprobe.exe");
            File.WriteAllText(
                ffmpegPath,
                "fixture-ffmpeg-8.1.2",
                new UTF8Encoding(false));
            File.WriteAllText(
                ffprobePath,
                "fixture-ffprobe-8.1.2",
                new UTF8Encoding(false));

            var fixture = new Fixture
            {
                sourceRoot = sourceRoot,
                outputRoot = outputRoot,
                git = CleanGit(),
                proxyEncoder = new FixtureProxyEncoder(includeAudio: false)
            };

            SourceCapture city = CreateCapture(
                fixture,
                "fixture-city",
                new[]
                {
                    Shot("g01", 240, "hud-off"),
                    Shot("g02", 420, "hud-on"),
                    Shot("g03", 300, "hud-off")
                },
                new[]
                {
                    Baseline("bl01", "g01", 120, "hud-off"),
                    Baseline("bl02", "g02", 240, "hud-on")
                },
                writeFrames);
            SourceCapture g04 = CreateCapture(
                fixture,
                "fixture-g04",
                new[] { Shot("g04", 238, "hud-off") },
                new[]
                {
                    Baseline("bl04", "g04", 66, "hud-off"),
                    Baseline("bl05", "g04", 178, "hud-off")
                },
                writeFrames);
            SourceCapture g06 = CreateCapture(
                fixture,
                "fixture-g06",
                new[] { Shot("g06", 360, "hud-on") },
                new[]
                {
                    Baseline("bl06", "g06", 189, "hud-on"),
                    Baseline("bl07", "g06", 251, "hud-on")
                },
                writeFrames);

            fixture.sources = new Dictionary<string, SourceCapture>(
                StringComparer.Ordinal)
            {
                [city.captureId] = city,
                [g04.captureId] = g04,
                [g06.captureId] = g06
            };
            fixture.specification = new AuditionPvTwelveSecondSegmentManifest
            {
                proxyTools = new AuditionPvTwelveSecondProxyToolSpec
                {
                    ffmpegPath = ffmpegPath,
                    ffmpegSha256 = AuditionPvSha256.FileHash(ffmpegPath),
                    ffprobePath = ffprobePath,
                    ffprobeSha256 = AuditionPvSha256.FileHash(ffprobePath)
                },
                segments = new[]
                {
                    Segment("city-wide", 0, city, "g01", 60, 149),
                    Segment("city-gameplay", 1, city, "g02", 150, 299),
                    Segment("dimension-transition", 2, city, "g03", 195, 299),
                    Segment("olympus-c33-c34", 3, g04, "g04", 0, 237),
                    Segment("perfect-dodge-counter", 4, g06, "g06", 180, 316)
                }
            };
            return fixture;
        }

        private SourceCapture CreateCapture(
            Fixture fixture,
            string captureId,
            AuditionPvShotManifestEntry[] shots,
            AuditionPvBaselineManifestEntry[] baselines,
            bool writeFrames)
        {
            string directory = Path.Combine(fixture.sourceRoot, captureId);
            Directory.CreateDirectory(directory);
            string runtimeProofPath = string.Empty;
            string runtimeProofSha256 = string.Empty;
            if (writeFrames)
            {
                foreach (AuditionPvShotManifestEntry shot in shots)
                {
                    for (int frame = shot.startFrame;
                        frame <= shot.endFrame;
                        frame++)
                    {
                        WriteFixturePng(
                            SourceFramePath(
                                fixture.sourceRoot,
                                captureId,
                                shot.id,
                                frame),
                            2560,
                            1440,
                            captureId + "/" + shot.id + "/" + frame);
                    }
                }

                string baselineDirectory = Path.Combine(directory, "baselines");
                Directory.CreateDirectory(baselineDirectory);
                foreach (AuditionPvBaselineManifestEntry baseline in baselines)
                {
                    File.Copy(
                        SourceFramePath(
                            fixture.sourceRoot,
                            captureId,
                            baseline.shotId,
                            baseline.sourceFrame),
                        Path.Combine(baselineDirectory, baseline.fileName));
                }
            }

            if (string.Equals(captureId, "fixture-g06", StringComparison.Ordinal))
            {
                string evidenceDirectory = Path.Combine(
                    directory,
                    AuditionPvTwelveSecondGoldAssembler.G06EvidenceFolderName);
                Directory.CreateDirectory(evidenceDirectory);
                string warmupPath = Path.Combine(
                    evidenceDirectory,
                    AuditionPvTwelveSecondGoldAssembler.G06WarmupEvidenceFileName);
                WriteFixturePng(
                    warmupPath,
                    AuditionPvCaptureContract.Width,
                    AuditionPvCaptureContract.Height,
                    "fixture-g06-warmup-evidence");
                runtimeProofPath = Path.Combine(
                    evidenceDirectory,
                    AuditionPvTwelveSecondGoldAssembler.G06RuntimeProofFileName);
                WriteG06RuntimeProof(
                    runtimeProofPath,
                    captureId,
                    warmupPath,
                    CreatePassingG06RuntimeProof());
                runtimeProofSha256 = AuditionPvSha256.FileHash(runtimeProofPath);
            }

            var manifest = new AuditionPvCaptureManifest
            {
                captureId = captureId,
                createdAtUtc = "2026-08-16T00:00:00.0000000Z",
                outputRoot = Normalize(fixture.sourceRoot),
                outputDirectory = Normalize(directory),
                gitCommitSha = CommitSha,
                gitBranch = "main",
                gitWorktreeDirty = false,
                worktreeDirtyHashSha256 = CleanHash,
                unityVersion = "6000.3.5f2",
                unityVersionWithRevision = "6000.3.5f2 (fixture)",
                recorderPackageVersion =
                    AuditionPvCaptureContract.RecorderPackageVersion,
                urpPackageVersion = "17.3.0",
                activeRenderPipelineAssetPath = PipelinePath,
                shots = shots,
                baselines = baselines,
                dependencyHashes = new[]
                {
                    new AuditionPvDependencyHash
                    {
                        path = PipelinePath,
                        exists = true,
                        byteLength = 1,
                        sha256 = DependencySha
                    }
                },
                testResults = FixtureTestResults(
                    captureId,
                    directory,
                    baselines,
                    runtimeProofPath)
            };
            AuditionPvCaptureManifestWriter.Validate(manifest);
            string manifestPath = Path.Combine(
                directory,
                AuditionPvCaptureContract.ManifestFileName);
            File.WriteAllText(
                manifestPath,
                JsonUtility.ToJson(manifest, true) + Environment.NewLine,
                new UTF8Encoding(false));
            return new SourceCapture
            {
                captureId = captureId,
                manifestPath = manifestPath,
                manifest = manifest,
                manifestSha256 = AuditionPvSha256.FileHash(manifestPath),
                dependencyIdentitySha256 =
                    AuditionPvTwelveSecondGoldAssembler
                        .ComputeDependencyIdentityForTests(manifest),
                runtimeProofPath = string.IsNullOrEmpty(runtimeProofPath)
                    ? string.Empty
                    : Normalize(runtimeProofPath),
                runtimeProofSha256 = runtimeProofSha256
            };
        }

        private void RewriteSourceManifest(
            Fixture fixture,
            string captureId,
            Action<AuditionPvCaptureManifest> mutate)
        {
            SourceCapture source = fixture.sources[captureId];
            mutate(source.manifest);
            File.WriteAllText(
                source.manifestPath,
                JsonUtility.ToJson(source.manifest, true) + Environment.NewLine,
                new UTF8Encoding(false));
            source.manifestSha256 = AuditionPvSha256.FileHash(
                source.manifestPath);
            source.dependencyIdentitySha256 =
                AuditionPvTwelveSecondGoldAssembler
                    .ComputeDependencyIdentityForTests(source.manifest);
            foreach (AuditionPvTwelveSecondSegmentSpec segment in
                     fixture.specification.segments.Where(value =>
                         string.Equals(
                             value.sourceManifestPath,
                             source.manifestPath,
                             StringComparison.OrdinalIgnoreCase)))
            {
                segment.sourceManifestSha256 = source.manifestSha256;
                segment.sourceDependencyIdentitySha256 =
                    source.dependencyIdentitySha256;
            }
        }

        private static void RewriteG06RuntimeProof(
            Fixture fixture,
            Action<AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof>
                mutate)
        {
            SourceCapture source = fixture.sources["fixture-g06"];
            AuditionPvG06RuntimeProofArtifact artifact =
                JsonUtility.FromJson<AuditionPvG06RuntimeProofArtifact>(
                    File.ReadAllText(source.runtimeProofPath, Encoding.UTF8));
            mutate(artifact.runtime);
            File.WriteAllText(
                source.runtimeProofPath,
                JsonUtility.ToJson(artifact, true) + Environment.NewLine,
                new UTF8Encoding(false));
            source.runtimeProofSha256 = AuditionPvSha256.FileHash(
                source.runtimeProofPath);
            fixture.specification.segments[^1].sourceRuntimeProofSha256 =
                source.runtimeProofSha256;
        }

        private static void RewritePinnedG06RuntimeProofText(
            Fixture fixture,
            Func<string, string> mutate)
        {
            SourceCapture source = fixture.sources["fixture-g06"];
            string original = File.ReadAllText(
                source.runtimeProofPath,
                Encoding.UTF8);
            string rewritten = mutate(original)
                ?? throw new InvalidOperationException(
                    "The runtime-proof text mutation returned null.");
            File.WriteAllText(
                source.runtimeProofPath,
                rewritten,
                new UTF8Encoding(false));
            source.runtimeProofSha256 = AuditionPvSha256.FileHash(
                source.runtimeProofPath);
            fixture.specification.segments[^1].sourceRuntimeProofSha256 =
                source.runtimeProofSha256;
        }

        private static string ReplaceExactlyOnce(
            string value,
            string oldValue,
            string newValue)
        {
            int first = value.IndexOf(oldValue, StringComparison.Ordinal);
            if (first < 0 ||
                value.IndexOf(
                    oldValue,
                    first + oldValue.Length,
                    StringComparison.Ordinal) >= 0)
            {
                throw new InvalidOperationException(
                    "Expected exactly one runtime-proof lexical replacement for '"
                    + oldValue
                    + "'.");
            }

            return value.Substring(0, first)
                   + newValue
                   + value.Substring(first + oldValue.Length);
        }

        private static string JsonNumberAtPropertyPath(
            string propertyPath,
            string number)
        {
            string value = number;
            string[] properties = propertyPath.Split(
                new[] { '.' },
                StringSplitOptions.None);
            for (int index = properties.Length - 1; index >= 0; index--)
            {
                value = "{\"" + properties[index] + "\":" + value + "}";
            }

            return value;
        }

        private static void WriteG06RuntimeProof(
            string path,
            string captureId,
            string warmupPath,
            AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof runtime)
        {
            runtime.warmupEvidencePath = Normalize(warmupPath);
            runtime.warmupEvidenceSha256 = AuditionPvSha256.FileHash(warmupPath);
            var artifact = new AuditionPvG06RuntimeProofArtifact
            {
                schema = AuditionPvG06RuntimeProofArtifact.Schema,
                captureId = captureId,
                mapping = AuditionPvG06RuntimeProofArtifact.Mapping,
                productScreenProfile =
                    AuditionPvG06RuntimeProofArtifact.ProductScreenProfile,
                summonCounterContract =
                    AuditionPvG06RuntimeProofArtifact.SummonCounterContract,
                runtime = runtime
            };
            File.WriteAllText(
                path,
                JsonUtility.ToJson(artifact, true) + Environment.NewLine,
                new UTF8Encoding(false));
        }

        private static AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof
            CreatePassingG06RuntimeProof()
        {
            return new AuditionPvStationPhase2SummonCounterGoldenRunner.RuntimeProof
            {
                directorCompleted = true,
                lastLogicalFrame = 359,
                presentedFrameCount = 360,
                presentedFramesExact = true,
                presentationClockExact = true,
                perfectDodgeCount = 1,
                firedProjectileCount = 7,
                usedActualCrushNetPattern = true,
                impactAppliedOrBlocked = true,
                impactProjectileInactive = true,
                damageBlockedObservationCount = 1,
                damageModifyingObservationCount = 0,
                playerHealthUnchanged = true,
                cameraCueRequested = true,
                screenCueRequested = true,
                screenCueActiveAtBaselineFrame = true,
                productScreenProfileActive = true,
                bossRiskAtFirstFrame = 0.6f,
                bossRiskAtFireFrame = 0.9f,
                bossRiskAtImpactFrame = 0.91f,
                exactHudRenderable = true,
                exactHudResources = true,
                exactEnergyBinding = true,
                hudAmmo = 12,
                hudMagazineSize = 12,
                hudEnergyMana = 100f,
                hudEnergyMaxMana = 300f,
                summonEnergyBeforeUse = 300f,
                summonEnergyAfterUse = 100f,
                summonSpentTier = 2,
                summonUseCountDelta = 1,
                summonInterceptCountDelta = 1,
                summonUsedEventCount = 1,
                summonBlockedEventCount = 1,
                screenInterceptEventCount = 1,
                screenFirstObservedFrame = 239,
                summonPressureScreenTier = 2,
                summonPressureScreenRemainingIntercepts = 1,
                uniqueSummonPressureScreenObserved = true,
                retainedProjectileCountBeforeIntercept = 6,
                retainedProjectileIdentitySetExact = true,
                retainedProjectileImpactApplied = true,
                retainedProjectileInactive = true,
                activeCounterProjectileCountAfterIntercept = 1,
                bossDamageEventCount = 1,
                bossAllyDamageEventCount = 1,
                bossCounterDamageEventCount = 1,
                bossCounterDamageFrame = 280,
                counterProjectileDamageAppliedCount = 1,
                counterProjectileDamageAppliedFrame = 280,
                authoredCounterDamage = 29.439999f,
                bossCounterDamageAmount = 29.439999f,
                bossCounterHealthDelta = 29.439999f,
                fixedDeltaTimeExact = true,
                recorderWarmupEndOfFrameCount = 2,
                recorderPaddingActiveAtLogicalFrameZero = true,
                recorderCaptureDeltaTimeAtLogicalFrameZero =
                    1f / AuditionPvCaptureContract.Fps + 0.0001f,
                recorderAutoStoppedAfterLastFrame = true,
                stateRestored = true,
                screenProfileRestored = true,
                fixedDeltaTimeRestored = true,
                captureInputLocksReleased = true,
                captureHudStateRestored = true,
                captureEventsReleased = true,
                captureSummonArtifactsReleased = true,
                bossCompositionRestored = true,
                presentationClockReleased = true,
                cadenceSuspensionCountAfterRestore = 0,
                visualMetrics = new AuditionPvStationPhase2SummonCounterGoldenRunner
                    .SequenceVisualMetrics
                {
                    sampleCount = 1296000,
                    blackSampleCount = 12960,
                    magentaSampleCount = 1296,
                    healthyFrameCount = 360,
                    magentaAffectedFrameCount = 1,
                    blackRatio = 0.01d,
                    magentaRatio = 0.001d,
                    maximumFrameMagentaRatio = 0.001d,
                    minimumSampledLuma = 12,
                    maximumSampledLuma = 180,
                    frameZeroHudAccentSamples = 12
                },
                screenDelta = new AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ScreenDeltaMetrics
                {
                    sampleCount = 115200,
                    changedSampleCount = 11520,
                    meanAbsoluteRgb = 2.5d,
                    changedSampleRatio = 0.1d
                },
                counterDelta = new AuditionPvStationPhase2SummonCounterGoldenRunner
                    .ScreenDeltaMetrics
                {
                    sampleCount = 115200,
                    changedSampleCount = 11520,
                    meanAbsoluteRgb = 1.5d,
                    changedSampleRatio = 0.1d
                }
            };
        }

        private static AuditionPvShotManifestEntry Shot(
            string id,
            int frameCount,
            string hudMode)
        {
            return new AuditionPvShotManifestEntry
            {
                id = id,
                scenePath = "Assets/_Game/Scenes/Fixture.unity",
                startFrame = 0,
                endFrame = frameCount - 1,
                expectedFrameCount = frameCount,
                hudMode = hudMode,
                notes = "fixture"
            };
        }

        private static AuditionPvBaselineManifestEntry Baseline(
            string id,
            string shotId,
            int frame,
            string hudMode)
        {
            return new AuditionPvBaselineManifestEntry
            {
                id = id,
                shotId = shotId,
                sourceFrame = frame,
                fileName = id.ToUpperInvariant() + "_FIXTURE.png",
                hudMode = hudMode,
                status = "captured"
            };
        }

        private static AuditionPvTestResult[] FixtureTestResults(
            string captureId,
            string captureDirectory,
            AuditionPvBaselineManifestEntry[] baselines,
            string runtimeProofPath)
        {
            if (!string.Equals(
                    captureId,
                    "fixture-g06",
                    StringComparison.Ordinal))
            {
                return new[]
                {
                    new AuditionPvTestResult
                    {
                        suite = "fixture",
                        name = "deterministic-source",
                        status = "passed",
                        durationMilliseconds = 0,
                        details = "fixture",
                        artifactPath = "fixture"
                    }
                };
            }

            string evidenceDirectory = Path.Combine(
                captureDirectory,
                AuditionPvTwelveSecondGoldAssembler.G06EvidenceFolderName);
            string baselineDirectory = Path.Combine(captureDirectory, "baselines");
            string Artifact(string baselineId) => Path.Combine(
                baselineDirectory,
                baselines.Single(value => value.id == baselineId).fileName);
            AuditionPvTestResult Passed(
                string suite,
                string name,
                string artifactPath)
            {
                return new AuditionPvTestResult
                {
                    suite = suite,
                    name = name,
                    status = "passed",
                    durationMilliseconds = 0,
                    details = "fixture golden-runner evidence",
                    artifactPath = Normalize(artifactPath)
                };
            }

            return new[]
            {
                Passed(
                    "recorder",
                    "raw-warmup-and-logical-frame-mapping",
                    Path.Combine(
                        evidenceDirectory,
                        AuditionPvTwelveSecondGoldAssembler
                            .G06WarmupEvidenceFileName)),
                Passed(
                    "product-state",
                    "real-station-phase2-perfect-dodge-slot1-counter",
                    runtimeProofPath),
                Passed(
                    "render",
                    "png-hud-and-visual-sanity",
                    Path.Combine(captureDirectory, "frames", "g06")),
                Passed(
                    "render",
                    "perfect-dodge-screen-domain-f189",
                    Artifact("bl06")),
                Passed(
                    "render",
                    "slot1-screen-intercept-counter-f251",
                    Artifact("bl07")),
                Passed(
                    "provenance",
                    "git-dependencies-and-station-scene-stable",
                    runtimeProofPath),
                Passed(
                    "lifecycle",
                    "state-restored-and-product-scene-reopened",
                    runtimeProofPath)
            };
        }

        private static AuditionPvTwelveSecondSegmentSpec Segment(
            string role,
            int order,
            SourceCapture source,
            string shotId,
            int start,
            int end)
        {
            return new AuditionPvTwelveSecondSegmentSpec
            {
                role = role,
                order = order,
                sourceManifestPath = source.manifestPath,
                sourceManifestSha256 = source.manifestSha256,
                sourceDependencyIdentitySha256 =
                    source.dependencyIdentitySha256,
                sourceRuntimeProofSha256 = source.runtimeProofSha256,
                shotId = shotId,
                startFrame = start,
                endFrame = end,
                sourceFrameSha256 = Enumerable.Range(start, end - start + 1)
                    .Select(frame =>
                    {
                        string path = SourceFramePath(
                            source.manifest.outputRoot,
                            source.captureId,
                            shotId,
                            frame);
                        return File.Exists(path)
                            ? AuditionPvSha256.FileHash(path)
                            : AuditionPvSha256.TextHash(
                                "unmaterialized-fixture\0"
                                + source.captureId + "\0" + shotId + "\0"
                                + frame.ToString(CultureInfo.InvariantCulture));
                    })
                    .ToArray()
            };
        }

        private static AuditionPvGitSnapshot CleanGit()
        {
            return new AuditionPvGitSnapshot
            {
                commitSha = CommitSha,
                branch = "main",
                isDirty = false,
                dirtyStateHashSha256 = CleanHash,
                probeSucceeded = true,
                probeError = string.Empty
            };
        }

        private static AuditionPvGitSnapshot CloneGit(AuditionPvGitSnapshot value)
        {
            return new AuditionPvGitSnapshot
            {
                commitSha = value.commitSha,
                branch = value.branch,
                isDirty = value.isDirty,
                dirtyStateHashSha256 = value.dirtyStateHashSha256,
                probeSucceeded = value.probeSucceeded,
                probeError = value.probeError
            };
        }

        private static string SourceFramePath(
            Fixture fixture,
            string captureId,
            string shotId,
            int frame)
        {
            return SourceFramePath(
                fixture.sourceRoot,
                captureId,
                shotId,
                frame);
        }

        private static string SourceFramePath(
            string sourceRoot,
            string captureId,
            string shotId,
            int frame)
        {
            string frameDirectory = string.Equals(
                    shotId,
                    "g04",
                    StringComparison.Ordinal)
                ? Path.Combine(
                    sourceRoot,
                    captureId,
                    AuditionPvStationTransitionGoldenCapture.FramesFolderName)
                : Path.Combine(sourceRoot, captureId, "frames", shotId);
            return Path.Combine(
                frameDirectory,
                "frame_" + frame.ToString(
                    "0000",
                    CultureInfo.InvariantCulture) + ".png");
        }

        private static void WriteFixturePng(
            string path,
            int width,
            int height,
            string payload)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (width == AuditionPvCaptureContract.Width &&
                height == AuditionPvCaptureContract.Height &&
                ContactSamplePayloads.TryGetValue(
                    payload,
                    out int contactCellIndex))
            {
                File.WriteAllBytes(
                    path,
                    GetFixtureContactSourcePng(contactCellIndex));
                return;
            }

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload ?? string.Empty);
            var bytes = new byte[29 + payloadBytes.Length];
            byte[] header =
            {
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a,
                0x00, 0x00, 0x00, 0x0d,
                (byte)'I', (byte)'H', (byte)'D', (byte)'R',
                (byte)(width >> 24), (byte)(width >> 16),
                (byte)(width >> 8), (byte)width,
                (byte)(height >> 24), (byte)(height >> 16),
                (byte)(height >> 8), (byte)height,
                0x08, 0x06, 0x00, 0x00, 0x00
            };
            Buffer.BlockCopy(header, 0, bytes, 0, header.Length);
            Buffer.BlockCopy(
                payloadBytes,
                0,
                bytes,
                header.Length,
                payloadBytes.Length);
            File.WriteAllBytes(path, bytes);
        }

        private static void AssertContactSheetPixels(string path)
        {
            var texture = new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: true);
            try
            {
                Assert.That(
                    texture.LoadImage(
                        File.ReadAllBytes(path),
                        markNonReadable: false),
                    Is.True);
                Assert.That(texture.width, Is.EqualTo(2560));
                Assert.That(texture.height, Is.EqualTo(1080));
                Color32[] pixels = texture.GetPixels32();
                for (int index = 0;
                     index < AuditionPvTwelveSecondGoldAssembler
                         .ContactSheetOutputFrames.Length;
                     index++)
                {
                    int column = index % 4;
                    int rowFromTop = index / 4;
                    int x = column * 640 + 320;
                    int y = (2 - rowFromTop) * 360 + 180;
                    Color32 expected = new(
                        24,
                        24,
                        (byte)(32 + index),
                        255);
                    Assert.That(
                        pixels[y * texture.width + x],
                        Is.EqualTo(expected),
                        "contact cell " + index);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static byte[] GetFixtureContactSourcePng(int contactCellIndex)
        {
            lock (FixturePngLock)
            {
                if (FixtureContactSourcePngs.TryGetValue(
                        contactCellIndex,
                        out byte[] cached))
                {
                    return cached;
                }

                int width = AuditionPvCaptureContract.Width;
                int height = AuditionPvCaptureContract.Height;
                var pixels = new Color32[width * height];
                for (int y = 0; y < height; y++)
                {
                    int row = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        pixels[row + x] = new Color32(
                            (byte)((x % 4) * 16),
                            (byte)((y % 4) * 16),
                            (byte)(32 + contactCellIndex),
                            255);
                    }
                }

                var texture = new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    mipChain: false,
                    linear: true);
                try
                {
                    texture.SetPixels32(pixels);
                    texture.Apply(
                        updateMipmaps: false,
                        makeNoLongerReadable: false);
                    cached = texture.EncodeToPNG();
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                if (cached == null || cached.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Could not encode the fixture contact-sheet source PNG.");
                }

                FixtureContactSourcePngs.Add(contactCellIndex, cached);
                return cached;
            }
        }

        private static string Normalize(string path)
        {
            return Path.GetFullPath(path).Replace('\\', '/');
        }

        private sealed class Fixture
        {
            public string sourceRoot;
            public string outputRoot;
            public AuditionPvGitSnapshot git;
            public AuditionPvTwelveSecondSegmentManifest specification;
            public Dictionary<string, SourceCapture> sources;
            public IAuditionPvTwelveSecondProxyEncoder proxyEncoder;
        }

        private sealed class SourceCapture
        {
            public string captureId;
            public string manifestPath;
            public string manifestSha256;
            public string dependencyIdentitySha256;
            public string runtimeProofPath;
            public string runtimeProofSha256;
            public AuditionPvCaptureManifest manifest;
        }

        private sealed class FixtureProxyEncoder :
            IAuditionPvTwelveSecondProxyEncoder
        {
            private readonly bool includeAudio;

            public FixtureProxyEncoder(bool includeAudio)
            {
                this.includeAudio = includeAudio;
            }

            public AuditionPvTwelveSecondProxyArtifact Encode(
                string stagingDirectory,
                AuditionPvTwelveSecondProxyToolSpec tools)
            {
                string proxyPath = Path.Combine(
                    stagingDirectory,
                    AuditionPvTwelveSecondGoldAssembler.ProxyFileName);
                File.WriteAllBytes(
                    proxyPath,
                    Encoding.UTF8.GetBytes("fixture-silent-h264-proxy"));
                string audio = includeAudio
                    ? ",{\"codec_type\":\"audio\"}"
                    : string.Empty;
                string probeJson =
                    "{\"streams\":[{\"codec_name\":\"h264\","
                    + "\"codec_type\":\"video\",\"width\":2560,"
                    + "\"height\":1440,\"pix_fmt\":\"yuv420p\","
                    + "\"r_frame_rate\":\"60/1\","
                    + "\"avg_frame_rate\":\"60/1\","
                    + "\"duration\":\"12.000000\","
                    + "\"nb_frames\":\"720\"}"
                    + audio
                    + "],\"format\":{\"duration\":\"12.000000\"}}\n";
                string probePath = Path.Combine(
                    stagingDirectory,
                    AuditionPvTwelveSecondGoldAssembler.ProxyProbeFileName);
                File.WriteAllText(
                    probePath,
                    probeJson,
                    new UTF8Encoding(false));
                return new AuditionPvTwelveSecondProxyArtifact
                {
                    proxyFile =
                        AuditionPvTwelveSecondGoldAssembler.ProxyFileName,
                    proxySha256 = AuditionPvSha256.FileHash(proxyPath),
                    proxyByteLength = new FileInfo(proxyPath).Length,
                    probeFile =
                        AuditionPvTwelveSecondGoldAssembler.ProxyProbeFileName,
                    probeSha256 = AuditionPvSha256.FileHash(probePath),
                    ffmpegPath = tools.ffmpegPath,
                    ffmpegSha256 = tools.ffmpegSha256,
                    ffmpegVersionLine =
                        "ffmpeg version 8.1.2-essentials_build fixture",
                    ffprobePath = tools.ffprobePath,
                    ffprobeSha256 = tools.ffprobeSha256,
                    ffprobeVersionLine =
                        "ffprobe version 8.1.2-essentials_build fixture",
                    codecName = "h264",
                    pixelFormat = "yuv420p",
                    width = 2560,
                    height = 1440,
                    rFrameRate = "60/1",
                    avgFrameRate = "60/1",
                    frameCount = 720,
                    durationSeconds = 12d,
                    videoStreamCount = 1,
                    audioStreamCount = includeAudio ? 1 : 0,
                    silent = !includeAudio
                };
            }
        }
    }
}
