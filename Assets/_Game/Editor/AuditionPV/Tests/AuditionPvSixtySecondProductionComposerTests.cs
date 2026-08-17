using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvSixtySecondProductionComposerTests
    {
        [Test]
        public void DefaultEdl_IsExactContiguousSixtySeconds_WithPhysicalSelectLengths()
        {
            AuditionPvSixtySecondProductionEdlRow[] rows =
                AuditionPvSixtySecondProductionComposer.CreateDefaultEdlForTests();

            Assert.That(rows, Has.Length.EqualTo(14));
            Assert.That(rows[0].timelineStartFrame, Is.Zero);
            Assert.That(rows[^1].timelineEndFrame, Is.EqualTo(3599));
            for (int index = 0; index < rows.Length; index++)
            {
                if (index > 0)
                    Assert.That(rows[index].timelineStartFrame,
                        Is.EqualTo(rows[index - 1].timelineEndFrame + 1));
                int timelineLength = rows[index].timelineEndFrame -
                    rows[index].timelineStartFrame + 1;
                if (rows[index].bucketId == "PV_S100") continue;
                int sourceLength = rows[index].selectEndFrame -
                    rows[index].selectStartFrame + 1;
                Assert.That(sourceLength, Is.EqualTo(timelineLength), rows[index].atomicShotId);
                Assert.That(rows[index].handleBeforeFrames, Is.EqualTo(180));
                Assert.That(rows[index].handleAfterFrames, Is.EqualTo(180));
                Assert.That(rows[index].selectStartFrame - rows[index].sourceRangeStartFrame,
                    Is.EqualTo(180));
                Assert.That(rows[index].sourceRangeEndFrame - rows[index].selectEndFrame,
                    Is.EqualTo(180));
            }

            AuditionPvSixtySecondProductionEdlRow[] s060 = rows
                .Where(value => value.bucketId == "PV_S060").ToArray();
            Assert.That(s060, Has.Length.EqualTo(2));
            Assert.That(s060.Sum(Length), Is.EqualTo(240));
            Assert.That(s060[0].selectEndFrame - s060[1].selectStartFrame + 1,
                Is.EqualTo(2), "G04's 238 authored frames use one explicit 2-frame overlap.");
            Assert.That(rows.Where(value => value.bucketId == "PV_S080").Sum(Length),
                Is.EqualTo(600));

            AuditionPvSixtySecondProductionEdlRow[] s010 = rows
                .Where(value => value.bucketId == "PV_S010").ToArray();
            Assert.That(s010, Has.Length.EqualTo(2));
            Assert.That(s010.Sum(Length), Is.EqualTo(240));
            Assert.That(s010[0].sourceShotId, Is.EqualTo("g01"));
            Assert.That(s010[0].beatIds, Is.EquivalentTo(new[]
                { "city-alert", "city-skyline" }));
            Assert.That(s010[1].sourceShotId, Is.EqualTo("g03"));
            Assert.That(s010[1].beatIds, Is.EqualTo(new[]
                { "dimensional-anomaly" }));
        }

        [Test]
        public void EmptyInput_ProducesOnlyFailClosedInventory_WithAllMajorHolds()
        {
            string root = NewRoot();
            try
            {
                AuditionPvSixtySecondProductionComposition result =
                    AuditionPvSixtySecondProductionComposer.BuildInventoryForTests(
                        new AuditionPvSixtySecondProductionComposeInput(), root);

                Assert.That(result.finalManifestReady, Is.False);
                Assert.That(result.manifest, Is.Null);
                Assert.That(result.inventory.status, Is.EqualTo("partial-evidence-missing"));
                Assert.That(result.inventory.authoritativeEligible, Is.False);
                Assert.That(result.inventory.expectedCaptureRunCount, Is.EqualTo(19));
                Assert.That(result.inventory.observedEligibleCaptureRunCount, Is.Zero);
                Assert.That(result.inventory.missingRequirements,
                    Has.Some.EqualTo("AUDIO_ROWS_MISSING"));
                Assert.That(result.inventory.missingRequirements,
                    Has.Some.EqualTo("RIGHTS_ROWS_MISSING"));
                Assert.That(result.inventory.missingRequirements,
                    Has.Some.StartsWith("APPROVED_TAKE_COUNT:"));
                Assert.That(result.inventory.missingRequirements,
                    Has.Some.StartsWith("CLEAN_PLATE_COUNT:"));
            }
            finally { Delete(root); }
        }

        [Test]
        public void AuthoritativeDestination_RejectsTemporaryOrHermeticPath()
        {
            string temporary = Path.Combine(Path.GetTempPath(),
                "preedit_60s_shot_gate_manifest.json");
            Assert.Throws<InvalidOperationException>(() =>
                AuditionPvSixtySecondProductionComposer
                    .AssertAuthoritativeDestinationForTests(temporary));
        }

        [Test]
        public void CompleteHermeticAssembly_BuildsStructureButCannotBecomeAuthoritativePass()
        {
            using var fixture = CompleteFixture.Create();

            AuditionPvSixtySecondProductionComposition result =
                AuditionPvSixtySecondProductionComposer.ComposeForTests(
                    fixture.input, fixture.captureRoot);

            Assert.That(result.finalManifestReady, Is.False,
                string.Join(Environment.NewLine,
                    result.inventory.missingRequirements ?? Array.Empty<string>()));
            Assert.That(result.hermeticTestSeam, Is.True);
            Assert.That(result.inventory.status, Is.EqualTo("hermetic-structure-only"));
            Assert.That(result.inventory.authoritativeEligible, Is.False);
            Assert.That(result.manifest, Is.Not.Null);
            AuditionPvSixtySecondGateValidationReport structure =
                AuditionPvSixtySecondGateManifestValidator.ValidateStructure(result.manifest);
            Assert.That(structure.structureValid, Is.True,
                string.Join(Environment.NewLine, (structure.issues ??
                    Array.Empty<AuditionPvSixtySecondGateIssue>()).Select(value =>
                    value.code + "@" + value.location)));
            Assert.That(structure.passed, Is.False);

            string path = Path.Combine(fixture.root, "hermetic_manifest.json");
            File.WriteAllText(path, JsonUtility.ToJson(result.manifest, true),
                new UTF8Encoding(false));
            AuditionPvSixtySecondGateValidationReport nonAuthoritative =
                AuditionPvSixtySecondGateManifestValidator.ValidateProductionFile(path,
                    new AuditionPvSixtySecondValidationContext
                    {
                        projectRoot = fixture.root,
                        currentGitCommitSha = CompleteFixture.GitSha,
                        currentGitClean = true,
                        allowedEvidenceRoots = new[] { fixture.root },
                        allowedCaptureRoots = new[] { fixture.captureRoot },
                        allowedSelectRoots = new[] { fixture.root },
                        allowedAudioRoots = new[] { fixture.root },
                        allowedLicenseRoots = new[] { fixture.root },
                        allowedGraphicsRoots = new[] { fixture.root },
                        allowedReviewRoots = new[] { fixture.root }
                    });
            Assert.That(nonAuthoritative.passed, Is.False,
                "Caller roots and temporary manifests must never be authoritative.");
        }

        private static int Length(AuditionPvSixtySecondProductionEdlRow row) =>
            row.timelineEndFrame - row.timelineStartFrame + 1;

        private static string NewRoot()
        {
            string root = Path.Combine(Path.GetTempPath(),
                "DimensionBrawl_PV60_Composer_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        private static void Delete(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        private sealed class CompleteFixture : IDisposable
        {
            internal const string GitSha = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            private const string Hash =
                "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
            private readonly Dictionary<string, string[]> captureIdsByFamily =
                new(StringComparer.Ordinal);

            public string root, captureRoot;
            public AuditionPvSixtySecondProductionComposeInput input;

            public static CompleteFixture Create()
            {
                var result = new CompleteFixture
                {
                    root = NewRoot()
                };
                result.captureRoot = Path.Combine(result.root, "captures");
                Directory.CreateDirectory(result.captureRoot);
                result.Materialize();
                return result;
            }

            private void Materialize()
            {
                var paths = new List<string>();
                AddFamily(paths, "city-g01-g03", 3,
                    Shot("g01", 599, "hud-off", CityScene()),
                    Shot("g02", 779, "hud-on", CityScene()),
                    Shot("g03", 659, "hud-off", CityScene()));
                AddFamily(paths, "city-s030", 3,
                    Shot("s030", 719, "hud-on", CityScene()));
                AddFamily(paths, "station-s050", 1,
                    Shot("s050", 599, "hud-off", StationScene()));
                AddFamily(paths, "station-g04", 3,
                    Shot("g04", 597, "hud-off", StationScene()),
                    Shot("g04-clean", 597, "clean-plate", StationScene()));
                AddFamily(paths, "station-g06", 3,
                    Shot("g06", 719, "hud-on", StationScene()));
                AddFamily(paths, "station-g07", 3,
                    Shot("g07", 779, "hud-on", StationScene()));
                AddFamily(paths, "station-g08", 3,
                    Shot("g08", 719, "hud-on-to-result", StationScene()));

                AuditionPvPinnedArtifact dummy = WritePin("evidence/dummy.bin", "dummy");
                AuditionPvPinnedArtifact graphic = WritePin("graphics/end-card.png", "png");
                input = new AuditionPvSixtySecondProductionComposeInput
                {
                    productCheckpointGitSha = GitSha,
                    captureManifestPaths = paths.ToArray(),
                    endCardGraphic = graphic,
                    rights = Rights(dummy),
                    usedItems = UsedItems(dummy, graphic),
                    audio = Audio(dummy),
                    gateEvidence = GateEvidence(dummy)
                };

                AuditionPvSixtySecondProductionComposition partial =
                    AuditionPvSixtySecondProductionComposer.BuildInventoryForTests(
                        input, captureRoot);
                var bindings = new List<AuditionPvSixtySecondTakeEvidenceBinding>();
                var refs = new List<AuditionPvSixtySecondShotReferenceBinding>();
                foreach (AuditionPvSixtySecondProductionEdlRow row in partial.inventory.edl)
                {
                    refs.Add(new AuditionPvSixtySecondShotReferenceBinding
                    {
                        atomicShotId = row.atomicShotId,
                        audioRefIds = new[]
                            { "audio-music", "audio-sfx", "audio-vo", "audio-ambience" },
                        usedItemIds = row.bucketId == "PV_S100"
                            ? new[] { "item-graphic", "item-font" }
                            : new[] { "item-product", "item-ai" }
                    });
                    for (int index = 0; index < row.candidateCaptureIds.Length; index++)
                    {
                        bindings.Add(new AuditionPvSixtySecondTakeEvidenceBinding
                        {
                            atomicShotId = row.atomicShotId,
                            sourceCaptureId = row.candidateCaptureIds[index],
                            sourceShotId = row.sourceShotId,
                            approved = index == 0,
                            semanticProof = dummy,
                            automatedProof = index == 0 ? dummy : new AuditionPvPinnedArtifact(),
                            humanReview = index == 0 ? dummy : new AuditionPvPinnedArtifact()
                        });
                    }
                }
                AuditionPvSixtySecondProductionEdlRow cleanRow = partial.inventory.edl
                    .Single(value => value.atomicShotId == "pv-s060-eye-open");
                bindings.Add(new AuditionPvSixtySecondTakeEvidenceBinding
                {
                    atomicShotId = cleanRow.atomicShotId,
                    sourceCaptureId = cleanRow.candidateCaptureIds[0],
                    sourceShotId = "g04-clean",
                    cleanPlate = true,
                    cleanPlateProof = dummy,
                    automatedProof = dummy,
                    humanReview = dummy
                });
                input.takeEvidence = bindings.ToArray();
                input.shotReferences = refs.ToArray();
            }

            private void AddFamily(List<string> paths, string family, int count,
                params AuditionPvShotManifestEntry[] shots)
            {
                var ids = new List<string>();
                for (int ordinal = 1; ordinal <= count; ordinal++)
                {
                    string id = family.Replace("city-", "c-")
                        .Replace("station-", "s-") + "-take-" + ordinal;
                    ids.Add(id);
                    paths.Add(CreateCapture(id, shots));
                }
                captureIdsByFamily.Add(family, ids.ToArray());
            }

            private string CreateCapture(string captureId,
                AuditionPvShotManifestEntry[] sourceShots)
            {
                string directory = Path.Combine(captureRoot, captureId);
                string evidence = Path.Combine(directory, "evidence");
                Directory.CreateDirectory(evidence);
                string ledger = Path.Combine(evidence, "frame_hashes.sha256");
                File.WriteAllText(ledger,
                    Hash + "  frames/" + sourceShots[0].id + "/frame_0000.png\n",
                    new UTF8Encoding(false));

                var capture = new AuditionPvCaptureManifest
                {
                    captureId = captureId,
                    createdAtUtc = "2026-08-17T00:00:00.0000000Z",
                    outputRoot = captureRoot.Replace('\\', '/'),
                    outputDirectory = directory.Replace('\\', '/'),
                    gitCommitSha = GitSha,
                    gitBranch = "codex/test",
                    gitWorktreeDirty = false,
                    worktreeDirtyHashSha256 = Hash,
                    unityVersion = "6000.3.5f2",
                    unityVersionWithRevision = "6000.3.5f2 (test)",
                    recorderPackageVersion = AuditionPvCaptureContract.RecorderPackageVersion,
                    urpPackageVersion = "17.3.0",
                    activeRenderPipelineAssetPath = "Assets/fake-pipeline.asset",
                    shots = sourceShots.Select(CloneShot).ToArray(),
                    baselines = new[]
                    {
                        new AuditionPvBaselineManifestEntry
                        {
                            id = "baseline", shotId = sourceShots[0].id,
                            sourceFrame = sourceShots[0].startFrame,
                            fileName = "baseline.png", hudMode = sourceShots[0].hudMode,
                            status = "captured"
                        }
                    },
                    dependencyHashes = new[]
                    {
                        new AuditionPvDependencyHash
                        {
                            path = "Assets/fake-pipeline.asset", exists = true,
                            byteLength = 1, sha256 = Hash
                        }
                    },
                    testResults = Array.Empty<AuditionPvTestResult>()
                };
                string core = AuditionPvSixtySecondGateManifestValidator
                    .CaptureCoreSha256(capture);
                var tests = new List<AuditionPvTestResult>
                {
                    new AuditionPvTestResult
                    {
                        suite = "fixture", name = "frame-hash-ledger", status = "passed",
                        details = "fixture", artifactPath = ledger.Replace('\\', '/')
                    }
                };
                foreach (AuditionPvShotManifestEntry shot in sourceShots)
                {
                    string directionId = shot.id == "g04-clean" ? "g04" : shot.id;
                    string authorshipPath = Path.Combine(evidence,
                        shot.id + "_shot_authorship.json");
                    var authorship = new AuditionPvShotAuthorshipArtifact
                    {
                        schemaVersion = AuditionPvSixtySecondGateManifestValidator
                            .ShotAuthorshipSchema,
                        sourceCaptureCoreSha256 = core,
                        captureId = captureId,
                        sourceShotId = shot.id,
                        cameraId = "camera-" + directionId,
                        gameplayState = "state-" + directionId,
                        timelineId = "timeline-" + directionId,
                        deterministicSeed = 1000 + directionId.Sum(value => value),
                        runtimeProof = new AuditionPvPinnedArtifact
                            { path = ledger.Replace('\\', '/'), sha256 = AuditionPvSha256.FileHash(ledger) },
                        tool = "fixture", toolVersion = "1",
                        createdAtUtc = "2026-08-17T00:00:00.0000000Z"
                    };
                    File.WriteAllText(authorshipPath, JsonUtility.ToJson(authorship, true),
                        new UTF8Encoding(false));
                    string hash = AuditionPvSha256.FileHash(authorshipPath);
                    tests.Add(new AuditionPvTestResult
                    {
                        suite = "AuditionPvSixtySecondEvidence",
                        name = "shot-authorship/" + shot.id,
                        status = "passed",
                        details = "artifact-sha256=" + hash,
                        artifactPath = authorshipPath.Replace('\\', '/')
                    });
                }
                capture.testResults = tests.ToArray();
                AuditionPvCaptureManifestWriter.Validate(capture);
                string manifestPath = Path.Combine(directory,
                    AuditionPvCaptureContract.ManifestFileName);
                File.WriteAllText(manifestPath, JsonUtility.ToJson(capture, true),
                    new UTF8Encoding(false));
                return manifestPath;
            }

            private AuditionPvPinnedArtifact WritePin(string relative, string contents)
            {
                string path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? root);
                File.WriteAllText(path, contents, new UTF8Encoding(false));
                return new AuditionPvPinnedArtifact
                    { path = path.Replace('\\', '/'), sha256 = AuditionPvSha256.FileHash(path) };
            }

            private static AuditionPvShotManifestEntry Shot(string id, int end,
                string hud, string scene) => new()
            {
                id = id, scenePath = scene, startFrame = 0, endFrame = end,
                expectedFrameCount = end + 1, hudMode = hud, notes = "fixture"
            };

            private static AuditionPvShotManifestEntry CloneShot(
                AuditionPvShotManifestEntry value) => new()
            {
                id = value.id, scenePath = value.scenePath, startFrame = value.startFrame,
                endFrame = value.endFrame, expectedFrameCount = value.expectedFrameCount,
                hudMode = value.hudMode, notes = value.notes
            };

            private static string CityScene() => AuditionPvCityHeroPocketCapture.CityScenePath;
            private static string StationScene() =>
                AuditionPvStationPhase2PatternRelayCapture.StationScenePath;

            private static AuditionPvSixtySecondRightsEvidence[] Rights(
                AuditionPvPinnedArtifact dummy) => new[]
            {
                Right("right-asset", "asset", dummy),
                Right("right-font", "font", dummy),
                Right("right-audio", "audio", dummy),
                Right("right-ai", "ai", dummy)
            };

            private static AuditionPvSixtySecondRightsEvidence Right(string id,
                string scope, AuditionPvPinnedArtifact dummy) => new()
            {
                id = id, scope = scope, record = dummy
            };

            private static AuditionPvSixtySecondUsedItem[] UsedItems(
                AuditionPvPinnedArtifact dummy, AuditionPvPinnedArtifact graphic) => new[]
            {
                Item("item-product", "asset", "right-asset", dummy),
                Item("item-graphic", "asset", "right-asset", graphic),
                Item("item-font", "font", "right-font", dummy),
                Item("item-ai", "ai", "right-ai", dummy),
                Item("item-audio-music", "audio", "right-audio", dummy),
                Item("item-audio-sfx", "audio", "right-audio", dummy),
                Item("item-audio-vo", "audio", "right-audio", dummy),
                Item("item-audio-ambience", "audio", "right-audio", dummy)
            };

            private static AuditionPvSixtySecondUsedItem Item(string id, string scope,
                string right, AuditionPvPinnedArtifact pin) => new()
            {
                id = id, scope = scope, rightsRecordId = right,
                sourceLocator = pin.path, dependencyBinding = "external-artifact",
                artifact = pin
            };

            private static AuditionPvSixtySecondAudioEvidence[] Audio(
                AuditionPvPinnedArtifact dummy) => new[]
            {
                AudioRow("music", new[] { "music-bed" }, dummy),
                AudioRow("ambience", new[] { "city-ambience", "olympus-ambience" }, dummy),
                AudioRow("vo", new[] { "announcement-vo", "inori-vo", "boss-vo" }, dummy),
                AudioRow("sfx", new[]
                {
                    "gun-mechanical", "gun-fire", "gun-tail", "dodge", "summon", "hit",
                    "boss-charge", "boss-fire", "boss-death", "wing-deploy", "eye-open"
                }, dummy)
            };

            private static AuditionPvSixtySecondAudioEvidence AudioRow(string category,
                string[] cues, AuditionPvPinnedArtifact dummy) => new()
            {
                id = "audio-" + category,
                category = category,
                usedItemId = "item-audio-" + category,
                cueIds = cues,
                cueRegions = cues.Select((cue, index) => new AuditionPvAudioCueRegion
                {
                    cueId = cue, startMilliseconds = index * 100,
                    endMilliseconds = index * 100 + 100
                }).ToArray(),
                file = dummy,
                sampleRate = 48000,
                channels = category == "vo" ? 1 : 2,
                generatedByAi = false,
                humanListeningStatus = "pending"
            };

            private static AuditionPvSixtySecondGateEvidence GateEvidence(
                AuditionPvPinnedArtifact dummy) => new()
            {
                twelveSecondPackageDirectory = "selects/12s",
                twelveSecondManifestSha256 = Hash,
                twelveSecondValidationSha256 = Hash,
                twelveSecondApproval = dummy,
                visualReview = dummy,
                rightsCoverageReview = dummy,
                twelveSecondSourceFrameLedgers = Enumerable.Range(0,
                    AuditionPvTwelveSecondGoldAssembler.RequiredRoles.Length).Select(index =>
                    new AuditionPvTwelveSecondSourceFrameLedgerBinding
                    {
                        segmentOrder = index,
                        sourceCaptureId = "source-" + index,
                        sourceManifestSha256 = new string((char)('1' + index), 64),
                        sourceDependencyIdentitySha256 = Hash,
                        sourceShotId = "shot-" + index,
                        frameLedger = dummy
                    }).ToArray()
            };

            public void Dispose() => Delete(root);
        }
    }
}
