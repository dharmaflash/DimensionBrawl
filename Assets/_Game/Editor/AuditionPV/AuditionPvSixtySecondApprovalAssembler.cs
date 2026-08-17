using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV
{
    /// <summary>
    /// Binds already-produced physical evidence to an explicit human decision.
    /// This class never chooses a take, invents a review result, or mutates a
    /// capture manifest. It only materializes typed wrappers after every byte
    /// identity declared by the operator has been checked.
    /// </summary>
    internal static class AuditionPvSixtySecondApprovalAssembler
    {
        internal const string SpecSchema =
            "dimension-brawl.audition-pv.preedit-60s-operator-approval-spec.v1";
        internal const string ReceiptSchema =
            "dimension-brawl.audition-pv.preedit-60s-approval-assembly.v1";
        internal const string InputFileName = "preedit_60s_compose_input_fragment.json";
        internal const string AssemblyReceiptFileName = "approval_assembly_receipt.json";
        internal const string BatchSpecArgument = "-pv60ApprovalSpec=";
        internal const int ExpectedCaptureCount = 19;
        internal const int ExpectedEvidenceReceiptCount = 37;
        private const long MaxJsonBytes = 16L * 1024L * 1024L;
        private const int MaxCaptureShots = 512;
        private const int MaxCaptureBaselines = 2048;
        private const int MaxCaptureDependencies = 4096;
        private const int MaxCaptureTestResults = 4096;
        private const long MaxPngBytes = 64L * 1024L * 1024L;
        private const string EvidenceSuite = "AuditionPvSixtySecondEvidence";
        private static readonly UTF8Encoding Utf8 = new(false, true);

        private static readonly FamilyContract[] Families =
        {
            new("city-g01-g03", 3, "g01", "g02", "g03"),
            new("city-s030", 3, "s030"),
            new("station-s050", 1, "s050"),
            new("station-g04", 3, "g04", "g04-clean"),
            new("station-g06", 3, "g06"),
            new("station-g07", 3, "g07"),
            new("station-g08", 3, "g08")
        };

        public static void RunBatchAssemble()
        {
            try
            {
                string path = ReadArgument(BatchSpecArgument);
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentException(
                        "RunBatchAssemble requires -pv60ApprovalSpec=<absolute-json-path>.");
                AuditionPvSixtySecondApprovalAssemblyResult result =
                    AssembleProductionFromPath(path);
                if (!result.currentTwelveSecondReady)
                {
                    Debug.LogWarning("[AuditionPV] Approval evidence assembled, but the " +
                                     "current 12-second package is on explicit HOLD: " +
                                     string.Join(", ", result.holds ?? Array.Empty<string>()));
                    EditorApplication.Exit(2);
                    return;
                }
                Debug.Log("[AuditionPV] Approval compose-input fragment assembled: " +
                          result.composeInputPath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        internal static AuditionPvSixtySecondApprovalAssemblyResult AssembleProduction(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            string specPath)
        {
            RequireAbsoluteSpecPath(specPath);
            PinnedJson<AuditionPvSixtySecondOperatorApprovalSpec> bound =
                ReadJsonSnapshot<AuditionPvSixtySecondOperatorApprovalSpec>(
                    specPath, null, null, MaxJsonBytes, "operator approval spec");
            RequireSameOperatorSpec(spec, bound.value);
            return Assemble(bound.value, bound.pin, production: true);
        }

        private static AuditionPvSixtySecondApprovalAssemblyResult AssembleProductionFromPath(
            string specPath)
        {
            RequireAbsoluteSpecPath(specPath);
            PinnedJson<AuditionPvSixtySecondOperatorApprovalSpec> bound =
                ReadJsonSnapshot<AuditionPvSixtySecondOperatorApprovalSpec>(
                    specPath, null, null, MaxJsonBytes, "operator approval spec");
            return Assemble(bound.value, bound.pin, production: true);
        }

        internal static void BindOperatorSpecForTests(
            AuditionPvSixtySecondOperatorApprovalSpec supplied,
            string specPath,
            Action<string> mutationSeam = null)
        {
            RequireAbsoluteSpecPath(specPath);
            PinnedJson<AuditionPvSixtySecondOperatorApprovalSpec> bound =
                ReadJsonSnapshot<AuditionPvSixtySecondOperatorApprovalSpec>(
                    specPath, null, null, MaxJsonBytes, "operator approval spec", mutationSeam);
            RequireSameOperatorSpec(supplied, bound.value);
        }

        internal static void ValidateEdlForTests(
            AuditionPvSixtySecondProductionEdlRow[] rows) => ValidateEdl(rows);

        internal static int[] SampledFramesForTests(int start, int end) =>
            SampledFrames(start, end);

        internal static int[] DeterministicPreviewIndexesForTests(int count, int capacity) =>
            DeterministicPreviewIndexes(count, capacity);

        internal static bool CaptureCardinalityWithinGateForTests(
            AuditionPvCaptureManifest manifest) => CaptureCardinalityWithinGate(manifest);

        internal static string TakeIdForTests(string atomicShotId, string captureId,
            bool cleanPlate) => TakeId(atomicShotId, captureId, cleanPlate);

        internal static string CurrentTwelveSecondHoldForTests(
            AuditionPvSixtySecondCurrentTwelveSecondSpec value) =>
            ValidateCurrentTwelveSecondStatus(value);

        internal static void ValidateHeaderForTests(
            AuditionPvSixtySecondOperatorApprovalSpec value) => ValidateHeader(value);

        internal static bool ReviewRowsMatchSkeletonForTests(
            AuditionPvSixtySecondReviewCriterionSpec[] rows,
            AuditionPvMeasuredFrame[] proposed) => ReviewRowsMatchSkeleton(rows, proposed);

        private static AuditionPvSixtySecondApprovalAssemblyResult Assemble(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            AuditionPvPinnedArtifact specPin,
            bool production)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            ValidateHeader(spec);
            ValidateEdl(spec.edl);
            ValidateSupplementPins(spec, production);
            string reviewOutput = Full(spec.reviewOutputDirectory);
            if (production)
                RequireUnderOrEqual(reviewOutput,
                    AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot,
                    "approval review output");
            RejectReparseChainForExistingParents(reviewOutput);

            List<CaptureSource> captures = LoadCaptures(spec, production);
            List<EvidenceSource> evidence = LoadEvidence(spec, captures, production);
            ValidateEvidenceCoverage(captures, evidence);

            Dictionary<string, AuditionPvSixtySecondAtomicApprovalSpec> approvals =
                IndexApprovals(spec.approvals);
            AuditionPvSixtySecondProductionEdlRow[] sourceRows = spec.edl
                .Where(value => value != null && value.bucketId != "PV_S100").ToArray();
            if (approvals.Count != sourceRows.Length || sourceRows.Any(row =>
                    !approvals.ContainsKey(row.atomicShotId)))
                throw new InvalidDataException(
                    "The operator spec must approve exactly one take for every source atomic shot.");

            var files = new List<PlannedFile>();
            var bindings = new List<AuditionPvSixtySecondTakeEvidenceBinding>();
            var approvedTakes = new List<ApprovedTake>();
            foreach (AuditionPvSixtySecondProductionEdlRow row in sourceRows)
            {
                AuditionPvSixtySecondAtomicApprovalSpec approval = approvals[row.atomicShotId];
                CaptureSource[] candidates = captures.Where(value => value.familyId == row.familyId)
                    .OrderBy(value => value.manifest.captureId, StringComparer.Ordinal).ToArray();
                if (candidates.Length == 0 || !candidates.Any(value =>
                        value.manifest.captureId == approval.approvedSourceCaptureId))
                    throw new InvalidDataException("Approved capture is not a candidate for " +
                                                   row.atomicShotId + ".");
                foreach (CaptureSource candidate in candidates)
                {
                    EvidenceSource bundle = FindEvidence(evidence, candidate,
                        row.sourceShotId, row.sourceRangeStartFrame, row.sourceRangeEndFrame,
                        row.selectStartFrame, row.selectEndFrame);
                    AuditionPvShotAuthorshipArtifact authorship = candidate.Authorship(row.sourceShotId);
                    AuditionPvTakeSemanticProofArtifact semantic = CreateSemanticProof(
                        spec, row, candidate, authorship);
                    string semanticPath = CaptureApprovalPath(candidate, spec.assemblyId,
                        row.atomicShotId, "semantic_proof.json");
                    AuditionPvPinnedArtifact semanticPin = PlanJson(files, semanticPath, semantic);
                    bool approved = candidate.manifest.captureId == approval.approvedSourceCaptureId;
                    AuditionPvPinnedArtifact reviewPin = new();
                    AuditionPvTakeHumanReviewArtifact review = null;
                    if (approved)
                    {
                        review = CreateHumanReview(spec, row, candidate, bundle,
                            approval.review, cleanPlate: false);
                        string reviewPath = ReviewPath(reviewOutput, spec.assemblyId,
                            row.atomicShotId, candidate.manifest.captureId, "human_review.json");
                        reviewPin = PlanJson(files, reviewPath, review);
                    }
                    bindings.Add(new AuditionPvSixtySecondTakeEvidenceBinding
                    {
                        atomicShotId = row.atomicShotId,
                        sourceCaptureId = candidate.manifest.captureId,
                        sourceShotId = row.sourceShotId,
                        approved = approved,
                        cleanPlate = false,
                        sourceFrameLedger = ClonePin(bundle.receipt.sourceFrameLedger),
                        semanticProof = semanticPin,
                        automatedProof = approved
                            ? ClonePin(bundle.receipt.automatedProof) : new AuditionPvPinnedArtifact(),
                        humanReview = reviewPin
                    });
                    if (approved)
                        approvedTakes.Add(new ApprovedTake
                        {
                            row = row,
                            capture = candidate,
                            evidence = bundle,
                            review = review,
                            reviewPin = reviewPin,
                            takeId = TakeId(row.atomicShotId, candidate.manifest.captureId, false)
                        });
                }
            }

            AddCleanPlateBinding(spec, sourceRows, captures, evidence, approvals,
                reviewOutput, files, bindings);

            AuditionPvPinnedArtifact visualPin = BuildVisualReview(spec, approvedTakes,
                reviewOutput, files);
            var holds = new List<string>();
            AuditionPvSixtySecondGateEvidence gate = BuildGateEvidence(spec, captures,
                reviewOutput, files, visualPin, holds, production);
            AuditionPvSixtySecondProductionComposeInput input = BuildComposeInput(
                spec, captures, bindings, gate);
            string inputPath = Path.Combine(reviewOutput, Safe(spec.assemblyId), InputFileName);
            AuditionPvPinnedArtifact inputPin = PlanJson(files, inputPath, input);
            string[] supplemental = MissingSupplemental(input, holds);

            var assemblyReceipt = new AuditionPvSixtySecondApprovalAssemblyReceipt
            {
                schemaVersion = ReceiptSchema,
                status = holds.Count == 0 ? "approval-bindings-ready" :
                    "approval-bindings-ready-current-12s-hold",
                assemblyId = spec.assemblyId,
                operatorReviewedSpec = specPin,
                reviewedBy = spec.reviewedBy,
                reviewedAtUtc = spec.reviewedAtUtc,
                captureManifestCount = captures.Count,
                evidenceBundleReceiptCount = evidence.Count,
                normalTakeBindingCount = bindings.Count(value => !value.cleanPlate),
                approvedTakeCount = bindings.Count(value => value.approved && !value.cleanPlate),
                cleanPlateBindingCount = bindings.Count(value => value.cleanPlate),
                currentTwelveSecondStatus = spec.currentTwelveSecond?.status ?? string.Empty,
                composeInput = inputPin,
                materializedArtifacts = files.Select(value => value.pin).ToArray(),
                holds = holds.ToArray(),
                missingSupplementalRequirements = supplemental
            };
            string receiptPath = Path.Combine(reviewOutput, Safe(spec.assemblyId),
                AssemblyReceiptFileName);
            PlanJson(files, receiptPath, assemblyReceipt);
            PlannedFile visualSheet = files.Single(value => value.role == "visual-contact-sheet");
            VisualFrame[] visualSources = approvedTakes.SelectMany(value =>
                    value.review.reviewedFrames.Select(frame => new
                    VisualFrame
                    {
                        take = value,
                        frame = frame,
                        sourcePath = SourceFramePath(value.capture.manifest,
                            value.row.sourceShotId, frame.sourceFrame)
                    })).ToArray();
            int[] preview = DeterministicPreviewIndexes(visualSources.Length, 32);
            VisualFrame[] selectedVisualSources = preview.Select(index =>
                visualSources[index]).ToArray();
            string[] selectedSources = selectedVisualSources.Select(value =>
                value.sourcePath).ToArray();
            int columns = Math.Min(4, selectedSources.Length);
            int rows = (selectedSources.Length + columns - 1) / columns;
            FreezeExternalInputs(specPin, spec, captures, evidence, production);
            Commit(files, () =>
            {
                FreezeExternalInputs(specPin, spec, captures, evidence, production);
                foreach (VisualFrame selected in selectedVisualSources)
                    VerifyPhysicalFrame(selected.take.capture.manifest,
                        selected.take.row.sourceShotId, selected.frame.sourceFrame,
                        selected.frame.frameSha256);
                if (!AuditionPvSixtySecondGateManifestValidator.ContactSheetMatchesQuarterScale(
                        visualSheet.path, selectedSources, columns, rows))
                    throw new InvalidDataException(
                        "The materialized 25% visual contact sheet failed byte-level verification.");
            });

            return new AuditionPvSixtySecondApprovalAssemblyResult
            {
                composeInputPath = Normalize(inputPath),
                assemblyReceiptPath = Normalize(receiptPath),
                currentTwelveSecondReady = holds.Count == 0,
                fullComposeInputCandidate = supplemental.Length == 0,
                holds = holds.ToArray(),
                missingSupplementalRequirements = supplemental,
                composeInput = input
            };
        }

        private static void ValidateHeader(AuditionPvSixtySecondOperatorApprovalSpec spec)
        {
            if (spec.schemaVersion != SpecSchema)
                throw new InvalidDataException("Operator approval spec schema is invalid.");
            if (string.IsNullOrWhiteSpace(spec.assemblyId) || Safe(spec.assemblyId) != spec.assemblyId)
                throw new InvalidDataException("assemblyId must be one safe path component.");
            if (string.IsNullOrWhiteSpace(spec.reviewedBy) || !Utc(spec.reviewedAtUtc))
                throw new InvalidDataException("A named human reviewer and UTC review time are required.");
            if (spec.judgementOrigin != "human-operator")
                throw new InvalidDataException(
                    "The assembler accepts human-operator judgement only; it never self-approves.");
            if (!spec.allCandidateSemanticTestBindingsReviewed ||
                string.IsNullOrWhiteSpace(spec.semanticEvidenceReviewNote))
                throw new InvalidDataException(
                    "The human operator must explicitly review every candidate semantic-test binding.");
            if (!FullGitSha(spec.productCheckpointGitSha))
                throw new InvalidDataException("Product checkpoint must be a full Git SHA-1.");
            if (string.IsNullOrWhiteSpace(spec.captureRootDirectory) ||
                string.IsNullOrWhiteSpace(spec.reviewOutputDirectory))
                throw new InvalidDataException("Capture and review roots are required.");
        }

        private static void RequireSameOperatorSpec(
            AuditionPvSixtySecondOperatorApprovalSpec supplied,
            AuditionPvSixtySecondOperatorApprovalSpec pinned)
        {
            if (supplied == null || pinned == null ||
                JsonUtility.ToJson(supplied, false) != JsonUtility.ToJson(pinned, false))
                throw new InvalidDataException(
                    "Supplied operator spec object is not the exact JSON identity pinned by specPath.");
        }

        private static void ValidateSupplementPins(
            AuditionPvSixtySecondOperatorApprovalSpec spec, bool production)
        {
            AuditionPvSixtySecondProductionComposeInput supplement =
                spec.composeInputSupplement;
            if (PinShape(supplement?.endCardGraphic))
                VerifyPin(supplement.endCardGraphic,
                    AuditionPvSixtySecondGateManifestValidator.ProductionGraphicsRoot,
                    MaxPngBytes, "supplement end-card graphic", allowAnyRoot: !production);
            if (PinShape(supplement?.gateEvidence?.rightsCoverageReview))
                VerifyPin(supplement.gateEvidence.rightsCoverageReview,
                    AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot,
                    MaxJsonBytes, "supplement rights-coverage review", allowAnyRoot: !production);
        }

        private static void ValidateEdl(AuditionPvSixtySecondProductionEdlRow[] rows)
        {
            AuditionPvSixtySecondProductionEdlRow[] expected =
                AuditionPvSixtySecondProductionComposer.CreateDefaultEdlForTests();
            rows ??= Array.Empty<AuditionPvSixtySecondProductionEdlRow>();
            if (rows.Length != expected.Length)
                throw new InvalidDataException("Operator-reviewed EDL row count drifted.");
            for (int index = 0; index < rows.Length; index++)
            {
                AuditionPvSixtySecondProductionEdlRow actual = rows[index];
                AuditionPvSixtySecondProductionEdlRow wanted = expected[index];
                if (actual == null || wanted == null ||
                    actual.bucketId != wanted.bucketId ||
                    actual.atomicShotId != wanted.atomicShotId ||
                    actual.timelineStartFrame != wanted.timelineStartFrame ||
                    actual.timelineEndFrame != wanted.timelineEndFrame ||
                    actual.familyId != wanted.familyId ||
                    actual.sourceShotId != wanted.sourceShotId ||
                    actual.sourceRangeStartFrame != wanted.sourceRangeStartFrame ||
                    actual.sourceRangeEndFrame != wanted.sourceRangeEndFrame ||
                    actual.selectStartFrame != wanted.selectStartFrame ||
                    actual.selectEndFrame != wanted.selectEndFrame ||
                    actual.handleBeforeFrames != wanted.handleBeforeFrames ||
                    actual.handleAfterFrames != wanted.handleAfterFrames ||
                    !(actual.beatIds ?? Array.Empty<string>()).SequenceEqual(
                        wanted.beatIds ?? Array.Empty<string>(), StringComparer.Ordinal))
                    throw new InvalidDataException("Operator-reviewed EDL drift at row " +
                                                   index.ToString(CultureInfo.InvariantCulture) + ".");
            }
        }

        private static List<CaptureSource> LoadCaptures(
            AuditionPvSixtySecondOperatorApprovalSpec spec, bool production)
        {
            AuditionPvPinnedArtifact[] declared = spec.captureManifests ??
                Array.Empty<AuditionPvPinnedArtifact>();
            if (declared.Length != ExpectedCaptureCount)
                throw new InvalidDataException("Exactly 19 immutable capture manifests are required.");
            string root = Full(spec.captureRootDirectory);
            if (production && !PathsEqual(root, AuditionPvCaptureContract.OutputRoot))
                throw new InvalidDataException("Production capture root is not PREEDIT_GOLD.");
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<CaptureSource>();
            foreach (AuditionPvPinnedArtifact pin in declared)
            {
                PinnedJson<AuditionPvCaptureManifest> pinnedManifest =
                    ReadJsonSnapshot<AuditionPvCaptureManifest>(pin.path, pin.sha256,
                        root, MaxJsonBytes, "capture manifest");
                string path = pinnedManifest.path;
                if (!paths.Add(path)) throw new InvalidDataException("Duplicate capture path.");
                AuditionPvCaptureManifest manifest = pinnedManifest.value;
                if (!CaptureCardinalityWithinGate(manifest))
                    throw new InvalidDataException(
                        "Capture manifest cardinality exceeds the production Gate limits.");
                AuditionPvCaptureManifestWriter.Validate(manifest);
                if (!ids.Add(manifest.captureId))
                    throw new InvalidDataException("Duplicate capture ID.");
                if (manifest.gitWorktreeDirty || manifest.gitCommitSha != spec.productCheckpointGitSha)
                    throw new InvalidDataException(
                        "Capture is dirty or does not match the approved product checkpoint.");
                string canonical = Path.Combine(manifest.outputDirectory,
                    AuditionPvCaptureContract.ManifestFileName);
                if (!PathsEqual(path, canonical))
                    throw new InvalidDataException("Capture manifest is not canonical/direct.");
                RequireUnder(path, root, "capture manifest");
                if ((manifest.testResults ?? Array.Empty<AuditionPvTestResult>())
                    .Any(value => value == null || value.status != "passed"))
                    throw new InvalidDataException("Capture contains a non-passing test.");
                string family = Classify(manifest);
                if (string.IsNullOrEmpty(family))
                    throw new InvalidDataException("Capture does not match a 60-second family.");
                string core = AuditionPvSixtySecondGateManifestValidator.CaptureCoreSha256(manifest);
                var source = new CaptureSource
                {
                    manifest = manifest,
                    manifestPin = ClonePin(pinnedManifest.pin),
                    manifestPath = path,
                    familyId = family,
                    captureCoreSha256 = core,
                    dependencyIdentitySha256 = DependencyIdentity(manifest)
                };
                foreach (AuditionPvShotManifestEntry shot in manifest.shots ??
                         Array.Empty<AuditionPvShotManifestEntry>())
                {
                    if (shot == null) continue;
                    AuditionPvPinnedArtifact authorshipPin = ExactTestArtifact(
                        manifest, EvidenceSuite, "shot-authorship/" + shot.id);
                    AuditionPvShotAuthorshipArtifact authorship = ReadJsonSnapshot<
                        AuditionPvShotAuthorshipArtifact>(authorshipPin.path,
                            authorshipPin.sha256, manifest.outputDirectory, MaxJsonBytes,
                            "shot-authorship artifact").value;
                    if (authorship.schemaVersion != AuditionPvSixtySecondGateManifestValidator
                            .ShotAuthorshipSchema ||
                        authorship.captureId != manifest.captureId ||
                        authorship.sourceCaptureCoreSha256 != core ||
                        authorship.sourceShotId != shot.id ||
                        string.IsNullOrWhiteSpace(authorship.cameraId) ||
                        string.IsNullOrWhiteSpace(authorship.gameplayState) ||
                        string.IsNullOrWhiteSpace(authorship.timelineId) ||
                        authorship.deterministicSeed < 0)
                        throw new InvalidDataException("Shot-authorship artifact is invalid.");
                    source.authorship.Add(shot.id, authorship);
                }
                result.Add(source);
            }
            foreach (FamilyContract family in Families)
            {
                int actual = result.Count(value => value.familyId == family.id);
                if (actual != family.count)
                    throw new InvalidDataException("Capture family count drift for " + family.id + ".");
            }
            return result;
        }

        private static bool CaptureCardinalityWithinGate(
            AuditionPvCaptureManifest manifest) =>
            manifest != null &&
            (manifest.shots ?? Array.Empty<AuditionPvShotManifestEntry>()).Length <=
                MaxCaptureShots &&
            (manifest.baselines ?? Array.Empty<AuditionPvBaselineManifestEntry>()).Length <=
                MaxCaptureBaselines &&
            (manifest.dependencyHashes ?? Array.Empty<AuditionPvDependencyHash>()).Length <=
                MaxCaptureDependencies &&
            (manifest.testResults ?? Array.Empty<AuditionPvTestResult>()).Length <=
                MaxCaptureTestResults;

        private static List<EvidenceSource> LoadEvidence(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            IReadOnlyList<CaptureSource> captures,
            bool production)
        {
            AuditionPvPinnedArtifact[] declared = spec.evidenceBundleReceipts ??
                Array.Empty<AuditionPvPinnedArtifact>();
            if (declared.Length != ExpectedEvidenceReceiptCount)
                throw new InvalidDataException("Exactly 37 range-bound evidence receipts are required.");
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var identities = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<EvidenceSource>();
            foreach (AuditionPvPinnedArtifact pin in declared)
            {
                PinnedJson<AuditionPvSixtySecondEvidenceBundleReceipt> pinnedReceipt =
                    ReadJsonSnapshot<AuditionPvSixtySecondEvidenceBundleReceipt>(
                        pin.path, pin.sha256, spec.captureRootDirectory, MaxJsonBytes,
                        "evidence bundle receipt");
                string path = pinnedReceipt.path;
                if (!paths.Add(path)) throw new InvalidDataException("Duplicate evidence receipt path.");
                AuditionPvSixtySecondEvidenceBundleReceipt receipt = pinnedReceipt.value;
                CaptureSource capture = captures.SingleOrDefault(value =>
                    value.manifest.captureId == receipt.captureId) ??
                    throw new InvalidDataException("Evidence receipt references an unknown capture.");
                RequireUnder(path, capture.manifest.outputDirectory, "evidence bundle receipt");
                string identity = EvidenceIdentity(receipt);
                if (!identities.Add(identity))
                    throw new InvalidDataException("Duplicate evidence range identity.");
                AuditionPvTakeReviewSkeletonArtifact reviewSkeleton =
                    ValidateReceipt(receipt, capture, production);
                result.Add(new EvidenceSource
                {
                    pin = ClonePin(pinnedReceipt.pin), receipt = receipt, capture = capture,
                    reviewSkeleton = reviewSkeleton
                });
            }
            return result;
        }

        private static AuditionPvTakeReviewSkeletonArtifact ValidateReceipt(
            AuditionPvSixtySecondEvidenceBundleReceipt receipt,
            CaptureSource capture, bool production)
        {
            if (receipt == null || receipt.schemaVersion !=
                    AuditionPvSixtySecondEvidenceProducer.ReceiptSchema ||
                receipt.status != "physical-evidence-complete-human-review-required" ||
                receipt.captureId != capture.manifest.captureId ||
                receipt.sourceCaptureCoreSha256 != capture.captureCoreSha256 ||
                receipt.sourceRangeStartFrame < 0 ||
                receipt.sourceRangeEndFrame < receipt.sourceRangeStartFrame ||
                receipt.selectStartFrame < receipt.sourceRangeStartFrame ||
                receipt.selectEndFrame > receipt.sourceRangeEndFrame ||
                receipt.selectEndFrame < receipt.selectStartFrame ||
                receipt.maxSimultaneousDecodedSourcePngs != 1 ||
                receipt.maxTransientWorkingSetBytes !=
                    AuditionPvEvidenceMemoryContract.MaxTransientWorkingSetBytes ||
                receipt.producer != nameof(AuditionPvSixtySecondEvidenceProducer) ||
                receipt.producerVersion != AuditionPvSixtySecondEvidenceProducer.ToolVersion ||
                !Utc(receipt.createdAtUtc))
                throw new InvalidDataException("Evidence receipt envelope is invalid.");
            AuditionPvShotManifestEntry shot = (capture.manifest.shots ??
                    Array.Empty<AuditionPvShotManifestEntry>())
                .SingleOrDefault(value => value != null && value.id == receipt.sourceShotId);
            if (shot == null || receipt.sourceRangeStartFrame < shot.startFrame ||
                receipt.sourceRangeEndFrame > shot.endFrame)
                throw new InvalidDataException("Evidence range is outside its source shot.");

            VerifyPin(receipt.sourceFrameLedger, capture.manifest.outputDirectory,
                64L * 1024L * 1024L, "source frame ledger");
            PinnedJson<AuditionPvTakeAutomatedProofArtifact> automatedSnapshot =
                ReadJsonSnapshot<AuditionPvTakeAutomatedProofArtifact>(
                    receipt.automatedProof.path, receipt.automatedProof.sha256,
                    capture.manifest.outputDirectory, MaxJsonBytes, "automated proof");
            VerifyPin(receipt.rec709Config,
                AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot,
                MaxJsonBytes, "Rec.709 config", allowAnyRoot: !production);
            VerifyPin(receipt.rec709OutputLedger,
                AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot,
                MaxJsonBytes, "Rec.709 ledger", allowAnyRoot: !production);
            VerifyPin(receipt.rendererRuntimeWorkload, capture.manifest.outputDirectory,
                MaxJsonBytes, "renderer workload");
            if (receipt.sourceShotId == "g04-clean")
                VerifyPin(receipt.hudRuntimeWorkload, capture.manifest.outputDirectory,
                    MaxJsonBytes, "clean-plate HUD workload");
            else if (PinShape(receipt.hudRuntimeWorkload))
                throw new InvalidDataException("Non-clean evidence unexpectedly declares HUD-absent proof.");
            string contact = VerifyPin(receipt.contactSheet,
                capture.manifest.outputDirectory,
                MaxPngBytes, "evidence contact sheet");
            PinnedJson<AuditionPvTemporalFilmstripSkeletonArtifact> filmstripSnapshot =
                ReadJsonSnapshot<AuditionPvTemporalFilmstripSkeletonArtifact>(
                    receipt.filmstripSkeleton.path, receipt.filmstripSkeleton.sha256,
                    capture.manifest.outputDirectory, MaxJsonBytes, "filmstrip skeleton");
            PinnedJson<AuditionPvTakeReviewSkeletonArtifact> reviewSnapshot =
                ReadJsonSnapshot<AuditionPvTakeReviewSkeletonArtifact>(
                    receipt.humanReviewSkeleton.path, receipt.humanReviewSkeleton.sha256,
                    production
                        ? AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot
                        : null,
                    MaxJsonBytes, "human review skeleton");

            AuditionPvTemporalFilmstripSkeletonArtifact filmstrip = filmstripSnapshot.value;
            AuditionPvTakeReviewSkeletonArtifact skeleton = reviewSnapshot.value;
            AuditionPvTakeAutomatedProofArtifact automated = automatedSnapshot.value;
            if (automated.schemaVersion != AuditionPvSixtySecondGateManifestValidator
                    .AutomatedProofSchema || automated.captureId != receipt.captureId ||
                automated.sourceCaptureCoreSha256 != receipt.sourceCaptureCoreSha256 ||
                automated.sourceShotId != receipt.sourceShotId || !SameRange(automated, receipt))
                throw new InvalidDataException("Automated-proof identity is invalid.");
            if (filmstrip.schemaVersion != AuditionPvSixtySecondEvidenceProducer
                    .FilmstripSkeletonSchema || !filmstrip.previewOnly ||
                filmstrip.acceptedAsFullRangeScan || filmstrip.captureId != receipt.captureId ||
                filmstrip.sourceCaptureCoreSha256 != receipt.sourceCaptureCoreSha256 ||
                filmstrip.sourceShotId != receipt.sourceShotId ||
                !SameRange(filmstrip, receipt) || !SamePin(filmstrip.contactSheet, receipt.contactSheet))
                throw new InvalidDataException("Filmstrip skeleton identity is invalid.");
            if (skeleton.schemaVersion != AuditionPvSixtySecondEvidenceProducer
                    .ReviewSkeletonSchema || skeleton.approved ||
                skeleton.status != "human-review-required" ||
                skeleton.captureId != receipt.captureId ||
                skeleton.sourceCaptureCoreSha256 != receipt.sourceCaptureCoreSha256 ||
                skeleton.sourceShotId != receipt.sourceShotId || !SameRange(skeleton, receipt) ||
                !SamePin(skeleton.contactSheet, receipt.contactSheet) ||
                !SamePin(skeleton.filmstripSkeleton, receipt.filmstripSkeleton) ||
                !(skeleton.requiredDecisions ?? Array.Empty<string>()).Contains(
                    "full-motion-range-reviewed", StringComparer.Ordinal) ||
                !(skeleton.requiredDecisions ?? Array.Empty<string>()).Contains(
                    "no-black-mesh", StringComparer.Ordinal) ||
                !(skeleton.requiredDecisions ?? Array.Empty<string>()).Contains(
                    "no-broken-trail", StringComparer.Ordinal))
                throw new InvalidDataException("Human-review skeleton identity is invalid.");

            ValidateMeasuredFrames(filmstrip.orderedFrames,
                SampledFrames(receipt.selectStartFrame, receipt.selectEndFrame),
                capture.manifest, receipt.sourceShotId, "filmstrip");
            ValidateMeasuredFrames(skeleton.proposedFullRangeFrames,
                SampledFrames(receipt.sourceRangeStartFrame, receipt.sourceRangeEndFrame),
                capture.manifest, receipt.sourceShotId, "review skeleton");
            string[] sources = (filmstrip.orderedFrames ?? Array.Empty<AuditionPvMeasuredFrame>())
                .Select(frame => SourceFramePath(capture.manifest, receipt.sourceShotId,
                    frame.sourceFrame)).ToArray();
            int columns = Math.Min(4, sources.Length);
            int rows = sources.Length == 0 ? 0 : (sources.Length + columns - 1) / columns;
            VerifyStableFileWhile(receipt.contactSheet, capture.manifest.outputDirectory,
                MaxPngBytes, "evidence contact sheet", () =>
                {
                    if (!AuditionPvSixtySecondGateManifestValidator
                            .ContactSheetMatchesQuarterScale(contact, sources, columns, rows))
                        throw new InvalidDataException(
                            "Evidence contact sheet is not the pinned quarter-scale media.");
                });

            string[] expectedChecks = receipt.sourceShotId == "g04-clean"
                ? new[] { "contact-sheet", "missing-frame", "error-magenta", "resolution",
                    "rec709", "renderer-material-scan", "hud-layer-absent" }
                : new[] { "contact-sheet", "missing-frame", "error-magenta", "resolution",
                    "rec709", "renderer-material-scan" };
            AuditionPvTestResult[] generatedTests = receipt.generatedTestResults ??
                Array.Empty<AuditionPvTestResult>();
            string[] mandatoryTests = expectedChecks.Concat(new[]
                { "renderer-material-scan/runtime-workload" })
                .Concat(receipt.sourceShotId == "g04-clean"
                    ? new[] { "hud-layer-absent/runtime-workload" }
                    : Array.Empty<string>()).ToArray();
            var generatedNames = new HashSet<string>(generatedTests
                .Where(value => value != null).Select(value => value.name), StringComparer.Ordinal);
            var allowedTests = new HashSet<string>(mandatoryTests.Concat(
                receipt.sourceShotId == "g04-clean"
                    ? new[] { "hud-layer-absent/scene-contract-no-hud" }
                    : Array.Empty<string>()), StringComparer.Ordinal);
            if (generatedTests.Any(value => value == null) ||
                generatedNames.Count != generatedTests.Length ||
                mandatoryTests.Any(value => !generatedNames.Contains(value)) ||
                generatedNames.Any(value => !allowedTests.Contains(value)))
                throw new InvalidDataException("Evidence receipt generated-test set is incomplete.");
            AuditionPvNamedPinnedArtifact[] checks = receipt.checkResults ??
                Array.Empty<AuditionPvNamedPinnedArtifact>();
            if (!checks.Select(value => value?.id)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(expectedChecks.OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal))
                throw new InvalidDataException("Evidence receipt check-result set is incomplete.");
            AuditionPvAutomatedCheckEvidence[] automatedChecks = automated.checks ??
                Array.Empty<AuditionPvAutomatedCheckEvidence>();
            if (!automatedChecks.Select(value => value?.id)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(expectedChecks.OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal) || automatedChecks.Any(value => value == null ||
                        value.status != "passed" || value.supportingTestSuite != EvidenceSuite ||
                        value.supportingTestName != value.id))
                throw new InvalidDataException("Automated-proof check set is incomplete.");
            foreach (AuditionPvNamedPinnedArtifact check in checks)
                VerifyPin(check.artifact, capture.manifest.outputDirectory, MaxJsonBytes,
                    "evidence check result");
            foreach (AuditionPvAutomatedCheckEvidence check in automatedChecks)
            {
                AuditionPvNamedPinnedArtifact named = checks.Single(value => value.id == check.id);
                if (!SamePin(check.artifact, named.artifact))
                    throw new InvalidDataException("Automated/check receipt pins disagree.");
            }

            foreach (AuditionPvTestResult generated in generatedTests)
            {
                AuditionPvTestResult captured = (capture.manifest.testResults ??
                        Array.Empty<AuditionPvTestResult>())
                    .SingleOrDefault(value => SameTestIdentity(value, generated));
                if (captured == null || captured.details != generated.details ||
                    captured.status != "passed")
                    throw new InvalidDataException("Generated evidence test is absent from capture manifest.");
                AuditionPvPinnedArtifact actual = DeclaredTestArtifact(capture.manifest, captured);
                if (!ContainsToken(generated.details, "artifact-sha256=" + actual.sha256))
                    throw new InvalidDataException("Evidence test does not pin its exact artifact bytes.");
            }
            return skeleton;
        }

        private static void ValidateEvidenceCoverage(IReadOnlyList<CaptureSource> captures,
            IReadOnlyList<EvidenceSource> evidence)
        {
            AuditionPvSixtySecondProductionEdlRow[] rows =
                AuditionPvSixtySecondProductionComposer.CreateDefaultEdlForTests()
                    .Where(value => value.bucketId != "PV_S100").ToArray();
            var expected = new HashSet<string>(StringComparer.Ordinal);
            foreach (CaptureSource capture in captures)
            {
                foreach (AuditionPvSixtySecondProductionEdlRow row in rows.Where(value =>
                             value.familyId == capture.familyId))
                    expected.Add(EvidenceIdentity(capture.manifest.captureId, row.sourceShotId,
                        row.sourceRangeStartFrame, row.sourceRangeEndFrame,
                        row.selectStartFrame, row.selectEndFrame));
                if (capture.familyId == "station-g04")
                {
                    AuditionPvSixtySecondProductionEdlRow eye = rows.Single(value =>
                        value.atomicShotId == "pv-s060-eye-open");
                    expected.Add(EvidenceIdentity(capture.manifest.captureId, "g04-clean",
                        eye.sourceRangeStartFrame, eye.sourceRangeEndFrame,
                        eye.selectStartFrame, eye.selectEndFrame));
                }
            }
            string[] actual = evidence.Select(value => EvidenceIdentity(value.receipt))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (!expected.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(actual, StringComparer.Ordinal))
                throw new InvalidDataException(
                    "Evidence receipts do not exactly cover the composer EDL candidate ranges.");
        }

        private static Dictionary<string, AuditionPvSixtySecondAtomicApprovalSpec> IndexApprovals(
            AuditionPvSixtySecondAtomicApprovalSpec[] values)
        {
            values ??= Array.Empty<AuditionPvSixtySecondAtomicApprovalSpec>();
            var result = new Dictionary<string, AuditionPvSixtySecondAtomicApprovalSpec>(
                StringComparer.Ordinal);
            foreach (AuditionPvSixtySecondAtomicApprovalSpec value in values)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.atomicShotId) ||
                    string.IsNullOrWhiteSpace(value.approvedSourceCaptureId) ||
                    !result.TryAdd(value.atomicShotId, value))
                    throw new InvalidDataException("Atomic approval rows are null, incomplete, or duplicated.");
            }
            return result;
        }

        private static AuditionPvTakeSemanticProofArtifact CreateSemanticProof(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            AuditionPvSixtySecondProductionEdlRow row,
            CaptureSource capture,
            AuditionPvShotAuthorshipArtifact authorship)
        {
            AuditionPvSemanticBeatProof[] beats = (row.beatIds ?? Array.Empty<string>())
                .Select(beatId =>
                {
                    AuditionPvPinnedArtifact artifact = ExactTestArtifact(capture.manifest,
                        EvidenceSuite, "semantic-beat/" + beatId, "semantic-fact=" + beatId);
                    return new AuditionPvSemanticBeatProof
                    {
                        beatId = beatId,
                        verifiedBy = spec.reviewedBy,
                        verifiedAtUtc = spec.reviewedAtUtc,
                        supportingTestSuite = EvidenceSuite,
                        supportingTestName = "semantic-beat/" + beatId,
                        runtimeFactKey = beatId,
                        runtimeProof = artifact
                    };
                }).ToArray();
            if (!AuditionPvSixtySecondGateManifestValidator.SemanticBeatProofSetValid(
                    beats, row.beatIds))
                throw new InvalidDataException("Semantic beat proof set is incomplete.");
            return BindRange(new AuditionPvTakeSemanticProofArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.SemanticProofSchema,
                captureId = capture.manifest.captureId,
                sourceManifestSha256 = capture.manifestPin.sha256,
                sourceShotId = row.sourceShotId,
                bucketId = row.bucketId,
                atomicShotId = row.atomicShotId,
                scenePath = capture.Shot(row.sourceShotId).scenePath,
                cameraId = authorship.cameraId,
                gameplayState = authorship.gameplayState,
                timelineId = authorship.timelineId,
                deterministicSeed = authorship.deterministicSeed,
                beatIds = (row.beatIds ?? Array.Empty<string>()).ToArray(),
                beatProofs = beats
            }, row);
        }

        private static AuditionPvTakeHumanReviewArtifact CreateHumanReview(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            AuditionPvSixtySecondProductionEdlRow row,
            CaptureSource capture,
            EvidenceSource bundle,
            AuditionPvSixtySecondTakeReviewDecisionSpec decision,
            bool cleanPlate)
        {
            if (decision == null || !decision.approved ||
                !decision.fullMotionRangeReviewed || !decision.noBlackMesh ||
                !decision.noBrokenTrail)
                throw new InvalidDataException("Human review decisions must be explicitly true.");
            AuditionPvTakeReviewSkeletonArtifact skeleton = bundle.reviewSkeleton ??
                throw new InvalidDataException("Validated human-review skeleton is absent.");
            AuditionPvSixtySecondReviewCriterionSpec[] rows = decision.criteria ??
                Array.Empty<AuditionPvSixtySecondReviewCriterionSpec>();
            AuditionPvMeasuredFrame[] proposed = skeleton.proposedFullRangeFrames ??
                Array.Empty<AuditionPvMeasuredFrame>();
            if (!ReviewRowsMatchSkeleton(rows, proposed))
                throw new InvalidDataException(
                    "Every deterministic skeleton frame needs one explicit criterion/note row.");
            foreach (AuditionPvMeasuredFrame frame in proposed)
            {
                VerifyPhysicalFrame(capture.manifest, bundle.receipt.sourceShotId,
                    frame.sourceFrame, frame.frameSha256);
            }
            string takeId = TakeId(row.atomicShotId, capture.manifest.captureId, cleanPlate);
            return BindRange(new AuditionPvTakeHumanReviewArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.TakeReviewSchema,
                takeId = takeId,
                captureId = capture.manifest.captureId,
                sourceManifestSha256 = capture.manifestPin.sha256,
                sourceShotId = bundle.receipt.sourceShotId,
                bucketId = row.bucketId,
                atomicShotId = row.atomicShotId,
                beatIds = (row.beatIds ?? Array.Empty<string>()).ToArray(),
                approved = true,
                fullMotionRangeReviewed = true,
                noBlackMesh = true,
                noBrokenTrail = true,
                reviewedBy = spec.reviewedBy,
                reviewedAtUtc = spec.reviewedAtUtc,
                reviewedFrames = proposed.Select(value => new AuditionPvMeasuredFrame
                {
                    sourceFrame = value.sourceFrame,
                    frameSha256 = value.frameSha256
                }).ToArray()
            }, row);
        }

        private static bool ReviewRowsMatchSkeleton(
            AuditionPvSixtySecondReviewCriterionSpec[] rows,
            AuditionPvMeasuredFrame[] proposed)
        {
            rows ??= Array.Empty<AuditionPvSixtySecondReviewCriterionSpec>();
            proposed ??= Array.Empty<AuditionPvMeasuredFrame>();
            if (rows.Length == 0 || rows.Length != proposed.Length ||
                rows.Any(value => value == null) || proposed.Any(value => value == null) ||
                rows.Select(value => value.sourceFrame).Distinct().Count() != rows.Length)
                return false;
            Dictionary<int, AuditionPvSixtySecondReviewCriterionSpec> byFrame =
                rows.ToDictionary(value => value.sourceFrame);
            return proposed.All(frame => byFrame.TryGetValue(frame.sourceFrame,
                    out AuditionPvSixtySecondReviewCriterionSpec criterion) &&
                criterion.frameSha256 == frame.frameSha256 &&
                AuditionPvSha256.IsSha256(criterion.frameSha256) &&
                !string.IsNullOrWhiteSpace(criterion.criterion) &&
                !string.IsNullOrWhiteSpace(criterion.note));
        }

        private static void AddCleanPlateBinding(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            IReadOnlyList<AuditionPvSixtySecondProductionEdlRow> sourceRows,
            IReadOnlyList<CaptureSource> captures,
            IReadOnlyList<EvidenceSource> evidence,
            IReadOnlyDictionary<string, AuditionPvSixtySecondAtomicApprovalSpec> approvals,
            string reviewOutput,
            ICollection<PlannedFile> files,
            ICollection<AuditionPvSixtySecondTakeEvidenceBinding> bindings)
        {
            AuditionPvSixtySecondCleanPlateApprovalSpec clean = spec.cleanPlate ??
                throw new InvalidDataException("One explicit clean-plate approval is required.");
            AuditionPvSixtySecondProductionEdlRow row = sourceRows.Single(value =>
                value.atomicShotId == "pv-s060-eye-open");
            if (clean.atomicShotId != row.atomicShotId ||
                string.IsNullOrWhiteSpace(clean.sourceCaptureId))
                throw new InvalidDataException("Clean plate must bind the S060 eye-open atomic shot.");
            AuditionPvSixtySecondAtomicApprovalSpec referenceApproval = approvals[row.atomicShotId];
            if (clean.referenceApprovedSourceCaptureId !=
                    referenceApproval.approvedSourceCaptureId ||
                clean.sourceCaptureId != referenceApproval.approvedSourceCaptureId)
                throw new InvalidDataException(
                    "The clean plate must be the explicitly linked companion from the approved capture.");
            CaptureSource capture = captures.Single(value =>
                value.manifest.captureId == clean.sourceCaptureId &&
                value.familyId == "station-g04");
            EvidenceSource bundle = FindEvidence(evidence, capture, "g04-clean",
                row.sourceRangeStartFrame, row.sourceRangeEndFrame,
                row.selectStartFrame, row.selectEndFrame);
            AuditionPvSixtySecondTakeEvidenceBinding reference = bindings.Single(value =>
                value.atomicShotId == row.atomicShotId && value.approved && !value.cleanPlate);
            AuditionPvShotAuthorshipArtifact cleanAuthorship = capture.Authorship("g04-clean");
            AuditionPvShotAuthorshipArtifact mainAuthorship = capture.Authorship(row.sourceShotId);
            if (cleanAuthorship.cameraId != mainAuthorship.cameraId ||
                cleanAuthorship.gameplayState != mainAuthorship.gameplayState ||
                cleanAuthorship.timelineId != mainAuthorship.timelineId ||
                cleanAuthorship.deterministicSeed != mainAuthorship.deterministicSeed)
                throw new InvalidDataException("Clean-plate direction metadata drifted from its reference.");

            string referenceTakeId = TakeId(row.atomicShotId, capture.manifest.captureId, false);
            AuditionPvCleanPlateCompanionProofArtifact proof = BindRange(
                new AuditionPvCleanPlateCompanionProofArtifact
                {
                    schemaVersion = AuditionPvSixtySecondGateManifestValidator.CleanPlateProofSchema,
                    captureId = capture.manifest.captureId,
                    sourceManifestSha256 = capture.manifestPin.sha256,
                    sourceShotId = "g04-clean",
                    bucketId = row.bucketId,
                    atomicShotId = row.atomicShotId,
                    referenceTakeId = referenceTakeId,
                    referenceCaptureId = capture.manifest.captureId,
                    referenceSourceManifestSha256 = capture.manifestPin.sha256,
                    referenceSourceShotId = row.sourceShotId,
                    referenceFrameLedgerSha256 = reference.sourceFrameLedger.sha256,
                    referenceSourceRangeStartFrame = row.sourceRangeStartFrame,
                    referenceSourceRangeEndFrame = row.sourceRangeEndFrame,
                    referenceSelectStartFrame = row.selectStartFrame,
                    referenceSelectEndFrame = row.selectEndFrame,
                    scenePath = capture.Shot("g04-clean").scenePath,
                    cameraId = cleanAuthorship.cameraId,
                    gameplayState = cleanAuthorship.gameplayState,
                    timelineId = cleanAuthorship.timelineId,
                    deterministicSeed = cleanAuthorship.deterministicSeed,
                    proofTool = nameof(AuditionPvSixtySecondApprovalAssembler),
                    createdAtUtc = spec.reviewedAtUtc
                }, row);
            string proofPath = CaptureApprovalPath(capture, spec.assemblyId,
                row.atomicShotId, "clean_plate_proof.json");
            AuditionPvPinnedArtifact proofPin = PlanJson(files, proofPath, proof);
            AuditionPvTakeHumanReviewArtifact review = CreateHumanReview(spec, row, capture,
                bundle, clean.review, cleanPlate: true);
            string reviewPath = ReviewPath(reviewOutput, spec.assemblyId,
                row.atomicShotId, capture.manifest.captureId, "clean_human_review.json");
            AuditionPvPinnedArtifact reviewPin = PlanJson(files, reviewPath, review);
            bindings.Add(new AuditionPvSixtySecondTakeEvidenceBinding
            {
                atomicShotId = row.atomicShotId,
                sourceCaptureId = capture.manifest.captureId,
                sourceShotId = "g04-clean",
                approved = false,
                cleanPlate = true,
                sourceFrameLedger = ClonePin(bundle.receipt.sourceFrameLedger),
                cleanPlateProof = proofPin,
                automatedProof = ClonePin(bundle.receipt.automatedProof),
                humanReview = reviewPin
            });
        }

        private static AuditionPvPinnedArtifact BuildVisualReview(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            IReadOnlyList<ApprovedTake> approvedTakes,
            string reviewOutput,
            ICollection<PlannedFile> files)
        {
            AuditionPvSixtySecondVisualReviewDecisionSpec decision = spec.visualReview ??
                throw new InvalidDataException("An explicit 25% visual-review decision is required.");
            if (!decision.approved || !decision.faceReadable || !decision.bossReadable ||
                !decision.attackDirectionReadable || !decision.impactPointReadable ||
                !decision.noPinkShader || !decision.noErrorMagenta ||
                !decision.noNullMaterial || !decision.noBlackMesh || !decision.noBrokenTrail)
                throw new InvalidDataException(
                    "The assembler cannot turn a failed or incomplete visual judgement into approval.");
            if (approvedTakes.Count != 13)
                throw new InvalidDataException("Exactly 13 approved moving-image atomic takes are required.");

            var all = new List<VisualFrame>();
            foreach (ApprovedTake take in approvedTakes)
                foreach (AuditionPvMeasuredFrame frame in take.review.reviewedFrames ??
                         Array.Empty<AuditionPvMeasuredFrame>())
                    all.Add(new VisualFrame
                    {
                        take = take,
                        frame = frame,
                        sourcePath = SourceFramePath(take.capture.manifest,
                            take.row.sourceShotId, frame.sourceFrame)
                    });
            int[] indexes = DeterministicPreviewIndexes(all.Count, 32);
            VisualFrame[] preview = indexes.Select(index => all[index]).ToArray();
            if (preview.Length == 0)
                throw new InvalidDataException("Visual review has no deterministic preview frames.");
            byte[] sheetBytes = CreateQuarterScaleContactSheet(preview);
            string sheetPath = Path.Combine(reviewOutput, Safe(spec.assemblyId),
                "visual_review_contact_sheet_q25.png");
            AuditionPvPinnedArtifact sheetPin = PlanBytes(files, sheetPath, sheetBytes,
                "visual-contact-sheet");

            AuditionPvSixtySecondVisualCriterionSpec[] criteria = decision.criterionRefs ??
                Array.Empty<AuditionPvSixtySecondVisualCriterionSpec>();
            string[] required = { "face", "boss", "attack-direction", "impact-point" };
            if (criteria.Length != required.Length ||
                !criteria.Select(value => value?.criterion)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(required.OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal))
                throw new InvalidDataException("Visual review requires exactly four named criteria.");
            var refs = new List<AuditionPvVisualCriterionRef>();
            foreach (AuditionPvSixtySecondVisualCriterionSpec criterion in criteria)
            {
                ApprovedTake take = approvedTakes.SingleOrDefault(value =>
                    value.row.atomicShotId == criterion.atomicShotId) ??
                    throw new InvalidDataException("Visual criterion references a non-approved atomic shot.");
                VisualFrame cell = preview.SingleOrDefault(value =>
                    value.take.takeId == take.takeId &&
                    value.frame.sourceFrame == criterion.sourceFrame &&
                    value.frame.frameSha256 == criterion.frameSha256) ??
                    throw new InvalidDataException(
                        "Visual criterion must reference an exact deterministic contact-sheet cell.");
                if (string.IsNullOrWhiteSpace(criterion.note))
                    throw new InvalidDataException("Visual criterion note is required.");
                AuditionPvSixtySecondReviewCriterionSpec reviewed =
                    spec.approvals.Single(value => value.atomicShotId == criterion.atomicShotId)
                        .review.criteria.SingleOrDefault(value => value != null &&
                            value.sourceFrame == criterion.sourceFrame &&
                            value.frameSha256 == criterion.frameSha256) ??
                    throw new InvalidDataException(
                        "Visual criterion is not part of the operator's exact take review.");
                if (string.IsNullOrWhiteSpace(reviewed.note))
                    throw new InvalidDataException("Referenced take-review note is empty.");
                var criterionRef = new AuditionPvVisualCriterionRef
                {
                    criterion = criterion.criterion,
                    takeId = take.takeId,
                    atomicShotId = criterion.atomicShotId,
                    sourceFrame = cell.frame.sourceFrame,
                    frameSha256 = cell.frame.frameSha256,
                    note = criterion.note
                };
                var gateTake = new AuditionPvSixtySecondTakeCandidate
                {
                    takeId = take.takeId,
                    selectStartFrame = take.row.selectStartFrame,
                    selectEndFrame = take.row.selectEndFrame
                };
                var gateShot = new AuditionPvSixtySecondAtomicShot
                {
                    shotId = take.row.atomicShotId,
                    beatIds = (take.row.beatIds ?? Array.Empty<string>()).ToArray()
                };
                if (!AuditionPvSixtySecondGateManifestValidator.VisualCriterionRefMatches(
                        criterionRef, gateTake, gateShot, take.review.reviewedFrames))
                    throw new InvalidDataException(
                        "Visual criterion is not relevant to the referenced atomic shot.");
                refs.Add(criterionRef);
            }

            int columns = Math.Min(4, preview.Length);
            int rows = (preview.Length + columns - 1) / columns;
            var artifact = new AuditionPvVisualReviewArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator.VisualReviewSchema,
                manifestId = "dimension-brawl-audition-pv-60s-preedit",
                productCheckpointGitSha = spec.productCheckpointGitSha,
                reviewedBy = spec.reviewedBy,
                reviewedAtUtc = spec.reviewedAtUtc,
                approved = true,
                downscalePercent = 25,
                faceReadable = true,
                bossReadable = true,
                attackDirectionReadable = true,
                impactPointReadable = true,
                noPinkShader = true,
                noErrorMagenta = true,
                noNullMaterial = true,
                noBlackMesh = true,
                noBrokenTrail = true,
                endCardLogoReadable = false,
                endCardSloganReadable = false,
                endCardAuditionNoticeReadable = false,
                contactSheet = sheetPin,
                contactSheetColumns = columns,
                contactSheetRows = rows,
                contactSheetCellCount = preview.Length,
                contactSheetGenerator = "AuditionPvQuarterScaleContactSheet",
                contactSheetGeneratorVersion = "nearest-rgba32-bottom-left-v1",
                contactSheetInputSha256 = preview.Select(value => value.frame.frameSha256).ToArray(),
                approvedTakeReviewSha256 = approvedTakes.Select(value => value.reviewPin.sha256)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                approvedEndCardGraphicSha256 = Array.Empty<string>(),
                reviewedFrameSha256 = preview.Select(value => value.frame.frameSha256)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                criterionRefs = refs.ToArray()
            };
            string path = Path.Combine(reviewOutput, Safe(spec.assemblyId),
                "visual_review_25pct.json");
            return PlanJson(files, path, artifact);
        }

        private static AuditionPvSixtySecondGateEvidence BuildGateEvidence(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            IReadOnlyList<CaptureSource> captures,
            string reviewOutput,
            ICollection<PlannedFile> files,
            AuditionPvPinnedArtifact visualPin,
            ICollection<string> holds,
            bool production)
        {
            var result = new AuditionPvSixtySecondGateEvidence
            {
                visualReview = visualPin,
                rightsCoverageReview = ClonePin(
                    spec.composeInputSupplement?.gateEvidence?.rightsCoverageReview)
            };
            string hold = ValidateCurrentTwelveSecondStatus(spec.currentTwelveSecond);
            if (!string.IsNullOrEmpty(hold))
            {
                holds.Add(hold);
                return result;
            }
            AuditionPvSixtySecondCurrentTwelveSecondSpec current = spec.currentTwelveSecond;
            string package = Full(current.packageDirectory);
            if (production)
                RequireUnder(package, AuditionPvTwelveSecondGoldAssembler.OutputRoot,
                    "current 12-second package");
            string manifestPath = Path.Combine(package,
                AuditionPvTwelveSecondGoldAssembler.ManifestFileName);
            string validationPath = Path.Combine(package,
                AuditionPvTwelveSecondGoldAssembler.ValidationReportFileName);
            PinnedJson<AuditionPvTwelveSecondSelectManifest> manifestSnapshot =
                ReadJsonSnapshot<AuditionPvTwelveSecondSelectManifest>(manifestPath,
                    current.manifestSha256, package, MaxJsonBytes,
                    "current 12-second manifest");
            PinnedJson<AuditionPvTwelveSecondValidationReport> validationSnapshot =
                ReadJsonSnapshot<AuditionPvTwelveSecondValidationReport>(validationPath,
                    current.validationSha256, package, MaxJsonBytes,
                    "current 12-second validation");
            AuditionPvTwelveSecondGoldAssembler.ValidateInstalledPackage(package);
            VerifySnapshotStillCurrent(manifestSnapshot.snapshot,
                "current 12-second manifest after package validation");
            VerifySnapshotStillCurrent(validationSnapshot.snapshot,
                "current 12-second validation after package validation");
            AuditionPvTwelveSecondSelectManifest twelve = manifestSnapshot.value;
            AuditionPvTwelveSecondValidationReport validation = validationSnapshot.value;
            if (!validation.passed || validation.manifestSha256 != current.manifestSha256 ||
                !PathsEqual(validation.outputDirectory, package) ||
                twelve.outputDirectory == null || !PathsEqual(twelve.outputDirectory, package) ||
                twelve.totalFrames != AuditionPvTwelveSecondGoldAssembler.ExpectedFrameCount)
                throw new InvalidDataException("Current 12-second validation identity is invalid.");

            AuditionPvTwelveSecondSourceLedgerSpec[] declared = current.sourceLedgers ??
                Array.Empty<AuditionPvTwelveSecondSourceLedgerSpec>();
            if (declared.Length != AuditionPvTwelveSecondGoldAssembler.RequiredRoles.Length)
                throw new InvalidDataException("Current 12-second source-ledger count is invalid.");
            var bindings = new List<AuditionPvTwelveSecondSourceFrameLedgerBinding>();
            foreach (AuditionPvTwelveSecondSelectSegment segment in
                     (twelve.segments ?? Array.Empty<AuditionPvTwelveSecondSelectSegment>())
                     .OrderBy(value => value.order))
            {
                AuditionPvTwelveSecondSourceLedgerSpec declaredBinding = declared.SingleOrDefault(
                    value => value != null && value.segmentOrder == segment.order) ??
                    throw new InvalidDataException("Missing current 12-second source-ledger row.");
                AuditionPvTwelveSecondSourceManifestIdentity source =
                    (twelve.sourceManifests ??
                        Array.Empty<AuditionPvTwelveSecondSourceManifestIdentity>())
                    .Single(value => value.captureId == segment.sourceCaptureId);
                CaptureSource capture = captures.SingleOrDefault(value =>
                    value.manifest.captureId == segment.sourceCaptureId) ??
                    throw new InvalidDataException(
                        "Current 12-second package is not sourced from the approved 19 captures.");
                if (declaredBinding.sourceCaptureId != segment.sourceCaptureId ||
                    declaredBinding.sourceShotId != segment.sourceShotId ||
                    declaredBinding.sourceManifestSha256 != source.manifestSha256 ||
                    declaredBinding.sourceManifestSha256 != capture.manifestPin.sha256 ||
                    declaredBinding.sourceDependencyIdentitySha256 != source.dependencyIdentitySha256 ||
                    declaredBinding.sourceDependencyIdentitySha256 !=
                        capture.dependencyIdentitySha256)
                    throw new InvalidDataException("Current 12-second source-ledger identity drifted.");
                FileSnapshot ledgerSnapshot = ReadFileSnapshot(
                    declaredBinding.frameLedger.path, declaredBinding.frameLedger.sha256,
                    capture.manifest.outputDirectory, 64L * 1024L * 1024L,
                    "current 12-second source ledger");
                Dictionary<string, string> ledger = ReadLedger(ledgerSnapshot.bytes);
                foreach (AuditionPvTwelveSecondFrameMapping mapping in
                         (twelve.frames ?? Array.Empty<AuditionPvTwelveSecondFrameMapping>())
                         .Where(value => value != null && value.segmentOrder == segment.order))
                {
                    int expectedFrame = checked(segment.sourceStartFrame + mapping.selectFrame -
                                                segment.selectStartFrame);
                    string relative = SourceFrameRelative(segment.sourceShotId, expectedFrame);
                    if (mapping.sourceCaptureId != capture.manifest.captureId ||
                        mapping.sourceManifestSha256 != capture.manifestPin.sha256 ||
                        mapping.sourceDependencyIdentitySha256 != capture.dependencyIdentitySha256 ||
                        mapping.sourceShotId != segment.sourceShotId ||
                        mapping.sourceFrame != expectedFrame ||
                        Normalize(mapping.sourceRelativePath) != relative ||
                        !(ledger.TryGetValue(relative, out string ledgerHash) ||
                          ledger.TryGetValue(Path.GetFileName(relative), out ledgerHash)) ||
                        ledgerHash != mapping.sha256)
                        throw new InvalidDataException("Current 12-second frame mapping drifted.");
                    VerifyPhysicalFrame(capture.manifest, segment.sourceShotId,
                        expectedFrame, mapping.sha256);
                }
                bindings.Add(new AuditionPvTwelveSecondSourceFrameLedgerBinding
                {
                    segmentOrder = segment.order,
                    sourceCaptureId = capture.manifest.captureId,
                    sourceManifestSha256 = capture.manifestPin.sha256,
                    sourceDependencyIdentitySha256 = capture.dependencyIdentitySha256,
                    sourceShotId = segment.sourceShotId,
                    frameLedger = ClonePin(declaredBinding.frameLedger)
                });
            }

            if (!current.approved || string.IsNullOrWhiteSpace(current.approvedBy) ||
                !Utc(current.approvedAtUtc) ||
                string.IsNullOrWhiteSpace(current.sourceLedgerIdentityReviewNote))
                throw new InvalidDataException(
                    "Current 12-second package needs its own explicit human approval.");
            var approval = new AuditionPvTwelveSecondApprovalArtifact
            {
                schemaVersion = AuditionPvSixtySecondGateManifestValidator
                    .TwelveSecondApprovalSchema,
                manifestId = "dimension-brawl-audition-pv-60s-preedit",
                twelveSecondManifestSha256 = current.manifestSha256,
                approved = true,
                approvedBy = current.approvedBy,
                approvedAtUtc = current.approvedAtUtc
            };
            string approvalPath = Path.Combine(reviewOutput, Safe(spec.assemblyId),
                "current_12s_approval.json");
            AuditionPvPinnedArtifact approvalPin = PlanJson(files, approvalPath, approval);
            result.twelveSecondPackageDirectory = Normalize(package);
            result.twelveSecondManifestSha256 = current.manifestSha256;
            result.twelveSecondValidationSha256 = current.validationSha256;
            result.twelveSecondApproval = approvalPin;
            result.twelveSecondSourceFrameLedgers = bindings.ToArray();
            return result;
        }

        private static string ValidateCurrentTwelveSecondStatus(
            AuditionPvSixtySecondCurrentTwelveSecondSpec value)
        {
            if (value == null) return "CURRENT_12S_HOLD:spec-missing";
            if (value.status == "hold")
            {
                if (string.IsNullOrWhiteSpace(value.holdReason))
                    throw new InvalidDataException("Current 12-second HOLD requires a reason.");
                return "CURRENT_12S_HOLD:" + value.holdReason.Trim();
            }
            if (value.status != "ready")
                throw new InvalidDataException("Current 12-second status must be ready or hold.");
            if (string.IsNullOrWhiteSpace(value.packageDirectory) ||
                !AuditionPvSha256.IsSha256(value.manifestSha256) ||
                !AuditionPvSha256.IsSha256(value.validationSha256))
                throw new InvalidDataException("Ready current 12-second pins are incomplete.");
            return string.Empty;
        }

        private static AuditionPvSixtySecondProductionComposeInput BuildComposeInput(
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            IReadOnlyList<CaptureSource> captures,
            IReadOnlyList<AuditionPvSixtySecondTakeEvidenceBinding> bindings,
            AuditionPvSixtySecondGateEvidence gate)
        {
            AuditionPvSixtySecondProductionComposeInput supplement =
                spec.composeInputSupplement ?? new AuditionPvSixtySecondProductionComposeInput();
            return new AuditionPvSixtySecondProductionComposeInput
            {
                schemaVersion = AuditionPvSixtySecondProductionComposer.InputSchema,
                productCheckpointGitSha = spec.productCheckpointGitSha,
                captureManifestPaths = captures.OrderBy(value => value.manifest.captureId,
                        StringComparer.Ordinal)
                    .Select(value => Normalize(value.manifestPath)).ToArray(),
                takeEvidence = bindings.OrderBy(value => value.atomicShotId, StringComparer.Ordinal)
                    .ThenBy(value => value.cleanPlate)
                    .ThenBy(value => value.sourceCaptureId, StringComparer.Ordinal).ToArray(),
                shotReferences = supplement.shotReferences ??
                    Array.Empty<AuditionPvSixtySecondShotReferenceBinding>(),
                audio = supplement.audio ?? Array.Empty<AuditionPvSixtySecondAudioEvidence>(),
                rights = supplement.rights ?? Array.Empty<AuditionPvSixtySecondRightsEvidence>(),
                usedItems = supplement.usedItems ?? Array.Empty<AuditionPvSixtySecondUsedItem>(),
                gateEvidence = gate,
                endCardGraphic = ClonePin(supplement.endCardGraphic)
            };
        }

        private static string[] MissingSupplemental(
            AuditionPvSixtySecondProductionComposeInput input,
            IEnumerable<string> holds)
        {
            var missing = new SortedSet<string>(holds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            int expectedRefs = AuditionPvSixtySecondProductionComposer.CreateDefaultEdlForTests()
                .Length;
            if ((input.shotReferences ?? Array.Empty<AuditionPvSixtySecondShotReferenceBinding>())
                .Length != expectedRefs) missing.Add("SHOT_REFERENCES_INCOMPLETE");
            if ((input.audio ?? Array.Empty<AuditionPvSixtySecondAudioEvidence>()).Length == 0)
                missing.Add("AUDIO_INCOMPLETE");
            if ((input.rights ?? Array.Empty<AuditionPvSixtySecondRightsEvidence>()).Length == 0)
                missing.Add("RIGHTS_INCOMPLETE");
            if ((input.usedItems ?? Array.Empty<AuditionPvSixtySecondUsedItem>()).Length == 0)
                missing.Add("USED_ITEMS_INCOMPLETE");
            if (!PinCurrent(input.endCardGraphic)) missing.Add("END_CARD_GRAPHIC_INCOMPLETE");
            if (!PinCurrent(input.gateEvidence?.rightsCoverageReview))
                missing.Add("RIGHTS_COVERAGE_REVIEW_INCOMPLETE");
            return missing.ToArray();
        }

        private static byte[] CreateQuarterScaleContactSheet(VisualFrame[] sourceFrames)
        {
            sourceFrames ??= Array.Empty<VisualFrame>();
            if (sourceFrames.Length == 0 || sourceFrames.Length > 32 ||
                sourceFrames.Any(value => value?.take?.capture?.manifest == null ||
                    value.frame == null || string.IsNullOrWhiteSpace(value.sourcePath)))
                throw new InvalidDataException("Visual contact-sheet cell count is invalid.");
            int columns = Math.Min(4, sourceFrames.Length);
            int rows = (sourceFrames.Length + columns - 1) / columns;
            int cellWidth = AuditionPvSixtySecondGateManifestValidator.Width / 4;
            int cellHeight = AuditionPvSixtySecondGateManifestValidator.Height / 4;
            int sheetWidth = checked(columns * cellWidth);
            int sheetHeight = checked(rows * cellHeight);
            var sheetPixels = new Color32[checked(sheetWidth * sheetHeight)];
            long persistentBytes = checked((long)sheetPixels.Length * 4L);
            if (AuditionPvEvidenceMemoryContract.ConservativePeakBytes(
                    MaxPngBytes, 0, persistentBytes) >
                AuditionPvEvidenceMemoryContract.MaxTransientWorkingSetBytes)
                throw new InvalidDataException("Visual contact sheet exceeds the memory contract.");
            for (int cell = 0; cell < sourceFrames.Length; cell++)
            {
                VisualFrame sourceFrame = sourceFrames[cell];
                using LoadedQhdPng source = LoadedQhdPng.Open(sourceFrame.sourcePath,
                    sourceFrame.frame.frameSha256,
                    sourceFrame.take.capture.manifest.outputDirectory);
                int cellX = cell % columns;
                int cellY = cell / columns;
                for (int y = 0; y < cellHeight; y++)
                {
                    int sourceRow = y * 4 * AuditionPvSixtySecondGateManifestValidator.Width;
                    int targetRow = (cellY * cellHeight + y) * sheetWidth + cellX * cellWidth;
                    for (int x = 0; x < cellWidth; x++)
                        sheetPixels[targetRow + x] = source.pixels[sourceRow + x * 4];
                }
            }
            Texture2D sheet = null;
            try
            {
                sheet = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32,
                    false, true);
                sheet.SetPixels32(sheetPixels);
                sheet.Apply(false, false);
                byte[] bytes = ImageConversion.EncodeToPNG(sheet);
                if (bytes == null || bytes.LongLength <= 0 || bytes.LongLength > MaxPngBytes)
                    throw new InvalidDataException("Visual contact-sheet PNG is outside bounds.");
                return bytes;
            }
            finally
            {
                if (sheet != null) UnityEngine.Object.DestroyImmediate(sheet);
            }
        }

        private static void ValidateMeasuredFrames(AuditionPvMeasuredFrame[] frames,
            int[] expectedFrames, AuditionPvCaptureManifest capture, string shotId, string role)
        {
            frames ??= Array.Empty<AuditionPvMeasuredFrame>();
            expectedFrames ??= Array.Empty<int>();
            if (frames.Length != expectedFrames.Length ||
                !frames.Select(value => value?.sourceFrame ?? -1)
                    .SequenceEqual(expectedFrames))
                throw new InvalidDataException(role + " frame order/cardinality drifted.");
            foreach (AuditionPvMeasuredFrame frame in frames)
            {
                if (frame == null || !AuditionPvSha256.IsSha256(frame.frameSha256))
                    throw new InvalidDataException(role + " contains an invalid frame pin.");
                VerifyPhysicalFrame(capture, shotId, frame.sourceFrame, frame.frameSha256);
            }
        }

        private static void VerifyPhysicalFrame(AuditionPvCaptureManifest capture,
            string shotId, int frame, string sha256)
        {
            string path = SourceFramePath(capture, shotId, frame);
            var pin = new AuditionPvPinnedArtifact
                { path = Normalize(path), sha256 = sha256 };
            VerifyStableFileWhile(pin, capture.outputDirectory,
                32L * 1024L * 1024L, "physical source frame", () =>
                {
                    if (!AuditionPvSixtySecondGateManifestValidator.TryPngPreflight(path,
                            32L * 1024L * 1024L, out int width, out int height) ||
                        width != AuditionPvSixtySecondGateManifestValidator.Width ||
                        height != AuditionPvSixtySecondGateManifestValidator.Height)
                        throw new InvalidDataException(
                            "Physical source-frame bytes do not match review evidence.");
                });
        }

        private static AuditionPvPinnedArtifact ExactTestArtifact(AuditionPvCaptureManifest capture,
            string suite, string name, string requiredToken = null)
        {
            AuditionPvTestResult[] matches = (capture.testResults ??
                    Array.Empty<AuditionPvTestResult>())
                .Where(value => value != null && value.status == "passed" &&
                                value.suite == suite && value.name == name &&
                                (requiredToken == null || ContainsToken(value.details, requiredToken)))
                .ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException("Exact passed capture test is missing/ambiguous: " + name);
            AuditionPvTestResult test = matches[0];
            return DeclaredTestArtifact(capture, test);
        }

        private static AuditionPvPinnedArtifact DeclaredTestArtifact(
            AuditionPvCaptureManifest capture, AuditionPvTestResult test)
        {
            if (capture == null || test == null || test.status != "passed")
                throw new InvalidDataException("Capture test declaration is invalid.");
            string path = Path.IsPathRooted(test.artifactPath)
                ? Full(test.artifactPath)
                : Full(Path.Combine(capture.outputDirectory, test.artifactPath ?? string.Empty));
            string declaredSha = ArtifactShaFromDetails(test.details);
            FileIdentity identity = ReadStableIdentity(path, declaredSha,
                capture.outputDirectory, MaxPngBytes, "capture test artifact");
            return new AuditionPvPinnedArtifact
                { path = Normalize(identity.path), sha256 = identity.sha256 };
        }

        private static EvidenceSource FindEvidence(IEnumerable<EvidenceSource> values,
            CaptureSource capture, string shotId, int sourceStart, int sourceEnd,
            int selectStart, int selectEnd)
        {
            return values.SingleOrDefault(value => value.capture == capture &&
                value.receipt.sourceShotId == shotId &&
                value.receipt.sourceRangeStartFrame == sourceStart &&
                value.receipt.sourceRangeEndFrame == sourceEnd &&
                value.receipt.selectStartFrame == selectStart &&
                value.receipt.selectEndFrame == selectEnd) ??
                throw new InvalidDataException("Exact range-bound evidence receipt is missing.");
        }

        private static Dictionary<string, string> ReadLedger(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new InvalidDataException("Frame ledger bytes are empty.");
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            using var stream = new MemoryStream(bytes, writable: false);
            using var reader = new StreamReader(stream, Utf8, true, 4096, false);
            int count = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (++count > 100000 || line.Length < 67 || line[64] != ' ' || line[65] != ' ' ||
                    !AuditionPvSha256.IsSha256(line.Substring(0, 64)))
                    throw new InvalidDataException("Frame ledger is malformed or too large.");
                string relative = Normalize(line.Substring(66));
                if (Path.IsPathRooted(relative) || relative.Contains("..", StringComparison.Ordinal) ||
                    relative.Contains(':') || !result.TryAdd(relative, line.Substring(0, 64)))
                    throw new InvalidDataException("Frame ledger path is unsafe/duplicated.");
            }
            if (result.Count == 0) throw new InvalidDataException("Frame ledger is empty.");
            return result;
        }

        private static int[] SampledFrames(int start, int end)
        {
            if (start < 0 || end < start || (long)end - start + 1L > 100000L)
                throw new ArgumentOutOfRangeException();
            var result = new List<int>();
            for (long frame = start; frame <= end;
                 frame += AuditionPvSixtySecondGateManifestValidator.Fps)
                result.Add(checked((int)frame));
            if (result.Count == 0 || result[^1] != end) result.Add(end);
            return result.ToArray();
        }

        private static int[] DeterministicPreviewIndexes(int count, int capacity)
        {
            if (count <= 0 || capacity <= 0) return Array.Empty<int>();
            int selected = Math.Min(count, capacity);
            if (selected == 1) return new[] { 0 };
            var result = new int[selected];
            for (int index = 0; index < selected; index++)
                result[index] = checked((int)((long)index * (count - 1) / (selected - 1)));
            return result;
        }

        private static AuditionPvPinnedArtifact PlanJson<T>(ICollection<PlannedFile> files,
            string path, T value)
        {
            byte[] bytes = Utf8.GetBytes(JsonUtility.ToJson(value, true) + "\n");
            if (bytes.LongLength > MaxJsonBytes)
                throw new InvalidDataException("Planned JSON exceeds its byte limit.");
            return PlanBytes(files, path, bytes, "json", typeof(T));
        }

        private static AuditionPvPinnedArtifact PlanBytes(ICollection<PlannedFile> files,
            string path, byte[] bytes, string role, Type jsonType = null)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            path = Full(path);
            if (bytes == null || bytes.Length == 0)
                throw new InvalidDataException("Planned artifact bytes are empty.");
            string sha = BytesHash(bytes);
            if (files.Any(value => PathsEqual(value.path, path)))
                throw new InvalidDataException("Duplicate planned output path.");
            var pin = new AuditionPvPinnedArtifact { path = Normalize(path), sha256 = sha };
            files.Add(new PlannedFile
                { path = path, bytes = bytes, role = role, pin = pin, jsonType = jsonType });
            return ClonePin(pin);
        }

        private static void FreezeExternalInputs(AuditionPvPinnedArtifact specPin,
            AuditionPvSixtySecondOperatorApprovalSpec spec,
            IReadOnlyList<CaptureSource> captures,
            IReadOnlyList<EvidenceSource> evidence,
            bool production)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Freeze(AuditionPvPinnedArtifact pin, string root, long byteLimit,
                string role, bool allowAnyRoot = false)
            {
                if (!PinShape(pin))
                    throw new InvalidDataException(role + " pin is incomplete at final freeze.");
                string key = Full(pin.path) + "\0" + pin.sha256;
                if (seen.Add(key))
                    VerifyPin(pin, root, byteLimit, role + " final freeze", allowAnyRoot);
            }

            void FreezeDeclaredTest(AuditionPvCaptureManifest capture,
                AuditionPvTestResult test)
            {
                AuditionPvPinnedArtifact artifact = DeclaredTestArtifact(capture, test);
                seen.Add(Full(artifact.path) + "\0" + artifact.sha256);
            }

            Freeze(specPin, null, MaxJsonBytes, "operator approval spec", allowAnyRoot: true);
            AuditionPvSixtySecondProductionEdlRow[] sourceRows =
                AuditionPvSixtySecondProductionComposer.CreateDefaultEdlForTests()
                    .Where(value => value.bucketId != "PV_S100").ToArray();
            foreach (CaptureSource capture in captures)
            {
                Freeze(capture.manifestPin, spec.captureRootDirectory, MaxJsonBytes,
                    "capture manifest");
                foreach (AuditionPvShotManifestEntry shot in capture.manifest.shots ??
                         Array.Empty<AuditionPvShotManifestEntry>())
                {
                    if (shot == null) continue;
                    AuditionPvTestResult authorship = (capture.manifest.testResults ??
                            Array.Empty<AuditionPvTestResult>()).Single(value =>
                            value != null && value.status == "passed" &&
                            value.suite == EvidenceSuite &&
                            value.name == "shot-authorship/" + shot.id);
                    FreezeDeclaredTest(capture.manifest, authorship);
                }
                foreach (string beatId in sourceRows.Where(value =>
                             value.familyId == capture.familyId)
                         .SelectMany(value => value.beatIds ?? Array.Empty<string>())
                         .Distinct(StringComparer.Ordinal))
                {
                    AuditionPvTestResult semantic = (capture.manifest.testResults ??
                            Array.Empty<AuditionPvTestResult>()).Single(value =>
                            value != null && value.status == "passed" &&
                            value.suite == EvidenceSuite &&
                            value.name == "semantic-beat/" + beatId &&
                            ContainsToken(value.details, "semantic-fact=" + beatId));
                    FreezeDeclaredTest(capture.manifest, semantic);
                }
            }

            foreach (EvidenceSource bundle in evidence)
            {
                AuditionPvSixtySecondEvidenceBundleReceipt receipt = bundle.receipt;
                string captureRoot = bundle.capture.manifest.outputDirectory;
                Freeze(bundle.pin, captureRoot, MaxJsonBytes, "evidence receipt");
                Freeze(receipt.sourceFrameLedger, captureRoot, MaxPngBytes,
                    "source frame ledger");
                Freeze(receipt.automatedProof, captureRoot, MaxJsonBytes,
                    "automated proof");
                Freeze(receipt.contactSheet, captureRoot, MaxPngBytes,
                    "evidence contact sheet");
                Freeze(receipt.filmstripSkeleton, captureRoot, MaxJsonBytes,
                    "filmstrip skeleton");
                Freeze(receipt.humanReviewSkeleton,
                    production
                        ? AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot
                        : null,
                    MaxJsonBytes, "human review skeleton", allowAnyRoot: !production);
                Freeze(receipt.rec709Config,
                    production
                        ? AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot
                        : null,
                    MaxJsonBytes, "Rec.709 config", allowAnyRoot: !production);
                Freeze(receipt.rec709OutputLedger,
                    production
                        ? AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot
                        : null,
                    MaxJsonBytes, "Rec.709 output ledger", allowAnyRoot: !production);
                Freeze(receipt.rendererRuntimeWorkload, captureRoot, MaxJsonBytes,
                    "renderer runtime workload");
                if (PinShape(receipt.hudRuntimeWorkload))
                    Freeze(receipt.hudRuntimeWorkload, captureRoot, MaxJsonBytes,
                        "HUD runtime workload");
                foreach (AuditionPvNamedPinnedArtifact check in receipt.checkResults ??
                         Array.Empty<AuditionPvNamedPinnedArtifact>())
                    Freeze(check.artifact, captureRoot, MaxJsonBytes,
                        "evidence check result");
                foreach (AuditionPvTestResult generated in receipt.generatedTestResults ??
                         Array.Empty<AuditionPvTestResult>())
                {
                    AuditionPvTestResult captured = (bundle.capture.manifest.testResults ??
                            Array.Empty<AuditionPvTestResult>())
                        .Single(value => SameTestIdentity(value, generated));
                    FreezeDeclaredTest(bundle.capture.manifest, captured);
                }
            }

            AuditionPvSixtySecondCurrentTwelveSecondSpec current = spec.currentTwelveSecond;
            if (current != null && current.status == "ready")
            {
                string package = Full(current.packageDirectory);
                Freeze(new AuditionPvPinnedArtifact
                    {
                        path = Normalize(Path.Combine(package,
                            AuditionPvTwelveSecondGoldAssembler.ManifestFileName)),
                        sha256 = current.manifestSha256
                    }, package, MaxJsonBytes, "current 12-second manifest");
                Freeze(new AuditionPvPinnedArtifact
                    {
                        path = Normalize(Path.Combine(package,
                            AuditionPvTwelveSecondGoldAssembler.ValidationReportFileName)),
                        sha256 = current.validationSha256
                    }, package, MaxJsonBytes, "current 12-second validation");
                foreach (AuditionPvTwelveSecondSourceLedgerSpec ledger in
                         current.sourceLedgers ?? Array.Empty<AuditionPvTwelveSecondSourceLedgerSpec>())
                {
                    CaptureSource capture = captures.Single(value =>
                        value.manifest.captureId == ledger.sourceCaptureId);
                    Freeze(ledger.frameLedger, capture.manifest.outputDirectory,
                        MaxPngBytes, "current 12-second source ledger");
                }
            }

            AuditionPvSixtySecondProductionComposeInput supplement = spec.composeInputSupplement;
            if (PinShape(supplement?.endCardGraphic))
                Freeze(supplement.endCardGraphic,
                    production
                        ? AuditionPvSixtySecondGateManifestValidator.ProductionGraphicsRoot
                        : null,
                    MaxPngBytes, "supplement end-card graphic", allowAnyRoot: !production);
            if (PinShape(supplement?.gateEvidence?.rightsCoverageReview))
                Freeze(supplement.gateEvidence.rightsCoverageReview,
                    production
                        ? AuditionPvSixtySecondGateManifestValidator.ProductionReviewRoot
                        : null,
                    MaxJsonBytes, "supplement rights-coverage review",
                    allowAnyRoot: !production);
            foreach (AuditionPvSixtySecondAudioEvidence audio in supplement?.audio ??
                     Array.Empty<AuditionPvSixtySecondAudioEvidence>())
            {
                if (PinShape(audio?.file))
                    Freeze(audio.file, null, MaxPngBytes, "supplement audio", true);
                if (PinShape(audio?.generationManifest))
                    Freeze(audio.generationManifest, null, MaxJsonBytes,
                        "supplement audio generation manifest", true);
                if (PinShape(audio?.listeningReport))
                    Freeze(audio.listeningReport, null, MaxJsonBytes,
                        "supplement audio listening report", true);
            }
            foreach (AuditionPvSixtySecondRightsEvidence rights in supplement?.rights ??
                     Array.Empty<AuditionPvSixtySecondRightsEvidence>())
                if (PinShape(rights?.record))
                    Freeze(rights.record, null, MaxJsonBytes,
                        "supplement rights record", true);
            foreach (AuditionPvSixtySecondUsedItem item in supplement?.usedItems ??
                     Array.Empty<AuditionPvSixtySecondUsedItem>())
                if (PinShape(item?.artifact))
                    Freeze(item.artifact, null, MaxPngBytes,
                        "supplement used-item artifact", true);
        }

        private static void VerifyPlannedOutputs(IEnumerable<PlannedFile> values, string phase)
        {
            foreach (PlannedFile file in values ?? Array.Empty<PlannedFile>())
            {
                string role = "planned output " + phase;
                if (file.jsonType == null)
                {
                    ReadStableIdentity(file.path, file.pin.sha256, null, MaxPngBytes, role);
                    continue;
                }
                FileSnapshot snapshot = ReadFileSnapshot(file.path, file.pin.sha256,
                    null, MaxJsonBytes, role);
                string json;
                try { json = Utf8.GetString(snapshot.bytes); }
                catch (DecoderFallbackException exception)
                { throw new InvalidDataException(role + " is not strict UTF-8.", exception); }
                object parsed;
                try { parsed = JsonUtility.FromJson(json, file.jsonType); }
                catch (Exception exception) when (exception is ArgumentException ||
                    exception is InvalidOperationException)
                { throw new InvalidDataException(role + " JSON is invalid.", exception); }
                if (parsed == null)
                    throw new InvalidDataException(role + " JSON decoded to null.");
            }
        }

        private static void Commit(IEnumerable<PlannedFile> values, Action postVerify)
        {
            PlannedFile[] files = (values ?? Array.Empty<PlannedFile>()).ToArray();
            var installed = new List<string>();
            var temporaries = new List<string>();
            try
            {
                foreach (PlannedFile file in files)
                {
                    string parent = Path.GetDirectoryName(file.path) ??
                        throw new InvalidDataException("Planned artifact has no parent.");
                    Directory.CreateDirectory(parent);
                    RejectReparseChainForExistingParents(file.path);
                    if (File.Exists(file.path))
                    {
                        if (AuditionPvSha256.FileHash(file.path) != file.pin.sha256)
                            throw new IOException(
                                "Immutable approval artifact exists with different bytes: " + file.path);
                        continue;
                    }
                    string temporary = file.path + ".tmp-" + Guid.NewGuid().ToString("N");
                    File.WriteAllBytes(temporary, file.bytes);
                    temporaries.Add(temporary);
                    if (AuditionPvSha256.FileHash(temporary) != file.pin.sha256)
                        throw new IOException("Temporary approval artifact hash drifted.");
                    File.Move(temporary, file.path);
                    temporaries.Remove(temporary);
                    installed.Add(file.path);
                }
                VerifyPlannedOutputs(files, "before post-verification");
                postVerify?.Invoke();
                VerifyPlannedOutputs(files, "after post-verification");
            }
            catch
            {
                foreach (string temporary in temporaries)
                    if (File.Exists(temporary)) File.Delete(temporary);
                foreach (string path in installed.AsEnumerable().Reverse())
                    if (File.Exists(path)) File.Delete(path);
                throw;
            }
        }

        private static string VerifyPin(AuditionPvPinnedArtifact pin, string root,
            long byteLimit, string role, bool allowAnyRoot = false)
        {
            if (!PinShape(pin)) throw new InvalidDataException(role + " pin is incomplete.");
            FileIdentity identity = ReadStableIdentity(pin.path, pin.sha256,
                allowAnyRoot ? null : root, byteLimit, role);
            return identity.path;
        }

        private static PinnedJson<T> ReadJsonSnapshot<T>(string path,
            string expectedSha256, string root, long byteLimit, string role,
            Action<string> mutationSeam = null)
        {
            FileSnapshot snapshot = ReadFileSnapshot(path, expectedSha256, root,
                byteLimit, role, mutationSeam);
            string json;
            try { json = Utf8.GetString(snapshot.bytes); }
            catch (DecoderFallbackException exception)
            { throw new InvalidDataException(role + " is not strict UTF-8.", exception); }
            T value;
            try { value = JsonUtility.FromJson<T>(json); }
            catch (Exception exception) when (exception is ArgumentException ||
                exception is InvalidOperationException)
            { throw new InvalidDataException(role + " JSON is invalid.", exception); }
            if (value == null) throw new InvalidDataException(role + " JSON decoded to null.");
            return new PinnedJson<T>
            {
                path = snapshot.path,
                pin = new AuditionPvPinnedArtifact
                    { path = Normalize(snapshot.path), sha256 = snapshot.sha256 },
                value = value,
                snapshot = snapshot
            };
        }

        private static FileSnapshot ReadFileSnapshot(string path,
            string expectedSha256, string root, long byteLimit, string role,
            Action<string> mutationSeam = null)
        {
            path = Full(path);
            RejectReparseChain(path);
            if (!string.IsNullOrWhiteSpace(root)) RequireUnder(path, root, role);
            var file = new FileInfo(path);
            if (!file.Exists || file.Length <= 0 || file.Length > byteLimit ||
                file.Length > int.MaxValue)
                throw new InvalidDataException(role + " is missing or outside byte limits.");
            byte[] bytes;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                       FileShare.Read, 64 * 1024, FileOptions.SequentialScan))
            {
                if (stream.Length != file.Length || stream.Length <= 0 ||
                    stream.Length > byteLimit || stream.Length > int.MaxValue)
                    throw new InvalidDataException(role + " changed before bounded read.");
                bytes = new byte[checked((int)stream.Length)];
                int offset = 0;
                while (offset < bytes.Length)
                {
                    int read = stream.Read(bytes, offset, bytes.Length - offset);
                    if (read <= 0) throw new EndOfStreamException(role + " was truncated.");
                    offset += read;
                }
                if (stream.ReadByte() != -1)
                    throw new InvalidDataException(role + " grew during bounded read.");
            }
            string sha256 = BytesHash(bytes);
            if (!string.IsNullOrWhiteSpace(expectedSha256) &&
                (!AuditionPvSha256.IsSha256(expectedSha256) || sha256 != expectedSha256))
                throw new InvalidDataException(role + " hash drifted.");
            var result = new FileSnapshot
            {
                path = path,
                length = bytes.LongLength,
                sha256 = sha256,
                bytes = bytes
            };
            mutationSeam?.Invoke(path);
            VerifySnapshotStillCurrent(result, role + " post-read freeze");
            return result;
        }

        private static FileIdentity ReadStableIdentity(string path,
            string expectedSha256, string root, long byteLimit, string role)
        {
            FileIdentity before = ReadPinnedIdentityOnce(path, expectedSha256, root,
                byteLimit, role + " first pass");
            FileIdentity after = ReadPinnedIdentityOnce(path, expectedSha256, root,
                byteLimit, role + " second pass");
            if (before.length != after.length || before.sha256 != after.sha256 ||
                !PathsEqual(before.path, after.path))
                throw new InvalidDataException(role + " changed during identity verification.");
            return after;
        }

        private static FileIdentity ReadPinnedIdentityOnce(string path,
            string expectedSha256, string root, long byteLimit, string role)
        {
            path = Full(path);
            RejectReparseChain(path);
            if (!string.IsNullOrWhiteSpace(root)) RequireUnder(path, root, role);
            var before = new FileInfo(path);
            if (!before.Exists || before.Length <= 0 || before.Length > byteLimit ||
                !AuditionPvSha256.IsSha256(expectedSha256))
                throw new InvalidDataException(role + " is missing or outside byte limits.");
            long length = before.Length;
            string sha256 = AuditionPvSha256.FileHash(path);
            var after = new FileInfo(path);
            if (!after.Exists || after.Length != length ||
                sha256 != expectedSha256)
                throw new InvalidDataException(role + " changed during pinned identity read.");
            return new FileIdentity { path = path, length = length, sha256 = sha256 };
        }

        private static void VerifySnapshotStillCurrent(FileSnapshot snapshot, string role)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var file = new FileInfo(snapshot.path);
            if (!file.Exists || file.Length != snapshot.length ||
                AuditionPvSha256.FileHash(snapshot.path) != snapshot.sha256)
                throw new InvalidDataException(role + " changed after its bounded byte snapshot.");
        }

        private static void VerifyStableFileWhile(AuditionPvPinnedArtifact pin,
            string root, long byteLimit, string role, Action action)
        {
            if (!PinShape(pin)) throw new InvalidDataException(role + " pin is incomplete.");
            FileIdentity before = ReadPinnedIdentityOnce(pin.path, pin.sha256, root,
                byteLimit, role + " before use");
            action?.Invoke();
            FileIdentity after = ReadPinnedIdentityOnce(pin.path, pin.sha256, root,
                byteLimit, role + " after use");
            if (before.length != after.length || before.sha256 != after.sha256 ||
                !PathsEqual(before.path, after.path))
                throw new InvalidDataException(role + " changed while it was consumed.");
        }

        private static AuditionPvPinnedArtifact ClonePin(AuditionPvPinnedArtifact value) =>
            value == null ? new AuditionPvPinnedArtifact() : new AuditionPvPinnedArtifact
            {
                path = value.path ?? string.Empty,
                sha256 = value.sha256 ?? string.Empty
            };

        private static bool PinShape(AuditionPvPinnedArtifact value) =>
            value != null && !string.IsNullOrWhiteSpace(value.path) &&
            AuditionPvSha256.IsSha256(value.sha256);

        private static bool PinCurrent(AuditionPvPinnedArtifact value)
        {
            if (!PinShape(value)) return false;
            try
            {
                return File.Exists(value.path) &&
                    AuditionPvSha256.FileHash(value.path) == value.sha256;
            }
            catch (Exception exception) when (exception is IOException ||
                exception is UnauthorizedAccessException || exception is ArgumentException ||
                exception is NotSupportedException) { return false; }
        }

        private static bool SamePin(AuditionPvPinnedArtifact left,
            AuditionPvPinnedArtifact right) => left != null && right != null &&
            PathsEqual(left.path, right.path) && left.sha256 == right.sha256;

        private static T BindRange<T>(T artifact,
            AuditionPvSixtySecondProductionEdlRow row) where T : AuditionPvRangeBoundArtifact
        {
            artifact.sourceRangeStartFrame = row.sourceRangeStartFrame;
            artifact.sourceRangeEndFrame = row.sourceRangeEndFrame;
            artifact.selectStartFrame = row.selectStartFrame;
            artifact.selectEndFrame = row.selectEndFrame;
            return artifact;
        }

        private static bool SameRange(AuditionPvRangeBoundArtifact left,
            AuditionPvRangeBoundArtifact right) => left != null && right != null &&
            left.sourceRangeStartFrame == right.sourceRangeStartFrame &&
            left.sourceRangeEndFrame == right.sourceRangeEndFrame &&
            left.selectStartFrame == right.selectStartFrame &&
            left.selectEndFrame == right.selectEndFrame;

        private static string EvidenceIdentity(AuditionPvSixtySecondEvidenceBundleReceipt value) =>
            value == null ? string.Empty : EvidenceIdentity(value.captureId, value.sourceShotId,
                value.sourceRangeStartFrame, value.sourceRangeEndFrame,
                value.selectStartFrame, value.selectEndFrame);

        private static string EvidenceIdentity(string captureId, string shotId,
            int sourceStart, int sourceEnd, int selectStart, int selectEnd) =>
            string.Join("\0", captureId ?? string.Empty, shotId ?? string.Empty,
                sourceStart.ToString(CultureInfo.InvariantCulture),
                sourceEnd.ToString(CultureInfo.InvariantCulture),
                selectStart.ToString(CultureInfo.InvariantCulture),
                selectEnd.ToString(CultureInfo.InvariantCulture));

        private static string Classify(AuditionPvCaptureManifest capture)
        {
            string[] actual = (capture.shots ?? Array.Empty<AuditionPvShotManifestEntry>())
                .Where(value => value != null).Select(value => value.id)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return Families.FirstOrDefault(value => actual.SequenceEqual(
                value.shotIds.OrderBy(id => id, StringComparer.Ordinal),
                StringComparer.Ordinal))?.id ?? string.Empty;
        }

        private static string DependencyIdentity(AuditionPvCaptureManifest manifest)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var material = new StringBuilder();
            foreach (AuditionPvDependencyHash dependency in (manifest.dependencyHashes ??
                         Array.Empty<AuditionPvDependencyHash>())
                     .OrderBy(value => value?.path, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(value => value?.path, StringComparer.Ordinal))
            {
                string path = Normalize(dependency?.path);
                if (dependency == null || string.IsNullOrWhiteSpace(path) || !seen.Add(path) ||
                    !dependency.exists || dependency.byteLength < 0 ||
                    !AuditionPvSha256.IsSha256(dependency.sha256))
                    throw new InvalidDataException("Capture dependency identity is invalid.");
                material.Append(path).Append('\0').Append('1').Append('\0')
                    .Append(dependency.byteLength.ToString(CultureInfo.InvariantCulture))
                    .Append('\0').Append(dependency.sha256).Append('\0');
            }
            if (seen.Count == 0) throw new InvalidDataException("Dependency identity is empty.");
            return AuditionPvSha256.TextHash(material.ToString());
        }

        private static string CaptureApprovalPath(CaptureSource capture, string assemblyId,
            string atomicShotId, string fileName)
        {
            string path = Path.Combine(capture.manifest.outputDirectory, "evidence",
                "sixty_second", "operator_approval", Safe(assemblyId), Safe(atomicShotId),
                fileName);
            RequireUnder(path, capture.manifest.outputDirectory, "capture approval output");
            return path;
        }

        private static string ReviewPath(string root, string assemblyId, string atomicShotId,
            string captureId, string fileName)
        {
            string path = Path.Combine(root, Safe(assemblyId), "takes", Safe(atomicShotId),
                Safe(captureId), fileName);
            RequireUnder(path, root, "review output");
            return path;
        }

        private static string TakeId(string atomicShotId, string captureId, bool cleanPlate) =>
            atomicShotId + (cleanPlate ? "-clean-" : "-take-") + captureId;

        private static string SourceFrameRelative(string shotId, int frame) => Normalize(
            (shotId == AuditionPvStationTransitionGoldenCapture.ShotId
                ? AuditionPvStationTransitionGoldenCapture.FramesFolderName
                : "frames/" + shotId) +
            "/frame_" + frame.ToString("0000", CultureInfo.InvariantCulture) + ".png");

        private static string SourceFramePath(AuditionPvCaptureManifest capture,
            string shotId, int frame) => Full(Path.Combine(capture.outputDirectory,
                SourceFrameRelative(shotId, frame).Replace('/', Path.DirectorySeparatorChar)));

        private static string ReadArgument(string prefix) => Environment.GetCommandLineArgs()
            .FirstOrDefault(value => value != null &&
                value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))?[prefix.Length..]
            ?? string.Empty;

        private static bool SameTestIdentity(AuditionPvTestResult left,
            AuditionPvTestResult right) => left != null && right != null &&
            left.suite == right.suite && left.name == right.name &&
            PathsEqual(left.artifactPath, right.artifactPath);

        private static bool ContainsToken(string value, string token) =>
            (value ?? string.Empty).Split(new[] { ' ', '\t', '\r', '\n', ';', ',' },
                StringSplitOptions.RemoveEmptyEntries).Contains(token, StringComparer.Ordinal);

        private static string ArtifactShaFromDetails(string details)
        {
            const string prefix = "artifact-sha256=";
            string[] declared = (details ?? string.Empty)
                .Split(new[] { ' ', '\t', '\r', '\n', ';', ',' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(value => value.StartsWith(prefix, StringComparison.Ordinal))
                .Select(value => value.Substring(prefix.Length)).ToArray();
            if (declared.Length != 1 || !AuditionPvSha256.IsSha256(declared[0]))
                throw new InvalidDataException(
                    "Capture test must declare exactly one valid artifact-sha256 token.");
            return declared[0];
        }

        private static bool Utc(string value) => DateTimeOffset.TryParse(value,
            CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsed) &&
            parsed.Offset == TimeSpan.Zero;

        private static bool FullGitSha(string value) => value != null && value.Length == 40 &&
            value.All(character => character >= '0' && character <= '9' ||
                                   character >= 'a' && character <= 'f');

        private static string Safe(string value)
        {
            value ??= string.Empty;
            if (value.Length == 0 || value.Length > 128 || value == "." || value == ".." ||
                value.Any(character => !(char.IsLetterOrDigit(character) || character == '-' ||
                                         character == '_' || character == '.')))
                throw new InvalidDataException("Unsafe path component: " + value);
            return value;
        }

        private static string Full(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Path is empty.");
            return Path.GetFullPath(value);
        }

        private static void RequireAbsoluteSpecPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Path.IsPathRooted(value))
                throw new ArgumentException(
                    "operatorReviewedSpec must be an absolute JSON path.", nameof(value));
        }

        private static string Normalize(string value) =>
            (value ?? string.Empty).Replace('\\', '/');

        private static bool PathsEqual(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right)) return false;
            try { return string.Equals(Full(left), Full(right), StringComparison.OrdinalIgnoreCase); }
            catch (Exception exception) when (exception is IOException ||
                exception is UnauthorizedAccessException || exception is ArgumentException ||
                exception is NotSupportedException) { return false; }
        }

        private static void RequireUnder(string path, string root, string role)
        {
            string full = Full(path);
            string parent = Full(root).TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(parent, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(role + " escapes its allowed root.");
        }

        private static void RequireUnderOrEqual(string path, string root, string role)
        {
            if (PathsEqual(path, root)) return;
            RequireUnder(path, root, role);
        }

        private static void RejectReparseChain(string path)
        {
            path = Full(path);
            string current = File.Exists(path) ? path : Path.GetDirectoryName(path);
            while (!string.IsNullOrWhiteSpace(current))
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Reparse points are forbidden: " + current);
                current = Path.GetDirectoryName(current);
            }
        }

        private static void RejectReparseChainForExistingParents(string path)
        {
            string current = Full(path);
            while (!File.Exists(current) && !Directory.Exists(current))
                current = Path.GetDirectoryName(current) ?? string.Empty;
            if (!string.IsNullOrEmpty(current)) RejectReparseChain(current);
        }

        private static string BytesHash(byte[] bytes)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(bytes).Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private sealed class FamilyContract
        {
            public FamilyContract(string id, int count, params string[] shotIds)
            { this.id = id; this.count = count; this.shotIds = shotIds; }
            public readonly string id;
            public readonly int count;
            public readonly string[] shotIds;
        }

        private sealed class CaptureSource
        {
            public AuditionPvCaptureManifest manifest;
            public AuditionPvPinnedArtifact manifestPin;
            public string manifestPath = string.Empty, familyId = string.Empty;
            public string captureCoreSha256 = string.Empty;
            public string dependencyIdentitySha256 = string.Empty;
            public readonly Dictionary<string, AuditionPvShotAuthorshipArtifact> authorship =
                new(StringComparer.Ordinal);

            public AuditionPvShotAuthorshipArtifact Authorship(string shotId) =>
                authorship.TryGetValue(shotId, out var value) ? value :
                    throw new InvalidDataException("Missing shot-authorship: " + shotId);

            public AuditionPvShotManifestEntry Shot(string shotId) =>
                (manifest.shots ?? Array.Empty<AuditionPvShotManifestEntry>())
                    .SingleOrDefault(value => value != null && value.id == shotId) ??
                throw new InvalidDataException("Missing source shot: " + shotId);
        }

        private sealed class EvidenceSource
        {
            public AuditionPvPinnedArtifact pin;
            public AuditionPvSixtySecondEvidenceBundleReceipt receipt;
            public CaptureSource capture;
            public AuditionPvTakeReviewSkeletonArtifact reviewSkeleton;
        }

        private sealed class ApprovedTake
        {
            public AuditionPvSixtySecondProductionEdlRow row;
            public CaptureSource capture;
            public EvidenceSource evidence;
            public AuditionPvTakeHumanReviewArtifact review;
            public AuditionPvPinnedArtifact reviewPin;
            public string takeId = string.Empty;
        }

        private sealed class VisualFrame
        {
            public ApprovedTake take;
            public AuditionPvMeasuredFrame frame;
            public string sourcePath = string.Empty;
        }

        private sealed class PlannedFile
        {
            public string path = string.Empty, role = string.Empty;
            public byte[] bytes = Array.Empty<byte>();
            public AuditionPvPinnedArtifact pin = new();
            public Type jsonType;
        }

        private class FileIdentity
        {
            public string path = string.Empty, sha256 = string.Empty;
            public long length;
        }

        private sealed class FileSnapshot : FileIdentity
        {
            public byte[] bytes = Array.Empty<byte>();
        }

        private sealed class PinnedJson<T>
        {
            public string path = string.Empty;
            public AuditionPvPinnedArtifact pin = new();
            public T value;
            public FileSnapshot snapshot;
        }

        private sealed class LoadedQhdPng : IDisposable
        {
            private IDisposable lease;
            private Texture2D texture;
            public Color32[] pixels;

            public static LoadedQhdPng Open(string path, string expectedSha256,
                string captureRoot)
            {
                IDisposable lease = AuditionPvEvidenceMemoryContract.AcquireDecodedSourcePng();
                Texture2D texture = null;
                try
                {
                    FileSnapshot snapshot = ReadFileSnapshot(path, expectedSha256,
                        captureRoot, 32L * 1024L * 1024L,
                        "visual contact-sheet source PNG");
                    byte[] encoded = snapshot.bytes;
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                    if (!ImageConversion.LoadImage(texture, encoded, false) ||
                        texture.width != AuditionPvSixtySecondGateManifestValidator.Width ||
                        texture.height != AuditionPvSixtySecondGateManifestValidator.Height)
                        throw new InvalidDataException("Visual source PNG is not QHD.");
                    Color32[] pixels = texture.GetPixels32();
                    if (pixels.LongLength != checked((long)texture.width * texture.height))
                        throw new InvalidDataException("Visual source pixel cardinality drifted.");
                    return new LoadedQhdPng
                        { lease = lease, texture = texture, pixels = pixels };
                }
                catch
                {
                    if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                    lease.Dispose();
                    throw;
                }
            }

            public void Dispose()
            {
                pixels = null;
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                texture = null;
                lease?.Dispose();
                lease = null;
            }
        }
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondOperatorApprovalSpec
    {
        public string schemaVersion = AuditionPvSixtySecondApprovalAssembler.SpecSchema;
        public string assemblyId = string.Empty;
        public string judgementOrigin = string.Empty;
        public string reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
        public bool allCandidateSemanticTestBindingsReviewed;
        public string semanticEvidenceReviewNote = string.Empty;
        public string productCheckpointGitSha = string.Empty;
        public string captureRootDirectory = string.Empty;
        public string reviewOutputDirectory = string.Empty;
        public AuditionPvSixtySecondProductionEdlRow[] edl =
            Array.Empty<AuditionPvSixtySecondProductionEdlRow>();
        public AuditionPvPinnedArtifact[] captureManifests =
            Array.Empty<AuditionPvPinnedArtifact>();
        public AuditionPvPinnedArtifact[] evidenceBundleReceipts =
            Array.Empty<AuditionPvPinnedArtifact>();
        public AuditionPvSixtySecondAtomicApprovalSpec[] approvals =
            Array.Empty<AuditionPvSixtySecondAtomicApprovalSpec>();
        public AuditionPvSixtySecondCleanPlateApprovalSpec cleanPlate = new();
        public AuditionPvSixtySecondVisualReviewDecisionSpec visualReview = new();
        public AuditionPvSixtySecondCurrentTwelveSecondSpec currentTwelveSecond = new();
        public AuditionPvSixtySecondProductionComposeInput composeInputSupplement = new();
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondAtomicApprovalSpec
    {
        public string atomicShotId = string.Empty;
        public string approvedSourceCaptureId = string.Empty;
        public AuditionPvSixtySecondTakeReviewDecisionSpec review = new();
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondCleanPlateApprovalSpec
    {
        public string atomicShotId = string.Empty;
        public string sourceCaptureId = string.Empty;
        public string referenceApprovedSourceCaptureId = string.Empty;
        public AuditionPvSixtySecondTakeReviewDecisionSpec review = new();
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondTakeReviewDecisionSpec
    {
        public bool approved;
        public bool fullMotionRangeReviewed;
        public bool noBlackMesh;
        public bool noBrokenTrail;
        public AuditionPvSixtySecondReviewCriterionSpec[] criteria =
            Array.Empty<AuditionPvSixtySecondReviewCriterionSpec>();
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondReviewCriterionSpec
    {
        public string criterion = string.Empty;
        public int sourceFrame = -1;
        public string frameSha256 = string.Empty;
        public string note = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondVisualReviewDecisionSpec
    {
        public bool approved;
        public bool faceReadable, bossReadable, attackDirectionReadable, impactPointReadable;
        public bool noPinkShader, noErrorMagenta, noNullMaterial, noBlackMesh, noBrokenTrail;
        public AuditionPvSixtySecondVisualCriterionSpec[] criterionRefs =
            Array.Empty<AuditionPvSixtySecondVisualCriterionSpec>();
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondVisualCriterionSpec
    {
        public string criterion = string.Empty;
        public string atomicShotId = string.Empty;
        public int sourceFrame = -1;
        public string frameSha256 = string.Empty;
        public string note = string.Empty;
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondCurrentTwelveSecondSpec
    {
        public string status = "hold";
        public string holdReason = "current-package-not-reviewed";
        public string packageDirectory = string.Empty;
        public string manifestSha256 = string.Empty, validationSha256 = string.Empty;
        public bool approved;
        public string approvedBy = string.Empty, approvedAtUtc = string.Empty;
        public string sourceLedgerIdentityReviewNote = string.Empty;
        public AuditionPvTwelveSecondSourceLedgerSpec[] sourceLedgers =
            Array.Empty<AuditionPvTwelveSecondSourceLedgerSpec>();
    }

    [Serializable]
    internal sealed class AuditionPvTwelveSecondSourceLedgerSpec
    {
        public int segmentOrder = -1;
        public string sourceCaptureId = string.Empty, sourceShotId = string.Empty;
        public string sourceManifestSha256 = string.Empty;
        public string sourceDependencyIdentitySha256 = string.Empty;
        public AuditionPvPinnedArtifact frameLedger = new();
    }

    [Serializable]
    internal sealed class AuditionPvSixtySecondApprovalAssemblyReceipt
    {
        public string schemaVersion = string.Empty, status = string.Empty;
        public string assemblyId = string.Empty;
        public AuditionPvPinnedArtifact operatorReviewedSpec = new();
        public string reviewedBy = string.Empty, reviewedAtUtc = string.Empty;
        public int captureManifestCount, evidenceBundleReceiptCount;
        public int normalTakeBindingCount, approvedTakeCount, cleanPlateBindingCount;
        public string currentTwelveSecondStatus = string.Empty;
        public AuditionPvPinnedArtifact composeInput = new();
        public AuditionPvPinnedArtifact[] materializedArtifacts =
            Array.Empty<AuditionPvPinnedArtifact>();
        public string[] holds = Array.Empty<string>();
        public string[] missingSupplementalRequirements = Array.Empty<string>();
    }

    internal sealed class AuditionPvSixtySecondApprovalAssemblyResult
    {
        public string composeInputPath = string.Empty, assemblyReceiptPath = string.Empty;
        public bool currentTwelveSecondReady, fullComposeInputCandidate;
        public string[] holds = Array.Empty<string>();
        public string[] missingSupplementalRequirements = Array.Empty<string>();
        public AuditionPvSixtySecondProductionComposeInput composeInput;
    }
}
