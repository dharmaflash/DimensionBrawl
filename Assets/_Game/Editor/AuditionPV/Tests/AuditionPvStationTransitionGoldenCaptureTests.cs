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
            AuditionPvShotManifestEntry shot =
                AuditionPvStationTransitionGoldenCapture.CreateShotManifestEntry();

            Assert.That(shot.id, Is.EqualTo("g04"));
            Assert.That(shot.scenePath, Is.EqualTo(
                "Assets/_Game/Scenes/OlympusStationCombatStage.unity"));
            Assert.That(shot.startFrame, Is.EqualTo(0));
            Assert.That(shot.endFrame, Is.EqualTo(237));
            Assert.That(shot.expectedFrameCount, Is.EqualTo(238));
            Assert.That(shot.hudMode, Is.EqualTo("hud-off"));
            Assert.That(AuditionPvCaptureContract.Width, Is.EqualTo(2560));
            Assert.That(AuditionPvCaptureContract.Height, Is.EqualTo(1440));
            Assert.That(AuditionPvCaptureContract.Fps, Is.EqualTo(60));
        }

        [TestCase(0, true)]
        [TestCase(95, true)]
        [TestCase(96, false)]
        [TestCase(237, false)]
        public void CameraRouting_CutsExactlyFromC33ToC34(int frameIndex, bool usesWing)
        {
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.UsesWingCamera(frameIndex),
                Is.EqualTo(usesWing));
        }

        [Test]
        public void FrameNames_AreContiguousAndRangeChecked()
        {
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.FrameFileName(0),
                Is.EqualTo("frame_0000.png"));
            Assert.That(
                AuditionPvStationTransitionGoldenCapture.FrameFileName(237),
                Is.EqualTo("frame_0237.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationTransitionGoldenCapture.FrameFileName(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationTransitionGoldenCapture.FrameFileName(238));
        }

        [Test]
        public void Baselines_AreExactFramesNamesAndHudOff()
        {
            AuditionPvBaselineManifestEntry[] baselines =
                AuditionPvStationTransitionGoldenCapture.CreateBaselineManifestEntries();

            Assert.That(baselines, Has.Length.EqualTo(2));
            Assert.That(baselines[0].id, Is.EqualTo("bl04"));
            Assert.That(baselines[0].shotId, Is.EqualTo("g04"));
            Assert.That(baselines[0].sourceFrame, Is.EqualTo(66));
            Assert.That(baselines[0].fileName, Is.EqualTo(
                "BL04_AKAZA_C33_WING_OPEN__HUDOFF__t01.100000.png"));
            Assert.That(baselines[0].hudMode, Is.EqualTo("hud-off"));
            Assert.That(baselines[0].status, Is.EqualTo("captured"));

            Assert.That(baselines[1].id, Is.EqualTo("bl05"));
            Assert.That(baselines[1].shotId, Is.EqualTo("g04"));
            Assert.That(baselines[1].sourceFrame, Is.EqualTo(178));
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
            const int Samples = 238 * 576;
            Assert.DoesNotThrow(() =>
                AuditionPvStationTransitionGoldenCapture.ValidateVisualSanity(
                    Samples,
                    Samples / 10,
                    0,
                    238,
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
                    238,
                    1));
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
    }
}
