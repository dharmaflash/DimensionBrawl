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
    public sealed class AuditionPvSixtySecondAudioRightsAssemblerTests
    {
        private const string GitSha = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string ReviewTime = "2026-08-17T04:00:00Z";

        [Test]
        public void EmptySelection_ReportsRequiredAudioAndCoverageHolds_WithoutFakeRows()
        {
            using var fixture = Fixture.Create();
            var spec = new AuditionPvSixtySecondAudioRightsSelectionSpec
            {
                judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
                assemblyId = "empty-selection", productCheckpointGitSha = GitSha,
                coverage = new AuditionPvAudioRightsCoverageSelection
                {
                    judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin
                }
            };

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssemblePreview(spec, fixture.context);

            Assert.That(result.readyForComposer, Is.False);
            Assert.That(result.audio, Is.Empty);
            Assert.That(result.rights, Is.Empty);
            Assert.That(result.usedItems, Is.Empty);
            Assert.That(result.shotReferences, Has.Length.EqualTo(14));
            Assert.That(result.coverageInput.exactClosure, Is.False);
            Assert.That(result.rightsCoverageReview.sha256, Is.Empty);
            Assert.That(Codes(result), Does.Contain("AUDIO_CATEGORY_MISSING"));
            Assert.That(Codes(result), Does.Contain("AUDIO_REQUIRED_CUE_MISSING"));
            Assert.That(Codes(result), Does.Contain("RIGHTS_COVERAGE_REVIEW_PENDING"));
        }

        [Test]
        public void CompleteProjectAuthoredSelection_StreamsWaveAndWritesExactCoverageFragment()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssembleAndWriteForTests(
                    spec, fixture.context);

            Assert.That(result.readyForComposer, Is.True,
                string.Join(Environment.NewLine, result.issues.Select(value =>
                    value.severity + ":" + value.code + ":" + value.message)));
            Assert.That(result.audio, Has.Length.EqualTo(4));
            Assert.That(result.rights, Has.Length.EqualTo(4));
            Assert.That(result.usedItems, Has.Length.EqualTo(4));
            Assert.That(result.shotReferences, Has.Length.EqualTo(14));
            Assert.That(result.coverageInput.exactClosure, Is.True);
            Assert.That(File.Exists(result.rightsCoverageReview.path), Is.True);
            Assert.That(AuditionPvSha256.FileHash(result.rightsCoverageReview.path),
                Is.EqualTo(result.rightsCoverageReview.sha256));
            Assert.That(File.Exists(result.fragmentPath), Is.True);
            Assert.That(AuditionPvSha256.FileHash(result.fragmentPath),
                Is.EqualTo(result.fragmentSha256));
            Assert.That(File.Exists(result.operatorSelectionSpec.path), Is.True);
            Assert.That(AuditionPvSha256.FileHash(result.operatorSelectionSpec.path),
                Is.EqualTo(result.operatorSelectionSpec.sha256));

            AuditionPvSixtySecondShotReferenceBinding city = result.shotReferences.Single(value =>
                value.atomicShotId == "pv-s020-city-gameplay");
            Assert.That(city.audioRefIds, Does.Contain("music-main"));
            Assert.That(city.audioRefIds, Does.Contain("ambience-main"));
            Assert.That(city.audioRefIds, Does.Contain("sfx-main"));
            Assert.That(city.audioRefIds, Does.Not.Contain("vo-main"));
            AuditionPvSixtySecondShotReferenceBinding end = result.shotReferences.Single(value =>
                value.atomicShotId == "pv-s100-end-card");
            Assert.That(end.audioRefIds, Is.EqualTo(new[] { "music-main" }));
        }

        [Test]
        public void MissingHumanOrigin_CannotBecomeReady()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            spec.audio[0].listening.judgementOrigin = string.Empty;

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssemblePreview(spec, fixture.context);

            Assert.That(result.readyForComposer, Is.False);
            Assert.That(Codes(result), Does.Contain("AUDIO_LISTENING_ORIGIN_INVALID"));
        }

        [Test]
        public void DuplicateCueAcrossRows_IsRejectedBeforeGateComposition()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            spec.audio[1].cueRegions[0].cueId = "music-bed";

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssemblePreview(spec, fixture.context);

            Assert.That(result.readyForComposer, Is.False);
            Assert.That(Codes(result), Does.Contain("AUDIO_CUE_GLOBAL_DUPLICATE"));
        }

        [Test]
        public void DirtySelectedCapture_CannotProduceExactRightsClosure()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            AuditionPvAudioRightsSelectedCapture selected = spec.coverage.selectedCaptures[0];
            AuditionPvCaptureManifest capture = JsonUtility.FromJson<AuditionPvCaptureManifest>(
                File.ReadAllText(selected.sourceManifest.path, Encoding.UTF8));
            capture.gitWorktreeDirty = true;
            File.WriteAllText(selected.sourceManifest.path, JsonUtility.ToJson(capture, true) + "\n",
                new UTF8Encoding(false));
            selected.sourceManifest.sha256 = AuditionPvSha256.FileHash(selected.sourceManifest.path);
            spec.coverage.dependencies[0].sourceManifestSha256 = selected.sourceManifest.sha256;

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssemblePreview(spec, fixture.context);

            Assert.That(result.readyForComposer, Is.False);
            Assert.That(Codes(result), Does.Contain("RIGHTS_SELECTED_CAPTURE_IDENTITY_MISMATCH"));
        }

        [Test]
        public void CoverageDependencyRowsAboveGateLimit_CannotBecomeReady()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            AuditionPvRightsDependencyClassification row = spec.coverage.dependencies[0];
            spec.coverage.dependencies = Enumerable.Repeat(row, 4097).ToArray();

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssemblePreview(spec, fixture.context);

            Assert.That(result.readyForComposer, Is.False);
            Assert.That(Codes(result), Does.Contain("RIGHTS_COVERAGE_CARDINALITY_EXCEEDED"));
        }

        [Test]
        public void AudioRowsAboveGateLimit_FailBeforeAnyWaveRead()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            string wave = spec.audio[0].file.path;
            spec.audio = Enumerable.Repeat(spec.audio[0], 129).ToArray();
            File.Delete(wave);

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssemblePreview(spec, fixture.context);

            Assert.That(result.readyForComposer, Is.False);
            Assert.That(Codes(result), Does.Contain("AUDIO_CARDINALITY_EXCEEDED"));
            Assert.That(Codes(result), Does.Not.Contain("PIN_WAV_NOT_STABLE"));
        }

        [Test]
        public void CaptureShotRowsAboveGateLimit_CannotProduceExactRightsClosure()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            AuditionPvAudioRightsSelectedCapture selected = spec.coverage.selectedCaptures[0];
            AuditionPvCaptureManifest capture = JsonUtility.FromJson<AuditionPvCaptureManifest>(
                File.ReadAllText(selected.sourceManifest.path, Encoding.UTF8));
            capture.shots = Enumerable.Range(0, 513).Select(index =>
                new AuditionPvShotManifestEntry
                {
                    id = "g" + index.ToString("D3", CultureInfo.InvariantCulture),
                    scenePath = "Assets/Test.unity", startFrame = 0, endFrame = 59,
                    expectedFrameCount = 60, hudMode = "hud-on"
                }).ToArray();
            File.WriteAllText(selected.sourceManifest.path,
                JsonUtility.ToJson(capture, true) + "\n", new UTF8Encoding(false));
            selected.sourceManifest.sha256 = AuditionPvSha256.FileHash(selected.sourceManifest.path);
            spec.coverage.dependencies[0].sourceManifestSha256 = selected.sourceManifest.sha256;

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssemblePreview(spec, fixture.context);

            Assert.That(result.readyForComposer, Is.False);
            Assert.That(Codes(result), Does.Contain("RIGHTS_SELECTED_CAPTURE_UNREADABLE"));
        }

        [Test]
        public void ConflictingImmutableOutput_RollsBackFilesInstalledByThisRun()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            string reviewDirectory = Path.Combine(fixture.context.reviewRoot, "GATE_60S",
                spec.assemblyId);
            Directory.CreateDirectory(reviewDirectory);
            string conflict = Path.Combine(reviewDirectory, "audio_rights_fragment.json");
            File.WriteAllText(conflict, "conflicting immutable bytes\n", new UTF8Encoding(false));

            Assert.Throws<InvalidDataException>(() =>
                AuditionPvSixtySecondAudioRightsAssembler.AssembleAndWriteForTests(
                    spec, fixture.context));

            Assert.That(File.ReadAllText(conflict, Encoding.UTF8),
                Is.EqualTo("conflicting immutable bytes\n"));
            string audioOutput = Path.Combine(fixture.context.audioRoot, "GATE_60S", spec.assemblyId);
            string licenseOutput = Path.Combine(fixture.context.licenseRoot, "GATE_60S", spec.assemblyId);
            Assert.That(Directory.Exists(audioOutput) &&
                        Directory.EnumerateFiles(audioOutput, "*", SearchOption.AllDirectories).Any(),
                Is.False);
            Assert.That(Directory.Exists(licenseOutput) &&
                        Directory.EnumerateFiles(licenseOutput, "*", SearchOption.AllDirectories).Any(),
                Is.False);
        }

        [Test]
        public void ExternalPinMutationAfterInstall_IsDetectedAndRollsBackThisRun()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            string consumedWave = spec.audio.Single(value => value.id == "music-main").file.path;
            fixture.context.afterEvidenceInstallForTests = () =>
                File.AppendAllText(consumedWave, "external-drift", new UTF8Encoding(false));

            Assert.Throws<InvalidDataException>(() =>
                AuditionPvSixtySecondAudioRightsAssembler.AssembleAndWriteForTests(
                    spec, fixture.context));

            string audioOutput = Path.Combine(fixture.context.audioRoot, "GATE_60S", spec.assemblyId);
            string licenseOutput = Path.Combine(fixture.context.licenseRoot, "GATE_60S", spec.assemblyId);
            string reviewOutput = Path.Combine(fixture.context.reviewRoot, "GATE_60S", spec.assemblyId);
            Assert.That(Directory.Exists(audioOutput) &&
                        Directory.EnumerateFiles(audioOutput, "*", SearchOption.AllDirectories).Any(),
                Is.False);
            Assert.That(Directory.Exists(licenseOutput) &&
                        Directory.EnumerateFiles(licenseOutput, "*", SearchOption.AllDirectories).Any(),
                Is.False);
            Assert.That(Directory.Exists(reviewOutput) &&
                        Directory.EnumerateFiles(reviewOutput, "*", SearchOption.AllDirectories).Any(),
                Is.False);
        }

        [Test]
        public void AiSelection_PinsPromptRecipeGenerationAndSeparateAiRights()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            AuditionPvAudioRightsAudioSelection music = spec.audio.Single(value =>
                value.id == "music-main");
            string terms = fixture.WriteLicense("eleven_terms.json", "{\"plan\":\"Pro\"}\n");
            string evidence = fixture.WriteLicense("eleven_music_generation.json",
                "{\"provider\":\"ElevenLabs\",\"model\":\"Music v2\"}\n");
            string alternate = fixture.WriteAudio("music_alternate.wav", 1500);
            AuditionPvPinnedArtifact alternatePin = fixture.Pin(alternate);
            string sourceManifest = Path.Combine(fixture.context.audioRoot,
                "eleven_music_source_manifest.json");
            File.WriteAllText(sourceManifest,
                "{\"selectedSha256\":\"" + music.file.sha256 +
                "\",\"alternateSha256\":\"" + alternatePin.sha256 + "\"}\n",
                new UTF8Encoding(false));
            music.generatedByAi = true;
            music.rights = AiRights(fixture.Pin(terms));
            music.rights.generationEvidence = fixture.Pin(evidence);
            music.generation = new AuditionPvAudioRightsGenerationSelection
            {
                provider = "ElevenLabs", model = "Music v2", accountPlan = "Pro",
                tool = "ffmpeg", toolVersion = "8.1.2", generatedAtUtc = ReviewTime,
                voiceIdentityDisposition = "non-real-person-imitation",
                promptText = "Original instrumental trailer score; no vocals or imitation.",
                recipeSteps = new[] { "provider master", "two-pass loudness derivative" },
                sourceManifest = fixture.Pin(sourceManifest),
                originalGeneratedWav = music.file,
                alternateGeneratedWavs = new[] { alternatePin },
                termsSnapshot = fixture.Pin(terms), generationEvidence = fixture.Pin(evidence)
            };

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssembleAndWriteForTests(
                    spec, fixture.context);

            Assert.That(result.readyForComposer, Is.True,
                string.Join(Environment.NewLine, result.issues.Select(value => value.code)));
            AuditionPvSixtySecondAudioEvidence row = result.audio.Single(value =>
                value.id == "music-main");
            Assert.That(row.generatedByAi, Is.True);
            Assert.That(File.Exists(row.generationManifest.path), Is.True);
            Assert.That(AuditionPvSha256.FileHash(row.generationManifest.path),
                Is.EqualTo(row.generationManifest.sha256));
            Assert.That(result.usedItems.Select(value => value.id),
                Does.Contain("item-ai-music-main"));
            Assert.That(result.rights.Select(value => value.id),
                Does.Contain("rights-ai-music-main"));
            AuditionPvAudioRightsGenerationLedgerBinding ledger =
                result.generationLedgers.Single(value => value.audioId == "music-main");
            Assert.That(File.Exists(ledger.ledger.path), Is.True);
        }

        [Test]
        public void PretendardAndTokyoProfiles_RequireAndMaterializeRealAdmissionPins()
        {
            using var fixture = Fixture.Create();
            AuditionPvSixtySecondAudioRightsSelectionSpec spec = fixture.CompleteSpec();
            string fontRelative = "Assets/_Game/Art/Fonts/Pretendard/Pretendard-SemiBold.otf";
            string font = fixture.WriteProject(fontRelative, "font-bytes");
            string ofl = fixture.WriteProject(
                "Assets/_Game/Art/Fonts/Pretendard/Pretendard_LICENSE.txt",
                "SIL OPEN FONT LICENSE Version 1.1\n");
            string tokyoRelative =
                "Assets/_Game/Art/Environment/CityHeroPocket/TokyoStreet/Prefabs/Street.prefab";
            string tokyo = fixture.WriteProject(tokyoRelative, "tokyo-prefab");
            string admission = fixture.WriteLicense("CITY_ASSET_ADMISSION.json",
                "{\"name\":\"Tokyo Street\",\"status\":\"ADMITTED_FOR_ISOLATED_STAGING\"}\n");
            string entitlement = fixture.WriteLicense("TokyoStreet_MyAssets.png", "png-evidence");
            spec.items = new[]
            {
                new AuditionPvAudioRightsItemSelection
                {
                    id = "font-pretendard-semibold", scope = "font",
                    sourceLocator = fontRelative, expectedSha256 = AuditionPvSha256.FileHash(font),
                    dependencyBinding = "unity-dependency",
                    admissionProfile = "pretendard-ofl-1.1",
                    atomicShotIds = new[] { "pv-s100-end-card" },
                    rights = new AuditionPvAudioRightsRecordSelection
                    {
                        judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
                        disposition = "open-license", verified = true,
                        verifiedBy = "operator", verifiedAtUtc = ReviewTime,
                        useBoundary = "PV typography and embedded rendered frames only.",
                        provider = "Pretendard", licenseId = "SIL-OFL",
                        licenseVersion = "1.1", termsSnapshot = fixture.Pin(ofl)
                    }
                },
                new AuditionPvAudioRightsItemSelection
                {
                    id = "asset-tokyo-street", scope = "asset",
                    sourceLocator = tokyoRelative, expectedSha256 = AuditionPvSha256.FileHash(tokyo),
                    dependencyBinding = "unity-dependency",
                    admissionProfile = "tokyo-street-single-entity",
                    atomicShotIds = new[] { "pv-s010-city-alert-skyline",
                        "pv-s020-city-gameplay" },
                    rights = new AuditionPvAudioRightsRecordSelection
                    {
                        judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
                        disposition = "purchased", verified = true,
                        verifiedBy = "operator", verifiedAtUtc = ReviewTime,
                        useBoundary = "DimensionBrawl audition PV city footage.",
                        provider = "Art Equilibrium / Unity Asset Store",
                        licenseId = "Standard Unity Asset Store EULA",
                        licenseVersion = "2024-12-04",
                        accountEntitlementId = "single-entity-test",
                        termsSnapshot = fixture.Pin(admission),
                        entitlementEvidence = fixture.Pin(entitlement)
                    }
                }
            };
            // The selected capture does not depend on these two illustrative
            // items; shot references still make both used-item rows non-orphaned.

            AuditionPvSixtySecondAudioRightsAssembly result =
                AuditionPvSixtySecondAudioRightsAssembler.AssembleAndWriteForTests(
                    spec, fixture.context);

            Assert.That(result.readyForComposer, Is.True,
                string.Join(Environment.NewLine, result.issues.Select(value =>
                    value.code + ":" + value.message)));
            Assert.That(result.usedItems.Select(value => value.id),
                Does.Contain("font-pretendard-semibold"));
            Assert.That(result.usedItems.Select(value => value.id),
                Does.Contain("asset-tokyo-street"));
            AuditionPvSixtySecondRightsEvidence fontRights = result.rights.Single(value =>
                value.id == "rights-font-pretendard-semibold");
            Assert.That(fontRights.record.path.Replace('\\', '/'),
                Does.StartWith(fixture.context.licenseRoot.Replace('\\', '/') + "/"));
            Assert.That(File.Exists(fontRights.record.path), Is.True);
        }

        private static string[] Codes(AuditionPvSixtySecondAudioRightsAssembly value) =>
            value.issues.Select(issue => issue.code).Distinct().ToArray();

        private static AuditionPvAudioRightsRecordSelection ProjectRights() => new()
        {
            judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
            disposition = "project-authored", verified = true,
            verifiedBy = "operator", verifiedAtUtc = ReviewTime,
            useBoundary = "DimensionBrawl 60-second audition PV only.",
            owner = "DimensionBrawl project", sourceDescription = "Project-authored audio stem."
        };

        private static AuditionPvAudioRightsRecordSelection AiRights(
            AuditionPvPinnedArtifact terms) => new()
        {
            judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
            disposition = "ai-generated", verified = true,
            verifiedBy = "operator", verifiedAtUtc = ReviewTime,
            useBoundary = "Online DimensionBrawl audition promotional PV only.",
            provider = "ElevenLabs", accountPlan = "Pro", termsSnapshot = terms
        };

        private sealed class Fixture : IDisposable
        {
            public string root;
            public AuditionPvSixtySecondAudioRightsContext context;

            public static Fixture Create()
            {
                string root = Path.Combine(Path.GetTempPath(),
                    "DimensionBrawl_PV60_AudioRights_" + Guid.NewGuid().ToString("N"));
                var result = new Fixture
                {
                    root = root,
                    context = new AuditionPvSixtySecondAudioRightsContext
                    {
                        projectRoot = Path.Combine(root, "project"),
                        audioRoot = Path.Combine(root, "audio"),
                        licenseRoot = Path.Combine(root, "licenses"),
                        reviewRoot = Path.Combine(root, "reviews"),
                        captureRoots = new[] { Path.Combine(root, "captures") }
                    }
                };
                Directory.CreateDirectory(result.context.projectRoot);
                Directory.CreateDirectory(result.context.audioRoot);
                Directory.CreateDirectory(result.context.licenseRoot);
                Directory.CreateDirectory(result.context.reviewRoot);
                Directory.CreateDirectory(result.context.captureRoots[0]);
                return result;
            }

            public AuditionPvSixtySecondAudioRightsSelectionSpec CompleteSpec()
            {
                AuditionPvAudioRightsAudioSelection music = Audio("music-main", "music",
                    new[] { "music-bed" }, 2000);
                AuditionPvAudioRightsAudioSelection ambience = Audio("ambience-main", "ambience",
                    new[] { "city-ambience", "olympus-ambience" }, 2000);
                AuditionPvAudioRightsAudioSelection vo = Audio("vo-main", "vo",
                    new[] { "announcement-vo", "inori-vo", "boss-vo" }, 1000);
                AuditionPvAudioRightsAudioSelection sfx = Audio("sfx-main", "sfx",
                    new[] { "gun-mechanical", "gun-fire", "gun-tail", "dodge", "summon", "hit",
                        "boss-charge", "boss-fire", "boss-death", "wing-deploy", "eye-open" }, 2000);
                AuditionPvAudioRightsSelectedCapture capture = Capture(
                    out var classification, out AuditionPvPinnedArtifact approvalInput);
                return new AuditionPvSixtySecondAudioRightsSelectionSpec
                {
                    judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
                    assemblyId = "complete-hermetic", productCheckpointGitSha = GitSha,
                    audio = new[] { music, ambience, vo, sfx },
                    coverage = new AuditionPvAudioRightsCoverageSelection
                    {
                        judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
                        approveComplete = true, reviewedBy = "operator", reviewedAtUtc = ReviewTime,
                        approvedComposeInput = approvalInput,
                        selectedCaptures = new[] { capture }, dependencies = new[] { classification }
                    }
                };
            }

            private AuditionPvAudioRightsAudioSelection Audio(string id, string category,
                string[] cues, int durationMilliseconds)
            {
                string path = Path.Combine(context.audioRoot, id + ".wav");
                WriteWave(path, durationMilliseconds);
                return new AuditionPvAudioRightsAudioSelection
                {
                    id = id, category = category, file = Pin(path),
                    cueRegions = cues.Select((cue, index) => new AuditionPvAudioCueRegion
                    {
                        cueId = cue, startMilliseconds = 50 + index * 20,
                        endMilliseconds = Math.Min(durationMilliseconds - 50, 550 + index * 20)
                    }).ToArray(),
                    generatedByAi = false, rights = ProjectRights(),
                    listening = new AuditionPvAudioRightsListeningSelection
                    {
                        judgementOrigin = AuditionPvSixtySecondAudioRightsAssembler.HumanOperatorOrigin,
                        status = "approved", reviewedBy = "listener", reviewedAtUtc = ReviewTime
                    }
                };
            }

            private AuditionPvAudioRightsSelectedCapture Capture(
                out AuditionPvRightsDependencyClassification classification,
                out AuditionPvPinnedArtifact approvalInput)
            {
                string dependencyPath = "Assets/Settings/UniversalRP.asset";
                byte[] dependencyBytes = Encoding.UTF8.GetBytes("scene");
                string dependencySha = AuditionPvSha256.TextHash("scene");
                var dependency = new AuditionPvDependencyHash
                {
                    path = dependencyPath, exists = true, byteLength = dependencyBytes.Length,
                    sha256 = dependencySha
                };
                string captureId = "capture-one";
                string outputRoot = Path.GetFullPath(context.captureRoots[0]);
                string outputDirectory = Path.Combine(outputRoot, captureId);
                Directory.CreateDirectory(outputDirectory);
                var manifest = new AuditionPvCaptureManifest
                {
                    captureId = captureId,
                    createdAtUtc = ReviewTime,
                    outputRoot = outputRoot.Replace('\\', '/'),
                    outputDirectory = Path.GetFullPath(outputDirectory).Replace('\\', '/'),
                    gitCommitSha = GitSha,
                    gitBranch = "test",
                    gitWorktreeDirty = false,
                    worktreeDirtyHashSha256 = new string('0', 64),
                    unityVersion = "6000.3.5f2",
                    unityVersionWithRevision = "6000.3.5f2 (test)",
                    recorderPackageVersion = AuditionPvCaptureContract.RecorderPackageVersion,
                    urpPackageVersion = "17.3.0",
                    activeRenderPipelineAssetPath = dependencyPath,
                    shots = new[]
                    {
                        new AuditionPvShotManifestEntry
                        {
                            id = "g01", scenePath = "Assets/Test.unity", startFrame = 0,
                            endFrame = 59, expectedFrameCount = 60, hudMode = "hud-on"
                        }
                    },
                    baselines = new[]
                    {
                        new AuditionPvBaselineManifestEntry
                        {
                            id = "baseline-g01", shotId = "g01", sourceFrame = 0,
                            fileName = "baseline.png", hudMode = "hud-on", status = "passed"
                        }
                    },
                    dependencyHashes = new[] { dependency },
                    testResults = new[]
                    {
                        new AuditionPvTestResult
                        {
                            suite = "Hermetic", name = "capture-ready", status = "passed"
                        }
                    }
                };
                AuditionPvCaptureManifestWriter.Validate(manifest);
                string path = Path.Combine(outputDirectory, AuditionPvCaptureContract.ManifestFileName);
                File.WriteAllText(path, JsonUtility.ToJson(manifest, true) + "\n",
                    new UTF8Encoding(false));
                string pinSha = AuditionPvSha256.FileHash(path);
                string digestMaterial = dependencyPath + "\0" + "1" + "\0" +
                    dependencyBytes.Length.ToString(CultureInfo.InvariantCulture) + "\0" +
                    dependencySha + "\0";
                classification = new AuditionPvRightsDependencyClassification
                {
                    captureId = manifest.captureId, sourceManifestSha256 = pinSha,
                    path = dependencyPath, byteLength = dependencyBytes.Length,
                    sha256 = dependencySha, disposition = "project-authored",
                    reason = "Project-authored scene dependency verified by the operator."
                };
                string[] movingShots =
                {
                    "pv-s010-city-alert-skyline", "pv-s010-dimensional-anomaly",
                    "pv-s020-city-gameplay", "pv-s030-hit-dodge-summon",
                    "pv-s040-dimension-rift", "pv-s050-boss-low-angle",
                    "pv-s060-wing-deployment", "pv-s060-eye-open",
                    "pv-s070-pattern-one", "pv-s070-patterns-two-three",
                    "pv-s080-dodge-summon-defense", "pv-s080-tier3-ultimate",
                    "pv-s090-finisher-aftermath"
                };
                var approval = new AuditionPvSixtySecondProductionComposeInput
                {
                    productCheckpointGitSha = GitSha,
                    captureManifestPaths = new[] { path.Replace('\\', '/') },
                    takeEvidence = movingShots.Select(shot =>
                        new AuditionPvSixtySecondTakeEvidenceBinding
                        {
                            atomicShotId = shot, sourceCaptureId = captureId,
                            sourceShotId = "g01", approved = true, cleanPlate = false
                        }).Append(new AuditionPvSixtySecondTakeEvidenceBinding
                        {
                            atomicShotId = "pv-s060-eye-open", sourceCaptureId = captureId,
                            sourceShotId = "g01-clean", approved = false, cleanPlate = true
                        }).ToArray()
                };
                string approvalPath = Path.Combine(context.reviewRoot,
                    "approved_compose_input.json");
                File.WriteAllText(approvalPath, JsonUtility.ToJson(approval, true) + "\n",
                    new UTF8Encoding(false));
                approvalInput = Pin(approvalPath);
                return new AuditionPvAudioRightsSelectedCapture
                {
                    captureId = manifest.captureId,
                    sourceManifest = new AuditionPvPinnedArtifact { path = path, sha256 = pinSha },
                    sourceDependencyIdentitySha256 = AuditionPvSha256.TextHash(digestMaterial)
                };
            }

            public string WriteProject(string relative, string text)
            {
                string path = Path.Combine(context.projectRoot,
                    relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, text, new UTF8Encoding(false));
                return path;
            }

            public string WriteLicense(string name, string text)
            {
                string path = Path.Combine(context.licenseRoot, name);
                File.WriteAllText(path, text, new UTF8Encoding(false));
                return path;
            }

            public string WriteAudio(string name, int durationMilliseconds)
            {
                string path = Path.Combine(context.audioRoot, name);
                WriteWave(path, durationMilliseconds);
                return path;
            }

            public AuditionPvPinnedArtifact Pin(string path) => new()
                { path = path, sha256 = AuditionPvSha256.FileHash(path) };

            public void Dispose()
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }

            private static void WriteWave(string path, int durationMilliseconds)
            {
                const int sampleRate = 48000, channels = 2, bits = 16;
                int frames = checked(sampleRate * durationMilliseconds / 1000);
                int dataBytes = checked(frames * channels * (bits / 8));
                using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                using var writer = new BinaryWriter(stream, Encoding.ASCII, true);
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(checked(36 + dataBytes));
                writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((ushort)1); writer.Write((ushort)channels);
                writer.Write(sampleRate); writer.Write(sampleRate * channels * (bits / 8));
                writer.Write((ushort)(channels * (bits / 8))); writer.Write((ushort)bits);
                writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(dataBytes);
                for (int frame = 0; frame < frames; frame++)
                {
                    short sample = (short)(Math.Sin(frame * 2d * Math.PI * 440d / sampleRate) * 6000d);
                    writer.Write(sample); writer.Write(sample);
                }
            }
        }
    }
}
