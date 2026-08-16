using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    [TestFixture]
    public sealed class AuditionPvSixtySecondGateManifestTests
    {
        private Fixture fixture;

        [SetUp]
        public void SetUp() => fixture = Fixture.Create();

        [TearDown]
        public void TearDown() => fixture?.Dispose();

        [Test]
        public void EmptyPlan_UsesTenAdjustableReadFirstBuckets_AndTruthfullyFailsStructure()
        {
            AuditionPvSixtySecondShotGateManifest plan =
                AuditionPvSixtySecondGateManifestValidator.CreateEmptyPlan();

            Assert.That(plan.buckets, Has.Length.EqualTo(10));
            Assert.That(plan.buckets[0].bucketId, Is.EqualTo("PV_S010"));
            Assert.That(plan.buckets[0].timelineStartFrame, Is.Zero);
            Assert.That(plan.buckets[9].bucketId, Is.EqualTo("PV_S100"));
            Assert.That(plan.buckets[9].timelineEndFrame, Is.EqualTo(3599));
            Assert.That(plan.buckets[6].requiredBeatIds,
                Does.Contain("boss-pattern-1").And.Contain("boss-pattern-2")
                    .And.Contain("boss-pattern-3"));
            Assert.That(plan.buckets[7].requiredBeatIds, Does.Contain("player-tier3-ultimate")
                .And.Not.Contain("boss-finisher"));
            Assert.That(plan.buckets[8].requiredBeatIds, Does.Contain("boss-finisher")
                .And.Not.Contain("player-tier3-ultimate"));

            AuditionPvSixtySecondGateValidationReport report =
                AuditionPvSixtySecondGateManifestValidator.ValidateStructure(plan);

            Assert.That(report.passed, Is.False);
            Assert.That(report.structureValid, Is.False);
            Assert.That(Issues(report), Does.Contain("BUCKET_SHOTS_MISSING"));
            Assert.That(Issues(report), Does.Contain("AUDIO_CATEGORY_EVIDENCE_MISSING"));
            Assert.That(Issues(report), Does.Contain("TWELVE_SECOND_PACKAGE_PATH_MISSING"));
        }

        [Test]
        public void SyntheticCompleteFixture_CanBeStructureValid_ButCanNeverPassTheGate()
        {
            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateStructure();

            Assert.That(report.structureValid, Is.True,
                string.Join("\n", report.issues.Select(value => value.code + ": " + value.message)));
            Assert.That(report.passed, Is.False);
            Assert.That(report.validationMode, Is.EqualTo("structure-only"));
            Assert.That(Issues(report), Does.Contain("STRUCTURE_ONLY_NOT_GATE"));
            Assert.That(report.bucketCount, Is.EqualTo(10));
            Assert.That(report.shotCount, Is.EqualTo(11));
            Assert.That(report.declaredApprovedTakeCount, Is.EqualTo(10));
            Assert.That(report.declaredGraphicPlaceholderCount, Is.EqualTo(1));

            AuditionPvSixtySecondGateValidationReport inMemory = fixture.ValidateProduction();
            Assert.That(inMemory.passed, Is.False);
            Assert.That(inMemory.productionEvidenceVerified, Is.False);
            Assert.That(Issues(inMemory), Does.Contain("IN_MEMORY_MANIFEST_NOT_AUTHORITATIVE"));
        }

        [Test]
        public void ProductionValidation_FakeBytesAndMinimalTwelveSecondJson_CannotPass()
        {
            fixture.MaterializeFakeTwelveSecondPackage();
            string manifestPath = fixture.MaterializeManifestFile();

            AuditionPvSixtySecondGateValidationReport report =
                AuditionPvSixtySecondGateManifestValidator.ValidateProductionFile(
                    manifestPath, fixture.context);

            Assert.That(report.structureValid, Is.True);
            Assert.That(report.passed, Is.False);
            Assert.That(report.validationMode, Is.EqualTo("production"));
            Assert.That(report.productionEvidenceVerified, Is.False);
            Assert.That(report.inputManifestPath, Is.EqualTo(Path.GetFullPath(manifestPath)));
            Assert.That(AuditionPvSha256.IsSha256(report.inputManifestSha256), Is.True);
            Assert.That(Issues(report), Does.Contain("CALLER_CONTEXT_NOT_AUTHORITATIVE"));
            Assert.That(Issues(report), Does.Contain("TWELVE_SECOND_PACKAGE_INVALID"));
            Assert.That(Issues(report), Does.Contain("TAKE_SOURCE_MANIFEST_MISSING"));
        }

        [Test]
        public void ParseableNestedNullPins_FailClosedAsIssuesWithoutThrowing()
        {
            AuditionPvSixtySecondAudioEvidence music = fixture.manifest.audio.Single(value =>
                value.category == "music");
            AuditionPvSixtySecondUsedItem musicItem = fixture.manifest.usedItems.Single(value =>
                value.id == music.usedItemId);
            string wavPath = Path.Combine(fixture.root, "Evidence", "music.wav");
            WritePcm16Wav(wavPath, 1000, 1000, 12000);
            string wavSha = AuditionPvSha256.FileHash(wavPath);
            music.file = new AuditionPvPinnedArtifact
                { path = "Evidence/music.wav", sha256 = wavSha };
            musicItem.artifact = new AuditionPvPinnedArtifact
                { path = "Evidence/music.wav", sha256 = wavSha };

            void AssertMutation(Action mutate, Action restore, string expectedIssue)
            {
                mutate();
                try
                {
                    AuditionPvSixtySecondGateValidationReport report = null;
                    Assert.DoesNotThrow(() =>
                    {
                        AuditionPvSixtySecondShotGateManifest parsed =
                            JsonUtility.FromJson<AuditionPvSixtySecondShotGateManifest>(
                                JsonUtility.ToJson(fixture.manifest));
                        report = AuditionPvSixtySecondGateManifestValidator.ValidateProduction(
                            parsed, fixture.context);
                    });
                    Assert.That(report, Is.Not.Null);
                    Assert.That(Issues(report), Does.Contain(expectedIssue));
                    Assert.That(report.passed, Is.False);
                }
                finally { restore(); }
            }

            AuditionPvPinnedArtifact musicItemArtifact = musicItem.artifact;
            AssertMutation(() => musicItem.artifact = null,
                () => musicItem.artifact = musicItemArtifact, "USED_ITEM_ARTIFACT_PIN_MISSING");

            AuditionPvPinnedArtifact musicFile = music.file;
            AssertMutation(() => music.file = null, () => music.file = musicFile,
                "AUDIO_FILE_PIN_MISSING");

            AuditionPvSixtySecondUsedItem productItem = fixture.manifest.usedItems.Single(value =>
                value.id == "item-product-asset");
            AuditionPvPinnedArtifact productArtifact = productItem.artifact;
            AssertMutation(() => productItem.artifact = null,
                () => productItem.artifact = productArtifact, "USED_ITEM_ARTIFACT_PIN_MISSING");

            AuditionPvPinnedArtifact visualReview = fixture.manifest.gateEvidence.visualReview;
            AssertMutation(() => fixture.manifest.gateEvidence.visualReview = null,
                () => fixture.manifest.gateEvidence.visualReview = visualReview,
                "VISUAL_REVIEW_PIN_MISSING");
        }

        [Test]
        public void ReportMetrics_AreDeclaredAndDoNotCountNullTakeSlots()
        {
            int expectedSlots = fixture.manifest.buckets.Where(bucket => bucket != null)
                .SelectMany(bucket => bucket.shots ?? Array.Empty<AuditionPvSixtySecondAtomicShot>())
                .Where(shot => shot != null)
                .Sum(shot => (shot.candidateTakes ??
                    Array.Empty<AuditionPvSixtySecondTakeCandidate>()).Count(take =>
                    take != null && !string.IsNullOrWhiteSpace(take.takeId)));
            AuditionPvSixtySecondGateValidationReport initial = fixture.ValidateStructure();
            Assert.That(initial.declaredTakeSlotCount, Is.EqualTo(expectedSlots));
            Assert.That(initial.declaredApprovedTakeCount, Is.EqualTo(10));
            Assert.That(initial.declaredGraphicPlaceholderCount, Is.EqualTo(1));

            AuditionPvSixtySecondAtomicShot core = fixture.manifest.buckets[1].shots[0];
            Assert.That(core.candidateTakes[1].takeId, Is.Not.EqualTo(core.approvedTakeId));
            core.candidateTakes[1] = null;

            AuditionPvSixtySecondGateValidationReport withNull = fixture.ValidateStructure();
            Assert.That(withNull.structureValid, Is.False);
            Assert.That(withNull.declaredTakeSlotCount, Is.EqualTo(expectedSlots - 1));
            Assert.That(withNull.declaredApprovedTakeCount, Is.EqualTo(10));
            Assert.That(withNull.declaredGraphicPlaceholderCount, Is.EqualTo(1));
        }

        [Test]
        public void BucketBoundariesMayMove_WhenBucketsAndAtomicShotsRemainContiguous()
        {
            ResizeSingleShotBucket(fixture.manifest.buckets[0], 0, 251);
            ResizeSingleShotBucket(fixture.manifest.buckets[1], 252, 599);

            Assert.That(fixture.ValidateStructure().structureValid, Is.True);

            fixture.manifest.buckets[1].timelineStartFrame++;
            AuditionPvSixtySecondGateValidationReport gap = fixture.ValidateStructure();
            Assert.That(gap.structureValid, Is.False);
            Assert.That(Issues(gap), Does.Contain("BUCKET_TIMELINE_NOT_CONTIGUOUS_60S"));
        }

        [Test]
        public void CoreTakeIdentity_UsesDistinctCaptureInvocations_NotSourceShotOrRangeAliases()
        {
            AuditionPvSixtySecondAtomicShot shot = fixture.manifest.buckets[1].shots[0];
            AuditionPvSixtySecondTakeCandidate first = shot.candidateTakes[0];
            foreach (AuditionPvSixtySecondTakeCandidate take in shot.candidateTakes)
            {
                take.sourceCaptureId = first.sourceCaptureId;
                take.sourceCaptureCoreSha256 = first.sourceCaptureCoreSha256;
            }
            Assert.That(shot.candidateTakes.Select(value => value.sourceManifestSha256)
                .Distinct(StringComparer.Ordinal).Count(), Is.GreaterThan(1),
                "Post-capture testResults may produce different full-manifest hashes.");
            AuditionPvSixtySecondGateValidationReport sameInvocation = fixture.ValidateStructure();
            Assert.That(sameInvocation.structureValid, Is.False,
                "Same core+capture invocation cannot inflate editorial takes despite distinct full manifests/shot IDs.");
            Assert.That(Issues(sameInvocation),
                Does.Contain("SHOT_CAPTURE_CANDIDATE_COUNT_INSUFFICIENT"));

            shot.candidateTakes[1].sourceShotId = first.sourceShotId;
            AuditionPvSixtySecondGateValidationReport alias = fixture.ValidateStructure();
            Assert.That(alias.structureValid, Is.False);
            Assert.That(Issues(alias), Does.Contain("SHOT_CAPTURE_CANDIDATES_NOT_DISTINCT"));
        }

        [Test]
        public void FiveSecondGameplayProof_RequiresExactBeatGameplayKindAndApprovedHudOnTake()
        {
            AuditionPvSixtySecondAtomicShot city = fixture.manifest.buckets[1].shots[0];
            city.sourceKind = "cinematic";
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("CITY_CONTINUOUS_HUD_GAMEPLAY_MISSING"));

            city.sourceKind = "gameplay";
            city.candidateTakes[0].declaredHudMode = "hud-off";
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("CITY_CONTINUOUS_HUD_GAMEPLAY_MISSING"));
        }

        [Test]
        public void SemanticBeatProofs_AreExactPerBeatFacts_EvenWhenRuntimeArtifactIsShared()
        {
            AuditionPvPinnedArtifact shared = Pin("runtime.json", 'a');
            var values = new[]
            {
                BeatProof("player-tier3-ultimate", shared), BeatProof("boss-finisher", shared)
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.SemanticBeatProofSetValid(
                values, new[] { "player-tier3-ultimate", "boss-finisher" }), Is.True);

            values[1].runtimeFactKey = "player-tier3-ultimate";
            Assert.That(AuditionPvSixtySecondGateManifestValidator.SemanticBeatProofSetValid(
                values, new[] { "player-tier3-ultimate", "boss-finisher" }), Is.False);
        }

        [Test]
        public void DetachedShotAuthorship_IsPhysicalAcyclicAndExactToDirectorMetadata()
        {
            AuditionPvSixtySecondAtomicShot shot = fixture.manifest.buckets[1].shots[0];
            AuditionPvSixtySecondTakeCandidate take = shot.candidateTakes[0];
            string runtimePath = Path.Combine(fixture.root, "runtime-proof.json");
            File.WriteAllText(runtimePath, "{\"captured\":true}", new UTF8Encoding(false));
            var runtimePin = new AuditionPvPinnedArtifact
                { path = runtimePath, sha256 = AuditionPvSha256.FileHash(runtimePath) };
            var authorship = new AuditionPvShotAuthorshipArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.ShotAuthorshipSchema,
                sourceCaptureCoreSha256 = take.sourceCaptureCoreSha256,
                captureId = take.sourceCaptureId, sourceShotId = take.sourceShotId,
                cameraId = take.cameraId, gameplayState = take.gameplayState,
                deterministicSeed = take.deterministicSeed, timelineId = take.timelineId,
                runtimeProof = runtimePin,
                tool = "capture-authorship", toolVersion = "1",
                createdAtUtc = "2026-08-17T00:00:00Z"
            };
            string path = Path.Combine(fixture.root, "shot-authorship.json");
            File.WriteAllText(path, JsonUtility.ToJson(authorship, true), new UTF8Encoding(false));
            var pin = new AuditionPvPinnedArtifact
                { path = path, sha256 = AuditionPvSha256.FileHash(path) };
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .ShotAuthorshipFileIdentityValidForTest(path, pin, shot, take,
                    take.sourceCaptureCoreSha256), Is.True);
            var capture = new AuditionPvCaptureManifest
            {
                outputDirectory = fixture.root,
                testResults = new[]
                {
                    new AuditionPvTestResult
                    {
                        suite = "AuditionPvSixtySecondEvidence",
                        name = "shot-authorship/" + take.sourceShotId, status = "passed",
                        artifactPath = path, details = "artifact-sha256=" + pin.sha256
                    },
                    new AuditionPvTestResult
                    {
                        suite = "AuditionPvSixtySecondEvidence",
                        name = "shot-authorship-runtime/" + take.sourceShotId, status = "passed",
                        artifactPath = runtimePath,
                        details = "artifact-sha256=" + runtimePin.sha256
                    }
                }
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .CaptureTestArtifactMatchesForTest(capture, "AuditionPvSixtySecondEvidence",
                    "shot-authorship/" + take.sourceShotId, path, pin.sha256), Is.True);
            capture.testResults[0].name = "shot-authorship/unrelated-shot";
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .CaptureTestArtifactMatchesForTest(capture, "AuditionPvSixtySecondEvidence",
                    "shot-authorship/" + take.sourceShotId, path, pin.sha256), Is.False);

            authorship.cameraId = "unrelated-camera";
            File.WriteAllText(path, JsonUtility.ToJson(authorship, true), new UTF8Encoding(false));
            pin.sha256 = AuditionPvSha256.FileHash(path);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .ShotAuthorshipFileIdentityValidForTest(path, pin, shot, take,
                    take.sourceCaptureCoreSha256), Is.False);

            take.shotAuthorship = new AuditionPvPinnedArtifact();
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("TAKE_SHOT_AUTHORSHIP_PIN_MISSING"));
        }

        [Test]
        public void CoreShot_TwoEditorialCandidatesPlusCleanPlate_DoesNotSatisfyThreeTakes()
        {
            AuditionPvSixtySecondAtomicShot shot = fixture.manifest.buckets[1].shots[0];
            AuditionPvSixtySecondTakeCandidate clean = shot.candidateTakes[3];
            shot.candidateTakes = new[]
            {
                shot.candidateTakes[0], shot.candidateTakes[1], clean
            };
            shot.cleanPlateTakeId = clean.takeId;

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateStructure();

            Assert.That(report.structureValid, Is.False);
            Assert.That(Issues(report), Does.Contain("SHOT_CAPTURE_CANDIDATE_COUNT_INSUFFICIENT"));
        }

        [Test]
        public void BossLowAnglePlanningRole_IsNonCoreAndNeedsOnlyOneEligibleTake()
        {
            AuditionPvSixtySecondAtomicShot shot = fixture.manifest.buckets
                .Single(bucket => bucket.bucketId == "PV_S050").shots.Single();
            Assert.That(shot.beatIds, Does.Contain("boss-low-angle"));
            Assert.That(shot.coreShot, Is.False);
            Assert.That(shot.candidateTakes, Has.Length.EqualTo(1));
            Assert.That(fixture.ValidateStructure().structureValid, Is.True);
        }

        [Test]
        public void EndCardPlaceholderAndPendingWording_AreTruthfulEditStartStates()
        {
            AuditionPvSixtySecondAtomicShot endCard = fixture.manifest.buckets
                .Single(bucket => bucket.bucketId == "PV_S100").shots.Single();
            Assert.That(endCard.graphicProductionStatus, Is.EqualTo("layout-placeholder-approved"));
            Assert.That(endCard.sloganApprovalStatus, Is.EqualTo("pending-approval"));
            Assert.That(fixture.ValidateStructure().structureValid, Is.True);

            endCard.sloganApprovalStatus = string.Empty;
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("SHOT_END_CARD_GRAPHIC_ID_INVALID"));
        }

        [Test]
        public void EndCardPlanningRequiresPhysicalQhdPlaceholder_ButFinalAeWordingRemainsWarningOnly()
        {
            AuditionPvSixtySecondAtomicShot endCard = fixture.manifest.buckets
                .Single(bucket => bucket.bucketId == "PV_S100").shots.Single();
            string path = Path.Combine(fixture.root, "Evidence", "end-card.png");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            WriteSolidPng(path, 2560, 1440, new Color32(16, 24, 32, 255));
            string hash = AuditionPvSha256.FileHash(path);
            endCard.graphicArtifact = new AuditionPvPinnedArtifact
                { path = "Evidence/end-card.png", sha256 = hash };
            fixture.manifest.usedItems.Single(value => value.id == "item-endcard-graphic")
                .artifact = new AuditionPvPinnedArtifact
                    { path = "Evidence/end-card.png", sha256 = hash };

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateProduction();

            Assert.That(Issues(report), Does.Not.Contain("SHOT_END_CARD_LAYOUT_PLACEHOLDER_INVALID"));
            Assert.That(Issues(report), Does.Contain("SHOT_END_CARD_FINAL_GRAPHIC_PENDING"));
            Assert.That(Issues(report), Does.Contain("SHOT_END_CARD_WORDING_APPROVAL_PENDING"));

            WriteSolidPng(path, 1920, 1080, new Color32(16, 24, 32, 255));
            hash = AuditionPvSha256.FileHash(path);
            endCard.graphicArtifact.sha256 = hash;
            fixture.manifest.usedItems.Single(value => value.id == "item-endcard-graphic")
                .artifact.sha256 = hash;
            Assert.That(Issues(fixture.ValidateProduction()),
                Does.Contain("SHOT_END_CARD_LAYOUT_PLACEHOLDER_INVALID"),
                "A 1080p image is not the physical QHD planning placeholder.");

            File.WriteAllText(path, "not a PNG", new UTF8Encoding(false));
            hash = AuditionPvSha256.FileHash(path);
            endCard.graphicArtifact.sha256 = hash;
            fixture.manifest.usedItems.Single(value => value.id == "item-endcard-graphic")
                .artifact.sha256 = hash;
            Assert.That(Issues(fixture.ValidateProduction()),
                Does.Contain("SHOT_END_CARD_LAYOUT_PLACEHOLDER_INVALID"),
                "Arbitrary bytes with a current pin are not a graphic placeholder.");

            WriteSolidPng(path, 2560, 1440, new Color32(16, 24, 32, 255));
            hash = AuditionPvSha256.FileHash(path);
            fixture.manifest.usedItems.Single(value => value.id == "item-endcard-graphic")
                .artifact.sha256 = hash;
            endCard.graphicArtifact.sha256 = Sha('f');
            Assert.That(Issues(fixture.ValidateProduction()),
                Does.Contain("SHOT_END_CARD_GRAPHIC_DRIFT"),
                "A physical QHD placeholder whose bytes drift from its pin is rejected.");
        }

        [Test]
        public void ApprovedTakeAndCleanPlateCompanion_AreExplicitSingleCandidateReferences()
        {
            AuditionPvSixtySecondAtomicShot core = fixture.manifest.buckets[2].shots[0];
            core.approvedTakeId = "missing-take";
            core.cleanPlateTakeId = core.candidateTakes[1].takeId;

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateStructure();

            Assert.That(report.structureValid, Is.False);
            Assert.That(Issues(report), Does.Contain("SHOT_APPROVED_TAKE_INVALID"));
            Assert.That(Issues(report), Does.Contain("SHOT_CLEAN_PLATE_TAKE_INVALID"));
            Assert.That(Issues(report), Does.Contain("GAMEPLAY_CINEMATIC_CLEAN_PLATE_MISSING"));
        }

        [Test]
        public void ExpensiveAutomatedAndHumanProofs_ApplyOnlyToApprovedAndLinkedCleanInputs()
        {
            AuditionPvSixtySecondAtomicShot shot = fixture.manifest.buckets[2].shots[0];
            AuditionPvSixtySecondTakeCandidate approved = shot.candidateTakes[0];
            AuditionPvSixtySecondTakeCandidate alternate = shot.candidateTakes[1];
            AuditionPvSixtySecondTakeCandidate clean = shot.candidateTakes[3];
            alternate.automatedProof = new AuditionPvPinnedArtifact();
            alternate.humanReview = new AuditionPvPinnedArtifact();
            Assert.That(fixture.ValidateStructure().structureValid, Is.True,
                "Editorial alternates are selection coverage, not duplicated scan/color review inputs.");

            AuditionPvPinnedArtifact approvedAutomated = approved.automatedProof;
            approved.automatedProof = new AuditionPvPinnedArtifact();
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("TAKE_AUTOMATED_PROOF_PIN_MISSING"));
            approved.automatedProof = approvedAutomated;

            AuditionPvPinnedArtifact approvedReview = approved.humanReview;
            approved.humanReview = new AuditionPvPinnedArtifact();
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("TAKE_HUMAN_REVIEW_PIN_MISSING"));
            approved.humanReview = approvedReview;

            AuditionPvPinnedArtifact cleanAutomated = clean.automatedProof;
            clean.automatedProof = new AuditionPvPinnedArtifact();
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("TAKE_AUTOMATED_PROOF_PIN_MISSING"));
            clean.automatedProof = cleanAutomated;

            clean.humanReview = new AuditionPvPinnedArtifact();
            Assert.That(Issues(fixture.ValidateStructure()),
                Does.Contain("TAKE_HUMAN_REVIEW_PIN_MISSING"),
                "The linked clean plate also needs full-motion handle review for black meshes/trails.");
        }

        [Test]
        public void CandidateSelectLengthAndThreeToFiveSecondHandles_AreStructuralContract()
        {
            AuditionPvSixtySecondTakeCandidate take =
                fixture.manifest.buckets[1].shots[0].candidateTakes[0];
            take.selectEndFrame--;
            take.sourceRangeStartFrame = 1;

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateStructure();

            Assert.That(report.structureValid, Is.False);
            Assert.That(Issues(report), Does.Contain("TAKE_SELECT_TIMELINE_LENGTH_MISMATCH"));
            Assert.That(Issues(report), Does.Contain("TAKE_HANDLE_ARITHMETIC_INVALID"));
        }

        [Test]
        public void CandidatePhysicalFrames_AreOnlySelectPlusDeclaredHandles_NotWholeSourceShot()
        {
            var take = new AuditionPvSixtySecondTakeCandidate
                { sourceRangeStartFrame = 180, sourceRangeEndFrame = 183 };
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .CandidateSourceFramesForTest(take), Is.EqualTo(new[] { 180, 181, 182, 183 }));
        }

        [Test]
        public void ApprovedAndCleanFullMotionReviewSampling_IncludesBothHandleEndpoints()
        {
            var take = new AuditionPvSixtySecondTakeCandidate
            {
                sourceRangeStartFrame = 0, selectStartFrame = 180,
                selectEndFrame = 240, sourceRangeEndFrame = 420
            };
            int[] reviewed = AuditionPvSixtySecondGateManifestValidator
                .RequiredHumanReviewFramesForTest(take);

            Assert.That(reviewed.First(), Is.EqualTo(take.sourceRangeStartFrame));
            Assert.That(reviewed.Last(), Is.EqualTo(take.sourceRangeEndFrame));
            Assert.That(reviewed.Any(frame => frame >= take.selectStartFrame &&
                                              frame <= take.selectEndFrame), Is.True);
            Assert.That(reviewed.Any(frame => frame < take.selectStartFrame), Is.True);
            Assert.That(reviewed.Any(frame => frame > take.selectEndFrame), Is.True);
        }

        [Test]
        public void AllFourAudioStemsAreRequired_ButHumanListeningIsNotSelfAttestedGateProof()
        {
            fixture.manifest.audio = fixture.manifest.audio
                .Where(value => value.category != "ambience").ToArray();
            fixture.manifest.usedItems = fixture.manifest.usedItems
                .Where(value => value.id != "item-audio-ambience").ToArray();

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateStructure();

            Assert.That(report.structureValid, Is.False);
            Assert.That(Issues(report), Does.Contain("AUDIO_CATEGORY_EVIDENCE_MISSING"));
            Assert.That(report.issues.Single(value => value.code == "AUDIO_CATEGORY_EVIDENCE_MISSING").message,
                Is.EqualTo("ambience"));
        }

        [Test]
        public void AudioCueMarkersMayOverlap_ButEachMustRemainPositiveAndSignalBound()
        {
            AuditionPvSixtySecondAudioEvidence sfx = fixture.manifest.audio
                .Single(value => value.category == "sfx");
            sfx.cueRegions[1].startMilliseconds = sfx.cueRegions[0].startMilliseconds;
            sfx.cueRegions[1].endMilliseconds = sfx.cueRegions[0].endMilliseconds;
            Assert.That(fixture.ValidateStructure().structureValid, Is.True,
                "Music tails, ambience, and layered effects may legitimately overlap.");

            sfx.cueRegions[1].endMilliseconds = sfx.cueRegions[1].startMilliseconds;
            Assert.That(Issues(fixture.ValidateStructure()), Does.Contain("AUDIO_CUE_REGION_INVALID"));
        }

        [Test]
        public void RightsMetadata_IsDispositionConditional_NotUniversalCommerceFields()
        {
            var authored = new AuditionPvRightsRecordArtifact
            {
                verified = true, verifiedBy = "owner", verifiedAtUtc = "2026-08-17T00:00:00Z",
                disposition = "project-authored", useBoundary = "submission",
                owner = "Dimension Brawl team", sourceDescription = "project-authored logo bytes"
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(authored),
                Is.True);
            authored.provider = "invented-commerce-provider";
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(authored),
                Is.False);

            var purchased = new AuditionPvRightsRecordArtifact
            {
                verified = true, verifiedBy = "reviewer", verifiedAtUtc = "2026-08-17T00:00:00Z",
                disposition = "purchased", useBoundary = "submission", provider = "store",
                licenseId = "license", licenseVersion = "1", accountEntitlementId = "receipt",
                termsSnapshot = Pin("terms.txt", 'a'), entitlementEvidence = Pin("receipt.txt", 'b')
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(purchased),
                Is.True);
            purchased.entitlementEvidence = new AuditionPvPinnedArtifact();
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(purchased),
                Is.False);

            var open = new AuditionPvRightsRecordArtifact
            {
                verified = true, verifiedBy = "reviewer", verifiedAtUtc = "2026-08-17T00:00:00Z",
                disposition = "open-license", useBoundary = "submission", provider = "author",
                licenseId = "CC-BY", licenseVersion = "4.0", attributionRequired = true,
                termsSnapshot = Pin("cc-by.txt", 'c'), attributionArtifact = Pin("credits.txt", 'd')
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(open),
                Is.True);
            open.accountEntitlementId = "invented-store-account";
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(open),
                Is.False);

            var ai = new AuditionPvRightsRecordArtifact
            {
                verified = true, verifiedBy = "reviewer", verifiedAtUtc = "2026-08-17T00:00:00Z",
                disposition = "ai-generated", useBoundary = "submission", provider = "provider",
                accountPlan = "commercial-plan", termsSnapshot = Pin("ai-terms.txt", 'e'),
                generationEvidence = Pin("generation.json", 'f')
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(ai),
                Is.True);
            ai.generationEvidence = new AuditionPvPinnedArtifact();
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RightsRecordMetadataValid(ai),
                Is.False);
        }

        [Test]
        public void PendingListeningIsAnExplicitHold_ButRejectedRawAudioIsAProductionError()
        {
            AuditionPvSixtySecondGateValidationReport pending = fixture.ValidateProduction();
            Assert.That(Issues(pending), Does.Contain("AUDIO_HUMAN_LISTENING_PENDING"));

            AuditionPvSixtySecondAudioEvidence rejected = fixture.manifest.audio[0];
            rejected.humanListeningStatus = "rejected";
            rejected.listeningReport = Pin("Evidence/rejected-audio-review.json", 'f');

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateProduction();

            Assert.That(report.passed, Is.False);
            Assert.That(Issues(report), Does.Contain("AUDIO_HUMAN_LISTENING_REJECTED"));
        }

        [Test]
        public void AudioCueRegionsAndCleanPlateCompanionProof_AreRequiredStructurally()
        {
            fixture.manifest.audio[0].cueRegions = Array.Empty<AuditionPvAudioCueRegion>();
            AuditionPvSixtySecondAtomicShot cleanShot = fixture.manifest.buckets[2].shots[0];
            cleanShot.candidateTakes[3].cleanPlateProof = new AuditionPvPinnedArtifact();

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateStructure();

            Assert.That(report.structureValid, Is.False);
            Assert.That(Issues(report), Does.Contain("AUDIO_CUE_REGION_INVALID"));
            Assert.That(Issues(report), Does.Contain("TAKE_CLEAN_PLATE_PROOF_PIN_MISSING"));
        }

        [Test]
        public void AudioCueRegionArithmetic_IsOverflowSafe()
        {
            Assert.That(AuditionPvSixtySecondGateManifestValidator.CueRegionShapeValid(
                new AuditionPvAudioCueRegion
                {
                    cueId = "music-bed", startMilliseconds = int.MaxValue - 50,
                    endMilliseconds = int.MinValue
                }), Is.False);
        }

        [Test]
        public void AudioCueRegion_MustContainSustainedAudiblePcm_NotSilenceNoiseOrShortBlip()
        {
            string wav = Path.Combine(fixture.root, "cue.wav");
            var region = new[]
            {
                new AuditionPvAudioCueRegion
                    { cueId = "music-bed", startMilliseconds = 0, endMilliseconds = 1000 }
            };
            WritePcm16Wav(wav, 1000, 0, 0);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.WaveCueRegionsHaveSignal(
                wav, region), Is.False);
            WritePcm16Wav(wav, 1000, 1000, 1);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.WaveCueRegionsHaveSignal(
                wav, region), Is.False, "Near-zero PCM noise is not meaningful cue material.");
            WritePcm16Wav(wav, 1000, 5, 12000);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.WaveCueRegionsHaveSignal(
                wav, region), Is.False, "A sub-threshold blip cannot close a long cue region.");
            WritePcm16Wav(wav, 1000, 20, 12000);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.WaveCueRegionsHaveSignal(
                wav, region), Is.True);
            region[0].endMilliseconds = 1001;
            Assert.That(AuditionPvSixtySecondGateManifestValidator.WaveCueRegionsHaveSignal(
                wav, region), Is.False, "Cue regions must remain inside the measured PCM duration.");
        }

        [Test]
        public void AiAudioGenerationV2_BindsFinalFileExactAiItemRightsAndConsentDisposition()
        {
            AuditionPvSixtySecondAudioEvidence audio = fixture.manifest.audio
                .Single(value => value.generatedByAi);
            AuditionPvSixtySecondUsedItem aiItem = fixture.manifest.usedItems
                .Single(value => value.id == audio.aiUsedItemId);
            var generation = new AuditionPvAudioGenerationArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.AudioGenerationSchema,
                audioId = audio.id, aiUsedItemId = aiItem.id,
                rightsRecordId = aiItem.rightsRecordId, provider = "provider", model = "model",
                tool = "generator", toolVersion = "2", accountPlan = "licensed",
                generatedAtUtc = "2026-08-17T00:00:00Z",
                voiceIdentityDisposition = "non-real-person-imitation",
                promptArtifact = Pin("Evidence/prompt.txt", '1'),
                originalGeneratedWav = Pin("Evidence/original.wav", '2'),
                editedWav = audio.file,
                derivationRecipe = Pin("Evidence/recipe.json", '3')
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.AudioGenerationIdentityValid(
                generation, audio, aiItem), Is.True);

            aiItem.artifact = Pin("Evidence/unrelated-ai.json", '7');
            Assert.That(AuditionPvSixtySecondGateManifestValidator.AudioGenerationIdentityValid(
                generation, audio, aiItem), Is.False);
        }

        [Test]
        public void ProductionRelativeTraversal_IsRejectedEvenWhenThePinShapeIsValid()
        {
            AuditionPvSixtySecondUsedItem item = fixture.manifest.usedItems.Single(value =>
                value.id == "item-endcard-graphic");
            item.artifact.path = "../outside.bin";
            fixture.manifest.buckets.Single(value => value.bucketId == "PV_S100")
                .shots.Single().graphicArtifact.path = "../outside.bin";

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateProduction();

            Assert.That(report.passed, Is.False);
            Assert.That(Issues(report), Does.Contain("USED_ITEM_ARTIFACT_PATH_INVALID"));
        }

        [Test]
        public void ProductionContext_RejectsDirtyCurrentProductState()
        {
            fixture.context.currentGitClean = false;

            AuditionPvSixtySecondGateValidationReport report = fixture.ValidateProduction();

            Assert.That(report.passed, Is.False);
            Assert.That(Issues(report), Does.Contain("CONTEXT_CURRENT_GIT_DIRTY"));
        }

        [Test]
        public void HistoricalCleanCaptureShaIsValidWhileCurrentAssetsAndPackagesBytesMustMatch()
        {
            AuditionPvCaptureManifest capture = CaptureCoreFixture();
            var take = new AuditionPvSixtySecondTakeCandidate
                { sourceCaptureId = capture.captureId, gitCommitSha = capture.gitCommitSha };
            fixture.context.currentGitCommitSha = new string('f', 40);
            Assert.That(capture.gitCommitSha, Is.Not.EqualTo(fixture.context.currentGitCommitSha));
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .CaptureRecordedCleanIdentityValidForTest(capture, take), Is.True,
                "A recorded clean historical source SHA does not need to equal validator HEAD.");

            string assets = Path.Combine(fixture.root, "Assets", "dependency.bin");
            string packages = Path.Combine(fixture.root, "Packages", "dependency.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(assets));
            Directory.CreateDirectory(Path.GetDirectoryName(packages));
            File.WriteAllBytes(assets, new byte[] { 1, 2, 3 });
            File.WriteAllBytes(packages, new byte[] { 4, 5, 6 });
            string assetsSha = AuditionPvSha256.FileHash(assets);
            string packagesSha = AuditionPvSha256.FileHash(packages);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DependencyBytesMatchForTest(
                "Assets/dependency.bin", fixture.context, 3, assetsSha), Is.True);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DependencyBytesMatchForTest(
                "Packages/dependency.bin", fixture.context, 3, packagesSha), Is.True);

            File.WriteAllBytes(assets, new byte[] { 1, 2, 4 });
            File.WriteAllBytes(packages, new byte[] { 4, 5, 7 });
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DependencyBytesMatchForTest(
                "Assets/dependency.bin", fixture.context, 3, assetsSha), Is.False);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DependencyBytesMatchForTest(
                "Packages/dependency.bin", fixture.context, 3, packagesSha), Is.False);
        }

        [Test]
        public void FinalSnapshot_RehashDetectsEvidenceChangedAfterInitialValidation()
        {
            string path = Path.Combine(fixture.root, "final-snapshot.bin");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            var snapshot = new AuditionPvValidationFileSnapshot
            {
                path = path, length = 3, sha256 = AuditionPvSha256.FileHash(path)
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FinalSnapshotFileMatches(
                snapshot), Is.True);

            File.WriteAllBytes(path, new byte[] { 1, 2, 4 });
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FinalSnapshotFileMatches(
                snapshot), Is.False);
        }

        [Test]
        public void CaptureCoreDigest_IsAcyclicAndExcludesOnlyPostCaptureTestResults()
        {
            AuditionPvCaptureManifest capture = CaptureCoreFixture();
            string core = AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(capture);
            Assert.That(AuditionPvSha256.IsSha256(core), Is.True);
            Assert.That(core, Is.EqualTo(
                "232ed621df6e8eb9a204871424226315c9ba4a6a01911ac2f4308f234714061f"),
                "Independent 698-byte big-endian/length-prefixed capture-core golden vector drifted.");
            string coreJson = JsonUtility.ToJson(capture, true);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(
                JsonUtility.FromJson<AuditionPvCaptureManifest>(coreJson)), Is.EqualTo(core));
            Action<AuditionPvCaptureManifest>[] mutations =
            {
                value => value.createdAtUtc = "2026-08-17T00:00:01Z",
                value => value.outputDirectory += "-mutated",
                value => value.shots[0].notes = "mutated-note",
                value => value.baselines[0].status = "rejected",
                value => value.dependencyHashes[0].sha256 = Sha('e')
            };
            foreach (Action<AuditionPvCaptureManifest> mutate in mutations)
            {
                AuditionPvCaptureManifest changed =
                    JsonUtility.FromJson<AuditionPvCaptureManifest>(coreJson);
                mutate(changed);
                Assert.That(AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(changed),
                    Is.Not.EqualTo(core));
            }

            string resultPath = Path.Combine(fixture.root, "acyclic-result.json");
            var result = new AuditionPvAutomatedCheckResultArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.AutomatedCheckResultSchema,
                id = "resolution", captureId = capture.captureId,
                sourceCaptureCoreSha256 = core, sourceShotId = capture.shots[0].id
            };
            File.WriteAllText(resultPath, JsonUtility.ToJson(result, true), new UTF8Encoding(false));
            string resultSha = AuditionPvSha256.FileHash(resultPath);
            capture.testResults = new[]
            {
                new AuditionPvTestResult
                {
                    suite = "AuditionPvSixtySecondEvidence", name = "resolution", status = "passed",
                    artifactPath = resultPath, details = "artifact-sha256=" + resultSha
                }
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(capture),
                Is.EqualTo(core), "Adding the result hash to testResults must not change capture core.");
            string capturePath = Path.Combine(fixture.root, "acyclic-capture-manifest.json");
            File.WriteAllText(capturePath, JsonUtility.ToJson(capture, true), new UTF8Encoding(false));
            Assert.That(AuditionPvSixtySecondGateManifestValidator.CaptureCoreIdentityMatches(
                capture, AuditionPvSha256.FileHash(capturePath)), Is.False,
                "A full manifest hash is not an accepted substitute for the acyclic core digest.");

            capture.testResults[0].details = "mutated-post-capture-details";
            Assert.That(AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(capture),
                Is.EqualTo(core));
            capture.dependencyHashes[0].sha256 = Sha('f');
            Assert.That(AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(capture),
                Is.Not.EqualTo(core));
        }

        [Test]
        public void MaterializedPrimitiveAcyclicReader_HasNoErrorsButIsNotAProductionPositive()
        {
            string outputDirectory = Path.Combine(fixture.root, "hermetic-capture");
            Directory.CreateDirectory(outputDirectory);
            AuditionPvCaptureManifest capture = CaptureCoreFixture();
            capture.outputRoot = fixture.root.Replace('\\', '/');
            capture.outputDirectory = outputDirectory.Replace('\\', '/');
            string core = AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(capture);
            string sourcePath = Path.Combine(outputDirectory, "source.png");
            string outputPath = Path.Combine(outputDirectory, "output.png");
            var source = new[] { new Color32(10, 64, 128, 7), new Color32(192, 255, 0, 231) };
            var output = source.Select(pixel => new Color32(
                AuditionPvSixtySecondGateManifestValidator.TransformSrgb8ToRec709(pixel.r),
                AuditionPvSixtySecondGateManifestValidator.TransformSrgb8ToRec709(pixel.g),
                AuditionPvSixtySecondGateManifestValidator.TransformSrgb8ToRec709(pixel.b),
                pixel.a)).ToArray();
            WritePixelsPng(sourcePath, 2, 1, source);
            WritePixelsPng(outputPath, 2, 1, output);
            string resultPath = Path.Combine(outputDirectory, "rec709-result.json");
            var result = new AuditionPvAutomatedCheckResultArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.AutomatedCheckResultSchema,
                id = "rec709", captureId = capture.captureId,
                sourceCaptureCoreSha256 = core, sourceShotId = capture.shots[0].id,
                measuredWidth = 2, measuredHeight = 1, detectedPixelCount = 0,
                sourceMediaArtifact = new AuditionPvPinnedArtifact
                    { path = sourcePath, sha256 = AuditionPvSha256.FileHash(sourcePath) },
                outputMediaArtifact = new AuditionPvPinnedArtifact
                    { path = outputPath, sha256 = AuditionPvSha256.FileHash(outputPath) }
            };
            File.WriteAllText(resultPath, JsonUtility.ToJson(result, true), new UTF8Encoding(false));
            string resultSha = AuditionPvSha256.FileHash(resultPath);
            capture.testResults = new[]
            {
                new AuditionPvTestResult
                {
                    suite = "AuditionPvSixtySecondEvidence", name = "rec709", status = "passed",
                    artifactPath = resultPath, details = "artifact-sha256=" + resultSha
                }
            };

            AuditionPvSixtySecondGateValidationReport report =
                AuditionPvSixtySecondGateManifestValidator
                    .ValidateHermeticAcyclicEvidenceReaderFixture(
                        capture, resultPath, sourcePath, outputPath);
            Assert.That(report.errorCount, Is.Zero,
                string.Join("\n", report.issues.Select(value => value.code + ": " + value.message)));
            Assert.That(report.passed, Is.False);
            Assert.That(report.productionEvidenceVerified, Is.False);
            Assert.That(report.issues.Select(value => value.code),
                Is.EqualTo(new[] { "CALLER_CONTEXT_NOT_AUTHORITATIVE" }));
        }

        [Test]
        public void Rec709StringsWithoutPinnedParserTransformAndExactOutput_AreRejected()
        {
            var result = new AuditionPvAutomatedCheckResultArtifact
            {
                colorPrimaries = "bt709", transferCharacteristics = "bt709",
                matrixCoefficients = "identity-rgb", signalRange = "full",
                transformId = "srgb8-to-bt709-oetf-rgba8-v1",
                parserName = "png-color-sidecar", parserVersion = "1",
                rec709Config = Pin("rec709-config.json", 'b'),
                rec709OutputLedger = Pin("rec709-output-ledger.json", 'c')
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.Rec709EvidenceShapeValid(result),
                Is.True);

            result.rec709OutputLedger = new AuditionPvPinnedArtifact();

            Assert.That(AuditionPvSixtySecondGateManifestValidator.Rec709EvidenceShapeValid(result),
                Is.False);
        }

        [Test]
        public void Rec709OutputLedger_RequiresEverySourceRangeFrameIncludingHandles()
        {
            var take = RangeTake(10, 12);
            var config = RangeArtifact(new AuditionPvRec709TransformArtifact
            {
                colorPrimaries = "bt709", transferCharacteristics = "bt709",
                matrixCoefficients = "identity-rgb", signalRange = "full",
                transformId = "srgb8-to-bt709-oetf-rgba8-v1",
                inputProfile = "iec-61966-2-1-srgb8",
                outputProfile = "itu-r-bt709-oetf-rgba8",
                roundingMode = "nearest-away-from-zero-u8", alphaMode = "copy-exact",
                editorialSourceRole = "canonical-approved-edit-original"
            }, take);
            var ledger = RangeArtifact(new AuditionPvRec709OutputLedgerArtifact
            {
                configSha256 = Sha('a'),
                frames = new[] { RecFrame(9, '1', "out/frame_0009.png", config),
                    RecFrame(10, '2', "out/frame_0010.png", config),
                    RecFrame(11, '3', "out/frame_0011.png", config),
                    RecFrame(12, '4', "out/frame_0012.png", config),
                    RecFrame(13, '5', "out/frame_0013.png", config) }
            }, take);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .Rec709OutputLedgerTopologyValid(ledger, take, Sha('a'), config), Is.True);

            ledger.frames[2].outputPath = ledger.frames[1].outputPath;
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .Rec709OutputLedgerTopologyValid(ledger, take, Sha('a'), config), Is.False);

            ledger.frames = new[]
            {
                RecFrame(10, '2', "out/frame_0010.png", config),
                RecFrame(11, '3', "out/frame_0011.png", config),
                RecFrame(12, '4', "out/frame_0012.png", config),
                RecFrame(13, '5', "out/frame_0013.png", config)
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .Rec709OutputLedgerTopologyValid(ledger, take, Sha('a'), config), Is.False,
                "A missing leading handle frame is fail-closed.");
            ledger.frames = new[] { RecFrame(9, '1', "out/frame_0009.png", config),
                RecFrame(10, '2', "out/frame_0010.png", config),
                RecFrame(11, '3', "out/frame_0011.png", config),
                RecFrame(12, '4', "out/frame_0012.png", config),
                RecFrame(13, '5', "out/frame_0013.png", config) };
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .Rec709OutputLedgerTopologyValid(ledger, take, Sha('b'), config), Is.False,
                "Config hash drift invalidates the entire output ledger.");
        }

        [Test]
        public void Rec709FixedTransform_RejectsArbitraryOutputAndOnePixelMutation()
        {
            byte[] inputs = { 0, 10, 32, 64, 128, 192, 255 };
            byte[] expected = { 0, 3, 17, 48, 115, 185, 255 };
            Assert.That(inputs.Select(AuditionPvSixtySecondGateManifestValidator
                .TransformSrgb8ToRec709).ToArray(), Is.EqualTo(expected));
            byte[] fullLut = Enumerable.Range(0, 256).Select(value =>
                AuditionPvSixtySecondGateManifestValidator.TransformSrgb8ToRec709((byte)value))
                .ToArray();
            Assert.That(ShaBytes(fullLut), Is.EqualTo(
                "c7a84e8c3af3607d2abd8d2ad5f4e198f4cce194606342d52371c34b5d86ea35"),
                "Independent raw 256-byte BT.709 LUT golden digest drifted.");

            var source = new[]
            {
                new Color32(10, 64, 128, 7), new Color32(192, 255, 0, 231)
            };
            Color32[] converted = source.Select(pixel => new Color32(
                AuditionPvSixtySecondGateManifestValidator.TransformSrgb8ToRec709(pixel.r),
                AuditionPvSixtySecondGateManifestValidator.TransformSrgb8ToRec709(pixel.g),
                AuditionPvSixtySecondGateManifestValidator.TransformSrgb8ToRec709(pixel.b),
                pixel.a)).ToArray();
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .Rec709PixelTransformMatches(source, converted), Is.True);
            string sourcePath = Path.Combine(fixture.root, "rec709-source.png");
            string outputPath = Path.Combine(fixture.root, "rec709-output.png");
            WritePixelsPng(sourcePath, 2, 1, source);
            WritePixelsPng(outputPath, 2, 1, converted);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DecodedRec709TransformMatches(
                sourcePath, outputPath, 2, 1), Is.True);
            converted[1].g++;
            WritePixelsPng(outputPath, 2, 1, converted);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .Rec709PixelTransformMatches(source, converted), Is.False);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DecodedRec709TransformMatches(
                sourcePath, outputPath, 2, 1), Is.False);

            string qhdSource = Path.Combine(fixture.root, "rec709-qhd-source.png");
            string qhdArbitrary = Path.Combine(fixture.root, "rec709-qhd-arbitrary.png");
            WriteSolidPng(qhdSource, 2560, 1440, new Color32(64, 128, 192, 255));
            WriteSolidPng(qhdArbitrary, 2560, 1440, new Color32(64, 128, 192, 255));
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DecodedRec709TransformMatches(
                qhdSource, qhdArbitrary, 2560, 1440), Is.False,
                "An arbitrary decoded QHD PNG is not a canonical Rec.709 edit original.");
        }

        [Test]
        public void FullRangeScanTopologyAndAggregates_RejectSparseOrForgedMagentaPixels()
        {
            var take = RangeTake(20, 21);
            var config = RangeArtifact(new AuditionPvSelectedFrameScanConfigArtifact
            {
                checkId = "error-magenta", frameStride = 1, temporalPairStride = 0,
                pixelStride = 1, algorithm = "full-frame-error-magenta-rgb255-0-255-v1",
                algorithmSha256 = ShaText("full-frame-error-magenta-rgb255-0-255-v1")
            }, take);
            long pixels = 2560L * 1440L;
            var ledger = RangeArtifact(new AuditionPvSelectedFrameScanLedgerArtifact
            {
                checkId = "error-magenta",
                frames = new[] { ScanFrame(19, '1', pixels), ScanFrame(20, '2', pixels),
                    ScanFrame(21, '3', pixels), ScanFrame(22, '4', pixels) }
            }, take);
            var result = new AuditionPvAutomatedCheckResultArtifact
                { inspectedFrameCount = 4, sampledPixelCount = pixels * 4, detectedPixelCount = 0 };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FullRangeScanTopologyValid(
                "error-magenta", config, ledger, take), Is.True);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FullScanAggregatesMatch(
                "error-magenta", result, ledger), Is.True);

            config.temporalPairStride = 1;
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FullRangeScanTopologyValid(
                "error-magenta", config, ledger, take), Is.False,
                "Temporal filmstrips are preview/human-review aids, not a self-attested Gate scan.");
            config.temporalPairStride = 0;

            AuditionPvSelectedFrameScanEntry[] fullRangeFrames = ledger.frames;
            ledger.frames = ledger.frames.Skip(1).ToArray();
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FullRangeScanTopologyValid(
                "error-magenta", config, ledger, take), Is.False,
                "Approved and linked clean inputs cannot omit a handle from full-range scans.");
            ledger.frames = fullRangeFrames;

            ledger.frames[1].sampledPixelCount--;
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FullRangeScanTopologyValid(
                "error-magenta", config, ledger, take), Is.False);
            ledger.frames[1].sampledPixelCount++;
            result.sampledPixelCount--;
            Assert.That(AuditionPvSixtySecondGateManifestValidator.FullScanAggregatesMatch(
                "error-magenta", result, ledger), Is.False);

            string png = Path.Combine(fixture.root, "magenta-2x2.png");
            WritePixelsPng(png, 2, 2, new[]
            {
                new Color32(255, 0, 255, 255), new Color32(1, 2, 3, 255),
                new Color32(255, 0, 255, 0), new Color32(0, 0, 0, 255)
            });
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DecodedMagentaCountMatches(
                png, 2, 2, 2), Is.True);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.DecodedMagentaCountMatches(
                png, 2, 2, 0), Is.False);
        }

        [Test]
        public void RuntimeMaterialAndCleanPlateWorkloads_RejectZeroInspection()
        {
            string[] rendererIds = { "renderer/global/001", "renderer/global/002" };
            string[] materialIds = { "material/guid-a/1", "material/guid-b/2", "material/guid-c/3" };
            string rendererHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("renderers", rendererIds);
            string materialHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("material-slots", materialIds);
            var frame = new AuditionPvSelectedFrameScanEntry
            {
                sourceFrame = 10, inspectedRendererCount = 2, inspectedMaterialSlotCount = 3,
                rendererInventorySha256 = rendererHash, materialInventorySha256 = materialHash
            };
            var workload = new AuditionPvRuntimeFrameWorkload
            {
                sourceFrame = 10, inspectedRendererCount = 2, inspectedMaterialSlotCount = 3,
                rendererStableIds = rendererIds, materialSlotStableIds = materialIds,
                rendererInventorySha256 = rendererHash, materialInventorySha256 = materialHash
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RuntimeWorkloadFramesMatch(
                "renderer-material-scan", new[] { workload }, new[] { frame }), Is.True);
            workload.rendererStableIds = rendererIds.Reverse().ToArray();
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RuntimeWorkloadFramesMatch(
                "renderer-material-scan", new[] { workload }, new[] { frame }), Is.False,
                "Opaque counts/hashes cannot replace sorted stable renderer identities.");
            workload.rendererStableIds = rendererIds;
            workload.inspectedRendererCount = 0;
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RuntimeWorkloadFramesMatch(
                "renderer-material-scan", new[] { workload }, new[] { frame }), Is.False);

            string[] canvasIds = { "canvas/global/001" };
            string[] hudIds = { "hud/global/001", "hud/global/002" };
            string canvasHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("canvases", canvasIds);
            string hudHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("hud-renderers", hudIds);
            workload = new AuditionPvRuntimeFrameWorkload
            {
                sourceFrame = 10, inspectedCanvasCount = 1, inspectedHudRendererCount = 2,
                inspectedDrawCommandCount = 8, canvasStableIds = canvasIds,
                hudRendererStableIds = hudIds, canvasInventorySha256 = canvasHash,
                hudInventorySha256 = hudHash
            };
            frame = new AuditionPvSelectedFrameScanEntry
            {
                sourceFrame = 10, inspectedCanvasCount = 1, inspectedHudRendererCount = 2,
                inspectedDrawCommandCount = 8, canvasInventorySha256 = canvasHash,
                hudInventorySha256 = hudHash,
                rendererHudLayerExcluded = true
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RuntimeWorkloadFramesMatch(
                "hud-layer-absent", new[] { workload }, new[] { frame }), Is.True);
            workload.visibleUiElementCount = 1;
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RuntimeWorkloadFramesMatch(
                "hud-layer-absent", new[] { workload }, new[] { frame }), Is.False);

            string emptyCanvasHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("canvases", Array.Empty<string>());
            string emptyHudHash = AuditionPvSixtySecondGateManifestValidator
                .StableInventorySha256("hud-renderers", Array.Empty<string>());
            workload = new AuditionPvRuntimeFrameWorkload
            {
                sourceFrame = 10, inspectedDrawCommandCount = 8,
                canvasInventorySha256 = emptyCanvasHash, hudInventorySha256 = emptyHudHash
            };
            frame = new AuditionPvSelectedFrameScanEntry
            {
                sourceFrame = 10, inspectedDrawCommandCount = 8,
                canvasInventorySha256 = emptyCanvasHash, hudInventorySha256 = emptyHudHash,
                rendererHudLayerExcluded = true
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RuntimeWorkloadFramesMatch(
                "hud-layer-absent", new[] { workload }, new[] { frame },
                "scene-contract-no-hud"), Is.True);
            workload.inspectedDrawCommandCount = 0;
            Assert.That(AuditionPvSixtySecondGateManifestValidator.RuntimeWorkloadFramesMatch(
                "hud-layer-absent", new[] { workload }, new[] { frame },
                "scene-contract-no-hud"), Is.False);
        }

        [Test]
        public void TwelveSecondSourceLedgerBindings_AreRequiredPerSegmentAndConflictFree()
        {
            AuditionPvTwelveSecondSourceFrameLedgerBinding[] original =
                fixture.manifest.gateEvidence.twelveSecondSourceFrameLedgers;
            fixture.manifest.gateEvidence.twelveSecondSourceFrameLedgers =
                original.Take(4).ToArray();
            AuditionPvSixtySecondGateValidationReport missing = fixture.ValidateStructure();
            Assert.That(Issues(missing),
                Does.Contain("TWELVE_SECOND_SOURCE_LEDGER_BINDING_COUNT_INVALID"));

            fixture.manifest.gateEvidence.twelveSecondSourceFrameLedgers = original;
            fixture.manifest.gateEvidence.twelveSecondSourceFrameLedgers[1].segmentOrder = 0;
            AuditionPvSixtySecondGateValidationReport duplicate = fixture.ValidateStructure();
            Assert.That(Issues(duplicate),
                Does.Contain("TWELVE_SECOND_SOURCE_LEDGER_BINDING_INVALID"));
        }

        [Test]
        public void TwelveSecondMapping_RejoinsExactCurrentCaptureShotFrameAndPath()
        {
            var source = new AuditionPvTwelveSecondSourceManifestIdentity
            {
                captureId = "capture", manifestSha256 = Sha('a'),
                dependencyIdentitySha256 = Sha('b')
            };
            var segment = new AuditionPvTwelveSecondSelectSegment
            {
                order = 2, role = "combat", sourceCaptureId = "capture",
                sourceShotId = "g06", sourceStartFrame = 10, selectStartFrame = 100
            };
            var binding = new AuditionPvTwelveSecondSourceFrameLedgerBinding
            {
                segmentOrder = 2, sourceCaptureId = "capture", sourceManifestSha256 = Sha('a'),
                sourceDependencyIdentitySha256 = Sha('b'), sourceShotId = "g06"
            };
            var mapping = new AuditionPvTwelveSecondFrameMapping
            {
                selectFrame = 105, segmentOrder = 2, role = "combat",
                sourceCaptureId = "capture", sourceManifestSha256 = Sha('a'),
                sourceDependencyIdentitySha256 = Sha('b'), sourceShotId = "g06",
                sourceFrame = 15, sourceRelativePath = "frames/g06/frame_0015.png"
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .TwelveSecondSourceBindingMatches(binding, segment, source), Is.True);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .TwelveSecondSourceMappingIdentityValid(mapping, segment, source, 15,
                    "frames/g06/frame_0015.png"), Is.True);

            mapping.sourceFrame = 16;
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .TwelveSecondSourceMappingIdentityValid(mapping, segment, source, 15,
                    "frames/g06/frame_0015.png"), Is.False);
        }

        [Test]
        public void RightsDependencyClassification_RequiresDispositionReasonAndExactLicensedItem()
        {
            var dependency = new AuditionPvRightsDependencyClassification
            {
                path = "Assets/product.bytes", sha256 = new string('5', 64),
                disposition = "licensed-item", usedItemId = "item-product-asset", reason = "licensed"
            };
            var items = fixture.manifest.usedItems.ToDictionary(value => value.id, StringComparer.Ordinal);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .RightsDependencyClassificationShapeValid(dependency, items), Is.True);

            dependency.reason = string.Empty;

            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .RightsDependencyClassificationShapeValid(dependency, items), Is.False);
        }

        [Test]
        public void CleanPlateReferenceProof_RejectsApprovedSourceRangeDrift()
        {
            AuditionPvSixtySecondAtomicShot shot = fixture.manifest.buckets[2].shots[0];
            AuditionPvSixtySecondTakeCandidate reference = shot.candidateTakes[0];
            var proof = new AuditionPvCleanPlateCompanionProofArtifact
            {
                referenceTakeId = reference.takeId,
                referenceCaptureId = reference.sourceCaptureId,
                referenceSourceManifestSha256 = reference.sourceManifestSha256,
                referenceSourceShotId = reference.sourceShotId,
                referenceFrameLedgerSha256 = reference.sourceFrameLedger.sha256,
                referenceSourceRangeStartFrame = reference.sourceRangeStartFrame,
                referenceSourceRangeEndFrame = reference.sourceRangeEndFrame,
                referenceSelectStartFrame = reference.selectStartFrame,
                referenceSelectEndFrame = reference.selectEndFrame
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.CleanPlateReferenceMatches(
                proof, reference), Is.True);

            proof.referenceSelectEndFrame--;

            Assert.That(AuditionPvSixtySecondGateManifestValidator.CleanPlateReferenceMatches(
                proof, reference), Is.False);
        }

        [Test]
        public void VisualCriterionRef_MustJoinRelevantShotTakeRangeAndReviewedFrame()
        {
            AuditionPvSixtySecondAtomicShot shot = fixture.manifest.buckets[2].shots[0];
            AuditionPvSixtySecondTakeCandidate take = shot.candidateTakes[0];
            string hash = new string('a', 64);
            var reviewed = new[]
            {
                new AuditionPvMeasuredFrame { sourceFrame = take.selectStartFrame, frameSha256 = hash }
            };
            var criterion = new AuditionPvVisualCriterionRef
            {
                criterion = "attack-direction", takeId = take.takeId, atomicShotId = shot.shotId,
                sourceFrame = take.selectStartFrame, frameSha256 = hash, note = "dodge direction readable"
            };
            Assert.That(AuditionPvSixtySecondGateManifestValidator.VisualCriterionRefMatches(
                criterion, take, shot, reviewed), Is.True);

            criterion.criterion = "face";

            Assert.That(AuditionPvSixtySecondGateManifestValidator.VisualCriterionRefMatches(
                criterion, take, shot, reviewed), Is.False,
                "An unrelated gameplay frame cannot be reused as face-readability proof.");
        }

        [Test]
        public void QuarterScaleContactSheet_MustReproduceItsExactSourceCells()
        {
            string source = Path.Combine(fixture.root, "source.png");
            string sheet = Path.Combine(fixture.root, "sheet.png");
            WriteSolidPng(source, 2560, 1440, new Color32(12, 34, 56, 255));
            WriteSolidPng(sheet, 640, 360, new Color32(12, 34, 56, 255));
            Assert.That(AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                sheet, new[] { source }, 1, 1), Is.True);

            WriteSolidPng(sheet, 640, 360, new Color32(12, 34, 57, 255));
            Assert.That(AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                sheet, new[] { source }, 1, 1), Is.False);
        }

        [Test]
        public void QuarterScaleContactSheetAndCriterionRefs_JoinExactAsymmetricCells()
        {
            string red = Path.Combine(fixture.root, "red-source.png");
            string blue = Path.Combine(fixture.root, "blue-source.png");
            string sheet = Path.Combine(fixture.root, "two-cell-sheet.png");
            Color32 redPixel = new(220, 10, 20, 255);
            Color32 bluePixel = new(10, 20, 220, 255);
            WriteSolidPng(red, 2560, 1440, redPixel);
            WriteSolidPng(blue, 2560, 1440, bluePixel);
            Color32[] pixels = new Color32[1280 * 360];
            for (int y = 0; y < 360; y++)
                for (int x = 0; x < 1280; x++)
                    pixels[y * 1280 + x] = x < 640 ? redPixel : bluePixel;
            WritePixelsPng(sheet, 1280, 360, pixels);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                sheet, new[] { red, blue }, 2, 1), Is.True);
            Assert.That(AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                sheet, new[] { blue, red }, 2, 1), Is.False);

            string hash = AuditionPvSha256.FileHash(red);
            string key = string.Join("\0", "take-a", "180", hash);
            var face = new AuditionPvVisualCriterionRef
                { criterion = "face", takeId = "take-a", sourceFrame = 180, frameSha256 = hash };
            var boss = new AuditionPvVisualCriterionRef
                { criterion = "boss", takeId = "take-a", sourceFrame = 180, frameSha256 = hash };
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .VisualCriterionRefIsContactCell(face, new[] { key }), Is.True);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .VisualCriterionRefIsContactCell(boss, new[] { key }), Is.True,
                "One clear contact cell may legitimately satisfy more than one criterion.");
            boss.sourceFrame++;
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .VisualCriterionRefIsContactCell(boss, new[] { key }), Is.False);
        }

        [Test]
        public void PngAndPreviewSafety_RejectsNonCanonicalGridOversizedIhdrAndInvalidBytes()
        {
            string source = Path.Combine(fixture.root, "safe-source.png");
            string sheet = Path.Combine(fixture.root, "safe-sheet.png");
            WriteSolidPng(source, 2560, 1440, new Color32(1, 2, 3, 255));
            WriteSolidPng(sheet, 640, 360, new Color32(1, 2, 3, 255));
            Assert.That(AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                sheet, new[] { source }, 2, 1), Is.False, "One cell has one canonical column.");
            Assert.That(AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                sheet, Enumerable.Repeat(source, 33).ToArray(), 4, 9), Is.False);

            string huge = Path.Combine(fixture.root, "huge-ihdr.png");
            File.WriteAllBytes(huge, PngIhdrOnly(100000, 100000));
            Assert.That(AuditionPvSixtySecondGateManifestValidator.TryPngPreflight(
                huge, 64L * 1024 * 1024, out _, out _), Is.False);
            string invalid = Path.Combine(fixture.root, "invalid.png");
            File.WriteAllBytes(invalid, Encoding.UTF8.GetBytes("not a png"));
            Assert.That(AuditionPvSixtySecondGateManifestValidator.TryPngPreflight(
                invalid, 64L * 1024 * 1024, out _, out _), Is.False);

            string capped = Path.Combine(fixture.root, "capped-evidence.json");
            using (FileStream stream = File.Create(capped)) stream.SetLength(1025);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .EvidenceFileWithinLimitForTest(capped, 1024), Is.False);

            string graphics = Path.Combine(fixture.root, "graphics");
            string canonical = Path.Combine(graphics, "rec709", "capture-a", "shot-a",
                "frame_0012.png");
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .CanonicalRec709OutputPathForTest(canonical, new[] { graphics },
                    "capture-a", "shot-a", 12), Is.True);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .CanonicalRec709OutputPathForTest(Path.Combine(graphics, "frame_0012.png"),
                    new[] { graphics }, "capture-a", "shot-a", 12), Is.False);
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .PathHasNoReparseChainForTest("\0invalid"), Is.False);
        }

        [Test]
        public void EvidenceReparseChain_IsRejectedWhenRuntimeCanCreateDirectoryLinks()
        {
            var createLink = typeof(Directory).GetMethod("CreateSymbolicLink",
                new[] { typeof(string), typeof(string) });
            if (createLink == null) Assert.Ignore("Runtime has no managed symbolic-link API.");
            string target = Path.Combine(fixture.root, "reparse-target");
            string link = Path.Combine(fixture.root, "reparse-link");
            Directory.CreateDirectory(target);
            try { createLink.Invoke(null, new object[] { link, target }); }
            catch (Exception exception)
            { Assert.Ignore("OS policy did not allow a temporary directory link: " + exception.Message); }
            Assert.That(AuditionPvSixtySecondGateManifestValidator
                .PathHasNoReparseChainForTest(link), Is.False);
        }

        private static void ResizeSingleShotBucket(
            AuditionPvSixtySecondSequenceBucket bucket, int start, int end)
        {
            bucket.timelineStartFrame = start;
            bucket.timelineEndFrame = end;
            AuditionPvSixtySecondAtomicShot shot = bucket.shots.Single();
            shot.timelineStartFrame = start;
            shot.timelineEndFrame = end;
            int length = end - start + 1;
            foreach (AuditionPvSixtySecondTakeCandidate take in shot.candidateTakes)
            {
                take.selectEndFrame = take.selectStartFrame + length - 1;
                take.sourceRangeEndFrame = take.selectEndFrame + take.handleAfterFrames;
            }
        }

        private static string[] Issues(AuditionPvSixtySecondGateValidationReport report) =>
            report.issues.Select(value => value.code).Distinct().ToArray();

        private static AuditionPvPinnedArtifact Pin(string path, char hash) =>
            new() { path = path, sha256 = new string(hash, 64) };
        private static string Sha(char value) => new(value, 64);

        private static AuditionPvSemanticBeatProof BeatProof(string beatId,
            AuditionPvPinnedArtifact runtime) => new()
            {
                beatId = beatId, runtimeFactKey = beatId, verifiedBy = "reviewer",
                verifiedAtUtc = "2026-08-17T00:00:00Z",
                supportingTestSuite = "AuditionPvSixtySecondEvidence",
                supportingTestName = "semantic-beat/" + beatId,
                runtimeProof = runtime
            };

        private static AuditionPvSixtySecondTakeCandidate RangeTake(int start, int end) => new()
        {
            selectStartFrame = start, selectEndFrame = end,
            sourceRangeStartFrame = start - 1, sourceRangeEndFrame = end + 1
        };

        private static T RangeArtifact<T>(T value, AuditionPvSixtySecondTakeCandidate take)
            where T : AuditionPvRangeBoundArtifact
        {
            value.selectStartFrame = take.selectStartFrame;
            value.selectEndFrame = take.selectEndFrame;
            value.sourceRangeStartFrame = take.sourceRangeStartFrame;
            value.sourceRangeEndFrame = take.sourceRangeEndFrame;
            return value;
        }

        private static AuditionPvRec709OutputFrame RecFrame(int frame, char hash, string path,
            AuditionPvRec709TransformArtifact config) => new()
            {
                sourceFrame = frame, sourceFrameSha256 = Sha(hash), outputSha256 = Sha('f'),
                outputPath = path, width = 2560, height = 1440,
                colorPrimaries = config.colorPrimaries,
                transferCharacteristics = config.transferCharacteristics,
                matrixCoefficients = config.matrixCoefficients, signalRange = config.signalRange
            };

        private static AuditionPvSelectedFrameScanEntry ScanFrame(int frame, char hash,
            long pixels) => new()
            {
                sourceFrame = frame, frameSha256 = Sha(hash), width = 2560, height = 1440,
                sampledPixelCount = pixels, rendererHudLayerExcluded = true
            };

        private static AuditionPvCaptureManifest CaptureCoreFixture() => new()
        {
            schemaVersion = AuditionPvCaptureContract.SchemaVersion,
            captureId = "acyclic-capture", createdAtUtc = "2026-08-17T00:00:00.0000000Z",
            outputRoot = "D:/capture", outputDirectory = "D:/capture/acyclic-capture",
            sourceFormat = AuditionPvCaptureContract.SourceFormat,
            width = 2560, height = 1440, fps = 60,
            gitCommitSha = new string('a', 40), gitBranch = "main", gitWorktreeDirty = false,
            worktreeDirtyHashSha256 = Sha('0'), worktreeDirtyHashAlgorithm = "sha256-v1",
            unityVersion = "6000.3.5f2", unityVersionWithRevision = "6000.3.5f2 (revision)",
            recorderPackageVersion = "5.1.0", urpPackageVersion = "17.3.0",
            activeRenderPipelineAssetPath = "Assets/Settings/URP.asset",
            shots = new[]
            {
                new AuditionPvShotManifestEntry
                {
                    id = "g01", scenePath = "Assets/Scene.unity", startFrame = 0,
                    endFrame = 359, expectedFrameCount = 360, hudMode = "hud-on", notes = "core"
                }
            },
            baselines = new[]
            {
                new AuditionPvBaselineManifestEntry
                {
                    id = "baseline", shotId = "g01", sourceFrame = 0,
                    fileName = "frame_0000.png", hudMode = "hud-on", status = "approved"
                }
            },
            dependencyHashes = new[]
            {
                new AuditionPvDependencyHash
                    { path = "Assets/source.asset", exists = true, byteLength = 3, sha256 = Sha('d') }
            }
        };

        private static string ShaText(string value)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(new UTF8Encoding(false, true).GetBytes(value))
                .Select(item => item.ToString("x2")));
        }

        private static string ShaBytes(byte[] value)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(value ?? Array.Empty<byte>())
                .Select(item => item.ToString("x2")));
        }

        private static void WriteSolidPng(string path, int width, int height, Color32 color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            try
            {
                var pixels = Enumerable.Repeat(color, width * height).ToArray();
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static void WritePixelsPng(string path, int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static byte[] PngIhdrOnly(uint width, uint height)
        {
            byte[] value = new byte[29];
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            Array.Copy(signature, value, signature.Length);
            value[11] = 13; value[12] = (byte)'I'; value[13] = (byte)'H';
            value[14] = (byte)'D'; value[15] = (byte)'R';
            for (int shift = 24, index = 16; shift >= 0; shift -= 8, index++)
                value[index] = (byte)(width >> shift);
            for (int shift = 24, index = 20; shift >= 0; shift -= 8, index++)
                value[index] = (byte)(height >> shift);
            value[24] = 8; value[25] = 6;
            return value;
        }

        private static void WritePcm16Wav(string path, int durationMilliseconds,
            int loudMilliseconds, short amplitude)
        {
            const int sampleRate = 48000, channels = 1, bytesPerSample = 2;
            int sampleCount = sampleRate * durationMilliseconds / 1000;
            int loudSamples = sampleRate * loudMilliseconds / 1000;
            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream, Encoding.ASCII, false);
            int dataBytes = sampleCount * bytesPerSample;
            writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + dataBytes);
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
            writer.Write((ushort)1); writer.Write((ushort)channels); writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bytesPerSample);
            writer.Write((ushort)(channels * bytesPerSample)); writer.Write((ushort)16);
            writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(dataBytes);
            for (int index = 0; index < sampleCount; index++)
                writer.Write(index < loudSamples ? amplitude : (short)0);
        }

        private sealed class Fixture : IDisposable
        {
            private const string CurrentGit = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
            private const string SourceGit = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            private static readonly HashSet<string> CoreBeats = new(StringComparer.Ordinal)
            {
                "city-hud-gameplay", "perfect-dodge", "summon-chain", "summon-defense",
                "c33-wing-deployment", "c34-eye-open", "boss-pattern-1", "boss-pattern-2",
                "boss-pattern-3", "olympus-hud-gameplay", "player-tier3-ultimate",
                "boss-finisher",
                "boss-collapse", "aftermath"
            };

            public string root;
            public AuditionPvSixtySecondShotGateManifest manifest;
            public AuditionPvSixtySecondValidationContext context;

            public static Fixture Create()
            {
                var result = new Fixture
                {
                    root = Path.Combine(Path.GetTempPath(),
                        "DimensionBrawl_PV60_v2_" + Guid.NewGuid().ToString("N"))
                };
                Directory.CreateDirectory(result.root);
                string captures = Path.Combine(result.root, "Captures");
                string selects = Path.Combine(result.root, "Selects");
                string evidence = Path.Combine(result.root, "Evidence");
                Directory.CreateDirectory(captures);
                Directory.CreateDirectory(selects);
                Directory.CreateDirectory(evidence);

                AuditionPvSixtySecondShotGateManifest manifest =
                    AuditionPvSixtySecondGateManifestValidator.CreateEmptyPlan();
                manifest.declaredStatus = "ready-for-editing";
                manifest.productCheckpointGitSha = CurrentGit;

                manifest.rights = new[]
                {
                    Right("rights-asset", "asset", '1'), Right("rights-font", "font", '2'),
                    Right("rights-audio", "audio", '3'), Right("rights-ai", "ai", '4')
                };
                var items = new List<AuditionPvSixtySecondUsedItem>
                {
                    Item("item-product-asset", "asset", "rights-asset", "Assets/product.bytes", '5'),
                    Item("item-endcard-graphic", "asset", "rights-asset", "Evidence/end-card.png", 'e'),
                    Item("item-endcard-font", "font", "rights-font", "Assets/font.bytes", '6'),
                    Item("item-ai-provenance", "ai", "rights-ai", "Evidence/generation.json", '7')
                };
                var audio = new List<AuditionPvSixtySecondAudioEvidence>();
                int audioIndex = 0;
                foreach (string category in new[] { "music", "sfx", "vo", "ambience" })
                {
                    string itemId = "item-audio-" + category;
                    char hash = new[] { '8', '9', 'a', 'b' }[audioIndex];
                    items.Add(Item(itemId, "audio", "rights-audio",
                        "Evidence/" + category + ".wav", hash));
                    string[] categoryCues = Cues(category);
                    audio.Add(new AuditionPvSixtySecondAudioEvidence
                    {
                        id = "audio-" + category,
                        category = category,
                        cueIds = categoryCues,
                        cueRegions = categoryCues.Select((cue, cueIndex) => new AuditionPvAudioCueRegion
                        {
                            cueId = cue,
                            startMilliseconds = cueIndex * 100,
                            endMilliseconds = cueIndex * 100 + 100
                        }).ToArray(),
                        usedItemId = itemId,
                        file = Pin("Evidence/" + category + ".wav", hash),
                        sampleRate = 48000,
                        channels = category == "vo" ? 1 : 2,
                        generatedByAi = category == "vo",
                        aiUsedItemId = category == "vo" ? "item-ai-provenance" : string.Empty,
                        generationManifest = category == "vo"
                            ? Pin("Evidence/generation.json", '7') : new AuditionPvPinnedArtifact(),
                        humanListeningStatus = "pending"
                    });
                    audioIndex++;
                }
                manifest.usedItems = items.ToArray();
                manifest.audio = audio.ToArray();

                foreach ((AuditionPvSixtySecondSequenceBucket bucket, int bucketIndex) in
                         manifest.buckets.Select((value, index) => (value, index)))
                {
                    if (bucket.bucketId == "PV_S070")
                    {
                        bucket.shots = new[]
                        {
                            Shot(bucket, bucket.timelineStartFrame,
                                bucket.timelineStartFrame + 359,
                                new[] { "boss-pattern-1", "olympus-hud-gameplay" }, 0, "hud-on"),
                            Shot(bucket, bucket.timelineStartFrame + 360, bucket.timelineEndFrame,
                                new[] { "boss-pattern-2", "boss-pattern-3" }, 1, "mixed")
                        };
                    }
                    else
                    {
                        string hud = bucket.bucketId == "PV_S020" ? "hud-on" :
                            bucket.bucketId == "PV_S100" ? "end-card" :
                            bucket.bucketId == "PV_S080" ? "mixed" : "hud-off";
                        bucket.shots = new[]
                        {
                            Shot(bucket, bucket.timelineStartFrame, bucket.timelineEndFrame,
                                bucket.requiredBeatIds, 0, hud)
                        };
                    }
                    _ = bucketIndex;
                }
                AuditionPvSixtySecondAtomicShot cleanPlateShot = manifest.buckets[2].shots[0];
                cleanPlateShot.cleanPlateTakeId = cleanPlateShot.candidateTakes[3].takeId;

                manifest.gateEvidence = new AuditionPvSixtySecondGateEvidence
                {
                    twelveSecondPackageDirectory = "Selects/fake-package",
                    twelveSecondManifestSha256 = Sha('d'),
                    twelveSecondValidationSha256 = Sha('e'),
                    twelveSecondApproval = Pin("Evidence/12s-approval.json", 'f'),
                    visualReview = Pin("Evidence/visual-review.json", 'a'),
                    rightsCoverageReview = Pin("Evidence/rights-coverage-review.json", 'b'),
                    twelveSecondSourceFrameLedgers = Enumerable.Range(0,
                        AuditionPvTwelveSecondGoldAssembler.RequiredRoles.Length).Select(index =>
                        new AuditionPvTwelveSecondSourceFrameLedgerBinding
                        {
                            segmentOrder = index,
                            sourceCaptureId = "12s-capture-" + index,
                            sourceManifestSha256 = Sha((char)('1' + index)),
                            sourceDependencyIdentitySha256 = Sha('c'),
                            sourceShotId = "12s-shot-" + index,
                            frameLedger = Pin("Captures/12s-capture-" + index +
                                "/evidence/frame_hashes.sha256", (char)('b' + index))
                        }).ToArray()
                };

                result.manifest = manifest;
                result.context = new AuditionPvSixtySecondValidationContext
                {
                    projectRoot = result.root,
                    currentGitCommitSha = CurrentGit,
                    currentGitClean = true,
                    allowedEvidenceRoots = new[] { result.root },
                    allowedCaptureRoots = new[] { captures },
                    allowedSelectRoots = new[] { selects },
                    allowedAudioRoots = new[] { evidence },
                    allowedLicenseRoots = new[] { evidence },
                    allowedGraphicsRoots = new[] { evidence },
                    allowedReviewRoots = new[] { evidence }
                };
                return result;
            }

            public AuditionPvSixtySecondGateValidationReport ValidateStructure() =>
                AuditionPvSixtySecondGateManifestValidator.ValidateStructure(manifest);

            public AuditionPvSixtySecondGateValidationReport ValidateProduction() =>
                AuditionPvSixtySecondGateManifestValidator.ValidateProduction(manifest, context);

            public void MaterializeFakeTwelveSecondPackage()
            {
                string directory = Path.Combine(root, "Selects", "fake-package");
                Directory.CreateDirectory(directory);
                string manifestPath = Path.Combine(directory,
                    AuditionPvTwelveSecondGoldAssembler.ManifestFileName);
                string validationPath = Path.Combine(directory,
                    AuditionPvTwelveSecondGoldAssembler.ValidationReportFileName);
                File.WriteAllText(manifestPath, "{}", new UTF8Encoding(false));
                File.WriteAllText(validationPath, "{}", new UTF8Encoding(false));
                manifest.gateEvidence.twelveSecondManifestSha256 = AuditionPvSha256.FileHash(manifestPath);
                manifest.gateEvidence.twelveSecondValidationSha256 = AuditionPvSha256.FileHash(validationPath);
            }

            public string MaterializeManifestFile()
            {
                string path = Path.Combine(root, "pv60_manifest.json");
                File.WriteAllText(path, JsonUtility.ToJson(manifest, true) + Environment.NewLine,
                    new UTF8Encoding(false));
                return path;
            }

            private static AuditionPvSixtySecondAtomicShot Shot(
                AuditionPvSixtySecondSequenceBucket bucket, int start, int end,
                string[] beats, int ordinal, string hud)
            {
                string shotId = bucket.bucketId.ToLowerInvariant() + "-shot-" + ordinal;
                bool core = beats.Any(CoreBeats.Contains);
                var shot = new AuditionPvSixtySecondAtomicShot
                {
                    shotId = shotId,
                    sourceKind = bucket.bucketId == "PV_S100" ? "end-card" :
                        bucket.bucketId == "PV_S020" || bucket.bucketId == "PV_S030" ||
                        bucket.bucketId == "PV_S070" || bucket.bucketId == "PV_S080"
                            ? "gameplay" : "cinematic",
                    timelineStartFrame = start,
                    timelineEndFrame = end,
                    coreShot = core,
                    scenePath = bucket.bucketId.CompareTo("PV_S050") < 0
                        ? AuditionPvCityHeroPocketCapture.CityScenePath
                        : AuditionPvStationPhase2PatternRelayCapture.StationScenePath,
                    cameraId = "camera-" + shotId,
                    gameplayState = "state-" + shotId,
                    deterministicSeed = 1000 + start,
                    timelineId = "timeline-" + shotId,
                    editorialHudMode = hud,
                    beatIds = beats.ToArray(),
                    audioRefIds = new[] { "audio-music" },
                    usedItemIds = bucket.bucketId == "PV_S100"
                        ? new[] { "item-endcard-graphic", "item-endcard-font" }
                        : new[] { "item-product-asset" }
                };
                if (shot.sourceKind == "end-card")
                {
                    shot.scenePath = string.Empty;
                    shot.cameraId = string.Empty;
                    shot.gameplayState = string.Empty;
                    shot.timelineId = string.Empty;
                    shot.deterministicSeed = -1;
                    shot.graphicSourceId = "layout-placeholder";
                    shot.graphicProductionStatus = "layout-placeholder-approved";
                    shot.sloganApprovalStatus = "pending-approval";
                    shot.auditionNoticeApprovalStatus = "pending-approval";
                    shot.graphicArtifact = Pin("Evidence/end-card.png", 'e');
                    shot.candidateTakes = Array.Empty<AuditionPvSixtySecondTakeCandidate>();
                    shot.approvedTakeId = string.Empty;
                    return shot;
                }
                int takeCount = core ? 4 : 1;
                int length = end - start + 1;
                shot.candidateTakes = Enumerable.Range(0, takeCount).Select(index =>
                {
                    string takeId = shotId + "-take-" + index;
                    string captureId = "capture-" + shotId + "-" + index;
                    string sourceShotId = "source-" + shotId + "-" + index;
                    string declaredHud = index == 0
                        ? hud == "mixed" ? "hud-on-to-result" : hud
                        : index == 1 ? "hud-off" : index == 2 ? "hud-on" : "clean-plate";
                    char hash = (char)('a' + index);
                    return new AuditionPvSixtySecondTakeCandidate
                    {
                        takeId = takeId,
                        sourceCaptureId = captureId,
                        sourceShotId = sourceShotId,
                        gitCommitSha = SourceGit,
                        declaredHudMode = declaredHud,
                        cameraId = shot.cameraId,
                        gameplayState = shot.gameplayState,
                        deterministicSeed = shot.deterministicSeed,
                        timelineId = shot.timelineId,
                        sourceCaptureCoreSha256 = Sha((char)('1' + index)),
                        sourceManifest = Pin("Captures/" + captureId + "/capture_manifest.json", hash),
                        sourceDependencyIdentitySha256 = Sha('c'),
                        sourceFrameLedger = Pin("Captures/" + captureId + "/evidence/frame_hashes.sha256", hash),
                        shotAuthorship = Pin("Captures/" + captureId +
                            "/evidence/shot-authorship.json", hash),
                        semanticProof = declaredHud != "clean-plate"
                            ? Pin("Captures/" + captureId + "/evidence/semantic.json", hash)
                            : new AuditionPvPinnedArtifact(),
                        cleanPlateProof = declaredHud == "clean-plate"
                            ? Pin("Captures/" + captureId + "/evidence/clean-plate.json", hash)
                            : new AuditionPvPinnedArtifact(),
                        automatedProof = Pin("Captures/" + captureId + "/evidence/automated.json", hash),
                        humanReview = Pin("Evidence/review-" + takeId + ".json", hash),
                        sourceRangeStartFrame = 0,
                        selectStartFrame = 180,
                        selectEndFrame = 180 + length - 1,
                        sourceRangeEndFrame = 180 + length - 1 + 180,
                        handleBeforeFrames = 180,
                        handleAfterFrames = 180
                    };
                }).ToArray();
                shot.approvedTakeId = shot.candidateTakes[0].takeId;
                return shot;
            }

            private static AuditionPvSixtySecondRightsEvidence Right(
                string id, string scope, char hash) => new()
                { id = id, scope = scope, record = Pin("Evidence/" + id + ".json", hash) };

            private static AuditionPvSixtySecondUsedItem Item(string id, string scope,
                string right, string source, char hash) => new()
                {
                    id = id, scope = scope, rightsRecordId = right,
                    sourceLocator = source,
                    dependencyBinding = source.StartsWith("Assets/", StringComparison.Ordinal)
                        ? "unity-dependency" : "external-artifact",
                    artifact = Pin(source, hash)
                };

            private static string[] Cues(string category) => category switch
            {
                "music" => new[] { "music-bed" },
                "ambience" => new[] { "city-ambience", "olympus-ambience" },
                "vo" => new[] { "announcement-vo", "inori-vo", "boss-vo" },
                _ => new[]
                {
                    "gun-mechanical", "gun-fire", "gun-tail", "dodge", "summon", "hit", "boss-charge",
                    "boss-fire", "boss-death", "wing-deploy", "eye-open"
                }
            };

            private static AuditionPvPinnedArtifact Pin(string path, char hash) =>
                new() { path = path, sha256 = Sha(hash) };

            private static string Sha(char value) => new(value, 64);

            public void Dispose()
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
