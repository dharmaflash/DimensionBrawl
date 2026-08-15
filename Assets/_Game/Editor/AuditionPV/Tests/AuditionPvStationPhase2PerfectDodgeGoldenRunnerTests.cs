using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DimensionBrawl.Presentation;
using DimensionBrawl.Rendering;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvStationPhase2PerfectDodgeGoldenRunnerTests
    {
        private const string PerfectDodgeDomainMaterialPath =
            "Assets/_Game/Art/VFX/CombatCues/Materials/DB_PerfectDodgeScreenDomain.mat";

        [Test]
        public void PerfectDodgeRendererFeatureMutatesOnlyItsRuntimeMaterialCopy()
        {
            Material source = AssetDatabase.LoadAssetAtPath<Material>(PerfectDodgeDomainMaterialPath);
            Assert.That(source, Is.Not.Null);

            float sourceIntensity = source.GetFloat("_Intensity");
            float sourceSustain = source.GetFloat("_Sustain");
            bool sourceWasDirty = EditorUtility.IsDirty(source);
            PerfectDodgeScreenDomainRendererFeature feature =
                ScriptableObject.CreateInstance<PerfectDodgeScreenDomainRendererFeature>();

            try
            {
                feature.SetPassMaterial(source);
                feature.Create();
                FieldInfo runtimeMaterialField = typeof(PerfectDodgeScreenDomainRendererFeature)
                    .GetField("runtimePassMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(runtimeMaterialField, Is.Not.Null);

                Material runtimeMaterial = runtimeMaterialField.GetValue(feature) as Material;
                Assert.That(runtimeMaterial, Is.Not.Null);
                Assert.That(runtimeMaterial, Is.Not.SameAs(source));
                Assert.That(runtimeMaterial.hideFlags, Is.EqualTo(HideFlags.HideAndDontSave));

                PerfectDodgeScreenDomainRuntime.Publish(
                    Color.black,
                    Color.cyan,
                    Color.white,
                    0.2f,
                    0.1f,
                    0.3f,
                    0.15f,
                    0.92f,
                    0.8f,
                    0.25f,
                    0.5f,
                    0.7f,
                    0.5f,
                    0.3f,
                    0.6f,
                    0.4f,
                    0.9f,
                    new Vector2(0.4f, 0.6f),
                    1.25f);
                PerfectDodgeScreenDomainRuntime.ApplyToMaterial(runtimeMaterial, 2560, 1440);

                Assert.That(runtimeMaterial.GetFloat("_Intensity"), Is.EqualTo(0.92f).Within(0.0001f));
                Assert.That(source.GetFloat("_Intensity"), Is.EqualTo(sourceIntensity).Within(0.0001f));
                Assert.That(source.GetFloat("_Sustain"), Is.EqualTo(sourceSustain).Within(0.0001f));
                Assert.That(EditorUtility.IsDirty(source), Is.EqualTo(sourceWasDirty));

                feature.SetPassMaterial(null);
                Assert.That(runtimeMaterialField.GetValue(feature), Is.Null);
            }
            finally
            {
                PerfectDodgeScreenDomainRuntime.Clear();
                feature.SetPassMaterial(null);
                UnityEngine.Object.DestroyImmediate(feature);
            }
        }

        [Test]
        public void RecorderMapping_IsRawZeroWarmupAndRawOneThrough197LogicalShot()
        {
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RawWarmupFrame,
                Is.EqualTo(0));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RawFirstShotFrame,
                Is.EqualTo(1));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RawLastShotFrame,
                Is.EqualTo(197));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.ExpectedRawFrameCount,
                Is.EqualTo(198));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.ExpectedFrameCount,
                Is.EqualTo(197));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RawFrameFileName(0),
                Is.EqualTo("frame_0000.png"));
            Assert.That(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RawFrameFileName(197),
                Is.EqualTo("frame_0197.png"));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RawFrameFileName(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RawFrameFileName(198));
        }

        [Test]
        public void BatchArguments_RequireAsyncGraphicsAndNoAudioContract()
        {
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateBatchCommandLine(new[]
                    {
                        "Unity.exe",
                        "-noaudio",
                        "-executeMethod",
                        "DimensionBrawl.Editor.AuditionPV.AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunBatchCapture"
                    }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateBatchCommandLine(new[]
                    {
                        "Unity.exe", "-noaudio", "-quit"
                    }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateBatchCommandLine(new[]
                    {
                        "Unity.exe", "-noaudio", "-nographics"
                    }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateBatchCommandLine(new[] { "Unity.exe" }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateBatchCommandLine(new[]
                    {
                        "Unity.exe", "-batchmode", "-noaudio"
                    }));
        }

        [Test]
        public void RunnerSource_UsesRecorderPublicApiSessionStateAndNoSceneSaveOrReflection()
        {
            string source = ReadProjectFile(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunnerScriptPath);

            Assert.That(source, Does.Contain("[MenuItem(MenuPath)]"));
            Assert.That(source, Does.Contain("public static void RunBatchCapture()"));
            Assert.That(source, Does.Contain("SessionState.SetString"));
            Assert.That(source, Does.Contain("EditorApplication.playModeStateChanged"));
            Assert.That(source, Does.Contain("SetRecordModeToFrameInterval("));
            Assert.That(source, Does.Contain("recorderController.PrepareRecording();"));
            Assert.That(source, Does.Contain("recorderController.StartRecording()"));
            Assert.That(source, Does.Contain("new WaitForEndOfFrame()"));
            Assert.That(source, Does.Contain("director.BeginShotForRecorder();"));
            Assert.That(source, Does.Contain("recorderController?.StopRecording();"));
            Assert.That(source, Does.Contain(".g05-remap-"));
            Assert.That(source, Does.Contain("ReopenProductSceneAfterPlayMode"));
            Assert.That(source, Does.Contain("ValidateStableGitSnapshot"));
            Assert.That(source, Does.Contain("ValidateStableDependencies"));
            Assert.That(source, Does.Contain("AuditionPvCaptureManifestWriter.WriteNew"));
            Assert.That(source, Does.Contain(".42/.18/.48/.16"));
            Assert.That(source, Does.Not.Contain("System.Reflection"));
            Assert.That(source, Does.Not.Contain("BindingFlags"));
            Assert.That(source, Does.Not.Contain("GetComponentsInChildren<Canvas"));
            Assert.That(source, Does.Not.Contain("EditorSceneManager.Save"));
        }

        [Test]
        public void Dependencies_IncludeRunnerTestsSceneAndProductGraph()
        {
            string[] dependencies =
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .CollectCaptureDependencyPaths();

            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunnerScriptPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunnerTestPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.CaptureScriptPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.CadenceSchedulerPath));
            Assert.That(dependencies, Does.Contain(
                AuditionPvStationPhase2PerfectDodgeCapture.ActionScreenPath));

            foreach (string direct in new[]
                     {
                         AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunnerScriptPath,
                         AuditionPvStationPhase2PerfectDodgeGoldenRunner.RunnerTestPath,
                         AuditionPvStationPhase2PerfectDodgeCapture.StationScenePath
                     })
            {
                Assert.That(AssetDatabase.LoadMainAssetAtPath(direct), Is.Not.Null, direct);
            }
        }

        [Test]
        public void RawRemap_PreservesWarmupAndMapsEveryLogicalFrameWithoutOverwrite()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_G05Remap_" + Guid.NewGuid().ToString("N"));
            string frames = Path.Combine(root, "frames", "g05");
            string evidence = Path.Combine(root, "evidence");
            Directory.CreateDirectory(frames);
            try
            {
                for (int raw = 0;
                    raw < AuditionPvStationPhase2PerfectDodgeGoldenRunner
                        .ExpectedRawFrameCount;
                    raw++)
                {
                    File.WriteAllBytes(
                        Path.Combine(
                            frames,
                            AuditionPvStationPhase2PerfectDodgeGoldenRunner
                                .RawFrameFileName(raw)),
                        BitConverter.GetBytes(raw));
                }

                string warmup =
                    AuditionPvStationPhase2PerfectDodgeGoldenRunner.RemapRawFrames(
                        frames,
                        evidence);

                Assert.That(File.Exists(warmup), Is.True);
                Assert.That(BitConverter.ToInt32(File.ReadAllBytes(warmup), 0),
                    Is.EqualTo(0));
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateLogicalFrameSequence(frames);
                Assert.That(
                    BitConverter.ToInt32(
                        File.ReadAllBytes(Path.Combine(frames, "frame_0000.png")),
                        0),
                    Is.EqualTo(1));
                Assert.That(
                    BitConverter.ToInt32(
                        File.ReadAllBytes(Path.Combine(frames, "frame_0196.png")),
                        0),
                    Is.EqualTo(197));
                Assert.That(
                    Directory.GetFiles(frames, "frame_*.png").Length,
                    Is.EqualTo(197));
                Assert.That(
                    Directory.GetDirectories(Path.Combine(root, "frames"), ".g05-remap-*").Length,
                    Is.EqualTo(0));
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
        public void RawSequence_RejectsMissingOrExtraFrame()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_G05RawSequence_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                for (int raw = 0;
                    raw < AuditionPvStationPhase2PerfectDodgeGoldenRunner
                        .ExpectedRawFrameCount - 1;
                    raw++)
                {
                    File.WriteAllBytes(
                        Path.Combine(
                            root,
                            AuditionPvStationPhase2PerfectDodgeGoldenRunner
                                .RawFrameFileName(raw)),
                        new byte[] { 1 });
                }

                Assert.Throws<InvalidOperationException>(() =>
                    AuditionPvStationPhase2PerfectDodgeGoldenRunner
                        .ValidateRawFrameSequence(root));
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
                "DimensionBrawl_G05Png_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string path = Path.Combine(root, "frame.png");
            try
            {
                File.WriteAllBytes(path, BuildPngHeader(2560, 1440));
                Assert.DoesNotThrow(() =>
                    AuditionPvStationPhase2PerfectDodgeGoldenRunner.ValidatePngFile(
                        path,
                        2560,
                        1440));
                Assert.Throws<InvalidDataException>(() =>
                    AuditionPvStationPhase2PerfectDodgeGoldenRunner.ValidatePngFile(
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
        public void ScreenDelta_RequiresVisibleF189PixelChange()
        {
            Color32[] before = Enumerable.Repeat(
                new Color32(20, 30, 40, 255),
                128).ToArray();
            Color32[] after = Enumerable.Repeat(
                new Color32(35, 50, 65, 255),
                128).ToArray();
            AuditionPvStationPhase2PerfectDodgeGoldenRunner.ScreenDeltaMetrics
                visible = AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .EvaluateScreenDelta(before, after);

            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateScreenDelta(visible));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateScreenDelta(
                        AuditionPvStationPhase2PerfectDodgeGoldenRunner
                            .EvaluateScreenDelta(before, before)));
        }

        [Test]
        public void VisualMetrics_RejectBlackMagentaAndMissingHudEvidence()
        {
            var healthy = new AuditionPvStationPhase2PerfectDodgeGoldenRunner
                .SequenceVisualMetrics
            {
                sampleCount = 10000,
                blackRatio = 0.2,
                magentaRatio = 0,
                maximumFrameMagentaRatio = 0,
                healthyFrameCount = 197,
                minimumSampledLuma = 8,
                maximumSampledLuma = 220,
                frameZeroHudAccentSamples = 40
            };
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateVisualSequence(healthy));

            var black = CloneMetrics(healthy);
            black.blackRatio = 0.95;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateVisualSequence(black));

            var magenta = CloneMetrics(healthy);
            magenta.maximumFrameMagentaRatio = 0.05;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateVisualSequence(magenta));

            var noHud = CloneMetrics(healthy);
            noHud.frameZeroHudAccentSamples = 0;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateVisualSequence(noHud));
        }

        [Test]
        public void RuntimeProof_RequiresExactGameplayRecorderHudAndRestorationEvidence()
        {
            AuditionPvStationPhase2PerfectDodgeGoldenRunner.RuntimeProof passing =
                PassingRuntimeProof();
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateRuntimeProof(passing));

            AuditionPvStationPhase2PerfectDodgeGoldenRunner.RuntimeProof duplicate =
                PassingRuntimeProof();
            duplicate.perfectDodgeCount = 2;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateRuntimeProof(duplicate));

            AuditionPvStationPhase2PerfectDodgeGoldenRunner.RuntimeProof damaged =
                PassingRuntimeProof();
            damaged.playerHealthUnchanged = false;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateRuntimeProof(damaged));

            AuditionPvStationPhase2PerfectDodgeGoldenRunner.RuntimeProof leaked =
                PassingRuntimeProof();
            leaked.cadenceSuspensionCountAfterRestore = 1;
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateRuntimeProof(leaked));
        }

        [Test]
        public void SourceStability_RejectsGitOrDependencyDrift()
        {
            AuditionPvGitSnapshot git = Git("abc1234", "main", true, "dirty");
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateStableGitSnapshot(
                        git,
                        Git("abc1234", "main", true, "dirty")));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateStableGitSnapshot(
                        git,
                        Git("def5678", "main", true, "dirty")));

            AuditionPvDependencyHash[] dependencies =
            {
                Dependency("Assets/Scene.unity", 12, "aaa"),
                Dependency("Assets/Code.cs", 34, "bbb")
            };
            Assert.DoesNotThrow(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateStableDependencies(
                        dependencies,
                        new[]
                        {
                            Dependency("Assets/Code.cs", 34, "bbb"),
                            Dependency("Assets/Scene.unity", 12, "aaa")
                        }));
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .ValidateStableDependencies(
                        dependencies,
                        new[]
                        {
                            Dependency("Assets/Scene.unity", 12, "changed"),
                            Dependency("Assets/Code.cs", 34, "bbb")
                        }));
        }

        [Test]
        public void ManifestDisclosure_RecordsImpactAndFirstVisibleScreenFramesSeparately()
        {
            AuditionPvShotManifestEntry shot =
                AuditionPvStationPhase2PerfectDodgeCapture
                    .CreateShotManifestEntry();
            AuditionPvBaselineManifestEntry bl06 =
                AuditionPvStationPhase2PerfectDodgeCapture
                    .CreateBaselineManifestEntries()
                    .Single(value => value.id == "bl06");

            Assert.That(
                AuditionPvStationPhase2PerfectDodgeCapture.ImpactFrame,
                Is.EqualTo(188));
            Assert.That(bl06.sourceFrame, Is.EqualTo(189));
            Assert.That(shot.notes, Does.Contain("impact f188"));
            Assert.That(shot.notes, Does.Contain("screen-domain hero f189"));
            Assert.That(shot.notes, Does.Contain(".42/.18/.48/.16"));
        }

        private static string ReadProjectFile(string projectRelativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve project root.");
            return File.ReadAllText(
                Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath)));
        }

        private static byte[] BuildPngHeader(int width, int height)
        {
            byte[] bytes = new byte[24];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Buffer.BlockCopy(signature, 0, bytes, 0, signature.Length);
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

        private static AuditionPvStationPhase2PerfectDodgeGoldenRunner
            .SequenceVisualMetrics CloneMetrics(
                AuditionPvStationPhase2PerfectDodgeGoldenRunner
                    .SequenceVisualMetrics source)
        {
            return new AuditionPvStationPhase2PerfectDodgeGoldenRunner
                .SequenceVisualMetrics
            {
                sampleCount = source.sampleCount,
                blackSampleCount = source.blackSampleCount,
                magentaSampleCount = source.magentaSampleCount,
                healthyFrameCount = source.healthyFrameCount,
                magentaAffectedFrameCount = source.magentaAffectedFrameCount,
                blackRatio = source.blackRatio,
                magentaRatio = source.magentaRatio,
                maximumFrameMagentaRatio = source.maximumFrameMagentaRatio,
                minimumSampledLuma = source.minimumSampledLuma,
                maximumSampledLuma = source.maximumSampledLuma,
                frameZeroHudAccentSamples = source.frameZeroHudAccentSamples
            };
        }

        private static AuditionPvStationPhase2PerfectDodgeGoldenRunner.RuntimeProof
            PassingRuntimeProof()
        {
            return new AuditionPvStationPhase2PerfectDodgeGoldenRunner.RuntimeProof
            {
                directorCompleted = true,
                lastLogicalFrame = 196,
                presentedFrameCount = 197,
                presentedFramesExact = true,
                presentationClockExact = true,
                perfectDodgeCount = 1,
                firedProjectileCount = 6,
                usedActualCrushNetPattern = true,
                impactAppliedOrBlocked = true,
                impactProjectileInactive = true,
                damageBlockedObservationCount = 1,
                damageModifyingObservationCount = 0,
                playerHealthUnchanged = true,
                cameraCueRequested = true,
                screenCueRequested = true,
                screenCueActiveAtBaselineFrame = true,
                captureOnlyScreenProfileActive = true,
                exactHudRenderable = true,
                exactHudResources = true,
                exactEnergyBinding = true,
                hudAmmo = 24,
                hudMagazineSize = 24,
                hudEnergyMana = 100f,
                hudEnergyMaxMana = 100f,
                recorderWarmupEndOfFrameCount = 2,
                recorderPaddingActiveAtLogicalFrameZero = true,
                recorderAutoStoppedAfterLastFrame = true,
                stateRestored = true,
                screenProfileRestored = true,
                presentationClockReleased = true,
                cadenceSuspensionCountAfterRestore = 0
            };
        }

        private static AuditionPvGitSnapshot Git(
            string commit,
            string branch,
            bool dirty,
            string dirtyHash)
        {
            return new AuditionPvGitSnapshot
            {
                commitSha = commit,
                branch = branch,
                isDirty = dirty,
                dirtyStateHashSha256 = dirtyHash,
                probeSucceeded = true
            };
        }

        private static AuditionPvDependencyHash Dependency(
            string path,
            long bytes,
            string hash)
        {
            return new AuditionPvDependencyHash
            {
                path = path,
                exists = true,
                byteLength = bytes,
                sha256 = hash
            };
        }
    }
}
