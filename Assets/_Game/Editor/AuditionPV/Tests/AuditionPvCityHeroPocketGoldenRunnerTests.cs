using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using DimensionBrawl.Presentation;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvCityHeroPocketGoldenRunnerTests
    {
        [Test]
        public void RecorderMapping_PreservesRawZeroAndMapsExactThreeShots()
        {
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.ShotOrder,
                Is.EqualTo(new[]
                {
                    AuditionPvCityShot.G01,
                    AuditionPvCityShot.G02,
                    AuditionPvCityShot.G03
                }));

            AssertShotMapping(AuditionPvCityShot.G01, 240, 600, 601);
            AssertShotMapping(AuditionPvCityShot.G02, 420, 780, 781);
            AssertShotMapping(AuditionPvCityShot.G03, 300, 660, 661);
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.G02DodgeVisualBeforeFrame,
                Is.EqualTo(AuditionPvCityHeroPocketCapture.G02DodgeDownFrame - 1));
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.G02DodgeVisualAfterFrame,
                Is.EqualTo(AuditionPvCityHeroPocketCapture.G02DodgeUpFrame + 1));
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.G02DodgeVisualAfterFrame,
                Is.EqualTo(AuditionPvCityHeroPocketCapture.G02SecondMoveUpFrame));
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.RawFrameFileName(
                    AuditionPvCityShot.G01,
                    0),
                Is.EqualTo("frame_0000.png"));
            Assert.That(
                    AuditionPvCityHeroPocketGoldenRunner.RawFrameFileName(
                        AuditionPvCityShot.G02,
                    780),
                Is.EqualTo("frame_0780.png"));
            Assert.That(
                    AuditionPvCityHeroPocketGoldenRunner.RawFrameFileName(
                        AuditionPvCityShot.G03,
                    660),
                Is.EqualTo("frame_0660.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.RawFrameFileName(
                    AuditionPvCityShot.G01,
                    -1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                    AuditionPvCityHeroPocketGoldenRunner.RawFrameFileName(
                        AuditionPvCityShot.G01,
                    601));
        }

        [Test]
        public void BatchArguments_RequireGuiExecuteMethodAndNoAudio()
        {
            string[] valid =
            {
                "Unity.exe",
                "-noaudio",
                "-executeMethod",
                "DimensionBrawl.Editor.AuditionPV."
                + "AuditionPvCityHeroPocketGoldenRunner.RunBatchCapture"
            };
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateBatchCommandLine(valid));

            foreach (string forbidden in new[]
                     {
                         "-batchmode",
                         "-quit",
                         "-nographics"
                     })
            {
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidateBatchCommandLine(
                        valid.Append(forbidden)));
            }

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateBatchCommandLine(
                    new[] { "Unity.exe", "-executeMethod", "Runner" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateBatchCommandLine(
                    new[] { "Unity.exe", "-noaudio" }));
        }

        [Test]
        public void RunnerSource_UsesPublicRecorderCoreAndSessionLifecycleOnly()
        {
            string source = ReadProjectFile(
                AuditionPvCityHeroPocketGoldenRunner.RunnerScriptPath);

            Assert.That(source, Does.Contain("[MenuItem(MenuPath)]"));
            Assert.That(source, Does.Contain("public static void RunBatchCapture()"));
            Assert.That(source, Does.Contain("SessionState.SetString"));
            Assert.That(source, Does.Contain("EditorApplication.playModeStateChanged"));
            Assert.That(source, Does.Contain("SetRecordModeToFrameInterval("));
            Assert.That(source, Does.Contain("recorderController.PrepareRecording();"));
            Assert.That(source, Does.Contain("recorderController.StartRecording()"));
            Assert.That(source, Does.Contain("new WaitForEndOfFrame()"));
            Assert.That(source, Does.Contain("director.BeginShotForRecorder("));
            Assert.That(source, Does.Contain(
                "activeProof.recorderWarmupEndOfFrameCount"));
            Assert.That(source, Does.Contain(
                "activeProof.recordedPreHandleFrameCount++"));
            Assert.That(source, Does.Contain(
                "activeProof.recordedPostHandleFrameCount"));
            Assert.That(source, Does.Contain(
                "while (recorderController.IsRecording()"));
            Assert.That(source, Does.Contain(
                "yield return EndOfFrameYield;"));
            Assert.That(source, Does.Contain(
                "AcquireRecordedPostHandleFreeze();"));
            Assert.That(source, Does.Contain(
                "recordedPostHandleTimeFreeze.Acquire();"));
            Assert.That(source, Does.Match(
                @"ReleaseRecordedPostHandleFreeze\(\);\s*"
                + @"MarkNoDirectorCleanupBeforeContinuation\(previous\);\s*"
                + @"IEnumerator continuation = director\.PrepareContinuationShot\(shot\);"));
            Assert.That(source, Does.Contain(
                "CaptureFailure(ref firstFailure, ReleaseRecordedPostHandleFreeze);"));
            Assert.That(source, Does.Contain(
                "WriteGateEvidenceArtifacts("));
            Assert.That(source, Does.Contain(
                "shot-authorship/"));
            Assert.That(source, Does.Contain(
                "semantic-beat/"));
            int warmupArmIndex = source.IndexOf(
                "director.ArmG02RecorderWarmupSuspension();",
                StringComparison.Ordinal);
            int prepareRecordingIndex = source.IndexOf(
                "recorderController.PrepareRecording();",
                StringComparison.Ordinal);
            Assert.That(warmupArmIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(
                warmupArmIndex,
                Is.LessThan(prepareRecordingIndex),
                "G02 must acquire its product-state barrier before Recorder warmup frames.");
            Assert.That(source, Does.Contain("director.RestoreShotState();"));
            Assert.That(
                source,
                Does.Match(
                    @"AttachToFreshActiveScene\s*\(\s*AuditionPvCityShot\.G01\s*\)"));
            Assert.That(source, Does.Contain("PrepareContinuationShot(shot)"));
            Assert.That(source, Does.Contain("director?.LastSealedRuntimeProof"));
            Assert.That(source, Does.Contain("director.SnapshotRuntimeProof()"));
            Assert.That(source, Does.Contain("catch (Exception continuationFailure)"));
            Assert.That(source, Does.Contain(
                "AppendValidatedSealedRuntimeProof("));
            Assert.That(
                CountOccurrences(source, "CaptureSealedRuntimeProof(previous);"),
                Is.EqualTo(2),
                "Both successful and failed continuation paths must preserve "
                + "the prior sealed proof.");
            Assert.That(source, Does.Not.Contain("AttachToFreshActiveScene(shot)"));
            Assert.That(
                CountOccurrences(source, "AttachToFreshActiveScene("),
                Is.EqualTo(1),
                "The City product scene/director must be attached exactly once.");
            Assert.That(
                CountOccurrences(source, "director.RestoreShotState();"),
                Is.EqualTo(1),
                "Only the post-G03 finalizer may restore the shared director.");
            Assert.That(
                CountOccurrences(source, "directorRestoreCallCount++;"),
                Is.EqualTo(1),
                "The actual final restore invocation must increment its counter.");
            Assert.That(
                CountOccurrences(
                    source,
                    "UnityEngine.Object.Destroy(director.gameObject);"),
                Is.EqualTo(1),
                "Only the post-G03 finalizer may destroy the shared director.");
            Assert.That(
                CountOccurrences(source, "directorDestroyCallCount++;"),
                Is.EqualTo(1),
                "The actual final destroy invocation must increment its counter.");
            Assert.That(source, Does.Match(
                @"recorder\.directorRestoreCallCountBeforeNextShot\s*=\s*"
                + @"directorRestoreCallCount\s*;"));
            Assert.That(source, Does.Match(
                @"recorder\.directorDestroyCallCountBeforeNextShot\s*=\s*"
                + @"directorDestroyCallCount\s*;"));
            Assert.That(source, Does.Not.Contain(
                "recorder.directorRestoreCallCountBeforeNextShot = 0;"));
            Assert.That(source, Does.Not.Contain(
                "recorder.directorDestroyCallCountBeforeNextShot = 0;"));
            Assert.That(source, Does.Not.Contain("CleanupActiveShot"));
            Assert.That(
                source,
                Does.Contain(
                    "AuditionPvCityHeroPocketCapture.ValidateRuntimeProof"));
            Assert.That(source, Does.Contain("ReopenProductSceneAfterPlayMode"));
            Assert.That(source, Does.Contain(".city-")
                .And.Contain("-remap-"));
            Assert.That(source, Does.Contain("AuditionPvCaptureManifestWriter.WriteNew"));
            Assert.That(source, Does.Contain(
                "frame_hashes.sha256 artifact SHA-256="));
            Assert.That(source, Does.Contain("frameHashArtifactSha256"));
            Assert.That(source, Does.Not.Contain("System.Reflection"));
            Assert.That(source, Does.Not.Contain("BindingFlags"));
            Assert.That(source, Does.Not.Contain("EditorSceneManager.Save"));
            Assert.That(source, Does.Not.Contain("SetPositionAndRotation("));
            Assert.That(source, Does.Not.Contain("CharacterController.Move("));
            Assert.That(source, Does.Not.Contain("StartTransition("));
            Assert.That(source, Does.Not.Contain("BeginTransition("));
            Assert.That(source, Does.Not.Contain("TriggerTransition("));
            Assert.That(source, Does.Not.Contain("ApplyDamage("));
            Assert.That(source, Does.Not.Contain("TakeDamage("));
            Assert.That(source, Does.Not.Contain("transform.position ="));
            Assert.That(source, Does.Not.Contain("GetRawTextureData<byte>().ToArray()"));
            Assert.That(source, Does.Not.Contain("native.ToArray()"));

            int preflight = source.IndexOf(
                "ValidateAuthoredPreflight(",
                StringComparison.Ordinal);
            int reserve = source.IndexOf(
                "ReserveNewOutput(",
                StringComparison.Ordinal);
            Assert.That(preflight, Is.GreaterThanOrEqualTo(0));
            Assert.That(reserve, Is.GreaterThan(preflight));
        }

        [Test]
        public void RecordedPostHandleTimeFreeze_RestoresExactScaleAndReleasesIdempotently()
        {
            float originalTimeScale = Time.timeScale;
            var freeze = new AuditionPvRecordedPostHandleTimeFreeze();
            try
            {
                Time.timeScale = 0.37f;
                freeze.Acquire();

                Assert.That(freeze.IsOwned, Is.True);
                Assert.That(Time.timeScale, Is.Zero.Within(0.0001f));
                Assert.DoesNotThrow(freeze.AssertHeld);
                Assert.DoesNotThrow(freeze.Acquire);

                freeze.Release();

                Assert.That(freeze.IsOwned, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(0.37f).Within(0.0001f));
                Assert.DoesNotThrow(freeze.Release);
            }
            finally
            {
                freeze.Release();
                Time.timeScale = originalTimeScale;
            }
        }

        [Test]
        public void RecordedPostHandleTimeFreeze_LostOwnershipFailsAfterRestoringScale()
        {
            float originalTimeScale = Time.timeScale;
            var freeze = new AuditionPvRecordedPostHandleTimeFreeze();
            try
            {
                Time.timeScale = 0.43f;
                freeze.Acquire();
                Time.timeScale = 0.81f;

                Assert.Throws<InvalidOperationException>(freeze.AssertHeld);
                Assert.Throws<InvalidOperationException>(freeze.Release);
                Assert.That(freeze.IsOwned, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(0.43f).Within(0.0001f));
                Assert.DoesNotThrow(freeze.Release);
            }
            finally
            {
                freeze.Release();
                Time.timeScale = originalTimeScale;
            }
        }

        [Test]
        public void AuthoredPreflight_FailsBeforeAnyOutputReservationCanRun()
        {
            int validatorCalls = 0;
            int reservationCalls = 0;
            Assert.Throws<InvalidOperationException>(() =>
            {
                AuditionPvCityHeroPocketGoldenRunner.ValidateAuthoredPreflight(() =>
                {
                    validatorCalls++;
                    throw new InvalidOperationException("authored pack invalid");
                });
                reservationCalls++;
            });
            Assert.That(validatorCalls, Is.EqualTo(1));
            Assert.That(reservationCalls, Is.EqualTo(0));
        }

        [Test]
        public void ExecutionOrder_ArmsBeforeInputAndComposesAfterActionCamera()
        {
            int runner = ExecutionOrderOf<
                AuditionPvCityHeroPocketGoldenRunnerBehaviour>();
            int input = ExecutionOrderOf<AuditionPvCityHeroPocketDirector>();
            int actionCamera = ExecutionOrderOf<ActionCameraController>();
            int rail = ExecutionOrderOf<AuditionPvCityHeroPocketCameraRail>();

            Assert.That(runner, Is.EqualTo(-32600));
            Assert.That(input, Is.EqualTo(-32500));
            Assert.That(actionCamera, Is.EqualTo(200));
            Assert.That(rail, Is.EqualTo(32500));
            Assert.That(runner, Is.LessThan(input));
            Assert.That(input, Is.LessThan(actionCamera));
            Assert.That(actionCamera, Is.LessThan(rail));
        }

        [Test]
        public void Dependencies_IncludeRunnerTestsCityCoreTransitionAndProductGraph()
        {
            string[] dependencies =
                AuditionPvCityHeroPocketGoldenRunner.CollectCaptureDependencyPaths();
            foreach (string required in new[]
                     {
                         AuditionPvCityHeroPocketGoldenRunner.RunnerScriptPath,
                         AuditionPvCityHeroPocketGoldenRunner.RunnerTestPath,
                         AuditionPvCityHeroPocketCapture.CityScenePath,
                         AuditionPvCityHeroPocketCapture.CaptureScriptPath,
                         AuditionPvCityHeroPocketCapture.CaptureTestPath,
                         AuditionPvCityHeroPocketCapture.ExitTransitionPath
                     })
            {
                Assert.That(dependencies, Does.Contain(required), required);
                Assert.That(
                    AssetDatabase.LoadMainAssetAtPath(required),
                    Is.Not.Null,
                    required);
            }

            string[] explicitProduct =
                AuditionPvCityHeroPocketCapture.ExplicitProductDependencyPaths();
            foreach (string requiredSeed in new[]
                     {
                         AuditionPvCityHeroPocketCapture.CityScenePath,
                         AuditionPvCityHeroPocketCapture.CaptureScriptPath,
                         AuditionPvCityHeroPocketCapture.CaptureTestPath,
                         AuditionPvCityHeroPocketCapture.ExitTransitionPath,
                         AuditionPvCityHeroPocketCapture.PresentationClockPath,
                         AuditionPvCityHeroPocketCapture.ActionCameraPath,
                         AuditionPvCityHeroPocketCapture.RangedActionPath,
                         AuditionPvCityHeroPocketCapture.EncounterPath,
                         AuditionPvCityHeroPocketCapture.HudPointerPath,
                         AuditionPvCityHeroPocketCapture.HudJoystickPath
                     })
            {
                Assert.That(explicitProduct, Does.Contain(requiredSeed), requiredSeed);
                Assert.That(dependencies, Does.Contain(requiredSeed), requiredSeed);
            }
        }

        [Test]
        public void RawRemap_AllShotsPreserveWarmupAndNeverOverwrite()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_CityGoldenRemap_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                foreach (AuditionPvCityShot shot in
                         AuditionPvCityHeroPocketGoldenRunner.ShotOrder)
                {
                    string frames = Path.Combine(
                        root,
                        "frames",
                        AuditionPvCityHeroPocketGoldenRunner.ShotId(shot));
                    string evidence = Path.Combine(root, "evidence");
                    Directory.CreateDirectory(frames);
                    int count =
                        AuditionPvCityHeroPocketGoldenRunner.ExpectedRawFrameCount(shot);
                    for (int raw = 0; raw < count; raw++)
                    {
                        File.WriteAllBytes(
                            Path.Combine(
                                frames,
                                AuditionPvCityHeroPocketGoldenRunner.RawFrameFileName(
                                    shot,
                                    raw)),
                            BitConverter.GetBytes(raw));
                    }

                    string warmup =
                        AuditionPvCityHeroPocketGoldenRunner.RemapRawFrames(
                            shot,
                            frames,
                            evidence);
                    Assert.That(BitConverter.ToInt32(File.ReadAllBytes(warmup), 0),
                        Is.EqualTo(0));
                    AuditionPvCityHeroPocketGoldenRunner
                        .ValidateSourceFrameSequence(shot, frames);
                    Assert.That(
                        BitConverter.ToInt32(
                            File.ReadAllBytes(Path.Combine(frames, "frame_0000.png")),
                            0),
                        Is.EqualTo(1));
                    string last = AuditionPvCityHeroPocketCapture.SourceFrameFileName(
                        shot,
                        AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot));
                    Assert.That(
                        BitConverter.ToInt32(
                            File.ReadAllBytes(Path.Combine(frames, last)),
                            0),
                        Is.EqualTo(count - 1));
                    Assert.That(
                        Directory.GetDirectories(
                            Path.Combine(root, "frames"),
                            ".city-*-remap-*").Length,
                        Is.EqualTo(0));
                }
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void RawSequence_RejectsMissingAndExtraFrames()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_CityGoldenRaw_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                AuditionPvCityShot shot = AuditionPvCityShot.G01;
                int expected =
                    AuditionPvCityHeroPocketGoldenRunner.ExpectedRawFrameCount(shot);
                for (int raw = 0; raw < expected - 1; raw++)
                {
                    File.WriteAllBytes(
                        Path.Combine(
                            root,
                            AuditionPvCityHeroPocketGoldenRunner.RawFrameFileName(
                                shot,
                                raw)),
                        new byte[] { 1 });
                }

                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidateRawFrameSequence(
                        shot,
                        root));
                File.WriteAllBytes(
                    Path.Combine(root, "frame_9999.png"),
                    new byte[] { 1 });
                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidateRawFrameSequence(
                        shot,
                        root));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void PngValidator_RequiresSignatureAndExactQhdDimensions()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_CityGoldenPng_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string path = Path.Combine(root, "frame.png");
            try
            {
                File.WriteAllBytes(path, BuildPngHeader(2560, 1440));
                Assert.DoesNotThrow(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidatePngFile(
                        path,
                        2560,
                        1440));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidatePngFile(
                        path,
                        1920,
                        1080));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void PixelAndHashEvidence_FailClosedOnFlatMagentaOrDuplicateData()
        {
            Color32[] healthy = Enumerable.Repeat(
                new Color32(50, 120, 180, 255),
                64 * 64).ToArray();
            AuditionPvCityHeroPocketGoldenRunner.SequenceVisualMetrics measured =
                AuditionPvCityHeroPocketGoldenRunner.EvaluatePixels(
                    healthy,
                    64,
                    64,
                    measureHud: true);
            Assert.That(measured.sampleCount, Is.GreaterThan(0));
            Assert.That(measured.magentaRatio, Is.EqualTo(0d));

            Color32[] magenta = Enumerable.Repeat(
                new Color32(255, 0, 255, 255),
                64 * 64).ToArray();
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.EvaluatePixels(
                    magenta,
                    64,
                    64,
                    measureHud: false).magentaRatio,
                Is.EqualTo(1d));

            string hash = new string('a', 64);
            var entries = new[]
            {
                new AuditionPvCityHeroPocketGoldenRunner.FrameHashEntry
                {
                    id = "g01/f0000",
                    relativePath = "frames/g01/frame_0000.png",
                    byteLength = 25,
                    sha256 = hash
                }
            };
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateHashEntries(entries));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateHashEntries(
                    entries.Concat(entries)));
        }

        [Test]
        public void HudCyanProbe_CountsOnlyTheReviewedEnergyBarRoi()
        {
            const int Width = 512;
            const int Height = 512;
            Color32 gray = new(60, 70, 80, 255);
            Color32 cyan = new(40, 190, 230, 255);
            Color32[] pixels = Enumerable.Repeat(gray, Width * Height).ToArray();

            // City/portal cyan outside the reviewed energy-bar ROI is not HUD proof.
            pixels[16 * Width + 16] = cyan;
            pixels[240 * Width + 400] = cyan;
            // A matching cyan inside the horizontal band but above the reviewed
            // bottom HUD region must not count.
            pixels[80 * Width + 144] = cyan;
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.EvaluatePixels(
                    pixels,
                    Width,
                    Height,
                    measureHud: true).frameZeroHudAccentSamples,
                Is.EqualTo(0));

            for (int topY = 432; topY <= 464; topY += 32)
            {
                for (int x = 144; x <= 240; x += 32)
                {
                    pixels[topY * Width + x] = cyan;
                }
            }

            AuditionPvCityHeroPocketGoldenRunner.SequenceVisualMetrics metrics =
                AuditionPvCityHeroPocketGoldenRunner.EvaluatePixels(
                    pixels,
                    Width,
                    Height,
                    measureHud: true);
            Assert.That(metrics.frameZeroHudAccentSamples, Is.EqualTo(8));

            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.EvaluatePixels(
                    pixels,
                    Width,
                    Height,
                    measureHud: false).frameZeroHudAccentSamples,
                Is.EqualTo(0));
        }

        [Test]
        public void HudCyanProbe_QhdPresentationGridAvoidsRawRowPhaseDrift()
        {
            const int Width = 2560;
            const int Height = 1440;
            Color32 gray = new(60, 70, 80, 255);
            Color32 cyan = new(40, 190, 230, 255);
            Color32[] pixels = Enumerable.Repeat(gray, Width * Height).ToArray();

            // The reviewed QHD grid samples topY=1264. A one-pixel phase drift
            // must not be accepted merely because the cyan band is nearby.
            for (int x = 720; x <= 1296; x += 32)
            {
                pixels[1263 * Width + x] = cyan;
            }
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.EvaluatePixels(
                    pixels,
                    Width,
                    Height,
                    measureHud: true).frameZeroHudAccentSamples,
                Is.EqualTo(0));

            for (int x = 720; x <= 1296; x += 32)
            {
                pixels[1264 * Width + x] = cyan;
            }
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.EvaluatePixels(
                    pixels,
                    Width,
                    Height,
                    measureHud: true).frameZeroHudAccentSamples,
                Is.EqualTo(19));
        }

        [Test]
        public void HudCyanProbe_LoadedPngUsesSemanticColorAndOrientation()
        {
            const int Width = 512;
            const int Height = 512;
            Color32 gray = new(60, 70, 80, 255);
            Color32 cyan = new(40, 190, 230, 255);
            var source = new Texture2D(
                Width,
                Height,
                TextureFormat.RGBA32,
                false,
                true);
            var loaded = new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                false,
                true);
            try
            {
                source.SetPixels32(
                    Enumerable.Repeat(gray, Width * Height).ToArray());
                for (int topY = 432; topY <= 464; topY += 32)
                {
                    for (int x = 144; x <= 240; x += 32)
                    {
                        source.SetPixel(x, Height - 1 - topY, cyan);
                    }
                }
                source.Apply(false, false);

                byte[] png = source.EncodeToPNG();
                Assert.That(
                    ImageConversion.LoadImage(loaded, png, markNonReadable: false),
                    Is.True);
                Assert.That(
                    AuditionPvCityHeroPocketGoldenRunner.EvaluateTexturePixels(
                        loaded,
                        measureHud: true).frameZeroHudAccentSamples,
                    Is.EqualTo(8));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(loaded);
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void VisualAcceptance_IsShotSpecificAndFailsEveryBoundary()
        {
            foreach (AuditionPvCityShot shot in
                     AuditionPvCityHeroPocketGoldenRunner.ShotOrder)
            {
                Assert.DoesNotThrow(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                        shot,
                        PassingVisualMetrics(shot)));
            }

            var g01Hud = PassingVisualMetrics(AuditionPvCityShot.G01);
            g01Hud.hudProbes[0].cyanAccentSamples = 1;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G01,
                    g01Hud));
            var g01Delta = PassingVisualMetrics(AuditionPvCityShot.G01);
            g01Delta.primaryDelta.meanAbsoluteRgb = 1.49d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G01,
                    g01Delta));
            g01Delta = PassingVisualMetrics(AuditionPvCityShot.G01);
            g01Delta.primaryDelta.changedSampleRatio = 0.049d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G01,
                    g01Delta));

            var g02Hud = PassingVisualMetrics(AuditionPvCityShot.G02);
            g02Hud.hudProbes[2].cyanAccentSamples = 11;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G02,
                    g02Hud));
            var g02Dodge = PassingVisualMetrics(AuditionPvCityShot.G02);
            g02Dodge.dodgeDelta.changedSampleRatio = 0.049d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G02,
                    g02Dodge));
            var g02End = PassingVisualMetrics(AuditionPvCityShot.G02);
            g02End.primaryDelta.meanAbsoluteRgb = 2.99d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G02,
                    g02End));

            var g03Cover = PassingVisualMetrics(AuditionPvCityShot.G03);
            g03Cover.minimumFullCoverSampledLuma = 219;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G03,
                    g03Cover));
            g03Cover = PassingVisualMetrics(AuditionPvCityShot.G03);
            g03Cover.fullCoverFrameCount = 23;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G03,
                    g03Cover));
            g03Cover = PassingVisualMetrics(AuditionPvCityShot.G03);
            g03Cover.maximumFullCoverSpatialChannelRange = 5;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G03,
                    g03Cover));
            var g03Delta = PassingVisualMetrics(AuditionPvCityShot.G03);
            g03Delta.primaryDelta.meanAbsoluteRgb = 59.99d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G03,
                    g03Delta));
            g03Delta = PassingVisualMetrics(AuditionPvCityShot.G03);
            g03Delta.primaryDelta.changedSampleRatio = 0.699d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G03,
                    g03Delta));
            g03Cover = PassingVisualMetrics(AuditionPvCityShot.G03);
            g03Cover.fullCoverDecodedPixelHashesExact = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G03,
                    g03Cover));

            var globalBlack = PassingVisualMetrics(AuditionPvCityShot.G01);
            globalBlack.blackRatio = 0.90d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G01,
                    globalBlack));
            var unhealthy = PassingVisualMetrics(AuditionPvCityShot.G01);
            unhealthy.healthyFrameCount = 215;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G01,
                    unhealthy));
            var magentaFrame = PassingVisualMetrics(AuditionPvCityShot.G01);
            magentaFrame.maximumFrameMagentaRatio = 0.02d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G01,
                    magentaFrame));
            var magentaGlobal = PassingVisualMetrics(AuditionPvCityShot.G01);
            magentaGlobal.magentaRatio = 0.005d;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateVisualSequence(
                    AuditionPvCityShot.G01,
                    magentaGlobal));
        }

        [Test]
        public void CanonicalHashLedger_IsSortedCreateNewAndReparsedExactly()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_CityGoldenHash_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string path = Path.Combine(root, "frame_hashes.sha256");
            var entries = new[]
            {
                HashEntry("g02/f0000", "frames/g02/frame_0000.png", "b"),
                HashEntry("g01/f0000", "frames/g01/frame_0000.png", "a")
            };
            try
            {
                AuditionPvCityHeroPocketGoldenRunner.WriteCanonicalHashLedgerNew(
                    path,
                    entries);
                Assert.DoesNotThrow(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidateCanonicalHashLedger(
                        path,
                        entries));
                Assert.That(
                    File.ReadLines(path).First(),
                    Does.EndWith("  frames/g01/frame_0000.png"));
                Assert.Throws<IOException>(() =>
                    AuditionPvCityHeroPocketGoldenRunner.WriteCanonicalHashLedgerNew(
                        path,
                        entries));
                File.WriteAllText(path, "corrupt\n");
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvCityHeroPocketGoldenRunner.ValidateCanonicalHashLedger(
                        path,
                        entries));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void DecodedPixelHash_RequiresAllTwentyFourCoverFramesToMatch()
        {
            byte[] decoded = Enumerable.Range(0, 4097)
                .Select(index => (byte)(index * 31))
                .ToArray();
            string[] identical = Enumerable.Range(0, 24)
                .Select(_ =>
                    AuditionPvCityHeroPocketGoldenRunner.HashDecodedPixelBytes(
                        decoded,
                        chunkSize: 127))
                .ToArray();
            Assert.That(identical.Length, Is.EqualTo(24));
            Assert.That(
                identical.Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(1));

            byte[] changed = (byte[])decoded.Clone();
            changed[^1] ^= 0x01;
            string changedHash =
                AuditionPvCityHeroPocketGoldenRunner.HashDecodedPixelBytes(
                    changed,
                    chunkSize: 127);
            Assert.That(changedHash, Is.Not.EqualTo(identical[0]));
            Assert.That(
                identical.Take(23).Append(changedHash)
                    .Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(2));
        }

        [Test]
        public void RecorderProof_RequiresExactCountsClockAutoStopAndRestore()
        {
            var proof = PassingRecorderProof(AuditionPvCityShot.G02);
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(proof));

            proof.presentedFrameCount--;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(proof));
            proof = PassingRecorderProof(AuditionPvCityShot.G02);
            proof.directorStateRestored = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(proof));
            proof = PassingRecorderProof(AuditionPvCityShot.G02);
            proof.recordedPreHandleFrameCount--;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(proof));
            proof = PassingRecorderProof(AuditionPvCityShot.G02);
            proof.recordedPostHandleFrameCount--;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(proof));
            proof = PassingRecorderProof(AuditionPvCityShot.G02);
            proof.logicalFirstSourceFrame++;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateRecorderProof(proof));
        }

        [Test]
        public void DirectorLifecycleProof_ObservesZeroBeforeContinuationAndOneAtEnd()
        {
            var recorder = AuditionPvCityHeroPocketGoldenRunner.ShotOrder
                .Select(PassingRecorderProof)
                .ToArray();
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .ValidateFinalDirectorLifecycle(recorder));

            recorder[1].directorRestoreCallCountBeforeNextShot = 1;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .ValidateFinalDirectorLifecycle(recorder));

            recorder = AuditionPvCityHeroPocketGoldenRunner.ShotOrder
                .Select(PassingRecorderProof)
                .ToArray();
            recorder[2].directorRestoreCallCountAtSequenceEnd = 0;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .ValidateFinalDirectorLifecycle(recorder));

            recorder = AuditionPvCityHeroPocketGoldenRunner.ShotOrder
                .Select(PassingRecorderProof)
                .ToArray();
            recorder[0].directorDestroyCallCountAtSequenceEnd = 2;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .ValidateFinalDirectorLifecycle(recorder));
        }

        [Test]
        public void FailureSealedProofAppender_PreservesExactG01G02OrderOnly()
        {
            var proofs = new List<AuditionPvCityHeroPocketRuntimeProof>();
            var g01 = new AuditionPvCityHeroPocketRuntimeProof
            {
                shotId = AuditionPvCityHeroPocketCapture.G01ShotId
            };
            var g02 = new AuditionPvCityHeroPocketRuntimeProof
            {
                shotId = AuditionPvCityHeroPocketCapture.G02ShotId
            };
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .AppendValidatedSealedRuntimeProof(
                        proofs,
                        g01,
                        AuditionPvCityShot.G01,
                        _ => { }));
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .AppendValidatedSealedRuntimeProof(
                        proofs,
                        g02,
                        AuditionPvCityShot.G02,
                        _ => { }));
            Assert.That(
                proofs.Select(value => value.shotId),
                Is.EqualTo(new[]
                {
                    AuditionPvCityHeroPocketCapture.G01ShotId,
                    AuditionPvCityHeroPocketCapture.G02ShotId
                }));

            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .AppendValidatedSealedRuntimeProof(
                        proofs,
                        g02,
                        AuditionPvCityShot.G02,
                        _ => { }));
            Assert.That(proofs.Count, Is.EqualTo(2));

            var invalidId = new List<AuditionPvCityHeroPocketRuntimeProof>
            {
                g01
            };
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .AppendValidatedSealedRuntimeProof(
                        invalidId,
                        new AuditionPvCityHeroPocketRuntimeProof
                        {
                            shotId = AuditionPvCityHeroPocketCapture.G03ShotId
                        },
                        AuditionPvCityShot.G02,
                        _ => { }));
            Assert.That(invalidId.Count, Is.EqualTo(1));

            var invalidProof = new List<AuditionPvCityHeroPocketRuntimeProof>
            {
                g01
            };
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .AppendValidatedSealedRuntimeProof(
                        invalidProof,
                        g02,
                        AuditionPvCityShot.G02,
                        _ => throw new InvalidOperationException("invalid proof")));
            Assert.That(invalidProof.Count, Is.EqualTo(1));

            var wrongHistory = new List<AuditionPvCityHeroPocketRuntimeProof>
            {
                g02
            };
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner
                    .AppendValidatedSealedRuntimeProof(
                        wrongHistory,
                        g02,
                        AuditionPvCityShot.G02,
                        _ => { }));
            Assert.That(wrongHistory.Count, Is.EqualTo(1));
        }

        [Test]
        public void G02G03Continuity_RequiresSameObjectsWonAndZeroReplayOrCleanup()
        {
            var recorder = PassingRecorderProof(AuditionPvCityShot.G02);
            AuditionPvCityHeroPocketRuntimeProof g02 = PassingG02Continuity();
            AuditionPvCityHeroPocketRuntimeProof g03 = PassingG03Continuity();
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateG02G03Continuity(
                    recorder,
                    g02,
                    g03));

            recorder.directorRestoreCallCountBeforeNextShot = 1;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateG02G03Continuity(
                    recorder,
                    g02,
                    g03));
            recorder = PassingRecorderProof(AuditionPvCityShot.G02);
            g03 = PassingG03Continuity();
            g03.enemyInstanceId++;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateG02G03Continuity(
                    recorder,
                    g02,
                    g03));
            g03 = PassingG03Continuity();
            g03.g03NewDamageEventCount = 1;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateG02G03Continuity(
                    recorder,
                    g02,
                    g03));
            g03 = PassingG03Continuity();
            g03.captureTransitionStartCallCount = 1;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateG02G03Continuity(
                    recorder,
                    g02,
                    g03));
            g03 = PassingG03Continuity();
            g03.g03StartedAlreadyWon = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateG02G03Continuity(
                    recorder,
                    g02,
                    g03));
        }

        [Test]
        public void SourceStability_RejectsGitAndDependencyDrift()
        {
            AuditionPvGitSnapshot original = Git("abc1234", "main", true, "dirty");
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateStableGitSnapshot(
                    original,
                    Git("abc1234", "main", true, "dirty")));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateStableGitSnapshot(
                    original,
                    Git("def5678", "main", true, "dirty")));

            AuditionPvDependencyHash[] dependencies =
            {
                Dependency("Assets/City.unity", 12, "aaa"),
                Dependency("Assets/Core.cs", 34, "bbb")
            };
            Assert.DoesNotThrow(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateStableDependencies(
                    dependencies,
                    dependencies.Reverse().ToArray()));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateStableDependencies(
                    dependencies,
                    new[]
                    {
                        Dependency("Assets/City.unity", 12, "changed"),
                        Dependency("Assets/Core.cs", 34, "bbb")
                    }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvCityHeroPocketGoldenRunner.ValidateStableDependencies(
                    dependencies,
                    new[]
                    {
                        Dependency("Assets/City.unity", 12, "aaa")
                    }));
        }

        private static void AssertShotMapping(
            AuditionPvCityShot shot,
            int logicalCount,
            int sourceCount,
            int rawCount)
        {
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetExpectedFrameCount(shot),
                Is.EqualTo(logicalCount));
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot),
                Is.EqualTo(sourceCount));
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetSelectStartFrame(shot),
                Is.EqualTo(180));
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetSelectEndFrame(shot),
                Is.EqualTo(180 + logicalCount - 1));
            Assert.That(
                AuditionPvCityHeroPocketCapture.GetSourceLastFrame(shot),
                Is.EqualTo(sourceCount - 1));
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.ExpectedRawFrameCount(shot),
                Is.EqualTo(rawCount));
            Assert.That(
                AuditionPvCityHeroPocketGoldenRunner.RawLastFrame(shot),
                Is.EqualTo(rawCount - 1));
        }

        private static AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof
            PassingRecorderProof(AuditionPvCityShot shot)
        {
            return new AuditionPvCityHeroPocketGoldenRunner.ShotRecorderProof
            {
                shotId = AuditionPvCityHeroPocketGoldenRunner.ShotId(shot),
                expectedRawFrameCount =
                    AuditionPvCityHeroPocketGoldenRunner.ExpectedRawFrameCount(shot),
                recorderWarmupEndOfFrameCount = 2,
                canonicalSourceFrameCount =
                    AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot),
                logicalFirstSourceFrame =
                    AuditionPvCityHeroPocketCapture.GetSelectStartFrame(shot),
                logicalLastSourceFrame =
                    AuditionPvCityHeroPocketCapture.GetSelectEndFrame(shot),
                recordedPreHandleFrameCount =
                    AuditionPvCityHeroPocketCapture.HandleFrameCount,
                recordedPostHandleFrameCount =
                    AuditionPvCityHeroPocketCapture.HandleFrameCount,
                recorderPaddingActiveAtLogicalFrameZero = true,
                recorderAutoStoppedAfterLastFrame = true,
                presentedFrameCount =
                    AuditionPvCityHeroPocketCapture.GetExpectedFrameCount(shot),
                presentedFramesExact = true,
                presentationClockExact = true,
                directorStateRestored = true,
                directorRestoreCallCountAtSequenceEnd = 1,
                directorDestroyCallCountAtSequenceEnd = 1
            };
        }

        private static AuditionPvCityHeroPocketGoldenRunner.SequenceVisualMetrics
            PassingVisualMetrics(AuditionPvCityShot shot)
        {
            int frameCount =
                AuditionPvCityHeroPocketCapture.GetSourceExpectedFrameCount(shot);
            var metrics = new AuditionPvCityHeroPocketGoldenRunner
                .SequenceVisualMetrics
            {
                shotId = AuditionPvCityHeroPocketGoldenRunner.ShotId(shot),
                sampleCount = frameCount * 100,
                blackRatio = 0.1d,
                magentaRatio = 0d,
                maximumFrameMagentaRatio = 0d,
                healthyFrameCount = frameCount,
                minimumSampledLuma = 5,
                maximumSampledLuma = 245,
                primaryDelta = Delta(60d, 0.70d)
            };
            switch (shot)
            {
                case AuditionPvCityShot.G01:
                    metrics.hudProbes = new[]
                    {
                        Hud(AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            shot, 0), 0)
                    };
                    metrics.primaryDelta = Delta(1.5d, 0.05d);
                    break;
                case AuditionPvCityShot.G02:
                    metrics.hudProbes = new[]
                    {
                        Hud(AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            shot, 0), 12),
                        Hud(AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            shot, 120), 12),
                        Hud(AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            shot, 240), 12),
                        Hud(AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            shot, 419), 12)
                    };
                    metrics.primaryDelta = Delta(3d, 0.12d);
                    metrics.dodgeDelta = Delta(2d, 0.05d);
                    break;
                case AuditionPvCityShot.G03:
                    metrics.hudProbes = new[]
                    {
                        Hud(AuditionPvCityHeroPocketCapture.LogicalToSourceFrame(
                            shot, 0), 0)
                    };
                    metrics.primaryDelta = Delta(60d, 0.70d);
                    metrics.fullCoverFrameCount = 24;
                    metrics.minimumFullCoverSampledLuma = 220;
                    metrics.maximumFullCoverSpatialChannelRange = 4;
                    metrics.fullCoverDecodedPixelHashesExact = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shot), shot, null);
            }

            return metrics;
        }

        private static AuditionPvCityHeroPocketRuntimeProof PassingG02Continuity()
        {
            return new AuditionPvCityHeroPocketRuntimeProof
            {
                shotId = AuditionPvCityHeroPocketCapture.G02ShotId,
                encounterInstanceId = 101,
                playerInstanceId = 202,
                enemyInstanceId = 303,
                enemyDiedCount = 1,
                encounterWonCount = 1,
                naturalEnemyDeathObserved = true,
                naturalWonObserved = true,
                g02EndedOutsideExitTrigger = true
            };
        }

        private static AuditionPvCityHeroPocketRuntimeProof PassingG03Continuity()
        {
            return new AuditionPvCityHeroPocketRuntimeProof
            {
                shotId = AuditionPvCityHeroPocketCapture.G03ShotId,
                encounterInstanceId = 101,
                playerInstanceId = 202,
                enemyInstanceId = 303,
                continuityFromPreviousShot = true,
                g03StartedAlreadyWon = true,
                g03StartedTransitionArmed = true,
                g03NewDamageEventCount = 0,
                g03NewDeathEventCount = 0,
                g03NewWonEventCount = 0,
                captureTransitionStartCallCount = 0
            };
        }

        private static AuditionPvCityHeroPocketGoldenRunner.HudProbeMetrics Hud(
            int frame,
            int accents)
        {
            return new AuditionPvCityHeroPocketGoldenRunner.HudProbeMetrics
            {
                frame = frame,
                cyanAccentSamples = accents
            };
        }

        private static AuditionPvCityHeroPocketGoldenRunner.ScreenDeltaMetrics Delta(
            double mean,
            double changed)
        {
            return new AuditionPvCityHeroPocketGoldenRunner.ScreenDeltaMetrics
            {
                sampleCount = 100,
                changedSampleCount = (long)Math.Round(changed * 100d),
                meanAbsoluteRgb = mean,
                changedSampleRatio = changed
            };
        }

        private static AuditionPvCityHeroPocketGoldenRunner.FrameHashEntry HashEntry(
            string id,
            string path,
            string seed)
        {
            return new AuditionPvCityHeroPocketGoldenRunner.FrameHashEntry
            {
                id = id,
                relativePath = path,
                byteLength = 25,
                sha256 = AuditionPvSha256.TextHash(seed)
            };
        }

        private static int ExecutionOrderOf<T>()
        {
            var attribute = (DefaultExecutionOrder)Attribute.GetCustomAttribute(
                typeof(T),
                typeof(DefaultExecutionOrder));
            Assert.That(attribute, Is.Not.Null, typeof(T).FullName);
            return attribute.order;
        }

        private static int CountOccurrences(string source, string value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(
                       value,
                       offset,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }

            return count;
        }

        private static string ReadProjectFile(string projectRelativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException(
                    "Could not resolve project root.");
            return File.ReadAllText(
                Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath)));
        }

        private static byte[] BuildPngHeader(int width, int height)
        {
            byte[] bytes = new byte[24];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Array.Copy(signature, bytes, signature.Length);
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

        private static void WriteBigEndianInt32(
            byte[] bytes,
            int offset,
            int value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private static AuditionPvGitSnapshot Git(
            string commit,
            string branch,
            bool dirty,
            string dirtySeed)
        {
            return new AuditionPvGitSnapshot
            {
                commitSha = commit,
                branch = branch,
                isDirty = dirty,
                dirtyStateHashSha256 = AuditionPvSha256.TextHash(dirtySeed),
                probeSucceeded = true
            };
        }

        private static AuditionPvDependencyHash Dependency(
            string path,
            long bytes,
            string hashSeed)
        {
            return new AuditionPvDependencyHash
            {
                path = path,
                exists = true,
                byteLength = bytes,
                sha256 = AuditionPvSha256.TextHash(hashSeed)
            };
        }
    }
}
