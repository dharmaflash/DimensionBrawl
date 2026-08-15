using System;
using System.IO;
using NUnit.Framework;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvCaptureFoundationTests
    {
        [Test]
        public void Contract_IsLosslessQhdAtSixtyFps()
        {
            Assert.That(AuditionPvCaptureContract.OutputRoot, Is.EqualTo("D:/DimensionBrawl_PV/01_capture_video/PREEDIT_GOLD"));
            Assert.That(AuditionPvCaptureContract.Width, Is.EqualTo(2560));
            Assert.That(AuditionPvCaptureContract.Height, Is.EqualTo(1440));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
            Assert.That(AuditionPvCaptureContract.SourceFormat, Does.Contain("png_sequence"));
            Assert.That(AuditionPvCaptureContract.RecorderPackageVersion, Is.EqualTo("5.1.6"));
        }

        [Test]
        public void OutputId_IsSafeDeterministicAndProvenanced()
        {
            var time = new DateTime(2026, 8, 15, 12, 34, 56, DateTimeKind.Utc);
            string outputId = AuditionPvOutputPaths.CreateOutputId(
                "Station Boss / Gold Take",
                time,
                "0123456789abcdef0123456789abcdef01234567",
                true,
                new string('a', 64));

            Assert.That(
                outputId,
                Is.EqualTo("20260815t123456z_station-boss-gold-take_g0123456789ab_dirty-aaaaaaaaaaaa"));
            Assert.DoesNotThrow(() => AuditionPvOutputPaths.ValidateOutputId(outputId));
            Assert.That(outputId, Does.Not.Contain("/"));
            Assert.That(outputId, Does.Not.Contain(".."));
        }

        [Test]
        public void OutputPath_RejectsTraversalAndSanitizesReservedNames()
        {
            Assert.That(AuditionPvOutputPaths.SanitizeSegment("../CON<>"), Is.EqualTo("x-con"));
            Assert.Throws<ArgumentException>(() =>
                AuditionPvOutputPaths.ResolveOutputDirectory("C:/tmp/audition-pv", "../escape"));
            Assert.Throws<ArgumentException>(() =>
                AuditionPvOutputPaths.ResolveOutputDirectory("C:/tmp/audition-pv", "bad/name"));
        }

        [Test]
        public void DirtyStateHash_IsStableAndOrderIndependent()
        {
            string first = AuditionPvEnvironmentProbe.ComputeDirtyStateHash(
                "abc",
                " M file-a\0?? file-b\0",
                new[] { "file-b|2|bb", "file-a|1|aa" });
            string second = AuditionPvEnvironmentProbe.ComputeDirtyStateHash(
                "abc",
                " M file-a\0?? file-b\0",
                new[] { "file-a|1|aa", "file-b|2|bb" });
            string changed = AuditionPvEnvironmentProbe.ComputeDirtyStateHash(
                "abc",
                " M file-a\0?? file-b\0",
                new[] { "file-a|1|changed", "file-b|2|bb" });

            Assert.That(first, Is.EqualTo(second));
            Assert.That(AuditionPvSha256.IsSha256(first), Is.True);
            Assert.That(changed, Is.Not.EqualTo(first));
        }

        [Test]
        public void RecorderFactory_UsesRecorder516PublicPngSequenceApi()
        {
            using AuditionPvRecorderSettingsBundle bundle =
                AuditionPvRecorderSettingsFactory.CreateLosslessPngSequence("C:/tmp/audition-pv", "G04 C33 C34");

            Assert.DoesNotThrow(() => AuditionPvRecorderSettingsFactory.Validate(bundle));
            Assert.That(bundle.controllerSettings.FrameRatePlayback, Is.EqualTo(FrameRatePlayback.Constant));
            Assert.That(bundle.controllerSettings.FrameRate, Is.EqualTo(60f));
            Assert.That(bundle.controllerSettings.CapFrameRate, Is.True);
            Assert.That(bundle.imageSettings.OutputFormat, Is.EqualTo(ImageRecorderSettings.ImageRecorderOutputFormat.PNG));
            Assert.That(bundle.imageSettings.CaptureAlpha, Is.False);
            Assert.That(bundle.imageSettings.imageInputSettings, Is.TypeOf<GameViewInputSettings>());

            var input = (GameViewInputSettings)bundle.imageSettings.imageInputSettings;
            Assert.That(input.OutputWidth, Is.EqualTo(2560));
            Assert.That(input.OutputHeight, Is.EqualTo(1440));
            Assert.That(bundle.imageSettings.OutputFile, Does.Contain(DefaultWildcard.Frame));
            Assert.That(bundle.normalizedShotId, Is.EqualTo("g04-c33-c34"));
        }

        [Test]
        public void ManifestWriter_RoundTripsAllGateProvenanceAndRefusesOverwrite()
        {
            string temporaryRoot = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_AuditionPV_Test_" + Guid.NewGuid().ToString("N"));
            const string captureId = "20260815t123456z_test_g0123456789ab_clean";
            string outputDirectory = AuditionPvOutputPaths.ResolveOutputDirectory(temporaryRoot, captureId);

            try
            {
                AuditionPvCaptureManifest manifest = AuditionPvCaptureManifestFactory.CreateForRoot(
                    captureId,
                    temporaryRoot,
                    outputDirectory,
                    new[]
                    {
                        new AuditionPvShotManifestEntry
                        {
                            id = "g04",
                            scenePath = "Assets/_Game/Scenes/OlympusStationCombatStage.unity",
                            startFrame = 0,
                            endFrame = 59,
                            expectedFrameCount = 60,
                            hudMode = "hud-off"
                        }
                    },
                    new[]
                    {
                        new AuditionPvBaselineManifestEntry
                        {
                            id = "bl04",
                            shotId = "g04",
                            sourceFrame = 33,
                            fileName = "BL04_AKAZA_C33_WING_OPEN__HUDOFF.png",
                            hudMode = "hud-off",
                            status = "planned"
                        }
                    },
                    new[]
                    {
                        new AuditionPvTestResult
                        {
                            suite = "EditMode",
                            name = "foundation",
                            status = "passed"
                        }
                    },
                    createdAtUtc: new DateTime(2026, 8, 15, 12, 34, 56, DateTimeKind.Utc),
                    gitSnapshot: FakeGitSnapshot(),
                    engineSnapshot: FakeEngineSnapshot(),
                    dependencyHashSnapshot: new[]
                    {
                        new AuditionPvDependencyHash
                        {
                            path = "Assets/Settings/PC_RPAsset.asset",
                            exists = true,
                            byteLength = 1,
                            sha256 = new string('b', 64)
                        }
                    });

                string manifestPath = AuditionPvCaptureManifestWriter.WriteNew(manifest);
                Assert.That(File.Exists(manifestPath), Is.True);
                Assert.Throws<IOException>(() => AuditionPvCaptureManifestWriter.WriteNew(manifest));

                AuditionPvCaptureManifest roundTripped =
                    JsonUtility.FromJson<AuditionPvCaptureManifest>(File.ReadAllText(manifestPath));
                Assert.That(roundTripped.gitCommitSha, Is.EqualTo(new string('0', 40)));
                Assert.That(roundTripped.worktreeDirtyHashSha256, Is.EqualTo(new string('a', 64)));
                Assert.That(roundTripped.unityVersion, Is.EqualTo("6000.3.5f2"));
                Assert.That(roundTripped.urpPackageVersion, Is.EqualTo("17.3.0"));
                Assert.That(roundTripped.width, Is.EqualTo(2560));
                Assert.That(roundTripped.height, Is.EqualTo(1440));
                Assert.That(roundTripped.fps, Is.EqualTo(60));
                Assert.That(roundTripped.shots, Has.Length.EqualTo(1));
                Assert.That(roundTripped.baselines, Has.Length.EqualTo(1));
                Assert.That(roundTripped.dependencyHashes, Has.Length.EqualTo(1));
                Assert.That(roundTripped.testResults, Has.Length.EqualTo(1));
            }
            finally
            {
                if (Directory.Exists(temporaryRoot))
                {
                    Directory.Delete(temporaryRoot, true);
                }
            }
        }

        private static AuditionPvGitSnapshot FakeGitSnapshot()
        {
            return new AuditionPvGitSnapshot
            {
                commitSha = new string('0', 40),
                branch = "test",
                isDirty = false,
                dirtyStateHashSha256 = new string('a', 64),
                probeSucceeded = true
            };
        }

        private static AuditionPvEngineSnapshot FakeEngineSnapshot()
        {
            return new AuditionPvEngineSnapshot
            {
                unityVersion = "6000.3.5f2",
                unityVersionWithRevision = "6000.3.5f2 (3fa8bc678cb0)",
                recorderPackageVersion = "5.1.6",
                urpPackageVersion = "17.3.0",
                activeRenderPipelineAssetPath = "Assets/Settings/PC_RPAsset.asset"
            };
        }
    }
}
