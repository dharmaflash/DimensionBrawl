using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationTransitionGoldenCaptureTests
    {
        [Test]
        public void G04Contract_IsExactQhdSixtyFpsHudOffRange()
        {
            AuditionPvShotManifestEntry[] shots =
                AuditionPvStationTransitionGoldenCapture.CreateShotManifestEntries();
            AuditionPvShotManifestEntry shot = shots[0];
            AuditionPvShotManifestEntry cleanPlate = shots[1];

            Assert.That(shots, Has.Length.EqualTo(2));
            Assert.That(shot.id, Is.EqualTo("g04"));
            Assert.That(shot.scenePath, Is.EqualTo(
                "Assets/_Game/Scenes/OlympusStationCombatStage.unity"));
            Assert.That(shot.startFrame, Is.EqualTo(0));
            Assert.That(shot.endFrame, Is.EqualTo(597));
            Assert.That(shot.expectedFrameCount, Is.EqualTo(598));
            Assert.That(shot.hudMode, Is.EqualTo("hud-off"));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.SelectStartFrame,
                Is.EqualTo(180));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.SelectEndFrame,
                Is.EqualTo(417));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.HandleFrameCount,
                Is.EqualTo(180));
            Assert.That(shot.notes, Does.Contain("180-frame handles"));
            Assert.That(cleanPlate.id, Is.EqualTo("g04-clean"));
            Assert.That(cleanPlate.scenePath, Is.EqualTo(shot.scenePath));
            Assert.That(cleanPlate.startFrame, Is.EqualTo(shot.startFrame));
            Assert.That(cleanPlate.endFrame, Is.EqualTo(shot.endFrame));
            Assert.That(cleanPlate.expectedFrameCount, Is.EqualTo(shot.expectedFrameCount));
            Assert.That(cleanPlate.hudMode, Is.EqualTo("clean-plate"));
            Assert.That(cleanPlate.notes, Does.Contain("Byte-exact companion"));
            Assert.That(AuditionPvCaptureContract.Width, Is.EqualTo(2560));
            Assert.That(AuditionPvCaptureContract.Height, Is.EqualTo(1440));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
        }

        [Test]
        public void G04GateIdentityAndSemanticBeats_AreExactAndSplitFromTestResults()
        {
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.GateEvidenceTestSuite,
                Is.EqualTo("AuditionPvSixtySecondEvidence"));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.GateCameraId,
                Is.EqualTo("station-c33-wing-to-c34-eye-authored-cut"));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.GateGameplayState,
                Is.EqualTo("station-phase1-to-phase2-authored-transition"));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.GateTimelineId,
                Is.EqualTo(AuditionPvStationTransitionGoldenCapture.TimelinePath));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.DeterministicRandomSeed,
                Is.EqualTo(0x4704));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.GateSemanticBeatIds(),
                Is.EqualTo(new[] { "c33-wing-deployment", "c34-eye-open" }));

            string source = File.ReadAllText(ProjectAbsolutePath(
                AuditionPvStationTransitionGoldenCapture.CaptureScriptPath));
            Assert.That(source, Does.Contain("Array.Empty<AuditionPvTestResult>()"));
            Assert.That(source, Does.Contain("CaptureCoreSha256(captureCoreManifest)"));
            Assert.That(source, Does.Contain("shot-authorship/"));
            Assert.That(source, Does.Contain("shot-authorship-runtime/"));
            Assert.That(source, Does.Contain("semantic-beat/"));
            Assert.That(source, Does.Contain("sourceCaptureCoreSha256"));
            Assert.That(source, Does.Contain("UnityEngine.Random.InitState"));
            Assert.That(source, Does.Contain("UnityEngine.Random.state = randomState"));
        }

        [TestCase(0, true)]
        [TestCase(179, true)]
        [TestCase(180, true)]
        [TestCase(275, true)]
        [TestCase(276, false)]
        [TestCase(417, false)]
        [TestCase(418, false)]
        [TestCase(597, false)]
        public void CameraRouting_CutsExactlyFromC33ToC34(int frameIndex, bool usesWing)
        {
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.UsesWingCamera(frameIndex),
                Is.EqualTo(usesWing));
        }

        [TestCase(0, 0)]
        [TestCase(179, 0)]
        [TestCase(180, 0)]
        [TestCase(275, 95)]
        [TestCase(276, 96)]
        [TestCase(417, 237)]
        [TestCase(418, 237)]
        [TestCase(597, 237)]
        public void RecordedHandles_ClampToAuthoredTimelineEndpoints(
            int sourceFrame,
            int expectedLogicalFrame)
        {
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.SourceToLogicalFrame(sourceFrame),
                Is.EqualTo(expectedLogicalFrame));
        }

        [Test]
        public void FrameNames_AreContiguousAndRangeChecked()
        {
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.FrameFileName(0),
                Is.EqualTo("frame_0000.png"));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.FrameFileName(597),
                Is.EqualTo("frame_0597.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationTransitionGoldenCapture.FrameFileName(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationTransitionGoldenCapture.FrameFileName(598));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.FrameLedgerRelativePath(0),
                Is.EqualTo("G04_C33_C34_PNG/frame_0000.png"));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.FrameLedgerRelativePath(597),
                Is.EqualTo("G04_C33_C34_PNG/frame_0597.png"));
        }

        [Test]
        public void Baselines_AreExactFramesNamesAndHudOff()
        {
            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationTransitionGoldenCapture.CreateBaselineManifestEntries();

            Assert.That(baselines, Has.Length.EqualTo(2));
            Assert.That(baselines[0].id, Is.EqualTo("bl04"));
            Assert.That(baselines[0].shotId, Is.EqualTo("g04"));
            Assert.That(baselines[0].sourceFrame, Is.EqualTo(246));
            Assert.That(baselines[0].fileName, Is.EqualTo(
                "BL04_AKAZA_C33_WING_OPEN__HUDOFF__t01.100000.png"));
            Assert.That(baselines[0].hudMode, Is.EqualTo("hud-off"));
            Assert.That(baselines[0].status, Is.EqualTo("captured"));

            Assert.That(baselines[1].id, Is.EqualTo("bl05"));
            Assert.That(baselines[1].shotId, Is.EqualTo("g04"));
            Assert.That(baselines[1].sourceFrame, Is.EqualTo(358));
            Assert.That(baselines[1].fileName, Is.EqualTo(
                "BL05_AKAZA_C34_EYE_OPEN__HUDOFF__t02.966667.png"));
            Assert.That(baselines[1].hudMode, Is.EqualTo("hud-off"));
            Assert.That(baselines[1].status, Is.EqualTo("captured"));
        }

        [Test]
        public void ExplicitDependencies_ResolveSceneTimelineActorCameraAndCaptureCode()
        {
            string[] dependencies =
                AuditionPvStationTransitionGoldenCapture.ExplicitProductDependencyPaths();

            Assert.That(dependencies, Does.Contain(
                AuditionPvStationTransitionGoldenCapture.StationScenePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationTransitionGoldenCapture.TimelinePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationTransitionGoldenCapture.C33ActorPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationTransitionGoldenCapture.C34ActorPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationTransitionGoldenCapture.C33CameraPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationTransitionGoldenCapture.C34CameraPath));

            foreach (string dependency in dependencies)
            {
                Assert.That(
                    AssetDatabase.LoadMainAssetAtPath(dependency),
                    Is.Not.Null,
                    dependency);
            }
        }

        [Test]
        public void CaptureImplementation_UsesDirectEvaluateAndExactSerializedHudReference()
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve project root.");
            string sourcePath = Path.Combine(
                projectRoot,
                AuditionPvStationTransitionGoldenCapture.CaptureScriptPath);
            string source = File.ReadAllText(sourcePath);

            Assert.That(source, Does.Contain("bindings.director.Evaluate();"));
            Assert.That(source, Does.Contain("SourceToLogicalFrame(frameIndex)"));
            Assert.That(source, Does.Contain("BuildFrameHashLedger(frameDirectory)"));
            Assert.That(source, Does.Contain("CopyCleanPlateFrames("));
            Assert.That(source, Does.Contain("CleanPlateFramesFolderName"));
            Assert.That(source, Does.Contain(
                "G04 editorial source requires a clean Git snapshot."));
            Assert.That(source, Does.Contain("new SerializedObject(flowController)"));
            Assert.That(source, Does.Contain("combatHudCanvasGroup"));
            Assert.That(source, Does.Contain("group.alpha = 0f;"));
            Assert.That(source, Does.Contain("group.interactable = false;"));
            Assert.That(source, Does.Contain("group.blocksRaycasts = false;"));
            Assert.That(source, Does.Contain("EditorSceneManager.OpenScene(StationScenePath"));
        }

        [Test]
        public void PngValidator_RequiresSignatureAndExactDimensions()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_G04PngTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "frame.png");
            try
            {
                byte[] header = BuildPngHeader(2560, 1440);
                AuditionPvStationTransitionGoldenCapture.WriteBytesNew(path, header);
                Assert.DoesNotThrow(() =>
                    AuditionPvStationTransitionGoldenCapture.ValidatePngFile(
                        path,
                        2560,
                        1440));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationTransitionGoldenCapture.ValidatePngFile(
                        path,
                        1920,
                        1080));
                Assert.Throws<IOException>(() =>
                    AuditionPvStationTransitionGoldenCapture.WriteBytesNew(path, header));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        [Test]
        public void VisualSanity_RejectsBlackAndMissingShaderSequences()
        {
            const int Samples = 598 * 576;
            Assert.DoesNotThrow(() =>
                AuditionPvStationTransitionGoldenCapture.ValidateVisualSanity(
                    Samples,
                    Samples / 10,
                    0,
                    598,
                    0));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationTransitionGoldenCapture.ValidateVisualSanity(
                    Samples,
                    Samples * 95 / 100,
                    0,
                    5,
                    0));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationTransitionGoldenCapture.ValidateVisualSanity(
                    Samples,
                    0,
                    Samples / 20,
                    598,
                    1));
        }

        [Test]
        public void FrameHashLedger_CoversEveryCanonicalFrameWithRelativePaths()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_G04LedgerTest_" + Guid.NewGuid().ToString("N"));
            string cleanPlateDirectory = Path.Combine(directory, "clean");
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory(cleanPlateDirectory);
            try
            {
                for (int frame = AuditionPvStationTransitionGoldenCapture.FirstFrame;
                    frame <= AuditionPvStationTransitionGoldenCapture.LastFrame;
                    frame++)
                {
                    File.WriteAllText(
                        Path.Combine(
                            directory,
                            AuditionPvStationTransitionGoldenCapture.FrameFileName(frame)),
                        frame.ToString());
                    File.WriteAllText(
                        Path.Combine(
                            cleanPlateDirectory,
                            AuditionPvStationTransitionGoldenCapture.FrameFileName(frame)),
                        frame.ToString());
                }

                string ledger =
                    AuditionPvStationTransitionGoldenCapture.BuildFrameHashLedger(directory);
                string[] lines = ledger.Split(
                    new[] { '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                Assert.That(lines, Has.Length.EqualTo(598));
                Assert.That(lines[0], Does.EndWith(
                    "  G04_C33_C34_PNG/frame_0000.png"));
                Assert.That(lines[^1], Does.EndWith(
                    "  G04_C33_C34_PNG/frame_0597.png"));

                string combined = AuditionPvStationTransitionGoldenCapture
                    .BuildFrameHashLedger(directory, cleanPlateDirectory);
                string[] combinedLines = combined.Split(
                    new[] { '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                Assert.That(combinedLines, Has.Length.EqualTo(1196));
                Assert.That(combinedLines[598], Does.EndWith(
                    "  frames/g04-clean/frame_0000.png"));
                Assert.That(
                    combinedLines[0].Substring(0, 64),
                    Is.EqualTo(combinedLines[598].Substring(0, 64)));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        private static byte[] BuildPngHeader(int width, int height)
        {
            byte[] bytes = new byte[24];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Buffer.BlockCopy(signature, 0, bytes, 0, signature.Length);
            bytes[8] = 0;
            bytes[9] = 0;
            bytes[10] = 0;
            bytes[11] = 13;
            bytes[12] = (byte)'I';
            bytes[13] = (byte)'H';
            bytes[14] = (byte)'D';
            bytes[15] = (byte)'R';
            WriteBigEndianInt32(bytes, 16, width);
            WriteBigEndianInt32(bytes, 20, height);
            return bytes;
        }

        private static void WriteBigEndianInt32(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private static string ProjectAbsolutePath(string projectRelativePath)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve project root.");
            return Path.GetFullPath(Path.Combine(
                projectRoot,
                projectRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
