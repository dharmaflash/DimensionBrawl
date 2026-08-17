using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvSixtySecondApprovalAssemblerTests
    {
        private const string HashA =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string HashB =
            "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        [Test]
        public void OperatorReviewedEdl_MustExactlyMatchComposerIdentityAndRanges()
        {
            AuditionPvSixtySecondProductionEdlRow[] exact =
                AuditionPvSixtySecondProductionComposer.CreateDefaultEdlForTests();
            Assert.DoesNotThrow(() => AuditionPvSixtySecondApprovalAssembler
                .ValidateEdlForTests(exact));

            AuditionPvSixtySecondProductionEdlRow[] drift =
                AuditionPvSixtySecondProductionComposer.CreateDefaultEdlForTests();
            drift[3].selectStartFrame++;
            Assert.Throws<InvalidDataException>(() =>
                AuditionPvSixtySecondApprovalAssembler.ValidateEdlForTests(drift));
        }

        [Test]
        public void Header_RejectsAutomatedJudgementOrigin()
        {
            var spec = new AuditionPvSixtySecondOperatorApprovalSpec
            {
                assemblyId = "operator-pass-001",
                judgementOrigin = "automated-agent",
                reviewedBy = "reviewer",
                reviewedAtUtc = "2026-08-17T00:00:00Z",
                allCandidateSemanticTestBindingsReviewed = true,
                semanticEvidenceReviewNote = "Matched each runtime beat to its passed test.",
                productCheckpointGitSha =
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                captureRootDirectory = "C:/fixture/captures",
                reviewOutputDirectory = "C:/fixture/reviews"
            };

            InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
                AuditionPvSixtySecondApprovalAssembler.ValidateHeaderForTests(spec));
            Assert.That(error.Message, Does.Contain("never self-approves"));
            spec.judgementOrigin = "human-operator";
            Assert.DoesNotThrow(() => AuditionPvSixtySecondApprovalAssembler
                .ValidateHeaderForTests(spec));
        }

        [Test]
        public void CurrentTwelveSecondHold_IsExplicitAndCannotMasqueradeAsReady()
        {
            string hold = AuditionPvSixtySecondApprovalAssembler
                .CurrentTwelveSecondHoldForTests(
                    new AuditionPvSixtySecondCurrentTwelveSecondSpec
                    {
                        status = "hold",
                        holdReason = "package-reassembly-pending"
                    });
            Assert.That(hold, Is.EqualTo(
                "CURRENT_12S_HOLD:package-reassembly-pending"));

            Assert.Throws<InvalidDataException>(() =>
                AuditionPvSixtySecondApprovalAssembler.CurrentTwelveSecondHoldForTests(
                    new AuditionPvSixtySecondCurrentTwelveSecondSpec
                    {
                        status = "ready",
                        packageDirectory = "C:/fixture/select"
                    }));
        }

        [Test]
        public void ReviewRows_MustPinEverySkeletonFrameWithCriterionAndNote()
        {
            AuditionPvMeasuredFrame[] skeleton =
            {
                new() { sourceFrame = 0, frameSha256 = HashA },
                new() { sourceFrame = 60, frameSha256 = HashB }
            };
            AuditionPvSixtySecondReviewCriterionSpec[] exact =
            {
                new()
                {
                    criterion = "motion-range", sourceFrame = 60,
                    frameSha256 = HashB, note = "trail remains continuous"
                },
                new()
                {
                    criterion = "motion-range", sourceFrame = 0,
                    frameSha256 = HashA, note = "opening pose readable"
                }
            };
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .ReviewRowsMatchSkeletonForTests(exact, skeleton), Is.True);

            exact[1].note = string.Empty;
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .ReviewRowsMatchSkeletonForTests(exact, skeleton), Is.False);
            exact[1].note = "restored";
            exact[1].frameSha256 = HashB;
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .ReviewRowsMatchSkeletonForTests(exact, skeleton), Is.False);
        }

        [Test]
        public void SamplingAndPreview_AreBoundedDeterministicAndIncludeEndpoints()
        {
            Assert.That(AuditionPvSixtySecondApprovalAssembler.SampledFramesForTests(60, 419),
                Is.EqualTo(new[] { 60, 120, 180, 240, 300, 360, 419 }));
            int[] preview = AuditionPvSixtySecondApprovalAssembler
                .DeterministicPreviewIndexesForTests(125, 32);
            Assert.That(preview, Has.Length.EqualTo(32));
            Assert.That(preview[0], Is.Zero);
            Assert.That(preview[^1], Is.EqualTo(124));
            Assert.That(preview.Distinct().Count(), Is.EqualTo(preview.Length));
        }

        [Test]
        public void CaptureCardinality_MirrorsProductionGateBounds()
        {
            var atLimits = new AuditionPvCaptureManifest
            {
                shots = new AuditionPvShotManifestEntry[512],
                baselines = new AuditionPvBaselineManifestEntry[2048],
                dependencyHashes = new AuditionPvDependencyHash[4096],
                testResults = new AuditionPvTestResult[4096]
            };
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .CaptureCardinalityWithinGateForTests(atLimits), Is.True);

            atLimits.shots = new AuditionPvShotManifestEntry[513];
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .CaptureCardinalityWithinGateForTests(atLimits), Is.False);
            atLimits.shots = Array.Empty<AuditionPvShotManifestEntry>();
            atLimits.baselines = new AuditionPvBaselineManifestEntry[2049];
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .CaptureCardinalityWithinGateForTests(atLimits), Is.False);
            atLimits.baselines = Array.Empty<AuditionPvBaselineManifestEntry>();
            atLimits.dependencyHashes = new AuditionPvDependencyHash[4097];
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .CaptureCardinalityWithinGateForTests(atLimits), Is.False);
            atLimits.dependencyHashes = Array.Empty<AuditionPvDependencyHash>();
            atLimits.testResults = new AuditionPvTestResult[4097];
            Assert.That(AuditionPvSixtySecondApprovalAssembler
                .CaptureCardinalityWithinGateForTests(atLimits), Is.False);
        }

        [Test]
        public void TakeIds_SeparateActionAndCleanCompanionIdentities()
        {
            Assert.That(AuditionPvSixtySecondApprovalAssembler.TakeIdForTests(
                    "pv-s060-eye-open", "capture-1", false),
                Is.EqualTo("pv-s060-eye-open-take-capture-1"));
            Assert.That(AuditionPvSixtySecondApprovalAssembler.TakeIdForTests(
                    "pv-s060-eye-open", "capture-1", true),
                Is.EqualTo("pv-s060-eye-open-clean-capture-1"));
        }

        [Test]
        public void OperatorSpecObjectAndPinnedPathMismatch_FailsClosed()
        {
            string directory = CreateTemporaryDirectory();
            try
            {
                string path = Path.Combine(directory, "operator-review.json");
                AuditionPvSixtySecondOperatorApprovalSpec pinned = ValidOperatorSpec();
                WriteJson(path, pinned);
                AuditionPvSixtySecondOperatorApprovalSpec unrelated = ValidOperatorSpec();
                unrelated.assemblyId = "different-human-decision";

                InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
                    AuditionPvSixtySecondApprovalAssembler.BindOperatorSpecForTests(
                        unrelated, path));
                Assert.That(error.Message, Does.Contain("not the exact JSON identity"));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void OperatorSpecMutationAfterBoundedRead_FailsBeforeDeserializeOrAssembly()
        {
            string directory = CreateTemporaryDirectory();
            try
            {
                string path = Path.Combine(directory, "operator-review.json");
                AuditionPvSixtySecondOperatorApprovalSpec spec = ValidOperatorSpec();
                WriteJson(path, spec);

                InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
                    AuditionPvSixtySecondApprovalAssembler.BindOperatorSpecForTests(
                        spec, path, value => File.AppendAllText(value, " ",
                            new UTF8Encoding(false, true))));
                Assert.That(error.Message, Does.Contain("changed after its bounded byte snapshot"));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static AuditionPvSixtySecondOperatorApprovalSpec ValidOperatorSpec() => new()
        {
            schemaVersion = AuditionPvSixtySecondApprovalAssembler.SpecSchema,
            assemblyId = "operator-pass-identity",
            judgementOrigin = "human-operator",
            reviewedBy = "fixture-reviewer",
            reviewedAtUtc = "2026-08-17T00:00:00Z",
            allCandidateSemanticTestBindingsReviewed = true,
            semanticEvidenceReviewNote = "Every candidate binding was reviewed.",
            productCheckpointGitSha = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            captureRootDirectory = "C:/fixture/captures",
            reviewOutputDirectory = "C:/fixture/reviews"
        };

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(),
                "pv60-approval-identity-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return Path.GetFullPath(path);
        }

        private static void WriteJson(string path,
            AuditionPvSixtySecondOperatorApprovalSpec spec) =>
            File.WriteAllText(path, JsonUtility.ToJson(spec, true) + "\n",
                new UTF8Encoding(false, true));
    }
}
